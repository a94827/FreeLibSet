// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Data;
using FreeLibSet.IO;
using System.Data.Common;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Npgsql
{
  /// <summary>
  /// База данных PostGreSQL через провайдер Npgsql.
  /// Для использования должны быть подключены сборки Npgsql.dll и Mono.Security из подкаталога FreeLibSet/Others
  /// </summary>
  public class NpgsqlDBx : DBx
  {
    #region Конструктор

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public NpgsqlDBx(NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
      _SyncRoot = new object();

      _DatabaseName = GetDataBaseName(connectionStringBuilder);
      if (!String.IsNullOrEmpty(_DatabaseName))
        base.DisplayName = _DatabaseName;

      SetFormatter(new
        NpgsqlDBxSqlFormatter());

      new NpgsqlDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public NpgsqlDBx(string connectionString)
      : this(new NpgsqlConnectionStringBuilder(connectionString))
    {
    }

    internal static string GetDataBaseName(NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
      if (!String.IsNullOrEmpty(connectionStringBuilder.Database))
        return connectionStringBuilder.Database;
      else
        throw new ArgumentException("В строке подключения не задан параметр Database, определяющий базу данных", "connectionStringBuilder");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвразает имя базы данных
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private readonly string _DatabaseName;

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new NpgsqlDBxEntry MainEntry { get { return (NpgsqlDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// Создает точку входа
    /// </summary>
    /// <param name="permissions">Разрешения на доступ к базе данных</param>
    /// <returns></returns>
    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new NpgsqlDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }


    private readonly object _SyncRoot;

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
            using (NpgsqlDBxCon con = MainEntry.CreateCon() as NpgsqlDBxCon)
            {
              _ServerVersionText = "PostGreSQL " + con.Connection.ServerVersion;
            }
          }
          return _ServerVersionText;
        }
      }
    }
    private string _ServerVersionText;

    /// <summary>
    /// Вызывает встроенную функцию pg_database_size()
    /// </summary>
    /// <returns>Размер базы данных в байтах</returns>
    public override long GetDBSize()
    {
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, false))
      {
        return DataTools.GetInt64(con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
      }
    }

    /// <summary>
    /// Возвращает ссылку на NpgsqlFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return NpgsqlFactory.Instance; }
    }

    /// <summary>
    /// Возвращает статический экзеипляр менеджера баз данных PostgreSQL
    /// </summary>
    public override DBxManager Manager { get { return NpgsqlDBxManager.TheManager; } }

    #endregion

    #region Обновление структуры

    /// <summary>
    /// Возвращает true, если база данных существует
    /// </summary>
    public override bool DatabaseExists
    {
      get
      {
        using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, true))
        {
          DataTable table = con.Connection.GetSchema("Databases");
          using (DataView dv = new DataView(table))
          {
            dv.Sort = "database_name";
            return dv.Find(this.DatabaseName) >= 0;
          }
        }
      }
    }
    /// <summary>
    /// Создает базу данных, если ее не существует
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, true))
      {
        con.CommandTimeout = 0; // Бесконечное время выполнения
        con.CreateDatabase();
      }
    }

    /// <summary>                               
    /// Обновляет структуру существующей базы
    /// данных на основании созданного описание в свойстве <see cref="DBx.Struct"/>.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство PhaseText для отображения выполненямых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    protected override bool OnUpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Делегируем все действия соединению, т.к. нужен доступ к защищенным методам
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, false))
      {
        con.CommandTimeout = 0; // Бесконечное время выполнения
        return con.UpdateDBStruct(splash, errors, options);
      }
    }


    /// <summary>
    /// Удаление базы данных, если она существует
    /// </summary>
    /// <returns>True, если существующая база данных была удалена.
    /// False, если база данных не зарегистрирована</returns>
    public override bool DropDatabaseIfExists()
    {
      if (!DatabaseExists)
        return false;

      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, true))
      {
        con.DropDatabase();
      }

      return true;
    }

    /// <summary>
    /// Удаляет таблицу данных, если она существует.
    /// Этот метод должен вызываться до установки свойства DBx.Struct и вызова UpdateStruct().
    /// Если обновление структуры не предполагается, после последовательности вызовов этого метода,
    /// должна быть выполнена установка DB.Struct=null, чтобы обновить список таблиц
    /// </summary>
    /// <param name="tableName">Имя удаляемой таблицы</param>
    public override void DropTableIfExists(string tableName)
    {
      using (DBxConBase con = MainEntry.CreateCon())
      {
        con.NameCheckingEnabled = false;
        DBxSqlBuffer buffer = new DBxSqlBuffer(this.Formatter);
        con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        buffer.SB.Append("DROP TABLE IF EXISTS ");
        buffer.FormatTableName(tableName);
        con.SQLExecuteNonQuery(buffer.SB.ToString());
      }
    }

    #endregion
  }

  /// <summary>
  /// Точка входа для базы данных PostgreSql
  /// </summary>
  public class NpgsqlDBxEntry : DBxEntry
  {
    #region Конструкторы

    /// <summary>
    /// Защищенный конструктор для основной точки входа
    /// </summary>
    /// <param name="db"></param>
    /// <param name="connectionStringBuilder"></param>
    internal NpgsqlDBxEntry(NpgsqlDBx db, NpgsqlConnectionStringBuilder connectionStringBuilder)
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
    public NpgsqlDBxEntry(NpgsqlDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new NpgsqlConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionStringBuilder">Строка подключения</param>
    /// <param name="permissions">Разрешения</param>
    public NpgsqlDBxEntry(NpgsqlDBx db, NpgsqlConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
      : base(db, permissions, false)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// База данных
    /// </summary>
    public new NpgsqlDBx DB { get { return (NpgsqlDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal NpgsqlConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private readonly NpgsqlConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private readonly string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создать новое соединение
    /// Этот метод может вызываться асинхронно
    /// </summary>
    /// <returns>Соединение с базовй данных</returns>
    public override DBxConBase CreateCon()
    {
      return new NpgsqlDBxCon(this, false);
    }

    /// <summary>
    /// Создает копию точки входа с другим набором прав
    /// </summary>
    /// <param name="newPermissions">Требуемые разрешения на доступ к объектам базы данных</param>
    /// <returns>Новая точка входа</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new NpgsqlDBxEntry(DB, ConnectionString, newPermissions);
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
        NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder(cs);
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

  /// <summary>
  /// Соединение с базой данных PostgreSQL
  /// </summary>
  public class NpgsqlDBxCon : DBxConBase
  {
    #region Конструктор и Dispose

    internal NpgsqlDBxCon(NpgsqlDBxEntry entry, bool serverWide)
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

    /// <summary>
    /// Точка входа
    /// </summary>
    public new NpgsqlDBxEntry Entry { get { return (NpgsqlDBxEntry)(base.Entry); } }

    /// <summary>
    /// База данных
    /// </summary>
    public new NpgsqlDBx DB { get { return (NpgsqlDBx)(base.DB); } }

    /// <summary>
    /// Если true, то подключение используется для работы со списком баз данных. 
    /// </summary>
    private readonly bool _ServerWide;

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    public NpgsqlConnection Connection
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
              NpgsqlConnectionStringBuilder csb2 = new NpgsqlConnectionStringBuilder();
              csb2.ConnectionString = Entry.ConnectionStringBuilder.ConnectionString;
              csb2.Database = String.Empty;
              _Connection = new NpgsqlConnection(csb2.ConnectionString);
              DoOpenConnection();
            }
            else
            {
              _Connection = new NpgsqlConnection(Entry.ConnectionString);
              DoOpenConnection();
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

    private NpgsqlConnection _Connection;

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
        //char[] a = e.Message.ToCharArray();
        e.Data["NpgsqlDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["NpgsqlDBxCon.ServerWide"] = _ServerWide.ToString();
        e.Data["NpgsqlDBxCon.ConnectionString"] = NpgsqlDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        e.Data["NpgsqlDBxCon.Remark"] = @"Если сообщение об ошибке не читается (сердечки вместо букв), настройте сервер на выдачу сообщений на английском языке в файле postgresql.conf. Задайте строку <lc_messages = 'en_EN.UTF-8'>";
        throw;
      }
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
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
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
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
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
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
      DataTable table = new DataTable(tableName);
      da.FillError += new FillErrorEventHandler(HandleDataAdapterFillError); // 03.10.2018
      da.Fill(table);
      return table;
    }

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего DbDataReader
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Объект для чтения данных</returns>
    protected override DbDataReader DoSQLExecuteReader(string cmdText, object[] paramValues)
    {
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(NpgsqlCommand cmd, object[] paramValues)
    {
      cmd.Parameters.Clear();
      if (paramValues != null)
      {
        for (int i = 0; i < paramValues.Length; i++)
        {
          NpgsqlParameter sqlParam = new NpgsqlParameter();
          sqlParam.ParameterName = "@P" + (i + 1).ToString();
          sqlParam.Value = paramValues[i];

          if (paramValues[i] != null)
          {
            if (paramValues[i] is Array)
            {
              //Param.SqlDbType = SqlDbType.VarBinary;
              sqlParam.DbType = DbType.Binary;
            }
          }
          cmd.Parameters.Add(sqlParam);
        }
      }
    }

    #endregion

    #region Переопределенные реализации выполнения запросов

    #region Подсчет строк

    /// <summary>
    /// Оптимизированная версия запроса проверки, что таблица пуста.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Пустая таблица?</returns>
    public override bool IsTableEmpty(string tableName)
    {
      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);

      Buffer.Clear();
      Buffer.SB.Append("SELECT EXISTS(SELECT * FROM ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(")");

      return !DataTools.GetBool(SQLExecuteScalar(Buffer.SB.ToString()));
    }

    #endregion

    #region Добавление записей

    /// <summary>
    /// Добавить строку с автоматическим присвоением идентификатора.
    /// Полученный идентификатор возвращается и может быть использован для ссылок на строку
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    /// <returns>Идентификатор добавленной записи</returns>
    public override Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      DBxColumnType[] ColumnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      string PrimaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Int32 id;

      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, ColumnTypes);
      Buffer.SB.Append(')');

      //DBxTableStruct ts = DB.Struct.Tables[tableName];
      Buffer.SB.Append(" RETURNING ");
      Buffer.FormatColumnName(PrimaryKeyColumnName);

      id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + tableName + "\" Id=" + id.ToString());

      return id;
    }

    /// <summary>
    /// Добавление строки в таблицу.
    /// Значения могут содержать, а могут и не содержать первичный ключ
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов</param>
    /// <param name="values">Значения</param>
    public override void AddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      base.AddRecord(tableName, columnNames, values);

      // Корректируем последовательности для первичного ключа
      int seqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, columnNames);
      if (seqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.GetInt(values[seqColumnIndex]));
    }

    /// <summary>
    /// Режимы форматирования значения поля при групповом добавлении записей в AddRecords
    /// </summary>
    private enum CopyFormattingMode
    {
      /// <summary>
      /// Форматирование значение выполняется методом NpgsqlDBxSqlFormatter.FormatValue()
      /// </summary>
      FormatValue,

      /// <summary>
      /// Форматирование выполняется с помощью NpgsqlCopySerializer.AddString()
      /// </summary>
      String,

      /// <summary>
      /// Форматирование выполняется с помощью NpgsqlCopySerializer.AddString(), но сначала строка преобразуется
      /// в Guid, а затем обратно в строку, чтобы убрать возможное форматирование
      /// </summary>
      Guid,
    }

    /// <summary>
    /// Групповое добавление записей с использованием NpgsqlCopyIn и NpgsqlCopySerializer
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются строки</param>
    /// <param name="table">Исходная таблица</param>
    protected override void DoAddRecords(string tableName, DataTable table)
    {
      if (table.Rows.Count <= 1)
      {
        base.DoAddRecords(tableName, table);
        return;
      }

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      if (TrimValues)
        PerformTrimValues(tableName, table);

      DBxColumns columnNames = DBxColumns.FromColumns(table.Columns);
      Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      DBxTableStruct ts = DB.Struct.Tables[tableName];

      #region Определяем режимы копирования

      CopyFormattingMode[] Modes = new CopyFormattingMode[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
      {
        DBxColumnStruct colDef = ts.Columns[columnNames[i]];

        switch (colDef.ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Date:
          case DBxColumnType.DateTime:
          case DBxColumnType.Time:
          case DBxColumnType.Money:
            Modes[i] = CopyFormattingMode.FormatValue;
            break;
          case DBxColumnType.String:
          case DBxColumnType.Xml:
          case DBxColumnType.Memo: // 18.12.2020
            Modes[i] = CopyFormattingMode.String;
            break;
          case DBxColumnType.Guid: // 08.10.2019 - для GUID'а не нужно добавлять апострофы
            Modes[i] = CopyFormattingMode.Guid;
            break;
          default:
            // Неизвестный формат.
            // Используем построчное добавление
            base.DoAddRecords(tableName, table); // исправлено 18.12.2020
            return;
        }
      }

      #endregion

      // Образец использования взял из http://stackoverflow.com/questions/11824967/how-to-use-npgsqlcopyin-with-npgsqlcopyserializer

      Buffer.Clear();
      Buffer.SB.Append("COPY ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") FROM STDIN");

      NpgsqlCommand cmd = new NpgsqlCommand(Buffer.SB.ToString(), Connection);
      NpgsqlCopySerializer serializer = new NpgsqlCopySerializer(Connection);
      NpgsqlCopyIn copyIn = new NpgsqlCopyIn(cmd, Connection, serializer.ToStream);

      copyIn.Start();

      string s;
      foreach (DataRow row in table.Rows)
      {
        for (int i = 0; i < columnNames.Count; i++)
        {
          if (row.IsNull(i))
            serializer.AddNull();
          else
          {
            DBxColumnStruct colDef = ts.Columns[columnNames[i]];

            switch (Modes[i])
            {
              case CopyFormattingMode.FormatValue:
                Buffer.Clear();
                Buffer.FormatValue(row[i], colDef.ColumnType); // сами форматируем
                serializer.AddString(Buffer.SB.ToString());
                break;
              case CopyFormattingMode.String:
                s = DataTools.GetString(row[i]);
                serializer.AddString(s); // пусть он форматирует
                break;
              case CopyFormattingMode.Guid: // 08.10.2019
                s = DataTools.GetString(row[i]);
                Guid g = new Guid(s);
                s = g.ToString("D");
                serializer.AddString(s);
                break;
              default:
                throw new BugException();
            }
          }
        }
        serializer.EndRow();
      }
      serializer.Flush();

      copyIn.End();

      int seqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, columnNames);
      if (seqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.MaxInt(table, columnNames[seqColumnIndex], true) ?? 0);
    }

    #region Корректировка последовательности

    // 09.06.2017
    // Проблема.
    // В отличие от MS SQL Server, числовой первичный ключ реализуется через "последовательность",
    // которая является самостоятельной сущностью. Если при добавлении записи задается значение
    // для поля автинкрементного поля, то последовательность "отстанет". При следующем добавлении
    // записи без указания поля первичного ключа, возникнет ошибка повторения значения.
    //
    // При выполнении AddRecord() и AddRecords() с полем первичного ключа, вызываем встроенную
    // функцию setval(), чтобы "подтянуть" значение

    /// <summary>
    /// Ключ - имя таблицы
    /// Значение - имя последовательности, используемой полем первичного ключа
    /// </summary>
    private Dictionary<string, string> _TablePKSequenceNames;


    /// <summary>
    /// "Подтягивание" идентификатора для последовательности
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="usedId"></param>
    private void CorrectPrimaryKeySequence(string tableName, Int32 usedId)
    {
      if (_TablePKSequenceNames == null)
        _TablePKSequenceNames = new Dictionary<string, string>();
      string SeqName;
      if (!_TablePKSequenceNames.TryGetValue(tableName, out SeqName))
      {
        SeqName = GetTablePKSequenceName(tableName);
        _TablePKSequenceNames.Add(tableName, SeqName);
      }

      string primaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);
      Int32 lastId = DataTools.GetInt(GetMaxValue(tableName, primaryKeyColumnName, null));
      usedId = Math.Max(lastId, usedId);

      StringBuilder sb = new StringBuilder();
      sb.Append("SELECT setval(\'");
      sb.Append(SeqName);
      sb.Append("\', ");
      sb.Append(usedId.ToString());
      sb.Append(", true)");

      SQLExecuteNonQuery(sb.ToString());
    }

    private string GetTablePKSequenceName(string tableName)
    {
      string primaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);
      StringBuilder sb = new StringBuilder();

      // Имя таблицы идет и в апострофах и в кавычках
      // А имя столбца - только в апострофах
      // Х.З. почему
      sb.Append("SELECT pg_get_serial_sequence(\'\"");
      sb.Append(tableName);
      sb.Append("\"\', \'");
      sb.Append(primaryKeyColumnName);
      sb.Append("\')");

      string seqName = (string)(SQLExecuteScalar(sb.ToString()));

      return seqName;
    }

    #endregion

    #endregion

    #region Удаление записей

    /// <summary>
    /// Удаление всех строк таблицы. Выполняет SQL-запрос "TRUNCATE TABLE".
    /// </summary>
    /// <param name="tableName">Имя очищаемой таблицы</param>
    public override void DeleteAll(string tableName)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.Full);

      Buffer.SB.Append("TRUNCATE TABLE ");
      Buffer.FormatTableName(tableName);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    #endregion

    #endregion

    #region Транзакция

    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public new NpgsqlTransaction CurrentTransaction
    {
      get { return (NpgsqlTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region Извлечение схемы данных

    /// <summary>
    /// Получить полный список таблиц
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName, "public" });
      string[] a = new string[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
        a[i] = DataTools.GetString(table.Rows[i], "table_name");

      return a;
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

      DataTable table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "public", tableName });
      table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct colStr = new DBxColumnStruct(columnName);

        string colTypeString = DataTools.GetString(drv.Row, "data_type");
        switch (colTypeString)
        {
          case "char":
          case "nchar":
          case "bpchar":
            colStr.ColumnType = DBxColumnType.String;
            break;

          case "bigint":
          case "int8":
            colStr.ColumnType = DBxColumnType.Int;
            colStr.MinValue = Int64.MinValue;
            colStr.MaxValue = Int64.MaxValue;
            break;
          case "int":
          case "int4":
            colStr.ColumnType = DBxColumnType.Int;
            colStr.MinValue = Int32.MinValue;
            colStr.MaxValue = Int32.MaxValue;
            break;
          case "smallint":
          case "int2":
            colStr.ColumnType = DBxColumnType.Int;
            colStr.MinValue = Int16.MinValue;
            colStr.MaxValue = Int16.MaxValue;
            break;
          case "tinyint":
            colStr.ColumnType = DBxColumnType.Int;
            colStr.MinValue = 0;
            colStr.MaxValue = 255;
            break;

          case "float":
          case "float4":
            colStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            colStr.MinValue = Double.MinValue;
            colStr.MaxValue = Double.MaxValue;
            break;
          case "real":
          case "float8":
            colStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            colStr.MinValue = Single.MinValue;
            colStr.MaxValue = Single.MaxValue;
            break;

          case "money":
            colStr.ColumnType = DBxColumnType.Money;
            colStr.MinValue = -922337203685477.5808;
            colStr.MaxValue = 922337203685477.5807;
            break;
          case "smallmoney":
            colStr.ColumnType = DBxColumnType.Money;
            colStr.MinValue = -214748.3648;
            colStr.MaxValue = 214748.3647;
            break;

          case "bit":
          case "bool":
            colStr.ColumnType = DBxColumnType.Boolean;
            break;

          case "date":
            colStr.ColumnType = DBxColumnType.Date;
            break;

          case "datetime":
          case "smalldatetime":
          case "datetimeoffset": // ???
          case "timestamp":
          case "datetime2": colStr.ColumnType = DBxColumnType.DateTime; break;

          case "time": colStr.ColumnType = DBxColumnType.Time; break;

          case "varchar":
          case "nvarchar":
          case "text":
          case "ntext":
            colStr.ColumnType = DBxColumnType.Memo;
            break;

          case "image":
          case "varbinary":
            colStr.ColumnType = DBxColumnType.Binary;
            break;

          case "binary":
            colStr.ColumnType = DBxColumnType.Binary;
            //ColStr.MaxLength=
            break;

          case "xml":
            colStr.ColumnType = DBxColumnType.Xml;
            break;

          case "uuid": // 06.10.2021
            colStr.ColumnType = DBxColumnType.Guid;
            break;

          default:
            break;
        }

        colStr.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string nullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (nullableStr) // 01.10.2019
        {
          case "YES": colStr.Nullable = true; break;
          case "NO": colStr.Nullable = false; break;
        }

        tableStr.Columns.Add(colStr);
      }

      #endregion

      #region Определение ссылочных полей (ограничения FOREIGN KEY)

      //DataTable Table2 = Connection.GetSchema();
      //string[]aaa=DataTools.GetStringsFromField(Table2, "CollectionName");

      //Table = Connection.GetSchema("Restrictions", new string[] { DB.DatabaseName, "public", TableName });
      //foreach (DataRow r in Table.Rows)
      //{ 
      //}


      int tableOID = GetTableOID(tableName);
      if (tableOID == 0)
        throw new BugException("Не удалось получить идентификатор object_id таблицы \"" + tableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append(@"SELECT confrelid,confdeltype,conkey FROM pg_catalog.pg_constraint WHERE contype='f' AND conrelid=");
      Buffer.FormatValue(tableOID, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "pg_constraint");
      foreach (DataRow row in tbl.Rows)
      {
        Int32 refTableOID = DataTools.GetInt(row, "confrelid");
        string refTableName = GetTableNameFromOID(refTableOID);
        if (String.IsNullOrEmpty(refTableName))
          throw new BugException("Не найдено имя для мастер-таблицы с OID=" + refTableOID);

        Int16[] detColPoss = (Int16[])(row["conkey"]);
        // не нужен Int16[] RefColPoss = (Int16[])(Row["confkey"]);
        if (detColPoss.Length != 1 /*|| RefColPoss.Length != 1*/)
          continue; // FOREIGN KEY по нескольким полям не поддерживаются

        DBxColumnStruct colStr = tableStr.Columns[detColPoss[0] - 1];
        colStr.MasterTableName = refTableName; // 01.10.2019

        string refTypeCode = DataTools.GetString(row, "confdeltype");
        switch (refTypeCode)
        {
          case "c": colStr.RefType = DBxRefType.Delete; break;
          case "n": colStr.RefType = DBxRefType.Clear; break;
          default: colStr.RefType = DBxRefType.Disallow; break;
        }

      }

      #endregion

      tableStr.SetReadOnly();
      return tableStr;
    }

    #endregion

    #region Получение информации

    /// <summary>
    /// Возвращает идентификатор таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Идентификатор OID</returns>
    public Int32 GetTableOID(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      // Имя таблицы идет в апострофах, а не в кавычках, т.к. это - строка
      return DataTools.GetInt(SQLExecuteScalar("SELECT oid FROM pg_catalog.pg_class WHERE relname=\'" + tableName + "\' AND relkind=\'r\'"));
    }

    /// <summary>
    /// Возвращает имя таблицы по идентификатору
    /// </summary>
    /// <param name="oid">Идентификатор OID</param>
    /// <returns>Имя таблицы</returns>
    public string GetTableNameFromOID(Int32 oid)
    {
      if (oid == 0)
        throw new ArgumentException("OID=0", "oid");

      return DataTools.GetString(SQLExecuteScalar("SELECT relname FROM pg_catalog.pg_class WHERE oid=" + oid.ToString() + " AND relkind=\'r\'"));
    }

    #endregion

    #region CreateDatabase

    internal void CreateDatabase()
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE DATABASE \"");
      Buffer.SB.Append(DB.DatabaseName);
      Buffer.SB.Append("\"");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    internal void DropDatabase()
    {
      NpgsqlConnection.ClearAllPools();

      Buffer.Clear();
      Buffer.SB.Append("DROP DATABASE \"");
      Buffer.SB.Append(DB.DatabaseName);
      Buffer.SB.Append("\"");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    #endregion

    #region Обновление структуры

    #region Основной метод UpdateDBStruct

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool modified = false;

      // Индексы и ограничения.
      // В отличие от MS SQL Server, требуется, чтобы имена индексов были уникальны в пределах
      // базы данных, а не в пределах таблицы
      // В официальной документации рекомендуется давать простым индексам имена "ИмяТаблицы_ИмяПоля_index"
      // В тоже время, правило нарушается для составных индексов, например "test2_mm_idx".
      // К тому же, ограничение на 31 символ никто не отменял
      //
      // Поэтому, используем правила:
      // 1. Для первичных ключей используем стандартное имя ограничения "ИмяТаблицы_pkey".
      //    Отдельный индекс не создается
      // 2. Для внешних ключей создаются индексы с произвольными именами, назначаемыми СУБД,
      //    а поиск выполняем в системной таблице pg_constraitns по имени таблицы и поля
      // 3. Для индексов используем имена, назначаемые СУБД "ИмяТаблицы_ИмяПоля".
      //    Свойство DBxIndexDef.IndexName не учитывается

      if (UpdateDBStructTables(splash, errors))
        modified = true;

      if (options.ForeignKeys)
      {
        if (UpdateDBStructForeignKeys(splash, errors))
          modified = true;
      }

      if (UpdateDBStructIndices(splash, errors, options))
        modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return modified;
    }

    #endregion

    #region Добавление таблиц, столбцов и первичных ключей

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;

      #region Извлечение информации из существующей схемы данных

      DataTable tableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(tableTables);
      dvTables.Sort = "TABLE_NAME";

      // Столбцы
      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // нужен такой порядок

      // Данные по индексам 
      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";
      //DebugTools.DebugDataView(dvIndexColumns, "Столбцы индексов базы данных "+DB.DisplayName);

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        if (table.Columns.Count == 0)
          throw new DBxStructException(table, "Не задано ни одного столбца");
        //CheckPrimaryKeyColumn(Table, Table.PrimaryKeyColumns[0]);

        if (dvTables.Find(table.TableName) < 0)
        {
          #region Требуется полное создание таблицы

          splash.PhaseText = "Создается таблица \"" + table.TableName + "\"";
          CreateTable(table);
          errors.AddInfo("Создана таблица \"" + table.TableName + "\"");
          modified = true;

          #endregion
        }
        else
        {
          #region Проверяем правильность первичного ключа

          if (CorrectPrimaryKey(table, dvIndexColumns, errors))
            modified = true;

          #endregion

          #region Проверяем наличие недостающих полей

          DBxSqlBuffer buffer2 = new DBxSqlBuffer(Buffer.Formatter);

          foreach (DBxColumnStruct colDef in table.Columns)
          {
            int columnRowIndex = dvColumns.Find(new object[] { table.TableName, colDef.ColumnName });
            if (columnRowIndex < 0)
            {
              // Поля не существует
              splash.PhaseText = "Добавление поля \"" + colDef.ColumnName + "\"в таблицу \"" + table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE ");
              Buffer.FormatTableName(table.TableName);
              Buffer.SB.Append(" ADD "); // а не ADD COLUMN
              AppendColumnDef(/*Table, */colDef, false, true);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("Создано поле \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName + "\"");
              modified = true;
            }
            else
            {
              // Проверяем соответствие поля
              DataRow columnRow = dvColumns[columnRowIndex].Row;
              // Проверяем соответствие типа столбца объявлению
              string realType = DataTools.GetString(columnRow, "DATA_TYPE").ToUpperInvariant();
              realType = ReplaceSynonymousPostGreSqlServerType(realType);
              buffer2.Clear();
              FormatValueType(buffer2, colDef, false);
              string wantedType = buffer2.SB.ToString();
              int p = wantedType.IndexOf('(');
              if (p >= 0)
                wantedType = wantedType.Substring(0, p);

              if (realType != wantedType)
              {
                errors.AddError("Несоответствие типа поля \"" + colDef.ColumnName + "\" таблицы \"" +
                    table.TableName + "\". Объявление поля типа " + colDef.ColumnType.ToString() +
                    " предполагает тип " + wantedType +
                    " в то время как реальный тип поля " + realType);
              }
              else
              {
                #region Проверка длины строкового поля

                if (colDef.ColumnType == DBxColumnType.String)
                {
                  int realLen = DataTools.GetInt(columnRow, "CHARACTER_MAXIMUM_LENGTH");
                  if (realLen != colDef.MaxLength)
                  {
                    if (realLen > colDef.MaxLength)
                    {
                      // !!! Проверка, нельзя ли укоротить поле
                      errors.AddWarning("Поле \"" + colDef.ColumnName + "\" таблицы \"" +
                          table.TableName + "\" должно иметь длину " + colDef.MaxLength.ToString() +
                          " символов, в то время, как реальное поле длиннее:  " + realLen.ToString() + " символов");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // Лучше пересоздать все индексы
                      errors.AddInfo("Все существующие индексы таблицы \"" + table.TableName + "\" будут удалены из-за изменения размера поля \"" + colDef.ColumnName + "\"");
                      if (DeleteAllIndices(table.TableName, splash, errors))
                        modified = true;

                      // Увеличиваем длину поля
                      splash.PhaseText = "Изменение длины поля \"" + colDef.ColumnName + "\" в таблице \"" + table.TableName + "\"";
                      AlterColumn(table, colDef, true, false);
                      errors.AddInfo("Длина поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                        "\" увеличена с " + realLen.ToString() + " до " + colDef.MaxLength.ToString() + " символов");
                      modified = true;
                    }
                  }
                } // Строковое поле

                #endregion

                #region Проверка Default

                // Установка свойства DEFAULT должна выполняться до установки NOT NULL, иначе возникнет ошибка

                string wantedDefExpr = String.Empty;
                if (colDef.DefaultExpression != null)
                {
                  buffer2.Clear();
                  buffer2.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
                  wantedDefExpr = buffer2.SB.ToString();
                }
                string realDefExpr = DataTools.GetString(columnRow, "COLUMN_DEFAULT");
                if (realDefExpr.StartsWith("nextval(", StringComparison.OrdinalIgnoreCase))
                  realDefExpr = String.Empty; // иначе изгадим первичный ключ
                if (realDefExpr != wantedDefExpr)
                {
                  // Вседа сначала убираем старое правило, потом добавляем новое
                  if (realDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(colDef.ColumnName);
                    Buffer.SB.Append(" DROP DEFAULT");

                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                        "\" очищен признак DEFAULT");
                  }
                  if (wantedDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(colDef.ColumnName);
                    Buffer.SB.Append("SET DEFAULT ");
                    Buffer.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
                    SQLExecuteNonQuery(Buffer.SB.ToString());

                    errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                      "\" установлен признак DEFAULT " + wantedDefExpr);
                  }
                  modified = true;
                }

                #endregion

                #region Проверка признака Nullable

                // Проверяем Nullable
                string s1 = DataTools.GetString(columnRow, "IS_NULLABLE").ToUpperInvariant();

                bool realNullable;
                switch (s1)
                {
                  case "YES":
                    realNullable = true;
                    break;
                  case "NO":
                    realNullable = false;
                    break;
                  default:
                    realNullable = colDef.Nullable;
                    break;
                }

                if (colDef.Nullable != realNullable)
                {
                  if (DeleteAllIndices(table.TableName, splash, errors))
                    modified = true;

                  #region Замена NULL'ов на DEFAULT

                  if ((!colDef.Nullable) && colDef.DefaultExpression != null && (!IsTableEmpty(table.TableName)))
                  {
                    //SetValue(Table.TableName, new ValueFilter(Column.ColumnName, null, CompareKind.Equal, Column.ColumnType),Column.ColumnName, Column.Default)
                    // Заменяем значения NULL на значение по умолчанию
                    Buffer.Clear();
                    Buffer.SB.Append("UPDATE ");
                    Buffer.FormatTableName(table.TableName);
                    Buffer.SB.Append(" SET ");
                    Buffer.FormatColumnName(colDef.ColumnName);
                    Buffer.SB.Append("=");
                    Buffer.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
                    Buffer.SB.Append(" WHERE ");
                    Buffer.FormatColumnName(colDef.ColumnName);
                    Buffer.SB.Append(" IS NULL");
                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                      "\" значения NULL заменены на значение по умолчанию");
                  }

                  #endregion

                  // Делаем поле NULLABLE
                  AlterColumn(table, colDef, false, true);
                  errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                    "\" установлен признак " + (colDef.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  modified = true;
                }

                #endregion
              }
            } // поле существует
          } // Цикл по столбцам

          #endregion
        }

        // Таблица существует

        #region Комментарии

        SetTableComment(table.TableName, table.Comment);
        for (int i = 0; i < table.Columns.Count; i++)
          SetColumnComment(table.TableName, table.Columns[i].ColumnName, table.Columns[i].Comment);


        #endregion

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      return modified;
    }

    private void CreateTable(DBxTableStruct table)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE TABLE ");
      Buffer.FormatTableName(table.TableName);
      Buffer.SB.Append(" (");
      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        AppendColumnDef(/*table, */table.Columns[i], i == 0, true);
      }

      if (table.PrimaryKey.Count > 0)
      {
        Buffer.SB.Append(", CONSTRAINT \"" + table.TableName + "_pkey\" PRIMARY KEY (");
        for (int i = 0; i < table.PrimaryKey.Count; i++)
        {
          if (i > 0)
            Buffer.SB.Append(",");
          Buffer.FormatColumnName(table.PrimaryKey[i]);
        }
        Buffer.SB.Append(")");
      }
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column, bool isPrimaryKey)
    {
      if (isPrimaryKey)
      {
        switch (column.ColumnType)
        {
          case DBxColumnType.Int:
            if (column.MinValue == 0 && column.MaxValue == 0)
              buffer.SB.Append("SERIAL"); // с автоинкрементом
            else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
              buffer.SB.Append("SERIAL"); // с автоинкрементом
            else
              buffer.SB.Append("BIGSERIAL");
            return;
        }
      }
      buffer.FormatValueType(column);
    }

    #region Синонимы типы данных

    /// <summary>
    /// Ключ - тип данных, который может возвращать PostGreSQL
    /// Значение - "основной" тип данных, который мы используем в библиотеке
    /// </summary>
    private static readonly Dictionary<string, string> _SynonymousPostGreSqlServerTypeReplaces =
      CreateSynonymousPostGreSqlServerTypeReplaces();

    private static Dictionary<string, string> CreateSynonymousPostGreSqlServerTypeReplaces()
    {
      Dictionary<string, string> d = new Dictionary<string, string>();
      d.Add("INT2", "SMALLINT");
      d.Add("INT4", "INTEGER");
      d.Add("INT8", "BIGINT");
      d.Add("BOOL", "BOOLEAN");
      //d.Add("BPCHAR", "CHARACTER"); // blank-padded char
      d.Add("BPCHAR", "CHAR"); // исправлено 20.09.2019
      d.Add("CHARACTER", "CHAR"); // 30.12.2019
      return d;
    }

    private static string ReplaceSynonymousPostGreSqlServerType(string realType)
    {
      string mainType;
      if (_SynonymousPostGreSqlServerTypeReplaces.TryGetValue(realType, out mainType))
        return mainType;
      else
        return realType;
    }

    #endregion

    private void AppendColumnDef(/*DBxTableStruct table, */DBxColumnStruct column, bool isPrimaryKey, bool addDefault)
    {
      Buffer.FormatColumnName(column.ColumnName);
      Buffer.SB.Append(" ");
      FormatValueType(Buffer, column, isPrimaryKey);

      if (column.Nullable)
        Buffer.SB.Append(" NULL");
      else
        Buffer.SB.Append(" NOT NULL");

      if (addDefault && column.DefaultExpression != null)
      {
        Buffer.SB.Append(" DEFAULT ");
        Buffer.FormatExpression(column.DefaultExpression, new DBxFormatExpressionInfo());
      }
    }

    /// <summary>
    /// Выполнение ALTER TABLE ALTER COLUMN
    /// </summary>
    private void AlterColumn(DBxTableStruct table, DBxColumnStruct column, bool setType, bool setNullable)
    {
      Buffer.Clear();
      Buffer.SB.Append("ALTER TABLE ");
      Buffer.FormatTableName(table.TableName);
      Buffer.SB.Append(" ALTER COLUMN ");
      Buffer.FormatColumnName(column.ColumnName);

      if (setType)
      {
        Buffer.SB.Append(" TYPE");
        Buffer.SB.Append(" ");
        FormatValueType(Buffer, column, false);
      }
      if (setNullable)
      {
        if (column.Nullable)
          Buffer.SB.Append(" DROP NOT NULL");
        else
          Buffer.SB.Append(" SET NOT NULL");
      }
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

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

    } */

    /// <summary>
    /// Проверяет наличие первичного ключа. Неправильное ограничение первичного ключа удаляется,
    /// новое ограничение создается
    /// </summary>
    /// <param name="table"></param>
    /// <param name="dvIndexColumns"></param>
    /// <param name="errors"></param>
    /// <returns>true, если были внесены изменения</returns>
    private bool CorrectPrimaryKey(DBxTableStruct table, DataView dvIndexColumns, ErrorMessageList errors)
    {
      if (table.PrimaryKey.Count != 1)
        throw new NotSupportedException("В таблице должен быть первичный ключ из одного поля");

      bool modified = false;
      bool found = false;
      string pkName = table.TableName + "_pkey";

      dvIndexColumns.RowFilter = "TABLE_NAME='" + table.TableName + "' AND INDEX_NAME='" + pkName + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string columnName = DataTools.GetString(drvCol.Row, "COLUMN_NAME");
        if (String.Equals(columnName, table.PrimaryKey[0], StringComparison.OrdinalIgnoreCase))
          found = true;
        else
        {
          // 13.10.2021.
          // Ограничение может иметь имя в любом регистре.
          // ALTER TABLE DROP CONSTRAINT требует указания имени ограничения с учетом регистра.
          // Берем имя, как оно записано в базе данных.
          string pkName2 = DataTools.GetString(drvCol.Row, "INDEX_NAME"); 

          //if (IndexName.StartsWith("Index"))
          //  continue; // составной пользовательский индекс, в который входит поле "Id"
          SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" DROP CONSTRAINT \"" + pkName2 + "\"");
          errors.AddInfo("Удалено неправильное ограничение первичного ключа \"" + pkName2 + "\" в таблице \"" + table.TableName + "\"");
          modified = true;
          break;
        }
      }

      if (!found)
      {
        SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" ADD CONSTRAINT \"" + pkName + "\" PRIMARY KEY (\"" + table.PrimaryKey[0] + "\")");
        errors.AddInfo("Добавлено ограничение первичного ключа \"" + pkName + "\" в таблице \"" + table.TableName + "\"");
        modified = true;
      }

      return modified;
    }

    #endregion

    #region Комментарии

    private void SetTableComment(string tableName, string comment)
    {

      if (!String.IsNullOrEmpty(comment))
      {
        Buffer.Clear();
        Buffer.SB.Append("COMMENT ON TABLE ");
        Buffer.FormatTableName(tableName);
        Buffer.SB.Append(" IS ");
        Buffer.FormatValue(comment, DBxColumnType.String);
        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    private void SetColumnComment(string tableName, string columnName, string comment)
    {
      if (!String.IsNullOrEmpty(comment))
      {
        Buffer.Clear();
        Buffer.SB.Append("COMMENT ON COLUMN "); // а не ADD COLUMN
        Buffer.FormatTableName(tableName);
        Buffer.SB.Append(".");
        Buffer.FormatColumnName(columnName);
        Buffer.SB.Append(" IS ");
        Buffer.FormatValue(comment, DBxColumnType.String);
        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    #endregion

    #region Внешние ключи

    private bool UpdateDBStructForeignKeys(ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;

      #region Создаем TableForeignKeys

      // Внешние ключи можно создавать только после создания всех таблиц
      // Так нельзя получить столбцы для ограничения
      //// Данные по внешним ключам
      //DataTable TableForeignKeys = Connection.GetSchema("ForeignKeys");
      //DataView dvForeignKeys = new DataView(TableForeignKeys);
      //dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // Столбцы
      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION";

      // Используем системную таблицу pg_constraint
      // 0: conname - имя ограчения (для удаления, если надо будет)
      // 1: conrelid::regclass - имя таблицы в кавычках
      // 2: conkey - массив индексов столбцов ссылочных полей (нас интересуют только ограничения из одного элемента)
      // contype='f' - фильтр по ограничениям внешнего ключа
      DataTable tablePGC = DoSQLExecuteDataTable(@"SELECT conname, conrelid::regclass, conkey FROM pg_catalog.pg_constraint WHERE contype='f'", "pg_constraint", null);
      // Собираем руками
      DataTable tableForeignKeys = new DataTable();
      tableForeignKeys.Columns.Add("CONSTRAINT_NAME", typeof(string));
      tableForeignKeys.Columns.Add("TABLE_NAME", typeof(string));
      tableForeignKeys.Columns.Add("COLUMN_NAME", typeof(string));
      foreach (DataRow rowPGC in tablePGC.Rows)
      {
        Array aColIdxs = rowPGC[2] as Array;
        if (aColIdxs == null)
        {
          errors.AddWarning("В системной таблице \"pg_constraint\" задано ограничение внешнего ключа с неправильным полем \"conkey\"");
          continue;
        }
        if (aColIdxs.Length != 1)
          continue; // явно не наше ограничение

        string constraintName = (string)(rowPGC[0]);
        string tableName = (string)(rowPGC[1]);
        if (tableName[0] == '\"') // убираем кавычки
          tableName = tableName.Substring(1, tableName.Length - 2);

        object oColIdx = aColIdxs.GetValue(0);
        int colIdx = DataTools.GetInt(oColIdx);
        int p = dvColumns.Find(new object[2] { tableName, colIdx });
        if (p < 0)
        {
          errors.AddWarning("В системной таблице \"pg_constraint\" задано ограничение внешнего ключа для таблицы \"" + tableName + "\" и столюца с индексом " + colIdx.ToString() +
            ". Для этих таблицы+поле не найдено записи в таблице столбцов. Существующее ограничение не рассматривается, но возможна ошибка повторного добавления ограничения");
          continue;
        }
        string colName = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");

        tableForeignKeys.Rows.Add(constraintName, tableName, colName);
      }


      DataView dvForeignKeys = tableForeignKeys.DefaultView;
      dvForeignKeys.Sort = "TABLE_NAME,COLUMN_NAME";

      #endregion

      splash.PhaseText = "Проверка внешних ключей";
      splash.PercentMax = DB.Struct.Tables.Count;
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        if (UpdateForeignKeys(table, dvForeignKeys, splash, errors))
          modified = true;
        splash.IncPercent();
      }

      return modified;
    }

    private bool UpdateForeignKeys(DBxTableStruct table, DataView dvForeignKeys, ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;
      foreach (DBxColumnStruct column in table.Columns)
      {
        if (String.IsNullOrEmpty(column.MasterTableName))
          continue;
        if (column.RefType == DBxRefType.Emulation)
          continue;

        if (dvForeignKeys.Find(new object[] { table.TableName, column.ColumnName }) >= 0)
        {
          continue;
        }

        // Создаем внешний ключ
        splash.PhaseText = "Создание внешнего ключа для таблицы \"" + table.TableName + "\", столбца \"" + column.ColumnName + "\"";

        Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE ");
        Buffer.FormatTableName(table.TableName);
        Buffer.SB.Append(" ADD FOREIGN KEY (");
        Buffer.FormatColumnName(column.ColumnName);
        Buffer.SB.Append(") REFERENCES ");
        Buffer.FormatTableName(column.MasterTableName);
        Buffer.SB.Append(" ");
        Buffer.FormatRefColumnDeleteAction(column.RefType);

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Создан внешний ключ в таблице \"" + table.TableName + "\" для столбца \"" + column.ColumnName + "\"");
        modified = true;
      }

      return modified; // 28.12.2020
    }

    #endregion

    #region Индексы

    private bool UpdateDBStructIndices(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Индексы удаляем и создаем отдельно, не пытаясь объединить это с созданием таблицы

      bool modified = false;

      splash.PhaseText = "Проверка индексов";

      #region Загружаем существующие индексы и столбцы

      // Индексы тоже лучше создавать не в цикле таблиц, а отдельно, чтобы загрузить системные таблицы один раз
      // 12.05.2017
      // Использовать NpgsqlConnection.GetSchema() для индексов сложно.
      // - GetSchema("Indexes") возвращает иногда(?) строки с повторами. Это можно было бы обойти, но:
      // - GetSchema("IndexColumns") не возвращает порядок столбца в индексе и таблица не отсортирована
      // Порядок столбцов в индексе важен.
      // Если в БД есть индекс с теми же полями, что и требуется, но порядок полей отличается,
      // старый индекс надо удалить, и создать новый
      // Кроме того, могут быть всякие вычисляемые индексы
      // Поэтому, делаем все руками, кроме списка столбцов

      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION"; // нумерация с 1

      // Используем хитрый запрос.
      // См. исходные тексты Npgsql (модуль NpgsqlSchema.cs, метод GetIndexColumns()) с изменениями
      // Получаем столбцы
      // 0 "tablename": - имя таблицы
      // 1 "indexname" - имя индекса
      // 2 "indexcolumns" - массив номеров столбцов (нумерация с 1), 0- вычисляемый индекс
      DataTable tableIdx = DoSQLExecuteDataTable(
@"SELECT 
    t.relname AS tablename, 
    c.relname AS indexname, 
    i.indkey  AS indexcolumns
  FROM 
         pg_catalog.pg_class     t
    JOIN pg_catalog.pg_index     i ON t.oid = i.indrelid 
    JOIN pg_catalog.pg_class     c ON i.indexrelid = c.oid 
    JOIN pg_catalog.pg_namespace n ON c.relnamespace = n.oid 
  WHERE 
    (c.relkind='i') AND 
    (NOT i.indisprimary) AND 
    (n.nspname='public')"

, "pg_index", null);

      // в оригинале последний фильтр был "n.nspname not in ('pg_catalog', 'pg_toast')"
      // добавлен фильтр, чтобы убрать первичный ключ

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        #region Словарь для поиска нужных индексов

        // Создаем словарь "СписокСтолбцовВВерхнемРегистре - НомерИндекса"
        Dictionary<string, int> dict = new Dictionary<string, int>(table.Indexes.Count);
        for (int i = 0; i < table.Indexes.Count; i++)
          dict.Add(table.Indexes[i].Columns.AsString.ToUpperInvariant(), i);

        #endregion

        #region Просмотр существующих индексов и удаление ненужных

        // Для каждого описания индекса устанавливается флаг, если такой индекс есть
        bool[] indexFlags = new bool[table.Indexes.Count];

        // Существующие индексы для текущей таблицы
        tableIdx.DefaultView.RowFilter = new ValueFilter("tablename", table.TableName).ToString();

        //object[] a = TableIdx.Rows[1].ItemArray;

        foreach (DataRowView drvIdx in tableIdx.DefaultView)
        {
          string indexName = DataTools.GetString(drvIdx.Row, "indexname");

          // Первичные ключи не проверяем, т.к. их отфильтровали в запросе
          //if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          //  continue;

          #region Столбцы существующего индекса

          // Поле "indexname" имеет противный тип int2vector
          // Х.З., как с ним правильно работать. Net Framework делает из него строку вида"1 3 5"

          string sColIdxs = drvIdx.Row[2].ToString();
          string[] aColIdxs = sColIdxs.Split(' ');

          string[] colNames = new string[aColIdxs.Length];
          for (int j = 0; j < aColIdxs.Length; j++)
          {
            int colIdx = int.Parse(aColIdxs[j]);
            if (colIdx == 0)
            {
              // Индекс с выражением не обрабатываем
              colNames = null;
              break;
            }
            else
            {
              int p = dvColumns.Find(new object[] { table.TableName, colIdx });
              if (p < 0)
              {
                errors.AddError("Для индекса \"" + indexName + "\" задан столбец с номером " + colIdx.ToString() + ", которого нет в таблице \"" + table.TableName + "\"");
                colNames = null;
                break;
              }
              colNames[j] = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");
            }
          }
          if (colNames == null)
            continue;

          string thisColumns = String.Join(",", colNames).ToUpperInvariant();

          #endregion

          int indexIdx;
          if (dict.TryGetValue(thisColumns, out indexIdx))
            indexFlags[indexIdx] = true; // нашли индекс
          else if (options.DropUnusedIndices)
          {
            splash.PhaseText = "Удаление индекса " + indexName;
            DropIndex(/*Table.TableName, */indexName);
            modified = true;
            errors.AddInfo("Удален индекс \"" + indexName + "\" в таблице \"" + table.TableName + "\", т.к. он не соответствует объявленному в структуре данных");
          }
        } // цикл по существующим индексам

        #endregion

        #region Создание новых индексов

        for (int i = 0; i < table.Indexes.Count; i++)
        {
          if (!indexFlags[i])
          {
            CreateIndex(table.TableName, table.Indexes[i].Columns);
            modified = true;
            errors.AddInfo("Добавлен индекс для таблицы \"" + table.TableName + "\", по полям " + table.Indexes[i].Columns.AsString);
          }
        }

        #endregion

        splash.IncPercent();
      }

      return modified;
    }


    private void DropIndex(/*string tableName, */string indexName)
    {
      Buffer.Clear();
      Buffer.SB.Append("DROP INDEX \"");
      Buffer.SB.Append(indexName);
      Buffer.SB.Append("\"");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Добавление одного индекса
    /// </summary>
    private void CreateIndex(string tableName, DBxColumns columns)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE INDEX ");
      Buffer.SB.Append(" ON \"");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("\" (");
      for (int i = 0; i < columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.SB.Append("\"");
        Buffer.SB.Append(columns[i]);
        Buffer.SB.Append("\"");
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

    private bool DeleteAllIndices(string tableName, ISplash splash, ErrorMessageList errors)
    {
      // Перебираем все существующие индексы
      // Один индекс может занимать несколько строк
      // Создаем список индексов для удаления
      List<string> indexNames = null;

      DataTable tableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, tableName });
      foreach (DataRow indexRow in tableIndexes.Rows)
      {
        string indexName = DataTools.GetString(indexRow, "INDEX_NAME");
        if (indexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          continue;

        // Добавляем индекс в список на удаление
        if (indexNames == null)
          indexNames = new List<string>();
        if (!indexNames.Contains(indexName))
          indexNames.Add(indexName);
      }

      if (indexNames == null)
        return false;

      splash.PhaseText = "Удаление индексов таблицы \"" + tableName + "\"";
      for (int i = 0; i < indexNames.Count; i++)
      {
        Buffer.Clear();
        Buffer.SB.Append("DROP INDEX \"");
        Buffer.SB.Append(indexNames[i]);
        Buffer.SB.Append("\"");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Удален индекс \"" + indexNames[i] + "\" в таблице \"" + tableName + "\"");
      }
      return true;
    }

    #endregion

    #endregion
  }

  internal class NpgsqlDBxManager : DBxManager
  {
    #region Конструктор

    private NpgsqlDBxManager()
      : base(DBxProviderNames.Npgsql)
    {
    }

    public static readonly NpgsqlDBxManager TheManager = new NpgsqlDBxManager();

    #endregion

    #region Переопределенные методы

    public override DBx CreateDBObject(string connectionString)
    {
      return new NpgsqlDBx(connectionString);
    }

    public override string ReplaceDBName(string connectionString, string oldDBName, string newDBName)
    {
      NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder(connectionString);
      csb.Database = ReplaceDBItem(csb.Database, oldDBName, newDBName);
      return csb.ConnectionString;
    }

    public override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
      return new NpgsqlConnectionStringBuilder(connectionString);
    }

    /// <summary>
    /// Возвращает ссылку на NpgsqlFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return NpgsqlFactory.Instance; }
    }

    #endregion
  }
}
