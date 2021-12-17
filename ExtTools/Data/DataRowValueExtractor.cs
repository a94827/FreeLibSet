// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowIntExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowIntExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public int this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0;
          else
            return (int)v;
        }
        else
          return DataTools.GetInt(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableIntExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableIntExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public int? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (int)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowInt64Extractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowInt64Extractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public long this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0L;
          else
            return (Int64)v;
        }
        else
          return DataTools.GetInt64(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableInt64Extractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableInt64Extractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public long? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (long)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt64(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowSingleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowSingleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public float this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Single))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0f;
          else
            return (float)v;
        }
        else
          return DataTools.GetSingle(row[~_ColumnIndex]);
      }
    }

    #endregion
  }


  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableSingleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableSingleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public float? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (float)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetSingle(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDoubleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowDoubleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public double this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0.0;
          else
            return (double)v;
        }
        else
          return DataTools.GetDouble(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDoubleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableDoubleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public double? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (double)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDouble(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDecimalExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowDecimalExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public decimal this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0m;
          else
            return (decimal)v;
        }
        else
          return DataTools.GetDecimal(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDecimalExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableDecimalExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public decimal? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (decimal)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDecimal(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// DataRowNullableDateTimeExtractor ���������� �������� null, ���� ���� �������� ������ ��������,
  /// � DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDateTimeExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowDateTimeExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public DateTime this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _ColumnIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _ColumnName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return DateTime.MinValue;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// DataRowNullableDateTimeExtractor ���������� �������� null, ���� ���� �������� ������ ��������,
  /// � DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDateTimeExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableDateTimeExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public DateTime? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(DateTime)) // 28.11.2017
            _ColumnIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _ColumnName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return null;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowTimeSpanExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowTimeSpanExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public TimeSpan this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(TimeSpan)) // ����. 5.12.2021
            _ColumnIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _ColumnName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return TimeSpan.Zero;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableTimeSpanExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableTimeSpanExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public TimeSpan? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(TimeSpan))
            _ColumnIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _ColumnName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return null;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowStringExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowStringExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public string this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(String))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return String.Empty;
          else
            return ((string)v).Trim();
        }
        else
          return DataTools.GetString(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowBoolExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowBoolExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public bool this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return false;
          else
            return (bool)v;
        }
        else
          return DataTools.GetBool(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableBoolExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DataRowNullableBoolExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public bool? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (bool)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetBool(v);
        }
      }
    }

    #endregion
  }
}
