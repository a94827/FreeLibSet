using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Data;
using AgeyevAV.IO;
using System.Data.Common;

/*
 * The BSD License
 * 
 * Copyright (c) 2016, Ageyev A.V.
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

namespace AgeyevAV.ExtDB.Npgsql
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
      SyncRoot = new object();

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


    private readonly object SyncRoot;

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
            using (NpgsqlDBxCon Con = MainEntry.CreateCon() as NpgsqlDBxCon)
            {
              _ServerVersionText = "PostGreSQL " + Con.Connection.ServerVersion;
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
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, false))
      {
        return DataTools.GetInt64(Con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
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
        using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, true))
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
    /// <summary>
    /// Создает базу данных, если ее не существует
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, true))
      {
        Con.CommandTimeout = 0; // Бесконечное время выполнения
        Con.CreateDatabase();
      }
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
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, false))
      {
        Con.CommandTimeout = 0; // Бесконечное время выполнения
        return Con.UpdateDBStruct(splash, errors, options);
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

      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, true))
      {
        Con.DropDatabase();
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
      using (DBxConBase Con = MainEntry.CreateCon())
      {
        Con.NameCheckingEnabled = false;
        DBxSqlBuffer Buffer = new DBxSqlBuffer(this.Formatter);
        Con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        Buffer.SB.Append("DROP TABLE IF EXISTS ");
        Buffer.FormatTableName(tableName);
        Con.SQLExecuteNonQuery(Buffer.SB.ToString());
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
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(NpgsqlCommand cmd, object[] ParamValues)
    {
      cmd.Parameters.Clear();
      if (ParamValues != null)
      {
        for (int i = 0; i < ParamValues.Length; i++)
        {
          NpgsqlParameter Param = new NpgsqlParameter();
          Param.ParameterName = "@P" + (i + 1).ToString();
          Param.Value = ParamValues[i];

          if (ParamValues[i] != null)
          {
            if (ParamValues[i] is Array)
            {
              //Param.SqlDbType = SqlDbType.VarBinary;
              Param.DbType = DbType.Binary;
            }
          }
          cmd.Parameters.Add(Param);
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

      Int32 Id;

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

      Id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (Id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + tableName + "\" Id=" + Id.ToString());

      return Id;
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
      int SeqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, columnNames);
      if (SeqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.GetInt(values[SeqColumnIndex]));
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

      DBxColumns ColumnNames = DBxColumns.FromColumns(table.Columns);
      Validator.CheckTableColumnNames(tableName, ColumnNames, false, DBxAccessMode.Full);
      DBxTableStruct ts = DB.Struct.Tables[tableName];

      #region Определяем режимы копирования

      CopyFormattingMode[] Modes = new CopyFormattingMode[ColumnNames.Count];

      for (int i = 0; i < ColumnNames.Count; i++)
      {
        DBxColumnStruct ColDef = ts.Columns[ColumnNames[i]];

        switch (ColDef.ColumnType)
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
      Buffer.FormatCSColumnNames(ColumnNames);
      Buffer.SB.Append(") FROM STDIN");

      NpgsqlCommand cmd = new NpgsqlCommand(Buffer.SB.ToString(), Connection);
      NpgsqlCopySerializer Serializer = new NpgsqlCopySerializer(Connection);
      NpgsqlCopyIn CopyIn = new NpgsqlCopyIn(cmd, Connection, Serializer.ToStream);

      CopyIn.Start();

      string s;
      foreach (DataRow Row in table.Rows)
      {
        for (int i = 0; i < ColumnNames.Count; i++)
        {
          if (Row.IsNull(i))
            Serializer.AddNull();
          else
          {
            DBxColumnStruct ColDef = ts.Columns[ColumnNames[i]];

            switch (Modes[i])
            {
              case CopyFormattingMode.FormatValue:
                Buffer.Clear();
                Buffer.FormatValue(Row[i], ColDef.ColumnType); // сами форматируем
                Serializer.AddString(Buffer.SB.ToString());
                break;
              case CopyFormattingMode.String:
                s = DataTools.GetString(Row[i]);
                Serializer.AddString(s); // пусть он форматирует
                break;
              case CopyFormattingMode.Guid: // 08.10.2019
                s = DataTools.GetString(Row[i]);
                Guid g = new Guid(s);
                s = g.ToString("D");
                Serializer.AddString(s);
                break;
              default:
                throw new BugException();
            }
          }
        }
        Serializer.EndRow();
      }
      Serializer.Flush();

      CopyIn.End();

      int SeqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, ColumnNames);
      if (SeqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.MaxInt(table, ColumnNames[SeqColumnIndex], true) ?? 0);
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
    private Dictionary<string, string> TablePKSequenceNames;


    /// <summary>
    /// "Подтягивание" идентификатора для последовательности
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="usedId"></param>
    private void CorrectPrimaryKeySequence(string tableName, Int32 usedId)
    {
      if (TablePKSequenceNames == null)
        TablePKSequenceNames = new Dictionary<string, string>();
      string SeqName;
      if (!TablePKSequenceNames.TryGetValue(tableName, out SeqName))
      {
        SeqName = GetTablePKSequenceName(tableName);
        TablePKSequenceNames.Add(tableName, SeqName);
      }

      string PrimaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);
      Int32 LastId = DataTools.GetInt(GetMaxValue(tableName, PrimaryKeyColumnName, null));
      usedId = Math.Max(LastId, usedId);

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
      string PrimaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);
      StringBuilder sb = new StringBuilder();

      // Имя таблицы идет и в апострофах и в кавычках
      // А имя столбца - только в апострофах
      // Х.З. почему
      sb.Append("SELECT pg_get_serial_sequence(\'\"");
      sb.Append(tableName);
      sb.Append("\"\', \'");
      sb.Append(PrimaryKeyColumnName);
      sb.Append("\')");

      string SeqName = (string)(SQLExecuteScalar(sb.ToString()));

      return SeqName;
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
      DataTable Table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName, "public" });
      string[] a = new string[Table.Rows.Count];
      for (int i = 0; i < Table.Rows.Count; i++)
        a[i] = DataTools.GetString(Table.Rows[i], "table_name");

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

      DataTable Table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "public", tableName });
      Table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in Table.DefaultView)
      {
        string ColumnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct ColStr = new DBxColumnStruct(ColumnName);

        string ColTypeString = DataTools.GetString(drv.Row, "data_type");
        switch (ColTypeString)
        {
          case "char":
          case "nchar":
          case "bpchar":
            ColStr.ColumnType = DBxColumnType.String;
            break;

          case "bigint":
          case "int8":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int64.MinValue;
            ColStr.MaxValue = Int64.MaxValue;
            break;
          case "int":
          case "int4":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int32.MinValue;
            ColStr.MaxValue = Int32.MaxValue;
            break;
          case "smallint":
          case "int2":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int16.MinValue;
            ColStr.MaxValue = Int16.MaxValue;
            break;
          case "tinyint":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = 0;
            ColStr.MaxValue = 255;
            break;

          case "float":
          case "float4":
            ColStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;
          case "real":
          case "float8":
            ColStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            ColStr.MinValue = Single.MinValue;
            ColStr.MaxValue = Single.MaxValue;
            break;

          case "money":
            ColStr.ColumnType = DBxColumnType.Money;
            ColStr.MinValue = -922337203685477.5808;
            ColStr.MaxValue = 922337203685477.5807;
            break;
          case "smallmoney":
            ColStr.ColumnType = DBxColumnType.Money;
            ColStr.MinValue = -214748.3648;
            ColStr.MaxValue = 214748.3647;
            break;

          case "bit":
          case "bool":
            ColStr.ColumnType = DBxColumnType.Boolean;
            break;

          case "date":
            ColStr.ColumnType = DBxColumnType.Date;
            break;

          case "datetime":
          case "smalldatetime":
          case "datetimeoffset": // ???
          case "timestamp":
          case "datetime2": ColStr.ColumnType = DBxColumnType.DateTime; break;

          case "time": ColStr.ColumnType = DBxColumnType.Time; break;

          case "varchar":
          case "nvarchar":
          case "text":
          case "ntext":
            ColStr.ColumnType = DBxColumnType.Memo;
            break;

          case "image":
          case "varbinary":
            ColStr.ColumnType = DBxColumnType.Binary;
            break;

          case "binary":
            ColStr.ColumnType = DBxColumnType.Binary;
            //ColStr.MaxLength=
            break;

          case "xml":
            ColStr.ColumnType = DBxColumnType.Xml;
            break;

          default:
            break;
        }

        ColStr.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string NullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (NullableStr) // 01.10.2019
        {
          case "YES": ColStr.Nullable = true; break;
          case "NO": ColStr.Nullable = false; break;
        }


        TableStr.Columns.Add(ColStr);
      }

      #endregion

      #region Определение ссылочных полей (ограничения FOREIGN KEY)

      //DataTable Table2 = Connection.GetSchema();
      //string[]aaa=DataTools.GetStringsFromField(Table2, "CollectionName");

      //Table = Connection.GetSchema("Restrictions", new string[] { DB.DatabaseName, "public", TableName });
      //foreach (DataRow r in Table.Rows)
      //{ 
      //}


      int TableOID = GetTableOID(tableName);
      if (TableOID == 0)
        throw new BugException("Не удалось получить идентификатор object_id таблицы \"" + tableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append(@"SELECT confrelid,confdeltype,conkey FROM pg_catalog.pg_constraint WHERE contype='f' AND conrelid=");
      Buffer.FormatValue(TableOID, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "pg_constraint");
      foreach (DataRow Row in tbl.Rows)
      {
        Int32 RefTableOID = DataTools.GetInt(Row, "confrelid");
        string RefTableName = GetTableNameFromOID(RefTableOID);
        if (String.IsNullOrEmpty(RefTableName))
          throw new BugException("Не найдено имя для мастер-таблицы с OID=" + RefTableOID);

        Int16[] DetColPoss = (Int16[])(Row["conkey"]);
        // не нужен Int16[] RefColPoss = (Int16[])(Row["confkey"]);
        if (DetColPoss.Length != 1 /*|| RefColPoss.Length != 1*/)
          continue; // FOREIGN KEY по нескольким полям не поддерживаются

        DBxColumnStruct ColStr = TableStr.Columns[DetColPoss[0] - 1];
        ColStr.MasterTableName = RefTableName; // 01.10.2019

        string RefTypeCode = DataTools.GetString(Row, "confdeltype");
        switch (RefTypeCode)
        {
          case "c": ColStr.RefType = DBxRefType.Delete; break;
          case "n": ColStr.RefType = DBxRefType.Clear; break;
          default: ColStr.RefType = DBxRefType.Disallow; break;
        }

      }

      #endregion

      TableStr.SetReadOnly();
      return TableStr;
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
      bool Modified = false;

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
        Modified = true;

      if (options.ForeignKeys)
      {
        if (UpdateDBStructForeignKeys(splash, errors))
          Modified = true;
      }

      if (UpdateDBStructIndices(splash, errors, options))
        Modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return Modified;
    }

    #endregion

    #region Добавление таблиц, столбцов и первичных ключей

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors)
    {
      bool Modified = false;

      #region Извлечение информации из существующей схемы данных

      DataTable TableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(TableTables);
      dvTables.Sort = "TABLE_NAME";

      // Столбцы
      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // нужен такой порядок

      // Данные по индексам 
      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";
      //DebugTools.DebugDataView(dvIndexColumns, "Столбцы индексов базы данных "+DB.DisplayName);

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        if (Table.Columns.Count == 0)
          throw new DBxStructException(Table, "Не задано ни одного столбца");
        //CheckPrimaryKeyColumn(Table, Table.PrimaryKeyColumns[0]);

        if (dvTables.Find(Table.TableName) < 0)
        {
          #region Требуется полное создание таблицы

          splash.PhaseText = "Создается таблица \"" + Table.TableName + "\"";
          CreateTable(Table);
          errors.AddInfo("Создана таблица \"" + Table.TableName + "\"");
          Modified = true;

          #endregion
        }
        else
        {
          #region Проверяем правильность первичного ключа

          if (CorrectPrimaryKey(Table, dvIndexColumns, errors))
            Modified = true;

          #endregion

          #region Проверяем наличие недостающих полей

          DBxSqlBuffer Buffer2 = new DBxSqlBuffer(Buffer.Formatter);

          foreach (DBxColumnStruct Column in Table.Columns)
          {
            int ColumnRowIndex = dvColumns.Find(new object[] { Table.TableName, Column.ColumnName });
            if (ColumnRowIndex < 0)
            {
              // Поля не существует
              splash.PhaseText = "Добавление поля \"" + Column.ColumnName + "\"в таблицу \"" + Table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE ");
              Buffer.FormatTableName(Table.TableName);
              Buffer.SB.Append(" ADD "); // а не ADD COLUMN
              AppendColumnDef(/*Table, */Column, false, true);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("Создано поле \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName + "\"");
              Modified = true;
            }
            else
            {
              // Проверяем соответствие поля
              DataRow ColumnRow = dvColumns[ColumnRowIndex].Row;
              // Проверяем соответствие типа столбца объявлению
              string RealType = DataTools.GetString(ColumnRow, "DATA_TYPE").ToUpperInvariant();
              RealType = ReplaceSynonymousPostGreSqlServerType(RealType);
              Buffer2.Clear();
              FormatValueType(Buffer2, Column, false);
              string WantedType = Buffer2.SB.ToString();
              int p = WantedType.IndexOf('(');
              if (p >= 0)
                WantedType = WantedType.Substring(0, p);

              if (RealType != WantedType)
              {
                errors.AddError("Несоответствие типа поля \"" + Column.ColumnName + "\" таблицы \"" +
                    Table.TableName + "\". Объявление поля типа " + Column.ColumnType.ToString() +
                    " предполагает тип " + WantedType +
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
                      errors.AddWarning("Поле \"" + Column.ColumnName + "\" таблицы \"" +
                          Table.TableName + "\" должно иметь длину " + Column.MaxLength.ToString() +
                          " символов, в то время, как реальное поле длиннее:  " + RealLen.ToString() + " символов");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // Лучше пересоздать все индексы
                      errors.AddInfo("Все существующие индексы таблицы \"" + Table.TableName + "\" будут удалены из-за изменения размера поля \"" + Column.ColumnName + "\"");
                      if (DeleteAllIndices(Table.TableName, splash, errors))
                        Modified = true;

                      // Увеличиваем длину поля
                      splash.PhaseText = "Изменение длины поля \"" + Column.ColumnName + "\" в таблице \"" + Table.TableName + "\"";
                      AlterColumn(Table, Column, true, false);
                      errors.AddInfo("Длина поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                        "\" увеличена с " + RealLen.ToString() + " до " + Column.MaxLength.ToString() + " символов");
                      Modified = true;
                    }
                  }
                } // Строковое поле

                #endregion

                #region Проверка Default

                // Установка свойства DEFAULT должна выполняться до установки NOT NULL, иначе возникнет ошибка

                string WantedDefExpr = String.Empty;
                if (Column.DefaultExpression != null)
                {
                  Buffer2.Clear();
                  Buffer2.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
                  WantedDefExpr = Buffer2.SB.ToString();
                }
                string RealDefExpr = DataTools.GetString(ColumnRow, "COLUMN_DEFAULT");
                if (RealDefExpr.StartsWith("nextval(", StringComparison.OrdinalIgnoreCase))
                  RealDefExpr = String.Empty; // иначе изгадим первичный ключ
                if (RealDefExpr != WantedDefExpr)
                {
                  // Вседа сначала убираем старое правило, потом добавляем новое
                  if (RealDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    Buffer.SB.Append(" DROP DEFAULT");

                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                        "\" очищен признак DEFAULT");
                  }
                  if (WantedDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    Buffer.SB.Append("SET DEFAULT ");
                    Buffer.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
                    SQLExecuteNonQuery(Buffer.SB.ToString());

                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                      "\" установлен признак DEFAULT " + WantedDefExpr);
                  }
                  Modified = true;
                }

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
                  if (DeleteAllIndices(Table.TableName, splash, errors))
                    Modified = true;

                  #region Замена NULL'ов на DEFAULT

                  if ((!Column.Nullable) && Column.DefaultExpression != null && (!IsTableEmpty(Table.TableName)))
                  {
                    //SetValue(Table.TableName, new ValueFilter(Column.ColumnName, null, CompareKind.Equal, Column.ColumnType),Column.ColumnName, Column.Default)
                    // Заменяем значения NULL на значение по умолчанию
                    Buffer.Clear();
                    Buffer.SB.Append("UPDATE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(" SET ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    Buffer.SB.Append("=");
                    Buffer.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
                    Buffer.SB.Append(" WHERE ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    Buffer.SB.Append(" IS NULL");
                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                      "\" значения NULL заменены на значение по умолчанию");
                  }

                  #endregion

                  // Делаем поле NULLABLE
                  AlterColumn(Table, Column, false, true);
                  errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
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

        #region Комментарии

        SetTableComment(Table.TableName, Table.Comment);
        for (int i = 0; i < Table.Columns.Count; i++)
          SetColumnComment(Table.TableName, Table.Columns[i].ColumnName, Table.Columns[i].Comment);


        #endregion

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      return Modified;
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
    private static readonly Dictionary<string, string> SynonymousPostGreSqlServerTypeReplaces =
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
      string MainType;
      if (SynonymousPostGreSqlServerTypeReplaces.TryGetValue(realType, out MainType))
        return MainType;
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

      bool Modified = false;
      bool Found = false;
      string PKName = table.TableName + "_PKEY";

      dvIndexColumns.RowFilter = "TABLE_NAME='" + table.TableName + "' AND INDEX_NAME='" + PKName + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string ColumnName = DataTools.GetString(drvCol.Row, "COLUMN_NAME");
        if (String.Compare(ColumnName, table.PrimaryKey[0], StringComparison.OrdinalIgnoreCase) == 0)
          Found = true;
        else
        {
          //if (IndexName.StartsWith("Index"))
          //  continue; // составной пользовательский индекс, в который входит поле "Id"
          SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" DROP CONSTRAINT \"" + PKName + "\"");
          errors.AddInfo("Удалено неправильное ограничение первичного ключа \"" + PKName + "\" в таблице \"" + table.TableName + "\"");
          Modified = true;
          break;
        }
      }

      if (!Found)
      {
        SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" ADD CONSTRAINT \"" + PKName + "\" PRIMARY KEY (\"" + table.PrimaryKey[0] + "\")");
        errors.AddInfo("Добавлено ограничение первичного ключа \"" + PKName + "\" в таблице \"" + table.TableName + "\"");
        Modified = true;
      }

      return Modified;
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
      bool Modified = false;

      #region Создаем TableForeignKeys

      // Внешние ключи можно создавать только после создания всех таблиц
      // Так нельзя получить столбцы для ограничения
      //// Данные по внешним ключам
      //DataTable TableForeignKeys = Connection.GetSchema("ForeignKeys");
      //DataView dvForeignKeys = new DataView(TableForeignKeys);
      //dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // Столбцы
      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION";

      // Используем системную таблицу pg_constraint
      // 0: conname - имя ограчения (для удаления, если надо будет)
      // 1: conrelid::regclass - имя таблицы в кавычках
      // 2: conkey - массив индексов столбцов ссылочных полей (нас интересуют только ограничения из одного элемента)
      // contype='f' - фильтр по ограничениям внешнего ключа
      DataTable TablePGC = DoSQLExecuteDataTable(@"SELECT conname, conrelid::regclass, conkey FROM pg_catalog.pg_constraint WHERE contype='f'", "pg_constraint", null);
      // Собираем руками
      DataTable TableForeignKeys = new DataTable();
      TableForeignKeys.Columns.Add("CONSTRAINT_NAME", typeof(string));
      TableForeignKeys.Columns.Add("TABLE_NAME", typeof(string));
      TableForeignKeys.Columns.Add("COLUMN_NAME", typeof(string));
      foreach (DataRow RowPGC in TablePGC.Rows)
      {
        Array aColIdxs = RowPGC[2] as Array;
        if (aColIdxs == null)
        {
          errors.AddWarning("В системной таблице \"pg_constraint\" задано ограничение внешнего ключа с неправильным полем \"conkey\"");
          continue;
        }
        if (aColIdxs.Length != 1)
          continue; // явно не наше ограничение

        string ConstraintName = (string)(RowPGC[0]);
        string TableName = (string)(RowPGC[1]);
        if (TableName[0] == '\"') // убираем кавычки
          TableName = TableName.Substring(1, TableName.Length - 2);

        object oColIdx = aColIdxs.GetValue(0);
        int ColIdx = DataTools.GetInt(oColIdx);
        int p = dvColumns.Find(new object[2] { TableName, ColIdx });
        if (p < 0)
        {
          errors.AddWarning("В системной таблице \"pg_constraint\" задано ограничение внешнего ключа для таблицы \"" + TableName + "\" и столюца с индексом " + ColIdx.ToString() +
            ". Для этих таблицы+поле не найдено записи в таблице столбцов. Существующее ограничение не рассматривается, но возможна ошибка повторного добавления ограничения");
          continue;
        }
        string ColName = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");

        TableForeignKeys.Rows.Add(ConstraintName, TableName, ColName);
      }


      DataView dvForeignKeys = TableForeignKeys.DefaultView;
      dvForeignKeys.Sort = "TABLE_NAME,COLUMN_NAME";

      #endregion

      splash.PhaseText = "Проверка внешних ключей";
      splash.PercentMax = DB.Struct.Tables.Count;
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        if (UpdateForeignKeys(Table, dvForeignKeys, splash, errors))
          Modified = true;
        splash.IncPercent();
      }

      return Modified;
    }

    private bool UpdateForeignKeys(DBxTableStruct table, DataView dvForeignKeys, ISplash splash, ErrorMessageList errors)
    {
      bool Modified = false;
      foreach (DBxColumnStruct Column in table.Columns)
      {
        if (String.IsNullOrEmpty(Column.MasterTableName))
          continue;
        if (Column.RefType == DBxRefType.Emulation)
          continue;

        if (dvForeignKeys.Find(new object[] { table.TableName, Column.ColumnName }) >= 0)
        {
          continue;
        }

        // Создаем внешний ключ
        splash.PhaseText = "Создание внешнего ключа для таблицы \"" + table.TableName + "\", столбца \"" + Column.ColumnName + "\"";

        Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE ");
        Buffer.FormatTableName(table.TableName);
        Buffer.SB.Append(" ADD FOREIGN KEY (");
        Buffer.FormatColumnName(Column.ColumnName);
        Buffer.SB.Append(") REFERENCES ");
        Buffer.FormatTableName(Column.MasterTableName);
        Buffer.SB.Append(" ");
        Buffer.FormatRefColumnDeleteAction(Column.RefType);

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Создан внешний ключ в таблице \"" + table.TableName + "\" для столбца \"" + Column.ColumnName + "\"");
        Modified = true;
      }

      return Modified; // 28.12.2020
    }

    #endregion

    #region Индексы

    private bool UpdateDBStructIndices(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Индексы удаляем и создаем отдельно, не пытаясь объединить это с созданием таблицы

      bool Modified = false;

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

      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION"; // нумерация с 1

      // Используем хитрый запрос.
      // См. исходные тексты Npgsql (модуль NpgsqlSchema.cs, метод GetIndexColumns()) с изменениями
      // Получаем столбцы
      // 0 "tablename": - имя таблицы
      // 1 "indexname" - имя индекса
      // 2 "indexcolumns" - массив номеров столбцов (нумерация с 1), 0- вычисляемый индекс
      DataTable TableIdx = DoSQLExecuteDataTable(
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
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        #region Словарь для поиска нужных индексов

        // Создаем словарь "СписокСтолбцовВВерхнемРегистре - НомерИндекса"
        Dictionary<string, int> Dict = new Dictionary<string, int>(Table.Indices.Count);
        for (int i = 0; i < Table.Indices.Count; i++)
          Dict.Add(Table.Indices[i].Columns.AsString.ToUpperInvariant(), i);


        #endregion

        #region Просмотр существующих индексов и удаление ненужных

        // Для каждого описания индекса устанавливается флаг, если такой индекс есть
        bool[] IndexFlags = new bool[Table.Indices.Count];

        // Существующие индексы для текущей таблицы
        TableIdx.DefaultView.RowFilter = new ValueFilter("tablename", Table.TableName).ToString();

        //object[] a = TableIdx.Rows[1].ItemArray;

        foreach (DataRowView drvIdx in TableIdx.DefaultView)
        {
          string IndexName = DataTools.GetString(drvIdx.Row, "indexname");

          // Первичные ключи не проверяем, т.к. их отфильтровали в запросе
          //if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          //  continue;

          #region Столбцы существующего индекса

          // Поле "indexname" имеет противный тип int2vector
          // Х.З., как с ним правильно работать. Net Framework делает из него строку вида"1 3 5"

          string sColIdxs = drvIdx.Row[2].ToString();
          string[] aColIdxs = sColIdxs.Split(' ');

          string[] ColNames = new string[aColIdxs.Length];
          for (int j = 0; j < aColIdxs.Length; j++)
          {
            int ColIdx = int.Parse(aColIdxs[j]);
            if (ColIdx == 0)
            {
              // Индекс с выражением не обрабатываем
              ColNames = null;
              break;
            }
            else
            {
              int p = dvColumns.Find(new object[] { Table.TableName, ColIdx });
              if (p < 0)
              {
                errors.AddError("Для индекса \"" + IndexName + "\" задан столбец с номером " + ColIdx.ToString() + ", которого нет в таблице \"" + Table.TableName + "\"");
                ColNames = null;
                break;
              }
              ColNames[j] = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");
            }
          }
          if (ColNames == null)
            continue;

          string ThisColumns = String.Join(",", ColNames).ToUpperInvariant();

          #endregion

          int IndexIdx;
          if (Dict.TryGetValue(ThisColumns, out IndexIdx))
            IndexFlags[IndexIdx] = true; // нашли индекс
          else if (options.DropUnusedIndices)
          {
            splash.PhaseText = "Удаление индекса " + IndexName;
            DropIndex(/*Table.TableName, */IndexName);
            Modified = true;
            errors.AddInfo("Удален индекс \"" + IndexName + "\" в таблице \"" + Table.TableName + "\", т.к. он не соответствует объявленному в структуре данных");
          }
        } // цикл по существующим индексам

        #endregion

        #region Создание новых индексов

        for (int i = 0; i < Table.Indices.Count; i++)
        {
          if (!IndexFlags[i])
          {
            CreateIndex(Table.TableName, Table.Indices[i].Columns);
            Modified = true;
            errors.AddInfo("Добавлен индекс для таблицы \"" + Table.TableName + "\", по полям " + Table.Indices[i].Columns.AsString);
          }
        }

        #endregion

        splash.IncPercent();
      }

      return Modified;
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
      List<string> IndexNames = null;

      DataTable TableIndices = Connection.GetSchema("Indexes", new string[] { null, null, tableName });
      foreach (DataRow IndexRow in TableIndices.Rows)
      {
        string IndexName = DataTools.GetString(IndexRow, "INDEX_NAME");
        if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          continue;

        // Добавляем индекс в список на удаление
        if (IndexNames == null)
          IndexNames = new List<string>();
        if (!IndexNames.Contains(IndexName))
          IndexNames.Add(IndexName);
      }

      if (IndexNames == null)
        return false;

      splash.PhaseText = "Удаление индексов таблицы \"" + tableName + "\"";
      for (int i = 0; i < IndexNames.Count; i++)
      {
        Buffer.Clear();
        Buffer.SB.Append("DROP INDEX \"");
        Buffer.SB.Append(IndexNames[i]);
        Buffer.SB.Append("\"");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Удален индекс \"" + IndexNames[i] + "\" в таблице \"" + tableName + "\"");
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
