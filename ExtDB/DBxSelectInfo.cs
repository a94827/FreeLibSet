// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ���������, ����������� �������.
  /// ����� ������������ ��� ���������� ����� � ������� ����������� ������� SELECT � ������� ����������� "AS".
  /// </summary>
  [Serializable]
  public sealed class DBxNamedExpression : IObjectWithCode
  {
    #region ������������

    /// <summary>
    /// ������� ��������� � �������
    /// </summary>
    /// <param name="expression">���������. �� ����� ���� null</param>
    /// <param name="alias">�����. ������ ���� �����</param>
    public DBxNamedExpression(DBxExpression expression, string alias)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(alias))
        throw new ArgumentNullException("alias");
      _Expression = expression;
      _Alias = alias;
    }

    /// <summary>
    /// ������� ��������� ��� ����.
    /// ����� ��������������� ������ ����� ����.
    /// </summary>
    /// <param name="expression">��������� DBxColumn. ���� �������� ������ ���������, ������������� ����������. �� ����� ���� null.</param>
    public DBxNamedExpression(DBxExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      _Expression = expression;
      if (expression is DBxColumn)
        _Alias = ((DBxColumn)expression).ColumnName;
      else
        throw new ArgumentException("��� ������������� ��������� ���� " + expression.GetType() + " ��������� �������� ����� � ����� ����");
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� (��� �������� ����, ��������� ���� ��� �������).
    /// �� ����� ���� null
    /// </summary>
    public DBxExpression Expression { get { return _Expression; } }
    private DBxExpression _Expression;

    /// <summary>
    /// ����� (� SQL-������� ������������ � ���������� "AS")
    /// �� ����� ���� ������ �������
    /// </summary>
    public string Alias { get { return _Alias; } }
    private string _Alias;

    /// <summary>
    /// ���������� true, ���� ����� ������� ��������� �����������
    /// </summary>
    public bool AliasRequired
    {
      get
      {
        DBxColumn col = Expression as DBxColumn;
        if (col == null)
          return true;
        if (col.ColumnName.IndexOf('.') >= 0)
          return true;
        return col.ColumnName != Alias;
      }
    }

    /// <summary>
    /// ���������� �������� Alias (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return _Alias;
    }

    #endregion

    #region IObjectWithCode members

    string IObjectWithCode.Code { get { return _Alias; } }

    #endregion
  }

  /// <summary>
  /// ������ ���������, ������� ������ ������������ ���������� SELECT.
  /// ����� ��������� �������� ������ ���� ����� �������������� �������.
  /// </summary>
  [Serializable]
  public sealed class DBxNamedExpressionList : NamedList<DBxNamedExpression>
  {
    #region �������������� ������

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    /// <summary>
    /// �������� ��������� ��� �������� ������
    /// </summary>
    /// <param name="expression">������ DBxColumn. ���� �������� ������ ���������, ����� ��������� ����������. �� ����� ���� null.</param>
    public void Add(DBxExpression expression)
    {
      base.Add(new DBxNamedExpression(expression));
    }

    /// <summary>
    /// �������� ��������� � ��������� ������.
    /// </summary>
    /// <param name="expression">���������. �� ����� ���� null.</param>
    /// <param name="alias">�����. ������ ���� �����</param>
    public void Add(DBxExpression expression, string alias)
    {
      base.Add(new DBxNamedExpression(expression, alias));
    }

    /// <summary>
    /// ������� ������ ��������� DBxColumn � ��������� ��� � ������.
    /// ���� ��� <paramref name="columnNames"/> �������� �������, ��������� ��������� ��������.
    /// </summary>
    /// <param name="columnNames">��� ����. ����� ��������� ����� ��� ��������� ��������� �����. ������ ���� ������. ����� ��������� ������� ��� ������� ���������� �����</param>
    public void Add(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw new ArgumentNullException("columnNames");
      if (columnNames.IndexOf(',') >= 0)
      {
        string[] a = columnNames.Split(',');
        for (int i = 0; i < a.Length; i++)
          Add(new DBxColumn(a[i]));
      }
      else
        Add(new DBxColumn(columnNames));
    }


    /// <summary>
    /// ������� ������ ��������� DBxColumn � ��������� ��� � ������ � ��������� ������.
    /// ����� ������ ������ ���� ��� ����.
    /// </summary>
    /// <param name="columnNames">��� ����. ����� ��������� ����� ��� ��������� ��������� �����. ������ ���� ������. �� ����� ��������� �������</param>
    public void Add(string columnName, string alias)
    {
      Add(new DBxColumn(columnName), alias);
    }

    /// <summary>
    /// ������� ��������� ��������� DBxColumn � ��������� �� � ������.
    /// </summary>
    /// <param name="columnNames">������ ���� �����. ���� null, �� ������� �������� �� �����������</param>
    public void Add(DBxColumns columnNames)
    {
      if (columnNames == null)
        return;
      for (int i = 0; i < columnNames.Count; i++)
        this.Add(new DBxColumn(columnNames[i]));
    }

    /// <summary>
    /// �������� DBxExpression.GetColumnNames() ��� ���� ���������, �������� � ������.
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif
      for (int i = 0; i < Count; i++)
        this[i].Expression.GetColumnNames(list);
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� �������� SELECT (������ IDBxCon.FillSelect() � IDBxCon.ReaderSelect()
  /// </summary>
  [Serializable]
  public sealed class DBxSelectInfo
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������.
    /// ��� ������������� �������, ��� �������, ������ ���� ����������� �������� TableName.
    /// </summary>
    public DBxSelectInfo()
    {
      _Expressions = new DBxNamedExpressionList();
      // �������������� ��� ���������
      //_GroupBy = new List<DBxExpression>();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� �������� �������, ������������ ������� �������� ����.
    /// �������� ������ ���� �����������
    /// </summary>
    public string TableName
    {
      get { return _TableName; }
      set { _TableName = value; }
    }
    private string _TableName;

    /// <summary>
    /// ������ ��������� (������, ���� �����), ������� ��������� �������.
    /// ���� ������ ������, �� ����� ������� ��� ���� �������, � ������� � ������������ ���� ������.
    /// ���� ����������� �� ������ � ����� ���, �� ������ ����� ������������ "SELECT * FROM"
    /// </summary>
    public DBxNamedExpressionList Expressions { get { return _Expressions; } }
    private DBxNamedExpressionList _Expressions;

    /// <summary>
    /// ������ ��� ������� WHERE. ���� �� �����, �� ����� ���������� ��� ������ �������
    /// </summary>
    public DBxFilter Where
    {
      get { return _Where; }
      set { _Where = value; }
    }
    private DBxFilter _Where;

    /// <summary>
    /// ������ ��������� ��� GROUP BY.
    /// �� ��������� ������ ������
    /// </summary>
    public IList<DBxExpression> GroupBy
    {
      get
      {
        if (_GroupBy == null)
          _GroupBy = new List<DBxExpression>();
        return _GroupBy;
      }
    }
    private List<DBxExpression> _GroupBy;

    internal bool HasGroupBy
    {
      get
      {
        if (_GroupBy == null)
          return false; // ���� ����� ��� � ������
        else
          return _GroupBy.Count > 0;
      }
    }

    /// <summary>
    /// ��������� ������ GroupBy ����������� �� ������ ���������Expressions.
    /// ��������������, ��� ������ Expressions �������� � �������� ��� ���������� �������, ��� � ������� ���������
    /// </summary>
    public void InitGroupBy()
    {
      if (Expressions.Count == 0)
        throw new InvalidOperationException("�� ����� ������ ���������. ��� ��������� \"SELECT * FROM xxx\" �� ����� �������������� ��������� GROUP BY");

      if (GroupBy.Count > 0)
        throw new InvalidOperationException("������ GroupBy ��� ��������. ��������� ����� ������ �� �����������");

      for (int i = 0; i < Expressions.Count; i++)
      {
        bool HasColumn;
        bool HasAgregate;
        GetExpressionInfo(Expressions[i].Expression, out HasColumn, out HasAgregate);
        if (HasColumn && HasAgregate)
          throw new InvalidOperationException("��������� Expressions[" + i.ToString() + "] (" + Expressions[i].Expression.ToString() + ") �������� ������������ � ���������� ������� � ������ �� ���� �������");
        else if (HasColumn)
          GroupBy.Add(Expressions[i].Expression);
      }
    }

    private static void GetExpressionInfo(DBxExpression expression, out bool hasColumn, out bool hasAgregate)
    {
      hasColumn = false;
      hasAgregate = false;
      DoGetExpressionInfo(expression, ref hasColumn, ref hasAgregate);
    }

    private static void DoGetExpressionInfo(DBxExpression expression, ref bool hasColumn, ref bool hasAgregate)
    {
      if (expression is DBxAgregateFunction)
        hasAgregate = true;
      else if (expression is DBxColumn)
        hasColumn = true;
      else if (expression is DBxFunction)
      {
        DBxFunction f = (DBxFunction)expression;
        for (int i = 0; i < f.Arguments.Length; i++)
          DoGetExpressionInfo(f.Arguments[i], ref hasColumn, ref hasAgregate);
      }
    }

    /// <summary>
    /// ������ ��� ��������� HAVING.
    /// �� ��������� - null - ������ �� �����
    /// </summary>
    public DBxFilter Having { get { return _Having; } set { _Having = value; } }
    private DBxFilter _Having;

    /// <summary>
    /// ������� ���������� ORDER BY.
    /// ���� �� �����, �� ������� ������������ ����� �������������.
    /// </summary>
    public DBxOrder OrderBy
    {
      get { return _OrderBy; }
      set { _OrderBy = value; }
    }
    private DBxOrder _OrderBy;

    /// <summary>
    /// ���� ����������� ��������, �������� �� 0, �� ������������ �������� TOP n
    /// </summary>
    public int MaxRecordCount
    {
      get { return _MaxRecordCount; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _MaxRecordCount = value;
      }
    }
    private int _MaxRecordCount;

    /// <summary>
    /// ���� ���������� � true, �� ������������ �������� DISTINCT
    /// </summary>
    public bool Unique
    {
      get { return _Unique; }
      set { _Unique = value; }
    }
    private bool _Unique;

    #endregion

    #region ������

    /// <summary>
    /// �������� ������ ���� �����, ������������ � ����������, ��������, ������� ����������.
    /// ��������������, ��� ������ Expressions ��������, ����� ������������� ����������.
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif

      if (Expressions.Count == 0)
        throw new InvalidOperationException("������ ��������� Expressions �� ��������");

      Expressions.GetColumnNames(list);

      if (Where != null)
        Where.GetColumnNames(list);

      if (HasGroupBy)
      {
        for (int i = 0; i < GroupBy.Count; i++)
          GroupBy[i].GetColumnNames(list);
      }
      if (Having != null)
        Having.GetColumnNames(list);

      if (OrderBy != null)
      {
        // OrderBy.GetColumnNames(list);
        // 25.12.2019
        // � ���������� OrderBy ����� ���� ��� ������ �� ���� �������, ��� � ������ �� ������ Expressions
        // ������ �� ���� ��������� � ������ �����
        DBxColumnList list2 = new DBxColumnList();
        for (int i = 0; i < OrderBy.Parts.Length; i++)
        {
          list2.Clear();
          OrderBy.Parts[i].Expression.GetColumnNames(list2);
          for (int j = 0; j < list2.Count; j++)
          {
            if (!Expressions.Contains(list2[j])) // ��� ������ ������?
              list.Add(list2[j]);
          }
        }
      }
    }

    #endregion
  }
}
