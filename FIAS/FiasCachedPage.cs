// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Remoting;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  // Буферизуемые данные

  #region Перечисление FiasSearchRowCount

  /// <summary>
  /// Результаты поиска строк
  /// </summary>
  [Serializable]
  public enum FiasSearchRowCount
  {
    /// <summary>
    /// Не найдено ни одной строки
    /// </summary>
    NotFound,

    /// <summary>
    /// Поиск завершен успешно.
    /// Найдена одна строка.
    /// </summary>
    Ok,

    /// <summary>
    /// Поиск завершен неудачно, так как найдено больше одной строки классификатора
    /// </summary>
    Multi,
  }

  #endregion

  /// <summary>
  /// Результаты поиска компонента адреса
  /// Не используется в прикладном коде
  /// </summary>
  [Serializable]
  public struct FiasSearchRowResult
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="count"></param>
    /// <param name="row"></param>
    public FiasSearchRowResult(FiasSearchRowCount count, DataRow row)
    {
      if (count == FiasSearchRowCount.Ok)
      {
        if (row == null)
          throw new ArgumentNullException("row");
      }
      else
      {
        if (row != null)
          throw new ArgumentException("row!=null", "row");
      }

      _Count = count;
      _Row = row;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Количество найденных строк: Ничего, одна или несколько
    /// </summary>
    public FiasSearchRowCount Count { get { return _Count; } }
    private readonly FiasSearchRowCount _Count;

    /// <summary>
    /// Строка таблицы кэша, если найдена ровно одна запись в классификаторе
    /// </summary>
    public DataRow Row { get { return _Row; } }
    private readonly DataRow _Row;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Count.ToString();
    }

    #endregion

    #region Статические свойства

    /// <summary>
    /// Статический экземпляр "Не нашли"
    /// </summary>
    public static readonly FiasSearchRowResult NotFound = new FiasSearchRowResult(FiasSearchRowCount.NotFound, null);

    /// <summary>
    /// Статический экземпляр "Нашли несколько строк"
    /// </summary>
    public static readonly FiasSearchRowResult Multi = new FiasSearchRowResult(FiasSearchRowCount.Multi, null);

    #endregion
  }

  /// <summary>
  /// Страница классификатора для адресных объектов.
  /// Этот класс не используется в пользовательском коде.
  /// Класс является потокобезопасным.
  /// </summary>
  [Serializable]
  public class FiasCachedPageAddrOb
  {
    #region Защищенный конструктор

    internal FiasCachedPageAddrOb(Guid pageAOGuid, FiasLevel level, DataSet ds)
    {
      _PageAOGuid = pageAOGuid;
      _Level = level;
      SerializationTools.PrepareDataSet(ds); // 19.01.2021
      _DS = ds;

      OnDeserializedMethod(new System.Runtime.Serialization.StreamingContext());
    }

    #endregion

    #region Поля

    /// <summary>
    /// GUID адресного объекта к которому относится страница
    /// </summary>
    internal Guid PageAOGuid { get { return _PageAOGuid; } }
    private readonly Guid _PageAOGuid;

    /// <summary>
    /// Уровень адресных объектов, которые загружены в таблице
    /// </summary>
    internal FiasLevel Level { get { return _Level; } }
    private readonly FiasLevel _Level;

    /// <summary>
    /// Внутренняя таблица данных "AddrOb"
    /// Первичным ключом таблицы является идентификатор записи AOID.
    /// DefaulView отсортирован по полю AOGUID
    /// Есть дополнительный DataView
    /// </summary>
    private readonly DataSet _DS;

    /// <summary>
    /// Объект для поиска строк по имени
    /// </summary>
    [NonSerialized]
    private DataView _dvOffName;


    /// <summary>
    /// Возвращает true, если на странице нет ни одной записи
    /// </summary>
    public bool IsEmpty { get { return _DS.Tables[0].Rows.Count == 0; } }

    /// <summary>
    /// Количество строк в таблице на странице (для отладочных целей)
    /// </summary>
    public int RowCount { get { return _DS.Tables[0].Rows.Count; } }

    #endregion

    #region Сериализация

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserializedMethod(System.Runtime.Serialization.StreamingContext context)
    {
      DataTools.SetPrimaryKey(_DS.Tables[0], "AOID");
      _DS.Tables[0].DefaultView.Sort = "AOGUID";
      _DS.Tables[0].DefaultView.RowFilter = "TopFlag=TRUE";

      _dvOffName = new DataView(_DS.Tables[0]);
      FiasTools.InitTopFlagAndDatesRowFilter(_dvOffName);
      _dvOffName.Sort = "OFFNAME";
    }

    #endregion

    #region Поиск

    internal DataRow FindRowByGuid(Guid guid)
    {

      int p = _DS.Tables[0].DefaultView.Find(guid);
      if (p >= 0)
        return _DS.Tables[0].DefaultView[p].Row;
      else
        return null;
    }

    internal DataRow FindRowByRecId(Guid recId)
    {
      return _DS.Tables[0].Rows.Find(recId);
    }

    /// <summary>
    /// Возвращает GUID родительского адресного объекта
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public Guid GetParentGuid(Guid guid)
    {
      DataRow row = FindRowByGuid(guid);
      if (row == null)
        return Guid.Empty;
      else
        return DataTools.GetGuid(row, "PARENTGUID");
    }

    #endregion

    #region Вспомогательные методы

    internal void AddToGuidDict(DictionaryWithMRU<FiasGuidKey, FiasGuidInfo> dict)
    {
      int pRecId = _DS.Tables[0].Columns.IndexOf("AOID");
      int pChild = _DS.Tables[0].Columns.IndexOf("AOGUID");
      int pParent = _DS.Tables[0].Columns.IndexOf("PARENTGUID");
      foreach (DataRow row in _DS.Tables[0].Rows)
      {
        FiasGuidKey key;
        key = new FiasGuidKey(DataTools.GetGuid(row[pChild]), FiasTableType.AddrOb, false);
        dict[key] = new FiasGuidInfo(_Level, DataTools.GetGuid(row[pParent]));
        key = new FiasGuidKey(DataTools.GetGuid(row[pRecId]), FiasTableType.AddrOb, true);
        dict[key] = new FiasGuidInfo(_Level, DataTools.GetGuid(row[pParent]));
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление объекта</returns>
    public override string ToString()
    {
      return Level.ToString() + " " + _PageAOGuid.ToString();
    }

    #endregion

    #region Методы и свойства для редактора

    /// <summary>
    /// Имена без сокращений для инициализации списка AutoComplete в поле ввода.
    /// В список входят только действующие элементы.
    /// </summary>
    public string[] Names
    {
      get
      {
        if (_Names != null)
          return _Names;

        lock (_DS)
        {
          if (_Names == null)
          {
            SingleScopeSortedList<string> lst = new SingleScopeSortedList<string>();
            foreach (DataRowView drv in _dvOffName)
            {
              lst.Add(DataTools.GetString(drv.Row, "OFFNAME"));
            }
            _Names = lst.ToArray();
          }
        }
        return _Names;
      }
    }
    [NonSerialized]
    private volatile string[] _Names;


    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный объект должен быть удален после того, как просмотр закрывается.
    /// В просмотр входят только актуальные записи.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasAddrObExtractor.
    /// </summary>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView()
    {
      return CreateDataView(true);
    }

    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный объект должен быть удален после того, как просмотр закрывается.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasAddrObExtractor.
    /// </summary>
    /// <param name="actualOnly">Если true, то в просмотр входят только актуальные записи.
    /// Если false, то фильтр не устанавливается. Однако наличие исторических записей зависит от FiasDBSettings.</param>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView(bool actualOnly)
    {
      DataView dv = new DataView(_DS.Tables[0]);
      if (actualOnly)
        FiasTools.InitTopFlagAndDatesRowFilter(dv);
      dv.Sort = "OFFNAME";
      return dv;
    }

    /// <summary>
    /// Поиск строки.
    /// Сначала выполняется поиск вместе с типом адресообразующего элемента, а затем - без него.
    /// Если есть больше одной подходящей строки (неоднозначность), то возвращается null
    /// </summary>
    /// <param name="name">Наименование адресообразующего элемента без сокращения</param>
    /// <param name="aoTypeId">Идентификатор типа адресообразующего элемента в таблице AOType</param>
    /// <returns>Строка таблицы или null</returns>
    public FiasSearchRowResult FindRow(string name, Int32 aoTypeId)
    {
      return FindRow(name, aoTypeId, false);
    }

    /// <summary>
    /// Поиск строки.
    /// Сначала выполняется поиск вместе с типом адресообразующего элемента, а затем - без него.
    /// Если есть больше одной подходящей строки (неоднозначность), то возвращается null
    /// </summary>
    /// <param name="name">Наименование адресообразующего элемента без сокращения</param>
    /// <param name="aoTypeId">Идентификатор типа адресообразующего элемента в таблице AOType</param>
    /// <param name="extSearch">True - выполнять расширенный поиск с подбором похожих наименований</param>
    /// <returns>Строка таблицы или null</returns>
    public FiasSearchRowResult FindRow(string name, Int32 aoTypeId, bool extSearch)
    {
      if (String.IsNullOrEmpty(name))
        return FiasSearchRowResult.NotFound;

      // Обычный поиск
      FiasSearchRowResult res = DoFindRowNorm(name, aoTypeId);
      if (res.Count == FiasSearchRowCount.Ok)
        return res;
      if (extSearch)
        res = DoFindRowExt(name, aoTypeId); // расширенный поиск
      return res;
    }

    /// <summary>
    /// Обычный поиск с использованием DataView.FindRows()
    /// </summary>
    private FiasSearchRowResult DoFindRowNorm(string name, Int32 aoTypeId)
    {
      DataRowView[] drvs = _dvOffName.FindRows(name);
      if (drvs.Length == 1)
        return new FiasSearchRowResult(FiasSearchRowCount.Ok, drvs[0].Row);

      if (aoTypeId == 0)
        return FiasSearchRowResult.NotFound;

      DataRow row = null;
      for (int i = 0; i < drvs.Length; i++)
      {
        if (DataTools.GetInt(drvs[i].Row, "AOTypeId") == aoTypeId)
        {
          if (row == null)
            row = drvs[i].Row;
          else
            return FiasSearchRowResult.Multi; // больше одной строки
        }
      }

      if (row == null)
        return FiasSearchRowResult.NotFound;
      else
        return new FiasSearchRowResult(FiasSearchRowCount.Ok, row);
    }

    /// <summary>
    /// Расширенный поиск с перебором строк
    /// </summary>
    private FiasSearchRowResult DoFindRowExt(string name, Int32 aoTypeId)
    {
      FiasAddrObName xname = new FiasAddrObName(name);
      DataRow row1 = null; // строка с совпадением сокращения
      DataRow row2 = null; // строка с любым сокращением
      int count2 = 0; // количество строк без совпадения сокращения
      foreach (DataRowView drv in _dvOffName)
      {
        FiasAddrObName xname2 = new FiasAddrObName(DataTools.GetString(drv.Row, "OFFNAME"));
        if (xname2 == xname)
        {
          // Есть совпадение по лексемам
          if (aoTypeId != 0)
          {
            Int32 aoTypeId2 = DataTools.GetInt(drv.Row, "AOTypeId");
            if (aoTypeId2 == aoTypeId)
            {
              if (row1 != null)
                return FiasSearchRowResult.Multi; // две строки с совпадающшим сокращением
              else
                row1 = drv.Row;
            }
          }

          count2++;
          row2 = drv.Row;
          // Здесь не прерываем поиск, если нашли несколько строк
          // Может быть, будет найдена ровно одна строка с совпадением сокращения.
        }
      }

      if (row1 != null)
        return new FiasSearchRowResult(FiasSearchRowCount.Ok, row1); // с совпадением сокращением
      else if (count2 == 1)
        return new FiasSearchRowResult(FiasSearchRowCount.Ok, row2); // без проверки сокращения
      else
        return FiasSearchRowResult.NotFound;
    }

    /// <summary>
    /// Устойчивые идентификаторы адресных объектов.
    /// В список входят только действующие элементы.
    /// </summary>
    public Guid[] AOGuids
    {
      get
      {
        if (_AOGuids != null)
          return _AOGuids;

        lock (_DS)
        {
          if (_AOGuids == null)
          {
            SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
            using (DataView dv = CreateDataView())
            {
              foreach (DataRowView drv in dv)
                lst.Add(DataTools.GetGuid(drv.Row, "AOGUID"));
            }
            _AOGuids = lst.ToArray();
          }
        }
        return _AOGuids;
      }
    }
    [NonSerialized]
    private volatile Guid[] _AOGuids;

    #endregion
  }

  /// <summary>
  /// Специальная страница классификатора адресных объектов
  /// Отдельный класс нужен, чтобы специальные страницы не путались при извлечении из кэша обычных страниц.
  /// Класс является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class FiasCachedPageSpecialAddrOb : FiasCachedPageAddrOb
  {
    #region Защищенный конструктор

    internal FiasCachedPageSpecialAddrOb(Guid pageAOGuid, FiasLevel level, FiasSpecialPageType specialPageType, DataSet ds)
      : base(pageAOGuid, level, ds)
    {
      _SpecialPageType = specialPageType;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Специальный тип страницы
    /// </summary>
    public FiasSpecialPageType SpecialPageType { get { return _SpecialPageType; } }
    private readonly FiasSpecialPageType _SpecialPageType;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Special " + _SpecialPageType.ToString() + " - " + base.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Страница классификатора для домов.
  /// Этот класс не используется в пользовательском коде.
  /// Класс является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class FiasCachedPageHouse
  {
    #region Защищенный конструктор

    internal FiasCachedPageHouse(Guid pageAOGuid, DataSet ds)
    {
      _PageAOGuid = pageAOGuid;

      SerializationTools.PrepareDataSet(ds); // 19.01.2021
      _DS = ds;

      OnDeserializedMethod(new System.Runtime.Serialization.StreamingContext());
    }

    #endregion

    #region Поля

    /// <summary>
    /// GUID адресного объекта к которому относится страница
    /// </summary>
    internal Guid PageAOGuid { get { return _PageAOGuid; } }
    private readonly Guid _PageAOGuid;

    /// <summary>
    /// Внутренняя таблица данных
    /// </summary>
    private readonly DataSet _DS;

    /// <summary>
    /// Объект для поиска домов, корпусов, строений.
    /// Имеет Sort="HOUSENUM". Поиск по корпусам и строениям выполняется перебором DataViewRow
    /// При работе блокировка не нужна
    /// </summary>
    [NonSerialized]
    private DataView _dvHouseNum;

    /// <summary>
    /// Возвращает true, если на странице нет ни одной записи
    /// </summary>
    public bool IsEmpty { get { return _DS.Tables[0].Rows.Count == 0; } }

    /// <summary>
    /// Количество строк в таблице на странице (для отладочных целей)
    /// </summary>
    public int RowCount { get { return _DS.Tables[0].Rows.Count; } }

    #endregion

    #region Сериализация

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserializedMethod(System.Runtime.Serialization.StreamingContext context)
    {
      DataTools.SetPrimaryKey(_DS.Tables[0], "HOUSEID");
      _DS.Tables[0].DefaultView.Sort = "HOUSEGUID";
      _DS.Tables[0].DefaultView.RowFilter = "TopFlag=TRUE";

      _dvHouseNum = new DataView(_DS.Tables[0]);
      FiasTools.InitTopFlagAndDatesRowFilter(_dvHouseNum);
      _dvHouseNum.Sort = "HOUSENUM";
    }

    #endregion

    #region Поиск

    internal DataRow FindRowByGuid(Guid guid)
    {
      int p = _DS.Tables[0].DefaultView.Find(guid);
      if (p >= 0)
        return _DS.Tables[0].DefaultView[p].Row;
      else
        return null;
    }

    internal DataRow FindRowByRecId(Guid recId)
    {
      return _DS.Tables[0].Rows.Find(recId);
    }

    #endregion

    #region Вспомогательные методы

    internal void AddToGuidDict(DictionaryWithMRU<FiasGuidKey, FiasGuidInfo> dict)
    {
      int pRecId = _DS.Tables[0].Columns.IndexOf("HOUSEID");
      int pChild = _DS.Tables[0].Columns.IndexOf("HOUSEGUID");
      FiasGuidInfo info = new FiasGuidInfo(FiasLevel.House, _PageAOGuid);
      foreach (DataRow row in _DS.Tables[0].Rows)
      {
        FiasGuidKey key;
        key = new FiasGuidKey(DataTools.GetGuid(row[pChild]), FiasTableType.House, false);
        dict[key] = info;
        key = new FiasGuidKey(DataTools.GetGuid(row[pRecId]), FiasTableType.House, true);
        dict[key] = info;
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление объекта</returns>
    public override string ToString()
    {
      return _PageAOGuid.ToString();
    }

    #endregion

    #region Методы и свойства для редактора

    /// <summary>
    /// Номера домов без сокращений для инициализации списка AutoComplete в поле ввода
    /// </summary>
    public string[] GetHouseNums()
    {
      if (_HouseNums != null)
        return _HouseNums;

      lock (_DS)
      {
        if (_HouseNums == null)
        {
          SingleScopeSortedList<string> lst = new SingleScopeSortedList<string>();
          foreach (DataRowView drv in _dvHouseNum)
          {
            string h = DataTools.GetString(drv.Row, "HOUSENUM");
            if (h.Length > 0)
              lst.Add(h);
          }
          _HouseNums = lst.ToArray();
        }
      }
      return _HouseNums;
    }
    [NonSerialized]
    private volatile string[] _HouseNums;

    // Для корпусов и строений данные не буферизуются

    /// <summary>
    /// Получить номера корпусов для заданного номера дома
    /// </summary>
    /// <param name="houseNum">Номер дома</param>
    /// <returns>Список корпусов</returns>
    public string[] GetBuildingNums(string houseNum)
    {
      if (houseNum == null)
        houseNum = String.Empty;

      DataRowView[] drvs = _dvHouseNum.FindRows(houseNum);
      SingleScopeList<String> lst = new SingleScopeList<string>();
      for (int i = 0; i < drvs.Length; i++)
      {
        string b = DataTools.GetString(drvs[i].Row, "BUILDNUM");
        if (b.Length > 0)
          lst.Add(b);
      }
      return lst.ToArray();
    }


    /// <summary>
    /// Получить номера строений для заданного номера дома и корпуса.
    /// Может вызываться с пустыми аргументами, так как строения могут задаваться самостоятельно
    /// </summary>
    /// <param name="houseNum">Номер дома</param>
    /// <param name="buildNum">Номер корпуса</param>
    /// <returns>Список корпусов</returns>
    public string[] GetStrNums(string houseNum, string buildNum)
    {
      if (houseNum == null)
        houseNum = String.Empty;
      if (buildNum == null)
        buildNum = String.Empty;

      DataRowView[] drvs = _dvHouseNum.FindRows(houseNum);
      SingleScopeList<String> lst = new SingleScopeList<string>();
      for (int i = 0; i < drvs.Length; i++)
      {
        string b = DataTools.GetString(drvs[i].Row, "BUILDNUM");
        if (b == buildNum)
        {
          string s = DataTools.GetString(drvs[i].Row, "STRUCNUM");
          if (s.Length > 0)
            lst.Add(s);
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный список должен быть удален после того, как просмотр закрывается.
    /// В просмотр входят только актуальные записи.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasHouseExtractor.
    /// </summary>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView()
    {
      return CreateDataView(true);
    }

    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный объект должен быть удален после того, как просмотр закрывается.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasHouseExtractor.
    /// </summary>
    /// <param name="actualOnly">Если true, то в просмотр входят только актуальные записи.
    /// Если false, то фильтр не устанавливается. Однако наличие исторических записей зависит от FiasDBSettings.</param>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView(bool actualOnly)
    {
      DataView dv = new DataView(_DS.Tables[0]);
      if (actualOnly)
        FiasTools.InitTopFlagAndDatesRowFilter(dv);
      dv.Sort = "nHouseNum,HOUSENUM,nBuildNum,BUILDNUM,STRSTATUS,nStrucNum,STRUCNUM";
      return dv;
    }

    /// <summary>
    /// Поиск строки.
    /// Если есть больше одной подходящей строки (неоднозначность), то возвращается null
    /// </summary>
    /// <returns>Строка таблицы или null</returns>
    public DataRow FindRow(string houseNum, FiasEstateStatus estStatus, string buildNum, string strNum, FiasStructureStatus strStatus)
    {
      if (houseNum == null)
        houseNum = String.Empty;
      if (buildNum == null)
        buildNum = String.Empty;
      if (strNum == null)
        strNum = String.Empty;
      if (String.IsNullOrEmpty(strNum))
        strStatus = FiasStructureStatus.Unknown;

      if (String.IsNullOrEmpty(houseNum) && String.IsNullOrEmpty(buildNum) && String.IsNullOrEmpty(strNum))
        return null;

      DataRowView[] drvs;
      if (houseNum.Length == 0)
        drvs = _dvHouseNum.FindRows(DBNull.Value);
      else
        drvs = _dvHouseNum.FindRows(houseNum);

      DataRow row = DoFindRow(drvs, estStatus, buildNum, strNum, strStatus, true);
      if (row != null)
        return row;
      else
        return DoFindRow(drvs, estStatus, buildNum, strNum, strStatus, false);

    }

    private DataRow DoFindRow(DataRowView[] drvs, FiasEstateStatus estStatus, string buildNum, string strNum, FiasStructureStatus strStatus, bool exact)
    {
      DataRow row = null;

      for (int i = 0; i < drvs.Length; i++)
      {
        string b = DataTools.GetString(drvs[i].Row, "BUILDNUM");
        string s = DataTools.GetString(drvs[i].Row, "STRUCNUM");

        //if (b != buildNum || s != strNum)
        if (!String.Equals(b, buildNum, StringComparison.OrdinalIgnoreCase)) // 09.03.2021
          continue;
        if (!String.Equals(s, strNum, StringComparison.OrdinalIgnoreCase)) // 09.03.2021
          continue;

        if (exact)
        {
          FiasEstateStatus thisEstStatus = (FiasEstateStatus)DataTools.GetInt(drvs[i].Row, "ESTSTATUS");
          FiasStructureStatus thisStrStatus = (FiasStructureStatus)DataTools.GetInt(drvs[i].Row, "STRSTATUS");
          if (thisEstStatus != estStatus || thisStrStatus != strStatus)
            continue;
        }

        if (row == null)
          row = drvs[i].Row;
        else
          return null;
      }

      return row;
    }

    /// <summary>
    /// Получить текстовое представление здания для строки кэша
    /// </summary>
    /// <param name="row">Строка таблицы кэша или null</param>
    /// <returns>Текствое представление</returns>
    public static string GetText(DataRow row)
    {
      if (row == null)
        return String.Empty;

      string houseNum = DataTools.GetString(row, "HOUSENUM");
      FiasEstateStatus estStatus = (FiasEstateStatus)DataTools.GetInt(row, "ESTSTATUS");
      string buildNum = DataTools.GetString(row, "BUILDNUM");
      string strNum = DataTools.GetString(row, "STRUCNUM");
      FiasStructureStatus strStatus = (FiasStructureStatus)DataTools.GetInt(row, "STRSTATUS");
      return GetText(houseNum, estStatus, buildNum, strNum, strStatus);
    }

    /// <summary>
    /// Получить текстовое представление для здания
    /// </summary>
    /// <param name="houseNum">Номер дома</param>
    /// <param name="estStatus">Признак владения</param>
    /// <param name="buildNum">Номер корпуса</param>
    /// <param name="strNum">Номер строения</param>
    /// <param name="strStatus">Признак строения</param>
    /// <returns>Текстовое представление</returns>
    public static string GetText(string houseNum, FiasEstateStatus estStatus, string buildNum, string strNum, FiasStructureStatus strStatus)
    {
      StringBuilder sb = new StringBuilder();
      if (houseNum.Length > 0)
      {
        if (estStatus == FiasEstateStatus.Unknown)
        {
          sb.Append("[");
          sb.Append(FiasEnumNames.ToString(FiasLevel.House, false));
          sb.Append("]");
        }
        else
          sb.Append(FiasEnumNames.ToString(estStatus));
        sb.Append(" ");
        sb.Append(houseNum);
      }
      if (buildNum.Length > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("корпус ");
        sb.Append(buildNum);
      }
      if (strNum.Length > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        if (strStatus == FiasStructureStatus.Unknown)
        {
          sb.Append("[");
          sb.Append(FiasEnumNames.ToString(FiasLevel.Structure, false));
          sb.Append("]");
        }
        else
          sb.Append(FiasEnumNames.ToString(strStatus));
        sb.Append(" ");
        sb.Append(strNum);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Устойчивые идентификаторы зданий.
    /// В список входят только действующие элементы.
    /// </summary>
    public Guid[] HouseGuids
    {
      get
      {
        if (_HouseGuids != null)
          return _HouseGuids;

        lock (_DS)
        {
          if (_HouseGuids == null)
          {
            SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
            using (DataView dv = CreateDataView())
            {
              foreach (DataRowView drv in dv)
                lst.Add(DataTools.GetGuid(drv.Row, "HOUSEGUID"));
            }
            _HouseGuids = lst.ToArray();
          }
        }
        return _HouseGuids;
      }
    }
    [NonSerialized]
    private volatile Guid[] _HouseGuids;

    #endregion
  }

  /// <summary>
  /// Страница классификатора для помещений.
  /// Этот класс не используется в пользовательском коде.
  /// Класс является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class FiasCachedPageRoom
  {
    #region Защищенный конструктор

    internal FiasCachedPageRoom(Guid pageHouseGuid, DataSet ds)
    {
      _PageHouseGuid = pageHouseGuid;
      SerializationTools.PrepareDataSet(ds); // 19.01.2021
      _DS = ds;

      OnDeserializedMethod(new System.Runtime.Serialization.StreamingContext());
    }

    #endregion

    #region Поля

    /// <summary>
    /// GUID дома, к которому относится страница
    /// </summary>
    internal Guid PageHouseGuid { get { return _PageHouseGuid; } }
    private readonly Guid _PageHouseGuid;

    /// <summary>
    /// Внутренняя таблица данных
    /// </summary>
    private readonly DataSet _DS;

    /// <summary>
    /// Объект для поиска домов, корпусов, строений.
    /// Имеет Sort="HOUSENUM". Поиск по корпусам и строениям выполняется перебором DataViewRow
    /// При работе блокировка не нужна
    /// </summary>
    [NonSerialized]
    private DataView _dvFlatNumber;

    /// <summary>
    /// Возвращает true, если на странице нет ни одной записи
    /// </summary>
    public bool IsEmpty { get { return _DS.Tables[0].Rows.Count == 0; } }

    /// <summary>
    /// Количество строк в таблице на странице (для отладочных целей)
    /// </summary>
    public int RowCount { get { return _DS.Tables[0].Rows.Count; } }

    #endregion

    #region Сериализация

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserializedMethod(System.Runtime.Serialization.StreamingContext context)
    {
      DataTools.SetPrimaryKey(_DS.Tables[0], "ROOMID");
      _DS.Tables[0].DefaultView.Sort = "ROOMGUID";
      _DS.Tables[0].DefaultView.RowFilter = "TopFlag=TRUE";

      _dvFlatNumber = new DataView(_DS.Tables[0]);
      FiasTools.InitTopFlagAndDatesRowFilter(_dvFlatNumber);
      _dvFlatNumber.Sort = "FLATNUMBER";
    }

    #endregion

    #region Поиск

    internal DataRow FindRowByGuid(Guid guid)
    {
      int p = _DS.Tables[0].DefaultView.Find(guid);
      if (p >= 0)
        return _DS.Tables[0].DefaultView[p].Row;
      else
        return null;
    }

    internal DataRow FindRowByRecId(Guid recId)
    {
      return _DS.Tables[0].Rows.Find(recId);
    }

    #endregion

    #region Вспомогательные методы

    internal void AddToGuidDict(DictionaryWithMRU<FiasGuidKey, FiasGuidInfo> dict)
    {
      int pRecId = _DS.Tables[0].Columns.IndexOf("ROOMID");
      int pChild = _DS.Tables[0].Columns.IndexOf("ROOMGUID");
      FiasGuidInfo info = new FiasGuidInfo(FiasLevel.Flat, _PageHouseGuid);
      foreach (DataRow row in _DS.Tables[0].Rows)
      {
        FiasGuidKey key;
        key = new FiasGuidKey(DataTools.GetGuid(row[pChild]), FiasTableType.Room, false);
        dict[key] = info;
        key = new FiasGuidKey(DataTools.GetGuid(row[pRecId]), FiasTableType.Room, true);
        dict[key] = info;
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление объекта</returns>
    public override string ToString()
    {
      return _PageHouseGuid.ToString();
    }

    #endregion

    #region Методы и свойства для редактора

    /// <summary>
    /// Номера квартир (помещений) без сокращений для инициализации списка AutoComplete в поле ввода
    /// </summary>
    public string[] GetFlatNums()
    {
      if (_FlatNums != null)
        return _FlatNums;

      lock (_DS)
      {
        if (_FlatNums == null)
        {
          SingleScopeSortedList<string> lst = new SingleScopeSortedList<string>();
          foreach (DataRowView drv in _dvFlatNumber)
          {
            string h = DataTools.GetString(drv.Row, "FLATNUMBER");
            if (h.Length > 0)
              lst.Add(h);
          }
          _FlatNums = lst.ToArray();
        }
      }
      return _FlatNums;
    }
    [NonSerialized]
    private volatile string[] _FlatNums;

    // Для комнат данные не буферизуются

    /// <summary>
    /// Получить номера комнат для заданного номера квартиры (помещения)
    /// </summary>
    /// <param name="flatNum">Номер квартиры</param>
    /// <returns>Список комнат</returns>
    public string[] GetRoomNums(string flatNum)
    {
      if (flatNum == null)
        flatNum = String.Empty;

      DataRowView[] drvs = _dvFlatNumber.FindRows(flatNum);
      SingleScopeList<String> lst = new SingleScopeList<string>();
      for (int i = 0; i < drvs.Length; i++)
      {
        string r = DataTools.GetString(drvs[i].Row, "ROOMNUMBER");
        if (r.Length > 0)
          lst.Add(r);
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный объект должен быть удален после того, как просмотр закрывается.
    /// В просмотр входят только актуальные записи.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasRoomExtractor.
    /// </summary>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView()
    {
      return CreateDataView(true);
    }

    /// <summary>
    /// Создать объект DataView для просмотра.
    /// Список отсортирован.
    /// Созданный объект должен быть удален после того, как просмотр закрывается.
    /// Для извлечения информации из строк DataRow просмотра используйте FiasRoomExtractor.
    /// </summary>
    /// <param name="actualOnly">Если true, то в просмотр входят только актуальные записи.
    /// Если false, то фильтр не устанавливается. Однако наличие исторических записей зависит от FiasDBSettings.</param>
    /// <returns>Новый объект DataView</returns>
    public DataView CreateDataView(bool actualOnly)
    {
      DataView dv = new DataView(_DS.Tables[0]);
      if (actualOnly)
        FiasTools.InitTopFlagAndDatesRowFilter(dv);
      dv.Sort = "FLATTYPE,nFlatNumber,FLATNUMBER,ROOMTYPE,nRoomNumber,ROOMNUMBER"; // квартиры до помещений, затем по номерам квартир
      return dv;
    }

    /// <summary>
    /// Поиск строки.
    /// Если есть больше одной подходящей строки (неоднозначность), то возвращается null
    /// </summary>
    /// <param name="flatNum">Номер квартиры (помещения)</param>
    /// <param name="flatType">Тип квартиры</param>
    /// <param name="roomNum">Номер комнаты</param>
    /// <param name="roomType">Тип комнаты</param>
    /// <returns>Строка таблицы или null</returns>
    public DataRow FindRow(string flatNum, FiasFlatType flatType, string roomNum, FiasRoomType roomType)
    {
      if (flatNum == null)
        flatNum = String.Empty;
      if (roomNum == null)
        roomNum = String.Empty;
      if (String.IsNullOrEmpty(flatNum))
        flatType = FiasFlatType.Unknown;
      if (String.IsNullOrEmpty(roomNum))
        roomType = FiasRoomType.Unknown;

      if (String.IsNullOrEmpty(flatNum) && String.IsNullOrEmpty(roomNum))
        return null;

      DataRowView[] drvs;
      if (flatNum.Length == 0)
        drvs = _dvFlatNumber.FindRows(DBNull.Value);
      else
        drvs = _dvFlatNumber.FindRows(flatNum);
      DataRow row = DoFindRow(drvs, flatType, roomNum, roomType, true);
      if (row != null)
        return row; // точное соответствие
      else
        return DoFindRow(drvs, flatType, roomNum, roomType, false);
    }

    private DataRow DoFindRow(DataRowView[] drvs, FiasFlatType flatType, string roomNum, FiasRoomType roomType, bool exact)
    {
      DataRow row = null;
      for (int i = 0; i < drvs.Length; i++)
      {
        // if (DataTools.GetString(drvs[i].Row, "ROOMNUMBER") != roomNum)
        if (!String.Equals(DataTools.GetString(drvs[i].Row, "ROOMNUMBER"), roomNum, StringComparison.OrdinalIgnoreCase)) // 09.03.2021
          continue;

        if (exact)
        {
          FiasFlatType thisFlatType = (FiasFlatType)DataTools.GetInt(drvs[i].Row, "FLATTYPE");
          FiasRoomType thisRoomType = (FiasRoomType)DataTools.GetInt(drvs[i].Row, "ROOMTYPE");
          if (thisFlatType != flatType || thisRoomType != roomType)
            continue;
        }

        if (row == null)
          row = drvs[i].Row;
        else
          return null; // больше одной строки
      }

      return row;
    }

    /// <summary>
    /// Получить текстовое представление для квартиры/помещения/комнаты
    /// </summary>
    /// <param name="row">Строка таблицы кэша или null</param>
    /// <returns>Текстовое представление</returns>
    public static string GetText(DataRow row)
    {
      if (row == null)
        return String.Empty;

      string flatNum = DataTools.GetString(row, "FLATNUMBER");
      FiasFlatType flatType = (FiasFlatType)DataTools.GetInt(row, "FLATTYPE");
      string roomNum = DataTools.GetString(row, "ROOMNUMBER");
      FiasRoomType roomType = (FiasRoomType)DataTools.GetInt(row, "ROOMTYPE");
      return GetText(flatNum, flatType, roomNum, roomType);
    }

    /// <summary>
    /// Получить текстовое представление для квартиры/помещения/комнаты
    /// </summary>
    /// <param name="flatNum">Номер квартиры/помещения</param>
    /// <param name="flatType">Тип помещения</param>
    /// <param name="roomNum">Номер комнаты</param>
    /// <param name="roomType">Тип комнаты</param>
    /// <returns>Текстовое представление</returns>
    public static string GetText(string flatNum, FiasFlatType flatType, string roomNum, FiasRoomType roomType)
    {
      StringBuilder sb = new StringBuilder();
      if (flatNum.Length > 0)
      {
        if (flatType == FiasFlatType.Unknown)
        {
          sb.Append("[");
          sb.Append(FiasEnumNames.ToString(FiasLevel.Flat, false));
          sb.Append("]");
        }
        else
          sb.Append(FiasEnumNames.ToString(flatType));
        sb.Append(" ");
        sb.Append(flatNum);
      }
      if (roomNum.Length > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        if (roomType == FiasRoomType.Unknown)
        {
          sb.Append("[");
          sb.Append(FiasEnumNames.ToString(FiasLevel.Room, false));
          sb.Append("]");
        }
        else
          sb.Append(FiasEnumNames.ToString(roomType));
        sb.Append(roomNum);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Устойчивые идентификаторы помещений.
    /// В список входят только действующие элементы.
    /// </summary>
    public Guid[] RoomGuids
    {
      get
      {
        if (_RoomGuids != null)
          return _RoomGuids;

        lock (_DS)
        {
          if (_RoomGuids == null)
          {
            SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
            using (DataView dv = CreateDataView())
            {
              foreach (DataRowView drv in dv)
                lst.Add(DataTools.GetGuid(drv.Row, "ROOMGUID"));
            }
            _RoomGuids = lst.ToArray();
          }
        }
        return _RoomGuids;
      }
    }
    [NonSerialized]
    private volatile Guid[] _RoomGuids;

    #endregion
  }

  /// <summary>
  /// Извлечение данных из строки классификатора для таблицы AddrOb.
  /// Строки должны быть получены из просмотра, создаваемым FiasCachedPageAddrOb.CreateDataView()
  /// После установки свойства Row становятся доступными свойства для извлечения информации.
  /// </summary>
  public struct FiasAddrObExtractor
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект.
    /// </summary>
    /// <param name="source">Источник данных. Не может быть null</param>
    public FiasAddrObExtractor(IFiasSource source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      _Source = source;
      _Row = null;
    }

    #endregion

    #region Свойства объекта

    private readonly IFiasSource _Source;

    /// <summary>
    /// Сюда должна быть помещена строка из просмотра страницы FiasCachedPageAddrOb.CreateDataView().
    /// После этого можно обращаться к другим свойствам.
    /// </summary>
    public DataRow Row { get { return _Row; } set { _Row = value; } }
    private DataRow _Row;

    #endregion

    #region Свойства адреса

    /// <summary>
    /// Идентификатор адресного объекта
    /// </summary>
    public Guid AOGuid { get { return DataTools.GetGuid(_Row, "AOGUID"); } }

    /// <summary>
    /// Текстовое наименование без сокращения
    /// </summary>
    public string Name { get { return DataTools.GetString(_Row, "OFFNAME"); } }

    /// <summary>
    /// Идентификатор типа адресообразующего элемента.
    /// Для получения типа в читаемом виде используйте метод FiasCachedAOTypes.GetAOType()
    /// </summary>
    public Int32 AOTypeId { get { return DataTools.GetInt(_Row, "AOTypeId"); } }

    /// <summary>
    /// Уровень адресного объекта
    /// </summary>
    public FiasLevel Level { get { return (FiasLevel)DataTools.GetInt(_Row, "AOLEVEL"); } }

    /// <summary>
    /// Почтовый индекс.
    /// Если не определен, возвращает пустую строку
    /// </summary>
    public string PostalCode
    {
      get
      {
        if (DataTools.GetInt(_Row, "POSTALCODE") == 0)
          return String.Empty;
        else
          return DataTools.GetInt(_Row, "POSTALCODE").ToString("000000");
      }
    }

    /// <summary>
    /// Код региона ("01"-"99")
    /// </summary>
    public string RegionCode
    {
      get
      {
        if (DataTools.GetInt(_Row, "REGIONCODE") == 0)
          return String.Empty;
        else
          return DataTools.GetInt(_Row, "REGIONCODE").ToString("00");
      }
    }


    /// <summary>
    /// Код ОКАТО.
    /// Если FiasDBSettings.UseOKATO=false, то возвращается пустая строка.
    /// </summary>
    public string OKATO
    {
      get
      {
        if (_Source.DBSettings.UseOKATO)
        {
          long v = DataTools.GetInt64(_Row, "OKATO");
          if (v == 0L)
            return String.Empty;
          else
            return v.ToString("00000000000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ОКТМО (8 или 11 знаков).
    /// Если FiasDBSettings.UseOKTMO=false, то возвращается пустая строка.
    /// </summary>
    public string OKTMO
    {
      get
      {
        if (_Source.DBSettings.UseOKTMO)
        {
          long v = DataTools.GetInt64(_Row, "OKTMO");
          if (v == 0L)
            return String.Empty;
          else if (v <= 99999999L)
            return v.ToString("00000000");
          else
            return v.ToString("00000000000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ФЛ.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    public string IFNSFL
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSFL") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSFL").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ФЛ территории.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    internal string IFNSFLTerr
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSFLTerr") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSFLTerr").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ЮЛ.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    public string IFNSUL
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSUL") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSUL").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ЮЛ территории.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    internal string IFNSULTerr
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSULTERR") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSULTERR").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Признак актуальности записи.
    /// Если FiasDBSettings.UseHistory=false, всегда возвращает true.
    /// </summary>
    public bool Actual
    {
      get
      {
        if (_Source.DBSettings.UseHistory)
          return DataTools.GetBool(_Row, "Actual");
        else
          return true;
      }
    }


    /// <summary>
    /// Признак действующей записи.
    /// Если FiasDBSettings.UseHistory=false, всегда возвращает true.
    /// </summary>
    public bool Live
    {
      get
      {
        if (_Source.DBSettings.UseHistory)
          return DataTools.GetBool(_Row, "Live");
        else
          return true;
      }
    }


    #endregion

    #region Свойства записи

    /// <summary>
    /// Идентификатор записи
    /// </summary>
    internal Guid RecId { get { return DataTools.GetGuid(_Row, "AOID"); } }

    /// <summary>
    /// Начало действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null.
    /// </summary>
    public DateTime? StartDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, true); } }

    /// <summary>
    /// Окончание действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null.
    /// </summary>
    public DateTime? EndDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, false); } }

    #endregion
  }

  /// <summary>
  /// Извлечение данных из строки классификатора для таблицы House.
  /// Строки должны быть получены из просмотра, создаваемым FiasCachedPageHouse.CreateDataView()
  /// После установки свойства Row становятся доступными свойства для извлечения информации.
  /// </summary>
  public struct FiasHouseExtractor
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект.
    /// </summary>
    /// <param name="source">Источник данных. Не может быть null</param>
    public FiasHouseExtractor(IFiasSource source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      _Source = source;
      _Row = null;
    }

    #endregion

    #region Свойства объекта

    private readonly IFiasSource _Source;

    /// <summary>
    /// Сюда должна быть помещена строка из просмотра страницы FiasCachedPageHouse.CreateDataView().
    /// После этого можно обращаться к другим свойствам.
    /// </summary>
    public DataRow Row { get { return _Row; } set { _Row = value; } }
    private DataRow _Row;

    #endregion

    #region Свойства адреса

    /// <summary>
    /// Идентификатор здания
    /// </summary>
    public Guid HouseGuid { get { return DataTools.GetGuid(_Row, "HOUSEGUID"); } }

    /// <summary>
    /// Номер дома
    /// </summary>
    public string HouseNum { get { return DataTools.GetString(_Row, "HOUSENUM"); } }

    /// <summary>
    /// Признак владения
    /// </summary>
    public FiasEstateStatus EstStatus { get { return (FiasEstateStatus)DataTools.GetInt(_Row, "ESTSTATUS"); } }

    /// <summary>
    /// Номер корпуса
    /// </summary>
    public string BuildNum { get { return DataTools.GetString(_Row, "BUILDNUM"); } }

    /// <summary>
    /// Номер строения
    /// </summary>
    public string StrucNum { get { return DataTools.GetString(_Row, "STRUCNUM"); } }

    /// <summary>
    /// Тип строения
    /// </summary>
    public FiasStructureStatus StrStatus { get { return (FiasStructureStatus)DataTools.GetInt(_Row, "STRSTATUS"); } }

    /// <summary>
    /// Почтовый индекс.
    /// Если не определен, возвращает пустую строку
    /// </summary>
    public string PostalCode
    {
      get
      {
        if (DataTools.GetInt(_Row, "POSTALCODE") == 0)
          return String.Empty;
        else
          return DataTools.GetInt(_Row, "POSTALCODE").ToString("000000");
      }
    }

    /// <summary>
    /// Код ОКАТО.
    /// Если FiasDBSettings.UseOKATO=false, то возвращается пустая строка.
    /// </summary>
    public string OKATO
    {
      get
      {
        if (_Source.DBSettings.UseOKATO)
        {
          long v = DataTools.GetInt64(_Row, "OKATO");
          if (v == 0L)
            return String.Empty;
          else
            return v.ToString("00000000000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ОКТМО (8 или 11 знаков).
    /// Если FiasDBSettings.UseOKTMO=false, то возвращается пустая строка.
    /// </summary>
    public string OKTMO
    {
      get
      {
        if (_Source.DBSettings.UseOKTMO)
        {
          long v = DataTools.GetInt64(_Row, "OKTMO");
          if (v == 0L)
            return String.Empty;
          else if (v <= 99999999L)
            return v.ToString("00000000");
          else
            return v.ToString("00000000000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ФЛ.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    public string IFNSFL
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSFL") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSFL").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ФЛ территории.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    internal string IFNSFLTerr
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSFLTerr") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSFLTerr").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ЮЛ.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    public string IFNSUL
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSUL") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSUL").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Код ИФНС ЮЛ территории.
    /// Если FiasDBSettings.UseIFNS=false, то возвращается пустая строка.
    /// </summary>
    internal string IFNSULTerr
    {
      get
      {
        if (_Source.DBSettings.UseIFNS)
        {
          if (DataTools.GetInt(_Row, "IFNSULTERR") == 0)
            return String.Empty;
          else
            return DataTools.GetInt(_Row, "IFNSULTERR").ToString("0000");
        }
        else
          return String.Empty;
      }
    }

    #endregion

    #region Свойства записи

    /// <summary>
    /// Идентификатор записи
    /// </summary>
    internal Guid RecId { get { return DataTools.GetGuid(_Row, "HOUSEID"); } }

    /// <summary>
    /// Начало действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null
    /// </summary>
    public DateTime? StartDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, true); } }

    /// <summary>
    /// Окончание действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null
    /// </summary>
    public DateTime? EndDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, false); } }

    #endregion
  }

  /// <summary>
  /// Извлечение данных из строки классификатора для таблицы Room.
  /// Строки должны быть получены из просмотра, создаваемым FiasCachedPageRoom.CreateDataView()
  /// После установки свойства Row становятся доступными свойства для извлечения информации.
  /// </summary>
  public struct FiasRoomExtractor
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект.
    /// </summary>
    /// <param name="source">Источник данных. Не может быть null</param>
    public FiasRoomExtractor(IFiasSource source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      _Source = source;
      _Row = null;
    }

    #endregion

    #region Свойства объекта

    private readonly IFiasSource _Source;

    /// <summary>
    /// Сюда должна быть помещена строка из просмотра страницы FiasCachedPageRoom.CreateDataView().
    /// После этого можно обращаться к другим свойствам.
    /// </summary>
    public DataRow Row { get { return _Row; } set { _Row = value; } }
    private DataRow _Row;

    #endregion

    #region Свойства адреса

    /// <summary>
    /// Идентификатор помещения
    /// </summary>
    public Guid RoomGuid { get { return DataTools.GetGuid(_Row, "ROOMGUID"); } }

    /// <summary>
    /// Номер квартиры, офиса
    /// </summary>
    public string FlatNumber { get { return DataTools.GetString(_Row, "FLATNUMBER"); } }

    /// <summary>
    /// Тип помещения (квартира, офис, ...)
    /// </summary>
    public FiasFlatType FlatType { get { return (FiasFlatType)DataTools.GetInt(_Row, "FLATTYPE"); } }

    /// <summary>
    /// Номер комнаты
    /// </summary>
    public string RoomNumber { get { return DataTools.GetString(_Row, "ROOMNUMBER"); } }

    /// <summary>
    /// Тип комнаты
    /// </summary>
    public FiasRoomType RoomType { get { return (FiasRoomType)DataTools.GetInt(_Row, "ROOMTYPE"); } }

    /// <summary>
    /// Почтовый индекс.
    /// Если не определен, возвращает пустую строку
    /// </summary>
    public string PostalCode
    {
      get
      {
        if (DataTools.GetInt(_Row, "POSTALCODE") == 0)
          return String.Empty;
        else
          return DataTools.GetInt(_Row, "POSTALCODE").ToString("000000");
      }
    }

    /// <summary>
    /// Признак действующей записи.
    /// Если FiasDBSettings.UseHistory=false, всегда возвращает true.
    /// </summary>
    public bool Live
    {
      get
      {
        if (_Source.DBSettings.UseHistory)
          return DataTools.GetBool(_Row, "Live");
        else
          return true;
      }
    }

    #endregion

    #region Свойства записи

    /// <summary>
    /// Идентификатор записи
    /// </summary>
    internal Guid RecId { get { return DataTools.GetGuid(_Row, "ROOMID"); } }

    /// <summary>
    /// Начало действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null
    /// </summary>
    public DateTime? StartDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, true); } }

    /// <summary>
    /// Окончание действия записи.
    /// Если FiasDBSetting.UseDates=false, возвращает null
    /// </summary>
    public DateTime? EndDate { get { return FiasTools.GetStartOrEndDate(_Source, _Row, false); } }

    #endregion
  }
}
