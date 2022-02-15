// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// ���������������� �����, �������� ����� ������ ��� ���� ������ �-�����
  /// ���� ����� �� ������������� � ���������� ������ �� ������� �������, 
  /// �.�. �������� ����� ����������� � ���� ������ ��� ����������� �������
  /// </summary>
  public class DBxRealDocProviderGlobal : IDBxCacheSource, IReadOnlyObject
  {
    #region �����������

    /// <summary>
    /// ��� ������ ������������ ������������, ����� � ��������� �� ������������ ������� BinData � FileNames
    /// </summary>
    /// <param name="docTypes">�������� ����� ����������. ������ ����������� � ����� "������ ������"</param>
    /// <param name="mainDBEntry">������� ����� �����</param>
    /// <param name="undoDBEntry">����� ����� � ���� ������. ����� ���� null</param>
    public DBxRealDocProviderGlobal(DBxDocTypes docTypes, DBxEntry mainDBEntry, DBxEntry undoDBEntry)
      : this(docTypes, mainDBEntry, undoDBEntry, null)
    {
    }

    /// <summary>
    /// ��� ������ ������������ ���� ����������� ������������ ������� BinData � FileNames
    /// </summary>
    /// <param name="docTypes">�������� ����� ����������. ������ ����������� � ����� "������ ������"</param>
    /// <param name="mainDBEntry">������� ����� �����</param>
    /// <param name="undoDBEntry">����� ����� � ���� ������. ����� ���� null</param>
    /// <param name="binDataHandler">���������� �������� ������. ����������� � ����� "������ ������". ����� ���� null</param>
    public DBxRealDocProviderGlobal(DBxDocTypes docTypes, DBxEntry mainDBEntry, DBxEntry undoDBEntry, DBxBinDataHandler binDataHandler)
    {
      if (docTypes == null)
        throw new ArgumentNullException("docTypes");

      docTypes.SetReadOnly();
      docTypes.CheckStruct(binDataHandler); // 07.12.2017
      _DocTypes = docTypes;

      if (mainDBEntry == null)
        throw new ArgumentNullException("mainDBEntry");
      _MainDBEntry = mainDBEntry;

      if (undoDBEntry != null)
      {
        if (!docTypes.UseDeleted)
          throw new ArgumentException("������� ������� ����������, ���� �������� DBxDocTypes.UseDeleted=false", "undoDBEntry");
        if (!docTypes.UseVersions)
          throw new ArgumentException("������� ������� ����������, ���� �������� DBxDocTypes.UseVersions=false", "undoDBEntry");
        if (!docTypes.UseTime)
          throw new ArgumentException("������� ������� ����������, ���� �������� DBxDocTypes.UseTime=false", "undoDBEntry");
      }
      else
      {
        if (docTypes.UseSessionId)
          throw new ArgumentException("������ ������������� �������� DBxDocTypes.UseSessionId, ���� ������� ���������� �� �������", "undoDBEntry");
      }
      _UndoDBEntry = undoDBEntry;

      if (binDataHandler == null)
      {
        if (docTypes.HasBinDataRefs || docTypes.HasFileRefs)
          throw new ArgumentNullException("binDataHandler", "��� ��� � DocTypes ������ ������������� ������ �� �������� ������, �������� BinDataHandler ������ ���� �����");
      }
      else
        binDataHandler.SetReadOnly();
      _BinDataHandler = binDataHandler;

      _BinDataDBStruct = new DBxStruct();
      if (binDataHandler != null)
        binDataHandler.AddMainTableStructs(_BinDataDBStruct);
      _BinDataDBStruct.SetReadOnly();

      _LastUsedTableIds = new Dictionary<string, Int32>();

      _ActiveLocks = new List<DBxShortDocsLock>();
      _LongLocks = new LongTermLockList(this.TextHandlers);

      _ClearCacheBuffer = new DBxClearCacheBuffer(DefaultClearCacheBuffer);
      _TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();
      _StructSource = new InternalStructSource(this);
      _DBCache = new DBxCache(this, false);

      _TextHandlers = new DBxDocTextHandlers(docTypes, DBCache);

      #region MainDocTableServiceColumns � SubDocTableServiceColumns

      DBxColumnList cols;

      cols = new DBxColumnList();
      cols.Add("Id");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
        cols.Add("Version");
      _MainDocTableServiceColumns = new DBxColumns(cols);

      cols = new DBxColumnList();
      cols.Add("Id");
      cols.Add("DocId");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");

      _SubDocTableServiceColumns = new DBxColumns(cols);

      #endregion

      #region AllDocServiceColumns � AllSubDocServiceColumns

      cols = new DBxColumnList();
      cols.Add("Id");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        cols.Add("Version");
        cols.Add("Version2");
      }
      if (docTypes.UseUsers)
      {
        cols.Add("CreateUserId");
        cols.Add("ChangeUserId");
      }
      if (docTypes.UseTime)
      {
        cols.Add("CreateTime");
        cols.Add("ChangeTime");
      }
      _AllDocServiceColumns = new DBxColumns(cols);


      cols = new DBxColumnList();
      cols.Add("Id");
      cols.Add("DocId");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        cols.Add("StartVersion");
        cols.Add("Version2");
      }

      _AllSubDocServiceColumns = new DBxColumns(cols);

      #endregion

      _MasterRefs = new DBxExtRefs(docTypes, binDataHandler);

      DBxDocDebugTools.InitLogout();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ ���������� ������ ��� �������� ����������.
    /// ��� �������������, �������� ����� ���� �������� �� ������ ������������ DBxRealDocProviderGlobal.
    /// ����������� �������� ������� �� ����������� ������� ������ ������ Swap() �� ������� �� ������� 
    /// ������� � ������� ������ ��������� ���������.
    /// ���� ����� ���������, �� ������� ����� �������� ������ ������ �������, �.�. ����� �������� �����
    /// ����� �������. ���� ����� ������� �������, �� ����� ������ ������ ������
    /// �������� �� ��������� ����� 10
    /// </summary>
    public static int DefaultClearCacheBuffer
    {
      get { return _DefaultClearCacheBuffer; }
      set { _DefaultClearCacheBuffer = value; }
    }
    private static int _DefaultClearCacheBuffer = 10;

    /// <summary>
    /// ���������� ��� ����������
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private DBxDocTypes _DocTypes;


    /// <summary>
    /// ����������� � �������� ���� ������.
    /// ��� ����������� �� �������� ����������� ������������
    /// �� ����� ���� null
    /// </summary>
    public DBxEntry MainDBEntry { get { return _MainDBEntry; } }
    private DBxEntry _MainDBEntry;

    /// <summary>
    /// ����������� � ���� ������ undo.
    /// ����� ����������� �� ������ ��������� ����������� ������������, ����� ��� ����������� ������ ��������� 
    /// �� ���� Main � Undo ����� ��������� ������
    /// ����� ���� null, ���� ���� ������ ������������� ��� ������, ��� ������� �� ������ ���������. ��� ���� �� ����� �������� ������ �
    /// ������� ����������. ������ � ���������� ��-�������� ����� ���� ��������� ���� Version � ������.
    /// ������� ��������� ����� ������������ ������������ ���������� � DBxDocTypes.
    /// </summary>
    public DBxEntry UndoDBEntry { get { return _UndoDBEntry; } }
    private DBxEntry _UndoDBEntry;

    /// <summary>
    /// ���������� �������� ������ (������ BinData � FileNames)
    /// ����� ���� null, ���� �������� ����� ������ �� �������������
    /// </summary>
    public DBxBinDataHandler BinDataHandler { get { return _BinDataHandler; } }
    private DBxBinDataHandler _BinDataHandler;

    /// <summary>
    /// ���������, ���������� ���������� ������ "BinData" � "FileNames".
    /// ���� �� ������ ����� �������������, ���� �������� BinDataHandler.UseBinData
    /// ��� UseFiles �� �����������.
    /// ���� BinDataHandler=null, �������� ������ ���������� ���������, � �� null.
    /// </summary>
    internal DBxStruct BinDataDBStruct { get { return _BinDataDBStruct; } }
    private DBxStruct _BinDataDBStruct;

    /// <summary>
    /// ������������� �������� ���� ������
    /// </summary>
    public string DBIdentity { get { return MainDBEntry.DB.DBIdentity; } }


    /// <summary>
    /// ��������� ���� � ������ ������� ����������, ����������� � ������� DBxMultiDocs
    /// </summary>
    internal DBxColumns MainDocTableServiceColumns { get { return _MainDocTableServiceColumns; } }
    private DBxColumns _MainDocTableServiceColumns;

    /// <summary>
    /// ��������� ���� � ������ ������� �������������, ����������� � ������� DBxMultiSubDocs
    /// </summary>
    internal DBxColumns SubDocTableServiceColumns { get { return _SubDocTableServiceColumns; } }
    private DBxColumns _SubDocTableServiceColumns;

    /// <summary>
    /// ��� ��������� ���� � �������� ����������
    /// </summary>
    internal DBxColumns AllDocServiceColumns { get { return _AllDocServiceColumns; } }
    private DBxColumns _AllDocServiceColumns;

    /// <summary>
    /// ��� ��������� ���� � �������� �������������
    /// </summary>
    internal DBxColumns AllSubDocServiceColumns { get { return _AllSubDocServiceColumns; } }
    private DBxColumns _AllSubDocServiceColumns;

    /// <summary>
    /// �������� ������������ ������ � ������������ �������� ������ ��� ������-�������
    /// </summary>
    internal DBxExtRefs MasterRefs { get { return _MasterRefs; } }
    private DBxExtRefs _MasterRefs;

    #endregion

    #region ��������� ��������������� ��� INSERT

    /// <summary>
    /// ��������� ��������������, �������������� ��� ������ ������� ����������
    /// ��� ��������� ���� ������ �����������
    /// </summary>
    private Dictionary<string, Int32> _LastUsedTableIds;

    /// <summary>
    /// �������� ��������� ��������� ������������� ��� ������ ��������� ��� ������������
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="mainDBEntry"></param>
    /// <returns></returns>
    internal Int32 GetNextId(string tableName, DBxEntry mainDBEntry)
    {
      Int32 nextId;

      lock (_LastUsedTableIds)
      {
        Int32 lastId;
        if (!_LastUsedTableIds.TryGetValue(tableName, out lastId))
        {
          using (DBxCon Con = new DBxCon(mainDBEntry))
          {
            lastId = DataTools.GetInt(Con.GetMaxValue(tableName, "Id", null));
          }
          _LastUsedTableIds.Add(tableName, lastId);
        }
        nextId = lastId + 1;
        _LastUsedTableIds[tableName] = nextId;
      }
      return nextId;
    }

    #endregion

    #region ����������

    #region ���������������

    /// <summary>
    /// ������ �������� ��������������� ���������� ���� ������
    /// ��� ������ � ���������� ������ ������ ������������� ������
    /// </summary>
    internal List<DBxShortDocsLock> ActiveLocks { get { return _ActiveLocks; } }
    private List<DBxShortDocsLock> _ActiveLocks;

    #endregion

    #region ����������

    /// <summary>
    /// ������ �������� ���������� ���������� ���� ������, ������������ � �������� 
    /// �������������� ���������� �������������.
    /// ����� �������� ����������������.
    /// </summary>
    public class LongTermLockList : SyncCollection<DBxLongDocsLock>
    {
      #region �����������

      internal LongTermLockList(DBxDocTextHandlers textHandlers)
      {
        _Source = (List<DBxLongDocsLock>)(base.Source);
        _TextHandlers = textHandlers;
      }

      /// <summary>
      /// ����� ������� ���������� ��� ��������������.
      /// �� ������ ������ ����������� ������ ���� lock(SyncRoot).
      /// </summary>
      private /*new*/ List<DBxLongDocsLock> _Source;

      private DBxDocTextHandlers _TextHandlers;

      #endregion

      #region ���������� � �������� ����������

      /// <summary>
      /// ������������� ����������.
      /// ���� ���������� ����������� � ��� ��������������, ������������� DBxDocsLockException
      /// </summary>
      /// <param name="item">����������</param>
      public new void Add(DBxLongDocsLock item)
      {
        if (item == null)
          throw new ArgumentNullException("item");
        item.SetReadOnly();
#if DEBUG
        if (!item.IsReadOnly)
          throw new BugException("!DBxLongDocsLock.IsReadOnly");
#endif

        lock (base.SyncRoot)
        {
          foreach (DBxLongDocsLock oldItem in base.Source)
          {
            if (item.Data.TestConflict(oldItem.Data))
              throw new DBxDocsLockException(item, oldItem, _TextHandlers);
          }

          base.Add(item);
        }
      }

      /// <summary>
      /// ��������� ���������� �� ������
      /// </summary>
      /// <param name="collection">����������</param>
      public new void AddRange(IEnumerable<DBxLongDocsLock> collection)
      {
#if DEBUG
        if (collection == null)
          throw new ArgumentException("collection");
#endif
        if (Object.ReferenceEquals(collection, this))
          throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

        foreach (DBxLongDocsLock item in collection)
          Add(item);
      }

      /// <summary>
      /// ������� ���������� � �������� ���������������
      /// </summary>
      /// <param name="lockGuid">������������� ���������� ����������</param>
      /// <returns>true, ���� ���������� ������� � �����</returns>
      public bool Remove(Guid lockGuid)
      {
        if (lockGuid == Guid.Empty)
          return false;

        lock (SyncRoot)
        {
          ResetCopyArray();
          for (int i = 0; i < _Source.Count; i++)
          {
            if (_Source[i].Guid == lockGuid)
            {
              _Source.RemoveAt(i);
              return true;
            }
          }
        }

        return false;
      }

      #endregion
    }

    /// <summary>
    /// ������ ������������� ���������� ����������
    /// </summary>
    public LongTermLockList LongLocks { get { return _LongLocks; } }
    private LongTermLockList _LongLocks;

    #endregion

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������ ����������� ������ "��� �����������".
    /// </summary>
    public DBxCache DBCache { get { return _DBCache; } }
    private DBxCache _DBCache;

    #endregion

    #region ������ ������ ��� ������� ����������� ��������

    /// <summary>
    /// ����� ��� ������������ ��������� � ��������.
    /// ���������� ������� ������ �������� �� ������� ����� ClearCacheBuffer.Swap()
    /// </summary>
    public DBxClearCacheBuffer ClearCacheBuffer { get { return _ClearCacheBuffer; } }
    private DBxClearCacheBuffer _ClearCacheBuffer;

    /// <summary>
    /// ������� ��� DBCache.
    /// � ����� ClearCacheBuffer ��������� ������ �� ����� ����������� ���� ������
    /// </summary>
    public void ClearCache()
    {
      DBCache.Clear();
      ClearCacheBuffer.Holder.AddAllTables();
    }

    /// <summary>
    /// ������� ��� ��� �������� ������� � �������� ������ � ClearCacheBuffer
    /// </summary>
    /// <param name="tableName">��� ������� ���� ������</param>
    public void ClearCache(string tableName)
    {
      DBCache.Clear(tableName);
      ClearCacheBuffer.Holder.Add(tableName);
    }

    /// <summary>
    /// ������� ��� ��� ��������� ������ ��������������� � �������� ������ � ClearCacheBuffer
    /// </summary>
    /// <param name="tableName">��� ������� ���� ������</param>
    /// <param name="ids">������ ���������������</param>
    public void ClearCache(string tableName, IdList ids)
    {
      DBCache.Clear(tableName, ids);
      ClearCacheBuffer.Holder.Add(tableName);
    }

    /// <summary>
    /// ������� ��� ��� �������� ������ � ��������������� � �������� ������ � ClearCacheBuffer.
    /// </summary>
    /// <param name="ds">����� ������ ������, � ������� ����������� ������ � ���������
    /// DataRowState.Added, Modified � Deleted</param>
    public void ClearCache(DataSet ds)
    {
      foreach (DataTable table in ds.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        IdList ids = new IdList();
        foreach (DataRow row in table.Rows)
        {
          switch (row.RowState)
          {
            case DataRowState.Added:
              Int32 newId = (Int32)(row["Id"]);
              if (newId > 0)
                ids.Add(newId);
              break;
            case DataRowState.Modified:
              ids.Add((Int32)(row["Id"]));
              break;
            case DataRowState.Deleted:
              Int32 oldId = (Int32)(row["Id", DataRowVersion.Original]);
              if (oldId > 0)
                ids.Add(oldId);
              break;
          }
        }

        DBCache.Clear(table.TableName, ids);
        ClearCacheBuffer.Holder.Add(table.TableName, ids.ToArray());
      }
    }

    #endregion

    #region IDBxCacheSource Members

    /// <summary>
    /// ���������� DBxStructSource, ������������ ������ �� ������� � ����, � ������� � ������������
    /// ���� ������ �� ������ ��� �� ������
    /// </summary>
    private class InternalStructSource : MarshalByRefObject, IDBxStructSource
    {
      #region �����������

      public InternalStructSource(DBxRealDocProviderGlobal owner)
      {
        _Owner = owner;
      }

      #endregion

      #region ����

      private DBxRealDocProviderGlobal _Owner;

      #endregion

      #region ���������������� ������

      public string[] GetAllTableNames()
      {
        return _Owner.MainDBEntry.DB.Struct.AllTableNames;
      }

      public DBxTableStruct GetTableStruct(string tableName)
      {
        return _Owner.MainDBEntry.DB.Struct.Tables[tableName];
      }

      #endregion
    }

    private InternalStructSource _StructSource;

    IDBxStructSource IDBxCacheSource.StructSource { get { return _StructSource; } }


    /// <summary>
    /// ��������� �������� ����� ������ ��� �����������.
    /// ���� - ��� �������.
    /// �� ������ ��������� ��������� ����������� ��� ����������� ������������������
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo info;
      lock (_TableCacheInfos)
      {
        if (!_TableCacheInfos.TryGetValue(tableName, out info))
        {
          DBxDocTypeBase docTypeBase;
          if (!DocTypes.FindByTableName(tableName, out docTypeBase))
            throw new ArgumentException("����������� ��� ������� \"" + tableName + "\"", "tableName");

          if (docTypeBase.IndividualCacheColumns.AreAllDefaults)
            info = null;
          else
          {
            info = new DBxTableCacheInfo(_StructSource.GetTableStruct(tableName));
            for (int i = 0; i < info.TableStruct.Columns.Count; i++)
            {
              string columnName = info.TableStruct.Columns[i].ColumnName;
              if (docTypeBase.Struct.Columns.Contains(columnName))
                info.IndividualColumnFlags[i] = docTypeBase.IndividualCacheColumns[columnName];
              // ��� ����� ���� "Id" ��� ���������������� �������� � ��������� ����� DBxDocTypeBase.Struct
            }
            info.SetReadOnly();
          }
          _TableCacheInfos.Add(tableName, info);
        }
      }
      return info;
    }

    DBxCacheLoadResponse IDBxCacheSource.LoadCachePages(DBxCacheLoadRequest request)
    {
      using (DBxCon mainCon = new DBxCon(MainDBEntry))
      {
        return mainCon.LoadCachePages(request);
      }
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      // ������ �� ������
    }

    #endregion

    #region ��������� ������������� ����������

    /// <summary>
    /// ���������� ���������� ������������� ���������.
    /// ������ ������ ���� �������� (�������� Add()) �� �������� ������� ������� DBxRealDocProviderSource
    /// </summary>
    public DBxDocTextHandlers TextHandlers { get { return _TextHandlers; } }
    private DBxDocTextHandlers _TextHandlers;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������".
    /// ����� ��������, ��� ������ ��� ������������ DBxRealDocProviderSource, �
    /// �� ����� ��������� ������.
    /// </summary>
    public bool IsReadOnly { get { return _TextHandlers.IsReadOnly; } }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ��������� ������ � ����� "������ ������".
    /// ����� �� ������ �������������� � ���������������� ����.
    /// </summary>
    public void SetReadOnly()
    {
      _TextHandlers.SetReadOnly();
    }

    #endregion

    #region ������ ������ �� ������� ��� ������

    /// <summary>
    /// ���������� ������ ������������ ��� ������ (��� ��������)
    /// </summary>
    /// <returns></returns>
    public DBx[] GetDBs()
    {
      SingleScopeList<DBx> dbs = new SingleScopeList<DBx>();

      dbs.Add(MainDBEntry.DB);
      if (UndoDBEntry != null)
        dbs.Add(UndoDBEntry.DB);
      if (BinDataHandler != null)
      {
        dbs.Add(BinDataHandler.MainEntry.DB);
        DBxEntry[] entrs = BinDataHandler.GetSectionEntries();
        for (int i = 0; i < entrs.Length; i++)
          dbs.Add(entrs[i].DB);
      }

      return dbs.ToArray();
    }


    /// <summary>
    /// �������� ��� ���� �������� DBx ����� Dispose().
    /// ��� DBxRealDocProviderGlobal �� �������� Disposable-��������
    /// </summary>
    public void DisposeDBs()
    {
      DBx[] dbs = GetDBs();
      for (int i = 0; i < dbs.Length; i++)
      {
        if (!dbs[i].IsDisposed)
          dbs[i].Dispose();
      }
    }

    #endregion
  }

  internal static class DBxDocDebugTools
  {
    #region ����������� ����� �������������

    public static void InitLogout()
    {
      DBxTools.InitLogout();

      lock (typeof(DBxDocDebugTools))
      {
        if (_FirstCall)
        {
          _FirstCall = false;
          LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
          LogoutTools.LogoutProp += new LogoutPropEventHandler(LogoutTools_LogoutProp);
        }
      }
    }

    private static bool _FirstCall = true;

    #endregion

    #region �������������� ���������� ����������

    static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
#if DEBUG
      if (DBxDocProvider.DebugDocProviderList != null)
      {
        GC.Collect(); // ���� ��?

        DBxDocProvider[] a = DBxDocProvider.DebugDocProviderList.ToArray();
        args.WriteHeader("DBxDocProvider list (" + a.Length.ToString() + ")");
        LogoutTools.LogoutObject(args, a);
      }
#endif
    }

    #endregion

    #region ���������� ������ �������

    static void LogoutTools_LogoutProp(object sender, LogoutPropEventArgs args)
    {
      if (args.Object is DBxDocProvider)
      {
        switch (args.PropertyName)
        {
          case "DocTypes":
          case "InternalFixedInfo": // 28.04.2018
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is DBxDocTextHandlers)
      {
        switch (args.PropertyName)
        {
          case "DocTypes":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is UserPermissions)
      {
        switch (args.PropertyName)
        {
          case "AsXmlText":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is DBxRealDocProviderSource)
      {
        switch (args.PropertyName)
        {
          case "GlobalData":
            args.Mode = LogoutPropMode.None; // 13.01.2017
            break;
        }
      }

      if (args.Object is DBxDocSet) // 21.03.2017
      {
        switch (args.PropertyName)
        {
          case "DocProvider":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is DBxSingleDoc ||
        args.Object is DBxMultiDocs ||
        args.Object is DBxSubDoc ||
        args.Object is DBxSingleSubDocs ||
        args.Object is DBxMultiSubDocs)
      {
        switch (args.PropertyName)
        {
          case "DocProvider":
          case "DocSet":
          case "Values":
          case "OriginalValues":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
    }

    #endregion
  }
}
