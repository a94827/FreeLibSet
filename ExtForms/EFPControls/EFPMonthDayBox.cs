// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using FreeLibSet.Controls;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер управляющего элемента для ввода месяца и дня (без года)
  /// Выбор месяца осуществляется с помощью выпадающего списка.
  /// Значение MonthDay.Empty не поддерживается.
  /// </summary>
  public class EFPMonthDayBox : EFPControl<MonthDayBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPMonthDayBox(EFPBaseProvider baseProvider, MonthDayBox control)
      : base(baseProvider, control, true)
    {
      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
    }

    #endregion

    #region Текущие значения

    #region Value

    /// <summary>
    /// Текущее значение как единое целое
    /// </summary>
    public MonthDay Value
    {
      get { return new MonthDay(Control.Month, Control.Day); }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentException(Res.MonthDayBox_Arg_IsEmpty); // если использовать ExceptionFactory.ArgIsEmpty(), то сообщение будет недостаточно понятным
        Control.SetValue(value.Month, value.Day);
      }
    }

    /// <summary>
    /// Месяц и день вместе
    /// </summary>
    public DepValue<MonthDay> ValueEx
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
    private DepInput<MonthDay> _ValueEx;

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<MonthDay>(Value, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
      }
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
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
    /// Метод вызывается при изменении выбранного значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
        _ValueEx.OwnerSetValue(new MonthDay(Control.Month, Control.Day));
      if (_MonthEx != null)
        _MonthEx.OwnerSetValue(Month);
      if (_DayEx != null)
        _DayEx.OwnerSetValue(Day);
      if (_DayOfYearEx != null)
        _DayOfYearEx.OwnerSetValue(DayOfYear);
      Validate();
    }

    #endregion

    #region Month

    /// <summary>
    /// Выбранный месяц (от 1 до 12)
    /// </summary>
    public int Month
    {
      get { return Control.Month; }
      set { Control.Month = value; }
    }

    /// <summary>
    /// Управляемое свойство для Month
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

    #endregion

    #region Day

    /// <summary>
    /// Отдельно день (от 1 до 31)
    /// </summary>
    public int Day
    {
      get { return Control.Day; }
      set { Control.Day = value; }
    }

    /// <summary>
    /// Управляемое свойство для Day
    /// </summary>
    public DepValue<int> DayEx
    {
      get
      {
        InitDayEx();
        return _DayEx;
      }
      set
      {
        InitDayEx();
        _DayEx.Source = value;
      }
    }
    private DepInput<int> _DayEx;

    private void InitDayEx()
    {
      if (_DayEx == null)
      {
        _DayEx = new DepInput<int>(Day,DayEx_ValueChanged);
        _DayEx.OwnerInfo = new DepOwnerInfo(this, "DayEx");
      }
    }

    void DayEx_ValueChanged(object sender, EventArgs args)
    {
      Day = _DayEx.Value;
    }

    #endregion

    #region DayOfYear

    /// <summary>
    /// Выбранный номер дня в году в диапазоне от 1 до 365
    /// </summary>
    public int DayOfYear
    {
      get { return Value.DayOfYear; }
      set
      {
        if (value == 0)
          throw new ArgumentException(Res.MonthDayBox_Arg_IsEmpty);
        Value = new MonthDay(value);
      }
    }

    /// <summary>
    /// Управляемое свойство для Month
    /// </summary>
    public DepValue<int> DayOfYearEx
    {
      get
      {
        InitDayOfYearEx();
        return _DayOfYearEx;
      }
      set
      {
        InitDayOfYearEx();
        _DayOfYearEx.Source = value;
      }
    }
    private DepInput<int> _DayOfYearEx;

    private void InitDayOfYearEx()
    {
      if (_DayOfYearEx == null)
      {
        _DayOfYearEx = new DepInput<int>(DayOfYear, DayOfYearEx_ValueChanged);
        _DayOfYearEx.OwnerInfo = new DepOwnerInfo(this, "DayOfYearEx");
      }
    }

    void DayOfYearEx_ValueChanged(object sender, EventArgs args)
    {
      DayOfYear = _DayOfYearEx.Value;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента для ввода месяца и дня (без года)
  /// Выбор месяца осуществляется с помощью текстового поля в формате "ДД.ММ".
  /// В отличие от EFPMonthDayBox, свойство Value может принимать значение MonthDay.Empty.
  /// </summary>
  public class EFPMonthDayTextBox : EFPControl<MaskedTextBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPMonthDayTextBox(EFPBaseProvider baseProvider, MaskedTextBox control)
      : base(baseProvider, control , true)
    {
      if (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Contains("m/d"))
        Format = "MM/dd";
      else
        Format = "dd/MM";
      if (!DesignMode)
        control.TextChanged += new EventHandler(Control_TextChanged);
      _Value = MonthDay.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Формат даты

    /// <summary>
    /// Используемый формат даты и месяца. 
    /// В зависимости от текущей культуры используется "dd/MM" или "MM/dd"
    /// </summary>
    public string Format
    {
      get { return _Format; }
      set
      {
        switch (value)
        {
          case "dd/MM":
          case "MM/dd":
            break;
          default:
            throw ExceptionFactory.ArgUnknownValue("value", value,new string[] { "dd/MM", "MM/dd"});
        }

        _Format = value;
        StringBuilder sb = new StringBuilder();
        sb.Append("00");
        foreach (char c in CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
        {
          sb.Append('\\');
          sb.Append(c);
        }
        sb.Append("00");
        Control.Mask = sb.ToString();
      }
    }
    private string _Format;

    #endregion

    #region Обработчик управляющего элемента

    private bool _InsideControlTextChanged;

    private bool _ParseError;

    void Control_TextChanged(object sender, EventArgs args)
    {
      if (_InsideControlTextChanged)
        return;
      _InsideControlTextChanged = true;
      try
      {
        _ParseError = false;
        if (Control.MaskCompleted)
        {
          DateTime dt;
          if (DateTime.TryParseExact(Control.Text, Format, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out dt))
            Value = new MonthDay(dt);
          else
          {
            _ParseError = true;
            Value = MonthDay.Empty;
          }
        }
        else
        {
          if (DataTools.ReplaceDigits(Control.Text, '0').IndexOf('0') >= 0)
            _ParseError = true;
          Value = MonthDay.Empty;
        }
      }
      finally
      {
        _InsideControlTextChanged = false;
      }
      Validate();
    }

    #endregion

    #region Текущие значения

    #region Value

    /// <summary>
    /// Текущее значение
    /// </summary>
    public MonthDay Value
    {
      get
      {
        return _Value;
      }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        if (!_InsideControlTextChanged)
        {
          if (value.IsEmpty)
            Control.Text = String.Empty;
          else
          {
            DateTime dt = new DateTime(2001, value.Month, value.Day);
            Control.Text = dt.ToString(Format);
          }
        }
        if (_ValueEx != null)
          _ValueEx.OwnerSetValue(_Value);
        if (_DayOfYearEx != null)
          _DayOfYearEx.OwnerSetValue(DayOfYear);

        if (!_InsideControlTextChanged)
          Validate();
      }
    }
    private MonthDay _Value;

    /// <summary>
    /// Месяц и день вместе
    /// </summary>
    public DepValue<MonthDay> ValueEx
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
    private DepInput<MonthDay> _ValueEx;

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<MonthDay>(Value, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
      }
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    #endregion

    #region DayOfYear

    /// <summary>
    /// Выбранный номер дня в году в диапазоне от 1 до 365
    /// </summary>
    public int DayOfYear
    {
      get { return Value.DayOfYear; }
      set { Value = new MonthDay(value); }
    }

    /// <summary>
    /// Управляемое свойство для Month
    /// </summary>
    public DepValue<int> DayOfYearEx
    {
      get
      {
        InitDayOfYearEx();
        return _DayOfYearEx;
      }
      set
      {
        InitDayOfYearEx();
        _DayOfYearEx.Source = value;
      }
    }
    private DepInput<int> _DayOfYearEx;

    private void InitDayOfYearEx()
    {
      if (_DayOfYearEx == null)
      {
        _DayOfYearEx = new DepInput<int>(DayOfYear, DayOfYearEx_ValueChanged);
        _DayOfYearEx.OwnerInfo = new DepOwnerInfo(this, "DayOfYearEx");
      }
    }

    void DayOfYearEx_ValueChanged(object sender, EventArgs args)
    {
      DayOfYear = _DayOfYearEx.Value;
    }

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое содержит true, если есть введенное значение (Value.IsEmpty=false).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, MonthDay>(ValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(MonthDay value)
    {
      return !value.IsEmpty;
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

    #region Проверка

    /// <summary>
    /// Проверка корректности данных
    /// </summary>
    protected override void OnValidate()
    {
      if (_ParseError)
      {
        SetError("Неправильная дата");
        return;
      }

      if (Value.IsEmpty)
        ValidateCanBeEmptyMode(CanBeEmptyMode);
    }

    #endregion
  }
}
