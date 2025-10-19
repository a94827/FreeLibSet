// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data;
using FreeLibSet.DependedValues;
using FreeLibSet.UICore;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Аргументы события, предназначенного для инициализации редактора <see cref="ExtEditDialog"/>,
  /// когда он используется для редактирования записи с доступам к значениям с помощью <see cref="IDBxExtValues"/>.
  /// Содержит методы для создания объектов-переходников
  /// Используется в <see cref="DataTableEditDialog"/> и библиотеке ExtDBDocForms.
  /// </summary>
  public class DBxExtValuesDialogInitEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="dialog">Инициализируемый диалог</param>
    /// <param name="values">Доступ к полям редактируемой записи</param>
    public DBxExtValuesDialogInitEventArgs(ExtEditDialog dialog, IDBxExtValues values)
    {
      if (dialog == null)
        throw new ArgumentNullException("dialog");
      if (values == null)
        throw new ArgumentNullException("values");

      _Dialog = dialog;
      _Values = values;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый диалог
    /// </summary>
    public ExtEditDialog Dialog { get { return _Dialog; } }
    private readonly ExtEditDialog _Dialog;

    /// <summary>
    /// "Серые" значения редактируемых полей
    /// </summary>
    public IDBxExtValues Values { get { return _Values; } }
    private readonly IDBxExtValues _Values;

    #endregion

    #region Методы привязки полей

    #region Общий метод

    /// <summary>
    /// Обобщенный метод добавления объекта редактирования поля документа или поддокумента
    /// (или нескольких полей).
    /// </summary>
    /// <param name="item">Интерфейс, с помощью которого выполняется чтение и запись значений при работе редактора</param>
    public void AddEditItem(IUIExtEditItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
      if (_Dialog.EditItems.Contains(item))
        throw ExceptionFactory.CannotAddItemAgain(item);
#endif
      _Dialog.EditItems.Add(item);
      if (item.ChangeInfo != null)
        CurrChangeInfoList.Add(item.ChangeInfo);
    }

    /// <summary>
    /// Возвращает объект <see cref="DBxExtValue"/> по имени поля
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    protected DBxExtValue GetExtValue(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      if (Values.IndexOf(columnName) < 0)
        throw ExceptionFactory.ArgUnknownValue("columnName", columnName);

      return Values[columnName];
    }

    /// <summary>
    /// Список с объектами, отслеживающими изменения в элементах, расположенных на текущей вкладке
    /// (свойство <see cref="IUIExtEditItem.ChangeInfo"/>)
    /// </summary>
    protected DepChangeInfoList CurrChangeInfoList
    {
      get
      {
        if (Dialog.Pages.Count == 0)
          return Dialog.ChangeInfoList;
        else
          return Dialog.Pages[Dialog.Pages.Count - 1].ChangeInfoList;
      }
    }

    #endregion

    #region Текстовые поля

    /// <summary>
    /// Добавляет связку для редактирования текстового поля
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента, в котором можно вводить текст,
    /// например <see cref="EFPTextBox"/> или <see cref="EFPTextComboBox"/></param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueTextBox AddText(IEFPTextBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueTextBox dvc = new ExtValueTextBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования текстового поля с помощью комбоблока без возможности редактирования.
    /// У <see cref="EFPListComboBox"/> используется свойство <see cref="IEFPListControl.SelectedCode"/> для редактируемого значения.
    /// Должен быть задан массив Codes для элементов списка.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueTextListControl AddText(IEFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueTextListControl dvc = new ExtValueTextListControl(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Поля даты и времени

    /// <summary>
    /// Добавляет связку для редактирования поля типа "Дата" с помощью <see cref="DateTimeBox"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDateBox AddDate(EFPDateTimeBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDateBox dvc = new ExtValueDateBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }


    /// <summary>
    /// Добавляет связку для редактирования двух полей типа "Дата" с помощью <see cref="DateRangeBox"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDateRangeBox AddDate(EFPDateRangeBox controlProvider, string firstColumnName, string lastColumnName, bool canMultiEdit)
    {
      ExtValueDateRangeBox dvc = new ExtValueDateRangeBox(GetExtValue(firstColumnName), GetExtValue(lastColumnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования двух полей типа "Дата" с помощью <see cref="EFPDateOrRangeBox"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDateOrRangeBox AddDate(EFPDateOrRangeBox controlProvider, string firstColumnName, string lastColumnName, bool canMultiEdit)
    {
      ExtValueDateOrRangeBox dvc = new ExtValueDateOrRangeBox(GetExtValue(firstColumnName), GetExtValue(lastColumnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования целочисленного поля с помощью <see cref="MonthDayBox"/>.
    /// Предполагается, что поле не может иметь пустого значения, так как при сохранении документа не может быть значения 0.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32MonthDayBox AddInteger(EFPMonthDayBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInt32MonthDayBox dvc = new ExtValueInt32MonthDayBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования месяца и дня в целочисленном поле с помощью <see cref="MaskedTextBox"/>.
    /// Значение лежит в диапазоне от 1 до 365. Значение 0 соответствует пустому полю.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32MonthDayTextBox AddInteger(EFPMonthDayTextBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInt32MonthDayTextBox dvc = new ExtValueInt32MonthDayTextBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования целочисленного поля с помощью <see cref="MonthDayBox"/>.
    /// Предполагается, что поле не может иметь пустого значения, так как при сохранении документа не может быть значения 0.
    /// </summary>
    /// <param name="controlProvider1">Провайдер управляющего элемента <see cref="CheckBox"/></param>
    /// <param name="controlProvider2">Провайдер управляющего элемента <see cref="MonthDayBox"/></param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32CheckBoxWithMonthDayBox AddInteger(EFPCheckBox controlProvider1, EFPMonthDayBox controlProvider2, string columnName, bool canMultiEdit)
    {
      ExtValueInt32CheckBoxWithMonthDayBox dvc = new ExtValueInt32CheckBoxWithMonthDayBox(GetExtValue(columnName), controlProvider1, controlProvider2, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

#if XXX
    public DocValueYearMonthBox AddYearMonth(EFPYearMonthBox ControlProvider, string YearColumnName, string MonthColumnName, bool CanMultiEdit)
    {
      DocValueYearMonthBox dvc = new DocValueYearMonthBox(GetDocValue(YearColumnName), GetDocValue(MonthColumnName), ControlProvider, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
    public DocValueYearMonthWithCheckBox AddYearMonth(EFPCheckBox ControlProvider1, EFPYearMonthBox ControlProvider2, string YearColumnName, string MonthColumnName, bool CanMultiEdit)
    {
      DocValueYearMonthWithCheckBox dvc = new DocValueYearMonthWithCheckBox(GetDocValue(YearColumnName),
        GetDocValue(MonthColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    public DocValueYearMonthRangeBox AddYearMonth(EFPYearMonthRangeBox ControlProvider, string YearColumnName, string FirstMonthColumnName, string LastMonthColumnName, bool CanMultiEdit)
    {
      DocValueYearMonthRangeBox dvc = new DocValueYearMonthRangeBox(GetDocValue(YearColumnName), GetDocValue(FirstMonthColumnName),
        GetDocValue(LastMonthColumnName), ControlProvider, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    public DocValueMonthDayBox AddMonthDay(EFPMonthDayBox ControlProvider, string MonthColumnName, string DayColumnName, bool CanMultiEdit)
    {
      DocValueMonthDayBox dvc = new DocValueMonthDayBox(GetDocValue(MonthColumnName), GetDocValue(DayColumnName), ControlProvider, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
#endif

    /// <summary>
    /// Добавляет связку для редактирования полей типа <see cref="DateTime"/> с помощью <see cref="DateTimePicker"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDateTimePicker AddDateTime(EFPDateTimePicker controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDateTimePicker dvc = new ExtValueDateTimePicker(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования полей типа <see cref="TimeSpan"/> с помощью <see cref="DateTimePicker"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueTimeSpanPicker AddTime(EFPDateTimePicker controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueTimeSpanPicker dvc = new ExtValueTimeSpanPicker(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Числовые поля

    #region NumEditBox и NumericUpDown

    #region Decimal

    /// <summary>
    /// Добавляет связку для редактирования числового поля.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDecimalEditBox AddDecimal(EFPDecimalEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDecimalEditBox dvc = new ExtValueDecimalEditBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /*
    public DocValueCheckBoxWithDecimalNumEditBox AddDecimal(EFPCheckBox ControlProvider1, IEFPNumEditBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithDecimalNumEditBox dvc = new DocValueCheckBoxWithDecimalNumEditBox(GetDocValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    } */

    #endregion

    #region Integer

    /// <summary>
    /// Добавляет связку для редактирования числового поля.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32EditBox AddInt32(EFPInt32EditBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInt32EditBox dvc = new ExtValueInt32EditBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /*
    public DocValueCheckBoxWithIntNumEditBox AddInt32(EFPCheckBox ControlProvider1, IEFPNumEditBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithIntNumEditBox dvc = new DocValueCheckBoxWithIntNumEditBox(GetDocValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    } */

    #endregion

    #region Single

    /// <summary>
    /// Добавляет связку для редактирования числового поля.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueSingleEditBox AddSingle(EFPSingleEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueSingleEditBox dvc = new ExtValueSingleEditBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /*
    public DocValueCheckBoxWithSingleNumEditBox AddSingle(EFPCheckBox ControlProvider1, IEFPNumEditBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithSingleNumEditBox dvc = new DocValueCheckBoxWithSingleNumEditBox(GetDocValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    } */

    #endregion

    #region Double

    /// <summary>
    /// Добавляет связку для редактирования числового поля.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDoubleEditBox AddDouble(EFPDoubleEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDoubleEditBox dvc = new ExtValueDoubleEditBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /*
    public DocValueCheckBoxWithDoubleNumEditBox AddDouble(EFPCheckBox ControlProvider1, IEFPNumEditBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithDoubleNumEditBox dvc = new DocValueCheckBoxWithDoubleNumEditBox(GetDocValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    } */

    #endregion

    #endregion

    #region ComboBox, ListBox и RadioButtons

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью <see cref="ListBox"/>, <see cref="ComboBox"/> или группы радиокнопок.
    /// Значение поля связывается со свойством <see cref="IEFPListControl.SelectedIndex"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента <see cref="EFPListBox"/>, <see cref="EFPListComboBox"/> или <see cref="EFPRadioButtons"/></param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32ListControl AddInt32(IEFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInt32ListControl dvc = new ExtValueInt32ListControl(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью <see cref="ComboBox"/>, в котором можно
    /// ввести числовое значение напрямую или выбрать одно из предопределенных значений.
    /// Значение поля связывается со свойством <see cref="EFPInt32EditComboBox.Value"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32EditComboBox AddInt32(EFPInt32EditComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInt32EditComboBox dvc = new ExtValueInt32EditComboBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью <see cref="ListBox"/> или <see cref="ComboBox"/>.
    /// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
    /// Значение поля связывается со свойством <see cref="EFPListControl.SelectedCode"/>, в котором хранится текстовое значение для enum.
    /// </summary>
    /// <typeparam name="T">Тип перечислимого значения</typeparam>
    /// <param name="controlProvider">Провайдер управляющего элемента <see cref="EFPListBox"/> или <see cref="EFPListComboBox"/></param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInt32EnumCodeListControl<T> AddInt32EnumCode<T>(EFPListControl controlProvider, string columnName, bool canMultiEdit)
      where T : struct
    {
      ExtValueInt32EnumCodeListControl<T> dvc = new ExtValueInt32EnumCodeListControl<T>(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #endregion

    #region Логические поля

    /// <summary>
    /// Добавляет связку для редактирования логического поля с помощью <see cref="CheckBox"/>.
    /// Значение поля связывается со свойством <see cref="EFPCheckBox.Checked"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае <see cref="CheckBox.ThreeState"/> устанавливается в true и значение <see cref="System.Windows.Forms.CheckState.Indeterminate"/> используется для отображения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueCheckBox AddBoolean(EFPCheckBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueCheckBox dvc = new ExtValueCheckBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования логического поля с помощью группы из двух радиокнопок.
    /// Первая кпопка в группе - для значения false, вторая - для true.
    /// Значение поля связывается со значением свойства (<see cref="EFPRadioButtons.SelectedIndex"/>=1?true:false).
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueBooleanRadioButtons AddBoolean(EFPRadioButtons controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueBooleanRadioButtons dvc = new ExtValueBooleanRadioButtons(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью <see cref="ListBox"/> или <see cref="ComboBox"/>, в котором можно
    /// выбрать одно из двух позиций в списке.
    /// Первая позиция в списке - для значения false, вторая - для true.
    /// Значение поля связывается со значением свойства (<see cref="EFPListControl.SelectedIndex"/>=1?true:false).
    /// Такой вариант редактирование логического поля, особенно с помощью <see cref="ListBox"/>, используется редко.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента <see cref="EFPListBox"/> или <see cref="EFPListComboBox"/></param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueBooleanListControl AddBoolean(EFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueBooleanListControl dvc = new ExtValueBooleanListControl(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Прочие поля

    /// <summary>
    /// Добавляет связку для редактирования XML-данных с помощью <see cref="XmlViewBox"/>.
    /// Текстовое (обычно, MEMO) поле содержит XML-данные.
    /// Редко используемый вариант пользовательского интерфейса.
    /// Групповое редактирование документов не поддерживается.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueXmlViewBox AddXml(EFPXmlViewBox controlProvider, string columnName)
    {
      ExtValueXmlViewBox dvc = new ExtValueXmlViewBox(GetExtValue(columnName), controlProvider);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DataTableEditDialog.InitEditForm"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="DataTableEditDialog"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxExtValuesDialogInitEventHandler(object sender, DBxExtValuesDialogInitEventArgs args);
}
