using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

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

namespace AgeyevAV.ExtDB.Docs
{
  /*
   * Реализация текстового представления для документов
   * Нельзя реализовать событие в DBxDocType, т.к. требуется получение текста и на стороне клиента и на стороне
   * сервера. Если использовать Marshal-by-ref, то упадет производительность. Следовательно, обработчик должен
   * быть реализован и там и там. После передачи клиенту, объект DBxDocType переходит в режим read only и его
   * нельзя инициализировать
   * 
   * Пользователь, к которому (косвенно, через DBxDocProvider) относится DBxCache, может не иметь доступа к 
   * части документов и поддокументов. Чтобы не делать инициализатор объекта слишком сложным, в процессе
   * заполнения можно использовать любые имена таблиц и столбцов. 
   */

  #region DBxTextValueNeededEventHandler

  /// <summary>
  /// Базовый класс для DBxTextValueNeededEventArgs и DBxImageValueNeededEventArgs
  /// Содержит исходные данные, но не результаты заполнения
  /// </summary>
  public abstract class DBxValueNeededEventArgsBase : EventArgs
  {
    #region Методы

    /// <summary>
    /// Этот метод вызывается перед вызовом события для установки значений
    /// </summary>
    /// <param name="tableName">Имя документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="columnNames">Имена полей, предназначенных для отображения</param>
    /// <param name="values">Массив значений столбцов <paramref name="columnNames"/></param>
    protected void InitData(string tableName, Int32 id, DBxColumns columnNames, object[] values)
    {
      _TableName = tableName;
      _Id = id;
      _ColumnNames = columnNames;
      _Values = values;
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public object GetValue(int columnIndex) { return _Values[columnIndex]; }

    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public object GetValue(string columnName)
    {
      int p = _ColumnNames.IndexOf(columnName);
      if (p < 0)
        throw new ArgumentException("Поле \"" + columnName + "\" не было объявлено добавлено в список требуемых для полученния текстового представления таблицы \"" + TableName + "\". Допустимые поля: " + _ColumnNames.ToString(), "columnName");
      return _Values[p];
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя документа или поддокумента
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Идентификатор документа или поддокумента.
    /// Не может быть равен 0
    /// </summary>
    public Int32 Id { get { return _Id; } }
    private Int32 _Id;

    /// <summary>
    /// Имена полей, предназначенных для отображения
    /// </summary>
    public DBxColumns ColumnNames { get { return _ColumnNames; } }
    private DBxColumns _ColumnNames;


    private object[] _Values;

    #endregion

    #region Типизированный доступ к значениям

    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public string GetString(string columnName)
    {
      return DataTools.GetString(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public string GetString(int columnIndex)
    {
      return DataTools.GetString(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public int GetInt(string columnName)
    {
      return DataTools.GetInt(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public int GetInt(int columnIndex)
    {
      return DataTools.GetInt(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public float GetSingle(string columnName)
    {
      return DataTools.GetSingle(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public float GetSingle(int columnIndex)
    {
      return DataTools.GetSingle(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public double GetDouble(string columnName)
    {
      return DataTools.GetDouble(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public double GetDouble(int columnIndex)
    {
      return DataTools.GetDouble(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public decimal GetDecimal(string columnName)
    {
      return DataTools.GetDecimal(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public decimal GetDecimal(int columnIndex)
    {
      return DataTools.GetDecimal(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public bool GetBool(string columnName)
    {
      return DataTools.GetBool(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public bool GetBool(int columnIndex)
    {
      return DataTools.GetBool(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public DateTime GetDateTime(string columnName)
    {
      return DataTools.GetDateTime(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public DateTime GetDateTime(int columnIndex)
    {
      return DataTools.GetDateTime(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public Nullable<DateTime> GetNullableDateTime(string columnName)
    {
      return DataTools.GetNullableDateTime(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public Nullable<DateTime> GetNullableDateTime(int columnIndex)
    {
      return DataTools.GetNullableDateTime(GetValue(columnIndex));
    }

    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public TimeSpan GetTimeSpan(string columnName)
    {
      return DataTools.GetTimeSpan(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public TimeSpan GetTimeSpan(int columnIndex)
    {
      return DataTools.GetTimeSpan(GetValue(columnIndex));
    }

    /// <summary>
    /// Получить значение поля по имени.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public Guid GetGuid(string columnName)
    {
      return DataTools.GetGuid(GetValue(columnName));
    }

    /// <summary>
    /// Получить значение по индексу поля
    /// </summary>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public Guid GetGuid(int columnIndex)
    {
      return DataTools.GetGuid(GetValue(columnIndex));
    }


    /// <summary>
    /// Получить значение перечислимого типа по имени поля.
    /// Если в списке ColumnNames нет поля <paramref name="columnName"/>, генерируется исключение.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public T GetEnum<T>(string columnName)
      where T : struct
    {
      return DataTools.GetEnum<T>(GetValue(columnName));
    }


    /// <summary>
    /// Получить значение перечислимого типа по индексу поля
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="columnIndex">Индекс поля от 0 до (ColumnNames.Count-1)</param>
    /// <returns>Значение поля</returns>
    public T GetEnum<T>(int columnIndex)
      where T : struct
    {
      return DataTools.GetEnum<T>(GetValue(columnIndex));
    }

    #endregion
  }

  /// <summary>
  /// Аргументы для событий Client/Server[Sub]DocType.TextValueNeeded
  /// </summary>
  public class DBxTextValueNeededEventArgs : DBxValueNeededEventArgsBase
  {
    #region Конструктор

    internal DBxTextValueNeededEventArgs()
    {
      _Text = new StringBuilder();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Этот метод вызывается перед вызовом события для установки значений
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <param name="columnNames"></param>
    /// <param name="values"></param>
    internal new void InitData(string tableName, Int32 id, DBxColumns columnNames, object[] values)
    {
      base.InitData(tableName, id, columnNames, values);
      _Text.Length = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сюда должно быть помещено текстовое значение
    /// </summary>
    public StringBuilder Text { get { return _Text; } }
    private StringBuilder _Text;

    #endregion
  }

  /// <summary>
  /// Делегат для событий Client/Server[Sub]DocType.TextValueNeeded
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void DBxTextValueNeededEventHandler(object sender,
    DBxTextValueNeededEventArgs args);

  #endregion

  /***
   * Если это сделать, то в DBxDocProvider методы для документов/поддокументов будут передавать весь DataSet на сервер

  /// <summary>
  /// Методы получения текста, реализуемые DBxDocTextHandlers.
  /// Они также реализованы в DBxDocProvider
  /// </summary>
  public interface IDBxDocTextHandlers
  {
    #region Методы

    string GetTextValue(string TableName, Int32 Id);
    string GetTextValue(DBxSingleDoc Doc);
    string GetTextValue(DBxSubDoc SubDoc);
    string GetRefTextValue(DataRow Row, string RefColumnName);

    #endregion
  }
      ***/

  /// <summary>
  /// Система получения текстового представления полей документов и поддокументов.
  /// Объект DBxDocTextHandlers может использоваться и на стороне клиента, и на стороне сервера.
  /// При этом требуется независимая инициализация объекта.
  /// В части извлечения значений объекты являются потокобезопасными, если используемый DBxCache является потокобезопасным. 
  /// В процессе установки обработчиков и заполнения полей, объект не является безопасным.
  /// </summary>
  /// <remarks>
  /// Порядок работы:
  /// 1. Автоматически создается DBxDocTextHandlers (на стороне сервера - конструктором DBxRealDocProviderGlobal, на стороне клиента - конструктором DBUI)
  /// 2. Объект инициализируется пользовательским кодом (вызовы Add())
  /// 3. Автоматически вызывается SetReadOnly(). При этом объект переводится в режим ReadOnly (на стороне сервера - конструктором DBxRealDocProviderSource,
  /// на стороне клиента - методом DBUI.EndInit())
  /// 4. Выполняются вызовы для получения текста документов
  /// </remarks>
  public class DBxDocTextHandlers : /* IDBxDocTextHandlers, */ IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для получения текстовых представлений
    /// </summary>
    /// <param name="docTypes">Описание видов документов</param>
    /// <param name="dbCache">Система кэширования</param>
    public DBxDocTextHandlers(DBxDocTypes docTypes, DBxCache dbCache)
    {
      if (docTypes == null)
        throw new ArgumentNullException("docTypes");
      if (dbCache == null)
        throw new ArgumentNullException("dbCache");

      _DocTypes = docTypes;
      _DBCache = dbCache;

      _TableItems = new Dictionary<string, TableHandler>();
      _Args = new DBxTextValueNeededEventArgs();

      _ColumnsId = new DBxColumns("Id");
      if (docTypes.UseDeleted)
        _ColumnsDoc = new DBxColumns("Id,Deleted");
      else
        _ColumnsDoc = _ColumnsId;

      if (docTypes.UseDeleted)
        _ColumnsSubDoc = new DBxColumns("Id,DocId,Deleted");
      else
        _ColumnsSubDoc = new DBxColumns("Id,DocId");
    }

    #endregion

    #region Списки полей для запроса

    internal DBxColumns ColumnsId { get { return _ColumnsId; } }
    private DBxColumns _ColumnsId;

    internal DBxColumns ColumnsDoc { get { return _ColumnsDoc; } }
    private DBxColumns _ColumnsDoc;

    internal DBxColumns ColumnsSubDoc { get { return _ColumnsSubDoc; } }
    private DBxColumns _ColumnsSubDoc;

    #endregion

    #region Источник данных

    /// <summary>
    /// Описание видов документов.
    /// Задается в конструкторе
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private DBxDocTypes _DocTypes;

    /// <summary>
    /// Источник для получения значений полей.
    /// Задается в конструторе
    /// </summary>
    public DBxCache DBCache
    {
      get { return _DBCache; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (object.ReferenceEquals(value, _DBCache))
          return;

        lock (_TableItems)
        {
          _DBCache = value;
          foreach (KeyValuePair<string, TableHandler> Pair in _TableItems)
          {
            Pair.Value.AccessDeniedFlag = false;
          }
        }
      }
    }
    private DBxCache _DBCache;

    #endregion

    #region Инициализация обработчиков

    /// <summary>
    /// Добавить обработчик
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnNames">Список столбцов, которые используются для текстового представления.
    /// Имена могут содержать точки для доступа к связанным документам</param>
    /// <param name="textValueNeeded">Обработчик для форматирования текста.
    /// Если обработчик задан, то он вызывается при каждом получении текста.
    /// Если обработчика нет, то выводятся все стролбцы из списка <paramref name="columnNames"/>,
    /// разделенные пробелами</param>
    public void Add(string tableName, DBxColumns columnNames, DBxTextValueNeededEventHandler textValueNeeded)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.Count == 0)
        throw new ArgumentException("Список полей для получения текстового представления таблицы \"" + tableName + "\" пустой", "columnNames");

      lock (_TableItems)
      {
        CheckNotReadOnly();

        // Удаляем в случае повторного вызова метода Add
        _TableItems.Remove(tableName);

        TableHandler Handler = new TableHandler(this, tableName, columnNames, textValueNeeded);
        _TableItems.Add(tableName, Handler);
      }
    }

    /// <summary>
    /// Добавить список столбцов для текстового представления документа.
    /// Обработчик не используется.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnNames">Список столбцов, которые используются для текстового представления.
    /// Имена могут содержать точки для доступа к связанным документам</param>
    public void Add(string tableName, DBxColumns columnNames)
    {
      Add(tableName, columnNames, null);
    }

    /// <summary>
    /// Добавить обработчик
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnNames">Список столбцов, которые используются для текстового представления.
    /// Имена могут содержать точки для доступа к связанным документам.
    /// Имена столбцов разделяются запятыми</param>
    /// <param name="textValueNeeded">Обработчик для форматирования текста.
    /// Если обработчик задан, то он вызывается при каждом получении текста.
    /// Если обработчика нет, то выводятся все стролбцы из списка <paramref name="columnNames"/>,
    /// разделенные пробелами</param>
    public void Add(string tableName, string columnNames, DBxTextValueNeededEventHandler textValueNeeded)
    {
      Add(tableName, new DBxColumns(columnNames), textValueNeeded);
    }

    /// <summary>
    /// Добавить список столбцов для текстового представления документа.
    /// Обработчик не используется.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnNames">Список столбцов, которые используются для текстового представления.
    /// Имена могут содержать точки для доступа к связанным документам.
    /// Имена столбцов разделяются запятыми</param>
    public void Add(string tableName, string columnNames)
    {
      Add(tableName, new DBxColumns(columnNames), null);
    }

    #endregion

    #region Получение текста

    /// <summary>
    /// Используем единственный экземпляр объекта, т.к. при запросе выполняется блокировка
    /// </summary>
    private DBxTextValueNeededEventArgs _Args;

    /// <summary>
    /// Получить текстовое представление документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(string tableName, Int32 id)
    {
      try
      {
        return DoGetTextValue(tableName, id, null, false);
      }
      catch (Exception e)
      {
        return "Id=" + id.ToString() + ". Ошибка. " + e.Message;
      }
    }

    /// <summary>
    /// Получить текстовое представление документа или поддокумента.
    /// В отличие от GetTextValue(), в текст обязательно добавляется идентификатор документа
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValueWithId(string tableName, Int32 id)
    {
      try
      {
        return DoGetTextValue(tableName, id, null, true);
      }
      catch (Exception e)
      {
        return "Id=" + id.ToString() + ". Ошибка. " + e.Message;
      }
    }

    internal string DoGetTextValue(string tableName, Int32 id, DataSet primaryDS, bool withId)
    {
      if (id == 0)
        return String.Empty;

      lock (_TableItems)
      {
        //if (FDocProvider == null)
        //  return Id.ToString(); // источник данных не присоединен

        TableHandler Handler;
        if (!_TableItems.TryGetValue(tableName, out Handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");

          Handler = new TableHandler(this, tableName, ColumnsId, null);
          _TableItems.Add(tableName, Handler);
        }

        if (Handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return id.ToString();
        if (Handler.AccessDeniedFlag)
          return id.ToString(); // 10.07.2018

        object[] Values;
        try
        {
          if (id < 0)
          {
            Values = InternalGetValues(tableName, id, Handler.QueriedColumnNames, primaryDS); // 27.03.2018
            if (Values == null)
              return "?? Id=" + id.ToString();
          }
          else
            Values = DBCache[tableName].GetValues(id, Handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
        }
        catch (DBxAccessException)
        {
          Handler.AccessDeniedFlag = true; // 10.07.2018
          throw;
        }
        _Args.InitData(tableName, id, Handler.QueriedColumnNames, Values);
        if (Handler.TextValueNeeded != null)
        {
          try
          {
            Handler.TextValueNeeded(this, _Args);
          }
          catch (Exception e)
          {
            if (_Args.Text.Length > 0) // часть текста была добавлена
              _Args.Text.Append(". ");
            _Args.Text.Append("Ошибка: ");
            _Args.Text.Append(e.Message);
          }
        }
        else
        {
          // Без обработчика помещаем текстовые значения через запятую
          for (int i = 0; i < Handler.ColumnNames.Count; i++)
          {
            if (Values[i] == null)
              continue;
            if (Values[i] is DBNull)
              continue;
            string s2 = Values[i].ToString().Trim();
            if (String.IsNullOrEmpty(s2))
              continue;

            if (i > 0)
              _Args.Text.Append(' ');
            _Args.Text.Append(s2);
          }
        }

        if (_Args.Text.Length == 0)
        {
          _Args.Text.Append("Id=");
          _Args.Text.Append(id);
        }
        else if (withId && (!Handler.ColumnNames.Contains("Id")))
        {
          _Args.Text.Append(" (Id=");
          _Args.Text.Append(id);
          _Args.Text.Append(")");
        }

        // Добавляем информацию об удаленном документе
        if (_DocTypes.UseDeleted)
        {
          if (Handler.SubDocType == null)
          {
            if (_Args.GetBool("Deleted"))
              _Args.Text.Append(" (удален)");
          }
          else
          {
            if (_Args.GetBool("Deleted"))
              _Args.Text.Append(" (удален)");
            if (GetDocIdDeleted(Handler))
              _Args.Text.Append(" (в удаленном документе)");
          }
        }

        return _Args.Text.ToString();
      }
    }


    private bool GetDocIdDeleted(TableHandler handler)
    {
      Int32 docId = _Args.GetInt("DocId");
      if (docId <= 0)
        return false;

      return _DBCache[handler.DocType.Name].GetBool(docId, "Deleted");
    }


    /// <summary>
    /// Извлечение значений из набора <paramref name="primaryDS"/> для строки,
    /// когда нельзя обращаться к DBxCache (фиктивный Id)
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <param name="columnNames"></param>
    /// <param name="primaryDS"></param>
    /// <returns></returns>
    private object[] InternalGetValues(string tableName, Int32 id, DBxColumns columnNames, DataSet primaryDS)
    {
      DataRow Row = InternalGetRow(tableName, id, primaryDS);
      if (Row == null)
        return null;

      object[] a = new object[columnNames.Count];
      for (int i = 0; i < columnNames.Count; i++)
        a[i] = InternalGetValue(Row, tableName, columnNames[i], primaryDS);
      return a;
    }

    private DataRow InternalGetRow(string tableName, Int32 id, DataSet primaryDS)
    {
      if (primaryDS == null)
        return null;

      DataTable Table = primaryDS.Tables[tableName];
      if (Table == null)
        return null;

      return Table.Rows.Find(id);
    }

    private object InternalGetValue(DataRow row, string tableName, string columnName, DataSet primaryDS)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      int p = columnName.IndexOf('.');
      if (p < 0)
      {
        // Простое поле таблицы
        if (row.Table.Columns.Contains(columnName))
          return row[columnName];
        else
          return null;
      }
      else
      {
        string RefColumnName = columnName.Substring(0, p);
        Int32 RefId = DataTools.GetInt(InternalGetValue(row, tableName, RefColumnName, primaryDS));
        if (RefId == 0)
          return null; // пустая ссылка

        DBxTableStruct TableStruct = DBCache[tableName].TableStruct;

        string ExtTableName = TableStruct.Columns[RefColumnName].MasterTableName;
        if (String.IsNullOrEmpty(ExtTableName))
          throw new ArgumentException("Поле \"" + ExtTableName + "\" таблицы \"" + tableName + "\" не является ссылочным", "columnName");
        string ExtColumnName = columnName.Substring(p + 1);

        DataRow Row2 = InternalGetRow(ExtTableName, RefId, primaryDS);
        if (Row2 == null)
        {
          if (RefId > 0)
            return DBCache[ExtTableName].GetValue(RefId, ExtColumnName);
          else
            return null;
        }
        return InternalGetValue(Row2, ExtTableName, ExtColumnName, primaryDS);
      }
    }

    #endregion

    #region Получение текста для объекта документа / поддокумента

    /// <summary>
    /// Получить текстовое представление для документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(DBxSingleDoc doc)
    {
      try
      {
        return DoGetTextValue(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, false);
      }
      catch (Exception e)
      {
        return "DocId=" + doc.DocId.ToString() + ". Ошибка. " + e.Message;
      }
    }

    /// <summary>
    /// Получить текстовое представление для поддокумента
    /// </summary>
    /// <param name="subDoc">Поддокумент</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(DBxSubDoc subDoc)
    {
      try
      {
        return DoGetTextValue(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, false);
      }
      catch (Exception e)
      {
        return "SubDocId=" + subDoc.SubDocId.ToString() + ". Ошибка. " + e.Message;
      }
    }

    /// <summary>
    /// Получить значение для ссылочного поля
    /// </summary>
    /// <param name="row">Строка документа или поддокумента</param>
    /// <param name="refColumnName">Имя ссылочного поля</param>
    /// <returns>Текстовое представление</returns>
    public string GetRefTextValue(DataRow row, string refColumnName)
    {
      try
      {
        Int32 RefId = DataTools.GetInt(row, refColumnName);
        if (RefId == 0)
          return String.Empty;
        else
          return DoGetTextValue(row.Table.TableName, RefId, row.Table.DataSet, false);
      }
      catch (Exception e)
      {
        return "Ошибка. " + e.Message;
      }
    }

    #endregion

    #region Получение текста документа по идентификатору TableId

    /// <summary>
    /// Получить текстовое представление документа
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(Int32 tableId, Int32 docId)
    {
      try
      {
        if (tableId == 0 || docId == 0)
          return String.Empty;
        string tableName = DocTypes.GetTableNameById(tableId);
        return DoGetTextValue(tableName, docId, null, false);
      }
      catch (Exception e)
      {
        return "TableId=" + tableId + ", DocId=" + docId.ToString() + ". Ошибка. " + e.Message;
      }
    }

    #endregion

    #region Внутренний доступ к таблицам

    private class TableHandler
    {
      #region Конструктор

      public TableHandler(DBxDocTextHandlers owner, string tableName, DBxColumns columnNames, DBxTextValueNeededEventHandler textValueNeeded)
      {
        _TableName = tableName;
        _ColumnNames = columnNames;
        _TextValueNeeded = textValueNeeded;

        owner.DocTypes.FindByTableName(tableName, out _DocType, out _SubDocType);

        if (DocType != null)
        {
          if (SubDocType == null)
            _QueriedColumnNames = columnNames + owner.ColumnsDoc;
          else
            _QueriedColumnNames = columnNames + owner.ColumnsSubDoc;
        }
        else
          _QueriedColumnNames = owner.ColumnsId;

      }

      #endregion

      #region Свойства, задаваемые в конструкторе

      /// <summary>
      /// Имя таблицы
      /// </summary>
      public string TableName { get { return _TableName; } }
      private string _TableName;

      /// <summary>
      /// Список полей, заданных пользователем
      /// </summary>
      public DBxColumns ColumnNames { get { return _ColumnNames; } }
      private DBxColumns _ColumnNames;

      public DBxTextValueNeededEventHandler TextValueNeeded { get { return _TextValueNeeded; } }
      private DBxTextValueNeededEventHandler _TextValueNeeded;

      public override string ToString()
      {
        return TableName;
      }

      /// <summary>
      /// Вид документа, к которому относится таблица (если найдено)
      /// </summary>
      public DBxDocType DocType { get { return _DocType; } }
      private DBxDocType _DocType;

      /// <summary>
      /// Вид поддокумента, к которому относится таблица, или null, если таблица относится к документу
      /// </summary>
      public DBxSubDocType SubDocType { get { return _SubDocType; } }
      private DBxSubDocType _SubDocType;

      /// <summary>
      /// Список полей, используемых для запросов.
      /// В начале идут поля из ColumnNames, затем - Id, DocId и Deleted
      /// Если null, значит объект TableHandler еще не был инициализирован для DocProvider
      /// </summary>
      public DBxColumns QueriedColumnNames { get { return _QueriedColumnNames; } }
      private DBxColumns _QueriedColumnNames;

      #endregion

      #region Флажок доступа

      /// <summary>
      /// true, если при попытке доступа к полям возникло исключение DBxAccessException.
      /// Предотврашает повторное обращение к серверу за кэшем страницы, чтобы избежать
      /// перегрузку канала связи
      /// </summary>
      public bool AccessDeniedFlag;

      #endregion
    }

    private Dictionary<string, TableHandler> _TableItems;

    #endregion

    #region Получение дополнительных сведений

    /// <summary>
    /// Получить список полей, из которых собирается значение.
    /// Если список полей не был задан в явном виде, то при <paramref name="forQuery"/>=false возвращается null.
    /// При <paramref name="forQuery"/>=true возвращается минимально необходимый список полей.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="forQuery">Если true, то возвращается расширенный список, который используется в запросах.
    /// Если false - то только те поля, которые были заданы пользователем в методе Add</param>
    /// <returns>Список полей</returns>
    public DBxColumns GetColumnNames(string tableName, bool forQuery)
    {
      lock (_TableItems)
      {
        TableHandler Handler;
        if (_TableItems.TryGetValue(tableName, out Handler))
        {
          if (forQuery)
            return Handler.QueriedColumnNames;
          else
            return Handler.ColumnNames;
        }
        else
        {
          if (forQuery)
          {
            DBxDocType dt;
            DBxSubDocType sdt;
            DocTypes.FindByTableName(tableName, out dt, out sdt);

            if (dt != null)
            {
              if (sdt == null)
                return ColumnsDoc;
              else
                return ColumnsSubDoc;
            }
            else
              return ColumnsId;
          }
          else
            return null;
        }
      }
    }

    /// <summary>
    /// Возвращает true, если текстовое представление определено
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>true, если список полей задан или обработчик задан</returns>
    public bool Contains(string tableName)
    {
      bool Flag;
      lock (_TableItems)
      {
        Flag = _TableItems.ContainsKey(tableName);
      }
      return Flag;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект находится в режиме "только чтение".
    /// При этом нельзя больше добавлять обработчики
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит список обработчиков в режим "только-чтение".
    /// Повторные вызовы игнорируется
    /// </summary>
    public void SetReadOnly()
    {
      if (!IsReadOnly)
        InitGroupDocsDefaults();
      _IsReadOnly = true;
    }

    #endregion

    #region Обработчики для документов групп

    /// <summary>
    /// Если для документов-дерева групп не задано текстовое представление, используется первое строковое поле
    /// </summary>
    private void InitGroupDocsDefaults()
    {
      DBxDocType[] a = DocTypes.GetGroupDocTypes();
      lock (_TableItems)
      {
        for (int i = 0; i < a.Length; i++)
        {
          if (!_TableItems.ContainsKey(a[i].Name))
          {
            // Находим первое текстовое поле
            string NameColumnName = null;
            for (int j = 0; j < a[i].Struct.Columns.Count; j++)
            {
              DBxColumnStruct ColDef = a[i].Struct.Columns[j];
              if (ColDef.ColumnType == DBxColumnType.String && (!ColDef.Nullable))
              {
                NameColumnName = ColDef.ColumnName;
                break;
              }
            }
            if (String.IsNullOrEmpty(NameColumnName))
              throw new BugException("У документов \"" + a[i].PluralTitle + "\" нет ни одного текстового столбца. Эти документы не образуют дерево групп");
            this.Add(a[i].Name, NameColumnName);
          }
        }
      }
    }

    #endregion
  }
}
