﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Parsing;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Выполняет парсинг строки форматирования, применяемой методом FiasHandler.Format()
  /// </summary>
  public static class FiasFormatStringParser
  {
    #region Константы TypeFormXXX

    /// <summary>
    /// Выводится имя + сокращение
    /// </summary>
    internal const int TypeFormNameAndAbbr = 0x100;

    /// <summary>
    /// Выводится имя (".NAME")
    /// </summary>
    internal const int TypeFormName = 0x200;

    /// <summary>
    /// Выводится тип объекта (".TYPE")
    /// </summary>
    internal const int TypeFormType = 0x300;

    /// <summary>
    /// Выводится сокращение (".ABBR")
    /// </summary>
    internal const int TypeFormAbbr = 0x400;

    /// <summary>
    /// Выводится номер (".NUM")
    /// </summary>
    internal const int TypeFormNum = 0x500;

    /// <summary>
    /// Выводится текст, начиная с заданного уровня
    /// </summary>
    internal const int TypeFormAt = 0x600;

    /// <summary>
    /// Устойчивый идентификатор адресного объекта.
    /// </summary>
    internal const int TypeFormGuid = 0x700;

    /// <summary>
    /// Идентификатор записи в базе данных
    /// </summary>
    internal const int TypeFormRecId = 0x800;

    /// <summary>
    /// Маска для выделения уровня объекта
    /// </summary>
    internal const int TypeLevelMask = 0xFF;

    /// <summary>
    /// Маска для выделения константы TypeFormXXX
    /// </summary>
    internal const int TypeFormMask = 0xFF00;

    #endregion

    #region Константы TypeXXX, не привязанные к уровню объекта

    /// <summary>
    /// Константа
    /// </summary>
    internal const int TypeFormConst = 0x1000;

    /// <summary>
    /// "TEXT"
    /// </summary>
    internal const int TypeFormText = 0x2000;

    /// <summary>
    /// "POSTALCODE"
    /// </summary>
    internal const int TypePostalCode = 0x3000;

    /// <summary>
    /// "REGIONCODE"
    /// </summary>
    internal const int TypeRegionCode = 0x4000;

    /// <summary>
    /// "AO.GUID"
    /// </summary>
    internal const int TypeAOGuid = 0x5000;

    /// <summary>
    /// "AO.RECID"
    /// </summary>
    internal const int TypeAORecId = 0x6000;

    /// <summary>
    /// "ANY.GUID"
    /// </summary>
    internal const int TypeAnyGuid = 0x7000;

    ///// <summary>
    ///// "ANY.RECID"
    ///// </summary>
    //internal const int TypeAnyRecId = 0x8000;

    /// <summary>
    /// "OKATO"
    /// </summary>
    internal const int TypeOKATO = 0x9000;

    /// <summary>
    /// "OKTMO"
    /// </summary>
    internal const int TypeOKTMO = 0xA000;

    /// <summary>
    /// "IFNSUL"
    /// </summary>
    internal const int TypeIFNSFL = 0xB000;

    ///// <summary>
    ///// "TERRIFNSUL"
    ///// </summary>
    //internal const int TypeTerrIFNSFL = 0xC000;

    /// <summary>
    /// "IFNSUL"
    /// </summary>
    internal const int TypeIFNSUL = 0xD000;

    ///// <summary>
    ///// "TERRIFNSUL"
    ///// </summary>
    //internal const int TypeTerrIFNSUL = 0xE000;

    #endregion

    #region Статический конструктор

    static FiasFormatStringParser()
    {
      ComponentTypes = new string[] {
        "TEXT",
        "AT.REGION",
        "AT.DISTRICT",
        "AT.CITY",
        "AT.VILLAGE",
        "AT.PS",
        "AT.STREET",
        "AT.HOUSE",

        "POSTALCODE",

        "REGION",
        "REGION.NAME",
        "REGION.TYPE",
        "REGION.ABBR",
        "REGION.GUID",
        "REGION.RECID",
        "REGIONCODE",

        "DISTRICT",
        "DISTRICT.NAME",
        "DISTRICT.TYPE",
        "DISTRICT.ABBR",
        "DISTRICT.GUID",
        "DISTRICT.RECID",
        
        "CITY",
        "CITY.NAME",
        "CITY.TYPE",
        "CITY.ABBR",
        "CITY.GUID",
        "CITY.RECID",
        
        "VILLAGE",
        "VILLAGE.NAME",
        "VILLAGE.TYPE",
        "VILLAGE.ABBR",
        "VILLAGE.GUID",
        "VILLAGE.RECID",
        
        "PS",
        "PS.NAME",
        "PS.TYPE",
        "PS.ABBR",
        "PS.GUID",
        "PS.RECID",
        
        "STREET",
        "STREET.NAME",
        "STREET.TYPE",
        "STREET.ABBR",
        "STREET.GUID",
        "STREET.RECID",
        
        "AO.GUID",
        "AO.RECID",
        
        "HOUSE",
        "HOUSE.NAME",
        "HOUSE.NUM",
        "HOUSE.TYPE",
        "HOUSE.ABBR",
        "BUILD.NUM",
        "BUILD.TYPE",
        "BUILD.ABBR",
        "STR.NUM",
        "STR.TYPE",
        "STR.ABBR",
        "HOUSE.GUID",
        "HOUSE.RECID",
        
        "ROOM",
        "ROOM.NAME",
        "FLAT.NUM",
        "FLAT.TYPE",
        "FLAT.ABBR",
        "ROOM.NUM",
        "ROOM.TYPE",
        "ROOM.ABBR",
        "ROOM.GUID",
        "ROOM.RECID",
        
        "ANY.GUID",
        //"ANY.RECID",
        
        "OKATO",
        "OKTMO",
        "IFNSFL",
        //"TERRIFNSFL",
        "IFNSUL",
        //"TERRIFNSUL",
      };

      _ComponentTypeValues = new int[] {
        TypeFormText,
        TypeFormAt | (int)FiasLevel.Region,
        TypeFormAt | (int)FiasLevel.District,
        TypeFormAt | (int)FiasLevel.City,
        TypeFormAt | (int)FiasLevel.Village,
        TypeFormAt | (int)FiasLevel.PlanningStructure,
        TypeFormAt | (int)FiasLevel.Street,
        TypeFormAt | (int)FiasLevel.House,

        TypePostalCode,

        TypeFormNameAndAbbr | (int)FiasLevel.Region,
        TypeFormName | (int)FiasLevel.Region,
        TypeFormType | (int)FiasLevel.Region,
        TypeFormAbbr | (int)FiasLevel.Region,
        TypeFormGuid | (int)FiasLevel.Region,
        TypeFormRecId | (int)FiasLevel.Region,
        TypeRegionCode,

        TypeFormNameAndAbbr | (int)FiasLevel.District,
        TypeFormName | (int)FiasLevel.District,
        TypeFormType | (int)FiasLevel.District,
        TypeFormAbbr | (int)FiasLevel.District,
        TypeFormGuid | (int)FiasLevel.District,
        TypeFormRecId | (int)FiasLevel.District,

        TypeFormNameAndAbbr | (int)FiasLevel.City,
        TypeFormName | (int)FiasLevel.City,
        TypeFormType | (int)FiasLevel.City,
        TypeFormAbbr | (int)FiasLevel.City,
        TypeFormGuid | (int)FiasLevel.City,
        TypeFormRecId | (int)FiasLevel.City,

        TypeFormNameAndAbbr | (int)FiasLevel.Village,
        TypeFormName | (int)FiasLevel.Village,
        TypeFormType | (int)FiasLevel.Village,
        TypeFormAbbr | (int)FiasLevel.Village,
        TypeFormGuid | (int)FiasLevel.Village,
        TypeFormRecId | (int)FiasLevel.Village,

        TypeFormNameAndAbbr | (int)FiasLevel.PlanningStructure,
        TypeFormName | (int)FiasLevel.PlanningStructure,
        TypeFormType | (int)FiasLevel.PlanningStructure,
        TypeFormAbbr | (int)FiasLevel.PlanningStructure,
        TypeFormGuid | (int)FiasLevel.PlanningStructure,
        TypeFormRecId | (int)FiasLevel.PlanningStructure,

        TypeFormNameAndAbbr | (int)FiasLevel.Street,
        TypeFormName | (int)FiasLevel.Street,
        TypeFormType | (int)FiasLevel.Street,
        TypeFormAbbr | (int)FiasLevel.Street,
        TypeFormGuid | (int)FiasLevel.Street,
        TypeFormRecId | (int)FiasLevel.Street,

        TypeAOGuid,
        TypeAORecId,

        TypeFormNameAndAbbr | (int)FiasLevel.House,
        TypeFormName | (int)FiasLevel.House,
        TypeFormNum | (int)FiasLevel.House,
        TypeFormType | (int)FiasLevel.House,
        TypeFormAbbr | (int)FiasLevel.House,
        TypeFormNum | (int)FiasLevel.Building,
        TypeFormType | (int)FiasLevel.Building,
        TypeFormAbbr | (int)FiasLevel.Building,
        TypeFormNum | (int)FiasLevel.Structure,
        TypeFormType | (int)FiasLevel.Structure,
        TypeFormAbbr | (int)FiasLevel.Structure,
        TypeFormGuid | (int)FiasLevel.House,
        TypeFormRecId | (int)FiasLevel.House,

        TypeFormNameAndAbbr | (int)FiasLevel.Room,
        TypeFormName | (int)FiasLevel.Room,
        TypeFormNum | (int)FiasLevel.Flat,
        TypeFormType | (int)FiasLevel.Flat,
        TypeFormAbbr | (int)FiasLevel.Flat,
        TypeFormNum | (int)FiasLevel.Room,
        TypeFormType | (int)FiasLevel.Room,
        TypeFormAbbr | (int)FiasLevel.Room,
        TypeFormGuid | (int)FiasLevel.Room,
        TypeFormRecId | (int)FiasLevel.Room,
      
        TypeAnyGuid,
        //TypeAnyRecId,

        TypeOKATO,
        TypeOKTMO,
        TypeIFNSFL,
        //TypeTerrIFNSFL,
        TypeIFNSUL,
        //TypeTerrIFNSUL
      };

#if DEBUG
      if (_ComponentTypeValues.Length != ComponentTypes.Length)
        throw new BugException("ComponentTypeValues.Length!=ComponentTypes.Length");
#endif


      _ComponentTypeIndexer = new StringArrayIndexer(ComponentTypes, false);

      _TheParserList1 = new ParserList();
      _TheParserList1.Add(new TypeParser());
      _TheParserList1.Add(new ComplexSepParser());
      _TheParserList1.Add(new SimpleSepParser()); // после ComplexSepParser
      _TheParserList1.Add(new BraceParser());

      _TheParserList2 = new ParserList();
      _TheParserList2.Add(new TypeParser());
      _TheParserList2.Add(new StrConstParser());
      _TheParserList2.Add(new SpaceParser());
      _TheParserList2.Add(new BraceParser()); // для завершающей фигурной скобки
    }

    #endregion

    #region Константы

    /// <summary>
    /// Список допустимых типов компонентов адреса
    /// </summary>
    public static readonly string[] ComponentTypes;

    private static readonly StringArrayIndexer _ComponentTypeIndexer;

    /// <summary>
    /// Значения для поля FiasParsedFormatString.ItemType, соответствующие массиву ComponentTypes
    /// </summary>
    private static readonly int[] _ComponentTypeValues;

    /// <summary>
    /// Формат, используемый по умолчанию
    /// </summary>
    public static string DefaultFormat = "TEXT";

    #endregion

    #region Парсинг

    /// <summary>
    /// Выполняет парсинг строки форматирования и возвращает внутренний объект FiasParsedFormatString.
    /// В случае неудачи генерируется исключение
    /// </summary>
    /// <param name="format">Строка формата</param>
    /// <returns>Результат парсинга - внутренний объект FiasParsedFormatString</returns>
    public static FiasParsedFormatString Parse(string format)
    {
      FiasParsedFormatString parsedFormat;
      string errorMessage;
      int errorStart, errorLen;
      if (TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen))
        return parsedFormat;
      else
        throw new FormatException(errorMessage);
    }

    /// <summary>
    /// Выполняет парсинг строки форматирования и создает внутренний объект FiasParsedFormatString в случае успеха.
    /// </summary>
    /// <param name="format">Строка формата</param>
    /// <param name="parsedFormat">Сюда записывается результат парсинга - внутренний объект FiasParsedFormatString в случае успеха</param>
    /// <param name="errorMessage">Сюда записывается текст сообщения об ошибке в случае неудачи</param>
    /// <returns>true, если парсинг успешно выполнен</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage)
    {
      int errorStart;
      int errorLen;
      ParsingData pd;
      return TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen, out pd);
    }

    /// <summary>
    /// Выполняет парсинг строки форматирования и создает внутренний объект FiasParsedFormatString в случае успеха.
    /// </summary>
    /// <param name="format">Строка формата</param>
    /// <param name="parsedFormat">Сюда записывается результат парсинга - внутренний объект FiasParsedFormatString в случае успеха</param>
    /// <param name="errorMessage">Сюда записывается текст сообщения об ошибке в случае неудачи</param>
    /// <param name="errorStart">В случае ошибки сюда помещается начало ошибочной лексемы</param>
    /// <param name="errorLen">В случае ошибки сюда помещается длина ошибочной лексемы</param>
    /// <returns>true, если парсинг успешно выполнен</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage, out int errorStart, out int errorLen)
    {
      ParsingData pd;
      return TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen, out pd);
    }

    /// <summary>
    /// Выполняет парсинг строки форматирования и создает внутренний объект FiasParsedFormatString в случае успеха.
    /// Эта перегрузка предназначена для отладочных целей и возвращает объект парсинга строки
    /// </summary>
    /// <param name="format">Строка формата</param>
    /// <param name="parsedFormat">Сюда записывается результат парсинга - внутренний объект FiasParsedFormatString в случае успеха</param>
    /// <param name="errorMessage">Сюда записывается текст сообщения об ошибке в случае неудачи</param>
    /// <param name="errorStart">В случае ошибки сюда помещается начало ошибочной лексемы</param>
    /// <param name="errorLen">В случае ошибки сюда помещается длина ошибочной лексемы</param>
    /// <param name="pd">Сюда помещаются данные парсинга</param>
    /// <returns>true, если парсинг успешно выполнен</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage, out int errorStart, out int errorLen, out ParsingData pd)
    {
      pd = new ParsingData(format);
      _TheParserList1.Parse(pd);

      if (pd.FirstErrorToken == null)
        parsedFormat = TryParse2(pd);
      else
        parsedFormat = null;

      if (pd.FirstErrorToken != null)
      {
        errorMessage = pd.FirstErrorToken.ErrorMessage.Value.Text;
        errorStart = pd.FirstErrorToken.Start;
        errorLen = pd.FirstErrorToken.Length;
        return false;
      }
      else
      {
        errorMessage = null;
        errorStart = 0;
        errorLen = 0;
        return true;
      }
    }

    private static FiasParsedFormatString TryParse2(ParsingData pd)
    {
      Token currSepToken = null;
      FiasParsedFormatString res = new FiasParsedFormatString();
      FiasParsedFormatString.FormatItem currItem = new FiasParsedFormatString.FormatItem();
      Token openBracesToken = null;

      for (int i = 0; i < pd.Tokens.Count; i++)
      {
        Token token = pd.Tokens[i];
        switch (token.TokenType)
        {
          case "Space":
            continue;

          case "{":
            if (openBracesToken != null)
            {
              token.SetError("Вложенные фигурные скобки не допускаются");
              return null;
            }
            openBracesToken = token;
            currItem = new FiasParsedFormatString.FormatItem();
            break;

          case "}":
            if (openBracesToken == null)
            {
              token.SetError("Не было открывающей скобки");
              return null;
            }
            openBracesToken = null;
            if (currItem.ItemType == 0)
            {
              if (currItem.Suffix != null)
              {
                token.SetError("Внутренняя ошибка. Есть суффикс, но нет типа компонента");
                return null;
              }
              if (currItem.Prefix == null)
              {
                token.SetError("Пустой компонент");
                return null;
              }
              currItem.ItemType = TypeFormConst;
            }
            if (currSepToken != null)
            {
              currItem.Separator = currSepToken.AuxData.ToString();
              currSepToken = null;
            }
            res.Items.Add(currItem);
            break;

          case "Type":
            string sType = token.AuxData.ToString();
            int pType = _ComponentTypeIndexer.IndexOf(sType);
            if (pType < 0)
            {
              if (sType.Length == 0)
                token.SetError("Внутренняя ошибка. Тип компонента имеет нулевую длину");
              else if (sType[0] == '.' || sType[sType.Length - 1] == '.')
                token.SetError("Тип компонента не может начинаться или заканчиваться на точку");
              else if (sType.IndexOf("..", StringComparison.Ordinal) >= 0)
                token.SetError("В типе компонента не могут быть две точки подряд");
              else
                token.SetError("Неизвестный тип компонента \"" + sType + "\"");
              return null;
            }
            if (openBracesToken != null)
            {
              if (currItem.ItemType != 0)
              {
                token.SetError("Внутри фигурных скобок может быть только один тип компонента");
                return null;
              }

              currItem.ItemType = _ComponentTypeValues[pType];
            }
            else
            {
              // Одиночный тип вне фигурных скобок
              currItem = new FiasParsedFormatString.FormatItem();
              currItem.ItemType = _ComponentTypeValues[pType];
              if (currSepToken != null)
              {
                currItem.Separator = currSepToken.AuxData.ToString();
                currSepToken = null;
              }
              res.Items.Add(currItem);
            }
            break;

          case "String":
            if (openBracesToken == null)
            {
              token.SetError("Внутренняя ошибка. Строка вне фигурных скобок");
              return null;
            }
            if (currItem.ItemType == 0)
            {
              if (currItem.Prefix != null)
              {
                token.SetError("Два префикса подряд не допускаются");
                return null;
              }
              currItem.Prefix = token.AuxData.ToString();
            }
            else
            {
              if (currItem.Suffix != null)
              {
                token.SetError("Два суффикса подряд не допускаются");
                return null;
              }
              currItem.Suffix = token.AuxData.ToString();
            }
            break;

          case "Sep":
            if (currSepToken != null)
            {
              token.SetError("Не могут идти два разделителя подряд");
              return null;
            }
            currSepToken = token;
            if (openBracesToken != null)
            {
              token.SetError("Внутренняя ошибка. Обнаружен разделитель внутри фигурных скобок");
              return null;
            }
            if (res.Items.Count == 0)
            {
              token.SetError("Разделитель не может идти до первого компонента");
              return null;
            }
            break;

          default:
            token.SetError("Внутренняя ошибка. Неизвестная лексема \"" + token.TokenType + "\"");
            return null;
        }
      }

      if (openBracesToken != null)
      {
        openBracesToken.SetError("Не найдена парная закрывающая скобка");
        return null;
      }

      if (currSepToken != null)
      {
        currSepToken.SetError("Разделитель не может идти после последнего компонента");
        return null;
      }

      return res;
    }

    /// <summary>
    /// Проверка корректности строки формата.
    /// Отличается от TryParse() только тем, что не возвращает объект FiasParsedFormatString.
    /// </summary>
    /// <param name="format">Проверяемая строка форматирования</param>
    /// <param name="errorMessage">Сюда помещается сообщение об ошибке, если формат неправильный</param>
    /// <returns>true, если строка форматирования является допустимой.</returns>
    public static bool IsValidFormat(string format, out string errorMessage)
    {
      FiasParsedFormatString parsedFormat;
      return TryParse(format, out parsedFormat, out errorMessage);
    }

    /// <summary>
    /// Проверка корректности строки формата.
    /// Отличается от TryParse() только тем, что не возвращает объект FiasParsedFormatString.
    /// </summary>
    /// <param name="format">Проверяемая строка форматирования</param>
    /// <returns>true, если строка форматирования является допустимой.</returns>
    public static bool IsValidFormat(string format)
    {
      FiasParsedFormatString parsedFormat;
      string errorMessage;
      return TryParse(format, out parsedFormat, out errorMessage);
    }

    #endregion

    #region Классы парсеров

    /// <summary>
    /// Основной список парсеров
    /// </summary>
    private static readonly ParserList _TheParserList1;

    /// <summary>
    /// Дополнительный список парсеров, применяемый внутри фигурных скобок
    /// </summary>
    private static readonly ParserList _TheParserList2;

    /// <summary>
    /// Парсинг выводимого типа - последовательность латинских символов и точки
    /// </summary>
    private class TypeParser : IParser
    {
      #region IParser Members

      private static readonly CharArrayIndexer _ValidCharIndexer = new CharArrayIndexer("ABCDEFGHIJKLMNOPQRSTUVWXYZ.", false);

      public void Parse(ParsingData data)
      {
        int cnt = 0;
        for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
        {
          if (_ValidCharIndexer.Contains(data.GetChar(i)))
            cnt++;
          else
            break;
        }
        if (cnt > 0)
          data.Tokens.Add(new Token(data, this, "Type", data.CurrPos, cnt, data.Text.Text.Substring(data.CurrPos, cnt)));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Парсинг разделителя в кавычках
    /// </summary>
    private class ComplexSepParser : IParser
    {
      #region IParser Members

      void IParser.Parse(ParsingData data)
      {
        #region Ведущие пробелы

        // Не будем ориентироваться на свойство ParsingData.CurrPos, т.к. будет ли оно обновляться при вызове Tokens.Add(), зависит от реализации.
        // А здесь может потребоваться добавить не одну лексему, а три (Space, Sep, Space)
        int currPos = data.CurrPos;

        int cntSpace1 = 0;
        for (int i = currPos; i < data.Text.Text.Length; i++)
        {
          if (data.GetChar(i) == ' ')
            cntSpace1++;
          else
            break;
        }

        if (data.GetChar(currPos + cntSpace1) != '\"')
          return; // если не кавычка, то это не наше

        if (cntSpace1 > 0)
        {
          data.Tokens.Add(new Token(data, this, "Space", currPos, cntSpace1));
          currPos += cntSpace1;
        }

        #endregion

        #region Текст в кавычках

        StringBuilder sb = new StringBuilder();
        bool quoteFound = false;
        int nextpos = currPos + 1;
        while (nextpos < data.Text.Text.Length)
        {
          if (data.GetChar(nextpos) == '\"')
          {
            if (data.GetChar(nextpos + 1) == '\"')
            {
              // Задвоенная кавычка
              nextpos += 2;
              sb.Append('\"');
            }
            else
            {
              // Конец разделителя
              nextpos++;
              quoteFound = true;
              break;
            }
          }
          else
          {
            // Обычный символ
            sb.Append(data.GetChar(nextpos));
            nextpos++;
          }
        }

        Token sepToken = new Token(data, this, "Sep", currPos, nextpos - currPos, sb.ToString());
        data.Tokens.Add(sepToken);
        currPos += sepToken.Length;
        if (!quoteFound)
        {
          sepToken.SetError("Не найдена завершающая кавычка разделителя");
          return;
        }

        #endregion

        #region Завершающие пробелы

        int cntSpace2 = 0;
        for (int i = currPos; i < data.Text.Text.Length; i++)
        {
          if (data.GetChar(i) == ' ')
            cntSpace2++;
          else
            break;
        }

        if (cntSpace2 > 0)
          data.Tokens.Add(new Token(data, this, "Space", currPos, cntSpace2));

        #endregion
      }

      IExpression IParser.CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Парсинг простого разделителя (без кавычек)
    /// </summary>
    private class SimpleSepParser : IParser
    {
      #region IParser Members

      private static readonly CharArrayIndexer ValidCharIndexer = new CharArrayIndexer(" .-,;/", false);

      public void Parse(ParsingData data)
      {
        int cnt = 0;
        for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
        {
          if (ValidCharIndexer.Contains(data.GetChar(i)))
            cnt++;
          else
            break;
        }
        if (cnt > 0)
          data.Tokens.Add(new Token(data, this, "Sep", data.CurrPos, cnt, data.Text.Text.Substring(data.CurrPos, cnt)));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    private class BraceParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        switch (data.GetChar(data.CurrPos))
        {
          case '{':
            data.Tokens.Add(new Token(data, this, "{", data.CurrPos, 1));
            _TheParserList2.SubParse(data, new string[] { "}" });
            break;
          case '}':
            data.Tokens.Add(new Token(data, this, "}", data.CurrPos, 1));
            break;
        }
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    #endregion

    #region Форматы для отдельных уровней

    private static Dictionary<FiasLevel, FiasParsedFormatString> _LevelFormats;

    private static Dictionary<FiasLevel, FiasParsedFormatString> InitLevelFormats()
    {
      Dictionary<FiasLevel, FiasParsedFormatString> dict = new Dictionary<FiasLevel, FiasParsedFormatString>();
      dict.Add(FiasLevel.Region, Parse("REGION"));
      dict.Add(FiasLevel.District, Parse("DISTRICT"));
      dict.Add(FiasLevel.City, Parse("CITY"));
      dict.Add(FiasLevel.Village, Parse("VILLAGE"));
      dict.Add(FiasLevel.PlanningStructure, Parse("PS"));
      dict.Add(FiasLevel.Street, Parse("STREET"));
      dict.Add(FiasLevel.House, Parse("HOUSE.ABBR HOUSE.NUM"));
      dict.Add(FiasLevel.Building, Parse("BUILD.ABBR BUILD.NUM"));
      dict.Add(FiasLevel.Structure, Parse("STR.ABBR STR.NUM"));
      dict.Add(FiasLevel.Flat, Parse("FLAT.ABBR FLAT.NUM"));
      dict.Add(FiasLevel.Room, Parse("ROOM.ABBR ROOM.NUM"));

      return dict;
    }


    internal static FiasParsedFormatString GetFormat(FiasLevel level)
    {
      if (_LevelFormats == null)
        _LevelFormats = InitLevelFormats();

      FiasParsedFormatString res;
      if (_LevelFormats.TryGetValue(level, out res))
        return res;
      else
        throw new ArgumentException("Неизвестный уровень: " + level.ToString(), "level");
    }

    #endregion
  }

  /// <summary>
  /// Результат парсинга строки форматирования методом FiasFormatStringParser.Parse().
  /// Этот класс не имеет открытых членов.
  /// Объект результата парсинга можно передавать соответствующей перегрузке метода FiasHandler.Format().
  /// </summary>
  public sealed class FiasParsedFormatString
  {
    #region Защищенный конструктор

    internal FiasParsedFormatString()
    {
      _Items = new List<FormatItem>();
    }

    #endregion

    #region Компоненты

    internal struct FormatItem
    {
      #region Поля

      /// <summary>
      /// Разделитель, выводимый перед этим компонентом
      /// </summary>
      public string Separator;

      /// <summary>
      /// Префикс.
      /// Для компонента-константы сюда помещается константное значение
      /// </summary>
      public string Prefix;

      ///// <summary>
      ///// Код компонента.
      ///// Для компонента-константы имеет значение "CONST".
      ///// </summary>
      //public string ItemType;

      /// <summary>
      /// Числовой код компонента.
      /// Состоит из комбинации TypeFormXXX (константы) и (int)FiasLevel
      /// Для компонента-константы имеет значение TypeConst.
      /// </summary>
      public int ItemType;


      /// <summary>
      /// Суффикс
      /// </summary>
      public string Suffix;

      #endregion
    }

    internal List<FormatItem> Items { get { return _Items; } }
    private List<FormatItem> _Items;

    #endregion
  }
}
