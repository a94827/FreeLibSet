using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Caching;
using System.Data;
using AgeyevAV.Remoting;
using System.Diagnostics;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.FIAS
{

  // ������������� �������������� ������� ������� ������� �� ������-GUID�� �������� �������� (������, ���������)
  // � ���������, � ���� ������ ���� ����� ����������� ��������������. �� ���� �������������� AOID � AOGUID ����� ���������
  // ����� ���� ������� ��������� �������, ���� ������������ ������������� �����

  [StructLayout(LayoutKind.Auto)]
  internal struct FiasGuidKey : IEquatable<FiasGuidKey>
  {
    #region �����������

    public FiasGuidKey(Guid guid, FiasTableType tableType, bool isRecId)
    {
      _Guid = guid;
      _TableType = tableType;
      _IsRecId = isRecId;
    }

    #endregion

    #region ����

    private readonly Guid _Guid;
    private readonly FiasTableType _TableType;
    private readonly bool _IsRecId;

    #endregion

    #region ��� �������

    public static bool operator ==(FiasGuidKey a, FiasGuidKey b)
    {
      return a._Guid == b._Guid && a._TableType == b._TableType && a._IsRecId == b._IsRecId;
    }

    public static bool operator !=(FiasGuidKey a, FiasGuidKey b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if (obj is FiasGuidKey)
        return this == (FiasGuidKey)obj;
      else
        return false;
    }

    public bool Equals(FiasGuidKey other)
    {
      return (this == other);
    }

    public override int GetHashCode()
    {
      return _Guid.GetHashCode();
    }

    public override string ToString()
    {
      return (_IsRecId ? "RecId " : "GUID ") + _Guid.ToString() + " - " + _TableType.ToString();
    }

    #endregion
  }


  /// <summary>
  /// ���������� ��� ������� �������� ��������, ����� � ���������.
  /// �� ������������ � ���������� ����
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct FiasGuidInfo
  {
    #region �����������

    /// <summary>
    /// ������������� ���������
    /// </summary>
    /// <param name="level"></param>
    /// <param name="parentGuid"></param>
    public FiasGuidInfo(FiasLevel level, Guid parentGuid)
    {
      _Level = level;
      _ParentGuid = parentGuid;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������� ��������� �������.
    /// ��� ����� ������������ FiasLevel.House
    /// ��� ��������� ������������ FiasLevel.Flat
    /// </summary>
    public FiasLevel Level { get { return _Level; } }
    private readonly FiasLevel _Level;

    /// <summary>
    /// ������������� ������������� ��������� �������.
    /// ���� Level=FiasLevel.Flat, �� ������������� ����.
    /// ��� �������� ������������ Guid.Empty.
    /// </summary>
    public Guid ParentGuid { get { return _ParentGuid; } }
    private readonly Guid _ParentGuid;

    /// <summary>
    /// ��������� "GUID �� ������"
    /// </summary>
    public static readonly FiasGuidInfo NotFound = new FiasGuidInfo((FiasLevel)0, FiasTools.GuidNotFound);

    /// <summary>
    /// ��������� ������������� ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return _ParentGuid.ToString() + " (" + FiasEnumNames.ToString(_Level, false) + ")";
    }

    #endregion
  }

  #region IFiasSource

  /// <summary>
  /// ��������� ��� ��������� ������� ��������������.
  /// ������ ���������� �� ������ �������������� � ���������� ����
  /// </summary>
  public interface IFiasSource
  {
    /// <summary>
    /// ������������� ���� ������.
    /// ������������ � �������� ����� ����� ��� ����������� ������.
    /// </summary>
    string DBIdentity { get;}

    /// <summary>
    /// ��������� ���� ������
    /// </summary>
    FiasDBSettings DBSettings { get;}

    /// <summary>
    /// ���������� ��������� ��������������
    /// </summary>
    FiasInternalSettings InternalSettings { get; }

    /// <summary>
    /// ���� ������������ ��������������
    /// </summary>
    DateTime ActualDate { get; }


    /// <summary>
    /// ��������� ��������� ���� ������������.
    /// ���� ����� ����� ���������� ��������, ���� ���� �������������, ��� ���� ������������ ����� ����������
    /// </summary>
    void UpdateActualDate();

    /// <summary>
    /// ������� ����� ������ ��� �������� �� ������� � ������� ��� ������������ FiasCachedSource ��� ��������� ������������� ���������� �� ���� ����� �� ����,
    /// � �� �� ������
    /// </summary>
    /// <returns></returns>
    FiasSourceProxy CreateProxy();

    /// <summary>
    /// �������� ������� ��������� �������� ��������.
    /// � ������� ���� - GUID ��������� ��������� �������, ���� ���������, �������� ������� ����� ������� � GUID ������������� ��������� �������
    /// </summary>
    /// <param name="guids">�������������� �������� ��������, ��� ������� ����������� �����</param>
    /// <param name="tableType">��� �������. ���� ��� ������������ �������� ����������, ����� �������� Unknown</param>
    /// <returns>������� ���������</returns>
    IDictionary<Guid, FiasGuidInfo> GetGuidInfo(Guid[] guids, FiasTableType tableType);

    /// <summary>
    /// �������� ������� ������� ��������.
    /// � ������� ���� - ID ������ ��������� ��������� �������, ���� ���������, �������� ������� ����� ������� � GUID ������������� ��������� �������
    /// </summary>
    /// <param name="recIds">�������������� �������� ��������, ��� ������� ����������� �����</param>
    /// <param name="tableType">��� �������. ���� ��� ������������ �������� ����������, ����� �������� Unknown</param>
    /// <returns>������� ���������</returns>
    IDictionary<Guid, FiasGuidInfo> GetRecIdInfo(Guid[] recIds, FiasTableType tableType);

    /// <summary>
    /// �������� �������� �������������� ��� �������� ��������������� �������� ��������.
    /// </summary>
    /// <param name="level">������� �������� ��������, ������� ����� ��������� � �������</param>
    /// <param name="pageAOGuids">�������������� ������������ ��������, ��� ������� ����������� �����</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageAddrOb> GetAddrObPages(FiasLevel level, Guid[] pageAOGuids);

    /// <summary>
    /// �������� ����������� �������� ��������������.
    /// ������������ ���������� ������
    /// </summary>
    /// <param name="pageType">��� ��������</param>
    /// <param name="pageAOGuid">������������� ������������� �������</param>
    /// <returns></returns>
    FiasCachedPageSpecialAddrOb GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid);

    /// <summary>
    /// �������� �������� �������������� ����� ��� �������� ��������������� �������� ��������.
    /// </summary>
    /// <param name="pageAOGuids">�������������� ������������ ��������, ��� ������� ����������� �����</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageHouse> GetHousePages(Guid[] pageAOGuids);

    /// <summary>
    /// �������� �������� �������������� ��������� ��� �������� ��������������� �����.
    /// </summary>
    /// <param name="pageHouseGuids">�������������� ������������ �������� (�����), ��� ������� ����������� �����</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageRoom> GetRoomPages(Guid[] pageHouseGuids);

    /// <summary>
    /// ��������� �������������� ������� ����������
    /// </summary>
    /// <returns>�������������� ������</returns>
    FiasCachedAOTypes GetAOTypes();

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    /// <param name="searchParams">��������� ������</param>
    /// <returns>���������� ������</returns>
    DataSet FindAddresses(FiasAddressSearchParams searchParams);

    /// <summary>
    /// ��������� ������� ��� ��������� "�����������" ��������� �������, ���� ��� ���������
    /// </summary>
    /// <param name="guid">�������� AOGUID, HOUSEGUID ��� ROOMGUID</param>
    /// <param name="tableType">��� �������</param>
    /// <returns>������� ������</returns>
    DataSet GetTableForGuid(Guid guid, FiasTableType tableType);

    /// <summary>
    /// ���������� ���������� �� ���� ������ ��������������.
    /// </summary>
    FiasDBStat DBStat { get;}

    /// <summary>
    /// ��������� ������� ������� ���������� ��������������
    /// </summary>
    /// <returns></returns>
    DataTable GetClassifUpdateTable();

    /// <summary>
    /// �������� "�����������" ������ ��������� �� �������.
    /// ������������ FiasDistributedSource
    /// </summary>
    /// <param name="args">����������� ������ �������</param>
    /// <param name="userData">�������� FiasDistributedSource.UserData</param>
    /// <returns>���������� ������������ �������</returns>
    DistributedCallData StartDistributedCall(NamedValues args, object userData);
  }

  #endregion

  /// <summary>
  /// �������� �������������� �� ������� � �������.
  /// ��������� ������������ FiasCachedSource. ��������� ������� IFiasSource.CreateProxy().
  /// �� �������� �������, ��������� �� ����������� ����
  /// </summary>
  [Serializable]
  public sealed class FiasSourceProxy
  {
    #region ���������� �����������

    internal FiasSourceProxy(IFiasSource source, string dbIdentity, FiasDBSettings dbSettings,
      FiasInternalSettings internalSettings, FiasDBStat dbStat)
    {
      _Source = source;
      _DBIdentity = dbIdentity;
      _DBSettings = dbSettings;
      _InternalSettings = internalSettings;
      _DBStat = dbStat;
    }

    #endregion

    #region ��������

    internal IFiasSource Source { get { return (IFiasSource)_Source; } }
    private readonly object _Source;

    internal string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    internal FiasDBSettings DBSettings { get { return (FiasDBSettings)_DBSettings; } }
    private readonly object _DBSettings;

    internal FiasInternalSettings InternalSettings { get { return (FiasInternalSettings)_InternalSettings; } }
    private readonly object _InternalSettings;

    internal FiasDBStat DBStat { get { return _DBStat; } }
    private readonly FiasDBStat _DBStat;

    #endregion
  }

  /// <summary>
  /// ����������� ������� ��������������.
  /// ��������� ���������������� ����� � ����� ���������� �� ������� �������. ������ �������� �� �������� ����� �������.
  /// �� ������� ������� ��������� ������������� � FiasDB
  /// ���������������� ������ ��� net remoting.
  /// ����� �������� ����������������.
  /// </summary>
  public class FiasCachedSource : MarshalByRefDisposableObject, IFiasSource
  {
    #region �����������

    /// <summary>
    /// ������� ������, �������������� � ������� ���������.
    /// ��� �������� �� ������� �������, � �������� ��������� <paramref name="proxy"/> ������ ��������������
    /// ������, ����������� ������� FiasDB.Source.CreateProxy(), ���������� �� ����.
    /// </summary>
    /// <param name="proxy">������ ��� ������������. �� ����� ���� null</param>
    public FiasCachedSource(FiasSourceProxy proxy)
    {
#if DEBUG
      if (proxy == null)
        throw new ArgumentNullException("proxy");
#endif

      _BaseSource = proxy.Source;

      _DBIdentity = proxy.DBIdentity;
      _DBSettings = proxy.DBSettings;
      _InternalSettings = proxy.InternalSettings;
      _DBStat = proxy.DBStat;


      _CacheFirstKey = DataTools.MD5SumFromString(_DBIdentity); // ��� ����� ���� ��������� �������
      _CacheFirstKeySimpleArray = new string[1] { _CacheFirstKey };

      _GuidDict = new DictionaryWithMRU<FiasGuidKey, FiasGuidInfo>();
      _GuidDict.MaxCapacity = 10000;

      _TextCache = new FiasAddressTextCache(this);

      SetCacheVersion();
    }

    /// <summary>
    /// ������ ������� ������������ ���������� ������������, ����������� FiasSourceProxy.
    /// ���� ������� ������������ ������������, ����� �� ������� �������� ������ FIAS.dll �������� ����������
    /// � ������������ ���������� ������ FiasSourceProxy �������.
    /// ��������, �������� ���������� IFiasSource ������� ��� System.Object, �� �������� � �������� FIAS.dll, 
    /// ���� �� ��������� ��� ����� � ����������� ����. ����� ������� ����������� FiasCachedSource, ���������� ����
    /// �����������. ��� ����, ������, ����� ��������� ������ ��������� � ������� ��� ��������� ����������� ����������
    /// </summary>
    /// <param name="source"></param>
    public FiasCachedSource(IFiasSource source)
      : this(source.CreateProxy())
    {
    }

    #endregion

    #region ��������

    private readonly IFiasSource _BaseSource;

    /// <summary>
    /// ������������� ���� ������.
    /// ������������ � �������� ����� ����� ��� ����������� ������.
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    /// <summary>
    /// ������ ���� ��� �����������.
    /// ������ ����� ������������ ������� ������ � Cache.Params.PersistDir.
    /// </summary>
    public string CacheFirstKey { get { return _CacheFirstKey; } }
    private readonly string _CacheFirstKey;

    /// <summary>
    /// ������ �� ������ �������� _CacheFirstKey
    /// </summary>
    private readonly string[] _CacheFirstKeySimpleArray;

    /// <summary>
    /// ��������� ���� ������
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// ���������� ��������� ��������������
    /// </summary>
    public FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private readonly FiasInternalSettings _InternalSettings;

    #endregion

    #region ������� GUID'��

    private readonly DictionaryWithMRU<FiasGuidKey, FiasGuidInfo> _GuidDict;

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="guids"></param>
    /// <param name="tableType"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasGuidInfo> GetGuidInfo(Guid[] guids, FiasTableType tableType)
    {
      IDictionary<Guid, FiasGuidInfo> dict;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetGuidInfo() started. tableType=" + tableType.ToString() + ", guids.Length=" + guids.Length.ToString());

      dict = DoGetGuidOrRecIdInfo(guids, tableType, false);

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetGuidInfo() finished");

      return dict;
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="recIds"></param>
    /// <param name="tableType"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasGuidInfo> GetRecIdInfo(Guid[] recIds, FiasTableType tableType)
    {
      IDictionary<Guid, FiasGuidInfo> dict;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRecIdInfo() started. tableType=" + tableType.ToString() + ", recIds.Length=" + recIds.Length.ToString());

      dict = DoGetGuidOrRecIdInfo(recIds, tableType, true);

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRecIdInfo() finished");

      return dict;
    }

    private IDictionary<Guid, FiasGuidInfo> DoGetGuidOrRecIdInfo(Guid[] guids, FiasTableType tableType, bool isRecIds)
    {
      Dictionary<Guid, FiasGuidInfo> dict = new Dictionary<Guid, FiasGuidInfo>(guids.Length);

      #region ����� � ����

      SingleScopeList<Guid> wanted = null;

      lock (_GuidDict)
      {
        for (int i = 0; i < guids.Length; i++)
        {
          FiasGuidKey key = new FiasGuidKey(guids[i], tableType, isRecIds);
          FiasGuidInfo info;
          if (_GuidDict.TryGetValue(key, out info))
            dict[guids[i]] = info; // �� ���������� dict.Add(), ��� ��� � ������� guids ����� ���� �������
          else
          {
            // ��� � ����
            if (wanted == null)
              wanted = new SingleScopeList<Guid>();
            wanted.Add(guids[i]);
          }
        }
      }

      #endregion

      #region ������ ����������� ��������

      if (wanted != null)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.DoGetGuidOrRecIdInfo(). Ids found in cache: " + (guids.Length - wanted.Count).ToString() + ", required from BaseSource: " + wanted.Count.ToString());

        IDictionary<Guid, FiasGuidInfo> dict2;
        if (isRecIds)
          dict2 = _BaseSource.GetRecIdInfo(wanted.ToArray(), tableType);
        else
          dict2 = _BaseSource.GetGuidInfo(wanted.ToArray(), tableType);

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.DoGetGuidOrRecIdInfo(). BaseSource query completed");

        lock (_GuidDict)
        {
          foreach (KeyValuePair<Guid, FiasGuidInfo> pair2 in dict2)
          {
            FiasGuidKey key = new FiasGuidKey(pair2.Key, tableType, isRecIds);
            _GuidDict[key] = pair2.Value;
            dict[pair2.Key] = pair2.Value;
          }
        }
      }
      else
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.DoGetGuidOrRecIdInfo(). All ids found in cache, no BaseSource required");
      }

      #endregion

      return dict;
    }

    #endregion

    #region �������������� ��������

    #region FiasCachedPageAddrOb

    private string[] GetAddrObPageKeys(FiasLevel level, Guid pageAOGuid)
    {
      return new string[] { _CacheFirstKey, level.ToString(), pageAOGuid.ToString("N") };
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="pageAOGuids"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasCachedPageAddrOb> GetAddrObPages(FiasLevel level, Guid[] pageAOGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages() started. level=" + level.ToString() + ", pageAOGuids.Length=" + pageAOGuids.Length.ToString());

      Dictionary<Guid, FiasCachedPageAddrOb> dict = new Dictionary<Guid, FiasCachedPageAddrOb>();

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "����� ������� �������� �������� \""+FiasEnumNames.ToString(level, false)+"\" ("+pageAOGuids.Length.ToString()+") � ����",
        "������ ����������� ������� � ���� ������",
        "������ ������� � ���"});
      try
      {

        #region ����� � ����

        SingleScopeList<Guid> wanted = null;

        spl.PercentMax = pageAOGuids.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < pageAOGuids.Length; i++)
        {
          FiasCachedPageAddrOb page = Cache.GetItemIfExists<FiasCachedPageAddrOb>(GetAddrObPageKeys(level, pageAOGuids[i]), CachePersistance.MemoryAndPersist);
          if (page == null)
          {
            if (wanted == null)
              wanted = new SingleScopeList<Guid>();
            wanted.Add(pageAOGuids[i]);
          }
          else
            dict[pageAOGuids[i]] = page;

          spl.IncPercent();
        }

        #endregion

        #region ������ ����������� �������

        if (wanted != null)
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages(). Pages found in cache: " + (pageAOGuids.Length - wanted.Count).ToString() + ", required from BaseSource: " + wanted.Count.ToString());

          IDictionary<Guid, FiasCachedPageAddrOb> dict2 = _BaseSource.GetAddrObPages(level, wanted.ToArray());
          spl.Complete();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages(). Writing pages to cache: " + dict2.Count.ToString());

          spl.PercentMax = dict2.Count;
          spl.AllowCancel = true;
          int cnt = 0;
          foreach (KeyValuePair<Guid, FiasCachedPageAddrOb> pair2 in dict2)
          {
            if (Cache.SetItemIfNew<FiasCachedPageAddrOb>(GetAddrObPageKeys(level, pair2.Key),
              CachePersistance.MemoryAndPersist, pair2.Value))
              cnt++;
            pair2.Value.AddToGuidDict(_GuidDict);
            dict[pair2.Key] = pair2.Value;
            spl.IncPercent();
          }

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages(). Written to cache pages: " + cnt.ToString() + ", skipped appeared: " + (dict2.Count - cnt).ToString());
        }
        else
        {
          spl.Skip();
          spl.Skip();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages(). All pages found in cache, no BaseSource required");
        }

        #endregion
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAddrObPages() finished");

      return dict;
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="pageType"></param>
    /// <param name="pageAOGuid"></param>
    /// <returns></returns>
    public FiasCachedPageSpecialAddrOb GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage() started. pageType=" + pageType.ToString());

      FiasCachedPageSpecialAddrOb page = Cache.GetItemIfExists<FiasCachedPageSpecialAddrOb>(GetAddrObPageKeys(FiasTools.GetSpecialPageLevel(pageType), pageAOGuid), CachePersistance.MemoryAndPersist);
      if (page == null)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage(). BaseSource request needed.");

        page = _BaseSource.GetSpecialAddrObPage(pageType, pageAOGuid);

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage(). Writing page to cache");

        if (Cache.SetItemIfNew<FiasCachedPageSpecialAddrOb>(GetAddrObPageKeys(FiasTools.GetSpecialPageLevel(pageType), pageAOGuid),
          CachePersistance.MemoryAndPersist, page))
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage(). Page written to cache");
        }
        else
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage(). Page skipped, because it appeared in the cache");
        }
      }
      else
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage(). Page found in cache. No BaseSource required");
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetSpecialAddrObPage() finished");

      return page;
    }

    #endregion

    #region FiasCachedPageHouse

    private string[] GetHousePageKeys(Guid pageAOGuid)
    {
      return new string[] { _CacheFirstKey, pageAOGuid.ToString("N") };
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="pageAOGuids"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasCachedPageHouse> GetHousePages(Guid[] pageAOGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages() started. pageAOGuids.Length=" + pageAOGuids.Length.ToString());

      Dictionary<Guid, FiasCachedPageHouse> dict = new Dictionary<Guid, FiasCachedPageHouse>();

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "����� ������� ������ ("+pageAOGuids.Length.ToString()+") � ����",
        "������ ����������� ������� � ���� ������",
        "������ ������� � ���"});
      try
      {
        #region ����� � ����

        SingleScopeList<Guid> wanted = null;

        spl.PercentMax = pageAOGuids.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < pageAOGuids.Length; i++)
        {
          FiasCachedPageHouse page = Cache.GetItemIfExists<FiasCachedPageHouse>(GetHousePageKeys(pageAOGuids[i]), CachePersistance.MemoryAndPersist);
          if (page == null)
          {
            if (wanted == null)
              wanted = new SingleScopeList<Guid>();
            wanted.Add(pageAOGuids[i]);
          }
          else
            dict[pageAOGuids[i]] = page;
          spl.IncPercent();
        }

        spl.Complete();

        #endregion

        #region ������ ����������� �������

        if (wanted != null)
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages(). Pages found in cache: " + (pageAOGuids.Length - wanted.Count).ToString() + ", required from BaseSource: " + wanted.Count.ToString());

          IDictionary<Guid, FiasCachedPageHouse> dict2 = _BaseSource.GetHousePages(wanted.ToArray());
          spl.Complete();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages(). Pages writing to cache: " + dict2.Count.ToString());

          spl.PercentMax = dict2.Count;
          spl.AllowCancel = true;
          int cnt = 0;
          foreach (KeyValuePair<Guid, FiasCachedPageHouse> pair2 in dict2)
          {
            if (Cache.SetItemIfNew<FiasCachedPageHouse>(GetHousePageKeys(pair2.Key),
              CachePersistance.MemoryAndPersist, pair2.Value))
              cnt++;
            pair2.Value.AddToGuidDict(_GuidDict);
            dict[pair2.Key] = pair2.Value;
            spl.IncPercent();
          }

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages(). Pages wriiten to cache: " + cnt.ToString() + ", skipped appeared: " + (dict2.Count - cnt).ToString());
        }
        else
        {
          spl.Skip();
          spl.Skip();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages(). All pages found in cache. No BaseSource required");
        }

        #endregion
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages() finished");

      return dict;
    }

    #endregion

    #region FiasCachedPageRoom

    private string[] GetRoomPageKeys(Guid pageHouseGuid)
    {
      return new string[] { _CacheFirstKey, pageHouseGuid.ToString("N") };
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="pageHouseGuids"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasCachedPageRoom> GetRoomPages(Guid[] pageHouseGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages() started. pageHouseGuids.Length=" + pageHouseGuids.Length.ToString());

      Dictionary<Guid, FiasCachedPageRoom> dict = new Dictionary<Guid, FiasCachedPageRoom>();

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "����� ������� ��������� ("+pageHouseGuids.Length.ToString()+") � ����",
        "������ ����������� ������� � ���� ������",
        "������ ������� � ���"});
      try
      {
        #region ����� � ����

        SingleScopeList<Guid> wanted = null;

        spl.PercentMax = pageHouseGuids.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < pageHouseGuids.Length; i++)
        {
          FiasCachedPageRoom page = Cache.GetItemIfExists<FiasCachedPageRoom>(GetRoomPageKeys(pageHouseGuids[i]), CachePersistance.MemoryAndPersist);
          if (page == null)
          {
            if (wanted == null)
              wanted = new SingleScopeList<Guid>();
            wanted.Add(pageHouseGuids[i]);
          }
          else
            dict[pageHouseGuids[i]] = page;

          spl.IncPercent();
        }

        #endregion

        #region ������ ����������� �������

        if (wanted != null)
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages(). Pages found in cache: " + (pageHouseGuids.Length - wanted.Count).ToString() + ", required from BaseSource: " + wanted.Count.ToString());

          IDictionary<Guid, FiasCachedPageRoom> dict2 = _BaseSource.GetRoomPages(wanted.ToArray());
          spl.Complete();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages(). Pages writing to cache: " + dict2.Count.ToString());

          spl.PercentMax = dict2.Count;
          spl.AllowCancel = true;
          int cnt = 0;
          foreach (KeyValuePair<Guid, FiasCachedPageRoom> pair2 in dict2)
          {
            if (Cache.SetItemIfNew<FiasCachedPageRoom>(GetRoomPageKeys(pair2.Key),
              CachePersistance.MemoryAndPersist, pair2.Value))
              cnt++;
            pair2.Value.AddToGuidDict(_GuidDict);
            dict[pair2.Key] = pair2.Value;

            spl.IncPercent();
          }

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages(). Pages written to cache: " + cnt.ToString() + ", skipped appeared: " + (dict2.Count - cnt).ToString());
        }
        else
        {
          spl.Skip();
          spl.Skip();

          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages(). All pages found in cache. No BaseSource required");
        }

        #endregion

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages() finished");

        return dict;
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }
    }

    #endregion

    #endregion

    #region ������� ����������

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <returns></returns>
    public FiasCachedAOTypes GetAOTypes()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes() started.");

      FiasCachedAOTypes page = Cache.GetItemIfExists<FiasCachedAOTypes>(new string[] { _CacheFirstKey }, CachePersistance.MemoryAndPersist);
      if (page == null)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes(). BaseSource request needed");

        page = _BaseSource.GetAOTypes();

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes(). Writting page to cache");

        if (Cache.SetItemIfNew<FiasCachedAOTypes>(new string[] { _CacheFirstKey }, CachePersistance.MemoryAndPersist, page))
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes(). Page written to cache");
        }
        else
        {
          if (FiasTools.TraceSwitch.Enabled)
            Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes(). Page skipped, because it appeared in the cache");
        }
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetAOTypes() finished");

      return page;
    }

    #endregion

    #region ��������� ������������� ������

    /// <summary>
    /// ����������� ��������� ������������� ��� ������������ ����� ������
    /// </summary>
    public FiasAddressTextCache TextCache { get { return _TextCache; } }
    private FiasAddressTextCache _TextCache;

    #endregion

    #region ������ ������

    /// <summary>
    /// ��������� ������� ����.
    /// ��������� ��� ������ ��� �������� �������. ��� BaseSource ������� �� �����������
    /// </summary>
    public void ClearCache()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.ClearCache() started.");

      Cache.Clear<FiasCachedAOTypes>(_CacheFirstKeySimpleArray);
      Cache.Clear<FiasCachedPageAddrOb>(_CacheFirstKeySimpleArray);
      Cache.Clear<FiasCachedPageSpecialAddrOb>(_CacheFirstKeySimpleArray);
      Cache.Clear<FiasCachedPageHouse>(_CacheFirstKeySimpleArray);
      Cache.Clear<FiasCachedPageRoom>(_CacheFirstKeySimpleArray);
      _GuidDict.Clear();

      _TextCache.Clear();

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.ClearCache() finished.");
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="searchParams"></param>
    /// <returns></returns>
    public DataSet FindAddresses(FiasAddressSearchParams searchParams)
    {
      return _BaseSource.FindAddresses(searchParams);
    }

    /// <summary>
    /// �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="tableType"></param>
    /// <returns></returns>
    public DataSet GetTableForGuid(Guid guid, FiasTableType tableType)
    {
      return _BaseSource.GetTableForGuid(guid, tableType);
    }

    /// <summary>
    /// ������� ������, ������� ����� ���� ������� ������������ FiasCachedSource ��� ����������� ������� �������
    /// </summary>
    /// <returns></returns>
    public FiasSourceProxy CreateProxy()
    {
      return new FiasSourceProxy(this, _DBIdentity, _DBSettings, _InternalSettings, _DBStat);
    }


    /// <summary>
    /// ��������� ������� ������� ���������� ��������������
    /// </summary>
    /// <returns></returns>
    public DataTable GetClassifUpdateTable()
    {
      return _BaseSource.GetClassifUpdateTable();
    }

    /// <summary>
    /// �������� ����� �������� ���������
    /// </summary>
    /// <param name="args"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    public virtual DistributedCallData StartDistributedCall(NamedValues args, object userData)
    {
      return _BaseSource.StartDistributedCall(args, userData);
    }

    #endregion

    #region ���� ������������ � ����������

    /// <summary>
    /// ���������� ���������� �� ���� ������
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private FiasDBStat _DBStat;

    /// <summary>
    /// ���� �����������
    /// </summary>
    public DateTime ActualDate { get { return DBStat.ActualDate; } }

    private void SetCacheVersion()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.SetCacheVersion() started.");

      //string version = StdConvert.ToString(_ActualDate, false);
      string version = ActualDate.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.SetCacheVersion(). Version=\"" + version + "\"");

      CacheSetVersionResult r = Cache.SetVersion(typeof(FiasCachedPageAddrOb), _CacheFirstKeySimpleArray, version);
      Cache.SyncVersion(typeof(FiasCachedPageSpecialAddrOb), _CacheFirstKeySimpleArray, r);
      Cache.SyncVersion(typeof(FiasCachedPageHouse), _CacheFirstKeySimpleArray, r);
      Cache.SyncVersion(typeof(FiasCachedPageRoom), _CacheFirstKeySimpleArray, r);
      Cache.SyncVersion(typeof(FiasCachedAOTypes), _CacheFirstKeySimpleArray, r);

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.SetCacheVersion() finished.");
    }

    /// <summary>
    /// ���������� ������ � �������� ������.
    /// ������������ ����������� �������. ������ �����, ��������, �� ������� ����������� � ������� ���� ������������ ��������������
    /// � �������� ���� �����, ���� ���� ����������.
    /// ����� ����������� � ��������� �������� ActualDate. ���� ���� ����������� ��������� � �������, ������� �������� �� �����������.
    /// ����� ����������� ������� ���� � ����������� �������� FiasCachedSource.ActualDate � DBStat
    /// </summary>
    public void UpdateActualDate()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.UpdateActualDate() started.");

      _BaseSource.UpdateActualDate(); // 28.10.2020, 04.11.2020

      DateTime newActualDate = _BaseSource.ActualDate;

      if (newActualDate != ActualDate)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.UpdateActualDate(). ActualData changed");

        FiasDBStat newStat = _BaseSource.DBStat;
        _DBStat = newStat;
        SetCacheVersion();
        ClearCache();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.UpdateActualDate() finished.");
    }

    #endregion
  }

  #region ������������ FiasSpecialPageType

  /// <summary>
  /// ����������� ���� �������, ������� ����� ��������� �� ��������������
  /// </summary>
  [Serializable]
  public enum FiasSpecialPageType
  {
    /// <summary>
    /// ��� ������ (� �������� ��)
    /// </summary>
    AllCities,

    /// <summary>
    /// ������ ���� �������� (� �������� ��)
    /// </summary>
    AllDistricts,


    /// <summary>
    /// ���������� ������ - ������� ������� (� �������� �������)
    /// </summary>
    DistrictCapitals,
  }

  #endregion

  /// <summary>
  /// ����������� ���������� ������������� ��� ������������ ����� ������.
  /// �� ����� ���������� ������ ������ � ������� FiasAddressConvert, � �� ������ ���������� ������ FiasHander.Format().
  /// ����� �������� ����������������.
  /// ��������������� ������ ��������������. ��� ��� ������������ ������ ������.
  /// </summary>
  public sealed class FiasAddressTextCache
  {
    #region �����������

    internal FiasAddressTextCache(FiasCachedSource source)
    {
      _Source = source;

      _ConvertWithoutFill = new FiasAddressConvert(source);
      _ConvertWithoutFill.FillAddress = false;

      _Handler = new FiasHandler(source);
      _SB = new StringBuilder();

      _Dict = new DictionaryWithMRU<string, string>();
      _Dict.MaxCapacity = 10000;
    }

    #endregion

    #region ��������

    private FiasCachedSource _Source;

    /// <summary>
    /// �������� ������� ����.
    /// ���� - ������ ���� "���������|������", �������� - ��������� �������������.
    /// ��� ���� ���������� ������������ ���������� ����� �������
    /// </summary>
    private DictionaryWithMRU<string, string> _Dict;

    /// <summary>
    /// ������� ������.
    /// �� ��������� ����� 10000 �������
    /// </summary>
    public int Capacity
    {
      get { lock (_Dict) { return _Dict.MaxCapacity; } }
      set { lock (_Dict) { _Dict.MaxCapacity = value; } }
    }

    /// <summary>
    /// ������������ ��� �������������� ������������ ������ � FiasAddress.
    /// �������� FiasAddressConvert.FillAddress=false.
    /// </summary>
    private FiasAddressConvert _ConvertWithoutFill;

    /// <summary>
    /// ������������ ��� ������ ������ Format
    /// </summary>
    private FiasHandler _Handler;

    /// <summary>
    /// ��� ������ ���������������� ������
    /// </summary>
    private StringBuilder _SB;

    #endregion

    #region ��������� ������

    /// <summary>
    /// ��������� ������ ��� ������ ������
    /// </summary>
    /// <param name="addressCode">������������ ������ ������</param>
    /// <param name="format">������</param>
    /// <returns>������������ ��������� �������������</returns>
    public string Format(string addressCode, string format)
    {
      if (String.IsNullOrEmpty(addressCode))
        return String.Empty;

      string dictKey = addressCode + "|" + format; // ���� ��� �������

      string text;
      lock (_Dict)
      {
        if (_Dict.TryGetValue(dictKey, out text))
          return text;
      }

      lock (_Handler)
      {
        FiasAddress address;
        if (_ConvertWithoutFill.TryParse(addressCode, out address)) 
        {
          _Handler.FillAddress(address);

          _SB.Length = 0;
          _Handler.Format(_SB, address, format);
          text = _SB.ToString();
        }
        else
          text = String.Empty;
      }

      lock (_Dict)
      {
        _Dict[dictKey] = text; // ��� ���� ������������ �����
      }

      return text;
    }

    /// <summary>
    /// ��������� ���������� ������������� ��� ���������� �������.
    /// ��� ���� ������� ������������ ���� ������ ��� ���������� �������������
    /// </summary>
    /// <param name="addressCodes">������ ������������ ����� ������</param>
    /// <param name="format">������</param>
    /// <returns>������ ������������ ��������� �������������</returns>
    public string[] Format(string[] addressCodes, string format)
    {
      string[] textArray = new string[addressCodes.Length];

      // ����������� ������
      SingleScopeStringList notFound = null;

      lock (_Dict)
      {
        for (int i = 0; i < addressCodes.Length; i++)
        {
          if (String.IsNullOrEmpty(addressCodes[i]))
            textArray[i] = String.Empty;
          else
          {
            string dictKey = addressCodes[i] + "|" + format; // ���� ��� �������
            string text;
            if (_Dict.TryGetValue(dictKey, out text))
              textArray[i] = text;
            else
            {
              if (notFound == null)
                notFound = new SingleScopeStringList(false);
              notFound.Add(addressCodes[i]);
            }
          }
        }

      }

      if (notFound != null)
      {
        Dictionary<string, FiasAddress> addrDict = new Dictionary<string, FiasAddress>(notFound.Count);

        lock (_Handler)
        {
          foreach (string s in notFound)
          {
            FiasAddress address;
            if (_ConvertWithoutFill.TryParse(s, out address)) // �� �������� ����� FillAddress()
              addrDict.Add(s, address);
          }
        }

        // ��������� ����� FillAddresses()
        if (addrDict.Count > 0) // �������� �����, �.�. ��� ������ ����� ���� � ��������
        {
          FiasAddress[] a = new FiasAddress[addrDict.Count];
          addrDict.Values.CopyTo(a, 0);
          _Handler.FillAddresses(a);
        }


        lock (_Dict)
        {
          for (int i = 0; i < addressCodes.Length; i++)
          {
            if (textArray[i] == null)
            {
              FiasAddress address;
              if (addrDict.TryGetValue(addressCodes[i], out address))
              {
                _SB.Length = 0;
                _Handler.Format(_SB, address, format);
                textArray[i] = _SB.ToString();
              }
              else
                textArray[i] = String.Empty;
            }
            string dictKey = addressCodes[i] + "|" + format; // ���� ��� �������
            _Dict[dictKey] = textArray[i]; // ��� ���� ������������ �����
          }
        }
      }
      return textArray;
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ������� ���
    /// </summary>
    public void Clear()
    {
      lock (_Dict)
      {
        _Dict.Clear();
      }
    }

    #endregion
  }
}
