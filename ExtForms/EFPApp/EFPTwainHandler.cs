// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Win32.Twain;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация сканирования через интерфейс TWAIN.
  /// Порядок работы:
  /// <list type="number">
  /// <item><description>Создать EFPTwainHandler</description></item>
  /// <item><description>Вызвать Acquire().</description></item>
  /// <item><description>Сохранить полученные изображения (результат Acquire()) на диске или создать копии объктов Bitmap
  /// для дальнейшего использования.</description></item>
  /// <item><description>Вызвать Dispose()</description></item>
  /// </list>
  /// </summary>
  public class EFPTwainHandler : DisposableObject, IMessageFilter
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    /// <summary>
    /// Создает обработчик
    /// </summary>
    public EFPTwainHandler()
    {
      if (!EFPApp.MainWindowVisible)
        throw new InvalidOperationException(Res.EFPTwainHandler_Err_NoMainWindow);

      Application.AddMessageFilter(this);
      _MsgFilterInstalled = true;

      _MainObj = null; // Создадим при первом обращении

    }

    /// <summary>
    /// Удаляет изображения
    /// </summary>
    /// <param name="disposing">true, если был вызван метод <see cref="IDisposable.Dispose()"/>, а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      try
      {
        ClearBitmaps();
      }
      catch
      {
      }

      if (_MsgFilterInstalled)
      {
        Application.RemoveMessageFilter(this);
        _MsgFilterInstalled = false;
      }

      if (disposing)
      {
        if (_MainObj != null)
        {
          try
          {
            _MainObj.Finish();
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, LogoutTools.GetTitleForCall("Twain.Finish()"));
          }
          _MainObj.Dispose();
          _MainObj = null;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    private TwainObject _MainObj;

    #endregion

    #region Сканирование

    private bool _InsideAcquire;

    /// <summary>
    /// <para>Получить изображения.</para>
    /// <para>Возвращает массив из 0 или более изображений.</para>
    /// <para>
    /// Внимание! 
    /// Объект <see cref="EFPTwainHandler"/> является единоличным владельцем своих изображений и удалит
    /// их при вызове <see cref="IDisposable.Dispose()"/> или повторном вызове <see cref="Acquire()"/>.
    /// Если требуется использование изображений, отличное от их немедленного сохранения на диске,
    /// всегда создавайте копии, перед тем как присвоить ссылки на них другим объектам (при условии,
    /// что эти объекты, например, при вызове <see cref="EFPThumbnailPictureButton.SetMainImage(Image)"/> сами не выполняют
    /// создание копии.
    /// </para>
    /// </summary>
    /// <returns>Массив изображений</returns>
    public Bitmap[] Acquire()
    {
      if (_InsideAcquire)
        throw new ReenteranceException();

      ClearBitmaps();
      _Bitmaps = new List<Bitmap>();

      _InsideAcquire = true;
      Splash spl = new Splash(new string[]{
        Res.EFPTwainHandler_Phase_Init,
        Res.EFPTwainHandler_Phase_Start,
        Res.EFPTwainHandler_Phase_Scan});
      try
      {
        // Если инициализировать внутри splash, не будет перекрываться окно сканера
        bool ErrorFlag = false;
        try
        {
          InitMainObj();
        }
        catch (Exception e)
        {
          EFPApp.ErrorMessageBox(e.Message, Res.EFPTwainHandler_ErrTitle_Init);
          ErrorFlag = true;
        }
        if (!ErrorFlag)
        {
          spl.Complete();

          _MainObj.Acquire();
          spl.Complete();

          spl.AllowCancel = true;
          while (_InsideAcquire)
          {
            //Application.DoEvents вызывается в CheckCancelled
            spl.CheckCancelled();
            Thread.Sleep(100);
          }
        }
      }
      finally
      {
        spl.Close();
      }

      return _Bitmaps.ToArray();
    }

    private void InitMainObj()
    {
      CheckNotDisposed();

      if (_MainObj == null)
      {
        _MainObj = new TwainObject();
        _MainObj.Init(EFPApp.MainWindowHandle);
      }
    }

    private List<Bitmap> _Bitmaps;

    private void ClearBitmaps()
    {
      if (_Bitmaps != null)
      {
        for (int i = 0; i < _Bitmaps.Count; i++)
          _Bitmaps[i].Dispose();
      }
      _Bitmaps = null;
    }

    #endregion

    #region IMessageFilter

    private bool _MsgFilterInstalled;

    bool IMessageFilter.PreFilterMessage(ref Message m)
    {
      if (_MainObj == null)
        return false;

      TwainCommand cmd = _MainObj.PassMessage(ref m);
      if (cmd == TwainCommand.Not)
        return false;

      switch (cmd)
      {
        case TwainCommand.CloseRequest:
          _InsideAcquire = false;
          _MainObj.CloseSrc();
          break;
        case TwainCommand.CloseOk:
          _InsideAcquire = false;
          _MainObj.CloseSrc();
          break;
        case TwainCommand.DeviceEvent:
          break;
        case TwainCommand.TransferReady:
          List<IntPtr> pics = _MainObj.TransferPictures();
          _InsideAcquire = false;
          _MainObj.CloseSrc();

          for (int i = 0; i < pics.Count; i++)
          {
            IntPtr bmpptr = TwainObject.GlobalLock(pics[i]);
            Rectangle bmprect;
            IntPtr pixptr = GetPixelInfo(bmpptr, out bmprect);

            Bitmap bmp = new Bitmap(bmprect.Width, bmprect.Height);
            Graphics gr = Graphics.FromImage(bmp);
            try
            {
              IntPtr hdc = gr.GetHdc();
              try
              {
                SetDIBitsToDevice(hdc, 0, 0, bmprect.Width, bmprect.Height,
                  0, 0, 0/*bmprect.Width*/, bmprect.Height, pixptr, bmpptr, 0);
              }
              finally
              {
                gr.ReleaseHdc();
              }
            }
            finally
            {
              gr.Dispose();
            }

            //            bmpptr.
            //Bitmap bmp = Bitmap.FromHbitmap(pics[i]);
            _Bitmaps.Add(bmp);
          }
          break;
        case TwainCommand.Null:
          return false;
      }

      return true;
    }

    [DllImport("gdi32.dll", ExactSpelling = true)]
    internal static extern int SetDIBitsToDevice(IntPtr hdc, int xdst, int ydst,
       int width, int height, int xsrc, int ysrc, int start, int lines,
       IntPtr bitsptr, IntPtr bmiptr, int color);



    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class BITMAPINFOHEADER
    {
      public int biSize;
      public int biWidth;
      public int biHeight;
      public short biPlanes;
      public short biBitCount;
      public int biCompression;
      public int biSizeImage;
      public int biXPelsPerMeter;
      public int biYPelsPerMeter;
      public int biClrUsed;
      public int biClrImportant;
    }

    private static IntPtr GetPixelInfo(IntPtr bmpptr, out Rectangle bmprect)
    {
      BITMAPINFOHEADER bmi = new BITMAPINFOHEADER();
      Marshal.PtrToStructure(bmpptr, bmi);


      bmprect = new Rectangle();
      bmprect.X = bmprect.Y = 0;
      bmprect.Width = bmi.biWidth;
      bmprect.Height = bmi.biHeight;

      if (bmi.biSizeImage == 0)
        bmi.biSizeImage = ((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight;

      int p = bmi.biClrUsed;
      if ((p == 0) && (bmi.biBitCount <= 8))
        p = 1 << bmi.biBitCount;
      p = (p * 4) + bmi.biSize + (int)bmpptr;
      return (IntPtr)p;
    }

    #endregion
  }
}
