using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.FIAS
{
  #region FiasAbbreviationPlace

  /// <summary>
  /// Положение типа адресного объекта относительно наименования
  /// </summary>
  [Serializable]
  public enum FiasAOTypePlace
  {
    /// <summary>
    /// Сокращение не нужно
    /// </summary>
    None,

    /// <summary>
    /// Сокращение идет до наименования ("ул. Республики")
    /// </summary>
    BeforeName,

    /// <summary>
    /// Сокращение идет после наименования ("Тюменский р-н")
    /// </summary>
    AfterName
  }

  #endregion

  /// <summary>
  /// Объект адреса. Содержит адрес, разложенный на компоненты, которые можно задавать по отдельности.
  /// Этот класс не является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class FiasAddress : ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой адрес
    /// </summary>
    public FiasAddress()
    {
      _Items = new Dictionary<int, string>();
    }

    #endregion

    #region Внутреннее хранилище

    #region Константы

    private const FiasLevel LevelUnknownGuid = (FiasLevel)301;
    private const FiasLevel LevelAnyAOGuid = (FiasLevel)302;
    private const FiasLevel LevelRegionCode = (FiasLevel)311;
    private const FiasLevel LevelPostalCode = (FiasLevel)312;
    private const FiasLevel LevelManualPostalCode = (FiasLevel)313; // для инвертированного свойства AutoPostalCode
    private const FiasLevel LevelFiasPostalCode = (FiasLevel)314;
    private const FiasLevel LevelOKATO = (FiasLevel)315;
    private const FiasLevel LevelOKTMO = (FiasLevel)316;
    private const FiasLevel LevelIFNSFL = (FiasLevel)317;
    private const FiasLevel LevelTerrIFNSFL = (FiasLevel)318;
    private const FiasLevel LevelIFNSUL = (FiasLevel)319;
    private const FiasLevel LevelTerrIFNSUL = (FiasLevel)320;
    private const FiasLevel LevelDivType = (FiasLevel)331;
    private const FiasLevel LevelActuality = (FiasLevel)332;
    private const FiasLevel LevelLive = (FiasLevel)333;
    private const FiasLevel LevelStartDate = (FiasLevel)341;
    private const FiasLevel LevelEndDate = (FiasLevel)342;

    #endregion

    #region FiasItemPart

    /// <summary>
    /// Определяет, какую часть компонента адреса требуется получить/задать: наименование, тип адресного объекта или другую информацию
    /// </summary>
    [Serializable]
    private enum FiasItemPart
    {
      /// <summary>
      /// Именная часть адреса ("Ленина")
      /// </summary>
      Name = 0,

      /// <summary>
      /// Полное наименование типа адресного объекта ("улица")
      /// </summary>
      AOType = 1,

      /// <summary>
      /// GUID адресного объекта, дома, квартиры
      /// </summary>
      Guid = 2,

      /// <summary>
      /// Уникальный идентификатор записи в таблице AddrOb, House или Room
      /// </summary>
      RecId = 3,

      /// <summary>
      /// Почтовый индекс, коды, даты
      /// </summary>
      Value = 4,
    }

    #endregion

    /// <summary>
    /// Здесь хранятся все данные, кроме списка сообщений об ошибках
    /// </summary>
    private readonly Dictionary<int, string> _Items;

    private string InternalGetString(FiasLevel level, FiasItemPart part)
    {
      int key = (int)level * 10 + (int)part;
      string s;
      if (_Items.TryGetValue(key, out s))
        return s;
      else
        return String.Empty;
    }

    private void InternalSetString(FiasLevel level, FiasItemPart part, string value)
    {
      int key = (int)level * 10 + (int)part;

      if (String.IsNullOrEmpty(value))
        _Items.Remove(key);
      else
        _Items[key] = value;
    }

    /// <summary>
    /// Возвращает true, если объект пустой
    /// </summary>
    public bool IsEmpty { get { return _Items.Count == 0; } }

    /// <summary>
    /// Очищает объект адреса.
    /// В том числе устанавливает AutoPostalCode=true.
    /// </summary>
    public void Clear()
    {
      _Items.Clear();
    }

    /// <summary>
    /// Удаляет всю дополнительную информацию, кроме компонентов адреса, GUID'ов
    /// Очищает почтовый индекс, код региона, ОКАТО, ОКТМО, коды ИФНС.
    /// GUID'ы не удаляются
    /// Очищает свойства DivType, Actuality, Live, StartDate и EndDate
    /// 
    /// Свойство AutoPostalCode не меняется. Если AutoPostalCode=false, то почтовый индекс также не стирается.
    /// </summary>
    public void ClearAuxInfo()
    {
      RegionCode = String.Empty;
      if (AutoPostalCode)
        PostalCode = String.Empty;
      OKATO = String.Empty;
      OKTMO = String.Empty;
      IFNSUL = String.Empty;
      TerrIFNSUL = String.Empty;
      IFNSFL = String.Empty;
      TerrIFNSFL = String.Empty;

      DivType = FiasDivType.Unknown;
      Actuality = FiasActuality.Unknown;
      Live = null;
      StartDate = null;
      EndDate = null;
    }

    /// <summary>
    /// Очищает все данные для уровней адресных объектов, кроме зданий и помещений
    /// </summary>
    internal void ClearAOLevels()
    {
      AOGuid = Guid.Empty; // очищаем "неопределенный" GUID
      AORecId = Guid.Empty;
      for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        ClearLevel(FiasTools.AOLevels[i]);
    }


    #endregion

    #region Наименование и тип адресообразующего элемента

    /// <summary>
    /// Получить наименование адресообразующего элемента (без типа) для уровня
    /// </summary>
    /// <param name="level">Уровень</param>
    ///<returns>Наименование</returns>
    public string GetName(FiasLevel level)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть в списке FiasTools.AllLevels");
      return InternalGetString(level, FiasItemPart.Name);
    }

    /// <summary>
    /// Установить наименование адресообразующего элемента (без типа) для уровня
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="value">Наименование</param>
    public void SetName(FiasLevel level, string value)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть в списке FiasTools.AllLevels");
      InternalSetString(level, FiasItemPart.Name, value);
    }

    /// <summary>
    /// Получить тип адресообразующего элемента для уровня.
    /// Используется полное наименование типа, то есть "улица", а не "ул.".
    /// </summary>
    /// <param name="level">Уровень</param>
    ///<returns>Тип адресного объекта </returns>
    public string GetAOType(FiasLevel level)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть в списке FiasTools.AllLevels");
      return InternalGetString(level, FiasItemPart.AOType);
    }

    /// <summary>
    /// Установить тип адресообразующего элемента для уровня.
    /// Используется полное наименование типа, то есть "улица", а не "ул.".
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="value">тип адресного объекта </param>
    public void SetAOType(FiasLevel level, string value)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть в списке FiasTools.AllLevels");
      InternalSetString(level, FiasItemPart.AOType, value);
    }

    /// <summary>
    /// Устанавливает тип адресообразующего элемента по умолчанию, используя FiasTools.GetDefaultAOType().
    /// Если для уровня не определен тип по умолчанию, устанавливается пустая строка.
    /// Текущее наименование элемента не проверяется.
    /// </summary>
    /// <param name="level">Уровень</param>
    public void SetDefaultAOType(FiasLevel level)
    {
      SetAOType(level, FiasTools.GetDefaultAOType(level));
    }

    /// <summary>
    /// Доступ к компонентам адреса в режиме "Имя плюс тип адресообразующего элемента" именно в таком порядке, то есть "Ленина улица".
    /// Это свойство обычно не следует использовать. Имя и тип удобнее задавать по отдельности.
    /// Для текстового вывода следует использовать метод FiasHandler.Format(), который обеспечивает правильный порядок имени и типа, 
    /// и использует сокращения.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <returns>Часть адреса</returns>
    public string this[FiasLevel level]
    {
      get
      {
        if (level == FiasLevel.Unknown)
          return String.Empty; // 29.04.2020

        string s1 = GetName(level);
        string s2 = GetAOType(level);
        if (s1.Length == 0)
          return String.Empty;
        else if (s2.Length == 0)
          return s1;
        else
          return s1 + " " + s2;
      }
      set
      {
        int p = value.LastIndexOf(' ');
        if (p >= 0 && FiasTools.AllLevelIndexer.Contains(level))
        {
          string s1 = value.Substring(0, p);
          string s2 = value.Substring(p + 1);
          SetName(level, s1);
          SetAOType(level, s2);
        }
        else
        {
          SetName(level, value);
          SetAOType(level, String.Empty);
        }
      }
    }

    /// <summary>
    /// Очищает один уровень: наименование, сокращение, GUID и RecId.
    /// Другие уровни не меняются.
    /// Уровни House/Building/Structure и Flat/Room различаются.
    /// </summary>
    /// <param name="level">Очищаемый уровень</param>
    public void ClearLevel(FiasLevel level)
    {
      InternalSetString(level, FiasItemPart.Name, String.Empty);
      InternalSetString(level, FiasItemPart.AOType, String.Empty);
      InternalSetString(level, FiasItemPart.Guid, String.Empty);
      InternalSetString(level, FiasItemPart.RecId, String.Empty);
    }


    /// <summary>
    /// Очищает все уровни, которые находятся ниже заданного.
    /// Уровень <paramref name="level"/> и вышележащие не меняются.
    /// Если <paramref name="level"/>=Unknown, очищаются все уровни.
    /// Уровни House/Building/Structure и Flat/Room различаются.
    /// </summary>
    /// <param name="level">Последний сохраняемый уровень</param>
    public void ClearBelow(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);

      for (int i = p + 1; i < FiasTools.AllLevels.Length; i++)
        ClearLevel(FiasTools.AllLevels[i]);
    }

    /// <summary>
    /// Очищает все уровни, начиная с заданного, и ниже.
    /// Уровени выше <paramref name="level"/> не меняются.
    /// Если <paramref name="level"/>=Unknown, выбрасывается исключение.
    /// Уровни House/Building/Structure и Flat/Room различаются.
    /// </summary>
    /// <param name="level">Первый очищаемый уровень</param>
    public void ClearStartingWith(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentException();

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        ClearLevel(FiasTools.AllLevels[i]);
    }

    /// <summary>
    /// Возвращает последний уровень, для которого есть заполненное наименование.
    /// Если ни уровень не заполнен, возвращает FiasLevel.Unknown.
    /// Уровни House/Building/Structure и Flat/Room различаются.
    /// </summary>
    public FiasLevel NameBottomLevel
    {
      get
      {
        if (IsEmpty)
          return FiasLevel.Unknown;
        for (int i = FiasTools.AllLevels.Length - 1; i >= 0; i--)
        {
          if (GetName(FiasTools.AllLevels[i]).Length > 0)
            return FiasTools.AllLevels[i];
        }

        return FiasLevel.Unknown;
      }
    }


    /// <summary>
    /// Возвращает предыдущий заполненный уровень адреса, расположенный выше заданного.
    /// Если выше заданного уровня нет заполненных уровней, возвращается Unknown.
    /// </summary>
    /// <param name="level">Уровень, выше которого выполняется проверка</param>
    /// <returns>Предыдущий заполненный уровень</returns>
    public FiasLevel GetPrevLevel(FiasLevel level)
    {
      level = CorrectLevel(level);

      int p = FiasTools.AOLevelIndexer.IndexOf(level);
      if (p < 0)
        return FiasLevel.Unknown;
      for (int i = p - 1; i >= 0; i--)
      {
        FiasLevel lvl2 = FiasTools.AllLevels[i];
        if (GetName(lvl2).Length > 0)
        {
          switch (lvl2)
          {
            case FiasLevel.Building:
            case FiasLevel.Structure:
              return FiasLevel.House;
            default:
              return lvl2;
          }
        }
      }
      return FiasLevel.Unknown;
    }

    /// <summary>
    /// Возвращает true, если заполнено наименование адресного объекат какого-либо уровня от региона до улицы включительно
    /// </summary>
    public bool ContainsAddrObName
    {
      get
      {
        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          if (GetName(FiasTools.AOLevels[i]).Length > 0)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если задан номер дома, корпуса или строения
    /// </summary>
    public bool ContainsHouseNum
    {
      get
      {
        return GetName(FiasLevel.House).Length > 0 ||
          GetName(FiasLevel.Building).Length > 0 ||
          GetName(FiasLevel.Structure).Length > 0;
      }
    }

    /// <summary>
    /// Возвращает true, если задан номер квартиры или помещения
    /// </summary>
    public bool ContainsRoomNum
    {
      get
      {
        return GetName(FiasLevel.Flat).Length > 0 ||
          GetName(FiasLevel.Room).Length > 0;
      }
    }

    /// <summary>
    /// Возвращает список уровней, для которых заполнено название.
    /// Уровни House/Buiding/Structure и Flat/Room различаются
    /// </summary>
    public FiasLevelSet NameLevels
    {
      get
      {
        int _IntValue = 0;
        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
            _IntValue |= (1 << i);
        }
        return new FiasLevelSet(_IntValue);
      }
    }

    /// <summary>
    /// Выполнить копирование наименований и типов адресообразующих элементов из текущего адреса в другой.
    /// GUIDы, идентификаторы записей, почтовый индекс, сообщения об ошибках не копируются.
    /// </summary>
    /// <param name="dest">Заполняемый адресный объект</param>
    /// <param name="levels">Копируемые уровни. 
    /// Если в текущем адресе для уровня из набора нет значения, то уровень очищается.
    /// В заполнямом адресе уровни, не перечисленные в наборе, не заменяются</param>
    public void CopyNamesTo(FiasAddress dest, FiasLevelSet levels)
    { 
#if DEBUG
      if (dest == null)
        throw new ArgumentNullException("dest");
#endif

      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if (levels[FiasTools.AllLevels[i]])
        {
          dest.SetName(FiasTools.AllLevels[i], this.GetName(FiasTools.AllLevels[i]));
          dest.SetAOType(FiasTools.AllLevels[i], this.GetAOType(FiasTools.AllLevels[i]));
        }
      }
    }

    #endregion

    #region Именные свойства компонентов адреса
#if XXX
    /// <summary>
    /// Наименование региона ("Тюменская обл")
    /// </summary>    
    public string Region { get { return this[FiasLevel.Region]; } set { this[FiasLevel.Region] = value; } }

    /// <summary>
    /// Автономный округ (устаревшее)
    /// </summary>
    public string AutonomousArea { get { return this[FiasLevel.AutonomousArea]; } set { this[FiasLevel.AutonomousArea] = value; } }

    /// <summary>
    /// Район ("Тюменский р-н")
    /// </summary>
    public string District { get { return this[FiasLevel.District]; } set { this[FiasLevel.District] = value; } }

    /// <summary>
    /// Городское/сельское поселение
    /// </summary>
    public string Settlement { get { return this[FiasLevel.Settlement]; } set { this[FiasLevel.Settlement] = value; } }

    /// <summary>
    /// Город
    /// </summary>
    public string City { get { return this[FiasLevel.City]; } set { this[FiasLevel.City] = value; } }

    /// <summary>
    /// Внутригородская территория (устаревшее)
    /// </summary>
    public string InnerCityArea { get { return this[FiasLevel.InnerCityArea]; } set { this[FiasLevel.InnerCityArea] = value; } }

    /// <summary>
    /// Населенный пункт
    /// </summary>
    public string Village { get { return this[FiasLevel.Village]; } set { this[FiasLevel.Village] = value; } }

    /// <summary>
    /// Планировочная структура
    /// </summary>
    public string PlanningStructure { get { return this[FiasLevel.PlanningStructure]; } set { this[FiasLevel.PlanningStructure] = value; } }

    /// <summary>
    /// Улица
    /// </summary>
    public string Street { get { return this[FiasLevel.Street]; } set { this[FiasLevel.Street] = value; } }

    /// <summary>
    /// Земельный участок
    /// </summary>
    public string LandPlot { get { return this[FiasLevel.LandPlot]; } set { this[FiasLevel.LandPlot] = value; } }

    /// <summary>
    /// Здание, сооружение, объект незавершенного строительства
    /// </summary>
    public string House { get { return this[FiasLevel.House]; } set { this[FiasLevel.House] = value; } }

    /// <summary>
    /// Помещение в пределах здания, сооружения
    /// </summary>
    public string Flat { get { return this[FiasLevel.Flat]; } set { this[FiasLevel.Flat] = value; } }

    /// <summary>
    /// Дополнительная территория (устаревшее)
    /// </summary>
    public string AdditionalTerritory { get { return this[FiasLevel.AdditionalTerritory]; } set { this[FiasLevel.AdditionalTerritory] = value; } }

    /// <summary>
    /// Объект на дополнительной территории (устаревшее)
    /// </summary>
    public string AdditionalTerritoryObject { get { return this[FiasLevel.AdditionalTerritoryObject]; } set { this[FiasLevel.AdditionalTerritoryObject] = value; } }

    /// <summary>
    /// Корпус
    /// </summary>
    public string Buiding { get { return this[FiasLevel.Building]; } set { this[FiasLevel.Building] = value; } }

    /// <summary>
    /// Строение
    /// </summary>
    public string Structure { get { return this[FiasLevel.Structure]; } set { this[FiasLevel.Structure] = value; } }

    /// <summary>
    /// Комната
    /// </summary>
    public string Room { get { return this[FiasLevel.Room]; } set { this[FiasLevel.Room] = value; } }
#endif
    #endregion

    #region Guid'ы

    private void InternalSetGuid(FiasLevel level, FiasItemPart part, Guid value)
    {
      if (value == Guid.Empty)
        InternalSetString(level, part, String.Empty);
      else
        InternalSetString(level, part, value.ToString());
    }

    private Guid InternalGetGuid(FiasLevel level, FiasItemPart part)
    {
      string s = InternalGetString(level, part);
      if (s.Length == 0)
        return Guid.Empty;
      else
        return new Guid(s);
    }

    /// <summary>
    /// Для зданий и помещений есть несколько уровней для наименований, но GUIDы у них общие.
    /// Для зданий возвращается FiasLevel.House, а для помещений - FiasLevel.Flat
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static FiasLevel CorrectLevel(FiasLevel level)
    {
      if (level == FiasLevel.Unknown)
        return FiasLevel.Unknown;

      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "Уровень должен быть в списке FiasTools.AllLevels");
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          return FiasLevel.House;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          return FiasLevel.Flat;
        default:
          return level;
      }
    }

    /// <summary>
    /// Получение GUIDа для уровня адресного объекта, дома или квартиры.
    /// Возвращает Guid.Empty, если соответствующий GUID не задан
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <returns>GUID</returns>
    public Guid GetGuid(FiasLevel level)
    {
      return InternalGetGuid(CorrectLevel(level), FiasItemPart.Guid);
    }

    /// <summary>
    /// Установка GUIDа для уровня адресного объекта, дома или квартиры.
    /// Следует учитывать, что для уровней <paramref name="level"/>=House, Building и Construction
    /// храниться единственный GUID, как и для уровней Flat и Room. Установка для одного уровня отражается для всех уровней.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="value">GUID</param>
    public void SetGuid(FiasLevel level, Guid value)
    {
      InternalSetGuid(CorrectLevel(level), FiasItemPart.Guid, value);
    }

    /// <summary>
    /// GUID адресного объекта.
    /// Возвращает GUID объекта самого вложенного уровня (обычно, улицы).
    /// Установка свойства задает GUID, не привязанный к конкретному уровню
    /// </summary>
    public Guid AOGuid
    {
      get
      {
        Guid g = InternalGetGuid(LevelAnyAOGuid, FiasItemPart.Guid);
        if (g != Guid.Empty)
          return g;

        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          g = InternalGetGuid(FiasTools.AOLevels[i], FiasItemPart.Guid);
          if (g != Guid.Empty)
            return g;
        }
        return Guid.Empty;
      }
      set
      {
        InternalSetGuid(LevelAnyAOGuid, FiasItemPart.Guid, value);
      }
    }
    /// <summary>
    /// GUID адресного объекта, дома или квартиры.
    /// Если поиск по устойчивым идентификаторам адресных объектов заканчивается неудачно,
    /// выполняется поиск по "неустойчивым" идентификаторам AORecId, HouseRecId и RoomRecId.
    /// </summary>
    public Guid UnknownGuid
    {
      get
      {
        return InternalGetGuid(LevelUnknownGuid, FiasItemPart.Guid);
      }
      set
      {
        InternalSetGuid(LevelUnknownGuid, FiasItemPart.Guid, value);
      }
    }

    /// <summary>
    /// GUID здания.
    /// Эквивалентно вызову методов GetGuid()/SetGuid для FiasLevel.House
    /// </summary>
    public Guid HouseGuid
    {
      get { return GetGuid(FiasLevel.House); }
      set { SetGuid(FiasLevel.House, value); }
    }

    /// <summary>
    /// GUID помещения.
    /// Эквивалентно вызову методов GetGuid()/SetGuid для FiasLevel.Room
    /// </summary>
    public Guid RoomGuid
    {
      get { return GetGuid(FiasLevel.Room); }
      set { SetGuid(FiasLevel.Room, value); }
    }

    /// <summary>
    /// Возвращает самый вложенный компонент адреса, для которого задан GUID
    /// (Помещение, Дом, Улица, ...).
    /// AOGuid и UnknownGuid не считаются.
    /// Если ни одного GUID'а не определено, возвращается FiasLevel.Unknown
    /// Если задан Guid для дома, то возвращается FiasLevel.House, а не FiasLevel.Structure, несмотря на то,
    /// что GetGuid(FiasLevel.Structure) также возвращает значение.
    /// Аналогично, для помещения возвращается значение FiasLevel.Flat, а не Room
    /// </summary>
    public FiasLevel GuidBottomLevel
    {
      get
      {
        if (InternalGetString(FiasLevel.Flat, FiasItemPart.Guid).Length > 0)
          return FiasLevel.Flat;
        if (InternalGetString(FiasLevel.House, FiasItemPart.Guid).Length > 0)
          return FiasLevel.House;
        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          if (InternalGetString(FiasTools.AOLevels[i], FiasItemPart.Guid).Length > 0)
            return FiasTools.AOLevels[i];
        }
        return FiasLevel.Unknown;
      }
    }

    /// <summary>
    /// Очищает все GUIDы, включая UnknownGuid и AOGuid
    /// </summary>
    public void ClearGuids()
    {
      ClearGuidsStartingWith(FiasLevel.Region);
    }

    /// <summary>
    /// Стирает GUIDы, начиная с заданного уровня и до уровня Flat включительно.
    /// Также очищает UnknownGuid и AOGuid
    /// </summary>
    /// <param name="level">Самый верхний уровень, который нужно очистить. Если FiasLevel.Region,
    /// то будут очищены все Guidы</param>
    public void ClearGuidsStartingWith(FiasLevel level)
    {
      int p;
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat);
          break;
        default:
          p = FiasTools.AllLevelIndexer.IndexOf(level);
          if (p < 0)
            throw new ArgumentException("level=" + level.ToString(), "level");
          break;
      }

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        SetGuid(FiasTools.AllLevels[i], Guid.Empty);
      UnknownGuid = Guid.Empty;
      AOGuid = Guid.Empty;
    }

    /// <summary>
    /// Стирает GUIDы, начиная с уровня, ниже заданного, и до уровня Flat включительно.
    /// Также очищает UnknownGuid и AOGuid
    /// </summary>
    /// <param name="level">Самый нижний уровень, который не нужно очищать. Если равно Guid.Unknown, будут очищены все уровни</param>
    public void ClearGuidsBelow(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          ClearGuidsStartingWith(FiasLevel.Flat);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          int p = FiasTools.AllLevelIndexer.IndexOf(level);
          ClearGuidsStartingWith(FiasTools.AllLevels[p + 1]);
          break;
      }
    }

    /// <summary>
    /// Возвращает устойчивый идентификатор объекта, если есть заданный, включая AOGuid и UnknownGuid
    /// </summary>
    public Guid AnyGuid
    {
      get
      {
        Guid g;
        g = UnknownGuid;
        if (g != Guid.Empty)
          return g;
        g = GetGuid(FiasLevel.Room);
        if (g != Guid.Empty)
          return g;
        g = GetGuid(FiasLevel.House);
        if (g != Guid.Empty)
          return g;
        g = AOGuid;
        if (g != Guid.Empty)
          return g;
        return Guid.Empty;
      }
    }

    /// <summary>
    /// Возвращает список уровней, для которых заполнен устойчивый идентификатор записи.
    /// Уровни House/Buiding/Structure и Flat/Room различаются в том смысле, что для них флажки устанавливаются,
    /// только если заполнена соответствующая именная часть.
    /// В промежуточном состоянии, до вызова FillAddress(), если, например, установлен HouseGuid, но имена не заполнены,
    /// устанавливаются все уровни House, Building и Structure.
    /// Идентификаторы, не привязанные к уровню (UnknownGuid, AOGuid) не учитываются.
    /// </summary>
    public FiasLevelSet GuidLevels
    {
      get
      {
        bool HasHouseGuid = false;
        bool HasHouseName = false;
        bool HasRoomGuid = false;
        bool HasRoomName = false;

        int _IntValue = 0;
        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          switch (FiasTools.AllLevels[i])
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Structure:
              if (InternalGetString(FiasLevel.House, FiasItemPart.Guid).Length > 0)
              {
                HasHouseGuid = true;
                if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
                {
                  _IntValue |= (1 << i);
                  HasHouseName = true;
                }
              }
              break;

            case FiasLevel.Flat:
            case FiasLevel.Room:
              if (InternalGetString(FiasLevel.Flat, FiasItemPart.Guid).Length > 0)
              {
                HasRoomGuid = true;
                if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
                {
                  _IntValue |= (1 << i);
                  HasRoomName = true;
                }
              }
              break;
            default:
              if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Guid).Length > 0)
                _IntValue |= (1 << i);
              break;
          }
        }

        FiasLevelSet res = new FiasLevelSet(_IntValue);

        if (HasHouseGuid && (!HasHouseName))
          res |= FiasLevelSet.HouseLevels;
        if (HasRoomGuid && (!HasRoomName))
          res |= FiasLevelSet.RoomLevels;

        return res;
      }
    }

    #endregion

    #region Идентификаторы записей

    /// <summary>
    /// Получение идентификатора записи для уровня адресного объекта (AOID), дома (HOUSEID) или квартиры (ROOMID).
    /// Возвращает Guid.Empty, если соответствующий GUID не задан.
    /// В прикладном коде обычно используются "устойчивые" GUIDы адресных объектов, а не идентификаторы записей.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <returns>Идентификаторы записи</returns>
    public Guid GetRecId(FiasLevel level)
    {
      return InternalGetGuid(CorrectLevel(level), FiasItemPart.RecId);
    }

    /// <summary>
    /// Установка идентификатора записи для уровня адресного объекта, дома или квартиры.
    /// Следует учитывать, что для уровней <paramref name="level"/>=House, Building и Construction
    /// храниться единственный идентификатор, как и для уровней Flat и Room. Установка для одного уровня отражается для всех уровней.
    /// В прикладном коде обычно используются "устойчивые" GUIDы адресных объектов, а не идентификаторы записей.
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="value">Идентфикатор записи ФИАС</param>
    public void SetRecId(FiasLevel level, Guid value)
    {
      InternalSetGuid(CorrectLevel(level), FiasItemPart.RecId, value);
    }

    /// <summary>
    /// Идкнтификатор записи адресного объекта.
    /// Возвращает ID объекта самого вложенного уровня (обычно, улицы).
    /// Установка свойства задает идентификатор, не привязанный к конкретному уровню
    /// </summary>
    public Guid AORecId
    {
      get
      {
        Guid g = InternalGetGuid(LevelAnyAOGuid, FiasItemPart.RecId);
        if (g != Guid.Empty)
          return g;

        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          g = InternalGetGuid(FiasTools.AOLevels[i], FiasItemPart.RecId);
          if (g != Guid.Empty)
            return g;
        }
        return Guid.Empty;
      }
      set
      {
        InternalSetGuid(LevelAnyAOGuid, FiasItemPart.RecId, value);
      }
    }

    ///// <summary>
    ///// GUID адресного объекта, дома или квартиры.
    ///// </summary>
    //public Guid UnknownGuid
    //{
    //  get
    //  {
    //    string s = this[LevelUnknownGuid, FiasItemPart.Guid];
    //    if (s.Length == 0)
    //      return Guid.Empty;
    //    else
    //      return new Guid(s);
    //  }
    //  set
    //  {
    //    SetGuid(LevelUnknownGuid, value);
    //  }
    //}

    ///// <summary>
    ///// Возвращает самый вложенный компонент адреса, для которого задан GUID
    ///// (Помещение, Дом, Улица, ...).
    ///// AOGuid и UnknownGuid не считаются.
    ///// Если ни одного GUID'а не определено, возвращается FiasLevel.Unknown
    ///// </summary>
    //public FiasLevel GuidBottomLevel
    //{
    //  get
    //  {
    //    for (int i = LevelWithGuidArray.Length - 1; i >= 0; i--)
    //    {
    //      if (this[LevelWithGuidArray[i], FiasItemPart.Guid].Length > 0)
    //        return LevelWithGuidArray[i];
    //    }
    //    return FiasLevel.Unknown;
    //  }
    //}

    /// <summary>
    /// Очищает все идентификаторы записи.
    /// Также очищает свойство AORecId.
    /// </summary>
    public void ClearRecIds()
    {
      ClearRecIdsStartingWith(FiasLevel.Region);
    }

    /// <summary>
    /// Стирает идентификаторы записей, начиная с заданного уровня.
    /// Также очищает свойство AORecId.
    /// </summary>
    /// <param name="level">Уровень, начиная с которого требуется выполнить очистку.
    /// Если задано значение Region, будут очищены все уровни</param>
    public void ClearRecIdsStartingWith(FiasLevel level)
    {
      int p;
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat);
          break;
        default:
          p = FiasTools.AllLevelIndexer.IndexOf(level);
          if (p < 0)
            throw new ArgumentException("level=" + level.ToString(), "level");
          break;
      }

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        SetRecId(FiasTools.AllLevels[i], Guid.Empty);
      AORecId = Guid.Empty;
    }

    /// <summary>
    /// Стирает идентификаторы записей, начиная с заданного уровня.
    /// Также очищает свойство AORecId.
    /// </summary>
    /// <param name="level">Уровень, начиная с которого требуется выполнить очистку.
    /// Если задано значение Region, будут очищены все уровни</param>
    public void ClearRecIdsBelow(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          ClearRecIdsStartingWith(FiasLevel.Flat);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          int p = FiasTools.AllLevelIndexer.IndexOf(level);
          ClearRecIdsStartingWith(FiasTools.AllLevels[p + 1]);
          break;
      }
    }


    #endregion

    #region Коды, связанные с адресом

    /// <summary>
    /// Код региона
    /// </summary>
    public string RegionCode
    {
      get { return InternalGetString(LevelRegionCode, FiasItemPart.Value); }
      internal set { InternalSetString(LevelRegionCode, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Если true (по умолчанию), то почтовый индекс берется из справочника ФИАС.
    /// Если false, то индекс задан вручную и не будет перезаписан при загрузке адреса
    /// </summary>
    public bool AutoPostalCode
    {
      get { return InternalGetInt(LevelManualPostalCode) == 0; }
      set { InternalSetInt(LevelManualPostalCode, value ? 0 : 1); }
    }

    /// <summary>
    /// Почтовый индекс.
    /// Установка свойства имеет смысл только при AutoPostalCode=false.
    /// </summary>
    public string PostalCode
    {
      get { return InternalGetString(LevelPostalCode, FiasItemPart.Value); }
      set { InternalSetString(LevelPostalCode, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Почтовый индекс, определенный в классификаторе ФИАС.
    /// Это свойство может отличаться от PostalCode при AutoPostalCode=false
    /// </summary>
    public string FiasPostalCode
    {
      get { return InternalGetString(LevelFiasPostalCode, FiasItemPart.Value); }
      internal set
      {
        InternalSetString(LevelFiasPostalCode, FiasItemPart.Value, value);
        if (AutoPostalCode)
          InternalSetString(LevelPostalCode, FiasItemPart.Value, value);
      }
    }

    /// <summary>
    /// ОКАТО
    /// </summary>
    public string OKATO
    {
      get { return InternalGetString(LevelOKATO, FiasItemPart.Value); }
      internal set { InternalSetString(LevelOKATO, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ОКТМО
    /// </summary>
    public string OKTMO
    {
      get { return InternalGetString(LevelOKTMO, FiasItemPart.Value); }
      internal set { InternalSetString(LevelOKTMO, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Код ИФНС ФЛ
    /// </summary>
    public string IFNSFL
    {
      get { return InternalGetString(LevelIFNSFL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelIFNSFL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Код территориального участка ИФНС ФЛ
    /// </summary>
    public string TerrIFNSFL
    {
      get { return InternalGetString(LevelTerrIFNSFL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelTerrIFNSFL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Код ИФНС ЮЛ
    /// </summary>
    public string IFNSUL
    {
      get { return InternalGetString(LevelIFNSUL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelIFNSUL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// Код территориального участка ИФНС ЮЛ
    /// </summary>
    public string TerrIFNSUL
    {
      get { return InternalGetString(LevelTerrIFNSUL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelTerrIFNSUL, FiasItemPart.Value, value); }
    }

    #endregion

    #region Прочие свойства адреса

    private void InternalSetInt(FiasLevel level, int value)
    {
      if (value == 0)
        InternalSetString(level, FiasItemPart.Value, String.Empty);
      else
        InternalSetString(level, FiasItemPart.Value, StdConvert.ToString(value));
    }

    private int InternalGetInt(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return 0;
      else
        return StdConvert.ToInt32(s);
    }

    /// <summary>
    /// Тип адресного деления
    /// </summary>
    public FiasDivType DivType
    {
      get
      {
        return (FiasDivType)InternalGetInt(LevelDivType);
      }
      internal set
      {
        InternalSetInt(LevelDivType, (int)value);
      }
    }

    /// <summary>
    /// Актуальность адреса.
    /// Определяется после загрузки адреса
    /// </summary>
    public FiasActuality Actuality
    {
      get
      {
        return (FiasActuality)InternalGetInt(LevelActuality);
      }
      internal set
      {
        InternalSetInt(LevelActuality, (int)value);
      }
    }

    private void InternalSetNullableBool(FiasLevel level, bool? value)
    {
      if (value.HasValue)
        InternalSetString(level, FiasItemPart.Value, value.Value ? "1" : "0");
      else
        InternalSetString(level, FiasItemPart.Value, String.Empty);
    }

    private bool? InternalGetNullableBool(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return null;
      else
        return StdConvert.ToInt32(s) != 0;
    }

    /// <summary>
    /// True, если адресный объект помечен как действующий
    /// </summary>
    public bool? Live
    {
      get { return InternalGetNullableBool(LevelLive); }
      internal set
      {
        InternalSetNullableBool(LevelLive, value);
      }
    }

    #endregion

    #region Даты действия записи

    private void InternalSetNullableDate(FiasLevel level, DateTime? value)
    {
      if (value.HasValue)
        InternalSetString(level, FiasItemPart.Value, StdConvert.ToString(value.Value, false));
      else
        InternalSetString(level, FiasItemPart.Value, String.Empty);
    }

    private DateTime? InternalGetNullableDate(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return null;
      else
        return StdConvert.ToDateTime(s, false);
    }

    /// <summary>
    /// Дата начала действия записи.
    /// Возвращается период действия для записи классификатора самого вложенного уровня (определяется GuidBottomLevel)
    /// Свойство возвращает null, если FiasDBSettings.UseDates=false.
    /// </summary>
    public DateTime? StartDate
    {
      get { return InternalGetNullableDate(LevelStartDate); }
      internal set
      {
        InternalSetNullableDate(LevelStartDate, value);
      }
    }


    /// <summary>
    /// Дата окончания действия записи.
    /// Возвращается период действия для записи классификатора самого вложенного уровня (определяется GuidBottomLevel)
    /// Свойство возвращает null, если FiasDBSettings.UseDates=false.
    /// </summary>
    public DateTime? EndDate
    {
      get { return InternalGetNullableDate(LevelEndDate); }
      internal set
      {
        InternalSetNullableDate(LevelEndDate, value);
      }
    }

    #endregion

    #region Ошибки

    /// <summary>
    /// Полный список сообщений.
    /// Список заполняется методом FillAddress().
    /// </summary>
    public ErrorMessageList Messages
    {
      get
      {
        if (_Messages == null)
          return ErrorMessageList.Empty;
        else
          return _Messages;
      }
    }
    private ErrorMessageList _Messages;

    internal void ClearMessages()
    {
      _Messages = null;
    }

    /// <summary>
    /// Добавляет сообщение об ошибке, не привязанное к конкретному уровню адреса
    /// </summary>
    /// <param name="kind">Серьезность</param>
    /// <param name="message">Текст сообщения</param>
    internal void AddMessage(ErrorMessageKind kind, string message)
    {
      AddMessage(kind, message, FiasLevel.Unknown);
    }

    /// <summary>
    /// Добавляет сообщение об ошибке, привязанное к конкретному уровню адреса
    /// </summary>
    /// <param name="kind">Серьезность</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="level">Уровень адреса</param>
    internal void AddMessage(ErrorMessageKind kind, string message, FiasLevel level)
    {
      if (_Messages == null)
        _Messages = new ErrorMessageList();
      if (level != FiasLevel.Unknown)
        message = "[" + FiasEnumNames.ToString(level, false) + "] " + message;
      _Messages.Add(new ErrorMessageItem(kind, message, String.Empty, level));
    }

    internal void AddHouseMessage(ErrorMessageKind kind, string message)
    {
      if (GetName(FiasLevel.House).Length > 0)
        AddMessage(kind, message, FiasLevel.House);
      else if (GetName(FiasLevel.Building).Length > 0)
        AddMessage(kind, message, FiasLevel.Building);
      else if (GetName(FiasLevel.Structure).Length > 0)
        AddMessage(kind, message, FiasLevel.Structure);
      else // все поля пустые
        AddMessage(kind, message, FiasLevel.House);
    }

    internal void AddRoomMessage(ErrorMessageKind kind, string message)
    {
      if (GetName(FiasLevel.Flat).Length > 0)
        AddMessage(kind, message, FiasLevel.Flat);
      else if (GetName(FiasLevel.Room).Length > 0)
        AddMessage(kind, message, FiasLevel.Room);
      else // все поля пустые
        AddMessage(kind, message, FiasLevel.Flat);
    }

    internal void AddMessages(ErrorMessageList messages)
    {
      if (messages.Count == 0)
        return;
      if (_Messages == null)
        _Messages = new ErrorMessageList();
      _Messages.Add(messages);
    }

    /// <summary>
    /// Возвращает список сообщений, связанных с заданным уровнем классификатора
    /// </summary>
    /// <param name="level">Уровень адреса ФИАС из перечня FiasTools.AllLevels.
    /// Если задано значение FiasLevel.Unknown, возвращаются сообщения, не привязанные ни к какому уровню</param>
    /// <returns>Выбранные сообщения из списка Messages</returns>
    public ErrorMessageList GetMessages(FiasLevel level)
    {
      if (_Messages == null)
        return ErrorMessageList.Empty;

      ErrorMessageList list2 = null;
      for (int i = 0; i < _Messages.Count; i++)
      {
        FiasLevel thisLevel = FiasLevel.Unknown;
        if (_Messages[i].Tag is FiasLevel)
          thisLevel = (FiasLevel)(_Messages[i].Tag);
        if (thisLevel == level)
        {
          if (list2 == null)
            list2 = new ErrorMessageList();
          list2.Add(_Messages[i]);
        }
      }
      if (list2 == null)
        return ErrorMessageList.Empty;
      else
      {
        list2.SetReadOnly();
        return list2;
      }
    }


    /// <summary>
    /// Возвращает список сообщений, связанных с заданными уровнями классификатора.
    /// Нельзя получить сообщения, не привязанные к уровню
    /// </summary>
    /// <param name="levels">Уровни адреса ФИАС из перечня FiasTools.AllLevels.</param>
    /// <returns>Выбранные сообщения из списка Messages</returns>
    public ErrorMessageList GetMessages(FiasLevelSet levels)
    {
      if (_Messages == null)
        return ErrorMessageList.Empty;

      ErrorMessageList list2 = null;
      for (int i = 0; i < _Messages.Count; i++)
      {
        if (_Messages[i].Tag is FiasLevel)
        {
          FiasLevel thisLevel = (FiasLevel)(_Messages[i].Tag);
          if (levels[thisLevel])
          {
            if (list2 == null)
              list2 = new ErrorMessageList();
            list2.Add(_Messages[i]);
          }
        }
      }
      if (list2 == null)
        return ErrorMessageList.Empty;
      else
      {
        list2.SetReadOnly();
        return list2;
      }
    }

    /// <summary>
    /// Возвращает уровень адресного объекта к которому относится сообщение из списка Messages.
    /// Некоторые сообщения могут быть не привязаны к конкруетному уровню. Для них возвращается FiasLevel.Unknown
    /// </summary>
    /// <param name="item">Сообщение</param>
    /// <returns>Уровень адресного объекта</returns>
    public FiasLevel GetMessageFiasLevel(ErrorMessageItem item)
    {
      if (item.Tag is FiasLevel)
        return (FiasLevel)(item.Tag);
      else
        return FiasLevel.Unknown;
    }                       

    #endregion

    #region ToString()

    /// <summary>
    /// Текстовое представление для отладки.
    /// Включает список ошибок
    /// Используйте метод FiasHandler.GetText()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(PostalCode);
      AddPartToString(sb, FiasLevel.Region, FiasAOTypePlace.AfterName);
      AddPartToString(sb, FiasLevel.District, FiasAOTypePlace.AfterName);
      AddPartToString(sb, FiasLevel.City, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Village, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.PlanningStructure, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Street, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.House, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Building, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Structure, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Flat, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Room, FiasAOTypePlace.BeforeName);

      if (_Messages != null)
      {
        for (int i = 0; i < _Messages.Count; i++)
        {
          sb.Append(Environment.NewLine);
          sb.Append(_Messages[i].ToString());
        }
      }

      return sb.ToString();
    }

    internal void AddPartToString(StringBuilder sb, FiasLevel level, FiasAOTypePlace place)
    {
      string s1 = GetName(level);
      string s2 = GetAOType(level);

      if (level == FiasLevel.Region)
      {
        if (s1.IndexOf(s2, StringComparison.OrdinalIgnoreCase) >= 0)
          s2 = String.Empty; // 05.10.2020
      }

      if (s1.Length == 0)
        return;
      if (sb.Length > 0)
        sb.Append(", ");

      if (place == FiasAOTypePlace.BeforeName && s2.Length > 0)
      {
        sb.Append(s2);
        sb.Append(" ");
      }
      sb.Append(s1);
      if (place == FiasAOTypePlace.AfterName && s2.Length > 0)
      {
        sb.Append(" ");
        sb.Append(s2);
      }
    }

    #endregion

    #region Clone()

    /// <summary>
    /// Клонирование адреса.
    /// Копируются все поля адреса и список сообщений.
    /// </summary>
    /// <returns>Новый объект адреса</returns>
    public FiasAddress Clone()
    {
      FiasAddress a = new FiasAddress();
      foreach (KeyValuePair<int, string> pair in _Items)
        a._Items.Add(pair.Key, pair.Value);

      if (_Messages != null)
      {
        a._Messages = new ErrorMessageList();
        a._Messages.AddRange(_Messages);
      }
      return a;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Сравнивает нижний заполненный уровень в текущем адресе с заданным уровнем редактора.
    /// Возвращает Equal, если заданный уровень соответствует требуемому.
    /// Возвращает Less, если текущем адрес является недозаполненным или совсем пустым.
    /// Возвращает true, если текущий адрес содержит лишнюю информацию.
    /// </summary>
    /// <param name="editorLevel">Уровень адреса в редакторе</param>
    /// <returns>Результат сравнения</returns>
    public FiasLevelCompareResult CompareTo(FiasEditorLevel editorLevel)
    {
      string errorText;
      return CompareTo(editorLevel, out errorText);
    }

    /// <summary>
    /// Сравнивает нижний заполненный уровень в текущем адресе с заданным уровнем редактора.
    /// Возвращает Equal, если заданный уровень соответствует требуемому.
    /// Возвращает Less, если текущем адрес является недозаполненным или совсем пустым.
    /// Возвращает true, если текущий адрес содержит лишнюю информацию.
    /// </summary>
    /// <param name="editorLevel">Уровень адреса в редакторе</param>
    /// <param name="errorText">Сюда помещается текст сообщения об ошибке</param>
    /// <returns>Результат сравнения</returns>
    public FiasLevelCompareResult CompareTo(FiasEditorLevel editorLevel, out string errorText)
    {
      FiasLevel BL = this.NameBottomLevel;
      if (BL == FiasLevel.Unknown)
      {
        errorText = "Адрес не заполнен";
        return FiasLevelCompareResult.Less;
      }
      int pBL = FiasTools.AllLevelIndexer.IndexOf(BL);
#if DEBUG
      if (pBL < 0)
        throw new BugException("Неправильный NameBottomLevel=" + BL.ToString());
#endif

      switch (editorLevel)
      {
        case FiasEditorLevel.Village:
          // Должен быть задан город или населенный пункт
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.City))
          {
            errorText = "Должен быть заполнен город или населенный пункт";
            return FiasLevelCompareResult.Less;
          }
          if (pBL > FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Village))
          {
            errorText = "Не должны заполняться уровни ниже населенного пункта";
            return FiasLevelCompareResult.Greater;
          }

          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.Street:
          // Должна быть задана улица или населенный пункт, но не в городе
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Village))
          {
            errorText = "Должен быть заполнен населенный пункт или улица";
            return FiasLevelCompareResult.Less;
          }
          if (pBL > FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Street))
          {
            errorText = "Не должны заполняться уровни ниже улицы";
            return FiasLevelCompareResult.Greater;
          }
          if (BL == FiasLevel.Village && GetName(FiasLevel.City).Length > 0)
          {
            errorText = "Если заполнен населенный пункт без улицы, то населенный пункт не может располагаться в городе";
            return FiasLevelCompareResult.Less;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.House:
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House))
          {
            errorText = "Должен быть заполнен дом и/или строение";
            return FiasLevelCompareResult.Less;
          }
          if (pBL >= FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat))
          {
            errorText = "Не должны заполняться уровни ниже дома";
            return FiasLevelCompareResult.Greater;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.Room:
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House))
          {
            errorText = "Должен быть заполнен дом или помещение";
            return FiasLevelCompareResult.Less;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;
        default:
          throw new ArgumentException("Неизестный editorLevel=" + editorLevel.ToString(), "editorLevel");
      }
    }

    /// <summary>
    /// Выполняет проверку, что верхние уровни адреса выбраны из справочника, а не заданы вручную.
    /// </summary>
    /// <param name="minRefBookLevel">Уровень, выше и включая который, должен использовать справочник.
    /// Если задано значение Unknown, то проверка не выполняется</param>
    /// <returns>true, если проверка выполнена</returns>
    public bool IsMinRefBookLevel(FiasLevel minRefBookLevel)
    {
      string errorText;
      return IsMinRefBookLevel(minRefBookLevel, out errorText);
    }

    /// <summary>
    /// Выполняет проверку, что верхние уровни адреса выбраны из справочника, а не заданы вручную.
    /// </summary>
    /// <param name="minRefBookLevel">Уровень, выше и включая который, должен использовать справочник.
    /// Если задано значение Unknown, то проверка не выполняется</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>true, если проверка выполнена</returns>
    public bool IsMinRefBookLevel(FiasLevel minRefBookLevel, out string errorText)
    {
      errorText = null;
      if (minRefBookLevel == FiasLevel.Unknown)
        return true;
      if (IsEmpty)
        return true;

#if XXX
      FiasLevel gl = this.GuidBottomLevel;
      FiasLevel nl = this.NameBottomLevel;

      if (nl == gl)
        return true; // нет компонентов, введенных вручную

      minRefBookLevel = CorrectLevel(minRefBookLevel);
      int pMin = FiasTools.AllLevelIndexer.IndexOf(minRefBookLevel);
      if (pMin < 0)
        throw new ArgumentException("Неизвестный уровень " + minRefBookLevel.ToString(), "minRefBookLevel");
      int pNL = FiasTools.AllLevelIndexer.IndexOf(nl);
      if (pNL < pMin)
      {
        minRefBookLevel = nl; // для частично заполненного адреса не надо выдавать лишние сообщения
        pMin = pNL;
      }

      if (gl == FiasLevel.Unknown)
      {
        errorText = "Все адресные элементы заданые вручную. Адресные элементы до уровня \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" должны быть выбраны из справочника ФИАС";
        return false;
      }

      int pGL = FiasTools.AllLevelIndexer.IndexOf(gl);
      if (pGL < pMin)
      {
        errorText = "Адресные элементы до уровня \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" должны быть выбраны из справочника ФИАС. В адресе из справочника выбраны только уровни до \"" +
          FiasEnumNames.ToString(gl, false) + "\" включительно";
        return false;
      }

#endif

      minRefBookLevel = CorrectLevel(minRefBookLevel);
      int pMin = FiasTools.AllLevelIndexer.IndexOf(minRefBookLevel);
      if (pMin < 0)
        throw new ArgumentException("Неизвестный уровень " + minRefBookLevel.ToString(), "minRefBookLevel");
      for (int i = 0; i <= pMin; i++)
      {
        FiasLevel lvl = FiasTools.AllLevels[i];
        string nm = GetName(lvl);
        Guid g = GetGuid(lvl);
        if (nm.Length > 0 && g == Guid.Empty)
        {
          errorText = "Адресные элементы до уровня \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" должны быть выбраны из справочника ФИАС. Адресного элемента \"" + this[lvl] + "\" уровня \"" + FiasEnumNames.ToString(lvl, false) + "\" нет в ФИАС";
          return false;
        }
      }
      return true;
    }

    #endregion
  }
}
