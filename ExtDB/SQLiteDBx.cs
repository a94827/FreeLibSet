// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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

// ��������� ��� ������� � ���� ������ SQLite

namespace FreeLibSet.Data.SQLite
{
  /// <summary>
  /// ���� ������ PostGreSQL ����� ��������� Npgsql.
  /// ��� ������������� ������ ���� ���������� ������ Npgsql.dll � Mono.Security �� ����������� FreeLibSet/Others
  /// </summary>
  public class SQLiteDBx : DBx
  {
    #region ������������ � Dispose

    internal const string MemoryFileName = ":memory:";

    /// <summary>
    /// ��������������� � 1 ��� ������ ������ ������������
    /// </summary>
    private static int _FirstFlagValue = 0;

    /// <summary>
    /// �������� ����������� � ���� ������.
    /// ������������� ��������� �������� ����� �����������
    /// </summary>
    /// <param name="connectionStringBuilder">��������� ������ �����������</param>
    public SQLiteDBx(SQLiteConnectionStringBuilder connectionStringBuilder)
    {
      bool firstCall = false;
      if (System.Threading.Interlocked.Exchange(ref _FirstFlagValue, 1) == 0) // ����� ���� ����������� ������ ������������
      {
        LoadDLLs();
        LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
        firstCall = true;
      }

      _SyncRoot = new object();

      _InMemory = connectionStringBuilder.DataSource.EndsWith(MemoryFileName);
      if (!InMemory)
      {
        _FileName = new AbsPath(connectionStringBuilder.DataSource);
        if (_FileName.IsEmpty)
          throw new ArgumentException("� ������ ����������� �� ����� �������� DataSource", "connectionStringBuilder");
      }

      _DateFormat = @"yyyy\-MM\-dd";
      _TimeFormat = @"HH\:mm\:ss";
      SetFormatter(new
        SQLiteDBxSqlFormatter(this));

      connectionStringBuilder.FailIfMissing = false; // ������ ������������� ������� ���� ������

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
    /// �������� ����������� � ���� ������.
    /// ������������� ��������� �������� ����� �����������
    /// </summary>
    /// <param name="connectionString">������ �����������</param>
    public SQLiteDBx(string connectionString)
      : this(new SQLiteConnectionStringBuilder(connectionString))
    {
    }

    /// <summary>
    /// ��������� ���� ������ �� ���������� ����.
    /// ������������� � ������ ����������� ������� "foreign keys=true".
    /// </summary>
    /// <param name="path">���� � ���� ������. ������ ���� �����</param>
    /// <param name="readOnly">True - ������� ���� ������ ��� ���������, false - ��� ������</param>
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
      b.ForeignKeys = true; // 02.02.2022
      return b;
    }

    /// <summary>
    /// ��� ������ ������������ ������������� ��� �������� ���� ������ � ������
    /// </summary>
    public SQLiteDBx()
      : this("Data Source=" + MemoryFileName +
      ";foreign keys=true") // 02.02.2022
    {
    }

    /// <summary>
    /// ���� InMemory=true, �� ��������� ���������� � ����� ������, ��� �������� � �� ��������.
    /// </summary>
    /// <param name="disposing">true, ���� ������ ����� Dispose()</param>
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

    #region ��������

    /// <summary>
    /// ���������� ������ ���� � ����� ���� ������
    /// </summary>
    public override string DatabaseName { get { return FileName.Path; } }

    /// <summary>
    /// ��� �����
    /// </summary>
    public AbsPath FileName { get { return _FileName; } }
    private AbsPath _FileName;

    /// <summary>
    /// ������� ����� ����� � ���� ������.
    /// �� �������� ���� ����������� ���� ������������
    /// </summary>
    public new SQLiteDBxEntry MainEntry { get { return (SQLiteDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// ������� SQLiteDBxEntry
    /// </summary>
    /// <param name="permissions">���������� �� ������ � ���� ������</param>
    /// <returns>����� �����</returns>
    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new SQLiteDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }

    private object _SyncRoot;

    /// <summary>
    /// ��������� ������������� ������ �������
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
    /// �������� ���������� ������� pg_database_size()
    /// </summary>
    /// <returns>������ ���� ������ � ������</returns>
    public override long GetDBSize()
    {
      throw new NotImplementedException();
      //using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry, false))
      //{
      //  return DataTools.GetInt64(Con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
      //}
    }

    /// <summary>
    /// ���������� ����������� ��������� ��������� ��� ������ PostgreSQL
    /// </summary>
    public override DBxManager Manager { get { return SQLiteDBxManager.TheManager; } }

    /// <summary>
    /// ���������� true, ���� ���� ������ ������������� � ������
    /// </summary>
    public bool InMemory { get { return _InMemory; } }
    private bool _InMemory;

    /// <summary>
    /// ���� ���� ������ ������������� � ������, ��������� ��������� �������� ���������� ��� ���.
    /// ����� ��� ���������� �����������, ���� ������ ������������.
    /// </summary>
    internal SQLiteConnection MainConnection;

    #endregion

    #region �������������� ��������

    /// <summary>
    /// ������ ����
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
    /// ������ �������
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

    #region ���������� ���������

    /// <summary>
    /// ���������� ������� ������������� ����� FileName
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
    /// ������� ���� ���� ������, ���� �� �� ����������
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (SQLiteDBxCon con = new SQLiteDBxCon(MainEntry))
      {
        // ������ �� ������. ���� ���������
        // 21.07.2021. �������. ���� ����������� ���������� � �������� Connection

        con.Connection.GetSchema();
      }
    }

    /// <summary>                               
    /// ��������� ��������� ������������ ����
    /// ������ �� ��������� ���������� �������� � �������� DBx.Struct.
    /// �� ������ ������ ���� ������ (��������, ������) ������ ������������.
    /// </summary>
    /// <param name="splash">����� ��������������� �������� PhaseText ��� ����������� ������������ ��������</param>
    /// <param name="errors">���� ���������� �������������� � �������������� ���������. ���� ������� ���������
    /// �� �������� �� ��������, ��������� �� �����������</param>
    /// <param name="options">����� ����������</param>
    /// <returns>true, ���� � ���� ������ ���� ������� ���������</returns>
    protected override bool OnUpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // ���������� ��� �������� ����������, �.�. ����� ������ � ���������� �������
      using (SQLiteDBxCon con = new SQLiteDBxCon(MainEntry))
      {
        con.CommandTimeout = 0; // ����������� ����� ����������
        return con.UpdateDBStruct(splash, errors, options);
      }
    }

    /// <summary>
    /// ������� ������� ������, ���� ��� ����������.
    /// ���� ����� ������ ���������� �� ��������� �������� DBx.Struct � ������ UpdateStruct().
    /// ���� ���������� ��������� �� ��������������, ����� ������������������ ������� ����� ������,
    /// ������ ���� ��������� ��������� DB.Struct=null, ����� �������� ������ ������
    /// </summary>
    /// <param name="tableName">��� ��������� �������</param>
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

    #region ������

    /// <summary>
    /// �������� ���� ������, ���� ��� ����������
    /// </summary>
    /// <returns>True, ���� ������������ ���� ������ ���� �������.
    /// False, ���� ���� ������ �� ����������������</returns>
    public override bool DropDatabaseIfExists()
    {
      if (!DatabaseExists)
        return false;

      SQLiteConnection.ClearAllPools();

      System.IO.File.Delete(FileName.Path);

      return true;
    }

    /// <summary>
    /// ���������� ������ �� SQLiteFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }

    #endregion

    #region �������� ��������� ������� � SQLite

    /// <summary>
    /// ��������� ����������, ����������� ��� ������ � SQLite.
    /// ��������� ������ ������������
    /// </summary>
    public static void LoadDLLs()
    {
      //if (IntPtr.Size == 8)
      //  System.Reflection.Assembly.Load("SQLite.Interop.x64");
      //else
      //  System.Reflection.Assembly.Load("SQLite.Interop.x86");

      System.Reflection.Assembly.Load("System.Data.SQLite"); // ����������� ����� SQLite.Interop.dll
    }

    #endregion

    #region ������������� �������          

    /// <summary>
    /// ���� true (�� ���������), �� ����� �������� ������� UPPER(s) � LOWER().
    /// ��� ����� �������� ������ String.ToUpperInvariant() � ToLowerInvariant() ��������������.
    /// �������� ����� ������������� ������ �� ������� ������ ������������ SQLiteDBx.
    /// </summary>
    public static bool UseInvariantStringFunctions
    {
      get { return _UseInvariantStringFunctions; }
      set
      {                                                         
        if (_FirstFlagValue != 0)
          throw new InvalidOperationException("��� ��� ����� ������������");
        _UseInvariantStringFunctions = value;
      }
    }                                                       
    private static bool _UseInvariantStringFunctions = true;
                                                                                                     
    /// <summary>
    /// ������������� ������� UPPER() � LOWER().
    /// ���������� � ����� ������ ������������ ��� �������� ������ ���� ������
    /// </summary>
    private void InitInvariantStringFunctions()
    {
      // �� ������������� ������� ������ ��������� ��� LIKE, ����� ��� ����������������������
      /* ���-�� �� ��� ��������, ��� ���������
      using (DBxConBase con = MainEntry.CreateCon())
      {
        con.SQLExecuteNonQuery("PRAGMA case_sensitive_like=1");
      }
       * */

      // ��. https://stackoverflow.com/questions/10349839/turkish-character-in-sqlite-while-using-like-expression/10361892#10361892
      // ������ ��� ���������� ������� TOUPPER() � �� UPPER

      SQLiteFunction.RegisterFunction(typeof(UPPERFunction));
      SQLiteFunction.RegisterFunction(typeof(LOWERFunction));
      //CollationCaseInsensitive.RegisterFunction(typeof(CollationCaseInsensitive));
    }

    [SQLiteFunction(Name = "UPPER", Arguments = 1, FuncType = FunctionType.Scalar)]
    private class UPPERFunction : SQLiteFunction
    {
      public override object Invoke(object[] args)
      {
        return args[0].ToString().ToUpperInvariant();
      }
    }

    [SQLiteFunction(Name = "LOWER", Arguments = 1, FuncType = FunctionType.Scalar)]
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

    #region ������� ������������� ������

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
      catch (Exception e) { args.WriteLine("*** ������ ***. " + e.Message); }

      args.WriteLine("SQLiteConnection.GetMemoryStatistics()");
      args.IndentLevel++;
      try
      {
        IDictionary<string, long> dict = null;
        SQLiteConnection.GetMemoryStatistics(ref dict);
        LogoutTools.LogoutObject(args, dict);
      }
      catch (Exception e) { args.WriteLine("*** ������ ***. " + e.Message); }
      args.IndentLevel--;
    }

    #endregion
  }

  /// <summary>
  /// ����� ����� ��� ���� ������ SQLite
  /// </summary>
  public class SQLiteDBxEntry : DBxEntry
  {
    #region ������������

    /// <summary>
    /// ���������� ����������� ��� �������� ����� �����
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
    /// ������ ��� ����������� � ������������ ���� ������ (��������� ���� � ������ ������� �����������)
    /// </summary>
    /// <param name="db">���� ������</param>
    /// <param name="connectionString">������ �����������</param>
    /// <param name="permissions">����������</param>
    public SQLiteDBxEntry(SQLiteDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new SQLiteConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// ������ ��� ����������� � ������������ ���� ������ (��������� ���� � ������ ������� �����������)
    /// </summary>
    /// <param name="db">���� ������</param>
    /// <param name="connectionStringBuilder">������ �����������</param>
    /// <param name="permissions">����������</param>
    public SQLiteDBxEntry(SQLiteDBx db, SQLiteConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
      : base(db, permissions, false)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ������, � ������� ��������� ����� �����
    /// </summary>
    public new SQLiteDBx DB { get { return (SQLiteDBx)(base.DB); } }

    // �������� ConnectionString �� ����� ������ public. ��� ����� ���� ������

    internal SQLiteConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private SQLiteConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private string _ConnectionString;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� ������ SQLiteDBxCon
    /// </summary>
    /// <returns>����������</returns>
    public override DBxConBase CreateCon()
    {
      return new SQLiteDBxCon(this);
    }

    /// <summary>
    /// ������� ����� ����� ����� � ������ ������� ����
    /// </summary>
    /// <param name="newPermissions">��������� ���������� �� ������ � �������� ���� ������</param>
    /// <returns>����� ����� �����</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new SQLiteDBxEntry(DB, ConnectionString, newPermissions);
    }

    #endregion

    #region ������ ����������� ��� ������

    /// <summary>
    /// ���������� ������ �����������, �� ���������� ������.
    /// </summary>
    public override string UnpasswordedConnectionString
    {
      get
      {
        return GetUnpasswordedConnectionString(ConnectionString);
      }
    }

    /// <summary>
    /// �������� ������ �� ������ ����������, ���� �� ����
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
        return "������ ��������� ������ �����������. " + e.Message;
      }
    }

    #endregion
  }

  /// <summary>
  /// ����������� � ����� ������ SQLite.
  /// ��� �������� ������� ����������� SQLiteDBxEntry
  /// </summary>
  public class SQLiteDBxCon : DBxConBase
  {
    #region ����������� � Dispose

    internal SQLiteDBxCon(SQLiteDBxEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// ��������� ����������� ADO.NET, ���� ��� ���� �������, � ���������� ��� � ���.
    /// ������� ���������� �� ����� �����.
    /// </summary>
    /// <param name="disposing">True, ���� ��� ������ ����� Dispose().
    /// False, ���� ������ ����������</param>
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

        // ��� ������ ��������� ���������� �������
        //if (ClearPoolCalled)
        //  SQLiteConnection.Shutdown(false, true);
      }
      base.Dispose(disposing);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ����� �������, � ������� ��������� ����������
    /// </summary>
    public new SQLiteDBxEntry Entry { get { return (SQLiteDBxEntry)(base.Entry); } }

    /// <summary>
    /// ���������� ���� ������, � ������� ��������� ����������
    /// </summary>
    public new SQLiteDBx DB { get { return (SQLiteDBx)(base.DB); } }

    #endregion

    #region ����������

    /// <summary>
    /// ���������� ���������� ADO.NET.
    /// ������ ��������� ��� ������ ��������� � ��������
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
              // ��� ������������ ������ new SQLiteConnection(Entry.DB.MainConnection)
              // ������� ���������� ��� ����� ��������� ���� ������
              // �.�., ��� ������������ � ������������
              //_Connection = (SQLiteConnection)(Entry.DB.MainConnection.Clone());

              // TODO: �����. ���� ������������ DBDataReader, �� ���������� ����� ������
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
    /// ���������� ���������� ADO.NET.
    /// ������ ��������� ��� ������ ��������� � ��������
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
    /// �������� SQLiteConnection.ClearPool()
    /// </summary>
    public override void ClearPool()
    {
      SQLiteConnection.ClearPool(Connection);
      base.ClearPool();
    }

    #endregion

    #region ���������� SQL-��������

    /// <summary>
    /// ����������� ����� ���������� SLQ-�������, ������������� ������������ ��������
    /// </summary>
    /// <param name="cmdText">����� SQL-�������</param>
    /// <param name="paramValues">��������� �������</param>
    /// <returns>��������, ������������ ��������</returns>
    protected override object DoSQLExecuteScalar(string cmdText, object[] paramValues)
    {
      SQLiteCommand cmd = new SQLiteCommand(cmdText, Connection);
      InitCmdParameters(cmd, paramValues);
      cmd.CommandTimeout = CommandTimeout;
      cmd.Transaction = CurrentTransaction;
      return cmd.ExecuteScalar();
    }


    /// <summary>
    /// ���������� SQL-�������, �� ������������� ��������
    /// </summary>
    /// <param name="cmdText">SQL-��������</param>
    /// <param name="paramValues">�������� ���������� �������</param>
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
    /// ����������� ����� ���������� SLQ-�������, ������������� ������� ������
    /// </summary>
    /// <param name="cmdText">����� SQL-�������</param>
    /// <param name="tableName">��� ������� ��� ������������� DataTable</param>
    /// <param name="paramValues">��������� �������</param>
    /// <returns>����������� �������</returns>
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
    /// ����������� ����� ���������� SLQ-�������, ������������� DbDataReader
    /// </summary>
    /// <param name="cmdText">����� SQL-�������</param>
    /// <param name="paramValues">��������� �������</param>
    /// <returns>������ ��� ������ ������</returns>
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
          sqlParam.ParameterName = ":P" + (i + 1).ToString(); // ���������� 29.05.2020
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

    #region ���������������� ���������� ���������� ��������

    /// <summary>
    /// ���������� ���������������� ���������� ��������
    /// </summary>
    /// <param name="writerInfo"></param>
    /// <returns></returns>
    protected override DBxDataWriter OnCreateWriter(DBxDataWriterInfo writerInfo)
    {
      return new SQLiteDBxDataWriter(this, writerInfo);
    }

    #region ������� �����

#if XXX
    // �� �������� ���������� "AS"

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
    /// ���������������� ������ ������� ��������, ��� ������� �����.
    /// ������� ������ SELECT COUNT(*) �������� ����������� � SQLite
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

    #region ���������� �������

    /// <summary>
    /// �������� ������ � �������������� ����������� ��������������.
    /// ���������� ������������� ������������ � ����� ���� ����������� ��� ������ �� ������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">����� ��������. � ������ �� ������ ���� ���� ���������� �����</param>
    /// <param name="values">��������. ������� �������� ������ ��������������� ������ ��������</param>
    /// <returns>������������� ����������� ������</returns>
    public override Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      /*string PrimaryKeyColumnName = */
      Validator.CheckTablePrimaryKeyInt32(tableName);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("����� ����� �� ��������� � ������ ��������");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Int32 id;

      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, columnTypes);
      Buffer.SB.Append(')');

      Buffer.SB.Append("; SELECT last_insert_rowid()");

      id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (id <= 0)
        throw new BugException("������� ������������ ������������� ��� ����������� ������ � ������� \"" + tableName + "\" Id=" + id.ToString());

      return id;
    }

    #endregion

    #endregion

    #region ����������

    /// <summary>
    /// ������� ����������, ���� ��� ����� ������ TransactionBegin(), ��� null, ���� ��� �������� ����������
    /// </summary>
    public new SQLiteTransaction CurrentTransaction
    {
      get { return (SQLiteTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region ���������� ����� ������

    /// <summary>
    /// �������� ������ ������ ������
    /// </summary>
    /// <returns></returns>
    internal protected override string[] GetAllTableNamesFromSchema()
    {
      DataTable table = Connection.GetSchema("Tables"); // ���������� 28.08.2020
      string[] a = new string[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
        a[i] = DataTools.GetString(table.Rows[i], "table_name");

      return a;
    }

    /// <summary>
    /// �������� �������� �������� ��������� �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>���������</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct tableStr = new DBxTableStruct(tableName);

      #region ������ ��������, ���, MaxLen, Nullable

      DataTable table = Connection.GetSchema("Columns", new string[] { null, null, tableName }); // ���������� 28.08.2020
      table.DefaultView.Sort = "ordinal_position"; // ����������� �� �������, ����� �������� ���� ����� �� ������

      foreach (DataRowView drv in table.DefaultView)
      {
        string columnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct colDef = new DBxColumnStruct(columnName);

        string colTypeString = DataTools.GetString(drv.Row, "data_type");
        // 28.08.2020
        // ������� ����� ����� �� ����� System.Data.SQLite\Resources\DataTypes.xml
        // �������� ������� System.Data.SQLite
        switch (colTypeString)
        {
          case "tinyint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = 0;
            colDef.MaxValue = 255;
            break;

          case "smallint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int16.MinValue;
            colDef.MaxValue = Int16.MaxValue;
            break;

          case "int":
          case "integer": // �� ����, ��� Int64, �� ���� ��� ������������ ��� ���������������
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int32.MinValue;
            colDef.MaxValue = Int32.MaxValue;
            break;

          case "counter": // ?? ����?
          case "autoincrement": // ?? ����?
          case "identity": // ?? ����?
          case "long":
          case "bigint":
            colDef.ColumnType = DBxColumnType.Int;
            colDef.MinValue = Int64.MinValue;
            colDef.MaxValue = Int64.MaxValue;
            break;

          case "single":
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Single.MinValue;
            colDef.MaxValue = Single.MaxValue;
            break;

          case "real":
          case "float":
          case "double":
            colDef.ColumnType = DBxColumnType.Float;
            colDef.MinValue = Double.MinValue;
            colDef.MaxValue = Double.MaxValue;
            break;

          case "money":
          case "currency":
          case "decimal":
          case "numeric":
            colDef.ColumnType = DBxColumnType.Money;
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
          case "string": // ��� - ��� ��������
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
            colDef.ColumnType = DBxColumnType.Memo;
            break;

          case "date":
          case "smalldate": // ��� ��������
            colDef.ColumnType = DBxColumnType.Date;
            break;

          case "time":
            colDef.ColumnType = DBxColumnType.Time;
            break;

          case "datetime":
          case "timestamp": // ?? ��� ��������
            colDef.ColumnType = DBxColumnType.DateTime;
            break;

          case "binary":
          case "varbinary":
          case "blob":
          case "image":
          case "general":
          case "oleobject": // ?? ����?
            colDef.ColumnType = DBxColumnType.Binary;
            break;

          case "uniqueidentifier":
          case "guid":
            colDef.ColumnType = DBxColumnType.Guid;
            break;

          case "xml": // ����� ���� ���, � ��� ��� ��������
            colDef.ColumnType = DBxColumnType.Xml;
            break;

          default:
            throw new BugException("����������� ��� ����: \"" + colTypeString + "\"");
        }

        colDef.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string nullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (nullableStr)
        {
          case "TRUE": colDef.Nullable = true; break;  // ���������� 28.08.2020
          case "FALSE": colDef.Nullable = false; break;
        }

        tableStr.Columns.Add(colDef);
      }

      #endregion

      #region ����������� ��������� �����

      DataTable schema = Connection.GetSchema("FOREIGNKEYS", new string[] { null, null, tableName, null });
      foreach (DataRow row in schema.Rows)
      {
        string refColName = DataTools.GetString(row, "FKEY_FROM_COLUMN");
        DBxColumnStruct colStr = tableStr.Columns[refColName];
        if (colStr == null)
          continue; // �� ����, ��� ������

        colStr.MasterTableName = DataTools.GetString(row, "FKEY_TO_TABLE");

        string mode = DataTools.GetString(row, "FKEY_ON_DELETE");

        // ��.������: https://www.sqlite.org/foreignkeys.html
        // ������ 4.3

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

    #region ��������� ����������

    /// <summary>
    /// ������ SQLite � ���� �������, ����� ��� ����� ���� ����������
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

    #region ���������� ���������

    #region �������� ����� UpdateDBStruct

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool modified = false;

      if (UpdateDBStructTables(splash, errors, options))
        modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return modified;
    }

    #endregion

    #region ���������� ������, �������� � ��������� ������

    /// <summary>
    /// ��� ��������� ������������� �������, ������������ ��� ���������� ����� �����
    /// </summary>
    private const string AlterTableName = "_TempAlterTable";

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      /*
       * SQLite �� ������������ ���������� ALTER TABLE ALTER COLUMN.
       * ���� ��������� �������� ��� ������� ��� ����������� NULL/NOT NULL, �� ��������� �������
       * ����� �������
       */

      bool modified = false;

      #region ���������� ���������� �� ������������ ����� ������

      /*
       * ���������� ���������� ����� ������ ����� ��� ���� ������ ������������ � System.Data.SQLite.
       * �������� ��������� ���������� �� ������ ������� ��������.
       * ����� ����� ������ ������ ��������������, ��� ��� ����� �������� ����� �������������� ����������,
       * ������� �� ����� ��� ���������� ���������, �� ��� ������� �������� �������������� ������.
       */

      DataTable tableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(tableTables);
      dvTables.Sort = "TABLE_NAME";

      //// �������
      //DataTable TableColumns = Connection.GetSchema("Columns", new string[] { String.Empty });
      //DataView dvColumns = new DataView(TableColumns);
      //dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // ����� ����� �������

      //// ������ �� �������� 
      //DataTable TableIndexes = Connection.GetSchema("Indexes", new string[] { String.Empty }); // ������� "main"
      //DataView dvIndexes = new DataView(TableIndices);

      //DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      //DataView dvIndexColumns = new DataView(TableIndexColumns);
      //dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;

      // ���� �� ��������
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        if (table.Columns.Count == 0)
          throw new DBxStructException(table, "�� ������ �� ������ �������");

        //CheckPrimaryKeyColumn(Table, Table.PrimaryKeyColumns[0]);
        //bool IndicesDropped = false;

        List<DBxColumnStruct> columnsToAdd;
        DBxColumns existedColumns;
        List<DBxColumnStruct> columnsToReplaceNullToDef; // ������� ��� ������ NULL �� DEFAULT

        if (dvTables.Find(table.TableName) < 0)
        {
          #region ��������� ������ �������� �������

          splash.PhaseText = "��������� ������� \"" + table.TableName + "\"";
          CreateTable(table, table.TableName, options);
          errors.AddInfo("������� ������� \"" + table.TableName + "\"");
          modified = true;

          #endregion
        }
        else if (NeedsRecreateTable(table, /*dvColumns,*/ errors, out columnsToAdd, out existedColumns, out columnsToReplaceNullToDef))
        {
          #region ��������� ������������ � �������������� ������������� �������

          // ��. ������� �������� � ���������� ����� SQLite
          // lang_altertable.html

          DropAllIndices(splash, table.TableName);
          //IndicesDropped = true;

          splash.PhaseText = "��������� ��������� ������� \"" + table.TableName + "\"";

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
                    // �������� �������� NULL �� �������� �� ���������
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
                    errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
                      "\" �������� NULL �������� �� �������� �� ���������");
                  }
                }

                // ������� ������������� �������
                CreateTable(table, AlterTableName, options);

                // ��������� ������. ����� ����� ���������� ����������, ���� ���� ������ ������������
                InsertIntoFrom(AlterTableName, table.TableName, existedColumns);

                // ������� �������� �������
                DropTable(table.TableName, false);

                // ��������������� �������
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
          errors.AddInfo("������� \"" + table.TableName + "\" ����������� ��-�� ��������� ������� ��������");
          modified = true;

          #endregion
        }
        else if (columnsToAdd != null)
        {
          #region ��������� ���������� ����������� ��������

          foreach (DBxColumnStruct colDef in columnsToAdd)
          {
            splash.PhaseText = "���������� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName + "\"";
            Buffer.Clear();
            Buffer.SB.Append("ALTER TABLE ");
            Buffer.FormatTableName(table.TableName);
            Buffer.SB.Append(" ADD "); // � �� ADD COLUMN
            AppendColumnDef(/*Table, */colDef, false, options, true);
            SQLExecuteNonQuery(Buffer.SB.ToString());
            errors.AddInfo("������� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName + "\"");
            modified = true;
          }

          #endregion
        }

        // ������� ����������

        #region �����������

        SetTableComment(table.TableName, table.Comment);
        for (int i = 0; i < table.Columns.Count; i++)
          SetColumnComment(table.TableName, table.Columns[i].ColumnName, table.Columns[i].Comment);

        #endregion

        UpdateDBStructIndices(table, splash, errors, options);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // ���� �� ��������

      return modified;
    }

    /// <summary>
    /// �������������� �������
    /// </summary>
    /// <param name="oldName">��� ������������ �������</param>
    /// <param name="newName">����� ���</param>
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
    /// ���������� ������� INSERT INTO xxx SELECT yyy
    /// </summary>
    /// <param name="resTableName">����������� �������</param>
    /// <param name="srcTableName">�������� �������</param>
    /// <param name="columns">������ �������� ��� �����������</param>
    private void InsertIntoFrom(string resTableName, string srcTableName, DBxColumns columns)
    {
      if (columns.IsEmpty)
        throw new ArgumentException("�� ����� ������ �������� ��� �����������", "columns");

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
    /// ���������� ������� PRAGMA foreign keys ON/OFF
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
    /// ��������� ������������� ������������ �������
    /// </summary>
    /// <param name="table">��������� �������� ��������� �������</param>
    /// <param name="columnsToAdd">���� ���������� ������� �� Table.Columns, ������� �����
    /// ��������. ������ �� ���������, ���� ��������� ������������ �������</param>
    /// <param name="errors">������ ��� ���������� �������������� ���������</param>
    /// <param name="existedColumns">������ ������������ �������� �������.
    /// ���� ������ ����������� ���������� �� ����, ����� �� ��������������� �������</param>
    /// <param name="columnsToReplaceNullToDef">������ �����, ��� ������� ��������� ������ NULL �� DEFAULT.
    /// ���� ���������� null, ������ �� �����</param>
    /// <returns>true, ���� ��������� ������������ �������</returns>
    private bool NeedsRecreateTable(DBxTableStruct table, /* DataView dvColumns, */
      ErrorMessageList errors,
      out List<DBxColumnStruct> columnsToAdd,
      out DBxColumns existedColumns,
      out List<DBxColumnStruct> columnsToReplaceNullToDef)
    {
      bool needsRecreate = false;
      DBxColumnList lstExistedColumns = new DBxColumnList(table.Columns.Count);
      columnsToAdd = null;
      // �� ����� ��������� ������������ DBxSqlBuffer Buffer2 = new DBxSqlBuffer(Buffer.Formatter);
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
          // ���� �� ����������
          if (columnsToAdd == null)
            columnsToAdd = new List<DBxColumnStruct>();
          columnsToAdd.Add(colDef);
        }
        else
        {
          lstExistedColumns.Add(colDef.ColumnName);
          // ��������� ������������ ����
          DataRow columnRow = tblColumns.DefaultView[columnRowIndex].Row;
          // ��������� ������������ ���� ������� ����������

          string realType = DataTools.GetString(columnRow, "type").ToUpperInvariant();
          int realLength;
          SplitValueType(ref realType, out realLength);
          bool realIsPK = DataTools.GetBool(columnRow, "pk");
          if (realIsPK)
          {
            if (realType == "INTEGER")
              realType = "INT";
          }

          Buffer.Clear();
          FormatValueType(Buffer, colDef, false);
          string wantedType = Buffer.SB.ToString();
          int wantedLength; // ����� Column.MaxLength
          SplitValueType(ref wantedType, out wantedLength);

          if (realType != wantedType)
          {
            errors.AddInfo("�������������� ���� ���� \"" + colDef.ColumnName + "\" ������� \"" +
              table.TableName + "\". ���������� ���� ���� " + colDef.ColumnType.ToString() +
              " ������������ ��� " + wantedType +
              " � �� ����� ��� �������� ��� ���� " + realType + ". ��������� ����������� �������");
            needsRecreate = true;
          }
          else
          {
            #region �������� ����� ���������� ����

            if (colDef.ColumnType == DBxColumnType.String)
            {
              // ����������� ��������� ���� �������� ������ �� �����, ��� ��� � ����� ������ �������
              // �� ��������, ��� � Access ��� MS SQL Server.
              // �������� ����, ��� ��� ��-�� ������������� �������� � ��������� ����� �����������
              // �����������, ���� ����� ��������� �������� ��������� �������

              if (realLength < wantedLength)
              {
                errors.AddInfo("���� \"" + colDef.ColumnName + "\" ������� \"" +
                      table.TableName + "\" ������ ����� ����� " + wantedLength.ToString() +
                      " ��������, � �� �����, ��� �������� ���� �������:  " + realLength.ToString() + " ��������. ��� ��������� ���� ��������� ����������� �������");
                needsRecreate = true;
              }
            } // ��������� ����

            #endregion
          } // RealType==WantedType

          #region �������� Default

          // ��������� �������� DEFAULT ������ ����������� �� ��������� NOT NULL, ����� ��������� ������

          string wantedDefExpr = String.Empty;
          if (colDef.DefaultExpression != null)
          {
            Buffer.Clear();
            Buffer.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
            wantedDefExpr = Buffer.SB.ToString();
          }
          string RealDefExpr = DataTools.GetString(columnRow, "dflt_value");
          if (RealDefExpr != wantedDefExpr)
          {
            errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
              "\" ������ ���� " + (RealDefExpr.Length > 0 ? "����������" : "�������") + " ������� DEFAULT. ��� ��������� �������� DEFAULT ��������� ����������� �������");
            needsRecreate = true;
          }

          #endregion

          #region �������� �������� Nullable

          // ��������� Nullable
          bool realNullable = !DataTools.GetBool(columnRow, "notnull");

          if (colDef.Nullable != realNullable)
          {
            errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
              "\" ���������� ������� " + (colDef.Nullable ? "\"NULL\"" : "\"NOT NULL\"") + ". ��� ��������� ����������� NULLABLE ��������� ����������� �������");
            needsRecreate = true;

            if ((!colDef.Nullable) && colDef.DefaultExpression != null)
            {
              if (columnsToReplaceNullToDef == null)
                columnsToReplaceNullToDef = new List<DBxColumnStruct>();
              columnsToReplaceNullToDef.Add(colDef);
            }
          }

          #endregion
        } // ���� ����������
      } // ���� �� ��������

      #region ��������� ������������ ���������� �����

      //if (CorrectPrimaryKey(Table, dvIndexColumns, Errors))
      //  Modified = true;

      #endregion

      existedColumns = new DBxColumns(lstExistedColumns); // ��� ������ ��������������
      return needsRecreate;
    }

    /// <summary>
    /// ������� � ���� ���� "CHAR(x)" ������ � ��������� �����
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
        throw new ParsingException("������ ������� �������� ���� \"" + typeStr + "\". " + e.Message, e);
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
      int p3 = s.IndexOf(',');
      if (p3 >= 0)
        s = s.Substring(0, p3); // 06.01.2022
      length = int.Parse(s);
      typeStr = typeStr.Substring(0, p1);
    }

    private void CreateTable(DBxTableStruct table, string tableName, DBxUpdateStructOptions options)
    {
      // 19.01.2020
      // ��������� ��������� ������, �������� �� Integer
      //
      // 09.03.2020
      // ��������� WITHOUT ROWID �� �����������, ���� ���� BLOB-����
      // TODO: ���� �� ��� ���������, ��� ������� ������ ������ �� ��������� 5% �� ������� ��������, �� ��� ������ �������
      // ����� �� ����� �������� ���������� ���� �����, �������� ������������ ������� � rowid.

      int pPrimaryKey;
      bool withoutRowId;
      switch (table.PrimaryKey.Count)
      {
        case 1:
          pPrimaryKey = table.Columns.IndexOf(table.PrimaryKey[0]);
          withoutRowId = table.PrimaryKeyColumns[0].ColumnType != DBxColumnType.Int && (!table.Columns.ContainsBlob());
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
        for (int i = 0; i < table.PrimaryKey.Count; i++) // ���������� 28.12.2020
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
          buffer.SB.Append("INTEGER"); // ������ ���������� �������� ��� ������ ��� ���������� �����
          return;
        }
      }
      buffer.FormatValueType(column);
    }

    #endregion

    #region �����������

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
        Buffer.SB.Append("COMMENT ON COLUMN "); // � �� ADD COLUMN
        Buffer.FormatTableName(tableName);
        Buffer.SB.Append(".");
        Buffer.FormatColumnName(columnName);
        Buffer.SB.Append(" IS ");
        Buffer.FormatValue(comment, DBxColumnType.String);
        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    #endregion

    #region �������

    /*
     * ���������� ����� �������� ���� "����������_1", "����������_2", ... ����� ���������� ������������
     * � �������� ���� ������
     */

    private void DropAllIndices(ISplash splash, string tableName)
    {
      splash.PhaseText = "�������� �������� ������� \"" + tableName + "\"";

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
      bool modified = false;

      // ������� ������������ ��������
      DataTable tblIndices = CreateIndicesTable(table.TableName);
      tblIndices.DefaultView.Sort = "IndexName";
      DataView dvIdxCols = new DataView(tblIndices);
      dvIdxCols.Sort = "Columns";


      #region ��������� ����������� �������

      for (int i = 0; i < table.Indexes.Count; i++)
      {
        string wantedCols = table.Indexes[i].Columns.AsString;
        int pIndex = dvIdxCols.Find(wantedCols);
        if (pIndex >= 0)
          // ������ ������
          dvIdxCols[pIndex].Row["Flag"] = true;
        else
        {
          // ��������� ������� ������
          // ��� ������ �������
          string indexName = GetNewIndexName(table.TableName, tblIndices);

          splash.PhaseText = "�������� ������� \"" + indexName + "\" ��� ������� \"" + table.TableName + "\", �������: " + wantedCols;

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
          errors.AddInfo("������ ������ \"" + indexName + "\" ��� ������� \"" + table.TableName + "\", �������: " + wantedCols);

          tblIndices.Rows.Add(indexName, wantedCols, true); // ����������� ���������, ����� ��������� ��� ������� ����� ������������
        }
      } // ���� �� ����������� ��������

      #endregion

      #region ������� ������ �������

      if (options.DropUnusedIndices)
      {
        foreach (DataRow idxRow in tblIndices.Rows)
        {
          if (!DataTools.GetBool(idxRow, "Flag"))
          {
            string indexName = DataTools.GetString(idxRow, "IndexName");
            splash.PhaseText = "�������� ������� ������� ��� ������� \"" + table.TableName + "\"";
            DropIndex(/*table.TableName, */indexName);
            errors.AddInfo("������ ������ ������ \"" + indexName + "\" � ������� \"" + table.TableName + "\"");
            modified = true;
          }
        }
      }

      #endregion

      return modified;
    }

    /// <summary>
    /// ������� �������, ���������� ���� "IndexName", "Columns" � "Flag".
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    private DataTable CreateIndicesTable(string tableName)
    {
      DataTable tblIndices = new DataTable();
      tblIndices.Columns.Add("IndexName", typeof(string));
      tblIndices.Columns.Add("Columns", typeof(string));
      tblIndices.Columns.Add("Flag", typeof(bool));

#if XXX // ��� ������-�� �� �������� ��� ��������� ��������. ����� �������� ����� �� ����� �����
      
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
            // ����������� ������. �� ��������������
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
          continue; // 18.01.2020. ����������� �� ����� ���������

        // ��������� - ��� ��, ��� � ������� �������
        int p1 = expr.IndexOf('(');
        if (p1 < 0)
          throw new BugException("�� ������� \"(\"");
        int p2 = expr.LastIndexOf(')');
        if (p2 < p1)
          throw new BugException("�� ������� \")\"");
        expr = expr.Substring(p1 + 1, p2 - p1 - 1);
        // ������ ����� ���������� �������, ���� - � ��������
        string[] a = expr.Split(',');
        for (int i = 0; i < a.Length; i++)
          a[i] = GetIndexColumnName(a[i]);

        tblIndices.Rows.Add(indexName, String.Join(",", a));
      }
#endif

      return tblIndices;
    }

    /// <summary>
    /// ��������� ��� ����, ������ ������� � �������
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
    /// ����������� ��� ��� ������ �������
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
      throw new BugException("�� ������ ��������� ��� ��� ������ ������� ������� \"" + tableName + "\". ������� ����� ������������ ��������");
    }
    #endregion

    #endregion
  }

  internal class SQLiteDBxManager : DBxManager
  {
    #region �����������

    private SQLiteDBxManager()
      : base(DBxProviderNames.SQLite)
    {
    }

    public static readonly SQLiteDBxManager TheManager = new SQLiteDBxManager();

    #endregion

    #region ���������������� ������

    public override DBx CreateDBObject(string connectionString)
    {
      return new SQLiteDBx(connectionString);
    }

    public override string ReplaceDBName(string connectionString, string oldDBName, string newDBName)
    {
      SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(connectionString);

      if (csb.DataSource.EndsWith(SQLiteDBx.MemoryFileName))
        return csb.ConnectionString; // 21.07.2021 � ��� ������ � ������ ���� � ���� ���

      //csb.DataSource = ReplaceDBItem(csb.DataSource, OldDBName, NewDBName);
      // 31.07.2018. ������ ������� ������ ������, �������� ��� �����
      AbsPath path = new AbsPath(csb.DataSource);
      if (String.Equals(path.FileNameWithoutExtension, oldDBName, StringComparison.OrdinalIgnoreCase))
      {
        string fileName = newDBName + path.Extension;
        path = new AbsPath(path.ParentDir, fileName);
        csb.DataSource = path.Path;
      }

      return csb.ConnectionString;
    }

    /// <summary>
    /// ���������� ������ �� SQLiteFactory
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
  /// ������ ��� ������ � ���� ������ SQLite.
  /// ��� SQLite ��� �������� ����� ��� ���������� ���������� �������, �� ���� �������� "INSERT OR UPDATE".
  /// ����� ������� ���������� ������� SQLiteCommand
  /// </summary>
  internal class SQLiteDBxDataWriter : DBxDataWriter
  {
    #region ����������� � Dispose

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

    #region �������������� �������

    private SQLiteCommand _Command;

    private void PrepareCommand()
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer(Con.DB.Formatter);
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          FormatInsertSQL(buffer);
          break;

        case DBxDataWriterMode.Update:
          FormatUpdateSql(buffer);
          break;

        case DBxDataWriterMode.InsertOrUpdate:
          // TODO: �����-���� ������������ "UPSERT", �� ������ ���� ��� ������� ������ �� ��� �������

          //if (((SQLiteDBxCon)Con).ServerVersion>=new Version(3, 24))
          //{ 
          //}
          //else
          //{
          // ���������� "�������" �������, ������� INSERT, ����� UPDATE
          // https://stackoverflow.com/questions/15277373/sqlite-upsert-update-or-insert/38463024#38463024
          // ��. ����� �69

          FormatUpdateSql(buffer);
          buffer.SB.Append(";");
          buffer.SB.Append(Environment.NewLine);

          //FormatInsertSQL(Buffer);
          buffer.SB.Append("INSERT INTO ");
          buffer.FormatTableName(WriterInfo.TableName);
          buffer.SB.Append(" (");
          buffer.FormatCSColumnNames(WriterInfo.Columns);
          buffer.SB.Append(") SELECT ");

          for (int i = 0; i < Values.Length; i++)
          {
            if (i > 0)
              buffer.SB.Append(',');
            buffer.FormatParamPlaceholder(i);
          }

          buffer.SB.Append(" WHERE (SELECT Changes()=0)");
          //}
          break;

        default:
          throw new BugException("����������� Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = new SQLiteCommand(buffer.SB.ToString());
      for (int i = 0; i < Values.Length; i++)
        _Command.Parameters.Add(new SQLiteParameter("P" + (i + 1).ToString(), null));
      _Command.Connection = ((SQLiteDBxCon)Con).Connection;
      _Command.Transaction = ((SQLiteDBxCon)Con).CurrentTransaction;
      _Command.CommandTimeout = Con.CommandTimeout;
      _Command.Prepare(); // ��� �������.
    }

    private void FormatInsertSQL(DBxSqlBuffer buffer)
    {
      buffer.SB.Append("INSERT INTO ");
      buffer.FormatTableName(WriterInfo.TableName);
      buffer.SB.Append(" (");
      buffer.FormatCSColumnNames(WriterInfo.Columns);
      buffer.SB.Append(") VALUES (");

      for (int i = 0; i < Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(',');
        buffer.FormatParamPlaceholder(i);
      }
      buffer.SB.Append(")");
    }

    private void FormatUpdateSql(DBxSqlBuffer buffer)
    {
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


      // TODO: ��������� ������� SQL-�������

      _Command.ExecuteNonQuery();
    }

    #endregion
  }
}
