// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Logging;
using System.Data.Common;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Статические методы для работы с базами данных.
  /// Не предназначены для использования в прикладном коде.
  /// </summary>
  public static class DBxTools
  {
    #region Типы данных

    /// <summary>
    /// Возвращает true, если тип данных является целочисленным
    /// </summary>
    /// <param name="columnType">Проверяемый тип данных</param>
    /// <returns>Целочисленный тип?</returns>
    public static bool IsIntegerType(DBxColumnType columnType)
    {
      return columnType >= DBxColumnType.SByte && columnType <= DBxColumnType.UInt64;
    }

    /// <summary>
    /// Возвращает true, если тип данных является числом с плавающей точкой
    /// </summary>
    /// <param name="columnType">Проверяемый тип данных</param>
    /// <returns>Число с плавающей точкой?</returns>
    public static bool IsFloatType(DBxColumnType columnType)
    {
      return columnType >= DBxColumnType.Single && columnType <= DBxColumnType.Decimal;
    }

    /// <summary>
    /// Возвращает true, если тип данных является числовым
    /// </summary>
    /// <param name="columnType">Проверяемый тип данных</param>
    /// <returns>Числовой тип?</returns>
    public static bool IsNumericType(DBxColumnType columnType)
    {
      return columnType >= DBxColumnType.SByte && columnType <= DBxColumnType.Decimal;
    }

    /// <summary>
    /// Возвращает true, если тип данных является подходящим для столбца первичного ключа и ссылочных столбцов
    /// </summary>
    /// <param name="columnType">Проверяемый тип данных</param>
    /// <returns>Может быть ключом</returns>
    public static bool IsValidIdColumnType(DBxColumnType columnType)
    {
      switch (columnType)
      {
        case DBxColumnType.Int16:
        case DBxColumnType.UInt16:
        case DBxColumnType.Int32:
        case DBxColumnType.UInt32:
        case DBxColumnType.Int64:
        case DBxColumnType.UInt64:
        case DBxColumnType.String:
        case DBxColumnType.Guid:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Получить следующий идентификатор.
    /// Для числовых типов возвращается значение, увеличенное на единицу с проверкой переполнения.
    /// Для GUID'ов возвращается <see cref="System.Guid.NewGuid()"/>.
    /// Для строкового типа и типов, не входящих в <see cref="IsValidIdColumnType(DBxColumnType)"/>, выбрасывается исключение.
    /// </summary>
    /// <param name="prevId">Текущее значение идентификатора</param>
    /// <param name="colType">Тип поля</param>
    /// <returns>Следующее значение идентификатора</returns>
    public static object GetNextId(object prevId, DBxColumnType colType)
    {
      switch (colType)
      {
        case DBxColumnType.Int16:
          checked { return System.Convert.ToInt16(DataTools.GetInt32(prevId) + 1); }
        case DBxColumnType.UInt16:
          checked { return System.Convert.ToUInt16(DataTools.GetInt32(prevId) + 1); }
        case DBxColumnType.Int32:
          checked { return DataTools.GetInt32(prevId) + 1; }
        case DBxColumnType.UInt32:
          checked { return System.Convert.ToUInt32(DataTools.GetInt64(prevId) + 1); }
        case DBxColumnType.Int64:
          checked { return DataTools.GetInt64(prevId) + 1; }
        case DBxColumnType.UInt64:
          checked { return System.Convert.ToUInt64(DataTools.GetInt64(prevId) + 1); } // не совсем верно, но пока неважно
        case DBxColumnType.Guid:
          return Guid.NewGuid();
        default:
          throw ExceptionFactory.ArgUnknownValue("colType", colType,
            new DBxColumnType[] { DBxColumnType.Int16, DBxColumnType.UInt16,
              DBxColumnType.Int32, DBxColumnType.UInt32,
              DBxColumnType.Int64, DBxColumnType.UInt64,
              DBxColumnType.Guid});
      }
    }


    /// <summary>
    /// Преобразует тип данных Net Framework в тип столбца.
    /// Если имеется значение, а не только тип <see cref="Type"/>, следует использовать метод <see cref="ValueToColumnType(object)"/>.
    /// </summary>
    /// <param name="t">Тип данных</param>
    /// <returns>Тип столбца</returns>
    public static DBxColumnType DataTypeToColumnType(Type t)
    {
      if (t == null)
        return DBxColumnType.Unknown;

      switch (Type.GetTypeCode(t))
      {
        case TypeCode.DBNull: return DBxColumnType.Unknown;
        case TypeCode.String: return DBxColumnType.String;
        case TypeCode.SByte: return DBxColumnType.SByte;
        case TypeCode.Byte: return DBxColumnType.Byte;
        case TypeCode.Int16: return DBxColumnType.Int16;
        case TypeCode.UInt16: return DBxColumnType.UInt16;
        case TypeCode.Int32: return DBxColumnType.Int32;
        case TypeCode.Int64: return DBxColumnType.Int64;
        case TypeCode.UInt64: return DBxColumnType.UInt64;
        case TypeCode.Single: return DBxColumnType.Single;
        case TypeCode.Double: return DBxColumnType.Double;
        case TypeCode.Decimal:return DBxColumnType.Decimal;
        case TypeCode.Boolean:return DBxColumnType.Boolean;
        case TypeCode.DateTime: return DBxColumnType.DateTime;
        default:
          if (t == typeof(TimeSpan))
            return DBxColumnType.Time;
          if (t == typeof(Guid))
            return DBxColumnType.Guid;
          if (t == typeof(Byte[]))
            return DBxColumnType.Binary;
          throw ExceptionFactory.ArgUnknownValue("t", t);
      }
    }

    /// <summary>
    /// Возвращает тип данных Net Framework для типа данных столбца.
    /// Так как передается только тип столбца, но не диапазон значений, возвращаемое значение может быть неточным.
    /// </summary>
    /// <param name="columnType">Тип столбца</param>
    /// <returns>Тип данных</returns>
    public static Type ColumnTypeToDataType(DBxColumnType columnType)
    {
      switch (columnType)
      {
        case DBxColumnType.Unknown: return null;
        case DBxColumnType.String: return typeof(String);
        case DBxColumnType.SByte: return typeof(SByte);
        case DBxColumnType.Byte: return typeof(Byte);
        case DBxColumnType.Int16: return typeof(Int16);
        case DBxColumnType.UInt16: return typeof(UInt16);
        case DBxColumnType.Int32: return typeof(Int32);
        case DBxColumnType.UInt32: return typeof(UInt32);
        case DBxColumnType.Int64: return typeof(Int64);
        case DBxColumnType.UInt64: return typeof(UInt64);
        case DBxColumnType.Single: return typeof(Single);
        case DBxColumnType.Double: return typeof(Double);
        case DBxColumnType.Decimal: return typeof(Decimal);
        case DBxColumnType.Boolean: return typeof(Boolean);
        case DBxColumnType.Date:
        case DBxColumnType.DateTime: return typeof(DateTime);
        case DBxColumnType.Time: return typeof(TimeSpan); // 16.05.2023
        case DBxColumnType.Guid: return typeof(Guid);
        case DBxColumnType.Memo:
        case DBxColumnType.Xml: return typeof(String);
        case DBxColumnType.Binary: return typeof(Byte[]);
        default:
          throw ExceptionFactory.ArgUnknownValue("columnType", columnType);
      }
    }

    /// <summary>
    /// Получить тип данных для столбца, исходя из значения.
    /// В отличие от <see cref="DataTypeToColumnType(Type)"/>, отличает Date и DateTime
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <returns>Тип столбца</returns>
    public static DBxColumnType ValueToColumnType(object value)
    {
      if (value == null)
        return DBxColumnType.Unknown;
      if (value is DBNull)
        return DBxColumnType.Unknown;

      if (value is DateTime)
      {
        DateTime dt = (DateTime)value;
        if (dt.TimeOfDay.Ticks == 0L)
          return DBxColumnType.Date;
        else if (dt.Date == DateTime.MinValue)
          return DBxColumnType.Time;
        else
          return DBxColumnType.DateTime;
      }

      return DataTypeToColumnType(value.GetType());
    }

    internal static DBxColumnType ValueToColumnTypeRequired(object value)
    {
      DBxColumnType ct = ValueToColumnType(value);

      if (ct == DBxColumnType.Unknown)
        throw new ArgumentException(String.Format(Res.DBxTools_Arg_ValueToColumnTypeRequired, value.GetType().ToString()), "value");

      return ct;
    }

    /// <summary>
    /// Получить значение по умолчанию для типа столбца
    /// </summary>
    /// <param name="columnType">Тип столбца</param>
    /// <returns>Значение по умолчанию</returns>
    public static object GetDefaultValue(DBxColumnType columnType)
    {
      switch (columnType)
      {
        case DBxColumnType.Unknown: return null;
        case DBxColumnType.String: return String.Empty;
        case DBxColumnType.SByte: return ZeroValue<SByte>.Object;
        case DBxColumnType.Byte: return ZeroValue<Byte>.Object;
        case DBxColumnType.Int16: return ZeroValue<Int16>.Object;
        case DBxColumnType.UInt16: return ZeroValue<UInt16>.Object;
        case DBxColumnType.Int32: return ZeroValue<Int32>.Object;
        case DBxColumnType.UInt32: return ZeroValue<UInt32>.Object;
        case DBxColumnType.Int64: return ZeroValue<Int64>.Object;
        case DBxColumnType.UInt64: return ZeroValue<UInt64>.Object;
        case DBxColumnType.Single: return ZeroValue<Single>.Object;
        case DBxColumnType.Double: return ZeroValue<Double>.Object;
        case DBxColumnType.Decimal: return ZeroValue<Decimal>.Object;
        case DBxColumnType.Boolean: return ZeroValue<Boolean>.Object;
        case DBxColumnType.Date:
        case DBxColumnType.DateTime: return ZeroValue<DateTime>.Object;
        case DBxColumnType.Time: return ZeroValue<TimeSpan>.Object; // 16.05.2023
        case DBxColumnType.Guid: return ZeroValue<Guid>.Object;
        case DBxColumnType.Memo:
        case DBxColumnType.Xml: return String.Empty;
        case DBxColumnType.Binary: return EmptyArray<Byte>.Empty;
        default:
          throw ExceptionFactory.ArgUnknownValue("columnType", columnType);
      }
    }

    /// <summary>
    /// Преобразование значения к указанному типу данных.
    /// Исходное значение <see cref="DBNull"/> заменяется на null.
    /// Если <paramref name="columnType"/>=<see cref="DBxColumnType.Unknown"/>, то возвращается неизмененное значение.
    /// Если <paramref name="value"/>=null и <paramref name="columnType"/> задан, то возвращается значение по умолчанию <see cref="GetDefaultValue(DBxColumnType)"/>.
    /// Если требуется преобразование из строки или в строку, то используются преобразования как в классе <see cref="StdConvert"/>.
    /// Если преобразование невозможно, то выбрасывается исключение. В частности, не разрешаются преобразования между числами и датами.
    /// </summary>
    /// <param name="value">Исходное значение, может быть null</param>
    /// <param name="columnType">Требуемый тип данных, может быть <see cref="DBxColumnType.Unknown"/></param>
    /// <returns>Преобразованное значение</returns>
    public static object Convert(object value, DBxColumnType columnType)
    {
      if (value is DBNull)
        value = null;
      if (columnType == DBxColumnType.Unknown)
        return value;
      if (Object.ReferenceEquals(value, null))
        return GetDefaultValue(columnType);

      switch (columnType)
      {
        case DBxColumnType.String:
        case DBxColumnType.Memo:
        case DBxColumnType.Xml:
          if (value is String)
            return value; // без обрезки
          else if (value is byte[])
            return StringTools.BytesToHex((byte[])value, false);
          else
            return DataTools.GetString(value);
        case DBxColumnType.Single:
          return DataTools.GetSingle(value);
        case DBxColumnType.Double:
          return DataTools.GetDouble(value);
        case DBxColumnType.Decimal:
          return DataTools.GetDecimal(value);
        case DBxColumnType.Boolean:
          return DataTools.GetBoolean(value);
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          DateTime? dt = DataTools.GetNullableDateTime(value);
          if (dt.HasValue)
          {
            dt = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
            if (columnType == DBxColumnType.Date)
              dt = dt.Value.Date;
            return dt;
          }
          else
            return DateTime.MinValue;
        case DBxColumnType.Time:
          return DataTools.GetTimeSpan(value);
        case DBxColumnType.Guid:
          return DataTools.GetGuid(value);
        case DBxColumnType.Binary:
          byte[] binData = value as byte[];
          if (binData != null)
            return binData;
          else
            return StringTools.HexToBytes(value.ToString());
        default:
          if (IsIntegerType(columnType))
          {
            Int64 x = DataTools.GetInt64(value);
            if (x == 0L)
              return GetDefaultValue(columnType); // без боксинга
            return System.Convert.ChangeType(x, ColumnTypeToDataType(columnType));
          }

          throw new NotImplementedException("columnType=" + columnType.ToString());
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Создает массив объектов <see cref="DBxColumn"/> для списка имен полей
    /// </summary>
    /// <param name="columnNames">Массив имен полей</param>
    /// <returns>Массив выражений</returns>
    public static DBxExpression[] GetColumnNameExpressions(string[] columnNames)
    {
      DBxExpression[] a = new DBxExpression[columnNames.Length];
      for (int i = 0; i < columnNames.Length; i++)
        a[i] = new DBxColumn(columnNames[i]);
      return a;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    public static bool IsComparision(DBxFunctionKind function)
    {
      switch (function)
      { 
        case DBxFunctionKind.Equal:
        case DBxFunctionKind.LessThan:
        case DBxFunctionKind.LessOrEqualThan:
        case DBxFunctionKind.GreaterThan:
        case DBxFunctionKind.GreaterOrEqualThan:
        case DBxFunctionKind.NotEqual:
          return true;
        default:
          return false;
      }
    }

    #endregion
  }
}
