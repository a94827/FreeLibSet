// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  #region FiasLevel

  /// <summary>
  /// Значения поля AOLEVEL (Уровень адресного объекта)
  /// </summary>
  [Serializable]
  public enum FiasLevel
  {
    #region Значения, определенные в ФИАС

    /// <summary>
    /// Уровень региона
    /// </summary>
    Region = 1,

    /// <summary>
    /// уровень автономного округа (устаревшее)
    /// </summary>
    AutonomousArea = 2,

    /// <summary>
    /// уровень района
    /// </summary>
    District = 3,

    /// <summary>
    /// уровень городских и сельских поселений
    /// </summary>
    Settlement = 35,

    /// <summary>
    /// уровень города
    /// </summary>
    City = 4,

    /// <summary>
    /// уровень внутригородской территории (устаревшее)
    /// </summary>
    InnerCityArea = 5,

    /// <summary>
    /// уровень населенного пункта
    /// </summary>
    Village = 6,

    /// <summary>
    /// планировочная структура
    /// </summary>
    PlanningStructure = 65,

    /// <summary>
    /// уровень улицы
    /// </summary>
    Street = 7,

    /// <summary>
    /// земельный участок
    /// </summary>
    LandPlot = 75,

    /// <summary>
    /// здания, сооружения, объекта незавершенного строительства
    /// </summary>
    House = 8,

    /// <summary>
    ///  уровень помещения в пределах здания, сооружения
    /// </summary>
    Flat = 9,

    /// <summary>
    /// уровень дополнительных территорий (устаревшее)
    /// </summary>
    AdditionalTerritory = 90,

    /// <summary>
    /// уровень объектов на дополнительных территориях (устаревшее)
    /// </summary>
    AdditionalTerritoryObject = 91,

    #endregion

    #region Дополнительные значения

    /// <summary>
    /// Корпус.
    /// Это значение не определено в ФИАС
    /// </summary>
    Building = 201,

    /// <summary>
    /// Строение.
    /// Это значение не определено в ФИАС
    /// </summary>
    Structure = 202,

    /// <summary>
    /// Комната
    /// Это значение не определено в ФИАС
    /// </summary>
    Room = 203,

    /// <summary>
    /// Уровень не задан
    /// </summary>
    Unknown = 0,

    #endregion
  }

  #endregion

  #region FiasCenterStatus

  /// <summary>
  /// Является ли населенный пункт столицей района и/или региона?
  /// Значение поля AddressObjects.CENTSTATUS
  /// </summary>
  [Serializable]
  public enum FiasCenterStatus
  {
    /// <summary>
    /// объект не является центром административно-территориального образования
    /// </summary>
    None = 0,

    /// <summary>
    /// объект является центром района
    /// </summary>
    District = 1,

    /// <summary>
    /// объект является центром (столицей) региона
    /// </summary>
    Region = 2,

    /// <summary>
    /// объект является одновременно и центром района и центром региона.
    /// </summary>
    RegionAndDistrict = 3
  }

  #endregion

  #region FiasDivType

  /// <summary>
  /// Признак адресации
  /// </summary>
  [Serializable]
  public enum FiasDivType
  {
    /// <summary>
    /// не определено
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// муниципальное деление
    /// </summary>
    MD = 1,

    /// <summary>
    /// административно-территориальное деление
    /// </summary>
    ATD = 2,
  }

  #endregion

  #region FiasStructureStatus

  /// <summary>
  /// Признак строения.
  /// Значение поля "STRSTATUS"
  /// </summary>
  [Serializable]
  public enum FiasStructureStatus
  {
    /// <summary>
    /// Не определено
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Строение
    /// </summary>
    Structure = 1,

    /// <summary>
    /// Сооружение
    /// </summary>
    Construction = 2,

    /// <summary>
    /// Литер
    /// </summary>
    Character = 3,
  }

  #endregion

  #region FiasEstateStatus

  /// <summary>
  /// Признак владения (строением).
  /// Значение поля ESTSTATUS
  /// </summary>
  [Serializable]
  public enum FiasEstateStatus
  {
    /// <summary>
    /// Не определено
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Владение
    /// </summary>
    Hold = 1,

    /// <summary>
    /// Дом
    /// </summary>
    House = 2,

    /// <summary>
    /// Домовладение
    /// </summary>
    Household = 3,

    /// <summary>
    /// Гараж
    /// </summary>
    Garage = 4,

    /// <summary>
    /// Здание
    /// </summary>
    Bulding = 5,

    /// <summary>
    /// Шахта
    /// </summary>
    Mine = 6,

    /// <summary>
    /// Подвал
    /// </summary>
    Basement = 7,

    /// <summary>
    /// Котельная
    /// </summary>
    Steamshop = 8,

    /// <summary>
    /// Погреб
    /// </summary>
    Cellar = 9,

    /// <summary>
    /// Объект незавершенного строительства
    /// </summary>
    UnderConstruction = 10,
  }

  #endregion

  #region FiasFlatType

  /// <summary>
  /// Тип помещения.
  /// Значения поля FLATTYPE
  /// </summary>
  [Serializable]
  public enum FiasFlatType
  {
    /// <summary>
    /// Не определено
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Помещение
    /// </summary>
    Space = 1,

    /// <summary>
    /// Квартира
    /// </summary>
    Flat = 2,

    /// <summary>
    /// Офис,
    /// </summary>
    Office = 3,

    /// <summary>
    /// Комната
    /// </summary>
    Room = 4,

    /// <summary>
    /// Рабочий участок
    /// </summary>
    WorkingSection = 5,

    /// <summary>
    /// Склад
    /// </summary>
    Storage = 6,

    /// <summary>
    /// Торговый зал
    /// </summary>
    SalesArea = 7,

    /// <summary>
    /// Цех
    /// </summary>
    Workshop = 8,

    /// <summary>
    /// Павильон
    /// </summary>
    Pavilion = 9,

    /// <summary>
    /// Подвал
    /// </summary>
    Basement = 10,

    /// <summary>
    /// Котельная
    /// </summary>
    Steamshop = 11,

    /// <summary>
    /// Погреб
    /// </summary>
    Cellar = 12,

    /// <summary>
    /// Гараж
    /// </summary>
    Garage = 13,
  };

  #endregion

  #region FiasRoomType

  /// <summary>
  /// Тип комнаты.
  /// Значения поля ROOMTYPE
  /// </summary>
  [Serializable]
  public enum FiasRoomType
  {
    /// <summary>
    /// Не определено
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Комната
    /// </summary>
    Room = 1,

    /// <summary>
    /// Помещение
    /// </summary>
    Space = 2
  }

  #endregion

  #region FiasActuality

  /// <summary>
  /// Актульность адреса
  /// </summary>
  [Serializable]
  public enum FiasActuality
  {
    /// <summary>
    /// Не определена
    /// </summary>
    Unknown,

    /// <summary>
    /// Актуальный адрес
    /// </summary>
    Actual,

    /// <summary>
    /// Исторические сведения
    /// </summary>
    Historical,
  }

  #endregion

  #region FiasTableType

  /// <summary>
  /// Таблица в справочнике ФИАС
  /// </summary>
  [Serializable]
  public enum FiasTableType
  {
    /// <summary>
    /// Не определена
    /// </summary>
    Unknown,

    /// <summary>
    /// Адресные объекты
    /// </summary>
    AddrOb,

    /// <summary>
    /// Здания и сооружения
    /// </summary>
    House,

    /// <summary>
    /// Помещения
    /// </summary>
    Room
  }

  #endregion

  #region FiasAOTypeMode

  /// <summary>
  /// Вариант типа адресообразующего элемента
  /// </summary>
  [Serializable]
  public enum FiasAOTypeMode
  {
    /// <summary>
    /// Полное наименование типа элемента ("улица"). Соответствует полю "SOCRNAME" в таблице "SOCRBASE"
    /// </summary>
    Full,

    /// <summary>
    /// Сокращенное наименование типа элемента ("ул."). Соответствует полю "SCNAME" в таблице "SOCRBASE"
    /// </summary>
    Abbreviation
  }

  #endregion

  #region FiasEditorLevel

  /// <summary>
  /// Уровень, до которого можно редактировать адрес в редакторе
  /// </summary>
  [Serializable]
  public enum FiasEditorLevel
  {
    /// <summary>
    /// До населенного пункта
    /// </summary>
    Village,

    /// <summary>
    /// До улицы
    /// </summary>
    Street,

    /// <summary>
    /// До здания
    /// </summary>
    House,

    /// <summary>
    /// До помещения
    /// </summary>
    Room
  }

  #endregion

  #region FiasLevelCompareResult

  /// <summary>
  /// Результаты сравнения уровней адреса.
  /// Содержит значения 0, 1 и (-1), совместимые с возвращаемыми значениями стандартных методов Compare() в Net Framework
  /// </summary>
  [Serializable]
  public enum FiasLevelCompareResult
  {
    /// <summary>
    /// Уровни совпадают
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Первый сравниваемый уровень является более детальным, чем второй
    /// </summary>
    Greater = +1,

    /// <summary>
    /// Первый сравниваемый уровень является менее детальным, чем второй
    /// </summary>
    Less = -1,
  }

  #endregion

  /// <summary>
  /// Наименования для перечислений, используемых ФИАС
  /// </summary>
  public static class FiasEnumNames
  {
    /// <summary>
    /// Символы, удаляемые из сокращений при дополнительном распознавании
    /// </summary>
    private static readonly CharArrayIndexer _AbbrRemovedCharIndexer = new CharArrayIndexer(" .-/");

    #region FiasLevel

    private static Dictionary<FiasLevel, string> _LevelNames1 = CreateLevelNames1();
    private static Dictionary<FiasLevel, string> CreateLevelNames1()
    {
      Dictionary<FiasLevel, string> d = new Dictionary<FiasLevel, string>();
      d.Add(FiasLevel.Region, "Регион");
      d.Add(FiasLevel.AutonomousArea, "АО");
      d.Add(FiasLevel.District, "Район");
      d.Add(FiasLevel.Settlement, "Г/С пос.");
      d.Add(FiasLevel.City, "Город");
      d.Add(FiasLevel.InnerCityArea, "Внутригор.т.");
      d.Add(FiasLevel.Village, "Нас. пункт");
      d.Add(FiasLevel.PlanningStructure, "План.стр");
      d.Add(FiasLevel.Street, "Улица");
      d.Add(FiasLevel.LandPlot, "Участок");
      d.Add(FiasLevel.AdditionalTerritory, "Доп.тер.");
      d.Add(FiasLevel.AdditionalTerritoryObject, "Доп.тер.об.");
      d.Add(FiasLevel.House, "Дом");
      d.Add(FiasLevel.Building, "Корпус");
      d.Add(FiasLevel.Structure, "Строение");
      d.Add(FiasLevel.Flat, "Квартира");
      d.Add(FiasLevel.Room, "Комната");
      return d;
    }

    private static Dictionary<FiasLevel, string> _LevelNames2 = CreateLevelNames2();
    private static Dictionary<FiasLevel, string> CreateLevelNames2()
    {
      Dictionary<FiasLevel, string> d = new Dictionary<FiasLevel, string>();
      d.Add(FiasLevel.Region, "Субъект Российской Федерации");
      d.Add(FiasLevel.AutonomousArea, "Автономный округ (устаревшее)");
      d.Add(FiasLevel.District, "Район");
      d.Add(FiasLevel.Settlement, "Городское или сельское поселение");
      d.Add(FiasLevel.City, "Город");
      d.Add(FiasLevel.InnerCityArea, "Внутригородская территория (устаревшее)");
      d.Add(FiasLevel.Village, "Населенный пункт");
      d.Add(FiasLevel.PlanningStructure, "Планировочная структура");
      d.Add(FiasLevel.Street, "Улица");
      d.Add(FiasLevel.LandPlot, "Земельный участок");
      d.Add(FiasLevel.AdditionalTerritory, "Дополнительная территория (устаревшее)");
      d.Add(FiasLevel.AdditionalTerritoryObject, "Объект на дополнительной территории (устаревшее)");
      d.Add(FiasLevel.House, "Здание, сооружение, объект незавершенного строительства");
      d.Add(FiasLevel.Building, "Корпус");
      d.Add(FiasLevel.Structure, "Строение");
      d.Add(FiasLevel.Flat, "Помещение в пределах здания, сооружения");
      d.Add(FiasLevel.Room, "Комната");
      return d;
    }

    /// <summary>
    /// Получить текстовое представление для уровня адресного объекта
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="isLong">true - длинное представление ("Населенный пункт"),
    /// false - краткое представление ("Нас. пункт")</param>
    /// <returns>Текст</returns>
    public static string ToString(FiasLevel level, bool isLong)
    {
      if (level == FiasLevel.Unknown)
        return "Не задан"; // 29.04.2020
      Dictionary<FiasLevel, string> d = isLong ? _LevelNames2 : _LevelNames1;
      string s;
      if (d.TryGetValue(level, out s))
        return s;
      else
        return level.ToString();
    }

    #endregion

    #region FiasCenterStatus

    /// <summary>
    /// Является ли населенный пункт столицей района и/или региона?
    /// Значения поля AddressObjects.CENTSTATUS
    /// </summary>
    public static readonly string[] CenterStatusNames = new string[] { 
      "Нет",
      "Районный центр",
      "Центр (столица) региона",
      "Центр региона и района"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasCenterStatus.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasCenterStatus value)
    {
      return GetName((int)value, CenterStatusNames);
    }

    private static string GetName(int value, string[] names)
    {
      if (value < 0 || value >= names.Length)
        return "?? " + value.ToString();
      else
        return names[value];
    }

    #endregion

    #region FiasDivType

    /// <summary>
    /// Признаки адресации
    /// </summary>
    public static readonly string[] DivTypeNames = new string[] { 
      "Не определено",
      "Муниципальное деление",
      "Административно-территориальное деление"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasDivType.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasDivType value)
    {
      return GetName((int)value, DivTypeNames);
    }

    #endregion

    #region FiasEstateStatus (тип строения)

    /// <summary>
    /// Признак владения (строением).
    /// Значения поля ESTSTATUS
    /// </summary>
    public static readonly string[] EstateStatusAOTypes = new string[] { 
     "Не определено",
     "Владение",
     "Дом",
     "Домовладение",
     "Гараж",
     "Здание",
     "Шахта",
     "Подвал", // ФИАС от 23.03.2020
     "Котельная", // ФИАС от 23.03.2020
     "Погреб", // ФИАС от 23.03.2020
     "Объект нез-го стр-ва" // ФИАС от 23.03.2020
    };

    private static readonly StringArrayIndexer _EstateStatusAOTypeIndexer = new StringArrayIndexer(EstateStatusAOTypes, true);


    /// <summary>
    /// Признак владения (строением). Сокращения
    /// Значения поля ESTSTATUS
    /// </summary>
    public static readonly string[] EstateStatusAbbrs = new string[] { 
     String.Empty,
     "влд.",
     "д.",
     "двлд.",
     "г-ж",
     "зд.",
     "шахта",
     "подв.", 
     "кот.", 
     "пб.", 
     "онс"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasEstateStatus.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasEstateStatus value)
    {
      return GetName((int)value, EstateStatusAOTypes);
    }

    /// <summary>
    /// Преобразует строку сокращения, например, "дом" в перечисление FiasEstateStatus.
    /// Регистр не учитывается.
    /// Дополнительно обрабатываются сокращения, например, "д".
    /// Если задана пустая строка, возвращается значение Unknown.
    /// Если задана неизвестная строка, возвращается значение Unknown. Исключение не выбрасывается.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Значение перечисления</returns>
    public static FiasEstateStatus ParseEstateStatus(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasEstateStatus.Unknown;

      int p = _EstateStatusAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasEstateStatus)p;

      // При добавлении не забыть про метод FiasCachedAOTypes.CreateAOTypeLevelDict()

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "д": return FiasEstateStatus.House;
        case "влд": return FiasEstateStatus.Hold;
        case "дв":
        case "двл":
        case "двлд":
        case "двлад": return FiasEstateStatus.Hold;
        case "гар":
        case "г-ж":
        case "гж": return FiasEstateStatus.Garage;
        case "зд": return FiasEstateStatus.Bulding;
        case "подв": return FiasEstateStatus.Basement;
        case "кот": return FiasEstateStatus.Steamshop;
        case "пб": return FiasEstateStatus.Cellar;
        case "онс": return FiasEstateStatus.UnderConstruction;
      }

      return FiasEstateStatus.Unknown;
    }

    #endregion

    #region FiasStructureStatus (тип строения)

    /// <summary>
    /// Признак строения.
    /// Значения поля "STRSTATUS"
    /// </summary>
    public static readonly string[] StructureStatusAOTypes = new string[] { 
      "Не определено",
      "Строение",
      "Сооружение",
      "Литер"};

    private static readonly StringArrayIndexer _StructureStatusAOTypeIndexer = new StringArrayIndexer(StructureStatusAOTypes, true);

    /// <summary>
    /// Признак строения. Сокращение
    /// Значения поля "STRSTATUS"
    /// </summary>
    public static readonly string[] StructureStatusAbbrs = new string[] { 
      String.Empty,
      "стр.",
      "cоор.",
      "л."};


    /// <summary>
    /// Получить текстовое представление для перечисления FiasStructureStatus.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasStructureStatus value)
    {
      return GetName((int)value, StructureStatusAOTypes);
    }

    /// <summary>
    /// Преобразует строку сокращения, например, "строение" в перечисление FiasStructureStatus.
    /// Регистр не учитывается.
    /// Дополнительно обрабатываются сокращения, например, "стр".
    /// Если задана пустая строка, возвращается значение Unknown.
    /// Если задана неизвестная строка, возвращается значение Unknown. Исключение не выбрасывается.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Значение перечисления</returns>
    public static FiasStructureStatus ParseStructureStatus(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasStructureStatus.Unknown;

      int p = _StructureStatusAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasStructureStatus)p;

      // При добавлении не забыть про метод FiasCachedAOTypes.CreateAOTypeLevelDict()

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "стр":
        case "ст": return FiasStructureStatus.Structure;
        case "сооруж":
        case "соор": return FiasStructureStatus.Construction;
        case "лит":
        case "л": return FiasStructureStatus.Character;
      }

      return FiasStructureStatus.Unknown;
    }

    #endregion

    #region FiasFlatType

    /// <summary>
    /// Тип помещения.
    /// Значения поля FLATTYPE
    /// </summary>
    public static readonly string[] FlatTypeAOTypes = new string[] { 
      "Не определено",
      "Помещение",
      "Квартира",
      "Офис",
      "Комната",
      "Рабочий участок",
      "Склад",
      "Торговый зал",
      "Цех",
      "Павильон",
      "Подвал", // ФИАС от 23.03.2020
      "Котельная", // ФИАС от 23.03.2020
      "Погреб", // ФИАС от 23.03.2020
      "Гараж" // ФИАС от 23.03.2020
    };

    private static readonly StringArrayIndexer _FlatTypeAOTypeIndexer = new StringArrayIndexer(FlatTypeAOTypes, true);

    /// <summary>
    /// Тип помещения. Сокращения
    /// Значения поля FLATTYPE
    /// </summary>
    public static readonly string[] FlatTypeAbbrs = new string[] { 
      String.Empty,
      "пом.",
      "кв.",
      "оф.",
      "ком.",
      "раб.уч.",
      "скл.",
      "торг.зал",
      "цех",
      "пав.",
      "подв.", // ФИАС от 23.03.2020
      "кот.", // ФИАС от 23.03.2020
      "п-б", // ФИАС от 23.03.2020
      "г-ж" // ФИАС от 23.03.2020
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasFlatType.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasFlatType value)
    {
      return GetName((int)value, FlatTypeAOTypes);
    }

    /// <summary>
    /// Преобразует строку сокращения, например, "квартира" в перечисление FiasFlatType.
    /// Регистр не учитывается.
    /// Дополнительно обрабатываются сокращения, например, "кв".
    /// Если задана пустая строка, возвращается значение Unknown.
    /// Если задана неизвестная строка, возвращается значение Unknown. Исключение не выбрасывается.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Значение перечисления</returns>
    public static FiasFlatType ParseFlatType(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasFlatType.Unknown;

      int p = _FlatTypeAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasFlatType)p;

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "пом": return FiasFlatType.Space;
        case "кв": return FiasFlatType.Flat;
        case "оф": return FiasFlatType.Office;
        case "ком":
        case "комн": return FiasFlatType.Room;
        case "скл": return FiasFlatType.Storage;
        case "тз":
        case "тзал":
        case "торгзал": return FiasFlatType.SalesArea;
        case "пав": return FiasFlatType.Pavilion;
        case "подв": return FiasFlatType.Basement;
        case "кот": return FiasFlatType.Steamshop;
        case "пб": return FiasFlatType.Cellar;
        case "гар":
        case "гж": return FiasFlatType.Garage;
      }

      return FiasFlatType.Unknown;
    }

    #endregion

    #region FiasRoomType

    /// <summary>
    /// Тип комнаты.
    /// Значения поля ROOMTYPE
    /// </summary>
    public static readonly string[] RoomTypeAOTypes = new string[] { 
      "Не определено",
      "Комната",
      "Помещение"
    };

    private static readonly StringArrayIndexer _RoomTypeAOTypeIndexer = new StringArrayIndexer(RoomTypeAOTypes, true);

    /// <summary>
    /// Тип комнаты. Сокращение
    /// Значения поля ROOMTYPE
    /// </summary>
    public static readonly string[] RoomTypeAbbrs = new string[] { 
      String.Empty,
      "ком.",
      "пом."
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasRoomType.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasRoomType value)
    {
      return GetName((int)value, RoomTypeAOTypes);
    }

    /// <summary>
    /// Преобразует строку сокращения, например, "комната" в перечисление FiasRoomType.
    /// Регистр не учитывается.
    /// Дополнительно обрабатываются сокращения, например, "ком".
    /// Если задана пустая строка, возвращается значение Unknown.
    /// Если задана неизвестная строка, возвращается значение Unknown. Исключение не выбрасывается.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Значение перечисления</returns>
    public static FiasRoomType ParseRoomType(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasRoomType.Unknown;

      int p = _RoomTypeAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasRoomType)p;

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "ком":
        case "комн": return FiasRoomType.Room;
        case "пом": return FiasRoomType.Space;
      }

      return FiasRoomType.Unknown;
    }

    #endregion

    #region FiasActuality

    /// <summary>
    /// Актульность адреса
    /// </summary>
    public static readonly string[] ActualityNames = new string[] { 
      "Не определено",
      "Актуальный",
      "Исторический"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasActuality.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasActuality value)
    {
      return GetName((int)value, ActualityNames);
    }

    #endregion

    #region FiasTableType

    /// <summary>
    /// Текстовые значения, соответствующие перечислению FiasTableType, во множественном числе
    /// </summary>
    public static readonly string[] TableTypeNamesPlural = new string[] { 
      "Неизвестно",
      "Адресные объекты",
      "Здания",
      "Помещения"
    };

    /// <summary>
    /// Текстовые значения, соответствующие перечислению FiasTableType, в единственном числе
    /// </summary>
    public static readonly string[] TableTypeNamesSingular = new string[] { 
      "Неизвестно",
      "Адресный объект",
      "Здание",
      "Помещение"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasTableType.
    /// Если передано недопустимое значение <paramref name="value"/>, возвращается числовое значение со знаками "??" 
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="plural">true - во множественном числе, false - в единственном</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasTableType value, bool plural)
    {
      return GetName((int)value, plural ? TableTypeNamesPlural : TableTypeNamesSingular);
    }

    #endregion

    #region FiasTableType

    /// <summary>
    /// Названия для перечисления FiasTableType
    /// </summary>
    public static readonly string[] FiasTableTypeNames = new string[] { 
      "Не определено",
      "Адресные объекты",
      "Здания",
      "Помещения"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasTableType
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasTableType value)
    {
      return GetName((int)value, FiasTableTypeNames);
    }

    #endregion

    #region FiasEditorLebel

    /// <summary>
    /// Названия для перечисления FiasEditorLevel
    /// </summary>
    public static readonly string[] FiasEditorLevelNames = new string[] { 
      "Населенный пункт",
      "Улица",
      "Здание",
      "Помещение"
    };

    /// <summary>
    /// Получить текстовое представление для перечисления FiasEditorLevel
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(FiasEditorLevel value)
    {
      return GetName((int)value, FiasEditorLevelNames);
    }

    #endregion
  }
}
