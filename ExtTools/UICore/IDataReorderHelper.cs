using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using System.ComponentModel;
using FreeLibSet.Models.Tree;
using FreeLibSet.Collections;

namespace FreeLibSet.UICore
{
  /// <summary>
  /// ��������� ���������������� �������, ���������������� ��� ������ ������������ ����� ���������� ���������, ���������� � DataTable.
  /// ��������� ������� ����� � ��������� ������������ �������� �����.
  /// ��������� ����������� �������� DataTableReorderHelper (������� �������) � DataTableTreeReorderHelper (������).
  /// ������������ ��� ���������� ������ ���������� ���� � EFPDataGridViewCommandItems � EFPDataTreeViewCommandItems.
  /// </summary>
  public interface IDataReorderHelper
  {
    #region ������

    /// <summary>
    /// ����������� ��������� ����� ����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� �, ��������,
    /// ��� ������ ����� � ���������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ����� ������ � �� ������ ����������.</returns>
    bool MoveDown(DataRow[] rows);

    /// <summary>
    /// ����������� ��������� ����� �����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� �, ��������,
    /// ��� ������ ����� � ���������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ������ ������ � �� ������ ����������.</returns>
    bool MoveUp(DataRow[] rows);

    /// <summary>
    /// ������������� ������ ����� ���������� � �������� ��� ��������������.
    /// ���� �������� ���� ��� ���������� ����� �������� 0, �� ��� ���� ��������������� ����� ��������, ����� ������
    /// ��������� � ����� ������ (��� ������ - � �������� ������ ������).
    /// ���� ���� ��� �������� ������� ��������, �� ������� �������� �� �����������.
    /// </summary>
    /// <param name="rows">������ ������, ������� ����� ����������������</param>
    /// <param name="otherRowsChanged">���� ������������ �������� true, ���� ���� �������� ������ ������ � ���������, ����� ���������.
    /// ������������ ������ ��� ������������� ���������, ��� ������� ������� (DataTableReorderHelper) ������ ������������ false.</param>
    /// <returns>True, ���� ������ (���� ��� ���������) ��������� ������� �������� � ���� ����������������.
    /// ���� ��� ������ ��� ��������� ��������� ��������, �� ������������ false.</returns>
    bool InitRows(DataRow[] rows, out bool otherRowsChanged);

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... ������� ����� � ��������� �� ����� �� ��������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    bool Reorder();

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... � ������������ � ��������� ��������.
    /// ��������������, ��� <paramref name="desiredOrder"/> �������� ��� ������ ���������.
    /// ��� ������ �������������� ������� ���������� ����� ����������, ��� ��� �� ��������� ������� ������ ����� ������,
    /// � <paramref name="desiredOrder"/> �� ������ ��� ���������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    bool Reorder(DataRow[] desiredOrder);

    #endregion
  }

  /// <summary>
  /// ������ ��� ������ ������������ ����� � ������� �������
  /// </summary>
  public class DataTableReorderHelper : IDataReorderHelper
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="dv">�������� DataView</param>
    /// <param name="orderColumnName">�������� �������, ������������ ��� ����������</param>
    public DataTableReorderHelper(DataView dv, string orderColumnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(orderColumnName))
        throw new ArgumentNullException("orderColumnName");
#endif

      _DV = dv;
      _OrderColumnPos = _DV.Table.Columns.IndexOf(orderColumnName);
      if (_OrderColumnPos < 0)
        throw new ArgumentException("������� " + dv.Table.TableName + " �� �������� ������� � ������ \"" + orderColumnName + "\"", "orderColumnName");
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� DataView.
    /// </summary>
    public DataView DV { get { return _DV; } }
    private DataView _DV;

    /// <summary>
    /// ��� ��������� �������, ������������� ��� ����������
    /// </summary>
    public string OrderColumnName { get { return _DV.Table.Columns[_OrderColumnPos].ColumnName; } }
    /// <summary>
    /// ������� ��������� �������, ������������� ��� ����������
    /// </summary>
    private int _OrderColumnPos;

    /// <summary>
    /// ���������� true, ���� �������� DV ������������ �� OrderColumnName
    /// </summary>
    private bool IsSuitableDV
    {
      get
      {
        if (String.IsNullOrEmpty(_DV.Sort))
          return false; // ��� ����������
        if (_DV.Sort.IndexOf(',') >= 0)
          return false; // ���������� �� ���������� �����

        if (_DV.AllowNew)
          return false; // ����� ����� �������� "�����������" ������

        string realColName;
        ListSortDirection realDir;
        DataTools.GetDataViewSortSingleColumnName(_DV.Sort, out realColName, out realDir);
        return String.Equals(realColName, OrderColumnName, StringComparison.OrdinalIgnoreCase) &&
          realDir == ListSortDirection.Ascending;
      }
    }

    #endregion

    #region IDataReorderHelper Members

    /// <summary>
    /// ����������� ��������� ����� ����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� � ��� ������ ����� � ���������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ����� ������ � �� ������ ����������.</returns>
    public bool MoveDown(DataRow[] rows)
    {
      return DoMove(rows, true);
    }

    /// <summary>
    /// ����������� ��������� ����� �����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� � ��� ������ ����� � ���������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ������ ������ � �� ������ ����������.</returns>
    public bool MoveUp(DataRow[] rows)
    {
      return DoMove(rows, false);
    }

    private bool DoMove(DataRow[] selRows, bool down)
    {
#if DEBUG
      if (selRows == null)
        throw new ArgumentNullException("rows");
#endif

      if (selRows.Length == 0)
        return false;

      // 1. ��������� ������ ������ ����� DataRow � ������
      DataRow[] rows1 = DataTools.GetDataViewRows(_DV);

      // 2. �������� ������� ��������� ����� � ������� ���� �����
      int[] selPoss = GetSelRowPositions(ref selRows, rows1);
      if (selRows.Length == 0)
        return false; // ��� ���� ��������, �.�. ������ ����� ���� �������

      // 3. ���������, ��� �� �������� � ������� ������
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Length - 1)
          return false;
      }
      else
      {
        if (selPoss[0] == 0)
          return false;
      }

      // 4. �������������� ������ ����� ��� �� ���������� � ����� �������
      // �������� null � ���� ������� �������� �������� ������ �������
      DataRow[] rows2 = new DataRow[rows1.Length];

      // 5. �������� � rows2 ������ �� Rows1 �� ������� ��� �������, ������������ � selRows.
      // � �������� ����������� ����� ������� ������ Rows1
      int delta = down ? 1 : -1; // �������� ��������
      for (int i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 6. ���������� �������� ������ � ���������� �������� ������ ��������� �
      // ����� �������, ��������� ������ �����. ��� ����� ���������� ���������� FreePos
      // ��� �������� �� ��������� ������ ������� ������� �������
      int freePos = 0;
      for (int i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] == null) // ������������ �������
          continue;
        // ����� �����
        while (rows2[freePos] != null)
          freePos++;
        // ����� �����
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 7. ���������� ������ ����� � ���� �������� ������ ������� � Rows2
      bool changed = false;
      for (int i = 0; i < rows2.Length; i++)
      {
        if (DataTools.GetInt(rows2[i][_OrderColumnPos]) != (i + 1))
        {
          rows2[i][_OrderColumnPos] = i + 1;
          changed = true;
        }
      }

      return changed;
    }

    /// <summary>
    /// ������� ������� ��������� ��� ����������� ����� � ���������.
    /// �� ������� ��������� ����� ������������� ����������� ������.
    /// ����� ������ ��������� ����� ����������� �� ����������� ������� �������.
    /// </summary>
    /// <param name="selRows">��������� ������. ���� ������ ����������� �, ��������, �����������</param>
    /// <param name="rows1"></param>
    /// <returns></returns>
    internal static int[] GetSelRowPositions(ref DataRow[] selRows, DataRow[] rows1)
    {
      // ������������� ���������.
      // ������������ ��� ���������� � ������ ��������.
      // ���� - ������� ������ �� selRows, �������� - ������.
      SortedDictionary<int, DataRow> dict = new SortedDictionary<int, DataRow>();

      for (int i = 0; i < selRows.Length; i++)
      {
        if (selRows[i] == null)
          throw new ArgumentException("rows[" + i.ToString() + "]=null", "rows");

        int p = Array.IndexOf<DataRow>(rows1, selRows[i]);
        if (p < 0)
          continue;
        if (dict.ContainsKey(p))
          continue; // ������ ������ ������
        dict.Add(p, selRows[i]);
      }

      if (dict.Count != selRows.Length)
        selRows = new DataRow[dict.Count];
      int[] selPoss = new int[dict.Count];
      dict.Keys.CopyTo(selPoss, 0);
      dict.Values.CopyTo(selRows, 0);

      return selPoss;
    }

    /// <summary>
    /// ������������� ������ ����� ���������� � �������� ��� ��������������.
    /// ���� �������� ���� ��� ���������� ����� �������� 0, �� ��� ���� ��������������� ����� ��������, ����� ������
    /// ��������� � ����� ������ (��� ������ - � �������� ������ ������).
    /// ���� ���� ��� �������� ������� ��������, �� ������� �������� �� �����������.
    /// </summary>
    /// <param name="rows">������ ������, ������� ����� ����������������</param>
    /// <param name="otherRowsChanged">���� ������������ false.</param>
    /// <returns>True, ���� ������ (���� ��� ���������) ��������� ������� �������� � ���� ����������������.
    /// ���� ��� ������ ��� ��������� ��������� ��������, �� ������������ false.</returns>
    public bool InitRows(DataRow[] rows, out bool otherRowsChanged)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif
      otherRowsChanged = false; // ������
      if (rows.Length == 0)
        return false;

      bool changed = false;
      int maxVal = -1; // ���� �� ����������
      for (int i = 0; i < rows.Length; i++)
      {
        if (DataTools.GetInt(rows[i][_OrderColumnPos]) == 0)
        {
          changed = true;
          if (maxVal < 0)
          {
            if (IsSuitableDV)
            {
              if (_DV.Count == 0)
                // ������ ����� � �� ���� ���� � ���������, ������� ������ ����������� DataView.Count=0.
                maxVal = 0;
              else
                maxVal = DataTools.GetInt(_DV[_DV.Count - 1].Row[_OrderColumnPos]);
            }
            else
            {
              // ��� ������ ��������� ����������� DataView, ����� ����� ������������ ��������
              maxVal = DataTools.MaxInt(_DV, OrderColumnName, true) ?? 0;
            }
          }

          maxVal++;
          rows[i][_OrderColumnPos] = maxVal;
        }
      }

      return changed;
    }

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... ������� ����� � ��������� �� ����� �� ��������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    public bool Reorder()
    {
      return Reorder(DataTools.GetDataViewRows(_DV));
    }

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... � ������������ � ��������� ��������.
    /// ��������������, ��� <paramref name="desiredOrder"/> �������� ��� ������ ���������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    public bool Reorder(DataRow[] desiredOrder)
    {
#if DEBUG
      if (desiredOrder == null)
        throw new ArgumentNullException("desiredOrder");
#endif

      bool changed = false;
      for (int i = 0; i < desiredOrder.Length; i++)
      {
        if (DataTools.GetInt(desiredOrder[i][_OrderColumnPos]) != i + 1)
        {
          changed = true;
          desiredOrder[i][_OrderColumnPos] = i + 1;
        }
      }
      return changed;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ������ ������������ ����� � ������� �������������� ��������� �� ��������� IDataTableTreeModel.
  /// ������������ ����� ����������� � �������� ������ ������������� ����, �� ��� ���� ����� �������� ������� ���������
  /// �����, ����� ������� ��������� �������� ��������� ������ � ��� �� �������, ��� � ������.
  /// </summary>
  public class DataTableTreeReorderHelper : IDataReorderHelper
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="model">������ �������������� ���������</param>
    /// <param name="orderColumnName">�������� �������, ������������ ��� ����������</param>
    public DataTableTreeReorderHelper(IDataTableTreeModel model, string orderColumnName)
    {
#if DEBUG
      if (model == null)
        throw new ArgumentNullException("model");
      if (String.IsNullOrEmpty(orderColumnName))
        throw new ArgumentNullException("orderColumnName");
#endif

      _Model = model;
      _OrderColumnPos = model.Table.Columns.IndexOf(orderColumnName);
      if (_OrderColumnPos < 0)
        throw new ArgumentException("������� " + model.Table.TableName + " �� �������� ������� � ������ \"" + orderColumnName + "\"", "orderColumnName");
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ �������������� ���������
    /// </summary>
    public IDataTableTreeModel Model { get { return _Model; } }
    private IDataTableTreeModel _Model;

    /// <summary>
    /// ��� ��������� �������, ������������� ��� ����������
    /// </summary>
    public string OrderColumnName { get { return _Model.Table.Columns[_OrderColumnPos].ColumnName; } }
    /// <summary>
    /// ������� ��������� �������, ������������� ��� ����������
    /// </summary>
    private int _OrderColumnPos;

    /// <summary>
    /// ���������� true, ���� �������� Model.Table.DefaultView ������������ �� OrderColumnName
    /// </summary>
    private bool IsSuitableDV
    {
      get
      {
        if (String.IsNullOrEmpty(_Model.Table.DefaultView.Sort))
          return false; // ��� ����������
        if (_Model.Table.DefaultView.Sort.IndexOf(',') >= 0)
          return false; // ���������� �� ���������� �����

        if (_Model.Table.DefaultView.AllowNew)
          return false; // ����� ����� �������� "�����������" ������

        string realColName;
        ListSortDirection realDir;
        DataTools.GetDataViewSortSingleColumnName(_Model.Table.DefaultView.Sort, out realColName, out realDir);
        return String.Equals(realColName, OrderColumnName, StringComparison.OrdinalIgnoreCase) &&
          realDir == ListSortDirection.Ascending;
      }
    }

    #endregion

    #region IDataReorderHelper Members

    /// <summary>
    /// ����������� ��������� ����� ����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� � ��� ������ ����� � ���������.
    /// ���� ������ ��������� � ������ ������������ �����, �� ������� �������� �� �����������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ����� ������ � �� ������ ����������.</returns>
    public bool MoveDown(DataRow[] rows)
    {
      return DoMove(rows, true);
    }

    /// <summary>
    /// ����������� ��������� ����� �����.
    /// ����� ������ �������� ��������� ����, ���������������� ��� ����������, �� ������ ��� ��������� �����, �� � ��� ������ ����� � ���������.
    /// ���� ������ ��������� � ������ ������������ �����, �� ������� �������� �� �����������.
    /// </summary>
    /// <param name="rows">������, ��������� � ���������</param>
    /// <returns>True, ���� ����������� ���� ���������.
    /// False, ���� ������ ��� ��������� ������ ������ � �� ������ ����������.</returns>
    public bool MoveUp(DataRow[] rows)
    {
      return DoMove(rows, false);
    }

    private bool DoMove(DataRow[] selRows, bool down)
    {
#if DEBUG
      if (selRows == null)
        throw new ArgumentNullException("rows");
#endif

      if (selRows.Length == 0)
        return false;

      // 0. ���������, ��� ��� ������ ��������� � ������ ������������� ���� � �������� ���� � ������������� ����
      TreePath parentPath = Model.TreePathFromDataRow(selRows[0]).Parent;
      for (int i = 1; i < selRows.Length; i++)
      {
        TreePath thisPath = Model.TreePathFromDataRow(selRows[i]);
        if (!thisPath.IsChildOf(parentPath))
          return false;
      }

      // 1. ��������� ������ ������ ����� DataRow ��� ������ ������
      List<DataRow> rows1 = new List<DataRow>();
      foreach (object node in Model.GetChildren(parentPath))
      {
        TreePath rowPath = new TreePath(parentPath, node);
        rows1.Add(Model.TreePathToDataRow(rowPath));
      }

      // 2. �������� ������� ��������� ����� � ������� ���� �����
      int[] selPoss = DataTableReorderHelper.GetSelRowPositions(ref selRows, rows1.ToArray());
      if (selRows.Length == 0)
        return false; // ��� ���� ��������, �.�. ������ ����� ���� �������

      // 3. ���������, ��� �� �������� � ������� ������ � �������� ������
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Count - 1)
          return false;
      }
      else
      {
        if (selPoss[0] == 0)
          return false;
      }

      // 4. �������������� ������ ����� ��� �� ���������� � ����� �������
      // �������� null � ���� ������� �������� �������� ������ �������
      DataRow[] rows2 = new DataRow[rows1.Count];

      // 5. �������� � rows2 ������ �� rows1 �� ������� ��� �������, ������������ � selRows.
      // � �������� ����������� ����� ������� ������ Rows1
      int delta = down ? 1 : -1; // �������� ��������
      for (int i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 6. ���������� �������� ������ � ���������� �������� ������ ��������� �
      // ����� �������, ��������� ������ �����. ��� ����� ���������� ���������� FreePos
      // ��� �������� �� ��������� ������ ������� ������� �������
      int freePos = 0;
      for (int i = 0; i < rows1.Count; i++)
      {
        if (rows1[i] == null) // ������������ �������
          continue;
        // ����� �����
        while (rows2[freePos] != null)
          freePos++;
        // ����� �����
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 7. ���������� ������ ����� � ���� �������� ������ ������� � Rows2
      bool changed = false;
      for (int i = 0; i < rows2.Length; i++)
      {
        if (DataTools.GetInt(rows2[i][_OrderColumnPos]) != (i + 1))
        {
          rows2[i][_OrderColumnPos] = i + 1;
          changed = true;
        }
      }

      // 8. ������������ ��������� �� ���� ������
      if (changed)
        Reorder();

      return changed;
    }

    /// <summary>
    /// ������������� ������ ����� ���������� � �������� ��� ��������������.
    /// ���� �������� ���� ��� ���������� ����� �������� 0, �� ��� ���� ��������������� ����� ��������, ����� ������
    /// ��������� � ����� ������ � �������� ������ ������������ ��������.
    /// ���� ���� ��� �������� ������� ��������, �� ������� �������� �� �����������.
    /// </summary>
    /// <param name="rows">������ ������, ������� ����� ����������������.
    /// � ������� �� ������� MoveUp() � MoveDown(), ������ �� ������� ���������� � ������ ������������� ����</param>
    /// <param name="otherRowsChanged">���� ������������ �������� true, ���� ���� �������� ������ ������ � ���������, ����� ���������.</param>
    /// <returns>True, ���� ������ (���� ��� ���������) ��������� ������� �������� � ���� ����������������.
    /// ���� ��� ������ ��� ��������� ��������� ��������, �� ������������ false.</returns>
    public bool InitRows(DataRow[] rows, out bool otherRowsChanged)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      int maxVal = -1;
      bool changed = false;
      for (int i = 0; i < rows.Length; i++)
      {
        if (DataTools.GetInt(rows[i][_OrderColumnPos]) == 0)
        {
          changed = true;
          if (maxVal < 0)
          {
            if (IsSuitableDV)
            {
              if (Model.Table.DefaultView.Count == 0)
                // ������ ����� � �� ���� ���� � ���������, ������� ������ ����������� DataView.Count=0.
                maxVal = 0;
              else
                maxVal = DataTools.GetInt(Model.DataView[Model.DataView.Count - 1].Row[_OrderColumnPos]);
            }
            else
            {
              // ��� ������ ��������� ����������� DataView, ����� ����� ������������ ��������
              maxVal = DataTools.MaxInt(Model.Table.DefaultView, OrderColumnName, true) ?? 0;
            }
          }

          maxVal++;
          rows[i][_OrderColumnPos] = maxVal;
        }
      }

      if (changed)
        otherRowsChanged = Reorder();
      else
        otherRowsChanged = false;
      return changed;
    }

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... ������� ����� � ��������� �� ����� �� ��������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    public bool Reorder()
    {
      bool changed = false;
      int cnt = 0;
      foreach (TreePath path in new TreePathEnumerable(_Model))
      {
        cnt++;
        DataRow row = _Model.TreePathToDataRow(path);
        if (DataTools.GetInt(row[_OrderColumnPos]) != cnt)
        {
          changed = true;
          row[_OrderColumnPos] = cnt;
        }
      }
      return changed;
    }

    /// <summary>
    /// ������������� ��������� ���� ���������� 1,2,3,... � ������������ � ��������� ��������.
    /// ��������������, ��� <paramref name="desiredOrder"/> �������� ��� ������ ���������.
    /// �������������� ������� ���������� ����� ����������, ��� ��� �� ��������� ������� ������ ����� ������,
    /// � <paramref name="desiredOrder"/> �� ������ ��� ���������.
    /// </summary>
    /// <returns>True, ���� �������� ���� �������� ���� �� ��� ����� ������</returns>
    public bool Reorder(DataRow[] desiredOrder)
    {
      // 1. �������� ������ ����� ������ � ������� �������
      List<DataRow> rows1 = new List<DataRow>();
      foreach (TreePath rowPath in new TreePathEnumerable(Model))
        rows1.Add(Model.TreePathToDataRow(rowPath));

      // 2. ���������, ����� ������ ���������
      if (desiredOrder.Length == rows1.Count)
      {
        bool areSame = true;
        for (int i = 0; i < desiredOrder.Length; i++)
        {
          if (!Object.ReferenceEquals(desiredOrder[i], rows1[i]))
          {
            areSame = false;
            break;
          }
          if (DataTools.GetInt(rows1[i][_OrderColumnPos]) != (i + 1))
          {
            // ������ ��������� ��������������
            areSame = false;
            break;
          }
        }
        if (areSame)
          return false;
      }

      // 3. ��������� ��������� ��� ����� ��������� ������
      for (int i = 0; i < desiredOrder.Length; i++)
        desiredOrder[i][_OrderColumnPos] = i + 1;

      // 4. ���������������� ��� ��� ��� ����� ��������
      Reorder();

      return true;
    }

    #endregion
  }
}
