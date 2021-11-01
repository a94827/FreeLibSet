using System;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Collections.Generic;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.Controls;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  #region Абстрактный базовый класс инициализации формы

  /// <summary>
  /// Базовый класс для InitDocEditFormEventArgs и InitSubDocEditFormEventArgs
  /// </summary>
  public abstract class InitDocEditFormEventArgsBase : IReadOnlyObject
  {
    #region Конструктор

    internal InitDocEditFormEventArgsBase(DocEditForm editorForm, IDBxDocValues values)
    {
      _EditorForm = editorForm;
      _Values = values;

    }

    #endregion

    #region Свойства

    internal DocEditForm EditorForm { get { return _EditorForm; } }
    private DocEditForm _EditorForm;

    /// <summary>
    /// "Серые" значения редактируемых документов или поддокументов (поля)
    /// </summary>
    public IDBxDocValues Values { get { return _Values; } }
    private IDBxDocValues _Values;

    /// <summary>
    /// Добавить форму для последующего разрушения
    /// Метод должен вызываться до того, как управляющий элемент переприсоединяется
    /// к форме редактора
    /// </summary>
    /// <param name="form"></param>
    public void AddFormToDispose(Form form)
    {
      if (form == null)
        return;
      if (EditorForm.DisposeFormList.IndexOf(form) < 0)
        EditorForm.DisposeFormList.Add(form);
    }

    #endregion

    #region AddPage

    /// <summary>
    /// Добавить вкладку в редактор документа или поддокумента.
    /// После вызова метода можно создавать провайдеры управляющих элементов, используя свойство
    /// DocEditPage.BaseProvider.
    /// </summary>
    /// <param name="title">Заголовок вкладки</param>
    /// <param name="mainControl">Управляющий элемент (обычно, панель), на котором располагаются другие элементы</param>
    /// <returns>Объект для управления вкладкой.</returns>
    public DocEditPage AddPage(string title, Control mainControl)
    {
      if (mainControl == null)
        throw new ArgumentNullException("mainControl");

      AddFormToDispose(mainControl.FindForm());

      if (String.IsNullOrEmpty(title))
        title = (_EditorForm.TabControlProvider.TabPages.Count + 1).ToString();

      EFPTabPage TabPage = _EditorForm.TabControlProvider.TabPages.Add(title);
      DocEditPage Page = new DocEditPage(_EditorForm.Pages, TabPage);

      //Size sz = new Size();
      //sz.Width = MainControl.DisplayRectangle.Width;
      //sz.Height = MainControl.DisplayRectangle.Height;
      Size sz = mainControl.Size; // 21.10.2019. Почему-то раньше было DisplayRectangle

      //TabPage.Control.BackColor = Color.Transparent;
      //TabPage.Control.UseVisualStyleBackColor = true;
      if (mainControl is Panel)
        mainControl.BackColor = Color.Transparent;

      mainControl.Visible = false; // 31.07.2012 прячем до вызова FirstShow

      TabPage.Control.Controls.Add(mainControl);
      _EditorForm.RegPageSize(sz);

      TabPage.Control.Resize += new EventHandler(TabPageResize);
      mainControl.Dock = DockStyle.None; // управляется из Resize

      InitPageChangeInfoList(title);

      return Page;
    }

    /// <summary>
    /// Добавить в редактор вкладку, содержащую единственный управляющий элемент и панель инструментов.
    /// Метод создает вкладку, управляющий элемент и панель инструментов.
    /// </summary>
    /// <typeparam name="T">Класс управляющего элемента, производного от Control. У элемента должен
    /// быть конструктор по умолчанию.</typeparam>
    /// <param name="title">Заголовок вкладки</param>
    /// <param name="controlWithToolBar">Сюда помещается объект с элементом управления и панелью инструментов</param>
    /// <returns>Объект для управления вкладкой.</returns>
    public DocEditPage AddSimplePage<T>(string title, out EFPControlWithToolBar<T> controlWithToolBar)
      where T : Control, new()
    {         
      Panel ThePanel = new Panel();
      DocEditPage Page = AddPage(title, ThePanel);
      controlWithToolBar = new EFPControlWithToolBar<T>(Page.BaseProvider, Page.MainControl);
      return Page;
    }

    #endregion

    #region Отслеживание изменений
              
    /// <summary>
    /// Список с объектами, отслеживающими изменения в элементах, расположенных на текущей вкладке
    /// (свойство IDocEditItem.ChangeInfo)
    /// </summary>
    protected DepChangeInfoList CurrChangeInfoList
    {
      get
      {
        if (_CurrChangeInfoList == null)
          return _EditorForm.ChangeInfoList;
        else
          return _CurrChangeInfoList;
      }
    }
    private DepChangeInfoList _CurrChangeInfoList;

    /// <summary>
    /// Инициализация списка CurrChangeInfoList
    /// </summary>
    /// <param name="displayName">Значение свойства DepChangeInfo.DisplayName</param>
    protected void InitPageChangeInfoList(string displayName)
    {
      _CurrChangeInfoList = new DepChangeInfoList();
      _CurrChangeInfoList.DisplayName = displayName;
      _EditorForm.ChangeInfoList.Add(_CurrChangeInfoList);
    }

    #endregion

    #region Методы привязки полей

    #region Общий метод

    /// <summary>
    /// Обобщенный метод добавления объекта редактирования поля документа или поддокумента
    /// (или нескольких полей)
    /// </summary>
    /// <param name="item">Интерфейс, с помощью которого выполняется чтение и запись значений при работе редактора</param>
    public void AddDocEditItem(IDocEditItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
      if (_EditorForm.DocEditItems.Contains(item))
        throw new InvalidOperationException("Повторное добавление элемента \"" + item.ToString() + "\"");
#endif
      _EditorForm.DocEditItems.Add(item);
      if (item.ChangeInfo != null)
        CurrChangeInfoList.Add(item.ChangeInfo);
    }

    private DBxDocValue GetDocValue(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      if (Values.IndexOf(columnName) < 0)
        throw new ArgumentException("В редактируемом документе нет поля \"" + columnName + "\"", "columnName");

      return Values[columnName];
    }

    #endregion

    #region Текстовые поля

    /// <summary>
    /// Добавляет связку для редактирования текстового поля
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента, в котором можно вводить текст,
    /// например EFPTextBox или EFPTextComboBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueTextBox AddText(IEFPTextBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueTextBox dvc = new DocValueTextBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования текстового поля с помощью комбоблока без возможности редактирования.
    /// У EFPListComboBox используется свойство SelectedCode для редактируемого значения.
    /// Должен быть задан массив Codes для элементов списка.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueTextListControl AddText(IEFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueTextListControl dvc = new DocValueTextListControl(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    ///// <summary>
    ///// Добавляет связку для редактирования текстового поля с помощью группы радиокнопок.
    ///// У EFPRadioButtons используется свойство SelectedCode для редактируемого значения.
    ///// Должен быть задан массив Codes для кнопок.
    ///// </summary>
    ///// <param name="controlProvider">Провайдер EFPRadioButtons</param>
    ///// <param name="columnName">Имя поля</param>
    ///// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    ///// В текущей реализации использовать не рекомендуется</param>
    ///// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    //public DocValueTextRadioButtons AddText(EFPRadioButtons controlProvider, string columnName, bool canMultiEdit)
    //{
    //  DocValueTextRadioButtons dvc = new DocValueTextRadioButtons(GetDocValue(columnName), controlProvider, canMultiEdit);
    //  AddDocEditItem(dvc);
    //  return dvc;
    //}

    #endregion

    #region Поля даты и времени

    /// <summary>
    /// Добавляет связку для редактирования поля типа "Дата" с помощью DateBox.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDateBox AddDate(EFPDateTimeBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDateBox dvc = new DocValueDateBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }


    /// <summary>
    /// Добавляет связку для редактирования двух полей типа "Дата" с помощью DateRangeBox.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDateRangeBox AddDate(EFPDateRangeBox controlProvider, string firstColumnName, string lastColumnName, bool canMultiEdit)
    {
      DocValueDateRangeBox dvc = new DocValueDateRangeBox(GetDocValue(firstColumnName), GetDocValue(lastColumnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования двух полей типа "Дата" с помощью DateOrRangeBox.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDateOrRangeBox AddDate(EFPDateOrRangeBox controlProvider, string firstColumnName, string lastColumnName, bool canMultiEdit)
    {
      DocValueDateOrRangeBox dvc = new DocValueDateOrRangeBox(GetDocValue(firstColumnName), GetDocValue(lastColumnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования целочисленного поля с помощью MonthDayBox.
    /// Предполагается, что поле не может иметь пустого значения, так как при сохранении документа не может быть значения 0.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntMonthDayBox AddInt(EFPMonthDayBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueIntMonthDayBox dvc = new DocValueIntMonthDayBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования месяца и дня в целочисленном поле с помощью MaskedTextBox.
    /// Значение лежит в диапазоне от 1 до 365. Значение 0 соответствует пустому полю.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntMonthDayTextBox AddInt(EFPMonthDayTextBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueIntMonthDayTextBox dvc = new DocValueIntMonthDayTextBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования целочисленного поля с помощью MonthDayBox.
    /// Предполагается, что поле не может иметь пустого значения, так как при сохранении документа не может быть значения 0.
    /// </summary>
    /// <param name="controlProvider1">Провайдер управляющего элемента CheckBox</param>
    /// <param name="controlProvider2">Провайдер управляющего элемента MonthDayBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntCheckBoxWithMonthDayBox AddInt(EFPCheckBox controlProvider1, EFPMonthDayBox controlProvider2, string columnName, bool canMultiEdit)
    {
      DocValueIntCheckBoxWithMonthDayBox dvc = new DocValueIntCheckBoxWithMonthDayBox(GetDocValue(columnName), controlProvider1, controlProvider2, canMultiEdit);
      AddDocEditItem(dvc);
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
    /// Добавляет связку для редактирования полей типа DateTime с помощью DateTimePicker.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDateTimePicker AddDateTime(EFPDateTimePicker controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDateTimePicker dvc = new DocValueDateTimePicker(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования полей типа TimeSpan с помощью DateTimePicker.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueTimeSpanPicker AddTime(EFPDateTimePicker controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueTimeSpanPicker dvc = new DocValueTimeSpanPicker(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Числовые поля

    #region NumEditBox и NumericUpDown

    #region Decimal

    /// <summary>
    /// Добавляет связку для редактирования числового поля.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента, реализующий интерфейс IEFPNumEditBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDecimalEditBox AddDecimal(EFPDecimalEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDecimalEditBox dvc = new DocValueDecimalEditBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
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
    /// <param name="controlProvider">Провайдер управляющего элемента, реализующий интерфейс IEFPNumEditBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntEditBox AddInt(EFPIntEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueIntEditBox dvc = new DocValueIntEditBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /*
    public DocValueCheckBoxWithIntNumEditBox AddInt(EFPCheckBox ControlProvider1, IEFPNumEditBox ControlProvider2, string ColumnName, bool CanMultiEdit)
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
    /// <param name="controlProvider">Провайдер управляющего элемента, реализующий интерфейс IEFPNumEditBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueSingleEditBox AddSingle(EFPSingleEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueSingleEditBox dvc = new DocValueSingleEditBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
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
    /// <param name="controlProvider">Провайдер управляющего элемента, реализующий интерфейс IEFPNumEditBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDoubleEditBox AddDouble(EFPDoubleEditBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDoubleEditBox dvc = new DocValueDoubleEditBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
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
    /// Добавляет связку для редактирования числового поля с помощью ListBox, ComboBox или группы радиокнопок.
    /// Значение поля связывается со свойством SelectedIndex.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента EFPListBox, EFPListComboBox или EFPRadioButtons</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntListControl AddInt(IEFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueIntListControl dvc = new DocValueIntListControl(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью ComboBox, в котором можно
    /// ввести числовое значение напрямую или выбрать одно из предопределенных значений.
    /// Значение поля связывается со свойством Value.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента EFPListBox или EFPListComboBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntEditComboBox AddInt(EFPIntEditComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueIntEditComboBox dvc = new DocValueIntEditComboBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью ListBox илм ComboBox.
    /// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
    /// Значение поля связывается со свойством SelectedCode, в котором хранится текстовое значение для enum.
    /// </summary>
    /// <typeparam name="T">Тип перечислимого значения</typeparam>
    /// <param name="controlProvider">Провайдер управляющего элемента EFPListBox или EFPListComboBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueIntEnumCodeListControl<T> AddIntEnumCode<T>(EFPListControl controlProvider, string columnName, bool canMultiEdit)
      where T:struct
    {
      DocValueIntEnumCodeListControl<T> dvc = new DocValueIntEnumCodeListControl<T>(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Радиокнопки

    ///// <summary>
    ///// Добавляет связку для редактирования числового поля с помощью группы радиокнопок.
    ///// Значение поля связывается со свойством SelectedIndex.
    ///// </summary>
    ///// <param name="controlProvider">Провайдер группы радиокнопок</param>
    ///// <param name="columnName">Имя поля</param>
    ///// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    ///// В текущей реализации использовать не рекомендуется.</param>
    ///// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    //public DocValueIntRadioButtons AddInt(EFPRadioButtons controlProvider, string columnName, bool canMultiEdit)
    //{
    //  DocValueIntRadioButtons dvc = new DocValueIntRadioButtons(GetDocValue(columnName), controlProvider, canMultiEdit);
    //  AddDocEditItem(dvc);
    //  return dvc;
    //}

    ///// <summary>
    ///// Добавляет связку для редактирования числового поля с помощью группы радиокнопок.
    ///// Числовое поле содержит перечислимое значение, но значения не обязаны следовать по порядку.
    ///// Значение поля связывается со свойством SelectedCode, в котором хранится текстовое значение для enum.
    ///// </summary>
    ///// <typeparam name="T">Тип перечислимого значения</typeparam>
    ///// <param name="controlProvider">Провайдер управляющего элемента EFPRadioButtons</param>
    ///// <param name="columnName">Имя поля</param>
    ///// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    ///// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    ///// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    //public DocValueIntEnumCodeRadioButtons<T> AddIntEnumCode<T>(EFPRadioButtons controlProvider, string columnName, bool canMultiEdit)
    //  where T : struct
    //{
    //  DocValueIntEnumCodeRadioButtons<T> dvc = new DocValueIntEnumCodeRadioButtons<T>(GetDocValue(columnName), controlProvider, canMultiEdit);
    //  AddDocEditItem(dvc);
    //  return dvc;
    //}

    #endregion

    #endregion

    #region Логические поля

    /// <summary>
    /// Добавляет связку для редактирования логического поля с помощью CheckBox.
    /// Значение поля связывается со свойством Checked.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае CheckBox.ThreeState устанавливается в true и ThreeState.Intermediate используется для отображения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueCheckBox AddBool(EFPCheckBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueCheckBox dvc = new DocValueCheckBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования логического поля с помощью группы из двух радиокнопок.
    /// Первая кпопка в группе - для значения false, вторая - для true.
    /// Значение поля связывается со значением свойства (SelectedIndex=1?true:false).
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В текущей реализации использовать не рекомендуется</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueBoolRadioButtons AddBool(EFPRadioButtons controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueBoolRadioButtons dvc = new DocValueBoolRadioButtons(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования числового поля с помощью ListBox или ComboBox, в котором можно
    /// выбрать одно из двух позиций в списке.
    /// Первая позиция в списке - для значения false, вторая - для true.
    /// Значение поля связывается со значением свойства (SelectedIndex=1?true:false).
    /// Такой вариант редактирование логического поля, особенно с помощью ListBox, используется редко.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента EFPListBox или EFPListComboBox</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueBoolListControl AddBool(EFPListControl controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueBoolListControl dvc = new DocValueBoolListControl(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Ссылочные поля

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на документ с помощью комбоблока.
    /// Значение поля связывается со свойством DocId.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDocComboBox AddRef(EFPDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDocComboBox dvc = new DocValueDocComboBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на поддокумент с помощью комбоблока.
    /// Поддокумент не должен относиться к документу, открытому в текущем редакторе.
    /// Значение поля связывается со свойством SubDocId.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueSubDocComboBox AddRef(EFPSubDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueSubDocComboBox dvc = new DocValueSubDocComboBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на поддокументы с помощью комбоблока.
    /// Значение поля связывается со свойством SubDocId.
    /// Предполагается, что поддокумент относится к тому документу, который в настоящее время
    /// открыт в редакторе документа
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueInsideSubDocComboBox AddRef(EFPInsideSubDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueInsideSubDocComboBox dvc = new DocValueInsideSubDocComboBox(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Ссылки на таблицу DocType

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на таблицу документов.
    /// Числовое поле содержит идентификатор таблицы документов DBxDocType.Tableid.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDocTypeComboBoxByTableId AddTableId(EFPDocTypeComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDocTypeComboBoxByTableId dvc = new DocValueDocTypeComboBoxByTableId(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
#if XXX
    public DocValueCheckBoxWithDocTypeComboBoxByTableId AddTableId(EFPCheckBox ControlProvider1, EFPDocTypeComboBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithDocTypeComboBoxByTableId dvc = new DocValueCheckBoxWithDocTypeComboBoxByTableId(GetDocValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
    public DocValueDocComboBoxByTableId AddTableId(EFPDocComboBox ControlProvider, string ColumnName, bool CanMultiEdit)
    {
      DocValueDocComboBoxByTableId dvc = new DocValueDocComboBoxByTableId(GetDocValue(ColumnName), ControlProvider, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
#endif

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на таблицу документов.
    /// Строкое поле содержит имя таблицы документов DBxDocType.Name.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов.
    /// В этом случае может быть добавлен CheckBox для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueDocTypeComboBoxByName AddTableName(EFPDocTypeComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      DocValueDocTypeComboBoxByName dvc = new DocValueDocTypeComboBoxByName(GetDocValue(columnName), controlProvider, canMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Прочие поля

    /// <summary>
    /// Добавляет связку для редактирования XML-данных с помощью XmlViewBox.
    /// Текстовое (обычно, MEMO) поле содержит XML-данные.
    /// Редко используемый вариант пользовательского интерфейса.
    /// Групповое редактирование документов не поддерживается.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Переходник, реализующий интерфейс IDocEditItem</returns>
    public DocValueXmlViewBox AddXml(EFPXmlViewBox controlProvider, string columnName)
    {
      DocValueXmlViewBox dvc = new DocValueXmlViewBox(GetDocValue(columnName), controlProvider);
      AddDocEditItem(dvc);
      return dvc;
    }

    #endregion

    #endregion

    #region Внутренняя реализация

    private static void TabPageResize(object sender, EventArgs args)
    {
      TabPage tp = (TabPage)sender;
      if (tp.Controls.Count > 0) // 23.04.2018. Вообще-то так быть не должно
      {
        Control ctrl = tp.Controls[0];
        ctrl.Location = new Point(0, 0);
        ctrl.Size = new Size(tp.ClientSize.Width, tp.ClientSize.Height);
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Свойство возвращает true, если редактор документа или поддокумента
    /// находится в режиме просмотра данных
    /// </summary>
    public abstract bool IsReadOnly { get;}

    /// <summary>
    /// Генерирует ObjectReadOnlyException, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException("Редактор находится в режиме просмотра данных");
    }

    #endregion
  }

  #endregion

  #region Инициализация основной формы документа

  /// <summary>
  /// Аргументы события DocTypeUI.InitEditForm
  /// </summary>
  public class InitDocEditFormEventArgs : InitDocEditFormEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создается в DocTypeUI
    /// </summary>
    /// <param name="editor">Открываемый редактор документов</param>
    /// <param name="multiDocs">Редактируемые документы</param>
    public InitDocEditFormEventArgs(DocumentEditor editor, DBxMultiDocs multiDocs)
      : base(editor.Form, multiDocs.Values)
    {
      _Editor = editor;
      _MultiDocs = multiDocs;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Открываемый редактор документов
    /// </summary>
    public DocumentEditor Editor { get { return _Editor; } }
    private DocumentEditor _Editor;

    /// <summary>                              
    /// Редактируемые документы, для которых открыт редактор.
    /// Для доступа к документов других видов используйте объект DBxDocSet, который доступен через свойство Editor.Documents.
    /// </summary>
    public DBxMultiDocs MultiDocs { get { return _MultiDocs; } }
    private DBxMultiDocs _MultiDocs;

    private DocTypeUI DocTypeUI { get { return _Editor.UI.DocTypes[_MultiDocs.DocType.Name]; } }

    /// <summary>
    /// Коллекция для добавления объектов синхронизации
    /// </summary>
    public DepSyncCollection Syncs
    {
      get
      {
        return EditorForm.FormProvider.Syncs;
      }
    }

    /// <summary>
    /// Возвращает свойство DocumentEditor.IsReadOnly
    /// </summary>
    public override bool IsReadOnly { get { return Editor.IsReadOnly; } }

    #endregion

    #region Методы добавления страницы

    #region С поддокументами

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// Эта перегрузка метода не дает доступа к созданному провайдеру просмотра EFPSubDocGridView
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <returns>Объект для управления вкладкой</returns>
    public DocEditPage AddSubDocsPage(string subDocTableName)
    {
      // TODO: Переделать с использованием отложенной инициализации
      EFPSubDocGridView ControlProvider;
      return AddSubDocsPage(subDocTableName, out ControlProvider);
    }

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <param name="controlProvider">Сюда записывается ссылка на созданный провайдер просмотра EFPSubDocGridView</param>
    /// <returns>Объект для управления вкладкой</returns>
    public DocEditPage AddSubDocsPage(string subDocTableName, out EFPSubDocGridView controlProvider)
    {
      Panel Panel = new Panel();

      SubDocTypeUI sdtUI = DocTypeUI.SubDocTypes[subDocTableName];
      DocEditPage Page = AddPage(sdtUI.SubDocType.PluralTitle, Panel);
      Page.ImageKey = sdtUI.TableImageKey;
      EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(Page.BaseProvider, Panel);

      controlProvider = new EFPSubDocGridView(cwt, _Editor, _MultiDocs.SubDocs[subDocTableName]);
      return Page;
    }

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// Эта перегрузка метода не дает доступа к созданному провайдеру просмотра EFPSubDocTreeView
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <returns>Объект для управления вкладкой</returns>
    public DocEditPage AddSubDocsTreePage(string subDocTableName)
    {
      // TODO: Переделать с использованием отложенной инициализации
      EFPSubDocTreeView ControlProvider;
      return AddSubDocsTreePage(subDocTableName, out ControlProvider);
    }

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <param name="controlProvider">Сюда записывается ссылка на созданный провайдер просмотра EFPSubDocTreeView</param>
    /// <returns>Объект для управления вкладкой</returns>
    public DocEditPage AddSubDocsTreePage(string subDocTableName, out EFPSubDocTreeView controlProvider)
    {
      Panel Panel = new Panel();

      SubDocTypeUI sdtUI = DocTypeUI.SubDocTypes[subDocTableName];
      DocEditPage Page = AddPage(sdtUI.SubDocType.PluralTitle, Panel);
      EFPControlWithToolBar<TreeViewAdv> cwt = new EFPControlWithToolBar<TreeViewAdv>(Page.BaseProvider, Panel);

      controlProvider = new EFPSubDocTreeView(cwt, _Editor, _MultiDocs.SubDocs[subDocTableName]);
      return Page;
    }


#if XXXX
    #region Вложенный класс SubDocsPageInfo

    private class SubDocsPageInfo
    {
      public Panel MainPanel;
      public ClientSubDocType SubDocType;
      public SubDocsGrid Grid; // создается в FirstShow
      public string ManualOrderColumn;
      public bool ValidateBeforeEdit;

      public void FirstShow(object Sender, DocEditPageEventArgs Args)
      {
        if (AccDepClientExec.Permissions.Tables[SubDocType.Name] == AccDepAccessMode.None)
        {
          Args.Page.SetPanelMessage("У Вас нет права доступа к поддокументам \"" + SubDocType.PluralTitle + "\"", MainPanel);
          return;
        }

        if (Args.Editor.MultiDocMode && (!String.IsNullOrEmpty(ManualOrderColumn)))
        {
          Args.Page.SetPanelMessage("Таблица недоступна если одновременно редактируется несколько документов", MainPanel);
          return;
        }

        Grid = new SubDocsGrid(Args.Page.BaseProvider, Args.Editor, Args.MultiDocs, SubDocType.Name);
        Grid.ManualOrderColumn = ManualOrderColumn;
        Grid.ValidateBeforeEdit = ValidateBeforeEdit;
        Grid.CreateControl(MainPanel);
      }
    }

    #endregion

#endif

#if XXX
    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText)
    {
      return AddSubDocsPage(Title, SubDocTableName, ImageKey, ToolTipText, String.Empty, false);
    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <param name="ManualOrderColumn">Имя столбца ручной сортировки. null - нет</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText, string ManualOrderColumn)
    {
      return AddSubDocsPage(Title, SubDocTableName, ImageKey, ToolTipText, ManualOrderColumn, false);
    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <param name="ManualOrderColumn">Имя столбца ручной сортировки. null - нет</param>
    /// <param name="ValidateBeforeEdit">Если установлено в true, то перед добавлением или редактированием записи,
    /// будет вызываться метод DocumentEditor.ValidateData(), чтобы поля основного документа содержали
    /// актуальные значения. Необходимо, если редактор поддокумента обращается к
    /// полям основного документа</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText, string ManualOrderColumn, bool ValidateBeforeEdit)
    {
      if (SubDocTableName == null)
        throw new ArgumentNullException("SubDocTableName");


      SubDocsPageInfo sdpi = new SubDocsPageInfo();
      sdpi.SubDocType = (ClientSubDocType)(MultiDocs.DocType.SubDocs[SubDocTableName]);
      if (sdpi.SubDocType == null)
        throw new BugException("Поддокумент типа \"" + SubDocTableName + "\" не объявлен");

      sdpi.MainPanel = new Panel();
      sdpi.MainPanel.Size = new Size(300, 200);
      sdpi.ManualOrderColumn = ManualOrderColumn;
      sdpi.ValidateBeforeEdit = ValidateBeforeEdit;

      DocEditPage dep = AddPage(Title, sdpi.MainPanel, ImageKey, ToolTipText);
      dep.FirstShow += new DocEditPageEventHandler(sdpi.FirstShow);
      return dep;

    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// Заголовок вкладки и значок задаются автоматически
    /// </summary>
    /// <param name="SubDocTableName">Имя поддокументов</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string SubDocTableName)
    {
      ClientSubDocType SubDocType = (ClientSubDocType)(MultiDocs.DocType.SubDocs[SubDocTableName]);

      return AddSubDocsPage(SubDocType.PluralTitle, SubDocTableName, SubDocType.ImageKey, null, null);
    }
#endif

    #endregion
#if XXX
    #region С произвольным табличным просмотром

    /// <summary>
    /// Добавление в редактор закладки, содержащей табличный просмотр
    /// По ссылке возвращаются обработчик табличного просмотра и панель для кнопок.
    /// После добавления команд локального меню следует вызвать DocGridHandler.SetCommandItems(PanSpb)
    /// </summary>
    /// <param name="Title">Заголовок закладки</param>
    /// <param name="DocGridHandler">Сюда помещается обработчик созданного табличного просмотра</param>
    /// <param name="PanSpb">Сюда помещается ссылка на панель кнопок</param>
    /// <param name="ImageKey">Изображение для закладки</param>
    /// <param name="ToolTipText">Подсказка</param>
    /// <returns>Страница редактора</returns>
    public DocEditPage AddGridPage(string Title, out EFPAccDepGrid ControlProvider, out Panel PanSpb, string ImageKey, string ToolTipText)
    {
      Panel MainPanel = new Panel();
      MainPanel.Dock = DockStyle.Fill;
      DataGridView Grid = new DataGridView();
      Grid.Dock = DockStyle.Fill;
      MainPanel.Controls.Add(Grid);
      PanSpb = new Panel();
      PanSpb.Dock = DockStyle.Top;
      MainPanel.Controls.Add(PanSpb);
      ControlProvider = new EFPAccDepGrid(BaseProvider, Grid);
      return AddPage(Title, MainPanel, ImageKey, ToolTipText);
    }

    public DocEditPage AddGridPage(string Title, out EFPAccDepGrid ControlProvider, out Panel PanSpb, string ImageKey)
    {
      return AddGridPage(Title, out ControlProvider, out PanSpb, ImageKey, null);
    }

    #endregion

    #region С отчетом

    /// <summary>
    /// Добавить страницу со встроенным отчетом.
    /// Отчет перестраивается при каждом переходе на страницу. Перед этим вызывается ValidateData()
    /// Заголовок страницы берется из параметров отчета, а значок - из MainImageKey
    /// </summary>
    /// <param name="Report">Отчет</param>
    /// <param name="ToolTipText">Текст всплывающей подсказки для страницы</param>
    /// <returns>Страница редактора документа</returns>
    public DocEditPage AddPageWithGridReport(GridReport Report, string ToolTipText)
    {
      // !!! Надо бы обойтись без доп. панели и встривать отчет непосредственно в EFPTabPage

#if DEBUG
      if (Report == null)
        throw new ArgumentNullException("Report");
#endif

      GridReportHandler Handler = new GridReportHandler();
      Handler.Report = Report;
      Panel MainPanel = new Panel();
      Handler.Page = AddPage(Report.ReportParams.Title, MainPanel, Report.MainImageKey, ToolTipText);
      Handler.PanelHandler = new EFPPanel(Handler.Page.BaseProvider, MainPanel);
      Handler.Page.FirstShow += new DocEditPageEventHandler(Handler.Page_FirstShow);
      Handler.Page.PageShow += new DocEditPageEventHandler(Handler.Page_PageShow);

      return Handler.Page;
    }

    private class EFPPanel : EFPControl<Panel>
    {
    #region Конструктор

      public EFPPanel(EFPBaseProvider BaseProvider, Panel Control)
        : base(BaseProvider, Control, false)
      { 
      }

      #endregion
    }

    private class GridReportHandler
    {
    #region Поля

      public GridReport Report;
      public DocEditPage Page;
      public EFPPanel PanelHandler;

      private bool FirstFlag;

      #endregion

    #region Обработчики

      public void Page_FirstShow(object Sender, DocEditPageEventArgs Args)
      {
        FirstFlag = true;
      }

      public void Page_PageShow(object Sender, DocEditPageEventArgs Args)
      {
        if (!Page.Editor.ValidateData())
          return;


        if (FirstFlag)
        {
          Report.RunBuiltIn(PanelHandler, Page.BaseProvider);
          FirstFlag = false;
        }
        else
          Report.RefreshReport();
      }

      #endregion
    }

    #endregion

#endif

    #endregion
  }

  /// <summary>
  /// Делегат события DocTypeUI.InitEditForm
  /// </summary>
  /// <param name="sender">Объект DocTypeUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitDocEditFormEventHandler(object sender, InitDocEditFormEventArgs args);

  #endregion

  #region Инициализация формы поддокумента

  /// <summary>
  /// Аргументы события SubDocTypeUI.InitEditForm
  /// </summary>
  public class InitSubDocEditFormEventArgs : InitDocEditFormEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создается в SubDocTypeUI
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    public InitSubDocEditFormEventArgs(SubDocumentEditor editor)
      : base(editor.Form, editor.SubDocs.Values)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор поддокумента, инициализация которого выполняется
    /// </summary>
    public SubDocumentEditor Editor { get { return _Editor; } }
    private SubDocumentEditor _Editor;

    /// <summary>
    /// Редактор документа, для которого открывается редактор поддокумента
    /// </summary>
    public DocumentEditor MainEditor { get { return Editor.MainEditor; } }

    /// <summary>
    /// Коллекция для добавления объектов синхронизации
    /// </summary>
    public DepSyncCollection Syncs { get { return MainEditor.Form.FormProvider.Syncs; } }

    /// <summary>
    /// Возвращает значение свойства SubDocumentEditor.IsReadOnly
    /// </summary>
    public override bool IsReadOnly { get { return Editor.IsReadOnly; } }

    #endregion
  }

  /// <summary>
  /// Делегат события SubDocTypeUI.InitEditForm
  /// </summary>
  /// <param name="sender">Объект SubDocTypeUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitSubDocEditFormEventHandler(object sender, InitSubDocEditFormEventArgs args);

  #endregion
}

