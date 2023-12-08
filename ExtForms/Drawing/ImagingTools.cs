// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Drawing
{
  /// <summary>
  /// Функции для работы с графикой
  /// </summary>
  public static class ImagingTools
  {
    #region Поиск ближайшего цвета

    /// <summary>
    /// Найти индекс ближайшего цвета в массиве цветов (палитре)
    /// </summary>
    /// <param name="array">Массив цветов, в котором выполлняется поиск (обычно свойство ColorPalette.Entries)</param>
    /// <param name="color">Цвет, который нужно найти</param>
    /// <returns></returns>
    public static int GetNearestColorIndex(Color[] array, Color color)
    {
      int resIndex = -1;
      int minDiff = 1000;

      for (int i = 0; i < array.Length; i++)
      {
        Color thisColor = array[i];

        int thisDiff = 
          Math.Abs(thisColor.R - color.R) + 
          Math.Abs(thisColor.G - color.G) + 
          Math.Abs(thisColor.B - color.B);

        if (thisDiff < minDiff)
        {
          if (thisDiff == 0)
            return i; // точное совпадение

          resIndex = i;
          minDiff = thisDiff;
        }
      }

      return resIndex;
    }

    #endregion

    #region Преобразование формата Bitmap

    /*
     */

    /// <summary>
    /// Преобразование цветного 24-битного изображения в монохромное, 4-цветное
    /// или 256-цветное изображение, использущее палитру
    /// Метод Bitmap.Clone() может принимать аргумент PixelFormat и менять формат 
    /// хранения изображения. К сожалению, это не работает (всегда?) под Windows-XP
    /// </summary>
    /// <param name="source">Исходное изображение с PixelFormat=Format24bppRgb</param>
    /// <param name="resFormat">Требуемый формат (Format1bppIndexed, Format4bppIndexed или Format8bppIndexed)</param>
    /// <returns>Преобразованное изображение</returns>
    public static Bitmap ConvertToIndexed(Bitmap source, PixelFormat resFormat)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (source.PixelFormat != PixelFormat.Format24bppRgb)
        throw new ArgumentException("Неверный формат исходного изображения: " + source.PixelFormat.ToString() + ". Ожидалось: " + PixelFormat.Format24bppRgb.ToString(), "source");

      switch (resFormat)
      {
        case PixelFormat.Format1bppIndexed:
        case PixelFormat.Format4bppIndexed:
        case PixelFormat.Format8bppIndexed:
          break;
        default:
          throw new ArgumentException("Неподдерживаемый выходной формат: " + resFormat.ToString(), "resFormat");
      }

      Dictionary<Color, int> colorIndices = new Dictionary<Color, int>();

      Bitmap res;

      Rectangle rc = new Rectangle(0, 0, source.Width, source.Height);
      BitmapData srcData = source.LockBits(rc, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
      try
      {
        res = new Bitmap(source.Width, source.Height, resFormat);
        try
        {
          ColorPalette palette = res.Palette;

          BitmapData resData = res.LockBits(rc, ImageLockMode.ReadWrite, resFormat);
          try
          {
            byte[] srcA = new byte[srcData.Height * srcData.Stride];
            byte[] resA = new byte[resData.Height * resData.Stride];
            DataTools.FillArray<byte>(resA, 0);
            Marshal.Copy(srcData.Scan0, srcA, 0, srcA.Length);

            for (int i = 0; i < source.Height; i++)
            {
              int srcStartIndex = srcData.Stride * i;
              int resStartIndex = resData.Stride * i;

              for (int j = 0; j < srcData.Width; j++)
              {
                int srcIdx = srcStartIndex + 3 * j;
                Color srcColor = Color.FromArgb(srcA[srcIdx + 2], srcA[srcIdx + 1], srcA[srcIdx + 0]);

                int resColorIndex;
                if (!colorIndices.TryGetValue(srcColor, out resColorIndex))
                {
                  // Находим ближайший цвет
                  resColorIndex = GetNearestColorIndex(palette.Entries, srcColor);
                  // Запоминаем для будущего использования
                  colorIndices.Add(srcColor, resColorIndex);
                }

                switch (resFormat)
                {
                  case PixelFormat.Format1bppIndexed:
                    if (resColorIndex == 1)
                    {
                      int ResIdx = resStartIndex + (j / 8);
                      int Pix = j % 8;
                      int b = resA[ResIdx];
                      int v = 0x80 >> Pix;
                      b = b | v;
                      resA[ResIdx] = (byte)b;
                    }
                    break;

                  case PixelFormat.Format4bppIndexed:
                    if (resColorIndex != 0)
                    {
                      int ResIdx = resStartIndex + (j / 2);
                      int b = resA[ResIdx];
                      int v = resColorIndex;
                      if ((j % 2) == 0)
                        v = v << 4;
                      b = b | v;
                      resA[ResIdx] = (byte)b;
                    }
                    break;

                  case PixelFormat.Format8bppIndexed:
                    resA[resStartIndex + j] = (byte)resColorIndex;
                    break;
                }
              }
            }

            Marshal.Copy(resA, 0, resData.Scan0, resA.Length);
          }
          finally
          {
            res.UnlockBits(resData);
          }
        }
        catch
        {
          res.Dispose();
          throw;
        }
      }
      finally
      {
        source.UnlockBits(srcData);
      }

      return res;
    }

    #endregion

    #region Другие преобразования Bitmap

    /*
    [StructLayout(LayoutKind.Sequential)]
    private struct PixelData
    {
      public byte B;
      public byte G;
      public byte R;
      public byte A;
    }
     * */

    internal static void SetAlphaChannelValue(Bitmap image, byte value)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (image.PixelFormat != PixelFormat.Format32bppArgb)
        throw new ArgumentException("Wrong PixelFormat");

      BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                   ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

      byte[] a = new byte[bitmapData.Height * bitmapData.Stride];
      Marshal.Copy(bitmapData.Scan0, a, 0, a.Length);
      for (int i = 0; i < bitmapData.Height; i++)
      {
        int off = bitmapData.Stride*i;
        for (int j = 0; j < bitmapData.Width; j++)
        {
          a[off + 3] = value;
          off += 4;
        }
      }
      Marshal.Copy(a, 0, bitmapData.Scan0, a.Length);



#if XXXX
			unsafe
			{
				PixelData* pPixel = (PixelData*)bitmapData.Scan0;
				for (int i = 0; i < bitmapData.Height; i++)
				{
					for (int j = 0; j < bitmapData.Width; j++)
					{
						pPixel->A = value;
						pPixel++;
					}
					pPixel += bitmapData.Stride - (bitmapData.Width * 4);
				}
			}
#endif
      image.UnlockBits(bitmapData);
    }

    /// <summary>
    /// Получить данные растра из объекта <see cref="Bitmap"/> в виде массива байт
    /// </summary>
    /// <param name="image">Изображение</param>
    /// <returns>Данные растра</returns>
    internal static byte[] GetImageRasterBytes(Bitmap image)
    {
      BitmapData bmpdata = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
      byte[] bits = new byte[bmpdata.Stride * bmpdata.Height];
      System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, bits, 0, bits.Length);
      image.UnlockBits(bmpdata);
      return bits;
    }


    #endregion

    #region Объекты ImageCodecInfo

    private static readonly Dictionary<Guid, ImageCodecInfo> _FormatDict =
      CreateFormatDict();

    private static Dictionary<Guid, ImageCodecInfo> CreateFormatDict()
    {
      Dictionary<Guid, ImageCodecInfo> dict = new Dictionary<Guid, ImageCodecInfo>();
      ImageCodecInfo[] icis = ImageCodecInfo.GetImageDecoders();
      for (int i = 0; i < icis.Length; i++)
      {
        if (!dict.ContainsKey(icis[i].FormatID))
          dict.Add(icis[i].FormatID, icis[i]);
      }
      return dict;
    }

    /// <summary>
    /// Получить кодек, отвечающий за выбранный формат
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
    {
      ImageCodecInfo ici;
      _FormatDict.TryGetValue(format.Guid, out ici);
      return ici;
    }

    #endregion

    #region Загрузка значков из файла

#if XXXX // Пока не нужно
    // Взято из:
    // http://www.codeproject.com/Articles/29923/Assigning-an-application-s-icon-to-all-forms-in-th
    // Copyright: (C) 2008, Sergey Stoyan

    /// <summary>
    /// Used as a parameter type for ExtractIconFromFile
    /// </summary>
    public enum IconSize : uint
    {
      /// <summary>
      /// 32x32
      /// </summary>
      Large = 0x0,
      /// <summary>
      /// 16x16
      /// </summary>
      Small = 0x1
    }

    /// <summary>
    /// Extracts the specified icon from the file.
    /// </summary>
    /// <param name="lpszFile">path of the icon file</param>
    /// <param name="nIconIndex">index of the icon with the file</param>
    /// <param name="phIconLarge">32x32 icon</param>
    /// <param name="phIconSmall">16x16 icon</param>
    /// <param name="nIcons">number of icons to extract</param>
    /// <returns>number of icons within the file</returns>
    [DllImport("Shell32", CharSet = CharSet.Auto)]
    extern static int ExtractIconEx(
        [MarshalAs(UnmanagedType.LPTStr)] 
            string lpszFile,
        int nIconIndex,
        IntPtr[] phIconLarge,
        IntPtr[] phIconSmall,
        int nIcons
        );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

    /// <summary>
    /// Extracts icon from the binary file like .exe, .dll, etc. (because of .NET ExtractAssociatedIcon does not get 16x16 icons)
    /// </summary>
    /// <param name="file">fiel path from where icon is to be extracted</param>
    /// <param name="size">size of the icon</param>
    /// <returns>extracted icon</returns>
    public static Icon ExtractIconFromLibrary(string file, IconSize size)
    {
      if (ExtractIconEx(file, -1, null, null, 0) < 1)
        return null;

      IntPtr[] icon_ptr = new IntPtr[1];
      if (size == IconSize.Small)
        ExtractIconEx(file, 0, null, icon_ptr, 1);
      else
        ExtractIconEx(file, 0, icon_ptr, null, 1);

      Icon unmanaged_icon = (Icon)Icon.FromHandle(icon_ptr[0]);
      Icon icon = (Icon)unmanaged_icon.Clone();
      DestroyIcon(unmanaged_icon.Handle);

      return icon;
    }
#endif

    #endregion

    #region Маленькие изображения

    /// <summary>
    /// Определяет необходимость уменьшения изображения.
    /// </summary>
    /// <param name="image">Исходное изображение</param>
    /// <param name="maxSize">Ограничение на размер изображения</param>
    /// <param name="newSize">Сюда помещается новый размер изображения, меньший или равный исходному.
    /// Пропорции сохраняются</param>
    /// <returns>true, если изображение должно быть уменьшено</returns>
    public static bool IsImageShrinkNeeded(Image image, Size maxSize, out Size newSize)
    {
      return IsImageShrinkNeeded(image.Size, maxSize, out newSize);
    }

    /// <summary>
    /// Определяет необходимость уменьшить размер изображения, чтобы вписать его в заданную область.
    /// При уменьшении сохраняются существующие пропорции.
    /// Если изображение меньше, чем <paramref name="maxSize"/>, то уменьшение не выполняется.
    /// </summary>
    /// <param name="srcImageSize">Существующий размер изображение</param>
    /// <param name="maxSize">Размер, в который нужно вписать</param>
    /// <param name="newSize">Сюда записываются уменьшенные размеры.
    /// Если уменьшение не требуется, возвращается <paramref name="srcImageSize"/></param>
    /// <returns>true, если требуется уменьшение размеров</returns>
    public static bool IsImageShrinkNeeded(Size srcImageSize, Size maxSize, out Size newSize)
    {
#if DEBUG
      if (srcImageSize.Width < 0 || srcImageSize.Height < 0)
        throw new ArgumentException("Неправильный исходный размер: " + srcImageSize.ToString(), "maxSize");
      if (maxSize.Width < 1 || maxSize.Height < 1)
        throw new ArgumentException("Неправильный максимальный размер: " + maxSize.ToString(), "maxSize");
#endif

      if (srcImageSize.Width > maxSize.Width || srcImageSize.Height > maxSize.Height)
      {
        double s1 = 1.0;
        double s2 = 1.0;
        if (srcImageSize.Width > 0) // 27.12.2020
          s1 = (double)(maxSize.Width) / (double)(srcImageSize.Width);
        if (srcImageSize.Height > 0) // 27.12.2020
          s2 = (double)(maxSize.Height) / (double)(srcImageSize.Height);
        double s = Math.Min(s1, s2);

#if DEBUG
        if (s <= 0.0 || s > 1.0)
          throw new BugException("Неправильный коэффициент масштабирования: " + s.ToString());
#endif

        newSize = new Size((int)(Math.Round(srcImageSize.Width * s)),
        (int)(Math.Round(srcImageSize.Height * s)));
        return true;
      }
      else
      {
        newSize = srcImageSize;
        return false;
      }
    }

    #endregion
  }
}
