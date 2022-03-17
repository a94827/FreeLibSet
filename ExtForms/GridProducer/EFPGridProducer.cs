// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms
{
  #region IEFPGridProducer

  /// <summary>
  /// ���������� ��������� ���������� ���������� ���������
  /// �� �������� �������, ����� ���������������� ������� ���������� ���������
  /// </summary>
  public interface IEFPGridProducer : IReadOnlyObject
  {
    /// <summary>
    /// ���������� ���������� ��������� ��������� ���������� ��� ����������� ��������� ������ ����.
    /// </summary>
    int OrderCount { get; }

    /// <summary>
    /// ����� ������ ������� ������� � ��������� ��������� � ������������ � ������������� �������������
    /// (��������� �������� EFPDataGridView.CurrentConfig)
    /// �� ������ ������ ��������� �������� �� �������� ��������
    /// ����� ������ ����� ������ ������� ������� EFPDataGridView.PerformGridProducerPostInit()
    /// </summary>
    /// <param name="controlProvider">����������� ��������</param>
    /// <param name="reInit">true, ���� ������������� ����������� ��������</param>
    void InitGridView(EFPDataGridView controlProvider, bool reInit);

    /// <summary>
    /// ����� ������ ������� ������� � ������������� ��������� � ������������ � ������������� �������������
    /// (��������� �������� EFPDataTreeView.CurrentConfig)
    /// �� ������ ������ ��������� �������� �� �������� ��������
    /// ����� ������ ����� ������ ������� ������� EFPDataTreeView.PerformGridProducerPostInit()
    /// </summary>
    /// <param name="controlProvider">����������� ��������</param>
    /// <param name="reInit">true, ���� ������������� ����������� ��������</param>
    void InitTreeView(EFPDataTreeView controlProvider, bool reInit);

    /// <summary>
    /// ���������� � ���������� IReadOnlyObject.
    /// ���������� ��� ������������� � ���������� ���������
    /// </summary>
    void SetReadOnly();
  }

  #endregion

  #region IEFPGridProducerColumn

  /// <summary>
  /// �������� �������, ������������ ���������� �������, ���������� ����������� ���������� ���������
  /// ���������� IEFPGridProducer ����� �� ������������ ���� ���������, ��� ������������ ��� �� ��� ����
  /// ��������, ��� ������������ ���� ������ ��� ���������� ��������
  /// </summary>
  public interface IEFPGridProducerColumn
  {
    /// <summary>
    /// ��������� �������������� ��� ������.
    /// ���������� �� EFPDataGridView.PerformEditData()
    /// ���� ����� �������� ��������, ��������� � ���������������, ������� ������� true. � ���� ������ ���������� ���������� �� �����������
    /// ���� ������� ��������� ������� �������� �� ��������������, � ���������, ������� ������� EFPDataGridView.EditData,
    /// ������� ������� false
    /// </summary>
    /// <param name="rowInfo">���������� � ������� ������ � ��������� ���������</param>
    /// <param name="columnName">��� �������</param>
    /// <returns>true, ���� ��������� ���������</returns>
    bool PerformCellEdit(EFPDataViewRowInfo rowInfo, string columnName);

    /// <summary>
    /// ������ ���������� ��� ������ ���� �� ������
    /// </summary>
    /// <param name="rowInfo">���������� � ������� ������ � ��������� ���������</param>
    /// <param name="columnName">��� �������</param>
    void PerformCellClick(EFPDataViewRowInfo rowInfo, string columnName);
  }

  #endregion

  #region IEFPConfigurableGridProducer

  /// <summary>
  /// ��������� �������������� ���������� ���������� ���������.
  /// � ������� �� IEFPGridProducer, ������������ ����������� ������������ ����������� �������������� �������
  /// �� ������ EFPDataGrifViewConfig
  /// ��������� ������������ �� ������ EFPDataGridViewWithFilters, � �� EFPDataGridView.
  /// </summary>
  public interface IEFPConfigurableGridProducer : IEFPGridProducer
  {
    /// <summary>
    /// ������������� ��������� ��������� �������� ���������� ���������.
    /// ����� ������ �������� ����������� �������� � ����� ��������� � ������� ��������� ����������.
    /// ��������� ��������� �������� � �������� �� �������
    /// </summary>
    /// <param name="parentControl">������ � ���� ��������� ����� ��� ���������� ��������� ���������</param>
    /// <param name="baseProvider">������� ��������� ��������� ��������</param>
    /// <param name="callerControlProvider">��������� �������������� ���������� ���������</param>
    /// <returns>��������� ������� ���������</returns>
    IEFPGridProducerEditor CreateEditor(Control parentControl, EFPBaseProvider baseProvider, IEFPGridControl callerControlProvider);
  }

  #endregion

  /// <summary>
  /// �������� ���� ��������� ����� � �������� �� ��������� ���
  /// ���������� ��������� EFPDataGridView ��� �������������� ��������� EFPDataTreeView. 
  /// ����� �� �������� ����������������. �� ������ ���������� ������ �� ��������� ������ ���������� EFPApp.MainThread
  /// </summary>
  public partial class EFPGridProducer : IEFPConfigurableGridProducer
  {
    #region ���������

    /// <summary>
    /// ��� ������������ �� ��������� ��� ����������� � �������
    /// </summary>
    public const string DefaultConfigDisplayName = "< �� ��������� >";

    #endregion

    #region �����������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public EFPGridProducer()
    {
      _FixedColumns = new SingleScopeList<string>();
    }

    #endregion

    #region �������� ���������

    /// <summary>
    /// ������ ������ ��������, ������� ����� ���� ���������� � ��������� ���������
    /// </summary>
    public EFPGridProducerColumns Columns
    {
      get
      {
        if (_Columns == null)
          _Columns = CreateColumns();
        return _Columns;
      }
    }
    private EFPGridProducerColumns _Columns;

    /// <summary>
    /// ����������� ����� ����� ������� ������ ������������ ������ ��� ������ ��������
    /// </summary>
    /// <returns>���������</returns>
    protected virtual EFPGridProducerColumns CreateColumns() { return new EFPGridProducerColumns(); }

    /// <summary>
    /// ���������� ���������� ��������, ������� ��������� � ��������� Columns
    /// </summary>
    public int ColumnCount
    {
      get
      {
        if (_Columns == null)
          return 0;
        else
          return _Columns.Count;
      }
    }

    /// <summary>
    /// ������ ����� ����������� ���������, ������� ����� ���� ���������� ��� ������ ���������� ���������
    /// </summary>
    public EFPGridProducerToolTips ToolTips
    {
      get
      {
        if (_ToolTips == null)
          _ToolTips = CreateToolTips();
        return _ToolTips;
      }
    }
    private EFPGridProducerToolTips _ToolTips;

    /// <summary>
    /// ����������� ����� ����� ������� ������ ������������ ������ ��� ������ ���������
    /// </summary>
    /// <returns>���������</returns>
    protected virtual EFPGridProducerToolTips CreateToolTips() { return new EFPGridProducerToolTips(); }

    /// <summary>
    /// ���������� ���������� ����������� ���������, ����������� � ��������� ToolTips
    /// </summary>
    public int ToolTipCount
    {
      get
      {
        if (_ToolTips == null)
          return 0;
        else
          return _ToolTips.Count;
      }
    }

    /// <summary>
    /// ������ ��������� �������� ���������� ���������� ���������
    /// </summary>
    public EFPDataViewOrders Orders
    {
      get
      {
        if (_Orders == null)
          _Orders = CreateOrders();
        return _Orders;
      }
    }
    private EFPDataViewOrders _Orders;

    /// <summary>
    /// ����������� ����� ����� ������� ������ ������������ ������ ��� ������ �������� ����������
    /// </summary>
    /// <returns>���������</returns>
    protected virtual EFPDataViewOrders CreateOrders() { return new EFPDataViewOrders(); }

    /// <summary>
    /// ���������� ������� � ������ Orders
    /// </summary>
    public int OrderCount
    {
      get
      {
        if (_Orders == null)
          return 0;
        else
          return _Orders.Count;
      }
    }

    /// <summary>
    /// ������ ���� "������������" �����, ������� ������ ����������� � ������
    /// Columns ��� ������ InitGrid() (��������, "RefId")
    /// �� ��������� - ������ ����
    /// </summary>
    public IList<string> FixedColumns { get { return _FixedColumns; } }
    private SingleScopeList<string> _FixedColumns;

    /// <summary>
    /// ����������� ������ ���� "������������" �����, ������� ������ ����������� � ������, � ���� �������
    /// </summary>
    /// <returns>������ ����</returns>
    public string[] GetFixedColumnArray()
    {
      return _FixedColumns.ToArray();
    }


    #endregion

    #region ������������

    /// <summary>
    /// ������������ ���������� ��������� �� ���������.
    /// ���� ������������ �� ���� ������ � ����� ����, ��� ��������� ������������� ���
    /// ������ ��������� � ��������
    /// </summary>
    public EFPDataGridViewConfig DefaultConfig
    {
      get
      {
        if (_DefaultConfig == null)
          return MakeDefaultConfig();
        else
          return _DefaultConfig;
      }
      set
      {
        _DefaultConfig = value;
      }
    }

    private EFPDataGridViewConfig _DefaultConfig;

    /// <summary>
    /// ����������� ������ ������������
    /// </summary>
    private Dictionary<string, EFPDataGridViewConfig> _NamedConfigs;

    #endregion

    #region ������������� ���������� ���������

    /// <summary>
    /// ������������� ���������� ���������
    /// </summary>
    /// <param name="controlProvider">���������� ���������� ���������</param>
    /// <param name="reInit">��� ������ ������ ���������� ��������� �������� �������� False.
    /// ��� ��������� �������, ����� ��������� �������� ��� ��� ���������������, �������� �������� true</param>
    public void InitGridView(EFPDataGridView controlProvider, bool reInit)
    {
      if (!(controlProvider is EFPConfigurableDataGridView))
        throw new ArgumentException("�������� EFPConfigurableDataGridView", "controlProvider");
      List<string> dummyColumns = new List<string>();
      InitGridView((EFPConfigurableDataGridView)controlProvider, reInit, controlProvider.CurrentConfig, dummyColumns);
    }

    /// <summary>
    /// ������������� ���������� ���������
    /// </summary>
    /// <param name="controlProvider">���������� ���������� ���������</param>
    /// <param name="reInit"></param>
    /// <param name="config">������������ ��� null ��� ������������� ������������ �� ���������</param>
    /// <param name="usedColumns">���� ����������� ����� �����, ������� ������ ���� � ������ ������</param>
    public void InitGridView(EFPConfigurableDataGridView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (reInit)
      {
        if (controlProvider.GridProducer != this)
          throw new InvalidOperationException("��������� ��������� �������������, �� �������� EFPDataGridView.GridProducer �� ����������� ��� ����������� �������");
      }
      if (usedColumns == null)
        throw new ArgumentNullException("usedColumns");
      //usedColumns.CheckNotReadOnly();

#if XXX // ????????????????
      if (Config == null && (!String.IsNullOrEmpty(ControlProvider.CurrentConfigName)))
      {
        try
        {
          Config = ControlProvider.ReadGridConfig(ControlProvider.CurrentConfigName);

          if (Config == null)
            EFPApp.MessageBox("��������� ��������� \"" + ControlProvider.CurrentConfigName + "\" ���� �������. ����������� ��������� �� ���������");
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("������ ��� �������� ��������� \"" + ControlProvider.CurrentConfigName + "\": " +
            e.Message + ". ����� ������������ ��������� �� ���������");
          Config = null;
        }
      }
#endif

      if (config == null)
      {
        if (String.IsNullOrEmpty(controlProvider.DefaultConfigName))
          config = DefaultConfig;
        else
        {
          config = GetNamedConfig(controlProvider.DefaultConfigName);
          if (config == null)
            throw new BugException("�� ������� ������� ������������ \"" + controlProvider.DefaultConfigName +
              "\". ������������ �������� �������� EFPAccDepGrid.DefaultConfigName");
        }
        // TODO: ????? ControlProvider.CurrentConfigName = String.Empty;
      }


      // ������������� Stack overflow
      if (config != controlProvider.CurrentConfig)
      {
        controlProvider.CurrentConfig = config;
        //ControlProvider.GridProducer = this;
      }

      #region ������������� ����

      foreach (string fixedName in FixedColumns)
        usedColumns.Add(fixedName);

      #endregion

      #region ���������� �������� � ��������

      int maxTextRowHeight = 1;
      for (int i = 0; i < config.Columns.Count; i++)
      {
        string columnName = config.Columns[i].ColumnName;
        EFPGridProducerColumn colDef = Columns[columnName];
        if (colDef == null)
          // ��� � � ������ ��������� ��������
          continue;
        DataGridViewColumn gridCol = colDef.CreateColumn();
        colDef.ApplyConfig(gridCol, config.Columns[i], controlProvider);
        controlProvider.Control.Columns.Add(gridCol);
        // ���������� ����, ������� �����
        colDef.GetColumnNames(usedColumns);

        EFPDataGridViewColumn col2 = controlProvider.Columns[gridCol];
        col2.ColumnProducer = colDef;
        col2.SizeGroup = colDef.SizeGroup;
        col2.CanIncSearch = colDef.CanIncSearch;
        col2.MaskProvider = colDef.MaskProvider;
        col2.DbfInfo = colDef.DbfInfo;
        col2.PrintHeaders = colDef.PrintHeaders;
        col2.ColorType = colDef.ColorType;
        col2.Grayed = colDef.Grayed;
        col2.CustomOrderColumnName = colDef.CustomOrderSourceColumnName;

        maxTextRowHeight = Math.Max(maxTextRowHeight, colDef.TextRowHeight);
      }

      if (config.FrozenColumns > 0 && config.FrozenColumns < config.Columns.Count)
        controlProvider.Control.Columns[config.FrozenColumns - 1].Frozen = true;

      controlProvider.TextRowHeight = maxTextRowHeight;

      if (!String.IsNullOrEmpty(config.StartColumnName))
      {
        int startColumnIndex = controlProvider.Columns.IndexOf(config.StartColumnName);
        //else
        //  // ���������� ������ ������� � ���������������
        //  16.05.2018
        //  �� ����. ��������� ������� ������� ���������� � EFPDataGridView
        //  StartColumnIndex = ControlProvider.FirstIncSearchColumnIndex;

        controlProvider.CurrentColumnIndex = startColumnIndex;
        controlProvider.SaveCurrentColumnAllowed = false; // ����� �� ���������������� ������� �� ����������� ������ ������������
      }
      else
        controlProvider.SaveCurrentColumnAllowed = true;

      // 16.07.2021
      // ��� ���������� �������� ���������� ������ �� ����� ������������ ����, ������� ����� ������ ��� ����������� ���������
      SingleScopeList<string> usedColumnsForOrders = new SingleScopeList<string>(usedColumns);

      #endregion

      #region ����������� ���������

      for (int i = 0; i < config.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip item = ToolTips[config.ToolTips[i].ToolTipName];
        if (item == null)
          continue;
        item.GetColumnNames(usedColumns);
      }

      #endregion

      if (!reInit)
        controlProvider.GetCellAttributes += EFPDataGridView_GetCellAttributes;

      #region ��������� ������� ����������

      if (controlProvider.UseGridProducerOrders)
      {
        controlProvider.Orders.Clear();
        if (this.OrderCount > 0)
        {
          for (int i = 0; i < Orders.Count; i++)
          {
            // ��������� ������ ������� ����������, ��� ������� ���������� ������� ��������� ��� ������� ��������� ��������������
            if (Orders[i].AreAllColumnsPresented(usedColumnsForOrders)) // ���� ��� ����������� ���� ?
              controlProvider.Orders.Add(Orders[i]);
          }
        }
      } // ControlProvider.UseGridProducerOrders

      #endregion
    }

    private void EFPDataGridView_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      EFPDataGridView controlProvider = (EFPDataGridView)sender;

      EFPGridProducerColumn colDef = args.Column.ColumnProducer as EFPGridProducerColumn;
      if (colDef == null)
        return;

      colDef.OnGetCellAttributes(args);

      switch (args.Reason)
      {
        case EFPDataGridViewAttributesReason.View:
        case EFPDataGridViewAttributesReason.Print:
          DoGetValue(controlProvider, args, colDef);
          break;
        case EFPDataGridViewAttributesReason.ToolTip:
          try
          {
            DoGetToolTipText(controlProvider, args, colDef);
          }
          catch (Exception e) // 21.05.2021
          {
            args.ToolTipText = "������ ��� ��������� ���������: " + e.Message;
          }
          break;
      }
    }

    private void DoGetValue(EFPDataGridView controlProvider, EFPDataGridViewCellAttributesEventArgs args, EFPGridProducerColumn colDef)
    {
      if (args.Value != null)
        return; // ��� ����������

      DataRow sourceRow = controlProvider.GetDataRow(args.RowIndex);

      try
      {
        EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
        args.Value = colDef.GetValue(rowInfo);
        controlProvider.FreeRowInfo(rowInfo);
      }
      catch
      {
        args.Value = null;
      }
    }

    private void DoGetToolTipText(EFPDataGridView controlProvider, EFPDataGridViewCellAttributesEventArgs args, EFPGridProducerColumn colDef)
    {
#if DEBUG
      if (controlProvider.CurrentConfig == null)
        throw new NullReferenceException("�� ������ �������� EFPDataGridView.CurrentConfig");
#endif

      if (!controlProvider.CurrentConfig.CurrentCellToolTip)
        args.ToolTipText = String.Empty;
      else
      {
        EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
        string s2 = colDef.GetCellToolTipText(rowInfo, args.ColumnName);
        controlProvider.FreeRowInfo(rowInfo);
        if (s2.Length > 0)
          args.ToolTipText = s2;
      }

      if (controlProvider.CurrentConfig.ToolTips.Count == 0)
        return;

      List<string> lst2 = new List<string>();
      for (int i = 0; i < controlProvider.CurrentConfig.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip toolTip = this.ToolTips[controlProvider.CurrentConfig.ToolTips[i].ToolTipName];
        if (toolTip == null)
          continue; // ������ �����-��

        // ���� � ��������� ������ �������, �� ������� �������� ����, �� ���������� ���������
        List<string> lst1 = new List<string>();
        toolTip.GetColumnNames(lst1);
        if (lst1.Contains(args.ColumnName))
          continue;

        string s;
        try
        {
          EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
          s = toolTip.GetToolTipText(rowInfo);
          controlProvider.FreeRowInfo(rowInfo);
        }
        catch (Exception e)
        {
          s = toolTip.DisplayName + ": ������! " + e.Message;
        }
        if (!String.IsNullOrEmpty(s))
          lst2.Add(s);
      }
      if (lst2.Count > 0)
      {
        lst2.Insert(0, new string('-', 32));
        if (!String.IsNullOrEmpty(args.ToolTipText))
          lst2.Insert(0, args.ToolTipText);

        args.ToolTipText = String.Join(Environment.NewLine, lst2.ToArray());
      }
    }

    #endregion

    #region ������������� ������������ ���������

    /// <summary>
    /// ������������� �������������� ���������
    /// </summary>
    /// <param name="controlProvider">���������� ���������� ���������</param>
    /// <param name="reInit">��� ������ ������ ���������� ��������� �������� �������� False.
    /// ��� ��������� �������, ����� ��������� �������� ��� ��� ���������������, �������� �������� true</param>
    public void InitTreeView(EFPDataTreeView controlProvider, bool reInit)
    {
      if (!(controlProvider is EFPConfigurableDataTreeView))
        throw new ArgumentException("�������� EFPConfigurableDataTreeView", "controlProvider");
      List<string> dummyColumns = new List<string>();
      InitTreeView((EFPConfigurableDataTreeView)controlProvider, reInit, controlProvider.CurrentConfig, dummyColumns);
    }

    /// <summary>
    /// ������������� �������������� ���������
    /// </summary>
    /// <param name="controlProvider">���������� ���������</param>
    /// <param name="reInit"></param>
    /// <param name="config">������������ ��� null ��� ������������� ������������ �� ���������</param>
    /// <param name="usedColumns">���� ����������� ����� �����, ������� ������ ���� � ������ ������</param>
    public void InitTreeView(EFPConfigurableDataTreeView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (reInit)
      {
        if (controlProvider.GridProducer != this)
          throw new InvalidOperationException("��������� ��������� �������������, �� �������� EFPDataGridView.GridProducer �� ����������� ��� ����������� �������");
      }
      if (usedColumns == null)
        throw new ArgumentNullException("usedColumns");
      //usedColumns.CheckNotReadOnly();

#if XXX //???????????????
      if (config == null && (!String.IsNullOrEmpty(controlProvider.CurrentConfigName)))
      {
        try
        {
          config = controlProvider.ReadGridConfig(controlProvider.CurrentConfigName);

          if (config == null)
            EFPApp.MessageBox("��������� ��������� \"" + controlProvider.CurrentConfigName + "\" ���� �������. ����������� ��������� �� ���������");
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("������ ��� �������� ��������� \"" + controlProvider.CurrentConfigName + "\": " +
            e.Message + ". ����� ������������ ��������� �� ���������");
          config = null;
        }
      }
#endif

      if (config == null)
      {
        if (String.IsNullOrEmpty(controlProvider.DefaultConfigName))
          config = this.DefaultConfig;
        else
        {
          config = this.GetNamedConfig(controlProvider.DefaultConfigName);
          if (config == null)
            throw new BugException("�� ������� ������� ������������ \"" + controlProvider.DefaultConfigName +
              "\". ������������ �������� �������� EFPAccDepGrid.DefaultConfigName");
        }
        // TODO: ????? controlProvider.CurrentConfigName = String.Empty;
      }


      // ������������� Stack overflow
      if (config != controlProvider.CurrentConfig)
      {
        controlProvider.CurrentConfig = config;
        controlProvider.GridProducer = this;
      }

      controlProvider.Control.UseColumns = true;

      foreach (string fixedName in FixedColumns)
        usedColumns.Add(fixedName);

      int maxTextRowHeight = 1;
      for (int i = 0; i < config.Columns.Count; i++)
      {
        string columnName = config.Columns[i].ColumnName;
        EFPGridProducerColumn colDef = this.Columns[columnName];
        if (colDef == null)
          // ��� � � ������ ��������� ��������
          continue;
        // ������� ������ TreeColumn
        TreeColumn tc = colDef.CreateTreeColumn(config.Columns[i]);
        controlProvider.Control.Columns.Add(tc);

        // ������� ������ NodeControl
        BindableControl bc = colDef.CreateNodeControl();
        colDef.ApplyConfig(bc, config.Columns[i], controlProvider);
        bc.VirtualMode = true;
        bc.DataPropertyName = colDef.Name;
        bc.ParentColumn = controlProvider.Control.Columns[controlProvider.Control.Columns.Count - 1];
        controlProvider.Control.NodeControls.Add(bc);



        // ���������� ����, ������� �����
        colDef.GetColumnNames(usedColumns);
        /*
        EFPDataGridViewColumn Col2 = ControlProvider.Columns[Col];
        Col2.ColumnProducer = ColDef;
        Col2.SizeGroup = ColDef.SizeGroup;
        Col2.CanIncSearch = ColDef.CanIncSearch;
        Col2.MaskProvider = ColDef.MaskProvider;
        Col2.DbfInfo = ColDef.DbfInfo;
        Col2.PrintHeaders = ColDef.PrintHeaders;
                                                                            */
        maxTextRowHeight = Math.Max(maxTextRowHeight, colDef.TextRowHeight);
      }

      //if (Config.FrozenColumns > 0 && Config.FrozenColumns < Config.Columns.Count)
      //  ControlProvider.Control.Columns[Config.FrozenColumns - 1].Frozen = true;

      //ControlProvider.TextRowHeight = MaxTextRowHeight;
      /*
      int StartColumnIndex;
      if (String.IsNullOrEmpty(Config.StartColumnName))
        // ���������� ������ ������� � ���������������
        StartColumnIndex = ControlProvider.FirstIncSearchColumnIndex;
      else
        StartColumnIndex = ControlProvider.Columns.IndexOf(Config.StartColumnName);
      if (StartColumnIndex < 0)
        StartColumnIndex = 0;
      ControlProvider.CurrentColumnIndex = StartColumnIndex;
       */

      /*
      for (int i = 0; i < Config.ToolTips.Count; i++)
      {
        GridProducerToolTip Item = ToolTips[Config.ToolTips[i].ToolTipName];
        if (Item == null)
          continue;
        Item.GetColumnNames(UsedColumns);
      }
       */

      // ��� ����������� ����������� ��������� � ��������� �������� ����� ��������� 
      // ������, ������� ����� ������� ������ ��������� �����
      // ��������� � ��������� ����������
      if (!reInit)
      {              /*
        GridProducerHandler tth = new GridProducerHandler(this, ControlProvider);

        ControlProvider.Control.CellValueNeeded += new DataGridViewCellValueEventHandler(tth.CellValueNeeded);
        ControlProvider.Control.VirtualMode = true;*/
      }

      // ���������� ��������� ������ ������� ����������
      string orgOrderDisplayName = String.Empty;
      if (reInit && controlProvider.CurrentOrder != null)
        orgOrderDisplayName = controlProvider.CurrentOrder.DisplayName;

      if (controlProvider.OrderCount > 0)
        controlProvider.Orders.Clear();
      else
        controlProvider.DisableOrdering(); // ������ ������� �� ����������
#if XXX
      int NewOrderIndex = -1;
      if (Orders.Count > 0)
      {
        for (int i = 0; i < Orders.Count; i++)
        {
          // ��������� ������ ������� ����������, ��� ������� ���������� ����
          // � ������� Fields
          if (UsedColumns.Contains(Orders[i].RequiredColumns)) // ���� ��� ����������� ���� ?
          {
            ControlProvider.Orders.Add(Orders[i].ColumnNames, Orders[i].DisplayName, Orders[i].SortInfo);
            if (Orders[i].DisplayName == OrgOrderDisplayName)
              NewOrderIndex = ControlProvider.Orders.Count - 1;
          }
        }
        ControlProvider.AutoSort = ControlProvider.Orders.Count > 0;
        if (ReInit)
        {
          if (NewOrderIndex >= 0)
            ControlProvider.CurrentOrderIndex = NewOrderIndex;
          else
          {
            if (ControlProvider.OrderCount > 0)
              ControlProvider.CurrentOrderIndex = 0;
          }
        }
      }
      if (ReInit)
        ControlProvider.CommandItems.RefreshOrderItems();
#endif
    }

    #endregion

    #region ������ � ����������� ���������

    /// <summary>
    /// ������� ����� ������ ��� DefaultConfig � ����������� ��������� ��� �����
    /// ���������� ��������� � ToolTip'���
    /// </summary>
    /// <param name="addAll">���� true, �� ����� ������� ������ GridConfig.Columns.Add() �
    /// ToolTips.Add() ��� ��������� �� ������� ������ ����������</param>
    public void NewDefaultConfig(bool addAll)
    {
      DefaultConfig = new EFPDataGridViewConfig();
      if (addAll)
        DefaultConfig = CreateDefaultConfig();
      else
        DefaultConfig = new EFPDataGridViewConfig();
    }


    /// <summary>
    /// ������� ������ ������������ � ������������� � ��� ������� ��� ���� �������� �
    /// ����������� ���������.
    /// </summary>
    /// <returns>����� ������������</returns>
    public EFPDataGridViewConfig CreateDefaultConfig()
    {
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      for (int i = 0; i < Columns.Count; i++)
        config.Columns.Add(Columns[i].Name);
      for (int i = 0; i < ToolTips.Count; i++)
        config.ToolTips.Add(ToolTips[i].Name);
      return config;
    }

    /// <summary>
    /// ������� ��������� � ������������� ������
    /// </summary>
    /// <param name="fixedName">��� ���������</param>
    /// <returns>������ ������������, ������� ����� ���������</returns>
    public EFPDataGridViewConfig NewNamedConfig(string fixedName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fixedName))
        throw new ArgumentNullException(fixedName);
#endif

      if (_NamedConfigs == null)
        _NamedConfigs = new Dictionary<string, EFPDataGridViewConfig>();
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      _NamedConfigs.Add(fixedName, config);
      return config;
    }

    /// <summary>
    /// �������� ��������� � ������������� ������.
    /// ���� ��������� �� ���� �������, ������������ ����������
    /// </summary>
    /// <param name="fixedName">��� ���������</param>
    /// <returns></returns>
    public EFPDataGridViewConfig GetNamedConfig(string fixedName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fixedName))
        throw new ArgumentNullException(fixedName);
#endif
      if (_NamedConfigs == null)
        return null;

      EFPDataGridViewConfig config;
      if (_NamedConfigs.TryGetValue(fixedName, out config))
        return config;
      else
        throw new ArgumentException("������������� ��������� ���������� ��������� � ������ \"" + fixedName +
          "\" �� ���� ��������� � ���������� ���������� ���������", "fixedName");
    }

    /// <summary>
    /// ���������� ����� ��������, ����������� � ������� NewNamedConfig()
    /// </summary>
    /// <returns></returns>
    public string[] GetNamedConfigNames()
    {
      if (_NamedConfigs == null)
        return DataTools.EmptyStrings;
      else
      {
        string[] a = new string[_NamedConfigs.Count];
        _NamedConfigs.Keys.CopyTo(a, 0);
        return a;
      }
    }

    /// <summary>
    /// ��������� ����������� ����� ������������ � �������� ������. ���� ��� �� ������,
    /// �� ������������ DefaultConfig. ���� ��� ������������, ��� ��������� ���� �������,
    /// �� ������������ null
    /// </summary>
    /// <param name="configSectionName">��� ������ ������������, ������������ ��������� ����������</param>
    /// <param name="defaulConfigName">��� ������������� ��������� ��� ������ ������, ���� ������������ ��������� �� ���������</param>
    /// <param name="cfgName">��� ����������� ������</param>
    /// <returns></returns>
    public EFPDataGridViewConfig LoadConfig(string configSectionName, string defaulConfigName, string cfgName)
    {
      if (String.IsNullOrEmpty(cfgName))
      {
        if (String.IsNullOrEmpty(defaulConfigName))
          return DefaultConfig;
        else
          return GetNamedConfig(defaulConfigName);
      }
      else
      {
        throw new NotImplementedException();
        //TODO: return GridHandlerConfigs.GetConfig(ConfigSectionName, CfgName);
      }
    }

    /// <summary>
    /// ��������� ����������� ����� ������������, ��������� ��� ������� ������������. 
    /// ���� ��� �� ���� ��������� ��� ������������, ��� ��������� ���� �������,
    /// �� ������������ DefaultConfig
    /// </summary>
    /// <param name="configSectionName">��� ������ ������������, ������������ ��������� ����������</param>
    /// <param name="defaultConfigName">��� ������������� ��������� ��� ������ ������, ���� ������������ ��������� �� ���������</param>
    /// <returns>����������� ������ ��� DefaultConfig</returns>
    public EFPDataGridViewConfig LoadConfig(string configSectionName, string defaultConfigName)
    {
      string cfgName = GetCurrentConfigName(configSectionName);
      EFPDataGridViewConfig config = LoadConfig(configSectionName, defaultConfigName, cfgName);
      if (config == null)
        config = DefaultConfig;
      return config;
    }

    /// <summary>
    /// ���������� ��� ������� ������������, ������� ������ �������������� ����������
    /// </summary>
    /// <param name="configSectionName"></param>
    /// <returns></returns>
    public static string GetCurrentConfigName(string configSectionName)
    {
      return String.Empty;
      // TODO:
      /*
      if (String.IsNullOrEmpty(ConfigSectionName))
        throw new ArgumentNullException("ConfigSectionName");
      ConfigSection Sect = AccDepClientExec.ConfigSections[ConfigSectionName, "��������"];
      return Sect.GetString("���������");
       * */
    }

    /// <summary>
    /// �������� ������ �����, ����������� ��� �������� ������������
    /// </summary>
    /// <param name="config">������������. ���� null, �� ������������ FixedColumns</param>
    /// <param name="usedColumns">���� ����������� ����� �����</param>
    public void GetColumnNames(EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (usedColumns == null)
        throw new ArgumentNullException();
      //usedColumns.CheckNotReadOnly();

      foreach (string fixedName in FixedColumns)
        usedColumns.Add(fixedName);
      if (config != null)
      {
        // �������
        for (int i = 0; i < config.Columns.Count; i++)
        {
          string columnName = config.Columns[i].ColumnName;
          EFPGridProducerColumn colDef = Columns[columnName];
          if (colDef == null)
            // ��� � � ������ ��������� ��������
            continue;
          // ���������� ����, ������� �����
          colDef.GetColumnNames(usedColumns);
        }

        // ����������� ���������
        for (int i = 0; i < config.ToolTips.Count; i++)
        {
          EFPGridProducerToolTip item = ToolTips[config.ToolTips[i].ToolTipName];
          if (item == null)
            continue;
          item.GetColumnNames(usedColumns);
        }
      }
    }

    /// <summary>
    /// �������� ������ �����, ����������� ��� ���������� ���������, ��� �������������
    /// ������������, ����������� �������������
    /// </summary>
    /// <param name="configSectioName">��� ������ ������������ ��� ���������� ���������</param>
    /// <param name="defaultConfigName">��� ������������� ��������� ��� ������ ������, ���� ������������ ��������� �� ���������</param>
    /// <param name="usedColumns">���� ����������� ����� �����</param>
    public void GetColumnNames(string configSectioName, string defaultConfigName, IList<string> usedColumns)
    {
      GetColumnNames(LoadConfig(configSectioName, defaultConfigName), usedColumns);
    }

    private EFPDataGridViewConfig MakeDefaultConfig()
    {
      EFPDataGridViewConfig res = new EFPDataGridViewConfig();
      for (int i = 0; i < Columns.Count; i++)
      {
        res.Columns.Add(new EFPDataGridViewConfigColumn(Columns[i].Name));
        // ���� ��������� ������ ���� ������� - ������ ��� � �����������
        if (Columns.Count == 1)
          res.Columns[0].FillMode = true;
      }

      for (int i = 0; i < ToolTips.Count; i++)
        res.ToolTips.Add(ToolTips[i].Name);

      res.SetReadOnly();
      return res;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������������� ��������� ��������� �������� ���������� ���������.
    /// ����� ������ �������� ����������� �������� � ����� ��������� � ������� ��������� ����������.
    /// ��������� ��������� �������� � �������� �� �������
    /// </summary>
    /// <param name="parentControl">������ � ���� ��������� ����� ��� ���������� ��������� ���������</param>
    /// <param name="baseProvider">������� ��������� ��������� ��������</param>
    /// <param name="callerControlProvider">��������� �������������� ���������� ���������</param>
    /// <returns>��������� ������� ���������</returns>
    public IEFPGridProducerEditor CreateEditor(Control parentControl, EFPBaseProvider baseProvider, IEFPGridControl callerControlProvider)
    {
      EFPGridProducerEditor form = new EFPGridProducerEditor(this, callerControlProvider, baseProvider);
      parentControl.Controls.Add(form.TheTabControl);
      return form;
    }

    #endregion

    #region IReadOnlyObject

    /// <summary>
    /// ���������� true, ���� GridProducer ��� ��������� � ����� "������ ������".
    /// ����������� ��� ������ ������������� � ���������� ��������� ��� �� ��������� ������������� ������� DBUI � ExtDBDocForms.dll
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        if (_Columns == null)
          return false;
        else
          return _Columns.IsReadOnly;
      }
    }

    /// <summary>
    /// ���������� ����������, ���� GridProducer ��� ��������� � ����� "������ ������".
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ������� GridProducer � ����� "������ ������".
    /// ��� ������ ������ ����������� �������� ������������ ������.
    /// ��������� ������ ������������
    /// </summary>
    public void SetReadOnly()
    {
      if (IsReadOnly)
        return;

      Columns.SetReadOnly();
      ToolTips.SetReadOnly();
      Orders.SetReadOnly();

      // ��������� ���������� �������

      Validate();
    }

    #endregion

    #region �������� ������������ ������

    /// <summary>
    /// �������� ������������ ������.
    /// ���������� ���������� ��� ������ ������ SetReadOnly()
    /// </summary>
    protected virtual void Validate()
    {
      // ������ ������ ���� �����, ������� ������ ���� � ���� ������
      SingleScopeStringList srcColumnNames = new SingleScopeStringList(true); // ��� ����� ��������
      foreach (EFPGridProducerColumn col in Columns)
      {
        ValidateItemBase(col);
        col.GetColumnNames(srcColumnNames);
      }
      foreach (EFPGridProducerToolTip tt in ToolTips)
      {
        ValidateItemBase(tt);
        tt.GetColumnNames(srcColumnNames);
      }

      SingleScopeStringList calcColumnNames = new SingleScopeStringList(true); // ��� �������� �������� ����������

      // ���������, ��� ��� �� ��������� � ������� ����������� �����
      foreach (EFPGridProducerColumn col in Columns)
      {
        if (col.SourceColumnNames != null)
        {
          if (srcColumnNames.Contains(col.Name))
            throw new EFPGridProducerValidationException("������������ ��� ������������ ������� \"" + col.Name + "\", ��� ��� ��� ��� ���� � ������ �������� �������� � ������ �������� EFPGridProducer");
          calcColumnNames.Add(col.Name);
        }
      }
      foreach (EFPGridProducerToolTip tt in ToolTips)
      {
        if (tt.SourceColumnNames != null)
        {
          if (srcColumnNames.Contains(tt.Name))
            throw new EFPGridProducerValidationException("������������ ��� ����������� ����������� ��������� \"" + tt.Name + "\", ��� ��� ��� ��� ���� � ������ �������� �������� � ������ �������� EFPGridProducer");

          // ����� ����������� ��������� �� ��������� ��� ������� ����������
        }
      }

      List<string> orderColumnNames = new List<string>();
      foreach (EFPDataViewOrder order in Orders)
      {
        orderColumnNames.Clear();
        order.GetColumnNames(orderColumnNames);
        for (int i = 0; i < orderColumnNames.Count; i++)
        {
          if (!calcColumnNames.Contains(orderColumnNames[i])) // ���������� �� ������������ �������
          {
            // ���������� �� ��������� ������� �� ���� ������

            string errorText;
            if (!IsValidSourceColumnName(orderColumnNames[i], out errorText))
              throw new EFPGridProducerValidationException("������������ ��� ��������� ������� \"" + calcColumnNames + "\", ������������� � ������� ���������� \"" + order.Name + "\". " + errorText);
          }
        }
      }
    }

    private void ValidateItemBase(EFPGridProducerItemBase item)
    {
      string errorText;
      if (item.SourceColumnNames == null)
      {
        if (!IsValidSourceColumnName(item.Name, out errorText))
          throw new EFPGridProducerValidationException("������������ ��� ������� \"" + item.Name + "\". " + errorText);
      }
      else
      {
        for (int i = 0; i < item.SourceColumnNames.Length; i++)
        {
          if (!IsValidSourceColumnName(item.SourceColumnNames[i], out errorText))
            throw new EFPGridProducerValidationException("������������ ��� ��������� ������� \"" + item.SourceColumnNames[i] + "\" � ����������� �������/��������� \"" + item.Name + "\". " + errorText);
        }
      }
    }

    /// <summary>
    /// �������� ������������ ����� ��������� �������.
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="errorText">���� ������ ���� �������� ��������� �� ������</param>
    /// <returns>true, ���� ��� �������� ����������</returns>
    public virtual bool IsValidSourceColumnName(string columnName, out string errorText)
    {
      if (String.IsNullOrEmpty(columnName))
      {
        errorText = "��� �� ������";
        return false;
      }

      if (columnName.IndexOf(',') >= 0)
      {
        errorText = "��� �� ����� ��������� �������";
        return false;
      }

      if (columnName.IndexOf(' ') >= 0)
      {
        errorText = "��� �� ����� ��������� �������";
        return false;
      }

      if (columnName[0] == '.' || columnName[columnName.Length - 1] == '.')
      {
        errorText = "��� �� ����� ���������� ��� �������������� ������";
        return false;
      }

      if (columnName.IndexOf("..") >= 0)
      {
        errorText = "��� �� ����� ��������� 2 ����� ������";
        return false;
      }

      errorText = null;
      return true;
    }


    #endregion
  }

  /// <summary>
  /// ����������, ����������� ��� �������� ������� EFPGridProducer � ����� "������ ������", � �������� �������� ������������ ������
  /// </summary>
  [Serializable]
  public class EFPGridProducerValidationException : ApplicationException
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������� � �������� ����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    public EFPGridProducerValidationException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// ������� ������ ���������� � �������� ���������� � ��������� �����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    /// <param name="innerException">��������� ����������</param>
    public EFPGridProducerValidationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected EFPGridProducerValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
