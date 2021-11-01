using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// �������� �������� ���������� ���������� ������
  /// </summary>
  internal partial class EFPAppRemoteExitForm : Form, IEFPAppTimeHandler
  {
    #region �����������

    public EFPAppRemoteExitForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("Exit");

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpForm.HelpContext = EFPApp.RemoteExitHandler.HelpContext;
      efpMessage = new EFPTextBox(efpForm, edMessage);

      btnClose.Image = EFPApp.MainImages.Images["Exit"];
      btnClose.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpClose = new EFPButton(efpForm, btnClose);
      efpClose.Click += new EventHandler(efpClose_Click);

      btnCancel.Image = EFPApp.MainImages.Images["Cancel"];
      btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpCancel = new EFPButton(efpForm, btnCancel);
      efpCancel.Click += new EventHandler(efpCancel_Click);


      //ThePB.Maximum = EFPApp.RemoteExitHandler.WaitingTime;
      // �� ���������� ������� EFPFormProvider.UpdateByTime, �.�. ���, ������������, ����� �� ����������, ���� ����� ������� ������� ������.
      // ����������� ����������� ����������

      _NewState = EFPAppRemoteExitState.UserCancelled; // �� ������, ���� ������������ ������ ������� �����

    }

    void efpClose_Click(object sender, EventArgs args)
    {
      _NewState = EFPAppRemoteExitState.Exiting;
      Hide();
    }

    void efpCancel_Click(object sender, EventArgs args)
    {
      _NewState = EFPAppRemoteExitState.UserCancelled;
      Hide();
    }

    #endregion

    #region ����

    public EFPTextBox efpMessage;

    /// <summary>
    /// ����� ���������, ������� ����� ����������� ��� �������� �����
    /// </summary>
    private EFPAppRemoteExitState _NewState;

    #endregion

    #region ���������� �����

    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      if (Visible)
      {
        TimerTick(); // �������������� ������������� ��������. �� ����� ���� � ������������
        EFPApp.Timers.Add(this);
      }
      else
      {
        EFPApp.Timers.Remove(this);
        EFPApp.RemoteExitHandler.State = _NewState;
        EFPApp.RemoteExitHandler.TheForm = null; // ����� ������ �� ������

        // ������ ���� �������� EFPApp.Exit() ����� ������.
        // ����� ��� ��������
        if (_NewState == EFPAppRemoteExitState.Exiting)
          EFPApp.Timers.Add(new EFPAppRemoteExitDelayedHandler());

        // ���������� �����
        Dispose();
      }
    }

    public void TimerTick()
    {
      TimeSpan ts = DateTime.Now - EFPApp.RemoteExitHandler.StartTime;
      int RemainderTime = EFPApp.RemoteExitHandler.WaitingTime - (int)ts.TotalMilliseconds;
      if (RemainderTime < 0)
        RemainderTime = 0;
      if (RemainderTime > EFPApp.RemoteExitHandler.WaitingTime)
        RemainderTime = EFPApp.RemoteExitHandler.WaitingTime;
      EFPApp.RemoteExitHandler.RemainderTime = RemainderTime;
      ThePB.Maximum = EFPApp.RemoteExitHandler.WaitingTime; // ����� ����������
      ThePB.Value = ThePB.Maximum - RemainderTime;
      lblTime.Text = (RemainderTime / 1000).ToString();


      if (RemainderTime == 0)
      {
        _NewState = EFPAppRemoteExitState.Exiting;
        Hide(); // ��������� �����
      }
    }

    #endregion
  }

  #region ������������ EFPAppRemoteExitState

  /// <summary>
  /// �������� �������� EFPAppRemoteExitHandler.State
  /// </summary>
  [Serializable]
  public enum EFPAppRemoteExitState
  {
    /// <summary>
    /// �������� ���������.
    /// ������ �� ���������� ���������� �� ��� �������
    /// </summary>
    NotStarted,

    /// <summary>
    /// ��� ������ ����� Start.
    /// � ������ ������ �������� �������� �������� � ��������� ������� ������������ � ������� ��������� ������� WaitingTime 
    /// </summary>
    Started,

    /// <summary>
    /// ������������ ����� ������ "������", ���� ���������� ��������.
    /// ������ ���������� ������������ � ������� ������
    /// </summary>
    UserCancelled,

    /// <summary>
    /// ������ "���������" � ������� ���������� ���������� � ������� ��������� ��������.
    /// ��� �������� ��������������� ������� Cancel().
    /// ������ ���������� ������������ � ������� ������
    /// </summary>
    ExternalCancelled,

    /// <summary>
    /// ���������� �������� ������������ ������� BeforeStart ��� BeforeExit
    /// </summary>
    InternalCancelled,

    /// <summary>
    /// ��������� ���������, ��������������� �� ����� ������ EFPApp.Exit
    /// </summary>
    Exiting,

    /// <summary>
    /// ������ ���������� ���� ������� ���������
    /// </summary>
    Exited,

    /// <summary>
    /// ������ ���������� �� ���� ���������. ����� EFPApp.Exit() ������ false.
    /// ��� ���� ����� ������� ���������� ����� ��������� �����������
    /// </summary>
    ExitError,
  }

  #endregion


  /// <summary>
  /// ��������� ���������� ������, ������������ ��������.
  /// ��������� �������� EFPApp.RemoteExitHandler.
  /// ������ � ������� �������� ������ �� ��������� ������ ����������
  /// </summary>
  public sealed class EFPAppRemoteExitHandler
  {
    #region ���������� �����������

    internal EFPAppRemoteExitHandler()
    {
      _WaitingTime = 30000;
      _State = EFPAppRemoteExitState.NotStarted;
    }

    #endregion

    #region ��������, ��������������� �� �������

    /// <summary>
    /// ����� �������� ������� ������������ (������ ��������) � �������������.
    /// �� ��������� ����� 30000 (30 ������).
    /// � ������� ���������� �������� ���������� 1 �������
    /// </summary>
    public int WaitingTime
    {
      get { return _WaitingTime; }
      set { _WaitingTime = value; }
    }
    private int _WaitingTime;

    /// <summary>
    /// �������������� ���������, ���������� ��������.
    /// ��������� �������, �� ������� ��������� ��������� ������ ����������.
    /// �������� ����� �� ��������������� ��� ��������������� � �������� ������ �����
    /// 
    /// </summary>
    public string Message
    {
      get { return _Message; }
      set
      {
        if (value == _Message)
          return;
        _Message = value;
        InitFormMessage();
      }
    }
    private string _Message;

    private void InitFormMessage()
    {
      if (TheForm != null)
      {
        TheForm.efpMessage.Text = Message;
        TheForm.grpMessage.Visible = !String.IsNullOrEmpty(Message);
      }
    }

    /// <summary>
    /// �������� ������� ��� ����� ��������.
    /// �������� ����� ��������������� ������ �� ������� ������ Start()
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set
      {
        if (State != EFPAppRemoteExitState.NotStarted)
          throw new InvalidOperationException("�������� ������ ��������������� �� ������� ������ Start()");
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    #endregion

    #region �������� �������� ���������

    /// <summary>
    /// ������� ���������
    /// </summary>
    public EFPAppRemoteExitState State
    {
      get { return _State; }
      internal set
      {
        if (value == _State)
          return;
        _State = value;
        OnStateChanged();
      }
    }
    private EFPAppRemoteExitState _State;

    /// <summary>
    /// ���������� �������� ��� �������� ���������.
    /// ���������� true, ���� ��������� ����� Started ��� Exiting.
    /// ��������� �������� �������� ����� Start() ��� Cancel()
    /// </summary>
    public bool Active
    {
      get { return _State == EFPAppRemoteExitState.Started || _State == EFPAppRemoteExitState.Exiting; }
      set
      {
        if (value)
          Start();
        else
          Cancel();
      }
    }

    /// <summary>
    /// ����� � ���������, ���� ��� ������ ��������
    /// </summary>
    internal EFPAppRemoteExitForm TheForm;

    /// <summary>
    /// ����� (����������) ������ ������ Start(). ��������� ������ Start() �� �����������.
    /// ���� State=NotStarted, ������������ �������� �� ����� ������
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private DateTime _StartTime;

    /// <summary>
    /// ���������� ������� ������� Start(). ��������� ������ Start() �� �����������.
    /// �������� ����� ���� ������ 1, ���� ������������ ������� ������ "������" � �������� ��� ���������� �� ���� �������, ��������, ��-�� ������� ������������� ������.
    /// �������� ���������� ���������� ��������� �� ������ ����-��������
    /// </summary>
    public int StartCount { get { return _StartCount; } }
    private int _StartCount;

    /// <summary>
    /// �����, ���������� �� �������� ���� ��������.
    /// �������� ����� �������� ������ � ��������� State=Started
    /// </summary>
    public int RemainderTime
    {
      get { return _RemainderTime; }
      internal set { _RemainderTime = value; }
    }
    private int _RemainderTime;

    #endregion

    #region ��������� � ������

    /// <summary>
    /// �������������� ���������� ������ ����������.
    /// ������� �� ����� ����-��������, � ������� �������� ����������� ������ ������� �� WaitingTime �� 0.
    /// ��������������� ��������� State=Started.
    /// ��������� ����� � �������� ������ �������� �� ��������� ������� ��������. ������ �� ������������.
    /// ���� ����� ����� ���������� ����������
    /// </summary>
    public void Start()
    {
      switch (State)
      {
        case EFPAppRemoteExitState.Started:
        case EFPAppRemoteExitState.Exiting:
          return; // ��������� ������ �����������
      }

      _StartTime = DateTime.Now;
      _StartCount++;

      State = EFPAppRemoteExitState.Started;

      if (BeforeStart != null)
      {
        CancelEventArgs Args = new CancelEventArgs();
        BeforeStart(this, Args);
        if (Args.Cancel)
        {
          State = EFPAppRemoteExitState.InternalCancelled;
          return;
        }
      }

      TheForm = new EFPAppRemoteExitForm();
      InitFormMessage();

      // ��� ��������� ���������� ����������� �������� �������� � ����� ����������������
      EFPApp.Activate();
      _RemainderTime = WaitingTime;
      EFPApp.ShowFormInternal(TheForm); // ������
    }

    /// <summary>
    /// ���� ������� ������� ����������, �� �������� ���
    /// </summary>
    public void Cancel()
    {
      if (State == EFPAppRemoteExitState.Started)
      {
        EFPAppRemoteExitForm frm = TheForm;
        if (frm != null)
          frm.Hide(); // ������ TheForm=null
        State = EFPAppRemoteExitState.ExternalCancelled;
      }
    }

    #endregion

    #region �������

    /// <summary>
    /// ������� ���������� ��� ��������� �������� �������� State
    /// </summary>
    public event EventHandler StateChanged;

    private void OnStateChanged()
    {
      if (StateChanged != null)
        StateChanged(this, EventArgs.Empty);
    }


    /// <summary>
    /// ��� ������� ���������� ��� ������ ������ Start(), ���� �� �� �������� ���������.
    /// ���� ���������� ��������� �������� Cancel=true, �� �������� ��������� �� �����, � State ����� ����������� � InternalCancelled
    /// </summary>
    public event CancelEventHandler BeforeStart;

    /// <summary>
    /// ��� ������� ���������� ����� ����, ��� ���������� ���� ��������, ����� ������� EFPApp.Exit().
    /// ���� ���������� ��������� �������� Cancel=true, �� ������ Exit() �� �����, � State ����� ����������� � InternalCancelled
    /// </summary>
    public event CancelEventHandler BeforeExit;

    internal bool OnBeforeExit()
    {
      if (BeforeExit == null)
        return true;

      CancelEventArgs Args = new CancelEventArgs();
      BeforeExit(this, Args);
      return !Args.Cancel;
    }

    /// <summary>
    /// ��� ������� ���������� ����� ������ EFPApp.Exit(), ���������� �� ����������� ������.
    /// ����� ���������� ���������� ������, ������� ������������� �������� State. ��� ����� ����� �������� Exited ��� ExitError
    /// </summary>
    public event EventHandler AfterExit;

    internal void OnAfterExit()
    {
      if (AfterExit != null)
        AfterExit(this, EventArgs.Empty);
    }

    #endregion
  }

  /// <summary>
  /// ���������� ����� EFPApp.Exit()
  /// </summary>
  internal class EFPAppRemoteExitDelayedHandler : IEFPAppTimeHandler
  {
    #region IEFPAppTimeHandler Members

    public void TimerTick()
    {
      // ��� �������� ����������� ������ ���� ���
      EFPApp.Timers.Remove(this);

      if (EFPApp.RemoteExitHandler.State != EFPAppRemoteExitState.Exiting)
        return; // ������ ����� Cancel()

      if (!EFPApp.RemoteExitHandler.OnBeforeExit())
      {
        EFPApp.RemoteExitHandler.State = EFPAppRemoteExitState.InternalCancelled;
        return;
      }
      try
      {
        if (EFPApp.Exit())
          EFPApp.RemoteExitHandler.State = EFPAppRemoteExitState.Exited;
        else
          EFPApp.RemoteExitHandler.State = EFPAppRemoteExitState.ExitError;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "�� ������� �������� ����������");
        EFPApp.RemoteExitHandler.State = EFPAppRemoteExitState.ExitError;
      }

      EFPApp.RemoteExitHandler.OnAfterExit();
    }

    #endregion
  }
}