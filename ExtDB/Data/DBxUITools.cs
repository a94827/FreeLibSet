// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.UICore;

// #pragma warning disable 1591

namespace FreeLibSet.Data
{
  /// <summary>
  /// Создание текстов сообщений
  /// </summary>
  internal static class DBxUITools
  {
    /// <summary>
    /// Сообщения для процентного индикатора
    /// </summary>
    public static class PhaseText
    {
      public static string TableCreation(DBxTableStruct table)
      {
        return String.Format(Res.DBx_Phase_TableCreation, table.TableName);
      }

      public static string ColumnCreation(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Phase_ColumnCreation, column.ColumnName, table.TableName);
      }

      public static string ColumnLengthChanging(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Phase_ColumnSetLength, column.ColumnName, table.TableName);
      }

      public static string FKVerification()
      {
        return Res.DBx_Phase_FKVerification;
      }

      public static string FKCreation(string fkName)
      {
        return String.Format(Res.DBx_Phase_FKCreation, fkName);
      }

      public static string IndexCreation(DBxTableStruct table, DBxIndexStruct index)
      {
        return String.Format(Res.DBx_Phase_IndexCreation, table.TableName, UITools.ValueListToString(index.Columns.AsArray));
      }

      public static string IndexRemoving(string indexName)
      {
        return String.Format(Res.DBx_Phase_IndexRemoving, indexName);
      }
    }

    /// <summary>
    /// Сообщения для отчета по обновлению базы данных
    /// </summary>
    public static class UpdateMsg
    {
      public static string TableCreated(DBxTableStruct table)
      {
        return String.Format(Res.DBx_Msg_TableCreated, table.TableName);
      }

      public static string ColumnCreated(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Msg_ColumnCreated, column.ColumnName, table.TableName);
      }

      public static string ColumnTypeDiff(DBxTableStruct table, DBxColumnStruct column, string wantedType, string realType)
      {
        return String.Format(Res.DBx_Err_ColumnTypeDiff,
          column.ColumnName, table.TableName, column.ColumnType, wantedType, realType);
      }

      public static string ColumnIsLonger(DBxTableStruct table, DBxColumnStruct column, int realLen)
      {
        return String.Format(Res.DBx_Msg_ColumnLonger,
          column.ColumnName, table.TableName, column.MaxLength, realLen);
      }

      public static string DeleteAllIndexesByColumnLength(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Msg_DelAllIndexesByColumnLength,
          table.TableName, column.ColumnName);
      }

      public static string ColumnLengthChanged(DBxTableStruct table, DBxColumnStruct column, int realLen)
      {
        return String.Format(Res.DBx_Msg_ColumnLengthChanged,
          column.ColumnName, table.TableName, realLen, column.MaxLength);
      }

      public static string ColumnDefaultSet(DBxTableStruct table, DBxColumnStruct column, string wantedDefExpr)
      {
        if (String.IsNullOrEmpty(wantedDefExpr))
          return String.Format(Res.DBx_Msg_DefaultReset, table.TableName, column.ColumnName);
        else
          return String.Format(Res.DBx_Msg_DefaultSet, table.TableName, column.ColumnName, wantedDefExpr);
      }

      public static string NullToDefaultValue(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Msg_NullToDefaultValue, table.TableName, column.ColumnName);
      }

      public static string NullChanged(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Msg_NullChanged, table.TableName, column.ColumnName, column.Nullable ? "NULL" : "NOT NULL");
      }

      public static string PKAdded(DBxTableStruct table, string pkName)
      {
        return String.Format(Res.DBx_Msg_PKAdded, table.TableName, pkName);
      }

      public static string PKRemovedWrong(DBxTableStruct table, string pkName)
      {
        return String.Format(Res.DBx_Msg_PKRemovedWrong, table.TableName, pkName);
      }

      public static string PKRemovedNoPK(DBxTableStruct table, string pkName)
      {
        return String.Format(Res.DBx_Msg_PKRemovedNoPK, table.TableName, pkName);
      }

      public static string FKCreated(DBxTableStruct table, string fkName, params DBxColumnStruct[] columns)
      {
        string[] colNames = new string[columns.Length];
        for (int i = 0; i < columns.Length; i++)
          colNames[i] = columns[i].ColumnName;
        return String.Format(Res.DBx_Msg_FKCreated, fkName, table.TableName, UITools.ValueListToString(colNames));
      }

      public static string IndexCreated(DBxTableStruct table, DBxIndexStruct index)
      {
        return String.Format(Res.DBx_Msg_IndexCreated, index.IndexName, table.TableName, UITools.ValueListToString(index.Columns.AsArray));
      }

      public static string IndexRemovedExcess(DBxTableStruct table, string indexName)
      {
        return String.Format(Res.DBx_Msg_IndexRemovedExcess, table.TableName, indexName);
      }

      public static string IndexRemovedWrong(DBxTableStruct table, string indexName)
      {
        return String.Format(Res.DBx_Msg_IndexRemovedWrong, table.TableName, indexName);
      }

      public static string TableCommentChanged(DBxTableStruct table)
      {
        return String.Format(Res.DBx_Msg_TableCommentChanged, table.TableName);
      }

      public static string ColumnCommentChanged(DBxTableStruct table, DBxColumnStruct column)
      {
        return String.Format(Res.DBx_Msg_ColumnCommentChanged, column.ColumnName, table.TableName);
      }
    }
  }
}
