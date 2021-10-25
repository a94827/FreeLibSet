using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;

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
  #region Интерфейс IEFPNumEditBox

  /// <summary>
  /// Интерфейс, объявляющий свойства провайдера редактора числового поля
  /// Применяется вместо EFPNumEditBase там, где шаблоны неудобны
  /// </summary>
  public interface IEFPNumEditBox : IEFPReadOnlyControl
  {
    /// <summary>
    /// Значение типа Decimal
    /// </summary>
    decimal DecimalValue { get; set; }

    /// <summary>
    /// Управляемое свойство DecimalValue
    /// </summary>
    DepValue<decimal> DecimalValueEx { get; set; }

    /// <summary>
    /// Целочисленное значение
    /// </summary>
    int IntValue { get; set; }

    /// <summary>
    /// Управляемое свойство IntValue
    /// </summary>
    DepValue<int> IntValueEx { get; set; }

    /// <summary>
    /// Значение типа Single
    /// </summary>
    float SingleValue { get; set; }

    /// <summary>
    /// Управляемое свойство SingleValue
    /// </summary>
    DepValue<float> SingleValueEx { get; set; }

    /// <summary>
    /// Значение типа Double
    /// </summary>
    double DoubleValue { get; set; }

    /// <summary>
    /// Управляемое свойство DoubleValue
    /// </summary>
    DepValue<double> DoubleValueEx { get; set; }
  }

  #endregion

  /// <summary>
  /// Базовый класс для EFPNumEdit и EFPNumericUpDown
  /// (редактирование значения с плавающей точкой или целого числа)
  /// Смнхронизация выполняется для DecimalValue
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class EFPNumEditBase<T> : EFPSyncControl<T>, IEFPNumEditBox, IEFPReadOnlyControl
    where T : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPNumEditBase(EFPBaseProvider baseProvider, T control)
      : base(baseProvider, control, true)
    {
      // Присоединение обработчиков к управляющему элементу выполняется в 
      // конструкторе класса-наследника
    }

    #endregion

    #region Переопределенные свойства и методы

    /// <summary>
    /// Свойство DecimalValue
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return DecimalValue;
      }
      set
      {
        DecimalValue = (decimal)value;
      }
    }

    /// <summary>
    /// Инициализация DecimalValue при смене свойств Enabled или ReadOnly
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledValue && EnabledState)
        _HasSavedDecimalValue = true;
      InitControlValue();
    }

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
    /// Проверяет попадание значения в диапазон {Minimum, Maximum}.
    /// </summary>
    protected override void OnValidate()
    {
      //base.OnValidate(); там пусто

      string ErrorMessage = null;

      if (Minimum.HasValue)
      {
        if (Maximum.HasValue)
        {
          if (DecimalValue < Minimum.Value || DecimalValue > Maximum.Value)
            ErrorMessage = "Значение должно быть в диапазоне от " + Minimum.Value.ToString() + " до " + Maximum.Value.ToString();
        }
        else
        {
          if (DecimalValue < Minimum.Value)
            ErrorMessage = "Значение должно быть больше " + Minimum.Value.ToString();
        }
      }
      else
      {
        if (Maximum.HasValue)
        {
          if (DecimalValue > Maximum.Value)
            ErrorMessage = "Значение должно меньше " + Maximum.Value.ToString();
        }
      }

      if (ErrorMessage != null)
      {
        if (WarningIfOutOfRange)
          SetWarning(ErrorMessage);
        else
          SetError(ErrorMessage);
      }

      base.OnValidate();
    }

    #endregion

    #region Свойство DecimalValue

    /// <summary>
    /// Доступ к свойству Text.ValueEx без принудительного создания объекта
    /// </summary>
    public decimal DecimalValue
    {
      get { return ControlDecimalValue; }
      set
      {
        _SavedDecimalValue = value;
        _HasSavedDecimalValue = true;
        InitControlValue();
      }
    }

    private decimal _SavedDecimalValue;
    private bool _HasSavedDecimalValue;

    /// <summary>
    /// Инициализация ControlDecimalValue.
    /// </summary>
    protected void InitControlValue()
    {
      // Не нужно, иначе может не обновляться
      // if (InsideValueChanged)
      //   return;
      if (AllowDisabledValue && (!EnabledState))
        ControlDecimalValue = DisabledDecimalValue;
      else if (_HasSavedDecimalValue)
      {
        _HasSavedDecimalValue = false;
        ControlDecimalValue = _SavedDecimalValue;
      }
    }

    /// <summary>
    /// Свойство DecimalValueEx
    /// </summary>
    public DepValue<decimal> DecimalValueEx
    {
      get
      {
        InitDecimalValueEx();
        return _DecimalValueEx;
      }
      set
      {
        InitDecimalValueEx();
        _DecimalValueEx.Source = value;
      }
    }
    private DepInput<decimal> _DecimalValueEx;

    private void InitDecimalValueEx()
    {
      if (_DecimalValueEx == null)
      {
        _DecimalValueEx = new DepInput<decimal>();
        _DecimalValueEx.OwnerInfo = new DepOwnerInfo(this, "DecimalValueEx");
        _DecimalValueEx.Value = DecimalValue;
        _DecimalValueEx.ValueChanged += new EventHandler(DecimalValueEx_ValueChanged);
      }
    }

    /// <summary>
    /// Обработчик события значения в управляющем элементе
    /// </summary>
    /// <param name="sender">Управляющий элемент (Control)</param>
    /// <param name="args">Пустой аргумент</param>
    protected void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!InsideValueChanged)
        {
          InsideValueChanged = true;
          try
          {
            OnValueChanged();
          }
          finally
          {
            InsideValueChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.ValueChanged");
      }
    }

    private bool InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении выбранного значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_DecimalValueEx != null)
        _DecimalValueEx.Value = DecimalValue;
      if (_IntValueEx != null)
        _IntValueEx.Value = IntValue;
      if (_SingleValueEx != null)
        _SingleValueEx.Value = SingleValue;
      if (_DoubleValueEx != null)
        _DoubleValueEx.Value = DoubleValue;

      if (AllowDisabledValue && EnabledState)
        _SavedDecimalValue = DecimalValue;

      Validate();
      DoSyncValueChanged();
    }

    void DecimalValueEx_ValueChanged(object sender, EventArgs args)
    {
      DecimalValue = _DecimalValueEx.Value;
    }

    /// <summary>
    /// Получение значения от управляющего элемента.
    /// Этот свойство должно быть переопределено
    /// </summary>
    /// <returns></returns>
    protected abstract decimal ControlDecimalValue { get; set; }

    #endregion

    #region Свойство IntValue

    /// <summary>
    /// Целочисленное значение
    /// </summary>
    public int IntValue
    {
      get { return (int)(Math.Round(DecimalValue, 0, MidpointRounding.AwayFromZero)); }
      set { DecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое свойство IntValue
    /// </summary>
    public DepValue<int> IntValueEx
    {
      get
      {
        InitIntValueEx();
        return _IntValueEx;
      }
      set
      {
        InitIntValueEx();
        _IntValueEx.Source = value;
      }
    }
    private DepInput<int> _IntValueEx;

    private void InitIntValueEx()
    {
      if (_IntValueEx == null)
      {
        _IntValueEx = new DepInput<int>();
        _IntValueEx.OwnerInfo = new DepOwnerInfo(this, "IntValueEx");
        _IntValueEx.Value = IntValue;
        _IntValueEx.ValueChanged += new EventHandler(IntValueEx_ValueChanged);
      }
    }

    void IntValueEx_ValueChanged(object sender, EventArgs args)
    {
      IntValue = _IntValueEx.Value;
    }

    #endregion

    #region Свойство SingleValue

    /// <summary>
    /// Значение, приведенное к типу Single
    /// </summary>
    public float SingleValue
    {
      get { return (float)DecimalValue; }
      set { DecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое значение SingleValue
    /// </summary>
    public DepValue<float> SingleValueEx
    {
      get
      {
        InitSingleValueEx();
        return _SingleValueEx;
      }
      set
      {
        InitSingleValueEx();
        _SingleValueEx.Source = value;
      }
    }
    private DepInput<float> _SingleValueEx;

    private void InitSingleValueEx()
    {
      if (_SingleValueEx == null)
      {
        _SingleValueEx = new DepInput<float>();
        _SingleValueEx.OwnerInfo = new DepOwnerInfo(this, "SingleValueEx");
        _SingleValueEx.Value = SingleValue;
        _SingleValueEx.ValueChanged += new EventHandler(SingleValueEx_ValueChanged);
      }
    }

    void SingleValueEx_ValueChanged(object sender, EventArgs args)
    {
      SingleValue = _SingleValueEx.Value;
    }

    #endregion

    #region Свойство DoubleValue

    /// <summary>
    /// Значение, приведенное к типу Single
    /// </summary>
    public double DoubleValue
    {
      get { return (double)DecimalValue; }
      set { DecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое свойство DoubleValue
    /// </summary>
    public DepValue<double> DoubleValueEx
    {
      get
      {
        InitDoubleValueEx();
        return _DoubleValueEx;
      }
      set
      {
        InitDoubleValueEx();
        _DoubleValueEx.Source = value;
      }
    }
    private DepInput<double> _DoubleValueEx;

    private void InitDoubleValueEx()
    {
      if (_DoubleValueEx == null)
      {
        _DoubleValueEx = new DepInput<double>();
        _DoubleValueEx.OwnerInfo = new DepOwnerInfo(this, "DoubleValueEx");
        _DoubleValueEx.Value = DoubleValue;
        _DoubleValueEx.ValueChanged += new EventHandler(DoubleValueEx_ValueChanged);
      }
    }

    void DoubleValueEx_ValueChanged(object sender, EventArgs args)
    {
      DoubleValue = _DoubleValueEx.Value;
    }

    #endregion

    #region Свойство DisabledDecimalValue

    /// <summary>
    /// "Серое" значение, действующее при AllowDisabledValue=true и Editable=false
    /// </summary>
    public decimal DisabledDecimalValue
    {
      get { return _DisabledDecimalValue; }
      set
      {
        if (value == _DisabledDecimalValue)
          return;
        _DisabledDecimalValue = value;
        if (_DisabledDecimalValueEx != null)
          _DisabledDecimalValueEx.Value = DisabledDecimalValue;
        if (_DisabledIntValueEx != null)
          _DisabledIntValueEx.Value = DisabledIntValue;
        if (_DisabledSingleValueEx != null)
          _DisabledSingleValueEx.Value = DisabledSingleValue;
        if (_DisabledDoubleValueEx != null)
          _DisabledDoubleValueEx.Value = DisabledDoubleValue;
        InitControlValue();
      }
    }
    private decimal _DisabledDecimalValue;

    /// <summary>
    /// Этот текст замещает свойство DecimalValue, когда Enabled=false или ReadOnly=true
    /// Свойство действует при установленном свойстве AllowDisabledText
    /// </summary>
    public DepValue<decimal> DisabledDecimalValueEx
    {
      get
      {
        InitDisabledDecimalValueEx();
        return _DisabledDecimalValueEx;
      }
      set
      {
        InitDisabledDecimalValueEx();
        _DisabledDecimalValueEx.Source = value;
      }
    }
    private DepInput<decimal> _DisabledDecimalValueEx;

    private void InitDisabledDecimalValueEx()
    {
      if (_DisabledDecimalValueEx == null)
      {
        _DisabledDecimalValueEx = new DepInput<decimal>();
        _DisabledDecimalValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledDecimalValueEx");
        _DisabledDecimalValueEx.Value = DisabledDecimalValue;
        _DisabledDecimalValueEx.ValueChanged += new EventHandler(DisabledDecimalValueEx_ValueChanged);
      }
    }

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledDecimalValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledDecimalValue = _DisabledDecimalValueEx.Value;
    }

    /// <summary>
    /// Разрешает использование "серого" значения DisabledDecimalValue.
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

    #region Свойство DisabledIntValue

    /// <summary>
    /// "Серое" значение, приведеденное к целочисленному типу.
    /// </summary>
    public int DisabledIntValue
    {
      get { return (int)(Math.Round(DisabledDecimalValue, 0, MidpointRounding.AwayFromZero)); }
      set { DisabledDecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое свойство DisabledIntValue.
    /// </summary>
    public DepValue<int> DisabledIntValueEx
    {
      get
      {
        InitDisabledIntValueEx();
        return _DisabledIntValueEx;
      }
      set
      {
        InitDisabledIntValueEx();
        _DisabledIntValueEx.Source = value;
      }
    }
    private DepInput<int> _DisabledIntValueEx;

    private void InitDisabledIntValueEx()
    {
      if (_DisabledIntValueEx == null)
      {
        _DisabledIntValueEx = new DepInput<int>();
        _DisabledIntValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledIntValueEx");
        _DisabledIntValueEx.Value = DisabledIntValue;
        _DisabledIntValueEx.ValueChanged += new EventHandler(DisabledIntValueEx_ValueChanged);
      }
    }

    void DisabledIntValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledIntValue = _DisabledIntValueEx.Value;
    }

    #endregion

    #region Свойство DisabledSingleValue

    /// <summary>
    /// "Серое" значение, приведеденное к типу Single.
    /// </summary>
    public float DisabledSingleValue
    {
      get { return (float)DisabledDecimalValue; }
      set { DisabledDecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое свойство DisableSingleValue.
    /// </summary>
    public DepValue<float> DisabledSingleValueEx
    {
      get
      {
        InitDisabledSingleValueEx();
        return _DisabledSingleValueEx;
      }
      set
      {
        InitDisabledSingleValueEx();
        _DisabledSingleValueEx.Source = value;
      }
    }
    private DepInput<float> _DisabledSingleValueEx;

    private void InitDisabledSingleValueEx()
    {
      if (_DisabledSingleValueEx == null)
      {
        _DisabledSingleValueEx = new DepInput<float>();
        _DisabledSingleValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledSingleValueEx");
        _DisabledSingleValueEx.Value = DisabledSingleValue;
        _DisabledSingleValueEx.ValueChanged += new EventHandler(FDisabledSingleValueEx_ValueChanged);
      }
    }

    void FDisabledSingleValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledSingleValue = _DisabledSingleValueEx.Value;
    }

    #endregion

    #region Свойство DisabledDoubleValue

    /// <summary>
    /// "Серое" значение, приведеденное к типу Double.
    /// </summary>
    public double DisabledDoubleValue
    {
      get { return (double)DisabledDecimalValue; }
      set { DisabledDecimalValue = (decimal)value; }
    }

    /// <summary>
    /// Управляемое свойство DisableDoubleValue.
    /// </summary>
    public DepValue<double> DisabledDoubleValueEx
    {
      get
      {
        InitDisabledDoubleValueEx();
        return _DisabledDoubleValueEx;
      }
      set
      {
        InitDisabledDoubleValueEx();
        _DisabledDoubleValueEx.Source = value;
      }
    }
    private DepInput<double> _DisabledDoubleValueEx;

    private void InitDisabledDoubleValueEx()
    {
      if (_DisabledDoubleValueEx == null)
      {
        _DisabledDoubleValueEx = new DepInput<double>();
        _DisabledDoubleValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledDoubleValueEx");
        _DisabledDoubleValueEx.Value = DisabledDoubleValue;
        _DisabledDoubleValueEx.ValueChanged += new EventHandler(DisabledDoubleValueEx_ValueChanged);
      }
    }

    void DisabledDoubleValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledDoubleValue = _DisabledDoubleValueEx.Value;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// true, если можно только просматривать текст, но не редактировать
    /// </summary>
    public bool ReadOnly
    {
      get { return ControlReadOnly; }
      set
      {
        if (value == ControlReadOnly)
          return;
        ControlReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }

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

    /// <summary>
    /// Свойство Control.ReadOnly реализуется по-разному для разных элементов
    /// </summary>
    protected abstract bool ControlReadOnly { get; set; }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Если свойство установлено, то задает минимальное значение которое можно ввести.
    /// Для NumEditBox по умолчанию ограничение не установлено. Для NumericUpoDown и ExtNumericUpoDown 
    /// значение связано со свойством управляющего элемента и по умолчанию имеет значение 0
    /// Если свойство устанолено, то при проверке выдается ошибка или предупреждение, в зависимости от 
    /// свойства WarningIfOutOfRange
    /// </summary>
    public abstract decimal? Minimum { get; set;}

    /// <summary>
    /// Если свойство установлено, то задает максимальное значение которое можно ввести.
    /// Для NumEditBox по умолчанию ограничение не установлено. Для NumericUpDown и ExtNumericUpDown 
    /// значение связано со свойством управляющего элемента и по умолчанию имеет значение 100
    /// Если свойство устанолено, то при проверке выдается ошибка или предупреждение, в зависимости от 
    /// свойства WarningIfOutOfRange
    /// </summary>
    public abstract decimal? Maximum { get; set;}

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// Внимание! Для EFPNumericUpDown свойство нельзя установить в true, т.к. этот элемент в принципе не 
    /// позволяет вводить значения вне диапазона
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Проверка попадания значения в диапазон {Minimum, Maximum}
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>true, если значение попадает в интервал</returns>
    public bool TestRange(decimal value)
    {
      if (Minimum.HasValue)
      {
        if (value < Minimum.Value)
          return false;
      }
      if (Maximum.HasValue)
      {
        if (value < Maximum.Value)
          return false;
      }

      return true;
    }

    /// <summary>
    /// Устанавливает значение, если оно не выходит за разрешенный диапазон
    /// значений (Minimum:Maximum). Иначе не выполняется никаких действий
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    /// <returns>true - значение установлено. false - значение выходит за разрешенный диапазон</returns>
    public bool TrySetDecimalValue(decimal value)
    {
      if (TestRange(value))
      {
        DecimalValue = value;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Устанавливает значение, если оно не выходит за разрешенный диапазон
    /// значений (Minimum:Maximum). Иначе не выполняется никаких действий
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    /// <returns>true - значение установлено. false - значение выходит за разрешенный диапазон</returns>
    public bool TrySetDoubleValue(double value)
    {
      return TrySetDecimalValue((decimal)value);
    }

    /// <summary>
    /// Устанавливает значение, если оно не выходит за разрешенный диапазон
    /// значений (Minimum:Maximum). Иначе не выполняется никаких действий
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    /// <returns>true - значение установлено. false - значение выходит за разрешенный диапазон</returns>
    public bool TrySetSingleValue(float value)
    {
      return TrySetDecimalValue((decimal)value);
    }

    /// <summary>
    /// Устанавливает значение, если оно не выходит за разрешенный диапазон
    /// значений (Minimum:Maximum). Иначе не выполняется никаких действий
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    /// <returns>true - значение установлено. false - значение выходит за разрешенный диапазон</returns>
    public bool TrySetIntValue(int value)
    {
      return TrySetDecimalValue((decimal)value);
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для NumEditBox
  /// </summary>
  public class EFPNumEditBox : EFPNumEditBase<NumEditBox>, IEFPSimpleTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPNumEditBox(EFPBaseProvider baseProvider, NumEditBox control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
      {
        control.ValueChanged += new EventHandler(base.Control_ValueChanged);
        control.TextChanged += Control_TextChanged;
      }

      _Minimum = Decimal.MinValue;
    }

    /// <summary>
    /// Вызывается, когда в управляющем элементе изменяется текст (вызывается чаще,
    /// чем ValueChanged)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        Validate();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика NumEditBox.TextChanged");
      }
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Свойство NumEditBox.Value
    /// </summary>
    protected override decimal ControlDecimalValue
    {
      get { return Control.Value; }
      set { Control.Value = value; }
    }

    /// <summary>
    /// Свойство NumEditBox.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Проверка корректности значения.
    /// Подсвечивает ошибку, если введенный текст нельзя преобразовать в число.
    /// Затем выполняются проверки базового класса (попадание значения в диапазон).
    /// </summary>
    protected override void OnValidate()
    {
      if (!Control.TextIsValid)
      {
        SetError("Введенный текст нельзя преобразовать " + (Control.DecimalPlaces == 0 ? "в целое число" : "в число"));
        return;
      }
      base.OnValidate();
    }

    /// <summary>
    /// Метод вызывается при изменении выбранного значения в управляющем элементе.
    /// </summary>
    protected override void OnValueChanged()
    {
      base.OnValueChanged();
      if (_NullableDecimalValueEx != null)
        _NullableDecimalValueEx.Value = NullableDecimalValue;
      if (_NullableIntValueEx != null)
        _NullableIntValueEx.Value = NullableIntValue;
      if (_NullableSingleValueEx != null)
        _NullableSingleValueEx.Value = NullableSingleValue;
      if (_NullableDoubleValueEx != null)
        _NullableDoubleValueEx.Value = NullableDoubleValue;
    }

    /// <summary>
    /// Создает EFPTextBoxCommandItems.
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
    /// Минимально допустимое значение
    /// </summary>
    public override decimal? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private decimal? _Minimum;

    /// <summary>
    /// Максимально допустимое значение
    /// </summary>
    public override decimal? Maximum
    {
      get { return FMaximum; }
      set { FMaximum = value; }
    }
    private decimal? FMaximum;

    #endregion

    #region Nullable-значения

    /// <summary>
    /// Nullable-значение
    /// </summary>
    public decimal? NullableDecimalValue
    {
      get { return Control.NValue; }
      set { Control.NValue = value; }
    }

    /// <summary>
    /// Управляемое nullable-значение
    /// </summary>
    public DepValue<decimal?> NullableDecimalValueEx
    {
      get
      {
        InitNullableDecimalValueEx();
        return _NullableDecimalValueEx;
      }
      set
      {
        InitNullableDecimalValueEx();
        _NullableDecimalValueEx.Source = value;
      }
    }
    private DepInput<decimal?> _NullableDecimalValueEx;

    private void InitNullableDecimalValueEx()
    {
      if (_NullableDecimalValueEx == null)
      {
        _NullableDecimalValueEx = new DepInput<decimal?>();
        _NullableDecimalValueEx.OwnerInfo = new DepOwnerInfo(this, "NullableDecimalValueEx");
        _NullableDecimalValueEx.Value = NullableDecimalValue;
        _NullableDecimalValueEx.ValueChanged += new EventHandler(NullableDecimalValueEx_ValueChanged);
      }
    }

    void NullableDecimalValueEx_ValueChanged(object sender, EventArgs args)
    {
      NullableDecimalValue = _NullableDecimalValueEx.Value;
    }

    /// <summary>
    /// Nullable-значение, приведенное к целочисленному типу.
    /// </summary>
    public int? NullableIntValue
    {
      get
      {
        if (NullableDecimalValue.HasValue)
          return (int)(NullableDecimalValue.Value);
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          NullableDecimalValue = (decimal)(value.Value);
        else
          NullableDecimalValue = null;
      }
    }

    /// <summary>
    /// Управляемое nullable-значение, приведенное к целочисленному типу.
    /// </summary>
    public DepValue<int?> NullableIntValueEx
    {
      get
      {
        InitNullableIntValueEx();
        return _NullableIntValueEx;
      }
      set
      {
        InitNullableIntValueEx();
        _NullableIntValueEx.Source = value;
      }
    }
    private DepInput<int?> _NullableIntValueEx;

    private void InitNullableIntValueEx()
    {
      if (_NullableIntValueEx == null)
      {
        _NullableIntValueEx = new DepInput<int?>();
        _NullableIntValueEx.OwnerInfo = new DepOwnerInfo(this, "NullableIntValueEx");
        _NullableIntValueEx.Value = NullableIntValue;
        _NullableIntValueEx.ValueChanged += new EventHandler(NullableIntValueEx_ValueChanged);
      }
    }

    void NullableIntValueEx_ValueChanged(object sender, EventArgs args)
    {
      NullableIntValue = _NullableIntValueEx.Value;
    }

    /// <summary>
    /// Nullable-значение, приведенное к типу Single.
    /// </summary>
    public float? NullableSingleValue
    {
      get
      {
        if (NullableDecimalValue.HasValue)
          return (float)(NullableDecimalValue.Value);
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          NullableDecimalValue = (decimal)(value.Value);
        else
          NullableDecimalValue = null;
      }
    }

    /// <summary>
    /// Управляемое nullable-значение, приведенное к типу Single.
    /// </summary>
    public DepValue<float?> NullableSingleDecimalValueEx
    {
      get
      {
        InitNullableSingleValueEx();
        return _NullableSingleValueEx;
      }
      set
      {
        InitNullableSingleValueEx();
        _NullableSingleValueEx.Source = value;
      }
    }
    private DepInput<float?> _NullableSingleValueEx;

    private void InitNullableSingleValueEx()
    {
      if (_NullableSingleValueEx == null)
      {
        _NullableSingleValueEx = new DepInput<float?>();
        _NullableSingleValueEx.OwnerInfo = new DepOwnerInfo(this, "NullableSingleValueEx");
        _NullableSingleValueEx.Value = NullableSingleValue;
        _NullableSingleValueEx.ValueChanged += new EventHandler(NullableSingleValueEx_ValueChanged);
      }
    }

    void NullableSingleValueEx_ValueChanged(object sender, EventArgs args)
    {
      NullableSingleValue = _NullableSingleValueEx.Value;
    }

    /// <summary>
    /// Nullable-значение, приведенное к типу Double.
    /// </summary>
    public double? NullableDoubleValue
    {
      get
      {
        if (NullableDecimalValue.HasValue)
          return (double)(NullableDecimalValue.Value);
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          NullableDecimalValue = (decimal)(value.Value);
        else
          NullableDecimalValue = null;
      }
    }

    /// <summary>
    /// Управляемое nullable-значение, приведенное к типу Double.
    /// </summary>
    public DepValue<double?> NullableDoubleDecimalValueEx
    {
      get
      {
        InitNullableDoubleValueEx();
        return _NullableDoubleValueEx;
      }
      set
      {
        InitNullableDoubleValueEx();
        _NullableDoubleValueEx.Source = value;
      }
    }
    private DepInput<double?> _NullableDoubleValueEx;

    private void InitNullableDoubleValueEx()
    {
      if (_NullableDoubleValueEx == null)
      {
        _NullableDoubleValueEx = new DepInput<double?>();
        _NullableDoubleValueEx.OwnerInfo = new DepOwnerInfo(this, "NullableDoubleValueEx");
        _NullableDoubleValueEx.Value = NullableDoubleValue;
        _NullableDoubleValueEx.ValueChanged += new EventHandler(NullableDoubleValueEx_ValueChanged);
      }
    }

    void NullableDoubleValueEx_ValueChanged(object sender, EventArgs args)
    {
      NullableDoubleValue = _NullableDoubleValueEx.Value;
    }


    #endregion

    #region Интерфейс IEFPSimpleTextBox

    string IEFPSimpleTextBox.Text
    {
      get { return Control.Text; }
      set { Control.Text = value; }
    }

    int IEFPSimpleTextBox.TextLength { get { return Control.TextLength; } }

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
  /// Провайдер для стандартного элемента NumericUpDown
  /// Рекомендуется использовать EFPExtNumericUpDown, который более удобен для пользователя
  /// </summary>
  public class EFPNumericUpDown : EFPNumEditBase<NumericUpDown>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPNumericUpDown(EFPBaseProvider baseProvider, NumericUpDown control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
      {
        control.ValueChanged += new EventHandler(base.Control_ValueChanged);
        control.TextChanged += Control_TextChanged;
        control.MouseWheel += new MouseEventHandler(Control_MouseWheel); // 15.04.2019
      }
      _PrevIncrement = control.Increment;
    }

    /// <summary>
    /// Вызывается, когда в управляющем элементе изменяется текст (вызывается чаще,
    /// чем ValueChanged)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        Validate();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика NumericUpDown.TextChanged");
      }
    }

    /// <summary>
    /// Упрощенная реализация прокрутки колесиком мыши.
    /// В классе UpDownBase выполнется прокрутка, зависящая от системных настроек.
    /// Обычно, за один щелчок колесика выполняется прокрутка на 3 единицы, а не 1, как при нажатии на стрелочки.
    /// Реализуем прокрутку на 1 единицу с помощью обработчика события.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_MouseWheel(object sender, MouseEventArgs args)
    {
      HandledMouseEventArgs Args2 = args as HandledMouseEventArgs;
      if (Args2 == null)
        return;

      // Взято из UpDownBase.OnMouseWheel()
      if ((System.Windows.Forms.Control.ModifierKeys & (Keys.Shift | Keys.Alt)) != 0 ||
        System.Windows.Forms.Control.MouseButtons != MouseButtons.None)
        return; // Do not scroll when Shift or Alt key is down, or when a mouse button is down.

      if (Args2.Delta == 0)
        return;

      if (Args2.Delta > 0)
        Control.UpButton();
      else
        Control.DownButton();

      Args2.Handled = true;
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Свойство NumericUpDown.Value.
    /// </summary>
    protected override decimal ControlDecimalValue
    {
      get { return Control.Value; }
      set
      {
#if DEBUG
        try
        {
#endif
          Control.Value = value;
#if DEBUG
        }
        catch (Exception e)
        {
          e.Data["DisplayName"] = this.DisplayName;
          e.Data["New Value"] = value;
          e.Data["Minimum"] = Control.Minimum;
          e.Data["Maximum"] = Control.Maximum;
          throw;
        }
#endif
      }
    }

    /// <summary>
    /// Сохраняем значение свойства Increment на время установки свойства ReadOnly=true
    /// </summary>
    private decimal _PrevIncrement;

    /// <summary>
    /// Свойство UpDownBase.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set
      {
        if (value == Control.ReadOnly)
          return;

        Control.ReadOnly = value;
        Control.InterceptArrowKeys = !value;
        if (value)
        {
          _PrevIncrement = Control.Increment;
          Control.Increment = 0;
        }
        else
          Control.Increment = _PrevIncrement;
      }
    }

    /// <summary>
    /// Свойство NumericUpDown.Minimum
    /// </summary>
    public override decimal? Minimum
    {
      get { return Control.Minimum; }
      set
      {
        if (value.HasValue)
          Control.Minimum = value.Value;
        else
          Control.Minimum = Decimal.MinValue;
      }
    }

    /// <summary>
    /// Свойство NumericUpDown.Maximum
    /// </summary>
    public override decimal? Maximum
    {
      get { return Control.Maximum; }
      set
      {
        if (value.HasValue)
          Control.Maximum = value.Value;
        else
          Control.Maximum = Decimal.MaxValue;
      }
    }

    /// <summary>
    /// Для EFPNumericUpdDown свойство нельзя устанавливать
    /// </summary>
    public override bool WarningIfOutOfRange
    {
      get { return base.WarningIfOutOfRange; }
      set
      {
        if (value)
          throw new NotSupportedException("Для NumericUpDown нельзя установить выдачу предупреждений, т.к. этот управляющий элемент не позволяет вводить значения вне диапазона");
        base.WarningIfOutOfRange = value;
      }
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для ExtNumericUpDowm.
  /// Содержит конструктор, позволяющий заменить стандартый NumericUpDown на
  /// ExtNumericUpDown
  /// </summary>
  public class EFPExtNumericUpDown : EFPNumEditBase<ExtNumericUpDown>
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер базового класса
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPExtNumericUpDown(EFPBaseProvider baseProvider, ExtNumericUpDown control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
      {
        control.ValueChanged += new EventHandler(base.Control_ValueChanged);
        //Control.TextChanged += Control_TextChanged;
      }
    }

    /// <summary>
    /// Заменяет управляющий элемент NumericUpDown на ExtNumericUpDown и
    /// создает провайдер управляющиего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент, который будет заменен</param>
    public EFPExtNumericUpDown(EFPBaseProvider baseProvider, NumericUpDown control)
      : this(baseProvider, ReplaceToExtControl(control))
    {
    }


    /// <summary>
    /// Заменяет управляющий элемент NumericUpDown на ExtNumericUpDown с помощью
    /// WinFormsTools.ReplaceControl().
    /// Затем устанавливаются идентичные свойства в ExtNumericUpDown.
    /// </summary>
    /// <param name="control">Старый управляющий элемент</param>
    /// <returns>Новый управляющий элемент</returns>
    public static ExtNumericUpDown ReplaceToExtControl(NumericUpDown control)
    {
      ExtNumericUpDown NewControl = new ExtNumericUpDown();
      WinFormsTools.ReplaceControl(control, NewControl);

      NewControl.ReadOnly = control.ReadOnly;
      NewControl.TextAlign = control.TextAlign;
      NewControl.InterceptArrowKeys = control.InterceptArrowKeys;
      NewControl.UpDownAlign = control.UpDownAlign;
      NewControl.Minimum = control.Minimum;
      NewControl.Maximum = control.Maximum;
      NewControl.DecimalPlaces = control.DecimalPlaces;
      NewControl.Value = control.Value;
      if (control.Increment > 0)
        NewControl.Increment = control.Increment;

      return NewControl;
    }

    ///// <summary>
    ///// Вызывается, когда в управляющем элементе изменяется текст (вызывается чаще,
    ///// чем ValueChanged)
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="args"></param>
    //private void Control_TextChanged(object sender, EventArgs args)
    //{
    //  try
    //  {
    //    Validate();
    //  }
    //  catch (Exception e)
    //  {
    //    EFPApp.ShowException(e, "Ошибка обработчика ExtNumericUpDown.TextChanged");
    //  }
    //}

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Свойство ExtNumericUpDown.Value
    /// </summary>
    protected override decimal ControlDecimalValue
    {
      get { return Control.Value; }
      set { Control.Value = value; }
    }

    /// <summary>
    /// Свойство ExtNumericUpDown.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Проверка корректности значения.
    /// Если введенный текст нельзя преобразовать в число, выдается сообщение об ошибке.
    /// Затем вызываются проверки базового класса
    /// </summary>
    protected override void OnValidate()
    {
      if (!Control.TextIsValid)
      {
        SetError("Введенный текст нельзя преобразовать " + (Control.DecimalPlaces == 0 ? "в целое число" : "в число"));
        return;
      }
      base.OnValidate();
    }

    /// <summary>
    /// Минимальное значение
    /// </summary>
    public override decimal? Minimum
    {
      get { return Control.Minimum; }
      set
      {
        if (value.HasValue)
          Control.Minimum = value.Value;
        else
          Control.Minimum = Decimal.MinValue;
      }
    }

    /// <summary>
    /// Максимальное значение
    /// </summary>
    public override decimal? Maximum
    {
      get { return Control.Maximum; }
      set
      {
        if (value.HasValue)
          Control.Maximum = value.Value;
        else
          Control.Maximum = Decimal.MaxValue;
      }
    }

    #endregion
  }
}
