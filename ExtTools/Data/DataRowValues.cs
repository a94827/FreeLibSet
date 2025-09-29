// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Интерфейс для чтения значений полей из строк данных.
  /// Содержит коллекцию <see cref="IDataColumnValue"/>.
  /// </summary>
  public interface IDataRowValues : INamedValuesAccess
  {
    #region Свойства

    /// <summary>
    /// Доступ к объекту, который выполняет чтение значений из заданного поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Объект для доступа к значениям</returns>
    IDataColumnValue this[string columnName] { get; }

    #endregion
  }

  /// <summary>
  /// Интефрейс для доступа к значению поля
  /// </summary>
  public interface IDataColumnValue
  {
    #region Свойства и методы

    /// <summary>
    /// Имя поля
    /// </summary>
    string ColumnName { get; }

    /// <summary>
    /// Возвращает неформатированное значение
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Возвращает true, если поле не содержит значения
    /// </summary>
    bool IsNull { get; }

    /// <summary>
    /// Возвращает значение поля как строку
    /// </summary>
    string AsString { get; }

    /// <summary>
    /// Возвращает значение поля как целое число
    /// </summary>
    Int32 AsInt32 { get; }

    /// <summary>
    /// Возвращает значение поля как целое число
    /// </summary>
    Int32? AsNullableInt32 { get; }

    /// <summary>
    /// Возвращает значение поля как целое число
    /// </summary>
    Int64 AsInt64 { get; }

    /// <summary>
    /// Возвращает значение поля как целое число
    /// </summary>
    Int64? AsNullableInt64 { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    float AsSingle { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    float? AsNullableSingle { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    double AsDouble { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    double? AsNullableDouble { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    decimal AsDecimal { get; }

    /// <summary>
    /// Возвращает значение поля как число с плавающей точкой
    /// </summary>
    decimal? AsNullableDecimal { get; }

    /// <summary>
    /// Возвращает значение поля как логическое значение
    /// </summary>
    bool AsBoolean { get; }

    /// <summary>
    /// Возвращает значение поля как дату/время
    /// </summary>
    DateTime AsDateTime { get; }

    /// <summary>
    /// Возвращает значение поля как дату/время
    /// </summary>
    DateTime? AsNullableDateTime { get; }

    /// <summary>
    /// Возвращает значение поля как интервал времени
    /// </summary>
    TimeSpan AsTimeSpan { get; }

    /// <summary>
    /// Возвращает значение поля как GUID
    /// </summary>
    Guid AsGuid { get; }

    /// <summary>
    /// Возвращает значение поля как перечислимое значение
    /// </summary>
    /// <typeparam name="T">Тип перечислимого значения</typeparam>
    /// <returns>Значение поля</returns>
    T GetEnum<T>() where T : struct;

    #endregion
  }

  /// <summary>
  /// Доступ к значениям полей строки
  /// </summary>
  public class DataRowValues : IDataRowNamedValuesAccess, IDataRowValues
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта без текущей строки
    /// </summary>
    public DataRowValues()
    {
      _ColumnNameIndexer = null;
      _ColumnValues = EmptyArray<DataColumnValue>.Empty;
    }

    #endregion

    #region Текущая строка

    /// <summary>
    /// Текущая строка таблицы, из которой извлекаются или куда записываются значения.
    /// При установке свойства выбирается подходящее значение <see cref="RowVersion"/>.
    /// Чтобы выбрать нестандартную версию данных строки <see cref="System.Data.DataRowVersion"/>,
    /// используйте метод <see cref="SetCurrentRow(DataRow, DataRowVersion)"/>.
    /// Если устанавливаемая строка относится к другой таблице данных, чем текущая таблица <see cref="Table"/>,
    /// то свойство <see cref="Table"/> также устанавливается.
    /// </summary>
    public DataRow CurrentRow
    {
      get { return _CurrentRow; }
      set
      {
        if (value == null)
          SetCurrentRow(null, DataRowVersion.Default);
        else
        {
          if (value.RowState == DataRowState.Deleted)
            SetCurrentRow(value, DataRowVersion.Original);
          else
            SetCurrentRow(value, DataRowVersion.Default);
        }
      }
    }
    private DataRow _CurrentRow;

    /// <summary>
    /// Текущая версия данных строки <see cref="CurrentRow"/>.
    /// Чтобы выбрать определенную версию, используйте метод <see cref="SetCurrentRow(DataRow, DataRowVersion)"/>.
    /// </summary>
    public DataRowVersion RowVersion { get { return _RowVersion; } }
    private DataRowVersion _RowVersion;

    /// <summary>
    /// Выбирает текущую строку <see cref="CurrentRow"/> и версию данных <see cref="RowVersion"/>.
    /// Если устанавливаемая строка относится к другой таблице данных, чем текущая таблица <see cref="Table"/>,
    /// то свойство <see cref="Table"/> также устанавливается.
    /// </summary>
    /// <param name="row">Текущая строка. Может быть null</param>
    /// <param name="version">Версия данных. Доступные значения зависят <see cref="System.Data.DataRow.RowState"/>.</param>
    public void SetCurrentRow(DataRow row, DataRowVersion version)
    {
      if (row != null)
        Table = row.Table;
      _CurrentRow = row;
      _RowVersion = version;
    }

    #endregion

    #region Свойство Table

    /// <summary>
    /// Текущая таблица данных.
    /// Свойство может быть установлено до <see cref="CurrentRow"/>, чтобы можно было обращаться к столбцам <see cref="DataColumnValue"/>.
    /// Классы-наследники <see cref="DataTableValues"/> и <see cref="DataViewValues"/> устанавливают свойство в конструторе.
    /// Допускается значение null.
    /// </summary>
    public DataTable Table
    {
      get { return _Table; }
      set
      {
        if (Object.ReferenceEquals(value, _Table))
          return;

        if (TableIsFixed)
          throw ExceptionFactory.ObjectProperty(this, "TableIsFixed", true, null);

        _Table = value;
        _ColumnNameIndexer = null;
        if (value == null)
          _ColumnValues = EmptyArray<DataColumnValue>.Empty;
        else
          Array.Resize<DataColumnValue>(ref _ColumnValues, value.Columns.Count);
      }
    }
    private DataTable _Table;

    /// <summary>
    /// Запрет изменения таблицы для производных классов <see cref="DataTableValues"/> и <see cref="DataViewValues"/> 
    /// </summary>
    public bool TableIsFixed
    {
      get { return _TableIsFixed; }
      protected set { _TableIsFixed = value; }
    }
    private bool _TableIsFixed;

    #endregion

    #region Доступ к DataColumnValue

    private DataColumnValue[] _ColumnValues;

    /// <summary>
    /// Индексатор имен полей.
    /// Обычно не используется в прикладном коде.
    /// </summary>
    public StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
        {
          string[] columnNames;
          if (_Table == null)
            columnNames = EmptyArray<string>.Empty;
          else
            columnNames = DataTools.GetColumnNames(_Table);
          _ColumnNameIndexer = new StringArrayIndexer(columnNames, true);
        }
        return _ColumnNameIndexer;
      }
    }
    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// Доступ к данным столбцов по индексу.
    /// Требуется, чтобы свойство <see cref="Table"/> было установлено.
    /// </summary>
    /// <param name="columnIndex">Индекс столбца в <see cref="System.Data.DataColumnCollection"/></param>
    /// <returns>Объект <see cref="DataColumnValue"/></returns>
    public DataColumnValue this[int columnIndex]
    {
      get
      {
#if DEBUG
        if (columnIndex < 0 || columnIndex >= _ColumnValues.Length)
          throw ExceptionFactory.ArgOutOfRange("columnIndex", columnIndex, 0, _ColumnValues.Length - 1);
#endif
        if (_ColumnValues[columnIndex] == null)
          _ColumnValues[columnIndex] = new DataColumnValue(this, columnIndex);
        return _ColumnValues[columnIndex];
      }
    }

    /// <summary>
    /// Доступ к данным столбцов по имени столбца.
    /// Требуется, чтобы свойство <see cref="Table"/> было установлено.
    /// Если задано неверное имя столбца, выбрасывается исключение.
    /// </summary>
    /// <param name="columnName">Имя столбца в <see cref="System.Data.DataColumnCollection"/>.
    /// Имя не чувствительно к регистру</param>
    /// <returns>Объект <see cref="DataColumnValue"/></returns>
    public DataColumnValue this[string columnName]
    {
      get
      {
        int columnIndex = ColumnNameIndexer.IndexOf(columnName);
        if (columnIndex < 0)
        {
          if (Table == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Table");

          if (String.IsNullOrEmpty(columnName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
          else
            throw ExceptionFactory.ArgUnknownColumnName("columnName", Table, columnName);
        }
        return this[columnIndex];
      }
    }

    IDataColumnValue IDataRowValues.this[string columnName]
    {
      get { return this[columnName]; }
    }

    #endregion

    #region INamedValuesAccess

    /// <summary>
    /// Получить имена столбцов таблицы <see cref="Table"/>. Если свойство не установлено, то возвращается пустой массив.
    /// См. <see cref="DataTools.GetColumnNames(DataTable)"/>.
    /// </summary>
    /// <returns>Имена столбцов</returns>
    public string[] GetNames()
    {
      if (_Table == null)
        return EmptyArray<string>.Empty;
      else
        return DataTools.GetColumnNames(_Table);
    }

    bool INamedValuesAccess.Contains(string name)
    {
      if (_Table == null)
        return false;
      else
        return _Table.Columns.Contains(name);
    }

    /// <summary>
    /// Возвращает неформатированное значение поля строки <see cref="CurrentRow"/>.
    /// Если строка не выбрана, возвращается null (а не <see cref="DBNull"/>).
    /// Обычно для доступа к значениям следует использовать объекты <see cref="DataColumnValue"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object GetValue(string name)
    {
      if (_CurrentRow == null)
        return null;
      else
        return _CurrentRow[name, _RowVersion];
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значению поля для строки <see cref="DataRowValues"/>.
  /// Для доступа к объектам используйте индексированное по имени или индексу поля значение из <see cref="DataRowValues"/>.
  /// </summary>
  public sealed class DataColumnValue : IDataColumnValue
  {
    #region Защищенный конструктор

    internal DataColumnValue(DataRowValues owner, int columnIndex)
    {
      _Owner = owner;
      _ColumnIndex = columnIndex;
    }

    #endregion

    #region Общие свойства

    private readonly DataRowValues _Owner;
    private readonly int _ColumnIndex;

    /// <summary>
    /// Возвращает имя поля, к которому относится объект.
    /// Если свойство <see cref="DataRowValues.Table"/> не установлено, возвращается null.
    /// Если в процессе работы свойство <see cref="DataRowValues.Table"/> измненилось и выбрана таблица с другой структурой,
    /// объект может начать возвращать не те значения, которые ожидались.
    /// </summary>
    public string ColumnName
    {
      get
      {
        if (_Owner.Table == null)
          return String.Empty;
        else if (_Owner.Table.Columns.Count <= _ColumnIndex)
          return String.Empty;
        else
          return _Owner.Table.Columns[_ColumnIndex].ColumnName;
      }
    }

    internal DataColumn Column { get { return _Owner.Table.Columns[_ColumnIndex]; } }

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = "ColumnIndex=" + _ColumnIndex.ToString();
      string nm = ColumnName;
      if (!String.IsNullOrEmpty(nm))
        s += ", ColumnName=\"" + nm + "\"";
      return s;
    }

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Неформатированное значение.
    /// Если <see cref="DataRowValues.CurrentRow"/>=null, то возвращается null, а не <see cref="DBNull"/>.
    /// При установке значения null оно заменяется на <see cref="DBNull"/>.
    /// </summary>
    public object Value
    {
      get
      {
        if (_Owner.CurrentRow == null)
          return null;
        else
          return _Owner.CurrentRow[_ColumnIndex, _Owner.RowVersion];
      }
      set
      {
        if (value == null)
          value = DBNull.Value;
        _Owner.CurrentRow[_ColumnIndex] = value;
      }
    }

    /// <summary>
    /// Возвращает true, если значение поля текущей строки имеет значение <see cref="DBNull"/>.
    /// Также возвращает true, если <see cref="DataRowValues.CurrentRow"/>=null.
    /// </summary>
    public bool IsNull
    {
      get
      {
        if (_Owner.CurrentRow == null)
          return true;
        else
          return _Owner.CurrentRow.IsNull(_Owner.Table.Columns[_ColumnIndex], _Owner.RowVersion);
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public string AsString
    {
      get { return DataTools.GetString(Value); }
      set
      {
        if (_Owner.CurrentRow == null)
          throw ExceptionFactory.ObjectPropertyNotSet(_Owner, "CurrentRow");

        if (String.IsNullOrEmpty(value))
          DoClearColumnValue();
        else
        {
          DataColumn col = Column;
          object value2 = value;
          if (col.DataType == typeof(string) || col.DataType == null)
          {
            int l = col.MaxLength;
            if (l >= 0 && l < value.Length)
              value2 = value.Substring(0, l);
          }
          else
            value2 = StdConvert.ChangeType(value, col.DataType);
          _Owner.CurrentRow[_ColumnIndex] = value2;
        }
      }
    }

    private void DoClearColumnValue()
    {
      DataColumn col = Column;
      if (col.AllowDBNull)
        _Owner.CurrentRow[_ColumnIndex] = DBNull.Value;
      else
      {
        object v0 = DataTools.GetEmptyValue(col.DataType);
        if (v0 == null)
          v0 = String.Empty;
        _Owner.CurrentRow[_ColumnIndex] = v0;
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public int AsInt32
    {
      get { return DataTools.GetInt32(Value); }
      set
      {
        if (value == 0)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public int? AsNullableInt32
    {
      get { return DataTools.GetNullableInt32(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public long AsInt64
    {
      get { return DataTools.GetInt64(Value); }
      set
      {
        if (value == 0L)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public long? AsNullableInt64
    {
      get { return DataTools.GetNullableInt64(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public float AsSingle
    {
      get { return DataTools.GetSingle(Value); }
      set
      {
        if (value == 0f)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public float? AsNullableSingle
    {
      get { return DataTools.GetNullableSingle(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public double AsDouble
    {
      get { return DataTools.GetDouble(Value); }
      set
      {
        if (value == 0.0)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public double? AsNullableDouble
    {
      get { return DataTools.GetNullableDouble(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public decimal AsDecimal
    {
      get { return DataTools.GetDecimal(Value); }
      set
      {
        if (value == 0m)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public decimal? AsNullableDecimal
    {
      get { return DataTools.GetNullableDecimal(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public bool AsBoolean
    {
      get { return DataTools.GetBoolean(Value); }
      set
      {
        if (value == false)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public DateTime AsDateTime
    {
      get { return DataTools.GetDateTime(Value); }
      set
      {
        if (value == DateTime.MinValue)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public DateTime? AsNullableDateTime
    {
      get { return DataTools.GetNullableDateTime(Value); }
      set
      {
        if (value.HasValue)
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
        else
          DoClearColumnValue();
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public TimeSpan AsTimeSpan
    {
      get { return DataTools.GetTimeSpan(Value); }
      set
      {
        if (value == TimeSpan.Zero)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить или записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public Guid AsGuid
    {
      get { return DataTools.GetGuid(Value); }
      set
      {
        if (value == Guid.Empty)
          DoClearColumnValue();
        else
        {
          Type t = Column.DataType ?? typeof(string);
          _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
        }
      }
    }

    /// <summary>
    /// Получить форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public T GetEnum<T>()
      where T : struct
    {
      return DataTools.GetEnum<T>(Value);
    }

    /// <summary>
    /// Записать форматированное значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/>.
    /// </summary>
    public void SetEnum<T>(T value)
      where T : struct
    {
      if (value.Equals(default(T)))
        DoClearColumnValue();
      else
      {
        Type t = Column.DataType ?? typeof(string);
        _Owner.CurrentRow[Column] = StdConvert.ChangeType(value, t);
      }
    }

    #endregion

    #region Инкремент значений

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на указанное значение <paramref name="delta"/> .
    /// </summary>
    /// <param name="delta">Инкремент</param>
    public void IncInt32(int delta)
    {
      int res = AsInt32;
      checked { res += delta; }
      AsInt32 = res;
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на 1.
    /// </summary>
    public void IncInt32()
    {
      IncInt32(1);
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на указанное значение <paramref name="delta"/> .
    /// </summary>
    /// <param name="delta">Инкремент</param>
    public void IncInt64(long delta)
    {
      long res = AsInt64;
      checked { res += delta; }
      AsInt64 = res;
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на 1.
    /// </summary>
    public void IncInt64()
    {
      IncInt64(1);
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на указанное значение <paramref name="delta"/> .
    /// </summary>
    /// <param name="delta">Инкремент</param>
    public void IncSingle(float delta)
    {
      float res = AsSingle + delta;
      AsSingle = res;
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на указанное значение <paramref name="delta"/> .
    /// </summary>
    /// <param name="delta">Инкремент</param>
    public void IncDouble(double delta)
    {
      double res = AsDouble + delta;
      AsDouble = res;
    }

    /// <summary>
    /// Увеличить значение поля <see cref="ColumnName"/> текущей строки в <see cref="DataRowValues.CurrentRow"/> на указанное значение <paramref name="delta"/> .
    /// </summary>
    /// <param name="delta">Инкремент</param>
    public void IncDecimal(decimal delta)
    {
      decimal res = AsDecimal + delta;
      AsDecimal = res;
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям строк таблицы <see cref="System.Data.DataTable"/>.
  /// Таблица задается в конструкторе и не может быть изменена.
  /// Реализует перебор по коллекции <see cref="DataRowCollection"/>.
  /// </summary>
  public sealed class DataTableValues : DataRowValues
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для заданной таблицы
    /// </summary>
    /// <param name="table">Таблица. Не может быть null</param>
    public DataTableValues(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      Table = table;
      TableIsFixed = true;
      _CurrentIndex = -1;
    }

    #endregion

    #region Чтение данных в стиле DbDataReader

    /// <summary>
    /// Последовательный перебор строк в стиле <see cref="System.Data.Common.DbDataReader"/>.
    /// Каждый вызов устанавливает свойство <see cref="DataRowValues.CurrentRow"/> на очередную строку.
    /// Если строк больше нет, метод возвращает false.
    /// Если требуется еще один проход по строкам, используйте метод <see cref="ResetReading()"/>.
    /// </summary>
    /// <returns>Наличие очередной строки</returns>
    public bool Read()
    {
      _CurrentIndex++;
      if (_CurrentIndex >= Table.Rows.Count)
      {
        CurrentRow = null;
        return false;
      }
      CurrentRow = Table.Rows[_CurrentIndex];
      return true;
    }

    private int _CurrentIndex;

    /// <summary>
    /// Организация еще одного прохода по строкам с помощью метода <see cref="Read()"/>.
    /// </summary>
    public void ResetReading()
    {
      _CurrentIndex = -1;
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям строк таблицы просмотра <see cref="System.Data.DataView"/>.
  /// Просмотр задается в конструкторе. Таблица <see cref="DataRowValues.Table"/> не может быть изменена.
  /// Реализует перебор строк таблицы в соответствии с порядком строк <see cref="System.Data.DataView.Sort"/>.
  /// </summary>
  public sealed class DataViewValues : DataRowValues
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для <see cref="System.Data.DataView"/>
    /// </summary>
    /// <param name="dv">Просмотр. Не может быть null.</param>
    public DataViewValues(DataView dv)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");
      _DataView = dv;
      Table = dv.Table;
      TableIsFixed = true;
      _CurrentIndex = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект просмотра. Задается в конструкторе.
    /// </summary>
    public DataView DataView { get { return _DataView; } }
    private readonly DataView _DataView;

    #endregion

    #region Чтение данных в стиле DbDataReader

    /// <summary>
    /// Последовательный перебор строк в стиле <see cref="System.Data.Common.DbDataReader"/>.
    /// Каждый вызов устанавливает свойство <see cref="DataRowValues.CurrentRow"/> на очередную строку.
    /// Если строк больше нет, метод возвращает false.
    /// Если требуется еще один проход по строкам, используйте метод <see cref="ResetReading()"/>.
    /// </summary>
    /// <returns>Наличие очередной строки</returns>
    public bool Read()
    {
      _CurrentIndex++;
      if (_CurrentIndex >= _DataView.Count)
      {
        CurrentRow = null;
        return false;
      }
      CurrentRow = _DataView[_CurrentIndex].Row;
      return true;
    }

    private int _CurrentIndex;

    /// <summary>
    /// Организация еще одного прохода по строкам с помощью метода <see cref="Read()"/>.
    /// </summary>
    public void ResetReading()
    {
      _CurrentIndex = -1;
    }

    #endregion
  }

  /// <summary>
  /// Доступ к значениям строк, заданных как массив.
  /// Строки могут относиться к разным таблицам.
  /// Реализует перебор строк в том порядке, в котором они заданы в массиве или перечислителе.
  /// Массив строк может быть пустым.
  /// </summary>
  public class DataRowArrayValues : DataRowValues
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект для заданного массива строк.
    /// Массив может быть пустым, но не может содержать значения null.
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <param name="table">Таблица для начального значения свойства <see cref="DataRowValues.Table"/>, чтобы можно было обращаться к объектам <see cref="DataColumnValue"/> до начала перебора строк.
    /// Можно передать значение null, в этом случае будет использована первая строка массива <see cref="Rows"/>, если массив не пустой. 
    /// Иначе свойство <see cref="DataRowValues.Table"/> останется неинициализированным.</param>
    public DataRowArrayValues(DataRow[] rows, DataTable table)
    {
      _Rows = rows;
      if (table == null && rows.Length > 0)
        Table = rows[0].Table;
      else
        Table = table;

      _CurrentIndex = -1;
    }

    /// <summary>
    /// Создает объект для заданного списка строк.
    /// Список может быть пустым, но не может содержать значения null.
    /// Список будет преобразован в массив строк в конструкторе.
    /// </summary>
    /// <param name="rows">Список строк</param>
    /// <param name="table">Таблица для начального значения свойства <see cref="DataRowValues.Table"/>, чтобы можно было обращаться к объектам <see cref="DataColumnValue"/> до начала перебора строк.
    /// Можно передать значение null, в этом случае будет использована первая строка массива <see cref="Rows"/>, если массив не пустой. 
    /// Иначе свойство <see cref="DataRowValues.Table"/> останется неинициализированным.</param>
    public DataRowArrayValues(IEnumerable<DataRow> rows, DataTable table)
      : this(ArrayTools.CreateArray<DataRow>(rows), table)
    {
    }

    /// <summary>
    /// Создает объект для заданного массива объектов <see cref="DataRowView"/>, который может быть, например, возвращен методом 
    /// <see cref="System.Data.DataView.FindRows(object)"/>.
    /// Массив может быть пустым, но не может содержать значения null.
    /// В конструкторе переданный массив преобразуется в массив строк <see cref="System.Data.DataRow"/>.
    /// </summary>
    /// <param name="drvs">Массив строк</param>
    /// <param name="table">Таблица для начального значения свойства <see cref="DataRowValues.Table"/>, чтобы можно было обращаться к объектам <see cref="DataColumnValue"/> до начала перебора строк.
    /// Можно передать значение null, в этом случае будет использована первая строка массива <see cref="Rows"/>, если массив не пустой. 
    /// Иначе свойство <see cref="DataRowValues.Table"/> останется неинициализированным.</param>
    public DataRowArrayValues(DataRowView[] drvs, DataTable table)
      : this(DataTools.GetDataRowViewRows(drvs), table)
    {
    }

    /// <summary>
    /// Создает объект для заданного списка объектов <see cref="DataRowView"/>.
    /// Список может быть пустым, но не может содержать значения null.
    /// В конструкторе переданный список преобразуется в массив строк <see cref="System.Data.DataRow"/>.
    /// </summary>
    /// <param name="drvs">Список строк</param>
    /// <param name="table">Таблица для начального значения свойства <see cref="DataRowValues.Table"/>, чтобы можно было обращаться к объектам <see cref="DataColumnValue"/> до начала перебора строк.
    /// Можно передать значение null, в этом случае будет использована первая строка массива <see cref="Rows"/>, если массив не пустой. 
    /// Иначе свойство <see cref="DataRowValues.Table"/> останется неинициализированным.</param>
    public DataRowArrayValues(IEnumerable<DataRowView> drvs, DataTable table)
      : this(ArrayTools.CreateArray<DataRowView>(drvs), table)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив строк <see cref="System.Data.DataRow"/>, по которым выполняется перебор. 
    /// Определяется в конструкторе.
    /// </summary>
    public DataRow[] Rows { get { return _Rows; } }
    private readonly DataRow[] _Rows;

    #endregion

    #region Чтение данных в стиле DbDataReader

    /// <summary>
    /// Последовательный перебор строк в стиле <see cref="System.Data.Common.DbDataReader"/>.
    /// Каждый вызов устанавливает свойство <see cref="DataRowValues.CurrentRow"/> на очередную строку.
    /// Если строк больше нет, метод возвращает false.
    /// Если требуется еще один проход по строкам, используйте метод <see cref="ResetReading()"/>.
    /// </summary>
    /// <returns>Наличие очередной строки</returns>
    public bool Read()
    {
      _CurrentIndex++;
      if (_CurrentIndex >= _Rows.Length)
      {
        CurrentRow = null;
        return false;
      }
      CurrentRow = _Rows[_CurrentIndex];
      return true;
    }

    private int _CurrentIndex;

    /// <summary>
    /// Организация еще одного прохода по строкам с помощью метода <see cref="Read()"/>.
    /// </summary>
    public void ResetReading()
    {
      _CurrentIndex = -1;
    }

    #endregion
  }
}
