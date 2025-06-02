using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Извлечение значения поля с заданным именем или позицией из объекта <see cref="DbDataReader"/>.
  /// Перед извлечением выполняется проверка наличия значения с помощью <see cref="DbDataReader.IsDBNull(int)"/>.
  /// При несовпадении типа поля выполняется попытка преобразования значения.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// Если <see cref="DbDataReader"/> предназначен для чтения нескольких таблиц с разными списками полей, следует создавать отдельные
  /// экземпляры структуры для каждой таблицы.
  /// </summary>
  public struct DbDataReaderValue /*: IObjectWithCode*/
  {
    #region Конструкторы

    /// <summary>
    /// Инициализация структуры для поля с заданным индексом в пределах <see cref="DbDataReader"/>.
    /// </summary>
    /// <param name="reader">Ссылка на <see cref="DbDataReader"/></param>
    /// <param name="columnIndex">Индекс столбца в пределах от 0 до (<see cref="DbDataReader.FieldCount"/>-1)</param>
    public DbDataReaderValue(DbDataReader reader, int columnIndex)
    {
#if DEBUG
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (columnIndex < 0 || columnIndex >= reader.FieldCount)
        throw ExceptionFactory.ArgOutOfRange("columnIndex", columnIndex, 0, reader.FieldCount - 1);
#endif
      _Reader = reader;
      _ColumnIndex = columnIndex;
      _ColumnType = reader.GetFieldType(columnIndex);
    }

    /// <summary>
    /// Инициализация структуры для поля с заданным именем
    /// </summary>
    /// <param name="reader">Ссылка на <see cref="DbDataReader"/></param>
    /// <param name="columnName">Имя столбца. Если <paramref name="reader"/> не содержит такого поля, выбрасывается исключение.</param>
    public DbDataReaderValue(DbDataReader reader, string columnName)
      : this(reader, GetColumnIndex(reader, columnName))
    {
    }

    private static int GetColumnIndex(DbDataReader reader, string columnName)
    {
#if DEBUG
      if (reader == null)
        throw new ArgumentNullException("reader");
#endif
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      int index = reader.GetOrdinal(columnName);
      if (index < 0)
        throw ExceptionFactory.ArgUnknownValue("columnName", columnName);
      return index;
    }

    #endregion

    #region Свойства

    private readonly DbDataReader _Reader;

    private readonly int _ColumnIndex;

    private readonly Type _ColumnType;

    /// <summary>
    /// Возвращает имя используемого столбца
    /// </summary>
    public string ColumnName
    {
      get
      {
        if (_Reader == null)
          return String.Empty;
        if (_Reader.IsClosed)
          return String.Empty;
        return _Reader.GetName(_ColumnIndex);
      }
    }

    //string IObjectWithCode.Code { get { return ColumnName; } }

    /// <summary>
    /// Возвращает имя используемого столбца, если <see cref="DbDataReader"/> не был закрыт
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Reader == null)
        return "{Empty}";
      if (_Reader.IsClosed)
        return "{Closed}";
      return _Reader.GetName(_ColumnIndex);
    }

    #endregion

    #region Извлечение значения

    /// <summary>
    /// Вызывает <see cref="DbDataReader.IsDBNull(int)"/>
    /// </summary>
    public bool IsDBNull { get { return _Reader.IsDBNull(_ColumnIndex); } }

    /// <summary>
    /// Возвращает текущее значение поля <see cref="DbDataReader.GetValue(int)"/>.
    /// Если <see cref="DbDataReader.IsDBNull(int)"/>=true, возвращает null.
    /// </summary>
    public object Value
    {
      get
      {
        if (_Reader.IsDBNull(_ColumnIndex))
          return null;
        else
          return _Reader.GetValue(_ColumnIndex);
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Int32 AsInt32
    {
      get
      {
        if (_ColumnType == typeof(Int32))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return 0;
          else
            return _Reader.GetInt32(_ColumnIndex);
        }
        else
          return DataTools.GetInt(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Int32? AsNullableInt32
    {
      get
      {
        if (_ColumnType == typeof(Int32))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetInt32(_ColumnIndex);
        }
        else
          return DataTools.GetNullableInt(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Int64 AsInt64
    {
      get
      {
        if (_ColumnType == typeof(Int64))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return 0L;
          else
            return _Reader.GetInt64(_ColumnIndex);
        }
        else
          return DataTools.GetInt64(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Int64? AsNullableInt64
    {
      get
      {
        if (_ColumnType == typeof(Int64))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetInt64(_ColumnIndex);
        }
        else
          return DataTools.GetNullableInt64(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Single AsSingle
    {
      get
      {
        if (_ColumnType == typeof(Single))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return 0;
          else
            return _Reader.GetFloat(_ColumnIndex);
        }
        else
          return DataTools.GetSingle(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Single? AsNullableSingle
    {
      get
      {
        if (_ColumnType == typeof(Single))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetFloat(_ColumnIndex);
        }
        else
          return DataTools.GetNullableSingle(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Double AsDouble
    {
      get
      {
        if (_ColumnType == typeof(Double))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return 0;
          else
            return _Reader.GetDouble(_ColumnIndex);
        }
        else
          return DataTools.GetDouble(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Double? AsNullableDouble
    {
      get
      {
        if (_ColumnType == typeof(Double))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetDouble(_ColumnIndex);
        }
        else
          return DataTools.GetNullableDouble(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Decimal AsDecimal
    {
      get
      {
        if (_ColumnType == typeof(Decimal))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return 0;
          else
            return _Reader.GetDecimal(_ColumnIndex);
        }
        else
          return DataTools.GetDecimal(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Decimal? AsNullableDecimal
    {
      get
      {
        if (_ColumnType == typeof(Decimal))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetDecimal(_ColumnIndex);
        }
        else
          return DataTools.GetNullableDecimal(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Boolean AsBoolean
    {
      get
      {
        if (_ColumnType == typeof(Boolean))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return false;
          else
            return _Reader.GetBoolean(_ColumnIndex);
        }
        else
          return DataTools.GetBool(_Reader.GetValue(_ColumnIndex));
      }
    }

    ///// <summary>
    ///// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    ///// значения в нужный тип при необходимости.
    ///// </summary>
    //public Boolean? AsNullableBoolean
    //{
    //  get
    //  {
    //    if (_Reader.IsDBNull(_ColumnIndex))
    //      return null;
    //    else
    //    {
    //      if (_ColumnType == typeof(Boolean))
    //        return _Reader.GetBoolean(_ColumnIndex);
    //      else
    //        return DataTools.GetBool(_Reader.GetValue(_ColumnIndex));
    //    }
    //  }
    //}

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public String AsString
    {
      get
      {
        if (_ColumnType == typeof(String))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return String.Empty;
          else
            return _Reader.GetString(_ColumnIndex).Trim();
        }
        else
          return DataTools.GetString(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Guid AsGuid
    {
      get
      {
        if (_ColumnType == typeof(Guid))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return Guid.Empty;
          else
            return _Reader.GetGuid(_ColumnIndex);
        }
        else
          return DataTools.GetGuid(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public Guid? AsNullableGuid
    {
      get
      {
        if (_Reader.IsDBNull(_ColumnIndex))
          return null;
        else
        {
          if (_ColumnType == typeof(Guid))
            return _Reader.GetGuid(_ColumnIndex);
          else
            return DataTools.GetGuid(_Reader.GetValue(_ColumnIndex));
        }
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public DateTime AsDateTime
    {
      get
      {
        if (_ColumnType == typeof(DateTime))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return DateTime.MinValue;
          else
            return _Reader.GetDateTime(_ColumnIndex);
        }
        else
          return DataTools.GetDateTime(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public DateTime? AsNullableDateTime
    {
      get
      {
        if (_ColumnType == typeof(DateTime))
        {
          if (_Reader.IsDBNull(_ColumnIndex))
            return null;
          else
            return _Reader.GetDateTime(_ColumnIndex);
        }
        else
          return DataTools.GetNullableDateTime(_Reader.GetValue(_ColumnIndex));
      }
    }


    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public TimeSpan AsTimeSpan
    {
      get
      {
        if (_Reader.IsDBNull(_ColumnIndex))
          return TimeSpan.Zero;
        else
          return DataTools.GetTimeSpan(_Reader.GetValue(_ColumnIndex));
      }
    }

    /// <summary>
    /// Возвращает текущее значение поля. Проверяется наличие значение поля <see cref="DbDataReader.IsDBNull(int)"/> и преобразование
    /// значения в нужный тип при необходимости.
    /// </summary>
    public TimeSpan? AsNullableTimeSpan
    {
      get
      {
        if (_Reader.IsDBNull(_ColumnIndex))
          return null;
        else
          return DataTools.GetTimeSpan(_Reader.GetValue(_ColumnIndex));
      }
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Создает словарь объектов доступа к значениям по именам полей для инициализированного <see cref="DbDataReader"/>.
    /// Словарь не чувствителен к регистру имен полей.
    /// </summary>
    /// <param name="reader">Ссылка на <see cref="DbDataReader"/></param>
    /// <returns>Словарь</returns>
    public static IDictionary<string, DbDataReaderValue> CreateDict(DbDataReader reader)
    {
#if DEBUG
      if (reader == null)
        throw new ArgumentNullException("reader");
#endif

      Collections.TypedStringDictionary<DbDataReaderValue> dict = new Collections.TypedStringDictionary<DbDataReaderValue>(reader.FieldCount, true);
      for (int i = 0; i < reader.FieldCount; i++)
        dict.Add(reader.GetName(i), new DbDataReaderValue(reader, i));
      return dict;
    }

    #endregion
  }

#if XXX
  public sealed class DbDataReaderValueList : NamedList<DbDataReaderValue>
  {
  #region Конструктор

    public DbDataReaderValueList(DbDataReader reader)
      :base(reader.FieldCount, true)
    {
      for (int i = 0; i < reader.FieldCount; i++)
        Add(new DbDataReaderValue(reader, i));

      SetReadOnly();
    }

  #endregion
  }
#endif
}
