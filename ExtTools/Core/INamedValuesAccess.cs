using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// ��������� ������� ������ ��� ������ � ����������� ���������.
  /// ������� ��������� � ��������� ��������� ��������������.
  /// </summary>
  public interface INamedValuesAccess
  {
    #region ������

    /// <summary>
    /// �������� �������� � �������� ������
    /// </summary>
    /// <param name="name">���. �� ����� ���� ������ �������</param>
    /// <returns>��������. ��� ����������, ���� ��������� �������������� ���, ������� �� ����������</returns>
    object GetValue(string name);

    /// <summary>
    /// ���������� ������� ��������� ����� � ���������.
    /// </summary>
    /// <param name="name">����������� ���</param>
    /// <returns>True, ���� � ��������� ���� �������� � ����� ������</returns>
    bool Contains(string name);

    /// <summary>
    /// �������� ������ ���� ����, ������� ���� � ���������
    /// </summary>
    /// <returns>������ ����</returns>
    string[] GetNames();

    #endregion
  }

  /// <summary>
  /// ������ ���������� ���������� INamedValuesAccess
  /// </summary>
  public sealed class DummyNamedValues:INamedValuesAccess
  {
    #region INamedValuesAccess Members

    /// <summary>
    /// ����������� ����������
    /// </summary>
    /// <param name="name">�� ������������</param>
    /// <returns>�� ������������</returns>
    public object GetValue(string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// ���������� false
    /// </summary>
    /// <param name="name">�� ������������</param>
    /// <returns>false</returns>
    public bool Contains(string name)
    {
      return false;
    }

    /// <summary>
    /// ���������� ������ ������
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      return DataTools.EmptyStrings;
    }

    #endregion
  }
}

namespace FreeLibSet.Data
{
  /// <summary>
  /// ���������� ���������� INamedValuesAccess ��� ������� � ��������� ������ DataRow.
  /// ����������� �������� DataTableValueArray �
  /// </summary>
  public interface IDataRowNamedValuesAccess : INamedValuesAccess
  {
    /// <summary>
    /// ������� ������.
    /// �������� ������ ���� ����������� ����� �������� � ���������.
    /// </summary>
    DataRow CurrentRow { get; set;}
  }

  /// <summary>
  /// ���������� ���������� INamedValuesAccess ��� ����� � ������� DataTable.
  /// ����� �������� �������� ������ ��������������� �������� CurrentRow, ����� ���� ����� �������� ������ � ��������� �����.
  /// ����� �� �������� ����������������.
  /// ���� ��� �����������, ��� ������ ��������� � ����� �������, ����������� ����� DataRowValueArray
  /// </summary>
  public class DataTableValueArray : IDataRowNamedValuesAccess
  {
    #region �����������

    /// <summary>
    /// ������� ������.
    /// ����� �������� ������� ������ ��������� ������� � �������.
    /// </summary>
    /// <param name="table">������� ������</param>
    public DataTableValueArray(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      _Table = table;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������, �������� � ������������
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    /// <summary>
    /// ������� ������.
    /// �������� ������ ���� ����������� ����� �������� � ���������.
    /// ����������� ����������� ������ ������ �� ������, ����������� � ������� Table.
    /// </summary>
    public DataRow CurrentRow
    {
      get { return _CurrentRow; }
      set
      {
        if (value != null)
        {
          if (!Object.ReferenceEquals(value.Table, _Table))
            throw new ArgumentException("������ ��������� � ������ �������");
        }
        _CurrentRow = value;
      }
    }
    private DataRow _CurrentRow;

    /// <summary>
    /// ���������� ���� �������� ��������
    /// </summary>
    protected StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
          _ColumnNameIndexer = new StringArrayIndexer(GetNames(), false);
        return _ColumnNameIndexer;
      }

    }
    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// ���������� ��� �������
    /// </summary>
    /// <returns>��������� ������������� ��� �������</returns>
    public override string ToString()
    {
      return Table.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    /// <summary>
    /// �������� �������� ���� �� ������� ������.
    /// �������� CurrentRow ������ ���� �������������� �����������.
    /// </summary>
    /// <param name="name">��� ����. ������������� � ��������</param>
    /// <returns>�������� ����</returns>
    public object GetValue(string name)
    {
      int p = ColumnNameIndexer.IndexOf(name);
      if (p < 0)
      {
        if (String.IsNullOrEmpty(name))
          throw new ArgumentNullException("name");
        else
          throw new ArgumentException("������� " + _Table.TableName + " �� �������� ������� \"" + name + "\"", "name");
      }
      if (_CurrentRow == null)
        throw new NullReferenceException("�������� CurrentRow �� �����������");
      return _CurrentRow[p];
    }

    /// <summary>
    /// ���������� true, ���� ������� �������� ������� � �������� ������
    /// </summary>
    /// <param name="name">��� ����</param>
    /// <returns>������� ����</returns>
    public bool Contains(string name)
    {
      return _ColumnNameIndexer.Contains(name);
    }

    /// <summary>
    /// ���������� ������ ���� ���� ��������
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      string[] FColumnNames = new string[Table.Columns.Count];
      for (int i = 0; i < Table.Columns.Count; i++)
        FColumnNames[i] = Table.Columns[i].ColumnName;
      return FColumnNames;
    }

    #endregion
  }

  /// <summary>
  /// ���������� ���������� INamedValuesAccess ��� ����� � ������� DataTable.
  /// ����� �������� �������� ������ ��������������� �������� CurrentRow, ����� ���� ����� �������� ������ � ��������� �����.
  /// ���� ������ �� �����������, ���������, ��� ������ �������� ������
  /// ����� �� �������� ����������������.
  /// </summary>
  public class DataRowValueArray : IDataRowNamedValuesAccess
  {
    #region �����������

    /// <summary>
    /// ������� ������.
    /// ���� ������ �� �����������, ���������, ��� ������ �������� ������
    /// </summary>
    public DataRowValueArray()
    {
    }

    #endregion

    #region ��������

    private DataTable _Table;

    /// <summary>
    /// ������� ������.
    /// �������� ������ ���� ����������� ����� �������� � ���������.
    /// ����������� ����������� ������ �� ������, ����������� � ������ ��������
    /// </summary>
    public DataRow CurrentRow
    {
      get { return _CurrentRow; }
      set
      {
        if (value != null)
        {
          if (!Object.ReferenceEquals(value.Table, _Table))
          {
            _ColumnNameIndexer = null;
            _Table = value.Table;
          }
        }
        _CurrentRow = value;
      }
    }
    private DataRow _CurrentRow;

    /// <summary>
    /// ���������� ���� �������� � �������.
    /// ����� �������� ��� ��������� �������� CurrentRow
    /// </summary>
    protected StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
          _ColumnNameIndexer = new StringArrayIndexer(GetNames(), false);
        return _ColumnNameIndexer;
      }

    }
    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// ���������� ��� �������
    /// </summary>
    /// <returns>��������� ������������� ��� �������</returns>
    public override string ToString()
    {
      if (_Table == null)
        return String.Empty;
      else
        return _Table.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    /// <summary>
    /// �������� �������� ���� �� ������� ������.
    /// �������� CurrentRow ������ ���� �������������� �����������.
    /// </summary>
    /// <param name="name">��� ����. ������������� � ��������</param>
    /// <returns>�������� ����</returns>
    public object GetValue(string name)
    {
      if (_CurrentRow == null)
        throw new NullReferenceException("�������� CurrentRow �� �����������");

      int p = ColumnNameIndexer.IndexOf(name);
      if (p < 0)
      {
        if (String.IsNullOrEmpty(name))
          throw new ArgumentNullException("name");
        else
          throw new ArgumentException("������� " + _Table.TableName + " �� �������� ������� \"" + name + "\"", "name");
      }
      return _CurrentRow[p];
    }

    /// <summary>
    /// ���������� true, ���� ������� �������� ������� � �������� ������
    /// </summary>
    /// <param name="name">��� ����</param>
    /// <returns>������� ����</returns>
    public bool Contains(string name)
    {
      if (_CurrentRow == null)
        return false;
      else
        return _ColumnNameIndexer.Contains(name);
    }

    /// <summary>
    /// ���������� ������ ���� ���� ��������
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      if (_CurrentRow == null)
        return DataTools.EmptyStrings;

      string[] names = new string[_Table.Columns.Count];
      for (int i = 0; i < _Table.Columns.Count; i++)
        names[i] = _Table.Columns[i].ColumnName;
      return names;
    }

    #endregion
  }
}