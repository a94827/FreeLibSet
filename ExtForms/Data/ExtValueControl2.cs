// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using FreeLibSet.DependedValues;
using FreeLibSet.IO;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.Data;

// Конкретные реализации ExtValueControl

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Переходник для <see cref="TextBox"/> или любого другого провайдера управляющего элемента,
  /// поддерживающего <see cref="IEFPTextBox"/>.
  /// </summary>
  public class ExtValueTextBox : ExtValueControlBase2<string>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueTextBox(DBxExtValue extValue, IEFPTextBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      UserDisabledValue = String.Empty;

      // На всякий случай
      if (controlProvider is IEFPReadOnlyControl)
        ((IEFPReadOnlyControl)controlProvider).ReadOnly = false;

      SetCurrentValueEx(controlProvider.TextEx);

      if (controlProvider is IEFPReadOnlyControl)
        DepOr.AttachInput(((IEFPReadOnlyControl)controlProvider).ReadOnlyEx, DepNot.NotOutput(EnabledEx));
      else
        DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);

      if (extValue.MaxLength > 0)
        controlProvider.MaxLength = extValue.MaxLength;
      else
        controlProvider.MaxLength = 0; // 20.09.2017. Снимаем ограничение

      _NewLineSubst = '\0';
    }

    #endregion

    #region Свойство

    /// <summary>
    /// Установка значения свойства, отличного от '\0' приводит к замене пары \r\n
    /// в процессе редактирования значения в многострочном редакторе на заданный
    /// символ при его хранении в базе данных
    /// </summary>
    public char NewLineSubst
    {
      get { return _NewLineSubst; }
      set { _NewLineSubst = value; }
    }
    private char _NewLineSubst;
    private string NewLineSubstStr
    {
      get { return new string(_NewLineSubst, 1); }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      string s = ExtValue.AsString;
      if (NewLineSubst != '\0')
        s = s.Replace(NewLineSubstStr, Environment.NewLine);
      CurrentValueEx.Value = s;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      string s = CurrentValueEx.Value;
      if (NewLineSubst != '\0')
        s = s.Replace(Environment.NewLine, NewLineSubstStr);
      ExtValue.SetString(s);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="FreeLibSet.Controls.DateTimeBox"/>.
  /// Поддерживается редактирование даты и/или времени, в зависимости от установленного свойства <see cref="FreeLibSet.Controls.DateTimeBox.Kind"/>.
  /// </summary>
  public class ExtValueDateBox : ExtValueControl<Nullable<DateTime>, EFPDateTimeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDateBox(DBxExtValue extValue, EFPDateTimeBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      //EFPAnd.AttachInput(ControlProvider.EnabledEx, EnabledEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    // /// <summary>
    // /// Формат поля для хранения даты
    // /// </summary>
    // public DocValueDateMode Mode;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsNullableDateTime;
      /*
      switch (Mode)
      {
        case DocValueDateMode.Date:
          CurrentValue.Value = DocValue.AsNullableDate;
          break;
        case DocValueDateMode.DateStr10:
          if (String.IsNullOrEmpty(DocValue.AsString))
            CurrentValue.Value = null;
          else
          {
            DateTime dt;
            if (DataConv.TryDateFromStr10(DocValue.AsString, out dt))
              CurrentValue.Value = dt;
            else
              CurrentValue.Value = null;
          }
          break;
        case DocValueDateMode.DateStrYYYYMMDD:
          if (String.IsNullOrEmpty(DocValue.AsString))
            CurrentValue.Value = null;
          else
          {
            DateTime dt;
            if (DataConv.TryDateFromStrYYYYMMDD(DocValue.AsString, out dt))
              CurrentValue.Value = dt;
            else
              CurrentValue.Value = null;
          }
          break;
        default:
          throw new BugException("Неизвестное значение Mode: " + Mode.ToString());
      }
       * */
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetNullableDateTime(CurrentValueEx.Value);
      /*
      switch (Mode)
      {
        case DocValueDateMode.Date:
          DocValue.AsNullableDate = CurrentValue.Value;
          break;
        case DocValueDateMode.DateStr10:
          if (CurrentValue.Value.HasValue)
            DocValue.AsString = DataConv.DateStr10(CurrentValue.Value.Value);
          else
            DocValue.SetNull();
          break;
        case DocValueDateMode.DateStrYYYYMMDD:
          if (CurrentValue.Value.HasValue)
            DocValue.AsString = DataConv.DateToStrYYYYMMDD(CurrentValue.Value.Value);
          else
            DocValue.SetNull();
          break;
        default:
          throw new BugException("Неизвестное значение Mode: " + Mode.ToString());
      }
       * */
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="FreeLibSet.Controls.DateRangeBox"/>.
  /// Предполагается, что в документе есть два поля, задающие начало и конец диапазона.
  /// </summary>
  public class ExtValueDateRangeBox : TwoExtValueControl<DateTime?, DateTime?, EFPDateRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValueFirst">Доступ к значению поля (первому)</param>
    /// <param name="extValueLast">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDateRangeBox(DBxExtValue extValueFirst, DBxExtValue extValueLast, EFPDateRangeBox controlProvider, bool canMultiEdit)
      : base(extValueFirst, extValueLast, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.First.NValueEx, controlProvider.Last.NValueEx);
      //EFPAnd.AttachInput(ControlProvider.EnabledEx, EnabledEx);
      DepOr.AttachInput(controlProvider.First.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
      DepOr.AttachInput(controlProvider.Last.ReadOnlyEx, DepNot.NotOutput(EnabledEx));

      // 04.02.2012
      // Если редактор периода в-целом скрыт из-за !CanMultiEdit, то надо скрыть и
      // отдельные редакторы дат, т.к. иначе появится сообщение вида
      // "Поле "Начальная дата" должно быть заполнено"
      if (!controlProvider.Visible)
      {
        controlProvider.First.Visible = false;
        controlProvider.Last.Visible = false;
      }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValue1Ex.Value = ExtValue1.AsNullableDateTime;
      CurrentValue2Ex.Value = ExtValue2.AsNullableDateTime;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue1.SetNullableDateTime(CurrentValue1Ex.Value);
      ExtValue2.SetNullableDateTime(CurrentValue2Ex.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPDateOrRangeBox"/>.
  /// Предполагается, что в документе есть два поля, задающие начало и конец диапазона.
  /// </summary>
  public class ExtValueDateOrRangeBox : TwoExtValueControl<DateTime?, DateTime?, EFPDateOrRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValueFirst">Доступ к значению поля (первому)</param>
    /// <param name="extValueLast">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDateOrRangeBox(DBxExtValue extValueFirst, DBxExtValue extValueLast, EFPDateOrRangeBox controlProvider, bool canMultiEdit)
      : base(extValueFirst, extValueLast, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NFirstValueEx, controlProvider.NLastValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (ExtValue1.IsNull || ExtValue2.IsNull)
        ControlProvider.DateRange = DateRange.Empty;
      else
        ControlProvider.DateRange = new DateRange(ExtValue1.AsDateTime, ExtValue2.AsDateTime);
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (ControlProvider.DateRange.IsEmpty)
      {
        ExtValue1.SetNull();
        ExtValue2.SetNull();
      }
      else
      {
        ExtValue1.SetNullableDateTime(ControlProvider.DateRange.FirstDate);
        ExtValue2.SetNullableDateTime(ControlProvider.DateRange.LastDate);
      }
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="FreeLibSet.Controls.MonthDayBox"/> для числового значения, которое хранит номер дня как число в диапазоне от 1 до 365.
  /// </summary>
  public class ExtValueInt32MonthDayBox : ExtValueControl<int, EFPMonthDayBox>/*, IDocValueNullableControl*/
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32MonthDayBox(DBxExtValue extValue, EFPMonthDayBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DayOfYearEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (ExtValue.AsInt32 > 0)
        CurrentValueEx.Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInt32(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="MaskedTextBox"/> и числового значения, которое хранит номер дня как число в диапазоне от 1 до 365.
  /// 0 задает пустое значение.
  /// </summary>
  public class ExtValueInt32MonthDayTextBox : ExtValueControl<int, EFPMonthDayTextBox>/*, IDocValueNullableControl*/
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32MonthDayTextBox(DBxExtValue extValue, EFPMonthDayTextBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DayOfYearEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (ExtValue.AsInt32 > 0)
        CurrentValueEx.Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInt32(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="DateTimePicker"/> для поля данных типа <see cref="DateTime"/>.
  /// Поддерживается редактирование даты и/или времени, в зависимости от настроек <see cref="DateTimePicker"/>.
  /// </summary>
  public class ExtValueDateTimePicker : ExtValueControl<DateTime?, EFPDateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDateTimePicker(DBxExtValue extValue, EFPDateTimePicker controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      DepAnd.AttachInput(ControlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsNullableDateTime;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetNullableDateTime(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="DateTimePicker"/> для поля данных типа <see cref="TimeSpan"/>.
  /// </summary>
  public class ExtValueTimeSpanPicker : ExtValueControl<TimeSpan, EFPDateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueTimeSpanPicker(DBxExtValue extValue, EFPDateTimePicker controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.TimeEx);
      DepAnd.AttachInput(ControlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsTimeSpan;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetTimeSpan(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="ComboBox"/> со стилем <see cref="ComboBoxStyle.DropDownList"/> для числового поля,
  /// которое хранит индекс выбранной позиции в выпадающем списке.
  /// </summary>
  public class ExtValueInt32ListControl : ExtValueControl<int, IEFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32ListControl(DBxExtValue extValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SelectedIndexEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInt32(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="ComboBox"/> со стилем <see cref="ComboBoxStyle.DropDownList"/> для строкового поля.
  /// Текущее значение устанавливается свойством <see cref="EFPListControl.SelectedCode"/>.
  /// </summary>
  public class ExtValueTextListControl : ExtValueControl<string, IEFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueTextListControl(DBxExtValue extValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      UserDisabledValue = String.Empty;

      SetCurrentValueEx(controlProvider.SelectedCodeEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPInt32EditComboBox"/>, предназначенного для ввода целых чисел.
  /// </summary>
  public class ExtValueInt32EditComboBox : ExtValueControl<int, EFPInt32EditComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32EditComboBox(DBxExtValue extValue, EFPInt32EditComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.ValueEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInt32(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник между числовым полем, в котором хранится перечислимое значение, и свойством SelectedCodeEx управляющего элемента
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal sealed class Int32EnumCodeProxy<T>
    where T : struct
  {
    #region Конструктор

    public Int32EnumCodeProxy()
    {
      _Int32ValueEx = new DepInput<int>(0, Int32ValueEx_ValueChanged);
      _Int32ValueEx.OwnerInfo = new DepOwnerInfo(this, "Int32ValueEx");

      _CodeValueEx = new DepInput<string>(String.Empty, CodeValueEx_ValueChanged);
      _CodeValueEx.OwnerInfo = new DepOwnerInfo(this, "CodeValueEx");
    }

    #endregion

    #region DefaultValue

    /// <summary>
    /// Перечислимое значение по умолчанию.
    /// Если свойство не установлено в явном виде, используется первое значение в выпадающем списке
    /// </summary>
    public T DefaultValue
    {
      get { return _DefaultValue; }
      set
      {
        _DefaultValue = value;
        CurrentValue = value;
      }
    }
    private T _DefaultValue;

    #endregion

    #region Вход для числового значения CurrentValueEx

    public DepValue<int> Int32ValueEx
    {
      get { return _Int32ValueEx; }
      set { _Int32ValueEx.Source = value; }
    }
    private DepInput<int> _Int32ValueEx;

    void Int32ValueEx_ValueChanged(object sender, EventArgs args)
    {
      T value = (T)Enum.ToObject(typeof(T), _Int32ValueEx.Value);
      _CodeValueEx.Value = value.ToString();
    }

    public int Int32Value
    {
      get { return _Int32ValueEx.Value; }
      set { _Int32ValueEx.Value = value; }
    }

    /// <summary>
    /// Текущее значение переслимого типа
    /// </summary>
    public T CurrentValue
    {
      get { return (T)Enum.ToObject(typeof(T), _Int32ValueEx.Value); }
      set { _Int32ValueEx.Value = Convert.ToInt32(value); }
    }

    #endregion

    #region Вход для строкового кода SelecedCodeEx

    public DepValue<string> CodeValueEx
    {
      get { return _CodeValueEx; }
      set { _CodeValueEx.Source = value; }
    }
    private DepInput<string> _CodeValueEx;

    void CodeValueEx_ValueChanged(object sender, EventArgs args)
    {
      T value;
      if (String.IsNullOrEmpty(_CodeValueEx.Value))
        value = DefaultValue;
      else
        value = StdConvert.ToEnum<T>(_CodeValueEx.Value);
      _Int32ValueEx.Value = Convert.ToInt32(value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для редактирования числового поля с помощью <see cref="ListBox"/> или <see cref="ComboBox"/>.
  /// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
  /// Значение поля связывается со свойством <see cref="IEFPListControl.SelectedCode"/>, в котором хранится текстовое значение для <see cref="Enum"/>.
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  public class ExtValueInt32EnumCodeListControl<T> : ExtValueControl<int, IEFPListControl>
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32EnumCodeListControl(DBxExtValue extValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      if (controlProvider.Codes == null)
        throw ExceptionFactory.ArgProperty("controlProvider", controlProvider, "Codes", controlProvider.Codes, null);

      _ValueProxy = new Int32EnumCodeProxy<T>();

      if (controlProvider.Codes.Length == 0)
        _ValueProxy.DefaultValue = default(T);
      else
        _ValueProxy.DefaultValue = StdConvert.ToEnum<T>(controlProvider.Codes[0]);
      controlProvider.SelectedCodeEx = _ValueProxy.CodeValueEx;
      _ValueProxy.CodeValueEx = controlProvider.SelectedCodeEx;

      SetCurrentValueEx(_ValueProxy.Int32ValueEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region IntEnumCodeProxy

    private Int32EnumCodeProxy<T> _ValueProxy;

    #endregion

    #region Значение по умолчанию

    /// <summary>
    /// Перечислимое значение по умолчанию.
    /// Если свойство не установлено в явном виде, используется первое значение в выпадающем списке.
    /// </summary>
    public T DefaultValue
    {
      get { return _ValueProxy.DefaultValue; }
      set { _ValueProxy.DefaultValue = value; }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (ExtValue.IsNull)
        _ValueProxy.CurrentValue = _ValueProxy.DefaultValue;
      else
        _ValueProxy.Int32Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInt32(_ValueProxy.Int32ValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для Radio Buttons для логического значения.
  /// Предполагается, что есть две радиокнопки в группе. Первая из них - для
  /// значения false, вторая - для true
  /// </summary>
  public class ExtValueBooleanRadioButtons : ExtValueControl<bool, EFPRadioButtons>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueBooleanRadioButtons(DBxExtValue extValue, EFPRadioButtons controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, false, canMultiEdit)
    {
      if (controlProvider.Controls.Length != 2)
        throw ExceptionFactory.ArgProperty("controlProvider", controlProvider, "Controls.Length", controlProvider.Controls.Length, new object[] { 2 });

      DepInput<bool> currentValueInput = new DepInput<bool>(false, null);
      currentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(currentValueInput);

      DepInput<bool> grayedInput = new DepInput<bool>(false, null);
      grayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = grayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, bool>(CalcSelectedIndex));
      controlProvider.SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (extValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedIndexValueChanged(object sender, EventArgs args)
    {
      if (ExtValue.Grayed)
        Grayed = (ControlProvider.SelectedIndex < 0);
      if (ControlProvider.SelectedIndex >= 0)
        CurrentValueEx.Value = (ControlProvider.SelectedIndex > 0);
      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление положения точки в зависимости от Grayed и текущего значения
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private int CalcSelectedIndex(bool arg1, bool arg2)
    {
      if (arg1)
        return -1;
      else
        return arg2 ? 1 : 0;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (EnabledEx=False), даже в режиме Grayed.
    /// </summary>
    /// <returns></returns>
    protected override bool GetEnabledState()
    {
      return !ExtValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsBoolean;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetBoolean(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="ComboBox"/> со стилем <see cref="ComboBoxStyle.DropDownList"/> или <see cref="ListBox"/> для логического 
  /// значения.
  /// Предполагается, что в списке есть две позиции. Первая из них - для
  /// значения false, вторая - для true.
  /// </summary>
  public class ExtValueBooleanListControl : ExtValueControl<bool, EFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueBooleanListControl(DBxExtValue extValue, EFPListControl controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, false, canMultiEdit)
    {

      DepInput<bool> currentValueInput = new DepInput<bool>(false, null);
      currentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(currentValueInput);

      DepInput<bool> grayedInput = new DepInput<bool>(false, null);
      grayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = grayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, bool>(CalcSelectedIndex));
      controlProvider.SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (extValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedIndexValueChanged(object sender, EventArgs args)
    {
      if (ExtValue.Grayed)
        Grayed = (ControlProvider.SelectedIndex < 0);
      if (ControlProvider.SelectedIndex >= 0)
        CurrentValueEx.Value = (ControlProvider.SelectedIndex > 0);
      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление положения точки в зависимости от Grayed и текущего значения
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private static int CalcSelectedIndex(bool arg1, bool arg2)
    {
      if (arg1)
        return -1;
      else
        return arg2 ? 1 : 0;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (EnabledEx=False), даже в режиме Grayed.
    /// </summary>
    /// <returns></returns>
    protected override bool GetEnabledState()
    {
      return !ExtValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsBoolean;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetBoolean(CurrentValueEx.Value);
    }

    #endregion
  }

  // **********************************************************************
  /// <summary>
  /// Переходник для <see cref="EFPXmlViewBox"/>, хранящий данные в XML поле (как текст)
  /// </summary>
  public class ExtValueXmlViewBox : ExtValueControl<string, EFPXmlViewBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public ExtValueXmlViewBox(DBxExtValue extValue, EFPXmlViewBox controlProvider)
      : base(extValue, controlProvider, true, false)
    {
      SetCurrentValueEx(controlProvider.XmlTextEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }
}
