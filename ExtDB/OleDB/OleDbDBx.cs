// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

// 06.07.2022
// В Mono почему-то нет класса OleDbConnectionStringBuilder.
// Это не имеет практического значения, так как OleDbConnection - это просто заглушка в Mono.
// 08.05.2024: и в Net6 тоже нет
#if (!MONO) && (!NET)

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Logging;

namespace FreeLibSet.Data.OleDb
{
  /// <summary>
  /// База данных JET OLEDB Provider
  /// </summary>
  public class OleDbDBx : DBx
  {
    #region Конструктор

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionStringBuilder">Собранная строка подключения</param>
    public OleDbDBx(OleDbConnectionStringBuilder connectionStringBuilder)
    {
      SyncRoot = new object();

      if (String.IsNullOrEmpty(connectionStringBuilder.DataSource))
        throw new ArgumentException("Не указан элемент DataSource", "connectionStringBuilder");

      _DataSource = new AbsPath(connectionStringBuilder.DataSource);
      if (_DataSource.IsEmpty)
        throw new ArgumentException("Задан неправильный элемент DataSource \"" + connectionStringBuilder.DataSource + "\" строки подключения. Элемент должен указывать на путь к файлу базы данных", "connectionStringBuilder");
      if (_DataSource.Extension.ToUpperInvariant() != ".MDB")
        throw new ArgumentException("Путь к базе данных \"" + _DataSource.Path + "\" должен иметь расширение \".mdb\"", "connectionStringBuilder");

      base.DisplayName = _DataSource.FileName;

      SetFormatter(new OleDbDBxSqlFormatter());

      new OleDbDBxEntry(this, connectionStringBuilder);
    }

    /// <summary>
    /// Создание подключения к базе данных.
    /// Автоматически создается основная точка подключения.
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    public OleDbDBx(string connectionString)
      : this(new OleDbConnectionStringBuilder(connectionString))
    {
    }


    /// <summary>
    /// Основная версия конструктора с указанием путь к файлу .mdb
    /// </summary>
    /// <param name="DataSource"></param>
    public OleDbDBx(AbsPath DataSource)
      : this(GetConnectionStringBuilder(DataSource))
    {
    }

    private static OleDbConnectionStringBuilder GetConnectionStringBuilder(AbsPath dataSource)
    {
      OleDbConnectionStringBuilder csb = new OleDbConnectionStringBuilder();
      csb.Provider = "Microsoft.Jet.OLEDB.4.0";
      csb.DataSource = dataSource.Path;
      return csb;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к mdb-файлу
    /// </summary>
    public AbsPath DataSource { get { return _DataSource; } }
    private AbsPath _DataSource;

    /// <summary>
    /// Объект для синхронизации
    /// </summary>
    protected readonly object SyncRoot;

    /// <summary>
    /// Идентификатор базы данных (для работы буфера обмена).
    /// </summary>
    public override string DBIdentity
    {
      get { return Environment.MachineName + "|" + DataSource.Path; }
    }

    /// <summary>
    /// Главная точка входа в базу данных.
    /// Не содержит явно назначенных прав пользователя
    /// </summary>
    public new OleDbDBxEntry MainEntry { get { return (OleDbDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// Создает объект OleDbDBxEntry
    /// </summary>
    /// <param name="permissions">Требуемые разрешения на доступ к базе данных</param>
    /// <returns>Точка входа</returns>
    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new OleDbDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }

    /// <summary>
    /// Возвращает DBxLockMode.SingleWrite
    /// </summary>
    public override DBxLockMode LockMode
    {
      get { return DBxLockMode.SingleWrite; }
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
            using (OleDbDBxCon Con = MainEntry.CreateCon() as OleDbDBxCon)
            {
              _ServerVersionText = Con.Connection.ServerVersion; // ??
            }
          }
          return _ServerVersionText;
        }
      }
    }
    private string _ServerVersionText;

    /// <summary>
    /// Возвращает размер файла в байтах
    /// </summary>
    /// <returns>Размер</returns>
    public override long GetDBSize()
    {
      if (DatabaseExists)
      {
        return new System.IO.FileInfo(MainEntry.ConnectionStringBuilder.FileName).Length;
      }
      else
        return 0L;
    }

    /// <summary>
    /// Возвращает 2Гб
    /// </summary>
    public override long DBSizeLimit
    {
      get
      {
        return FileTools.GByte * 2;
      }
    }

    /// <summary>
    /// Возвращает ссылку на OleDbFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return OleDbFactory.Instance; }
    }

    #endregion

    #region Структура базы данных

    /// <summary>
    /// Возвращает true, если файл <see cref="DataSource"/> существует на диске
    /// </summary>
    public override bool DatabaseExists
    {
      get
      {
        return System.IO.File.Exists(DataSource.Path);
      }
    }

    /// <summary>
    /// Создает пустую базу данных, если файла <see cref="DataSource"/> не существует.
    /// Для выполнение распаковки файла необходимо наличие библиотеки ICSharpCode.SharpZipLib.dll
    /// (свойство <see cref="ZipFileTools.ZipLibAvailable"/>=true).
    /// </summary>
    public override void CreateIfRequired()
    {
      if (!DatabaseExists)
      {
        ZipFileTools.ExtractZipResourceFile(OleDbTemplateFilesResource.Access2000, DataSource);
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
      using (OleDbDBxCon con = new OleDbDBxCon(MainEntry))
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

      OleDbConnection.ReleaseObjectPool(); // ???

      System.IO.File.Delete(DataSource.Path);

      return true;
    }

    #endregion
  }

  /// <summary>
  /// Точка входа для MS Jet OLEDB Provider
  /// </summary>
  public class OleDbDBxEntry : DBxEntry
  {
    #region Конструкторы

    /// <summary>
    /// Защищенный конструктор для основной точки входа
    /// </summary>
    /// <param name="db"></param>
    /// <param name="connectionStringBuilder"></param>
    internal OleDbDBxEntry(OleDbDBx db, OleDbConnectionStringBuilder connectionStringBuilder)
      : base(db, DBxPermissions. FullAccess, true)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionString">Строка подключения</param>
    /// <param name="permissions">Разрешнения</param>
    public OleDbDBxEntry(OleDbDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new OleDbConnectionStringBuilder(connectionString ), permissions)
    {
    }

    /// <summary>
    /// Версия для подключения к существующей базе данных (повторный вход с другой строкой подключения)
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="connectionStringBuilder"></param>
    /// <param name="permissions">Разрешнения</param>
    public OleDbDBxEntry(OleDbDBx db, OleDbConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
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
    public new OleDbDBx DB { get { return (OleDbDBx)(base.DB); } }

    // Свойство ConnectionString не стоит делать public. Там может быть пароль

    internal OleDbConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private OleDbConnectionStringBuilder _ConnectionStringBuilder;

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
      return new OleDbDBxCon(this);
    }

    /// <summary>
    /// Создает копию точки входа с другим набором прав
    /// </summary>
    /// <param name="newPermissions">Требуемые разрешения на доступ к объектам базы данных</param>
    /// <returns>Новая точка входа</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new OleDbDBxEntry(DB, ConnectionString, newPermissions);
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
        OleDbConnectionStringBuilder csb = new OleDbConnectionStringBuilder(cs);
        foreach (string key in csb.Keys)
        {
          if (String.Equals(key, "Password", StringComparison.OrdinalIgnoreCase))
          {
            object v;
            if (csb.TryGetValue(key, out v))
            {
              string s = DataTools.GetString(v);
              if (s.Length > 0)
              {
                csb[key] = "***";
              }
            }
          }
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
  /// Соединение с базой данных MS Jet OLEDB Provider
  /// </summary>
  public class OleDbDBxCon : DBxConBase
  {
    #region Конструктор и Dispose()

    internal OleDbDBxCon(OleDbDBxEntry entry)
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
    public new OleDbDBxEntry Entry { get { return (OleDbDBxEntry)(base.Entry); } }

    /// <summary>
    /// База данных
    /// </summary>
    public new OleDbDBx DB { get { return (OleDbDBx)(base.DB); } }

    #endregion

    #region Соединение

    /// <summary>
    /// Возвращает соединение ADO.NET.
    /// Объект создается при первом обращении к свойству
    /// </summary>
    public OleDbConnection Connection
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
            _Connection = new OleDbConnection(Entry.ConnectionString);
            DoOpenConnection();
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

    private OleDbConnection _Connection;

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
        e.Data["SqlDBx.Datasource"] = DB.DataSource;
        e.Data["SqlDBxCon.ConnectionString"] = OleDbDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        throw;
      }
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
      OleDbCommand cmd = new OleDbCommand(cmdText, Connection);
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
      OleDbCommand cmd = new OleDbCommand(cmdText, Connection);
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
      OleDbCommand cmd = new OleDbCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;

      OleDbDataAdapter da = new OleDbDataAdapter(cmd);
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
      OleDbCommand cmd = new OleDbCommand(cmdText, Connection);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteReader(CommandBehavior.SingleResult);
    }

    private static void InitCmdParameters(OleDbCommand cmd, object[] paramValues)
    {
      // TODO:
      //throw new NotImplementedException(); // не помню, как делать

#if XXX
        cmd.Parameters.Clear();
      if (ParamValues != null)
      {
        for (int i = 0; i < ParamValues.Length; i++)
        {
          /*
          SqlParameter Param = new SqlParameter();
          Param.ParameterName = "@P" + (i + 1).ToString();
          Param.Value = ParamValues[i];

          if (ParamValues[i] != null)
          {
            if (ParamValues[i] is Array)
            {
              Param.SqlDbType = SqlDbType.VarBinary;
              Param.DbType = DbType.Binary;
            }
          }
          cmd.Parameters.Add(Param);
           * */
        }
      }
#endif
    }

    #endregion

    #region Переопределенные реализации выполнения запросов

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
      throw new NotImplementedException();
      /*
      CheckTableName(TableName, DBxAccessMode.Full);
      DBxColumnType[] ColumnTypes = CheckTableColumnNames(TableName, ColumnNames, false, DBxAccessMode.Full);

      if (ColumnNames.Count != Values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(TableName, ColumnNames, Values);

      Int32 Id;

      Buffer.Clear();
      Buffer.SB.Append("INSERT INTO [");
      Buffer.SB.Append(TableName);
      Buffer.SB.Append("] (");
      Buffer.FormatCSColumnNames(ColumnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(Values, ColumnTypes);
      Buffer.SB.Append(')');
      Buffer.SB.Append("; SELECT @@IDENTITY");


      Id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (Id <= 0)
        throw new BugException("Получен неправильный идентификатор для добавленной записи в таблице \"" + TableName + "\" Id=" + Id.ToString());

      return Id;
       * */
    }

    #endregion

    #endregion

    #region Транзакция


    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public new OleDbTransaction CurrentTransaction
    {
      get { return (OleDbTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region Извлечение схемы данных

    /// <summary>
    /// Получить полный список таблиц
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable table = Connection.GetSchema("Tables");
      List<string> lst = new List<string>();
      for (int i = 0; i < table.Rows.Count; i++)
      {
        string tt = DataTools.GetString(table.Rows[i], "table_type");
        if (tt == "TABLE")
          lst.Add(DataTools.GetString(table.Rows[i], "table_name"));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить реальное описание структуры таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct tableStr = new DBxTableStruct(tableName);

      #region Описание таблицы

      DataTable tableTable = Connection.GetSchema("Tables", new string[] { null, null, tableName });
      if (tableTable.Rows.Count == 1)
      {
        tableStr.Comment = DataTools.GetString(tableTable.Rows[0], "description"); // 09.06.2023
      }

      #endregion

      #region Список столбцов, тип, MaxLen, Nullable, DefaultValue

      DataTable table = Connection.GetSchema("Columns", new string[] { null, null, tableName });
      table.DefaultView.Sort = "ordinal_position"; // обязательно по порядку, иначе ключевое поле будет не первым

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct colDef = new DBxColumnStruct(columnName);

        OleDbType colType = (OleDbType)(DataTools.GetInt(drv.Row, "data_type"));
        switch (colType)
        {
          // Summary:
          //     A 16-bit signed integer (DBTYPE_I2). This maps to System.Int16.
          case OleDbType.SmallInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int16.MinValue;
            colDef.MaxValue = Int16.MaxValue;
            break;

          //
          // Summary:
          //     A 32-bit signed integer (DBTYPE_I4). This maps to System.Int32.
          case OleDbType.Integer:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int32.MinValue;
            colDef.MaxValue = Int32.MaxValue;
            break;
          //
          // Summary:
          //     A floating-point number within the range of -3.40E +38 through 3.40E +38
          //     (DBTYPE_R4). This maps to System.Single.
          case OleDbType.Single:
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Single.MinValue;
            colDef.MaxValue = Single.MaxValue;
            break;
          //
          // Summary:
          //     A floating-point number within the range of -1.79E +308 through 1.79E +308
          //     (DBTYPE_R8). This maps to System.Double.
          case OleDbType.Double:
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Double.MinValue;
            colDef.MaxValue = Double.MaxValue;
            break;
          //
          // Summary:
          //     A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63
          //     -1 (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of
          //     a currency unit (DBTYPE_CY). This maps to System.Decimal.
          case OleDbType.Currency:
            colDef.ColumnType = DBxColumnType.Decimal;
            //ColStr.MinValue = Int16.MinValue;
            //ColStr.MaxValue = Int16.MaxValue;
            break;
          //
          // Summary:
          //     Date data, stored as a double (DBTYPE_DATE). The whole portion is the number
          //     of days since December 30, 1899, and the fractional portion is a fraction
          //     of a day. This maps to System.DateTime.
          case OleDbType.Date:
            colDef.ColumnType = DBxColumnType.Date;
            break;
          //
          // Summary:
          //     A null-terminated character string of Unicode characters (DBTYPE_BSTR). This
          //     maps to System.String.
          case OleDbType.BSTR:
            colDef.ColumnType = DBxColumnType.String;
            break;
          //
          // Summary:
          //     A pointer to an IDispatch interface (DBTYPE_IDISPATCH). This maps to System.Object.
          //case OleDbType.IDispatch:
          //
          // Summary:
          //     A 32-bit error code (DBTYPE_ERROR). This maps to System.Exception.
          //case OleDbType.Error:
          //
          // Summary:
          //     A Boolean value (DBTYPE_BOOL). This maps to System.Boolean.
          case OleDbType.Boolean:
            colDef.ColumnType = DBxColumnType.Boolean;
            break;
          //
          // Summary:
          //     A special data type that can contain numeric, string, binary, or date data,
          //     and also the special values Empty and Null (DBTYPE_VARIANT). This type is
          //     assumed if no other is specified. This maps to System.Object.
          //case OleDbType.Variant:
          //
          // Summary:
          //     A pointer to an IUnknown interface (DBTYPE_UNKNOWN). This maps to System.Object.
          //case OleDbType.IUnknown:
          //
          // Summary:
          //     A fixed precision and scale numeric value between -10 38 -1 and 10 38 -1
          //     (DBTYPE_DECIMAL). This maps to System.Decimal.
          case OleDbType.Decimal:
            colDef.ColumnType = DBxColumnType.Decimal;
            break;
          //
          // Summary:
          //     A 8-bit signed integer (DBTYPE_I1). This maps to System.SByte.
          case OleDbType.TinyInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = SByte.MinValue;
            colDef.MaxValue = SByte.MaxValue;
            break;
          //
          // Summary:
          //     A 8-bit unsigned integer (DBTYPE_UI1). This maps to System.Byte.
          case OleDbType.UnsignedTinyInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Byte.MinValue;
            colDef.MaxValue = Byte.MaxValue;
            break;
          //
          // Summary:
          //     A 16-bit unsigned integer (DBTYPE_UI2). This maps to System.UInt16.
          case OleDbType.UnsignedSmallInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = UInt16.MinValue;
            colDef.MaxValue = UInt16.MaxValue;
            break;
          //
          // Summary:
          //     A 32-bit unsigned integer (DBTYPE_UI4). This maps to System.UInt32.
          case OleDbType.UnsignedInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = UInt32.MinValue;
            colDef.MaxValue = UInt32.MaxValue;
            break;
          //
          // Summary:
          //     A 64-bit signed integer (DBTYPE_I8). This maps to System.Int64.
          case OleDbType.BigInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int64.MinValue;
            colDef.MaxValue = Int64.MaxValue;
            break;
          //
          // Summary:
          //     A 64-bit unsigned integer (DBTYPE_UI8). This maps to System.UInt64.
          case OleDbType.UnsignedBigInt:
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = UInt64.MinValue;
            colDef.MaxValue = UInt64.MaxValue;
            break;
          //
          // Summary:
          //     A 64-bit unsigned integer representing the number of 100-nanosecond intervals
          //     since January 1, 1601 (DBTYPE_FILETIME). This maps to System.DateTime.
          //case OleDbType.Filetime:
          //
          // Summary:
          //     A globally unique identifier (or GUID) (DBTYPE_GUID). This maps to System.Guid.
          case OleDbType.Guid:
            colDef.ColumnType = DBxColumnType.Guid;
            break;
          //
          // Summary:
          //     A stream of binary data (DBTYPE_BYTES). This maps to an System.Array of type
          //     System.Byte.
          case OleDbType.Binary:
            colDef.ColumnType = DBxColumnType.Binary;
            break;

          //
          // Summary:
          //     A character string (DBTYPE_STR). This maps to System.String.
          case OleDbType.Char:
          //
          // Summary:
          //     A null-terminated stream of Unicode characters (DBTYPE_WSTR). This maps to
          //     System.String.
          case OleDbType.WChar:
            //
            // Summary:
            //     An exact numeric value with a fixed precision and scale (DBTYPE_NUMERIC).
            //     This maps to System.Decimal.
            colDef.ColumnType = DBxColumnType.String;
            break;

          //
          // Summary:
          //     An exact numeric value with a fixed precision and scale (DBTYPE_NUMERIC).
          //     This maps to System.Decimal.
          case OleDbType.Numeric:
            colDef.ColumnType = DBxColumnType.Float; // !! диапазон
            break;

          //
          // Summary:
          //     Date data in the format yyyymmdd (DBTYPE_DBDATE). This maps to System.DateTime.
          case OleDbType.DBDate:
            colDef.ColumnType = DBxColumnType.Date;
            break;
          //
          // Summary:
          //     Time data in the format hhmmss (DBTYPE_DBTIME). This maps to System.TimeSpan.
          case OleDbType.DBTime:
            colDef.ColumnType = DBxColumnType.Time;
            break;
          //
          // Summary:
          //     Data and time data in the format yyyymmddHHmmss (DBTYPE_DBTIMESTAMP). This
          //     maps to System.DateTime.
          case OleDbType.DBTimeStamp:
            colDef.ColumnType = DBxColumnType.DateTime;
            break;
          //
          // Summary:
          //     An automation PROPVARIANT (DBTYPE_PROP_VARIANT). This maps to System.Object.
          //case OleDbType.PropVariant:
          //
          // Summary:
          //     A variable-length numeric value (System.Data.OleDb.OleDbParameter only).
          //     This maps to System.Decimal.
          //case OleDbType.VarNumeric:
          //
          // Summary:
          //     A variable-length stream of non-Unicode characters (System.Data.OleDb.OleDbParameter
          //     only). This maps to System.String.
          case OleDbType.VarChar:
          //
          // Summary:
          //     A long string value (System.Data.OleDb.OleDbParameter only). This maps to
          //     System.String.
          case OleDbType.LongVarChar:
          //
          // Summary:
          //     A variable-length, null-terminated stream of Unicode characters (System.Data.OleDb.OleDbParameter
          //     only). This maps to System.String.
          case OleDbType.VarWChar:
          //
          // Summary:
          //     A long null-terminated Unicode string value (System.Data.OleDb.OleDbParameter
          //     only). This maps to System.String.
          case OleDbType.LongVarWChar:
            colDef.ColumnType = DBxColumnType.String;
            break;
          //
          // Summary:
          //     A variable-length stream of binary data (System.Data.OleDb.OleDbParameter
          //     only). This maps to an System.Array of type System.Byte.
          case OleDbType.VarBinary:
          //
          // Summary:
          //     A long binary value (System.Data.OleDb.OleDbParameter only). This maps to
          //     an System.Array of type System.Byte.
          case OleDbType.LongVarBinary:
            colDef.ColumnType = DBxColumnType.Binary;
            break;


        }

        colDef.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");
        colDef.Nullable = DataTools.GetBool(drv.Row, "is_nullable");

        if ((!colDef.Nullable) && DataTools.GetBool(drv.Row, "column_hasdefault"))
        {
          try
          {
            // 09.06.2023
            string sDefault = DataTools.GetString(drv.Row, "column_default");
            if (colDef.ColumnType == DBxColumnType.Boolean)
            {
              switch (sDefault.ToUpperInvariant())
              {
                case "YES": colDef.DefaultValue = true; break;
                case "NO": colDef.DefaultValue = false; break;
              }
            }
            else
              colDef.DefaultValue = DBxTools.Convert(sDefault, colDef.ColumnType);
          }
          catch (Exception e)
          {
            e.Data["DB"] = DB.ToString();
            e.Data["Table"] = tableName;
            e.Data["Column"] = colDef.ColumnName;
            LogoutTools.LogoutException(e, "Ошибка получения значения DBxColumnStruct.DefaultValue");
          }
        }

        colDef.Comment = DataTools.GetString(drv.Row, "description"); // 09.06.2023

        tableStr.Columns.Add(colDef);
      }

      #endregion

      #region Определение ссылочных полей
#if XXX
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
#endif
      #endregion

      tableStr.SetReadOnly();
      return tableStr;
    }

    //private int GetTableObjId(string TableName)
    //{
    //  Buffer.Clear();
    //  Buffer.SB.Append("SELECT [object_id] FROM sys.tables WHERE [Name]=");
    //  Buffer.FormatValue(TableName, DBxColumnType.String);
    //  return DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));
    //}

    #endregion

    #region Выполнение служебных команд

    internal void CreateDatabase(AbsPath filePath)
    {
      throw new NotImplementedException();
      //Buffer.Clear();
      //Buffer.SB.Append("CREATE DATABASE [");
      //Buffer.SB.Append(FilePath.Path);
      //Buffer.SB.Append("] ON PRIMARY (NAME='File1', FILENAME=");
      //Buffer.FormatValue(FilePath.Path, DBxColumnType.String);
      //Buffer.SB.Append(")");
      //SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      throw new NotImplementedException();
#if XXX
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

      DataTable tableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(tableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME,ORDINAL_POSITION";
      //DebugTools.DebugDataView(dvIndexColumns, "Столбцы индексов базы данных "+DB.DisplayName);

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // Цикл по таблицам
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (table.Columns.Count == 0)
          throw new DBxStructException(table, "Не задано ни одного столбца");

        //CheckPrimaryKeyColumn(Table, Table.Columns[0]);

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

          foreach (DBxColumnStruct column in table.Columns)
          {
            int columnRowIndex = dvColumns.Find(new object[] { table.TableName, column.ColumnName });
            if (columnRowIndex < 0)
            {
              // Поля не существует
              splash.PhaseText = "Добавление поля \"" + column.ColumnName + "\"в таблицу \"" + table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE [");
              Buffer.SB.Append(table.TableName);
              Buffer.SB.Append("] ADD "); // а не ADD COLUMN
              AppendColumnDef(table, column);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("Создано поле \"" + column.ColumnName + "\"в таблице \"" + table.TableName + "\"");
              modified = true;
            }
            else
            {
              // Проверяем соответствие поля
              DataRow columnRow = dvColumns[columnRowIndex].Row;
              // Проверяем соответствие типа столбца объявлению
              string realType = DataTools.GetString(columnRow, "DATA_TYPE").ToUpperInvariant();
              string wantedType = GetSqlServerType(table, column);
              int p = wantedType.IndexOf('(');
              if (p >= 0)
                wantedType = wantedType.Substring(0, p);

              if (realType != wantedType)
              {
                errors.AddError("Несоответствие типа поля \"" + column.ColumnName + "\" таблицы \"" +
                    table.TableName + "\". Объявление поля типа " + column.ColumnType.ToString() +
                    " предполагает тип  " + wantedType +
                    " в то время как реальный тип поля " + realType);
              }
              else
              {
      #region Проверка длины строкового поля

                if (column.ColumnType == DBxColumnType.String)
                {
                  int realLen = DataTools.GetInt(columnRow, "CHARACTER_MAXIMUM_LENGTH");
                  if (realLen != column.MaxLength)
                  {
                    if (realLen > column.MaxLength)
                    {
                      // !!! Проверка, нельзя ли укоротить поле
                      errors.AddWarning("Поле \"" + column.ColumnName + "\" таблицы \"" +
                          table.TableName + "\" должно иметь длину " + column.MaxLength.ToString() +
                          " символов, в то время, как реальное поле длиннее:  " + realLen.ToString() + " символов");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // Лучше пересоздать все индексы
                      errors.AddInfo("Все существующие индексы таблицы \"" + table.TableName + "\" будут удалены из-за изменения размера поля \"" + column.ColumnName + "\"");
                      if (DeleteAllIndices(table.TableName, splash, errors))
                        modified = true;

                      // Увеличиваем длину поля
                      splash.PhaseText = "Изменение длины поля \"" + column.ColumnName + "\" в таблице \"" + table.TableName + "\"";
                      AlterColumn(table, column);
                      errors.AddInfo("Длина поля \"" + column.ColumnName + "\"в таблице \"" + table.TableName +
                        "\" увеличена с " + realLen.ToString() + " до " + column.MaxLength.ToString() + " символов");
                      modified = true;
                    }
                  }
                } // Строковое поле

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
                    realNullable = column.Nullable;
                    break;
                }

                if (column.Nullable != realNullable)
                {
                  if (DeleteAllIndices(table.TableName, splash, errors))
                    modified = true;

                  // Делаем поле NULLABLE
                  AlterColumn(table, column);
                  errors.AddInfo("Для поля \"" + column.ColumnName + "\"в таблице \"" + table.TableName +
                    "\" установлен признак " + (column.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  modified = true;
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
        List<string> goodIndexes = new List<string>();
        RemoveExtraIndices(table, goodIndexes, errors, splash);
        CreateIndices(table, goodIndexes, errors, splash);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // Цикл по таблицам

      // Внешние ключи можно создавать только после создания всех таблиц

      splash.PhaseText = "Проверка внешних ключей";
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (UpdateForeignKeys(table, dvForeignKeys, splash, errors))
          modified = true;
      }
      splash.PhaseText = String.Empty;


      splash.PercentMax = 0;
      return modified;
#endif
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

#if XXX
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
        AppendColumnDef(table, table.Columns[i]);

        if (table.Columns[i].ColumnName == table.PrimaryKey[0])
        {
          Buffer.SB.Append(" IDENTITY CONSTRAINT [PK_");
          Buffer.SB.Append(table.TableName);
          Buffer.SB.Append("] PRIMARY KEY");
        }
      }
      Buffer.SB.Append(")");
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private string GetSqlServerType(DBxTableStruct table, DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        case DBxColumnType.String:
          if (column.MaxLength < 1)
            throw new DBxStructException(table, "Для строкового поля \"" + column.ColumnName + "\" должна быть задана длина поля");

          return "NCHAR(" + column.MaxLength.ToString() + ")";

        case DBxColumnType.Boolean:
          return "BIT";

        case DBxColumnType.Int:
          if (column.MinValue == 0 && column.MaxValue == 0)
            return "INT";
          else if (column.MinValue >= 0 && column.MaxValue <= 255)
            return "TINYINT";
          else if (column.MinValue >= Int16.MinValue && column.MaxValue <= Int16.MaxValue)
            return "SMALLINT";
          else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
            return "INT";
          else
            return "BIGINT";

        case DBxColumnType.Float:
          if (column.MinValue == 0 && column.MaxValue == 0)
            return "FLOAT";
          else if (column.MinValue >= Single.MinValue && column.MaxValue <= Single.MaxValue)
            return "REAL";
          else
            return "FLOAT";

        case DBxColumnType.Decimal:
          return "MONEY";

        case DBxColumnType.Date:
          // Только дата
          if (DB.IsSqlServer2008orNewer)
            return "DATE";
          else
            return "DATETIME";

        case DBxColumnType.DateTime:
          // Дата и время
          if (DB.IsSqlServer2008orNewer)
            return "DATETIME2(0)";
          else
            return "DATETIME";

        case DBxColumnType.Time:
          // Только время
          if (DB.IsSqlServer2008orNewer)
            return "TIME(0)";
          else
            return "DATETIME";


        case DBxColumnType.Memo:
          return "NVARCHAR(MAX)";

        case DBxColumnType.Xml:
          return "XML";

        case DBxColumnType.Binary:
          return "VARBINARY(MAX)";

        default:
          throw new BugException("Неизвестный тип поля " + column.ColumnType.ToString());
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
        SQLExecuteNonQuery("ALTER TABLE [" + Table.TableName + "] ADD CONSTRAINT [" + PKName + "] PRIMARY KEY ([" + Table.PrimaryKey[0] + "])");
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

#endif

#if XXX
    #region Индексы

    private void RemoveExtraIndices(DBxTableStruct Table, List<string> GoodIndices, ErrorMessageList Errors, ISplash Splash)
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
          Splash.PhaseText = "Удаление индекса " + IndexName;
          DropIndex(Table.TableName, IndexName);
          Errors.AddInfo("Удален лишний индекс \"" + IndexName + "\" в таблице \"" + Table.TableName + "\"");
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
    /// <param name="TableDef">Описание таблицы с индексами</param>
    /// <param name="IndexIndex">Номер индекса в TableDef.Indices</param>
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
#endif
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
}
#endif // !MONO
