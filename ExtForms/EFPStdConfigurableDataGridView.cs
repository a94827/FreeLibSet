using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ���������� ���������� ���������� ��������� ��� ������������� ������ ��������� EFPGridProducer, ������ ���������� IEFPGridProducer.
  /// ��������� ������������ ����������� ������ EFPGridProducerDataTableRepeater.
  /// </summary>
  public class EFPStdConfigurableDataGridView : EFPConfigurableDataGridView
  {
    #region ������������

    /// <summary>
    /// �������� ����������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    public EFPStdConfigurableDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
    }

    /// <summary>
    /// �������� ����������
    /// </summary>
    /// <param name="controlWithToolBar">����������� ������� � ������ ������������</param>
    public EFPStdConfigurableDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// ��������� �������� �������. ���� �����, �� � ��������� ���� ��������
    /// ������� ��������� �������� �������.
    /// ��������� �������� ����� ������������� �������� AutoSort=true.
    /// ��� ������������� ������������� ������� CurrentOrderChanged, ���������� AutoSort=false �������.
    /// </summary>
    public new EFPGridProducer GridProducer
    {
      // ��� EFPGridProducer �����, �.�. �� �������� ����� InitGridView() � ����������� ������� ����������.
      // ������������ � DocTypeUI.PerformInitGrid()

      get { return (EFPGridProducer)(base.GridProducer); }
      set 
      { 
        base.GridProducer = value;
        if (value != null)
          base.AutoSort = true; // 16.07.2021
      }
    }

    /// <summary>
    /// �������� InitTableRepeaterForGridProducer(), ���� ���� �� ��� ����������� ��������� ������� MasterDataTable ��� MasterDataView
    /// </summary>
    protected override void OnGridProducerPostInit()
    {
      if (_MasterDataTableHasBeenSet) // ���� �� ��������� �������� MasterDataTable ��� MasterDataView?
      {
        if (TableRepeater == null)
          InitTableRepeaterForGridProducer(SourceAsDataTable); // ����� ������ ����������� �����������
        else
          InitTableRepeaterForGridProducer(TableRepeater.MasterTable); // ������� ����� �����������
      }

      base.OnGridProducerPostInit(); // ����� ����������������� �����������
    }

    #endregion

    #region MasterDataTable

    /// <summary>
    /// ������� �������� ��� ��������������� ������������� �������-�����������.
    /// ������ �������������� ������ ���������������� ��������� DataGridView.DataSource ��� EFPDataGridView.SourceAsDataTable
    /// </summary>
    public new DataTable MasterDataTable
    {
      get
      {
        if (TableRepeater == null)
          return SourceAsDataTable;
        else
          return TableRepeater.MasterTable;
      }
      set
      {
        _MasterDataTableHasBeenSet = true;
        if (CurrentConfig != null && GridProducer!=null)
          InitTableRepeaterForGridProducer(value);
        else
          SourceAsDataTable = value; // ����� �� �������� ������������
      }
    }
    private bool _MasterDataTableHasBeenSet;

    /// <summary>
    /// ��������������� ��������� �������� MasterDataTable.
    /// ���������, ���� �� � ��������������� DataView ������ �� ������� ������.
    /// ���� ����, �� ������� ��� ���� ������������� �������.
    /// </summary>
    public new DataView MasterDataView
    {
      get
      {
        if (TableRepeater == null)
          return SourceAsDataView;
        else
          return TableRepeater.MasterTable.DefaultView;
      }
      set
      {
        _MasterDataTableHasBeenSet = true;
        if (CurrentConfig != null && GridProducer != null)
        {
          if (value == null)
            MasterDataTable = null;
          else if (String.IsNullOrEmpty(value.RowFilter))
            MasterDataTable = value.Table;
          else if (TableRepeaterRequired(value.Table))
            MasterDataTable = value.ToTable(); // ��������� ��� ���� ����� �������
          else
            MasterDataTable = value.Table; // ����� �������� ��� �����
        }
        else
          SourceAsDataView = value;
      }
    }

    #endregion

    #region InitTableRepeaterForGridProducer

    /// <summary>
    /// ��������� ������� ��������� �� ������� ����� � ������� ������ <paramref name="masterTable"/>.
    /// ���� ���� ����������� �������, �� ��������� �������-����������� EFPGridProducerDataTableRepeater.
    /// ����� ����� ��������������� �������� DataGridViewColumn.DataPropertyName, ����� �������� ��������
    /// �� �����������, � ������� �� �������-�����������. ��� ��������� ������������ ��� ���� �������� ������������ ����������.
    /// 
    /// ���� � ��������� ��� ���������� ����������� ��������, �������� SourceAsDataTable ��������������� �������� �� <paramref name="masterTable"/>.
    /// ���� ������ �������������� �������-�����������, ��� ���������.
    /// 
    /// �������� GridProducer ������ ���� �����������.
    /// 
    /// � ���������� ���� ���������������� ������������ �������� MasterDataTable ��� MasterDataView.
    /// </summary>
    /// <param name="masterTable">�������� ������� ������</param>
    public void InitTableRepeaterForGridProducer(DataTable masterTable)
    {
      if (GridProducer == null)
        throw new NullReferenceException("�������� GridProducer �� �����������");

      if (masterTable == null)
      {
        TableRepeater = null;
        return;
      }

      //EFPDataGridViewSelection oldSel = Selection;

      if (TableRepeaterRequired(masterTable))
      {
        //bool initColumnSortModeRequired = false;

        EFPGridProducerDataTableRepeater rep = new EFPGridProducerDataTableRepeater(GridProducer, this);
        rep.SlaveTable = masterTable.Clone();
        foreach (EFPDataGridViewColumn col1 in Columns)
        {
          EFPGridProducerColumn col2 = col1.ColumnProducer as EFPGridProducerColumn;
          if (col2 == null)
            continue;
          if (col2.SourceColumnNames == null)
            continue;
          if (col2.SourceColumnNames.Length == 0)
            continue;
          if (!rep.SlaveTable.Columns.Contains(col2.Name))
          {

            Type typ = col2.DataType;
            if (typ == null)
              typ = typeof(string);
            rep.SlaveTable.Columns.Add(col2.Name, typ);
          }
          if (String.IsNullOrEmpty(col1.CustomOrderColumnName))
          {
            if (!(col1.GridColumn is DataGridViewImageColumn))
            {
              col1.CustomOrderColumnName = col2.Name;
              //if (CustomOrderActive)
              //  initColumnSortModeRequired = true; // ��� ����� ������� ����� ����� ���������� SortMode, ����� �������� ������ ��� ������� ���������� ����������� ����� ������ ����
            }
          }
          if (col1.CustomOrderColumnName == col2.Name)
            col1.GridColumn.DataPropertyName = col2.Name; // ������� ������ �� �������� �����������, � ����� �������� �� ���� ������
        }
        
        // �� �����. DataTable.Clone() �������� � ��������� ����
        //DataTools.SetPrimaryKey(rep.SlaveTable, DataTools.GetPrimaryKey(masterTable));

        rep.SlaveTable.TableName = "Repeater";

        rep.MasterTable = masterTable;
        TableRepeater = rep;

        //if (initColumnSortModeRequired)
        //  InitColumnSortMode(); // ��������� SortMode
      }
      else
      {
        TableRepeater = null;
        SourceAsDataTable = masterTable;
      }
      //Selection = oldSel;
    }

    private bool TableRepeaterRequired(DataTable masterTable)
    {
      foreach (EFPDataGridViewColumn col1 in Columns)
      {
        EFPGridProducerColumn col2 = col1.ColumnProducer as EFPGridProducerColumn;
        if (col2 == null)
          continue;
        if (col2.SourceColumnNames == null)
          continue;
        if (col2.SourceColumnNames.Length == 0)
          continue;
        return true;
      }
      return false;
    }

    #endregion
  }

  /// <summary>
  /// ����������� �������, ����������� ���� � ������� EFPGridProducerColumn.GetValue()
  /// </summary>
  public class EFPGridProducerDataTableRepeater : DataTableRepeater
  {
    #region �����������

    /// <summary>
    /// ������� �����������
    /// </summary>
    /// <param name="gridProducer">��������� ���������� ���������. ������ ���� �����</param>
    /// <param name="controlProvider">������ �� ��������� ���������� ���������. ���������� ������ EFPGridProducerColumn.GetValue()</param>
    public EFPGridProducerDataTableRepeater(EFPGridProducer gridProducer, IEFPDataView controlProvider)
    {
      if (gridProducer == null)
        throw new ArgumentNullException("gridProducer");

      _GridProducer = gridProducer;
      _ControlProvider = controlProvider;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ���������� ���������.
    /// �������� � ������������.
    /// </summary>
    public EFPGridProducer GridProducer { get { return _GridProducer; } }
    EFPGridProducer _GridProducer;

    /// <summary>
    /// ��������� ������������ ��������
    /// </summary>
    public IEFPDataView ControlProvider { get { return _ControlProvider; } }
    private IEFPDataView _ControlProvider;

    #endregion

    #region �������

    /// <summary>
    /// ���� - ��� ������� ������������ ����
    /// �������� - ������� � EFPGridProducer
    /// </summary>
    private Dictionary<string, EFPGridProducerColumn> _ColumnDict;

    private DataTableValueArray _VA;

    /// <summary>
    /// ������� ���������� ������� ����������� �����
    /// </summary>
    protected override void OnMasterTableChanged()
    {
      base.OnMasterTableChanged();
      if (MasterTable == null)
      {
        _ColumnDict = null;
        _VA = null;
      }
      else
      {
        _ColumnDict = new Dictionary<string, EFPGridProducerColumn>();
        foreach (DataColumn col1 in SlaveTable.Columns)
        {
          EFPGridProducerColumn col2 = _GridProducer.Columns[col1.ColumnName];
          if (col2 == null)
            continue;
          if (col2.SourceColumnNames == null)
            continue; // ������������� �������
          if (col2.SourceColumnNames.Length == 0)
            continue; // ������� ���� ��/�
          // ���������, ��� ���� ��� �������� �������
          bool allPresents = true;
          for (int j = 0; j < col2.SourceColumnNames.Length; j++)
          {
            if (!MasterTable.Columns.Contains(col2.SourceColumnNames[j]))
            {
              allPresents = false;
              break;
            }
          }

          if (allPresents)
            _ColumnDict.Add(col1.ColumnName, col2);
        }
        _VA = new DataTableValueArray(MasterTable);
      }
    }

    #endregion

    #region OnValueNeeded()

    /// <summary>
    /// �������� �������� ��� ������������ ����
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected override void OnValueNeeded(DataTableRepeaterValueNeededEventArgs args)
    {
      EFPGridProducerColumn col;
      if (_ColumnDict.TryGetValue(args.ColumnName, out col))
      {
        _VA.CurrentRow = args.SourceRow;
        EFPDataViewRowInfo rowInfo = new EFPDataViewRowInfo(_ControlProvider, args.SourceRow, _VA, -1);
        args.Value = col.GetValue(rowInfo);
      }

      base.OnValueNeeded(args);
    }

    #endregion
  }
}
