using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Функции, которые можно использовать с классами DepExprX для вычислений в удаленном пользовательском интерфейсе (RI).
  /// В RI обычно нельзя использовать делегаты на собственные пользовательские методы, если загрузка сборки с этим методом запрещена.
  /// В отличие от стандартных методов Net Framework, методы DepTools не выбрасываются исключения.
  /// Для каждой функции существует парная функция с суффиксом Ex, которая возвращает готовое выражение DepExprX на основании аргументов DepValue.
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
    public static int Length(string s)
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
    public static DepExpr1<int, string> LengthEx(DepValue<string> s)
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
    public static bool IsNotEmpty(string value)
    {
      return !String.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Возвращает true, если строка непустая (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">Проверяемая строка</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, string> IsNotEmptyEx(DepValue<string> value)
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
    public static string Substring(string s, int startIndex, int length)
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
    public static DepExpr3<string, string, int, int> SubstringEx(DepValue<string> s, DepValue<int> startIndex, DepValue<int> length)
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
    public static DepExpr3<string, string, int, int> SubstringEx(DepValue<string> s, int startIndex, int length)
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
    public static bool StartsWithOrdinal(string s, string substring)
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
    public static DepExpr2<bool, string, string> StartsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepExpr2<bool, string, string> StartsWithOrdinalEx(DepValue<string> s, string substring)
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
    public static bool StartsWithOrdinalIgnoreCase(string s, string substring)
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
    public static DepExpr2<bool, string, string> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepExpr2<bool, string, string> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
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
    public static bool EndsWithOrdinal(string s, string substring)
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
    public static DepExpr2<bool, string, string> EndsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepExpr2<bool, string, string> EndsWithOrdinalEx(DepValue<string> s, string substring)
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
    public static bool EndsWithOrdinalIgnoreCase(string s, string substring)
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
    public static DepExpr2<bool, string, string> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepExpr2<bool, string, string> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
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
    public static string ToString<T>(T value)
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
    public static DepExpr1<string, T> ToStringEx<T>(DepValue<T> value)
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
    public static bool RegexIsMatch(string s, string pattern)
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
    public static DepExpr2<bool, string, string> RegexIsMatchEx(DepValue<string> s, string pattern)
    {
      return new DepExpr2<bool, string, string>(s, pattern, RegexIsMatch);
    }

    #endregion

    #endregion

    #region DateTime

    #region Компоненты

    /// <summary>
    /// Возвращает год. Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    public static int Year(DateTime? dt)
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
    public static DepExpr1<int, DateTime?> YearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Year);
    }

    /// <summary>
    /// Возвращает месяц (1-12). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    public static int Month(DateTime? dt)
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
    public static DepExpr1<int, DateTime?> MonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Month);
    }

    /// <summary>
    /// Возвращает день месяца (1-31). Если дата не задана, возвращает 0
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    public static int Day(DateTime? dt)
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
    public static DepExpr1<int, DateTime?> DayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Day);
    }

    /// <summary>
    /// Возвращает день недели. Если дата не задана, возвращает воскресенье
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Значение компонента</returns>
    public static DayOfWeek DayOfWeek(DateTime? dt)
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
    public static DepExpr1<DayOfWeek, DateTime?> DayOfWeekEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime?>(dt, DayOfWeek);
    }

    #endregion

    #region IsBottom/EndOfXXX()

    /// <summary>
    /// Возвращает true, если дата приходится на 1 января
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsBottomOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 1 января
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfYear);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 31 декабря
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsEndOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на 31 декабря
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfYear);
    }


    /// <summary>
    /// Возвращает true, если дата приходится на первый день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsBottomOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfMonth);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsEndOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день месяца
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfMonth);
    }


    /// <summary>
    /// Возвращает true, если дата приходится на первый день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsBottomOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на первый день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfQuarter);
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Результат проверки</returns>
    public static bool IsEndOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// Возвращает true, если дата приходится на последний день квартала
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfQuarter);
    }

    #endregion

    #region Min()/Max()

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static DateTime Min(params DateTime[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      DateTime res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] < res)
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
    public static DepExprTA<DateTime, DateTime> MinEx(params DepValue<DateTime>[] values)
    {
      return new DepExprTA<DateTime, DateTime>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static DateTime Max(params DateTime[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      DateTime res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] > res)
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
    public static DepExprTA<DateTime, DateTime> MaxEx(params DepValue<DateTime>[] values)
    {
      return new DepExprTA<DateTime, DateTime>(values, Max);
    }

    /// <summary>
    /// Возвращает минимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static DateTime? Min(params DateTime?[] values)
    {
      DateTime? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value < res.Value)
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
    public static DepExprTA<DateTime?, DateTime?> MinEx(params DepValue<DateTime?>[] values)
    {
      return new DepExprTA<DateTime?, DateTime?>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Значения null пропускаются.
    /// Если список аргументов пустой или не содержит значений, отличных от null - возвращается null.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static DateTime? Max(params DateTime?[] values)
    {
      DateTime? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value > res.Value)
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
    public static DepExprTA<DateTime?, DateTime?> MaxEx(params DepValue<DateTime?>[] values)
    {
      return new DepExprTA<DateTime?, DateTime?>(values, Max);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если есть Nullable-значение (свойство HasValue)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Признак непустой строки</returns>
    public static bool IsNotEmpty(DateTime? value)
    {
      return value.HasValue;
    }

    /// <summary>
    /// Возвращает true, если строка непустая (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">Проверяемая строка</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, DateTime?> IsNotEmptyEx(DepValue<DateTime?> value)
    {
      return new DepExpr1<bool, DateTime?>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region YearMonth

    #region NBottom/EndOfMonth()

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    public static DateTime? NBottomOfMonth(YearMonth ym)
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
    public static DepExpr1<DateTime?, YearMonth> NBottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NBottomOfMonth);
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается null
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    public static DateTime? NEndOfMonth(YearMonth ym)
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
    public static DepExpr1<DateTime?, YearMonth> NEndOfMonthEx(DepValue<YearMonth> ym)
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
    public static DateTime BottomOfMonth(YearMonth ym)
    {
        return ym.BottomOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую первому дню месяца.
    /// Для YearMonth.Empty возвращается 01.01.0001.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<DateTime, YearMonth> BottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, BottomOfMonth);
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается 31.12.9999.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Дата</returns>
    public static DateTime EndOfMonth(YearMonth ym)
    {
      return ym.EndOfMonth;
    }

    /// <summary>
    /// Возвращает дату, соответствующую последнему дню месяца.
    /// Для YearMonth.Empty возвращается 31.12.9999.
    /// </summary>
    /// <param name="ym">Месяц и год</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<DateTime, YearMonth> EndOfMonthEx(DepValue<YearMonth> ym)
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
    public static YearMonth YearMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return new YearMonth(dt.Value);
      else
        return new YearMonth();
    }

    /// <summary>
    /// Преобразование даты в структуру YearMonth.
    /// Для значения null возвращает YearMonth.Empty
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<YearMonth, DateTime?> YearMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<YearMonth, DateTime?>(dt, YearMonth);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// Возвращает true, если значение непустое (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Значение свойства</returns>
    public static bool IsNotEmpty(YearMonth value)
    {
      return !value.IsEmpty;
    }

    /// <summary>
    /// Возвращает true, если значение непустое (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr1<bool, YearMonth> IsNotEmptyEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<bool, YearMonth>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region Nullable

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#).
    /// Не имеет смысла использовать в прикладном коде
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <param name="nullValue">Замещающее значение для null</param>
    /// <returns>Значение <paramref name="value"/>.Value или <paramref name="nullValue"/>.</returns>
    public static T ReplaceNull<T>(T? value, T nullValue)
      where T:struct
    {
      return value ?? nullValue;
    }

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#).
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <param name="nullValue">Замещающее значение для null</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr2<T, Nullable<T>, T> ReplaceNullEx<T>(DepValue<T?> value, T nullValue)
      where T:struct
    {
      return new DepExpr2<T, T?, T>(value, nullValue, ReplaceNull);
    }

    /// <summary>
    /// Замена для Nullable-значения (оператор ?? в C#) на значение по умолчанию default(T).
    /// </summary>
    /// <param name="value">Значение, которое может быть null</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DepExpr2<T, Nullable<T>, T> ReplaceNullEx<T>(DepValue<T?> value)
      where T : struct
    {
      return new DepExpr2<T, T?, T>(value, default(T), ReplaceNull);
    }

    #endregion

    #region Перечисления

    #region Enum <--> Int32

    /// <summary>
    /// Преобразование перечисления в целочисленное значение
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="value">Перечислимое значение</param>
    /// <returns>Преобразованное значение</returns>
    public static int EnumToInt<T>(T value)
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
    public static DepExpr1<int, T> EnumToIntEx<T>(DepValue<T> value)
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
    public static T EnumFromInt<T>(int value)
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
    public static DepExpr1<T,int> EnumFromIntEx<T>(DepValue<int> value)
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
    public static string EnumToString<T>(T value)
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
    public static DepExpr1<string, T> EnumToStringEx<T>(DepValue<T> value)
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
    public static T EnumFromString<T>(string value)
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
    public static DepExpr1<T, string> EnumFromStringEx<T>(DepValue<string> value)
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
    public static int Length<T>(T[] array)
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
    public static DepExpr1<int, T[]> LengthEx<T>(DepValue<T[]> array)
    {
      return new DepExpr1<int, T[]>(array, Length<T>);
    }

    #endregion

    #endregion

    #region Математические функции

    #region Min()/Max()

    #region Int32

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static int Min(params int[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      int res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] < res)
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
    public static DepExprTA<int, int> MinEx(params DepValue<int>[] values)
    {
      return new DepExprTA<int, int>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static int Max(params int[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      int res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] > res)
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
    public static DepExprTA<int, int> MaxEx(params DepValue<int>[] values)
    {
      return new DepExprTA<int, int>(values, Max);
    }

    #endregion

    #region Single

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static float Min(params float[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      float res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] < res)
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
    public static DepExprTA<float, float> MinEx(params DepValue<float>[] values)
    {
      return new DepExprTA<float, float>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static float Max(params float[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      float res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] > res)
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
    public static DepExprTA<float, float> MaxEx(params DepValue<float>[] values)
    {
      return new DepExprTA<float, float>(values, Max);
    }

    #endregion

    #region Double

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static double Min(params double[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      double res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] < res)
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
    public static DepExprTA<double, double> MinEx(params DepValue<double>[] values)
    {
      return new DepExprTA<double, double>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static double Max(params double[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      double res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] > res)
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
    public static DepExprTA<double, double> MaxEx(params DepValue<double>[] values)
    {
      return new DepExprTA<double, double>(values, Max);
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Возвращает минимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static decimal Min(params decimal[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      decimal res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] < res)
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
    public static DepExprTA<decimal, decimal> MinEx(params DepValue<decimal>[] values)
    {
      return new DepExprTA<decimal, decimal>(values, Min);
    }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Список аргументов не может быть пустым.
    /// </summary>
    /// <param name="values">Список аргументов</param>
    /// <returns>Вычисленное значение</returns>
    public static decimal Max(params decimal[] values)
    {
      if (values.Length == 0)
        throw new ArgumentException("Список аргументов пустой");

      decimal res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i] > res)
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
    public static DepExprTA<decimal, decimal> MaxEx(params DepValue<decimal>[] values)
    {
      return new DepExprTA<decimal, decimal>(values, Max);
    }

    #endregion

    #endregion

    #endregion
  }
}
