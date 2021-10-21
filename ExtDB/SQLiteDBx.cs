using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data;
using FreeLibSet.IO;
using System.Data.Common;
using System.Diagnostics;
using FreeLibSet.Logging;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2017, Ageyev A.V.
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

// Провайдер для доступа к базе данных SQLite

namespace FreeLibSet.Data.SQLite
{
  /// <summary>
  /// База данных PostGreSQL через провайдер Npgsql.
  /// Для использования должны быть подключены сборки Npgsql.dll и Mono.Security из подкаталога FreeLibSet/Others
  /// </summary>
  public class SQLiteDBx : DBx
  {
    #region Конструкторы и Dispose

    internal const string MemoryFileName = ":memory:";

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public SQLiteDBx(SQLiteConnectionStringBuilder connectionStringBuilder)
    {
      LoadDLLs();

      if (_FirstFlag)
      {
        _FirstFlag = false;
        LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
      }

      SyncRoot = new object();


      _InMemory = connectionStringBuilder.DataSource.EndsWith(MemoryFileName);
      if (!InMemory)
      {
        _FileName = new AbsPath(connectionStringBuilder.DataSource);
        if (_FileName.IsEmpty)
          throw new ArgumentException("В строке подключения не задан параметр DataSource", "connectionStringBuilder");
      }

      _DateFormat = @"yyyy\-MM\-dd";
      _TimeFormat = @"HH\:mm\:ss";
      SetFormatter(new
        SQLiteDBxSqlFormatter(this));

      connectionStringBuilder.FailIfMissing = false; // всегда автоматически создаем базу данных

      if (InMemory)
      {
        MainConnection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
        MainConnection.Open();
      }

      new SQLiteDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public SQLiteDBx(string connectionString)
      : this(new SQLiteConnectionStringBuilder(connectionString))
    {
    }

    /// <summary>
    /// Открывает базу данных по указанному пути
    /// </summary>
    /// <param name="path">Путь к базе данных. Должен быть задан</param>
    /// <param name="readOnly">True - открыть базу только для просмотра, false - для записи</param>
    public SQLiteDBx(AbsPath path, bool readOnly)
      : this(GetConnectionStringBuilder(path, readOnly))
    {
    }

    private static SQLiteConnectionStringBuilder GetConnectionStringBuilder(AbsPath path, bool readOnly)
    {
      if (path.IsEmpty)
        throw new ArgumentNullException("path");
      SQLiteConnectionStringBuilder b = new SQLiteConnectionStringBuilder();
      b.DataSource = path.Path;
      b.ReadOnly = readOnly;
      return b;
    }

    /// <summary>
    /// Эта версия конструктора предназначена для создания базы данных в памяти
    /// </summary>
    public SQLiteDBx()
      : this("Data Source=" + MemoryFileName)
    {
    }

    /// <summary>
    /// Если InMemory=true, то закрывает соединение с базой данных, что приводит к ее удалению.
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (MainConnection != null)
        {
          MainConnection.Dispose();
          MainConnection = null;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает полный путь к файлу базы данных
    /// </summary>
    public override string DatabaseName { get { return FileName.Path; } }

    /// <summary>
    /// Имя файла
    /// </summary>
    public AbsPath FileName { get { return _FileName; } }
    private AbsPath _FileName;

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new SQLiteDBxEntry MainEntry { get { return (SQLiteDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// Создает SQLiteDBxEntry
    /// </summary>
    /// <param name="permissions">Разрешения на доступ к базе данных</param>
    /// <returns>Точка входа</returns>
    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new SQLiteDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }

    private object SyncRoot;

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
            using (SQLiteDBxCon Con = MainEntry.CreateCon() as SQLiteDBxCon)
            {
              _ServerVersionText = "SQLite " + Con.Connection.ServerVersion;
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
      throw new NotImplementedException();
      //using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry, false))
      //{
      //  return DataTools.GetInt64(Con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
      //}
    }

    /// <summary>
    /// Возвращает статический экзеипляр менеджера баз данных PostgreSQL
    /// </summary>
    public override DBxManager Manager { get { return SQLiteDBxManager.TheManager; } }

    /// <summary>
    /// Возвращает true, если база данных располагается в памяти
    /// </summary>
    public bool InMemory { get { return _InMemory; } }
    private bool _InMemory;

    /// <summary>
    /// Если база данных располагается в памяти, требуется постоянно открытое соединение для нее.
    /// Когда это соединение закрывается, база данных уничтожается.
    /// </summary>
    internal SQLiteConnection MainConnection;

    #endregion

    #region Форматирование значений

    /// <summary>
    /// Формат даты
    /// </summary>
    public string DateFormat
    {
      get { return _DateFormat; }
      set
      {
        CheckHasNotConnected();
        _DateFormat = value;
      }
    }
    private string _DateFormat;

    /// <summary>
    /// Формат времени
    /// </summary>
    public string TimeFormat
    {
      get { return _TimeFormat; }
      set
      {
        CheckHasNotConnected();
        _TimeFormat = value;
      }
    }
    private string _TimeFormat;

    #endregion

    #region Обновление структуры

    /// <summary>
    /// Возвращает признак существования файла FileName
    /// </summary>
    public override bool DatabaseExists
    {
      get
      {
        if (InMemory)
          return true;

        return System.IO.File.Exists(FileName.Path);
      }
    }

    /// <summary>
    /// Создает файл базы данных, если он не существует
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry))
      {
        // Ничего не делаем. Само создается
        // 21.07.2021. Неверно. Надо обязательно обратиться к свойству Connection

        Con.Connection.GetSchema();
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
      using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry))
      {
        Con.CommandTimeout = 0; // Бесконечное время выполнения
        return Con.UpdateDBStruct(splash, errors, options);
      }
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
        Buffer.SB.Append("DROP TABLE IF EXISTS ");
        Con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        Buffer.FormatTableName(tableName);
        Con.SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Удаление базы данных, если она существует
    /// </summary>
    /// <returns>True, если существующая база данных была удалена.
    /// False, если база данных не зарегистрирована</returns>
    public override bool DropDatabaseIfExists()
    {
      if (!DatabaseExists)
        return false;

      SQLiteConnection.ClearAllPools();

      System.IO.File.Delete(FileName.Path);

      return true;
    }

    /// <summary>
    /// Возвращает ссылку на SQLiteFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }

    #endregion

    #region Загрузка библиотек доступа к SQLite

    /// <summary>
    /// Загружает библиотеки, необходимы для работы с SQLite.
    /// Повторные вызовы игнорируются
    /// </summary>
    public static void LoadDLLs()
    {
      if (LoadDLLsFlag)
        return; // повторный вызов

      //if (IntPtr.Size == 8)
      //  System.Reflection.Assembly.Load("SQLite.Interop.x64");
      //else
      //  System.Reflection.Assembly.Load("SQLite.Interop.x86");

      System.Reflection.Assembly.Load("System.Data.SQLite"); // обязательно после SQLite.Interop.dll

      LoadDLLsFlag = true;
    }

    private static bool LoadDLLsFlag = false;

    #endregion

    #region Отладка использования памяти

    private static bool _FirstFlag = true;

    private static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
      try
      {
        args.WriteHeader("SQLite");
        args.WritePair("SQLiteVersion", SQLiteConnection.SQLiteVersion);
        args.WritePair("SQLiteSourceId", SQLiteConnection.SQLiteSourceId);
        args.WritePair("SQLiteCompileOptions", SQLiteConnection.SQLiteCompileOptions);
        args.WritePair("ProviderVersion", SQLiteConnection.ProviderVersion);
        args.WritePair("ProviderSourceId", SQLiteConnection.ProviderSourceId);
        args.WritePair("InteropVersion", SQLiteConnection.InteropVersion);
        args.WritePair("InteropSourceId", SQLiteConnection.InteropSourceId);
        args.WritePair("InteropCompileOptions", SQLiteConnection.InteropCompileOptions);
        args.WritePair("DefineConstants", SQLiteConnection.DefineConstants);
      }
      catch (Exception e) { args.WriteLine("*** Ошибка ***. " + e.Message); }

      args.WriteLine("SQLiteConnection.GetMemoryStatistics()");
      args.IndentLevel++;
      try
      {
        IDictionary<string, long> dict = null;
        SQLiteConnection.GetMemoryStatistics(ref dict);
        LogoutTools.LogoutObject(args, dict);
      }
      catch (Exception e) { args.WriteLine("*** Ошибка ***. " + e.Message); }
      args.IndentLevel--;
    }

    #endregion
  }

  /// <summary>
  /// Точка входа для базы данных SQLite
  /// </summary>
  public class SQLiteDBxEntry : DBxEntry
  {
    #region Конструкторы

    /// <summary>
    /// Защищенный конструктор для основной точки входа
    /// </summary>
    /// <param name="db"></param>
    /// <param name="connectionStringBuilder"></param>
    internal SQLiteDBxEntry(SQLiteDBx db, SQLiteConnectionStringBuilder connectionStringBuilder)
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
    public SQLiteDBxEntry(SQLiteDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new SQLiteConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionStringBuilder">Строка подключения</param>
    /// <param name="permissions">Разрешения</param>
    public SQLiteDBxEntry(SQLiteDBx db, SQLiteConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
      : base(db, permissions, false)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// База данных, к которой относится точка входа
    /// </summary>
    public new SQLiteDBx DB { get { return (SQLiteDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal SQLiteConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private SQLiteConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект SQLiteDBxCon
    /// </summary>
    /// <returns>Соединение</returns>
    public override DBxConBase CreateCon()
    {
      return new SQLiteDBxCon(this);
    }

    /// <summary>
    /// Создает копию точки входа с другим набором прав
    /// </summary>
    /// <param name="newPermissions">Требуемые разрешения на доступ к объектам базы данных</param>
    /// <returns>Новая точка входа</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new SQLiteDBxEntry(DB, ConnectionString, newPermissions);
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
    /// <param name="cs"></param>
    /// <returns></returns>
    internal static string GetUnpasswordedConnectionString(string cs)
    {
      try
      {
        SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(cs);
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
  /// Соедиенение с базой данных SQLite.
  /// Для создания объекта используйте SQLiteDBxEntry
  /// </summary>
  public class SQLiteDBxCon : DBxConBase
  {
    #region Конструктор и Dispose

    internal SQLiteDBxCon(SQLiteDBxEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Закрывает соедиенение ADO.NET, если оно было открыто, и возвращает его в пул.
    /// Удаляет соединение из точки входа.
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose().
    /// False, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Connection != null)
        {
          if (_Connection.State == ConnectionState.Open && (!Entry.DB.InMemory))
            _Connection.Close();
          _Connection = null;
        }

        // Это вообще завершает приложение сервера
        //if (ClearPoolCalled)
        //  SQLiteConnection.Shutdown(false, true);
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает точку доступа, к которой относится соединение
    /// </summary>
    public new SQLiteDBxEntry Entry { get { return (SQLiteDBxEntry)(base.Entry); } }

    /// <summary>
    /// Возвращает базу данных, к которой относится соединение
    /// </summary>
    public new SQLiteDBx DB { get { return (SQLiteDBx)(base.DB); } }

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    public SQLiteConnection Connection
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
            if (Entry.DB.MainConnection == null)
            {
              _Connection = new SQLiteConnection(Entry.ConnectionString);
              DoOpenConnection();
            }
            else
            {
              // Это эквивалентно вызову new SQLiteConnection(Entry.DB.MainConnection)
              // Создает соединение для новой временной базы данных
              // Х.з., как подключиться к существующей
              //_Connection = (SQLiteConnection)(Entry.DB.MainConnection.Clone());

              // TODO: Плохо. Если используется DBDataReader, то соединение будет занято
              _Connection = Entry.DB.MainConnection;
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

    private SQLiteConnection _Connection;

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
        e.Data["SQLiteDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["SQLiteDBxCon.ConnectionString"] = SQLiteDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
    }

    /// <summary>
    /// Вызывает SQLiteConnection.ClearPool()
    /// </summary>
    public override void ClearPool()
    {
      SQLiteConnection.ClearPool(Connection);
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
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
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
    [DebuggerStepThrough]
    protected override void DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
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
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
      da.FillError += new FillErrorEventHandler(HandleDataAdapterFillError); // 03.10.2018
      DataTable Table = new DataTable(tableName);
      da.Fill(Table);
      Table.AcceptChanges(); // 06.08.2018
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
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(SQLiteCommand cmd, object[] paramValues)
    {
      cmd.Parameters.Clear();
      if (paramValues != null)
      {
        for (int i = 0; i < paramValues.Length; i++)
        {
          SQLiteParameter Param = new SQLiteParameter();
          //Param.ParameterName = "@P" + (i + 1).ToString();
          Param.ParameterName = ":P" + (i + 1).ToString(); // исправлено 29.05.2020
          Param.Value = paramValues[i];

          if (paramValues[i] != null)
          {
            if (paramValues[i] is Array)
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

    /// <summary>
    /// Возвращает оптимизированную реализацию писателя
    /// </summary>
    /// <param name="writerInfo"></param>
    /// <returns></returns>
    protected override DBxDataWriter OnCreateWriter(DBxDataWriterInfo writerInfo)
    {
      return new SQLiteDBxDataWriter(this, writerInfo);
    }

    #region Подсчет строк

#if XXX
    // Не нравится предлжение "AS"

    public override int RecordCount(string TableName)
    {
      CheckTableName(TableName, DBxAccessMode.ReadOnly);

      Buffer.Clear();
      Buffer.SB.Append("SELECT COUNT(*) FROM ");
      Buffer.FormatTableName(TableName);
      return (int)SQLExecuteScalar(Buffer.SB.ToString());
    }

    public override int RecordCount(string TableName, DBxFilter Where)
    {
      CheckTableName(TableName, DBxAccessMode.ReadOnly);
      if (Where != null)
        CheckFilterColumnNames(TableName, Where, false);

      Buffer.Clear();
      Buffer.SB.Append("SELECT COUNT(*) FROM ");
      Buffer.FormatTableName(TableName);

      if (Where != null && (!Where.AlwaysTrue()))
      {
        Buffer.SB.Append(" WHERE ");
        Buffer.FormatFilter(Where);
      }

      return (int)SQLExecuteScalar(Buffer.SB.ToString());
    }
#endif

    /// <summary>
    /// Оптимизированная версия запроса проверки, что таблица пуста.
    /// Обычный запрос SELECT COUNT(*) медленно выполняется в SQLite
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
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
      /*string PrimaryKeyColumnName = */
      Validator.CheckTablePrimaryKeyInt32(tableName);

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

      Buffer.SB.Append("; SELECT last_insert_rowid()");

      Id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (Id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + tableName + "\" Id=" + Id.ToString());

      return Id;
    }

    #endregion

    #endregion

    #region Транзакция


    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public new SQLiteTransaction CurrentTransaction
    {
      get { return (SQLiteTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region Извлечение схемы данных

    /// <summary>
    /// Получить полный список таблиц
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable Table = Connection.GetSchema("Tables"); // исправлено 28.08.2020
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

      DataTable Table = Connection.GetSchema("Columns", new string[] { null, null, tableName }); // исправлено 28.08.2020
      Table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in Table.DefaultView)
      {
        string ColumnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct ColStr = new DBxColumnStruct(ColumnName);

        string ColTypeString = DataTools.GetString(drv.Row, "data_type");
        // 28.08.2020
        // Таблица типов взята из файла System.Data.SQLite\Resources\DataTypes.xml
        // исходных текстов System.Data.SQLite
        switch (ColTypeString)
        {
          case "tinyint":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = 0;
            ColStr.MaxValue = 255;
            break;

          case "smallint":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int16.MinValue;
            ColStr.MaxValue = Int16.MaxValue;
            break;

          case "int":
          case "integer": // По идее, это Int64, но этот тип используется для идентификаторов
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int32.MinValue;
            ColStr.MaxValue = Int32.MaxValue;
            break;

          case "counter": // ?? Надо?
          case "autoincrement": // ?? Надо?
          case "identity": // ?? Надо?
          case "long":
          case "bigint":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int64.MinValue;
            ColStr.MaxValue = Int64.MaxValue;
            break;

          case "single":
            ColStr.ColumnType = DBxColumnType.Float;
            ColStr.MinValue = Single.MinValue;
            ColStr.MaxValue = Single.MaxValue;
            break;

          case "real":
          case "float":
          case "double":
            ColStr.ColumnType = DBxColumnType.Float;
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;

          case "money":
          case "currency":
          case "decimal":
          case "numeric":
            ColStr.ColumnType = DBxColumnType.Money;
            break;

          case "bit":
          case "yesno":
          case "logical":
          case "bool":
          case "boolean":
            ColStr.ColumnType = DBxColumnType.Boolean;
            break;

          case "char":
          case "nchar":
          case "string": // это - под вопросом
            ColStr.ColumnType = DBxColumnType.String;
            break;
          case "smallmoney":

          case "varchar":
          case "nvarchar":
          case "memo":
          case "longtext":
          case "note":
          case "text":
          case "ntext":
            ColStr.ColumnType = DBxColumnType.Memo;
            break;

          case "date":
          case "smalldate": // под вопросом
            ColStr.ColumnType = DBxColumnType.Date;
            break;

          case "time":
            ColStr.ColumnType = DBxColumnType.Time;
            break;

          case "datetime":
          case "timestamp": // ?? под вопросом
            ColStr.ColumnType = DBxColumnType.DateTime;
            break;

          case "binary":
          case "varbinary":
          case "blob":
          case "image":
          case "general":
          case "oleobject": // ?? надо?
            ColStr.ColumnType = DBxColumnType.Binary;
            break;

          case "uniqueidentifier":
          case "guid":
            ColStr.ColumnType = DBxColumnType.Guid;
            break;

          case "xml": // Этого типа нет, я его сам придумал
            ColStr.ColumnType = DBxColumnType.Xml;
            break;

          default:
            throw new BugException("Неизвестный тип поля: \"" + ColTypeString + "\"");
        }

        ColStr.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string NullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (NullableStr)
        {
          case "TRUE": ColStr.Nullable = true; break;  // исправлено 28.08.2020
          case "FALSE": ColStr.Nullable = false; break;
        }

        TableStr.Columns.Add(ColStr);
      }

      #endregion

      #region Определение ссылочных полей

      DataTable schema = Connection.GetSchema("FOREIGNKEYS", new string[] { null, null, tableName, null });
      foreach (DataRow row in schema.Rows)
      {
        string refColName = DataTools.GetString(row, "FKEY_FROM_COLUMN");
        DBxColumnStruct colStr = TableStr.Columns[refColName];
        if (colStr == null)
          continue; // по идее, это ошибка

        colStr.MasterTableName = DataTools.GetString(row, "FKEY_TO_TABLE");

        string mode = DataTools.GetString(row, "FKEY_ON_DELETE");

        // См.режимы: https://www.sqlite.org/foreignkeys.html
        // раздел 4.3

        switch (mode)
        {
          case "SET NULL":
          case "SET DEFAULT":
            colStr.RefType = DBxRefType.Clear;
            break;
          case "CASCADE":
            colStr.RefType = DBxRefType.Delete;
            break;
          default:
            colStr.RefType = DBxRefType.Disallow;
            break;
        }
      }

      #endregion

      TableStr.SetReadOnly();
      return TableStr;
    }


    #endregion

    #region Получение информации

    /// <summary>
    /// Версия SQLite в виде объекта, чтобы его можно было сравнивать
    /// </summary>
    internal Version ServerVersion
    {
      get
      {
        if (_ServerVersion == null)
        {
          try
          {
            string sServerVersion = Connection.ServerVersion;
            if (String.IsNullOrEmpty(sServerVersion))
              _ServerVersion = new Version();
            else
              _ServerVersion = new Version(sServerVersion);
          }
          catch
          {
            _ServerVersion = new Version();
          }
        }
        return _ServerVersion;
      }
    }
    private Version _ServerVersion;

    #endregion

    #region Обновление структуры

    #region Основной метод UpdateDBStruct

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool Modified = false;

      if (UpdateDBStructTables(splash, errors, options))
        Modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return Modified;
    }

    #endregion

    #region Добавление таблиц, столбцов и первичных ключей

    /// <summary>
    /// Имя временной промежуточной таблицы, используемой при обновлении типов полей
    /// </summary>
    private const string AlterTableName = "_TempAlterTable";

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      /*
       * SQLite не поддерживает инструкцию ALTER TABLE ALTER COLUMN.
       * Если требуется изменить тип столбца или ограничение NULL/NOT NULL, то требуется создать
       * копию таблицы
       */

      bool Modified = false;

      #region Извлечение информации из существующей схемы данных

      /*
       * Реализация извлечения схемы данных сразу для всех таблиц неоптимально в System.Data.SQLite.
       * Выгоднее извлекать информацию по каждой таблице отдельно.
       * Также лучше делать вызовы самостоятельно, так как схема содержит много дополнительной информации,
       * которая не нужна для обновления структуры, но для которой делаются дополнительные вызовы.
       */

      DataTable TableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(TableTables);
      dvTables.Sort = "TABLE_NAME";

      //// Столбцы
      //DataTable TableColumns = Connection.GetSchema("Columns", new string[] { String.Empty });
      //DataView dvColumns = new DataView(TableColumns);
      //dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // нужен такой порядок

      //// Данные по индексам 
      //DataTable TableIndices = Connection.GetSchema("Indexes", new string[] { String.Empty }); // каталог "main"
      //DataView dvIndices = new DataView(TableIndices);

      //DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      //DataView dvIndexColumns = new DataView(TableIndexColumns);
      //dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";

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
        //bool IndicesDropped = false;

        List<DBxColumnStruct> ColumnsToAdd;
        DBxColumns ExistedColumns;
        List<DBxColumnStruct> ColumnsToReplaceNullToDef; // Столбцы для замены NULL на DEFAULT

        if (dvTables.Find(Table.TableName) < 0)
        {
          #region Требуется полное создание таблицы

          splash.PhaseText = "Создается таблица \"" + Table.TableName + "\"";
          CreateTable(Table, Table.TableName, options);
          errors.AddInfo("Создана таблица \"" + Table.TableName + "\"");
          Modified = true;

          #endregion
        }
        else if (NeedsRecreateTable(Table, /*dvColumns,*/ errors, out ColumnsToAdd, out ExistedColumns, out ColumnsToReplaceNullToDef))
        {
          #region Требуется пересоздание с использованием промежуточной таблицы

          // См. порядок действий в справочном файле SQLite
          // lang_altertable.html

          DropAllIndices(splash, Table.TableName);
          //IndicesDropped = true;

          splash.PhaseText = "Изменение структуры таблицы \"" + Table.TableName + "\"";

          DropTable(AlterTableName, true);
          PragmaForeighKeys(false);
          try
          {
            using (SQLiteTransaction tran = Connection.BeginTransaction())
            {
              try
              {
                if (ColumnsToReplaceNullToDef != null)
                {
                  foreach (DBxColumnStruct Column in ColumnsToReplaceNullToDef)
                  {
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
                }

                // Создаем промежуточную таблицу
                CreateTable(Table, AlterTableName, options);

                // Переносим данные. Здесь может возникнуть исключение, если типы данных несовместимы
                InsertIntoFrom(AlterTableName, Table.TableName, ExistedColumns);

                // Удаляем исходную таблицу
                DropTable(Table.TableName, false);

                // Переименовываем таблицу
                RenameTable(AlterTableName, Table.TableName);

                tran.Commit();
              }
              catch
              {
                tran.Rollback();
                throw;
              }
            }
          }
          finally
          {
            PragmaForeighKeys(true);
          }
          errors.AddInfo("Таблица \"" + Table.TableName + "\" пересоздана из-за изменения формата столбцов");
          Modified = true;

          #endregion
        }
        else if (ColumnsToAdd != null)
        {
          #region Требуется добавление недостающих столбцлв

          foreach (DBxColumnStruct Column in ColumnsToAdd)
          {
            splash.PhaseText = "Добавление поля \"" + Column.ColumnName + "\"в таблицу \"" + Table.TableName + "\"";
            Buffer.Clear();
            Buffer.SB.Append("ALTER TABLE ");
            Buffer.FormatTableName(Table.TableName);
            Buffer.SB.Append(" ADD "); // а не ADD COLUMN
            AppendColumnDef(/*Table, */Column, false, options, true);
            SQLExecuteNonQuery(Buffer.SB.ToString());
            errors.AddInfo("Создано поле \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName + "\"");
            Modified = true;
          }

          #endregion
        }

        // Таблица существует

        #region Комментарии

        SetTableComment(Table.TableName, Table.Comment);
        for (int i = 0; i < Table.Columns.Count; i++)
          SetColumnComment(Table.TableName, Table.Columns[i].ColumnName, Table.Columns[i].Comment);

        #endregion

        UpdateDBStructIndices(Table, splash, errors, options);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      return Modified;
    }

    /// <summary>
    /// Переименование таблицы
    /// </summary>
    /// <param name="oldName">Имя существующей таблицы</param>
    /// <param name="newName">Новое имя</param>
    private void RenameTable(string oldName, string newName)
    {
      Buffer.Clear();
      Buffer.SB.Append("ALTER TABLE ");
      Buffer.FormatTableName(oldName);
      Buffer.SB.Append(" RENAME TO ");
      Buffer.FormatTableName(newName);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void DropTable(string tableName, bool ifExists)
    {
      Buffer.Clear();
      Buffer.SB.Append("DROP TABLE ");
      if (ifExists)
        Buffer.SB.Append("IF EXISTS ");
      Buffer.FormatTableName(tableName);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Выполнение запроса INSERT INTO xxx SELECT yyy
    /// </summary>
    /// <param name="resTableName">Заполняемая таблица</param>
    /// <param name="srcTableName">Исходная таблица</param>
    /// <param name="columns">Список столбцов для копирования</param>
    private void InsertIntoFrom(string resTableName, string srcTableName, DBxColumns columns)
    {
      if (columns.IsEmpty)
        throw new ArgumentException("Не задан список столбцов для копирования", "columns");

      Buffer.Clear();
      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(resTableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columns);
      Buffer.SB.Append(")");
      Buffer.SB.Append(" SELECT ");
      Buffer.FormatCSColumnNames(columns);
      Buffer.SB.Append(" FROM ");
      Buffer.FormatTableName(srcTableName);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Выполнение запроса PRAGMA foreign keys ON/OFF
    /// </summary>
    /// <param name="isOn"></param>
    private void PragmaForeighKeys(bool isOn)
    {
      Buffer.Clear();
      Buffer.SB.Append("PRAGMA foreigh_keys=");
      Buffer.SB.Append(isOn ? "ON" : "OFF");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Проверяет необходимость пересоздания таблицы
    /// </summary>
    /// <param name="table">Требуемое описание структуры таблицы</param>
    /// <param name="columnsToAdd">Сюда помещаются столбцы из Table.Columns, которые нужно
    /// добавить. Список не определен, если требуется пересохдание таблицы</param>
    /// <param name="errors">Список для добавления информационных сообщений</param>
    /// <param name="existedColumns">Список существующих столбцов таблицы.
    /// Этот список заполняется независимо от того, будет ли пересоздаваться таблица</param>
    /// <param name="columnsToReplaceNullToDef">Список полей, для которых требуется замена NULL на DEFAULT.
    /// Если возвращает null, замены не нужны</param>
    /// <returns>true, если требуется пересоздание таблицы</returns>
    private bool NeedsRecreateTable(DBxTableStruct table, /* DataView dvColumns, */
      ErrorMessageList errors,
      out List<DBxColumnStruct> columnsToAdd,
      out DBxColumns existedColumns,
      out List<DBxColumnStruct> columnsToReplaceNullToDef)
    {
      bool NeedsRecreate = false;
      DBxColumnList lstExistedColumns = new DBxColumnList(table.Columns.Count);
      columnsToAdd = null;
      // не нужен отдельный форматизатор DBxSqlBuffer Buffer2 = new DBxSqlBuffer(Buffer.Formatter);
      columnsToReplaceNullToDef = null;

      Buffer.Clear();
      Buffer.SB.Append("PRAGMA table_info(");
      Buffer.FormatTableName(table.TableName);
      Buffer.SB.Append(")");
      DataTable tblColumns = SQLExecuteDataTable(Buffer.SB.ToString());
      tblColumns.DefaultView.Sort = "name";

      foreach (DBxColumnStruct Column in table.Columns)
      {
        int ColumnRowIndex = tblColumns.DefaultView.Find(Column.ColumnName);
        if (ColumnRowIndex < 0)
        {
          // Поля не существует
          if (columnsToAdd == null)
            columnsToAdd = new List<DBxColumnStruct>();
          columnsToAdd.Add(Column);
        }
        else
        {
          lstExistedColumns.Add(Column.ColumnName);
          // Проверяем соответствие поля
          DataRow ColumnRow = tblColumns.DefaultView[ColumnRowIndex].Row;
          // Проверяем соответствие типа столбца объявлению

          string RealType = DataTools.GetString(ColumnRow, "type").ToUpperInvariant();
          int RealLength;
          SplitValueType(ref RealType, out RealLength);
          bool RealIsPK = DataTools.GetBool(ColumnRow, "pk");
          if (RealIsPK)
          {
            if (RealType == "INTEGER")
              RealType = "INT";
          }

          Buffer.Clear();
          FormatValueType(Buffer, Column, false);
          string WantedType = Buffer.SB.ToString();
          int WantedLength; // равно Column.MaxLength
          SplitValueType(ref WantedType, out WantedLength);

          if (RealType != WantedType)
          {
            errors.AddInfo("Несоответствие типа поля \"" + Column.ColumnName + "\" таблицы \"" +
              table.TableName + "\". Объявление поля типа " + Column.ColumnType.ToString() +
              " предполагает тип " + WantedType +
              " в то время как реальный тип поля " + RealType + ". Требуется пересоздать таблицу");
            NeedsRecreate = true;
          }
          else
          {
            #region Проверка длины строкового поля

            if (Column.ColumnType == DBxColumnType.String)
            {
              // Укорачивать строковые поля никакого смысла не имеет, так как в файле лишние символы
              // не хранятся, как в Access или MS SQL Server.
              // Удлинять надо, так как из-за неправильного описания в программе может срабатывать
              // ограничение, если будет прочитана реальная структура таблицы

              if (RealLength < WantedLength)
              {
                errors.AddInfo("Поле \"" + Column.ColumnName + "\" таблицы \"" +
                      table.TableName + "\" должно иметь длину " + WantedLength.ToString() +
                      " символов, в то время, как реальное поле длиннее:  " + RealLength.ToString() + " символов. Для удлинения поля требуется пересоздать таблицу");
                NeedsRecreate = true;
              }
            } // Строковое поле

            #endregion
          } // RealType==WantedType

          #region Проверка Default

          // Установка свойства DEFAULT должна выполняться до установки NOT NULL, иначе возникнет ошибка

          string WantedDefExpr = String.Empty;
          if (Column.DefaultExpression != null)
          {
            Buffer.Clear();
            Buffer.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
            WantedDefExpr = Buffer.SB.ToString();
          }
          string RealDefExpr = DataTools.GetString(ColumnRow, "dflt_value");
          if (RealDefExpr != WantedDefExpr)
          {
            errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + table.TableName +
              "\" должен быть " + (RealDefExpr.Length > 0 ? "установлен" : "сброшен") + " признак DEFAULT. Для изменения признака DEFAULT требуется пересоздать таблицу");
            NeedsRecreate = true;
          }

          #endregion

          #region Проверка признака Nullable

          // Проверяем Nullable
          bool RealNullable = !DataTools.GetBool(ColumnRow, "notnull");

          if (Column.Nullable != RealNullable)
          {
            errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + table.TableName +
              "\" установлен признак " + (Column.Nullable ? "\"NULL\"" : "\"NOT NULL\"") + ". Для изменения ограничения NULLABLE требуется пересоздать таблицу");
            NeedsRecreate = true;

            if ((!Column.Nullable) && Column.DefaultExpression != null)
            {
              if (columnsToReplaceNullToDef == null)
                columnsToReplaceNullToDef = new List<DBxColumnStruct>();
              columnsToReplaceNullToDef.Add(Column);
            }
          }

          #endregion
        } // поле существует
      } // Цикл по столбцам

      #region Проверяем правильность первичного ключа

      //if (CorrectPrimaryKey(Table, dvIndexColumns, Errors))
      //  Modified = true;

      #endregion

      existedColumns = new DBxColumns(lstExistedColumns); // нет смысла оптимизировать
      return NeedsRecreate;
    }

    /// <summary>
    /// Находим в типе поля "CHAR(x)" скобку и извлекаем длину
    /// </summary>
    /// <param name="typeStr"></param>
    /// <param name="length"></param>
    private static void SplitValueType(ref string typeStr, out int length)
    {
      try
      {
        DoSplitValueType(ref typeStr, out length);
      }
      catch (Exception e)
      {
        throw new ParsingException("Ошибка разбора описания типа \"" + typeStr + "\". " + e.Message, e);
      }
    }

    private static void DoSplitValueType(ref string typeStr, out int length)
    {
      int p1 = typeStr.IndexOf('(');
      if (p1 < 0)
      {
        length = 0;
        return;
      }

      int p2 = typeStr.IndexOf(')');
      string s = typeStr.Substring(p1 + 1, p2 - p1 - 1);
      length = int.Parse(s);
      typeStr = typeStr.Substring(0, p1);
    }

    private void CreateTable(DBxTableStruct table, string tableName, DBxUpdateStructOptions options)
    {
      // 19.01.2020
      // Поддержка первичных ключей, отличных от Integer
      //
      // 09.03.2020
      // Выражение WITHOUT ROWID не применяется, если есть BLOB-поля
      // TODO: Надо бы еще проверять, что средний размер записи не превышает 5% от размера страницы, но это сложно сделать
      // Когда на одной странице помещается мало строк, выгоднее использовать таблицу с rowid.

      int pPrimaryKey;
      bool WithoutRowId;
      switch (table.PrimaryKey.Count)
      {
        case 1:
          pPrimaryKey = table.Columns.IndexOf(table.PrimaryKey[0]);
          WithoutRowId = table.PrimaryKeyColumns[0].ColumnType != DBxColumnType.Int && (!table.Columns.ContainsBlob());
          break;
        case 0:
          pPrimaryKey = -1;
          WithoutRowId = false;
          break;
        default:
          pPrimaryKey = -1;
          //WithoutRowId = true;
          WithoutRowId = !table.Columns.ContainsBlob();
          break;
      }

      Buffer.Clear();
      Buffer.SB.Append("CREATE TABLE ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" (");
      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        AppendColumnDef(/*table, */table.Columns[i], i == 0, options, true);
        if (i == pPrimaryKey)
          Buffer.SB.Append(" PRIMARY KEY");
      }

      if (pPrimaryKey < 0 && table.PrimaryKey.Count > 0)
      {
        Buffer.SB.Append(", PRIMARY KEY (");
        for (int i = 0; i < table.PrimaryKey.Count; i++) // Исправлено 28.12.2020
        {
          if (i > 0)
            Buffer.SB.Append(",");
          Buffer.FormatColumnName(table.PrimaryKey[i]);
        }
        Buffer.SB.Append(")");
      }

      Buffer.SB.Append(")");

      if (WithoutRowId)
        Buffer.SB.Append(" WITHOUT ROWID");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void AppendColumnDef(/*DBxTableStruct table, */DBxColumnStruct column, bool isPrimaryKey, DBxUpdateStructOptions options, bool addDefault)
    {
      Buffer.FormatColumnName(column.ColumnName);
      Buffer.SB.Append(" ");
      FormatValueType(Buffer, column, isPrimaryKey);

      if (column.Nullable)
        Buffer.SB.Append(" NULL");
      else
        Buffer.SB.Append(" NOT NULL");
      if ((!String.IsNullOrEmpty(column.MasterTableName)) && column.RefType != DBxRefType.Emulation)
      {
        if (options.ForeignKeys)
        {
          Buffer.SB.Append(" REFERENCES ");
          Buffer.FormatTableName(column.MasterTableName);
          Buffer.SB.Append(" ");
          Buffer.FormatRefColumnDeleteAction(column.RefType);
        }
      }
      if (addDefault && column.DefaultExpression != null)
      {
        Buffer.SB.Append(" DEFAULT ");
        Buffer.FormatExpression(column.DefaultExpression, new DBxFormatExpressionInfo());
      }
    }

    private void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column, bool isPrimaryKey)
    {
      if (isPrimaryKey)
      {
        if (column.ColumnType == DBxColumnType.Int)
        {
          buffer.SB.Append("INTEGER"); // всегда используем основной тип данных для первичного ключа
          return;
        }
      }
      buffer.FormatValueType(column);
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

    #region Индексы

    /*
     * Используем имена индексов вида "ИмяТаблицы_1", "ИмяТаблицы_2", ... чтобы обеспечить уникальность
     * в пределах базы данных
     */

    private void DropAllIndices(ISplash splash, string tableName)
    {
      splash.PhaseText = "Удаление индексов таблицы \"" + tableName + "\"";

      Buffer.Clear();
      Buffer.SB.Append("PRAGMA index_list(");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(")");
      DataTable tblList = SQLExecuteDataTable(Buffer.SB.ToString());
      foreach (DataRow ListRow in tblList.Rows)
      {
        string IndexName = DataTools.GetString(ListRow, "name");
        DropIndex(/*tableName, */IndexName);
      }
    }

    private void DropIndex(/*string tableName, */string indexName)
    {
      Buffer.Clear();
      Buffer.SB.Append("DROP INDEX \"");
      Buffer.SB.Append(indexName);
      Buffer.SB.Append("\"");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private bool UpdateDBStructIndices(DBxTableStruct table, ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool Modified = false;

      // Таблица существующих индексов
      DataTable tblIndices = CreateIndicesTable(table.TableName);
      tblIndices.DefaultView.Sort = "IndexName";
      DataView dvIdxCols = new DataView(tblIndices);
      dvIdxCols.Sort = "Columns";


      #region Добавляем недостающие индексы

      for (int i = 0; i < table.Indices.Count; i++)
      {
        string WantedCols = table.Indices[i].Columns.AsString;
        int pIndex = dvIdxCols.Find(WantedCols);
        if (pIndex >= 0)
          // Индекс найден
          dvIdxCols[pIndex].Row["Flag"] = true;
        else
        {
          // Требуется создать индекс
          // Имя нового индекса
          string IndexName = GetNewIndexName(table.TableName, tblIndices);

          splash.PhaseText = "Создание индекса \"" + IndexName + "\" для таблицы \"" + table.TableName + "\", столбцы: " + WantedCols;

          Buffer.Clear();
          Buffer.SB.Append("CREATE INDEX \"");
          Buffer.SB.Append(IndexName);
          Buffer.SB.Append("\" ON ");
          Buffer.FormatTableName(table.TableName);
          Buffer.SB.Append(" (");
          Buffer.FormatCSColumnNames(table.Indices[i].Columns);
          Buffer.SB.Append(")");
          SQLExecuteNonQuery(Buffer.SB.ToString());

          Modified = true;
          errors.AddInfo("Создан индекс \"" + IndexName + "\" для таблицы \"" + table.TableName + "\", столбцы: " + WantedCols);

          tblIndices.Rows.Add(IndexName, WantedCols, true); // обязательно добавляем, иначе следующее имя индекса будет неправильным
        }
      } // цикл по объявленным индексам

      #endregion

      #region Удаляем лишние индексы

      if (options.DropUnusedIndices)
      {
        foreach (DataRow IdxRow in tblIndices.Rows)
        {
          if (!DataTools.GetBool(IdxRow, "Flag"))
          {
            string IndexName = DataTools.GetString(IdxRow, "IndexName");
            splash.PhaseText = "Удаление лишнего индекса для таблицы \"" + table.TableName + "\"";
            DropIndex(/*table.TableName, */IndexName);
            errors.AddInfo("Удален лишний индекс \"" + IndexName + "\" в таблице \"" + table.TableName + "\"");
            Modified = true;
          }
        }
      }

      #endregion

      return Modified;
    }

    /// <summary>
    /// Создает таблицу, содержащую поля "IndexName", "Columns" и "Flag".
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    private DataTable CreateIndicesTable(string tableName)
    {
      DataTable tblIndices = new DataTable();
      tblIndices.Columns.Add("IndexName", typeof(string));
      tblIndices.Columns.Add("Columns", typeof(string));
      tblIndices.Columns.Add("Flag", typeof(bool));

#if XXX // Так почему-то не работает для составных индексов. Часть столбцов может не иметь имени
      
          Buffer.Clear();
      Buffer.SB.Append("PRAGMA index_list(");
      Buffer.FormatTableName(TableName);
      Buffer.SB.Append(")");
      DataTable tblList = SQLExecuteDataTable(Buffer.SB.ToString());
      foreach (DataRow ListRow in tblList.Rows)
      {
        DataRow IdxRow = tblIndices.NewRow();
        IdxRow["IndexName"] = ListRow["name"];
          Buffer.Clear();
        Buffer.SB.Append("PRAGMA index_info(\"");
        Buffer.SB.Append(ListRow["name"]);
        Buffer.SB.Append("\")");
        DataTable tblCols = SQLExecuteDataTable(Buffer.SB.ToString());
          Buffer.Clear();
        for (int j = 0; j < tblCols.Rows.Count; j++)
        {
          string ColName = DataTools.GetString(tblCols.Rows[j], "name");
          if (String.IsNullOrEmpty(ColName))
          {
            // Вычисляемый индекс. Не поддерживается
          Buffer.Clear();
            Buffer.SB.Append("[Formula]");
            break;
          }
          if (j > 0)
            Buffer.SB.Append(',');
          Buffer.SB.Append(ColName);
        }
        IdxRow["Columns"] = Buffer.SB.ToString();
        tblIndices.Rows.Add(IdxRow);
      }
#else
      Buffer.Clear();
      Buffer.SB.Append(@"SELECT * FROM main.sqlite_master WHERE [type] = 'index' AND tbl_name=");
      Buffer.FormatValue(tableName, DBxColumnType.String);

      DataTable tblSchema = SQLExecuteDataTable(Buffer.SB.ToString());
      foreach (DataRow ListRow in tblSchema.Rows)
      {
        string IndexName = DataTools.GetString(ListRow, "name");
        string Expr = DataTools.GetString(ListRow, "sql");

        if (Expr.Length == 0)
          continue; // 18.01.2020. Автоиндексы не имеют выражения

        // выражение - это то, что в круглых скобках
        int p1 = Expr.IndexOf('(');
        if (p1 < 0)
          throw new BugException("Не найдена \"(\"");
        int p2 = Expr.LastIndexOf(')');
        if (p2 < p1)
          throw new BugException("Не найдена \")\"");
        Expr = Expr.Substring(p1 + 1, p2 - p1 - 1);
        // внутри могут оставаться пробелы, поля - в кавычках
        string[] a = Expr.Split(',');
        for (int i = 0; i < a.Length; i++)
          a[i] = GetIndexColumnName(a[i]);

        tblIndices.Rows.Add(IndexName, String.Join(",", a));
      }
#endif

      return tblIndices;
    }

    /// <summary>
    /// Извлекаем имя поля, убирая пробелы и кавычки
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string GetIndexColumnName(string s)
    {
      s = s.Trim();
      if (String.IsNullOrEmpty(s))
        return "?";
      if (s[0] == '\"' && s[s.Length - 1] == '\"')
      {
        if (s.Length > 2)
          s = s.Substring(1, s.Length - 2);
        else
          return "?";
      }
      return s;
    }


    /// <summary>
    /// Придумываем имя для нового индекса
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="tblIndices"></param>
    /// <returns></returns>
    private static string GetNewIndexName(string tableName, DataTable tblIndices)
    {
      for (int i = 1; i < 1000; i++)
      {
        string IndexName = tableName + "_" + i.ToString();
        if (tblIndices.DefaultView.Find(IndexName) < 0)
          return IndexName;
      }
      throw new BugException("Не смогли придумать имя для нового индекса таблицы \"" + tableName + "\". Слишком много существующих индексов");
    }
    #endregion

    #endregion
  }

  internal class SQLiteDBxManager : DBxManager
  {
    #region Конструктор

    private SQLiteDBxManager()
      : base(DBxProviderNames.SQLite)
    {
    }

    public static readonly SQLiteDBxManager TheManager = new SQLiteDBxManager();

    #endregion

    #region Переопределенные методы

    public override DBx CreateDBObject(string connectionString)
    {
      return new SQLiteDBx(connectionString);
    }

    public override string ReplaceDBName(string connectionString, string oldDBName, string newDBName)
    {
      SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(connectionString);

      if (csb.DataSource.EndsWith(SQLiteDBx.MemoryFileName))
        return csb.ConnectionString; // 21.07.2021 У баз данных в памяти одно и тоже имя

      //csb.DataSource = ReplaceDBItem(csb.DataSource, OldDBName, NewDBName);
      // 31.07.2018. Вместо простой замены текста, заменяем имя файла
      AbsPath Path = new AbsPath(csb.DataSource);
      if (String.Equals(Path.FileNameWithoutExtension, oldDBName, StringComparison.OrdinalIgnoreCase))
      {
        string FileName = newDBName + Path.Extension;
        Path = new AbsPath(Path.ParentDir, FileName);
        csb.DataSource = Path.Path;
      }

      return csb.ConnectionString;
    }

    /// <summary>
    /// Возвращает ссылку на SQLiteFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }

    public override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
      return new SQLiteConnectionStringBuilder(connectionString);
    }

    #endregion
  }

  /// <summary>
  /// Объект для записи в базу данных SQLite.
  /// Для SQLite нет обходных путей для группового добавления записей, но есть оператор "INSERT OR UPDATE".
  /// Также создаем однократно команду SQLiteCommand
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
      DBxSqlBuffer Buffer = new DBxSqlBuffer(Con.DB.Formatter);
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          FormatInsertSQL(Buffer);
          break;

        case DBxDataWriterMode.Update:
          FormatUpdateSql(Buffer);
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

          FormatUpdateSql(Buffer);
          Buffer.SB.Append(";");
          Buffer.SB.Append(Environment.NewLine);

          //FormatInsertSQL(Buffer);
          Buffer.SB.Append("INSERT INTO ");
          Buffer.FormatTableName(WriterInfo.TableName);
          Buffer.SB.Append(" (");
          Buffer.FormatCSColumnNames(WriterInfo.Columns);
          Buffer.SB.Append(") SELECT ");

          for (int i = 0; i < Values.Length; i++)
          {
            if (i > 0)
              Buffer.SB.Append(',');
            Buffer.FormatParamPlaceholder(i);
          }

          Buffer.SB.Append(" WHERE (SELECT Changes()=0)");
          //}
          break;

        default:
          throw new BugException("Неизвестный Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = new SQLiteCommand(Buffer.SB.ToString());
      for (int i = 0; i < Values.Length; i++)
        _Command.Parameters.Add(new SQLiteParameter("P" + (i + 1).ToString(), null));
      _Command.Connection = ((SQLiteDBxCon)Con).Connection;
      _Command.Transaction = ((SQLiteDBxCon)Con).CurrentTransaction;
      _Command.CommandTimeout = Con.CommandTimeout;
      _Command.Prepare(); // для порядка.
    }

    private void FormatInsertSQL(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(WriterInfo.Columns);
      Buffer.SB.Append(") VALUES (");

      for (int i = 0; i < Values.Length; i++)
      {
        if (i > 0)
          Buffer.SB.Append(',');
        Buffer.FormatParamPlaceholder(i);
      }
      Buffer.SB.Append(")");
    }

    private void FormatUpdateSql(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("UPDATE ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" SET ");

      for (int i = 0; i < OtherColumns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.FormatColumnName(OtherColumns[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(OtherColumnPositions[i]);
      }

      Buffer.SB.Append(" WHERE ");
      for (int i = 0; i < SearchColumns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(" AND ");
        Buffer.FormatColumnName(SearchColumns[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(SearchColumnPositions[i]);
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


      // TODO: Требуется отладка SQL-запроса

      _Command.ExecuteNonQuery();
    }

    #endregion
  }
}
