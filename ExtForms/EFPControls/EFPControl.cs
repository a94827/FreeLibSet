// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Config;
using System.ComponentModel;
using FreeLibSet.Logging;
using FreeLibSet.Collections;
using FreeLibSet.UICore;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using System.Diagnostics;

namespace FreeLibSet.Forms
{
  #region Перечисление EFPControlState

  /// <summary>
  /// Возможные состояния провайдера управляющего элемента.
  /// Свойство EFPControlBase.ProviderState
  /// </summary>
  public enum EFPControlProviderState
  {
    /// <summary>
    /// Исходное состояние. Вызван конструктор класса, но не вызывалось событие Created
    /// </summary>
    Initialization,

    /// <summary>
    /// Вызвано событие Created, но еще не вызывалось событие Attached
    /// </summary>
    Created,

    /// <summary>
    /// Вызвано событие Attached. Элемент присоединен к форме, которая сейчас находится на экране
    /// (при этом свойство Control.Visible может быть false, если элемент был временно или постоянно скрыт)
    /// </summary>
    Attached,

    /// <summary>
    /// Вызвано событие Detached. Обычно вызывается скрытием формы с экрана, но может быть из-за отсоединения EFPBaseProvider
    /// от родительского провайдера. После этого провайдер может снова перейти в состояние Attached, если блок диалога
    /// повторно выводится на экран, или устанавливается свойство EFPBaseProvider.Parent.
    /// </summary>
    Detached,

    /// <summary>
    /// Финальное состояние. Вызвано событие Disposed
    /// </summary>
    Disposed,
  }

  #endregion

  #region Интерфейсы

  /// <summary>
  /// Интерфейс провайдера управляющего элемента.
  /// Реализуется EFPControlBase.
  /// </summary>
  public interface IEFPControl : IUIValidableObject, IEFPAppIdleHandler
  {
    /// <summary>
    /// Базовый провайдер. Не может быть null.
    /// </summary>
    EFPBaseProvider BaseProvider { get; }

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Управляет видимомостью элемента. 
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Связываемое свойство видимости элемента.
    /// </summary>
    DepValue<Boolean> VisibleEx { get; set; }

    /// <summary>
    /// Управляет блокировкой элемента.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Связываемое свойство блокировки элемента.
    /// </summary>
    DepValue<Boolean> EnabledEx { get; set; }

    /// <summary>
    /// Возвращает true, если элемент доступен для воздействия пользователя.
    /// Для обычных элементо возвращает true, если и Visible и Enabled установлены в true.
    /// Для TextBox и других полей ввода, также проверяется, что ReadOnly=false.
    /// </summary>
    bool Editable { get; }

    /// <summary>
    /// Связыываемое свойство Editable
    /// </summary>
    DepValue<Boolean> EditableEx { get; }

    /// <summary>
    /// Событие однократно вызывается перед выводом элемента на экран
    /// </summary>
    event EventHandler Created;

    /// <summary>
    /// Событие вызывается после того, как форма закрывается.
    /// </summary>
    event EventHandler Detached;

    /// <summary>
    /// Свойство возвращает true, если управляющий элемент был выведен на экран
    /// Свойство однократно переходит из false в true. Перед этим вызывается событие Shown
    /// Свойство Control.Visible может многократно изменяться еще до вывода элемента на экран
    /// </summary>
    bool HasBeenCreated { get; }

    /// <summary>
    /// Возвращает true, если для данного вида управляющих элементов желательно использование метки
    /// </summary>
    bool LabelNeeded { get; }

    /// <summary>
    /// Возвращает и позволяет установить метку для управляющего элемента.
    /// Меткой может быть Label или GroupBox.
    /// </summary>
    Control Label { get; set; }

    /// <summary>
    /// Отображаемое имя элемента
    /// </summary>
    string DisplayName { get; set; }

    /// <summary>
    /// Команды локального меню (и панели инструментов, статусной строки) для управляющего элемента.
    /// Не может возвращать null
    /// </summary>
    EFPControlCommandItems CommandItems { get; }

    /// <summary>
    /// Событие вызывается из PrepareCommandItems(), перед тем, как он выполнит инициализацию локального меню,
    /// панелей инструментов и статусной строки.
    /// Пользовательский обработчик может, например, выполнить окончательную настройку команд.
    /// </summary>
    event EventHandler BeforePrepareCommandItems;

    /// <summary>
    /// Панель инструментов для локального меню.
    /// Если управляющий элемент создан без использования ControlWithToolBar, но ему требуется
    /// панель инструментов, то следует установить это свойство.
    /// </summary>
    Panel ToolBarPanel { get; set; }

    /// <summary>
    /// Провайдер панели инструментов.
    /// Создается при установке свойства ToolBarPanel.
    /// </summary>
    EFPPanelToolBar ToolBar { get; }


    /// <summary>
    /// Менеджер чтения и записи конфигурации.
    /// Если не определен специально для этого управляющего элемента, берется из
    /// EFPBaseProvider. Если он не определен явно в цепочке провайдеров до EFPFormProvider,
    /// берется EFPApp.ConfigManager.
    /// Свойство не может возвращать null, т.к. в EFPApp.ConfigManager всегда есть заглушка
    /// </summary>
    IEFPConfigManager ConfigManager { get; }

    /// <summary>
    /// Обработчик конфигурации. Для элементов, не выполняющих сохранение конфигурации, обычно возвращает null
    /// </summary>
    EFPConfigHandler ConfigHandler { get; }

    /// <summary>
    /// Имя секции конфигурации для хранения настроек элемента.
    /// Реализуется объектом EFPConfigHandler.
    /// Если свойство ConfigHandler равно null, ConfigSectionName возвращает пустую строку и установить свойство нельзя.
    /// </summary>
    string ConfigSectionName { get; set; }

    /// <summary>
    /// Подсказка для управляющего элемента - описание назначения управляющего 
    /// элемента (не включая сообщение об ошибке и подсказку по текущему значению)
    /// </summary>
    string ToolTipText { get; set; }

    /// <summary>
    /// Подсказка по текущему значению
    /// Если используется, то свойство должно устанавливаться при изменении 
    /// пользователем введенного значения
    /// </summary>
    string ValueToolTipText { get; set; }

    /// <summary>
    /// Выполнить проверку значений.
    /// Проверка не выполняется, пока форма не выведена на экран. При выводе формы
    /// проверка выполняется для всех управляющих элементов
    /// </summary>
    void Validate();

    /// <summary>
    /// Специальная версия выполнения проверки для управляющего элемента, которую
    /// можно использовать в обработчике изменения значения в другом элементе в виде
    /// efp1.Value.ValueChanged+=new EventHandler(efp2.Validate);
    /// Аргументы игнорируются, вызывается обычная версия Validate() без аргументов
    /// </summary>
    /// <param name="sender">Источник события - игнорируется</param>
    /// <param name="args">Аргументы события - игнорируются</param>
    void Validate(object sender, EventArgs args);

    /// <summary>
    /// Пользовательский обработчик для проверки ошибок
    /// </summary>
    event UIValidatingEventHandler Validating;

    /// <summary>
    /// Если свойство установлено в true, то метод Validate() будет дополнительно вызываться после получения и
    /// потери элементом фокуса ввода.
    /// Вызов метода Validate() выполняется не сразу в обработчике событий Control.Enter и Control.Leave,
    /// а с небольшой задержкой, в обработчике Idle. Соответственно, свойство UseIdle принимает значение true.
    /// По умолчанию свойство имеет значение false и проверка не выполняется.
    /// Установка свойства возможна только до вывода элемента на экран.
    /// </summary>
    bool ValidateWhenFocusChanged { get; set; }

    /// <summary>
    /// Список валидаторов, присоединенных к управляющему элементу
    /// </summary>
    UIValidatorList Validators { get; }

    /// <summary>
    /// Возвращает true, если список Validators содержит элементы
    /// </summary>
    bool HasValidators { get; }

    /// <summary>
    /// Передать фокус ввода управляющему элементу.
    /// Если управляющий элемент находится на вкладке TabPage, которая не активна,
    /// то она активируется
    /// </summary>
    void SetFocus();

    /// <summary>
    /// Передать фокус ввода управляющему элементу и выдать сообщение об ошибке
    /// Если управляющий элемент находится на вкладке TabPage, которая не активна,
    /// то она активируется
    /// </summary>
    /// <param name="errorMessage">Строка сообщения</param>
    void SetFocus(string errorMessage);

    /// <summary>
    /// Сюда можно добавить обработчик для обновления данных по таймеру.
    /// Добавление обработчика возможно только до вывода формы на экран или показа элемента (до вызова OnShown)
    /// </summary>
    ICollection<EFPUpdateByTimeHandler> UpdateByTimeHandlers { get; }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    object Tag { get; set; }
  }

  #endregion

  /// <summary>
  /// Абстрактный базовый класс для шаблонного класса EFPControl.
  /// Реализует свойство Control
  /// </summary>
  public abstract class EFPControlBase : IEFPControl, IUIValidableObject, IEFPStatusBarControl, IEFPAppIdleHandler, IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, TextBox, обычно нужна метка.
    /// Если false, то метка не нужна (например, CheckBox)</param>
    public EFPControlBase(EFPBaseProvider baseProvider, Control control, bool labelNeeded)
    {
#if DEBUG
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.IsDisposed)
        throw new ObjectDisposedException("control");
#endif


      _BaseProvider = baseProvider;
      _BaseProvider.AddControlProvider(this);
      _Control = control;
      _ValidateState = UIValidateState.Ok;
      _LabelNeeded = labelNeeded;
      _Visible = true;
      _PrevVisibleState = false;
      _Enabled = true;
      _PrevEnabledState = true;
      _OrgControlForeColor = control.ForeColor;
      _ProviderState = EFPControlProviderState.Initialization;
      _ConfigSectionName = String.Empty;


      // Автоматическое присоединение метки
      if (labelNeeded)
        FindLabel();

      // Присоединение обработчиков
      if (!DesignMode)
      {
        //Control.VisibleChanged += new EventHandler(Control_VisibleChanged); // без этого никак
        if (control is UserControl)
        {
          for (int i = 0; i < control.Controls.Count; i++)
            control.Controls[i].MouseDown += new MouseEventHandler(Control_MouseDown);
        }
        else
          control.MouseDown += new MouseEventHandler(Control_MouseDown);

        control.Enter += new EventHandler(Control_Enter);

        control.PreviewKeyDown += new PreviewKeyDownEventHandler(Control_PreviewKeyDown); // 04.02.2021

        if (baseProvider.IsFormVisible && Control.Visible)
        {
          // 21.05.2021
          // Создается управляющий элемент для формы, которая в данный момент выведена на экран.
          // Например, элемент располагается на странице отчета EFPReportPage с отложенной инициализацией
          // Свойство Control.Visible не собирается меняться, поэтому метод OnShown() не будет вызван.
          // Нельзя вызвать его сейчас, пока не завершен конструктор класса-наследника.
          // Сделаем это в событии Idle
          EFPApp.IdleHandlers.AddSingleAction(DelayedUpdateFormProviderState);
        }

        control.Disposed += new EventHandler(Control_Disposed);
      }
    }


    void Control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs args)
    {
      // 04.02.2020
      // Комбинации Ctrl-Enter, Shift-Enter, Alt-Enter относятся к управляющему элементу, а не предназначены для нажатия кнопки <ОК>
      if (args.KeyCode == Keys.Enter)
      {
        if (args.Shift || args.Control || args.Alt)
          args.IsInputKey = true;
      }
    }

    #endregion

    #region Простые свойства

    /// <summary>
    /// Список, в который будет добавляться сообщения об ошибке
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Управляющий элемент, к которому относится провайдер.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public Control Control { get { return _Control; } }
    private Control _Control;

    /// <summary>
    /// Не должно использоваться в прикладном коде.
    /// Переопределяется для EFPRadioButtons.
    /// </summary>
    protected virtual Control[] GetControls()
    {
      return new Control[1] { _Control };
    }

    internal bool ContainsControl(Control control)
    {
      return Array.IndexOf<Control>(GetControls(), control) >= 0;
    }

    /// <summary>
    /// Возвращает true, если управляющему элементу полагается метка.
    /// Значение false означает, что состояние проверки отображается только в данном
    /// элементе
    /// Например, для поля TextBox (и большинства других элементов) возвращается true,
    /// а для CheckBox - false.
    /// Задается в конструкторе
    /// </summary>
    public bool LabelNeeded { get { return _LabelNeeded; } }
    private bool _LabelNeeded;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Свойство возвращает true, если компонент Control находится в режиме разработки
    /// </summary>
    public bool DesignMode
    {
      get
      {
        if (_Control.Site == null)
          return false;
        return _Control.Site.DesignMode;
      }
    }

    #endregion

    #region IEFPCheckItem Members

    internal void ParentProviderChanged()
    {
      // Закулисное пересоединение EFPBaseProvider в составном элементе не нужно учитывать
      if (ProviderState != EFPControlProviderState.Initialization || BaseProvider.IsFormVisible)
        UpdateFormProviderState();
    }

    internal void FormVisibleChanged()
    {
      UpdateFormProviderState();
    }

    private void UpdateFormProviderState()
    {
      if (Control == null)
        return; // вызов из конструктора

      if (_InsideSetProviderState)
        return; // реентрантный вызов

      if (ProviderState == EFPControlProviderState.Initialization)
      {
        InternalSetProviderState(EFPControlProviderState.Created);
        if (ProviderState == EFPControlProviderState.Initialization)
          // возникла ошибка, отчет выведен
          return; // 09.07.2021
      }


      switch (ProviderState)
      {
        case EFPControlProviderState.Attached:
          if (!BaseProvider.IsFormVisible)
            InternalSetProviderState(EFPControlProviderState.Detached);
          break;
        case EFPControlProviderState.Created:
        case EFPControlProviderState.Detached:
          if (BaseProvider.IsFormVisible && ControlVisible)
            InternalSetProviderState(EFPControlProviderState.Attached);
          else
            InitLabelVisible(); // Иначе будут висячие метки
          break;
        case EFPControlProviderState.Disposed:
          return;
        default:
          throw new BugException("ProviderState=" + ProviderState.ToString());
      }
    }


    /// <summary>
    /// Отложенный вызов UpdateFormProviderState() инициированный в конструкторе
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void DelayedUpdateFormProviderState(object sender, EventArgs args)
    {
      UpdateFormProviderState();
    }

    #endregion

    #region События переключения провайдера

    #region Свойство ProviderState

    /// <summary>
    /// Текущее состояние провайдера управляющего элемента.
    /// Когда вызываются события переключения на новое состояние (Created, Attached, Detached и Destroyed),
    /// свойство возвращает новое (устанавливаемое) значение.
    /// </summary>
    public EFPControlProviderState ProviderState { get { return _ProviderState; } }
    private EFPControlProviderState _ProviderState;

    /// <summary>
    /// Возвращает true, если в данный момент выполняется установка состояния ProviderState и вызывается один из обработчиков.
    /// При этом свойство ProviderState возврашает новое значение
    /// </summary>
    public bool InsideSetProviderState { get { return _InsideSetProviderState; } }
    private bool _InsideSetProviderState;

    private void InternalSetProviderState(EFPControlProviderState value)
    {
      if (_InsideSetProviderState)
      {
        if (value == _ProviderState)
          return;
        else
          throw new InvalidOperationException("Вложенный вызов InternalSetProviderState(). В данный момент выполняется установка другого состояния: " + _ProviderState.ToString());
      }

      EFPControlProviderState oldState = _ProviderState;

      _InsideSetProviderState = true;
      try
      {
        try
        {
#if DEBUG
          if (TraceProviderState)
            Trace.WriteLine("EFPControl.ProviderState is about to change from " + oldState.ToString() + " to " + value.ToString() + ". Type=" + this.GetType().ToString() + ". DisplayName=" + DisplayName.ToString());
#endif

          CheckIsValidNewProviderState(_ProviderState, value);
          _ProviderState = value;

          switch (value)
          {
            case EFPControlProviderState.Created:
              OnCreated();
              break;
            case EFPControlProviderState.Attached:
              OnAttached();
              break;
            case EFPControlProviderState.Detached:
              OnDetached();
              break;
            case EFPControlProviderState.Disposed:
              OnDisposed();
              break;
          }

#if DEBUG
          if (TraceProviderState)
            Trace.WriteLine("EFPControl.ProviderState successfully changed from " + oldState.ToString() + " to " + value.ToString() + ". Type=" + this.GetType().ToString() + ". DisplayName=" + DisplayName.ToString());
#endif
        }
        catch (Exception e)
        {
          AddExceptionInfo(e);
          e.Data["InternalSetProviderState.NewState"] = value;
          e.Data["InternalSetProviderState.OldState"] = oldState;
          _ProviderState = oldState; // 07.07.2021
#if DEBUG
          if (TraceProviderState)
            Trace.WriteLine("EFPControl.ProviderState changing from " + oldState.ToString() + " to " + value.ToString() + " failed. Type=" + this.GetType().ToString() + ". DisplayName=" + DisplayName.ToString());
#endif

          if (value == EFPControlProviderState.Disposed)
            LogoutTools.LogoutException(e, "Ошибка установки состояния EFPControlProviderState.Disposed");
          else
            throw; // 17.07.2021
        }
      }
      finally
      {
        _InsideSetProviderState = false;
      }
    }

    /// <summary>
    /// Проверка корректности переключения состояния
    /// </summary>
    /// <param name="oldState">Старое состояние</param>
    /// <param name="newState">Новое состояние</param>
    private static void CheckIsValidNewProviderState(EFPControlProviderState oldState, EFPControlProviderState newState)
    {
      bool isValid;
      switch (newState)
      {
        case EFPControlProviderState.Created:
          isValid = (oldState == EFPControlProviderState.Initialization);
          break;
        case EFPControlProviderState.Attached:
          isValid = (oldState == EFPControlProviderState.Created || oldState == EFPControlProviderState.Detached);
          break;
        case EFPControlProviderState.Detached:
          isValid = (oldState == EFPControlProviderState.Attached);
          break;
        case EFPControlProviderState.Disposed:
          isValid = (oldState == EFPControlProviderState.Detached ||
            oldState == EFPControlProviderState.Created || // элемент не был ни разу показан
            oldState == EFPControlProviderState.Initialization); // исключение в конструкторе
          break;
        default:
          throw new ArgumentException("newState");
      }
      if (!isValid)
        throw new InvalidOperationException("Недопустимое переключение ProviderState с " + oldState.ToString() + " на " + newState.ToString());
    }

#if DEBUG
    /// <summary>
    /// Отладочное свойство.
    /// Если установить в true, то в Trace будет выводиться информация о состоянии провайдера
    /// </summary>
    public bool TraceProviderState { get { return _TraceProviderState; } set { _TraceProviderState = value; } }
    private bool _TraceProviderState;
#else
    /// <summary>
    /// Отладочное свойство.
    /// Игнорируется в варианте Release
    /// </summary>
    public bool TraceProviderState { get { return false; } set { } }
#endif

    #endregion

    #region 1. Событие Created

    /// <summary>
    /// Событие вызывается при окончании инициализации элемента.
    /// Это событие вызывается однократно
    /// </summary>
    public event EventHandler Created;

    /// <summary>
    /// Вызывает событие Created. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnCreated()
    {
      if (Created != null)
        Created(this, EventArgs.Empty);

      LoadConfig();

      PrepareCommandItems(); // 18.09.2018. Должно быть после LoadConfig()
      UpdateEnabledState();
    }

    /// <summary>
    /// Свойство возвращает true, если управляющий элемент был инициализирован
    /// Свойство однократно переходит из false в true. Перед этим вызывается событие Created
    /// Свойство Control.Visible может многократно изменяться еще до вывода элемента на экран
    /// </summary>
    public bool HasBeenCreated
    {
      get
      {
        switch (ProviderState)
        {
          case EFPControlProviderState.Initialization: return false;
          case EFPControlProviderState.Created: return !InsideSetProviderState;
          default: return true;
        }
      }
    }

    /// <summary>
    /// Если для элемента свойство HasBeenCreated установлено в true, генерирует исключение InvalidOperationException
    /// </summary>
    public void CheckHasNotBeenCreated()
    {
      if (HasBeenCreated)
        throw new InvalidOperationException("Элемент " + ToString() + " уже был выведен на экран");
    }

    #endregion

    #region 2. Событие Attached

    /// <summary>
    /// Событие вызывается, когда форма выводится на экран или когда элемент присоединяется к видимой форме.
    /// Событие не вызывается, если на момент присоединения к форме, свойство Control.Visible=false. 
    /// Событие будет вызвано позже, когда элемент управления станет видимым. 
    /// Это событие может вызываться многократно, чередуясь с событием Detached.
    /// </summary>
    public event EventHandler Attached;

    /// <summary>
    /// Вызывает событие Attached. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnAttached()
    {
      if (UseIdle) // обязательно после PrepareCommandItems()
        EFPApp.IdleHandlers.Add(this);

      if (_UpdateByTimeHandlers != null)
      {
        _UpdateByTimeHandlers.SetReadOnly();
        for (int i = 0; i < _UpdateByTimeHandlers.Count; i++)
          BaseProvider.FormProvider.UpdateByTimeHandlers.Add(_UpdateByTimeHandlers[i]);
      }

      if (Object.ReferenceEquals(BaseProvider.FormProvider.DelayedSetFocusControlProvider, this))
      {
        BaseProvider.FormProvider.DelayedSetFocusControlProvider = null;
        WinFormsTools.FocusToControl(Control);
      }

      InitToolTipNestedControls();
      InitToolTips();

      if (Attached != null)
        Attached(this, EventArgs.Empty);

      Validate(); // 09.07.2019

      if (ToolBarPanel != null)
        PrepareContextMenu(); // 24.06.2021

      if (Control.ContainsFocus)
        CommandItems.Active = true; // 05.07.2021

      UpdateVisibleState();

      AttachValidators();
    }

    private void AttachValidators()
    {
      if (this.HasValidators)
      {
        this.Validators.SetReadOnly();
        foreach (FreeLibSet.UICore.UIValidator v in this.Validators)
        {
          v.ResultEx.ValueChanged += new EventHandler(this.Validate);
          if (v.PreconditionEx != null)
            v.PreconditionEx.ValueChanged += new EventHandler(this.Validate);
        }
      }
    }

    #endregion

    #region 3. Событие Detached

    /// <summary>
    /// Событие вызывается при закрытии формы с элементом или при отсоединении элемента от формы.
    /// Событие не вызывается, если управляющий элемент становится невидимым (Control.Visible=false),
    /// но продолжает оставаться присоединенным к форме. 
    /// Событие не вызывается, как и событие Attached, если форма была выведена на экран, но свойство Control.Visible все время оставалось равным false.
    /// Это событие может вызываться многократно, чередуясь с событием Attached.
    /// </summary>
    public event EventHandler Detached;

    /// <summary>
    /// Вызывает событие Detached. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnDetached()
    {
      if (UseIdle) // проверка условия предотвратит бесполезный вызов Remove()
        EFPApp.IdleHandlers.Remove(this);

      if (_UpdateByTimeHandlers != null)
      {
        if (BaseProvider.FormProvider != null)
        {
          for (int i = 0; i < _UpdateByTimeHandlers.Count; i++)
            BaseProvider.FormProvider.UpdateByTimeHandlers.Remove(_UpdateByTimeHandlers[i]);
        }
      }


      if (Detached != null)
        Detached(this, EventArgs.Empty);

      SaveConfig();
      UpdateVisibleState();

      DetachValidators();
    }

    private void DetachValidators()
    {
      if (this.HasValidators)
      {
        foreach (FreeLibSet.UICore.UIValidator v in this.Validators)
        {
          v.ResultEx.ValueChanged -= new EventHandler(this.Validate);
          if (v.PreconditionEx != null)
            v.PreconditionEx.ValueChanged -= new EventHandler(this.Validate);
        }
      }
    }

    #endregion

    #region 4. Событие Disposed

    /// <summary>
    /// Событие вызывается в ответ на Control.Disposed.
    /// Событие вызывается однократно
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Вызывает событие Disposed. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnDisposed()
    {
      if (Disposed != null)
        Disposed(this, EventArgs.Empty);
    }

    void Control_Disposed(object sender, EventArgs args)
    {
      try
      {
        if (ProviderState == EFPControlProviderState.Attached)
          InternalSetProviderState(EFPControlProviderState.Detached); // 16.09.2021
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Ошибка аварийной установки состояния Detached при вызове Control.Disposed");
        // Будет и вторая ошибка при установке Disposed
      }
      InternalSetProviderState(EFPControlProviderState.Disposed);
    }

    #endregion

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Видимость управляющего элемента
    /// Возвращает и устанавливает свойство Visible управляющего элемента
    /// Установка значения true может оказаться отложенной, если в момент вызова
    /// один из родительских элементов не является видимым
    /// </summary>
    public bool Visible
    {
      get { return _Visible; }
      set
      {
        _Visible = value;
        if (_VisibleEx != null)
          _VisibleEx.Value = value;
        ControlVisible = value;
      }
    }
    private bool _Visible;

    /// <summary>
    /// Расширенное свойство Visible.
    /// В .Net Framework свойство Control.Visible изменяется, когда изменяется
    /// видимость одного из родительских элементов (например, саиой формы).
    /// При этом, когда свойство переключается в false, событие VisibleChanged
    /// не посылается, поэтому в VisibleEx может оказаться неактуальное значение
    /// </summary>
    public DepValue<Boolean> VisibleEx
    {
      get
      {
        InitVisibleEx();
        return _VisibleEx;
      }
      set
      {
        InitVisibleEx();
        _VisibleEx.Source = value;
      }
    }

    private void InitVisibleEx()
    {
      if (_VisibleEx == null)
      {
        _VisibleEx = new DepInput<Boolean>(Visible, VisibleEx_ValueChanged);
        _VisibleEx.OwnerInfo = new DepOwnerInfo(this, "VisibleEx");
      }
    }

    void VisibleEx_ValueChanged(object sender, EventArgs args)
    {
      Visible = _VisibleEx.Value;
    }

    private DepInput<Boolean> _VisibleEx;

    /// <summary>
    /// Чтение и запись свойства Control.Visible
    /// </summary>
    protected abstract bool ControlVisible { get; set; }

    /// <summary>
    /// Сюда должен быть присоединен обработчик события Control.VisibleChanged
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void ControlVisibleChanged(object sender, EventArgs args)
    {
      try
      {
        EFPFormProvider FormProvider = BaseProvider.FormProvider;
        if (FormProvider == null)
          return; // 09.06.2021. До присоединения к форме - игнорируем

        if (ControlVisible && FormProvider.Form.Visible /* 09.06.2021 */)
        {
          FormProvider.FirstReadConfig(); // 04.10.2018. Выполняем предзагрузку секций конфигурации.
          // 15.10.2018  Но делаем это не из метода OnShown(), так как
          //             чтение конфигурации может сделать элемент невидимым и получится,
          //             что Control.Visible=false в процессе OnShown(), что может привести к сбою
          // 18.10.2018  И только если элемент переходит в видимое состояние, а не скрывается


          if (Control.IsHandleCreated && (!Control.IsDisposed))
            _Visible = ControlVisible; // 15.10.2018. Здесь ControlVisible могло поменяться

          if (_Visible)
          {
            UpdateFormProviderState();
          }
          if (_VisibleEx != null)
            _VisibleEx.Value = Visible;
        }

        UpdateVisibleState();
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка обработчика Control.VisibleChanged");
      }
    }

    private void InitLabelVisible()
    {
      if (Label == null)
        return;
      if (Label is Label)
        Label.Visible = Visible;
    }

    private bool NeedsResetControlColorsInMono()
    {
      // 06.04.2018
      // В Mono, почему-то поля ввода по умолчанию имеют не такой цвет как в Windows
      //if (!EnvironmentTools.IsMono)
      //  return false;

      if ((Control is ComboBox) || (Control is DateTimePicker) || (Control is ListBox) || (Control is UpDownBase))
        return true;
      if (Control is TextBoxBase)
        //return !((TextBoxBase)Control).ReadOnly;
        return true;
      if (Control is UserTextComboBoxBase)
        //return !((UserTextComboBoxBase)Control).ReadOnly;
        return true;
      // Так не работает
      //if (Control is YearMonthBox)
      //  return true;
      //if (Control is YearMonthRangeBox)
      //  return true;

      return false;
    }


    #endregion

    #region Свойство VisibleState

    /// <summary>
    /// Свойство возвращает true, когда ProviderState=Attached и Visible=true.
    /// В отличие от основного свойства Visible, это свойство имеет одинаковый "жизненный цикл",
    /// независимо от способа показа формы и прикрепления к ней управляющего элемента.
    /// Сначала свойство имеет значение false. Затем, при показе формы, оно приобретает значение true,
    /// если элемент является видимым. Если свойство Visiblу меняется в процессе показа формы, то VisibleState
    /// также меняется. Затем, при скрытии элемента или его отключении, свойство переходит в false.
    /// </summary>
    public bool VisibleState { get { return ProviderState == EFPControlProviderState.Attached && Visible; } }

    /// <summary>
    /// Этот метод вызывается при изменении значения свойства VisibleState
    /// </summary>
    protected virtual void OnVisibleStateChanged()
    {
      InitControlColors();
      InitLabelVisible();
      Validate();
    }

    /// <summary>
    /// Вызывает метод OnVisibleStateChanged(), если значение свойства VisibleChanged изменилось
    /// </summary>
    private void UpdateVisibleState()
    {
      if (VisibleState == _PrevVisibleState)
        return;
      _PrevVisibleState = VisibleState;
      OnVisibleStateChanged();
      if (_EditableEx != null)
        _EditableEx.OwnerSetValue(Editable);
    }

    private bool _PrevVisibleState;

    #endregion

    #region Свойство Enabled

    /// <summary>
    /// Доступность управляющего элемента
    /// Возвращает и устанавливает свойство Enabled управляющего элемента
    /// Установка значения true может оказаться отложенной, если в момент вызова
    /// один из родительских элементов не является доступным
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        _Enabled = value;
        if (_EnabledEx != null)
          _EnabledEx.Value = value;
        ControlEnabled = value;
        UpdateEnabledState(); // ? надо ли
      }
    }
    private bool _Enabled;

    /// <summary>
    /// Расширенное свойство Enabled.
    /// В .Net Framework свойство Control.Enabled изменяется, когда изменяется
    /// доступность одного из родительских элементов (например, GroupBox).
    /// В отличие от Control.VisibleChanged, событие Control.EnabledChanged посылается
    /// при любом перключении, поэтому свойство EnabledEx содержит актуальное значение
    /// </summary>
    public DepValue<Boolean> EnabledEx
    {
      get
      {
        InitEnabledEx();
        return _EnabledEx;
      }
      set
      {
        InitEnabledEx();
        _EnabledMain.Source = value;
      }
    }

    private void InitEnabledEx()
    {
      if (_EnabledEx == null)
      {
        _EnabledEx = new DepInput<Boolean>(Enabled, EnabledEx_ValueChanged);
        _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");

        _EnabledMain = new DepInput<bool>(Enabled, null);
        _EnabledMain.OwnerInfo = new DepOwnerInfo(this, "EnabledMain");

        _EnabledSync = new DepInput<bool>(true, null);
        _EnabledSync.OwnerInfo = new DepOwnerInfo(this, "EnabledSync");

        DepAnd EnabledAnd = new DepAnd(_EnabledMain, _EnabledSync);
        _EnabledEx.Source = EnabledAnd;
      }
    }

    private void EnabledEx_ValueChanged(object sender, EventArgs args)
    {
      Enabled = _EnabledEx.Value;
    }

    /// <summary>
    /// Выходная часть свойства EnabledEx
    /// </summary>
    private DepInput<Boolean> _EnabledEx;
    /// <summary>
    /// Основной вход для EnabledEx
    /// </summary>
    private DepInput<Boolean> _EnabledMain;
    /// <summary>
    /// Дополнительный вход для EnabledEx для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _EnabledSync;

    /// <summary>
    /// Свойство Control.Enabled
    /// </summary>
    protected abstract bool ControlEnabled { get; set; }

    /// <summary>
    /// Сюда должен быть присоединен обработчик события Control.EnabledChanged
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void ControlEnabledChanged(object sender, EventArgs args)
    {
      try
      {
        UpdateEnabledState();
        if (_EnabledEx != null)
          _EnabledEx.Value = Enabled;
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка обработчика Control.EnabledChanged");
      }
    }

    #endregion

    #region Свойство EnabledState

    /// <summary>
    /// "Доступное" состояние.
    /// Для большинства управляющих элементов свойство дублирует Enabled.
    /// Для EFPTextBox и некоторых других возвращает комбинацию Enabled and not ReadOnly.
    /// </summary>
    public virtual bool EnabledState { get { return Enabled; } }

    /// <summary>
    /// Этот метод вызывается при изменении значении свойства EnabledState.
    /// Переопределенный метод должен обязательно вызывать метод базового класса.
    /// </summary>
    protected virtual void OnEnabledStateChanged()
    {
      Validate();
    }

    /// <summary>
    /// Проверяет, не изменилось ли значение свойства EnabledState, и вызывает метод OnEnabledStateChanged() при необходимости.
    /// Этот метод должен вызываться из установщика свойства ReadOnly, если свойство есть в производном классе
    /// </summary>
    protected void UpdateEnabledState()
    {
      if (EnabledState == _PrevEnabledState)
        return;
      _PrevEnabledState = EnabledState;
      OnEnabledStateChanged();

      if (_EditableEx != null)
        _EditableEx.OwnerSetValue(Editable);
    }

    private bool _PrevEnabledState;

    #endregion

    #region Свойство Editable

    /// <summary>
    /// Свойство возвращает true, если свойства Visible и EnabledState установлены в true.
    /// </summary>
    public bool Editable { get { return Visible && EnabledState; } }

    /// <summary>
    /// Управляемое свойство, возвращающее установку свойств Visible и Enabled
    /// (и сброс ReadOnly, если оно есть)
    /// Свойство может быть использовано для упрощения организации блокировок в
    /// качестве входа для свойства Enabled другого элемента
    /// </summary>
    public DepValue<bool> EditableEx
    {
      get
      {
        if (_EditableEx == null)
        {
          _EditableEx = new DepOutput<bool>(Editable);
          _EditableEx.OwnerInfo = new DepOwnerInfo(this, "EditableEx");
        }
        return _EditableEx;
      }
    }
    private DepOutput<bool> _EditableEx;

    #endregion

    #region Метка для управляющего элемента

    /// <summary>
    /// Метка для управляющего элемента.
    /// По умолчанию свойство устанавливается на предыдущий управляющий элемент (вызовом Control.GetNextControl() для родительского элемента),
    /// если он является меткой или GroupBox.
    /// Свойство не имеет значения, если в конструкторе задана аргумент LabelNeeded=false (зависит от типа управляющего элемента).
    /// Свойству может быть присвоено значение null, Label, GroupBox или CheckBox. Использование других
    /// типов управляющих элементов не допускается
    /// </summary>
    public virtual Control Label
    {
      get { return _Label; }
      set
      {
        if (value == _Label)
          return;

        if (value != null)
        {
          if (!IsValidLabelControl(value))
            throw new ArgumentException("Свойство Label может быть меткой, GroupBox, CheckBox или RadioButton");

          // 03.07.2017
          // Запоминаем цвет метки
          _OrgLabelForeColor = value.ForeColor;

          // 02.03.2013
          // Изменение цвета текста GroupBox'а не должно менять цвет элементов, в нем
          // расположенных
          // При необходимости, например, для TextEdit, цвет текста в управляющем
          // элементе устанавливается в явном виде
          if (value is GroupBox)
          {
            //for (int i = 0; i < value.Controls.Count; i++)
            //  value.Controls[i].ForeColor = value.Controls[i].ForeColor; // отцепили цвет
            // 27.06.2017
            // Делаем это рекурсивно
            UnambientForeColor(value);
          }
        }

        _Label = value;
        InitLabelVisible();
        InitLabelColors();
      }
    }
    private Control _Label;

    /// <summary>
    /// 03.07.2017. Запоминаем исходный цвет метки, который был на момент установки свойства Label
    /// </summary>
    private Color _OrgLabelForeColor;

    private bool IsValidLabelControl(Control value)
    {
      return value is Label || value is GroupBox || value is CheckBox || value is RadioButton;
    }

    private void FindLabel()
    {
      if (Control.Parent == null)
        return;

      Control PrevControl = Control.Parent.GetNextControl(Control, false);

      if (PrevControl == null)
      {
        if (Control.Parent is GroupBox)
          Label = Control.Parent;
        return;
      }
      if (PrevControl is Label)
      {
        if (((Label)PrevControl).UseMnemonic) // 22.03.2016
          Label = PrevControl;
        return;
      }

      // 02.03.2013
      if ((PrevControl is Panel) && (Control.Parent is GroupBox) && (Control.Dock == DockStyle.Fill))
        Label = Control.Parent;
    }

    private static void UnambientForeColor(Control parentControl)
    {
      if (parentControl.HasChildren)
      {
        for (int i = 0; i < parentControl.Controls.Count; i++)
        {
          Control ctrl = parentControl.Controls[i];
          ctrl.ForeColor = ctrl.ForeColor; // отцепили цвет
          UnambientForeColor(parentControl.Controls[i]); // рекурсивный вызов
        }
      }
    }

    #endregion

    #region Название элемента

    /// <summary>
    /// Название элемента управления, которое используется при выдачи сообщений 
    /// об ошибках
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
        {
          if (Label != null)
            return MyDelSC(Label.Text);
          if (LabelNeeded)
            return DefaultDisplayName;
          if (String.IsNullOrEmpty(Control.Text))
            //return Control.GetType().ToString();
            return DefaultDisplayName; // 15.08.2020
          return MyDelSC(Control.Text);
        }
        return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }

    private string _DisplayName;

    /// <summary>
    /// Название элемента управления по умолчанию.
    /// Используется в качестве возвращаемого значения свойства DisplayName, если свойство не установлено явно и у элемента нет метки.
    /// Непереопределенное свойство возвращает "Без названия".
    /// Для некоторых специализированных элементов может быть переопределено.
    /// </summary>
    protected virtual string DefaultDisplayName { get { return "Без названия"; } }

    /// <summary>
    /// Удаление амперсанда
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Строка без амперсанда</returns>
    private string MyDelSC(string s)
    {
      int p = s.IndexOf('&');
      if (p >= 0)
        s = s.Remove(p, 1);

      s = s.Replace(Environment.NewLine, " ");
      return s;
    }

    /// <summary>
    /// Возвращает DisplayName плюс вспомогательную информацию (Disposes, Hidden, Disabled)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DisplayName);
      sb.Append(", ProviderState=");
      sb.Append(ProviderState.ToString());
      if (ProviderState == EFPControlProviderState.Attached)
      {
        if (!Control.Visible)
          sb.Append(" (Hidden)");
        if (!Control.Enabled)
          sb.Append(" (Disabled)");
      }
      return sb.ToString();
    }

    #endregion

    #region Всплывающая подсказка

    /// <summary>
    /// Подсказка для управляющего элемента - описание назначения управляющего 
    /// элемента (не включая сообщение об ошибке и подсказку по текущему значению)
    /// </summary>
    public virtual string ToolTipText
    {
      get { return _ToolTipText; }
      set
      {
        if (value == _ToolTipText)
          return;
        _ToolTipText = value;
        InitToolTips();
      }
    }
    private string _ToolTipText;

    /// <summary>
    /// Подсказка по текущему значению
    /// Если используется, то свойство должно устанавливаться при изменении 
    /// пользователем введенного значения
    /// </summary>
    public string ValueToolTipText
    {
      get { return _ValueToolTipText; }
      set
      {
        if (value == _ValueToolTipText)
          return;
        _ValueToolTipText = value;
        if (!_InsideValidate) // 15.02.2021. Иначе метод DoValidate() не добавит текст сообщения об ошибки или предупреждении
          InitToolTips();
      }
    }
    private string _ValueToolTipText;



    /// <summary>
    /// Добавление дочерних управляющих элементов для дублирования всплывающих подсказок
    /// </summary>
    protected virtual void InitToolTipNestedControls()
    {
      EFPControlBase[] nested = GetNestedControlProviders();
      DoInitToolTipNestedControls(this.Control, false, nested);
    }

    private void DoInitToolTipNestedControls(Control testedControl, bool includeThisControl, EFPControlBase[] nested)
    {
      if (testedControl is ControlRightButton)
        return; // иначе будут перекрываться собственные подсказки

      // Массив nested, как правило, очень короткий, обычно нулевой длины.
      // Нет смысла оптимизировать поиск в нем, создавая словарь элементов.
      for (int i = 0; i < nested.Length; i++)
      {
        Control[] nestedControls = nested[i].GetControls();
        if (Array.IndexOf<Control>(nestedControls, testedControl) >= 0)
          return;
        if (nested[i].Label == testedControl)
          return;
      }


      if (includeThisControl)
        BaseProvider.FormProvider.SetNestedToolTipControl(testedControl, this.Control);

      if (testedControl.HasChildren)
      {
        for (int i = 0; i < testedControl.Controls.Count; i++)
          DoInitToolTipNestedControls(testedControl.Controls[i], true, nested); // рекурсивный вызов
      }
    }

    /// <summary>
    /// Инициализация всплывающих подсказок.
    /// Непереопределенный метод вызывает InitToolTips для Control и его рекурсивно дочерних управляющих элементов,
    /// если для них нет собственных объектов EFPControlBase
    /// </summary>
    protected virtual void InitToolTips()
    {
      InitToolTips(this.Control);
    }

    /// <summary>
    /// Инициализация всплывающей подсказки для заданного элемента.
    /// Вызывается из виртуального метода InitToolTips()
    /// </summary>
    /// <param name="control">Элемент</param>
    protected void InitToolTips(Control control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      BaseProvider.SetToolTip(control, DisplayName, ToolTipText, ValueToolTipText, ValidateState, ValidateErrorMessage);
    }

    #endregion

    #region Свойство CommandItems

    // ?? не знаю, как сделать Shared-списки и при этом иметь возможность добавлять
    // обработчики события EFPApp.AddCommandItems (не релизовано) для централизованного
    // добавления команд к определенным типам управляющих элементов

    /// <summary>
    /// Доступ к свойству создает не-Shared список команд, к которому можно добавить команды
    /// Свойство Control не устанавливается
    /// Допускается установка списка команд "снаружи". Она должна выполняться
    /// до любого обращения к свойству
    /// </summary>
    public EFPControlCommandItems CommandItems
    {
      get
      {
        if (_CommandItems == null)
        {
          if (!_InsideInitCommandItems)
          {
            _InsideInitCommandItems = true;
            try
            {
              EFPApp.OnControlCommandItemsNeeded(this);
            }
            finally
            {
              _InsideInitCommandItems = false;
            }
          }

          if (_CommandItems == null)
            _CommandItems = GetCommandItems();
        }
        return _CommandItems;
      }
      set
      {
        _CommandItems = value;
      }
    }
    private EFPControlCommandItems _CommandItems;

    /// <summary>
    /// Возвращает CommandItems, только если команды уже были инициализированы
    /// </summary>
    protected EFPControlCommandItems CommandItemsIfAssigned
    {
      get { return _CommandItems; }
    }

    private bool _InsideInitCommandItems;

    /// <summary>
    /// Метод должен создать объект EFPControlCommandItems.
    /// Если производный класс использует собственный список команд, он должен 
    /// переопределить этот метод.
    /// При этом вызывать иетод EFPControlBase.GetCommandItems() не нужно
    /// </summary>
    /// <returns></returns>
    protected virtual EFPControlCommandItems GetCommandItems()
    {
      return new EFPControlCommandItems();
    }

    /// <summary>
    /// Возвращает true, если было обращение к свойству CommandItems и ему присвоено значение
    /// Используется в командах обновления, когда нежелательно "случайно" создать
    /// CommandItems.
    /// Обычно удобнее использовать свойство CommandItemsIfAssigned
    /// </summary>
    protected bool CommandItemsAssigned
    {
      get { return _CommandItems != null; }
    }

    #endregion

    #region Активация правой кнопкой мыши и локальное меню

    private void Control_MouseDown(object sender, MouseEventArgs args)
    {
      try
      {
        // Установка фокуса
        InternalSetFocus();
        // Инициализация локального меню
        PrepareContextMenu();
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка обработчика Control.MouseDown");
      }
    }

    /// <summary>
    /// Установка фокуса ввода на элемент, если его еще там нет
    /// </summary>
    private void InternalSetFocus()
    {
      if (Control.ContainsFocus)
        return;
      if (!Control.CanFocus)
        return;
      Control.Focus();
    }

    private void Control_Enter(object sender, EventArgs args)
    {
      try
      {
        // 21.08.2015
        // Событие Enter может возникнуть до установки Visible=true
        UpdateFormProviderState();

        PrepareContextMenu();
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка обработчика Control.Enter");
      }

      if (ValidateWhenFocusChanged)
        _IdleValidationRequired = true;
    }

    private void Control_Leave(object sender, EventArgs args)
    {
      if (ValidateWhenFocusChanged)
        _IdleValidationRequired = true;
    }

    private bool _ContextMenuWasInit;

    /// <summary>
    /// Обеспечивает готовность локального меню.
    /// После вызова метода устанавливается свойство Control.ContextMenuStrip, если оно не было установлено ранее.
    /// Перед присоединеием вызывается PrepareCommandItems().
    /// Повторные вызовы метода игнорируются.
    /// </summary>
    protected void PrepareContextMenu()
    {
      if (_ContextMenuWasInit)
        return;

      if (Control.ContextMenuStrip != null) // создано другим способом
      {
        _ContextMenuWasInit = true;
        return;
      }

      try
      {
        if (PrepareCommandItems())
        {
          _ContextMenuWasInit = true;

          // Присоединяем команды
          bool ToolBarControllable = false;
          if (ToolBar != null)
            ToolBarControllable = ToolBar.PanelCommandItems != null;

          List<EFPCommandItems> List = new List<EFPCommandItems>();
          if (CommandItems.Count > 0)
            List.Add(CommandItems);
          if (BaseCommandItemsNeeded) // 28.09.2018
            BaseProvider.InitCommandItemList(List);

          if (List.Count > 0 || ToolBarControllable)
          {
            EFPContextMenu ccm = new EFPContextMenu();
            for (int i = 0; i < List.Count; i++)
              ccm.Add(List[i]);

            //if (ToolBarControllable)
            //{
            //  ccm.Menu.Items.Add(
            //  EFPDropDownMenu ddm = new EFPDropDownMenu();
            //  ddm.Add(ToolBar.PanelCommandItems);
            //  //ddm.a
            //}

            ccm.DefaultCommandItem = CommandItems.DefaultCommandItem;

            ccm.Attach(Control);

            if (Control is UserControl)
            {
              foreach (Control ChildControl in Control.Controls)
                ChildControl.ContextMenuStrip = Control.ContextMenuStrip;
            }

            //CommandItems.Active = true; // ?? Control.ContainsFocus;
          }
        }
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка инициализации локального меню для " + ToString());
      }
    }

    /// <summary>
    /// Возвращает true (по умолчанию), если в локальное меню нужно добавить команды из 
    /// родительского элемента.
    /// </summary>
    protected virtual bool BaseCommandItemsNeeded { get { return true; } }

    /// <summary>
    /// Показ локального меню управляющего элемента (как при нажатии правой кнопки мыши).
    /// Переопределяется EFPButtonWithMenu, чтобы в меню были только явно заданные команды
    /// </summary>
    public void ShowLocalMenu()
    {
      PrepareContextMenu();

      if (Control.ContextMenuStrip != null)
        Control.ContextMenuStrip.Show(Control, Control.Width, Control.Height);
      else if (Control.ContextMenu != null)
        Control.ContextMenu.Show(Control, new Point(Control.Width, Control.Height));
    }

    #endregion

    #region Локальная панель инструментов

    /// <summary>
    /// Панель инструментов для локального меню.
    /// Если управляющий элемент создан без использования ControlWithToolBar, но ему требуется
    /// панель инструментов, то следует установить это свойство.
    /// </summary>
    public Panel ToolBarPanel
    {
      get
      {
        if (_ToolBar == null)
          return null;
        else
          return (Panel)(_ToolBar.Bar.Parent);
      }
      set
      {
        if (value == this.ToolBarPanel)
          return;
        if (_ToolBar != null)
          throw new InvalidOperationException("Повторная установка свойства EFPControl.ToolBarPanel не допускается");

        if (!EFPApp.ShowControlToolBars)
        {
          value.Visible = false;
          return;
        }

        _ToolBar = new EFPPanelToolBar();
        _ToolBar.Attach(value);

        if (CommandItems.Control != null)
        {
          ToolBar.Add(CommandItems);
          ToolBar.ToolBarVisible = ToolBar.Count > 0; // 10.09.2012
        }
      }
    }

    /// <summary>
    /// Провайдер панели инструментов.
    /// Создается при установке свойства ToolBarPanel.
    /// </summary>
    public EFPPanelToolBar ToolBar { get { return _ToolBar; } }
    private EFPPanelToolBar _ToolBar;

    /// <summary>
    /// Если управляющий элемент присоединен к форме и форма выведена на экран,
    /// то присваивает значение свойству CommandItems.Control.
    /// Инициализирует локальную панель кнопок, если она есть
    /// Инициализирует панели статусной строки
    /// Возвращает true, если значение CommandItem.Control установлено сейчас или
    /// было установлено раньше.
    /// Для инициализации локального меню используйте PrepareContextMenu().
    /// Повторные вызовы метода игнорируются.
    /// </summary>
    public bool PrepareCommandItems()
    {
      if (CommandItems.Control != null)
        return true;

      if (Control.IsDisposed)
        return false; // 23.10.2017

      if (!WinFormsTools.AreControlAndFormVisible(Control))
        return false;

      OnBeforePrepareCommandItems(); // 28.09.2018

      CommandItems.CallBeforeControlAssigned();

      // Статусная строка должна быть собрана заранее, до установки Control
      bool SBFlag = false;
      foreach (EFPCommandItem Item in CommandItems)
      {
        if (Item.StatusBarUsage)
        {
          SBFlag = true;
          break;
        }
      }

      if (SBFlag)
      {
        EFPStatusBarPanels sbPanels = new EFPStatusBarPanels(this, Control);
        sbPanels.Add(CommandItems);
        CommandItems.StatusBarPanels = sbPanels;
      }

      CommandItems.SetControl(Control);
      CommandItems.CallAfterControlAssigned();

      if (ToolBar != null)
      {
        ToolBar.Add(CommandItems);
        ToolBar.ToolBarVisible = ToolBar.Count > 0; // 10.09.2012
      }

      if (Control.ContainsFocus)
        CommandItems.Active = true;

      return true;
    }

    /// <summary>
    /// Событие вызывается из PrepareCommandItems(), перед тем, как он выполнит инициализацию локального меню,
    /// панелей инструментов и статусной строки.
    /// Пользовательский обработчик может, например, выполнить окончательную настройку команд.
    /// </summary>
    public event EventHandler BeforePrepareCommandItems;

    /// <summary>
    /// Вызывает событие BeforePrepareCommandItems, если обработчик установлен
    /// </summary>
    protected virtual void OnBeforePrepareCommandItems()
    {
      if (BeforePrepareCommandItems != null)
        BeforePrepareCommandItems(this, EventArgs.Empty);
    }

    EFPStatusBarHandler IEFPStatusBarControl.StatusBarHandler
    {
      get
      {
        if (BaseProvider.FormProvider == null)
          return null; // еще не присоединен
        else
          return BaseProvider.FormProvider.StatusBarHandler;
      }
    }

    #endregion

    #region Validate()

    /// <summary>
    /// Выполнить проверку значений.
    /// Проверка не выполняется, пока форма не выведена на экран. При выводе формы
    /// проверка выполняется для всех управляющих элементов
    /// </summary>
    public void Validate()
    {
      if (_InsideValidate)
        return;
      _InsideValidate = true;
      try
      {
        DoValidate();
      }
      finally
      {
        _InsideValidate = false;
      }
    }

    private bool _InsideValidate;

    private void DoValidate()
    {
      Form frm = Control.FindForm();
      if (frm == null)
        return;
      if (!frm.Visible)
        return;

      UIValidateState PrevState = _ValidateState;
      string PrevMessage = _ValidateErrorMessage;
      string PrevValueToolTipText = _ValueToolTipText;

      _ValidateState = UIValidateState.Ok;
      _ValidateErrorMessage = null;

      if (Editable)
      {
        try
        {
          OnValidate();

          if (_Validators != null)
            _Validators.Validate(this);

          if ((_ValidateState != UIValidateState.Error) && (Validating != null))
          {
            if (_ValidatingArgs == null)
              _ValidatingArgs = new UIValidatingEventArgs(this);
            Validating(this, _ValidatingArgs);
          }
        }
        catch (Exception e) // 25.06.2015
        {
          SetError("Ошибка при проверке. " + e.Message);
        }
      } // Editable

      if (_ValidateState != PrevState ||
        _ValidateErrorMessage != PrevMessage ||
        _ValueToolTipText != PrevValueToolTipText ) 
      {
        // Состояние ошибки изменилось
        if (BaseProvider != null)
        {
          BaseProvider.ItemStateChanged();
          InitToolTips();
        }

        InitControlColors();
        InitLabelColors();
      }
    }

    /// <summary>
    /// Список валидаторов элемента, основанных на управляемых значениях
    /// </summary>
    public UIValidatorList Validators
    {
      get
      {
        if (_Validators == null)
        {
          if (ProviderState == EFPControlProviderState.Initialization || ProviderState == EFPControlProviderState.Created)
            _Validators = new UIValidatorList();
          else
            _Validators = UIValidatorList.Empty;
        }
        return _Validators;
      }
    }
    private UIValidatorList _Validators;

    /// <summary>
    /// Возвращает true, если список содержит валидаторы
    /// </summary>
    public bool HasValidators
    {
      get
      {
        if (_Validators == null)
          return false;
        else
          return _Validators.Count > 0;
      }
    }

    /// <summary>
    /// Специальная версия выполнения проверки для управляющего элемента, которую
    /// можно использовать в обработчике изменения значения в другом элементе в виде
    /// efp1.Value.ValueChanged+=new EventHandler(efp2.Validate);
    /// Аргументы игнорируются, вызывается обычная версия Validate() без аргументов
    /// </summary>
    /// <param name="sender">Источник события - игнорируется</param>
    /// <param name="args">Аргументы события - игнорируются</param>
    public void Validate(object sender, EventArgs args)
    {
      Validate();
    }

    internal int ErrorCount
    {
      get { return (ValidateState == UIValidateState.Error) ? 1 : 0; }
    }

    internal int WarningCount
    {
      get { return (ValidateState == UIValidateState.Warning) ? 1 : 0; }
    }

    internal void GetErrorMessages(List<EFPErrorInfo> errorList)
    {
      if (ValidateState == UIValidateState.Ok)
        return;
      EFPErrorInfo Info = new EFPErrorInfo(ValidateErrorMessage, ValidateState == UIValidateState.Error, Control);
      errorList.Add(Info);
    }

    /// <summary>
    /// Если свойство установлено в true, то метод Validate() будет дополнительно вызываться после получения и
    /// потери элементом фокуса ввода.
    /// Вызов метода Validate() выполняется не сразу в обработчике событий Control.Enter и Control.Leave,
    /// а с небольшой задержкой, в обработчике Idle. Соответственно, свойство UseIdle принимает значение true.
    /// По умолчанию свойство имеет значение false и проверка не выполняется.
    /// Установка свойства возможна только до вывода элемента на экран.
    /// </summary>
    public bool ValidateWhenFocusChanged
    {
      get { return _ValidateWhenFocusChanged; }
      set
      {
        CheckHasNotBeenCreated();

        _ValidateWhenFocusChanged = value;
        if (value)
        {
          UseIdle = true;
          if (!_ControlLeaveHandlerAssigned)
          {
            _ControlLeaveHandlerAssigned = true;
            _Control.Leave += Control_Leave;
          }
        }
      }
    }
    private bool _ValidateWhenFocusChanged;

    private bool _IdleValidationRequired;

    private bool _ControlLeaveHandlerAssigned;

    #endregion

    #region IEFPValidator Members

    /// <summary>
    /// Этот метод может вызываться из OnValidate для указания наличия ошибки
    /// </summary>
    /// <param name="message"></param>
    void IUIValidableObject.SetError(string message)
    {
      SetError(message);
    }

    /// <summary>
    /// Этот метод может вызываться из OnValidate для указания наличия прежупреждения
    /// </summary>
    /// <param name="message"></param>
    void IUIValidableObject.SetWarning(string message)
    {
      SetWarning(message);
    }

    /// <summary>
    /// Этот метод может вызываться из OnValidate() для указания наличия ошибки
    /// </summary>
    /// <param name="message"></param>
    protected void SetError(string message)
    {
#if DEBUG
      if (!_InsideValidate)
        throw new InvalidOperationException("Метод может вызываться только из OnValidate() или обработчика события Validating");
#endif

      if (_ValidateState == UIValidateState.Error)
        return;
      _ValidateState = UIValidateState.Error;
      _ValidateErrorMessage = message;
    }

    /// <summary>
    /// Этот метод может вызываться из OnValidate() для указания наличия прежупреждения
    /// </summary>
    /// <param name="message"></param>
    protected void SetWarning(string message)
    {
#if DEBUG
      if (!_InsideValidate)
        throw new InvalidOperationException("Метод может вызываться только из OnValidate() или обработчика события Validating");
#endif

      if (_ValidateState != UIValidateState.Ok)
        return;
      _ValidateState = UIValidateState.Warning;
      _ValidateErrorMessage = message;
    }

    /// <summary>
    /// Текущее состояние проверки ошибок
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    #endregion

    #region Прочие свойства, методы и события проверки ошибок

    private Color _OrgControlForeColor;

    /// <summary>
    /// Установка цветов управляющего элемента после изменения состояния ошибки
    /// </summary>
    protected virtual void InitControlColors()
    {
      // 06.04.2018
      // Mono, в отличие от NetFramework, для TextBox использует ForeColor и BackColor в режиме ReadOnly
      if (EnvironmentTools.IsMono && NeedsResetControlColorsInMono())
      {
        //MessageBox.Show("Editable=" + Editable.ToString(), Control.GetType().ToString());
        if (Editable)
        {
          Control.BackColor = SystemColors.Window;
          // Метод Validate() установит правильный цвет ForeColor
        }
        else
        {
          Control.BackColor = SystemColors.Control;
          Control.ForeColor = SystemColors.ControlText;
        }
      }

      switch (ValidateState)
      {
        case UIValidateState.Ok:
          if (EFPApp.IsMono)
            Control.ForeColor = _OrgControlForeColor; // 01.10.2013
          else
            Control.ResetForeColor();
          break;
        case UIValidateState.Error:
          Control.ForeColor = EFPApp.Colors.LabelErrorForeColor;
          break;
        case UIValidateState.Warning:
          Control.ForeColor = EFPApp.Colors.LabelWarningForeColor;
          break;
      }
    }

    /// <summary>
    /// Установка цветов для метки (если она есть)
    /// </summary>
    protected void InitLabelColors()
    {
      if (Label == null)
        return;

      SetLabelForeColor(Label, ValidateState, _OrgLabelForeColor);
    }

    /// <summary>
    /// Установить цвет текста метки в соответствии с состоянием проверки
    /// Используется свойство Control.ForeColor.
    /// Эта версия задает цвет SystemColors.ControlText для состояния без ошибок
    /// </summary>
    /// <param name="label">Управляющий элемент</param>
    /// <param name="state">Цвет текста</param>
    public static void SetLabelForeColor(Control label, UIValidateState state)
    {
      SetLabelForeColor(label, state, SystemColors.ControlText);
    }

    /// <summary>
    /// Установить цвет текста метки в соответствии с состоянием проверки
    /// Используется свойство Control.ForeColor
    /// </summary>
    /// <param name="label">Управляющий элемент</param>
    /// <param name="state">Цвет текста</param>
    /// <param name="defaultForeColor">Основной цвет метки. Обычно равно SystemColors.ControlText.
    /// Если задано Color.Transparent, вызывается метод Control.ResetForeColor()</param>
    public static void SetLabelForeColor(Control label, UIValidateState state, Color defaultForeColor)
    {
#if DEBUG
      if (label == null)
        throw new ArgumentNullException("label");
#endif

      switch (state)
      {
        case UIValidateState.Ok:
          //if (EFPApp.IsMono)
          //  Label.ForeColor = SystemColors.ControlText; // 01.10.2013
          //else
          //  Label.ResetForeColor();

          // 03.07.2017
          // Всегда устанавливаем цвет явно
          if (defaultForeColor == Color.Transparent)
            label.ResetForeColor();
          else
            label.ForeColor = defaultForeColor;
          break;
        case UIValidateState.Error:
          label.ForeColor = EFPApp.Colors.LabelErrorForeColor;
          break;
        case UIValidateState.Warning:
          label.ForeColor = EFPApp.Colors.LabelWarningForeColor;
          break;
      }
    }
    /// <summary>
    /// Используем один объект аргументов, чтобы не создавать каждый раз
    /// </summary>
    private UIValidatingEventArgs _ValidatingArgs;

    /// <summary>
    /// Текущее сообщение об ошибке или предупреждении
    /// </summary>
    public string ValidateErrorMessage { get { return _ValidateErrorMessage; } }
    private string _ValidateErrorMessage;

    /// <summary>
    /// Частные проверки для конкретных управляющих элементов.
    /// Этот метод сам не вызывает обработчик события Validating. Событие всегда вызывается после вызова OnValidate()
    /// </summary>
    protected virtual void OnValidate()
    {
      // Могут переопределяться для некоторых управляющих элементов
    }

    /// <summary>
    /// Пользовательский обработчик для проверки ошибок.
    /// Обработчик всегда вызывается после защищенного метода OnValidate().
    /// Обработчик не может "понизить" состояние проверки, например, убрать предупреждение.
    /// Событие не вызывается, если OnValidate() установил состояние ошибки.
    /// </summary>
    public event UIValidatingEventHandler Validating;

    #endregion

    #region Активация управляющего элемента

    /// <summary>
    /// Передать фокус ввода управляющему элементу.
    /// Если управляющий элемент находится на вкладке TabPage, которая не активна,
    /// то она активируется
    /// </summary>
    public void SetFocus()
    {
      SetFocus(null);
    }

    /// <summary>
    /// Передать фокус ввода управляющему элементу и выдать сообщение об ошибке
    /// Если управляющий элемент находится на вкладке TabPage, которая не активна,
    /// то она активируется
    /// </summary>
    /// <param name="errorMessage">Строка сообщения</param>
    public void SetFocus(string errorMessage)
    {
      if (HasBeenCreated)
        WinFormsTools.FocusToControl(Control);
      else if (BaseProvider.FormProvider != null)
        BaseProvider.FormProvider.DelayedSetFocusControlProvider = this;
      if (!String.IsNullOrEmpty(errorMessage))
        EFPApp.ShowTempMessage(errorMessage);
    }

    #endregion

    #region Свойства и методы для IEFPSyncObject

    /// <summary>
    /// Этот метод реализует часть интерфейса IEFPSyncObject и не должен 
    /// вызываться из прикладной программы
    /// </summary>
    /// <param name="value"></param>
    public virtual void SyncMasterState(bool value)
    {
      InitEnabledEx();
      _EnabledSync.Value = value;
    }

    #endregion

    #region IEFPAppIdleHandler Members

    /// <summary>
    /// Метод обработки события Idle().
    /// Вызывается периодически, когда приложение ничем не занято.
    /// Чтобы метод вызывался, свойство UseIdle должно быть установлено в true
    /// </summary>
    public virtual void HandleIdle()
    {
      SaveConfig();

      if (Idle != null)
        Idle(this, EventArgs.Empty);

      if (_CommandItems != null)
        _CommandItems.HandleIdle(); // 28.01.2021

      if (_IdleValidationRequired)
      {
        _IdleValidationRequired = false;
        Validate();
      }
    }

    /// <summary>
    /// Если свойство установлено в true, то будет периодически вызываться событие HandleIdle.
    /// По умолчанию свойство не установлено.
    /// Свойство может устанавливаться производным классом только до вывода управляющего элемента на экран.
    /// Если установлен обработчик Idle, то свойство всегда возвращает true.
    /// Может вернуть true, если требуется вызвать метод OnShown().
    /// Также вызывает true, если список команд присоединен и содержит обработчики Idle.
    /// </summary>
    public bool UseIdle
    {
      get
      {
        if (_UseIdle || Idle != null)
          return true;

        if (_CommandItems != null)
        {
          if (_CommandItems.HasIdle)
            return true;
        }
        return false;
      }
      set
      {
        CheckHasNotBeenCreated();
        _UseIdle = value;
      }
    }
    private bool _UseIdle;

    /// <summary>
    /// Событие вызывается 1 раз в секунду или после обработки других сообщений.
    /// Обработчик должен присоединяться до вывода элемента на экран
    /// </summary>
    public event EventHandler Idle;

    #endregion

    #region Обновление по времени

    private class UpdateByTimeHandlerList : ListWithReadOnly<EFPUpdateByTimeHandler>
    {
      #region Конструктор

      public UpdateByTimeHandlerList(bool isReadOnly)
      {
        if (isReadOnly)
          SetReadOnly();
      }

      #endregion

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    /// <summary>
    /// Используется при обращении к свойству UpdateByTimeHandlers после вызова OnShown()
    /// </summary>
    private static readonly UpdateByTimeHandlerList _EmptyUpdateByTimeHandlers = new UpdateByTimeHandlerList(true);

    /// <summary>
    /// Сюда можно добавить обработчик для обновления данных по таймеру.
    /// Добавление обработчика возможно только до вывода формы на экран или показа элемента (до вызова OnShown)
    /// </summary>
    public ICollection<EFPUpdateByTimeHandler> UpdateByTimeHandlers
    {
      get
      {
        if (_UpdateByTimeHandlers == null)
        {
          if (HasBeenCreated)
            _UpdateByTimeHandlers = _EmptyUpdateByTimeHandlers;
          else
            _UpdateByTimeHandlers = new UpdateByTimeHandlerList(false);
        }
        return _UpdateByTimeHandlers;
      }
    }
    private UpdateByTimeHandlerList _UpdateByTimeHandlers;

    #endregion

    #region Сохранение конфигурации

    /// <summary>
    /// Менеджер чтения и записи конфигурации.
    /// Если не определен специально для этого управляющего элемента, берется из
    /// EFPBaseProvider. Если он не определен явно в цепочке провайдеров до EFPFormProvider,
    /// берется EFPApp.ConfigManager.
    /// Свойство не может возвращать null, т.к. в EFPApp.ConfigManager всегда есть заглушка
    /// </summary>
    public IEFPConfigManager ConfigManager
    {
      get
      {
        if (_ConfigManager == null)
          return BaseProvider.ConfigManager;
        else
          return _ConfigManager;
      }
      set
      {
        _ConfigManager = value;
      }
    }
    private IEFPConfigManager _ConfigManager;

    /// <summary>
    /// Если свойство возвращает непустую строку, то элемент умеет сохранять собственные
    /// данные в секциях конфигурации.
    /// Реализуется через ConfigHandler
    /// </summary>
    public string ConfigSectionName
    {
      get
      {
        if (ConfigHandler == null)
          return _ConfigSectionName;
        else
          return ConfigHandler.ConfigSectionName;
      }
      set
      {
        if (ConfigHandler != null)
          ConfigHandler.ConfigSectionName = value;
        else
          _ConfigSectionName = value;
      }
    }
    private string _ConfigSectionName;

    /// <summary>
    /// Этот метод вызывается в конструкторе производного класса, если управляющий
    /// элемент может сохранять свои настройки.
    /// Повторные вызовы игнорируются 
    /// </summary>
    protected void InitConfigHandler()
    {
      CheckHasNotBeenCreated();
      if (_ConfigHandler == null)
      {
        _ConfigHandler = new EFPConfigHandler();
        _ConfigHandler.ConfigSectionName = _ConfigSectionName;
        _ConfigSectionName = null;
        _ConfigHandler.Sources.Add(this);
      }
    }


    /// <summary>
    /// Обработчик конфигурации. Равен null до вызова InitConfigHandler()
    /// Чтобы зарегистрировать категорию, для которой будут записываться данные секции конфигурации, конструктор производного класса вызывает ConfigHandler.Categories.Add(“Filters”). Методы коллекции могут вызываться только до OnShown().
    /// Если нужно записать конфигурацию, то должен быть установлен флаг для категории вызовом ConfigHandler.Changed[“Filters”]=true.
    /// </summary>
    public EFPConfigHandler ConfigHandler { get { return _ConfigHandler; } }
    private EFPConfigHandler _ConfigHandler;

    /// <summary>
    /// Вызывает ConfigHandler.ReadConfig() для чтения значений.
    /// Ничего не делает, если ConfigHandler не инициализирован или свойство ConfigSectionName не установлено.
    /// Повторные вызовы метода игнорируются
    /// Первоначальное чтение конфигурации элемента.
    /// Как правило, этот метод вызывается из OnShown(), но может быть вызван досрочно внешним кодом.
    /// Не вызывайте этот метод из конструктора производного класса, так как конструктор класса-наследника
    /// (если он есть) может вызвать ошибку. К тому же, внешний код может, например, изменить ConfigManager.
    /// Не рекомендуется использовать этот метод без крайней необходимости.
    /// </summary>
    public void LoadConfig()
    {
      if (_LoadConfigCalled)
        return;
      _LoadConfigCalled = true;

      OnBeforeLoadConfig();
      try
      {
        if (ConfigHandler != null)
          OnLoadConfig();
      }
      finally
      {
        OnAfterLoadConfig();
      }

      if (EFPApp.InsideLoadComposition && ConfigHandler != null && ConfigManager != null)
        ConfigHandler.ReadConfig(ConfigManager); // 25.10.2021 Иначе не прочитается
    }

    /// <summary>
    /// Метод возвращает true, если LoadConfig() был вызван.
    /// Свойство действительно, дажн если ConfigManager=null
    /// </summary>
    public bool LoadConfigCalled { get { return _LoadConfigCalled; } }
    private bool _LoadConfigCalled;

    /// <summary>
    /// Генерирует исключение, если LoadConfig() был вызван
    /// </summary>
    public void CheckLoadConfigHasNotBeenCalled()
    {
      if (LoadConfigCalled)
        throw new InvalidOperationException("Для элемента " + this.ToString() + " уже была загружена конфигурация");
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента.
    /// Непереопределенный метод вызывает ConfigHandler.ReadConfig().
    /// Если свойство ConfigHandler=null, метод не вызывается.
    /// Следовательно, управляющий элемент, в котором определен данный класс, должен в конструкторе вызывать InitConfigHandler().
    /// Метод вызывается, даже если свойство ConfigSectionName не установлено.
    /// Обычно метод косвенно вызывается из OnShown(), но может быть вызван и раньше.
    /// </summary>
    protected virtual void OnLoadConfig()
    {
      if (!EFPApp.InsideLoadComposition)
        ConfigHandler.ReadConfig(this.ConfigManager);
    }


    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента перед вызовом LoadConfig().
    /// Метод вызывается независимо от установки свойства ConfigHandler.
    /// Переопределенный метод может, например, сохранить значения свойств, установленных на момент загрузки конфигурации
    /// </summary>
    protected virtual void OnBeforeLoadConfig()
    {
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента после вызова LoadConfig().
    /// Метод вызывается независимо от установки свойства ConfigHandler.
    /// </summary>
    protected virtual void OnAfterLoadConfig()
    {
    }


    /// <summary>
    /// Сохраняет все несохраненные изменения для элемента.
    /// Этот метод вызывается по таймеру и при закрытии формы.
    /// Если ConfigHandler=null, метод ничего не делает
    /// </summary>
    public void SaveConfig()
    {
      if (ConfigHandler != null)
        OnSaveConfig();
    }

    /// <summary>
    /// Этот метод сохраняет все несохраненные настройки для элемента.
    /// Вызывается, только если ConfigHandler!=null.
    /// Непереопределенный метод вызывает ConfigHandler.WriteConfigChanges().
    /// Переопреденный метод может сначала установить признаки ConfigHandler.Changed, чтобы учесть изменения, которые нужно сохранить,
    /// например, текущую позицию в просмотре
    /// </summary>
    protected virtual void OnSaveConfig()
    {
      ConfigHandler.WriteConfigChanges(this.ConfigManager);
    }

    #endregion

    #region Поиск провайдера

    /// <summary>
    /// Выполняет поиск провайдера для управляющего элемента.
    /// Если провайдер не найден, возвращает null.
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Найденный объект или null</returns>
    public static EFPControlBase FindControlProvider(Control control)
    {
      if (control == null)
        return null;

      Control Control2 = control;
      do
      {
        if (Control2 is Form)
        {
          EFPFormProvider FormProvider = EFPFormProvider.FindFormProvider((Form)Control2);
          if (FormProvider != null)
          {
            EFPControlBase Res = FormProvider.FindControlProvider(control); // не Control2!
            if (Res != null)
              return Res;
          }
        }

        // спускаемся ниже
        Control2 = Control2.Parent;
      }
      while (Control2 != null);

      return null;
    }

    private static readonly EFPControlBase[] _EmptyArray = new EFPControlBase[0];

    /// <summary>
    /// Возвращает список провайдеров управляющих элементов, дочерних по отношению к текущему провайдеру.
    /// Для этого перебираются все элементы EFPControlBase в EFPBaseProvider, включая рекурсивно вложенные EFPBaseProvider.
    /// Если элемент EFPControlBase.Control является вложенным (метод WinFormsTools.ContainsControl()).
    /// </summary>
    /// <returns>Массив найденных вложенных провайдеров</returns>
    public EFPControlBase[] GetNestedControlProviders()
    {
      List<EFPControlBase> list = null; // Создадим по готовности
      AddNestedControlProviders(ref list, BaseProvider); // рекурсивный вызов
      if (list == null)
        return _EmptyArray;
      else
        return list.ToArray();
    }

    private void AddNestedControlProviders(ref List<EFPControlBase> list, EFPBaseProvider currProvider)
    {
      foreach (EFPControlBase item in currProvider.ControlProviders)
      {
        if (item == this)
          continue;
        Control[] childControls = item.GetControls();
        for (int i = 0; i < childControls.Length; i++)
        {
          if (WinFormsTools.ContainsControl(this.Control, childControls[i]))
          {
            // нашли
            if (list == null)
              list = new List<EFPControlBase>();
            list.Add(item);
            break; // однократное добавление
          }
        }
      }

      foreach (EFPBaseProvider item in currProvider.Children)
        AddNestedControlProviders(ref list, item); // рекурсивный вызов
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Получить список категорий, которые должны быть обработаны.
    /// Непереопределенный метод не выполняет никаких действий.
    /// </summary>
    /// <param name="categories">Список для добавления категорий</param>
    /// <param name="rwMode">Чтение или запись</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public virtual void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      if (rwMode == EFPConfigMode.Write)
        UpdateFormProviderState();
    }

    /// <summary>
    /// Выполнить запись одной секции конфигурации.
    /// Непереопределенный метод не выполняет никаких действий.
    /// </summary>
    /// <param name="category">Категория записываемой секции</param>
    /// <param name="cfg">Объект для записb значений</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public virtual void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    /// <summary>
    /// Выполнить чтение одной секции конфигурации.
    /// Непереопределенный метод не выполняет никаких действий.
    /// </summary>
    /// <param name="Category">Категория считываемой секции</param>
    /// <param name="cfg">Объект для чтения значений</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public virtual void ReadConfigPart(string Category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    #endregion

    #region Отладка

    /// <summary>
    /// Добавляет в объект Exception информацию об управляющем элементе.
    /// Вызывается в блоке catch при перехвате исключения
    /// </summary>
    /// <param name="e">Заполняемое исключение</param>
    protected virtual void AddExceptionInfo(Exception e)
    {
      try
      {
        e.Data["EFPControlBase.DisplayName"] = this.DisplayName;
        e.Data["EFPControlBase.GetType()"] = this.GetType().ToString();
        e.Data["EFPControlBase.ProviderState"] = ProviderState;
        e.Data["Control.GetType()"] = Control.GetType().ToString();
        try
        {
          e.Data["Control"] = Control.ToString();
          e.Data["Control.IsDisposed"] = Control.IsDisposed;
          e.Data["EFPControlBase.ControlVisible"] = ControlVisible;
          e.Data["EFPControlBase.ControlEnabled"] = ControlEnabled;
        }
        catch { }

        try
        {
          if (BaseProvider.FormProvider == null)
            e.Data["FormProvider"] = "null";
          else
          {
            e.Data["Form.GetType()"] = BaseProvider.FormProvider.Form.GetType().ToString();
            try { e.Data["FormProvider"] = BaseProvider.FormProvider.ToString(); }
            catch { }
            e.Data["FormProvider.Modal"] = BaseProvider.FormProvider.Modal;
            e.Data["FormProvider.Visible"] = BaseProvider.FormProvider.Visible;
          }
        }
        catch { }
      }
      catch { }
    }

    #endregion
  }


  /// <summary>
  /// Абстрактный базовый класс для реализации провайдеров для нескольких управляющих
  /// элементов. Реализует свойство "EnabledEx"
  /// </summary>
  /// <typeparam name="T">Класс управляющего элемента, производного от Control</typeparam>
  public class EFPControl<T> : EFPControlBase, IEFPControl
    where T : Control
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, TextBox, обычно нужна метка.
    /// Если false, то метка не нужна (например, CheckBox)</param>
    public EFPControl(EFPBaseProvider baseProvider, T control, bool labelNeeded)
      : base(baseProvider, control, labelNeeded)
    {
      if (!DesignMode)
      {
        control.VisibleChanged += new EventHandler(ControlVisibleChanged);
        control.EnabledChanged += new EventHandler(ControlEnabledChanged);
      }
    }

    /// <summary>
    /// Создает провайдер для управляющего элемента с использование ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    /// <param name="labelNeeded">Если true, то элементу, например, TextBox, обычно нужна метка.
    /// Если false, то метка не нужна (например, CheckBox)</param>
    public EFPControl(IEFPControlWithToolBar<T> controlWithToolBar, bool labelNeeded)
      : this(controlWithToolBar.BaseProvider, controlWithToolBar.Control, labelNeeded)
    {
    }

    #endregion

    #region Переопределенные свойства

    /// <summary>
    /// Управляющий элемент, к которому относится провайдер
    /// </summary>
    public new T Control { get { return (T)(base.Control); } }

    /// <summary>
    /// Свойство Control.Visible
    /// </summary>
    protected override bool ControlVisible
    {
      get { return Control.Visible; }
      set { Control.Visible = value; }
    }

    /// <summary>
    /// Свойство Control.Enabled
    /// </summary>
    protected override bool ControlEnabled
    {
      get { return Control.Enabled; }
      set { Control.Enabled = value; }
    }

    #endregion
  }

  /// <summary>
  /// Расширение класса EFPControl для реализации методов интерфейса IEFPSyncObject
  /// </summary>
  /// <typeparam name="T">Класс управляющего элемента Control</typeparam>
  public abstract class EFPSyncControl<T> : EFPControl<T>, IDepSyncObject
    where T : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, TextBox, обычно нужна метка.
    /// Если false, то метка не нужна (например, CheckBox)</param>
    public EFPSyncControl(EFPBaseProvider baseProvider, T control, bool labelNeeded)
      : base(baseProvider, control, labelNeeded)
    {
      _SyncGroup = null;
      _SyncMaster = false;
    }

    /// <summary>
    /// Создает провайдер для управляющего элемента с использование ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    /// <param name="labelNeeded">Если true, то элементу, например, TextBox, обычно нужна метка.
    /// Если false, то метка не нужна (например, CheckBox)</param>
    public EFPSyncControl(IEFPControlWithToolBar<T> controlWithToolBar, bool labelNeeded)
      : base(controlWithToolBar, labelNeeded)
    {
      _SyncGroup = null;
      _SyncMaster = false;
    }

    #endregion

    #region Свойства IEFPSyncObject

    /// <summary>
    /// True, если объект является ведущим в группе синхронизации.
    /// False, если объект является ведомым и будет заблокированным
    /// </summary>
    public bool SyncMaster
    {
      get
      {
        return _SyncMaster;
      }
      set
      {
        if (value == _SyncMaster)
          return;
        _SyncMaster = value;
        if (_SyncGroup != null)
          _SyncGroup.ObjectSyncMasterChanged((IDepSyncObject)this);
      }
    }
    private bool _SyncMaster;

    /// <summary>
    /// Группа синхронизации, к которой в настоящий момент подключен управляющий
    /// элемент.
    /// Это свойство реализует часть интерфейса IEFPSyncObject и не должно 
    /// устанавливаться из прикладной программы.
    /// </summary>
    public DepSyncGroup SyncGroup
    {
      get
      {
        return _SyncGroup;
      }
      set
      {
        _SyncGroup = value;
      }
    }
    private DepSyncGroup _SyncGroup;

    /// <summary>
    /// Доступ к синхронизированному значению должен быть определен в классе-наследнике
    /// </summary>
    public abstract object SyncValue { get; set; }


    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Вызывается в обработчиках изменения редактируемого значения для передачи
    /// значения в группу синхронизации в режиме SyncMaster=true
    /// </summary>
    protected void DoSyncValueChanged()
    {
      if (SyncMaster && (SyncGroup != null))
      {
        object value = ((
          IDepSyncObject)this).SyncValue;
        SyncGroup.Value = value;
      }
    }

    #endregion
  }
}
