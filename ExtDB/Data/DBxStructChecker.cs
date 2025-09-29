using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// 
  /// </summary>
  public static class DBxStructChecker
  {
    #region Проверка структуры базы данных

    /// <summary>
    /// Проверка структуры базы данных.
    /// Вызывает методы <see cref="CheckTableStruct(DBxTableStruct)"/> и проверяет корректность ссылочных полей.
    /// Эта версия не проверяет корректность имен обычных полей.
    /// </summary>
    /// <param name="dbs">Проверяемая структура базы данных</param>
    public static void CheckStruct(DBxStruct dbs)
    {
      CheckStruct(dbs, null);
    }

    /// <summary>
    /// Проверка структуры базы данных.
    /// Вызываются методы <see cref="CheckTableStruct(DBxTableStruct, DBx)"/> и проверяет корректность ссылочных полей.
    /// Эта версия проверяет корректность имен обычных полей, если параметр <paramref name="db"/> задан.
    /// </summary>
    /// <param name="dbs">Проверяемая структура базы данных</param>
    /// <param name="db">База данных для проверки корректности имен полей или null</param>
    public static void CheckStruct(DBxStruct dbs, DBx db)
    {
      try
      {
        if (!dbs.IsReadOnly)
          dbs.UpdateColumnTypes();

        foreach (DBxTableStruct table in dbs.Tables)
        {
          CheckTableStruct(table, db);

          foreach (DBxColumnStruct col in table.Columns)
          {
            if (!String.IsNullOrEmpty(col.MasterTableName))
            {
              DBxTableStruct masterTable = dbs.Tables[col.MasterTableName];
              if (masterTable == null)
                throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_RefToUnknownTable,
                  table.TableName, col.ColumnName, col.MasterTableName));
              switch (masterTable.PrimaryKey.Count)
              {
                case 0:
                  throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_RefToTableNoPK,
                    table.TableName, col.ColumnName, masterTable.TableName));
                case 1:
                  DBxColumnStruct pkCol = masterTable.Columns[masterTable.PrimaryKey[0]];
                  if (pkCol.ColumnType != col.ColumnType)
                    throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_RefColumnTypeMismatch,
                      table.TableName, col.ColumnName, col.ColumnType.ToString(),
                      masterTable.TableName, pkCol.ColumnName, pkCol.ColumnType));
                  break;
                default:
                  throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_RefToTableComplexPK,
                    table.TableName, col.ColumnName, masterTable.TableName));
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        if (db != null)
          e.Data["DBx.DatabaseName"] = db.DatabaseName;
        throw;
      }
    }

    #endregion

    #region Методы проверки

    /// <summary>
    /// Проверяет корректность описаний таблицы.
    /// Обычно следует проверять структуру базы данных в-целом, а не отдельной таблицы,
    /// так как там проверяется корректность ссылочных полей.
    /// Эта версия не проверяет корректность имен обычных полей.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    public static void CheckTableStruct(DBxTableStruct table)
    {
      CheckTableStruct(table, null);
    }

    /// <summary>
    /// Проверяет корректность описаний таблицы.
    /// Обычно следует проверять структуру базы данных в-целом, а не отдельной таблицы,
    /// так как там проверяется корректность ссылочных полей.
    /// Эта версия может проверять корректность имен обычных полей.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    /// <param name="db">База данных для проверки корректности имен полей или null</param>
    public static void CheckTableStruct(DBxTableStruct table, DBx db)
    {
      if (db != null)
      {
        string errorText;
        if (!db.IsValidTableName(table.TableName, out errorText))
          throw new DBxStructException(table, String.Format(Res.DBxNameValidator_Arg_InvalidTableName,
            table.TableName, errorText));
      }

      if (table.Columns.Count == 0)
        throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_TableWithoutColumns, table.TableName)); // 07.06.2023

      foreach (DBxColumnStruct column in table.Columns)
        CheckColumnStruct(column, table, db);
    }

    /// <summary>
    /// Проверяет корректность описания столбцп
    /// </summary>
    /// <param name="column">Проверяемое описание столбца</param>
    /// <param name="table">Описание таблицы</param>
    /// <param name="db">База данных (может быть null</param>
    public static void CheckColumnStruct(DBxColumnStruct column, DBxTableStruct table, DBx db)
    {
      if (db != null)
      {
        string ErrorText;
        if (!db.IsValidColumnName(column.ColumnName, false, out ErrorText))
          throw new DBxStructException(table, String.Format(Res.DBxNameValidator_Arg_InvalidColumnName,
            column.ColumnName, ErrorText));
      }

      switch (column.ColumnType)
      {
        case DBxColumnType.String:
          if (column.MaxLength < 1)
            throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_StringColumnWithoutLen,
              column.ColumnName));
          break;
        case DBxColumnType.Unknown:
          throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_UnknownColumnType,
            column.ColumnName));
        default:
          if (DBxTools.IsNumericType(column.ColumnType))
          {
            if (column.MinValue != 0.0 || column.MaxValue != 0.0)
            {
              if (column.MinValue > column.MaxValue)
                throw new DBxStructException(table, String.Format(Res.DBxStructChecker_Err_InvNumRange,
                  column.ColumnName, column.MinValue, column.MaxValue));
            }
          }
          break;
      }
    }


    /// <summary>
    /// Проверяет, что у таблицы есть первичный ключ.
    /// Если это не так, генерируется <see cref="PrimaryKeyException"/>.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public static void CheckTablePrimaryKeyExists(DBxTableStruct table)
    {
      if (table.PrimaryKey.Count==0)
        throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_NoPK,
          table.TableName, "Any type"));
    }

    /// <summary>
    /// Проверяет, что первичным ключом таблицы является единственное поле.
    /// Если это не так, генерируется <see cref="PrimaryKeyException"/>.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public static string CheckTablePrimaryKeySimple(DBxTableStruct table)
    {
      switch (table.PrimaryKey.Count)
      {
        case 1:
          DBxColumnStruct column = table.Columns[table.PrimaryKey[0]];
          return column.ColumnName;
        case 0:
          throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_NoPK,
            table.TableName, "Any type"));
        default:
          throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_ComplexPK,
            table.TableName, "Any type"));
      }
    }

    /// <summary>
    /// Проверяет, что первичным ключом таблицы является единственное целочисленное поле.
    /// Если это не так, генерируется <see cref="PrimaryKeyException"/>.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public static string CheckTablePrimaryKeyInteger(DBxTableStruct table)
    {
      switch (table.PrimaryKey.Count)
      {
        case 1:
          DBxColumnStruct column = table.Columns[table.PrimaryKey[0]];
          if (!MathTools.IsIntegerType(column.DataType))
            throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_PKWrongType,
              table.TableName, column.ColumnName, column.ColumnType, "Int32"));
          return column.ColumnName;
        case 0:
          throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_NoPK,
            table.TableName, "Int32"));
        default:
          throw new PrimaryKeyException(String.Format(Res.DBxStructChecker_Err_ComplexPK,
            table.TableName, "Int32"));
      }
    }

    #endregion
  }
}
