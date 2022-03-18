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
  /// Не предназначены для использования в прикладном коде
  /// </summary>
  public static class DBxTools
  {
    #region Типы данных

    /// <summary>
    /// Преобразует тип данных Net Framework в тип столбца
    /// Если имеется значение, а не только тип Type, следует использовать метод ValueToColumnType().
    /// </summary>
    /// <param name="t">Тип данных</param>
    /// <returns>Тип столбца</returns>
    public static DBxColumnType DataTypeToColumnType(Type t)
    {
      if (t == null)
        return DBxColumnType.Unknown;
      if (t == typeof(DBNull))
        return DBxColumnType.Unknown;
      if (t == typeof(string))
        return DBxColumnType.String;
      if (DataTools.IsIntegerType(t))
        return DBxColumnType.Int;
      if (t == typeof(decimal))
        return DBxColumnType.Money;
      if (DataTools.IsFloatType(t))
        return DBxColumnType.Float;
      if (t == typeof(DateTime))
        return DBxColumnType.DateTime;
      if (t == typeof(Boolean))
        return DBxColumnType.Boolean;
      if (t == typeof(Guid))
        return DBxColumnType.Guid;
      if (t == typeof(Byte[]))
        return DBxColumnType.Binary;

      throw new ArgumentException("Неизвестный тип данных: " + t.ToString(), "t");
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
        case DBxColumnType.String: return typeof(string);
        case DBxColumnType.Int: return typeof(int);
        case DBxColumnType.Float: return typeof(double);
        case DBxColumnType.Money: return typeof(decimal);
        case DBxColumnType.Boolean: return typeof(bool);
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
        case DBxColumnType.Time: return typeof(DateTime);
        case DBxColumnType.Guid: return typeof(Guid);
        case DBxColumnType.Memo:
        case DBxColumnType.Xml: return typeof(string);
        case DBxColumnType.Binary: return typeof(byte[]);
        default:
          throw new ArgumentException("Неизвестный тип данных: " + columnType.ToString(), "columnType");
      }
    }

    /// <summary>
    /// Получить тип данных для столбца, исходя из значения.
    /// В отличие от DataTypeToColumnType(), отличает Date и DateTime
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


    private static readonly byte[] _EmptyBytes = new byte[0];

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
        case DBxColumnType.Int: return 0;
        case DBxColumnType.Float: return 0.0;
        case DBxColumnType.Money: return 0m;
        case DBxColumnType.Boolean: return false;
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
        case DBxColumnType.Time: return DateTime.MinValue;
        case DBxColumnType.Guid: return Guid.Empty;
        case DBxColumnType.Memo:
        case DBxColumnType.Xml: return String.Empty;
        case DBxColumnType.Binary: return _EmptyBytes;
        default:
          throw new ArgumentException("Неизвестный тип данных: " + columnType.ToString(), "columnType");
      }
    }

    /// <summary>
    /// Создает массив объектов DBxColumn для списка имен полей
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

    #endregion

    #region Вывод отладочной информации

    /// <summary>
    /// Инициализация улучшенной отладки объектов из ExtDB.
    /// Для сервера вызывается автоматически при создании объекта DBx.
    /// При использовании библиотеки ExtDBDocs вызывается автоматически при создании DBxDocProvider.
    /// Повторные вызовы отбрасываются
    /// </summary>
    public static void InitLogout()
    {
      if (_InitLogoutCalled)
        return;
      _InitLogoutCalled = true;
      LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded); // 19.08.2020 перенесено сюда
      LogoutTools.LogoutProp += new LogoutPropEventHandler(LogoutTools_LogoutProp);
    }

    private static bool _InitLogoutCalled = false;

    private static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader("DBx");
      foreach (DBx db in DBx.AllDBList)
      {
        args.WriteLine(db.ToString());
        args.IndentLevel++;
        if (db.IsDisposed)
          args.WriteLine("Disposed");
        else
        {
          LogoutTools.LogoutObject(args, db);
        }
        args.IndentLevel--;
      }
    }

    /// <summary>
    /// Предотвращаем вывод строк подключения, т.к. они могут содержать секретную информацию о пароле.
    /// Также не выводим информацию о структуре БД, т.к. она может быть очень большой
    /// </summary>
    private static void LogoutTools_LogoutProp(object sender, LogoutPropEventArgs args)
    {
      if (args.Object is DBx)
      {
        switch (args.PropertyName)
        {
          case "Struct":
            args.Mode = LogoutPropMode.ToString;
            break;
        }
        return;
      }

      if (args.Object is DbConnection)
      {
        switch (args.PropertyName)
        {
          case "ConnectionString":
            args.Mode = LogoutPropMode.None;
            break;
        }
        return;
      }
      if (args.Object is DBxPermissions)
      {
        args.Mode = LogoutPropMode.ToString; // 28.08.2019
        return;
      }
      if (args.Object is DBxColumns || args.Object is DBxColumnList)
      {
        args.Mode = LogoutPropMode.ToString; // 14.11.2019
        return;
      }
      if (args.Object is DBxNamedExpression)
      {
        args.Mode = LogoutPropMode.ToString; // 19.12.2019
        return;
      }
    }

    #endregion
  }
}
