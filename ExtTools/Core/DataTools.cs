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


namespace FreeLibSet.Core
{
  /// <summary>
  /// Вспомогательные функции при работе с данными
  /// </summary>
  public static partial class DataTools
  {
    #region Преобразование типов

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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return 0;
        else
          return int.Parse((String)value);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return null;
        else
          return int.Parse((String)value);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return 0L;
        else
          return long.Parse((String)value);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return null;
        else
          return long.Parse((String)value);
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

      if (value is String)
      {
        string s = (String)value;
        if (s.Length == 0)
          return 0m;
        else
          return Decimal.Parse(s);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return null;
        else
          return Decimal.Parse((String)value);
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

      if (value is String)
      {
        string s = (String)value;
        if (s.Length == 0)
          return 0.0;
        else
          return Double.Parse(s);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return null;
        else
          return Double.Parse((String)value);
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

      if (value is String)
      {
        string s = (String)value;
        if (s.Length == 0)
          return 0f;
        else
          return Single.Parse(s);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return null;
        else
          return Single.Parse((String)value);
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

      if (value is String)
      {
        string s = (String)value;
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
    /// Если поле содержит DBNull, возвращается пустая строка.
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
    /// Обычный вызов Object.ToString() за исключением:
    /// null преобразуется в пустую строку.
    /// DBNull преобразуется в пустую строку.
    /// Строковый тип обрезается вызовом String.Trim().
    /// Обычно следует использовать методы Object.ToString() или Convert.ToString(Object).
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Строкое значение</returns>
    public static string GetString(object value)
    {
      if (value == null)
        return String.Empty;
      if (value is DBNull)
        return String.Empty;
      if (value is String)
        return ((string)value).Trim();
      return value.ToString();
    }

    #endregion

    #region GetDateTime()

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
      if (value is String)
      {
        string s = (string)value;
        if (s.Length == 0)
          return new DateTime();
        else
          return Convert.ToDateTime(s);
      }
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
        return (DateTime)(row[columnName]);

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
      if (value is String)
      {
        string s = (string)value;
        if (s.Length == 0)
          return null;
        else
          return Convert.ToDateTime(s);
      }
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
        return (TimeSpan)(row[columnName]);

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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return Guid.Empty;
        else
          return new Guid((String)value);
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

      if (value is String)
      {
        if (((String)value).Length == 0)
          return default(T);
        else
          return (T)Enum.Parse(typeof(T), (String)value, true);
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (String.IsNullOrEmpty(value))
      {
        if (Column.AllowDBNull)
          row[columnName] = DBNull.Value;
        else
          row[columnName] = "";
      }
      else
      {
        int l = Column.MaxLength;
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (IsIntColumn(Column))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (Column.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, value.ToString());
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (IsIntColumn(Column))
      {
        if (value == 0L)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (Column.DataType == typeof(String))
      {
        // Для текстового столбца выполняем преобразование, кроме нулевого значения
        if (value == 0)
          SetString(row, columnName, null);
        else
          SetString(row, columnName, value.ToString());
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (Column.DataType == typeof(float) || Column.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (Column.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (decimal)value;
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (Column.DataType == typeof(float) || Column.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (Column.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (decimal)value;
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
      DataColumn Column = row.Table.Columns[columnName];
#if DEBUG
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (Column.DataType == typeof(decimal))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = value;
        return;
      }
      if (Column.DataType == typeof(float) || Column.DataType == typeof(double))
      {
        if (value == 0)
        {
          if (Column.AllowDBNull)
          {
            row[columnName] = DBNull.Value;
            return;
          }
        }
        row[columnName] = (double)value;
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
      DataColumn Column = row.Table.Columns[columnName];
      if (Column == null)
        throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + columnName + "\"", "ColumnName");
#endif
      if (value.HasValue)
        row[columnName] = value.Value;
      else
        row[columnName] = DBNull.Value;
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

      if (value.Ticks == 0L)
        row[columnName] = DBNull.Value;
      else
        row[columnName] = value;
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
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");
      if (value == Guid.Empty)
        row[columnName] = DBNull.Value;
      else
      {
        if (col.DataType == typeof(Guid))
          row[columnName] = value;
        else if (col.DataType == typeof(string))
          row[columnName] = value.ToString();
        else if (col.DataType == typeof(byte[]))
          row[columnName] = value.ToByteArray();
        else
          throw new ArgumentException("Поле \"" + columnName + "\" имеет неподходящий тип " + col.DataType.ToString(), "colName");
      }
    }

    #endregion

    #endregion

    private static readonly ArrayIndexer<Type> _IntegerTypeArrayIndexer =
      new ArrayIndexer<Type>(new Type[] {
          typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16),
          typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64) });

    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из 8 
    /// целочисленных типов
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsIntegerType(Type type)
    {
      return _IntegerTypeArrayIndexer.Contains(type);
    }

    private static readonly ArrayIndexer<Type> _FloatTypeArrayIndexer =
      new ArrayIndexer<Type>(new Type[] {
          typeof(Single), typeof(Double), typeof(Decimal)});

    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из трех
    /// типов с плавающей точкой
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsFloatType(Type type)
    {
      return _FloatTypeArrayIndexer.Contains(type);
    }


    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из 11 числовых типов, 
    /// то есть IsIntegerType() или IsFloatType()
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsNumericType(Type type)
    {
      return IsIntegerType(type) || IsFloatType(type);
    }

    /// <summary>
    /// Преобразователь чисел с разделителем-точкой без разделителей групп
    /// </summary>
    public static readonly System.Globalization.NumberFormatInfo DotNumberConv = InitDotNumberConv();

    private static System.Globalization.NumberFormatInfo InitDotNumberConv()
    {
      System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";
      nfi.NumberGroupSeparator = String.Empty;
      return nfi;
    }

    #endregion

    #region Деление чисел с округлением

    /// <summary>
    /// Деление двух целых чисел.
    /// В отличие от обычного деления, округление выполняется не в сторону 0, а к ближайшему числу.
    /// </summary>
    /// <param name="x">Делимое</param>
    /// <param name="y">Делитель</param>
    /// <returns>Частное</returns>
    /// <remarks>
    /// 8/3=2
    /// DivideWithRounding(8,3)=3
    /// </remarks>
    public static int DivideWithRounding(int x, int y)
    {
      int r; // остаток (всегда имеет тот же знак, что и делимое)
      int q = Math.DivRem(x, y, out r); // q-частное

      if (r == 0)
        return q; // включает также случаи, когда x=0 или y=1

      // Нужно определить условие |r/y|>=0.5, и тогда "отодвинуть" от 0 значение q на единицу
      // Это тоже самое, что и |r|*2>=|y|, что позволяет вычислять без перехода к числам с плавающей точкой
      // Проблема: Вычисление r*2 может привести к переполнению
      // Лучше использовать условие |r|>=|y|-|r|

      if (Math.Abs(r) >= (Math.Abs(y) - Math.Abs(r)))
      {
        // Требуется изменить остаток
        if ((r > 0) ^ (y > 0)) // исключающее или
          return q - 1;
        else
          return q + 1;
      }
      else
        // Округление правильное
        return q;
    }

    /// <summary>
    /// Деление двух целых чисел типа Int64.
    /// В отличие от обычного деления, округление выполняется не в сторону 0, а к ближайшему числу.
    /// </summary>
    /// <param name="x">Делимое</param>
    /// <param name="y">Делитель</param>
    /// <returns>Частное</returns>
    public static long DivideWithRounding(long x, long y)
    {
      long r; // остаток (всегда имеет тот же знак, что и делимое)
      long q = Math.DivRem(x, y, out r); // q-частное

      if (r == 0L)
        return q; // включает также случаи, когда x=0 или y=1

      // Нужно определить условие |r/y|>=0.5, и тогда "отодвинуть" от 0 значение q на единицу
      // Это тоже самое, что и |r|*2>=|y|, что позволяет вычислять без перехода к числам с плавающей точкой
      // Проблема: Вычисление r*2 может привести к переполнению
      // Лучше использовать условие |r|>=|y|-|r|

      if (Math.Abs(r) >= (Math.Abs(y) - Math.Abs(r)))
      {
        // Требуется изменить остаток
        if ((r > 0L) ^ (y > 0L)) // исключающее или
          return q - 1L;
        else
          return q + 1L;
      }
      else
        // Округление правильное
        return q;
    }

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

    #region Суммирование полей и массивов

    #region Int

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt(DataView dv, string columnName)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int s = 0;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetInt(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt(DataTable table, string columnName)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int s = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetInt(Row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowIntExtractor Extr = new DataRowIntExtractor(columnName);
      int s = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[rows[i]]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowIntExtractor Extr = new DataRowIntExtractor(columnName);
      int s = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[Row]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumInt(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumInt(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static int SumInt(int[] a)
    {
      if (a == null)
        return 0;

      int s = 0;
      for (int i = 0; i < a.Length; i++)
        checked { s += a[i]; }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static int SumInt(int[,] a)
    {
      if (a == null)
        return 0;

      int s = 0;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          checked { s += a[i, j]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static int SumInt(int[][] a)
    {
      if (a == null)
        return 0;

      int s = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            checked { s += a[i][j]; }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static int SumInt(IEnumerable<int> items)
    {
      if (items == null)
        return 0;

      int s = 0;
      foreach (int v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(DataView dv, string columnName)
    {
      if (dv == null)
        return 0L;
      if (dv.Count == 0)
        return 0L;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long s = 0L;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetInt64(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(DataTable table, string columnName)
    {
      if (table == null)
        return 0L;
      if (table.Rows.Count == 0)
        return 0L;
      int p = GetColumnPosWithCheck(table, columnName);
      long s = 0L;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetInt64(Row[p]); }
      }
      return s;
    }


    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return 0L;
      // Строки могут относиться к разным таблицам
      DataRowInt64Extractor Extr = new DataRowInt64Extractor(columnName);
      long s = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;

        checked { s += Extr[rows[i]]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowInt64Extractor Extr = new DataRowInt64Extractor(columnName);
      long s = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[Row]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumInt64(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumInt64(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static long SumInt64(long[] a)
    {
      if (a == null)
        return 0;

      long s = 0;
      for (int i = 0; i < a.Length; i++)
        checked { s += a[i]; }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static long SumInt64(long[,] a)
    {
      if (a == null)
        return 0L;

      long s = 0;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          checked { s += a[i, j]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static long SumInt64(long[][] a)
    {
      if (a == null)
        return 0L;

      long s = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            checked { s += a[i][j]; }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static long SumInt64(IEnumerable<long> items)
    {
      if (items == null)
        return 0;

      long s = 0;
      foreach (long v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(DataView dv, string columnName)
    {
      if (dv == null)
        return 0f;
      if (dv.Count == 0)
        return 0f;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float s = 0f;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetSingle(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(DataTable table, string columnName)
    {
      if (table == null)
        return 0f;
      if (table.Rows.Count == 0)
        return 0f;
      int p = GetColumnPosWithCheck(table, columnName);
      float s = 0f;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetSingle(Row[p]); }
      }
      return s;
    }


    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return 0f;
      // Строки могут относиться к разным таблицам
      DataRowSingleExtractor Extr = new DataRowSingleExtractor(columnName);
      float s = 0f;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[rows[i]]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0f;
      // Строки могут относиться к разным таблицам
      DataRowSingleExtractor Extr = new DataRowSingleExtractor(columnName);
      float s = 0f;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[Row]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumSingle(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumSingle(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static float SumSingle(float[] a)
    {
      if (a == null)
        return 0f;

      float s = 0f;
      for (int i = 0; i < a.Length; i++)
        checked { s += a[i]; }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static float SumSingle(float[,] a)
    {
      if (a == null)
        return 0;

      float s = 0f;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          checked { s += a[i, j]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static float SumSingle(int[][] a)
    {
      if (a == null)
        return 0;

      float s = 0f;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            checked { s += a[i][j]; }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static float SumSingle(IEnumerable<float> items)
    {
      if (items == null)
        return 0f;

      float s = 0f;
      foreach (float v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(DataView dv, string columnName)
    {
      if (dv == null)
        return 0.0;
      if (dv.Count == 0)
        return 0.0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double s = 0.0;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetDouble(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(DataTable table, string columnName)
    {
      if (table == null)
        return 0.0;
      if (table.Rows.Count == 0)
        return 0.0;
      int p = GetColumnPosWithCheck(table, columnName);
      double s = 0.0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetDouble(Row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return 0.0;
      // Строки могут относиться к разным таблицам
      DataRowDoubleExtractor Extr = new DataRowDoubleExtractor(columnName);
      double s = 0.0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[rows[i]]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0.0;
      // Строки могут относиться к разным таблицам
      DataRowDoubleExtractor Extr = new DataRowDoubleExtractor(columnName);
      double s = 0.0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[Row]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumDouble(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumDouble(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static double SumDouble(double[] a)
    {
      if (a == null)
        return 0.0;

      double s = 0.0;
      for (int i = 0; i < a.Length; i++)
        checked { s += a[i]; }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static double SumDouble(double[,] a)
    {
      if (a == null)
        return 0.0;

      double s = 0.0;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          checked { s += a[i, j]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static double SumDouble(double[][] a)
    {
      if (a == null)
        return 0.0;

      double s = 0.0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            checked { s += a[i][j]; }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static double SumDouble(IEnumerable<double> items)
    {
      if (items == null)
        return 0.0;

      double s = 0;
      foreach (double v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(DataView dv, string columnName)
    {
      if (dv == null)
        return 0m;
      if (dv.Count == 0)
        return 0m;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal s = 0m;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetDecimal(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(DataTable table, string columnName)
    {
      if (table == null)
        return 0m;
      if (table.Rows.Count == 0)
        return 0m;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal s = 0m;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetDecimal(Row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return 0m;
      // Строки могут относиться к разным таблицам
      DataRowDecimalExtractor Extr = new DataRowDecimalExtractor(columnName);
      decimal s = 0m;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[rows[i]]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0m;
      // Строки могут относиться к разным таблицам
      DataRowDecimalExtractor Extr = new DataRowDecimalExtractor(columnName);
      decimal s = 0m;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        checked { s += Extr[Row]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumDecimal(DataRow row, string columnName)
    {
      row[columnName] = SumDecimal(row.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static decimal SumDecimal(decimal[] a)
    {
      if (a == null)
        return 0m;

      decimal s = 0m;
      for (int i = 0; i < a.Length; i++)
        checked { s += a[i]; }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static decimal SumDecimal(decimal[,] a)
    {
      if (a == null)
        return 0m;

      decimal s = 0m;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          checked { s += a[i, j]; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static decimal SumDecimal(decimal[][] a)
    {
      if (a == null)
        return 0m;

      decimal s = 0m;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            checked { s += a[i][j]; }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static decimal SumDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return 0m;

      decimal s = 0;
      foreach (decimal v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(DataView dv, string columnName)
    {
      if (dv == null)
        return TimeSpan.Zero;
      if (dv.Count == 0)
        return TimeSpan.Zero;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRowView drv in dv)
        s += DataTools.GetTimeSpan(drv.Row[p]);
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(DataTable table, string columnName)
    {
      if (table == null)
        return TimeSpan.Zero;
      if (table.Rows.Count == 0)
        return TimeSpan.Zero;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        s += DataTools.GetTimeSpan(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return TimeSpan.Zero;
      // Строки могут относиться к разным таблицам
      DataRowTimeSpanExtractor Extr = new DataRowTimeSpanExtractor(columnName);
      TimeSpan s = TimeSpan.Zero;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        s += Extr[rows[i]];
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return TimeSpan.Zero;
      // Строки могут относиться к разным таблицам
      DataRowTimeSpanExtractor Extr = new DataRowTimeSpanExtractor(columnName);
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        s += Extr[Row];
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumTimeSpan(DataRow row, string columnName)
    {
      row[columnName] = SumTimeSpan(row.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static TimeSpan SumTimeSpan(TimeSpan[] a)
    {
      if (a == null)
        return TimeSpan.Zero;

      TimeSpan s = TimeSpan.Zero;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return s;
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static TimeSpan SumTimeSpan(TimeSpan[,] a)
    {
      if (a == null)
        return TimeSpan.Zero;

      TimeSpan s = TimeSpan.Zero;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static TimeSpan SumTimeSpan(TimeSpan[][] a)
    {
      if (a == null)
        return TimeSpan.Zero;

      TimeSpan s = TimeSpan.Zero;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            s += a[i][j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static TimeSpan SumTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return TimeSpan.Zero;

      TimeSpan s = TimeSpan.Zero;
      foreach (TimeSpan v in items)
        s += v;
      return s;
    }

    #endregion

    #region Внутренние функции для методов XxxValue

    /// <summary>
    /// Типы для суммирования.
    /// В перечислении они располагаются в порядке "возрастания" типа
    /// </summary>
    private enum SumType { None, Int, Int64, Single, Double, Decimal, DateTime, TimeSpan };

    private static readonly Dictionary<Type, SumType> _SumTypeDict = CreateSumTypeDict();

    private static Dictionary<Type, SumType> CreateSumTypeDict()
    {
      Dictionary<Type, SumType> Dict = new Dictionary<Type, SumType>();
      Dict.Add(typeof(Byte), SumType.Int);
      Dict.Add(typeof(SByte), SumType.Int);
      Dict.Add(typeof(Int16), SumType.Int);
      Dict.Add(typeof(UInt16), SumType.Int);
      Dict.Add(typeof(Int32), SumType.Int);
      Dict.Add(typeof(UInt32), SumType.Int);
      Dict.Add(typeof(Int64), SumType.Int64);
      Dict.Add(typeof(UInt64), SumType.Int64);

      Dict.Add(typeof(Single), SumType.Single);
      Dict.Add(typeof(Double), SumType.Double);
      Dict.Add(typeof(Decimal), SumType.Decimal);
      Dict.Add(typeof(DateTime), SumType.DateTime);
      Dict.Add(typeof(TimeSpan), SumType.TimeSpan);

      return Dict;
    }

    private static SumType GetSumType(Type typ)
    {
#if DEBUG
      if (typ == null)
        throw new ArgumentNullException("typ");
#endif

      SumType Res;
      if (_SumTypeDict.TryGetValue(typ, out Res))
        return Res;
      else
        return SumType.None;
    }

    /// <summary>
    /// Получить тип данных для столбца в массиве строк.
    /// Извлекается DataType для столбца в первой строке массива, в которой хранится не ссылка null.
    /// Если массив пустой или содержит только ссылки null, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="column">Сюда помещается столбец, откуда взято описание</param>
    /// <returns>Свойство DataColumn.DataType</returns>
    private static Type GetDataTypeFromRows(DataRow[] rows, string columnName, out DataColumn column)
    {
      column = null;
      if (rows == null)
        return null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        column = rows[i].Table.Columns[columnName];
        if (column == null)
          throw new ArgumentException("Таблица " + rows[i].Table.TableName + " не содержит столбца с именем \"" + columnName + "\"", "columnName");
        return column.DataType;
      }
      return null; // Нет ни одной строки
    }

    /// <summary>
    /// Получить тип данных для столбца в массиве строк.
    /// Извлекается DataType для столбца в первой строке массива, в которой хранится не ссылка null.
    /// Если массив пустой или содержит только ссылки null, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="column">Сюда помещается столбец, откуда взято описание</param>
    /// <returns>Свойство DataColumn.DataType</returns>
    private static Type GetDataTypeFromRows(IEnumerable<DataRow> rows, string columnName, out DataColumn column)
    {
      column = null;
      if (rows == null)
        return null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        column = Row.Table.Columns[columnName];
        if (column == null)
          throw new ArgumentException("Таблица " + Row.Table.TableName + " не содержит столбца с именем \"" + columnName + "\"", "columnName");
        return column.DataType;
      }
      return null; // Нет ни одной строки
    }


    private static Exception ColumnTypeException(string message, DataColumn column)
    {
      string s = message + ". Столбец \"" + column.ColumnName + "\"";
      if (column.Table != null)
        s += " таблицы \"" + column.Table.TableName + "\"";
      s += " имеет неподходящий тип " + column.DataType.ToString();
      return new InvalidOperationException(s);
    }

    private static object ConvertValue(object v, SumType st)
    {
      switch (st)
      {
        case SumType.Int: return DataTools.GetInt(v);
        case SumType.Int64: return DataTools.GetInt64(v);
        case SumType.Single: return DataTools.GetSingle(v);
        case SumType.Double: return DataTools.GetDouble(v);
        case SumType.Decimal: return DataTools.GetDecimal(v);
        case SumType.TimeSpan: return DataTools.GetTimeSpan(v);
        case SumType.DateTime: return DataTools.GetNullableDateTime(v);
        case SumType.None: return null;
        default:
          throw new ArgumentException();
      }
    }

    /// <summary>
    /// Возвращает более общий из двух типов.
    /// Если типы не совместимы, генерируется исключение
    /// </summary>
    /// <param name="st1">Первый тип</param>
    /// <param name="st2">Второй тип</param>
    /// <returns></returns>
    private static SumType GetLargestSumType(SumType st1, SumType st2)
    {
      if (st1 == st2)
        return st1;
      if (st1 == SumType.None)
        return st2;
      if (st2 == SumType.None)
        return st1;

      if (st1 == SumType.TimeSpan || st2 == SumType.TimeSpan ||
        st1 == SumType.DateTime || st2 == SumType.DateTime)
        throw new InvalidCastException("Несовместимые типы данных");

      return (SumType)Math.Max((int)st1, (int)st2);
    }

    /// <summary>
    /// Возвращает индекс столбца в таблице.
    /// Если столбца нет, генерируется исключение
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Индекс столбца</returns>
    private static int GetColumnPosWithCheck(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      int p = table.Columns.IndexOf(columnName);
      if (p < 0)
      {
        ArgumentException e = new ArgumentException("Таблица \"" + table.TableName + "\" не содержит столбца \"" + columnName + "\"", "columnName");
        AddExceptionColumnsInfo(e, table);
        throw e;
      }
      return p;
    }


    /// <summary>
    /// Добавляет в исключение данные об имеющихся полях таблицы.
    /// </summary>
    /// <param name="e">Созданный объект исключения</param>
    /// <param name="table">Таблица данных</param>
    private static void AddExceptionColumnsInfo(ArgumentException e, DataTable table) // 15.07.2019
    {
      if (table == null)
        return;

      try
      {
        StringBuilder sb = new StringBuilder();
        sb.Append("AvailableColumns (");
        sb.Append(table.Columns.Count.ToString());
        sb.Append("): ");
        for (int i = 0; i < table.Columns.Count; i++)
        {
          if (i > 0)
            sb.Append(", ");
          sb.Append('\"');
          sb.Append(table.Columns[i].ColumnName);
          sb.Append('\"');
        }
        e.Data["Table.ColumnNamesInfo"] = sb.ToString();
      }
      catch { }
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(DataView dv, string columnName)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return SumInt(dv, columnName);
        case SumType.Int64: return SumInt64(dv, columnName);
        case SumType.Single: return SumSingle(dv, columnName);
        case SumType.Double: return SumDouble(dv, columnName);
        case SumType.Decimal: return SumDecimal(dv, columnName);
        case SumType.TimeSpan: return SumTimeSpan(dv, columnName);
        default:
          throw ColumnTypeException("Нельзя вычислить сумму значений", dv.Table.Columns[columnName]);
      }
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(DataTable table, string columnName)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");


      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return SumInt(table, columnName);
        case SumType.Int64: return SumInt64(table, columnName);
        case SumType.Single: return SumSingle(table, columnName);
        case SumType.Double: return SumDouble(table, columnName);
        case SumType.Decimal: return SumDecimal(table, columnName);
        case SumType.TimeSpan: return SumTimeSpan(table, columnName);
        default:
          throw ColumnTypeException("Нельзя вычислить сумму значений", col);
      }
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(DataRow[] rows, string columnName)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return SumInt(rows, columnName);
        case SumType.Int64: return SumInt64(rows, columnName);
        case SumType.Single: return SumSingle(rows, columnName);
        case SumType.Double: return SumDouble(rows, columnName);
        case SumType.Decimal: return SumDecimal(rows, columnName);
        case SumType.TimeSpan: return SumTimeSpan(rows, columnName);
        default:
          throw ColumnTypeException("Нельзя вычислить сумму значений", Column);
      }
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(IEnumerable<DataRow> rows, string columnName)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return SumInt(rows, columnName);
        case SumType.Int64: return SumInt64(rows, columnName);
        case SumType.Single: return SumSingle(rows, columnName);
        case SumType.Double: return SumDouble(rows, columnName);
        case SumType.Decimal: return SumDecimal(rows, columnName);
        case SumType.TimeSpan: return SumTimeSpan(rows, columnName);
        default:
          throw ColumnTypeException("Нельзя вычислить сумму значений", Column);
      }
    }


    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumValue(DataRow row, string columnName)
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
        case SumType.Int: SumInt(row, columnName); break;
        case SumType.Int64: SumInt64(row, columnName); break;
        case SumType.Single: SumSingle(row, columnName); break;
        case SumType.Double: SumDouble(row, columnName); break;
        case SumType.Decimal: SumDecimal(row, columnName); break;
        case SumType.TimeSpan: SumTimeSpan(row, columnName); break;
        default:
          throw ColumnTypeException("Нельзя вычислить сумму значений", row.Table.Columns[columnName]);
      }
    }


    /// <summary>
    /// Получить сумму элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object SumValue(object[] a)
    {
      if (a == null)
        return null;

      object res = null;
      SumType st = SumType.None;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] == null)
          continue;
        SumType st2 = GetSumType(a[i].GetType());
        st = GetLargestSumType(st, st2);
        res = ConvertValue(res, st);
        object v = ConvertValue(a[i], st);
        checked
        {
          switch (st)
          {
            case SumType.Int: res = (int)res + (int)v; break;
            case SumType.Int64: res = (long)res + (long)v; break;
            case SumType.Single: res = (float)res + (float)v; break;
            case SumType.Double: res = (double)res + (double)v; break;
            case SumType.Decimal: res = (decimal)res + (decimal)v; break;
            case SumType.TimeSpan: res = (TimeSpan)res + (TimeSpan)v; break;
            default:
              throw new ArgumentException("Значение типа " + a[i].GetType() + " не может быть просуммировано");
          }
        }
      }

      return res;
    }

#if XXX
    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="Items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object SumValue(IEnumerable<object> Items)
    {
      if (Items == null)
        return 0;

      int s = 0;
      foreach (int v in Items)
        checked { s += v; }
      return s;
    }
#endif

    #endregion

    #endregion

    #region Математические функции двух аргументов

    /// <summary>
    /// Выполнить суммирование двух аргументов
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается другой аргумент.
    /// Если оба аргумента равны null, возвращается null.
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат сложения</returns>
    public static object SumValues(object a, object b)
    {
      if (a == null)
        return b;
      if (b == null)
        return a;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());
      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return (int)a + (int)b;
          case SumType.Int64: return (long)a + (long)b;
          case SumType.Single: return (float)a + (float)b;
          case SumType.Double: return (double)a + (double)b;
          case SumType.Decimal: return (decimal)a + (decimal)b;
          case SumType.TimeSpan: return (TimeSpan)a + (TimeSpan)b;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть просуммированы");
        }
      }
    }


    /// <summary>
    /// Выполнить вычитание второго аргумента из первого
    /// Выполняется преобразование к самому большому типу.
    /// Если первый аргумент равен null, возвращается null.
    /// Если второй аргумент равен null, возвращается первый аргумент.
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат вычитания</returns>
    public static object SubstractValues(object a, object b)
    {
      if (a == null)
        return null;
      if (b == null)
        return a;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());
      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return (int)a - (int)b;
          case SumType.Int64: return (long)a - (long)b;
          case SumType.Single: return (float)a - (float)b;
          case SumType.Double: return (double)a - (double)b;
          case SumType.Decimal: return (decimal)a - (decimal)b;
          case SumType.TimeSpan: return (TimeSpan)a - (TimeSpan)b;
          case SumType.DateTime: return (DateTime)a - (DateTime)b;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут вычитаться");
        }
      }
    }


    /// <summary>
    /// Получить отрицательное значение аргумента.
    /// Если аргумент равен null, возвращается null.
    /// </summary>
    /// <param name="a">Аргумент</param>
    /// <returns>Отрицательное значение</returns>
    public static object NegValue(object a)
    {
      if (a == null)
        return null;

      SumType st = GetSumType(a.GetType());
      object v = ConvertValue(a, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return -(int)a;
          case SumType.Int64: return -(long)a;
          case SumType.Single: return -(float)a;
          case SumType.Double: return -(double)a;
          case SumType.Decimal: return -(decimal)a;
          case SumType.TimeSpan: return -(TimeSpan)a;
          default:
            throw new ArgumentException("Для значения типа " + a.GetType() + " не может быть получено отрицательное значение");
        }
      }
    }

    /// <summary>
    /// Выполнить умножение первого аргумента на второй
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается null.
    /// Если при перемножении целых чисел возникает переполнение, результат преобразуется к типу Double
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат умножения</returns>
    public static object MultiplyValues(object a, object b)
    {
      if (a == null || b == null)
        return null;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());
      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int:
            try
            {
              return (int)a * (int)b;
            }
            catch (OverflowException)
            {
              return (double)a * (double)b;
            }
          case SumType.Int64:
            try
            {
              return (long)a * (long)b;
            }
            catch (OverflowException)
            {
              return (double)a * (double)b;
            }
          case SumType.Single: return (float)a * (float)b;
          case SumType.Double: return (double)a * (double)b;
          case SumType.Decimal: return (decimal)a * (decimal)b;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть перемножены");
        }
      }
    }


    /// <summary>
    /// Выполнить деление первого аргумента на второй
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается null.
    /// При делении целых чисел, если результат не является целым, выполняется преобразование к типу Double
    /// </summary>
    /// <param name="a">Делимое</param>
    /// <param name="b">Делитель</param>
    /// <returns>Частное</returns>
    public static object DivideValues(object a, object b)
    {
      if (a == null || b == null)
        return null;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());
      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int:
            int ai = (int)a;
            int bi = (int)b;
            int di = ai / bi;
            if (di * bi == ai)
              return di;
            else
              return (double)a / (double)b;
          case SumType.Int64:
            long al = (long)a;
            long bl = (long)b;
            long dl = al / bl;
            if (dl * bl == al)
              return dl;
            else
              return (double)a / (double)b;
          case SumType.Single: return (float)a * (float)b;
          case SumType.Double: return (double)a - (double)b;
          case SumType.Decimal: return (decimal)a * (decimal)b;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть перемножены");
        }
      }
    }


    /// <summary>
    /// Получить модуль (абсолютное значение аргумента. Используется метод Math.Abs()
    /// Если аргумент равен null, возвращается null.
    /// </summary>
    /// <param name="a">Аргумент</param>
    /// <returns>Отрицательное значение</returns>
    public static object AbsValue(object a)
    {
      if (a == null)
        return null;

      SumType st = GetSumType(a.GetType());
      object v = ConvertValue(a, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return Math.Abs((int)a);
          case SumType.Int64: return Math.Abs((long)a);
          case SumType.Single: return Math.Abs((float)a);
          case SumType.Double: return Math.Abs((double)a);
          case SumType.Decimal: return Math.Abs((decimal)a);
          case SumType.TimeSpan:
            TimeSpan ts = (TimeSpan)a;
            if (ts.Ticks < 0L)
              return -ts;
            else
              return ts;
          default:
            throw new ArgumentException("Для значения типа " + a.GetType() + " не может быть получено абсолютное значение");
        }
      }
    }

    #endregion

    #region Минимальное значение

    #region Int

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt(drv.Row[p]));
        else
          s = DataTools.GetInt(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt(Row[p]));
        else
          s = DataTools.GetInt(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        int? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinInt(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = MinInt(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MinInt(int[] a)
    {
      if (a == null)
        return null;

      int? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MinInt(int[,] a)
    {
      if (a == null)
        return null;

      int? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MinInt(int[][] a)
    {
      if (a == null)
        return null;

      int? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static int? MinInt(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int? s = null;
      foreach (int Item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt64(drv.Row[p]));
        else
          s = DataTools.GetInt64(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt64(Row[p]));
        else
          s = DataTools.GetInt64(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;

        long? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinInt64(DataRow totalRow, string columnName, bool skipNulls)
    {
      long? v = MinInt64(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MinInt64(long[] a)
    {
      if (a == null)
        return null;

      long? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MinInt64(long[,] a)
    {
      if (a == null)
        return null;

      long? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MinInt64(long[][] a)
    {
      if (a == null)
        return null;

      long? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static long? MinInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long? s = null;
      foreach (long Item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetSingle(drv.Row[p]));
        else
          s = DataTools.GetSingle(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetSingle(Row[p]));
        else
          s = DataTools.GetSingle(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = MinSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MinSingle(float[] a)
    {
      if (a == null)
        return null;

      float? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MinSingle(float[,] a)
    {
      if (a == null)
        return null;

      float? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MinSingle(float[][] a)
    {
      if (a == null)
        return null;

      float? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static float? MinSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float? s = null;
      foreach (float Item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDouble(drv.Row[p]));
        else
          s = DataTools.GetDouble(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDouble(Row[p]));
        else
          s = DataTools.GetDouble(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = MinDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MinDouble(double[] a)
    {
      if (a == null)
        return null;

      double? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MinDouble(double[,] a)
    {
      if (a == null)
        return null;

      double? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MinDouble(double[][] a)
    {
      if (a == null)
        return null;

      double? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static double? MinDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double? s = null;
      foreach (double Item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDecimal(drv.Row[p]));
        else
          s = DataTools.GetDecimal(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDecimal(Row[p]));
        else
          s = DataTools.GetDecimal(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = MinDecimal(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MinDecimal(decimal[] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MinDecimal(decimal[,] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MinDecimal(decimal[][] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static decimal? MinDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal? s = null;
      foreach (decimal Item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(DataView dv, string columnName)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      DateTime? s = null;
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Min(s.Value, DataTools.GetNullableDateTime(drv.Row[p]));
        else
          s = DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(DataTable table, string columnName)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      DateTime? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Min(s.Value, DataTools.GetNullableDateTime(Row[p]));
        else
          s = DataTools.GetNullableDateTime(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      DateTime? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[rows[i]];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Min(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      DateTime? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[Row];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Min(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void MinDateTime(DataRow row, string columnName)
    {
      DateTime? v = MinDateTime(row.Table, columnName);
      if (v.HasValue)
        row[columnName] = v.Value;
      else
        row[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MinDateTime(DateTime[] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = DataTools.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MinDateTime(DateTime[,] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = DataTools.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MinDateTime(DateTime[][] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = DataTools.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static DateTime? MinDateTime(IEnumerable<DateTime> items)
    {
      if (items == null)
        return null;

      DateTime? s = null;
      foreach (DateTime Item in items)
      {
        if (s.HasValue)
          s = DataTools.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = DataTools.Min(s.Value, DataTools.GetTimeSpan(drv.Row[p]));
        else
          s = DataTools.GetTimeSpan(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = DataTools.Min(s.Value, DataTools.GetTimeSpan(Row[p]));
        else
          s = DataTools.GetTimeSpan(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = DataTools.Min(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = DataTools.Min(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = MinTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MinTimeSpan(TimeSpan[] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = DataTools.Min(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MinTimeSpan(TimeSpan[,] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = DataTools.Min(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MinTimeSpan(TimeSpan[][] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = DataTools.Min(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static TimeSpan? MinTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan? s = null;
      foreach (TimeSpan Item in items)
      {
        if (s.HasValue)
          s = DataTools.Min(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MinValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return MinInt(dv, columnName, skipNulls);
        case SumType.Int64: return MinInt64(dv, columnName, skipNulls);
        case SumType.Single: return MinSingle(dv, columnName, skipNulls);
        case SumType.Double: return MinDouble(dv, columnName, skipNulls);
        case SumType.Decimal: return MinDecimal(dv, columnName, skipNulls);
        case SumType.DateTime: return MinDateTime(dv, columnName);
        case SumType.TimeSpan: return MinTimeSpan(dv, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить минимальное значение", dv.Table.Columns[columnName]);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MinValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return MinInt(table, columnName, skipNulls);
        case SumType.Int64: return MinInt64(table, columnName, skipNulls);
        case SumType.Single: return MinSingle(table, columnName, skipNulls);
        case SumType.Double: return MinDouble(table, columnName, skipNulls);
        case SumType.Decimal: return MinDecimal(table, columnName, skipNulls);
        case SumType.DateTime: return MinDateTime(table, columnName);
        case SumType.TimeSpan: return MinTimeSpan(table, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить минимальное значение", col);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MinValue(DataRow[] rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return MinInt(rows, columnName, skipNulls);
        case SumType.Int64: return MinInt64(rows, columnName, skipNulls);
        case SumType.Single: return MinSingle(rows, columnName, skipNulls);
        case SumType.Double: return MinDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return MinDecimal(rows, columnName, skipNulls);
        case SumType.DateTime: return MinDateTime(rows, columnName);
        case SumType.TimeSpan: return MinTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить минимальное значение", Column);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static object MinValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return MinInt(rows, columnName, skipNulls);
        case SumType.Int64: return MinInt64(rows, columnName, skipNulls);
        case SumType.Single: return MinSingle(rows, columnName, skipNulls);
        case SumType.Double: return MinDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return MinDecimal(rows, columnName, skipNulls);
        case SumType.DateTime: return MinDateTime(rows, columnName);
        case SumType.TimeSpan: return MinTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить минимальное значение", Column);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка <paramref name="totalRow"/>. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: MinInt(totalRow, columnName, skipNulls); break;
        case SumType.Int64: MinInt64(totalRow, columnName, skipNulls); break;
        case SumType.Single: MinSingle(totalRow, columnName, skipNulls); break;
        case SumType.Double: MinDouble(totalRow, columnName, skipNulls); break;
        case SumType.Decimal: MinDecimal(totalRow, columnName, skipNulls); break;
        case SumType.DateTime: MinDateTime(totalRow, columnName); break;
        case SumType.TimeSpan: MinTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ColumnTypeException("Нельзя вычислить минимальное значение", totalRow.Table.Columns[columnName]);
      }
    }


    /// <summary>
    /// Получить минимальное значение среди элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object MinValue(object[] a)
    {
      if (a == null)
        return null;

      object res = null;
      SumType st = SumType.None;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] == null)
          continue;
        SumType st2 = GetSumType(a[i].GetType());
        object v = ConvertValue(a[i], st2);

        if (res == null)
        {
          st = st2;
          res = v;
        }
        else
        {
          st = GetLargestSumType(st, st2);
          res = ConvertValue(res, st);
          switch (st)
          {
            case SumType.Int: res = Math.Min((int)res, (int)v); break;
            case SumType.Int64: res = Math.Min((long)res, (long)v); break;
            case SumType.Single: res = Math.Min((float)res, (float)v); break;
            case SumType.Double: res = Math.Min((double)res, (double)v); break;
            case SumType.Decimal: res = Math.Min((decimal)res, (decimal)v); break;
            case SumType.DateTime: res = DataTools.Min((DateTime)res, (DateTime)v); break;
            case SumType.TimeSpan: res = DataTools.Min((TimeSpan)res, (TimeSpan)v); break;
            default:
              throw new ArgumentException("Значение типа " + a[i].GetType() + " не может быть просуммировано");
          }
        }
      }

      return res;
    }

    #endregion

    #endregion

    #region Максимальное значение

    #region Int

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt(drv.Row[p]));
        else
          s = DataTools.GetInt(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (skipNulls)
        {
          if (Row.RowState == DataRowState.Deleted)
            continue;
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt(Row[p]));
        else
          s = DataTools.GetInt(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxInt(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = MaxInt(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MaxInt(int[] a)
    {
      if (a == null)
        return null;

      int? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MaxInt(int[,] a)
    {
      if (a == null)
        return null;

      int? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? MaxInt(int[][] a)
    {
      if (a == null)
        return null;

      int? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static int? MaxInt(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int? s = null;
      foreach (int Item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt64(drv.Row[p]));
        else
          s = DataTools.GetInt64(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt64(Row[p]));
        else
          s = DataTools.GetInt64(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxInt64(DataRow row, string columnName, bool skipNulls)
    {
      long? v = MaxInt64(row.Table, columnName, skipNulls);
      if (v.HasValue)
        row[columnName] = v.Value;
      else
        row[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MaxInt64(long[] a)
    {
      if (a == null)
        return null;

      long? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MaxInt64(long[,] a)
    {
      if (a == null)
        return null;

      long? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? MaxInt64(long[][] a)
    {
      if (a == null)
        return null;

      long? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static long? MaxInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long? s = null;
      foreach (long Item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetSingle(drv.Row[p]));
        else
          s = DataTools.GetSingle(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetSingle(Row[p]));
        else
          s = DataTools.GetSingle(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = MaxSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MaxSingle(float[] a)
    {
      if (a == null)
        return null;

      float? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MaxSingle(float[,] a)
    {
      if (a == null)
        return null;

      float? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? MaxSingle(float[][] a)
    {
      if (a == null)
        return null;

      float? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static float? MaxSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float? s = null;
      foreach (float Item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDouble(drv.Row[p]));
        else
          s = DataTools.GetDouble(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDouble(Row[p]));
        else
          s = DataTools.GetDouble(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = MaxDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MaxDouble(double[] a)
    {
      if (a == null)
        return null;

      double? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MaxDouble(double[,] a)
    {
      if (a == null)
        return null;

      double? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? MaxDouble(double[][] a)
    {
      if (a == null)
        return null;

      double? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static double? MaxDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double? s = null;
      foreach (double Item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDecimal(drv.Row[p]));
        else
          s = DataTools.GetDecimal(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDecimal(Row[p]));
        else
          s = DataTools.GetDecimal(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = MaxDecimal(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MaxDecimal(decimal[] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MaxDecimal(decimal[,] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = Math.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? MaxDecimal(decimal[][] a)
    {
      if (a == null)
        return null;

      decimal? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = Math.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static decimal? MaxDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal? s = null;
      foreach (decimal Item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(DataView dv, string columnName)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      DateTime? s = null;
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Max(s.Value, DataTools.GetNullableDateTime(drv.Row[p]));
        else
          s = DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(DataTable table, string columnName)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      DateTime? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Max(s.Value, DataTools.GetNullableDateTime(Row[p]));
        else
          s = DataTools.GetNullableDateTime(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      DateTime? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[rows[i]];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Max(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      DateTime? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[Row];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = DataTools.Max(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void MaxDateTime(DataRow totalRow, string columnName)
    {
      DateTime? v = MaxDateTime(totalRow.Table, columnName);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MaxDateTime(DateTime[] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = DataTools.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MaxDateTime(DateTime[,] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = DataTools.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static DateTime? MaxDateTime(DateTime[][] a)
    {
      if (a == null)
        return null;

      DateTime? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = DataTools.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static DateTime? MaxDateTime(IEnumerable<DateTime> items)
    {
      if (items == null)
        return null;

      DateTime? s = null;
      foreach (DateTime Item in items)
      {
        if (s.HasValue)
          s = DataTools.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = DataTools.Max(s.Value, DataTools.GetTimeSpan(drv.Row[p]));
        else
          s = DataTools.GetTimeSpan(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan? s = null;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = DataTools.Max(s.Value, DataTools.GetTimeSpan(Row[p]));
        else
          s = DataTools.GetTimeSpan(Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan? s = null;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = DataTools.Max(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan? s = null;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = DataTools.Max(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = MaxTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MaxTimeSpan(TimeSpan[] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (s.HasValue)
          s = DataTools.Max(s.Value, a[i]);
        else
          s = a[i];
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MaxTimeSpan(TimeSpan[,] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
        {
          if (s.HasValue)
            s = DataTools.Max(s.Value, a[i, j]);
          else
            s = a[i, j];
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? MaxTimeSpan(TimeSpan[][] a)
    {
      if (a == null)
        return null;

      TimeSpan? s = null;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            if (s.HasValue)
              s = DataTools.Max(s.Value, a[i][j]);
            else
              s = a[i][j];
          }
        }
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static TimeSpan? MaxTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan? s = null;
      foreach (TimeSpan Item in items)
      {
        if (s.HasValue)
          s = DataTools.Max(s.Value, Item);
        else
          s = Item;
      }
      return s;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MaxValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return MaxInt(dv, columnName, skipNulls);
        case SumType.Int64: return MaxInt64(dv, columnName, skipNulls);
        case SumType.Single: return MaxSingle(dv, columnName, skipNulls);
        case SumType.Double: return MaxDouble(dv, columnName, skipNulls);
        case SumType.Decimal: return MaxDecimal(dv, columnName, skipNulls);
        case SumType.DateTime: return MaxDateTime(dv, columnName);
        case SumType.TimeSpan: return MaxTimeSpan(dv, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить максимальное значение", dv.Table.Columns[columnName]);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MaxValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return MaxInt(table, columnName, skipNulls);
        case SumType.Int64: return MaxInt64(table, columnName, skipNulls);
        case SumType.Single: return MaxSingle(table, columnName, skipNulls);
        case SumType.Double: return MaxDouble(table, columnName, skipNulls);
        case SumType.Decimal: return MaxDecimal(table, columnName, skipNulls);
        case SumType.DateTime: return MaxDateTime(table, columnName);
        case SumType.TimeSpan: return MaxTimeSpan(table, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить максимальное значение", col);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MaxValue(DataRow[] rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return MaxInt(rows, columnName, skipNulls);
        case SumType.Int64: return MaxInt64(rows, columnName, skipNulls);
        case SumType.Single: return MaxSingle(rows, columnName, skipNulls);
        case SumType.Double: return MaxDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return MaxDecimal(rows, columnName, skipNulls);
        case SumType.DateTime: return MaxDateTime(rows, columnName);
        case SumType.TimeSpan: return MaxTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить максимальное значение", Column);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static object MaxValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return MaxInt(rows, columnName, skipNulls);
        case SumType.Int64: return MaxInt64(rows, columnName, skipNulls);
        case SumType.Single: return MaxSingle(rows, columnName, skipNulls);
        case SumType.Double: return MaxDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return MaxDecimal(rows, columnName, skipNulls);
        case SumType.DateTime: return MaxDateTime(rows, columnName);
        case SumType.TimeSpan: return MaxTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить максимальное значение", Column);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: MaxInt(totalRow, columnName, skipNulls); break;
        case SumType.Int64: MaxInt64(totalRow, columnName, skipNulls); break;
        case SumType.Single: MaxSingle(totalRow, columnName, skipNulls); break;
        case SumType.Double: MaxDouble(totalRow, columnName, skipNulls); break;
        case SumType.Decimal: MaxDecimal(totalRow, columnName, skipNulls); break;
        case SumType.DateTime: MaxDateTime(totalRow, columnName); break;
        case SumType.TimeSpan: MaxTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ColumnTypeException("Нельзя вычислить максимальное значение", totalRow.Table.Columns[columnName]);
      }
    }

    /// <summary>
    /// Получить максимальное значение среди элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object MaxValue(object[] a)
    {
      if (a == null)
        return null;

      object res = null;
      SumType st = SumType.None;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] == null)
          continue;
        SumType st2 = GetSumType(a[i].GetType());
        object v = ConvertValue(a[i], st2);

        if (res == null)
        {
          st = st2;
          res = v;
        }
        else
        {
          st = GetLargestSumType(st, st2);
          res = ConvertValue(res, st);
          switch (st)
          {
            case SumType.Int: res = Math.Max((int)res, (int)v); break;
            case SumType.Int64: res = Math.Max((long)res, (long)v); break;
            case SumType.Single: res = Math.Max((float)res, (float)v); break;
            case SumType.Double: res = Math.Max((double)res, (double)v); break;
            case SumType.Decimal: res = Math.Max((decimal)res, (decimal)v); break;
            case SumType.DateTime: res = DataTools.Max((DateTime)res, (DateTime)v); break;
            case SumType.TimeSpan: res = DataTools.Max((TimeSpan)res, (TimeSpan)v); break;
            default:
              throw new ArgumentException("Значение типа " + a[i].GetType() + " не может быть просуммировано");
          }
        }
      }

      return res;
    }

    #endregion

    #endregion

    #region Минимальное и максимальное значение вместе

    #region Int

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxInt Res = new MinMaxInt();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetInt(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxInt Res = new MinMaxInt();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetInt(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxInt Res = new MinMaxInt();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxInt Res = new MinMaxInt();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        int? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(int[] a)
    {
      MinMaxInt Res = new MinMaxInt();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(int[,] a)
    {
      MinMaxInt Res = new MinMaxInt();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(int[][] a)
    {
      MinMaxInt Res = new MinMaxInt();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt MinMaxInt(IEnumerable<int> items)
    {
      MinMaxInt Res = new MinMaxInt();
      if (items == null)
        return Res;

      foreach (int Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetInt64(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetInt64(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0L;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        long? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0L;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt(long[] a)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(long[,] a)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(long[][] a)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxInt64 MinMaxInt64(IEnumerable<long> items)
    {
      MinMaxInt64 Res = new MinMaxInt64();
      if (items == null)
        return Res;

      foreach (long Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetSingle(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetSingle(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0f;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        float? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0f;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(float[] a)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(float[,] a)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxSingle(float[][] a)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxSingle MinMaxInt(IEnumerable<float> items)
    {
      MinMaxSingle Res = new MinMaxSingle();
      if (items == null)
        return Res;

      foreach (float Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetDouble(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetDouble(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0.0;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxInMinMaxDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        double? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0.0;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(double[] a)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(double[,] a)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(double[][] a)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDouble MinMaxDouble(IEnumerable<double> items)
    {
      MinMaxDouble Res = new MinMaxDouble();
      if (items == null)
        return Res;

      foreach (double Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetDecimal(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetDecimal(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0m;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        decimal? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? 0m;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(decimal[] a)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(decimal[,] a)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(decimal[][] a)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDecimal MinMaxDecimal(IEnumerable<decimal> items)
    {
      MinMaxDecimal Res = new MinMaxDecimal();
      if (items == null)
        return Res;

      foreach (decimal Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(DataView dv, string columnName)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        Res += DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(DataTable table, string columnName)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        Res += DataTools.GetNullableDateTime(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(DataRow[] rows, string columnName)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[rows[i]];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        Res += v;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        DateTime? v = Extr[Row];
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        Res += v;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(DateTime[] a)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(DateTime[,] a)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxInt(DateTime[][] a)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxDateTime MinMaxDateTime(IEnumerable<DateTime> items)
    {
      MinMaxDateTime Res = new MinMaxDateTime();
      if (items == null)
        return Res;

      foreach (DateTime Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (dv == null)
        return Res;
      if (dv.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetTimeSpan(drv.Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (table == null)
        return Res;
      if (table.Rows.Count == 0)
        return Res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        Res += DataTools.GetTimeSpan(Row[p]);
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(DataRow[] rows, string columnName, bool skipNulls)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? TimeSpan.Zero;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (rows == null)
        return Res;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        TimeSpan? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        Res += v ?? TimeSpan.Zero;
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов одномерного массива чисел.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxInt(TimeSpan[] a)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
        Res += a[i];
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(TimeSpan[,] a)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (a == null)
        return Res;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          Res += a[i, j];
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(TimeSpan[][] a)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (a == null)
        return Res;

      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            Res += a[i][j];
        }
      }
      return Res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Диапазон</returns>
    public static MinMaxTimeSpan MinMaxTimeSpan(IEnumerable<TimeSpan> items)
    {
      MinMaxTimeSpan Res = new MinMaxTimeSpan();
      if (items == null)
        return Res;

      foreach (TimeSpan Item in items)
        Res += Item;
      return Res;
    }

    #endregion

    #endregion

    #region Среднее значение

    #region Int

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int s = 0;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int s = 0;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0;
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      int s = 0;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0;
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageInt(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = AverageInt(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? AverageInt(int[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      int s = 0;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return DivideWithRounding(s, a.Length);
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? AverageInt(int[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      int s = 0;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return DivideWithRounding(s, cnt);
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static int? AverageInt(int[][] a)
    {
      if (a == null)
        return null;

      int s = 0;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static int? AverageInt(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int s = 0;
      int cnt = 0;
      foreach (int Item in items)
      {
        s += Item;
        cnt++;
      }
      if (cnt == 0)
        return null;
      else
        return DivideWithRounding(s, cnt);
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt64(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long s = 0;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt64(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long s = 0;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0L;
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Коллекции строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      long s = 0;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0L;
        cnt++;
      }
      if (cnt > 0)
        return DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageInt64(DataRow totalRow, string columnName, bool skipNulls)
    {
      long? v = AverageInt64(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? AverageInt64(long[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      long s = 0;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return DivideWithRounding(s, (long)(a.Length));
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? AverageInt64(long[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      long s = 0;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return DivideWithRounding(s, (long)cnt);
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static long? AverageInt64(long[][] a)
    {
      if (a == null)
        return null;

      long s = 0;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static long? AverageInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long s = 0;
      int cnt = 0;
      foreach (long Item in items)
      {
        s += Item;
        cnt++;
      }
      if (cnt == 0)
        return null;
      else
        return DivideWithRounding(s, cnt);
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetSingle(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float s = 0;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetSingle(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float s = 0;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0f;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      float s = 0;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0f;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = AverageSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? AverageSingle(float[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      float s = 0;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return s / a.Length;
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? AverageSingle(float[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      float s = 0;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return s / cnt;
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static float? AverageSingle(float[][] a)
    {
      if (a == null)
        return null;

      float s = 0;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static float? AverageSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float s = 0;
      int cnt = 0;
      foreach (float Item in items)
      {
        s += Item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDouble(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double s = 0;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDouble(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double s = 0;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0.0;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      double s = 0;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0.0;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = AverageDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? AverageDouble(double[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      double s = 0;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return s / a.Length;
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? AverageDouble(double[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      double s = 0;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return s / cnt;
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static double? AverageDouble(double[][] a)
    {
      if (a == null)
        return null;

      double s = 0;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static double? AverageDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double s = 0;
      int cnt = 0;
      foreach (double Item in items)
      {
        s += Item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDecimal(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal s = 0;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDecimal(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal s = 0;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0m;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      decimal s = 0;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0m;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = AverageInt(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? AverageDecimal(decimal[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      decimal s = 0;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return s / a.Length;
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? AverageDecimal(decimal[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      decimal s = 0;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return s / cnt;
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static decimal? AverageDecimal(decimal[][] a)
    {
      if (a == null)
        return null;

      decimal s = 0;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static decimal? AverageDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal s = 0;
      int cnt = 0;
      foreach (decimal Item in items)
      {
        s += Item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetTimeSpan(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        s += DataTools.GetTimeSpan(Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? TimeSpan.Zero;
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[Row];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? TimeSpan.Zero;
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = AverageTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? AverageTimeSpan(TimeSpan[] a)
    {
      if (a == null)
        return null;

      if (a.Length == 0)
        return null;

      TimeSpan s = TimeSpan.Zero;
      for (int i = 0; i < a.Length; i++)
        s += a[i];
      return new TimeSpan(s.Ticks / a.Length);
    }

    /// <summary>
    /// Получить среднее значение элементов двумерного массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? AverageTimeSpan(TimeSpan[,] a)
    {
      if (a == null)
        return null;

      int n1 = a.GetLength(0);
      int n2 = a.GetLength(1);
      int cnt = n1 * n2;
      if (cnt == 0)
        return null;
      TimeSpan s = TimeSpan.Zero;
      for (int i = 0; i < n1; i++)
      {
        for (int j = 0; j < n2; j++)
          s += a[i, j];
      }
      return new TimeSpan(s.Ticks / cnt);
    }

    /// <summary>
    /// Получить среднее значение элементов jagged-массива чисел
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если массив пустой</returns>
    public static TimeSpan? AverageTimeSpan(TimeSpan[][] a)
    {
      if (a == null)
        return null;

      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
          {
            s += a[i][j];
            cnt++;
          }
        }
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static TimeSpan? AverageTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (TimeSpan Item in items)
      {
        s += Item;
        cnt++;
      }

      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return AverageInt(dv, columnName, skipNulls);
        case SumType.Int64: return AverageInt64(dv, columnName, skipNulls);
        case SumType.Single: return AverageSingle(dv, columnName, skipNulls);
        case SumType.Double: return AverageDouble(dv, columnName, skipNulls);
        case SumType.Decimal: return AverageDecimal(dv, columnName, skipNulls);
        case SumType.TimeSpan: return AverageTimeSpan(dv, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить среднее значение", dv.Table.Columns[columnName]);
      }
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: return AverageInt(table, columnName, skipNulls);
        case SumType.Int64: return AverageInt64(table, columnName, skipNulls);
        case SumType.Single: return AverageSingle(table, columnName, skipNulls);
        case SumType.Double: return AverageDouble(table, columnName, skipNulls);
        case SumType.Decimal: return AverageDecimal(table, columnName, skipNulls);
        case SumType.TimeSpan: return AverageTimeSpan(table, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить среднее значение", col);
      }
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в массиве строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(DataRow[] rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return AverageInt(rows, columnName, skipNulls);
        case SumType.Int64: return AverageInt64(rows, columnName, skipNulls);
        case SumType.Single: return AverageSingle(rows, columnName, skipNulls);
        case SumType.Double: return AverageDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return AverageDecimal(rows, columnName, skipNulls);
        case SumType.TimeSpan: return AverageTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить среднее значение", Column);
      }
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn Column;
      Type t = GetDataTypeFromRows(rows, columnName, out Column);
      if (t == null)
        return null;

      switch (GetSumType(t))
      {
        case SumType.Int: return AverageInt(rows, columnName, skipNulls);
        case SumType.Int64: return AverageInt64(rows, columnName, skipNulls);
        case SumType.Single: return AverageSingle(rows, columnName, skipNulls);
        case SumType.Double: return AverageDouble(rows, columnName, skipNulls);
        case SumType.Decimal: return AverageDecimal(rows, columnName, skipNulls);
        case SumType.TimeSpan: return AverageTimeSpan(rows, columnName, skipNulls);
        default:
          throw ColumnTypeException("Нельзя вычислить среднее значение", Column);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");

      switch (GetSumType(col.DataType))
      {
        case SumType.Int: AverageInt(totalRow, columnName, skipNulls); break;
        case SumType.Int64: AverageInt64(totalRow, columnName, skipNulls); break;
        case SumType.Single: AverageSingle(totalRow, columnName, skipNulls); break;
        case SumType.Double: AverageDouble(totalRow, columnName, skipNulls); break;
        case SumType.Decimal: AverageDecimal(totalRow, columnName, skipNulls); break;
        case SumType.TimeSpan: AverageTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ColumnTypeException("Нельзя вычислить среднее значение", col);
      }
    }


    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="a">Массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object AverageValue(object[] a)
    {
      if (a == null)
        return null;

      object res = null;
      SumType st = SumType.None;
      int cnt = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] == null)
          continue;
        SumType st2 = GetSumType(a[i].GetType());
        st = GetLargestSumType(st, st2);
        res = ConvertValue(res, st);
        object v = ConvertValue(a[i], st);
        switch (st)
        {
          case SumType.Int: res = (int)res + (int)v; break;
          case SumType.Int64: res = (long)res + (long)v; break;
          case SumType.Single: res = (float)res + (float)v; break;
          case SumType.Double: res = (double)res + (double)v; break;
          case SumType.Decimal: res = (decimal)res + (decimal)v; break;
          case SumType.TimeSpan: res = (TimeSpan)res + (TimeSpan)v; break;
          default:
            throw new ArgumentException("Значение типа " + a[i].GetType() + " не может быть просуммировано");
        }
        cnt++;
      }

      if (res == null)
        return null;

      switch (st)
      {
        case SumType.Int: return DivideWithRounding((int)res, cnt);
        case SumType.Int64: return DivideWithRounding((long)res, (long)cnt);
        case SumType.Single: return (float)res / cnt;
        case SumType.Double: return (double)res / cnt;
        case SumType.Decimal: return (decimal)res / cnt;
        case SumType.TimeSpan:
          return new TimeSpan(((TimeSpan)res).Ticks / cnt);
        default:
          throw new BugException();
      }
    }

    #endregion

    #endregion

    #region Подсчет числа строк

    #region Bool

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, bool searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetBool(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, bool searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetBool(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, bool searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowBoolExtractor Extr = new DataRowBoolExtractor(columnName);

      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, bool searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowBoolExtractor Extr = new DataRowBoolExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Int

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, int searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetInt(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, int searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetInt(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, int searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowIntExtractor Extr = new DataRowIntExtractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, int searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowIntExtractor Extr = new DataRowIntExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, long searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetInt64(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, long searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetInt64(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, long searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowInt64Extractor Extr = new DataRowInt64Extractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, long searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowInt64Extractor Extr = new DataRowInt64Extractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Single

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, float searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetSingle(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, float searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetSingle(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, float searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowSingleExtractor Extr = new DataRowSingleExtractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, float searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowSingleExtractor Extr = new DataRowSingleExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Double

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, double searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetDouble(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, double searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetDouble(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, double searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowDoubleExtractor Extr = new DataRowDoubleExtractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, double searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowDoubleExtractor Extr = new DataRowDoubleExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, decimal searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetDecimal(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, decimal searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetDecimal(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, decimal searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowDecimalExtractor Extr = new DataRowDecimalExtractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, decimal searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowDecimalExtractor Extr = new DataRowDecimalExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataView dv, string columnName, TimeSpan searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetTimeSpan(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataTable table, string columnName, TimeSpan searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetTimeSpan(Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в массиве с заданным значением поля 
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(DataRow[] rows, string columnName, TimeSpan searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowTimeSpanExtractor Extr = new DataRowTimeSpanExtractor(columnName);
      int cnt = 0;
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        if (Extr[rows[i]] == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCount(IEnumerable<DataRow> rows, string columnName, TimeSpan searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowTimeSpanExtractor Extr = new DataRowTimeSpanExtractor(columnName);
      int cnt = 0;
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (Extr[Row] == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #endregion

    #region XxxInRange

    /// <summary>
    /// Возвращает true, если целое число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool IntInRange(int testValue, int? firstValue, int? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если целое число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool Int64InRange(long testValue, long? firstValue, long? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool SingleInRange(float testValue, float? firstValue, float? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool DoubleInRange(double testValue, double? firstValue, double? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool DecimalInRange(decimal testValue, decimal? firstValue, decimal? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

    // DateInRange() есть в другом месте

    /// <summary>
    /// Возвращает true, если значение времени находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Мвксимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool TimeSpanInRange(TimeSpan testValue, TimeSpan? firstValue, TimeSpan? lastValue)
    {
      if (firstValue.HasValue)
      {
        if (testValue < firstValue)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue > lastValue)
          return false;
      }
      return true;
    }

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
    /// <param name="FieldNames">Результирующий массив имен полей</param>
    /// <param name="FieldValues">Результирующий массив значений</param>
    public static void PairsToNamesAndValues(IDictionary columnNamesAndValues,
      out string[] FieldNames, out object[] FieldValues)
    {
      if (columnNamesAndValues == null)
        throw new ArgumentNullException("columnNamesAndValues");

      int n = columnNamesAndValues.Count;
      FieldNames = new string[n];
      FieldValues = new object[n];
      int i = 0;
      foreach (object Key in columnNamesAndValues.Keys)
      {
        FieldNames[i] = (string)Key;
        FieldValues[i] = columnNamesAndValues[Key];
        i++;
      }
    }

    #endregion

    #region Пустые массивы

    /// <summary>
    /// Пустой масив объектов
    /// </summary>
    public static object[] EmptyObjects = new object[0];

    /// <summary>
    /// Массив идентификаторов нулевой длины (Int32[0])
    /// </summary>
    public static readonly Int32[] EmptyIds = new Int32[0];

    /// <summary>
    /// Пустой массив строк
    /// </summary>
    public static readonly string[] EmptyStrings = new string[0];

    /// <summary>
    /// Пустой массив Int32
    /// </summary>
    public static readonly int[] EmptyInts = new int[0];

    /// <summary>
    /// Пустой массив Int64
    /// </summary>
    public static readonly long[] EmptyInt64s = new long[0];

    /// <summary>
    /// Пустой массив Boolean
    /// </summary>
    public static readonly bool[] EmptyBools = new bool[0];

    /// <summary>
    /// Пустой массив Single
    /// </summary>
    public static readonly float[] EmptySingles = new float[0];

    /// <summary>
    /// Пустой массив Double
    /// </summary>
    public static readonly double[] EmptyDoubles = new double[0];

    /// <summary>
    /// Пустой массив Decimal
    /// </summary>
    public static readonly decimal[] EmptyDecimals = new decimal[0];

    /// <summary>
    /// Пустой массив объектов DateTime
    /// </summary>
    public static readonly DateTime[] EmptyDateTimes = new DateTime[0];

    /// <summary>
    /// Пустой массив объектов TimeSpan
    /// </summary>
    public static readonly TimeSpan[] EmptyTimeSpans = new TimeSpan[0];

    /// <summary>
    /// Пустой массив байт
    /// </summary>
    public static readonly byte[] EmptyBytes = new byte[0];

    /// <summary>
    /// Пустой массив направлений сортировки
    /// </summary>
    public static readonly ListSortDirection[] EmptySortDirections = new ListSortDirection[0];

    #endregion

    #region Создание массивов

    /// <summary>
    /// Заполнение всех элементов одномерного массива одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="a">Массив</param>
    /// <param name="value">Значение, которое получит каждый элемент массива</param>
    public static void FillArray<T>(T[] a, T value)
    {
      for (int i = 0; i < a.Length; i++)
        a[i] = value;
    }

    /// <summary>
    /// Заполнение всех элементов двумерного массива одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="a">Массив</param>
    /// <param name="value">Значение, которое получит каждый элемент массива</param>
    public static void FillArray2<T>(T[,] a, T value)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);
      for (int i = n1; i <= n2; i++)
        for (int j = m1; j <= m2; j++)
          a[i, j] = value;
    }

    /// <summary>
    /// Создание массива произвольного типа, заполненного одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="n">Количество элементов, которые будут в массиве</param>
    /// <param name="value">Значение, которое будет записано во все элементы массива</param>
    /// <returns>Созданный массив</returns>
    public static T[] CreateArray<T>(int n, T value)
    {
      T[] a = new T[n];
      for (int i = 0; i < a.Length; i++)
        a[i] = value;
      return a;
    }

    // Убрано 26.07.2019
    ///// <summary>
    ///// Создание массива произвольного типа из интерфейса ICollection
    ///// </summary>
    ///// <typeparam name="T">Тип данных</typeparam>
    ///// <returns>Созданный массив</returns>
    //public static T[] CreateArray<T>(ICollection<T> Source)
    //{
    //  T[] a = new T[Source.Count];
    //  int cnt = 0;
    //  foreach (T Item in Source)
    //  {
    //    a[cnt] = Item;
    //    cnt++;
    //  }
    //  return a;
    //}

    /// <summary>
    /// Создание массива произвольного типа из интерфейса IEnumerable
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <returns>Созданный массив</returns>
    public static T[] CreateArray<T>(IEnumerable<T> source)
    {
      //int cnt = 0;
      //foreach (T Item in Source)
      //  cnt++;

      //T[] a = new T[cnt];
      //cnt = 0;
      //foreach (T Item in Source)
      //{
      //  a[cnt] = Item;
      //  cnt++;
      //}
      //return a;

      // 26.07.2019
      List<T> lst = new List<T>();
      foreach (T item in source)
        lst.Add(item);

      return lst.ToArray();
    }

    /// <summary>
    /// Получение массива, заполненного значенияи DBNull
    /// </summary>
    /// <param name="n">Требуемая длина массива</param>
    /// <returns>Массив объектов</returns>
    public static object[] CreateDBNullArray(int n)
    {
      object[] res = new object[n];
      for (int i = 0; i < n; i++)
        res[i] = DBNull.Value;
      return res;
    }

    /// <summary>
    /// Создание массива, содержащего элементы массива a, для которых установлен
    /// соответствующий флаг в массиве <paramref name="flags"/>.
    /// Применяется для извлечения выбранных идентификаторов из первоначального
    /// списка.
    /// </summary>
    /// <typeparam name="T">Тип данных исходного и конечного массивов</typeparam>
    /// <param name="a">Исходный массив, из которого берется значение</param>
    /// <param name="flags">Массив флагов. Должен иметь ту же длину, что и массив <paramref name="a"/>.</param>
    /// <returns>Массив, содержащий выбранные элементы из <paramref name="a"/></returns>
    public static T[] CreateSelectedArray<T>(T[] a, bool[] flags)
    {
      if (flags.Length != a.Length)
        throw new ArgumentException("Длина массива флагов (" + flags.Length.ToString() +
          ") не совпадает с длинной исходного массива (" + a.Length.ToString() + ")");
      // Подсчитываем число установленных флагов
      int i;
      int cnt = 0;
      for (i = 0; i < flags.Length; i++)
      {
        if (flags[i])
          cnt++;
      }

      T[] a2 = new T[cnt];
      cnt = 0;
      for (i = 0; i < flags.Length; i++)
      {
        if (flags[i])
        {
          a2[cnt] = a[i];
          cnt++;
        }
      }
      return a2;
    }

    #endregion

    #region Сравнение массивов

    /// <summary>
    /// Сравнение двух произвольных одномерных массивов
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов Equals() возвращает true.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// </summary>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual(Array a, Array b)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false; // 13.12.2016
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (!Object.Equals(a.GetValue(i), b.GetValue(i)))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Сравнение двух произвольных одномерных массивов
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов Equals() возвращает true.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// Для строковых массивов можно использовать метод AreStringArrayEqual()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массивах</typeparam>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual<T>(T[] a, T[] b)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false; // 13.12.2016
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (!Object.Equals(a[i], b[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Сравнение двух одномерных массивов строк
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов значения совпадают.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// </summary>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <param name="comparisonType">Режим сравнения строк</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual(string[] a, string[] b, StringComparison comparisonType)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false; // 13.12.2016
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (String.Equals(a[i], b[i], comparisonType))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Сравнение двух наборов числовых идентификаторов
    /// Возвращает true, если массивы полностью идентичны. Порядок элементов в массивах имеет значение.
    /// </summary>
    /// <param name="ids1">Первый массив идентификаторов</param>
    /// <param name="ids2">Второй массив идентификаторов</param>
    /// <returns>Идентичность массивов</returns>
    public static bool AreIdsEqual(Int32[] ids1, Int32[] ids2)
    {
      // TODO: Не уверен, что эта функция нужна. AreArrayEqual<Int32>() делает тоже самое

      if (ids1 == null)
        throw new ArgumentNullException("Ids1");
      if (ids2 == null)
        throw new ArgumentNullException("Ids2");
      if (ids1.Length != ids2.Length)
        return false;
      for (int i = 0; i < ids1.Length; i++)
      {
        if (ids2[i] != ids1[i])
          return false;
      }
      return true;
    }

    #endregion

    #region Объединение массивов

    /// <summary>
    /// Объединение нескольких одномерных массивов произвольного типа в один
    /// Значения никак не обрабатываются. Получается одномерный массив с длиной,
    /// равной суммарной длине исходных массивов.
    /// Этот метод также может преобразовать jagged-массив в одномерный, если проигнорировать слово params.
    /// Некоторые аргументы (элементы jagged-массива) могут иметь значение null. Они игнорируются.
    /// Если же в самих массивах есть элементы, равные null, то они добавляются в конечный массив
    /// </summary>
    /// <param name="arrays">Массивы, подлежащие объединению в один</param>
    /// <returns>Одномерный массив</returns>
    public static T[] MergeArrays<T>(params T[][] arrays)
    {
      // Длина массива
      int n = 0;
      for (int i = 0; i < arrays.Length; i++)
      {
        if (arrays[i] == null)
          continue;
        n += arrays[i].Length;
      }

      // Результат
      T[] res = new T[n];

      // Копирование элементов
      n = 0;
      for (int i = 0; i < arrays.Length; i++)
      {
        if (arrays[i] == null)
          continue;
        Array.Copy(arrays[i], 0, res, n, arrays[i].Length);
        n += arrays[i].Length;
      }
      return res;
    }

    /// <summary>
    /// Объединить два массива, отбрасывая повторы элементов.
    /// Предполагается, что исходные массивы <paramref name="array1"/> и <paramref name="array2"/> повторов не содержат.
    /// Функция симметрична относительно перестановки массивов.
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="array1">Первый массив</param>
    /// <param name="array2">Второй массив</param>
    /// <returns>Результирующий массив</returns>
    public static T[] MergeArraysOnce<T>(T[] array1, T[] array2)
    {
      List<T> ResArray = null;
      for (int i = 0; i < array2.Length; i++)
      {
        if (Array.IndexOf<T>(array1, array2[i]) < 0)
        {
          // Требуется добавить элемент из массива Array2
          if (ResArray == null)
          {
            ResArray = new List<T>(array1.Length + array2.Length);
            ResArray.AddRange(array1);
          }
          ResArray.Add(array2[i]);
        }
      }
      if (ResArray == null)
        return array1;
      else
        return ResArray.ToArray();
    }

    /// <summary>
    /// Создать массив с элементами, которые присутствуют и в массиве <paramref name="array1"/> и в <paramref name="array2"/> .
    /// Предполагается, что исходные массивы <paramref name="array1"/> и <paramref name="array2"/>  повторов не содержат.
    /// Функция симметрична относительно перестановки массивов
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="array1">Первый массив</param>
    /// <param name="array2">Второй массив</param>
    /// <returns>Результирующий массив</returns>
    public static T[] MergeArraysBoth<T>(T[] array1, T[] array2)
    {
      List<T> ResArray = null;
      for (int i = 0; i < array2.Length; i++)
      {
        if (Array.IndexOf<T>(array1, array2[i]) < 0)
        {
          // Требуется удалить элемент из массива Array1
          if (ResArray == null)
          {
            ResArray = new List<T>(array1.Length);
            ResArray.AddRange(array1);
          }
          ResArray.Remove(array2[i]);
        }
      }
      if (ResArray == null)
        return array1;
      else
        return ResArray.ToArray();
    }

    #endregion

    #region Прочие методы работы с массивами

    /// <summary>
    /// Удалить из массива <paramref name="a"/> элементы, содержащиеся в массиве <paramref name="removingArray"/>.
    /// Если в исходном массиве <paramref name="a"/> не найдено ни одного элемента, никаких действий не выполняется
    /// и ссылка на существующий массив не меняется.
    /// Таким образом, по ссылке может быть возвращена как копия массива, так и оригинал.
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="a">Массив (по ссылке) из которого удаляются элементы. Не может быть null</param>
    /// <param name="removingArray">Массив элементов, которые надо удалить</param>
    /// <returns>Массив, меньший или равный SrcArray</returns>
    public static void RemoveFromArray<T>(ref T[] a, T[] removingArray)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (removingArray == null)
        return;
      if (removingArray.Length == 0)
        return;

      List<T> ResArray = null;
      for (int i = 0; i < removingArray.Length; i++)
      {
        if (Array.IndexOf<T>(a, removingArray[i]) >= 0)
        {
          // Требуется удалить элемент из массива SrcArray
          if (ResArray == null)
          {
            ResArray = new List<T>(a.Length);
            ResArray.AddRange(a);
          }
          ResArray.Remove(removingArray[i]);
        }
      }
      if (ResArray != null)
        a = ResArray.ToArray();
    }

    /// <summary>
    /// Удалить из массива <paramref name="a"/> <paramref name="count"/> элементов, начиная с позиции <paramref name="startPosition"/>.
    /// <paramref name="startPosition"/> и <paramref name="count"/> должны находиться в пределах массива.
    /// Если <paramref name="count"/>=0, никаких действий не выполняется и ссылка <paramref name="a"/> не меняется.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве</typeparam>
    /// <param name="a">Изменяемый массив (по ссылке). Не может быть null</param>
    /// <param name="startPosition">Позиция первого удаляемого элемента.</param>
    /// <param name="count">Количество удаляемых элементов</param>
    public static void DeleteFromArray<T>(ref T[] a, int startPosition, int count)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (startPosition < 0 || startPosition > a.Length)
        throw new ArgumentOutOfRangeException("startPosition");
      if (count < 0 || (startPosition + count) > a.Length)
        throw new ArgumentOutOfRangeException("count");

      if (count == 0)
        return; // не надо ничего делать.

      T[] b = new T[a.Length - count];
      if (startPosition > 0)
        Array.Copy(a, 0, b, 0, startPosition); // часть до удаления
      if ((startPosition + count) < a.Length)
        Array.Copy(a, startPosition + count, b, startPosition, a.Length - startPosition - count); // часть после удаления

      a = b;
    }


    /// <summary>
    /// Вставить в массив <paramref name="a"/>, начиная с позиции <paramref name="startPosition"/>,
    /// все элементы из массива <paramref name="insertingArray"/>.
    /// <paramref name="startPosition"/> должна находиться в пределах массива <paramref name="a"/> или
    /// быть равна <paramref name="a"/>.Length для добавления в конец массива.
    /// Если <paramref name="insertingArray"/>=null или пустой массива, никаких действий не выполняется и ссылка <paramref name="a"/> не меняется.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве</typeparam>
    /// <param name="a">Изменяемый массив (по ссылке). Не может быть null</param>
    /// <param name="startPosition">Позиция первого удаляемого элемента.</param>
    /// <param name="insertingArray">Добавляемые элементы</param>
    public static void InsertIntoArray<T>(ref T[] a, int startPosition, T[] insertingArray)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      if (startPosition < 0 || startPosition > a.Length)
        throw new ArgumentOutOfRangeException("startPosition");

      if (insertingArray == null)
        return;
      if (insertingArray.Length == 0)
        return;

      T[] b = new T[a.Length + insertingArray.Length];
      if (startPosition > 0)
        Array.Copy(a, 0, b, 0, startPosition); // начало массива
      Array.Copy(insertingArray, 0, b, startPosition, insertingArray.Length); // вставка
      if (startPosition < a.Length)
        Array.Copy(a, startPosition, b, startPosition + insertingArray.Length, a.Length - startPosition); // конец массива

      a = b;
    }


    /// <summary>
    /// Возвращает первый элемент одномерного массива или null (значение по умолчанию для структур), если массив пустой
    /// </summary>
    /// <typeparam name="T">Тип элементов в массиве</typeparam>
    /// <param name="a">Массив</param>
    /// <returns>Первый элемент</returns>
    public static T FirstItem<T>(T[] a)
    {
      if (a == null)
        return default(T);
      if (a.Length == 0)
        return default(T);
      return a[0];
    }

    /// <summary>
    /// Возвращает последний элемент одномерного массива или null (значение по умолчанию для структур), если массив пустой
    /// </summary>
    /// <typeparam name="T">Тип элементов в массиве</typeparam>
    /// <param name="a">Массив</param>
    /// <returns>Последний элемент</returns>
    public static T LastItem<T>(T[] a)
    {
      if (a == null)
        return default(T);
      if (a.Length == 0)
        return default(T);
      return a[a.Length - 1];
    }

    /// <summary>
    /// Возвращает true, если массив <paramref name="a1"/> содержит первые элементы, совпадающие с <paramref name="a2"/> (по аналогии с методом String.StartsWith).
    /// Для сравнения используется метод Object.Equals()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массиве</typeparam>
    /// <param name="a1">Проверяемый массив</param>
    /// <param name="a2">Начальные элементы массива</param>
    /// <returns>true, если начало <paramref name="a1"/> совпадает с <paramref name="a2"/></returns>
    public static bool ArrayStartsWith<T>(T[] a1, T[] a2)
    {
      if (a2.Length > a1.Length)
        return false;

      for (int i = 0; i < a2.Length; i++)
      {
        if (!(a1[i].Equals(a2[i])))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если массив <paramref name="a1"/> содержит последние элементы, совпадающие с <paramref name="a2"/> (по аналогии с методом String.EndsWith).
    /// Для сравнения используется метод Object.Equals()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массиве</typeparam>
    /// <param name="a1">Проверяемый массив</param>
    /// <param name="a2">Последние элементы массива</param>
    /// <returns>true, если конец <paramref name="a1"/> совпадает с <paramref name="a2"/></returns>
    public static bool ArrayEndsWith<T>(T[] a1, T[] a2)
    {
      if (a2.Length > a1.Length)
        return false;

      int off1 = a1.Length - a2.Length;

      for (int i = 0; i < a2.Length; i++)
      {
        if (!(a1[i + off1].Equals(a2[i])))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Получение одномерного массива (строки) из двумерного массива.
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве (произвольный)</typeparam>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="rowIndex">Индекс строки</param>
    /// <returns>Одномерный массив</returns>
    public static T[] GetArray2Row<T>(T[,] a, int rowIndex)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (rowIndex < a.GetLowerBound(0) || rowIndex > a.GetUpperBound(0))
        throw new ArgumentOutOfRangeException("rowIndex", rowIndex, "Индекс строки должен быть в диапазоне от " +
          a.GetLowerBound(0).ToString() + " до " + a.GetUpperBound(0).ToString());
#endif

      int n1 = a.GetLowerBound(1);
      int n2 = a.GetUpperBound(1);
      T[] Res = new T[n2 - n1 + 1];
      for (int i = n1; i <= n2; i++)
        Res[i - n1] = a[rowIndex, i];
      return Res;
    }

    /// <summary>
    /// Получение одномерного массива (столбца) из двумерного массива.
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве (произвольный)</typeparam>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>Одномерный массив</returns>
    public static T[] GetArray2Column<T>(T[,] a, int columnIndex)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (columnIndex < a.GetLowerBound(1) || columnIndex > a.GetUpperBound(1))
        throw new ArgumentOutOfRangeException("columnIndex", columnIndex, "Индекс столбца должен быть в диапазоне от " +
          a.GetLowerBound(1).ToString() + " до " + a.GetUpperBound(1).ToString());
#endif

      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      T[] Res = new T[n2 - n1 + 1];
      for (int i = n1; i <= n2; i++)
        Res[i - n1] = a[i, columnIndex];
      return Res;
    }

    /// <summary>
    /// Получение одномерного массива из двумерного путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[,] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);

      T[] Res = new T[(n2 - n1 + 1) * (m2 - m1 + 1)];

      int pos = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          Res[pos] = a[i, j];
          pos++;
        }
      }
      return Res;
    }


    /// <summary>
    /// Получение одномерного массива из jagged-массива путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/> и вложенных массивов. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[][] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);

      int len = 0;
      for (int i = n1; i <= n2; i++)
      {
        if (a[i] == null)
          continue;
        len += a[i].Length;
      }

      T[] Res = new T[len];

      len = 0;
      for (int i = n1; i <= n2; i++)
      {
        if (a[i] == null)
          continue;
        Array.Copy(a[i], a[i].GetLowerBound(0), Res, len, a[i].Length);
        len += a[i].Length;
      }

      return Res;
    }

    /// <summary>
    /// Получение одномерного массива из двумерного jagged-массива путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/> и вложенных массивов. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[,][] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);

      int len = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (a[i, j] == null)
            continue;
          len += a[i, j].Length;
        }
      }

      T[] Res = new T[len];

      len = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (a[i, j] == null)
            continue;
          Array.Copy(a[i, j], a[i, j].GetLowerBound(0), Res, len, a[i, j].Length);
          len += a[i, j].Length;
        }
      }

      return Res;
    }

    /// <summary>
    /// Преобразование Jagged-массива в прямоугольный двумерный массив.
    /// Первая размерность результирующего массива (количество строк) равна длине массива <paramref name="a"/>.
    /// Вторая размерность результирующего массива (количество столбцов) определяется как максимальная длина среди вложенных массивов.
    /// Если длина вложенных массивов различается, то недостающие элементы результирующего массива будут иметь пустое значение.
    /// Ссылка на массив <paramref name="a"/> не может быть равна null, то может быть пустым массивом или содержать ссылки null вместо вложенных массивов
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="a">Jagged массив</param>
    /// <returns>Двумерный массив</returns>
    public static T[,] ToArray2<T>(T[][] a)
    {
      int m = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          m = Math.Max(m, a[i].Length);
      }

      T[,] res = new T[a.Length, m];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            res[i, j] = a[i][j];
        }
      }
      return res;
    }


    /// <summary>
    /// Получить двумерный массив из одномерного.
    /// Порядок элементов в полученных массивах порядку строк в исходном массиве
    /// Возвращается двумерный jagged-массив, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// Предполагается, что полученный массив не будет меняться. Если он содержит единственный подмассив (при <paramref name="a"/>.Length меньше или равном <paramref name="n"/> разбиение не требуется),
    /// то ссылка на подмассив совпадает с <paramref name="a"/>. В этом случае изменения в подмассиве затронут исходный массив.
    /// Если разбиение требуется только для однократного перебора элементов массива, используйте класс ArraySegmentEnumerator.
    /// Он работает быстрее, так как не выполняет копирование элементов.
    /// Если же при переборе требуется создавать подмассивы как отдельные объекты, то выгодно использовать ArrayBlockEnumerator.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Исходный одномерный массив</param>
    /// <param name="n">Длина подмассивов</param>
    /// <returns>Jagged-массив</returns>
    public static T[][] GetBlockedArray<T>(T[] a, int n)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (n < 1)
        throw new ArgumentOutOfRangeException("n");

      if (a.Length <= n)
        return new T[1][] { a };

      int NN = ((a.Length + (n - 1))) / n;

      T[][] res = new T[NN][];

      int cnt = 0;
      for (int i = 0; i < NN; i++)
      {
        if (i == (NN - 1))
          res[i] = new T[a.Length - cnt];
        else
          res[i] = new T[n];
        Array.Copy(a, cnt, res[i], 0, res[i].Length);
        cnt += n;
      }

      return res;
    }


    /// <summary>
    /// Возвращает количество элементов произвольного массива, не равных null.
    /// Может рекурсивно обрабатывать Jagged-массивы.
    /// Предполагается, что массив содержит объекты а не ValueType, иначе метод просто вернет общее количество элементов.
    /// </summary>
    /// <param name="a">Массив с произвольным числом измерений</param>
    /// <param name="proceedJaggedArrays">Если true, то при переборе элементов проверяется,
    /// не является ли элемент сам массивом. Если это вложенный (jagged) массив, то учитываются все его элементы.
    /// Если false, то вложенные массивы не определяются и считаются за один элемент</param>
    /// <returns>Количество элементов</returns>
    public static int GetArrayNotNullCount(Array a, bool proceedJaggedArrays)
    {
      int cnt = 0;
      foreach (object item in a)
      {
        if (!Object.ReferenceEquals(item, null))
        {
          if (proceedJaggedArrays)
          {
            Array a2 = item as Array;
            if (a2 != null)
              cnt += GetArrayNotNullCount(a2, true);
            else
              cnt++;
          }
          else
            cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Возвращает количество элементов произвольного массива, равных null.
    /// Может рекурсивно обрабатывать Jagged-массивы.
    /// Предполагается, что массив содержит объекты а не ValueType, иначе метод вернет 0.
    /// </summary>
    /// <param name="a">Массив с произвольным числом измерений</param>
    /// <param name="proceedJaggedArrays">Если true, то при переборе элементов проверяется,
    /// не является ли элемент сам массивом. Если это вложенный (jagged) массив, то учитываются все его элементы.
    /// Если false, то вложенные массивы считаются обычными элементами и пропускаются</param>
    /// <returns>Количество элементов</returns>
    public static int GetArrayNullCount(Array a, bool proceedJaggedArrays)
    {
      int cnt = 0;
      foreach (object item in a)
      {
        if (Object.ReferenceEquals(item, null))
          cnt++;
        else if (proceedJaggedArrays)
        {
          Array a2 = item as Array;
          if (a2 != null)
            cnt += GetArrayNullCount(a2, true);
        }
      }
      return cnt;
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
    /// <param name="comparer"></param>
    /// <returns>true, если перечислители содержат одинаковые объекты</returns>
    public static bool AreEnumerablesEqual<T>(IEnumerable<T> list1, IEnumerable<T> list2, IComparer<T> comparer)
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

            if (comparer.Compare(en1.Current, en2.Current) != 0)
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
      foreach (KeyValuePair<TKey, TValue> Pair in dict1)
      {
        TValue Value2;
        if (!dict2.TryGetValue(Pair.Key, out Value2))
          return false;

        if (!object.Equals(Pair.Value, Value2))
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
      foreach (DictionaryEntry Pair in dict1)
      {
        if (!dict2.Contains(Pair.Key))
          return false;

        object Value2 = dict2[Pair.Key];

        if (!object.Equals(Pair.Value, Value2))
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
        DataColumn[] Columns = new DataColumn[1];
        Columns[0] = table.Columns[columnNames];
        if (Columns[0] == null)
        {
          ArgumentException e = new ArgumentException("Таблица \"" + table.TableName +
            "\" не содержит столбца \"" + columnNames + "\", которое предполагалось сделать ключевым", "columnNames");
          AddExceptionColumnsInfo(e, table);
          throw e;
        }
        table.PrimaryKey = Columns;

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
      DataColumn[] Columns = new DataColumn[columnNames.Length];
      for (int i = 0; i < columnNames.Length; i++)
      {
        Columns[i] = table.Columns[columnNames[i]];
        if (Columns[i] == null)
        {
          ArgumentException e = new ArgumentException("Таблица \"" + table.TableName +
            "\" не содержит столбца \"" + columnNames[i] + "\", которое предполагалось сделать ключевым", "columnNames");
          AddExceptionColumnsInfo(e, table);
          throw e;
        }
      }
      table.PrimaryKey = Columns;
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

      object[,] KeyValues = new object[rows.Length, table.PrimaryKey.Length];
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        for (int j = 0; j < table.PrimaryKey.Length; j++)
          KeyValues[i, j] = rows[i][table.PrimaryKey[j]];
      }
      return KeyValues;
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
      object[] KeyValues = new object[table.PrimaryKey.Length];
      for (int i = 0; i < KeyValues.Length; i++)
      {
        string ColName = table.PrimaryKey[i].ColumnName;
        KeyValues[i] = row[ColName];
      }
      return KeyValues;
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

      object[] FindKeys = null;
      if (n > 1)
        FindKeys = new object[n]; // сюда будем копировать ключи для поиска

      DataRow[] Rows = new DataRow[keyValues.GetLength(0)];
      for (int i = 0; i < Rows.Length; i++)
      {
        if (keyValues[i, 0] == null)
          continue;
        if (n == 1)
          // Простой поиск по одному столбцу. Копирование не нужно
          Rows[i] = table.Rows.Find(keyValues[i, 0]);
        else
        {
          // Требуется дополнительное копирование
          for (int j = 0; j < n; j++)
            FindKeys[j] = keyValues[i, j];
          Rows[i] = table.Rows.Find(FindKeys);
        }
      }
      return Rows;
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
      DataColumn[] RefCols = new DataColumn[aColNames.Length];
      for (int i = 0; i < RefCols.Length; i++)
      {
        RefCols[i] = detailsTable.Columns[aColNames[i]];
        if (RefCols[i] == null)
          throw new InvalidOperationException("Таблица \"" + detailsTable.TableName + "\" не имеет столбца \"" +
            aColNames[i] + "\", которое предполагалось использовать для связывания");
      }
      if (RefCols.Length != masterTable.PrimaryKey.Length)
        throw new InvalidOperationException("В мастер-таблице \"" + masterTable.TableName + "\" объявлены ключевые поля \"" +
          GetPrimaryKey(masterTable) + "\" (" + masterTable.PrimaryKey.Length.ToString() +
          " шт.) Нельзя использовать для связи поля \"" + referenceColumn +
          "\" (" + RefCols.Length.ToString() + " шт). Количество полей должно быть одинаковым");

      if (String.IsNullOrEmpty(relationName))
        relationName = referenceColumn.Replace(',', '_') + "_Ref";
      DataRelation Rel = new DataRelation(relationName, masterTable.PrimaryKey, RefCols);
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

      DataTable ResTable = table.Clone();
      for (int i = 0; i < flags.Length; i++)
      {
        if (flags[i])
          ResTable.Rows.Add(table.Rows[i].ItemArray);
      }
      return ResTable;
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

      DataTable Table2 = table.Clone();
      for (int i = 0; i < ids.Length; i++)
      {
        DataRow Row1 = table.Rows.Find(ids[i]);
        if (Row1 == null)
          throw new InvalidOperationException("Для таблицы \"" + table.TableName + "\" не удалось получить строку с Id=" + ids[i].ToString());
        Table2.Rows.Add(Row1.ItemArray);
      }
      return Table2;
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
      // Сначала пытаемся проверить, не подойдет ли исходная таблица
      if (table.Rows.Count == ids.Length)
      {
        DataRowInt64Extractor extId = new DataRowInt64Extractor("Id");

        bool Good = true;
        for (int i = 0; i < ids.Length; i++)
        {
          if (extId[table.Rows[i]] != ids[i])
          {
            Good = false;
            break;
          }
        }
        if (Good)
          return table;
      }

      // Возвращаем копию таблицы
      return CloneTableForSelectedIds(table, ids);
    }

    #endregion

    #region Обработка DataTable.DateTimeMode

    /// <summary>
    /// Устанавливает для всех столбцов всех таблиц набора, имеющих тип данных DateTime, свойство
    /// DataColumn.DateTimeMode = DataSetDateTime.Unspecified, если текущим значением
    /// является DataSetDateTime.UnspecifiedLocal.
    /// Применение метода позволяет избежать ошибок передачи данных между компьютерами,
    /// если на них действуют разные часовые пояса.
    /// </summary>
    /// <param name="ds">Проверяемый набор</param>
    public static void SetUnspecifiedDateTimeMode(DataSet ds)
    {
      for (int i = 0; i < ds.Tables.Count; i++)
        SetUnspecifiedDateTimeMode(ds.Tables[i]);
    }

    /// <summary>
    /// Устанавливает для всех столбцов таблицы, имеющих тип данных DateTime, свойство
    /// DataColumn.DateTimeMode = DataSetDateTime.Unspecified, если текущим значением
    /// является DataSetDateTime.UnspecifiedLocal.
    /// Применение метода позволяет избежать ошибок передачи данных между компьютерами,
    /// если на них действуют разные часовые пояса.
    /// </summary>
    /// <param name="table">Проверяемая таблица</param>
    public static void SetUnspecifiedDateTimeMode(DataTable table)
    {
      for (int i = 0; i < table.Columns.Count; i++)
      {
        DataColumn Column = table.Columns[i];
        if (Column.DataType == typeof(DateTime) && Column.DateTimeMode == DataSetDateTime.UnspecifiedLocal)
          Column.DateTimeMode = DataSetDateTime.Unspecified;
      }
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
    /// <param name="ds">Целевой набор данных</param>
    /// <param name="table">Присоежиняемыая таблицы</param>
    public static void AddTableToDataSet(DataSet ds, DataTable table)
    {
      if (table.DataSet != null)
        table.DataSet.Tables.Remove(table);

      // 14.09.2015
      // Убираем существующую таблицу с совпадающим именем
      if (!String.IsNullOrEmpty(table.TableName))
      {
        if (ds.Tables.Contains(table.TableName))
          ds.Tables.Remove(table.TableName);
      }

      ds.Tables.Add(table);
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

    #region GetIdsFromField

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static Int32[] GetIdsFromField(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      int ColPos = GetColumnPosWithCheck(table, columnName);
      DataColumn Col = table.Columns[ColPos];

      SingleScopeList<Int32> Ids = null;
      foreach (DataRow Row in table.Rows)
      {
        Int32 Id;
        if (Row.RowState == DataRowState.Deleted)
        {
          if (Row.IsNull(Col, DataRowVersion.Original))
            continue;
          Id = (Int32)(Row[ColPos, DataRowVersion.Original]);
        }
        else
        {
          if (Row.IsNull(ColPos))
            continue;
          Id = (Int32)(Row[ColPos]);
        }
        if (Id == 0)
          continue;

        if (Ids == null)
          Ids = new SingleScopeList<Int32>();
        Ids.Add(Id);
      }

      if (Ids == null)
        return EmptyIds;
      else
        return Ids.ToArray();
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
    public static Int32[] GetIdsFromField(DataView dv, string columnName)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      if (dv.Count == 0)
        return EmptyIds;

      int ColPos = GetColumnPosWithCheck(dv.Table, columnName);
      DataColumn Col = dv.Table.Columns[ColPos];

      SingleScopeList<Int32> Ids = null;
      for (int i = 0; i < dv.Count; i++)
      {
        Int32 Id;
        if (dv[i].Row.RowState == DataRowState.Deleted)
        {
          if (dv[i].Row.IsNull(Col, DataRowVersion.Original))
            continue;
          Id = (int)(dv[i].Row[ColPos, DataRowVersion.Original]);
        }
        else
        {
          if (dv[i].Row.IsNull(ColPos))
            continue;
          Id = (int)(dv[i].Row[ColPos]);
        }
        if (Id == 0)
          continue;

        if (Ids == null)
          Ids = new SingleScopeList<Int32>();
        Ids.Add(Id);
      }

      if (Ids == null)
        return EmptyIds;
      else
        return Ids.ToArray();
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
    public static Int32[] GetIdsFromField(ICollection<DataRow> rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      SingleScopeList<Int32> Ids = null;
      int ColPos = -1;
      DataColumn Col = null;

      foreach (DataRow Row in rows)
      {
        if (ColPos < 0)
        {
          ColPos = GetColumnPosWithCheck(Row.Table, columnName);
          Col = Row.Table.Columns[ColPos];
        }

        Int32 Id;
        if (Row.RowState == DataRowState.Deleted)
        {
          if (Row.IsNull(Col, DataRowVersion.Original))
            continue;
          Id = (Int32)(Row[ColPos, DataRowVersion.Original]);
        }
        else
        {
          if (Row.IsNull(ColPos))
            continue;
          Id = (Int32)(Row[ColPos]);
        }
        if (Id == 0)
          continue;
        if (Ids == null)
          Ids = new SingleScopeList<Int32>();
        Ids.Add(Id);
      }
      if (Ids == null)
        return EmptyIds;
      else
        return Ids.ToArray();
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
    public static Int32[] GetIdsFromField(ICollection<DataRowView> rows, string columnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("Rows");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("ColumnName");
#endif

      if (rows.Count == 0)
        return EmptyIds;

      SingleScopeList<Int32> Ids = null;
      int ColPos = -1;
      DataColumn Col = null;
      foreach (DataRowView drv in rows)
      {
        if (ColPos < 0)
        {
          ColPos = GetColumnPosWithCheck(drv.Row.Table, columnName);
          Col = drv.Row.Table.Columns[ColPos];
        }

        Int32 Id;
        if (drv.Row.RowState == DataRowState.Deleted)
        {
          if (drv.Row.IsNull(Col, DataRowVersion.Original))
            continue;
          Id = (Int32)(drv.Row[ColPos, DataRowVersion.Original]);
        }
        else
        {
          if (drv.Row.IsNull(ColPos))
            continue;
          Id = (Int32)(drv.Row[ColPos]);
        }
        if (Id == 0)
          continue;
        if (Ids == null)
          Ids = new SingleScopeList<Int32>();
        Ids.Add(Id);
      }
      if (Ids == null)
        return EmptyIds;
      else
        return Ids.ToArray();
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

      int ColPos = GetColumnPosWithCheck(table, "Id");

      if (table.Rows.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[table.Rows.Count];
      for (int i = 0; i < res.Length; i++)
      {
        if (table.Rows[i].RowState == DataRowState.Deleted)
          res[i] = (Int32)(table.Rows[i][ColPos, DataRowVersion.Original]);
        else
          res[i] = (Int32)(table.Rows[i][ColPos]);
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

      int ColPos = GetColumnPosWithCheck(dv.Table, "Id");

      if (dv.Count == 0)
        return EmptyIds;

      Int32[] res = new Int32[dv.Count];
      for (int i = 0; i < res.Length; i++)
      {
        DataRow Row = dv[i].Row;
        if (Row.RowState == DataRowState.Deleted)
          res[i] = (Int32)(Row[ColPos, DataRowVersion.Original]);
        else
          res[i] = (Int32)(Row[ColPos]);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк. В отличие от GetIdsFromField()
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
      int Index = 0;
      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (Index == 0)
          ColPos = GetColumnPosWithCheck(Row.Table, "Id");

        if (Row.RowState == DataRowState.Deleted)
          res[Index] = (Int32)(Row[ColPos, DataRowVersion.Original]);
        else
          res[Index] = (Int32)(Row[ColPos]);
        Index++;
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк DataRowView. В отличие от GetIdsFromField()
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
      int Index = 0;
      int ColPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (Index == 0)
          ColPos = GetColumnPosWithCheck(drv.Row.Table, "Id");

        if (drv.Row.RowState == DataRowState.Deleted)
          res[Index] = (Int32)(drv.Row[ColPos, DataRowVersion.Original]);
        else
          res[Index] = (Int32)(drv.Row[ColPos]);
        Index++;
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
        throw new ArgumentNullException("Table");
#endif

      if (table.Rows.Count == 0)
        return 0;

      DataRow Row = table.Rows[0];
      return DataTools.GetInt(Row, "Id");
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
        throw new ArgumentNullException("Table");
#endif

      if (table.Rows.Count == 0)
        return 0;

      DataRow Row = table.Rows[table.Rows.Count - 1];
      return DataTools.GetInt(Row, "Id");
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

      DataRow Row = dv[0].Row;
      return DataTools.GetInt(Row, "Id");
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

      DataRow Row = dv[dv.Count - 1].Row;
      return DataTools.GetInt(Row, "Id");
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

      DataRow Row = rows[0];
      return DataTools.GetInt(Row, "Id");
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

      DataRow Row = rows[rows.Count - 1];
      return DataTools.GetInt(Row, "Id");
    }

    #endregion

    #region GetBlockedIds

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше N-элементов
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

      int NN = ((table.Rows.Count + (n - 1))) / n;
      Int32[][] res = new Int32[NN][];

      for (int i = 0; i < NN; i++)
      {
        if (i == (NN - 1))
          res[i] = new Int32[table.Rows.Count - (NN - 1) * n];
        else
          res[i] = new Int32[n];
      }

      for (int Index = 0; Index < table.Rows.Count; Index++)
      {
        int idx1 = Index / n;
        int idx2 = Index - (idx1 * n);

        if (table.Rows[Index].RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(table.Rows[Index][ColPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(table.Rows[Index][ColPos]);
      }
      return res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице для 
    /// строк, входящих в просмотр DataView
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше N-элементов
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

      int ColPos = GetColumnPosWithCheck(dv.Table, "Id");

      if (n < 1)
        throw new ArgumentOutOfRangeException("N", n, "N<1");

      int NN = ((dv.Count + (n - 1))) / n;

      Int32[][] res = new Int32[NN][];
      for (int i = 0; i < NN; i++)
      {
        if (i == (NN - 1))
          res[i] = new Int32[dv.Count - (NN - 1) * n];
        else
          res[i] = new Int32[n];
      }

      for (int Index = 0; Index < dv.Count; Index++)
      {
        int idx1 = Index / n;
        int idx2 = Index - (idx1 * n);
        DataRow Row = dv[Index].Row;
        if (Row.RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(Row[ColPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(Row[ColPos]);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк.
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше N-элементов
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

      int NN = ((rows.Count + (n - 1))) / n;

      Int32[][] res = new Int32[NN][];
      if (rows.Count == 0)
        return res;

      for (int i = 0; i < NN; i++)
      {
        if (i == (NN - 1))
          res[i] = new Int32[rows.Count - (NN - 1) * n];
        else
          res[i] = new Int32[n];
      }

      int Index = 0;
      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (Index == 0)
          ColPos = GetColumnPosWithCheck(Row.Table, "Id");

        int idx1 = Index / n;
        int idx2 = Index - (idx1 * n);
        if (Row.RowState == DataRowState.Deleted)
          res[idx1][idx2] = GetInt(Row[ColPos, DataRowVersion.Original]);
        else
          res[idx1][idx2] = GetInt(Row[ColPos]);
        Index++;
      }
      return res;
    }

    /// <summary>
    /// Получить двумерный массив идентификаторов из одномерного
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре
    /// Возвращается двумерный jagged-массив идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов
    /// </summary>
    /// <param name="ids">Исходный одномерный массив</param>
    /// <param name="n">Длина подмассивов</param>
    /// <returns>Jagged-массив</returns>
    public static Int32[][] GetBlockedIds(Int32[] ids, int n)
    {
      return GetBlockedArray<Int32>(ids, n);
    }

    #endregion

    #region GetIdsFromArray

    /// <summary>
    /// Получить массив идентификаторов из другого массива.
    /// Повторяющиеся и нулевые идентификаторы пропускаются
    /// </summary>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns></returns>
    public static Int32[] GetIdsFromArray(Int32[] ids)
    {
      if (ids == null || ids.Length == 0)
        return EmptyIds;

      List<Int32> Ids2 = new List<Int32>();
      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (!Ids2.Contains(ids[i]))
          Ids2.Add(ids[i]);
      }
      return Ids2.ToArray();
    }

    #endregion

    #region MergeIds

    /// <summary>
    /// Объединение нескольких массивов идентификаторов, полученных из нескольких мест, в один массив.
    /// Повторы отбрасываются.
    /// Для работы с массивами идентификаторов рекомендуется использовать класс IdList.
    /// </summary>
    /// <param name="idArrays">Объединяемые массивы идентификаторов</param>
    /// <returns>Объединенный массив</returns>
    public static Int32[] MergeIds(params Int32[][] idArrays)
    {
      if (idArrays.Length == 0)
        return EmptyIds;

      List<Int32> a = new List<int>();
      a.AddRange(idArrays[0]);
      for (int i = 1; i < idArrays.Length; i++)
      {
        for (int j = 0; j < idArrays[i].Length; j++)
        {
          Int32 Id = idArrays[i][j];
          if (!a.Contains(Id))
            a.Add(Id);
        }
      }
      return a.ToArray();
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
      DataTable Table = new DataTable();
      Table.Columns.Add("Id");
      if (ids != null)
      {
        for (int i = 0; i < ids.Length; i++)
          Table.Rows.Add(ids[i]);
      }
      return Table;
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

#if XXXXX
    /*
     * int Start=0;
     * DataRow[] Rows;
     * while(GetDataTableRowsBlock(Table, out Rows, ref Start, 100)
     * {
     *   for (int i=0; i<Rows.Length; i++)
     *   {
     *     // Действия со строками таблицы
     *   }
     * }
     */

    public static DataRow[] GetDataTableRowsBlock(DataTable Table, int Start, int Count)
#endif

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

      if (source is IEnumerable<DataRow>)
        return (IEnumerable<DataRow>)source;

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
          foreach (DataRow Row1 in ((DataTable)source).Rows)
            yield return Row1;
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
              foreach (DataRow Row2 in en2)
                yield return Row2;
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

    #region GetStringsFromField

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
    public static string[] GetStringsFromField(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      int ColPos = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<string> Values = null;
      foreach (DataRow Row in table.Rows)
      {
        string s = DataTools.GetString(Row[ColPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (Values == null)
          Values = new SingleScopeList<string>();
        if (!Values.Contains(s))
          Values.Add(s);
      }

      if (Values == null)
        return EmptyStrings;
      else
      {
        string[] a = Values.ToArray();
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
    public static string[] GetStringsFromField(DataView dv, string columnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      if (dv.Count == 0)
        return DataTools.EmptyStrings;

      int ColPos = GetColumnPosWithCheck(dv.Table, columnName);

      SingleScopeList<String> Values = null;
      for (int i = 0; i < dv.Count; i++)
      {
        string s = DataTools.GetString(dv[i].Row[ColPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (Values == null)
          Values = new SingleScopeList<string>();
        Values.Add(s);
      }

      if (Values == null)
        return EmptyStrings;
      else
      {
        string[] a = Values.ToArray();
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
    /// <param name="ColumnName">Имя поля типа String или другого типа,
    /// обрабатывемого функцией GetString()</param>
    /// <returns>Массив идентификаторов</returns>
    public static string[] GetStringsFromField(ICollection<DataRow> rows, string ColumnName)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Count == 0)
        return DataTools.EmptyStrings;

      SingleScopeList<string> Values = null;
      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (ColPos < 1)
          ColPos = GetColumnPosWithCheck(Row.Table, ColumnName);

        string s = DataTools.GetString(Row[ColPos]);
        if (String.IsNullOrEmpty(s))
          continue;

        if (Values == null)
          Values = new SingleScopeList<string>();
        Values.Add(s);
      }
      if (Values == null)
        return DataTools.EmptyStrings;
      else
      {
        string[] a = Values.ToArray();
        Array.Sort<string>(a);
        return a;
      }
    }

    #endregion

    #region GetValuesFromField

    /// <summary>
    /// Получить значения поля для всех строк таблицы в виде массива
    /// Повторы и пустые значения не отбрасываются. Количество и порядок элементов в массиве соответствуют строкам в таблице
    /// Хранящиеся в таблице значения DBNull заменяются на default
    /// Если тип массива T не совпадает с типом данных в столбце таблицы, используется метод Convert.ChangeType(). Методы типа GetInt() не применяются
    /// </summary>
    /// <typeparam name="T">Тип поля данных (один из поддерживаемых типов DataColumn.DataType)</typeparam>
    /// <param name="table">Таблица исходных данных</param>
    /// <param name="columnName">Имя столбца, из которого извлекаются значения</param>
    /// <returns>Массив значений поля</returns>
    public static T[] GetValuesFromField<T>(DataTable table, string columnName)
    {
      int FieldPos = GetColumnPosWithCheck(table, columnName);

      T[] res = new T[table.Rows.Count];

      Type ResType = typeof(T);
      if (ResType == table.Columns[FieldPos].DataType)
      {
        for (int i = 0; i < table.Rows.Count; i++)
        {
          if (!table.Rows[i].IsNull(FieldPos))
            res[i] = (T)(table.Rows[i][FieldPos]);
        }
      }
      else
      {
        for (int i = 0; i < table.Rows.Count; i++)
        {
          if (!table.Rows[i].IsNull(FieldPos))
            res[i] = (T)(Convert.ChangeType(table.Rows[i][FieldPos], ResType));
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
    public static T[] GetValuesFromField<T>(DataView dv, string columnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif
      int FieldPos = GetColumnPosWithCheck(dv.Table, columnName);

      T[] res = new T[dv.Count];
      Type ResType = typeof(T);
      if (ResType == dv.Table.Columns[FieldPos].DataType)
      {
        for (int i = 0; i < dv.Count; i++)
        {
          DataRow Row = dv[i].Row;
          if (!Row.IsNull(FieldPos))
            res[i] = (T)(Row[FieldPos]);
        }
      }
      else
      {
        for (int i = 0; i < dv.Count; i++)
        {
          DataRow Row = dv[i].Row;
          if (!Row.IsNull(FieldPos))
            res[i] = (T)(Convert.ChangeType(Row[FieldPos], ResType));
        }
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static int[] GetUniqueInts(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyInts;
      if (table.Rows.Count == 0)
        return EmptyInts;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<int> lst = new SingleScopeList<int>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int[] GetUniqueInts(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInts;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      SingleScopeList<int> lst = new SingleScopeList<int>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        int? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0);
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
    /// <returns>Найденное значение или null</returns>
    public static int[] GetUniqueInts(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInts;
      // Строки могут относиться к разным таблицам
      DataRowNullableIntExtractor Extr = new DataRowNullableIntExtractor(columnName);
      SingleScopeList<int> lst = new SingleScopeList<int>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        int? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static long[] GetUniqueInt64s(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyInt64s;
      if (table.Rows.Count == 0)
        return EmptyInt64s;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<long> lst = new SingleScopeList<long>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetInt64(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long[] GetUniqueInt64s(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInt64s;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      SingleScopeList<long> lst = new SingleScopeList<long>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        long? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0L);
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
    /// <returns>Найденное значение или null</returns>
    public static long[] GetUniqueInt64s(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyInt64s;
      // Строки могут относиться к разным таблицам
      DataRowNullableInt64Extractor Extr = new DataRowNullableInt64Extractor(columnName);
      SingleScopeList<long> lst = new SingleScopeList<long>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        long? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static float[] GetUniqueSingles(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptySingles;
      if (table.Rows.Count == 0)
        return EmptySingles;
      int p = GetColumnPosWithCheck(table, columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetSingle(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float[] GetUniqueSingles(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptySingles;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        float? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0f);
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
    /// <returns>Найденное значение или null</returns>
    public static float[] GetUniqueSingles(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptySingles;
      // Строки могут относиться к разным таблицам
      DataRowNullableSingleExtractor Extr = new DataRowNullableSingleExtractor(columnName);
      SingleScopeList<float> lst = new SingleScopeList<float>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        float? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static double[] GetUniqueDoubles(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyDoubles;
      if (table.Rows.Count == 0)
        return EmptyDoubles;
      int p = GetColumnPosWithCheck(table, columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDouble(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double[] GetUniqueDoubles(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDoubles;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        double? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0.0);
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
    /// <returns>Найденное значение или null</returns>
    public static double[] GetUniqueDoubles(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDoubles;
      // Строки могут относиться к разным таблицам
      DataRowNullableDoubleExtractor Extr = new DataRowNullableDoubleExtractor(columnName);
      SingleScopeList<double> lst = new SingleScopeList<double>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        double? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static decimal[] GetUniqueDecimals(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyDecimals;
      if (table.Rows.Count == 0)
        return EmptyDecimals;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetDecimal(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal[] GetUniqueDecimals(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDecimals;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        decimal? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? 0m);
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
    /// <returns>Найденное значение или null</returns>
    public static decimal[] GetUniqueDecimals(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyDecimals;
      // Строки могут относиться к разным таблицам
      DataRowNullableDecimalExtractor Extr = new DataRowNullableDecimalExtractor(columnName);
      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        decimal? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static DateTime[] GetUniqueDateTimes(DataTable table, string columnName)
    {
      if (table == null)
        return EmptyDateTimes;
      if (table.Rows.Count == 0)
        return EmptyDateTimes;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        DateTime? v = DataTools.GetNullableDateTime(Row[p]);
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
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime[] GetUniqueDateTimes(DataRow[] rows, string columnName)
    {
      if (rows == null)
        return EmptyDateTimes;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        DateTime? v = Extr[rows[i]];
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
    /// <returns>Найденное значение или null</returns>
    public static DateTime[] GetUniqueDateTimes(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return EmptyDateTimes;
      // Строки могут относиться к разным таблицам
      DataRowNullableDateTimeExtractor Extr = new DataRowNullableDateTimeExtractor(columnName);
      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        DateTime? v = Extr[Row];
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
    /// <returns>Найденное значение или null</returns>
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
    /// <returns>Массив уникальных значений</returns>
    public static TimeSpan[] GetUniqueTimeSpans(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return EmptyTimeSpans;
      if (table.Rows.Count == 0)
        return EmptyTimeSpans;
      int p = GetColumnPosWithCheck(table, columnName);

      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        lst.Add(DataTools.GetTimeSpan(Row[p]));
      }

      return lst.ToArray();
    }

    /// <summary>
    /// Получить все уникальные значения для одного поля таблицы.
    /// Возвращаемый массив не является отсортированным.
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan[] GetUniqueTimeSpans(DataRow[] rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyTimeSpans;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          continue;
        if (rows[i].RowState == DataRowState.Deleted)
          continue;
        TimeSpan? v = Extr[rows[i]];
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        lst.Add(v ?? TimeSpan.Zero);
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
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan[] GetUniqueTimeSpans(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return EmptyTimeSpans;
      // Строки могут относиться к разным таблицам
      DataRowNullableTimeSpanExtractor Extr = new DataRowNullableTimeSpanExtractor(columnName);
      SingleScopeList<TimeSpan> lst = new SingleScopeList<TimeSpan>();
      foreach (DataRow Row in rows)
      {
        if (Row == null)
          continue;
        if (Row.RowState == DataRowState.Deleted)
          continue;

        TimeSpan? v = Extr[Row];
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

    ///// <summary>
    ///// Получить все уникальные значения для одного поля таблицы.
    ///// В список входят все варианты значения, включая DBNull
    ///// </summary>
    ///// <param name="Table">Таблица</param>
    ///// <param name="FieldName">Имя поля</param>
    ///// <returns></returns>
    //public object[] GetUniqueValues(DataTable Table, string FieldName)
    //{ 
    //}

    #endregion

    #region IsIntColumn

    /// <summary>
    /// Возвращает true, если столбец предназначен для хранения целых чисел
    /// (8,16,32 или-битных, со знаком или без него) 
    /// </summary>
    /// <param name="column">Столбец таблицы</param>
    /// <returns>true, если Целочисленный столбец</returns>
    public static bool IsIntColumn(DataColumn column)
    {
      return IsIntegerType(column.DataType);
    }

    #endregion

    #region GetColumnNames, GetTableNames

    /// <summary>
    /// Получить список имен столбцов в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Массив имен</returns>
    public static string[] GetColumnNames(DataTable table)
    {
      string[] Names = new string[table.Columns.Count];
      for (int i = 0; i < Names.Length; i++)
        Names[i] = table.Columns[i].ColumnName;
      return Names;
    }

    /// <summary>
    /// Получить список имен таблиц в наборе DataSet
    /// </summary>
    /// <param name="ds">Набор данных</param>
    /// <returns>Массив имен таблиц</returns>
    public static string[] GetTableNames(DataSet ds)
    {
      string[] Names = new string[ds.Tables.Count];
      for (int i = 0; i < Names.Length; i++)
        Names[i] = ds.Tables[i].TableName;
      return Names;
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

      int MainColumnPos = GetColumnPosWithCheck(table, mainColumnName);

      int BeginFlagColumnPos, EndFlagColumnPos;

      if (String.IsNullOrEmpty(beginFlagColumnName))
        BeginFlagColumnPos = -1;
      else
      {
        BeginFlagColumnPos = GetColumnPosWithCheck(table, beginFlagColumnName);
#if DEBUG
        if (table.Columns[BeginFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + beginFlagColumnName + "\" таблицы \"" + table.TableName + "\" имеет тип \"" + table.Columns[BeginFlagColumnPos].DataType.ToString() + "\", а не логический", "beginFlagColumnName");
#endif
      }

      if (String.IsNullOrEmpty(endFlagColumnName))
        EndFlagColumnPos = -1;
      else
      {
        EndFlagColumnPos = GetColumnPosWithCheck(table, endFlagColumnName);
#if DEBUG
        if (table.Columns[EndFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + endFlagColumnName + "\" таблицы \"" + table.TableName + "\" имеет тип \"" + table.Columns[EndFlagColumnPos].DataType.ToString() + "\", а не логический", "endFlagColumnName");
#endif
      }

      if (table.Rows.Count == 0)
        return;

      // Перебор строк
      for (int i = 1; i < table.Rows.Count; i++)
      {
        DataRow PrevRow = table.Rows[i - 1];
        DataRow ThisRow = table.Rows[i];
        bool Flag = !AreValuesEqual(PrevRow, ThisRow, MainColumnPos);
        if (BeginFlagColumnPos >= 0)
          ThisRow[BeginFlagColumnPos] = Flag;
        if (EndFlagColumnPos >= 0)
          PrevRow[EndFlagColumnPos] = Flag;
      }
      // Для первой и последней строки флаги устанавливаются принудительно
      if (BeginFlagColumnPos >= 0)
        table.Rows[0][BeginFlagColumnPos] = true;
      if (EndFlagColumnPos >= 0)
        table.Rows[table.Rows.Count - 1][EndFlagColumnPos] = true;
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

      int MainColumnPos = GetColumnPosWithCheck(dv.Table, mainColumnName);

      int BeginFlagColumnPos, EndFlagColumnPos;

      if (String.IsNullOrEmpty(beginFlagColumnName))
        BeginFlagColumnPos = -1;
      else
      {
        BeginFlagColumnPos = GetColumnPosWithCheck(dv.Table, beginFlagColumnName);
#if DEBUG
        if (dv.Table.Columns[BeginFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + beginFlagColumnName + "\" таблицы \"" + dv.Table.TableName + "\" имеет тип \"" + dv.Table.Columns[BeginFlagColumnPos].DataType.ToString() + "\", а не логический", "beginFlagColumnName");
#endif
      }

      if (String.IsNullOrEmpty(endFlagColumnName))
        EndFlagColumnPos = -1;
      else
      {
        EndFlagColumnPos = GetColumnPosWithCheck(dv.Table, endFlagColumnName);
#if DEBUG
        if (dv.Table.Columns[EndFlagColumnPos].DataType != typeof(bool))
          throw new ArgumentException("Столбец \"" + endFlagColumnName + "\" таблицы \"" + dv.Table.TableName + "\" имеет тип \"" + dv.Table.Columns[EndFlagColumnPos].DataType.ToString() + "\", а не логический", "endFlagColumnName");
#endif
      }

      if (dv.Count == 0)
        return;

      // Перебор строк
      for (int i = 1; i < dv.Count; i++)
      {
        DataRowView PrevRow = dv[i - 1];
        DataRowView ThisRow = dv[i];
        bool Flag = !AreValuesEqual(PrevRow, ThisRow, MainColumnPos);
        if (BeginFlagColumnPos >= 0)
          ThisRow[BeginFlagColumnPos] = Flag;
        if (EndFlagColumnPos >= 0)
          PrevRow[EndFlagColumnPos] = Flag;
      }
      // Для первой и последней строки флаги устанавливаются принудительно
      if (BeginFlagColumnPos >= 0)
        dv[0][BeginFlagColumnPos] = true;
      if (EndFlagColumnPos >= 0)
        dv[dv.Count - 1][EndFlagColumnPos] = true;
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
    /// считаются одинаковыми. Для сравнения используется метод Object.IsEqual.
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
        int Level1 = GetEqLevel(value1);
        int Level2 = GetEqLevel(value2);
        if (Level1 > 0 && Level2 > 0)
        {
          int Level = Math.Max(Level1, Level2);
          value1 = ToEqLevel(value1, Level);
          value2 = ToEqLevel(value2, Level);
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
    /// Таблица должна содержать все поля, перечисленные в KeyFieldNames</param>
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
        throw new ArgumentNullException("KeyFieldNames");
#endif

      string[] aKeyColumnNames = keyColumnNames.Split(',');
      int[] KeyColumnPoss = new int[aKeyColumnNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyColumnNames.Length; i++)
      {
        KeyColumnPoss[i] = GetColumnPosWithCheck(srcTable, aKeyColumnNames[i]);
        DataColumn SrcCol = srcTable.Columns[KeyColumnPoss[i]];
        keyTable.Columns.Add(SrcCol.ColumnName, SrcCol.DataType); // другие ограничения не применяем
      }

      // 26.01.2016
      // Среди значений полей, по которым выполяется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyFieldNames);
      object[] KeyValues = new object[aKeyColumnNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      foreach (DataRow SrcRow in srcTable.Rows)
      {
        InitGroupRowsKeyValues(SrcRow, KeyValues, KeyColumnPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> Rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        Rows2.Add(new List<DataRow>());

      foreach (DataRow SrcRow in srcTable.Rows)
      {
        InitGroupRowsKeyValues(SrcRow, KeyValues, KeyColumnPoss, dbNullAsZero);
        DataRow KeyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues); // 26.01.2017

        int KeyRowIndex = keyTable.Rows.IndexOf(KeyRow);
        List<DataRow> CurrList = Rows2[KeyRowIndex];
        CurrList.Add(SrcRow);
      }

      // Преобразование списков в массив
      rows = new DataRow[Rows2.Count][];
      for (int i = 0; i < Rows2.Count; i++)
        rows[i] = Rows2[i].ToArray();
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcTable, keyColumnNames, out KeyTable, out Rows, false);
      return Rows;
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcTable, keyColumnNames, out KeyTable, out Rows, dbNullAsZero);
      return Rows;
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
    /// Таблица, на основе которой построен SrcDataView, должна содержать все поля, перечисленные в <paramref name="keyColumnNames"/></param>
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
        throw new ArgumentNullException("keyFieldNames");
#endif
      string[] aKeyFieldNames = keyColumnNames.Split(',');
      int[] KeyFieldPoss = new int[aKeyFieldNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyFieldNames.Length; i++)
      {
        KeyFieldPoss[i] = GetColumnPosWithCheck(srcDataView.Table, aKeyFieldNames[i]);
        DataColumn SrcCol = srcDataView.Table.Columns[KeyFieldPoss[i]];
        keyTable.Columns.Add(SrcCol.ColumnName, SrcCol.DataType); // другие ограничения не применяем
      }
      // 26.01.2016
      // Среди значений полей, по которым выполяется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyFieldNames);
      object[] KeyValues = new object[aKeyFieldNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      foreach (DataRowView SrcDRV in srcDataView)
      {
        InitGroupRowsKeyValues(SrcDRV.Row, KeyValues, KeyFieldPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> Rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        Rows2.Add(new List<DataRow>());

      foreach (DataRowView SrcDRV in srcDataView)
      {
        InitGroupRowsKeyValues(SrcDRV.Row, KeyValues, KeyFieldPoss, dbNullAsZero);
        DataRow KeyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues); // 26.01.2017

        int KeyRowIndex = keyTable.Rows.IndexOf(KeyRow);
        List<DataRow> CurrList = Rows2[KeyRowIndex];
        CurrList.Add(SrcDRV.Row);
      }

      // Преобразование списков в массив
      rows = new DataRow[Rows2.Count][];
      for (int i = 0; i < Rows2.Count; i++)
        rows[i] = Rows2[i].ToArray();
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcDataView, keyColumnNames, out KeyTable, out Rows, false);
      return Rows;
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcDataView, keyColumnNames, out KeyTable, out Rows, dbNullAsZero);
      return Rows;
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


      string[] aKeyFieldNames = keyColumnNames.Split(',');
      int[] KeyFieldPoss = new int[aKeyFieldNames.Length];
      keyTable = new DataTable();
      for (int i = 0; i < aKeyFieldNames.Length; i++)
      {
        KeyFieldPoss[i] = GetColumnPosWithCheck(srcRows[0].Table, aKeyFieldNames[i]);
        DataColumn SrcCol = srcRows[0].Table.Columns[KeyFieldPoss[i]];
        keyTable.Columns.Add(SrcCol.ColumnName, SrcCol.DataType); // другие ограничения не применяем
      }

      // 26.01.2016
      // Среди значений полей, по которым выполяется группировка, могут быть DBNull.
      // Их нельзя применять в качестве первичных ключей таблицы, но можно использовать в сортировке DataView.Sort

      //DataTools.SetPrimaryKey(KeyTable, KeyFieldNames);
      object[] KeyValues = new object[aKeyFieldNames.Length];
      keyTable.DefaultView.Sort = keyColumnNames;

      // 15.02.2019
      // Сначала добавляем все строки в ключевую таблицу, а затем - сортируем ее
      for (int j = 0; j < srcRows.Length; j++)
      {
        InitGroupRowsKeyValues(srcRows[j], KeyValues, KeyFieldPoss, dbNullAsZero);
        DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues);
      }
      keyTable = keyTable.DefaultView.ToTable();
      keyTable.DefaultView.Sort = keyColumnNames;

      List<List<DataRow>> Rows2 = new List<List<DataRow>>(keyTable.Rows.Count);
      for (int i = 0; i < keyTable.Rows.Count; i++)
        Rows2.Add(new List<DataRow>());

      for (int j = 0; j < srcRows.Length; j++)
      {
        InitGroupRowsKeyValues(srcRows[j], KeyValues, KeyFieldPoss, dbNullAsZero);
        DataRow KeyRow = DataTools.FindOrAddDataRow(keyTable.DefaultView, KeyValues); // 26.01.2017

        int KeyRowIndex = keyTable.Rows.IndexOf(KeyRow);
        List<DataRow> CurrList = Rows2[KeyRowIndex];
        CurrList.Add(srcRows[j]);
      }

      // Преобразование списков в массив
      rows = new DataRow[Rows2.Count][];
      for (int i = 0; i < Rows2.Count; i++)
        rows[i] = Rows2[i].ToArray();
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcRows, keyColumnNames, out KeyTable, out Rows);
      return Rows;
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
      DataTable KeyTable;
      DataRow[][] Rows;
      GroupRows(srcRows, keyColumnNames, out KeyTable, out Rows, dbNullAsZero);
      return Rows;
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

      DataColumn Res = new DataColumn();

      Res.ColumnName = srcColumn.ColumnName;
      Res.DataType = srcColumn.DataType;
      Res.Caption = srcColumn.Caption;
      Res.Expression = srcColumn.Expression;
      Res.DefaultValue = srcColumn.DefaultValue;
      Res.AllowDBNull = srcColumn.AllowDBNull;
      Res.AutoIncrement = srcColumn.AutoIncrement;
      Res.AutoIncrementSeed = srcColumn.AutoIncrementSeed;
      Res.AutoIncrementStep = srcColumn.AutoIncrementStep;
      Res.ColumnMapping = srcColumn.ColumnMapping;
      Res.DateTimeMode = srcColumn.DateTimeMode;
      Res.MaxLength = srcColumn.MaxLength;
      Res.Namespace = srcColumn.Namespace;
      Res.ReadOnly = srcColumn.ReadOnly;
      Res.Unique = srcColumn.Unique;

      CopyProperties(srcColumn.ExtendedProperties, Res.ExtendedProperties);

      return Res;
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

      Dictionary<string, object> Dict = new Dictionary<string, object>(row.Table.Columns.Count);
      for (int i = 0; i < row.Table.Columns.Count; i++)
        Dict.Add(row.Table.Columns[i].ColumnName, row[i, rowVersion]);
      return Dict;
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
      int OrdColPos = table.Columns.Count - 1;
      table.BeginLoadData();
      for (int i = 0; i < rows.Length; i++)
      {
        DataRow row2 = table.Rows.Add(rows[i].ItemArray);
        row2[OrdColPos] = i;
      }
      table.EndLoadData();
      table.DefaultView.Sort = sort;

      DataRow[] rows2 = new DataRow[rows.Length];
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        int order = (int)(table.DefaultView[i].Row[OrdColPos]);
        rows2[i] = rows[order];
      }

      // Лучше замену массива выполнить в самом конце, тут меньше вероятность исключения
      rows2.CopyTo(rows, 0);
    }

    #endregion

    #endregion

    #region Копирование данных

    /// <summary>
    /// Копирование "Строка в строку".
    /// Копируются все таблицы из исходного набора в конечный набор
    /// Копируются ExtendedProperties
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
        DataTable SrcTable = srcDS.Tables[i];
        DataTable DstTable = dstDS.Tables[SrcTable.TableName];
        if (DstTable != null)
          CopyRowsToRows(SrcTable, DstTable, false, false);
      }
      CopyProperties(srcDS.ExtendedProperties, dstDS.ExtendedProperties);
    }

    private static void CopyProperties(PropertyCollection src, PropertyCollection dst)
    {
      if (src.Count == 0)
        return;
      foreach (object Key in src.Keys)
        dst[Key] = src[Key];
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
      DataColumn[] OldPrimaryKey = dstTable.PrimaryKey;
      dstTable.PrimaryKey = null;
      try
      {
        if (useColumnNames)
        {
          // При копировании по именам столбцов сначала создаем список пар, которые можно копировать
          List<int> SrcColPos = new List<int>();
          List<int> DstColPos = new List<int>();
          for (int i = 0; i < srcTable.Columns.Count; i++)
          {
            int p = dstTable.Columns.IndexOf(srcTable.Columns[i].ColumnName);
            if (p >= 0)
            {
              SrcColPos.Add(i);
              DstColPos.Add(p);
              // Копируем свойства столбцов
              DataColumn SrcCol = srcTable.Columns[i];
              DataColumn DstCol = dstTable.Columns[p];
              CopyProperties(SrcCol.ExtendedProperties, DstCol.ExtendedProperties);
            }
          }
          if (SrcColPos.Count > 0)
          {
            for (int i = 0; i < srcTable.Rows.Count; i++)
            {
              DataRow SrcRow = srcTable.Rows[i];
              if (SrcRow.RowState == DataRowState.Deleted)
                continue;
              DataRow DstRow = addRows ? dstTable.NewRow() : dstTable.Rows[i];
              for (int j = 0; j < SrcColPos.Count; j++)
                DstRow[DstColPos[j]] = SrcRow[SrcColPos[j]];
              if (addRows)
                dstTable.Rows.Add(DstRow);
            }
          }
        }
        else
        {
          for (int i = 0; i < srcTable.Rows.Count; i++)
          {
            DataRow SrcRow = srcTable.Rows[i];
            if (SrcRow.RowState == DataRowState.Deleted)
              continue;
            DataRow DstRow = addRows ? dstTable.NewRow() : dstTable.Rows[i];
            CopyRowValues(SrcRow, DstRow, false);
            if (addRows)
              dstTable.Rows.Add(DstRow);
          }
          // Копируем свойства столбцов
          for (int j = 0; j < srcTable.Columns.Count; j++)
          {
            DataColumn SrcCol = srcTable.Columns[j];
            DataColumn DstCol = dstTable.Columns[j];
            CopyProperties(SrcCol.ExtendedProperties, DstCol.ExtendedProperties);
          }
        }
      }
      finally
      {
        dstTable.PrimaryKey = OldPrimaryKey;
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
        DataColumnCollection SrcColumns = srcRow.Table.Columns;
        DataColumnCollection DstColumns = dstRow.Table.Columns;
        DataRowVersion srcVer = DataRowVersion.Default;
        if (srcRow.RowState == DataRowState.Deleted)
          srcVer = DataRowVersion.Original; // 17.07.2020

        for (int i = 0; i < SrcColumns.Count; i++)
        {
          int p = DstColumns.IndexOf(SrcColumns[i].ColumnName);
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
    /// <param name="srcColumnPrefix">Префикс имени поля в исходной строке</param>
    /// <param name="dstColumnPrefix">Префикс имени поля в конечной строке</param>
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
        string ColumnName = srcRow.Table.Columns[i].ColumnName;
        if (!String.IsNullOrEmpty(srcColumnPrefix))
        {
          if (ColumnName.StartsWith(srcColumnPrefix))
            ColumnName = ColumnName.Substring(srcColumnPrefix.Length);
          else
            continue;
        }
        if (!String.IsNullOrEmpty(dstColumnPrefix))
          ColumnName = dstColumnPrefix + ColumnName;
        int p = dstRow.Table.Columns.IndexOf(ColumnName);
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
      DataRow[] Rows = GetDataViewRows(dv);
      ReplaceNulls(Rows, columnName, newValue);
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
      DataColumn Column = table.Columns[columnIndex];

      if (Column.DataType != typeof(string))
        throw new ArgumentException("Столбец \"" + Column.ColumnName + "\" не является строковым");

      foreach (DataRow Row in table.Rows)
      {
        if (Row.IsNull(columnIndex))
          continue;
        string s1 = (string)Row[columnIndex];
        string s2 = s1.Trim();
        if (s2.Length == s1.Length)
          continue;
        if (s2.Length == 0 && Column.AllowDBNull)
          Row[columnIndex] = DBNull.Value;
        else
          Row[columnIndex] = s2;
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

      foreach (DataRow SrcRow in srcTable.Rows)
        UpdateRowByPrimaryKey(SrcRow, dstTable, useColumnNames);
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
      object[] Keys = GetPrimaryKeyValues(srcRow, dstTable);
      if (Keys == null)
        throw new ArgumentException("Не удалось извлечь ключевые поля для строки", "dstTable");

      DataRow DstRow = dstTable.Rows.Find(Keys);
      if (DstRow == null)
        return false;

      CopyRowValues(srcRow, DstRow, useColumnNames);
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
      Dictionary<DataRow, object> UsedRows = new Dictionary<DataRow, object>(dstTable.Rows.Count);

      int OrgRowCount = dstTable.Rows.Count;

      dstTable.BeginLoadData();
      try
      {
        #region Первый проход - добавление / изменение строк

        foreach (DataRow SrcRow in srcTable.Rows)
        {
          // Поиск по ключу
          object[] Keys = GetPrimaryKeyValues(SrcRow, dstTable);
          if (Keys == null)
            throw new InvalidOperationException("Не удалось извлечь ключевые поля для строки");

          DataRow DstRow = dstTable.Rows.Find(Keys);
          if (DstRow == null)
          {
            DstRow = dstTable.NewRow();
            CopyRowValues(SrcRow, DstRow, useColumnNames);
            dstTable.Rows.Add(DstRow);
          }
          else
          {
            CopyRowValues(SrcRow, DstRow, useColumnNames);
            UsedRows.Add(DstRow, null);
          }
        }

        #endregion

        #region Второй проход - удаление ненужных строк

        for (int i = OrgRowCount - 1; i >= 0; i--)
        {
          if (!UsedRows.ContainsKey(dstTable.Rows[i]))
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
        Int32 Id = (Int32)(dv[i].Row["Id"]);
        res.Add(Id, i);
      }
      return res;
    }

    #region GetDataViewSortFieldNames

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
        if (s.StartsWith("[") && s.EndsWith("]"))
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
      if (columnName.StartsWith("[") && columnName.EndsWith("]"))
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
        return columnNames[0];

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < columnNames.Length; i++)
      {
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
        string FieldName = GetDataViewSortSingleColumnName(dv.Sort);
        row[FieldName] = searchValue;
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
        string[] FieldNames = GetDataViewSortColumnNames(dv.Sort); // не может вернуть null, т.к. dvSort - непустая строка
        if (searchValues.Length != FieldNames.Length)
          throw new ArgumentException("Длина списка значений (" + searchValues.Length.ToString() + ") не совпадает с числом полей для сортироки (" + FieldNames.Length.ToString() + ") заданном в свойстве Sort объекта DataView", "searchValues");
        for (int i = 0; i < FieldNames.Length; i++)
          row[FieldNames[i]] = searchValues[i];
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

      DataTable ResTable = srcTable.Clone();
      if (srcTable.Rows.Count == 0)
        return ResTable;
      int[] UniqueFieldPoss = new int[uniqueColumnNames.Length];
      for (int j = 0; j < uniqueColumnNames.Length; j++)
        UniqueFieldPoss[j] = GetColumnPosWithCheck(srcTable, uniqueColumnNames[j]);

      ResTable.DefaultView.Sort = String.Join(",", uniqueColumnNames);
      ResTable.DefaultView.RowFilter = String.Empty;

      object[] Keys = new object[uniqueColumnNames.Length];
      for (int i = 0; i < srcTable.Rows.Count; i++)
      {
        DataRow SrcRow = srcTable.Rows[i];
        if (SrcRow.RowState == DataRowState.Deleted)
          continue;

        for (int j = 0; j < uniqueColumnNames.Length; j++)
          Keys[j] = SrcRow[UniqueFieldPoss[j]];

        if (ResTable.DefaultView.Find(Keys) < 0)
          ResTable.Rows.Add(SrcRow.ItemArray);
      }

      ResTable.DefaultView.Sort = srcTable.DefaultView.Sort;
      ResTable.DefaultView.RowFilter = srcTable.DefaultView.RowFilter;
      return ResTable;
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

      DataTable ResTable = srcDV.Table.Clone();
      if (srcDV.Count == 0)
        return ResTable;
      int[] UniqueFieldPoss = new int[uniqueColumnNames.Length];
      for (int j = 0; j < uniqueColumnNames.Length; j++)
        UniqueFieldPoss[j] = GetColumnPosWithCheck(srcDV.Table, uniqueColumnNames[j]);

      ResTable.DefaultView.Sort = String.Join(",", uniqueColumnNames);
      ResTable.DefaultView.RowFilter = String.Empty;

      object[] Keys = new object[uniqueColumnNames.Length];
      for (int i = 0; i < srcDV.Count; i++)
      {
        DataRow SrcRow = srcDV[i].Row;

        for (int j = 0; j < uniqueColumnNames.Length; j++)
          Keys[j] = SrcRow[UniqueFieldPoss[j]];

        if (ResTable.DefaultView.Find(Keys) < 0)
          ResTable.Rows.Add(SrcRow.ItemArray);
      }

      ResTable.DefaultView.Sort = String.Empty;
      ResTable.DefaultView.RowFilter = String.Empty;
      return ResTable;
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
        bytes = new byte[0];
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
      object EmptyValue = GetEmptyValue(testValue.GetType());
      return testValue.Equals(EmptyValue);

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
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (!AreAllDecimalZeros(Row, columnNames))
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
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (!AreAllDecimalZeros(Row))
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
    public static MinMaxInt GetEnumRange(Type enumTyp)
    {
      Array a = Enum.GetValues(enumTyp);

      MinMaxInt mm = new MinMaxInt();
      for (int i = 0; i < a.Length; i++)
        mm += (int)(a.GetValue(i));
      return mm;
    }

    /// <summary>
    /// "Классическая" функция обмена двуж значений по ссылке
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
