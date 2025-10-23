// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

// Убрать, когда точно ясно будет
//#define OLD_TOTYPE // Если определено, то используется первоначальный вариант методов ToTypeEx()/ToNTypeEx()

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using System.Reflection;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Функции, которые можно использовать с классами DepExprX для вычислений в удаленном пользовательском интерфейсе (RI).
  /// В RI обычно нельзя использовать делегаты на собственные пользовательские методы, если загрузка сборки с этим методом запрещена.
  /// В отличие от стандартных методов Net Framework, методы DepTools не выбрасываются исключения.
  /// Функции с суффиксом Ex возвращают готовое выражение DepExprX на основании аргументов DepValue.
  /// 
  /// Функции CreateXXX() предназначены для создания типизированных классов на основании аргумента Type
  /// </summary>
  public static class DepTools
  {
    #region Строковые функции

    #region Length()

    /// <summary>
    /// Возвращает длину строки String.Length.
    /// Может использоваться в конструкторе класса DepExpr1.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Длина строки</returns>
    private static int Length(string s)
    {
      if (Object.ReferenceEquals(s, null))
        return 0;
      else
        return s.Length;
    }

    /// <summary>
    /// Вычисляет длину строки String.Length
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> LengthEx(DepValue<string> s)
    {
      return new DepExpr1<int, string>(s, Length);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если строка непустая (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">Проверяемая строка</param>
    /// <returns>Признак непустой строки</returns>
    private static bool IsNotEmpty(string value)
    {
      return !String.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Возвращает true, если строка непустая (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">Проверяемая строка</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<string> value)
    {
      return new DepExpr1<bool, string>(value, IsNotEmpty);
    }

    #endregion

    #region Substring()

    /// <summary>
    /// Получение подстроки (метод String.Substring()).
    /// Если <paramref name="startIndex"/> и/или <paramref name="length"/> выходят за пределы строки, возвращается пустая строка.
    /// Может использоваться в конструкторе класса DepExpr3.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="length">Длина подстроки</param>
    /// <returns>Подстрока</returns>
    private static string Substring(string s, int startIndex, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (startIndex < 0 || length <= 0)
        return String.Empty;

      if (s.Length < startIndex)
        return String.Empty;

      if (s.Length < (startIndex + length))
        return s.Substring(startIndex);
      else
        return s.Substring(startIndex, length);
    }

    /// <summary>
    /// Вычисляет подстроку String.Substring().
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="length">Длина подстроки</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<string> SubstringEx(DepValue<string> s, DepValue<int> startIndex, DepValue<int> length)
    {
      return new DepExpr3<string, string, int, int>(s, startIndex, length, Substring);
    }

    /// <summary>
    /// Вычисляет подстроку String.Substring().
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="length">Длина подстроки</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<string> SubstringEx(DepValue<string> s, int startIndex, int length)
    {
      return new DepExpr3<string, string, int, int>(s, startIndex, length, Substring);
    }

    #endregion

    #region StartsWith()

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// Если <paramref name="s"/> - пустая строка, возвращается false.
    /// Если <paramref name="substring"/> - пустая строка, возвращается true.
    /// Если обе строки пустые - возвращается false.
    /// Может использоваться в конструкторе класса DepExpr2.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Признак совпадения</returns>
    private static bool StartsWithOrdinal(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.StartsWith(substring, StringComparison.Ordinal);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> StartsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinal);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> StartsWithOrdinalEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinal);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// Если <paramref name="s"/> - пустая строка, возвращается false.
    /// Если <paramref name="substring"/> - пустая строка, возвращается true.
    /// Если обе строки пустые - возвращается false.
    /// Может использоваться в конструкторе класса DepExpr2.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Признак совпадения</returns>
    private static bool StartsWithOrdinalIgnoreCase(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.StartsWith(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinalIgnoreCase);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> начинается с заданной подстроки.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinalIgnoreCase);
    }

    #endregion

    #region EndsWith()

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// Если <paramref name="s"/> - пустая строка, возвращается false.
    /// Если <paramref name="substring"/> - пустая строка, возвращается true.
    /// Если обе строки пустые - возвращается false.
    /// Может использоваться в конструкторе класса DepExpr2.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Признак совпадения</returns>
    private static bool EndsWithOrdinal(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.EndsWith(substring, StringComparison.Ordinal);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> EndsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinal);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения с учетом регистра StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> EndsWithOrdinalEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinal);
    }



    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// Если <paramref name="s"/> - пустая строка, возвращается false.
    /// Если <paramref name="substring"/> - пустая строка, возвращается true.
    /// Если обе строки пустые - возвращается false.
    /// Может использоваться в конструкторе класса DepExpr2.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Признак совпадения</returns>
    private static bool EndsWithOrdinalIgnoreCase(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.EndsWith(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinalIgnoreCase);
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> заканчивается заданной подстрокой.
    /// Используется режим сравнения без учета регистра StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="substring">Подстрока</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinalIgnoreCase);
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Преобразование в строку (метод Object.ToString()). Используется, в основном, для отладочных целей.
    /// Если <paramref name="value"/>=null, то возвращается пустая строка
    /// Может использоваться в конструкторе класса DepExpr1.
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление</returns>
    private static string ToString<T>(T value)
    {
      if (value == null)
        return String.Empty;
      else
        return value.ToString();
    }

    /// <summary>
    /// Преобразование в строку (метод Object.ToString()). Используется, в основном, для отладочных целей.
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<string> ToStringEx<T>(DepValue<T> value)
    {
      return new DepExpr1<string, T>(value, ToString);
    }

    #endregion

    #region Regex.IsMatch()

    /// <summary>
    /// Проверка соответствия строки регулярному выражению.
    /// Этот метод вызывает статический метод Regex.IsMatch(). Если аргументы <paramref name="s"/>==null или <paramref name="pattern"/>==null, то 
    /// они заменяются на пустую строку. Если вызов Regex.IsMatch() вызывает исключение (из-за неправильного <paramref name="pattern"/>), то оно перехватывается и возвращается false.
    /// Может использоваться в конструкторе класса DepExpr2.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="pattern">Регулярное выражение</param>
    /// <returns>Признак соответствия</returns>
    private static bool RegexIsMatch(string s, string pattern)
    {
      if (s == null)
        s = String.Empty;
      if (pattern == null)
        pattern = String.Empty;

      try
      {
        return System.Text.RegularExpressions.Regex.IsMatch(s, pattern);
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Проверка соответствия строки регулярному выражению.
    /// Этот метод вызывает статический метод Regex.IsMatch(). 
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="pattern">Регулярное выражение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> RegexIsMatchEx(DepValue<string> s, string pattern)
    {
      return new DepExpr2<bool, string, string>(s, pattern, RegexIsMatch);
    }

    #endregion

    #endregion

    #region DateTime

    #region Компоненты

    #region Year

    /// <summary>
    /// Возвращает год. Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Year(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Year;
      else
        return 0;
    }

    /// <summary>
    /// Возвращает год. Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> YearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Year);
    }

    /// <summary>
    /// Возвращает год. Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Year(DateTime dt)
    {
      return dt.Year;
    }

    /// <summary>
    /// Возвращает год. 
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> YearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Year);
    }

    #endregion

    #region Month

    /// <summary>
    /// Возвращает месяц (1-12). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Month(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Month;
      else
        return 0;
    }

    /// <summary>
    /// Возвращает месяц (1-12). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> MonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Month);
    }

    /// <summary>
    /// Возвращает месяц (1-12)
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Month(DateTime dt)
    {
      return dt.Month;
    }

    /// <summary>
    /// Возвращает месяц (1-12). 
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> MonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Month);
    }

    #endregion

    #region Day

    /// <summary>
    /// Возвращает день месяца (1-31). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Day(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Day;
      else
        return 0;
    }

    /// <summary>
    /// Возвращает день месяца (1-31). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> DayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Day);
    }

    /// <summary>
    /// Возвращает день месяца (1-31). 
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static int Day(DateTime dt)
    {
      return dt.Day;
    }

    /// <summary>
    /// Возвращает день месяца (1-31).
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> DayEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Day);
    }

    #endregion

    #region DayOfWeek

    /// <summary>
    /// Возвращает день недели. Если дата не задана, возвращает воскресенье
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static DayOfWeek DayOfWeek(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.DayOfWeek;
      else
        return System.DayOfWeek.Sunday;
    }

    /// <summary>
    /// Возвращает день недели. Если дата не задана, возвращает воскресенье
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DayOfWeek> DayOfWeekEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime?>(dt, DayOfWeek);
    }

    /// <summary>
    /// Возвращает день недели. 
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    private static DayOfWeek DayOfWeek(DateTime dt)
    {
      return dt.DayOfWeek;
    }

    /// <summary>
    /// Возвращает день недели.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DayOfWeek> DayOfWeekEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime>(dt, DayOfWeek);
    }

    #endregion

    #endregion

    #region IsBottom/EndOfXXX()

    #region Year

    /// <summary>
    /// Возвращает true, если дата приходится на 1 января
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsBottomOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsBottomOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 1 января
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfYear);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 1 января
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsBottomOfYear);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 31 декабря
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsEndOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsEndOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 31 декабря
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfYear);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 31 декабря
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsEndOfYear);
    }

    /// <summary>
    /// Возвращает дату 01 января.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? BottomOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.BottomOfYear(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает дату 01 января.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> BottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfYear);
    }

    /// <summary>
    /// Возвращает дату 01 января.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> BottomOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.BottomOfYear);
    }

    /// <summary>
    /// Возвращает дату 31 декабря.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? EndOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.EndOfYear(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает дату 31 декабря.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> EndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfYear);
    }


    /// <summary>
    /// Возвращает дату 31 декабря.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> EndOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.EndOfYear);
    }

    #endregion

    #region Quarter

    /// <summary>
    /// Возвращает true, если дата приходится на первый день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsBottomOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsBottomOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfQuarter);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsBottomOfQuarter);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsEndOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsEndOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfQuarter);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsEndOfQuarter);
    }

    /// <summary>
    /// Возвращает дату начала квартала.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? BottomOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.BottomOfQuarter(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает дату начала квартала.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> BottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfQuarter);
    }

    /// <summary>
    /// Возвращает дату начала квартала.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<DateTime, DateTime> BottomOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.BottomOfQuarter);
    }

    /// <summary>
    /// Возвращает дату конца квартала.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? EndOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.EndOfQuarter(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает дату конца квартала.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> EndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfQuarter);
    }

    /// <summary>
    /// Возвращает дату конца квартала.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> EndOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.EndOfQuarter);
    }

    #endregion

    #region Month

    /// <summary>
    /// Возвращает true, если дата приходится на первый день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsBottomOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsBottomOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfMonth);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsBottomOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsBottomOfMonth);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static bool IsEndOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.IsEndOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfMonth);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsEndOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, TimeTools.IsEndOfMonth);
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? BottomOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.BottomOfMonth(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> BottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfMonth);
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> BottomOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.BottomOfMonth);
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    private static DateTime? EndOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return TimeTools.EndOfMonth(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// Возвращает null, если <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> EndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfMonth);
    }

    /// <summary>
    /// Возвращает первый день месяца.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> EndOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, TimeTools.EndOfMonth);
    }

    #endregion

    #endregion

    #endregion

    #region YearMonth

    #region Компоненты

    private static int Year(YearMonth value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Year;
    }

    /// <summary>
    /// Возвращает год из структуры YearMonth.
    /// Если YearMonth.IsEmpty=true, возвращает 0.
    /// </summary>
    /// <param name="value">Управляемое исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> YearEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<int, YearMonth>(value, Year);
    }


    private static int Month(YearMonth value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Month;
    }

    /// <summary>
    /// Возвращает месяц (1-12) из структуры YearMonth.
    /// Если YearMonth.IsEmpty=true, возвращает 0.
    /// </summary>
    /// <param name="value">Управляемое исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> MonthEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<int, YearMonth>(value, Month);
    }

    #endregion

    #region NBottom/EndOfMonth()

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    private static DateTime? NBottomOfMonth(YearMonth ym)
    {
      if (ym.IsEmpty)
        return null;
      else
        return ym.BottomOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> NBottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NBottomOfMonth);
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    private static DateTime? NEndOfMonth(YearMonth ym)
    {
      if (ym.IsEmpty)
        return null;
      else
        return ym.EndOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime?> NEndOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NEndOfMonth);
    }

    #endregion

    #region Bottom/EndOfMonth()

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается 01.01.0001.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    private static DateTime BottomOfMonth(YearMonth ym)
    {
      return ym.BottomOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается 01.01.0001.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> BottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, BottomOfMonth);
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается 31.12.9999.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    private static DateTime EndOfMonth(YearMonth ym)
    {
      return ym.EndOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается 31.12.9999.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<DateTime> EndOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, EndOfMonth);
    }

    #endregion

    #region YearMonth

    /// <summary>
    /// Преобразование даты в структуру YearMonth.
    /// Для значения null возвращает YearMonth.Empty
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Год и месяц</returns>
    private static YearMonth YearMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return YearMonth(dt.Value);
      else
        return new YearMonth();
    }

    /// <summary>
    /// Преобразование даты в структуру YearMonth.
    /// Для значения null возвращает YearMonth.Empty
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<YearMonth, DateTime?>(dt, YearMonth);
    }


    /// <summary>
    /// Преобразование даты в структуру YearMonth.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Год и месяц</returns>
    private static YearMonth YearMonth(DateTime dt)
    {
      if (dt.Year >= FreeLibSet.Calendar.YearMonth.MinYear && dt.Year <= FreeLibSet.Calendar.YearMonth.MaxYear)
        return new YearMonth(dt);
      else
        return new YearMonth();
    }

    /// <summary>
    /// Преобразование даты в структуру YearMonth.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<YearMonth, DateTime>(dt, YearMonth);
    }


    private static YearMonth YearMonth(int year, int month)
    {
      if (year >= FreeLibSet.Calendar.YearMonth.MinYear && year <= FreeLibSet.Calendar.YearMonth.MaxYear &&
        month >= 1 && month <= 12)
        return new YearMonth(year, month);
      else
        return new YearMonth();
    }

    /// <summary>
    /// Создание структуры YearMonth из года и месяца (1-12).
    /// Если год или месяц имеют неправильные значения, возвращается YearMonth.Empty
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<int> year, DepValue<int> month)
    {
      return new DepExpr2<YearMonth, int, int>(year, month, YearMonth);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если значение непустое (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Значение свойства</returns>
    private static bool IsNotEmpty(YearMonth value)
    {
      return !value.IsEmpty;
    }

    /// <summary>
    /// Возвращает true, если значение непустое (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<bool, YearMonth>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region MonthDay

    #region Компоненты

    private static int Month(MonthDay value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Month;
    }

    /// <summary>
    /// Возвращает месяц (1-12) из структуры MonthDay.
    /// Если MonthDay.IsEmpty=true, возвращает 0.
    /// </summary>
    /// <param name="value">Управляемое исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> MonthEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<int, MonthDay>(value, Month);
    }


    private static int Day(MonthDay value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Day;
    }

    /// <summary>
    /// Возвращает день из структуры MonthDay.
    /// Если MonthDay.IsEmpty=true, возвращает 0.
    /// </summary>
    /// <param name="value">Управляемое исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> DayEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<int, MonthDay>(value, Day);
    }

    #endregion

    #region MonthDay

    /// <summary>
    /// Преобразование даты в структуру MonthDay.
    /// Для значения null возвращает MonthDay.Empty
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Структура</returns>
    private static MonthDay MonthDay(DateTime? dt)
    {
      if (dt.HasValue)
        return MonthDay(dt.Value);
      else
        return new MonthDay();
    }

    /// <summary>
    /// Преобразование даты в структуру MonthDay.
    /// Для значения null возвращает MonthDay.Empty
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<MonthDay, DateTime?>(dt, MonthDay);
    }


    /// <summary>
    /// Преобразование даты в структуру MonthDay.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Структура</returns>
    private static MonthDay MonthDay(DateTime dt)
    {
      return new MonthDay(dt);
    }

    /// <summary>
    /// Преобразование даты в структуру MonthDay.
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<MonthDay, DateTime>(dt, MonthDay);
    }


    private static MonthDay MonthDay(int month, int day)
    {
      if (month < 1 || month > 12)
        return new MonthDay();

      if (day < 1 || day > DateTime.DaysInMonth(2021, month)) // обязательно невисокосный год
        return new MonthDay();

      return new MonthDay(month, day);
    }

    /// <summary>
    /// Создание структуры MonthDay из месяца (1-12) и дня (1-28/30/31).
    /// Если год или месяц имеют неправильные значения, возвращается MonthDay.Empty
    /// </summary>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<int> month, DepValue<int> day)
    {
      return new DepExpr2<MonthDay, int, int>(month, day, MonthDay);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если значение непустое (MonthDay.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Значение свойства</returns>
    private static bool IsNotEmpty(MonthDay value)
    {
      return !value.IsEmpty;
    }

    /// <summary>
    /// Возвращает true, если значение непустое (MonthDay.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<bool, MonthDay>(value, IsNotEmpty);
    }

    #endregion

    #region GetDate()

    /// <summary>
    /// Возвращает дату, соответвующую заданному году.
    /// Если структура <paramref name="md"/> не инициализирована, или <paramref name="year"/> задает недопустиый год, возвращается null.
    /// </summary>
    /// <param name="md">Месяц и день</param>
    /// <param name="year">Год</param>
    /// <param name="february29">Если true и год високосный, то 28 февраля заменяется на 29</param>
    /// <returns>Дата или null</returns>
    private static DateTime? GetNDate(MonthDay md, int year, bool february29)
    {
      if (md.IsEmpty || year < DateRange.Whole.FirstDate.Year || year > DateRange.Whole.LastDate.Year)
        return null;
      else
        return md.GetDate(year, february29);
    }

    /// <summary>
    /// Возвращает дату, соответвующую заданному году.
    /// Если структура <paramref name="md"/> не инициализирована, или <paramref name="year"/> задает недопустиый год, возвращается null.
    /// </summary>
    /// <param name="md">Месяц и день</param>
    /// <param name="year">Год</param>
    /// <param name="february29">Если true и год високосный, то 28 февраля заменяется на 29</param>
    /// <returns>Дата или null</returns>
    public static DepValue<DateTime?> GetNDateEx(DepValue<MonthDay> md, DepValue<int> year, bool february29)
    {
      return new DepExpr3<DateTime?, MonthDay, int, bool>(md, year, february29, GetNDate);
    }

    /// <summary>
    /// Возвращает дату, соответвующую заданному году.
    /// Если структура <paramref name="md"/> не инициализирована, или <paramref name="year"/> задает недопустиый год, возвращается DataTime.MinValue.
    /// </summary>
    /// <param name="md">Месяц и день</param>
    /// <param name="year">Год</param>
    /// <param name="february29">Если true и год високосный, то 28 февраля заменяется на 29</param>
    /// <returns>Дата или null</returns>
    private static DateTime GetDate(MonthDay md, int year, bool february29)
    {
      if (md.IsEmpty || year < DateRange.Whole.FirstDate.Year || year > DateRange.Whole.LastDate.Year)
        return DateTime.MinValue;
      else
        return md.GetDate(year, february29);
    }

    /// <summary>
    /// Возвращает дату, соответвующую заданному году.
    /// Если структура <paramref name="md"/> не инициализирована, или <paramref name="year"/> задает недопустиый год, возвращается DataTime.MinValue.
    /// </summary>
    /// <param name="md">Месяц и день</param>
    /// <param name="year">Год</param>
    /// <param name="february29">Если true и год високосный, то 28 февраля заменяется на 29</param>
    /// <returns>Дата или null</returns>
    public static DepValue<DateTime> GetDateEx(DepValue<MonthDay> md, DepValue<int> year, bool february29)
    {
      return new DepExpr3<DateTime, MonthDay, int, bool>(md, year, february29, GetDate);
    }

    #endregion

    #endregion

    #region Nullable

    #region ReplaceNull

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#).
    /// Не имеет смысла использовать в прикладном коде
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <param name="nullValue">Замещающее значение для null</param>
    /// <returns>Значение <paramref name="value"/>.Value или <paramref name="nullValue"/>.</returns>
    private static T ReplaceNull<T>(T? value, T nullValue)
      where T : struct
    {
      return value ?? nullValue;
    }

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#).
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <param name="nullValue">Замещающее значение для null</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> ReplaceNullEx<T>(DepValue<T?> value, T nullValue)
      where T : struct
    {
      return new DepExpr2<T, T?, T>(value, nullValue, ReplaceNull);
    }

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#) на значение по умолчанию default(T).
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> ReplaceNullEx<T>(DepValue<T?> value)
      where T : struct
    {
      return new DepExpr2<T, T?, T>(value, default(T), ReplaceNull);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если есть Nullable-значение (свойство HasValue)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Признак непустой строки</returns>
    private static bool IsNotEmpty<T>(T? value)
      where T : struct
    {
      return value.HasValue;
    }

    /// <summary>
    /// Возвращает true, если есть Nullable-значение (свойство HasValue)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> IsNotEmptyEx<T>(DepValue<T?> value)
      where T : struct
    {
      return new DepExpr1<bool, T?>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region Перечисления

    #region Enum <--> Int32

    /// <summary>
    /// Преобразование перечисления в целочисленное значение
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Преобразованное значение</returns>
    private static int EnumToInt<T>(T value)
      where T : struct
    {
      return Convert.ToInt32((object)value);
    }

    /// <summary>
    /// Преобразование перечисления в целочисленное значение
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> EnumToIntEx<T>(DepValue<T> value)
      where T : struct
    {
      return new DepExpr1<int, T>(value, EnumToInt<T>);
    }

    /// <summary>
    /// Преобразование целочисленного значения в перечислимое
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Целочисленное значение</param>
    /// <returns>Преобразованное значение</returns>
    private static T EnumFromInt<T>(int value)
      where T : struct
    {
      return (T)Enum.ToObject(typeof(T), value);
    }


    /// <summary>
    /// Преобразование целочисленного значения в перечислимое
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Целочисленное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> EnumFromIntEx<T>(DepValue<int> value)
      where T : struct
    {
      return new DepExpr1<T, int>(value, EnumFromInt<T>);
    }

    #endregion

    #region Enum <--> String

    /// <summary>
    /// Преобразование перечисления в строковое значение.
    /// Выполняет простой вызов <typeparamref name="T"/>.ToString().
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Преобразованное значение</returns>
    private static string EnumToString<T>(T value)
      where T : struct
    {
      return value.ToString();
    }

    /// <summary>
    /// Преобразование перечисления в строковое значение.
    /// Выполняет простой вызов <typeparamref name="T"/>.ToString().
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<string> EnumToStringEx<T>(DepValue<T> value)
      where T : struct
    {
      return new DepExpr1<string, T>(value, EnumToString);
    }

    /// <summary>
    /// Преобразование перечисления в строковое значение.
    /// Вызывает StdConvert.TryParseEnum(). Для пустой строки или неправильной строки возвращает значение перечисления, соответствующее 0.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Преобразованное значение</returns>
    private static T EnumFromString<T>(string value)
      where T : struct
    {
      T res;
      if (StdConvert.TryParseEnum<T>(value, out res))
        return res;
      else
        return (T)Enum.ToObject(typeof(T), 0);
    }

    /// <summary>
    /// Преобразование перечисления в строковое значение.
    /// Вызывает StdConvert.TryParseEnum(). Для пустой строки или неправильной строки возвращает значение перечисления, соответствующее 0.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> EnumFromStringEx<T>(DepValue<string> value)
      where T : struct
    {
      return new DepExpr1<T, string>(value, EnumFromString<T>);
    }

    #endregion

    #endregion

    #region Массивы

    #region Length()

    /// <summary>
    /// Возвращает длину массива Array.Length.
    /// Если ссылка на массив равна null, возвращает 0.
    /// </summary>
    /// <param name="array">Массив</param>
    /// <returns>Длина массива</returns>
    private static int Length<T>(T[] array)
    {
      if (Object.ReferenceEquals(array, null))
        return 0;
      else
        return array.Length;
    }

    /// <summary>
    /// Возвращает длину массива Array.Length.
    /// Если ссылка на массив равна null, возвращает 0.
    /// </summary>
    /// <param name="array">Массив</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<int> LengthEx<T>(DepValue<T[]> array)
    {
      return new DepExpr1<int, T[]>(array, Length<T>);
    }

    #endregion

    #endregion

    #region Математические функции

    #region Min()/Max()

    #region Min()

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    private static T Min<T>(params T[] values)
      where T : IComparable<T>
    {
      if (values.Length == 0)
        throw ExceptionFactory.ArgIsEmpty("values");

      T res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i].CompareTo(res) < 0)
          res = values[i];
      }
      return res;
    }

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> MinEx<T>(params DepValue<T>[] values)
      where T : IComparable<T>
    {
      return new DepExprTA<T, T>(values, Min<T>);
    }

    /// <summary>
    /// Возвращает минимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    private static T? Min<T>(params T?[] values)
      where T : struct, IComparable<T>
    {
      T? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value.CompareTo(res.Value) < 0)
              res = values[i];
          }
          else
            res = values[i];
        }
      }
      return res;
    }

    /// <summary>
    /// Возвращает минимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T?> MinEx<T>(params DepValue<T?>[] values)
      where T : struct, IComparable<T>
    {
      return new DepExprTA<T?, T?>(values, Min);
    }

    #endregion

    #region Max()

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    private static T Max<T>(params T[] values)
      where T : IComparable<T>
    {
      if (values.Length == 0)
        throw ExceptionFactory.ArgIsEmpty("values");

      T res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i].CompareTo(res) > 0)
          res = values[i];
      }
      return res;
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> MaxEx<T>(params DepValue<T>[] values)
      where T : IComparable<T>
    {
      return new DepExprTA<T, T>(values, Max<T>);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    private static T? Max<T>(params T?[] values)
      where T : struct, IComparable<T>
    {
      T? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value.CompareTo(res.Value) > 0)
              res = values[i];
          }
          else
            res = values[i];
        }
      }
      return res;
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T?> MaxEx<T>(params DepValue<T?>[] values)
      where T : struct, IComparable<T>
    {
      return new DepExprTA<T?, T?>(values, Max);
    }

    #endregion

    #endregion

    #region InRange()

    /// <summary>
    /// Возвращает true, если значение находится в указанном диапазоне.
    /// Поддерживаются открытые и полуоткрытые интервалы.
    /// </summary>
    /// <typeparam name="T">Значимый тип (обычно, числовой) данных. Должен поддерживать интерфейс для сравнения значений</typeparam>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение или null, если ограничение не задано</param>
    /// <param name="maximum">Максимальное значение или null, если ограничение не задано</param>
    /// <returns>true, если значение находится внутри диапазона</returns>
    private static bool InRange<T>(T value, T? minimum, T? maximum)
      where T : struct, IComparable<T>
    {
      if (minimum.HasValue)
      {
        if (value.CompareTo(minimum.Value) < 0)
          return false;
      }
      if (maximum.HasValue)
      {
        if (value.CompareTo(maximum.Value) > 0)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если значение находится в указанном диапазоне.
    /// Поддерживаются открытые и полуоткрытые интервалы.
    /// </summary>
    /// <typeparam name="T">Значимый тип (обычно, числовой) данных. Должен поддерживать интерфейс для сравнения значений</typeparam>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение или null, если ограничение не задано</param>
    /// <param name="maximum">Максимальное значение или null, если ограничение не задано</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> InRangeEx<T>(DepValue<T> value, T? minimum, T? maximum)
      where T : struct, IComparable<T>
    {
      return new DepExpr3<bool, T, T?, T?>(value, minimum, maximum, InRange);
    }

    /// <summary>
    /// Возвращает true, если значение находится в указанном диапазоне.
    /// Поддерживаются открытые и полуоткрытые интервалы.
    /// </summary>
    /// <typeparam name="T">Значимый тип (обычно, числовой) данных. Должен поддерживать интерфейс для сравнения значений</typeparam>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение или null, если ограничение не задано</param>
    /// <param name="maximum">Максимальное значение или null, если ограничение не задано</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<bool> InRangeEx<T>(DepValue<T> value, DepValue<T?> minimum, DepValue<T?> maximum)
      where T : struct, IComparable<T>
    {
      return new DepExpr3<bool, T, T?, T?>(value, minimum, maximum, InRange);
    }

    #endregion

    #endregion

    #region Преобразование типов

#if OLD_TOTYPE

    private static T ToType<T>(object[] a)
    {
      if (object.ReferenceEquals(a[0], null))
        return default(T);
      else
        return (T)(Convert.ChangeType(a[0], typeof(T)));
    }

    /// <summary>
    /// Преобразование Object к заданному типу.
    /// Для преобразования используется метод Convert.ChangeType().
    /// Если исходное значение равно null, то возвращается default(T).
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому требуется преобразование</typeparam>
    /// <param name="value">Исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> ToTypeEx<T>(IDepValue value)
    {
      return new DepExprOA<T>(new IDepValue[1] { value }, ToType<T>);
    }

    private static T? ToNType<T>(object[] a)
      where T : struct
    {
      if (Object.ReferenceEquals(a[0], null))
        return null;
      else
      {
        T res = (T)(Convert.ChangeType(a[0], typeof(T)));
        return res;
      }
    }

    /// <summary>
    /// Преобразование Object к заданному Nullable-типу.
    /// Если исходное значение равно null, то возвращается null.
    /// Иначе для преобразования используется метод Convert.ChangeType().
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому требуется преобразование</typeparam>
    /// <param name="value">Исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T?> ToNTypeEx<T>(IDepValue value)
      where T : struct
    {
      return new DepExprOA<T?>(new IDepValue[1] { value }, ToNType<T>);
    }

#else

    /// <summary>
    /// Если тип T является Nullable-структурой, возвращает значимый тип, для которого объявлена структура.
    /// Иначе возвращает null
    /// </summary>
    /// <param name="t">Проверяемый тип данных</param>
    /// <returns>Базовый значимый тип или null</returns>
    private static Type GetNullableBaseType(Type t)
    {
      if (!t.IsGenericType)
        return null;

      if (t.GetGenericTypeDefinition() != typeof(Nullable<>))
        return null;

      Type[] a = t.GetGenericArguments();
#if DEBUG
      if (a.Length != 1)
        throw new BugException("GetGenericArguments() for the type " + t.ToString());
#endif
      return a[0];
    }

    #region Перечисление FormatProviderKind

    /// <summary>
    /// Используемый вариант форматировщика
    /// </summary>
    private enum FormatProviderKind
    {
      /// <summary>
      /// Нет форматировщика.
      /// Этот вариант используется, если не используется преобразование в/из строки.
      /// Например, преобразование из Int32 в Double не треует форматировщика.
      /// Также этот вариант используется для форматирования с использованием текущей культуры.
      /// При передаче объектов DepToType по сети культура может не совпадать. 
      /// Также она, в теории, может динамически меняться в результате работы панели управления.
      /// </summary>
      Null,

      /// <summary>
      /// Задана фиксированная культура (по имени). В частности, может быть задана инвариантная культура "".
      /// </summary>
      CultureInfo,

      /// <summary>
      /// Задан пользовательский объект, реализующий IFormatProvider.
      /// Если выполняется передача по сети, необходимо, чтобы этот объект был сериализуемым.
      /// </summary>
      UserDefined
    }

    #endregion

    [Serializable]
    private class DepToType<T> : DepExprOA<T>
    {
      #region Конструктор

      public DepToType(IDepValue arg, string cultureName, IFormatProvider userFormatProvider)
        : base(new IDepValue[1] { arg }, null)
      {
        _CultureName = cultureName;
        _UserFormatProvider = userFormatProvider;

        BaseSetValue(Calculate(), false);
      }

      #endregion

      #region FormatProvider

      public IFormatProvider FormatProvider
      {
        get
        {
          if (_CultureName != null) // пустая строка и null - не одно и то же
          {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo(_CultureName);
            return ci;
          }
          return _UserFormatProvider;
        }
      }

      /// <summary>
      /// Какой тип IFormatProvider используется?
      /// </summary>
      public FormatProviderKind FormatProviderKind
      {
        get
        {
          return GetFormatProviderKind(_CultureName, _UserFormatProvider);
        }
      }

      private static FormatProviderKind GetFormatProviderKind(string cultureName, IFormatProvider userFormatProvider)
      {
        if (cultureName != null)
          return DepTools.FormatProviderKind.CultureInfo;
        if (Object.ReferenceEquals(userFormatProvider, null))
          return DepTools.FormatProviderKind.Null;
        else
          return DepTools.FormatProviderKind.UserDefined;
      }

      /// <summary>
      /// Имя культуры для получения форматирования, например, "ru-RU".
      /// Null - не задано. "" - инвариантная культура
      /// </summary>
      public string CultureName { get { return _CultureName; } }
      private readonly string _CultureName;

      /// <summary>
      /// Пользовательский объект.
      /// Вряд ли будет часто использоваться
      /// </summary>
      public IFormatProvider UserFormatProvider { get { return _UserFormatProvider; } }
      private readonly IFormatProvider _UserFormatProvider;

      public bool IsSameFormatProvider(string cultureName, IFormatProvider userFormatProvider)
      {
        FormatProviderKind otherKind = GetFormatProviderKind(cultureName, userFormatProvider);
        if (otherKind != this.FormatProviderKind)
          return false;

        switch (otherKind)
        {
          case DepTools.FormatProviderKind.Null:
            return true;
          case DepTools.FormatProviderKind.CultureInfo:
            return String.Equals(cultureName, this.CultureName, StringComparison.Ordinal);
          case DepTools.FormatProviderKind.UserDefined:
            return userFormatProvider.Equals(this.UserFormatProvider); // там может быть пользовательская логика сравнения
          default:
            throw new BugException();
        }
      }

      #endregion

      #region Расчет

      protected override T Calculate()
      {
        if (object.ReferenceEquals(Args[0].Value, null))
          return default(T);

        Type t2 = DepTools.GetNullableBaseType(typeof(T));

        if (t2 == null)
        {
          try { return (T)(Convert.ChangeType(Args[0].Value, typeof(T), FormatProvider)); }
          catch { return default(T); }
        }
        else
        {
          try { return (T)(Convert.ChangeType(Args[0].Value, t2, FormatProvider)); }
          catch { return default(T); }
        }
      }

      #endregion
    }

    /// <summary>
    /// Преобразование Object к заданному типу.
    /// Для преобразования используется метод Convert.ChangeType().
    /// Если исходное значение равно null, то возвращается default(T).
    /// Работает с любыми типами, включая nullable.
    /// Если исходное вычисляемое значение уже имеет подходящий тип, оно возвращается без изменений.
    /// Если для него уже был создан преобразователь, он возвращается существующий экземпляр.
    /// В противном случае создается новый объект преобразователя.
    /// 
    /// Эта перегрузка не использует IFormatProvider при вызове метода ChangeType(). Ее следует использовать в двух
    /// случаях:
    /// 1. Преобразование не использует строки. Например, для преобразования Int32 в Double не требуется форматизатор.
    /// 2. Преобразование должно выполняться с учетом текущей культуры. При передаче объектов по сети культуры могут отличаться.
    /// Также возможно динамическоен изменение текущей культуры в процессе работы. 
    /// Использование этой перегрузки не идентично вызову перегрузки с аргументом cultureInfo=CultureInfo.CurrentCulture,
    /// так как не происходит фиксации действующего значения CultureInfo.CurrentCulture.
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому требуется преобразование</typeparam>
    /// <param name="value">Исходное значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> ToTypeEx<T>(IDepValue value)
    {
      return ToTypeEx<T>(value, null, null);
    }

    /// <summary>
    /// Преобразование Object к заданному типу.
    /// Для преобразования используется метод Convert.ChangeType().
    /// Если исходное значение равно null, то возвращается default(T).
    /// Работает с любыми типами, включая nullable.
    /// Если исходное вычисляемое значение уже имеет подходящий тип, оно возвращается без изменений.
    /// Если для него уже был создан преобразователь, он возвращается существующий экземпляр.
    /// В противном случае создается новый объект преобразователя.
    /// 
    /// Эта перегрузка, использует IFormatProvider при вызове метода ChangeType(). 
    /// Ее следует использовать для преобразования в/из строк. В частности, можно указывать инвариантную культуру.
    /// Если задана текущая культура CultureInfo.CurrentCulture, то она фиксируется. При передаче объекта по сети
    /// будет использоваться та культура, которая действовала на машине, где вызван метод ToTypeEx(), а культура
    /// на машине, выполняющей преобразование. Это поведение отличается от поведения перегрузки без аргумента <paramref name="formatProvider"/>.
    /// 
    /// Также перегрузка позволяет передавать произвольный пользовательский объект, реализующий интерфейс IFormatProvider.
    /// Если используется передача объекта, то объект должен быть сериализуемым.
    ///
    /// При работе в сети не передавайте объекты NumberFormatInfo и DateTimeFormatInfo без крайней необходимости.
    /// В отличие от CultureInfo, для них не предусмотрено оптимизированной сериализации.
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому требуется преобразование</typeparam>
    /// <param name="value">Исходное значение</param>
    /// <param name="formatProvider">Используемая культура CultureInfo при преобразовании строк или пользовательский объект
    /// Не может быть null.</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepValue<T> ToTypeEx<T>(IDepValue value, IFormatProvider formatProvider)
    {
      // Можно было бы разрешить использовать null, но тогда непонятно, что это точно означает:
      // 1. Не передавать FormatProvider совсем (плавающая CurrentCulture)
      // 2. Зафиксировать CurrentCulture и всегда использовать ее

      if (formatProvider == null)
        throw new ArgumentNullException("formatProvider");

      System.Globalization.CultureInfo cultureInfo = formatProvider as System.Globalization.CultureInfo;
      if (!Object.ReferenceEquals(cultureInfo, null))
        return ToTypeEx<T>(value, cultureInfo.Name, null);
      else
        return ToTypeEx<T>(value, null, formatProvider);
    }

    private static DepValue<T> ToTypeEx<T>(IDepValue value, string cultureName, IFormatProvider userFormatProvider)
    {
      if (value == null)
        throw new ArgumentNullException("value");

#if DEBUG
      if ((!Object.ReferenceEquals(cultureName, null)) && (!Object.ReferenceEquals(userFormatProvider, null)))
        throw new BugException();
#endif

      DepValue<T> v2 = value as DepValue<T>;
      if (v2 != null)
        return v2;

      if (!value.IsConst) // иначе будет исключение
      {
        IDepExpr[] a = value.GetChildExpressions(false);
        for (int i = 0; i < a.Length; i++)
        {
          DepToType<T> v3 = a[i] as DepToType<T>;
          if (v3 != null)
          {
            if (v3.IsSameFormatProvider(cultureName, userFormatProvider))
              return v3;
          }
        }
      }

      // Создаем новый преобразователь
      return new DepToType<T>(value, cultureName, userFormatProvider);
    }

#endif

    #endregion

    #region Методы Create()

    /// <summary>
    /// Создает экземпляр шаблонного класса DepOutput, используя механизм рефлексии.
    /// Начальным значением IDepOutput.Value будет значение по умолчанию для типа <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">Тип данных, который будет храниться в новом объекте. Должен быть задан.</param>
    /// <returns>Новый объект DepOutput</returns>
    public static IDepOutput CreateOutput(Type valueType)
    {
      if (valueType == null)
        throw new ArgumentNullException("valueType");

      Type t2 = typeof(DepOutput<>).MakeGenericType(valueType);
      ConstructorInfo ci = t2.GetConstructor(new Type[0]);
      return (IDepOutput)(ci.Invoke(EmptyArray<object>.Empty));
    }

    /// <summary>
    /// Создает экземпляр шаблонного класса DepInput, используя механизм рефлексии.
    /// Начальным значением IDepInput.Value будет значение по умолчанию для типа <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">Тип данных, который будет храниться в новом объекте. Должен быть задан.</param>
    /// <returns>Новый объект DepInput</returns>
    public static IDepInput CreateInput(Type valueType)
    {
      if (valueType == null)
        throw new ArgumentNullException("valueType");

      Type t2 = typeof(DepInput<>).MakeGenericType(valueType);
      ConstructorInfo ci = t2.GetConstructor(new Type[0]);
      return (IDepInput)(ci.Invoke(EmptyArray<object>.Empty));
    }

    /// <summary>
    /// Пустой массив объектов IDepInput.
    /// </summary>
    internal static readonly IDepInput[] EmptyInputs = new IDepInput[0];

    internal static IDepExpr[] EmptyDepExpr = new IDepExpr[0];

    #endregion
  }
}
