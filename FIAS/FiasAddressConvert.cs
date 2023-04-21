// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Parsing;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  #region Перечисление FiasAddressConvertGuidMode

  /// <summary>
  /// Возможные значения для свойства
  /// </summary>
  public enum FiasAddressConvertGuidMode
  {
    /// <summary>
    /// GUIDы не записываются. Записывается текст для всех уровней
    /// </summary>
    Text = 0x20,

    /// <summary>
    /// Записывается AOGUID. Здание и помещение записываются как текст
    /// </summary>
    AOGuid = 0x1,

    /// <summary>
    /// Записывается HOUSEGUID. Помещение записывается как текст
    /// </summary>
    HouseGuid = 0x2,

    /// <summary>
    /// Записывается ROOMGUID. Здание и помещение записываются как текст
    /// </summary>
    RoomGuid = 0x4,

    /// <summary>
    /// Записывается AOGUID и текст для всех уровней
    /// </summary>
    AOGuidWithText = 0x21,

    /// <summary>
    /// Записывается HOUSEGUID и текст для всех уровней
    /// </summary>
    HouseGuidWithText = 0x22,

    /// <summary>
    /// Записывается ROOMGUID и текст для всех уровней
    /// </summary>
    RoomGuidWithText = 0x24,

    /// <summary>
    /// Записывается не кодированная строка, а "голый" идентификатор адресного объекта, здания или помещения.
    /// Если это невозможно, например, здание выбрано не из справочника, выбрасывается исключение.
    /// Также выбрасывается исключение, если в адресе задан почтовый индекс.
    /// Обычно не следует использовать этот режим.
    /// </summary>
    GuidOnly = 0x40,

    /// <summary>
    /// Если возможно, то записывается "голый" идентификатор адресного объекта, здания или помещения.
    /// Иначе записывается кодированная строка.
    /// </summary>
    GuidPreferred = 0x47,
  }

  #endregion

  /// <summary>
  /// Преобразование адреса в строку для хранения в базе данных или файлах.
  /// Строка состоит из пар код-значение и имеет формат: N1="v1",N2="v2"
  /// Использование класса:
  /// 1. Создать экземпляр FiasAddressConvert.
  /// 2. Установить свойства.
  /// 3. Вызвать ToString() для записи или Parse()/TryParse() для чтения.
  /// 
  /// Этот класс не является потокобезопасным в части установки свойств. Вызовы методов являются потокобезопасными.
  /// </summary>
  public sealed class FiasAddressConvert : ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает конвертер
    /// </summary>
    /// <param name="source">Источник данных ФИАС. Не может быть null.</param>
    public FiasAddressConvert(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _GuidMode = FiasAddressConvertGuidMode.AOGuid;
      _FillAddress = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к классификатору.
    /// Задается в конструкторе
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private readonly IFiasSource _Source;

    // Нельзя хранить объект FiasHandler как поле, т.к. он не является потокобезопасным

    /// <summary>
    /// Способ записи адреса.
    /// По умолчанию используется режим AOGuid.
    /// </summary>
    public FiasAddressConvertGuidMode GuidMode
    {
      get { return _GuidMode; }
      set { _GuidMode = value; }
    }
    private FiasAddressConvertGuidMode _GuidMode;

    /// <summary>
    /// Нужно ли вызывать метод FiasHandler.FillAddress()?
    /// По умолчанию - true.
    /// 
    /// При вызове Parse()/TryParse(), если свойство равно false, в FiasAddress() будут заполнены только те поля,
    /// которые были в кодированной строке. Например, могут оказаться заполненными только AOGuid и номер дома.
    /// Предполагается, что вызывающий код вызовет FiasHandler.FillAddress() перед получением компонентов адреса.
    /// 
    /// При вызове ToString(), если свойство равно false, предполагается, что в адресе уже заполнены все компоненты,
    /// необходимые для создания кодированной строки. Например, для адреса уже был вызван метод FillAddress().
    /// Также, в некоторых случаях можно обойтись без вызова FillAddress(), если в FiasAddress гарантировано установлены
    /// все необходимые компоненты адреса.
    /// </summary>
    public bool FillAddress { get { return _FillAddress; } set { _FillAddress = value; } }
    private bool _FillAddress;

    /// <summary>
    /// Выводит GuidMode для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return GuidMode.ToString();
    }

    #endregion

    #region Словари

    static FiasAddressConvert()
    {
      _NameDict = new BidirectionalDictionary<FiasLevel, string>();
      _NameDict.Add(FiasLevel.Region, "REGION");
      _NameDict.Add(FiasLevel.District, "DISTRICT");
      _NameDict.Add(FiasLevel.City, "CITY");
      _NameDict.Add(FiasLevel.Village, "VILLAGE");
      _NameDict.Add(FiasLevel.PlanningStructure, "PLANSTR");
      _NameDict.Add(FiasLevel.Street, "STREET");
      _NameDict.Add(FiasLevel.House, "HOUSE");
      _NameDict.Add(FiasLevel.Building, "BUILDING");
      _NameDict.Add(FiasLevel.Structure, "STRUCTURE");
      _NameDict.Add(FiasLevel.Flat, "FLAT");
      _NameDict.Add(FiasLevel.Room, "ROOM");
    }

    private static readonly BidirectionalDictionary<FiasLevel, string> _NameDict;

    /// <summary>
    /// Возвращает список доступных режимов для создания строки.
    /// Режим GuidOnly не возвращается, так как он может приводить к исключениям при преобразовании адреса
    /// </summary>
    public FiasAddressConvertGuidMode[] AvailableGuidModes
    {
      get
      {
        List<FiasAddressConvertGuidMode> lst = new List<FiasAddressConvertGuidMode>();
        lst.Add(FiasAddressConvertGuidMode.Text);
        lst.Add(FiasAddressConvertGuidMode.AOGuid);
        lst.Add(FiasAddressConvertGuidMode.AOGuidWithText);
        if (_Source.DBSettings.UseHouse)
        {
          lst.Add(FiasAddressConvertGuidMode.HouseGuid);
          lst.Add(FiasAddressConvertGuidMode.HouseGuidWithText);
        }
        if (_Source.DBSettings.UseRoom)
        {
          lst.Add(FiasAddressConvertGuidMode.RoomGuid);
          lst.Add(FiasAddressConvertGuidMode.RoomGuidWithText);
        }
        lst.Add(FiasAddressConvertGuidMode.GuidPreferred);
        return lst.ToArray();
      }
    }

    #endregion

    #region Создание строки

    /// <summary>
    /// Преобразует адрес в текстовую строку.
    /// Наличие полей определяется свойством GuidMode.
    /// Предполагается, что адрес уже заполнен полностью, то есть был вызов метода FiasHandler.FillAddress().
    /// 
    /// Если свойство FillAddress=true, то будет создана копия адреса и вызван метод FiasHandler.FillAddress().
    /// Этот метод является потокобезопасным.
    /// </summary>
    /// <param name="address">Адрес, который нужно преобразовать. Не может быть null</param>
    /// <returns>Кодированная строка адреса</returns>
    public string ToString(FiasAddress address)
    {
#if DEBUG
      if (address == null)
        throw new ArgumentNullException("address");
#endif

      if (address.IsEmpty)
        return String.Empty;

      if (FillAddress)
      {
        address = address.Clone();
        new FiasHandler(_Source).FillAddress(address);
      }

      if (GuidMode == FiasAddressConvertGuidMode.GuidOnly)
      {
        if (!address.AutoPostalCode)
          throw new InvalidCastException("Нельзя преобразовать адрес в GUID, так как задан почтовый индекс");
        if (address.GuidBottomLevel != FiasTools.ReplaceToHouseOrFlat(address.NameBottomLevel))
          throw new InvalidCastException("Нельзя преобразовать адрес в GUID, так как некоторые элементы выбраны не из справочника. Задан текст для уровня \"" +
            FiasEnumNames.ToString(address.NameBottomLevel, true) + "\", в то время, как из справочника выбрано только до уровня \"" + FiasEnumNames.ToString(address.GuidBottomLevel, true) + "\"");

        return address.AnyGuid.ToString();
      }

      FiasAddressConvertGuidMode thisGuidMode = _GuidMode & (FiasAddressConvertGuidMode.AOGuid | FiasAddressConvertGuidMode.HouseGuid | FiasAddressConvertGuidMode.RoomGuid);

      if (GuidMode == FiasAddressConvertGuidMode.GuidPreferred)
      {
        if (address.AutoPostalCode &&
          address.GuidBottomLevel == FiasTools.ReplaceToHouseOrFlat(address.NameBottomLevel))
        {
          return address.AnyGuid.ToString();
        }
        else
          thisGuidMode = FiasAddressConvertGuidMode.RoomGuid;
      }

      List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();

      if (!address.AutoPostalCode)
        lst.Add(new KeyValuePair<string, string>("POSTALCODE", address.PostalCode));

      // С какого уровня записывается текст. Unknown - не писать, Region - писать все
      FiasLevel firstTextLevel = FiasLevel.Unknown;
      if (thisGuidMode == FiasAddressConvertGuidMode.RoomGuid && address.GetGuid(FiasLevel.Flat) == Guid.Empty)
        thisGuidMode = FiasAddressConvertGuidMode.HouseGuid;
      if (thisGuidMode == FiasAddressConvertGuidMode.HouseGuid && address.GetGuid(FiasLevel.House) == Guid.Empty)
        thisGuidMode = FiasAddressConvertGuidMode.AOGuid;

      if (thisGuidMode == FiasAddressConvertGuidMode.AOGuid && address.AOGuid != Guid.Empty)
      {
        lst.Add(new KeyValuePair<string, string>("AOGUID", address.AOGuid.ToString()));
        firstTextLevel = FiasLevel.House;
        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          if (address.GetName(FiasTools.AOLevels[i]).Length > 0 && address.GetGuid(FiasTools.AOLevels[i]) == Guid.Empty)
          {
            firstTextLevel = FiasTools.AOLevels[i];
            break;
          }
        }
      }
      else if (thisGuidMode == FiasAddressConvertGuidMode.HouseGuid /*&& address.GetGuid(FiasLevel.House) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("HOUSEGUID", address.GetGuid(FiasLevel.House).ToString()));
        firstTextLevel = FiasLevel.Flat;
      }
      else if (thisGuidMode == FiasAddressConvertGuidMode.RoomGuid /*&& address.GetGuid(FiasLevel.Room) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("ROOMGUID", address.GetGuid(FiasLevel.Flat).ToString()));
        firstTextLevel = FiasLevel.Unknown;
      }
      else
        firstTextLevel = FiasLevel.Region;

      if ((_GuidMode & FiasAddressConvertGuidMode.Text) != 0)
        firstTextLevel = FiasLevel.Region; // принудительная запись всех уровней

      if (firstTextLevel != FiasLevel.Unknown)
      {
        for (int i = FiasTools.AllLevelIndexer.IndexOf(firstTextLevel); i < FiasTools.AllLevels.Length; i++)
          AddNameAndTypePairs(lst, address, FiasTools.AllLevels[i]);
      }
      return StringKeyValueParser.ToString(lst.ToArray());
    }

    private static void AddNameAndTypePairs(List<KeyValuePair<string, string>> lst, FiasAddress address, FiasLevel level)
    {
      string n = address.GetName(level);
      if (n.Length == 0)
        return;

      lst.Add(new KeyValuePair<string, string>(_NameDict[level], n));
      string a = address.GetAOType(level);
      if (a != FiasTools.GetDefaultAOType(level))
        lst.Add(new KeyValuePair<string, string>(_NameDict[level] + ".TYPE", a));
    }

    /// <summary>
    /// Групповое преобразование адресов.
    /// При FillHandler=true метод обладает повышенным быстродействием, по сравнению с вызовом ToString() в цикле
    /// </summary>
    /// <param name="addresses">Массив преобразуемых адресов</param>
    /// <returns>Массив кодированных строк</returns>
    public string[] ToStringArray(FiasAddress[] addresses)
    {
#if DEBUG
      if (addresses == null)
        throw new ArgumentNullException("addresses");
#endif
      if (addresses.Length == 0)
        return DataTools.EmptyStrings;

      string[] strs = new string[addresses.Length];
      if (FillAddress)
      {
        FiasAddress[] a2 = new FiasAddress[addresses.Length];
        for (int i = 0; i < a2.Length; i++)
          a2[i] = addresses[i].Clone();
        new FiasHandler(_Source).FillAddresses(a2);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < a2.Length; i++)
          strs[i] = conv2.ToString(a2[i]);
      }
      else
      {
        // Простой вызов в цикле
        for (int i = 0; i < addresses.Length; i++)
          strs[i] = ToString(addresses[i]);
      }
      return strs;
    }

    #endregion

    #region Чтение строки

    /// <summary>
    /// Преобразование из строки в адрес.
    /// При наличии ошибок в строке выбрасывается исключение InvalidCastException.
    /// Если полученный адрес содержит ошибки, то они записываются в список FiasAddress.Messages, а не вызывают исключение.
    /// Альтернативно, строка может содержать единственный GUID вместо пар "код-значение". В этом случае он записывается в FiasAddress.UnknownGuid.
    /// Для пустой строки возвращается пустой FiasAddress.
    /// Свойство GuidMode не используется.
    /// Этот метод является потокобезопасным.
    /// </summary>
    /// <param name="s">Строка с кодами</param>
    /// <returns>Адрес</returns>
    public FiasAddress Parse(string s)
    {
      FiasAddress address;
      if (TryParse(s, out address))
        return address;
      else
        throw new InvalidCastException("Строку \"" + s + "\" нельзя преобразовать в адрес");
    }

    /// <summary>
    /// Преобразование из строки в адрес.
    /// При наличии ошибок в строке, исключение не выбрасывается.
    /// Если полученный адрес содержит ошибки, то они записываются в список FiasAddress.Messages, при этом возвращается true.
    /// Альтернативно, строка может содержать единственный GUID вместо пар "код-значение". В этом случае он записывается в FiasAddress.UnknownGuid.
    /// Для пустой строки возвращается пустой FiasAddress.
    /// Свойство GuidMode не используется.
    /// 
    /// Если свойство FillAddress=true, то будет выполнено заполнение всех компонентов адреса.
    /// Если FillAddress=false, то должен быть отдельно выполнен вызов FiasHandler.FillAddress(), если адрес не содержит ошибок.
    /// Обычно используется при массовом преобразовании адресов. Адреса добавляются в список, а затем вызывается метод FiasHandler.FillAddresses() 
    /// для быстрого заполненения адресов.
    /// 
    /// Этот метод является потокобезопасным.
    /// </summary>
    /// <param name="s">Строка с кодами</param>
    /// <param name="address">Сюда помещается адрес.
    /// В случае ошибки при парсинге строки, создается пустой объект (FiasAddress.IsEmpty=true)</param>
    /// <returns>Результат парсинга</returns>
    public bool TryParse(string s, out FiasAddress address)
    {
      address = new FiasAddress();
      if (String.IsNullOrEmpty(s))
        return true;

      Guid g;
      if (FiasTools.TryParseGuid(s, out g))
      {
        // 21.08.2020
        address.UnknownGuid = g;
        if (FillAddress)
          new FiasHandler(_Source).FillAddress(address);
        return true;
      }

      KeyValuePair<string, string>[] pairs;
      try
      {
        pairs = StringKeyValueParser.Parse(s);
      }
      catch (Exception e)
      {
        address.AddMessage(ErrorMessageKind.Error, "Ошибка парсинга строки адреса. " + e.Message);
        return false;
      }

      try
      {
        // 30.07.2020
        // Нужно добавлять недостающие "автоматические" типы объектов, например, "квартира"
        SingleScopeList<FiasLevel> missedAOTypeLevels = null; // создадим, когда понадобится. 

        foreach (KeyValuePair<string, string> pair in pairs)
        {
          try
          {
            switch (pair.Key)
            {
              case "AOGUID":
                address.AOGuid = new Guid(pair.Value);
                break;
              case "HOUSEGUID":
                address.SetGuid(FiasLevel.House, new Guid(pair.Value));
                break;
              case "ROOMGUID":
                address.SetGuid(FiasLevel.Flat, new Guid(pair.Value));
                break;
              case "POSTALCODE":
                address.AutoPostalCode = false;
                address.PostalCode = pair.Value;
                break;
              default:
                bool isAOType = false;
                string key = pair.Key;
                if (key.EndsWith(".TYPE", StringComparison.Ordinal))
                {
                  key = key.Substring(0, key.Length - 5); // без ".TYPE"
                  isAOType = true;
                }

                FiasLevel level;
                if (!_NameDict.TryGetKey(key, out level))
                {
                  address.AddMessage(ErrorMessageKind.Error, "Неизвестный тег \"" + pair.Key + "\" в строке адреса");
                  return false; // Неизвестное имя
                }

                if (missedAOTypeLevels == null)
                  missedAOTypeLevels = new SingleScopeList<FiasLevel>();

                if (isAOType)
                {
                  address.SetAOType(level, pair.Value);
                  missedAOTypeLevels.Remove(level);
                }
                else
                {
                  address.SetName(level, pair.Value);
                  missedAOTypeLevels.Add(level);
                }
                break;
            }
          }
          catch (Exception e)
          {
            address.AddMessage(ErrorMessageKind.Error, "Ошибка обработки тега \"" + pair.Key + "\" строки адреса. " + e.Message);
          }
        } // цикл по парам

        // 30.07.2020
        // Добавляем автоматические сокращения
        if (missedAOTypeLevels != null)
        {
          for (int i = 0; i < missedAOTypeLevels.Count; i++)
          {
            string aoType = FiasTools.GetDefaultAOType(missedAOTypeLevels[i]);
            if (aoType.Length > 0)
              address.SetAOType(missedAOTypeLevels[i], aoType);
          }
        }

        if (address.Messages.Count == 0 && FillAddress)
          new FiasHandler(_Source).FillAddress(address);
      }
      catch (Exception e)
      {
        address.AddMessage(ErrorMessageKind.Error, "Ошибка обработки компонентов строки адреса. " + e.Message);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Групповое выполнение парсинга строк.
    /// При FillHandler=true метод обладает повышенным быстродействием, по сравнению с вызовом TryParse() в цикле
    /// </summary>
    /// <param name="strs">Массив кодированных строк. Не может быть null</param>
    /// <param name="addresses">Сюда помещаются адреса.
    /// Если какая-либо из строк имеет неправильный формат, то помещается пустой объект FiasAddress с заполненными сообщениями.
    /// Пустым строкам соответствуют пустые объекты FiasAddress.</param>
    /// <returns>true, если все строки имели правильный формат</returns>
    public bool TryParseArray(string[] strs, out FiasAddress[] addresses)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      addresses = new FiasAddress[strs.Length];
      if (strs.Length == 0)
        return true;

      FiasAddress addr;
      bool res = true;
      if (FillAddress)
      {
        // Адреса, для которых нужно будет вызвать FiasHandler=FillAddresses().
        // Оптимистически создаем список с начальной емкостью для всех элементов.
        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            res = false;
          addresses[i] = addr;
        }

        // Заполняем адреса
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }
      }
      else
      {
        // Простой вызов цикла
        for (int i = 0; i < strs.Length; i++)
        {
          if (!TryParse(strs[i], out addr))
            res = false;
          addresses[i] = addr;
        }
      }
      return res;
    }

    /// <summary>
    /// Групповое выполнение парсинга строк.
    /// Если хотя бы одна строка имеет неправильный формат, выбрасывается исключение
    /// </summary>
    /// <param name="strs">Массив преобразуемых строк</param>
    /// <returns>Массив адресов</returns>
    public FiasAddress[] ParseArray(string[] strs)
    {
      // Можно было бы вызвать TryParseArray(), проверить результат и выбросить исключения.
      // Но тогда, если одна из строк неправильная, оставшиеся строки преобразовывались бы напрасно.

#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      FiasAddress[] addresses = new FiasAddress[strs.Length];
      if (strs.Length == 0)
        return addresses;

      FiasAddress addr;
      if (FillAddress)
      {
        // Адреса, для которых нужно будет вызвать FiasHandler=FillAddresses().
        // Оптимистически создаем список с начльной емкостью для всех элементов.
        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            throw new InvalidCastException("Строку \"" + strs[i] + "\" нельзя преобразовать в адрес");

          addresses[i] = addr;
        }

        // Заполняем адреса
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }
      }
      else
      {
        // Простой вызов цикла
        for (int i = 0; i < strs.Length; i++)
          addresses[i] = Parse(strs[i]);
      }

      return addresses;
    }

    #endregion

    #region Преобразование строки

    /// <summary>
    /// Комбинация вызовов Parse() и ToString() для кодированной строки адреса.
    /// При этом возвращается строка в формате GuidMode.
    /// Если исходная строка имеет неправильный формат, выбрасывается исключение.
    /// 
    /// Предполагается, что свойство FillAddress установлено.
    /// Иначе корректный результат можно получить только для немногих преобразований GuidMode, например из AOGuidWithText в Text.
    /// </summary>
    /// <param name="s">Исходная кодированная строка</param>
    /// <returns>Кодировананная строка в требуемом формате</returns>
    public string ConvertString(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (FillAddress)
      {
        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        FiasAddress addr = conv2.Parse(s);
        s = this.ToString(addr); // вызван FillAddress()
      }
      else
        s = ToString(Parse(s));

      return s;
    }

    /// <summary>
    /// Комбинация вызовов TryParse() и ToString() для кодированной строки адреса.
    /// При этом возвращается строка в формате GuidMode.
    /// Если исходная строка имеет неправильный формат, возвращается false.
    /// 
    /// Предполагается, что свойство FillAddress установлено.
    /// Иначе корректный результат можно получить только для немногих преобразований GuidMode, например из AOGuidWithText в Text.
    /// </summary>
    /// <param name="s">Кодированная строка (вход и выход)</param>
    /// <returns>Результат вызова TryParse</returns>
    public bool TryConvertString(ref string s)
    {
      if (String.IsNullOrEmpty(s))
        return true;

      FiasAddress addr;
      bool res;
      if (FillAddress)
      {
        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        res = conv2.TryParse(s, out addr);

        if (res)
          s = this.ToString(addr); // вызван FillAddress()
      }
      else
      {
        res = TryParse(s, out addr);
        if (res)
          s = ToString(addr);
      }

      return res;
    }

    /// <summary>
    /// Комбинация вызовов ParseArray() и ToStringArray() для кодированных строк адреса.
    /// При этом возвращаются строки в формате GuidMode.
    /// Если какая-либо из исходных строк имеет неправильный формат, выбрасывается исключение.
    /// 
    /// Предполагается, что свойство FillAddress установлено.
    /// Иначе корректный результат можно получить только для немногих преобразований GuidMode, например из AOGuidWithText в Text.
    /// </summary>
    /// <param name="strs">Исходные кодированные строки. Этот массив не меняется в процессе работы, в том числе и при возникновении ошибки</param>
    /// <returns>Кодировананные строки в требуемом формате</returns>
    public string[] ConvertStringArray(string[] strs)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      if (strs.Length == 0)
        return DataTools.EmptyStrings;

      if (FillAddress)
      {
        // Не выйдет обойтись без промежуточного массива адресов.
        // Если конвертировать адреса по одному, не будет группового вызова FiasHandler.FillAddresses().
        // Выигрыш в размере памяти не компенсирует падения быстродействия

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        FiasAddress[] addrs = conv2.ParseArray(strs);
        string[] a2 = this.ToStringArray(addrs); // вызван FillAddresses()
        return a2;
      }
      else
      {
        string[] a2 = new string[strs.Length];
        for (int i = 0; i < strs.Length; i++)
          a2[i] = ToString(Parse(strs[i]));
        return a2;
      }
    }

    /// <summary>
    /// Комбинация вызовов TryParseArray() и ToStringArray() для кодированных строк адреса.
    /// При этом возвращаются строки в формате GuidMode.
    /// Если какая-либо из исходных строк имеет неправильный формат, метод возвращает false.
    /// 
    /// Предполагается, что свойство FillAddress установлено.
    /// Иначе корректный результат можно получить только для немногих преобразований GuidMode, например из AOGuidWithText в Text.
    /// </summary>
    /// <param name="strs">Кодированные строки. В процессе работы элементы массива заменяются. Если какую-либо строку не удалось
    /// преобразовать, она остается без изменений, а оставшиеся строки продолжают заменяться</param>
    /// <returns>Кодировананные строки в требуемом формате</returns>
    public bool TryConvertStringArray(string[] strs)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      if (strs.Length == 0)
        return true;

      FiasAddress addr;
      bool res = true;
      if (FillAddress)
      {
        // Придется частично повторить реализацию метода TryParseArray(), т.к. этот метод не возвращает признаков,
        // какие именно адреса оказались неправильными.

        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;

        FiasAddress[] addrs = new FiasAddress[strs.Length];
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            res = false;
          addrs[i] = addr;
        }

        // Заполняем адреса
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }


        string[] strs2 = conv2.ToStringArray(addrs); // опять без вызова FillAddress()
        strs2.CopyTo(strs, 0);
      }
      else
      {
        for (int i = 0; i < strs.Length; i++)
        {
          if (TryParse(strs[i], out addr))
            strs[i] = ToString(addr);
          else
            res = false;
        }
      }
      return res;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию объекта FiasAddressConvert с такими же значениями свойств
    /// </summary>
    /// <returns></returns>
    public FiasAddressConvert Clone()
    {
      FiasAddressConvert res = new FiasAddressConvert(_Source);
      res.GuidMode = this.GuidMode;
      res.FillAddress = this.FillAddress;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  // TODO: Кандидат на перенос в ExtTools.dll

  /// <summary>
  /// Парсер для пар Код=Значение
  /// </summary>
  internal static class StringKeyValueParser
  {
    #region Статические методы

    public static string ToString(KeyValuePair<string, string>[] a)
    {
      if (a.Length == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");

        if (String.IsNullOrEmpty(a[i].Key))
          throw new ArgumentNullException("a[" + i.ToString() + "]");

        sb.Append(a[i].Key);
        sb.Append('=');
        if (String.IsNullOrEmpty(a[i].Value))
          sb.Append("\"\"");
        else
        {
          sb.Append('\"');
          for (int j = 0; j < a[i].Value.Length; j++)
          {
            char c = a[i].Value[j];
            if (c == '\"')
              sb.Append("\"\"");
            else
              sb.Append(c);
          }
          sb.Append('\"');
        }
      }
      return sb.ToString();
    }

    public static KeyValuePair<string, string>[] Parse(string s)
    {
      ParsingData pd = new ParsingData(s);
      _TheParserList.Parse(pd);
      if (pd.FirstErrorToken != null)
        throw new InvalidCastException("Ошибка парсинга строки");
      List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
      string CurrName = null;
      bool HasEq = false;
      for (int i = 0; i < pd.Tokens.Count; i++)
      {
        switch (pd.Tokens[i].TokenType)
        {
          case "Key":
            if (CurrName != null)
              throw new InvalidCastException("Два ключа подряд");
            if (HasEq)
              throw new InvalidCastException("Ключ после знака \"=\"");
            CurrName = pd.Tokens[i].Text;
            break;
          case "=":
            if (HasEq)
              throw new InvalidCastException("Два знака \"=\" подряд");
            if (CurrName == null)
              throw new InvalidCastException("Не было ключа перед знаком \"=\"");
            HasEq = true;
            break;
          case "Const":
            if (!HasEq)
              throw new InvalidCastException("Не было знака \"=\"");
            lst.Add(new KeyValuePair<string, string>(CurrName, pd.Tokens[i].AuxData.ToString()));
            CurrName = null;
            HasEq = false;
            break;
          case "Space":
            break;
          default:
            throw new BugException("Неизвестная лексема " + pd.Tokens[i].TokenType);
        }
      }

      if (HasEq)
        throw new InvalidCastException("После знака \"=\" не было значения");
      if (CurrName != null)
        throw new InvalidCastException("После ключа не было значения");

      return lst.ToArray();
    }

    #endregion

    #region Парсинг

    private static readonly ParserList _TheParserList = CreateParserList();

    private static ParserList CreateParserList()
    {
      ParserList lst = new ParserList();
      lst.Add(new KeyParser()); // ключи
      lst.Add(new EqParser()); // знак "="
      lst.Add(new StrConstParser()); // значения
      lst.Add(new SpaceParser(' ', '\t', ','));
      return lst;
    }

    /// <summary>
    /// Парсер, распознающий имена
    /// </summary>
    private class KeyParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        if (!(Char.IsLetter(data.GetChar(data.CurrPos)) || data.GetChar(data.CurrPos) == '_'))
          return;
        int cnt = 1;
        while (Char.IsLetterOrDigit(data.GetChar(data.CurrPos + cnt)) || data.GetChar(data.CurrPos + cnt) == '_' || data.GetChar(data.CurrPos + cnt) == '.')
          cnt++;

        data.Tokens.Add(new Token(data, this, "Key", data.CurrPos, cnt));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        return null;
      }

      #endregion
    }

    /// <summary>
    /// Парсер для знака "="
    /// </summary>
    private class EqParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        if (data.GetChar(data.CurrPos) == '=')
          data.Tokens.Add(new Token(data, this, "=", data.CurrPos, 1));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        return null;
      }

      #endregion
    }

    #endregion
  }
}
