// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
#define USE_NEW_READER


using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
#if NET
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Logging;

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
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public SqlDBx(SqlConnectionStringBuilder connectionStringBuilder)
    {
      if (connectionStringBuilder == null)
        throw new ArgumentNullException("connectionStringBuilder");
      if (!String.IsNullOrEmpty(connectionStringBuilder.AttachDBFilename))
      {
        AbsPath path = new AbsPath(connectionStringBuilder.AttachDBFilename);
        if (path.Extension.ToUpperInvariant() != ".MDF")
          throw new ArgumentException("Неправильное значение SqlConnectionStringBuilder.AttachDBFilename=\"" + connectionStringBuilder.AttachDBFilename + "\". " +
            "У файла должно быть расширение \".mdf\", иначе работа с базой данных невозможна", "connectionStringBuilder");
      }

      _SyncRoot = new object();

      _DatabaseName = GetDataBaseName(connectionStringBuilder);
      if (!String.IsNullOrEmpty(_DatabaseName))
        base.DisplayName = _DatabaseName;

      SetFormatter(new SqlDBxSqlFormatter(this));

      new SqlDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
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
    /// Определяется в конструкторе из строки подключения.
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private readonly string _DatabaseName;

    /// <summary>
    /// Возвращает ссылку на <see cref="SqlClientFactory"/>
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
    private readonly object _SyncRoot;

    private void GetReadyServerInfo()
    {
      // Используем "ленивую"

      if (_ServerVersion != null)
        return; // можно не проверять

      lock (_SyncRoot)
      {
        if (_ServerVersion == null)
        {
          if (MainEntry == null)
            throw new InvalidOperationException("Основная точка подключения еще не была инициализирована");
          using (SqlDBxCon con = new SqlDBxCon(MainEntry, true))
          {
            _ServerVersionText = DataTools.GetString(con.SQLExecuteScalar("SELECT @@VERSION"));
            int p = DataTools.IndexOfAny(_ServerVersionText, "\n\r\t");
            if (p >= 0)
              _ServerVersionText = _ServerVersionText.Substring(0, p);
            _ServerVersionText = _ServerVersionText.Trim();
            _EngineEdition = (SqlServerEngineEdition)DataTools.GetInt(con.SQLExecuteScalar(@"SELECT SERVERPROPERTY('EngineEdition')"));

            string sServerVesrion = con.Connection.ServerVersion; // в последнюю очередь
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

    /*
    internal static SqlDbType GetSqlDbType(DBxColumnStruct colDef)
    {
      switch (colDef.ColumnType)
      {
        case DBxColumnType.String: return SqlDbType.NVarChar;
        case DBxColumnType.Int:return SqlDbType.Int; // TODO:
        case DBxColumnType.Float: return SqlDbType.Float; // TODO:
        case DBxColumnType.Decimal:return SqlDbType.Decimal;
        case DBxColumnType.Boolean:return SqlDbType.Bit;
        case DBxColumnType.Date:return SqlDbType.Date;
        case DBxColumnType.DateTime: return SqlDbType.DateTime2;
        case DBxColumnType.Time:return SqlDbType.Time;
        case DBxColumnType.Guid:return SqlDbType.NVarChar; // TODO:
        case DBxColumnType.Memo: return SqlDbType.NVarChar;
        case DBxColumnType.Xml:return SqlDbType.Xml;
        case DBxColumnType.Binary: return SqlDbType.VarBinary;
        default: return SqlDbType.Variant; // ?
}
  }
    */

    internal static void InitSqlParameter(DBxColumnStruct colDef, SqlParameter p)
    {
      switch (colDef.ColumnType)
      {
        case DBxColumnType.String:
          p.SqlDbType = SqlDbType.NChar;
          p.Size = colDef.MaxLength;
          break;
        case DBxColumnType.Int:
          p.SqlDbType = SqlDbType.Int; // TODO:
          break;
        case DBxColumnType.Float:
          p.SqlDbType = SqlDbType.Float; // TODO:
          break;
        case DBxColumnType.Decimal:
          p.SqlDbType = SqlDbType.Decimal;
          break;
        case DBxColumnType.Boolean:
          p.SqlDbType = SqlDbType.Bit;
          break;
        case DBxColumnType.Date:
          p.SqlDbType = SqlDbType.Date;
          break;
        case DBxColumnType.DateTime:
          p.SqlDbType = SqlDbType.DateTime2;
          p.Size = 7; // 23.07.2024
          break;
        case DBxColumnType.Time:
          p.SqlDbType = SqlDbType.Time;
          break;
        case DBxColumnType.Guid:
          p.SqlDbType = SqlDbType.NChar;
          p.Size = 36;//?
          break;
        case DBxColumnType.Memo:
          p.SqlDbType = SqlDbType.NVarChar;
          break;
        case DBxColumnType.Xml:
          p.SqlDbType = SqlDbType.Xml;
          break;
        case DBxColumnType.Binary:
          p.SqlDbType = SqlDbType.VarBinary;
          break;
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

    /// <summary>
    /// Минимальная дата, поддерживаемая SQL Server 2005
    /// </summary>
    public static readonly DateTime MinDateTimeSqlServer2005 = new DateTime(1753, 1, 1);

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
          AbsPath filePath = new AbsPath(DatabaseName);
          if (System.IO.File.Exists(filePath.Path))
            return true;
          // 19.07.2021. А если файла не существует, то проверяем и обычным способом
        }

        using (SqlDBxCon con = new SqlDBxCon(MainEntry, true))
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
    /// Создает пустую базу данных, если ее еще нет
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists) // 19.07.2021
        return;
      if (DatabaseName.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
      {
        AbsPath filePath = new AbsPath(DatabaseName);
        using (SqlDBxCon con = new SqlDBxCon(MainEntry, true))
        {
          con.CommandTimeout = 0; // Бесконечное время выполнения
          con.CreateDatabaseByFile(filePath);
        }
      }
      else
      {
        using (SqlDBxCon con = new SqlDBxCon(MainEntry, true))
        {
          con.CommandTimeout = 0; // Бесконечное время выполнения
          con.CreateDatabaseByName(DatabaseName);
        }
      }
    }

    /// <summary>                               
    /// Обновляет структуру существующей базы
    /// данных на основании созданного описание в свойстве <see cref="DBx.Struct"/>.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство <see cref="ISplash.PhaseText"/> для отображения выполняемых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    protected override bool OnUpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // Делегируем все действия соединению, т.к. нужен доступ к защищенным методам
      using (SqlDBxCon con = new SqlDBxCon(MainEntry, false))
      {
        con.CommandTimeout = 0; // Бесконечное время выполнения
        return con.UpdateDBStruct(splash, errors, options);
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
      using (SqlDBxCon con = new SqlDBxCon(MainEntry, false))
      {
        //DataTable tbl = Con.SQLExecuteDataTable(@"EXEC sp_spaceused");
        //if (tbl.Rows.Count != 1)
        //  throw new BugException("Запрос вернул неправильное число строк");
        //long szMB = DataTools.GetInt64(tbl.Rows[0], "database_size");
        // return szMB * FileTools.MByte;

        return DataTools.GetInt64(con.SQLExecuteScalar(@"SELECT CAST (SUM(size) AS BIGINT) * 8 *1024  FROM sys.master_files WHERE database_id = DB_ID()"));
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

      using (SqlDBxCon con = new SqlDBxCon(MainEntry, true))
      {
        con.DropDatabase();
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


    /// <summary>
    /// Проверка корректности имени таблицы.
    /// Возвращает true, если имя правильное. 
    /// Наличие реальной таблицы с таким именем не проверяется.
    /// 
    /// Разрешает имена временных таблиц, начинающихся с "#" или "##"
    /// </summary>
    /// <param name="tableName">Проверяемое имя таблицы</param>
    /// <param name="errorText">Сообщение об ошибке</param>
    /// <returns>true, если имя правильное</returns>
    public override bool IsValidTableName(string tableName, out string errorText)
    {
      if (String.IsNullOrEmpty(tableName))
      {
        errorText = "Имя таблицы не задано";
        return false;
      }

      // 25.05.2023
      if (tableName[0] == '#')
      {
        if (tableName.StartsWith("##", StringComparison.Ordinal))
          tableName = "__" + tableName.Substring(2);
        else
          tableName = "_" + tableName.Substring(1);
      }

      return CheckInvalidChars(tableName, out errorText);
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
    private readonly SqlConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private readonly string _ConnectionString;

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
    private readonly bool _ServerWide;

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
    /// Абстрактный метод выполнения SQL-запроса, возвращающего единственное значение
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
    /// <returns>Количество записей, обработанных в запросе, или (-1), если неизвестно</returns>
    protected override int DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      SqlCommand cmd = new SqlCommand(cmdText, Connection);
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
      //if (CmdText == @"SELECT [Id], [ЧисловойКод], [Код], [Значение], [Deleted], [ParentId] FROM [ЭлементыКЗ] WHERE ISNULL([Deleted],0)=0 ORDER BY [ЧисловойКод], [Код]")
      //{ 
      //}

      SqlCommand cmd = new SqlCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      SqlDataAdapter da = new SqlDataAdapter(cmd);
      DataTable table = new DataTable(tableName);
      da.FillError += new FillErrorEventHandler(HandleDataAdapterFillError); // 03.10.2018
      da.Fill(table);
      return table;
    }

    /// <summary>
    /// Абстрактный метод выполнения SQL-запроса, возвращающего DbDataReader
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
          SqlParameter sqlParam = new SqlParameter();
          sqlParam.ParameterName = "@P" + (i + 1).ToString();
          sqlParam.Value = paramValues[i];

          if (paramValues[i] != null)
          {
            if (paramValues[i] is Array)
            {
              sqlParam.SqlDbType = SqlDbType.VarBinary;
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
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Int32 id;

      Buffer.SB.Append("INSERT INTO [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("] (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, columnTypes);
      Buffer.SB.Append(')');
      Buffer.SB.Append("; SELECT @@IDENTITY");


      id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + tableName + "\" Id=" + id.ToString());

      return id;
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

      DBxColumnType[] columnTypes = null;
      columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      //CheckTableLocked(TableName);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений", "values");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      bool hasKeyCol = base.ContainsKeyColumn(tableName, columnNames);

      // Для MS SQL Server необходимо выставлять SET IDENTITY_INSERT ON
      // Но обязательно возвращать обратно, даже если вставка завершилась аварийно
      if (hasKeyCol)
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
      Buffer.FormatCSValues(values, columnTypes);
      Buffer.SB.Append(");");
      if (hasKeyCol)
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
        if (hasKeyCol)
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
      if (!DB.IsSqlServer2008orNewer)
        table = ReplaceTimeSpanColumns(table); // 04.06.2023

      bool hasKeyCol = base.ContainsKeyColumn(tableName, table.Columns);

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
      SqlBulkCopyOptions options = SqlBulkCopyOptions.Default;
      if (hasKeyCol)
        options |= SqlBulkCopyOptions.KeepIdentity;
      using (SqlBulkCopy bc = new SqlBulkCopy(this.Connection, options, this.CurrentTransaction))
      {
        bc.DestinationTableName = tableName;

        // 13.04.2017
        // Таблицу соответствий задаем всегда, т.к. порядок полей в исходной таблице не
        // обязан совпадать с полями в базе данных
        for (int i = 0; i < table.Columns.Count; i++)
          bc.ColumnMappings.Add(table.Columns[i].ColumnName, table.Columns[i].ColumnName);

        bc.WriteToServer(table);
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

    /// <summary>
    /// MS SQL Server 2005 не поддерживает столбцы типа TimeSpan
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private static DataTable ReplaceTimeSpanColumns(DataTable table)
    {
      bool hasTimeSpan = false;
      foreach (DataColumn col in table.Columns)
      {
        if (col.DataType == typeof(TimeSpan))
        {
          hasTimeSpan = true;
          break;
        }
      }

      if (!hasTimeSpan)
        return table;

      List<int> timeSpanColIndexes = new List<int>();
      DataTable table2 = table.Clone();
      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (table.Columns[i].DataType == typeof(TimeSpan))
        {
          table2.Columns[i].DataType = typeof(DateTime);
          timeSpanColIndexes.Add(i);
        }
      }
#if DEBUG
      if (timeSpanColIndexes.Count < 1)
        throw new BugException();
#endif

      foreach (DataRow srcRow in table.Rows)
      {
        object[] a = srcRow.ItemArray;
        for (int i = 0; i < timeSpanColIndexes.Count; i++)
        {
          int colIndex = timeSpanColIndexes[i];
          if (a[colIndex] is TimeSpan) // не DBNull
            a[colIndex] = DataTools.GetDateTime((TimeSpan)(a[colIndex]), SqlDBx.MinDateTimeSqlServer2005);
        }
        table2.Rows.Add(a);
      }
      table2.AcceptChanges();
      return table2;
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
#if USE_NEW_READER
      return new SqlDBxDataWriter(this, writerInfo);
#else
      if (writerInfo.Mode == DBxDataWriterMode.Insert)
        return new SqlDBxDataInsertWriter(this, writerInfo);
      else
        return base.OnCreateWriter(writerInfo); // TODO: Пока не реализовано
#endif
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
      DataTable table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName, "dbo" });
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
      DBxTableStruct TableStr = new DBxTableStruct(tableName);

      #region Список столбцов, тип, MaxLen, Nullable

      DataTable table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "dbo", tableName });
      table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct colDef = new DBxColumnStruct(columnName);

        string colTypeString = DataTools.GetString(drv.Row, "data_type");
        switch (colTypeString)
        {
          // TODO: Сделать все типы

          case "char":
          case "nchar":
            colDef.ColumnType = DBxColumnType.String;
            break;

          case "bigint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int64.MinValue;
            colDef.MaxValue = Int64.MaxValue;
            break;
          case "int":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int32.MinValue;
            colDef.MaxValue = Int32.MaxValue;
            break;
          case "smallint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int16.MinValue;
            colDef.MaxValue = Int16.MaxValue;
            break;
          case "tinyint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = 0;
            colDef.MaxValue = 255;
            break;

          case "float":
            colDef.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            colDef.MinValue = Double.MinValue;
            colDef.MaxValue = Double.MaxValue;
            break;
          case "real":
            colDef.ColumnType = DBxColumnType.Float;
            // TODO: Использовать длину поля для разделения float/double
            colDef.MinValue = Single.MinValue;
            colDef.MaxValue = Single.MaxValue;
            break;

          case "money":
            colDef.ColumnType = DBxColumnType.Decimal;
            colDef.MinValue = -922337203685477.5808;
            colDef.MaxValue = 922337203685477.5807;
            break;
          case "smallmoney":
            colDef.ColumnType = DBxColumnType.Decimal;
            colDef.MinValue = -214748.3648;
            colDef.MaxValue = 214748.3647;
            break;

          case "bit": colDef.ColumnType = DBxColumnType.Boolean; break;

          case "date": colDef.ColumnType = DBxColumnType.Date; break;

          case "datetime":
          case "smalldatetime":
          case "datetimeoffset": // ???
          case "datetime2": colDef.ColumnType = DBxColumnType.DateTime; break;

          case "time": colDef.ColumnType = DBxColumnType.Time; break;

          case "varchar":
          case "nvarchar":
          case "text":
          case "ntext":
            colDef.ColumnType = DBxColumnType.Memo;
            break;

          case "image":
          case "varbinary":
            colDef.ColumnType = DBxColumnType.Binary;
            break;

          case "binary":
            colDef.ColumnType = DBxColumnType.Binary;
            //ColStr.MaxLength=
            break;

          case "xml":
            colDef.ColumnType = DBxColumnType.Xml;
            break;
        }

        colDef.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string nullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        //if (NullableStr == "YES")
        //  ColStr.Nullable = true;
        switch (nullableStr) // 01.10.2019
        {
          case "YES": colDef.Nullable = true; break;
          case "NO": colDef.Nullable = false; break;
        }

        if ((!colDef.Nullable) && (!drv.Row.IsNull("column_default")))
        {
          try
          {
            // 09.06.2023
            string sDefault = DataTools.GetString(drv.Row, "column_default");
            //if
            //colDef.DefaultValue = DBxTools.Convert(sDefault, colDef.ColumnType);
            Buffer.Clear();
            if (sDefault.Length > 2)
            {
              if (sDefault[0] == '(' && sDefault[sDefault.Length - 1] == ')')
                sDefault = sDefault.Substring(1, sDefault.Length - 2);
              Buffer.SB.Append("SELECT ");
              Buffer.SB.Append(sDefault);
              colDef.DefaultValue = SQLExecuteScalar(Buffer.SB.ToString());
            }
          }
          catch (Exception e)
          {
            e.Data["DB"] = DB.ToString();
            e.Data["Table"] = tableName;
            e.Data["Column"] = colDef.ColumnName;
            LogoutTools.LogoutException(e, "Ошибка получения значения DBxColumnStruct.DefaultValue");
          }
        }

        TableStr.Columns.Add(colDef);
      }

      #endregion

      #region Определение ссылочных полей

      int tableObjId = GetTableObjId(tableName);
      if (tableObjId == 0)
        throw new BugException("Не удалось получить идентификатор object_id таблицы \"" + tableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append("SELECT [name],[object_id],[referenced_object_id],[delete_referential_action] FROM sys.foreign_keys WHERE parent_object_id=");
      Buffer.FormatValue(tableObjId, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "sys.foreign_keys");
      foreach (DataRow row in tbl.Rows)
      {
        int fkObjId = DataTools.GetInt(row, "object_id");
        int refTableObjId = DataTools.GetInt(row, "referenced_object_id");

        Buffer.Clear();
        Buffer.SB.Append("SELECT [parent_column_id] FROM sys.foreign_key_columns WHERE constraint_object_id=");
        Buffer.FormatValue(fkObjId, DBxColumnType.Int);
        // TODO: Нужно сделать метод SQLExecuteScalarSingle(), который будет работать как ExecuteScalar(), но с проверкой количества данных в DataReader'е
        DataTable tbl2 = SQLExecuteDataTable(Buffer.SB.ToString(), "sys.foreign_key_columns");
        if (tbl2.Rows.Count == 0)
          throw new BugException("Не найдено ни одного столбца для ограничения с constraint_object_id=" + fkObjId.ToString());
        if (tbl2.Rows.Count > 1)
          throw new BugException("Ограничение с constraint_object_id=" + fkObjId.ToString() + " содержит несколько столбцов (" + tbl2.Rows.Count.ToString() + ")");
        int ParentColumnId = DataTools.GetInt(tbl2.Rows[0], "parent_column_id");

        Buffer.Clear();
        Buffer.SB.Append("SELECT [name] FROM sys.columns WHERE object_id=");
        Buffer.FormatValue(tableObjId, DBxColumnType.Int);
        Buffer.SB.Append(" AND column_id=");
        Buffer.FormatValue(ParentColumnId, DBxColumnType.Int);
        string parentColumnName = DataTools.GetString(SQLExecuteScalar(Buffer.SB.ToString()));
        if (String.IsNullOrEmpty(parentColumnName))
          throw new BugException("Не удалось определить имя ссылочного поля");

        Buffer.Clear();
        Buffer.SB.Append("SELECT [name] FROM sys.tables WHERE object_id=");
        Buffer.FormatValue(refTableObjId, DBxColumnType.Int);
        string RefTableName = DataTools.GetString(SQLExecuteScalar(Buffer.SB.ToString()));
        if (String.IsNullOrEmpty(RefTableName))
          throw new BugException("Не удалось определить имя ссылочной таблицы для RefTableId=" + refTableObjId.ToString());

        DBxColumnStruct colDef = TableStr.Columns[parentColumnName];
        colDef.MasterTableName = RefTableName;

        int refTypeCode = DataTools.GetInt(row, "delete_referential_action");
        switch (refTypeCode)
        {
          case 0: colDef.RefType = DBxRefType.Disallow; break;
          case 1: colDef.RefType = DBxRefType.Delete; break;
          case 2: colDef.RefType = DBxRefType.Clear; break;
          case 3: colDef.RefType = DBxRefType.Clear; break; // устанавливается значение по умолчанию
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
      Buffer.Clear();
      Buffer.SB.Append("CREATE DATABASE [");
      Buffer.SB.Append(databaseName);
      Buffer.SB.Append("]");
      SQLExecuteNonQuery(Buffer.SB.ToString());
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
      bool modified = false;

      #region Извлечение информации из существующей схемы данных

      DataTable tableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(tableTables);
      dvTables.Sort = "TABLE_NAME";

      // Столбцы
      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME";

      // Данные по внешним ключам
      DataTable tableForeignKeys = Connection.GetSchema("ForeignKeys");
      DataView dvForeignKeys = new DataView(tableForeignKeys);
      dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // Данные по индексам 
      DataTable tableIndexes = Connection.GetSchema("Indexes");
      DataView dvIndexes = new DataView(tableIndexes);

      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME,ORDINAL_POSITION";
      //DebugTools.DebugDataView(dvIndexColumns, "Столбцы индексов базы данных "+DB.DisplayName);

      #endregion

      DBxSqlBuffer buffer2 = new DBxSqlBuffer(Buffer.Formatter); // используется для получения выражений

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;


        //CheckPrimaryKeyColumn(Table, Table.Columns[0]);

        if (dvTables.Find(table.TableName) < 0)
        {
          #region Требуется полное создание таблицы

          splash.PhaseText = "Создается таблица \"" + table.TableName + "\"";
          CreateTable(table, table.TableName, false);
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

          foreach (DBxColumnStruct colDef in table.Columns)
          {
            int ColumnRowIndex = dvColumns.Find(new object[] { table.TableName, colDef.ColumnName });
            if (ColumnRowIndex < 0)
            {
              // Поля не существует
              splash.PhaseText = "Добавление поля \"" + colDef.ColumnName + "\"в таблицу \"" + table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE [");
              Buffer.SB.Append(table.TableName);
              Buffer.SB.Append("] ADD "); // а не ADD COLUMN
              AppendColumnDef(/*Table, */colDef, true);

              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("Создано поле \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName + "\"");
              modified = true;
            }
            else
            {
              // Проверяем соответствие поля
              DataRow columnRow = dvColumns[ColumnRowIndex].Row;
              // Проверяем соответствие типа столбца объявлению
              string realType = DataTools.GetString(columnRow, "DATA_TYPE").ToUpperInvariant();
              buffer2.Clear();
              buffer2.FormatValueType(colDef);
              string wantedType = buffer2.SB.ToString();
              int p = wantedType.IndexOf('(');
              if (p >= 0)
                wantedType = wantedType.Substring(0, p);

              if (realType != wantedType)
              {
                errors.AddError("Несоответствие типа поля \"" + colDef.ColumnName + "\" таблицы \"" +
                    table.TableName + "\". Объявление поля типа " + colDef.ColumnType.ToString() +
                    " предполагает тип  " + wantedType +
                    " в то время как реальный тип поля " + realType);

                // TODO: И что с этим делать?
              }
              //              else
              //              {
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
                    AlterColumn(table, colDef);
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
              string RealDefExpr = DataTools.GetString(columnRow, "COLUMN_DEFAULT");

              if (RealDefExpr != wantedDefExpr)
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
                  Buffer.FormatTableName(table.TableName);
                  Buffer.SB.Append(@"') AND [name] = '");
                  Buffer.SB.Append(colDef.ColumnName); // а не FormatColumnName() !
                  Buffer.SB.Append(@"';");
                  Buffer.SB.Append(Environment.NewLine);
                  Buffer.SB.Append(@"EXEC('ALTER TABLE ");
                  Buffer.FormatTableName(table.TableName);
                  Buffer.SB.Append(@" DROP CONSTRAINT ' + @ObjectName)");

                  SQLExecuteNonQuery(Buffer.SB.ToString());
                  errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                      "\" очищен признак DEFAULT");
                }
                if (wantedDefExpr.Length > 0)
                {
                  Buffer.Clear();
                  Buffer.SB.Append("ALTER TABLE ");
                  Buffer.FormatTableName(table.TableName);
                  Buffer.SB.Append("ADD DEFAULT (");
                  Buffer.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
                  Buffer.SB.Append(") FOR ");
                  Buffer.FormatColumnName(colDef.ColumnName);
                  SQLExecuteNonQuery(Buffer.SB.ToString());

                  errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                    "\" установлен признак DEFAULT " + wantedDefExpr);
                }
                modified = true;
              }

              #endregion

              #region Проверка признака Nullable

              //if (Table.TableName == "ПакетыФормУведомления" && Column.ColumnName == "Таймаут")
              //{ 
              //}

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

                if ((!colDef.Nullable) && colDef.DefaultExpression != null && GetRecordCount(table.TableName) > 0)
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
                AlterColumn(table, colDef);
                if (colDef.Nullable != realNullable)
                  errors.AddInfo("Для поля \"" + colDef.ColumnName + "\"в таблице \"" + table.TableName +
                    "\" установлен признак " + (colDef.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                modified = true;
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
        List<string> goodIndices = new List<string>();
        RemoveExtraIndices(table, goodIndices, errors, splash, options.DropUnusedIndices);
        CreateIndices(table, goodIndices, errors, splash);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      // Внешние ключи можно создавать только после создания всех таблиц

      if (options.ForeignKeys)
      {
        splash.PhaseText = "Проверка внешних ключей";
        foreach (DBxTableStruct table in DB.Struct.Tables)
        {
          if (UpdateForeignKeys(table, dvForeignKeys, splash, errors))
            modified = true;
        }
      }

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;

      return modified;
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

    private void CreateTable(DBxTableStruct table, string tableName, bool isTempTable)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE TABLE [");
      Buffer.SB.Append(tableName);
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
              Buffer.SB.Append(" IDENTITY ");
              if (!isTempTable)
              {
                Buffer.SB.Append("CONSTRAINT [PK_");
                Buffer.SB.Append(tableName);
                Buffer.SB.Append("] ");
              }
              Buffer.SB.Append("PRIMARY KEY");
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
    /// новое ограничение создается.
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

      bool modified = false;
      bool found = false;

      string pkIndexName = "PK_" + table.TableName; // имя индекса первичного ключа

      dvIndexColumns.RowFilter = "TABLE_NAME='" + table.TableName + "' AND COLUMN_NAME='" + table.PrimaryKey[0] + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string indexName = DataTools.GetString(drvCol.Row, "CONSTRAINT_NAME");
        if (indexName == pkIndexName)
          found = true;
        else
        {
          if (indexName.StartsWith("Index", StringComparison.Ordinal))
            continue; // составной пользовательский индекс, в который входит поле "Id"
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] DROP CONSTRAINT [" + indexName + "]");
          errors.AddInfo("Удалено неправильное ограничение первичного ключа \"" + indexName + "\" в таблице \"" + table.TableName + "\"");
          modified = true;
        }
      }

      if (table.PrimaryKey.Count == 1)
      {
        if (!found)
        {
          string pkName = "PK_" + table.TableName;
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] ADD CONSTRAINT [" + pkName + "] PRIMARY KEY ([" + table.PrimaryKey[0] + "])");
          errors.AddInfo("Добавлено ограничение первичного ключа \"" + pkName + "\" в таблице \"" + table.TableName + "\"");
          modified = true;
        }
      }
      else
      {
        if (found)
        {
          SQLExecuteNonQuery("ALTER TABLE [" + table.TableName + "] DROP CONSTRAINT [" + pkIndexName + "]");
          errors.AddInfo("Удалено ограничение первичного ключа \"" + pkIndexName + "\", так как таблица \"" + table.TableName + "\" не содержит первичного ключа");
          modified = true;
        }
      }

      return modified;
    }

    private bool UpdateForeignKeys(DBxTableStruct table, DataView dvForeignKeys, ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;

      foreach (DBxColumnStruct colDef in table.Columns)
      {
        if (String.IsNullOrEmpty(colDef.MasterTableName))
          continue;
        if (colDef.RefType == DBxRefType.Emulation)
          continue;

        string fkName = "FK_" + table.TableName + "_" + colDef.ColumnName;
        if (dvForeignKeys.Find(fkName) >= 0)
        {
          // Ключ существует
          // TODO: Проверка соответствия параметров ключа и удаление его


          continue;
        }

        // Создаем внешний ключ
        splash.PhaseText = "Создание внешнего ключа " + fkName;

        Buffer.Clear();
        Buffer.SB.Append("ALTER TABLE ");
        Buffer.FormatTableName(table.TableName);
        Buffer.SB.Append(" ADD CONSTRAINT [");
        Buffer.SB.Append(fkName);
        Buffer.SB.Append("] FOREIGN KEY (");
        Buffer.FormatColumnName(colDef.ColumnName);
        Buffer.SB.Append(") REFERENCES ");
        Buffer.FormatTableName(colDef.MasterTableName);
        Buffer.SB.Append(" ");
        BaseDBxSqlFormatter.FormatRefColumnDeleteAction(Buffer, colDef.RefType);

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Создан внешний ключ \"" + fkName + "\" в таблице \"" + table.TableName + "\"");
        modified = true;
      }

      return modified; // 28.12.2020
    }

    #region Индексы

    private void RemoveExtraIndices(DBxTableStruct table, List<string> goodIndices, ErrorMessageList errors, ISplash splash, bool drop)
    {
      DataTable tableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, table.TableName });
      foreach (DataRow indexRow in tableIndexes.Rows)
      {
        string indexName = DataTools.GetString(indexRow, "INDEX_NAME");
        //        if (IndexName == "PrimaryKey")
        //          continue;
        if (indexName.StartsWith("FK_", StringComparison.Ordinal) || indexName.StartsWith("PK_", StringComparison.Ordinal))
          continue;

        if (!table.Indexes.Contains(indexName))
        {
          // Лишний индекс
          if (drop)
          {
            splash.PhaseText = "Удаление индекса " + indexName;
            DropIndex(table.TableName, indexName);
            errors.AddInfo("Удален лишний индекс \"" + indexName + "\" в таблице \"" + table.TableName + "\"");
          }
          continue;
        }

        DBxIndexStruct indexDef = table.Indexes[indexName];

        DataTable tableIndexColumns = Connection.GetSchema("IndexColumns", new string[] { null, null, table.TableName, indexName });
        if (!CheckIndexColumns(indexDef, tableIndexColumns))
        {
          // Описание индекса не соответствует
          splash.PhaseText = "Удаление индекса " + indexName;
          DropIndex(table.TableName, indexName);
          errors.AddInfo("Удален индекс \"" + indexName + "\" в таблице \"" + table.TableName + "\", т.к. он не соответствует объявленному в структуре данных");
          continue;
        }

        // Индекс хороший
        goodIndices.Add(indexDef.IndexName);
      }
    }

    private static bool CheckIndexColumns(DBxIndexStruct indexDef, DataTable tableIndexColumns)
    {
      if (indexDef.Columns.Count != tableIndexColumns.Rows.Count)
        return false;

      using (DataView dv = new DataView(tableIndexColumns))
      {
        dv.Sort = "ORDINAL_POSITION";
        for (int i = 0; i < indexDef.Columns.Count; i++)
        {
          if (indexDef.Columns[i] != DataTools.GetString(dv[i].Row, "COLUMN_NAME"))
            return false;
        }
      }
      return true;
    }

    private void DropIndex(string tableName, string indexName)
    {
      Buffer.Clear();
      Buffer.SB.Append("DROP INDEX [");
      Buffer.SB.Append(indexName);
      Buffer.SB.Append("] ON [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("]");

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    private void CreateIndices(DBxTableStruct table, List<string> goodIndices, ErrorMessageList errors, ISplash splash)
    {
      for (int i = 0; i < table.Indexes.Count; i++)
      {
        DBxIndexStruct indexDef = table.Indexes[i];

        if (goodIndices.Contains(indexDef.IndexName))
          continue;

        splash.PhaseText = "Создание индекса таблицы \"" + table.TableName + "\" для полей \"" + indexDef.Columns.ToString() + "\"";
        CreateIndex(table.TableName, indexDef.IndexName, indexDef.Columns);
        errors.AddInfo("Создан индекс \"" + indexDef.IndexName + "\" в таблице \"" + table.TableName + "\" с полями \"" + indexDef.Columns.ToString() + "\"");
      }
    }

    /// <summary>
    /// Добавление одного индекса
    /// </summary>
    private void CreateIndex(string tableName, string indexName, DBxColumns columns)
    {
      Buffer.Clear();
      Buffer.SB.Append("CREATE INDEX [");
      Buffer.SB.Append(indexName);
      Buffer.SB.Append("] ON [");
      Buffer.SB.Append(tableName);
      Buffer.SB.Append("] (");
      for (int i = 0; i < columns.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.SB.Append("[");
        Buffer.SB.Append(columns[i]);
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
        //        if (IndexName == "PrimaryKey")
        //          continue;
        if (indexName.StartsWith("FK_", StringComparison.Ordinal) || indexName.StartsWith("PK_", StringComparison.Ordinal))
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
        Buffer.SB.Append("DROP INDEX [");
        Buffer.SB.Append(indexNames[i]);
        Buffer.SB.Append("] ON [");
        Buffer.SB.Append(tableName);
        Buffer.SB.Append("]");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("Удален индекс \"" + indexNames[i] + "\" в таблице \"" + tableName + "\"");
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

    #region Временные таблицы

    /// <summary>
    /// Структуры объявленных временных таблиц.
    /// Ключ - имя временной таблицы, Значение - структура
    /// </summary>
    private Dictionary<string, DBxTableStruct> _TempTableDict;

    /// <summary>
    /// Создает временную таблицу с заданной структурой.
    /// Таблица будет существовать, пока не закрыто соединение.
    /// Имя таблицы генерируется автоматически, а не берется из <see cref="DBxTableStruct.TableName"/>.
    /// 
    /// Временная реализация для тестов.
    /// </summary>
    /// <param name="tableStruct">Описание структуры. Должно быть заполнено</param>
    /// <returns>Имя временной таблицы, которое следует использовать в запросах</returns>
    public string CreateTempTableInternal(DBxTableStruct tableStruct)
    {
      if (_TempTableDict == null)
        _TempTableDict = new Dictionary<string, DBxTableStruct>();

      string tableName = "#Temp" + StdConvert.ToString(_TempTableDict.Count + 1);
      DBxTableStruct tableStruct2 = tableStruct.Clone(tableName);

      _TempTableDict.Add(tableName, tableStruct2);

      CreateTable(tableStruct2, tableName, true);
      return tableName;
    }

    /// <summary>
    /// Возвращает описание структуры таблицы, в том числе, временной для этого соединения.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура или null</returns>
    public override DBxTableStruct GetTableStruct(string tableName)
    {
      if (tableName.StartsWith("#Temp", StringComparison.Ordinal))
      {
        if (_TempTableDict == null)
          _TempTableDict = new Dictionary<string, DBxTableStruct>();
        DBxTableStruct res;
        if (_TempTableDict.TryGetValue(tableName, out res))
          return res;
        else
          throw new ArgumentException("Неизвестное имя временной таблицы \"" + tableName + "\"", "tableName");
      }

      return base.GetTableStruct(tableName);
    }

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
}
