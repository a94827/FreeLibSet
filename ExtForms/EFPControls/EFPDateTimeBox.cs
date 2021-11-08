using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using FreeLibSet.Controls;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.Formatting;
using FreeLibSet.UICore;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер поля ввода даты.
  /// Время не используется.
  /// </summary>
  public abstract class EFPDateTimeControl<T> : EFPSyncControl<T>
    where T : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateTimeControl(EFPBaseProvider baseProvider, T control)
      : base(baseProvider, control, true)
    {
      _SavedNValue = ControlNValue;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Абстрактные свойства

    /// <summary>
    /// Значение в управляющем элементе
    /// </summary>
    public abstract DateTime? ControlNValue { get; set; }

    /// <summary>
    /// Установка свойства для управляющегт элемента
    /// </summary>
    public abstract bool ControlCanBeEmpty { set; }

    /// <summary>
    /// Возвращает true, если формат содержит компонент даты
    /// </summary>
    public abstract bool FormatContainsDate { get; }

    /// <summary>
    /// Возвращает true, если формат содержит компонент времени
    /// </summary>
    public abstract bool FormatContainsTime { get; }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вызывается при изменении значения свойства EnabledState.
    /// Инициализация "серого" значения.
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledValue && EnabledState)
        _HasSavedNValue = true;
      InitControlValue();
    }

    #endregion

    #region Свойства Value/NValue/Time/NTime

    // Nullable-свойство NValue является основным, а Value - подчиненным

    #region NValue

    /// <summary>
    /// Доступ к выбранной дате.
    /// Значение null используется, если дата не введена.
    /// Если пользователь ввел текст, который нельзя преобразовать в дату, также возвращается null.
    /// При установке значения свойства удаляется компонент времени.
    /// </summary>
    public DateTime? NValue
    {
      get { return ControlNValue; }
      set
      {
        if (value.HasValue && (!FormatContainsTime))
          _SavedNValue = value.Value.Date;
        else
          _SavedNValue = value;
        _HasSavedNValue = true;
        InitControlValue();
      }
    }

    /// <summary>
    /// Управляемое свойство NValue.
    /// </summary>
    public DepValue<DateTime?> NValueEx
    {
      get
      {
        InitNValueEx();
        return _NValueEx;
      }
      set
      {
        InitNValueEx();
        _NValueEx.Source = value;
      }
    }
    private DepInput<DateTime?> _NValueEx;

    private void InitNValueEx()
    {
      if (_NValueEx == null)
      {
        _NValueEx = new DepInput<DateTime?>(NValue, NValueEx_ValueChanged);
        _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        _NValueEx.CheckValue += new DepInputCheckEventHandler<DateTime?>(NValueEx_CheckValue);
      }
    }

    /// <summary>
    /// Обрезает компонент времени
    /// </summary>
    void NValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime?> args)
    {
      if (args.NewValue.HasValue && (!FormatContainsTime))
        args.NewValue = args.NewValue.Value.Date;
    }

    void NValueEx_ValueChanged(object sender, EventArgs args)
    {
      // Поле _InsideValueChanged=true, если значение изменено пользователем в управляющем элементе и сейчас работает метод OnValueChanged().
      // Также поле равно true, если сейчас устанавливается свойство Value или NValue из внешнего кода со следующим стеком вызова
      // - Установка EFPDateBox.Value или NValue
      // - Записаны поля _SavedNValue и _HasSavedValue
      // - Вызов EFPDateBox.InitControlValue()
      // - Установка свойства DateBox.NValue (DateBox.Value никогда не устанавливается)
      // - Событие DateBox.ValueChanged
      // - Вызов метода Control_ValueChanged()
      // - Установка поля _InsideValueChanged=true
      // - Вызов метода OnValueChanged() 
      // - Вызов DepInput.OwnerSetValue() для _NValueEx
      // - Вызов NValueEx_ValueChanged()
      // Поле _InsideValueChanged=false, если внешний код устанавливает значение NValueEx.Value.
      // Только в этом случае нужно инициализировать основное свойство EFPDateBox.NValue.
      // В остальных случаях просто меняется значение управляемого свойства NValueEx и уведомляются подключенные к нему входы.

      if (!_InsideValueChanged)
        NValue = _NValueEx.Value;
    }

    #endregion

    #region Value

    /// <summary>
    /// Доступ к выбранной дате без значения null.
    /// Если поле не заполнено или пользователь ввел текст, который нельзя преобразовать в дату, возвращается DateTime.MinValue.
    /// При установке значения свойства удаляется компонент времени.
    /// </summary>
    public DateTime Value
    {
      get { return NValue ?? DateTime.MinValue; }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое свойство Value.
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
    private DepInput<DateTime> _ValueEx;

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<DateTime>(Value, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.CheckValue += new DepInputCheckEventHandler<DateTime>(ValueEx_CheckValue);
      }
    }

    /// <summary>
    /// Обрезает компонент времени
    /// </summary>
    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime> args)
    {
      if (!FormatContainsTime)
        args.NewValue = args.NewValue.Date;

      if ((!NValue.HasValue) && args.NewValue.Equals(default(DateTime)))
        args.Forced = true;
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      // См. комментарий в методе NValueEx_ValueChanged()

      if (!_InsideValueChanged)
        Value = _ValueEx.Value;
    }

    #endregion

    #region Свойство NTime

    /// <summary>
    /// Доступ к компоненту времени.
    /// Если нет введенного значения, свойство возвращает null
    /// </summary>
    public TimeSpan? NTime
    {
      get
      {
        if (NValue.HasValue)
          return NValue.Value.TimeOfDay;
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          NValue = Value.Date /* а не NValue */ + value;
        else
          NValue = null;
      }
    }

    /// <summary>
    /// Управляемое свойство для NTime
    /// </summary>
    public DepValue<TimeSpan?> NTimeEx
    {
      get
      {
        InitNTimeEx();
        return _NTimeEx;
      }
      set
      {
        InitNTimeEx();
        _NTimeEx.Source = value;
      }
    }
    private DepInput<TimeSpan?> _NTimeEx;

    private void InitNTimeEx()
    {
      if (_NTimeEx == null)
      {
        _NTimeEx = new DepInput<TimeSpan?>(NTime, NTimeEx_ValueChanged);
        _NTimeEx.OwnerInfo = new DepOwnerInfo(this, "NTimeEx");
      }
    }

    void NTimeEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        NTime = NTimeEx.Value;
    }


    #endregion

    #region Свойство Time

    /// <summary>
    /// Доступ к компоненту времени.
    /// В отличие от NTime, это свойство не nullable
    /// </summary>
    public TimeSpan Time
    {
      get { return NTime ?? TimeSpan.Zero; }
      set { NTime = value; }
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
    private DepInput<TimeSpan> _TimeEx;

    private void InitTimeEx()
    {
      if (_TimeEx == null)
      {
        _TimeEx = new DepInput<TimeSpan>(Time, TimeEx_ValueChanged);
        _TimeEx.OwnerInfo = new DepOwnerInfo(this, "TimeEx");
        _TimeEx.CheckValue += TimeEx_CheckValue;
      }
    }

    void TimeEx_CheckValue(object sender, DepInputCheckEventArgs<TimeSpan> args)
    {
      if ((!NTime.HasValue) && args.CurrValue == TimeSpan.Zero)
        args.Forced = true;
    }

    void TimeEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        Time = TimeEx.Value;
    }

    #endregion

    #region InitControlValue()

    /// <summary>
    /// Сохраняемое значение после установки свойства "Value".
    /// Используется, если в данный момент используется DisabledValue.
    /// </summary>
    private DateTime? _SavedNValue;

    /// <summary>
    /// Поле получает значение true, если было установлено поле _SavedNValue.
    /// После переноса значения в управляющий элемент вызовом InitControlValue(), поле сбрасывается в false.
    /// </summary>
    private bool _HasSavedNValue;

    /// <summary>
    /// Инициализация DateBox.Value.
    /// </summary>
    protected void InitControlValue()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideValueChanged)
      //  return;
      if (AllowDisabledValue && (!EnabledState))
        ControlNValue = DisabledNValue;
      else if (_HasSavedNValue)
      {
        _HasSavedNValue = false; // сбрасываем признак сохраненного значения
        ControlNValue = _SavedNValue;
      }
    }

    #endregion

    #region OnValueChanged()

    /// <summary>
    /// Обработчик события DateBox.ValueChanged
    /// </summary>
    /// <param name="sender">Ссылка на DateBox. Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    protected void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideValueChanged) // предотвращение реентрантного вызова метода OnValueChanged()
        {
          _InsideValueChanged = true; // Не нужно, чтобы при установке NValueEx/ValueEx.Value рекурсивно устанавливались NValue и Value. Это бы привело к порче значения null.
          try
          {
            OnValueChanged(); // виртуальный метод
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

    /// <summary>
    /// Флажок для предотвращения рекурсивного вызова OnValueChanged() или установки свойст NValue/Value из NValueEx/ValueEx.
    /// </summary>
    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод.
    /// </summary>
    protected virtual void OnValueChanged()
    {
      // Поле _InsideValueChanged=true

      if (_NValueEx != null)
        _NValueEx.OwnerSetValue(NValue);
      if (_ValueEx != null)
        _ValueEx.OwnerSetValue(Value);
      if (_NTimeEx != null)
        _NTimeEx.Value = NTime;
      if (_TimeEx != null)
        _TimeEx.Value = Time;

      if (AllowDisabledValue && EnabledState)
        _SavedNValue = NValue;

      Validate();
      DoSyncValueChanged();
    }

    #endregion

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если есть введенная дата (NValue.HasValue=true).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, DateTime?>(NValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(DateTime? value)
    {
      return value.HasValue;
    }

    #endregion

    #region Свойства DisabledValue/DisabledNValue/DisabledTime/DisabledNTime

    #region AllowDisabledValue

    /// <summary>
    /// Разрешает использование свойства DisabledValue или DisabledNValue.
    /// По умолчанию - false
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

    #region DisabledNValue

    /// <summary>
    /// Значение, которое показывается в управляющем элементе при EnabledState=false и AllowDisabledValue=true.
    /// Если это значение равно null, то поле ввода будет показано пустым.
    /// </summary>
    public DateTime? DisabledNValue
    {
      get { return _DisabledNValue; }
      set
      {
        if (value.HasValue)
          value = value.Value.Date;

        if (value == _DisabledNValue)
          return;

        _DisabledNValue = value;
        if (_DisabledNValueEx != null)
          _DisabledNValueEx.Value = value;
        if (_DisabledValueEx != null)
          _DisabledValueEx.Value = DisabledValue;
        if (_DisabledNTimeEx != null)
          _DisabledNTimeEx.Value = DisabledNTime;
        if (_DisabledTimeEx != null)
          _DisabledTimeEx.Value = DisabledTime;
        InitControlValue();
      }
    }
    private DateTime? _DisabledNValue;

    /// <summary>
    /// Управляемое свойство DisabledNValue.
    /// Свойство действует при установленном свойстве AllowDisabledValue
    /// </summary>
    public DepValue<DateTime?> DisabledNValueEx
    {
      get
      {
        InitDisabledNValueEx();
        return _DisabledNValueEx;
      }
      set
      {
        InitDisabledNValueEx();
        _DisabledNValueEx.Source = value;
      }
    }
    private DepInput<DateTime?> _DisabledNValueEx;

    private void InitDisabledNValueEx()
    {
      if (_DisabledNValueEx == null)
      {
        _DisabledNValueEx = new DepInput<DateTime?>(DisabledNValue, DisabledNValueEx_ValueChanged);
        _DisabledNValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledNValueEx");
      }
    }

    private void DisabledNValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledNValue = _DisabledNValueEx.Value;
    }

    #endregion

    #region DisabledValue

    /// <summary>
    /// Значение, которое показывается при EnabledState=false и AllowDisabledValue=true.
    /// Это свойство дублирует DisabledNValue. Если DisabledNValue=null, то возвращает DateTime.MinValue.
    /// </summary>
    public DateTime DisabledValue
    {
      get { return DisabledNValue ?? DateTime.MinValue; }
      set { DisabledNValue = value; }
    }

    /// <summary>
    /// Управляемое свойство DisabledValue.
    /// Свойство действует при установленном свойстве AllowDisabledValue.
    /// </summary>
    public DepValue<DateTime> DisabledValueEx
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
    private DepInput<DateTime> _DisabledValueEx;

    private void InitDisabledValueEx()
    {
      if (_DisabledValueEx == null)
      {
        _DisabledValueEx = new DepInput<DateTime>(DisabledValue, DisabledValueEx_ValueChanged);
        _DisabledValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledValueEx");
      }
    }

    private void DisabledValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledValue = _DisabledValueEx.Value;
    }

    #endregion

    #region DisabledNTime

    /// <summary>
    /// Значение, которое показывается при EnabledState=false и AllowDisabledValue=true.
    /// Это свойство возвращает DisabledNValue.TimeOfDay. Если DisabledNValue=null, то возвращает null.
    /// </summary>
    public TimeSpan? DisabledNTime
    {
      get
      {
        if (DisabledNValue.HasValue)
          return DisabledNValue.Value.TimeOfDay;
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          DisabledNValue = DisabledValue.Date + value;
        else
          DisabledNValue = null;
      }
    }

    /// <summary>
    /// Управляемое свойство DisabledNTime.
    /// Свойство действует при установленном свойстве AllowDisabledValue.
    /// </summary>
    public DepValue<TimeSpan?> DisabledNTimeEx
    {
      get
      {
        InitDisabledNTimeEx();
        return _DisabledNTimeEx;
      }
      set
      {
        InitDisabledNTimeEx();
        _DisabledNTimeEx.Source = value;
      }
    }
    private DepInput<TimeSpan?> _DisabledNTimeEx;

    private void InitDisabledNTimeEx()
    {
      if (_DisabledNTimeEx == null)
      {
        _DisabledNTimeEx = new DepInput<TimeSpan?>(DisabledNTime, DisabledNTimeEx_ValueChanged);
        _DisabledNTimeEx.OwnerInfo = new DepOwnerInfo(this, "DisabledNTimeEx");
      }
    }

    private void DisabledNTimeEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledNTime = _DisabledNTimeEx.Value;
    }

    #endregion

    #region DisabledTime

    /// <summary>
    /// Значение, которое показывается при EnabledState=false и AllowDisabledValue=true.
    /// Это свойство возвращает DisabledNValue.TimeOfDay. Если DisabledNValue=null, то возвращает TimeSpan.Zero.
    /// </summary>
    public TimeSpan DisabledTime
    {
      get { return DisabledNTime ?? TimeSpan.Zero; }
      set { DisabledNTime = value; }
    }

    /// <summary>
    /// Управляемое свойство DisabledTime.
    /// Свойство действует при установленном свойстве AllowDisabledValue.
    /// </summary>
    public DepValue<TimeSpan> DisabledTimeEx
    {
      get
      {
        InitDisabledTimeEx();
        return _DisabledTimeEx;
      }
      set
      {
        InitDisabledTimeEx();
        _DisabledTimeEx.Source = value;
      }
    }
    private DepInput<TimeSpan> _DisabledTimeEx;

    private void InitDisabledTimeEx()
    {
      if (_DisabledTimeEx == null)
      {
        _DisabledTimeEx = new DepInput<TimeSpan>(DisabledTime, DisabledTimeEx_ValueChanged);
        _DisabledTimeEx.OwnerInfo = new DepOwnerInfo(this, "DisabledTimeEx");
      }
    }

    private void DisabledTimeEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledTime = _DisabledTimeEx.Value;
    }

    #endregion

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - Error.
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента
    /// </summary>
    public virtual UIValidateState CanBeEmptyMode
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
    /// True, если ли элемент содержать пустой текст.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public virtual bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
      set
      {
        if (value.HasValue)
          _Minimum = value.Value.Date;
        else
          _Minimum = null;
        Validate();
      }
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
      set
      {
        if (value.HasValue)
          _Maximum = value.Value.Date;
        else
          _Maximum = null;
        Validate();
      }
    }
    private DateTime? _Maximum;

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка корректности значения.
    /// Выдает сообщение об ошибке, если дата не входит в диапазон значений {Minimum, Maximum}.
    /// Если дата не введена, а CanBeEmpty=false, также устанавливается ошибка.
    /// </summary>
    protected override void OnValidate()
    {
      if (NValue.HasValue)
      {
        if (!DataTools.DateInRange(NValue.Value, Minimum, Maximum))
          SetError("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));
      }
      else
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
            break;
          case UIValidateState.Warning:
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
            break;
        }
      }

      base.OnValidate();
    }

    #endregion

    #region Синхронизация

    /// <summary>
    /// Синронизированное значение.
    /// Свойство NValue.
    /// </summary>
    public override object SyncValue
    {
      get { return NValue; }
      set { NValue = (DateTime?)value; }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер поля ввода даты.
  /// Время не используется.
  /// </summary>
  public class EFPDateTimeBox : EFPDateTimeControl<DateTimeBox>, IEFPReadOnlyControl, IEFPSimpleTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провадер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateTimeBox(EFPBaseProvider baseProvider, DateTimeBox control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);

      if (String.IsNullOrEmpty(control.PopupButtonToolTipText))
        control.PopupButtonToolTipText = "Выбрать из календаря";
      if (String.IsNullOrEmpty(control.ClearButtonToolTipText))
        control.ClearButtonToolTipText = "Очистить дату";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
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
    /// Свойство DateTimeBox.NValue
    /// </summary>
    public override DateTime? ControlNValue
    {
      get { return Control.NValue; }
      set { Control.NValue = value; }
    }

    /// <summary>
    /// Установка свойства DateTimeBox/ClearButton
    /// </summary>
    public override bool ControlCanBeEmpty
    {
      set { Control.ClearButton = value; }
    }

    /// <summary>
    /// Возврашает свойство объекта EditableDateTimeFormatter 
    /// </summary>
    public override bool FormatContainsDate
    {
      get { return Control.Formatter.ContainsDate; }
    }

    /// <summary>
    /// Возврашает свойство объекта EditableDateTimeFormatter 
    /// </summary>
    public override bool FormatContainsTime
    {
      get { return Control.Formatter.ContainsTime; }
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
        _DefaultYearEx = new DepInput<int>(DefaultYear, DefaultYearEx_ValueChanged);
        _DefaultYearEx.OwnerInfo = new DepOwnerInfo(this, "DefaultYearEx");
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
        _ReadOnlyEx = new DepInput<Boolean>(false, ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");

        _ReadOnlyMain = new DepInput<bool>(false, null);
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");

        _NotReadOnlySync = new DepInput<bool>(true, null);
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");

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
  }

  /// <summary>
  /// Провайдер для стандартного управляющего элемента - DateTimePicker
  /// Для ввода даты лучше использовать компонент DateBox.
  /// Разрешает использовать значение null с помощью свойство DateTimePicker.ShowCheckBox.
  /// </summary>
  public class EFPDateTimePicker : EFPDateTimeControl<DateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateTimePicker(EFPBaseProvider baseProvider, DateTimePicker control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
    }

    #endregion

    #region Свойства управляющего элемента

    /// <summary>
    /// Обработка свойств DateTimePicker.Checked и Value.
    /// </summary>
    public override DateTime? ControlNValue
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
        if (value.HasValue)
        {
          Control.Checked = true;
          Control.Value = value.Value;
        }
        else
          Control.Checked = false;
      }
    }

    /// <summary>
    /// Установка свойства DateTimePicker.ShowCheckBox
    /// </summary>
    public override bool ControlCanBeEmpty
    {
      set { Control.ShowCheckBox = value; }
    }

    #endregion

    #region Свойства FormatContainsDate и FormatContainsTime

    /// <summary>
    /// Возвращает true, если свойства DateTimePicker.Format и CustomFormat определяют формат, содержащий компонент даты
    /// </summary>
    public override bool FormatContainsDate
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
    public override bool FormatContainsTime
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
  /// Провайдер для стандартного управляющего элемента - календаря
  /// Позволяет выбирать единственную дату, а не диапазон дат.
  /// Пустое значение null не поддерживается.
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
    /// Проверяет попадание даты в диапазон {Minimum, Maximum}
    /// </summary>
    protected override void OnValidate()
    {
      if (ValidateState == UIValidateState.Error)
        return;

      if (!DataTools.DateInRange(Value, Minimum, Maximum))
          SetError("Дата должна быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum, true));

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
        _ValueEx = new DepInput<DateTime>(Value, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.CheckValue += new DepInputCheckEventHandler<DateTime>(ValueEx_CheckValue);
      }
    }

    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime> args)
    {
      Value = args.NewValue.Date;
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      this.Value = _ValueEx.Value;
    }

    private DepInput<DateTime> _ValueEx;

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
    /// По умолчанию - null - ограничение не установлено.
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
    /// при проверке контроля.
    /// По умолчанию - null - ограничение не установлено.
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    #endregion

    #region Синхронизация

    /// <summary>
    /// Свойство Value для реализации интерфейса IDepSyncObject.
    /// Для совместимости с EFPDateBox, поддерживается "присвоение" значения null.
    /// В этом случае никаких действий не выполняется, значение остается неизменным
    /// </summary>
    public override object SyncValue
    {
      get { return Value; }
      set
      {
        if (value != null) // 22.10.2021
          Value = (DateTime)value;
      }
    }

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
}
