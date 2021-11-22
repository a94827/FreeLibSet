// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  #region ��������

  /// <summary>
  /// ��������� ������� DBxRealDocProviderSource.BeforeApplyChanges
  /// </summary>
  public class DBxBeforeApplyChangesEventArgs : EventArgs
  {
    #region �����������

    /// <summary>
    /// ��������� ������� �� ��������� � ���������������� ����
    /// </summary>
    /// <param name="docSet">����� ������</param>
    public DBxBeforeApplyChangesEventArgs(DBxDocSet docSet)
    {
#if DEBUG
      if (docSet == null)
        throw new ArgumentNullException("docSet");
#endif

      _DocSet = docSet;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������, ���������� �� �������� �����������
    /// </summary>
    public DBxDocSet DocSet { get { return _DocSet; } }
    private DBxDocSet _DocSet;

    #endregion
  }

  /// <summary>
  /// ������� ������� DBxRealDocProviderSource.BeforeApplyChanges
  /// </summary>
  /// <param name="sender">������ �� DBxRealDocProviderSource</param>
  /// <param name="args">��������� �������</param>
  public delegate void DBxBeforeApplyChangesEventHandler(object sender,
    DBxBeforeApplyChangesEventArgs args);

  #endregion

  /// <summary>
  /// ���������������� (� ������ ReadOnly) �����, �������� ����� ������ ��� ������ ��� ���������� DBxRealDocProvider 
  /// ������ ������ ��������� ���������� ��� ������������
  /// ���� ����� �� ������������� � ���������� ������ �� ������� �������, 
  /// �.�. ����� �������� GlobalData �������� ����� ����������� � ���� ������ ��� ����������� �������
  /// </summary>
  public class DBxRealDocProviderSource : IReadOnlyObject, IDBxCacheSource
  {
    #region �����������

    /// <summary>
    /// ������� ������ DBxRealDocProviderSource.
    /// ��� ����������� <paramref name="globalData"/> ����� ������ SetReadOnly().
    /// </summary>
    /// <param name="globalData">����� ������ �� ������� ���������� ��� ��������� ���� ��������� �������������</param>
    public DBxRealDocProviderSource(DBxRealDocProviderGlobal globalData)
    {
#if DEBUG
      if (globalData == null)
        throw new ArgumentNullException("globalData");
#endif
      globalData.SetReadOnly(); // 17.11.2017
      _GlobalData = globalData;

      _MainDBEntry = globalData.MainDBEntry;

      _UserPermissions = UserPermissions.Empty;

      _DisplayName = "[ ��� ����� ]";

      _SyncRoot = new object();

      //_TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������ ��� ���� �������������
    /// </summary>
    public DBxRealDocProviderGlobal GlobalData { get { return _GlobalData; } }
    private DBxRealDocProviderGlobal _GlobalData;

    /// <summary>
    /// ������������� ����������, ����������� ������������
    /// ���� �������� �� ����������� � ����� ����, ������������ ������ ������ UserPermissions.Empty.
    /// ��� ��������� ���������� ��������� ������� ����� ������ UserPermissions, ��������� ���, 
    /// � ����� - ���������� ��� ��������. ��� ���� ������ UserPermissions ����������� � ��������� ReadOnly
    /// </summary>
    public UserPermissions UserPermissions
    {
      get { return _UserPermissions; }
      set
      {
        CheckNotReadOnly();
        _UserPermissions = value;
        _UserPermissions.SetReadOnly();
      }
    }
    private UserPermissions _UserPermissions;

    /// <summary>
    /// ����������� � �������� ���� ������.
    /// �� ��� ����������� ����� ������������ �� ������� ���� ������
    /// �������� ������ ���� ����������� �����������, ����� ������������ ������� �������������� �����
    /// </summary>
    public DBxEntry MainDBEntry
    {
      get { return _MainDBEntry; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          throw new ArgumentNullException();
        _MainDBEntry = value;
      }
    }
    private DBxEntry _MainDBEntry;

    /// <summary>
    /// ������������� ���� ������. ������������ ��� ������ ������ ������.
    /// </summary>
    public string DBIdentity { get { return MainDBEntry.DB.DBIdentity; } }

    /// <summary>
    /// ������������ ���
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set
      {
        CheckNotReadOnly();
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// ���������� DisplayName
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region �����������

    /// <summary>
    /// ������������ � �������� ������������ ������� GlobalData ��� typeof() �� �����
    /// </summary>
    private object _SyncRoot;

    /// <summary>
    /// ���������������� ��� ������ �� ������� �������
    /// ������� DBxRealDocProvider, ��������� � ������� ����������, ���������� ���� ������
    /// ���� IsReadOnly=false, �������� ���������� null
    /// </summary>
    public DBxCache DBCache
    {
      get
      {
        lock (_SyncRoot)
        {
          if (_DBCache == null)
          {
            if (IsReadOnly)
            {
              // ������� ��� �������� � ����� ReadOnly
              //_StructSource = new InternalStructSource(this);
              _DBCache = new DBxCache(this, false);
            }
            else
              return null;
          }
          return _DBCache;
        }
      }
    }

    private DBxCache _DBCache;

    /// <summary>
    /// �������� DBxCache.ClearCache()
    /// </summary>
    public void ClearCache()
    {
      lock (_SyncRoot)
      {
        if (_DBCache != null)
          _DBCache.Clear();
      }
    }

    /// <summary>
    /// �������� ����������� ����� DBxDocProvider.ClearCache()
    /// </summary>
    /// <param name="ds">����� ������ � DBxDocSet</param>
    public void ClearCache(DataSet ds)
    {
      lock (_SyncRoot)
      {
        if (_DBCache != null)
          DBxDocProvider.ClearCache(ds, _DBCache);
      }
    }

    #region IDBxCacheSource Members

    IDBxStructSource IDBxCacheSource.StructSource { get { return _StructSource; } }
    private DBxDocStructSource _StructSource;


    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      if (_StructSource == null)
        return null;
      else
        return _StructSource.GetTableCacheInfo(tableName);
    }

#if XXX
    /// <summary>
    /// ��������� �������� ����� ������ ��� �����������.
    /// ���� - ��� �������.
    /// �� ������ ��������� ��������� ����������� ��� ����������� ������������������
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo Info;
      lock (_SyncRoot)
      {
        if (!_TableCacheInfos.TryGetValue(tableName, out Info))
        {
          if (tableName == "FileNames")
          {
            Info = new DBxTableCacheInfo(GlobalData.BinDataHandler.MainEntry.DB.Struct.Tables[tableName]);
            Info.SetReadOnly();
          }
          else if (tableName == "BinData")
          {
            Info = new DBxTableCacheInfo(GlobalData.BinDataHandler.BinDataCacheTableStruct);
            Info.SetReadOnly();
          }
          else
          {
            DBxDocTypeBase DocTypeBase;
            if (!GlobalData.DocTypes.FindByTableName(tableName, out DocTypeBase))
              throw new ArgumentException("����������� ��� ������� \"" + tableName + "\"", "TableName");

            if (DocTypeBase.IndividualCacheColumns.AreAllDefaults)
              Info = null;
            else
            {
              if (_StructSource == null)
                return null;
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
          }
          _TableCacheInfos.Add(tableName, Info);
        }
      }
      return Info;
    }
#endif

    DBxCacheLoadResponse IDBxCacheSource.LoadCachePages(DBxCacheLoadRequest request)
    {
      // � ����� ������� ����� ���� ��������� � �������� �������� ���� ������ � � ������� �������� ������

      // ��������� �� ��������
      Dictionary<string, DBxCacheLoadRequest> dict1 = request.SplitByTables();

      // ��������� �� ����� ������
      Dictionary<DBxEntry, List<DBxCacheLoadRequest>> dict2 = new Dictionary<DBxEntry, List<DBxCacheLoadRequest>>();
      foreach (KeyValuePair<string, DBxCacheLoadRequest> pair1 in dict1)
      {
        DBxEntry entr = GetEntry(pair1.Key);
        List<DBxCacheLoadRequest> lst;
        if (!dict2.TryGetValue(entr, out lst))
        {
          lst = new List<DBxCacheLoadRequest>();
          dict2.Add(entr, lst);
        }
        lst.Add(pair1.Value);
      }

      List<DBxCacheLoadResponse> resps = new List<DBxCacheLoadResponse>();
      foreach (KeyValuePair<DBxEntry, List<DBxCacheLoadRequest>> pair2 in dict2)
      {
        using (DBxCon Con = new DBxCon(pair2.Key))
        {
          for (int i = 0; i < pair2.Value.Count; i++)
          {
            DBxCacheLoadResponse r = Con.LoadCachePages(pair2.Value[i]);
            resps.Add(r);
          }
        }
      }

      return DBxCacheLoadResponse.Merge(resps.ToArray());
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstId)
    {
      // ������ �� ������
    }

    private DBxEntry GetEntry(string TableName)
    {
      if (TableName == "BinData" || TableName == "FileNames")
        return GlobalData.BinDataHandler.MainEntry;
      else
        return MainDBEntry;
    }

    #endregion

    /// <summary>
    /// ��������� ���������� ������������� ��� ���������.
    /// ���� � ������������ ��� ���� �� ������ ����������, ���������� ������ �������������.
    /// ������� ���� �� ��������� ���� �� �����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">������������� ��������� ��� ������������</param>
    /// <returns>��������� �������������</returns>
    public string GetTextValue(string tableName, Int32 id)
    {
      if (MainDBEntry.Permissions.TableModes[tableName] == DBxAccessMode.None)
        return "Id=" + id.ToString();
      else
        return GlobalData.TextHandlers.GetTextValue(tableName, id);
    }

    internal string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      if (MainDBEntry.Permissions.TableModes[tableName] == DBxAccessMode.None)
        return "Id=" + id.ToString();
      else
      {
        try
        {
          return GlobalData.TextHandlers.DoGetTextValue(tableName, id, primaryDS, false);
        }
        catch (Exception e)
        {
          return "Id=" + id.ToString() + ". ������. " + e.Message;
        }
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ������ DBxRealDocProviderSource ��� ��������� � ����� "������ ������".
    /// ��� ���� ������ ������������� ����������� ��������.
    /// �� ����� ��������� � ������ �� ��������� ����������.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

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
    /// ��� ������������� �������� ����� �� ����������������� ����, ��� ��� �� ���������� ��� 
    /// �������� DBxRealDocProvider.
    /// </summary>
    public void SetReadOnly()
    {
      if (_IsReadOnly)
        return;

      //if (FDocTypes == null)
      //  throw new NullReferenceException("�� ����������� �������� DocTypes");
      if (_MainDBEntry == null)
        throw new NullReferenceException("�� ����������� �������� MainDBEntry");

      _StructSource = new DBxDocStructSource(_GlobalData.DocTypes,
        _MainDBEntry.Permissions, new DBxBinDataHandlerInfo(_GlobalData.BinDataHandler));

      _IsReadOnly = true;
    }

    #endregion

    #region �����������

    /// <summary>
    /// ��� ������� ���������� �� ���������� ��������� � ����������.
    /// ���������� ������� �����, ��������, ����������� ���������� ������������.
    /// ���������� ������ ���� ����������������
    /// </summary>
    public event DBxBeforeApplyChangesEventHandler BeforeApplyChanges;

    /// <summary>
    /// �������� ������� BeforeApplyChanges, ���� ���������� ����������
    /// </summary>
    /// <param name="docSet">����� ������, ��� �������� ����������� ���������</param>
    public void PerformBeforeApplyChanges(DBxDocSet docSet)
    {
      if (BeforeApplyChanges != null)
      {
        DBxBeforeApplyChangesEventArgs Args = new DBxBeforeApplyChangesEventArgs(docSet);
        BeforeApplyChanges(this, Args);
      }
    }

    #endregion
  }

  /// <summary>
  /// ������������� ���������� � DBxBinDataHandler, ������������ � DBxDocProvider
  /// </summary>
  [Serializable]
  internal class DBxBinDataHandlerInfo
  {
    #region �����������

    /// <summary>
    /// ������������� �������.
    /// �������� ����� ���� null
    /// </summary>
    /// <param name="handler">���������� �������� ������ �� ������� ������� ��� null</param>
    public DBxBinDataHandlerInfo(DBxBinDataHandler handler)
    {
      if (handler != null)
      {
        _UseBinData = handler.UseBinData;
        _UseFiles = handler.UseFiles;

        // ������ DBxStruct, � ������� �� DBxTableStruct, �� �������� �������������.
        // ����� ������� � ����� DBxTableStruct
        DBxStruct dbs = new DBxStruct();
        handler.AddMainTableStructs(dbs);
        _BinDataTableStruct = dbs.Tables["BinData"];
        _FileNamesTableStruct = dbs.Tables["FileNames"];
      }
    }

    #endregion

    #region ��������

    public bool UseBinData { get { return _UseBinData; } }
    private bool _UseBinData;

    public bool UseFiles { get { return _UseFiles; } }
    private bool _UseFiles;

    public DBxTableStruct BinDataTableStruct { get { return _BinDataTableStruct; } }
    private DBxTableStruct _BinDataTableStruct;

    public DBxTableStruct FileNamesTableStruct { get { return _FileNamesTableStruct; } }
    private DBxTableStruct _FileNamesTableStruct;

    #endregion

    #region ������

    public DBxTableStruct GetTableStruct(string tableName)
    {
      if (_BinDataTableStruct != null)
      {
        if (tableName == _BinDataTableStruct.TableName)
          return _BinDataTableStruct;
      }

      if (_FileNamesTableStruct != null)
      {
        if (tableName == _FileNamesTableStruct.TableName)
          return _FileNamesTableStruct;
      }
      return null;
    }

    #endregion
  }

  /// <summary>
  /// ���������� ������� DBxDocProvider � DBxRealDocProviderSource.StructSource
  /// ��������� "���������" ��������, � ������������ � ������� ������������.
  /// ����� �������� ����������������
  /// </summary>
  internal class DBxDocStructSource : IDBxStructSource
  {
    #region �����������

    internal DBxDocStructSource(DBxDocTypes docTypes, DBxPermissions dbPermissions, DBxBinDataHandlerInfo binDataInfo)
    {
      _DocTypes = docTypes;
      _DBPermissions = dbPermissions;
      _BinDataInfo = binDataInfo;

      _StructDict = new Dictionary<string, DBxTableStruct>(); // ���������� � �������� ������������ �������
    }

    #endregion

    #region �������� ������

    DBxDocTypes _DocTypes;
    DBxPermissions _DBPermissions;
    DBxBinDataHandlerInfo _BinDataInfo;

    /// <summary>
    /// ������ ��� ������������� �� ���� ���������.
    /// ����� ��������� ���������� ��� ����������
    /// </summary>
    private object SyncRoot { get { return _StructDict; } }

    #endregion

    #region ������ ���������

    /// <summary>
    /// ��������� �������� ���� ������, ��������� DBxDocTypes.GetMainDBStruct().
    /// �������� ��� ���� ���������� � �������������, ������� ��, � ������� ��� �������.
    /// ���������������� ��� ������ ���������
    /// </summary>
    /// <returns></returns>
    private DBxStruct MainDBstruct
    {
      get
      {
        if (_MainDBstruct == null)
          _MainDBstruct = _DocTypes.GetMainDBStruct();
        return _MainDBstruct;
      }
    }
    private DBxStruct _MainDBstruct;

    #endregion

    #region ������ ������, � ������� ���� ������

    /// <summary>
    /// ���������� ������ ������ ������, � ������� � ������������ ���� ������
    /// �������� ������� "BinData" � "FileNames"
    /// </summary>
    /// <returns></returns>
    public string[] GetAllTableNames()
    {
      if (_AllTableNames == null)
      {
        lock (SyncRoot)
        {
          if (_AllTableNames == null)
          {
            List<string> lst = new List<string>();
            foreach (DBxDocType DocType in _DocTypes)
            {
              if (_DBPermissions.TableModes[DocType.Name] == DBxAccessMode.None)
                continue;
              lst.Add(DocType.Name);
              foreach (DBxSubDocType SubDocType in DocType.SubDocs)
              {
                if (_DBPermissions.TableModes[SubDocType.Name] == DBxAccessMode.None)
                  continue;
                lst.Add(SubDocType.Name);
              }
            }

            if (_BinDataInfo.BinDataTableStruct != null)
              lst.Add(_BinDataInfo.BinDataTableStruct.TableName);
            if (_BinDataInfo.FileNamesTableStruct != null)
              lst.Add(_BinDataInfo.FileNamesTableStruct.TableName);

            _AllTableNames = lst.ToArray();
          }
        }
      }
      return _AllTableNames;
    }

    private volatile string[] _AllTableNames;

    #endregion

    #region ��������� ���������

    /// <summary>
    /// ��������� ���������.
    /// ���� - ��� �������, �������� - ���������
    /// </summary>
    private Dictionary<string, DBxTableStruct> _StructDict;

    /// <summary>
    /// ���������� ��������� ������� ��������� ��� ������������, � ����� ������
    /// "BinData" � "FileNames".
    /// � ��������� ������� ������ ��� ��������� ���� (��� ���������������
    /// ������������� ������� � DBxDocTypes): Id, Deleted, CreateUserId � �.�.
    /// � ��������� �� ������ ����, � ������� � ������������ ��� ���������� �� ������.
    /// ���� ��������� �������������� ��� �������, ������������ ����������.
    /// ���� � ������������ ��� ���������� �� �������� �������, ������������ ����������.
    /// </summary>
    /// <param name="tableName">��� ������� ��������� ��� ������������</param>
    /// <returns>������ �������, ���������� ���������. � ��� ������������ ���� Id </returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      //try
      //{
      lock (SyncRoot)
      {
        DBxTableStruct ts;
        if (!_StructDict.TryGetValue(tableName, out ts))
        {
          ts = DoGetTableStruct(tableName);
          _StructDict[tableName] = ts;
        }
        return ts;
      }
      //}
      //catch (Exception e)
      //{
      //  AddExceptionInfo(e);
      //  throw;
      //}
    }

    private DBxTableStruct DoGetTableStruct(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      DBxTableStruct ts = _BinDataInfo.GetTableStruct(tableName);
      if (ts != null)
        return ts;

      DBxTableStruct Struct1 = MainDBstruct.Tables[tableName];
      if (Struct1 == null)
        throw new ArgumentException("����������� ������� \"" + tableName + "\"", "tableName");

      if (_DBPermissions.TableModes[tableName] == DBxAccessMode.None)
        throw new DBxAccessException("������ � ������� \"" + tableName + "\" ��������");

      // ������� "���������" �����, �������� ����� ������������.
      DBxTableStruct Struct2 = new DBxTableStruct(tableName);
      for (int i = 0; i < Struct1.Columns.Count; i++)
      {
        if (_DBPermissions.ColumnModes[tableName, Struct1.Columns[i].ColumnName] == DBxAccessMode.None)
          continue;
        Struct2.Columns.Add(Struct1.Columns[i]);
      }
      Struct2.SetReadOnly();
      return Struct2;
    }

    #endregion

    /// <summary>
    /// ��������� �������� ����� ������ ��� �����������.
    /// ���� - ��� �������.
    /// �� ������ ��������� ��������� ����������� ��� ����������� ������������������
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    public DBxTableCacheInfo GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo Info;
      lock (SyncRoot)
      {
        if (_TableCacheInfos == null)
          _TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();

        if (!_TableCacheInfos.TryGetValue(tableName, out Info))
        {
          DBxTableStruct ts = GetTableStruct(tableName);
          if (ts == null)
            throw new ArgumentException("����������� ��� ������� \"" + tableName + "\"", "tableName");
          Info = new DBxTableCacheInfo(ts);

          DBxDocTypeBase DocTypeBase;
          if (_DocTypes.FindByTableName(tableName, out DocTypeBase))
          {
            if (!DocTypeBase.IndividualCacheColumns.AreAllDefaults)
            {
              for (int i = 0; i < Info.TableStruct.Columns.Count; i++)
              {
                string ColumnName = Info.TableStruct.Columns[i].ColumnName;
                if (DocTypeBase.Struct.Columns.Contains(ColumnName))
                  Info.IndividualColumnFlags[i] = DocTypeBase.IndividualCacheColumns[ColumnName];
                // ��� ����� ���� "Id" ��� ���������������� �������� � ��������� ����� DBxDocTypeBase.Struct
              }
            }
          }
          Info.SetReadOnly();
          _TableCacheInfos.Add(tableName, Info);
        }
      }
      return Info;
    }
  }
}
