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
    public static bool TryParseInt32(string s, out int value)
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
    public static bool TryParseInt64(string s, out long value)
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
    public static bool TryParseSingle(string s, out float value)
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
    public static bool TryParseDouble(string s, out double value)
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
    public static bool TryParseDecimal(string s, out decimal value)
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
    /// Преобразует значение <see cref="DateTime"/> в строку.
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
    /// Пытается преобразовать строку в значение <see cref="DateTime"/>.
    /// Преобразование выполняется в формате ISO 8601 "ГГГГ-ММ-ДДTЧЧ:ММ:СС" или "ГГГГ-ММ-ДД".
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <param name="useTime">Нужно ли использовать компонент времени</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParseDateTime(string s, out DateTime value, bool useTime)
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
    /// Преобразует строку в значение <see cref="DateTime"/>.
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
    /// Преобразует значение <see cref="TimeSpan"/> в строку.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(TimeSpan value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Пытается преобразовать строку в значение <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParseTimeSpan(string s, out TimeSpan value)
    {
      return TimeSpan.TryParse(s, out value);
    }

    /// <summary>
    /// Преобразует строку в <see cref="TimeSpan"/>.
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
    /// Типизированный вызов метода <see cref="Enum.Parse(Type, string, bool)"/>.
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
    /// Преобразует значение типа <see cref="Guid"/> в строку.
    /// Просто вызывает <see cref="Guid.ToString()"/>.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строковое представление значения</returns>
    public static string ToString(Guid value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Пытается преобразовать строку в <see cref="Guid"/>.
    /// Вызывает конструктор <see cref="Guid"/>(String), перехватывая исключение.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    [DebuggerStepThrough]
    public static bool TryParseGuid(string s, out Guid value)
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
    /// Преобразует строку в <see cref="Guid"/>.
    /// В случае невозможности преобразования генерируется исключение.
    /// Просто вызывает конструктор <see cref="Guid"/>.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public static Guid ToGuid(string s)
    {
      return new Guid(s);
    }

    #endregion

    #endregion

    #region Произвольный тип

    /// <summary>
    /// Преобразование значение любого типа 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string ToString<T>(T value)
    {
      IFormattable fvalue = value as IFormattable;
      if (fvalue != null)
      {
        if (typeof(T) == typeof(DateTime))
          return fvalue.ToString("yyyy\\-MM\\-dd", DateTimeFormat);
        else if (MathTools.IsNumericType(typeof(T)))
          return fvalue.ToString(String.Empty, NumberFormat);
        else
          return fvalue.ToString(String.Empty, CultureInfo.InvariantCulture);
      }
      else
      {
        if (typeof(T).IsClass)
        {
          if ((Object)value == null)
            return String.Empty;
        }
        return value.ToString();
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Форматировщик для чисел.
    /// Разделитель - точка. Разделителей групп разрядов нет.
    /// </summary>
    public static readonly NumberFormatInfo NumberFormat = CreateNumberFormat();

    private static NumberFormatInfo CreateNumberFormat()
    {
      NumberFormatInfo fi = (NumberFormatInfo)(CultureInfo.InvariantCulture.NumberFormat.Clone());
      fi.NumberGroupSeparator = String.Empty;
      fi.NumberGroupSizes = EmptyArray<Int32>.Empty;
      return fi;
    }

    /// <summary>
    /// Форматировщик даты времени.
    /// Разделитель даты - "-", разделитель времени - ":".
    /// </summary>
    public static readonly DateTimeFormatInfo DateTimeFormat = CreateDateTimeFormat();

    private static DateTimeFormatInfo CreateDateTimeFormat()
    {
      DateTimeFormatInfo fi = (DateTimeFormatInfo)(CultureInfo.InvariantCulture.DateTimeFormat.Clone());
      fi.DateSeparator = "-";
      return fi;
    }

    private class InternalFormatProvider : IFormatProvider
    {
      object IFormatProvider.GetFormat(Type formatType)
      {
        if (formatType == typeof(NumberFormatInfo))
          return StdConvert.NumberFormat;
        if (formatType == typeof(DateTimeFormatInfo))
          return StdConvert.DateTimeFormat;
        return null;
      }
    }

    /// <summary>
    /// Реализация интерфейса <see cref="IFormatProvider"/>, который возвращает <see cref="NumberFormat"/> и <see cref="DateTimeFormat"/>.
    /// </summary>
    public static IFormatProvider FormatProvider { get { return _FormatProvider; } }
    private static InternalFormatProvider _FormatProvider = new InternalFormatProvider();


    #endregion

    #region ChangeType()

    /// <summary>
    /// Преобразование типа значений.
    /// Использует вызов <see cref="System.Convert.ChangeType(object, Type, IFormatProvider)"/> со следующими изменениям:
    /// - Для строк используется форматизатор <see cref="StdConvert.FormatProvider"/>.
    /// - Когда число с плавающей точкой преобразуется в целое число, выполняется математическое округление, а не округление до четного.
    /// - Строки "0" и "1" можно преобразовать в логические значения.
    /// - null и пустая строка преобразуются в нулевое значение (<see cref="System.Convert"/> не преобразует пустые строки).
    /// - Поддерживается преобразование из <see cref="Guid"/> в массив <see cref="byte"/> и обратно.
    /// </summary>
    /// <param name="value">Исходное значение</param>
    /// <param name="conversionType">Тип, в который требуется выполнить преобразование</param>
    /// <returns></returns>
    public static object ChangeType(object value, Type conversionType)
    {
      if (value == null)
        return DataTools.GetEmptyValue(conversionType);
      if (MathTools.IsIntegerType(conversionType))
      {
        if (value.GetType() == typeof(Single))
        {
          double v = Math.Round((float)value, 0, MidpointRounding.AwayFromZero);
          return Convert.ChangeType(v, conversionType);
        }
        if (value.GetType() == typeof(Double))
        {
          double v = Math.Round((double)value, 0, MidpointRounding.AwayFromZero);
          return Convert.ChangeType(v, conversionType);
        }
        if (value.GetType() == typeof(Decimal))
        {
          decimal v = Math.Round((decimal)value, 0, MidpointRounding.AwayFromZero);
          return Convert.ChangeType(v, conversionType);
        }
      }

      string sValue = value as string;
      if (sValue != null)
      {
        if (sValue.Length == 0)
          return DataTools.GetEmptyValue(conversionType);

        if (conversionType == typeof(Boolean))
        {
          switch (sValue)
          {
            case "0": return DataTools.FalseObject;
            case "1": return DataTools.TrueObject;
          }
        }
        if (conversionType == typeof(TimeSpan))
          return TimeSpan.Parse(sValue);
        if (conversionType == typeof(Guid))
          return new Guid(sValue);
        if (conversionType.IsEnum)
          return Enum.Parse(conversionType, sValue);
      }

      if (conversionType.IsEnum)
      {
        if (MathTools.IsNumericType(value.GetType()))
        {
          int intValue = Convert.ToInt32(value);
          return Enum.ToObject(conversionType, intValue);
        }
      }
      if (conversionType == typeof(string))
      {
        if (value is DateTime)
          return ToString((DateTime)value, true);
        if (value is TimeSpan)
          return ToString((TimeSpan)value);
        if (value is Guid)
          return ToString((Guid)value);
      }
      if (value is byte[] && conversionType == typeof(Guid))
        return new Guid((byte[])value);
      if (value is Guid && conversionType == typeof(byte[]))
        return ((Guid)value).ToByteArray();

      return Convert.ChangeType(value, conversionType, StdConvert.FormatProvider);
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
    public static bool TryParseInt32Array(string s, out int[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Int32>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Int32>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new int[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        int v;
        if (TryParseInt32(a[i].Trim(), out v))
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
      if (TryParseInt32Array(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Int32[]));
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
    public static bool TryParseInt64Array(string s, out long[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Int64>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Int64>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new long[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        long v;
        if (TryParseInt64(a[i].Trim(), out v))
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
      if (TryParseInt64Array(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Int64[]));
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
    public static bool TryParseSingleArray(string s, out float[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Single>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Single>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new float[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        float v;
        if (TryParseSingle(a[i].Trim(), out v))
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
      if (TryParseSingleArray(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Single[]));
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
    public static bool TryParseDoubleArray(string s, out double[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Double>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Double>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new double[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        double v;
        if (TryParseDouble(a[i].Trim(), out v))
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
      if (TryParseDoubleArray(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Double[]));
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
    public static bool TryParseDecimalArray(string s, out decimal[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Decimal>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Decimal>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new decimal[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        decimal v;
        if (TryParseDecimal(a[i].Trim(), out v))
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
      if (TryParseDecimalArray(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Decimal[]));
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
    public static bool TryParseDateTimeArray(string s, out DateTime[] values, bool useTime)
    {
      if (s == null)
      {
        values = EmptyArray<DateTime>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<DateTime>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new DateTime[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        DateTime v;
        if (TryParseDateTime(a[i].Trim(), out v, useTime))
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
      if (TryParseDateTimeArray(s, out values, useTime))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(DateTime[]));
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразует массив значений <see cref="TimeSpan"/> в строку с разделителем - запятой.
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
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений <see cref="TimeSpan"/>.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParseTimeSpanArray(string s, out TimeSpan[] values)
    {
      if (s == null)
      {
        values = EmptyArray<TimeSpan>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<TimeSpan>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new TimeSpan[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        TimeSpan v;
        if (TryParseTimeSpan(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив значений <see cref="TimeSpan"/>.
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
      if (TryParseTimeSpanArray(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(TimeSpan[]));
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
    public static bool TryParseEnumArray<T>(string s, out T[] values)
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
      if (TryParseEnumArray<T>(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(T[]));
    }

    #endregion

    #region Guid

    /// <summary>
    /// Преобразует массив <see cref="Guid"/> в строку с разделителем - запятой.
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
    /// Пытается преобразовать строки, разделенной запятыми, в массив <see cref="Guid"/>.
    /// Экранирование значений кавычками не поддерживается.
    /// Строка может содержать пробелы, которые удаляются.
    /// Если строка пустая или содержит только пробелы, записывается массив нулевой длины и возвращается true. 
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="values">Сюда записываются преобразованные значения</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParseGuidArray(string s, out Guid[] values)
    {
      if (s == null)
      {
        values = EmptyArray<Guid>.Empty;
        return true;
      }
      s = s.Trim();
      if (s.Length == 0)
      {
        values = EmptyArray<Guid>.Empty;
        return true;
      }

      string[] a = s.Split(',');
      values = new Guid[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        Guid v;
        if (TryParseGuid(a[i].Trim(), out v))
          values[i] = v;
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// Пытается преобразовать строки, разделенной запятыми, в массив <see cref="Guid"/>.
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
      if (TryParseGuidArray(s, out values))
        return values;
      else
        throw ExceptionFactory.Inconvertible(s, typeof(Guid[]));
    }

    #endregion

    #endregion
  }
}
