// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
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
      :this(true)
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
            EFPApp.ShowException(e1, "Неперехваченное исключение UnhandledException с завершением работы");
          else
            LogoutTools.LogoutException(e1, "Неперехваченное исключение UnhandledException без завершения работы");
        }
      }
      catch (Exception e2)
      {
        MessageBox.Show(e2.Message, "Внутренняя ошибка обработки UnhandledException", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        FreeLibSet.Forms.Diagnostics.DebugTools.ShowException(args.Exception, "Перехват Application.ThreadException"); // Не используем EFPApp.ShowException(), ибо сюда попадать не должно
      }
      catch
      {
      }
    }

    private bool _IconDisposedExceptionFlag = false;

    private bool ProcessIconDisposedException(ObjectDisposedException exOD)
    {
      if (exOD.ObjectName == "Icon" && exOD.Source == "System.Drawing")
      {
        if (_IconDisposedExceptionFlag)
          return true;
        _IconDisposedExceptionFlag = true;

        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(exOD, "Перехват Application.ThreadException. Ошибка доступа к удаленному значку. Исключение регистрируется однократно");

        return true;
      }
      else
        return false;
    }

    private bool _ToolStripSetItemLocationExceptionFlag = false;

    private bool ProcessToolStripSetItemLocationException(NotSupportedException exNS)
    {
      if (exNS.StackTrace==null)
        return false; // на всякий случай

      if (exNS.StackTrace.Contains("System.Windows.Forms.ToolStrip.SetItemLocation"))
      {
        if (_ToolStripSetItemLocationExceptionFlag)
          return true;
        _ToolStripSetItemLocationExceptionFlag = true;
        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(exNS, "Перехват Application.ThreadException. Ошибка позицонирования ToolStrip. Исключение регистрируется однократно");

        return true;
      }
      else
        return false;
    }

    private bool _DataGridViewSetCurrentCellAddressCoreExceptionFlag = false;

    /// <summary>
    /// Проверка исключения "Недопустимая операция: приводит к повторному вызову функции SetCurrentCellAddressCore."
    /// </summary>
    /// <param name="exInvOp">Исключение InvalidOperationException</param>
    /// <returns>true, если это нужное исключение</returns>
    private bool ProcessDataGridViewSetCurrentCellAddressCoreException(InvalidOperationException exInvOp)
    {
      // не регистрируем исключение даже в первый раз
      if (exInvOp.Source != "System.Windows.Forms")
        return false;
      if (!exInvOp.Message.Contains("SetCurrentCellAddressCore"))
        return false;
      if (exInvOp.StackTrace==null)
        return false; // на всякий случай
      if (!exInvOp.StackTrace.Contains("System.Windows.Forms.DataGridView.SetCurrentCellAddressCore"))
        return false;
      if (!exInvOp.StackTrace.Contains("System.Windows.Forms.DataGridView.OnCellMouseDown")) // только от мыши
        return false;

      if (!_DataGridViewSetCurrentCellAddressCoreExceptionFlag)
      {
        _DataGridViewSetCurrentCellAddressCoreExceptionFlag = true;
        if (LogoutKnownBugExceptions)
          LogoutTools.LogoutException(exInvOp, "Перехват Application.ThreadException. " + exInvOp.Message + ". Исключение регистрируется однократно");
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
        : base("Необрабатываемое исключение (событие AppDomain.UnhandledException)",
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
