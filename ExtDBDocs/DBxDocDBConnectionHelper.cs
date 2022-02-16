// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using FreeLibSet.Core;
using System.Data.Common;

namespace FreeLibSet.Data.Docs
{
  #region InitDBxEventHandler

  /// <summary>
  /// ��������� ������� DBxDocDBConnectionHelper.BeforeInitDB
  /// </summary>
  public sealed class DBxDocDBConnectionHelperInitDBxEventArgs : EventArgs
  {
    #region �����������

    /// <summary>
    /// ����������� ���������� �� DBxDocDBConnectionHelper.InitDB()
    /// </summary>
    internal DBxDocDBConnectionHelperInitDBxEventArgs(DBx db, DBxStruct dbStruct, string dbName)
    {
#if DEBUG
      if (db == null)
        throw new ArgumentNullException("db");
      if (dbStruct == null)
        throw new ArgumentNullException("dbStruct");
      if (dbStruct.IsReadOnly)
        throw new ObjectReadOnlyException("DBxStruct.IsReadOnly=true");
      if (String.IsNullOrEmpty(dbName))
        throw new ArgumentNullException("dbName");
#endif

      _DB = db;
      _Struct = dbStruct;
      _DBName = dbName;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������� ���� ������.
    /// �������� DBx.Struct �� �����������
    /// </summary>
    public DBx DB { get { return _DB; } }
    private DBx _DB;

    /// <summary>
    /// ��������� ���� ������.
    /// ���������������� ��� ����� �������� �������� ������ � �����.
    /// </summary>
    public DBxStruct Struct { get { return _Struct; } }
    private DBxStruct _Struct;

    /// <summary>
    /// ��� ���� ������, ���������� ��� ������ InitDB() ("db", "undo", "files", "files2", ...)
    /// </summary>
    public string DBName { get { return _DBName; } }
    private string _DBName;

    #endregion
  }

  /// <summary>
  /// ������� ������� DBxDocDBConnectionHelper.BeforeInitDB
  /// </summary>
  /// <param name="sender">������ DBxDocDBConnectionHelper</param>
  /// <param name="args">��������� �������</param>
  public delegate void DBxDocDBConnectionHelperInitDBxEventHandler(object sender,
    DBxDocDBConnectionHelperInitDBxEventArgs args);

  #endregion

  /// <summary>
  /// ��������������� ����� ��� �������� �������� DBx ��� ��������� ������ � �����������.
  /// ����� �������� ���������� � ��������� ����������� �������, ������� ������� ����� CreateRealDocProviderGlobal().
  /// </summary>
  public class DBxDocDBConnectionHelper
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public DBxDocDBConnectionHelper()
    {
      _CommandTimeout = 30;
      _InitDocTableIds = true;
      _Errors = new ErrorMessageList();
      _UpdateStructOptions = new DBxUpdateStructOptions();
    }

    #endregion

    #region ����������� ��������� ��� �������� ��

    /// <summary>
    /// ��� ���������� ���� ������.
    /// </summary>
    public string ProviderName { get { return _ProviderName; } set { _ProviderName = value; } }
    private string _ProviderName;

    /// <summary>
    /// ������ ���������� � �������� ����� ������
    /// </summary>
    public string ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; } }
    private string _ConnectionString;

#if UNUSED

    // ������������� ConnectionStringSettings ������� ����������� System.Configuration.dll,
    // ��� ���� �� ���������.
    // ���� ���������� ����� ������ ������ �� app.config, ����� ��������� ������������� �������
    // ProviderName � ConnectionString �������

    /// <summary>
    /// ��� ���������� ���� ������ � ������ �����������.
    /// ������ ����� ���� �������� �� ����������������� ����� ����������.
    /// ������ ������ ����������� ��� �������� ���� ������ ("db"). ����������� � ��������� �����
    /// ������������ �������������. � ���� ������ ����������� ��� ���� ������ ����� ��������� 
    /// ������������ �������, ������� ������������ � ��� ������ ��� ������. ��������, ���� � 
    /// ������ ����������� ������ "DATABASE=myprog_db", �� ��� ���� ������ undo ����� ������������
    /// "DATABASE=myprog_undo"
    /// ���� �������� �� ����������� (�� ���������),
    /// �� ������������ ��������� Microsoft SQL Server Express � ����������� � DBDir.
    /// </summary>
    /// <remarks>
    /// ��� �������� �� �������� � ����� ����. ������ ����� ��� ���������� �� �������
    /// ProviderName � ConnectionString.
    /// ��� ������ ������ �������� ��������� ����� ������
    /// </remarks>
    public System.Configuration.ConnectionStringSettings ConnectionStringSettings
    {
      get 
      {
        if (String.IsNullOrEmpty(ConnectionString))
          return null;
        return new System.Configuration.ConnectionStringSettings("db", ConnectionString, ProviderName); 
      }
      set 
      {
        if (value == null)
        {
          ProviderName = String.Empty;
          ConnectionString = String.Empty;
        }
        else
        {
          ProviderName = value.ProviderName;
          ConnectionString = value.ConnectionString;
        }
      }
    }
#endif

    /// <summary>
    /// ������� ��� ���������� ��� ������ MS SQL Server Express.
    /// �������� ������������, ���� �� ������ ConnectionStringSettings
    /// </summary>
    public AbsPath DBDir { get { return _DBDir; } set { _DBDir = value; } }
    private AbsPath _DBDir;

    /// <summary>
    /// �����, ���������� �� ���������� SQL-������� � ��������.
    /// �� ��������� - 30�.
    /// 0 - ����������� ����� ����������
    /// </summary>
    public int CommandTimeout { get { return _CommandTimeout; } set { _CommandTimeout = value; } }
    private int _CommandTimeout;

    /// <summary>
    /// ���������� ��������� �� ����� ��������/���������� ��������� ���� ������.
    /// ����� ���� �� ������
    /// </summary>
    public ISplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISplash _Splash;

    /// <summary>
    /// ���� ����������� ��������� �� ������� ��� ���������� ��������� ���� ������.
    /// ����� ����������� ��������� ��� ������, ��������� ����� ��������� ������ ��������� � log-���� ��� ��������� ��������������
    /// </summary>
    public ErrorMessageList Errors { get { return _Errors; } }
    private ErrorMessageList _Errors;

    /// <summary>
    /// ��������� ���������� ��������� ��� ������.
    /// </summary>
    public DBxUpdateStructOptions UpdateStructOptions { get { return _UpdateStructOptions; } }
    private DBxUpdateStructOptions _UpdateStructOptions;

    #endregion

    #region �������

    /// <summary>
    /// ������� ���������� ������� InitDB().
    /// ���������� ������� ����� ��������� �������������� ������������� ������� DBx ����� ��� ��������������.
    /// �� ������ ������ ����������� �������� DBx.DisplayName � CommandTimeout,
    /// �� �������� DBStruct �� �����������. 
    /// �������� ������������� ��������� ���� ������ �� ���������.
    /// </summary>
    public event DBxDocDBConnectionHelperInitDBxEventHandler BeforeInitDB;

    /// <summary>
    /// ����� ������� BeforeInitDB
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected void OnBeforeInitDB(DBxDocDBConnectionHelperInitDBxEventArgs args)
    {
      if (BeforeInitDB != null)
        BeforeInitDB(this, args);
    }

    #endregion

    #region �������� ������������ �����������

    /// <summary>
    /// �������� ������������ ���� ������ � �������� ������ �� ��������� ����������� ����������.
    /// ���� ���� ������ �� ����������, ��� ���������. ��������� ���� ������ ����������� ���
    /// ���������� ����������� ��������� ���������, ���� ��� ������
    /// </summary>
    /// <param name="dbName">��� ���� ������, ��������, "db", "undo"</param>
    /// <param name="dbStruct">��������� ���� ������. ���� null, �� �������� ���� ������ � ���������� ��������� �� �����������</param>
    /// <returns>������ ����������� � ���� ������</returns>
    public DBx InitDB(string dbName, DBxStruct dbStruct)
    {
      #region �������� ����������

      if (String.IsNullOrEmpty(dbName))
        throw new ArgumentNullException("dbName");
      // if (DBStruct == null)
      //   throw new ArgumentNullException("DBStruct");

      #endregion

      DBx DB;

      string OldSplashText = null;
      if (Splash != null)
      {
        OldSplashText = Splash.PhaseText;
        Splash.PhaseText = "���������� ���� ������ " + dbName;
      }
      try
      {
        if (String.IsNullOrEmpty(ConnectionString))
        {
          // TODO: ���� ProviderName ��� ������ ������� ���� ������

          if (DBDir.IsEmpty)
            throw new NullReferenceException("�� ����������� �������� DBDir");

          // ������ ����������� �� ������
          System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
          csb.DataSource = @".\SQLEXPRESS";
          csb.AttachDBFilename = DBDir.SlashedPath + dbName + ".mdf";
          csb.IntegratedSecurity = true;

          DB = new FreeLibSet.Data.SqlClient.SqlDBx(csb);
        }
        else
        {
          if (String.IsNullOrEmpty(ProviderName))
            throw new NullReferenceException("�� ����������� �������� \"ProviderName\"");
          DBxManager dbMan = DBxManager.Managers[ProviderName];
          if (dbMan == null)
          {
            if (DBxManager.Managers.Count == 0)
              throw new NullReferenceException("������ ���������� ��� ������ DBxManager.Managers ����");
            else
              throw new NullReferenceException("�������� ProviderName �������� ����������� ��������� \"" + ProviderName + "\". ���������� ��������: " +
                String.Join(", ", DBxManager.Managers.GetCodes()));
          }
          string ConStr = dbMan.ReplaceDBName(ConnectionString, "db", dbName);

          if (ProviderName == DBxProviderNames.SQLite && (!DocTypes.UseDeleted)) // 02.02.2022
          {
            DbConnectionStringBuilder csb = dbMan.CreateConnectionStringBuilder(ConStr);
            csb["foreign keys"] = true;
            ConStr = csb.ConnectionString;
          }

          DB = dbMan.CreateDBObject(ConStr);
        }

        DB.DisplayName = dbName;
        DB.CommandTimeout = CommandTimeout;


        if (dbStruct != null)
        {
          // 08.10.2018
          // ���������������� �������������
          DBxDocDBConnectionHelperInitDBxEventArgs Args = new DBxDocDBConnectionHelperInitDBxEventArgs(DB, dbStruct, dbName);
          OnBeforeInitDB(Args);

          DB.CreateIfRequired();
          DB.Struct = dbStruct; // ��������, ���� �������� ���������������� ������������

          ISplash spl = this.Splash ?? new DummySplash();
          int OldErrorCount = Errors.Count;
          DB.UpdateStruct(spl, Errors, UpdateStructOptions);
          Errors.SetPrefix("���������� ��������� " + dbName + ". ", OldErrorCount);
        }
      }
      finally
      {
        if (Splash != null)
          Splash.PhaseText = OldSplashText;
      }
      return DB;
    }

    /// <summary>
    /// ���������� true, ���� ���� ������ � �������� ������ ����������
    /// </summary>
    /// <param name="dbName">��� ���� ������</param>
    /// <returns>������������� ���� ������</returns>
    public bool DatabaseExists(string dbName)
    {
      using (DBx DB = InitDB(dbName, null))
      {
        return DB.DatabaseExists;
      }
    }

    #endregion

    #region �������� DBxRealDocProviderGlobal

    /// <summary>
    /// ���� ������ ���� ������������ ����������� ��������� � ������������ ����������.
    /// ��������� �������� �������� � ������������� ������� MainDBStruct � UndoDBStruct
    /// </summary>
    public DBxDocTypes DocTypes
    {
      get { return _DocTypes; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value.Count == 0)
          throw new ArgumentException("������ ���������� �� ��������");
        if (_DocTypes != null)
          throw new InvalidOperationException("��������� ��������� ��������");
        value.SetReadOnly();
        _DocTypes = value;

        _MainDBStruct = value.GetMainDBStruct();
        _UndoDBStruct = value.GetUndoDBStruct();
      }
    }
    private DBxDocTypes _DocTypes;

    /// <summary>
    /// ������ �� ����������� ���� ������ �������.
    /// ���� �������� �� ������ � ����� ����, �������� ������������ �������������, ������
    /// �� ������� DBxDocTypes
    /// </summary>
    public bool UseUndo
    {
      get
      {
        if (_UseUndo.HasValue)
          return _UseUndo.Value;
        else if (DocTypes == null)
          return false;
        else
          return DocTypes.UseVersions && DocTypes.UseTime && DocTypes.UseDeleted;
      }
      set
      {
        _UseUndo = value;
      }
    }
    private bool? _UseUndo;

    /// <summary>
    /// ���������� ������������� � DBxBinDataHandler.
    /// ���� null (�� ���������, �� ������������ �������������)
    /// </summary>
    public bool? UseBinDataFragmentation
    {
      get { return _UseBinDataFragmentation; }
      set { _UseBinDataFragmentation = value; }
    }
    private bool? _UseBinDataFragmentation;


    /// <summary>
    /// ��������� �������� ���� ������.
    /// �������� ���������������� ����� ��������� DocTypes.
    /// ���������� ��� ����� �������� ����������� �������, �� ��������� � �����������
    /// </summary>
    public DBxStruct MainDBStruct { get { return _MainDBStruct; } }
    private DBxStruct _MainDBStruct;

    /// <summary>
    /// ��������� ���� ������ �������.
    /// �������� ���������������� ����� ��������� DocTypes.
    /// ����� ���� null, ���� ������� �� ����� �������
    /// </summary>
    public DBxStruct UndoDBStruct { get { return _UndoDBStruct; } }
    private DBxStruct _UndoDBStruct;

    /// <summary>
    /// ����������� �������� ��� CreateRealDocProviderGlobal().
    /// ���� true (�� ���������), �� ����� �������� ����� DBxDocTypes.InitDocTableIds()
    /// </summary>
    public bool InitDocTableIds { get { return _InitDocTableIds; } set { _InitDocTableIds = value; } }
    private bool _InitDocTableIds;

    /// <summary>
    /// ������� �������� ������ ��� ������� � ����������.
    /// �������������� ��������� �������:
    /// - BinDataHandler, ���� ���� �������� ������ �/��� �������� �����
    /// - ���� ������ DBx "db", "undo" � "files" (��� �������������)
    /// </summary>
    /// <returns>����� DBxRealDocProviderGlobal</returns>
    public DBxRealDocProviderGlobal CreateRealDocProviderGlobal()
    {
      if (DocTypes == null)
        throw new NullReferenceException("�������� DocTypes �� �����������");

      DBxBinDataHandler binDataHandler;
      DBxStruct filesDBStruct; // ��������� ��� ������ files. ������������, ���� DBxBinDataHandler.UseFragmentation=true.
      InitBinDataHandler(out binDataHandler, out filesDBStruct);

      DBx MainDB = InitDB("db", MainDBStruct);
      DBx UndoDB = null;
      if (UseUndo)
      {
        if (UndoDBStruct == null)
          throw new InvalidOperationException("��� �������� ���������� ������� DBxDocTypes, ������� ������� ����������");
        UndoDB = InitDB("undo", UndoDBStruct);
      }

      if (binDataHandler != null)
      {
        binDataHandler.MainEntry = MainDB.MainEntry;
        if (filesDBStruct != null)
        {
          DBx filesDB = InitDB("files", filesDBStruct.Clone());
          binDataHandler.AddSectionEntry(filesDB.MainEntry);

          // ���������� �������������� ���� ������
          for (int i = 2; i <= int.MaxValue; i++)
          {
            if (DatabaseExists("files" + i.ToString()))
            {
              filesDB = InitDB("files" + i.ToString(), filesDBStruct.Clone());
              binDataHandler.AddSectionEntry(filesDB.MainEntry);
            }
            else
              break;
          }

          binDataHandler.CheckBinDataSections();
          binDataHandler.DBSizeLimitExceeded += new EventHandler(BinDataHandler_DBSizeLimitExceeded);
        }
      }


      if (InitDocTableIds)
        DocTypes.InitDocTableIds(MainDB.MainEntry);

      this.Splash = null; // ������ ����� �������� "������������" � DBxBinDataHelper

      DBxEntry MainDBEntry = MainDB.MainEntry;
      DBxEntry UndoDBEntry = null;
      if (UndoDB != null)
        UndoDBEntry = UndoDB.MainEntry;
      return new DBxRealDocProviderGlobal(DocTypes, MainDBEntry, UndoDBEntry, binDataHandler);
    }

    private void InitBinDataHandler(out DBxBinDataHandler binDataHandler, out DBxStruct filesDBStruct)
    {
      binDataHandler = null;
      filesDBStruct = null;

      bool useBinData = DocTypes.HasBinDataRefs;
      bool useFiles = DocTypes.HasFileRefs;

      if (!(useBinData || useFiles))
        return;

      binDataHandler = new DBxBinDataHandler();
      binDataHandler.UseBinData = useBinData;
      binDataHandler.UseFiles = useFiles;

      if (_UseBinDataFragmentation.HasValue)
        binDataHandler.UseFragmentation = _UseBinDataFragmentation.Value;
      else
      {
        using (DBx DummyDB = InitDB("db", null))
        {
          binDataHandler.UseFragmentation = DummyDB.DBSizeLimit < FileTools.TByte;
        }
      }

      binDataHandler.AddMainTableStructs(MainDBStruct);
      if (binDataHandler.UseFragmentation)
      {
        filesDBStruct = new DBxStruct();
        binDataHandler.AddSectionTableStructs(filesDBStruct);
        filesDBStruct.SetReadOnly(); // ����� ��� ����� ����� ����������� �����
      }
    }

    #endregion

    #region �������� �� files2, files3

    /// <summary>
    /// ���������� ������� ��������� ���� ������ "files2", "files3"
    /// </summary>
    /// <param name="sender">������ DBxBinDataHandler</param>
    /// <param name="args">�� ������������</param>
    void BinDataHandler_DBSizeLimitExceeded(object sender, EventArgs args)
    {
      DBxBinDataHandler BinDataHandler = (DBxBinDataHandler)sender;

      System.Diagnostics.Trace.WriteLine("DBxBinDataHandler.DBSizeLimitExceeded event handler called");

      string DBName = "files" + (BinDataHandler.SectionEntryCount + 1).ToString();
      DBxStruct dbs = new DBxStruct();
      BinDataHandler.AddSectionTableStructs(dbs);
      DBx DB = InitDB(DBName, dbs);
      BinDataHandler.AddSectionEntry(DB.MainEntry);

      System.Diagnostics.Trace.WriteLine("New database created: " + DB.DisplayName);
    }

    #endregion
  }
}
