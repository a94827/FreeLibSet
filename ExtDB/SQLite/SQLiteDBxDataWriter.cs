using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Data.SQLite
{

  /// <summary>
  /// Объект для записи в базу данных SQLite.
  /// Для SQLite нет обходных путей для группового добавления записей, но есть оператор "INSERT OR UPDATE".
  /// Также создаем однократно команду <see cref="SQLiteCommand"/>.
  /// </summary>
  internal class SQLiteDBxDataWriter : DBxDataWriter
  {
    #region Конструктор и Dispose

    public SQLiteDBxDataWriter(SQLiteDBxCon con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Command != null)
        {
          _Command.Dispose(); // 28.12.2020
          _Command = null;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Подготовленная команда

    private SQLiteCommand _Command;

    private void PrepareCommand()
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer(Con.DB.Formatter);
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          FormatInsertSQL(buffer);
          break;

        case DBxDataWriterMode.Update:
          FormatUpdateSql(buffer);
          break;

        case DBxDataWriterMode.InsertOrUpdate:
          // TODO: Можно-таки использовать "UPSERT", но только если нет внешних ключей на эту таблицу

          //if (((SQLiteDBxCon)Con).ServerVersion>=new Version(3, 24))
          //{ 
          //}
          //else
          //{
          // Используем "двойную" команду, сначала INSERT, потом UPDATE
          // https://stackoverflow.com/questions/15277373/sqlite-upsert-update-or-insert/38463024#38463024
          // см. ответ №69

          FormatUpdateSql(buffer);
          buffer.SB.Append(";");
          buffer.SB.Append(Environment.NewLine);

          //FormatInsertSQL(Buffer);
          buffer.SB.Append("INSERT INTO ");
          buffer.FormatTableName(WriterInfo.TableName);
          buffer.SB.Append(" (");
          buffer.FormatCSColumnNames(WriterInfo.Columns);
          buffer.SB.Append(") SELECT ");

          for (int i = 0; i < Values.Length; i++)
          {
            if (i > 0)
              buffer.SB.Append(',');
            buffer.FormatParamPlaceholder(i);
          }

          buffer.SB.Append(" WHERE (SELECT Changes()=0)");
          //}
          break;

        default:
          throw new BugException("Unknown Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = new SQLiteCommand(buffer.SB.ToString());
      for (int i = 0; i < Values.Length; i++)
        _Command.Parameters.Add(new SQLiteParameter("P" + (i + 1).ToString(), null));
      _Command.Connection = ((SQLiteDBxCon)Con).Connection;
      _Command.Transaction = ((SQLiteDBxCon)Con).CurrentTransaction;
      _Command.CommandTimeout = Con.CommandTimeout;
      _Command.Prepare(); // для порядка.
    }

    private void FormatInsertSQL(DBxSqlBuffer buffer)
    {
      buffer.SB.Append("INSERT INTO ");
      buffer.FormatTableName(WriterInfo.TableName);
      buffer.SB.Append(" (");
      buffer.FormatCSColumnNames(WriterInfo.Columns);
      buffer.SB.Append(") VALUES (");

      for (int i = 0; i < Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(',');
        buffer.FormatParamPlaceholder(i);
      }
      buffer.SB.Append(")");
    }

    private void FormatUpdateSql(DBxSqlBuffer buffer)
    {
      buffer.SB.Append("UPDATE ");
      buffer.FormatTableName(WriterInfo.TableName);
      buffer.SB.Append(" SET ");

      for (int i = 0; i < OtherColumns.Count; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatColumnName(OtherColumns[i]);
        buffer.SB.Append("=");
        buffer.FormatParamPlaceholder(OtherColumnPositions[i]);
      }

      buffer.SB.Append(" WHERE ");
      for (int i = 0; i < SearchColumns.Count; i++)
      {
        if (i > 0)
          buffer.SB.Append(" AND ");
        buffer.FormatColumnName(SearchColumns[i]);
        buffer.SB.Append("=");
        buffer.FormatParamPlaceholder(SearchColumnPositions[i]);
      }
    }

    #endregion

    #region OnWrite

    protected override void OnWrite()
    {
      if (_Command == null)
        PrepareCommand();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        _Command.Parameters[i].Value = v;
      }

      Con.SQLExecuteNonQuery(_Command);
    }

    #endregion
  }
}
