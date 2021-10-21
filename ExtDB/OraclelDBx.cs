using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Data.Common;
using FreeLibSet.IO;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#pragma warning disable 1591

namespace FreeLibSet.Data.OracleClient
{
  /// <summary>
  /// База данных Oracle
  /// </summary>
  public class OracleDBx : DBx
  {
    #region Конструктор

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public OracleDBx(OracleConnectionStringBuilder connectionStringBuilder)
    {
      SyncRoot = new object();

      _DatabaseName = GetDataBaseName(connectionStringBuilder);
      if (!String.IsNullOrEmpty(_DatabaseName))
        base.DisplayName = _DatabaseName;

      SetFormatter(new OracleDBxSqlFormatter());

      new OracleDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public OracleDBx(string connectionString)
      : this(new OracleConnectionStringBuilder(connectionString))
    {
    }

    internal static string GetDataBaseName(OracleConnectionStringBuilder connectionStringBuilder)
    {
      return connectionStringBuilder.UserID.ToUpperInvariant(); // !!!! ерунда
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвразает имя базы данных
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private string _DatabaseName;

    protected readonly object SyncRoot;

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new OracleDBxEntry MainEntry { get { return (OracleDBxEntry)(base.MainEntry); } }

    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new OracleDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }


    /// <summary>
    /// Текстовое представление версии сервера
    /// </summary>
    public override string ServerVersionText
    {
      get
      {
        lock (SyncRoot)
        {
          if (_ServerVersionText == null)
          {
            using (OracleDBxCon Con = MainEntry.CreateCon() as OracleDBxCon)
            {
              _ServerVersionText = "Oracle " + Con.Connection.ServerVersion;
            }
          }
          return _ServerVersionText;
        }
      }
    }
    private string _ServerVersionText;

    public override long GetDBSize()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает ссылку на OracleClientFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return OracleClientFactory.Instance; }
    }

    #endregion

    #region Структура базы данных

    public override bool DatabaseExists
    {
      get
      {
        using (OracleDBxCon Con = new OracleDBxCon(MainEntry, true))
        {
          DataTable Table = Con.Connection.GetSchema("Databases");
          using (DataView dv = new DataView(Table))
          {
            dv.Sort = "database_name";
            return dv.Find(this.DatabaseName) >= 0;
          }
        }
      }
    }

    public override void CreateIfRequired()
    {
      throw new NotSupportedException();
    }

    /// <summary>                               
    /// Обновляет структуру существующей базы
    /// данных на основании созданного описание в свойстве DBx.Struct.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство PhaseText для отображения выполненямых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    protected override bool OnUpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Делегируем все действия соединению, т.к. нужен доступ к защищенным методам
      using (OracleDBxCon Con = new OracleDBxCon(MainEntry, false))
      {
        Con.CommandTimeout = 0; // Бесконечное время выполнения
        return Con.UpdateDBStruct(splash, errors, options);
      }
    }

    #endregion

    #region Прочее


    /// <summary>
    /// Не реализовано.
    /// Удаление базы данных, если она существует.
    /// </summary>
    /// <returns>True, если существующая база данных была удалена.
    /// False, если база данных не зарегистрирована</returns>
    public override bool DropDatabaseIfExists()
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  public class OracleDBxEntry : DBxEntry
  {
    #region Конструкторы

    /// <summary>
    /// Защищенный конструктор для основной точки входа
    /// </summary>
    /// <param name="db"></param>
    /// <param name="connectionStringBuilder"></param>
    internal OracleDBxEntry(OracleDBx db, OracleConnectionStringBuilder connectionStringBuilder)
      : base(db, DBxPermissions.FullAccess, true)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionString">Строка подключения</param>
    /// <param name="permissions">Разрешения</param>
    public OracleDBxEntry(OracleDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new OracleConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    /// <param name="permissions">Разрешения</param>
    public OracleDBxEntry(OracleDBx db, OracleConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
      : base(db, permissions, false)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    #endregion

    #region Свойства

    public new OracleDBx DB { get { return (OracleDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal OracleConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private OracleConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создать новое соединение
    /// Этот метод может вызываться асинхронно
    /// </summary>
    /// <returns>Соединение с базовй данных</returns>
    public override DBxConBase CreateCon()
    {
      return new OracleDBxCon(this, false);
    }

    /// <summary>
    /// Создает копию точки входа с другим набором прав
    /// </summary>
    /// <param name="newPermissions">Требуемые разрешения на доступ к объектам базы данных</param>
    /// <returns>Новая точка входа</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new OracleDBxEntry(DB, ConnectionString, newPermissions);
    }

    #endregion

    #region Строка подкючения без пароля

    /// <summary>
    /// Возвращает строку подключения, не содержащую пароль.
    /// </summary>
    public override string UnpasswordedConnectionString
    {
      get
      {
        return GetUnpasswordedConnectionString(ConnectionString);
      }
    }

    /// <summary>
    /// Удаление пароля из строки соединения, если он есть
    /// </summary>
    /// <param name="cs"></param>
    /// <returns></returns>
    internal static string GetUnpasswordedConnectionString(string cs)
    {
      try
      {
        OracleConnectionStringBuilder csb = new OracleConnectionStringBuilder(cs);
        if (!String.IsNullOrEmpty(csb.Password))
        {
          csb.Password = "***";
          cs = csb.ToString();
        }
        return cs;
      }
      catch (Exception e)
      {
        return "Ошибка получения строки подключения. " + e.Message;
      }
    }

    #endregion
  }

  public class OracleDBxCon : DBxConBase
  {
    #region Конструктор и Dispose()

    internal OracleDBxCon(OracleDBxEntry entry, bool serverWide)
      : base(entry)
    {
      _ServerWide = serverWide;
    }

    /// <summary>
    /// Закрывает соедиенение ADO.NET, если оно было открыто, и возвращает его в пул.
    /// Удаляет соединение из точки входа.
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose().
    /// False, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (_Connection != null)
      {
        if (_Connection.State == ConnectionState.Open)
          _Connection.Close();
        _Connection = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    public new OracleDBxEntry Entry { get { return (OracleDBxEntry)(base.Entry); } }

    public new OracleDBx DB { get { return (OracleDBx)(base.DB); } }

    /// <summary>
    /// В текущей реализации не используется 
    /// </summary>
    private bool _ServerWide;

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    public OracleConnection Connection
    {
      get
      {
        if (_Connection == null)
        {
#if DEBUG
          CheckNotDisposed();
#endif
          try
          {
            if (_ServerWide)
            {
              OracleConnectionStringBuilder csb2 = new OracleConnectionStringBuilder();
              csb2.ConnectionString = Entry.ConnectionStringBuilder.ConnectionString;
              throw new NotImplementedException();
              //csb2.InitialCatalog = String.Empty;
              //csb2.AttachDBFilename = String.Empty;

              // TODO:
              /*
            FConnection = new OracleConnection(csb2.ConnectionString);
            DoOpenConnection();
               * */
            }
            else
            {
              _Connection = new OracleConnection(Entry.ConnectionString);
              DoOpenConnection();
              // SQLExecuteNonQuery("USE [" + DB.DatabaseName + "]");
            }
          }
          catch
          {
            _Connection = null; // 27.11.2019
            throw;
          }
        }
        return _Connection;
      }
    }

    private OracleConnection _Connection;

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    protected override DbConnection DbConnection { get { return Connection; } }

    private void DoOpenConnection()
    {
      try
      {
        _Connection.Open();
      }
      catch (Exception e)
      {
        e.Data["OracleDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["OracleDBxCon.ServerWide"] = _ServerWide.ToString();
        e.Data["OracleDBxCon.ConnectionString"] = OracleDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
    }

    /// <summary>
    /// Вызывает OracleConnection.ClearPool()
    /// </summary>
    public override void ClearPool()
    {
      OracleConnection.ClearPool(Connection);
      base.ClearPool();
    }

    #endregion

    #region Выполнение SQL-запросов

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Значение, возвращаемое запросом</returns>
    protected override object DoSQLExecuteScalar(string cmdText, object[] paramValues)
    {
      OracleCommand cmd = new OracleCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteScalar();
    }


    /// <summary>
    /// Выполнение SQL-запроса, не возвращающего значения
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    protected override void DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      OracleCommand cmd = new OracleCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего таблицу данных
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="tableName">Имя таблицы для возвращаемого DataTable</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Заполненная таблица</returns>
    protected override DataTable DoSQLExecuteDataTable(string cmdText, string tableName, object[] paramValues)
    {
      OracleCommand cmd = new OracleCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      OracleDataAdapter da = new OracleDataAdapter(cmd);
      DataTable Table = new DataTable(tableName);
      da.FillError += new FillErrorEventHandler(HandleDataAdapterFillError); // 03.10.2018
      da.Fill(Table);
      return Table;
    }

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего DbDataReader
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Объект для чтения данных</returns>
    protected override DbDataReader DoSQLExecuteReader(string cmdText, object[] paramValues)
    {
      OracleCommand cmd = new OracleCommand(cmdText, Connection);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(OracleCommand cmd, object[] ParamValues)
    {
      cmd.Parameters.Clear();
      if (ParamValues != null)
      {
        for (int i = 0; i < ParamValues.Length; i++)
        {
          OracleParameter Param = new OracleParameter();
          Param.ParameterName = "@P" + (i + 1).ToString();
          Param.Value = ParamValues[i];

          if (ParamValues[i] != null)
          {
            if (ParamValues[i] is Array)
            {
              Param.OracleType = OracleType.Blob;
              Param.DbType = DbType.Binary;
            }
          }
          cmd.Parameters.Add(Param);
        }
      }
    }

    #endregion

    #region Переопределенные реализации выполнения запросов

    #region Добавление записей

    /// <summary>
    /// Не реализовано.
    /// Добавить строку с автоматическим присвоением идентификатора.
    /// Полученный идентификатор возвращается и может быть использован для ссылок на строку
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    /// <returns>Идентификатор добавленной записи</returns>
    public override Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      throw new NotImplementedException();

      // TODO:
#if XXX
      CheckTableName(TableName, DBxAccessMode.Full);
      DBxColumnType[] ColumnTypes = CheckTableColumnNames(TableName, ColumnNames, false, DBxAccessMode.Full);

      if (ColumnNames.Count != Values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(TableName, ColumnNames, Values);

      Int32 Id;

      Buffer.Clear();
      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(TableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(ColumnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(Values, ColumnTypes);
      Buffer.SB.Append(')');
      Buffer.SB.Append("; SELECT @@IDENTITY");


      Id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (Id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + TableName + "\" Id=" + Id.ToString());

      return Id;

#endif
    }

    /// <summary>
    /// Реализация группового добавления строк с помощью <see cref="System.Data.SqlClient.SqlBulkCopy"/> 
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую выполняется запись</param>
    /// <param name="table">Таблица исходных данных</param>
    protected override void DoAddRecords(string tableName, DataTable table)
    {
      // TODO: Не реализовано
      base.AddRecords(tableName, table);
    }

    #endregion

    #endregion

    #region Транзакция

    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public new OracleTransaction CurrentTransaction
    {
      get { return (OracleTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region Извлечение схемы данных

    /// <summary>
    /// Получить полный список таблиц
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable Table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName });
      string[] a = new string[Table.Rows.Count];
      for (int i = 0; i < Table.Rows.Count; i++)
        a[i] = DataTools.GetString(Table.Rows[i], "TABLE_NAME");

      return a;
    }

    /// <summary>
    /// Получить реальное описание структуры таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct TableStr = new DBxTableStruct(tableName);

      #region Список столбцов, тип, MaxLen, Nullable

      DataTable Table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, tableName.ToUpperInvariant() });

      foreach (DataRowView drv in Table.DefaultView)
      {
        string ColumnName = DataTools.GetString(drv.Row, "COLUMN_NAME");
        DBxColumnStruct ColStr = new DBxColumnStruct(ColumnName);

        string ColTypeString = DataTools.GetString(drv.Row, "DATATYPE");
        //int Prec = DataTools.GetInt(drv.Row, "PRECISION"); // на самом деле имеет тип decimal
        int Scale = DataTools.GetInt(drv.Row, "SCALE"); // на самом деле имеет тип decimal

        switch (ColTypeString.ToUpperInvariant())
        {
          // TODO: Сделать все типы

          case "VARCHAR2":
          case "NVARCHAR2":
          case "CHAR":
          case "NCHAR":
            ColStr.ColumnType = DBxColumnType.String;
            break;
          case "NUMBER":
            if (Scale == 0)
            {
              ColStr.ColumnType = DBxColumnType.Int;
              ColStr.MinValue = Int64.MinValue; // !!!!! Думать лень
              ColStr.MaxValue = Int64.MaxValue;
            }
            else
              ColStr.ColumnType = DBxColumnType.Float;
            break;

          case "FLOAT":
            ColStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;

          case "BINARY_FLOAT":
            ColStr.ColumnType = DBxColumnType.Float;
            ColStr.MinValue = Single.MinValue;
            ColStr.MaxValue = Single.MaxValue;
            break;

          case "BINARY_DOUBLE":
            ColStr.ColumnType = DBxColumnType.Float;
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;

          case "DATE":
          case "TIMESTAMP":
            ColStr.ColumnType = DBxColumnType.DateTime;
            break;

          case "LONG":
          case "CLOB":
          case "NCLOB":
            ColStr.ColumnType = DBxColumnType.Memo;
            break;

          case "RAW":
          case "LONG RAW":
          case "BLOB":
            ColStr.ColumnType = DBxColumnType.Binary;
            break;
        }

        ColStr.MaxLength = DataTools.GetInt(drv.Row, "LENGTH"); // это в байтах

        string NullableStr = DataTools.GetString(drv.Row, "NULLABLE").ToUpperInvariant();
        if (NullableStr == "N")
          ColStr.Nullable = false;
        else
          ColStr.Nullable = true;

        TableStr.Columns.Add(ColStr);
      }

      #endregion

#if XXX
      #region Определение ссылочных полей

      int TableObjId = GetTableObjId(TableName);
      if (TableObjId == 0)
        throw new BugException("Не удалось получить идентификатор object_id таблицы \"" + TableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append("SELECT [name],[object_id],[referenced_object_id],[delete_referential_action] FROM sys.foreign_keys WHERE parent_object_id=");
      Buffer.FormatValue(TableObjId, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "sys.foreign_keys");
      foreach (DataRow Row in tbl.Rows)
      {
        int FKObjId = DataTools.GetInt(Row, "object_id");
        int RefTableObjId = DataTools.GetInt(Row, "referenced_object_id");

      Buffer.Clear();
        Buffer.SB.Append("SELECT [parent_column_id] FROM sys.foreign_key_columns WHERE constraint_object_id=");
        Buffer.FormatValue(FKObjId, DBxColumnType.Int);
        // TODO: Нужно сделать метод SQLExecuteScalarSingle(), который будет работать как ExecuteScalar(), но с проверкой количества данных в DataReader'е
        DataTable tbl2 = SQLExecuteDataTable(Buffer.SB.ToString(), "sys.foreign_key_columns");
        if (tbl2.Rows.Count == 0)
          throw new BugException("Не найдено ни одно столбца для ограничения с constraint_object_id=" + FKObjId.ToString());
        if (tbl2.Rows.Count > 1)
          throw new BugException("Ограничение с constraint_object_id=" + FKObjId.ToString() + " содержит несколько столбцов (" + tbl2.Rows.Count.ToString() + ")");
        int ParentColumnId = DataTools.GetInt(tbl2.Rows[0], "parent_column_id");

      Buffer.Clear();
        Buffer.SB.Append("SELECT [name] FROM sys.columns WHERE object_id=");
        Buffer.FormatValue(TableObjId, DBxColumnType.Int);
        Buffer.SB.Append(" AND column_id=");
        Buffer.FormatValue(ParentColumnId, DBxColumnType.Int);
        string ParentColumnName = DataTools.GetString(SQLExecuteScalar(Buffer.SB.ToString()));
        if (String.IsNullOrEmpty(ParentColumnName))
          throw new BugException("Не удалось определить имя ссылочного поля");

      Buffer.Clear();
        Buffer.SB.Append("SELECT [name] FROM sys.tables WHERE object_id=");
        Buffer.FormatValue(RefTableObjId, DBxColumnType.Int);
        string RefTableName = DataTools.GetString(SQLExecuteScalar(Buffer.SB.ToString()));
        if (String.IsNullOrEmpty(RefTableName))
          throw new BugException("Не удалось определить имя ссылочной таблицы для RefTableId=" + RefTableObjId.ToString());

        DBxColumnStruct ColStr = TableStr.Columns[ParentColumnName];
        ColStr.MasterTableName = RefTableName;

        int RefTypeCode = DataTools.GetInt(Row, "delete_referential_action");
        switch (RefTypeCode)
        {
          case 0: ColStr.RefType = DBxRefType.Disallow; break;
          case 1: ColStr.RefType = DBxRefType.Delete; break;
          case 2: ColStr.RefType = DBxRefType.Clear; break;
          case 3: ColStr.RefType = DBxRefType.Clear; break; // устанавливается значение по умолчанию
        }
      }

      #endregion
#endif

      TableStr.SetReadOnly();
      return TableStr;
    }

#if XXX
    private int GetTableObjId(string tableName)
    {
      Buffer.Clear();
      Buffer.SB.Append("SELECT [object_id] FROM sys.tables WHERE [Name]=");
      Buffer.FormatValue(tableName, DBxColumnType.String);
      return DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));
    }
#endif

    #endregion

    #region Выполнение служебных команд

    internal void CreateDatabase(AbsPath filePath)
    {
      // TODO:
      throw new NotImplementedException();
#if XXX
      Buffer.Clear();
      Buffer.SB.Append("CREATE DATABASE [");
      Buffer.SB.Append(FilePath.Path);
      Buffer.SB.Append("] ON PRIMARY (NAME='File1', FILENAME=");
      Buffer.FormatValue(FilePath.Path, DBxColumnType.String);
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
#endif
    }

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // TODO:
      throw new NotImplementedException();
#if XXX
      bool Modified = false;

      #region Извлечение информации из существующей схемы данных

      DataTable TableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(TableTables);
      dvTables.Sort = "TABLE_NAME";

      // Столбцы
      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME";

      // Данные по внешним ключам
      DataTable TableForeignKeys = Connection.GetSchema("ForeignKeys");
      DataView dvForeignKeys = new DataView(TableForeignKeys);
      dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // Данные по индексам 
      DataTable TableIndices = Connection.GetSchema("Indexes");
      DataView dvIndices = new DataView(TableIndices);

      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME,ORDINAL_POSITION";
      //DebugTools.DebugDataView(dvIndexColumns, "Столбцы индексов базы данных "+DB.DisplayName);

      #endregion

      Splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (Table.Columns.Count == 0)
          throw new DBxStructException(Table, "Не задано ни одного столбца");

        //CheckPrimaryKeyColumn(Table, Table.Columns[0]);

        if (dvTables.Find(Table.TableName) < 0)
        {
      #region Требуется полное создание таблицы

          Splash.PhaseText = "Создается таблица \"" + Table.TableName + "\"";
          CreateTable(Table);
          Errors.AddInfo("Создана таблица \"" + Table.TableName + "\"");
          Modified = true;

          #endregion
        }
        else
        {
      #region Проверяем правильность первичного ключа

          if (CorrectPrimaryKey(Table, dvIndexColumns, Errors))
            Modified = true;

          #endregion

      #region Проверяем наличие недостающих полей

          foreach (DBxColumnStruct Column in Table.Columns)
          {
            int ColumnRowIndex = dvColumns.Find(new object[] { Table.TableName, Column.ColumnName });
            if (ColumnRowIndex < 0)
            {
              // Поля не существует
              Splash.PhaseText = "Добавление поля \"" + Column.ColumnName + "\"в таблицу \"" + Table.TableName + "\"";
      Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE [");
              Buffer.SB.Append(Table.TableName);
              Buffer.SB.Append("] ADD "); // а не ADD COLUMN
              AppendColumnDef(Table, Column);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              Errors.AddInfo("Создано поле \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName + "\"");
              Modified = true;
            }
            else
            {
              // Проверяем соответствие поля
              DataRow ColumnRow = dvColumns[ColumnRowIndex].Row;
              // Проверяем соответствие типа столбца объявлению
              string RealType = DataTools.GetString(ColumnRow, "DATA_TYPE").ToUpperInvariant();
              string WantedType = GetSqlServerType(Table, Column);
              int p = WantedType.IndexOf('(');
              if (p >= 0)
                WantedType = WantedType.Substring(0, p);

              if (RealType != WantedType)
              {
                Errors.AddError("Несоответствие типа поля \"" + Column.ColumnName + "\" таблицы \"" +
                    Table.TableName + "\". Объявление поля типа " + Column.ColumnType.ToString() +
                    " предполагает тип  " + WantedType +
                    " в то время как реальный тип поля " + RealType);
              }
              else
              {
      #region Проверка длины строкового поля

                if (Column.ColumnType == DBxColumnType.String)
                {
                  int RealLen = DataTools.GetInt(ColumnRow, "CHARACTER_MAXIMUM_LENGTH");
                  if (RealLen != Column.MaxLength)
                  {
                    if (RealLen > Column.MaxLength)
                    {
                      // !!! Проверка, нельзя ли укоротить поле
                      Errors.AddWarning("Поле \"" + Column.ColumnName + "\" таблицы \"" +
                          Table.TableName + "\" должно иметь длину " + Column.MaxLength.ToString() +
                          " символов, в то время, как реальное поле длиннее:  " + RealLen.ToString() + " символов");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // Лучше пересоздать все индексы
                      Errors.AddInfo("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены из-за изменения размера поля \"" + Column.ColumnName + "\"");
                      if (DeleteAllIndices(Table.TableName, Splash, Errors))
                        Modified = true;

                      // Увеличиваем длину поля
                      Splash.PhaseText = "Изменение длины поля \"" + Column.ColumnName + "\" в таблице \"" + Table.TableName + "\"";
                      AlterColumn(Table, Column);
                      Errors.AddInfo("Длина поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                        "\" увеличена с " + RealLen.ToString() + " до " + Column.MaxLength.ToString() + " символов");
                      Modified = true;
                    }
                  }
                } // Строковое поле

      #endregion

      #region Проверка признака Nullable

                // Проверяем Nullable
                string s1 = DataTools.GetString(ColumnRow, "IS_NULLABLE").ToUpperInvariant();
                bool RealNullable;
                switch (s1)
                {
                  case "YES":
                    RealNullable = true;
                    break;
                  case "NO":
                    RealNullable = false;
                    break;
                  default:
                    RealNullable = Column.Nullable;
                    break;
                }

                if (Column.Nullable != RealNullable)
                {
                  if (DeleteAllIndices(Table.TableName, Splash, Errors))
                    Modified = true;

                  // Делаем поле NULLABLE
                  AlterColumn(Table, Column);
                  Errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                    "\" установлен признак " + (Column.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  Modified = true;
                }

      #endregion
              }
            } // поле существует
          } // Цикл по столбцам

          #endregion
        }

        // Таблица существует


#if XXX         // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

          if (!CreateIndices)
          {
            // Проверяем индексы
            dvIndices.RowFilter = "TABLE_NAME='" + Table.TableName + "'";
            // В отличие от столбцов, все лишние индексы уничтожаются
            bool[] Flags = new bool[Table.Indices.Count];
            DataTools.FillArray<bool>(Flags, false);

            foreach (DataRowView drvIndex in dvIndices)
            {
              string RealIndexName = DataTools.GetString(drvIndex.Row, "CONSTRAINT_NAME");
              if (RealIndexName == "PK_" + Table.TableName ||
                RealIndexName == "FK_" + Table.TableName + "_DocId")
                continue; // первичный ключ не трогаем
              if (!RealIndexName.StartsWith("Index"))
              {
                Actions.Add("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены, т.к. обнаружен посторонний индекс \"" + RealIndexName + "\"");
                CreateIndices = true;
                break;
              }

              int pos;
              if (!int.TryParse(RealIndexName.Substring(5), out pos))
              {
                Actions.Add("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены, т.к. обнаружен посторонний индекс \"" + RealIndexName + "\"");
                CreateIndices = true;
                break;
              }
              if (pos < 1 || pos > Table.Indices.Count)
              {
                // Был лишний индекс 
                Actions.Add("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены, т.к. обнаружен индекс \"" + RealIndexName + "\", который не объявлен");
                CreateIndices = true;
                break;
              }


              DBStruct.Index IndexDef = Table.Indices[pos - 1];
              Flags[pos - 1] = true;
              // Проверяем поля
              dvIndexColumns.RowFilter = "TABLE_NAME='" + Table.TableName + "' AND INDEX_NAME='" + RealIndexName + "'";
              string[] FieldNames = IndexDef.FieldNames.Split(',');
              if (dvIndexColumns.Count != FieldNames.Length)
              {
                CreateIndices = true;
                Actions.Add("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены, т.к. в индексе \"" + RealIndexName + "\" количество полей (" + dvIndices.Count.ToString() +
                  ") не совпадает с объявленным (" + FieldNames.Length + ")");
                break;
              }
              for (int i = 0; i < FieldNames.Length; i++)
              {
                string RealFieldName = DataTools.GetString(dvIndexColumns[i].Row, "COLUMN_NAME");
                if (RealFieldName != FieldNames[i])
                {
                  string[] RealFieldNames = new string[dvIndexColumns.Count];
                  for (int j = 0; j < RealFieldNames.Length; j++)
                    RealFieldNames[j] = DataTools.GetString(dvIndexColumns[j].Row, "COLUMN_NAME");
                  Actions.Add("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены, т.к. в существующем индексе \"" + RealIndexName +
                    "\" имена полей (" + String.Join(",", RealFieldNames) +
                    ") не совпадают с объявленными (" + IndexDef.FieldNames + ")");
                  CreateIndices = true;
                  break;
                }
              }
              if (CreateIndices)
                break;
            } // Цикл по описателям реальных индексов
            if (CreateIndices)
            {
              if (DeleteAllIndices(Table.TableName))
                Modified = true;
            }
            else
            {
              // Создаем недостающие индексы
              for (int i = 0; i < Table.Indices.Count; i++)
              {
                if (!Flags[i])
                {
                  CreateIndex(Table, i);
                  Modified = true;
                }
              }
            }

#endif

#if XXX
        if (CreateIndices)
        {
          // Все существующие индексы таблицы удалены, требуется все создать заново
          for (int i = 0; i < Table.Indices.Count; i++)
          {
            CreateIndex(Table, i);
            Modified = true;
          }
        }
#endif

        // Удаление лишних индексов
        List<string> GoodIndices = new List<string>();
        RemoveExtraIndices(Table, GoodIndices, Errors, Splash, Options.DropUnusedIndices);
        CreateIndices(Table, GoodIndices, Errors, Splash);

        Splash.PhaseText = String.Empty;
        Splash.IncPercent();
      } // Цикл по таблицам

      // Внешние ключи можно создавать только после создания всех таблиц
      if (Options.ForeignKeys)
      {
        Splash.PhaseText = "Проверка внешних ключей";
        foreach (DBxTableStruct Table in DB.Struct.Tables)
        {
          if (UpdateForeignKeys(Table, dvForeignKeys, Splash, Errors))
            Modified = true;
        }
      }

      Splash.PhaseText = String.Empty;


      Splash.PercentMax = 0;
      return Modified;
#endif
    }

#if XXX // TODO:
    /*
    private static void CheckPrimaryKeyColumn(DBxTableStruct Table, DBxColumnStruct Column)
    {
      switch (Column.ColumnType)
      {
        case DBxColumnType.Int:
        case DBxColumnType.Guid:
          break;
        default:
          throw new DBxStructException(Table, "Первым столбцом таблицы должен быть объявлен первичный ключ. Столбец \"" +
            Column.ColumnName + "\" имеет недопустимый тип \"" + Column.ColumnType.ToString() + "\"");
      }

      if (Column.Nullable)
        throw new DBxStructException(Table, "Первым столбцом таблицы должен быть объявлен первичный ключ. Столбец \"" +
          Column.ColumnName + "\" не может быть объявлен как Nullable");

    }
     * */

    private void CreateTable(DBxTableStruct Table)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE TABLE [");
      Buffer.SB.Append(Table.TableName);
      Buffer.SB.Append("] (");
      for (int i = 0; i < Table.Columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        AppendColumnDef(Table, Table.Columns[i]);

        if (Table.Columns[i].ColumnName == Table.PrimaryKey[0])
        {
          Buffer.SB.Append(" IDENTITY CONSTRAINT [PK_");
          Buffer.SB.Append(Table.TableName);
          Buffer.SB.Append("] PRIMARY KEY");
        }
      }
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private string GetSqlServerType(DBxTableStruct Table, DBxColumnStruct Column)
    {
      switch (Column.ColumnType)
      {
        case DBxColumnType.String:
          if (Column.MaxLength < 1)
            throw new DBxStructException(Table, "Для строкового поля \"" + Column.ColumnName + "\" должна быть задана длина поля");

          return "NCHAR(" + Column.MaxLength.ToString() + ")";

        case DBxColumnType.Boolean:
          return "BIT";

        case DBxColumnType.Int:
          if (Column.MinValue == 0 && Column.MaxValue == 0)
            return "INT";
          else if (Column.MinValue >= 0 && Column.MaxValue <= 255)
            return "TINYINT";
          else if (Column.MinValue >= Int16.MinValue && Column.MaxValue <= Int16.MaxValue)
            return "SMALLINT";
          else if (Column.MinValue >= Int32.MinValue && Column.MaxValue <= Int32.MaxValue)
            return "INT";
          else
            return "BIGINT";

        case DBxColumnType.Float:
          if (Column.MinValue == 0 && Column.MaxValue == 0)
            return "FLOAT";
          else if (Column.MinValue >= Single.MinValue && Column.MaxValue <= Single.MaxValue)
            return "REAL";
          else
            return "FLOAT";

        case DBxColumnType.Money:
          return "MONEY";

        case DBxColumnType.Date:
          // Только дата
          return "DATE";

        case DBxColumnType.DateTime:
          // Дата и время
          return "DATETIME2(0)";

        case DBxColumnType.Time:
          return "TIME(0)";


        case DBxColumnType.Memo:
          return "NVARCHAR(MAX)";

        case DBxColumnType.Xml:
          return "XML";

        case DBxColumnType.Binary:
          return "VARBINARY(MAX)";

        default:
          throw new BugException("Неизвестный тип поля " + Column.ColumnType.ToString());
      }
    }

    private void AppendColumnDef(DBxTableStruct Table, DBxColumnStruct Column)
    {
      Buffer.SB.Append('[');
      Buffer.SB.Append(Column.ColumnName);
      Buffer.SB.Append("] ");

      Buffer.SB.Append(GetSqlServerType(Table, Column));

      if (Column.Nullable)
        Buffer.SB.Append(" NULL");
      else
        Buffer.SB.Append(" NOT NULL");
    }

    /// <summary>
    /// Выполнение ALTER TABLE ALTER COLUMN
    /// </summary>
    /// <param name="Table"></param>
    /// <param name="Column"></param>
    private void AlterColumn(DBxTableStruct Table, DBxColumnStruct Column)
    {
      Buffer.Clear();
      Buffer.SB.Append("ALTER TABLE [");
      Buffer.SB.Append(Table.TableName);
      Buffer.SB.Append("] ALTER COLUMN ");
      AppendColumnDef(Table, Column);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Проверяет наличие первичного ключа. Неправильное ограничение первичного ключа удаляется,
    /// новое ограничение создается
    /// </summary>
    /// <param name="Table"></param>
    /// <param name="dvIndexColumns"></param>
    /// <param name="Errors"></param>
    /// <returns>true, если были внесены изменения</returns>
    private bool CorrectPrimaryKey(DBxTableStruct Table, DataView dvIndexColumns, ErrorMessageList Errors)
    {
      if (Table.PrimaryKey.Count != 1)
        throw new NotSupportedException("В таблице должен быть первичный ключ из одного поля");

      bool Modified = false;
      bool Found = false;
      dvIndexColumns.RowFilter = "TABLE_NAME='" + Table.TableName + "' AND COLUMN_NAME='" + Table.PrimaryKey[0] + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string IndexName = DataTools.GetString(drvCol.Row, "CONSTRAINT_NAME");
        if (IndexName == "PK_" + Table.TableName)
          Found = true;
        else
        {
          if (IndexName.StartsWith("Index"))
            continue; // составной пользовательский индекс, в который входит поле "Id"
          SQLExecuteNonQuery("ALTER TABLE [" + Table.TableName + "] DROP CONSTRAINT [" + IndexName + "]");
          Errors.AddInfo("Удалено неправильное ограничение первичного ключа \"" + IndexName + "\" в таблице \"" + Table.TableName + "\"");
          Modified = true;
        }
      }

      if (!Found)
      {
        string PKName = "PK_" + Table.TableName;
        SQLExecuteNonQuery("ALTER TABLE [" + Table.TableName + "] ADD CONSTRAINT [" + PKName + "] PRIMARY KEY ([" + Table.Columns[0].ColumnName + "])");
        Errors.AddInfo("Добавлено ограничение первичного ключа \"" + PKName + "\" в таблице \"" + Table.TableName + "\"");
        Modified = true;
      }

      return Modified;
    }

    private bool UpdateForeignKeys(DBxTableStruct Table, DataView dvForeignKeys, ISplash Splash, ErrorMessageList Errors)
    {
      foreach (DBxColumnStruct Column in Table.Columns)
      {
        if (String.IsNullOrEmpty(Column.MasterTableName))
          continue;

        string FKName = "FK_" + Table.TableName + "_" + Column.ColumnName;
        if (dvForeignKeys.Find(FKName) >= 0)
        {
          // Ключ существует
          // TODO: Проверка соответствия параметров ключа и удаление его


          continue;
        }

        // Создаем внешний ключ
        Splash.PhaseText = "Создание внешнего ключа " + FKName;

      Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE [");
        Buffer.SB.Append(Table.TableName);
        Buffer.SB.Append("] ADD CONSTRAINT [");
        Buffer.SB.Append(FKName);
        Buffer.SB.Append("] FOREIGN KEY (");
        Buffer.FormatColumnName(Column.ColumnName);
        Buffer.SB.Append(") REFERENCES [");
        Buffer.SB.Append(Column.MasterTableName);
        Buffer.SB.Append("]");
        switch (Column.RefType)
        {
          case DBxRefType.Disallow:
            Buffer.SB.Append(" ON DELETE NO ACTION");
            break;
          case DBxRefType.Delete:
            Buffer.SB.Append(" ON DELETE CASCADE");
            break;
          case DBxRefType.Clear:
            Buffer.SB.Append(" ON DELETE SET NULL");
            break;
        }

        SQLExecuteNonQuery(Buffer.SB.ToString());
        Errors.AddInfo("Создан внешний ключ \"" + FKName + "\" в таблице \"" + Table.TableName + "\"");
      }

      return false;
    }

    #region Индексы

    private void RemoveExtraIndices(DBxTableStruct Table, List<string> GoodIndices, ErrorMessageList Errors, ISplash Splash, bool Drop)
    {
      DataTable TableIndices = Connection.GetSchema("Indexes", new string[] { null, null, Table.TableName });
      foreach (DataRow IndexRow in TableIndices.Rows)
      {
        string IndexName = DataTools.GetString(IndexRow, "INDEX_NAME");
        //        if (IndexName == "PrimaryKey")
        //          continue;
        if (IndexName.StartsWith("FK_") || IndexName.StartsWith("PK_"))
          continue;

        if (!Table.Indices.Contains(IndexName))
        {
          // Лишний индекс
          if (Drop)
          {
            Splash.PhaseText = "Удаление индекса " + IndexName;
            DropIndex(Table.TableName, IndexName);
            Errors.AddInfo("Удален лишний индекс \"" + IndexName + "\" в таблице \"" + Table.TableName + "\"");
          }
          continue;
        }

        DBxIndexStruct IndexDef = Table.Indices[IndexName];

        DataTable TableIndexColumns = Connection.GetSchema("IndexColumns", new string[] { null, null, Table.TableName, IndexName });
        if (!CheckIndexColumns(IndexDef, TableIndexColumns))
        {
          // Описание индекса не соответствует
          Splash.PhaseText = "Удаление индекса " + IndexName;
          DropIndex(Table.TableName, IndexName);
          Errors.AddInfo("Удален индекс \"" + IndexName + "\" в таблице \"" + Table.TableName + "\", т.к. он не соответствует объявленному в структуре данных");
          continue;
        }

        // Индекс хороший
        GoodIndices.Add(IndexDef.IndexName);
      }
    }

    private static bool CheckIndexColumns(DBxIndexStruct IndexDef, DataTable TableIndexColumns)
    {
      if (IndexDef.Columns.Count != TableIndexColumns.Rows.Count)
        return false;

      for (int i = 0; i < IndexDef.Columns.Count; i++)
      {
        if (IndexDef.Columns[i] != DataTools.GetString(TableIndexColumns.Rows[i], "COLUMN_NAME"))
          return false;
      }

      return true;
    }

    private void DropIndex(string TableName, string IndexName)
    {
      Buffer.Clear();
      Buffer.SB.Append("DROP INDEX [");
      Buffer.SB.Append(IndexName);
      Buffer.SB.Append("] ON [");
      Buffer.SB.Append(TableName);
      Buffer.SB.Append("]");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void CreateIndices(DBxTableStruct Table, List<string> GoodIndices, ErrorMessageList Errors, ISplash Splash)
    {
      for (int i = 0; i < Table.Indices.Count; i++)
      {
        DBxIndexStruct IndexDef = Table.Indices[i];

        if (GoodIndices.Contains(IndexDef.IndexName))
          continue;

        Splash.PhaseText = "Создание индекса таблицы \"" + Table.TableName + "\" для полей \"" + IndexDef.Columns.ToString() + "\"";
        CreateIndex(Table.TableName, IndexDef.IndexName, IndexDef.Columns);
        Errors.AddInfo("Создан индекс \"" + IndexDef.IndexName + "\" в таблице \"" + Table.TableName + "\" с полями \"" + IndexDef.Columns.ToString() + "\"");
      }
    }

    /// <summary>
    /// Добавление одного индекса
    /// </summary>
    private void CreateIndex(string TableName, string IndexName, DBxColumns Columns)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE INDEX [");
      Buffer.SB.Append(IndexName);
      Buffer.SB.Append("] ON [");
      Buffer.SB.Append(TableName);
      Buffer.SB.Append("] (");
      for (int i = 0; i < Columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.SB.Append("[");
        Buffer.SB.Append(Columns[i]);
        Buffer.SB.Append("]");
      }
      Buffer.SB.Append(")");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

#if XXX
    private void CreateFK(DBStruct.Table TableDef)
    {
      DBStruct.Field FieldDef = TableDef.Fields["DocId"];
      string FKName = "FK_" + TableDef.TableName + "_DocId";

      SetPhaseText("Создание внешнего ключа таблицы \"" + TableDef.TableName + "\"");

#if DEBUG
      if (FieldDef.FieldType != DBStruct.AccDepFieldType.Reference)
        throw new InvalidOperationException("Поле \"" + FieldDef.FieldName + "\" не является ссылочным");

      // Если имя Primary таблицы не задано, то возникает ошибка доступа к памяти в OLE DB
      if (String.IsNullOrEmpty(FieldDef.MasterTableName))
        throw new InvalidOperationException("Для поля \"" + FieldDef.FieldName + "\" не задана мастер-таблица");
#endif

      sb.Length = 0;
      sb.Append("ALTER TABLE [");
      sb.Append(TableDef.TableName);
      sb.Append("] ADD CONSTRAINT [");
      sb.Append(FKName);
      sb.Append("] FOREIGN KEY ([DocId]) REFERENCES [");
      sb.Append(FieldDef.MasterTableName);
      sb.Append("]");

      DB.SQLExecuteNonQuery(sb.ToString());
      Actions.Add("Создан внешний ключ \"" + FKName + "\" в таблице \"" + TableDef.TableName + "\"");
    }
#endif

    private bool DeleteAllIndices(string TableName, ISplash Splash, ErrorMessageList Errors)
    {
      // Перебираем все существующие индексы
      // Один индекс может занимать несколько строк
      // Создаем список индексов для удаления
      List<string> IndexNames = null;

      DataTable TableIndices = Connection.GetSchema("Indexes", new string[] { null, null, TableName });
      foreach (DataRow IndexRow in TableIndices.Rows)
      {
        string IndexName = DataTools.GetString(IndexRow, "INDEX_NAME");
        //        if (IndexName == "PrimaryKey")
        //          continue;
        if (IndexName.StartsWith("FK_") || IndexName.StartsWith("PK_"))
          continue;

        // Добавляем индекс в список на удаление
        if (IndexNames == null)
          IndexNames = new List<string>();
        if (!IndexNames.Contains(IndexName))
          IndexNames.Add(IndexName);
      }

      if (IndexNames == null)
        return false;

      Splash.PhaseText = "Удаление индексов таблицы \"" + TableName + "\"";
      for (int i = 0; i < IndexNames.Count; i++)
      {
      Buffer.Clear();
        Buffer.SB.Append("DROP INDEX [");
        Buffer.SB.Append(IndexNames[i]);
        Buffer.SB.Append("] ON [");
        Buffer.SB.Append(TableName);
        Buffer.SB.Append("]");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        Errors.AddInfo("Удален индекс \"" + IndexNames[i] + "\" в таблице \"" + TableName + "\"");
      }
      return true;
    }

    #endregion

#if XXX

    private bool DeleteAllForeignKeys(string TableName, out bool FKDocIdExists, ISplash Splash, ErrorMessageList Errors)
    {
      List<string> FKNames = null;

      FKDocIdExists = false;

      DataTable TableFKs = Connection.GetSchema("ForeignKeys");
      foreach (DataRow FKRow in TableFKs.Rows)
      {
        string ThisTableName = DataTools.GetString(FKRow, "TABLE_NAME");
        if (ThisTableName != TableName)
          continue;
        string FKName = DataTools.GetString(FKRow, "CONSTRAINT_NAME");
        // TODO: Нужно ли пропускать
        /*
        if (FKName == "FK_" + TableName + "_DocId")
        {
          FKDocIdExists = true;
          continue;
        }
         * */

        // Добавляем индекс в список на удаление
        if (FKNames == null)
          FKNames = new List<string>();
        if (!FKNames.Contains(FKName))
          FKNames.Add(FKName);
      }

      if (FKNames == null)
        return false;

      Splash.PhaseText = "Удаление внешних ключей таблицы \"" + TableName + "\"";
      for (int i = 0; i < FKNames.Count; i++)
      {
      Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE [");
        Buffer.SB.Append(TableName);
        Buffer.SB.Append("] DROP CONSTRAINT [");
        Buffer.SB.Append(FKNames[i]);
        Buffer.SB.Append("]");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        Errors.AddInfo("Удален внешний ключ \"" + FKNames[i] + "\" из таблицы \"" + TableName + "\"");
      }

      return true;
    }
#endif

#endif

    #endregion
  }
}
