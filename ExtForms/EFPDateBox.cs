using AgeyevAV.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Провайдер поля ввода даты.
  /// Время не используется.
  /// </summary>
  public class EFPDateBox : EFPSyncControl<DateBox>, IEFPReadOnlyControl, IEFPSimpleTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провадер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateBox(EFPBaseProvider baseProvider, DateBox control)
      : base(baseProvider, control, true)
    {
      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);

      if (String.IsNullOrEmpty(control.PopupButtonToolTipText))
        control.PopupButtonToolTipText = "Выбрать из календаря";
      if (String.IsNullOrEmpty(control.ClearButtonToolTipText))
        control.ClearButtonToolTipText = "Очистить дату";

      _SavedValue = control.Value;
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство EnabledEx, как
    /// у других управляющих элементов, а через свойство ReadOnly
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitReadOnlyEx();
      _NotReadOnlySync.Value = value;
    }

    /// <summary>
    /// Проверка корректности значения.
    /// Выдает сообщение об ошибке, если дата не входит в диапазон значений {Minimum, Maximum}.
    /// Если дата не введена, а CanBeEmpty=false, также устанавливается ошибка.
    /// </summary>
    protected override void OnValidate()
    {
      if (Value.HasValue)
      {
        if (!DataTools.DateInRange(Value.Value, Minimum, Maximum))
        {
          if (WarningIfOutOfRange)
            SetWarning("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
          else
            SetError("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
        }
      }
      else
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
        }
        else
          SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
      }

      base.OnValidate();
    }

    /// <summary>
    /// Синронизированное значение.
    /// Свойство Value.
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Value;
      }
      set
      {
        Value = (Nullable<DateTime>)value;
      }
    }

    /// <summary>
    /// Инициализация "серого" значения.
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledValue && EnabledState)
        _HasSavedValue = true;
      InitControlValue();
    }

    /// <summary>
    /// Создает EFPTextBoxCommandItems()
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      if (EFPApp.EasyInterface)
        return base.GetCommandItems();
      else
        return new EFPTextBoxCommandItems(this, false);
    }

    #endregion

    #region Свойство Value

    /// <summary>
    /// Доступ к выбранной дате.
    /// Значение null используется, если дата не введена.
    /// Если пользователь ввел текст, который нельзя преобразовать в дату, также возвращается null.
    /// </summary>
    public DateTime? Value
    {
      get { return Control.Value; }
      set
      {
        if (value.HasValue)
          _SavedValue = value.Value.Date;
        else
          _SavedValue = null;
        _HasSavedValue = true;
        InitControlValue();
      }
    }

    /// <summary>
    /// Сохраняемое значение после установки свойства "Value".
    /// Используется, если в данный момент используется DisabledValue.
    /// </summary>
    private DateTime? _SavedValue;
    private bool _HasSavedValue;

    /// <summary>
    /// Инициализация DateBox.Value.
    /// </summary>
    protected void InitControlValue()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideValueChanged)
      //  return;
      if (AllowDisabledValue && (!EnabledState))
        Control.Value = DisabledValue;
      else if (_HasSavedValue)
      {
        _HasSavedValue = false;
        Control.Value = _SavedValue;
      }
    }

    /// <summary>
    /// Управляемое свойство Value.
    /// </summary>
    public DepValue<DateTime?> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInputWithCheck<DateTime?>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.OwnerSetValue(Value);
        _ValueEx.CheckValue += new DepInputCheckEventHandler<DateTime?>(ValueEx_CheckValue);
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private DepInputWithCheck<DateTime?> _ValueEx;

    private void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            OnValueChanged();
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика DateBox.ValueChanged");
      }
    }

    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
        _ValueEx.OwnerSetValue(Control.Value);

      if (AllowDisabledValue && EnabledState)
        _SavedValue = Value;

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Вызывается, когда изменяется значение "снаружи" элемента
    /// </summary>
    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime?> args)
    {
      if (args.NewValue.HasValue)
        args.NewValue = args.NewValue.Value.Date;
    }
    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }



    /// <summary>
    /// Управляемое свойство, которое содержит true, если есть введенная дата (Value.HasValue=true).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, DateTime?>(ValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(DateTime? value)
    {
      return value.HasValue;
    }

    #endregion

    #region Свойство DisabledValue

    /// <summary>
    /// Значение, которое показывается при Enabled=false и AllowDisabledValue=true
    /// </summary>
    public DateTime? DisabledValue
    {
      get { return _DisabledValue; }
      set
      {
        if (value == _DisabledValue)
          return;
        _DisabledValue = value;
        if (_DisabledValueEx != null)
          _DisabledValueEx.Value = value;
        InitControlValue();
      }
    }
    private DateTime? _DisabledValue;

    /// <summary>
    /// Управляемое свойство DisabledValue.
    /// Свойство действует при установленном свойстве AllowDisabledText
    /// </summary>
    public DepValue<DateTime?> DisabledValueEx
    {
      get
      {
        InitDisabledValueEx();
        return _DisabledValueEx;
      }
      set
      {
        InitDisabledValueEx();
        _DisabledValueEx.Source = value;
      }
    }

    private void InitDisabledValueEx()
    {
      if (_DisabledValueEx == null)
      {
        _DisabledValueEx = new DepInput<DateTime?>();
        _DisabledValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledValueEx");
        _DisabledValueEx.Value = DisabledValue;
        _DisabledValueEx.ValueChanged += new EventHandler(DisabledValueEx_ValueChanged);
      }
    }
    private DepInput<DateTime?> _DisabledValueEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledValue = _DisabledValueEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledValue
    /// </summary>
    public bool AllowDisabledValue
    {
      get { return _AllowDisabledValue; }
      set
      {
        if (value == _AllowDisabledValue)
          return;
        _AllowDisabledValue = value;
        InitControlValue();
      }
    }
    private bool _AllowDisabledValue;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Допускается ли значение null.
    /// Значение совпадает со свойством DateBox.ClearButton, которое по умолчанию имеет значение false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }

    /// <summary>.
    /// Управляемое свойство для CanBeEmpty.
    /// True, если можно выбирать значение null. По умолчанию - false.
    /// Определяет наличие наличие кнопочки
    /// "X" рядом с комбоблоком. При значении false выполняется проверка ошибки
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true).
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Управляемое свойство для WarningIfEmpty.
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Value=null.
    /// По умолчанию - False.
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region Свойство DefaultYear

    /// <summary>
    /// Год по умолчанию
    /// Если задано ненулевое значение и пользователь ввел только 4 цифры дня и месяца,
    /// дата будет считаться корректной и использовать выбранный год
    /// </summary>
    public int DefaultYear
    {
      get { return Control.DefaultYear; }
      set { Control.DefaultYear = value; }
    }

    /// <summary>
    /// Управляемое свойство для DefaultYear
    /// </summary>
    public DepValue<int> DefaultYearEx
    {
      get
      {
        InitDefaultYearEx();
        return _DefaultYearEx;
      }
      set
      {
        InitDefaultYearEx();
        _DefaultYearEx.Source = value;
      }
    }

    private DepInput<int> _DefaultYearEx;

    private void InitDefaultYearEx()
    {
      if (_DefaultYearEx == null)
      {
        _DefaultYearEx = new DepInput<int>();
        _DefaultYearEx.OwnerInfo = new DepOwnerInfo(this, "DefaultYearEx");
        _DefaultYearEx.Value = DefaultYear;
        _DefaultYearEx.ValueChanged += new EventHandler(DefaultYearEx_ValueChanged);
        Control.DefaultYearChanged += new EventHandler(Control_DefaultYearChanged);
      }
    }

    void Control_DefaultYearChanged(object sender, EventArgs args)
    {
      _DefaultYearEx.Value = Control.DefaultYear;
    }

    void DefaultYearEx_ValueChanged(object sender, EventArgs args)
    {
      Control.DefaultYear = DefaultYearEx.Value;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Режим "только чтение"
    /// </summary>
    public bool ReadOnly
    {
      get { return Control.ReadOnly; }
      set
      {
        if (value == Control.ReadOnly)
          return;
        Control.ReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }

    /// <summary>
    /// Управляемое свойство для ReadOnly.
    /// </summary>
    public DepValue<Boolean> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyMain.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>();
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
        _ReadOnlyEx.Value = false;
        _ReadOnlyEx.ValueChanged += new EventHandler(ReadOnlyEx_ValueChanged);

        _ReadOnlyMain = new DepInput<bool>();
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");
        _ReadOnlyMain.Value = false;

        _NotReadOnlySync = new DepInput<bool>();
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");
        _NotReadOnlySync.Value = true;

        DepOr ReadOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = ReadOnlyOr;
      }
    }
    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;
    /// <summary>
    /// Основной вход для ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyMain;
    /// <summary>
    /// Дополнительный вход для ReadOnly для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _NotReadOnlySync;

    private void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Интерфейс IEFPSimpleTextBox

    string IEFPSimpleTextBox.Text
    {
      get { return Control.Text; }
      set { Control.Text = value; }
    }

    int IEFPSimpleTextBox.TextLength { get { return Control.Text.Length; } }

    int IEFPSimpleTextBox.SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    int IEFPSimpleTextBox.SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    string IEFPSimpleTextBox.SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    void IEFPSimpleTextBox.Select(int start, int length)
    {
      Control.Select(start, length);
    }

    void IEFPSimpleTextBox.SelectAll()
    {
      Control.SelectAll();
    }

    #endregion

    #region Диапазон допустимых значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion
  }

  /// <summary>
  /// Проверка правильности интервала дат для двух управляющих элементов
  /// </summary>
  public class EFPDateBoxRangeCheck
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// Далее должны быть установлены свойства FirstDateBox и LastDateBox.
    /// </summary>
    public EFPDateBoxRangeCheck()
    {
      DoInit();
    }

    /// <summary>
    /// Создает объект с заданными свойствами FirstDateBox и LastDateBox.
    /// </summary>
    /// <param name="firstDateBox">Провайдер поля ввода начальной даты диапазона</param>
    /// <param name="lastDateBox">Провайдер поля ввода конечной даты диапазона</param>
    public EFPDateBoxRangeCheck(EFPDateBox firstDateBox, EFPDateBox lastDateBox)
    {
      DoInit();
      this.FirstDateBox = firstDateBox;
      this.LastDateBox = lastDateBox;
    }

    internal EFPDateBoxRangeCheck(EFPDateBox firstDateBox, EFPDateBox lastDateBox, EFPDateRangeBox owner)
    {
      DoInit();
      this.FirstDateBox = firstDateBox;
      this.LastDateBox = lastDateBox;
      _Owner = owner;
    }

    private void DoInit()
    {
      _FirstDateInput = new DepInput<DateTime?>();
      _FirstDateInput.OwnerInfo = new DepOwnerInfo(this, "FirstDateInput");
      _FirstDateInput.ValueChanged += new EventHandler(FirstDateInput_ValueChanged);

      _LastDateInput = new DepInput<DateTime?>();
      _LastDateInput.OwnerInfo = new DepOwnerInfo(this, "LastDateInput");
      _LastDateInput.ValueChanged += new EventHandler(LastDateInput_ValueChanged);

      _EHValidatingFirstDate = new EFPValidatingEventHandler(DoValidate);
      _EHValidatingLastDate = new EFPValidatingEventHandler(DoValidate);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для поля ввода начальной даты
    /// </summary>
    public EFPDateBox FirstDateBox
    {
      get { return _FirstDateBox; }
      set
      {
        if (value == _FirstDateBox)
          return;

        if (_FirstDateBox != null)
          _FirstDateBox.Validating -= _EHValidatingFirstDate;
        _FirstDateBox = value;
        if (_FirstDateBox != null)
          _FirstDateBox.Validating += _EHValidatingFirstDate;

        if (value == null)
          _FirstDateInput.Source = null;
        else
          _FirstDateInput.Source = value.ValueEx;
      }
    }
    private EFPDateBox _FirstDateBox;

    /// <summary>
    /// Провайдер для поля ввода конечной даты
    /// </summary>
    public EFPDateBox LastDateBox
    {
      get { return _LastDateBox; }
      set
      {
        if (value == _LastDateBox)
          return;

        if (_LastDateBox != null)
          _LastDateBox.Validating -= _EHValidatingLastDate;
        _LastDateBox = value;
        if (_LastDateBox != null)
          _LastDateBox.Validating += _EHValidatingLastDate;

        if (value == null)
          _LastDateInput.Source = null;
        else
          _LastDateInput.Source = value.ValueEx;
      }
    }
    private EFPDateBox _LastDateBox;

    private EFPDateRangeBox _Owner;

    #endregion

    #region Внутренняя реализация

    private DepInput<Nullable<DateTime>> _FirstDateInput;
    private DepInput<Nullable<DateTime>> _LastDateInput;

    void FirstDateInput_ValueChanged(object sender, EventArgs args)
    {
      if (LastDateBox != null)
        LastDateBox.Validate();
    }

    void LastDateInput_ValueChanged(object sender, EventArgs args)
    {
      if (FirstDateBox != null)
        FirstDateBox.Validate();
    }


    private EFPValidatingEventHandler _EHValidatingFirstDate;
    private EFPValidatingEventHandler _EHValidatingLastDate;

    private void DoValidate(object sender, EFPValidatingEventArgs args)
    {
      if (_Owner != null)
        _Owner.Validate();

      if (_FirstDateInput.Value.HasValue && _LastDateInput.Value.HasValue)
      {
        if (FirstDateBox.Enabled && LastDateBox.Enabled)
        {
          if (_FirstDateInput.Value.Value > _LastDateInput.Value.Value)
            args.SetError("Неправильный интервал дат. Начальная дата больше конечной");
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для DateRangeBox.
  /// Содержит два "вложенных" провайдера EFPDateBox.
  /// </summary>
  public class EFPDateRangeBox : EFPControl<DateRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateRangeBox(EFPBaseProvider baseProvider, DateRangeBox control)
      : base(new EFPDateRangeBoxBaseProvider(), control, true)
    {
      this.BaseProvider.Parent = baseProvider;
      ((EFPDateRangeBoxBaseProvider)(this.BaseProvider)).ControlProvider = this;

      _FirstDate = new EFPDateBox(this.BaseProvider, control.FirstDate);
      _FirstDate.DisplayName = "Начальная дата";
      _LastDate = new EFPDateBox(this.BaseProvider, control.LastDate);
      _LastDate.DisplayName = "Конечная дата";
      new EFPDateBoxRangeCheck(_FirstDate, _LastDate, this);

      _FirstDate.ValueEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _LastDate.ValueEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _FirstDate.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _LastDate.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);

      if (!DesignMode)
        control.MenuButtonClicked += new System.ComponentModel.CancelEventHandler(Control_MenuButtonClicked);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер поля ввода начальной даты диапазона.
    /// </summary>
    public EFPDateBox FirstDate { get { return _FirstDate; } }
    private EFPDateBox _FirstDate;

    /// <summary>
    /// Провайдер поля ввода конечной даты диапазона.
    /// </summary>
    public EFPDateBox LastDate { get { return _LastDate; } }
    private EFPDateBox _LastDate;

    #endregion

    #region Свойство Formatter

    /// <summary>
    /// Форматировщик диапазона дат.
    /// Если свойство не установлено в явном виде, возвращает DateRangeFormatter.Default
    /// </summary>
    public DateRangeFormatter Formatter
    {
      get { return Control.Formatter; }
      set { Control.Formatter = value; }
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Всплывающая подсказка.
    /// Для элементов задаются подсказки "ToolTipText - начальная дата" и "ToolTipText - конечная дата".
    /// </summary>
    public override string ToolTipText
    {
      get
      {
        return base.ToolTipText;
      }
      set
      {
        base.ToolTipText = value;
        string s = String.Empty;
        if (!String.IsNullOrEmpty(value))
          s = value + " - ";
        FirstDate.ToolTipText = s + "Начальная дата";
        LastDate.ToolTipText = s + "Конечная дата";
      }
    }

    private bool _InsideOnValidate;

    /// <summary>
    /// Проверка корректности.
    /// Вызывает метод проверки для "вложенных" провайдеров.
    /// </summary>
    protected override void OnValidate()
    {
      if (_InsideOnValidate)
        return;
      _InsideOnValidate = true;
      try
      {
        base.OnValidate();
        FirstDate.Validate();
        LastDate.Validate();
      }
      finally
      {
        _InsideOnValidate = false;
      }
    }

    #endregion

    #region Команды меню

    private class EFPDateRangeBoxBaseProvider : EFPBaseProvider
    {
      #region Доступ к меню

      public EFPDateRangeBox ControlProvider;

      public override void InitCommandItemList(List<EFPCommandItems> list)
      {
        if (ControlProvider != null)
        {
          if (ControlProvider.CommandItems.Count > 0)
            list.Add(ControlProvider.CommandItems);
        }
        base.InitCommandItemList(list);
      }

      #endregion
    }

    /// <summary>
    /// Создает EFPDateRangeBoxCommandItems.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDateRangeBoxCommandItems(this);
    }

    void InitCommandItemsState(object sender, EventArgs args)
    {
      EFPDateRangeBoxCommandItems CommandItems2 = CommandItemsIfAssigned as EFPDateRangeBoxCommandItems;
      if (CommandItems2 != null)
        CommandItems2.InitCommandItemsState();
    }

    void Control_MenuButtonClicked(object sender, System.ComponentModel.CancelEventArgs args)
    {
      PrepareContextMenu(); // иначе меню не будет показано
    }


    #endregion
  }

  /// <summary>
  /// Команды меню для EFPDateRangeBox
  /// </summary>
  internal sealed class EFPDateRangeBoxCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    public EFPDateRangeBoxCommandItems(EFPDateRangeBox controlProvider)
    {
      _ControlProvider = controlProvider;

      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = "Сегодня";
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ci1To2 = new EFPCommandItem("Edit", "LastDateEqualFirst");
      ci1To2.MenuText = "Конечная дата равна начальной";
      ci1To2.ShortCut = Keys.Control | Keys.OemCloseBrackets;
      ci1To2.Click += ci1To2_Click;
      ci1To2.GroupBegin = true;
      Add(ci1To2);

      ci2To1 = new EFPCommandItem("Правка", "FirstDateEqualLast");
      ci2To1.MenuText = "Начальная дата равна конечной";
      ci2To1.ShortCut = Keys.Control | Keys.OemOpenBrackets;
      ci2To1.Click += ci2To1_Click;
      ci2To1.GroupEnd = true;
      Add(ci2To1);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = "Месяц";
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = "Квартал";
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = "Год";
      ciYear.ShortCut = Keys.Control | Keys.Y; // Redo все равно недоступна, можно занимать
      ciYear.Click += ciYear_Click;
      Add(ciYear);

      ciNextPeriod = new EFPCommandItem("Edit", "NextPeriod");
      ciNextPeriod.MenuText = controlProvider.Control.TheRightButton.ToolTipText;
      ciNextPeriod.ShortCut = Keys.Control | Keys.OemPeriod;
      ciNextPeriod.ImageKey = "DateRangeBoxRight";
      ciNextPeriod.Click += new EventHandler(controlProvider.Control.TheRightButton_Click);
      ciNextPeriod.GroupBegin = true;
      Add(ciNextPeriod);

      ciPrevPeriod = new EFPCommandItem("Edit", "PrevPeriod");
      ciPrevPeriod.MenuText = controlProvider.Control.TheLeftButton.ToolTipText;
      ciPrevPeriod.ShortCut = Keys.Control | Keys.Oemcomma;
      ciPrevPeriod.ImageKey = "DateRangeBoxLeft";
      ciPrevPeriod.Click += new EventHandler(controlProvider.Control.TheLeftButton_Click);
      Add(ciPrevPeriod);

      ciNextYear = new EFPCommandItem("Edit", "NextYear");
      ciNextYear.MenuText = "Следующий год";
      ciNextYear.ShortCut = Keys.Control | Keys.Shift | Keys.OemPeriod;
      ciNextYear.Click += new EventHandler(ciNextYear_Click);
      Add(ciNextYear);

      ciPrevYear = new EFPCommandItem("Edit", "PrevYear");
      ciPrevYear.MenuText = "Предыдущий год";
      ciPrevYear.ShortCut = Keys.Control | Keys.Shift | Keys.Oemcomma;
      ciPrevYear.Click += new EventHandler(ciPrevYear_Click);
      ciPrevYear.GroupEnd = true;
      Add(ciPrevYear);

      InitCommandItemsState();
    }

    #endregion

    #region Общие свойства

    public EFPDateRangeBox ControlProvider { get { return _ControlProvider; } }
    private EFPDateRangeBox _ControlProvider;

    #endregion

    #region Команды установки значений

    EFPCommandItem ciToday, ci1To2, ci2To1;

    private void ciToday_Click(object sender, EventArgs args)
    {
      ControlProvider.FirstDate.Value = DateTime.Today;
      ControlProvider.LastDate.Value = ControlProvider.FirstDate.Value;
    }

    private void ci1To2_Click(object sender, EventArgs args)
    {
      ControlProvider.LastDate.Value = ControlProvider.FirstDate.Value;
    }

    private void ci2To1_Click(object sender, EventArgs args)
    {
      ControlProvider.FirstDate.Value = ControlProvider.LastDate.Value;
    }


    #endregion

    #region Команды установки диапазонов

    EFPCommandItem ciMonth, ciQuarter, ciYear;

    private void ciMonth_Click(object sender, EventArgs args)
    {
      DoPeriod(1);
    }

    private void ciQuarter_Click(object sender, EventArgs args)
    {
      DoPeriod(2);
    }

    private void ciYear_Click(object sender, EventArgs args)
    {
      DoPeriod(3);
    }

    private void DoPeriod(int mode)
    {
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      ControlProvider.FirstDate.Value = FirstDateValue;
      ControlProvider.LastDate.Value = LastDateValue;
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime Date;
      if (ControlProvider.FirstDate.Value.HasValue)
        Date = ControlProvider.FirstDate.Value.Value;
      else if (ControlProvider.LastDate.Value.HasValue)
        Date = ControlProvider.LastDate.Value.Value;
      else
        Date = DateTime.Today;

      int m1 = Date.Month;
      int m2 = Date.Month;

      switch (mode)
      {
        case 1:
          // Для месяца ничего не делаем
          break;
        case 2:
          // Квартал
          m1 = ((m1 - 1) / 3) * 3 + 1;
          m2 = ((m2 + 2) / 3) * 3;
          break;
        case 3:
          // Год
          m1 = 1;
          m2 = 12;
          break;
      }
      firstDateValue = new DateTime(Date.Year, m1, 1);
      lastDateValue = new DateTime(Date.Year, m2, DateTime.DaysInMonth(Date.Year, m2));
    }

    #endregion

    #region Следующий / предыдущий период

    EFPCommandItem ciNextPeriod, ciPrevPeriod, ciNextYear, ciPrevYear;

    void ciNextYear_Click(object sender, EventArgs args)
    {
      ShiftYear(true);
    }

    void ciPrevYear_Click(object sender, EventArgs args)
    {
      ShiftYear(false);
    }

    private void ShiftYear(bool forward)
    {
      DateTime? FirstDate = ControlProvider.FirstDate.Value;
      DateTime? LastDate = ControlProvider.LastDate.Value;
      if (ShiftYear(ref FirstDate, ref LastDate, forward))
      {
        ControlProvider.FirstDate.Value = FirstDate;
        ControlProvider.LastDate.Value = LastDate;
      }
      else
        EFPApp.ShowTempMessage("Нельзя изменить год");
    }

    [DebuggerStepThrough]
    private static bool ShiftYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      bool Res = false;
      bool IsWholeMonth = false;
      if (firstDate.HasValue && lastDate.HasValue)
      {
        if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
          IsWholeMonth = true;
      }

      if (firstDate.HasValue)
      {
        firstDate = DataTools.CreateDateTime(firstDate.Value.Year + (forward ? +1 : -1), firstDate.Value.Month, firstDate.Value.Day);
        Res = true;
      }

      if (lastDate.HasValue)
      {
        lastDate = DataTools.CreateDateTime(lastDate.Value.Year + (forward ? +1 : -1), lastDate.Value.Month,
          IsWholeMonth ? 31 : lastDate.Value.Day);
        Res = true;
      }
      return Res;
    }

    #endregion

    #region Управление доступностью команд и текстом справа

    public void InitCommandItemsState()
    {
      DateRangeFormatter Formatter = ControlProvider.Control.Formatter;

      SetMenuRightText(ciToday, Formatter.ToString(DateTime.Today, false));

      bool RangeEnabled = ControlProvider.FirstDate.Editable && ControlProvider.LastDate.Editable;

      ci1To2.Enabled = RangeEnabled;
      ci2To1.Enabled = RangeEnabled;
      ciMonth.Enabled = RangeEnabled;
      ciQuarter.Enabled = RangeEnabled;
      ciYear.Enabled = RangeEnabled;

      if (RangeEnabled)
      {
        if (ControlProvider.FirstDate.Value.HasValue)
          SetMenuRightText(ci1To2, Formatter.ToString(ControlProvider.FirstDate.Value.Value, false));
        else
        {
          SetMenuRightText(ci1To2, String.Empty);
          ci1To2.Enabled = false;
        }
        if (ControlProvider.LastDate.Value.HasValue)
          SetMenuRightText(ci2To1, Formatter.ToString(ControlProvider.LastDate.Value.Value, false));
        else
        {
          SetMenuRightText(ci2To1, String.Empty);
          ci2To1.Enabled = false;
        }

        SetMenuRightText(ciMonth, GetPeriodText(1));
        SetMenuRightText(ciQuarter, GetPeriodText(2));
        SetMenuRightText(ciYear, GetPeriodText(3));

      }
      else
      {
        SetMenuRightText(ci1To2, String.Empty);
        SetMenuRightText(ci2To1, String.Empty);
        SetMenuRightText(ciMonth, String.Empty);
        SetMenuRightText(ciQuarter, String.Empty);
        SetMenuRightText(ciYear, String.Empty);
      }

      DateTime? dt1;
      DateTime? dt2;

      ciNextPeriod.Enabled = ControlProvider.Control.TheRightButton.Enabled;
      if (ciNextPeriod.Enabled)
      {
        dt1 = ControlProvider.FirstDate.Value;
        dt2 = ControlProvider.LastDate.Value;
        DateRangeBox.ShiftDateRange(ref dt1, ref dt2, true);
        SetMenuRightText(ciNextPeriod, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      }
      else
        SetMenuRightText(ciNextPeriod, String.Empty);

      ciPrevPeriod.Enabled = ControlProvider.Control.TheLeftButton.Enabled;
      if (ciPrevPeriod.Enabled)
      {
        dt1 = ControlProvider.FirstDate.Value;
        dt2 = ControlProvider.LastDate.Value;
        DateRangeBox.ShiftDateRange(ref dt1, ref dt2, false);
        SetMenuRightText(ciPrevPeriod, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      }
      else
        SetMenuRightText(ciPrevPeriod, String.Empty);

      dt1 = ControlProvider.FirstDate.Value;
      dt2 = ControlProvider.LastDate.Value;
      ciNextYear.Enabled = ShiftYear(ref dt1, ref dt2, true);
      if (ciNextYear.Enabled)
        SetMenuRightText(ciNextYear, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      else
        SetMenuRightText(ciNextYear, String.Empty);

      dt1 = ControlProvider.FirstDate.Value;
      dt2 = ControlProvider.LastDate.Value;
      ciPrevYear.Enabled = ShiftYear(ref dt1, ref dt2, false);
      if (ciPrevYear.Enabled)
        SetMenuRightText(ciPrevYear, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      else
        SetMenuRightText(ciPrevYear, String.Empty);
    }

    private string GetPeriodText(int mode)
    {
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      return ControlProvider.Control.Formatter.ToString(FirstDateValue, LastDateValue, false);
    }

    internal static void SetMenuRightText(EFPCommandItem ci, string text)
    {
#if XXX
      string s2 = EFPCommandItem.GetShortCutText(ci.ShortCut);
      if (String.IsNullOrEmpty(Text))
        ci.MenuRightText = s2;
      else if (s2.Length == 0)
        ci.MenuRightText = Text;
      else
        ci.MenuRightText = Text + " " + s2;
#else
      ci.MenuRightText = text;
#endif
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для стандартного управляющего элемента - календаря
  /// Позволяет выбирать единственную дату, а не диапазон дат
  /// </summary>
  public class EFPMonthCalendarSingleDay : EFPSyncControl<MonthCalendar>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPMonthCalendarSingleDay(EFPBaseProvider baseProvider, MonthCalendar control)
      : base(baseProvider, control, true)
    {
      control.MaxSelectionCount = 1; // Разрешено выбирать только один день
      control.DateChanged += new DateRangeEventHandler(Control_ValueChanged);

      _DoubleClickAsOk = true;

      // Нельзя использовать событие MouseDoubleClick, т.к. оно никогда не вызывается для радиокнопок
      if (!DesignMode)
      {
        ControlMouseClickHandler ClickHandler = new ControlMouseClickHandler(control);
        ClickHandler.MouseDoubleClick += new MouseEventHandler(ClickHandler_MouseDoubleClick);
      }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Свойство Value для реализации интерфейса IDepSyncObject
    /// </summary>
    public override object SyncValue
    {
      get { return Value; }
      set { Value = (DateTime)value; }
    }

    /// <summary>
    /// Проверяет попадание даты в диапазон {Minimum, Maximum}
    /// </summary>
    protected override void OnValidate()
    {
      if (!DataTools.DateInRange(Value, Minimum, Maximum))
      {
        if (WarningIfOutOfRange)
          SetWarning("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
        else
          SetError("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
      }

      base.OnValidate();
    }

    #endregion

    #region Свойство Value

    /// <summary>
    /// Свойство MonthCalendar.SelectionStart = MonthCalendar.SelectionStart
    /// </summary>
    public DateTime Value
    {
      // 26.08.2013
      // Свинячий календарь может возвращать время для текущей даты

      get { return Control.SelectionStart.Date; }
      set { Control.SelectionStart = value.Date; }
    }

    /// <summary>
    /// Управляемое свойство Value
    /// </summary>
    public DepValue<DateTime> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInputWithCheck<DateTime>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.OwnerSetValue(Value);
        _ValueEx.CheckValue += new DepInputCheckEventHandler<DateTime>(ValueEx_CheckValue);
      }
    }

    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime> args)
    {
      Value = args.NewValue;
      args.Cancel = true;
    }

    private DepInputWithCheck<DateTime> _ValueEx;

    /// <summary>
    /// Вызывается, когда изменятся текст в управляющем элементе
    /// </summary>
    private void Control_ValueChanged(object sender, DateRangeEventArgs args)
    {
      try
      {
        OnValueChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика MonthCalendar.ValueChanged");
      }
    }

    /// <summary>
    /// Метод вызывается при изменении выбранного значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
        _ValueEx.OwnerSetValue(Value);
      Validate();
      DoSyncValueChanged();
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion

    #region Свойство DoubleClickAsOk

    /// <summary>
    /// Если true (значение по умолчанию), то двойной щелчок мыши на дате
    /// приводит к эмуляции нажатия кнопки по умолчанию в блоке диалога
    /// </summary>
    public bool DoubleClickAsOk
    {
      get { return _DoubleClickAsOk; }
      set { _DoubleClickAsOk = value; }
    }
    private bool _DoubleClickAsOk;

    void ClickHandler_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      if (args.Button == MouseButtons.Left && DoubleClickAsOk)
      {
        if (Control.FindForm().AcceptButton != null)
          Control.FindForm().AcceptButton.PerformClick();
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для стандартного управляющего элемента - DateTimePicker
  /// Для ввода даты лучше использовать компонент DateBox.
  /// </summary>
  public class EFPDateTimePicker : EFPSyncControl<DateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateTimePicker(EFPBaseProvider baseProvider, DateTimePicker control)
      : base(baseProvider, control, true)
    {
      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
      _SavedValue = control.Value;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Свойство Value для интерфейса IDepSyncObject
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Value;
      }
      set
      {
        Value = (Nullable<DateTime>)value;
      }
    }

    /// <summary>
    /// Этот метод вызывается при изменении свойств Control.Visible или Control.Enabled
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledValue && EnabledState)
        _HasSavedValue = true;
      InitControlValue();
    }

    /*
    protected override EFPControlCommandItems GetCommandItems()
    {
      if (EFPApp.EasyInterface)
        return base.GetCommandItems();
      else
        return new EFPTextBoxCommandItems(this, false);
    } */

    /// <summary>
    /// Проверяет попадание даты в диапазон {Minimum, Maximum}
    /// </summary>
    protected override void OnValidate()
    {
      if (Value.HasValue)
      {
        if (!DataTools.DateInRange(Value.Value, Minimum, Maximum))
        {
          if (WarningIfOutOfRange)
            SetWarning("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
          else
            SetError("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
        }
      }

      base.OnValidate();
    }

    #endregion

    #region Свойство Value

    /// <summary>
    /// Доступ к выбранной дате без принудительного создания объекта
    /// </summary>
    public DateTime? Value
    {
      get
      {
        if (Control.ShowCheckBox)
        {
          if (Control.Checked)
            return Control.Value;
          else
            return null;
        }
        else
          return Control.Value;
      }
      set
      {
        _SavedValue = value;
        _HasSavedValue = true;
        InitControlValue();
      }
    }

    private DateTime? _SavedValue;
    private bool _HasSavedValue;

    private void InitControlValue()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideValueChanged)
      //  return;

      DateTime? Value2;
      if (AllowDisabledValue && (!EnabledState))
        Value2 = DisabledValue;
      else if (_HasSavedValue)
      {
        _HasSavedValue = false;
        Value2 = _SavedValue;
      }
      else
        return;

      if (Value2.HasValue)
      {
        Control.Value = Value2.Value;
        if (Control.ShowCheckBox)
          Control.Checked = true;
      }
      else
      {
        if (Control.ShowCheckBox)
          Control.Checked = false;
      }
    }

    /// <summary>
    /// Свойство ValueEx
    /// </summary>
    public DepValue<DateTime?> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInputWithCheck<DateTime?>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.OwnerSetValue(Value);
        _ValueEx.CheckValue += new DepInputCheckEventHandler<DateTime?>(ValueEx_CheckValue);
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private DepInputWithCheck<DateTime?> _ValueEx;

    private void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            OnValueChanged();
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика DateTimePicker.ValueChanged");
      }
    }

    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении выбранного значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
        _ValueEx.OwnerSetValue(Control.Value);
      if (_TimeEx != null)
        _TimeEx.OwnerSetValue(Control.Value.TimeOfDay);

      if (AllowDisabledValue && EnabledState)
        _SavedValue = Value;

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Вызывается, когда изменяется значение "снаружи" элемента
    /// </summary>
    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime?> args)
    {
      if (args.NewValue.HasValue)
        args.NewValue = args.NewValue.Value.Date;
    }
    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = ValueEx.Value;
    }



    #endregion

    #region Свойство DisabledValue

    /// <summary>
    /// Значение, которое показывается при Enabled=false и AllowDisabledValue=true
    /// </summary>
    public DateTime? DisabledValue
    {
      get { return _DisabledValue; }
      set
      {
        if (value == _DisabledValue)
          return;
        _DisabledValue = value;
        if (_DisabledValueEx != null)
          _DisabledValueEx.Value = value;
        InitControlValue();
      }
    }
    private DateTime? _DisabledValue;

    /// <summary>
    /// Значение, которое показывается при Enabled=false и AllowDisabledValue=true
    /// </summary>
    public DepValue<DateTime?> DisabledValueEx
    {
      get
      {
        InitDisabledValueEx();
        return _DisabledValueEx;
      }
      set
      {
        InitDisabledValueEx();
        _DisabledValueEx.Source = value;
      }
    }

    private void InitDisabledValueEx()
    {
      if (_DisabledValueEx == null)
      {
        _DisabledValueEx = new DepInput<DateTime?>();
        _DisabledValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledValueEx");
        _DisabledValueEx.Value = DisabledValue;
        _DisabledValueEx.ValueChanged += new EventHandler(DisabledValueEx_ValueChanged);
      }
    }
    private DepInput<DateTime?> _DisabledValueEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledValue = _DisabledValueEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledCheckStateEx
    /// </summary>
    public bool AllowDisabledValue
    {
      get { return _AllowDisabledValue; }
      set
      {
        if (value == _AllowDisabledValue)
          return;
        _AllowDisabledValue = value;
        InitControlValue();
      }
    }
    private bool _AllowDisabledValue;

    #endregion

    #region Свойство Time

    /// <summary>
    /// Доступ к компоненту времени.
    /// В отличие от Value, это свойство не nullable
    /// </summary>
    public TimeSpan Time
    {
      get
      {
        return Control.Value.TimeOfDay;
      }
      set
      {
        _SavedValue = DateTime.Today + value;
        InitControlValue();
      }
    }

    /// <summary>
    /// Свойство ValueEx
    /// </summary>
    public DepValue<TimeSpan> TimeEx
    {
      get
      {
        InitTimeEx();
        return _TimeEx;
      }
      set
      {
        InitTimeEx();
        _TimeEx.Source = value;
      }
    }

    private void InitTimeEx()
    {
      if (_TimeEx == null)
      {
        _TimeEx = new DepInputWithCheck<TimeSpan>();
        _TimeEx.OwnerInfo = new DepOwnerInfo(this, "TimeEx");
        if (Value.HasValue)
          _TimeEx.OwnerSetValue(Value.Value.TimeOfDay);
        _TimeEx.CheckValue += new DepInputCheckEventHandler<TimeSpan>(TimeEx_CheckValue);
      }
    }

    private DepInputWithCheck<TimeSpan> _TimeEx;

    /// <summary>
    /// Вызывается, когда изменяется значение "снаружи" элемента
    /// </summary>
    void TimeEx_CheckValue(object sender, DepInputCheckEventArgs<TimeSpan> args)
    {
      _SavedValue = DateTime.Today + args.NewValue;
      args.Cancel = true;
      InitControlValue();
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion

    #region Свойства FormatContainsDate и FormatContainsTime

    /// <summary>
    /// Возвращает true, если свойства DateTimePicker.Format и CustomFormat определяют формат, содержащий компонент даты
    /// </summary>
    public bool FormatContainsDate
    {
      get
      {
        switch (Control.Format)
        {
          case DateTimePickerFormat.Long:
          case DateTimePickerFormat.Short:
            return true;
          case DateTimePickerFormat.Time:
            return false;
          default:
            return FormatStringTools.ContainsDate(Control.CustomFormat);
        }
      }
    }

    /// <summary>
    /// Возвращает true, если свойства DateTimePicker.Format и CustomFormat определяют формат, содержащий компонент времени
    /// </summary>
    public bool FormatContainsTime
    {
      get
      {
        switch (Control.Format)
        {
          case DateTimePickerFormat.Long:
          case DateTimePickerFormat.Short:
            return false;
          case DateTimePickerFormat.Time:
            return true;
          default:
            return FormatStringTools.ContainsTime(Control.CustomFormat);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер комбоблока для выбора даты или интервала дат.
  /// Первоначально комбоблок выглядит как DateBox и позволяет ввести дату вручную.
  /// При нажатии "стрелочки" открывается диалог выбора интервала дат.
  /// Текущим значением является DateRange. Пустое значение допускается (при CanBeEmpty=true).
  /// Полуоткрытые интервалы не поддерживаются.
  /// </summary>
  public class EFPDateOrRangeBox : EFPSyncControl<UserMaskedComboBox>, IEFPReadOnlyControl, IEFPSimpleTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateOrRangeBox(EFPBaseProvider baseProvider, UserMaskedComboBox control)
      : base(baseProvider, control, true)
    {
      control.Mask = EditableDateTimeFormatters.Date.EditMask;
      if (!DesignMode)
      {
        control.TextChanged += new EventHandler(Control_TextChanged);
        control.PopupClick += new EventHandler(Control_PopupClick);
        control.ClearClick += new EventHandler(Control_ClearClick);
      }
    }
    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство EnabledEx, как
    /// у других управляющих элементов, а через свойство ReadOnly
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitReadOnlyEx();
      _NotReadOnlySync.Value = value;
    }

    /// <summary>
    /// Проверка попадания обних дат диапазона в интервал {Minimum, Maximum}
    /// </summary>
    protected override void OnValidate()
    {
      if (!Value.IsEmpty)
      {
        if ((!DataTools.DateInRange(Value.FirstDate, Minimum, Maximum)) ||
          (!DataTools.DateInRange(Value.LastDate, Minimum, Maximum)))
        {
          if (WarningIfOutOfRange)
            SetWarning("Дата должна быть в диапазоне " + Formatter.ToString(Minimum, Maximum, true));
          else
            SetError("Дата должна быть в диапазоне " + Formatter.ToString(Minimum, Maximum, true));
        }
      }
      else
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
        }
        else
          SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
      }

      base.OnValidate();
    }

    /// <summary>
    /// Синхронизированное значение типа DateRange
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Value;
      }
      set
      {
        Value = (DateRange)value;
      }
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Создает объект команд локального меню EFPDateOrRangeBoxCommandItems.
    /// </summary>
    /// <returns>Список команд</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      //if (EFPApp.EasyInterface)
      //  return base.GetCommandItems();
      //else
      return new EFPDateOrRangeBoxCommandItems(this);
    }

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPDateOrRangeBoxCommandItems CommandItems { get { return (EFPDateOrRangeBoxCommandItems)(base.CommandItems); } }

    #endregion

    #region Выпадающий диалог и кнопка очистки

    void Control_PopupClick(object sender, EventArgs args)
    {
      InitText(); // на случай частично введенной даты, типа "12.03.19"

      if (Value.IsEmpty || Value.LastDate == Value.FirstDate)
        ShowDateDialog();
      else
        ShowRangeDialog();
    }

    #region Для одиночной даты

    #region Класс MyCalendar

    private class MyCalendar : MonthCalendar
    {
      public EFPDateOrRangeBox Owner;

      protected override void OnDateSelected(DateRangeEventArgs drevent)
      {
        try
        {
          base.OnDateSelected(drevent);
          Owner.Value = new DateRange(SelectionStart);
          //Owner.MainControl.SelectAll();
          FindForm().Hide();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
        }
      }

      protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
      {
        base.OnPreviewKeyDown(args);
        if (/*Args.KeyCode == Keys.Tab || */args.KeyCode == Keys.Escape)
        {
          if ((!args.Alt) && (!args.Control))
            args.IsInputKey = true;
        }
      }

      protected override void OnKeyDown(KeyEventArgs args)
      {
        try
        {
          switch (args.KeyCode)
          {
            case Keys.Return:
              Owner.Value = new DateRange(SelectionStart);
              //Owner.MainControl.SelectAll();
              FindForm().Hide();
              args.Handled = true;
              return;
            //case Keys.Tab:
            //  FindForm().Hide();
            //  Args.Handled = true;
            //  Owner.Control.FindForm().SelectNextControl(Owner.Control, !Args.Shift, true, true, true);
            //  return;
            case Keys.Escape:
              FindForm().Hide();
              return;
          }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
        }
        base.OnKeyDown(args);
      }
    }

    #endregion

    #region Класс MyCalendarForm

    private class MyCalendarForm : Form
    {
      #region Конструктор

      public MyCalendarForm(EFPDateOrRangeBox owner)
      {
        this.SuspendLayout();
        // Нужно для правильного масшабирования кнопки
        // Календарик и так всегда правильный
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        //TopMost = false;
        //TopLevel = false;
        //base.Owner = Owner.FindForm();
        FormButtonStub.AssignCancel(this);

        base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        base.AutoSize = true;

        // Если используется тема Windows-XP, то MonthCalendar рисует нормальную рамку
        // Если используется тема Windows-98, то MonthCalendar не содержит никакой рамки
        // и получается некрасиво
        // Пусть лучше будет лишняя рамка
        TableLayoutPanel pan = new TableLayoutPanel();
        pan.SuspendLayout();
        pan.BorderStyle = BorderStyle.Fixed3D;
        pan.Padding = new Padding(0);
        pan.Margin = new Padding(0);
        pan.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //pan.RowStyles.Add(new RowStyle(SizeType.Absolute, 24 + pan.Margin.Vertical));
        pan.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pan.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pan.AutoSize = true;

        Calendar = new MyCalendar();
        Calendar.Owner = owner;
        Calendar.MaxSelectionCount = 1;
        Calendar.Margin = new Padding(3);
        //Calendar.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        Calendar.SizeChanged += new EventHandler(Calendar_SizeChanged);

        _ToRangeButton = new Button();
        _ToRangeButton.Size = new Size(Calendar.Width, 24);
        _ToRangeButton.Text = "Диапазон";
        _ToRangeButton.Image = EFPApp.MainImages.Images["DateRange"];
        _ToRangeButton.ImageAlign = ContentAlignment.MiddleLeft;
        //ToRangeButton.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        _ToRangeButton.Margin = Calendar.Margin; // для красоты
        _ToRangeButton.Click += new EventHandler(ToRangeButton_Click);

        pan.Controls.Add(Calendar, 0, 0);
        pan.Controls.Add(_ToRangeButton, 0, 1);

        this.Controls.Add(pan);
        pan.ResumeLayout();
        this.ResumeLayout();
      }

      #endregion

      #region Свойства

      public readonly MyCalendar Calendar;

      private readonly Button _ToRangeButton;

      #endregion

      #region Обработчики

      protected override bool ShowWithoutActivation
      {
        get { return false; }
      }

      protected override void OnDeactivate(EventArgs args)
      {
        base.OnDeactivate(args);

        // Закрытие формы при попытке щелкнуть мимо
        try
        {
          if (Visible)
            Hide();
        }
        catch /*(Exception e)*/
        {
          //MessageBox.Show(e.Message, "Ошибка при деактивации календаря");
        }
      }

      void ToRangeButton_Click(object sender, EventArgs args)
      {
        Hide();
        Calendar.Owner.ShowRangeDialog(); // показываем окно выбора диапазона
      }

      private void Calendar_SizeChanged(object sender, EventArgs args)
      {
        _ToRangeButton.Width = Calendar.Width;
      }

      #endregion
    }

    #endregion

    /// <summary>
    /// Форма календарика. Нельзя уничтожать форму изнутри обработчика календаря,
    /// т.к. будет ошибка обращения к удаленному объекту при нажатии мыши на
    /// кнопке "текущая дата" (видимо, дефект в MonthCalendar).
    /// Поэтому форма не уничтожается, а делается скрытой.
    /// </summary>
    private MyCalendarForm _CalendarForm;

    private void ShowDateDialog()
    {
      try
      {
        if (WinFormsTools.IsNullOrDisposed(_CalendarForm))
        {
          _CalendarForm = new MyCalendarForm(this);
          Control.Disposed += new EventHandler(Control_Disposed);
        }

        Point pt = new Point(3, Control.Height);
        pt = Control.PointToScreen(pt);
        _CalendarForm.Location = pt;
        if (!Value.IsEmpty)
        {
          if (Value.FirstDate >= _CalendarForm.Calendar.MinDate && Value.FirstDate <= _CalendarForm.Calendar.MaxDate)
            _CalendarForm.Calendar.SelectionStart = Value.FirstDate;
          else
            _CalendarForm.Calendar.SelectionStart = DateTime.Today; // 24.05.2019
        }
        else
          _CalendarForm.Calendar.SelectionStart = DateTime.Today;
        _CalendarForm.Calendar.SelectionEnd = _CalendarForm.Calendar.SelectionStart;

        _CalendarForm.Show();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Ошибка вызова календаря");
      }
    }

    void Control_Disposed(object sender, EventArgs args)
    {
      if (_CalendarForm != null)
      {
        _CalendarForm.Dispose();
        _CalendarForm = null;
        Control.Disposed -= new EventHandler(Control_Disposed);
      }
    }

    #endregion

    #region Для диапазона дат

    private void ShowRangeDialog()
    {
      DateRangeDialog dlg = new DateRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.Prompt = "Диапазон значений";
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (!Value.IsEmpty)
      {
        dlg.FirstDate = Value.FirstDate;
        dlg.LastDate = Value.LastDate;
      }
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Validating += new EFPValidatingTwoValuesEventHandler<DateTime?, DateTime?>(RangeDialog_Validating);

      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      if (dlg.FirstDate.HasValue && dlg.LastDate.HasValue)
        Value = new DateRange(dlg.FirstDate.Value, dlg.LastDate.Value);
      else
        Value = DateRange.Empty;
    }

    void RangeDialog_Validating(object sender, EFPValidatingTwoValuesEventArgs<DateTime?, DateTime?> args)
    {
      if (args.Value1.HasValue != args.Value2.HasValue)
        args.SetError("Полуоткрытые интервалы не допускаются");
    }

    #endregion

    void Control_ClearClick(object sender, EventArgs args)
    {
      Value = DateRange.Empty;
    }

    #endregion

    #region Свойство Value

    /// <summary>
    /// Доступ к выбранной дате без принудительного создания объекта
    /// </summary>
    public DateRange Value
    {
      get { return _Value; }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = _Value;
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            InitText();
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }

        OnValueChanged();
      }
    }
    private DateRange _Value;

    private void InitText()
    {
      if (Value.IsEmpty)
      {
        Control.Mask = EditableDateTimeFormatters.Date.EditMask;
        Control.Text = String.Empty;
        Control.ReadOnly = ReadOnly;
      }
      else if (Value.FirstDate == Value.LastDate)
      {
        Control.Mask = EditableDateTimeFormatters.Date.EditMask;
        Control.Text = EditableDateTimeFormatters.Date.ToString(Value.FirstDate); // 29.04.2021, 19.05.2021
        Control.ReadOnly = ReadOnly;
      }
      else
      {
        Control.Mask = String.Empty;
        Control.Text = Formatter.ToString(Value, false);
        Control.ReadOnly = true;
      }
      Control.PopupButtonEnabled = !ReadOnly;
      Control.ClearButtonEnabled = (!Value.IsEmpty) && (!ReadOnly);
    }

    /// <summary>
    /// Свойство ValueEx
    /// </summary>
    public DepValue<DateRange> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<DateRange>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private DepInput<DateRange> _ValueEx;

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      this.Value = _ValueEx.Value;
    }

    void Control_TextChanged(object sender, EventArgs args)
    {
      if (_InsideValueChanged)
        return;
      _InsideValueChanged = true;
      try
      {
        DateTime? dt = DateBox.TextToDate(Control.Text, DefaultYear);
        if (dt.HasValue)
          this.Value = new DateRange(dt.Value, dt.Value);
        else
          this.Value = DateRange.Empty;
      }
      finally
      {
        _InsideValueChanged = false;
      }
    }

    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
        _ValueEx.Value = Value;
      if (_FirstValueEx != null)
      {
        if (Value.IsEmpty)
          _FirstValueEx.OwnerSetValue(null);
        else
          _FirstValueEx.OwnerSetValue(Value.FirstDate);
      }
      if (_LastValueEx != null)
      {
        if (Value.IsEmpty)
          _LastValueEx.OwnerSetValue(null);
        else
          _LastValueEx.OwnerSetValue(Value.LastDate);
      }

      EFPDateOrRangeBoxCommandItems CommandItems2 = CommandItemsIfAssigned as EFPDateOrRangeBoxCommandItems;
      if (CommandItems2 != null)
        CommandItems2.InitCommandItemsState(); // 03.06.2019

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Объект содержит true, если есть введенная дата
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, DateRange>(ValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(DateRange value)
    {
      return !value.IsEmpty;
    }

    #endregion

    #region Свойства FirstValueEx и LastValueEx

    /// <summary>
    /// Управляемое значение для начальной даты диапазона Value.FirstDate.
    /// Управляемое свойство возвращает значение null, если интервал не задан.
    /// Свойство предназначено только для чтения, а не для установки значения снаружи.
    /// </summary>
    public DepValue<DateTime?> FirstValueEx
    {
      get
      {
        if (_FirstValueEx == null)
        {
          _FirstValueEx = new DepValueObject<DateTime?>();
          _FirstValueEx.OwnerInfo = new DepOwnerInfo(this, "FirstValueEx");
          if (Value.IsEmpty)
            _FirstValueEx.OwnerSetValue(null);
          else
            _FirstValueEx.OwnerSetValue(Value.FirstDate);
        }
        return _FirstValueEx;
      }
    }
    private DepValueObject<DateTime?> _FirstValueEx;

    /// <summary>
    /// Управляемое значение для конечной даты диапазона Value.LastDate.
    /// Управляемое свойство возвращает значение null, если интервал не задан.
    /// Свойство предназначено только для чтения, а не для установки значения снаружи.
    /// </summary>
    public DepValue<DateTime?> LastValueEx
    {
      get
      {
        if (_LastValueEx == null)
        {
          _LastValueEx = new DepValueObject<DateTime?>();
          _LastValueEx.OwnerInfo = new DepOwnerInfo(this, "LastValueEx");
          if (Value.IsEmpty)
            _LastValueEx.OwnerSetValue(null);
          else
            _LastValueEx.OwnerSetValue(Value.LastDate);
        }
        return _LastValueEx;
      }
    }
    private DepValueObject<DateTime?> _LastValueEx;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// True, если можно выбирать значение DateRange.Empty.
    /// Значение совпадает со свойством UserMaskedComboBox.ClearButton, которое по умолчанию имеет значение false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }

    /// <summary>
    /// Управляемое свойство для CanBeEmpty
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true)
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Value=null.
    /// По умолчанию - False
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region Свойство DefaultYear

    /// <summary>
    /// Год по умолчанию
    /// Если задано ненулевое значение и пользователь ввел только 4 цифры дня и месяца,
    /// дата будет считаться корректной и использовать выбранный год
    /// </summary>
    public int DefaultYear
    {
      get { return _DefaultYear; }
      set
      {
        _DefaultYear = value;
        if (_DefaultYearEx != null)
          _DefaultYearEx.Value = value;
        Control_TextChanged(null, null);
      }
    }
    private int _DefaultYear;

    /// <summary>
    /// Свойство DefaultYearEx
    /// </summary>
    public DepValue<int> DefaultYearEx
    {
      get
      {
        InitDefaultYearEx();
        return _DefaultYearEx;
      }
      set
      {
        InitDefaultYearEx();
        _DefaultYearEx.Source = value;
      }
    }

    private DepInput<int> _DefaultYearEx;

    private void InitDefaultYearEx()
    {
      if (_DefaultYearEx == null)
      {
        _DefaultYearEx = new DepInput<int>();
        _DefaultYearEx.OwnerInfo = new DepOwnerInfo(this, "DefaultYearEx");
        _DefaultYearEx.Value = DefaultYear;
        _DefaultYearEx.ValueChanged += new EventHandler(DefaultYearEx_ValueChanged);
      }
    }

    //void Control_DefaultYearChanged(object sender, EventArgs args)
    //{
    //  _DefaultYearEx.Value = DefaultYear;
    //}

    void DefaultYearEx_ValueChanged(object sender, EventArgs args)
    {
      DefaultYear = DefaultYearEx.Value;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Если true, то управляющий элемент разрешает только просмотр диапазона дат, но не его изменение
    /// </summary>
    public bool ReadOnly
    {
      // Нельзя использовать свойство Control.ReadOnly напрямую.
      // Элемент переводится в состояние "Только чтение", если в нем введен интервал дат

      get { return _ReadOnly; }
      set
      {
        if (value == _ReadOnly)
          return;
        _ReadOnly = value;
        InitText(); // переключает Control.ReadOnly
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство ReadOnly
    /// </summary>
    public DepValue<Boolean> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyMain.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>();
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
        _ReadOnlyEx.Value = false;
        _ReadOnlyEx.ValueChanged += new EventHandler(ReadOnlyEx_ValueChanged);

        _ReadOnlyMain = new DepInput<bool>();
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");
        _ReadOnlyMain.Value = false;

        _NotReadOnlySync = new DepInput<bool>();
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");
        _NotReadOnlySync.Value = true;

        DepOr ReadOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = ReadOnlyOr;
      }
    }
    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;
    /// <summary>
    /// Основной вход для ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyMain;
    /// <summary>
    /// Дополнительный вход для ReadOnly для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _NotReadOnlySync;

    private void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Диапазон допустимых значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion

    #region Свойство Formatter

    /// <summary>
    /// Форматировщик диапазона дат.
    /// Если свойство не установлено в явном виде, возвращает DateRangeFormatter.Default
    /// </summary>
    public DateRangeFormatter Formatter
    {
      get
      {
        if (_Formatter == null)
          return DateRangeFormatter.Default;
        else
          return _Formatter;
      }
      set
      {
        _Formatter = value;
      }
    }
    private DateRangeFormatter _Formatter;

    #endregion

    #region IEFPSimpleTextBox Members

    string IEFPSimpleTextBox.Text
    {
      get
      {
        return Control.Text;
      }
      set
      {
        Control.Text = value; // ?? может быть, нужна проверка
      }
    }

    int IEFPSimpleTextBox.TextLength { get { return Control.Text.Length; } }

    int IEFPSimpleTextBox.SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    int IEFPSimpleTextBox.SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    string IEFPSimpleTextBox.SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    void IEFPSimpleTextBox.Select(int start, int length)
    {
      Control.Select(start, length);
    }

    void IEFPSimpleTextBox.SelectAll()
    {
      Control.SelectAll();
    }

    #endregion
  }

  /// <summary>
  /// Команды меню для EFPDateRangeBox
  /// </summary>
  public class EFPDateOrRangeBoxCommandItems : EFPTextBoxCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPDateOrRangeBoxCommandItems(EFPDateOrRangeBox controlProvider)
      : base(controlProvider, false, true)
    {
      _ControlProvider = controlProvider;

      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = "Сегодня";
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = "Месяц";
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = "Квартал";
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = "Год";
      ciYear.ShortCut = Keys.Control | Keys.Y;
      ciYear.Click += ciYear_Click;
      Add(ciYear);

      InitCommandItemsState();
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента 
    /// </summary>
    public EFPDateOrRangeBox ControlProvider { get { return _ControlProvider; } }
    private EFPDateOrRangeBox _ControlProvider;

    #endregion

    #region Команды установки значений

    EFPCommandItem ciToday;
    EFPCommandItem ciMonth, ciQuarter, ciYear;

    private void ciToday_Click(object sender, EventArgs args)
    {
      ControlProvider.Value = new DateRange(DateTime.Today);
    }


    private void ciMonth_Click(object sender, EventArgs args)
    {
      DoPeriod(1);
    }

    private void ciQuarter_Click(object sender, EventArgs args)
    {
      DoPeriod(2);
    }

    private void ciYear_Click(object sender, EventArgs args)
    {
      DoPeriod(3);
    }

    private void DoPeriod(int mode)
    {
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      ControlProvider.Value = new DateRange(FirstDateValue, LastDateValue);
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime Date;
      if (ControlProvider.Value.IsEmpty)
        Date = DateTime.Today;
      else
        Date = ControlProvider.Value.FirstDate;

      int m1 = Date.Month;
      int m2 = Date.Month;

      switch (mode)
      {
        case 1:
          // Для месяца ничего не делаем
          break;
        case 2:
          // Квартал
          m1 = ((m1 - 1) / 3) * 3 + 1;
          m2 = ((m2 + 2) / 3) * 3;
          break;
        case 3:
          // Год
          m1 = 1;
          m2 = 12;
          break;
      }
      firstDateValue = new DateTime(Date.Year, m1, 1);
      lastDateValue = new DateTime(Date.Year, m2, DateTime.DaysInMonth(Date.Year, m2));
    }

    #endregion

    #region Управление доступностью команд и текстом справа

    /// <summary>
    /// Инициализация текста для команд
    /// </summary>
    public void InitCommandItemsState()
    {
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciToday, ControlProvider.Formatter.ToString(DateTime.Today, false));

      EFPDateRangeBoxCommandItems.SetMenuRightText(ciMonth, GetPeriodText(1));
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciQuarter, GetPeriodText(2));
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciYear, GetPeriodText(3));

    }

    private string GetPeriodText(int mode)
    {
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      return ControlProvider.Formatter.ToString(FirstDateValue, LastDateValue, false);
    }

    #endregion
  }
}
