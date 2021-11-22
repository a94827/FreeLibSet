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
      SyncRoot = new object();

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


    private readonly object SyncRoot;

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
    /// �������� ���������� ������� pg_database_size()
    /// </summary>
    /// <returns>������ ���� ������ � ������</returns>
    public override long GetDBSize()
    {
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, false))
      {
        return DataTools.GetInt64(Con.SQLExecuteScalar("SELECT pg_database_size( \'" + DatabaseName + "\' )"));
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
    /// ������� ���� ������, ���� �� �� ����������
    /// </summary>
    public override void CreateIfRequired()
    {
      if (DatabaseExists)
        return;
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, true))
      {
        Con.CommandTimeout = 0; // ����������� ����� ����������
        Con.CreateDatabase();
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
      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, false))
      {
        Con.CommandTimeout = 0; // ����������� ����� ����������
        return Con.UpdateDBStruct(splash, errors, options);
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

      using (NpgsqlDBxCon Con = new NpgsqlDBxCon(MainEntry, true))
      {
        Con.DropDatabase();
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
      DataTable Table = new DataTable(tableName);
      da.FillError += new FillErrorEventHandler(HandleDataAdapterFillError); // 03.10.2018
      da.Fill(Table);
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
        throw new BugException("������� ������������ ������������� ��� ����������� ������ � ������� \"" + tableName + "\" Id=" + Id.ToString());

      return Id;
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
      int SeqColumnIndex = Validator.GetPrimaryKeyInt32ColumnIndex(tableName, columnNames);
      if (SeqColumnIndex >= 0)
        CorrectPrimaryKeySequence(tableName, DataTools.GetInt(values[SeqColumnIndex]));
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

      DBxColumns ColumnNames = DBxColumns.FromColumns(table.Columns);
      Validator.CheckTableColumnNames(tableName, ColumnNames, false, DBxAccessMode.Full);
      DBxTableStruct ts = DB.Struct.Tables[tableName];

      #region ���������� ������ �����������

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
                Buffer.FormatValue(Row[i], ColDef.ColumnType); // ���� �����������
                Serializer.AddString(Buffer.SB.ToString());
                break;
              case CopyFormattingMode.String:
                s = DataTools.GetString(Row[i]);
                Serializer.AddString(s); // ����� �� �����������
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
    private Dictionary<string, string> TablePKSequenceNames;


    /// <summary>
    /// "������������" �������������� ��� ������������������
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

      // ��� ������� ���� � � ���������� � � ��������
      // � ��� ������� - ������ � ����������
      // �.�. ������
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
      DataTable Table = Connection.GetSchema("Tables", new string[] { DB.DatabaseName, "public" });
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

      DataTable Table = Connection.GetSchema("Columns", new string[] { DB.DatabaseName, "public", tableName });
      Table.DefaultView.Sort = "ordinal_position"; // ����������� �� �������, ����� �������� ���� ����� �� ������

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
            // TODO: ������������ ����� ���� ��� ���������� float/double
            ColStr.MinValue = Double.MinValue;
            ColStr.MaxValue = Double.MaxValue;
            break;
          case "real":
          case "float8":
            ColStr.ColumnType = DBxColumnType.Float;
            // TODO: ������������ ����� ���� ��� ���������� float/double
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

          case "uuid": // 06.10.2021
            ColStr.ColumnType = DBxColumnType.Guid;
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

      #region ����������� ��������� ����� (����������� FOREIGN KEY)

      //DataTable Table2 = Connection.GetSchema();
      //string[]aaa=DataTools.GetStringsFromField(Table2, "CollectionName");

      //Table = Connection.GetSchema("Restrictions", new string[] { DB.DatabaseName, "public", TableName });
      //foreach (DataRow r in Table.Rows)
      //{ 
      //}


      int TableOID = GetTableOID(tableName);
      if (TableOID == 0)
        throw new BugException("�� ������� �������� ������������� object_id ������� \"" + tableName + "\"");

      Buffer.Clear();
      Buffer.SB.Append(@"SELECT confrelid,confdeltype,conkey FROM pg_catalog.pg_constraint WHERE contype='f' AND conrelid=");
      Buffer.FormatValue(TableOID, DBxColumnType.Int);
      DataTable tbl = SQLExecuteDataTable(Buffer.SB.ToString(), "pg_constraint");
      foreach (DataRow Row in tbl.Rows)
      {
        Int32 RefTableOID = DataTools.GetInt(Row, "confrelid");
        string RefTableName = GetTableNameFromOID(RefTableOID);
        if (String.IsNullOrEmpty(RefTableName))
          throw new BugException("�� ������� ��� ��� ������-������� � OID=" + RefTableOID);

        Int16[] DetColPoss = (Int16[])(Row["conkey"]);
        // �� ����� Int16[] RefColPoss = (Int16[])(Row["confkey"]);
        if (DetColPoss.Length != 1 /*|| RefColPoss.Length != 1*/)
          continue; // FOREIGN KEY �� ���������� ����� �� ��������������

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
      bool Modified = false;

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

    #region ���������� ������, �������� � ��������� ������

    private bool UpdateDBStructTables(ISplash splash, ErrorMessageList errors)
    {
      bool Modified = false;

      #region ���������� ���������� �� ������������ ����� ������

      DataTable TableTables = Connection.GetSchema("Tables");
      DataView dvTables = new DataView(TableTables);
      dvTables.Sort = "TABLE_NAME";

      // �������
      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,COLUMN_NAME"; // ����� ����� �������

      // ������ �� �������� 
      DataTable TableIndexColumns = Connection.GetSchema("IndexColumns");
      DataView dvIndexColumns = new DataView(TableIndexColumns);
      dvIndexColumns.Sort = "TABLE_NAME,INDEX_NAME";
      //DebugTools.DebugDataView(dvIndexColumns, "������� �������� ���� ������ "+DB.DisplayName);

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

        if (dvTables.Find(Table.TableName) < 0)
        {
          #region ��������� ������ �������� �������

          splash.PhaseText = "��������� ������� \"" + Table.TableName + "\"";
          CreateTable(Table);
          errors.AddInfo("������� ������� \"" + Table.TableName + "\"");
          Modified = true;

          #endregion
        }
        else
        {
          #region ��������� ������������ ���������� �����

          if (CorrectPrimaryKey(Table, dvIndexColumns, errors))
            Modified = true;

          #endregion

          #region ��������� ������� ����������� �����

          DBxSqlBuffer Buffer2 = new DBxSqlBuffer(Buffer.Formatter);

          foreach (DBxColumnStruct Column in Table.Columns)
          {
            int ColumnRowIndex = dvColumns.Find(new object[] { Table.TableName, Column.ColumnName });
            if (ColumnRowIndex < 0)
            {
              // ���� �� ����������
              splash.PhaseText = "���������� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName + "\"";
              Buffer.Clear();
              Buffer.SB.Append("ALTER TABLE ");
              Buffer.FormatTableName(Table.TableName);
              Buffer.SB.Append(" ADD "); // � �� ADD COLUMN
              AppendColumnDef(/*Table, */Column, false, true);
              SQLExecuteNonQuery(Buffer.SB.ToString());
              errors.AddInfo("������� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName + "\"");
              Modified = true;
            }
            else
            {
              // ��������� ������������ ����
              DataRow ColumnRow = dvColumns[ColumnRowIndex].Row;
              // ��������� ������������ ���� ������� ����������
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
                errors.AddError("�������������� ���� ���� \"" + Column.ColumnName + "\" ������� \"" +
                    Table.TableName + "\". ���������� ���� ���� " + Column.ColumnType.ToString() +
                    " ������������ ��� " + WantedType +
                    " � �� ����� ��� �������� ��� ���� " + RealType);
              }
              else
              {
                #region �������� ����� ���������� ����

                if (Column.ColumnType == DBxColumnType.String)
                {
                  int RealLen = DataTools.GetInt(ColumnRow, "CHARACTER_MAXIMUM_LENGTH");
                  if (RealLen != Column.MaxLength)
                  {
                    if (RealLen > Column.MaxLength)
                    {
                      // !!! ��������, ������ �� ��������� ����
                      errors.AddWarning("���� \"" + Column.ColumnName + "\" ������� \"" +
                          Table.TableName + "\" ������ ����� ����� " + Column.MaxLength.ToString() +
                          " ��������, � �� �����, ��� �������� ���� �������:  " + RealLen.ToString() + " ��������");
                      //DisallowFieldChange = true;
                    }
                    else
                    {
                      // ����� ����������� ��� �������
                      errors.AddInfo("��� ������������ ������� ������� \"" + Table.TableName + "\" ����� ������� ��-�� ��������� ������� ���� \"" + Column.ColumnName + "\"");
                      if (DeleteAllIndices(Table.TableName, splash, errors))
                        Modified = true;

                      // ����������� ����� ����
                      splash.PhaseText = "��������� ����� ���� \"" + Column.ColumnName + "\" � ������� \"" + Table.TableName + "\"";
                      AlterColumn(Table, Column, true, false);
                      errors.AddInfo("����� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName +
                        "\" ��������� � " + RealLen.ToString() + " �� " + Column.MaxLength.ToString() + " ��������");
                      Modified = true;
                    }
                  }
                } // ��������� ����

                #endregion

                #region �������� Default

                // ��������� �������� DEFAULT ������ ����������� �� ��������� NOT NULL, ����� ��������� ������

                string WantedDefExpr = String.Empty;
                if (Column.DefaultExpression != null)
                {
                  Buffer2.Clear();
                  Buffer2.FormatExpression(Column.DefaultExpression, new DBxFormatExpressionInfo());
                  WantedDefExpr = Buffer2.SB.ToString();
                }
                string RealDefExpr = DataTools.GetString(ColumnRow, "COLUMN_DEFAULT");
                if (RealDefExpr.StartsWith("nextval(", StringComparison.OrdinalIgnoreCase))
                  RealDefExpr = String.Empty; // ����� ������� ��������� ����
                if (RealDefExpr != WantedDefExpr)
                {
                  // ����� ������� ������� ������ �������, ����� ��������� �����
                  if (RealDefExpr.Length > 0)
                  {
                    Buffer.Clear();
                    Buffer.SB.Append("ALTER TABLE ");
                    Buffer.FormatTableName(Table.TableName);
                    Buffer.SB.Append(" ALTER COLUMN ");
                    Buffer.FormatColumnName(Column.ColumnName);
                    Buffer.SB.Append(" DROP DEFAULT");

                    SQLExecuteNonQuery(Buffer.SB.ToString());
                    errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName +
                        "\" ������ ������� DEFAULT");
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

                    errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName +
                      "\" ���������� ������� DEFAULT " + WantedDefExpr);
                  }
                  Modified = true;
                }

                #endregion

                #region �������� �������� Nullable

                // ��������� Nullable
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

                  #region ������ NULL'�� �� DEFAULT

                  if ((!Column.Nullable) && Column.DefaultExpression != null && (!IsTableEmpty(Table.TableName)))
                  {
                    //SetValue(Table.TableName, new ValueFilter(Column.ColumnName, null, CompareKind.Equal, Column.ColumnType),Column.ColumnName, Column.Default)
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

                  #endregion

                  // ������ ���� NULLABLE
                  AlterColumn(Table, Column, false, true);
                  errors.AddInfo("��� ���� \"" + Column.ColumnName + "\"� ������� \"" + Table.TableName +
                    "\" ���������� ������� " + (Column.Nullable ? "\"NULL\"" : "\"NOT NULL\""));
                  Modified = true;
                }

                #endregion
              }
            } // ���� ����������
          } // ���� �� ��������

          #endregion
        }

        // ������� ����������

        #region �����������

        SetTableComment(Table.TableName, Table.Comment);
        for (int i = 0; i < Table.Columns.Count; i++)
          SetColumnComment(Table.TableName, Table.Columns[i].ColumnName, Table.Columns[i].Comment);


        #endregion

        splash.PhaseText = String.Empty;
        splash.IncPercent();
      } // ���� �� ��������

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
      d.Add("BPCHAR", "CHAR"); // ���������� 20.09.2019
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

      bool Modified = false;
      bool Found = false;
      string PKName = table.TableName + "_pkey";

      dvIndexColumns.RowFilter = "TABLE_NAME='" + table.TableName + "' AND INDEX_NAME='" + PKName + "'";
      foreach (DataRowView drvCol in dvIndexColumns)
      {
        string ColumnName = DataTools.GetString(drvCol.Row, "COLUMN_NAME");
        if (String.Equals(ColumnName, table.PrimaryKey[0], StringComparison.OrdinalIgnoreCase))
          Found = true;
        else
        {
          // 13.10.2021.
          // ����������� ����� ����� ��� � ����� ��������.
          // ALTER TABLE DROP CONSTRAINT ������� �������� ����� ����������� � ������ ��������.
          // ����� ���, ��� ��� �������� � ���� ������.
          string PKName2 = DataTools.GetString(drvCol.Row, "INDEX_NAME"); 

          //if (IndexName.StartsWith("Index"))
          //  continue; // ��������� ���������������� ������, � ������� ������ ���� "Id"
          SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" DROP CONSTRAINT \"" + PKName2 + "\"");
          errors.AddInfo("������� ������������ ����������� ���������� ����� \"" + PKName2 + "\" � ������� \"" + table.TableName + "\"");
          Modified = true;
          break;
        }
      }

      if (!Found)
      {
        SQLExecuteNonQuery("ALTER TABLE \"" + table.TableName + "\" ADD CONSTRAINT \"" + PKName + "\" PRIMARY KEY (\"" + table.PrimaryKey[0] + "\")");
        errors.AddInfo("��������� ����������� ���������� ����� \"" + PKName + "\" � ������� \"" + table.TableName + "\"");
        Modified = true;
      }

      return Modified;
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
      bool Modified = false;

      #region ������� TableForeignKeys

      // ������� ����� ����� ��������� ������ ����� �������� ���� ������
      // ��� ������ �������� ������� ��� �����������
      //// ������ �� ������� ������
      //DataTable TableForeignKeys = Connection.GetSchema("ForeignKeys");
      //DataView dvForeignKeys = new DataView(TableForeignKeys);
      //dvForeignKeys.Sort = "CONSTRAINT_NAME";

      // �������
      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION";

      // ���������� ��������� ������� pg_constraint
      // 0: conname - ��� ��������� (��� ��������, ���� ���� �����)
      // 1: conrelid::regclass - ��� ������� � ��������
      // 2: conkey - ������ �������� �������� ��������� ����� (��� ���������� ������ ����������� �� ������ ��������)
      // contype='f' - ������ �� ������������ �������� �����
      DataTable TablePGC = DoSQLExecuteDataTable(@"SELECT conname, conrelid::regclass, conkey FROM pg_catalog.pg_constraint WHERE contype='f'", "pg_constraint", null);
      // �������� ������
      DataTable TableForeignKeys = new DataTable();
      TableForeignKeys.Columns.Add("CONSTRAINT_NAME", typeof(string));
      TableForeignKeys.Columns.Add("TABLE_NAME", typeof(string));
      TableForeignKeys.Columns.Add("COLUMN_NAME", typeof(string));
      foreach (DataRow RowPGC in TablePGC.Rows)
      {
        Array aColIdxs = RowPGC[2] as Array;
        if (aColIdxs == null)
        {
          errors.AddWarning("� ��������� ������� \"pg_constraint\" ������ ����������� �������� ����� � ������������ ����� \"conkey\"");
          continue;
        }
        if (aColIdxs.Length != 1)
          continue; // ���� �� ���� �����������

        string ConstraintName = (string)(RowPGC[0]);
        string TableName = (string)(RowPGC[1]);
        if (TableName[0] == '\"') // ������� �������
          TableName = TableName.Substring(1, TableName.Length - 2);

        object oColIdx = aColIdxs.GetValue(0);
        int ColIdx = DataTools.GetInt(oColIdx);
        int p = dvColumns.Find(new object[2] { TableName, ColIdx });
        if (p < 0)
        {
          errors.AddWarning("� ��������� ������� \"pg_constraint\" ������ ����������� �������� ����� ��� ������� \"" + TableName + "\" � ������� � �������� " + ColIdx.ToString() +
            ". ��� ���� �������+���� �� ������� ������ � ������� ��������. ������������ ����������� �� ���������������, �� �������� ������ ���������� ���������� �����������");
          continue;
        }
        string ColName = DataTools.GetString(dvColumns[p].Row, "COLUMN_NAME");

        TableForeignKeys.Rows.Add(ConstraintName, TableName, ColName);
      }


      DataView dvForeignKeys = TableForeignKeys.DefaultView;
      dvForeignKeys.Sort = "TABLE_NAME,COLUMN_NAME";

      #endregion

      splash.PhaseText = "�������� ������� ������";
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

        // ������� ������� ����
        splash.PhaseText = "�������� �������� ����� ��� ������� \"" + table.TableName + "\", ������� \"" + Column.ColumnName + "\"";

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
        errors.AddInfo("������ ������� ���� � ������� \"" + table.TableName + "\" ��� ������� \"" + Column.ColumnName + "\"");
        Modified = true;
      }

      return Modified; // 28.12.2020
    }

    #endregion

    #region �������

    private bool UpdateDBStructIndices(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      // ������� ������� � ������� ��������, �� ������� ���������� ��� � ��������� �������

      bool Modified = false;

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

      DataTable TableColumns = Connection.GetSchema("Columns");
      DataView dvColumns = new DataView(TableColumns);
      dvColumns.Sort = "TABLE_NAME,ORDINAL_POSITION"; // ��������� � 1

      // ���������� ������ ������.
      // ��. �������� ������ Npgsql (������ NpgsqlSchema.cs, ����� GetIndexColumns()) � �����������
      // �������� �������
      // 0 "tablename": - ��� �������
      // 1 "indexname" - ��� �������
      // 2 "indexcolumns" - ������ ������� �������� (��������� � 1), 0- ����������� ������
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

      // � ��������� ��������� ������ ��� "n.nspname not in ('pg_catalog', 'pg_toast')"
      // �������� ������, ����� ������ ��������� ����

      #endregion

      splash.PercentMax = DB.Struct.Tables.Count;
      // ���� �� ��������
      foreach (DBxTableStruct Table in DB.Struct.Tables)
      {
        if (!Table.AutoCreate)
          continue;

        #region ������� ��� ������ ������ ��������

        // ������� ������� "������������������������������ - ������������"
        Dictionary<string, int> Dict = new Dictionary<string, int>(Table.Indexes.Count);
        for (int i = 0; i < Table.Indexes.Count; i++)
          Dict.Add(Table.Indexes[i].Columns.AsString.ToUpperInvariant(), i);


        #endregion

        #region �������� ������������ �������� � �������� ��������

        // ��� ������� �������� ������� ��������������� ����, ���� ����� ������ ����
        bool[] IndexFlags = new bool[Table.Indexes.Count];

        // ������������ ������� ��� ������� �������
        TableIdx.DefaultView.RowFilter = new ValueFilter("tablename", Table.TableName).ToString();

        //object[] a = TableIdx.Rows[1].ItemArray;

        foreach (DataRowView drvIdx in TableIdx.DefaultView)
        {
          string IndexName = DataTools.GetString(drvIdx.Row, "indexname");

          // ��������� ����� �� ���������, �.�. �� ������������� � �������
          //if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          //  continue;

          #region ������� ������������� �������

          // ���� "indexname" ����� ��������� ��� int2vector
          // �.�., ��� � ��� ��������� ��������. Net Framework ������ �� ���� ������ ����"1 3 5"

          string sColIdxs = drvIdx.Row[2].ToString();
          string[] aColIdxs = sColIdxs.Split(' ');

          string[] ColNames = new string[aColIdxs.Length];
          for (int j = 0; j < aColIdxs.Length; j++)
          {
            int ColIdx = int.Parse(aColIdxs[j]);
            if (ColIdx == 0)
            {
              // ������ � ���������� �� ������������
              ColNames = null;
              break;
            }
            else
            {
              int p = dvColumns.Find(new object[] { Table.TableName, ColIdx });
              if (p < 0)
              {
                errors.AddError("��� ������� \"" + IndexName + "\" ����� ������� � ������� " + ColIdx.ToString() + ", �������� ��� � ������� \"" + Table.TableName + "\"");
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
            IndexFlags[IndexIdx] = true; // ����� ������
          else if (options.DropUnusedIndices)
          {
            splash.PhaseText = "�������� ������� " + IndexName;
            DropIndex(/*Table.TableName, */IndexName);
            Modified = true;
            errors.AddInfo("������ ������ \"" + IndexName + "\" � ������� \"" + Table.TableName + "\", �.�. �� �� ������������� ������������ � ��������� ������");
          }
        } // ���� �� ������������ ��������

        #endregion

        #region �������� ����� ��������

        for (int i = 0; i < Table.Indexes.Count; i++)
        {
          if (!IndexFlags[i])
          {
            CreateIndex(Table.TableName, Table.Indexes[i].Columns);
            Modified = true;
            errors.AddInfo("�������� ������ ��� ������� \"" + Table.TableName + "\", �� ����� " + Table.Indexes[i].Columns.AsString);
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
      List<string> IndexNames = null;

      DataTable TableIndexes = Connection.GetSchema("Indexes", new string[] { null, null, tableName });
      foreach (DataRow IndexRow in TableIndexes.Rows)
      {
        string IndexName = DataTools.GetString(IndexRow, "INDEX_NAME");
        if (IndexName.EndsWith("_PKEY", StringComparison.OrdinalIgnoreCase))
          continue;

        // ��������� ������ � ������ �� ��������
        if (IndexNames == null)
          IndexNames = new List<string>();
        if (!IndexNames.Contains(IndexName))
          IndexNames.Add(IndexName);
      }

      if (IndexNames == null)
        return false;

      splash.PhaseText = "�������� �������� ������� \"" + tableName + "\"";
      for (int i = 0; i < IndexNames.Count; i++)
      {
        Buffer.Clear();
        Buffer.SB.Append("DROP INDEX \"");
        Buffer.SB.Append(IndexNames[i]);
        Buffer.SB.Append("\"");

        SQLExecuteNonQuery(Buffer.SB.ToString());
        errors.AddInfo("������ ������ \"" + IndexNames[i] + "\" � ������� \"" + tableName + "\"");
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
