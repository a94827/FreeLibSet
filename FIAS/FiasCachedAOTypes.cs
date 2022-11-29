// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{

  /// <summary>
  /// Буферизованная таблица типов адресных объектов
  /// Этот класс не используется в пользовательском коде
  /// </summary>
  [Serializable]
  public sealed class FiasCachedAOTypes
  {
    #region Защищенный конструктор

    internal FiasCachedAOTypes(DataSet ds)
    {
      _DS = ds;

      OnDeserializedMethod(new System.Runtime.Serialization.StreamingContext());
    }

    #endregion

    #region Поля

    /// <summary>
    /// Внутренняя таблица данных.
    /// Содержит поля "Id", "Level", "SOCRNAME", "SCNAME".
    /// </summary>
    private readonly DataSet _DS;

    /// <summary>
    /// Количество строк в таблице на странице (для отладочных целей)
    /// </summary>
    public int RowCount
    {
      get
      {
        lock (_DS)
        {
          return _DS.Tables[0].Rows.Count;
        }
      }

    }

    #endregion

    #region Сериализация

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserializedMethod(System.Runtime.Serialization.StreamingContext context)
    {
      DataTools.SetPrimaryKey(_DS.Tables[0], "Id");
    }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает тип адресного объекта по идентификатору.
    /// Этот метод пригоден только для таблицы адресных объектов до уровня улицы включительно, но не для зданий и помещений.
    /// </summary>
    /// <param name="id">Идентификатор типа адресного объекта</param>
    /// <param name="typeMode">Полное наименование типа или сокращение</param>
    /// <returns></returns>
    public string GetAOType(Int32 id, FiasAOTypeMode typeMode)
    {
      if (id == 0)
        return String.Empty;

      lock (_DS)
      {
        DataRow row = _DS.Tables[0].Rows.Find(id);
        if (row == null)
          return "?? " + id.ToString();
        else
          return DataTools.GetString(row, typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME");
      }
    }

    #region Специальные таблицы имен

    [NonSerialized]
    private string[] _EstateStatusAOTypes;

    [NonSerialized]
    private string[] _EstateStatusAbbrs;


    [NonSerialized]
    private string[] _StructureStatusAOTypes;

    [NonSerialized]
    private string[] _StructureStatusAbbrs;


    [NonSerialized]
    private string[] _FlatTypeAOTypes;

    [NonSerialized]
    private string[] _FlatTypeAbbrs;


    [NonSerialized]
    private string[] _RoomTypeAOTypes;

    [NonSerialized]
    private string[] _RoomTypeAbbrs;


    private string[] PrepareSpecialName(ref string[] ourNames, string[] source)
    {
      if (ourNames != null)
        return ourNames;

      // Берем весь список, кроме элемента с индексом 0
      string[] a = new string[source.Length - 1];
      Array.Copy(source, 1, a, 0, a.Length);
      ourNames = a;
      return a;
    }


    #endregion

    /// <summary>
    /// Получить список доступных типов адресообразующих элементов или сокращений для заданного уровня
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="typeMode">Тип или сокращение</param>
    /// <returns>Массив имен</returns>
    public string[] GetAOTypes(FiasLevel level, FiasAOTypeMode typeMode)
    {
      switch (level)
      {
        case FiasLevel.House:
          // 10.03.2021
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _EstateStatusAOTypes, FiasEnumNames.EstateStatusAOTypes);
          else
            return PrepareSpecialName(ref _EstateStatusAbbrs, FiasEnumNames.EstateStatusAbbrs);
        case FiasLevel.Building:
          return new string[1] { typeMode == FiasAOTypeMode.Full ? "Корпус" : "корп." };
        case FiasLevel.Structure:
          //return new string[1] { isLong ? "Строение" : "стр." };
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _StructureStatusAOTypes, FiasEnumNames.StructureStatusAOTypes);
          else
            return PrepareSpecialName(ref _StructureStatusAbbrs, FiasEnumNames.StructureStatusAbbrs);
        case FiasLevel.Flat:
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _FlatTypeAOTypes, FiasEnumNames.FlatTypeAOTypes);
          else
            return PrepareSpecialName(ref _FlatTypeAbbrs, FiasEnumNames.FlatTypeAbbrs);
        case FiasLevel.Room:
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _RoomTypeAOTypes, FiasEnumNames.RoomTypeAOTypes);
          else
            return PrepareSpecialName(ref _RoomTypeAbbrs, FiasEnumNames.RoomTypeAbbrs);
      }

#if DEBUG
      CheckLevel(level, true);
#endif

      string[] a;
      lock (_DS)
      {
        DataView dv = _DS.Tables[0].DefaultView;
        dv.Sort = typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME";
        dv.RowFilter = "LEVEL=" + (int)level;

        // В таблице могут быть провторяющиеся сокращения
        SingleScopeList<string> lst = new SingleScopeList<string>(dv.Count);
        for (int i = 0; i < dv.Count; i++)
          lst.Add(dv[i].Row[typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME"].ToString());

        a = lst.ToArray();
      }
      return a;
    }

    private void CheckLevel(FiasLevel level, bool includeHouseAndFlat)
    {
      if (FiasTools.GetTableType(level) == FiasTableType.AddrOb)
        return;

      if (includeHouseAndFlat)
      {
        if (level == FiasLevel.House || level == FiasLevel.Flat)
          return;
      }

      throw new ArgumentException("Неправильный уровень " + level.ToString() + " для поиска в таблице сокращений. Допускаются только уровни, относящиеся к таблице адресных объектов", "level");
    }

    /// <summary>
    /// Поиск идентификатора в таблице SOCRBASE по длинному или краткому наименованию типа адресного объекта
    /// </summary>
    /// <param name="level">Уровень адресообразующего элемента</param>
    /// <param name="aoType">Искомый тип элемента</param>
    /// <returns>Найденный идентификатор или 0</returns>
    public Int32 FindAOTypeId(FiasLevel level, string aoType)
    {
      if (level != FiasLevel.House)
        CheckLevel(level, false);
      if (String.IsNullOrEmpty(aoType))
        return 0;

      Int32 aoTypeId;
      lock (_DS)
      {
        DataView dv = _DS.Tables[0].DefaultView;
        dv.RowFilter = "LEVEL=" + (int)level;
        dv.Sort = "SOCRNAME";
        int p = dv.Find(aoType);
        if (p < 0)
        {
          dv.Sort = "SCNAME";
          p = dv.Find(aoType);

          // Если задано сокращение с точкой, ищем сокращение без точки
          if (p < 0 &&
            aoType[aoType.Length - 1] == '.' &&
            aoType.Length > 1 &&
            aoType.IndexOf('-') < 0 /* без дефисов */)

            p = dv.Find(aoType.Substring(0, aoType.Length - 1));
        }
        if (p < 0)
          aoTypeId = 0;
        else
          aoTypeId = DataTools.GetInt(dv[p].Row, "Id");
      }

      return aoTypeId;
    }

    /// <summary>
    /// Возвращает true, если тип адресообразующего элемента или сокращение подходит для заданного уровня.
    /// Учитываются специальные имена.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="aoType">Проверяемое имя</param>
    /// <returns>Применимость</returns>
    public bool IsValidAOType(FiasLevel level, string aoType)
    {
      string longName;
      Int32 id;
      return IsValidAOType(level, aoType, out longName, out id);
    }

    /// <summary>
    /// Возвращает true, если тип адресообразующего элемента или сокращение подходит для заданного уровня.
    /// Учитываются специальные имена.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="aoType">Проверяемое имя</param>
    /// <param name="fullAOType">Сюда помещается правильный полный тип адресообразующего элемента.</param>
    /// <param name="id">Сюда помещается идентификатор в таблице "AOType".
    /// Может быть 0, если задано специальное имя</param>
    /// <returns>Применимость</returns>
    public bool IsValidAOType(FiasLevel level, string aoType, out string fullAOType, out Int32 id)
    {
      // При добавлении нестандартных сокращений не забыть про метод CreateAOTypeLevelDict()

      fullAOType = String.Empty;
      id = 0;

      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          CheckLevel(level, false);
          break;
      }
      if (String.IsNullOrEmpty(aoType))
        return false;

      // Специальные таблицы
      switch (level)
      {
        case FiasLevel.House: // дом

          // 27.10.2020
          // Для домов сначала выполняем стандартную проверку.
          id = FindAOTypeId(level, aoType);
          if (id != 0)
          {
            fullAOType = GetAOType(id, FiasAOTypeMode.Full);
            return true;
          }

          // Потом пытаемся проверить нестандартные сокращения, как было
          FiasEstateStatus EstStatus = FiasEnumNames.ParseEstateStatus(aoType);
          if (EstStatus == FiasEstateStatus.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(EstStatus);
            return true;
          }

        //break;
        case FiasLevel.Building: // корпус
          switch (aoType.ToLowerInvariant())
          {
            case "корпус":
            case "корп.":
            case "к.":
              fullAOType = "Корпус";
              return true;
            default:
              return false;
          }
        case FiasLevel.Structure: // строение
          FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(aoType);
          if (strStatus == FiasStructureStatus.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(strStatus);
            return true;
          }

        case FiasLevel.Flat:
          FiasFlatType flatType = FiasEnumNames.ParseFlatType(aoType);
          if (flatType == FiasFlatType.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(flatType);
            return true;
          }

        case FiasLevel.Room:
          FiasRoomType roomType = FiasEnumNames.ParseRoomType(aoType);
          if (roomType == FiasRoomType.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(roomType);
            return true;
          }

        // Дополнительные сокращения:
        case FiasLevel.Village:
          switch (aoType.ToLowerInvariant())
          {
            case "рп":
            case "р/п":
            case "р.п.":
              fullAOType = "Рабочий поселок";
              return true;
            case "пос.": // 25.11.2022
              fullAOType = "Поселок";
              return true;
          }
          break;
        case FiasLevel.PlanningStructure:
        case FiasLevel.Street:
          switch (aoType.ToLowerInvariant())
          {
            case "м-н": // 03.11.2020
              fullAOType = "Микрорайон";
              return true;
          }
          break;
      }

      // Стандартная проверка
      id = FindAOTypeId(level, aoType);

      if (id != 0)
        fullAOType = GetAOType(id, FiasAOTypeMode.Full);
      return id != 0;
    }

    /// <summary>
    /// Возвращает сокращение, соответствующее заданному типу адресообразующего элемента
    /// </summary>
    /// <param name="aoType">Тип адресообразующего элемента</param>
    /// <param name="level">Уровень</param>
    /// <returns>Сокращение</returns>
    public string GetAbbreviation(string aoType, FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House: // 01.09.2021
          FiasEstateStatus estStatus = FiasEnumNames.ParseEstateStatus(aoType);
          if (estStatus != FiasEstateStatus.Unknown)
            return FiasEnumNames.EstateStatusAbbrs[(int)estStatus];
          else
            break; // вдруг есть в справочнике
        case FiasLevel.Building:
          if (aoType == "Корпус")
            return "корп.";
          else
            return aoType;
        case FiasLevel.Structure:
          FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(aoType);
          if (strStatus == FiasStructureStatus.Unknown)
            return aoType;
          else
            return FiasEnumNames.StructureStatusAbbrs[(int)strStatus];
        case FiasLevel.Flat:
          FiasFlatType flatType = FiasEnumNames.ParseFlatType(aoType);
          if (flatType == FiasFlatType.Unknown)
            return aoType;
          else
            return FiasEnumNames.FlatTypeAbbrs[(int)flatType];
        case FiasLevel.Room:
          FiasRoomType roomType = FiasEnumNames.ParseRoomType(aoType);
          if (roomType == FiasRoomType.Unknown)
            return aoType;
          else
            return FiasEnumNames.RoomTypeAbbrs[(int)roomType];
      }

      // Общий случай. Используем справочник
      Int32 id = FindAOTypeId(level, aoType);
      if (id >= 0)
        return GetAOType(id, FiasAOTypeMode.Abbreviation);
      else
        return aoType; // если не нашли, обойдемся без сокращения
    }

    #endregion

    #region GetAOTypeLevels()

    /// <summary>
    /// Возвращает уровни, для которых подходит заданный тип адресообразующего элемента.
    /// Проверяются и сокращения и полные наименования.
    /// Если строка <paramref name="aoType"/> не является каким-либо типом, возвращается Empty
    /// </summary>
    /// <param name="aoType">Тип адресообразующего элемента</param>
    /// <returns>Уровни, для которых применим данный тип</returns>
    public FiasLevelSet GetAOTypeLevels(string aoType)
    {
      return GetAOTypeLevels(aoType, FiasAOTypeMode.Full) | GetAOTypeLevels(aoType, FiasAOTypeMode.Abbreviation);
    }

    /// <summary>
    /// Возвращает уровни, для которых подходит заданный тип адресообразующего элемента.
    /// Проверяются и сокращения и полные наименования.
    /// Если строка <paramref name="aoType"/> не является каким-либо типом, возвращается Empty
    /// </summary>
    /// <param name="aoType">Тип адресообразующего элемента. Регистр символов игнорируется</param>
    /// <param name="typeMode">Тип: полное наименование или сокращение</param>
    /// <returns>Уровни, для которых применим данный тип</returns>
    public FiasLevelSet GetAOTypeLevels(string aoType, FiasAOTypeMode typeMode)
    {
      if (String.IsNullOrEmpty(aoType))
        return FiasLevelSet.Empty;

      // Выполняем создание коллекций
      TypedStringDictionary<FiasLevelSet> dict = GetAOTypeLevelDict(typeMode);
      FiasLevelSet res;
      if (dict.TryGetValue(aoType, out res))
        return res;
      else
        return FiasLevelSet.Empty;
    }

    private volatile TypedStringDictionary<FiasLevelSet> _FullAOTypeLevels;

    private volatile TypedStringDictionary<FiasLevelSet> _AbbrAOTypeLevels;

    private TypedStringDictionary<FiasLevelSet> GetAOTypeLevelDict(FiasAOTypeMode typeMode)
    {
      switch (typeMode)
      {
        case FiasAOTypeMode.Full:
          if (_FullAOTypeLevels == null)
          {
            lock (_DS)
            {
              if (_FullAOTypeLevels == null)
                _FullAOTypeLevels = CreateAOTypeLevelDict(typeMode);
            }
          }
          return _FullAOTypeLevels;

        case FiasAOTypeMode.Abbreviation:
          if (_AbbrAOTypeLevels == null)
          {
            lock (_DS)
            {
              if (_AbbrAOTypeLevels == null)
                _AbbrAOTypeLevels = CreateAOTypeLevelDict(typeMode);
            }
          }
          return _AbbrAOTypeLevels;
        default:
          throw new ArgumentOutOfRangeException("typeMode");
      }
    }

    private TypedStringDictionary<FiasLevelSet> CreateAOTypeLevelDict(FiasAOTypeMode typeMode)
    {
      TypedStringDictionary<FiasLevelSet> dict = new TypedStringDictionary<FiasLevelSet>(true);

      #region Уровни из справочника

      foreach (DataRow row in _DS.Tables[0].Rows)
      {
        FiasLevel level = (FiasLevel)DataTools.GetInt(row, "Level");
        string s = DataTools.GetString(row, typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME");

        FiasLevelSet ls = FiasLevelSet.FromLevel(level);
        AddToAOTypeLevelDict(dict, s, ls);
      }

      #endregion

      #region Специальные таблицы

      #region Населенный пункт

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "рп", FiasLevelSet.FromLevel(FiasLevel.Village));
        AddToAOTypeLevelDict(dict, "р/п", FiasLevelSet.FromLevel(FiasLevel.Village));
        AddToAOTypeLevelDict(dict, "р.п.", FiasLevelSet.FromLevel(FiasLevel.Village));
        AddToAOTypeLevelDict(dict, "пос.", FiasLevelSet.FromLevel(FiasLevel.Village)); // 25.11.2022
      }

      #endregion

      #region Планировочная структура

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "м-н", FiasLevelSet.FromLevel(FiasLevel.PlanningStructure));
      }

      #endregion

      #region Улица

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "м-н", FiasLevelSet.FromLevel(FiasLevel.Street));
      }

      #endregion

      #region Дом

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.EstateStatusAOTypes.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.EstateStatusAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.House));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.EstateStatusAbbrs.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.EstateStatusAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.House));

        AddToAOTypeLevelDict(dict, "д.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "влд.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "дв.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "двл.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "двлд.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "двлад.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "гар.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "г-ж", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "гж", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "зд.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "подв.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "кот.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "пб", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "онс", FiasLevelSet.FromLevel(FiasLevel.House));
      }

      #endregion

      #region Корпус

      if (typeMode == FiasAOTypeMode.Full)
        AddToAOTypeLevelDict(dict, "Корпус", FiasLevelSet.FromLevel(FiasLevel.Building));
      else
      {
        AddToAOTypeLevelDict(dict, "корпус", FiasLevelSet.FromLevel(FiasLevel.Building));
        AddToAOTypeLevelDict(dict, "корп.", FiasLevelSet.FromLevel(FiasLevel.Building));
        AddToAOTypeLevelDict(dict, "к.", FiasLevelSet.FromLevel(FiasLevel.Building));
      }

      #endregion

      #region Строение

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.StructureStatusAOTypes.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.StructureStatusAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Structure));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.StructureStatusAbbrs.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.StructureStatusAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "ст.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "сооруж.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "соор.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "лит.", FiasLevelSet.FromLevel(FiasLevel.Structure));
      }

      #endregion

      #region Квартира

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.FlatTypeAOTypes.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.FlatTypeAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.FlatTypeAbbrs.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.FlatTypeAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }

      #endregion

      #region Комната

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.RoomTypeAOTypes.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.RoomTypeAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Room));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.RoomTypeAbbrs.Length; i++) // [0] - "Не определено"
          AddToAOTypeLevelDict(dict, FiasEnumNames.RoomTypeAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }

      #endregion

      #endregion

      return dict;
    }

    private static void AddToAOTypeLevelDict(TypedStringDictionary<FiasLevelSet> dict, string s, FiasLevelSet ls)
    {
      if (ls.IsEmpty)
        return;

      FiasLevelSet oldVal;
      if (dict.TryGetValue(s, out oldVal))
        dict[s] = oldVal | ls;
      else
        dict.Add(s, ls);
    }


    #endregion

    #region GetMaxSpaceCount()

    private volatile int[] _MaxSpaceCounts;

    /// <summary>
    /// Возвращает максимальное количество пробелов типе адресного объекта заданного уровня
    /// </summary>
    /// <param name="level">Уровень адресного объекта из списка FiasTools.AllLevels</param>
    /// <returns></returns>
    public int GetMaxSpaceCount(FiasLevel level)
    {
      if (_MaxSpaceCounts == null)
      {
        lock (_DS)
        {
          if (_MaxSpaceCounts == null)
          {
            int[] a = new int[FiasTools.AllLevels.Length];
            TypedStringDictionary<FiasLevelSet> dict = GetAOTypeLevelDict(FiasAOTypeMode.Full);
            foreach (KeyValuePair<string, FiasLevelSet> pair in dict)
            {
              int n = 0;
              for (int j = 0; j < pair.Key.Length; j++)
              {
                if (pair.Key[j] == ' ')
                  n++;
              }
              if (n == 0)
                continue;

              foreach (FiasLevel lvl in pair.Value)
              {
                int pLvl = FiasTools.AllLevelIndexer.IndexOf(lvl);
                a[pLvl] = Math.Max(a[pLvl], n);
              }
            }

            _MaxSpaceCounts = a; // присвоение в самом конце, когда все готово
          }
        }
      }

      int pLevel = FiasTools.AllLevelIndexer.IndexOf(level);
      if (pLevel < 0)
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть из списка FiasTools.AllLevels");

      return _MaxSpaceCounts[pLevel];
    }

    #endregion

    #region Точка после сокращения

    // Взяты значения поля SCNAME из таблицы AOType. 
    // Добавлены сокращения для домов и помещений из FiasEnumNames

    private static readonly StringArrayIndexer _AbbrWithDotIndexer = new StringArrayIndexer(new string[] { 
      "ал", // Аллея
      "взд", // Въезд
      "влд", // Владение 
      "г",
      "д", // Деревня или дом
      "двлд", // Домовладение
      "дор", // Дорога
      "ж/д казарм",
      "ж/д платф",
      "ж/д рзд",
      "ж/д ст",
      "зд",
      "ззд", // Заезд
      "зим", // Зимовье
      "кв",
      "киш", // Кишлак
      "ком", // Комната
      "корп",
      "кот", // Котельная
      "лн", // Линия
      "м", // Местечко
      "мгстр", // Магистраль
      "межсел.тер", // межселенная территория
      "месторожд",
      "мкр",
      "мр", // Месторождение
      "наб", // Набережная
      "л", // Литера
      "лит",
      "оф", // Офис
      "п", // Поселение, Поселок
      "п. ж/д ст", // Поселок при железнодорожной станции
      "п. ст", // Поселок при станции (поселок станции)
      "пав", // Павильон
      "пб", // Погреб
      "пер",
      "пл", // Площадь
      "платф",
      "подв", // Подвал
      "пом", // Помещение
      "пос",
      "проул", // Проулок
      "раб.уч",
      "рзд", // Разъезд
      "с",
      "сзд", // Съезд
      "скл", // Склад
      "сл", // Слобода
      "cоор", // Сооружение
      "сооруж",
      "ст", // Станция
      "стр", // Строение
      "тер", 
      "туп", // Тупик
      "у", // Улус
      "ул",
      "ус", // Усадьба
      "х", // Хутор
      "ш", // Шоссе
      "ю" // Юрты
     }, true);

    /// <summary>
    /// Возвращает true, если после сокращения должна идти точка.
    /// Например, для "ул" возвращается true, а для "аул" - false.
    /// Если сокращение уже заканчивается на точку, возвращается false.
    /// </summary>
    /// <param name="abbr">Проверяемое сокращение</param>
    /// <param name="level">Уровень адресного объекта (в текущей реализации не учитывается)</param>
    /// <returns>Необходимость добавления точки</returns>
    internal bool IsAbbreviationDotRequired(string abbr, FiasLevel level)
    {
      if (String.IsNullOrEmpty(abbr))
        return false;
      if (abbr[abbr.Length - 1] == '.')
        return false;

      // После сокращения должна идти точка с пробелом
      // http://new.gramota.ru/spravka/buro/search-answer?s=238880 (вопрос № 238880)
      // После составных сокращений "пгт", "рп", ... точка не нужна
      // http://new.gramota.ru/spravka/buro/search-answer?s=261380 (вопрос 261380)


      abbr = abbr.Replace('_', ' '); // бывают с подчеркиваниями

      // Не знаю разумного правила, только тупо перебирать.
      // Так как точка чаще не нужна, чем нужна, перебираем то, где нужна точка.
      return _AbbrWithDotIndexer.Contains(abbr);
    }

    #endregion
  }
}
