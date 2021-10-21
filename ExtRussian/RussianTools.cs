using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Russian
{
  /// <summary>
  /// Разнообразные статические функции для русского языка
  /// </summary>
  public static class RussianTools
  {
    #region Символы

    /// <summary>
    /// 33 буквы русского алфавита в верхнем регистре от "А" до "Я"
    /// </summary>
    public const string UpperRussianChars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

    /// <summary>
    /// Индексатор для UpperRussianChars 
    /// </summary>
    public static readonly CharArrayIndexer UpperRussianCharIndexer = new CharArrayIndexer(UpperRussianChars, false);

    /// <summary>
    /// 33 буквы русского алфавита в нижнем регистре от "а" до "я"
    /// </summary>
    public const string LowerRussianChars = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

    /// <summary>
    /// Индексатор для LowerRussianChars 
    /// </summary>
    public static readonly CharArrayIndexer LowerRussianCharIndexer = new CharArrayIndexer(LowerRussianChars, false);

    /// <summary>
    /// Индексатор для букв русского алфавита в верхнем и нижнем регистрах
    /// </summary>
    public static readonly CharArrayIndexer RussianCharIndexer = new CharArrayIndexer(UpperRussianChars + LowerRussianChars, false);


#if XXX
    /// <summary>
    /// 26 букв английского алфавита в верхнем регистре от "A" до "Z"
    /// </summary>
    public const string UpperEnglishChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// Индексатор для UpperEnglishChars 
    /// </summary>
    public static readonly CharArrayIndexer UpperEnglishCharIndexer = new CharArrayIndexer(UpperEnglishChars, false);

    /// <summary>
    /// 26 букв английского алфавита в нижнем регистре от "a" до "z"
    /// </summary>
    public const string LowerEnglishChars = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// Индексатор для LowerEnglishChars 
    /// </summary>
    public static readonly CharArrayIndexer LowerEnglishCharIndexer = new CharArrayIndexer(LowerEnglishChars, false);

    /// <summary>
    /// Индексатор для букв латинского алфавита
    /// </summary>
    public static readonly CharArrayIndexer EnglishCharIndexer = new CharArrayIndexer(UpperEnglishChars + LowerEnglishChars, false);
#endif

#if XXX
    /// <summary>
    /// Буквы русского и латинского алфавитов верхнем и нижем регистра (33+33+26+26=118 знаков)
    /// </summary>
    public const string AlphaChars =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
      "abcdefghijklmnopqrstuvwxyz" +
      "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
      "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

    private static readonly Dictionary<char, object> _AlphaCharsDict = CreateDict(AlphaChars);

    private static Dictionary<char, object> CreateDict(string s)
    {
      Dictionary<char, object> Dict = new Dictionary<char, object>(s.Length);
      for (int i = 0; i < s.Length; i++)
        Dict.Add(s[i], null);
      return Dict;
    }
#endif

    #endregion

    #region Методы ContainsXXXChar()

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну русскую букву в верхнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsUpperRussianChar(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      for (int i = 0; i < s.Length; i++)
      {
        char c = s[i];
        if (c >= 'А' && c <= 'Я')
          return true;
        if (c == 'Ё')
          return true;
      }

      return false;
    }

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну русскую букву в нижнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsLowerRussianChar(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      for (int i = 0; i < s.Length; i++)
      {
        char c = s[i];
        if (c >= 'а' && c <= 'я')
          return true;
        if (c == 'ё')
          return true;
      }

      return false;
    }

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну русскую букву в верхнем или нижнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsRussianChar(string s)
    {
      return ContainsUpperRussianChar(s) || ContainsLowerRussianChar(s);
    }

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну букву латинского алфавита в верхнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsUpperLatinChar(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      for (int i = 0; i < s.Length; i++)
      {
        char c = s[i];
        if (c >= 'A' && c <= 'Z')
          return true;
      }

      return false;
    }

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну букву латинского алфавита в нижнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsLowerLatinChar(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      for (int i = 0; i < s.Length; i++)
      {
        char c = s[i];
        if (c >= 'a' && c <= 'z')
          return true;
      }

      return false;
    }

    /// <summary>
    /// Функция возвращает true, если строка содержит хотя бы одну букву латинского алфавита в верхнем или нижнем регистре.
    /// Если строка пустая, возвращается false
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Результат проверки</returns>
    public static bool ContainsLatinChar(string s)
    {
      return ContainsUpperLatinChar(s) || ContainsLowerLatinChar(s);
    }

#if XXX
    /// <summary>
    /// Возвращает true, если строка содержит хотя бы одну букву русского или латинского алфавита
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool ContainsAlphaChar(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      for (int i = 0; i < s.Length; i++)
      {
        if (_AlphaCharsDict.ContainsKey(s[i]))
          return true;
      }
      return false;
    }
#endif

    #endregion

    #region Методы IsXXXChar()

    /// <summary>
    /// Возвращает true, если символ является заглавной русской буквой
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsUpperRussianChar(char c)
    {
      return (c >= 'А' && c <= 'Я') || c == 'Ё';
    }

    /// <summary>
    /// Возвращает true, если символ является строчной русской буквой
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsLowerRussianChar(char c)
    {
      return (c >= 'а' && c <= 'я') || c == 'ё';
    }

    /// <summary>
    /// Возвращает true, если символ является русской буквой в любом регистре
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsRussianChar(char c)
    {
      return IsUpperRussianChar(c) || IsLowerRussianChar(c);
    }

    /// <summary>
    /// Возвращает true, если символ является заглавной буквой английского алфавита
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsUpperLatinChar(char c)
    {
      return c >= 'A' && c <= 'Z';
    }

    /// <summary>
    /// Возвращает true, если символ является строочной буквой английского алфавита
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsLowerLatinChar(char c)
    {
      return c >= 'a' && c <= 'z';
    }

    /// <summary>
    /// Возвращает true, если символ является буквой английского алфавита в любом регистре
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsLatinChar(char c)
    {
      return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

#if XXX
    /// <summary>
    /// Возвращает true, если символ относится к русским или латинским буквам
    /// (входит в строку-константу AlphaChars).
    /// Для определения принадлежности символа к любым буквам, а не только латинице и кирилице, 
    /// испольщуйте стандартный метод Char.IsLetter()
    /// </summary>
    /// <param name="c">Проверяемый символ</param>
    /// <returns>Принадлежность символа к диапазону</returns>
    public static bool IsAlphaChar(char c)
    {
      return _AlphaCharsDict.ContainsKey(c);
    }
#endif

    #endregion

    #region Преобразование регистра

#if XXX

    /// <summary>
    /// Преобразует одиночный символ к верхнему регистру
    /// </summary>
    /// <param name="c">Исходный символ</param>
    /// <returns>Преобразованный символ</returns>
    [Obsolete("Используйте метод Char.ToUpper()", false)]
    public static char ToUpper(char c)
    {
      int p = LowerRussianChars.IndexOf(c);
      if (p >= 0)
        return UpperRussianChars[p];
      p = LowerEnglishChars.IndexOf(c);
      if (p >= 0)
        return UpperEnglishChars[p];
      return c;
    }

    /// <summary>
    /// Преобразует одиночный символ к нижнему регистру
    /// </summary>
    /// <param name="c">Исходный символ</param>
    /// <returns>Преобразованный символ</returns>
    [Obsolete("Используйте метод Char.ToLower()", false)]
    public static char ToLower(char c)
    {
      int p = UpperRussianChars.IndexOf(c);
      if (p >= 0)
        return LowerRussianChars[p];
      p = UpperEnglishChars.IndexOf(c);
      if (p >= 0)
        return LowerEnglishChars[p];
      return c;
    }
#endif

#if XXX
    /// <summary>
    /// Преобразует первые символы каждого слова к верхнему регистру, а остальные - к нижнему
    /// </summary>
    /// <param name="sb">Буфер, содержащий текст, который нужно преобразовать</param>
    //[Obsolete("Используйте метод DataTools.ToUpperWordsInvariant()", false)]
    internal static void ToUpperWords(StringBuilder sb)
    {
      bool NextUpper = true;
      for (int i = 0; i < sb.Length; i++)
      {
        if (IsAlphaChar(sb[i]))
        {
          if (NextUpper)
          {
            //sb[i] = ToUpper(sb[i]);
            sb[i] = Char.ToUpperInvariant(sb[i]); // 18.06.2020
            NextUpper = false;
          }
          else
          {
            //sb[i] = ToLower(sb[i]);
            sb[i] = Char.ToLowerInvariant(sb[i]); // 18.06.2020
          }
        }
        else
          NextUpper = true;
      }
    }

    /// <summary>
    /// Преобразует первые символы каждого слова к верхнему регистру, а остальные - к нижнему
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Преобразованный текст</returns>
    // [Obsolete("Используйте метод DataTools.ToUpperWordsInvariant()", false)]
    internal static string ToUpperWords(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      StringBuilder sb = new StringBuilder(s);
      ToUpperWords(sb);
      return sb.ToString();
    }
#endif

    #endregion

    #region Списки

    /// <summary>
    /// Получить строку, содержащую выражение "и", например, "Иванов, Петров и Сидоров".
    /// В строку добавляются все выражения из списка Strings, разделенные запятыми и союзом "и"
    /// по правилам грамматики русского языка.
    /// Если список пустой, то возвращается пустая строка. Если список содержит одну
    /// строку, то она и возвращается.
    /// Данная версия метода использует аргумент StringBuilder, куда дописывается строка-результат
    /// </summary>
    /// <param name="sb">Буфер, в котором формируется строка</param>
    /// <param name="strings">Массив строк, которые входят в список</param>
    public static void AndList(StringBuilder sb, List<string> strings)
    {
      for (int i = 0; i < strings.Count; i++)
      {
        if (i > 0)
        {
          if (i == strings.Count - 1)
            sb.Append(" и ");
          else
            sb.Append(", ");
        }
        sb.Append(strings[i]);
      }
    }

    /// <summary>
    /// Получить строку, содержащую выражение "и", например, "Иванов, Петров и Сидоров".
    /// В строку добавляются все выражения из списка Strings, разделенные запятыми и союзом "и"
    /// по правилам грамматики русского языка.
    /// Если список пустой, то возвращается пустая строка. Если список содержит одну
    /// строку, то она и возвращается.
    /// </summary>
    /// <param name="strings">Массив строк, которые входят в список</param>
    /// <returns>Созданная строка</returns>
    public static string AndList(List<string> strings)
    {
      if (strings.Count == 0)
        return String.Empty;
      if (strings.Count == 1)
        return strings[0];

      StringBuilder sb = new StringBuilder();
      AndList(sb, strings);
      return sb.ToString();
    }

    /// <summary>
    /// Получить строку, содержащую выражение "или", например, "Иванов, Петров или Сидоров".
    /// В строку добавляются все выражения из списка Strings, разделенные запятыми и союзом "или"
    /// по правилам грамматики русского языка.
    /// Если список пустой, то возвращается пустая строка. Если список содержит одну
    /// строку, то она и возвращается.
    /// Данная версия метода использует аргумент StringBuilder, куда дописывается строка-результат
    /// </summary>
    /// <param name="sb">Буфер, в котором формируется строка</param>
    /// <param name="strings">Массив строк, которые входят в список</param>
    public static void OrList(StringBuilder sb, List<string> strings)
    {
      for (int i = 0; i < strings.Count; i++)
      {
        if (i > 0)
        {
          if (i == strings.Count - 1)
            sb.Append(" или ");
          else
            sb.Append(", ");
        }
        sb.Append(strings[i]);
      }
    }

    /// <summary>
    /// Получить строку, содержащую выражение "или", например, "Иванов, Петров или Сидоров".
    /// В строку добавляются все выражения из списка Strings, разделенные запятыми и союзом "или"
    /// по правилам грамматики русского языка.
    /// Если список пустой, то возвращается пустая строка. Если список содержит одну
    /// строку, то она и возвращается.
    /// </summary>
    /// <param name="strings">Массив строк, которые входят в список</param>
    /// <returns>Созданная строка</returns>
    public static string OrList(List<string> strings)
    {
      if (strings.Count == 0)
        return String.Empty;
      if (strings.Count == 1)
        return strings[0];

      StringBuilder sb = new StringBuilder();
      OrList(sb, strings);
      return sb.ToString();
    }

    #endregion

    #region Названия месяцев

    /// <summary>
    /// Массив названий месяцев в именительном падеже с заглавной буквы
    /// </summary>
    public static readonly string[] MonthNames = new string[]{"Январь", "Февраль","Март","Апрель",
      "Май","Июнь","Июль","Август","Сентябрь","Октябрь","Ноябрь","Декабрь"};

    /// <summary>
    /// Массив нахваний месяцев в родительном падеже
    /// </summary>
    public static readonly string[] MonthNamesGenitive = new string[]{"января", "февраля","марта","апреля",
      "мая","июня","июля","августа","сентября","октября","ноября","декабря"};

    /// <summary>
    /// Массив сокращений названий месяцев из трех букв ("янв", "фев", ...)
    /// </summary>
    public static readonly string[] MonthNames3 = new string[]{"янв", "фев", "мар", "апр",
      "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"};

    /// <summary>
    /// Получить текстовое название месяца с заглавной буквы: "Январь", "Февраль", ...
    /// </summary>
    /// <param name="month">Номер месяца от 1 до 12.
    /// Если задано значение вне диапазона, возвращается пустая строка</param>
    /// <returns>Название месяца</returns>
    public static string GetMonthName(int month)
    {
      if (month < 1 || month > 12)
        return String.Empty;
      else
        return MonthNames[month - 1];
    }

    /// <summary>
    /// Получить текстовое название месяца в родительном падеже: "января", "февраля", ...
    /// </summary>
    /// <param name="month">Номер месяца от 1 до 12.
    /// Если задано значение вне диапазона, возвращается пустая строка</param>
    /// <returns>Название месяца</returns>
    public static string GetMonthNameGenitive(int month)
    {
      if (month < 1 || month > 12)
        return String.Empty;
      else
        return MonthNamesGenitive[month - 1];
    }

    /// <summary>
    /// Получить трехбуквенное название месяца: "янв", "фев", ...
    /// </summary>
    /// <param name="month">Номер месяца от 1 до 12.
    /// Если задано значение вне диапазона, возвращается пустая строка</param>
    /// <returns>Название месяца</returns>
    public static string GetMonthName3(int month)
    {
      if (month < 1 || month > 12)
        return String.Empty;
      else
        return MonthNames3[month - 1];
    }

    #endregion

    #region Падежи

    /// <summary>
    /// Падежи в сокращенной форме ("И.П.", "Р.П.", ..., "П.П.")
    /// </summary>
    public static readonly string[] CaseNames4 = new string[] { "И.П.", "Р.П.", "Д.П.", "В.П.", "Т.П.", "П.П." };

    #endregion
  }
}
