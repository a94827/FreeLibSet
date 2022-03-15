// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Преобразование значений стандартных типов в строку и обратно.
  /// Для чисел используется разделитель точка
  /// Для дат используется формат "гггг-мм-дд", для времени: "гггг-мм-ддTчч:мм:сс" (без дробной части секунд)
  /// </summary>
  public static class StdConvert
  {
    #region Методы преобразования одиночных значений

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
      DateTime res = DateTime.Parse(s, DateTimeFormat, DateTimeStyles.None);
      if (!useTime)
        res = res.Date;
      return res;
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
    /// Преобразует перечислимое значение в строку.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    /// <typeparam name="T">Тип перечисления</typeparam>
    public static string EnumToString<T>(T value)
    {
      return value.ToString();
    }

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
    /// Разделитель - точка. Разделителей групп разрядов нет
    /// </summary>
    public static readonly NumberFormatInfo NumberFormat = CreateNumberFormat();

    private static NumberFormatInfo CreateNumberFormat()
    {
      NumberFormatInfo fi = (NumberFormatInfo)(CultureInfo.InvariantCulture.NumberFormat.Clone());
      fi.NumberGroupSeparator = String.Empty;
      fi.NumberGroupSizes = DataTools.EmptyInts;
      return fi;
    }

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

    #region Методы преобразования значений через запятую

    #region Int32

    /// <summary>
    /// Преобразует массив целых значений в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(int[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив целых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out int[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyInts;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyInts;
        return true;
      }

      string[] a = s.Split(',');
      values = new int[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        int v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив целых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static int[] ToInt32Array(string s)
    {
      int[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив целых чисел");
    }

    #endregion

    #region Int64

    /// <summary>
    /// Преобразует массив целых значений в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(long[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив целых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out long[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyInt64s;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyInt64s;
        return true;
      }

      string[] a = s.Split(',');
      values = new long[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        long v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив целых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static long[] ToInt64Array(string s)
    {
      long[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив целых чисел");
    }

    #endregion

    #region Single

    /// <summary>
    /// Преобразует массив числовых значений в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(float[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out float[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptySingles;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptySingles;
        return true;
      }

      string[] a = s.Split(',');
      values = new float[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        float v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static float[] ToSingleArray(string s)
    {
      float[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив чисел");
    }

    #endregion

    #region Double

    /// <summary>
    /// Преобразует массив числовых значений в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(double[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out double[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyDoubles;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyDoubles;
        return true;
      }

      string[] a = s.Split(',');
      values = new double[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        double v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static double[] ToDoubleArray(string s)
    {
      double[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив чисел");
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Преобразует массив числовых значений в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(decimal[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out decimal[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyDecimals;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyDecimals;
        return true;
      }

      string[] a = s.Split(',');
      values = new decimal[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        decimal v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив числовых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static decimal[] ToDecimalArray(string s)
    {
      decimal[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив чисел");
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Преобразует массив значений даты/времени в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <param name="useTime">Нужно ли добавлять компонент времени</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(DateTime[] values, bool useTime)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i], useTime);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений даты/времени.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <param name="useTime">Нужно ли использовать компонент времени</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out DateTime[] values, bool useTime)
    {
      if (s == null)
      {
        values = DataTools.EmptyDateTimes;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyDateTimes;
        return true;
      }

      string[] a = s.Split(',');
      values = new DateTime[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        DateTime v;
        if (TryParse(a[i].Trim(), out v, useTime))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений даты/времени.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="useTime">Нужно ли использовать компонент времени</param>
    /// <returns>Массив преобразованных значений</returns>
    public static DateTime[] ToDateTimeArray(string s, bool useTime)
    {
      DateTime[] values;
      if (TryParse(s, out values, useTime))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив значений даты/времени");
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразует массив значений TimeSpan в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(TimeSpan[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений TimeSpan.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out TimeSpan[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyTimeSpans;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyTimeSpans;
        return true;
      }

      string[] a = s.Split(',');
      values = new TimeSpan[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        TimeSpan v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений TimeSpan.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static TimeSpan[] ToTimeSpanArray(string s)
    {
      TimeSpan[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив чисел");
    }

    #endregion

    #region Перечисление

    /// <summary>
    /// Преобразует массив значений перечисления в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string EnumToString<T>(T[] values)
      where T : struct
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = EnumToString<T>(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив перечислимых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParseEnum<T>(string s, out T[] values)
      where T : struct
    {
      if (s == null)
      {
        values = new T[0];
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = new T[0];
        return true;
      }

      string[] a = s.Split(',');
      values = new T[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        T v;
        if (TryParseEnum<T>(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив перечислимых значений.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static T[] ToEnumArray<T>(string s)
      where T : struct
    {
      T[] values;
      if (TryParseEnum<T>(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив перечислимых значений");
    }

    #endregion

    #region Guid

    /// <summary>
    /// Преобразует массив Guid в строку с разделителем - запятой.
    /// Экранирование значений кавычками не выполняется.
    /// Для <paramref name="values"/>=null и пустого массива возвращается пустая строка.
    /// </summary>
    /// <param name="values">Преобразуемые значения</param>
    /// <returns>Строка, разделенная запятыми</returns>
    public static string ToString(Guid[] values)
    {
      if (values == null)
        return String.Empty;
      string[] a = new string[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = ToString(values[i]);
      return String.Join(",", a);
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив Guid.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out Guid[] values)
    {
      if (s == null)
      {
        values = DataTools.EmptyGuids;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = DataTools.EmptyGuids;
        return true;
      }

      string[] a = s.Split(',');
      values = new Guid[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        Guid v;
        if (TryParse(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив Guid.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, возвращается массив нулевой длины.
    /// В случае невозможности преобразования генерируется исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив преобразованных значений</returns>
    public static Guid[] ToGuidArray(string s)
    {
      Guid[] values;
      if (TryParse(s, out values))
        return values;
      else
        throw new InvalidCastException("Строку нельзя преобразовать в массив Guid");
    }

    #endregion

    #endregion
  }
}
