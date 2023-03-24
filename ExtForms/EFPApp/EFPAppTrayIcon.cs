// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Значок в системном трее.
  /// Реализует свойство EFPApp.TrayIcon      
  /// </summary>
  public sealed class EFPAppTrayIcon : DisposableObject, IReadOnlyObject/*, IEFPAppTimeHandler*/
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    internal EFPAppTrayIcon()
    {
      _Icon = WinFormsTools.AppIcon;
      _Text = EnvironmentTools.ApplicationName;
      _CommandItems = new EFPCommandItems();
      _UseHiddenForm = true;
      //FForAllUsers = true;
      _NotifyIcons = new Dictionary<int, NotifyIcon>();
    }

    /// <summary>
    /// Удаляет значок, если он выведен на экран.
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      Hide();
      if (_TheHiddenForm != null)
      {
        _TheHiddenForm.Dispose();
        _TheHiddenForm = null;
      }
      if (_NotifyIcons != null)
      {
        foreach (NotifyIcon ni in _NotifyIcons.Values)
        {
          try
          {
            ni.Dispose();
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "Ошибка удаления NotifyIcon");
          }
        }
        _NotifyIcons = null;
      }

      //Microsoft.Win32.SystemEvents.SessionSwitch -= new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
      //EFPApp.Timers.Remove(this);
      base.Dispose(disposing);
    }

    #endregion

    #region Значок

    /// <summary>
    /// Изображение для значка.
    /// По умолчанию содержит значок приложения.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public Icon Icon
    {
      get { return _Icon; }
      set
      {
        if (Object.ReferenceEquals(value, _Icon))
          return;
        _Icon = value;
        foreach (NotifyIcon ni in _NotifyIcons.Values)
          ni.Icon = value;
      }
    }
    private Icon _Icon;

    #endregion

    #region Подсказка

    /// <summary>
    /// Текст всплывающей подсказки для значка.
    /// По умолчанию содержит имя приложения.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        _Text = value;
        foreach (NotifyIcon ni in _NotifyIcons.Values)
          ni.Text = value;
      }
    }
    private string _Text;

    #endregion

    #region Меню

    /// <summary>
    /// Команды меню, появляющегося при нажатии правой кнопки мыши на значке.
    /// По умолчанию список команд пустой.
    /// Список может заполняться только до показа значка.
    /// </summary>
    public EFPCommandItems CommandItems { get { return _CommandItems; } }
    private EFPCommandItems _CommandItems;

    /// <summary>
    /// Команда, вызываемая при двойном щелчке мыши на значке.
    /// Если свойство не установлено в явном виде, то оно возвращает первую команду в списке 
    /// CommandItems или null, если список пустой.
    /// Свойство должно устанавливаться до вывода значка на экран.
    /// </summary>
    public EFPCommandItem DefaultCommandItem
    {
      get
      {
        if (_DefaultCommandItemDefined)
          return _DefaultCommandItem;
        else if (CommandItems.Count == 0)
          return null;
        else
        {
          foreach (EFPCommandItem ci in CommandItems)
          {
            if (ci.HasClick)
              return ci;
          }
          return null;
        }
      }
      set
      {
        CheckNotReadOnly();
        _DefaultCommandItem = value;
        _DefaultCommandItemDefined = true;
      }
    }
    private EFPCommandItem _DefaultCommandItem;
    private bool _DefaultCommandItemDefined;

    #endregion

    #region Доступ из других рабочих столов

    ///// <summary>
    ///// Если true (по умолчанию), то значок будет показываться на рабочих столах других пользователей,
    ///// если они войдут в систему.
    ///// Если false, то у других пользователей не будет значка.
    ///// </summary>
    //public bool ForAllUsers
    //{
    //  get { return FForAllUsers; }
    //  set 
    //  {
    //    CheckNotDisposed();
    //    FForAllUsers = value; 
    //  }
    //}
    //private bool FForAllUsers;

    //void SystemEvents_SessionSwitch(object Sender, Microsoft.Win32.SessionSwitchEventArgs Args)
    //{
    //  switch (Args.Reason)
    //  { 
    //    case Microsoft.Win32.SessionSwitchReason.SessionLogon:
    //    case Microsoft.Win32.SessionSwitchReason.SessionUnlock:
    //    case Microsoft.Win32.SessionSwitchReason.RemoteConnect:
    //    case Microsoft.Win32.SessionSwitchReason.ConsoleConnect:
    //      int Key = GetKey();
    //      EFPApp.MessageBox("Получен сигнал Args.Reason="+Args.Reason.ToString()+" для SessionId=" + Key.ToString());
    //      if (!NotifyIcons.ContainsKey(Key))
    //        NotifyIcons.Add(Key, CreateNotifyIcon());
    //      break;
    //  }
    //}

    //private int PrevSessionId;

    //void IEFPAppTimeHandler.TimerTick()
    //{
    //  int SessionId = EnvironmentTools.ActiveConsoleSessionId;
    //  if (SessionId != PrevSessionId)
    //  {
    //    Console.WriteLine(DateTime.Now.ToString("G")+". Обнаружено переключение SessionId с " + PrevSessionId.ToString() + " на " + SessionId.ToString()+
    //      ", UserInteractive="+Environment.UserInteractive.ToString());
    //    if (!NotifyIcons.ContainsKey(SessionId))
    //    {
    //      Console.WriteLine("Добавляем значок для SessionId=" + SessionId.ToString());
    //      NotifyIcons.Add(SessionId, CreateNotifyIcon());
    //    }
    //    PrevSessionId = SessionId;
    //  }
    //}


    #endregion

    #region Скрытая форма для получения сообщений

    /// <summary>
    /// Нужно ли создавать скрытую форму (true) или у приложения есть «нормальная» форма (false). 
    /// Если у приложения не будет формы, то Application.Run() сразу завершится.
    /// По умолчанию – true – класс поддерживает скрытую форму.
    /// </summary>
    public bool UseHiddenForm
    {
      get { return _UseHiddenForm; }
      set
      {
        CheckNotReadOnly();
        _UseHiddenForm = value;
      }
    }
    private bool _UseHiddenForm;

    private class HiddenForm : Form
    {
      #region Конструктор

      public HiddenForm()
      {
        // Чтобы меньше мигало, делаем форму в углу
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        base.Text = EnvironmentTools.ApplicationName + " - Hidden window for messages";
        StartPosition = FormStartPosition.Manual;
        Location = new Point(0, 0);
        Size = new Size(1, 1);
      }

      #endregion

      #region Обработчики формы

      protected override void WndProc(ref Message m)
      {
        const int WM_QUERYENDSESSION = 0x0011;

        switch (m.Msg)
        {
          case WM_QUERYENDSESSION:
            //Console.Beep(200,200);
            //Console.Beep(400, 200);
            //Show();
            if (!EFPApp.Exit())
            {
              m.Result = new IntPtr(0);
              return;
            }
            break;

#if XXXXX
        case WM_POWERBROADCAST:
          // Это событие посылается только в Windows-2000 и Windows XP
          // В Windows Vista и более новых должна использоваться функция
          // SetThreadExecutionState()
          if (m.WParam == PBT_APMQUERYSUSPEND)
          { 
            m.re
          }
          break;
#endif
        }

        base.WndProc(ref m);

        FreeLibSet.Win32.PowerSuspendLocker.WndProc(ref m);
      }

      #endregion
    }

    /// <summary>
    /// Скрытая форма
    /// </summary>
    private HiddenForm _TheHiddenForm;

    #endregion

    #region Видимость значка

    /// <summary>
    /// Если true, то значок выведен на экран, если false, то скрыт. 
    /// Изначально равно false.
    /// Видимость значка можно менять динамически
    /// Можно использовать методы Show() и Hide().
    /// </summary>
    public bool Visible
    {
      get { return _Visible; }
      set
      {
        if (value == _Visible)
          return;
        _Visible = value;
        if (value)
          DoShow();
        else
          DoHide();
      }
    }
    private bool _Visible;

    /// <summary>
    /// Показывает значок. Устанавливает Visible=true
    /// </summary>
    public void Show()
    {
      Visible = true;
    }

    /// <summary>
    /// Прячет значок. Устанавливает Visible=false
    /// </summary>
    public void Hide()
    {
      Visible = false;
    }

    private void DoShow()
    {
      CheckNotDisposed();

      _CommandItems.SetReadOnly();
      if (UseHiddenForm && _TheHiddenForm == null)
      {
        _TheHiddenForm = new HiddenForm();
        // Если форму показать, а затем спрятать, то она будет получать сообщения.
        // А если просто вызвать CreateHandle(), то не будет
        _TheHiddenForm.Show();
        _TheHiddenForm.Hide();
      }

      //int SessionId = EnvironmentTools.ActiveConsoleSessionId;
      if (_NotifyIcons.Count == 0)
      {
        _NotifyIcons.Add(0, CreateNotifyIcon());
        //if (ForAllUsers)
        //{
        //  PrevSessionId = SessionId;
        //  EFPApp.Timers.Add(this);
        //  try
        //  {
        //    //Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        //  }
        //  catch { }
        //}
      }

      foreach (NotifyIcon ni in _NotifyIcons.Values)
        ni.Visible = true;
    }

    private void DoHide()
    {
      try
      {
        foreach (NotifyIcon ni in _NotifyIcons.Values)
          ni.Visible = false;
      }
      catch (ObjectDisposedException) { } // 23.08.2021
    }

    #endregion

    #region Объекты NotifyIcon

    /// <summary>
    /// Ключ - идентификатор SessionId, значение - объект значка
    /// Зарезервировано.
    /// В текущей реализации всегда используется единственный значок, а ключ равен 0
    /// </summary>
    private Dictionary<int, NotifyIcon> _NotifyIcons;

    private NotifyIcon CreateNotifyIcon()
    {
      NotifyIcon ni = new NotifyIcon();
      ni.Icon = Icon;
      ni.Text = Text;
      if (DefaultCommandItem != null)
        //ni.DoubleClick += new EventHandler(DefaultCommandItem.PerformClick);
        // Надо перехватывать повторный щелчок, если команда еще выполняется
        ni.DoubleClick += new EventHandler(ni_DoubleClick);

      if (CommandItems.Count > 0)
      {
        EFPContextMenu cm = new EFPContextMenu();
        cm.Add(CommandItems);
        cm.DefaultCommandItem = DefaultCommandItem;
        ni.ContextMenuStrip = cm.Menu;
      }
      return ni;
    }

    void ni_DoubleClick(object sender, EventArgs args)
    {
      if (DefaultCommandItem == null)
        return; // в принципе, необязательно

      if (DefaultCommandItem.InsideClick)
        return;

      DefaultCommandItem.PerformClick();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если значок уже был выведен на экран, и добавление новых команд в меню не допускается
    /// </summary>
    public bool IsReadOnly { get { return _CommandItems.IsReadOnly; } }

    /// <summary>
    /// Выбрасывает исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}
