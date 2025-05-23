﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Serialization;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Массив имен полей.
  /// Поля могут быть заданы только в конструкторе и в дальнейшем не могут быть изменены.
  /// Имена не могут содержать пробелы и запятые. Остальные символы не проверяются.
  /// Регистр символов учитывается.
  /// Повторы не допускаются.
  /// Массив может быть пустым.
  /// </summary>
  [Serializable]
  public class DBxColumns : IEnumerable<string>
  {
    #region Конструкторы

    /// <summary>
    /// Создание списка полей из строки имен, разделенных запятыми
    /// </summary>
    /// <param name="columnNames">Имена столбцов. Может быть пустой строкой.</param>
    public DBxColumns(string columnNames)
      : this(CreateArrayFromString(columnNames), false)
    {
    }

    private static string[] CreateArrayFromString(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return DataTools.EmptyStrings;
      else
      {
        if (columnNames.IndexOf(',') >= 0)
          return columnNames.Split(new char[] { ',' });
        else
          return new string[1] { columnNames };
      }
    }

    /// <summary>
    /// Создает список на основании массива строк.
    /// Имена столбцов не могут содержать запятые.
    /// </summary>
    /// <param name="сolumnNames">Список полей</param>
    public DBxColumns(string[] сolumnNames)
      : this(сolumnNames, true)
    {
    }

    private DBxColumns(string[] columnNames, bool copyArray)
    {
      if (copyArray)
      {
        if (columnNames == null)
          _Items = new string[0];
        else
        {
          _Items = new string[columnNames.Length];
          columnNames.CopyTo(_Items, 0);
        }
      }
      else
        _Items = columnNames;
#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }


    /// <summary>
    /// Создает список на основании массива строк.
    /// </summary>
    /// <param name="list">Список имен. Может быть пустым списком, но не должен содержать повторов</param>
    public DBxColumns(ICollection<string> list)
    {
      if (list == null)
        _Items = DataTools.EmptyStrings;
      else if (list.Count == 0)
        _Items = DataTools.EmptyStrings;
      else
      {
        _Items = new string[list.Count];
        list.CopyTo(_Items, 0);
      }
#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }

    /// <summary>
    /// Создает объект <see cref="DBxColumns"/>, если список полей не пустой, иначе возвращает null
    /// </summary>
    /// <param name="columnNames">Имена полей</param>
    /// <returns></returns>
    public static DBxColumns FromNames(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return null;
      else
        return new DBxColumns(columnNames);
    }

    /// <summary>
    /// Создает список на основании массива имен полей
    /// </summary>
    /// <param name="columnNames">Имена столбцов</param>
    /// <param name="copyArray">Если true, то будет создана копия массива.
    /// Если false, то будет использован оригинальный массив без копирования.
    /// Еспользуйте false, только если передаваемый массив <paramref name="columnNames"/>
    /// больше нигде не используется в вызывающем коде</param>
    /// <returns>Объект <see cref="DBxColumns"/> или null</returns>
    public static DBxColumns FromNames(string[] columnNames, bool copyArray)
    {
      if (columnNames == null || columnNames.Length == 0)
        return null;
      else
        return new DBxColumns(columnNames, copyArray);
    }

    /// <summary>
    /// Создает список на основании массива имен полей
    /// </summary>
    /// <param name="columnNames">Имена столбцов</param>
    /// <returns>Объект <see cref="DBxColumns"/> или null</returns>
    public static DBxColumns FromNames(string[] columnNames)
    {
      return FromNames(columnNames, true);
    }

    /// <summary>
    /// Создает список на основании списка имен столбцов.
    /// Если таблица не содержит ни одного столбца или <paramref name="columns"/>==null, возвращается null.
    /// </summary>
    /// <param name="columns">Список столбцов или null</param>
    /// <returns>Объект <see cref="DBxColumns"/> или null</returns>
    public static DBxColumns FromColumns(DataColumnCollection columns)
    {
      if (columns == null || columns.Count == 0 /* 01.05.2023 */)
        return null;

      string[] items = new String[columns.Count];

      for (int i = 0; i < columns.Count; i++)
        items[i] = columns[i].ColumnName;

      return new DBxColumns(items);
    }

    /// <summary>
    /// Получить список имен полей таблицы с заданным префиксом
    /// </summary>
    /// <param name="columns">Коллекция столбцов DataTable.Columns</param>
    /// <param name="prefix">Префикс имен столбцов. Регистр символов учитывается</param>
    /// <param name="stripPrefix">Удалить префикс из списка имен</param>
    /// <returns>Объект <see cref="DBxColumns"/> или null</returns>
    public static DBxColumns FromColumns(DataColumnCollection columns, string prefix, bool stripPrefix)
    {
      if (columns == null)
        return null;
      if (String.IsNullOrEmpty(prefix))
        return FromColumns(columns);

      List<String> names = new List<string>();
      foreach (DataColumn column in columns)
      {
        if (column.ColumnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) // 21.04.2023
        {
          if (stripPrefix)
            names.Add(column.ColumnName.Substring(prefix.Length));
          else
            names.Add(column.ColumnName);
        }
      }
      if (names.Count == 0)
        return null;
      else
        return new DBxColumns(names);
    }


    private static readonly CharArrayIndexer _DataViewSortBadChars = new CharArrayIndexer("()+*/");

    /// <summary>
    /// Извлечь имена столбцов из свойства <see cref="DataView.Sort"/>.
    /// Строка может содержать пробелы и суффиксы ASC и DESC (игнорируются).
    /// Если строка пустая, возвращается null.
    /// Используется метод <see cref="DataTools.GetDataViewSortColumnNames(string)"/>.
    /// Если задан неправильный порядок сортировки, в котором одно и то же поле встречается дважды (например, "F1,F2,F1 DESC"), выбрасывается исключение.
    /// </summary>
    /// <param name="sort">Свойство <see cref="DataView.Sort"/></param>
    /// <returns>Список столбцов</returns>
    public static DBxColumns FromDataViewSort(string sort)
    {
      //if (String.IsNullOrEmpty(sort))
      //  return null;
      //
      //if (DataTools.IndexOfAny(sort, _DataViewSortBadChars) >= 0)
      //  throw new ArgumentException("Порядок сортировки \"" + sort + "\" содержит недопустимые символы", "sort");
      //string[] a = sort.Split(',');
      //for (int i = 0; i < a.Length; i++)
      //{
      //  string s = a[i].Trim();
      //  string s1 = s.ToUpperInvariant();
      //  if (s1.EndsWith(" DESC", StringComparison.Ordinal))
      //    s = s.Substring(0, s.Length - 5);
      //  else
      //  {
      //    if (s1.EndsWith(" ASC", StringComparison.Ordinal))
      //      s = s.Substring(0, s.Length - 4);
      //  }
      //  if (s.Length == 0)
      //    throw new ArgumentException("Неправильный формат порядка сортировки", "sort");

      //  if (s[0] == '[' && s[s.Length - 1] == ']') // 21.04.2023
      //    s = s.Substring(1, s.Length - 2);
      //  a[i] = s;
      //}

      // 27.04.2023
      string[] a = DataTools.GetDataViewSortColumnNames(sort);
      return FromNames(a, false);
    }


    /// <summary>
    /// Извлечь имена столбцов из объекта <see cref="System.Data.Common.DbDataReader"/>
    /// </summary>
    /// <param name="reader">Объект для извлечения полей</param>
    /// <returns>Список</returns>
    public static DBxColumns FromDataReader(System.Data.Common.DbDataReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      string[] a = new string[reader.FieldCount];
      for (int i = 0; i < a.Length; i++)
        a[i] = reader.GetName(i);
      return new DBxColumns(a);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если список столбцов пустой
    /// </summary>
    public bool IsEmpty { get { return _Items.Length == 0; } }

    /// <summary>
    /// Возвращает число столбцов в списке
    /// </summary>
    public int Count { get { return _Items.Length; } }

    /// <summary>
    /// Доступ к столбцу по индексу
    /// </summary>
    /// <param name="index">Индекс столбца</param>
    /// <returns>Имя столбца</returns>
    public string this[int index] { get { return _Items[index]; } }

    /// <summary>
    /// Преобразование имен полей в виде строки имен, разделенных запятыми.
    /// Пробелов и кавычек нет.
    /// </summary>
    public string AsString
    {
      get
      {
        return String.Join(",", _Items);
      }
    }

    /// <summary>
    /// Получение имен полей в виде массива строк
    /// </summary>
    public string[] AsArray { get { return _Items; } }
    private readonly string[] _Items;

    /// <summary>
    /// Индексатор для быстрого доступа к полям.
    /// Создается только при количестве полей в списке три или более.
    /// </summary>
    [NonSerialized]
    private StringArrayIndexer _Indexer;

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      CreateIndexer();
    }

    private void CreateIndexer()
    {
      if (_Items.Length >= 3)
        _Indexer = new StringArrayIndexer(_Items, false); // проверяет повторы
      else
      {
        // 27.04.2023. Проверяем повторы
        for (int i = 1; i < _Items.Length; i++)
        {
          for (int j = 0; j < i; j++)
          {
            if (String.Equals(_Items[i], _Items[j], StringComparison.Ordinal))
              throw ExceptionFactory.KeyAlreadyExists(_Items[i]);
          }
        }
      }
    }

    /// <summary>
    /// Возвращает true, если в списке есть имена столбцов с точками
    /// </summary>
    public bool ContainsDots
    {
      get
      {
        for (int i = 0; i < _Items.Length; i++)
        {
          if (_Items[i].IndexOf('.') >= 0)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region Проверка имен полей

#if DEBUG

    private void CheckNames()
    {
      if (_Items == null)
        throw new BugException("Items==null");

      for (int i = 0; i < _Items.Length; i++)
      {
        try
        {
          CheckName(_Items[i]);
        }
        catch (Exception e)
        {
          throw new InvalidOperationException(String.Format(Res.DBxColumns_Err_BadColumnName, _Items[i] + i, e.Message), e);
        }
      }
    }

    private static readonly CharArrayIndexer _BadChars = new CharArrayIndexer(" ,");

    private void CheckName(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgIsEmpty("columnName");
      for (int i = 0; i < columnName.Length; i++)
      {
        if (_BadChars.Contains(columnName[i]))
          throw ExceptionFactory.ArgInvalidChar("columnName", columnName, i);
      }
    }
#endif

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает true, если поле есть в списке.
    /// Имя поля может содержать запятые. В этом случае возвращается true, если
    /// существуют ВСЕ перечисленные поля.
    /// Если <paramref name="columnNames"/> - пустая строка или null, возвращается true.
    /// </summary>
    /// <param name="columnNames">Имя поля, которое надо найти или нескольких полей,
    /// разделенных запятыми</param>
    /// <returns></returns>
    public bool Contains(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return true; // 17.10.2019

      if (_Indexer == null)
      {
        #region Без использования индексатора

        if (columnNames.IndexOf(',') >= 0)
        {
          // Комплексный поиск всех полей в списке
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
          {
            if (Array.IndexOf<string>(_Items, a[i]) < 0)
              return false;
          }
          return true;
        }
        else
          // Простой поиск вхождения
          return Array.IndexOf<string>(_Items, columnNames) >= 0;

        #endregion
      }
      else
      {
        #region С использованием индексатора

        if (columnNames.IndexOf(',') >= 0)
        {
          // Комплексный поиск всех полей в списке
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
          {
            if (!_Indexer.Contains(a[i]))
              return false;
          }
          return true;
        }
        else
          // Простой поиск вхождения
          return _Indexer.Contains(columnNames);

        #endregion
      }
    }

    /// <summary>
    /// Содержит ли текущий список все запрошенные поля из списка <paramref name="columnNames"/>.
    /// Если проверяемый список пустой или равен null, метод возвращает true.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей</param>
    /// <returns>true, если все поля имеются в списке</returns>
    public bool Contains(DBxColumns columnNames)
    {
      if (columnNames == null)
        return true;  // 17.10.2019

      if (_Indexer == null)
      {
        for (int i = 0; i < columnNames.Count; i++)
        {
          if (Array.IndexOf<string>(_Items, columnNames[i]) < 0)
            return false;
        }
      }
      else
      {
        for (int i = 0; i < columnNames.Count; i++)
        {
          if (!_Indexer.Contains(columnNames[i]))
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если поле есть в списке
    /// Имя поля может содержать запятые. В этом случае возвращается true, если
    /// существуют ХОТЯ БЫ ОДНО из перечисленных полей.
    /// Если проверяемый список пустой, возвращается false.
    /// </summary>
    /// <param name="columnNames">Имя поля, которое надо найти или несколькиз полей,
    /// разделенных запятыми</param>
    /// <returns></returns>
    public bool ContainsAny(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return false;

      if (_Indexer == null)
      {
        #region Без использования индексатора

        if (columnNames.IndexOf(',') >= 0)
        {
          // Комплексный поиск всех полей в списке
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
          {
            if (Array.IndexOf<string>(_Items, a[i]) >= 0)
              return true;
          }
          return false;
        }
        else
          // Простой поиск вхождения
          return Array.IndexOf<string>(_Items, columnNames) >= 0;

        #endregion
      }
      else
      {
        #region С использованием индексатора

        if (columnNames.IndexOf(',') >= 0)
        {
          // Комплексный поиск всех полей в списке
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
          {
            if (_Indexer.Contains(a[i]))
              return true;
          }
          return false;
        }
        else
          // Простой поиск вхождения
          return _Indexer.Contains(columnNames);

        #endregion
      }
    }

    /// <summary>
    /// Содержит ли текущий список ХОТЯ БЫ ОДНО из полей из списка <paramref name="columnNames"/>.
    /// Если проверяемый список пустой или равен null, возвращается false.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей</param>
    /// <returns>true, если хотя бы одно поле имеется в списке</returns>
    public bool ContainsAny(DBxColumns columnNames)
    {
      if (columnNames == null)
        return false;

      if (_Indexer == null)
      {
        for (int i = 0; i < columnNames.Count; i++)
        {
          if (Array.IndexOf<string>(_Items, columnNames[i]) >= 0)
            return true;
        }
      }
      else
      {
        for (int i = 0; i < columnNames.Count; i++)
        {
          if (_Indexer.Contains(columnNames[i]))
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если имеется хотя бы одно поле, имя которого начинается с <paramref name="columnNamePrefix"/>.
    /// Если <paramref name="columnNamePrefix"/> - пустая строка, то возвращается true, если в текущем массиве есть хотя бы одно поле
    /// </summary>
    /// <param name="columnNamePrefix">Префикс имени поля для поиска. Регистр символов учитывается</param>
    /// <returns>true, если есть поле с подходящим именем</returns>
    public bool ContainsStartedWith(string columnNamePrefix)
    {
      if (String.IsNullOrEmpty(columnNamePrefix))
        return _Items.Length > 0; // 17.10.2019

      for (int i = 0; i < _Items.Length; i++)
      {
        if (_Items[i].StartsWith(columnNamePrefix, StringComparison.Ordinal)) // 21.04.2023
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если таблица <paramref name="table"/> содержит ВСЕ столбцы из списка <paramref name="checkedColumns"/>.
    /// В отличие от других методов, поиск в <see cref="System.Data.DataColumnCollection"/> игнорирует регистр символов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="checkedColumns">Список столбцов, наличие которых проверяется</param>
    /// <returns>true, если все столбцы есть в таблице</returns>
    public static bool TableContains(DataTable table, DBxColumns checkedColumns)
    {
      if (checkedColumns == null)
        return true;
      if (table == null)
        return false;

      for (int i = 0; i < checkedColumns.Count; i++)
      {
        if (!table.Columns.Contains(checkedColumns[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если таблица <paramref name="table"/> содержит ХОТЯ БЫ ОДИН столбец из списка <paramref name="checkedColumns"/>
    /// В отличие от других методов, поиск в <see cref="System.Data.DataColumnCollection"/> игнорирует регистр символов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="checkedColumns">Список столбцов, наличие которых проверяется</param>
    /// <returns>true, если хотя бы один столбец есть в таблице</returns>
    public static bool TableContainsAny(DataTable table, DBxColumns checkedColumns)
    {
      if (checkedColumns == null)
        return false;
      if (table == null)
        return false;

      for (int i = 0; i < checkedColumns.Count; i++)
      {
        if (table.Columns.Contains(checkedColumns[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Найти позицию имени поля. 
    /// Поиск нескольких полей не допускается.
    /// </summary>
    /// <param name="columnName">Искомое имя поля</param>
    /// <returns>Номер позиции или (-1), если поля нет</returns>
    public int IndexOf(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        return -1;
#if DEBUG
      if (columnName.IndexOf(',') >= 0)
        throw ExceptionFactory.ArgInvalidChar("columnName", columnName, ",");
#endif

      if (_Indexer == null)
        return Array.IndexOf<string>(_Items, columnName);
      else
        return _Indexer.IndexOf(columnName);
    }

    /// <summary>
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в <paramref name="otherColumns"/>
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в <paramref name="otherColumns"/></returns>
    public bool HasMoreThan(DBxColumns otherColumns)
    {
      if (otherColumns == null)
        return Count > 0;

      for (int i = 0; i < _Items.Length; i++)
      {
        if (!otherColumns.Contains(_Items[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в списке полей <paramref name="thisColumns"/> есть поля, которых нет в <paramref name="otherColumns"/>.
    /// Статическая версия метода позволяет не проверять на null оба аргумента.
    /// </summary>
    /// <param name="thisColumns">Текущий список полей (может быть null)</param>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в <paramref name="otherColumns"/></returns>
    public static bool HasMoreThan(DBxColumns thisColumns, DBxColumns otherColumns)
    {
      if (thisColumns == null)
        return false;
      return thisColumns.HasMoreThan(otherColumns);
    }


    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      string s = "";
      for (int i = 0; i < _Items.Length; i++)
      {
        if (i > 0)
          s += ", ";
        s += "\"" + _Items[i] + "\"";
      }
      return "{" + s + "}";
    }

    #endregion

    #region Методы клонирования

    // DBxColumns не реализует интерфейс ICloneable, т.к. список полей является классом однократной записи

    /// <summary>
    /// Создает копию списка, задав префикс перед именами всех полей.
    /// Если префикс не задан, копирование не выполняется, возвращается ссылка на текущий список.
    /// Если метод вызывается для создания ссылочных полей, не забудьте включить точку в префикс.
    /// </summary>
    /// <param name="prefix">Префикс</param>
    /// <returns>Список полей с префиксом</returns>
    public DBxColumns CloneWithPrefix(string prefix)
    {
      if (String.IsNullOrEmpty(prefix))
        return this;
      string[] a = new string[Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = prefix + this[i];

      return new DBxColumns(a);
    }

    /// <summary>
    /// Создает копию списка, задав суффикс после имен всех полей.
    /// Если суффикс не задан, копирование не выполняется, возвращается ссылка на текущий список.
    /// </summary>
    /// <param name="suffix">Суффикс</param>
    /// <returns>Список полей с суффиксом</returns>
    public DBxColumns CloneWithSuffix(string suffix)
    {
      if (String.IsNullOrEmpty(suffix))
        return this;
      string[] a = new string[Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = this[i] + suffix;

      return new DBxColumns(a);
    }

    #endregion

    #region Методы извлечения значений

    /// <summary>
    /// Извлечение подмассива из <paramref name="values"/> для полей данного объекта.
    /// Если в <paramref name="columnNames"/>нет какого-либо поля, возвращается единственное значение null вместо массива.
    /// </summary>
    /// <param name="columnNames">Имена большего набора полей</param>
    /// <param name="values">Значения большего набора полей</param>
    /// <returns>Массив значений длиной <see cref="Count"/> элементов или null, если какого-нибудь поля не хватает в <paramref name="columnNames"/></returns>
    public object[] ExtractColumnValues(DBxColumns columnNames, object[] values)
    {
      if (columnNames == null)
        return new object[0];
      if (values == null)
        throw new ArgumentNullException("values");
      if (values.Length != columnNames.Count)
        throw ExceptionFactory.ArgWrongCollectionCount("values", values, columnNames.Count);

      object[] res = new object[Count];
      for (int i = 0; i < Count; i++)
      {
        int p = columnNames.IndexOf(this[i]);
        if (p < 0)
          //throw new ArgumentException("Список полей/значений не содержит поля \"" + this[i] + "\"");
          return null;
        res[i] = values[p];
      }
      return res;
    }

    #endregion

    #region Манипуляции с DataTable

    /// <summary>
    /// Добавить к коллекции столбцов <see cref="DataTable"/> все поля в объекте.
    /// Поля будут иметь одинаковый тип.
    /// </summary>
    /// <param name="columns">Коллекция столбцов в <see cref="DataTable"/>, куда добавляются столбцы</param>
    /// <param name="dataType">Тип значений, хранящихся в столбцах</param>
    public void AddColumns(DataColumnCollection columns, Type dataType)
    {
      for (int i = 0; i < Count; i++)
        columns.Add(this[i], dataType);
    }

    /// <summary>
    /// Добавить к столбцам таблицы <see cref="DataTable"/> столбцы с указанными именами, если они
    /// имеются в текущем списке полей.
    /// Не проверяется наличие уже существующих столбцов в <paramref name="columns"/>.
    /// </summary>
    /// <param name="columns">Столбцы <see cref="DataTable.Columns"/>, куда выполняется добавление</param>
    /// <param name="columnNames">Список имен добавляемых столбцов, разделенных запятыми</param>
    /// <param name="dataType">Тип значений, хранящихся в добавляемых столбцах</param>
    /// <exception cref="System.Data.DuplicateNameException">Если в таблице <paramref name="columns"/> уже есть столбец с именем добавляемого столбца</exception>
    public void AddContainedColumns(DataColumnCollection columns, string columnNames, Type dataType)
    {
      if (String.IsNullOrEmpty(columnNames))
        return;
      AddContainedColumns(columns, columnNames.Split(','), dataType);
    }

    /// <summary>
    /// Добавить к столбцам таблицы <see cref="DataTable"/> столбцы с указанными именами, если они
    /// имеются в текущем списке полей.
    /// Не проверяется наличие уже существующих столбцов в <paramref name="columns"/>.
    /// </summary>
    /// <param name="columns">Столбцы <see cref="DataTable.Columns"/>, куда выполняется добавление</param>
    /// <param name="columnNames">Массив имен добавляемых столбцов</param>
    /// <param name="dataType">Тип добавляемых столбцов</param>
    /// <exception cref="System.Data.DuplicateNameException">Если в таблице <paramref name="columns"/> уже есть столбец с именем добавляемого столбца</exception>
    public void AddContainedColumns(DataColumnCollection columns, string[] columnNames, Type dataType)
    {
      if (columnNames == null)
        return;

      for (int i = 0; i < columnNames.Length; i++)
      {
        if (Contains(columnNames[i]))
          columns.Add(columnNames[i], dataType);
      }
    }

    /// <summary>
    /// Добавить к столбцам таблицы <see cref="DataTable"/> столбцы из текущего списка, используя типы данных и другие свойства из
    /// столбцов исходной таблицы <paramref name="sourceColumns"/>. Если в структуре исходной таблицы нет
    /// какого-либо столбца, генерируется исключение.
    /// Для создания столбца используется метод <see cref="DataTools.CloneDataColumn(DataColumn)"/>.
    /// Порядок добавления столбцов соответствует текущему объекту, а не столбцов в <paramref name="sourceColumns"/>.
    /// При поиске столбцов игнорируется регистр символов.
    /// Не проверяется наличие уже существующих столбцов в <paramref name="columns"/>.
    /// </summary>
    /// <param name="columns">Столбцы <see cref="DataTable.Columns"/>, куда выполняется добавление</param>
    /// <param name="sourceColumns">Структура таблицы, откуда берутся типы столбцов</param>
    /// <exception cref="System.Data.DuplicateNameException">Если в таблице <paramref name="columns"/> уже есть столбец с именем добавляемого столбца</exception>
    public void AddContainedColumns(DataColumnCollection columns, DataColumnCollection sourceColumns)
    {
      for (int i = 0; i < _Items.Length; i++)
      {
        int p = sourceColumns.IndexOf(_Items[i]);
        if (p < 0)
          throw new InvalidOperationException(String.Format(Res.DBxColumns_Err_SourceColumnNotFound, _Items[i]));
        DataColumn srcColumn = sourceColumns[p];
        columns.Add(DataTools.CloneDataColumn(srcColumn));
      }
    }

    /// <summary>
    /// Если таблица <paramref name="sourceTable"/> имеет список столбцов, совпадающий с текущим,
    /// она возвращается без изменений. Иначе возвращается копия таблицы со столбцами из текущего объекта
    /// Если <paramref name="sourceTable"/> не содержит какого-либо столбца из текущего списка, 
    /// генерируется исключение.
    /// </summary>
    /// <param name="sourceTable">Исходная таблица</param>
    /// <returns>Копия таблицы или исходная таблица без изменений</returns>
    public DataTable CreateSubTableIfRequired(DataTable sourceTable)
    {
      if (sourceTable == null)
        throw new ArgumentNullException("sourceTable");

      if (sourceTable.Columns.Count != Count)
        return CreateSubTable(sourceTable); // 08.06.2017

      for (int i = 0; i < Count; i++)
      {
        if (!String.Equals(this[i], sourceTable.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase))
          return CreateSubTable(sourceTable);
      }

      return sourceTable;
    }

    /// <summary>
    /// Создает копию таблицы, содержащую поля из текущего списка.
    /// Если <paramref name="sourceTable"/> не содержит какого-либо столбца из текущего списка, 
    /// генерируется исключение.
    /// </summary>
    /// <param name="sourceTable">Исходная таблица</param>
    /// <returns>Копия таблицы</returns>
    public DataTable CreateSubTable(DataTable sourceTable)
    {
      if (sourceTable == null)
        throw new ArgumentNullException("sourceTable");

      DataTable resTable = new DataTable();
      for (int i = 0; i < Count; i++)
      {
        int p = sourceTable.Columns.IndexOf(this[i]);
        if (p < 0)
          throw ExceptionFactory.ArgUnknownColumnName("sourceTable", sourceTable, this[i]);

        resTable.Columns.Add(DataTools.CloneDataColumn(sourceTable.Columns[p])); // испр.01.05.2023
      }

      DataTools.CopyRowsToRows(sourceTable, resTable, true, true);
      return resTable;
    }

    #endregion

    #region Манипуляции с DataRow

    /// <summary>
    /// Извлечь из строки значения полей. Если строка не содержит каких-либо полей,
    /// то они получают значение null. Длина выходного массива равна <see cref="Count"/>.
    /// </summary>
    /// <param name="row">Строка, откуда будут браться значения</param>
    /// <returns>Массив полученных значений</returns>
    public object[] GetRowValues(DataRow row)
    {
      return GetRowValues(row, false);
    }
    /// <summary>
    /// Извлечь из строки значения полей. Если строка не содержит каких-либо полей,
    /// то они получают значение null, либо генерируется исключение, в зависимости 
    /// от параметра <paramref name="throwIfNoColumn"/>. Длина выходного массива равна <see cref="Count"/>.
    /// </summary>
    /// <param name="row">Строка, откуда будут браться значения</param>
    /// <param name="throwIfNoColumn">Если true, то при отсутствии в строке <paramref name="row"/>
    /// одного из полей выбрасывается исключение. Еслb false, то для отсутствующих
    /// полей будут возвращены значения null</param>
    /// <returns>Массив полученных значений</returns>
    public object[] GetRowValues(DataRow row, bool throwIfNoColumn)
    {
      object[] values = new object[Count];
      for (int i = 0; i < values.Length; i++)
      {
        int p = row.Table.Columns.IndexOf(this[i]);
        if (p < 0)
        {
          if (throwIfNoColumn)
            throw ExceptionFactory.ArgUnknownColumnName("row", row.Table, this[i]);
          continue;
        }
        values[i] = row[p];
      }
      return values;
    }

    /// <summary>
    /// Поместить в строку <paramref name="row"/> значения из массива <paramref name="values"/>. Предполагается, что
    /// <paramref name="values"/> содержит значения для полей, которые определяет этот объект.
    /// Если строка <paramref name="row"/> не содержит каких-либо полей, то они пропускаются.
    /// Если <paramref name="row"/>==null, никаких действий не выполняется.
    /// </summary>
    /// <param name="row">Строка данных. Может быть null</param>
    /// <param name="values">Массив значений</param>
    public void SetRowValues(DataRow row, object[] values)
    {
      if (row == null)
        return;
#if DEBUG
      if (values == null)
        throw new ArgumentNullException("values");
      if (values.Length != Count)
        throw ExceptionFactory.ArgWrongCollectionCount("values", values, Count);
#endif

      for (int i = 0; i < Count; i++)
      {
        int p = row.Table.Columns.IndexOf(this[i]);
        if (p < 0)
          continue;

        if (values[i] == null)
          row[p] = DBNull.Value;
        else
          row[p] = values[i];
      }
    }

    #endregion

    #region Операторы

    #region Add

    /// <summary>
    /// Создает объединенный список столбцов.
    /// Одинаковые имена отбрасываются.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="arg2">Второй список. Может быть null</param>
    /// <returns>Объединенный список</returns>
    public static DBxColumns operator +(DBxColumns arg1, DBxColumns arg2)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (arg2 == null)
        arg2 = Empty;

      if (arg1.Count == 0)
        return arg2;
      if (arg2.Count == 0)
        return arg1;

      return new DBxColumns(DataTools.MergeArraysOnce<string>(arg1.AsArray, arg2.AsArray));
    }

    /// <summary>
    /// Создает объединенный список столбцов.
    /// Одинаковые имена отбрасываются.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="columnNames">Второй список имен, разделенных запятыми</param>
    /// <returns>Объединенный список</returns>
    public static DBxColumns operator +(DBxColumns arg1, string columnNames)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (String.IsNullOrEmpty(columnNames))
        return arg1;
      return arg1 + new DBxColumns(columnNames);
    }

    /// <summary>
    /// Создает объединенный список столбцов.
    /// Одинаковые имена отбрасываются.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="columnNames">Второй список. Может быть null</param>
    /// <returns>Объединенный список</returns>
    public static DBxColumns operator +(DBxColumns arg1, string[] columnNames)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (columnNames == null || columnNames.Length == 0)
        return arg1;
      return arg1 + new DBxColumns(columnNames);
    }

    #endregion

    #region Substract

    /// <summary>
    /// Возвращает список столбцов, содержащий имена из списка <paramref name="arg1"/>,
    /// но которых нет в <paramref name="arg2"/>.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="arg2">Второй список. Может быть null</param>
    /// <returns>Разностный список</returns>
    public static DBxColumns operator -(DBxColumns arg1, DBxColumns arg2)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (arg2 == null || arg2.Count == 0)
        return arg1;

      DBxColumnList list = new DBxColumnList(arg1);
      list.Remove(arg2);

      return new DBxColumns(list);
    }

    /// <summary>
    /// Возвращает список столбцов, содержащий имена из списка <paramref name="arg1"/>,
    /// но которых нет в <paramref name="columnNames"/>.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="columnNames">Второй список имен, разделенных запятыми</param>
    /// <returns>Разностный список</returns>
    public static DBxColumns operator -(DBxColumns arg1, string columnNames)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (String.IsNullOrEmpty(columnNames))
        return arg1;
      return arg1 - new DBxColumns(columnNames);
    }

    /// <summary>
    /// Возвращает список столбцов, содержащий имена из списка <paramref name="arg1"/>,
    /// но которых нет в <paramref name="columnNames"/>.
    /// </summary>
    /// <param name="arg1">Первый список. Может быть null</param>
    /// <param name="columnNames">Второй список. Может быть null</param>
    /// <returns>Разностный список</returns>
    public static DBxColumns operator -(DBxColumns arg1, string[] columnNames)
    {
      if (arg1 == null)
        arg1 = Empty;
      if (columnNames == null || columnNames.Length == 0)
        return arg1;
      return arg1 - new DBxColumns(columnNames);
    }

    #endregion

    #region And

    /// <summary>
    /// Возвращает список полей, которые содержатся в обоих исходных списках.
    /// Если один или оба списка null, возвращается <see cref="Empty"/>.
    /// </summary>
    /// <param name="arg1">Первый список</param>
    /// <param name="arg2">Второй список</param>
    /// <returns>Результат пересечения</returns>
    public static DBxColumns operator &(DBxColumns arg1, DBxColumns arg2)
    {
      if (arg1 == null || arg1.Count == 0 || arg2 == null || arg2.Count == 0)
        return Empty;


      List<string> columnNames = new List<string>();
      for (int i = 0; i < arg1.Count; i++)
      {
        if (arg2.Contains(arg1[i]))
          columnNames.Add(arg1[i]);
      }
      if (columnNames.Count == 0)
        return Empty;
      else
        return new DBxColumns(columnNames);
    }

    /// <summary>
    /// Возвращает список полей, которые содержатся в обоих исходных списках.
    /// Если один или оба списка null, возвращается null.
    /// </summary>
    /// <param name="arg1">Первый список</param>
    /// <param name="columnNames">Второй список</param>
    /// <returns>Результат пересечения</returns>
    public static DBxColumns operator &(DBxColumns arg1, string columnNames)
    {
      if (arg1 == null || arg1.Count == 0 || String.IsNullOrEmpty(columnNames))
        return Empty;
      return arg1 & new DBxColumns(columnNames);
    }

    /// <summary>
    /// Возвращает список полей, которые содержатся в обоих исходных списках.
    /// Если один или оба списка null, возвращается null.
    /// </summary>
    /// <param name="arg1">Первый список</param>
    /// <param name="columnNames">Второй список</param>
    /// <returns>Результат пересечения</returns>
    public static DBxColumns operator &(DBxColumns arg1, string[] columnNames)
    {
      if (arg1 == null || arg1.Count == 0 || columnNames == null || columnNames.Length == 0)
        return Empty;
      return arg1 & new DBxColumns(columnNames);
    }

    #endregion

    #endregion

    #region IEnumerable<string> Members

    /// <summary>
    /// Возвращает перечислитель по именам столбцов.
    /// 
    /// Тип возвращаемого значения (<see cref="ArrayEnumerable{String}"/>) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerable<string>.Enumerator GetEnumerator()
    {
      return new ArrayEnumerable<string>.Enumerator(_Items);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return new ArrayEnumerable<string>.Enumerator(_Items);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerable<string>.Enumerator(_Items);
    }

    #endregion

    #region Статические поля

    /// <summary>
    /// Пустой список полей
    /// </summary>
    public static readonly DBxColumns Empty = new DBxColumns(DataTools.EmptyStrings);

    /// <summary>
    /// Список из одного поля "Id"
    /// </summary>
    public static readonly DBxColumns Id = new DBxColumns("Id");

    #endregion
  }

  /// <summary>
  /// Список имен полей для редактирования.
  /// Поля входят в список однократно.
  /// Порядок полей в массиве имеет значение.
  /// Список может быть пустым.
  /// </summary>
  [Serializable]
  public class DBxColumnList : SingleScopeList<string>, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public DBxColumnList()
    {
    }

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="capacity">Предполагаемое количество имен, которые будут добавлены</param>
    public DBxColumnList(int capacity)
      : base(capacity)
    {
    }

    /// <summary>
    /// Создает список, в которой будут добавлены имена столбцов из источника.
    /// Имена в списке не должны содержать запятых.
    /// </summary>
    /// <param name="src">Список столбцов для добавления</param>
    public DBxColumnList(ICollection<string> src)
      : base(src)
    {
    }

    /// <summary>
    /// Создает список, в которой будут добавлены имена столбцов из источника
    /// Имена в списке не должны содержать запятых.
    /// </summary>
    /// <param name="src">Список столбцов для добавления</param>
    public DBxColumnList(IEnumerable<string> src)
      : base(src)
    {
    }

    /// <summary>
    /// Создает список и добавляет в него поля из строки.
    /// Имена полей могут разделяться запятыми
    /// </summary>
    /// <param name="columnNames"></param>
    public DBxColumnList(string columnNames)
      : this()
    {
      if (!String.IsNullOrEmpty(columnNames))
        Add(columnNames);
    }

    #endregion

    #region Добавление и удаление полей из списка

    /// <summary>
    /// Добавить столбцы в список.
    /// Можно задать несколько столбцов, разделенных запятыми.
    /// Если задана пустая строка, никаких действий не выполняется.
    /// </summary>
    /// <param name="columnNames">Имена столбцов</param>
    public new void Add(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return;

      if (columnNames.IndexOf(',') >= 0)
      {
        string[] a = columnNames.Split(',');
        base.AddRange(a);
      }
      else
        base.Add(columnNames);
    }

    /// <summary>
    /// Добавляет столбцы из другого списка
    /// </summary>
    /// <param name="сolumns">Добавляемый список</param>
    public void Add(DBxColumns сolumns)
    {
      if (сolumns != null)
        base.AddRange(сolumns);
    }

    /// <summary>
    /// Добавляет столбцы из другого списка
    /// </summary>
    /// <param name="сolumns">Добавляемый список</param>
    public void Add(DBxColumnList сolumns)
    {
      if (сolumns != null)
        base.AddRange(сolumns);
    }

    /// <summary>
    /// Удалить столбцы из списка.
    /// Можно задать несколько столбцов, разделенных запятыми.
    /// Если задана пустая строка, никаких действий не выполняется.
    /// </summary>
    /// <param name="сolumnNames">Имена столбцов</param>
    public new void Remove(string сolumnNames)
    {
      if (String.IsNullOrEmpty(сolumnNames))
        return;

      if (сolumnNames.IndexOf(',') >= 0)
      {
        string[] a = сolumnNames.Split(',');
        for (int i = 0; i < a.Length; i++)
          base.Remove(a[i]);
      }
      else
        base.Remove(сolumnNames); // исправлено 20.07.2021
    }

    /// <summary>
    /// Удалить столбцы из списка.
    /// </summary>
    /// <param name="сolumns">Удаляемые имена столбцов</param>
    public void Remove(DBxColumns сolumns)
    {
      if (сolumns == null)
        return;
      for (int i = 0; i < сolumns.Count; i++)
        base.Remove(сolumns[i]);
    }

    /// <summary>
    /// Удалить столбцы из списка.
    /// </summary>
    /// <param name="сolumns">Удаляемые имена столбцов</param>
    public void Remove(DBxColumnList сolumns)
    {
      if (сolumns == null)
        return;
      for (int i = 0; i < сolumns.Count; i++)
        base.Remove(сolumns[i]);
    }

    #endregion

    #region Проверка наличия полей

    /// <summary>
    /// Содержит ли список все запрошенные поля из списка <paramref name="columnNames"/>.
    /// Если <paramref name="columnNames"/>==null или пустой список, то метод возвращает true.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей. Список имен разделен запятыми</param>
    /// <returns>true, если все поля имеются в списке</returns>
    public bool Contains(DBxColumns columnNames)
    {
      if (columnNames == null)
        //return false;
        return true; // 17.10.2019

      for (int i = 0; i < columnNames.Count; i++)
      {
        if (IndexOf(columnNames[i]) < 0)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Содержит ли список все запрошенные поля из списка <paramref name="columnNames"/>.
    /// Если <paramref name="columnNames"/>==null или пустой список, то метод возвращает true.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей. Список имен разделен запятыми</param>
    /// <returns>true, если все поля имеются в списке</returns>
    public bool Contains(DBxColumnList columnNames)
    {
      if (columnNames == null)
        //return false;
        return true; // 17.10.2019

      for (int i = 0; i < columnNames.Count; i++)
      {
        if (IndexOf(columnNames[i]) < 0)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если все поля из <paramref name="columnNames"/> есть в текущем массиве.
    /// Имена полей разделяются запятыми.
    /// Если <paramref name="columnNames"/> - пустая строка или null, возвращается true.
    /// </summary>
    /// <param name="columnNames">Имя поля, которое надо найти или несколькиз полей,
    /// разделенных запятыми.</param>
    /// <returns>Наличие полей</returns>
    public new bool Contains(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return true;
      if (columnNames.IndexOf(',') >= 0)
      {
        // Комплексный поиск всех полей в списке
        string[] a = columnNames.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          if (IndexOf(a[i]) < 0)
            return false;
        }
        return true;
      }
      else
        // Простой поиск вхождения
        return IndexOf(columnNames) >= 0;
    }

    /// <summary>
    /// Содержит ли список хотя бы одно из запрошенных полей <paramref name="columnNames"/>.
    /// Если <paramref name="columnNames"/>=null или пустой список, то возвращается false.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей. Список имен разделен запятыми</param>
    /// <returns>true, если хотя бы одно поле имеется в списке</returns>
    public bool ContainsAny(DBxColumns columnNames)
    {
      if (columnNames == null)
        return false;

      for (int i = 0; i < columnNames.Count; i++)
      {
        if (IndexOf(columnNames[i]) >= 0)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Содержит ли список хотя бы одно из запрошенных полей <paramref name="columnNames"/>.
    /// Если <paramref name="columnNames"/>=null или пустой список, то возвращается false.
    /// </summary>
    /// <param name="columnNames">Имена искомых полей. Список имен разделен запятыми</param>
    /// <returns>true, если хотя бы одно поле имеется в списке</returns>
    public bool ContainsAny(DBxColumnList columnNames)
    {
      if (columnNames == null)
        return false;

      for (int i = 0; i < columnNames.Count; i++)
      {
        if (IndexOf(columnNames[i]) >= 0)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем массиве есть любое из полей, заданных в строке <paramref name="columnNames"/>.
    /// Имена полей разделяются запятыми.
    /// Если <paramref name="columnNames"/> - пустая строка или null, возвращается false.
    /// </summary>
    /// <param name="columnNames">Имя поля, которое надо найти или несколькиз полей,
    /// разделенных запятыми.</param>
    /// <returns>Наличие полей</returns>
    public bool ContainsAny(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return false;
      if (columnNames.IndexOf(',') >= 0)
      {
        // Комплексный поиск всех полей в списке
        string[] a = columnNames.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          if (IndexOf(a[i]) >= 0)
            return true;
        }
        return false;
      }
      else
        // Простой поиск вхождения
        return IndexOf(columnNames) >= 0;
    }

    /// <summary>
    /// Возвращает true, если имеется хотя бы одно поле, имя которого начинается с <paramref name="columnNamePrefix"/>.
    /// Если <paramref name="columnNamePrefix"/> - пустая строка, то возвращается true, если текущий массив непустой.
    /// Если выполняется поиск для ссылочных полей, точка вероятно, должна быть добавлена в аргумент <paramref name="columnNamePrefix"/>.
    /// </summary>
    /// <param name="columnNamePrefix">Префикс имени поля для поиска. Регистр символов учитывается</param>
    /// <returns>true, если есть поле с подходящим именем</returns>
    public bool ContainsStartedWith(string columnNamePrefix)
    {
      if (String.IsNullOrEmpty(columnNamePrefix))
        return Count > 0; // 17.10.2019

      for (int i = 0; i < Count; i++)
      {
        if (this[i].StartsWith(columnNamePrefix, StringComparison.Ordinal))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в <paramref name="otherColumns"/>.
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в <paramref name="otherColumns"/></returns>
    public bool HasMoreThan(DBxColumns otherColumns)
    {
      if (otherColumns == null)
        return Count > 0;

      for (int i = 0; i < Count; i++)
      {
        if (!otherColumns.Contains(this[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в <paramref name="otherColumns"/>.
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в <paramref name="otherColumns"/></returns>
    public bool HasMoreThan(DBxColumnList otherColumns)
    {
      if (otherColumns == null)
        return Count > 0;

      for (int i = 0; i < Count; i++)
      {
        if (!otherColumns.Contains(this[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в списке полей <paramref name="thisColumns"/> есть поля, которых нет в <paramref name="otherColumns"/>.
    /// Статическая версия метода позволяет не проверять на null оба аргумента.
    /// </summary>
    /// <param name="thisColumns">Текущий список полей (может быть null)</param>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в <paramref name="otherColumns"/></returns>
    public static bool HasMoreThan(DBxColumnList thisColumns, DBxColumnList otherColumns)
    {
      if (thisColumns == null)
        return false;
      return thisColumns.HasMoreThan(otherColumns);
    }

    /// <summary>
    /// Преобразование имен полей в виде строки имен, разделенных запятыми.
    /// Пробелов и кавычек нет.
    /// </summary>
    public string AsString
    {
      get
      {
        return String.Join(",", ToArray());
      }
    }

    /// <summary>
    /// Возвращает true, если в списке есть имена столбцов с точками
    /// </summary>
    public bool ContainsDots
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].IndexOf('.') >= 0)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region ReadOnly

    /// <summary>
    /// Переводит список в режим "только чтение".
    /// Повторные вызовы методы игнорируются.
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка со сброшенным свойством <see cref="SingleScopeList{String}.IsReadOnly"/>.
    /// </summary>
    /// <returns></returns>
    public DBxColumnList Clone()
    {
      return new DBxColumnList(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию списка, задав префикс перед именами всех полей.
    /// У копии списка сброшено свойство <see cref="SingleScopeList{String}.IsReadOnly"/>.
    /// Если префикс не задан, выполняется обычное клонирование.
    /// Если метод вызывается для создания ссылочных полей, не забудьте включить точку в префикс.
    /// </summary>
    /// <param name="prefix">Префикс</param>
    /// <returns>Список полей с префиксом</returns>
    public DBxColumnList CloneWithPrefix(string prefix)
    {
      if (String.IsNullOrEmpty(prefix))
        return Clone();
      string[] a = new string[Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = prefix + this[i];

      return new DBxColumnList(a);
    }

    /// <summary>
    /// Создает копию списка, задав суффикс после имен всех полей.
    /// У копии списка сброшено свойство <see cref="SingleScopeList{String}.IsReadOnly"/>.
    /// Если суффикс не задан, выполняется обычное клоинрование.
    /// </summary>
    /// <param name="suffix">Суффикс</param>
    /// <returns>Список полей с префиксом</returns>
    public DBxColumnList CloneWithSuffix(string suffix)
    {
      if (String.IsNullOrEmpty(suffix))
        return Clone();
      string[] a = new string[Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = this[i] + suffix;

      return new DBxColumnList(a);
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список полей. Список доступен только для чтения.
    /// </summary>
    public static readonly DBxColumnList Empty = CreateEmpty();

    private static DBxColumnList CreateEmpty()
    {
      DBxColumnList list = new DBxColumnList();
      list.SetReadOnly();
      return list;
    }

    #endregion
  }

  /// <summary>
  /// Простая структура, хранящая имя таблицы и имя поля
  /// </summary>
  [Serializable]
  public struct DBxTableColumnName : IEquatable<DBxTableColumnName>
  {
    #region Конструктор

    /// <summary>
    /// Создает структуру
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано</param>
    /// <param name="columnName">Имя поля. Должно быть задано</param>
    public DBxTableColumnName(string tableName, string columnName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      _TableName = tableName;
      _ColumnName = columnName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя таблицы
    /// </summary>
    public string TableName { get { return _TableName; } }
    private readonly string _TableName;

    /// <summary>
    /// Имя столбца
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private readonly string _ColumnName;

    /// <summary>
    /// Возвращает текстовое представление
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _TableName + "." + _ColumnName;
    }

    #endregion

    #region Empty

    /// <summary>
    /// Пустой объект
    /// </summary>
    public static readonly DBxTableColumnName Empty = new DBxTableColumnName();

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _TableName == null; } }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение с другим экземпляром
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(DBxTableColumnName other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим экземпляром
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is DBxTableColumnName)
        return this == (DBxTableColumnName)obj;
      else
        return false;
    }

    /// <summary>
    /// Сравнение с другим экземпляром
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(DBxTableColumnName a, DBxTableColumnName b)
    {
      if (Object.ReferenceEquals(a.TableName, null) || Object.ReferenceEquals(b.TableName, null))
        return Object.ReferenceEquals(a.TableName, null) && Object.ReferenceEquals(b.TableName, null);

      return a.TableName == b.TableName && a.ColumnName == b.ColumnName;
    }

    /// <summary>
    /// Сравнение с другим экземпляром
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(DBxTableColumnName a, DBxTableColumnName b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      if (_TableName == null)
        return 0;
      else
        return _TableName.GetHashCode() ^ _ColumnName.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// Список имен таблиц и полей.
  /// Пары входят в список однократно. Порядок объектов учитывается.
  /// Предоставляет доступ как в виде структур <see cref="DBxTableColumnName"/>, так и виде коллекции таблиц,
  /// для каждой из которых имеется список <see cref="DBxColumnList"/>.
  /// Класс становится потокобезопасным после вызова <see cref="DBxTableColumnList.SetReadOnly()"/>.
  /// </summary>
  [Serializable]
  public class DBxTableColumnList : ICollection<DBxTableColumnName>, IReadOnlyObject, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public DBxTableColumnList()
    {
      _List = new NamedList<TableItem>();
      _Tables = new TableCollection(this);
    }

    #endregion

    #region Основные данные

    private class TableItem : IObjectWithCode // не может быть структурой, т.к. NamedList требует тип класса.
    {
      #region Конструктор

      internal TableItem(string tableName)
      {
        _TableName = tableName;
        _Columns = new DBxColumnList();
      }

      #endregion

      #region Свойства

      internal string TableName { get { return _TableName; } }
      private string _TableName;

      internal DBxColumnList Columns { get { return _Columns; } }
      private DBxColumnList _Columns;

      string IObjectWithCode.Code { get { return _TableName; } }

      #endregion
    }

    private readonly NamedList<TableItem> _List;

    #endregion

    #region Список таблиц

    /// <summary>
    /// Реализация свойства <see cref="Tables"/>
    /// </summary>
    [Serializable]
    public sealed class TableCollection : IDictionary<string, DBxColumnList>
    {
      // Не может быть структурой, т.к. возникает ошибка Compiler Error CS1612 "Cannot modify the return value of 'expression' because it is not a variable".

      #region Конструктор

      internal TableCollection(DBxTableColumnList owner)
      {
        _Owner = owner;
      }

      private readonly DBxTableColumnList _Owner;

      #endregion

      #region IDictionary<string,DBxColumnList> Members

      /// <summary>
      /// Добавляет столбцы для указанной таблицы
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <param name="columnNames">Добавляемые столбцы</param>
      public void Add(string tableName, DBxColumnList columnNames)
      {
        _Owner.CheckNotReadOnly();

        if (columnNames == null)
          return;
        if (columnNames.Count == 0)
          return;

        TableItem ti;
        if (!_Owner._List.TryGetValue(tableName, out ti))
        {
          ti = new TableItem(tableName);
          _Owner._List.Add(ti);
        }
        ti.Columns.Add(columnNames);
      }

      /// <summary>
      /// Возвращает true, если в коллекции есть такая таблица и для нее есть хотя бы одно поле.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Наличие таблицы</returns>
      public bool ContainsKey(string tableName)
      {
        TableItem ti;
        if (_Owner._List.TryGetValue(tableName, out ti))
          return ti.Columns.Count > 0;
        else
          return false;
      }

      /// <summary>
      /// Возвращает коллекцию имен таблиц
      /// </summary>
      public ICollection<string> Keys { get { return _Owner._List.GetCodes(); } }

      /// <summary>
      /// Удаляет все поля для указанной таблицы
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>True, если таблица была в списке</returns>
      public bool Remove(string tableName)
      {
        _Owner.CheckNotReadOnly();
        return _Owner._List.Remove(tableName);
      }

      /// <summary>
      /// Возвращает список полей для указанной таблицы.
      /// Если в списке нет такой таблицы, то результат зависит от свойства <see cref="DBxTableColumnList.IsReadOnly"/>.
      /// Если список находится в режиме "Только чтение", то возвращается <see cref="DBxColumnList.Empty"/>.
      /// Иначе создается запись для таблицы с пустым списком.
      /// Метод никогда не возвращает false и всегда предоставляет список.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <param name="columnNames">Сюда записывается ссылка на список полей</param>
      /// <returns>Всегда true</returns>
      public bool TryGetValue(string tableName, out DBxColumnList columnNames)
      {
        TableItem ti;
        if (_Owner._List.TryGetValue(tableName, out ti))
          columnNames = ti.Columns;
        else if (_Owner.IsReadOnly)
          columnNames = DBxColumnList.Empty;
        else
        {
          ti = new TableItem(tableName);
          _Owner._List.Add(ti);
          columnNames = ti.Columns;
        }
        return true;
      }

      ICollection<DBxColumnList> IDictionary<string, DBxColumnList>.Values
      {
        get { throw new NotImplementedException(); }
      }

      /// <summary>
      /// Возвращает список полей для указанной таблицы.
      /// Если в списке нет такой таблицы, то результат зависит от свойства <see cref="DBxTableColumnList.IsReadOnly"/>.
      /// Если список находится в режиме "Только чтение", то возвращается <see cref="DBxColumnList.Empty"/>.
      /// Иначе создается запись для таблицы с пустым списком.
      /// Метод никогда не возвращает null.
      /// 
      /// Установка значения разрешается только при <see cref="DBxTableColumnList.IsReadOnly"/>=false.
      /// Текущие поля таблицы, если есть, очищаются и заменяются на переданные.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Список полей</returns>
      public DBxColumnList this[string tableName]
      {
        get
        {
          TableItem ti;
          if (_Owner._List.TryGetValue(tableName, out ti))
            return ti.Columns;
          else if (_Owner.IsReadOnly)
            return DBxColumnList.Empty;
          else
          {
            ti = new TableItem(tableName);
            _Owner._List.Add(ti);
            return ti.Columns;
          }
        }
        set
        {
          _Owner.CheckNotReadOnly();

          if (value == null)
            value = DBxColumnList.Empty;

          TableItem ti;
          if (!_Owner._List.TryGetValue(tableName, out ti))
          {
            ti = new TableItem(tableName);
            _Owner._List.Add(ti);
          }
          ti.Columns.Clear();
          ti.Columns.Add(value);
        }
      }

      #endregion

      #region ICollection<KeyValuePair<string,DBxColumnList>> Members

      void ICollection<KeyValuePair<string, DBxColumnList>>.Add(KeyValuePair<string, DBxColumnList> item)
      {
        Add(item.Key, item.Value);
      }

      /// <summary>
      /// Очищает весь список
      /// </summary>
      public void Clear()
      {
        _Owner.CheckNotReadOnly();
        _Owner._List.Clear();
      }

      bool ICollection<KeyValuePair<string, DBxColumnList>>.Contains(KeyValuePair<string, DBxColumnList> item)
      {
        throw new NotImplementedException();

      }

      /// <summary>
      /// Копирует список
      /// </summary>
      /// <param name="array">Записываемый массив</param>
      /// <param name="arrayIndex">Смещение в массиве <paramref name="array"/>, по которому выполняется копирование</param>
      public void CopyTo(KeyValuePair<string, DBxColumnList>[] array, int arrayIndex)
      {
        //ICollection<KeyValuePair<string, DBxColumnList>> lst = (ICollection<KeyValuePair<string, DBxColumnList>>)(_Owner._List);
        //lst.CopyTo(array, arrayIndex);
        // так не получится, надо делать руками
        for (int i = 0; i < _Owner._List.Count; i++)
        {
          TableItem ti = _Owner._List[i];
          array[arrayIndex] = new KeyValuePair<string, DBxColumnList>(ti.TableName, ti.Columns);
          arrayIndex++;
        }
      }

      /// <summary>
      /// Возврашает количество таблиц (не полей!) в списке.
      /// Пока <see cref="DBxTableColumnList.IsReadOnly"/>=false, список может содержать таблицы без полей.
      /// Вызов <see cref="SetReadOnly()"/> убирает из списка пустые таблицы, при этом значение свойства <see cref="Count"/> может уменьшиться.
      /// </summary>
      public int Count
      {
        get { return _Owner._List.Count; }
      }

      bool ICollection<KeyValuePair<string, DBxColumnList>>.IsReadOnly
      {
        get { return _Owner.IsReadOnly; }
      }

      bool ICollection<KeyValuePair<string, DBxColumnList>>.Remove(KeyValuePair<string, DBxColumnList> item)
      {
        _Owner.CheckNotReadOnly();
        TableItem ti = _Owner._List[item.Key];
        if (ti == null)
          return false;
        int oldN = ti.Columns.Count;
        ti.Columns.Remove(item.Value);
        return oldN > ti.Columns.Count;
      }

      #endregion

      #region IEnumerable<KeyValuePair<string,DBxColumnList>> Members

      /// <summary>
      /// Реализация перечислителя по парам "Таблица-Поля"
      /// </summary>
      public struct Enumerator : IEnumerator<KeyValuePair<string, DBxColumnList>>
      {
        #region Конструктор

        internal Enumerator(DBxTableColumnList owner)
        {
          _Owner = owner;
          _TableIndex = -1;
        }

        #endregion

        #region Поля

        DBxTableColumnList _Owner;

        private int _TableIndex;

        #endregion

        #region IEnumerator<KeyValuePair<string,DBxColumnList>> Members

        /// <summary>
        /// Возвращает текущий элементь перечислителя
        /// </summary>
        public KeyValuePair<string, DBxColumnList> Current
        {
          get
          {
            TableItem ti = _Owner._List[_TableIndex];
            return new KeyValuePair<string, DBxColumnList>(ti.TableName, ti.Columns);
          }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Ничего не делает
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current { get { return Current; } }

        /// <summary>
        /// Переход к следующей таблице
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
          _TableIndex++;
          return _TableIndex < _Owner._List.Count;
        }

        void System.Collections.IEnumerator.Reset()
        {
          _TableIndex = -1;
        }

        #endregion
      }

      /// <summary>
      /// Возвращает перечислитель по парам "Имя таблицы"-"Список столбцов"
      /// </summary>
      /// <returns></returns>
      public Enumerator GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      IEnumerator<KeyValuePair<string, DBxColumnList>> IEnumerable<KeyValuePair<string, DBxColumnList>>.GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      #endregion
    }

    /// <summary>
    /// Доступ к отдельным таблицам, для каждой из которых доступен список <see cref="DBxColumnList"/>
    /// </summary>
    public TableCollection Tables { get { return _Tables; } }
    private readonly TableCollection _Tables;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список переведен в режим "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly; // можно было бы использовать _Tables.ReadOnly, но тогда нужно выводить класс из NamedList

    /// <summary>
    /// Генерирует исключение, если список переведен в режим "Только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Переводит список в режим "Только чтение".
    /// Если в списке таблиц имеются пустые записи (то есть, для таблицы нет ни одного поля,
    /// то таблица удаляется.
    /// Повторные вызовы отбрасываются.
    /// </summary>
    public void SetReadOnly()
    {
      if (IsReadOnly)
        return; // Повторный вызов
      for (int i = _List.Count - 1; i >= 0; i--)
      {
        if (_List[i].Columns.Count == 0)
          _List.RemoveAt(i);
        else
          _List[i].Columns.SetReadOnly();
      }
      _IsReadOnly = true;
    }

    #endregion

    #region ICollection<DBxTableColumnName> Members

    /// <summary>
    /// Добавляет поле для таблицы в список
    /// </summary>
    /// <param name="item">Имя таблицы и поля</param>
    public void Add(DBxTableColumnName item)
    {
      CheckNotReadOnly();
      if (item.IsEmpty)
        throw new ArgumentNullException("item");

      Tables[item.TableName].Add(item.ColumnName);
    }

    /// <summary>
    /// Добавляет все элементы из другого списка
    /// </summary>
    /// <param name="collection">Список добавляемых элементов</param>
    public void AddRange(IEnumerable<DBxTableColumnName> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw ExceptionFactory.ArgCollectionSameAsThis("collection");

      foreach (DBxTableColumnName item in collection)
      {
        if (item.IsEmpty)
          throw ExceptionFactory.ArgInvalidEnumerableItem("collection", collection, item);

        Tables[item.TableName].Add(item.ColumnName);
      }
    }

    /// <summary>
    /// Очищает весь список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _List.Clear();
    }

    /// <summary>
    /// Возвращает true, если список содержит такое поле для таблицы
    /// </summary>
    /// <param name="item">Имя таблицы и поля</param>
    /// <returns>Наличие поля</returns>
    public bool Contains(DBxTableColumnName item)
    {
      if (item.IsEmpty)
        return false;

      return Tables[item.TableName].Contains(item.ColumnName);
    }

    /// <summary>
    /// Возвращает количество полей во всех таблицах
    /// </summary>
    public int Count
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _List.Count; i++)
          cnt += _List[i].Columns.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Копирует пары "Имя таблицы"-"Имя поля" в указанный массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Смещение в массиве, по которому записывается первый элемент</param>
    public void CopyTo(DBxTableColumnName[] array, int arrayIndex)
    {
      for (int i = 0; i < _List.Count; i++)
      {
        TableItem ti = _List[i];
        for (int j = 0; j < ti.Columns.Count; j++)
        {
          array[arrayIndex] = new DBxTableColumnName(ti.TableName, ti.Columns[j]);
          arrayIndex++;
        }
      }
    }

    /// <summary>
    /// Копирует все пары "Имя таблицы"-"Имя поля" в массив
    /// </summary>
    /// <returns>Новый массив из Count элементов</returns>
    public DBxTableColumnName[] ToArray()
    {
      DBxTableColumnName[] a = new DBxTableColumnName[Count];
      CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// Удаляет поле для заданной таблицы из списка
    /// </summary>
    /// <param name="item">Имя таблицы и поля</param>
    /// <returns>true, если поле было добавлено для таблицы</returns>
    public bool Remove(DBxTableColumnName item)
    {
      CheckNotReadOnly();
      if (item.IsEmpty)
        return false;
      TableItem ti = _List[item.TableName];
      if (ti == null)
        return false;

      int oldN = ti.Columns.Count;
      ti.Columns.Remove(item.ColumnName);
      return ti.Columns.Count < oldN;
    }

    #endregion

    #region IEnumerable<DBxTableColumnName> Members

    /// <summary>
    /// Возвращает перечислитель по всем парам "Имя таблицы"-"Имя поля"
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<DBxTableColumnName> GetEnumerator()
    {
      // Неохота делать свой перечислитель
      return new ArrayEnumerable<DBxTableColumnName>.Enumerator(ToArray());
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Доступ как к логическому значению

    /// <summary>
    /// Возвращает true, если в списке есть указанное поле. Эквивалентно вызову <see cref="Contains(DBxTableColumnName)"/>.
    /// Установка свойства в true эквивалентна вызову <see cref="Add(DBxTableColumnName)"/>, а в false - <see cref="Remove(DBxTableColumnName)"/>.
    /// </summary>
    /// <param name="item">Имя таблицы и поля</param>
    /// <returns>Наличие в списке</returns>
    public bool this[DBxTableColumnName item]
    {
      get
      {
        return Contains(item);
      }
      set
      {
        if (value)
          Add(item);
        else
          Remove(item);
      }
    }

    #endregion

    #region Доступ без структуры DBxTableColumnName

    /// <summary>
    /// Возвращает true, если в списке есть указанное поле. Эквивалентно вызову <see cref="Contains(DBxTableColumnName)"/>.
    /// Установка свойства в true эквивалентна вызову <see cref="Add(DBxTableColumnName)"/>, а в false - <see cref="Remove(DBxTableColumnName)"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Наличие в списке</returns>
    public bool this[string tableName, string columnName]
    {
      get
      {
        if (String.IsNullOrEmpty(tableName) || String.IsNullOrEmpty(columnName))
          return false;
        return Contains(new DBxTableColumnName(tableName, columnName));
      }
      set
      {
        if (value)
          Add(new DBxTableColumnName(tableName, columnName));
        else
          Remove(new DBxTableColumnName(tableName, columnName));
      }
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка.
    /// У созданной копии свойство <see cref="IsReadOnly"/> не установлено
    /// </summary>
    /// <returns></returns>
    public DBxTableColumnList Clone()
    {
      DBxTableColumnList res = new DBxTableColumnList();
      for (int i = 0; i < _List.Count; i++)
      {
        if (_List[i].Columns.Count > 0)
          res.Tables.Add(_List[i].TableName, _List[i].Columns);
      }
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список полей. Список доступен только для чтения.
    /// </summary>
    public static readonly DBxTableColumnList Empty = CreateEmpty();

    private static DBxTableColumnList CreateEmpty()
    {
      DBxTableColumnList list = new DBxTableColumnList();
      list.SetReadOnly();
      return list;
    }

    #endregion
  }

  /// <summary>
  /// Хранилище для массива значений полей, связанное с заданным списком столбцов <see cref="DBxColumns"/>.
  /// Однократно созданному объекту можно многократно присваивать значения (свойство <see cref="DBxColumnValueArray.Values"/>).
  /// Используется при проверке фильтров.
  /// </summary>
  [Serializable]
  public class DBxColumnValueArray : INamedValuesAccess
  {
    #region Конструкторы

    /// <summary>
    /// Создает список с заданными значениями.
    /// дальнейшая установка свойства <see cref="Values"/> позволит их изменить.
    /// </summary>
    /// <param name="columns">Список столбцов. Не может быть null, но может быть пустым.</param>
    /// <param name="values">Массив значений. Должен иметь ту же длину, что и список <paramref name="columns"/>.</param>
    public DBxColumnValueArray(DBxColumns columns, object[] values)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");
      _Columns = columns;
      this.Values = values; // там проверяется длина массива
    }

    /// <summary>
    /// Создает список, в котором все значения пустые.
    /// Далее может быть установлено свойство <see cref="Values"/>.
    /// </summary>
    /// <param name="columns">Список столбцов. Не может быть null, но может быть пустым.</param>
    public DBxColumnValueArray(DBxColumns columns)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");
      _Columns = columns;
      _Values = new object[columns.Count]; // пустой список значений
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список столбцов. Задается в конструкторе
    /// </summary>
    public DBxColumns Columns { get { return _Columns; } }
    private readonly DBxColumns _Columns;

    /// <summary>
    /// Массив значений.
    /// </summary>
    public object[] Values
    {
      get { return _Values; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value.Length != _Columns.Count)
          throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Columns.Count);
        _Values = value;
      }
    }
    private object[] _Values;

    /// <summary>
    /// Возвращает значение столбца с заданным именем.
    /// Если нет столбца с таким именем, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Значение поля</returns>
    public object this[string columnName]
    {
      get
      {
        int p = _Columns.IndexOf(columnName);
        if (p < 0)
        {
          if (String.IsNullOrEmpty(columnName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
          else
            throw ExceptionFactory.ArgUnknownValue("columnName", columnName);
        }
        return _Values[p];
      }
    }

    /// <summary>
    /// Возвращает текстовое представление для Columns
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Columns.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      // Не надо, чтобы проверялись множества полей с запятыми, как делает метод DBxColumns.Contains()
      if (String.IsNullOrEmpty(name))
        return false;
      if (name.IndexOf(',') >= 0)
        return false;

      return Columns.Contains(name);
    }

    string[] INamedValuesAccess.GetNames()
    {
      return Columns.AsArray;
    }

    #endregion
  }
}
