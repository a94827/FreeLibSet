using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace AgeyevAV
{
  /// <summary>
  /// Преобразование значений стандартных типов в строку и обратно.
  /// Для чисел используется разделитель точка
  /// Для дат используется формат "гггг-мм-дд", для времени: "гггг-мм-ддTчч:мм:сс" (без дробной части секунд)
  /// </summary>
  public static class StdConvert
  {
    #region Методы преобразования

    #region Int32

    /// <summary>
    /// Преобразует целое значение в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(int value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в целое значение
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out int value)
    {
      return int.TryParse(s, NumberStyles.Integer, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразует строку в целое число.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static int ToInt32(string s)
    {
      return int.Parse(s, NumberStyles.Integer, NumberFormat);
    }

    #endregion

    #region Int64

    /// <summary>
    /// Преобразует целое значение в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(long value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в целое значение
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out long value)
    {
      return long.TryParse(s, NumberStyles.Integer, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразует строку в целое число.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static long ToInt64(string s)
    {
      return long.Parse(s, NumberStyles.Integer, NumberFormat);
    }

    #endregion

    #region Single

    /// <summary>
    /// Преобразует числовое значение в строку.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(float value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в числовое значение.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out float value)
    {
      return float.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразует строку в число.
    /// Разделитель дробной части - точка.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static float ToSingle(string s)
    {
      return float.Parse(s, NumberStyles.Float, NumberFormat);
    }

    #endregion

    #region Double

    /// <summary>
    /// Преобразует числовое значение в строку.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(double value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в числовое значение.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out double value)
    {
      return double.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразует строку в число.
    /// Разделитель дробной части - точка.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static double ToDouble(string s)
    {
      return double.Parse(s, NumberStyles.Float, NumberFormat);
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Преобразует числовое значение в строку.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(decimal value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в числовое значение.
    /// Разделитель дробной части - точка.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out decimal value)
    {
      return decimal.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразует строку в число.
    /// Разделитель дробной части - точка.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static decimal ToDecimal(string s)
    {
      return decimal.Parse(s, NumberStyles.Float, NumberFormat);
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Преобразует значение DateTime в строку.
    /// Преобразование выполняется в формате ISO 8601 "ГГГГ-ММ-ДДTЧЧ:ММ:СС" или "ГГГГ-ММ-ДД".
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <param name="useTime">Нужно ли добавлять компонент времени</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(DateTime value, bool useTime)
    {
      if (useTime)
        return value.ToString("s", DateTimeFormat);
      else
        return value.ToString("yyyy\\-MM\\-dd", DateTimeFormat);
    }

    /// <summary>
    /// Пытается преобразовать строку в значение DateTime.
    /// Преобразование выполняется в формате ISO 8601 "ГГГГ-ММ-ДДTЧЧ:ММ:СС" или "ГГГГ-ММ-ДД".
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <param name="useTime">Нужно ли использовать компонент времени</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out DateTime value, bool useTime)
    {
      if (DateTime.TryParseExact(s,
        new string[] { "s", "yyyy\\-MM\\-dd" },
        DateTimeFormat, DateTimeStyles.None, out value))
      {
        if (!useTime)
          value = value.Date;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Преобразует строку в значение DateTime.
    /// Преобразование выполняется в формате ISO 8601 "ГГГГ-ММ-ДДTЧЧ:ММ:СС" или "ГГГГ-ММ-ДД".
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="useTime">Нужно ли использовать компонент времени</param>
    /// <returns>Преобразованное значение</returns>
    public static DateTime ToDateTime(string s, bool useTime)
    {
      DateTime Res = DateTime.Parse(s, DateTimeFormat, DateTimeStyles.None);
      if (!useTime)
        Res = Res.Date;
      return Res;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразует значение TimeSpan в строку.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(TimeSpan value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Пытается преобразовать строку в значение TimeSpan.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out TimeSpan value)
    {
      return TimeSpan.TryParse(s, out value);
    }

    /// <summary>
    /// Преобразует строку в TimeSpan.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static TimeSpan ToTimeSpan(string s)
    {
      return TimeSpan.Parse(s);
    }

    #endregion

    #region Перечисление

    /// <summary>
    /// Преобразование строки в перечислимый тип.
    /// Регистр символов игнорируется.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Преобразованное значение, если преобразование выполнено</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    [DebuggerStepThrough]
    public static bool TryParseEnum<T>(string s, out T value)
      where T : struct
    {
      value = default(T);

      if (String.IsNullOrEmpty(s))
        return false;

      try
      {
        // К сожалению, в Net Framework 2 нет метода TryParse()
        value = (T)Enum.Parse(typeof(T), s, true);
      }
      catch
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Типизированный вызов метода Enum.Parse().
    /// Выбрасывает исключение, если <paramref name="s"/> - пустая строка, или содержит неизвестное значение.
    /// Регистр символов игнорируется.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static T ToEnum<T>(string s)
    {
      return (T)Enum.Parse(typeof(T), s, true);
    }

    #endregion

#if XXX
    // Не нужны
    #region Nullable<DateTime>

    public static string ToString(DateTime? Value, bool UseTime)
    {
      if (Value.HasValue)
        return ToString(Value, UseTime);
      else
        return String.Empty;
    }

    public static bool TryParse(string s, out DateTime? Value, bool UseTime)
    {
      if (String.IsNullOrEmpty(s))
      {
        Value = null;
        return true;
      }

      DateTime Value2;
      if (TryParse(s, out Value2, UseTime))
      {
        Value = Value2;
        return true;
      }
      else
      {
        Value = null;
        return false;
      }
    }

    public static DateTime? ToNullableDateTime(string s, bool UseTime)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return ToDateTime(s, UseTime);
    }

    #endregion
#endif

    #region Guid

    /// <summary>
    /// Преобразует значение типа Guid в строку.
    /// Просто вызывает Guid.ToString()
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(Guid value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Пытается преобразовать строку в Guid.
    /// Вызывает конструктор Guid(String), перехватывая исключение
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    [DebuggerStepThrough]
    public static bool TryParse(string s, out Guid value)
    {
      try
      {
        value = new Guid(s);
        return true;
      }
      catch
      {
        value = Guid.Empty;
        return false;
      }
    }

    /// <summary>
    /// Преобразует строку в Guid.
    /// В случае невозможности преобразования генерируется исключение.
    /// Просто вызывает конструктор Guid(String).
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static Guid ToGuid(string s)
    {
      return new Guid(s);
    }

    #endregion

    #endregion

    #region Свойства

    /// <summary>
    /// Форматировщик для чисел.
    /// Разделитель - точка
    /// </summary>
    public static readonly NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

    /// <summary>
    /// Форматировщик даты времени.
    /// Разделитель даты - "-", разделитель времени - ":"
    /// </summary>
    public static readonly DateTimeFormatInfo DateTimeFormat = CreateDateTimeFormat();

    private static DateTimeFormatInfo CreateDateTimeFormat()
    {
      DateTimeFormatInfo fi = (DateTimeFormatInfo)(CultureInfo.InvariantCulture.DateTimeFormat.Clone());
      fi.DateSeparator = "-";
      return fi;
    }

    #endregion
  }
}
