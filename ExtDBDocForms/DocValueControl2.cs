// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using System.Xml;
using FreeLibSet.IO;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

// Конкретные реализации DocValueControl

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Переходник для TextBox или любого другого провайдера управляющего элемента,
  /// поддерживающего ITextBox
  /// </summary>
  public class DocValueTextBox : DocValueControlBase2<string>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueTextBox(DBxDocValue docValue, IEFPTextBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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

      if (docValue.MaxLength > 0)
        controlProvider.MaxLength = docValue.MaxLength;
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
      string s = DocValue.AsString;
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
      DocValue.SetString(s);
    }

    #endregion
  }


#if XXXX
  /// <summary>
  /// Способ хранения даты в поле (свойство DocValueDateBox.Mode)
  /// </summary>
  public enum DocValueDateMode
  {
    /// <summary>
    /// Обычное поле для хранения даты
    /// </summary>
    Date,

    /// <summary>
    /// Текстовое поле для хранения даты в формате ДД.ММ.ГГГГ
    /// </summary>
    DateStr10,

    /// <summary>
    /// Текстовое поле для хранения даты в формате ГГГГММДД
    /// </summary>
    DateStrYYYYMMDD
  }
#endif

  /// <summary>
  /// Переходник для DateBox
  /// </summary>
  public class DocValueDateBox : DocValueControl<Nullable<DateTime>, EFPDateTimeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDateBox(DBxDocValue docValue, EFPDateTimeBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsNullableDateTime;
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
      DocValue.SetNullableDateTime(CurrentValueEx.Value);
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
  /// Переходник для DateRangeBox
  /// </summary>
  public class DocValueDateRangeBox : TwoDocValueControl<DateTime?, DateTime?, EFPDateRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValueFirst">Доступ к значению поля (первому)</param>
    /// <param name="docValueLast">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDateRangeBox(DBxDocValue docValueFirst, DBxDocValue docValueLast, EFPDateRangeBox controlProvider, bool canMultiEdit)
      : base(docValueFirst, docValueLast, controlProvider, true, canMultiEdit)
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
      CurrentValue1Ex.Value = DocValue1.AsNullableDateTime;
      CurrentValue2Ex.Value = DocValue2.AsNullableDateTime;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue1.SetNullableDateTime(CurrentValue1Ex.Value);
      DocValue2.SetNullableDateTime(CurrentValue2Ex.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для DateOrRangeBox
  /// </summary>
  public class DocValueDateOrRangeBox : TwoDocValueControl<DateTime?, DateTime?, EFPDateOrRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValueFirst">Доступ к значению поля (первому)</param>
    /// <param name="docValueLast">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDateOrRangeBox(DBxDocValue docValueFirst, DBxDocValue docValueLast, EFPDateOrRangeBox controlProvider, bool canMultiEdit)
      : base(docValueFirst, docValueLast, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NFirstDateEx, controlProvider.NLastDateEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (DocValue1.IsNull || DocValue2.IsNull)
        ControlProvider.DateRange = DateRange.Empty;
      else
        ControlProvider.DateRange = new DateRange(DocValue1.AsDateTime, DocValue2.AsDateTime);
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (ControlProvider.DateRange.IsEmpty)
      {
        DocValue1.SetNull();
        DocValue2.SetNull();
      }
      else
      {
        DocValue1.SetNullableDateTime(ControlProvider.DateRange.FirstDate);
        DocValue2.SetNullableDateTime(ControlProvider.DateRange.LastDate);
      }
    }

    #endregion
  }

  /// <summary>
  /// Переходник для MonthDayBox для числового значения, которое хранит номер дня как число в диапазоне от 1 до 365
  /// </summary>
  public class DocValueIntMonthDayBox : DocValueControl<int, EFPMonthDayBox>/*, IDocValueNullableControl*/
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntMonthDayBox(DBxDocValue docValue, EFPMonthDayBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      if (DocValue.AsInteger > 0)
        CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для MaskedTextBox для числового значения, которое хранит номер дня как число в диапазоне от 1 до 365.
  /// 0 задает пустое значение
  /// </summary>
  public class DocValueIntMonthDayTextBox : DocValueControl<int, EFPMonthDayTextBox>/*, IDocValueNullableControl*/
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntMonthDayTextBox(DBxDocValue docValue, EFPMonthDayTextBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      if (DocValue.AsInteger > 0)
        CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для DateTimePicker для поля данных типа DateTime
  /// </summary>
  public class DocValueDateTimePicker : DocValueControl<DateTime?, EFPDateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDateTimePicker(DBxDocValue docValue, EFPDateTimePicker controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsNullableDateTime;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetNullableDateTime(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для DateTimePicker для поля данных типа TimeSpan
  /// </summary>
  public class DocValueTimeSpanPicker : DocValueControl<TimeSpan, EFPDateTimePicker>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueTimeSpanPicker(DBxDocValue docValue, EFPDateTimePicker controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsTimeSpan;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetTimeSpan(CurrentValueEx.Value);
    }

    #endregion
  }


  /// <summary>
  /// Переходник для ComboBox со стилем DropDownList для числового значения
  /// </summary>
  public class DocValueIntListControl : DocValueControl<int, IEFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntListControl(DBxDocValue docValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для ComboBox со стилем DropDownList для строкового значения
  /// Текущее значение устанавливается свойством EFPListControl.SelectedCode
  /// </summary>
  public class DocValueTextListControl : DocValueControl<string, IEFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueTextListControl(DBxDocValue docValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }


  /// <summary>
  /// Переходник для IntEditComboBox, предназначенного для ввода целых чисел
  /// </summary>
  public class DocValueIntEditComboBox : DocValueControl<int, EFPIntEditComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntEditComboBox(DBxDocValue docValue, EFPIntEditComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
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
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник между числовым полем, в котором хранится перечислимое значение, и свойством SelectedCodeEx управляющего элемента
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal sealed class IntEnumCodeProxy<T>
    where T : struct
  {
    #region Конструктор

    public IntEnumCodeProxy()
    {
      _IntValueEx = new DepInput<int>(0, IntValueEx_ValueChanged);
      _IntValueEx.OwnerInfo = new DepOwnerInfo(this, "IntValueEx");

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

    public DepValue<int> IntValueEx
    {
      get { return _IntValueEx; }
      set { _IntValueEx.Source = value; }
    }
    private DepInput<int> _IntValueEx;

    void IntValueEx_ValueChanged(object sender, EventArgs args)
    {
      T value = (T)Enum.ToObject(typeof(T), _IntValueEx.Value);
      _CodeValueEx.Value = value.ToString();
    }

    public int IntValue
    {
      get { return _IntValueEx.Value; }
      set { _IntValueEx.Value = value; }
    }

    /// <summary>
    /// Текущее значение переслимого типа
    /// </summary>
    public T CurrentValue
    {
      get { return (T)Enum.ToObject(typeof(T), _IntValueEx.Value); }
      set { _IntValueEx.Value = Convert.ToInt32(value); }
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
      _IntValueEx.Value = Convert.ToInt32(value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для редактирования числового поля с помощью ListBox илм ComboBox.
  /// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
  /// Значение поля связывается со свойством SelectedCode, в котором хранится текстовое значение для enum.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class DocValueIntEnumCodeListControl<T> : DocValueControl<int, IEFPListControl>
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntEnumCodeListControl(DBxDocValue docValue, IEFPListControl controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      if (controlProvider.Codes == null)
        throw new ArgumentNullException("Должно быть установлено свойство Codes", "controlProvider");

      _ValueProxy = new IntEnumCodeProxy<T>();

      if (controlProvider.Codes.Length == 0)
        _ValueProxy.DefaultValue = default(T);
      else
        _ValueProxy.DefaultValue = StdConvert.ToEnum<T>(controlProvider.Codes[0]);
      controlProvider.SelectedCodeEx = _ValueProxy.CodeValueEx;
      _ValueProxy.CodeValueEx = controlProvider.SelectedCodeEx;

      SetCurrentValueEx(_ValueProxy.IntValueEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region IntEnumCodeProxy

    private IntEnumCodeProxy<T> _ValueProxy;

    #endregion

    #region Значение по умолчанию

    /// <summary>
    /// Перечислимое значение по умолчанию.
    /// Если свойство не установлено в явном виде, используется первое значение в выпадающем списке
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
      if (DocValue.IsNull)
        _ValueProxy.CurrentValue = _ValueProxy.DefaultValue;
      else
        _ValueProxy.IntValue = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(_ValueProxy.IntValueEx.Value);
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Переходник для Radio Buttons для числового значения
  /// </summary>
  public class DocValueIntRadioButtons : DocValueControl<int, EFPRadioButtons>
  {
  #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntRadioButtons(DBxDocValue docValue, EFPRadioButtons controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, false, canMultiEdit)
    {
      if (canMultiEdit && docValue.Grayed)
        controlProvider.CanBeEmpty = true; // 23.07.2019

      DepInput<int> CurrentValueInput = new DepInput<int>();
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>();
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, int>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, int>(CalcSelectedIndex));
      controlProvider.SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (docValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedIndexValueChanged(object sender, EventArgs args)
    {
      if (DocValue.Grayed)
        GrayedEx.Value = (ControlProvider.SelectedIndex < 0);
      if (ControlProvider.SelectedIndex >= 0)
        CurrentValueEx.Value = ControlProvider.SelectedIndex;
      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление положения точки в зависимости от Grayed и текущего значения
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private int CalcSelectedIndex(bool arg1, int arg2)
    {
      if (arg1)
        return -1;
      else
        return arg2;
    }

  #endregion

  #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (Enabled=False), даже в режиме Grayed.
    /// </summary>
    /// <returns></returns>
    protected override bool GetEnabledState()
    {
      return !DocValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

  #endregion
  }

  /// <summary>
  /// Переходник для Radio Buttons для строкового значения кода подстановки
  /// </summary>
  public class DocValueTextRadioButtons : DocValueControl<string, EFPRadioButtons>
  {
  #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueTextRadioButtons(DBxDocValue docValue, EFPRadioButtons controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, false, canMultiEdit)
    {
      UserDisabledValue = String.Empty;

      DepInput<string> CurrentValueInput = new DepInput<string>();
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>();
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, string>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, string>(CalcSelectedIndex));
      controlProvider.SelectedCodeEx.ValueChanged += new EventHandler(SelectedCodeValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (docValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedCodeValueChanged(object sender, EventArgs args)
    {
      // Надо обязательно обрабатывать SelectedCodeEx.ValueChanged, а не SelectedIndexEx.ValueChanged,
      // иначе будет получено неверное значение
      if (DocValue.Grayed)
        GrayedEx.Value = (ControlProvider.SelectedIndex < 0);
      if (ControlProvider.SelectedIndex >= 0)
        CurrentValueEx.Value = ControlProvider.SelectedCode;
      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление положения точки в зависимости от Grayed и текущего значения
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private int CalcSelectedIndex(bool arg1, string arg2)
    {
      if (arg1)
        return -1;
      else
        return Array.IndexOf<string>(ControlProvider.Codes, arg2);
    }

  #endregion

  #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (EnabledEx=False), даже в режиме Grayed.
    /// </summary>
    /// <returns></returns>
    protected override bool GetEnabledState()
    {
      return !DocValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetString(CurrentValueEx.Value);
    }

  #endregion
  }
#endif

#if XXX
  /// <summary>
  /// Переходник для редактирования числового поля с помощью группы радиокнопок.
  /// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
  /// Значение поля связывается со свойством SelectedCode, в котором хранится текстовое значение для enum.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class DocValueIntEnumCodeRadioButtons<T> : DocValueControl<int, EFPRadioButtons>
    where T : struct
  {
  #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntEnumCodeRadioButtons(DBxDocValue docValue, EFPRadioButtons controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      if (controlProvider.Codes == null)
        throw new ArgumentNullException("Должно быть установлено свойство Codes", "controlProvider");

      _ValueProxy = new IntEnumCodeProxy<T>();

      if (controlProvider.Codes.Length == 0)
        _ValueProxy.DefaultValue = default(T);
      else
        _ValueProxy.DefaultValue = StdConvert.ToEnum<T>(controlProvider.Codes[0]);
      controlProvider.SelectedCodeEx = _ValueProxy.CodeValueEx;
      _ValueProxy.CodeValueEx = controlProvider.SelectedCodeEx;

      SetCurrentValueEx(_ValueProxy.IntValueEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

  #endregion

  #region IntEnumCodeProxy

    private IntEnumCodeProxy<T> _ValueProxy;

  #endregion

  #region Значение по умолчанию

    /// <summary>
    /// Перечислимое значение по умолчанию.
    /// Если свойство не установлено в явном виде, используется первое значение в выпадающем списке
    /// </summary>
    public T DefaultValue
    {
      get { return _ValueProxy.DefaultValue; }
      set { _ValueProxy.DefaultValue = value; }
    }
    private T _DefaultValue;

  #endregion

  #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (DocValue.IsNull)
        _ValueProxy.CurrentValue = _ValueProxy.DefaultValue;
      else
        _ValueProxy.IntValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(_ValueProxy.IntValueEx.Value);
    }

  #endregion
  }
#endif

  /// <summary>
  /// Переходник для Radio Buttons для логического значения
  /// Предполагается, что есть две радиокнопки в группе. Первая из них - для
  /// значения false, вторая - для true
  /// </summary>
  public class DocValueBoolRadioButtons : DocValueControl<bool, EFPRadioButtons>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueBoolRadioButtons(DBxDocValue docValue, EFPRadioButtons controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, false, canMultiEdit)
    {
      if (controlProvider.Controls.Length != 2)
        throw new ArgumentException("Группа должна состоять из двух радиокнопок", "controlProvider");

      DepInput<bool> CurrentValueInput = new DepInput<bool>(false, null);
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>(false, null);
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, bool>(CalcSelectedIndex));
      controlProvider.SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (docValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedIndexValueChanged(object sender, EventArgs args)
    {
      if (DocValue.Grayed)
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
      return !DocValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsBoolean;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetBoolean(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для Combobox'а со стилем DropDownList или ListBox'а для логического 
  /// значения
  /// Предполагается, что в выпадющем списке есть две позиции. Первая из них - для
  /// значения false, вторая - для true
  /// </summary>
  public class DocValueBoolListControl : DocValueControl<bool, EFPListControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueBoolListControl(DBxDocValue docValue, EFPListControl controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, false, canMultiEdit)
    {

      DepInput<bool> CurrentValueInput = new DepInput<bool>(false, null);
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>(false, null);
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.SelectedIndexEx = new DepExpr2<int, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<int, bool, bool>(CalcSelectedIndex));
      controlProvider.SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
      if (docValue.Grayed)
        controlProvider.CanBeEmpty = true;
    }

    void SelectedIndexValueChanged(object sender, EventArgs args)
    {
      if (DocValue.Grayed)
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
      return !DocValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsBoolean;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetBoolean(CurrentValueEx.Value);
    }

    #endregion
  }

#if XXXX
  /// <summary>
  /// Переходник для хранения фото и его миниатюры 160x160
  /// </summary>
  public class DocValuePhotoButton : IDocEditItem
  {
  #region Конструктор

    public DocValuePhotoButton(string TableName, IDocValues Values, EFPThumbnailPictureButton ControlProvider)
    {
      FTableName = TableName;
      FMainValue = Values["Фото"];
      FThumbnailValue = Values["ФотоМиниатюра"];
      FControlProvider = ControlProvider;
      ControlProvider.NoImageText = "Нет фото";
      ControlProvider.ReadOnly = Values.DataReadOnly;
      ControlProvider.ImageChanged += new EventHandler(ControlProvider_ImageChanged);
      ControlProvider.MainImageNeeded += new EventHandler(ControlProvider_MainImageNeeded);

      FChangeInfo = new DepChangeInfoItem();
      FChangeInfo.DisplayName = "Фото";
    }

    void ControlProvider_ImageChanged(object Sender, EventArgs Args)
    {
      if (FChangeInfo != null)
        FChangeInfo.Changed = true;
    }

    void ControlProvider_MainImageNeeded(object Sender, EventArgs Args)
    {
      if (FChangeInfo == null)
        return;
      if (FChangeInfo.Changed)
        return; // стерто

      Int32 FileId = FMainValue.AsInteger;
      if (FileId == 0)
        return;

      try
      {
        AccDepFileContainer File = AccDepClientExec.GetDBFile(FTableName, FMainValue.Name, FileId);
        MemoryStream strm = new MemoryStream(File.Contents);
        try
        {
          FControlProvider.MainImage = Image.FromStream(strm);
        }
        finally
        {
          strm.Dispose();
        }
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Ошибка загрузки фото");
      }
    }

  #endregion

  #region Свойства

    string FTableName;

    IDocValue FMainValue;

    IDocValue FThumbnailValue;

    EFPThumbnailPictureButton FControlProvider;

  #endregion

  #region IDocEditItem Members

    public void BeforeReadValues()
    {
    }

    public void ReadValues()
    {
      FControlProvider.Clear();
      Int32 FileId = FThumbnailValue.AsInteger;
      if (FileId != 0)
      {
        try
        {
          AccDepFileContainer File = AccDepClientExec.GetDBFile(FTableName, FThumbnailValue.Name, FileId);
          MemoryStream strm = new MemoryStream(File.Contents);
          try
          {
            FControlProvider.ThumbnailImage = Image.FromStream(strm);
          }
          finally
          {
            strm.Dispose();
          }
        }
        catch (Exception e)
        {
          DebugTools.ShowException(e, "Ошибка загрузки миниатюры фото");
        }
      }
      if (FChangeInfo != null)
        FChangeInfo.Changed = false;
    }

    public void AfterReadValues()
    {
    }

    public void WriteValues()
    {
      if (FChangeInfo == null)
        return;

      if (!FChangeInfo.Changed)
        return;

      if (FControlProvider.MainImage == null)
      {
        FMainValue.SetNull();
        FThumbnailValue.SetNull();
      }
      else
      {
        MemoryStream strm1 = new MemoryStream();
        try
        {
          FControlProvider.MainImage.Save(strm1, ImageFormat.Jpeg);
          AccDepFileContainer File = new AccDepFileContainer(FControlProvider.MainImage.Width.ToString() + "x" +
            FControlProvider.MainImage.Height.ToString() + ".jpg", strm1.GetBuffer());
          FMainValue.AsInteger = AccDepClientExec.AppendDBFile(FTableName, FMainValue.Name, File);
        }
        finally
        {
          strm1.Dispose();
        }

        MemoryStream strm2 = new MemoryStream();
        try
        {
          FControlProvider.ThumbnailImage.Save(strm2, ImageFormat.Jpeg);
          AccDepFileContainer File = new AccDepFileContainer(FControlProvider.ThumbnailImage.Width.ToString() + "x" +
            FControlProvider.ThumbnailImage.Height.ToString() + ".jpg", strm2.GetBuffer());
          FThumbnailValue.AsInteger = AccDepClientExec.AppendDBFile(FTableName, FThumbnailValue.Name, File);
        }
        finally
        {
          strm2.Dispose();
        }
      }

      FChangeInfo.Changed = false;
    }

    public DepChangeInfo ChangeInfo { get { return FChangeInfo; } }
    private DepChangeInfoItem FChangeInfo;

  #endregion
  }
#endif

  // ******************************************************************************
  // Переходники для управляющих элементов, объявленных в EFPDocComboBox

  /// <summary>
  /// Базовый класс переходника для управляющего элемента, производного от EFPDocComboBoxBase.
  /// Управляет очисткой свойства EmptyText, если в элементе выведены "серые" значения
  /// </summary>
  /// <typeparam name="TValue">Тип редактируемого значения (Int32)</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента, производный от EFPAnyDocComboBoxBase</typeparam>
  public abstract class DocValueAnyDocComboBoxBase<TValue, TControlProvider> : DocValueControl<TValue, TControlProvider>
    where TControlProvider : EFPAnyDocComboBoxBase
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueAnyDocComboBoxBase(DBxDocValue docValue, TControlProvider controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      DepAnd.AttachInput(controlProvider.SelectableEx, EnabledEx);

      if (canMultiEdit)
      {
        _OrgEmptyText = controlProvider.EmptyText;
        controlProvider.EmptyTextEx.ValueChanged += new EventHandler(EmptyTextEx_ValueChanged);
      }
      else
        _OrgEmptyText = null; // маркер !CanMultiEdit
      _InsideInitEnabled = false;
    }

    #endregion

    #region Управление свойством EmptyText

    private string _OrgEmptyText;

    private bool _InsideInitEnabled;

    void EmptyTextEx_ValueChanged(object sender, EventArgs args)
    {
      if (_InsideInitEnabled)
        return;
      _OrgEmptyText = ControlProvider.EmptyText;
    }

    /// <summary>
    /// Вызывается при изменении состояния Grayed или UserEnabled
    /// </summary>
    protected override void InitEnabled()
    {
      base.InitEnabled();

      if (_InsideInitEnabled)
        return; // по идее, никогда не должно быть

      if (_OrgEmptyText != null)
      {
        _InsideInitEnabled = true;
        try
        {
          // Метод вызывается, в частности, при изменении свойства Grayed
          if (base.GrayedEx.Value)
            ControlProvider.EmptyText = String.Empty;
          else
            ControlProvider.EmptyText = _OrgEmptyText;
        }
        finally
        {
          _InsideInitEnabled = false;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Переходник для EFPDocComboBox
  /// </summary>
  public class DocValueDocComboBox : DocValueAnyDocComboBoxBase<Int32, EFPDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDocComboBox(DBxDocValue docValue, EFPDocComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (CurrentValueEx.Value == 0)
        DocValue.SetNull();
      else
        DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для EFPSubDocComboBox
  /// </summary>
  public class DocValueSubDocComboBox : DocValueAnyDocComboBoxBase<Int32, EFPSubDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueSubDocComboBox(DBxDocValue docValue, EFPSubDocComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SubDocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (CurrentValueEx.Value == 0)
        DocValue.SetNull();
      else
        DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

#if XXXX
  // ??? Сомнительная реализация
  public class DocValueDocComboBoxByTableId : DocValueControl<int, EFPDocComboBox>
  {
  #region Конструктор

    public DocValueDocComboBoxByTableId(IDocValue DocValue, EFPDocComboBox ControlProvider, bool CanMultiEdit)
      : base(DocValue, ControlProvider, true, CanMultiEdit)
    {
      SetCurrentValue(ControlProvider.DocTableIdEx);
      CurrentValue.ValueChanged += new EventHandler(ControlChanged);
      DepAnd.AttachInput(ControlProvider.SelectableEx, EnabledEx);
    }

  #endregion

  #region Переопределенные методы

    protected override void ValueToControl()
    {
      CurrentValue.Value = DocValue.AsInteger;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }

  #endregion
  }

  /// <summary>
  /// Переходник для EFPSubDocComboBox
  /// </summary>
  public class DocValueSubDocComboBox : DocValueControl<Int32, EFPSubDocComboBox>
  {
  #region Конструктор

    public DocValueSubDocComboBox(IDocValue DocValue, EFPSubDocComboBox ControlProvider, bool CanMultiEdit)
      : base(DocValue, ControlProvider, true, CanMultiEdit)
    {
      SetCurrentValue(ControlProvider.SubDocIdEx);
      CurrentValue.ValueChanged += new EventHandler(ControlChanged);
      DepAnd.AttachInput(ControlProvider.SelectableEx, EnabledEx);
    }

  #endregion

  #region Переопределенные методы

    protected override void ValueToControl()
    {
      CurrentValue.Value = DocValue.AsInteger;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }


    /// <summary>
    /// На время чтения значений отключаем свойство автоматического выбора
    /// единственного поддокумента
    /// </summary>
    public override void BeforeReadValues()
    {
      base.BeforeReadValues();
      AutoSetIfSingle = ControlProvider.AutoSetIfSingle;
      ControlProvider.AutoSetIfSingle = false;
    }

    public override void AfterReadValues()
    {
      // 13.03.2010
      // Провайдер EFPSubDocComboBox может быть связан с помощью свойства DocId
      // с другим управляющим элементом, например с EFPDocComboBox, в котором 
      // выбирается основной документ.
      // Для нового документа в результате взаимодействия сохраненных предыдущих
      // значений ClientFields и текущих установленных фильтров IDocumentEditorCaller
      // могут возникнуть несовместимые значения полей.
      // Например, сначала было записано платежное поручение на одну организацию и
      // с расчетным счетом, относящимся к этой организации. Затем пользователь
      // устанавливает фильтр по контрагенту в журнале платежных поручений, который
      // не совпадает с первой организацией. Затем пользователь создает новое
      // платежное поручение. Сначала ClientFields устанавливают поля на предыдущего
      // контрагента и его счет (согласованные данные). Но затем фильтр устанавливает
      // контрагента на другую организацию, а счет не изменяет. Получаются 
      // несогласованные начальные значения полей

      // Решение проблемы. Проверяем совпадение текущего значения EFPDocComboBox.DocId
      // (которое вычислено исходя из заданного SubDocId) с тем DocId, на который 
      // установлена связь

      if (ControlProvider.Enabled && // то есть не Grayed и не DataReadOnly
        ControlProvider.SubDocId != 0 && // есть выбранный поддокумент
        ControlProvider.DocIdExSource != null) // и есть внешнее управление
      {
        if (ControlProvider.DocId != ControlProvider.DocIdExSource.Value &&
          ControlProvider.DocIdExSource.Value != 0) // 12.06.2011
        {
          EFPApp.WarningMessageBox("Поле \"" + ControlProvider.DisplayName +
            "\" изменено, т.к. выбранный ранее поддокумент " + ControlProvider.SubDocType.SingularTitle + " \"" +
            ControlProvider.SubDocType.GetTextValue(ControlProvider.SubDocId) +
            "\" относится к документу " + ControlProvider.DocType.SingularTitle +
            " \"" + ControlProvider.DocType.GetTextValue(ControlProvider.DocId) +
            "\", а не к \"" + ControlProvider.DocType.GetTextValue(ControlProvider.DocIdExSource.Value) + "\"");
          ControlProvider.DocId = 0; // Иначе не сбросится
          ControlProvider.DocId = ControlProvider.DocIdExSource.Value;
        }
      }


      base.AfterReadValues();
      ControlProvider.AutoSetIfSingle = AutoSetIfSingle;
    }

    private bool AutoSetIfSingle;

  #endregion
  }

#endif

  /// <summary>
  /// Переходник для EFPInsideSubDocComboBox
  /// </summary>
  public class DocValueInsideSubDocComboBox : DocValueAnyDocComboBoxBase<Int32, EFPInsideSubDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueInsideSubDocComboBox(DBxDocValue docValue, EFPInsideSubDocComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SubDocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      // TODO: 21.11.2019 Затычка
      // Если текущее значение DocValue является неправильным (относится к другому документу), то предыдущий оператор приводит к некорректному состоянию.
      // Свойство EFPInsideSubDocComboBox.SubDicId остается равным 0, но SubDicIdEx остается с неправильным значением.
      //CurrentValueEx.Value = DocValue.AsInteger;

      // Так не будет бяки
      ControlProvider.SubDocId = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для EFPDocTypeComboBox с использованием в качестве значения идентификатора таблицы
  /// </summary>
  public class DocValueDocTypeComboBoxByTableId : DocValueControl<int, EFPDocTypeComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDocTypeComboBoxByTableId(DBxDocValue docValue, EFPDocTypeComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DocTableIdEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для EFPDocTypeComboBox с использованием в качестве значения имени таблицы
  /// </summary>
  public class DocValueDocTypeComboBoxByName : DocValueControl<string, EFPDocTypeComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDocTypeComboBoxByName(DBxDocValue docValue, EFPDocTypeComboBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      UserDisabledValue = String.Empty;

      SetCurrentValueEx(controlProvider.DocTypeNameEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }


  // **********************************************************************
  /// <summary>
  /// Переходник для XmlViewBox, хранящий данные в XML поле (как текст)
  /// </summary>
  public class DocValueXmlViewBox : DocValueControl<string, EFPXmlViewBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public DocValueXmlViewBox(DBxDocValue docValue, EFPXmlViewBox controlProvider)
      : base(docValue, controlProvider, true, false)
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
      CurrentValueEx.Value = DocValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }

}
