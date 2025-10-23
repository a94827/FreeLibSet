// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.Common;
using System.Data.SQLite;
using FreeLibSet.IO;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Провайдер для доступа к базе данных SQLite

namespace FreeLibSet.Data.SQLite
{
  /// <summary>
  /// База данных SQLite.
  /// </summary>
  public class SQLiteDBx : DBx
  {
    #region Конструкторы и Dispose

    internal const string MemoryFileName = ":memory:";

    /// <summary>
    /// Устанавливается в 1 при первом вызове конструктора
    /// </summary>
    private static int _FirstFlagValue = 0;

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public SQLiteDBx(SQLiteConnectionStringBuilder connectionStringBuilder)
    {
      bool firstCall = false;
      if (System.Threading.Interlocked.Exchange(ref _FirstFlagValue, 1) == 0) // могут быть асинхронные вызовы конструктора
      {
        LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
        firstCall = true;
      }

      _SyncRoot = new object();

      _InMemory = connectionStringBuilder.DataSource.EndsWith(MemoryFileName, StringComparison.Ordinal);
      if (!InMemory)
      {
        _FileName = new AbsPath(connectionStringBuilder.DataSource);
        if (_FileName.IsEmpty)
          throw new ArgumentException(Res.SQLiteDBx_Arg_NoDataSource, "connectionStringBuilder");
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

      if (firstCall && UseInvariantStringFunctions)
        InitInvariantStringFunctions();
    }

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public SQLiteDBx(string connectionString)
      : this(new SQLiteConnectionStringBuilder(connectionString))
    {
    }

    /// <summary>
    /// Открывает базу данных по указанному пути.
    /// Устанавливает в строке подключения атрибут "foreign keys=true".
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
        throw ExceptionFactory.ArgIsEmpty("path");
      SQLiteConnectionStringBuilder b = new SQLiteConnectionStringBuilder();
      b.DataSource = path.Path;
      b.ReadOnly = readOnly;
      b.ForeignKeys = true; // 02.02.2022
      return b;
    }

    /// <summary>
    /// Эта версия конструктора предназначена для создания базы данных в памяти
    /// </summary>
    public SQLiteDBx()
      : this("Data Source=" + MemoryFileName
          + ";foreign keys=true" // 02.02.2022
          )
    {
    }

    /// <summary>
    /// Если InMemory=true, то закрывает соединение с базой данных, что приводит к ее удалению.
    /// </summary>
    /// <param name="disposing">true, если вызван метод <see cref="IDisposable.Dispose()"/></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          // 16.10.2023.
          // Выделено в отдельный метод, на случай, если в конструкторе не удалось загрузить сборку System.Data.SQLite.dll.
          // Было бы плохо пытаться загрузить ее в деструкторе и получить исключение
          DisposeMainConnection();
        }
        catch { }
      }

      base.Dispose(disposing);
    }

    private void DisposeMainConnection()
    {
      if (MainConnection != null)
      {
        MainConnection.Dispose();
        MainConnection = null;
      }
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
    private readonly AbsPath _FileName;

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
      System.IO.FileInfo fi = new System.IO.FileInfo(FileName.Path);
      return fi.Length;
    }

    /// <summary>
    /// Возвращает статический экземпляр менеджера <see cref="SQLiteDBxManager"/>.
    /// </summary>
    public override DBxManager Manager { get { return SQLiteDBxManager.TheManager; } }

    /// <summary>
    /// Возвращает true, если база данных располагается в памяти
    /// </summary>
    public bool InMemory { get { return _InMemory; } }
    private readonly bool _InMemory;

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

    /// <summary>
    /// Минимальная дата, которую можно хранить в базе данных
    /// </summary>
    internal static readonly DateTime MinDate = DateTime.SpecifyKind(new DateTime(1000, 1, 1), DateTimeKind.Unspecified);

    #endregion

    #region Обновление структуры

    /// <summary>
    /// Возвращает признак существования файла <see cref="FileName"/>
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
      using (SQLiteDBxCon con = new SQLiteDBxCon(MainEntry))
      {
        // Ничего не делаем. Само создается
        // 21.07.2021. Неверно. Надо обязательно обратиться к свойству Connection

        con.Connection.GetSchema();
      }
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
      using (SQLiteDBxCon con = new SQLiteDBxCon(MainEntry))
      {
        con.CommandTimeout = 0; // Бесконечное время выполнения
        return con.UpdateDBStruct(splash, errors, options);
      }
    }

    /// <summary>
    /// Удаляет таблицу данных, если она существует.
    /// Этот метод должен вызываться до установки свойства <see cref="DBx.Struct"/> и вызова <see cref="DBx.UpdateStruct()"/>.
    /// Если обновление структуры не предполагается, после последовательности вызовов этого метода,
    /// должна быть выполнена установка <see cref="DBx"/>.Struct=null, чтобы обновить список таблиц.
    /// </summary>
    /// <param name="tableName">Имя удаляемой таблицы</param>
    public override void DropTableIfExists(string tableName)
    {
      using (DBxConBase con = MainEntry.CreateCon())
      {
        con.NameCheckingEnabled = false;
        DBxSqlBuffer buffer = new DBxSqlBuffer(this.Formatter);
        buffer.SB.Append("DROP TABLE IF EXISTS ");
        con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        buffer.FormatTableName(tableName);
        con.SQLExecuteNonQuery(buffer.SB.ToString());
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

      if (!InMemory)
      {
        SQLiteConnection.ClearAllPools();
        System.IO.File.Delete(FileName.Path);
      }

      return true;
    }

    /// <summary>
    /// Возвращает ссылку на <see cref="SQLiteFactory"/>
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }

    /// <summary>
    /// Обрезка текстовых полей не требуется для SQLite
    /// </summary>
    public override bool UseTrimEnd { get { return false; } }

    #endregion

    #region Инициализация функций

    /// <summary>
    /// Если true (по умолчанию), то будут заменены функции UPPER(s) и LOWER().
    /// Они будут вызывать методы <see cref="String.ToUpperInvariant()"/> и <see cref="String.ToLowerInvariant()"/> соответственно.
    /// Свойство можно устанавливать только до первого вызова конструктора <see cref="SQLiteDBx"/>.
    /// </summary>
    public static bool UseInvariantStringFunctions
    {
      get { return _UseInvariantStringFunctions; }
      set
      {
        if (_FirstFlagValue != 0)
          throw ExceptionFactory.ConstructorAlreadyCalled(typeof(SQLiteDBx));
        _UseInvariantStringFunctions = value;
      }
    }
    private static bool _UseInvariantStringFunctions = true;

    /// <summary>
    /// Инициализация функций UPPER() и LOWER().
    /// Вызывается в конце работы конструктора при создании первой базы данных
    /// </summary>
    private void InitInvariantStringFunctions()
    {
      // До инициализации функций меняем установку для LIKE, делая его регистрочувствительным
      /* Как-то не так работает, как ожидается
      using (DBxConBase con = MainEntry.CreateCon())
      {
        con.SQLExecuteNonQuery("PRAGMA case_sensitive_like=1");
      }
       * */

      // См. https://stackoverflow.com/questions/10349839/turkish-character-in-sqlite-while-using-like-expression/10361892#10361892
      // Только там определены функция TOUPPER() а не UPPER

      SQLiteFunction.RegisterFunction(typeof(UPPERFunction));
      SQLiteFunction.RegisterFunction(typeof(LOWERFunction));
      //CollationCaseInsensitive.RegisterFunction(typeof(CollationCaseInsensitive));
    }

    [SQLiteFunctionAttribute(Name = "UPPER", Arguments = 1, FuncType = FunctionType.Scalar)]
    private class UPPERFunction : SQLiteFunction
    {
      public override object Invoke(object[] args)
      {
        return args[0].ToString().ToUpperInvariant();
      }
    }

    [SQLiteFunctionAttribute(Name = "LOWER", Arguments = 1, FuncType = FunctionType.Scalar)]
    private class LOWERFunction : SQLiteFunction
    {
      public override object Invoke(object[] args)
      {
        return args[0].ToString().ToLowerInvariant();
      }
    }

    //[SQLiteFunction(Name = "COLLATION_CASE_INSENSITIVE", FuncType = FunctionType.Collation)]
    //class CollationCaseInsensitive : SQLiteFunction
    //{
    //  public override int Compare(string param1, string param2) //According to Turkish character sorting to patch
    //  {
    //    return String.Compare(param1, param2, true);
    //  }
    //}

    #endregion

    #region Отладка использования памяти

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
      catch (Exception e) { args.WriteLine("*** Error ***. " + e.Message); }

      args.WriteLine("SQLiteConnection.GetMemoryStatistics()");
      args.IndentLevel++;
      try
      {
        IDictionary<string, long> dict = null;
        SQLiteConnection.GetMemoryStatistics(ref dict);
        LogoutTools.LogoutObject(args, dict);
      }
      catch (Exception e) { args.WriteLine("*** Error ***. " + e.Message); }
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
    private readonly SQLiteConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private readonly string _ConnectionString;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект <see cref="SQLiteDBxCon"/>
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
        return String.Format(Res.DBx_Err_GetConnectionString, e.Message);
      }
    }

    #endregion
  }

  /// <summary>
  /// Соедиенение с базой данных SQLite.
  /// Для создания объекта используйте <see cref="SQLiteDBxEntry.CreateCon()"/>.
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
    /// <param name="disposing">True, если был вызван метод <see cref="IDisposable.Dispose()"/>.
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
    /// Объект создается при первом обращении к свойству.
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
        e.Data["SQLiteDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["SQLiteDBxCon.ConnectionString"] = SQLiteDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
    }

    /// <summary>
    /// Вызывает <see cref="SQLiteConnection.ClearPool(SQLiteConnection)"/>
    /// </summary>
    public override void ClearPool()
    {
      SQLiteConnection.ClearPool(Connection);
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
    /// <returns>Количество записей, обработанных в запросе, или (-1), если неизвестно</returns>
    [DebuggerStepThrough]
    protected override int DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
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
    /// Абстрактный метод выполнения SQL-запроса, возвращающего <see cref="DbDataReader"/>
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
          SQLiteParameter sqlParam = new SQLiteParameter();
          //Param.ParameterName = "@P" + (i + 1).ToString();
          sqlParam.ParameterName = ":P" + (i + 1).ToString(); // исправлено 29.05.2020
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

    /// <summary>
    /// Возвращает оптимизированную реализацию писателя
    /// </summary>
    /// <param name="writerInfo">Параметры писателя</param>
    /// <returns>Писатель</returns>
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

      return !DataTools.GetBoolean(SQLExecuteScalar(Buffer.SB.ToString()));
    }

    #endregion

    #region Добавление записей

    /// <summary>
    /// Добавить строку с автоматическим присвоением идентификатора.
    /// Полученный идентификатор возвращается и может быть использован для ссылок на строку.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    /// <returns>Идентификатор добавленной записи</returns>
    public override object AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      /*string PrimaryKeyColumnName = */
      Validator.CheckTablePrimaryKeyInteger(tableName);

      if (columnNames.Count != values.Length)
        throw ExceptionFactory.ArgWrongCollectionCount("values", values, columnNames.Count);

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, columnTypes);
      Buffer.SB.Append(')');

      Buffer.SB.Append("; SELECT last_insert_rowid()");

      object id = SQLExecuteScalar(Buffer.SB.ToString());

      if (DataTools.IsEmptyValue(id))
        throw new BugException("Invalid record identifier returned for table \"" + tableName + "\" Id=" + DataTools.GetString(id));

      return id;
    }

    #endregion

    #endregion

    #region Транзакция

    /// <summary>
    /// Текущая транзакция, если был вызов метода <see cref="DBxConBase.TransactionBegin()"/>, или null, если нет активной транзакции
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
      DataTable table = Connection.GetSchema("Tables"); // исправлено 28.08.2020
      string[] a = new string[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
        a[i] = DataTools.GetString(table.Rows[i], "table_name");

      return a;
    }

    /// <summary>
    /// Получить реальное описание структуры таблицы.
    /// SQLite не поддерживает описания таблиц и столбцов, несмотря на наличие в схеме столбца COLUMN_DESCRIPTION
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct tableStr = new DBxTableStruct(tableName);

      #region Список столбцов, тип, MaxLen, Nullable, DefaultValue

      DataTable table = Connection.GetSchema("Columns", new string[] { null, null, tableName }); // исправлено 28.08.2020
      table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct colDef = new DBxColumnStruct(columnName);

        string colTypeString = DataTools.GetString(drv.Row, "data_type");
        // 28.08.2020
        // Таблица типов взята из файла System.Data.SQLite\Resources\DataTypes.xml
        // исходных текстов System.Data.SQLite
        switch (colTypeString)
        {
          case "tinyint":
            colDef.ColumnType = DBxColumnType.Byte;
            break;

          case "smallint":
            colDef.ColumnType = DBxColumnType.Int16;
            break;

          case "int":
          case "integer": // По идее, это Int64, но этот тип используется для идентификаторов
            colDef.ColumnType = DBxColumnType.Int32;
            break;

          case "counter": // ?? Надо?
          case "autoincrement": // ?? Надо?
          case "identity": // ?? Надо?
          case "long":
          case "bigint":
            colDef.ColumnType = DBxColumnType.Int64;
            break;

          case "single":
            colDef.ColumnType = DBxColumnType.Single;
            break;

          case "real":
          case "float":
          case "double":
          case "double precision": // 05.09.2025
            colDef.ColumnType = DBxColumnType.Double;
            break;

          case "money":
          case "currency":
          case "decimal":
          case "numeric":
            colDef.ColumnType = DBxColumnType.Decimal;
            break;

          case "bit":
          case "yesno":
          case "logical":
          case "bool":
          case "boolean":
            colDef.ColumnType = DBxColumnType.Boolean;
            break;

          case "char":
          case "nchar":
          case "string": // это - под вопросом
            colDef.ColumnType = DBxColumnType.String;
            break;
          case "smallmoney":

          case "varchar":
          case "nvarchar":
          case "memo":
          case "longtext":
          case "note":
          case "text":
          case "ntext":
          case "clob":
            colDef.ColumnType = DBxColumnType.Memo;
            break;

          case "date":
          case "smalldate": // под вопросом
            colDef.ColumnType = DBxColumnType.Date;
            break;

          case "time":
            colDef.ColumnType = DBxColumnType.Time;
            break;

          case "datetime":
          case "timestamp": // ?? под вопросом
            colDef.ColumnType = DBxColumnType.DateTime;
            break;

          case "binary":
          case "varbinary":
          case "blob":
          case "image":
          case "general":
          case "oleobject": // ?? надо?
            colDef.ColumnType = DBxColumnType.Binary;
            break;

          case "uniqueidentifier":
          case "guid":
            colDef.ColumnType = DBxColumnType.Guid;
            break;

          case "xml": // Этого типа нет, я его сам придумал
            colDef.ColumnType = DBxColumnType.Xml;
            break;

          default:
            throw new BugException("Wrong column type: \"" + colTypeString + "\"");
        }

        if (colDef.ColumnType == DBxColumnType.String) // 08.06.2023
          colDef.MaxLength = DataTools.GetInt32(drv.Row, "character_maximum_length");

        string nullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (nullableStr)
        {
          case "TRUE": colDef.Nullable = true; break;  // исправлено 28.08.2020
          case "FALSE": colDef.Nullable = false; break;
        }

        if ((!colDef.Nullable) && (!drv.Row.IsNull("column_default")))
        {
          try
          {
            // 09.06.2023
            string sDefault = DataTools.GetString(drv.Row, "column_default");
            colDef.DefaultValue = DBxTools.Convert(sDefault, colDef.ColumnType);
          }
          catch (Exception e)
          {
            e.Data["DB"] = DB.ToString();
            e.Data["Table"] = tableName;
            e.Data["Column"] = colDef.ColumnName;
            LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("DBxColumnStruct.DefaultValue"));
          }
        }

        tableStr.Columns.Add(colDef);
      }

      #endregion

      #region Определение ссылочных полей

      DataTable schema = Connection.GetSchema("FOREIGNKEYS", new string[] { null, null, tableName, null });
      foreach (DataRow row in schema.Rows)
      {
        string refColName = DataTools.GetString(row, "FKEY_FROM_COLUMN");
        DBxColumnStruct colStr = tableStr.Columns[refColName];
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

      tableStr.SetReadOnly();
      return tableStr;
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

    internal bool UpdateDBStruct(ISimpleSplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool modified = false;

      if (UpdateDBStructTables(splash, errors, options))
        modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return modified;
    }

    #endregion

    #region Добавление таблиц, столбцов и первичных ключей

    /// <summary>
    /// Имя временной промежуточной таблицы, используемой при обновлении типов полей
    /// </summary>
    private const string AlterTableName = "_TempAlterTable";

    private bool UpdateDBStructTables(ISimpleSplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      /*
       * SQLite не поддерживает инструкцию ALTER TABLE ALTER COLUMN.
       * Если требуется изменить тип столбца или ограничение NULL/NOT NULL, то требуется создать
       * копию таблицы
       */

      bool modified = false;

      #region Извлечение информации из существующей схемы данных

      /*
       * Реализация извлечения схемы данных сразу для всех таблиц неоптимально в System.Data.SQLite.
       * Выгоднее извлекать информацию по каждой таблице отдельно.
       * Также лучше делать вызовы самостоятельно, так как схема содержит много дополнительной информации,
       * которая не нужна для обновления структуры, но для которой делаются дополнительные вызовы.
       */

      DataTable tableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(tableTables);
      dvTables.Sort = "TABLE_NAME";

      //// Столбцы
      //DataTable TableColumns = Connection.GetSchema("Columns", new string[] { String.Empty });
      //DataView dvColumns = new DataView(TableColumns);
      //dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // нужен такой порядок

      //// Данные по индексам 
      //DataTable TableIndexes = Connection.GetSchema("Indexes", new string[] { String.Empty }); // каталог "main"
      //DataView dvIndexes = new DataView(TableIndices);

      //DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      //DataView dvIndexColumns = new DataView(TableIndexColumns);
      //dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;

      // Цикл по таблицам
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        //CheckPrimaryKeyColumn(Table, Table.PrimaryKeyColumns[0]);
        //bool IndicesDropped = false;

        List<DBxColumnStruct> columnsToAdd;
        DBxColumns existedColumns;
        List<DBxColumnStruct> columnsToReplaceNullToDef; // Столбцы для замены NULL на DEFAULT

        if (dvTables.Find(table.TableName) < 0)
        {
          #region Требуется полное создание таблицы

          splash.PhaseText = DBxUITools.PhaseText.TableCreation(table);
          CreateTable(table, table.TableName, options);
          errors.AddInfo(DBxUITools.UpdateMsg.TableCreated(table));
          modified = true;

          #endregion
        }
        else if (NeedsRecreateTable(table, /*dvColumns,*/ errors, out columnsToAdd, out existedColumns, out columnsToReplaceNullToDef))
        {
          #region Требуется пересоздание с использованием промежуточной таблицы

          // См. порядок действий в справочном файле SQLite
          // lang_altertable.html

          DropAllIndices(splash, table);
          //IndicesDropped = true;

          splash.PhaseText = String.Format(Res.SQLiteDBx_Phase_TableStructChanging, table.TableName);

          DropTable(AlterTableName, true);
          PragmaForeighKeys(false);
          try
          {
            using (SQLiteTransaction tran = Connection.BeginTransaction())
            {
              try
              {
                if (columnsToReplaceNullToDef != null)
                {
                  foreach (DBxColumnStruct colDef in columnsToReplaceNullToDef)
                  {
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
                    errors.AddInfo(DBxUITools.UpdateMsg.NullToDefaultValue(table, colDef));
                  }
                }

                // Создаем промежуточную таблицу
                CreateTable(table, AlterTableName, options);

                // Переносим данные. Здесь может возникнуть исключение, если типы данных несовместимы
                InsertIntoFrom(AlterTableName, table.TableName, existedColumns);

                // Удаляем исходную таблицу
                DropTable(table.TableName, false);

                // Переименовываем таблицу
                RenameTable(AlterTableName, table.TableName);

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
          errors.AddInfo(String.Format(Res.SQLiteDBx_Msg_TableRecreatedByColumnFormat,
            table));
          modified = true;

          #endregion
        }
        else if (columnsToAdd != null)
        {
          #region Требуется добавление недостающих столбцлв

          foreach (DBxColumnStruct colDef in columnsToAdd)
          {
            splash.PhaseText = DBxUITools.PhaseText.ColumnCreation(table, colDef);
            Buffer.Clear();
            Buffer.SB.Append("ALTER TABLE ");
            Buffer.FormatTableName(table.TableName);
            Buffer.SB.Append(" ADD "); // а не ADD COLUMN
            AppendColumnDef(/*Table, */colDef, false, options, true);
            SQLExecuteNonQuery(Buffer.SB.ToString());
            errors.AddInfo(DBxUITools.UpdateMsg.ColumnCreated(table, colDef));
            modified = true;
          }

          #endregion
        }

        // Таблица существует

        #region Комментарии

        SetTableComment(table.TableName, table.Comment);
        for (int i = 0; i < table.Columns.Count; i++)
          SetColumnComment(table.TableName, table.Columns[i].ColumnName, table.Columns[i].Comment);

        #endregion

        UpdateDBStructIndices(table, splash, errors, options);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      return modified;
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
        throw ExceptionFactory.ArgIsEmpty("columns");

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
      bool needsRecreate = false;
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

      foreach (DBxColumnStruct colDef in table.Columns)
      {
        int columnRowIndex = tblColumns.DefaultView.Find(colDef.ColumnName);
        if (columnRowIndex < 0)
        {
          // Поля не существует
          if (columnsToAdd == null)
            columnsToAdd = new List<DBxColumnStruct>();
          columnsToAdd.Add(colDef);
        }
        else
        {
          lstExistedColumns.Add(colDef.ColumnName);
          // Проверяем соответствие поля
          DataRow columnRow = tblColumns.DefaultView[columnRowIndex].Row;
          // Проверяем соответствие типа столбца объявлению

          string realType = DataTools.GetString(columnRow, "type").ToUpperInvariant();
          int realLength;
          SplitColumnType(ref realType, out realLength);
          bool realIsPK = DataTools.GetBoolean(columnRow, "pk");
          if (realIsPK)
          {
            if (realType == "INTEGER")
              realType = "INT";
          }

          Buffer.Clear();
          FormatValueType(Buffer, colDef, false);
          string wantedType = Buffer.SB.ToString();
          int wantedLength; // равно Column.MaxLength
          SplitColumnType(ref wantedType, out wantedLength);

          if (realType != wantedType)
          {
            errors.AddInfo(DBxUITools.UpdateMsg.ColumnTypeDiff(table, colDef, wantedType, realType));
            needsRecreate = true;
          }
          else
          {
            #region Проверка длины строкового поля

            if (colDef.ColumnType == DBxColumnType.String)
            {
              // Укорачивать строковые поля никакого смысла не имеет, так как в файле лишние символы
              // не хранятся, как в Access или MS SQL Server.
              // Удлинять надо, так как из-за неправильного описания в программе может срабатывать
              // ограничение, если будет прочитана реальная структура таблицы

              if (realLength < wantedLength)
              {
                errors.AddInfo(DBxUITools.UpdateMsg.ColumnIsLonger(table, colDef, realLength));
                needsRecreate = true;
              }
            } // Строковое поле

            #endregion
          } // RealType==WantedType

          #region Проверка Default

          // Установка свойства DEFAULT должна выполняться до установки NOT NULL, иначе возникнет ошибка

          string wantedDefExpr = String.Empty;
          if (colDef.DefaultExpression != null)
          {
            Buffer.Clear();
            Buffer.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
            wantedDefExpr = Buffer.SB.ToString();
          }
          string realDefExpr = DataTools.GetString(columnRow, "dflt_value");
          if (realDefExpr != wantedDefExpr)
          {
            errors.AddInfo(DBxUITools.UpdateMsg.ColumnDefaultSet(table, colDef, wantedDefExpr));
            needsRecreate = true;
          }

          #endregion

          #region Проверка признака Nullable

          // Проверяем Nullable
          bool realNullable = !DataTools.GetBoolean(columnRow, "notnull");

          if (colDef.Nullable != realNullable)
          {
            errors.AddInfo(DBxUITools.UpdateMsg.NullChanged(table, colDef));
            needsRecreate = true;

            if ((!colDef.Nullable) && colDef.DefaultExpression != null)
            {
              if (columnsToReplaceNullToDef == null)
                columnsToReplaceNullToDef = new List<DBxColumnStruct>();
              columnsToReplaceNullToDef.Add(colDef);
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
      return needsRecreate;
    }

    /// <summary>
    /// Находим в типе поля "CHAR(x)" скобку и извлекаем длину
    /// </summary>
    /// <param name="typeStr"></param>
    /// <param name="length"></param>
    private static void SplitColumnType(ref string typeStr, out int length)
    {
      try
      {
        DoSplitColumnType(ref typeStr, out length);
      }
      catch (Exception e)
      {
        throw new ParsingException(String.Format(Res.SQLiteDBx_Err_SplitColumnType, typeStr, e.Message), e);
      }
    }

    private static void DoSplitColumnType(ref string typeStr, out int length)
    {
      int p1 = typeStr.IndexOf('(');
      if (p1 < 0)
      {
        length = 0;
        return;
      }

      int p2 = typeStr.IndexOf(')');
      string s = typeStr.Substring(p1 + 1, p2 - p1 - 1);
      int p3 = s.IndexOf(',');
      if (p3 >= 0)
        s = s.Substring(0, p3); // 06.01.2022
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
      bool withoutRowId;
      switch (table.PrimaryKey.Count)
      {
        case 1:
          pPrimaryKey = table.Columns.IndexOf(table.PrimaryKey[0]);
          withoutRowId = table.PrimaryKeyColumns[0].ColumnType != DBxColumnType.Int32 && (!table.Columns.ContainsBlob());
          break;
        case 0:
          pPrimaryKey = -1;
          withoutRowId = false;
          break;
        default:
          pPrimaryKey = -1;
          //WithoutRowId = true;
          withoutRowId = !table.Columns.ContainsBlob();
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

      if (withoutRowId)
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
          BaseDBxSqlFormatter.FormatRefColumnDeleteAction(Buffer, column.RefType);
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
        if (DBxTools.IsIntegerType(column.ColumnType))
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

    private void DropAllIndices(ISimpleSplash splash, DBxTableStruct table)
    {
      //splash.PhaseText = "Удаление индексов таблицы \"" + tableName + "\"";

      Buffer.Clear();
      Buffer.SB.Append("PRAGMA index_list(");
      Buffer.FormatTableName(table.TableName);
      Buffer.SB.Append(")");
      DataTable tblList = SQLExecuteDataTable(Buffer.SB.ToString());
      foreach (DataRow listRow in tblList.Rows)
      {
        string indexName = DataTools.GetString(listRow, "name");
        DBxUITools.PhaseText.IndexRemoving(indexName);
        DropIndex(/*tableName, */indexName);
        DBxUITools.UpdateMsg.IndexRemovedExcess(table, indexName);
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

    private bool UpdateDBStructIndices(DBxTableStruct table, ISimpleSplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool modified = false;

      // Таблица существующих индексов
      DataTable tblIndices = CreateIndicesTable(table.TableName);
      tblIndices.DefaultView.Sort = "IndexName";
      DataView dvIdxCols = new DataView(tblIndices);
      dvIdxCols.Sort = "Columns";


      #region Добавляем недостающие индексы

      for (int i = 0; i < table.Indexes.Count; i++)
      {
        string wantedCols = table.Indexes[i].Columns.AsString;
        int pIndex = dvIdxCols.Find(wantedCols);
        if (pIndex >= 0)
          // Индекс найден
          dvIdxCols[pIndex].Row["Flag"] = true;
        else
        {
          // Требуется создать индекс
          // Имя нового индекса
          string indexName = GetNewIndexName(table.TableName, tblIndices);

          splash.PhaseText = DBxUITools.PhaseText.IndexCreation(table, table.Indexes[i]);

          Buffer.Clear();
          Buffer.SB.Append("CREATE INDEX \"");
          Buffer.SB.Append(indexName);
          Buffer.SB.Append("\" ON ");
          Buffer.FormatTableName(table.TableName);
          Buffer.SB.Append(" (");
          Buffer.FormatCSColumnNames(table.Indexes[i].Columns);
          Buffer.SB.Append(")");
          SQLExecuteNonQuery(Buffer.SB.ToString());

          modified = true;
          errors.AddInfo(DBxUITools.UpdateMsg.IndexCreated(table, table.Indexes[i]));

          tblIndices.Rows.Add(indexName, wantedCols, true); // обязательно добавляем, иначе следующее имя индекса будет неправильным
        }
      } // цикл по объявленным индексам

      #endregion

      #region Удаляем лишние индексы

      if (options.DropUnusedIndices)
      {
        foreach (DataRow idxRow in tblIndices.Rows)
        {
          if (!DataTools.GetBoolean(idxRow, "Flag"))
          {
            string indexName = DataTools.GetString(idxRow, "IndexName");
            splash.PhaseText = DBxUITools.PhaseText.IndexRemoving(indexName);
            DropIndex(/*table.TableName, */indexName);
            errors.AddInfo(DBxUITools.UpdateMsg.IndexRemovedExcess(table, indexName));
            modified = true;
          }
        }
      }

      #endregion

      return modified;
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
      foreach (DataRow listRow in tblSchema.Rows)
      {
        string indexName = DataTools.GetString(listRow, "name");
        string expr = DataTools.GetString(listRow, "sql");

        if (expr.Length == 0)
          continue; // 18.01.2020. Автоиндексы не имеют выражения

        // выражение - это то, что в круглых скобках
        int p1 = expr.IndexOf('(');
        if (p1 < 0)
          throw new BugException("Char \"(\" not found");
        int p2 = expr.LastIndexOf(')');
        if (p2 < p1)
          throw new BugException("Char \")\" not found");
        expr = expr.Substring(p1 + 1, p2 - p1 - 1);
        // внутри могут оставаться пробелы, поля - в кавычках
        string[] a = expr.Split(',');
        for (int i = 0; i < a.Length; i++)
          a[i] = GetIndexColumnName(a[i]);

        tblIndices.Rows.Add(indexName, String.Join(",", a));
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
        string indexName = tableName + "_" + i.ToString();
        if (tblIndices.DefaultView.Find(indexName) < 0)
          return indexName;
      }
      throw new BugException("Cannot assign an unique name for a new index for table \"" + tableName + "\". There are too many indexes exist");
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

      if (csb.DataSource.EndsWith(SQLiteDBx.MemoryFileName, StringComparison.Ordinal))
        return csb.ConnectionString; // 21.07.2021 У баз данных в памяти одно и тоже имя

      //csb.DataSource = ReplaceDBItem(csb.DataSource, OldDBName, NewDBName);
      // 31.07.2018. Вместо простой замены текста, заменяем имя файла
      //AbsPath path = new AbsPath(csb.DataSource);
      //if (String.Equals(path.FileNameWithoutExtension, oldDBName, StringComparison.OrdinalIgnoreCase))
      //{
      //  string fileName = newDBName + path.Extension;
      //  path = new AbsPath(path.ParentDir, fileName);
      //  csb.DataSource = path.Path;
      //}
      // 16.06.2023
      csb.DataSource = ReplaceFileItem(csb.DataSource, oldDBName, newDBName);

      return csb.ConnectionString;
    }

    /// <summary>
    /// Возвращает ссылку на <see cref="SQLiteFactory"/>
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get
      {
        return SQLiteFactory.Instance;
      }
    }

    public override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
      return new SQLiteConnectionStringBuilder(connectionString);
    }

    #endregion
  }
}
