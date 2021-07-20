﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Serialization;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Массив имен полей.
  /// Поля могут быть заданы только в конструкторе и в дальнейшем не могут быть изменены.
  /// Имена не могут содержать пробелы и запятые. Остальные символы не проверяются.
  /// Массив может быть пустым.
  /// </summary>
  [Serializable]
  public class DBxColumns : IEnumerable<string>
  {
    #region Конструкторы

    /// <summary>
    /// Создает список на основании DBxColumnList
    /// </summary>
    /// <param name="columnList">Список столбцов</param>
    public DBxColumns(DBxColumnList columnList)
    {
#if DEBUG
      if (columnList == null)
        throw new ArgumentNullException();
#endif
      _Items = columnList.ToArray();
#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }

    /// <summary>
    /// Создание списка полей из строки имен, разделенных запятыми
    /// </summary>
    /// <param name="сolumnNames">Имена столбцов</param>
    public DBxColumns(string сolumnNames)
    {
      if (String.IsNullOrEmpty(сolumnNames))
        _Items = DataTools.EmptyStrings;
      else
      {
        if (сolumnNames.IndexOf(',') >= 0)
          _Items = сolumnNames.Split(new char[] { ',' });
        else
          _Items = new string[1] { сolumnNames };
      }

#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }

    /// <summary>
    /// Создает список на основании массива строк.
    /// Имена столбцов не меогут содержать запятые
    /// </summary>
    /// <param name="сolumnNames">Список столбов</param>
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
    /// Создает список на основании списка столбов
    /// </summary>
    /// <param name="list">Имена столбцов</param>
    public DBxColumns(List<string> list)
    {
      if (list == null)
        _Items = DataTools.EmptyStrings;
      else if (list.Count == 0)
        _Items = DataTools.EmptyStrings;
      else
        _Items = list.ToArray();
#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }

    private DBxColumns(DBxColumns columnNames1, DBxColumns columnNames2)
    {
      List<String> a = new List<string>();
      if (columnNames1 != null)
      {
        for (int i = 0; i < columnNames1._Items.Length; i++)
        {
          if (!a.Contains(columnNames1._Items[i]))
            a.Add(columnNames1._Items[i]);
        }
      }
      if (columnNames2 != null)
      {
        for (int i = 0; i < columnNames2._Items.Length; i++)
        {
          if (!a.Contains(columnNames2._Items[i]))
            a.Add(columnNames2._Items[i]);
        }
      }
      _Items = a.ToArray();
#if DEBUG
      CheckNames();
#endif
      CreateIndexer();
    }

    /// <summary>
    /// Создает объект DBxColumns, если список полей не пустой, иначе возвращает null
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
    /// Создает список на основании массива имен столбов
    /// </summary>
    /// <param name="columnNames">Имена столбцов</param>
    /// <param name="copyArray">Если true, то будет создана копия массива.
    /// Если false, то будет использован оригинальный массив без копирования.
    /// Еспользуйте false, только если передаваемый массив <paramref name="columnNames"/>
    /// больше нигде не используется в вызывающем коде</param>
    /// <returns>Объект DBxColumns или null</returns>
    public static DBxColumns FromNames(string[] columnNames, bool copyArray)
    {
      if (columnNames == null || columnNames.Length == 0)
        return null;
      else
        return new DBxColumns(columnNames, copyArray);
    }

    /// <summary>
    /// Создает список на основании массива имен столбов
    /// </summary>
    /// <param name="columnNames">Имена столбцов</param>
    /// <returns>Объект DBxColumns или null</returns>
    public static DBxColumns FromNames(string[] columnNames)
    {
      return FromNames(columnNames, true);
    }

    /// <summary>
    /// Создает список на основании списка имен столбцов.
    /// </summary>
    /// <param name="columns">Список столбцов или null</param>
    /// <returns>Объект DBxColumns или null</returns>
    public static DBxColumns FromColumns(DataColumnCollection columns)
    {
      if (columns == null)
        return null;

      string[] Items = new String[columns.Count];

      for (int i = 0; i < columns.Count; i++)
        Items[i] = columns[i].ColumnName;

      return new DBxColumns(Items);
    }

    /// <summary>
    /// Получить список имен полей таблицы с заданным префиксом
    /// </summary>
    /// <param name="columns">Коллекция столбцов DataTable.Columns</param>
    /// <param name="prefix">Префикс имен столбцов</param>
    /// <param name="stripPrefix">Удалить префикс из списка имен</param>
    /// <returns>Объект DBxColumns или null</returns>
    public static DBxColumns FromColumns(DataColumnCollection columns, string prefix, bool stripPrefix)
    {
      if (columns == null)
        return null;
      if (String.IsNullOrEmpty(prefix))
        return FromColumns(columns);

      List<String> Names = new List<string>();
      foreach (DataColumn Column in columns)
      {
        if (Column.ColumnName.StartsWith(prefix))
        {
          if (stripPrefix)
            Names.Add(Column.ColumnName.Substring(prefix.Length));
          else
            Names.Add(Column.ColumnName);
        }
      }
      if (Names.Count == 0)
        return null;
      else
        return new DBxColumns(Names);
    }


    private static readonly CharArrayIndexer DataViewSortBadChars = new CharArrayIndexer("()+*/");

    /// <summary>
    /// Извлечь имена столбцов из свойства DataView.Sort.
    /// Строка может содержать пробелы и суффиксы ASC и DESC (игнорируются).
    /// Если строка пустая, возвращается null.
    /// В текущей реализации не поддерживается полноценный парсинг, поэтому в выражении не должно быть функций и математических выражений
    /// </summary>
    /// <param name="sort">Свойство DataView.Sort</param>
    /// <returns></returns>
    public static DBxColumns FromDataViewSort(string sort)
    {
      if (String.IsNullOrEmpty(sort))
        return null;
      if (DataTools.IndexOfAny(sort, DataViewSortBadChars) >= 0)
        throw new ArgumentException("Порядок сортировки \"" + sort + "\" содержит недопустимые символы", "sort");
      string[] a = sort.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        string s = a[i].Trim();
        string s1 = s.ToUpper();
        if (s1.EndsWith(" DESC"))
          s = s.Substring(0, s.Length - 5);
        else
        {
          if (s1.EndsWith(" ASC"))
            s = s.Substring(0, s.Length - 4);
        }
        if (s.StartsWith("[") && s.EndsWith("]"))
          s = s.Substring(1, s.Length - 2);
        a[i] = s;
      }
      return new DBxColumns(a);
    }


    /// <summary>
    /// Извлечь имена столбцов из объекта DbDataReader
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
    private string[] _Items;

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
        _Indexer = new StringArrayIndexer(_Items, false);
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
          throw new InvalidOperationException("Неправильное имя поля \"" + _Items[i] + "\" в позиции с индексом " + i.ToString() + ". " + e.Message, e);
        }
      }
    }

    private static readonly CharArrayIndexer BadChars = new CharArrayIndexer(" ,");

    private void CheckName(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName", "Имя поля не может быть пустым");
      for (int i = 0; i < columnName.Length; i++)
      {
        if (BadChars.Contains(columnName[i]))
          throw new ArgumentException("columnName", "Недопустимый символ \"" + columnName[i] + "\" в позиции " + (i + 1).ToString());
      }
    }
#endif

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает true, если поле есть в списке
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
    /// Если <paramref name="columnNamePrefix"/> - пустая строка, то возвращается true, если в текушем массиве есть хотя бы одно поле
    /// </summary>
    /// <param name="columnNamePrefix">Префикс имени поля для поиска</param>
    /// <returns>true, если есть поле с подходящим именем</returns>
    public bool ContainsStartedWith(string columnNamePrefix)
    {
      if (String.IsNullOrEmpty(columnNamePrefix))
        return _Items.Length > 0; // 17.10.2019

      for (int i = 0; i < _Items.Length; i++)
      {
        if (_Items[i].StartsWith(columnNamePrefix))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если таблицы <paramref name="table"/> содержит ВСЕ столбцы из списка <paramref name="checkedColumns"/>
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
    /// Возвращает true, если таблицы <paramref name="table"/> содержит ХОТЯ БЫ ОДИН столбец из списка <paramref name="checkedColumns"/>
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
    /// Поиск нескольких полей не допускается
    /// </summary>
    /// <param name="columnName">Искомое имя поля</param>
    /// <returns>Номер позиции или (-1), если поля нет</returns>
    public int IndexOf(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        return -1;
#if DEBUG
      if (columnName.IndexOf(',') >= 0)
        throw new ArgumentException("Неправильное имя поля \"" + columnName + "\". Может быть задано только одно поле для поиска", "columnName");
#endif

      if (_Indexer == null)
        return Array.IndexOf<string>(_Items, columnName);
      else
        return _Indexer.IndexOf(columnName);
    }

    /// <summary>
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в OtherColumns
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в OtherColumns</returns>
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
    /// Возвращает true, если в списке полей ThisColumns есть поля, которых нет в OtherColumns
    /// Статическая версия метода позволяет не проверять на null оба аргумента
    /// </summary>
    /// <param name="thisColumns">Текущий список полей (может быть null)</param>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в OtherColumns</returns>
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
    /// Если метод вызывается для создания ссылочных полей, не забудьте включить точку в префикс
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
    /// Извлечение подмассива из Values для полей данного объекта
    /// Если в ColumnNames нет какого-либо поля, возвращается null
    /// </summary>
    /// <param name="columnNames">Имена большего набора полей</param>
    /// <param name="values">Значения большего набора полей</param>
    /// <returns>Массив значений длиной Count элементов или null, если какого-нибудь поля не хватает в ColumnNames</returns>
    public object[] ExtractColumnValues(DBxColumns columnNames, object[] values)
    {
      if (columnNames == null)
        return new object[0];
      if (values == null)
        throw new ArgumentNullException("values");
      if (values.Length != columnNames.Count)
        throw new ArgumentException("Массив значений не соответствует списку полей", "values");

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
    /// Добавить к коллекции столбцов DataTable все поля в объекте.
    /// Поля будут иметь одинаковый тип
    /// </summary>
    /// <param name="columns">Коллекция столбцов в DataTable, куда добавляются столбцы</param>
    /// <param name="dataType">Тип значений, хранящихся в столбцах</param>
    public void AddColumns(DataColumnCollection columns, Type dataType)
    {
      for (int i = 0; i < Count; i++)
        columns.Add(this[i], dataType);
    }

    /// <summary>
    /// Добавить к столбцам таблицы DataTable столбцы с указанными именами, если они
    /// имеются в текущем списке полей
    /// </summary>
    /// <param name="columns">Столбцы DataTable.Columns, куда выполняется добавление</param>
    /// <param name="columnNames">Список имен добавляемых столбцов, разделенных запятыми</param>
    /// <param name="dataType">Тип значений, хранящихся в добавляемых столбцах</param>
    public void AddContainedColumns(DataColumnCollection columns, string columnNames, Type dataType)
    {
      if (String.IsNullOrEmpty(columnNames))
        return;
      AddContainedColumns(columns, columnNames.Split(','), dataType);
    }

    /// <summary>
    /// Добавить к столбцам таблицы DataTable столбцы с указанными именами, если они
    /// имеются в текущем списке полей
    /// </summary>
    /// <param name="columns">Столбцы DataTable.Columns, куда выполняется добавление</param>
    /// <param name="columnNames">Массив имен добавляемых столбцов</param>
    /// <param name="dataType">Тип добавляемых столбцов</param>
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
    /// Добавить к столбцам таблицы DataTable столбцы из текущего списка, используя типы данных из
    /// столбцов исходной таблицы <paramref name="sourceColumns"/>. Если в структцре исходной таблицы нет
    /// какого-либо столбца, генерируется исключение.
    /// </summary>
    /// <param name="columns">Столбцы DataTable.Columns, куда выполняется добавление</param>
    /// <param name="sourceColumns">Структура таблицы, откуда берутся типы столбцов</param>
    public void AddContainedColumns(DataColumnCollection columns, DataColumnCollection sourceColumns)
    {
      for (int i = 0; i < _Items.Length; i++)
      {
        int p = sourceColumns.IndexOf(_Items[i]);
        if (p < 0)
          throw new InvalidOperationException("Таблица-шаблон не содержит поля \"" + _Items[i] + "\"");
        DataColumn SrcColumn = sourceColumns[p];
        columns.Add(DataTools.CloneDataColumn(SrcColumn));
      }
    }

    /// <summary>
    /// Если таблица <paramref name="sourceTable"/> имеет список столбцов, совпадающий с текущим,
    /// она возвращается без изменений. Иначе возвращается копия таблицы со столбцами из текущего объекта
    /// Если <paramref name="sourceTable"/> не содержит какого-либо столбца из текущего списка, 
    /// генерируется исключение
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
        if (String.Compare(this[i], sourceTable.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase) != 0)
          return CreateSubTable(sourceTable);
      }

      return sourceTable;
    }

    /// <summary>
    /// Создает копию таблицы, содержащую поля из текущего списка.
    /// Если <paramref name="sourceTable"/> не содержит какого-либо столбца из текущего списка, 
    /// генерируется исключение
    /// </summary>
    /// <param name="sourceTable">Исходная таблица</param>
    /// <returns>Копия таблицы</returns>
    public DataTable CreateSubTable(DataTable sourceTable)
    {
      if (sourceTable == null)
        throw new ArgumentNullException("sourceTable");

      DataTable ResTable = new DataTable();
      for (int i = 0; i < Count; i++)
      {
        int p = sourceTable.Columns.IndexOf(this[i]);
        if (p < 0)
          throw new ArgumentException("Таблица \"" + sourceTable.TableName + "\" не содержит столбца \"" + this[i] + "\"", "sourceTable");

        ResTable.Columns.Add(DataTools.CloneDataColumn(sourceTable.Columns[i]));
      }

      DataTools.CopyRowsToRows(sourceTable, ResTable, true, true);
      return ResTable;
    }

    #endregion

    #region Манипуляции с DataRow

    /// <summary>
    /// Извлечь из строки значения полей. Если строка не содержит каких-либо полей,
    /// то они получают значение null. Длина выходного массива равна Count
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
    /// от параметра ThrowIfNoColumn. Длина выходного массива равна Count
    /// </summary>
    /// <param name="row">Строка, откуда будут браться значения</param>
    /// <param name="throwIfNoColumn">Если true, то при отсутствии в строке <paramref name="row"/>
    /// одного из полей выбрасывается исключение. Еслb false, то для отсутствующих
    /// полей будут возвращены значения null</param>
    /// <returns>Массив полученных значений</returns>
    public object[] GetRowValues(DataRow row, bool throwIfNoColumn)
    {
      object[] Values = new object[Count];
      for (int i = 0; i < Values.Length; i++)
      {
        int p = row.Table.Columns.IndexOf(this[i]);
        if (p < 0)
        {
          if (throwIfNoColumn)
            throw new ArgumentException("Строка таблицы \"" + row.Table.TableName +
              "\" не содержит столбца \"" + this[i] + "\"", "row");
          continue;
        }
        Values[i] = row[p];
      }
      return Values;
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
        throw new ArgumentException("Неправильная длина массива значений", "values");
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
        return arg2;
      if (arg2 == null)
        return arg1;

      return new DBxColumns(arg1, arg2);
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
      if (String.IsNullOrEmpty(columnNames))
        return arg1;
      if (arg1 == null)
        return new DBxColumns(columnNames);
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
      if (columnNames == null || columnNames.Length == 0)
        return arg1;
      if (arg1 == null)
        return new DBxColumns(columnNames);
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
        return null;
      if (arg2 == null)
        return arg1;

      DBxColumnList List = new DBxColumnList(arg1);
      List.Remove(arg2);

      return new DBxColumns(List);
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
      if (String.IsNullOrEmpty(columnNames))
        return arg1;
      if (arg1 == null)
        return null;
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
      if (columnNames == null || columnNames.Length == 0)
        return arg1;
      if (arg1 == null)
        return null;
      return arg1 - new DBxColumns(columnNames);
    }

    #endregion

    #region And

    /// <summary>
    /// Возвращает список полей, которые содержатся в обоих исходных списках.
    /// Если один или оба списка null, возвращается null.
    /// </summary>
    /// <param name="arg1">Первый список</param>
    /// <param name="arg2">Второй список</param>
    /// <returns>Результат пересечения</returns>
    public static DBxColumns operator &(DBxColumns arg1, DBxColumns arg2)
    {
      if (arg1 == null || arg2 == null)
        return null;

      List<string> ColumnNames = new List<string>();
      for (int i = 0; i < arg1.Count; i++)
      {
        if (arg2.Contains(arg1[i]))
          ColumnNames.Add(arg1[i]);
      }
      if (ColumnNames.Count == 0)
        return null;
      else
        return new DBxColumns(ColumnNames);
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
      if (arg1 == null || String.IsNullOrEmpty(columnNames))
        return null;
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
      if (arg1 == null || columnNames == null || columnNames.Length == 0)
        return null;
      return arg1 & new DBxColumns(columnNames);
    }

    #endregion

    #endregion

    #region IEnumerable<string> Members

    /// <summary>
    /// Возвращает перечислитель по именам столбцов.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerator<string> GetEnumerator()
    {
      return new ArrayEnumerator<string>(_Items);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return new ArrayEnumerator<string>(_Items);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<string>(_Items);
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
    /// Создает список, в которой будут добавлены имена столбцов из источника
    /// </summary>
    /// <param name="src">Список столбцов для добавления</param>
    public DBxColumnList(ICollection<string> src)
      : base(src)
    {
    }

    /// <summary>
    /// Создает список, в которой будут добавлены имена столбцов из источника
    /// </summary>
    /// <param name="src">Список столбцов для добавления</param>
    public DBxColumnList(IEnumerable<string> src)
      : base(src)
    {
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
        base.Add(сolumnNames);
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
    /// </summary>
    /// <param name="columnNamePrefix">Префикс имени поля для поиска</param>
    /// <returns>true, если есть поле с подходящим именем</returns>
    public bool ContainsStartedWith(string columnNamePrefix)
    {
      if (String.IsNullOrEmpty(columnNamePrefix))
        return Count > 0; // 17.10.2019

      for (int i = 0; i < Count; i++)
      {
        if (this[i].StartsWith(columnNamePrefix))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в OtherColumns
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в OtherColumns</returns>
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
    /// Возвращает true, если в текущем списке полей есть поля, которых нет в OtherColumns
    /// </summary>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в OtherColumns</returns>
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
    /// Возвращает true, если в списке полей ThisColumns есть поля, которых нет в OtherColumns
    /// Статическая версия метода позволяет не проверять на null оба аргумента
    /// </summary>
    /// <param name="thisColumns">Текущий список полей (может быть null)</param>
    /// <param name="otherColumns">Список проверяемых полей (может быть null)</param>
    /// <returns>true, если есть поля, отсутствующие в OtherColumns</returns>
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
    /// Повторные вызовы методы игнорируются
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка со сброшенным свойством IsReadOnly
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
    /// У копии списка сброшено свойство IsReadOnly
    /// Если префикс не задан, выполняется обычное клоинрование.
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
    /// У копии списка сброшено свойство IsReadOnly
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
  }

  /// <summary>
  /// Хранилище для массива значений полей, связанное с заданным списком столбцов DBxColumns.
  /// Однократно созданному объекту можно многократно присваивать значения (свойство Values).
  /// Используется при проверке фильтров.
  /// </summary>
  [Serializable]
  public class DBxColumnValueArray : INamedValuesAccess
  {
    #region Конструктор

    /// <summary>
    /// Создает список с заданными значениями.
    /// дальнейшая установка свойства Values позволит их изменить.
    /// </summary>
    /// <param name="columns">Список столбцов</param>
    /// <param name="values">Массив значений. Должен иметь ту же длину, что и список <paramref name="columns"/>.</param>
    public DBxColumnValueArray(DBxColumns columns, object[] values)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");
      _Columns = columns;
      this.Values = values; // там проверяется длина массива
    }

    /// <summary>
    /// Создает список, в котором все значения пустые
    /// </summary>
    /// <param name="columns">Список столбцов</param>
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
    private DBxColumns _Columns;

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
          throw new ArgumentException("Неправильная длина массива значений. Ожидалось значений: " + _Columns.Count.ToString() + ", передано: " + value.Length.ToString());
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
            throw new ArgumentNullException("columnName");
          else
            throw new ArgumentException("В списке значений нет столбца с именем \"" + columnName + "\"", "columnName");
        }
        return _Values[p];
      }
    }

    /// <summary>
    /// Возващает текстовое представление для Columns
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
