// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.UICore;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Обработчик для нескольких объектов RadioButton, которые образуют группу радиокнопок.
  /// Может также использоваться для элемента RadioGroupBox.
  /// </summary>
  public class EFPRadioButtons : EFPControlBase, IEnumerable<EFPSingleRadioButton>, IDepSyncObject, IEFPListControl
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер для явно заданного списка управляющих элементов - отдельных кнопок.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="controls">Массив кнопок. Не может быть пустым или содержать значения null</param>
    public EFPRadioButtons(EFPBaseProvider baseProvider, RadioButton[] controls)
      : base(baseProvider, controls[0], true)
    {
      _Controls = controls;
      Init();
    }

    /// <summary>
    /// Создает провайдер для группы кнопок.
    /// Эта версия конструктора принимает первую кнопку в группе и находит остальные кнопки с помощью 
    /// вызова Control.GetNextControl().
    /// Для корректной работы необходимо, чтобы <paramref name="firstControl"/> задавал именно первую кнопку в группе.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="firstControl">Первая кнопка в группе</param>
    public EFPRadioButtons(EFPBaseProvider baseProvider, RadioButton firstControl)
      : base(baseProvider, firstControl, true)
    {
      // Ищем все кнопки в группе
      List<RadioButton> items = new List<RadioButton>();
      Control thisControl = firstControl;
      while (thisControl != null)
      {
        if (thisControl is RadioButton)
          items.Add((RadioButton)thisControl);
        else
          break;
        thisControl = thisControl.Parent.GetNextControl(thisControl, true);
      }
      _Controls = items.ToArray();

      Init();
    }

    /// <summary>
    /// Создает провайдер для элемента RadioGroupBox.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPRadioButtons(EFPBaseProvider baseProvider, RadioGroupBox control)
      : base(baseProvider, control, true)
    {
      _Controls = control.Buttons;
      Init();
      if (control.ImageList == null)
        control.ImageList = EFPApp.MainImages.ImageList;
    }


    private void Init()
    {
      _CanBeEmptyMode = UIValidateState.Error;
      _AllowDisabledSelectedIndex = false;

      //_Enabled = true;
      _SavedSelectedIndex = -1;
      _CurrentSelectedIndex = -1;
      _DisabledSelectedIndex = -1;
      _Codes = null;
      _UnselectedCode = String.Empty;

      _SyncGroup = null;
      _SyncMaster = false;
      _SyncValueType = EFPListControlSyncValueType.SelectedIndex;

      _Buttons = new EFPSingleRadioButton[_Controls.Length];
      for (int i = 0; i < _Controls.Length; i++)
      {
        _Buttons[i] = new EFPSingleRadioButton(this, _Controls[i]);
        if (_Controls[i].Checked)
        {
          _SavedSelectedIndex = i;
          _CurrentSelectedIndex = i;
        }
      }

      //FVisible = Controls[0].Visible;
      //FEnabled = Controls[0].Enabled;
      _Visible = true; // 23.07.2019
      _Enabled = true; // 23.07.2019

      // Устанавливаем обработчики на родительский элемент
      if (!DesignMode)
      {
        Controls[0].Parent.VisibleChanged += new EventHandler(ControlVisibleChanged);
        Controls[0].Parent.EnabledChanged += new EventHandler(ControlEnabledChanged);
      }

      // Сразу выполняем проверку
      Validate();

      _DoubleClickAsOk = true;
    }

    #endregion

    #region Управляющие элементы, составляющие группу

    /// <summary>
    /// Массив объектов RadioButton
    /// </summary>
    public RadioButton[] Controls { get { return _Controls; } }
    private readonly RadioButton[] _Controls;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override Control[] GetControls()
    {
      if (base.Control is RadioButton)
        return _Controls;
      else
        return base.GetControls(); // RadioGroupBox
    }

#if  XXXX
    /// <summary>
    /// Установка свойства EFPControlBaseOld.Control
    /// </summary>
    private void InitBaseControl()
    {
      if (FControls == null)
        base.Control = null;
      else
      {
        if (SelectedIndex >= 0)
          base.Control = FControls[SelectedIndex];
        else
          base.Control = FControls[0];
      }
    }
#endif

    /// <summary>
    /// Доступ к отдельным подобъектам радиокнопок по индексу
    /// </summary>
    /// <param name="index">Индекс кнопки в диапазоне от 0 до (Count-1)</param>
    /// <returns>Объект, управляющий отдельной радиокнопкой</returns>
    public EFPSingleRadioButton this[int index]
    {
      get { return _Buttons[index]; }
    }
    private EFPSingleRadioButton[] _Buttons;

    /// <summary>
    /// Возвращает количество кнопок в группе.
    /// </summary>
    public int Count { get { return _Buttons.Length; } }

    /// <summary>
    /// Возвращает индекс кнопки.
    /// </summary>
    /// <param name="item">Провайдер кнопки</param>
    /// <returns>Индекс кнопки или (-1), если провайдер кнопки не найден</returns>
    public int IndexOf(EFPSingleRadioButton item)
    {
      return Array.IndexOf<EFPSingleRadioButton>(_Buttons, item);
    }

    /// <summary>
    /// Возвращает true, если кнопка есть в списке
    /// </summary>
    /// <param name="item">Объект кнопки</param>
    /// <returns></returns>
    public bool Contains(EFPSingleRadioButton item)
    {
      return IndexOf(item) >= 0;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализирует цвета каждой кнопки в группе.
    /// </summary>
    protected override void InitControlColors()
    {
      for (int i = 0; i < _Controls.Length; i++)
      {
        switch (ValidateState)
        {
          case UIValidateState.Error:
            _Controls[i].ForeColor = EFPApp.Colors.LabelErrorForeColor;
            break;
          case UIValidateState.Warning:
            _Controls[i].ForeColor = EFPApp.Colors.LabelWarningForeColor;
            break;
          default:
            if (EnvironmentTools.IsMono)
              _Controls[i].ForeColor = SystemColors.ControlText; // 01.10.2013
            else
              _Controls[i].ResetForeColor();
            break;
        }
      }
    }

    /// <summary>
    /// Проверка корректности.
    /// Выдает сообщение об ошибке, если нет выбранной кнопки в группе, а CanBeEmpty=false.
    /// Также выдает сообщение, если выбрана заблокированная кнопка в группе, а DisabledItemCanBeSelected=false.
    /// </summary>
    protected override void OnValidate()
    {
      //base.OnValidate();

      if (SelectedIndex < 0)
        ValidateCanBeEmptyMode(CanBeEmptyMode);
      else
      {
        if ((!DisabledItemCanBeSelected) && (!this[SelectedIndex].Enabled))
          SetError(Res.EFPRadioButtons_Err_DisabledSelected);
      }
    }

    /// <summary>
    /// Инициализация всплывающих подсказок
    /// </summary>
    protected override void InitToolTips()
    {
      // Базовый метод вызывать не надо.

      for (int i = 0; i < Controls.Length; i++)
        InitToolTips(Controls[i]);
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    protected override void InitToolTipNestedControls()
    {
      // ничего не добавляем
    }

    /// <summary>
    /// Инициализация "серого" значения
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledSelectedIndex && EnabledState)
        _HasSavedSelectedIndex = true;
      InitControlSelectedIndex();
    }

    #endregion

    #region Свойства Visible и Enabled

    /// <summary>
    /// Видимость кнопок.
    /// Обычно требуется управлять видимостью отдельных кнопок с помощью EFPSingleRadioButton.Visible.
    /// </summary>
    protected override bool ControlVisible
    {
      get { return _Visible && Controls[0].Parent.Visible; }
      set
      {
        if (value == _Visible)
          return;
        _Visible = value;
        for (int i = 0; i < _Buttons.Length; i++)
          _Buttons[i].GroupSetVisible();

      }
    }
    private bool _Visible;

    /// <summary>
    /// Доступность кнопок.
    /// Обычно требуется управлять доступностью отдельных кнопок с помощью EFPSingleRadioButton.Enabled.
    /// </summary>
    protected override bool ControlEnabled
    {
      get { return _Enabled && Controls[0].Parent.Enabled; }
      set
      {
        if (value == _Enabled)
          return;
        _Enabled = value;
        for (int i = 0; i < _Buttons.Length; i++)
          _Buttons[i].GroupSetEnabled();

      }
    }
    private bool _Enabled;

    /// <summary>
    /// Получение и установка свойств <see cref="EFPSingleRadioButton.Enabled"/> с помощью массива логических значений.
    /// Массив должен иметь длину, совпадающую с количеством кнопок
    /// </summary>
    public bool[] EnabledFlags
    {
      get
      {
        bool[] a = new bool[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].Enabled;
        return a;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value.Length != Count)
          throw ExceptionFactory.ArgWrongCollectionCount("value", value, Count);
        for (int i = 0; i < value.Length; i++)
          this[i].Enabled = value[i];
      }
    }

    #endregion

    #region Свойство SelectedIndex

    /// <summary>
    /// Доступ к SelectedOndex.ValueEx без принудительного создания объекта
    /// По умолчанию (-1)(ни одна кнопка не выбрана)
    /// </summary>
    public int SelectedIndex
    {
      get { return _CurrentSelectedIndex; }
      set
      {
        _SavedSelectedIndex = value;
        _HasSavedSelectedIndex = true;
        InitControlSelectedIndex();
      }
    }

    /// <summary>
    /// Текущая выбранная позиция
    /// </summary>
    private int _CurrentSelectedIndex;

    /// <summary>
    /// Сохраненная позиция (на время Disabled)
    /// </summary>
    private int _SavedSelectedIndex;
    private bool _HasSavedSelectedIndex;

    private void InitControlSelectedIndex()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideSelectedIndexChanged)
      //  return;
      int value2;
      if (AllowDisabledSelectedIndex && (!EnabledState))
        value2 = DisabledSelectedIndex;
      else if (_HasSavedSelectedIndex)
      {
        _HasSavedSelectedIndex = false;
        value2 = _SavedSelectedIndex;
      }
      else
        return;

      for (int i = 0; i < _Controls.Length; i++)
        _Controls[i].Checked = (i == value2);
    }

    private bool _InsideSelectedIndexChanged;

    /// <summary>
    /// Установка выбранной позиции при смене свойства RadioButton.CheckedChanged
    /// </summary>
    /// <param name="value">Индекс кнопки</param>
    internal void Control_SelectedIndexChanged(int value)
    {
      if (_InsideSelectedIndexChanged)
        return;
      _InsideSelectedIndexChanged = true;
      try
      {
        OnSelectedIndexChanged(value);
      }
      finally
      {
        _InsideSelectedIndexChanged = false;
      }
    }

    /// <summary>
    /// Метод вызывается при изменении выбранной позиции в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnSelectedIndexChanged(int value)
    {
      _CurrentSelectedIndex = value;

      if (_SelectedIndexEx != null)
        _SelectedIndexEx.Value = SelectedIndex;
      if (_SelectedCodeEx != null)
        _SelectedCodeEx.Value = SelectedCode;

      if (AllowDisabledSelectedIndex && EnabledState)
        _DisabledSelectedIndex = SelectedIndex;

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Выбранная позиция в группе. Значение (-1) (значение по умолчанию) означает, 
    /// что ни одна позиция не выбрана
    /// </summary>
    public DepValue<int> SelectedIndexEx
    {
      get
      {
        InitSelectedIndexEx();
        return _SelectedIndexEx;
      }
      set
      {
        InitSelectedIndexEx();
        _SelectedIndexEx.Source = value;
      }
    }
    private DepInput<int> _SelectedIndexEx;

    private void InitSelectedIndexEx()
    {
      if (_SelectedIndexEx == null)
      {
        _SelectedIndexEx = new DepInput<int>(SelectedIndex,SelectedIndexEx_ValueChanged);
        _SelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "SelectedIndexEx");
      }
    }

    void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedIndex = _SelectedIndexEx.Value;
    }

    /// <summary>
    /// Объект содержит true, если есть выбранное значение (SelectedIndex>=0)
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, int>(SelectedIndexEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(int selectedIndex)
    {
      return selectedIndex >= 0;
    }

    #endregion

    #region Свойство DisabledSelectedIndex

    /// <summary>
    /// Доступ к DisabledSelectedIndex.ValueEx без принудительного создания объекта
    /// По умолчанию - (-1)
    /// </summary>
    public int DisabledSelectedIndex
    {
      get { return _DisabledSelectedIndex; }
      set
      {
        if (value == _DisabledSelectedIndex)
          return;
        _DisabledSelectedIndex = value;
        if (_DisabledSelectedIndexEx != null)
          _DisabledSelectedIndexEx.Value = value;
        InitControlSelectedIndex();
      }
    }
    private int _DisabledSelectedIndex;

    /// <summary>
    /// Это значение замещает свойство SelectedIndex, когда Enabled=false
    /// Свойство действует при установленном свойстве AllowDisabledSelectedIndex
    /// По умолчанию DisabledSelectedIndex.ValueEx=-1 (нет выбранной кнопки)
    /// </summary>
    public DepValue<int> DisabledSelectedIndexEx
    {
      get
      {
        InitDisabledSelectedIndexEx();
        return _DisabledSelectedIndexEx;
      }
      set
      {
        InitDisabledSelectedIndexEx();
        _DisabledSelectedIndexEx.Source = value;
      }
    }

    private void InitDisabledSelectedIndexEx()
    {
      if (_DisabledSelectedIndexEx == null)
      {
        _DisabledSelectedIndexEx = new DepInput<int>(DisabledSelectedIndex, DisabledSelectedIndexEx_ValueChanged);
        _DisabledSelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "DisabledSelectedIndexEx");
      }
    }
    private DepInput<int> _DisabledSelectedIndexEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledText
    /// </summary>
    private void DisabledSelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledSelectedIndex = _DisabledSelectedIndexEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledSelectedIndex
    /// </summary>
    public bool AllowDisabledSelectedIndex
    {
      get { return _AllowDisabledSelectedIndex; }
      set
      {
        if (value == _AllowDisabledSelectedIndex)
          return;
        _AllowDisabledSelectedIndex = value;
        InitControlSelectedIndex();
      }
    }
    private bool _AllowDisabledSelectedIndex;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, если ли элемент может содержать пустое значение.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Свойство DisabledItemCanBeSelected

    /// <summary>
    /// Если False, то при проверке состояния считается ошибкой, если свойство
    /// <see cref="SelectedIndex"/> указывает на элемент, свойство которого <see cref="EFPSingleRadioButton.Enabled"/> 
    /// установлено в false.
    /// По умолчанию - False (считается ошибкой)
    /// </summary>
    public bool DisabledItemCanBeSelected
    {
      get { return _DisabledItemCanBeSelected; }
      set
      {
        if (value == _DisabledItemCanBeSelected)
          return;
        _DisabledItemCanBeSelected = value;
        if (_DisabledItemCanBeSelectedEx != null)
          _DisabledItemCanBeSelectedEx.Value = value;
        Validate();
      }
    }
    private bool _DisabledItemCanBeSelected;

    /// <summary>
    /// Если False, то при проверке состояния считается ошибкой, если свойство
    /// SelectedIndex указывает на элемент, свойство которого this[SelectedIndexEx].Enabled
    /// установлено в false.
    /// По умолчанию - False (считается ошибкой)
    /// </summary>
    public DepValue<bool> DisabledItemCanBeSelectedEx
    {
      get
      {
        InitDisabledItemCanBeSelectedEx();
        return _DisabledItemCanBeSelectedEx;
      }
      set
      {
        InitDisabledItemCanBeSelectedEx();
        _DisabledItemCanBeSelectedEx.Source = value;
      }
    }

    private void InitDisabledItemCanBeSelectedEx()
    {
      if (_DisabledItemCanBeSelectedEx == null)
      {
        _DisabledItemCanBeSelectedEx = new DepInput<bool>(DisabledItemCanBeSelected,DisabledItemCanBeSelectedEx_ValueChanged);
        _DisabledItemCanBeSelectedEx.OwnerInfo = new DepOwnerInfo(this, "DisabledItemCanBeSelectedEx");
      }
    }
    private DepInput<Boolean> _DisabledItemCanBeSelectedEx;

    void DisabledItemCanBeSelectedEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledItemCanBeSelected = _DisabledItemCanBeSelectedEx.Value;
    }

    #endregion

    #region Подстановочные значения

    /// <summary>
    /// Необязательный список подстановочных значений, соответствующих элементам списка
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
#if DEBUG
        if (value != null)
        {
          if (value.Length != _Controls.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, Count);
          foreach (string s in value)
          {
            if (s == null)
              throw ExceptionFactory.ArgInvalidEnumerableItem("value", value, null);
          }
        }
#endif
        _Codes = value;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.OwnerSetValue(SelectedCode);
      }
    }
    private string[] _Codes;

    /// <summary>
    /// Подстановочное значение для <see cref="SelectedCode"/> при <see cref="SelectedIndex"/> = (-1).
    /// Если <see cref="UnselectedCode"/> совпадает с одним из значений массива <see cref="Codes"/>, то при
    /// установке <see cref="SelectedCode"/>=<see cref="UnselectedCode"/> свойство <see cref="SelectedIndex"/> 
    /// примет значение IndexOf(Codes), а не (-1).
    /// </summary>
    public string UnselectedCode
    {
      get { return _UnselectedCode; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _UnselectedCode)
          return;
        _UnselectedCode = value;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.OwnerSetValue(SelectedCode);
      }
    }
    private string _UnselectedCode;

    /// <summary>
    /// Текущая выбранная позиция в виде кода из массива <see cref="Codes"/>. Если нет выбранной
    /// кнопки (<see cref="SelectedIndex"/>=(-1)), то принимает значение <see cref="UnselectedCode"/>.
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (Codes == null || SelectedIndex < 0)
          return UnselectedCode;
        if (SelectedIndex >= Codes.Length)
          return UnselectedCode;
        return Codes[SelectedIndex];
      }
      set
      {
        if (_InsideSelectedIndexChanged)
          return;
        if (Codes == null)
          return;
        if (value == null)
          value = String.Empty;
        SelectedIndex = Array.IndexOf<string>(Codes, value);
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="SelectedCode"/>.
    /// </summary>
    public DepValue<string> SelectedCodeEx
    {
      get
      {
        InitSelectedCodeEx();
        return _SelectedCodeEx;
      }
      set
      {
        InitSelectedCodeEx();
        _SelectedCodeEx.Source = value;
      }
    }

    private void InitSelectedCodeEx()
    {
      if (_SelectedCodeEx == null)
      {
        _SelectedCodeEx = new DepInput<string>(SelectedCode, SelectedCodeEx_ValueChanged);
        _SelectedCodeEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodeEx");
      }
    }

    private DepInput<string> _SelectedCodeEx;

    /// <summary>
    /// Вызывается, когда снаружи устанавливается значение SelectedCodeEx.ValueEx
    /// </summary>
    void SelectedCodeEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCode = _SelectedCodeEx.Value;
    }

    /// <summary>
    /// Возвращает провайдер кнопки по коду.
    /// </summary>
    /// <param name="code">Код кнопки в массиве <see cref="Codes"/>.</param>
    /// <returns>Найденная кнопка или null</returns>
    public EFPSingleRadioButton FindByCode(string code)
    {
      if (_Codes == null)
        return null;
      if (String.IsNullOrEmpty(code))
        return null;
      int p = Array.IndexOf<string>(_Codes, code);
      if (p >= 0)
        return _Buttons[p];
      else
        return null;
    }


    #endregion

    #region Управление синхронизацией

    /// <summary>
    /// Какое свойство будет использовано для синхронизации в SyncGroup
    /// (по умолчанию - SelectedIndexEx)
    /// Свойство должно устанавливаться жо добавления объекта в список синхронизации
    /// </summary>
    public EFPListControlSyncValueType SyncValueType
    {
      get { return _SyncValueType; }
      set
      {
        if (SyncGroup != null)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "SyncGroup");
        _SyncValueType = value;
      }
    }
    private EFPListControlSyncValueType _SyncValueType;

    #endregion

    #region IEFPSyncObject Members

    // Класс EFPRadioButtons не может быть выведен из EFPSyncControlOld, поэтому методы
    // интерфейса IEFPSyncObject должны быть реализованы полностью

    /// <summary>
    /// Синхронизируемое значение.
    /// Используется свойство SelectedIndex или SelectedCode, в зависимости от SyncValueType.
    /// </summary>
    public object SyncValue
    {
      get
      {
        if (SyncValueType == EFPListControlSyncValueType.SelectedIndex)
          return SelectedIndex;
        else
          return SelectedCode;
      }
      set
      {
        if (SyncValueType == EFPListControlSyncValueType.SelectedIndex)
          SelectedIndex = (int)value;
        else
          SelectedCode = (string)value;
      }
    }

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
    /// Вызывается в обработчиках изменения редактируемого значения для передачи
    /// значения в группу синхронизации в режиме SyncMaster=true
    /// </summary>
    protected void DoSyncValueChanged()
    {
      if (SyncMaster && (SyncGroup != null))
      {
        object value = ((IDepSyncObject)this).SyncValue;
        SyncGroup.Value = value;
      }
    }

    #endregion

    #region Активация первой или последней кнопки

    /// <summary>
    /// Возвращает индекс первой незаблокированной кнопки.
    /// Возвращает (-1), если все кнопки заблокированы
    /// </summary>
    public int FirstEnabledIndex
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].Enabled)
            return i;
        }
        return -1;
      }
    }

    /// <summary>
    /// Возвращает индекс последней незаблокированной кнопки.
    /// Возвращает (-1), если все кнопки заблокированы
    /// </summary>
    public int LastEnabledIndex
    {
      get
      {
        for (int i = Count - 1; i >= 0; i--)
        {
          if (this[i].Enabled)
            return i;
        }
        return -1;
      }
    }

    /// <summary>
    /// Если нет выбранной радиокнопки или выбранная радиокнопка является заблокированной, 
    /// то активируется первая доступная кнопка (вызов SelectFirst())
    /// </summary>
    public void SelectFirstIfNeeded()
    {
      if (SelectedIndex < 0)
        SelectFirst();
      else
      {
        if (!this[SelectedIndex].Enabled)
          SelectFirst();
      }
    }

    /// <summary>
    /// Если нет выбранной радиокнопки или выбранная радиокнопка является заблокированной, 
    /// то активируется последняя доступная кнопка (вызов SelectLast())
    /// </summary>
    public void SelectLastIfNeeded()
    {
      if (SelectedIndex < 0)
        SelectLast();
      else
      {
        if (!this[SelectedIndex].Enabled)
          SelectLast();
      }
    }

    /// <summary>
    /// Если нет выбранной радиокнопки или выбранная радиокнопка является заблокированной, 
    /// то активируется следующая доступная кнопка (вызов SelectNext())
    /// </summary>
    public void SelectNextIfNeeded()
    {
      if (SelectedIndex < 0)
        SelectFirst();
      else
      {
        if (!this[SelectedIndex].Enabled)
          SelectNext();
      }
    }

    /// <summary>
    /// Если нет выбранной радиокнопки или выбранная радиокнопка является заблокированной, 
    /// то активируется предыдущая доступная кнопка (вызов SelectPrev())
    /// </summary>
    public void SelectPrevIfNeeded()
    {
      // исправлено 27.12.2020

      if (SelectedIndex < 0)
        SelectLast();
      else
      {
        if (!this[SelectedIndex].Enabled)
          SelectPrev();
      }
    }

    /// <summary>
    /// Активирует первую незаблокированную кнопку в группе
    /// </summary>
    public void SelectFirst()
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          break;
        }
      }
    }

    /// <summary>
    /// Активирует последнюю незаблокированную кнопку в группе
    /// </summary>
    public void SelectLast()
    {
      for (int i = Count - 1; i >= 0; i--)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          break;
        }
      }
    }

    /// <summary>
    /// Активирует следующую кнопку, у которой свойство EFPSingleRadioButton.Enabled=true.
    /// Если текущая кнопка является последней в группе или после нее нет доступных кнопок,
    /// поиск выполняется с начала группы.
    /// </summary>
    public void SelectNext()
    {
      if (SelectedIndex < 0)
      {
        SelectFirst();
        return;
      }

      for (int i = SelectedIndex + 1; i < Count; i++)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          return;
        }
      }

      for (int i = 0; i < SelectedIndex; i++)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          return;
        }
      }
    }

    /// <summary>
    /// Активирует предыдущую кнопку, у которой свойство EFPSingleRadioButton.Enabled=true.
    /// Если текущая кнопка является первой в группе или перед ней нет доступных кнопок,
    /// поиск выполняется с конца группы.
    /// </summary>
    public void SelectPrev()
    {
      if (SelectedIndex < 0)
      {
        SelectLast();
        return;
      }

      for (int i = SelectedIndex - 1; i >= 0; i--)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          return;
        }
      }

      for (int i = Count - 1; i > SelectedIndex; i--)
      {
        if (this[i].Enabled)
        {
          SelectedIndex = i;
          return;
        }
      }
    }

    #endregion

    #region Свойство DoubleClickAsOk

    /// <summary>
    /// Если true (значение по умолчанию), то двойной щелчок мыши на любой кнопке
    /// приводит к эмуляции нажатия кнопки по умолчанию в блоке диалога
    /// </summary>
    public bool DoubleClickAsOk
    {
      get { return _DoubleClickAsOk; }
      set { _DoubleClickAsOk = value; }
    }
    private bool _DoubleClickAsOk;

    #endregion

    #region Изображения

    // некрасиво
    //public void SetImages(string[] ImageKeys)
    //{
    //  int n = Math.Min(ImageKeys.Length, Controls.Length);
    //  for (int i = 0; i < n; i++)
    //  {
    //    Controls[i].ImageList = EFPApp.MainImages;
    //    //Controls[i].ImageAlign = ContentAlignment.MiddleLeft;
    //    Controls[i].TextImageRelation = TextImageRelation.ImageBeforeText;
    //    Controls[i].ImageKey = ImageKeys[i];
    //    //Controls[i].AutoSize = true;
    //  }
    //}

    #endregion

    #region IEnumerable<EFPSingleRadioButton> Members

    /// <summary>
    /// Возвращает перечислитель по провайдерам отдельных кнопок.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<EFPSingleRadioButton> GetEnumerator()
    {
      return ((IEnumerable<EFPSingleRadioButton>)_Buttons).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Buttons.GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для одной кнопки в группе. Реализация свойства EFPRadioButtons.this
  /// </summary>
  public class EFPSingleRadioButton
  {
    #region Конструктор

    internal EFPSingleRadioButton(EFPRadioButtons group, RadioButton control)
    {
      _Group = group;
      _Control = control;
      _Enabled = true;
      if (!group.DesignMode)
      {
        _Control.CheckedChanged += new EventHandler(Control_CheckedChanged);
        _Control.EnabledChanged += new EventHandler(Control_EnabledChanged);

        // Нельзя использовать событие MouseDoubleClick, т.к. оно никогда не вызывается для радиокнопок
        ControlMouseClickHandler ClickHandler = new ControlMouseClickHandler(control);
        ClickHandler.MouseDoubleClick += new MouseEventHandler(ClickHandler_MouseDoubleClick);
      }
    }

    #endregion

    #region Простые свойства

    /// <summary>
    /// Управляющий элемент кнопки Windows Forms
    /// </summary>
    public RadioButton Control { get { return _Control; } }
    private readonly RadioButton _Control;

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPRadioButtons Group { get { return _Group; } }
    private readonly EFPRadioButtons _Group;

    #endregion

    #region Свойство Enabled

    /// <summary>
    /// Доступность кнопки. Возвращает true, если кнопка доступна и группа в-целом
    /// тоже доступна. Установка свойства может не отображаться сразу, если
    /// EFPRadioButtons.Enabled=false.
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        if (value == _Enabled)
          return;
        _Enabled = value;
        if (_Group.Enabled)
          Control.Enabled = value;
        if (_EnabledEx != null)
          _EnabledEx.OwnerSetValue(Enabled);
        if (_Group.Enabled)
          Group.Validate();
      }
    }
    private bool _Enabled;

    /// <summary>
    /// Управляемое свойство для <see cref="Enabled"/>.
    /// </summary>
    public DepValue<bool> EnabledEx
    {
      get
      {
        InitEnabledEx();
        return _EnabledEx;
      }
      set
      {
        InitEnabledEx();
        _EnabledEx.Source = value;
      }
    }

    private void InitEnabledEx()
    {
      if (_EnabledEx == null)
      {
        _EnabledEx = new DepInput<bool>(Enabled, EnabledEx_ValueChanged);
        _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");
      }
    }

    private void EnabledEx_ValueChanged(object sender, EventArgs args)
    {
      Enabled = _EnabledEx.Value;
    }

    DepInput<Boolean> _EnabledEx;

    void Control_EnabledChanged(object sender, EventArgs args)
    {
      try
      {
        if (_EnabledEx != null)
          _EnabledEx.OwnerSetValue(Enabled);
        _Group.Validate();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    internal void GroupSetVisible()
    {
      Control.Visible = _Group.Visible;
    }

    internal void GroupSetEnabled()
    {
      if (_Group.Enabled)
        Control.Enabled = _Enabled;
      else
        Control.Enabled = false;
      if (_EnabledEx != null)
        _EnabledEx.OwnerSetValue(Enabled);
    }

    #endregion

    #region Свойство Checked

    /// <summary>
    /// Доступ к CheckedEx.ValueEx без принудительного создания объекта
    /// По умолчанию - false
    /// </summary>
    public bool Checked { get { return Control.Checked; } }

    /// <summary>
    /// Возвращает true, если у кнопки есть точка.
    /// Свойство не может быть установлено. Для установки используйте EFPRadioButtons.SelectedIndexEx
    /// </summary>
    public DepValue<Boolean> CheckedEx
    {
      get
      {
        if (_CheckedEx == null)
        {
          _CheckedEx = new DepOutput<bool>(Checked);
          _CheckedEx.OwnerInfo = new DepOwnerInfo(this, "CheckedEx");
        }
        return _CheckedEx;
      }
    }

    private DepOutput<Boolean> _CheckedEx;

    private void Control_CheckedChanged(object sender, EventArgs args)
    {
      try
      {
        if (_CheckedEx != null)
          _CheckedEx.OwnerSetValue(Control.Checked);
        if (Control.Checked)
        {
          int p = Group.IndexOf(this);
          Group.Control_SelectedIndexChanged(p);
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region События мыши

    void ClickHandler_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      if (args.Button == MouseButtons.Left && _Group.DoubleClickAsOk)
      {
        if (Control.FindForm().AcceptButton != null)
          Control.FindForm().AcceptButton.PerformClick();
      }
    }

    #endregion
  }
}
