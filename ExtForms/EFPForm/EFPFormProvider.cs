// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using FreeLibSet.DependedValues;
using System.Drawing;
using FreeLibSet.Config;
using FreeLibSet.Logging;
using System.Runtime.CompilerServices;
using FreeLibSet.Collections;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  #region Перечисление EFPFormValidateReason

  /// <summary>
  /// Причина проверки формы (свойство EFPFormProvider.ValidateReason)
  /// </summary>
  public enum EFPFormValidateReason
  {
    /// <summary>
    /// Проверка сейчас не выполняется, вызвана при изменении состоянии управляющих элементов,
    /// или программный вызов метода ValidateForm()
    /// </summary>
    Unknown,

    /// <summary>
    /// Начальная проверка при выводе формы на экран
    /// </summary>
    Shown,

    /// <summary>
    /// Проверка перед закрытием формы
    /// </summary>
    Closing,

    /// <summary>
    /// Проверка при вызове метода EFPFormProvider.ValidateForm().
    /// Обычно должен обрабатываться так же, как и вариант Closing.
    /// </summary>
    ValidateForm,
  }

  #endregion

  /// <summary>
  /// Расширение для работы с формой (класс Form) в EFPApp.
  /// Этот объект должен создается в конструкторе форме.
  /// </summary>
  public class EFPFormProvider : EFPBaseProvider, IEFPAppTimeHandler, IEFPAppIdleHandler, IEFPStatusBarControl, IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для формы
    /// </summary>
    /// <param name="form">Форма Windows Forms, еще не выведенная на экран</param>
    public EFPFormProvider(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");
#endif

      _Form = form;

      // Порядок вызова событий в Form при показе блока диалога:
      // - HandleCreated
      // - Load
      // - VisibleChanged
      // - Shown

      // Так нельзя, т.к. дочерние элементы еще не полностью созданы
      //_Form.HandleCreated += new EventHandler(Form_HandleCreated);

      // Бесполезно. Все равно мерцает 
      _Form.Load += new EventHandler(Form_Load);

      _Form.FormClosing += new FormClosingEventHandler(Form_FormClosing);
      _Form.Closing += new CancelEventHandler(Form_Closing);
      _Form.FormClosed += new FormClosedEventHandler(Form_FormClosed);
      _Form.VisibleChanged += new EventHandler(Form_VisibleChanged);
      _Form.Activated += new EventHandler(Form_Activated);
      _Form.Deactivate += new EventHandler(Form_Deactivate);
      _Form.SizeChanged += new EventHandler(Form_SizeChanged);

      _SyncProviderDefined = false;

      if (EFPApp.ShowToolTips)
      {
        _ToolTipIcons = new Dictionary<Control, ToolTipIcon>();
        _ToolTipTitles = new Dictionary<Control, string>();
        _ToolTipNestedControls = new Dictionary<Control, Control>();

        _TheToolTip = new ToolTip();
        _TheToolTip.ToolTipIcon = ToolTipIcon.None;
        _TheToolTip.Popup += new PopupEventHandler(TheToolTip_Popup);
        _TheToolTip.ShowAlways = true;
        _TheToolTip.AutoPopDelay = 10000;
        //TheToolTip.InitialDelay = 50;
        //TheToolTip.ReshowDelay = 100;
      }
      _Form.KeyPreview = true;
      // так не работает. _Form.PreviewKeyDown += new PreviewKeyDownEventHandler(Form_PreviewKeyDown); // 04.02.2021
      _Form.KeyDown += new KeyEventHandler(Form_KeyDown);

      _Form.Disposed += new EventHandler(Form_Disposed);

      ToolFormsEnabled = true;

      if (EFPApp.AppWasInit)
        InitStdButtonTextAndImage(form);

#if DEBUG
      DebugFormDispose.Add(_Form);

      _DebugCreateTime = DateTime.Now;
#endif

      _ValidateReason = EFPFormValidateReason.Unknown;

      _CloseModalFormByEscape = true;
      _ReentranceLocker = new EFPReentranceLocker();

      _ProviderList.Add(this);

      _ConfigHandler = new EFPConfigHandler();
      _ConfigHandler.Sources.Add(this);
      // DesignAutoScaleDimensions = Form.AutoScaleBaseSize; 

      _DefaultScreen = EFPApp.DefaultScreen;
      _DialogOwnerForm = EFPApp.DialogOwnerWindow as Form;

      _ReadConfigFormBoundsParts = EFPFormBoundsPart.None;

      if (_Form.WindowState == FormWindowState.Minimized)
        _WindowStateBeforeMinimized = FormWindowState.Normal;
      else
        _WindowStateBeforeMinimized = _Form.WindowState;
    }

    void Form_Load(object sender, EventArgs args)
    {
      try
      {
        DoForm_VisibleChanged(true);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Дублирует свойство <see cref="System.Windows.Forms.Form.Modal"/> с возможностью упреждающей установки
    /// </summary>
    internal bool Modal
    {
      get { return _Modal ?? Form.Modal; }
    }
    private bool? _Modal;

    internal void InternalSetVisible(bool modal)
    {
      _Modal = modal;
      try
      {
        DoForm_VisibleChanged(true);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
      _Modal = null;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Форма (задается в конструкторе)
    /// </summary>
    public Form Form { get { return _Form; } }
    private readonly Form _Form;

    /// <summary>
    /// Провайдер синхронизации значений. К нему будет подключен объект Syncs
    /// на время запуска формы
    /// Значение по-умолчанию можно переопределить для использования чужого
    /// </summary>
    [Browsable(true)]
    public DepSyncProvider SyncProvider
    {
      get
      {
        if (!_SyncProviderDefined)
        {
          _SyncProviderDefined = true;
          _SyncProvider = new DepSyncProvider();
        }
        return _SyncProvider;
      }
      set
      {
        _SyncProviderDefined = true;
        _SyncProvider = value;
      }
    }
    private DepSyncProvider _SyncProvider;
    private bool _SyncProviderDefined;

    /// <summary>
    /// Список для добавления объектов синхронизации.
    /// Объекты будут подключены к SyncProvider на время запуска формы
    /// </summary>
    [Browsable(true)]
    public DepSyncCollection Syncs
    {
      get
      {
        if (_Syncs == null)
          _Syncs = new DepSyncCollection();
        return _Syncs;
      }
    }
    private DepSyncCollection _Syncs;

    /// <summary>
    /// Возвращает true, если список объектов синхронизации пустой.
    /// Не требует создания дополнительго объекта
    /// </summary>
    public bool HasSyncs
    {
      get
      {
        if (_Syncs == null)
          return false;
        else
          return _Syncs.Count > 0;
      }
    }

    /// <summary>
    /// Возвращает указатель на сам текущий объект
    /// </summary>
    public override EFPFormProvider FormProvider
    {
      get
      {
        return this;
      }
    }

    /// <summary>
    /// Показывать ли открытые панели инструментов при выводе блока диалога
    /// (по умолчанию - true). Действительно только при запуске формы в модальном
    /// режиме (с помощью EFPApp.ShowDialog())
    /// Свойство может быть выключено в false для окон предварительного просмотра
    /// и других, где загромождение экрана не желательно
    /// </summary>
    public bool ToolFormsEnabled { get { return _ToolFormsEnabled; } set { _ToolFormsEnabled = value; } }
    private bool _ToolFormsEnabled;

    /// <summary>
    /// Сюда могут быть добавлены команды локального меню для формы в-целом
    /// Если задано свойство HelpContext, то будет добавлена команда меню "Справка"
    /// </summary>
    public EFPFormCommandItems CommandItems
    {
      get
      {
        if (_CommandItems == null)
          _CommandItems = GetCommandItems();
        return _CommandItems;
      }
      set
      {
        _CommandItems = value;
      }
    }
    private EFPFormCommandItems _CommandItems;

    /// <summary>
    /// Команды локального меню для формы в-целом
    /// </summary>
    /// <returns></returns>
    protected virtual EFPFormCommandItems GetCommandItems()
    {
      return new EFPFormCommandItems(this);
    }

    /// <summary>
    /// Добавляет команды для формы в-целом в список <paramref name="list"/>.
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public override void InitCommandItemList(List<EFPCommandItems> list)
    {
      if (CommandItems.Count > 0)
        list.Add(CommandItems);
      //base.InitCommandItemList(List);
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = "Form " + Form.ToString();
#if DEBUG
      s += ", created: " + _DebugCreateTime.ToString("G");
#endif
      return s;
    }

    /// <summary>
    /// Возвращает ссылку на объект EFPReentranceLocker, относящийся к данной форме.
    /// Объект создается в конструкторе EFPFormProvider.
    /// </summary>
    public override IEFPReentranceLocker ReentranceLocker
    {
      get { return _ReentranceLocker; }
    }
    private EFPReentranceLocker _ReentranceLocker;

    /// <summary>
    /// Используется в EFPAppInterface
    /// </summary>
    internal bool Prepared;

    /// <summary>
    /// Используется реализациями EFPAppInterface для передачи данных от OnPrepareChildForm() к OnShowChildForm()
    /// </summary>
    internal object InternalPreparationData;

    #endregion

    #region VisibleChanged

    private void Form_VisibleChanged(object sender, EventArgs args)
    {
      try
      {
        DoForm_VisibleChanged(_Form.Visible);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// "Правильное" состояние видимости формы.
    /// Исходно имеет значение false.
    /// Выполняется упреждающая установка из EFPApp.ShowDialog().
    /// При закрытии формы методом Form.Close() также переводится в false.
    /// </summary>
    public bool Visible { get { return _Visible; } }
    private bool _Visible;

    /// <summary>
    /// В отличие от Visible, свойство возвращает true только после полного окончания установки видимости
    /// формы, когда отработается событие Applicaction.Idle
    /// </summary>
    internal bool VisibleCompleted { get { return _VisibleCompleted; } }
    private bool _VisibleCompleted;

    private void DoForm_VisibleChanged(bool isVisible)
    {
      if (isVisible == _Visible)
        return;

      _Visible = isVisible;
      _VisibleCompleted = false;

      // Начальная проверка ошибок для всех элементов
      if (isVisible)
        OnSetFormVisible();
      else
        OnSetFormHidden();

      // Подключение или отключение списка синхронизации к провайдеру
      if (HasSyncs)
      {
        if (SyncProvider != null)
        {
          if (isVisible)
            Syncs.Provider = SyncProvider;
          else
            Syncs.Provider = null;
        }
      }
      if (_FormHelpButtonClickedHandler != null)
      {
        // Присоединение обработчика кнопки "?" в заголовке 
        if (isVisible)
          _Form.HelpButtonClicked += _FormHelpButtonClickedHandler;
        else
          _Form.HelpButtonClicked -= _FormHelpButtonClickedHandler;
      }

      base.FormVisibleChanged();
    }

    private void OnSetFormVisible()
    {
      if (_TheToolTip != null)
        _TheToolTip.Active = true;

      _ValidateReason = EFPFormValidateReason.Shown;
      try
      {
        Validate();
      }
      finally
      {
        _ValidateReason = EFPFormValidateReason.Unknown;
      }

      if (!HasBeenShown) // 24.09.2018
      {
        if (_Form.WindowState == FormWindowState.Normal)
        {
          // 06.11.2009
          // Изменение размеров формы помогает форме красиво выглядеть на экране с нестандартным DPI
          // (когда управляющие элементы выходят за края, пока не изменишь размер формы)
          // PerformLayout() почему-то не помогает
          // Наверное, есть какой-нибудь способ получше
          _Form.SetBounds(_Form.Left, _Form.Top, _Form.Width, _Form.Height - 1);
          _Form.SetBounds(_Form.Left, _Form.Top, _Form.Width, _Form.Height + 1);
        }

        if (!CommandItems.IsReadOnly)
        {
          CommandItems.SetReadOnly();

          if (CommandItems.Count > 0)
          {
            EFPContextMenu ccm = new EFPContextMenu();
            ccm.Add(CommandItems);
            ccm.Attach(_Form);
          }
        }

        //bool AdjustFormSize = (FBounds == null); // нужно ли увеличивать размер формы при добавлении статусной строки?

        try
        {
          _InsideOnSetFormVisible_StatusBarNeeded = true; // флажок для вызова EFPCommandItems.SetReadOnly()
          try
          {
            if (!_OwnStatusBar.HasValue)
              _OwnStatusBar = GetDefaultOwnStatusBar(); // фиксация значения свойства
            if (OwnStatusBar && _StatusStripControl == null)
              InitOwnStatusBar(/*AdjustFormSize*/ true); // должно быть до вызова Form.Activate(), иначе первый элемент не инициализирует статусную строку
          }
          finally
          {
            _InsideOnSetFormVisible_StatusBarNeeded = false;
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }

      try
      {
        // 12.10.2023. Вынесено из условия HasBeenShown.
        // Не работало, если форма показывается повторно, например, если форма сворачивается в tray.
        if (_StatusStripControl != null && _StatusBarHandler == null)
          _StatusBarHandler = new EFPStatusBarHandler(_StatusStripControl, true);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }

      if (!HasBeenShown) // 24.09.2018
      {
        OnShown();

        if (!Modal)
          InitButtonHandlers(Form); // 08.05.2022

        _HasBeenShown = true;
      } // !HasBeenShown

      bool oldActive = _ActiveInternal;

      // Подключение форм инструментов
      if (Modal && ToolFormsEnabled)
      {
        try
        {
          foreach (Form toolForm in EFPApp.ToolFormsForDialogs)
          {
            if (toolForm.IsDisposed)
              continue;
            toolForm.Owner = _Form;
            toolForm.Visible = true;
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Включение tool forms"); // без вывода диалога
        }
        Form.Activate(); // добавлено 17.12.2010 для работы формы предварительного просмотра
      }

      // 21.05.2021 Перенесено из OnShown(), т.к. должно вызываться и при повторном вызове формы
      if (_UpdateByTimeHandlers != null)
      {
        EFPApp.Timers.Add(this);
      }
      EFPApp.IdleHandlers.Add(this);

      DelayedSetFocus(); // Этот вызов не является лишним. Предпочтительнее установить фокус до прорисовки формы, чем в обработчике Idle

      if (EFPApp.SaveFormBounds)
      {
        _StartBounds = new EFPFormBounds();
        _StartBounds.FromControl(Form);
      }

      _FormActivatedSetFocusFlag = true;

      if (oldActive)
        OnSetFormActive();
    }

    private void OnSetFormHidden()
    {
      EFPApp.Timers.Remove(this);
      EFPApp.IdleHandlers.Remove(this);
      if (_ToolTipDelayTimer != null)
      {
        _ToolTipDelayTimer.Dispose();
        _ToolTipDelayTimer = null;
      }

      if (_ActiveInternal)
        OnSetFormInactive();

      if (_TheToolTip != null)
      {
        // 06.07.2021
        // Регулярно появляется неперехватываемое исключение ObjectDisposedException.
        // Stack trace:
        // в System.Windows.Forms.Control.CreateHandle()
        // в System.Windows.Forms.Control.get_Handle()
        // в System.Windows.Forms.Control.GetSafeHandle(IWin32Window window)
        // в System.Windows.Forms.ToolTip.Hide(IWin32Window win)
        // в System.Windows.Forms.Timer.OnTick(EventArgs e)
        // в System.Windows.Forms.Timer.TimerNativeWindow.WndProc(Message& m)
        // в System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
        // Попытаемся очищать список сообщений до вызова Control.Disposed().

        _TheToolTip.Active = false;
        _TheToolTip.RemoveAll();
      }

      // Отключение форм инструментов
      if (Modal && ToolFormsEnabled)
      {
        try
        {
          foreach (Form toolForm in EFPApp.ToolFormsForDialogs)
          {
            if (toolForm.IsDisposed)
              continue;
            toolForm.Visible = false;
            toolForm.Owner = null;
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e);
        }
      }

      if (HasBeenShown)
      {
        if (_StartBounds != null)
        {
          EFPFormBounds currBounds = new EFPFormBounds();
          currBounds.FromControl(Form);
          if (!EFPFormBounds.Equals(currBounds, _StartBounds, EFPFormBounds.GetParts(Form)))
            ConfigHandler.Changed[EFPConfigCategories.FormBounds] = true;
        }

        OnHidden();
      }
    }

    private bool _FormActivatedSetFocusFlag; // 17.02.2022

    /// <summary>
    /// Для предотвращения повторного вывода сообщений в log-файл
    /// </summary>
    private bool _SaveFormConfigExceptionFlag;

    [MethodImpl(MethodImplOptions.NoInlining)] // для удобства поиска ошибок
    private void SaveFormConfig()
    {
      try
      {
        try
        {
          ConfigHandler.WriteConfigChanges(this.ConfigManager);
        }
        catch (Exception e)
        {
          if (!_SaveFormConfigExceptionFlag)
          {
            _SaveFormConfigExceptionFlag = true;
            LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("EFPFormProvider.ConfigHandler.WriteConfigChanges()"));
          }
        }
        foreach (EFPControlBase controlProvider in GetAllControlProviders())
        {
          try
          {
            if (controlProvider.ProviderState == EFPControlProviderState.Attached) // 20.09.2021
              controlProvider.SaveConfig();
          }
          catch (Exception e)
          {
            if (!_SaveFormConfigExceptionFlag)
            {
              _SaveFormConfigExceptionFlag = true;
              LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("EFPControlProvider.SaveConfig()"));
            }
          }
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e);
      }
    }


    #endregion

    #region Событие Shown

    /// <summary>
    /// Свойство возвращает true, если форма был выведена на экран.
    /// Свойство однократно переходит из false в true. Перед этим вызывается событие Shown
    /// Свойство <see cref="System.Windows.Forms.Control.Visible"/> может многократно изменяться еще до вывода формы на экран.
    /// </summary>
    public bool HasBeenShown { get { return _HasBeenShown; } }
    private bool _HasBeenShown;

    /// <summary>
    /// Событие вызывается при первом появлении формы на экране.
    /// </summary>
    public event EventHandler Shown;

    /// <summary>
    /// Метод вызывается при первом появлении формы на экране.
    /// Вызывает событие <see cref="Shown"/> и подготавливает команды локального меню.
    /// Переопределенный метод обязательно должен вызывать базовый метод.
    /// На момент вызова свойство <see cref="HasBeenShown"/> еще не установлено.
    /// </summary>
    protected virtual void OnShown()
    {
      FirstReadConfig(); // перенесено в начало метода 04.10.2018

      if (Shown != null)
        Shown(this, EventArgs.Empty);

      InitControl(Form); // перенесено из конструктора 24.09.2018

      if (CloseModalFormByEscape && Modal)
        FreeLibSet.Controls.FormButtonStub.AssignCancel(Form);
    }

    /// <summary>
    /// Если свойство <see cref="HasBeenShown"/> установлено в true, генерирует исключение <see cref="InvalidOperationException"/>
    /// </summary>
    public void CheckHasNotBeenShown()
    {
      if (HasBeenShown)
        throw new InvalidOperationException(String.Format(Res.EFPFormProvider_Err_HasBeenShown, _Form.ToString()));
    }

    #endregion

    #region Событие Hidden

    /// <summary>
    /// Событие вызывается при закрытии формы условии, что событие <see cref="Shown"/> было вызвано
    /// </summary>
    public event EventHandler Hidden;

    /// <summary>
    /// Вызывается при скрытии формы с экрана
    /// </summary>
    protected virtual void OnHidden()
    {
      if (Hidden != null)
        Hidden(this, EventArgs.Empty);

      if (_StatusBarHandler != null)
      {
        _StatusBarHandler.Dispose();
        _StatusBarHandler = null;
      }

      //ConfigHandler.WriteConfigChanges(this.ConfigManager);
      SaveFormConfig();
    }

    #endregion

    #region Свойство IsActive

    void Form_Activated(object sender, EventArgs args)
    {
      try
      {
        // Перенесено сюда 21.05.2021
        CallUpdateByTime(); // сразу выполняем обновление

        _ActiveInternal = true;
        if (Visible)
          OnSetFormActive();
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e);
      }
    }

    void Form_Deactivate(object sender, EventArgs args)
    {
      // Иногда это событие вызывается после закрытия формы
      if (!Form.Visible)
        return;
      if (Form.IsDisposed)
        return;

      try
      {
        // Записываем положение и размеры формы, параметры для всех управляющих элементов
        SaveFormConfig();

        _ActiveInternal = false;
        OnSetFormInactive();
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e);
      }
    }

    /// <summary>
    /// Свойство возвращает true, если форма видима и активна
    /// </summary>
    public bool Active
    {
      get { return _Visible && _ActiveInternal; }
    }
    private bool _ActiveInternal;


    private void OnSetFormActive()
    {
      if (!CommandItems.IsReadOnly)
        CommandItems.SetReadOnly();
      CommandItems.SetActive(true);
      UpdateCommandItemsActive(); // 02.05.2022
    }

    private void OnSetFormInactive()
    {
      CommandItems.SetActive(false);
      UpdateCommandItemsActive(); // 02.05.2022
    }

    private void UpdateCommandItemsActive()
    {
      EFPControlBase[] a = base.GetAllControlProviders();
      for (int i = 0; i < a.Length; i++)
        a[i].UpdateCommandItemsActive();
    }

    #endregion

    #region Проверка ошибок в форме

    /// <summary>
    /// True, если нужно использовать объект <see cref="ErrorProvider"/> для всех форм.
    /// По умолчанию - false - выполняется только раскраска элементов с ошибками/предупреждениями, без использования
    /// стандартного <see cref="ErrorProvider"/>.
    /// </summary>
    public static bool UseErrorProvider { get { return _UseErrorProvider; } set { _UseErrorProvider = value; } }
    private static bool _UseErrorProvider = false; // 07.05.2022

    /// <summary>
    /// Текущая причина вызова метода <see cref="ValidateForm(EFPFormValidateReason)"/>.
    /// Свойство может использоваться в обработчике события <see cref="EFPControlBase.Validating"/>, если требуется различная реакция
    /// при проверке формы.
    /// Например, может потребоваться пропускать проверку при открытии формы.
    /// </summary>
    public new EFPFormValidateReason ValidateReason { get { return _ValidateReason; } }
    private EFPFormValidateReason _ValidateReason;

    /// <summary>
    /// Выполняем проверку ошибок. Если есть ошибки, выдаем сообщение о первой из
    /// них и позиционируемся на управляющий элемент с ошибкой.
    /// Метод может вызываться, например, при обработке нажатия кнопки, при которой
    /// нужны корректные введенные значения.
    /// На время работы метода свойство <see cref="ValidateReason"/> принимает значение <see cref="EFPFormValidateReason.ValidateForm"/>.
    /// </summary>
    /// <returns>True, если нет ошибок (возможно, есть предупреждение).
    /// False, если есть ошибка</returns>
    public bool ValidateForm()
    {
      return ValidateForm(EFPFormValidateReason.ValidateForm);
    }

    private bool ValidateForm(EFPFormValidateReason reason)
    {
      bool res;

      _ValidateReason = reason;
      try
      {
        res = DoValidateForm();
      }
      finally
      {
        _ValidateReason = EFPFormValidateReason.Unknown;
      }

      return res;
    }

    private bool DoValidateForm()
    {
      EFPApp.ShowTempMessage(null);
      base.Validate();
      if (UseErrorProvider)
        InitErrorProvider();

      if (ErrorCount > 0)
      {
        EFPErrorInfo info = GetFirstError();
        if (info == null)
          throw new BugException("EFPErrorInfo has been lost");
        WinFormsTools.FocusToControl(info.FocusedControl);
        EFPApp.ShowTempMessage(info.Message);
        return false;
      }
      return true;

    }

    /// <summary>
    /// Инициализация объекта ErrorProvider
    /// </summary>
    private void InitErrorProvider()
    {
      if (_TheErrorProvider == null)
        _TheErrorProvider = new ErrorProvider(Form);
      _TheErrorProvider.Clear();

      List<EFPErrorInfo> errorList = new List<EFPErrorInfo>();
      GetErrorMessages(errorList);

      for (int i = 0; i < errorList.Count; i++)
      {
        if (errorList[i].FocusedControl != null)
        {
          _TheErrorProvider.SetError(errorList[i].FocusedControl, errorList[i].Message);
        }
      }
    }

    private ErrorProvider _TheErrorProvider;

    /// <summary>
    /// Поиск первого сообщения об ошибке
    /// </summary>
    /// <returns>Описание сообщения с <see cref="EFPErrorInfo.IsError"/>=true</returns>
    public EFPErrorInfo GetFirstError()
    {
      List<EFPErrorInfo> errorList = new List<EFPErrorInfo>();
      GetErrorMessages(errorList);
      for (int i = 0; i < errorList.Count; i++)
      {
        if (errorList[i].IsError)
          return errorList[i];
      }
      return null;
    }

    #endregion

    #region Всплывающие подсказки ToolTip для элементов

    /// <summary>
    /// Если флаг установлен, то в локальное меню формы добавляется команда
    /// отладки объекта <see cref="EFPFormProvider"/>
    /// </summary>
    public static bool DebugFormProvider { get { return _DebugFormProvider; } set { _DebugFormProvider = value; } }
    private static bool _DebugFormProvider = false;

    /// <summary>
    /// Создание команды в главном меню, которая будет переключать наличие подсказок.
    /// Команда переключает свойство <see cref="EFPApp.ShowToolTips"/>.
    /// Команда создается, но не подключается к меню.
    /// </summary>
    /// <param name="parentMenu"></param>
    /// <returns>Объект команды</returns>
    public static EFPCommandItem CreateToolTipsVisibleCommandItem(EFPCommandItem parentMenu)
    {
      EFPCommandItem ci = new EFPCommandItem("View", "ShowToolTips");
      ci.MenuText = Res.Cmd_Menu_View_ShowToolTips;
      ci.Parent = parentMenu;
      ci.Checked = EFPApp.ShowToolTips;
      ci.Click += new EventHandler(ciShowToolTips_Click);
      return ci;
    }

    static void ciShowToolTips_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPApp.ShowToolTips = !EFPApp.ShowToolTips;
      ci.Checked = EFPApp.ShowToolTips;
    }

    private ToolTip _TheToolTip;

    private Dictionary<Control, ToolTipIcon> _ToolTipIcons;
    private Dictionary<Control, string> _ToolTipTitles;

    /// <summary>
    /// Инициализация всплывающей подсказки для управляющего элемента
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="title">Заголовок подсказки (<see cref="EFPControlBase.DisplayName"/>)</param>
    /// <param name="mainInfo">Основной текст всплывающей подсказки (<see cref="EFPControlBase.ToolTipText"/>)</param>
    /// <param name="valueInfo">Информация о текущем введенном значении (обычно, пустая строка)</param>
    /// <param name="state">Наличие ошибки или предупреждения</param>
    /// <param name="errorMessage">Текст ошибки или предупреждения</param>
    internal override void SetToolTip(Control control, string title, string mainInfo, string valueInfo, UIValidateState state, string errorMessage)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      // Проверяем объект TheToolTip, а не флаг ShowToolTips, т.к. флаг может
      // быть переключен в процессе вывода формы, а объект не создается в конструкторе
      // при выключенном ShowToolTips
      if (_TheToolTip == null)
        // Вывод подсказок запрещен
        return;

      if (state == UIValidateState.Ok)
        errorMessage = String.Empty;

      StringBuilder sb = new StringBuilder();

      if (!String.IsNullOrEmpty(errorMessage))
        sb.Append(errorMessage);
      if (!String.IsNullOrEmpty(mainInfo))
      {
        if (sb.Length > 0)
        {
          sb.Append(Environment.NewLine);
          sb.Append(Environment.NewLine);
        }
        sb.Append(mainInfo);
      }
      if (!String.IsNullOrEmpty(valueInfo))
      {
        if (sb.Length > 0)
        {
          sb.Append(Environment.NewLine);
          sb.Append(Environment.NewLine);
        }
        sb.Append(valueInfo);
      }


      ToolTipIcon icon;
      switch (state)
      {
        case UIValidateState.Error:
          icon = ToolTipIcon.Error;
          title += " - " + UITools.ToString(ErrorMessageKind.Error);
          break;
        case UIValidateState.Warning:
          icon = ToolTipIcon.Warning;
          title += " - " + UITools.ToString(ErrorMessageKind.Warning);
          break;
        default:
          icon = ToolTipIcon.Info;
          break;
      }
      // Рекурсия убрана 11.06.2021

      _TheToolTip.SetToolTip(control, sb.ToString());
      _ToolTipIcons[control] = icon;
      _ToolTipTitles[control] = title;
    }

    void TheToolTip_Popup(object sender, PopupEventArgs args)
    {
      if (args.AssociatedControl.Enabled)
      {
        _TheToolTip.ToolTipIcon = _ToolTipIcons[args.AssociatedControl];
        _TheToolTip.ToolTipTitle = _ToolTipTitles[args.AssociatedControl];
      }
      else
      {
        _TheToolTip.ToolTipIcon = ToolTipIcon.None;
        _TheToolTip.ToolTipTitle = String.Format(Res.EFPFormProvider_ToolTip_Disabled, _ToolTipTitles[args.AssociatedControl]);
      }
    }

    /// <summary>
    /// Полное перестроение списка подсказок для всех управляющих элементов формы
    /// </summary>
    public void RefreshToolTips()
    {
      if (_TheToolTip == null) // 14.03.2008 - подсказки могут быть отключены
        return;
      _TheToolTip.RemoveAll();
      _ToolTipIcons.Clear();
      _ToolTipTitles.Clear();

      // Заставляем все управляющие элементы переустановить подсказки
      ParentProviderChanged();
    }

    #region Вложенные управляющие элементы для ToolTip'ов

    /// <summary>
    /// Вложенные управляющие элементы, подсказки в которых должны дублировать подсказку элемента.
    /// Ключ - вложенный элемент.
    /// Значение - родительский элемент (задаваемый EFPControlBase.Control).
    /// Если курсор мыши находится в элементе, задаваемым ключом, то 
    /// </summary>
    private Dictionary<Control, Control> _ToolTipNestedControls;

    /// <summary>
    /// Добавляет вложенный элемент, для которого надо показывать всплывающую подсказку как для основного элемента.
    /// Вызывается из <see cref="EFPControlBase.InitToolTipNestedControls()"/> (или переопределенного метода).
    /// </summary>
    /// <param name="childControl">Вложенный управляющий элемент</param>
    /// <param name="mainControl">Основной управляющий элемент <see cref="EFPControlBase.Control"/></param>
    public void SetNestedToolTipControl(Control childControl, Control mainControl)
    {
#if DEBUG
      if (childControl == null)
        throw new ArgumentNullException("childControl");
      if (mainControl == null)
        throw new ArgumentNullException("mainControl");
      if (childControl == mainControl)
        throw ExceptionFactory.ArgAreSame("childControl", "mainControl");
#endif
      if (_ToolTipNestedControls == null)
        return;

      _ToolTipNestedControls[childControl] = mainControl;
    }

    /// <summary>
    /// Удаляет из словаря _ToolTipNestedControls пары для уничтоженных управляющих элементов.
    /// </summary>
    private void ClearToolTipNestedControls()
    {
      if (_ToolTipNestedControls == null)
        return;
      if (_ToolTipNestedControls.Count == 0)
        return;
      Control[] children = new Control[_ToolTipNestedControls.Count];
      _ToolTipNestedControls.Keys.CopyTo(children, 0);

      for (int i = 0; i < children.Length; i++)
      {
        if (children[i].IsDisposed)
          _ToolTipNestedControls.Remove(children[i]);
        else if (_ToolTipNestedControls[children[i]].IsDisposed)
          _ToolTipNestedControls.Remove(children[i]);
      }
    }

    #endregion

    private Control _PrevControlWithToolTip;

    #region IEFPAppIdleHandler Members

    /// <summary>
    /// Используется для организации задержки перед показом подсказки в "ручном" режиме
    /// </summary>
    private System.Windows.Forms.Timer _ToolTipDelayTimer;

    void IEFPAppIdleHandler.HandleIdle()
    {
      ClearToolTipNestedControls(); // убираем удаленные ненужные ссылки

      if (Form.IsDisposed)
        return; // 16.06.2021
      if (!Visible)
        return; // 07.03.2022. На всякий случай
      if (Form.WindowState == FormWindowState.Minimized)
        return; // 27.02.2025

      if (_CommandItems != null)
      {
        if (_CommandItems.HasIdle)
          _CommandItems.HandleIdle(); // 27.02.2025
      }

      if (_ToolTipNestedControls != null)
      {
        // Элемент, над которым находится мышь
        Point pt = Form.PointToClient(Control.MousePosition);
        Control wantedControl = WinFormsTools.GetChildAtPointRecursive(Form, pt, GetChildAtPointSkip.Invisible);
        // Нужно ли выводить подсказку нестандартным образом?
        TestIsManualToolTipNeeded(ref wantedControl);

        if (!Object.ReferenceEquals(wantedControl, _PrevControlWithToolTip))
        {
          if (_ToolTipDelayTimer != null)
            _ToolTipDelayTimer.Enabled = false; // вдруг еще ожидается подсказка

          // Прячем старую подсказку
          if (_PrevControlWithToolTip != null)
          {
            if (_PrevControlWithToolTip.IsHandleCreated) // не был ли элемент отсоединен и разрушен?
              _TheToolTip.Hide(_PrevControlWithToolTip);
          }
          _PrevControlWithToolTip = wantedControl;
          if (wantedControl != null)
          {
            // Запускаем таймер для организации задержки
            if (_ToolTipDelayTimer == null)
            {
              _ToolTipDelayTimer = new Timer();
              _ToolTipDelayTimer.Enabled = false;
              _ToolTipDelayTimer.Interval = _TheToolTip.AutomaticDelay;
              _ToolTipDelayTimer.Tick += new EventHandler(ToolTipDelayTimer_Tick);
            }
            _ToolTipDelayTimer.Start(); // Запускаем
          }
        }
      }

      DelayedSetFocus(); // 17.02.2022
      InitLastFocusedControl();
      _VisibleCompleted = true;
    }

    /// <summary>
    /// Возвращает true, если для элемента задана всплывающая подсказка, с которой не справляется ToolTip стандартным образом
    /// </summary>
    /// <param name="wantedControl"></param>
    private void TestIsManualToolTipNeeded(ref Control wantedControl)
    {
      if (wantedControl == null)
        return;
      if (_ToolTipNestedControls == null)
      {
        wantedControl = null;
        return;
      }

      Control mainControl;
      if (_ToolTipNestedControls.TryGetValue(wantedControl, out mainControl))
      {
        wantedControl = mainControl;
        if (String.IsNullOrEmpty(_TheToolTip.GetToolTip(wantedControl)))
          wantedControl = null;
        return;
      }

      if (String.IsNullOrEmpty(_TheToolTip.GetToolTip(wantedControl)))
      {
        wantedControl = null;
        return;
      }

      if (!wantedControl.Enabled)
        return;

      if (wantedControl is Panel || wantedControl is UserControl || wantedControl is GroupBox)
        return;

      wantedControl = null;
    }

    void ToolTipDelayTimer_Tick(object sender, EventArgs args)
    {
      // Повторное срабатывание недопустимо
      if (_ToolTipDelayTimer != null)
        _ToolTipDelayTimer.Enabled = false;
      else
        return; // форма была уже скрыта

      if (_PrevControlWithToolTip == null)
        return; // успел поменяться или что-то пошло не так

      if (_PrevControlWithToolTip.IsDisposed)
        return;
      if (!_PrevControlWithToolTip.Visible) // спрятался
        return;

      if (!Form.Visible)
        return; // 25.11.2021

      string tt = _TheToolTip.GetToolTip(_PrevControlWithToolTip);
      // Считаем вручную позицию для отображения
      int x = _PrevControlWithToolTip.ClientSize.Width / 2;
      int y = _PrevControlWithToolTip.Height;
      _TheToolTip.Show(tt, _PrevControlWithToolTip, new Point(x, y), _TheToolTip.AutoPopDelay);
    }

    #endregion

    #endregion

    #region Вызов справки

    /// <summary>
    /// Контекст справки для формы в-целом
    /// </summary>
    public string HelpContext
    {
      get { return GetHelpContext(_Form); }
      set { SetHelpContext(_Form, value); }
    }

    /// <summary>
    /// Задание контекста справки для управляющего элемента
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="value">Контекст справки</param>
    public void SetHelpContext(Control control, string value)
    {
      if (control == null)
        throw new ArgumentNullException("control");
      if (_HelpContextItems == null)
        _HelpContextItems = new Dictionary<Control, string>();
      _HelpContextItems[control] = value;
      if (!String.IsNullOrEmpty(value)) // 26.10.2015
        InitFormHelp();
    }
    private Dictionary<Control, String> _HelpContextItems;

    /// <summary>
    /// Получение контекста справки для управляющего элемента. Сначала извлекается
    /// контекст для самого элемента. Если его нет, то берется контекст родительского
    /// элемента и т.д. до контекста формы в-целом.
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Контекст справки</returns>
    public string GetHelpContext(Control control)
    {
      if (_HelpContextItems == null)
        return null;
      while (control != null)
      {
        string HelpContext;
        if (_HelpContextItems.TryGetValue(control, out HelpContext))
        {
          if (!String.IsNullOrEmpty(HelpContext)) // условие добавлено 09.10.2017
            return HelpContext;
        }
        control = control.Parent;
      }
      return null;
    }

    /// <summary>
    /// Этот метод при первом вызове инициализирует подсистему обработки клавиши
    /// F1 для формы и наличие кнопки "?" в заголовке.
    /// Вызывается при установке контекста справки
    /// </summary>
    private void InitFormHelp()
    {
      if (_FormHelpButtonClickedHandler != null)
        return; // не первый вызов
      if (CommandItems.IsReadOnly)
        throw new InvalidOperationException(Res.EFPFormProvider_Err_CannotInitHelpContext);

      _FormHelpButtonClickedHandler = new CancelEventHandler(FormHelpButtonClickedProc);
      _Form.HelpButton = true;

      // Создание команды локального меню
      EFPCommandItem ciHelp;

      ciHelp = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.ContextHelp);
      ciHelp.MenuText = Res.EFPFormProvider_Menu_Help_ContextHelp;
      ciHelp.Enabled = true;
      ciHelp.Click += new EventHandler(ciHelp_Click);
      CommandItems.Add(ciHelp);

      EFPCommandItem ciDebug = null;
      if (DebugFormProvider)
      {
        ciDebug = new EFPCommandItem("Debug", "EFPFormProvider");
        ciDebug.MenuText = Res.EFPFormProvider_Menu_Debug_EFPFormProvider;
        ciDebug.Click += new EventHandler(ciDebug_Click);
        CommandItems.Add(ciDebug);
      }
    }

    /// <summary>
    /// Обработчик нажатия кнопки "?" на заголовке формы
    /// </summary>
    private CancelEventHandler _FormHelpButtonClickedHandler;

    private void FormHelpButtonClickedProc(object sender, CancelEventArgs args)
    {
      args.Cancel = true;
      PerformCallHelp();
    }

    /// <summary>
    /// Реализация запроса справки
    /// </summary>
    public void PerformCallHelp()
    {
      Control ctrl = _Form.ActiveControl;
      if (ctrl == null)
        ctrl = _Form;
      if (ctrl is TabControl)
      {
        // Если активен сам управляющий элемент с закладками, то надо выбрать 
        // текущую страницу. Так бывает, когда фокус ввода имеет непосредственно 
        // корешок закладки
        TabControl tabCtrl = (TabControl)ctrl;
        if (tabCtrl.TabPages.Count > 0)
          ctrl = tabCtrl.SelectedTab;
      }
      string ctx = GetHelpContext(ctrl);
      EFPApp.ShowHelp(ctx);
    }

    #endregion

    #region Обработка нажатий мыши в TabControl

    private static void InitControl(Control control)
    {
#if !NET
      if (control is DataGrid)
        return; // Там бяка - есть вложенный текстбокс, который мне не нужен
#endif
      // Рекурсивная функция


      //if (Control is TabPage)
      //  ((TabPage)Control).MouseClick += new MouseEventHandler(TabPage_MouseClick);
      if (control is TabControl)
        ((TabControl)control).MouseClick += new MouseEventHandler(TabControl_MouseClick);

      if (control.HasChildren)
      {
        foreach (Control ChildControl in control.Controls)
          InitControl(ChildControl);
      }
    }

    static void TabControl_MouseClick(object sender, MouseEventArgs args)
    {
      TabControl theControl = (TabControl)sender;
      WinFormsTools.CorrectTabControlActivation(theControl);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Закрыть форму, открытую в модальном или немодальном редиме.
    /// Выполняются все обычные действия по закрытию формы, в частности, вызывается
    /// обработчик <see cref="System.Windows.Forms.Form.Closing"/>.
    /// Метод возвращает true, если форма успешноь закрыта. Возвращается false,
    /// если обработчик отверг закрытие формы, при этом свойство <see cref="System.Windows.Forms.Form.DialogResult"/>
    /// сбрасывается в <see cref="DialogResult.None"/>.
    /// </summary>
    /// <param name="dialogResult">Код закрытия формы</param>
    /// <returns>true, если форма была успешно закрыта</returns>
    public bool CloseForm(DialogResult dialogResult)
    {
      _Form.DialogResult = dialogResult;
      _TempFormClosingFlag = false;
      // Добавляем временный обработчик FormClosing. Он будет последним в цепочке,
      // поэтому можно будет узнать значение Cancel
      FormClosingEventHandler TempFormClosingHandler = new FormClosingEventHandler(Temp_FormClosing);
      _Form.FormClosing += TempFormClosingHandler;
      try
      {
        _Form.Close();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, Res.EFPFormProvider_ErrTitle_FormClosed);
        _Form.DialogResult = DialogResult.None;
        _TempFormClosingFlag = false;
      }
      _Form.FormClosing -= TempFormClosingHandler;
      if (!_TempFormClosingFlag)
        _Form.DialogResult = DialogResult.None;
      return _TempFormClosingFlag;
    }

    private bool _TempFormClosingFlag;

    /// <summary>
    /// Флаг выполнения проверки закрытия формы должен выставляться в последнюю
    /// очередь, поэтому используем обработчик FormClosing, т.к. он вызывается
    /// после Closing
    /// </summary>
    private void Temp_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (!args.Cancel)
        _TempFormClosingFlag = true;
    }

    #endregion

    #region Обработка нажатия клавиш

    void Form_KeyDown(object sender, KeyEventArgs args)
    {
      try
      {
        EFPCommandItems.PerformKeyDown(sender, args);
        if (EFPApp.ShowToolTips && _TheToolTip != null)
        {
          _TheToolTip.Active = false;
          _TheToolTip.Active = true;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    //void Form_PreviewKeyDown(object sender, PreviewKeyDownEventArgs args)
    //{
    //  // 04.02.2020
    //  // Комбинации Ctrl-Enter, Shift-Enter, Alt-Enter относятся к управляющему элементу, а не предназначены для нажатия кнопки <ОК>
    //  if (args.KeyCode == Keys.Enter)
    //  {
    //    if (args.Shift || args.Control || args.Alt)
    //      args.IsInputKey = true;
    //  }
    //}


    #endregion

    #region События закрытия формы

    #region Closing

    /// <summary>
    /// Есть два события: Form.Closing (старое) и Form.FormClosing (новое в Net Framework 2).
    /// Closing вызывается до FormClosing.
    /// Проверка корректности полей формы должна выполняться ДО выполнения других
    /// действий пользователя, поэтому ее надо поместить в обработчик Closing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Form_Closing(object sender, CancelEventArgs args)
    {
      if (args.Cancel)
        return;
      try
      {
        if (_InsideFormClosing1)
        {
          args.Cancel = true;
          Form.DialogResult = DialogResult.None;
          EFPApp.ShowTempMessage(Res.EFPFormProvider_Err_NestedClosing);
        }
        else
        {
          _InsideFormClosing1 = true;
          try
          {
            if (ReentranceLocker.TryLock(Res.EFPFormProvider_Phase_FormClosing))
            {
              try
              {
                OnClosing(args);
              }
              finally
              {
                ReentranceLocker.Unlock();
              }
            }
            else
              args.Cancel = true;
          }
          finally
          {
            _InsideFormClosing1 = false;
          }
        }
      }
      catch (Exception e)
      {
        args.Cancel = true;
        Form.DialogResult = DialogResult.None;
        EFPApp.ShowException(e);
      }
    }

    private bool _InsideFormClosing1;

    /// <summary>
    /// Обработчик события <see cref="System.Windows.Forms.Form.Closing"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnClosing(CancelEventArgs args)
    {
      switch (Form.DialogResult)
      {
        case DialogResult.OK:
        case DialogResult.Yes:
          bool res = ValidateForm(EFPFormValidateReason.Closing);
          if (!res)
          {
            args.Cancel = true;
            Form.DialogResult = DialogResult.None;
          }
          break;
      }
    }

    #endregion

    #region FormClosing

    private void Form_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (args.Cancel)
        return;
      try
      {
        if (_InsideFormClosing2)
        {
          args.Cancel = true;
          Form.DialogResult = DialogResult.None;
          EFPApp.ShowTempMessage(Res.EFPFormProvider_Err_NestedClosing);
        }
        else
        {
          // 20.03.2018
          // Если в данный момент обрабатывается событие EFPApp.Closing, то вместо CloseReason.UserClosing
          // надо использовать ApplicationExitCall
          CloseReason reason = args.CloseReason;
          if (reason == CloseReason.UserClosing && EFPApp.IsClosing)
            reason = CloseReason.ApplicationExitCall;
          FormClosingEventArgs args2 = new FormClosingEventArgs(reason, false);

          _InsideFormClosing2 = true;
          try
          {
            OnFormClosing(args2);
          }
          finally
          {
            _InsideFormClosing2 = false;
          }
          if (args2.Cancel)
          {
            Form.DialogResult = DialogResult.None; // 19.09.2017
            args.Cancel = true;
          }
        }
      }
      catch (Exception e)
      {
        args.Cancel = true;
        Form.DialogResult = DialogResult.None;
        EFPApp.ShowException(e);
      }
    }

    private bool _InsideFormClosing2;

    /// <summary>
    /// Дублирует событие <see cref="System.Windows.Forms.Form.FormClosing"/> с перехватом исключений и блокировкой вложенного вызова,
    /// если обработка события выполняется долго.
    /// </summary>
    public event FormClosingEventHandler FormClosing;

    /// <summary>
    /// Вызывает обработчик события <see cref="FormClosing"/>, если он присоединен.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnFormClosing(FormClosingEventArgs args)
    {
      if (FormClosing != null && (!args.Cancel))
        FormClosing(this, args);
    }

    #endregion

    #region Closed

    void Form_FormClosed(object sender, FormClosedEventArgs args)
    {
      try
      {
        if (_InsideFormClosed)
          EFPApp.ShowTempMessage(Res.EFPFormProvider_Err_NestedClosing);
        else
        {
          // 20.03.2018
          // Если в данный момент обрабатывается событие EFPApp.Closing, то вместо CloseReason.UserClosing
          // надо использовать ApplicationExitCall
          CloseReason reason = args.CloseReason;
          if (reason == CloseReason.UserClosing && EFPApp.IsClosing)
            reason = CloseReason.ApplicationExitCall;
          FormClosedEventArgs args2 = new FormClosedEventArgs(reason);

          _InsideFormClosed = true;
          try
          {
            OnFormClosed(args2);
          }
          finally
          {
            _InsideFormClosed = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Дублирует событие <see cref="System.Windows.Forms.Form.FormClosed"/> с перехватом исключений.
    /// </summary>
    public event FormClosedEventHandler FormClosed;

    private void OnFormClosed(FormClosedEventArgs args)
    {
      DoForm_VisibleChanged(false);

      if (FormClosed != null)
        FormClosed(this, args);
    }

    private bool _InsideFormClosed;

    #endregion

    void Form_Disposed(object sender, EventArgs args)
    {
      _ProviderList.Remove(this);

      // 25.11.2021
      // Пытаемся исправить неперехватываемое исключение ObjectDisposedException.
      // Вдруг получится?
      if (_TheToolTip != null)
      {
        _TheToolTip.Dispose();
        _TheToolTip = null;
      }
    }

    #endregion

    #region Обработка команд

    void ciHelp_Click(object sender, EventArgs args)
    {
      PerformCallHelp();
    }

    void ciDebug_Click(object sender, EventArgs args)
    {
      DebugTools.DebugObject(this, "EFPFormProvider");
    }

    #endregion

    #region ChangeInfo

    /// <summary>
    /// Присоединение списка изменений.
    /// Когда есть изменения, в заголовке рисуется звездочка "(*)" слева от текста заголовка
    /// </summary>
    public DepChangeInfo ChangeInfo
    {
      get { return _ChangeInfo; }
      set
      {
        if (_ChangeInfo != null)
        {
          // Отсоединяем обработчик
          _ChangeInfo.ChangedChanged -= ChangeInfo_ChangedChanged;
        }
        _ChangeInfo = value;
        if (value != null)
        {
          value.ChangedChanged += ChangeInfo_ChangedChanged;
        }

        if (_ChangeInfo == null)
          ChangedFlag = false;
        else
          ChangedFlag = _ChangeInfo.Changed;
      }
    }
    private DepChangeInfo _ChangeInfo;

    private void ChangeInfo_ChangedChanged(object sender, EventArgs args)
    {
      ChangedFlag = _ChangeInfo.Changed;
    }

    internal bool ChangedFlag
    {
      get { return _ChangedFlag; }
      set
      {
        if (value == _ChangedFlag)
          return;
        _ChangedFlag = value;

        if (value)
          _Form.Text = "(*) " + _Form.Text;
        else if (_Form.Text.StartsWith("(*) ", StringComparison.Ordinal))
          _Form.Text = _Form.Text.Substring(4);
      }
    }
    private bool _ChangedFlag;

    #endregion

    #region Положение формы

    /// <summary>
    /// Инициализируется соответствующей перегрузкой метода EFPApp.ShowDialog(), если объект непустой.
    /// Иначе - null.
    /// </summary>
    internal EFPDialogPosition DialogPosition { get { return _DialogPosition; } set { _DialogPosition = value; } }
    private EFPDialogPosition _DialogPosition;

    /// <summary>
    /// Копия свойства EFPApp.DefaultScreen, действующая на момент создания EFPFormProvider.
    /// Нужна для вычисления положения блока диалога
    /// </summary>
    private Screen _DefaultScreen;

    private Form _DialogOwnerForm;

    /// <summary>
    /// Возвращает экран, на котором должен быть показан блок диалога
    /// </summary>
    private Screen StartScreen
    {
      get
      {
        if (_DialogPosition != null)
        {
          if (!_DialogPosition.PopupOwnerBounds.IsEmpty)
            return Screen.FromRectangle(_DialogPosition.PopupOwnerBounds);
          else if (_DialogPosition.PopupOwnerControl != null)
            return Screen.FromControl(_DialogPosition.PopupOwnerControl);
        }
        return _DefaultScreen;
      }
    }

    /// <summary>
    /// Положение формы при ее открытии
    /// </summary>
    private EFPFormBounds _StartBounds;

    #endregion

    #region Состояние формы

    void Form_SizeChanged(object sender, EventArgs args)
    {
      if (_Form.WindowState != FormWindowState.Minimized)
        _WindowStateBeforeMinimized = _Form.WindowState;
    }


    /// <summary>
    /// Возвращает состояние формы (<see cref="FormWindowState.Normal"/> или <see cref="FormWindowState.Maximized"/>), которое было до минимизации.
    /// Если в текущий момент окно не свернуто, возвращает <see cref="System.Windows.Forms.Form.WindowState"/>.
    /// </summary>
    public FormWindowState WindowStateBeforeMinimized { get { return _WindowStateBeforeMinimized; } }
    private FormWindowState _WindowStateBeforeMinimized;

    #endregion

    #region Сохранение конфигурации

    /// <summary>
    /// Если свойство возвращает непустую строку, то форма умеет сохранять собственные
    /// данные в секциях конфигурации.
    /// Дублирует свойство <see cref="EFPConfigHandler.ConfigSectionName"/>.
    /// </summary>
    public string ConfigSectionName
    {
      get { return ConfigHandler.ConfigSectionName; }
      set { ConfigHandler.ConfigSectionName = value; }
    }


    /// <summary>
    /// Обработчик конфигурации. Существует всегда.
    /// Чтобы зарегистрировать категорию, для которой будут записываться данные секции конфигурации, конструктор производного класса вызывает ConfigHandler.Categories.Add("Filters"). 
    /// Методы коллекции могут вызываться только до <see cref="OnShown()"/>.
    /// Если нужно записать конфигурацию, то должен быть установлен флаг для категории вызовом <see cref="ConfigHandler"/>.Changed["Filters"]=true.
    /// </summary>
    public EFPConfigHandler ConfigHandler { get { return _ConfigHandler; } }
    private EFPConfigHandler _ConfigHandler;

    /// <summary>
    /// Вызывает <see cref="ConfigHandler"/>.ReadConfig() для чтения значений.
    /// Ничего не делает, если свойство <see cref="ConfigSectionName"/> не установлено.
    /// Повторные вызовы метода игнорируются
    /// Первоначальное чтение конфигурации формы.
    /// Как правило, этот метод вызывается из <see cref="OnShown()"/>, но может быть вызван досрочно внешним кодом.
    /// Не вызывайте этот метод из конструктора производного класса, так как конструктор класса-наследника
    /// (если он есть) может вызвать ошибку. К тому же, внешний код может, например, изменить <see cref="ConfigHandler"/>.
    /// Не рекомендуется использовать этот метод без крайней необходимости.
    /// </summary>
    public void FirstReadConfig()
    {
      if (_FirstReadConfigCalled)
        return;
      _FirstReadConfigCalled = true;

      //if (ConfigHandler == null)
      //  return;

      if (!EFPApp.InsideLoadComposition)
      {
        PreloadConfigSections(EFPConfigMode.Read);
        ConfigHandler.ReadConfig(this.ConfigManager);
      }

      FormWindowState oldWS = Form.WindowState;
      Form.WindowState = FormWindowState.Normal;

      if (Modal)
      {
        if (DialogPosition != null)
        {
          Form.StartPosition = FormStartPosition.Manual;
          if (DialogPosition.PopupOwnerControl != null)
            WinFormsTools.PlacePopupForm(Form, DialogPosition.PopupOwnerControl);
          else if (!DialogPosition.PopupOwnerBounds.IsEmpty)
            WinFormsTools.PlacePopupForm(Form, DialogPosition.PopupOwnerBounds);
        }

        switch (Form.StartPosition)
        {
          case FormStartPosition.CenterScreen:
          case FormStartPosition.CenterParent:
            if (_DialogOwnerForm == null)
              WinFormsTools.PlaceFormInScreenCenter(Form, _DefaultScreen);
            else
            {
              // 14.05.2021
              // Центрируем относительно родительского окна, а не экрана
              Size sz1 = Form.Size;
              Size sz2 = _DialogOwnerForm.ClientSize;
              Form.Location = new Point(_DialogOwnerForm.Location.X + (sz2.Width - sz1.Width) / 2,
                _DialogOwnerForm.Location.Y + (sz2.Height - sz1.Height) / 2);
              Form.StartPosition = FormStartPosition.Manual;
            }
            break;
        }

        Screen scr = Screen.FromControl(Form);
        if (_DialogPosition != null)
        {
          if (!_DialogPosition.PopupOwnerBounds.IsEmpty)
            scr = Screen.FromRectangle(_DialogPosition.PopupOwnerBounds);
          else if (_DialogPosition.PopupOwnerControl != null)
            scr = Screen.FromControl(_DialogPosition.PopupOwnerControl);
        }
        WinFormsTools.PlaceFormInRectangle(Form, scr.WorkingArea);
      }

      Form.WindowState = oldWS;
    }

    private bool _FirstReadConfigCalled;

    /// <summary>
    /// Этот метод собирается список EFPPreloadConfigSectionInfo и вызывает EFPApp.ConfigManager.Preload()
    /// </summary>
    /// <param name="rwMode"></param>
    private void PreloadConfigSections(EFPConfigMode rwMode)
    {
      SingleScopeList<EFPConfigSectionInfo> configInfos = new SingleScopeList<EFPConfigSectionInfo>();
      ConfigHandler.GetPreloadConfigSections(configInfos, rwMode);

      foreach (EFPControlBase controlProvider in GetAllControlProviders())
      {
        if (controlProvider.ConfigHandler != null)
          controlProvider.ConfigHandler.GetPreloadConfigSections(configInfos, rwMode);
      }

      if (configInfos.Count >= 2) // иначе смысла нет
        ConfigManager.Preload(configInfos.ToArray(), rwMode);
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Добавляет в список <paramref name="categories"/> категории, для которых будут создаваться секции конфигурации.
    /// Непереопределенный метод добавляет только категорию <see cref="EFPConfigCategories.FormBounds"/>.
    /// Не следует добавлять категории, которые записываются самими управляющими элементами.
    /// </summary>
    /// <param name="categories">Список для добавления категорий</param>
    /// <param name="rwMode">Режим чтения или записи</param>
    /// <param name="actionInfo">Информация о действии</param>
    public virtual void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      bool saveFormBounds;
      if (actionInfo.Purpose == EFPConfigPurpose.Composition)
        saveFormBounds = true;
      else
        saveFormBounds = EFPApp.SaveFormBounds && (GetWantedFormBoundsParts(rwMode) != EFPFormBoundsPart.None); // 09.06.2021

      if (saveFormBounds)
        categories.Add(EFPConfigCategories.FormBounds);
    }

    /// <summary>
    /// Возввращает имя подсекции конфигурации, связанной с текущим разрешением и системным шрифтом
    /// </summary>
    private string ScreenSubSectionName
    {
      get
      {
        Screen scr = StartScreen;
#if DEBUG
        if (scr == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "StartScreen");
#endif
        StringBuilder sb = new StringBuilder();
        sb.Append("Screen");
        sb.Append(StdConvert.ToString(scr.Bounds.Width));
        sb.Append("x");
        sb.Append(StdConvert.ToString(scr.Bounds.Height));
        switch (Form.AutoScaleMode)
        {
          case AutoScaleMode.Font:
            sb.Append("_Font");
            sb.Append(Form.CurrentAutoScaleDimensions.Height);
            sb.Append("x");
            sb.Append(Form.CurrentAutoScaleDimensions.Width);
            break;
          case AutoScaleMode.Dpi:
            sb.Append("_Dpi");
            sb.Append(Form.CurrentAutoScaleDimensions.Width);
            if (Form.CurrentAutoScaleDimensions.Width != Form.CurrentAutoScaleDimensions.Height)
            {
              sb.Append("x");
              sb.Append(Form.CurrentAutoScaleDimensions.Height);
            }
            break;
        }
        return sb.ToString();
      }
    }

    /// <summary>
    /// Возвращает, какие части координат формы должны быть прочитаны или записаны.
    /// Не используется при сохранении композиции рабочего стола.
    /// </summary>
    /// <param name="rwMode">Запись или чтение?</param>
    /// <returns></returns>
    private EFPFormBoundsPart GetWantedFormBoundsParts(EFPConfigMode rwMode)
    {
      if (!Form.IsMdiChild)
      {
        if (Form.Parent != null)
          return EFPFormBoundsPart.None; // Окно на tab-интерфейсе
      }

      EFPFormBoundsPart parts = EFPFormBoundsPart.None;
      if (Form.MaximizeBox)
      {
        if (rwMode == EFPConfigMode.Read || Form.WindowState != FormWindowState.Minimized)
          parts |= EFPFormBoundsPart.WindowState;

        if (Form.MdiParent != null && rwMode == EFPConfigMode.Read)
        {
          if (Form.MdiParent.MdiChildren.Length > 1)
            // Если в MDI-контейнере уже есть другие открытые окна, то состояние Maximized зависит от активного окна,
            // а не от сохраненного состояния
            parts &= ~EFPFormBoundsPart.WindowState;
        }
      }

      if (Form.FormBorderStyle == FormBorderStyle.Sizable || Form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
      {
        //if (Modal || rwMode == EFPConfigMode.Write)
        parts |= EFPFormBoundsPart.Size;
        //else
        //{
        //  if (Form.StartPosition != FormStartPosition.Manual)
        //    parts |= EFPFormBoundsPart.Size;
        //}

        // В немодальном режиме сохраняем размеры даже в режиме FormStartPosition.Manual.
        // Они все равно такие в момент показа формы, так как размещением окон занимается интерфейс
      }

      if (Modal && (DialogPosition == null))
        parts |= EFPFormBoundsPart.Location;

      return parts;
    }

    /// <summary>
    /// Записывает секцию конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Доступ к секции конфигурации</param>
    /// <param name="actionInfo">Информация о действии</param>
    public virtual void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      if (category == EFPConfigCategories.FormBounds)
      {
        EFPFormBounds bounds = new EFPFormBounds();
        bounds.FromControl(Form);
        if (actionInfo.Purpose == EFPConfigPurpose.Composition)
          bounds.WriteConfig(cfg);
        else
        {
          EFPFormBoundsPart wantedParts = GetWantedFormBoundsParts(EFPConfigMode.Write);
          CfgPart cfg2 = cfg.GetChild(Modal ? "Dialog" : "Form", true);
          if ((wantedParts & EFPFormBoundsPart.WindowState) != 0)
            cfg2.SetEnum<FormWindowState>("State", bounds.WindowState);
          //if (bounds.WindowState != FormWindowState.Maximized) // 13.09.2021
          if (bounds.WindowState == FormWindowState.Normal) // 26.02.2025
          {
            CfgPart cfg3 = cfg2.GetChild(ScreenSubSectionName, true);
            if ((wantedParts & EFPFormBoundsPart.Size) != 0)
            {
              cfg3.SetInt("Width", bounds.Bounds.Width);
              cfg3.SetInt("Height", bounds.Bounds.Height);
            }
            if ((wantedParts & EFPFormBoundsPart.Location) != 0)
            {
              // Середина окна
              int cx = bounds.Bounds.Left + bounds.Bounds.Width / 2;
              int cy = bounds.Bounds.Top + bounds.Bounds.Height / 2;

              if (EFPApp.MainWindow == null)
              {
                // Середина экрана
                int cx0 = _DefaultScreen.WorkingArea.X + _DefaultScreen.WorkingArea.Width / 2;
                int cy0 = _DefaultScreen.WorkingArea.Y + _DefaultScreen.WorkingArea.Height / 2;

                cfg3.SetInt("ScreenCenterDX", cx - cx0);
                cfg3.SetInt("ScreenCenterDY", cy - cy0);
              }
              else
              {
                // Середина главного окна
                int cx0 = EFPApp.MainWindow.Bounds.Left + EFPApp.MainWindow.Bounds.Width / 2;
                int cy0 = EFPApp.MainWindow.Bounds.Top + EFPApp.MainWindow.Bounds.Height / 2;

                cfg3.SetInt("MainWindowCenterDX", cx - cx0);
                cfg3.SetInt("MainWindowCenterDY", cy - cy0);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Какие части координат и состояния формы были прочитаны из секции конфигурации и применены при показе формы.
    /// Инициализируется методом ReadConfigPart()
    /// </summary>
    internal EFPFormBoundsPart ReadConfigFormBoundsParts { get { return _ReadConfigFormBoundsParts; } }
    private EFPFormBoundsPart _ReadConfigFormBoundsParts;

    /// <summary>
    /// Читает секцию конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Доступ к секции конфигурации</param>
    /// <param name="actionInfo">Информация о действии</param>
    public virtual void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      if (category == EFPConfigCategories.FormBounds)
      {
        if (actionInfo.Purpose == EFPConfigPurpose.Composition)
        {
          EFPFormBounds Bounds = new EFPFormBounds();
          Bounds.ReadConfig(cfg);
          _ReadConfigFormBoundsParts = Bounds.ToControl(Form);
        }
        else
        {
          CfgPart cfg2 = cfg.GetChild(Modal ? "Dialog" : "Form", false);
          if (cfg2 != null)
          {
            EFPFormBoundsPart wantedParts = GetWantedFormBoundsParts(EFPConfigMode.Read); // исправлено 13.09.2021
            EFPFormBoundsPart realParts = EFPFormBoundsPart.None;
            EFPFormBounds bounds = new EFPFormBounds();
            bounds.FromControl(Form);

            if ((wantedParts & EFPFormBoundsPart.WindowState) != 0)
            {
              FormWindowState ws = FormWindowState.Normal;
              if (cfg2.GetEnum<FormWindowState>("State", ref ws))
              {
                bounds.WindowState = ws;
                realParts |= EFPFormBoundsPart.WindowState;
              }
            }
            CfgPart cfg3 = cfg2.GetChild(ScreenSubSectionName, false);
            if (cfg3 != null)
            {
              if ((wantedParts & EFPFormBoundsPart.Size) != 0)
              {
                int w = cfg3.GetInt("Width");
                int h = cfg3.GetInt("Height");
                if (w > 0 && h > 0)
                  realParts |= EFPFormBoundsPart.Size;
                else
                {
                  w = bounds.Bounds.Width;
                  h = bounds.Bounds.Height;
                }

                if ((wantedParts & EFPFormBoundsPart.Location) != 0) // только если размеры тоже прочитали
                {

                  string prefix = EFPApp.MainWindow == null ? "ScreenCenter" : "MainWindowCenter";
                  Rectangle rect = EFPApp.MainWindow == null ? _DefaultScreen.WorkingArea : EFPApp.MainWindow.Bounds;
                  int cx0 = rect.Left + rect.Width / 2;
                  int cy0 = rect.Top + rect.Height / 2;
                  int dx = cfg3.GetInt(prefix + "DX");
                  int dy = cfg3.GetInt(prefix + "DY");

                  int cx = cx0 + dx;
                  int cy = cy0 + dy;

                  int left = cx - w / 2;
                  int top = cy - h / 2;
                  bounds.Bounds = new Rectangle(left, top, w, h);
                  if (_DialogPosition == null)
                    realParts |= EFPFormBoundsPart.Location;
                }
                else // !Location
                {
                  bounds.Bounds = new Rectangle(bounds.Bounds.Left, bounds.Bounds.Top, w, h);
                }
              }
            }
            _ReadConfigFormBoundsParts = bounds.ToControl(Form, realParts);
          }
        }
      }
    }

    #endregion

    #region Чтение и запись композиции

    /// <summary>
    /// Имя класс для записи композиции окон.
    /// По умолчанию возвращает имя класса формы (если это не базовый класс <see cref="System.Windows.Forms.Form"/>).
    /// Если свойство установлено как пустая строка, то окно не будет сохранять композицию.
    /// </summary>
    public string ConfigClassName
    {
      get
      {
        if (_ConfigClassName == null)
        {
          if (Form.GetType() == typeof(Form))
            return String.Empty;
          else
            return Form.GetType().ToString();
        }
        else
          return _ConfigClassName;
      }
      set { _ConfigClassName = value; }
    }
    private string _ConfigClassName = null; // различаем null и пустую строку

    /// <summary>
    /// Записывает композицию рабочего стола.
    /// Записываются как параметры самой формы, так и управляющих элементов,
    /// для которых это предусмотрено.
    /// Этот метод вызывается классом <see cref="EFPAppInterface"/>.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      cfg.SetString("Class", ConfigClassName);
      if (!String.IsNullOrEmpty(ConfigSectionName))
        cfg.SetString("ConfigSectionName", ConfigSectionName);
      cfg.SetString("Title", Form.Text);

      ConfigHandler.WriteComposition(cfg);

      foreach (EFPControlBase controlProvider in GetAllControlProviders())
      {
        if (controlProvider.ConfigHandler == null)
          continue;

        if (String.IsNullOrEmpty(controlProvider.Control.Name))
          continue;
        CfgPart cfgControl = cfg.GetChild(controlProvider.Control.Name, true);
        controlProvider.ConfigHandler.WriteComposition(cfgControl);
      }
    }

    /// <summary>
    /// Восстанавливает композицию рабочего стола.
    /// Читаются как параметры самой формы, так и управляющих элементов,
    /// для которых это предусмотрено.
    /// Этот метод вызывается классом <see cref="EFPAppInterface"/>.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      ConfigHandler.ReadComposition(cfg);

      foreach (EFPControlBase controlProvider in GetAllControlProviders())
      {
        controlProvider.LoadConfig(); // чтобы предотвратить отложенную инициализацию после EFPApp.InsideLoadComposition=true
        // Также инициализируются, например, фильтры.
        // Загружаются значения по умолчанию, т.к. ConfigHandler.LoadConfig() не вызывается

        if (controlProvider.ConfigHandler != null)
        {
          if (!String.IsNullOrEmpty(controlProvider.Control.Name))
          {
            CfgPart cfgControl = cfg.GetChild(controlProvider.Control.Name, false);
            if (cfgControl != null)
              controlProvider.ConfigHandler.ReadComposition(cfgControl);
          }
        }
      }
    }

    #endregion

    #region Минимальный размер формы
#if XXX // так не работает
    private SizeF DesignAutoScaleDimensions;


    /// <summary>
    /// Дублирует защищенное свойство Form.AutoScaleFactor
    /// </summary>
    private SizeF AutoScaleFactor
    {
      get 
      {
        SizeF sz1 = DesignAutoScaleDimensions;
        SizeF sz2=Form.CurrentAutoScaleDimensions;
        if (sz1.Width == 0 || sz1.Height == 0)
          return new SizeF(1, 1);
        else
          return new SizeF(sz2.Width / sz1.Width, sz2.Height / sz1.Height);
      }
    }

    public Size ClientMinimumSize
    {
      get
      {
        if (FClientMinimumSize.IsEmpty)
          return WinFormsTools.GetFormClientMinimumSize(Form);
        else
          return FClientMinimumSize;
      }
      set
      {
        CheckHasNotBeenShown();
        FClientMinimumSize = value;
      }
    }
    private Size FClientMinimumSize;
#endif

    #endregion

    #region Обновление по таймеру

    /// <summary>
    /// Пользовательские обработчики обновления формы по времени.
    /// Предотвращается вложенный вызов обработчиков.
    /// События могут не вызываться, если окно свернуто или закрыто другими окнами.
    /// 
    /// Если требуется обработка по таймеру, независимая от видимости формы на экране, то следует:
    /// 1. Реализовать интерфейс <see cref="IEFPAppTimeHandler"/> в программе.
    /// 2. Переопределить <see cref="System.Windows.Forms.Control.OnVisibleChanged(EventArgs)"/> и присоединить/отсоединить в нем форму к <see cref="EFPApp.Timers"/>
    /// </summary>
    public ICollection<EFPUpdateByTimeHandler> UpdateByTimeHandlers
    {
      get
      {
        if (_UpdateByTimeHandlers == null)
        {
          _UpdateByTimeHandlers = new List<EFPUpdateByTimeHandler>();
          if (/*HasBeenShown*/ Form.Visible /* 21.05.2021 */)
            EFPApp.Timers.Add(this);
        }
        return _UpdateByTimeHandlers;
      }
    }
    private List<EFPUpdateByTimeHandler> _UpdateByTimeHandlers;


    /// <summary>
    /// Вызывает событие <see cref="EFPUpdateByTimeHandler.Tick"/> для всех присоединенных обработчиков в списке <see cref="UpdateByTimeHandlers"/>, независимо от того, вышло требуемое время или нет.
    /// Значения свойств <see cref="EFPUpdateByTimeHandler.Enabled"/> игнорируется.
    /// </summary>
    public void CallUpdateByTime()
    {
      if (_UpdateByTimeHandlers == null)
        return;
      for (int i = 0; i < _UpdateByTimeHandlers.Count; i++)
        _UpdateByTimeHandlers[i].CallTick();
    }

    void IEFPAppTimeHandler.TimerTick()
    {
      if (_UpdateByTimeHandlers == null)
        return;
      if (_UpdateByTimeHandlers.Count == 0)
        return;
      if (Form.IsDisposed)
        return; // 16.06.2021

      // 20.10.2016
      // Не посылаем сигнал таймера, если окно свернуто
      if (Form.WindowState == FormWindowState.Minimized)
        return;
      if (Form.MdiParent != null)
      {
        if (Form.MdiParent.WindowState == FormWindowState.Minimized)
          return;
        if (Form.MdiParent.ActiveMdiChild != Form && Form.MdiParent.ActiveMdiChild != null) // активно другое MDI-окно
        {
          if (Form.MdiParent.ActiveMdiChild.WindowState == FormWindowState.Maximized)
            return;
        }
      }

      for (int i = 0; i < _UpdateByTimeHandlers.Count; i++)
        _UpdateByTimeHandlers[i].TimerTick();
    }

    #endregion

    #region Статусная строка

    /// <summary>
    /// Использовать собственную статусную строку для формы.
    /// Это - Ambient-свойство.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public bool OwnStatusBar
    {
      get
      {
        if (_OwnStatusBar.HasValue)
          return _OwnStatusBar.Value;
        else
          return GetDefaultOwnStatusBar();
      }
      set
      {
        if (HasBeenShown)
          throw new InvalidOperationException();
        _OwnStatusBar = value;
      }
    }
    private bool? _OwnStatusBar;

    /// <summary>
    /// Сброс ambient-свойства <see cref="OwnStatusBar"/> в значение по умолчанию
    /// </summary>
    public void ResetOwnStatusBar()
    {
      if (HasBeenShown)
        throw new InvalidOperationException();
      _OwnStatusBar = null;
    }


    private bool GetDefaultOwnStatusBar()
    {
      bool res;
      try
      {
        res = DoGetDefaultOwnStatusBar();
      }
      catch (Exception e) // 23.11.2018
      {
        LogoutTools.LogoutException(e);
        res = false;
      }
      return res;
    }

    private bool DoGetDefaultOwnStatusBar()
    {
      if (!EFPApp.AppWasInit)
        return false; // 13.12.2016

      if (!EFPApp.OwnStatusBarsIfNeeded)
        return false;

      if (EFPApp.ExternalDialogOwnerWindow == null) // 14.03.2017
      {
        // 14.01.2017
        // Проверяем, что уже есть максимизированный блок диалога
        if (EFPApp.IsMainThread) // наверное, не нужно
        {
          Form[] dlgs = EFPApp.GetDialogStack(); // наша форма тоже может быть в списке
          for (int i = 0; i < dlgs.Length; i++)
          {
#if DEBUG
            if (dlgs[i] == null)
              throw new BugException("A hole in dialog stack");
#endif
            if (Object.ReferenceEquals(dlgs[i], this.Form)) // 06.12.2018 пропускаем себя в списке, т.к. определяем ниже
              continue;

            if (dlgs[i].WindowState == FormWindowState.Maximized)
              return ContainsControlWantedStatusBar();
          }
        }

        bool isMaximized;
        //EFPFormBounds Bounds = GetSavedBounds();
        //if (Bounds != null)
        //  IsMaximized = (Bounds.WindowState == FormWindowState.Maximized); // исправлено 06.12.2018
        //else
        isMaximized = _Form.WindowState == FormWindowState.Maximized;

        bool isFullScreenModal = Modal && isMaximized;

        if (!isFullScreenModal)
        {
          if (EFPApp.StatusBar != null) // 23.11.2018
          {
            if (EFPApp.StatusBar.StatusStripControl != null)
              return false;
          }
        }
      }

      if (ContainsControlWantedStatusBar())
        return true;
      return false;
    }

    private bool _InsideOnSetFormVisible_StatusBarNeeded;

    private bool ContainsControlWantedStatusBar()
    {
      foreach (EFPControlBase controlProvider in GetAllControlProviders())
      {
        foreach (EFPCommandItem ci in controlProvider.CommandItems)
        {
          //if (ci.StatusBarUsage && ci.Visible /* 21.02.2020 */)
          //  return true;
          if (ci.StatusBarUsage)
          {
            if (_InsideOnSetFormVisible_StatusBarNeeded)
            {
              //controlProvider.PrepareCommandItems(); // 02.12.2024
              controlProvider.FormVisibleChanged(); // 08.04.2025. Вызывает событие Created, а не только итнициализирует команды меню
            }
            if (ci.Visible /* 21.02.2020 */ &&
              ci.StatusBarUsage) // 03.12.2024 - свойство могло поменяться в EFPCommandItem.OnPrepare()
              return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Управляющий элемент статусной строки для добавления панелек.
    /// Если для формы не предусмотрена собственная статусная строка, возвращается 
    /// <see cref="EFPApp.StatusBar"/>.Control.
    /// Свойство может быть установлено вручную (до вывода формы на экран) или статусная строка
    /// может быть создана автоматически, установкой свойства <see cref="OwnStatusBar"/>.
    /// </summary>
    public StatusStrip StatusStripControl
    {
      get
      {
        if (_StatusStripControl == null)
          return EFPApp.StatusBar.StatusStripControl;
        else
          return _StatusStripControl;
      }
      set
      {
        if (HasBeenShown)
          throw new InvalidOperationException();

        _OwnStatusBar = (value != null);
        _StatusStripControl = value;
      }
    }
    private StatusStrip _StatusStripControl;

    /// <summary>
    /// Добавление внизу формы панели статусной строки
    /// </summary>                                                             
    private void InitOwnStatusBar(bool adjustFormSize)
    {
      _StatusStripControl = new StatusStrip();
      _StatusStripControl.Dock = DockStyle.Bottom;
      if (EFPApp.AppWasInit) // 24.11.2016
        _StatusStripControl.ImageList = EFPApp.MainImages.ImageList;
      //FStatusStripControl.ShowItemToolTips = true;
      EFPApp.SetStatusStripHeight(_StatusStripControl, Form);
      Form.Controls.Add(_StatusStripControl);
      if (adjustFormSize)
      {
        if (Form.WindowState == FormWindowState.Normal) // ? более сложная проверка
          Form.Height += _StatusStripControl.Height;
      }
    }

    /// <summary>
    /// Обработчик статусной строки формы.
    /// Может быть либо собственной статусной строкой, либо статусной строкой главного окна.
    /// </summary>
    public EFPStatusBarHandler StatusBarHandler
    {
      get
      {
        if (_StatusStripControl == null)
        {
          if (EFPApp.Interface != null)
          {
            EFPAppMainWindowLayout mw = EFPApp.Interface.FindMainWindowLayout(Form);
            if (mw != null)
            {
              if (mw.StatusBar != null)
                return mw.StatusBar.StatusBarHandler; // 09.07.2021
            }
          }
          if (EFPApp.StatusBar == null)
            return null; // 22.07.2019
          else
            return EFPApp.StatusBar.StatusBarHandler;
        }
        else
          return _StatusBarHandler;
      }
    }
    private EFPStatusBarHandler _StatusBarHandler;

    /// <summary>
    /// Ссылка на управляющий элемент, который последним получил фокус ввода.
    /// Может быть null.
    /// </summary>
    internal Control LastFocusedControl // TODO: не оптимизировано, надо использовать Idle
    {
      get
      {
        InitLastFocusedControl();
        return _LastFocusedControl;
      }
    }

    private Control _LastFocusedControl;

    private void InitLastFocusedControl()
    {
      Control focusedControl = WinFormsTools.GetFocusedControl(Form);
      if (focusedControl != null)
        _LastFocusedControl = focusedControl;
      else if (_LastFocusedControl != null)
      {
        if (_LastFocusedControl.IsDisposed || (!_LastFocusedControl.Visible) || (!_LastFocusedControl.Enabled))
          _LastFocusedControl = null;
      }
    }

    internal bool StatusBarPanelsShouldBeDetached()
    {
      if (EFPApp.ActiveDialog != null && EFPApp.ActiveDialog != this.Form)
        return true;

      if (StatusBarHandler == null)
        return true;
      if (StatusBarHandler.IsFormOwned)
        return false;
      else
        return !Active;
    }

    #endregion

    #region Временная установка кнопки по умолчанию

    internal IButtonControl TempDefaultButton
    {
      get { return _TempDefaultButton; }
      set
      {
        if (Object.ReferenceEquals(value, _TempDefaultButton))
          return;
        if (_TempDefaultButton == null)
          _SavedDefaultButton = Form.AcceptButton;
        _TempDefaultButton = value;
        if (value == null)
        {
          Form.AcceptButton = _SavedDefaultButton;
          _SavedDefaultButton = null;
        }
        else
          Form.AcceptButton = value;
      }
    }
    private IButtonControl _TempDefaultButton;

    private IButtonControl _SavedDefaultButton;

    #endregion

    #region Закрытие формы по нажатию клавиши Cancel

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), свойство <see cref="System.Windows.Forms.Form.CancelButton"/> не установлено 
    /// (нет кнопки "Отмена") в блоке диалога) и форма выводится в модальном режиме, 
    /// то форма будет закрываться нажатием кнопки "Отмена".
    /// </summary>
    public bool CloseModalFormByEscape
    {
      get { return _CloseModalFormByEscape; }
      set { _CloseModalFormByEscape = value; }
    }
    private bool _CloseModalFormByEscape;

    #endregion

    #region Отложенная установка фокуса ввода

    /// <summary>
    /// Запоминается вызов EFPControlBase.SetFocus(), если форма или управляющий элемент еще не выведен на экран
    /// </summary>
    internal EFPControlBase DelayedSetFocusControlProvider;

    /// <summary>
    /// Предотвращение переполнения вывода в log-файлы
    /// </summary>
    private bool _DelayedSetFocusExceptionLogged;

    private void DelayedSetFocus()
    {
      if (!Form.Visible)
        return; // 18.02.2022

      try
      {
        if (_FormActivatedSetFocusFlag)
        {
          _FormActivatedSetFocusFlag = false;
          Control startControl = Form.ActiveControl;
          if (startControl == null)
            startControl = Form.GetNextControl(null, true); // 24.08.2016
          startControl = WinFormsTools.FocusToControl(startControl); // 18.02.2022
          if (startControl is TabControl)
            WinFormsTools.CorrectTabControlActivation((TabControl)(startControl));
        }

        // Исправлено 17.02.2022
        if (DelayedSetFocusControlProvider != null)
        {
          switch (DelayedSetFocusControlProvider.ProviderState)
          {
            case EFPControlProviderState.Attached:
              WinFormsTools.FocusToControl(DelayedSetFocusControlProvider.Control);
              DelayedSetFocusControlProvider = null;
              break;
            case EFPControlProviderState.Disposed:
              DelayedSetFocusControlProvider = null;
              break;

            default:
              // Иначе подождем еще
              break;
          }
        }
      }
      catch (Exception e)
      {
        if (!_DelayedSetFocusExceptionLogged)
        {
          _DelayedSetFocusExceptionLogged = true;
          EFPApp.ShowException(e, Res.EFPFormProvider_ErrTitle_SetFocus);
        }
      }
    }

    #endregion

    #region Сохранение значка для формы

    internal Image FormIconImage { get { return _FormIconImage; } }
    private Image _FormIconImage;

    /// <summary>
    /// Создает изображения из текущего значка формы.
    /// Если значка нет, используется стандартное пустое "изображение".
    /// Используется в <see cref="EFPAppInterface"/>.
    /// </summary>
    internal void InitFormIconImage()
    {
      try
      {
        if (Form.Icon == null)
          _FormIconImage = EFPApp.MainImages.Images["EmptyImage"];
        else
          _FormIconImage = Form.Icon.ToBitmap();
      }
      catch
      {
        _FormIconImage = EFPApp.MainImages.Images["Error"];
      }
    }

    #endregion

    #region Значки и текст кнопок

    /// <summary>
    /// Установка изображений для кнопок "ОК", "Отмена" в форме
    /// </summary>
    /// <param name="control">Форма, в которой нужно украсить кнопки</param>
    private static void InitStdButtonTextAndImage(Control control)
    {
      if (control == null)
        return;

      Button btn = control as Button;
      if (btn != null)
      {
        WinFormsTools.UpdateStdButtonText(btn);
        string imageKey;
        switch (btn.DialogResult)
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
        btn.Image = EFPApp.MainImages.Images[imageKey];
        btn.ImageAlign = ContentAlignment.MiddleLeft;
      }
      else
      {
        if (control.HasChildren)
        {
          foreach (Control childControl in control.Controls)
            InitStdButtonTextAndImage(childControl); // рекурсивная процедура
        }
      }
    }

    #endregion

    #region Обработчики для кнопок

    private void InitButtonHandlers(Control control)
    {
      Button btn = control as Button;
      if (btn == null)
      {
        if (control.HasChildren && (!(control is UserControl)) && base.FindControlProvider(control) == null)
        {
          foreach (Control child in control.Controls)
            InitButtonHandlers(child); // рекурсивный вызов
        }
        return;
      }

      if (btn.DialogResult == DialogResult.None)
        return;

      if (base.FindControlProvider(btn) == null)
        new EFPButton(this, btn); // создаем недостающий провайдер кнопки, который умеет закрывать форму
    }


    #endregion

    #region Поиск провайдера для формы

    private static WeakReferenceCollection<EFPFormProvider> _ProviderList = new WeakReferenceCollection<EFPFormProvider>();

    /// <summary>
    /// Поиск объекта EFPFormProvider, созданного для указанной формы.
    /// Если провайдер не найден, возвращает null.
    /// Этот метод является потокобезопасным
    /// </summary>
    /// <param name="form">Форма, для которой нужно найти провайдер. Может быть null.</param>
    /// <returns>Найденный провайдер или null</returns>
    public static EFPFormProvider FindFormProvider(Form form)
    {
      if (form == null)
        return null;
      foreach (EFPFormProvider provider in _ProviderList)
      {
        if (Object.ReferenceEquals(provider.Form, form))
          return provider;
      }
      return null;
    }

    /// <summary>
    /// Поиск объекта <see cref="EFPFormProvider"/>, созданного для указанной формы.
    /// Если провайдер не найден, генерирует исключение.
    /// Этот метод является потокобезопасным.
    /// </summary>
    /// <param name="form">Форма, для которой нужно найти провайдер. Не может быть null</param>
    /// <returns>Найденный провайдер</returns>
    public static EFPFormProvider FindFormProviderRequired(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      EFPFormProvider res = FindFormProvider(form);
      if (res == null)
      {
        Exception e = new NullReferenceException(String.Format(Res.EFPFormProvider_Err_FormProviderNotFound, form.ToString()));
        e.Data["FormClass"] = form.GetType().ToString();
        e.Data["AvailableFormProviders"] = DataTools.ToStringArray<EFPFormProvider>(_ProviderList.ToArray());
        throw e;
      }
      return res;
    }

    /// <summary>
    /// Возвращает полный список созданных объектов <see cref="EFPFormProvider"/>
    /// </summary>
    /// <returns>Массив объектов</returns>
    public static EFPFormProvider[] GetAllFormProviders()
    {
      return _ProviderList.ToArray();
    }

    #endregion

    #region Отладочные свойства

#if DEBUG

    /// <summary>
    /// Время создания объекта (для отладки)
    /// </summary>
    public DateTime DebugCreateTime { get { return _DebugCreateTime; } }
    private DateTime _DebugCreateTime;

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public ToolTip DebugTheToolTip { get { return _TheToolTip; } set { } }

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public Control[] DebugToolTipControls
    {
      get
      {
        Control[] a = new Control[_ToolTipTitles.Keys.Count];
        _ToolTipTitles.Keys.CopyTo(a, 0);
        return a;
      }
      set
      {
      }
    }

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public string[] DebugToolTipTitles
    {
      get
      {
        Control[] dc = DebugToolTipControls;
        string[] a = new string[dc.Length];
        for (int i = 0; i < dc.Length; i++)
          a[i] = _ToolTipTitles[dc[i]];
        return a;
      }
      set
      {
      }
    }

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public string[] DebugToolTipStrings
    {
      get
      {
        Control[] dc = DebugToolTipControls;
        string[] a = new string[dc.Length];
        for (int i = 0; i < dc.Length; i++)
          a[i] = _TheToolTip.GetToolTip(dc[i]);
        return a;
      }
      set
      {
      }
    }

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public ISite DebugTheToolTipSite { get { return _TheToolTip.Site; } set { } }

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public IContainer DebugTheToolTipContainer { get { return _TheToolTip.Container; } set { } }

#endif

    #endregion
  }

  /// <summary>
  /// Обработчик обновления формы по времени.
  /// Вызывает пользовательское событие с заданным интервалом времени для обновления формы.
  /// Событие не вызывается, если форма свернута или закрыта другими окнами.
  /// Пользовательский код может создать объект класса <see cref="EFPUpdateByTimeHandler"/> или производного от него и присоединить к
  /// списку <see cref="EFPFormProvider.UpdateByTimeHandlers"/>.
  /// Также обработчик можно присоединить к провайдеру любого управляющего элемента, используя <see cref="EFPControlBase.UpdateByTimeHandlers"/>.
  /// </summary>
  public class EFPUpdateByTimeHandler
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект без присоединенного обработчика события.
    /// Обновление будет вызываться 1 раз в секунду.
    /// </summary>
    public EFPUpdateByTimeHandler()
    {
      _Enabled = true;
    }

    /// <summary>
    /// Создает объет с заданным интервалом обновления и присоединяет к нему пользовательский обработчик события
    /// </summary>
    /// <param name="tick">Пользовательский обработчик</param>
    /// <param name="interval">Период вызова обработчика события <see cref="Tick"/>. См. примечания к свойству <see cref="Interval"/>.</param>
    public EFPUpdateByTimeHandler(EventHandler tick, int interval)
      : this()
    {
      if (tick != null)
        this.Tick += tick;
      this.Interval = interval;
    }

    /// <summary>
    /// Создает объет с интервалом обновления 1 секунда и присоединяет к нему пользовательский обработчик события
    /// </summary>
    /// <param name="tick">Пользовательский обработчик</param>
    public EFPUpdateByTimeHandler(EventHandler tick)
      : this()
    {
      if (tick != null)
        this.Tick += tick;
    }

    #endregion

    #region Обновление по таймеру

    /// <summary>
    /// Обработчик для обновления формы по таймеру.
    /// Вызывается с интервалом, заданным свойством <see cref="Interval"/>.
    /// Ошибки перехватываются. При первой ошибке выдается окно <see cref="EFPApp.ShowException(Exception, string)"/>, далее выдается сообщение
    /// в статусной строке.
    /// Предотвращается вложенный вызов события.
    /// Событие может не вызываться, если окно свернуто или закрыто другими окнами.
    /// </summary>
    public event EventHandler Tick;

    /// <summary>
    /// Вызывает обработчик события <see cref="Tick"/>.
    /// </summary>
    /// <param name="args">Фиктивный объект. Передается обработчику события</param>
    protected virtual void OnTick(EventArgs args)
    {
      if (Tick != null)
        Tick(this, args);
    }

    /// <summary>
    /// Вызывает событие <see cref="Tick"/>, не взирая, прошло ли требуемое время.
    /// Исключения перехватываются.
    /// Свойство <see cref="Enabled"/> не учитывается. Событие вызывается, даже если обновление по таймеру отключено.
    /// </summary>
    public void CallTick()
    {
      if (Tick == null)
        return;

      // Убрано 04.09.2019
      // if (!Enabled)
      //  return;

      _Counter = 0;

      try
      {
        OnTick(EventArgs.Empty);
      }
      catch (Exception e)
      {
        if (_ErrorWasShown)
          EFPApp.ShowTempMessage(String.Format(Res.EFPFormProvider_Err_OnTick, e.Message));
        else
        {
          _ErrorWasShown = true;
          EFPApp.ShowException(e, Res.EFPFormProvider_ErrTitle_OnTick);
        }
      }
    }

    /// <summary>
    /// Внутренний счетчик пропущенных 1-секундных тиков таймера для реализации делителя
    /// </summary>
    private int _Counter;

    private bool _ErrorWasShown;

    /// <summary>
    /// Если сбросить в false, событие <see cref="Tick"/> вызываться не будет.
    /// Позволяет выполнять временное отключение обновления формы.
    /// По умолчанию свойство установлено в true.
    /// Событие <see cref="Tick"/> может быть вызвано при <see cref="Enabled"/>=false, если вызов выполняется программным способом, а не по таймеру.
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        if (value == _Enabled)
          return;
        _Enabled = value;
        OnEnabledChanged(EventArgs.Empty);
        if (value)
          _Counter = 0;
      }
    }
    private bool _Enabled;

    /// <summary>
    /// Событие вызывается после изменения свойства <see cref="Enabled"/>
    /// </summary>
    public event EventHandler EnabledChanged;

    /// <summary>
    /// Вызывает обработчик события <see cref="EnabledChanged"/>
    /// </summary>
    /// <param name="args">Фиктивные аргументы</param>
    protected virtual void OnEnabledChanged(EventArgs args)
    {
      if (EnabledChanged != null)
        EnabledChanged(this, args);
    }

    /// <summary>
    /// Период вызова события <see cref="Tick"/> в .
    /// В текущей реализация периодичность задается с точностью до одной секунды.
    /// Свойство может динамически меняться в процессе работы.
    /// </summary>
    public int Interval
    {
      get { return (_PeriodDivider + 1) * 1000; }
      set
      {
        if (value < 1)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, null);

        int newPeriodDivider = value / 1000 - 1;
        if (newPeriodDivider < 0)
          newPeriodDivider = 0;
        if (newPeriodDivider == _PeriodDivider)
          return;
        _PeriodDivider = newPeriodDivider;
        OnIntervalChanged(EventArgs.Empty);
      }
    }
    private int _PeriodDivider;

    /// <summary>
    /// Событие вызывается при изменении значения свойства <see cref="Interval"/>
    /// </summary>
    public event EventHandler IntervalChanged;

    /// <summary>
    /// Вызывает обработчик события <see cref="IntervalChanged"/>
    /// </summary>
    /// <param name="args">Фиктивные аргументы</param>
    protected virtual void OnIntervalChanged(EventArgs args)
    {
      if (IntervalChanged != null)
        IntervalChanged(this, args);
    }

    internal void TimerTick()
    {
      _Counter++;
      if (Enabled /*04.09.2019 */ && _Counter > _PeriodDivider)
      {
        //System.Console.Beep(1000, 200);
        CallTick();
      }
    }

    #endregion
  }
}
