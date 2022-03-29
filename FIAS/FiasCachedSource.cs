// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Caching;
using System.Data;
using FreeLibSet.Remoting;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.FIAS
{

  // Первоначально предполагалось хранить простой словарь по ключам-GUIDам дочерних объектов (зданий, помещений)
  // К сожалению, в базе данных ФИАС могут повторяться идентификаторы. То есть идентификаторы AOID и AOGUID могут совпадать
  // Нужно либо хранить отдельные словари, либо использовать различающиеся ключи

  internal struct FiasGuidKey : IEquatable<FiasGuidKey>
  {
    #region Конструктор

    public FiasGuidKey(Guid guid, FiasTableType tableType, bool isRecId)
    {
      _Guid = guid;
      _TableType = tableType;
      _IsRecId = isRecId;
    }

    #endregion

    #region Поля

    private readonly Guid _Guid;
    private readonly FiasTableType _TableType;
    private readonly bool _IsRecId;

    #endregion

    #region Для словаря

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
  /// Информация для словаря адресных объектов, домов и помещений.
  /// Не используется в прикладном коде
  /// </summary>
  [Serializable]
  public struct FiasGuidInfo
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="level"></param>
    /// <param name="parentGuid"></param>
    public FiasGuidInfo(FiasLevel level, Guid parentGuid)
    {
      _Level = level;
      _ParentGuid = parentGuid;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Уровень данного адресного объекта.
    /// Для домов возвращается FiasLevel.House
    /// Для помещений возвращается FiasLevel.Flat
    /// </summary>
    public FiasLevel Level { get { return _Level; } }
    private readonly FiasLevel _Level;

    /// <summary>
    /// Идентификатор родительского адресного объекта.
    /// Если Level=FiasLevel.Flat, то идентификатор дома.
    /// Для регионов возвращается Guid.Empty.
    /// </summary>
    public Guid ParentGuid { get { return _ParentGuid; } }
    private readonly Guid _ParentGuid;

    /// <summary>
    /// Экземпляр "GUID не найден"
    /// </summary>
    public static readonly FiasGuidInfo NotFound = new FiasGuidInfo((FiasLevel)0, FiasTools.GuidNotFound);

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ParentGuid.ToString() + " (" + FiasEnumNames.ToString(_Level, false) + ")";
    }

    #endregion
  }

  #region IFiasSource

  /// <summary>
  /// Интерфейс для получения страниц классификатора.
  /// Методы интерфейса не должны использоваться в прикладном коде
  /// </summary>
  public interface IFiasSource
  {
    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в качестве части ключа при кэшировании данных.
    /// </summary>
    string DBIdentity { get; }

    /// <summary>
    /// Настройки базы данных
    /// </summary>
    FiasDBSettings DBSettings { get; }

    /// <summary>
    /// Внутренние настройки классификатора
    /// </summary>
    FiasInternalSettings InternalSettings { get; }

    /// <summary>
    /// Дата актуальности классификатора
    /// </summary>
    DateTime ActualDate { get; }


    /// <summary>
    /// Проверить изменение даты актуальности.
    /// Этот метод может вызываться клиентом, если есть предположение, что дата актуальности могла измениться
    /// </summary>
    void UpdateActualDate();

    /// <summary>
    /// Создает пакет данных для передачи от сервера к клиенту для конструктора FiasCachedSource для получения фиксированной информации за один вызов по сети,
    /// а не по частям
    /// </summary>
    /// <returns></returns>
    FiasSourceProxy CreateProxy();

    /// <summary>
    /// Получить словарь отношений адресных объектов.
    /// В словаре ключ - GUID дочернего адресного объекта, дома помещения, значение уровень этого объекта и GUID родительского адресного объекта
    /// </summary>
    /// <param name="guids">Идентификаторы дочерних объектов, для которых выполняется поиск</param>
    /// <param name="tableType">Тип таблицы. Если тип интересующих объектов неизвестен, можно задавать Unknown</param>
    /// <returns>Словарь отношений</returns>
    IDictionary<Guid, FiasGuidInfo> GetGuidInfo(Guid[] guids, FiasTableType tableType);

    /// <summary>
    /// Получить словарь записей объектов.
    /// В словаре ключ - ID записи дочернего адресного объекта, дома помещения, значение уровень этого объекта и GUID родительского адресного объекта
    /// </summary>
    /// <param name="recIds">Идентификаторы дочерних объектов, для которых выполняется поиск</param>
    /// <param name="tableType">Тип таблицы. Если тип интересующих объектов неизвестен, можно задавать Unknown</param>
    /// <returns>Словарь отношений</returns>
    IDictionary<Guid, FiasGuidInfo> GetRecIdInfo(Guid[] recIds, FiasTableType tableType);

    /// <summary>
    /// Получить страницы классификатора для заданных идентификаторов адресных объектов.
    /// </summary>
    /// <param name="level">Уровень адресных объектов, которые будут загружены в таблицы</param>
    /// <param name="pageAOGuids">Идентификаторы родительских объектов, для которых выполняется поиск</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageAddrOb> GetAddrObPages(FiasLevel level, Guid[] pageAOGuids);

    /// <summary>
    /// Получить специальную страницу классификатора.
    /// Используется редактором адреса
    /// </summary>
    /// <param name="pageType">Тип страницы</param>
    /// <param name="pageAOGuid">Идентификатор родительского объекта</param>
    /// <returns></returns>
    FiasCachedPageSpecialAddrOb GetSpecialAddrObPage(FiasSpecialPageType pageType, Guid pageAOGuid);

    /// <summary>
    /// Получить страницы классификатора домов для заданных идентификаторов адресных объектов.
    /// </summary>
    /// <param name="pageAOGuids">Идентификаторы родительских объектов, для которых выполняется поиск</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageHouse> GetHousePages(Guid[] pageAOGuids);

    /// <summary>
    /// Получить страницы классификатора помещений для заданных идентификаторов домов.
    /// </summary>
    /// <param name="pageHouseGuids">Идентификаторы родительских объектов (домов), для которых выполняется поиск</param>
    /// <returns></returns>
    IDictionary<Guid, FiasCachedPageRoom> GetRoomPages(Guid[] pageHouseGuids);

    /// <summary>
    /// Загрузить буферизованную таблицу сокращений
    /// </summary>
    /// <returns>Буферизованный объект</returns>
    FiasCachedAOTypes GetAOTypes();

    /// <summary>
    /// Выполняет поиск адресов
    /// </summary>
    /// <param name="searchParams">Параметры поиска</param>
    /// <returns>Результаты поиска</returns>
    DataSet FindAddresses(FiasAddressSearchParams searchParams);

    /// <summary>
    /// Загрузить таблицу для заданного "устойчивого" адресного объекта, дома или помещения
    /// </summary>
    /// <param name="guid">Значение AOGUID, HOUSEGUID или ROOMGUID</param>
    /// <param name="tableType">Тип таблицы</param>
    /// <returns>Таблица данных</returns>
    DataSet GetTableForGuid(Guid guid, FiasTableType tableType);

    /// <summary>
    /// Возвращает статистику по базе данных классификатора.
    /// </summary>
    FiasDBStat DBStat { get; }

    /// <summary>
    /// Загружает таблицу истории обновлений классификатора
    /// </summary>
    /// <returns></returns>
    DataTable GetClassifUpdateTable();

    /// <summary>
    /// Начинает "разделенный" запуск процедуры на сервере.
    /// Используется FiasDistributedSource
    /// </summary>
    /// <param name="args">Упакованные данные запроса</param>
    /// <param name="userData">Свойство FiasDistributedSource.UserData</param>
    /// <returns>Результаты разделенного запуска</returns>
    DistributedCallData StartDistributedCall(NamedValues args, object userData);
  }

  #endregion

  /// <summary>
  /// Передача классификатора от сервера к клиенту.
  /// Требуется конструктору FiasCachedSource. Создается вызовом IFiasSource.CreateProxy().
  /// Не содержит свойств, доступных из прикладного кода
  /// </summary>
  [Serializable]
  public sealed class FiasSourceProxy
  {
    #region Защищенный конструктор

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

    #region Свойства

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
  /// Буферизация страниц классификатора.
  /// Создается пользовательским кодом в одном экземпляре на стороне клиента. Клиент отвечает за удаление этого объекта.
  /// На стороне сервера создается автоматически в FiasDB
  /// Самоспонсируемый объект для net remoting.
  /// Класс является потокобезопасным.
  /// </summary>
  public class FiasCachedSource : MarshalByRefDisposableObject, IFiasSource
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, присоединенный к другому источнику.
    /// При создании на стороне клиента, в качестве источника <paramref name="proxy"/> должен использоваться
    /// объект, создаваемый вызовом FiasDB.Source.CreateProxy(), переданный по сети.
    /// </summary>
    /// <param name="proxy">Данные для конструктора. Не может быть null</param>
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


      _CacheFirstKey = DataTools.MD5SumFromString(_DBIdentity); // тут могут быть нехорошие символы
      _CacheFirstKeySimpleArray = new string[1] { _CacheFirstKey };

      _GuidDict = new DictionaryWithMRU<FiasGuidKey, FiasGuidInfo>();
      _GuidDict.MaxCapacity = 10000;

      _TextCache = new FiasAddressTextCache(this);

      SetCacheVersion();
    }

    /// <summary>
    /// Обычно следует использовать перегрузку конструктора, принимающую FiasSourceProxy.
    /// Этот вариант конструктора используется, когда на клиенте загрузка модуля FIAS.dll является отложенной
    /// и нежелательно передавать объект FiasSourceProxy заранее.
    /// Напротив, передача интерфейса IFiasSource клиенту как System.Object, не приводит к загрузке FIAS.dll, 
    /// если не приводить его сразу к правильному типу. Когда клиенту потребуется FiasCachedSource, вызывается этот
    /// конструктор. При этом, однако, будет выполнено лишнее обращение к серверу для получения недостающей информации
    /// </summary>
    /// <param name="source"></param>
    public FiasCachedSource(IFiasSource source)
      : this(source.CreateProxy())
    {
    }

    #endregion

    #region Свойства

    private readonly IFiasSource _BaseSource;

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в качестве части ключа при кэшировании данных.
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    /// <summary>
    /// Первый ключ для кэширования.
    /// Задает имена подкаталогов второго уровня в Cache.Params.PersistDir.
    /// </summary>
    public string CacheFirstKey { get { return _CacheFirstKey; } }
    private readonly string _CacheFirstKey;

    /// <summary>
    /// Массив из одного элемента _CacheFirstKey
    /// </summary>
    private readonly string[] _CacheFirstKeySimpleArray;

    /// <summary>
    /// Настройки базы данных
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// Внутренние настройки классификатора
    /// </summary>
    public FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private readonly FiasInternalSettings _InternalSettings;

    #endregion

    #region Словари GUID'ов

    private readonly DictionaryWithMRU<FiasGuidKey, FiasGuidInfo> _GuidDict;

    /// <summary>
    /// Не используется в прикладном коде.
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
    /// Не используется в прикладном коде.
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

      #region Поиск в кэше

      SingleScopeList<Guid> wanted = null;

      lock (_GuidDict)
      {
        for (int i = 0; i < guids.Length; i++)
        {
          FiasGuidKey key = new FiasGuidKey(guids[i], tableType, isRecIds);
          FiasGuidInfo info;
          if (_GuidDict.TryGetValue(key, out info))
            dict[guids[i]] = info; // не используем dict.Add(), так как в массиве guids могут быть повторы
          else
          {
            // нет в кэше
            if (wanted == null)
              wanted = new SingleScopeList<Guid>();
            wanted.Add(guids[i]);
          }
        }
      }

      #endregion

      #region Запрос недостающих значений

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

    #region Буферизованные страницы

    #region FiasCachedPageAddrOb

    private string[] GetAddrObPageKeys(FiasLevel level, Guid pageAOGuid)
    {
      return new string[] { _CacheFirstKey, level.ToString(), pageAOGuid.ToString("N") };
    }

    /// <summary>
    /// Не используется в прикладном коде.
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
        "Поиск страниц адресных объектов \""+FiasEnumNames.ToString(level, false)+"\" ("+pageAOGuids.Length.ToString()+") в кэше",
        "Запрос недостающих страниц в базе данных",
        "Запись страниц в кэш"});
      try
      {

        #region Поиск в кэше

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

        #region Запрос недостающих страниц

        if (wanted != null)
        {
          spl.PhaseText = "Запрос недостающих страниц в базе данных (" + wanted.Count.ToString() + ")";
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
            lock (_GuidDict) // 06.09.2021
            {
              pair2.Value.AddToGuidDict(_GuidDict);
            }
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
    /// Не используется в прикладном коде.
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
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="pageAOGuids"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasCachedPageHouse> GetHousePages(Guid[] pageAOGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetHousePages() started. pageAOGuids.Length=" + pageAOGuids.Length.ToString());

      Dictionary<Guid, FiasCachedPageHouse> dict = new Dictionary<Guid, FiasCachedPageHouse>();

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "Поиск страниц зданий ("+pageAOGuids.Length.ToString()+") в кэше",
        "Запрос недостающих страниц в базе данных",
        "Запись страниц в кэш"});
      try
      {
        #region Поиск в кэше

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

        #region Запрос недостающих страниц

        if (wanted != null)
        {
          spl.PhaseText = "Запрос недостающих страниц в базе данных (" + wanted.Count.ToString() + ")";
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
            lock (_GuidDict) // 06.09.2021
            {
              pair2.Value.AddToGuidDict(_GuidDict);
            }
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
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="pageHouseGuids"></param>
    /// <returns></returns>
    public IDictionary<Guid, FiasCachedPageRoom> GetRoomPages(Guid[] pageHouseGuids)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.GetRoomPages() started. pageHouseGuids.Length=" + pageHouseGuids.Length.ToString());

      Dictionary<Guid, FiasCachedPageRoom> dict = new Dictionary<Guid, FiasCachedPageRoom>();

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
        "Поиск страниц помещений ("+pageHouseGuids.Length.ToString()+") в кэше",
        "Запрос недостающих страниц в базе данных",
        "Запись страниц в кэш"});
      try
      {
        #region Поиск в кэше

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

        #region Запрос недостающих страниц

        if (wanted != null)
        {
          spl.PhaseText = "Запрос недостающих страниц в базе данных (" + wanted.Count.ToString() + ")";
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
            lock (_GuidDict) // 06.09.2021
            {
              pair2.Value.AddToGuidDict(_GuidDict);
            }
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

    #region Таблица сокращений

    /// <summary>
    /// Не используется в прикладном коде.
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

    #region Текстовое представление адреса

    /// <summary>
    /// Кэширование текстовых представлений для кодированных строк адреса
    /// </summary>
    public FiasAddressTextCache TextCache { get { return _TextCache; } }
    private FiasAddressTextCache _TextCache;

    #endregion

    #region Прочие методы

    /// <summary>
    /// Выполняет очистку кэша.
    /// Очищается кэш только для текущего объекта. Для BaseSource очистка не выполняется
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
      lock (_GuidDict) // 06.09.2021
      {
        _GuidDict.Clear();
      }

      _TextCache.Clear();

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasCachedSource.ClearCache() finished.");
    }

    /// <summary>
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="searchParams"></param>
    /// <returns></returns>
    public DataSet FindAddresses(FiasAddressSearchParams searchParams)
    {
      return _BaseSource.FindAddresses(searchParams);
    }

    /// <summary>
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="tableType"></param>
    /// <returns></returns>
    public DataSet GetTableForGuid(Guid guid, FiasTableType tableType)
    {
      return _BaseSource.GetTableForGuid(guid, tableType);
    }

    /// <summary>
    /// Создает объект, который может быть передан конструктору FiasCachedSource при организации сложной цепочки
    /// </summary>
    /// <returns></returns>
    public FiasSourceProxy CreateProxy()
    {
      return new FiasSourceProxy(this, _DBIdentity, _DBSettings, _InternalSettings, _DBStat);
    }


    /// <summary>
    /// Загружает таблицу истории обновлений классификатора
    /// </summary>
    /// <returns></returns>
    public DataTable GetClassifUpdateTable()
    {
      return _BaseSource.GetClassifUpdateTable();
    }

    /// <summary>
    /// Вызывает метод базового источника
    /// </summary>
    /// <param name="args"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    public virtual DistributedCallData StartDistributedCall(NamedValues args, object userData)
    {
      return _BaseSource.StartDistributedCall(args, userData);
    }

    #endregion

    #region Дата актуальности и статистика

    /// <summary>
    /// Возвращает статистику по базе данных
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private FiasDBStat _DBStat;

    /// <summary>
    /// Дата актульности
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
    /// Обновление версии в процессе работы.
    /// Используется приложением клиента. Клиент может, например, по таймеру запрашивать у сервера дату актуальности классификатора
    /// и вызывать этот метод, если дата изменилась.
    /// Метод запрашивает у источника свойство ActualDate. Если дата актульности совпадает с текущей, никаких действий не выполняется.
    /// Иначе выполняется очистка кэша и обновляются свойства FiasCachedSource.ActualDate и DBStat
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

  #region Перечисление FiasSpecialPageType

  /// <summary>
  /// Специальные типы страниц, которые можно загружать из классификатора
  /// </summary>
  [Serializable]
  public enum FiasSpecialPageType
  {
    /// <summary>
    /// Все города (в пределах РФ)
    /// </summary>
    AllCities,

    /// <summary>
    /// Районы всех регионов (в пределах РФ)
    /// </summary>
    AllDistricts,


    /// <summary>
    /// Населенные пункты - столицы районов (в пределах региона)
    /// </summary>
    DistrictCapitals,
  }

  #endregion

  /// <summary>
  /// Кэширование текстового представления для кодированных строк адреса.
  /// На входе передается строка адреса в формате FiasAddressConvert, а на выходе получается строка FiasHander.Format().
  /// Класс является потокобезопасным.
  /// Перехватываются ошибки преобразования. Для них возвращаются пустые строки.
  /// </summary>
  public sealed class FiasAddressTextCache : MarshalByRefObject
  {
    #region Конструктор

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

    #region Свойства

    private FiasCachedSource _Source;

    /// <summary>
    /// Основная таблица кэша.
    /// Ключ - строка вида "КодАдреса|Формат", значение - текстовое представление.
    /// При всех обращениях используется блокировка этого объекта
    /// </summary>
    private DictionaryWithMRU<string, string> _Dict;

    /// <summary>
    /// Емкость списка.
    /// По умолчанию равна 10000 записей
    /// </summary>
    public int Capacity
    {
      get { lock (_Dict) { return _Dict.MaxCapacity; } }
      set { lock (_Dict) { _Dict.MaxCapacity = value; } }
    }

    /// <summary>
    /// Используется для преобразования кодированной строки в FiasAddress.
    /// Свойство FiasAddressConvert.FillAddress=false.
    /// </summary>
    private FiasAddressConvert _ConvertWithoutFill;

    /// <summary>
    /// Используется для вызова метода Format
    /// </summary>
    private FiasHandler _Handler;

    /// <summary>
    /// Для вывода форматированного текста
    /// </summary>
    private StringBuilder _SB;

    #endregion

    #region Получение данных

    /// <summary>
    /// Получение текста для одного адреса
    /// </summary>
    /// <param name="addressCode">Кодированная строка адреса</param>
    /// <param name="format">Формат</param>
    /// <returns>Кэшированное текстовое представление</returns>
    public string Format(string addressCode, string format)
    {
      if (String.IsNullOrEmpty(addressCode))
        return String.Empty;

      string dictKey = addressCode + "|" + format; // ключ для словаря

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
        _Dict[dictKey] = text; // мог быть реентрантный вызов
      }

      return text;
    }

    /// <summary>
    /// Получение текстового представления для нескольких адресов.
    /// Для всех адресов используется один формат для текстового представления
    /// </summary>
    /// <param name="addressCodes">Массив кодированных строк адреса</param>
    /// <param name="format">Формат</param>
    /// <returns>Массив кэшированных текстовых представлений</returns>
    public string[] Format(string[] addressCodes, string format)
    {
      string[] textArray = new string[addressCodes.Length];

      // Ненайденные адреса
      SingleScopeStringList notFound = null;

      lock (_Dict)
      {
        for (int i = 0; i < addressCodes.Length; i++)
        {
          if (String.IsNullOrEmpty(addressCodes[i]))
            textArray[i] = String.Empty;
          else
          {
            string dictKey = addressCodes[i] + "|" + format; // ключ для словаря
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
            if (_ConvertWithoutFill.TryParse(s, out address)) // Не вызываем метод FillAddress()
              addrDict.Add(s, address);
          }
        }

        // Групповой вызов FillAddresses()
        if (addrDict.Count > 0) // проверка нужна, т.к. все адреса могут быть с ошибками
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
            string dictKey = addressCodes[i] + "|" + format; // ключ для словаря
            _Dict[dictKey] = textArray[i]; // мог быть реентрантный вызов
          }
        }
      }
      return textArray;
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Очищает кэш
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
