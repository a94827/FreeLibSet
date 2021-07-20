using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Remoting;
using System.Data;
using System.Diagnostics;

namespace AgeyevAV.FIAS
{

  /// <summary>
  /// ���� ����� ����� �������������� �� ������� ������� � ����������� � ������� ����� ���������� FiasCachedSource � ��������, ���������� � �������
  /// </summary>
  public sealed class FiasDistributedSource : IFiasSource
  {
    #region �����������

    /// <summary>
    /// ������� ������, �������������� � ��������� �� �������.
    /// � �������� ��������� <paramref name="proxy"/> ������ ��������������
    /// ������, ����������� ������� FiasDB.Source.CreateProxy(), ���������� �� ����.
    /// </summary>
    /// <param name="proxy">������ ��� ������������. �� ����� ���� null</param>
    /// <param name="execProcList">���������� ���������� ��������. ��� ������� WinForms ������� �������� ������ �� EFPApp.ExecProcList</param>
    public FiasDistributedSource(FiasSourceProxy proxy, ExecProcCallList execProcList)
    {
#if DEBUG
      if (proxy == null)
        throw new ArgumentNullException("proxy");
      if (execProcList == null)
        throw new ArgumentNullException("execProcList");
#endif

      _BaseSource = proxy.Source;
      _ExecProcList = execProcList;

      _DBIdentity = proxy.DBIdentity;
      _DBSettings = proxy.DBSettings;
      _InternalSettings = proxy.InternalSettings;
      _DBStat = proxy.DBStat;
    }

    /// <summary>
    /// ������ ������� ������������ ���������� ������������, ����������� FiasSourceProxy.
    /// ���� ������� ������������ ������������, ����� �� ������� �������� ������ FIAS.dll �������� ����������
    /// � ������������ ���������� ������ FiasSourceProxy �������.
    /// ��������, �������� ���������� IFiasSource ������� ��� System.Object, �� �������� � �������� FIAS.dll, 
    /// ���� �� ��������� ��� ����� � ����������� ����. ����� ������� ����������� FiasCachedSource � FiasDistributedSource, ���������� ����
    /// �����������. ��� ����, ������, ����� ��������� ������ ��������� � ������� ��� ��������� ����������� ����������
    /// </summary>
    /// <param name="source">���������, ���������� � �������</param>
    /// <param name="execProcList">���������� ���������� ��������. ��� ������� WinForms ������� �������� ������ �� EFPApp.ExecProcList</param>
    public FiasDistributedSource(IFiasSource source, ExecProcCallList execProcList)
      : this(source.CreateProxy(), execProcList)
    {
    }

    #endregion

    #region ��������

    private IFiasSource _BaseSource;

    private ExecProcCallList _ExecProcList;

    /// <summary>
    /// ������������� ���� ������.
    /// ������������ � �������� ����� ����� ��� ����������� ������.
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    /// <summary>
    /// ��������� ���� ������
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// ���������� ��������� ��������������
    /// </summary>
    public FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private FiasInternalSettings _InternalSettings;

    /// <summary>
    /// ���������� ���������� �� ���� ������
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private FiasDBStat _DBStat;

    /// <summary>
    /// ���� �����������
    /// </summary>
    public DateTime ActualDate { get { return DBStat.ActualDate; } }

    /// <summary>
    /// ������������ ���������������� ������.
    /// ����� ����������� ����������� ����� ��������� �� �������, ���� ������ ���������� �� ������ � �������� � ������������ �������, ��������� � ����������.
    /// �������� ����� ��������������, ��������, ��� ������������� ������������.
    /// � �������� �������, �� ������� � �������, ������ �� ����������.
    /// �� ��������� - null.
    /// ������ ������ ���� �������������.
    /// </summary>
    public object UserData { get { return _UserData; } set { _UserData = value; } }
    private object _UserData;

    #endregion

    #region ������, ���������� ��������� ���������

    IDictionary<Guid, FiasGuidInfo> IFiasSource.GetGuidInfo(Guid[] guids, FiasTableType tableType)
    {
      // ��� ������������� �������������� ��� ������ ��������� ���-�� ����������
      if (guids.Length == 1)
        return _BaseSource.GetGuidInfo(guids, tableType);
      else
      {
        NamedValues args = new NamedValues();
        args["Action"] = FiasExecProcAction.GetGuidInfo;
        args["Guids"] = guids;
        args["TableType"] = tableType;
        NamedValues res = CallProc(args);
        return (IDictionary<Guid, FiasGuidInfo>)(res["Res"]);
      }
    }

    IDictionary<Guid, FiasGuidInfo> IFiasSource.GetRecIdInfo(Guid[] recIds, FiasTableType tableType)
    {
      if (recIds.Length == 1)
        return _BaseSource.GetRecIdInfo(recIds, tableType);
      else
      {
        NamedValues args = new NamedValues();
        args["Action"] = FiasExecProcAction.GetRecIdInfo;
        args["RecIds"] = recIds;
        args["TableType"] = tableType;
        NamedValues res = CallProc(args);
        return (IDictionary<Guid, FiasGuidInfo>)(res["Res"]);
      }
    }

    IDictionary<Guid, FiasCachedPageAddrOb> IFiasSource.GetAddrObPages(FiasLevel level, Guid[] pageAOGuids)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetAddrObPages;
      args["Level"] = level;
      args["PageAOGuids"] = pageAOGuids;
      NamedValues res = CallProc(args);
      return (IDictionary<Guid, FiasCachedPageAddrOb>)(res["Res"]);
    }

    FiasCachedPageSpecialAddrOb IFiasSource.GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetSpecialAddrObPage;
      args["PageType"] = pageType;
      args["PageAOGuid"] = pageAOGuid;
      NamedValues res = CallProc(args);
      return (FiasCachedPageSpecialAddrOb)(res["Res"]);
    }

    IDictionary<Guid, FiasCachedPageHouse> IFiasSource.GetHousePages(Guid[] pageAOGuids)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetHousePages;
      args["PageAOGuids"] = pageAOGuids;
      NamedValues res = CallProc(args);
      return (IDictionary<Guid, FiasCachedPageHouse>)(res["Res"]);
    }

    IDictionary<Guid, FiasCachedPageRoom> IFiasSource.GetRoomPages(Guid[] pageHouseGuids)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetRoomPages;
      args["PageHouseGuids"] = pageHouseGuids;
      NamedValues res = CallProc(args);
      return (IDictionary<Guid, FiasCachedPageRoom>)(res["Res"]);
    }

    FiasCachedAOTypes IFiasSource.GetAOTypes()
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetAOTypes;
      NamedValues res = CallProc(args);
      return (FiasCachedAOTypes)(res["Res"]);
    }

    DataSet IFiasSource.FindAddresses(FiasAddressSearchParams searchParams)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.FindAddresses;
      args["SearchParams"] = searchParams;
      NamedValues res = CallProc(args);
      return (DataSet)(res["Res"]);
    }

    DataSet IFiasSource.GetTableForGuid(Guid guid, FiasTableType tableType)
    {
      NamedValues args = new NamedValues();
      args["Action"] = FiasExecProcAction.GetTableForGuid;
      args["Guid"] = guid;
      args["TableType"] = tableType;
      NamedValues res = CallProc(args);
      return (DataSet)(res["Res"]);
    }

    private NamedValues CallProc(NamedValues args)
    {
      DistributedCallData cd = _BaseSource.StartDistributedCall(args, UserData);
      DistributedProcCallItem callItem = new DistributedProcCallItem(cd);
      callItem.DisplayName = "����� ������� ��� ��������� ������ ���� (" + args.GetString("Action") + ")";
      return _ExecProcList.ExecuteAsyncAndWait(callItem);
    }

    #endregion

    #region ������ � ��������, ������������ �������� �������

    DataTable IFiasSource.GetClassifUpdateTable()
    {
      return _BaseSource.GetClassifUpdateTable();
    }

    DistributedCallData IFiasSource.StartDistributedCall(NamedValues args, object userData)
    {
      return _BaseSource.StartDistributedCall(args, userData);
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
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDistributedSource.UpdateActualDate() started.");

      _BaseSource.UpdateActualDate(); // 28.10.2020

      DateTime newActualDate = _BaseSource.ActualDate;

      if (newActualDate != ActualDate)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDistributedSource.UpdateActualDate(). ActualData changed");

        FiasDBStat newStat = _BaseSource.DBStat;
        _DBStat = newStat;
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDistributedSource.UpdateActualDate() finished.");
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ������� ������, ������� ����� �������� ������������ FiasCachedSource
    /// </summary>
    /// <returns>������</returns>
    public FiasSourceProxy CreateProxy()
    {
      return new FiasSourceProxy(this, DBIdentity, DBSettings, InternalSettings, DBStat);
    }

    #endregion
  }
}
