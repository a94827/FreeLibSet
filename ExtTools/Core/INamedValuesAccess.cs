// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Интерфейс доступа только для чтения к именованным значениям.
  /// Порядок элементов в коллекции считается неопределенным.
  /// </summary>
  public interface INamedValuesAccess
  {
    #region Методы

    /// <summary>
    /// Получить значение с заданным именем
    /// </summary>
    /// <param name="name">Имя. Не может быть пустой строкой</param>
    /// <returns>Значение. Что происходит, если запрошено несуществующее имя, зависит от реализации</returns>
    object GetValue(string name);

    /// <summary>
    /// Определить наличие заданного имени в коллекции.
    /// </summary>
    /// <param name="name">Проверяемое имя</param>
    /// <returns>True, если в коллекции есть значение с таким именем</returns>
    bool Contains(string name);

    /// <summary>
    /// Получить список всех имен, которые есть в коллекции
    /// </summary>
    /// <returns>Массив имен</returns>
    string[] GetNames();

    #endregion
  }

  /// <summary>
  /// Пустая реализация интерфейса <see cref="INamedValuesAccess"/>
  /// </summary>
  public sealed class DummyNamedValues:INamedValuesAccess
  {
    #region INamedValuesAccess Members

    /// <summary>
    /// Выбрасывает исключение
    /// </summary>
    /// <param name="name">Не используется</param>
    /// <returns>Не возвращается</returns>
    public object GetValue(string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает false
    /// </summary>
    /// <param name="name">Не используется</param>
    /// <returns>false</returns>
    public bool Contains(string name)
    {
      return false;
    }

    /// <summary>
    /// ВОзвращает пустой массив
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      return DataTools.EmptyStrings;
    }

    #endregion
  }
}

namespace FreeLibSet.Data
{
  /// <summary>
  /// Расширение интерфейса <see cref="INamedValuesAccess"/> для доступа к значениям строки <see cref="DataRow"/>.
  /// Реализуется классами <see cref="DataTableValueArray"/> и <see cref="DataRowValueArray"/>.
  /// </summary>
  public interface IDataRowNamedValuesAccess : INamedValuesAccess
  {
    /// <summary>
    /// Текущая строка.
    /// Свойство должно быть установлено перед доступом к значениям.
    /// </summary>
    DataRow CurrentRow { get; set;}
  }

  /// <summary>
  /// Реализация интерфейса <see cref="INamedValuesAccess"/> для строк в таблице <see cref="DataTable"/>.
  /// После создания объектов должно устанавливаться свойство <see cref="CurrentRow"/>, после чего можно получать доступ к значениям полей.
  /// Класс не является потокобезопасным.
  /// Если нет уверенности, что строки относятся к одной таблице, используйте класс <see cref="DataRowValueArray"/>.
  /// </summary>
  public class DataTableValueArray : IDataRowNamedValuesAccess
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// После создания объекта нельзя добавлять столбцы в таблицу.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    public DataTableValueArray(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      _Table = table;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица данных, заданная в конструкторе
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private readonly DataTable _Table;

    /// <summary>
    /// Текущая строка.
    /// Свойство должно быть установлено перед доступом к значениям.
    /// Разрешается присваивать ссылки только на строки, относящиеся к таблице <see cref="Table"/>.
    /// </summary>
    public DataRow CurrentRow
    {
      get { return _CurrentRow; }
      set
      {
        if (value != null)
        {
          if (!Object.ReferenceEquals(value.Table, _Table))
            throw ExceptionFactory.ArgDataRowNotInSameTable("value", value, _Table);
        }
        _CurrentRow = value;
      }
    }
    private DataRow _CurrentRow;

    /// <summary>
    /// Индексатор имен столбцов стаблицы
    /// </summary>
    protected StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
          _ColumnNameIndexer = new StringArrayIndexer(GetNames(), false);
        return _ColumnNameIndexer;
      }

    }
    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// Возвращает имя таблицы
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      return Table.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    /// <summary>
    /// Получить значение поля из текущей строки.
    /// Свойство <see cref="CurrentRow"/> должно быть предварительно установлено.
    /// </summary>
    /// <param name="name">Имя поля. Чувствительно к регистру</param>
    /// <returns>Значение поля</returns>
    public object GetValue(string name)
    {
      int p = ColumnNameIndexer.IndexOf(name);
      if (p < 0)
      {
        if (String.IsNullOrEmpty(name))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
        else
          throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);
      }
      if (_CurrentRow == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CurrentRow");
      return _CurrentRow[p];
    }

    /// <summary>
    /// Возвращает true, если таблица содержит столбец с заданным именем
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Наличие поля</returns>
    public bool Contains(string name)
    {
      return ColumnNameIndexer.Contains(name);
    }

    /// <summary>
    /// Возвращает список имен всех столбцов
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetNames()
    {
      string[] colNames = new string[Table.Columns.Count];
      for (int i = 0; i < Table.Columns.Count; i++)
        colNames[i] = Table.Columns[i].ColumnName;
      return colNames;
    }

    #endregion
  }

  /// <summary>
  /// Реализация интерфейса <see cref="INamedValuesAccess"/> для строк <see cref="DataRow"/>.
  /// После создания объектов должно устанавливаться свойство <see cref="CurrentRow"/>, после чего можно получать доступ к значениям полей.
  /// Если строка не установлена, считается, что массив значений пустой.
  /// Класс не является потокобезопасным.
  /// Если требуется перебор строк в одной таблице <see cref="DataTable"/>, используйте <see cref="DataTableValueArray"/>.
  /// </summary>
  public class DataRowValueArray : IDataRowNamedValuesAccess
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// Пока строка не установлена, считается, что массив значений пустой
    /// </summary>
    public DataRowValueArray()
    {
    }

    #endregion

    #region Свойства

    private DataTable _Table;

    /// <summary>
    /// Текущая строка.
    /// Свойство должно быть установлено перед доступом к значениям.
    /// Разрешается присваивать ссылки на строки, относящиеся к разным таблицам.
    /// </summary>
    public DataRow CurrentRow
    {
      get { return _CurrentRow; }
      set
      {
        if (value != null)
        {
          if (!Object.ReferenceEquals(value.Table, _Table))
          {
            _ColumnNameIndexer = null;
            _Table = value.Table;
          }
        }
        _CurrentRow = value;
      }
    }
    private DataRow _CurrentRow;

    /// <summary>
    /// Индексатор имен столбцов в таблице.
    /// Может меняться при установке свойства <see cref="CurrentRow"/>.
    /// </summary>
    protected StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
          _ColumnNameIndexer = new StringArrayIndexer(GetNames(), false);
        return _ColumnNameIndexer;
      }

    }
    private StringArrayIndexer _ColumnNameIndexer;

    /// <summary>
    /// Возвращает имя таблицы
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (_Table == null)
        return String.Empty;
      else
        return _Table.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    /// <summary>
    /// Получить значение поля из текущей строки.
    /// Свойство <see cref="CurrentRow"/> должно быть предварительно установлено.
    /// </summary>
    /// <param name="name">Имя поля. Чувствительно к регистру</param>
    /// <returns>Значение поля</returns>
    public object GetValue(string name)
    {
      if (_CurrentRow == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CurrentRow");

      int p = ColumnNameIndexer.IndexOf(name);
      if (p < 0)
      {
        if (String.IsNullOrEmpty(name))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
        else
          throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);
      }
      return _CurrentRow[p];
    }

    /// <summary>
    /// Возвращает true, если таблица содержит столбец с заданным именем.
    /// Если свойство <see cref="CurrentRow"/>==null, возвращается false.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Наличие поля</returns>
    public bool Contains(string name)
    {
      if (_CurrentRow == null)
        return false;
      else
        return ColumnNameIndexer.Contains(name);
    }

    /// <summary>
    /// Возвращает список имен всех столбцов
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      if (_CurrentRow == null)
        return DataTools.EmptyStrings;

      string[] names = new string[_Table.Columns.Count];
      for (int i = 0; i < _Table.Columns.Count; i++)
        names[i] = _Table.Columns[i].ColumnName;
      return names;
    }

    #endregion
  }
}
