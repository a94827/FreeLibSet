using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Remoting;
using System.Data;
using System.Diagnostics;

namespace AgeyevAV.FIAS
{

  /// <summary>
  /// Этот класс может использоваться на стороне клиента и вставляться в цепочку между клиентским FiasCachedSource и объектом, полученным с сервера
  /// </summary>
  public sealed class FiasDistributedSource : IFiasSource
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, присоединенный к источнику на сервере.
    /// В качестве источника <paramref name="proxy"/> должен использоваться
    /// объект, создаваемый вызовом FiasDB.Source.CreateProxy(), переданный по сети.
    /// </summary>
    /// <param name="proxy">Данные для конструктора. Не может быть null</param>
    /// <param name="execProcList">Обработчик выполнения процедур. Для клиента WinForms следует передать ссылку на EFPApp.ExecProcList</param>
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
    /// Обычно следует использовать перегрузку конструктора, принимающую FiasSourceProxy.
    /// Этот вариант конструктора используется, когда на клиенте загрузка модуля FIAS.dll является отложенной
    /// и нежелательно передавать объект FiasSourceProxy заранее.
    /// Напротив, передача интерфейса IFiasSource клиенту как System.Object, не приводит к загрузке FIAS.dll, 
    /// если не приводить его сразу к правильному типу. Когда клиенту потребуется FiasCachedSource и FiasDistributedSource, вызывается этот
    /// конструктор. При этом, однако, будет выполнено лишнее обращение к серверу для получения недостающей информации
    /// </summary>
    /// <param name="source">Интерфейс, переданный с сервера</param>
    /// <param name="execProcList">Обработчик выполнения процедур. Для клиента WinForms следует передать ссылку на EFPApp.ExecProcList</param>
    public FiasDistributedSource(IFiasSource source, ExecProcCallList execProcList)
      : this(source.CreateProxy(), execProcList)
    {
    }

    #endregion

    #region Свойства

    private IFiasSource _BaseSource;

    private ExecProcCallList _ExecProcList;

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в качестве части ключа при кэшировании данных.
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    /// <summary>
    /// Настройки базы данных
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// Внутренние настройки классификатора
    /// </summary>
    public FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private FiasInternalSettings _InternalSettings;

    /// <summary>
    /// Возвращает статистику по базе данных
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private FiasDBStat _DBStat;

    /// <summary>
    /// Дата актульности
    /// </summary>
    public DateTime ActualDate { get { return DBStat.ActualDate; } }

    /// <summary>
    /// Произвольные пользовательские данные.
    /// Когда выполняется асинхронный вызов процедура на сервере, этот объект передается на сервер и доступен в обработчиках событий, связанных с процедурой.
    /// Свойство может использоваться, например, для идентификации пользователя.
    /// В обратную сторону, от сервера к клиенту, данные не передаются.
    /// По умолчанию - null.
    /// Объект должен быть сериализуемым.
    /// </summary>
    public object UserData { get { return _UserData; } set { _UserData = value; } }
    private object _UserData;

    #endregion

    #region Методы, вызывающие удаленную процедуру

    IDictionary<Guid, FiasGuidInfo> IFiasSource.GetGuidInfo(Guid[] guids, FiasTableType tableType)
    {
      // Для единственного идентификатора нет смысла запускать что-то асинхронно
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
      callItem.DisplayName = "Вызов сервера для получения данных ФИАС (" + args.GetString("Action") + ")";
      return _ExecProcList.ExecuteAsyncAndWait(callItem);
    }

    #endregion

    #region Методы и свойства, передаваемые базовому объекту

    DataTable IFiasSource.GetClassifUpdateTable()
    {
      return _BaseSource.GetClassifUpdateTable();
    }

    DistributedCallData IFiasSource.StartDistributedCall(NamedValues args, object userData)
    {
      return _BaseSource.StartDistributedCall(args, userData);
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

    #region Прочие методы

    /// <summary>
    /// Создает прокси, которое нужно передать конструктору FiasCachedSource
    /// </summary>
    /// <returns>Прокси</returns>
    public FiasSourceProxy CreateProxy()
    {
      return new FiasSourceProxy(this, DBIdentity, DBSettings, InternalSettings, DBStat);
    }

    #endregion
  }
}
