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
  /// ���� ������ PostGreSQL ����� ��������� Npgsql.
  /// ��� ������������� ������ ���� ���������� ������ Npgsql.dll � Mono.Security �� ����������� FreeLibSet/Others
  /// </summary>
  public class NpgsqlDBx : DBx
  {
    #region �����������

    /// <summary>
    /// �������� ����������� � ���� ������.
    /// ������������� ��������� �������� ����� �����������
    /// </summary>
    /// <param name="connectionStringBuilder">��������� ������ �����������</param>
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
    /// �������� ����������� � ���� ������.
    /// ������������� ��������� �������� ����� �����������
    /// </summary>
    /// <param name="connectionString">������ �����������</param>
    public NpgsqlDBx(string connectionString)
      : this(new NpgsqlConnectionStringBuilder(connectionString))
    {
    }

    internal static string GetDataBaseName(NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
      if (!String.IsNullOrEmpty(connectionStringBuilder.Database))
        return connectionStringBuilder.Database;
      else
        throw new ArgumentException("� ������ ����������� �� ����� �������� Database, ������������ ���� ������", "connectionStringBuilder");
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ��� ���� ������
    /// </summary>
    public override string DatabaseName { get { return _DatabaseName; } }
    private readonly string _DatabaseName;

    /// <summary>
    /// ������� ����� ����� � ���� ������.
    /// �� �������� ���� ����������� ���� ������������
    /// </summary>
    public new NpgsqlDBxEntry MainEntry { get { return (NpgsqlDBxEntry)(base.MainEntry); } }

    /// <summary>
    /// ������� ����� �����
    /// </summary>
    /// <param name="permissions">���������� �� ������ � ���� ������</param>
    /// <returns></returns>
    public override DBxEntry CreateEntry(DBxPermissions permissions)
    {
      return new NpgsqlDBxEntry(this, MainEntry.ConnectionStringBuilder, permissions);
    }


    private readonly object _SyncRoot;

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
    /// �������� ���������� ������� pg_database_size()
    /// </summary>
    /// <returns>������ ���� ������ � ������</returns>
    public override long GetDBSize()
    {
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, false))
      {
        return DataTools.GetInt64(con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
      }
    }

    /// <summary>
    /// ���������� ������ �� NpgsqlFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return NpgsqlFactory.Instance; }
    }

    /// <summary>
    /// ���������� ����������� ��������� ��������� ��� ������ PostgreSQL
    /// </summary>
    public override DBxManager Manager { get { return NpgsqlDBxManager.TheManager; } }

    #endregion

    #region ���������� ���������

    /// <summary>
    /// ���������� true, ���� ���� ������ ����������
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
    /// ������� ���� ������, ���� �� �� ����������
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, true))
      {
        con.CommandTimeout = 0; // ����������� ����� ����������
        con.CreateDatabase();
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
      using (NpgsqlDBxCon con = new NpgsqlDBxCon(MainEntry, false))
      {
        con.CommandTimeout = 0; // ����������� ����� ����������
        return con.UpdateDBStruct(splash, errors, options);
      }
    }


    /// <summary>
    /// �������� ���� ������, ���� ��� ����������
    /// </summary>
    /// <returns>True, ���� ������������ ���� ������ ���� �������.
    /// False, ���� ���� ������ �� ����������������</returns>
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
        con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        buffer.SB.Append("DROP TABLE IF EXISTS ");
        buffer.FormatTableName(tableName);
        con.SQLExecuteNonQuery(buffer.SB.ToString());
      }
    }

    #endregion
  }

  /// <summary>
  /// ����� ����� ��� ���� ������ PostgreSql
  /// </summary>
  public class NpgsqlDBxEntry : DBxEntry
  {
    #region ������������

    /// <summary>
    /// ���������� ����������� ��� �������� ����� �����
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
    /// ������ ��� ����������� � ������������ ���� ������ (��������� ���� � ������ ������� �����������)
    /// </summary>
    /// <param name="db">���� ������</param>
    /// <param name="connectionString">������ �����������</param>
    /// <param name="permissions">����������</param>
    public NpgsqlDBxEntry(NpgsqlDBx db, string connectionString, DBxPermissions permissions)
      : this(db, new NpgsqlConnectionStringBuilder(connectionString), permissions)
    {
    }

    /// <summary>
    /// ������ ��� ����������� � ������������ ���� ������ (��������� ���� � ������ ������� �����������)
    /// </summary>
    /// <param name="db">���� ������</param>
    /// <param name="connectionStringBuilder">������ �����������</param>
    /// <param name="permissions">����������</param>
    public NpgsqlDBxEntry(NpgsqlDBx db, NpgsqlConnectionStringBuilder connectionStringBuilder, DBxPermissions permissions)
      : base(db, permissions, false)
    {
      _ConnectionStringBuilder = connectionStringBuilder;
      _ConnectionString = connectionStringBuilder.ConnectionString;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ������
    /// </summary>
    public new NpgsqlDBx DB { get { return (NpgsqlDBx)(base.DB); } }

    // �������� ConnectionString �� ����� ������ public. ��� ����� ���� ������

    internal NpgsqlConnectionStringBuilder ConnectionStringBuilder { get { return _ConnectionStringBuilder; } }
    private readonly NpgsqlConnectionStringBuilder _ConnectionStringBuilder;

    internal string ConnectionString { get { return _ConnectionString; } }
    private readonly string _ConnectionString;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� ����� ����������
    /// ���� ����� ����� ���������� ����������
    /// </summary>
    /// <returns>���������� � ������ ������</returns>
    public override DBxConBase CreateCon()
    {
      return new NpgsqlDBxCon(this, false);
    }

    /// <summary>
    /// ������� ����� ����� ����� � ������ ������� ����
    /// </summary>
    /// <param name="newPermissions">��������� ���������� �� ������ � �������� ���� ������</param>
    /// <returns>����� ����� �����</returns>
    public override DBxEntry Clone(DBxPermissions newPermissions)
    {
      return new NpgsqlDBxEntry(DB, ConnectionString, newPermissions);
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
    /// <param name="cs">������ ����������� � �������</param>
    /// <returns>������ ����������� ��� ������</returns>
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
        return "������ ��������� ������ �����������. " + e.Message;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� � ����� ������ PostgreSQL
  /// </summary>
  public class NpgsqlDBxCon : DBxConBase
  {
    #region ����������� � Dispose

    internal NpgsqlDBxCon(NpgsqlDBxEntry entry, bool serverWide)
      : base(entry)
    {
      _ServerWide = serverWide;
    }

    /// <summary>
    /// ��������� ����������� ADO.NET, ���� ��� ���� �������, � ���������� ��� � ���.
    /// ������� ���������� �� ����� �����.
    /// </summary>
    /// <param name="disposing">True, ���� ��� ������ ����� Dispose().
    /// False, ���� ������ ����������</param>
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

    #region ��������

    /// <summary>
    /// ����� �����
    /// </summary>
    public new NpgsqlDBxEntry Entry { get { return (NpgsqlDBxEntry)(base.Entry); } }

    /// <summary>
    /// ���� ������
    /// </summary>
    public new NpgsqlDBx DB { get { return (NpgsqlDBx)(base.DB); } }

    /// <summary>
    /// ���� true, �� ����������� ������������ ��� ������ �� ������� ��� ������. 
    /// </summary>
    private readonly bool _ServerWide;

    #endregion

    #region ����������

    /// <summary>
    /// ���������� ���������� ADO.NET.
    /// ������ ��������� ��� ������ ��������� � ��������
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
        //char[] a = e.Message.ToCharArray();
        e.Data["NpgsqlDBx.DatabaseName"] = DB.DatabaseName;
        e.Data["NpgsqlDBxCon.ServerWide"] = _ServerWide.ToString();
        e.Data["NpgsqlDBxCon.ConnectionString"] = NpgsqlDBxEntry.GetUnpasswordedConnectionString(_Connection.ConnectionString);
        e.Data["NpgsqlDBxCon.Remark"] = @"���� ��������� �� ������ �� �������� (�������� ������ ����), ��������� ������ �� ������ ��������� �� ���������� ����� � ����� postgresql.conf. ������� ������ <lc_messages = 'en_EN.UTF-8'>";
        throw;
      }
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
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
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
    protected override void DoSQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      NpgsqlCommand cmd = new NpgsqlCommand(cmdText, Connection);
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
    /// ����������� ����� ���������� SLQ-�������, ������������� DbDataReader
    /// </summary>
    /// <param name="cmdText">����� SQL-�������</param>
    /// <param name="paramValues">��������� �������</param>
    /// <returns>������ ��� ������ ������</returns>
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

    #region ���������������� ���������� ���������� ��������

    #region ������� �����

    /// <summary>
    /// ���������������� ������ ������� ��������, ��� ������� �����.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>������ �������?</returns>
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
      string PrimaryKeyColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);

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
      Buffer.FormatCSValues(values, ColumnTypes);
      Buffer.SB.Append(')');

      //DBxTableStruct ts = DB.Struct.Tables[tableName];
      Buffer.SB.Append(" RETURNING ");
      Buffer.FormatColumnName(PrimaryKeyColumnName);

      id = DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString()));

      if (id <= 0)
        throw new BugException("������� ������������ ������������� ��� ����������� ������ � ������� \"" + tableName + "\" Id=" + id.ToString());

      return id;
    }

    /// <summary>
    /// ���������� ������ � �������.
    /// �������� ����� ���������, � ����� � �� ��������� ��������� ����
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">����� ��������</param>
    /// <param name="values">��������</param>
    public override void AddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      base.AddRecord(tableName, columnNames, values);

      // ������������ ������������������ ��� ���������� �����
      int seqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, columnNames);
      if (seqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.GetInt(values[seqColumnIndex]));
    }

    /// <summary>
    /// ������ �������������� �������� ���� ��� ��������� ���������� ������� � AddRecords
    /// </summary>
    private enum CopyFormattingMode
    {
      /// <summary>
      /// �������������� �������� ����������� ������� NpgsqlDBxSqlFormatter.FormatValue()
      /// </summary>
      FormatValue,

      /// <summary>
      /// �������������� ����������� � ������� NpgsqlCopySerializer.AddString()
      /// </summary>
      String,

      /// <summary>
      /// �������������� ����������� � ������� NpgsqlCopySerializer.AddString(), �� ������� ������ �������������
      /// � Guid, � ����� ������� � ������, ����� ������ ��������� ��������������
      /// </summary>
      Guid,
    }

    /// <summary>
    /// ��������� ���������� ������� � �������������� NpgsqlCopyIn � NpgsqlCopySerializer
    /// </summary>
    /// <param name="tableName">��� �������, � ������� ����������� ������</param>
    /// <param name="table">�������� �������</param>
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

      #region ���������� ������ �����������

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
          case DBxColumnType.Guid: // 08.10.2019 - ��� GUID'� �� ����� ��������� ���������
            Modes[i] = CopyFormattingMode.Guid;
            break;
          default:
            // ����������� ������.
            // ���������� ���������� ����������
            base.DoAddRecords(tableName, table); // ���������� 18.12.2020
            return;
        }
      }

      #endregion

      // ������� ������������� ���� �� http://stackoverflow.com/questions/11824967/how-to-use-npgsqlcopyin-with-npgsqlcopyserializer

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
                Buffer.FormatValue(row[i], colDef.ColumnType); // ���� �����������
                serializer.AddString(Buffer.SB.ToString());
                break;
              case CopyFormattingMode.String:
                s = DataTools.GetString(row[i]);
                serializer.AddString(s); // ����� �� �����������
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

    #region ������������� ������������������

    // 09.06.2017
    // ��������.
    // � ������� �� MS SQL Server, �������� ��������� ���� ����������� ����� "������������������",
    // ������� �������� ��������������� ���������. ���� ��� ���������� ������ �������� ��������
    // ��� ���� ���������������� ����, �� ������������������ "��������". ��� ��������� ����������
    // ������ ��� �������� ���� ���������� �����, ��������� ������ ���������� ��������.
    //
    // ��� ���������� AddRecord() � AddRecords() � ����� ���������� �����, �������� ����������
    // ������� setval(), ����� "���������" ��������

    /// <summary>
    /// ���� - ��� �������
    /// �������� - ��� ������������������, ������������ ����� ���������� �����
    /// </summary>
    private Dictionary<string, string> _TablePKSequenceNames;


    /// <summary>
    /// "������������" �������������� ��� ������������������
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

      // ��� ������� ���� � � ���������� � � ��������
      // � ��� ������� - ������ � ����������
      // �.�. ������
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

    #region �������� �������

    /// <summary>
    /// �������� ���� ����� �������. ��������� SQL-������ "TRUNCATE TABLE".
    /// </summary>
    /// <param name="tableName">��� ��������� �������</param>
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

    #region ����������

    /// <summary>
    /// ������� ����������, ���� ��� ����� ������ TransactionBegin(), ��� null, ���� ��� �������� ����������
    /// </summary>
    public new NpgsqlTransaction CurrentTransaction
    {
      get { return (NpgsqlTransaction)(base.CurrentTransaction); }
    }

    #endregion

    #region ���������� ����� ������

    /// <summary>
    /// �������� ������ ������ ������
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
    /// �������� �������� �������� ��������� �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>���������</returns>
    internal protected override DBxTableStruct GetRealTableStructFromSchema(string tableName)
    {
      DBxTableStruct tableStr = new DBxTableStruct(tableName);

      #region ������ ��������, ���, MaxLen, Nullable

      DataTable table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "public", tableName });
      table.DefaultView.Sort = "ordinal_position"; // ����������� �� �������, ����� �������� ���� ����� �� ������

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
            // TODO: ������������ ����� ���� ��� ���������� float/double
            colStr.MinValue = Double.MinValue;
            colStr.MaxValue = Double.MaxValue;
            break;
          case "real":
          case "float8":
            colStr.ColumnType = DBxColumnType.Float;
            // TODO: ������������ ����� ���� ��� ���������� float/double
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

      #region ����������� ��������� ����� (����������� FOREIGN KEY)

      //DataTable Table2 = Connection.GetSchema();
      //string[]aaa=DataTools.GetStringsFromField(Table2, "CollectionName");

      //Table = Connection.GetSchema("Restrictions", new string[] { DB.DatabaseName, "public", TableName });
      //foreach (DataRow r in Table.Rows)
      //{ 
      //}


      int tableOID = GetTableOID(tableName);
      if (tableOID == 0)
        throw new BugException("�� ������� �������� ������������� object_id ������� \"" + tableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append(@"SELECT confrelid,confdeltype,conkey FROM pg_catalog.pg_constraint WHERE contype='f' AND conrelid=");
      Buffer.FormatValue(tableOID, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "pg_constraint");
      foreach (DataRow row in tbl.Rows)
      {
        Int32 refTableOID = DataTools.GetInt(row, "confrelid");
        string refTableName = GetTableNameFromOID(refTableOID);
        if (String.IsNullOrEmpty(refTableName))
          throw new BugException("�� ������� ��� ��� ������-������� � OID=" + refTableOID);

        Int16[] detColPoss = (Int16[])(row["conkey"]);
        // �� ����� Int16[] RefColPoss = (Int16[])(Row["confkey"]);
        if (detColPoss.Length != 1 /*|| RefColPoss.Length != 1*/)
          continue; // FOREIGN KEY �� ���������� ����� �� ��������������

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

    #region ��������� ����������

    /// <summary>
    /// ���������� ������������� �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>������������� OID</returns>
    public Int32 GetTableOID(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      // ��� ������� ���� � ����������, � �� � ��������, �.�. ��� - ������
      return DataTools.GetInt(SQLExecuteScalar("SELECT oid FROM pg_catalog.pg_class WHERE relname=\'" + tableName + "\' AND relkind=\'r\'"));
    }

    /// <summary>
    /// ���������� ��� ������� �� ��������������
    /// </summary>
    /// <param name="oid">������������� OID</param>
    /// <returns>��� �������</returns>
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

    #region ���������� ���������

    #region �������� ����� UpdateDBStruct

    internal bool UpdateDBStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      bool modified = false;

      // ������� � �����������.
      // � ������� �� MS SQL Server, ���������, ����� ����� �������� ���� ��������� � ��������
      // ���� ������, � �� � �������� �������
      // � ����������� ������������ ������������� ������ ������� �������� ����� "����������_�������_index"
      // � ���� �����, ������� ���������� ��� ��������� ��������, �������� "test2_mm_idx".
      // � ���� ��, ����������� �� 31 ������ ����� �� �������
      //
      // �������, ���������� �������:
      // 1. ��� ��������� ������ ���������� ����������� ��� ����������� "����������_pkey".
      //    ��������� ������ �� ���������
      // 2. ��� ������� ������ ��������� ������� � ������������� �������, ������������ ����,
      //    � ����� ��������� � ��������� ������� pg_constraitns �� ����� ������� � ����
      // 3. ��� �������� ���������� �����, ����������� ���� "����������_�������".
      //    �������� DBxIndexDef.IndexName �� �����������

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

    #region ���������� ������, �������� � ��������� ������

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;

      #region ���������� ���������� �� ������������ ����� ������

      DataTable tableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(tableTables);
      dvTables.Sort = "TABLE_NAME";

      // �������
      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // ����� ����� �������

      // ������ �� �������� 
      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";
      //DebugTools.DebugDataView(dvIndexColumns, "������� �������� ���� ������ "+DB.DisplayName);

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

        if (dvTables.Find(table.TableName) < 0)
        {
          #region ��������� ������ �������� �������

          splash.PhaseText = "��������� ������� \"" + table.TableName + "\"";
          CreateTable(table);
          errors.AddInfo("������� ������� \"" + table.TableName + "\"");
          modified = true;

          #endregion
        }
        else
        {
          #region ��������� ������������ ���������� �����

          if (CorrectPrimaryKey(table, dvIndexColumns, errors))
            modified = true;

          #endregion

          #region ��������� ������� ����������� �����

          DBxSqlBuffer buffer2 = new DBxSqlBuffer(Buffer.Formatter);

          foreach (DBxColumnStruct colDef in table.Columns)
          {
            int columnRowIndex = dvColumns.Find(new object[] { table.TableName, colDef.ColumnName });
            if (columnRowIndex < 0)
            {
              // ���� �� ����������
              splash.PhaseText = "���������� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE ");
              Buffer.FormatTableName(table.TableName);
              Buffer.SB.Append(" ADD "); // � �� ADD COLUMN
              AppendColumnDef(/*Table, */colDef, false, true);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("������� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName + "\"");
              modified = true;
            }
            else
            {
              // ��������� ������������ ����
              DataRow columnRow = dvColumns[columnRowIndex].Row;
              // ��������� ������������ ���� ������� ����������
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
                errors.AddError("�������������� ���� ���� \"" + colDef.ColumnName + "\" ������� \"" +
                    table.TableName + "\". ���������� ���� ���� " + colDef.ColumnType.ToString() +
                    " ������������ ��� " + wantedType +
                    " � �� ����� ��� �������� ��� ���� " + realType);
              }
              else
              {
                #region �������� ����� ���������� ����

                if (colDef.ColumnType == DBxColumnType.String)
                {
                  int realLen = DataTools.GetInt(columnRow, "CHARACTER_MAXIMUM_LENGTH");
                  if (realLen != colDef.MaxLength)
                  {
                    if (realLen > colDef.MaxLength)
                    {
                      // !!! ��������, ������ �� ��������� ����
                      errors.AddWarning("���� \"" + colDef.ColumnName + "\" ������� \"" +
                          table.TableName + "\" ������ ����� ����� " + colDef.MaxLength.ToString() +
                          " ��������, � �� �����, ��� �������� ���� �������:  " + realLen.ToString() + " ��������");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // ����� ����������� ��� �������
                      errors.AddInfo("��� ������������ ������� ������� \"" + table.TableName + "\" ����� ������� ��-�� ��������� ������� ���� \"" + colDef.ColumnName + "\"");
                      if (DeleteAllIndices(table.TableName, splash, errors))
                        modified = true;

                      // ����������� ����� ����
                      splash.PhaseText = "��������� ����� ���� \"" + colDef.ColumnName + "\" � ������� \"" + table.TableName + "\"";
                      AlterColumn(table, colDef, true, false);
                      errors.AddInfo("����� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
                        "\" ��������� � " + realLen.ToString() + " �� " + colDef.MaxLength.ToString() + " ��������");
                      modified = true;
                    }
                  }
                } // ��������� ����

                #endregion

                #region �������� Default

                // ��������� �������� DEFAULT ������ ����������� �� ��������� NOT NULL, ����� ��������� ������

                string wantedDefExpr = String.Empty;
                if (colDef.DefaultExpression != null)
                {
                  buffer2.Clear();
                  buffer2.FormatExpression(colDef.DefaultExpression, new DBxFormatExpressionInfo());
                  wantedDefExpr = buffer2.SB.ToString();
                }
                string realDefExpr = DataTools.GetString(columnRow, "COLUMN_DEFAULT");
                if (realDefExpr.StartsWith("nextval(", StringComparison.OrdinalIgnoreCase))
                  realDefExpr = String.Empty; // ����� ������� ��������� ����
                if (realDefExpr != wantedDefExpr)
                {
                  // ����� ������� ������� ������ �������, ����� ��������� �����
                  if (realDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(colDef.ColumnName);
                    Buffer.SB.Append(" DROP DEFAULT");

                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
                        "\" ������ ������� DEFAULT");
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

                    errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
                      "\" ���������� ������� DEFAULT " + wantedDefExpr);
                  }
                  modified = true;
                }

                #endregion

                #region �������� �������� Nullable

                // ��������� Nullable
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

                  #region ������ NULL'�� �� DEFAULT

                  if ((!colDef.Nullable) && colDef.DefaultExpression != null && (!IsTableEmpty(table.TableName)))
                  {
                    //SetValue(Table.TableName, new ValueFilter(Column.ColumnName, null, CompareKind.Equal, Column.ColumnType),Column.ColumnName, Column.Default)
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

                  #endregion

                  // ������ ���� NULLABLE
                  AlterColumn(table, colDef, false, true);
                  errors.AddInfo("��� ���� \"" + colDef.ColumnName + "\"� ������� \"" + table.TableName +
                    "\" ���������� ������� " + (colDef.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  modified = true;
                }

                #endregion
              }
            } // ���� ����������
          } // ���� �� ��������

          #endregion
        }

        // ������� ����������

        #region �����������

        SetTableComment(table.TableName, table.Comment);
        for (int i = 0; i < table.Columns.Count; i++)
          SetColumnComment(table.TableName, table.Columns[i].ColumnName, table.Columns[i].Comment);


        #endregion

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // ���� �� ��������

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
              buffer.SB.Append("SERIAL"); // � ���������������
            else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
              buffer.SB.Append("SERIAL"); // � ���������������
            else
              buffer.SB.Append("BIGSERIAL");
            return;
        }
      }
      buffer.FormatValueType(column);
    }

    #region �������� ���� ������

    /// <summary>
    /// ���� - ��� ������, ������� ����� ���������� PostGreSQL
    /// �������� - "��������" ��� ������, ������� �� ���������� � ����������
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
      d.Add("BPCHAR", "CHAR"); // ���������� 20.09.2019
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
    /// ���������� ALTER TABLE ALTER COLUMN
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
          throw new DBxStructException(Table, "������ �������� ������� ������ ���� �������� ��������� ����. ������� \"" +
            Column.ColumnName + "\" ����� ������������ ��� \"" + Column.ColumnType.ToString() + "\"");
      }

      if (Column.Nullable)
        throw new DBxStructException(Table, "������ �������� ������� ������ ���� �������� ��������� ����. ������� \"" +
          Column.ColumnName + "\" �� ����� ���� �������� ��� Nullable");

    } */

    /// <summary>
    /// ��������� ������� ���������� �����. ������������ ����������� ���������� ����� ���������,
    /// ����� ����������� ���������
    /// </summary>
    /// <param name="table"></param>
    /// <param name="dvIndexColumns"></param>
    /// <param name="errors"></param>
    /// <returns>true, ���� ���� ������� ���������</returns>
    private bool CorrectPrimaryKey(DBxTableStruct table, DataView dvIndexColumns, ErrorMessageList errors)
    {
      if (table.PrimaryKey.Count != 1)
        throw new NotSupportedException("� ������� ������ ���� ��������� ���� �� ������ ����");

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
          // ����������� ����� ����� ��� � ����� ��������.
          // ALTER TABLE DROP CONSTRAINT ������� �������� ����� ����������� � ������ ��������.
          // ����� ���, ��� ��� �������� � ���� ������.
          string pkName2 = DataTools.GetString(drvCol.Row, "INDEX_NAME"); 

          //if (IndexName.StartsWith("Index"))
          //  continue; // ��������� ���������������� ������, � ������� ������ ���� "Id"
          SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" DROP CONSTRAINT \"" + pkName2 + "\"");
          errors.AddInfo("������� ������������ ����������� ���������� ����� \"" + pkName2 + "\" � ������� \"" + table.TableName + "\"");
          modified = true;
          break;
        }
      }

      if (!found)
      {
        SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" ADD CONSTRAINT \"" + pkName + "\" PRIMARY KEY (\"" + table.PrimaryKey[0] + "\")");
        errors.AddInfo("��������� ����������� ���������� ����� \"" + pkName + "\" � ������� \"" + table.TableName + "\"");
        modified = true;
      }

      return modified;
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

    #region ������� �����

    private bool UpdateDBStructForeignKeys(ISplash splash, ErrorMessageList errors)
    {
      bool modified = false;

      #region ������� TableForeignKeys

      // ������� ����� ����� ��������� ������ ����� �������� ���� ������
      // ��� ������ �������� ������� ��� �����������
      //// ������ �� ������� ������
      //DataTable TableForeignKeys = Connection.GetSchema("ForeignKeys");
      //DataView dvForeignKeys = new DataView(TableForeignKeys);
      //dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // �������
      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION";

      // ���������� ��������� ������� pg_constraint
      // 0: conname - ��� ��������� (��� ��������, ���� ���� �����)
      // 1: conrelid::regclass - ��� ������� � ��������
      // 2: conkey - ������ �������� �������� ��������� ����� (��� ���������� ������ ����������� �� ������ ��������)
      // contype='f' - ������ �� ������������ �������� �����
      DataTable tablePGC = DoSQLExecuteDataTable(@"SELECT conname, conrelid::regclass, conkey FROM pg_catalog.pg_constraint WHERE contype='f'", "pg_constraint", null);
      // �������� ������
      DataTable tableForeignKeys = new DataTable();
      tableForeignKeys.Columns.Add("CONSTRAINT_NAME", typeof(string));
      tableForeignKeys.Columns.Add("TABLE_NAME", typeof(string));
      tableForeignKeys.Columns.Add("COLUMN_NAME", typeof(string));
      foreach (DataRow rowPGC in tablePGC.Rows)
      {
        Array aColIdxs = rowPGC[2] as Array;
        if (aColIdxs == null)
        {
          errors.AddWarning("� ��������� ������� \"pg_constraint\" ������ ����������� �������� ����� � ������������ ����� \"conkey\"");
          continue;
        }
        if (aColIdxs.Length != 1)
          continue; // ���� �� ���� �����������

        string constraintName = (string)(rowPGC[0]);
        string tableName = (string)(rowPGC[1]);
        if (tableName[0] == '\"') // ������� �������
          tableName = tableName.Substring(1, tableName.Length - 2);

        object oColIdx = aColIdxs.GetValue(0);
        int colIdx = DataTools.GetInt(oColIdx);
        int p = dvColumns.Find(new object[2] { tableName, colIdx });
        if (p < 0)
        {
          errors.AddWarning("� ��������� ������� \"pg_constraint\" ������ ����������� �������� ����� ��� ������� \"" + tableName + "\" � ������� � �������� " + colIdx.ToString() +
            ". ��� ���� �������+���� �� ������� ������ � ������� ��������. ������������ ����������� �� ���������������, �� �������� ������ ���������� ���������� �����������");
          continue;
        }
        string colName = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");

        tableForeignKeys.Rows.Add(constraintName, tableName, colName);
      }


      DataView dvForeignKeys = tableForeignKeys.DefaultView;
      dvForeignKeys.Sort = "TABLE_NAME,COLUMN_NAME";

      #endregion

      splash.PhaseText = "�������� ������� ������";
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

        // ������� ������� ����
        splash.PhaseText = "�������� �������� ����� ��� ������� \"" + table.TableName + "\", ������� \"" + column.ColumnName + "\"";

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
        errors.AddInfo("������ ������� ���� � ������� \"" + table.TableName + "\" ��� ������� \"" + column.ColumnName + "\"");
        modified = true;
      }

      return modified; // 28.12.2020
    }

    #endregion

    #region �������

    private bool UpdateDBStructIndices(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // ������� ������� � ������� ��������, �� ������� ���������� ��� � ��������� �������

      bool modified = false;

      splash.PhaseText = "�������� ��������";

      #region ��������� ������������ ������� � �������

      // ������� ���� ����� ��������� �� � ����� ������, � ��������, ����� ��������� ��������� ������� ���� ���
      // 12.05.2017
      // ������������ NpgsqlConnection.GetSchema() ��� �������� ������.
      // - GetSchema("Indexes") ���������� ������(?) ������ � ���������. ��� ����� ���� �� ������, ��:
      // - GetSchema("IndexColumns") �� ���������� ������� ������� � ������� � ������� �� �������������
      // ������� �������� � ������� �����.
      // ���� � �� ���� ������ � ���� �� ������, ��� � ���������, �� ������� ����� ����������,
      // ������ ������ ���� �������, � ������� �����
      // ����� ����, ����� ���� ������ ����������� �������
      // �������, ������ ��� ������, ����� ������ ��������

      DataTable tableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(tableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION"; // ��������� � 1

      // ���������� ������ ������.
      // ��. �������� ������ Npgsql (������ NpgsqlSchema.cs, ����� GetIndexColumns()) � �����������
      // �������� �������
      // 0 "tablename": - ��� �������
      // 1 "indexname" - ��� �������
      // 2 "indexcolumns" - ������ ������� �������� (��������� � 1), 0- ����������� ������
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

      // � ��������� ��������� ������ ��� "n.nspname not in ('pg_catalog', 'pg_toast')"
      // �������� ������, ����� ������ ��������� ����

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // ���� �� ��������
      foreach (DBxTableStruct table in DB.Struct.Tables)
      {
        if (!table.AutoCreate)
          continue;

        #region ������� ��� ������ ������ ��������

        // ������� ������� "������������������������������ - ������������"
        Dictionary<string, int> dict = new Dictionary<string, int>(table.Indexes.Count);
        for (int i = 0; i < table.Indexes.Count; i++)
          dict.Add(table.Indexes[i].Columns.AsString.ToUpperInvariant(), i);

        #endregion

        #region �������� ������������ �������� � �������� ��������

        // ��� ������� �������� ������� ��������������� ����, ���� ����� ������ ����
        bool[] indexFlags = new bool[table.Indexes.Count];

        // ������������ ������� ��� ������� �������
        tableIdx.DefaultView.RowFilter = new ValueFilter("tablename", table.TableName).ToString();

        //object[] a = TableIdx.Rows[1].ItemArray;

        foreach (DataRowView drvIdx in tableIdx.DefaultView)
        {
          string indexName = DataTools.GetString(drvIdx.Row, "indexname");

          // ��������� ����� �� ���������, �.�. �� ������������� � �������
          //if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          //  continue;

          #region ������� ������������� �������

          // ���� "indexname" ����� ��������� ��� int2vector
          // �.�., ��� � ��� ��������� ��������. Net Framework ������ �� ���� ������ ����"1 3 5"

          string sColIdxs = drvIdx.Row[2].ToString();
          string[] aColIdxs = sColIdxs.Split(' ');

          string[] colNames = new string[aColIdxs.Length];
          for (int j = 0; j < aColIdxs.Length; j++)
          {
            int colIdx = int.Parse(aColIdxs[j]);
            if (colIdx == 0)
            {
              // ������ � ���������� �� ������������
              colNames = null;
              break;
            }
            else
            {
              int p = dvColumns.Find(new object[] { table.TableName, colIdx });
              if (p < 0)
              {
                errors.AddError("��� ������� \"" + indexName + "\" ����� ������� � ������� " + colIdx.ToString() + ", �������� ��� � ������� \"" + table.TableName + "\"");
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
            indexFlags[indexIdx] = true; // ����� ������
          else if (options.DropUnusedIndices)
          {
            splash.PhaseText = "�������� ������� " + indexName;
            DropIndex(/*Table.TableName, */indexName);
            modified = true;
            errors.AddInfo("������ ������ \"" + indexName + "\" � ������� \"" + table.TableName + "\", �.�. �� �� ������������� ������������ � ��������� ������");
          }
        } // ���� �� ������������ ��������

        #endregion

        #region �������� ����� ��������

        for (int i = 0; i < table.Indexes.Count; i++)
        {
          if (!indexFlags[i])
          {
            CreateIndex(table.TableName, table.Indexes[i].Columns);
            modified = true;
            errors.AddInfo("�������� ������ ��� ������� \"" + table.TableName + "\", �� ����� " + table.Indexes[i].Columns.AsString);
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
    /// ���������� ������ �������
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

      SetPhaseText("�������� �������� ����� ������� \"" + TableDef.TableName + "\"");

#if DEBUG
      if (FieldDef.FieldType != DBStruct.AccDepFieldType.Reference)
        throw new InvalidOperationException("���� \"" + FieldDef.FieldName + "\" �� �������� ���������");

      // ���� ��� Primary ������� �� ������, �� ��������� ������ ������� � ������ � OLE DB
      if (String.IsNullOrEmpty(FieldDef.MasterTableName))
        throw new InvalidOperationException("��� ���� \"" + FieldDef.FieldName + "\" �� ������ ������-�������");
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
      Actions.Add("������ ������� ���� \"" + FKName + "\" � ������� \"" + TableDef.TableName + "\"");
    }
#endif

    private bool DeleteAllIndices(string tableName, ISplash splash, ErrorMessageList errors)
    {
      // ���������� ��� ������������ �������
      // ���� ������ ����� �������� ��������� �����
      // ������� ������ �������� ��� ��������
      List<string> indexNames = null;

      DataTable tableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, tableName });
      foreach (DataRow indexRow in tableIndexes.Rows)
      {
        string indexName = DataTools.GetString(indexRow, "INDEX_NAME");
        if (indexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          continue;

        // ��������� ������ � ������ �� ��������
        if (indexNames == null)
          indexNames = new List<string>();
        if (!indexNames.Contains(indexName))
          indexNames.Add(indexName);
      }

      if (indexNames == null)
        return false;

      splash.PhaseText = "�������� �������� ������� \"" + tableName + "\"";
      for (int i = 0; i < indexNames.Count; i++)
      {
        Buffer.Clear();
        Buffer.SB.Append("DROP INDEX \"");
        Buffer.SB.Append(indexNames[i]);
        Buffer.SB.Append("\"");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("������ ������ \"" + indexNames[i] + "\" � ������� \"" + tableName + "\"");
      }
      return true;
    }

    #endregion

    #endregion
  }

  internal class NpgsqlDBxManager : DBxManager
  {
    #region �����������

    private NpgsqlDBxManager()
      : base(DBxProviderNames.Npgsql)
    {
    }

    public static readonly NpgsqlDBxManager TheManager = new NpgsqlDBxManager();

    #endregion

    #region ���������������� ������

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
    /// ���������� ������ �� NpgsqlFactory
    /// </summary>
    public override DbProviderFactory ProviderFactory
    {
      get { return NpgsqlFactory.Instance; }
    }

    #endregion
  }
}
