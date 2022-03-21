// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.ComponentModel;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// ����������� ���������� ���������� ���������, ����������� �� ������ ������
  /// ������������ ������� ����� � ������� ��������� DBxOrder, � ��� �����, ���������� �������.
  /// ��� ����� � ����� ������ ����������� ����������� ����������� �������.
  /// </summary>
  public class EFPDBxViewOrder : EFPDataViewOrder
  {
    #region �����������

    /// <summary>
    /// ������� ������ ����������
    /// </summary>
    /// <param name="name">�������� ��� ��� ������� ����������</param>
    /// <param name="order">������� ���������� ��� ORDER BY</param>
    public EFPDBxViewOrder(string name, DBxOrder order)
      :base(name, String.Empty)
    {
      if (order == null)
        throw new ArgumentNullException("order");
      _Order = order;

      string ColumnName;
      ListSortDirection SortOrder;
      _Order.GetFirstColumnInfo(out ColumnName, out SortOrder);
      SortInfo = new EFPDataGridViewSortInfo(ColumnName, SortOrder);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ���������� ��� SQL-������� SELECT .. ORDER BY
    /// </summary>
    public DBxOrder Order { get { return _Order; } }
    private DBxOrder _Order;

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName + " (" + Order.ToString() + ")";
    }

    #endregion

    #region ���������� ����������

    /// <summary>
    /// ���������� ���������� INamedValuesAccess.
    /// ���������� �������� �� ��������� ��� ���� �������� ������ �������
    /// </summary>
    private class DefValAccess : INamedValuesAccess
    {
      #region �����������

      public DefValAccess(DataTable table)
      {
        _Table = table;
      }

      private DataTable _Table;

      #endregion

      #region INamedValuesAccess Members

      public object GetValue(string name)
      {
        int p = _Table.Columns.IndexOf(name);
        if (p < 0)
          throw new ArgumentException("����������� ������� \"" + name + "\"");

        return DataTools.GetEmptyValue(_Table.Columns[p].DataType);
      }

      public bool Contains(string name)
      {
        return _Table.Columns.Contains(name);
      }

      public string[] GetNames()
      {
        string[] names = new string[_Table.Columns.Count];
        for (int i = 0; i < names.Length; i++)
          names[i] = _Table.Columns[i].ColumnName;
        return names;
      }

      #endregion
    }

    /// <summary>
    /// ��������� ���������� � ���������� ���������.
    /// ��������� ������ ����� ���, ����������� �� EFPDBxGridView.
    /// ���������� ������ ������ ���� ������ DataView (�������� EFPDataGridView.SourceAsDataView ������ ���������� �������� ��������).
    /// ������������� �������� DataView.Sort.
    /// ���� ����� ����������� ���������� ���� �������, �������� �� DBxOrderColumn,
    /// �� � ������� ����������� ����������� ������� "$$Sort_XXX", ��� ������� ��������������� �������� DataColumn.Expression.
    /// </summary>     
    /// <param name="controlProvider">��������� ���������� ���������, ��� �������� ����� ���������� ������� ����������.</param>
    public override void PerformSort(IEFPDataView controlProvider)
    {
      //if (!(controlProvider is EFPDBxGridView))
      //  throw new InvalidOperationException("������������ ��� ControlProvider: " + controlProvider.GetType().ToString());

      DataView dv = controlProvider.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException("���������� ������ ��������� �� �������� DataView");

      // ������, ������� ���������� �������� ����������� ������ � ����������� �������
      // �� �����. ��� ������� ���������, ��������, DBxOrderColumnIfNull, ��� ������
      // �������� � DataView.Sort. ������ ����� ��������� ����������� �������,
      // ������� �������� ������������� �������� Expression. �� ���� �����������
      // ����������.
      // � ����� DataOrder ����� ���� ��������� ���������, ����� �������, �, �����,
      // ������� ����. ����������� ������� ������������ ������ ��� �����������
      // DataOrderItem, � ��� ������� ����� �� ������������

      DBxSqlBuffer buf = new DBxSqlBuffer();
      buf.Clear();
      for (int i = 0; i < Order.Parts.Length; i++)
      {
        if (i > 0)
          buf.SB.Append(',');
        if ((Order.Parts[i].Expression) is DBxColumn)
          // ������� ����
          buf.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
        else
        {
          // ����������� ����
          DBxSqlBuffer buf2 = new DBxSqlBuffer();
          buf2.Clear();
          buf2.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
          string Expr = buf2.SB.ToString();
          // ��� �������
          string ExprColName = "$$Sort_" + DataTools.MD5SumFromString(Expr);

          // 16.10.2019
          // ��� ������ ��� ������� ��������� �� �������
          DefValAccess dva = new DefValAccess(dv.Table);
          object defVal = Order.Parts[i].Expression.GetValue(dva, false);
          if (defVal == null)
            throw new NullReferenceException("��� ��������� \"" + Expr + "\" ������� ���������� \"" + DisplayName + "\" �� ������� ��������� �������� �� ���������, ����� ���������� ��� ������");
          Type dataType = defVal.GetType();


          // ������� ����������� ������ ��� �������������, ����� ��������� �����������
          // �������� ��� ������ ������������ ����������
          if (!dv.Table.Columns.Contains(ExprColName))
          {
            DataColumn Col = new DataColumn(ExprColName, dataType, Expr);
            dv.Table.Columns.Add(Col);
          }

          // � Sort ����������� ��� ������������ �������
          buf.SB.Append(ExprColName);
        }

        // ������� �������� ����������
        if (Order.Parts[i].SortOrder == ListSortDirection.Descending)
          buf.SB.Append(" DESC");
      }
      dv.Sort = buf.SB.ToString();
    }
    #endregion
  }

  /// <summary>
  /// ���������� �������� EFPDBxGridView.Orders.
  /// ����� ������������ ��� EFPDBxGridProducer.Orders.
  /// </summary>
  public class EFPDBxViewOrders : EFPDataViewOrders
  {
    #region ������ ����������

    /// <summary>
    /// ������� ������� ���������� EFPDBxGridViewOrder � ��������� ��� � ������
    /// </summary>
    /// <param name="order">������� ���������� ��� SELECT .. ORDER BY</param>
    /// <param name="displayName">������������ ��� ��� ����</param>
    /// <param name="sortInfo">������� ���������� ��������� ��� ������� �����</param>
    /// <returns>��������� ������� ����������</returns>
    public EFPDBxViewOrder Add(DBxOrder order, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
#if DEBUG
      if (order == null)
        throw new ArgumentNullException("order");
#endif     
      string name = order.ToString();
      EFPDBxViewOrder item = new EFPDBxViewOrder(order.ToString(), order);
      item.DisplayName = displayName;
      if (!sortInfo.IsEmpty)
        item.SortInfo = sortInfo;
      base.Add(item);
      //// ���� ������ ���� � ������� ���������� ������������ � ���������, �� �������
      //// ����� ������� �� ���������
      //EFPDataGridViewColumn Column = GetUsedColumn(Item); // 19.06.2019
      //if (Column != null)
      //  Column.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
      return item;
    }

    /// <summary>
    /// ������� ������� ���������� EFPDBxGridViewOrder � ��������� ��� � ������.
    /// ������� ���������� ��������� ��� ������� ����� ������������ �������������.
    /// </summary>
    /// <param name="order">������� ���������� ��� SELECT .. ORDER BY</param>
    /// <param name="displayName">������������ ��� ��� ����</param>
    /// <returns>��������� ������� ����������</returns>
    public EFPDBxViewOrder Add(DBxOrder order, string displayName)
    {
      return Add(order, displayName, EFPDataGridViewSortInfo.Empty);
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// �������� ������ ������� ����������
  /// </summary>
  public class EFPGridProducerOrder
  {
    #region ������������

    /// <summary>
    /// �������� ����������� �� ������� DBxOrder. �������������� ��� ������� ���� ��� ����������, ��� � ���������
    /// </summary>
    /// <param name="dataOrder">������ ���������� ��� SQL-�������</param>
    /// <param name="displayName">�������� ������� ���������� ��� ������ �� ����</param>
    public EFPGridProducerOrder(DBxOrder dataOrder, string displayName)
    {
#if DEBUG
      if (dataOrder == null)
        throw new ArgumentNullException("dataOrder");
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
#endif


#if XXX
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < DataOrder.Infos.Length; i++)
      {
        if (i > 0)
          sb.Append(',');

        if (DataOrder.Infos[i].Descend)
          sb.Append('!');

        if (DataOrder.Infos[i].Item is DBxOrderColumn)
          sb.Append(((DBxOrderColumn)(DataOrder.Infos[i].Item)).ColumnName);
        else
          throw new InvalidOperationException("������� ���������� " + DataOrder.ToString() + " � ������� " + i.ToString() + " �������� �������, �� ���������� ������� �����. �������������� � GridProducerOrder ����������");
      }

      FColumnNames = sb.ToString();
#endif
      _DataOrder = dataOrder;


      _DisplayName = displayName;

      _ConfigName = dataOrder.ToString();
    }

    /// <summary>
    /// ����������� �� ������ ����� � ��������.
    /// ����� �������� �������� ������� ����������, �� ������ ������������ ���������
    /// </summary>
    /// <param name="columnNames">������ ���� �������� ��� ����������, ����������� ��������. ���� �� ������ ����������� � ������ "[]". 
    /// ��� �������� ������� ���������� �� �������� �������� ������ "!" ����� ������ ����</param>
    /// <param name="displayName">�������� ������� ���������� ��� ������ �� ����</param>
    public EFPGridProducerOrder(string columnNames, string displayName)
      : this(DBxOrder.FromColumnNames(columnNames), displayName)
    {
    }

    /// <summary>
    /// ����������� �� ������� DBxColumns.
    /// ������ ������ �������� ������� ����������
    /// </summary>
    /// <param name="columnNames">������ ���� ����� ��� ����������</param>
    /// <param name="displayName">�������� ������� ���������� ��� ������ �� ����</param>
    public EFPGridProducerOrder(DBxColumns columnNames, string displayName)
      : this(columnNames.AsString, displayName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ������� ���������� ��� ������ DBxOrder.
    /// �������� �������� � ������������
    /// </summary>
    public DBxOrder DataOrder { get { return _DataOrder; } }
    private DBxOrder _DataOrder;

    /// <summary>
    /// ������������ ��� ��� ������ ������� ���������� ����� ����
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// ������� ���������� ��� ���������� ���������. 
    /// ���� SortInfo.IsEmpty=true (�������� �� ���������), �� ���������� ����������� ��� ������� ������� �
    /// ������ ColumnNames
    /// </summary>
    public EFPDataGridViewSortInfo SortInfo { get { return _SortInfo; } set { _SortInfo = value; } }
    private EFPDataGridViewSortInfo _SortInfo;

    /// <summary>
    /// ������ ����������� ��������, ������� ��� ������ �������������� � ���������, ����� ������ ������� ���������� ��� ��������
    /// �� ��������� ������������ ������ �����, �� ������� ����������� ���������� (�������� Columns)
    /// ��������� ������������ ������ �������� ����� �������������, ���� � ������ ������ ������������ ����������� ����, ���� ��
    /// ������� ������������ ��� ����������� (������������ � GridProducerColumn), � ������ �������� �������� ��� ���������� (� �� ������������)
    /// </summary>
    public DBxColumns RequiredColumns
    {
      get
      {
        if (_RequiredColumns == null)
          return Columns;
        else
          return _RequiredColumns;
      }
      set
      {
        _RequiredColumns = value;
      }
    }
    private DBxColumns _RequiredColumns;

    /// <summary>
    /// ���������� ������ �������� � ���� ������� DBxColumns
    /// ���������� � ������� ���������� (�� ����������� ��� ��������) ��������
    /// </summary>
    public DBxColumns Columns
    {
      get
      {
        if (_Columns == null)
        {
          DBxColumnList lst = new DBxColumnList();
          _DataOrder.GetColumnNames(lst);
          _Columns = new DBxColumns(lst);
        }
        return _Columns;
      }
    }
    private DBxColumns _Columns;

    /// <summary>
    /// ������� ���������� �� ���������
    /// ������������, ����� ������� ���������� �� ����� � ����� ���� (SortInfo.IsEmpty=true)
    /// </summary>
    public EFPDataGridViewSortInfo DefaultSortInfo
    {
      get
      {
        DBxColumnList lst = new DBxColumnList();
        _DataOrder.Parts[0].Expression.GetColumnNames(lst);
        if (lst.Count == 0)
          throw new InvalidOperationException("������� ���������� " + _DataOrder.Parts[0].Expression.ToString() + " �� ������ �� ������ ����� ����");

        return new EFPDataGridViewSortInfo(lst[0], _DataOrder.Parts[0].SortOrder==ListSortDirection.Desc);
      }
    }

    /// <summary>
    /// ����� ��� ���������� ���������� ������� � ���������� ������������
    /// </summary>
    public string ConfigName { get { return _ConfigName; } }
    private string _ConfigName;

    #endregion

    #region ������

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName + " (" + ConfigName + ")";
    }

    /// <summary>
    /// ���������� �������� SortInfo �� �������, �������� �������� ColumnIndex
    /// (��������� �������� ���������� � ����)
    /// </summary>
    /// <param name="columnIndex"></param>
    public void SetSortInfo(int columnIndex)
    {
#if DEBUG
      if (columnIndex < 0 || columnIndex >= DataOrder.Parts.Length)
        throw new ArgumentOutOfRangeException("columnIndex", columnIndex, "������ ����� ������� ��� ���������");
#endif
      DBxOrderPart Item = DataOrder.Parts[columnIndex];

      DBxColumnList lst = new DBxColumnList();
      Item.Expression.GetColumnNames(lst);
      if (lst.Count > 0)
        SortInfo = new EFPDataGridViewSortInfo(lst[0], Item.SortOrder==ListSortDirection.Desc);
      else
        throw new InvalidOperationException("������� ���������� " + Item.Expression.ToString() + " �� ������ �� ������ ����� ����");
    }

    #endregion
  }

  /// <summary>
  /// ��������� �������� ���������� ��� �������� Gridproducer.Orders
  /// </summary>
  public class EFPGridProducerOrders : List<EFPGridProducerOrder>
  {
    #region ������

    /// <summary>
    /// �������� ������� ����������.
    /// </summary>
    /// <param name="columnNames">����� ��������, ����������� ��������. ��� �������� ���������� �� ��������, ����� ���� ������ ������������ "!"</param>
    /// <param name="displayName">������������ ���. ������������ ��� ���������� ���������� ���� ���������</param>
    /// <param name="sortInfo">�������� ������� ������ ������� ���������� � ���������: �� ������ ������� ������� � ������ ������� �������� �����������</param>
    /// <returns>������ GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
      EFPGridProducerOrder Item = new EFPGridProducerOrder(columnNames, displayName);
      Item.SortInfo = sortInfo;
      base.Add(Item);
      return Item;
    }

    /// <summary>
    /// �������� ������� ����������.
    /// ��� ������ ������������� ����������, �� ������ ������� ����� ������ ����� � ���� �������� �����������,
    /// ������ �� ������� ������� � ������<paramref name="columnNames"/>. ���� � ��������� �� ������������ ������
    /// ������� �� ������ ����������, ������� ���������� ����� ����� �������� ������ �� ���������� ����
    /// </summary>
    /// <param name="columnNames">����� ��������, ����������� ��������. ��� �������� ���������� �� ��������, ����� ���� ������ ������������ "!"</param>
    /// <param name="displayName">������������ ���. ������������ ��� ���������� ���������� ���� ���������</param>
    /// <returns>������ GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames, string displayName)
    {
      EFPGridProducerOrder Item = new EFPGridProducerOrder(columnNames, displayName);
      base.Add(Item);
      return Item;
    }

    /// <summary>
    /// �������� ������� ����������.
    /// ��� ������ ������ ������������ ���, ������ ������ �������� � ����������� �����.
    /// ��� ������ ������������� ����������, �� ������ ������� ����� ������ ����� � ���� �������� �����������,
    /// ������ �� ������� ������� � ������<paramref name="columnNames"/>. ���� � ��������� �� ������������ ������
    /// ������� �� ������ ����������, ������� ���������� ����� ����� �������� ������ �� ���������� ����
    /// </summary>
    /// <param name="columnNames">����� ��������, ����������� ��������. ��� �������� ���������� �� ��������, ����� ���� ������ ������������ "!"</param>
    /// <returns>������ GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames)
    {
      return Add(columnNames, columnNames);
    }

    #endregion
  }
#endif
}
