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
  /// Свойство <see cref="EFPControlBase.ProviderState"/>.
  /// </summary>
  public enum EFPControlProviderState
  {
    /// <summary>
    /// Исходное состояние. Вызван конструктор класса, но не вызывалось событие <see cref="EFPControlBase.Created"/>
    /// </summary>
    Initialization,

    /// <summary>
    /// Вызвано событие <see cref="EFPControlBase.Created"/>, но еще не вызывалось событие <see cref="EFPControlBase.Attached"/>.
    /// </summary>
    Created,

    /// <summary>
    /// Вызвано событие <see cref="EFPControlBase.Attached"/>. Элемент присоединен к форме, которая сейчас находится на экране
    /// (при этом свойство <see cref="Control.Visible"/> может быть false, если элемент был временно или постоянно скрыт).
    /// В это состояние элемент может перейти повторно из <see cref="Detached"/>.
    /// </summary>
    Attached,

    /// <summary>
    /// Вызвано событие <see cref="EFPControlBase.Detached"/>. Обычно вызывается скрытием формы с экрана, но может быть из-за отсоединения <see cref="EFPBaseProvider"/>
    /// от родительского провайдера. После этого провайдер может снова перейти в состояние <see cref="Attached"/>, если блок диалога
    /// повторно выводится на экран, или устанавливается свойство <see cref="EFPBaseProvider.Parent"/>.
    /// </summary>
    Detached,

    /// <summary>
    /// Финальное состояние. Вызвано событие <see cref="EFPControlBase.Disposed"/>.
    /// Элемент больше не может быть выведен на экран.
    /// </summary>
    Disposed,
  }

  #endregion

  #region Интерфейсы

  /// <summary>
  /// Интерфейс провайдера управляющего элемента.
  /// Реализуется только классом <see cref="EFPControlBase"/>.
  /// Используется в качестве базового для специализированных интерфейсов, например, <see cref="IEFPDataView"/>.
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
    /// Управляет видимостью элемента. 
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Связываемое свойство видимости элемента <see cref="Visible"/>.
    /// </summary>
    DepValue<Boolean> VisibleEx { get; set; }

    /// <summary>
    /// Управляет блокировкой элемента.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Связываемое свойство блокировки элемента <see cref="Enabled"/>.
    /// </summary>
    DepValue<Boolean> EnabledEx { get; set; }

    /// <summary>
    /// Возвращает true, если элемент доступен для воздействия пользователя.
    /// Для обычных элементов возвращает true, если и <see cref="Visible"/> и <see cref="Enabled"/> установлены в true.
    /// Для <see cref="EFPTextBox"/> и других полей ввода, также проверяется, что <see cref="EFPTextBoxControlWithReadOnly{TextBox}.ReadOnly"/>=false.
    /// </summary>
    bool Editable { get; }

    /// <summary>
    /// Связываемое свойство <see cref="Editable"/>.
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
    /// Свойство возвращает true, если управляющий элемент был выведен на экран или сейчас будет выведен.
    /// Свойство однократно переходит из false в true. Перед этим вызывается событие <see cref="Created"/>.
    /// Свойство <see cref="System.Windows.Forms.Control.Visible "/> может многократно изменяться еще до вывода элемента на экран.
    /// </summary>
    bool HasBeenCreated { get; }

    /// <summary>
    /// Возвращает true, если для данного вида управляющих элементов желательно использование метки
    /// </summary>
    bool LabelNeeded { get; }

    /// <summary>
    /// Возвращает и позволяет установить метку для управляющего элемента.
    /// Меткой может быть <see cref="Label"/> или <see cref="GroupBox"/>.
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
    /// Событие вызывается из <see cref="EFPControlBase.PrepareCommandItems()"/>, перед тем, как он выполнит инициализацию локального меню,
    /// панелей инструментов и статусной строки.
    /// Пользовательский обработчик может, например, выполнить окончательную настройку команд.
    /// </summary>
    event EventHandler BeforePrepareCommandItems;

    /// <summary>
    /// Панель инструментов для локального меню.
    /// Если управляющий элемент создан без использования <see cref="EFPControlWithToolBar{Control}"/>, но ему требуется
    /// панель инструментов, то следует установить это свойство.
    /// </summary>
    Panel ToolBarPanel { get; set; }

    /// <summary>
    /// Провайдер панели инструментов.
    /// Создается при установке свойства <see cref="ToolBarPanel"/>.
    /// </summary>
    EFPPanelToolBar ToolBar { get; }

    /// <summary>
    /// Менеджер чтения и записи конфигурации.
    /// Если не определен специально для этого управляющего элемента, берется из
    /// <see cref="EFPBaseProvider"/>. Если он не определен явно в цепочке провайдеров до <see cref="EFPFormProvider"/>,
    /// берется <see cref="EFPApp.ConfigManager"/>.
    /// Свойство не может возвращать null, т.к. в <see cref="EFPApp.ConfigManager"/> всегда есть заглушка.
    /// </summary>
    IEFPConfigManager ConfigManager { get; }

    /// <summary>
    /// Обработчик конфигурации. Для элементов, не выполняющих сохранение конфигурации, обычно возвращает null.
    /// </summary>
    EFPConfigHandler ConfigHandler { get; }

    /// <summary>
    /// Имя секции конфигурации для хранения настроек элемента.
    /// Реализуется объектом <see cref="EFPConfigHandler"/>.
    /// Если свойство <see cref="ConfigHandler"/> равно null, <see cref="ConfigSectionName"/> возвращает пустую строку и установить свойство нельзя.
    /// </summary>
    string ConfigSectionName { get; set; }

    /// <summary>
    /// Подсказка для управляющего элемента - описание назначения управляющего 
    /// элемента (не включая сообщение об ошибке и подсказку по текущему значению)
    /// </summary>
    string ToolTipText { get; set; }

    /// <summary>
    /// Подсказка по текущему значению.
    /// Если используется, то свойство должно устанавливаться при изменении 
    /// пользователем введенного значения.
    /// </summary>
    string ValueToolTipText { get; set; }

    /// <summary>
    /// Выполнить проверку значений.
    /// Проверка не выполняется, пока форма не выведена на экран. При выводе формы
    /// проверка выполняется для всех управляющих элементов.
    /// </summary>
    void Validate();

    /// <summary>
    /// Специальная версия выполнения проверки для управляющего элемента, которую
    /// можно использовать в обработчике изменения значения в другом элементе в виде
    /// efp1.Value.ValueChanged+=new EventHandler(efp2.Validate);
    /// Аргументы игнорируются, вызывается обычная версия <see cref="Validate()"/> без аргументов
    /// </summary>
    /// <param name="sender">Источник события - игнорируется</param>
    /// <param name="args">Аргументы события - игнорируются</param>
    void Validate(object sender, EventArgs args);

    /// <summary>
    /// Пользовательский обработчик для проверки ошибок
    /// </summary>
    event UIValidatingEventHandler Validating;

    /// <summary>
    /// Если свойство установлено в true, то метод <see cref="Validate()"/> будет дополнительно вызываться после получения и
    /// потери элементом фокуса ввода.
    /// Вызов метода <see cref="Validate()"/> выполняется не сразу в обработчике событий <see cref="System.Windows.Forms.Control.Enter"/> и <see cref="System.Windows.Forms.Control.Leave"/>,
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
    /// Возвращает true, если список <see cref="Validators"/> содержит элементы
    /// </summary>
    bool HasValidators { get; }

    /// <summary>
    /// Передать фокус ввода управляющему элементу.
    /// Если управляющий элемент находится на вкладке <see cref="TabPage"/>, которая не активна,
    /// то она активируется.
    /// </summary>
    void SetFocus();

    /// <summary>
    /// Передать фокус ввода управляющему элементу и выдать сообщение об ошибке
    /// Если управляющий элемент находится на вкладке <see cref="TabPage"/>, которая не активна,
    /// то она активируется.
    /// </summary>
    /// <param name="errorMessage">Строка сообщения</param>
    void SetFocus(string errorMessage);

    /// <summary>
    /// Если свойство установлено, то у управляющего элемента будет собственная кнопка по умолчанию.
    /// Когда элемент получает фокус ввода, то свойство <see cref="System.Windows.Forms.Form.AcceptButton"/> временно переключается
    /// на указанную кнопку, а после утери фокуса восстанавливается на предыдущее значение (кнопку "ОК" в диалоге).
    /// Свойство можно устанавливать только до вывода формы на экран.
    /// </summary>
    IButtonControl DefaultButton { get; set; }

    /// <summary>
    /// Сюда можно добавить обработчик для обновления данных по таймеру.
    /// Добавление обработчика возможно только до вывода формы на экран или показа элемента (до вызова <see cref="Created"/>)
    /// </summary>
    ICollection<EFPUpdateByTimeHandler> UpdateByTimeHandlers { get; }

    /// <summary>
    /// Событие вызывается из обработчика события <see cref="System.Windows.Forms.Control.Enter"/> перед выполнением стандартных действий.
    /// Обработчик может, например, изменить текущее значение элемента до выполнения проверки.
    /// Если требуется присоединить обработчик, вызываемый после выполнения проверки, используйте оригинальное событие <see cref="System.Windows.Forms.Control.Enter"/>.
    /// </summary>
    event EventHandler Enter;

    /// <summary>
    /// Событие вызывается из обработчика события <see cref="System.Windows.Forms.Control.Leave"/> перед выполнением стандартных действий.
    /// Обработчик может, например, изменить текущее значение элемента до выполнения проверки.
    /// Если требуется присоединить обработчик, вызываемый после выполнения проверки, используйте оригинальное событие <see cref="System.Windows.Forms.Control.Leave"/>.
    /// </summary>
    event EventHandler Leave;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    object Tag { get; set; }
  }

  #endregion

  /// <summary>
  /// Абстрактный базовый класс для шаблонного класса <see cref="EFPControl{T}"/>.
  /// Реализует свойства, общие для всех провайдеров управляющих элементов.
  /// </summary>
  public abstract class EFPControlBase : IEFPControl, IUIValidableObject, IEFPStatusBarControl, IEFPAppIdleHandler, IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, <see cref="TextBox"/>, обычно нужна метка.
    /// Если false, то метка не нужна (например, <see cref="CheckBox"/>)</param>
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

      _BaseProvider = baseProvider;
      _BaseProvider.AddControlProvider(this);

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
        control.Leave += new EventHandler(Control_Leave);

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
      // 04.02.2021
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
    /// Базовый провайдер.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private readonly EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Управляющий элемент, к которому относится провайдер.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public Control Control { get { return _Control; } }
    private readonly Control _Control;

    /// <summary>
    /// Не должно использоваться в прикладном коде.
    /// Переопределяется для <see cref="EFPRadioButtons"/>.
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
    /// элементе.
    /// Например, для поля <see cref="TextBox"/> (и большинства других элементов) возвращается true,
    /// а для <see cref="CheckBox"/> - false.
    /// Задается в конструкторе.
    /// </summary>
    public bool LabelNeeded { get { return _LabelNeeded; } }
    private readonly bool _LabelNeeded;

    /// <summary>
    /// Если свойство установлено, то у управляющего элемента будет собственная кнопка по умолчанию.
    /// Когда элемент получает фокус ввода, то свойство <see cref="System.Windows.Forms.Form.AcceptButton"/> временно переключается
    /// на указанную кнопку, а после утери фокуса восстанавливается на предыдущее значение (кнопку "ОК" в диалоге).
    /// Свойство можно устанавливать только до вывода формы на экран.
    /// </summary>
    public IButtonControl DefaultButton
    {
      get { return _DefaultButton; }
      set
      {
        CheckHasNotBeenCreated();
        _DefaultButton = value;
      }
    }
    private IButtonControl _DefaultButton;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Свойство возвращает true, если компонент <see cref="Control"/> находится в режиме разработки
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

      if (InsideSetProviderState)
        return; // реентрантный вызов

      if (ProviderState == EFPControlProviderState.Initialization)
      {
        InternalSetProviderState(EFPControlProviderState.Created, EFPControlSetProviderStateReason.UpdateFormProviderState);
        if (ProviderState == EFPControlProviderState.Initialization)
          // возникла ошибка, отчет выведен
          return; // 09.07.2021
      }

      switch (ProviderState)
      {
        case EFPControlProviderState.Attached:
          if (!BaseProvider.IsFormVisible)
            InternalSetProviderState(EFPControlProviderState.Detached, EFPControlSetProviderStateReason.UpdateFormProviderState);
          break;
        case EFPControlProviderState.Created:
        case EFPControlProviderState.Detached:
          if (BaseProvider.IsFormVisible && ControlVisible)
            InternalSetProviderState(EFPControlProviderState.Attached, EFPControlSetProviderStateReason.UpdateFormProviderState);
          else
            InitLabelVisible(); // Иначе будут висячие метки
          break;
        case EFPControlProviderState.Disposed:
          break;
        default:
          throw new BugException("ProviderState=" + ProviderState.ToString());
      }

      // 26.04.2022
      if (ProviderState == EFPControlProviderState.Detached && Control.IsDisposed)
        InternalSetProviderState(EFPControlProviderState.Disposed, EFPControlSetProviderStateReason.UpdateFormProviderState);

      UpdateCommandItemsActive(); // 02.05.2022
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

    private enum EFPControlSetProviderStateReason
    {
      None,
      UpdateFormProviderState,
      ControlDisposed
    }

    /// <summary>
    /// Возвращает true, если в данный момент выполняется установка состояния <see cref="ProviderState"/> и вызывается один из обработчиков.
    /// При этом свойство <see cref="ProviderState"/> возврашает новое значение.
    /// </summary>
    public bool InsideSetProviderState { get { return _InsideSetProviderStateReason != EFPControlSetProviderStateReason.None; } }
    private EFPControlSetProviderStateReason _InsideSetProviderStateReason;

    private void InternalSetProviderState(EFPControlProviderState value, EFPControlSetProviderStateReason reason)
    {
#if DEBUG
      if (reason == EFPControlSetProviderStateReason.None)
        throw new ArgumentException("reason=None", "reason");
#endif

      if (InsideSetProviderState)
      {
        if (value == _ProviderState)
          return;
        else
        {
          Exception e = new ReenteranceException();
          AddExceptionInfo(e);
          e.Data["InternalSetProviderState.NewState"] = value;
          e.Data["InternalSetProviderState.OldState"] = _ProviderState;
          e.Data["InternalSetProviderState.NewReason"] = reason;
          e.Data["InternalSetProviderState.OldReason"] = _InsideSetProviderStateReason;
          throw e;
        }
      }

      EFPControlProviderState oldState = _ProviderState;

      _InsideSetProviderStateReason = reason;
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
          e.Data["InternalSetProviderState.Reason"] = reason;
          _ProviderState = oldState; // 07.07.2021
#if DEBUG
          if (TraceProviderState)
            Trace.WriteLine("EFPControl.ProviderState changing from " + oldState.ToString() + " to " + value.ToString() + " failed. Type=" + this.GetType().ToString() + ". DisplayName=" + DisplayName.ToString());
#endif

          if (value == EFPControlProviderState.Disposed)
            LogoutTools.LogoutException(e, Res.EFPControl_ErrTitle_SetProviderStateDisposed);
          else
            throw; // 17.07.2021
        }
      }
      finally
      {
        _InsideSetProviderStateReason = EFPControlSetProviderStateReason.None;
      }
    }

    /// <summary>
    /// Проверка корректности переключения состояния
    /// </summary>
    /// <param name="oldState">Старое состояние</param>
    /// <param name="newState">Новое состояние</param>
    private void CheckIsValidNewProviderState(EFPControlProviderState oldState, EFPControlProviderState newState)
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
        throw ExceptionFactory.ObjectPropertySwitch(this, "ProviderState", oldState, newState);
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
    /// Это событие вызывается однократно.
    /// </summary>
    public event EventHandler Created;

    /// <summary>
    /// Вызывает событие <see cref="Created"/>. См. описание события.
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
    /// Свойство однократно переходит из false в true. Перед этим вызывается событие <see cref="Created"/>.
    /// Свойство <see cref="System.Windows.Forms.Control.Visible"/> может многократно изменяться еще до вывода элемента на экран.
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
    /// Если для элемента свойство <see cref="HasBeenCreated"/> установлено в true, генерирует исключение <see cref="InvalidOperationException"/>.
    /// </summary>
    public void CheckHasNotBeenCreated()
    {
      if (HasBeenCreated)
        throw new InvalidOperationException(String.Format(Res.EFPControl_Err_Created, ToString()));
    }

    #endregion

    #region 2. Событие Attached

    /// <summary>
    /// Событие вызывается, когда форма выводится на экран или когда элемент присоединяется к видимой форме.
    /// Событие не вызывается, если на момент присоединения к форме, свойство <see cref="System.Windows.Forms.Control.Visible"/>=false. 
    /// Событие будет вызвано позже, когда элемент управления станет видимым. 
    /// Это событие может вызываться многократно, чередуясь с событием <see cref="Detached"/>.
    /// </summary>
    public event EventHandler Attached;

    /// <summary>
    /// Вызывает событие <see cref="Attached"/>. См. описание события.
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

      UpdateVisibleState();

      AttachValidators();

      if (ToolBarPanel != null)
        PrepareContextMenu(); // 24.06.2021
      UpdateCommandItemsActive();
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
    /// Событие не вызывается, если управляющий элемент становится невидимым (<see cref="System.Windows.Forms.Control.Visible"/>=false),
    /// но продолжает оставаться присоединенным к форме. 
    /// Событие не вызывается, как и событие <see cref="Attached"/>, если форма была выведена на экран, но свойство <see cref="System.Windows.Forms.Control.Visible"/> все время оставалось равным false.
    /// Это событие может вызываться многократно, чередуясь с событием <see cref="Attached"/>.
    /// </summary>
    public event EventHandler Detached;

    /// <summary>
    /// Вызывает событие <see cref="Detached"/>. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnDetached()
    {
      if (UseIdle) // проверка условия предотвратит бесполезный вызов Remove()
        EFPApp.IdleHandlers.Remove(this);

      EFPFormProvider formProvider = BaseProvider.FormProvider;
      if (formProvider != null)
      {
        if (_UpdateByTimeHandlers != null)
        {
          for (int i = 0; i < _UpdateByTimeHandlers.Count; i++)
            formProvider.UpdateByTimeHandlers.Remove(_UpdateByTimeHandlers[i]);
        }

        //if (formProvider.LastFocusedControl == this.Control)
        //  formProvider.LastFocusedControl = null;
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
    /// Событие вызывается в ответ на установку <see cref="System.Windows.Forms.Control.IsDisposed"/>.
    /// Событие вызывается однократно.
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Вызывает событие <see cref="Disposed"/>. См. описание события.
    /// Переопределенный метод должен обязательно вызывать метод базового класса, иначе работа будет нарушена.
    /// </summary>
    protected virtual void OnDisposed()
    {
      if (Disposed != null)
        Disposed(this, EventArgs.Empty);
    }

    void Control_Disposed(object sender, EventArgs args)
    {
      if (_InsideSetProviderStateReason == EFPControlSetProviderStateReason.ControlDisposed)
        return; // 28.03.2022. Вложенный вызов события

      if (_InsideSetProviderStateReason == EFPControlSetProviderStateReason.UpdateFormProviderState)
        return; // 26.04.2022. Может возникнуть событие Control.Disposed в процессе UpdateFormProviderState

      try
      {
        if (ProviderState == EFPControlProviderState.Attached)
          InternalSetProviderState(EFPControlProviderState.Detached, EFPControlSetProviderStateReason.ControlDisposed); // 16.09.2021
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        LogoutTools.LogoutException(e);
        // Будет и вторая ошибка при установке Disposed
      }

      try
      {
        InternalSetProviderState(EFPControlProviderState.Disposed, EFPControlSetProviderStateReason.ControlDisposed);
      }
      catch (Exception e) // 25.01.2022
      {
        AddExceptionInfo(e);
        LogoutTools.LogoutException(e);
      }
    }

    #endregion

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Видимость управляющего элемента
    /// Возвращает и устанавливает свойство <see cref="System.Windows.Forms.Control.Visible"/> управляющего элемента.
    /// Установка значения true может оказаться отложенной, если в момент вызова
    /// один из родительских элементов не является видимым.
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
    /// Расширенное свойство <see cref="Visible"/>.
    /// В .Net Framework свойство <see cref="System.Windows.Forms.Control.Visible"/> изменяется, когда изменяется
    /// видимость одного из родительских элементов (например, самой формы).
    /// При этом, когда свойство переключается в false, событие <see cref="System.Windows.Forms.Control.VisibleChanged"/>
    /// не посылается, поэтому в <see cref="Visible"/> может оказаться неактуальное значение.
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
        if (DesignMode)
          return; // 31.03.2025

        EFPFormProvider formProvider = BaseProvider.FormProvider;
        if (formProvider == null)
          return; // 09.06.2021. До присоединения к форме - игнорируем

        if (ControlVisible && formProvider.Form.Visible /* 09.06.2021 */)
        {
          formProvider.FirstReadConfig(); // 04.10.2018. Выполняем предзагрузку секций конфигурации.
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
        EFPApp.ShowException(e);
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
    /// Свойство возвращает true, когда <see cref="ProviderState"/>=Attached и <see cref="Visible"/>=true.
    /// В отличие от основного свойства <see cref="Visible"/>, это свойство имеет одинаковый "жизненный цикл",
    /// независимо от способа показа формы и прикрепления к ней управляющего элемента.
    /// Сначала свойство имеет значение false. Затем, при показе формы, оно приобретает значение true,
    /// если элемент является видимым. Если свойство <see cref="Visible"/> меняется в процессе показа формы, то <see cref="VisibleState"/>
    /// также меняется. Затем, при скрытии элемента или его отключении, свойство переходит в false.
    /// </summary>
    public bool VisibleState { get { return ProviderState == EFPControlProviderState.Attached && Visible; } }

    /// <summary>
    /// Этот метод вызывается при изменении значения свойства <see cref="VisibleState"/>
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
    /// Возвращает и устанавливает свойство <see cref="System.Windows.Forms.Control.Enabled"/> управляющего элемента.
    /// Установка значения true может оказаться отложенной, если в момент вызова
    /// один из родительских элементов не является доступным.
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
    /// Расширенное свойство <see cref="Enabled"/>.
    /// В .Net Framework свойство <see cref="System.Windows.Forms.Control.Enabled "/> изменяется, когда изменяется
    /// доступность одного из родительских элементов (например, GroupBox).
    /// В отличие от <see cref="System.Windows.Forms.Control.VisibleChanged"/>, событие <see cref="System.Windows.Forms.Control.EnabledChanged"/> посылается
    /// при любом перключении, поэтому свойство <see cref="Enabled"/> содержит актуальное значение.
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
    /// Свойство <see cref="System.Windows.Forms.Control.Enabled"/>
    /// </summary>
    protected abstract bool ControlEnabled { get; set; }

    /// <summary>
    /// Сюда должен быть присоединен обработчик события <see cref="System.Windows.Forms.Control.EnabledChanged"/>
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
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region Свойство EnabledState

    /// <summary>
    /// "Доступное" состояние.
    /// Для большинства управляющих элементов свойство дублирует <see cref="System.Windows.Forms.Control.Enabled"/>.
    /// Для <see cref="EFPTextBox"/> и некоторых других возвращает комбинацию <see cref="Enabled"/> and not ReadOnly.
    /// Свойство <see cref="System.Windows.Forms.Control.Visible"/> не учитывается
    /// </summary>
    public virtual bool EnabledState { get { return Enabled; } }

    /// <summary>
    /// Этот метод вызывается при изменении значении свойства <see cref="EnabledState"/>.
    /// Переопределенный метод должен обязательно вызывать метод базового класса.
    /// </summary>
    protected virtual void OnEnabledStateChanged()
    {
      Validate();
    }

    /// <summary>
    /// Проверяет, не изменилось ли значение свойства <see cref="EnabledState"/>, и вызывает метод OnEnabledStateChanged() при необходимости.
    /// Этот метод должен вызываться из установщика свойства ReadOnly, если свойство есть в производном классе.
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
    /// Свойство возвращает true, если свойства <see cref="Visible"/> и <see cref="EnabledState"/> установлены в true.
    /// </summary>
    public bool Editable { get { return Visible && EnabledState; } }

    /// <summary>
    /// Управляемое свойство, возвращающее установку свойств <see cref="Visible"/> и <see cref="Enabled"/>
    /// (и сброс ReadOnly, если оно есть).
    /// Свойство может быть использовано для упрощения организации блокировок в
    /// качестве входа для свойства <see cref="EnabledEx"/> другого элемента.
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
    /// По умолчанию свойство устанавливается на предыдущий управляющий элемент (вызовом <see cref="System.Windows.Forms.Control.GetNextControl(System.Windows.Forms.Control, bool)"/> для родительского элемента),
    /// если он является <see cref="Label"/> или <see cref="GroupBox"/>.
    /// Свойство не имеет значения, если в конструкторе задана аргумент <see cref="LabelNeeded"/>=false (зависит от типа управляющего элемента).
    /// Свойству может быть присвоено значение null, 
    /// <see cref="System.Windows.Forms.Label"/>, 
    /// <see cref="System.Windows.Forms.GroupBox"/>, 
    /// <see cref="System.Windows.Forms.CheckBox"/> или 
    /// <see cref="System.Windows.Forms.RadioButton"/>. Использование других
    /// типов управляющих элементов не допускается.
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
            throw ExceptionFactory.ArgUnknownValue("value", value, new Type[] {
            typeof(Label), typeof(GroupBox), typeof(CheckBox), typeof(RadioButton)});

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

      Control prevControl = Control.Parent.GetNextControl(Control, false);

      if (prevControl == null)
      {
        if (Control.Parent is GroupBox)
          Label = Control.Parent;
        return;
      }
      if (prevControl is Label)
      {
        if (((Label)prevControl).UseMnemonic) // 22.03.2016
          Label = prevControl;
        return;
      }

      // 02.03.2013
      if ((prevControl is Panel) && (Control.Parent is GroupBox) && (Control.Dock == DockStyle.Fill))
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
            return WinFormsTools.RemoveMnemonic(Label.Text);
          if (LabelNeeded)
            return DefaultDisplayName;
          if (String.IsNullOrEmpty(Control.Text))
            //return Control.GetType().ToString();
            return DefaultDisplayName; // 15.08.2020
          return WinFormsTools.RemoveMnemonic(Control.Text);
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
    /// Используется в качестве возвращаемого значения свойства <see cref="DisplayName"/>, если свойство не установлено явно и у элемента нет метки.
    /// Непереопределенное свойство возвращает "Без названия".
    /// Для некоторых специализированных элементов может быть переопределено.
    /// </summary>
    protected virtual string DefaultDisplayName
    {
      get
      {
        //return "Без названия";
        return Control.GetType().Name; // 18.06.2024
      }
    }

    /// <summary>
    /// Возвращает <see cref="DisplayName"/> плюс вспомогательную информацию (Disposes, Hidden, Disabled)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DisplayName);
      if (String.IsNullOrEmpty(_DisplayName))
      {
        sb.Append(" (");
        sb.Append(GetType().ToString());
        sb.Append(")");
      }
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
    /// Подсказка по текущему значению.
    /// Если используется, то свойство должно устанавливаться при изменении 
    /// пользователем введенного значения.
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
    /// Непереопределенный метод вызывает <see cref="InitToolTips(Control)"/> для <see cref="Control"/> и его рекурсивно дочерних управляющих элементов,
    /// если для них нет собственных объектов <see cref="EFPControlBase"/>.
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
    /// Список команд локального меню.
    /// Доступ к свойству создает предопределенный или пустой список команд локального меню, к которому можно добавить собственные команды.
    /// Допускается установка списка команд "снаружи". Она должна выполняться до любого обращения к свойству.
    /// Для использования локальной панели инструментов следует установить свойство <see cref="ToolBarPanel"/> или
    /// использовать конструктор (производного класса), принимающий <see cref="IEFPControlWithToolBar"/>.
    /// Чтобы получить список без его принудительного создания, используйте свойство <see cref="CommandItemsIfAssigned"/>.
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
            _CommandItems = CreateCommandItems();

          OnCommandItemsCreated();
        }
        return _CommandItems;
      }
      set
      {
        _CommandItems = value;
        OnCommandItemsCreated();
      }
    }
    private EFPControlCommandItems _CommandItems;

    /// <summary>
    /// Возвращает <see cref="CommandItems"/>, только если команды уже были инициализированы
    /// </summary>
    protected EFPControlCommandItems CommandItemsIfAssigned
    {
      get { return _CommandItems; }
    }

    private bool _InsideInitCommandItems;

    /// <summary>
    /// Метод должен создать объект <see cref="EFPControlCommandItems"/>.
    /// Если производный класс использует собственный список команд, он должен 
    /// переопределить этот метод.
    /// При этом вызывать метод базового класса <see cref="EFPControlBase.CreateCommandItems()"/> не нужно.
    /// </summary>
    /// <returns></returns>
    protected virtual EFPControlCommandItems CreateCommandItems()
    {
      return new EFPControlCommandItems(this);
    }

    /// <summary>
    /// Этот метод однократно вызывается после инициализации свойства <see cref="CommandItems"/>.
    /// </summary>
    protected virtual void OnCommandItemsCreated()
    {
    }

    /// <summary>
    /// Возвращает true, если было обращение к свойству <see cref="CommandItems"/> и ему присвоено значение.
    /// Используется в командах обновления, когда нежелательно "случайно" создать
    /// <see cref="CommandItems"/>.
    /// Обычно удобнее использовать свойство <see cref="CommandItemsIfAssigned"/>.
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
        EFPApp.ShowException(e);
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

    /// <summary>
    /// Возвращает true, если <see cref="EFPStatusBarHandler.IsFormOwned"/>=true
    /// </summary>
    private bool StatusBarHandlerIsFormOwned
    {
      get
      {
        if (BaseProvider.FormProvider == null)
          return false;
        if (BaseProvider.FormProvider.StatusBarHandler == null)
          return false;
        return BaseProvider.FormProvider.StatusBarHandler.IsFormOwned;
      }
    }

    internal void UpdateCommandItemsActive()
    {
      bool wantedHasFocus = ProviderState == EFPControlProviderState.Attached &&
        //Control.ContainsFocus && // нельзя полагаться на это свойство из обработчика Control.Leave
        _ControlHasFocus &&
        BaseProvider.FormProvider.Active;

      //bool wantedHasStatus = wantsHasFocus;
      //if (StatusBarHandlerIsFormOwned)
      //wantedHasStatus = (ProviderState == EFPControlProviderState.Attached) && _ControlHasFocus; // 12.10.2023

      if (wantedHasFocus)
        PrepareContextMenu();
      if (_CommandItems != null)
      {
        _CommandItems.SetHasFocus(wantedHasFocus);
        //_CommandItems.SetHasStatus(wantedHasStatus);
        _CommandItems.SetHasStatus(wantedHasFocus);
      }
    }

    private bool _ControlHasFocus;

    internal bool StatusBarPanelsShouldBeDetached()
    {
      // Открыт другой блок диалога?
      Form currForm = null;
      if (BaseProvider.FormProvider != null) // 28.11.2024
        currForm = BaseProvider.FormProvider.Form;

      if (EFPApp.ActiveDialog != null && EFPApp.ActiveDialog != currForm)
      {
        // return true;

        // 28.05.2025
        // Если у блока диалога есть собственная статусная строка или она ему не нужна, 
        // можно не отсоединять текущие панели
        EFPFormProvider dlgFormProvider = EFPFormProvider.FindFormProvider(EFPApp.ActiveDialog);
        if (dlgFormProvider != null)
        {
          if (dlgFormProvider.OwnStatusBar)
            return false;
          if (!dlgFormProvider.ContainsControlWantedStatusBar())
            return false;

          return true;
        }
        else
          return false; // не должно быть
      }

      bool wantedHasStatus = (ProviderState == EFPControlProviderState.Attached) &&
          WinFormsTools.ContainsControl(this.Control, BaseProvider.FormProvider.LastFocusedControl);
      if (!StatusBarHandlerIsFormOwned)
        wantedHasStatus &= BaseProvider.FormProvider.Active;

      return !wantedHasStatus;
    }


    private void Control_Enter(object sender, EventArgs args)
    {
      _ControlHasFocus = true;

      try
      {
        //BaseProvider.FormProvider.LastFocusedControl = this.Control;

        if (Enter != null)
          Enter(this, EventArgs.Empty);

        // 21.08.2015
        // Событие Enter может возникнуть до установки Visible=true
        UpdateFormProviderState();

        PrepareContextMenu();

        BaseProvider.FormProvider.TempDefaultButton = DefaultButton;
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e);
      }

      if (ValidateWhenFocusChanged)
        _IdleValidationRequired = true;
    }

    /// <summary>
    /// Событие вызывается из обработчика события <see cref="System.Windows.Forms.Control.Enter"/> перед выполнением стандартных действий.
    /// Обработчик может, например, изменить текущее значение элемента до выполнения проверки.
    /// Если требуется присоединить обработчик, вызываемый после выполнения проверки, используйте оригинальное событие <see cref="System.Windows.Forms.Control.Enter"/>.
    /// </summary>
    public event EventHandler Enter;

    private void Control_Leave(object sender, EventArgs args)
    {
      _ControlHasFocus = false;

      if (Leave != null)
        Leave(this, EventArgs.Empty);


      if (ValidateWhenFocusChanged)
        _IdleValidationRequired = true;

      try
      {
        UpdateCommandItemsActive();

        if (BaseProvider.FormProvider != null)
          BaseProvider.FormProvider.TempDefaultButton = null;
      }
      catch { }
    }

    /// <summary>
    /// Событие вызывается из обработчика события <see cref="System.Windows.Forms.Control.Leave"/> перед выполнением стандартных действий.
    /// Обработчик может, например, изменить текущее значение элемента до выполнения проверки.
    /// Если требуется присоединить обработчик, вызываемый после выполнения проверки, используйте оригинальное событие <see cref="System.Windows.Forms.Control.Leave"/>.
    /// </summary>
    public event EventHandler Leave;

    private bool _ContextMenuWasInit;

    /// <summary>
    /// Обеспечивает готовность локального меню.
    /// После вызова метода устанавливается свойство <see cref="System.Windows.Forms.Control.ContextMenuStrip"/>, если оно не было установлено ранее.
    /// Перед присоединеием вызывается <see cref="PrepareCommandItems()"/>.
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
        PrepareCommandItems();

        _ContextMenuWasInit = true;

        // Присоединяем команды
        bool toolBarControllable = false;
        if (ToolBar != null)
          toolBarControllable = ToolBar.PanelCommandItems != null;

        List<EFPCommandItems> list = new List<EFPCommandItems>();
        if (CommandItems.Count > 0)
          list.Add(CommandItems);
        if (BaseCommandItemsNeeded) // 28.09.2018
          BaseProvider.InitCommandItemList(list);

        if (list.Count > 0 || toolBarControllable)
        {
          EFPContextMenu ccm = new EFPContextMenu();
          for (int i = 0; i < list.Count; i++)
            ccm.AddRange(list[i]);

          //if (ToolBarControllable)
          //{
          //  ccm.Menu.Items.Add(
          //  EFPDropDownMenu ddm = new EFPDropDownMenu();
          //  ddm.Add(ToolBar.PanelCommandItems);
          //  //ddm.a
          //}

          ccm.DefaultCommandItem = CommandItems.DefaultCommandItem;

          ccm.Attach(Control);
        }
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Возвращает true (по умолчанию), если в локальное меню нужно добавить команды из 
    /// родительского элемента.
    /// </summary>
    protected virtual bool BaseCommandItemsNeeded { get { return true; } }

    /// <summary>
    /// Показ локального меню управляющего элемента (как при нажатии правой кнопки мыши).
    /// Переопределяется <see cref="EFPButtonWithMenu"/>, чтобы в меню были только явно заданные команды.
    /// </summary>
    public void ShowLocalMenu()
    {
      PrepareContextMenu();

      if (Control.ContextMenuStrip != null)
        Control.ContextMenuStrip.Show(Control, Control.Width, Control.Height);
#if !NET
      else if (Control.ContextMenu != null)
        Control.ContextMenu.Show(Control, new Point(Control.Width, Control.Height));
#endif
    }

    #endregion

    #region Локальная панель инструментов

    /// <summary>
    /// Панель инструментов для локального меню.
    /// Если провайдер управляющего элемента создан без использования <see cref="IEFPControlWithToolBar"/>, но ему требуется
    /// панель инструментов, то следует установить это свойство.
    /// Свойство может устанавливаться однократно.
    /// </summary>
    public Panel ToolBarPanel
    {
      get
      {
        if (_ToolBar == null)
          return null;
        else
          return _ToolBar.Bar.Parent as Panel;
      }
      set
      {
        if (value == this.ToolBarPanel)
          return;
        if (_ToolBar != null)
          throw ExceptionFactory.RepeatedCall(this, "ToolBarPanel");

        if (!EFPApp.ShowControlToolBars)
        {
          value.Visible = false;
          return;
        }

        _ToolBar = new EFPPanelToolBar();
        _ToolBar.Attach(value);

        if (CommandItems.IsReadOnly)
        {
          ToolBar.AddRange(CommandItems);
          ToolBar.ToolBarVisible = ToolBar.Count > 0; // 10.09.2012
        }
      }
    }

    /// <summary>
    /// Провайдер панели инструментов.
    /// Создается при установке свойства <see cref="ToolBarPanel"/>.
    /// </summary>
    public EFPPanelToolBar ToolBar { get { return _ToolBar; } }
    private EFPPanelToolBar _ToolBar;

    /// <summary>
    /// Инициализация команд локального меню, панели инструментов и статусной строки.
    /// Этот метод не используется в прикладном коде.
    /// Повторные вызовы метода игнорируются.
    /// В классах-наследниках можно переопределить методы <see cref="OnBeforePrepareCommandItems()"/> или <see cref="OnAfterPrepareCommandItems()"/>,
    /// а в прикладном коде добавить обработчик события <see cref="BeforePrepareCommandItems"/>.
    /// </summary>
    private void PrepareCommandItems()
    {
      if (CommandItems.IsReadOnly)
        return;

      if (Control.IsDisposed)
        return; // 23.10.2017

      // Убрано 07.05.2022
      //if (!WinFormsTools.AreControlAndFormVisible(Control))
      //  return false;

      OnBeforePrepareCommandItems(); // 28.09.2018
      CommandItems.SetReadOnly();
      OnAfterPrepareCommandItems(); // 21.02.2023

      if (ToolBar != null)
      {
        ToolBar.AddRange(CommandItems);
        ToolBar.ToolBarVisible = ToolBar.Count > 0; // 10.09.2012
      }
    }

    /// <summary>
    /// Событие вызывается из метода <see cref="PrepareCommandItems()"/>, перед тем, как он выполнит инициализацию локального меню,
    /// панелей инструментов и статусной строки.
    /// Пользовательский обработчик может, например, выполнить окончательную настройку команд.
    /// </summary>
    public event EventHandler BeforePrepareCommandItems;

    /// <summary>
    /// Вызывает событие <see cref="BeforePrepareCommandItems"/>, если обработчик установлен
    /// </summary>
    protected virtual void OnBeforePrepareCommandItems()
    {
      if (BeforePrepareCommandItems != null)
        BeforePrepareCommandItems(this, EventArgs.Empty);
    }

    /// <summary>
    /// Вызывается из метода <see cref="PrepareCommandItems"/>() после того, как команды меню будут инициализированы.
    /// На момент вызова свойство <see cref="EFPContextCommandItems"/>.IsReadOnly уже установлено в true и изменения в списке команд меню не допускаются.
    /// Переопределяется в табличном просмотре <see cref="EFPDataGridView"/> для добавления обработчиков.
    /// </summary>
    protected virtual void OnAfterPrepareCommandItems()
    {
      // События AfterPrepareCommandItems пока нет
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

      UIValidateState prevState = _ValidateState;
      string prevMessage = _ValidateErrorMessage;
      string prevValueToolTipText = _ValueToolTipText;

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
          SetError(String.Format(Res.EFPControl_Err_Validating, e.Message));
        }
      } // Editable

      if (_ValidateState != prevState ||
        _ValidateErrorMessage != prevMessage ||
        _ValueToolTipText != prevValueToolTipText)
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
    /// Список валидаторов <see cref="UIValidator"/> элемента, основанных на управляемых значениях
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
      EFPErrorInfo info = new EFPErrorInfo(ValidateErrorMessage, ValidateState == UIValidateState.Error, Control);
      errorList.Add(info);
    }

    /// <summary>
    /// Если свойство установлено в true, то метод <see cref="Validate()"/> будет дополнительно вызываться после получения и
    /// потери элементом фокуса ввода.
    /// Вызов метода <see cref="Validate()"/> выполняется не сразу в обработчике событий <see cref="System.Windows.Forms.Control.Enter"/> и <see cref="System.Windows.Forms.Control.Leave"/>,
    /// а с небольшой задержкой, в обработчике Idle. Соответственно, свойство <see cref="UseIdle"/> принимает значение true.
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
          UseIdle = true;
      }
    }
    private bool _ValidateWhenFocusChanged;

    private bool _IdleValidationRequired;

    #endregion

    #region IEFPValidator Members

    /// <summary>
    /// Этот метод может вызываться из <see cref="OnValidate()"/> для указания наличия ошибки
    /// </summary>
    /// <param name="message"></param>
    void IUIValidableObject.SetError(string message)
    {
      SetError(message);
    }

    /// <summary>
    /// Этот метод может вызываться из <see cref="OnValidate()"/> для указания наличия предупреждения
    /// </summary>
    /// <param name="message"></param>
    void IUIValidableObject.SetWarning(string message)
    {
      SetWarning(message);
    }

    /// <summary>
    /// Этот метод может вызываться из <see cref="OnValidate()"/> для указания наличия ошибки
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    protected void SetError(string message)
    {
#if DEBUG
      if (!_InsideValidate)
        throw new InvalidOperationException(Res.EFPControl_Err_OutsideValidating);
#endif

      if (_ValidateState == UIValidateState.Error)
        return;
      _ValidateState = UIValidateState.Error;
      _ValidateErrorMessage = message;
    }

    /// <summary>
    /// Этот метод может вызываться из <see cref="OnValidate()"/> для указания наличия прежупреждения
    /// </summary>
    /// <param name="message">Предупреждение</param>
    protected void SetWarning(string message)
    {
#if DEBUG
      if (!_InsideValidate)
        throw new InvalidOperationException(Res.EFPControl_Err_OutsideValidating);
#endif

      if (_ValidateState != UIValidateState.Ok)
        return;
      _ValidateState = UIValidateState.Warning;
      _ValidateErrorMessage = message;
    }

    /// <summary>
    /// Текущее состояние проверки ошибок <see cref="OnValidate()"/>.
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    #endregion

    #region Прочие свойства, методы и события проверки ошибок

    private readonly Color _OrgControlForeColor;

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
          if (EnvironmentTools.IsMono)
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
    /// Используется свойство <see cref="System.Windows.Forms.Control.ForeColor"/>.
    /// Эта версия задает цвет <see cref="SystemColors.ControlText"/> для состояния без ошибок.
    /// </summary>
    /// <param name="label">Управляющий элемент</param>
    /// <param name="state">Цвет текста</param>
    public static void SetLabelForeColor(Control label, UIValidateState state)
    {
      SetLabelForeColor(label, state, SystemColors.ControlText);
    }

    /// <summary>
    /// Установить цвет текста метки в соответствии с состоянием проверки.
    /// Используется свойство <see cref="System.Windows.Forms.Control.ForeColor"/>.
    /// </summary>
    /// <param name="label">Управляющий элемент</param>
    /// <param name="state">Цвет текста</param>
    /// <param name="defaultForeColor">Основной цвет метки. Обычно равно <see cref="SystemColors.ControlText"/>.
    /// Если задано <see cref="Color.Transparent"/>, вызывается метод <see cref="System.Windows.Forms.Control.ResetForeColor()"/></param>
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
    /// Этот метод сам не вызывает обработчик события <see cref="Validating"/>. 
    /// Событие всегда вызывается после вызова <see cref="OnValidate()"/>.
    /// Если в переопределенном методе требуется узнать причину проверки, используйте свойство <see cref="EFPBaseProvider.ValidateReason"/>.
    /// </summary>
    protected virtual void OnValidate()
    {
      // Могут переопределяться для некоторых управляющих элементов
    }

    /// <summary>
    /// Пользовательский обработчик для проверки ошибок.
    /// Обработчик всегда вызывается после защищенного метода <see cref="OnValidate()"/>.
    /// Обработчик не может "понизить" состояние проверки, например, убрать предупреждение.
    /// Событие не вызывается, если <see cref="OnValidate()"/> установил состояние ошибки.
    /// Если в обработчике требуется узнать причину проверки, используйте свойство <see cref="EFPBaseProvider.ValidateReason"/>.
    /// </summary>
    public event UIValidatingEventHandler Validating;

    #endregion

    #region Методы, которые можно использовать в OnValidate()


    /// <summary>
    /// Вспомогательный метод для выдачи ошибки или предупреждения, в зависимости от свойства 'CanBeEmptyMode'.
    /// Используется в реализации <see cref="OnValidate()"/> классов-наследников, имеющих такое свойство, когда текущее значение пустое.
    /// </summary>
    /// <param name="canBeEmptyMode">Значение свойства 'CanBeEmptyMode'</param>
    protected void ValidateCanBeEmptyMode(UIValidateState canBeEmptyMode)
    {
      UITools.ValidateCanBeEmptyMode(canBeEmptyMode, this, DisplayName);
    }

    #endregion

    #region Активация управляющего элемента

    /// <summary>
    /// Передать фокус ввода управляющему элементу.
    /// Если управляющий элемент находится на вкладке <see cref="System.Windows.Forms.TabPage"/>, которая не активна,
    /// то она активируется.
    /// </summary>
    public void SetFocus()
    {
      SetFocus(null);
    }

    /// <summary>
    /// Передать фокус ввода управляющему элементу и выдать сообщение об ошибке
    /// Если управляющий элемент находится на вкладке <see cref="System.Windows.Forms.TabPage"/>, которая не активна,
    /// то она активируется.
    /// </summary>
    /// <param name="errorMessage">Строка сообщения</param>
    public void SetFocus(string errorMessage)
    {
      //if (HasBeenCreated)
      // Исправлено 17.02.2022

      switch (ProviderState)
      {
        case EFPControlProviderState.Attached:
          // Немедленная установка
          WinFormsTools.FocusToControl(Control);
          break;
        case EFPControlProviderState.Disposed:
          break;
        default:
          // Отложенная установка
          if (BaseProvider.FormProvider != null)
            BaseProvider.FormProvider.DelayedSetFocusControlProvider = this;
          break;
      }
      if (!String.IsNullOrEmpty(errorMessage))
        EFPApp.ShowTempMessage(errorMessage);
    }

    #endregion

    #region Свойства и методы для IDepSyncObject

    /// <summary>
    /// Этот метод реализует часть интерфейса <see cref="IDepSyncObject"/> и не должен 
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
    /// Метод обработки события <see cref="Idle"/>.
    /// Вызывается периодически, когда приложение ничем не занято.
    /// Чтобы метод вызывался, свойство <see cref="UseIdle"/> должно быть установлено в true.
    /// </summary>
    public virtual void HandleIdle()
    {
      SaveConfig();
      
      if (_Idle != null)
        _Idle(this, EventArgs.Empty);

      if (_CommandItems != null)
      {
        if (_CommandItems.HasIdle &&
          BaseProvider.FormProvider.Form.WindowState != FormWindowState.Minimized) // 27.02.2025

        _CommandItems.HandleIdle(); // 28.01.2021
      }
      
      if (_IdleValidationRequired)
      {
        _IdleValidationRequired = false;
        Validate();
      }
    }

    /// <summary>
    /// Если свойство установлено в true, то будет периодически вызываться метод <see cref="HandleIdle"/>.
    /// По умолчанию свойство не установлено.
    /// Свойство может устанавливаться производным классом только до вывода управляющего элемента на экран.
    /// Если установлен обработчик <see cref="Idle"/>, то свойство всегда возвращает true.
    /// </summary>
    public bool UseIdle
    {
      get
      {
        if (_UseIdle || _Idle != null)
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
    /// Событие вызывается для выполнения ленивой инициализации.
    /// Событие вызывается 1 раз в секунду или после обработки других сообщений.
    /// Обработчик должен присоединяться до вывода элемента на экран.
    /// </summary>
    public event EventHandler Idle
    {
      add
      {
        CheckHasNotBeenCreated();
        _Idle += value;
      }
      remove
      {
        _Idle -= value;
      }
    }
    private EventHandler _Idle;

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
    /// Используется при обращении к свойству UpdateByTimeHandlers после вызова Created
    /// </summary>
    private static readonly UpdateByTimeHandlerList _EmptyUpdateByTimeHandlers = new UpdateByTimeHandlerList(true);

    /// <summary>
    /// Сюда можно добавить обработчик для обновления данных по таймеру.
    /// Добавление обработчика возможно только до вывода формы на экран или показа элемента (до события <see cref="Created"/>)
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
    /// <see cref="EFPBaseProvider"/>. Если он не определен явно в цепочке провайдеров до <see cref="EFPFormProvider"/>,
    /// берется <see cref="EFPApp.ConfigManager"/>.
    /// Свойство не может возвращать null, т.к. в <see cref="EFPApp.ConfigManager"/> всегда есть заглушка.
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
    /// Реализуется через <see cref="ConfigHandler"/>.
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
    /// Повторные вызовы игнорируются.
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
    /// Обработчик конфигурации. Равен null до вызова <see cref="InitConfigHandler()"/>.
    /// Чтобы зарегистрировать категорию, для которой будут записываться данные секции конфигурации, 
    /// конструктор производного класса вызывает <see cref="ConfigHandler"/>.Categories.Add("Категория"). Методы коллекции могут вызываться только до события <see cref="Created"/>.
    /// Если нужно записать конфигурацию, то должен быть установлен флаг для категории вызовом <see cref="ConfigHandler"/>.Changed["Категория"]=true.
    /// </summary>
    public EFPConfigHandler ConfigHandler { get { return _ConfigHandler; } }
    private EFPConfigHandler _ConfigHandler;

    /// <summary>
    /// Первоначальное чтение конфигурации элемента.
    /// Вызывает <see cref="EFPConfigHandler.ReadConfig(IEFPConfigManager)"/> для чтения значений.
    /// Ничего не делает, если <see cref="ConfigHandler"/> еще не инициализирован или свойство <see cref="ConfigSectionName"/> не установлено.
    /// Повторные вызовы метода игнорируются.
    /// Как правило, этот метод вызывается из <see cref="OnCreated"/>, но может быть вызван досрочно внешним кодом.
    /// Не вызывайте этот метод из конструктора производного класса, так как конструктор класса-наследника
    /// (если он есть) может вызвать ошибку. К тому же, внешний код может, например, изменить Свойство <see cref="ConfigManager"/>.
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
    /// Метод возвращает true, если <see cref="LoadConfig()"/> был вызван.
    /// Свойство действительно, дажн если свойство <see cref="ConfigManager"/>=null.
    /// </summary>
    public bool LoadConfigCalled { get { return _LoadConfigCalled; } }
    private bool _LoadConfigCalled;

    /// <summary>
    /// Генерирует исключение, если <see cref="LoadConfig()"/> был вызван
    /// </summary>
    public void CheckLoadConfigHasNotBeenCalled()
    {
      if (LoadConfigCalled)
        throw new InvalidOperationException(String.Format(Res.EFPControl_Err_LoadConfigCalled, this.ToString()));
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента.
    /// Непереопределенный метод вызывает <see cref="EFPConfigHandler.ReadConfig(IEFPConfigManager)"/>.
    /// Если свойство <see cref="ConfigHandler"/>=null, метод не вызывается.
    /// Следовательно, управляющий элемент, в котором определен данный класс, должен в конструкторе вызывать <see cref="InitConfigHandler()"/>.
    /// Метод вызывается, даже если свойство <see cref="ConfigSectionName "/>не установлено.
    /// Обычно метод косвенно вызывается из <see cref="OnCreated"/>, но может быть вызван и раньше.
    /// </summary>
    protected virtual void OnLoadConfig()
    {
      if (!EFPApp.InsideLoadComposition)
        ConfigHandler.ReadConfig(this.ConfigManager);
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента перед вызовом <see cref="LoadConfig()"/>.
    /// Метод вызывается независимо от установки свойства <see cref="ConfigHandler"/>.
    /// Переопределенный метод может, например, сохранить значения свойств, установленных на момент загрузки конфигурации.
    /// </summary>
    protected virtual void OnBeforeLoadConfig()
    {
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента после вызова <see cref="LoadConfig()"/>.
    /// Метод вызывается независимо от установки свойства <see cref="ConfigHandler"/>.
    /// </summary>
    protected virtual void OnAfterLoadConfig()
    {
    }


    /// <summary>
    /// Сохраняет все несохраненные изменения для элемента.
    /// Этот метод вызывается по таймеру и при закрытии формы.
    /// Если <see cref="ConfigHandler"/>=null, метод ничего не делает.
    /// </summary>
    public void SaveConfig()
    {
      if (ConfigHandler != null)
        OnSaveConfig();
    }

    /// <summary>
    /// Этот метод сохраняет все несохраненные настройки для элемента.
    /// Вызывается, только если <see cref="ConfigHandler"/>!=null.
    /// Непереопределенный метод вызывает <see cref="EFPConfigHandler.WriteConfigChanges(IEFPConfigManager)"/>.
    /// Переопреденный метод может сначала установить признаки <see cref="EFPConfigHandler.Changed"/>, чтобы учесть изменения, которые нужно сохранить,
    /// например, текущую позицию в просмотре.
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

      Control control2 = control;
      do
      {
        if (control2 is Form)
        {
          EFPFormProvider formProvider = EFPFormProvider.FindFormProvider((Form)control2);
          if (formProvider != null)
          {
            EFPControlBase res = formProvider.FindControlProvider(control); // не Control2!
            if (res != null)
              return res;
          }
        }

        // спускаемся ниже
        control2 = control2.Parent;
      }
      while (control2 != null);

      return null;
    }

    private static readonly EFPControlBase[] _EmptyArray = new EFPControlBase[0];

    /// <summary>
    /// Возвращает список провайдеров управляющих элементов, дочерних по отношению к текущему провайдеру.
    /// Для этого перебираются все элементы <see cref="EFPControlBase"/> в <see cref="EFPBaseProvider"/>, включая рекурсивно вложенные <see cref="EFPBaseProvider"/>.
    /// Если элемент <see cref="EFPControlBase.Control"/> является вложенным (метод <see cref="WinFormsTools.ContainsControl(Control, Control)"/>), то он добавляется в список.
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
    /// <param name="category">Категория считываемой секции</param>
    /// <param name="cfg">Объект для чтения значений</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public virtual void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    #endregion

    #region Отладка

    /// <summary>
    /// Добавляет в список <see cref="System.Exception.Data"/> информацию об управляющем элементе.
    /// Вызывается в блоке catch при перехвате исключения.
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
  /// элементов. Реализует типизированное свойство <see cref="Control"/>.
  /// </summary>
  /// <typeparam name="T">Класс управляющего элемента, производного от <see cref="System.Windows.Forms.Control"/></typeparam>
  public class EFPControl<T> : EFPControlBase, IEFPControl
    where T : Control
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, <typeparamref name="T"/>=<see cref="TextBox"/>, обычно нужна метка.
    /// Если false, то метка не нужна (например, <see cref="CheckBox"/>)</param>
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
    /// Создает провайдер для управляющего элемента с использованием <see cref="IEFPControlWithToolBar{T}"/>.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    /// <param name="labelNeeded">Если true, то элементу, например, <typeparamref name="T"/>=<see cref="TextBox"/>, обычно нужна метка.
    /// Если false, то метка не нужна (например, <see cref="CheckBox"/>)</param>
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
    /// Свойство <see cref="System.Windows.Forms.Control.Visible"/>
    /// </summary>
    protected override bool ControlVisible
    {
      get { return Control.Visible; }
      set { Control.Visible = value; }
    }

    /// <summary>
    /// Свойство <see cref="System.Windows.Forms.Control.Enabled"/>
    /// </summary>
    protected override bool ControlEnabled
    {
      get { return Control.Enabled; }
      set { Control.Enabled = value; }
    }

    #endregion
  }

  /// <summary>
  /// Расширение класса <see cref="EFPControl{T}"/> для реализации методов интерфейса <see cref="IDepSyncObject"/>
  /// </summary>
  /// <typeparam name="T">Класс управляющего элемента <see cref="System.Windows.Forms.Control"/></typeparam>
  public abstract class EFPSyncControl<T> : EFPControl<T>, IDepSyncObject
    where T : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="labelNeeded">Если true, то элементу, например, <typeparamref name="T"/>=<see cref="TextBox"/>, обычно нужна метка.
    /// Если false, то метка не нужна (например, <see cref="CheckBox"/>)</param>
    public EFPSyncControl(EFPBaseProvider baseProvider, T control, bool labelNeeded)
      : base(baseProvider, control, labelNeeded)
    {
      _SyncGroup = null;
      _SyncMaster = false;
    }

    /// <summary>
    /// Создает провайдер для управляющего элемента с использованием <see cref="IEFPControlWithToolBar{T}"/>.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    /// <param name="labelNeeded">Если true, то элементу, например, <typeparamref name="T"/>=<see cref="TextBox"/>, обычно нужна метка.
    /// Если false, то метка не нужна (например, <see cref="CheckBox"/>)</param>
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
    /// Это свойство реализует часть интерфейса <see cref="IDepSyncObject"/> и не должно 
    /// устанавливаться из прикладной программы.
    /// </summary>
    public DepSyncGroup SyncGroup
    {
      get { return _SyncGroup; }
      set { _SyncGroup = value; }
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
    /// значения в группу синхронизации в режиме <see cref="SyncMaster"/>=true.
    /// </summary>
    protected void OnSyncValueChanged()
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
