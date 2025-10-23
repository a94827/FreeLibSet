// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{

  /// <summary>
  /// Доступ к значениям одной строки <see cref="DataRow"/>.
  /// Не может иметь "серых" значений (реализация <see cref="IDBxExtValues.GetGrayed(int)"/> всегда возвращает false).
  /// Если предполагается обработка множества строк таблицы, используйте <see cref="DBxDataTableExtValues"/>.
  /// </summary>
  public struct DBxDataRowExtValues : IDBxExtValues
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа.
    /// </summary>
    /// <param name="row">Строка таблицы данных</param>
    public DBxDataRowExtValues(DataRow row)
      : this(row, null)
    {
    }

    /// <summary>
    /// Создает объект доступа.
    /// Эта перегрузка с индексатором столбцов предназначена, в основном, для внутреннего использования в библиотеке.
    /// </summary>
    /// <param name="row">Строка таблицы данных</param>
    /// <param name="columnNameIndexer">Индексатор имен полей. Если не задан, то будет создан автоматически при необходимости</param>
    public DBxDataRowExtValues(DataRow row, StringArrayIndexer columnNameIndexer)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
#endif

      _Row = row;
      _IsReadOnly = false;
      _ColumnNameIndexer = columnNameIndexer;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Строка данных
    /// </summary>
    public DataRow Row { get { return _Row; } }
    private readonly DataRow _Row;

    /// <summary>
    /// Режим "только чтение". По умолчанию - false.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } set { _IsReadOnly = value; } }
    private bool _IsReadOnly;

    /// <summary>
    /// См. описание в <see cref="DBxDataTableExtValues"/>.
    /// </summary>
    private /*readonly*/ StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Row.ToString() + (IsReadOnly ? " ReadOnly" : "");
    }

    #endregion

    #region IDBxExtValues Members

    /// <summary>
    /// Доступ к значению по имени поля.
    /// Если запрошено имя поля, которого нет в таблице, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Доступ к значению поля</returns>
    public DBxExtValue this[string name]
    {
      get
      {
        int index = IndexOf(name);
        if (index < 0)
          throw ExceptionFactory.ArgUnknownColumnName("name", _Row.Table, name);
        return new DBxExtValue(this, index);
      }
    }

    /// <summary>
    /// Возвращает имя поля <see cref="DataColumn.ColumnName"/> по индексу поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Имя поля</returns>
    public string GetName(int index)
    {
      return _Row.Table.Columns[index].ColumnName;
    }

    /// <summary>
    /// Возвращает отображаемое имя поля <see cref="DataColumn.Caption"/> по индексу поля.
    /// Если заголовок не задан, возвращается <see cref="DataColumn.ColumnName"/>.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Отображаемое имя поля</returns>
    public string GetDisplayName(int index)
    {
      string displayName = _Row.Table.Columns[index].Caption;
      if (String.IsNullOrEmpty(displayName))
        return GetName(index);
      else
        return displayName;
    }

    /// <summary>
    /// Поиск столбца по имени
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Индекс</returns>
    public int IndexOf(string name)
    {
      if (_ColumnNameIndexer == null)
      {
        string[] a = DataTools.GetColumnNames(_Row.Table);
        _ColumnNameIndexer = new StringArrayIndexer(a, true);
      }
      return _ColumnNameIndexer.IndexOf(name);
    }

    /// <summary>
    /// Доступ к значению по индексу поля.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Объект доступа к значениям</returns>
    public DBxExtValue this[int index]
    {
      get { return new DBxExtValue(this, index); }
    }

    /// <summary>
    /// Возвращает количество столбцов в таблице <see cref="Row"/>.Table.Columns.Count.
    /// </summary>
    public int Count
    {
      get { return _Row.Table.Columns.Count; }
    }

    int IDBxExtValues.RowCount { get { return 1; } }

    /// <summary>
    /// Получить значение поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <param name="preferredType">Не используется</param>
    /// <returns>Значение</returns>
    public object GetValue(int index, DBxExtValuePreferredType preferredType)
    {
      object v = _Row[index];

      // 08.12.2017
      // Если поле не разрешает DBNull, то дата 01.01.0001 заменяется на null
      if (v is DateTime && (!_Row.Table.Columns[index].AllowDBNull))
      {
        DateTime dt = (DateTime)v;
        if (dt == DateTime.MinValue)
          return DBNull.Value;
      }

      return v;
    }

    /// <summary>
    /// Установить значение поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <param name="value">Значение поля. Null заменяется на <see cref="DBNull"/></param>
    public void SetValue(int index, object value)
    {
      CheckNotReadOnly();
      if (value == null)
        _Row[index] = DBNull.Value; // 14.10.2015
      else
        _Row[index] = value;
    }

    /// <summary>
    /// Возвращает <see cref="DataRow.IsNull(int)"/>.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>True, если значение поля пустое</returns>
    public bool IsNull(int index)
    {
      return _Row.IsNull(index);
    }

    /// <summary>
    /// Возвращает <see cref="DataColumn.AllowDBNull"/>.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Допустимость пустых значений</returns>
    public bool AllowDBNull(int index)
    {
      return _Row.Table.Columns[index].AllowDBNull;
    }

    /// <summary>
    /// Возвращает <see cref="DataColumn.MaxLength"/>.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Маскимальная длина текстового поля</returns>
    public int MaxLength(int index)
    {
      return _Row.Table.Columns[index].MaxLength;
    }

    /// <summary>
    /// Возвращает true, если столбец предназначен только для просмотра (свойство <see cref="DataColumn.ReadOnly"/>).
    /// Если для текущего объкекта свойство <see cref="IsReadOnly"/>=true, то возвращается true независимо от значений для столбца.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (<see cref="Row"/>.Table.Columns.Count-1)</param>
    /// <returns>Признак "только чтение"</returns>
    public bool GetValueReadOnly(int index)
    {
      // лишнее
      //if (_IsReadOnly)
      //  return true;

      DataColumn col = _Row.Table.Columns[index];
      return col.ReadOnly || col.Expression.Length > 0; // 01.03.2022
    }

    bool IDBxExtValues.GetGrayed(int index)
    {
      return false;
    }

    object[] IDBxExtValues.GetValueArray(int index)
    {
      return new object[1] { _Row[index] };
    }

    void IDBxExtValues.SetValueArray(int index, object[] values)
    {
      if (values.Length != 1)
        throw new ArgumentException("values.Length must be 1", "values");
      _Row[index] = values[0];
    }

    object IDBxExtValues.GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      return GetValue(valueIndex, DBxExtValuePreferredType.Unknown);
    }

    void IDBxExtValues.SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      SetValue(valueIndex, value);
    }

    #endregion

    #region IEnumerable<DBxExtValue> Members

    /// <summary>
    /// Возвращает перечислитель по объектам <see cref="DBxExtValue"/>.
    /// Перебираются все столбцы таблицы.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxExtValueEnumerator GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// Значения свойства для столбцов <see cref="DataColumn.ReadOnly"/> не влияют на этот метод.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям полей таблицы в-целом, с поддержкой "серых" значений.
  /// Используется буферизация. При изменении значений полей отдельных строк (кроме установки значения с помощью <see cref="DBxDataTableExtValues.SetRowValue(int, int, object)"/>)или добавлении/удалении строк, 
  /// должен быть вызван метод <see cref="DBxDataTableExtValues.ResetBuffer()"/>.
  /// Доступ к значениям возможен, даже если таблица не содержит ни одной строки.
  /// Возможна последовательная обработка нескольких однотипных таблиц установкой свойства <see cref="DBxDataTableExtValues.Table"/>.
  /// </summary>
  public class DBxDataTableExtValues : IDBxExtValues
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа к таблице.
    /// У таблицы уже должны быть добавлены столбцы, дальнейшее изменение структуры таблицы не допускается.
    /// Строки могут добавляться или удаляться после вызова конструктора, с вызовом метода <see cref="ResetBuffer()"/>.
    /// </summary>
    /// <param name="table">Таблица</param>
    public DBxDataTableExtValues(DataTable table)
      : this(table, null)
    {
    }

    /// <summary>
    /// Создает объект доступа к таблице.
    /// У таблицы уже должны быть добавлены столбцы, дальнейшее изменение структуры таблицы не допускается.
    /// Строки могут добавляться или удаляться после вызова конструктора, с вызовом метода <see cref="ResetBuffer()"/>.
    /// Эта перегрузка с индексатором столбцов предназначена, в основном, для внутреннего использования в библиотеке.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNameIndexer">Индексатор имен полей. Если не задан, то будет создан автоматически при необходимости.</param>
    public DBxDataTableExtValues(DataTable table, StringArrayIndexer columnNameIndexer)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      _Table = table;

      _BufFlags = new bool[_Table.Columns.Count];
      _BufValues = new object[_Table.Columns.Count];
      _DummyRow = _Table.NewRow(); // добавлять в таблицу не будем

      _ColumnNameIndexer = columnNameIndexer;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица значений. Задается в конструкторе. Не может быть null.
    /// Свойство может быть установлено при условии, что новая таблица имеет такой же список и тип полей,
    /// что и первоначальная таблица.
    /// </summary>
    public DataTable Table
    {
      get { return _Table; }
      set
      {
        #region Проверка совпадения имен и типов полей

        if (value == null)
          throw new ArgumentNullException();

        if (!DataTools.AreColumnNamesEqual(_Table, value, false))
          throw new ArgumentException(String.Format(Res.DBxDataRowExtValues_Arg_TableDiff, value.TableName, _Table.TableName));

        // 13.06.2017 Убрано.
        // Так может быть: в первоначальной таблице поле имеет тип Byte, а в новой - Int16
        // for (int i = 0; i < value.Columns.Count; i++)
        // {
        //   DataColumn oldCol = _Table.Columns[i];
        //   DataColumn newCol = value.Columns[i];
        //   if (NewCol.DataType!=OldCol.DataType)
        //     throw new ArgumentException("Тип данных столбца с \""+NewCol.ColumnName+"\" в новой таблице \""+value.TableName+"\" (" + NewCol.DataType.ToString() + ") не совпадает с типом данных столбца в существующей таблице (" + OldCol.DataType.ToString()+ ")");
        // }

        #endregion

        _Table = value;
        // Сброс буфера
        ArrayTools.FillArray<bool>(_BufFlags, false);
        _DummyRow = _Table.NewRow(); // добавлять в таблицу не будем
      }
    }
    private DataTable _Table;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      string tableName;
      if (String.IsNullOrEmpty(_Table.TableName))
        tableName = "noname";
      else
        tableName = _Table.TableName;
      return tableName + ", RowCount=" + _Table.Rows.Count.ToString();
    }

    /// <summary>
    /// Индексатор столбцов по именам. 
    /// Создается автоматически при необходимости, или задается в конструкторе.
    /// Реализация в NetFramework метода <see cref="DataColumnCollection.IndexOf(string)"/> является медленной.
    /// Несмотря на наличие внутреннего словаря DataColumnCollection.columnFromName, после проверки факта наличия
    /// столбца выполняется обычный перебор столбцов в цикле. Перебор выполняется, даже если имя поля задано в правильном регистре.
    /// Индексатор обычно должен быть нерегистрочувствительным.
    /// </summary>
    private StringArrayIndexer _ColumnNameIndexer;
    // TODO: StringArrayIndexer не позволяет указывать локаль. По идее, в индексаторе надо использовать свойство DataTable.Locale.

    #endregion

    #region Буферизация значений

    /// <summary>
    /// Если флажок установлен, для соответствующего поля вычислено буферизованное значение
    /// </summary>
    private readonly bool[] _BufFlags;

    /// <summary>
    /// Буферизованные значениея для полей. null соответствует Grayed-значению
    /// Для вычисленных значений, если они пустые, используется DBNull или 0 или пустая строка
    /// </summary>
    private readonly object[] _BufValues;

    /// <summary>
    /// Получение "пустых" значений, когда таблица не содержит строк
    /// </summary>
    private DataRow _DummyRow;

    private void GetReadyBuffer(int index)
    {
      if (_BufFlags[index])
        return;

      if (_Table.Rows.Count == 0)
        _BufValues[index] = _DummyRow[index];
      else
      {
        _BufValues[index] = _Table.Rows[0][index];
        for (int i = 1; i < _Table.Rows.Count; i++)
        {
          if (!_BufValues[index].Equals(Table.Rows[i][index]))
          {
            _BufValues[index] = null;
            break;
          }
        }
      }

      // 08.12.2017
      // Если поле не разрешает DBNull, то дата 01.01.0001 заменяется на null
      if (_BufValues[index] is DateTime && (!_Table.Columns[index].AllowDBNull))
      {
        DateTime dt = (DateTime)(_BufValues[index]);
        if (dt == DateTime.MinValue)
          _BufValues[index] = DBNull.Value;
      }

      _BufFlags[index] = true;
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для всех столбцов
    /// </summary>
    public void ResetBuffer()
    {
      ArrayTools.FillArray<bool>(_BufFlags, false);
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для столбца с заданным индексом
    /// </summary>
    /// <param name="index">Индекс столбца в таблице</param>
    public void ResetBuffer(int index)
    {
      _BufFlags[index] = false;
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для столбца с заданным именем
    /// </summary>
    /// <param name="name">Имя столбца</param>
    public void ResetBuffer(string name)
    {
      int index = IndexOf(name);
      if (index < 0)
        throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);

      _BufFlags[index] = false;
    }

    #endregion

    #region IDBxExtValues Members

    /// <summary>
    /// Возвращает объект доступа к значениям поля.
    /// Если запрошено несуществующее имя столбца, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект доступа</returns>
    public DBxExtValue this[string name]
    {
      get
      {
        int index = IndexOf(name);
        if (index < 0)
          throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);
        return new DBxExtValue(this, index);
      }
    }

    /// <summary>
    /// Получить имя столбца по индексу (свойство <see cref="DataColumn.ColumnName"/>)
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Имя столбца</returns>
    public string GetName(int index)
    {
      return _Table.Columns[index].ColumnName;
    }

    /// <summary>
    /// Возвращает отображаемое имя столбца (свойство <see cref="DataColumn.Caption"/>).
    /// Если свойство не установлено, возвращается <see cref="DataColumn.ColumnName"/>.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Заголовок столбца</returns>
    public string GetDisplayName(int index)
    {
      string displayName = _Table.Columns[index].Caption;
      if (String.IsNullOrEmpty(displayName))
        return _Table.Columns[index].ColumnName;
      else
        return displayName;
    }

    /// <summary>
    /// Поиск столбца по имени
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Индекс столбца</returns>
    public int IndexOf(string name)
    {
      if (_ColumnNameIndexer == null)
      {
        string[] a = DataTools.GetColumnNames(_Table);
        _ColumnNameIndexer = new StringArrayIndexer(a, true);
      }
      return _ColumnNameIndexer.IndexOf(name);
    }

    /// <summary>
    /// Возвращает объект доступа к значениям поля.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Объект доступа</returns>
    public DBxExtValue this[int index]
    {
      get { return new DBxExtValue(this, index); }
    }

    /// <summary>
    /// Возвращает <see cref="Table"/>.Columns.Count
    /// </summary>
    public int Count
    {
      get { return _Table.Columns.Count; }
    }

    /// <summary>
    /// Возвращает количество строк в таблице <see cref="Table"/>.Rows.Count.
    /// </summary>
    public int RowCount
    {
      get { return _Table.Rows.Count; }
    }

    /// <summary>
    /// Свойство "только чтение" можно устанавливать произвольно
    /// </summary>
    public bool IsReadOnly
    {
      get { return _IsReadOnly; }
      set { _IsReadOnly = value; }
    }
    private bool _IsReadOnly;

    /// <summary>
    /// Получить значение.
    /// Если во всех строках таблицы находится одинаковое значение, оно возвращается.
    /// Если в таблице во всех строках находится пустое значение, возвращается <see cref="DBNull"/>.
    /// Если в таблице в строках находятся разные значения ("серое" значение, Grayed=true), возвращается null.
    /// Если таблица не содержит ни одной строки, возвращается <see cref="DBNull"/>.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="preferredType">Игнорируется</param>
    /// <returns>Значение поля.</returns>
    public object GetValue(int index, DBxExtValuePreferredType preferredType)
    {
      GetReadyBuffer(index);
      return _BufValues[index];
    }

    /// <summary>
    /// Установить одинаковое значение поля во все строки таблицы.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="value">Новое значение. Значение null заменяется на <see cref="DBNull"/>.</param>
    public void SetValue(int index, object value)
    {
      CheckNotReadOnly(); // 28.02.2022

      if (value == null)
        value = DBNull.Value; // 17.04.2017

      // Убираем флажок на случай вознкновения исключения при записи значения
      _BufFlags[index] = false;

      // Записываем значения во все строки
      for (int i = 0; i < _Table.Rows.Count; i++)
        _Table.Rows[i][index] = value;

      // Записываем общее значение
      _BufValues[index] = value;
      _BufFlags[index] = true;
    }

    /// <summary>
    /// Возвращает true, если значения поля во всех строках равны <see cref="DBNull"/>.
    /// Если значение поля "серое", возвращается true.
    /// Если таблица не содержит строк, возвращается true.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Наличие Null</returns>
    public bool IsNull(int index)
    {
      GetReadyBuffer(index);
      if (_BufValues[index] == null)
        return true;
      else
        return _BufValues[index] is DBNull;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.AllowDBNull"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Разрешение значения <see cref="DBNull"/> для поля</returns>
    public bool AllowDBNull(int index)
    {
      return _Table.Columns[index].AllowDBNull;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.MaxLength"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Максимальная длина текстового поля в символах</returns>
    public int MaxLength(int index)
    {
      return _Table.Columns[index].MaxLength;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.ReadOnly"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>true, если столбец доступен только для чтения</returns>
    public bool GetValueReadOnly(int index)
    {
      DataColumn col = _Table.Columns[index];
      return col.ReadOnly || col.Expression.Length > 0; // 01.03.2022
    }

    /// <summary>
    /// Возвращает true, если в разных строках значения поля не совпадают
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>True, если значение "серое"</returns>
    public bool GetGrayed(int index)
    {
      GetReadyBuffer(index);
      return _BufValues[index] == null;
    }

    #endregion

    #region Данные для отдельных строк

    /// <summary>
    /// Получение массива всех значений.
    /// Количество элементов равно количеству строк в таблице, независимо от наличия повторяющихся значений
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Массив</returns>
    public object[] GetValueArray(int index)
    {
      object[] a = new object[Table.Rows.Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = Table.Rows[i][index];
      return a;
    }

    /// <summary>
    /// Присвоение значения для всех строк.
    /// Длина массива должна быть равна количеству строк
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="values">Массив значений</param>
    public void SetValueArray(int index, object[] values)
    {
#if DEBUG
      if (values == null)
        throw new ArgumentNullException("values");
#endif
      if (values.Length != Table.Rows.Count)
        throw ExceptionFactory.ArgWrongCollectionCount("values", values, Table.Rows.Count);

      CheckNotReadOnly(); // 28.02.2022

      for (int i = 0; i < values.Length; i++)
        Table.Rows[i][index] = values[i];
      ResetBuffer(index);
    }


    /// <summary>
    /// Получить значение для одной из строк. Значение не может быть "серым".
    /// Так как свойство <see cref="RowCount"/> может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <returns></returns>
    public object GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex < 0 || rowIndex >= Table.Rows.Count)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, Table.Rows.Count - 1);

      object v = Table.Rows[rowIndex][valueIndex];
      return v;
    }

    /// <summary>
    /// Установить значение для одной из строк.
    /// Так как свойство <see cref="RowCount"/> может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до (<see cref="RowCount"/>-1)</param>
    /// <param name="value">Значение</param>
    public void SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex < 0 || rowIndex >= Table.Rows.Count)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, Table.Rows.Count - 1);

      CheckNotReadOnly(); // 28.02.2022

      if (value == null)
        value = DBNull.Value;
      Table.Rows[rowIndex][valueIndex] = value;
      ResetBuffer(valueIndex);
    }

    #endregion

    #region IEnumerable<DBxExtValue> Members

    /// <summary>
    /// Возвращает перечислитель по столбцам таблицы.
    /// Перечислитель получает объекты доступа <see cref="DBxExtValue"/> к значениям поля.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxExtValueEnumerator GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// Свойства столбцов <see cref="DataColumn.ReadOnly"/> не влияют на этот метод.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям полей выбраных строк таблицы, с поддержкой "серых" значений.
  /// Используется буферизация. При изменении значений полей отдельных строк (кроме установки значения с помощью <see cref="DBxDataTableExtValues.SetRowValue(int, int, object)"/>)или добавлении/удалении строк, 
  /// должен быть вызван метод <see cref="DBxDataRowArrayExtValues.ResetBuffer()"/>.
  /// Доступ к значениям возможен, даже если нет ни одной выбранной строки.
  /// </summary>
  public class DBxDataRowArrayExtValues : IDBxExtValues
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа к таблице.
    /// У таблицы уже должны быть добавлены столбцы, дальнейшее изменение структуры таблицы не допускается.
    /// Строки могут добавляться или удаляться после вызова конструктора, с вызовом метода <see cref="ResetBuffer()"/>.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="rows">Строки таблицы, из которых извлекаются данные. Может быть пустой список.</param>
    public DBxDataRowArrayExtValues(DataTable table, IEnumerable<DataRow> rows)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      _Table = table;
      _Rows = ArrayTools.CreateArray<DataRow>(rows);

      _BufFlags = new bool[_Table.Columns.Count];
      _BufValues = new object[_Table.Columns.Count];
      _DummyRow = _Table.NewRow(); // добавлять в таблицу не будем

      _ColumnNameIndexer = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица значений. Задается в конструкторе. Не может быть null.
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private readonly DataTable _Table;

    /// <summary>
    /// Выбранные строки таблицы, из которых извлекаются значения
    /// </summary>
    public DataRow[] Rows { get { return _Rows; } }
    private readonly DataRow[] _Rows;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      string tableName;
      if (String.IsNullOrEmpty(_Table.TableName))
        tableName = "noname";
      else
        tableName = _Table.TableName;
      return tableName + ", RowCount=" + Rows.Length.ToString();
    }

    /// <summary>
    /// Индексатор столбцов по именам. 
    /// Создается автоматически при необходимости.
    /// Реализация в NetFramework метода <see cref="DataColumnCollection.IndexOf(string)"/> является медленной.
    /// Несмотря на наличие внутреннего словаря DataColumnCollection.columnFromName, после проверки факта наличия
    /// столбца выполняется обычный перебор столбцов в цикле. Перебор выполняется, даже если имя поля задано в правильном регистре.
    /// Индексатор обычно должен быть нерегистрочувствительным.
    /// </summary>
    private StringArrayIndexer _ColumnNameIndexer;
    // TODO: StringArrayIndexer не позволяет указывать локаль. По идее, в индексаторе надо использовать свойство DataTable.Locale.

    #endregion

    #region Буферизация значений

    /// <summary>
    /// Если флажок установлен, для соответствующего поля вычислено буферизованное значение
    /// </summary>
    private readonly bool[] _BufFlags;

    /// <summary>
    /// Буферизованные значениея для полей. null соответствует Grayed-значению
    /// Для вычисленных значений, если они пустые, используется DBNull или 0 или пустая строка
    /// </summary>
    private readonly object[] _BufValues;

    /// <summary>
    /// Получение "пустых" значений, когда таблица не содержит строк
    /// </summary>
    private readonly DataRow _DummyRow;

    private void GetReadyBuffer(int index)
    {
      if (_BufFlags[index])
        return;

      if (Rows.Length == 0)
        _BufValues[index] = _DummyRow[index];
      else
      {
        _BufValues[index] = Rows[0][index];
        for (int i = 1; i < Rows.Length; i++)
        {
          if (!_BufValues[index].Equals(Rows[i][index]))
          {
            _BufValues[index] = null;
            break;
          }
        }
      }

      // 08.12.2017
      // Если поле не разрешает DBNull, то дата 01.01.0001 заменяется на null
      if (_BufValues[index] is DateTime && (!_Table.Columns[index].AllowDBNull))
      {
        DateTime dt = (DateTime)(_BufValues[index]);
        if (dt == DateTime.MinValue)
          _BufValues[index] = DBNull.Value;
      }

      _BufFlags[index] = true;
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для всех столбцов
    /// </summary>
    public void ResetBuffer()
    {
      ArrayTools.FillArray<bool>(_BufFlags, false);
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для столбца с заданным индексом
    /// </summary>
    /// <param name="index">Индекс столбца в таблице</param>
    public void ResetBuffer(int index)
    {
      _BufFlags[index] = false;
    }

    /// <summary>
    /// Сброс буферизации "серых" значений для столбца с заданным именем
    /// </summary>
    /// <param name="name">Имя столбца</param>
    public void ResetBuffer(string name)
    {
      int index = IndexOf(name);
      if (index < 0)
        throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);

      _BufFlags[index] = false;
    }

    #endregion

    #region IDBxExtValues Members

    /// <summary>
    /// Возвращает объект доступа к значениям поля.
    /// Если запрошено несуществующее имя столбца, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект доступа</returns>
    public DBxExtValue this[string name]
    {
      get
      {
        int index = IndexOf(name);
        if (index < 0)
          throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);
        return new DBxExtValue(this, index);
      }
    }

    /// <summary>
    /// Получить имя столбца по индексу (свойство <see cref="DataColumn.ColumnName"/>)
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Имя столбца</returns>
    public string GetName(int index)
    {
      return _Table.Columns[index].ColumnName;
    }

    /// <summary>
    /// Возвращает отображаемое имя столбца (свойство <see cref="DataColumn.Caption"/>).
    /// Если свойство не установлено, возвращается <see cref="DataColumn.ColumnName"/>.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Заголовок столбца</returns>
    public string GetDisplayName(int index)
    {
      string displayName = _Table.Columns[index].Caption;
      if (String.IsNullOrEmpty(displayName))
        return _Table.Columns[index].ColumnName;
      else
        return displayName;
    }

    /// <summary>
    /// Поиск столбца по имени
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Индекс столбца</returns>
    public int IndexOf(string name)
    {
      if (_ColumnNameIndexer == null)
      {
        string[] a = DataTools.GetColumnNames(_Table);
        _ColumnNameIndexer = new StringArrayIndexer(a, true);
      }
      return _ColumnNameIndexer.IndexOf(name);
    }

    /// <summary>
    /// Возвращает объект доступа к значениям поля.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Объект доступа</returns>
    public DBxExtValue this[int index]
    {
      get { return new DBxExtValue(this, index); }
    }

    /// <summary>
    /// Возвращает <see cref="Table"/>.Columns.Count
    /// </summary>
    public int Count
    {
      get { return _Table.Columns.Count; }
    }

    /// <summary>
    /// Возвращает количество строк в массиве <see cref="Rows"/>.
    /// </summary>
    public int RowCount
    {
      get { return _Rows.Length; }
    }

    /// <summary>
    /// Свойство "только чтение" можно устанавливать произвольно
    /// </summary>
    public bool IsReadOnly
    {
      get { return _IsReadOnly; }
      set { _IsReadOnly = value; }
    }
    private bool _IsReadOnly;

    /// <summary>
    /// Получить значение.
    /// Если во всех строках таблицы находится одинаковое значение, оно возвращается.
    /// Если в таблице во всех строках находится пустое значение, возвращается <see cref="DBNull"/>.
    /// Если в таблице в строках находятся разные значения ("серое" значение, Grayed=true), возвращается null.
    /// Если таблица не содержит ни одной строки, возвращается <see cref="DBNull"/>.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="preferredType">Игнорируется</param>
    /// <returns>Значение поля.</returns>
    public object GetValue(int index, DBxExtValuePreferredType preferredType)
    {
      GetReadyBuffer(index);
      return _BufValues[index];
    }

    /// <summary>
    /// Установить одинаковое значение поля во все строки таблицы.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="value">Новое значение. Значение null заменяется на <see cref="DBNull"/>.</param>
    public void SetValue(int index, object value)
    {
      CheckNotReadOnly(); // 28.02.2022

      if (value == null)
        value = DBNull.Value; // 17.04.2017

      // Убираем флажок на случай вознкновения исключения при записи значения
      _BufFlags[index] = false;

      // Записываем значения во все строки
      for (int i = 0; i < _Rows.Length; i++)
        _Rows[i][index] = value;

      // Записываем общее значение
      _BufValues[index] = value;
      _BufFlags[index] = true;
    }

    /// <summary>
    /// Возвращает true, если значения поля во всех строках равны <see cref="DBNull"/>.
    /// Если значение поля "серое", возвращается true.
    /// Если таблица не содержит строк, возвращается true.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Наличие Null</returns>
    public bool IsNull(int index)
    {
      GetReadyBuffer(index);
      if (_BufValues[index] == null)
        return true;
      else
        return _BufValues[index] is DBNull;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.AllowDBNull"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Разрешение значения <see cref="DBNull"/> для поля</returns>
    public bool AllowDBNull(int index)
    {
      return _Table.Columns[index].AllowDBNull;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.MaxLength"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Максимальная длина текстового поля в символах</returns>
    public int MaxLength(int index)
    {
      return _Table.Columns[index].MaxLength;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DataColumn.ReadOnly"/>
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>true, если столбец доступен только для чтения</returns>
    public bool GetValueReadOnly(int index)
    {
      DataColumn col = _Table.Columns[index];
      return col.ReadOnly || col.Expression.Length > 0; // 01.03.2022
    }

    /// <summary>
    /// Возвращает true, если в разных строках значения поля не совпадают
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>True, если значение "серое"</returns>
    public bool GetGrayed(int index)
    {
      GetReadyBuffer(index);
      return _BufValues[index] == null;
    }

    #endregion

    #region Данные для отдельных строк

    /// <summary>
    /// Получение массива всех значений.
    /// Количество элементов равно количеству строк в таблице, независимо от наличия повторяющихся значений
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <returns>Массив</returns>
    public object[] GetValueArray(int index)
    {
      object[] a = new object[Rows.Length];
      for (int i = 0; i < a.Length; i++)
        a[i] = Rows[i][index];
      return a;
    }

    /// <summary>
    /// Присвоение значения для всех строк.
    /// Длина массива должна быть равна количеству строк
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (<see cref="Table"/>.Columns.Count-1)</param>
    /// <param name="values">Массив значений</param>
    public void SetValueArray(int index, object[] values)
    {
#if DEBUG
      if (values == null)
        throw new ArgumentNullException("values");
#endif
      if (values.Length != Rows.Length)
        throw ExceptionFactory.ArgWrongCollectionCount("values", values, Rows.Length);

      CheckNotReadOnly(); // 28.02.2022

      for (int i = 0; i < values.Length; i++)
        Rows[i][index] = values[i];
      ResetBuffer(index);
    }


    /// <summary>
    /// Получить значение для одной из строк. Значение не может быть "серым".
    /// Так как свойство <see cref="RowCount"/> может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <returns></returns>
    public object GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex < 0 || rowIndex >= Table.Rows.Count)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, Table.Rows.Count - 1);

      object v = Rows[rowIndex][valueIndex];
      return v;
    }

    /// <summary>
    /// Установить значение для одной из строк.
    /// Так как свойство <see cref="RowCount"/> может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до (<see cref="RowCount"/>-1)</param>
    /// <param name="value">Значение</param>
    public void SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex < 0 || rowIndex >= Table.Rows.Count)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, Table.Rows.Count - 1);

      CheckNotReadOnly(); // 28.02.2022

      if (value == null)
        value = DBNull.Value;
      Rows[rowIndex][valueIndex] = value;
      ResetBuffer(valueIndex);
    }

    #endregion

    #region IEnumerable<DBxExtValue> Members

    /// <summary>
    /// Возвращает перечислитель по столбцам таблицы.
    /// Перечислитель получает объекты доступа <see cref="DBxExtValue"/> к значениям поля.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxExtValueEnumerator GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// Свойства столбцов <see cref="DataColumn.ReadOnly"/> не влияют на этот метод.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion
  }
}
