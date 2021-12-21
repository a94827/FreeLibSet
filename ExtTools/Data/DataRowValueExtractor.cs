// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Интерфейс, реализуемый структурами DataRowXXXExtractor.
  /// Предназначен, в основном, для целей тестирования.
  /// Не следует использовать в прикладном коде.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IDataRowExtractor<T>
  {
    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    string ColumnName { get; }

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    T this[DataRow row] { get; }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowIntExtractor : IDataRowExtractor<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowIntExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public int this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0;
          else
            return (int)v;
        }
        else
          return DataTools.GetInt(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableIntExtractor : IDataRowExtractor<Int32?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableIntExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public int? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (int)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowInt64Extractor : IDataRowExtractor<Int64>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowInt64Extractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public long this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0L;
          else
            return (Int64)v;
        }
        else
          return DataTools.GetInt64(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableInt64Extractor : IDataRowExtractor<Int64?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableInt64Extractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public long? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (long)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt64(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowSingleExtractor : IDataRowExtractor<Single>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowSingleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public float this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Single))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0f;
          else
            return (float)v;
        }
        else
          return DataTools.GetSingle(row[~_ColumnIndex]);
      }
    }

    #endregion
  }


  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableSingleExtractor : IDataRowExtractor<Single?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableSingleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public float? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (float)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetSingle(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowDoubleExtractor : IDataRowExtractor<Double>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowDoubleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public double this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0.0;
          else
            return (double)v;
        }
        else
          return DataTools.GetDouble(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableDoubleExtractor : IDataRowExtractor<Double?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableDoubleExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public double? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (double)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDouble(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowDecimalExtractor : IDataRowExtractor<Decimal>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowDecimalExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public decimal this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return 0m;
          else
            return (decimal)v;
        }
        else
          return DataTools.GetDecimal(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableDecimalExtractor : IDataRowExtractor<Decimal?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableDecimalExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public decimal? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (decimal)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDecimal(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// DataRowNullableDateTimeExtractor возвращает значение null, если поле содержит пустое значение,
  /// а DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  public struct DataRowDateTimeExtractor : IDataRowExtractor<DateTime>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowDateTimeExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public DateTime this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(DateTime))
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return DateTime.MinValue;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// DataRowNullableDateTimeExtractor возвращает значение null, если поле содержит пустое значение,
  /// а DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  public struct DataRowNullableDateTimeExtractor : IDataRowExtractor<DateTime?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableDateTimeExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public DateTime? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(DateTime)) // 28.11.2017
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return null;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowTimeSpanExtractor : IDataRowExtractor<TimeSpan>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowTimeSpanExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public TimeSpan this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(TimeSpan)) // испр. 5.12.2021
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return TimeSpan.Zero;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableTimeSpanExtractor : IDataRowExtractor<TimeSpan?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableTimeSpanExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public TimeSpan? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(TimeSpan))
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return null;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowStringExtractor : IDataRowExtractor<String>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowStringExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public string this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(String))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return String.Empty;
          else
            return ((string)v).Trim();
        }
        else
          return DataTools.GetString(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowBoolExtractor : IDataRowExtractor<Boolean>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowBoolExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public bool this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return false;
          else
            return (bool)v;
        }
        else
          return DataTools.GetBool(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableBoolExtractor : IDataRowExtractor<Boolean?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableBoolExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public bool? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (bool)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetBool(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowGuidExtractor : IDataRowExtractor<Guid>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowGuidExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public Guid this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Guid))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return Guid.Empty;
          else
            return (Guid)v;
        }
        else
          return DataTools.GetGuid(row[~_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableGuidExtractor : IDataRowExtractor<Guid?>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableGuidExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0), если для извлечения значения используется прямой доступ к полю,
    /// когда поле имеет подходящий тип.
    /// Если при извлечении значения требуется преобразование, то содержит отрицательное значение, 
    /// которое является двоичным сопряжением (оператор "~") индекса поля.
    /// Например, если поле имеет индекс 1, то для простого извлечения поля хранится значение 0x00000001,
    /// а для извлечения с преобразованием - 0xFFFFFFFE.
    /// Имеет смысл, только когда CurrentTable не null.
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public Guid? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Guid))
            _ColumnIndex = p;
          else
            _ColumnIndex = ~p;
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        if (_ColumnIndex >= 0)
        {
          object v = row[_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return (Guid)v;
        }
        else
        {
          object v = row[~_ColumnIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetGuid(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowEnumExtractor<T> : IDataRowExtractor<T>
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowEnumExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Не используется инвертированный индекс, как в других структурах
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля</returns>
    public T this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(int) || row.Table.Columns[p].DataType == typeof(string))
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        return DataTools.GetEnum<T>(row[_ColumnIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Извлечение значения поля с заданным именем из строк DataRow.
  /// Рекомендуется для использования, когда в цикле требуется извлекать значение одного поля по имени 
  /// из множества строк, при этом строки могут относиться к разным таблицам.
  /// При извлечении очередного значения проверяется, относится ли строка DataRow к той же DataTable, что
  /// и при предыдущем вызове. Если нет, то определяется индекс столбца DataColumn в таблице. 
  /// При последующих вызовах используется доступ по индексу поля, а не по имени, что квеличивает скорость
  /// извлечения значения.
  /// Не предназначено для работы с удаленными строками с RowState=Deleted или Detached.
  /// Как правило, структура используется как локальная переменная в пределах одного метода.
  /// </summary>
  public struct DataRowNullableEnumExtractor<T> : IDataRowExtractor<T?>
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DataRowNullableEnumExtractor(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif

      _ColumnName = columnName;
      // Требуется компилятору для конструктора структуры
      _CurrentTable = null;
      _ColumnIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля, из которого извлекаются значения
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Возвращает имя поля.
    /// </summary>
    /// <returns>Свойство ColumnName</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    /// <summary>
    /// Таблица, из которой посндний раз было извлечено значения.
    /// До первого извлечения содержит null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// Индекс поля (больший или равный 0).
    /// Не используется инвертированный индекс, как в других структурах
    /// </summary>
    private int _ColumnIndex;

    #endregion

    #region Извлечение значений

    /// <summary>
    /// Извлечение значения поля из строки.
    /// Если поле не содержит значения (DBNull), возвращается null
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <returns>Значение поля или null</returns>
    public T? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_ColumnName);
          if (p < 0)
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит столбца \"" + _ColumnName + "\"");
          if (row.Table.Columns[p].DataType == typeof(int) || row.Table.Columns[p].DataType == typeof(string))
            _ColumnIndex = p;
          else
            throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" содержит столбец \"" + _ColumnName + "\" неподходящего типа " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // присваиваем в последнюю очередь
        }

        object v = row[_ColumnIndex];
        if (v is DBNull)
          return null;
        else
          return DataTools.GetEnum<T>(v);
      }
    }

    #endregion
  }
}
