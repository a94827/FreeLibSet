// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// ���������������� �������� ������ ��� �������� ������� ��������������.
  /// �������� �� ���������� SQL-�������� SELECT.
  /// ��������� � ������������ FiasDB.
  /// </summary>
  internal class FiasDBUnbufferedSource : IFiasSource
  {
    #region �����������

    public FiasDBUnbufferedSource(FiasDB fiasDB)
    {
      _FiasDB = fiasDB;
      CreateTemplateTables();
    }

    #endregion

    #region ��������

    private FiasDB _FiasDB;

    #endregion

    #region ������� c������� IFiasSource

    public string DBIdentity { get { return _FiasDB.DB.DBIdentity; } }

    public FiasDBSettings DBSettings { get { return _FiasDB.DBSettings; } }

    public FiasInternalSettings InternalSettings { get { return _FiasDB.InternalSettings; } }

    public DateTime ActualDate { get { return _FiasDB.ActualDate; } }

    public FiasSourceProxy CreateProxy()
    {
      return new FiasSourceProxy(this, DBIdentity, DBSettings, InternalSettings, DBStat);
    }

    public FiasDBStat DBStat { get { return _FiasDB.DBStat; } }

    FreeLibSet.Remoting.DistributedCallData IFiasSource.StartDistributedCall(FreeLibSet.Remoting.NamedValues args, object userData)
    {
      throw new InvalidOperationException("���� ����� ������ ���������� ��� FiasDB.Source");
    }

    #endregion

    #region ��������� �������� ���������������

    public IDictionary<Guid, FiasGuidInfo> GetGuidInfo(Guid[] guids, FiasTableType tableType)
    {
#if DEBUG
      if (guids == null)
        throw new ArgumentNullException("guids");
#endif

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetGuidInfo() started. tableType=" + tableType.ToString() + ", guids.Length=" + guids.Length.ToString());

      _FiasDB.CheckIsReady();

      Dictionary<Guid, FiasGuidInfo> dict = new Dictionary<Guid, FiasGuidInfo>();

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        #region AddrOb

        if (tableType == FiasTableType.AddrOb || tableType == FiasTableType.Unknown)
        {
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� ������������ �������� �������� ��� ��������������� (" + guids.Length.ToString() + ")");
          try
          {
            spl.PercentMax = guids.Length;
            spl.AllowCancel = true;
            foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(guids, 500))
            {
              DBxFilter filter = new ValuesFilter("AOGUID", guids2);
              if (_FiasDB.DBSettings.UseHistory)
                filter = new AndFilter(filter, new ValueFilter("Actual", true));
              using (DbDataReader rdr = con.ReaderSelect("AddrOb",
                new DBxColumns("AOGUID,PARENTGUID,AOLEVEL"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid Child = DataTools.GetGuid(rdr.GetValue(0));
                  Guid Parent = DataTools.GetGuid(rdr.GetValue(1));
                  FiasLevel Level = (FiasLevel)DataTools.GetInt(rdr.GetValue(2));
                  dict[Child] = new FiasGuidInfo(Level, Parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += guids2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion

        #region House

        if (_FiasDB.DBSettings.UseHouse && (tableType == FiasTableType.House || tableType == FiasTableType.Unknown))
        {
          guids = RemoveFoundGuids(guids, dict); // ������ ��������� �������� �������
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� �������� �������� ��� ��������������� ������ (" + guids.Length.ToString() + ")");
          try
          {
            spl.PercentMax = guids.Length;
            spl.AllowCancel = true;
            foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(guids, 500))
            {
              DBxFilter filter = new ValuesFilter("HOUSEGUID", guids2);
              using (DbDataReader rdr = con.ReaderSelect("House",
                new DBxColumns("HOUSEGUID,AOGUID"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid Child = rdr.GetGuid(0);
                  Guid Parent = rdr.GetGuid(1);
                  dict[Child] = new FiasGuidInfo(FiasLevel.House, Parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += guids2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion

        #region Room

        if (_FiasDB.DBSettings.UseRoom && (tableType == FiasTableType.Room || tableType == FiasTableType.Unknown))
        {
          guids = RemoveFoundGuids(guids, dict); // ������ ��������� ����
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� ����� ��� ��������������� ��������� (" + guids.Length.ToString() + ")");
          try
          {
            spl.PercentMax = guids.Length;
            spl.AllowCancel = true;
            foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(guids, 500))
            {
              DBxFilter filter = new ValuesFilter("ROOMGUID", guids2);
              using (DbDataReader rdr = con.ReaderSelect("Room",
                new DBxColumns("ROOMGUID,HOUSEGUID"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid Child = rdr.GetGuid(0);
                  Guid Parent = rdr.GetGuid(1);
                  dict[Child] = new FiasGuidInfo(FiasLevel.Flat, Parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += guids2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion
      }

      #region ��������� - �� �������

      if (guids.Length > 0)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetGuidInfo() Not found guid count: " + guids.Length.ToString());

        for (int i = 0; i < guids.Length; i++)
        {
          if (!dict.ContainsKey(guids[i]))
            dict[guids[i]] = FiasGuidInfo.NotFound;
        }
      }

      #endregion

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetGuidInfo() finished.");

      return dict;
    }

    public IDictionary<Guid, FiasGuidInfo> GetRecIdInfo(Guid[] recIds, FiasTableType tableType)
    {
#if DEBUG
      if (recIds == null)
        throw new ArgumentNullException("recIds");
#endif

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetRecIdInfo() started. tableType=" + tableType.ToString() + ", recIds.Length=" + recIds.Length.ToString());

      _FiasDB.CheckIsReady();

      Dictionary<Guid, FiasGuidInfo> dict = new Dictionary<Guid, FiasGuidInfo>();

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        #region AddrOb

        if (tableType == FiasTableType.AddrOb || tableType == FiasTableType.Unknown)
        {
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� ������������ �������� �������� ��� ��������������� (" + recIds.Length.ToString() + ")");
          try
          {
            spl.PercentMax = recIds.Length;
            spl.AllowCancel = true;
            foreach (Guid[] recIds2 in new ArrayBlockEnumerable<Guid>(recIds, 500))
            {
              DBxFilter filter = new ValuesFilter("AOID", recIds2);
              using (DbDataReader rdr = con.ReaderSelect("AddrOb",
                new DBxColumns("AOID,PARENTGUID,AOLEVEL"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid Child = DataTools.GetGuid(rdr.GetValue(0));
                  Guid Parent = DataTools.GetGuid(rdr.GetValue(1));
                  FiasLevel Level = (FiasLevel)DataTools.GetInt(rdr.GetValue(2));
                  dict[Child] = new FiasGuidInfo(Level, Parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += recIds2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion

        #region House

        if (_FiasDB.DBSettings.UseHouse && (tableType == FiasTableType.House || tableType == FiasTableType.Unknown))
        {
          recIds = RemoveFoundGuids(recIds, dict); // ������ ��������� �������� �������
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� �������� �������� ��� ��������������� ������ (" + recIds.Length.ToString() + ")");
          try
          {
            spl.PercentMax = recIds.Length;
            spl.AllowCancel = true;
            foreach (Guid[] recIds2 in new ArrayBlockEnumerable<Guid>(recIds, 500))
            {
              DBxFilter filter = new ValuesFilter("HOUSEID", recIds2);
              using (DbDataReader rdr = con.ReaderSelect("House",
                new DBxColumns("HOUSEID,AOGUID"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid child = rdr.GetGuid(0);
                  Guid parent = rdr.GetGuid(1);
                  dict[child] = new FiasGuidInfo(FiasLevel.House, parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += recIds2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion

        #region Room

        if (_FiasDB.DBSettings.UseRoom && (tableType == FiasTableType.Room || tableType == FiasTableType.Unknown))
        {
          recIds = RemoveFoundGuids(recIds, dict); // ������ ��������� ����
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("����� ����� ��� ��������������� ��������� (" + recIds.Length.ToString() + ")");
          try
          {
            spl.PercentMax = recIds.Length;
            spl.AllowCancel = true;
            foreach (Guid[] recIds2 in new ArrayBlockEnumerable<Guid>(recIds, 500))
            {
              DBxFilter filter = new ValuesFilter("ROOMID", recIds2);
              using (DbDataReader rdr = con.ReaderSelect("Room",
                new DBxColumns("ROOMID,HOUSEGUID"),
                filter))
              {
                while (rdr.Read())
                {
                  Guid child = rdr.GetGuid(0);
                  Guid parent = rdr.GetGuid(1);
                  dict[child] = new FiasGuidInfo(FiasLevel.Flat, parent);
                  //spl.CheckCancelled();
                }
              }
              spl.Percent += recIds2.Length;
            }
          }
          finally
          {
            SplashTools.ThreadSplashStack.EndSplash();
          }
        }

        #endregion
      }

      #region ��������� - �� �������

      if (recIds.Length > 0)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetRecIdInfo() Not found recId count: " + recIds.Length.ToString());

        for (int i = 0; i < recIds.Length; i++)
        {
          if (!dict.ContainsKey(recIds[i]))
            dict[recIds[i]] = FiasGuidInfo.NotFound;
        }
      }

      #endregion

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetGuidInfo() finished.");

      return dict;
    }

    private static Guid[] RemoveFoundGuids(Guid[] guids, Dictionary<Guid, FiasGuidInfo> dict)
    {
      if (guids.Length == 0 || dict.Count == 0)
        return guids;

      List<Guid> list = new List<Guid>();
      for (int i = 0; i < guids.Length; i++)
      {
        if (!dict.ContainsKey(guids[i]))
          list.Add(guids[i]);
      }

      return list.ToArray();
    }

    #endregion

    #region ������� ������ ��������������

    /// <summary>
    /// ������ ������� ��� �������� FiasCachedPageAddrOb
    /// </summary>
    private DataTable _AddrObTemplate;

    /// <summary>
    /// ������ ������� ��� �������� FiasCachedPageHouse
    /// </summary>
    private DataTable _HouseTemplate;

    /// <summary>
    /// ������ ������� ��� �������� FiasCachedPageRoom
    /// </summary>
    private DataTable _RoomTemplate;

    private void CreateTemplateTables()
    {
      #region "AddrOb"

      _AddrObTemplate = new DataTable("AddrOb");
      //_AddrObTemplate.RemotingFormat = SerializationFormat.Binary;

      _AddrObTemplate.Columns.Add("AOID", typeof(Guid)); // ������������� ������ � ��������� ����. 
      if (_FiasDB.DBSettings.UseHistory)
      {
        _AddrObTemplate.Columns.Add("PREVID", typeof(Guid));  // ������ ��� ������������ ��������������
        _AddrObTemplate.Columns.Add("NEXTID", typeof(Guid));  // ������ ��� ������������ ��������������
      }
      _AddrObTemplate.Columns.Add("AOGUID", typeof(Guid)); // ������������� ������� ��������������
      _AddrObTemplate.Columns.Add("PARENTGUID", typeof(Guid)); // ������������ �������
      if (_FiasDB.DBSettings.UseHistory)
      {
        _AddrObTemplate.Columns.Add("Actual", typeof(bool)); // ���������� ������� ������������
        _AddrObTemplate.Columns.Add("Live", typeof(bool)); // ������������� ���� LIVESTATUS � ����
      }
      _AddrObTemplate.Columns.Add("AOLEVEL", typeof(int)); // ������� ��������� ������� ������, � �� ��������
      _AddrObTemplate.Columns.Add("OFFNAME", typeof(string)); // ������������ �������. 
      _AddrObTemplate.Columns.Add("AOTypeId", typeof(Int32)); // ������ �� ������� "AOTypes". 
      _AddrObTemplate.Columns.Add("CENTSTATUS", typeof(int)); // ������ ������
      _AddrObTemplate.Columns.Add("DIVTYPE", typeof(int)); // ������� ���������:
      _AddrObTemplate.Columns.Add("REGIONCODE", typeof(int));	// ��� �������. ��� �������� ����� ������ ��� �����. ���� �� ����� ����� �������� NULL
      _AddrObTemplate.Columns.Add("POSTALCODE", typeof(int));	// �������� ������
      if (_FiasDB.DBSettings.UseIFNS)
      {
        _AddrObTemplate.Columns.Add("IFNSFL", typeof(int));
        _AddrObTemplate.Columns.Add("TERRIFNSFL", typeof(int));
        _AddrObTemplate.Columns.Add("IFNSUL", typeof(int));
        _AddrObTemplate.Columns.Add("TERRIFNSUL", typeof(int));
      }
      if (_FiasDB.DBSettings.UseOKATO)
        _AddrObTemplate.Columns.Add("OKATO", typeof(long));
      if (_FiasDB.DBSettings.UseOKTMO)
        _AddrObTemplate.Columns.Add("OKTMO", typeof(long));
      if (_FiasDB.DBSettings.UseDates)
      {
        if (_FiasDB.InternalSettings.UseOADates)
        {
          _AddrObTemplate.Columns.Add("dStartDate", typeof(int));
          _AddrObTemplate.Columns.Add("dEndDate", typeof(int));
        }
        else
        {
          _AddrObTemplate.Columns.Add("STARTDATE", typeof(DateTime));
          _AddrObTemplate.Columns.Add("ENDDATE", typeof(DateTime));
        }
      }
      _AddrObTemplate.Columns.Add("TopFlag", typeof(bool)); // ���� ��� � ���� ������. �������� ���������� ������ ��� ��������� AOGUID.

      #endregion

      #region "House"

      if (_FiasDB.DBSettings.UseHouse)
      {
        _HouseTemplate = new DataTable("AddrOb");
        //_HouseTemplate.RemotingFormat = SerializationFormat.Binary;

        _HouseTemplate.Columns.Add("HOUSEID", typeof(Guid)); // ������������� ������ � ��������� ����. 
        _HouseTemplate.Columns.Add("HOUSEGUID", typeof(Guid)); // ������������� ������� ��������������
        // ��� �� �����. �������� ������ ��������� � ������ ��������� ������� (����� ��� ����������� ������)
        //_HouseTemplate.Columns.Add("AOGUID", typeof(Guid)); // ������������ �������

        // ���� ��� ����
        _HouseTemplate.Columns.Add("nHouseNum", typeof(int)); // �������� ����� ���� ��� ����������. 
        _HouseTemplate.Columns.Add("HOUSENUM", typeof(string)); // ����� ����. 
        _HouseTemplate.Columns.Add("nBuildNum", typeof(int)); // �������� ����� ������� ��� ����������. 
        _HouseTemplate.Columns.Add("BUILDNUM", typeof(string)); // ����� �������.
        _HouseTemplate.Columns.Add("nStrucNum", typeof(int)); // �������� ����� �������� ��� ����������. 
        _HouseTemplate.Columns.Add("STRUCNUM", typeof(string)); // ����� ��������
        _HouseTemplate.Columns.Add("STRSTATUS", typeof(int)); // ������ ��������
        _HouseTemplate.Columns.Add("ESTSTATUS", typeof(string)); // ������� ��������
        //ts.Columns.AddInt("STARSTATUS", 0, 99, false); // ����������� ��� ��������

        _HouseTemplate.Columns.Add("DIVTYPE", typeof(string)); // ������� ���������:
        // �� �����ts.Columns.AddInt("REGIONCODE", 0, 99, _UseNullableCodes);	// ��� �������.
        _HouseTemplate.Columns.Add("POSTALCODE", typeof(int));	// �������� ������
        if (_FiasDB.DBSettings.UseIFNS)
        {
          _HouseTemplate.Columns.Add("IFNSFL", typeof(int));
          _HouseTemplate.Columns.Add("TERRIFNSFL", typeof(int));
          _HouseTemplate.Columns.Add("IFNSUL", typeof(int));
          _HouseTemplate.Columns.Add("TERRIFNSUL", typeof(int));
        }
        if (_FiasDB.DBSettings.UseOKATO)
          _HouseTemplate.Columns.Add("OKATO", typeof(long));
        if (_FiasDB.DBSettings.UseOKTMO)
          _HouseTemplate.Columns.Add("OKTMO", typeof(long));
        if (_FiasDB.DBSettings.UseDates)
        {
          if (_FiasDB.InternalSettings.UseOADates)
          {
            _HouseTemplate.Columns.Add("dStartDate", typeof(int));
            _HouseTemplate.Columns.Add("dEndDate", typeof(int));
          }
          else
          {
            _HouseTemplate.Columns.Add("STARTDATE", typeof(DateTime));
            _HouseTemplate.Columns.Add("ENDDATE", typeof(DateTime));
          }
        }
        _HouseTemplate.Columns.Add("TopFlag", typeof(bool)); // ���� ��� � ���� ������. �������� ���������� ������ ��� ��������� HOUSEGUID.
      }

      #endregion

      #region "Room"

      if (_FiasDB.DBSettings.UseRoom)
      {
        _RoomTemplate = new DataTable("Room");
        //_RoomTemplate.RemotingFormat = SerializationFormat.Binary;

        _RoomTemplate.Columns.Add("ROOMID", typeof(Guid)); // ������������� ������ � ��������� ����. 
        if (DBSettings.UseHistory)
        {
          _RoomTemplate.Columns.Add("PREVID", typeof(Guid));  // ������ ��� ������������ ��������������
          _RoomTemplate.Columns.Add("NEXTID", typeof(Guid));  // ������ ��� ������������ ��������������
        }
        _RoomTemplate.Columns.Add("ROOMGUID", typeof(Guid)); // ������������� ������� ��������������
        // ��� �� �����. ������� ������ ��������� � ������ ����
        //_RoomTemplate.Columns.Add("HOUSEGUID", typeof(Guid)); // ������������ �������

        // ���� ��� �������
        if (DBSettings.UseHistory)
          _RoomTemplate.Columns.Add("Live", typeof(bool)); // ������������� ���� LIVESTATUS � ����
        _RoomTemplate.Columns.Add("nFlatNumber", typeof(int)); // �������� ����� �������� ��� ����������
        _RoomTemplate.Columns.Add("FLATNUMBER", typeof(string)); // ����� ��������, ����� ��� �������
        _RoomTemplate.Columns.Add("FLATTYPE", typeof(int)); // ��� ���������
        _RoomTemplate.Columns.Add("nRoomNumber", typeof(int)); // �������� ����� ��������� ��� ����������
        _RoomTemplate.Columns.Add("ROOMNUMBER", typeof(String)); // ����� �������
        _RoomTemplate.Columns.Add("ROOMTYPE", typeof(int)); // ��� �������
        _RoomTemplate.Columns.Add("POSTALCODE", typeof(int));	// �������� ������
        if (_FiasDB.DBSettings.UseDates)
        {
          if (_FiasDB.InternalSettings.UseOADates)
          {
            _RoomTemplate.Columns.Add("dStartDate", typeof(int));
            _RoomTemplate.Columns.Add("dEndDate", typeof(int));
          }
          else
          {
            _RoomTemplate.Columns.Add("STARTDATE", typeof(DateTime));
            _RoomTemplate.Columns.Add("ENDDATE", typeof(DateTime));
          }
        }
        _RoomTemplate.Columns.Add("TopFlag", typeof(bool)); // ���� ��� � ���� ������. �������� ���������� ������ ��� ��������� ROOMGUID.
      }

      #endregion
    }

    #endregion

    #region �������� ������� ��������������

    /// <summary>
    /// �������� �� ������� � ExtDB
    /// </summary>
    private class ColMapper
    {
      #region �����������

      public ColMapper(DBxColumns columns1, DataTable table2)
      {
        _Map = new Dictionary<int, int>();
        for (int i = 0; i < columns1.Count; i++)
        {
          int p2 = table2.Columns.IndexOf(columns1[i]);
          if (p2 >= 0)
            _Map.Add(i, p2);
        }
      }

      #endregion

      #region ����

      private Dictionary<int, int> _Map;

      #endregion

      #region ������

      public void Fill(DbDataReader rdr1, DataRow row2)
      {
        foreach (KeyValuePair<int, int> pair in _Map)
        {
          row2[pair.Value] = rdr1[pair.Key];
        }
      }

      #endregion
    }

    #region "AddrOb"

    public IDictionary<Guid, FiasCachedPageAddrOb> GetAddrObPages(FiasLevel level, Guid[] pageAOGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetAddrObPages() started. level=" + level.ToString() + ", pageAOGuids.Length=" + pageAOGuids.Length.ToString());

      _FiasDB.CheckIsReady();

      Dictionary<Guid, FiasCachedPageAddrOb> dict;

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "�������� ������� �������� �������� \"" + FiasEnumNames.ToString(level, false) + "\" (" + pageAOGuids.Length.ToString() + ")",
        "���������� ������"});
      try
      {
        #region ������� �������

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageAOGuids.Length);
        int pEmptyGuid = -1; // ��� ����������� ��������
        for (int i = 0; i < pageAOGuids.Length; i++)
        {
          Guid g = pageAOGuids[i];
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("��������� guid, �������� ����������� �������", "pageAOGuids");

          DataSet ds = new DataSet();
          //ds.RemotingFormat = SerializationFormat.Binary;
          DataTable table = _AddrObTemplate.Clone();
          ds.Tables.Add(table);
          tables[g] = table;

          if (g == Guid.Empty)
            pEmptyGuid = i;
        }

        #endregion

        using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
        {
          DBxColumns columns = (DBxColumns.FromColumns(_AddrObTemplate.Columns) + "PARENTGUID") - "TopFlag";
          int pParentGuid = columns.IndexOf("PARENTGUID");
          ColMapper map = new ColMapper(columns, _AddrObTemplate);

          spl.PercentMax = pageAOGuids.Length;
          spl.AllowCancel = true;

          #region ���������� ��������

          // ��� ����������� �������� PARENTGUID=GuidEmpty.
          // � ���� ������ �� �������� ��� NULL. � ���������� ������ ����� �������������. 
          // ��-�� ������� COALESCE() �� ����� ����������� ������ �� ���� PARENTGUID.
          // ������������ � ��������� �������

          if (pEmptyGuid >= 0)
          {
            // ������� Guid.Empty �� ������ pageAOGuids
            DataTools.DeleteFromArray(ref pageAOGuids, pEmptyGuid, 1);

            // ��������� ������
            List<DBxFilter> filters = new List<DBxFilter>();
            filters.Add(new ValueFilter("PARENTGUID", null, DBxColumnType.Guid));
            filters.Add(new ValueFilter("AOLEVEL", (int)level));

            using (DbDataReader rdr = con.ReaderSelect("AddrOb",
              columns,
              AndFilter.FromList(filters)))
            {
              while (rdr.Read())
              {
                DataTable table = tables[Guid.Empty];
                DataRow row = table.NewRow();
                map.Fill(rdr, row);
                CorrectRowDates(row, FiasTableType.AddrOb);
                table.Rows.Add(row);
              }
            }

            spl.Complete();
          }

          #endregion

          #region ��������� �������

          foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(pageAOGuids, 100))
          {
            List<DBxFilter> filters = new List<DBxFilter>();
            filters.Add(new ValuesFilter("PARENTGUID", guids2));
            filters.Add(new ValueFilter("AOLEVEL", (int)level));

            using (DbDataReader rdr = con.ReaderSelect("AddrOb",
              columns,
              AndFilter.FromList(filters)))
            {
              while (rdr.Read())
              {
                Guid parent = DataTools.GetGuid(rdr[pParentGuid]);
                DataTable table = tables[parent];
                DataRow row = table.NewRow();
                map.Fill(rdr, row);
                CorrectRowDates(row, FiasTableType.AddrOb);
                table.Rows.Add(row);
                //spl.CheckCancelled();
              }
            }

            spl.Percent += guids2.Length;
          }

          #endregion

          spl.Complete();
        } // using con

        #region �������� ��������� �������

        dict = new Dictionary<Guid, FiasCachedPageAddrOb>(tables.Count);
        foreach (KeyValuePair<Guid, DataTable> pair in tables)
        {
          SetTopFlag(pair.Value, "AOGUID");
          pair.Value.AcceptChanges();
          dict[pair.Key] = new FiasCachedPageAddrOb(pair.Key, level, pair.Value.DataSet);
        }

        #endregion
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetAddrObPages() finished.");

      return dict;
    }

    #endregion

    #region "House"

    public IDictionary<Guid, FiasCachedPageHouse> GetHousePages(Guid[] pageAOGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetHousePages() started. pageAOGuids.Length=" + pageAOGuids.Length.ToString());

      _FiasDB.DBSettings.CheckUseHouse();
      _FiasDB.CheckIsReady();

      Dictionary<Guid, FiasCachedPageHouse> dict;

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "�������� ������� ������ (" + pageAOGuids.Length.ToString() + ")",
        "���������� ������"});
      try
      {
        #region ������� �������

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageAOGuids.Length);
        foreach (Guid g in pageAOGuids)
        {
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("��������� guid, �������� ����������� �������", "pageAOGuids");
          if (g == Guid.Empty)
            throw new ArgumentException("��������� Guid.Empty", "pageAOGuids");

          DataSet ds = new DataSet();
          //ds.RemotingFormat = SerializationFormat.Binary;
          DataTable table = _HouseTemplate.Clone();
          ds.Tables.Add(table);
          tables[g] = table;
        }

        #endregion

        using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
        {
          #region ��������� �������

          DBxColumns columns = (DBxColumns.FromColumns(_HouseTemplate.Columns) + "AOGUID") - "TopFlag";
          int pParentGuid = columns.IndexOf("AOGUID");
          int pNHouseNum = columns.IndexOf("nHouseNum");
          int pNBuildNum = columns.IndexOf("nBuildNum");
          int pNStrucNum = columns.IndexOf("nStrucNum");
#if DEBUG
          if (pNHouseNum < 0)
            throw new BugException("��� ���� nHouseNum");
          if (pNBuildNum < 0)
            throw new BugException("��� ���� nBuildNum");
          if (pNStrucNum < 0)
            throw new BugException("��� ���� nStrucNum");
#endif
          ColMapper map = new ColMapper(columns, _HouseTemplate);

          spl.PercentMax = pageAOGuids.Length;
          spl.AllowCancel = true;
          foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(pageAOGuids, 100))
          {
            DBxFilter filter = new ValuesFilter("AOGUID", guids2);
            //if (_FiasDB.DBSettings.UseHistory)
            //  filter = new AndFilter(filter, new ValueFilter("Actual", actuality == FiasCachedPageActuality.Actual));

            using (DbDataReader rdr = con.ReaderSelect("House",
              columns,
              filter))
            {
              while (rdr.Read())
              {
                Guid parent = DataTools.GetGuid(rdr[pParentGuid]);
                DataTable table = tables[parent];
                DataRow row = table.NewRow();
                map.Fill(rdr, row);
                ExpandNumInt(row, pNHouseNum);
                ExpandNumInt(row, pNBuildNum);
                ExpandNumInt(row, pNStrucNum);
                table.Rows.Add(row);
                //spl.CheckCancelled();
              }
            }
            spl.Percent += guids2.Length;
          }

          #endregion
        } // using con

        #region �������� ��������� �������

        dict = new Dictionary<Guid, FiasCachedPageHouse>(tables.Count);
        foreach (KeyValuePair<Guid, DataTable> pair in tables)
        {
          SetTopFlag(pair.Value, "HOUSEGUID");
          pair.Value.AcceptChanges();
          dict[pair.Key] = new FiasCachedPageHouse(pair.Key, pair.Value.DataSet);
        }

        #endregion
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetHousePages() finished.");

      return dict;
    }

    #endregion

    #region "Room"

    public IDictionary<Guid, FiasCachedPageRoom> GetRoomPages(Guid[] pageHouseGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetRoomPages() started. pageHouseGuids.Length=" + pageHouseGuids.Length.ToString());

      _FiasDB.DBSettings.CheckUseRoom();
      _FiasDB.CheckIsReady();
      Dictionary<Guid, FiasCachedPageRoom> dict;

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "�������� ������� ������ (" + pageHouseGuids.Length.ToString() + ")",
        "���������� ������"});
      try
      {
        #region ������� �������

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageHouseGuids.Length);
        foreach (Guid g in pageHouseGuids)
        {
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("��������� guid, �������� ����������� �������", "pageHouseGuids");
          if (g == Guid.Empty)
            throw new ArgumentException("��������� Guid.Empty", "pageHouseGuids");

          DataSet ds = new DataSet();
          //ds.RemotingFormat = SerializationFormat.Binary;
          DataTable table = _RoomTemplate.Clone();
          ds.Tables.Add(table);
          tables[g] = table;
        }

        #endregion

        using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
        {
          #region ��������� �������

          DBxColumns columns = (DBxColumns.FromColumns(_RoomTemplate.Columns) + "HOUSEGUID") - "TopFlag";
          int pParentGuid = columns.IndexOf("HOUSEGUID");
          int pNFlatNumber = columns.IndexOf("nFlatNumber");
          int pNRoomNumber = columns.IndexOf("nRoomNumber");
#if DEBUG
          if (pNFlatNumber < 0)
            throw new BugException("��� ���� nFlatNumber");
          if (pNRoomNumber < 0)
            throw new BugException("��� ���� nRoomNumber");
#endif
          ColMapper map = new ColMapper(columns, _RoomTemplate);

          spl.PercentMax = pageHouseGuids.Length;
          spl.AllowCancel = true;
          foreach (Guid[] guids2 in new ArrayBlockEnumerable<Guid>(pageHouseGuids, 100))
          {
            DBxFilter filter = new ValuesFilter("HOUSEGUID", guids2);
            //if (_FiasDB.DBSettings.UseHistory)
            //  filter = new AndFilter(filter, new ValueFilter("Actual", actuality == FiasCachedPageActuality.Actual));

            using (DbDataReader rdr = con.ReaderSelect("Room",
              columns,
              filter))
            {
              while (rdr.Read())
              {
                Guid parent = DataTools.GetGuid(rdr[pParentGuid]);
                DataTable table = tables[parent];
                DataRow row = table.NewRow();
                map.Fill(rdr, row);
                ExpandNumInt(row, pNFlatNumber);
                ExpandNumInt(row, pNRoomNumber);
                CorrectRowDates(row, FiasTableType.Room);
                table.Rows.Add(row);
                //spl.CheckCancelled();
              }
            }
            spl.Percent += guids2.Length;
          }

          #endregion
        } // using con

        #region �������� ��������� �������

        dict = new Dictionary<Guid, FiasCachedPageRoom>(tables.Count);
        foreach (KeyValuePair<Guid, DataTable> pair in tables)
        {
          SetTopFlag(pair.Value, "ROOMGUID");
          pair.Value.AcceptChanges();
          dict[pair.Key] = new FiasCachedPageRoom(pair.Key, pair.Value.DataSet);
        }

        #endregion
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetRoomPages() finished.");

      return dict;
    }

    #endregion

    #region ����������� �������� ��� ��������� ������

    public FiasCachedPageSpecialAddrOb GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetSpecialAddrObPage() started. pageType=" + pageType.ToString());

      _FiasDB.CheckIsReady();

      DataTable table = _AddrObTemplate.Clone();

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        #region ��������� �������

        DBxColumns columns = DBxColumns.FromColumns(_AddrObTemplate.Columns) /*+ "PARENTGUID"*/ - "TopFlag";
        //int pParentGuid = columns.IndexOf("PARENTGUID");
        ColMapper map = new ColMapper(columns, _AddrObTemplate);

        List<DBxFilter> filters = new List<DBxFilter>();
        switch (pageType)
        {
          case FiasSpecialPageType.AllCities:
            if (pageAOGuid != Guid.Empty)
              throw new ArgumentException("pageAOGuid ��������");

            //filters.Add(new ValuesFilter("CENTSTATUS", new int[] { (int)FiasCenterStatus.Region, (int)FiasCenterStatus.RegionAndDistrict }));

            // ����� ������ ������, � �� ���������� � ������, ��� ����� ���� �� ������ "�����"
            IdList CityAOTypeIds = con.GetIds("AOType", new AndFilter(new ValueFilter("LEVEL", (int)FiasLevel.City),
              new ValueFilter("SOCRNAME", "�����"))); // ����� ���� ��������� ����������: "�." � "�".
            if (CityAOTypeIds.Count > 0)
              filters.Add(new IdsFilter("AOTypeId", CityAOTypeIds));

            break;
          case FiasSpecialPageType.AllDistricts:
            if (pageAOGuid != Guid.Empty)
              throw new ArgumentException("pageAOGuid ��������");

            // � ������ ���� ���������, ����������� � ����������� �������
            filters.Add(new NotFilter(new ValuesFilter("REGIONCODE", FiasTools.FederalCityRegionCodesInt)));

            break;
          default:
            throw new ArgumentException("pageType");
        }
        filters.Add(new ValueFilter("AOLEVEL", (int)FiasTools.GetSpecialPageLevel(pageType)));
        if (_FiasDB.DBSettings.UseHistory)
          filters.Add(new ValueFilter("Actual", true));

        using (DbDataReader rdr = con.ReaderSelect("AddrOb",
          columns,
          AndFilter.FromList(filters)))
        {
          while (rdr.Read())
          {
            DataRow row = table.NewRow();
            map.Fill(rdr, row);
            CorrectRowDates(row, FiasTableType.AddrOb);
            table.Rows.Add(row);
          }
        }

        #endregion
      } // using con


      #region �������� ��������

      SetTopFlag(table, "AOGUID");

      DataSet ds = new DataSet();
      ds.Tables.Add(table);
      //ds.RemotingFormat = SerializationFormat.Binary;
      //ds.AcceptChanges();
      SerializationTools.PrepareDataSet(ds); // 19.01.2021

      #endregion

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetSpecialAddrObPage() finished.");

      return new FiasCachedPageSpecialAddrOb(pageAOGuid, FiasTools.GetSpecialPageLevel(pageType), pageType, ds);
    }

    #endregion

    /// <summary>
    /// �������� ������ �������� ��� ����� ������� ����� � �������.
    /// ��������, ���� ���� "nHouseNum" ����� �������� 25 (�� 1 �� 255), � ���� "HOUSENUM" ����� �������� NULL,
    /// �� ���� "HOUSENUM" ���������� �� "25"
    /// </summary>
    /// <param name="row">������ ������������ �������</param>
    /// <param name="pNColumn">������� ��������� �������, ������� ���� �� ��������� ����</param>
    private static void ExpandNumInt(DataRow row, int pNColumn)
    {
      if (row.IsNull(pNColumn + 1))
      {
        int n = DataTools.GetInt(row[pNColumn]);
        if (n >= 1)
          row[pNColumn + 1] = StdConvert.ToString(n);
      }
    }

    private static void ExpandNumInt(DataTable table, string nameNColumn)
    {
      int pNColumn = table.Columns.IndexOf(nameNColumn);
#if DEBUG
      if (pNColumn < 0)
        throw new ArgumentException("������� " + table.TableName + " �� �������� ���� \"" + nameNColumn + "\"", "nameNColumn");
#endif

      foreach (DataRow row in table.Rows)
        ExpandNumInt(row, pNColumn);
    }


    /// <summary>
    /// ��������� ���� "TopFlag" � ����������� ������� ��������
    /// </summary>
    /// <param name="table">����������� �������</param>
    /// <param name="guidColName">���� "�����������" �������������� ��������� �������</param>
    private void SetTopFlag(DataTable table, string guidColName)
    {
      if (table.Rows.Count == 0)
        return;

      string s = guidColName;
      int pGuidCol = table.Columns.IndexOf(guidColName);
      int pTopFlag = table.Columns.IndexOf("TopFlag");
      if (table.Columns.IndexOf("Actual") >= 0)
        s += ",Actual DESC";
      if (_FiasDB.DBSettings.UseDates)
      {
        if (_FiasDB.InternalSettings.UseOADates)
          s += ",dEndDate DESC";
        else
          s += ",ENDDATE DESC";
      }
      table.DefaultView.Sort = s;
      Guid PrevGuid = Guid.Empty;
      foreach (DataRowView drv in table.DefaultView)
      {
        Guid g = (Guid)(drv.Row[pGuidCol]);
        if (g == PrevGuid)
          drv.Row[pTopFlag] = false;
        else
        {
          drv.Row[pTopFlag] = true;
          PrevGuid = g;
        }
      }
    }

    #region ������������� ���

    private static readonly DateTime _DT19991231 = new DateTime(1999, 12, 31);
    private static readonly int _NDT19991231 = (int)(new DateTime(1999, 12, 31).ToOADate());
    private static readonly int _NDT99991231 = (int)(DateTime.MaxValue.ToOADate());

    private void CorrectRowDates(DataRow row, FiasTableType tableType)
    {
      // � ����� �� 02.03.2020 ��� ���� ����������� �������� �������� �������� ���� ������ ��� 31.12.1999
      // ��� ������� - ����
      // ��� ����� - ���������� �������� ���� 31.12.9999

      if (!_FiasDB.DBSettings.UseDates)
        return;

      if (_FiasDB.DBSettings.UseHistory && tableType == FiasTableType.AddrOb)
      {
        if (!DataTools.GetBool(row, "Actual"))
          return;
      }

      if (_FiasDB.InternalSettings.UseOADates)
      {
        int dEnd = DataTools.GetInt(row, "dEndDate");
        if (dEnd == _NDT19991231)
          row["dEndDate"] = _NDT99991231;
      }
      else
      {
        DateTime dEnd = DataTools.GetNullableDateTime(row, "ENDDATE") ?? DateTime.MinValue;
        if (dEnd == _DT19991231)
          row["ENDDATE"] = DateTime.MaxValue;
      }
    }

    #endregion

    #endregion

    #region ������� ����������

    public FiasCachedAOTypes GetAOTypes()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetAOTypes() started.");

      _FiasDB.CheckIsReady();

      FiasCachedAOTypes page;

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        DataTable table = con.FillSelect("AOType");

        // � ������ ����� �� 02.03.2020 �����-�� �������� ��������� ������:
        // 18 - ������ (������ ���� 8)
        // 19 - ��������� (������ ���� 9)
        // 20 - ��������� ������� (������ ���� 75, �� ������ �������)
        // 21 - �.�., ������ �� 35, �� 35 ���� ����
        foreach (DataRow row in table.Rows)
        {
          switch (DataTools.GetInt(row, "LEVEL"))
          {
            case 18: row["LEVEL"] = 8; break;
            case 19: row["LEVEL"] = 9; break;
            case 20: row["LEVEL"] = 75; break;
          }
        }
        //table.AcceptChanges();

        //table.RemotingFormat = SerializationFormat.Binary;
        DataSet ds = new DataSet();
        //ds.RemotingFormat = SerializationFormat.Binary;
        ds.Tables.Add(table);
        SerializationTools.PrepareDataSet(ds); // 19.01.2021

        page = new FiasCachedAOTypes(ds);
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetAOTypes() started.");

      return page;
    }

    #endregion

    #region ����� �������

    public DataSet FindAddresses(FiasAddressSearchParams searchParams)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.FindAddresses() started.");

      _FiasDB.CheckIsReady();

      DataTable table;
      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        switch (_FiasDB.InternalSettings.FTSMode)
        {
          case FiasFTSMode.FTS3:
            table = FindAddressesFTS3(searchParams, con);
            break;
          default:
            throw new InvalidOperationException("������������� ����� �� ��������������");
        }
      }
      DataSet ds = new DataSet();
      ds.Tables.Add(table);

      ds.AcceptChanges();

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.FindAddresses() finished.");

      return ds;
    }

#if XXX
    /// <summary>
    /// �������������� ������� MATCH ������ LIKE
    /// </summary>
    private class MyFormatter : DBxSqlChainFormatter
    {
    #region �����������

      public MyFormatter(DBxSqlFormatter source)
        :base(source)
      { 
      }

    #endregion

    #region �������������� �������

      protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
      {
        buffer.FormatExpression(filter.Expression, new DBxFormatExpressionInfo());
        buffer.SB.Append(" MATCH ");
        buffer.FormatValue(filter.Value, DBxColumnType.String); // ��������� ���� ��������� �������
      }

    #endregion
    }

    private DataTable FindAddressesFTS3(FiasAddressSearchParams searchParams, DBxConBase con)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = "AddrOb";
      info.Expressions.Add("AOGUID");
      if (_FiasDB.DBSettings.UseHistory)
        info.Expressions.Add("Actual");
      info.Expressions.Add("NameId.rowid");

      List<DBxFilter> filters = new List<DBxFilter>();

      // ���� ������ ����� ������� �� "MATCH" ��� �������������� � ������ MyFormatter
      filters.Add(new StartsWithFilter("NameId.OFFNAME", FiasTools.PrepareForFTS(searchParams.Text) + "*"));

      if (searchParams.ActualOnly && _FiasDB.DBSettings.UseHistory)
        filters.Add(new ValueFilter("Actual", true));
      FiasLevel[] levels;
      if (searchParams.Levels != null)
      {
        int[] levels2 = new int[searchParams.Levels.Length];
        for (int i = 0; i < levels2.Length; i++)
          levels2[i] = (int)(searchParams.Levels[i]);
        filters.Add(new ValuesFilter("AOLEVEL", levels2, DBxColumnType.Int));
        levels = searchParams.Levels;
      }
      else
        levels = FiasTools.AOLevels; // ��� ��������� ������ ��� ������
      if (searchParams.StartAddress != null)
      { 
        FiasLevel parentLevel=searchParams.StartAddress.Guids.BottomLevel;
        if (parentLevel != FiasLevel.Unknown)
        { 
          // ������ ���������
          SingleScopeList<int> distList = new SingleScopeList<int>();
          for (int i = 0; i < levels.Length; i++)
            FiasTools.GetPossibleInheritanceDistance(parentLevel, levels[i], distList);

          distList.Sort();
          if (distList.Count == 0)
            throw new ArgumentException("��� �������� ������ " + searchParams.StartAddress.ToString() + " �� ������� ���������� ������� ������������ ������� ������� ��������� ��������", "searchParams.StartAddress");
          DBxFilter[] treeFilters = new DBxFilter[distList.Count];
          for (int i = 0; i < distList.Count; i++)
          { 
            StringBuilder sb=new StringBuilder();
            for (int j=0; j<distList[i];j++)
            {
              if (j>0)
                sb.Append(".");
              sb.Append("PARENTGUID");
            }
            treeFilters[i] = new ValueFilter(sb.ToString(), searchParams.StartAddress.Guids.AOGuid, CompareKind.Equal, DBxColumnType.Guid);
            
            info.Expressions.Add(sb.ToString());
          }
          filters.Add(OrFilter.FromArray(treeFilters));
        }
      }

      info.Where = AndFilter.FromList(filters);
      info.MaxRecordCount = FiasTools.AddressSearchLimit;
      DBxSqlBuffer buffer = new DBxSqlBuffer(new MyFormatter(_FiasDB.DB.Formatter));
      buffer.FormatSelect(info, con.Validator);

      string cmdText = buffer.SB.ToString();
      // ����� �������� ����� ����� ��������� AddrOb � AddrObFTS � "LEFT JOIN" �� "JOIN".
      // � ��������� ����� ����� AddrOb � AddrOb �������� "LEFT JOIN".
      int p = cmdText.IndexOf("LEFT JOIN");
      cmdText = cmdText.Substring(0, p) + cmdText.Substring(p + 5); // ������ LEFT

      return con.SQLExecuteDataTable(cmdText);
    }
#else

    private DataTable FindAddressesFTS3(FiasAddressSearchParams searchParams, DBxConBase con)
    {
      FiasLevel[] levels;
      if (searchParams.Levels != null)
        levels = searchParams.Levels;
      else
        levels = FiasTools.AOLevels; // ��� ��������� ������ ��� ������

      // ������������ ��������� ��� ��������� � PARENTAOGUID.
      // ���� 0, �� ������� ����� �� �����.
      // ���� 1, �� ��������� ������ � ������� PARENTGUID (��������, ����� ����� � ���������� ������ ��� ���. ������ � ������)
      // ���� 2, �� �� ���� ������� ������ (��������, ����� � ������)
      int maxDist = 0;
      Guid parentGuid = Guid.Empty;
      if (searchParams.StartAddress != null)
      {
        FiasLevel parentLevel = searchParams.StartAddress.GuidBottomLevel;
        if (parentLevel != FiasLevel.Unknown)
        {
          parentGuid = searchParams.StartAddress.GetGuid(parentLevel);
          // ������ ���������
          SingleScopeList<int> distList = new SingleScopeList<int>();
          for (int i = 0; i < levels.Length; i++)
            FiasTools.GetPossibleInheritanceDistance(parentLevel, levels[i], distList);

          distList.Sort();
          if (distList.Count == 0)
            throw new ArgumentException("��� �������� ������ " + searchParams.StartAddress.ToString() + " (������� " + FiasEnumNames.ToString(parentLevel, true) + ") �� ������� ���������� ������� ������������ ������� ������� �������� ��������", "searchParams");
          maxDist = distList[distList.Count - 1];
        }
      }


      DBxSqlBuffer buffer = new DBxSqlBuffer(_FiasDB.DB.Formatter);

      // ������� SQL-������ �������
      buffer.SB.Append("SELECT DISTINCT ");

      buffer.FormatColumnName("AddrOb_1", "AOGUID");
      buffer.SB.Append(" AS ");
      buffer.FormatColumnName("AOGUID");

      buffer.SB.Append(", ");
      buffer.FormatColumnName("AddrOb_1", "AOID");
      buffer.SB.Append(" AS ");
      buffer.FormatColumnName("AOID");

      if (_FiasDB.DBSettings.UseHistory)
      {
        buffer.SB.Append(", ");
        buffer.FormatColumnName("AddrOb_1", "Actual");
        buffer.SB.Append(" AS ");
        buffer.FormatColumnName("Actual");

        buffer.SB.Append(", ");
        buffer.FormatColumnName("AddrOb_1", "Live");
        buffer.SB.Append(" AS ");
        buffer.FormatColumnName("Live");
      }

      if (_FiasDB.DBSettings.UseDates)
      {
        if (_FiasDB.InternalSettings.UseOADates)
        {
          buffer.SB.Append(", ");
          buffer.FormatColumnName("AddrOb_1", "dStartDate");
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName("dStartDate");
          buffer.SB.Append(", ");
          buffer.FormatColumnName("AddrOb_1", "dEndDate");
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName("dEndDate");
        }
        else
        {
          buffer.SB.Append(", ");
          buffer.FormatColumnName("AddrOb_1", "STARTDATE");
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName("STARTDATE");
          buffer.SB.Append(", ");
          buffer.FormatColumnName("AddrOb_1", "ENDDATE");
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName("ENDDATE");
        }
      }

      buffer.SB.Append(" FROM ");
      // ����������� ������
      if (maxDist > 1)
        buffer.SB.Append(new string('(', maxDist - 1));
      buffer.SB.Append("(");
      buffer.FormatTableName("AddrOb");
      buffer.SB.Append(" AS ");
      buffer.FormatTableName("AddrOb_1");
      buffer.SB.Append(" JOIN ");
      buffer.FormatTableName("AddrObFTS");
      buffer.SB.Append(" ON ");
      buffer.FormatColumnName("AddrOb_1", "NameId");
      buffer.SB.Append(" = ");
      buffer.FormatColumnName("AddrObFTS", "rowid");
      buffer.SB.Append(")");
      for (int i = 1; i <= maxDist; i++)
      {
        buffer.SB.Append(" LEFT JOIN ");
        buffer.FormatTableName("AddrOb");
        buffer.SB.Append(" AS ");
        buffer.FormatTableName("AddrOb_" + (i + 1).ToString());
        buffer.SB.Append(" ON ");
        buffer.FormatColumnName("AddrOb_" + i.ToString(), "PARENTGUID");
        buffer.SB.Append(" = ");
        buffer.FormatColumnName("AddrOb_" + (i + 1).ToString(), "AOGUID");
        if (i != maxDist)
          buffer.SB.Append(")");
      }
      buffer.SB.Append(" WHERE (");
      buffer.FormatColumnName("AddrObFTS", "OFFNAME");
      buffer.SB.Append(" MATCH ");

      string sText = FiasTools.PrepareForFTS(searchParams.Text);
      sText += "*";
      buffer.FormatValue(sText, DBxColumnType.String);
      buffer.SB.Append(")");
      if (searchParams.ActualOnly && _FiasDB.DBSettings.UseHistory)
      {
        buffer.SB.Append(" AND ");
        buffer.FormatColumnName("AddrOb_1", "Actual");
        buffer.SB.Append(" = ");
        buffer.FormatValue(true, DBxColumnType.Boolean);

        buffer.SB.Append(" AND ");
        buffer.FormatColumnName("AddrOb_1", "Live");
        buffer.SB.Append(" = ");
        buffer.FormatValue(true, DBxColumnType.Boolean);
      }

      if (searchParams.Levels != null)
      {
        buffer.SB.Append(" AND ");
        buffer.FormatColumnName("AddrOb_1", "AOLEVEL");
        buffer.SB.Append(" IN (");
        for (int i = 0; i < searchParams.Levels.Length; i++)
        {
          if (i > 0)
            buffer.SB.Append(",");
          buffer.FormatValue((int)(searchParams.Levels[i]), DBxColumnType.Int);
        }
        buffer.SB.Append(")");
      }
      if (maxDist > 0)
      {
        buffer.SB.Append(" AND (");
        for (int i = 1; i <= maxDist; i++)
        {
          if (i > 1)
            buffer.SB.Append(" OR ");
          buffer.FormatColumnName("AddrOb_" + i.ToString(), "PARENTGUID");
          buffer.SB.Append(" = ");
          buffer.FormatValue(parentGuid, DBxColumnType.Guid);
        }
        buffer.SB.Append(")");
      }

      buffer.SB.Append(" LIMIT ");
      buffer.FormatValue(FiasTools.AddressSearchLimit, DBxColumnType.Int);

      string cmdText = buffer.SB.ToString();
      return con.SQLExecuteDataTable(cmdText);
    }

#endif
    #endregion

    #region ������ � �������

    public DataSet GetTableForGuid(Guid guid, FiasTableType tableType)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetTableForGuid() started.");

      _FiasDB.CheckIsReady();

      if (guid == Guid.Empty || guid == FiasTools.GuidNotFound)
        throw new ArgumentException("������������ GUID=" + guid.ToString(), "guid");

      string tableName, colName;
      switch (tableType)
      {
        case FiasTableType.AddrOb:
          tableName = "AddrOb"; colName = "AOGUID"; break;
        case FiasTableType.House:
          tableName = "House"; colName = "HOUSEGUID"; break;
        case FiasTableType.Room:
          tableName = "Room"; colName = "ROOMGUID"; break;
        default:
          throw new ArgumentException("����������� ������� " + tableType.ToString(), "tableType");
      }

      string order = String.Empty;
      if (_FiasDB.DBSettings.UseDates)
      {
        if (_FiasDB.InternalSettings.UseOADates)
          order = "dStartDate,dEndDate";
        else
          order = "STARTDATE,ENDDATE";
      }

      // 26.02.2021
      // ���� �� ����, ��� �������
      // ��� AddrOb � Room ������� ���������� ������ ����:
      // ORDER BY STARTDATE,ISNULL(PREVID)?0:1,ISNULL(NEXTID)?1:0,ENDDATE

      //List<DBxOrderPart> orderParts=new List<DBxOrderPart>();
      //if (_FiasDB.DBSettings.UseHistory)
      //{
      //  switch (tableType)
      //  { 
      //    case FiasTableType.AddrOb:
      //    case FiasTableType.Room:
      //      orderParts.Add(new DBxOrderPart(new DBxFunction(DBxFunctionKind.c
      //  }
      //}
      //if (_FiasDB.DBSettings.UseDates)
      //{
      //  if (_FiasDB.InternalSettings.UseOADates)
      //    order = "dStartDate,dEndDate";
      //  else
      //    order = "STARTDATE,ENDDATE";
      //}


      DataSet ds;

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        DataTable table = con.FillSelect(tableName,
          null,
          new ValueFilter(colName, guid, CompareKind.Equal, DBxColumnType.Guid),
          DBxOrder.FromDataViewSort(order));

        // ����� �� ��������, ��� ��� �������� �������������� �������
        switch (tableType)
        {
          case FiasTableType.House:
            ExpandNumInt(table, "nHouseNum");
            ExpandNumInt(table, "nBuildNum");
            ExpandNumInt(table, "nStrucNum");
            break;
          case FiasTableType.Room:
            ExpandNumInt(table, "nFlatNumber");
            ExpandNumInt(table, "nRoomNumber");
            break;
        }

        //table.AcceptChanges();
        //table.RemotingFormat = SerializationFormat.Binary;
        ds = new DataSet();
        //ds.RemotingFormat = SerializationFormat.Binary;
        ds.Tables.Add(table);
        SerializationTools.PrepareDataSet(ds); // 19.01.2021
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetTableForGuid() finished.");

      return ds;
    }

    #endregion

    #region ������� ����������

    /// <summary>
    /// ��������� ������� ������� ���������� ��������������
    /// </summary>
    /// <returns></returns>
    public DataTable GetClassifUpdateTable()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetClassifUpdateTable() started.");

      DataTable table;
      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        table = con.FillSelect("ClassifUpdate");
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetClassifUpdateTable() finished.");

      return table;
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    void IFiasSource.UpdateActualDate()
    {
    }

    #endregion
  }
}
