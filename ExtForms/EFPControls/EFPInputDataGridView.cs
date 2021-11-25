// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер табличного просмотра, предназначенного для редактирования данных DataTable "по месту".
  /// Расширяет EFPDataGridView, в основном, для поддержки операций вставки из буфера обмена с автоматическим
  /// добавлением строк.
  /// Инициализация столбцов выполняется при инициализации свойства DataGridView.DataSource ссылкой на DataTable.
  /// Размеры и форматирование столбцов просмотра можно сделать либор после присоединения таблицы, либо заранее,
  /// используя класс InputDataGridColumns.
  /// Свойство FixedRows позволяет работать с таблицами произвольного размера или с фиксированным числом строк.
  /// В режиме 
  /// </summary>
  public class EFPInputDataGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к DataGridView
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPInputDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элмент и панель инструментов</param>
    public EFPInputDataGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      Control.AutoGenerateColumns = false; // Сами добавляем
      Control.ReadOnly = false;
      Control.MultiSelect = true;
      Control.AllowUserToOrderColumns = false;
      Control.DataSourceChanged += new EventHandler(Control_DataSourceChanged);

      CanInsertCopy = false;
      CanView = false;
      CommandItems.ClipboardInToolBar = true;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Если свойство установлено в true, то пользователь не может добавлять или удалять строки.
    /// Если false (по умолчанию), то можно добавлять строки
    /// </summary>
    public bool FixedRows
    {
      get { return !Control.AllowUserToAddRows; }
      set
      {
        Control.AllowUserToAddRows = !value;
        CanInsert = !value;
        CanDelete = !value;
      }
    }

    #endregion

    #region Обработчики просмотра

    void Control_DataSourceChanged(object sender, EventArgs args)
    {
      if (Columns.Count > 0)
        Columns.Clear();
      DataTable table = Control.DataSource as DataTable;
      if (table == null)
        return;
      for (int i = 0; i < table.Columns.Count; i++)
      {
        string colName = table.Columns[i].ColumnName;
        string title = table.Columns[i].Caption;
        string format = DataTools.GetString(table.Columns[i].ExtendedProperties["Format"]);
        if (String.IsNullOrEmpty(title))
          title = colName;
        int TextWidth = DataTools.GetInt(table.Columns[i].ExtendedProperties["TextWidth"]);
        int MinTextWidth = DataTools.GetInt(table.Columns[i].ExtendedProperties["MinTextWidth"]);
        int FillWeight = DataTools.GetInt(table.Columns[i].ExtendedProperties["FillWeight"]);
        string Align = DataTools.GetString(table.Columns[i].ExtendedProperties["Align"]);

        if (table.Columns[i].DataType == typeof(Boolean))
          Columns.AddBool(colName, true, title);
        else if (table.Columns[i].DataType == typeof(DateTime))
        {
          if (String.IsNullOrEmpty(format))
            Columns.AddDate(colName, true, title);
          else
          {
            Columns.AddText(colName, true, title);
            Columns.LastAdded.TextAlign = HorizontalAlignment.Center;
          }
        }
        else
          Columns.AddText(colName, true, title);

        if (TextWidth > 0)
          Columns.LastAdded.TextWidth = TextWidth;
        if (FillWeight > 0)
        {
          Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          Columns.LastAdded.GridColumn.FillWeight = FillWeight;
        }
        switch (Align.ToUpperInvariant())
        {
          case "LEFT": Columns.LastAdded.TextAlign = HorizontalAlignment.Left; break;
          case "CENTER": Columns.LastAdded.TextAlign = HorizontalAlignment.Center; break;
          case "RIGHT": Columns.LastAdded.TextAlign = HorizontalAlignment.Right; break;
        }
        if (!String.IsNullOrEmpty(format))
          Columns.LastAdded.GridColumn.DefaultCellStyle.Format = format;
      }

      DisableOrdering();
    }


    #endregion

    #region Редактирование

    /// <summary>
    /// Реализует добавление и удаление строк
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected override bool OnEditData(EventArgs args)
    {
      DataTable tbl = SourceAsDataTable;
      int[] rowIndices;

      switch (State)
      {
        case EFPDataGridViewState.Insert:
          if (ReadOnly || FixedRows)
            return true;
          // Добавляем перед текущей строкой столько строк, сколько выбрано
          int nAdded = SelectedRowCount;
          if (nAdded == 0)
            nAdded = 1;
          int index = CurrentRowIndex;
          if (index < 0)
            index = 0;
          rowIndices = new int[nAdded];
          for (int i = 0; i < nAdded; i++)
          {
            tbl.Rows.InsertAt(tbl.NewRow(), index + i);
            rowIndices[i] = index + i;
          }
          SelectedRowIndices = rowIndices;
          return true;

        case EFPDataGridViewState.Delete:
          if (ReadOnly || FixedRows)
            return true;
          rowIndices = SelectedRowIndices;
          Array.Sort<int>(rowIndices);
          for (int i = rowIndices.Length - 1; i >= 0; i--)
          {
            if (rowIndices[i] < tbl.Rows.Count) // последняя строка может быть виртуальной
              tbl.Rows.RemoveAt(rowIndices[i]);
          }
          return true;
      }
      return base.OnEditData(args);
    }

    #endregion

    #region Вставка текста из буфера обмена

    /// <summary>
    /// Можно добавлять строки при вставке, если FixedRows=false.
    /// </summary>
    protected override bool AutoAddRowsAllowed { get { return !FixedRows; } }

    /// <summary>
    /// Добавляет недостающие строки при вставке из буфера обмена
    /// </summary>
    /// <param name="addCount">Количество строк, которые нужно добавить</param>
    protected override void AddInsuficientRows(int addCount)
    {
      if (FixedRows)
        throw new InvalidOperationException("FixedRows=true");

      DataTable tbl = SourceAsDataTable;
      for (int i = 0; i < addCount; i++)
        tbl.Rows.Add();
    }

    #endregion

    #region Сжатие таблицы при закрытии формы

    /// <summary>
    /// При закрытии формы выполняет сжатие таблицы данных
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (BaseProvider.ValidateReason == EFPFormValidateReason.Closing || BaseProvider.ValidateReason == EFPFormValidateReason.ValidateForm)
      {
        if ((!Control.ReadOnly) && (!FixedRows))
          CompactTable();

        if (!Control.ReadOnly)
          SourceAsDataTable.AcceptChanges();
      }
    }

    private void CompactTable()
    {
      DataTable tbl = SourceAsDataTable;
      for (int i = tbl.Rows.Count - 1; i >= 0; i--)
      {
        DataRow row = tbl.Rows[i];
        bool isRowEmpty = true;
        for (int j = 0; j < tbl.Columns.Count; j++)
        {
          if (!String.IsNullOrEmpty(tbl.Columns[j].Expression))
            continue;
          if (row.IsNull(j))
            continue;
          if (tbl.Columns[j].DataType == typeof(string))
          {
            if (row[j].ToString().Trim().Length == 0)
              continue;
          }

          isRowEmpty = false;
          break;
        }
        if (isRowEmpty)
          tbl.Rows.RemoveAt(i);
      }
    }

    #endregion
  }

    /// <summary>
  /// Класс для установки свойств DataColumn.ExtendedProperties для табличного просмотра InputGridDataDialog
  /// </summary>
  public sealed class InputDataGridColumn
  {
    // Этот класс не сериализуется.

    #region Защищенный конструктор

    internal InputDataGridColumn(DataColumn column)
    {
      _Column = column;
    }


    #endregion

    #region Основные свойства

    /// <summary>
    /// Столбец таблицы данных
    /// </summary>
    public DataColumn Column { get { return _Column; } }
    private DataColumn _Column;

    #endregion

    #region Значения для столбцов

    // "Format" - задает формат числового столбца или даты/времени.
    // "TextWidth" - задает ширину столбца в текстовых единицах.
    // "MinTextWidth" - задает минимальную ширину столбца в текстовых единицах.
    // "FillWeight" - задает относительную ширину столбца, если столбец должен заполнять просмотр по ширине.
    // "Align" - задает горизонтальное выравнивание (строковое значение "Left", "Center" или "Right").

    /// <summary>
    /// Горизонтальное выравнивание
    /// </summary>
    public HorizontalAlignment Align
    {
      get
      {
        string s = DataTools.GetString(Column.ExtendedProperties["Align"]);
        if (String.IsNullOrEmpty(s))
        {
          if (DataTools.IsNumericType(Column.DataType))
            return HorizontalAlignment.Right;
          if (Column.DataType == typeof(DateTime) || Column.DataType == typeof(bool))
            return HorizontalAlignment.Center;
          else
            return HorizontalAlignment.Left;
        }
        else
          return StdConvert.ToEnum<HorizontalAlignment>(s);
      }
      set
      {
        Column.ExtendedProperties["Align"] = value.ToString();
      }
    }

    /// <summary>
    /// Формат для числового столбца или столбца даты/времени.
    /// </summary>
    public string Format
    {
      get       {        return DataTools.GetString(Column.ExtendedProperties["Format"]);      }
      set      {        Column.ExtendedProperties["Format"] = value;      }
    }

    /// <summary>
    /// Ширина столбца как количество символов.
    /// </summary>
    public int TextWidth
    {      
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["TextWidth"]));      }
      set      {        Column.ExtendedProperties["TextWidth"] = StdConvert.ToString(value);      }
    }

    /// <summary>
    /// Минимальная ширина столбца как количество символов.
    /// </summary>
    public int MinTextWidth
    {
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["MinTextWidth"]));      }
      set      {        Column.ExtendedProperties["MinTextWidth"] = StdConvert.ToString(value);      }
    }

    /// <summary>
    /// Весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
    /// По умолчанию - 0 - используется ширина столбца, задаваемая TextWidth.
    /// </summary>
    public int FillWeight
    {
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["FillWeight"]));      }
      set      {        Column.ExtendedProperties["FillWeight"] = StdConvert.ToString(value);      }
    }

    #endregion
  }

  /// <summary>
  /// Коллекция объектов InputDataGridColumn с доступом по имени столбца.
  /// Реализует свойство InputDataGridDialog.Columns и может использоваться в прикладном коде для
  /// инициализации столбцов EFPInputDataGridView. Свойство EFPInputDataGridView.SourceAsDataTable должно
  /// устанавливаться после окончания работы с InputDataGridColumns.
  /// </summary>
  public sealed class InputDataGridColumns
  {
    // Учитываем возможность, что может появится класс табличного просмотра InputGridDataView без блока диалога.
    // Этот класс не сериализуется.

    #region Защищенный конструктор

    #region Конструктор

    /// <summary>
    /// Создает объект, привязанный к таблице данных
    /// </summary>
    /// <param name="table">Таблица данных. Не может быть null</param>
    public InputDataGridColumns(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      _Table = table;
      _Dict = new TypedStringDictionary<InputDataGridColumn>(true);
    }

    #endregion

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Таблица данных, в которой находятся столбцы
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    private TypedStringDictionary<InputDataGridColumn> _Dict;

    /// <summary>
    /// Доступ к свойствам столбца по имени.
    /// На момент вызова столбец должен быть добавлен в таблицу.
    /// </summary>
    /// <param name="columnName">Имя столбца (свойство DataColumn.ColumnName)</param>
    /// <returns>Свойства столбца табличного просмотра</returns>
    public InputDataGridColumn this[string columnName]
    {
      get
      {
        InputDataGridColumn info;
        if (!_Dict.TryGetValue(columnName, out info))
        {
          DataColumn column = Table.Columns[columnName];
          if (column == null)
          {
            if (String.IsNullOrEmpty(columnName))
              throw new ArgumentNullException("columnName");
            else
              throw new ArgumentException("В таблице " + Table.ToString() + " нет столбца с именем \"" + columnName + "\"");
          }
          info = new InputDataGridColumn(column);
          _Dict.Add(columnName, info);
        }
        return info;
      }
    }

    /// <summary>
    /// Доступ к свойствам столбца.
    /// На момент вызова столбец должен быть добавлен в таблицу.
    /// </summary>
    /// <param name="column">Столбец DataTable</param>
    /// <returns>Свойства столбца табличного просмотра</returns>
    public InputDataGridColumn this[DataColumn column]
    {
      get
      {
        if (column == null)
          throw new ArgumentNullException("column");
        return this[column.ColumnName];
      }
    }

    /// <summary>
    /// Доступ к свойствам последнего столбца, который был добавлен в таблицу.
    /// </summary>
    public InputDataGridColumn LastAdded
    {
      get
      {
        if (Table.Columns.Count == 0)
          return null;
        else
          return this[Table.Columns[Table.Columns.Count - 1]];
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода табличных данных.
  /// </summary>
  public class InputDataGridDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект значениями по умолчанию.
    /// Перед выводом диалога должно быть установлено свойство DataSource
    /// </summary>
    public InputDataGridDialog()
    {
      Title = "Таблица";
      ImageKey = "Table";
      _Table = new DataTable();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - редактируемая таблица данных.
    /// Список столбцов должен быть заполнен перед показом диалога или свойство должно быть установлено.
    /// По умолчанию содержит ссылку на пустую таблицу
    /// </summary>
    public DataTable Table
    {
      get { return _Table; }
      set
      {
        if (value == null)
          _Table = new DataTable();
        else
          _Table = value;
        _Columns = null;
      }
    }
    private DataTable _Table;

    /// <summary>
    /// Доступ к расширенным свойствам столбцов
    /// </summary>
    public InputDataGridColumns Columns
    {
      get
      {
        if (_Columns == null)
          _Columns = new InputDataGridColumns(_Table);
        return _Columns;
      }
    }
    private InputDataGridColumns _Columns;

    /// <summary>
    /// Фиксированные строки.
    /// Если false (по умолчанию), то пользователь может добавлять и удалять строки в таблицу.
    /// Если true, то пользователь может только редактировать существующие строки в таблице.
    /// </summary>
    public bool FixedRows
    {
      get { return _FixedRows; }
      set { _FixedRows = value; }
    }
    private bool _FixedRows;

    /// <summary>
    /// Если true, то просмотр будет предназначен только для просмотра, но не для редактирования данных.
    /// По умолчанию - false.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Информационный текст, выводимый в нижней части диалога
    /// </summary>
    public string InfoText
    {
      get
      {
        if (_InfoText == null)
        {
          if (ReadOnly || FixedRows)
            return String.Empty;
          else
            return "При вставке из буфера обмена недостающие строки будут добавлены автоматически";
        }
        else
          return _InfoText;
      }
      set { _InfoText = value; }
    }
    private string _InfoText;

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показывает блок диалога с таблицей
    /// </summary>
    /// <returns>Результат выполнения диалога</returns>
    public override DialogResult ShowDialog()
    {
      if (_Table == null)
      {
        EFPApp.ErrorMessageBox("Данные не присоединены", Title);
        return DialogResult.Cancel;
      }

      DialogResult res;

      using (OKCancelGridForm form = new OKCancelGridForm(!String.IsNullOrEmpty(Prompt)))
      {
        InitFormTitle(form);
        form.FormProvider.HelpContext = HelpContext;
        if (!String.IsNullOrEmpty(Prompt))
          form.GroupBox.Text = Prompt;
        if (ReadOnly)
          WinFormsTools.OkCancelFormToOkOnly(form);

        EFPInputDataGridView efpGrid = new EFPInputDataGridView(form.ControlWithToolBar);
        efpGrid.Control.DataSource = Table;
        efpGrid.ReadOnly = ReadOnly;
        efpGrid.Control.ReadOnly = ReadOnly;
        efpGrid.FixedRows = FixedRows || ReadOnly;

        if (!String.IsNullOrEmpty(InfoText))
          form.AddInfoLabel(DockStyle.Bottom).Text = InfoText;

        res = EFPApp.ShowDialog(form, false, DialogPosition);
      }

      if (res == DialogResult.OK)
        Table.AcceptChanges();

      return res;
    }

    #endregion
  }

}
