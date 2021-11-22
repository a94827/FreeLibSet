// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ��������� �������� ���� ������ � �������� � DBxConBase ��� ���������� ��������.
  /// ��� �������� ������������ ���� �������� ��������� ������ � DBxSqlBuffer.DBxColumnStruct
  /// </summary>
  public sealed class DBxNameValidator
  {
    #region �����������

    /// <summary>
    /// ������� ������ ��� �������� ����� ����������� � ���� ������
    /// </summary>
    /// <param name="entry">����� ����������� � ���� ������</param>
    /// <param name="buffer">����� ��� �������� SQL-��������</param>
    public DBxNameValidator(DBxEntry entry, DBxSqlBuffer buffer)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      _Entry = entry;
      _Buffer = buffer;
      _NameCheckingEnabled = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ����������� � ���� ������
    /// </summary>
    public DBxEntry Entry { get { return _Entry; } }
    private DBxEntry _Entry;

    /// <summary>
    /// ����� ��� �������� SQL-��������
    /// </summary>
    public DBxSqlBuffer Buffer { get { return _Buffer; } }
    private DBxSqlBuffer _Buffer;

    /// <summary>
    /// ���� �������� ����������� (�� ���������), �� ����������� �������� �������������
    /// �������� ������ � ����� � �������� ��������� �������.
    /// ���� �������� �������� � false, ����������� ������ ����� ������������ ���� (�� ������� ������������ ��������)
    /// ��� �������� �� ����������� � �������� ���������� DBxCon. �������������, �������� ����� ���� ���������
    /// ������ �� ������� ������� (������������)
    /// </summary>
    public bool NameCheckingEnabled
    {
      get { return _NameCheckingEnabled; }
      set { _NameCheckingEnabled = value; }
    }
    private bool _NameCheckingEnabled;

    #endregion

    #region ������ ��������

    /// <summary>
    /// �������� ����� ������� �� ������������ � ���� �� ������/������ � �������
    /// ���� �������� NameCheckingEnabled �����������, ����������� ����� ������� �������� ������� � �������� ��������� ���� ������
    /// </summary>
    /// <param name="tableName">����������� ���</param>
    /// <param name="mode">����������� ����� ������������� ������� (Full - ���������, ReadOnly - ������)</param>
    public void CheckTableName(string tableName, DBxAccessMode mode)
    {
      string ErrorText;
      if (!Entry.DB.IsValidTableName(tableName, out ErrorText))
        throw new ArgumentException("������������ ��� ������� \"" + tableName + "\". " + ErrorText);

      if (NameCheckingEnabled)
      {
        if (!Entry.DB.Struct.Tables.Contains(tableName))
          throw new ArgumentException("����������� ��� ������� \"" + tableName + "\" �� ���������� ��� �� \"" + Entry.DB.ToString() + "\"", "tableName");
      }

      switch (mode)
      {
        case DBxAccessMode.Full:
          if (Entry.Permissions.TableModes[tableName] != DBxAccessMode.Full)
            throw new DBxAccessException("��� ���������� �� ������ � ������� \"" + tableName + "\"");
          break;
        case DBxAccessMode.ReadOnly:
          if (Entry.Permissions.TableModes[tableName] == DBxAccessMode.None)
            throw new DBxAccessException("��� ���������� �� ������ � ������� \"" + tableName + "\"");
          break;
      }
    }

    /// <summary>
    /// �������� ����� �������, ������� ������� ��� � ������� ������ � ������� � ����.
    /// ���� ��� ������� ������������ ��� ������� ����������, �� ������������� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� �������</param>
    /// <param name="allowDots">����� �� ���� ��������� �����</param>
    /// <param name="mode">����� �������</param>
    /// <returns>���������� ��� �������</returns>
    public DBxColumnType CheckTableColumnName(string tableName, string columnName, bool allowDots, DBxAccessMode mode)
    {
      DBxColumnStruct cs = DoCheckTableColumnName(tableName, columnName, allowDots, mode);
      if (cs != null)
      {
        Buffer.ColumnStructs[columnName] = cs; // �� Add(), ��� ��� ����� ���� ������������� ������
        return cs.ColumnType;
      }
      else
        return DBxColumnType.Unknown;
    }

    private DBxColumnStruct DoCheckTableColumnName(string tableName, string columnName, bool allowDots, DBxAccessMode mode)
    {
      string ErrorText;
      if (!Entry.DB.IsValidColumnName(columnName, allowDots, out ErrorText))
        throw new ArgumentException("������������ ��� ������� \"" + columnName + "\". " + ErrorText, "columnName");

      int pDot = columnName.IndexOf('.');

      if (pDot >= 0)
      {
        string MainColumnName = columnName.Substring(0, pDot);
        DBxColumnStruct ColDef = Entry.DB.Struct.Tables[tableName].Columns[MainColumnName];
        if (ColDef == null)
        {
          if (NameCheckingEnabled)
            throw new ArgumentException("����������� ��� ������� \"" + MainColumnName + "\" ��� � ����������� ������� \"" + tableName + "\" �� \"" + Entry.DB.ToString() + "\"", "columnName");
          else
            return null;
        }
        if (String.IsNullOrEmpty(ColDef.MasterTableName))
          throw new ArgumentException("������� \"" + MainColumnName + "\" ������� \"" + tableName + "\" �� \"" + Entry.DB.ToString() + "\" �� �������� ���������", "columnName");

        if (NameCheckingEnabled)
          CheckTableName(ColDef.MasterTableName, mode);

        // ����������� �����
        return DoCheckTableColumnName(ColDef.MasterTableName, columnName.Substring(pDot + 1), true, mode);
      }
      else
      {
        DBxTableStruct ts = Entry.DB.Struct.Tables[tableName];
        if (ts == null)
        {
          if (NameCheckingEnabled)
            throw new ArgumentException("��� ����������� ��� ������� \"" + tableName + "\" �� \"" + Entry.DB.ToString() + "\"", "tableName");
          else
            return null; // 22.07.2021
        }
        if (!ts.Columns.Contains(columnName))
        {
          if (NameCheckingEnabled)
            throw new ArgumentException("����������� ��� ������� \"" + columnName + "\" ��� � ����������� ������� \"" + tableName + "\" �� \"" + Entry.DB.ToString() + "\"", "columnName");
          else
            return null;
        }

        if (NameCheckingEnabled)
        {
          switch (mode)
          {
            case DBxAccessMode.Full:
              switch (Entry.Permissions.ColumnModes[tableName, columnName])
              {
                case DBxAccessMode.ReadOnly:
                  throw new DBxAccessException("��������� ��������� ���� \"" + columnName + "\" ������� \"" + tableName + "\". ���� ����� ������ �� �������� ����");
                case DBxAccessMode.None:
                  throw new DBxAccessException("�������� ������ � ���� \"" + columnName + "\" ������� \"" + tableName + "\"");
              }
              break;
            case DBxAccessMode.ReadOnly:
              if (Entry.Permissions.ColumnModes[tableName, columnName] == DBxAccessMode.None)
                throw new DBxAccessException("�������� ������ � ���� \"" + columnName + "\" ������� \"" + tableName + "\"");
              break;
          }
        }
        return Entry.DB.Struct.Tables[tableName].Columns[columnName];
      }
    }

    /// <summary>
    /// �������� ���� ������ ��������, ������� ������� �� � ������� ������ � ������� � ���.
    /// ���� ��� ������-���� ������� � ������ ������������ ��� ������� ����������, �� ������������� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">������ ����������� ���� ��������</param>
    /// <param name="allowDots">����� �� ���� ��������� �����</param>
    /// <param name="mode">����� �������</param>
    /// <returns>���������� ���� ��������. ����� ������� ������������� ������ <paramref name="columnNames"/>.</returns>
    public DBxColumnType[] CheckTableColumnNames(string tableName, DBxColumns columnNames, bool allowDots, DBxAccessMode mode)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.Count == 0)
        throw new ArgumentException("������ ������ ���� �����", "columnNames");

      DBxColumnType[] ColumnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        ColumnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return ColumnTypes;
    }

    /// <summary>
    /// �������� ���� ������ ��������, ������� ������� �� � ������� ������ � ������� � ���.
    /// ���� ��� ������-���� ������� � ������ ������������ ��� ������� ����������, �� ������������� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">������ ����������� ���� ��������</param>
    /// <param name="allowDots">����� �� ���� ��������� �����</param>
    /// <param name="mode">����� �������</param>
    /// <returns>���������� ���� ��������. ����� ������� ������������� ������ <paramref name="columnNames"/>.</returns>
    public DBxColumnType[] CheckTableColumnNames(string tableName, DBxColumnList columnNames, bool allowDots, DBxAccessMode mode)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.Count == 0)
        throw new ArgumentException("������ ������ ���� �����", "columnNames");

      DBxColumnType[] ColumnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        ColumnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return ColumnTypes;
    }



    /// <summary>
    /// ���������� ������, ������������ ��� �������� �������� � ������� ����������
    /// </summary>
    private DBxColumnList CheckColumnList;


    /// <summary>
    /// �������� ���� ������ ��������, ������� ������� �� � ������� ������ � ������� � ���.
    /// ����� ����� ����������� �� DBxNamedExpressionList.
    /// ���� ��� ������-���� ������� � ������ ������������ ��� ������� ����������, �� ������������� ����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="expressions">������ ����������� ���������</param>
    public void CheckExpressionColumnNames(string tableName, DBxNamedExpressionList expressions)
    {
      DBxColumnList list = new DBxColumnList();
      expressions.GetColumnNames(list);
      for (int i = 0; i < list.Count; i++)
        CheckTableColumnName(tableName, list[i], true, DBxAccessMode.ReadOnly);
    }

    /// <summary>
    /// �������� ���� ������ ��������, ������� ������� �� � ������� ������ � ������� � ���.
    /// ����� ����� ����������� �� DBxNamedExpressionList.
    /// ���� ��� ������-���� ������� � ������ ������������ ��� ������� ����������, �� ������������� ����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="expressions">������ ����������� ���������</param>
    public void CheckExpressionColumnNames(string tableName, IList<DBxExpression> expressions)
    {
      DBxColumnList list = new DBxColumnList();
      for (int i = 0; i < expressions.Count;i++ )
        expressions[i].GetColumnNames(list);
      for (int i = 0; i < list.Count; i++)
        CheckTableColumnName(tableName, list[i], true, DBxAccessMode.ReadOnly);
    }

    /// <summary>
    /// ��������� �������� ���� �������� � ��������.
    /// ����� �������� ������ ���� � ������� ������� DBxFilter.GetColumnNames(),
    /// � ����� �������� CheckTableColumnName() ��� ���� ���� �����
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="filter">������. �� ����� ���� null</param>
    /// <param name="allowDots">����� �� ���� ��������� �����</param>
    public void CheckFilterColumnNames(string tableName, DBxFilter filter, bool allowDots)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (CheckColumnList == null)
        CheckColumnList = new DBxColumnList();
      CheckColumnList.Clear();
      filter.GetColumnNames(CheckColumnList);
      for (int i = 0; i < CheckColumnList.Count; i++)
        CheckTableColumnName(tableName, CheckColumnList[i], allowDots, DBxAccessMode.ReadOnly);
    }

    ///// <summary>
    ///// ��������� �������� ���� �������� � ������� ����������.
    ///// ����� �������� ������ ���� � ������� ������� DBxOrder.GetColumnNames(),
    ///// � ����� �������� CheckTableColumnName() ��� ���� ���� �����
    ///// </summary>
    ///// <param name="tableName">��� �������</param>
    ///// <param name="order">������� ����������. �� ����� ���� null</param>
    ///// <param name="allowDots">����� �� ���� ��������� �����</param>
    //public void CheckOrderColumnNames(string tableName, DBxOrder order, bool allowDots)
    //{
    //  if (order == null)
    //    throw new ArgumentNullException("order");

    //  if (CheckColumnList == null)
    //    CheckColumnList = new DBxColumnList();
    //  CheckColumnList.Clear();
    //  order.GetColumnNames(CheckColumnList);
    //  for (int i = 0; i < CheckColumnList.Count; i++)
    //    CheckTableColumnName(tableName, CheckColumnList[i], allowDots, DBxAccessMode.ReadOnly);
    //}

    /// <summary>
    /// ���������, ��� ��������� ������ ������� �������� ������������ ������������� ����.
    /// ���� ��� �� ���, ������������ DBxPrimaryKeyException.
    /// ��������������, ��� �������� CheckTableName ��� ���������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>��� ���� ���������� �����</returns>
    public string CheckTablePrimaryKeyInt32(string tableName)
    {
      DBxTableStruct ts = Entry.DB.Struct.Tables[tableName];
      return ts.CheckTablePrimaryKeyInt32();
    }

    /// <summary>
    /// ���������� ������ ���� ���������� ����� � ������ �������� <paramref name="columnNames"/>
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">������ �������� ��� ������</param>
    /// <returns>������ ������� ��� (-1), ���� ������� �� ������</returns>
    public int GetPrimaryKeyInt32ColumnIndex(string tableName, DBxColumns columnNames)
    {
      DBxTableStruct ts = Entry.DB.Struct.Tables[tableName];
      if (ts.PrimaryKey.Count != 1)
        return -1;

      DBxColumnStruct cs = ts.Columns[ts.PrimaryKey[0]];
      if (cs.ColumnType != DBxColumnType.Int)
        return -1;

      return columnNames.IndexOf(cs.ColumnName);
    }

    #endregion
  }
}
