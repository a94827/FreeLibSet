// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Parsing
{
  /// <summary>
  /// Парсер для распознания пробельных символов 
  /// Создает лексемы "Space". Если несколько пробелов идут подряд, создается единственная лексема
  /// </summary>
  public class SpaceParser : IParser
  {
    #region Конструкторы

    /// <summary>
    /// Создает парсер, распознаюший символы "пробел", "неразрывный пробел", "перевод строки",
    /// "возврат каретки" и "табуляция"
    /// </summary>
    public SpaceParser()
      : this(' ', DataTools.NonBreakSpaceChar, '\r', '\n', 't')
    {
    }

    /// <summary>
    /// Создает парсер, распознающий указанные символы
    /// </summary>
    /// <param name="spaceChars">Пробельные символы, которые должны распознаваться парсером</param>
    public SpaceParser(params char[] spaceChars)
    {
      _SpaceChars = spaceChars;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Распознаваемые пробельные символы.
    /// Задаются в конструкторе
    /// </summary>
    public char[] SpaceChars { get { return _SpaceChars; } }
    private char[] _SpaceChars;

    #endregion

    #region IParser Members

    /// <summary>
    /// Распознание лексемы
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    public void Parse(ParsingData data)
    {
      int cnt = 0;
      for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
      {
        char ch = data.Text.Text[i];
        if (Array.IndexOf<char>(_SpaceChars, ch) < 0)
          break;
        cnt++;
      }

      if (cnt > 0)
        data.Tokens.Add(new Token(data, this, "Space", data.CurrPos, cnt));
    }

    /// <summary>
    /// Возвращает null
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Игнорируется</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// Парсер для распознания конца строки
  /// Используется при преобразовании многострочного текста, когда конец строки является значимым символом,
  /// отличным от пробела.
  /// Создает лексему "NewLine" длиной 1 или 2 символа
  /// Распознает символы CR и LF в любых сочетаниях
  /// Если конец строки является обычным пробельным символом (как в синтаксисе C#), следует использовать
  /// SpaceParser, указав символы конца строки в списке пробельных символов
  /// </summary>
  public class NewLineParser : IParser
  {
    #region IParser Members

    /// <summary>
    /// Распознание лексемы
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    public void Parse(ParsingData data)
    {
      switch (data.GetChar(data.CurrPos))
      {
        case '\n':
        case '\r':
          int Len = 1;
          switch (data.GetChar(data.CurrPos + 1))
          {
            case '\n':
            case '\r':
              if (data.GetChar(data.CurrPos + 1) != data.GetChar(data.CurrPos)) // только если не два одинаковых символа подряд
                Len++;
              break;
          }
          data.Tokens.Add(new Token(data, this, "NewLine", data.CurrPos, Len));
          break;
      }
    }

    /// <summary>
    /// Возвращает null
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Игнорируется</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// Парсер для распознания комментариев, начинающихся с заданного символа и идущего до конца строки,
  /// например, в C# - комментарий, начинающийся с двух символов "/", в бейсике - с апострофа
  /// Маркеры конца строки не входят в лексему
  /// </summary>
  public class ToEOLCommentParser : IParser
  {
    #region Конструктор

    /// <summary>
    /// Создает парсер
    /// </summary>
    /// <param name="startString">Начальные символы</param>
    public ToEOLCommentParser(string startString)
    {
      if (String.IsNullOrEmpty(startString))
        throw new ArgumentNullException("startString");
      _StartString = startString;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Начальные символы.
    /// Задаются в конструкторе
    /// </summary>
    public string StartString { get { return _StartString; } }
    private string _StartString;

    #endregion

    #region IParser Members

    /// <summary>
    /// Распознание лексемы
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    public void Parse(ParsingData data)
    {
      if (data.StartsWith(_StartString, false))
      {
        TextPosition tp = data.Text.GetPosition(data.CurrPos);
        int cnt = data.Text.GetRowLength(tp.Row) - tp.Column;
        data.Tokens.Add(new Token(data, this, "Comment", data.CurrPos, cnt));
      }
    }

    /// <summary>
    /// Возвращает null
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Игнорируется</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }
}
