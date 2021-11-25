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
  /// ��������� ���������� ���������, ���������������� ��� �������������� ������ DataTable "�� �����".
  /// ��������� EFPDataGridView, � ��������, ��� ��������� �������� ������� �� ������ ������ � ��������������
  /// ����������� �����.
  /// ������������� �������� ����������� ��� ������������� �������� DataGridView.DataSource ������� �� DataTable.
  /// ������� � �������������� �������� ��������� ����� ������� ����� ����� ������������� �������, ���� �������,
  /// ��������� ����� InputDataGridColumns.
  /// �������� FixedRows ��������� �������� � ��������� ������������� ������� ��� � ������������� ������ �����.
  /// � ������ 
  /// </summary>
  public class EFPInputDataGridView : EFPDataGridView
  {
    #region ������������

    /// <summary>
    /// ������� ������, ����������� � DataGridView
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� Windows Forms</param>
    public EFPInputDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// ������� ������, ����������� � ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">����������� ������ � ������ ������������</param>
    public EFPInputDataGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      Control.AutoGenerateColumns = false; // ���� ���������
      Control.ReadOnly = false;
      Control.MultiSelect = true;
      Control.AllowUserToOrderColumns = false;
      Control.DataSourceChanged += new EventHandler(Control_DataSourceChanged);

      CanInsertCopy = false;
      CanView = false;
      CommandItems.ClipboardInToolBar = true;
    }

    #endregion

    #region ����� ��������

    /// <summary>
    /// ���� �������� ����������� � true, �� ������������ �� ����� ��������� ��� ������� ������.
    /// ���� false (�� ���������), �� ����� ��������� ������
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

    #region ����������� ���������

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

    #region ��������������

    /// <summary>
    /// ��������� ���������� � �������� �����
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
          // ��������� ����� ������� ������� ������� �����, ������� �������
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
            if (rowIndices[i] < tbl.Rows.Count) // ��������� ������ ����� ���� �����������
              tbl.Rows.RemoveAt(rowIndices[i]);
          }
          return true;
      }
      return base.OnEditData(args);
    }

    #endregion

    #region ������� ������ �� ������ ������

    /// <summary>
    /// ����� ��������� ������ ��� �������, ���� FixedRows=false.
    /// </summary>
    protected override bool AutoAddRowsAllowed { get { return !FixedRows; } }

    /// <summary>
    /// ��������� ����������� ������ ��� ������� �� ������ ������
    /// </summary>
    /// <param name="addCount">���������� �����, ������� ����� ��������</param>
    protected override void AddInsuficientRows(int addCount)
    {
      if (FixedRows)
        throw new InvalidOperationException("FixedRows=true");

      DataTable tbl = SourceAsDataTable;
      for (int i = 0; i < addCount; i++)
        tbl.Rows.Add();
    }

    #endregion

    #region ������ ������� ��� �������� �����

    /// <summary>
    /// ��� �������� ����� ��������� ������ ������� ������
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
  /// ����� ��� ��������� ������� DataColumn.ExtendedProperties ��� ���������� ��������� InputGridDataDialog
  /// </summary>
  public sealed class InputDataGridColumn
  {
    // ���� ����� �� �������������.

    #region ���������� �����������

    internal InputDataGridColumn(DataColumn column)
    {
      _Column = column;
    }


    #endregion

    #region �������� ��������

    /// <summary>
    /// ������� ������� ������
    /// </summary>
    public DataColumn Column { get { return _Column; } }
    private DataColumn _Column;

    #endregion

    #region �������� ��� ��������

    // "Format" - ������ ������ ��������� ������� ��� ����/�������.
    // "TextWidth" - ������ ������ ������� � ��������� ��������.
    // "MinTextWidth" - ������ ����������� ������ ������� � ��������� ��������.
    // "FillWeight" - ������ ������������� ������ �������, ���� ������� ������ ��������� �������� �� ������.
    // "Align" - ������ �������������� ������������ (��������� �������� "Left", "Center" ��� "Right").

    /// <summary>
    /// �������������� ������������
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
    /// ������ ��� ��������� ������� ��� ������� ����/�������.
    /// </summary>
    public string Format
    {
      get       {        return DataTools.GetString(Column.ExtendedProperties["Format"]);      }
      set      {        Column.ExtendedProperties["Format"] = value;      }
    }

    /// <summary>
    /// ������ ������� ��� ���������� ��������.
    /// </summary>
    public int TextWidth
    {      
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["TextWidth"]));      }
      set      {        Column.ExtendedProperties["TextWidth"] = StdConvert.ToString(value);      }
    }

    /// <summary>
    /// ����������� ������ ������� ��� ���������� ��������.
    /// </summary>
    public int MinTextWidth
    {
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["MinTextWidth"]));      }
      set      {        Column.ExtendedProperties["MinTextWidth"] = StdConvert.ToString(value);      }
    }

    /// <summary>
    /// ������� ����������� ��� �������, ������� ������ ��������� ������� �� ������.
    /// �� ��������� - 0 - ������������ ������ �������, ���������� TextWidth.
    /// </summary>
    public int FillWeight
    {
      get      {        return StdConvert.ToInt32(DataTools.GetString(Column.ExtendedProperties["FillWeight"]));      }
      set      {        Column.ExtendedProperties["FillWeight"] = StdConvert.ToString(value);      }
    }

    #endregion
  }

  /// <summary>
  /// ��������� �������� InputDataGridColumn � �������� �� ����� �������.
  /// ��������� �������� InputDataGridDialog.Columns � ����� �������������� � ���������� ���� ���
  /// ������������� �������� EFPInputDataGridView. �������� EFPInputDataGridView.SourceAsDataTable ������
  /// ��������������� ����� ��������� ������ � InputDataGridColumns.
  /// </summary>
  public sealed class InputDataGridColumns
  {
    // ��������� �����������, ��� ����� �������� ����� ���������� ��������� InputGridDataView ��� ����� �������.
    // ���� ����� �� �������������.

    #region ���������� �����������

    #region �����������

    /// <summary>
    /// ������� ������, ����������� � ������� ������
    /// </summary>
    /// <param name="table">������� ������. �� ����� ���� null</param>
    public InputDataGridColumns(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      _Table = table;
      _Dict = new TypedStringDictionary<InputDataGridColumn>(true);
    }

    #endregion

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ������� ������, � ������� ��������� �������
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    private TypedStringDictionary<InputDataGridColumn> _Dict;

    /// <summary>
    /// ������ � ��������� ������� �� �����.
    /// �� ������ ������ ������� ������ ���� �������� � �������.
    /// </summary>
    /// <param name="columnName">��� ������� (�������� DataColumn.ColumnName)</param>
    /// <returns>�������� ������� ���������� ���������</returns>
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
              throw new ArgumentException("� ������� " + Table.ToString() + " ��� ������� � ������ \"" + columnName + "\"");
          }
          info = new InputDataGridColumn(column);
          _Dict.Add(columnName, info);
        }
        return info;
      }
    }

    /// <summary>
    /// ������ � ��������� �������.
    /// �� ������ ������ ������� ������ ���� �������� � �������.
    /// </summary>
    /// <param name="column">������� DataTable</param>
    /// <returns>�������� ������� ���������� ���������</returns>
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
    /// ������ � ��������� ���������� �������, ������� ��� �������� � �������.
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
  /// ������ ��� ����� ��������� ������.
  /// </summary>
  public class InputDataGridDialog : BaseInputDialog
  {
    #region �����������

    /// <summary>
    /// �������������� ������ ���������� �� ���������.
    /// ����� ������� ������� ������ ���� ����������� �������� DataSource
    /// </summary>
    public InputDataGridDialog()
    {
      Title = "�������";
      ImageKey = "Table";
      _Table = new DataTable();
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� �������� - ������������� ������� ������.
    /// ������ �������� ������ ���� �������� ����� ������� ������� ��� �������� ������ ���� �����������.
    /// �� ��������� �������� ������ �� ������ �������
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
    /// ������ � ����������� ��������� ��������
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
    /// ������������� ������.
    /// ���� false (�� ���������), �� ������������ ����� ��������� � ������� ������ � �������.
    /// ���� true, �� ������������ ����� ������ ������������� ������������ ������ � �������.
    /// </summary>
    public bool FixedRows
    {
      get { return _FixedRows; }
      set { _FixedRows = value; }
    }
    private bool _FixedRows;

    /// <summary>
    /// ���� true, �� �������� ����� ������������ ������ ��� ���������, �� �� ��� �������������� ������.
    /// �� ��������� - false.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// �������������� �����, ��������� � ������ ����� �������
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
            return "��� ������� �� ������ ������ ����������� ������ ����� ��������� �������������";
        }
        else
          return _InfoText;
      }
      set { _InfoText = value; }
    }
    private string _InfoText;

    #endregion

    #region ����� �������

    /// <summary>
    /// ���������� ���� ������� � ��������
    /// </summary>
    /// <returns>��������� ���������� �������</returns>
    public override DialogResult ShowDialog()
    {
      if (_Table == null)
      {
        EFPApp.ErrorMessageBox("������ �� ������������", Title);
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
