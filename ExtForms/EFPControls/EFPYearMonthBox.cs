// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Calendar;
using FreeLibSet.UICore;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер для YearMonthBox
  /// </summary>
  public class EFPYearMonthBox : EFPControl<YearMonthBox>
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPYearMonthBox(EFPBaseProvider baseProvider, YearMonthBox control)
      : base(baseProvider, control, true)
    {
      control.MinimumYear = Math.Max(control.MinimumYear, YearMonth.MinYear);
      //control.MaximumYear = Math.Max(control.MinimumYear, YearMonth.MaxYear);
      control.MaximumYear = Math.Min(control.MaximumYear, YearMonth.MaxYear); // 27.12.2020

      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка попадания значения в диапазон
    /// </summary>
    protected override void OnValidate()
    {
      if (ValidateState == UICore.UIValidateState.Error)
        return;

      YearMonth ym2;
      try
      {
        ym2 = YM;
      }
      catch (Exception e) // 15.04.2019
      {
        SetError(e.Message);
        return;
      }

      UITools.ValidateInRange(ym2, Minimum, Maximum, this, DisplayName, true);

      base.OnValidate();
    }

    private static Nullable<YearMonth> ToNullable(YearMonth value)
    {
      if (value.IsEmpty)
        return null;
      else
        return value;
    }

    #endregion

    #region Текущие значения

    /// <summary>
    /// Год
    /// </summary>
    public int Year
    {
      get { return Control.Year; }
      set { Control.Year = value; }
    }

    /// <summary>
    /// Год. Управляемое свойство
    /// </summary>
    public DepValue<int> YearEx
    {
      get
      {
        InitYearEx();
        return _YearEx;
      }
      set
      {
        InitYearEx();
        _YearEx.Source = value;
      }
    }
    private DepInput<int> _YearEx;

    private void InitYearEx()
    {
      if (_YearEx == null)
      {
        _YearEx = new DepInput<int>(Year, YearEx_ValueChanged);
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
      }
    }

    void YearEx_ValueChanged(object sender, EventArgs args)
    {
      Year = _YearEx.Value;
    }

    /// <summary>
    /// Месяц (1 - 12)
    /// </summary>
    public int Month
    {
      get { return Control.Month; }
      set { Control.Month = value; }
    }

    /// <summary>
    /// Месяц. Управляемое свойство
    /// </summary>
    public DepValue<int> MonthEx
    {
      get
      {
        InitMonthEx();
        return _MonthEx;
      }
      set
      {
        InitMonthEx();
        _MonthEx.Source = value;
      }
    }
    private DepInput<int> _MonthEx;

    private void InitMonthEx()
    {
      if (_MonthEx == null)
      {
        _MonthEx = new DepInput<int>(Month, MonthEx_ValueChanged);
        _MonthEx.OwnerInfo = new DepOwnerInfo(this, "MonthEx");
      }
    }

    void MonthEx_ValueChanged(object sender, EventArgs args)
    {
      Month = _MonthEx.Value;
    }


    void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        OnValueChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_YearEx != null)
        _YearEx.OwnerSetValue(Year);
      if (_MonthEx != null)
        _MonthEx.OwnerSetValue(Month);
      if (_YMEx != null)
        _YMEx.OwnerSetValue(YM);

      Validate();
    }

    #endregion

    #region Использование структуры YearMonth

    /// <summary>
    /// Текущее значение (свойства Year и Month) в виде одной структуры.
    /// Не может быть пустое значение YearMonth.Empty
    /// </summary>
    public YearMonth YM
    {
      get { return new YearMonth(Year, Month); }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException();
        Year = value.Year;
        Month = value.Month;
      }
    }

    /// <summary>
    /// Месяц
    /// </summary>
    public DepValue<YearMonth> YMEx
    {
      get
      {
        InitYMEx();
        return _YMEx;
      }
      set
      {
        InitYMEx();
        _YMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _YMEx;

    private void InitYMEx()
    {
      if (_YMEx == null)
      {
        _YMEx = new DepInput<YearMonth>(YM, YMEx_ValueChanged);
        _YMEx.OwnerInfo = new DepOwnerInfo(this, "YMEx");
      }
    }

    void YMEx_ValueChanged(object sender, EventArgs args)
    {
      YM = _YMEx.Value;
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которую можно ввести.
    /// Если значение свойства не равно YearMonth.Empty и свойство <see cref="YM"/> меньше заданной даты, будет выдана ошибка
    /// или предупреждение при проверке контроля.
    /// По умолчанию ограничение не установлено (YearMonth.Empty)
    /// Управляющий элемент имеет свойство <see cref="YearMonthBox.MinimumYear"/>, которое устанавливается синхронно, но установка
    /// <see cref="YearMonthBox.MinimumYear"/> не выполняет обратную установку.
    /// </summary>
    public YearMonth Minimum
    {
      get { return _Minimum; }
      set
      {
        _Minimum = value;
        if (value.IsEmpty)
          Control.MinimumYear = YearMonth.MinYear;
        else
          Control.MinimumYear = value.Year;
      }
    }
    private YearMonth _Minimum;

    /// <summary>
    /// Максимальное значение, которую можно ввести.
    /// Если значение свойства задано и свойство <see cref="YM"/> больше заданной даты, будет выдана ошибка
    /// или предупреждение при проверке контроля.
    /// По умолчанию ограничение не установлено (YearMonth.Empty)
    /// Управляющий элемент имеет свойство <see cref="YearMonthBox.MaximumYear"/>, которое устанавливается синхронно, но установка
    /// <see cref="YearMonthBox.MaximumYear"/> не выполняет обратную установку.
    /// </summary>
    public YearMonth Maximum
    {
      get { return _Maximum; }
      set
      {
        _Maximum = value;
        if (value.IsEmpty)
          Control.MaximumYear = YearMonth.MaxYear;
        else
          Control.MaximumYear = value.Year;
      }
    }
    private YearMonth _Maximum;

    #endregion
  }

  /// <summary>
  /// Переходник для YearMonthRangeBox
  /// </summary>
  public class EFPYearMonthRangeBox : EFPControl<YearMonthRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPYearMonthRangeBox(EFPBaseProvider baseProvider, YearMonthRangeBox control)
      : base(baseProvider, control, true)
    {
      control.MinimumYear = Math.Max(control.MinimumYear, YearMonth.MinYear);
      // control.MaximumYear = Math.Max(control.MinimumYear, YearMonth.MaxYear);
      control.MaximumYear = Math.Min(control.MaximumYear, YearMonth.MaxYear); // 27.12.2020

      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка корректности диапазона и попадания в диапазон значений
    /// </summary>
    protected override void OnValidate()
    {
      if (ValidateState == UICore.UIValidateState.Error)
        return;

      if (LastMonth < FirstMonth)
      {
        SetError(Res.YearMonthBox_Err_Inverted);
        return;
      }

      YearMonth firstYM2, lastYM2;
      try
      {
        firstYM2 = FirstYM;
        lastYM2 = LastYM;
      }
      catch (Exception e) // 15.04.2019
      {
        SetError(e.Message);
        return;
      }

      UITools.ValidateInRange(firstYM2, Minimum, Maximum, this, DisplayName, true);
      UITools.ValidateInRange(lastYM2, Minimum, Maximum, this, DisplayName, true);

      base.OnValidate();
    }

    #endregion

    #region Текущие значения

    #region Year

    /// <summary>
    /// Год
    /// </summary>
    public int Year
    {
      get { return Control.Year; }
      set { Control.Year = value; }
    }

    /// <summary>
    /// Год. Управляемое свойство
    /// </summary>
    public DepValue<int> YearEx
    {
      get
      {
        InitYearEx();
        return _YearEx;
      }
      set
      {
        InitYearEx();
        _YearEx.Source = value;
      }
    }
    private DepInput<int> _YearEx;

    private void InitYearEx()
    {
      if (_YearEx == null)
      {
        _YearEx = new DepInput<int>(Year, YearEx_ValueChanged);
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
      }
    }

    void YearEx_ValueChanged(object sender, EventArgs args)
    {
      Year = _YearEx.Value;
    }

    #endregion

    #region FirstMonth

    /// <summary>
    /// Первый месяц диапазона (1 - 12)
    /// </summary>
    public int FirstMonth
    {
      get { return Control.FirstMonth; }
      set { Control.FirstMonth = value; }
    }

    /// <summary>
    /// Первый месяц. Управляемое свойство
    /// </summary>
    public DepValue<int> FirstMonthEx
    {
      get
      {
        InitFirstMonthEx();
        return _FirstMonthEx;
      }
      set
      {
        InitFirstMonthEx();
        _FirstMonthEx.Source = value;
      }
    }
    private DepInput<int> _FirstMonthEx;

    private void InitFirstMonthEx()
    {
      if (_FirstMonthEx == null)
      {
        _FirstMonthEx = new DepInput<int>(FirstMonth, FirstMonthEx_ValueChanged);
        _FirstMonthEx.OwnerInfo = new DepOwnerInfo(this, "FirstMonthEx");
      }
    }

    void FirstMonthEx_ValueChanged(object sender, EventArgs args)
    {
      FirstMonth = _FirstMonthEx.Value;
    }

    #endregion

    #region LastMonth

    /// <summary>
    /// Последний месяц диапазона (1 - 12)
    /// </summary>
    public int LastMonth
    {
      get { return Control.LastMonth; }
      set { Control.LastMonth = value; }
    }

    /// <summary>
    /// Последний месяц. Управляемое свойство
    /// </summary>
    public DepValue<int> LastMonthEx
    {
      get
      {
        InitLastMonthEx();
        return _LastMonthEx;
      }
      set
      {
        InitLastMonthEx();
        _LastMonthEx.Source = value;
      }
    }
    private DepInput<int> _LastMonthEx;

    private void InitLastMonthEx()
    {
      if (_FirstMonthEx == null)
      {
        _LastMonthEx = new DepInput<int>(LastMonth, LastMonthEx_ValueChanged);
        _LastMonthEx.OwnerInfo = new DepOwnerInfo(this, "LastMonthEx");
      }
    }

    void LastMonthEx_ValueChanged(object sender, EventArgs args)
    {
      LastMonth = _LastMonthEx.Value;
    }

    #endregion

    #region FirstYM

    /// <summary>
    /// Синхронный доступ к свойствам Year и FirstMonth
    /// </summary>
    public YearMonth FirstYM
    {
      get { return new YearMonth(Year, FirstMonth); }
      set { Year = value.Year; FirstMonth = value.Month; }
    }

    /// <summary>
    /// Синхронный доступ к свойствам Year и FirstMonth.
    /// Управляемое свойство
    /// </summary>
    public DepValue<YearMonth> FirstYMEx
    {
      get
      {
        InitFirstYMEx();
        return _FirstYMEx;
      }
      set
      {
        InitFirstYMEx();
        _FirstYMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _FirstYMEx;

    private void InitFirstYMEx()
    {
      if (_FirstYMEx == null)
      {
        _FirstYMEx = new DepInput<YearMonth>(FirstYM, FirstYMEx_ValueChanged);
        _FirstYMEx.OwnerInfo = new DepOwnerInfo(this, "FirstYMEx");
      }
    }

    void FirstYMEx_ValueChanged(object sender, EventArgs args)
    {
      FirstYM = _FirstYMEx.Value;
    }

    #endregion

    #region LastYM

    /// <summary>
    /// Синхронный доступ к свойствам Year и LastMonth
    /// </summary>
    public YearMonth LastYM
    {
      get { return new YearMonth(Year, LastMonth); }
      set { Year = value.Year; LastMonth = value.Month; }
    }

    /// <summary>
    /// Синхронный доступ к свойствам Year и FirstMonth.
    /// Управляемое свойство
    /// </summary>
    public DepValue<YearMonth> LastYMEx
    {
      get
      {
        InitLastYMEx();
        return _LastYMEx;
      }
      set
      {
        InitLastYMEx();
        _LastYMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _LastYMEx;

    private void InitLastYMEx()
    {
      if (_LastYMEx == null)
      {
        _LastYMEx = new DepInput<YearMonth>(LastYM, LastYMEx_ValueChanged);
        _LastYMEx.OwnerInfo = new DepOwnerInfo(this, "LastYMEx");
      }
    }

    void LastYMEx_ValueChanged(object sender, EventArgs args)
    {
      LastYM = _LastYMEx.Value;
    }

    #endregion

    #region DateRange

    /// <summary>
    /// Вспомогательное свойство для чтения/записи значений как интервала дат.
    /// При установке свойства интервал дат должен относиться к одному году
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        return new DateRange(FirstYM.DateRange.FirstDate, LastYM.DateRange.LastDate);
      }
      set
      {
        if (value.IsEmpty)
          throw ExceptionFactory.ArgIsEmpty("value");
        if (value.FirstDate.Year != value.LastDate.Year)
          throw new ArgumentException(Res.YearMonthRangeBox_Arg_DiffYear, "value");

        Year = value.FirstDate.Year;
        FirstMonth = value.FirstDate.Month;
        LastMonth = value.LastDate.Month;
      }
    }

    #endregion

    #region YMRange

    /// <summary>
    /// Период в виде диапазона месяцев
    /// </summary>
    public YearMonthRange YMRange
    {
      get { return new YearMonthRange(FirstYM, LastYM); }
      set
      {
        if (value.FirstYM.Year != value.LastYM.Year)
          throw new ArgumentException(Res.YearMonthRangeBox_Arg_DiffYear, "value");
        Year = value.FirstYM.Year;
        FirstMonth = value.FirstYM.Month;
        LastMonth = value.LastYM.Month;
      }
    }

    #endregion

    #region OnValueChanged

    void Control_ValueChanged(object sender, EventArgs args)
    {
      OnValueChanged();
    }

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_YearEx != null)
        _YearEx.Value = Control.Year;
      if (_FirstMonthEx != null)
        _FirstMonthEx.Value = Control.FirstMonth;
      if (_LastMonthEx != null)
        _LastMonthEx.Value = Control.LastMonth;
      if (_FirstYMEx != null)
        _FirstYMEx.Value = FirstYM;
      if (_LastYMEx != null)
        _LastYMEx.Value = LastYM;

      Validate();
    }

    #endregion

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которую можно ввести.
    /// Если значение свойства не равно YearMonth.Empty и свойство Value меньше заданной даты, будет выдана ошибка
    /// или предупреждение при проверке контроля.
    /// По умолчанию ограничение не установлено (YearMonth.Empty)
    /// Управляющий элемент имеет свойство MinimumYear, которое устанавливается синхронно, но установка
    /// MinimumYear не выполняет обратную установку
    /// </summary>
    public YearMonth Minimum
    {
      get { return _Minimum; }
      set
      {
        _Minimum = value;
        if (value.IsEmpty)
          Control.MinimumYear = YearMonth.MinYear;
        else
          Control.MinimumYear = value.Year;
      }
    }
    private YearMonth _Minimum;

    /// <summary>
    /// Максимальное значение, которую можно ввести.
    /// Если значение свойства не равно YearMonth.Empty и свойство Value больше заданной даты, будет выдана ошибка
    /// или предупреждение при проверке контроля.
    /// По умолчанию ограничение не установлено (YearMonth.Empty)
    /// Управляющий элемент имеет свойство MaximumYear, которое устанавливается синхронно, но установка
    /// MaximumYear не выполняет обратную установку
    /// </summary>
    public YearMonth Maximum
    {
      get { return _Maximum; }
      set
      {
        _Maximum = value;
        if (value.IsEmpty)
          Control.MaximumYear = YearMonth.MaxYear;
        else
          Control.MaximumYear = value.Year;
      }
    }
    private YearMonth _Maximum;

    #endregion
  }

}
