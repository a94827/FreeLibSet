using System;
using System.Collections.Generic;
using System.Text;

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

namespace AgeyevAV.DependedValues
{
  /// <summary>
  /// Функции, которые можно использовать с классами DepExprXXX для вычислений в удаленном пользовательском интерфейсе (RI).
  /// В RI обычно нельзя использовать делегаты на собственные пользовательские методы, если загрузка сборки с этим методом запрещена.
  /// В отличие от стандартных методов Net Framework, методы DepTools не выбрасываются исключения.
  /// </summary>
  public static class DepTools
  {
    #region Строковые функции

    /// <summary>
    /// Возвращает длину строки String.Length.
    /// Может использоваться в конструкторе класса DepExpr1.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Длина строкаи</returns>
    public static int Length(string s)
    {
      if (Object.ReferenceEquals(s, null))
        return 0;
      else
        return s.Length;
    }

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

    #endregion
  }
}
