// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

//#define ACTIVEDIALOG_OLD // Если определено, то свойство ActiveDialog определяется по старым правилам до 20.08.2020: до вызова Form.ShowDialog(), а не в обработчике Form.VisibleChanged
//#define TRACE_DIALOGOWNERWINDOW // Если определено, то выполняется трассировка установки свойства EFPApp.DialogOwnerWindow

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Diagnostics;
using FreeLibSet.IO;
using System.Xml;
using FreeLibSet.Logging;
using FreeLibSet.Remoting;
using System.Runtime.InteropServices;
using FreeLibSet.Config;
using System.Data;
using System.Reflection;
using FreeLibSet.Core;
using FreeLibSet.Shell;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Controls;
using FreeLibSet.Drawing;

namespace FreeLibSet.Forms
{
  /*
   * Терминология для окон
   * 
   * MainWindow - главное окно программы
   * Dialog     - модальный блок диалога (может быть и окно без кнопок)
   * ChildForm  - дочернее окно в интерфейсе MDI, между которыми можно переключаться Ctrl+Tab
   *              (если будет реализован интерфейс без MDI, то такие окна будут на рабочем столе)
   * ToolWindow - маленькое окно с узким заголовком, например, калькулятор
   * ToolBar    - панель с кнопками, дублирующими команды меню
   */

  #region Делегаты

  /// <summary>
  /// Аргументы события EFPApp.ShowHelpNeeded
  /// </summary>
  public class EFPHelpContextEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="helpContext">Контекст справки в терминах EFPControlBase.HelpContext</param>
    public EFPHelpContextEventArgs(string helpContext)
    {
      _HelpContext = helpContext;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Контекст справки в терминах EFPControlBase.HelpContext
    /// </summary>
    public string HelpContext { get { return _HelpContext; } }
    private readonly string _HelpContext;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPApp.ShowHelpNeeded
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPHelpContextEventHandler(object sender,
    EFPHelpContextEventArgs args);


  /// <summary>
  /// Аргументы событий EFPApp.ExceptionShowing и Wizard.HandleException
  /// </summary>
  public class EFPAppExceptionEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="exception">Перехваченное исключение</param>
    /// <param name="title">Предполагаемый текст заголовка при выдаче сообщения об ошибке</param>
    public EFPAppExceptionEventArgs(Exception exception, string title)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");
      _Exception = exception;

      if (String.IsNullOrEmpty(title))
        _Title = "Ошибка";
      else
        _Title = title;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Перехваченное исключение
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    /// <summary>
    /// Предполагаемый текст заголовка при выдаче сообщения об ошибке
    /// </summary>
    public string Title { get { return _Title; } }
    private readonly string _Title;

    /// <summary>
    /// Было ли исключение обработано.
    /// Обработчик события должен сначала проверить это свойство и не выполнять никаких действий, если оно установлено.
    /// Если обработчик вывел пользователю сообщение об ошибке, то следует установить свойство.
    /// В противном случае, сообшение будет выведено еще раз
    /// </summary>
    public bool Handled { get { return _Handled; } set { _Handled = value; } }
    private bool _Handled;

    #endregion
  }

  /// <summary>
  /// Делегат событий EFPApp.ExceptionShowing и Wizard.HandleException
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPAppExceptionEventHandler(object sender, EFPAppExceptionEventArgs args);

  #endregion

  /// <summary>
  /// Объект, который позволяет выполнять действия с главным
  /// окном программы, не обращаясь к нему непосредственно
  /// Только статические методы. Экземпляры объекта не создаются
  /// </summary>
  public static class EFPApp
  {
    #region Инициализация

    /// <summary>
    /// Этот метод должен вызываться до всех других методов работы
    /// </summary>
    public static void InitApp()
    {
#if DEBUG
      if (_MainThread != null)
        throw new BugException("Повторный вызова InitApp()");
#endif
      _MainThread = Thread.CurrentThread;

      // Загружаем общий список изображений
      InitImages();

      _AppStartTime = DateTime.Now;
      _AppStartStopwatchTimestamp = Stopwatch.GetTimestamp(); // 17.02.2021
      Timers.InitTimer();

      DebugTools.DummyInit(); // 23.10.2017
      LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(DebugTools.LogoutTools_LogoutInfoNeeded);

      try
      {
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged); // 17.10.2018
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Не удалось установить обработчик системных событий");
      }

      // SplashTools.PushThreadSplashStack(new SplashStack()); // 19.10.2020. Отменено 24.10.2020
    }

    /// <summary>
    /// Возвращает true, если инициализация приложения (InitApp()) была выполнена
    /// </summary>
    public static bool AppWasInit { get { return _MainImages != null; } }

    /// <summary>
    /// Проверяет, что текущий поток является тем, в котором был вызван метод EFPApp.InitApp().
    /// При вызове из другого потока вызывается исключение.
    /// Если не было вызова EFPApp.InitApp(), также выбрасывается исключение.
    /// </summary>
    public static void CheckMainThread()
    {
      if (_MainThread == null)
        throw new InvalidOperationException("Не было вызова EFPApp.InitApp()");
      if (Thread.CurrentThread != _MainThread)
        throw new DifferentThreadException("Вызов не из основного потока приложения");
    }


    /// <summary>
    /// Возвращает поток, из которого был вызван InitApp
    /// </summary>
    public static Thread MainThread { get { return _MainThread; } }
    private static Thread _MainThread;

    /// <summary>
    /// Возвращает true, если обращение к свойству выполняется из потока, 
    /// в котором был вызван InitApp()
    /// Если InitApp() еще не вызывался, свойство возвращает false
    /// </summary>
    public static bool IsMainThread
    {
      get
      {
        if (_MainThread == null)
          return false;
        return Thread.CurrentThread == _MainThread;
      }
    }

    /// <summary>
    /// Время, когда было вызван метод InitApp()
    /// </summary>
    public static DateTime AppStartTime { get { return _AppStartTime; } }
    private static DateTime _AppStartTime;

    /// <summary>
    /// Время, когда был вызван метод InitApp() в виде количества тиков Stopwatch.GetTimestamp()
    /// </summary>
    public static long AppStartStopwatchTimestamp { get { return _AppStartStopwatchTimestamp; } }
    private static long _AppStartStopwatchTimestamp;

    /// <summary>
    /// Время, прошедшее с вызова InitApp().
    /// Если не было вызова InitApp(), возвращает TimeSpan.Zero
    /// </summary>
    public static TimeSpan AppWorkTime
    {
      get
      {
        if (AppWasInit)
        {
          long delta = Stopwatch.GetTimestamp();
          return TimeSpan.FromSeconds((double)delta / (double)Stopwatch.Frequency);
        }
        else
          return TimeSpan.Zero;
      }
    }


    static void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs args)
    {
      switch (args.Category)
      {
        case Microsoft.Win32.UserPreferenceCategory.Color:
        case Microsoft.Win32.UserPreferenceCategory.Window:
        case Microsoft.Win32.UserPreferenceCategory.VisualStyle:
          EFPApp.ResetColors();
          break;
        case Microsoft.Win32.UserPreferenceCategory.General: // для принтера нет отдельной категории
          EFPApp.Printers.Reset();

          EFPApp.ResetColors(); // если менять цвет фона в Windows-7 для темы Aero, то вызывается эта категория
          break;
      }
    }

    #endregion

    #region Приложение в-целом


    /// <summary>
    /// Это событие вызывается при завершении приложения перед тем, как послать открытым окнам
    /// запрос на закрытие.
    /// Этот обработчик может сохранить расположение окон рабочего стола
    /// </summary>
    public static event CancelEventHandler BeforeClosing;

    /// <summary>
    /// Событие вызывается при закрытии главного окна приложения и при вызове метода Exit().
    /// Если обработчик установит свойство CancelEventArgs.Cancel=true, приложение продолжит работу.
    /// Это событие вызывается после того, как все дочерние окна будут закрыты.
    /// Используйте событие BeforeClosing для сохранения рабочего стола
    /// </summary>
    public static event CancelEventHandler Closing;

    /// <summary>
    /// Свойство возвращает true, если в данный момент выполняется попытка завершить приложение
    /// </summary>
    public static bool IsClosing { get { return _IsClosing; } }
    private static bool _IsClosing = false;

    internal static void OnClosing(CancelEventArgs args/*, bool TestForms*/)
    {
#if DEBUG
      CheckMainThread();
#endif

      if (_IsClosing)
        return;

      if (args.Cancel)
        return;
      try
      {
        _IsClosing = true;
        try
        {
          OnClosing2(args/*, TestForms*/);
        }
        finally
        {
          _IsClosing = false;
        }
      }
      catch (Exception e) // 11.01.2019
      {
        args.Cancel = true;
        EFPApp.ShowException(e, "Ошибка вызова EFPApp.OnClosing2");
      }
    }

    private static void OnClosing2(CancelEventArgs args/*, bool TestForms*/)
    {
      // Вызываем событие
      if (BeforeClosing != null)
      {
        BeforeClosing(null, args);
        if (args.Cancel)
          return;
      }


      // Завершаем печать
      if (!EFPApp.BackgroundPrinting.QueryCancelPrinting())
      {
        args.Cancel = true;
        return;
      }

      // Запоминаем видимость панелей инструментов и меню
      //if (EFPApp.Interface != null && (!IsObsoleteInterfaceMode))
      //  SaveMainWindowLayout();

      // 01.08.2013
      // Закрываем дочерние формы
      if (/*TestForms && */EFPApp.Interface != null)
      {
        /*
        Form[] Forms = EFPApp.Interface.GetChildForms(false);
        for (int i = 0; i < Forms.Length; i++)
        {
          Forms[i].Close();
          if (Forms[i].Visible)
          {
            Args.Cancel = true;
            return;
          }
        }
         * */
        EFPAppMainWindowLayout[] layouts = EFPApp.Interface.GetMainWindowLayouts(false);
        for (int i = 0; i < layouts.Length; i++)
        {
          if (!layouts[i].CloseMainWindow())
          {
            args.Cancel = true;
            return;
          }
        }
      }

      #region Закрытие немодальных форм

      // 13.04.2018
      List<Form> forms2 = new List<Form>();
      foreach (Form frm in Application.OpenForms)
      {
        if (object.ReferenceEquals(frm, MainWindow))
          continue;
        if (frm.Modal)
          continue;
        if (frm.IsMdiChild)
          continue;
        if (!frm.Visible)
          continue;
        forms2.Add(frm);
      }
      for (int i = 0; i < forms2.Count; i++)
      {
        forms2[i].Activate();
        forms2[i].Close();
        if (forms2[i].Visible)
        {
          args.Cancel = true;
          return;
        }
      }

      #endregion

      // Вызываем событие
      if (Closing != null)
        Closing(null, args);

      if (!args.Cancel)
      {
        if (_Fonts != null)
        {
          _Fonts.Dispose();
          _Fonts = null;
        }
      }
    }

    /// <summary>
    /// Событие вызывается при закрытии главного окна приложения и при вызове метода Exit().
    /// Событие вызывается только один раз
    /// </summary>
    public static event EventHandler Closed;

    /// <summary>
    /// Предотвращает повторный вызов события Closed
    /// </summary>
    private static bool _ClosedCalled = false;

    internal static void OnClosed()
    {
#if DEBUG
      CheckMainThread();
#endif

      if (_ClosedCalled)
        return;

      _ClosedCalled = true; // устанавливаем до вызова пользовательского обработчика

      try
      {
        if (Closed != null)
          Closed(null, EventArgs.Empty);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Вызов обработчика события EFPApp.Closed");
      }

      Microsoft.Win32.SystemEvents.UserPreferenceChanged -= new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

      if (_TrayIcon != null)
      {
        _TrayIcon.Dispose();
        _TrayIcon = null;
      }

      ((IDisposable)_FileExtAssociations).Dispose(); // ссылку на объект не удаляем
    }


    /// <summary>
    /// Вызывает событие Closing, затем Application.Exit
    /// Возвращает true, если Application.Exit() закрыло приложение
    /// </summary>
    /// <returns>true, если удалось закрыть все дочерние формы и обарботчик события Closing не установил значение Camcel=true</returns>
    public static bool Exit()
    {
#if DEBUG
      CheckMainThread();
#endif

      _ExitCount++;

      CancelEventArgs args = new CancelEventArgs();
      OnClosing(args/*, true*/);
      if (args.Cancel)
        return false;

      _IsClosing = true;
      try
      {
        Application.Exit(args);
      }
      finally
      {
        _IsClosing = false;
      }

      if (!args.Cancel)
      {
        if (StatusBar != null)
          StatusBar.StatusStripControl = null;
        Timers.Dispose();
        IdleHandlers.Dispose();
        _Exited = true;

        // Закрываем панели инструментов
        while (EFPApp.ToolWindows.Count > 0)
          EFPApp.ToolWindows[0].Dispose();


        OnClosed();
      }

      return !args.Cancel;
    }

    /// <summary>
    /// Количество вызовов метода Exit (включая отмененные обработчиками)
    /// </summary>
    public static int ExitCount { get { return _ExitCount; } }
    private static int _ExitCount = 0;

    /// <summary>
    /// Возвращает true, если был вызов метода Exit(), который не был отменен обработчиком
    /// </summary>
    public static bool Exited { get { return _Exited; } }
    private static bool _Exited;

    /// <summary>
    /// Завершение приложения и перезапуск (если завершение выполнено)
    /// </summary>
    public static void ExitAndRestart()
    {
      if (!Exit())
        return;

      Application.DoEvents();
      Application.Restart();
    }

    /// <summary>
    /// Завершение работы приложения.
    /// В отличие от Exit, может вызываться из любого потока.
    /// Нельзя проверить, выполнилось ли действительно завершение
    /// </summary>
    public static void ExitAsync()
    {
      ExitHandler hand = new ExitHandler();
      hand.Restart = false;
      Timers.Add(hand);
    }

    /// <summary>
    /// Завершение работы приложения. 
    /// После завершения работы приложение перезапускается.
    /// В отличие от Exit, может вызываться из любого потока.
    /// Нельзя проверить, выполнилось ли действительно завершение
    /// </summary>
    public static void ExitAsyncAndRestart()
    {
      ExitHandler hand = new ExitHandler();
      hand.Restart = true;
      Timers.Add(hand);
    }

    private class ExitHandler : IEFPAppTimeHandler
    {
      #region Свойства

      public bool Restart;

      #endregion

      #region IEFPAppTimeHandler Members

      public void TimerTick()
      {
        // Удаляем таймер до выполнения действий, чтобы не было повторного
        // вызова в случае медленной работы
        Timers.Remove(this);

        if (Restart)
          EFPApp.ExitAndRestart();
        else
          EFPApp.Exit();
      }

      #endregion
    }

    /// <summary>
    /// Удаленное управление завершением работы приложения.
    /// Используется в системах клиент-сервер, когда сервер должен уметь завершать работу приложения в отсутствие пользователя.
    /// Приложение периодически опрашивает сервер и, если получен сигнал на завершение работы, вызывает EFPApp.RemoteExitHandler.Start().
    /// При этом на некоторое время выводится заставка. Если в течение этого времени пользователь не отменит завершение, вызывается EFPApp.Exit.
    /// Обращение к свойству может выполняться только из основного потока приложения
    /// </summary>
    public static EFPAppRemoteExitHandler RemoteExitHandler
    {
      get
      {
        if (IsMainThread)
        {
          if (_RemoteExitHandler == null)
            _RemoteExitHandler = new EFPAppRemoteExitHandler();
          return _RemoteExitHandler;
        }
        else
          return null;
      }
    }
    private static EFPAppRemoteExitHandler _RemoteExitHandler;

    /// <summary>
    /// Возвращает первую видимую форму верхнего уровня в приложении
    /// Это может быть и не MainWindow
    /// </summary>
    public static Form FirstVisibleApplicationForm
    {
      get
      {
        /*
#if DEBUG
        CheckTread();
#endif    */

        // 13.05.2016
        // Предпочтительно использовать активную форму
        if (Form.ActiveForm != null)
        {
          foreach (Form frm in Application.OpenForms)
          {
            if (frm == Form.ActiveForm && frm.Visible && frm.IsHandleCreated)
              return frm;
          }
        }

        foreach (Form frm in Application.OpenForms)
        {
          if (frm.Visible && frm.IsHandleCreated)
            return frm;
        }

        return null;
      }
    }

    /// <summary>
    /// Устанавливает для формы свойство ShowInTaskBar=true, если нет ни одной
    /// видимой формы на экране. Иначе свойство устанавливается в false
    /// </summary>
    /// <param name="form">Форма, которую предполагается выводить</param>
    public static void InitShowInTaskBar(Form form)
    {
      form.ShowInTaskbar = (FirstVisibleApplicationForm == null);
      //Form.MinimizeBox = false;
      if (form.MinimizeBox)
        form.MinimizeBox = form.ShowInTaskbar; // 02.03.2016
    }

    /// <summary>
    /// Какое окно использовать как базовое для вывода процентного индикатора
    /// и блоков диалога
    /// Возвращает MainWindow, если оно задано.
    /// Иначе возвращается первое открытое окно.
    /// Если установлено свойство ExternalDialogOwnerWindow, то оно возвращается.
    /// Это свойство является потокобезопасным и возвращает null, если вызвано не из основного потока приложения
    /// </summary>
    public static IWin32Window DialogOwnerWindow
    {
      get
      {
        IWin32Window res = GetDialogOwnerWindow2();
        try
        {
          if (res != null)
          {
            if (res.Handle == IntPtr.Zero)
              return null; // 10.01.2019

            Control ctrl = res as Control;
            if (ctrl != null)
            {
              if (ctrl.IsDisposed)
                return null; // 10.01.2019
            }
          }
          return res;
        }
        catch // 10.01.2019
        {
          return null;
        }
      }
    }

    private static IWin32Window GetDialogOwnerWindow2()
    {
      if (IsMainThread)
      {
        if (_ExternalDialogOwnerWindowSetFlag && _ExternalDialogOwnerWindow != null)
          return ValidateWindow(_ExternalDialogOwnerWindow);
        else
        {
          if (MainWindow == null)
          {

            // 20.12.2016
            // Если к свойству DefaultScreen обращаются в процессе
            // обработки события Form.Load (для центрирования формы),
            // то экран по умолчанию может быть определен прямо из 
            // этой же формы, т.к. она уже Visible и имеет созданный Handle
            // Поэтому, использовать FirstVisibleApplicationForm не следует
            // Используем урезанный вариант, который разрешает использовать только
            // активную форму в качестве владельца

            //return FirstVisibleApplicationForm;

            // 21.12.2018
            // Свойство Form.ActiveForm может меняться асинхронно.
            // Фиксируем ссылку на форму для выполнения проверок с ней.
            Form af = Form.ActiveForm;
            if (af != null && (!af.InvokeRequired))
            {
              if (af.Visible)
                return af;
            }
            return null;
          }
          else
          {
            if (ActiveDialog != null)
              return ActiveDialog; // 14.03.2017
            return MainWindow;
          }
        }
      }
      else
        return null;
    }

    /// <summary>
    /// Вызывает Form.Show(DialogOwnerWindow).
    /// Метод может вызываться асинхронно.
    /// </summary>
    /// <param name="form">Форма, для которой вызывается метод Show()</param>
    internal static void ShowFormInternal(Form form)
    {
      IWin32Window dow = DialogOwnerWindow;

      if (dow != null)
      {
        try
        {
          form.Show(dow);
          return;
        }
        catch { }
      }
      form.Show();
    }

    /// <summary>
    /// Внешнее окно, используемое в качестве владельца при выводе блоков диалога.
    /// Например, если приложение активно взаимодействует с Microsoft Excel и требуется вывести блок диалога с помощью метода ShowDialog(), но так,
    /// чтобы он оказался на окном Excel, а не над главным окном приложения, следует временно установить свойство на главное окно Excel, создав объект
    /// NativeWindow.
    /// В большинстве приложений свойство не используется и всегда содержит значение null. Нельзя устанавливать свойство на главное окно приложения (EFPApp.MainWindow)
    /// или другой блок диалога.
    /// Если свойство установлено, оно возвращается свойством DialogOwnerWindow.
    /// 
    /// Можно использовать класс EFPDialogOwnerWindowHandler для временной установки свойства.
    /// </summary>
    public static IWin32Window ExternalDialogOwnerWindow
    {
      get
      {
        if (IsMainThread)
          return ValidateWindow(_ExternalDialogOwnerWindow);
        else
          return null;
      }
      set
      {
#if DEBUG
        CheckMainThread();
#endif

#if TRACE_DIALOGOWNERWINDOW
        if (value == null)
          Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow is about to be set to null");
        else
          Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow is about to be set to Handle=" + value.Handle.ToString());
        Trace.WriteLine(Environment.StackTrace);
#endif

        if (value != null && MainWindow != null)
        {
          if (value.Handle == MainWindow.Handle)
          {
#if TRACE_DIALOGOWNERWINDOW
            Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow set value is EFPApp.MainWindow");
#endif
            //throw new ArgumentException("Свойство не может быть установлено на главное окно программы");
            value = null; // 02.11.2016
          }
        }
        //if (value != null && ActiveDialog != null)
        //{
        //  if (value.Handle == ActiveDialog.Handle)
        //    value = null;
        //}
        // 14.03.2017
        // Проверяем все окна в стеке диалогов, а не только верхнее
        if (value != null)
        {
          foreach (Form dlg in _DialogStack)
          {
            if (dlg.IsDisposed)
              continue; // 17.09.2021
            if (value.Handle == dlg.Handle)
            {
#if TRACE_DIALOGOWNERWINDOW
              Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow set value is a dialog form (" + dlg.Text + ")");
#endif
              value = null;
              break;
            }
          }
        }

        _ExternalDialogOwnerWindow = ValidateWindow(value);
        _ExternalDialogOwnerWindowSetFlag = (value != null);

#if TRACE_DIALOGOWNERWINDOW
        if (_ExternalDialogOwnerWindow == null)
          Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow is set to null");
        else
          Trace.WriteLine("EFPApp.ExternalDialogOwnerWindow is set to Handle=" + _ExternalDialogOwnerWindow.Handle.ToString());
#endif
      }
    }
    private static IWin32Window _ExternalDialogOwnerWindow;

    /// <summary>
    /// 15.03.2017.
    /// Флаг показывает, что свойство ExternalDialogOwnerWindow было установлено.
    /// При выводе модального окна флаг запоминается и временно переключается в false.
    /// Свойство DialogOwnerForm учитывает ExternalDialogOwnerWindow, только если флаг установлен
    /// </summary>
    private static bool _ExternalDialogOwnerWindowSetFlag;

    /// <summary>
    /// Дополнительные цвета для раскрашивания управляющих элементов
    /// </summary>
    public static EFPAppColors Colors
    {
      get
      {
        if (_Colors == null)
          _Colors = new EFPAppColors();
        return _Colors;
      }
    }
    private static EFPAppColors _Colors;

    /// <summary>
    /// Повторная инициализация цветов для раскрашивания управляющих элементов
    /// </summary>
    public static void ResetColors()
    {
      _Colors = null;
    }

    /// <summary>
    /// Дополнительные цвета, исходящие из приложения, что цветом фона является белый
    /// Эти цвета не изменяются при смене оформления экрана
    /// </summary>
    public static EFPAppColors StdBasedColors { get { return _StdBasedColors; } }
    private static readonly EFPAppColors _StdBasedColors = new EFPAppColors(true);

    /// <summary>
    /// Список обработчиков таймеров, вызываемых 1 раз в секунду
    /// </summary>
    public static EFPAppTimers Timers { get { return _Timers; } }
    private static readonly EFPAppTimers _Timers = new EFPAppTimers();

    /// <summary>
    /// Список обработчиков события Idle
    /// </summary>
    public static EFPAppIdleHandlers IdleHandlers { get { return _IdleHandlers; } }
    private static readonly EFPAppIdleHandlers _IdleHandlers = new EFPAppIdleHandlers();

    #region Приостановка вызова обработчиков по таймеру и события Idle

    /// <summary>
    /// Временная приостановка вызова обработчиков IdleHandlers.
    /// Также прекращается обработка сигналов таймера в Timers.
    /// Для продолжения работы должен быть вызван парный метод ResumeIdle() в блоке finally.
    /// Разрешаются вызовы из любого потока и вложенные вызовы.
    /// 
    /// Работа оконного интерфейса полагается на вызовы сигналов Idle, поэтому длительная приостановка
    /// вызова обработчиков может привести к неправильной работе окон.
    /// Этот метод не предназначен для использования в прикладном коде.
    /// </summary>
    internal static void SuspendIdle()
    {
      Interlocked.Increment(ref _IdleSuspendCount);
    }

    /// <summary>
    /// Возобновление работы обработчиков IdleHandlers, прерванного SuspendIdle().
    /// Если было несколько вызовов SuspendIdle(), то должно быть такое же количество вызовов ResumeIdle().
    /// Этот метод не предназначен для использования в прикладном коде.
    /// </summary>
    internal static void ResumeIdle()
    {
      Interlocked.Decrement(ref _IdleSuspendCount);
      if (IsMainThread && (!_IdleHandlers.IsDisposed))
        _IdleHandlers.Application_Idle(null, null);
    }

    /// <summary>
    /// Возвращает true, если был непарный вызов SuspendIdle()
    /// </summary>
    internal static bool IdleSuspended { get { return _IdleSuspendCount > 0; } }
    private static int _IdleSuspendCount = 0;

    #endregion

    /// <summary>
    /// Расширенные методы работы с буфером обмена
    /// </summary>
    public static EFPAppClipboard Clipboard { get { return _Clipboard; } }
    private static readonly EFPAppClipboard _Clipboard = new EFPAppClipboard();

    #endregion

    #region Проверка дескрипотора окна

    /// <summary>
    /// Проверяет корректность дескриптора IWin32Window.
    /// Если реального окна нет, возвращает null.
    /// Иначе возвращает исходный аргумент
    /// </summary>
    /// <param name="window">Проверяемое окно</param>
    /// <returns>Окно или null</returns>
    private static IWin32Window ValidateWindow(IWin32Window window)
    {
      if (window == null)
        return null;

      try
      {
        Control ctrl = window as Control;
        if (ctrl != null)
        {
#if TRACE_DIALOGOWNERWINDOW
          Trace.WriteLine("EFPApp.ValidateWindow(). Control " + ctrl.GetType().ToString() + " is disposed");
#endif
          if (ctrl.IsDisposed)
            return null; // 04.10.2018
        }

        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            return ValidateWindowWin32(window);
          default:
            return window; // не проверяем
        }
      }
      catch
      {
        return null; // 04.10.2018
      }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWindowVisible(IntPtr hWnd);

    //[DebuggerStepThrough]
    private static IWin32Window ValidateWindowWin32(IWin32Window window)
    {
      try
      {
        if (IsWindow(window.Handle) &&
          IsWindowVisible(window.Handle)) // 13.02.2018
          return window;
        else
        {
#if TRACE_DIALOGOWNERWINDOW
          if (!IsWindow(window.Handle))
            Trace.WriteLine("EFPApp.ValidateWindowWin32(). IsWindow(" + window.Handle.ToString() + ") returned false");
          else if (!IsWindowVisible(window.Handle))
            Trace.WriteLine("EFPApp.ValidateWindowWin32(). IsWindowVisible(" + window.Handle.ToString() + ") returned false");
#endif
          return null;
        }
      }
      catch
      {
        return null;
      }
    }

    #endregion

    #region Упрощение интерфейса пользователя

    /// <summary>
    /// Использование упрощенного интерфейса (по умолчанию - false)
    /// Если свойство установлено, то для полей ввода не создаются команды локального
    /// меню (будут использоваться стандартные локальные меню Windows).
    /// Изменение значения свойства приводит к установке свойств ShowXXX, поэтому
    /// оно должно выполняться до установки этих свойств.
    /// Также установка в true устанавливает ToolStripManager.RenderMode в System
    /// </summary>
    public static bool EasyInterface
    {
      get { return _EasyInterface; }
      set
      {
        _EasyInterface = value;
        ShowControlToolBars = !value;
        ShowToolTips = !value;
        ShowListImages = !value;
        ShowAutoCalcSums = !value;
        ShowParamSetAuxText = !value;
        SaveFormBounds = !value;

        if (value)
        {
          Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.NoneEnabled; // 10.02.2018
          ToolStripManager.RenderMode = ToolStripManagerRenderMode.System; // 10.02.2018
          ToolStripManager.VisualStylesEnabled = false; // 10.02.2018
        }
      }
    }
    private static bool _EasyInterface = false;

    /// <summary>
    /// Глобальный флаг, управляющий наличием локальных панелей инструментов
    /// По умолчанию - true. Установка в false подавляет создание панелей для EFPControl
    /// </summary>
    public static bool ShowControlToolBars { get { return _ShowControlToolBars; } set { _ShowControlToolBars = value; } }
    private static bool _ShowControlToolBars = true;

    /// <summary>
    /// Глобальный флаг, управляющий наличием всплывающих подсказок
    /// По умолчанию (true) подсказки выводятся
    /// </summary>
    public static bool ShowToolTips { get { return _ShowToolTips; } set { _ShowToolTips = value; } }
    private static bool _ShowToolTips = true;

    /// <summary>
    /// Глобальный флаг, управляющий наличием изображений в списках и комбоблоках
    /// (кроме случаев, когда такое изображение является абсолютно необходимым)
    /// По умолчанию - true
    /// </summary>
    public static bool ShowListImages { get { return _ShowListImages; } set { _ShowListImages = value; } }
    private static bool _ShowListImages = true;

    /// <summary>
    /// Глобальный флаг, управляющий наличием в статусной строке для табличного 
    /// просмотра панелей подсчета сумм (как в Excel)
    /// По умолчанию - true
    /// </summary>
    public static bool ShowAutoCalcSums { get { return _ShowAutoCalcSums; } set { _ShowAutoCalcSums = value; } }
    private static bool _ShowAutoCalcSums = true;

    /// <summary>
    /// Нужно ли в комбоблоках "Готовые наборы" (фильтры табличных просмотров, отчеты с EFPRepotExtParams)
    /// выводить в выпадающем списке дополнительную строку с пояснениями.
    /// Для отчетов требуется переопределение метода EFPRepotExtParams.GetAuxText()
    /// По умолчанию - true - подсказки выводятся.
    /// Свойство устанавливается в false, если EasyInterface=true
    /// </summary>
    public static bool ShowParamSetAuxText { get { return _ShowParamSetAuxText; } set { _ShowParamSetAuxText = value; } }
    private static bool _ShowParamSetAuxText = true;

    #endregion

    #region Интерфейс пользователя

    /// <summary>
    /// Основное свойство, определяющее тип интерфейса.
    /// Это свойство должно быть установлено при запуске приложения, если оно хочет использовать новые возможности.
    /// Также свойство может быть установлено в процессе работы программы.
    /// Для установки интерфейса в приложении, поддерживающем несколько интерфейсов, например, MDI и SDI,
    /// удобнее использовать метод SetInterface()
    /// </summary>
    public static EFPAppInterface Interface
    {
      get { return _Interface; }
      set
      {
#if DEBUG
        CheckMainThread();
#endif
        if (InsideInterfaceAssignation)
          throw new InvalidOperationException("Вложенная установка свойства EFPApp.Interface");

        if (Object.ReferenceEquals(_Interface, value))
          return;

        _InsideInterfaceAssignation = true;
        BeginUpdateInterface();
        try
        {
          if (_Interface != null)
            _Interface.Detach();
          _Interface = value;
          if (value != null)
          {
            try
            {
              value.Attach();
            }
            catch
            {
              _Interface = null; // не удалось присоединить
              throw;
            }
          }
        }
        finally
        {
          _InsideInterfaceAssignation = false;
          EndUpdateInterface();
        }
        _InterfaceHasBeenAssigned = true;

        // Отключаем устаревшие списки
        _AppToolBars = null;
        _StatusBar = null;
      }
    }

    private static EFPAppInterface _Interface = null;

    /// <summary>
    /// Возвращает true, если свойству Interface было хотя бы один раз присвоено значение
    /// </summary>
    public static bool InterfaceHasBeenAssigned { get { return _InterfaceHasBeenAssigned; } }
    private static bool _InterfaceHasBeenAssigned = false;

    /// <summary>
    /// Возвращает true, если в данный момент выполняется присвоение значения свойству EFPApp.Interface
    /// </summary>
    public static bool InsideInterfaceAssignation { get { return _InsideInterfaceAssignation; } }
    private static bool _InsideInterfaceAssignation = false;

    /// <summary>
    /// Текущее значение свойства EFPAppInterface.Name.
    /// Если интерфейс не установлен, возвращает пустую строку.
    /// Для установки значения свойства необходимо наличие списка AvailableInterfaces.
    /// Попытка установки несуществующего имени интерфейса генерирует исключение ArgumentException.
    /// Для установки интерфейса в приложении, поддерживающем несколько интерфейсов, например, MDI и SDI,
    /// удобнее использовать метод SetInterface()
    /// </summary>
    public static string InterfaceName
    {
      get
      {
        if (Interface == null)
          return String.Empty;
        else
          return Interface.Name;
      }
      set
      {
        if (AvailableInterfaces == null)
          throw new NullReferenceException("Свойство AvailableInterfaces не установлено");
        if (String.IsNullOrEmpty(value))
          Interface = null;
        else
        {
          for (int i = 0; i < AvailableInterfaces.Length; i++)
          {
            if (AvailableInterfaces[i].Name == value)
            {
              Interface = AvailableInterfaces[i];
              return;
            }
          }
          throw new ArgumentException("Неизвестный интерфейс с кодом \"" + value + "\"");
        }
      }
    }

    /// <summary>
    /// Начальная установка интерфейса или переключение в процессе работы.
    /// На момент вызова должно быть установлено свойство AvailableInterfaces.
    /// Пытается установить один из интерфейсов из списка AvailableInterfaces. Если в списке нет
    /// интерфейса с заданным именем, выполняет установку интерфейса AvailableInterfaces[0].
    /// После установки интерфейса создает одно пустое главное окно. Для интерфейса SDI, если задан аргумент
    /// <paramref name="sdiFormType"/>, вместо пустого окна создает окно формы указанного класса.
    /// Если на момент вызова свойство EFPApp.Interface уже установлено (переключение в процессе работы),
    /// то выполняется попытка восстановления рабочего стола
    /// </summary>
    /// <param name="name">Тип интерейса: "MDI", "SDI", ...</param>
    /// <param name="sdiFormType">Класс формы, отображаемой по умолчанию в интерфейсе SDI</param>
    public static void SetInterface(string name, Type sdiFormType)
    {
      DoSetInterface(name);

      if (EFPApp.Interface == null)
        return;

      if (EFPApp.Interface.MainWindowCount > 0)
        return; // непустой интерфейс

      if (EFPApp.Interface.IsSDI && sdiFormType != null)
      {
        try
        {
          Form frm = (Form)(Activator.CreateInstance(sdiFormType, null));
          EFPApp.Interface.ShowChildForm(frm);
          return;
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось создать окно класса \"" + sdiFormType.ToString() + "\" для интефрейса SDI. Будет создано пустое окно");
        }
      }

      EFPApp.Interface.ShowMainWindow();
    }

    /// <summary>
    /// Начальная установка интерфейса или переключение в процессе работы.
    /// На момент вызова должно быть установлено свойство AvailableInterfaces.
    /// Пытается установить один из интерфейсов из списка AvailableInterfaces. Если в списке нет
    /// интерфейса с заданным именем, выполняет установку интерфейса AvailableInterfaces[0].
    /// После установки интерфейса создает одно пустое главное окно. Для интерфейса SDI, если задан аргумент
    /// <paramref name="sdiFormCreator"/>, вместо пустого окна создает окно формы, которое создаст пользовательский объект.
    /// Если на момент вызова свойство EFPApp.Interface уже установлено (переключение в процессе работы),
    /// то выполняется попытка восстановления рабочего стола
    /// </summary>
    /// <param name="name">Тип интерейса: "MDI", "SDI", ...</param>
    /// <param name="sdiFormCreator">Генератор формы, отображаемой по умолчанию в интерфейсе SDI.
    /// Этот генератор получает на входе пустую секцию конфигурации</param>
    public static void SetInterface(string name, IEFPFormCreator sdiFormCreator)
    {
      DoSetInterface(name);

      if (EFPApp.Interface == null)
        return;

      if (EFPApp.Interface.MainWindowCount > 0)
        return; // непустой интерфейс

      if (EFPApp.Interface.IsSDI && sdiFormCreator != null)
      {
        try
        {
          EFPFormCreatorParams creatorParams = new EFPFormCreatorParams(CfgPart.Empty);
          Form frm = sdiFormCreator.CreateForm(creatorParams);
          EFPApp.Interface.ShowChildForm(frm);
          return;
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось создать окно для интефрейса SDI. Будет создано пустое окно");
        }
      }

      EFPApp.Interface.ShowMainWindow();
    }

    /// <summary>
    /// Начальная установка интерфейса или переключение в процессе работы.
    /// На момент вызова должно быть установлено свойство AvailableInterfaces.
    /// Пытается установить один из интерфейсов из списка AvailableInterfaces. Если в списке нет
    /// интерфейса с заданным именем, выполняет установку интерфейса AvailableInterfaces[0].
    /// После установки интерфейса создает одно пустое главное окно.
    /// Если среди используемых интерфейсов есть SDI, рекомендуется использовать другие перегрузки метода,
    /// т.к. эта перегрузка создает окно-"пустышку".
    /// Если на момент вызова свойство EFPApp.Interface уже установлено (переключение в процессе работы),
    /// то выполняется попытка восстановления рабочего стола.
    /// </summary>
    /// <param name="name">Тип интерейса: "MDI", "SDI", ...</param>
    public static void SetInterface(string name)
    {
      DoSetInterface(name);

      if (EFPApp.Interface == null)
        return;

      if (EFPApp.Interface.MainWindowCount > 0)
        return; // непустой интерфейс

      EFPApp.Interface.ShowMainWindow();
    }

    private static void DoSetInterface(string name)
    {
      if (EFPApp.AvailableInterfaces == null)
        throw new NullReferenceException("Свойство EFPApp.AvailableInterfaces не установлено");

      if (String.IsNullOrEmpty(name))
        name = EFPApp.AvailableInterfaces[0].Name;

      if (name == EFPApp.InterfaceName)
        return; // ничего делать не надо

      TempCfg tempCfg = null;
      if (!Object.ReferenceEquals(Interface, null))
      {
        try
        {
          // Установка с восстановлением конфигурации
          tempCfg = new TempCfg();
          Interface.SaveComposition(tempCfg);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось сохранить существующую композицию окон");
          tempCfg = null;
        }
      }


      try
      {
        InterfaceName = name;
      }
      catch (ArgumentException)
      {
        EFPApp.ErrorMessageBox("Запрошена установка неизвестного интерфейса пользователя \"" + name +
          "\". Будет использован интерфейс \"" + AvailableInterfaces[0].Name + "\"");
        Interface = AvailableInterfaces[0];
      }
      if (tempCfg != null)
      {
        try
        {
          Interface.LoadComposition(tempCfg);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось восстановить композицию окон");
        }
      }
    }

    #region Событие InterfaceStateChanged

    /// <summary>
    /// Сводная информация о состоянии интерфейса
    /// </summary>
    public static EFPAppInterfaceState InterfaceState { get { return _InterfaceState; } }
    private static EFPAppInterfaceState _InterfaceState = new EFPAppInterfaceState();

    /// <summary>
    /// Принудительно вызывает событие InterfaceChanged, независимо от наличия реальных изменений.
    /// Ксли не было вызова BeginUpdateInterface(), событие вызывается немедленно.
    /// Иначе, событие будет вызвано при (последнем) вызове EndUpdateInterface()
    /// </summary>
    public static void SetInterfaceChanged()
    {
      _InterfaceChangedFlag = true;
      if (_UpdateInterfaceCount == 0)
        TestInterfaceChanged();
    }

    private static bool _InterfaceChangedFlag = false;

    /// <summary>
    /// Этот метод не должен использоваться в пользовательском коде
    /// </summary>
    public static void BeginUpdateInterface()
    {
      _UpdateInterfaceCount++;
    }

    /// <summary>
    /// Этот метод не должен использоваться в пользовательском коде
    /// </summary>
    public static void EndUpdateInterface()
    {
      _UpdateInterfaceCount--;
#if DEBUG
      if (_UpdateInterfaceCount < 0)
        throw new InvalidOperationException("Лишний вызов EFPApp.EndUpdateInterface()");
#endif
      TestInterfaceChanged();
    }

    private static int _UpdateInterfaceCount = 0;

    /// <summary>
    /// Этот метод не должен использоваться в пользовательском коде
    /// </summary>
    public static void TestInterfaceChanged()
    {
      if (_UpdateInterfaceCount > 0)
        return;

      EFPAppInterfaceState newState = new EFPAppInterfaceState();
      //System.Diagnostics.Trace.WriteLine("TestInterfaceChanged: " + (NewState == FInterfaceState).ToString(), "CurrentChildForm=" + DataTools.GetString(NewState.CurrentChildForm));

      if ((newState == _InterfaceState) && (!_InterfaceChangedFlag)) // проверяем наличие изменений
        return;

      _InterfaceState = newState;
      _InterfaceChangedFlag = false;
      OnInterfaceChanged(); // вызов события
    }

    private static void OnInterfaceChanged()
    {
      if (InterfaceChanged != null)
        InterfaceChanged(null, EventArgs.Empty);

      CommandItems.OnMdiCildrenChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Событие вызывается при переключении окон в пользовательском интерфейсе
    /// </summary>
    public static event EventHandler InterfaceChanged;

    #endregion

    /// <summary>
    /// Используется, если приложение поддерживает несколько вариантов интерфейса (например, MDI и SDI). Используется методом LoadComposition() 
    /// </summary>
    public static EFPAppInterface[] AvailableInterfaces
    {
      get { return _AvailableInterfaces; }
      set
      {
#if DEBUG
        CheckMainThread();
#endif
        if (MainWindow != null)
          throw new InvalidOperationException("Главное окно уже выведено");
        if (value != null)
        {
          if (value.Length == 0)
            throw new ArgumentException("Список пустой");
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] == null)
              throw new ArgumentNullException();
          }
        }
        _AvailableInterfaces = value;
      }
    }
    private static EFPAppInterface[] _AvailableInterfaces = null;

    /// <summary>
    /// Дублирует свойство EFPAppInterface.MainWindowNumberTextUsed
    /// </summary>
    public static bool MainWindowNumberUsed
    {
      get
      {
        if (Interface == null)
          return false;
        else
          return Interface.MainWindowNumberUsed;
      }
    }

    /// <summary>
    /// Возвращает номер главного окна ("#1", "#2", ...) для заданного дочернего окна.
    /// Если свойство MainWindowNumberTextUsed возвращает false, возвращается пустая строка
    /// </summary>
    /// <param name="form">Дочернее окно</param>
    /// <returns>Строка с номером или пустая строка</returns>
    public static string GetMainWindowNumberText(Form form)
    {
      if (!MainWindowNumberUsed)
        return String.Empty;
      EFPAppMainWindowLayout mw = Interface.FindMainWindowLayout(form);
      if (mw == null)
        return String.Empty;
      else
        return mw.MainWindowNumberText;
    }

    #endregion

    #region Композиция окна

    internal const string CompositionConfigSectionName = "Composition";

    /// <summary>
    /// Загружает интерфейс из секции «Composition» - «UI».
    /// На момент вызова должно быть либо установлено свойство Interface, либо AvailableInterfaces.
    /// </summary>
    public static void LoadComposition()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(CompositionConfigSectionName,
        EFPConfigCategories.UI, String.Empty);
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
      {
        LoadComposition(cfg);
      }
    }


    /// <summary>
    /// Загружает интерфейс из произвольной секции конфигурации.
    /// Устанавливает свойство Interface и вызывает EFPAppInterface.LoadComposition(),
    /// Используется окном SelectCompositionDialog().
    /// Также может использоваться пользовательским кодом 
    /// На момент вызова должно быть либо установлено свойство Interface, либо AvailableInterfaces.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public static void LoadComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (InsideLoadComposition || InsideSaveComposition)
        throw new ReenteranceException("Вложенный вызов LoadComposition");

      InsideLoadComposition = true;
      try
      {
        if (AvailableInterfaces != null)
        {
          string interfaceType = cfg.GetString("InterfaceType");
          if (String.IsNullOrEmpty(interfaceType))
            EFPApp.Interface = AvailableInterfaces[0];
          else
          {
            int p = -1;
            for (int i = 0; i < AvailableInterfaces.Length; i++)
            {
              if (AvailableInterfaces[i].Name == interfaceType)
              {
                p = i;
                break;
              }
            }
            if (p >= 0)
              EFPApp.Interface = AvailableInterfaces[p];
            else
            {
              EFPApp.WarningMessageBox("Интефрейс типа \"" + interfaceType + "\" недоступен. Будет использован интерфейс \"" + AvailableInterfaces[0].Name + "\"", "Восстановление интерфейса");
              EFPApp.Interface = AvailableInterfaces[0];
            }
          }
        }
        else if (Interface == null)
          throw new NullReferenceException("Не установлен список доступных интерфейсов EFPApp.AvailableInterfaces и текущий интерфейс EFPApp.Interface также не задан");
      }
      finally
      {
        InsideLoadComposition = false;
      }

      Interface.LoadComposition(cfg); // основной метод загрузки 
    }

    /// <summary>
    /// Возвращает true, если в данный момент выполняется метод LoadComposition()
    /// </summary>
    public static bool InsideLoadComposition
    {
      get { return _InsideLoadComposition; }
      internal set { _InsideLoadComposition = value; }
    }
    private static bool _InsideLoadComposition;

    /// <summary>
    /// Сохраняет интерфейс в секцию «Composition» - «UI»
    /// Если CompositionHistoryCount > 0, запоминает несколько последних вариантов настроек.
    /// </summary>
    public static void SaveComposition()
    {
      if (ConfigManager == null || Interface == null)
        return;

      // Записываем во временную секцию
      TempCfg tmp = new TempCfg();
      Interface.SaveComposition(tmp);

      Bitmap snapshot = null;
      try
      {
        snapshot = EFPApp.CreateSnapshot(true);
      }
      catch { }

      InsideSaveComposition = true;
      try
      {
        if (EFPApp.CompositionHistoryCount > 0)
          EFPAppCompositionHistoryHandler.SaveHistory(tmp, snapshot);

        #region Основная секция UI

        EFPConfigSectionInfo configInfoUI = new EFPConfigSectionInfo(CompositionConfigSectionName,
          EFPConfigCategories.UI, String.Empty);
        CfgPart cfgUI;
        using (ConfigManager.GetConfig(configInfoUI, EFPConfigMode.Write, out cfgUI))
        {
          cfgUI.Clear();
          tmp.CopyTo(cfgUI);
        }

        EFPConfigSectionInfo configInfoSnapshot = new EFPConfigSectionInfo(CompositionConfigSectionName,
          EFPConfigCategories.UISnapshot, String.Empty);
        EFPApp.SnapshotManager.SaveSnapshot(configInfoSnapshot, snapshot);

        #endregion
      }
      finally
      {
        InsideSaveComposition = false;
      }
    }


    /// <summary>
    /// Сохраняет интерфейс в выбранную секцию конфигурации.
    /// Вызывает EFPAppInterface.SaveComposition().
    /// Snapshot не сохраняется
    /// </summary>
    /// <param name="cfg">Секция конфигурации для записи</param>
    public static void SaveComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (Interface == null)
        return;
      Interface.SaveComposition(cfg);
    }

    /// <summary>
    /// Возвращает true, если в данный момент выполняется метод LoadComposition()
    /// </summary>
    public static bool InsideSaveComposition
    {
      get { return _InsideSaveComposition; }
      internal set { _InsideSaveComposition = value; }
    }
    private static bool _InsideSaveComposition;

#if XXX
    // Вызывает LoadComposition() и устанавливает обработчик EFPApp.Closing, который вызовет SaveComposition() при завершении приложения.
    // Аргумент SDIFormType позволяет задать класс дочернего окна, которое будет открыто,
    // если интерфейс SDI, а прочитать композицию не удалось.
    public static void SetAutoComposition(Type SDIFormType)
    {
      throw new NotImplementedException();
    }
#endif

    /// <summary>
    /// Список генераторов форм, которые не могут быть созданы с помощью Reflection
    /// </summary>
    public static ICollection<IEFPFormCreator> FormCreators { get { return _FormCreators; } }
    private static readonly List<IEFPFormCreator> _FormCreators = new List<IEFPFormCreator>();


    /// <summary>
    /// Создает форму с помощью одного из объектов в списке FormCreators илм с помощью Reflection.
    /// Если нет создателя формы или подходящего конструктора, возвращается null.
    /// Форма не выводится на экран.
    /// Если при создании формы возникло исключение, оно не перехватывается
    /// </summary>
    /// <param name="creatorParams">Параметры для создания формы</param>
    /// <returns>Форма или null</returns>
    public static Form CreateForm(EFPFormCreatorParams creatorParams)
    {
      if (creatorParams == null)
        throw new ArgumentNullException("creatorParams");

      // 20.09.2018 - не все формы восстанавливаются корректно
      if (String.IsNullOrEmpty(creatorParams.ConfigSectionName))
        return null;

      #region В списке FormCreators

      foreach (IEFPFormCreator creator in FormCreators)
      {
        Form res = creator.CreateForm(creatorParams);
        if (res != null)
          return res;
      }

      #endregion

      #region С помощью конструктора по умолчанию

      Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
      Type t = null;
      for (int i = 0; i < asms.Length; i++)
      {
        t = asms[i].GetType(creatorParams.ClassName);
        if (t != null)
          break;
      }
      if (t == null)
        return null;

      if (t == typeof(Form))
        return null; // 17.06.2021. Если нет переопределенного класса, то бесполезно создавать пустую форму.
      // Возникнет ошибка при попытке получения EFPFormProvider.
      // Может быть, стоит выдать в Trace сообщение об ошибке.

      ConstructorInfo ci = t.GetConstructor(Type.EmptyTypes);
      if (ci == null)
        return null; // нет конструктора без параметров

      object x = ci.Invoke(null);
      if (x is Form)
        return (Form)x;

      #endregion

      #region Вложенный IEFPFormCreator

      IEFPFormCreator fc = x as IEFPFormCreator;
      if (fc != null)
        return fc.CreateForm(creatorParams);

      #endregion

      throw new InvalidCastException("Класс " + t.ToString() + " не является производным от Form и не поддерживает интерфейс IEFPFormCreator");
    }


    /// <summary>
    /// Надо ли сохранять последние композиции интерфейса. 
    /// По умолчанию 0 – история не сохраняется.
    /// Иначе задает количество сохраняемых вариантов (например, 10).
    /// Именные варианты не входят в это количество
    /// Влияет на работу метода SaveComposition()
    /// </summary>
    public static int CompositionHistoryCount
    {
      get { return _CompositionHistoryCount; }
      set
      {
#if DEBUG
        CheckMainThread();
#endif
        if (value < 0)
          throw new ArgumentException();

        if (value == _CompositionHistoryCount)
          return;

        _CompositionHistoryCount = value;

        OnInterfaceChanged(); // независимо от изменения в открытых окнах
      }
    }
    private static int _CompositionHistoryCount = 0;

    /// <summary>
    /// Возвращает true, если дочерняя форма желает сохранять композицию
    /// </summary>
    /// <param name="form">Проверяемая форма. Если null, то возвращается false</param>
    /// <returns>Требуется сохранение композиции?</returns>
    internal static bool FormWantsSaveComposition(Form form)
    {
      if (form == null)
        return false;
      EFPFormProvider formProvider = EFPFormProvider.FindFormProvider(form);
      if (formProvider == null)
        return false; // этого не должно быть

      if (formProvider.ConfigClassName.Length == 0)
        return false; // не сохраняется

      // Может не быть класса-наследника формы, если есть подходящий IEFPFormCreator.
      // if (form.GetType() == typeof(Form))
      //   return false; 

      if (String.IsNullOrEmpty(formProvider.ConfigSectionName))
        return false; // 20.09.2018 Не все формы корректно восстанавливаются

      return true;
    }

    #endregion

    #region Сохранение видимости элементов главного окна

    internal const string MainWindowConfigSectionName = "MainWindow"; // исправлено 22.05.2020

    /// <summary>
    /// Сохраняет видимость панели инструментов и статусной строки главного окна
    /// в секции "MainWindow"-"MainWindow".
    /// Этот метод должен вызываться в явном виде в обработчике события EFPApp.BeforeClosing.
    /// Нельзя выполнять это действие в обработчиках Closing и Closed, т.к. в это время окна уже могут быть закрыты.
    /// Этот метод рекомендуется вызывать, даже если вызывается SaveComposition().
    /// </summary>
    public static void SaveMainWindowLayout()
    {
      if (EFPApp.Interface == null)
        throw new NullReferenceException("Свойство Interface не установлено");

      if (ConfigManager == null)
        throw new NullReferenceException("Свойство ConfigManager не установлено");

      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(MainWindowConfigSectionName,
        EFPConfigCategories.MainWindow, String.Empty);
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        EFPApp.Interface.SaveMainWindowLayout(cfg);
      }
    }


    /// <summary>
    /// Устанавливает видимость панели инструментов и статусной строки главного окна
    /// из секции "MainWindow"-"MainWindow".
    /// Этот метод должен вызываться в явном виде после инициализации главного окна,
    /// если не вызывается LoadComposition().
    /// </summary>
    public static void LoadMainWindowLayout()
    {
      if (EFPApp.Interface == null)
        throw new NullReferenceException("Свойство Interface не установлено");

      if (ConfigManager == null)
        throw new NullReferenceException("Свойство ConfigManager не установлено");

      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(MainWindowConfigSectionName,
        EFPConfigCategories.MainWindow, String.Empty);
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
      {
        EFPApp.Interface.LoadMainWindowLayout(cfg);
      }
    }

    #endregion

    #region Главное окно

    /// <summary>
    /// Вызывается приложением перед запуском цикла обработки сообщений.
    /// Если свойство Interface установлено, то создает главное окно для этого интерфейса.
    /// Иначе используется устаревший метод создания интерфейса MDI
    /// </summary>
    public static void ShowMainWindow()
    {
#if DEBUG
      CheckMainThread();
#endif

      if (InterfaceHasBeenAssigned)
        Interface.ShowMainWindow();
      else
      {
        // Устаревший вариант
#if DEBUG
        if (EFPApp.StatusBar == null)
          throw new BugException("EFPApp.StatusBar==null");
#endif

        EFPAppToolBars tb2 = EFPApp.AppToolBars;
        EFPAppStatusBar sb2 = EFPApp.StatusBar;

        Interface = new EFPAppInterfaceMDI();
        Interface.ObsoleteMode = true;
        EFPAppMainWindowLayout lo = Interface.ShowMainWindow();
        lo.ToolBars = tb2;
        lo.StatusBar = sb2;

        FormToolStripInfo info = new FormToolStripInfo(lo.MainWindow);

        //foreach (EFPAppToolBar tb in tb2)
        //  tb.Info = new FormToolStripInfo(lo.MainWindow);

        sb2.StatusStripControl = info.StatusBar;
      }
    }

    /// <summary>
    /// Возвращает true, если был вызов ShowMainWindow() в режиме совместимости со старыми программами,
    /// без предварительной установки свойства Interface
    /// </summary>
    public static bool IsObsoleteInterfaceMode
    {
      get
      {
        EFPAppInterfaceMDI mdi = EFPApp.Interface as EFPAppInterfaceMDI;
        if (mdi == null)
          return false;
        else
          return mdi.ObsoleteMode;
      }
    }

    /// <summary>
    /// Ссылка на главное окно программы, используемая, например, при вызове 
    /// MessageBox(). Если главное окно не открыто или уже закрыто, то возвращается null
    /// При обращении не из основного потока приложения возвращается null.
    /// 
    /// При открытии основного окна программы (при вызове ShowMainWindow()) свойство должно быть установлено,
    /// а при закрытии - присвоено значение null.
    /// Обработчик события пересоздания дескриптора должен устанавливать свойство повторно
    /// </summary>
    public static Form MainWindow
    {
      get
      {
        if (IsMainThread)
          return _MainWindow;
        else
          return null;
      }
      set
      {
#if DEBUG
        CheckMainThread();
#endif
        if (value == null)
          _MainWindowHandle = IntPtr.Zero;
        else if (value.IsHandleCreated)
          _MainWindowHandle = value.Handle;
        else
          _MainWindowHandle = IntPtr.Zero;

        _MainWindow = value;

        if (!EFPApp.InsideLoadComposition)
          EFPApp.TestInterfaceChanged();
      }
    }
    private static Form _MainWindow;

    /// <summary>
    /// Дескриптор основного окна программы, если оно открыто или Zero, если окно не открыто
    /// Доступ к окну по дескриптору является потокобезопасным, поэтому свойство доступно всегда
    /// </summary>
    public static IntPtr MainWindowHandle { get { return _MainWindowHandle; } }
    private static IntPtr _MainWindowHandle;

    /// <summary>
    /// Возвращает true, если главное окно открыто
    /// Можно обращаться к свойству из любого потока
    /// </summary>
    public static bool MainWindowVisible
    {
      get
      {
        return (_MainWindow != null);
      }
    }

    /// <summary>
    /// Заголовок главного окна приложения.
    /// Свойство может быть получено и установлено только в основном потоке приложения.
    /// При доступе не из основного потока возвращается пустая строка
    /// </summary>
    public static string MainWindowTitle
    {
      get
      {
        return _MainWindowTitle;
      }
      set
      {
#if DEBUG
        CheckMainThread();
#endif
        _MainWindowTitle = value;
        if (Interface != null)
          Interface.InitMainWindowTitles();
      }
    }
    private static string _MainWindowTitle = String.Empty;

    /// <summary>
    /// Если true (по умолчанию), то главное окно будет развернуто на весь экран.
    /// Если свойство сбросить в false, то окно будет иметь размеры по умолчанию.
    /// Свойство может устанавливаться только до вызова ShowMainWindow().
    /// </summary>
    public static bool MainWindowDefaultMaximized
    {
      get { return _MainWindowDefaultMaximized; }
      set
      {
#if DEBUG
        CheckMainThread();
        if (MainWindow != null)
          throw new InvalidOperationException("Главное окно уже выведено");
#endif
        _MainWindowDefaultMaximized = value;
      }
    }
    private static bool _MainWindowDefaultMaximized = true;

    /// <summary>
    /// Вывод окна приложения на первый план.
    /// При этом выключается экранная заставка.
    /// На некоторых версиях Windows возможно включение дисплея, если он выключен для энергосбережения.
    /// </summary>
    public static void Activate()
    {
      try
      {
        FreeLibSet.Win32.PowerSuspendLocker.TurnDisplayOn(); // отключение экранной заставки

        //#if DEBUG
        //      CheckTread();
        //#endif
        // 12.05.2016
        // Вызов не из основного потока не является ошибкой. Просто нельзя ничего сделать
        if (!IsMainThread)
          return;

        Form frm = MainWindow; // 18.08.2021. Используем локальную переменную, т.к.MainWindow может внезапно обнулиться.
        if (frm == null)
          return;

        if (frm.WindowState == FormWindowState.Minimized)
          frm.WindowState = FormWindowState.Maximized;
        frm.BringToFront();
        frm.Activate();
      }
      catch { } // 18.08.2021
    }

    #endregion

    #region Экран

    /*
     *     /// Свойство нельзя использовать для размещения формы из обработчика Form.OnLoad(),
    /// т.к. оно может вернуть экран, на котором размещена 

     * */

    /// <summary>
    /// Экран по умолчанию.
    /// Если выведено главное окно программы, то возвращается экран, на котором оно находится.
    /// Иначе возвращается экран, на котором последний раз был выведен блок диалога.
    /// Если RunDialog() не разу не вызывался, возвращается StartupScreen.
    /// Свойство никогда не возвращает null.
    /// Свойство является потокобезопасным.
    /// Установка свойства не имеет значения, пока не выведено главное окно программы
    /// </summary>
    public static Screen DefaultScreen
    {
      get
      {
        lock (WinFormsTools.InternalSyncRoot)
        {
          IWin32Window dow = DialogOwnerWindow; // 31.08.2020 может меняться асинхронно
          if (dow != null) // 24.11.2016
            return Screen.FromHandle(dow.Handle);

          if (MainWindowVisible)
            return Screen.FromHandle(MainWindowHandle); // написано, что является потокобезопасным
          else if (_DefaultScreen == null)
            return StartupScreen;
          else
            return _DefaultScreen;
        }
      }
      set
      {
        if (value == null)
          return;
        lock (WinFormsTools.InternalSyncRoot)
        {
          _DefaultScreen = value;
        }
      }
    }
    private static Screen _DefaultScreen = null;

    /// <summary>
    /// Экран, с которого приложение было запущено.
    /// В течение сеанса работы свойство возвращает одно и то же значение.
    /// Свойство доступно из любого потока
    /// </summary>
    public static Screen StartupScreen { get { return _StartupScreen; } }
    private static readonly Screen _StartupScreen = GetStartupScreen();

    private static Screen GetStartupScreen()
    {
      try
      {
        // Так определяется экран для формы, когда задано свойство Form.StartPosition=CenterScreen
        // См. исходные тексты Net Framework, методы 
        // System.Windows.Forms.Form.CreateParams()
        //   System.Windows.Forms.Form.FillInCreateParamsStartPosition()
        Screen scr = Screen.FromPoint(Control.MousePosition);

        if (scr == null)
          return Screen.PrimaryScreen; // Агеев А.В.
        else
          return scr;
      }
      catch
      {
        return Screen.PrimaryScreen;
      }
    }

    /// <summary>
    /// Размещение формы в центре экрана, содержащего главное окно программы
    /// или экрана, на котором последний раз был показан блок диалога.
    /// Если в данный момент есть выведенный блок диалога, то используется
    /// экран, на котором он находится
    /// </summary>
    /// <param name="form"></param>
    public static void PlaceFormInScreenCenter(Form form)
    {
      WinFormsTools.PlaceFormInScreenCenter(form, DefaultScreen);
    }

    #endregion

    #region Главное меню, панели инструментов и статусная строка

    /// <summary>
    /// Команды главного меню, основных панелей инструментов и статусной строки
    /// </summary>
    public static EFPAppCommandItems CommandItems { get { return _CommandItems; } }
    private static readonly EFPAppCommandItems _CommandItems = new EFPAppCommandItems();


    /// <summary>
    /// Список панелей инструментов. Используется совместно со списком EFPApp.CommandItems 
    /// при инициализации приложения.
    /// </summary>
    public static EFPAppToolBarList ToolBars { get { return _ToolBars; } }
    private static readonly EFPAppToolBarList _ToolBars = new EFPAppToolBarList();

    /// <summary>
    /// Список основых панелей инструментов с кнопками, дублирующими команды главного меню.
    /// Если EFPApp.Interface установлено, возвращает панели инструментов для текущего главного окна
    /// </summary>
    //[Obsolete("В новых программах следует использовать EFPApp.Interface и EFPApp.ToolBars")]
    public static EFPAppToolBars AppToolBars
    {
      get
      {
        if (InterfaceHasBeenAssigned)
        {
          if (EFPApp.Interface == null)
            return null;
          if (EFPApp.Interface.CurrentMainWindowLayout == null)
            return null;
          return EFPApp.Interface.CurrentMainWindowLayout.ToolBars;
        }

        // Для старых приложений
        if (_AppToolBars == null)
          _AppToolBars = new EFPAppToolBars();
        return _AppToolBars;
      }
    }
    private static EFPAppToolBars _AppToolBars = null;

    /// <summary>
    /// Список зарегистрированных панелей Tool Window
    /// Среди форм могут быть как видимые, так и скрытые
    /// </summary>
    public static EFPAppToolWindows ToolWindows { get { return _ToolWindows; } }
    private static readonly EFPAppToolWindows _ToolWindows = new EFPAppToolWindows();

    /// <summary>
    /// Список окон инструментов, типа калькулятора, которые должны оставаться
    /// доступными во время работы модальных форм (диалогов).
    /// Для обычных инструментов, которые не доступны только из MDI-окна, достаточно
    /// установить свойство Owner=EFPApp.MainForm. 
    /// Для "вездесущих" панелей требуется специальная обработка, поэтому такие
    /// формы должны быть зарегистрированы.
    /// Когда форма панели скрывается, но не удаляется, она должна удалить себя из этого списка
    /// Также форма панели должна определять свойство ShowWithoutActivation
    /// </summary>
    internal static List<Form> ToolFormsForDialogs { get { return _ToolFormsForDialogs; } }
    private static readonly List<Form> _ToolFormsForDialogs = new List<Form>();

    /// <summary>
    /// Доступ к статусной строке текущего окна.
    /// Если EFPApp.Interface = null, возвращает временный объект
    /// </summary>
    //[Obsolete("В новых программах следует использовать EFPApp.Interface")]
    public static EFPAppStatusBar StatusBar
    {
      get
      {
        if (InterfaceHasBeenAssigned)
        {
          if (EFPApp.Interface == null)
            return null;
          if (EFPApp.Interface.CurrentMainWindowLayout == null)
            return null;
          return EFPApp.Interface.CurrentMainWindowLayout.StatusBar;
        }

        // Для старых приложений
        if (_StatusBar == null)
          _StatusBar = new EFPAppStatusBar();
        return _StatusBar;
      }
    }
    private static EFPAppStatusBar _StatusBar = null;

    #region Свойство OwnStatusBarsIfNeeded

    /// <summary>
    /// Использовать ли для форм собственные статусные строки, если это необходимо.
    /// По умолчанию свойство имеет значение true.
    /// Если свойство сброшено в false, то свойство EFPFormProvider.OwnStatusBar возвращает
    /// false, если оно не установлено в явном виде
    /// </summary>
    public static bool OwnStatusBarsIfNeeded
    {
      get { return _OwnStatusBarsIfNeeded; }
      set { _OwnStatusBarsIfNeeded = value; }
    }
    private static bool _OwnStatusBarsIfNeeded = true;

    #endregion

    /// <summary>
    /// Это событие вызывается перед событием EFPCommandItem.Click для любой команды главного или локального меню, панели инструментов или статусной строки.
    /// Обработчик может отменить выполнение команды.
    /// Свойство sender имеет значение null.
    /// Обработчик вызывается до EFPCommandItems.BeforeClick.
    /// </summary>
    public static event EFPCommandItemBeforeClickEventHandler BeforeCommandItemClick;

    /// <summary>
    /// Вызов обработчика события BeforeCommandItemClick
    /// </summary>
    /// <param name="commandItem">Вызываемая команда</param>
    /// <returns>Инвентированное значение свойства Cancel</returns>
    internal static bool OnBeforeCommandItemClick(EFPCommandItem commandItem)
    {
      if (BeforeCommandItemClick == null)
        return true;
      EFPCommandItemBeforeClickEventArgs args = new EFPCommandItemBeforeClickEventArgs(commandItem);
      BeforeCommandItemClick(null, args);
      return !args.Cancel;
    }

    /// <summary>
    /// Это событие вызывается после события EFPCommandItem.Click для любой команды главного или локального меню, панели инструментов или статусной строки
    /// Свойство sender имеет значение null.
    /// Обработчик вызывается после EFPCommandItems.AfterClick.
    /// </summary>
    public static event EFPCommandItemAfterClickEventHandler AfterCommandItemClick;

    /// <summary>
    /// Вызов обработчика события BeforeCommandItemClick
    /// </summary>
    /// <param name="commandItem">Вызываемая команда</param>
    /// <returns>Инвентированное значение свойства Cancel</returns>
    internal static void OnAfterCommandItemClick(EFPCommandItem commandItem)
    {
      if (AfterCommandItemClick != null)
      {
        EFPCommandItemAfterClickEventArgs args = new EFPCommandItemAfterClickEventArgs(commandItem);
        AfterCommandItemClick(null, args);
      }
    }

    /// <summary>
    /// Устанавливает свойство StatusStrip.AutoSize=false и Height=24, с учетом текущего масштабирования формы
    /// </summary>
    /// <param name="statusControl">Статусная строка</param>
    /// <param name="form">Форма, на которой располагается или будет располагаться статусная строка</param>
    internal static void SetStatusStripHeight(StatusStrip statusControl, Form form)
    {
      statusControl.AutoSize = false;
      if (form.CurrentAutoScaleDimensions.Height > 0)
        statusControl.Height = (int)Math.Round(24 * form.CurrentAutoScaleDimensions.Height / 13F); // 18.02.2020
      else
        statusControl.Height = 24;
    }

    #endregion

    #region Значки 16x16 для меню

    #region Свойство MainImages

    private static void InitImages()
    {
      // 04.09.2012
      // Нельзя использовать список frm.MainImageList в качестве статического списка.
      // Возникают глюки - некоторые изображения черные

      _MainImages = new ImageList();
      _MainImages.ImageSize = new Size(16, 16);
      _MainImages.ColorDepth = ColorDepth.Depth4Bit;

      DummyForm frm = new DummyForm();
      AddMainImages(frm.MainImageList); // Стандартные изображения
    }

    /// <summary>
    /// Основной список изображений для значков.
    /// Значки используются в командах меню, кнопках, списках и других элементах интерфейса.
    /// Список заполнен стандартными изображениями и может быть дополнен изображениями при инициализации программы
    /// с помощью AddMainImages().
    /// Изображения имеют размер 16x16 пикселей, 16 цветов. Цвет Magenta используется в качестве прозрачного.
    /// </summary>
    public static ImageList MainImages
    {
      get
      {
#if DEBUG
        CheckMainThread();
#endif

        return _MainImages;
      }
    }
    private static ImageList _MainImages;

    /// <summary>
    /// Добавляет в список MainImages изображения, специфичные для приложения.
    /// Используется только при инициализации программы.
    /// Если в списке MainImages уже есть изображение с совпадающим ключом, то оно будет заменено на новое
    /// </summary>
    /// <param name="srcImages">Пользовательский список изображений</param>
    public static void AddMainImages(ImageList srcImages)
    {
#if DEBUG
      CheckMainThread();
#endif

      WinFormsTools.CopyImages(srcImages, MainImages, true /* 10.01.2019 */);
    }

    #endregion

    #region MainImageIcon()

    /// <summary>
    /// Получение значка для формы, соответствующего изображению в списке MainImages.
    /// Если задано несуществующее имя изображения, то возвращается null
    /// </summary>
    /// <param name="imageKey">Имя изображения в списке MainImages</param>
    /// <returns>Соответствующая изображению иконка для формы</returns>
    public static Icon MainImageIcon(string imageKey)
    {
      if (MainThread == null)
        return null; // Не было вызова InitApp()

#if DEBUG
      CheckMainThread();
#endif

      if (String.IsNullOrEmpty(imageKey))
        return null;
      Icon res;
      if (!_MainImageIcons.TryGetValue(imageKey, out res))
      {
        Bitmap bmp = MainImages.Images[imageKey] as Bitmap;
        if (bmp == null)
          return null;
        res = Icon.FromHandle(bmp.GetHicon());
        _MainImageIcons.Add(imageKey, res);
      }
      return res;
    }

    /// <summary>
    /// Буферизация значков для форм
    /// </summary>
    private static readonly Dictionary<string, Icon> _MainImageIcons = new Dictionary<string, Icon>();

#if XXX
    /// <summary>
    /// Установка значка формы (свойства Form.Icon и Form.ShowIcon)
    /// Если задано имя изображения, то форма будет иметь значок.
    /// Иначе свойство ShowIcon сбрасывается в false, но форме присваивается иконка приложения, чтобы форма
    /// правильно отображалась в панели задач
    /// </summary>
    /// <param name="form">Инициализируемая форма</param>
    /// <param name="imageKey">Имя изображения из списка EFPApp.MainImages</param>
    [Obsolete("Используйте перегрузку метода с 3 аргументами", false)]
    public static void InitMainImageIcon(Form form, string imageKey)
    {
      InitMainImageIcon(form, imageKey, false);
    }
#endif

    /// <summary>
    /// Установка значка формы (свойства Form.Icon и Form.ShowIcon)
    /// Если задано имя изображения, то форма будет иметь значок.
    /// Иначе свойство ShowIcon сбрасывается в false, но форме присваивается иконка приложения, чтобы форма
    /// правильно отображалась в панели задач.
    /// </summary>
    /// <param name="form">Инициализируемая форма</param>
    /// <param name="imageKey">Имя изображения из списка EFPApp.MainImages</param>
    /// <param name="modal">True, если форма будет показана в модальном режиме, false - если в немодальном</param>
    public static void InitMainImageIcon(Form form, string imageKey, bool modal)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (!String.IsNullOrEmpty(imageKey))
      {
        form.ShowIcon = true;
        form.Icon = EFPApp.MainImageIcon(imageKey);
        return;
      }

      if (modal && DialogOwnerWindow != null)
      {
        // 01.03.2021
        form.ShowIcon = false;
        return;
      }

      WinFormsTools.InitAppIcon(form);
      form.ShowIcon = !EFPApp.MainWindowVisible;
    }

    #endregion

    #region InitFormImages

    /// <summary>
    /// Установка изображений для кнопок "ОК", "Отмена" в форме
    /// </summary>
    /// <param name="form">Форма, в которой нужно украсить кнопки</param>
    public static void InitFormImages(Form form)
    {
      // 28.02.2013
      // В процессе дизайна формы в Visual Studio картинки не инициализиоуем
      if (!AppWasInit)
        return;

#if DEBUG
      CheckMainThread();
#endif

      InitControlImages(form); // рекурсивная процедура
    }

    private static void InitControlImages(Control control)
    {
      if (control == null)
        return;

      if (control is Button)
      {
        string imageKey;
        switch (((Button)control).DialogResult)
        {
          case DialogResult.OK: imageKey = "Ok"; break;
          case DialogResult.Cancel:
            // Форма может иметь единственную кнопку закрытия с
            // DialogResult.Cancel. В этом случае рисуется значок от
            // кнопки <OK>
            if (control.FindForm().AcceptButton == (Button)control)
              imageKey = "Ok";
            else
              imageKey = "Cancel";
            break;
          case DialogResult.Yes: imageKey = "Yes"; break;
          case DialogResult.No: imageKey = "No"; break;
          default: return;
        }
        ((Button)control).ImageList = MainImages;
        ((Button)control).ImageKey = imageKey;
        ((Button)control).ImageAlign = ContentAlignment.MiddleLeft;
      }
      else
      {
        if (control.HasChildren)
        {
          foreach (Control childControl in control.Controls)
            InitControlImages(childControl); // рекурсивная процедура
        }
      }
    }

    #endregion

    #region GetFormIconImage()

    /// <summary>
    /// Возвращает изображение значка для формы как объект Image.
    /// Если значка нет, возращается пустое "изображение".
    /// Используется в диспетчере окон.
    /// В интерфейсе SDI для всех форм может использоваться один значок приложения.
    /// Этот метод возвращает изображение, предусмотренное в пользовательском коде, а не изображение стандартного значка.
    /// </summary>
    /// <param name="form">Форма</param>
    /// <returns>Изображение</returns>
    public static Image GetFormIconImage(Form form)
    {
      EFPFormProvider formProvider = EFPFormProvider.FindFormProvider(form);
      if (formProvider == null)
        return EFPApp.MainImages.Images["EmptyImage"];

      if (formProvider.FormIconImage == null)
        return EFPApp.MainImages.Images["EmptyImage"];
      else
        return formProvider.FormIconImage;
    }

    #endregion

    #endregion

    #region Контекстные меню

    /// <summary>
    /// Сюда может быть добавлен обработчик для инициализации расширенного контекстного
    /// меню для некоторых типов управляющих элементов
    /// </summary>
    public static event EFPControlCommandItemsNeededEventHandler ControlCommandItemsNeeded;

    internal static void OnControlCommandItemsNeeded(EFPControlBase controlProvider)
    {
      if (ControlCommandItemsNeeded == null)
        return;

      EFPControlCommandItemsNeededEventArgs args = new EFPControlCommandItemsNeededEventArgs(controlProvider);
      ControlCommandItemsNeeded(null, args);
    }

    #endregion

    #region Открытые формы и диалоги

    #region Создание формы

    /// <summary>
    /// Создает либо простую форму без ничего, либо форму с кнопками "ОК" и "Отмена"
    /// </summary>
    /// <param name="isOKCancelForm">true - создается форма с кнопками "ОК" и "Отмена". 
    /// false - создается форма без управляющих элементов</param>
    /// <param name="formProvider">Сюда помещается ссылка на провайдер формы</param>
    /// <param name="mainPanel">Cюда помещается ссылка на главную панель формы OKCancelForm или
    /// ссылка на саму пустую форму. В этот элемент должны добавляться другие управляющие элементы</param>
    /// <returns>Созданный объект Form</returns>
    public static Form CreateForm(bool isOKCancelForm, out EFPFormProvider formProvider, out Control mainPanel)
    {
      if (isOKCancelForm)
      {
        OKCancelForm form = new OKCancelForm();
        formProvider = form.FormProvider;
        mainPanel = form.MainPanel;
        return form;
      }
      else
      {
        Form form = new Form();
        formProvider = new EFPFormProvider(form);
        mainPanel = form;
        return form;
      }
    }

    /// <summary>
    /// Создает форму с закладками и, возможно, с кнопками "ОК" и "Отмена"
    /// </summary>
    /// <param name="isOKCancelForm">true - создается форма с кнопками "ОК" и "Отмена". 
    /// false - создается форма без управляющих элементов</param>
    /// <param name="formProvider">Сюда помещается ссылка на провайдер формы</param>
    /// <param name="theTabControl">Сюда помещается ссылка на созданный элемент TabControl</param>
    /// <returns>Созданный объект Form</returns>
    public static Form CreateTabControlForm(bool isOKCancelForm, out EFPFormProvider formProvider, out TabControl theTabControl)
    {
      Control mainPanel;
      Form form = CreateForm(isOKCancelForm, out formProvider, out mainPanel);
      theTabControl = new TabControl();
      theTabControl.Dock = DockStyle.Fill;
      theTabControl.ImageList = EFPApp.MainImages;
      mainPanel.Controls.Add(theTabControl);
      return form;
    }

    #endregion

    #region Немодальные формы

#if XXX
    /// <summary>
    /// Возвращает список открытых дочерних MDI-форм.
    /// </summary>
    [Obsolete("Это свойство является устаревшим и предназначено только для программ, поддерживающих единственное окно MDI. " +
       "В новых программах используйте метод Interface.GetChildForms()", false)]
    internal static Form[] MdiChildren
    {
      get
      {
#if DEBUG
        CheckMainThread();
#endif

        if (MainWindow == null)
          return new Form[0];
        else
          return MainWindow.MdiChildren;
      }
    }
#endif

    /// <summary>
    /// Выводит дочернюю форму.
    /// Вызывает EFPApp.Interface.ShowChildForm().
    /// Если EFPApp.Interface=null. выводит форму на рабочем столе Windows с помощью Form.Show()
    /// Это основной метод, наряду с ShowDialog(), который должен использоваться в пользовательском коде.
    /// </summary>
    /// <param name="form">Дочерняя форма</param>
    public static void ShowChildForm(Form form)
    {
#if DEBUG
      CheckMainThread();

      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");
      if (!DebugFormDispose.Exists(form))
        throw new ArgumentException("Форма \"" + form.ToString() + "\" не была зарегистрирована в списке", "form");
#endif

      if (Interface != null)
      {
        Interface.ShowChildForm(form);
      }
      else
      {
        form.Show();
      }
    }

#if XXX
    /// <summary>
    /// Вывод формы со встраиванием в интерфейс MDI
    /// Может быть добавлено несколько одинаковых форм.
    /// </summary>
    /// <param name="form">Добавляемая форма</param>
    [Obsolete("Этот метод является устаревшим. Используйте ShowChildForm().", false)]
    public static void ShowMdiChild(Form form)
    {
      ShowChildForm(form);
    }
#endif

#if XXX
    /// <summary>
    /// Вывод формы со встраиванием в интерфейс MDI
    /// Можно запретить добавление, если такая форма уже есть.
    /// </summary>
    /// <param name="form">Добавляемая форма</param>
    /// <param name="singleInstance">Если true, то просматривается список существующих MDI-окон. Если
    /// среди них есть форма с тем же GetType(), то она активируется</param>
    [Obsolete("Этот метод является устаревшим. Вместо этого метода с SingleInstance=true, " +
      "предпочтительнее использовать типизированный метод ShowSingleInstanceForm(). " +
      "В этом случае не требуется создания экземпляра формы, если она не будет выводится на экран", false)]
    public static void ShowMdiChild(Form form, bool singleInstance)
    {
#if DEBUG
      CheckMainThread();

      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");
#endif

      if (singleInstance && Interface != null)
      {
        Form[] Forms = Interface.GetChildForms(false);

        for (int i = 0; i < Forms.Length; i++)
        {
          if (Forms[i].GetType() == form.GetType())
          {
            Activate(Forms[i]);// 07.06.2021

            // Уничтожаем новую форму
            form.Dispose();
            return;
          }
        }
      }

      // Надо добавить форму
      ShowChildForm(form);
    }
#endif

#if XXX
    /// <summary>
    /// Делает попытку закрыть все открытые формы
    /// </summary>
    [Obsolete("Этот метод является устаревшим. Используйте Interface.CloseAllChildren()", false)]
    public static void CloseAllMdiChildren()
    {
#if DEBUG
      CheckMainThread();
#endif

#pragma warning disable 0618 // обход [Obsolete]
      Form[] Forms = EFPApp.MdiChildren; // 04.04.2012. Если не сохранить массив, то
#pragma warning restore 0618

      // оригинальный Forms будет укорачиваться в процессе удаления

      for (int i = 0; i < Forms.Length; i++)
      {
        Forms[i].Close();
        if (Forms[i].Visible)
          break;
      }
    }
#endif

#if XXX
    /// <summary>
    /// Найти MDI-форму заданного класса.
    /// Возвращает первую найденную форму или null
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>Найденная форма или null</returns>
    [Obsolete("Этот метод устарел. Используйте Interface.FindChildForm()", false)]
    public static Form FindMdiChild(Type formType)
    {
#if DEBUG
      CheckMainThread();
#endif

#pragma warning disable 0618 // обход [Obsolete]

      for (int i = 0; i < MdiChildren.Length; i++)
      {
        if (MdiChildren[i].GetType() == formType)
          return MdiChildren[i];
      }
      return null;

#pragma warning restore 0618
    }
#endif

    #if XXX
/// <summary>
    /// Найти и активировать форму заданного класса.
    /// Возвращает true в случае успеха
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>true, если форма найдена и активирована. false, если форма не найдена</returns>
    [Obsolete("Этот метод устарел. Используйте Interface.FindAndActivateChildForm()", false)]
    public static bool FindAndActivateMdiChild(Type formType)
    {
#if DEBUG
      CheckMainThread();
#endif

#pragma warning disable 0618 // обход [Obsolete]
      Form frm = FindMdiChild(formType);
#pragma warning restore 0618

      if (frm == null)
        return false;
      if (frm.WindowState == FormWindowState.Minimized)
        frm.WindowState = FormWindowState.Normal; // 21.09.2017
      frm.Activate();
      return true;
    }
#endif


#if XXX
    /// <summary>
    /// Выполнение команд меню "Окно"
    /// </summary>
    /// <param name="layout"></param>
    [Obsolete("Этот метод устарел. Упорядочение окон должно выполняться для определенного главного окна " +
      "вызовом Interface.CurrentMainWindowLayout.LayoutChildForms()", false)]
    public static void LayoutMdiChildren(MdiLayout layout)
    {
#if DEBUG
      CheckMainThread();
#endif

      EFPApp.MainWindow.LayoutMdi(layout);
    }
#endif

    #endregion

    #region Блоки диалога

    /// <summary>
    /// Активный диалог, запущенный с помощью ShowDialog()
    /// null - если диалогов нет.
    /// Доступ возможен только из основного потока приложения.
    /// </summary>
    public static Form ActiveDialog
    {
      get
      {
        CheckMainThread();

        if (_DialogStack.Count > 0)
        {
          Form frm = _DialogStack.Peek();
          if ((!frm.IsDisposed) && (frm.Visible) && 
            EFPFormProvider.FindFormProviderRequired(frm).VisibleCompleted) // 07.03.2022
            return _DialogStack.Peek();

          // 16.09.2021
          // Перебираем весь стек диалогов, если верхний уже не актуален
          Form[] a = _DialogStack.ToArray();
          for (int i = 0; i < a.Length; i++)
          {
            if ((!a[i].IsDisposed) && (a[i].Visible) &&
              EFPFormProvider.FindFormProviderRequired(a[i]).VisibleCompleted) // 07.03.2022
              return a[i];
          }
          return null;
        }
        else
          return null;
      }
    }

    /// <summary>
    /// Стек диалоговых окон, выведенных ShowDialog().
    /// Нулевой элемент массива соответствует ActiveDialog.
    /// Возвращает пустой массив, если нет активного диалога.
    /// Доступ возможен только из основного потока приложения.
    /// </summary>
    /// <returns>Массив форм</returns>
    public static Form[] GetDialogStack()
    {
      CheckMainThread();

      return _DialogStack.ToArray();
    }

    private static readonly Stack<Form> _DialogStack = new Stack<Form>();

    /// <summary>
    /// Вывод формы в модальном режиме.
    /// После показа формы она может быть разрушена при установке соответствующего
    /// флага.
    /// Размеры диалога могут быть уменьшены, если они превышают размеры экрана.
    /// Выполняется центирование диалога. Используйте перегрузку с дополнительным аргументом,
    /// если размеры и положение формы диалога были установлены до вызова ShowDialog().
    /// </summary>
    /// <param name="form">Форма для показа</param>
    /// <param name="dispose">При значении true форма будет разрушена</param>
    /// <returns>Результат выполнения блока диалога</returns>
    public static DialogResult ShowDialog(Form form, bool dispose)
    {
      return ShowDialog(form, dispose, true);
    }

    /// <summary>
    /// Если установлено в true, то будет игнорироваться позиция для вывода блока диалога, задаваемая с помощью EFPDialogPosition.
    /// По умолчанию - false - позиция учитывается.
    /// Позиция обычно задается для диалогов, относящихся к выпадающим спискам.
    /// </summary>
    public static bool IgnoreDialogPosition { get { return _IgnoreDialogPosition; } set { _IgnoreDialogPosition = value; } }
    private static bool _IgnoreDialogPosition = false;

    /// <summary>
    /// Нужно ли сохранять положения блоков диалога (и форм) между сеансами работы программы.
    /// По умолчанию - true - выполняется сохранение в секции "FormBounds".
    /// Сбрасывается в false в режиме упрощенного интерфейса
    /// </summary>
    public static bool SaveFormBounds { get { return _SaveFormBounds; } set { _SaveFormBounds = value; } }
    private static bool _SaveFormBounds = true;

    /// <summary>
    /// Вывод формы в модальном режиме.
    /// После показа формы она может быть разрушена при установке флага <paramref name="dispose"/>=true.
    /// </summary>
    /// <param name="form">Форма для показа</param>
    /// <param name="dispose">При значении true форма будет разрушена</param>
    /// <param name="position">Правила позиционирования блока диалога.
    /// Если null, то выполняется центрирование диалога на экране. См. свойство EFPApp.DefaultScreen.
    /// Если свойство EFPApp.IgnoreDialogPosition=true, то аргумент игнорируется</param>
    /// <returns>Результат выполнения блока диалога</returns>
    public static DialogResult ShowDialog(Form form, bool dispose, EFPDialogPosition position)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);

      bool centerInScreen = true;
      if (position != null && (!IgnoreDialogPosition))
      {
        if ((!position.PopupOwnerBounds.IsEmpty) || position.PopupOwnerControl != null)
        {
          formProvider.DialogPosition = position;
          centerInScreen = false;
        }
      }
      return ShowDialog(form, dispose, centerInScreen);
    }

    /// <summary>
    /// Вывод формы в модальном режиме.
    /// После показа формы она может быть разрушена при установке флага <paramref name="dispose"/>=true.
    /// </summary>
    /// <param name="form">Форма для показа</param>
    /// <param name="dispose">При значении true форма будет разрушена</param>
    /// <param name="centerInScreen">Если true, то диалог будет центрирован относительно экрана.
    /// Если false, то координаты диалога должны быть правильно заданы до вызова метода</param>
    /// <returns>Результат выполнения блока диалога</returns>
    public static DialogResult ShowDialog(Form form, bool dispose, bool centerInScreen)
    {
#if DEBUG
      CheckMainThread();

      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");
      if (!DebugFormDispose.Exists(form))
        throw new ArgumentException("Форма \"" + form.ToString() + "\" не была зарегистрирована в списке", "form");
#endif

      if (ExternalDialogOwnerWindow == null)
        Activate(); // 03.06.2015

      //Form.ShowInTaskbar = false;
      //Form.ShowInTaskbar = !EFPApp.MainWindowVisible; // 26.01.2014
      InitShowInTaskBar(form); //  06.10.2014

      //Form.StartPosition = FormStartPosition.CenterScreen;
      // 12.03.2016
      //PlaceFormInScreenCenter(Form);

      // 14.11.2013
      // Подгоняем размеры формы
      //if (Form.FormBorderStyle == FormBorderStyle.Sizable || Form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
      //{
      //  if (Form.WindowState != FormWindowState.Maximized)
      //  {
      //    Size MaxSize = EFPApp.DefaultScreen.WorkingArea.Size;
      //    if (Form.Size.Width > MaxSize.Width)
      //      Form.Size = new Size(MaxSize.Width, Form.Size.Width);
      //    if (Form.Size.Height > MaxSize.Height)
      //      Form.Size = new Size(Form.Size.Width, MaxSize.Height);

      //    //WinFormsTools.PlaceFormInCenter(Form);
      //  }
      //}
      //

      try
      {

        // 12.03.2020
        // При выводе диалога с незаданными размерами, делаем его побольше, а не 300x300
        if (form.StartPosition == FormStartPosition.WindowsDefaultBounds)
        {
          form.Size = new Size((DefaultScreen.Bounds.Width * 2) / 3, (DefaultScreen.Bounds.Height * 2) / 3);
          form.StartPosition = FormStartPosition.WindowsDefaultLocation;
        }

        // 11.06.2021. Должно быть после обработки WindowsDefaultBounds, а не до,
        // иначе увеличение размеров не сработает
        if (centerInScreen)
          form.StartPosition = FormStartPosition.CenterParent;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка определения позиции блока диалога");
      }

      IWin32Window currDialogOwnerWindow = EFPApp.DialogOwnerWindow; // запоминаем до очистки ExternalDialogOwnerWindow

      DialogRunner runner = new DialogRunner();
      DialogResult res;
      runner.Attach(form);
      runner.PrevExternalDialogOwnerWindowSetFlag = _ExternalDialogOwnerWindowSetFlag;
      _ExternalDialogOwnerWindowSetFlag = false;


      Form lastActiveForm = Form.ActiveForm;

      // Отцепляем и прячем окна типа калькулятора
      Form lastToolFormOwner = null;
      try
      {
        foreach (Form toolForm in ToolFormsForDialogs)
        {
          if (lastToolFormOwner == null)
            lastToolFormOwner = toolForm.Owner;
          toolForm.Visible = false;
          toolForm.Owner = null;
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Выключение tool forms"); // без вывода диалога
      }

      // 20.08.2020
      // Стеком диалогов занимается объект DialogRunner в ответ на событие Form.VisibleChanged.
      // Если добавить форму в стек заранее, то при обработке событий OnShown форма будет уже как бы в стеке,
      // хотя она еще и не видна. Если используется EFPApp.BeginWait(), то заставка спозиционируется в неправильном месте
      // экрана.
#if ACTIVEDIALOG_OLD
      _DialogStack.Push(form);
#endif
      try
      {
        if (IsMainThread && _ExecProcList != null)
          _ExecProcList.ProcessAll();

        EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);
        formProvider.InternalSetVisible(true); // 17.05.2021 - для уменьшения мерцания

        //Form.ShowDialog();
        try
        {
          form.ShowDialog(currDialogOwnerWindow); // 05.04.2014
        }
        catch (Exception e)
        {
          if (CanRepeatShowDialogAfterError(e))
          {
            form.ShowDialog(); // 12.03.2018
            runner.PrevExternalDialogOwnerWindowSetFlag = false; // чтобы ExternalDialogOwnerWindow не восстанавливалось
          }
          else
            throw;
        }
        res = form.DialogResult;
        if (!form.IsDisposed) // 28.06.2016 Окно могло быть разрушено, если приложение завершилось
          DefaultScreen = Screen.FromControl(form); // пользователь мог переместить диалог
      }
      finally
      {
        try
        {
          runner.Detach(form);
        }
        catch (Exception e) // 13.04.2018
        {
          EFPApp.ShowException(e, "Ошибка отсоединения DialogRunner");
        }

        // 20.08.2020
        // Если все прошло нормально, то формы уже нет в стеке диалогов
#if ACTIVEDIALOG_OLD
        _DialogStack.Pop();
#else
        // Это - на всякий случай:
        //if (Object.ReferenceEquals(ActiveDialog, form))
        // 20.09.2021 - ActiveDialog не вернет текущий диалог, т.к. он уже закрыт
        if (_DialogStack.Count > 0) // 21.10.2021 - Может быть, что стек уже пуст
        {
          if (Object.ReferenceEquals(_DialogStack.Peek(), form)) 
            _DialogStack.Pop();
        }
#endif

        _ExternalDialogOwnerWindowSetFlag = runner.PrevExternalDialogOwnerWindowSetFlag;
        if (dispose)
          form.Dispose();

        // Включаем обратно окна
        try
        {
          foreach (Form toolForm in ToolFormsForDialogs)
          {
            if (lastToolFormOwner == null)
              toolForm.Owner = MainWindow;
            else
              //ToolForm.Owner = FDialogStack.Peek();
              // 14.03.2010
              // Если предыдущий блок диалога в стеке был вызван без EFPFormProvider,
              // то его нельзя делать владельцем для панелек. Иначе когда этот диалог
              // тоже будет закрыт, панельки будут уничтожены
              toolForm.Owner = lastToolFormOwner;
            toolForm.Visible = true;
            if (toolForm.Owner != null) // 27.12.2020 вдруг MainWindow=null?
              toolForm.Owner.Activate(); // иначе будет активной форма инструментов
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Включение tool forms"); // без вывода диалога
        }

        //if (ExternalDialogOwnerWindow == null)
        //{
        //  if (LastActiveForm != null && LastActiveForm.Visible)
        //    LastActiveForm.Activate();
        //}

        if (currDialogOwnerWindow is Form)
          ((Form)(currDialogOwnerWindow)).Activate(); // 14.03.2017
        else if (lastActiveForm != null && lastActiveForm.Visible)
          lastActiveForm.Activate();

        if (IsMainThread && _ExecProcList != null)
        {
          Application.DoEvents(); // 16.12.2019
          _ExecProcList.ProcessAll();
        }
      } // finally
      return res;
    }

    /// <summary>
    /// Проверяет исключение, возникшее при выводе диалога с помощью метода Form.ShowDialog(IWin32Window).
    /// Возвращает true, если ошибка могла произойти из-за какого-либо сбоя во "внешнем" окне-владельце.
    /// При этом вывод диалога должен быть повторен.
    /// </summary>
    /// <param name="e">Объект перехваченного исключения</param>
    /// <returns>true, если вывод диалога следует повторить</returns>
    internal static bool CanRepeatShowDialogAfterError(Exception e)
    {
      if (ExternalDialogOwnerWindow == null)
        return false;

      // Не знаю, как правильно эта константа называется
      const int ERROR_HANDLE = unchecked((int)0x80004005); // Error creating window handle

      Win32Exception we = (e as Win32Exception);
      if (we != null)
      {
        if (we.ErrorCode == ERROR_HANDLE)
        {
          e.Data["EFPApp.CanRepeatShowDialogAfterError.Message"] = "Внешнее окно-владелец EFPApp.ExternalDialogOwnerWindow=" + ExternalDialogOwnerWindow.Handle.ToString() + ", возможно вызвало ошибку вывода диалога. Свойство отключено";
          LogoutTools.LogoutException(e, "EFPApp.CanRepeatShowDialogAfterError");

          ExternalDialogOwnerWindow = null;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Вложенный класс, используемый внутри метода ShowDialog. Нужен для
    /// временного подключения / отключения обработчика KeyDown
    /// Обработчик нужен для перехвата нажатия клавиш, относящихся к локальному меню
    /// формы
    /// </summary>
    private class DialogRunner
    {
      #region Конструктор

      public DialogRunner()
      {
#if !ACTIVEDIALOG_OLD
        _EHVisibleChanged = new EventHandler(Form_VisibleChanged);
#endif
      }

      #endregion

      #region Методы

      public void Attach(Form form)
      {
        _PrevItems = EFPCommandItems.GetFocusedObjects();
        for (int i = 0; i < _PrevItems.Length; i++)
        {
          EFPControlCommandItems cis = _PrevItems[i] as EFPControlCommandItems;
          if (cis != null)
          {
            if (!cis.IsDisposed) // проверка добавлена 13.04.2018
              cis.Active = false;
          }
        }

#if !ACTIVEDIALOG_OLD
        form.VisibleChanged += _EHVisibleChanged;
#endif
      }

      public void Detach(Form form)
      {
#if !ACTIVEDIALOG_OLD
        form.VisibleChanged -= _EHVisibleChanged;
#endif

        for (int i = 0; i < _PrevItems.Length; i++)
        {
          EFPControlCommandItems cis = _PrevItems[i] as EFPControlCommandItems;
          if (cis != null)
          {
            if (!cis.IsDisposed) // проверка добавлена 13.04.2018
              cis.Active = true;
          }
        }
      }

      #endregion

      #region Внутренние поля

      /// <summary>
      /// Массив наборов команд, которые были активны на момент вызова диалога.
      /// На время работы диалога все команды отключаются, а, затем, восстанавливаются
      /// </summary>
      private EFPCommandItems[] _PrevItems;

      #endregion

      #region Обработка свойства Visible

#if !ACTIVEDIALOG_OLD
      private void Form_VisibleChanged(object sender, EventArgs args)
      {
        // 20.08.2020
        // Стек активных диалогов меняется здесь, а не в ShowDialog()

        Form form = (Form)sender;

        if (form.Visible)
        {
          if (!Object.ReferenceEquals(EFPApp.ActiveDialog, form))
            EFPApp._DialogStack.Push(form);
        }
        else
        {
          if (Object.ReferenceEquals(EFPApp.ActiveDialog, form))
            EFPApp._DialogStack.Pop();
        }
      }

      private readonly EventHandler _EHVisibleChanged;
#endif

      #endregion

      #region Дополнительные поля

      /// <summary>
      /// Сохранение свойства EFPApp.ExternalDialogOwnerWindowSetFlag на время вызова диалога
      /// </summary>
      public bool PrevExternalDialogOwnerWindowSetFlag;

      #endregion
    }

    #endregion

    #region Выборочный вызов модального или немодального окна

    /// <summary>
    /// Вывод формы в модальном или немодальном режиме в зависимости от наличия
    /// уже запущенных диалогов. Вызывает, соотетственно, метод ShowDialog() или
    /// ShowForm(). При закрытии формы всегда вызывается Dispose().
    /// Функция возвращает управление немедленно в немодальном режиме. 
    /// В модальном режиме ожидается завершение диалога
    /// </summary>
    /// <param name="form">Выводимая форма</param>
    public static void ShowFormOrDialog(Form form)
    {
      if (ActiveDialog == null &&
        MainWindow != null) // 09.06.2015

        ShowChildForm(form);
      else
        ShowDialog(form, true);
    }

    /// <summary>
    /// Найти и активировать форму для заданного класса формы.
    /// Если есть активный блок диалога, то он возвращается, если имеет соответствующий тип
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <returns>Найденная форма или null</returns>
    public static TForm FindAndActivate<TForm>()
      where TForm : Form
    {
#if DEBUG
      CheckMainThread();
#endif

      if (ActiveDialog != null)
      {
        if (ActiveDialog is TForm)
        {
          Activate();
          return (TForm)ActiveDialog;
        }
        else
          return null;
      }

      if (EFPApp.Interface != null)
      {
        TForm form = EFPApp.Interface.FindChildForm<TForm>();
        if (form != null)
        {
          Activate(form); // 07.06.2021
          return form;
        }
      }

      return null;
    }

    /// <summary>
    /// Открытие или активация формы, которая должна показываться только в одном экземпляре.
    /// Если в списке открытых mdi-окон есть форма данного класса, то она активируется.
    /// Иначе создается новый экземпляр класса формы и выводится как mdi-окно.
    /// Есть есть активный блок диалога, то поиск не выполняется, создается экземпляр формы, который выводится в модельном режиме.
    /// </summary>
    /// <typeparam name="TForm">Класс, производный от Form и имеющий конструктор по умолчанию</typeparam>
    /// <remarks>
    /// В пользовательском коде не требуется код, проверяющий наличие формы и статический ссылки на экземпляр класса.
    /// Метод может применяться, например, для форм мониторинга ресурсов, многократное открытие которых нежелательно.
    /// </remarks>
    public static TForm ShowSingleInstanceForm<TForm>()
      where TForm : Form, new()
    {
      TForm frm = FindAndActivate<TForm>();
      if (frm == null)
      {
        frm = new TForm();
        EFPFormProvider.FindFormProviderRequired(frm); // для проверки

        ShowFormOrDialog(frm);
      }
      return frm;
    }

    #endregion

    #region Активный управляющий элемент

    /// <summary>
    /// Активный управляющий элемент в активной форме
    /// Установка свойства сначала активирует форму, на которой расположен элемент
    /// </summary>
    public static Control CurrentControl
    {
      get
      {
        Form frm = Form.ActiveForm;
        if (frm == null)
          return null;
        return frm.ActiveControl;
      }
      set
      {
        if (value == null)
          return;
        Form frm = value.FindForm();
        frm.Select();
        // так может не работать
        //frm.ActiveControl = value;
      }
    }

    /// <summary>
    /// Делает активной указанную форму.
    /// Если она свернута, то она разворачивается.
    /// Если есть несколько главных MDI-окон, активируется нужное.
    /// Если форма не выведена на экран, никаких действий не выполняется
    /// </summary>
    /// <param name="form">Форма, которую требуется активировать.</param>
    public static void Activate(Form form)
    {
      if (form == null)
        return;
      if (!form.Visible)
        return;

      if (Interface == null || form.Modal)
      {
        form.Activate();
        return;
      }

      // 09.07.2021
      if (!Interface.IsSDI)
      {
        EFPAppMainWindowLayout mw = Interface.FindMainWindowLayout(form);
        if (mw != null)
        {
          if (mw.MainWindow.WindowState == FormWindowState.Minimized)
            mw.MainWindow.WindowState = mw.WindowStateBeforeMinimized;
        }
      }

      if (form.WindowState == FormWindowState.Minimized)
      {
        EFPFormProvider fp = EFPFormProvider.FindFormProvider(form); // не будем требовать обязательного наличия провайдера
        if (fp == null)
          form.WindowState = FormWindowState.Normal; // 21.09.2017
        else
          form.WindowState = fp.WindowStateBeforeMinimized;
      }

      form.Select(); // Так нормально работает
    }

    /// <summary>
    /// Возвращает true, если указанное дочернее окно свернуто, или свернуто соответствующее главное окно
    /// </summary>
    /// <param name="form">Дочернее окно</param>
    /// <returns>Признак свернутости</returns>
    public static bool IsMinimized(Form form)
    {
      if (form == null)
        return false;
      if (form.WindowState == FormWindowState.Minimized)
        return true;
      if (Interface == null)
        return false;
      if (Interface.IsSDI)
        return false;
      EFPAppMainWindowLayout mw = Interface.FindMainWindowLayout(form);
      if (mw == null)
        return false;
      if (mw.MainWindow.WindowState == FormWindowState.Minimized)
        return true;
      return false;
    }

    #endregion

    #endregion

    #region Добавление кнопок

    /// <summary>
    /// Добавление кнопки к панели
    /// Кнопки добавляются вертикально или горизонтально, в зависимости от свойства
    /// Parent.Dock. При горизонтальном размещении кнопки могут быть разной ширины
    /// (равной 88, 132 или 176 символов, в зависимости от длины текста)
    /// Предполагается, что на панели, кроме кнопок, ничего не будет.
    /// При первом вызове устанавливается высота или ширина панели
    /// </summary>
    /// <param name="parentPanel">Панель для размещения кнопок</param>
    /// <param name="text">Заголовок</param>
    /// <param name="imageKey">Изображение в списке MainImages</param>
    /// <returns>Объект кнопки</returns>
    public static Button AddButton(Panel parentPanel, string text, string imageKey)
    {
#if DEBUG
      CheckMainThread();
#endif

      bool isVertical = parentPanel.Dock == DockStyle.Left || parentPanel.Dock == DockStyle.Right;

      Button btn = new Button();
      btn.Text = text;
      if (!String.IsNullOrEmpty(imageKey))
        btn.Image = EFPApp.MainImages.Images[imageKey];
      btn.ImageAlign = ContentAlignment.MiddleLeft;

      if (parentPanel.Controls.Count == 0)
      {
        if (isVertical)
          parentPanel.Width = 88 + 16;
        else
          parentPanel.Height = 24 + 16;

        btn.Location = new Point(8, 8);
      }
      else
      {
        Control last = parentPanel.Controls[parentPanel.Controls.Count - 1];
        if (isVertical)
          btn.Location = new Point(8, last.Top + last.Height + 8);
        else
          btn.Location = new Point(last.Left + last.Width + 8);
      }

      int w = 88;
      if (!isVertical)
      {
        int n = WinFormsTools.RemoveMnemonic(text).Length;
        if (n > 7)
          w = n > 15 ? 176 : 132;
      }
      btn.Size = new Size(w, 24);

      btn.TabIndex = parentPanel.Controls.Count;
      parentPanel.Controls.Add(btn);
      return btn;
    }

    /// <summary>
    /// Добавление стандартной кнопки.
    /// У кнопки устанавливается свойство DialogResult
    /// </summary>
    /// <param name="parentPanel">Панель для добавления кнопки</param>
    /// <param name="buttonKind">Какая кнопка добавляется</param>
    /// <returns>Созданная кнопка</returns>
    public static Button AddButton(Panel parentPanel, DialogResult buttonKind)
    {
#if DEBUG
      CheckMainThread();
#endif

      Button btn;
      switch (buttonKind)
      {
        case DialogResult.OK:
          btn = AddButton(parentPanel, "О&К", "OK");
          break;
        case DialogResult.Cancel:
          btn = AddButton(parentPanel, "Отмена", "Cancel");
          break;
        case DialogResult.Yes:
          btn = AddButton(parentPanel, "&Да", "Yes");
          break;
        case DialogResult.No:
          btn = AddButton(parentPanel, "&Нет", "No");
          break;
        case DialogResult.Abort:
          btn = AddButton(parentPanel, "Прервать", null);
          break;
        case DialogResult.Retry:
          btn = AddButton(parentPanel, "Повторить", null);
          break;
        case DialogResult.Ignore:
          btn = AddButton(parentPanel, "Пропустить", null);
          break;
        default:
          throw new ArgumentException("Неизвестная кнопка " + buttonKind.ToString(), "buttonKind");
      }
      btn.DialogResult = buttonKind;
      return btn;
    }

    /// <summary>
    /// Добавление стандартных кнопок к панели
    /// У кнопок устанавливается свойство DialogResult
    /// </summary>
    /// <param name="parentPanel">Панель для добавления кнопок</param>
    /// <param name="buttons">Добавляемые кнопки</param>
    /// <returns>Массив созданных объектов Button</returns>
    public static Button[] AddButtons(Panel parentPanel, MessageBoxButtons buttons)
    {
#if DEBUG
      CheckMainThread();
#endif

      Form frm = parentPanel.FindForm();
      Button[] btns;
      switch (buttons)
      {
        case MessageBoxButtons.OK:
          btns = new Button[1];
          btns[0] = AddButton(parentPanel, DialogResult.OK);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            frm.CancelButton = btns[0];
          }
          break;
        case MessageBoxButtons.OKCancel:
          btns = new Button[2];
          btns[0] = AddButton(parentPanel, DialogResult.OK);
          btns[1] = AddButton(parentPanel, DialogResult.Cancel);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            frm.CancelButton = btns[1];
          }
          break;
        case MessageBoxButtons.YesNo:
          btns = new Button[2];
          btns[0] = AddButton(parentPanel, DialogResult.Yes);
          btns[1] = AddButton(parentPanel, DialogResult.No);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            //frm.CancelButton = btns[1];
          }
          break;
        case MessageBoxButtons.YesNoCancel:
          btns = new Button[3];
          btns[0] = AddButton(parentPanel, DialogResult.Yes);
          btns[1] = AddButton(parentPanel, DialogResult.No);
          btns[2] = AddButton(parentPanel, DialogResult.Cancel);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            frm.CancelButton = btns[2];
          }
          break;
        case MessageBoxButtons.RetryCancel:
          btns = new Button[2];
          btns[0] = AddButton(parentPanel, DialogResult.Retry);
          btns[1] = AddButton(parentPanel, DialogResult.Cancel);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            frm.CancelButton = btns[1];
          }
          break;
        case MessageBoxButtons.AbortRetryIgnore:
          btns = new Button[3];
          btns[0] = AddButton(parentPanel, DialogResult.Abort);
          btns[1] = AddButton(parentPanel, DialogResult.Retry);
          btns[2] = AddButton(parentPanel, DialogResult.Ignore);
          if (frm != null)
          {
            frm.AcceptButton = btns[0];
            //frm.CancelButton = btns[2];
          }
          break;
        default:
          throw new ArgumentException("Неизвестный набор кнопок " + buttons.ToString(), "buttons");
      }
      return btns;
    }

    #endregion

    #region Размеры формы

    /// <summary>
    /// Установить размеры формы в процентном соотношении от размеров экрана
    /// </summary>
    /// <param name="form">Форма</param>
    /// <param name="scrPercentWidth">Относительная ширина формы в процентах</param>
    /// <param name="scrPercentHeight">Относительная высота формы в процентах</param>
    public static void SetFormSize(Form form, int scrPercentWidth, int scrPercentHeight)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      if (scrPercentWidth < 1 || scrPercentWidth > 100)
        throw new ArgumentOutOfRangeException("scrPercentWidth");
      if (scrPercentHeight < 1 || scrPercentHeight > 100)
        throw new ArgumentOutOfRangeException("scrPercentHeight");

      Screen scr = Screen.PrimaryScreen;
      form.Size = new Size(scr.Bounds.Width * scrPercentWidth / 100,
        scr.Bounds.Height * scrPercentHeight / 100);
    }

    #endregion

    #region Диалог списка открытых окон

    /// <summary>
    /// Выводит диалог для выбора активного окна пользовательского интерфейса
    /// </summary>
    public static void ShowChildFormListDialog()
    {
      ShowChildFormListDialog(false);
    }

    /// <summary>
    /// Выводит диалог для выбора активного окна пользовательского интерфейса
    /// </summary>
    /// <param name="debugShowHWND">Если True, то в списке будут выведены дескрипоторы окон для отладки</param>
    public static void ShowChildFormListDialog(bool debugShowHWND)
    {
      if (EFPApp.Interface == null)
      {
        EFPApp.ErrorMessageBox("Нет подключенного пользовательского интерфейса");
        return;
      }

      Form[] forms = EFPApp.Interface.GetChildForms(false);
      Form curr = EFPApp.Interface.CurrentChildForm;
#if XXX
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = "Выбрать окно";
      dlg.ImageKey="WindowList";
      dlg.ListTitle = "Выберите окно для активации";
      dlg.Items = new string[Forms.Length];
      for (int i = 0; i < Forms.Length; i++)
      {
        dlg.Items[i] = (i + 1).ToString() + ". " + Forms[i].Text;
        if (Object.ReferenceEquals(Forms[i], Curr))
          dlg.SelectedIndex = i;
      }

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      Forms[dlg.SelectedIndex].Activate();
#endif

      using (OKCancelGridForm form = new OKCancelGridForm())
      {
        //SetFormSize(Form, 50, 50);
        form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        form.Text = "Выбрать окно";
        form.Icon = EFPApp.MainImageIcon("WindowList");
        form.FormProvider.OwnStatusBar = true;
        form.FormProvider.ConfigSectionName = "ChildFormListDialog";

        //bool ShowMainWindowNo = false;
        //if (!EFPApp.Interface.IsSDI)
        //  ShowMainWindowNo = EFPApp.Interface.MainWindowCount > 1;

        EFPDataGridView gh = new EFPDataGridView(form.ControlWithToolBar);
        gh.Control.AutoGenerateColumns = false;
        if (EFPApp.ShowListImages)
          gh.Columns.AddImage("Image");
        gh.Columns.AddInt("Order", false, String.Empty, 3);
        gh.Columns.LastAdded.CanIncSearch = true;
        gh.Columns.AddTextFill("Text", false, String.Empty, 100, 20);
        gh.Columns.LastAdded.CanIncSearch = true;

        bool useNumberText = EFPApp.MainWindowNumberUsed;
        if (useNumberText)
          gh.Columns.AddText("MainWindow", false, String.Empty, 3, 2);

        if (EFPApp.ShowListImages)
          gh.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ChildFormList_GetCellAttributes);
        gh.DisableOrdering();
        gh.Control.ColumnHeadersVisible = false;
        gh.ReadOnly = true;
        gh.Control.ReadOnly = true;
        gh.CanView = false;
        gh.CommandItems.EnterAsOk = true;
        gh.Control.RowCount = forms.Length;
        for (int i = 0; i < forms.Length; i++)
        {
          gh.Control.Rows[i].Tag = forms[i];
          gh.Control["Order", i].Value = i + 1;
          string txt = forms[i].Text;
          if (EFPApp.IsMinimized(forms[i]))
            txt += " (свернуто)";
          if (debugShowHWND)
            txt += " (HWND=" + forms[i].Handle.ToString() + ")";
          gh.Control["Text", i].Value = txt;
          if (Object.ReferenceEquals(forms[i], curr))
            gh.CurrentRowIndex = i;
          if (useNumberText)
            gh.Control["MainWindow", i].Value = EFPApp.GetMainWindowNumberText(forms[i]);
        }

        gh.CurrentColumnName = "Text";

        if (EFPApp.ShowDialog(form, false, true) == DialogResult.OK)
        {
          if (gh.CurrentRowIndex >= 0)
            Activate(forms[gh.CurrentRowIndex]); // 07.06.2021
        }
      }
    }

    static void ChildFormList_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      EFPDataGridView gh = (EFPDataGridView)sender;
      if (args.RowIndex < 0)
        return;


      Form form = (Form)(gh.Control.Rows[args.RowIndex].Tag);

      switch (args.ColumnName)
      {
        case "Image":
          args.Value = EFPApp.GetFormIconImage(form);
          break;
        //case "Order":
        //  Args.Value = Args.RowIndex + 1;
        //  break;
        //case "Text":
        //  Args.Value = Form.Text;
        //  break;
      }
    }

    #endregion

    #region Выдача сообщений

    /// <summary>
    /// Текущий контекст справки при вызове MessageBox
    /// </summary>
    internal static string CurrentHelpContext;

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания всех параметров
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <param name="icon">Значок в левой части диалога</param>
    /// <param name="defaultButton">Определяет, какая из кнопок, определяемых параметром <paramref name="buttons"/>
    /// будет активирована по умолчанию. Обычно активируется первая кнопка,
    /// но в некоторых случаях удобнее активировать другую кнопку.
    /// Допустимые значения параметра зависят от <paramref name="buttons"/>.</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, String helpContext)
    {
      if (ExternalDialogOwnerWindow == null) // 22.12.2016
        Activate(); // 03.06.2015

      bool suspenIdleRequired = EFPApp.InsideShowException;

      DialogResult res;
      CurrentHelpContext = helpContext;
      if (suspenIdleRequired)
        EFPApp.SuspendIdle(); // 18.08.2021
      try
      {
        try
        {
          res = DoShowMessageBox(text, caption, buttons, icon, defaultButton, helpContext);
        }
        catch (Exception e) // 12.03.2018
        {
          if (CanRepeatShowDialogAfterError(e))
            res = DoShowMessageBox(text, caption, buttons, icon, defaultButton, helpContext);
          else
            throw;
        }
      }
      finally
      {
        CurrentHelpContext = null;
        if (suspenIdleRequired)
          EFPApp.ResumeIdle();
      }

      return res;
    }

    /// <summary>
    /// Форма-"подложка" для окна MessageBox.
    /// Если при вызове стандартного метода MessageBox.Show() передается контекст справки,
    /// то он обрабатывается Net Framework не так, как для обычных форм.
    /// Родительское окно сразу вызывает Help.ShowHelp() (который ожидает путь к CHM-файлу,
    /// а не наш контекст).
    /// Нельзя перехватить событие WM_HELP неизвестно в каком окне, которое окажется активным 
    /// на момент вызова.
    /// Вместо этого создается маленькая форма-подложка, которая предсказуемо обработает событие WM_HELP,
    /// вызвав EFPApp.ShowHelp().
    /// 
    /// Если EFPApp.MessageBox вызывается без контекста справки, то "подложка" не нужна
    /// </summary>
    private class MessageBoxBaseForm : Form
    {
      #region Обработка WM_HELP

      public string HelpContext;

      //protected override void OnHelpButtonClicked(CancelEventArgs Args)
      //{
      //  Args.Cancel = true;
      //  ShowHelp(HelpContext);
      //}

      //protected override void OnHelpRequested(HelpEventArgs hevent)
      //{
      //  hevent.Handled = true;
      //  ShowHelp(HelpContext);
      //}

      const int WM_HELP = 0x0053;

      protected override void WndProc(ref Message m)
      {
        if (m.Msg == WM_HELP)
        {
          EFPApp.ShowHelp(HelpContext);
        }
        else
          base.WndProc(ref m);
      }

      #endregion
    }

    /// <summary>
    /// Количество выведенных в данный момент сообщений MessageBox.
    /// Так как MessageBox() может вызываться асинхронно, свойство может возвращать значение, большее 1
    /// </summary>
    public static int MessageBoxCount { get { return _MessageBoxCount; } }
    private static int _MessageBoxCount;

    private static DialogResult DoShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, String helpContext)
    {
      DialogResult res;
      try
      {
        Interlocked.Increment(ref _MessageBoxCount);
        if (IsMainThread && _ExecProcList != null)
          _ExecProcList.ProcessAll();
        res = DoShowMessageBox2(text, caption, buttons, icon, defaultButton, helpContext);
      }
      finally
      {
        Interlocked.Decrement(ref _MessageBoxCount);
        if (IsMainThread && _ExecProcList != null)
          _ExecProcList.ProcessAll();
      }
      return res;
    }

    private static DialogResult DoShowMessageBox2(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, String helpContext)
    {
      DialogResult res;
      if (String.IsNullOrEmpty(helpContext))
        res = System.Windows.Forms.MessageBox.Show(/*MainWindow 22.12.2016 */ DialogOwnerWindow,
          text, caption, buttons, icon, defaultButton);
      else
      {
        //res = System.Windows.Forms.MessageBox.Show(/*MainWindow 22.12.2016 */DialogOwnerWindow,
        //  Text, Caption, Buttons, Icon, DefaultButton, (MessageBoxOptions)0, HelpContext);

        // 28.11.2018

        MessageBoxBaseForm baseForm = new MessageBoxBaseForm();
        try
        {
          baseForm.Text = String.Empty;
          baseForm.FormBorderStyle = FormBorderStyle.None;
          baseForm.Size = new Size(1, 1);
          baseForm.StartPosition = FormStartPosition.CenterScreen;
          baseForm.HelpContext = helpContext;
          EFPApp.ShowFormInternal(baseForm);
          res = System.Windows.Forms.MessageBox.Show(baseForm,
            text, caption, buttons, icon, defaultButton, (MessageBoxOptions)0, helpContext);
          baseForm.Hide();
        }
        finally
        {
          baseForm.Dispose();
        }
      }
      return res;
    }

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания большинства параметров
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <param name="icon">Значок в левой части диалога</param>
    /// <param name="defaultButton">Определяет, какая из кнопок, определяемых параметром <paramref name="buttons"/>
    /// будет активирована по умолчанию. Обычно активируется первая кнопка,
    /// но в некоторых случаях удобнее активировать другую кнопку.
    /// Допустимые значения параметра зависят от <paramref name="buttons"/>.</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
    {
      return MessageBox(text, caption, buttons, icon, defaultButton, String.Empty);
    }

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания большинства параметров
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <param name="icon">Значок в левой части диалога</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      return MessageBox(text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания большинства параметров
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <param name="icon">Значок в левой части диалога</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons, MessageBoxIcon icon, string helpContext)
    {
      return MessageBox(text, caption, buttons, icon, MessageBoxDefaultButton.Button1, helpContext);
    }

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания набора кнопок
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons)
    {
      return MessageBox(text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Выводит диалог сообщения с возможностью задания набора кнопок
    /// и контекста справки.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="buttons">Задает набор кнопок, которые можно нажать.
    /// Определяет возможные результаты, возвращаемые MessageBox</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string caption,
      MessageBoxButtons buttons, string helpContext)
    {
      return MessageBox(text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, helpContext);
    }

    /// <summary>
    /// Выводит диалог сообщения с кнопкой "ОК"
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="Icon">Значок в левой части диалога</param>
    public static void MessageBox(string text, string caption,
      MessageBoxIcon Icon)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, Icon, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Выводит диалог сообщения с кнопкой "ОК"
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="icon">Значок в левой части диалога</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    public static void MessageBox(string text, string caption,
      MessageBoxIcon icon, string helpContext)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, helpContext);
    }

    /// <summary>
    /// Выводит диалог сообщения с кнопкой "ОК" без значка
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    public static void MessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Выводит сообщение без строки заголовка и значка
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public static void MessageBox(string text)
    {
      MessageBox(text, "", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }


    /// <summary>
    /// Выводит диалог с сообщением об ошибке и кнопкой "ОК".
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    public static void ErrorMessageBox(string text, string caption, String helpContext)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, helpContext);
    }

    /// <summary>
    /// Выводит диалог с сообщением об ошибке и кнопкой "ОК".
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    public static void ErrorMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Выводит диалог с сообщением об ошибке и кнопкой "ОК".
    /// Заголовок диалога содержит слово "Ошибка"
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public static void ErrorMessageBox(string text)
    {
      MessageBox(text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Выводит диалог с предупреждением и кнопкой "ОК".
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    /// <param name="helpContext">Контекст справки в терминах EFPApp.ShowHelp().
    /// Если задана непустая строка, то в диалоге будет показана кнопка "Справка".</param>
    public static void WarningMessageBox(string text, string caption, String helpContext)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, helpContext);
    }

    /// <summary>
    /// Выводит диалог с предупреждением и кнопкой "ОК".
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна сообщения</param>
    public static void WarningMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Выводит диалог с предупреждением и кнопкой "ОК".
    /// Заголовок окна содержит слово "Предупреждение"
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public static void WarningMessageBox(string text)
    {
      MessageBox(text, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }


    /// <summary>
    /// Вывод временного сообщения в статусной строке.
    /// Предыдущее сообщение убирается.
    /// Этот метод можно вызывать из любого потока. Если вызвано не из основного потока приложения,
    /// то вывод сообщения откладывается. Оно будет выведено по сигналу таймера в основном потоке с максимальной задержкой 1с
    /// (при условии, что основной поток приложения не занят).
    /// </summary>
    /// <param name="text">Текст сообщения. Если пустая строка или null,
    /// то существующее сообщение убирается немедленно</param>
    public static void ShowTempMessage(string text)
    {
      //CheckMainThread();

      if (EFPApp.IsMainThread)
      {
        _DelayedTempMessage = null;
        //if (EFPMainMDIForm.MainForm != null)
        //  EFPMainMDIForm.MainForm.SetTempMessage(Text);
        TempMessageForm.SetTempMessage(text);
      }
      else if (text == null)
        _DelayedTempMessage = String.Empty;
      else
        _DelayedTempMessage = text;
    }

    /// <summary>
    /// Хранение сообщения ShowTempMessage(), если метод вызван не в основном потоке приложения.
    /// Null означает отсутствие отложенного сообщения (в отличие от пустой строки "").
    /// </summary>
    private static volatile string _DelayedTempMessage = null;

    internal static void ProcessDelayedTempMessage()
    {
      string s = _DelayedTempMessage;
      if (s != null)
        ShowTempMessage(s);
    }

    #endregion

    #region Централизованная обработка ошибок

    /// <summary>
    /// Предназначено для централизованной обработки исключений на уровне приложения.
    /// Это событие вызывается, если перехвачено исключение, например, при выполнении команды меню.
    /// Обработчик может, например, вывести MessageBox для определенных классов исключений.
    /// Если нет присоединенных обработчиков, то исключение UserCancelException игнорируется, а для остальных вызывается DebugTools.ShowException().
    /// Как и метод EFPApp.ShowException(), событие может вызываться из любого потока.
    /// </summary>
    public static event EFPAppExceptionEventHandler ExceptionShowing;

    /// <summary>
    /// Вывести сообщение об ошибке с помощью DebugTools.ShowException или присоединенного обработчика события ExceptionShowing.
    /// Метод должен использоваться внутри блока catch.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="exception">Перехваченное исключение</param>
    /// <param name="title">Заголовок для выдачи сообщения</param>
    public static void ShowException(Exception exception, string title)
    {
      Interlocked.Increment(ref _InsideShowExceptionCount);
      try
      {
        DoShowException(exception, title);
      }
      finally
      {
        Interlocked.Decrement(ref _InsideShowExceptionCount);
      }
    }

    private static void DoShowException(Exception exception, string title)
    {
      try
      {
        if (ExceptionShowing != null)
        {
          EFPAppExceptionEventArgs args = new EFPAppExceptionEventArgs(exception, title);
          ExceptionShowing(null, args);
          if (!args.Handled)
          {
            if (LogoutTools.GetException<UserCancelException>(exception) != null)
              return;

            DebugTools.ShowException(exception, title);
          }
        }
        else
          DebugTools.ShowException(exception, title); // 19.09.2016
      }
      catch (Exception e2)
      {
        DebugTools.ShowException(e2, "Ошибка при выводе сообщения об исключении");
      }
    }


    /// <summary>
    /// Свойство возвращает true, если в данный момент работает метод EFPApp.ShowException()
    /// </summary>
    public static bool InsideShowException { get { return _InsideShowExceptionCount > 0; } }
    private static int _InsideShowExceptionCount = 0; // могут быть вложенные вызовы ShowException()

    /// <summary>
    /// Альтернативный способ вывода сообщения об исключении.
    /// Показывает сообщение a'la MessageBox() с дополнительной кнопкой "Подробности".
    /// Этот метод может вызываться из обработчика EFPApp.ExceptionShowing.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="message">Текст сообщения, выводимый в MessageBox</param>
    /// <param name="exception">Объект исключения</param>
    /// <param name="title">Заголовок сообщения</param>
    /// <param name="alwaysLogout">Если true, то log-файл будет создан, даже если пользователь не нажмет кнопку "Подробности"</param>
    public static void ExceptionMessageBox(string message, Exception exception, string title, bool alwaysLogout)
    {
      EFPApp.SuspendIdle(); // 18.08.2021
      try
      {
        try
        {
          ShowExceptionMsgBoxForm frm = new ShowExceptionMsgBoxForm();
          frm.MsgLabel.Text = message;
          frm.Text = title;
          frm.Exception = exception;
          if (alwaysLogout) // иначе потом запишем
            frm.LogFilePath = LogoutTools.LogoutExceptionToFile(exception, title);
          EFPApp.ShowDialog(frm, true);
        }
        catch (Exception e2)
        {
          DebugTools.ShowException(e2, "Ошибка при выводе сообщения об исключении");
        }
      }
      finally
      {
        EFPApp.ResumeIdle();
      }
    }

    /// <summary>
    /// Альтернативный способ вывода сообщения об исключении.
    /// Показывает сообщение a'la MessageBox() с текстом <paramref name="exception"/>.Message и дополнительной кнопкой "Подробности".
    /// Этот метод может вызываться из обработчика EFPApp.ExceptionShowing.
    /// Этот метод может вызываться из любого потока.
    /// Log-файл создается, только если пользователь нажмет кнопку "Подробности".
    /// </summary>
    /// <param name="exception">Объект исключения</param>
    /// <param name="title">Заголовок сообщения</param>
    public static void ExceptionMessageBox(Exception exception, string title)
    {
      ExceptionMessageBox(exception.Message, exception, title, false);
    }

    /// <summary>
    /// Альтернативный способ вывода сообщения об исключении.
    /// Показывает сообщение a'la MessageBox() с дополнительной кнопкой "Подробности".
    /// Этот метод может вызываться из обработчика EFPApp.ExceptionShowing.
    /// Этот метод может вызываться из любого потока.
    /// Log-файл создается, только если пользователь нажмет кнопку "Подробности".
    /// </summary>
    /// <param name="message">Текст сообщения, выводимый в MessageBox</param>
    /// <param name="exception">Объект исключения</param>
    /// <param name="title">Заголовок сообщения</param>
    public static void ExceptionMessageBox(string message, Exception exception, string title)
    {
      ExceptionMessageBox(message, exception, title, false);
    }

    #endregion

    #region Повторение процесса при возникновении исключения

    /// <summary>
    /// Вызвать произвольный пользовательский делегат с помощью Delegate.DynamicInvoke().
    /// Эта перегрузка метода предполагает использование делегата без аргументов.
    /// В случае ошибки выводится экранная заставка и попытки вызова делегата повторяются, пока 
    /// метод не выполнится без выброса исключения или пока пользователь не нажмет кнопку "Отмена".
    /// В заставке выводится текст сообщения об ошибке.
    /// Между попытками вызова делегата выполняется ожидание в 500 мс.
    /// Заставка не выводится на время выполнения делегата. Если действие длительное, то в делегате
    /// должна быть предусмотрена собственная заставка.
    /// Если пользователь нажимает кнопку "Отмена", то наружу выбрасывается исключение делегата,
    /// а не UserCancelException.
    /// </summary>
    /// <param name="action">Пользовательский делегат.
    /// Null или пустой массив, если делегат не получает аргументов</param>
    public static object RepeatWhileException(Delegate action)
    {
      return RepeatWhileException(action, null, String.Empty, 500);
    }

    /// <summary>
    /// Вызвать произвольный пользовательский делегат с помощью Delegate.DynamicInvoke().
    /// В случае ошибки выводится экранная заставка и попытки вызова делегата повторяются, пока 
    /// метод не выполнится без выброса исключения или пока пользователь не нажмет кнопку "Отмена".
    /// В заставке выводится текст сообщения об ошибке.
    /// Между попытками вызова делегата выполняется ожидание в 500 мс.
    /// Заставка не выводится на время выполнения делегата. Если действие длительное, то в делегате
    /// должна быть предусмотрена собственная заставка.
    /// Если пользователь нажимает кнопку "Отмена", то наружу выбрасывается исключение делегата,
    /// а не UserCancelException.
    /// </summary>
    /// <param name="action">Пользовательский делегат</param>
    /// <param name="args">Аргументы, которые должны быть переданы делегату. 
    /// Null или пустой массив, если делегат не получает аргументов</param>
    public static object RepeatWhileException(Delegate action, object[] args)
    {
      return RepeatWhileException(action, args, String.Empty, 500);
    }

    /// <summary>
    /// Вызвать произвольный пользовательский делегат с помощью Delegate.DynamicInvoke().
    /// В случае ошибки выводится экранная заставка и попытки вызова делегата повторяются, пока 
    /// метод не выполнится без выброса исключения или пока пользователь не нажмет кнопку "Отмена".
    /// Между попытками вызова делегата выполняется ожидание в 500 мс.
    /// Заставка не выводится на время выполнения делегата. Если действие длительное, то в делегате
    /// должна быть предусмотрена собственная заставка.
    /// Если пользователь нажимает кнопку "Отмена", то наружу выбрасывается исключение делегата,
    /// а не UserCancelException.
    /// </summary>
    /// <param name="action">Пользовательский делегат</param>
    /// <param name="args">Аргументы, которые должны быть переданы делегату. 
    /// Null или пустой массив, если делегат не получает аргументов</param>
    /// <param name="splashText">Текст для экранной заставки. Если не задано, то текст генерируется автоматически</param>
    public static object RepeatWhileException(Delegate action, object[] args, string splashText)
    {
      return RepeatWhileException(action, args, splashText, 500);
    }

    /// <summary>
    /// Вызвать произвольный пользовательский делегат с помощью Delegate.DynamicInvoke().
    /// В случае ошибки выводится экранная заставка и попытки вызова делегата повторяются, пока 
    /// метод не выполнится без выброса исключения или пока пользователь не нажмет кнопку "Отмена".
    /// Заставка не выводится на время выполнения делегата. Если действие длительное, то в делегате
    /// должна быть предусмотрена собственная заставка.
    /// Если пользователь нажимает кнопку "Отмена", то наружу выбрасывается исключение делегата,
    /// а не UserCancelException.
    /// </summary>
    /// <param name="action">Пользовательский делегат</param>
    /// <param name="args">Аргументы, которые должны быть переданы делегату. 
    /// Null или пустой массив, если делегат не получает аргументов</param>
    /// <param name="splashText">Текст для экранной заставки. Если не задано, то текст генерируется автоматически</param>
    /// <param name="interval">Интервал времени между попытками вызова в миллисекундах</param>
    /// <returns>Значение, которое было возвращено делегатом</returns>
    public static object RepeatWhileException(Delegate action, object[] args, string splashText, int interval)
    {
#if DEBUG
      if (action == null)
        throw new ArgumentNullException("action");
      if (interval < 1)
        throw new ArgumentException("Интервал времени должен быть задан", "interval");
#endif

      Splash spl = null;

      object res = null;
      try
      {
        while (true)
        {
          try
          {
            res = action.DynamicInvoke(args);
          }
          catch (Exception e)
          {
            if (spl == null)
            {
              spl = new Splash(String.IsNullOrEmpty(splashText) ? "?" : splashText);
              spl.AllowCancel = true;
            }
            if (String.IsNullOrEmpty(splashText))
              spl.PhaseText = e.Message;
            spl.Sleep(interval);
            if (spl.Cancelled)
              throw;
            else
              continue;
          }
          break;
        }

      }
      finally
      {
        if (spl != null)
          spl.Close();
      }

      return res;
    }

    #endregion

    #region Вывод окон для просмотра текста

    /// <summary>
    /// Показать окно просмотра текста.
    /// Окно выводится в модальном или немодальном режиме, в зависимости от состояния программы (наличия открытых блоков диалога).
    /// Этот метод может быть вызван не из главного потока приложения или инициализации InitApp().
    /// В этом случае окно показывается с упрощенным интерфейсом в можальном режиме
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="title">Заголовок окна</param>
    public static void ShowTextView(string text, string title)
    {
      ShowTextView(text, title, false);
    }

    /// <summary>
    /// Показать окно просмотра текста.
    /// Этот метод может быть вызван не из главного потока приложения или инициализации InitApp().
    /// В этом случае окно показывается с упрощенным интерфейсом в модальном режиме
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="title">Заголовок окна</param>
    /// <param name="isModal">Если true, то окно выводится в модальном режиме. Иначе режим зависит от текущего состояния программы (наличия открытых блоков диалога)</param>
    public static void ShowTextView(string text, string title, bool isModal)
    {
      SimpleForm<TextBox> frm = new SimpleForm<TextBox>();
      try
      {
        frm.Text = title;
        if (EFPApp.IsMainThread)
          frm.Icon = EFPApp.MainImageIcon("Notepad");
        // Убрано 31.08.2016 Form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        if (isModal) // 31.08.2016 
          frm.WindowState = FormWindowState.Maximized;
        TextBox tb = frm.ControlWithToolBar.Control;
        tb.Multiline = true;
        tb.ScrollBars = ScrollBars.Both;
        tb.ReadOnly = true;
        //tb.Font = new System.Drawing.Font(MonospaceFontName, SystemFonts.DefaultFont.Height);
        tb.Font = EFPApp.CreateMonospaceFont(); // 11.01.2019
        tb.Text = text;

        if (EFPApp.IsMainThread)
        {
          EFPTextBox efpTB = new EFPTextBox(frm.ControlWithToolBar);

          if (isModal)
            ShowDialog(frm, true);
          else
            EFPApp.ShowFormOrDialog(frm);
        }
        else // 12.05.2016
        {
          frm.ShowDialog();
          frm.Dispose();
        }
      }
      catch
      {
        frm.Dispose(); // 06.04.2018
        throw;
      }
    }

    /// <summary>
    /// Показать окно просмотра XML-документа.
    /// Модальный или немодальный режим зависит от текущего состояния программы 
    /// (наличия открытых блоков диалога).    
    /// </summary>
    /// <param name="xmlDoc">Просматриваемый XML-документ</param>
    /// <param name="title">Заголовок окна</param>
    public static void ShowXmlView(XmlDocument xmlDoc, string title)
    {
      ShowXmlView(xmlDoc, title, false, String.Empty);
    }

    /// <summary>
    /// Показать окно просмотра XML-документа.
    /// </summary>
    /// <param name="xmlDoc">Просматриваемый XML-документ</param>
    /// <param name="title">Заголовок окна</param>
    /// <param name="isModal">Если true, то окно выводится в модальном режиме. 
    /// Иначе режим зависит от текущего состояния программы (наличия открытых блоков диалога)</param>
    public static void ShowXmlView(XmlDocument xmlDoc, string title, bool isModal)
    {
      ShowXmlView(xmlDoc, title, isModal, String.Empty);
    }

    /// <summary>
    /// Показать окно просмотра XML-документа.
    /// </summary>
    /// <param name="xmlDoc">Просматриваемый XML-документ</param>
    /// <param name="title">Заголовок окна</param>
    /// <param name="isModal">Если true, то окно выводится в модальном режиме. 
    /// Иначе режим зависит от текущего состояния программы (наличия открытых блоков диалога)</param>
    /// <param name="fileName">Имя файла без пути. Используется при выполнении команды "Открыть с помощью",
    /// чтобы показать осмыленное имя файла, например, в блокноте.
    /// Файл не обязан реально существовать.</param>
    public static void ShowXmlView(XmlDocument xmlDoc, string title, bool isModal, string fileName)
    {
      SimpleForm<XmlViewBox> frm = new SimpleForm<XmlViewBox>();
      try
      {
        frm.Text = title;
        frm.Icon = EFPApp.MainImageIcon("XML");
        // Убрано 31.08.2016 Form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        if (isModal) // 31.08.2016 
          frm.WindowState = FormWindowState.Maximized;

        EFPXmlViewBox efpVB = new EFPXmlViewBox(frm.ControlWithToolBar);

        efpVB.XmlDocument = xmlDoc;
        efpVB.FileName = fileName;

        if (isModal)
          ShowDialog(frm, true);
        else
          EFPApp.ShowFormOrDialog(frm);
      }
      catch
      {
        frm.Dispose(); // 06.04.2018
        throw;
      }
    }

    /// <summary>
    /// Показать окно просмотра XML-файла.
    /// В заголовке окна отображается путь к файлу.
    /// Модальный или немодальный режим зависит от текущего состояния программы 
    /// (наличия открытых блоков диалога).    
    /// </summary>
    /// <param name="filePath">Путь к существующему файлу на диске</param>
    public static void ShowXmlView(AbsPath filePath)
    {
      ShowXmlView(filePath, filePath.Path, false);
    }

    /// <summary>
    /// Показать окно просмотра XML-файла.
    /// Модальный или немодальный режим зависит от текущего состояния программы 
    /// (наличия открытых блоков диалога).    
    /// </summary>
    /// <param name="filePath">Путь к существующему файлу на диске</param>
    /// <param name="title">Заголовок окна</param>
    public static void ShowXmlView(AbsPath filePath, string title)
    {
      ShowXmlView(filePath, title, false);
    }

    /// <summary>
    /// Показать окно просмотра XML-файла.
    /// </summary>
    /// <param name="filePath">Путь к существующему файлу на диске</param>
    /// <param name="title">Заголовок окна</param>
    /// <param name="isModal">Если true, то окно выводится в модальном режиме. 
    /// Иначе режим зависит от текущего состояния программы (наличия открытых блоков диалога)</param>
    public static void ShowXmlView(AbsPath filePath, string title, bool isModal)
    {
      SimpleForm<XmlViewBox> frm = new SimpleForm<XmlViewBox>();
      try
      {
        frm.Text = title;
        frm.Icon = EFPApp.MainImageIcon("XML");
        // Убрано 31.08.2016 Form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        if (isModal) // 31.08.2016 
          frm.WindowState = FormWindowState.Maximized;

        EFPXmlViewBox efpVB = new EFPXmlViewBox(frm.ControlWithToolBar);

        efpVB.Control.XmlFilePath = filePath.Path;

        if (isModal)
          ShowDialog(frm, true);
        else
          EFPApp.ShowFormOrDialog(frm);
      }
      catch
      {
        frm.Dispose(); // 06.04.2018
        throw;
      }
    }

    #endregion

    #region Вывод диалога со списком ошибок

    /// <summary>
    /// Показать диалог со списком ошибок
    /// Коды ошибок не показываются
    /// </summary>
    /// <param name="errorMessages">Заполненный список ошибок</param>
    /// <param name="title">Заголовок формы</param>
    public static void ShowErrorMessageListDialog(ErrorMessageList errorMessages, string title)
    {
      ShowErrorMessageListDialog(errorMessages, title, 0, null);
    }

    /// <summary>
    /// Показать диалог со списком ошибок
    /// </summary>
    /// <param name="errorMessages">Заполненный список ошибок</param>
    /// <param name="title">Заголовок формы</param>
    /// <param name="codeWidth">Нулевое значение запрещает вывод колонки "Код".
    /// Положительное значение задает ширину колонки "Код" в символах</param>
    public static void ShowErrorMessageListDialog(ErrorMessageList errorMessages, string title, int codeWidth)
    {
      ShowErrorMessageListDialog(errorMessages, title, codeWidth, null);
    }

    /// <summary>
    /// Показать диалог со списком ошибок и возможностью "редактирования" ошибок
    /// </summary>
    /// <param name="errorMessages">Заполненный список ошибок</param>
    /// <param name="title">Заголовок формы</param>
    /// <param name="codeWidth">Нулевое значение запрещает вывод колонки "Код".
    /// Положительное значение задает ширину колонки "Код" в символах</param>
    /// <param name="editHandler">Обработчик для "редактирования" сообщений об ошибках.
    /// Если null, то режим редактирования не поддерживается просмотром</param>
    public static void ShowErrorMessageListDialog(ErrorMessageList errorMessages, string title, int codeWidth, ErrorMessageItemEventHandler editHandler)
    {
#if DEBUG
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");
#endif

      OKCancelGridForm frm = new OKCancelGridForm();
      try
      {
        WinFormsTools.OkCancelFormToOkOnly(frm);
        frm.Text = title;
        frm.Icon = EFPApp.MainImageIcon(EFPApp.GetErrorImageKey(errorMessages.NullableSeverity));
        EFPApp.SetFormSize(frm, 80, 50);

        EFPErrorDataGridView TheHandler = new EFPErrorDataGridView(frm);
        TheHandler.CodeWidth = codeWidth;
        TheHandler.EditMessage += editHandler;
        //TheHandler.GridPageSetup.Title = Title;
        TheHandler.ErrorMessages = errorMessages;

        EFPApp.ShowDialog(frm, true);
      }
      catch
      {
        frm.Dispose(); // 06.04.2018
        throw;
      }
    }

    #endregion

    #region Изображения и текст для списков ошибок

    /// <summary>
    /// Возвращает тег изображения в списке EFPApp.MainImages, соответствующего
    /// значению перечисления.
    /// Эта перегрузка используется редко.
    /// </summary>
    /// <param name="kind">Элемент перечисления</param>
    /// <returns>Тег изображения</returns>
    public static string GetErrorImageKey(ErrorMessageKind kind)
    {
      switch (kind)
      {
        case ErrorMessageKind.Error:
          return "Error";
        case ErrorMessageKind.Warning:
          return "Warning";
        case ErrorMessageKind.Info:
          return "Information";
        default:
          return "UnknownState";
      }
    }

    /// <summary>
    /// Возвращает тег изображения в списке EFPApp.MainImages, соответствующего
    /// значению перечисления.
    /// Эта перегрузка поддерживает значение null, соответствующее пустому списку
    /// сообщений. Для null возвращается "Ok"
    /// </summary>
    /// <param name="kind">Элемент перечисления или null</param>
    /// <returns>Тег изображения</returns>
    public static string GetErrorImageKey(ErrorMessageKind? kind)
    {
      if (kind.HasValue)
        return GetErrorImageKey(kind.Value);
      else
        return "Ok";
    }

    /// <summary>
    /// Возвращает тег изображения в списке EFPApp.MainImages, соответствующего
    /// списку ошибок. Если список не содержит сообщений, возвращается "OK".
    /// </summary>
    /// <param name="errorMessages">Список сообщений</param>
    /// <returns>Тег изображения</returns>
    public static string GetErrorImageKey(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        return "UnknownState";
      else
        return GetErrorImageKey(errorMessages.NullableSeverity);
    }


    /// <summary>
    /// Получить текст для закладки в блоке диалога или отчете для отображения списка ошибок
    /// (""Ошибки", "Предупреждения", "Сообщения").
    /// Эта перегрузка используется редко.
    /// </summary>
    /// <param name="kind">Элемент перечисления</param>
    /// <returns>Текст заголовка</returns>
    public static string GetErrorTitleText(ErrorMessageKind kind)
    {
      switch (kind)
      {
        case ErrorMessageKind.Error:
          return "Ошибки";
        case ErrorMessageKind.Warning:
          return "Предупреждения";
        case ErrorMessageKind.Info:
          return "Сообщения";
        default:
          return "Неизв. состояние " + kind.ToString();
      }
    }

    /// <summary>
    /// Получить текст для закладки в блоке диалога или отчете для отображения списка ошибок
    /// ("Нет ошибок", "Ошибки", "Предупреждения", "Сообщения")
    /// </summary>
    /// <param name="kind">Элемент перечисления или null</param>
    /// <returns>Текст заголовка</returns>
    public static string GetErrorTitleText(ErrorMessageKind? kind)
    {
      if (kind.HasValue)
        return GetErrorTitleText(kind.Value);
      else
        return "Нет ошибок";
    }

    /// <summary>
    /// Получить текст для закладки в блоке диалога или отчете для отображения списка ошибок
    /// ("Нет ошибок", "Ошибки", "Предупреждения", "Сообщения")
    /// </summary>
    /// <param name="errorMessages">Список сообщений</param>
    /// <returns>Текст заголовка</returns>
    public static string GetErrorTitleText(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        return "Нет ошибок";
      else
        return GetErrorTitleText(errorMessages.NullableSeverity);
    }

    /// <summary>
    /// Текст подсказок о наличии сообщений об ошибках и предупреждений
    /// </summary>
    /// <param name="errorMessages">Список сообщений</param>
    /// <returns>Подсказка</returns>
    public static string GetErrorToolTipText(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        return "Список ошибок не присоединен";


      int n1 = errorMessages.ErrorCount;
      int n2 = errorMessages.WarningCount;
      int n3 = errorMessages.InfoCount;

      if (n1 == 0 && n2 == 0 && n3 == 0)
        return "Нет сообщений об ошибках";

      List<string> lst = new List<string>();
      if (n1 > 0)
        lst.Add("Ошибок: " + n1.ToString());
      if (n2 > 0)
        lst.Add("Предупреждений: " + n2.ToString());
      if (n3 > 0)
        lst.Add("Инф. сообщений: " + n3.ToString());
      return String.Join(", ", lst.ToArray());
    }

    #endregion

    #region Диалог "О программе"

    /// <summary>
    /// Выводит простое диалоговое окно "О программе".
    /// Если требуется настроить вид диалога, используйте класс AboutDialog
    /// </summary>
    public static void ShowAboutDialog()
    {
      AboutDialog dlg = new AboutDialog();
      dlg.ShowDialog();
    }

    /// <summary>
    /// Выводит блок диалога со списком загруженных сборок
    /// </summary>
    public static void ShowAssembliesDialog()
    {
      SimpleAboutDialogForm.ShowAssembliesDialog();
    }

    #endregion

    #region Работа с индикацией кратковременных процессов

    /* 
     * Методы BeginWait и EndWait используются, когда приложению нужно
     * показать пользователю признак занятости, но выводить окно-заставку
     * не целесообразно. Методы следует использовать, когда процесс 
     * длиться не более нескольких секунд.
     * 
     * Перед выполнением действия вызывается один из перегруженных
     * методов BeginWait, а после - EndWait:
     * 
     *   EFPApp.BeginWait()
     *   try
     *   {
     *     // Действие
     *     ...
     *   }
     *   finally
     *   {
     *     EFPApp.EndWait()
     *   }
     * 
     * Вызов BeginWait() устанавлиает индиатор - песочные часы и
     * (опционально) - тест в статусной строке и картинку. Вывод
     * процентного индикатора и возможность прервать процесс не
     * предусмотрены - следует использовать Splash()
     * 
     * Пары вызовов BeginWait() / EndWait() могут быть вложенными
     */

    /// <summary>
    /// Начало индикации ожидания только с выводом песочных часов -
    /// без сообщений в статусной строке
    /// </summary>
    public static void BeginWait()
    {
      BeginWait(String.Empty, String.Empty, false);
    }

    /// <summary>
    /// Начало индикации ожидания с выводом текстового сообщения в
    /// статусной строке.
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    public static void BeginWait(string message)
    {
      BeginWait(message, String.Empty, false);
    }

#if XXX
    /// <summary>
    /// Начало индикации ожидания с выводом текстового сообщения в
    /// статусной строке и значком
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    /// <param name="imageIndex">Номер изображения в MainImages</param>
    public static void BeginWait(string message, int imageIndex)
    {
      BeginWait(message, imageIndex, false);
    }

    /// <summary>
    /// Начало индикации ожидания с выводом текстового сообщения в
    /// статусной строке и значком
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    /// <param name="imageIndex">Номер изображения в MainImages</param>
    /// <param name="updateImmediately">true, если нужно немедленно нарисовать текст и значок</param>
    private static void BeginWait(string message, int imageIndex, bool updateImmediately)
    {
      if (!EFPApp.IsMainThread) // 16.05.2016
        return;

      TempWaitForm.BeginWait(message, imageIndex, updateImmediately);
    }
#endif

    /// <summary>
    /// Начало индикации ожидания с выводом текстового сообщения в
    /// статусной строке и значком
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    /// <param name="imageKey">Строковый идентификатор изображения в MainImages</param>
    public static void BeginWait(string message, string imageKey)
    {
      BeginWait(message, imageKey, false);
    }

    /// <summary>
    /// Начало индикации ожидания с выводом текстового сообщения в
    /// статусной строке и значком
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    /// <param name="imageKey">Строковый идентификатор изображения в MainImages</param>
    /// <param name="updateImmediately">true, если нужно немедленно нарисовать текст и значок</param>
    public static void BeginWait(string message, string imageKey, bool updateImmediately)
    {
      if (!EFPApp.IsMainThread) // 16.05.2016, 14.02.2020
        return;

      int imageIndex = -1;
      if (!String.IsNullOrEmpty(imageKey))
        imageIndex = MainImages.Images.IndexOfKey(imageKey);

      TempWaitForm.BeginWait(message, imageIndex, updateImmediately);
    }

    /// <summary>
    /// Завершение индикации ожидания
    /// </summary>
    public static void EndWait()
    {
      if (!EFPApp.IsMainThread) // 16.05.2016
        return;

      TempWaitForm.EndWait();
    }

    #endregion

    #region Буферы системной информации и печать

    /// <summary>
    /// Доступ к сведениям об установленных принтерах
    /// </summary>
    public static EFPAppPrinters Printers { get { return _Printers; } }
    private static readonly EFPAppPrinters _Printers = new EFPAppPrinters();

    /// <summary>
    /// Менеджер фоновой печати
    /// </summary>
    public static BackgroundPrinting BackgroundPrinting { get { return _BackgroundPrinting; } }
    private static readonly BackgroundPrinting _BackgroundPrinting = new BackgroundPrinting();

    /// <summary>
    /// Доступ к сведениям об установленных шрифтах
    /// </summary>
    public static EFPAppFonts Fonts
    {
      get
      {
        if (_Fonts == null)
          _Fonts = new EFPAppFonts();
        return _Fonts;
      }
    }
    private static EFPAppFonts _Fonts = null;

    /// <summary>
    /// Возвращает имя моноширинного шрифта по умолчанию.
    /// Если невозможно определить из настроек операционной системы, возвращает "Courier".
    /// Свойство доступно из любого потока.
    /// </summary>
    public static string MonospaceFontName
    {
      get
      {
        try
        {
          return System.Drawing.FontFamily.GenericMonospace.Name;
        }
        catch { }
        return "Courier";
      }
    }

    /// <summary>
    /// Создает объект моноширинного шрифт высотой 12 пунктов, 
    /// который следует использовать для просмотра текстовых файлов.
    /// Метод доступен из любого потока.
    /// </summary>
    public static Font CreateMonospaceFont()
    {
      return new Font(MonospaceFontName, 12);
    }

    #endregion

    #region Вызов справки

    /// <summary>
    /// Обработчик может быть установлен, если справочная система требует
    /// какой-либо дополнительной обработки.
    /// Иначе используется стандартный способ показа справки
    /// </summary>
    public static event EFPHelpContextEventHandler ShowHelpNeeded;

    /// <summary>
    /// Вызывает справочную систему для показа выбранного раздела.
    /// Если обработчик события ShowHelpNeeded установлен, то он вызывается.
    /// При этом формат <paramref name="helpContext"/> определяется приложением.
    /// 
    /// Если обработчика нет, то вызывается Help.ShowHelp().
    /// При этом предполагается, что справка создана в виде CHM-файла. 
    /// Контекст справки <paramref name="helpContext"/>
    /// задан в формате "FileName::Topic", где FileName - путь к CHM-файлу, 
    /// а Topic - раздел справки.
    /// </summary>
    /// <param name="helpContext">Контекст справки</param>
    public static void ShowHelp(string helpContext)
    {
#if DEBUG
      CheckMainThread();
#endif

      if (ShowHelpNeeded == null)
      {
        if (String.IsNullOrEmpty(helpContext))
          MessageBox("Контекст справки не задан");
        else
        {
          int p = helpContext.IndexOf("::", StringComparison.Ordinal);
          if (p < 0)
            throw new ArgumentException("Неправильный контекст справки: \"" + helpContext + "\"", "helpContext");
          string fileName = helpContext.Substring(0, p);
          string topic = helpContext.Substring(p + 2);
          Help.ShowHelp(null, fileName, HelpNavigator.Topic, topic);
        }
      }
      else
      {
        EFPHelpContextEventArgs args = new EFPHelpContextEventArgs(helpContext);
        ShowHelpNeeded(null, args);
      }
    }

    #endregion

    #region Текущий каталог

    /*
     * В стандартном варианте блоки диалога OpenFileDialog и SaveFileDialog 
     * меняют текущий каталог Environment.CurrentDirectory, если не установлено
     * свойство RestoreDirectory. Блок диалога FolderBrowseDialog не меняет каталог.
     * 
     * Если пользователь записывает файлы на флэш и Environment.CurrentDirectory
     * устанавливается на флэш, то флэш нельзя извлечь до завершения работы
     * программы.
     * 
     * Реализация.
     * Класс EFPApp хранит собственную переменную CurrentDirectory, которая никак
     * не блокирует каталог, поэтому флэш можно извлечь. Переопределенные методы
     * ShowDialog() работают с этой переменной
     */

    /// <summary>
    /// Текущий каталог, используемый блоками диалога
    /// Каталог не блокируется Windows, поэтому не препятствует безопасному
    /// извлечению устройства, если путь указывает на флэш
    /// Свойство является потокобезопасным.
    /// При установке свойства значение отбрасывается, если оно пустое или каталога не существует
    /// </summary>
    public static AbsPath CurrentDirectory
    {
      get
      {
        lock (WinFormsTools.InternalSyncRoot)
        {
          if (!_CurrentDirectory.HasValue)
            _CurrentDirectory = new AbsPath(Environment.CurrentDirectory);
          return _CurrentDirectory.Value;
        }
      }
      set
      {
        if (value.IsEmpty)
          return;
        lock (WinFormsTools.InternalSyncRoot)
        {
          if (!_CurrentDirectory.HasValue)
            _CurrentDirectory = new AbsPath(Environment.CurrentDirectory);
          if (value == _CurrentDirectory.Value)
            return;

          if (DirectoryExists(value))
            _CurrentDirectory = value;
        }
      }
    }
    private static AbsPath? _CurrentDirectory = null;

    /// <summary>
    /// Используйте этот метод вместо Open/SaveFileDialog.ShowDialog().
    /// Устанавливает FileDialog.InitialDirectory, если оно не установлено и не задан путь к файлу.
    /// Устанавливает свойство CurrentDirectory, если пользователь выбрал путь к файлу.
    /// </summary>
    /// <param name="dialog">Диалог выбора файла</param>
    /// <returns></returns>
    public static DialogResult ShowDialog(FileDialog dialog)
    {
      dialog.RestoreDirectory = true;
      if (String.IsNullOrEmpty(dialog.InitialDirectory) && String.IsNullOrEmpty(dialog.FileName))
        dialog.InitialDirectory = CurrentDirectory.Path;

      DialogResult res = dialog.ShowDialog();

      if (res == DialogResult.OK)
      {
        if (!String.IsNullOrEmpty(dialog.FileName))
          CurrentDirectory = new AbsPath(Path.GetDirectoryName(dialog.FileName));
      }

      return res;
    }

    /// <summary>
    /// Выводит блок диалога выбора каталога.
    /// Используйте этот метод вместо FolderBrowserDialog.ShowDialog().
    /// Устанавливает FolderBrowserDialog.SelectedPath, если оно не задано в явном виде.
    /// Устанавливает свойство CurrentDirectory, если пользователь выбрал каталог.
    /// </summary>
    /// <param name="dialog">Блок диалога</param>
    /// <returns>ОК, если пользователь выбрал каталог</returns>
    public static DialogResult ShowDialog(FolderBrowserDialog dialog)
    {
      if (String.IsNullOrEmpty(dialog.SelectedPath))
        dialog.SelectedPath = CurrentDirectory.Path;

      DialogResult res = dialog.ShowDialog();

      if (res == DialogResult.OK)
        CurrentDirectory = new AbsPath(dialog.SelectedPath);

      return res;
    }

    #endregion

    #region Запуск Проводника Windows

    /// <summary>
    /// Открывает окно проводника Windows для заданной папки.
    /// Если папка не существует, выдается соответствуюшее сообщение
    /// </summary>
    /// <param name="dir">Имя просматриваемой папки</param>
    /// <returns>true, если Проводник Windows успешно запущен</returns>
    public static bool ShowWindowsExplorer(AbsPath dir)
    {
      if (_InsideShowWindowsExplorer)
      {
        EFPApp.ShowTempMessage("Предыдущая команда открывтия Проводника еще не выполнена");
        return false;
      }

      bool res;
      _InsideShowWindowsExplorer = true;
      try
      {
        res = DoShowWindowsExplorer(dir);
      }
      finally
      {
        _InsideShowWindowsExplorer = false;
      }

      return res;
    }

    private static bool _InsideShowWindowsExplorer = false;

    private static bool DoShowWindowsExplorer(AbsPath dir)
    {
      //const int ERROR_ACCESS_DENIED = 0x5;

      if (dir.IsEmpty)
      {
        EFPApp.ErrorMessageBox("Каталог не задан");
        return false;
      }


      if (!DirectoryExists(dir))
      {
        EFPApp.ErrorMessageBox("Каталог не существует: \"" + dir.Path + "\"");
        return false;
      }

#if XXX
      bool Res = true;
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          EFPApp.BeginWait("Запуск проводника Windows", "WindowsExplorer");
          try
          {
            try
            {
              ProcessStartInfo psi = new ProcessStartInfo();
              psi.FileName = "explorer.exe";
              psi.Arguments = Dir.Path;
              Process.Start(psi);
            }
            catch (Win32Exception e)
            {
              e.Data["Dir"] = Dir.Path;
              if (e.NativeErrorCode == ERROR_ACCESS_DENIED)
                // 14.09.2017
                EFPApp.ErrorMessageBox("Нельзя просмотреть каталог \"" + Dir.Path + "\". Доступ запрещен", "Ошибка запуска Проводника Windows");
              else
                EFPApp.ShowException(e, "Ошибка запуска Проводника Windows");
              Res = false;
            }
            catch (Exception e)
            {
              e.Data["Dir"] = Dir.Path;
              EFPApp.ShowException(e, "Ошибка запуска Проводника Windows");
              Res = false;
            }
          }
          finally
          {
            EFPApp.EndWait();
          }
          return Res;

        default:
          EFPApp.ErrorMessageBox("Для установленной операционной системы программа просмотра каталогов неизвестна");
          return false;
      }
#endif

      if (IsWindowsExplorerSupported)
      {
        FreeLibSet.Shell.FileAssociationItem faItem = EFPApp.FileExtAssociations.ShowDirectory.OpenItem;
        try
        {
          EFPApp.BeginWait("Запуск " + faItem.DisplayName, "WindowsExplorer");
          try
          {
            faItem.Execute(dir);
          }
          finally
          {
            EFPApp.EndWait();
          }
          return true;
        }
        catch (Exception e)
        {
          e.Data["Dir"] = dir.Path;
          EFPApp.ShowException(e, "Ошибка запуска " + faItem.DisplayName);
          return false;
        }
      }
      else
      {
        EFPApp.ErrorMessageBox("Для установленной операционной системы программа просмотра каталогов неизвестна");
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если команда просмотра каталога поддерживается ОС и 
    /// метод ShowWindowsExplorer() может быть вызван
    /// </summary>
    public static bool IsWindowsExplorerSupported
    {
      get
      {
#if XXX
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            return true;
          default:
            return false;
        }
#endif

        return EFPApp.FileExtAssociations.ShowDirectory.OpenItem != null;
      }
    }

    /// <summary>
    /// Открывает произвольный файл с использованием файловой ассоциации "Открыть".
    /// Использует Process.Start() с UseShellExecute=true.
    /// В случае отсутствия файла или невозможности его открытия выдается сообщение об ошибке,
    /// а исключение не выбрасывается
    /// </summary>
    /// <param name="filePath">Путь к существующему файлу</param>
    /// <returns>true, если запуск был выполнен</returns>
    public static bool ShellExecute(AbsPath filePath)
    {
      try
      {
        return DoShellExecute(filePath);
      }
      catch (Exception e)
      {
        e.Data["Path"] = filePath;
        EFPApp.ExceptionMessageBox("Не удалось открыть файл \"" + filePath.Path + "\"", e, "Ошибка открытия файла");
        return false;
      }
    }

    private static bool DoShellExecute(AbsPath filePath)
    {
      if (!FileExists(filePath))
      {
        EFPApp.ErrorMessageBox("Файл не найден: " + filePath.Path, "Ошибка открытия файла");
        return false;
      }

      ProcessStartInfo psi = new ProcessStartInfo();
      psi.FileName = filePath.Path;
      psi.UseShellExecute = true;
      Process.Start(psi);
      return true;
    }

    /// <summary>
    /// Вызов Directory.Exists() для определения существования каталога с выдачей заставки
    /// </summary>
    /// <param name="dir">Путь к каталогу</param>
    /// <returns>Наличие каталога</returns>
    public static bool DirectoryExists(AbsPath dir)
    {
      bool res;
      EFPApp.BeginWait("Определение существования каталога", "Open");
      try
      {
        res = Directory.Exists(dir.Path);
      }
      finally
      {
        EFPApp.EndWait();
      }

      return res;
    }

    /// <summary>
    /// Вызов Directory.Exists() для определения существования файла с выдачей заставки
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Наличие файла</returns>
    public static bool FileExists(AbsPath filePath)
    {
      bool res;
      EFPApp.BeginWait("Определение существования файла", "Open");
      try
      {
        res = File.Exists(filePath.Path);
      }
      finally
      {
        EFPApp.EndWait();
      }

      return res;
    }

    #endregion

    #region Поддержка экспорта

    /// <summary>
    /// Поля сводки по умолчанию, используемые при экспорте табличного просмотра
    /// </summary>
    public static EFPDocumentProperties DefaultDocumentProperties { get { return _DefaultDocumentProperties; } set { _DefaultDocumentProperties = value; } }
    private static EFPDocumentProperties _DefaultDocumentProperties; // EFPDocumentProperties - это структура

    /// <summary>
    /// Временный каталог для хранения файлов, создаваемых командами "Файл-Передать"
    /// Приложение может использовать каталог, отличный от каталога по умолчанию, установив значение свойства
    /// </summary>
    public static SharedTempDirectory SharedTempDir
    {
      get
      {
        if (_SharedTempDir == null)
          _SharedTempDir = new SharedTempDirectory();
        return _SharedTempDir;
      }
      set
      {
        if (_SharedTempDir != null)
          _SharedTempDir.Dispose();
        _SharedTempDir = value;
      }
    }
    private static SharedTempDirectory _SharedTempDir = null;

    #endregion

    #region Внешние программы

    /*
     * В отличие от свойств XXXTools.XXXVersion,
     * свойства версий в этом классе возвращают пустую версию (0.0.0.0),
     * если приложение не установлена.
     * Также выполняется буферизация данных, а реальное определение выполняется
     * только один раз
     */

    #region Microsoft Office

    /// <summary>
    /// Установленная версия Microsoft Word
    /// Если Word не установлен, возвращается (0.0.0.0)
    /// </summary>
    public static Version MicrosoftWordVersion
    {
      get
      {
        if (_MicrosoftWordVersion == null)
        {
          try
          {
            _MicrosoftWordVersion = MicrosoftOfficeTools.WordVersion;
          }
          catch { }
          if (_MicrosoftWordVersion == null)
            _MicrosoftWordVersion = new Version();
        }
        return _MicrosoftWordVersion;
      }
    }
    private static Version _MicrosoftWordVersion;

    /// <summary>
    /// Установленная версия Microsoft Excel
    /// Если Excel не установлен, возвращается (0.0.0.0)
    /// </summary>
    public static Version MicrosoftExcelVersion
    {
      get
      {
        if (_MicrosoftExcelVersion == null)
        {
          try
          {
            _MicrosoftExcelVersion = MicrosoftOfficeTools.ExcelVersion;
          }
          catch { }
          if (_MicrosoftExcelVersion == null)
            _MicrosoftExcelVersion = new Version();
        }
        return _MicrosoftExcelVersion;
      }
    }
    private static Version _MicrosoftExcelVersion;

    #endregion

    #region Open Office / Libre Office

    /// <summary>
    /// Используемая версия OpenOffice / LibreOffice.
    /// При первом обращении к свойству оно принимает значение OpenOfficeTools.Installations[0], если на компьютере есть установленная версия офиса.
    /// Если в приложении предусматривается выбор версии офиса, свойству может быть присвоено значение, отличное от заданного по умолчанию.
    /// Свойству может быть присвоено значение null, что приводит к отказу от использования офиса, например в командах "Отправить" табличного просмотра
    /// </summary>
    public static OpenOfficeTools.OfficeInfo UsedOpenOffice
    {
      get
      {
        if (!_UsedOpenOfficeDefined)
        {
          _UsedOpenOfficeDefined = true;
          if (OpenOfficeTools.Installations.Length == 0)
            _UsedOpenOffice = null;
          else
            _UsedOpenOffice = OpenOfficeTools.Installations[0];
        }
        return _UsedOpenOffice;
      }
      set
      {
        _UsedOpenOfficeDefined = true;
        _UsedOpenOffice = value;
      }
    }
    private static OpenOfficeTools.OfficeInfo _UsedOpenOffice;
    private static bool _UsedOpenOfficeDefined = false;


    /// <summary>
    /// Какой вариант OpenOffice установлен на компьютере: Open Office или Libre Office
    /// </summary>
    public static OpenOfficeKind OpenOfficeKind
    {
      get
      {
        if (UsedOpenOffice == null)
          return OpenOfficeKind.Unknown;
        else
          return UsedOpenOffice.Kind;
      }
    }

    /// <summary>
    /// Возвращает "OpenOffice.org" или "Libre Office", в зависимости от установленной
    /// версии OpenOffice
    /// </summary>
    public static string OpenOfficeKindName
    {
      get
      {
        if (UsedOpenOffice == null)
          return "OpenOffice / LibreOffice";
        else
          return UsedOpenOffice.KindName;
      }
    }

    /// <summary>
    /// Установленная версия Open Office Writer
    /// Если редактор не установлен, возвращается (0.0.0.0)
    /// Предупреждение. Версия файла soffice.exe может не соответствовать реальной
    /// версии OpenOffice. Например, для Open Office 2.2.0 номер версии будет 1.9
    /// </summary>
    public static Version OpenOfficeWriterVersion
    {
      get
      {
        if (UsedOpenOffice == null)
          return new Version();
        if (UsedOpenOffice.HasWriter)
          return UsedOpenOffice.Version;
        return new Version();
      }
    }

    /// <summary>
    /// Установленная версия Open Office Calc
    /// Если редактор не установлен, возвращается (0.0.0.0)
    /// Предупреждение. Версия файла soffice.exe может не соответствовать реальной
    /// версии OpenOffice. Например, для Open Office 2.2.0 номер версии будет 1.9
    /// </summary>
    public static Version OpenOfficeCalcVersion
    {
      get
      {
        if (UsedOpenOffice == null)
          return new Version();
        if (UsedOpenOffice.HasCalc)
          return UsedOpenOffice.Version;
        return new Version();
      }
    }

    #endregion

    /// <summary>
    /// Этот метод сбрасывает буферизацию информации об установленных офисных программах
    /// </summary>
    public static void ResetExternalProgInfo()
    {
      _MicrosoftWordVersion = null;
      _MicrosoftExcelVersion = null;

      OpenOfficeTools.RefreshInstalls();
      _UsedOpenOffice = null;
      _UsedOpenOfficeDefined = false;
    }

    #endregion

    #region Менеджер конфигурации

    /// <summary>
    /// Менеджер конфигураций по умолчанию, используемый формами и управляющими элементами, если им не назначен собственный менеджер
    /// По умолчанию используется менеджер-пустышка, которая ничего не сохраняет.
    /// Если приложению требуется сохранять пользовательские настройки, свойству следует присвоить значение, 
    /// например, создав объект EFPRegistryConfigManager.
    /// 
    /// Инициализация менеджера настроек должна выполняться до начала работы с интефрейсом (установки свойства EFPApp.Interface
    /// и вызова ShowMainWindow()), иначе окно может получить значения по умолчанию.
    /// 
    /// Свойство никогда не принимает значение null.
    /// </summary>
    public static IEFPConfigManager ConfigManager
    {
      get { return _ConfigManager; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _ConfigManager = value;
      }
    }
    private static IEFPConfigManager _ConfigManager = new EFPDummyConfigManager();

    #endregion

    #region Асинхронные вызовы процедур

    /// <summary>
    /// Список ожидающих завершения асинхронных процедур.
    /// Свойство доступно только из основного потока приложения
    /// </summary>
    public static EFPAppExecProcCallList ExecProcList
    {
      get
      {
        if (!IsMainThread)
          return null;

        if (_ExecProcList == null)
        {
          _ExecProcList = new EFPAppExecProcCallList();
          Timers.Add(_ExecProcList); // навсегда
        }
        return _ExecProcList;
      }
    }
    private static EFPAppExecProcCallList _ExecProcList = null;

    /// <summary>
    /// Количество ожидающих завершения асинхронных процедур.
    /// Свойство доступно только из основного потока приложения
    /// </summary>
    public static int ExecProcCallCount
    {
      get
      {
        if (!IsMainThread)
          return 0;
        if (_ExecProcList == null)
          return 0;
        return _ExecProcList.Count;
      }
    }

    /// <summary>
    /// Возвращает имя значка в MainImages для отображения состояния выполняемой процедуры
    /// </summary>
    /// <param name="procInfo">Информация о процедуре</param>
    /// <returns>Имя изображения</returns>
    public static string GetExecProcStateImageKey(ExecProcInfo procInfo)
    {
      return GetExecProcStateImageKey(procInfo.State, procInfo.ThreadState);
    }

    /// <summary>
    /// Возвращает имя значка в MainImages для отображения состояния выполняемой процедуры
    /// </summary>
    /// <param name="state">Состояние выполнения процедуры</param>
    /// <param name="threadState">Состояние потока</param>
    /// <returns>Имя изображения</returns>
    public static string GetExecProcStateImageKey(ExecProcState state, System.Threading.ThreadState threadState)
    {
      switch (state)
      {
        case ExecProcState.NotStarted: return "New";
        case ExecProcState.Queued: return "HourGlass";
        case ExecProcState.Executing:
          if ((threadState & System.Threading.ThreadState.WaitSleepJoin) == System.Threading.ThreadState.WaitSleepJoin)
            return "Pause";
          threadState &= (System.Threading.ThreadState.Stopped | System.Threading.ThreadState.Suspended | System.Threading.ThreadState.Aborted);
          if (threadState == System.Threading.ThreadState.Running)
            return "Play";
          else
            return "No";

        case ExecProcState.Success: return "Ok";
        case ExecProcState.Error: return "Error";
        case ExecProcState.Disposed: return "Cancel";
        default: return "UnknownState";
      }
    }

    #endregion

    #region Удаленный интерфейс пользователя

    // 27.01.2021
    // Вместо единственного экземпляра RIExecProc, создаем процедуру каждый раз, когда выполняется удаленный вызов.
    // Это позволяет выполнять реентрантный показ блоков диалога.

    /// <summary>
    /// Объект класса, производного от MarshaByRefObject для реализии удаленного интерфейса.
    /// Методы интерфейса могут вызываться только из основного потока приложения.
    /// Ссылка на этот объект должна передаваться конструктору RIExecProc.
    /// </summary>
    public static FreeLibSet.RI.IRemoteInterface RemoteInterface { get { return _RemoteInterface; } }
    private static readonly FreeLibSet.Forms.RI.EFPAppRemoteInterface _RemoteInterface = new FreeLibSet.Forms.RI.EFPAppRemoteInterface();

    ///// <summary>
    ///// Процедура для реализации удаленного интерфейса пользователя
    ///// Может использоваться исключительно для вызовов из основного потока приложения.
    ///// Для процедуры можно вызывать метод CreateProxy() многократно
    ///// </summary>
    //public static ExecProc RemoteUIProc
    //{
    //  get
    //  {
    //    if (!IsMainThread)
    //      return null;
    //    if (_RemoteUIProc == null)
    //    {
    //      _RemoteUIProc = new FreeLibSet.RI.RIExecProc(new FreeLibSet.Forms.RI.EFPAppRemoteInterface());
    //      _RemoteUIProc.DisplayName = "EFPApp.RemoteUIProc";
    //    }
    //    return _RemoteUIProc;
    //  }
    //}
    //private static ExecProc _RemoteUIProc = null;

    private class UICallBackList : List<IExecProcCallBack>, IEFPAppTimeHandler
    {
      #region IEFPAppTimeHandler Members

      public void TimerTick()
      {
        if (EFPApp.ExecProcList.Count == 0)
          return;

        if (EFPApp._InsideProcessUICallBack)
          return;
        EFPApp._InsideProcessUICallBack = true;
        try
        {
          for (int i = 0; i < Count; i++)
          {
            try
            {
              EFPApp.DoProcessUICallBack(this[i]);
            }
            catch (Exception e)
            {
              // 31.08.2017
              // Перехватываем исключение.
              // Не зациклится ли вывод сообщения об ошибке?

              EFPApp.ShowException(e, "Ошибка обработки обратного вызова");
            }
          }
        }
        finally
        {
          EFPApp._InsideProcessUICallBack = false;
        }
      }

      #endregion
    }

    /// <summary>
    /// Список процедур обратного вызова, которые могут получать сигналы для интерфейса пользователя
    /// Используется в модели клиент-сервер, при этом в список добавляется единственный объект, получаемый
    /// от сервера
    /// Доступ к списку возможен только из основного потока приложения
    /// Опрос выполняется, если AsyncProcList содержит выполняющиеся процедуры
    /// </summary>
    public static ICollection<IExecProcCallBack> RemoteUICallBacks
    {
      get
      {
        if (!IsMainThread)
          return null;
        if (_RemoteUICallBacks == null)
        {
          _RemoteUICallBacks = new UICallBackList();
          EFPApp.Timers.Add(_RemoteUICallBacks); // навсегда
        }
        return _RemoteUICallBacks;
      }
    }
    private static UICallBackList _RemoteUICallBacks = null;

    /// <summary>
    /// Количество процедур обратного вызова, которые могут получать сигналы для интерфейса пользователя
    /// Свойство доступно только из основного потока приложения. Из других потоков возвращается 0.
    /// </summary>
    public static int RemoteUICallBackCount
    {
      get
      {
        if (!IsMainThread)
          return 0;

        if (_RemoteUICallBacks == null)
          return 0;
        return _RemoteUICallBacks.Count;
      }
    }

    /// <summary>
    /// Обработать обратный вызов для процедуры интерфейса
    /// </summary>
    /// <param name="callBack">Процедура обратного вызова</param>
    public static void ProcessUICallBack(IExecProcCallBack callBack)
    {
#if DEBUG
      if (callBack == null)
        throw new ArgumentNullException("callBack");
#endif

      if (_InsideProcessUICallBack)
        return;

      _InsideProcessUICallBack = true;
      try
      {
        DoProcessUICallBack(callBack);
      }
      finally
      {
        _InsideProcessUICallBack = false;
      }
    }

    private static void DoProcessUICallBack(IExecProcCallBack callBack)
    {
      NamedValues args = callBack.GetSuspended();
      if (args != null)
      {
        NamedValues res = null;
        using (FreeLibSet.RI.RIExecProc proc = new FreeLibSet.RI.RIExecProc(RemoteInterface))
        {
          try
          {
            res = proc.Execute(args);
          }
          catch (Exception e) // 10.04.2018
          {
            // Не надо. Исключение будет показано когда-нибудь потом. EFPApp.ShowException(e, "Ошибка вызова процедуры интерфейса");
            callBack.SetException(e);
          }
        }
        if (res != null)
          callBack.Resume(res);
      }
    }

    /// <summary>
    /// Предотвращение вложенного вызова ProcessUICallBack
    /// </summary>
    internal static bool _InsideProcessUICallBack = false;

    /// <summary>
    /// Список генераторов управляющих элементов удаленного интерфейса
    /// По умолчанию поддерживает создание всех удаленных элементов, реализованных в ExtTools.
    /// Если требуется реализовать собственные удаленные управляющие элементы, то следует создать
    /// классы, производные от RIItem, затем создать собственный генератор, реализующий интерфейс IEFPAppRICreator,
    /// и добавить его в список. Если используется расширение существующего класса, то генератор должен быть
    /// добавлен в начало списка методом Insert()
    /// </summary>
    public static FreeLibSet.Forms.RI.EFPAppRICreators RICreators
    {
      get
      {
        if (!IsMainThread)
          return null;
        if (_RICreators == null)
        {
          _RICreators = new FreeLibSet.Forms.RI.EFPAppRICreators();
          // Генераторы стандартных элементов интерфейса
          _RICreators.Add(new FreeLibSet.Forms.RI.RIDialogCreator());
          _RICreators.Add(new FreeLibSet.Forms.RI.RIControlCreator());
          // Генераторы стандартных блоков диалога
          _RICreators.Add(new FreeLibSet.Forms.RI.RIStandardDialogCreator());
        }
        return _RICreators;
      }
    }
    private static FreeLibSet.Forms.RI.EFPAppRICreators _RICreators = null;

    #endregion

    #region Создание миниатюрного изображения интерфейса

    /// <summary>
    /// Максимальный размер изображения, возвращаемый CreateSnapshot().
    /// По умолчанию - 560x360 (примерно по размеру области просмотра в окне выбора композиции)
    /// </summary>
    public static Size SnapshotSize
    {
      get { return _SnapshotSize; }
      set
      {
        if (value.Width < 16 || value.Height < 16)
          throw new ArgumentOutOfRangeException();
        _SnapshotSize = value;
      }
    }
    private static Size _SnapshotSize = new Size(560, 360);

    /// <summary>
    /// Создание изображения для предварительного просмотра интерфейса.
    /// Возвращаемое изображение вписывается в размер, задаваемый свойством SnapshotSize.
    /// В текущей реализации не учитываются блоки диалога и окна, выведенные вне системы EFPAppInerface
    /// </summary>
    /// <param name="forComposition">Если true, то будут сохранены только те, окна,
    /// которые могут быть записаны в композицию окон.
    /// Если false, то будут включены все окна</param>
    /// <returns>Уменьшенное изображение всех открытых окон</returns>
    public static Bitmap CreateSnapshot(bool forComposition)
    {
      if (EFPApp.Interface == null)
        return new Bitmap(1, 1); // пустышка

      #region Определение размеров занимаемой области экрана

      // Выделяем место под все окна, невзирая на ForComposition

      Rectangle wholeArea = Rectangle.Empty;
      EFPAppMainWindowLayout[] layouts = EFPApp.Interface.GetMainWindowLayouts(true); // порядок окон важен

      if (layouts.Length == 0)
        return new Bitmap(1, 1); // пустышка

      for (int i = 0; i < layouts.Length; i++)
      {
        Rectangle bounds;
        if (layouts[i].MainWindow.WindowState == FormWindowState.Minimized)
          bounds = layouts[i].MainWindow.RestoreBounds;
        else
          bounds = layouts[i].MainWindow.Bounds;
        if (i == 0)
          wholeArea = bounds;
        else
          wholeArea = Rectangle.Union(wholeArea, bounds);
      }

      #endregion

      #region Копирование изображений

      Bitmap bmp = new Bitmap(wholeArea.Width, wholeArea.Height);

      using (Graphics g = Graphics.FromImage(bmp))
      {
        g.FillRectangle(new SolidBrush(/*SystemColors.Desktop*/ Color.Gray), new Rectangle(0, 0, wholeArea.Width, wholeArea.Height));
      }

      for (int i = layouts.Length - 1; i >= 0; i--)
      {
        if (forComposition && EFPApp.Interface.IsSDI)
        {
          if (layouts[i].ChildFormCount == 0)
            continue; // пустышка

          if (!FormWantsSaveComposition(layouts[i].CurrentChildForm))
            continue;
        }

        Rectangle rc; // координаты относительно экрана
        if (layouts[i].MainWindow.WindowState == FormWindowState.Minimized)
          rc = layouts[i].MainWindow.RestoreBounds;
        else
          rc = layouts[i].MainWindow.Bounds;
        rc.Offset(-wholeArea.Left, -wholeArea.Top);

        if (layouts[i].MainWindow.WindowState == FormWindowState.Minimized)
        {
          // Временно восстанавливаем окно
          layouts[i].MainWindow.WindowState = FormWindowState.Normal;
          layouts[i].DrawMainWindowSnapshot(bmp, rc, forComposition);
          layouts[i].MainWindow.WindowState = FormWindowState.Minimized;
        }
        else
          layouts[i].DrawMainWindowSnapshot(bmp, rc, forComposition);
      }

      #endregion

      #region Уменьшаем изображение

      Size newSize;
      if (ImagingTools.IsImageShrinkNeeded(bmp, SnapshotSize, out newSize))
      {
        Bitmap bmp2 = new Bitmap(bmp, newSize);
        bmp.Dispose();
        bmp = bmp2;
      }

      #endregion

      return bmp;
    }

    /// <summary>
    /// Менеджер хранения изображений предварительного просмотра конфигураций интерфейса.
    /// По умолчанию используется EFPSnapshotConfigManager, сохраняющий изображения с помощью ConfigManager в виде строки
    /// в кодировке Base64
    /// </summary>
    public static IEFPSnapshotManager SnapshotManager
    {
      get { return _SnapshotManager; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _SnapshotManager = value;
      }
    }
    private static IEFPSnapshotManager _SnapshotManager = new EFPSnapshotConfigManager();

    #endregion

    #region Tray Icon

    /// <summary>
    /// Управление значков в области уведомлений
    /// </summary>
    public static EFPAppTrayIcon TrayIcon
    {
      get
      {
        if (_TrayIcon == null)
          _TrayIcon = new EFPAppTrayIcon();
        return _TrayIcon;
      }
    }
    private static EFPAppTrayIcon _TrayIcon;

    #endregion

    #region Файловые ассоциации

    /// <summary>
    /// Файловые ассоциации для расширений файлов.
    /// Обеспечивает буферизацию данных
    /// </summary>
    public static EFPAppFileExtAssociations FileExtAssociations { get { return _FileExtAssociations; } }
    private static readonly EFPAppFileExtAssociations _FileExtAssociations = new EFPAppFileExtAssociations();

    #endregion
  }


  /// <summary>
  /// Расширение списка выполняющихся процедур для обработки сигнала таймера.
  /// Реализация свойства EFPApp.ExecProcList
  /// </summary>
  public sealed class EFPAppExecProcCallList : ExecProcCallList, IEFPAppTimeHandler
  {
    #region IEFPAppTimeHandler Members

    /// <summary>
    /// Реализация интерфейса IEFPAppTimeHandler.
    /// </summary>
    public void TimerTick()
    {
      try
      {

        this.UseDefaultSplash = (EFPApp.ActiveDialog == null) && (EFPApp.MessageBoxCount == 0);

        base.Process();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка вызова EFPA");
      }

      // не нужно UpdateSplash();
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Используем управление закладками
    /// </summary>
    /// <returns></returns>
    protected override ISplashStack CreateSplashStack()
    {
      return new SplashStack();
    }

    /// <summary>
    /// Запуск процедуры в текущем потоке с ожиданием завершения.
    /// На время выполнения выводится дополнительная заставка в статусной строке.
    /// </summary>
    /// <param name="item">Объект процедуры</param>
    /// <param name="args">Параметры, передаваемые процедуры</param>
    public override NamedValues ExecuteSync(ExecProcCallItem item, NamedValues args)
    {
      NamedValues res;

      EFPApp.BeginWait("Выполнение " + item.DisplayName);
      try
      {
        res = base.ExecuteSync(item, args);
      }
      finally
      {
        EFPApp.EndWait();
      }

      return res;
    }

    /// <summary>
    /// Асинхронный вызов с ожиданием.
    /// В отличие от метода ExecuteAsync(), который сразу возвращает управление,
    /// вызов этого метода не завершается, пока выполнение не будет завершено.
    /// Метод должен использоваться с удаленной процедурой, которая выполняется долго и может вызвать ошибку тайм-аута при использовании ExecuteSync(),
    /// а использовать ExecuteAsync() нельзя, так как требуется получить результаты в текущем потоке.
    /// Использование с ExecProc также допускается, хотя в некоторых случаях может быть выгоднее использовать ExecuteSync().
    /// Фактически, этот метод вызывает ExecuteAsync() и дожидается завершения процедуры. Затем результаты возвращаются в основном потоке.
    /// Если при выполнении процедуры возникло исключение, то оно перевыбрасывается в основном потоке. Предполагается, что в <paramref name="item"/>
    /// нет своей обработки ошибок.
    /// На время выполнения выводится дополнительная заставка в статусной строке.
    /// </summary>
    /// <param name="item">Объект процедуры</param>
    /// <param name="args">Аргументы процедуры</param>
    /// <returns>Результаты выполнения процедуры</returns>
    public override NamedValues ExecuteAsyncAndWait(ExecProcCallItem item, NamedValues args)
    {
      NamedValues res;
      EFPApp.BeginWait("Выполнение " + item.DisplayName);
      try
      {
        res = base.ExecuteAsyncAndWait(item, args);
      }
      finally
      {
        EFPApp.EndWait();
      }

      return res;
    }

    /// <summary>
    /// Асинхронный вызов с ожиданием.
    /// В отличие от метода ExecuteAsync(), который сразу возвращает управление,
    /// вызов этого метода не завершается, пока выполнение не будет завершено.
    /// Метод должен использоваться с удаленной процедурой, которая выполняется долго и может вызвать ошибку тайм-аута при использовании ExecuteSync(),
    /// а использовать ExecuteAsync() нельзя, так как требуется получить результаты в текущем потоке.
    /// Использование с ExecProc также допускается.
    /// На время выполнения выводится дополнительная заставка в статусной строке.
    /// </summary>
    /// <param name="proc">Выполняемая процедура</param>
    /// <param name="args">Аргументы вызова</param>
    /// <returns>Результаты вызова</returns>
    public override NamedValues ExecuteAsyncAndWait(IExecProc proc, NamedValues args)
    {
      NamedValues res;
      EFPApp.BeginWait("Выполнение " + proc.DisplayName);
      try
      {
        res = base.ExecuteAsyncAndWait(proc, args);
      }
      finally
      {
        EFPApp.EndWait();
      }

      return res;
    }

    /// <summary>
    /// Вызывается, если при выполнении процедуры возникла ошибка.
    /// Вызывает базовый метод, перехватывая исключения и вызывая EFPApp.ShowException().
    /// Если ExecProcCallItem обработал исключение, сообщение не выдается.
    /// </summary>
    /// <param name="item">Описатель выполняемой процедуры</param>
    /// <param name="exception">Объект исключения</param>
    protected override void OnFailed(ExecProcCallItemBase item, Exception exception)
    {
      if (item.IsAsync)
      {
        try
        {
          base.OnFailed(item, exception);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка выполнения " + item.DisplayName);
        }
      }
      else // 02.11.2020
        base.OnFailed(item, exception);
    }

    /// <summary>
    /// Вызывает Appication.DoEvents() для обработки очереди событий. Затем вызывается Thread.Sleep()
    /// </summary>
    /// <param name="milliseconds">Интервал ожидания в милисекундах</param>
    protected override void Sleep(int milliseconds)
    {
      //Thread.Sleep(milliseconds);
      base.Sleep(milliseconds); // пусть проверит поток

      Application.DoEvents();
    }

    /// <summary>
    /// Перед обновлением заставок всех процедур устанавливает свойство UseDefaultSplash
    /// </summary>
    public new void ProcessAll()
    {
      this.UseDefaultSplash = (EFPApp.ActiveDialog == null) && (EFPApp.MessageBoxCount == 0);
      base.ProcessAll();
    }

    #endregion
  }
}
