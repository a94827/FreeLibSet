// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ������� ����� "���������", ������� ����� ���� ������� �����, ��������� ����� (���������� ����� JOIN), ����������, �������� ��� �������������� ����������.
  /// ��������� ������ ���������������.
  /// ������ ���� ������������� �������� Equals
  /// </summary>
  [Serializable]
  public abstract class DBxExpression
  {
    /// <summary>
    /// �������� ������ ������������ �����
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public abstract void GetColumnNames(DBxColumnList list);

    /// <summary>
    /// �������� �������� ��������� �� ������������� ��������� ������
    /// </summary>
    /// <param name="rowValues">�������� ������</param>
    /// <param name="nullAsDefaultValue">���� true, �� �������� null ������ ���� �������� �� �������� �� ���������</param>
    /// <returns>��������</returns>
    public abstract object GetValue(INamedValuesAccess rowValues, bool nullAsDefaultValue);

    /// <summary>
    /// ��������� ���� ��������� �� ���������.
    /// ���������� true, ���� ��������� ���������.
    /// </summary>
    /// <param name="a">������ ������������ ���������</param>
    /// <param name="b">������ ������������ ���������</param>
    /// <returns>��������� ��������</returns>
    public static bool operator ==(DBxExpression a, DBxExpression b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;
      return a.Equals(b);
    }

    /// <summary>
    /// ��������� ���� ��������� �� ���������.
    /// ���������� true, ���� ��������� ������.
    /// </summary>
    /// <param name="a">������ ������������ ���������</param>
    /// <param name="b">������ ������������ ���������</param>
    /// <returns>��������� ��������</returns>
    public static bool operator !=(DBxExpression a, DBxExpression b)
    {
      return !(a == b);
    }

    /// <summary>
    /// ���� ����� ������ ���� ����������� �������������.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public abstract override bool Equals(object obj);

    /// <summary>
    /// ���� ����� ������ ���� ����������� ������������� � ����������� ������.
    /// </summary>
    /// <returns>�������� �������� ��� ���������� � ��������� ���������</returns>
    public abstract override int GetHashCode();

    /// <summary>
    /// ���������� ���������, ���� ��������� �������� ����������.
    /// ��� ������� ��������� ������������ null.
    /// </summary>
    /// <returns>��������� ��� null</returns>
    public abstract DBxConst GetConst();

    /// <summary>
    /// ��������� ������� �� ���� �����, ������� ������ � ���������.
    /// ���� ����� ���������� ��� �������� ��������� �����, �� �������� �������� ����� � ����� ��������.
    /// </summary>
    /// <param name="prefix">����������� �������</param>
    /// <returns>����� ���������. ��� ��������� ������������ ������� ������</returns>
    public abstract DBxExpression SetColumnNamePrefix(string prefix);
  }

  /// <summary>
  /// ��������� - ����
  /// </summary>
  [Serializable]
  public sealed class DBxColumn : DBxExpression
  {
    #region �����������

    /// <summary>
    /// ������� ������ ��� �������� ��� ���������� ����.
    /// �� ����������� ������������ �����, ��� ��� ������ ������������ � ��� DataView, ��� ����������� �� ��� ����������� ��� 
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public DBxColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      if (columnName.IndexOf(',') >= 0)
        throw new ArgumentException("��� ���� �� ����� ��������� �������", "columnName");

      _ColumnName = columnName;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ��� ����
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private readonly string _ColumnName;
    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ��� ����
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return ColumnName;
    }

    /// <summary>
    /// ��������� � ������ �������� DBxColumn.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      DBxColumn obj2 = obj as DBxColumn;
      if (obj2 == null)
        return false;
      else
        return this._ColumnName == obj2.ColumnName;
    }

    /// <summary>
    /// ���-��������.
    /// </summary>
    /// <returns>�������� �������� ��� ���������� � ��������� ���������</returns>
    public override int GetHashCode()
    {
      return _ColumnName.GetHashCode();
    }

    /// <summary>
    /// �������� ������ ������������ �����.
    /// ��������� � ������ ��� ���� ColumnName.
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName);
    }

    /// <summary>
    /// �������� �������� ��������� �� ������������� ��������� ������.
    /// ��������� �������� ColumnName �� <paramref name="rowValues"/>.
    /// </summary>
    /// <param name="rowValues">�������� ������</param>
    /// <param name="nullAsDefaultValue">���� true, �� �������� null ������ ���� �������� �� �������� �� ���������</param>
    /// <returns>��������</returns>
    public override object GetValue(INamedValuesAccess rowValues, bool nullAsDefaultValue)
    {
      object v = rowValues.GetValue(ColumnName);
      if (v is DBNull)
        return null;
      else
        return v;
    }

    /// <summary>
    /// ���������� null
    /// </summary>
    /// <returns>null</returns>
    public override DBxConst GetConst()
    {
      return null;
    }

    /// <summary>
    /// ��������� ������� � ����� ����.
    /// </summary>
    /// <param name="prefix">����������� �������</param>
    /// <returns>����� ��������� DBxColumn</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      return new DBxColumn(prefix + ColumnName);
    }

    #endregion
  }

  /// <summary>
  /// ����������� ���������
  /// </summary>
  [Serializable]
  public sealed class DBxConst : DBxExpression
  {
    #region �����������

    /// <summary>
    /// ������� ��������� � �������� ���������.
    /// ������ ������������ �������� null ��� DBNull.
    /// ��� ������ ������������ �� ���������.
    /// </summary>
    /// <param name="value">��������</param>
    public DBxConst(object value)
    : this(value, DBxColumnType.Unknown)
    {
    }


    /// <summary>
    /// ������� ��������� � �������� ��������� � ����� ������.
    /// �������� DBNull ���������� �� null.
    /// </summary>
    /// <param name="value">��������</param>
    /// <param name="columnType">��� ������. ������ ���� ����������� �����, ���� <paramref name="value"/>=null ��� DBNull</param>
    public DBxConst(object value, DBxColumnType columnType)
    {
      if (columnType == DBxColumnType.Unknown)
      {
        if (value == null || value is DBNull)
          throw new ArgumentNullException("value", "����������� �����������, ����������� 2 ���������");
        _Value = value;

        _ColumnType = DBxTools.ValueToColumnType(value);
        if (_ColumnType == DBxColumnType.Unknown)
          throw new ArgumentException("�� ������� ���������� ��� ������� �� ���� " + value.GetType().ToString(), "value");
      }
      else
      {
        if (value is DBNull)
          _Value = null; // DBNull �������� �� null
        else
          _Value = value;
        _ColumnType = columnType;
      }
      CheckValueType(_Value);
    }

    private static void CheckValueType(object value)
    {
      if (value == null)
        return;

      if (DataTools.IsNumericType(value.GetType()))
        return;
      if (value is String)
        return;
      if (value is Boolean)
        return;
      if (value is DateTime)
        return;
      if (value is TimeSpan)
        return;
      if (value is Guid)
        return;

      throw new ArgumentException("�������� ��������� ����� ������������ ���: " + value.GetType().ToString(), "value");
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ���������.
    /// �������� DBNull �������� �� null.
    /// </summary>
    public object Value { get { return _Value; } }
    private readonly object _Value;

    /// <summary>
    /// ��� ������ ��� ���������.
    /// ����� ��� ������������� � ������� ISNULL.
    /// ������ ���������, �� ����� ���� Unknown.
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private readonly DBxColumnType _ColumnType;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ������������� � ���� "�������� (�������)" (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      string s;
      if (_Value == null)
        s = "NULL";
      else
      {
        DBxSqlBuffer buffer = new DBxSqlBuffer();
        buffer.FormatExpression(this, new DBxFormatExpressionInfo());
        s = buffer.SB.ToString();
      }

      return s + " (" + ColumnType.ToString() + ")";
    }

    /// <summary>
    /// ��������� � ������ �������� DBxConst.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      DBxConst obj2 = obj as DBxConst;
      if (obj2 == null)
        return false;
      else
        return Object.Equals(this._Value, obj2._Value);
    }

    /// <summary>
    /// ���-��������.
    /// </summary>
    /// <returns>�������� �������� ��� ���������� � ��������� ���������</returns>
    public override int GetHashCode()
    {
      if (_Value == null)
        return 0;
      else
        return _Value.GetHashCode();
    }

    /// <summary>
    /// �������� ������ ������������ �����
    /// ������ �� �����������
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
    }

    /// <summary>
    /// �������� �������� ��������� �� ������������� ��������� ������.
    /// ���������� �������� ��������� ��� ������ �������� ��� Value=null � <paramref name="nullAsDefaultValue"/>=true.
    /// </summary>
    /// <param name="rowValues">������������</param>
    /// <param name="nullAsDefaultValue">���� true, �� �������� null ������ ���� �������� �� �������� �� ���������</param>
    /// <returns>��������</returns>
    public override object GetValue(INamedValuesAccess rowValues, bool nullAsDefaultValue)
    {
      if (Value == null && nullAsDefaultValue)
        return DBxTools.GetDefaultValue(ColumnType);
      else
        return Value;
    }

    /// <summary>
    /// ���������� ������� ������
    /// </summary>
    /// <returns>���������</returns>
    public override DBxConst GetConst()
    {
      return this;
    }

    /// <summary>
    /// ���������� ������� ������ ��� ���������
    /// </summary>
    /// <param name="prefix">������������</param>
    /// <returns>������� ������</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      return this; // �� ��������� ���������
    }

    #endregion
  }

  /// <summary>
  /// ������ ������� � ����������
  /// </summary>
  [Serializable]
  public enum DBxFunctionKind
  {
    #region ������������� - ��������

    /// <summary>
    /// �������� "+"
    /// </summary>
    Add,

    /// <summary>
    /// �������� "-"
    /// </summary>
    Substract,

    /// <summary>
    /// �������� "*"
    /// </summary>
    Multiply,

    /// <summary>
    /// �������� "/"
    /// </summary>
    Divide,

    /// <summary>
    /// ������� �����
    /// </summary>
    Neg,

    #endregion

    #region �������

    /// <summary>
    /// ������ �����
    /// </summary>
    Abs,

    /// <summary>
    /// ������ NULL.
    /// ��� ��������� ��� ������, � ��� ����� MS SQL Server, ������������ ������� ISNULL
    /// </summary>
    Coalesce,

    /// <summary>
    /// ���������� ����� ������
    /// </summary>
    Length,

    /// <summary>
    /// ������� ������ � ������ �������
    /// </summary>
    Lower,

    /// <summary>
    /// ������� ������ � ������� �������
    /// </summary>
    Upper,

    /// <summary>
    /// ���������� ���������
    /// ������ �������� - �������� ��������� ���������
    /// ������ �������� - ��������� ������� ���������. ��������� � �������, � �� � ����!
    /// ������ �������� - ����� ������ � ��������
    /// </summary>
    Substring,

    /// <summary>
    /// ����������� �������� �� ������ ����������
    /// </summary>
    Min,

    /// <summary>
    /// ������������ �������� �� ������ ����������
    /// </summary>
    Max,

    #endregion
  }

  /// <summary>
  /// ������� (����� ����������) ��� �������������� ��������
  /// </summary>
  [Serializable]
  public sealed class DBxFunction : DBxExpression
  {
    #region �����������

    /// <summary>
    /// ������� ������ �������, ����������� ������ ���������� DBxExpression
    /// </summary>
    /// <param name="function">�������</param>
    /// <param name="args">��������� ������� - ������ ���������</param>
    public DBxFunction(DBxFunctionKind function, params DBxExpression[] args)
    {
      if (args == null)
        throw new ArgumentNullException("args");
      int MinArgCount, MaxArgCount;
      GetArgCount(function, out MinArgCount, out MaxArgCount);
      if (args.Length < MinArgCount || args.Length > MaxArgCount)
        throw new ArgumentException("������������ ���������� ����������: " + args.Length.ToString() + ". ��� ������� " + function.ToString() + " ��������� ��������� � ���������� �� " + MinArgCount.ToString() + " �� " + MaxArgCount.ToString());
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == null)
          throw new ArgumentNullException("args[" + i.ToString() + "]");
      }

      _Function = function;
      _Arguments = args;
    }


    /// <summary>
    /// ������� ������ �������, ����������� ������ ���������� - ���� �����.
    /// ������ ��� ������� ������� ���� "UPPER()"
    /// </summary>
    /// <param name="function">�������</param>
    /// <param name="columnNames">��������� ������� - ����� �����, ��� ������� ��������� DBxColumn</param>
    public DBxFunction(DBxFunctionKind function, params string[] columnNames)
      : this(function, DBxTools.GetColumnNameExpressions(columnNames))
    {
    }

    #endregion

    #region �������� ����������

    /// <summary>
    /// ���������� ���������� ����������, ������� ����� ����� �������
    /// </summary>
    /// <param name="function">�������</param>
    /// <param name="minArgCount">��������� - ����������� ���������� ����������</param>
    /// <param name="maxArgCount">��������� - ������������ ���������� ����������</param>
    public static void GetArgCount(DBxFunctionKind function, out int minArgCount, out int maxArgCount)
    {
      switch (function)
      {
        #region ������������� - ��������

        case DBxFunctionKind.Add:
        case DBxFunctionKind.Substract:
        case DBxFunctionKind.Multiply:
        case DBxFunctionKind.Divide:
          minArgCount = 2;
          maxArgCount = 2;
          break;

        case DBxFunctionKind.Neg:
          minArgCount = 1;
          maxArgCount = 1;
          break;

        #endregion

        #region ������� �������

        case DBxFunctionKind.Abs:
          minArgCount = 1;
          maxArgCount = 1;
          break;

        case DBxFunctionKind.Coalesce:
          minArgCount = 2;
          maxArgCount = int.MaxValue;
          break;

        case DBxFunctionKind.Length:
        case DBxFunctionKind.Lower:
        case DBxFunctionKind.Upper:
          minArgCount = 1;
          maxArgCount = 1;
          break;

        #endregion

        #region ���������� �������

        case DBxFunctionKind.Min:
        case DBxFunctionKind.Max:
          //        case DBxFunctionKind.Avg:
          //        case DBxFunctionKind.Sum:
          minArgCount = 1;
          maxArgCount = int.MaxValue;
          break;


        #endregion

        default:
          throw new ArgumentException("����������� ������� " + function.ToString());
      }
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� �������
    /// </summary>
    public DBxFunctionKind Function { get { return _Function; } }
    private readonly DBxFunctionKind _Function;

    /// <summary>
    /// ��������� ������� - ������ ���������
    /// </summary>
    public DBxExpression[] Arguments { get { return _Arguments; } }
    private readonly DBxExpression[] _Arguments;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ������������ ��� �������
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_Function.ToString());
      sb.Append("(");
      for (int i = 0; i < _Arguments.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append(_Arguments[i].ToString());
      }
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// ��������� � ������ �������� DBxFunction.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      DBxFunction obj2 = obj as DBxFunction;
      if (obj2 == null)
        return false;
      if (this._Function != obj2._Function)
        return false;
      if (this._Arguments.Length != obj2._Arguments.Length)
        return false;
      for (int i = 0; i < _Arguments.Length; i++)
      {
        if (this._Arguments[i] != obj2._Arguments[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// ���-��������.
    /// </summary>
    /// <returns>�������� �������� ��� ���������� � ��������� ���������</returns>
    public override int GetHashCode()
    {
      return (int)_Function;
    }

    /// <summary>
    /// �������� ������ ������������ �����.
    /// ���������� �������� ����� ��� ���� ����������
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Arguments.Length; i++)
        _Arguments[i].GetColumnNames(list);
    }

    /// <summary>
    /// �������� �������� ��������� �� ������������� ��������� ������.
    /// ��������� ����������� ����� ������ ��� ���� ����������. ����� ����������� ���������� �������.
    /// </summary>
    /// <param name="rowValues">�������� ������</param>
    /// <param name="nullAsDefaultValue">���� true, �� �������� null ������ ���� �������� �� �������� �� ���������</param>
    /// <returns>��������</returns>
    public override object GetValue(INamedValuesAccess rowValues, bool nullAsDefaultValue)
    {
      object[] a = new object[_Arguments.Length];
      for (int i = 0; i < a.Length; i++)
        a[i] = _Arguments[i].GetValue(rowValues, nullAsDefaultValue);

      switch (_Function)
      {
        case DBxFunctionKind.Add:
          return DataTools.SumValues(a[0], a[1]);
        case DBxFunctionKind.Substract:
          return DataTools.SubstractValues(a[0], a[1]);
        case DBxFunctionKind.Multiply:
          return DataTools.MultiplyValues(a[0], a[1]);
        case DBxFunctionKind.Divide:
          return DataTools.DivideValues(a[0], a[1]);
        case DBxFunctionKind.Neg:
          return DataTools.NegValue(a[0]);
        case DBxFunctionKind.Abs:
          return DataTools.AbsValue(a[0]);
        case DBxFunctionKind.Coalesce:
          for (int i = 0; i < a.Length; i++)
          {
            if (a[i] != null)
              return a[i];
          }
          return null;
        case DBxFunctionKind.Length:
          return DataTools.GetString(a[0]).Length;
        case DBxFunctionKind.Lower:
          return DataTools.GetString(a[0]).ToLowerInvariant();
        case DBxFunctionKind.Upper:
          return DataTools.GetString(a[0]).ToUpperInvariant();
        case DBxFunctionKind.Min:
          return DataTools.MinValue(a);
        case DBxFunctionKind.Max:
          return DataTools.MaxValue(a);
        //case DBxFunctionKind.Avg:
        //  return DataTools.AverageValue(a);
        //case DBxFunctionKind.Count:
        //  throw new NotSupportedException();
        //case DBxFunctionKind.Sum:
        //  return DataTools.SumValue(a);
        default:
          throw new BugException("����������� ������� " + _Function.ToString());
      }
    }

    /// <summary>
    /// � ������� ���������� ���������� null.
    /// ���������� ��������� �������� �� �����������
    /// </summary>
    /// <returns>null</returns>
    public override DBxConst GetConst()
    {
      // TODO:
      // ���� ��� ��������� - ���������, �� ����� ��������� � ������� ����� DBxConst
      return null;
    }

    /// <summary>
    /// ��������� ������� �� ���� �����, ������� ������ � ���������.
    /// ���������� �������� ����� ��� ���� ����������.
    /// ����� ��������� ����� ������ DBxFunction.
    /// </summary>
    /// <param name="prefix">����������� �������</param>
    /// <returns>����� ���������</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      bool HasDiff = false;
      DBxExpression[] args2 = new DBxExpression[Arguments.Length];
      for (int i = 0; i < Arguments.Length; i++)
      {
        args2[i] = Arguments[i].SetColumnNamePrefix(prefix);
        if (!object.ReferenceEquals(args2[i], Arguments[i]))
          HasDiff = true;
      }

      if (HasDiff)
        return new DBxFunction(Function, args2);
      else
        return this; // ������� ������� �� �����
    }

    #endregion
  }


  /// <summary>
  /// ������ ������� � ����������
  /// </summary>
  [Serializable]
  public enum DBxAgregateFunctionKind
  {
    #region ���������� �������

    /// <summary>
    /// ����������� ��������
    /// </summary>
    Min,

    /// <summary>
    /// ������������ ��������
    /// </summary>
    Max,

    /// <summary>
    /// ������� ��������
    /// </summary>
    Avg,

    /// <summary>
    /// ���������� �����
    /// </summary>
    Count,

    /// <summary>
    /// ��������� ��������
    /// </summary>
    Sum,

    #endregion
  }

  /// <summary>
  /// ���������� ������� 
  /// </summary>
  [Serializable]
  public sealed class DBxAgregateFunction : DBxExpression
  {
    #region �����������

    /// <summary>
    /// ������� ������ �������, ����������� ���� �������� DBxExpression ��� null (������ ��� COUNT(*) )
    /// </summary>
    /// <param name="function">�������</param>
    /// <param name="arg">�������� ������� - ������ ���������, ������, ��� ����. ����� ���� null ��� COUNT</param>
    public DBxAgregateFunction(DBxAgregateFunctionKind function, DBxExpression arg)
    {
      if (arg == null && function != DBxAgregateFunctionKind.Count)
        throw new ArgumentNullException("arg", "�������� (������, ��� ����) ������ ���� ����� ��� ���� ���������� �������, ����� COUNT(*)");

      _Function = function;
      _Argument = arg;
    }


    /// <summary>
    /// ������� ������ �������, ����������� ������ ���������� - ���� �����.
    /// ������ ��� ������� ������� ���� "UPPER()"
    /// </summary>
    /// <param name="function">�������</param>
    /// <param name="columnName">��������� ������� - ����� �����, ��� ������� ��������� DBxColumn</param>
    public DBxAgregateFunction(DBxAgregateFunctionKind function, string columnName)
      : this(function, String.IsNullOrEmpty(columnName) ? (DBxExpression)null : (DBxExpression)(new DBxColumn(columnName)))
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� �������
    /// </summary>
    public DBxAgregateFunctionKind Function { get { return _Function; } }
    private readonly DBxAgregateFunctionKind _Function;

    /// <summary>
    /// ��������� ������� - ������ ���������
    /// </summary>
    public DBxExpression Argument { get { return _Argument; } }
    private readonly DBxExpression _Argument;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ������������� ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_Function.ToString());
      sb.Append("(");
      if (_Argument == null)
        sb.Append("*");
      else
        sb.Append(_Argument.ToString());
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// ��������� � ������ �������� DBxFunction.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      DBxAgregateFunction obj2 = obj as DBxAgregateFunction;
      if (obj2 == null)
        return false;
      if (this._Function != obj2._Function)
        return false;
      if (this._Argument != obj2._Argument)
        return false;
      return true;
    }

    /// <summary>
    /// ���-��������.
    /// </summary>
    /// <returns>�������� �������� ��� ���������� � ��������� ���������</returns>
    public override int GetHashCode()
    {
      return (int)_Function;
    }

    /// <summary>
    /// �������� ������ ������������ �����.
    /// ���������� �������� ����� ��� ���� ����������
    /// </summary>
    /// <param name="list">����������� ������. �� ����� ���� null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      if (_Argument != null)
        _Argument.GetColumnNames(list);
    }

    /// <summary>
    /// ����������� ����������
    /// </summary>
    /// <param name="rowValues">������������</param>
    /// <param name="nullAsDefaultValue">������������</param>
    /// <returns>��������</returns>
    public override object GetValue(INamedValuesAccess rowValues, bool nullAsDefaultValue)
    {
      throw new NotSupportedException("��� ���������� ������� ���������� �� ��������� ������ ������ ����������");
    }

    /// <summary>
    /// � ������� ���������� ���������� null.
    /// ���������� ��������� �������� �� �����������
    /// </summary>
    /// <returns>null</returns>
    public override DBxConst GetConst()
    {
      return null;
    }

    /// <summary>
    /// ��������� ������� �� ���� �����, ������� ������ � ���������.
    /// ���������� �������� ����� ��� ���� ����������.
    /// ����� ��������� ����� ������ DBxFunction.
    /// </summary>
    /// <param name="prefix">����������� �������</param>
    /// <returns>����� ���������</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      if (_Argument == null)
        return this; // ������� COUNT(*)

      DBxExpression arg2 = _Argument.SetColumnNamePrefix(prefix);
      if (Object.ReferenceEquals(arg2, _Argument))
        return this; // ������� ������� �� �����
      else
        return new DBxAgregateFunction(Function, arg2);
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������.
  /// ����� �� ������������ � ���������� ����.
  /// </summary>
  [Serializable]
  public sealed class DBxExpressions : IList<DBxExpression>
  {
    #region ������������

    /// <summary>
    /// ������� ������ ��������� �� �������
    /// </summary>
    /// <param name="items">������ ���������.</param>
    public DBxExpressions(DBxExpression[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      for (int i = 0; i < items.Length; i++)
      {
        if (items[i] == null)
          throw new ArgumentNullException("items[" + i.ToString() + "]");
        _Items = items;
      }
    }

    /// <summary>
    /// ������� ������ ��������� DBxColumn � DBxRefColumn �� ������ ��������.
    /// ��������� ���� ������������ �� ������� ����������� - �����
    /// </summary>
    /// <param name="columns">����� ��������</param>
    public DBxExpressions(DBxColumns columns)
    {
      _Items = new DBxExpression[columns.Count];
      for (int i = 0; i < _Items.Length; i++)
        _Items[i] = new DBxColumn(columns[i]);
    }

    /// <summary>
    /// ������� ������ ��������� DBxColumn � DBxRefColumn �� ������ ��������.
    /// ��������� ���� ������������ �� ������� ����������� - �����
    /// </summary>
    /// <param name="columns">����� ��������</param>
    public DBxExpressions(DBxColumnList columns)
    {
      _Items = new DBxExpression[columns.Count];
      for (int i = 0; i < _Items.Length; i++)
        _Items[i] = new DBxColumn(columns[i]);
    }

    /// <summary>
    /// ������� ������ ��������� DBxColumn �� ������ ��������.
    /// ��������� ���� ������������ �� ������� ����������� - �����.
    /// ���� <paramref name="columns"/>=null, ������������ null.
    /// </summary>
    /// <param name="columns">������ ���� ����� ��� null</param>
    /// <returns>������ DBxExpressions ��� null</returns>
    public static DBxExpressions FromColumns(DBxColumns columns)
    {
      if (columns == null)
        return null;
      else
        return new DBxExpressions(columns);
    }

    /// <summary>
    /// ������� ������ ��������� DBxColumn �� ������ ��������.
    /// ��������� ���� ������������ �� ������� ����������� - �����.
    /// ���� <paramref name="columns"/>=null, ������������ null.
    /// </summary>
    /// <param name="columns">������ ���� ����� ��� null</param>
    /// <returns>������ DBxExpressions ��� null</returns>
    public static DBxExpressions FromColumns(DBxColumnList columns)
    {
      if (columns == null)
        return null;
      else
        return new DBxExpressions(columns);
    }

    #endregion

    #region ������ � ��������� ����������

    private DBxExpression[] _Items;

    /// <summary>
    /// ���������� ���������� ��������� � ������
    /// </summary>
    public int Count { get { return _Items.Length; } }

    /// <summary>
    /// ���������� ��������� �� �������
    /// </summary>
    /// <param name="index">������ ���������</param>
    /// <returns>���������</returns>
    public DBxExpression this[int index]
    {
      get { return _Items[index]; }
    }

    /// <summary>
    /// ���������� ������������� ���������.
    /// 
    /// ��� ������������� �������� (ArrayEnumerator) ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns></returns>
    public ArrayEnumerator<DBxExpression> GetEnumerator()
    {
      return new ArrayEnumerator<DBxExpression>(_Items);
    }

    IEnumerator<DBxExpression> IEnumerable<DBxExpression>.GetEnumerator()
    {
      return new ArrayEnumerator<DBxExpression>(_Items);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<DBxExpression>(_Items);
    }

    #endregion

    #region IList<DBxExpression> Members

    int IList<DBxExpression>.IndexOf(DBxExpression item)
    {
      return Array.IndexOf<DBxExpression>(_Items, item);
    }


    void IList<DBxExpression>.Insert(int index, DBxExpression item)
    {
      throw new NotSupportedException();
    }

    void IList<DBxExpression>.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    DBxExpression IList<DBxExpression>.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    #region ICollection<DBxExpression> Members

    void ICollection<DBxExpression>.Add(DBxExpression item)
    {
      throw new NotSupportedException();
    }

    void ICollection<DBxExpression>.Clear()
    {
      throw new NotSupportedException();
    }

    bool ICollection<DBxExpression>.Contains(DBxExpression item)
    {
      return Array.IndexOf<DBxExpression>(_Items, item) >= 0;
    }

    void ICollection<DBxExpression>.CopyTo(DBxExpression[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    bool ICollection<DBxExpression>.IsReadOnly
    {
      get { return true; }
    }

    bool ICollection<DBxExpression>.Remove(DBxExpression item)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ��������� � ������ <paramref name="list"/> ��� ����� �����, �������� � ���������
    /// </summary>
    /// <param name="list">����������� ������</param>
    public void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Items.Length; i++)
        _Items[i].GetColumnNames(list);
    }

    #endregion
  }
}
