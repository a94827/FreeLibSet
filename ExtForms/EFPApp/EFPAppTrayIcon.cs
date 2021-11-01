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
  /// ������ � ��������� ����.
  /// ��������� �������� EFPApp.TrayIcon      
  /// </summary>
  public sealed class EFPAppTrayIcon : DisposableObject, IReadOnlyObject/*, IEFPAppTimeHandler*/
  {
    // ������ ������������ SimpleDisposableObject � �������� �������� ������

    #region ����������� � Dispose

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
    /// ������� ������, ���� �� ������� �� �����.
    /// </summary>
    /// <param name="disposing">true, ���� ������ ����� Dispose(), � �� ����������</param>
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
            LogoutTools.LogoutException(e, "������ �������� NotifyIcon");
          }
        }
        _NotifyIcons = null;
      }

      //Microsoft.Win32.SystemEvents.SessionSwitch -= new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
      //EFPApp.Timers.Remove(this);
      base.Dispose(disposing);
    }

    #endregion

    #region ������

    /// <summary>
    /// ����������� ��� ������.
    /// �� ��������� �������� ������ ����������.
    /// �������� ����� ��������������� �����������.
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

    #region ���������

    /// <summary>
    /// ����� ����������� ��������� ��� ������.
    /// �� ��������� �������� ��� ����������.
    /// �������� ����� ��������������� �����������.
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

    #region ����

    /// <summary>
    /// ������� ����, ������������� ��� ������� ������ ������ ���� �� ������.
    /// �� ��������� ������ ������ ������.
    /// ������ ����� ����������� ������ �� ������ ������.
    /// </summary>
    public EFPCommandItems CommandItems { get { return _CommandItems; } }
    private EFPCommandItems _CommandItems;

    /// <summary>
    /// �������, ���������� ��� ������� ������ ���� �� ������.
    /// ���� �������� �� ����������� � ����� ����, �� ��� ���������� ������ ������� � ������ 
    /// CommandItems ��� null, ���� ������ ������.
    /// �������� ������ ��������������� �� ������ ������ �� �����.
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

    #region ������ �� ������ ������� ������

    ///// <summary>
    ///// ���� true (�� ���������), �� ������ ����� ������������ �� ������� ������ ������ �������������,
    ///// ���� ��� ������ � �������.
    ///// ���� false, �� � ������ ������������� �� ����� ������.
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
    //      EFPApp.MessageBox("������� ������ Args.Reason="+Args.Reason.ToString()+" ��� SessionId=" + Key.ToString());
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
    //    Console.WriteLine(DateTime.Now.ToString("G")+". ���������� ������������ SessionId � " + PrevSessionId.ToString() + " �� " + SessionId.ToString()+
    //      ", UserInteractive="+Environment.UserInteractive.ToString());
    //    if (!NotifyIcons.ContainsKey(SessionId))
    //    {
    //      Console.WriteLine("��������� ������ ��� SessionId=" + SessionId.ToString());
    //      NotifyIcons.Add(SessionId, CreateNotifyIcon());
    //    }
    //    PrevSessionId = SessionId;
    //  }
    //}


    #endregion

    #region ������� ����� ��� ��������� ���������

    /// <summary>
    /// ����� �� ��������� ������� ����� (true) ��� � ���������� ���� ������������ ����� (false). 
    /// ���� � ���������� �� ����� �����, �� Application.Run() ����� ����������.
    /// �� ��������� � true � ����� ������������ ������� �����.
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
      #region �����������

      public HiddenForm()
      {
        // ����� ������ ������, ������ ����� � ����
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        base.Text = EnvironmentTools.ApplicationName + " - Hidden window for messages";
        StartPosition = FormStartPosition.Manual;
        Location = new Point(0, 0);
        Size = new Size(1, 1);
      }

      #endregion

      #region ����������� �����

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
          // ��� ������� ���������� ������ � Windows-2000 � Windows XP
          // � Windows Vista � ����� ����� ������ �������������� �������
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
    /// ������� �����
    /// </summary>
    private HiddenForm _TheHiddenForm;

    #endregion

    #region ��������� ������

    /// <summary>
    /// ���� true, �� ������ ������� �� �����, ���� false, �� �����. 
    /// ���������� ����� false.
    /// ��������� ������ ����� ������ �����������
    /// ����� ������������ ������ Show() � Hide().
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
    /// ���������� ������. ������������� Visible=true
    /// </summary>
    public void Show()
    {
      Visible = true;
    }

    /// <summary>
    /// ������ ������. ������������� Visible=false
    /// </summary>
    public void Hide()
    {
      Visible = false;
    }

    private void DoShow()
    {
      CheckNotDisposed();

      _CommandItems.IsReadOnly = true;
      if (UseHiddenForm && _TheHiddenForm == null)
      {
        _TheHiddenForm = new HiddenForm();
        // ���� ����� ��������, � ����� ��������, �� ��� ����� �������� ���������.
        // � ���� ������ ������� CreateHandle(), �� �� �����
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

    #region ������� NotifyIcon

    /// <summary>
    /// ���� - ������������� SessionId, �������� - ������ ������
    /// ���������������.
    /// � ������� ���������� ������ ������������ ������������ ������, � ���� ����� 0
    /// </summary>
    private Dictionary<int, NotifyIcon> _NotifyIcons;

    private NotifyIcon CreateNotifyIcon()
    {
      NotifyIcon ni = new NotifyIcon();
      ni.Icon = Icon;
      ni.Text = Text;
      if (DefaultCommandItem != null)
        //ni.DoubleClick += new EventHandler(DefaultCommandItem.PerformClick);
        // ���� ������������� ��������� ������, ���� ������� ��� �����������
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
        return; // � ��������, �������������

      if (DefaultCommandItem.InsideClick)
        return;

      DefaultCommandItem.PerformClick();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ������ ��� ��� ������� �� �����, � ���������� ����� ������ � ���� �� �����������
    /// </summary>
    public bool IsReadOnly { get { return _CommandItems.IsReadOnly; } }

    /// <summary>
    /// ����������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}