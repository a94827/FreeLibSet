using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
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

namespace FreeLibSet.Data.SqlClient
{
  #region Перечисление SqlServerEngineEdition

  /// <summary>
  /// Редакция MS SQL Server
  /// </summary>
  public enum SqlServerEngineEdition
  {
    /// <summary>
    /// Personal или Desktop Engine (недоступен для SQL Server 2005 (9.x) и более поздних версий).
    /// </summary>
    Personal = 1,

    /// <summary>
    /// Возвращается для выпусков Standard, Web и Business Intelligence
    /// </summary>
    Standard = 2,

    /// <summary>
    /// Возвращается для выпусков Evaluation, Developer и Enterprise
    /// </summary>
    Enterprise = 3,

    /// <summary>
    /// Возвращается для выпусков Express, Express with Tools и Express with Advanced Services
    /// </summary>
    Express = 4,

    /// <summary>
    /// База данных SQL (?)
    /// </summary>
    SQLDatabase = 5,

    /// <summary>
    /// Раньше называлось SQL Data Warehouse 
    /// </summary>
    AzureSynapseAnalytics = 6,

    /// <summary>
    /// Управляемый экземпляр
    /// </summary>
    ManagedInstance = 8,
  }

  #endregion

  /// <summary>
  /// База данных MS SQL Server
  /// </summary>
  public class SqlDBx : DBx
  {
    #region Конструктор

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public SqlDBx(SqlConnectionStringBuilder connectionStringBuilder)
    {
      if (connectionStringBuilder == null)
        throw new ArgumentNullException("connectionStringBuilder");
      if (!String.IsNullOrEmpty(connectionStringBuilder.AttachDBFilename))
      {
        AbsPath Path = new AbsPath(connectionStringBuilder.AttachDBFilename);
        if (Path.Extension.ToUpperInvariant() != ".MDF")
          throw new ArgumentException("Неправильное значение SqlConnectionStringBuilder.AttachDBFilename=\"" + connectionStringBuilder.AttachDBFilename + "\". " +
            "У файла должно быть расширение \".mdf\", иначе работа с базой данных невозможна", "connectionStringBuilder");
      }

      SyncRoot = new object();

      _DatabaseName = GetDataBaseName(connectionStringBuilder);
      if (!String.IsNullOrEmpty(_DatabaseName))
        base.DisplayName = _DatabaseName;

      SetFormatter(new SqlDBxSqlFormatter(this));

      new SqlDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключение к базе данных.
    /// Автоматически создается основная точка подключения
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public SqlDBx(string connectionString)
      : this(new SqlConnectionStringBuilder(connectionString))
    {
    }

    internal static string GetDataBaseName(SqlConnectionStringBuilder connectionStringBuilder)
    {
      if (!String.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
        return connectionStringBuilder.InitialCatalog;
      else if (!String.IsNullOrEmpty(connectionStringBuilder.AttachDBFilename))
        return connectionStringBuilder.AttachDBFilename;
      else
        throw new ArgumentException("В строке подключения не заданы параметры InitialCatalog или AttachDBFilename, определяющие базу данных");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя базы данных.
    /// Определяется в конструкторе из строки подключения
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private string _DatabaseName;

    /// <summary>
    /// Возвращает ссылку на SqlClientFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SqlClientFactory.Instance; }
    }

    /// <summary>
    /// Возвращает статический экзеипляр менеджера баз данных SQL Server
    /// </summary>
    public override DBxManager Manager { get { return SqlDBxManager.TheManager; } }

    #endregion

    #region Информация о сервере, извлекаемая однократно

    #region Извлечение сведений

    /// <summary>
    /// Используется для синхронизации при вызове методов из нескольких потоков
    /// </summary>
    protected readonly object SyncRoot;

    private void GetReadyServerInfo()
    {
      // Используем "ленивую"

      if (_ServerVersion != null)
        return; // можно не проверять

      lock (SyncRoot)
      {
        if (_ServerVersion == null)
        {
          if (MainEntry == null)
            throw new InvalidOperationException("Основная точка подключения еще не была инициализирована");
          using (SqlDBxCon Con = new SqlDBxCon(MainEntry, true))
          {
            _ServerVersionText = DataTools.GetString(Con.SQLExecuteScalar("SELECT @@VERSION"));
            int p = DataTools.IndexOfAny(_ServerVersionText, "\n\r\t");
            if (p >= 0)
              _ServerVersionText = _ServerVersionText.Substring(0, p);
            _ServerVersionText = _ServerVersionText.Trim();
            _EngineEdition = (SqlServerEngineEdition)DataTools.GetInt(Con.SQLExecuteScalar(@"SELECT SERVERPROPERTY('EngineEdition')"));

            string sServerVesrion = Con.Connection.ServerVersion; // в последнюю очередь
            if (String.IsNullOrEmpty(sServerVesrion))
              _ServerVersion = new Version();
            else
              _ServerVersion = new Version(sServerVesrion);
          }
        }
      }
    }

    #endregion

    /// <summary>
    /// Версия сервера
    /// </summary>
    public Version ServerVersion
    {
      get
      {
        GetReadyServerInfo();
        return _ServerVersion;
      }
    }
    private Version _ServerVersion; // инициализируется обязательно в последнюю очередь

    /// <summary>
    /// Возвращает true, если используется SQL Server 2008 или более новый (10.00 +)
    /// Для SQL Server 2005 возвращает false
    /// </summary>
    public bool IsSqlServer2008orNewer
    {
      get
      {
        return ServerVersion.Major >= 10;
      }
    }

    /// <summary>
    /// Возвращает true, если используется SQL Server 2008 R2 или более новый (10.50 +)
    /// </summary>
    public bool IsSqlServer2008R2orNewer
    {
      get
      {
        if (ServerVersion.Major < 10)
          return false;
        if (ServerVersion.Major > 10)
          return true;
        return ServerVersion.Minor >= 50;
      }
    }

#if XXX
    /// <summary>
    /// Возвращает основную версию MS SQL Server
    /// </summary>
    public int MajorServerVersion
    {
      get
      {
        lock (SyncRoot)
        {
          if (!_MajorServerVersion.HasValue)
          {
            // Определяем версию сервера

            if (MainEntry == null)
              return 0;

            using (SqlDBxCon Con = MainEntry.CreateCon() as SqlDBxCon)
            {
              _MajorServerVersion = int.Parse(Con.Connection.ServerVersion.Substring(0, 2));
            }
          }
          return _MajorServerVersion.Value;
        }
      }
    }
    private int? _MajorServerVersion;
#endif

    /// <summary>
    /// Возвращает версию сервера, возвращаемую запросом "SELECT @@VERSION"
    /// </summary>
    public override string ServerVersionText
    {
      get
      {
        GetReadyServerInfo();
        return _ServerVersionText;
      }
    }
    private string _ServerVersionText;

    /// <summary>
    /// Версия выпуска компонента Database Engine для экземпляра SQL Server.
    /// </summary>
    public SqlServerEngineEdition EngineEdition
    {
      get
      {
        GetReadyServerInfo();
        return _EngineEdition;
      }
    }
    private SqlServerEngineEdition _EngineEdition;

    /// <summary>
    /// Для SQL Server Express возвращает 4Гб или 10Гб
    /// </summary>
    public override long DBSizeLimit
    {
      get
      {
        if (EngineEdition == SqlServerEngineEdition.Express)
        {
          if (IsSqlServer2008R2orNewer)
            return FileTools.GByte * 10;
          else
            return FileTools.GByte * 4;
        }
        else
          return base.DBSizeLimit; // неохота изучать документацию. Для разных редакций есть разные ограничения
      }
    }

    #endregion

    #region Точки подключения

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new SqlDBxEntry MainEntry { get { return (SqlDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// Создает новый объект SqlDBxEntry
    /// </summary>
    /// <param name="Permissions">Требуемые разрешения</param>
    /// <returns>SqlDBxEntry</returns>
    public override DBxEntry CreateEntry(DBxPermissions Permissions)
    {
      return new SqlDBxEntry(this, MainEntry.ConnectionStringBuilder, Permissions);
    }

    #endregion

    #region Структура базы данных

    /// <summary>
    /// Возвращает true, если база данных существует.
    /// Для базы данных SQL Server Express проверяется наличие файла на диске
    /// (при этом она может быть еще не подключена к серверу)
    /// Для "обычной" базы данных проверяется наличие базы, зарегистрированной на сервере.
    /// </summary>
    public override bool DatabaseExists
    {
      get
      {
        if (DatabaseName.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase) && DatabaseName.IndexOf('\\') >= 0)
        {
          // база данных SQL Server Express
          AbsPath FilePath = new AbsPath(DatabaseName);
          if (System.IO.File.Exists(FilePath.Path))
            return true;
          // 19.07.2021. А если файла не существует, то проверяем и обычным способом
        }

        using (SqlDBxCon Con = new SqlDBxCon(MainEntry, true))
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
    /// Создает пустую базу данных, если ее еще нет
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists) // 19.07.2021
        return;
      if (DatabaseName.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
      {
        AbsPath FilePath = new AbsPath(DatabaseName);
        using (SqlDBxCon Con = new SqlDBxCon(MainEntry, true))
        {
          Con.CommandTimeout = 0; // Бесконечное время выполнения
          Con.CreateDatabaseByFile(FilePath);
        }
      }
      else
      {
        using (SqlDBxCon Con = new SqlDBxCon(MainEntry, true))
        {
          Con.CommandTimeout = 0; // Бесконечное время выполнения
          Con.CreateDatabaseByName(DatabaseName);
        }
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
      using (SqlDBxCon Con = new SqlDBxCon(MainEntry, false))
      {
        Con.CommandTimeout = 0; // Бесконечное время выполнения
        return Con.UpdateDBStruct(splash, errors, options);
      }
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает текущий размер базы данных и файла журнала,
    /// используя системную таблицу "sys.master_files"
    /// </summary>
    /// <returns></returns>
    public override long GetDBSize()
    {
      using (SqlDBxCon Con = new SqlDBxCon(MainEntry, false))
      {
        //DataTable tbl = Con.SQLExecuteDataTable(@"EXEC sp_spaceused");
        //if (tbl.Rows.Count != 1)
        //  throw new BugException("Запрос вернул неправильное число строк");
        //long szMB = DataTools.GetInt64(tbl.Rows[0], "database_size");
        // return szMB * FileTools.MByte;

        return DataTools.GetInt64(Con.SQLExecuteScalar(@"SELECT CAST (SUM(size) AS BIGINT) * 8 *1024  FROM sys.master_files WHERE database_id = DB_ID()"));
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

      using (SqlDBxCon Con = new SqlDBxCon(MainEntry, true))
      {
        Con.DropDatabase();
      }

      return true;
    }

    /// <summary>
    /// Возвращает true, если исключение NO_MORE_SPACE_IN_FG
    /// https://msdn.microsoft.com/en-us/library/aa337441.aspx
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override bool IsDBSizeLimitExceededException(Exception e)
    {
      SqlException e2 = e as SqlException;
      if (e2 == null)
        return false;
      return e2.Number == 1105;
    }

    /// <summary>
    /// Создает соединение с сервером без привязки к базе данных для выполнения адиминстративных действий
    /// </summary>
    /// <returns>Объект соединения</returns>
    public SqlDBxCon CreateServerWideCon()
    {
      return new SqlDBxCon(MainEntry, true);
    }

    #endregion
  }

  /// <summary>
  /// Основная или дополнительная точка входа в базу данных
  /// </summary>
  public class SqlDBxEntry : DBxEntry
  {
    #region Конструкторы

    /// <summary>
    /// Защищенный конструктор для основной точки входа
    /// </summary>
    /// <param name="db"></param>
    /// <param name="connectionStringBuilder"></param>
    internal SqlDBxEntry(SqlDBx db, SqlConnectionStringBuilder connectionStringBuilder)
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
    public SqlDBxEntry(SqlDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new SqlConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    /// <param name="permissions">Разрешения</param>
    public SqlDBxEntry(SqlDBx db, SqlConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
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
    public new SqlDBx DB { get { return (SqlDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal SqlConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private SqlConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект SqlDBxCon
    /// </summary>
    /// <returns>Соединение с БД</returns>
    public override DBxConBase CreateCon()
    {
      return new SqlDBxCon(this, false);
    }

    /// <summary>
    /// Создает копию объекта SqlDBxEntry
    /// </summary>
    /// <param name="newPermissions">Разрешения на доступ к базе данных</param>
    /// <returns>Новая точка подключения</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new SqlDBxEntry(DB, ConnectionString, newPermissions);
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
        SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(cs);
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
  /// Соединение с базой данной SQL Server
  /// </summary>
  public class SqlDBxCon : DBxConBase
  {
    #region Конструктор и Dispose

    internal SqlDBxCon(SqlDBxEntry entry, bool serverWide)
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
    /// Точка доступа, к которой относится текущее соедиенение
    /// </summary>
    public new SqlDBxEntry Entry { get { return (SqlDBxEntry)(base.Entry); } }

    /// <summary>
    /// База данных, к которой относится текущее соедиенение
    /// </summary>
    public new SqlDBx DB { get { return (SqlDBx)(base.DB); } }

    /// <summary>
    /// Если true, то данное соедиение относится к соединению с SQL-сервером в-целом,
    /// а не с базой данных. 
    /// Для обычных соединений имеет значение false.
    /// </summary>
    private bool _ServerWide;

    /// <summary>
    /// Возвращает текстовое представление для соединения с добавлением "(ServerWide)", если свойство установлено
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return base.ToString() + (_ServerWide ? " (ServerWide)" : String.Empty);
    }

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    public SqlConnection Connection
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
              SqlConnectionStringBuilder csb2 = new SqlConnectionStringBuilder();
              csb2.ConnectionString = Entry.ConnectionStringBuilder.ConnectionString;
              csb2.InitialCatalog = String.Empty;
              csb2.AttachDBFilename = String.Empty;
              _Connection = new SqlConnection(csb2.ConnectionString);
              DoOpenConnection();
            }
            else
            {
              _Connection = new SqlConnection(Entry.ConnectionString);
              DoOpenConnection();

              // Оператор USE надоедает в LOG-файле
              SuspendTrace();
              try
              {
                SQLExecuteNonQuery("USE [" + DB.DatabaseName + "]");
              }
              finally
              {
                ResumeTrace();
              }

              _Connection.InfoMessage += Connection_InfoMessage;
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

    private SqlConnection _Connection;

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
        e.Data["SqlDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["SqlDBx.DisplayName"] = DB.DisplayName;
        e.Data["SqlDBxEntry.DisplayName"] = Entry.DisplayName;
        e.Data["SqlDBxCon.ServerWide"] = _ServerWide.ToString();
        e.Data["SqlDBxCon.ConnectionString"] = SqlDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
    }

    void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs args)
    {
      System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("g") + " SqlConnection InfoMessage. " + args.Message);
    }

    /// <summary>
    /// Вызывает SqlConnection.ClearPool()
    /// </summary>
    public override void ClearPool()
    {
      SqlConnection.ClearPool(Connection);
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
      SqlCommand cmd = new SqlCommand(cmdText, Connection);
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
      SqlCommand cmd = new SqlCommand(cmdText, Connection);
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
      //if (CmdText == @"SELECT [Id], [ЧисловойКод], [Код], [Значение], [Deleted], [ParentId] FROM [ЭлементыКЗ] WHERE ISNULL([Deleted],0)=0 ORDER BY [ЧисловойКод], [Код]")
      //{ 
      //}

      SqlCommand cmd = new SqlCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      SqlDataAdapter da = new SqlDataAdapter(cmd);
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
      SqlCommand cmd = new SqlCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(SqlCommand cmd, object[] paramValues)
    {
      cmd.Parameters.Clear();
      if (paramValues != null)
      {
        for (int i = 0; i < paramValues.Length; i++)
        {
          SqlParameter Param = new SqlParameter();
          Param.ParameterName = "@P" + (i + 1).ToString();
          Param.Value = paramValues[i];

          if (paramValues[i] != null)
          {
            if (paramValues[i] is Array)
            {
              Param.SqlDbType = SqlDbType.VarBinary;
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

      Buffer.SB.Append("IF EXISTS(SELECT * FROM ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(") SELECT 1 ELSE SELECT 0");

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

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Int32 Id;

      Buffer.SB.Append("INSERT INTO [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("] (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, ColumnTypes);
      Buffer.SB.Append(')');
      Buffer.SB.Append("; SELECT @@IDENTITY");


      Id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (Id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + tableName + "\" Id=" + Id.ToString());

      return Id;
    }

    /// <summary>
    /// Собственный вариант выполнения оператора INSERT INTO, дополненный установкой SET IDENTITY_INSERT
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    public override void AddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);

      DBxColumnType[] ColumnTypes = null;
      ColumnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      //CheckTableLocked(TableName);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений", "values");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      bool HasKeyCol = base.ContainsKeyColumn(tableName, columnNames);

      // Для MS SQL Server необходимо выставлять SET IDENTITY_INSERT ON
      // Но обязательно возвращать обратно, даже если вставка завершилась аварийно
      if (HasKeyCol)
      {
        Buffer.SB.Append("SET IDENTITY_INSERT [");
        Buffer.SB.Append(tableName);
        Buffer.SB.Append("] ON; ");
      }
      Buffer.SB.Append("INSERT INTO [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("] (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, ColumnTypes);
      Buffer.SB.Append(");");
      if (HasKeyCol)
      {
        Buffer.SB.Append(" SET IDENTITY_INSERT [");
        Buffer.SB.Append(tableName);
        Buffer.SB.Append("] OFF");
      }
      try
      {
        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
      catch
      {
        if (HasKeyCol)
        {
          Buffer.Clear();
          Buffer.SB.Append("SET IDENTITY_INSERT [");
          Buffer.SB.Append(tableName);
          Buffer.SB.Append("] OFF");
          SQLExecuteNonQuery(Buffer.SB.ToString());
        }
        throw;
      }
    }

    /// <summary>
    /// Реализация группового добавления строк с помощью <see cref="System.Data.SqlClient.SqlBulkCopy"/> 
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую выполняется запись</param>
    /// <param name="table">Таблица исходных данных</param>
    protected override void DoAddRecords(string tableName, DataTable table)
    {

      bool HasKeyCol = base.ContainsKeyColumn(tableName, table.Columns);

      /*
      if (HasKeyCol)
      {
      Buffer.Clear();
        Buffer.SB.Append("SET IDENTITY_INSERT [");
        Buffer.SB.Append(TableName);
        Buffer.SB.Append("] ON");
        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
      */
      //try
      //{
        SqlBulkCopyOptions Options = SqlBulkCopyOptions.Default;
        if (HasKeyCol)
          Options |= SqlBulkCopyOptions.KeepIdentity;
        using (SqlBulkCopy BC = new SqlBulkCopy(this.Connection, Options, this.CurrentTransaction))
        {
          BC.DestinationTableName = tableName;

          // 13.04.2017
          // Таблицу соответствий задаем всегда, т.к. порядок полей в исходной таблице не
          // обязан совпадать с полями в базе данных
          for (int i = 0; i < table.Columns.Count; i++)
            BC.ColumnMappings.Add(table.Columns[i].ColumnName, table.Columns[i].ColumnName);

          BC.WriteToServer(table);
        }
      //}
      //finally
      //{
        /*
      Buffer.Clear();
        Buffer.SB.Append("SET IDENTITY_INSERT [");
        Buffer.SB.Append(TableName);
        Buffer.SB.Append("] OFF");
        SQLExecuteNonQuery(Buffer.SB.ToString());
         * */
      //}
    }

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

      Buffer.SB.Append("TRUNCATE TABLE [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append(']');
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    #endregion

    /// <summary>
    /// Создает для режима Insert оптимизированный писатель, использующий SqlBulkCopy.
    /// </summary>
    /// <param name="writerInfo">Параметры создаваемого объекта</param>
    /// <returns>Писатель</returns>
    protected override DBxDataWriter OnCreateWriter(DBxDataWriterInfo writerInfo)
    {
      if (writerInfo.Mode == DBxDataWriterMode.Insert)
        return new SqlDBxDataInsertWriter(this, writerInfo);
      else
        return base.OnCreateWriter(writerInfo); // TODO: Пока не реализовано
    }

    #endregion

    #region Транзакция

    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public new SqlTransaction CurrentTransaction
    {
      get { return (SqlTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region Извлечение схемы данных

    /// <summary>
    /// Получить полный список таблиц
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable Table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName, "dbo" });
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

      DataTable Table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "dbo", tableName });
      Table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in Table.DefaultView)
      {
        string ColumnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct ColStr = new DBxColumnStruct(ColumnName);

        string ColTypeString = DataTools.GetString(drv.Row, "data_type");
        switch (ColTypeString)
        {
          // TODO: Сделать все типы

          case "char":
          case "nchar":
            ColStr.ColumnType = DBxColumnType.String;
            break;

          case "bigint":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int64.MinValue;
            ColStr.MaxValue = Int64.MaxValue;
            break;
          case "int":
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int32.MinValue;
            ColStr.MaxValue = Int32.MaxValue;
            break;
          case "smallint":
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
            ColStr.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;
          case "real":
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

          case "bit": ColStr.ColumnType = DBxColumnType.Boolean; break;

          case "date": ColStr.ColumnType = DBxColumnType.Date; break;

          case "datetime":
          case "smalldatetime":
          case "datetimeoffset": // ???
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
        }

        ColStr.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string NullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        //if (NullableStr == "YES")
        //  ColStr.Nullable = true;
        switch (NullableStr) // 01.10.2019
        {
          case "YES": ColStr.Nullable = true; break;
          case "NO": ColStr.Nullable = false; break;
        }

        TableStr.Columns.Add(ColStr);
      }

      #endregion

      #region Определение ссылочных полей

      int TableObjId = GetTableObjId(tableName);
      if (TableObjId == 0)
        throw new BugException("Не удалось получить идентификатор object_id таблицы \"" + tableName + "\"");

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

      TableStr.SetReadOnly();
      return TableStr;
    }

    private int GetTableObjId(string tableName)
    {
      Buffer.Clear();
      Buffer.SB.Append("SELECT [object_id] FROM sys.tables WHERE [Name]=");
      Buffer.FormatValue(tableName, DBxColumnType.String);
      return DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));
    }

    #endregion

    #region Выполнение служебных команд

    internal void CreateDatabaseByFile(AbsPath filePath)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE DATABASE [");
      Buffer.SB.Append(filePath.Path);
      Buffer.SB.Append("] ON PRIMARY (NAME='File1', FILENAME=");
      Buffer.FormatValue(filePath.Path, DBxColumnType.String);
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    internal void CreateDatabaseByName(string databaseName)
    {
      throw new NotImplementedException("Для не Express версии не реализовано");

      // пока не на чем протестировать
      // по идее, вот так
      /*
      Buffer.Clear();
      Buffer.SB.Append("CREATE DATABASE [");
      Buffer.SB.Append(databaseName);
      Buffer.SB.Append("]");
      SQLExecuteNonQuery(Buffer.SB.ToString());
       * */
    }

    internal void DropDatabase()
    {
      // Иначе может сказать "База данных используется"
      // Наше собственное соедиенение доживет до момента закрытия
      //SqlConnection.ClearPool(Connection); // этого мало, так как могли быть соединения с основной строкой подключения
      SqlConnection.ClearAllPools();

      // Нужно почистить историю резеревного копирования.
      // См. сценарий, который создает SQL Management Studio в окошке удаления базц данных.
      Buffer.Clear();
      Buffer.SB.Append("EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = \'");
      Buffer.SB.Append(DB.DatabaseName);
      Buffer.SB.Append("\'");
      SQLExecuteNonQuery(Buffer.SB.ToString());


      Buffer.Clear();
      Buffer.SB.Append("DROP DATABASE [");
      Buffer.SB.Append(DB.DatabaseName);
      Buffer.SB.Append("]");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }


    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
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

      DBxSqlBuffer Buffer2 = new DBxSqlBuffer(Buffer.Formatter); // используется для получения выражений

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        if (Table.Columns.Count == 0)
          throw new DBxStructException(Table, "Не задано ни одного столбца");

        //CheckPrimaryKeyColumn(Table, Table.Columns[0]);

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

          foreach (DBxColumnStruct Column in Table.Columns)
          {
            int ColumnRowIndex = dvColumns.Find(new object[] { Table.TableName, Column.ColumnName });
            if (ColumnRowIndex < 0)
            {
              // Поля не существует
              splash.PhaseText = "Добавление поля \"" + Column.ColumnName + "\"в таблицу \"" + Table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE [");
              Buffer.SB.Append(Table.TableName);
              Buffer.SB.Append("] ADD "); // а не ADD COLUMN
              AppendColumnDef(/*Table, */Column, true);

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
              Buffer2.Clear();
              Buffer2.FormatValueType(Column);
              string WantedType = Buffer2.SB.ToString();
              int p = WantedType.IndexOf('(');
              if (p >= 0)
                WantedType = WantedType.Substring(0, p);

              if (RealType != WantedType)
              {
                errors.AddError("Несоответствие типа поля \"" + Column.ColumnName + "\" таблицы \"" +
                    Table.TableName + "\". Объявление поля типа " + Column.ColumnType.ToString() +
                    " предполагает тип  " + WantedType +
                    " в то время как реальный тип поля " + RealType);

                // TODO: И что с этим делать?
              }
//              else
//              {
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
                      AlterColumn(Table, Column);
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

                if (RealDefExpr != WantedDefExpr)
                {
                  // Вседа сначала убираем старое правило, потом добавляем новое
                  if (RealDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    //Buffer.SB.Append("ALTER TABLE ");
                    //Buffer.FormatTableName(Table.TableName);
                    //Buffer.SB.Append(" ALTER COLUMN ");
                    //Buffer.FormatColumnName(Column.ColumnName);
                    //Buffer.SB.Append(" DROP DEFAULT");

                    // См.
                    // https://stackoverflow.com/questions/1430456/how-to-drop-sql-default-constraint-without-knowing-its-name
                    // ответ №11

                    Buffer.SB.Append(@"DECLARE @ObjectName NVARCHAR(100);");
                    Buffer.SB.Append(Environment.NewLine);
                    Buffer.SB.Append(@"SELECT @ObjectName = OBJECT_NAME([default_object_id]) FROM SYS.COLUMNS WHERE [object_id] = OBJECT_ID('");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(@"') AND [name] = '");
                    Buffer.SB.Append(Column.ColumnName); // а не FormatColumnName() !
                    Buffer.SB.Append(@"';");
                    Buffer.SB.Append(Environment.NewLine);
                    Buffer.SB.Append(@"EXEC('ALTER TABLE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(@" DROP CONSTRAINT ' + @ObjectName)");

                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                        "\" очищен признак DEFAULT");
                  }
                  if (WantedDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append("ADD DEFAULT (");
                    Buffer.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
                    Buffer.SB.Append(") FOR ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    SQLExecuteNonQuery(Buffer.SB.ToString());

                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                      "\" установлен признак DEFAULT " + WantedDefExpr);
                  }
                  Modified = true;
                }

                #endregion

                #region Проверка признака Nullable

                //if (Table.TableName == "ПакетыФормУведомления" && Column.ColumnName == "Таймаут")
                //{ 
                //}

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

                  if ((!Column.Nullable) && Column.DefaultExpression != null && GetRecordCount(Table.TableName) > 0)
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
                  AlterColumn(Table, Column);
                  if (Column.Nullable != RealNullable)
                    errors.AddInfo("Для поля \"" + Column.ColumnName + "\"в таблице \"" + Table.TableName +
                      "\" установлен признак " + (Column.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  Modified = true;
                }

                #endregion

              //}
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
        RemoveExtraIndices(Table, GoodIndices, errors, splash, options.DropUnusedIndices);
        CreateIndices(Table, GoodIndices, errors, splash);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      // Внешние ключи можно создавать только после создания всех таблиц

      if (options.ForeignKeys)
      {
        splash.PhaseText = "Проверка внешних ключей";
        foreach (DBxTableStruct Table in DB.Struct.Tables)
        {
          if (UpdateForeignKeys(Table, dvForeignKeys, splash, errors))
            Modified = true;
        }
      }

      splash.PhaseText = String.Empty;


      splash.PercentMax = 0;
      return Modified;
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

    }
     * */

    private void CreateTable(DBxTableStruct table)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE TABLE [");
      Buffer.SB.Append(table.TableName);
      Buffer.SB.Append("] (");
      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        AppendColumnDef(/*table, */table.Columns[i], true);

        switch (table.PrimaryKey.Count)
        {
          case 0:
            break;
          case 1:
            if (table.Columns[i].ColumnName == table.PrimaryKey[0])
            {
              Buffer.SB.Append(" IDENTITY CONSTRAINT [PK_");
              Buffer.SB.Append(table.TableName);
              Buffer.SB.Append("] PRIMARY KEY");
            }
            break;
          default:
            throw new NotImplementedException("Не реализованы таблицы с составным первичным ключом");
        }
      }
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void AppendColumnDef(/*DBxTableStruct table, */DBxColumnStruct column, bool addDefault)
    {
      Buffer.SB.Append('[');
      Buffer.SB.Append(column.ColumnName);
      Buffer.SB.Append("] ");

      Buffer.FormatValueType(column);

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
    /// <param name="table"></param>
    /// <param name="column"></param>
    private void AlterColumn(DBxTableStruct table, DBxColumnStruct column)
    {
      Buffer.Clear();
      Buffer.SB.Append("ALTER TABLE [");
      Buffer.SB.Append(table.TableName);
      Buffer.SB.Append("] ALTER COLUMN ");
      AppendColumnDef(/*table, */column, false);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

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
      if (table.PrimaryKey.Count > 1)
        throw new NotSupportedException("Составные первичные ключи не поддерживаются");

      if (table.PrimaryKey.Count == 0)
        return false; // TODO: пока не сделано до конца (28.04.2020)

      bool Modified = false;
      bool Found = false;

      string PKIndexName = "PK_" + table.TableName; // имя индекса первичного ключа

      dvIndexColumns.RowFilter = "TABLE_NAME='" + table.TableName + "' AND COLUMN_NAME='" + table.PrimaryKey[0] + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string IndexName = DataTools.GetString(drvCol.Row, "CONSTRAINT_NAME");
        if (IndexName == PKIndexName)
          Found = true;
        else
        {
          if (IndexName.StartsWith("Index"))
            continue; // составной пользовательский индекс, в который входит поле "Id"
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] DROP CONSTRAINT [" + IndexName + "]");
          errors.AddInfo("Удалено неправильное ограничение первичного ключа \"" + IndexName + "\" в таблице \"" + table.TableName + "\"");
          Modified = true;
        }
      }

      if (table.PrimaryKey.Count == 1)
      {
        if (!Found)
        {
          string PKName = "PK_" + table.TableName;
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] ADD CONSTRAINT [" + PKName + "] PRIMARY KEY ([" + table.PrimaryKey[0] + "])");
          errors.AddInfo("Добавлено ограничение первичного ключа \"" + PKName + "\" в таблице \"" + table.TableName + "\"");
          Modified = true;
        }
      }
      else
      {
        if (Found)
        {
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] DROP CONSTRAINT [" + PKIndexName + "]");
          errors.AddInfo("Удалено ограничение первичного ключа \"" + PKIndexName + "\", так как таблица \"" + table.TableName + "\" не содержит первичного ключа");
          Modified = true;
        }
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

        string FKName = "FK_" + table.TableName + "_" + Column.ColumnName;
        if (dvForeignKeys.Find(FKName) >= 0)
        {
          // Ключ существует
          // TODO: Проверка соответствия параметров ключа и удаление его


          continue;
        }

        // Создаем внешний ключ
        splash.PhaseText = "Создание внешнего ключа " + FKName;

        Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE ");
        Buffer.FormatTableName(table.TableName);
        Buffer.SB.Append(" ADD CONSTRAINT [");
        Buffer.SB.Append(FKName);
        Buffer.SB.Append("] FOREIGN KEY (");
        Buffer.FormatColumnName(Column.ColumnName);
        Buffer.SB.Append(") REFERENCES ");
        Buffer.FormatTableName(Column.MasterTableName);
        Buffer.SB.Append(" ");
        Buffer.FormatRefColumnDeleteAction(Column.RefType);

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Создан внешний ключ \"" + FKName + "\" в таблице \"" + table.TableName + "\"");
        Modified = true;
      }

      return Modified; // 28.12.2020
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

        if (!Table.Indexes.Contains(IndexName))
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

        DBxIndexStruct IndexDef = Table.Indexes[IndexName];

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

      using (DataView dv = new DataView(TableIndexColumns))
      {
        dv.Sort = "ORDINAL_POSITION";
        for (int i = 0; i < IndexDef.Columns.Count; i++)
        {
          if (IndexDef.Columns[i] != DataTools.GetString(dv[i].Row, "COLUMN_NAME"))
            return false;
        }
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
      for (int i = 0; i < Table.Indexes.Count; i++)
      {
        DBxIndexStruct IndexDef = Table.Indexes[i];

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

    #endregion
  }

  internal class SqlDBxManager : DBxManager
  {
    #region Конструктор

    private SqlDBxManager()
      : base(DBxProviderNames.Sql)
    {
    }

    public static readonly SqlDBxManager TheManager = new SqlDBxManager();

    #endregion

    #region Переопределенные методы

    public override DBx CreateDBObject(string connectionString)
    {
      return new SqlDBx(connectionString);
    }

    public override string ReplaceDBName(string connectionString, string oldDBName, string newDBName)
    {
      SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(connectionString);
      csb.InitialCatalog = ReplaceDBItem(csb.InitialCatalog, oldDBName, newDBName);
      csb.AttachDBFilename = ReplaceFileItem(csb.AttachDBFilename, oldDBName, newDBName);
      return csb.ConnectionString;
    }

    public override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
      return new SqlConnectionStringBuilder(connectionString);
    }

    /// <summary>
    /// Возвращает ссылку на SqlClientFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SqlClientFactory.Instance; }
    }

    #endregion
  }

#if !XXX
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
      SqlBulkCopyOptions Options = SqlBulkCopyOptions.Default;
      if (base.SearchColumns.Count > 0)
        Options |= SqlBulkCopyOptions.KeepIdentity;

      _BC = new SqlBulkCopy(Con.Connection, Options, Con.CurrentTransaction);
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
    /// SqlBulkCopy требует на входе строки или таблицу.
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
