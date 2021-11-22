// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data;
using System.Diagnostics;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Вспомогательные статические методы и свойства для работы с ФИАС.
  /// </summary>
  public class FiasTools
  {
    #region Статический конструктор

    static FiasTools()
    {
      #region AOLevels

      AOLevels = new FiasLevel[] { 
        FiasLevel.Region,
        FiasLevel.AutonomousArea,
        FiasLevel.District,
        FiasLevel.Settlement,
        FiasLevel.City,
        FiasLevel.InnerCityArea,
        FiasLevel.Village,
        FiasLevel.PlanningStructure,
        FiasLevel.Street};

      AOLevelIndexer = new ArrayIndexer<FiasLevel>(AOLevels);

      #endregion

      #region AllLevels

      // При добавлении уровней не забыть про структуру FiasLevelSet

      AllLevels = new FiasLevel[] { 
        FiasLevel.Region,
        FiasLevel.AutonomousArea,
        FiasLevel.District,
        FiasLevel.Settlement,
        FiasLevel.City,
        FiasLevel.InnerCityArea,
        FiasLevel.Village,
        FiasLevel.PlanningStructure,
        FiasLevel.Street,
        FiasLevel.LandPlot,
        FiasLevel.House,
        //FiasLevel.AdditionalTerritory,
        //FiasLevel.AdditionalTerritoryObject,
        FiasLevel.Building,
        FiasLevel.Structure,
        FiasLevel.Flat,
        FiasLevel.Room
      };

      AllLevelIndexer = new ArrayIndexer<FiasLevel>(AllLevels);

      _DefaultAOTypes = new string[] { 
        String.Empty,
        String.Empty,
        "Район",
        String.Empty,
        "Город",
        String.Empty,
        String.Empty,
        String.Empty,
        "Улица",
        String.Empty,
        "Дом",
        "Корпус",
        "Строение",
        "Квартира",
        "Комната"
      };

#if DEBUG
      if (_DefaultAOTypes.Length != AllLevels.Length)
        throw new BugException("_DefaultAOTypes.Length");
#endif

      #endregion

      #region ParseLevels

      ParseLevels = new FiasLevel[] { 
        FiasLevel.Region,
        FiasLevel.District,
        FiasLevel.City,
        FiasLevel.Village,
        FiasLevel.PlanningStructure, // добавлено 03.11.2020
        FiasLevel.Street,
        FiasLevel.House,
        FiasLevel.Building,
        FiasLevel.Structure,
        FiasLevel.Flat,
        FiasLevel.Room
      };

      ParseLevelIndexer = new ArrayIndexer<FiasLevel>(ParseLevels);

      #endregion

      EmptyLevels = new FiasLevel[0];

      #region ValidChars

      // Буквы, цифры, римские цифры
      string s1 = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
      s1 = s1.ToUpperInvariant() + s1 + "1234567890" + "IVXCLM";

      _ValidAONameChars = new CharArrayIndexer(s1 + " -+.,:/\\()<>N№\"\'`" + DataTools.LeftDoubleAngleQuotationStr + DataTools.RightDoubleAngleQuotationStr, false);
      // бывает еще символ "l", не знаю зачем

      _ValidNumChars = new CharArrayIndexer(s1 + " -.,;/\\()<>N№\"", false);
      // бывают еще латинские буквы "f", "i", "A", "Z"

      _InvalidStartAONameChars = new CharArrayIndexer(" -+.,:/\\)<>\"\'`" + DataTools.LeftDoubleAngleQuotationStr + DataTools.RightDoubleAngleQuotationStr, false);
      _InvalidStartNumChars = new CharArrayIndexer(" -+.,:;/\\)<>N№\"\'`" + DataTools.LeftDoubleAngleQuotationStr + DataTools.RightDoubleAngleQuotationStr, false);
      _InvalidEndChars = new CharArrayIndexer(" -+(<`N№" + DataTools.LeftDoubleAngleQuotationStr, false);
      _InvaliDoubleChars = new CharArrayIndexer(" -+.,:;/\\()<>N№\"\'`" + DataTools.LeftDoubleAngleQuotationStr + DataTools.RightDoubleAngleQuotationStr);

      #endregion

      #region TracePrefixText

      _TracePrefixText = EnvironmentTools.ApplicationName;
      if (String.IsNullOrEmpty(_TracePrefixText))
        _TracePrefixText = "Unknown application";

      #endregion

      #region AOTypePlaceArray

      _AOTypePlaceArray = new bool[] {
        true, true, // FiasLevel.Region,
        true, true, // (?) FiasLevel.AutonomousArea,
        false, true, // FiasLevel.District,
        true, true, // (?) FiasLevel.Settlement,
        true, false, // FiasLevel.City,
        true, true, // (?) FiasLevel.InnerCityArea,
        true, false, // FiasLevel.Village,
        true, true, // FiasLevel.PlanningStructure,
        true, true, // FiasLevel.Street,
        true, true, // (?) FiasLevel.LandPlot,
        true, false, // FiasLevel.House,
        true, false, //FiasLevel.Building,
        true, false, //FiasLevel.Structure,
        true, false, //FiasLevel.Flat,
        true, false //FiasLevel.Room
      };

#if DEBUG
      if (_AOTypePlaceArray.Length != AllLevels.Length * 2)
        throw new BugException();
#endif

      #endregion
    }

    #endregion

    /// <summary>
    /// Фиктивный GUID.
    /// Используется, если метод поиска не нашел запись с заданным GUID'ом.
    /// Guid.Empty имеет другое назначение
    /// </summary>
    public static readonly Guid GuidNotFound = new Guid("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    /// <summary>
    /// Константа "Российская Федерация"
    /// </summary>
    public const string RF = "Российская Федерация";

    /// <summary>
    /// Уровень "Вся РФ".
    /// Свойство FiasCachedPage.Level для списка регионов
    /// </summary>
    internal static readonly FiasLevel LevelRF = (FiasLevel)0;

    /// <summary>
    /// Получить таблицу, соответствующую уровню адресного объекта.
    /// </summary>
    /// <param name="level">Уровень адресного объекта</param>
    /// <returns>Тип таблицы</returns>
    public static FiasTableType GetTableType(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.Region:
        case FiasLevel.AutonomousArea:
        case FiasLevel.District:
        case FiasLevel.Settlement:
        case FiasLevel.City:
        case FiasLevel.InnerCityArea:
        case FiasLevel.Village:
        case FiasLevel.PlanningStructure:
        case FiasLevel.Street:
        case FiasLevel.LandPlot:
          return FiasTableType.AddrOb;
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          return FiasTableType.House;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          return FiasTableType.Room;
        default:
          return FiasTableType.Unknown;
      }
    }

    internal static FiasLevel GetSpecialPageLevel(FiasSpecialPageType pageType)
    {
      switch (pageType)
      {
        case FiasSpecialPageType.AllCities: return FiasLevel.City;
        case FiasSpecialPageType.AllDistricts: return FiasLevel.District;
        case FiasSpecialPageType.DistrictCapitals: return FiasLevel.Village;
        default:
          throw new ArgumentException();
      }
    }

    /// <summary>
    /// Получение значение для сортировки номеров домов и квартир
    /// Возвращает числовое значение в диапазоне от 0 до 255 для строки, если она содержит цифры.
    /// Если номер не содержит цифр, возвращается 0.
    /// Если номер соответствует значению, большему 255, возвращается 255.
    /// Если номер является строкой, содержащей только число в диапазоне от 1 до 255, например, "25", то ссылка <paramref name="sNumber"/>
    /// заменяется на null, так как его можно не хранить в базе данных
    /// </summary>
    /// <param name="sNumber">Номер в виде строки</param>
    /// <returns></returns>
    internal static int GetNumInt(ref string sNumber)
    {
      if (String.IsNullOrEmpty(sNumber))
        return 0;

      int nChars = Math.Min(sNumber.Length, 3);
      int nDigs = 0;
      for (int i = 0; i < nChars; i++)
      {
        if (sNumber[i] >= '0' && sNumber[i] <= '9')
          nDigs++;
        else
          break;
      }
      if (nDigs == 0)
        return 0; // не число

      int v = int.Parse(sNumber.Substring(0, nDigs));
      if (v == 0)
        return 0; // значения "00" и "000"
      if (v > 255)
        return 255;
      if (nDigs == sNumber.Length)
        sNumber = null; // не нужно хранить в базе данных
      return v;
    }

    /// <summary>
    /// Значение поля "ActualDate" таблицы "ClassifInfo", если классификатор еще не загружен
    /// </summary>
    internal static readonly DateTime UnknownActualDate = new DateTime(2000, 1, 1);

    #region Список кодов регионов

    /// <summary>
    /// Справочник регионов. Таблица содержит поля "Code" и "Name".
    /// Коды регионов хранятся как двусимвольные строки ("01", ...)
    /// Первичный ключ - поле "Code"
    /// 
    /// Эта таблица является фиксированной.
    /// В прикладном коде следует использовать метод FiasHandler.GetRegionAOGuid() или свойство RegionCodes
    /// </summary>
    public static readonly DataTable RegionCodes = CreateRegionCodes();

    private static DataTable CreateRegionCodes()
    {
      string[] a = new string[] { 
        "01", "Республика Адыгея (Адыгея)", "d8327a56-80de-4df2-815c-4f6ab1224c50",
        "02", "Республика Башкортостан", "6f2cbfd8-692a-4ee4-9b16-067210bde3fc",
        "03", "Республика Бурятия", "a84ebed3-153d-4ba9-8532-8bdf879e1f5a",
        "04", "Республика Алтай", "5c48611f-5de6-4771-9695-7e36a4e7529d",
        "05", "Республика Дагестан", "0bb7fa19-736d-49cf-ad0e-9774c4dae09b",
        "06", "Республика Ингушетия", "b2d8cd20-cabc-4deb-afad-f3c4b4d55821",
        "07", "Кабардино-Балкарская Республика", "1781f74e-be4a-4697-9c6b-493057c94818",
        "08", "Республика Калмыкия", "491cde9d-9d76-4591-ab46-ea93c079e686",
        "09", "Карачаево-Черкесская Республика", "61b95807-388a-4cb1-9bee-889f7cf811c8",
        "10", "Республика Карелия", "248d8071-06e1-425e-a1cf-d1ff4c4a14a8",
        "11", "Республика Коми", "c20180d9-ad9c-46d1-9eff-d60bc424592a",
        "12", "Республика Марий Эл", "de2cbfdf-9662-44a4-a4a4-8ad237ae4a3e",
        "13", "Республика Мордовия", "37a0c60a-9240-48b5-a87f-0d8c86cdb6e1",
        "14", "Республика Саха (Якутия)", "c225d3db-1db6-4063-ace0-b3fe9ea3805f",
        "15", "Республика Северная Осетия - Алания", "de459e9c-2933-4923-83d1-9c64cfd7a817",
        "16", "Республика Татарстан (Татарстан)", "0c089b04-099e-4e0e-955a-6bf1ce525f1a",
        "17", "Республика Тыва", "026bc56f-3731-48e9-8245-655331f596c0",
        "18", "Удмуртская Республика", "52618b9c-bcbb-47e7-8957-95c63f0b17cc",
        "19", "Республика Хакасия", "8d3f1d35-f0f4-41b5-b5b7-e7cadf3e7bd7",
        "20", "Чеченская Республика", "de67dc49-b9ba-48a3-a4cc-c2ebfeca6c5e",
        "21", "Чувашская Республика - Чувашия", "878fc621-3708-46c7-a97f-5a13a4176b3e",
        "22", "Алтайский край", "8276c6a1-1a86-4f0d-8920-aba34d4cc34a",
        "23", "Краснодарский край", "d00e1013-16bd-4c09-b3d5-3cb09fc54bd8",
        "24", "Красноярский край", "db9c4f8b-b706-40e2-b2b4-d31b98dcd3d1",
        "25", "Приморский край", "43909681-d6e1-432d-b61f-ddac393cb5da",
        "26", "Ставропольский край", "327a060b-878c-4fb4-8dc4-d5595871a3d8",
        "27", "Хабаровский край", "7d468b39-1afa-41ec-8c4f-97a8603cb3d4",
        "28", "Амурская область", "844a80d6-5e31-4017-b422-4d9c01e9942c",
        "29", "Архангельская область", "294277aa-e25d-428c-95ad-46719c4ddb44",
        "30", "Астраханская область", "83009239-25cb-4561-af8e-7ee111b1cb73",
        "31", "Белгородская область", "639efe9d-3fc8-4438-8e70-ec4f2321f2a7",
        "32", "Брянская область", "f5807226-8be0-4ea8-91fc-39d053aec1e2",
        "33", "Владимирская область", "b8837188-39ee-4ff9-bc91-fcc9ed451bb3",
        "34", "Волгоградская область", "da051ec8-da2e-4a66-b542-473b8d221ab4",
        "35", "Вологодская область", "ed36085a-b2f5-454f-b9a9-1c9a678ee618",
        "36", "Воронежская область", "b756fe6b-bbd3-44d5-9302-5bfcc740f46e",
        "37", "Ивановская область", "0824434f-4098-4467-af72-d4f702fed335",
        "38", "Иркутская область", "6466c988-7ce3-45e5-8b97-90ae16cb1249",
        "39", "Калининградская область", "90c7181e-724f-41b3-b6c6-bd3ec7ae3f30",
        "40", "Калужская область", "18133adf-90c2-438e-88c4-62c41656de70",
        "41", "Камчатский край", "d02f30fc-83bf-4c0f-ac2b-5729a866a207",
        "42", "Кемеровская область", "393aeccb-89ef-4a7e-ae42-08d5cebc2e30",
        "43", "Кировская область", "0b940b96-103f-4248-850c-26b6c7296728",
        "44", "Костромская область", "15784a67-8cea-425b-834a-6afe0e3ed61c",
        "45", "Курганская область", "4a3d970f-520e-46b9-b16c-50d4ca7535a8",
        "46", "Курская область", "ee594d5e-30a9-40dc-b9f2-0add1be44ba1",
        "47", "Ленинградская область", "6d1ebb35-70c6-4129-bd55-da3969658f5d",
        "48", "Липецкая область", "1490490e-49c5-421c-9572-5673ba5d80c8",
        "49", "Магаданская область", "9c05e812-8679-4710-b8cb-5e8bd43cdf48",
        "50", "Московская область", "29251dcf-00a1-4e34-98d4-5c47484a36d4",
        "51", "Мурманская область", "1c727518-c96a-4f34-9ae6-fd510da3be03",
        "52", "Нижегородская область", "88cd27e2-6a8a-4421-9718-719a28a0a088",
        "53", "Новгородская область", "e5a84b81-8ea1-49e3-b3c4-0528651be129",
        "54", "Новосибирская область", "1ac46b49-3209-4814-b7bf-a509ea1aecd9",
        "55", "Омская область", "05426864-466d-41a3-82c4-11e61cdc98ce",
        "56", "Оренбургская область", "8bcec9d6-05bc-4e53-b45c-ba0c6f3a5c44",
        "57", "Орловская область", "5e465691-de23-4c4e-9f46-f35a125b5970",
        "58", "Пензенская область", "c99e7924-0428-4107-a302-4fd7c0cca3ff",
        "59", "Пермский край", "4f8b1a21-e4bb-422f-9087-d3cbf4bebc14",
        "60", "Псковская область", "f6e148a1-c9d0-4141-a608-93e3bd95e6c4",
        "61", "Ростовская область", "f10763dc-63e3-48db-83e1-9c566fe3092b",
        "62", "Рязанская область", "963073ee-4dfc-48bd-9a70-d2dfc6bd1f31",
        "63", "Самарская область", "df3d7359-afa9-4aaa-8ff9-197e73906b1c",
        "64", "Саратовская область", "df594e0e-a935-4664-9d26-0bae13f904fe",
        "65", "Сахалинская область", "aea6280f-4648-460f-b8be-c2bc18923191",
        "66", "Свердловская область", "92b30014-4d52-4e2e-892d-928142b924bf",
        "67", "Смоленская область", "e8502180-6d08-431b-83ea-c7038f0df905",
        "68", "Тамбовская область", "a9a71961-9363-44ba-91b5-ddf0463aebc2",
        "69", "Тверская область", "61723327-1c20-42fe-8dfa-402638d9b396",
        "70", "Томская область", "889b1f3a-98aa-40fc-9d3d-0f41192758ab",
        "71", "Тульская область", "d028ec4f-f6da-4843-ada6-b68b3e0efa3d",
        "72", "Тюменская область", "54049357-326d-4b8f-b224-3c6dc25d6dd3",
        "73", "Ульяновская область", "fee76045-fe22-43a4-ad58-ad99e903bd58",
        "74", "Челябинская область", "27eb7c10-a234-44da-a59c-8b1f864966de",
        "75", "Забайкальский край", "b6ba5716-eb48-401b-8443-b197c9578734",
        "76", "Ярославская область", "a84b2ef4-db03-474b-b552-6229e801ae9b",
        "77", "Г. Москва", "0c5b2444-70a0-4932-980c-b4dc0d3f02b5",
        "78", "Г. Санкт-Петербург", "c2deb16a-0330-4f05-821f-1d09c93331e6",
        "79", "Еврейская автономная область", "1b507b09-48c9-434f-bf6f-65066211c73e",
        "83", "Ненецкий автономный округ", "89db3198-6803-4106-9463-cbf781eff0b8",
        "86", "Ханты-Мансийский автономный округ - Югра", "d66e5325-3a25-4d29-ba86-4ca351d9704b",
        "87", "Чукотский автономный округ", "f136159b-404a-4f1f-8d8d-d169e1374d5c",
        "89", "Ямало-Ненецкий автономный округ", "826fa834-3ee8-404f-bdbc-13a5221cfb6e",
        "91", "Республика Крым", "bd8e6511-e4b9-4841-90de-6bbc231a789e",
        "92", "Г. Севастополь", "6fdecb78-893a-4e3f-a5ba-aa062459463b",
        "99", "Иные территории, включая город и космодром Байконур", "63ed1a35-4be6-4564-a1ec-0c51f7383314" };

      DataTable table = new DataTable();
      table.Columns.Add("Code", typeof(string));
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("AOGuid", typeof(Guid));

      for (int i = 0; i < a.Length; i += 3)
        table.Rows.Add(a[i], a[i + 1], new Guid(a[i + 2]));

      table.AcceptChanges();
      DataTools.SetPrimaryKey(table, "Code");
      return table;
    }

    /// <summary>
    /// Коды регионов городов федерального значения
    /// </summary>
    public static readonly string[] FederalCityRegionCodes = new string[] { "77", "78", "92" };

    internal static readonly int[] FederalCityRegionCodesInt = new int[] { 77, 78, 92 };

    #endregion

    #region Полнотекстный поиск

    /// <summary>
    /// Максимальное количество строк адресных объектов, которое может быть возвращено полнотекстным поиском
    /// </summary>
    public const int AddressSearchLimit = 10000;

    /// <summary>
    /// Не используется в прикладном коде
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string PrepareForFTS(string s)
    {
      s = s.ToUpperInvariant();
      s = s.Replace('Ё', 'Е');
      return s;
    }

    #endregion

    #region Уровни и наследование

    /// <summary>
    /// Список уровней адресного объекта, начиная с Region и заканчивая Street
    /// </summary>
    public static readonly FiasLevel[] AOLevels;

    /// <summary>
    /// Индексатор для AOLevels
    /// </summary>
    internal static readonly ArrayIndexer<FiasLevel> AOLevelIndexer;

    /// <summary>
    /// Все уровни, которые могут хранится в адресе, начиная с Region и кончая Room.
    /// Список также содержит уровни Structure, Building и Room, которые не определены в ФИАС, но для которых
    /// могут задаваться компоненты.
    /// Порядок уровней в массиве соответствует иерархии (в порядке убывания).
    /// </summary>
    public static readonly FiasLevel[] AllLevels;

    /// <summary>
    /// Уровни, которые могут иметь сокращение
    /// </summary>
    internal static readonly ArrayIndexer<FiasLevel> AllLevelIndexer;

    /// <summary>
    /// Уровни из AllLevels, которые могут использоваться при парсинге адресов.
    /// Из списка исключены неиспользуемые уровни
    /// </summary>
    internal static readonly FiasLevel[] ParseLevels;

    /// <summary>
    /// Уровни, которые могут иметь сокращение
    /// </summary>
    internal static readonly ArrayIndexer<FiasLevel> ParseLevelIndexer;

    /// <summary>
    /// Пустой массив уровней
    /// </summary>
    public static readonly FiasLevel[] EmptyLevels;

    /// <summary>
    /// Возвращает true, если адресные объекты уровня <paramref name="child"/> могут непосредственно располагаться
    /// в адресных объектах уровня <paramref name="parent"/>.
    /// Наследование определено в смысле адресного объекта.
    /// В редакторе адреса допускаются дополнительные пропуски, например, можно выбирать город без региона.
    /// При этом недостающие уровни заполняются автоматически.
    /// </summary>
    /// <param name="parent">Родительский уровень. Может быть Unknown</param>
    /// <param name="child">Дочерний уровень</param>
    /// <param name="distinctHouseAndRoomLevels">Если true, то будут различаться уровни House, Building и Structure, Flat и Room.
    /// То есть, корпус может принадлежать только зданию, но не улице.
    /// Если false, то 3 уровня для зданий и 2 для помещений не различаются</param>
    /// <returns>Возможность наследования</returns>
    public static bool IsInheritableLevel(FiasLevel parent, FiasLevel child, bool distinctHouseAndRoomLevels)
    {
      if (!distinctHouseAndRoomLevels)
      {
        parent = ReplaceToHouseOrFlat(parent);
        child = ReplaceToHouseOrFlat(child);
      }

      switch (child)
      {
        case FiasLevel.Region:
          switch (parent)
          {
            case FiasLevel.Unknown:
              return true;
            default:
              return false;
          }
        case FiasLevel.District:
          switch (parent)
          {
            case FiasLevel.Region:
              return true;
            default:
              return false;
          }
        case FiasLevel.City:
          switch (parent)
          {
            case FiasLevel.Region:
            case FiasLevel.District:
              return true;
            default:
              return false;
          }
        case FiasLevel.Village:
          switch (parent)
          {
            case FiasLevel.Region:
            case FiasLevel.District:
            case FiasLevel.City:
              return true;
            default:
              return false;
          }
        case FiasLevel.PlanningStructure:
          switch (parent)
          {
            case FiasLevel.Region: // Москва
            case FiasLevel.District: // 27.10.2020
            case FiasLevel.City:
            case FiasLevel.Village:
              return true;
            default:
              return false;
          }
        case FiasLevel.Street:
          switch (parent)
          {
            case FiasLevel.Region: // Москва
            case FiasLevel.City:
            case FiasLevel.Village:
            case FiasLevel.PlanningStructure:
              return true;
            default:
              return false;
          }
        case FiasLevel.House:
          switch (parent)
          {
            //case FiasLevel.City: В ФИАСе бывают здания в городе, но эти записи неправильные

            case FiasLevel.Village:
            case FiasLevel.Street:
            case FiasLevel.PlanningStructure: // 02.10.2020
              return true;
            default:
              return false;
          }
        case FiasLevel.Building:
          switch (parent)
          {
            case FiasLevel.House: // корпус может быть только в доме, но не в строении
              return true;
            default:
              return false;
          }
        case FiasLevel.Structure:
          switch (parent)
          {
            case FiasLevel.Village:
            case FiasLevel.Street:
            case FiasLevel.PlanningStructure:
            case FiasLevel.House:
            case FiasLevel.Building:
              return true;
            default:
              return false;
          }
        case FiasLevel.Flat:
          switch (parent)
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Structure:
              return true;
            default:
              return false;
          }
        case FiasLevel.Room:
          switch (parent)
          {
            case FiasLevel.Flat:
              return true;
            default:
              return false;
          }

        default:
          //throw new ArgumentException("Неизвестный дочерний уровень: " + child.ToString(), "child");
          return false;
      }
    }

    /// <summary>
    /// Получить список возможных дистанций наследования для двух уровней адресных объектов
    /// </summary>
    /// <param name="parent">Уровень родительского объекта</param>
    /// <param name="child">Уровень дочерего объекта</param>
    /// <param name="distances">Заполняемый список</param>
    internal static void GetPossibleInheritanceDistance(FiasLevel parent, FiasLevel child, SingleScopeList<int> distances)
    {
      int pParent = AOLevelIndexer.IndexOf(parent);
      int pChild = AOLevelIndexer.IndexOf(child);
      if (pParent < 0)
        throw new ArgumentException("Уровень " + parent.ToString() + " не относится к адресным объектам", "parent");
      if (pChild < 0)
        throw new ArgumentException("Уровень " + child.ToString() + " не относится к адресным объектам", "child");
      if (pChild <= pParent)
        return;

      // TODO: Использовать IsInheritableLevel()
      int n = pChild - pParent;
      for (int i = 1; i <= n; i++)
        distances.Add(i);
    }

    /// <summary>
    /// Получить список возможных дистанций наследования для двух уровней адресных объектов
    /// </summary>
    /// <param name="parent">Уровень родительского объекта</param>
    /// <param name="child">Уровень дочерего объекта</param>
    /// <returns>Массив возможных дистанций. Если <paramref name="child"/> не может ни прямо, ни косвенно наследовать из <paramref name="parent"/> (например, район из улицы), возвращается пустой массив</returns>
    public static int[] GetPossibleInheritanceDistance(FiasLevel parent, FiasLevel child)
    {
      SingleScopeList<int> distances = new SingleScopeList<int>();
      GetPossibleInheritanceDistance(parent, child, distances);
      distances.Sort();
      return distances.ToArray();
    }

    /// <summary>
    /// Корректирует уровень. Заменяет Building и Structure на House.
    /// Заменяет Room на Flat.
    /// Остальные уровни не заменяются
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    internal static FiasLevel ReplaceToHouseOrFlat(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.Building:
        case FiasLevel.Structure:
          return FiasLevel.House;
        case FiasLevel.Room:
          return FiasLevel.Flat;
        default:
          return level;
      }
    }

    /// <summary>
    /// Корректирует уровень. Заменяет House и Building на Structure.
    /// Заменяет Flat на Room.
    /// Остальные уровни не заменяются.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    internal static FiasLevel ReplaceToStructureOrRoom(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
          return FiasLevel.Structure;
        case FiasLevel.Flat:
          return FiasLevel.Room;
        default:
          return level;
      }
    }



    #endregion

    #region Поля дат

    internal static DateTime? GetStartOrEndDate(IFiasSource _Source, DataRow _Row, bool isStartDate)
    {
      if (_Source.DBSettings.UseDates)
      {
        if (_Source.InternalSettings.UseOADates)
        {
          int v = DataTools.GetInt(_Row, isStartDate ? "dStartDate" : "dEndDate");
          if (v == 0)
            return null;
          else
            return DateTime.FromOADate(v);
        }
        else
          return DataTools.GetNullableDateTime(_Row, isStartDate ? "STARTDATE" : "ENDDATE");
      }
      else
        return null;
    }

    #endregion

    internal static void InitTopFlagAndDatesRowFilter(DataView dv)
    {
      string s = "TopFlag=TRUE";
      if (dv.Table.Columns.IndexOf("Actual") >= 0)
        s += " AND Actual=TRUE";
      if (dv.Table.Columns.IndexOf("Live") >= 0)
        s += " AND Live=TRUE";
      if (dv.Table.Columns.IndexOf("dStartDate") >= 0)
      {
        int dToday = (int)(DateTime.Today.ToOADate());
        s += " AND dStartDate<=" + dToday.ToString() + " AND dEndDate>=" + dToday.ToString();
      }
      else if (dv.Table.Columns.IndexOf("STARTDATE") >= 0)
      {
        s += " AND " + new DateRangeInclusionFilter("STARTDATE", "ENDDATE", DateTime.Today).ToString();
      }
      dv.RowFilter = s;
    }

    #region Именная часть

    private static readonly CharArrayIndexer _ValidAONameChars;

    private static readonly CharArrayIndexer _ValidNumChars;

    private static readonly CharArrayIndexer _InvalidStartAONameChars;

    private static readonly CharArrayIndexer _InvalidStartNumChars;

    private static readonly CharArrayIndexer _InvalidEndChars;

    private static readonly CharArrayIndexer _InvaliDoubleChars;

    /// <summary>
    /// Проверяет корректность именной части (без сокращения).
    /// Пустая строка считается правильным значением
    /// </summary>
    /// <param name="s">Проверяемый текст</param>
    /// <param name="level">Уровень объекта из списка AllLevels</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>true, если имя правильное</returns>
    public static bool IsValidName(string s, FiasLevel level, out string errorText)
    {
      if (String.IsNullOrEmpty(s))
      {
        errorText = null;
        return true;
      }

      bool isNum = GetTableType(level) != FiasTableType.AddrOb; // номера здания или помещений?

      int p = DataTools.IndexOfAnyOther(s, isNum ? _ValidNumChars : _ValidAONameChars);
      if (p >= 0)
      {
        errorText = "Недопустимый символ \"" + s[p] + "\" в позиции " + (p + 1).ToString();
        return false;
      }

      if ((isNum ? _InvalidStartNumChars : _InvalidStartAONameChars).Contains(s[0]))
      {
        errorText = "Строка не может начинаться с \"" + s[0] + "\"";
        return false;
      }
      if (_InvalidEndChars.Contains(s[s.Length - 1]))
      {
        errorText = "Строка не может заканчиваться с \"" + s[s.Length - 1] + "\"";
        return false;
      }

      for (int i = 1; i < s.Length; i++)
      {
        if (s[i] == s[i - 1] && _InvaliDoubleChars.Contains(s[i]))
        {
          errorText = "Два одинаковых символа \"" + s.Substring(i - 1, 2) + "\" в позиции " + i.ToString();
          return false;
        }
      }

      // TODO: Проверка недопустимых сочетаний типа "+-"

      errorText = null;
      return true;
    }

    /// <summary>
    /// Проверяет корректность именной части (без сокращения)
    /// </summary>
    /// <param name="s">Проверяемый текст</param>
    /// <param name="level">Уровень объекта из списка AllLevels</param>
    /// <returns>true, если имя правильное</returns>
    public static bool IsValidName(string s, FiasLevel level)
    {
      string errorText;
      return IsValidName(s, level, out errorText);
    }

    #endregion

    #region Типы адресного объекта

    private static readonly string[] _DefaultAOTypes;

    /// <summary>
    /// Возвращает тип адресообразующего элемента по умолчанию для заданного уровня.
    /// Например, возвращает "Город" для <paramref name="level"/>=City.
    /// Если для уровня нет "основного" типа, например, для <paramref name="level"/>=Region,
    /// возвращается пустая строка.
    /// </summary>
    /// <param name="level">Уровень адреса</param>
    /// <returns>Тип</returns>
    public static string GetDefaultAOType(FiasLevel level)
    {
      int p = AllLevelIndexer.IndexOf(level);
      if (p >= 0)
        return _DefaultAOTypes[p];
      else
        return String.Empty;
    }


    /// <summary>
    /// По 2 элемента на уровень из AllLevels
    /// Первый - значение beforeName,
    /// Второй - значение afterName,
    /// </summary>
    private static bool[] _AOTypePlaceArray;

    /// <summary>
    /// Возвращает положение, которое может занимать тип адресного объекта относительно именной части.
    /// В одно из значений <paramref name="beforeName"/> или <paramref name="afterName"/> обязательно записывается true,
    /// но могут быть и 2 значения true.
    /// </summary>
    /// <param name="level">Уровень из списка AllLevels</param>
    /// <param name="beforeName">Сюда записывается true, если тип может идти перед наименованием ("город Тюмень")</param>
    /// <param name="afterName">Сюда записывается true, если тип может идти после наименования ("Тюменский район")</param>
    public static void GetAOTypePlace(FiasLevel level, out bool beforeName, out bool afterName)
    {
      int pLevel = AllLevelIndexer.IndexOf(level);
      if (pLevel < 0)
        throw new ArgumentOutOfRangeException("level");

      beforeName = _AOTypePlaceArray[2 * pLevel];
      afterName = _AOTypePlaceArray[2 * pLevel + 1];
    }

    #endregion

    /// <summary>
    /// Уровень редактирования адреса, используемый по умолчанию
    /// </summary>
    public const FiasEditorLevel DefaultEditorLevel = FiasEditorLevel.Room;

    /// <summary>
    /// Минимальный уровень адресных объектов, которые должны выбираться из справочника ФИАС, а не задаваться вручную.
    /// Значение по умолчанию (City).
    /// </summary>
    public const FiasLevel DefaultMinRefBookLevel = FiasLevel.City;

    #region Почтовый индекс

    internal static readonly FreeLibSet.Formatting.SimpleDigitalMaskProvider PostalCodeMaskProvider = new FreeLibSet.Formatting.SimpleDigitalMaskProvider(6);

    #endregion

    #region GUID'ы

    private static readonly CharArrayIndexer GuidChars = new CharArrayIndexer("0123456789abcdefABCDEF-(){}[]", false);

    [DebuggerStepThrough]
    internal static bool TryParseGuid(string s, out Guid value)
    {
      value = Guid.Empty;

      if (String.IsNullOrEmpty(s))
        return true;

      if (DataTools.IndexOfAnyOther(s, GuidChars) >= 0)
        return false;

      try
      {
        value = new Guid(s);
        return true;
      }
      catch
      {
        return false;
      }
    }

    #endregion

    #region Трассировка

    /// <summary>
    /// Вывод трассировочной информации (переключатель).
    /// Автоматически инициализируется, если в конфигурационном файле приложения задан ключ "TraceFias"="1".
    /// Обычно следует включать трассировку только в отладочных целях на небольщой промежуток времени, т.к. выводится довольно большой объем информации.
    /// Не влияет на трассировку действий, связанных с обновлением классификатора
    /// </summary>
    public static readonly System.Diagnostics.BooleanSwitch TraceSwitch = new System.Diagnostics.BooleanSwitch("TraceFias", "Вывод подробной отладочной информации при обращениях к классификатору ФИАС");

    /// <summary>
    /// Текст, который выводится в начале каждого сообщения.
    /// По умолчанию содержит имя приложения.
    /// Сообщения имеют вид "TracePrefixText. ДД.ММ.ГГГГ ЧЧ:ММ:СС ФИАС. Текст сообщения"
    /// </summary>
    public static string TracePrefixText
    {
      get { return _TracePrefixText; }
      set
      {
        if (value == null)
          _TracePrefixText = String.Empty;
        else
          _TracePrefixText = value;
      }
    }
    private static string _TracePrefixText;

    /// <summary>
    /// Возвращает текст, который нужно вывести перед сообщением для трассировки
    /// </summary>
    /// <returns></returns>
    internal static string GetTracePrefix()
    {
      string s2 = DateTime.Now.ToString("G");
      if (_TracePrefixText.Length > 0)
        return _TracePrefixText + ". " + s2 + " FIAS. ";
      else
        return s2 + " FIAS. ";
    }

    #endregion
  }


  /// <summary>
  /// Компактная структура для хранения списка уровней FiasLevel из массива FiasTools.AllLevels.
  /// Уровни House/Building/Structure и Flat/Room отличаются.
  /// </summary>
  [Serializable]
  public struct FiasLevelSet : IEnumerable<FiasLevel>, IEquatable<FiasLevelSet>
  {
    #region Константы уровней

    const int RegionBit = 0x1;
    const int AutonomousAreaBit = 0x2;
    const int DistrictBit = 0x4;
    const int SettlementBit = 0x8;
    const int CityBit = 0x10;
    const int InnerCityAreaBit = 0x20;
    const int VillageBit = 0x40;
    const int PlanningStructureBit = 0x80;
    const int StreetBit = 0x100;
    const int LandPlotBit = 0x200;
    const int HouseBit = 0x400;
    const int BuildingBit = 0x800;
    const int StructureBit = 0x1000;
    const int FlatBit = 0x2000;
    const int RoomBit = 0x4000;

    const int AllBits = 0x7FFF;

    const int AllHouseBits = HouseBit | BuildingBit | StructureBit;
    const int AllRoomBits = FlatBit | RoomBit;
    const int AllAOBits = AllBits & (~AllHouseBits) & (~AllRoomBits);

    #endregion

    #region Конструкторы

    internal FiasLevelSet(int intValue)
    {
      _IntValue = intValue;
    }

    /// <summary>
    /// Создает набор из одного уровня
    /// </summary>
    /// <param name="level">Уровень из списка FiasTools.AllLevels</param>
    public FiasLevelSet(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть из списка FiasTools.AllLevels");
      _IntValue = 1 << p;
    }

    /// <summary>
    /// Создает набор из заданного списка уровней
    /// </summary>
    /// <param name="levels"></param>
    public FiasLevelSet(FiasLevel[] levels)
    {
      _IntValue = 0;
      for (int i = 0; i < levels.Length; i++)
      {
        int p = FiasTools.AllLevelIndexer.IndexOf(levels[i]);
        if (p < 0)
          throw new ArgumentOutOfRangeException("levels", levels[i], "Уровень должен быть из списка FiasTools.AllLevels");
        _IntValue |= 1 << p;
      }
    }

    #endregion

    #region Поле данных

    private readonly int _IntValue;

    /// <summary>
    /// Возвращает true, если установлен флажок для соответствующего уровня
    /// </summary>
    /// <param name="level">Уровень из списка FiasTools.AllLevels</param>
    /// <returns>Наличие уровня</returns>
    public bool this[FiasLevel level]
    {
      get
      {
        int p = FiasTools.AllLevelIndexer.IndexOf(level);
        if (p >= 0)
          return (_IntValue & (1 << p)) != 0;
        else
          return false;
      }
    }

    /// <summary>
    /// Возвращает массив установленных уровней.
    /// Уровни отсортированы в от верхних (регион) к нижним (помещение)
    /// </summary>
    public FiasLevel[] ToArray()
    {
      if (_IntValue == 0)
        return FiasTools.EmptyLevels;
      if (_IntValue == AllBits)
        return FiasTools.AllLevels;

      int cnt = 0;
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if ((_IntValue & (1 << i)) != 0)
          cnt++;
      }

      FiasLevel[] a = new FiasLevel[cnt];
      cnt = 0;
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if ((_IntValue & (1 << i)) != 0)
        {
          a[cnt] = FiasTools.AllLevels[i];
          cnt++;
        }
      }
      return a;
    }

    /// <summary>
    /// Возвращает true, если в наборе не задано ни одного уровня
    /// </summary>
    public bool IsEmpty { get { return _IntValue == 0; } }

    /// <summary>
    /// Возвращает количество уровней в списке
    /// </summary>
    public int Count
    {
      get
      {
        if (_IntValue == 0)
          return 0;
        if (_IntValue == AllBits)
          return FiasTools.AllLevels.Length;

        int cnt = 0;
        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          if ((_IntValue & (1 << i)) != 0)
            cnt++;
        }
        return cnt;
      }
    }

    #endregion

    #region Первый и последний уровни

    /// <summary>
    /// Возвращает верхний уровень в списке.
    /// Если IsEmpty=true, возвращает Unknown
    /// </summary>
    public FiasLevel TopLevel
    {
      get
      {
        if (_IntValue == 0)
          return FiasLevel.Unknown;

        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          if ((_IntValue & (1 << i)) != 0)
            return FiasTools.AllLevels[i];
        }
        throw new BugException();
      }
    }

    /// <summary>
    /// Возвращает нижний уровень в списке.
    /// Если IsEmpty=true, возвращает Unknown
    /// </summary>
    public FiasLevel BottomLevel
    {
      get
      {
        if (_IntValue == 0)
          return FiasLevel.Unknown;

        for (int i = FiasTools.AllLevels.Length - 1; i >= 0; i--)
        {
          if ((_IntValue & (1 << i)) != 0)
            return FiasTools.AllLevels[i];
        }
        throw new BugException();
      }
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Читаемое текстовое представление.
    /// Для обратимого преобразования в строку (например, для сохранения набора в настройках программы)
    /// используйте класс FiasLevelSetConvert.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_IntValue == 0)
        return "Нет";

      StringBuilder sb = new StringBuilder();
      int cnt = 0;
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if ((_IntValue & (1 << i)) != 0)
        {
          if (cnt > 0)
            sb.Append(", ");
          sb.Append(FiasEnumNames.ToString(FiasTools.AllLevels[i], false));
          cnt++;
        }
      }
      return sb.ToString();
    }

    #endregion

    #region Логические операции

    /// <summary>
    /// Возвращает набор, содержащий уровни, которые входят сразу в оба исходных набора
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат пересечения</returns>
    public static FiasLevelSet operator &(FiasLevelSet a, FiasLevelSet b)
    {
      return new FiasLevelSet(a._IntValue & b._IntValue);
    }

    /// <summary>
    /// Возвращает набор, содержащий уровни, которые входят хотя бы в один из исходных наборов
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator |(FiasLevelSet a, FiasLevelSet b)
    {
      return new FiasLevelSet(a._IntValue | b._IntValue);
    }

    /// <summary>
    /// Возвращает набор, содержащий уровни из первого набора и второй заданный уровень.
    /// Если <paramref name="b"/>=FiasLevel.Unknown, возвращается <paramref name="a"/>.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Добавляемый уровень</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator |(FiasLevelSet a, FiasLevel b)
    {
      if (b == FiasLevel.Unknown)
        return a;
      return a | new FiasLevelSet(b);
    }

    /// <summary>
    /// Возвращает набор, содержащий уровни, которых нет в исходном наборе
    /// </summary>
    /// <param name="a">Исходный набор</param>
    /// <returns>Инвертированный набор</returns>
    public static FiasLevelSet operator ~(FiasLevelSet a)
    {
      return new FiasLevelSet((~a._IntValue) & AllBits);
    }

    /// <summary>
    /// Возвращает набор, содержащий уровни, которые входят хотя бы в один из исходных наборов.
    /// Эквивалентно операции ИЛИ.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator +(FiasLevelSet a, FiasLevelSet b)
    {
      return new FiasLevelSet(a._IntValue | b._IntValue);
    }


    /// <summary>
    /// Возвращает набор, содержащий уровни, которые входят в первый набор, но которых нет во втором наборе.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator -(FiasLevelSet a, FiasLevelSet b)
    {
      return new FiasLevelSet(a._IntValue & (~b._IntValue));
    }

    /// <summary>
    /// Возвращает набор, содержащий исходные уровни и дополнительный уровень.
    /// Эквивалентно операции ИЛИ
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй уровень</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator +(FiasLevelSet a, FiasLevel b)
    {
      return a + FromLevel(b);
    }

    /// <summary>
    /// Возвращает набор, содержащий исходные уровни, за исключением одного уровня.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй уровень</param>
    /// <returns>Результат объединения</returns>
    public static FiasLevelSet operator -(FiasLevelSet a, FiasLevel b)
    {
      return a - FromLevel(b);
    }

    #endregion

    #region Получение других множеств

    /// <summary>
    /// Возвращает набор, который содержит уровни из текущего набора, находяшиеся ниже заданного.
    /// Уровень <paramref name="level"/> не будет входить в новый набор.
    /// </summary>
    /// <param name="level">Уровень, ниже которого надо сохранить уровни</param>
    /// <returns></returns>
    public FiasLevelSet GetBelow(FiasLevel level)
    {
      if (level == FiasLevel.Unknown)
        return this;

      int v = _IntValue;
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentOutOfRangeException();
      for (int i = 0; i <= p; i++)
        v &= ~(1 << i);

      return new FiasLevelSet(v);
    }

    /// <summary>
    /// Возвращает набор, который содержит уровни из текущего набора, находяшиеся выше заданного
    /// Уровень <paramref name="level"/> не будет входить в новый набор.
    /// </summary>
    /// <param name="level">Уровень, выше которого надо сохранить уровни</param>
    /// <returns></returns>
    public FiasLevelSet GetAbove(FiasLevel level)
    {
      if (level == FiasLevel.Unknown)
        return Empty;

      int v = _IntValue;
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentOutOfRangeException();
      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        v &= ~(1 << i);

      return new FiasLevelSet(v);
    }

    /// <summary>
    /// Возвращает набор, который содержит уровни, начиная с Region и заканчивая заданным.
    /// Если <paramref name="level"/>=Unknown, возвращается Empty. В противном случае возвращается непустой набор.
    /// </summary>
    /// <param name="level">Нижний уровень</param>
    /// <param name="expand">Если true, то будет выполнена замена уровней House и Building на Structure,
    /// а Flat - на Room</param>
    /// <returns>Набор уровней</returns>
    public static FiasLevelSet FromBottomLevel(FiasLevel level, bool expand)
    {
      if (level == FiasLevel.Unknown)
        return Empty;

      if (expand)
        level = FiasTools.ReplaceToStructureOrRoom(level);

      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentOutOfRangeException("level");

      int v = 0;
      for (int i = 0; i <= p; i++)
        v |= (1 << i);
      return new FiasLevelSet(v);
    }

    /// <summary>
    /// Возвращает набор, который содержит уровни, начиная с заданного и заканчивая уровнем Room.
    /// Если <paramref name="level"/>=Unknown, возвращается Empty. В противном случае возвращается непустой набор.
    /// </summary>
    /// <param name="level">Нижний уровень</param>
    /// <param name="expand">Если true, то будет выполнена замена уровней Building и Structure на House,
    /// а Room - на Flat</param>
    /// <returns>Набор уровней</returns>
    public static FiasLevelSet FromTopLevel(FiasLevel level, bool expand)
    {
      if (level == FiasLevel.Unknown)
        return Empty;

      if (expand)
        level = FiasTools.ReplaceToHouseOrFlat(level);

      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentOutOfRangeException("level");

      int v = 0;
      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        v |= (1 << i);
      return new FiasLevelSet(v);
    }


    /// <summary>
    /// Возвращает набор уровней, начиная с Region и заканчивая нижним уровнем, определенным редактором
    /// </summary>
    /// <param name="editorLevel">Уровень редактора</param>
    /// <returns>Набор уровней</returns>
    public static FiasLevelSet FromEditorLevel(FiasEditorLevel editorLevel)
    {
      switch (editorLevel)
      {
        case FiasEditorLevel.Village:
          return FromBottomLevel(FiasLevel.Village, false);
        case FiasEditorLevel.Street:
          return FromBottomLevel(FiasLevel.Street, false);
        case FiasEditorLevel.House:
          return FromBottomLevel(FiasLevel.Structure, false);
        case FiasEditorLevel.Room:
          return FromBottomLevel(FiasLevel.Room, false);
        default:
          throw new ArgumentOutOfRangeException("editorLevel");
      }
    }


    #endregion

    #region Статические значения

    /// <summary>
    /// Пустой набор
    /// </summary>
    public static readonly FiasLevelSet Empty = new FiasLevelSet(0);

    /// <summary>
    /// Набор, содержащий все уровни из FiasTools.AllLevels
    /// </summary>
    public static readonly FiasLevelSet AllLevels = new FiasLevelSet(AllBits);

    /// <summary>
    /// Набор, содержащий все уровни уровни адресных объектов до уровня улицы включительно
    /// </summary>
    public static readonly FiasLevelSet AOLevels = new FiasLevelSet(AllAOBits);

    /// <summary>
    /// Набор, содержащий уровни домов (House, Building и Structure)
    /// </summary>
    public static readonly FiasLevelSet HouseLevels = new FiasLevelSet(AllHouseBits);

    /// <summary>
    /// Набор, содержащий уровни помещений (Flat и Room)
    /// </summary>
    public static readonly FiasLevelSet RoomLevels = new FiasLevelSet(AllRoomBits);

    /// <summary>
    /// Возвращает набор из одного заданного уровня.
    /// Если запрошенный уровень не относится к FiasTools.AllLevels (например, FiasLevel.Unknown), возвращается пустой набор Empty
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <returns>Набор из одного уровня</returns>
    public static FiasLevelSet FromLevel(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p >= 0)
        return new FiasLevelSet(1 << p);
      else
        return Empty;
    }

    /// <summary>
    /// Уровни для административно-территориального деления (как в редакторе адреса
    /// </summary>
    public static readonly FiasLevelSet ATDivLevels = new FiasLevelSet(RegionBit | DistrictBit | CityBit | VillageBit | PlanningStructureBit | StreetBit | AllHouseBits | AllRoomBits);

    #endregion

    #region IEnumerable<FiasLevel> Members

    /// <summary>
    /// Перечислитель для уровней в наборе
    /// </summary>
    public struct Enumerator : IEnumerator<FiasLevel>
    {
      #region Защищенный конструктор

      internal Enumerator(FiasLevelSet owner)
      {
        _IntValue = owner._IntValue;
        _Index = -1;
      }

      #endregion

      #region Поля

      int _IntValue;

      int _Index;

      #endregion

      #region IEnumerator<FiasLevel> Members

      /// <summary>
      /// Текущий уровень при перечислении
      /// </summary>
      public FiasLevel Current
      {
        get { return FiasTools.AllLevels[_Index]; }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return FiasTools.AllLevels[_Index]; }
      }

      /// <summary>
      /// Переход к следующему уровню в наборе
      /// </summary>
      /// <returns>true, если есть очередной уровень</returns>
      public bool MoveNext()
      {
        while (_Index < FiasTools.AllLevels.Length)
        {
          _Index++;
          if ((_IntValue & (1 << _Index)) != 0)
            return true;
        }
        return false;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель для уровней в наборе
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<FiasLevel> IEnumerable<FiasLevel>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region IEquatable<FiasLevelSet> Members

    /// <summary>
    /// Возвращает true, если два набора равны
    /// </summary>
    /// <param name="other">Второй набор</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(FiasLevelSet other)
    {
      return this._IntValue == other._IntValue;
    }

    /// <summary>
    /// Возвращает true, если два набора равны
    /// </summary>
    /// <param name="other">Второй набор</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is FiasLevelSet)
        return ((FiasLevelSet)other)._IntValue == this._IntValue;
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если два набора совпадают.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй уровень</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(FiasLevelSet a, FiasLevelSet b)
    {
      return a._IntValue == b._IntValue;
    }

    /// <summary>
    /// Возвращает true, если два набора различаются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй уровень</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(FiasLevelSet a, FiasLevelSet b)
    {
      return a._IntValue != b._IntValue;
    }

    /// <summary>
    /// Возвращает хэш-код для коллекций
    /// </summary>
    /// <returns>Хэш</returns>
    public override int GetHashCode()
    {
      return _IntValue;
    }

    #endregion
  }

  /// <summary>
  /// Преобразование набора уровней в строку вида "Region,District,Town"...
  /// </summary>
  public static class FiasLevelSetConvert
  {
    #region Методы

    /// <summary>
    /// Преобразует набор уровней в строку.
    /// Пустой набор преобразуется в пустую строку
    /// </summary>
    /// <param name="value">Набор</param>
    /// <returns>Строка</returns>
    public static string ToString(FiasLevelSet value)
    {
      if (value.IsEmpty)
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      int cnt = 0;
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if (value[FiasTools.AllLevels[i]])
        {
          if (cnt > 0)
            sb.Append(',');
          sb.Append(FiasTools.AllLevels[i].ToString());
          cnt++;
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Преобразует строку в набор уровней.
    /// В случае ошибки генерируется InvalidCastException.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Набор уровней</returns>
    public static FiasLevelSet Parse(string s)
    {
      FiasLevelSet value;
      if (TryParse(s, out value))
        return value;
      else
        throw new InvalidCastException("Строку \"" + s + "\" нельзя преобразовать в набор уровней адресов ФИАС");
    }

    /// <summary>
    /// Преобразует строку в набор уровней.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="value">Сюда записывается результат преобразования</param>
    /// <returns>true, если преобразование выполнено успешно</returns>
    public static bool TryParse(string s, out FiasLevelSet value)
    {
      value = FiasLevelSet.Empty;
      if (String.IsNullOrEmpty(s))
        return true;

      string[] a = s.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        FiasLevel level;
        if (StdConvert.TryParseEnum(a[i], out level))
          value |= level;
        else
          return false;
      }

      return true;
    }

    #endregion
  }
}
