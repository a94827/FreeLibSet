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
  /// Небуферизованный источник данных для загрузки страниц классификатора.
  /// Отвечает за выполнение SQL-запросов SELECT.
  /// Создается в конструкторе FiasDB.
  /// </summary>
  internal class FiasDBUnbufferedSource : IFiasSource
  {
    #region Конструктор

    public FiasDBUnbufferedSource(FiasDB fiasDB)
    {
      _FiasDB = fiasDB;
      CreateTemplateTables();
    }

    #endregion

    #region Свойства

    private FiasDB _FiasDB;

    #endregion

    #region Простые cвойства IFiasSource

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
      throw new InvalidOperationException("Этот метод должен вызываться для FiasDB.Source");
    }

    #endregion

    #region Получение словарей идентификаторов

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
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск родительских адресных объектов для идентификаторов (" + guids.Length.ToString() + ")");
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
          guids = RemoveFoundGuids(guids, dict); // убрали найденные адресные объекты
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск адресных объектов для идентификаторов зданий (" + guids.Length.ToString() + ")");
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
          guids = RemoveFoundGuids(guids, dict); // убрали найденные дома
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск домов для идентификаторов помещений (" + guids.Length.ToString() + ")");
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

      #region Остальные - не найдены

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
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск родительских адресных объектов для идентификаторов (" + recIds.Length.ToString() + ")");
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
          recIds = RemoveFoundGuids(recIds, dict); // убрали найденные адресные объекты
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск адресных объектов для идентификаторов зданий (" + recIds.Length.ToString() + ")");
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
          recIds = RemoveFoundGuids(recIds, dict); // убрали найденные дома
          ISplash spl = SplashTools.ThreadSplashStack.BeginSplash("Поиск домов для идентификаторов помещений (" + recIds.Length.ToString() + ")");
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

      #region Остальные - не найдены

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

    #region Шаблоны таблиц классификатора

    /// <summary>
    /// Шаблон таблицы для страницы FiasCachedPageAddrOb
    /// </summary>
    private DataTable _AddrObTemplate;

    /// <summary>
    /// Шаблон таблицы для страницы FiasCachedPageHouse
    /// </summary>
    private DataTable _HouseTemplate;

    /// <summary>
    /// Шаблон таблицы для страницы FiasCachedPageRoom
    /// </summary>
    private DataTable _RoomTemplate;

    private void CreateTemplateTables()
    {
      #region "AddrOb"

      _AddrObTemplate = new DataTable("AddrOb");
      //_AddrObTemplate.RemotingFormat = SerializationFormat.Binary;

      _AddrObTemplate.Columns.Add("AOID", typeof(Guid)); // Идентификатор записи и первичный ключ. 
      if (_FiasDB.DBSettings.UseHistory)
      {
        _AddrObTemplate.Columns.Add("PREVID", typeof(Guid));  // Ссылки для отслеживания переименований
        _AddrObTemplate.Columns.Add("NEXTID", typeof(Guid));  // Ссылки для отслеживания переименований
      }
      _AddrObTemplate.Columns.Add("AOGUID", typeof(Guid)); // Идентификатор объекта классификатора
      _AddrObTemplate.Columns.Add("PARENTGUID", typeof(Guid)); // Родительский элемент
      if (_FiasDB.DBSettings.UseHistory)
      {
        _AddrObTemplate.Columns.Add("Actual", typeof(bool)); // загадочный признак актуальности
        _AddrObTemplate.Columns.Add("Live", typeof(bool)); // Соответствует полю LIVESTATUS в ФИАС
      }
      _AddrObTemplate.Columns.Add("AOLEVEL", typeof(int)); // Уровень адресного объекта строки, а не страницы
      _AddrObTemplate.Columns.Add("OFFNAME", typeof(string)); // Наименование объекта. 
      _AddrObTemplate.Columns.Add("AOTypeId", typeof(Int32)); // Ссылка на таблицу "AOTypes". 
      _AddrObTemplate.Columns.Add("CENTSTATUS", typeof(int)); // Статус центра
      _AddrObTemplate.Columns.Add("DIVTYPE", typeof(int)); // Признак адресации:
      _AddrObTemplate.Columns.Add("REGIONCODE", typeof(int));	// Код региона. Для экономии места храним как число. Поле не может иметь значения NULL
      _AddrObTemplate.Columns.Add("POSTALCODE", typeof(int));	// Почтовый индекс
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
      _AddrObTemplate.Columns.Add("TopFlag", typeof(bool)); // поля нет в базе данных. Отмечает актуальную строку для заданного AOGUID.

      #endregion

      #region "House"

      if (_FiasDB.DBSettings.UseHouse)
      {
        _HouseTemplate = new DataTable("AddrOb");
        //_HouseTemplate.RemotingFormat = SerializationFormat.Binary;

        _HouseTemplate.Columns.Add("HOUSEID", typeof(Guid)); // Идентификатор записи и первичный ключ. 
        _HouseTemplate.Columns.Add("HOUSEGUID", typeof(Guid)); // Идентификатор объекта классификатора
        // Это не нужно. Страница всегда относится к одному адресному объекту (улице или населенному пункту)
        //_HouseTemplate.Columns.Add("AOGUID", typeof(Guid)); // Родительский элемент

        // Поля для дома
        _HouseTemplate.Columns.Add("nHouseNum", typeof(int)); // Числовой номер дома для сортировки. 
        _HouseTemplate.Columns.Add("HOUSENUM", typeof(string)); // Номер дома. 
        _HouseTemplate.Columns.Add("nBuildNum", typeof(int)); // Числовой номер корпуса для сортировки. 
        _HouseTemplate.Columns.Add("BUILDNUM", typeof(string)); // Номер корпуса.
        _HouseTemplate.Columns.Add("nStrucNum", typeof(int)); // Числовой номер строения для сортировки. 
        _HouseTemplate.Columns.Add("STRUCNUM", typeof(string)); // Номер строения
        _HouseTemplate.Columns.Add("STRSTATUS", typeof(int)); // Статус строения
        _HouseTemplate.Columns.Add("ESTSTATUS", typeof(string)); // Признак владения
        //ts.Columns.AddInt("STARSTATUS", 0, 99, false); // размерность под вопросом

        _HouseTemplate.Columns.Add("DIVTYPE", typeof(string)); // Признак адресации:
        // Не нуженts.Columns.AddInt("REGIONCODE", 0, 99, _UseNullableCodes);	// Код региона.
        _HouseTemplate.Columns.Add("POSTALCODE", typeof(int));	// Почтовый индекс
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
        _HouseTemplate.Columns.Add("TopFlag", typeof(bool)); // поля нет в базе данных. Отмечает актуальную строку для заданного HOUSEGUID.
      }

      #endregion

      #region "Room"

      if (_FiasDB.DBSettings.UseRoom)
      {
        _RoomTemplate = new DataTable("Room");
        //_RoomTemplate.RemotingFormat = SerializationFormat.Binary;

        _RoomTemplate.Columns.Add("ROOMID", typeof(Guid)); // Идентификатор записи и первичный ключ. 
        if (DBSettings.UseHistory)
        {
          _RoomTemplate.Columns.Add("PREVID", typeof(Guid));  // Ссылки для отслеживания переименований
          _RoomTemplate.Columns.Add("NEXTID", typeof(Guid));  // Ссылки для отслеживания переименований
        }
        _RoomTemplate.Columns.Add("ROOMGUID", typeof(Guid)); // Идентификатор объекта классификатора
        // Это не нужно. Таблица всегда относится к одному дому
        //_RoomTemplate.Columns.Add("HOUSEGUID", typeof(Guid)); // Родительский элемент

        // Поля для объекта
        if (DBSettings.UseHistory)
          _RoomTemplate.Columns.Add("Live", typeof(bool)); // Соответствует полю LIVESTATUS в ФИАС
        _RoomTemplate.Columns.Add("nFlatNumber", typeof(int)); // Числовой номер квартиры для сортировки
        _RoomTemplate.Columns.Add("FLATNUMBER", typeof(string)); // Номер квартиры, офиса или прочего
        _RoomTemplate.Columns.Add("FLATTYPE", typeof(int)); // Тип помещения
        _RoomTemplate.Columns.Add("nRoomNumber", typeof(int)); // Числовой номер помещения для сортировки
        _RoomTemplate.Columns.Add("ROOMNUMBER", typeof(String)); // Номер комнаты
        _RoomTemplate.Columns.Add("ROOMTYPE", typeof(int)); // Тип комнаты
        _RoomTemplate.Columns.Add("POSTALCODE", typeof(int));	// Почтовый индекс
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
        _RoomTemplate.Columns.Add("TopFlag", typeof(bool)); // поля нет в базе данных. Отмечает актуальную строку для заданного ROOMGUID.
      }

      #endregion
    }

    #endregion

    #region Загрузка страниц классификатора

    /// <summary>
    /// Кандидат на перенос в ExtDB
    /// </summary>
    private class ColMapper
    {
      #region Конструктор

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

      #region Поля

      private Dictionary<int, int> _Map;

      #endregion

      #region Методы

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
        "Загрузка страниц адресных объектов \"" + FiasEnumNames.ToString(level, false) + "\" (" + pageAOGuids.Length.ToString() + ")",
        "Подготовка таблиц"});
      try
      {
        #region Создаем таблицы

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageAOGuids.Length);
        int pEmptyGuid = -1; // для справочника регионов
        for (int i = 0; i < pageAOGuids.Length; i++)
        {
          Guid g = pageAOGuids[i];
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("Обнаружен guid, задающий ненайденный уровень", "pageAOGuids");

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

          #region Справочник регионов

          // Для справочника регионов PARENTGUID=GuidEmpty.
          // В базе данных он хранится как NULL. В результате запрос будет неоптимальным. 
          // Из-за наличия COALESCE() не будет использован индекс по полю PARENTGUID.
          // Обрабатываем в отдельном запросе

          if (pEmptyGuid >= 0)
          {
            // Убираем Guid.Empty из списка pageAOGuids
            DataTools.DeleteFromArray(ref pageAOGuids, pEmptyGuid, 1);

            // Отдельный запрос
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

          #region Заполняем таблицы

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

        #region Создание коллекции страниц

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
        "Загрузка страниц зданий (" + pageAOGuids.Length.ToString() + ")",
        "Подготовка таблиц"});
      try
      {
        #region Создаем таблицы

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageAOGuids.Length);
        foreach (Guid g in pageAOGuids)
        {
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("Обнаружен guid, задающий ненайденный уровень", "pageAOGuids");
          if (g == Guid.Empty)
            throw new ArgumentException("Обнаружен Guid.Empty", "pageAOGuids");

          DataSet ds = new DataSet();
          //ds.RemotingFormat = SerializationFormat.Binary;
          DataTable table = _HouseTemplate.Clone();
          ds.Tables.Add(table);
          tables[g] = table;
        }

        #endregion

        using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
        {
          #region Заполняем таблицы

          DBxColumns columns = (DBxColumns.FromColumns(_HouseTemplate.Columns) + "AOGUID") - "TopFlag";
          int pParentGuid = columns.IndexOf("AOGUID");
          int pNHouseNum = columns.IndexOf("nHouseNum");
          int pNBuildNum = columns.IndexOf("nBuildNum");
          int pNStrucNum = columns.IndexOf("nStrucNum");
#if DEBUG
          if (pNHouseNum < 0)
            throw new BugException("Нет поля nHouseNum");
          if (pNBuildNum < 0)
            throw new BugException("Нет поля nBuildNum");
          if (pNStrucNum < 0)
            throw new BugException("Нет поля nStrucNum");
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

        #region Создание коллекции страниц

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
        "Загрузка страниц зданий (" + pageHouseGuids.Length.ToString() + ")",
        "Подготовка таблиц"});
      try
      {
        #region Создаем таблицы

        Dictionary<Guid, DataTable> tables = new Dictionary<Guid, DataTable>(pageHouseGuids.Length);
        foreach (Guid g in pageHouseGuids)
        {
          if (g == FiasTools.GuidNotFound)
            throw new ArgumentException("Обнаружен guid, задающий ненайденный уровень", "pageHouseGuids");
          if (g == Guid.Empty)
            throw new ArgumentException("Обнаружен Guid.Empty", "pageHouseGuids");

          DataSet ds = new DataSet();
          //ds.RemotingFormat = SerializationFormat.Binary;
          DataTable table = _RoomTemplate.Clone();
          ds.Tables.Add(table);
          tables[g] = table;
        }

        #endregion

        using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
        {
          #region Заполняем таблицы

          DBxColumns columns = (DBxColumns.FromColumns(_RoomTemplate.Columns) + "HOUSEGUID") - "TopFlag";
          int pParentGuid = columns.IndexOf("HOUSEGUID");
          int pNFlatNumber = columns.IndexOf("nFlatNumber");
          int pNRoomNumber = columns.IndexOf("nRoomNumber");
#if DEBUG
          if (pNFlatNumber < 0)
            throw new BugException("Нет поля nFlatNumber");
          if (pNRoomNumber < 0)
            throw new BugException("Нет поля nRoomNumber");
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

        #region Создание коллекции страниц

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

    #region Специальные страницы для редактора адреса

    public FiasCachedPageSpecialAddrOb GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetSpecialAddrObPage() started. pageType=" + pageType.ToString());

      _FiasDB.CheckIsReady();

      DataTable table = _AddrObTemplate.Clone();

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        #region Заполняем таблицы

        DBxColumns columns = DBxColumns.FromColumns(_AddrObTemplate.Columns) /*+ "PARENTGUID"*/ - "TopFlag";
        //int pParentGuid = columns.IndexOf("PARENTGUID");
        ColMapper map = new ColMapper(columns, _AddrObTemplate);

        List<DBxFilter> filters = new List<DBxFilter>();
        switch (pageType)
        {
          case FiasSpecialPageType.AllCities:
            if (pageAOGuid != Guid.Empty)
              throw new ArgumentException("pageAOGuid непустой");

            //filters.Add(new ValuesFilter("CENTSTATUS", new int[] { (int)FiasCenterStatus.Region, (int)FiasCenterStatus.RegionAndDistrict }));

            // Нужны только города, а не сельсоветы и прочее, что может быть на уровне "Город"
            IdList CityAOTypeIds = con.GetIds("AOType", new AndFilter(new ValueFilter("LEVEL", (int)FiasLevel.City),
              new ValueFilter("SOCRNAME", "Город"))); // может быть несколько сокращений: "г." и "г".
            if (CityAOTypeIds.Count > 0)
              filters.Add(new IdsFilter("AOTypeId", CityAOTypeIds));

            break;
          case FiasSpecialPageType.AllDistricts:
            if (pageAOGuid != Guid.Empty)
              throw new ArgumentException("pageAOGuid непустой");

            // В Москве есть поселения, находящиеся в справочнике районов
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


      #region Создание страницы

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
    /// Обратная замена значений для полей номеров домов и квартир.
    /// Например, если поле "nHouseNum" имеет значение 25 (от 1 до 255), а поле "HOUSENUM" имеет значение NULL,
    /// то поле "HOUSENUM" заменяется на "25"
    /// </summary>
    /// <param name="row">Строка буферизуемой таблицы</param>
    /// <param name="pNColumn">Позиция числового столбца, который идет до основного поля</param>
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
        throw new ArgumentException("Таблица " + table.TableName + " не содержит поля \"" + nameNColumn + "\"", "nameNColumn");
#endif

      foreach (DataRow row in table.Rows)
        ExpandNumInt(row, pNColumn);
    }


    /// <summary>
    /// Установка поля "TopFlag" в загруженной таблице страницы
    /// </summary>
    /// <param name="table">Заполненная таблица</param>
    /// <param name="guidColName">Поле "устойчивого" идентификатора адресного объекта</param>
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

    #region Корректировка дат

    private static readonly DateTime _DT19991231 = new DateTime(1999, 12, 31);
    private static readonly int _NDT19991231 = (int)(new DateTime(1999, 12, 31).ToOADate());
    private static readonly int _NDT99991231 = (int)(DateTime.MaxValue.ToOADate());

    private void CorrectRowDates(DataRow row, FiasTableType tableType)
    {
      // В ФИАСе от 02.03.2020 для всех действующих адресных объектов конечная дата задана как 31.12.1999
      // Для квартир - тоже
      // Для домов - правильная конечная дата 31.12.9999

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

    #region Таблица сокращений

    public FiasCachedAOTypes GetAOTypes()
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetAOTypes() started.");

      _FiasDB.CheckIsReady();

      FiasCachedAOTypes page;

      using (DBxConBase con = _FiasDB.DB.MainEntry.CreateCon())
      {
        DataTable table = con.FillSelect("AOType");

        // В версии ФИАСа от 02.03.2020 зачем-то заменены некоторые уровни:
        // 18 - здания (должно быть 8)
        // 19 - помещения (должно быть 9)
        // 20 - земельный участок (должно быть 75, не нужный уровень)
        // 21 - х.з., похоже на 35, но 35 тоже есть
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

    #region Поиск адресов

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
            throw new InvalidOperationException("Полнотекстный поиск не поддерживается");
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
    /// Форматирование фильтра MATCH вместо LIKE
    /// </summary>
    private class MyFormatter : DBxSqlChainFormatter
    {
    #region Конструктор

      public MyFormatter(DBxSqlFormatter source)
        :base(source)
      { 
      }

    #endregion

    #region Форматирование фильтра

      protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
      {
        buffer.FormatExpression(filter.Expression, new DBxFormatExpressionInfo());
        buffer.SB.Append(" MATCH ");
        buffer.FormatValue(filter.Value, DBxColumnType.String); // звездочка была добавлена заранее
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

      // Этот фильтр будет заменен на "MATCH" при форматировании в классе MyFormatter
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
        levels = FiasTools.AOLevels; // все возможные уровни для поиска
      if (searchParams.StartAddress != null)
      { 
        FiasLevel parentLevel=searchParams.StartAddress.Guids.BottomLevel;
        if (parentLevel != FiasLevel.Unknown)
        { 
          // Список дистанций
          SingleScopeList<int> distList = new SingleScopeList<int>();
          for (int i = 0; i < levels.Length; i++)
            FiasTools.GetPossibleInheritanceDistance(parentLevel, levels[i], distList);

          distList.Sort();
          if (distList.Count == 0)
            throw new ArgumentException("Для базового адреса " + searchParams.StartAddress.ToString() + " не удалось определить уровень наследования искомых уровней адроесных объектов", "searchParams.StartAddress");
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
      // Нужно заменить связь между таблицами AddrOb и AddrObFTS с "LEFT JOIN" на "JOIN".
      // А остальные связи между AddrOb и AddrOb оставить "LEFT JOIN".
      int p = cmdText.IndexOf("LEFT JOIN");
      cmdText = cmdText.Substring(0, p) + cmdText.Substring(p + 5); // убрали LEFT

      return con.SQLExecuteDataTable(cmdText);
    }
#else

    private DataTable FindAddressesFTS3(FiasAddressSearchParams searchParams, DBxConBase con)
    {
      FiasLevel[] levels;
      if (searchParams.Levels != null)
        levels = searchParams.Levels;
      else
        levels = FiasTools.AOLevels; // все возможные уровни для поиска

      // Максимальная дистанция для сравнения с PARENTAOGUID.
      // Если 0, то базовый адрес не задан.
      // Если 1, то сравнение только с текущим PARENTGUID (например, поиск улицы в населенном пункте или нас. пункта в районе)
      // Если 2, то на один уровень больше (например, улицы в городе)
      int maxDist = 0;
      Guid parentGuid = Guid.Empty;
      if (searchParams.StartAddress != null)
      {
        FiasLevel parentLevel = searchParams.StartAddress.GuidBottomLevel;
        if (parentLevel != FiasLevel.Unknown)
        {
          parentGuid = searchParams.StartAddress.GetGuid(parentLevel);
          // Список дистанций
          SingleScopeList<int> distList = new SingleScopeList<int>();
          for (int i = 0; i < levels.Length; i++)
            FiasTools.GetPossibleInheritanceDistance(parentLevel, levels[i], distList);

          distList.Sort();
          if (distList.Count == 0)
            throw new ArgumentException("Для базового адреса " + searchParams.StartAddress.ToString() + " (уровень " + FiasEnumNames.ToString(parentLevel, true) + ") не удалось определить уровень наследования искомых уровней адресных объектов", "searchParams");
          maxDist = distList[distList.Count - 1];
        }
      }


      DBxSqlBuffer buffer = new DBxSqlBuffer(_FiasDB.DB.Formatter);

      // Создаем SQL-запрос вручную
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
      // Открывающие скобки
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

    #region Доступ к истории

    public DataSet GetTableForGuid(Guid guid, FiasTableType tableType)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasDBUnbufferedSource.GetTableForGuid() started.");

      _FiasDB.CheckIsReady();

      if (guid == Guid.Empty || guid == FiasTools.GuidNotFound)
        throw new ArgumentException("Недопустимый GUID=" + guid.ToString(), "guid");

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
          throw new ArgumentException("Неизвестная таблица " + tableType.ToString(), "tableType");
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
      // Пока не знаю, как сделать
      // Для AddrOb и Room порядок сортировки должен быть:
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

        // Такие же действия, как при загрузке буферизованных страниц
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

    #region Таблица обновлений

    /// <summary>
    /// Загружает таблицу истории обновлений классификатора
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
    /// Ничего не делает
    /// </summary>
    void IFiasSource.UpdateActualDate()
    {
    }

    #endregion
  }
}
