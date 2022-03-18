// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ���� ����� ������������ � �������� �������� ������ ��� ������������� �������������� ��� ������ � ��� DataViewDBxSqlFormatter
  /// </summary>                                         
  public abstract class BaseDBxSqlFormatter : DBxSqlFormatter
  {
    #region ����� ������ � �����

    /// <summary>
    /// ������ ������������ ���� ������, ������� � �����.
    /// ������ ������������ �������, �� ��� DataView, MS SQL Server � Access ������ �������������� ���������� ������
    /// </summary>
    protected enum EnvelopMode
    {
      /// <summary>
      /// �������� ������� - ����� ����������� � ������� �������
      /// </summary>
      Quotation,

      /// <summary>
      /// ��� ��� ������ Microsoft - ���������� ������
      /// </summary>
      Brackets,

      /// <summary>
      /// ����� �� ������ ��������������
      /// </summary>
      None,

      /// <summary>
      /// ��� ������ ����������� ����������� ������� ��������������.
      /// BaseDBxSqlFormatter �������� ����������
      /// </summary>
      Unsupported
    }

    /// <summary>
    /// ������� ���������� ���� ������, ����� � ��������.
    /// �� ���������, ����� ����������� � �������
    /// </summary>
    protected virtual EnvelopMode NameEnvelopMode { get { return EnvelopMode.Quotation; } }

    /// <summary>
    /// �������������� ����� �������.
    /// ������������������ ����� ��������� ��� � ������� ��� ������, � ����������� �� �������� NameEnvelopMode
    /// </summary>
    /// <param name="buffer">����� ��� ������������ SQL-�������</param>
    /// <param name="tableName">��� �������</param>
    protected override void OnFormatTableName(DBxSqlBuffer buffer, string tableName)
    {
      DoFormatName(buffer, tableName);
    }

    /// <summary>
    /// �������������� ����� ����.
    /// ������������������ ����� ��������� ��� � ������� ��� ������, � ����������� �� �������� NameEnvelopMode
    /// </summary>
    /// <param name="buffer">����� ��� ������������ SQL-�������</param>
    /// <param name="columnName">��� �������</param>
    protected override void OnFormatColumnName(DBxSqlBuffer buffer, string columnName)
    {
      DoFormatName(buffer, columnName);
    }

    private void DoFormatName(DBxSqlBuffer buffer, string name)
    {
      switch (NameEnvelopMode)
      {
        case EnvelopMode.Quotation:
          buffer.SB.Append("\"");
          buffer.SB.Append(name);
          buffer.SB.Append("\"");
          break;
        case EnvelopMode.Brackets:
          buffer.SB.Append("[");
          buffer.SB.Append(name);
          buffer.SB.Append("]");
          break;
        case EnvelopMode.None:
          buffer.SB.Append(name);
          break;
        case EnvelopMode.Unsupported:
          throw new NotSupportedException("����� ������ ���� ���������� � ����������� ������");
        default:
          throw new BugException("������������ �������� �������� NameEnvelopMode=" + NameEnvelopMode.ToString());
      }
    }


    /// <summary>
    /// �������������� ����� ���� � ������ ������� ��� �������� SELECT c ������������ JOIN.
    /// ������������������ ����� �������� OnFormatTableName() ��� �������������� ������, ��������� ����� � 
    /// ����������� ��� ���� ������� OnFormatColumnName()
    /// </summary>
    /// <param name="buffer">����� ��� ������������ SQL-�������</param>
    /// <param name="tableAlias">����� ������� (�� �����)</param>
    /// <param name="columnName">��� ������� (����� �����)</param>
    protected override void OnFormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      OnFormatTableName(buffer, tableAlias);
      buffer.SB.Append('.');
      OnFormatColumnName(buffer, columnName);
    }

    #endregion

    #region ���� ������

    /// <summary>
    /// �������������� ���� ���� ��� ���������� CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// ����������� ������ ��� ������, ��������, "CHAR(20)".
    /// ��� ������� � ��������� NULL/NOT NULL �� �����������.
    /// </summary>
    /// <param name="buffer">����� ��� �������� SQL-�������</param>
    /// <param name="column">�������� �������</param>
    protected override void OnFormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        #region ������

        case DBxColumnType.String:
          buffer.SB.Append("CHAR(");
          buffer.SB.Append(column.MaxLength);
          buffer.SB.Append(")");
          break;

        #endregion

        #region ����������

        case DBxColumnType.Boolean:
          buffer.SB.Append("BOOLEAN");
          break;

        #endregion

        #region �����

        case DBxColumnType.Int:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("INTEGER"); // �������� ���
          else if (column.MinValue >= Int16.MinValue && column.MaxValue <= Int16.MaxValue)
            buffer.SB.Append("SMALLINT");
          else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
            buffer.SB.Append("INTEGER");
          else
            buffer.SB.Append("BIGINT");
          break;

        case DBxColumnType.Float:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("DOUBLE PRECISION");
          else if (column.MinValue >= Single.MinValue && column.MaxValue <= Single.MaxValue)
            buffer.SB.Append("REAL");
          else
            buffer.SB.Append("DOUBLE PRECISION");
          break;

        case DBxColumnType.Money: // ���������� ��������� ���� ���
          buffer.SB.Append("NUMERIC(18,2)");
          break;

        #endregion

        #region ���� / �����

        case DBxColumnType.Date: // ������ ����
          buffer.SB.Append("DATE");
          break;

        case DBxColumnType.DateTime: // ���� � �����
          buffer.SB.Append("TIMESTAMP");
          break;

        case DBxColumnType.Time:
          buffer.SB.Append("TIME");
          break;

        #endregion

        #region GUID

        case DBxColumnType.Guid:
          buffer.SB.Append("CHAR(36)"); // 32 ������� + 4 ����������� "-"
          break;

        #endregion

        #region MEMO

        case DBxColumnType.Memo:
          buffer.SB.Append("CLOB"); // ���������, ��� ���-������ ������������?
          break;

        case DBxColumnType.Xml:
          buffer.SB.Append("XML");
          break;

        case DBxColumnType.Binary:
          buffer.SB.Append("BLOB");
          break;

        #endregion

        default:
          throw new BugException("����������� ��� ���� " + column.ColumnType.ToString());
      }
    }

    #endregion

    #region ���������

    /// <summary>
    /// �������������� ����� ���������, ������������� �������� �������.
    /// ���� ������� ������������ �������� NULL � ����������� �������� DBxFormatExpressionInfo.NullAsDefaultValue,
    /// �� ���������� �������������� ������� COALESCE().
    /// ����� � ������ ����������� ��� ���� � ������� OnFormatColumnName(). ����� ���� ����������� ����� ������ ������� � ������ DBxSqlBuffer.ColumnTableAliases.
    /// ��� �������������, ����� ������ ������� ��������� ����� �������.
    /// </summary>
    /// <param name="buffer">����� ��� �������� SQL-�������</param>
    /// <param name="column">��������� - ��� ����</param>
    /// <param name="formatInfo">��������� ��������������</param>
    protected override void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    {
      string tableAlias;
      buffer.ColumnTableAliases.TryGetValue(column.ColumnName, out tableAlias);

      string actualName = column.ColumnName;
      int lastDotPos = column.ColumnName.LastIndexOf('.');
      if (lastDotPos >= 0)
      {
        actualName = actualName.Substring(lastDotPos + 1);
        if (String.IsNullOrEmpty(tableAlias))
          throw new InvalidOperationException("��� ���������� ������� \"" + column.ColumnName + "\" �� ������ ����� �������");
      }

      bool useCoalesce = false;
      DBxColumnType wantedType = formatInfo.WantedColumnType;
      if (formatInfo.NullAsDefaultValue)
      {
        DBxColumnStruct colStr;
        buffer.ColumnStructs.TryGetValue(column.ColumnName, out colStr);
        if (colStr != null)
        {
          useCoalesce = colStr.Nullable;
          wantedType = colStr.ColumnType;
        }
        else
          useCoalesce = true;
      }

      if (useCoalesce)
      {
        if (wantedType == DBxColumnType.Unknown)
          throw new InvalidOperationException("��� ������� \"" + column.ColumnName + "\" ��������� ��������� �������� NULL. �� ������� �������� ��������� ������� � �� ������� ��������� ��� ������");

        DBxFunction f2 = new DBxFunction(DBxFunctionKind.Coalesce, column, new DBxConst(DBxTools.GetDefaultValue(wantedType), wantedType));
        OnFormatExpression(buffer, f2, new DBxFormatExpressionInfo()); // ����������� ����� ��������������, �� ��� ��� ����� 
      }
      else
      {
        if (String.IsNullOrEmpty(tableAlias))
          OnFormatColumnName(buffer, actualName);
        else
          OnFormatColumnName(buffer, tableAlias, actualName);
      }
    }

    /// <summary>
    /// �������������� ���������-������� ��� �������������� ��������.
    /// ��� �������������� ���������� ������������ ����������� ����� FormatExpression()
    /// </summary>
    /// <param name="buffer">����� ��� �������� SQL-�������</param>
    /// <param name="function">��������� - �������</param>
    /// <param name="formatInfo">��������� ��������������</param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      switch (function.Function)
      {
        #region �������������� ��������

        case DBxFunctionKind.Add:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;

          formatInfo.NoParentheses = true;
          FormatExpression(buffer, function.Arguments[0], formatInfo);
          buffer.SB.Append("+");
          FormatExpression(buffer, function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Substract:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;

          formatInfo.NoParentheses = true;
          FormatExpression(buffer, function.Arguments[0], formatInfo);
          buffer.SB.Append("-");
          formatInfo.NoParentheses = false;
          FormatExpression(buffer, function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Multiply:
          formatInfo.NoParentheses = false;
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          FormatExpression(buffer, function.Arguments[0], formatInfo);
          buffer.SB.Append("*");
          FormatExpression(buffer, function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Divide:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          formatInfo.NoParentheses = false;
          FormatExpression(buffer, function.Arguments[0], formatInfo);
          buffer.SB.Append("/");
          FormatExpression(buffer, function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Neg:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          formatInfo.NoParentheses = false;
          buffer.SB.Append("-");
          FormatExpression(buffer, function.Arguments[0], formatInfo);
          break;

        #endregion

        #region �������

        case DBxFunctionKind.Abs:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Coalesce:
          // ���������� ��� ������ �� ���������
          for (int i = function.Arguments.Length - 1; i >= 1; i--)
          {
            DBxConst constExpr = function.Arguments[i].GetConst();
            if (constExpr != null)
            {
              formatInfo.WantedColumnType = constExpr.ColumnType; // �������������� ���������� ���
              break;
            }
          }
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Length:
        case DBxFunctionKind.Lower:
        case DBxFunctionKind.Upper:
          formatInfo.WantedColumnType = DBxColumnType.String;
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Substring:
          // ������ �������� - ������, ������ � ������ - �����
          buffer.SB.Append(GetFunctionName(function.Function));
          buffer.SB.Append('(');
          formatInfo.NoParentheses = true;
          formatInfo.WantedColumnType = DBxColumnType.String;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append(',');
          formatInfo.WantedColumnType = DBxColumnType.Int;
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          buffer.SB.Append(',');
          formatInfo.WantedColumnType = DBxColumnType.Int;
          buffer.FormatExpression(function.Arguments[2], formatInfo);
          buffer.SB.Append(')');
          break;

        #endregion

        default:
          throw new BugException("����������� ������� " + function.Function.ToString());
      }
    }

    private void DoFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      formatInfo.NoParentheses = true;

      buffer.SB.Append(GetFunctionName(function.Function));
      buffer.SB.Append('(');
      for (int i = 0; i < function.Arguments.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(',');
        FormatExpression(buffer, function.Arguments[i], formatInfo);
      }
      buffer.SB.Append(')');
    }

    /// <summary>
    /// ���������� true, ���� ��������� <paramref name="expression"/> ��������� ��������� � ������� ������ ��� ����������� ������� ����������.
    /// ���������� true ��� ���������-�������������� ��������.
    /// </summary>
    /// <param name="expression">���������, �������, ����� ����, ����� ��������� � ������</param>
    /// <returns>true, ���� ������ ���������</returns>
    protected override bool OnAreParenthesesRequired(DBxExpression expression)
    {
      DBxFunction f = expression as DBxFunction;
      if (f != null)
      {
        switch (f.Function)
        {
          case DBxFunctionKind.Add:
          case DBxFunctionKind.Substract:
          case DBxFunctionKind.Multiply:
          case DBxFunctionKind.Divide:
          case DBxFunctionKind.Neg:
            return true;
          default:
            return false;
        }
      }
      else
        return false;
    }

    /// <summary>
    /// ���������� ��� �������.
    /// ��� ��������� ����������� ������� ���������� ������������� �������.
    /// ��������, ��� MS SQL Server � DataView, ������� COALSECE() ���������� ISNULL(), ���� ������ �� �� �����.
    /// ���� ���� ��������� ������� � ������� �����������, �� ��������� ��������������� ������ OnFormatFunction()
    /// </summary>
    /// <param name="function">�������</param>
    protected virtual string GetFunctionName(DBxFunctionKind function)
    {
      switch (function)
      {
        case DBxFunctionKind.Abs: return "ABS";
        case DBxFunctionKind.Coalesce: return "COALESCE";
        case DBxFunctionKind.Length: return "LEN";
        case DBxFunctionKind.Lower: return "LOWER";
        case DBxFunctionKind.Upper: return "UPPER";
        case DBxFunctionKind.Substring: return "SUBSTRING";
        default:
          throw new ArgumentException("����������� ������� " + function.ToString(), "function");
      }
    }

    /// <summary>
    /// �������������� ���������-������� ��� �������������� ��������.
    /// ��� �������������� ���������� ������������ ����������� ����� FormatExpression()
    /// </summary>
    /// <param name="buffer">����� ��� �������� SQL-�������</param>
    /// <param name="function">��������� - �������</param>
    /// <param name="formatInfo">��������� ��������������</param>
    protected override void OnFormatAgregateFunction(DBxSqlBuffer buffer, DBxAgregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      buffer.SB.Append(GetFunctionName(function.Function));
      buffer.SB.Append('(');
      if (function.Argument == null)
        buffer.SB.Append('*');
      else
        buffer.FormatExpression(function.Argument, new DBxFormatExpressionInfo());
      buffer.SB.Append(')');
    }

    private string GetFunctionName(DBxAgregateFunctionKind kind)
    {
      // ����� ���� �� ������� Kind.ToString().ToUpperInvariant(), �� ��� ����������� � ����� ������ �������� ����
      switch (kind)
      { 
        case DBxAgregateFunctionKind.Sum:
        case DBxAgregateFunctionKind.Count:
        case DBxAgregateFunctionKind.Min:
        case DBxAgregateFunctionKind.Max:
        case DBxAgregateFunctionKind.Avg:
          return kind.ToString().ToUpperInvariant();
        default:
          throw new ArgumentException("����������� ���������� ������� " + kind.ToString());
      }
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������������� �������� ����
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>,
    /// ��������, ����������� � ���������.
    /// ����� �������� �� ������������� �������� ���������� ��������, ��������, �������� ����������.
    /// ����� �� ��������� �������������� ��������������, � �������� ���� �� ������� OnFormatXxxValue() ��� �������� ���������������� ����.
    /// ���������� �������� ����������� ������� DataTools.FormatDataValue() � ExtTools.dll.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    /// <param name="columnType">��� ��������</param>
    protected sealed /*��������*/ override void OnFormatValue(DBxSqlBuffer buffer, object value, DBxColumnType columnType)
    {
      if (value == null)
      {
        OnFormatNullValue(buffer);
        return;
      }
      if (value is DBNull)
      {
        OnFormatNullValue(buffer);
        return;
      }

      if (value is String)
      {
        //// ��� DataView ������ ������� � ���������. ����� ����������� ������
        //// ��. ������ ������� "DataColumn.Expression Property"

        // 15.02.2016
        // ��� ��������� ��������� ��������� ���������
        // ��������� ������� �� ����������

        string s = (string)value;

        if (columnType == DBxColumnType.Guid)
        {
          // 07.10.2019
          Guid g = new Guid(s);
          OnFormatGuidValue(buffer, g);
        }
        else
          OnFormatStringValue(buffer, s);
        return;
      }

      if (value is Int64)
      {
        OnFormatInt64Value(buffer, (long)value);
        return;
      }

      if (DataTools.IsIntegerType(value.GetType()))
      {
        OnFormatIntValue(buffer, DataTools.GetInt(value)); // ������ ������������ �������������� "(int)value", ��� ��� byte -> int �������� ����������
        return;
      }

      // ����� � ��������� ������ ����������� � �������������� �����,
      // � �� ����������� �� ���������
      if (value is Single)
      {
        OnFormatSingleValue(buffer, (float)value);
        return;
      }
      if (value is Double)
      {
        OnFormatDoubleValue(buffer, (double)value);
        return;
      }
      if (value is Decimal)
      {
        OnFormatDecimalValue(buffer, (decimal)value);
        return;
      }

      if (value is Boolean)
      {
        OnFormatBooleanValue(buffer, (bool)value);
        return;
      }

      if (value is DateTime)
      {
        DateTime TimeValue = (DateTime)value;
        TimeValue = DateTime.SpecifyKind(TimeValue, DateTimeKind.Unspecified);

        bool UseDate, UseTime;
        switch (columnType)
        {
          case DBxColumnType.Date:
            UseDate = true;
            UseTime = false;
            break;
          case DBxColumnType.DateTime:
            UseDate = true;
            UseTime = true;
            break;
          case DBxColumnType.Time:
            UseDate = false;
            UseTime = true;
            break;
          default:
            UseDate = true;
            UseTime = (int)(TimeValue.TimeOfDay.TotalSeconds) != 0;
            break;
        }

        OnFormatDateTimeValue(buffer, TimeValue, UseDate, UseTime);
        return;
      }

      if (value is TimeSpan)
      {
        DateTime TimeValue = new DateTime(((TimeSpan)value).Ticks, DateTimeKind.Unspecified);
        OnFormatDateTimeValue(buffer, TimeValue, false, true);
        return;
      }

      if (value is Guid)
      {
        OnFormatGuidValue(buffer, (Guid)value);
        return;
      }

      throw new NotImplementedException("�������� " + value.ToString() + " ����� ����������� ��� " + value.GetType().ToString());
    }

    /// <summary>
    /// �������������� ���������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>, ����������� � ���������.
    /// ���� ������ �������� ���������, �� ��� �����������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatStringValue(DBxSqlBuffer buffer, string value)
    {
      buffer.SB.Append(@"'");
      for (int i = 0; i < value.Length; i++)
      {
        if (value[i] == '\'')
          buffer.SB.Append(@"''");
        else
          buffer.SB.Append(value[i]);
      }
      buffer.SB.Append(@"'");
    }

    /// <summary>
    /// �������������� ��������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatIntValue(DBxSqlBuffer buffer, int value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// �������������� ��������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatInt64Value(DBxSqlBuffer buffer, long value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// �������������� ��������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatSingleValue(DBxSqlBuffer buffer, float value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// �������������� ��������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatDoubleValue(DBxSqlBuffer buffer, double value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// �������������� ��������� ��������
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatDecimalValue(DBxSqlBuffer buffer, decimal value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// �������������� ����������� ��������.
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// ������������������ ����� ���������� "TRUE" ��� "FALSE".
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatBooleanValue(DBxSqlBuffer buffer, bool value)
    {
      buffer.SB.Append(value ? "TRUE" : "FALSE");
    }

    /// <summary>
    /// �������������� �������� ���� �/��� �������.
    /// ���� ����� ���������� �� OnFormatValue().
    /// ���� �������� � ������� "#M/D/YYYY#"
    /// ���� � ����� �������� � ������� "#M/D/YYYY H:M:S#"
    /// </summary>
    /// <param name="buffer">����� ��� ������ ��������</param>
    /// <param name="value">������������ ��������</param>
    /// <param name="useDate">���� true, �� ������ ���� ������� ��������� ����</param>
    /// <param name="useTime">���� true, �� ������ ���� ������� ��������� �������</param>
    protected virtual void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      buffer.SB.Append('#');

      if (useDate)
      {
        buffer.SB.Append(StdConvert.ToString(value.Month));
        buffer.SB.Append('/');
        buffer.SB.Append(StdConvert.ToString(value.Day));
        buffer.SB.Append('/');
        buffer.SB.Append(StdConvert.ToString(value.Year));
      }
      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append(StdConvert.ToString(value.Hour));
        buffer.SB.Append(':');
        buffer.SB.Append(StdConvert.ToString(value.Minute));
        buffer.SB.Append(':');
        buffer.SB.Append(StdConvert.ToString(value.Second));
      }
      buffer.SB.Append('#');
    }

    /// <summary>
    /// �������������� �������� ���� GUID.
    /// ���������� � <paramref name="buffer"/>.SB �������� <paramref name="value"/>.
    /// �������� ����� Guid.ToString("D") ��� ��������� ������ ������ 36 �������� (��� ������, �� � ������������� "-").
    /// ����� �������� OnFormatStringValue() ��� ������ ������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="value">������������ ��������</param>
    protected virtual void OnFormatGuidValue(DBxSqlBuffer buffer, Guid value)
    {
      OnFormatStringValue(buffer, value.ToString("D"));
    }

    /// <summary>
    /// �������������� ����������� ��������.
    /// ���������� � <paramref name="buffer"/>.SB ����� "NULL".
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    protected virtual void OnFormatNullValue(DBxSqlBuffer buffer)
    {
      buffer.SB.Append("NULL");
    }

    #endregion

    #region �������

    /// <summary>
    /// �������� ���� ��� ������� ValueFilterKind 
    /// </summary>
    /// <param name="kind">��� ���������</param>
    /// <returns>���� �������� ���������</returns>
    protected virtual string GetSignStr(CompareKind kind)
    {
      switch (kind)
      {
        case CompareKind.Equal: return "=";
        case CompareKind.LessThan: return "<";
        case CompareKind.LessOrEqualThan: return "<=";
        case CompareKind.GreaterThan: return ">";
        case CompareKind.GreaterOrEqualThan: return ">=";
        case CompareKind.NotEqual: return "<>";
        default: throw new ArgumentException("����������� Kind: " + kind.ToString());
      }
    }

    #region ������� "��������� IN (������ ��������)"

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatIdsFilter(DBxSqlBuffer buffer, IdsFilter filter)
    {
      if (filter.Ids.Count == 1)
      {
        Int32 singleId = filter.Ids.SingleId;
        CompareFilter Filter2 = new CompareFilter(filter.Expression, new DBxConst(singleId), CompareKind.Equal, singleId == 0, DBxColumnType.Int);
        FormatFilter(buffer, Filter2);
        return;
      }

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.Int;
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(" IN (");
      bool first = true;
      foreach (Int32 id in filter.Ids)
      {
        if (first)
          first = false;
        else
          buffer.SB.Append(", ");

        buffer.SB.Append(id.ToString());
      }
      buffer.SB.Append(')');
    }


    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      // ���� �� � ������ �������� �������� �� ���������
      bool hasDefaultValue = false;
      foreach (object v in filter.Values)
      {
        if (DataTools.IsEmptyValue(v))
        {
          hasDefaultValue = true;
          break;
        }
      }

      if (filter.Values.Length == 1)
      {
        // ��� ������� ValueFilter
        CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.Values.GetValue(0)), CompareKind.Equal, hasDefaultValue, filter.ColumnType);
        FormatFilter(buffer, filter2);
        return;
      }

      // ������� ������ ���������� IN
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = hasDefaultValue;
      if (filter.ColumnType == DBxColumnType.Unknown)
      {
        foreach (object v in filter.Values)
        {
          if (v == null)
            continue;

          formatInfo.WantedColumnType = DBxTools.ValueToColumnType(v);
          if (formatInfo.WantedColumnType != DBxColumnType.Unknown)
            break;
        }
      }
      else
        formatInfo.WantedColumnType = filter.ColumnType;
      OnFormatExpression(buffer, filter.Expression, formatInfo);

      buffer.SB.Append(" IN (");
      for (int i = 0; i < filter.Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatValue(filter.Values.GetValue(i), formatInfo.WantedColumnType);
      }
      buffer.SB.Append(')');
    }

   
    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatInSelectFilter(DBxSqlBuffer buffer, InSelectFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      if (filter.ColumnType != DBxColumnType.Unknown)
        formatInfo.WantedColumnType = filter.ColumnType;

      OnFormatExpression(buffer, filter.Expression, formatInfo);

      buffer.SB.Append(" IN (");

      //DBxSelectFormatter formatter2=new DBxSelectFormatter(filter.selectInfo, DBxNameValidator.

      buffer.SB.Append(')');
      throw new NotImplementedException(); // TODO: 04.12.2020
    }

    #endregion

    #region ��������� �������

    /// <summary>
    /// �������������� �������.
    /// ������������������ ���������� �������� StringValueFilter �� CompareFilter � ������� ������� UPPER()
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {
      DBxExpression expr1, expr2;
      if (filter.IgnoreCase)
      {
        expr1 = new DBxFunction(DBxFunctionKind.Upper, filter.Expression);
        expr2 = new DBxConst(filter.Value.ToUpperInvariant(), DBxColumnType.String);
      }
      else
      {
        expr1 = filter.Expression;
        expr2 = new DBxConst(filter.Value, DBxColumnType.String);
      }
      CompareFilter filter2 = new CompareFilter(expr1, expr2, CompareKind.Equal, true);
      buffer.FormatFilter(filter2);
    }

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {
      DBxExpression expr1;
      if (filter.IgnoreCase)
        expr1 = new DBxFunction(DBxFunctionKind.Upper, filter.Expression);
      else
        expr1 = filter.Expression;

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = false;
      buffer.FormatExpression(expr1, formatInfo);
      buffer.SB.Append(" LIKE '");

      string v = filter.Value;
      if (filter.IgnoreCase)
        v = v.ToUpperInvariant();
      MakeEscapedChars(buffer, v, new char[] { '%', '_', '[', '\'' }, "[", "]");
      buffer.SB.Append("%\'");
    }

    /// <summary>
    /// ��������� ����������� �������� � ������ ��� ��������, ���������� �� ����������� LIKE
    /// </summary>
    /// <param name="buffer">����� ��� ������ ������</param>
    /// <param name="value">������, �������� ���������� �������</param>
    /// <param name="chars">�������, ������� ��������� ��������</param>
    /// <param name="prefix">��������� �����</param>
    /// <param name="suffix">��������� ������</param>
    protected static void MakeEscapedChars(DBxSqlBuffer buffer, string value, char[] chars, string prefix, string suffix)
    {
      if (String.IsNullOrEmpty(value))
        return;
      if (value.IndexOfAny(chars) < 0)
      {
        buffer.SB.Append(value);
        return;
      }

      for (int i = 0; i < value.Length; i++)
      {
        char c = value[i];
        if (Array.IndexOf<char>(chars, c) >= 0)
        {
          // ����������
          buffer.SB.Append(prefix);
          buffer.SB.Append(c);
          buffer.SB.Append(suffix);
        }
        else
          // ������� ������
          buffer.SB.Append(c);
      }
    }

    /// <summary>
    /// �������������� �������.
    /// ������������������ ���������� �������� StringValueFilter �� CompareFilter � ������� ������� UPPER() � SUBSTRING()
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      // 24.06.2019.
      // �� ��, ��� � ������ DataViewDBxSqlFormatter, �� � ��������� � �������� ��������
      DBxExpression expr1, expr2;
      if (filter.IgnoreCase)
      {
        expr1 = new DBxFunction(DBxFunctionKind.Upper, new DBxFunction(DBxFunctionKind.Substring,
          filter.Expression,
          new DBxConst(filter.StartIndex + 1),
          new DBxConst(filter.Value.Length)));
        expr2 = new DBxConst(filter.Value.ToUpperInvariant(), DBxColumnType.String);
      }
      else
      {
        expr1 = new DBxFunction(DBxFunctionKind.Substring,
          filter.Expression,
          new DBxConst(filter.StartIndex + 1),
          new DBxConst(filter.Value.Length));
        expr2 = new DBxConst(filter.Value, DBxColumnType.String);
      }

      CompareFilter filter2 = new CompareFilter(expr1, expr2, CompareKind.Equal, true);
      buffer.FormatFilter(filter2);
    }

    #endregion

    #region ������� �� ����������

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatNumRangeFilter(DBxSqlBuffer buffer, NumRangeFilter filter)
    {
      if (filter.MinValue.HasValue)
      {
        if (filter.MaxValue.HasValue)
        {
          if (filter.MaxValue.Value == filter.MinValue.Value)
          {
            CompareFilter filter3 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value));
            buffer.FormatFilter(filter3);
          }
          else
          {
            if (BetweenInstructionSupported)
            {
              DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
              formatInfo.NoParentheses = false;
              formatInfo.NullAsDefaultValue = true;
              formatInfo.WantedColumnType = DBxColumnType.Money;
              buffer.FormatExpression(filter.Expression, formatInfo);
              buffer.SB.Append(" BETWEEN ");
              buffer.FormatValue(filter.MinValue.Value, DBxColumnType.Unknown);
              buffer.SB.Append(" AND ");
              buffer.FormatValue(filter.MaxValue.Value, DBxColumnType.Unknown);
            }
            else
            {
              CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, true);
              CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value), CompareKind.LessOrEqualThan, true);
              AndFilter filter3 = new AndFilter(filter1, filter2); // 25.12.2020
              buffer.FormatFilter(filter3);
            }
          }
        }
        else
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, true);
          buffer.FormatFilter(filter1);
        }
      }
      else
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value), CompareKind.LessOrEqualThan, true);
          buffer.FormatFilter(filter2);
        }
        else
        {
          // ����������� ������
          buffer.FormatFilter(DummyFilter.AlwaysTrue);
        }
      }
    }

    /// <summary>
    /// ���������� true, ���� ���������� SQL ������������ ���������� BETWEEN.
    /// ������������������ ���������� ���������� true
    /// </summary>
    protected virtual bool BetweenInstructionSupported { get { return true; } }

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatDateRangeFilter(DBxSqlBuffer buffer, DateRangeFilter filter)
    {
      // ��� ��� ���� ����� ��������� ��������� �������, ������ ������������ �����������
      // "�������� <= #������������#". ������ ����� ���� ������������ �����������
      // "�������� < #������������+1#"
      // ������� �� ������ ������������ ���������� ������ �� ��������� ��� MinValue=MaxValue

      if (filter.MinValue.HasValue)
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, false);
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value.AddDays(1)), CompareKind.LessThan, false);
          AndFilter filter3 = new AndFilter(filter1, filter2);
          buffer.FormatFilter(filter3);
        }
        else
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, false);
          buffer.FormatFilter(filter1);
        }
      }
      else
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value.AddDays(1)), CompareKind.LessThan, false);
          buffer.FormatFilter(filter2);
        }
        else
        {
          // ����������� ������
          buffer.FormatFilter(DummyFilter.AlwaysTrue);
        }
      }
    }

    /// <summary>
    /// �������������� �������.
    /// ������������������ ���������� �������� ������ �� ��� CompareFilter, ������������ AndFilter � ������� COALESCE ��� ����� �������� NULL
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatDateRangeInclusionFilter(DBxSqlBuffer buffer, DateRangeInclusionFilter filter)
    {
      DBxFunction expr1 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.Value));
      CompareFilter filter1 = new CompareFilter(expr1, new DBxConst(filter.Value), CompareKind.LessOrEqualThan, false);

      DBxFunction expr2 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.Value));
      CompareFilter filter2 = new CompareFilter(expr2, new DBxConst(filter.Value), CompareKind.GreaterOrEqualThan, false);

      DBxFilter filter3 = new AndFilter(filter1, filter2);
      buffer.FormatFilter(filter3);
    }


    /// <summary>
    /// �������������� �������.
    /// ������������������ ���������� ���������� ���� ��� ��� CompareFilter
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatDateRangeCrossFilter(DBxSqlBuffer buffer, DateRangeCrossFilter filter)
    {
      // 30.06.2019
      // ����������, ����� ������ �� ������������ DateTime.MinValue � MaxValue.
      // ������ ����� ���������� ��� ��������.
      // �����, �� ������, ���� ��������� ���� ������ (SQLite) �� ������������ ���� �������� ��� DateTime

      List<DBxFilter> filters = new List<DBxFilter>();

      if (filter.FirstDate.HasValue)
      {
        // �� ������. ������ ���� (��������� ����) ������������ � �������� ����� ������� � ��������.
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.FirstDate.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.FirstDate.Value), CompareKind.GreaterOrEqualThan, false);
        filters.Add(filter2);
      }
      if (filter.LastDate.HasValue)
      {
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.LastDate.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.LastDate.Value), CompareKind.LessOrEqualThan, false);
        filters.Add(filter2);
      }
      DBxFilter resFilter = AndFilter.FromList(filters);
      if (resFilter == null)
        resFilter = DummyFilter.AlwaysTrue;

      buffer.FormatFilter(resFilter);
    }

    #endregion

    #region ���������� ������� AND/OR/NOT

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatAndFilter(DBxSqlBuffer buffer, AndFilter filter)
    {
      for (int i = 0; i < filter.Filters.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(" AND ");

        bool parentheses = FilterNeedsParentheses(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append('(');
        buffer.FormatFilter(filter.Filters[i]);
        if (parentheses)
          buffer.SB.Append(')');
      }
    }

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatOrFilter(DBxSqlBuffer buffer, OrFilter filter)
    {
      for (int i = 0; i < filter.Filters.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(" OR ");
        bool parentheses = FilterNeedsParentheses(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append('(');

        buffer.FormatFilter(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append(')');
      }
    }

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatNotFilter(DBxSqlBuffer buffer, NotFilter filter)
    {
      buffer.SB.Append("NOT ");
      bool parentheses = FilterNeedsParentheses(filter.BaseFilter);
      if (parentheses)
        buffer.SB.Append("(");

      buffer.FormatFilter(filter.BaseFilter);

      if (parentheses)
        buffer.SB.Append(')');
    }

    /// <summary>
    /// ����� ���������� true, ���� ������ ��������� ������� <paramref name="filter"/> ������ ���� ������
    /// ��� ����������� �������� ������������ true
    /// </summary>
    /// <param name="filter">������, ��� �������� ������������ ������������� �������� ��� ��������</param>
    /// <returns>������������� � �������</returns>
    protected virtual bool FilterNeedsParentheses(DBxFilter filter)
    {
      // TODO:
      //if (filter is ValueFilter)
      //{
      //  if (((ValueFilter)filter).Value == null)
      //    return true;
      //  else
      //    return false;
      //}

      return true;
    }

    #endregion

    #region ������ �������

    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatCompareFilter(DBxSqlBuffer buffer, CompareFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;
      DBxConst cnst2 = filter.Expression2.GetConst();
      if (cnst2 != null)
      {
        if (cnst2.Value == null)
        {
          switch (filter.Kind)
          {
            case CompareKind.Equal:
            case CompareKind.NotEqual:
              OnFormatNullNotNullCompareFilter(buffer, filter.Expression1, cnst2.ColumnType, filter.Kind);
              return;
            default:
              throw new ArgumentException("� ������� ������ ��������� �������� � NULL � ������ " + filter.Kind.ToString() + ". ����������� ������ ��������� �� ��������� � �����������");
          }
        }

        formatInfo.WantedColumnType = cnst2.ColumnType;
      }
      else
      {
        DBxConst cnst1 = filter.Expression1.GetConst();
        if (cnst1 != null)
        {
          if (cnst1.Value == null)
          {
            switch (filter.Kind)
            {
              case CompareKind.Equal:
              case CompareKind.NotEqual:
                OnFormatNullNotNullCompareFilter(buffer, filter.Expression2, cnst1.ColumnType, filter.Kind);
                return;
              default:
                throw new ArgumentException("� ������� ������ ��������� �������� � NULL � ������ " + filter.Kind.ToString() + ". ����������� ������ ��������� �� ��������� � �����������");
            }
          }

          formatInfo.WantedColumnType = cnst1.ColumnType;
        }
      }


      buffer.FormatExpression(filter.Expression1, formatInfo);
      buffer.SB.Append(GetSignStr(filter.Kind));
      buffer.FormatExpression(filter.Expression2, formatInfo);
    }

    /// <summary>
    /// ������ ������� CompareFilter � ������ ��������� �������� � NULL.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="expression">���������, ������� ���� �������� � NULL (����� ����� �������)</param>
    /// <param name="columnType">��� ������</param>
    /// <param name="kind">����� ���������: Equal ��� NotEqual</param>
    protected virtual void OnFormatNullNotNullCompareFilter(DBxSqlBuffer buffer, DBxExpression expression, DBxColumnType columnType, CompareKind kind)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.WantedColumnType = columnType;
      formatInfo.NoParentheses = false;
      formatInfo.NullAsDefaultValue = false;
      buffer.FormatExpression(expression, formatInfo);
      switch (kind)
      {
        case CompareKind.Equal:
          buffer.SB.Append(" IS NULL");
          break;
        case CompareKind.NotEqual:
          buffer.SB.Append(" IS NOT NULL");
          break;
        default:
          throw new ArgumentException("������������ kind=" + kind.ToString(), "kind");
      }
    }

#if XXX
    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="Buffer">����� ��� ������</param>
    /// <param name="Filter">������</param>
    protected override void OnFormatValueFilter(DBxSqlBuffer Buffer, ValueFilter Filter)
    {
      // ��� ������ �������� null ���������� ������� IsNull()
      if (Filter.Value == null || Filter.Value is DBNull)
      {
        if (Filter.Kind != ValueFilterKind.Equal)
          throw new InvalidOperationException("�������� NULL � ������� ��������� ����������� ������ ��� ��������� �� ��������� (���� \"" + Filter.ColumnName + "\")");

        if (Filter.DataType == null)
          throw new InvalidOperationException("��� ��������� � NULL ���������, ����� ��� ����� ��� �������� � �������� ValueFilter.DataType (���� \"" + Filter.ColumnName + "\")");

        Buffer.SB.Append(IsNullFunctionName);
        Buffer.SB.Append("(");
        Buffer.FormatColumnName(Filter.ColumnName);
        Buffer.SB.Append(", ");
        Buffer.FormatValue(DataTools.GetEmptyValue(Filter.DataType), DBxColumnType.Unknown);
        Buffer.SB.Append(")=");
        Buffer.FormatValue(DataTools.GetEmptyValue(Filter.DataType), DBxColumnType.Unknown);
        return;
      }

      if (Filter.Kind == ValueFilterKind.Equal)
      {
        // ��� �������� 0 � false ���������� ISNULL() � ���������� �� ����������
        if (DataTools.IsEmptyValue(Filter.Value))
        {
          Buffer.SB.Append(IsNullFunctionName);
          Buffer.SB.Append("(");
          Buffer.FormatColumnName(Filter.ColumnName);
          Buffer.SB.Append(',');
          Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
          Buffer.SB.Append(")=");
          Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
          return;
        }
      }

      Buffer.FormatColumnName(Filter.ColumnName);
      Buffer.SB.Append(GetSignStr(Filter.Kind));
      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    }

#endif


    /// <summary>
    /// �������������� �������.
    /// </summary>
    /// <param name="buffer">����� ��� ������</param>
    /// <param name="filter">������</param>
    protected override void OnFormatDummyFilter(DBxSqlBuffer buffer, DummyFilter filter)
    {
      if (filter.IsTrue)
        buffer.SB.Append("1=1");
      else
        buffer.SB.Append("1=0");
    }

    #endregion

    #endregion
  }
}
