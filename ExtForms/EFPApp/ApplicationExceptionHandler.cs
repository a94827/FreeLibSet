// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Обработчик событий <see cref="AppDomain.UnhandledException"/> и <see cref="Application.ThreadException"/>.
  /// Обработчик должен создаваться в using-блоке, внутри которого расположен <see cref="Application.Run()"/>.
  /// Это гарантирует отцепление обработчиков при завершении программы и должно
  /// предотвратить утечку ресурсов.
  /// </summary>
  public class ApplicationExceptionHandler : IDisposable
  {
    #region Конструктор и Dispose

    /*
     * Не используется DisposableObject, т.к. не нужно показывать этот объект 
     * в списке неудаленных объектов при отладке. У ApplicationExceptionHandler 
     * больший период жизни
     */

    /// <summary>
    /// Обычная версия конструктора
    /// </summary>
    public ApplicationExceptionHandler()
      : this(true)
    {
    }

    /// <summary>
    /// Эта версия позволяет подавлять логгинг известных ошибок
    /// </summary>
    /// <param name="logoutKnownBugExceptions">Если true, то известные исключения перехватываются и однократно регистрируются в log-файле.
    /// Если false, то исключения не будут записываться в log-файл</param>
    public ApplicationExceptionHandler(bool logoutKnownBugExceptions)
    {
      _IsDisposed = false;
      _LogoutKnownBugExceptions = logoutKnownBugExceptions;

      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      Interlocked.Increment(ref _HandlerCount);
    }

    /// <summary>
    /// Отключает обработчики событий
    /// </summary>
    ~ApplicationExceptionHandler()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    /// <summary>
    /// Реализация IDisposable
    /// </summary>
    public void Dispose()
    {
      if (!IsDisposed)
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }
    }

    /// <summary>
    /// Отключает обработчики событий
    /// </summary>
    /// <param name="Dispose">true, если был вызван метод <see cref="IDisposable.Dispose()"/></param>
    protected virtual void Dispose(bool Dispose)
    {
      AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
      Application.ThreadException -= new ThreadExceptionEventHandler(Application_ThreadException);
      Interlocked.Decrement(ref _HandlerCount);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// true, если метод <see cref="IDisposable.Dispose()"/> уже вызывался
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private readonly bool _IsDisposed;

    /// <summary>
    /// Если true, то известные исключения перехватываются и однократно регистрируются в log-файле.
    /// Если false, то исключения не будут записываться в log-файл.
    /// Задается в конструкторе
    /// </summary>
    public bool LogoutKnownBugExceptions { get { return _LogoutKnownBugExceptions; } }
    private readonly bool _LogoutKnownBugExceptions;

    #endregion

    #region Обработчики событий

    /// <summary>
    /// Обработчик события <see cref="AppDomain.UnhandledException"/>
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Аргументы события</param>
    protected virtual void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
      // Выбрасываем свой тип исключения и ловим его в log-файл
      try
      {
        try
        {
          throw new AppDomainUnhandledException(args);
        }
        catch (Exception e1)
        {
          // Если исключение не завершает программу, то ничего не выводим на экран, 
          // иначе - выводим
          if (args.IsTerminating)
            EFPApp.ShowException(e1, Res.ApplicationExceptionHandler_ErrTitle_UnhandledWithTermination);
          else
            LogoutTools.LogoutException(e1, Res.ApplicationExceptionHandler_ErrTitle_UnhandledNoTermination);
        }
      }
      catch (Exception e2)
      {
        //MessageBox.Show(e2.Message, "Внутренняя ошибка обработки UnhandledException", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // 28.11.2024
        string msg = e2.GetType().ToString() + Environment.NewLine +
          e2.Message + Environment.NewLine;
        try
        {
          System.Diagnostics.Process prc = System.Diagnostics.Process.GetCurrentProcess();
          msg += "Process ID=" + prc.Id + ", Name=" + prc.ProcessName + Environment.NewLine;
        }
        catch { }
        try
        {
          msg += "IsTerminating=" + args.IsTerminating.ToString();
        }
        catch { }
        MessageBox.Show(msg, Res.ApplicationExceptionHandler_ErrTitle_UnhandledInternalError, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// Обработчик события <see cref="Application.ThreadException"/>
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Аргументы события</param>
    protected virtual void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs args)
    {
      try
      {
        // 20.10.2016
        // В MDI-интерфейсе регулярно появляется исключение System.ObjectDisposedException:
        //  Доступ к ликвидированному объекту невозможен. Имя объекта: "Icon".
        // Stack trace:
        // в System.Drawing.Icon.get_Handle()
        // в System.Drawing.Icon.get_Size()
        // в System.Drawing.Icon.ToBitmap()
        // в System.Windows.Forms.MdiControlStrip.GetTargetWindowIcon()
        // в System.Windows.Forms.MdiControlStrip..ctor(IWin32Window target)
        // в System.Windows.Forms.Form.UpdateMdiControlStrip(Boolean maximized)
        // в System.Windows.Forms.Form.UpdateToolStrip()
        // в System.Windows.Forms.Form.OnMdiChildActivate(EventArgs e)
        // в System.Windows.Forms.Form.ActivateMdiChildInternal(Form form)
        // в System.Windows.Forms.Form.WmMdiActivate(Message& m)
        // в System.Windows.Forms.Form.WndProc(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
        // в System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
        //
        // Понятия не имею, как его предотвращать
        ObjectDisposedException exOD = args.Exception as ObjectDisposedException;
        if (exOD != null)
        {
          if (ProcessIconDisposedException(exOD))
            return;
        }


        // 02.06.2017
        // Иногда у пользователей появляется ошибка (сам не разу не видел):
        // ToolStrip не может определить положение других элементов ToolStrip.
        // Stack trace:
        // в System.Windows.Forms.ToolStrip.SetItemLocation(ToolStripItem item, Point location)
        // в System.Windows.Forms.StatusStrip.SetDisplayedItems()
        // в System.Windows.Forms.ToolStrip.OnLayout(LayoutEventArgs e)
        // в System.Windows.Forms.StatusStrip.OnLayout(LayoutEventArgs levent)
        // в System.Windows.Forms.Control.PerformLayout(LayoutEventArgs args)
        // в System.Windows.Forms.Control.System.Windows.Forms.Layout.IArrangedElement.PerformLayout(IArrangedElement affectedElement, String affectedProperty)
        // в System.Windows.Forms.Layout.LayoutTransaction.DoLayout(IArrangedElement elementToLayout, IArrangedElement elementCausingLayout, String property)
        // в System.Windows.Forms.Control.OnResize(EventArgs e)
        // в System.Windows.Forms.Control.OnSizeChanged(EventArgs e)
        // в System.Windows.Forms.Control.UpdateBounds(Int32 x, Int32 y, Int32 width, Int32 height, Int32 clientWidth, Int32 clientHeight)
        // в System.Windows.Forms.Control.UpdateBounds()
        // в System.Windows.Forms.Control.WndProc(Message& m)
        // в System.Windows.Forms.ScrollableControl.WndProc(Message& m)
        // в System.Windows.Forms.ToolStrip.WndProc(Message& m)
        // в System.Windows.Forms.StatusStrip.WndProc(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
        // в System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
        NotSupportedException exNS = args.Exception as NotSupportedException;
        if (exNS != null)
        {
          if (ProcessToolStripSetItemLocationException(exNS))
            return;
        }

        // 27.11.2018
        // Иногда у пользователей появляется ошибка рекурсивной установки текущей ячейки DataGridView 
        // при щелчке мышью
        // в System.Windows.Forms.DataGridView.SetCurrentCellAddressCore(Int32 columnIndex, Int32 rowIndex, Boolean setAnchorCellAddress, Boolean validateCurrentCell, Boolean throughMouseClick)
        // в System.Windows.Forms.DataGridView.OnCellMouseDown(HitTestInfo hti, Boolean isShiftDown, Boolean isControlDown)
        // в System.Windows.Forms.DataGridView.OnCellMouseDown(DataGridViewCellMouseEventArgs e)
        // в System.Windows.Forms.DataGridView.OnMouseDown(MouseEventArgs e)
        // в System.Windows.Forms.Control.WmMouseDown(Message& m, MouseButtons button, Int32 clicks)
        // в System.Windows.Forms.Control.WndProc(Message& m)
        // в System.Windows.Forms.DataGridView.WndProc(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
        // в System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
        // в System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
        InvalidOperationException exInvOp = args.Exception as InvalidOperationException;
        if (exInvOp != null)
        {
          if (ProcessDataGridViewSetCurrentCellAddressCoreException(exInvOp))
            return;
        }

        ArgumentException exArg = args.Exception as ArgumentException;
        if (exArg != null)
        {
          // 13.11.2024
          // В Wine+Mono может возникать ошибка при перемещении мышью над кнопкой панели инструментов при показе всплывающей подсказки
          // at System.Drawing.GDIPlus.CheckStatus(System.Drawing.Status status)[0x00098] in < b58d88bd041145279c2003cf306f84d5 >:0
          // at System.Drawing.Bitmap.FromHicon(System.IntPtr hicon)[0x00008] in < b58d88bd041145279c2003cf306f84d5 >:0
          // at System.Drawing.Icon..ctor(System.IntPtr handle)[0x00018] in < b58d88bd041145279c2003cf306f84d5 >:0
          // at(wrapper remoting - invoke - with - check) System.Drawing.Icon..ctor(intptr)
          // at System.Drawing.Icon.FromHandle(System.IntPtr handle)[0x00018] in < b58d88bd041145279c2003cf306f84d5 >:0
          // at System.Windows.Forms.Cursor.get_HotSpot()[0x00015] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.ToolStrip.UpdateToolTip(System.Windows.Forms.ToolStripItem item)[0x0009b] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at(wrapper remoting - invoke - with - check) System.Windows.Forms.ToolStrip.UpdateToolTip(System.Windows.Forms.ToolStripItem)
          // at System.Windows.Forms.ToolStripItem.OnMouseHover(System.EventArgs e)[0x00025] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.ToolStripItem.HandleMouseHover(System.EventArgs e)[0x00001] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.ToolStripItem.FireEventInteractive(System.EventArgs e, System.Windows.Forms.ToolStripItemEventType met)[0x00049] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.ToolStripItem.FireEvent(System.EventArgs e, System.Windows.Forms.ToolStripItemEventType met)[0x00086] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at(wrapper remoting - invoke - with - check) System.Windows.Forms.ToolStripItem.FireEvent(System.EventArgs, System.Windows.Forms.ToolStripItemEventType)
          // at System.Windows.Forms.MouseHoverTimer.OnTick(System.Object sender, System.EventArgs e)[0x0002c] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.Timer.OnTick(System.EventArgs e)[0x0000a] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.Timer + TimerNativeWindow.WndProc(System.Windows.Forms.Message & m)[0x0002c] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          // at System.Windows.Forms.NativeWindow.Callback(System.Windows.Forms.Message & m)[0x00025] in < ef3dd1b1af11490e89408e0d9c28d1f0 >:0
          if (ProcessArgumentExceptionInDrawingToolTipIcon(exArg))
            return;
        }

        FreeLibSet.Forms.Diagnostics.DebugTools.ShowException(args.Exception, "Перехват Application.ThreadException"); // Не используем EFPApp.ShowException(), ибо сюда попадать не должно
      }
      catch
      {
      }
    }

    private bool ProcessArgumentExceptionInDrawingToolTipIcon(ArgumentException ex)
    {
      if (!EnvironmentTools.IsWine)
        return false;
      if (!EnvironmentTools.IsMono)
        return false;
      if (ex.StackTrace == null)
        return false; // на всякий случай
      if (ex.Source != "System.Drawing")
        return false;
      if (!ex.StackTrace.Contains("System.Drawing.Bitmap.FromHicon"))
        return false;
      if (!ex.StackTrace.Contains("System.Windows.Forms.ToolStripItem.")) // там могут вызываться разные методы
        return false;

      return true;
    }

    private bool _IconDisposedExceptionFlag = false;

    private bool ProcessIconDisposedException(ObjectDisposedException ex)
    {
      if (ex.ObjectName == "Icon" && ex.Source == "System.Drawing")
      {
        if (_IconDisposedExceptionFlag)
          return true;
        _IconDisposedExceptionFlag = true;

        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(ex, Res.ApplicationExceptionHandler_ErrTitle_IconDisposed);

        return true;
      }
      else
        return false;
    }

    private bool _ToolStripSetItemLocationExceptionFlag = false;

    private bool ProcessToolStripSetItemLocationException(NotSupportedException ex)
    {
      if (ex.StackTrace == null)
        return false; // на всякий случай

      if (ex.StackTrace.Contains("System.Windows.Forms.ToolStrip.SetItemLocation"))
      {
        if (_ToolStripSetItemLocationExceptionFlag)
          return true;
        _ToolStripSetItemLocationExceptionFlag = true;
        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(ex, Res.ApplicationExceptionHandler_ErrTitle_ToolStripSetItemLocation);

        return true;
      }
      else
        return false;
    }

    private bool _DataGridViewSetCurrentCellAddressCoreExceptionFlag = false;

    /// <summary>
    /// Проверка исключения "Недопустимая операция: приводит к повторному вызову функции SetCurrentCellAddressCore."
    /// </summary>
    /// <param name="ex">Исключение InvalidOperationException</param>
    /// <returns>true, если это нужное исключение</returns>
    private bool ProcessDataGridViewSetCurrentCellAddressCoreException(InvalidOperationException ex)
    {
      // не регистрируем исключение даже в первый раз
      if (ex.Source != "System.Windows.Forms")
        return false;
      if (!ex.Message.Contains("SetCurrentCellAddressCore"))
        return false;
      if (ex.StackTrace == null)
        return false; // на всякий случай
      if (!ex.StackTrace.Contains("System.Windows.Forms.DataGridView.SetCurrentCellAddressCore"))
        return false;
      if (!ex.StackTrace.Contains("System.Windows.Forms.DataGridView.OnCellMouseDown")) // только от мыши
        return false;

      if (!_DataGridViewSetCurrentCellAddressCoreExceptionFlag)
      {
        _DataGridViewSetCurrentCellAddressCoreExceptionFlag = true;
        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(ex, Res.ApplicationExceptionHandler_ErrTitle_DataGridViewSetCurrentCellAddressCore);
      }

      return true;
    }

    #endregion

    /// <summary>
    /// Отдельный класс исключения, чтобы его было видно в log-файле
    /// </summary>
    [Serializable]
    public class AppDomainUnhandledException : Exception
    {
      #region Конструктор

      /// <summary>
      /// Создает объект исключения
      /// </summary>
      /// <param name="args">Аргументы события <see cref="AppDomain.UnhandledException"/></param>
      public AppDomainUnhandledException(UnhandledExceptionEventArgs args)
        : base(Res.Common_Err_AppDomainUnhandledException,
        args.ExceptionObject is Exception ? (Exception)(args.ExceptionObject) : null)
      {
        _ExceptionObject = args.ExceptionObject;
        _IsTerminating = args.IsTerminating;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Возвращает значение свойства <see cref="UnhandledExceptionEventArgs.ExceptionObject"/>
      /// </summary>
      public object ExceptionObject { get { return _ExceptionObject; } }
      private readonly object _ExceptionObject;

      /// <summary>
      /// Возвращает значение свойства <see cref="UnhandledExceptionEventArgs.IsTerminating"/>
      /// </summary>
      public bool IsTerminating { get { return _IsTerminating; } }
      private readonly bool _IsTerminating;

      #endregion
    }

    #region Статическое свойство

    /// <summary>
    /// Количество созданных объектов <see cref="ApplicationExceptionHandler"/> (для отладки)
    /// </summary>
    public static int HandlerCount { get { return _HandlerCount; } }
    private static int _HandlerCount = 0;

    #endregion
  }
}
