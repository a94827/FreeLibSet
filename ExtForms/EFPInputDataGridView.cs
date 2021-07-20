using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ��������� ���������� ���������, ���������������� ��� �������������� ������ DataTable "�� �����".
  /// ��������� EFPDataGridView, � ��������, ��� ��������� �������� ������� �� ������ ������ � ��������������
  /// ����������� �����.
  /// ������������� �������� ����������� ��� ������������� �������� DataGridView.DataSource ������� �� DataTable.
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
      int[] rowIndexes;

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
          rowIndexes = new int[nAdded];
          for (int i = 0; i < nAdded; i++)
          {
            tbl.Rows.InsertAt(tbl.NewRow(), index + i);
            rowIndexes[i] = index + i;
          }
          SelectedRowIndices = rowIndexes;
          return true;

        case EFPDataGridViewState.Delete:
          if (ReadOnly || FixedRows)
            return true;
          rowIndexes = SelectedRowIndices;
          Array.Sort<int>(rowIndexes);
          for (int i = rowIndexes.Length - 1; i >= 0; i--)
          {
            if (rowIndexes[i] < tbl.Rows.Count) // ��������� ������ ����� ���� �����������
              tbl.Rows.RemoveAt(rowIndexes[i]);
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
  /// ����� ��� ��������� ������� DataColumn.ExtendedProperties ��� ���������� ��������� EFPInputGridView �
  /// ������� InputGridDataDialog.
  /// ���������� ��� ����� ��������� ������ ��� �������, ������� ����� ������������ � EFPInputGridView.
  /// ����� InputGridDataDialog ����� �������� Columns ��� ������ �� ���������.
  /// </summary>
  public class EFPInputDataGridViewColumnProperties : AgeyevAV.RI.InputGridDataColumnProperties
  {
    // ����� �������������� ������ ��� ���� ������ HorizontalAlignment

    #region ������������

    /// <summary>
    /// ������� ������, �������������� � �������, ����������� ��������� �������
    /// </summary>
    /// <param name="table">�������, ���������� �������� ������� ����� ���������</param>
    public EFPInputDataGridViewColumnProperties(DataTable table)
      : this(table, false)
    {
    }

    /// <summary>
    /// ������� ������, �������������� � �������
    /// </summary>
    /// <param name="table">�������, ���������� �������� ������� ����� ���������</param>
    /// <param name="isReadOnly">���� true, �� ������ �������� ������ ������ ��������, �� �� ������������� ��</param>
    public EFPInputDataGridViewColumnProperties(DataTable table, bool isReadOnly)
      :base(table, isReadOnly)
    {
    }

    #endregion

    #region Align

    /// <summary>
    /// ���������� �������������� ������������ ��� �������.
    /// �� ������ ������ � ������� Table ������ ���� �������� �������, ��� �������� ��������������� ��������
    /// </summary>
    /// <param name="columnName">��� ������� DataColumn.ColumnName. ���� ������ ������ ������,
    /// �� �������� ����� ��������� � ���������� ������������ ������� ������� Table.</param>
    /// <param name="value">��������������� ��������</param>
    public /*new */void SetAlign(string columnName, HorizontalAlignment value)
    {
      base.SetAlign(columnName, (AgeyevAV.RI.HorizontalAlignment)(int)value);
    }

    /// <summary>
    /// ���������� �������������� ������������ ��� �������.
    /// �� ������ ������ � ������� Table ������ ���� �������� �������, ��� �������� ��������������� ��������
    /// </summary>
    /// <param name="column">������� �������. ���� ������ null,
    /// �� �������� ����� ��������� � ���������� ������������ ������� ������� Table.
    /// ������� ������������ ����������, ����������� ������ ������, ����� �� �������� ����� ���������� ���� "(DataColumn)null".
    /// </param>
    /// <param name="value">��������������� ��������</param>
    public /*new */void SetAlign(DataColumn column, HorizontalAlignment value)
    {
      base.SetAlign(column, (AgeyevAV.RI.HorizontalAlignment)(int)value);
    }

    /// <summary>
    /// �������� �������������� ������������ ��� �������.
    /// ���� �������� �� ���� ����������� � ����� ����, ����� ���������� �������� �� ���������.
    /// </summary>
    /// <param name="columnName">��� ������� DataColumn.ColumnName. ���� ������ ������ ������,
    /// �� ����� ���������� �������� ��� ���������� ������������ ������� ������� Table.</param>
    /// <returns>������������� ��������</returns>
    public new HorizontalAlignment GetAlign(string columnName)
    {
      return (HorizontalAlignment)(int)(base.GetAlign(columnName));
    }

    /// <summary>
    /// �������� �������������� ������������ ��� �������.
    /// ���� �������� �� ���� ����������� � ����� ����, ����� ���������� �������� �� ���������.
    /// </summary>
    /// <param name="column">������� �������. ���� ������ null ��� ������ ������,
    /// �� ����� ���������� �������� ��� ���������� ������������ ������� ������� Table.
    /// ������� ������������ ����������, ����������� ������ ������, ����� �� �������� ����� ���������� ���� "(DataColumn)null".
    /// </param>
    /// <returns>������������� ��������</returns>
    public new HorizontalAlignment GetAlign(DataColumn column)
    {
      return (HorizontalAlignment)(int)(base.GetAlign(column));
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����� ��������� ������.
  /// </summary>
  public class InputGridDataDialog : BaseInputDialog
  {
    #region �����������

    /// <summary>
    /// �������������� ������ ���������� �� ���������.
    /// ����� ������� ������� ������ ���� ����������� �������� DataSource
    /// </summary>
    public InputGridDataDialog()
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
        _Table = value;
        _Columns = null;
      }
    }
    private DataTable _Table;

    /// <summary>
    /// ������ � ����������� ��������� ��������
    /// </summary>
    public EFPInputDataGridViewColumnProperties Columns
    {
      get
      {
        if (_Columns == null && _Table != null)
          _Columns = new EFPInputDataGridViewColumnProperties(_Table);
        return _Columns;
      }
    }
    private EFPInputDataGridViewColumnProperties _Columns;

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
