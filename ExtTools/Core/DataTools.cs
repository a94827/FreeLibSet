// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Calendar;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Вспомогательные функции при работе с данными
  /// </summary>
  public static partial class DataTools
  {
    #region Преобразование типов

    #region GetXXX()

    #region GetInt()

    /// <summary>
    /// Возвращает значение поля как число.
    /// Если поле содержит DBNull, возвращается 0.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static int GetInt(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return 0;
        //else
        return GetInt(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(int));
      }
#endif
    }

#if DEBUG

    /// <summary>
    /// Перевыброс исключения при ошибке преобразования значения поля строки в методах GetXXX(<paramref name="row"/> , <paramref name="columnName"/>)
    /// </summary>
    /// <param name="e">Исходное исключение</param>
    /// <param name="row">Строка данных</param>
    /// <param name="columnName">Имя поля данных</param>
    /// <param name="resType"></param>
    private static Exception RecreateDataRowConvertException(Exception e, DataRow row, string columnName, Type resType)
    {
      if (row == null)
        return new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        return new ArgumentNullException("columnName");

      if (row.Table == null)
        e.Data["Row.Table"] = "null";
      else
      {
        e.Data["Row.Table.TableName"] = row.Table.TableName;
        //if (!Row.Table.Columns.Contains(ColumnName))
        //  throw new ArgumentException("Таблица \"" + Row.Table.TableName + "\" не содержит столбца \"" + ColumnName + "\"", e);

        e.Data["ColumnName"] = columnName;
        e.Data["Row.RowState"] = row.RowState.ToString();

        if (row.Table.Columns.Contains(columnName))
        {
          try
          {
            object x;
            if (row.RowState == DataRowState.Deleted)
              x = row[columnName, DataRowVersion.Original];
            else
              x = row[columnName];
            if (x == null)
              e.Data["Row[" + columnName + "]"] = "null";
            else
            {
              e.Data["Row[\"" + columnName + "\"]"] = x.ToString();
              e.Data["Row[\"" + columnName + "\"].GetType()"] = x.GetType().ToString();
            }
          }
          catch { }
        }
      }

      e.Data["ResultType"] = resType.ToString();

      return e;
    }

#endif

    /// <summary>
    /// Тоже, что и Convert.ToInt32(), но DBNull и пустая строка преобразуются в 0, а не выбрасывают исключение
    /// Тип Boolean преобразуется в значение 0 или 1.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Int32</returns>
    public static int GetInt(object value)
    {
      if (value is DBNull)
        return 0;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return 0;
        else
          return StdConvert.ToInt32(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1;
        else
          return 0;

      return Convert.ToInt32(value); // преобразуется, в том числе, значение null
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то значение Value не изменяется
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <param name="value">Значение</param>
    /// <returns>True, если значение прочитано, false, если оставлено без изменений</returns>
    public static bool GetInt(PropertyCollection collection, string propName, ref int value)
    {
      object x = collection[propName];
      if (x == null)
        return false;
      value = GetInt(x);
      return true;
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то возвращается 0
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <returns>Значение свойства</returns>
    public static int GetInt(PropertyCollection collection, string propName)
    {
      return GetInt(collection[propName]);
    }

    /// <summary>
    /// Преобразование значения в число или null.
    /// Для значений null, DBNull и пустой строки возвращается null. 
    /// Для других значений выполняется попытка преобразования
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Число или null</returns>
    public static int? GetNullableInt(object value)
    {
      if (Object.ReferenceEquals(value, null))
        return null;
      if (value is DBNull)
        return null;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToInt32(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1;
        else
          return 0;

      return Convert.ToInt32(value);
    }

    #endregion

    #region GetInt64()

    /// <summary>
    /// Возвращает значение поля как число.
    /// Если поле содержит DBNull, возвращается 0.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static long GetInt64(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return 0;
        //else
        return GetInt64(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(long));
      }
#endif
    }


    /// <summary>
    /// Тоже, что и Convert.ToInt32(), но DBNull и пустая строка преобразуются в 0, а не выбрасывают исключение
    /// Тип Boolean преобразуется в значение 0 или 1.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Int64</returns>
    public static long GetInt64(object value)
    {
      if (value is DBNull)
        return 0L;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return 0L;
        else
          return StdConvert.ToInt64(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1L;
        else
          return 0L;

      return Convert.ToInt64(value); // преобразуется, в том числе, значение null
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то значение Value не изменяется
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <param name="value">Значение</param>
    /// <returns>True, если значение прочитано, false, если оставлено без изменений</returns>
    public static bool GetInt64(PropertyCollection collection, string propName, ref long value)
    {
      object x = collection[propName];
      if (x == null)
        return false;
      value = GetInt64(x);
      return true;
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то возвращается 0
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <returns>Значениес свойства</returns>
    public static long GetInt64(PropertyCollection collection, string propName)
    {
      return GetInt64(collection[propName]);
    }

    /// <summary>
    /// Преобразование значения в число или null.
    /// Для значений null, DBNull и пустой строки возвращается null. 
    /// Для других значений выполняется попытка преобразования
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Число или null</returns>
    public static long? GetNullableInt64(object value)
    {
      if (Object.ReferenceEquals(value, null))
        return null;
      if (value is DBNull)
        return null;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToInt64(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1L;
        else
          return 0L;

      return Convert.ToInt64(value);
    }

    #endregion

    #region GetDecimal()

    /// <summary>
    /// Возвращает значение поля как число.
    /// Если поле содержит DBNull, возвращается 0.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static decimal GetDecimal(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return 0;
        //else
        return GetDecimal(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(decimal));
      }
#endif
    }

    /// <summary>
    /// Тоже, что и Convert.ToDecimal(), но пустая строка и DBNull преобразуются в 0m, а не выбрасывают исключение
    /// Тип Boolean преобразуется в значение 0 или 1.
    /// Строковый тип преобразуется с использованием CultureInfo.CurrentCulture (вызов Decimal.Parse() без дополнительных аргументов)
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Decimal</returns>
    public static decimal GetDecimal(object value)
    {
      if (value is Boolean)
        if ((bool)value)
          return 1m;
        else
          return 0m;

      if (value is DBNull)
        return 0m;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return 0m;
        else
          return StdConvert.ToDecimal(s);
      }

      return Convert.ToDecimal(value); // преобразуется, в том числе, значение null
    }

    /// <summary>
    /// Преобразование значения в число или null.
    /// Для значений null, DBNull и пустой строки возвращается null. 
    /// Для других значений выполняется попытка преобразования
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Число или null</returns>
    public static decimal? GetNullableDecimal(object value)
    {
      if (Object.ReferenceEquals(value, null))
        return null;
      if (value is DBNull)
        return null;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToDecimal(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1m;
        else
          return 0m;

      return Convert.ToDecimal(value);
    }

    #endregion

    #region GetDouble()

    /// <summary>
    /// Возвращает значение поля как число.
    /// Если поле содержит DBNull, возвращается 0.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static double GetDouble(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return 0.0;
        //else
        return GetDouble(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(double));
      }
#endif
    }


    /// <summary>
    /// Тоже, что и Convert.ToDouble(), но пустая строка и DBNull преобразуются в 0, а не выбрасывают исключение
    /// Тип Boolean преобразуется в значение 0 или 1.
    /// Строковый тип преобразуется с использованием CultureInfo.CurrentCulture (вызов Double.Parse() без дополнительных аргументов)
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Double</returns>
    public static double GetDouble(object value)
    {
      if (value is Boolean)
        if ((bool)value)
          return 1.0;
        else
          return 0.0;

      if (value is DBNull)
        return 0.0;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return 0.0;
        else
          return StdConvert.ToDouble(s);
      }

      return Convert.ToDouble(value); // преобразуется, в том числе, значение null
    }

    /// <summary>
    /// Преобразование значения в число или null.
    /// Для значений null, DBNull и пустой строки возвращается null. 
    /// Для других значений выполняется попытка преобразования
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Число или null</returns>
    public static double? GetNullableDouble(object value)
    {
      if (Object.ReferenceEquals(value, null))
        return null;
      if (value is DBNull)
        return null;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToDouble(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1.0;
        else
          return 0.0;

      return Convert.ToDouble(value);
    }

    #endregion

    #region GetSingle()

    /// <summary>
    /// Возвращает значение поля как число.
    /// Если поле содержит DBNull, возвращается 0.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static float GetSingle(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return 0f;
        //else
        return GetSingle(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(float));
      }
#endif
    }


    /// <summary>
    /// Тоже, что и Convert.ToSingle(), но пустая строка и DBNull преобразуются в 0, а не выбрасывают исключение
    /// Тип Boolean преобразуется в значение 0 или 1.
    /// Строковый тип преобразуется с использованием CultureInfo.CurrentCulture (вызов Single.Parse() без дополнительных аргументов)
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Single</returns>
    public static float GetSingle(object value)
    {
      if (value is Boolean)
        if ((bool)value)
          return 1f;
        else
          return 0f;

      if (value is DBNull)
        return 0f;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return 0f;
        else
          return StdConvert.ToSingle(s);
      }

      return Convert.ToSingle(value); // преобразуется, в том числе, значение null
    }

    /// <summary>
    /// Преобразование значения в число или null.
    /// Для значений null, DBNull и пустой строки возвращается null. 
    /// Для других значений выполняется попытка преобразования
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Число или null</returns>
    public static float? GetNullableSingle(object value)
    {
      if (Object.ReferenceEquals(value, null))
        return null;
      if (value is DBNull)
        return null;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToSingle(s);
      }

      if (value is Boolean)
        if ((bool)value)
          return 1f;
        else
          return 0f;

      return Convert.ToSingle(value);
    }

    #endregion

    #region GetBool()

    /// <summary>
    /// Возвращает значение поля как значение Boolean.
    /// Если поле содержит DBNull, возвращается false.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static bool GetBool(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return false;
        //else
        return GetBool(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(bool));
      }
#endif
    }


    /// <summary>
    /// Тоже, что и Convert.ToSingle(), но пустая строка и DBNull преобразуются в false, а не выбрасывают исключение
    /// Для типа String воспринимаются значения "0" и "1".
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Double</returns>
    public static bool GetBool(object value)
    {
      if (value is DBNull)
        return false;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return false;
        switch (s)
        {
          case "0": return false;
          case "1": return true;
          default: return Convert.ToBoolean(s);
        }
      }

      return Convert.ToBoolean(value); // преобразуется, в том числе, значение null
    }

    #endregion

    #region GetString()

    /// <summary>
    /// Возвращает значение поля как строку.
    /// Если поле содержит <see cref="DBNull"/>, возвращается пустая строка.
    /// Подробности см. в описании перегрузки <see cref="GetString(Object)"/>.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static string GetString(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return String.Empty;
        //else
        return GetString(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(string));
      }
#endif
    }

    /// <summary>
    /// Преобразование произвольного значения в строку.
    /// null преобразуется в пустую строку.
    /// <see cref="DBNull"/> преобразуется в пустую строку.
    /// Строковый тип обрезается вызовом <see cref="String.Trim()"/>.
    /// Тип <see cref="DateTime"/> преобразуется с помощью <see cref="StdConvert"/>.
    /// Числовые значения и другие типы, реализующие интерфейс <see cref="IConvertible"/>, преобразуются с использованием <see cref="StdConvert.NumberFormat"/>.
    /// Для прочих типов используется обычное преобразование с помощью <see cref="Object.ToString()"/>.
    /// 
    /// Обычно следует использовать методы <see cref="Object.ToString()"/> или <see cref="Convert.ToString(Object)"/>.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строкое значение</returns>
    public static string GetString(object value)
    {
      if (value == null)
        return String.Empty;
      if (value is DBNull)
        return String.Empty;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
        return s.Trim();

      if (value is DateTime)
      {
        DateTime dt = (DateTime)value;
        return StdConvert.ToString(dt, true); // 16.05.2023
      }

      IConvertible value2 = value as IConvertible;
      if (value2 != null)
        return value2.ToString(StdConvert.NumberFormat); // 16.05.2023

      return value.ToString();
    }

    #endregion

    #region GetDateTime()

    /// <summary>
    /// Получает объект <see cref="DateTime"/> в пределах суток 01.01.0001.
    /// Если <paramref name="value"/> превышает 86400 секунд, берется значение по модулю.
    /// Отрицательных значения заменяются, например, если <paramref name="value"/> равно минус 15 минут, то возвращается 01.01.0001 23:45
    /// </summary>
    /// <param name="value">Произвольный интервал времени</param>
    /// <returns>Значение в пределах даты <see cref="DateTime.MinValue"/></returns>
    public static DateTime GetDateTime(TimeSpan value)
    {
      long ticks = value.Ticks % TimeSpan.TicksPerDay;
      // Значение может быть положительным или отрицательным
      if (ticks < 0L)
        ticks += TimeSpan.TicksPerDay;
      return new DateTime(ticks);
    }

    /// <summary>
    /// Преобразование значения в тип DateTime без значения null.
    /// Значения null и DBNull преобразуется в DateTime.MinValue.
    /// Пустая строка также преобразуется в DateTime.MinValue, для непустой строки выполняется попытка преобразования с помощью Convert.ToDateTime().
    /// Для остальных типов будет выброшено исключение. 
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Результат преобразования</returns>
    public static DateTime GetDateTime(object value)
    {
      if (value == null)
        return new DateTime();
      if (value is DBNull)
        return new DateTime();
      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return new DateTime();
        else
          return StdConvert.ToDateTime(s, true);
      }
      if (value is TimeSpan)
        return GetDateTime((TimeSpan)value); // 16.05.2023
      return Convert.ToDateTime(value);
    }

    /// <summary>
    /// Возвращает значение поля как DateTime.
    /// Если поле содержит DBNull, возвращается неинициализированная дата.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static DateTime GetDateTime(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return new DateTime();
        //else
        return GetDateTime((row[columnName])); // исправлено 15.12.2021

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(DateTime));
      }
#endif
    }

    /// <summary>
    /// Преобразование значения в тип Nullable DateTime.
    /// Значения null и DBNull преобразуется в null.
    /// Пустая строка преобразуется в null, иначе выполняется попытка преобразования.
    /// Для остальных типов будет выброшено исключение. Для преобразования используется Convert.ToDateTime()
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Результат преобразования</returns>
    public static Nullable<DateTime> GetNullableDateTime(object value)
    {
      if (value == null)
        return null;
      if (value is DBNull)
        return null;
      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return null;
        else
          return StdConvert.ToDateTime(s, true);
      }
      if (value is TimeSpan)
        return GetDateTime((TimeSpan)value); // 16.05.2023
      return Convert.ToDateTime(value);
    }

    /// <summary>
    /// Возвращает значение поля как DateTime.
    /// Если поле содержит DBNull, возвращается null.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static Nullable<DateTime> GetNullableDateTime(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        //Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return null;
        //else
        return GetNullableDateTime(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(DateTime?));
      }
#endif
    }

    #endregion

    #region GetTimeSpan()

    /// <summary>
    /// Преобразование значения в тип TimeSpan.
    /// Значения null и DBNull преобразуется в TimeSpan.Zero. 
    /// TimeSpan возвращается без преобразования.
    /// Для остальных типов будет выброшено исключение InvalidCastException
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Результат преобразования</returns>
    public static TimeSpan GetTimeSpan(object value)
    {
      if (value == null)
        return TimeSpan.Zero;
      if (value is DBNull)
        return TimeSpan.Zero;
      if (value is TimeSpan)
        return (TimeSpan)value;
      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return TimeSpan.Zero;
        else
          return StdConvert.ToTimeSpan(s);
      }
      if (value is DateTime)
        return ((DateTime)value).TimeOfDay; // 16.05.2023
      throw new InvalidCastException("Тип " + value.GetType().FullName + " нельзя преобразовать в TimeSpan");
    }

    /// <summary>
    /// Возвращает значение поля как интервал времени.
    /// Если поле содержит DBNull, возвращается TimeSpan.Zero.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static TimeSpan GetTimeSpan(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return TimeSpan.Zero;
        //else
        return GetTimeSpan(row[columnName]); // исправлено 15.12.2021

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(TimeSpan));
      }
#endif
    }

    #endregion

    #region GetGuid()

    /// <summary>
    /// Возвращает значение поля как GUID.
    /// Если поле содержит DBNull, возвращается Guid.Empty.
    /// Поле может содержать Guid, строку или массив байт
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static Guid GetGuid(DataRow row, string columnName)
    {
#if DEBUG
      try
      {
#endif

        // Убрано 29.06.2020
        //if (row.IsNull(columnName))
        //  return Guid.Empty;
        //else
        return GetGuid(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(Guid));
      }
#endif
    }


    /// <summary>
    /// Преобразование значения в Guid.
    /// Допускаются значения Guid, String и Byte[16].
    /// Для значений null и DBNull пустой строки возвращается Guid.Empty.
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение типа Guid</returns>
    public static Guid GetGuid(object value)
    {
      if (value == null || value is DBNull)
        return Guid.Empty;

      if (value is Guid)
        return (Guid)value;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return Guid.Empty;
        else
          return StdConvert.ToGuid(s);
      }

      if (value is byte[])
        return new Guid((byte[])value);

      throw new InvalidCastException("Значение типа " + value.GetType().ToString() + " нельзя преобразовать в Guid");
    }

    /// <summary>
    /// Получение  значения Guid из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то значение Value не изменяется
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <param name="value">Значение</param>
    /// <returns>True, если значение прочитано, false, если оставлено без изменений</returns>
    public static bool GetGuid(PropertyCollection collection, string propName, ref Guid value)
    {
      object x = collection[propName];
      if (x == null)
        return false;
      value = GetGuid(x);
      return true;
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то возвращается Guid.Empty
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <returns>Значениес свойства</returns>
    public static Guid GetGuid(PropertyCollection collection, string propName)
    {
      return GetGuid(collection[propName]);
    }

    #endregion

    #region GetEnum()

    /// <summary>
    /// Возвращает значение поля как значение перечисления.
    /// Если поле содержит DBNull, возвращается нулевое значение для перечисления (default).
    /// Поле может содержать целочисленное значение или строку
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public static T GetEnum<T>(DataRow row, string columnName)
      where T : struct
    {
#if DEBUG
      try
      {
#endif

        return GetEnum<T>(row[columnName]);

#if DEBUG
      }
      catch (Exception e)
      {
        throw RecreateDataRowConvertException(e, row, columnName, typeof(Guid));
      }
#endif
    }


    /// <summary>
    /// Преобразование значения в перечисление.
    /// Допускаются значения типа <typeparamref name="T"/>, Int32 или String.
    /// Для значений null, DBNull и пустой строки возвращается нулевое значение для перечисления (default).
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Значение перечислимого типа</returns>
    public static T GetEnum<T>(object value)
      where T : struct
    {
      if (value == null || value is DBNull)
        return default(T);

      if (value is T)
        return (T)value;

      string s = value as string;
      if (!Object.ReferenceEquals(s, null))
      {
        if (s.Length == 0)
          return default(T);
        else
          return StdConvert.ToEnum<T>(s);
      }

      int v2 = GetInt(value);
      return (T)Enum.ToObject(typeof(T), v2);
    }

    /// <summary>
    /// Получение перечислимого значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то значение Value не изменяется
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <param name="value">Значение</param>
    /// <returns>True, если значение прочитано, false, если оставлено без изменений</returns>
    public static bool GetEnum<T>(PropertyCollection collection, string propName, ref T value)
      where T : struct
    {
      object x = collection[propName];
      if (x == null)
        return false;
      value = GetEnum<T>(x);
      return true;
    }

    /// <summary>
    /// Получение числового значения из ExtendedProperties
    /// Если коллекция не содержит свойства с именем Name, то возвращается Guid.Empty
    /// </summary>
    /// <param name="collection">Коллекция свойств</param>
    /// <param name="propName">Имя свойства</param>
    /// <returns>Значениес свойства</returns>
    public static T GetEnum<T>(PropertyCollection collection, string propName)
      where T : struct
    {
      return GetEnum<T>(collection[propName]);
    }

    #endregion

    #endregion

    #region SetXXX()

    #region SetString()

    /// <summary>
    /// Установка строки в поле. При этом выполняется обрезка строки
    /// до длины MaxLength. Также выполняется преобразование в DBNull для пустой
    /// строки, если Column.AllowDBNull установлено
    /// </summary>
    /// <param name="row">Строка данных, куда выполняется запись</param>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetString(DataRow row, string columnName, string value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (String.IsNullOrEmpty(value))
      {
        if (col.AllowDBNull)
          row[columnName] = DBNull.Value;
        else
          row[columnName] = "";
      }
      else
      {
        int l = col.MaxLength;
        if (l >= 0 && l < value.Length)
          row[columnName] = value.Substring(0, l);
        else
          row[columnName] = value;
      }
    }

    #endregion

    #region SetInt()

    /// <summary>
    /// Установка числового значения поля. Выполняет преобразование значения в
    /// конкретный тип поля. Для нулевого значения выполняет преобразование в DBNull,
    /// если Column.AllowDBNull поддерживается
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetInt(DataRow row, string columnName, int value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (IsIntegerType(col.DataType))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать числовое значение", "ColumnName");
    }

    #endregion

    #region SetInt64()

    /// <summary>
    /// Установка числового значения поля. Выполняет преобразование значения в
    /// конкретный тип поля. Для нулевого значения выполняет преобразование в DBNull,
    /// если Column.AllowDBNull поддерживается
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetInt64(DataRow row, string columnName, long value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (IsIntegerType(col.DataType))
      {
        if (value == 0L)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать числовое значение", "ColumnName");
    }

    #endregion

    #region SetSingle()

    /// <summary>
    /// Установка числового значения поля. Выполняет преобразование значения в
    /// конкретный тип поля. Для нулевого значения выполняет преобразование в DBNull,
    /// если Column.AllowDBNull поддерживается
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetSingle(DataRow row, string columnName, float value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (col.DataType == typeof(float) || col.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (decimal)value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0f)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать числовое значение с плавающей точкой", "ColumnName");
    }

    #endregion

    #region SetDouble()

    /// <summary>
    /// Установка числового значения поля. Выполняет преобразование значения в
    /// конкретный тип поля. Для нулевого значения выполняет преобразование в DBNull,
    /// если Column.AllowDBNull поддерживается
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetDouble(DataRow row, string columnName, double value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (col.DataType == typeof(float) || col.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (decimal)value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0.0)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать числовое значение с плавающей точкой", "ColumnName");
    }

    #endregion

    #region SetDecimal()

    /// <summary>
    /// Установка числового значения поля. Выполняет преобразование значения в
    /// конкретный тип поля. Для нулевого значения выполняет преобразование в DBNull,
    /// если Column.AllowDBNull поддерживается
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetDecimal(DataRow row, string columnName, decimal value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (col.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(float) || col.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (col.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (double)value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0m)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать значение типа Decimal", "ColumnName");
    }

    #endregion

    #region SetNullableDateTime()

    /// <summary>
    /// Установка значения поля типа Дата/Время. 
    /// Для значения null выполняет преобразование в DBNull,
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetNullableDateTime(DataRow row, string columnName, Nullable<DateTime> value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif

      if (col.DataType == typeof(DateTime))
      {
        if (value.HasValue)
          row[columnName] = value.Value;
        else
          row[columnName] = DBNull.Value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value.HasValue)
          SetString(row, columnName, StdConvert.ToString(value.Value, true));
        else
          SetString(row, columnName, null);
        return;
      }
      throw new ArgumentException("Столбец не может принимать значение типа DateTime", "ColumnName");
    }

    #endregion

    #region SetTimeSpan()

    /// <summary>
    /// Установка значения поля типа TimeSpan. 
    /// Для нулевого значения выполняет преобразование в DBNull,
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetTimeSpan(DataRow row, string columnName, TimeSpan value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif

      if (col.DataType == typeof(TimeSpan))
      {
        if (value.Ticks == 0L)
          row[columnName] = DBNull.Value;
        else
          row[columnName] = value;
        return;
      }
      if (col.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value.Ticks == 0L)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, StdConvert.ToString(value));
        return;
      }
      throw new ArgumentException("Столбец не может принимать значение типа TimeSpan", "ColumnName");
    }

    #endregion

    #region SetGuid()

    /// <summary>
    /// Установка значения поля типа Guid, String или byte[], в зависимости от типа поля 
    /// Для значения Guid.Empty записывается значение DBNull
    /// </summary>
    /// <param name="row">Строка</param>
    /// <param name="columnName">Имя записываемого поля</param>
    /// <param name="value">Записываемое значение</param>
    public static void SetGuid(DataRow row, string columnName, Guid value)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      DataColumn col = row.Table.Columns[columnName];
#if DEBUG
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");
#endif
      if (value == Guid.Empty)
        row[columnName] = DBNull.Value;
      else
      {
        if (col.DataType == typeof(Guid))
          row[columnName] = value;
        else if (col.DataType == typeof(string))
          row[columnName] = StdConvert.ToString(value);
        else if (col.DataType == typeof(byte[]))
          row[columnName] = value.ToByteArray();
        else
          throw new ArgumentException("Поле \"" + columnName + "\" имеет неподходящий тип " + col.DataType.ToString(), "colName");
      }
    }

    #endregion

    #endregion

    #endregion

    #region Инкремент значения

    // Не стоит все методы называть одинаково ("Inc"), т.к. легко перепутать тип
    // и получить ошибку округления

    #region IncInt

    /// <summary>
    /// Увеличение значения поля типа Int32
    /// </summary>
    /// <param name="row">Изменяемая строка (объект DataRow)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncInt(DataRow row, string columnName, int delta)
    {
      if (delta == 0)
        return;
      int v = GetInt(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа Int32 на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncInt(DataRow srcRow, DataRow dstRow, string columnName)
    {
      int v = GetInt(srcRow, columnName);
      IncInt(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа Int32
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncInt(DataRowView drv, string columnName, int delta)
    {
      if (delta == 0)
        return;
      int v = GetInt(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncInt64

    /// <summary>
    /// Увеличение значения поля типа Int64
    /// </summary>
    /// <param name="row">Изменяемая строка (объект DataRow)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncInt64(DataRow row, string columnName, long delta)
    {
      if (delta == 0)
        return;
      long v = GetInt64(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа Int64 на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncInt64(DataRow srcRow, DataRow dstRow, string columnName)
    {
      long v = GetInt64(srcRow, columnName);
      IncInt64(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа Int64
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncInt64(DataRowView drv, string columnName, long delta)
    {
      if (delta == 0L)
        return;
      long v = GetInt64(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncSingle

    /// <summary>
    /// Увеличение значения поля типа Single
    /// </summary>
    /// <param name="row">Изменяемая строка</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncSingle(DataRow row, string columnName, float delta)
    {
      if (delta == 0f)
        return;
      float v = GetSingle(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа Single на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncSingle(DataRow srcRow, DataRow dstRow, string columnName)
    {
      float v = GetSingle(srcRow, columnName);
      IncSingle(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа Single
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncSingle(DataRowView drv, string columnName, float delta)
    {
      if (delta == 0f)
        return;
      float v = GetSingle(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncDouble

    /// <summary>
    /// Увеличение значения поля типа Double
    /// </summary>
    /// <param name="row">Изменяемая строка</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncDouble(DataRow row, string columnName, double delta)
    {
      if (delta == 0.0)
        return;
      double v = GetDouble(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа Double на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncDouble(DataRow srcRow, DataRow dstRow, string columnName)
    {
      double v = GetDouble(srcRow, columnName);
      IncDouble(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа Double
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncDouble(DataRowView drv, string columnName, double delta)
    {
      if (delta == 0.0)
        return;
      double v = GetDouble(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncDecimal

    /// <summary>
    /// Увеличение значения поля типа Decimal
    /// </summary>
    /// <param name="row">Изменяемая строка</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncDecimal(DataRow row, string columnName, decimal delta)
    {
      if (delta == 0m)
        return;
      decimal v = GetDecimal(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа Decimal на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncDecimal(DataRow srcRow, DataRow dstRow, string columnName)
    {
      decimal v = GetDecimal(srcRow, columnName);
      IncDecimal(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа Decimal
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncDecimal(DataRowView drv, string columnName, decimal delta)
    {
      if (delta == 0m)
        return;
      decimal v = GetDecimal(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncTimeSpan

    /// <summary>
    /// Увеличение значения поля типа TimeSpan
    /// </summary>
    /// <param name="row">Изменяемая строка</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncTimeSpan(DataRow row, string columnName, TimeSpan delta)
    {
      if (delta.Ticks == 0L)
        return;
      TimeSpan v = GetTimeSpan(row, columnName);
      checked { v += delta; }
      row[columnName] = v;
    }

    /// <summary>
    /// Увеличение значения поля типа TimeSpan на значение того же поля в другой 
    /// строке. Применяется для расчета итоговых строк
    /// </summary>
    /// <param name="srcRow">Строка, содержащее прибавляемое значение</param>
    /// <param name="dstRow">Изменяемая (итоговая) строка</param>
    /// <param name="columnName">Имя поля</param>
    public static void IncTimeSpan(DataRow srcRow, DataRow dstRow, string columnName)
    {
      TimeSpan v = GetTimeSpan(srcRow, columnName);
      IncTimeSpan(dstRow, columnName, v);
    }

    /// <summary>
    /// Увеличение значения поля типа TimeSpan
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncTimeSpan(DataRowView drv, string columnName, TimeSpan delta)
    {
      if (delta.Ticks == 0L)
        return;
      TimeSpan v = GetTimeSpan(drv[columnName]);
      checked { v += delta; }
      drv[columnName] = v;
    }

    #endregion

    #region IncValue

    /// <summary>
    /// Увеличение значения поля произвольного числового типа или TimeSpan
    /// </summary>
    /// <param name="drv">Изменяемая строка (объект DataRowView)</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncValue(DataRowView drv, string columnName, object delta)
    {
      IncValue(drv.Row, columnName, delta); // не стоит делать повторяющийся код
    }

    /// <summary>
    /// Увеличение значения поля произвольного числового типа или TimeSpan
    /// </summary>
    /// <param name="row">Изменяемая строка</param>
    /// <param name="columnName">Имя поля, которое будет изменено</param>
    /// <param name="delta">Прибавляемое значение</param>
    public static void IncValue(DataRow row, string columnName, object delta)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = row.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: IncInt(row, columnName, DataTools.GetInt(delta)); break;
        case SumType.Int64: IncInt64(row, columnName, DataTools.GetInt(delta)); break;
        case SumType.Single: IncSingle(row, columnName, DataTools.GetSingle(delta)); break;
        case SumType.Double: IncDouble(row, columnName, DataTools.GetDouble(delta)); break;
        case SumType.Decimal: IncDecimal(row, columnName, DataTools.GetDecimal(delta)); break;
        case SumType.TimeSpan: IncTimeSpan(row, columnName, DataTools.GetTimeSpan(delta)); break;
        default:
          throw new InvalidOperationException("Столбец \"" + columnName + "\" нельзя суммировать");
      }
    }

    /// <summary>
    /// Увеличить значение числового поля в одной строке на значение этого же поля
    /// в другой строке. Поле может быть любого числового типа.
    /// Вызывает соответствующую типу поля функцию IncXXX. Используется, когда
    /// конкретный тип числового поля заранее неизвестен.
    /// Если тип поля известен, то рекомендуется вызывать конкретную функцию, т.к.
    /// она работает бьстрее
    /// </summary>
    /// <param name="srcRow"></param>
    /// <param name="dstRow"></param>
    /// <param name="columnName"></param>
    public static void IncValue(DataRow srcRow, DataRow dstRow, string columnName)
    {
#if DEBUG
      if (srcRow == null)
        throw new ArgumentNullException("srcRow");
      if (dstRow == null)
        throw new ArgumentNullException("dstRow");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = srcRow.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: IncInt(srcRow, dstRow, columnName); break;
        case SumType.Int64: IncInt64(srcRow, dstRow, columnName); break;
        case SumType.Single: IncSingle(srcRow, dstRow, columnName); break;
        case SumType.Double: IncDouble(srcRow, dstRow, columnName); break;
        case SumType.Decimal: IncDecimal(srcRow, dstRow, columnName); break;
        case SumType.TimeSpan: IncTimeSpan(srcRow, dstRow, columnName); break;
        default:
          throw new InvalidOperationException("Столбец \"" + columnName + "\" нельзя суммировать");
      }
    }

    #endregion

    #endregion

    #region Пары "Имя поля - значение"

    /// <summary>
    /// Преобразование массивов имен полей и значений в список пар для последующей модификации
    /// </summary>
    /// <param name="columnNames">Массив имен полей</param>
    /// <param name="columnValues">Массив значений</param>
    /// <returns>Список пар имен и значений</returns>
    public static Hashtable NamesAndValuesToPairs(string[] columnNames, object[] columnValues)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnValues == null)
        throw new ArgumentNullException("columnValues");
      if (columnNames.Length != columnValues.Length)
        throw new ArgumentException("Массивы имен полей и значений имеют разную длину");

      Hashtable res = new Hashtable();
      for (int i = 0; i < columnNames.Length; i++)
        res.Add(columnNames[i], columnValues[i]);
      return res;
    }

    /// <summary>
    /// Преобразование хэш-таблицы, содержащей значения полей с ключами
    /// по их именам в отдельные массивы имен полей и значений
    /// </summary>
    /// <param name="columnNamesAndValues">Исходные поля и значения</param>
    /// <param name="columnNames">Результирующий массив имен полей</param>
    /// <param name="columnValues">Результирующий массив значений</param>
    public static void PairsToNamesAndValues(IDictionary columnNamesAndValues,
      out string[] columnNames, out object[] columnValues)
    {
      if (columnNamesAndValues == null)
        throw new ArgumentNullException("columnNamesAndValues");

      int n = columnNamesAndValues.Count;
      columnNames = new string[n];
      columnValues = new object[n];
      int i = 0;
      foreach (object Key in columnNamesAndValues.Keys)
      {
        columnNames[i] = (string)Key;
        columnValues[i] = columnNamesAndValues[Key];
        i++;
      }
    }

    #endregion

    #region Коллекции

    /// <summary>
    /// Выполняет сравнение двух перечисляемых объектов, реализующих интерфейс IEnumerable
    /// Возвращает true, если оба перечислителя содержат одинаковое число элементов и элементы попанрно совпадают.
    /// Порядок элементов в перечислители имеет значение
    /// Для сравнение элементов используется Object.Equals()
    /// </summary>
    /// <typeparam name="T">Тип объектов</typeparam>
    /// <param name="list1">Первый сравниваемый объек</param>
    /// <param name="list2">Второй сравниваемый объек</param>
    /// <returns>true, если перечислители содержат одинаковые объекты</returns>
    public static bool AreEnumerablesEqual<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
#if DEBUG
      if (list1 == null)
        throw new ArgumentNullException("list1");
      if (list2 == null)
        throw new ArgumentNullException("list2");
#endif

      // Нельзя использовать оператор foreach, придется работать с IEnumerator
      using (IEnumerator<T> en1 = list1.GetEnumerator())
      {
        using (IEnumerator<T> en2 = list2.GetEnumerator())
        {
          en1.Reset();
          en2.Reset();
          while (true)
          {
            bool has1 = en1.MoveNext();
            bool has2 = en2.MoveNext();
            if (has1 != has2)
              return false; // число элементов не совпадает
            if (!has1)
              break;

            if (!object.Equals(en1.Current, en2.Current))
              return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Выполняет сравнение двух перечисляемых объектов, реализующих интерфейс IEnumerable
    /// Возвращает true, если оба перечислителя содержат одинаковое число элементов и элементы попанрно совпадают.
    /// Порядок элементов в перечислители имеет значение
    /// Для сравнение элементов используется интерфейс IEquatable
    /// </summary>
    /// <typeparam name="T">Тип объектов</typeparam>
    /// <param name="list1">Первый сравниваемый объек</param>
    /// <param name="list2">Второй сравниваемый объек</param>
    /// <param name="comparer">Компаратор</param>
    /// <returns>true, если перечислители содержат одинаковые объекты</returns>
    public static bool AreEnumerablesEqual<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
    {
      if (comparer == null)
        return AreEnumerablesEqual<T>(list1, list2);
#if DEBUG
      if (list1 == null)
        throw new ArgumentNullException("list1");
      if (list2 == null)
        throw new ArgumentNullException("list2");
#endif

      using (IEnumerator<T> en1 = list1.GetEnumerator())
      {
        using (IEnumerator<T> en2 = list2.GetEnumerator())
        {
          en1.Reset();
          en2.Reset();
          while (true)
          {
            bool has1 = en1.MoveNext();
            bool has2 = en2.MoveNext();
            if (has1 != has2)
              return false; // число элементов не совпадает
            if (!has1)
              break;

            if (!comparer.Equals(en1.Current, en2.Current))
              return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Выполняет сравнение двух перечисляемых объектов, реализующих интерфейс IEnumerable
    /// Возвращает true, если оба перечислителя содержат одинаковое число элементов и элементы попанрно совпадают.
    /// Порядок элементов в перечислители имеет значение
    /// Для сравнение элементов используется Object.Equals()
    /// </summary>
    /// <param name="list1">Первый сравниваемый объек</param>
    /// <param name="list2">Второй сравниваемый объек</param>
    /// <returns>true, если перечислители содержат одинаковые объекты</returns>
    public static bool AreEnumerablesEqual(IEnumerable list1, IEnumerable list2)
    {
      // Нельзя использовать оператор foreach, придется работать с IEnumerator
      IEnumerator en1 = list1.GetEnumerator();
      IEnumerator en2 = list2.GetEnumerator();
      en1.Reset();
      en2.Reset();
      while (true)
      {
        bool has1 = en1.MoveNext();
        bool has2 = en2.MoveNext();
        if (has1 != has2)
          return false; // число элементов не совпадает
        if (!has1)
          break;

        if (!object.Equals(en1.Current, en2.Current))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Выполняет сравнение двух перечисляемых объектов, реализующих интерфейс IEnumerable для строк
    /// Возвращает true, если оба перечислителя содержат одинаковое число элементов и элементы попанрно совпадают.
    /// Порядок элементов в перечислители имеет значение.
    /// </summary>
    /// <param name="list1">Первый сравниваемый объек</param>
    /// <param name="list2">Второй сравниваемый объек</param>
    /// <param name="comparisonType">Режим сравнения для строк</param>
    /// <returns>true, если перечислители содержат одинаковые объекты</returns>
    public static bool AreEnumerablesEqual(IEnumerable<string> list1, IEnumerable<string> list2, StringComparison comparisonType)
    {
#if DEBUG
      if (list1 == null)
        throw new ArgumentNullException("list1");
      if (list2 == null)
        throw new ArgumentNullException("list2");
#endif

      // Нельзя использовать оператор foreach, придется работать с IEnumerator
      using (IEnumerator<string> en1 = list1.GetEnumerator())
      {
        using (IEnumerator<string> en2 = list2.GetEnumerator())
        {
          en1.Reset();
          en2.Reset();
          while (true)
          {
            bool has1 = en1.MoveNext();
            bool has2 = en2.MoveNext();
            if (has1 != has2)
              return false; // число элементов не совпадает
            if (!has1)
              break;

            if (!String.Equals(en1.Current, en2.Current, comparisonType))
              return false;
          }
        }
      }
      return true;
    }

    #endregion

    #region Словари

    /// <summary>
    /// Сравнение двух типизированных словарей
    /// Возвращает true, если совпадает количество элементов в словарях, ключи и соответствующие значения
    /// Для сравнения используется Object.Equals()
    /// </summary>
    /// <typeparam name="TKey">Тип ключа в коллекциях</typeparam>
    /// <typeparam name="TValue">Тип значения в коллекциях</typeparam>
    /// <param name="dict1">Первая коллекция</param>
    /// <param name="dict2">Вторая коллекция</param>
    /// <returns>true, если к</returns>
    public static bool AreDictionariesEqual<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
    {
      if (dict1.Count != dict2.Count)
        return false;
      foreach (KeyValuePair<TKey, TValue> pair in dict1)
      {
        TValue value2;
        if (!dict2.TryGetValue(pair.Key, out value2))
          return false;

        if (!object.Equals(pair.Value, value2))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Сравнение двух нетипизированных словарей
    /// Возвращает true, если совпадает количество элементов в словарях, ключи и соответствующие значения
    /// Для сравнения используется Object.Equals()
    /// </summary>
    /// <param name="dict1">Первая коллекция</param>
    /// <param name="dict2">Вторая коллекция</param>
    /// <returns>true, если к</returns>
    public static bool AreDictionariesEqual(IDictionary dict1, IDictionary dict2)
    {
      if (dict1.Count != dict2.Count)
        return false;
      foreach (DictionaryEntry pair in dict1)
      {
        if (!dict2.Contains(pair.Key))
          return false;

        object value2 = dict2[pair.Key];

        if (!object.Equals(pair.Value, value2))
          return false;
      }

      return true;
    }

    #endregion

    #region Первичный ключ в DataTable

    /// <summary>
    /// Установка первичного ключа таблицы (свойство DataTable.PrimaryKey).
    /// Если <paramref name="columnNames"/> - пустая строка, первичный ключ удаляется из таблицы.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Список полей первичного ключа, разделенных запятыми</param>
    public static void SetPrimaryKey(DataTable table, string columnNames)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (String.IsNullOrEmpty(columnNames))
      {
        table.PrimaryKey = null;
        return;
      }

      if (columnNames.IndexOf(',') < 0)
      {
        // 14.06.2017 Оптимизация
        DataColumn[] cols = new DataColumn[1];
        cols[0] = table.Columns[columnNames];
        if (cols[0] == null)
        {
          ArgumentException e = new ArgumentException("Таблица \"" + table.TableName +
            "\" не содержит столбца \"" + columnNames + "\", которое предполагалось сделать ключевым", "columnNames");
          AddExceptionColumnsInfo(e, table);
          throw e;
        }
        table.PrimaryKey = cols;

        return;
      }

      string[] names = columnNames.Split(',');

      SetPrimaryKey(table, names);
    }

    /// <summary>
    /// Установка первичного ключа таблицы (свойство DataTable.PrimaryKey)
    /// Если <paramref name="columnNames"/> - пустой массив или null, первичный ключ удаляется из таблицы.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Список полей первичного ключа</param>
    public static void SetPrimaryKey(DataTable table, string[] columnNames)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (columnNames == null || columnNames.Length == 0)
      {
        table.PrimaryKey = null;
        return;
      }
      DataColumn[] cols = new DataColumn[columnNames.Length];
      for (int i = 0; i < columnNames.Length; i++)
      {
        cols[i] = table.Columns[columnNames[i]];
        if (cols[i] == null)
        {
          ArgumentException e = new ArgumentException("Таблица \"" + table.TableName +
            "\" не содержит столбца \"" + columnNames[i] + "\", которое предполагалось сделать ключевым", "columnNames");
          AddExceptionColumnsInfo(e, table);
          throw e;
        }
      }
      table.PrimaryKey = cols;
    }

    /// <summary>
    /// Получение первичного ключа таблицы в виде строки-списка полей,
    /// разделенных запятыми
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Строка имен ключевых полей. Если ключевых полей нет, то пустая строка ""</returns>
    public static string GetPrimaryKey(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (table.PrimaryKey == null)
        return String.Empty;
      if (table.PrimaryKey.Length == 0)
        return String.Empty;
      string s = table.PrimaryKey[0].ColumnName;
      for (int i = 1; i < table.PrimaryKey.Length; i++)
        s += "," + table.PrimaryKey[i].ColumnName;
      return s;
    }

    /// <summary>
    /// Установка первичного ключа таблицы, если он отличается от требуемого
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Список требуемых ключевых полей, разделенный запятями</param>
    public static void CheckPrimaryKey(DataTable table, string columnNames)
    {
      string s = GetPrimaryKey(table);
      if (columnNames != s)
        SetPrimaryKey(table, columnNames);
    }

    /// <summary>
    /// Получить значения полей первичного ключа для массива строк для таблицы данных
    /// Возвращается двумерный массив, первая размерность которого соответствует
    /// количеству полей первичного ключа (обычна равна 1), а вторая - количеству
    /// строк
    /// </summary>
    /// <param name="table">Таблица, в которой находятся строки</param>
    /// <param name="rows">Массив строк, для которых надо получить значения. В массиве могут быть ссылки null</param>
    /// <returns>Двумерный массив</returns>
    public static object[,] GetPrimaryKeyValues(DataTable table, DataRow[] rows)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
        throw new ArgumentException("Таблица \"" + table.TableName + "\" не содержит первичного ключа", "table");

#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      object[,] keyValues = new object[rows.Length, table.PrimaryKey.Length];
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        for (int j = 0; j < table.PrimaryKey.Length; j++)
          keyValues[i, j] = rows[i][table.PrimaryKey[j]];
      }
      return keyValues;
    }

    /// <summary>
    /// Получение массива значений ключевых полей для одной строки.
    /// Возвращает null, если таблица не имеет первичного ключа или строка не задана.
    /// </summary>
    /// <param name="row">Строка DataRow</param>
    /// <returns>Значения ключевых полей</returns>
    public static object[] GetPrimaryKeyValues(DataRow row)
    {
      if (row == null)
        return null;
      return GetPrimaryKeyValues(row, row.Table);
    }

    /// <summary>
    /// Получение массива значений ключевых полей для одной строки.
    /// Возвращает null, если таблица не имеет первичного ключа или строка не задана
    /// Строка Row может принадлежать другой таблице, чем та, в которой определяется ключ
    /// </summary>
    /// <param name="row">Строка DataRow</param>
    /// <param name="table">Таблица, в которой определен PrimaryKey</param>
    /// <returns>Значения ключевых полей</returns>
    public static object[] GetPrimaryKeyValues(DataRow row, DataTable table)
    {
      if (row == null)
        return null;
      if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
        return null;
      object[] keyValues = new object[table.PrimaryKey.Length];
      for (int i = 0; i < keyValues.Length; i++)
      {
        string colName = table.PrimaryKey[i].ColumnName;
        keyValues[i] = row[colName];
      }
      return keyValues;
    }

    /// <summary>
    /// Возвращает массив строк DataRow, в которых значения первичного ключа равны значениям
    /// в соответствующей строке двумерного массива.
    /// Если для очередной строки со значениями нет строки в таблице, соответствующий элемент в
    /// результирующем массиве равен null.
    /// </summary>
    /// <param name="table">Таблица данных, в которой задан первичный ключ по одному или нескольким полям</param>
    /// <param name="keyValues">Массив значений для поиска. Строки двумерного массива определяют длину 
    /// результирующего массива. Каждая строка задает значения для поиска первичного ключа.
    /// Количество столбцов в двумерном массиве должно быть равно количеству полей в первичном ключе таблицы</param>
    /// <returns>Массив найденных строк таблицы. В массиве могут быть значения null</returns>
    public static DataRow[] GetPrimaryKeyRows(DataTable table, object[,] keyValues)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
        throw new ArgumentException("Таблица \"" + table.TableName + "\" не содержит первичного ключа", "table");

#if DEBUG
      if (keyValues == null)
        throw new ArgumentNullException("KeyValues");
#endif

      int n = table.PrimaryKey.Length;
      if (keyValues.GetLength(1) != n)
        throw new ArgumentException("Размерность массива (" + keyValues.GetLength(1).ToString() + ") не соответствует количеству ключевых полей (" + n.ToString() + ") таблицы \"" + table.TableName + "\"", "keyValues");

      object[] findKeys = null;
      if (n > 1)
        findKeys = new object[n]; // сюда будем копировать ключи для поиска

      DataRow[] rows = new DataRow[keyValues.GetLength(0)];
      for (int i = 0; i < rows.Length; i++)
      {
        if (keyValues[i, 0] == null)
          continue;
        if (n == 1)
          // Простой поиск по одному столбцу. Копирование не нужно
          rows[i] = table.Rows.Find(keyValues[i, 0]);
        else
        {
          // Требуется дополнительное копирование
          for (int j = 0; j < n; j++)
            findKeys[j] = keyValues[i, j];
          rows[i] = table.Rows.Find(findKeys);
        }
      }
      return rows;
    }

    /// <summary>
    /// Найти строку в таблице с заданным значением ключевого поля. Если таблица
    /// не содержит строки с заданным ключом, то строка добавляется
    /// </summary>
    /// <param name="table">Таблица, содержащая первичный ключ для одного поля</param>
    /// <param name="keyValue">Значение ключевого поля</param>
    /// <returns>Найденная или созданная строка</returns>
    public static DataRow FindOrAddPrimaryKeyRow(DataTable table, object keyValue)
    {
      DataRow row;
      FindOrAddPrimaryKeyRow(table, keyValue, out row);
      return row;
    }

    /// <summary>
    /// Найти строку в таблице с заданным значением ключевого поля. Если таблица
    /// не содержит строки с заданным ключом, то строка добавляется
    /// Возвращает true, если была создана новая строка
    /// </summary>
    /// <param name="table">Таблица, содержащая первичный ключ для одного поля</param>
    /// <param name="keyValue">Значение ключевого поля</param>
    /// <param name="row">Найденная или созданная строка</param>
    /// <returns>Признак добавления строки</returns>
    public static bool FindOrAddPrimaryKeyRow(DataTable table, object keyValue, out DataRow row)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("Table");
#endif
      int nPK = 0; // 27.12.2020
      if (table.PrimaryKey != null)
        nPK = table.PrimaryKey.Length;
      if (nPK != 1)
        throw new ArgumentException("Таблица \"" + table.TableName + "\" должна иметь одно ключевое поле, а не " + nPK.ToString(), "table");

      row = table.Rows.Find(keyValue);
      if (row == null)
      {
        row = table.NewRow();
        row[table.PrimaryKey[0]] = keyValue;
        table.Rows.Add(row);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Найти строку в таблице с заданными значениями ключевых полей. Если таблица
    /// не содержит строки с заданным ключом, то строка добавляется
    /// </summary>
    /// <param name="table">Таблица, содержащая первичный ключ для одного или нескольких полей</param>
    /// <param name="keyValues">Значения ключевых полей</param>
    /// <returns>Найденная или созданная строка</returns>
    public static DataRow FindOrAddPrimaryKeyRow(DataTable table, object[] keyValues)
    {
      DataRow row;
      FindOrAddPrimaryKeyRow(table, keyValues, out row);
      return row;
    }

    /// <summary>
    /// Найти строку в таблице с заданными значениями ключевых полей. Если таблица
    /// не содержит строки с заданным ключом, то строка добавляется
    /// Возвращает true, если была создана новая строка
    /// </summary>
    /// <param name="table">Таблица, содержащая первичный ключ для одного или нескольких полей</param>
    /// <param name="keyValues">Значения ключевых полей</param>
    /// <param name="row">Найденная или созданная строка</param>
    /// <returns>Признак добавления строки</returns>
    public static bool FindOrAddPrimaryKeyRow(DataTable table, object[] keyValues, out DataRow row)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("Table");
      if (keyValues == null)
        throw new ArgumentNullException("KeyValues");
      int n;
      if (table.PrimaryKey == null)
        n = 0;
      else
        n = table.PrimaryKey.Length;
      if (n != keyValues.Length)
        throw new ArgumentException("Число значений в массиве KeyValues (" + keyValues.Length.ToString() +
          ") не совпадает с числом ключевых полей (" + n.ToString() + ") в таблице \"" + table.TableName + "\"");
#endif

      row = table.Rows.Find(keyValues);
      if (row == null)
      {
        row = table.NewRow();
        for (int i = 0; i < keyValues.Length; i++)
          row[table.PrimaryKey[i]] = keyValues[i];
        table.Rows.Add(row);
        return true;
      }
      return false;
    }

    #endregion

    #region Отношения "Мастер-детали" между DataTable

    /// <summary>
    /// Создание объекта DataRelation для отношения "Мастер-детали" между двумя
    /// таблицами. Мастер-таблица должна иметь первичный ключ
    /// </summary>
    /// <param name="masterTable">Главная таблица ("Customers")</param>
    /// <param name="detailsTable">Подчиненная таблица ("Orders")</param>
    /// <param name="referenceColumn">Имя столбца ("CustomerId") в подчиненной таблице, которое
    /// будет связано с ключевым полем в <paramref name="masterTable"/>.</param>
    /// <param name="relationName">Имя связи (свойство DataRelation.RelationName.
    /// Если не задано, то будет сгенерировано автоматически</param>
    public static void AddRelation(DataTable masterTable,
      DataTable detailsTable, string referenceColumn, string relationName)
    {
#if DEBUG
      if (masterTable == null)
        throw new ArgumentNullException("masterTable");
      if (detailsTable == null)
        throw new ArgumentNullException("detailsTable");
      if (String.IsNullOrEmpty(referenceColumn))
        throw new ArgumentNullException("referenceColumn");
#endif
      if (masterTable.DataSet != detailsTable.DataSet)
        throw new ArgumentException("Таблицы должны относится к одному DataSet'у", "detailsTable");
      if (masterTable.PrimaryKey == null || masterTable.PrimaryKey.Length == 0)
        throw new ArgumentException("Мастер таблица не имеет первичного ключа", "masterTable");

      string[] aColNames = referenceColumn.Split(',');
      DataColumn[] refCols = new DataColumn[aColNames.Length];
      for (int i = 0; i < refCols.Length; i++)
      {
        refCols[i] = detailsTable.Columns[aColNames[i]];
        if (refCols[i] == null)
          throw new InvalidOperationException("Таблица \"" + detailsTable.TableName + "\" не имеет столбца \"" +
            aColNames[i] + "\", которое предполагалось использовать для связывания");
      }
      if (refCols.Length != masterTable.PrimaryKey.Length)
        throw new InvalidOperationException("В мастер-таблице \"" + masterTable.TableName + "\" объявлены ключевые поля \"" +
          GetPrimaryKey(masterTable) + "\" (" + masterTable.PrimaryKey.Length.ToString() +
          " шт.) Нельзя использовать для связи поля \"" + referenceColumn +
          "\" (" + refCols.Length.ToString() + " шт). Количество полей должно быть одинаковым");

      if (String.IsNullOrEmpty(relationName))
        relationName = referenceColumn.Replace(',', '_') + "_Ref";
      DataRelation Rel = new DataRelation(relationName, masterTable.PrimaryKey, refCols);
      detailsTable.DataSet.Relations.Add(Rel);
    }


    #endregion

    #region Клонирование таблиц с выборочными строками

    /// <summary>
    /// Клонирование таблицы и ее строк, для которых выставлены флаги.
    /// Строки, для которых флаг не установлен, не добавляются в новую таблицу.
    /// Всегда возвращается копия таблицы, даже если флаги выставлены для всех строк.
    /// </summary>
    /// <param name="table">Исходная таблица</param>
    /// <param name="flags">Массив флагов, по одному для каждой строки</param>
    /// <returns>Таблица-копия</returns>
    public static DataTable CloneTableForSelectedRows(DataTable table, bool[] flags)
    {
      if (flags.Length != table.Rows.Count)
        throw new ArgumentException("Длина массива флагов (" + flags.Length.ToString() +
          ") не совпадает с количеством строк в таблице (" + table.Rows.Count.ToString() + ")");

      DataTable resTable = table.Clone();
      for (int i = 0; i < flags.Length; i++)
      {
        if (flags[i])
          resTable.Rows.Add(table.Rows[i].ItemArray);
      }
      return resTable;
    }

    /// <summary>
    /// Клонирование таблицы и ее строк, для которых выставлены флаги. Строки,
    /// для которых флаг не установлен, не добавляются в новую таблицу.
    /// Если флаги выставлены для всех строк, то клонирование не выполняется, а
    /// возвращается оригинальная таблица Table
    /// </summary>
    /// <param name="table">Исходная таблица</param>
    /// <param name="flags">Массив флагов, по одному для каждой строки</param>
    /// <returns>Таблица-копия</returns>
    public static DataTable CloneOrSameTableForSelectedRows(DataTable table, bool[] flags)
    {
      if (Array.IndexOf<bool>(flags, false) < 0)
        // Все строки выбраны
        return table;
      else
        return CloneTableForSelectedRows(table, flags);
    }

    /// <summary>
    /// Создать копию таблицы Table, содержащую строки с идентификаторами <paramref name="ids"/>.
    /// Создается копия таблицы, даже если исходная таблица уже содержит все строки
    /// в нужном порядке.
    /// Исходная таблица должна быть проиндексирована по числовому полю.
    /// если какой-либо ключ из массива <paramref name="ids"/> не будет найден в таблице, то будет 
    /// сгенерировано исключение
    /// </summary>
    /// <param name="table">Исходная таблица</param>
    /// <param name="ids">Массив значений ключаевого поля. Не должен содержать
    /// повторяющихся значений</param>
    /// <returns>Копия таблицы, содержащая <paramref name="ids"/>.Length строк</returns>
    public static DataTable CloneTableForSelectedIds(DataTable table, Int32[] ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      string sPK = GetPrimaryKey(table);
      if (sPK.Length == 0)
        throw new ArgumentException("У таблицы " + table.TableName + " не задан первичный ключ");
      if (sPK.IndexOf(',') >= 0)
        throw new ArgumentException("У таблицы " + table.TableName + " задан составной первичный ключ");

      DataTable table2 = table.Clone();
      for (int i = 0; i < ids.Length; i++)
      {
        DataRow row1 = table.Rows.Find(ids[i]);
        if (row1 == null)
          throw new InvalidOperationException("Для таблицы \"" + table.TableName + "\" не удалось получить строку со значением первичного ключа " + sPK + "=" + ids[i].ToString());
        table2.Rows.Add(row1.ItemArray);
      }
      return table2;
    }

    /// <summary>
    /// Создать копию таблицы <paramref name="table"/>, содержащую строки с идентификаторами <paramref name="ids"/>.
    /// Если исходная таблица уже содержит все строки в нужном порядке, то копирование
    /// не выполняется, а возвращается исходная таблица
    /// Исходная таблица должна быть проиндексирована по числовому полю.
    /// если какой-либо ключ из массива <paramref name="ids"/> не будет найден в таблице, то будет 
    /// сгенерировано исключение
    /// </summary>
    /// <param name="table">Исходная таблица</param>
    /// <param name="ids">Массив значений ключаевого поля. Не должен содержать
    /// повторяющихся значений</param>
    /// <returns>Копия таблицы, содержащая Values.Length строк</returns>
    public static DataTable CloneOrSameTableForSelectedIds(DataTable table, Int32[] ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      string sPK = GetPrimaryKey(table);
      if (sPK.Length == 0)
        throw new ArgumentException("У таблицы " + table.TableName + " не задан первичный ключ");
      if (sPK.IndexOf(',') >= 0)
        throw new ArgumentException("У таблицы " + table.TableName + " задан составной первичный ключ");

      // Сначала пытаемся проверить, не подойдет ли исходная таблица
      if (table.Rows.Count == ids.Length)
      {
        DataRowInt64Extractor extId = new DataRowInt64Extractor(sPK); // исправлено 17.12.2021

        bool isGood = true;
        for (int i = 0; i < ids.Length; i++)
        {
          if (extId[table.Rows[i]] != ids[i])
          {
            isGood = false;
            break;
          }
        }
        if (isGood)
          return table;
      }

      // Возвращаем копию таблицы
      return CloneTableForSelectedIds(table, ids);
    }

    #endregion

    #region Вспомогательные функции для работы с DataSet'ом

    #region GetRowCount

    /// <summary>
    /// Подсчет числа всех строк во всех таблицах
    /// </summary>
    /// <param name="ds">Набор данных, в котором перебираютися таблицы</param>
    /// <returns>Общее количество строк</returns>
    public static int GetRowCount(DataSet ds)
    {
      if (ds == null)
        return 0;

      int n = 0;
      for (int i = 0; i < ds.Tables.Count; i++)
        n += ds.Tables[i].Rows.Count;
      return n;
    }

    #endregion

    #region AddTableToDataSet

    /// <summary>
    /// Присоединение таблицы к датасету. Если таблица уже присоединена к другому
    /// набору, то она оттуда удаляется.
    /// Если в целевом наборе <paramref name="ds"/> уже есть таблица с совпадающим именем,
    /// она удаляется из набора
    /// </summary>
    /// <param name="ds">Целевой набор данных. Не может быть null</param>
    /// <param name="table">Присоединяемыая таблицы. Не может быть null</param>
    public static void AddTableToDataSet(DataSet ds, DataTable table)
    {
      if (ds == null) // эта проверка обязательно нужна
        throw new ArgumentNullException("ds");
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (Object.ReferenceEquals(table.DataSet, ds))
        return; // 17.12.2021

      if (table.DataSet != null)
        table.DataSet.Tables.Remove(table);

      // 14.09.2015
      // Убираем существующую таблицу с совпадающим именем
      if (!String.IsNullOrEmpty(table.TableName))
      {
        if (ds.Tables.Contains(table.TableName))
          ds.Tables.Remove(table.TableName);
      }

      // 27.05.2022
      // Восстанавливаем значения свойств для DefaultView
      // При добавлении таблицы в DataSet, свойство DataTable.DefaultView очищается (см. исходники Net Framework, внутренний метод DataTable.SetDataSet()).

      string rf = table.DefaultView.RowFilter;
      DataViewRowState rsf = table.DefaultView.RowStateFilter;
      string sort = table.DefaultView.Sort;

      ds.Tables.Add(table);

      table.DefaultView.RowFilter = rf;
      table.DefaultView.RowStateFilter = rsf;
      table.DefaultView.Sort = sort;
    }

    #endregion

    #region GetValues

    /// <summary>
    /// Извлечение массива значений полей из строки.
    /// Для не существующих имен полей возвращается null.
    /// DBNull также заменяется на null
    /// </summary>
    /// <param name="row">Строка данных (иожет быть null)</param>
    /// <param name="columnNames">Массив имен полей, разделенных запятыми</param>
    /// <returns>Значения полей</returns>
    public static object[] GetValues(DataRow row, string columnNames)
    {
      string[] aNames = columnNames.Split(',');
      object[] res = new object[aNames.Length];
      if (row == null)
        return res;
      for (int i = 0; i < aNames.Length; i++)
      {
        int p = row.Table.Columns.IndexOf(aNames[i]);
        if (p < 0)
          continue;
        if (row.IsNull(p))
          continue;
        res[i] = row[p];
      }
      return res;
    }

    #endregion

    #region GetIdsFromColumn

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIdsFromColumn(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      int colPos = GetColumnPosWithCheck(table, columnName);
      DataColumn col = table.Columns[colPos];

      SingleScopeList<Int32> ids = null;
      foreach (DataRow row in table.Rows)
      {
        Int32 id;
        if (row.RowState == DataRowState.Deleted)
        {
          if (row.IsNull(col, DataRowVersion.Original))
            continue;
          id = (Int32)(row[colPos, DataRowVersion.Original]);
        }
        else
        {
          if (row.IsNull(colPos))
            continue;
          id = (Int32)(row[colPos]);
        }
        if (id == 0)
          continue;

        if (ids == null)
          ids = new SingleScopeList<Int32>();
        ids.Add(id);
      }

      if (ids == null)
        return EmptyIds;
      else
        return ids.ToArray();
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице для строк, относящихся к 
    /// объекту DataView.
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="dv">Коллекция строк таблицы данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIdsFromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      if (dv.Count == 0)
        return EmptyIds;

      int colPos = GetColumnPosWithCheck(dv.Table, columnName);
      DataColumn col = dv.Table.Columns[colPos];

      SingleScopeList<Int32> ids = null;
      for (int i = 0; i < dv.Count; i++)
      {
        Int32 id;
        if (dv[i].Row.RowState == DataRowState.Deleted)
        {
          if (dv[i].Row.IsNull(col, DataRowVersion.Original))
            continue;
          id = (int)(dv[i].Row[colPos, DataRowVersion.Original]);
        }
        else
        {
          if (dv[i].Row.IsNull(colPos))
            continue;
          id = (int)(dv[i].Row[colPos]);
        }
        if (id == 0)
          continue;

        if (ids == null)
          ids = new SingleScopeList<Int32>();
        ids.Add(id);
      }

      if (ids == null)
        return EmptyIds;
      else
        return ids.ToArray();
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле для строк таблицы в массиве. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру
    /// </summary>
    /// <param name="rows">Массив однотипных строк</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIdsFromColumn(ICollection<DataRow> rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      SingleScopeList<Int32> ids = null;
      int colPos = -1;
      DataColumn col = null;

      foreach (DataRow row in rows)
      {
        if (colPos < 0)
        {
          colPos = GetColumnPosWithCheck(row.Table, columnName);
          col = row.Table.Columns[colPos];
        }

        Int32 id;
        if (row.RowState == DataRowState.Deleted)
        {
          if (row.IsNull(col, DataRowVersion.Original))
            continue;
          id = (Int32)(row[colPos, DataRowVersion.Original]);
        }
        else
        {
          if (row.IsNull(colPos))
            continue;
          id = (Int32)(row[colPos]);
        }
        if (id == 0)
          continue;
        if (ids == null)
          ids = new SingleScopeList<Int32>();
        ids.Add(id);
      }
      if (ids == null)
        return EmptyIds;
      else
        return ids.ToArray();
    }
    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле для строк таблицы в массиве. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру
    /// </summary>
    /// <param name="rows">Массив однотипных строк как коллекция DataRowView</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIdsFromColumn(ICollection<DataRowView> rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("Rows");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("ColumnName");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      SingleScopeList<Int32> ids = null;
      int colPos = -1;
      DataColumn col = null;
      foreach (DataRowView drv in rows)
      {
        if (colPos < 0)
        {
          colPos = GetColumnPosWithCheck(drv.Row.Table, columnName);
          col = drv.Row.Table.Columns[colPos];
        }

        Int32 id;
        if (drv.Row.RowState == DataRowState.Deleted)
        {
          if (drv.Row.IsNull(col, DataRowVersion.Original))
            continue;
          id = (Int32)(drv.Row[colPos, DataRowVersion.Original]);
        }
        else
        {
          if (drv.Row.IsNull(colPos))
            continue;
          id = (Int32)(drv.Row[colPos]);
        }
        if (id == 0)
          continue;
        if (ids == null)
          ids = new SingleScopeList<Int32>();
        ids.Add(id);
      }
      if (ids == null)
        return EmptyIds;
      else
        return ids.ToArray();
    }

    #endregion

    #region GetIds

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице.
    /// Обрабатываются все строки, включая удаленные
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static Int32[] GetIds(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      int colPos = GetColumnPosWithCheck(table, "Id");

      if (table.Rows.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[table.Rows.Count];
      for (int i = 0; i < res.Length; i++)
      {
        if (table.Rows[i].RowState == DataRowState.Deleted)
          res[i] = (Int32)(table.Rows[i][colPos, DataRowVersion.Original]);
        else
          res[i] = (Int32)(table.Rows[i][colPos]);
      }
      return res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице для 
    /// строк, входящих в просмотр DataView
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре.
    /// Обрабатываются все строки, включая удаленные
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static Int32[] GetIds(DataView dv)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      int colPos = GetColumnPosWithCheck(dv.Table, "Id");

      if (dv.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[dv.Count];
      for (int i = 0; i < res.Length; i++)
      {
        DataRow Row = dv[i].Row;
        if (Row.RowState == DataRowState.Deleted)
          res[i] = (Int32)(Row[colPos, DataRowVersion.Original]);
        else
          res[i] = (Int32)(Row[colPos]);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк. В отличие от GetIdsFromColumn()
    /// не проверяет нулевые значения и не проверяет повторы.
    /// Обрабатываются все строки, включая удаленные
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIds(ICollection<DataRow> rows)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[rows.Count];
      int index = 0;
      int colPos = -1;
      foreach (DataRow row in rows)
      {
        if (index == 0)
          colPos = GetColumnPosWithCheck(row.Table, "Id");

        if (row.RowState == DataRowState.Deleted)
          res[index] = (Int32)(row[colPos, DataRowVersion.Original]);
        else
          res[index] = (Int32)(row[colPos]);
        index++;
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк DataRowView. В отличие от GetIdsFromColumn()
    /// не проверяет нулевые значения и не проверяет повторы.
    /// Обрабатываются все строки, включая удаленные
    /// </summary>
    /// <param name="rows">Массив строк типа DataRowView</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIds(ICollection<DataRowView> rows)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[rows.Count];
      int index = 0;
      int colPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (index == 0)
          colPos = GetColumnPosWithCheck(drv.Row.Table, "Id");

        if (drv.Row.RowState == DataRowState.Deleted)
          res[index] = (Int32)(drv.Row[colPos, DataRowVersion.Original]);
        else
          res[index] = (Int32)(drv.Row[colPos]);
        index++;
      }
      return res;
    }

    #endregion

    #region GetFirstId, GetLastId

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из первой строки таблицы
    /// Возвращает 0, если таблица не содержит строк
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetFirstId(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (table.Rows.Count == 0)
        return 0;

      DataRow row = table.Rows[0];
      return DataTools.GetInt(row, "Id");
    }

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из последней строки таблицы
    /// Возвращает 0, если таблица не содержит строк
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetLastId(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (table.Rows.Count == 0)
        return 0;

      DataRow row = table.Rows[table.Rows.Count - 1];
      return DataTools.GetInt(row, "Id");
    }

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из первой строки просмотра таблицы
    /// Возвращает 0, если DataView не содержит строк
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetFirstId(DataView dv)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      if (dv.Count == 0)
        return 0;

      DataRow row = dv[0].Row;
      return DataTools.GetInt(row, "Id");
    }

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из последней строки просмотра таблицы
    /// Возвращает 0, если DataView не содержит строк
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetLastId(DataView dv)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      if (dv.Count == 0)
        return 0;

      DataRow row = dv[dv.Count - 1].Row;
      return DataTools.GetInt(row, "Id");
    }

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из первой строки в массиве строк
    /// Возвращает 0, если массив пустой
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetFirstId(IList<DataRow> rows)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return 0;

      DataRow row = rows[0];
      return DataTools.GetInt(row, "Id");
    }

    /// <summary>
    /// Получить идентификатор для ключевого поля "Id" из последней строки в массиве строк
    /// Возвращает 0, если массив пустой
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <returns>Идентификатор строки или 0</returns>
    public static Int32 GetLastId(IList<DataRow> rows)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return 0;

      DataRow row = rows[rows.Count - 1];
      return DataTools.GetInt(row, "Id");
    }

    #endregion

    #region GetBlockedIds

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="n"></param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static Int32[][] GetBlockedIds(DataTable table, int n)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      int ColPos = GetColumnPosWithCheck(table, "Id");

      if (n < 1)
        throw new ArgumentException("N<1", "n");

      int nn = ((table.Rows.Count + (n - 1))) / n;
      Int32[][] res = new Int32[nn][];

      for (int i = 0; i < nn; i++)
      {
        if (i == (nn - 1))
          res[i] = new Int32[table.Rows.Count - (nn - 1) * n];
        else
          res[i] = new Int32[n];
      }

      for (int index = 0; index < table.Rows.Count; index++)
      {
        int idx1 = index / n;
        int idx2 = index - (idx1 * n);

        if (table.Rows[index].RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(table.Rows[index][ColPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(table.Rows[index][ColPos]);
      }
      return res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице для 
    /// строк, входящих в просмотр DataView
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <param name="n"></param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static Int32[][] GetBlockedIds(DataView dv, int n)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      int colPos = GetColumnPosWithCheck(dv.Table, "Id");

      if (n < 1)
        throw new ArgumentOutOfRangeException("N", n, "N<1");

      int nn = ((dv.Count + (n - 1))) / n;

      Int32[][] res = new Int32[nn][];
      for (int i = 0; i < nn; i++)
      {
        if (i == (nn - 1))
          res[i] = new Int32[dv.Count - (nn - 1) * n];
        else
          res[i] = new Int32[n];
      }

      for (int index = 0; index < dv.Count; index++)
      {
        int idx1 = index / n;
        int idx2 = index - (idx1 * n);
        DataRow row = dv[index].Row;
        if (row.RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(row[colPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(row[colPos]);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк.
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <param name="n">Количество элементов в результирующих массивах</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[][] GetBlockedIds(ICollection<DataRow> rows, int n)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("Rows");
#endif
      if (n < 1)
        throw new ArgumentOutOfRangeException("N", n, "N<1");

      int nn = ((rows.Count + (n - 1))) / n;

      Int32[][] res = new Int32[nn][];
      if (rows.Count == 0)
        return res;

      for (int i = 0; i < nn; i++)
      {
        if (i == (nn - 1))
          res[i] = new Int32[rows.Count - (nn - 1) * n];
        else
          res[i] = new Int32[n];
      }

      int index = 0;
      int colPos = -1;
      foreach (DataRow row in rows)
      {
        if (index == 0)
          colPos = GetColumnPosWithCheck(row.Table, "Id");

        int idx1 = index / n;
        int idx2 = index - (idx1 * n);
        if (row.RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(row[colPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(row[colPos]);
        index++;
      }
      return res;
    }

    #endregion

    #region GetRowsFromIds

    /// <summary>
    /// Получить массив строк DataRow из таблицы для массива идентификаторов Id
    /// Результирующий массив будет содержать значения null для ненайденных или
    /// нулевых идентификаторов
    /// </summary>
    /// <param name="table">Таблица, содержащая поле "Id"</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] GetRowsFromIds(DataTable table, Int32[] ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (ids.Length == 0)
        return new DataRow[0];

      /*
      string CurrKey = GetPrimaryKey(Table);
      bool ValidKey = (CurrKey == "Id");
      if (!ValidKey)
        SetPrimaryKey(Table, "Id");

      DataRow[] res = new DataRow[Ids.Length];
      for (int i = 0; i < Ids.Length; i++)
      {
        if (Ids[i] == 0)
          continue;
        res[i] = Table.Rows.Find(Ids[i]);
      }

      if (!ValidKey)
        SetPrimaryKey(Table, CurrKey);
      return res;
       * */

      // Не стоит переключать первичный ключ таблицы. Это вызовет ошибку, если в столбце Id есть 
      // DBNull или повторы (нули)

      DataRow[] res = new DataRow[ids.Length];
      if (IsPrimaryIdKey(table))
      {
        // Простой поиск по первичному ключу
        for (int i = 0; i < ids.Length; i++)
        {
          if (ids[i] == 0)
            continue;
          res[i] = table.Rows.Find(ids[i]);
        }
      }
      else
      {
#if DEBUG
        GetColumnPosWithCheck(table, "Id");
#endif

        if (ids.Length == 1)
        {
          // Нет смиысла создавать DataView на один раз
          if (ids[0] != 0)
          {
            DataRow[] Row2 = table.Select("Id=" + ids[0]);
            if (Row2.Length > 0)
              res[0] = Row2[0];
          }
        }
        else
        {
          // Создаем временный DataView
          using (DataView dv = new DataView(table))
          {
            dv.Sort = "Id";
            for (int i = 0; i < ids.Length; i++)
            {
              if (ids[i] == 0)
                continue;
              int p = dv.Find(ids[i]);
              if (p >= 0)
                res[i] = dv[p].Row;
            }
          }
        }
      }

      return res;
    }

    /// <summary>
    /// Возвращает true, если таблица содержит первичный ключ по полю "Id"
    /// </summary>
    /// <param name="table"></param>
    /// <returns>Наличие ключа</returns>
    private static bool IsPrimaryIdKey(DataTable table)
    {
      if (table.PrimaryKey.Length == 1)
        return String.Equals(table.PrimaryKey[0].ColumnName, "Id", StringComparison.OrdinalIgnoreCase);
      else
        return false;
    }

    #endregion

    #region TableFromIds()

    /// <summary>
    /// Создает таблицу, содержащую единственный столбец "Id"
    /// </summary>
    /// <param name="ids">Массив идентификаторов. 
    /// Если null, то будет возвращен пустой массив</param>
    /// <returns>Таблица со столбцом "Id"</returns>
    public static DataTable TableFromIds(Int32[] ids)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(Int32));
      if (ids != null)
      {
        for (int i = 0; i < ids.Length; i++)
          table.Rows.Add(ids[i]);
      }
      return table;
    }

    #endregion

    #region GetDataTableRows

    /// <summary>
    /// Копирование строк таблицы DataTable в массив
    /// </summary>
    /// <param name="table">Таблица, откуда берутся строки.
    /// Если null, то будет возвращен пустой массив.</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] GetDataTableRows(DataTable table)
    {
      if (table == null)
        return new DataRow[0];

      DataRow[] res = new DataRow[table.Rows.Count];
      table.Rows.CopyTo(res, 0);
      return res;
    }

    /// <summary>
    /// Копирование строк таблицы DataTable в массив.
    /// Копируются строки из выбранного диапазона.
    /// Параметры диапазона Start и Count должны находится в пределах Table.RowList.Count
    /// </summary>
    /// <param name="table">Таблица, откуда берутся строки</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="count">Количество строк</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] GetDataTableRows(DataTable table, int startIndex, int count)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("Table");
#endif

      DataRow[] res = new DataRow[count];
      for (int i = 0; i < count; i++)
        res[i] = table.Rows[startIndex + i];
      return res;
    }

    #endregion

    #region GetDataRowEnumerable()

#if XXX
    /// <summary>
    /// Возвращает объект, реализующий перечислитель по строкам множества таблиц.
    /// В списке таблиц могут быть элементы null
    /// </summary>
    /// <param name="Tables">Перечислитель таблиц (например, массив)</param>
    /// <returns>Объект, реализующий перечислитель строк</returns>
    public static IEnumerable<DataRow> GetDataRowEnumerable(IEnumerable<DataTable> Tables)
    {
      if (Tables != null)
      {
        foreach (DataTable Table in Tables)
        {
          if (Table != null)
          {
            foreach (DataRow Row in Table.Rows)
              yield return Row;
          }
        }
      }
    }

    /// <summary>
    /// Возвращает объект, реализующий перечислитель по строкам множества таблиц, заданных объектами DataView.
    /// В списке просмотров могут быть элементы null
    /// </summary>
    /// <param name="Views">Перечислитель объектов DataView (например, массив)</param>
    /// <returns>Объект, реализующий перечислитель строк</returns>
    public static IEnumerable<DataRow> GetDataRowEnumerable(IEnumerable<DataView> Views)
    {
      if (Views != null)
      {
        foreach (DataView View in Views)
        {
          if (View != null)
          {
            foreach (DataRowView drv in View)
              yield return drv.Row;
          }
        }
      }
    }
#endif

    /// <summary>
    /// Получение объекта для перебора строк DataRow.
    /// В качестве источника строк могут выступать: DataTable, DataView, массивы любой размерности, 
    /// включая jagged, коллекции, реализующие IEnumerable, одиночные DataRow и DataRowView.
    /// Значения null пропускаются.
    /// Для массивов используется рекурсивное перечисление элементов.
    /// Если источник данных содержит ссылку на объект неподдерживаемого типа, генерируется исключение.
    /// </summary>
    /// <param name="source">Источник данных</param>
    /// <returns>Объект, реализующий интерфейс IEnumerable для строк DataRow</returns>
    public static IEnumerable<DataRow> GetDataRowEnumerable(object source)
    {
      return GetDataRowEnumerable(source, false);
    }

    /// <summary>
    /// Получение объекта для перебора строк DataRow.
    /// В качестве источника строк могут выступать: DataTable, DataView, массивы любой размерности, 
    /// включая jagged, коллекции, реализующие IEnumerable, одиночные DataRow и DataRowView.
    /// Значения null пропускаются.
    /// Для массивов используется рекурсивное перечисление элементов.
    /// </summary>
    /// <param name="source">Источник данных</param>
    /// <param name="skipBadSource">Определяет действие, когда в источнике данных обнаружен 
    /// элемент неизвестного типа.
    /// Если true, то элемент пропускается. Если false, то будет сгенерировано исключение</param>
    /// <returns>Объект, реализующий интерфейс IEnumerable для строк DataRow</returns>
    public static IEnumerable<DataRow> GetDataRowEnumerable(object source, bool skipBadSource)
    {
      // Используем готовые перечислители
      // Нельзя, т.к. DataRowCollection реализует только IEnumerable, но не IEnumerable<DataRow>
      // if (Source is DataTable)
      //   return ((DataTable)Source).Rows;

      IEnumerable<DataRow> source2 = source as IEnumerable<DataRow>;
      if (source2 != null)
        return source2;

      // Реализуем перечислитель через "машину состояний" (оператор yield return)
      // Обязательно требуется отдельный метод
      return GetDataRowEnumerable2(source, skipBadSource);
    }

    /// <summary>
    /// Этот метод реализует "машину состояний"
    /// </summary>
    /// <param name="source"></param>
    /// <param name="skipBadSource"></param>
    /// <returns></returns>
    private static IEnumerable<DataRow> GetDataRowEnumerable2(object source, bool skipBadSource)
    {
      if (source != null)
      {
        if (source is DataTable)
        {
          foreach (DataRow row1 in ((DataTable)source).Rows)
            yield return row1;
        }
        else if (source is DataView)
        {
          foreach (DataRowView drv1 in (DataView)source)
            yield return drv1.Row;
        }
        else if (source is DataRow)
        {
          yield return (DataRow)source;
        }
        else if (source is DataRowView)
        {
          yield return ((DataRowView)source).Row;
        }
        else if (source is IEnumerable)
        {
          if (source is IEnumerable<DataRowView>)
          {
            foreach (DataRowView drv1 in (IEnumerable<DataRowView>)source)
              yield return drv1.Row;
          }
          else
          {
            foreach (object Item1 in (IEnumerable)source)
            {
              IEnumerable<DataRow> en2 = GetDataRowEnumerable(Item1);
              foreach (DataRow row2 in en2)
                yield return row2;
            }
          }
        }
        else
        {
          if (!skipBadSource)
            throw new ArgumentException("Неподдерживаемый тип аргумента: " + source.GetType().ToString(), "source");
        }
      }
    }

    #endregion

    #region GetStringsFromColumn()

    /// <summary>
    /// Получение списка строковых значений поля. 
    /// Пустые строки и повторы отбрасываются.
    /// Регистр символов учитывается.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnName">Имя поля типа String или другого типа,
    /// обрабатываемого функцией GetString()</param>
    /// <returns>Массив идентификаторов</returns>
    public static string[] GetStringsFromColumn(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      int colPos = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<string> values = null;
      foreach (DataRow row in table.Rows)
      {
        string s = DataTools.GetString(row[colPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (values == null)
          values = new SingleScopeList<string>();
        if (!values.Contains(s))
          values.Add(s);
      }

      if (values == null)
        return EmptyStrings;
      else
      {
        string[] a = values.ToArray();
        Array.Sort<string>(a);
        return a;
      }
    }

    /// <summary>
    /// Получение списка строковых значений поля для строк, относящихся к 
    /// объекту DataView.
    /// Пустые строки и повторы отбрасываются
    /// Регистр символов учитывается.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="dv">Коллекция строк таблицы данных</param>
    /// <param name="columnName">Имя поля типа String или другого типа,
    /// обрабатывемого функцией GetString()</param>
    /// <returns>Массив идентификаторов</returns>
    public static string[] GetStringsFromColumn(DataView dv, string columnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      if (dv.Count == 0)
        return DataTools.EmptyStrings;

      int colPos = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<String> values = null;
      for (int i = 0; i < dv.Count; i++)
      {
        string s = DataTools.GetString(dv[i].Row[colPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (values == null)
          values = new SingleScopeList<string>();
        values.Add(s);
      }

      if (values == null)
        return EmptyStrings;
      else
      {
        string[] a = values.ToArray();
        Array.Sort<string>(a);
        return a;
      }
    }

    /// <summary>
    /// Получение списка строковых значений поля для строк таблицы в массиве. 
    /// Пустые строки и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру.
    /// Регистр символов учитывается.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="rows">Массив однотипных строк</param>
    /// <param name="columnName">Имя поля типа String или другого типа,
    /// обрабатывемого функцией GetString()</param>
    /// <returns>Массив идентификаторов</returns>
    public static string[] GetStringsFromColumn(ICollection<DataRow> rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return DataTools.EmptyStrings;

      SingleScopeList<string> values = null;
      int colPos = -1;
      foreach (DataRow row in rows)
      {
        if (colPos < 1)
          colPos = GetColumnPosWithCheck(row.Table, columnName);

        string s = DataTools.GetString(row[colPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (values == null)
          values = new SingleScopeList<string>();
        values.Add(s);
      }
      if (values == null)
        return DataTools.EmptyStrings;
      else
      {
        string[] a = values.ToArray();
        Array.Sort<string>(a);
        return a;
      }
    }

    #endregion

    #region GetValuesFromColumn()

    /// <summary>
    /// Получить значения поля для всех строк таблицы в виде массива
    /// Повторы и пустые значения не отбрасываются. Количество и порядок элементов в массиве соответствуют строкам в таблице
    /// Хранящиеся в таблице значения DBNull заменяются на default.
    /// Если тип массива T не совпадает с типом данных в столбце таблицы, используется метод Convert.ChangeType(). Методы типа GetInt() не применяются
    /// 
    /// Замечания для строковых полей:
    /// - Не выполняется обрезка концевых пробелов, как для DataTools.GetString().
    /// - Для значения DBNull возвращается null, а не пустая строка.
    /// - Может быть удобнее использовать метод GetStringsFromColumn(), но он убирает повторы и пустые строки.
    /// </summary>
    /// <typeparam name="T">Тип поля данных (один из поддерживаемых типов DataColumn.DataType)</typeparam>
    /// <param name="table">Таблица исходных данных</param>
    /// <param name="columnName">Имя столбца, из которого извлекаются значения</param>
    /// <returns>Массив значений поля</returns>
    public static T[] GetValuesFromColumn<T>(DataTable table, string columnName)
    {
      int colPos = GetColumnPosWithCheck(table, columnName);

      T[] res = new T[table.Rows.Count];

      Type resType = typeof(T);
      if (resType == table.Columns[colPos].DataType)
      {
        for (int i = 0; i < table.Rows.Count; i++)
        {
          if (!table.Rows[i].IsNull(colPos))
            res[i] = (T)(table.Rows[i][colPos]);
        }
      }
      else
      {
        for (int i = 0; i < table.Rows.Count; i++)
        {
          if (!table.Rows[i].IsNull(colPos))
            res[i] = (T)(Convert.ChangeType(table.Rows[i][colPos], resType));
        }
      }
      return res;
    }

    /// <summary>
    /// Получить значения поля для всех строк DataView в виде массива
    /// Повторы и пустые значения не отбрасываются. Количество и порядок элементов в массиве соответствуют строкам в просмотре
    /// Хранящиеся в таблице значения DBNull заменяются на default
    /// </summary>
    /// <typeparam name="T">Тип поля данных (один из поддерживаемых типов DataColumn.DataType)</typeparam>
    /// <param name="dv">Просмотр для таблицы исходных данных</param>
    /// <param name="columnName">Имя столбца, из которого извлекаются значения</param>
    /// <returns>Массив значений поля</returns>
    public static T[] GetValuesFromColumn<T>(DataView dv, string columnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif
      int colPos = GetColumnPosWithCheck(dv.Table, columnName);

      T[] res = new T[dv.Count];
      Type resType = typeof(T);
      if (resType == dv.Table.Columns[colPos].DataType)
      {
        for (int i = 0; i < dv.Count; i++)
        {
          DataRow Row = dv[i].Row;
          if (!Row.IsNull(colPos))
            res[i] = (T)(Row[colPos]);
        }
      }
      else
      {
        for (int i = 0; i < dv.Count; i++)
        {
          DataRow Row = dv[i].Row;
          if (!Row.IsNull(colPos))
            res[i] = (T)(Convert.ChangeType(Row[colPos], resType));
        }
      }
      return res;
    }


    /// <summary>
    /// Получить значения поля для строк таблицы в массиве. 
    /// Повторы и пустые значения не отбрасываются. Количество и порядок элементов в результируюшем массиве соответствуют <paramref name="rows"/>.
    /// Хранящиеся в таблице значения DBNull заменяются на default
    /// </summary>
    /// <typeparam name="T">Тип поля данных (один из поддерживаемых типов DataColumn.DataType)</typeparam>
    /// <param name="rows">Массив однотипных строк</param>
    /// <param name="columnName">Имя столбца, из которого извлекаются значения</param>
    /// <returns>Массив значений поля</returns>
    public static T[] GetValuesFromColumn<T>(DataRow[] rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      T[] res = new T[rows.Length];
      for (int i = 0; i < rows.Length; i++)
      {
        object x = rows[i][columnName];
        if (!(x is DBNull))
          res[i] = (T)(Convert.ChangeType(x, typeof(T)));
      }
      return res;
    }

    #endregion

    #region GetUniqueXXXs

    #region Int

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static int[] GetUniqueInts(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyInts;
      if (dv.Count == 0)
        return EmptyInts;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      SingleScopeList<int> lst = new SingleScopeList<int>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static int[] GetUniqueInts(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyInts;
      if (table.Rows.Count == 0)
        return EmptyInts;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<int> lst = new SingleScopeList<int>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static int[] GetUniqueInts(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInts;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor xtr = new DataRowNullableIntExtractor(columnName);
      SingleScopeList<int> lst = new SingleScopeList<int>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        int? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0);
      }
      return lst.ToArray();
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static long[] GetUniqueInt64s(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyInt64s;
      if (dv.Count == 0)
        return EmptyInt64s;
      int p = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<long> lst = new SingleScopeList<long>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt64(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static long[] GetUniqueInt64s(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyInt64s;
      if (table.Rows.Count == 0)
        return EmptyInt64s;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<long> lst = new SingleScopeList<long>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt64(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static long[] GetUniqueInt64s(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInt64s;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor xtr = new DataRowNullableInt64Extractor(columnName);
      SingleScopeList<long> lst = new SingleScopeList<long>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        long? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0L);
      }
      return lst.ToArray();
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static float[] GetUniqueSingles(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptySingles;
      if (dv.Count == 0)
        return EmptySingles;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetSingle(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static float[] GetUniqueSingles(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptySingles;
      if (table.Rows.Count == 0)
        return EmptySingles;
      int p = GetColumnPosWithCheck(table, columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetSingle(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static float[] GetUniqueSingles(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptySingles;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor xtr = new DataRowNullableSingleExtractor(columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        float? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0f);
      }
      return lst.ToArray();
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static double[] GetUniqueDoubles(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyDoubles;
      if (dv.Count == 0)
        return EmptyDoubles;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDouble(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static double[] GetUniqueDoubles(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyDoubles;
      if (table.Rows.Count == 0)
        return EmptyDoubles;
      int p = GetColumnPosWithCheck(table, columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDouble(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static double[] GetUniqueDoubles(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDoubles;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor xtr = new DataRowNullableDoubleExtractor(columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        double? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0.0);
      }
      return lst.ToArray();
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static decimal[] GetUniqueDecimals(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyDecimals;
      if (dv.Count == 0)
        return EmptyDecimals;
      int p = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDecimal(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static decimal[] GetUniqueDecimals(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyDecimals;
      if (table.Rows.Count == 0)
        return EmptyDecimals;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDecimal(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static decimal[] GetUniqueDecimals(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDecimals;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor xtr = new DataRowNullableDecimalExtractor(columnName);
      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        decimal? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0m);
      }
      return lst.ToArray();
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// Значения DBNull пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static DateTime[] GetUniqueDateTimes(DataView dv, string columnName)
    {
      if (dv == null)
        return EmptyDateTimes;
      if (dv.Count == 0)
        return EmptyDateTimes;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      foreach (DataRowView drv in dv)
      {
        DateTime? v = DataTools.GetNullableDateTime(drv.Row[p]);
        if (v.HasValue)
          lst.Add(v.Value);
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// Значения DBNull пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static DateTime[] GetUniqueDateTimes(DataTable table, string columnName)
    {
      if (table == null)
        return EmptyDateTimes;
      if (table.Rows.Count == 0)
        return EmptyDateTimes;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        DateTime? v = DataTools.GetNullableDateTime(row[p]);
        if (v.HasValue)
          lst.Add(v.Value);
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// Значения DBNull пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static DateTime[] GetUniqueDateTimes(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return EmptyDateTimes;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor xtr = new DataRowNullableDateTimeExtractor(columnName);
      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        DateTime? v = xtr[row];
        if (v.HasValue)
          lst.Add(v.Value);
      }
      return lst.ToArray();
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static TimeSpan[] GetUniqueTimeSpans(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyTimeSpans;
      if (dv.Count == 0)
        return EmptyTimeSpans;
      int p = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetTimeSpan(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static TimeSpan[] GetUniqueTimeSpans(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyTimeSpans;
      if (table.Rows.Count == 0)
        return EmptyTimeSpans;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetTimeSpan(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static TimeSpan[] GetUniqueTimeSpans(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyTimeSpans;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor xtr = new DataRowNullableTimeSpanExtractor(columnName);
      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        TimeSpan? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? TimeSpan.Zero);
      }
      return lst.ToArray();
    }

    #endregion

    #region Guid

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как Guid.Empty</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static Guid[] GetUniqueGuids(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return EmptyGuids;
      if (dv.Count == 0)
        return EmptyGuids;
      int p = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetGuid(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как Guid.Empty</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static Guid[] GetUniqueGuids(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyGuids;
      if (table.Rows.Count == 0)
        return EmptyGuids;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetGuid(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как Guid.Empty</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static Guid[] GetUniqueGuids(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyGuids;
      // Строки могут относиться к разным таблицам
      DataRowNullableGuidExtractor xtr = new DataRowNullableGuidExtractor(columnName);
      SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;


        Guid? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? Guid.Empty);
      }
      return lst.ToArray();
    }

    #endregion

    #region Enum

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как значение default</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static T[] GetUniqueEnums<T>(DataView dv, string columnName, bool skipNulls)
      where T : struct
    {
      if (dv == null)
        return new T[0];
      if (dv.Count == 0)
        return new T[0];
      int p = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<T> lst = new SingleScopeList<T>();
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetEnum<T>(drv.Row[p]));
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как значение default</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static T[] GetUniqueEnums<T>(DataTable table, string columnName, bool skipNulls)
      where T : struct
    {
      if (table == null)
        return new T[0];
      if (table.Rows.Count == 0)
        return new T[0];
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<T> lst = new SingleScopeList<T>();
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetEnum<T>(row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как значение default</param>
    /// <returns>Массив уникальных значений или пустой массив</returns>
    public static T[] GetUniqueEnums<T>(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
      where T : struct
    {
      if (rows == null)
        return new T[0];
      // Строки могут относиться к разным таблицам
      DataRowNullableEnumExtractor<T> xtr = new DataRowNullableEnumExtractor<T>(columnName);
      SingleScopeList<T> lst = new SingleScopeList<T>();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;


        T? v = xtr[row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? default(T));
      }
      return lst.ToArray();
    }

    #endregion

    #endregion

    #region GetColumnNames, GetTableNames

    /// <summary>
    /// Получить список имен столбцов в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Массив имен</returns>
    public static string[] GetColumnNames(DataTable table)
    {
      string[] names = new string[table.Columns.Count];
      for (int i = 0; i < names.Length; i++)
        names[i] = table.Columns[i].ColumnName;
      return names;
    }

    /// <summary>
    /// Получить список имен таблиц в наборе DataSet
    /// </summary>
    /// <param name="ds">Набор данных</param>
    /// <returns>Массив имен таблиц</returns>
    public static string[] GetTableNames(DataSet ds)
    {
      string[] names = new string[ds.Tables.Count];
      for (int i = 0; i < names.Length; i++)
        names[i] = ds.Tables[i].TableName;
      return names;
    }

    #endregion

    #region SetBoundariesFlags

    /// <summary>
    /// Установка логических полей для обозначения начала и конца групп в таблице
    /// Группа - это последовательность строк в таблице, содержащая одинаковое 
    /// значение поля MainColumnName. Перебираются все строки таблицы по порядку.
    /// Начало и окончание каждой группы отмечается установкой в true значений полей
    /// BeginFlagColumnName и EndFlagColumnName соответственно
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="mainColumnName">Имя поля, которое содержит одинаковые значения
    /// для выделения групп. Может быть любого типа</param>
    /// <param name="beginFlagColumnName">Имя логического поля, в котором устанавливается значение
    /// true для первой строки в группе. Может быть не задано, если установка флага начала группы не требуется</param>
    /// <param name="endFlagColumnName">Имя логического поля, в котором устанавливается значение
    /// true для последней строки в группе. Может быть не задано, если установка флага конца группы не требуется</param>
    public static void SetBoundariesFlags(DataTable table, string mainColumnName, string beginFlagColumnName, string endFlagColumnName)
    {
      // Проверка аргуементов и получение позиций столбцов для ускорения доступа
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("Table");
      if (String.IsNullOrEmpty(mainColumnName))
        throw new ArgumentNullException("MainColumnName");
#endif

      int mainColumnPos = GetColumnPosWithCheck(table, mainColumnName);

      int beginFlagColumnPos, endFlagColumnPos;

      if (String.IsNullOrEmpty(beginFlagColumnName))
        beginFlagColumnPos = -1;
      else
      {
        beginFlagColumnPos = GetColumnPosWithCheck(table, beginFlagColumnName);
#if DEBUG
        if (table.Columns[beginFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + beginFlagColumnName + "\" таблицы \"" + table.TableName + "\" имеет тип \"" + table.Columns[beginFlagColumnPos].DataType.ToString() + "\", а не логический", "beginFlagColumnName");
#endif
      }

      if (String.IsNullOrEmpty(endFlagColumnName))
        endFlagColumnPos = -1;
      else
      {
        endFlagColumnPos = GetColumnPosWithCheck(table, endFlagColumnName);
#if DEBUG
        if (table.Columns[endFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + endFlagColumnName + "\" таблицы \"" + table.TableName + "\" имеет тип \"" + table.Columns[endFlagColumnPos].DataType.ToString() + "\", а не логический", "endFlagColumnName");
#endif
      }

      if (table.Rows.Count == 0)
        return;

      // Перебор строк
      for (int i = 1; i < table.Rows.Count; i++)
      {
        DataRow prevRow = table.Rows[i - 1];
        DataRow thisRow = table.Rows[i];
        bool flag = !AreValuesEqual(prevRow, thisRow, mainColumnPos);
        if (beginFlagColumnPos >= 0)
          thisRow[beginFlagColumnPos] = flag;
        if (endFlagColumnPos >= 0)
          prevRow[endFlagColumnPos] = flag;
      }
      // Для первой и последней строки флаги устанавливаются принудительно
      if (beginFlagColumnPos >= 0)
        table.Rows[0][beginFlagColumnPos] = true;
      if (endFlagColumnPos >= 0)
        table.Rows[table.Rows.Count - 1][endFlagColumnPos] = true;
    }


    // !!!
    // Не проверял. М.Б. нужны DataRowView.Begin/EndEdit()

    /// <summary>
    /// Установка логических полей для обозначения начала и конца групп в таблице
    /// Группа - это последовательность строк в таблице, содержащая одинаковое 
    /// значение поля MainColumnName. Перебираются все строки таблицы по порядку.
    /// Начало и окончание каждой группы отмечается установкой в true значений полей
    /// BeginFlagColumnName и EndFlagColumnName соответственно
    /// Перебор строк осуществляется для заданного просмотра DataView
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <param name="mainColumnName">Имя поля, которое содержит одинаковые значения
    /// для выделения групп. Может быть любого типа</param>
    /// <param name="beginFlagColumnName">Имя логического поля, в котором устанавливается значение
    /// true для первой строки в группе. Может быть не задано, если установка флага начала группы не требуется</param>
    /// <param name="endFlagColumnName">Имя логического поля, в котором устанавливается значение
    /// true для последней строки в группе. Может быть не задано, если установка флага конца группы не требуется</param>
    public static void SetBoundariesFlags(DataView dv, string mainColumnName, string beginFlagColumnName, string endFlagColumnName)
    {
      // Проверка аргуементов и получение позиций столбцов для ускорения доступа
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      int mainColumnPos = GetColumnPosWithCheck(dv.Table, mainColumnName);

      int beginFlagColumnPos, endFlagColumnPos;

      if (String.IsNullOrEmpty(beginFlagColumnName))
        beginFlagColumnPos = -1;
      else
      {
        beginFlagColumnPos = GetColumnPosWithCheck(dv.Table, beginFlagColumnName);
#if DEBUG
        if (dv.Table.Columns[beginFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + beginFlagColumnName + "\" таблицы \"" + dv.Table.TableName + "\" имеет тип \"" + dv.Table.Columns[beginFlagColumnPos].DataType.ToString() + "\", а не логический", "beginFlagColumnName");
#endif
      }

      if (String.IsNullOrEmpty(endFlagColumnName))
        endFlagColumnPos = -1;
      else
      {
        endFlagColumnPos = GetColumnPosWithCheck(dv.Table, endFlagColumnName);
#if DEBUG
        if (dv.Table.Columns[endFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + endFlagColumnName + "\" таблицы \"" + dv.Table.TableName + "\" имеет тип \"" + dv.Table.Columns[endFlagColumnPos].DataType.ToString() + "\", а не логический", "endFlagColumnName");
#endif
      }

      if (dv.Count == 0)
        return;

      // Перебор строк
      for (int i = 1; i < dv.Count; i++)
      {
        DataRowView prevRow = dv[i - 1];
        DataRowView thisRow = dv[i];
        bool flag = !AreValuesEqual(prevRow, thisRow, mainColumnPos);
        if (beginFlagColumnPos >= 0)
          thisRow[beginFlagColumnPos] = flag;
        if (endFlagColumnPos >= 0)
          prevRow[endFlagColumnPos] = flag;
      }
      // Для первой и последней строки флаги устанавливаются принудительно
      if (beginFlagColumnPos >= 0)
        dv[0][beginFlagColumnPos] = true;
      if (endFlagColumnPos >= 0)
        dv[dv.Count - 1][endFlagColumnPos] = true;
    }

    #endregion

    #region AreValuesEqual

    /// <summary>
    /// Сравнение значений одного поля для двух строк.
    /// Возвращает значение true, если значения одинаковы. Если есть пустые
    /// значения DBNull, то строки считаются одинаковыми, если обе строки содержат
    /// DBNull
    /// </summary>
    /// <param name="row1">Первая сравниваемая строка</param>
    /// <param name="row2">Вторая сравниваемая строка</param>
    /// <param name="columnPos">Позиция столбца</param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool AreValuesEqual(DataRow row1, DataRow row2, int columnPos)
    {
      if (row1.IsNull(columnPos))
        return row2.IsNull(columnPos);
      if (row2.IsNull(columnPos))
        return false;
      return row1[columnPos].Equals(row2[columnPos]);
    }

    /// <summary>
    /// Сравнение значений одного поля для двух строк.
    /// Возвращает значение true, если значения одинаковы. Если есть пустые
    /// значения DBNull, то строки считаются одинаковыми, если обе строки содержат
    /// DBNull
    /// Предполагается, что строки имеют одинаковую струтуру
    /// </summary>
    /// <param name="row1">Первая сравниваемая строка</param>
    /// <param name="row2">Вторая сравниваемая строка</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool AreValuesEqual(DataRow row1, DataRow row2, string columnName)
    {
      return AreValuesEqual(row1, row2, row1.Table.Columns.IndexOf(columnName));
    }

    /// <summary>
    /// Сравнение значений одного поля для двух строк.
    /// Возвращает значение true, если значения одинаковы. Если есть пустые
    /// значения DBNull, то строки считаются одинаковыми, если обе строки содержат
    /// DBNull
    /// </summary>
    /// <param name="row1">Первая сравниваемая строка</param>
    /// <param name="row2">Вторая сравниваемая строка</param>
    /// <param name="columnPos">Позиция столбца</param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool AreValuesEqual(DataRowView row1, DataRowView row2, int columnPos)
    {
      object v1 = row1[columnPos];
      object v2 = row2[columnPos];
      if (v1 is DBNull)
        return v2 is DBNull;
      if (v2 is DBNull)
        return false;
      return v1.Equals(v2);
    }

    /// <summary>
    /// Сравнение значений одного поля для двух строк.
    /// Возвращает значение true, если значения одинаковы. Если есть пустые
    /// значения DBNull, то строки считаются одинаковыми, если обе строки содержат
    /// DBNull
    /// </summary>
    /// <param name="row1">Первая сравниваемая строка</param>
    /// <param name="row2">Вторая сравниваемая строка</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool AreValuesEqual(DataRowView row1, DataRowView row2, string columnName)
    {
      object v1 = row1[columnName];
      object v2 = row2[columnName];
      if (v1 is DBNull)
        return v2 is DBNull;
      if (v2 is DBNull)
        return false;
      return v1.Equals(v2);
    }

    /// <summary>
    /// Расширенное сравнение двух значений на равенство. Значения null и DBNull
    /// считаются одинаковыми. Для сравнения используется метод Object.Equals().
    /// Если одно значение содержит null или DBNull, а второе - нет, то возвращается
    /// false.
    /// </summary>
    /// <param name="value1">Первое сравниваемое значение</param>
    /// <param name="value2">Второе сравниваемое значение</param>
    /// <returns>true, если значения совпадают</returns>
    public static bool AreValuesEqual(object value1, object value2)
    {
      if (value1 == null)
        value1 = DBNull.Value;
      if (value2 == null)
        value2 = DBNull.Value;

      if (value1 is DBNull)
        return value2 is DBNull;
      if (value2 is DBNull)
        return false;
      if (value1.GetType() != value2.GetType())
      {
        // 28.08.2009
        // обычное сравнение не работает, например, для типов byte и short
        // !! Надо бы как-нибудь, используя Convert.GetTypeCode()
        int level1 = GetEqLevel(value1);
        int level2 = GetEqLevel(value2);
        if (level1 > 0 && level2 > 0)
        {
          int level = Math.Max(level1, level2);
          value1 = ToEqLevel(value1, level);
          value2 = ToEqLevel(value2, level);
        }
      }
      // 13.05.2015, 24.03.2016
      else if (value1 is String)
      {
        string s1 = ((string)value1).TrimEnd();
        string s2 = ((string)value2).TrimEnd();
        return String.Equals(s1, s2);
      }

      return value1.Equals(value2);
    }

    /// <summary>
    /// Возвращает "уровень" для числовых типов:
    /// 1-int
    /// 2-long
    /// 3-decimal
    /// Для других типов возвращает 0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static int GetEqLevel(object value)
    {
      if (value is Byte || value is SByte ||
        value is Int16 || value is UInt16 ||
        value is Int32 || value is UInt32)
        return 1;

      if (value is Int64 || value is UInt64)
        return 2;

      if (value is Single || value is Double || value is decimal)
        return 3;

      return 0;
    }

    private static object ToEqLevel(object value, int level)
    {
      switch (level)
      {
        case 1:
          return Convert.ToInt32(value);
        case 2:
          return Convert.ToInt64(value);
        case 3:
          return Convert.ToDecimal(value);
        default:
          throw new ArgumentException("Неизвестный уровень", "Level");
      }
    }

    #endregion

    #region GetRandomId

    /// <summary>
    /// Используется для глобальных блокировок lock в пределах библотеки ExtTools,
    /// когда нет своих объектов для синхронизации
    /// </summary>
    internal static object InternalSyncRoot { get { return _TheRandom; } }

    /// <summary>
    /// Генератор случайных чисел
    /// Это свойство не является потокобезопасным.
    /// При обращении должна выполняться блокировка объекта
    /// </summary>
    internal static Random TheRandom { get { return _TheRandom; } }
    private static readonly Random _TheRandom = new Random();


    /// <summary>
    /// Возвращает значение ключевого поля, которого нет в таблице данных.
    /// Таблица должна иметь первичный ключ по числовому полю
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Идентификатор для новой строки</returns>
    public static Int32 GetRandomId(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.PrimaryKey.Length != 1)
        throw new ArgumentException("Таблица должна иметь первичный ключ по одному полю");
      if (table.PrimaryKey[0].DataType != typeof(Int32))
        throw new ArgumentException("Таблица должна иметь первичный ключ по числовому полю");

      lock (_TheRandom)
      {
        while (true)
        {
          Int32 KeyValue = -_TheRandom.Next(); // отрицательные значения для наглядности
          if (KeyValue == 0)
            continue;
          if (table.Rows.Find(KeyValue) == null)
            return KeyValue;
        }
      }
    }

    #endregion

    #region GroupRows

    #region DataTable


    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается таблица <paramref name="srcTable"/> с произвольным набором строк.
    /// Создаются массивы <paramref name="rows"/>  ссылок на строки этой таблицы, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/>  помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames"/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// Значения DBNull и нулевые значения/пустые строки различаются
    /// </summary>
    /// <param name="srcTable">Исходная таблица, из которой берутся строки.
    /// Таблица должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки таблицы
    /// SrcTable. Первая размерность совпадает со строками в <paramref name="keyTable"/> и соответствует
    /// уникальным комбинациям значений полей</param>
    public static void GroupRows(DataTable srcTable, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows)
    {
      GroupRows(srcTable, keyColumnNames, out keyTable, out rows, false);
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается таблица <paramref name="srcTable"/> с произвольным набором строк.
    /// Создаются массивы <paramref name="rows"/>  ссылок на строки этой таблицы, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/>  помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames"/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// </summary>
    /// <param name="srcTable">Исходная таблица, из которой берутся строки.
    /// Таблица должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки таблицы
    /// SrcTable. Первая размерность совпадает со строками в KeyTable и соответствует
    /// уникальным комбинациям значений полей</param>
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    public static void GroupRows(DataTable srcTable, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows, bool dbNullAsZero)
    {
#if DEBUG
      if (srcTable == null)
        throw new ArgumentNullException("SrcTable");
      if (String.IsNullOrEmpty(keyColumnNames))
        throw new ArgumentNullException("KeyColumnNames");
#endif

      string[] aKeyColumnNames = keyColumnNames.Split(',');
      int[] keyColumnPoss = new int[aKeyColumnNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyColumnNames.Length; i++)
      {
        keyColumnPoss[i] = GetColumnPosWithCheck(srcTable, aKeyColumnNames[i]);
        DataColumn SrcCol = srcTable.Columns[keyColumnPoss[i]];
        keyTable.Columns.Add(SrcCol.ColumnName, SrcCol.DataType); // другие ограничения не применяем
      }

      // 26.01.2016
      // Среди значений полей, по которым выполняется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyColumnNames);
      object[] keyValues = new object[aKeyColumnNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      foreach (DataRow srcRow in srcTable.Rows)
      {
        InitGroupRowsKeyValues(srcRow, keyValues, keyColumnPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        rows2.Add(new List<DataRow>());

      foreach (DataRow srcRow in srcTable.Rows)
      {
        InitGroupRowsKeyValues(srcRow, keyValues, keyColumnPoss, dbNullAsZero);
        DataRow keyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues); // 26.01.2017

        int keyRowIndex = keyTable.Rows.IndexOf(keyRow);
        List<DataRow> currList = rows2[keyRowIndex];
        currList.Add(srcRow);
      }

      // Преобразование списков в массив
      rows = new DataRow[rows2.Count][];
      for (int i = 0; i < rows2.Count; i++)
        rows[i] = rows2[i].ToArray();
    }

    private static void InitGroupRowsKeyValues(DataRow srcRow, object[] keyValues, int[] keyColumnPoss, bool dbNullAsZero)
    {
      for (int i = 0; i < keyColumnPoss.Length; i++)
      {
        keyValues[i] = srcRow[keyColumnPoss[i]];
        if (dbNullAsZero && (keyValues[i] is DBNull)) // 26.01.2017
          keyValues[i] = GetEmptyValue(srcRow.Table.Columns[keyColumnPoss[i]].DataType);
      }
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается таблица <paramref name="srcTable "/> с произвольным набором строк.
    /// Возвращаются массивы ссылок на строки этой таблицы, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Значения DBNull и нулевые значения/пустые строки различаются
    /// </summary>
    /// <param name="srcTable">Исходная таблица, из которой берутся строки.
    /// Таблица должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <returns>Сюда записывается двумерный массив ссылок на строки таблицы
    /// SrcTable. Первая размерность соответствует
    /// уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataTable srcTable, string keyColumnNames)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcTable, keyColumnNames, out keyTable, out rows, false);
      return rows;
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается таблица <paramref name="srcTable"/> с произвольным набором строк.
    /// Возвращаются массивы ссылок на строки этой таблицы, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// </summary>
    /// <param name="srcTable">Исходная таблица, из которой берутся строки.
    /// Таблица должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    /// <returns>Сюда записывается двумерный массив ссылок на строки таблицы
    /// SrcTable. Первая размерность соответствует
    /// уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataTable srcTable, string keyColumnNames, bool dbNullAsZero)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcTable, keyColumnNames, out keyTable, out rows, dbNullAsZero);
      return rows;
    }

    #endregion

    #region DataView


    /// <summary>
    /// Распределение строк таблицы, выбранных в DataView  по группам с одинаковыми значениями ключевых
    /// полей. На входе задается список <paramref name="srcDataView"/> с произвольным набором строк.
    /// Создаются массивы <paramref name="rows"/> ссылок на строки таблицы, базовой для <paramref name="srcDataView"/> , каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/> помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames "/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// Значения DBNull и нулевые значения/пустые строки различаются
    /// </summary>
    /// <param name="srcDataView">Исходная просмотр таблицы , из которого берутся строки.
    /// Таблица, на основе которой построен <paramref name="srcDataView"/>, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки таблицы
    /// <paramref name="srcDataView"/>.Table. Первая размерность совпадает со строками в <paramref name="keyTable"/> и соответствует
    /// уникальным комбинациям значений полей</param>
    public static void GroupRows(DataView srcDataView, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows)
    {
      GroupRows(srcDataView, keyColumnNames, out keyTable, out rows, false);
    }

    /// <summary>
    /// Распределение строк таблицы, выбранных в DataView  по группам с одинаковыми значениями ключевых
    /// полей. На входе задается список <paramref name="srcDataView"/> с произвольным набором строк.
    /// Создаются массивы <paramref name="rows"/> ссылок на строки таблицы, базовой для <paramref name="srcDataView"/> , каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/> помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames "/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// </summary>
    /// <param name="srcDataView">Исходная DataView, из которого берутся строки.
    /// Таблица, на основе которой построен <paramref name="srcDataView"/>, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки таблицы
    /// <paramref name="srcDataView"/>.Table. Первая размерность совпадает со строками в <paramref name="keyTable"/> и соответствует
    /// уникальным комбинациям значений полей</param>
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    public static void GroupRows(DataView srcDataView, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows, bool dbNullAsZero)
    {
#if DEBUG
      if (srcDataView == null)
        throw new ArgumentNullException("srcDataView");
      if (String.IsNullOrEmpty(keyColumnNames))
        throw new ArgumentNullException("keyColumnNames");
#endif
      string[] aKeyColumnNames = keyColumnNames.Split(',');
      int[] keyColumnPoss = new int[aKeyColumnNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyColumnNames.Length; i++)
      {
        keyColumnPoss[i] = GetColumnPosWithCheck(srcDataView.Table, aKeyColumnNames[i]);
        DataColumn srcCol = srcDataView.Table.Columns[keyColumnPoss[i]];
        keyTable.Columns.Add(srcCol.ColumnName, srcCol.DataType); // другие ограничения не применяем
      }
      // 26.01.2016
      // Среди значений полей, по которым выполняется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyColumnNames);
      object[] keyValues = new object[aKeyColumnNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      foreach (DataRowView srcDRV in srcDataView)
      {
        InitGroupRowsKeyValues(srcDRV.Row, keyValues, keyColumnPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        rows2.Add(new List<DataRow>());

      foreach (DataRowView srcDRV in srcDataView)
      {
        InitGroupRowsKeyValues(srcDRV.Row, keyValues, keyColumnPoss, dbNullAsZero);
        DataRow keyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues); // 26.01.2017

        int keyRowIndex = keyTable.Rows.IndexOf(keyRow);
        List<DataRow> currList = rows2[keyRowIndex];
        currList.Add(srcDRV.Row);
      }

      // Преобразование списков в массив
      rows = new DataRow[rows2.Count][];
      for (int i = 0; i < rows2.Count; i++)
        rows[i] = rows2[i].ToArray();
    }

    /// <summary>
    /// Распределение строк таблицы, выбранных в DataView  по группам с одинаковыми значениями ключевых
    /// полей. На входе задается список <paramref name="srcDataView"/> с произвольным набором строк.
    /// Создаются массивы Rows ссылок на строки таблицы, базовой для SrcDataView, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Значения DBNull и нулевые значения/пустые строки различаются.
    /// </summary>
    /// <param name="srcDataView">Исходная DataView, из которого берутся строки.
    /// Таблица, на основе которой построен <paramref name="srcDataView"/>, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <returns>Возвращается двумерный массив ссылок на строки таблицы
    /// SrcDataView.Table. Первая размерность совпадает со строками в KeyTable и соответствует
    /// уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataView srcDataView, string keyColumnNames)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcDataView, keyColumnNames, out keyTable, out rows, false);
      return rows;
    }

    /// <summary>
    /// Распределение строк таблицы, выбранных в DataView по группам с одинаковыми значениями ключевых
    /// полей. На входе задается список <paramref name="srcDataView"/> с произвольным набором строк.
    /// Создаются массивы Rows ссылок на строки таблицы, базовой для <paramref name="srcDataView"/>, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>/
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// </summary>
    /// <param name="srcDataView">Исходная DataView, из которого берутся строки.
    /// Таблица, на основе которой построен <paramref name="srcDataView"/>, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    /// <returns>Возвращается двумерный массив ссылок на строки таблицы
    /// SrcDataView.Table. Первая размерность соответствует уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataView srcDataView, string keyColumnNames, bool dbNullAsZero)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcDataView, keyColumnNames, out keyTable, out rows, dbNullAsZero);
      return rows;
    }

    #endregion

    #region DataRow[]

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается произвольный массив строк. Строки должны относится
    /// к одной таблице или однотипным таблицам
    /// Создаются массивы <paramref name="rows"/> ссылок на строки исходного массива, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/> помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames"/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// В этой версии, если исходный массив строк пустой, то <paramref name="keyTable"/> не будет содержать ни одного поля
    /// Значения DBNull и нулевые значения/пустые строки различаются
    /// </summary>
    /// <param name="srcRows">Исходные строки.
    /// Таблица, к которой относятся строки, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки 
    /// <paramref name="srcRows"/>. Первая размерность совпадает со строками в <paramref name="keyTable"/> и соответствует
    /// уникальным комбинациям значений полей</param>
    public static void GroupRows(DataRow[] srcRows, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows)
    {
      GroupRows(srcRows, keyColumnNames, out keyTable, out rows, false);
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается произвольный массив строк. Строки должны относится
    /// к одной таблице или однотипным таблицам
    /// Создаются массивы <paramref name="rows"/> ссылок на строки исходного массива, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// Во вспомогательную таблицу <paramref name="keyTable"/> помещаются строки с уникальными 
    /// комбинациями значений полей <paramref name="keyColumnNames"/> (других полей в таблице нет).
    /// Число строк в <paramref name="keyTable"/> совпадает с числом массивов <paramref name="rows"/>.
    /// В этой версии, если исходный массив строк пустой, то <paramref name="keyTable"/> не будет содержать ни одного поля
    /// Эта версия позволяет выполнить замену значений DBNull на 0 или пустую строку, в зависимости от типа данных
    /// </summary>
    /// <param name="srcRows">Исходные строки.
    /// Таблица, к которой относятся строки, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="keyTable">Сюда записывается вспомогательная таблица значений полей</param>
    /// <param name="rows">Сюда записывается двумерный массив ссылок на строки таблицы
    /// SrcTable. Первая размерность совпадает со строками в <paramref name="keyTable"/> и соответствует
    /// уникальным комбинациям значений полей</param>
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    public static void GroupRows(DataRow[] srcRows, string keyColumnNames, out DataTable keyTable, out DataRow[][] rows, bool dbNullAsZero)
    {
#if DEBUG
      if (srcRows == null)
        throw new ArgumentNullException("srcTable");
      if (String.IsNullOrEmpty(keyColumnNames))
        throw new ArgumentNullException("keyColumnNames");
#endif
      //if (srcRows == null)
      if (srcRows.Length == 0) // 27.12.2020
      {
        keyTable = new DataTable(); // пустышка
        rows = new DataRow[0][];
        return;
      }


      string[] aKeyColumnNames = keyColumnNames.Split(',');
      int[] keyColumnPoss = new int[aKeyColumnNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyColumnNames.Length; i++)
      {
        keyColumnPoss[i] = GetColumnPosWithCheck(srcRows[0].Table, aKeyColumnNames[i]);
        DataColumn SrcCol = srcRows[0].Table.Columns[keyColumnPoss[i]];
        keyTable.Columns.Add(SrcCol.ColumnName, SrcCol.DataType); // другие ограничения не применяем
      }

      // 26.01.2016
      // Среди значений полей, по которым выполняется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyColumnNames);
      object[] keyValues = new object[aKeyColumnNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      for (int j = 0; j < srcRows.Length; j++)
      {
        InitGroupRowsKeyValues(srcRows[j], keyValues, keyColumnPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        rows2.Add(new List<DataRow>());

      for (int j = 0; j < srcRows.Length; j++)
      {
        InitGroupRowsKeyValues(srcRows[j], keyValues, keyColumnPoss, dbNullAsZero);
        DataRow keyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, keyValues); // 26.01.2017

        int keyRowIndex = keyTable.Rows.IndexOf(keyRow);
        List<DataRow> currList = rows2[keyRowIndex];
        currList.Add(srcRows[j]);
      }

      // Преобразование списков в массив
      rows = new DataRow[rows2.Count][];
      for (int i = 0; i < rows2.Count; i++)
        rows[i] = rows2[i].ToArray();
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается произвольный массив строк. Строки должны относится
    /// к одной таблице или однотипным таблицам
    /// Возвращаются массивы ссылок на строки исходного массива, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// </summary>
    /// <param name="srcRows">Исходные строки.
    /// Таблица, к которой относятся строки, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <returns>Возвращается двумерный массив ссылок на строки масива
    /// SrcRows. Первая размерность соответствует уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataRow[] srcRows, string keyColumnNames)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcRows, keyColumnNames, out keyTable, out rows);
      return rows;
    }

    /// <summary>
    /// Распределение строк таблицы по группам с одинаковыми значениями ключевых
    /// полей. На входе задается произвольный массив строк. Строки должны относится
    /// к одной таблице или однотипным таблицам
    /// Возвращаются массивы ссылок на строки исходного массива, каждый массив содержит
    /// строки с одинаковыми значеними полей в списке <paramref name="keyColumnNames"/>.
    /// </summary>
    /// <param name="srcRows">Исходные строки.
    /// Таблица, к которой относятся строки, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
    /// <param name="keyColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="dbNullAsZero">Если true, то значения DBNull полей <paramref name="keyColumnNames"/>
    /// в исходной таблице трактоваться как 0 (или пустая строка)</param>
    /// <returns>Возвращается двумерный массив ссылок на строки масива
    /// SrcRows. Первая размерность соответствует уникальным комбинациям значений полей</returns>
    public static DataRow[][] GroupRows(DataRow[] srcRows, string keyColumnNames, bool dbNullAsZero)
    {
      DataTable keyTable;
      DataRow[][] rows;
      GroupRows(srcRows, keyColumnNames, out keyTable, out rows, dbNullAsZero);
      return rows;
    }

    #endregion

    #endregion

    #region SetRowState

    /// <summary>
    /// Установка требуемого состояния строки DataRow.
    /// Свойство DataRow.RowState доступно только для чтения
    /// Вызывает методы DataRow.SetAdded(), SetModified(), AcceptChanges() или Delete()
    /// </summary>
    /// <param name="row">Строка, свойство RowState требуется установить</param>
    /// <param name="newState">Требуемое значение свойства</param>
    public static void SetRowState(DataRow row, DataRowState newState)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
#endif

      if (row.RowState == newState)
        return; // Не требуется вносить изменения

      if (row.RowState == DataRowState.Detached)
      {
#if DEBUG
        if (row.Table == null)
          throw new NullReferenceException("Не установлено свойство DataRow.Table");
#endif

        row.Table.Rows.Add(row);
        row.AcceptChanges(); // Нужно ли?
        if (newState == DataRowState.Unchanged)
          return;
      }

      switch (newState)
      {
        case DataRowState.Unchanged:
          switch (row.RowState)
          {
            case DataRowState.Added:
            case DataRowState.Modified:
              row.AcceptChanges();
              break;
            case DataRowState.Deleted:
              row.RejectChanges();
              break;
            default:
              throw new InvalidOperationException();
          }
          break;

        case DataRowState.Modified:
          switch (row.RowState)
          {
            case DataRowState.Added:
              row.AcceptChanges();
              row.SetModified();
              break;
            case DataRowState.Unchanged:
              row.SetModified();
              break;
            case DataRowState.Deleted:
              row.RejectChanges();
              row.SetModified();
              break;
            default:
              throw new InvalidOperationException();
          }
          break;

        case DataRowState.Added:
          switch (row.RowState)
          {
            case DataRowState.Modified:
              row.AcceptChanges();
              row.SetAdded();
              break;
            case DataRowState.Unchanged:
              row.SetAdded();
              break;
            case DataRowState.Deleted:
              row.RejectChanges();
              row.SetAdded();
              break;
            default:
              throw new InvalidOperationException();
          }
          break;

        case DataRowState.Deleted:
          switch (row.RowState)
          {
            case DataRowState.Unchanged:
              row.Delete();
              break;
            case DataRowState.Added:
            case DataRowState.Modified:
              row.AcceptChanges();
              row.Delete();
              break;
            default:
              throw new InvalidOperationException();
          }
          break;

        case DataRowState.Detached:
          row.Table.Rows.Remove(row);
          break;

        default:
          throw new ArgumentException("Неизвестное значение аргумента " + newState.ToString(), "newState");
      }
    }

    #endregion

    #region CloneDataColumn

    /// <summary>
    /// Создание копии столбца DataColumn.
    /// Функция полезна когда требуется выполнить DataTable.Clone(), но не для всех столбцов таблицы
    /// Возвращаемый объект DataColumn не присоединен ни к какой коллекции
    /// </summary>
    /// <param name="srcColumn">Исходный столбец</param>
    /// <returns>Копия столбца</returns>
    public static DataColumn CloneDataColumn(DataColumn srcColumn)
    {
#if DEBUG
      if (srcColumn == null)
        throw new ArgumentNullException("srcColumn");
#endif

      DataColumn res = new DataColumn();

      res.ColumnName = srcColumn.ColumnName;
      res.DataType = srcColumn.DataType;
      res.Caption = srcColumn.Caption;
      res.Expression = srcColumn.Expression;
      res.DefaultValue = srcColumn.DefaultValue;
      res.AllowDBNull = srcColumn.AllowDBNull;
      res.AutoIncrement = srcColumn.AutoIncrement;
      res.AutoIncrementSeed = srcColumn.AutoIncrementSeed;
      res.AutoIncrementStep = srcColumn.AutoIncrementStep;
      res.ColumnMapping = srcColumn.ColumnMapping;
      res.DateTimeMode = srcColumn.DateTimeMode;
      res.MaxLength = srcColumn.MaxLength;
      res.Namespace = srcColumn.Namespace;
      res.ReadOnly = srcColumn.ReadOnly;
      res.Unique = srcColumn.Unique;

      CopyProperties(srcColumn.ExtendedProperties, res.ExtendedProperties);

      return res;
    }

    #endregion

    #region GetRowValues

    /// <summary>
    /// Возвращает массив значений полей строки (аналогичный свойству DataRow.ItemArray),
    /// но для заданной версии значений строки.
    /// Позволяет, в частности, получить значения для удаленной строки
    /// </summary>
    /// <param name="row">Строка, откуда извлекаются данные</param>
    /// <param name="rowVersion">Требуемая версия значений</param>
    /// <returns>Массив значений</returns>
    public static object[] GetRowValues(DataRow row, DataRowVersion rowVersion)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
#endif
      object[] a = new object[row.Table.Columns.Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = row[i, rowVersion];
      return a;
    }

    /// <summary>
    /// Возвращает значения полей строки в виде коллекции Имя-Значение для заданной версии значений строки.
    /// В основном, предназначено для отладочных целей
    /// </summary>
    /// <param name="row">Строка, откуда извлекаются данные</param>
    /// <param name="rowVersion">Требуемая версия значений</param>
    /// <returns>Коллекция, где ключ - имя поля, значение - значение поля</returns>
    public static IDictionary<string, object> GetRowValueDictionary(DataRow row, DataRowVersion rowVersion)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
#endif

      Dictionary<string, object> dict = new Dictionary<string, object>(row.Table.Columns.Count);
      for (int i = 0; i < row.Table.Columns.Count; i++)
        dict.Add(row.Table.Columns[i].ColumnName, row[i, rowVersion]);
      return dict;
    }

    /// <summary>
    /// Возвращает значения полей строки в виде коллекции Имя-Значение.
    /// В основном, предназначено для отладочных целей.
    /// </summary>
    /// <param name="row">Строка, откуда извлекаются данные</param>
    /// <returns>Коллекция, где ключ - имя поля, значение - значение поля</returns>
    public static IDictionary<string, object> GetRowValueDictionary(DataRow row)
    {
      return GetRowValueDictionary(row, DataRowVersion.Default);
    }

    #endregion

    #region TrimEnd

    /// <summary>
    /// Во всех строках таблицы для всех строковых полей (DataColumn.DataType==typeof(string)) выполняется удаление
    /// конечных пробелов с помощью String.TrimEnd().
    /// В таблице не должно быть удаленных строк, иначе возникнет исключение при чтении значений.
    /// После вызова метода рекомендуется вызвать DataTable.AcceptChanges()
    /// </summary>
    /// <param name="table">Обрабатываемая таблица данных</param>
    public static void TrimEnd(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      switch (table.Rows.Count)
      {
        case 0:
          break;

        case 1:
          DataRow row = table.Rows[0];
          for (int i = 0; i < table.Columns.Count; i++)
          {
            if (table.Columns[i].DataType == typeof(string))
            {
              string s = row[i] as string;
              if (s != null)
              {
                s = s.TrimEnd();
                row[i] = s;
              }
            }
          }
          break;

        default:
          // Если несколько строк, собираем список полей
          List<int> colPoss = null;
          for (int i = 0; i < table.Columns.Count; i++)
          {
            if (table.Columns[i].DataType == typeof(string))
            {
              if (colPoss == null)
                colPoss = new List<int>();
              colPoss.Add(i);
            }
          }
          if (colPoss != null) // если нет строковых полей, незачем перебирать строки
          {
            foreach (DataRow row2 in table.Rows)
            {
              for (int j = 0; j < colPoss.Count; j++)
              {
                string s = row2[colPoss[j]] as string;
                if (s != null)
                {
                  s = s.TrimEnd();
                  row2[colPoss[j]] = s;
                }
              }
            }
          }
          break;
      }
    }

    /// <summary>
    /// Во всех строках таблиц набора, для всех строковых полей (DataColumn.DataType==typeof(string)) выполняется удаление
    /// конечных пробелов с помощью String.TrimEnd().
    /// В таблицах не должно быть удаленных строк, иначе возникнет исключение при чтении значений.
    /// После вызова метода рекомендуется вызвать DataSet.AcceptChanges()
    /// </summary>
    /// <param name="ds">Обрабатываемые таблицы данных</param>
    public static void TrimEnd(DataSet ds)
    {
#if DEBUG
      if (ds == null)
        throw new ArgumentNullException("ds");
#endif

      for (int i = 0; i < ds.Tables.Count; i++)
        TrimEnd(ds.Tables[i]);
    }

    #endregion

    #region SortDataRows

    /// <summary>
    /// Сортировка массива строк в соответствии с заданным выражением.
    /// Строки могут относится к разным таблицам, но таблицы должны иметь одинаковую структуру.
    /// Создает временный объект DataTable/DataView для выполнения сортировки.
    /// </summary>
    /// <param name="rows">Массив строк, которые требуется сортировать. Если в массиве меньше 2 строк, то никаких действий не выполняется</param>
    /// <param name="sort">Выражение для сортировки. Если пустая строка или null, то никаких действий не выполняется</param>
    public static void SortDataRows(DataRow[] rows, string sort)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif
      if (rows.Length < 2)
        return;
      if (String.IsNullOrEmpty(sort))
        return;

      DataTable table = rows[0].Table.Clone();
      table.Columns.Add("_$ORDER$_2c0dfea6832644e4ba55a994e0bedd10", typeof(int)); // чтобы точно такого имени небыло
      int ordColPos = table.Columns.Count - 1;
      table.BeginLoadData();
      for (int i = 0; i < rows.Length; i++)
      {
        DataRow row2 = table.Rows.Add(rows[i].ItemArray);
        row2[ordColPos] = i;
      }
      table.EndLoadData();
      table.DefaultView.Sort = sort;

      DataRow[] rows2 = new DataRow[rows.Length];
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        int order = (int)(table.DefaultView[i].Row[ordColPos]);
        rows2[i] = rows[order];
      }

      // Лучше замену массива выполнить в самом конце, тут меньше вероятность исключения
      rows2.CopyTo(rows, 0);
    }

    #endregion

    #region FormatDataValue()

    /// <summary>
    /// Форматирование константного значения для DataColumn.Expression, метода DataTable.Select().
    /// Строки заключаются в апострофы и т.п. См. справку по свойству DataColumn.Expression.
    /// Идентичные действия выполняются методом BaseDBxSqlFormatter.OnFormatValue() в ExtDB.dll.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public static string FormatDataValue(object value)
    {
      if (value == null || value is DBNull)
        return "NULL";
      if (value is String)
        return FormatDataString((string)value);
      if (value is Guid)
        return FormatDataString(((Guid)value).ToString("D"));
      if (value is Boolean)
        return (bool)value ? "TRUE" : "FALSE";
      if (value is DateTime)
        return FormatDataDateTime((DateTime)value);
      if (value is TimeSpan)
      {
        // Изучил стек вызовов при ошибке
        // Преобразование работает правильно.
        // К сожалению (?), нельзя использовать TimeSpan в выражении Select, так как операции сравнения (в частности, "=") не поддерживаются вычислителем выражений

        string s = System.Xml.XmlConvert.ToString((TimeSpan)value); // весьма странное представление
        // Так нельзя: return s;
        return "CONVERT(" + FormatDataString(s) + ",\'System.TimeSpan\')";
      }
      if (value is char)
        return FormatDataString(new string((char)value, 1));

      IFormattable fv = value as IFormattable;
      if (fv != null)
        return fv.ToString(String.Empty, StdConvert.NumberFormat);
      else if (value.GetType().IsArray)
        throw new ArgumentException("Массивы не поддерживаются", "value");
      else
        return value.ToString();
    }

    private static string FormatDataString(string value)
    {
      int p = value.IndexOf('\'');
      if (p >= 0)
      {
        // Требуется экранирование
        StringBuilder sb = new StringBuilder(value.Length + 3);
        sb.Append(@"'");
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] == '\'')
            sb.Append(@"''");
          else
            sb.Append(value[i]);
        }
        sb.Append(@"'");
        return sb.ToString();
      }
      else
        return @"'" + value + @"'";
    }

    private static string FormatDataDateTime(DateTime value)
    {
      bool useTime = value.TimeOfDay.Ticks != 0L;

      StringBuilder sb = new StringBuilder();
      sb.Append('#');

      sb.Append(StdConvert.ToString(value.Month));
      sb.Append('/');
      sb.Append(StdConvert.ToString(value.Day));
      sb.Append('/');
      sb.Append(StdConvert.ToString(value.Year));

      if (useTime)
      {
        sb.Append(' ');
        sb.Append(StdConvert.ToString(value.Hour));
        sb.Append(':');
        sb.Append(StdConvert.ToString(value.Minute));
        sb.Append(':');
        sb.Append(StdConvert.ToString(value.Second));
      }

      sb.Append('#');

      return sb.ToString();
    }

    #endregion

    #endregion

    #region Копирование данных

    /// <summary>
    /// Копирование "Строка в строку".
    /// Копируются все таблицы из исходного набора в конечный набор.
    /// Копируются только таблицы, существующие в обоих наборах. Они должны иметь одинаковую структуру.
    /// Строки заменяются, а не добавляются, то есть при копировании таблицы используются аргументы useColumnNames=false
    /// и addRows=false.
    /// Копируются ExtendedProperties как для DataSet, так и для копируемых таблиц.
    /// </summary>
    /// <param name="srcDS">Исходный набор данных</param>
    /// <param name="dstDS">Заполняемый набор данным</param>
    public static void CopyRowsToRows(DataSet srcDS, DataSet dstDS)
    {
#if DEBUG
      if (srcDS == null)
        throw new ArgumentNullException("srcDS");
      if (dstDS == null)
        throw new ArgumentNullException("dstDS");
#endif
      if (object.ReferenceEquals(srcDS, dstDS))
        throw new ArgumentException("Нельзя копировать набор сам в себя", "dstDS");

      for (int i = 0; i < srcDS.Tables.Count; i++)
      {
        DataTable srcTable = srcDS.Tables[i];
        DataTable dstTable = dstDS.Tables[srcTable.TableName];
        if (dstTable != null)
          CopyRowsToRows(srcTable, dstTable, false, false);
      }
      CopyProperties(srcDS.ExtendedProperties, dstDS.ExtendedProperties);
    }

    private static void CopyProperties(PropertyCollection src, PropertyCollection dst)
    {
      if (src.Count == 0)
        return;
      foreach (object key in src.Keys)
        dst[key] = src[key];
    }

    /// <summary>
    /// Построчное копирование таблиц.
    /// Все строки копируются по порядку номеров, исключая удаленные, т.к. нет доступа
    /// к полям
    /// Копирование значений полей может выполняться либо по номерам столбцов 
    /// (<paramref name="useColumnNames"/>=false), либо по именам столбцов.
    /// Копируются свойства ExtendedProperties таблицы.
    /// Копируются свойства ExtendedProperties каждого столбца.
    /// </summary>
    /// <param name="srcTable">Исходная таблица</param>
    /// <param name="dstTable">Конечная таблица</param>
    /// <param name="useColumnNames">true-использовать имена столбцов (таблицы могут иметь разную структуру), 
    /// false- использовать позиции столбцов (таблицы с одинаковой структурой)</param>
    /// <param name="addRows">true - добавлять строки в конечную таблицу, false-заменять значения в строках по номерам</param>
    public static void CopyRowsToRows(DataTable srcTable, DataTable dstTable, bool useColumnNames, bool addRows)
    {
#if DEBUG
      if (srcTable == null)
        throw new ArgumentNullException("srcTable");
      if (dstTable == null)
        throw new ArgumentNullException("dstTable");
#endif
      if (object.ReferenceEquals(srcTable, dstTable))
        throw new ArgumentException("Нельзя копировать таблицу саму в себя", "dstTable");

      // Таблица может содержать первичный ключ. 
      // При построчном копировании мы не используем поиск, поэтому ключ не нужен.
      // Если ключ оставить как есть, то может возникнуть ошибка копирования.
      //
      // Например, пусть исходная таблица SrcTable содержит две строки, со значениями 
      // ключевого поля "2" и "3". Перезаписываемая таблица пусть содержит строки
      // с ключами "1" и "2". Тогда, при копировании первой строки произойдет
      // ошибка, т.к. в DstTable окажутся строки с ключами "2" и "2".
      //
      // На время копирования первичный ключ убираем
      DataColumn[] oldPrimaryKey = dstTable.PrimaryKey;
      dstTable.PrimaryKey = null;
      try
      {
        if (useColumnNames)
        {
          // При копировании по именам столбцов сначала создаем список пар, которые можно копировать
          List<int> srcColPos = new List<int>();
          List<int> dstColPos = new List<int>();
          for (int i = 0; i < srcTable.Columns.Count; i++)
          {
            int p = dstTable.Columns.IndexOf(srcTable.Columns[i].ColumnName);
            if (p >= 0)
            {
              srcColPos.Add(i);
              dstColPos.Add(p);
              // Копируем свойства столбцов
              DataColumn srcCol = srcTable.Columns[i];
              DataColumn dstCol = dstTable.Columns[p];
              CopyProperties(srcCol.ExtendedProperties, dstCol.ExtendedProperties);
            }
          }
          if (srcColPos.Count > 0)
          {
            for (int i = 0; i < srcTable.Rows.Count; i++)
            {
              DataRow srcRow = srcTable.Rows[i];
              if (srcRow.RowState == DataRowState.Deleted)
                continue;
              DataRow dstRow = addRows ? dstTable.NewRow() : dstTable.Rows[i];
              for (int j = 0; j < srcColPos.Count; j++)
                dstRow[dstColPos[j]] = srcRow[srcColPos[j]];
              if (addRows)
                dstTable.Rows.Add(dstRow);
            }
          }
        }
        else
        {
          for (int i = 0; i < srcTable.Rows.Count; i++)
          {
            DataRow srcRow = srcTable.Rows[i];
            if (srcRow.RowState == DataRowState.Deleted)
              continue;
            DataRow dstRow = addRows ? dstTable.NewRow() : dstTable.Rows[i];
            CopyRowValues(srcRow, dstRow, false);
            if (addRows)
              dstTable.Rows.Add(dstRow);
          }
          // Копируем свойства столбцов
          for (int j = 0; j < srcTable.Columns.Count; j++)
          {
            DataColumn srcCol = srcTable.Columns[j];
            DataColumn dstCol = dstTable.Columns[j];
            CopyProperties(srcCol.ExtendedProperties, dstCol.ExtendedProperties);
          }
        }
      }
      finally
      {
        dstTable.PrimaryKey = oldPrimaryKey;
      }

      // Копируем свойства таблицы 
      CopyProperties(srcTable.ExtendedProperties, dstTable.ExtendedProperties);
    }

    /// <summary>
    /// Копирование одной строки в другую.
    /// При UseColumnNames=false копируются все поля по номерам. Предполагается, что таблицы,
    /// к которым относятся строки, имеют идентичный список полей.
    /// При <paramref name="useColumnNames"/>=true копируются только одноименные поля. Таблицы могут 
    /// иметь несовпадающую структуру полей (медленнее, т.к. требуется выполнять
    /// поиск для каждого поля).
    /// Если исходная строка помечена на удаление (RowState=Deleted), то используются значения полей в
    /// <paramref name="srcRow"/> версии RowVersion=Deleted, чтобы не возникало исключение System.Data.DeletedRowInaccessibleException.
    /// </summary>
    /// <param name="srcRow">Исходная строка</param>
    /// <param name="dstRow">Строка, принимающая значения</param>
    /// <param name="useColumnNames">true - использовать имена полей, false-использовать номера полей</param>
    public static void CopyRowValues(DataRow srcRow, DataRow dstRow, bool useColumnNames)
    {
#if DEBUG
      if (srcRow == null)
        throw new ArgumentNullException("srcRow");
      if (dstRow == null)
        throw new ArgumentNullException("dstRow");

      // Это ужасно, но работать будет
      if (object.ReferenceEquals(srcRow, dstRow))
        throw new ArgumentException("Нельзя копировать строку саму в себя", "dstRow");
#endif

      if (useColumnNames)
      {
        DataColumnCollection srcColumns = srcRow.Table.Columns;
        DataColumnCollection dstColumns = dstRow.Table.Columns;
        DataRowVersion srcVer = DataRowVersion.Default;
        if (srcRow.RowState == DataRowState.Deleted)
          srcVer = DataRowVersion.Original; // 17.07.2020

        for (int i = 0; i < srcColumns.Count; i++)
        {
          int p = dstColumns.IndexOf(srcColumns[i].ColumnName);
          if (p >= 0)
            dstRow[p] = srcRow[i, srcVer];
        }
      }
      else
      {
        if (srcRow.RowState == DataRowState.Deleted) // 17.07.2020
        {
          int n = srcRow.Table.Columns.Count;
          for (int i = 0; i < n; i++)
            dstRow[i] = srcRow[i, DataRowVersion.Original];
        }
        else
        {
          object[] a = srcRow.ItemArray;
          dstRow.ItemArray = a;
        }
      }
    }


    /// <summary>
    /// Копирование значений полей из одной строки в другую для заданных имен полей.
    /// Если исходная строка помечена на удаление (RowState=Deleted), то используются значения полей в
    /// <paramref name="srcRow"/> версии RowVersion=Deleted, чтобы не возникало исключение System.Data.DeletedRowInaccessibleException.
    /// </summary>
    /// <param name="srcRow">Исходная строка</param>
    /// <param name="dstRow">Конечная строка</param>
    /// <param name="columnNames">Список имен столбцов, разделенных запятыми</param>
    public static void CopyRowValues(DataRow srcRow, DataRow dstRow, string columnNames)
    {
      CopyRowValues(srcRow, dstRow, columnNames.Split(','));
    }

    /// <summary>
    /// Копирование значений полей из одной строки в другую для заданных имен полей.
    /// Если исходная строка помечена на удаление (RowState=Deleted), то используются значения полей в
    /// <paramref name="srcRow"/> версии RowVersion=Deleted, чтобы не возникало исключение System.Data.DeletedRowInaccessibleException.
    /// </summary>
    /// <param name="srcRow">Исходная строка</param>
    /// <param name="dstRow">Конечная строка</param>
    /// <param name="columnNames">Массив имен столбцов</param>
    public static void CopyRowValues(DataRow srcRow, DataRow dstRow, string[] columnNames)
    {
#if DEBUG
      if (srcRow == null)
        throw new ArgumentNullException("srcRow");
      if (dstRow == null)
        throw new ArgumentNullException("dstRow");

      // Это ужасно, но работать будет
      if (object.ReferenceEquals(srcRow, dstRow))
        throw new ArgumentException("Нельзя копировать строку саму в себя", "dstRow");
#endif

      DataRowVersion srcVer = DataRowVersion.Default;
      if (srcRow.RowState == DataRowState.Deleted)
        srcVer = DataRowVersion.Original; // 17.07.2020

      for (int i = 0; i < columnNames.Length; i++)
        dstRow[columnNames[i]] = srcRow[columnNames[i], srcVer];
    }

    /// <summary>
    /// Копирует все поля, имена которых начинаются с префикса <paramref name="srcColumnPrefix"/>, из строки <paramref name="srcRow"/> в
    /// строку <paramref name="dstRow"/> в поле, имя которого начинается с <paramref name="dstColumnPrefix"/>.
    /// Если исходная строка помечена на удаление (RowState=Deleted), то используются значения полей в
    /// <paramref name="srcRow"/> версии RowVersion=Deleted, чтобы не возникало исключение System.Data.DeletedRowInaccessibleException.
    /// </summary>
    /// <param name="srcRow">Исходная строка</param>
    /// <param name="dstRow">Конечная строка</param>
    /// <param name="srcColumnPrefix">Префикс имени поля в исходной строке. Регистр символов учитывается</param>
    /// <param name="dstColumnPrefix">Префикс имени поля в конечной строке. Регистр символов учитывается</param>
    public static void CopyRowValuesForPrefix(DataRow srcRow, DataRow dstRow, string srcColumnPrefix, string dstColumnPrefix)
    {
#if DEBUG
      if (srcRow == null)
        throw new ArgumentNullException("srcRow");
      if (dstRow == null)
        throw new ArgumentNullException("dstRow");

      // Строки могут совпадать
      //if (object.ReferenceEquals(srcRow, dstRow))
      //  throw new ArgumentException("Нельзя копировать строку саму в себя", "dstRow");
#endif

      DataRowVersion srcVer = DataRowVersion.Default;
      if (srcRow.RowState == DataRowState.Deleted)
        srcVer = DataRowVersion.Original; // 17.07.2020

      for (int i = 0; i < srcRow.Table.Columns.Count; i++)
      {
        string columnName = srcRow.Table.Columns[i].ColumnName;
        if (!String.IsNullOrEmpty(srcColumnPrefix))
        {
          if (columnName.StartsWith(srcColumnPrefix, StringComparison.Ordinal))
            columnName = columnName.Substring(srcColumnPrefix.Length);
          else
            continue;
        }
        if (!String.IsNullOrEmpty(dstColumnPrefix))
          columnName = dstColumnPrefix + columnName;
        int p = dstRow.Table.Columns.IndexOf(columnName);
        if (p >= 0)
          dstRow[p] = srcRow[i, srcVer];
      }
    }

    #endregion

    #region Замена значений

    /// <summary>
    /// Заменяет значения поля <paramref name="columnName"/>, содержащие DBNull на значение <paramref name="newValue"/>
    /// </summary>
    /// <param name="table">Таблица, в которой выполняется замена</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="newValue">Новое значение</param>
    public static void ReplaceNulls(DataTable table, string columnName, object newValue)
    {
#if DEBUG
      if (newValue == null)
        throw new ArgumentNullException("newValue");
#endif

      int fp = GetColumnPosWithCheck(table, columnName);

      for (int i = 0; i < table.Rows.Count; i++)
      {
        if (table.Rows[i].IsNull(fp))
          table.Rows[i][fp] = newValue;
      }
    }

    /// <summary>
    /// Заменяет значения поля <paramref name="columnName"/>, содержащие DBNull на значение <paramref name="newValue"/>.
    /// Т.к. замена значения может привести к перестроению DataView, собираем
    /// сначала массив строк, в котором выполняем замену
    /// </summary>
    /// <param name="dv">Просмотр для таблицы, в которой выполняется замена</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="newValue">Новое значение</param>
    public static void ReplaceNulls(DataView dv, string columnName, object newValue)
    {
      DataRow[] rows = GetDataViewRows(dv);
      ReplaceNulls(rows, columnName, newValue);
    }

    /// <summary>
    /// Заменяет значения поля <paramref name="columnName"/>, содержащие DBNull на значение <paramref name="newValue"/>
    /// Строки должны относиться к одной таблице или нескольким таблицам, чтобы
    /// позиция поля была одинаковой для всех строк
    /// </summary>
    /// <param name="rows">Массив строк, в которых выполняется замена</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="newValue">Новое значение</param>
    public static void ReplaceNulls(DataRow[] rows, string columnName, object newValue)
    {
#if DEBUG
      if (newValue == null)
        throw new ArgumentNullException("newValue");
#endif

      if (rows == null)
        return;
      if (rows.Length == 0)
        return;
      int fp = GetColumnPosWithCheck(rows[0].Table, columnName);

      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i].IsNull(fp))
          rows[i][fp] = newValue;
      }
    }

    #endregion

    #region Обрезка текстовых полей

    /// <summary>
    /// Обрезать строки значений заданного текстового поля в таблице.
    /// Необходимость в обрезке может появится, если таблица загружена из базы данныз,
    /// но для доступа не используются метод GetString(), который выполняет обрезку
    /// при извлечении строки.
    /// Если для столбца разрешен признак DataColumn.AllowDBNull, то пустые строки,
    /// а также строки, содержащие только пробелы, будут заменены на DBNull
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя текстового поля</param>
    public static void StrTrim(DataTable table, string columnName)
    {
      StrTrim(table, GetColumnPosWithCheck(table, columnName));
    }

    /// <summary>
    /// Обрезать строки значений заданного текстового поля в таблице.
    /// Необходимость в обрезке может появится, если таблица загружена из базы данныз,
    /// но для доступа не используются метод GetString(), который выполняет обрезку
    /// при извлечении строки.
    /// Если для столбца разрешен признак DataColumn.AllowDBNull, то пустые строки,
    /// а также строки, содержащие только пробелы, будут заменены на DBNull
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnIndex">Индекс текстового поля</param>
    public static void StrTrim(DataTable table, int columnIndex)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("Table");
      if (columnIndex < 0 || columnIndex >= table.Columns.Count)
        throw new ArgumentOutOfRangeException("columnIndex", columnIndex, "Неправильный индекс столбца таблицы");
#endif
      DataColumn col = table.Columns[columnIndex];

      if (col.DataType != typeof(string))
        throw new ArgumentException("Столбец \"" + col.ColumnName + "\" не является строковым");

      foreach (DataRow row in table.Rows)
      {
        if (row.IsNull(columnIndex))
          continue;
        string s1 = (string)row[columnIndex];
        string s2 = s1.Trim();
        if (s2.Length == s1.Length)
          continue;
        if (s2.Length == 0 && col.AllowDBNull)
          row[columnIndex] = DBNull.Value;
        else
          row[columnIndex] = s2;
      }
    }

    /// <summary>
    /// Обрезать строки значений всех текстовых полей в таблице.
    /// Необходимость в обрезке может появится, если таблица загружена из базы данныз,
    /// но для доступа не используются метод GetString(), который выполняет обрезку
    /// при извлечении строки.
    /// Если для столбца разрешен признак DataColumn.AllowDBNull, то пустые строки,
    /// а также строки, содержащие только пробелы, будут заменены на DBNull
    /// </summary>
    /// <param name="table">Таблица</param>
    public static void StrTrim(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.Rows.Count == 0)
        return;

      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (table.Columns[i].DataType == typeof(string))
          StrTrim(table, i);
      }
    }

    /// <summary>
    /// Обрезать строки значений всех текстовых полей в таблицах набора DataSet.
    /// Необходимость в обрезке может появится, если таблица загружена из базы данныз,
    /// но для доступа не используются метод GetString(), который выполняет обрезку
    /// при извлечении строки.
    /// Если для столбца разрешен признак DataColumn.AllowDBNull, то пустые строки,
    /// а также строки, содержащие только пробелы, будут заменены на DBNull
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public static void StrTrim(DataSet ds)
    {
#if DEBUG
      if (ds == null)
        throw new ArgumentNullException("ds");
#endif

      for (int i = 0; i < ds.Tables.Count; i++)
        StrTrim(ds.Tables[i]);
    }

    #endregion

    #region Обновление строк таблицы

    /// <summary>
    /// Обновление строк таблицы приемника строками из таблицы-источника по значению первичного ключа
    /// Таблица-приемник должна иметь первичный ключ. Строки в исходной таблице
    /// должны иметь поля для вычисления первичного ключа во второй таблице
    /// Копирование значений полей может выполняться либо по номерам столбцов 
    /// (<paramref name="useColumnNames"/>=false), либо по именам столбцов
    /// Cвойства ExtendedProperties не копируются.
    /// Если таблица-приемник не имеет строки, соответствуюшей какой-либо строке в исходной таблице, такая строка пропускается.
    /// Количество строк в таблице-приемнике не меняется.
    /// Для полного обновления таблицы используйте метод UpdataTableByPrimaryKey().
    /// </summary>
    /// <param name="srcTable">Таблица-источник строк</param>
    /// <param name="dstTable">Таблица-приемник строк (обновляемая таблица)</param>
    /// <param name="useColumnNames">Если true, то будут использоваться имена столбцов, а не позиции</param>
    public static void UpdateRowsByPrimaryKey(DataTable srcTable, DataTable dstTable, bool useColumnNames)
    {
#if DEBUG
      if (srcTable == null)
        throw new ArgumentNullException("srcTable");
      if (dstTable == null)
        throw new ArgumentNullException("dstTable");
#endif
      if (Object.ReferenceEquals(srcTable, dstTable))
        throw new ArgumentException("Таблицы не должны совпадать", "dstTable");

      foreach (DataRow srcRow in srcTable.Rows)
        UpdateRowByPrimaryKey(srcRow, dstTable, useColumnNames);
    }

    /// <summary>
    /// Обновление строки таблицы-приемника значениями из другой строки по значению ключа
    /// </summary>
    /// <param name="srcRow">Исходная строка. Если null, то никаких действий не выполняется</param>
    /// <param name="dstTable">Обновляемая таблица</param>
    /// <param name="useColumnNames">true, если при копировании значений будут использоваться имена полей,
    /// false-будет выполняться копирование столбцов по порядку</param>
    /// <returns>true, если в таблице <paramref name="dstTable"/> найдена строка для обновления</returns>
    private static bool UpdateRowByPrimaryKey(DataRow srcRow, DataTable dstTable, bool useColumnNames)
    {
      if (srcRow == null)
        return false;

#if DEBUG
      if (dstTable == null)
        throw new ArgumentNullException("dstTable");
#endif

      if (srcRow.RowState == DataRowState.Deleted)
        return false;

      // Поиск по ключу
      object[] keys = GetPrimaryKeyValues(srcRow, dstTable);
      if (keys == null)
        throw new ArgumentException("Не удалось извлечь ключевые поля для строки", "dstTable");

      DataRow dstRow = dstTable.Rows.Find(keys);
      if (dstRow == null)
        return false;

      CopyRowValues(srcRow, dstRow, useColumnNames);
      return true;
    }

    /// <summary>
    /// Обновление таблицы строками из другой таблицы по первичному ключу.
    /// Таблица-приемник должна иметь первичный ключ. Строки в исходной таблице
    /// должны иметь поля для вычисления первичного ключа во второй таблице
    /// После выполнения действия конечная таблица будет иметь столько же строк (кроме
    /// удаленных), сколько и исходная таблица. В конечной таблице строки могут
    /// добавляться, удаляться, изменяться или оставаться неизменными.
    /// Копирование значений полей может выполняться либо по номерам столбцов 
    /// (<paramref name="useColumnNames"/>=false), либо по именам столбцов
    /// Cвойства ExtendedProperties не копируются.
    /// В <paramref name="dstTable"/> могут оставаться удаленные строки. 
    /// После выполнения обновления рекомендуется вызвать <paramref name="dstTable"/>.AcceptChanges(), чтобы полностью убрать удаленные строки.
    /// </summary>
    /// <param name="srcTable">Исходная таблица</param>
    /// <param name="dstTable">Таблица, в которой выполняются замены</param>
    /// <param name="useColumnNames">true, если при копировании значений будут использоваться имена полей,
    /// false-будет выполняться копирование столбцов по порядку</param>
    public static void UpdateTableByPrimaryKey(DataTable srcTable, DataTable dstTable, bool useColumnNames)
    {
#if DEBUG
      if (srcTable == null)
        throw new ArgumentNullException("srcTable");
      if (dstTable == null)
        throw new ArgumentNullException("dstTable");
#endif
      if (Object.ReferenceEquals(srcTable, dstTable))
        throw new ArgumentException("Таблицы не должны совпадать", "dstTable");

      // Список строк в DstTable, которые были использованы
      Dictionary<DataRow, object> usedRows = new Dictionary<DataRow, object>(dstTable.Rows.Count);

      int orgRowCount = dstTable.Rows.Count;

      dstTable.BeginLoadData();
      try
      {
        #region Первый проход - добавление / изменение строк

        foreach (DataRow srcRow in srcTable.Rows)
        {
          // Поиск по ключу
          object[] keys = GetPrimaryKeyValues(srcRow, dstTable);
          if (keys == null)
            throw new InvalidOperationException("Не удалось извлечь ключевые поля для строки");

          DataRow dstRow = dstTable.Rows.Find(keys);
          if (dstRow == null)
          {
            dstRow = dstTable.NewRow();
            CopyRowValues(srcRow, dstRow, useColumnNames);
            dstTable.Rows.Add(dstRow);
          }
          else
          {
            CopyRowValues(srcRow, dstRow, useColumnNames);
            usedRows.Add(dstRow, null);
          }
        }

        #endregion

        #region Второй проход - удаление ненужных строк

        for (int i = orgRowCount - 1; i >= 0; i--)
        {
          if (!usedRows.ContainsKey(dstTable.Rows[i]))
            dstTable.Rows[i].Delete();
        }

        #endregion
      }
      finally
      {
        dstTable.EndLoadData();
      }
    }

    #endregion

    #region Функции для работы с DataView

    /// <summary>
    /// Поиск строки DataRowView, соответствующей исходной строке DataRow в объекте DataView
    /// </summary>
    /// <param name="dv">Объект DataView</param>
    /// <param name="row">Строка DataRow</param>
    /// <returns>Найденный объект DataRowView или null</returns>
    public static DataRowView FindDataRowView(DataView dv, DataRow row)
    {
      int idx = FindDataRowViewIndex(dv, row);
      if (idx < 0)
        return null;
      else
        return dv[idx];
    }

    /// <summary>
    /// Поиск индекса исходной строки DataRow в объекте DataView
    /// </summary>
    /// <param name="dv">Объект DataView</param>
    /// <param name="row">Строка DataRow</param>
    /// <returns>Индекс соответствующего объекта DataRowView в DataView или
    /// (-1), если строка не входит в DataView (например, отфильтрована)</returns>
    public static int FindDataRowViewIndex(DataView dv, DataRow row)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif
      if (row == null)
        return -1;

      if (!Object.ReferenceEquals(row.Table, dv.Table))
        return -1; // 04.07.2021 - строка относится к другой таблице. Поиск не имеет смысла

      if (dv.Count > 50 && (!String.IsNullOrEmpty(dv.Sort)))
      {
        // 04.07.2021
        // Оптимизированный поиск с использованием DataView.Find()

        string[] colNames = GetDataViewSortColumnNames(dv.Sort);
        int[] colPoss = new int[colNames.Length];
        object[] rowVals = new object[colNames.Length];
        for (int j = 0; j < colNames.Length; j++)
        {
          colPoss[j] = row.Table.Columns.IndexOf(colNames[j]);
#if DEBUG
          if (colPoss[j] < 0)
            throw new BugException("Не нашли поле \"" + colNames[j] + "\"");
#endif

          rowVals[j] = row[colPoss[j]];
        }

        int p0 = dv.Find(rowVals);
        if (p0 < 0)
          return -1;

        for (int i = p0; i < dv.Count; i++)
        {
          if (object.ReferenceEquals(dv[i].Row, row))
            return i; // нашли

          bool sameKeys = true;
          for (int j = 0; j < colNames.Length; j++)
          {
            if (!Object.Equals(dv[i].Row[colPoss[j]], rowVals[j]))
            {
              sameKeys = false;
              break;
            }
          }
          if (!sameKeys)
            break;
        }

        // Нет 100% гарантии, что не осталось строк перед найденной.
        // На всякий случай, выполняем поиск назад.
        // Скорее всего, цикл завершится после первого же такта
        for (int i = p0 - 1; i >= 0; i--)
        {
          if (object.ReferenceEquals(dv[i].Row, row))
            return i; // нашли

          bool sameKeys = true;
          for (int j = 0; j < colNames.Length; j++)
          {
            if (!Object.Equals(dv[i].Row[colPoss[j]], rowVals[j]))
            {
              sameKeys = false;
              break;
            }
          }
          if (!sameKeys)
            break;
        }
      }

      // Придется перебирать все строки вручную
      for (int i = 0; i < dv.Count; i++)
      {
        if (object.ReferenceEquals(dv[i].Row, row))
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Получение массива строк DataRow соответствующих объекту DataView
    /// </summary>
    /// <param name="dv">Объект DataView. Если null, то будет возвращен пустой массив строк</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] GetDataViewRows(DataView dv)
    {
      if (dv == null)
        return new DataRow[0];
      DataRow[] res = new DataRow[dv.Count];
      for (int i = 0; i < dv.Count; i++)
        res[i] = dv[i].Row;
      return res;
    }

#if XXXXXXXXXXXXXXX
    // Есть метод DataView.ToTable()


    /// <summary>
    /// Создает новую таблицу DataTable, куда попадают копии строк из таблицы,
    /// на которой основан объект DataView. Учитывается порядок строк и фильтр,
    /// заданный в просмотре
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <returns>Новая таблица</returns>
    public static DataTable CreateTableForDataView(DataView dv)
    {
      DataTable Table = dv.Table.Clone();
      Table.TableName = dv.Table.TableName;
      for (int i = 0; i < dv.Count; i++)
      {
        DataRow SrcRow = dv[i].Row;
        Table.Rows.Add(SrcRow.ItemArray);
      }
      return Table;
    }
#endif

    /// <summary>
    /// Создает словарь пар "Значение поле Id" - "Индекс строки в просмотре" для
    /// набора данных DataView.
    /// Предупреждение: после изменения параметров DataView или данных в таблице,
    /// полученный словарь становится недействительным
    /// </summary>
    /// <param name="dv">Объект DataView</param>
    /// <returns></returns>
    public static Dictionary<Int32, int> GetDataViewIdIndexDictionary(DataView dv)
    {
      Dictionary<Int32, int> res = new Dictionary<Int32, int>(dv.Count);
      for (int i = 0; i < dv.Count; i++)
      {
        Int32 id = (Int32)(dv[i].Row["Id"]);
        res.Add(id, i);
      }
      return res;
    }

    #region GetDataViewSortColumnNames

    /// <summary>
    /// Извлечь имена столбцов из свойства DataView.Sort.
    /// Строка может содержать пробелы и суффиксы ASC и DESC (игнорируются)
    /// Если строка <paramref name="sort"/> пустая, возвращается пустой массив.
    /// В этой версии информация о порядке сортировки теряется.
    /// Имя столбца может быть заключено в квадратные скобки, которые удаляются.
    /// </summary>
    /// <param name="sort">Свойство DataView.Sort</param>
    /// <returns>Массис имен полей или null</returns>
    /// <remarks>
    /// DataView.Sort не может содержать функции и математические операции, а только имена полей и порядок сортировки.
    /// </remarks>
    public static string[] GetDataViewSortColumnNames(string sort)
    {
      string[] columnNames;
      ListSortDirection[] directions;
      GetDataViewSortColumnNames(sort, out columnNames, out directions);
      return columnNames;
    }

    /// <summary>
    /// Извлечь имена столбцов из свойства DataView.Sort.
    /// Строка может содержать пробелы и суффиксы ASC и DESC (игнорируются)
    /// Расширенная версия. Возвращается массив значений порядка сортировки.
    /// Имя столбца может быть заключено в квадратные скобки, которые удаляются.
    /// Если строка <paramref name="sort"/> пустая, возвращаются пустые массивы.
    /// </summary>
    /// <param name="sort">Свойство DataView.Sort</param>
    /// <param name="columnNames">Сюда записывается массив имен столбцов</param>
    /// <param name="directions">Сюда записывается массив признаков сортировки</param>
    /// <returns>Массис имен полей или null</returns>
    /// <remarks>
    /// DataView.Sort не может содержать функции и математические операции, а только имена полей и порядок сортировки.
    /// </remarks>
    public static void GetDataViewSortColumnNames(string sort, out string[] columnNames, out ListSortDirection[] directions)
    {
      if (String.IsNullOrEmpty(sort))
      {
        columnNames = EmptyStrings;
        directions = EmptySortDirections;
        return;
      }

      columnNames = sort.Split(',');
      directions = new ListSortDirection[columnNames.Length];

      for (int i = 0; i < columnNames.Length; i++)
      {
        directions[i] = ListSortDirection.Ascending;

        string s = columnNames[i].Trim();
        if (s.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase))
        {
          s = s.Substring(0, s.Length - 5);
          directions[i] = ListSortDirection.Descending;
        }
        else
        {
          if (s.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(0, s.Length - 4);
        }
        if (s.Length == 0)
          throw new ArgumentException("Неправильный формат порядка сортировки", "sort");
        if (s[0] == '[' && s[s.Length - 1] == ']') // 21.04.2023
          s = s.Substring(1, s.Length - 2);
        columnNames[i] = s;
      }
    }

    /// <summary>
    /// Извлечь имя столбца из свойства DataView.Sort.
    /// Строка может содержать пробелы и суффикс ASC и DESC (игнорируются)
    /// Если строка <paramref name="sort"/> пустая, возвращается пустая строка.
    /// Имя столбца может быть заключено в квадратные скобки, которые удаляются.
    /// В этой версии информация о порядке сортировки теряется.
    /// Если задан порядок сортировки по нескольким полям, генерируется исключение.
    /// </summary>
    /// <param name="sort">Свойство DataView.Sort</param>
    /// <returns>Имя поля или пустая строка</returns>                          
    /// <remarks>
    /// DataView.Sort не может содержать функции и математические операции, а только имена полей и порядок сортировки.
    /// </remarks>
    public static string GetDataViewSortSingleColumnName(string sort)
    {
      string columnName;
      ListSortDirection direction;
      GetDataViewSortSingleColumnName(sort, out columnName, out direction);
      return columnName;
    }

    /// <summary>
    /// Извлечь имя столбца из свойства DataView.Sort.
    /// Строка может содержать пробелы и суффикс ASC и DESC.
    /// Если строка <paramref name="sort"/> пустая, возвращается пустая строка и порядок сортировки по возрастанию.
    /// Имя столбца может быть заключено в квадратные скобки, которые удаляются.
    /// Если задан порядок сортировки по нескольким полям, генерируется исключение.
    /// </summary>
    /// <param name="sort">Свойство DataView.Sort</param>
    /// <param name="columnName">Сюда записывается имя поля</param>
    /// <param name="direction">Сюда записывается направление сортировки</param>
    /// <remarks>
    /// DataView.Sort не может содержать функции и математические операции, а только имена полей и порядок сортировки.
    /// </remarks>
    public static void GetDataViewSortSingleColumnName(string sort, out string columnName, out ListSortDirection direction)
    {
      direction = ListSortDirection.Ascending;
      if (String.IsNullOrEmpty(sort))
      {
        columnName = String.Empty;
        return;
      }

      if (sort.IndexOf(',') >= 0)
        throw new InvalidOperationException("В сортировке присутствует символ \",\". Следует использовать метод GetDataViewSortColumnNames(), поддерживающий массив полей");

      columnName = sort.Trim();
      if (columnName.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase))
      {
        columnName = columnName.Substring(0, columnName.Length - 5);
        direction = ListSortDirection.Descending;
      }
      else
      {
        if (columnName.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
          columnName = columnName.Substring(0, columnName.Length - 4);
      }
      if (columnName.Length == 0)
        throw new ArgumentException("Неправильный формат порядка сортировки", "sort");
      if (columnName[0] == '[' && columnName[columnName.Length - 1] == ']')
        columnName = columnName.Substring(1, columnName.Length - 2);
    }

    /// <summary>
    /// Получение выражения для сортировки DataView.Sort.
    /// Длина массивов <paramref name="columnNames"/> и <paramref name="directions"/> должна быть одинаковой.
    /// Если длина массивов равна 0, возвращается пустая строка.
    /// В текущей реализации квадратные скобки вокруг имен полей не добавляются.
    /// Функция является обратной по отношению к GetDataViewSortColumnNames.
    /// </summary>
    /// <param name="columnNames">Массив имен столбцов</param>
    /// <param name="directions">Массив направлений</param>
    /// <returns></returns>
    public static string GetDataViewSort(string[] columnNames, ListSortDirection[] directions)
    {
#if DEBUG
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (directions == null)
        throw new ArgumentNullException("directions");
#endif
      if (directions.Length != columnNames.Length)
        throw new ArgumentException("Длина массивов должна быть одинаковой");

      if (columnNames.Length == 0)
        return String.Empty;
      if (columnNames.Length == 1 && directions[0] == ListSortDirection.Ascending)
      {
        if (String.IsNullOrEmpty(columnNames[0]))
          throw new ArgumentException("columnNames[0] is null or empty", "columnNames");
        return columnNames[0];
      }

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < columnNames.Length; i++)
      {
        if (String.IsNullOrEmpty(columnNames[i]))
          throw new ArgumentException("columnNames[" + i.ToString() + "] is null or empty", "columnNames");

        if (i > 0)
          sb.Append(',');
        sb.Append(columnNames[i]);
        if (directions[i] == ListSortDirection.Descending)
          sb.Append(" DESC");
      }
      return sb.ToString();
    }

    #endregion

    /// <summary>
    /// Получение выражения LIKE для DataView.RowFilter.
    /// Возвращает строку LIKE '<paramref name="str"/>*' с экранированием некоторых символов
    /// </summary>
    /// <param name="str">Начало шаблона поиска</param>
    /// <returns>Строка для фильтра</returns>
    public static string GetDataViewLikeExpressionString(string str)
    {
      // Аналогичный метод есть в ExtDB.dll
      // Метод DataViewDBxSqlFormatter.OnFormatStartsWithFilter()


      str = str.Replace("\'", "\'\'"); // 21.08.2017. Апострофы удваиваются
      return "LIKE \'" + MakeEscapedChars(str, new char[] { '*', '%', '[', ']' }, "[", "]") + "*\'";
    }

    /// <summary>
    /// Окружение специальных символов в строке
    /// </summary>
    /// <param name="str">Строка, возможно содержащая символы</param>
    /// <param name="escapedChars">Символы, которые требуется окружить</param>
    /// <param name="prefix">Окружение слева</param>
    /// <param name="suffix">Окружение справа</param>
    /// <returns></returns>
    private static string MakeEscapedChars(string str, char[] escapedChars, string prefix, string suffix)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (str.IndexOfAny(escapedChars) < 0)
        return str;
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < str.Length; i++)
      {
        char c = str[i];
        if (Array.IndexOf<char>(escapedChars, c) >= 0)
        {
          // Спецсимвол
          sb.Append(prefix);
          sb.Append(c);
          sb.Append(suffix);
        }
        else
          // Обычный символ
          sb.Append(c);
      }
      return sb.ToString();
    }


    #region FindOrAddRow

    /// <summary>
    /// Найти строку в таблице с заданным значением поля для поиска. Если таблица
    /// не содержит такой строки, то строка добавляется.
    /// Для поиска используется объект DataView, в котором задано свойство Sort.
    /// Эта перегрузка не позволяет определить, была найдена существующая строка или добавлена новая
    /// </summary>
    /// <param name="dv">Просмотр таблицы, в котором свойство Sort задает одно поле для поиска</param>
    /// <param name="searchValue">Значение поля для поиска</param>
    /// <returns>Найденная или созданная строка</returns>
    public static DataRow FindOrAddDataRow(DataView dv, object searchValue)
    {
      DataRow row;
      FindOrAddDataRow(dv, searchValue, out row);
      return row;
    }

    /// <summary>
    /// Найти строку в таблице с заданным значением поля для поиска. Если таблица
    /// не содержит такой строки, то строка добавляется.
    /// Для поиска используется объект DataView, в котором задано свойство Sort.
    /// Возвращает true, если была создана новая строка
    /// </summary>
    /// <param name="dv">Просмотр таблицы, в котором свойство Sort задает одно поле для поиска</param>
    /// <param name="searchValue">Значение поля для поиска</param>
    /// <param name="row">Найденная или созданная строка</param>
    /// <returns>Признак добавления строки</returns>
    public static bool FindOrAddDataRow(DataView dv, object searchValue, out DataRow row)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(dv.Sort))
        throw new ArgumentException("Свойство DataView.Sort не установлено", "dv");
#endif
      if (dv.Sort.IndexOf(',') >= 0)
        throw new ArgumentException("Свойство DataView.Sort задает несколько полей для сортировки. Используйте перегрузку метода FindOrAddRow(), принимающую массив значений", "dv");

      int p = dv.Find(searchValue);
      if (p < 0)
      {
        row = dv.Table.NewRow();
        string colName = GetDataViewSortSingleColumnName(dv.Sort);
        row[colName] = searchValue;
        dv.Table.Rows.Add(row);
        return true;
      }
      else
      {
        row = dv[p].Row;
        return false;
      }
    }

    /// <summary>
    /// Найти строку в таблице с заданными значениями полей для поиска. Если таблица
    /// не содержит такой строки, то строка добавляется.
    /// Для поиска используется объект DataView, в котором задано свойство Sort.
    /// Эта перегрузка не позволяет определить, была найдена существующая строка или добавлена новая
    /// </summary>
    /// <param name="dv">Просмотр таблицы, в котором свойство Sort задает поля для поиска</param>
    /// <param name="searchValues">Значения полей для поиска</param>
    /// <returns>Найденная или созданная строка</returns>
    public static DataRow FindOrAddDataRow(DataView dv, object[] searchValues)
    {
      DataRow row;
      FindOrAddDataRow(dv, searchValues, out row);
      return row;
    }

    /// <summary>
    /// Найти строку в таблице с заданными значениями полей для поиска. Если таблица
    /// не содержит такой строки, то строка добавляется.
    /// Для поиска используется объект DataView, в котором задано свойство Sort.
    /// Возвращает true, если была создана новая строка
    /// </summary>
    /// <param name="dv">Просмотр таблицы, в котором свойство Sort задает поля для поиска</param>
    /// <param name="searchValues">Значения полей для поиска</param>
    /// <param name="row">Найденная или созданная строка</param>
    /// <returns>Признак добавления строки</returns>
    public static bool FindOrAddDataRow(DataView dv, object[] searchValues, out DataRow row)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(dv.Sort))
        throw new ArgumentException("Свойство DataView.Sort не установлено", "dv");
      if (searchValues == null)
        throw new ArgumentNullException("searchValues");
#endif

      int p = dv.Find(searchValues);
      if (p < 0)
      {
        row = dv.Table.NewRow();
        string[] colNames = GetDataViewSortColumnNames(dv.Sort); // не может вернуть null, т.к. dvSort - непустая строка
        if (searchValues.Length != colNames.Length)
          throw new ArgumentException("Длина списка значений (" + searchValues.Length.ToString() + ") не совпадает с числом полей для сортироки (" + colNames.Length.ToString() + ") заданном в свойстве Sort объекта DataView", "searchValues");
        for (int i = 0; i < colNames.Length; i++)
          row[colNames[i]] = searchValues[i];
        dv.Table.Rows.Add(row);
        return true;
      }
      else
      {
        row = dv[p].Row;
        return false;
      }
    }

    #endregion

    #endregion

    #region Уникальные значения

    /// <summary>
    /// Создает копию исходной таблицы <paramref name="srcTable"/>, в которую добавляются только строки,
    /// содержащие уникальные значения полей, заданных <paramref name="uniqueColumnNames"/>.
    /// Результирующая таблица имеет такую же структуру, как и исходная.
    /// Удаленные строки исходной таблицы пропускаются.
    /// </summary>
    /// <param name="srcTable">Исходная таблица данных</param>
    /// <param name="uniqueColumnNames">Массив имен полей, которые должны содержать исходные значения</param>
    /// <returns>Копия таблицы, содержащая меньшее число строк</returns>
    public static DataTable CreateUniqueTable(DataTable srcTable, string[] uniqueColumnNames)
    {
#if DEBUG
      if (srcTable == null)
        throw new ArgumentNullException("srcTable");
      if (uniqueColumnNames == null)
        throw new ArgumentNullException("uniqueColumnNames");
#endif

      DataTable resTable = srcTable.Clone();
      if (srcTable.Rows.Count == 0)
        return resTable;
      int[] uniqueColPoss = new int[uniqueColumnNames.Length];
      for (int j = 0; j < uniqueColumnNames.Length; j++)
        uniqueColPoss[j] = GetColumnPosWithCheck(srcTable, uniqueColumnNames[j]);

      resTable.DefaultView.Sort = String.Join(",", uniqueColumnNames);
      resTable.DefaultView.RowFilter = String.Empty;

      object[] keys = new object[uniqueColumnNames.Length];
      for (int i = 0; i < srcTable.Rows.Count; i++)
      {
        DataRow srcRow = srcTable.Rows[i];
        if (srcRow.RowState == DataRowState.Deleted)
          continue;

        for (int j = 0; j < uniqueColumnNames.Length; j++)
          keys[j] = srcRow[uniqueColPoss[j]];

        if (resTable.DefaultView.Find(keys) < 0)
          resTable.Rows.Add(srcRow.ItemArray);
      }

      resTable.DefaultView.Sort = srcTable.DefaultView.Sort;
      resTable.DefaultView.RowFilter = srcTable.DefaultView.RowFilter;
      return resTable;
    }

    /// <summary>
    /// Создает копию исходной таблицы <paramref name="srcTable"/>, в которую добавляются только строки,
    /// содержащие уникальные значения полей, заданных <paramref name="uniqueColumnNames"/>.
    /// Результирующая таблица имеет такую же структуру, как и исходная.
    /// Удаленные строки исходной таблицы пропускаются.
    /// </summary>
    /// <param name="srcTable">Исходная таблица данных</param>
    /// <param name="uniqueColumnNames">Список имен полей, разделенных запятыми, которые должны содержать исходные значения</param>
    /// <returns>Копия таблицы, содержащая меньшее число строк</returns>
    public static DataTable CreateUniqueTable(DataTable srcTable, string uniqueColumnNames)
    {
#if DEBUG
      if (String.IsNullOrEmpty(uniqueColumnNames))
        throw new ArgumentNullException("UniqueColumnNames");
#endif

      return CreateUniqueTable(srcTable, uniqueColumnNames.Split(','));
    }

    /// <summary>
    /// Создает копию исходной таблицы <paramref name="srcDV"/>.Table, в которую добавляются только строки,
    /// содержащие уникальные значения полей, заданных <paramref name="uniqueColumnNames"/>.
    /// Результирующая таблица имеет такую же структуру, как и исходная.
    /// Порядок строк в <paramref name="srcDV"/> определяет, какая именно строка будет добавлена в 
    /// результирующую таблицу. Добавляется первая строка, содержащая уникальные значения полей
    /// </summary>
    /// <param name="srcDV">Просмотр для исходной таблицы данных</param>
    /// <param name="uniqueColumnNames">Массив имен полей, которые должны содержать исходные значения</param>
    /// <returns>Копия таблицы, содержащая меньшее число строк</returns>
    public static DataTable CreateUniqueTable(DataView srcDV, string[] uniqueColumnNames)
    {
#if DEBUG
      if (srcDV == null)
        throw new ArgumentNullException("srcDV");
      if (uniqueColumnNames == null)
        throw new ArgumentNullException("uniqueColumnNames");
#endif

      DataTable resTable = srcDV.Table.Clone();
      if (srcDV.Count == 0)
        return resTable;
      int[] uniqueColPoss = new int[uniqueColumnNames.Length];
      for (int j = 0; j < uniqueColumnNames.Length; j++)
        uniqueColPoss[j] = GetColumnPosWithCheck(srcDV.Table, uniqueColumnNames[j]);

      resTable.DefaultView.Sort = String.Join(",", uniqueColumnNames);
      resTable.DefaultView.RowFilter = String.Empty;

      object[] keys = new object[uniqueColumnNames.Length];
      for (int i = 0; i < srcDV.Count; i++)
      {
        DataRow SrcRow = srcDV[i].Row;

        for (int j = 0; j < uniqueColumnNames.Length; j++)
          keys[j] = SrcRow[uniqueColPoss[j]];

        if (resTable.DefaultView.Find(keys) < 0)
          resTable.Rows.Add(SrcRow.ItemArray);
      }

      resTable.DefaultView.Sort = String.Empty;
      resTable.DefaultView.RowFilter = String.Empty;
      return resTable;
    }

    /// <summary>
    /// Создает копию исходной таблицы <paramref name="srcDV"/>.Table, в которую добавляются только строки,
    /// содержащие уникальные значения полей, заданных <paramref name="uniqueColumnNames"/>.
    /// Результирующая таблица имеет такую же структуру, как и исходная.
    /// Порядок строк в <paramref name="srcDV"/> определяет, какая именно строка будет добавлена в 
    /// результирующую таблицу. Добавляется первая строка, содержащая уникальные значения полей
    /// </summary>
    /// <param name="srcDV">Просмотр для исходной таблицы данных</param>
    /// <param name="uniqueColumnNames">Список имен полей, разделенных запятыми, которые должны содержать исходные значения</param>
    /// <returns>Копия таблицы, содержащая меньшее число строк</returns>
    public static DataTable CreateUniqueTable(DataView srcDV, string uniqueColumnNames)
    {
#if DEBUG
      if (String.IsNullOrEmpty(uniqueColumnNames))
        throw new ArgumentNullException("uniqueColumnNames");
#endif

      return CreateUniqueTable(srcDV, uniqueColumnNames.Split(','));
    }

    #endregion

    #region Методы работы с Generic-коллекциями

    /// <summary>
    /// Получить массив ключей из коллекции.
    /// Этот метод нельзя применять к асинхронным коллекциям.
    /// </summary>
    /// <typeparam name="TKey">Тип ключей в коллекции</typeparam>
    /// <typeparam name="TValue">Тип значений в коллекции</typeparam>
    /// <param name="source">Исходная коллекция. Если null, то возвращается пустой массив.</param>
    /// <returns>Массив ключей</returns>
    public static TKey[] GetKeys<TKey, TValue>(Dictionary<TKey, TValue> source)
    {
      if (source == null)
        return new TKey[0];
      TKey[] res = new TKey[source.Count];
      source.Keys.CopyTo(res, 0);
      return res;
    }

    /// <summary>
    /// Получить массив значений, хранящихся в коллекции.
    /// Этот метод нельзя применять к асинхронным коллекциям.
    /// </summary>
    /// <typeparam name="TKey">Тип ключей в коллекции</typeparam>
    /// <typeparam name="TValue">Тип значений в коллекции</typeparam>
    /// <param name="source">Исходная коллекция. Если null, то возвращается пустой массив.</param>
    /// <returns>Массив значений</returns>
    public static TValue[] GetValues<TKey, TValue>(Dictionary<TKey, TValue> source)
    {
      if (source == null)
        return new TValue[0];
      TValue[] res = new TValue[source.Count];
      source.Values.CopyTo(res, 0);
      return res;
    }

    #endregion

    #region Вычисление ХЭШ-сумм

    /// <summary>
    /// Получение хэш-суммы массива байтов по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами
    /// Если bytes=null, то возвращается хэш-сумма для массива нулевой длины
    /// </summary>
    /// <param name="bytes">Исходный массив байт</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(byte[] bytes)
    {
      if (bytes == null)
        bytes = DataTools.EmptyBytes;
      MD5 md5Hasher = MD5.Create();
      byte[] HashRes = md5Hasher.ComputeHash(bytes);
      return DataTools.BytesToHex(HashRes, false);
    }

    /// <summary>
    /// Получение суммы MD5 для строки.
    /// Строка представляется в кодировке Unicode.
    /// </summary>
    /// <param name="s">Строка, для которой вычисляется сумма. Может быть пустой строкой.
    /// Null считается пустой строкой</param>
    /// <returns>Сумма MD5</returns>
    public static string MD5SumFromString(string s)
    {
      if (s == null)
        s = String.Empty;
      byte[] b = Encoding.Unicode.GetBytes(s);
      return MD5Sum(b);
    }

    #endregion

    #region Прочие функции

    /// <summary>
    /// Возвращает true, если <paramref name="testValue"/> содержит пустое значение в смысле
    /// записи в базу данных, то есть пустую строку, число 0, false, и.т.д.
    /// Для массивов возвращает true, если длина массива равна 0.
    /// </summary>
    /// <param name="testValue"></param>
    /// <returns>True, если значение пустое</returns>
    public static bool IsEmptyValue(object testValue)
    {
      if (testValue == null)
        return true;
      if (testValue is DBNull)
        return true;

      // 13.08.2018
      if (testValue is Array)
        return ((Array)testValue).Length == 0;

      // 17.04.2015
      object emptyValue = GetEmptyValue(testValue.GetType());
      return testValue.Equals(emptyValue);

      /*

      if (value is String)
        return ((string)value).Length == 0;

      if (value is SByte)
        return ((SByte)value) == 0;
      if (value is Byte)
        return ((Byte)value) == 0;

      if (value is Int16)
        return ((Int16)value) == 0;
      if (value is UInt16)
        return ((UInt16)value) == 0;

      if (value is Int32)
        return ((Int32)value) == 0;
      if (value is UInt32)
        return ((UInt32)value) == 0;

      if (value is Int64)
        return ((Int64)value) == 0;
      if (value is UInt64)
        return ((UInt64)value) == 0;

      if (value is Single)
        return ((Single)value) == 0f;
      if (value is Double)
        return ((Double)value) == 0.0;
      if (value is Decimal)
        return ((Decimal)value) == 0m;

      if (value is DateTime)
        return (DateTime)value == new DateTime(); // 06.04.2015

      if (value is TimeSpan)
        return ((TimeSpan)value) == TimeSpan.Zero; // 06.04.2015

      if (value is Boolean)
        return !((Boolean)value);

      throw new BugException("Неизвестный тип: " + value.GetType().ToString());
       * */
    }

    /// <summary>
    /// Получить пустое значение указанного типа.
    /// Для строк возвращается пустая строка, для числовых значений возвращается 0
    /// </summary>
    /// <param name="typ"></param>
    /// <returns>Пустое значение</returns>
    public static object GetEmptyValue(Type typ)
    {
      if (typ == null)
        return null;
      if (typ == typeof(string))
        return String.Empty;

      if (typ == typeof(DBNull))
        return DBNull.Value;

      return Activator.CreateInstance(typ); // 17.04.2015
      /*
      ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
      if (ci == null)
        throw new BugException("Для типа " + type.ToString() + " не найден конструктор по умолчанию");
      return ci.Invoke(null);
       * */
    }


    /// <summary>
    /// Возвращает true, если все указанные поля (типа Decimal) имеют значение 0
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <param name="columnNames">Список имен полей, разделенных запятыми</param>
    /// <returns>true, если все указанные поля имеют значение 0m или DBNull</returns>
    public static bool AreAllDecimalZeros(DataRow row, string columnNames)
    {
      string[] aNames = columnNames.Split(',');
      for (int i = 0; i < aNames.Length; i++)
      {
        if (GetDecimal(row, aNames[i]) != 0m)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если все поля типа Decimal в строке имеют значение 0 или DBNull.Value
    /// Перебираются все поля в таблице, имеющие тип decimal
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>true, если все указанные поля имеют значение 0m или DBNull</returns>
    public static bool AreAllDecimalZeros(DataRow row)
    {
      for (int i = 0; i < row.Table.Columns.Count; i++)
      {
        if (row.Table.Columns[i].DataType == typeof(decimal))
        {
          if (row.IsNull(i))
            continue;
          if ((decimal)(row[i]) != 0m)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если значения заданных полей типа Decimal во всех строках содержат DBNull или 0.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnNames">Имена полей, разделенных запятыми</param>
    /// <returns>true, если все нули</returns>
    public static bool AreAllDecimalZeros(DataTable table, string columnNames)
    {
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (!AreAllDecimalZeros(row, columnNames))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если значения полей типа Decimal во всех строках содержат DBNull или 0.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <returns>true, если все нули</returns>
    public static bool AreAllDecimalZeros(DataTable table)
    {
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (!AreAllDecimalZeros(row))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если значения заданных полей типа Decimal во всех строках содержат DBNull или 0.
    /// </summary>
    /// <param name="dv">Объект DataView</param>
    /// <param name="columnNames">Имена полей, разделенных запятыми</param>
    /// <returns>true, если все нули</returns>
    public static bool AreAllDecimalZeros(DataView dv, string columnNames)
    {
      foreach (DataRowView drv in dv)
      {
        if (!AreAllDecimalZeros(drv.Row, columnNames))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если значения полей типа Decimal во всех строках содержат DBNull или 0.
    /// </summary>
    /// <param name="dv">Объект DataView</param>
    /// <returns>true, если все нули</returns>
    public static bool AreAllDecimalZeros(DataView dv)
    {
      foreach (DataRowView drv in dv)
      {
        if (!AreAllDecimalZeros(drv.Row))
          return false;
      }
      return true;
    }


    /// <summary>
    /// Возвращает минимальное и максимальное значение для заданного перечисления.
    /// Возвращаемое значение может не иметь смысла, если у перечисления задан атрибут FlagsAtribute.
    /// Используется вызов Enum.GetValues()
    /// </summary>
    /// <param name="enumTyp">Тип перечисления</param>
    /// <returns>Минимальное и максимальное значения</returns>
    public static MinMax<Int32> GetEnumRange(Type enumTyp)
    {
      Array a = Enum.GetValues(enumTyp);

      MinMax<Int32> mm = new MinMax<Int32>();
      for (int i = 0; i < a.Length; i++)
        mm += (int)(a.GetValue(i));
      return mm;
    }

    /// <summary>
    /// "Классическая" функция обмена двух значений по ссылке
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="refValue1">Ссылка на первый объект или значение</param>
    /// <param name="refValue2">Ссылка на второй объект или значение</param>
    public static void Swap<T>(ref T refValue1, ref T refValue2)
    {
      T V3 = refValue1;
      refValue1 = refValue2;
      refValue2 = V3;
    }

    #endregion

    #region Эксперимент
#if XXX
    public static DataTable SimpleCloneTable(DataTable SrcTable)
    {
      DataTable ResTable = new DataTable();
      for (int i = 0; i < SrcTable.Columns.Count; i++)
      {
        DataColumn SrcCol = SrcTable.Columns[i];
        DataColumn ResCol = new DataColumn(SrcCol.ColumnName, SrcCol.DataType);
        ResTable.Columns.Add(ResCol);
      }
      return ResTable;
    }
#endif

    #endregion
  }

#if XXX //Пока не знаю, надо ли
  public sealed class ReetranceLock
  {
    internal bool Entered;
  }

  public struct ReetranceLockKey : IDisposable
  {
    private static readonly object _SyncRoot = new object();

    private ReetranceLock _Lock;

    public ReetranceLockKey(ref ReetranceLock lockRef)
    {
      if (lockRef == null)
      {
        lock (_SyncRoot)
        {
          if (lockRef == null)
            lockRef = new ReetranceLock();
        }
      }

      System.Threading.Monitor.Enter(lockRef);
      try
      {
        if (lockRef.Entered)
          throw new ReenteranceException();
      }
      catch
      {
        System.Threading.Monitor.Exit(lockRef);
        throw;
      }
      lockRef.Entered=true;
      _Lock = lockRef;
    }

    public void Dispose()
    {
      if (_Lock != null)
      {
        _Lock.Entered = false;
        System.Threading.Monitor.Exit(_Lock);
        _Lock = null;
      }
    }
  }
#endif
}
