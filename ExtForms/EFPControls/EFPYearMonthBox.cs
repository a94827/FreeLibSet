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
      :base(baseProvider, control, true)
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
      YearMonth YM2;
      try
      {
        YM2 = YM;
      }
      catch(Exception e) // 15.04.2019
      {
        SetError(e.Message);
        return;
      }

      if (!YM2.IsInRange(Minimum, Maximum))
      {
        if (WarningIfOutOfRange)
          SetWarning("Значение должно быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum));
        else
          SetError("Значение должно быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum));
      }

      base.OnValidate();
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
        _YearEx = new DepInput<int>();
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
        _YearEx.OwnerSetValue(Year);
        _YearEx.CheckValue += new DepInputCheckEventHandler<int>(YearEx_CheckValue);
      }
    }

    void YearEx_CheckValue(object sender, DepInputCheckEventArgs<int> args)
    {
      Year = args.NewValue;
      args.Cancel = true;
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
        _MonthEx = new DepInput<int>();
        _MonthEx.OwnerInfo = new DepOwnerInfo(this, "MonthEx");
        _MonthEx.OwnerSetValue(Month);
        _MonthEx.CheckValue += new DepInputCheckEventHandler<int>(MonthEx_CheckValue);
      }
    }

    void MonthEx_CheckValue(object sender, DepInputCheckEventArgs<int> args)
    {
      Month = args.NewValue;
      args.Cancel = true;
    }


    void Control_ValueChanged(object sender, EventArgs args)
    {
      try
      {
        OnValueChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика YearMonthBox.ValueChanged");
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
        _YMEx = new DepInput<YearMonth>();
        _YMEx.OwnerInfo = new DepOwnerInfo(this, "YMEx");
        _YMEx.OwnerSetValue(YM);
        _YMEx.CheckValue += new DepInputCheckEventHandler<YearMonth>(YMEx_CheckValue);
      }
    }

    void YMEx_CheckValue(object sender, DepInputCheckEventArgs<YearMonth> args)
    {
      YM = args.NewValue;
      args.Cancel = true;
    }

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
      :base(baseProvider, control, true)
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
      if (LastMonth < FirstMonth)
      {
        SetError("Начальный месяц больше конечного");
        return;
      }

      YearMonth FirstYM2, LastYM2;
      try
      {
        FirstYM2 = FirstYM;
        LastYM2 = LastYM;
      }
      catch (Exception e) // 15.04.2019
      {
        SetError(e.Message);
        return;
      }

      if (!(FirstYM2.IsInRange(Minimum, Maximum) && LastYM2.IsInRange(Minimum, Maximum)))
      {
        if (WarningIfOutOfRange)
          SetWarning("Значение должно быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum));
        else
          SetError("Значение должно быть в диапазоне " + DateRangeFormatter.Default.ToString(Minimum, Maximum));
      }

      base.OnValidate();
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
        _YearEx = new DepInput<int>();
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
        _YearEx.OwnerSetValue(Year);
        _YearEx.CheckValue += new DepInputCheckEventHandler<int>(YearEx_CheckValue);
      }
    }

    void YearEx_CheckValue(object sender, DepInputCheckEventArgs<int> args)
    {
      Year = args.NewValue;
      args.Cancel = true;
    }

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
        InitMonthEx();
        return _FirstMonthEx;
      }
      set
      {
        InitMonthEx();
        _FirstMonthEx.Source = value;
      }
    }
    private DepInput<int> _FirstMonthEx;

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
        InitMonthEx();
        return _LastMonthEx;
      }
      set
      {
        InitMonthEx();
        _LastMonthEx.Source = value;
      }
    }
    private DepInput<int> _LastMonthEx;

    private void InitMonthEx()
    {
      if (_FirstMonthEx == null)
      {
        _FirstMonthEx = new DepInput<int>();
        _FirstMonthEx.OwnerInfo = new DepOwnerInfo(this, "FirstMonthEx");
        _FirstMonthEx.OwnerSetValue(FirstMonth);
        _FirstMonthEx.CheckValue += new DepInputCheckEventHandler<int>(FirstMonthEx_CheckValue);

        _LastMonthEx = new DepInput<int>();
        _LastMonthEx.OwnerInfo = new DepOwnerInfo(this, "LastMonthEx");
        _LastMonthEx.OwnerSetValue(LastMonth);
        _LastMonthEx.CheckValue += new DepInputCheckEventHandler<int>(FLastMonthEx_CheckValue);
      }
    }

    void FirstMonthEx_CheckValue(object sender, DepInputCheckEventArgs<int> args)
    {
      FirstMonth = args.NewValue;
      args.Cancel = true;
    }

    void FLastMonthEx_CheckValue(object sender, DepInputCheckEventArgs<int> args)
    {
      LastMonth = args.NewValue;
      args.Cancel = true;
    }

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
      if (_YMRangeEx != null)
        _YMRangeEx.Value = YMRange;

      Validate();
    }

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

    #region Структуры YearMonth и YearMonthRange

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
        _FirstYMEx = new DepInput<YearMonth>();
        _FirstYMEx.OwnerInfo = new DepOwnerInfo(this, "FirstYMEx");
        _FirstYMEx.OwnerSetValue(FirstYM);
        _FirstYMEx.CheckValue += new DepInputCheckEventHandler<YearMonth>(FirstYMEx_CheckValue);
      }
    }

    void FirstYMEx_CheckValue(object sender, DepInputCheckEventArgs<YearMonth> args)
    {
      FirstYM = args.NewValue;
      args.Cancel = true;
    }

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
        _LastYMEx = new DepInput<YearMonth>();
        _LastYMEx.OwnerInfo = new DepOwnerInfo(this, "LastYMEx");
        _LastYMEx.OwnerSetValue(LastYM);
        _LastYMEx.CheckValue += new DepInputCheckEventHandler<YearMonth>(LastYMEx_CheckValue);
      }
    }

    void LastYMEx_CheckValue(object sender, DepInputCheckEventArgs<YearMonth> args)
    {
      LastYM = args.NewValue;
      args.Cancel = true;
    }

    /// <summary>
    /// Период в виде диапазона месяцев
    /// </summary>
    public YearMonthRange YMRange
    {
      get { return new YearMonthRange(FirstYM, LastYM); }
      set 
      {
        if (value.FirstYM.Year != value.LastYM.Year)
          throw new ArgumentException("Диапазон должен относится к одному году");
        Year = value.FirstYM.Year;
        FirstMonth = value.FirstYM.Month;
        LastMonth = value.LastYM.Month;
      }
    }

    /// <summary>
    /// Период в виде диапазона месяцев
    /// </summary>
    public DepValue<YearMonthRange> YMRangeEx
    {
      get
      {
        InitYMRangeEx();
        return _YMRangeEx;
      }
      set
      {
        InitYMRangeEx();
        _YMRangeEx.Source = value;
      }
    }
    private DepInput<YearMonthRange> _YMRangeEx;

    private void InitYMRangeEx()
    {
      if (_YMRangeEx == null)
      {
        _YMRangeEx = new DepInput<YearMonthRange>();
        _YMRangeEx.OwnerInfo = new DepOwnerInfo(this, "YMRangeEx");
        _YMRangeEx.OwnerSetValue(YMRange);
        _YMRangeEx.CheckValue += new DepInputCheckEventHandler<YearMonthRange>(YMRangeEx_CheckValue);
      }
    }

    void YMRangeEx_CheckValue(object sender, DepInputCheckEventArgs<YearMonthRange> args)
    {
      YMRange = args.NewValue;
      args.Cancel = true;
    }

    #endregion
  }

}
