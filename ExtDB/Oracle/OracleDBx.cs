﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#if !NET // неохота искать пакет

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Data.Common;
using FreeLibSet.IO;
using FreeLibSet.Core;

//#pragma warning disable 1591

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
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public OracleDBx(OracleConnectionStringBuilder connectionStringBuilder)
    {
      _SyncRoot = new object();

      _DatabaseName = GetDataBaseName(connectionStringBuilder);
      _UserId = connectionStringBuilder.UserID;
      if (!String.IsNullOrEmpty(_DatabaseName))
        base.DisplayName = _DatabaseName;

      SetFormatter(new OracleDBxSqlFormatter());

      new OracleDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public OracleDBx(string connectionString)
      : this(new OracleConnectionStringBuilder(connectionString))
    {
    }

    internal static string GetDataBaseName(OracleConnectionStringBuilder connectionStringBuilder)
    {
      // 25.07.2024
      // DataSource имеет формат "Сервер:Порт/БД"
      string s = connectionStringBuilder.DataSource;
      int p = s.LastIndexOf('/');
      if (p >= 0)
        return s.Substring(p + 1);
      else
        return "Unknown";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает имя базы данных
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private readonly string _DatabaseName;

    internal string UserId { get { return _UserId; } }
    private readonly string _UserId;

    private readonly object _SyncRoot;

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new OracleDBxEntry MainEntry { get { return (OracleDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// Создает новое подключение <see cref="OracleDBxEntry"/> с заданными правами
    /// </summary>
    /// <param name="permissions">Права на доступ к таблицам</param>
    /// <returns>Объект для подключения</returns>
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
        lock (_SyncRoot)
        {
          if (_ServerVersionText == null)
          {
            using (OracleDBxCon con = MainEntry.CreateCon() as OracleDBxCon)
            {
              _ServerVersionText = "Oracle " + con.Connection.ServerVersion;
            }
          }
          return _ServerVersionText;
        }
      }
    }
    private string _ServerVersionText;

    /// <summary>
    /// Не реализовано
    /// </summary>
    /// <returns></returns>
    public override long GetDBSize()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает ссылку на <see cref="OracleClientFactory"/>
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return OracleClientFactory.Instance; }
    }

    #endregion

    #region Структура базы данных

    /// <summary>
    /// Проверка не реализована.
    /// Всегда возвращает true.
    /// </summary>
    public override bool DatabaseExists
    {
      get
      {
        /*
        using (OracleDBxCon con = new OracleDBxCon(MainEntry, true))
        {
          DataTable table = con.Connection.GetSchema("Databases");
          using (DataView dv = new DataView(table))
          {
            dv.Sort = "database_name";
            return dv.Find(this.DatabaseName) >= 0;
          }
        }
        */
        return true; 
      }
    }

    /// <summary>
    /// Не реализовано
    /// </summary>
    public override void CreateIfRequired()
    {
      throw new NotSupportedException();
    }

    /// <summary>                               
    /// Обновляет структуру существующей базы
    /// данных на основании созданного описание в свойстве <see cref="DBx.Struct"/>.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство <see cref="ISimpleSplash.PhaseText"/> для отображения выполняемых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    protected override bool OnUpdateStruct(ISimpleSplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Делегируем все действия соединению, т.к. нужен доступ к защищенным методам
      using (OracleDBxCon con = new OracleDBxCon(MainEntry))
      {
        con.CommandTimeout = 0; // Бесконечное время выполнения
        return con.UpdateDBStruct(splash, errors, options);
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

    /*
     * Таблицы и просмотры в Oracle имеют формат "Owner.ИмяТаблицы".
     * Чаще используется обращение к "своим" таблицам без указания owner. 
     * Но иногда требуется в запросах указывать owner
     */

      /// <summary>
      /// Проверка корректности имени таблицы.
      /// Разрешаются имена в формате 
      /// </summary>
      /// <param name="tableName"></param>
      /// <param name="errorText"></param>
      /// <returns>Правильность имени</returns>
    public override bool IsValidTableName(string tableName, out string errorText)
    {
      if (String.IsNullOrEmpty(tableName))
      {
        errorText = Res.DBx_Err_NoTableName;
        return false;
      }

      int p = tableName.IndexOf('.');
      if (p >= 0)
      {
        string user = tableName.Substring(0, p);
        tableName = tableName.Substring(p + 1);
        if (!CheckInvalidChars(tableName, out errorText))
          return false;
        if (!CheckInvalidChars(user, out errorText))
        {
          errorText = String.Format(Res.OracleDBx_Err_OwnerPrefix, errorText);
          return false;
        }
        return true;
      }
      else
        return CheckInvalidChars(tableName, out errorText);
    }

    #endregion
  }

  /// <summary>
  /// Точка подключения к базе данных Oracle с заданными правами доступа к таблицам
  /// </summary>
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

    /// <summary>
    /// Ссылка на объект базы данных
    /// </summary>
    public new OracleDBx DB { get { return (OracleDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal OracleConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private OracleConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создать новое соединение.
    /// Этот метод может вызываться асинхронно.
    /// </summary>
    /// <returns>Соединение с базовй данных</returns>
    public override DBxConBase CreateCon()
    {
      return new OracleDBxCon(this);
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

    #region Строка подключения без пароля

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
    /// <param name="cs">Строка подключения с паролем</param>
    /// <returns>Строка подключения без пароля</returns>
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
        return String.Format(Res.DBx_Err_GetConnectionString, e.Message);
      }
    }

    #endregion
  }

  /// <summary>
  /// Соединение с базой данных Oracle для выполнения запросов.
  /// Используйте <see cref="OracleDBxEntry.CreateCon()"/> для получения соединения.
  /// Используйте блок using для гарантированного закрытия соединения.
  /// </summary>
  public class OracleDBxCon : DBxConBase
  {
    #region Конструктор и Dispose()

    internal OracleDBxCon(OracleDBxEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Закрывает соедиенение ADO.NET, если оно было открыто, и возвращает его в пул.
    /// Удаляет соединение из точки входа.
    /// </summary>
    /// <param name="disposing">True, если был вызван метод <see cref="IDisposable.Dispose()"/>.
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

    /// <summary>
    /// Ссылка на точку подключения
    /// </summary>
    public new OracleDBxEntry Entry { get { return (OracleDBxEntry)(base.Entry); } }

    /// <summary>
    /// Ссылка на базу данных
    /// </summary>
    public new OracleDBx DB { get { return (OracleDBx)(base.DB); } }

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству.
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
               _Connection = new OracleConnection(Entry.ConnectionString);
              DoOpenConnection();
              // SQLExecuteNonQuery("USE [" + DB.DatabaseName + "]");
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
    /// Объект создается при первом обращении к свойству.
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
        e.Data["OracleDBxCon.ConnectionString"] = OracleDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
    }

    /// <summary>
    /// Вызывает <see cref="OracleConnection.ClearPool(OracleConnection)"/>
    /// </summary>
    public override void ClearPool()
    {
#if !MONO
      OracleConnection.ClearPool(Connection);
#endif
      base.ClearPool();
    }

    #endregion

    #region Выполнение SQL-запросов

    /// <summary>
    /// Абстрактный метод выполнения SQL-запроса, возвращающего единственное значение
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
    /// <returns>Количество записей, обработанных в запросе, или (-1), если неизвестно</returns>
    protected override int DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      OracleCommand cmd = new OracleCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Абстрактный метод выполнения SQL-запроса, возвращающего таблицу данных
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
    /// Абстрактный метод выполнения SQL-запроса, возвращающего DbDataReader
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

    private static void InitCmdParameters(OracleCommand cmd, object[] paramValues)
    {
      cmd.Parameters.Clear();
      if (paramValues != null)
      {
        for (int i = 0; i < paramValues.Length; i++)
        {
          OracleParameter sqlParam = new OracleParameter();
          sqlParam.ParameterName = "@P" + (i + 1).ToString();
          sqlParam.Value = paramValues[i];

          if (paramValues[i] != null)
          {
            if (paramValues[i] is Array)
            {
              sqlParam.OracleType = OracleType.Blob;
              sqlParam.DbType = DbType.Binary;
            }
          }
          cmd.Parameters.Add(sqlParam);
        }
      }
    }

    #endregion

    #region Переопределенные реализации выполнения запросов

    #region Добавление записей

    /// <summary>
    /// Не реализовано.
    /// Добавить строку с автоматическим присвоением идентификатора.
    /// Полученный идентификатор возвращается и может быть использован для ссылок на строку.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    /// <returns>Идентификатор добавленной записи</returns>
    public override Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      throw new NotImplementedException();

      // TODO: AddRecordWithIdResult()
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
    /// Реализация группового добавления строк
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую выполняется запись</param>
    /// <param name="table">Таблица исходных данных</param>
    protected override void DoAddRecords(string tableName, DataTable table)
    {
      // TODO: Не реализовано
      base.DoAddRecords(tableName, table);
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
      // Схема Tables содержит 3 столбца:
      // OWNER
      // TABLE_NAME
      // TYPE: "System" или "User"

      // TODO: Надо хранить в OracleDBx список интересующих значений поля "Owner"
      const string filterText = "OWNER NOT LIKE \'SYS%\' AND OWNER<>\'WMSYS\' AND OWNER<>\'MDSYS\' AND OWNER<>\'CTXSYS\'";

      DataTable table1 = SQLExecuteDataTable("SELECT OWNER,TABLE_NAME FROM ALL_TABLES WHERE " + filterText); // так быстрее
      string[] a1 = new string[table1.Rows.Count];
      for (int i = 0; i < table1.Rows.Count; i++)
      {
        DataRow row = table1.Rows[i];
        a1[i] = MakeObjName(DataTools.GetString(row, "OWNER"), DataTools.GetString(row, "TABLE_NAME"));
      }

      // 25.07.2024
      //DataTable table2 = Connection.GetSchema("Views", new string[] { /*DB.DatabaseName*/ });
      DataTable table2 = SQLExecuteDataTable("SELECT OWNER,VIEW_NAME FROM ALL_VIEWS WHERE "+filterText); // так быстрее
      string[] a2 = new string[table2.Rows.Count];
      for (int i = 0; i < table2.Rows.Count; i++)
      {
        DataRow row = table2.Rows[i];
        a2[i] = MakeObjName(DataTools.GetString(row, "OWNER"), DataTools.GetString(row, "VIEW_NAME"));
      }

      return DataTools.MergeArrays<string>(a1, a2);
    }

    private string MakeObjName(string owner, string name)
    {
      if (String.Equals(owner, this.DB.UserId, StringComparison.OrdinalIgnoreCase))
        owner = String.Empty;
      if (String.IsNullOrEmpty(owner))
        return name;
      else
        return owner + "." + name;
    }

    /// <summary>
    /// Получить реальное описание структуры таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct tableStr = new DBxTableStruct(tableName);

      #region Список столбцов, тип, MaxLen, Nullable

      string owner;
      int p = tableName.IndexOf('.');
      if (p >= 0)
      {
        owner = tableName.Substring(0, p);
        tableName = tableName.Substring(p + 1);
      }
      else
        owner = DB.UserId;
      owner = owner.ToUpperInvariant();
      tableName = tableName.ToUpperInvariant();
      DataTable table = Connection.GetSchema("Columns", new string[] { owner, tableName });
      if (table.Rows.Count == 0)
        throw new InvalidOperationException(String.Format(Res.DBxCon_Err_SchemaColumnListIsEmpty, owner + "." + tableName));

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "COLUMN_NAME");
        DBxColumnStruct colDef = new DBxColumnStruct(columnName);

        string colTypeString = DataTools.GetString(drv.Row, "DATATYPE");
        p = colTypeString.IndexOf('(');
        if (p >= 0)
          colTypeString = colTypeString.Substring(0, p); // Может быть "TIMESTAMP(6)"
        int len = DataTools.GetInt(drv.Row, "LENGTH"); // это в байтах
        int prec = DataTools.GetInt(drv.Row, "PRECISION"); // на самом деле имеет тип decimal
        int scale = DataTools.GetInt(drv.Row, "SCALE"); // на самом деле имеет тип decimal

        switch (colTypeString.ToUpperInvariant())
        {
          // TODO: Сделать все типы

          case "VARCHAR2":
          case "NVARCHAR2":
          case "CHAR":
          case "NCHAR":
            colDef.ColumnType = DBxColumnType.String;
            break;
          case "NUMBER":
            if (prec == 0)
            {
              // Тип NUMBER - хранение числа с максимальной точностью
              colDef.ColumnType = DBxColumnType.Float;
            }
            else if (scale == 0) // Тип NUMBER(p)
            {
              colDef.ColumnType = DBxColumnType.Int;
              if (prec > 0 && prec<=18)
              {
                colDef.MaxValue = Math.Pow(10.0, prec) - 1;
                colDef.MinValue = -colDef.MaxValue; // на знак числа не нужен отдельный разряд
              }
            }
            else // Тип NUMBER(p,s)
            {
              colDef.ColumnType = DBxColumnType.Float; // TODO: При некоторых значениях может быть и Int
              // TODO: 
            }
            break;

          case "FLOAT":
            colDef.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            colDef.MinValue = Double.MinValue;
            colDef.MaxValue = Double.MaxValue;
            break;

          case "BINARY_FLOAT":
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Single.MinValue;
            colDef.MaxValue = Single.MaxValue;
            break;

          case "BINARY_DOUBLE":
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Double.MinValue;
            colDef.MaxValue = Double.MaxValue;
            break;

          case "DATE":
          case "TIMESTAMP":
            colDef.ColumnType = DBxColumnType.DateTime;
            break;

          case "BOOLEAN":
            colDef.ColumnType = DBxColumnType.Boolean;
            break;

          case "LONG":
          case "CLOB":
          case "NCLOB":
            colDef.ColumnType = DBxColumnType.Memo;
            break;

          case "RAW":
          case "LONG RAW":
          case "BLOB":
            colDef.ColumnType = DBxColumnType.Binary;
            break;
        }

        colDef.MaxLength = len;

        string nullableStr = DataTools.GetString(drv.Row, "NULLABLE").ToUpperInvariant();
        if (nullableStr == "N")
          colDef.Nullable = false;
        else
          colDef.Nullable = true;

        tableStr.Columns.Add(colDef);
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

      tableStr.SetReadOnly();
      return tableStr;
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

    internal bool UpdateDBStruct(ISimpleSplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
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
      DataTable TableIndexes = Connection.GetSchema("Indexes");
      DataView dvIndexes = new DataView(TableIndexes);

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

        case DBxColumnType.Decimal:
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
      DataTable TableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, Table.TableName });
      foreach (DataRow IndexRow in TableIndexes.Rows)
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

      DataTable TableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, TableName });
      foreach (DataRow IndexRow in TableIndexes.Rows)
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

#endif