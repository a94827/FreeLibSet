// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ��������� ���������� ���������, ���������������� ��� �������������� ������ DataTable "�� �����".
  /// ��������� EFPDataGridView, � ��������, ��� ��������� �������� ������� �� ������ ������ � ��������������
  /// ����������� �����.
  /// ������������� �������� ����������� ��� ������������� �������� Data.
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

      CanInsertCopy = false;
      CanView = false;
      CommandItems.ClipboardInToolBar = true;
      UseRowImages = true;

      _ValidatingResults = new Dictionary<int, RowValidatingResults>();
      _TempValidableObject = new UISimpleValidableObject();
    }

    #endregion

    #region �������� FixedRows

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

    #region �������� Data

    /// <summary>
    /// ��������������� ������ � ��������� ��������.
    /// ��������� �������� �������� � ������������� ���������
    /// </summary>
    public UIInputGridData Data
    {
      get { return _Data; }
      set
      {
        if (_Data != null)
          _Data.Table.DefaultView.ListChanged -= DV_ListChanged;

        _Data = value;
        InitColumns();

        if (value == null)
          SourceAsDataView = null;
        else
        {
          SourceAsDataView = value.Table.DefaultView;
          if (ProviderState == EFPControlProviderState.Attached)
            _Data.Table.DefaultView.ListChanged += DV_ListChanged;
        }
      }
    }
    private UIInputGridData _Data;

    #endregion

    #region ������������� ��������

    private void InitColumns()
    {
      if (Columns.Count > 0)
        Columns.Clear();

      if (_Data == null)
        return;

      for (int i = 0; i < _Data.Table.Columns.Count; i++)
      {
        DataColumn col = _Data.Table.Columns[i];
        UIInputGridData.ColumnInfo colInfo = _Data.Columns[col.ColumnName];

        string title = col.Caption;
        if (String.IsNullOrEmpty(title))
          title = col.ColumnName;

        if (col.DataType == typeof(Boolean))
          Columns.AddBool(col.ColumnName, true, title);
        else if (col.DataType == typeof(DateTime))
        {
          if (String.IsNullOrEmpty(colInfo.Format))
            Columns.AddDate(col.ColumnName, true, title);
          else
            Columns.AddText(col.ColumnName, true, title);
        }
        else
          Columns.AddText(col.ColumnName, true, title);

        if (colInfo.TextWidth > 0)
          Columns.LastAdded.TextWidth = colInfo.TextWidth;
        if (colInfo.FillWeight > 0)
        {
          Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          Columns.LastAdded.GridColumn.FillWeight = colInfo.FillWeight;
        }
        switch (colInfo.Align)
        {
          case UIHorizontalAlignment.Left: Columns.LastAdded.TextAlign = HorizontalAlignment.Left; break;
          case UIHorizontalAlignment.Center: Columns.LastAdded.TextAlign = HorizontalAlignment.Center; break;
          case UIHorizontalAlignment.Right: Columns.LastAdded.TextAlign = HorizontalAlignment.Right; break;
        }
        if (!String.IsNullOrEmpty(colInfo.Format))
          Columns.LastAdded.GridColumn.DefaultCellStyle.Format = colInfo.Format;
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

    #region ������ � �������� ������� ��� �������� �����

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
        {
          SourceAsDataTable.AcceptChanges();
          _ValidatingResults.Clear(); // ������ ����� ����������

          for (int i = 0; i < Data.Table.DefaultView.Count;i++ )
          {
            if (ValidateState == UIValidateState.Error)
              break;

            RowValidatingResults rvr = GetRowValidatingResults(i);

            if (rvr.CellErrors != null)
            {
              foreach (KeyValuePair<int, string> pair in rvr.CellErrors)
              {
                Control.CurrentCell = Control[pair.Key, i];
                SetError(pair.Value);
                break;
              }
            }

            if (ValidateState == UIValidateState.Ok && rvr.CellWarnings != null)
            {
              foreach (KeyValuePair<int, string> pair in rvr.CellWarnings)
              {
                Control.CurrentCell = Control[pair.Key, i];
                SetWarning(pair.Value);
                break;
              }
            }
          }
        }
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

    #region ��������� ���������

    /// <summary>
    /// ���������� �������� ��� ����� ������
    /// </summary>
    private struct RowValidatingResults
    {
      #region ����

      /// <summary>
      /// ������ � ��������.
      /// ���� - ������ ������� �������.
      /// �������� - ����� ���������.
      /// ���� ���� �������� null, �� � ������ ��� ��������� �� ������
      /// </summary>
      public Dictionary<int, string> CellErrors;

      /// <summary>
      /// ������ � ����������������.
      /// ���� - ������ ������� �������.
      /// �������� - ����� ���������.
      /// ���� ���� �������� null, �� � ������ ��� ��������������
      /// </summary>
      public Dictionary<int, string> CellWarnings;

      #endregion
    }

    /// <summary>
    /// ����������� �������� �����.
    /// ���� - ������ ������ � ���������
    /// �������� - ��������� �������� �� ������
    /// </summary>
    private Dictionary<int, RowValidatingResults> _ValidatingResults;

    void DV_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs args)
    {
      if (args.ListChangedType == System.ComponentModel.ListChangedType.ItemChanged)
        _ValidatingResults.Remove(args.NewIndex);
      else
        _ValidatingResults.Clear(); // ������� ��������� ��������� ������
    }

    /// <summary>
    /// ������������� ����������� ListChanged
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      if (_Data != null)
        _Data.Table.DefaultView.ListChanged += DV_ListChanged;
    }

    /// <summary>
    /// ���������� ����������� ListChanged
    /// </summary>
    protected override void OnDetached()
    {
      if (_Data != null)
        _Data.Table.DefaultView.ListChanged -= DV_ListChanged;

      base.OnDetached();
    }

    #endregion

    #region ��������� ����� � ��������

    /// <summary>
    /// ��������� ������ ��������� ��� ������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      base.OnGetRowAttributes(args);
      if (Data == null)
        return;
      if (args.RowIndex < 0 || args.RowIndex >= Data.Table.DefaultView.Count)
        return; // ��������� ����� ������

      RowValidatingResults rvr = GetRowValidatingResults(args.RowIndex);

      if (rvr.CellErrors != null)
      {
        foreach (KeyValuePair<int, string> pair in rvr.CellErrors)
        {
          string colTitle = Columns[pair.Key].GridColumn.HeaderText;
          args.AddRowError(colTitle + ". " + pair.Value, Data.Table.Columns[pair.Key].ColumnName);
        }
      }

      if (rvr.CellWarnings != null)
      {
        foreach (KeyValuePair<int, string> pair in rvr.CellWarnings)
        {
          string colTitle = Columns[pair.Key].GridColumn.HeaderText;
          args.AddRowWarning(colTitle + ". " + pair.Value, Data.Table.Columns[pair.Key].ColumnName);
        }
      }
    }

    private RowValidatingResults GetRowValidatingResults(int rowIndex)
    {
      RowValidatingResults rvr;

      if (!_ValidatingResults.TryGetValue(rowIndex, out rvr))
      {
        // ��������� ��������
        rvr = new RowValidatingResults();

        try
        {
          ValidateRow(rowIndex, ref rvr);
        }
        catch (Exception e)
        {
          rvr.CellErrors = new Dictionary<int, string>();
          rvr.CellErrors.Add(0, "������ ��� �������� ������. " + e.Message);
        }

        _ValidatingResults.Add(rowIndex, rvr);
      }
      return rvr;
    }

    /// <summary>
    /// ����� �� ��������� ��� ������ ������
    /// </summary>
    UISimpleValidableObject _TempValidableObject;

    /// <summary>
    /// �������� ����� ������
    /// </summary>
    private void ValidateRow(int rowIndex, ref RowValidatingResults rvr)
    {
      DataRow row = GetDataRow(rowIndex);
      if (row == null)
        return;
      Data.InternalSetValidatingRow(row);

      for (int i = 0; i < Data.Table.Columns.Count; i++)
      {
        _TempValidableObject.Clear();
        if (row.IsNull(i))
        {
          switch (Data.Columns[i].CanBeEmptyMode)
          {
            case UIValidateState.Error:
              _TempValidableObject.SetError("�������� ������ ���� ������");
              break;
            case UIValidateState.Warning:
              _TempValidableObject.SetWarning("��������, ��������, ������ ���� ������");
              break;
          }
        }

        if (Data.Columns[i].HasValidators)
          Data.Columns[i].Validators.Validate(_TempValidableObject);

        switch (_TempValidableObject.ValidateState)
        {
          case UIValidateState.Error:
            if (rvr.CellErrors == null)
              rvr.CellErrors = new Dictionary<int, string>();
            rvr.CellErrors.Add(i, _TempValidableObject.Message);
            break;

          case UIValidateState.Warning:
            if (rvr.CellWarnings == null)
              rvr.CellWarnings = new Dictionary<int, string>();
            rvr.CellWarnings.Add(i, _TempValidableObject.Message);
            break;
        }
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
      _Data = new UIInputGridData();
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� �������� - ������������� ������� ������.
    /// � ������� ������ ���� ��������� ������� ����� ������� �������.
    /// ����� �������������� ������� ��� ���������, ���� ����������� �������� DataColumn.Expression.
    /// �������������� ��������� ��� ����������� ��������, ��������, ������, ��������� � ������� ������� ��������� Columns.
    /// 
    /// ���� FixedRows=true, �� � ������� ������� �������� ������, ����� ������������ �� ������ ������ ������.
    /// 
    /// ����� �������� ����� ������� �������� Table ������ ���� ��������� ������, ��� ��� ��� �������� ������ �� ����� �������.
    /// �� ��������� - ������ �������.
    /// </summary>
    public UIInputGridData Data
    {
      get { return _Data; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Data = value;
      }
    }
    private UIInputGridData _Data;


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
        efpGrid.Data = this.Data;
        efpGrid.ReadOnly = ReadOnly;
        efpGrid.Control.ReadOnly = ReadOnly;
        efpGrid.FixedRows = FixedRows || ReadOnly;

        if (!String.IsNullOrEmpty(InfoText))
          form.AddInfoLabel(DockStyle.Bottom).Text = InfoText;

        res = EFPApp.ShowDialog(form, false, DialogPosition);
      }

      if (res == DialogResult.OK)
        this.Data.Table.AcceptChanges();

      return res;
    }

    #endregion
  }

}
