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
    /// �������� ����������� � ���� ������.
    /// ������������� ��������� �������� ����� �����������
    /// </summary>
    /// <param name="connectionStringBuilder">��������� ������ �����������</param>
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
    /// ��������� ���� ������ �� ���������� ����
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
      return b;
    }

    /// <summary>
    /// ��� ������ ������������ ������������� ��� �������� ���� ������ � ������
    /// </summary>
    public SQLiteDBx()
      : this("Data Source=" + MemoryFileName)
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

    private object SyncRoot;

    /// <summary>
    /// ��������� ������������� ������ �������
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
      using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry))
      {
        // ������ �� ������. ���� ���������
        // 21.07.2021. �������. ���� ����������� ���������� � �������� Connection

        Con.Connection.GetSchema();
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
      using (SQLiteDBxCon Con = new SQLiteDBxCon(MainEntry))
      {
        Con.CommandTimeout = 0; // ����������� ����� ����������
        return Con.UpdateDBStruct(splash, errors, options);
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
    /// ��������� ����������, ���������� ��� ������ � SQLite.
    /// ��������� ������ ������������
    /// </summary>
    public static void LoadDLLs()
    {
      if (LoadDLLsFlag)
        return; // ��������� �����

      //if (IntPtr.Size == 8)
      //  System.Reflection.Assembly.Load("SQLite.Interop.x64");
      //else
      //  System.Reflection.Assembly.Load("SQLite.Interop.x86");

      System.Reflection.Assembly.Load("System.Data.SQLite"); // ����������� ����� SQLite.Interop.dll

      LoadDLLsFlag = true;
    }

    private static bool LoadDLLsFlag = false;

    #endregion

    #region ������� ������������� ������

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
          SQLiteParameter Param = new SQLiteParameter();
          //Param.ParameterName = "@P" + (i + 1).ToString();
          Param.ParameterName = ":P" + (i + 1).ToString(); // ���������� 29.05.2020
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
      DBxColumnType[] ColumnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);
      /*string PrimaryKeyColumnName = */
      Validator.CheckTablePrimaryKeyInt32(tableName);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("����� ����� �� ��������� � ������ ��������");

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
        throw new BugException("������� ������������ ������������� ��� ����������� ������ � ������� \"" + tableName + "\" Id=" + Id.ToString());

      return Id;
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
      DataTable Table = Connection.GetSchema("Tables"); // ���������� 28.08.2020
      string[] a = new string[Table.Rows.Count];
      for (int i = 0; i < Table.Rows.Count; i++)
        a[i] = DataTools.GetString(Table.Rows[i], "table_name");

      return a;
    }

    /// <summary>
    /// �������� �������� �������� ��������� �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>���������</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct TableStr = new DBxTableStruct(tableName);

      #region ������ ��������, ���, MaxLen, Nullable

      DataTable Table = Connection.GetSchema("Columns", new string[] { null, null, tableName }); // ���������� 28.08.2020
      Table.DefaultView.Sort = "ordinal_position"; // ����������� �� �������, ����� �������� ���� ����� �� ������

      foreach (DataRowView drv in Table.DefaultView)
      {
        string ColumnName = DataTools.GetString(drv.Row, "column_name");
        DBxColumnStruct ColStr = new DBxColumnStruct(ColumnName);

        string ColTypeString = DataTools.GetString(drv.Row, "data_type");
        // 28.08.2020
        // ������� ����� ����� �� ����� System.Data.SQLite\Resources\DataTypes.xml
        // �������� ������� System.Data.SQLite
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
          case "integer": // �� ����, ��� Int64, �� ���� ��� ������������ ��� ���������������
            ColStr.ColumnType = DBxColumnType.Int;
            ColStr.MinValue = Int32.MinValue;
            ColStr.MaxValue = Int32.MaxValue;
            break;

          case "counter": // ?? ����?
          case "autoincrement": // ?? ����?
          case "identity": // ?? ����?
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
          case "string": // ��� - ��� ��������
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
          case "smalldate": // ��� ��������
            ColStr.ColumnType = DBxColumnType.Date;
            break;

          case "time":
            ColStr.ColumnType = DBxColumnType.Time;
            break;

          case "datetime":
          case "timestamp": // ?? ��� ��������
            ColStr.ColumnType = DBxColumnType.DateTime;
            break;

          case "binary":
          case "varbinary":
          case "blob":
          case "image":
          case "general":
          case "oleobject": // ?? ����?
            ColStr.ColumnType = DBxColumnType.Binary;
            break;

          case "uniqueidentifier":
          case "guid":
            ColStr.ColumnType = DBxColumnType.Guid;
            break;

          case "xml": // ����� ���� ���, � ��� ��� ��������
            ColStr.ColumnType = DBxColumnType.Xml;
            break;

          default:
            throw new BugException("����������� ��� ����: \"" + ColTypeString + "\"");
        }

        ColStr.MaxLength = DataTools.GetInt(drv.Row, "character_maximum_length");

        string NullableStr = DataTools.GetString(drv.Row, "is_nullable").ToUpperInvariant();
        switch (NullableStr)
        {
          case "TRUE": ColStr.Nullable = true; break;  // ���������� 28.08.2020
          case "FALSE": ColStr.Nullable = false; break;
        }

        TableStr.Columns.Add(ColStr);
      }

      #endregion

      #region ����������� ��������� �����

      DataTable schema = Connection.GetSchema("FOREIGNKEYS", new string[] { null, null, tableName, null });
      foreach (DataRow row in schema.Rows)
      {
        string refColName = DataTools.GetString(row, "FKEY_FROM_COLUMN");
        DBxColumnStruct colStr = TableStr.Columns[refColName];
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

      TableStr.SetReadOnly();
      return TableStr;
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
      bool Modified = false;

      if (UpdateDBStructTables(splash, errors, options))
        Modified = true;

      splash.PhaseText = String.Empty;
      splash.PercentMax = 0;
      return Modified;
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

      bool Modified = false;

      #region ���������� ���������� �� ������������ ����� ������

      /*
       * ���������� ���������� ����� ������ ����� ��� ���� ������ ������������ � System.Data.SQLite.
       * �������� ��������� ���������� �� ������ ������� ��������.
       * ����� ����� ������ ������ ��������������, ��� ��� ����� �������� ����� �������������� ����������,
       * ������� �� ����� ��� ���������� ���������, �� ��� ������� �������� �������������� ������.
       */

      DataTable TableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(TableTables);
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
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        if (Table.Columns.Count == 0)
          throw new DBxStructException(Table, "�� ������ �� ������ �������");

        //CheckPrimaryKeyColumn(Table, Table.PrimaryKeyColumns[0]);
        //bool IndicesDropped = false;

        List<DBxColumnStruct> ColumnsToAdd;
        DBxColumns ExistedColumns;
        List<DBxColumnStruct> ColumnsToReplaceNullToDef; // ������� ��� ������ NULL �� DEFAULT

        if (dvTables.Find(Table.TableName) < 0)
        {
          #region ��������� ������ �������� �������

          splash.PhaseText = "��������� ������� \"" + Table.TableName + "\"";
          CreateTable(Table, Table.TableName, options);
          errors.AddInfo("������� ������� \"" + Table.TableName + "\"");
          Modified = true;

          #endregion
        }
        else if (NeedsRecreateTable(Table, /*dvColumns,*/ errors, out ColumnsToAdd, out ExistedColumns, out ColumnsToReplaceNullToDef))
        {
          #region ��������� ������������ � �������������� ������������� �������

          // ��. ������� �������� � ���������� ����� SQLite
          // lang_altertable.html

          DropAllIndices(splash, Table.TableName);
          //IndicesDropped = true;

          splash.PhaseText = "��������� ��������� ������� \"" + Table.TableName + "\"";

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
                    // �������� �������� NULL �� �������� �� ���������
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
                    errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName +
                      "\" �������� NULL �������� �� �������� �� ���������");
                  }
                }

                // ������� ������������� �������
                CreateTable(Table, AlterTableName, options);

                // ��������� ������. ����� ����� ���������� ����������, ���� ���� ������ ������������
                InsertIntoFrom(AlterTableName, Table.TableName, ExistedColumns);

                // ������� �������� �������
                DropTable(Table.TableName, false);

                // ��������������� �������
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
          errors.AddInfo("������� \"" + Table.TableName + "\" ����������� ��-�� ��������� ������� ��������");
          Modified = true;

          #endregion
        }
        else if (ColumnsToAdd != null)
        {
          #region ��������� ���������� ����������� ��������

          foreach (DBxColumnStruct Column in ColumnsToAdd)
          {
            splash.PhaseText = "���������� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName + "\"";
            Buffer.Clear();
            Buffer.SB.Append("ALTER TABLE ");
            Buffer.FormatTableName(Table.TableName);
            Buffer.SB.Append(" ADD "); // � �� ADD COLUMN
            AppendColumnDef(/*Table, */Column, false, options, true);
            SQLExecuteNonQuery(Buffer.SB.ToString());
            errors.AddInfo("������� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName + "\"");
            Modified = true;
          }

          #endregion
        }

        // ������� ����������

        #region �����������

        SetTableComment(Table.TableName, Table.Comment);
        for (int i = 0; i < Table.Columns.Count; i++)
          SetColumnComment(Table.TableName, Table.Columns[i].ColumnName, Table.Columns[i].Comment);

        #endregion

        UpdateDBStructIndices(Table, splash, errors, options);

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // ���� �� ��������

      return Modified;
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
      bool NeedsRecreate = false;
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

      foreach (DBxColumnStruct Column in table.Columns)
      {
        int ColumnRowIndex = tblColumns.DefaultView.Find(Column.ColumnName);
        if (ColumnRowIndex < 0)
        {
          // ���� �� ����������
          if (columnsToAdd == null)
            columnsToAdd = new List<DBxColumnStruct>();
          columnsToAdd.Add(Column);
        }
        else
        {
          lstExistedColumns.Add(Column.ColumnName);
          // ��������� ������������ ����
          DataRow ColumnRow = tblColumns.DefaultView[ColumnRowIndex].Row;
          // ��������� ������������ ���� ������� ����������

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
          int WantedLength; // ����� Column.MaxLength
          SplitValueType(ref WantedType, out WantedLength);

          if (RealType != WantedType)
          {
            errors.AddInfo("�������������� ���� ���� \"" + Column.ColumnName + "\" ������� \"" +
              table.TableName + "\". ���������� ���� ���� " + Column.ColumnType.ToString() +
              " ������������ ��� " + WantedType +
              " � �� ����� ��� �������� ��� ���� " + RealType + ". ��������� ����������� �������");
            NeedsRecreate = true;
          }
          else
          {
            #region �������� ����� ���������� ����

            if (Column.ColumnType == DBxColumnType.String)
            {
              // ����������� ��������� ���� �������� ������ �� �����, ��� ��� � ����� ������ �������
              // �� ��������, ��� � Access ��� MS SQL Server.
              // �������� ����, ��� ��� ��-�� ������������� �������� � ��������� ����� �����������
              // �����������, ���� ����� ��������� �������� ��������� �������

              if (RealLength < WantedLength)
              {
                errors.AddInfo("���� \"" + Column.ColumnName + "\" ������� \"" +
                      table.TableName + "\" ������ ����� ����� " + WantedLength.ToString() +
                      " ��������, � �� �����, ��� �������� ���� �������:  " + RealLength.ToString() + " ��������. ��� ��������� ���� ��������� ����������� �������");
                NeedsRecreate = true;
              }
            } // ��������� ����

            #endregion
          } // RealType==WantedType

          #region �������� Default

          // ��������� �������� DEFAULT ������ ����������� �� ��������� NOT NULL, ����� ��������� ������

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
            errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + table.TableName +
              "\" ������ ���� " + (RealDefExpr.Length > 0 ? "����������" : "�������") + " ������� DEFAULT. ��� ��������� �������� DEFAULT ��������� ����������� �������");
            NeedsRecreate = true;
          }

          #endregion

          #region �������� �������� Nullable

          // ��������� Nullable
          bool RealNullable = !DataTools.GetBool(ColumnRow, "notnull");

          if (Column.Nullable != RealNullable)
          {
            errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + table.TableName +
              "\" ���������� ������� " + (Column.Nullable ? "\"NULL\"" : "\"NOT NULL\"") + ". ��� ��������� ����������� NULLABLE ��������� ����������� �������");
            NeedsRecreate = true;

            if ((!Column.Nullable) && Column.DefaultExpression != null)
            {
              if (columnsToReplaceNullToDef == null)
                columnsToReplaceNullToDef = new List<DBxColumnStruct>();
              columnsToReplaceNullToDef.Add(Column);
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
      return NeedsRecreate;
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
        for (int i = 0; i < table.PrimaryKey.Count; i++) // ���������� 28.12.2020
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
      bool Modified = false;

      // ������� ������������ ��������
      DataTable tblIndices = CreateIndicesTable(table.TableName);
      tblIndices.DefaultView.Sort = "IndexName";
      DataView dvIdxCols = new DataView(tblIndices);
      dvIdxCols.Sort = "Columns";


      #region ��������� ����������� �������

      for (int i = 0; i < table.Indexes.Count; i++)
      {
        string WantedCols = table.Indexes[i].Columns.AsString;
        int pIndex = dvIdxCols.Find(WantedCols);
        if (pIndex >= 0)
          // ������ ������
          dvIdxCols[pIndex].Row["Flag"] = true;
        else
        {
          // ��������� ������� ������
          // ��� ������ �������
          string IndexName = GetNewIndexName(table.TableName, tblIndices);

          splash.PhaseText = "�������� ������� \"" + IndexName + "\" ��� ������� \"" + table.TableName + "\", �������: " + WantedCols;

          Buffer.Clear();
          Buffer.SB.Append("CREATE INDEX \"");
          Buffer.SB.Append(IndexName);
          Buffer.SB.Append("\" ON ");
          Buffer.FormatTableName(table.TableName);
          Buffer.SB.Append(" (");
          Buffer.FormatCSColumnNames(table.Indexes[i].Columns);
          Buffer.SB.Append(")");
          SQLExecuteNonQuery(Buffer.SB.ToString());

          Modified = true;
          errors.AddInfo("������ ������ \"" + IndexName + "\" ��� ������� \"" + table.TableName + "\", �������: " + WantedCols);

          tblIndices.Rows.Add(IndexName, WantedCols, true); // ����������� ���������, ����� ��������� ��� ������� ����� ������������
        }
      } // ���� �� ����������� ��������

      #endregion

      #region ������� ������ �������

      if (options.DropUnusedIndices)
      {
        foreach (DataRow IdxRow in tblIndices.Rows)
        {
          if (!DataTools.GetBool(IdxRow, "Flag"))
          {
            string IndexName = DataTools.GetString(IdxRow, "IndexName");
            splash.PhaseText = "�������� ������� ������� ��� ������� \"" + table.TableName + "\"";
            DropIndex(/*table.TableName, */IndexName);
            errors.AddInfo("������ ������ ������ \"" + IndexName + "\" � ������� \"" + table.TableName + "\"");
            Modified = true;
          }
        }
      }

      #endregion

      return Modified;
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
      foreach (DataRow ListRow in tblSchema.Rows)
      {
        string IndexName = DataTools.GetString(ListRow, "name");
        string Expr = DataTools.GetString(ListRow, "sql");

        if (Expr.Length == 0)
          continue; // 18.01.2020. ����������� �� ����� ���������

        // ��������� - ��� ��, ��� � ������� �������
        int p1 = Expr.IndexOf('(');
        if (p1 < 0)
          throw new BugException("�� ������� \"(\"");
        int p2 = Expr.LastIndexOf(')');
        if (p2 < p1)
          throw new BugException("�� ������� \")\"");
        Expr = Expr.Substring(p1 + 1, p2 - p1 - 1);
        // ������ ����� ���������� �������, ���� - � ��������
        string[] a = Expr.Split(',');
        for (int i = 0; i < a.Length; i++)
          a[i] = GetIndexColumnName(a[i]);

        tblIndices.Rows.Add(IndexName, String.Join(",", a));
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
        string IndexName = tableName + "_" + i.ToString();
        if (tblIndices.DefaultView.Find(IndexName) < 0)
          return IndexName;
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
          // TODO: �����-���� ������������ "UPSERT", �� ������ ���� ��� ������� ������ �� ��� �������

          //if (((SQLiteDBxCon)Con).ServerVersion>=new Version(3, 24))
          //{ 
          //}
          //else
          //{
          // ���������� "�������" �������, ������� INSERT, ����� UPDATE
          // https://stackoverflow.com/questions/15277373/sqlite-upsert-update-or-insert/38463024#38463024
          // ��. ����� �69

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
          throw new BugException("����������� Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = new SQLiteCommand(Buffer.SB.ToString());
      for (int i = 0; i < Values.Length; i++)
        _Command.Parameters.Add(new SQLiteParameter("P" + (i + 1).ToString(), null));
      _Command.Connection = ((SQLiteDBxCon)Con).Connection;
      _Command.Transaction = ((SQLiteDBxCon)Con).CurrentTransaction;
      _Command.CommandTimeout = Con.CommandTimeout;
      _Command.Prepare(); // ��� �������.
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


      // TODO: ��������� ������� SQL-�������

      _Command.ExecuteNonQuery();
    }

    #endregion
  }
}
