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
      foreach (DBxTableStruct table in dbs.Tables)
      {
        CheckTableStruct(table, db);

        foreach (DBxColumnStruct col in table.Columns)
        {
          if (!String.IsNullOrEmpty(col.MasterTableName))
          {
            DBxTableStruct masterTable = dbs.Tables[col.MasterTableName];
            if (masterTable == null)
              throw new DBxStructException(table, "В таблице \"" + table.TableName + "\" ссылочное поле \"" + col.ColumnName + "\" ссылается на несуществующую таблицу \"" + col.MasterTableName + "\"");
            switch (masterTable.PrimaryKey.Count)
            {
              case 0:
                throw new DBxStructException(table, "В таблице \"" + table.TableName + "\" ссылочное поле \"" + col.ColumnName + "\" ссылается на таблицу \"" + masterTable.TableName + "\", которая не имеет первичного ключа");
              case 1:
                DBxColumnStruct pkCol = masterTable.Columns[masterTable.PrimaryKey[0]];
                if (pkCol.ColumnType != col.ColumnType)
                  throw new DBxStructException(table, "В таблице \"" + table.TableName + "\" ссылочное поле \"" + col.ColumnName + "\" типа \"" + col.ColumnType.ToString() + "\" ссылается на таблицу \"" + masterTable.TableName + "\", которая имеет первичный ключ по полю \"" + pkCol.ColumnType + "\"типа " + pkCol.ColumnType);
                break;
              default:
                throw new DBxStructException(table, "В таблице \"" + table.TableName + "\" ссылочное поле \"" + col.ColumnName + "\" ссылается на таблицу \"" + masterTable.TableName + "\", которая имеет составной первичный ключ");
            }
          }
        }
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
          throw new DBxStructException(table, "Неправильное имя таблицы \"" + table.TableName + "\"." + errorText);
      }

      if (table.Columns.Count == 0)
        throw new DBxStructException(table, "Таблица \"" + table.TableName + "\" не содержит ни одного столбца"); // 07.06.2023

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
          throw new DBxStructException(table, "Неправильное имя столбца \"" + column.ColumnName + "\". " + ErrorText);
      }

      switch (column.ColumnType)
      {
        case DBxColumnType.String:
          if (column.MaxLength < 1)
            throw new DBxStructException(table, "Для текстового столбца \"" + column.ColumnName + "\" не задана длина строки");
          break;
        case DBxColumnType.Int:
        case DBxColumnType.Float:
        case DBxColumnType.Decimal:
          if (column.MinValue != 0.0 || column.MaxValue != 0.0)
          {
            if (column.MinValue > column.MaxValue)
              throw new DBxStructException(table, "Для числового столбца \"" + column.ColumnName + "\" не задан неправильный диапазон значений: {" + column.MinValue.ToString() + ":" + column.MaxValue.ToString() + "}");
          }
          break;
        case DBxColumnType.Unknown:
          throw new DBxStructException(table, "Для столбца \"" + column.ColumnName + "\" не задан тип данных");
      }
    }


    /// <summary>
    /// Проверяет, что первичным ключом таблицы является единственное целочисленное поле.
    /// Если это не так, генерируется <see cref="DBxPrimaryKeyException"/>.
    /// </summary>
    /// <param name="table">Проверяемая структура таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public static string CheckTablePrimaryKeyInt32(DBxTableStruct table)
    {
      switch (table.PrimaryKey.Count)
      {
        case 1:
          DBxColumnStruct column = table.Columns[table.PrimaryKey[0]];
          if (column.DataType != typeof(Int32))
            throw new DBxPrimaryKeyException("Таблица \"" + table.TableName + "\" имеет первичный ключа по полю \"" + column.ColumnName + "\", которое имеет тип (" + column.ColumnType.ToString() + "), отличный от Int32");
          return column.ColumnName;
        case 0:
          throw new DBxPrimaryKeyException("Таблица \"" + table.TableName + "\" не имеет первичного ключа. Требуется первичный ключ по целочисленному полю");
        default:
          throw new DBxPrimaryKeyException("Таблица \"" + table.TableName + "\" имеет составной первичный ключ. Требуется первичный ключ по единственному целочисленному полю");
      }
    }

    #endregion
  }
}
