using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using AgeyevAV.Logging;

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

namespace AgeyevAV.ExtDB.Docs
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
      docTypes.CheckStruct(); // 07.12.2017
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
      TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();
      _StructSource = new InternalStructSource(this);
      _DBCache = new DBxCache(this, false);

      _TextHandlers = new DBxDocTextHandlers(docTypes, DBCache);


      #region MainDocTableServiceColumns � SubDocTableServiceColumns

      DBxColumnList Cols;

      Cols = new DBxColumnList();
      Cols.Add("Id");
      if (docTypes.UseDeleted)
        Cols.Add("Deleted");
      if (docTypes.UseVersions)
        Cols.Add("Version");
      _MainDocTableServiceColumns = new DBxColumns(Cols);

      Cols = new DBxColumnList();
      Cols.Add("Id");
      Cols.Add("DocId");
      if (docTypes.UseDeleted)
        Cols.Add("Deleted");

      _SubDocTableServiceColumns = new DBxColumns(Cols);

      #endregion


      #region AllDocServiceColumns � AllSubDocServiceColumns

      Cols = new DBxColumnList();
      Cols.Add("Id");
      if (docTypes.UseDeleted)
        Cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        Cols.Add("Version");
        Cols.Add("Version2");
      }
      if (docTypes.UseUsers)
      {
        Cols.Add("CreateUserId");
        Cols.Add("ChangeUserId");
      }
      if (docTypes.UseTime)
      {
        Cols.Add("CreateTime");
        Cols.Add("ChangeTime");
      }
      _AllDocServiceColumns = new DBxColumns(Cols);


      Cols = new DBxColumnList();
      Cols.Add("Id");
      Cols.Add("DocId");
      if (docTypes.UseDeleted)
        Cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        Cols.Add("StartVersion");
        Cols.Add("Version2");
      }

      _AllSubDocServiceColumns = new DBxColumns(Cols);

      #endregion

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
    /// ���������� �������� ������ (������ BinData � Filenames)
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
      Int32 NextId;

      lock (_LastUsedTableIds)
      {
        Int32 LastId;
        if (!_LastUsedTableIds.TryGetValue(tableName, out LastId))
        {
          using (DBxCon Con = new DBxCon(mainDBEntry))
          {
            LastId = DataTools.GetInt(Con.GetMaxValue(tableName, "Id", null));
          }
          _LastUsedTableIds.Add(tableName, LastId);
        }
        NextId = LastId + 1;
        _LastUsedTableIds[tableName] = NextId;
      }
      return NextId;
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
          foreach (DBxLongDocsLock OldItem in base.Source)
          {
            if (item.Data.TestConflict(OldItem.Data))
              throw new DBxDocsLockException(item, OldItem, _TextHandlers);
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
        foreach (DBxLongDocsLock Item in collection)
          Add(Item);
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
      foreach (DataTable Table in ds.Tables)
      {
        if (Table.Rows.Count == 0)
          continue;

        IdList Ids = new IdList();
        foreach (DataRow Row in Table.Rows)
        {
          switch (Row.RowState)
          {
            case DataRowState.Added:
              Int32 NewId = (Int32)(Row["Id"]);
              if (NewId > 0)
                Ids.Add(NewId);
              break;
            case DataRowState.Modified:
              Ids.Add((Int32)(Row["Id"]));
              break;
            case DataRowState.Deleted:
              Int32 OldId = (Int32)(Row["Id", DataRowVersion.Original]);
              if (OldId > 0)
                Ids.Add(OldId);
              break;
          }
        }

        DBCache.Clear(Table.TableName, Ids);
        ClearCacheBuffer.Holder.Add(Table.TableName, Ids.ToArray());
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

      public DBxTableStruct GetTableStruct(string TableName)
      {
        return _Owner.MainDBEntry.DB.Struct.Tables[TableName];
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
    private Dictionary<string, DBxTableCacheInfo> TableCacheInfos;

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo Info;
      lock (TableCacheInfos)
      {
        if (!TableCacheInfos.TryGetValue(tableName, out Info))
        {
          DBxDocTypeBase DocTypeBase;
          if (!DocTypes.FindByTableName(tableName, out DocTypeBase))
            throw new ArgumentException("����������� ��� ������� \"" + tableName + "\"", "tableName");

          if (DocTypeBase.IndividualCacheColumns.AreAllDefaults)
            Info = null;
          else
          {
            Info = new DBxTableCacheInfo(_StructSource.GetTableStruct(tableName));
            for (int i = 0; i < Info.TableStruct.Columns.Count; i++)
            {
              string ColumnName = Info.TableStruct.Columns[i].ColumnName;
              if (DocTypeBase.Struct.Columns.Contains(ColumnName))
                Info.IndividualColumnFlags[i] = DocTypeBase.IndividualCacheColumns[ColumnName];
              // ��� ����� ���� "Id" ��� ���������������� �������� � ��������� ����� DBxDocTypeBase.Struct
            }
            Info.SetReadOnly();
          }
          TableCacheInfos.Add(tableName, Info);
        }
      }
      return Info;
    }

    DBxCacheLoadResponse IDBxCacheSource.LoadCachePages(DBxCacheLoadRequest request)
    {
      using (DBxCon Con = new DBxCon(MainDBEntry))
      {
        return Con.LoadCachePages(request);
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
      SingleScopeList<DBx> DBs = new SingleScopeList<DBx>();

      DBs.Add(MainDBEntry.DB);
      if (UndoDBEntry != null)
        DBs.Add(UndoDBEntry.DB);
      if (BinDataHandler != null)
      {
        DBs.Add(BinDataHandler.MainEntry.DB);
        DBxEntry[] entrs = BinDataHandler.GetSectionEntries();
        for (int i = 0; i < entrs.Length; i++)
          DBs.Add(entrs[i].DB);
      }

      return DBs.ToArray();
    }


    /// <summary>
    /// �������� ��� ���� �������� DBx ����� Dispose().
    /// ��� DBxRealDocProviderGlobal �� �������� Disposable-��������
    /// </summary>
    public void DisposeDBs()
    {
      DBx[] DBs = GetDBs();
      for (int i = 0; i < DBs.Length; i++)
      {
        if (!DBs[i].IsDisposed)
          DBs[i].Dispose();
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
