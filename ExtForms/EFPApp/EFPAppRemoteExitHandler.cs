// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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
  /// Заставка ожидания удаленного завершения работы
  /// </summary>
  internal partial class EFPAppRemoteExitForm : Form, IEFPAppTimeHandler
  {
    #region Конструктор

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
      // Не используем событие EFPFormProvider.UpdateByTime, т.к. оно, теоретически, может не вызываться, если форма закрыта другими окнами.
      // Присоедияем собственный обработчик

      _NewState = EFPAppRemoteExitState.UserCancelled; // на случай, если пользователь просто закроет форму

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

    #region Поля

    public EFPTextBox efpMessage;

    /// <summary>
    /// Новое состояние, которое будет установлено при закрытии формы
    /// </summary>
    private EFPAppRemoteExitState _NewState;

    #endregion

    #region Обработчик формы

    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      if (Visible)
      {
        TimerTick(); // Первоначальная инициализация ползунка. Не может быть в конструкторе
        EFPApp.Timers.Add(this);
      }
      else
      {
        EFPApp.Timers.Remove(this);
        EFPApp.RemoteExitHandler.State = _NewState;
        EFPApp.RemoteExitHandler.TheForm = null; // чтобы ссылка не висела

        // Плохая идея вызывать EFPApp.Exit() прямо сейчас.
        // Лучше это отложить
        if (_NewState == EFPAppRemoteExitState.Exiting)
          EFPApp.Timers.Add(new EFPAppRemoteExitDelayedHandler());

        // Уничтожаем форму
        Dispose();
      }
    }

    public void TimerTick()
    {
      TimeSpan ts = DateTime.Now - EFPApp.RemoteExitHandler.StartTime;
      int remainderTime = EFPApp.RemoteExitHandler.WaitingTime - (int)ts.TotalMilliseconds;
      if (remainderTime < 0)
        remainderTime = 0;
      if (remainderTime > EFPApp.RemoteExitHandler.WaitingTime)
        remainderTime = EFPApp.RemoteExitHandler.WaitingTime;
      EFPApp.RemoteExitHandler.RemainderTime = remainderTime;
      ThePB.Maximum = EFPApp.RemoteExitHandler.WaitingTime; // вдруг поменялось
      ThePB.Value = ThePB.Maximum - remainderTime;
      lblTime.Text = (remainderTime / 1000).ToString();


      if (remainderTime == 0)
      {
        _NewState = EFPAppRemoteExitState.Exiting;
        Hide(); // закрываем форму
      }
    }

    #endregion
  }

  #region Перечисление EFPAppRemoteExitState

  /// <summary>
  /// Значения свойства EFPAppRemoteExitHandler.State
  /// </summary>
  [Serializable]
  public enum EFPAppRemoteExitState
  {
    /// <summary>
    /// Основное состояние.
    /// Запрос на завершение приложения не был получен
    /// </summary>
    NotStarted,

    /// <summary>
    /// Был вызван метод Start.
    /// В данный момент выведена экранная заставка и ожидается реакция пользователя в течение интервала времени WaitingTime 
    /// </summary>
    Started,

    /// <summary>
    /// Пользователь нажал кнопку "Отмена", пока выводилась заставка.
    /// Работа приложения продолжается в штатном режиме
    /// </summary>
    UserCancelled,

    /// <summary>
    /// Сервер "передумал" и отменил завершение приложения в течение интервала ожидания.
    /// Это свойство устанавливается вызовом Cancel().
    /// Работа приложения продолжается в штатном режиме
    /// </summary>
    ExternalCancelled,

    /// <summary>
    /// Завершение отменено обработчиком события BeforeStart или BeforeExit
    /// </summary>
    InternalCancelled,

    /// <summary>
    /// Временное состояние, устанавливаемое на время вызова EFPApp.Exit
    /// </summary>
    Exiting,

    /// <summary>
    /// Работа приложения была успешно завершена
    /// </summary>
    Exited,

    /// <summary>
    /// Работа приложения не была завершена. Вызов EFPApp.Exit() вернул false.
    /// При этом часть функций приложения может оказаться недоступной
    /// </summary>
    ExitError,
  }

  #endregion


  /// <summary>
  /// Удаленное завершение работы, инициируемое сервером.
  /// Реализует свойство EFPApp.RemoteExitHandler.
  /// Доступ к объекту возможен только из основного потока приложения
  /// </summary>
  public sealed class EFPAppRemoteExitHandler
  {
    #region Защищенный конструктор

    internal EFPAppRemoteExitHandler()
    {
      _WaitingTime = 30000;
      _State = EFPAppRemoteExitState.NotStarted;
    }

    #endregion

    #region Свойства, устанавливаемые до запуска

    /// <summary>
    /// Время ожидания реакции пользователя (вывода заставки) в миллисекундах.
    /// По умолчанию равно 30000 (30 секунд).
    /// В текущей реализации точность составляет 1 секунду
    /// </summary>
    public int WaitingTime
    {
      get { return _WaitingTime; }
      set { _WaitingTime = value; }
    }
    private int _WaitingTime;

    /// <summary>
    /// Информационное сообщение, переданное сервером.
    /// Описывает причину, по которой требуется завершить работу приложения.
    /// Свойство может не устанавливаться или устанавливаться в процессе показа формы
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
    /// Контекст справки для формы ожидания.
    /// Свойство может устаналвиваться только до первого вызова Start()
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set
      {
        if (State != EFPAppRemoteExitState.NotStarted)
          throw new InvalidOperationException("Свойство должно устанавливаться до первого вызова Start()");
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    #endregion

    #region Свойства текущего состояния

    /// <summary>
    /// Текущее состояние
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
    /// Упрощенное значение для текущего состояние.
    /// Возвращает true, если состояние равно Started или Exiting.
    /// Установка свойства вызывает метод Start() или Cancel()
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
    /// Форма с заставкой, если она сейчас выведена
    /// </summary>
    internal EFPAppRemoteExitForm TheForm;

    /// <summary>
    /// Время (последнего) вызова метода Start(). Вложенные вызовы Start() не учитываются.
    /// Если State=NotStarted, возвращаемое значение не имеет смысла
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private DateTime _StartTime;

    /// <summary>
    /// Количество вызовом методов Start(). Вложенные вызовы Start() не учитываются.
    /// Значение может быть больше 1, если пользователь нажимал кнопку "Отмена" в заставке или приложение не было закрыто, например, из-за наличия несохраненных данных.
    /// Свойство возвращает количество появлений на экране окна-заставки
    /// </summary>
    public int StartCount { get { return _StartCount; } }
    private int _StartCount;

    /// <summary>
    /// Время, оставшееся до закрытия окна заставки.
    /// Свойство имеет значение только в состоянии State=Started
    /// </summary>
    public int RemainderTime
    {
      get { return _RemainderTime; }
      internal set { _RemainderTime = value; }
    }
    private int _RemainderTime;

    #endregion

    #region Остановка и запуск

    /// <summary>
    /// Инициализирует завершение работы приложения.
    /// Выводит на экран окно-заставку, в которой начинает уменьшаться отсчет таймера от WaitingTime до 0.
    /// Устанавливается состояние State=Started.
    /// Повторный вызов в процессе показа заставки не выполняет никаких действий. Таймер не продлевается.
    /// Этот метод сразу возвращает управление
    /// </summary>
    public void Start()
    {
      switch (State)
      {
        case EFPAppRemoteExitState.Started:
        case EFPAppRemoteExitState.Exiting:
          return; // повторные вызовы отбрасываем
      }

      _StartTime = DateTime.Now;
      _StartCount++;

      State = EFPAppRemoteExitState.Started;

      if (BeforeStart != null)
      {
        CancelEventArgs args = new CancelEventArgs();
        BeforeStart(this, args);
        if (args.Cancel)
        {
          State = EFPAppRemoteExitState.InternalCancelled;
          return;
        }
      }

      TheForm = new EFPAppRemoteExitForm();
      InitFormMessage();

      // При активации приложения отключается экранная заставка и режим энергосбережения
      EFPApp.Activate();
      _RemainderTime = WaitingTime;
      EFPApp.ShowFormInternal(TheForm); // Запуск
    }

    /// <summary>
    /// Если запущен процесс завершения, то отменяет его
    /// </summary>
    public void Cancel()
    {
      if (State == EFPAppRemoteExitState.Started)
      {
        EFPAppRemoteExitForm frm = TheForm;
        if (frm != null)
          frm.Hide(); // делает TheForm=null
        State = EFPAppRemoteExitState.ExternalCancelled;
      }
    }

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при изменении значения свойства State
    /// </summary>
    public event EventHandler StateChanged;

    private void OnStateChanged()
    {
      if (StateChanged != null)
        StateChanged(this, EventArgs.Empty);
    }


    /// <summary>
    /// Это событие вызывается при вызове метода Start(), если он не является вложенным.
    /// Если обработчик установит свойство Cancel=true, то заставка выводится не будет, и State будет установлено в InternalCancelled
    /// </summary>
    public event CancelEventHandler BeforeStart;

    /// <summary>
    /// Это событие вызывается после того, как отработало окно ожидания, перед вызовом EFPApp.Exit().
    /// Если обработчик установит свойство Cancel=true, то вызова Exit() не будет, и State будет установлено в InternalCancelled
    /// </summary>
    public event CancelEventHandler BeforeExit;

    internal bool OnBeforeExit()
    {
      if (BeforeExit == null)
        return true;

      CancelEventArgs args = new CancelEventArgs();
      BeforeExit(this, args);
      return !args.Cancel;
    }

    /// <summary>
    /// Это событие вызывается после вызова EFPApp.Exit(), независимо от результатов вызова.
    /// Чтобы определить успешность вызова, следует анализировать свойство State. Оно может иметь значение Exited или ExitError
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
  /// Отложенный вызов EFPApp.Exit()
  /// </summary>
  internal class EFPAppRemoteExitDelayedHandler : IEFPAppTimeHandler
  {
    #region IEFPAppTimeHandler Members

    public void TimerTick()
    {
      // Это действие выполняется только один раз
      EFPApp.Timers.Remove(this);

      if (EFPApp.RemoteExitHandler.State != EFPAppRemoteExitState.Exiting)
        return; // прошел вызов Cancel()

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
        EFPApp.ShowException(e, "Не удалось заверить приложение");
        EFPApp.RemoteExitHandler.State = EFPAppRemoteExitState.ExitError;
      }

      EFPApp.RemoteExitHandler.OnAfterExit();
    }

    #endregion
  }
}