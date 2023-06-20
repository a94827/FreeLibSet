#define USE_NEW_READER
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace FreeLibSet.Data.SqlClient
{

#if USE_NEW_READER

  // Новая версия DBxDataWriter для всех режимов.
  // - Для режима Insert используется промежуточная DataTable, в которой накапливаются записи.
  //   В OnFinish() вызывается SqlBulkCopy.WriteToServer() для группого добавления строк
  // - Для режима Update используется объект SqlCommand для команды UPDATE, как в SQLiteDBxDataWriter.
  // - Для режима InsertOrUpdate используется комбинация действий:
  //   1. Выполняется поиск в накапливаемой таблице на предмет наличия добавленной строки, которая еще
  //      не записана на сервер. Если строка найдена, она заменяется в DataTable без обращения к серверу.
  //   2. Выполняется команда UPDATE. Возвращается переменная @@ROWCOUNT.
  //   3. Если п.2 вернул 0, то строка добавляется в промежуточную таблицу Insert.
  internal class SqlDBxDataWriter : DBxDataWriter
  {
    #region Конструктор и Dispose

    public SqlDBxDataWriter(SqlDBxCon con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

    internal new SqlDBxCon Con { get { return (SqlDBxCon)(base.Con); } }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    #endregion

    #region OnWrite(), OnFinish()

    protected override void OnWrite()
    {
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          AddInsertRow();
          break;
        case DBxDataWriterMode.Update:
          DoUpdate();
          break;
        case DBxDataWriterMode.InsertOrUpdate:
          if (!UpdateInInsertTable())
          {
            if (!DoUpdate())
              AddInsertRow();
          }
          break;
        default:
          throw new BugException("Invalid mode");
      }
    }

    protected override void OnFinish()
    {
      FlushInsertTable();
      if (_Command != null)
      {
        _Command.Dispose();
        _Command = null;
      }
    }

    /// <summary>
    /// Быстрое добавление записей в режиме Insert
    /// </summary>
    /// <param name="table">Таблица</param>
    public override void LoadFrom(DataTable table)
    {
      if (WriterInfo.Mode == DBxDataWriterMode.Insert)
      {
        OnFinish(); // Если были построчные вызовы Write()
        using (SqlBulkCopy bc = CreateBulkCopy())
        {
          bc.WriteToServer(table);
        }
      }
      else
        base.LoadFrom(table);
    }

    /// <summary>
    /// Быстрое добавление записей в режиме Insert
    /// </summary>
    /// <param name="reader">Исходные записи</param>
    public override void LoadFrom(DbDataReader reader)
    {
      if (WriterInfo.Mode == DBxDataWriterMode.Insert)
      {
        OnFinish(); // Если были построчные вызовы Write()
        using (SqlBulkCopy bc = CreateBulkCopy())
        {
          bc.WriteToServer(reader);
        }
      }
      else
        base.LoadFrom(reader);
    }

    #endregion

    #region Добавление записей

    /// <summary>
    /// Таблица для накопления новых строк в режимах Insert и InsertOrUpdate.
    /// </summary>
    private DataTable _InsertTable;

    private object[] _InsertTableSearchKeys; // чтобы не создавать массив при каждом вызове

    private void AddInsertRow()
    {
      if (_InsertTable == null)
      {
        _InsertTable = base.CreateDataTable();
        if (WriterInfo.Mode == DBxDataWriterMode.InsertOrUpdate)
        {
          DBxOrder order = DBxOrder.FromColumns(SearchColumns);
          _InsertTable.DefaultView.Sort = order.ToString();
          _InsertTableSearchKeys = new object[SearchColumns.Count];
        }
      }

      DataRow row = _InsertTable.NewRow();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        if (!Object.ReferenceEquals(v, null))
          row[i] = v;
      }

      _InsertTable.Rows.Add(row);
    }

    /// <summary>
    /// Выполняет поиск строки в InsertTable. 
    /// Если строка найдена по ключевым полям, то заменяются неключевые поля и возвращается true
    /// </summary>
    /// <returns></returns>
    private bool UpdateInInsertTable()
    {
      if (_InsertTable == null)
        return false;
      if (_InsertTable.Rows.Count == 0)
        return false;

      for (int i = 0; i < _InsertTableSearchKeys.Length; i++)
      {
        _InsertTableSearchKeys[i] = Values[SearchColumnPositions[i]];
        if (_InsertTableSearchKeys[i] == null)
          _InsertTableSearchKeys[i] = DBNull.Value;
      }

      int pos = _InsertTable.DefaultView.Find(_InsertTableSearchKeys);
      if (pos < 0)
        return false;

      DataRow resRow = _InsertTable.DefaultView[pos].Row;
      for (int i = 0; i < OtherColumnPositions.Length; i++)
      {
        object v = Values[OtherColumnPositions[i]];
        if (v == null)
          v = DBNull.Value;
        resRow[OtherColumnPositions[i]] = v;
      }
      return true;
    }

    /// <summary>
    /// Групповой INSERT для строк из _InsertTable
    /// </summary>
    private void FlushInsertTable()
    {
      if (_InsertTable == null)
        return;
      if (_InsertTable.Rows.Count == 0)
        return;

      using (SqlBulkCopy bc = CreateBulkCopy())
      {
        bc.WriteToServer(_InsertTable);
      }
      _InsertTable.Rows.Clear();
    }

    private SqlBulkCopy CreateBulkCopy()
    {
      SqlBulkCopyOptions options = SqlBulkCopyOptions.Default;
      if (base.SearchColumns.Count > 0)
        options |= SqlBulkCopyOptions.KeepIdentity;

      SqlBulkCopy bc = new SqlBulkCopy(Con.Connection, options, Con.CurrentTransaction);
      bc.DestinationTableName = WriterInfo.TableName;

      for (int i = 0; i < ColumnDefs.Length; i++)
        bc.ColumnMappings.Add(ColumnDefs[i].ColumnName, ColumnDefs[i].ColumnName);

      return bc;
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Подготовленная команда
    /// </summary>
    private SqlCommand _Command;

    /// <summary>
    /// Создание команды вынесено в отдельную функцию, чтобы при возникновении исключения не осталась недоинициализованная команда.
    /// </summary>
    /// <returns></returns>
    private SqlCommand CreateCommand()
    {
#if DEBUG
      if (Values.Length != (SearchColumns.Count + OtherColumns.Count))
        throw new BugException("Values.Length");
#endif

      DBxSqlBuffer buffer = new DBxSqlBuffer(Con.DB.Formatter);
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
      // Не требуется, т.к. ExecuteNonQuery() и так возвращает количество строк
      //if (WriterInfo.Mode == DBxDataWriterMode.InsertOrUpdate)
      //{
      //  buffer.SB.Append(";SELECT @@ROWCOUNT");
      //}

      SqlCommand cmd = new SqlCommand(buffer.SB.ToString());
      for (int i = 0; i < Values.Length; i++)
      {
        //DbType dbType = ColumnDefs[i].DbType;
        //SqlDbType sqlType = SqlDBx.GetSqlDbType(ColumnDefs[i]);
//#if DEBUG
        //if (dbType == DbType.Object)
//          throw new BugException("Для столбца " + ColumnDefs[i].ColumnName + " не определено свойство DbType");
//#endif
        SqlParameter prm = new SqlParameter();
        prm.ParameterName = "P" + (i + 1).ToString();
        SqlDBx.InitSqlParameter(ColumnDefs[i], prm);
        cmd.Parameters.Add(prm);

      }

      cmd.Connection = Con.Connection;
      cmd.Transaction = Con.CurrentTransaction;
      cmd.CommandTimeout = Con.CommandTimeout;
      cmd.Prepare();
      return cmd;
    }

    /// <summary>
    /// Выполнить команду UPDATE
    /// </summary>
    /// <returns>True, если строки были найдены и обновление выполнено</returns>
    private bool DoUpdate()
    {
      if (_Command == null)
        _Command = CreateCommand();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        _Command.Parameters[i].Value = v;
      }

      int nRecs = Con.SQLExecuteNonQuery(_Command);
      if (nRecs < 0)
        throw new BugException("Не получено количество строк, обработанных командой UPDATE");
      return nRecs > 0;
    }

    #endregion
  }

#else

  /// <summary>
  /// Объект для записи в базу данных MS SQL Server.
  /// Специальный объект SqlBulkCopy используется только для добавления строк
  /// (режим Insert).
  /// </summary>
  internal class SqlDBxDataInsertWriter : DBxDataWriter
  {
  #region Конструктор

    public SqlDBxDataInsertWriter(SqlDBxCon con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
#if DEBUG
      if (writerInfo.Mode != DBxDataWriterMode.Insert)
        throw new ArgumentException("writerInfo.Mode=" + writerInfo.Mode.ToString(), "writerInfo");
#endif

      PrepareBC();
    }

    internal new SqlDBxCon Con { get { return (SqlDBxCon)(base.Con); } }

    private void PrepareBC()
    {
      SqlBulkCopyOptions options = SqlBulkCopyOptions.Default;
      if (base.SearchColumns.Count > 0)
        options |= SqlBulkCopyOptions.KeepIdentity;

      _BC = new SqlBulkCopy(Con.Connection, options, Con.CurrentTransaction);
      _BC.DestinationTableName = WriterInfo.TableName;

      for (int i = 0; i < ColumnDefs.Length; i++)
        _BC.ColumnMappings.Add(ColumnDefs[i].ColumnName, ColumnDefs[i].ColumnName);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
        ((IDisposable)_BC).Dispose();
      _BC = null;
      base.Dispose(disposing);
    }

  #endregion

  #region OnWrite

    /// <summary>
    /// Копировщик ADO.NET
    /// </summary>
    private SqlBulkCopy _BC;

    /// <summary>
    /// <see cref="SqlBulkCopy"/> требует на входе строки или таблицу.
    /// Нет метода для одного набора значений
    /// </summary>
    private DataTable _Table;

    protected override void OnWrite()
    {
      // У метода SqlBulkCopy.WriteToServer() на входе могут быть только строки
      // таблицы
      if (_Table == null)
        _Table = base.CreateDataTable();

      DataRow row = _Table.NewRow();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        if (!Object.ReferenceEquals(v, null))
          row[i] = v;
      }

      _Table.Rows.Add(row);
    }

    protected override void OnFinish()
    {
      if (_Table != null)
      {
        if (_Table.Rows.Count > 0)
        {
          //object[] a = _Table.Rows[0].ItemArray;
          //_Table.Rows[0][2] = Guid.Empty.ToString();
          _BC.WriteToServer(_Table);
          _Table.Rows.Clear();
        }
      }
    }

    protected override void OnPulseTransaction()
    {
      // Метод OnFinish() уже вызван

      if (_BC != null)
      {
        ((IDisposable)_BC).Dispose();
        _BC = null;
      }

      base.OnPulseTransaction();

      PrepareBC(); // из-за смены транзакции, объект больше недействителен
    }

    public override void LoadFrom(DataTable table)
    {
      OnFinish(); // Если были построчные вызовы Write()
      _BC.WriteToServer(table);
    }

    public override void LoadFrom(DbDataReader reader)
    {
      OnFinish(); // Если были построчные вызовы Write()
      _BC.WriteToServer(reader);
    }

  #endregion
  }
#endif
}
