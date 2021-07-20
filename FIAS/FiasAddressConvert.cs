using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Parsing;

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

namespace AgeyevAV.FIAS
{
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

  /// <summary>
  /// Преобразование адреса в строку для хранения в базе данных или файлах.
  /// Строка состоит из пар код-значение и имеет формат: N1="v1",N2="v2"
  /// Использование класса:
  /// 1. Создать экземпляр FiasAddressConvert.
  /// 2. Установить свойства (важно только для записи, при считывании они игнорируются)
  /// 3. Вызвать ToString() для записи или Parse() для чтения
  /// </summary>
  public sealed class FiasAddressConvert
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
      _Handler = new FiasHandler(source);
      _GuidMode = FiasAddressConvertGuidMode.AOGuid;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к классификатору.
    /// Задается в конструкторе
    /// </summary>
    public IFiasSource Source { get { return _Handler.Source; } }
    private readonly FiasHandler _Handler;

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
        if (_Handler.Source.DBSettings.UseHouse)
        {
          lst.Add(FiasAddressConvertGuidMode.HouseGuid);
          lst.Add(FiasAddressConvertGuidMode.HouseGuidWithText);
        }
        if (_Handler.Source.DBSettings.UseRoom)
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
    /// Эта перегрузка вызывает FiasHandler.FillAddress() перед преобразованием.
    /// Если преобразуется множество адресов, рекомендуется вызывать FiasHandler.FillAddresses() в явном виде
    /// до преобразования адресов и вызывать перегрузку метода ToString() с параметром fillAddress=false.
    /// </summary>
    /// <param name="address">Адрес, который нужно преобразовать. Не может быть null</param>
    /// <returns>Кодированная строка адреса</returns>
    public string ToString(FiasAddress address)
    {
      return ToString(address, true);
    }

    /// <summary>
    /// Преобразует адрес в текстовую строку.
    /// Наличие полей определяется свойством GuidMode.
    /// Предполагается, что адрес уже заполнен полностью, то есть был вызов метода FiasHandler.FillAddress()
    /// </summary>
    /// <param name="address">Адрес, который нужно преобразовать. Не может быть null</param>
    /// <param name="fillAddress">Если true, то будет создана копия адреса и вызван метод FiasHandler.FillAddress()</param>
    /// <returns>Кодированная строка адреса</returns>
    public string ToString(FiasAddress address, bool fillAddress)
    {
#if DEBUG
      if (address == null)
        throw new ArgumentNullException("address");
#endif

      if (address.IsEmpty)
        return String.Empty;

      if (fillAddress)
      {
        address = address.Clone();
        _Handler.FillAddress(address);
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

      FiasAddressConvertGuidMode ThisGuidMode = _GuidMode & (FiasAddressConvertGuidMode.AOGuid | FiasAddressConvertGuidMode.HouseGuid | FiasAddressConvertGuidMode.RoomGuid);

      if (GuidMode == FiasAddressConvertGuidMode.GuidPreferred)
      {
        if (address.AutoPostalCode &&
        address.GuidBottomLevel == FiasTools.ReplaceToHouseOrFlat(address.NameBottomLevel))
        {
          return address.AnyGuid.ToString();
        }
        else
          ThisGuidMode = FiasAddressConvertGuidMode.RoomGuid;
      }

      List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();

      if (!address.AutoPostalCode)
        lst.Add(new KeyValuePair<string, string>("POSTALCODE", address.PostalCode));

      // С какого уровня записывается текст. Unknown - не писать, Region - писать все
      FiasLevel FirstTextLevel = FiasLevel.Unknown;
      if (ThisGuidMode == FiasAddressConvertGuidMode.RoomGuid && address.GetGuid(FiasLevel.Flat) == Guid.Empty)
        ThisGuidMode = FiasAddressConvertGuidMode.HouseGuid;
      if (ThisGuidMode == FiasAddressConvertGuidMode.HouseGuid && address.GetGuid(FiasLevel.House) == Guid.Empty)
        ThisGuidMode = FiasAddressConvertGuidMode.AOGuid;

      if (ThisGuidMode == FiasAddressConvertGuidMode.AOGuid && address.AOGuid != Guid.Empty)
      {
        lst.Add(new KeyValuePair<string, string>("AOGUID", address.AOGuid.ToString()));
        FirstTextLevel = FiasLevel.House;
        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          if (address.GetName(FiasTools.AOLevels[i]).Length > 0 && address.GetGuid(FiasTools.AOLevels[i]) == Guid.Empty)
          {
            FirstTextLevel = FiasTools.AOLevels[i];
            break;
          }
        }
      }
      else if (ThisGuidMode == FiasAddressConvertGuidMode.HouseGuid /*&& address.GetGuid(FiasLevel.House) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("HOUSEGUID", address.GetGuid(FiasLevel.House).ToString()));
        FirstTextLevel = FiasLevel.Flat;
      }
      else if (ThisGuidMode == FiasAddressConvertGuidMode.RoomGuid /*&& address.GetGuid(FiasLevel.Room) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("ROOMGUID", address.GetGuid(FiasLevel.Flat).ToString()));
        FirstTextLevel = FiasLevel.Unknown;
      }
      else
        FirstTextLevel = FiasLevel.Region;

      if ((_GuidMode & FiasAddressConvertGuidMode.Text) != 0)
        FirstTextLevel = FiasLevel.Region; // принудительная запись всех уровней

      if (FirstTextLevel != FiasLevel.Unknown)
      {
        for (int i = FiasTools.AllLevelIndexer.IndexOf(FirstTextLevel); i < FiasTools.AllLevels.Length; i++)
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

    #endregion

    #region Чтение строки

    /// <summary>
    /// Преобразование из строки в адрес.
    /// При наличии ошибок в строке выбрасывается исключение InvalidCastException.
    /// Если полученный адрес содержит ошибки, то они записываются в список FiasAddress.Messages, а не вызывают исключение.
    /// Альтернативно, строка может содержать единственный GUID вместо пар "код-значение". В этом случае он записывается в FiasAddress.UnknownGuid.
    /// </summary>
    /// <param name="s">Строка с кодами</param>
    /// <returns>Адрес</returns>
    public FiasAddress Parse(string s)
    {
      FiasAddress address;
      if (TryParse(s, out address, true))
        return address;
      else
        throw new InvalidCastException("Строку \"" + s + "\" нельзя преобразовать в адрес");
    }

    /// <summary>
    /// Преобразование из строки в адрес.
    /// При наличии ошибок в строке, исключение не выбрасывается.
    /// Если полученный адрес содержит ошибки, то они записываются в список FiasAddress.Messages, при этом возвращается true.
    /// Альтернативно, строка может содержать единственный GUID вместо пар "код-значение". В этом случае он записывается в FiasAddress.UnknownGuid.
    /// </summary>
    /// <param name="s">Строка с кодами</param>
    /// <param name="address">Сюда помещается адрес.
    /// В случае ошибки при парсинге строки, создается пустой объект (FiasAddress.IsEmpty=true)</param>
    /// <returns>Результат парсинга</returns>
    public bool TryParse(string s, out FiasAddress address)
    {
      return TryParse(s, out address, true);
    }

    /// <summary>
    /// Преобразование из строки в адрес.
    /// При наличии ошибок в строке, исключение не выбрасывается.
    /// Если полученный адрес содержит ошибки, то они записываются в список FiasAddress.Messages, при этом возвращается true.
    /// Альтернативно, строка может содержать единственный GUID вместо пар "код-значение". В этом случае он записывается в FiasAddress.UnknownGuid.
    /// </summary>
    /// <param name="s">Строка с кодами</param>
    /// <param name="address">Сюда помещается адрес.
    /// В случае ошибки при парсинге строки, создается пустой объект (FiasAddress.IsEmpty=true)</param>
    /// <param name="fillAddress">Если true, то будет выполнено заполнение всех компонентов адреса.
    /// Если false, то должен быть отдельно выполнен вызов FiasHandler.FillAddress(), если адрес не содержит ошибок.
    /// Обычно используется при массовом преобразовании адресов. Адреса добавляются в список, а затем вызывается метод FiasHandler.FillAddresses() 
    /// для быстрого заполненения адресов.</param>
    /// <returns>Результат парсинга</returns>
    public bool TryParse(string s, out FiasAddress address, bool fillAddress)
    {
      address = new FiasAddress();
      if (String.IsNullOrEmpty(s))
        return true;

      Guid g;
      if (FiasTools.TryParseGuid(s, out g))
      {
        // 21.08.2020
        address.UnknownGuid = g;
        if (fillAddress)
          _Handler.FillAddress(address);
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
        SingleScopeList<FiasLevel> MissedAOTypeLevels = null; // создадим, когда понадобится. 

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
                bool IsAOType = false;
                string key = pair.Key;
                if (key.EndsWith(".TYPE"))
                {
                  key = key.Substring(0, key.Length - 5); // без ".TYPE"
                  IsAOType = true;
                }

                FiasLevel level;
                if (!_NameDict.TryGetKey(key, out level))
                {
                  address.AddMessage(ErrorMessageKind.Error, "Неизвестный тег \"" + pair.Key + "\" в строке адреса");
                  return false; // Неизвестное имя
                }

                if (MissedAOTypeLevels == null)
                  MissedAOTypeLevels = new SingleScopeList<FiasLevel>();

                if (IsAOType)
                {
                  address.SetAOType(level, pair.Value);
                  MissedAOTypeLevels.Remove(level);
                }
                else
                {
                  address.SetName(level, pair.Value);
                  MissedAOTypeLevels.Add(level);
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
        if (MissedAOTypeLevels != null)
        {
          for (int i = 0; i < MissedAOTypeLevels.Count; i++)
          {
            string aoType = FiasTools.GetDefaultAOType(MissedAOTypeLevels[i]);
            if (aoType.Length > 0)
              address.SetAOType(MissedAOTypeLevels[i], aoType);
          }
        }

        if (address.Messages.Count == 0 && fillAddress)
          _Handler.FillAddress(address);
      }
      catch (Exception e)
      {
        address.AddMessage(ErrorMessageKind.Error, "Ошибка обработки компонентов строки адреса. " + e.Message);
        return false;
      }

      return true;
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
          case "String":
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
