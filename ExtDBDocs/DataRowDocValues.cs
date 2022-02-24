// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Доступ к значениям одной строки DataRow.
  /// Не может иметь "серых" значений
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDocValues : IDBxDocValues
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа
    /// </summary>
    /// <param name="row">Строка таблицы данных</param>
    /// <param name="isReadOnly">Если true, то разрешено только чтение значений</param>
    /// <param name="columnNameIndexer">Индексатор имен полей. Если не задан, то будет создан автоматически при необходимости</param>
    public DataRowDocValues(DataRow row, bool isReadOnly, StringArrayIndexer columnNameIndexer)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
#endif

      _Row = row;
      _IsReadOnly = isReadOnly;
      _ColumnNameIndexer = columnNameIndexer;
    }

    /// <summary>
    /// Создает объект доступа
    /// </summary>
    /// <param name="row">Строка таблицы данных</param>
    /// <param name="isReadOnly">Если true, то разрешено только чтение значений</param>
    public DataRowDocValues(DataRow row, bool isReadOnly)
      :this(row, isReadOnly, null)
    {
    }

    /// <summary>
    /// Создает объект доступа
    /// </summary>
    /// <param name="row">Строка таблицы данных</param>
    public DataRowDocValues(DataRow row)
      : this(row, false, null)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Строка данных
    /// </summary>
    public DataRow Row { get { return _Row; } }
    private readonly DataRow _Row;

    /// <summary>
    /// Режим "только чтение". Задается в конструкторе
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Row.ToString() + (IsReadOnly ? " ReadOnly" : "");
    }

    #endregion

    #region IDBxDocValues Members

    /// <summary>
    /// Доступ к значению по имени поля.
    /// Если запрошено имя поля, которого нет в таблице, генерируется исключение
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Доступ к значению поля</returns>
    public DBxDocValue this[string name]
    {
      get
      {
        int Index = IndexOf(name);
        if (Index < 0)
          throw new ArgumentException("Таблица \"" + _Row.Table.TableName + "\" не содержит поля \"" + name + "\"");
        return new DBxDocValue(this, Index);
      }
    }

    /// <summary>
    /// Возвращает имя поля DataColumn.ColumnName по индексу поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Имя поля</returns>
    public string GetName(int index)
    {
      return _Row.Table.Columns[index].ColumnName;
    }

    /// <summary>
    /// Возвращает отображаемое имя поля DataColumn.Caption по индексу поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Отображаемое имя поля</returns>
    public string GetDisplayName(int index)
    {
      string DisplayName = _Row.Table.Columns[index].Caption;
      if (String.IsNullOrEmpty(DisplayName))
        return GetName(index);
      else
        return DisplayName;
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
    /// Доступ к значению по индексу поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Объект доступа к значениям</returns>
    public DBxDocValue this[int index]
    {
      get { return new DBxDocValue(this, index); }
    }

    /// <summary>
    /// Возвращает количество столбцов в таблице Row.Table.Columns.Count.
    /// </summary>
    public int Count
    {
      get { return _Row.Table.Columns.Count; }
    }

    int IDBxDocValues.RowCount
    {
      get { return 1; }
    }

    /// <summary>
    /// Получить значение поля
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <param name="preferredType">Не используется</param>
    /// <returns>Значение</returns>
    public object GetValue(int index, DBxDocValuePreferredType preferredType)
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
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <param name="value">Значение поля. Null заменяется на DBNull</param>
    public void SetValue(int index, object value)
    {
      if (value == null)
        _Row[index] = DBNull.Value; // 14.10.2015
      else
        _Row[index] = value;
    }

    /// <summary>
    /// Возвращает DataRow.IsNull()
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>True, если значение поля пустое</returns>
    public bool IsNull(int index)
    {
      return _Row.IsNull(index);
    }

    /// <summary>
    /// Возвращает DataColumn.AllowDBNull
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Допустимость пустых значений</returns>
    public bool AllowDBNull(int index)
    {
      return _Row.Table.Columns[index].AllowDBNull;
    }

    /// <summary>
    /// Возвращает DataColumn.MaxLength
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Маскимальная длина текстового поля</returns>
    public int MaxLength(int index)
    {
      return _Row.Table.Columns[index].MaxLength;
    }

    /// <summary>
    /// Возвращает true, если столбец предназначен только для просмотра (свойство DataColumn.ReadOnly).
    /// Если IsReadOnly=true, то возвращается true.
    /// </summary>
    /// <param name="index">Индекс столбца в диапазоне от 0 до (Row.Table.Columns.Count-1)</param>
    /// <returns>Признак "только чтение"</returns>
    public bool GetValueReadOnly(int index)
    {
      if (_IsReadOnly)
        return true;

      return _Row.Table.Columns[index].ReadOnly;
    }

    bool IDBxDocValues.GetGrayed(int index)
    {
      return false;
    }

    object[] IDBxDocValues.GetValueArray(int index)
    {
      return new object[1] { _Row[index] };
    }

    void IDBxDocValues.SetValueArray(int index, object[] values)
    {
      if (values.Length != 1)
        throw new ArgumentException("values.Length must be 1", "values");
      _Row[index] = values[0];
    }

    object IDBxDocValues.GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      return GetValue(valueIndex, DBxDocValuePreferredType.Unknown);
    }

    void IDBxDocValues.SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      SetValue(valueIndex, value);
    }


    #endregion

    #region IEnumerable<DBxDocValue> Members

    /// <summary>
    /// Возвращает перечислитель по объектам DBxDocValue.
    /// Перебираются все столбцы таблицы
    /// </summary>
    /// <returns></returns>
    public DBxDocValueEnumerator GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    IEnumerator<DBxDocValue> IEnumerable<DBxDocValue>.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям полей таблицы в-целом, с поддержкой "серых" значений
  /// Используется буферизация. При изменении значений полей отдельных строк или добавлении/удалении строк, 
  /// должен быть вызван метод ResetBuffer()
  /// Доступ к значениям возможен, даже если таблица не содержит ни одной строки
  /// </summary>
  public class DataTableDocValues : IDBxDocValues
  {
    #region Конструктор

    /// <summary>
    /// Создает объект доступа к таблице.
    /// У таблицы уже должны быть добавлены столбцы и могут быть добавлены строки
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNameIndexer">Индексатор имен полей. Если не задан, то будет создан автоматически при необходимости</param>
    public DataTableDocValues(DataTable table, StringArrayIndexer columnNameIndexer)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      _Table = table;

      _BufFlags = new bool[_Table.Columns.Count];
      _BufValues = new object[_Table.Columns.Count];
      _DummyRow = _Table.NewRow(); // добавлять в таблицу не будем

      _ColumnNameIndexer = columnNameIndexer;
    }

    /// <summary>
    /// Создает объект доступа к таблице.
    /// У таблицы уже должны быть добавлены столбцы и могут быть добавлены строки
    /// </summary>
    /// <param name="table">Таблица</param>
    public DataTableDocValues(DataTable table)
      :this(table, null)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица значений. Задается в конструкторе. Не может быть null
    /// Свойство может быть установлено, при условии, что новая таблица имеет такой же список и тип полей,
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
        if (value.Columns.Count != _Table.Columns.Count)
          throw new ArgumentException("Количество полей в новой таблице \"" + value.TableName + "\" (" + value.Columns.Count + ") должно совпадать с существующей таблицей (" + _Table.Columns.Count + ")");
        for (int i = 0; i < value.Columns.Count; i++)
        {
          DataColumn OldCol = _Table.Columns[i];
          DataColumn NewCol = value.Columns[i];
          if (!String.Equals(OldCol.ColumnName, NewCol.ColumnName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Имя столбца с индексом " + i.ToString() + " в новой таблице \"" + value.TableName + "\" (" + NewCol.ColumnName + ") не совпадает с именем столбца в существующей таблице (" + OldCol.ColumnName + ")");

          // 13.06.2017 Убрано.
          // Так может быть: в первоначальной таблице поле имеет тип Byte, а в новой - Int16
          //if (NewCol.DataType!=OldCol.DataType)
          //  throw new ArgumentException("Тип данных столбца с \""+NewCol.ColumnName+"\" в новой таблице \""+value.TableName+"\" (" + NewCol.DataType.ToString() + ") не совпадает с типом данных столбца в существующей таблице (" + OldCol.DataType.ToString()+ ")");
        }

        #endregion

        _Table = value;
        // Сброс буфера
        DataTools.FillArray<bool>(_BufFlags, false);
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
      string TableName;
      if (String.IsNullOrEmpty(_Table.TableName))
        TableName = "Без имени";
      else
        TableName = _Table.TableName;
      return TableName + ", RowCount=" + _Table.Rows.Count.ToString();
    }

    private StringArrayIndexer _ColumnNameIndexer;

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
      DataTools.FillArray<bool>(_BufFlags, false);
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
      int Index = IndexOf(name);
      if (Index < 0)
        throw new ArgumentException("Таблица \"" + _Table.TableName + "\" не содержит столбца \"" + name + "\"");

      _BufFlags[Index] = false;
    }

    #endregion

    #region IDBxDocValues Members

    /// <summary>
    /// Возвращает объект доступа к значениям поля.
    /// Если запрошено несуществующее имя столбца, генерируется исключение
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект доступа</returns>
    public DBxDocValue this[string name]
    {
      get
      {
        int Index = IndexOf(name);
        if (Index < 0)
          throw new ArgumentException("Таблица \"" + _Table.TableName + "\" не содержит столбца \"" + name + "\"", "name");
        return new DBxDocValue(this, Index);
      }
    }

    /// <summary>
    /// Получить имя столбца по индексу (свойство DataColumn.ColumnName)
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Имя столбца</returns>
    public string GetName(int index)
    {
      return _Table.Columns[index].ColumnName;
    }

    /// <summary>
    /// Возвращает отображаемое имя столбца (свойство DataColumn.Caption)
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Заголовок столбца</returns>
    public string GetDisplayName(int index)
    {
      string DisplayName = _Table.Columns[index].Caption;
      if (String.IsNullOrEmpty(DisplayName))
        return _Table.Columns[index].ColumnName;
      else
        return DisplayName;
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
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Объект доступа</returns>
    public DBxDocValue this[int index]
    {
      get { return new DBxDocValue(this, index); }
    }

    /// <summary>
    /// Возвращает DataTable.Columns.Count
    /// </summary>
    public int Count
    {
      get { return _Table.Columns.Count; }
    }

    /// <summary>
    /// Возвращает количество строк в таблице
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
    /// Получить значение
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <param name="preferredType">Игнорируется</param>
    /// <returns>Значение поля. Для "серого" значения возвращается null</returns>
    public object GetValue(int index, DBxDocValuePreferredType preferredType)
    {
      GetReadyBuffer(index);
      return _BufValues[index];
    }

    /// <summary>
    /// Установить значение
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <param name="value">Новое значение. Значение null заменяется на DBNull.</param>
    public void SetValue(int index, object value)
    {
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
    /// Возвращает true, если значения поля во всех строках равны DBNull.
    /// Если значение поля "серое", возвращается true.
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
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
    /// Возвращает свойство DataColumn.AllowDBNull
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Разрешение значения DBNull для поля</returns>
    public bool AllowDBNull(int index)
    {
      return _Table.Columns[index].AllowDBNull;
    }

    /// <summary>
    /// Возвращает свойство DataColumn.MaxLength
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Максимальная длина текстового поля в символах</returns>
    public int MaxLength(int index)
    {
      return _Table.Columns[index].MaxLength;
    }

    /// <summary>
    /// Возвращает свойство DataColumn.ReadOnly
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>true, если столбец доступен только для чтения</returns>
    public bool GetValueReadOnly(int index)
    {
      return _Table.Columns[index].ReadOnly;
    }

    /// <summary>
    /// Возвращает true, если в разных строках значения поля не совпадают
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>True, если значение "серое"</returns>
    public bool GetGrayed(int index)
    {
      GetReadyBuffer(index);
      return _BufValues[index] == null;
    }

    #endregion

    #region Данные для отдельных строк

    /// <summary>
    /// Получение массива всех значений
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <returns>Массив</returns>
    public object[] GetValueArray(int index)
    {
      object[] a = new object[Table.Rows.Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = Table.Rows[i][index];
      return a;
    }

    /// <summary>
    /// Присвоение значения для всех строк
    /// </summary>
    /// <param name="index">Индекс столбца от 0 до (Table.Columns.Count-1)</param>
    /// <param name="values">Массив значений</param>
    public void SetValueArray(int index, object[] values)
    {
#if DEBUG
      if (values == null)
        throw new ArgumentNullException("values");
#endif
      if (values.Length != Table.Rows.Count)
        throw new ArgumentException("Длина массива должна быть равна " + RowCount.ToString(), "values");
      for (int i = 0; i < values.Length; i++)
        Table.Rows[i][index] = values[i];
      ResetBuffer();
    }


    /// <summary>
    /// Получить значение для одной из строк. Значение не может быть "серым".
    /// Так как свойство RowCount может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <returns></returns>
    public object GetRowValue(int valueIndex, int rowIndex)
    { 
      if (rowIndex<0||rowIndex>=Table.Rows.Count)
        throw new ArgumentOutOfRangeException("rowIndex");

      object v = Table.Rows[rowIndex][valueIndex];
      return v; 
    }

    /// <summary>
    /// Установить значение для одной из строк.
    /// Так как свойство RowCount может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <param name="value">Значение</param>
    public void SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex < 0 || rowIndex >= Table.Rows.Count)
        throw new ArgumentOutOfRangeException("rowIndex");
      if (value == null)
        value = DBNull.Value;
      Table.Rows[rowIndex][valueIndex] = value;
    }


    #endregion

    #region IEnumerable<DBxDocValue> Members

    /// <summary>
    /// Возвращает перечислитель по столбцам таблицы.
    /// Перечислитель получает объекты доступа DBxDocValue к значениям поля.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxDocValueEnumerator GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    IEnumerator<DBxDocValue> IEnumerable<DBxDocValue>.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}
