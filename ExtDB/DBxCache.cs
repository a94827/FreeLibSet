// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using FreeLibSet.Caching;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Data;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Информация о сериализации полей таблицы.
  /// Объект является потокобезопасным после установки свойства IsReadOnly
  /// </summary>
  [Serializable]
  public sealed class DBxTableCacheInfo : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, связанный с описанием структуры таблицы
    /// </summary>
    /// <param name="tableStruct">Описание структуры таблицы. Не моет быть null</param>
    public DBxTableCacheInfo(DBxTableStruct tableStruct)
    {
      if (tableStruct == null)
        throw new ArgumentNullException("tableStruct");
      _TableStruct = tableStruct;
      _IndividualColumnFlags = new IndividualColumnFlagList(this);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Структура таблицы
    /// </summary>
    public DBxTableStruct TableStruct
    {
      get { return _TableStruct; }
      internal set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (_IndividualColumnFlags._Flags != null)
        {
          if (_IndividualColumnFlags._Flags.Length != value.Columns.Count)
            throw new BugException("Количество полей не соответствует структуре таблицы");
        }
        _TableStruct = value;
      }
    }

    /// <summary>
    /// Это поле не сериализуется. После передачи от сервера к клиенту, поле восстанавливается
    /// </summary>
    [NonSerialized]
    private DBxTableStruct _TableStruct;


    /// <summary>
    /// Список для свойства DBxTableCacheInfo.IndividualColumnFlags
    /// </summary>
    [Serializable]
    public sealed class IndividualColumnFlagList
    {
      #region Защищенный конструктор

      internal IndividualColumnFlagList(DBxTableCacheInfo owner)
      {
        _Owner = owner;
        _Flags = null;
      }

      #endregion

      #region Свойства

      private DBxTableCacheInfo _Owner;

      /// <summary>
      /// Свойство имеет значение true, если значения поля должно буферизоваться в индивидуальном порядке,
      /// а не при загрузке блока строк таблицы.
      /// Если свойство не установлено в явном виде, то возвращается true для полей типа Memo, Xml и Binary
      /// </summary>
      /// <param name="ColumnIndex">Индекс столбца в списке DBxTableStruct.Columns</param>
      /// <returns>true, если используется индивидуальная буферизация</returns>
      public bool this[int ColumnIndex]
      {
        get
        {
          if (_Flags == null)
            return GetDefaultValue(ColumnIndex);
          else
            return _Flags[ColumnIndex];
        }
        set
        {
          _Owner.CheckNotReadOnly();
          if (_Flags == null)
          {
            if (value == GetDefaultValue(ColumnIndex))
              return;
            InitFlags();
          }
          _Flags[ColumnIndex] = value;
        }
      }

      /// <summary>
      /// Массив флагов. Длина соответствует списку DbxTableStruct.Columns.
      /// Массив не создается, пока 
      /// </summary>
      internal bool[] _Flags;

      private bool GetDefaultValue(int columnIndex)
      {
        DBxColumnStruct Col = _Owner._TableStruct.Columns[columnIndex];
        return DBxTableCacheInfo.IsIndividualByDefault(Col);
      }

      /// <summary>
      /// Инициализация массива Flags
      /// </summary>
      private void InitFlags()
      {
        _Flags = new bool[_Owner._TableStruct.Columns.Count];
        for (int i = 0; i < _Owner._TableStruct.Columns.Count; i++)
          _Flags[i] = GetDefaultValue(i);
      }

      /// <summary>
      /// Свойство имеет значение true, если значения поля должно буферизоваться в индивидуальном порядке,
      /// а не при загрузке блока строк таблицы.
      /// Если свойство не установлено в явном виде, то возвращается true для полей типа Memo, Xml и Binary
      /// </summary>
      /// <param name="columnName">Имя столбца в списке DBxTableStruct.Columns</param>
      /// <returns>true, если используется индивидуальная буферизация</returns>
      public bool this[string columnName]
      {
        get { return this[GetColumnIndex(columnName)]; }
        set { this[GetColumnIndex(columnName)] = value; }
      }

      private int GetColumnIndex(string columnName)
      {
        int ColumnIndex = _Owner._TableStruct.Columns.IndexOf(columnName);
        if (ColumnIndex < 0)
          throw new ArgumentException("Поля \"" + columnName + "\" нет в описании структуры таблицы \"" + _Owner._TableStruct.TableName + "\"", "columnName");
        return ColumnIndex;
      }

      #endregion
    }

    /// <summary>
    /// Доступ к флажкам для отдельных полей.
    /// Если флажок для поля установлен, то поле не входит в таблицу буферизации, а щагружается отдельно
    /// </summary>
    public IndividualColumnFlagList IndividualColumnFlags { get { return _IndividualColumnFlags; } }
    private IndividualColumnFlagList _IndividualColumnFlags;

    /// <summary>
    /// Возвращает TableStruct.TableName.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_TableStruct == null)
        return "[No table struct]";
      else
        return _TableStruct.TableName;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание находится в режиме "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит описание в режим "только чтение".
    /// Повторные вызовы игнорируются
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Статический метод

    /// <summary>
    /// Возвращает true, если для данного столбца по умолчанию следует применять индивидуальную
    /// буферизацию значений полей
    /// </summary>
    /// <param name="column">Описание столбца</param>
    /// <returns>true, если рекомендуется индивидуальная буфериазация, false, если поле следует включить в состав страничной буферизации</returns>
    public static bool IsIndividualByDefault(DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        case DBxColumnType.Memo:
        case DBxColumnType.Xml:
        case DBxColumnType.Binary:
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  #region IDBxCacheSource

  /// <summary>
  /// Интерфейс источника данных для буферизации
  /// </summary>
  public interface IDBxCacheSource
  {
    /// <summary>
    /// Идентификатор базы данных (используется в качестве первого ключа при буферизации)
    /// </summary>
    string DBIdentity { get; }

    /// <summary>
    /// Источник данных для получения описаний структуры таблиц и полей.
    /// Если в программе предусмотрены ограничения прав пользователя, то структуры
    /// должны быть соответствующим образом "урезаны", чтобы не включать описания к тому, к чему
    /// у пользователя нет доступа
    /// </summary>
    IDBxStructSource StructSource { get; }

    /// <summary>
    /// Получить информацию о методах буферизации отдельных полей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Флажки буферизации полей. Метод может возвращать null, если используются стандартные
    /// правила буферизации</returns>
    DBxTableCacheInfo GetTableCacheInfo(string tableName);

    /// <summary>
    /// Получить страницы
    /// </summary>
    /// <param name="request">Запрос со списком требуемых страниц</param>
    /// <returns>Загруженные страницы</returns>
    DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request);

    ///// <summary>
    ///// Загрузить страницу кэша таблицы
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <param name="columnNames">Список столбцов</param>
    ///// <param name="firstIds">Первые идентификаторы для страниц</param>
    ///// <returns>Загруженный DataTable</returns>
    //DBxCacheTablePage[] LoadCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds);

    /// <summary>
    /// Выполнить сброс страницы данных на источнике.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Первые идентификаторы для страниц</param>
    /// <returns>Загруженный DataTable</returns>
    void ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds);

    ///// <summary>
    ///// Получить значения одного поля
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <param name="ids">Идентификаторы строк таблицы</param>
    ///// <param name="columnName">Имя столбца</param>
    ///// <returns>Значения поля для одной или нескольких строк</returns>
    //object[] LoadCacheIndividualValues(string tableName, Int32[] ids, string columnName);
  }

  #endregion

  /// <summary>
  /// Кэширование объектов базы данных
  /// Объект является потокобезопасным, при условии что источник данных является потокобезопасным или
  /// принадлежит исключительно данному объекту. При загрузке данных из источника Source, а также, 
  /// структуры таблицы, выполняется блокировка, чтобы исключить параллельные обращения.
  /// В качестве источника может выступать "личный" DBxCon
  /// Может применяться как на стороне сервера, так и на стороне клиента.
  /// Если есть несколько пользователей с разными правами доступа, у каждого пользователя должен быть свой
  /// объект DBxCache, ссылающийся на IDBxCacheSource, который возвращает только таблицы и поля, доступные пользователю
  /// </summary>
  public class DBxCache : IDBxCacheSource
  {
    /// <summary>
    /// Количество строк на одной странице кэша таблицы
    /// </summary>
    internal const int PageRowCount = 100;

    #region Конструктор

    /// <summary>
    /// Конструктор кэша.
    /// </summary>
    /// <param name="source">Источник для загрузки данных</param>
    /// <param name="currentThreadOnly">Если true, то объект будет использоваться искоючительно из потока,
    /// в котором создается объект (например, в главном потоке приложения клиента). В этом случае, источник данных
    /// можно использовать и для дургих целей в текущем потоке. 
    /// Если false, то предполагается многопоточное использования объекта буферизации, и источник данных должен
    /// быть в исключительном пользовании кэша</param>
    public DBxCache(IDBxCacheSource source, bool currentThreadOnly)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;

      _Guid = Guid.NewGuid();

      _Tables = new Dictionary<string, DBxTableCache>();

      _DBIdentity = source.DBIdentity;
      byte[] b = Encoding.Unicode.GetBytes(_DBIdentity);
      _DBIdentityMD5 = DataTools.MD5Sum(b);

      if (currentThreadOnly)
        _SingleThread = Thread.CurrentThread;

      _RepeatCount = 5;
      _RepeatDelay = 200;

      if (TraceSwitch.Enabled)
      {
        Trace.WriteLine(GetTracePrefix() + "DBxCache created.");
        Trace.IndentLevel++;
        Trace.WriteLine("DBIdentity=" + _DBIdentity);
        string SourceText = source.ToString();
        if (source.ToString() != source.GetType().ToString())
          SourceText += " (" + source.GetType().ToString() + ")";
        Trace.WriteLine("Cache source=" + SourceText);
        Trace.IndentLevel--;
      }
    }

    #endregion

    #region Свойства

    //public IDBxCacheSource Source { get { return FSource; } }
    /// <summary>
    /// Т.К. DBxCache является потокобезопасным объектом, а IDBxCacheSource - нет, нельзя делать 
    /// свойство Source общедоступным
    /// При обращении к полю выполняется блокировка объекта _Tables
    /// </summary>
    private IDBxCacheSource _Source;


    /// <summary>
    /// Если свойство не равно null, то кэш предназначен только для использования в этом потоке.
    /// Попытки вызовов методов из других потоков вызывают исключения
    /// </summary>
    public Thread SingleThread { get { return _SingleThread; } }
    private Thread _SingleThread;

    /// <summary>
    /// Генеририрует DifferentThreadException, если меторд вызван не из подходящего потока
    /// </summary>
    public void CheckThread()
    {
      if (_SingleThread != null)
      {
        if (_SingleThread != Thread.CurrentThread)
          throw new DifferentThreadException();
      }
    }

    /// <summary>
    /// Возвращает true, если текущий поток может обращаться к кэшу
    /// </summary>
    public bool IsValidThread
    {
      get
      {
        if (_SingleThread == null)
          return true;
        else
          return (_SingleThread == Thread.CurrentThread);
      }
    }

    /// <summary>
    /// Идентификатор базы данных
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private string _DBIdentity;

    /// <summary>
    /// Идентификатор базы данных (первый ключ кэширования)
    /// Нельзя использовать в качестве ключа DBIdentity, т.к. она может содержать недопустимые символы или
    /// иметь большую длину. Используем контрольную сумму MD5
    /// </summary>
    public string DBIdentityMD5 { get { return _DBIdentityMD5; } }
    private string _DBIdentityMD5;

    /// <summary>
    /// Уникальный идентификатор кэша (для отладки)
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private Guid _Guid;

    /// <summary>
    /// Количество попыток получения данных перед выбросом исключения.
    /// По умолчанию выполняется 5 попыток.
    /// Если источник данных умеет перехватывать исключения с отправкой повторных запросов серверу, может иметь смысл
    /// установить значение свойтва, равным 1.
    /// </summary>
    public int RepeatCount
    {
      get { return _RepeatCount; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _RepeatCount = value;
      }
    }
    private int _RepeatCount;

    /// <summary>
    /// Интервал времени в миллисекундах между отправками запроса, когда RepeatCount больше 1.
    /// По умолчанию - 200мс
    /// </summary>
    public int RepeatDelay
    {
      get { return _RepeatDelay; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _RepeatDelay = value;
      }
    }
    private int _RepeatDelay;

    /// <summary>
    /// Возвращает Guid
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Guid.ToString();
    }

    #endregion

    #region Доступ к таблицам

    /// <summary>
    /// Доступ к данным для отдельной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Объект DBxTableCache</returns>
    public DBxTableCache this[string tableName]
    {
      get
      {
        lock (_Tables)
        {
          DBxTableCache Res;
          if (!_Tables.TryGetValue(tableName, out Res))
          {
            DBxTableStruct TableStruct = _Source.StructSource.GetTableStruct(tableName);
            DBxTableCacheInfo CacheInfo = _Source.GetTableCacheInfo(tableName);
            if (CacheInfo == null)
              CacheInfo = new DBxTableCacheInfo(TableStruct);
            else
              CacheInfo.TableStruct = TableStruct; // восстановление после сериализации

            // 25.10.2018
            // Кроме синхронизации потоков еще нужно предохраниться от реентрабельного вызова
            if (!_Tables.ContainsKey(tableName))
            {
              Res = new DBxTableCache(this, CacheInfo);
              _Tables.Add(tableName, Res);
            }
          }

          return Res;
        }
      }
    }

    private Dictionary<string, DBxTableCache> _Tables;

    /// <summary>
    /// Доступ к данным для отдельной таблицы.
    /// В отличие от свойства "this", этот метод возвращает null, если для таблицы не был заведен кэш.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public DBxTableCache GetIfExists(string tableName)
    {
      DBxTableCache Res;
      lock (_Tables)
      {
        _Tables.TryGetValue(tableName, out Res);
      }
      return Res;
    }

    #endregion

    #region Получение данных от источника

    /*
    internal DataTable FillSelect(string TableName, DBxColumns ColumnNames, DBxFilter Where)
    {
      CheckThread();
      lock (FTables)
      {
        return FSource.FillSelect(TableName, ColumnNames, Where);
      }
    }
    */
    //internal DBxCacheTablePage[] LoadCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    //{
    //  CheckThread();
    //  lock (_Tables)
    //  {
    //    return _Source.LoadCachePages(tableName, columnNames, firstIds);
    //  }
    //}

    //internal object[] LoadCacheIndividualValues(string tableName, Int32[] ids, string columnName)
    //{
    //  CheckThread();
    //  lock (_Tables)
    //  {
    //    return _Source.LoadCacheIndividualValues(tableName, ids, columnName);
    //  }
    //}

    #endregion

    #region Очистка буферов

    /// <summary>
    /// Очищает все буферы для всех объектов DBxCache для базы данных, задаваемой DBIdenity
    /// </summary>
    public void Clear()
    {
      Cache.Clear<DBxCacheTablePage>(new string[] { DBIdentityMD5 });
      Cache.Clear<DBxCacheIndividualValue>(new string[] { DBIdentityMD5 });
    }

    /// <summary>
    /// Очищает буферы для всех объектов DBxCache для базы данных, задаваемой DBIdenity и таблицы <paramref name="tableName"/>
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void Clear(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      DBxTableCache TC = GetIfExists(tableName);
      if (TC != null)
        TC.Clear();
    }

    /// <summary>
    /// Очищает буферы для всех объектов DBxCache для базы данных, задаваемой DBIdenity, таблицы <paramref name="tableName"/>
    /// и массива идентификаторов
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="ids">Массив идентификаторов</param>
    public void Clear(string tableName, Int32[] ids)
    {
      if (ids == null)
        throw new ArgumentNullException("ids");
      if (ids.Length == 1)
      {
        Clear(tableName, ids[0]);
        return;
      }

      DBxTableCache TC = GetIfExists(tableName);
      if (TC != null)
        TC.Clear(ids);
    }

    /// <summary>
    /// Очищает буферы для всех объектов DBxCache для базы данных, задаваемой DBIdenity, таблицы <paramref name="tableName"/>
    /// и заданного идентификатора
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    public void Clear(string tableName, Int32 id)
    {
      DBxTableCache TC = GetIfExists(tableName);
      if (TC != null)
        TC.Clear(id);
    }

    /// <summary>
    /// Очищает буферы для всех объектов DBxCache для базы данных, задаваемой DBIdenity, таблицы <paramref name="tableName"/>
    /// и массива идентификаторов
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="ids">Массив идентификаторов</param>
    public void Clear(string tableName, IdList ids)
    {
      DBxTableCache TC = GetIfExists(tableName);
      if (TC != null)
        TC.Clear(ids);
    }

    /// <summary>
    /// Сброс буферизации на стороне клиента на основании данных, полученных от сервера.
    /// Клиент, время от времени, должен запрашивать у сервера объект DBxClearCacheData, и выполнять очистку
    /// </summary>
    /// <param name="clearCacheData">Данные, полученные от сервера. Может быть null</param>
    public void Clear(DBxClearCacheData clearCacheData)
    {
      if (clearCacheData == null)
        return; // не было изменений

      if (clearCacheData.AreAllTables)
        Clear(); // запрошена полная очистка
      else
      {
        foreach (KeyValuePair<string, IdList> Pair in clearCacheData)
        {
          if (Object.ReferenceEquals(Pair.Value, null))
            Clear(Pair.Key); // запрошена очистка всей таблицы
          else
          {
            // Pair.Value содержит не все идентикаторы, а только первые идентификаторы страниц
            Clear(Pair.Key, Pair.Value);
          }
        }
      }
    }

    /// <summary>
    /// Сброс буферизации на стороне клиента на основании данных, полученных от сервера.
    /// Клиент, время от времени, должен запрашивать у сервера объект DBxClearCacheData, и выполнять очистку
    /// </summary>
    /// <param name="clearCacheData">Данные, полученные от сервера. Может быть null</param>
    public void Clear(DBxClearCacheData[] clearCacheData)
    {
      if (clearCacheData == null)
        return; // не было изменений
      for (int i = 0; i < clearCacheData.Length; i++)
        Clear(clearCacheData[i]);
    }

    /// <summary>
    /// Этот метод вызывается из DBxTableCache
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnNames"></param>
    /// <param name="firstIds"></param>
    internal void InternalClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      lock (_Tables)
      {
        _Source.ClearCachePages(tableName, columnNames, firstIds);
      }
    }


    #endregion

    #region IDBxCacheSource Members

    IDBxStructSource IDBxCacheSource.StructSource
    {
      get
      {
        CheckThread();
        return _Source.StructSource;
      }
    }

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      CheckThread();
      return _Source.GetTableCacheInfo(tableName);
    }

    /// <summary>
    /// Пытается извлечь из Cache существующие страницы.
    /// Если чего-то не хватает, выполняет обращение к источнику страниц.
    /// При этом попутно догружаются страницы, для которых был вызыван DBxTableCache.AddPreloadId()
    /// </summary>
    public DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");
      CheckThread();

      #region Поиск существующих страниц и создание списка недостающих

      DBxCacheLoadResponse response = new DBxCacheLoadResponse();
      DBxCacheLoadRequest request2 = null;

      if (request.HasTablePages)
      {
        for (int i = 0; i < request.TablePages.Count; i++)
        {
          DBxCacheLoadRequest.PageInfo pi = request.TablePages[i];

          string[] SearchPageKeys = new string[4] { DBIdentityMD5, 
            pi.TableName, 
            pi.FirstId.ToString(),
            DataTools.MD5SumFromString(pi.ColumnNames.AsString)};

          DBxCacheTablePage page = Cache.GetItemIfExists<DBxCacheTablePage>(SearchPageKeys, DBxTableCache.Persistance);
          if (page == null)
          {
            if (request2 == null)
              request2 = new DBxCacheLoadRequest();
            request2.TablePages.Add(pi);
          }
          else
            response.TablePages.Add(pi.InternalKey, page);
        }
      }

      if (request.HasIndividualValues)
      {
        for (int i = 0; i < request.IndividualValues.Count; i++)
        {
          DBxCacheLoadRequest.IndividualInfo ii = request.IndividualValues[i];

          string[] SearchPageKeys = new string[4] { DBIdentityMD5,             
            ii.TableName, 
            ii.Id.ToString(),
            ii.ColumnName};

          DBxCacheIndividualValue page = Cache.GetItemIfExists<DBxCacheIndividualValue>(SearchPageKeys, DBxTableCache.Persistance);
          if (page == null)
          {
            if (request2 == null)
              request2 = new DBxCacheLoadRequest();
            request2.IndividualValues.Add(ii);
          }
          else
            response.IndividualValues.Add(ii.InternalKey, page);
        }
      }

      #endregion

      if (request2 != null)
      {
        // Требуется запрос к источнику

        #region Заодно запрашиваем страницы, помеченные как желательные

        DBxCacheLoadRequest request3 = GetPreloadRequest();
        DBxCacheLoadRequest request4 = request2.Clone();
        request4.Add(request3);

        #endregion

        #region Запрос источника

        DBxCacheLoadResponse response4 = DoLoadCachePages(request4);

        #endregion

        // Помещаем в кэш
        response4.ToCache();

        #region Добавляем в основной ответ только то, что нужно

        if (request2.HasTablePages)
        {
          for (int i = 0; i < request2.TablePages.Count; i++)
          {
            DBxCacheLoadRequest.PageInfo pi = request2.TablePages[i];
            DBxCacheTablePage page = response4.TablePages[pi.InternalKey];
            if (page == null)
              throw new NullReferenceException("Не получена страница для " + pi.InternalKey);
            response.TablePages[pi.InternalKey] = page;
          }
        }

        if (request2.HasIndividualValues)
        {
          for (int i = 0; i < request2.IndividualValues.Count; i++)
          {
            DBxCacheLoadRequest.IndividualInfo ii = request2.IndividualValues[i];
            DBxCacheIndividualValue page = response4.IndividualValues[ii.InternalKey];
            if (page == null)
              throw new NullReferenceException("Не получено индивидуальное значение для " + ii.InternalKey);
            response.IndividualValues[ii.InternalKey] = page;
          }
        }

        #endregion
      }

      return response;
    }

    private DBxCacheLoadResponse DoLoadCachePages(DBxCacheLoadRequest request)
    {
      // 08.07.2016
      // Делаем 5 попыток с интервалом 0.2 секунды, прежде чем выкинуть исключение
      // 01.09.2017
      // Количество попыток и задержку можно настраивать

      Exception e2 = null;
      for (int i = 0; i < RepeatCount; i++)
      {
        if (i > 0)
          Thread.Sleep(RepeatDelay);
        try
        {
          return _Source.LoadCachePages(request);
        }
        catch (Exception e)
        {
          // Сообщение об ошибке выдаем независимо от флага трассировки
          Trace.WriteLine(GetTracePrefix() + "Error loading cache pages for " + request.ToString());
          Trace.IndentLevel++;
          try
          {
            Trace.WriteLine("Error message  :" + e.Message);
            Trace.WriteLine("Exception class:" + e.GetType().ToString());
          }
          finally
          {
            Trace.IndentLevel--;
          }

          e.Data["DBxCache.DBIdentity"] = DBIdentity;
          e.Data["DBxCache.GUID"] = Guid;
          e.Data["DBxCache.RepeatCount"] = RepeatCount;

          if (i == 0 && RepeatCount > 1) // 17.04.2018. Выводим в log-файл, только если обработка ошибок соединений с сервером не обрабатывается в пользовательском коде
            FreeLibSet.Logging.LogoutTools.LogoutException(e, "Ошибка получения страницы DBxCache");

          e2 = e;
        }
      }
      throw new DBxCacheLoadException("Не удалось получить данные из источника", request, e2);
    }

    /// <summary>
    /// Создает запрос на страницы, помеченные на предзагрузку
    /// </summary>
    /// <returns></returns>
    private DBxCacheLoadRequest GetPreloadRequest()
    {
      DBxCacheLoadRequest r = new DBxCacheLoadRequest();
      lock (_Tables)
      {
        foreach (KeyValuePair<string, DBxTableCache> pair in _Tables)
        {
          Int32[] ids = pair.Value.GetPreloadIds();
          for (int i = 0; i < ids.Length; i++)
            r.TablePages.Add(new DBxCacheLoadRequest.PageInfo(pair.Key, pair.Value.ColumnNames, ids[i]));
        }
      }

      // Презагрузки для индивидуальных значений нет

      return r;
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      CheckThread();
      this[tableName].ClearCachePages(columnNames, firstIds);
    }

    #endregion

    #region Пользовательская загрузка страниц

    /// <summary>
    /// Получить страницы кэша для заданных идентификаторов документов.
    /// Этот метод может использоваться в архитектуре клиент-сервер для упреждающей загрузки кэша для документов,
    /// которые скоро понадобятся.
    /// Клиент определяет, какие документы будут нужны и передает TableAndIdList на сервер в составе какого-либо запроса,
    /// который все равно требуется выполнить. 
    /// Сервер вызывает метод LoadCachePages для списка идентификаторов и возвращает полученный DBxCacheLoadResponse клиенту.
    /// Клиент вызывает DBxCacheLoadResponse.ToCache().
    /// </summary>
    /// <param name="ids">Идентификаторы документов с таблицами</param>
    /// <returns>Кэшированные страницы</returns>
    public DBxCacheLoadResponse LoadCachePages(TableAndIdList ids)
    {
      if (ids == null)
        ids = TableAndIdList.Empty;

      #region Преоборазуем идентификаторы строк в идентификаторы страниц

      TableAndIdList pageIds = new TableAndIdList(); // Первые идентификаторы страниц
      foreach (string tableName in ids.GetTableNames())
      {
        foreach (Int32 id in ids[tableName])
          pageIds[tableName].Add(DBxTableCache.GetFirstPageId(id));
      }

      #endregion

      #region Создаем параметры запроса

      DBxCacheLoadRequest request = new DBxCacheLoadRequest();
      foreach (string tableName in pageIds.GetTableNames())
      {
        foreach (Int32 pageId in pageIds[tableName])
        {
          DBxCacheLoadRequest.PageInfo pi = new DBxCacheLoadRequest.PageInfo(tableName, this[tableName].ColumnNames, pageId);
          request.TablePages.Add(pi);
        }
      }

      #endregion

      return LoadCachePages(request);
    }

    #endregion

    #region Трассировка

    /// <summary>
    /// Управляет трассировкой вызовов Swap
    /// </summary>
    public static readonly BooleanSwitch TraceSwitch = new BooleanSwitch("TraceDBxCache",
      "Трассировка кэша базы данных DBxCache");

    internal string GetTracePrefix()
    {
      return DateTime.Now.ToString("G") + " DBxCache " + _Guid.ToString() + ". ";
    }

    #endregion
  }

  /// <summary>
  /// Буферизация для одной таблицы
  /// Класс является потокобезопасным
  /// </summary>
  public class DBxTableCache : IObjectWithCode
  {
    #region Константы

    /// <summary>
    /// Режим хранения страниц в кэше
    /// </summary>
    internal const CachePersistance Persistance = CachePersistance.MemoryAndTempDir;

    #endregion

    #region Защищенный конструктор

    internal DBxTableCache(DBxCache owner, DBxTableCacheInfo cacheInfo)
    {
      _Owner = owner;
      cacheInfo.SetReadOnly();
      cacheInfo.TableStruct.SetReadOnly();
      _CacheInfo = cacheInfo;

      _CacheInfo.TableStruct.CheckTablePrimaryKeyInt32();

      DBxColumnList List = new DBxColumnList(_CacheInfo.TableStruct.Columns.Count);
      _CacheInfo.TableStruct.Columns.GetColumnNames(List);
      // Убираем поля, буферизуемые в индивидуальном порядке
      for (int i = List.Count - 1; i >= 0; i--)
      {
        if (_CacheInfo.IndividualColumnFlags[i])
          List.RemoveAt(i);
      }
      _ColumnNames = new DBxColumns(List);

      byte[] b = Encoding.Unicode.GetBytes(_ColumnNames.AsString);
      _ColumnsMD5 = DataTools.MD5Sum(b);

      /*
      SearchPageKeys = new string[4];
      SearchPageKeys[0] = Owner.DBIdentityMD5;
      SearchPageKeys[1] = TableStruct.TableName;
      SearchPageKeys[2] = ColumnsMD5;
      //SearchPageKeys[3] задает номер первого идентификатора страницы
       * */
    }

    #endregion

    #region Общие свойства

    // В отличие от DBxCache.Source, эти свойства можно делать общедоступными, т.к. объекты являются потокобезопасными

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public DBxCache Owner { get { return _Owner; } }
    private DBxCache _Owner;

    /// <summary>
    /// Информация о буферизации отдельных полей таблицы. Не может быть null.
    /// </summary>
    public DBxTableCacheInfo CacheInfo { get { return _CacheInfo; } }
    private DBxTableCacheInfo _CacheInfo;

    /// <summary>
    /// Структура таблицы. Содержит все поля, включая буферизуемые отдельно
    /// </summary>
    public DBxTableStruct TableStruct { get { return _CacheInfo.TableStruct; } }

    /// <summary>
    /// Имена буферизуемых столбцов таблицы.
    /// Сюда НЕ входят столбцы, буферизуемые в индивидуальном порядке
    /// </summary>
    public DBxColumns ColumnNames { get { return _ColumnNames; } }
    private DBxColumns _ColumnNames;

    /// <summary>
    /// Имя таблицы
    /// </summary>
    public string TableName { get { return _CacheInfo.TableStruct.TableName; } }

    /// <summary>
    /// Четвертый элемент ключа для поиска
    /// </summary>
    private string _ColumnsMD5;

    #endregion

    #region Извлечение значений полей

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public object GetValue(Int32 id, string columnName)
    {
      if (id == 0)
        return null;

      if (columnName.IndexOf('.') < 0 && CacheInfo.IndividualColumnFlags[columnName])
        return GetIndividualValue(id, columnName);

      DataRow Row = GetRow(id, null); // там выполняются необходимые блокировки
      return InternalGetValue(Row, id, columnName, null);
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки.
    /// Если задан первичный набор данных <paramref name="primaryDS"/>, то проверяется наличие
    /// в нем требуемой таблицы, а в ней - строки с идентификатором <paramref name="id"/> и
    /// столбца с именем <paramref name="columnName"/>. Если данные есть в наборе, то они возвращаются
    /// вместо данных из кэша. При запросе ссылочного поля также просматривается этот набор.
    /// 
    /// Эта версия используется в объектах пользовательского интерфейса, в которых редактируются
    /// составные документы.
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="primaryDS">Необязательный первичный набор данных</param>
    /// <returns>Значение поля</returns>
    public object GetValue(Int32 id, string columnName, DataSet primaryDS)
    {
      if (id == 0)
        return null;

      DataRow Row = GetRow(id, primaryDS); // там выполняются необходимые блокировки. Вызываем даже для индивидуально буферизуемых полей
      return InternalGetValue(Row, id, columnName, primaryDS);
    }

    private object InternalGetValue(DataRow row, Int32 id, string columnName, DataSet primaryDS)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      int p = columnName.IndexOf('.');
      if (p < 0)
      {
        // Простое поле таблицы
        if (!row.Table.Columns.Contains(columnName))
        {
          if (CacheInfo.IndividualColumnFlags[columnName])
            return GetIndividualValue(id, columnName);
          throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит поля \"" + columnName + "\", или нет прав для просмотра поля", "columnName");
        }
        object v = row[columnName];
        string s = v as string;
        if (s != null)
          return s.TrimEnd(); // 29.04.2021
        else
          return v;
      }
      else
      {
        string RefColumnName = columnName.Substring(0, p);
#if DEBUG
        if (!row.Table.Columns.Contains(RefColumnName))
          throw new ArgumentException("Таблица \"" + row.Table.TableName + "\" не содержит поля \"" + RefColumnName + "\", или нет прав для просмотра поля", "columnName");
#endif
        Int32 RefId = DataTools.GetInt(row, RefColumnName);
        if (RefId == 0)
          return null; // пустая ссылка

        string ExtTableName = TableStruct.Columns[RefColumnName].MasterTableName;
        if (String.IsNullOrEmpty(ExtTableName))
          throw new ArgumentException("Поле \"" + ExtTableName + "\" таблицы \"" + TableName + "\" не является ссылочным", "columnName");

        string ExtColumnName = columnName.Substring(p + 1);

        return Owner[ExtTableName].GetValue(RefId, ExtColumnName, primaryDS);
      }
    }


    /// <summary>
    /// Получить значение для ссылочного поля 
    /// (например, значение поля "Человек.ФИО" для таблицы "Сотрудники")
    /// </summary>
    /// <param name="refColumnName">Имя поля в виде ИмяСсылочногоПоля.ИмяТребуемогоПоля (например, "Человек.ФИО")</param>
    /// <param name="refId">Значение ссылочного поля (то есть поля "Человек" в таблице "Сотрудники")</param>
    /// <returns>Значение требуемого поля</returns>
    public object GetRefValue(string refColumnName, Int32 refId)
    {
      return GetRefValue(refColumnName, refId, null);
    }

    /// <summary>
    /// Получить значение для ссылочного поля 
    /// (например, значение поля "Человек.ФИО" для таблицы "Сотрудники")
    /// </summary>
    /// <param name="refColumnName">Имя поля в виде ИмяСсылочногоПоля.ИмяТребуемогоПоля (например, "Человек.ФИО")</param>
    /// <param name="refId">Значение ссылочного поля (то есть поля "Человек" в таблице "Сотрудники")</param>
    /// <param name="primaryDS">Необязательный первичный набор данных</param>
    /// <returns>Значение требуемого поля</returns>
    public object GetRefValue(string refColumnName, Int32 refId, DataSet primaryDS)
    {
      if (refId == 0)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(refColumnName))
        throw new ArgumentNullException("refColumnName");
#endif

      int p = refColumnName.IndexOf('.');
#if DEBUG
      if (p < 0)
        throw new ArgumentException("Поле \"" + refColumnName + "\" не является ссылочным, так как не содержить точки", "refColumnName");
#endif
      string ThisColumnName = refColumnName.Substring(0, p);
      string ExtColumnName = refColumnName.Substring(p + 1);

      string ExtTableName = TableStruct.Columns[ThisColumnName].MasterTableName;
      if (String.IsNullOrEmpty(ExtTableName))
        throw new ArgumentException("Поле \"" + ExtTableName + "\" таблицы \"" + TableName + "\" не является ссылочным", "refColumnName");

      return Owner[ExtTableName].GetValue(refId, ExtColumnName, primaryDS);
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы
    /// Поля могут быть ссылочными, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, DBxColumns columnNames)
    {
      object[] a = new object[columnNames.Count];
      if (id != 0)
      {
        DataRow Row = GetRow(id, null); // там выполняются необходимые блокировки
        for (int i = 0; i < columnNames.Count; i++)
          a[i] = InternalGetValue(Row, id, columnNames[i], null);
      }
      return a;
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы
    /// Поля могут быть ссылочными, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь</param>
    /// <param name="primaryDS">Необязательный первичный набор данных</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, DBxColumns columnNames, DataSet primaryDS)
    {
      object[] a = new object[columnNames.Count];
      if (id != 0)
      {
        DataRow Row = GetRow(id, primaryDS); // там выполняются необходимые блокировки
        for (int i = 0; i < columnNames.Count; i++)
          a[i] = InternalGetValue(Row, id, columnNames[i], primaryDS);
      }
      return a;
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы
    /// Поля могут быть ссылочными, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь, разделенные запятыми</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, string columnNames)
    {
      return GetValues(id, new DBxColumns(columnNames), null);
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы
    /// Поля могут быть ссылочными, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь, разделенные запятыми</param>
    /// <param name="primaryDS">Необязательный первичный набор данных</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, string columnNames, DataSet primaryDS)
    {
      return GetValues(id, new DBxColumns(columnNames), primaryDS);
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы.
    /// Поля могут быть ссылочными, то есть содержать точки.
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, string[] columnNames)
    {
      object[] a = new object[columnNames.Length];
      if (id != 0)
      {
        DataRow Row = GetRow(id, null); // там выполняются необходимые блокировки
        for (int i = 0; i < columnNames.Length; i++)
          a[i] = InternalGetValue(Row, id, columnNames[i], null);
      }
      return a;
    }

    /// <summary>
    /// Получить значение произвольных полей строки таблицы
    /// Поля могут быть ссылочными, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnNames">Имена полей, которые требуется извлечь</param>
    /// <param name="primaryDS">Необязательный первичный набор данных</param>
    /// <returns>Значение поля</returns>
    public object[] GetValues(Int32 id, string[] columnNames, DataSet primaryDS)
    {
      object[] a = new object[columnNames.Length];
      if (id != 0)
      {
        DataRow Row = GetRow(id, primaryDS); // там выполняются необходимые блокировки
        for (int i = 0; i < columnNames.Length; i++)
          a[i] = InternalGetValue(Row, id, columnNames[i], primaryDS);
      }
      return a;
    }

#if XXX
    /// <summary>
    /// Получить список таблиц, от которых зависит поле.
    /// Если поле не является ссылочным, то в возврашаемый список входит только
    /// основная таблица. Иначе туда входят все таблицы, участвующие в запросе.
    /// Например, для GetTableNames("Сотрудники", "Организация.Руководитель.ФИОДляВыбора")
    /// возвращается {"Сотрудники", "Организации", "Люди"}
    /// Каждая таблица входит в список не более одного раза. Первой в списке является
    /// основная таблица
    /// </summary>
    /// <param name="TableName"></param>
    /// <param name="ColumnName"></param>
    /// <returns></returns>
    public string[] GetTableNames(string TableName, string ColumnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");
      if (String.IsNullOrEmpty(ColumnName))
        throw new ArgumentNullException("ColumnName");
#endif
      if (ColumnName.IndexOf('.') < 0)
        return new string[] { TableName }; // Простое поле

      // Проверяем ссылочные поля
      List<string> Tables = new List<string>();
      Tables.Add(TableName);
      while (true)
      {
        int p = ColumnName.IndexOf(".");
        if (p < 0)
          break;
        string RefColumnName = ColumnName.Substring(0, p);
        ColumnName = ColumnName.Substring(p + 1);
        Dictionary<string, string> Dict = this[TableName].GetRefColumnsAndTables();
#if DEBUG
        if (!Dict.ContainsKey(RefColumnName))
          throw new ArgumentException("Таблица \"" + TableName + "\" не имеет ссылочного поля \"" + RefColumnName + "\"", "ColumnName");
#endif
        TableName = Dict[RefColumnName];
        if (!Tables.Contains(TableName))
          Tables.Add(TableName);
      }
      return Tables.ToArray();
    }
#endif

    #endregion

    #region Форматное извлечение полей

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public string GetString(Int32 id, string columnName)
    {
      return DataTools.GetString(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public int GetInt(Int32 id, string columnName)
    {
      return DataTools.GetInt(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public long GetInt64(Int32 id, string columnName)
    {
      return DataTools.GetInt64(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public bool GetBool(Int32 id, string columnName)
    {
      return DataTools.GetBool(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public float GetSingle(Int32 id, string columnName)
    {
      return DataTools.GetSingle(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public double GetDouble(Int32 id, string columnName)
    {
      return DataTools.GetDouble(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public decimal GetDecimal(Int32 id, string columnName)
    {
      return DataTools.GetDecimal(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public DateTime? GetNullableDateTime(Int32 id, string columnName)
    {
      return DataTools.GetNullableDateTime(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public DateTime GetDateTime(Int32 id, string columnName)
    {
      return DataTools.GetDateTime(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public TimeSpan GetTimeSpan(Int32 id, string columnName)
    {
      return DataTools.GetTimeSpan(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public Guid GetGuid(Int32 id, string columnName)
    {
      return DataTools.GetGuid(GetValue(id, columnName, null));
    }

    /// <summary>
    /// Получить значение произвольного поля таблицы
    /// Поле может быть ссылочным, то есть содержать точки
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="id">Значение идентификатора Id</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public T GetEnum<T>(Int32 id, string columnName)
      where T : struct
    {
      return DataTools.GetEnum<T>(GetValue(id, columnName, null));
    }

    #endregion

    #region Получение таблицы данных

    /// <summary>
    /// Создает таблицу данных DataTable с заданными именами полей и строками для заданных идентификаторов.
    /// Полученная таблица может использоваться произвольным образом, т.к. она не связана с системой буферизации.
    /// При повторных обращениях создается новая таблица
    /// </summary>
    /// <param name="ids">Идентификаторы записей в таблице, для которых создаются строки. 
    /// Может быть пустой массив, тогда возвращается заготовка таблицы</param>
    /// <param name="columnNames">Имена полей. Имена полей могут содержать точки. Если null, то загружаются все поля текущей таблицы, без ссылочных полей</param>
    /// <returns>Заполненная таблица данных</returns>
    public DataTable CreateTable(Int32[] ids, DBxColumns columnNames)
    {
      // Структура основной таблицы
      if (columnNames == null)
        columnNames = new DBxColumns(TableStruct.Columns.GetCodes());

      #region Формируем структуру создаваемой таблицы

      DataTable Table = new DataTable();
      for (int i = 0; i < columnNames.Count; i++)
      {
        DBxColumnStruct ColStr = GetColumnStruct(columnNames[i]); // рекурсивная процедура
        if (ColStr == null)
          throw new ArgumentException("Неизвестное имя столбца \"" + columnNames[i] + "\"", "columnNames");
        DataColumn Col = ColStr.CreateDataColumn(columnNames[i]);
        Col.AllowDBNull = true; // для ссылочных полей может быть не задана ссылка
        Table.Columns.Add(Col);
      }
      // Структура определена

      #endregion

      #region Цикл по строкам

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          throw new ArgumentException("Не задан идентификатор строки в массиве в позиции " + i.ToString(), "ids");

        DataRow SrcRow = GetRow(ids[i], null);
        DataRow NewRow = Table.NewRow();
        for (int j = 0; j < columnNames.Count; j++)
          NewRow[j] = InternalGetValue(SrcRow, ids[i], columnNames[j], null);
        Table.Rows.Add(NewRow);
      }

      // Table.RemotingFormat = SerializationFormat.Binary; // 16.03.2018
      Table.RemotingFormat = SerializationTools.GetPreferredRemotingFormat(Table); // 19.01.2021
      Table.AcceptChanges();

      #endregion

      return Table;
    }

    /// <summary>
    /// Получить описание столбца по имени.
    /// Имя может содержать точки для задания ссылочных столбцов.
    /// Если задано несуществующее имя столбца, возвращает null.
    /// Для простых имен столбца без точек возвращает TableStruct.Columns[ColumnName].
    /// 
    /// Предупреждение. Имя возвращаемого столбца DBxColumnStruct.ColumnName может не совпадать с <paramref name="columnName"/>,
    /// если столбец является ссылочным. Если далее вызывается DBxColumnStruct.CreateDataColumn(), то обычно следует также
    /// установить свойство DataColumn.ColumnName.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описатель структуры столбца. Может относиться к другой таблице, если поле является ссылочным</returns>
    public DBxColumnStruct GetColumnStruct(string columnName)
    {
      int p = columnName.IndexOf('.');
      if (p >= 0)
      {
        string ColumnName1 = columnName.Substring(0, p);
        string ColumnName2 = columnName.Substring(p + 1);
        DBxColumnStruct ColStr1 = TableStruct.Columns[ColumnName1];
        if (ColStr1 == null)
          return null;
        if (String.IsNullOrEmpty(ColStr1.MasterTableName))
          return null;
        return Owner[ColStr1.MasterTableName].GetColumnStruct(ColumnName2);
      }
      else
        return TableStruct.Columns[columnName];
    }

    #endregion

    #region Доступ к строке данных

    /*
     * 04.08.2015
     * Эта оптимизация запрещена
     * Можно было бы выкрутиться, и при вызове Cacje.Clear<DataTable>() вызывать сбрасывать и строку
     * НО, на сервере могут быть отдельные буферы для каждого пользователя и они сбрасываются одновременно
     * для всех пользователей. Другие пользователи не узнают о сбросе и останутся со старой строкой

    /// <summary>
    /// Если идет подряд несколько запросов к одной строке, то используем еще один уровень буферизации
    /// Для доступа к полям используем кратковременную блокировку объекта FColumnNames
    /// </summary>
    private Int32 LastAccessRowId;
    private DataRow LastAccessRow;

     * */


    // 08.07.2016
    // Это тоже запрещено, так как нарушает потокобезопасность.
    // Можно было бы блокировать на время использования, но lock будет выполняться дольше, чем создание массива

    ///// <summary>
    ///// Ключи для поиска страницы.
    ///// [0] - DBIdentity
    ///// [1] - Имя таблицы
    ///// [2] - Ключ по списку полей
    ///// [3] - Первый идентификатор диапазона ("1", "101", "201", ...)
    ///// В конструкторе устанавливаются элементы [0], [1] и [2]
    ///// В GetRow устанавливается только [3]
    ///// </summary>
    //private string[] SearchPageKeys;

    private DataRow GetRow(Int32 id, DataSet primaryDS)
    {
#if DEBUG
      if (id == 0)
        throw new BugException("Id=0");
#endif

      if (primaryDS != null)
      {
        if (primaryDS.Tables.Contains(TableName))
        {
          DataTable PrimaryTable = primaryDS.Tables[TableName];
          DataRow PrimaryRow = PrimaryTable.Rows.Find(id);
          if (PrimaryRow != null)
            return PrimaryRow;
        }
      }

      /*
      // Попытка доступа к строке
      lock (FColumnNames)
      {
        if (Id == LastAccessRowId)
          return LastAccessRow;
      } */

      // Получаем страницу
      Int32 FirstPageId = GetFirstPageId(id);


      DBxCacheTablePage Page = DoGetPage(FirstPageId);

      DataRow Row = Page.Table.Rows.Find(id);
      if (Row == null)
      {
        // Возможно, база данных содержит новые строки, которые не были загружены
        // Сбрасываем буфер данной страницы  и повторяем попытку
        ClearCachePages(ColumnNames, new Int32[1] { FirstPageId });

        Page = DoGetPage(FirstPageId);

        Row = Page.Table.Rows.Find(id);
        if (Row == null)
          throw new ArgumentException("Таблица \"" + TableName + "\" не содержит строки с идентификатором Id=" + id.ToString());
      }

      /*
      // Запоминаем строку для повторного использования
      lock (FColumnNames)
      {
        LastAccessRowId = Id;
        LastAccessRow = Row;
      }  */

      return Row;
    }

    /// <summary>
    /// Возвращает первый идентификатор страницы
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static Int32 GetFirstPageId(Int32 id)
    {
      if (id <= 0)
        throw new ArgumentException("Неправильный идентификатор: " + id.ToString() + ". Иденификатор должен быть больше 0", "id");
      return ((id - 1) / DBxCache.PageRowCount) * DBxCache.PageRowCount + 1;
    }

    #endregion

    #region Упреждающая загрузка страниц

    /// <summary>
    /// Идентификаторы страниц, которые стоит загрузить.
    /// На время обращения выполняется блокировка объекта _SyncRoot.
    /// Объект не создается до первого обращения, т.к. для большинства
    /// таблиц предзагрузка не выполняется
    /// </summary>
    private IdList _PreloadPageIds;

    private static readonly object _SyncRoot = new object();

    /// <summary>
    /// Добавляет указанные идентификаторы строк в список для предзагрузки страниц.
    /// Этот метод не выполяет реальную загрузку, а всего лишь учитывает пожелание
    /// </summary>
    /// <param name="ids">Идентификаторы записей для загрузки</param>
    public void AddPreloadIds(Int32[] ids)
    {
      if (ids == null)
        return;
      if (ids.Length == 0)
        return;

      // Сначала собираем идентификаторы страниц, чтобы не держать блокировку
      IdList pageIds = new IdList();
      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] != 0)
          pageIds.Add(GetFirstPageId(ids[i]));
      }

      lock (_SyncRoot)
      {
        if (_PreloadPageIds == null)
          _PreloadPageIds = new IdList();
        _PreloadPageIds.Add(pageIds);
      }
    }


    /// <summary>
    /// Добавляет указанные идентификаторы строк в список для предзагрузки страниц.
    /// Этот метод не выполяет реальную загрузку, а всего лишь учитывает пожелание
    /// </summary>
    /// <param name="ids">Идентификаторы записей для загрузки</param>
    public void AddPreloadIds(IdList ids)
    {
      if (ids == null)
        return;
      if (ids.Count == 0)
        return;

      // Сначала собираем идентификаторы страниц, чтобы не держать блокировку
      IdList pageIds = new IdList();
      foreach (Int32 id in ids)
        pageIds.Add(GetFirstPageId(id));

      lock (_SyncRoot)
      {
        if (_PreloadPageIds == null)
          _PreloadPageIds = new IdList();
        _PreloadPageIds.Add(pageIds);
      }
    }

    /// <summary>
    /// Добавляет указанный идентификатор строки в список для предзагрузки страниц.
    /// Этот метод не выполяет реальную загрузку, а всего лишь учитывает пожелание.
    /// Используйте AddPreloadIds(), если есть несколько идентификаторов
    /// </summary>
    /// <param name="id">Идентификатор записей для загрузки</param>
    public void AddPreloadId(Int32 id)
    {
      if (id == 0)
        return;

      lock (_SyncRoot)
      {
        if (_PreloadPageIds == null)
          _PreloadPageIds = new IdList();
        _PreloadPageIds.Add(GetFirstPageId(id));
      }
    }

    /// <summary>
    /// Возвращает список идентификаторов страниц для предварительной загрузки.
    /// При этом текущий список очищается
    /// </summary>
    /// <returns></returns>
    internal Int32[] GetPreloadIds()
    {
      Int32[] ids;
      lock (_SyncRoot)
      {
        if (_PreloadPageIds == null)
          ids = DataTools.EmptyIds;
        else
        {
          ids = _PreloadPageIds.ToArray();
          _PreloadPageIds = null;
        }
      }
      return ids;
    }

    #endregion

    /*
     * В качестве единицы буферизации данных используется класс, содержащий таблицу DataTable.
     * Таблица содержит набор из (не более чем) 100 строк (константа DBxCache) с последовательными идентикаторами (1-100, 101-200, ...)
     * В таблице присутствуют все доступные поля. "Точечные" поля отсутствуют. Для получения значений ссылочных
     * полей, используется другой объект DBxTableCache, относящийся к master-таблицы, откуда и извлекается
     * значение
     * Таблица имеет первичный ключ по полю идентификатора
     */

    /*
     * 15.08.2016
     * Буферизация значений отдельных полей.
     * Буферизация строк таблиц целиком имеет недостаток.
     * Таблица может содержать MEMO- или BLOB-поля, содержащие большой объем данных. Как правило,
     * они редко требуются при использовании буферизации, но иногда все-же к ним может быть обращение.
     * Для ускорения работы и уменьшения трафика их выгодно загружать отдельно от основной таблицы.
     * 
     * Чтобы отличить такие поля, в интерфейсе IDBCacheSource предусмотрен новый метод GetTableCacheInfo().
     * Метод позволяет для каждого поля установить флажок, если данное поле не должно буферизоваться.
     * По умолчанию, не буферизуются все поля типа Memo, Xml и Binary, но это может быть переопределено в
     * пользовательском коде.
     * Во избежание передачи лишних данных между клиентом и сервером, используется следующая реализация
     * IDBCacheSource.GetTableCacheInfo(). 
     * - При вызове клиентом этого метода, на стороне сервера создается объект DBxTableCacheInfo. 
     * В конструкторе ему передается DBxTableStruct.
     * - На стороне сервера устанавливаются значения полей.
     * - Заполненный DBxTableCacheInfo возвращается клиенту, но поле TableStruct не сериализуется.
     * - Клиент восстанавливает DBxTableCacheInfo передавая ему ссылку на собственную копию DBxTableStruct.
     * Серверная реализация IDBCacheSource.GetTableCacheInfo() может просто вернуть null, если используется
     * стандартная реализация буферизации
     * 
     * Основной класс хранения страницы данных (PageTable) не хранит "нежелательные" поля. 
     * Они хранятся как отдельные объекты SingleValue. Каждое значение буферизуется как отдельный объект
     */

    #region ICacheFactory<DBxCacheTablePage> Members

    private DBxCacheTablePage DoGetPage(Int32 firstId)
    {
#if DEBUG
      if ((firstId % DBxCache.PageRowCount) != 1)
        throw new ArgumentException("Неправильный firstId=" + firstId.ToString());
#endif

      string[] SearchPageKeys = new string[4] { _Owner.DBIdentityMD5, TableName, firstId.ToString(), _ColumnsMD5 };
      DBxCacheTablePage page = Cache.GetItemIfExists<DBxCacheTablePage>(SearchPageKeys, DBxTableCache.Persistance);

      if (page == null)
      {
        // Делаем запрос
        DBxCacheLoadRequest.PageInfo pi = new DBxCacheLoadRequest.PageInfo(TableName, ColumnNames, firstId);
        DBxCacheLoadRequest request = new DBxCacheLoadRequest();
        request.TablePages.Add(pi);
        DBxCacheLoadResponse response = _Owner.LoadCachePages(request);
        page = response.TablePages[pi.InternalKey];
        if (page == null)
          throw new NullReferenceException("Не получена страница для " + pi.InternalKey);
      }

      return page;
    }

    #endregion

    #region Буферизация отдельных значений

    private object GetIndividualValue(Int32 id, string columnName)
    {
      string[] SearchIndValKeys = new string[4] { Owner.DBIdentityMD5, TableName, id.ToString(), columnName };

      DBxCacheIndividualValue page = Cache.GetItemIfExists<DBxCacheIndividualValue>(SearchIndValKeys, DBxTableCache.Persistance);
      if (page == null)
      {
        // Требуется запрос
        DBxCacheLoadRequest.IndividualInfo ii = new DBxCacheLoadRequest.IndividualInfo(TableName, id, columnName);
        DBxCacheLoadRequest request = new DBxCacheLoadRequest();
        request.IndividualValues.Add(ii);

        DBxCacheLoadResponse response = _Owner.LoadCachePages(request);
        page = response.IndividualValues[ii.InternalKey];
        if (page == null)
          throw new NullReferenceException("Не получена страница индивидуального значения для " + ii.InternalKey);
      }
      return page.Value;
    }

    #endregion

    #region Очистка буфера

    /// <summary>
    /// Сбрасывает существующие буферы для этой таблицы
    /// </summary>
    public void Clear()
    {
      Cache.Clear<DBxCacheTablePage>(new string[] { Owner.DBIdentityMD5, TableName });
      Cache.Clear<DBxCacheIndividualValue>(new string[] { Owner.DBIdentityMD5, TableName });
    }

    /// <summary>
    /// Сбрасывает сущействующие буферы для выбранных идентификаторов
    /// </summary>
    /// <param name="ids">Массив идентификаторов. Значения 0 пропускаются</param>
    public void Clear(Int32[] ids)
    {
      if (ids.Length == 0)
        return;
      if (ids.Length == 1)
      {
        Clear(ids[0]);
        return;
      }

      // Многократно вызывать Cache.Clear() невыгодно
      IdList FirstPageIds = new IdList();
      for (int i = 0; i < ids.Length; i++)
      {
        FirstPageIds.Add(GetFirstPageId(ids[i]));
        Cache.Clear<DBxCacheIndividualValue>(new string[] { Owner.DBIdentityMD5, TableName, ids[i].ToString() });
      }

      foreach (Int32 FirstPageId in FirstPageIds)
        Cache.Clear<DBxCacheTablePage>(new string[] { Owner.DBIdentityMD5, TableName, FirstPageId.ToString() });
    }

    /// <summary>
    /// Сбрасывает существующие буферы для одного идентификатора.
    /// </summary>
    /// <param name="id">Идентификатор строки таблицы. Если 0, то никаких действий не выполняется</param>
    public void Clear(Int32 id)
    {
      if (id == 0)
        return;

      Int32 FirstPageId = GetFirstPageId(id);
      Cache.Clear<DBxCacheTablePage>(new string[] { Owner.DBIdentityMD5, TableName, FirstPageId.ToString() });
      Cache.Clear<DBxCacheIndividualValue>(new string[] { Owner.DBIdentityMD5, TableName, id.ToString() });
    }

    /// <summary>
    /// Сбрасывает сушествующие буферы для списка идентификаторов
    /// </summary>
    /// <param name="ids">Список идентификаторов</param>
    public void Clear(IdList ids)
    {
      if (ids.Count == 0)
        return;

      // Многократно вызывать Cache.Clear() невыгодно
      IdList FirstPageIds = new IdList();
      foreach (Int32 Id in ids)
      {
        FirstPageIds.Add(GetFirstPageId(Id));
        Cache.Clear<DBxCacheIndividualValue>(new string[] { Owner.DBIdentityMD5, TableName, Id.ToString() });
      }

      foreach (Int32 FirstPageId in FirstPageIds)
        Cache.Clear<DBxCacheTablePage>(new string[] { Owner.DBIdentityMD5, TableName, FirstPageId.ToString() });
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return TableStruct.TableName; }
    }

    #endregion

    internal DBxCacheTablePage GetCachePage(DBxColumns columnNames, Int32 firstId)
    {
      if ((firstId % DBxCache.PageRowCount) != 1)
        throw new ArgumentException("Идентификатор должен быть на границе страницы", "firstId");

      DBxCacheTablePage Page = DoGetPage(firstId);
      DBxColumns OrgColumnNames = DBxColumns.FromColumns(Page.Table.Columns);
      if (String.Equals(columnNames.AsString, OrgColumnNames.AsString, StringComparison.Ordinal))
        // Список полей совпадает
        return Page;
      else
      {
        // Создаем таблицу с меньшим числом полей
        DataTable Table2 = columnNames.CreateSubTable(Page.Table);
        return new DBxCacheTablePage(
          Page.DBIdentityMD5,
          Page.TableName,
          Page.FirstId,
          DataTools.MD5SumFromString(columnNames.AsString),
          Table2, Page.PrimaryKeyColumn);
      }
    }


    internal void ClearCachePages(DBxColumns columnNames, Int32[] firstIds)
    {
      // 1. Очищаем собственный кэш
      for (int i = 0; i < firstIds.Length; i++)
      {
        string[] SearchPageKeys = new string[] { Owner.DBIdentityMD5, TableName, firstIds[i].ToString() };
        Cache.Clear<DBxCacheTablePage>(SearchPageKeys);
      }

      // 2. Сбрасываем кэш владельца
      Owner.InternalClearCachePages(TableName, columnNames, firstIds);
    }

    #region Обновление

    /// <summary>
    /// Обновление значений в страницах кэша, если они загружены.
    /// Данные берутся из передаваемых строк данных <paramref name="rows"/>.
    /// Если какие-либо страницы кэша не загружены, для этих строк никаких действий не выполняется.
    /// Передаваемые строки должны относиться к одной таблице или к таблицам с одинаковой структурой.
    /// В таблице должно быть поле "Id". Остальные поля являются произвольными. Берутся значения только
    /// тех полей, которые есть в таблице кэша. 
    /// </summary>
    /// <param name="rows">Массив строк для обновления. Не может содержать значения null</param>
    public void UpdateRows(DataRow[] rows)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      if (rows.Length == 0)
        return;

      int pId = rows[0].Table.Columns.IndexOf("Id");
      if (pId < 0)
        throw new ArgumentException("Таблица " + rows[0].Table.TableName + " не содержит поля \"Id\"", "rows");

      #region Постраничная обработка

      #region Разбиение на страницы

      Dictionary<Int32, List<DataRow>> RowGroups = new Dictionary<int, List<DataRow>>();
      for (int iRow = 0; iRow < rows.Length; iRow++)
      {
        Int32 Id = (Int32)(rows[iRow][pId]);
        Int32 FirstId = GetFirstPageId(Id);
        List<DataRow> RowList;
        if (!RowGroups.TryGetValue(FirstId, out RowList))
        {
          RowList = new List<DataRow>();
          RowGroups.Add(FirstId, RowList);
        }
        RowList.Add(rows[iRow]);
      }

      #endregion

      // Словарь соответствий полей, которые требуется копировать.
      // Ключ - индекс поля в исходных строках
      // Значение - индекс поля в таблице кэша
      Dictionary<int, int> copyColumnIndices = null;

      #region Обновление страниц

      foreach (KeyValuePair<Int32, List<DataRow>> PagePair in RowGroups)
      {
        string[] SearchPageKeys = new string[4] { Owner.DBIdentityMD5, TableName, PagePair.Key.ToString(), _ColumnsMD5 };

        DBxCacheTablePage Page = Cache.GetItemIfExists<DBxCacheTablePage>(SearchPageKeys, Persistance);
        if (Page == null)
          continue;

        bool PageModified = false;

        for (int iRow = 0; iRow < PagePair.Value.Count; iRow++)
        {
          DataRow SrcRow = PagePair.Value[iRow];
          Int32 Id = (Int32)(SrcRow[pId]);
          DataRow CacheRow = Page.Table.Rows.Find(Id);
          if (CacheRow == null)
            continue; // может и не быть еще на странице кэша

          #region Создание словаря замены полей при первом обращении

          if (copyColumnIndices == null)
          {
            copyColumnIndices = new Dictionary<int, int>();
            for (int iCol = 0; iCol < ColumnNames.Count; iCol++)
            {
              int pSrc = SrcRow.Table.Columns.IndexOf(ColumnNames[iCol]);
              int pCache = Page.Table.Columns.IndexOf(ColumnNames[iCol]);
              if (pSrc < 0)
                continue;
#if DEBUG
              if (pCache < 0)
                throw new BugException("Не найден индекс поля \"" + ColumnNames[iCol] + "\" в таблице кэша \"" + TableName + "\"");
#endif
              if (pSrc == pId)
                continue; // идентификатор нельзя перезаписывать

              copyColumnIndices.Add(pSrc, pCache);
            }
          }

          #endregion

          if (copyColumnIndices.Count > 0)
          {
            PageModified = true;
            foreach (KeyValuePair<int, int> ColPair in copyColumnIndices)
              CacheRow[ColPair.Value] = SrcRow[ColPair.Key];
          }
        } // цикл по строкам в пределах страницы

        if (PageModified)
        {
          Page.Table.AcceptChanges();
          Cache.SetItem<DBxCacheTablePage>(SearchPageKeys, Persistance, Page);
        }

      } // цикл по страницам

      #endregion

      #endregion

      #region Обработка индивидуальных полей

      for (int iCol = 0; iCol < rows[0].Table.Columns.Count; iCol++)
      {
        string ColumnName = rows[0].Table.Columns[iCol].ColumnName;
        if (CacheInfo.IndividualColumnFlags[ColumnName])
        {
          for (int iRow = 0; iRow < rows.Length; iRow++)
          {
            Int32 Id = (Int32)(rows[iRow][pId]);
            string[] SearchIndValKeys = new string[4] { Owner.DBIdentityMD5, TableName, Id.ToString(), ColumnName };

            if (Cache.GetItemIfExists<DBxCacheIndividualValue>(SearchIndValKeys, Persistance) != null)
            {
              object v = rows[iRow][iCol];
              if (v is DBNull)
                v = null;
              else if (v is string)
                v = ((string)v).TrimEnd();

              Cache.SetItem<DBxCacheIndividualValue>(SearchIndValKeys, Persistance, new DBxCacheIndividualValue(
                Owner.DBIdentityMD5, TableName, Id, ColumnName,
                v));
            }
          }
        }
      }

      #endregion
    }

    #endregion
  }


  /// <summary>
  /// Сериализуемая страница табличных данных.
  /// Объекты этого класса сохраняются в кэше. Ключами страницы являются:
  /// [0] DBIdentityMD5
  /// [1] TableName
  /// [2] FirstPageId
  /// [3] ColumnsMD5
  /// </summary>
  [Serializable]
  internal sealed class DBxCacheTablePage
  {
    /*
     * 13.06.2017
     * Есть какая-то проблема в сериализации DataTable.
     * Вероятно, это происходит, если сериализация выполняется одновременно в нескольких потоках.
     * Возникает исключение:
     ExceptionClass            = System.NullReferenceException
Message                   = В экземпляре объекта не задана ссылка на объект.
Properties: 
  Data                      = System.Collections.ListDictionaryInternal
    [ChainDocProvider]        = "FreeLibSet.ExtDB.Docs.DBxChainDocProvider"
    Count                     = 1
    IsReadOnly                = False
  HelpLink                  = null
  Source                    = "System.Data"

Stack trace:

Server stack trace: 
   в System.Data.DataTable.SerializeDataTable(SerializationInfo info, StreamingContext context, Boolean isSingleTable, SerializationFormat remotingFormat)
   в System.Data.DataTable.GetObjectData(SerializationInfo info, StreamingContext context)
   в System.Runtime.Serialization.Formatters.Binary.WriteObjectInfo.InitSerialize(Object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter)
   в System.Runtime.Serialization.Formatters.Binary.WriteObjectInfo.Serialize(Object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter)
   в System.Runtime.Serialization.Formatters.Binary.ObjectWriter.Serialize(Object graph, Header[] inHeaders, __BinaryWriter serWriter, Boolean fCheck)
   в System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.Serialize(Stream serializationStream, Object graph, Header[] headers, Boolean fCheck)
   в System.Runtime.Remoting.Channels.CoreChannel.SerializeBinaryMessage(IMessage msg, Stream outputStream, Boolean includeVersions)
   в System.Runtime.Remoting.Channels.BinaryServerFormatterSink.SerializeResponse(IServerResponseChannelSinkStack sinkStack, IMessage msg, ITransportHeaders& headers, Stream& stream)
   в System.Runtime.Remoting.Channels.BinaryServerFormatterSink.ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, IMessage& responseMsg, ITransportHeaders& responseHeaders, Stream& responseStream)

Exception rethrown at [0]: 
   в System.Runtime.Remoting.Proxies.RealProxy.HandleReturnMessage(IMessage reqMsg, IMessage retMsg)
   в System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData& msgData, Int32 type)
   в FreeLibSet.ExtDB.Docs.DBxDocProvider.LoadCachePage(String TableName, DBxColumns ColumnNames, Int32 FirstId, Int32 LastId)
   в FreeLibSet.ExtDB.Docs.DBxChainDocProvider.LoadCachePage(String TableName, DBxColumns ColumnNames, Int32 FirstId, Int32 LastId)
   в FreeLibSet.ExtDB.DBxCache.LoadCachePage(String TableName, DBxColumns ColumnNames, Int32 FirstId, Int32 LastId)
   в FreeLibSet.ExtDB.DBxTableCache.FreeLibSet.Caching.ICacheFactory<FreeLibSet.ExtDB.DBxTableCache.PageTable>.CreateCacheItem(String[] Keys)
     * 
     * Для исправления ошибки, в классе DBxCacheTablePage храним и "полезный" объект DataTable и
     * сериализуемый байтовый массив. Объект DataTable не сериализуем, а восстанавливаем после
     * десериализации.
     */

    #region Конструктор

    /// <summary>
    /// Создает новый объект
    /// </summary>
    /// <param name="dbIdentityMD5">Первый ключ кэширования</param>
    /// <param name="tableName">Второй ключ кэширования</param>
    /// <param name="firstId">Третий ключ кэширования</param>
    /// <param name="columnsMD5">Четвертый ключ кэширования</param>
    /// <param name="table">Таблица страницы данных. Не может быть null</param>
    /// <param name="primaryKeyColumn">Имя столбца первичного ключа. Поле должно быть задано. Составные первичные ключи не допускаются</param>
    public DBxCacheTablePage(string dbIdentityMD5, string tableName, Int32 firstId, string columnsMD5,
      DataTable table, string primaryKeyColumn)
    {
      _DBIdentityMD5 = dbIdentityMD5;
      _TableName = tableName;
      _FirstId = firstId;
      _ColumnsMD5 = columnsMD5;

#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(primaryKeyColumn))
        throw new ArgumentNullException("primaryKeyColumn");
      if (primaryKeyColumn.IndexOf(',') >= 0)
        throw new ArgumentException("Первичный ключ не может быть составным");
#endif

      //table.RemotingFormat = SerializationFormat.Binary;
      table.RemotingFormat = SerializationTools.GetPreferredRemotingFormat(table); // 19.01.2021
      table.AcceptChanges();
      DataTools.SetPrimaryKey(table, primaryKeyColumn);

      _Table = table;
      _PrimaryKeyColumn = primaryKeyColumn;

      // Создаем байтовый массив для сериализации
      using (MemoryStream ms = new MemoryStream())
      {
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, table); // в крайнем случае, ошибка появится здесь
        TableBytes = ms.ToArray();
      }
    }

    #endregion

    #region Свойства ключей для Cache

    /// <summary>
    /// Идентификатор базы данных - сумма MD5 (первый ключ кэширования)
    /// </summary>
    public string DBIdentityMD5 { get { return _DBIdentityMD5; } }
    private string _DBIdentityMD5;

    /// <summary>
    /// Имя таблицы (второй ключ кэширования)
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Первый идентификатор для страницы (третий ключ кэширования)
    /// </summary>
    public Int32 FirstId { get { return _FirstId; } }
    private Int32 _FirstId;

    /// <summary>
    /// Имена столбцов - сумма MD5 (четвертый ключ кэширования)
    /// </summary>
    public string ColumnsMD5 { get { return _ColumnsMD5; } }
    private string _ColumnsMD5;

    #endregion

    #region Свойства таблицы данных

    /// <summary>
    /// Сериализуемое представление таблицы
    /// </summary>
    private byte[] TableBytes;

    /// <summary>
    /// Таблица данных
    /// </summary>
    public DataTable Table { get { return _Table; } }
    [NonSerialized]
    private DataTable _Table;

    /// <summary>
    /// Имя поля первичного ключа.
    /// После десериализации первичный ключ восстанавливается
    /// </summary>
    public string PrimaryKeyColumn { get { return _PrimaryKeyColumn; } }
    private string _PrimaryKeyColumn;

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      if (TableBytes == null)
        throw new NullReferenceException("Поле TableBytes не было десериализовано");

      using (MemoryStream ms = new MemoryStream(TableBytes))
      {
        BinaryFormatter bf = new BinaryFormatter();
        _Table = (DataTable)(bf.Deserialize(ms));
      }

      DataTools.SetPrimaryKey(_Table, PrimaryKeyColumn);
    }

    #endregion
  }

  /// <summary>
  /// Хранилище индивидуального значения поля.
  /// Отдельный класс требуется для использования в системе Cache. Ключами кэша являются:
  /// [0] - DBIdentityMD5
  /// [1] - TableName
  /// [2] - Id
  /// [3] - ColumnName
  /// </summary>
  [Serializable]
  internal class DBxCacheIndividualValue
  {
    #region Конструктор

    internal DBxCacheIndividualValue(
      string dbIdentityMD5,
      string tableName,
      Int32 id,
      string columnName,
      object value)
    {
      _DBIdentityMD5 = dbIdentityMD5;
      _TableName = tableName;
      _Id = id;
      _ColumnName = columnName;

      string s = value as string;
      if (s == null)
        _Value = value;
      else
        _Value = s.TrimEnd(); // 29.04.2021
    }

    #endregion

    #region Свойства ключей для Cache

    /// <summary>
    /// Идентификатор базы данных - сумма MD5 (первый ключ кэширования)
    /// </summary>
    public string DBIdentityMD5 { get { return _DBIdentityMD5; } }
    private string _DBIdentityMD5;

    /// <summary>
    /// Имя таблицы (второй ключ кэширования)
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Идентификатор строки (третий ключ кэширования)
    /// </summary>
    public Int32 Id { get { return _Id; } }
    private Int32 _Id;

    /// <summary>
    /// Имя столбца (четвертый ключ кэширования)
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    #endregion

    #region Свойство - Значение

    /// <summary>
    /// Значение поля
    /// </summary>
    public object Value { get { return _Value; } }
    private object _Value;

    #endregion
  }

  /// <summary>
  /// Запрос на загрузку страниц кэша.
  /// Этот объект создается в DBxCache
  /// </summary>
  [Serializable]
  public sealed class DBxCacheLoadRequest
  {
    #region Конструктор

    internal DBxCacheLoadRequest()
    {
      //_TablePages = new List<PageInfo>();
      //_Individuals = new List<IndividualInfo>();
    }

    #endregion

    #region Запросы страниц

    [Serializable]
    internal class PageInfo
    {
      #region Конструктор

      public PageInfo(string tableName, DBxColumns columnNames, Int32 firstId)
      {
#if DEBUG
        if (String.IsNullOrEmpty(tableName))
          throw new ArgumentNullException("tableName");
        if (columnNames == null)
          throw new ArgumentNullException("columnNames");
        if (firstId <= 0)
          throw new ArgumentOutOfRangeException("firstId");
#endif
        _TableName = tableName;
        _ColumnNames = columnNames;
        _FirstId = firstId;
      }

      #endregion

      #region Свойства

      public string TableName { get { return _TableName; } }
      private string _TableName;

      public DBxColumns ColumnNames { get { return _ColumnNames; } }
      private DBxColumns _ColumnNames;

      public Int32 FirstId { get { return _FirstId; } }
      private Int32 _FirstId;

      public string InternalKey { get { return _TableName + "|" + _ColumnNames.AsString + "|" + _FirstId.ToString(); } }

      #endregion

      #region Текстовое предсталение

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        ToString(sb);
        return sb.ToString();
      }

      internal void ToString(StringBuilder sb)
      {
        sb.Append('\"');
        sb.Append(TableName);
        sb.Append("\", FirstId=");
        sb.Append(FirstId.ToString());
      }

      #endregion
    }

    internal IList<PageInfo> TablePages
    {
      get
      {
        if (_TablePages == null)
          _TablePages = new List<PageInfo>();
        return _TablePages;
      }
    }
    private List<PageInfo> _TablePages;

    internal bool HasTablePages
    {
      get
      {
        if (_TablePages == null)
          return false;
        else
          return _TablePages.Count > 0;
      }
    }

    #endregion

    #region Запросы индивидуальных значений

    [Serializable]
    internal class IndividualInfo
    {
      #region Конструктор

      public IndividualInfo(string tableName, Int32 id, string columnName)
      {
#if DEBUG
        if (String.IsNullOrEmpty(tableName))
          throw new ArgumentNullException("tableName");
        if (id <= 0)
          throw new ArgumentOutOfRangeException("id");
        if (String.IsNullOrEmpty(columnName))
          throw new ArgumentNullException("columnName");
#endif
        _TableName = tableName;
        _Id = id;
        _ColumnName = columnName;
      }

      #endregion

      #region Свойства

      public string TableName { get { return _TableName; } }
      private string _TableName;

      public Int32 Id { get { return _Id; } }
      private Int32 _Id;

      public string ColumnName { get { return _ColumnName; } }
      private string _ColumnName;

      public string InternalKey
      {
        get
        {
          return _TableName + "|" + _Id.ToString() + "|" + _ColumnName;
        }
      }

      #endregion

      #region Текстовое предсталение

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        ToString(sb);
        return sb.ToString();
      }

      internal void ToString(StringBuilder sb)
      {
        sb.Append(TableName);
        sb.Append(", Id=");
        sb.Append(Id.ToString());
        sb.Append(", ColumnName=");
        sb.Append(ColumnName);
      }

      #endregion
    }

    internal IList<IndividualInfo> IndividualValues
    {
      get
      {
        if (_IndividualValues == null)
          _IndividualValues = new List<IndividualInfo>();
        return _IndividualValues;
      }
    }
    private List<IndividualInfo> _IndividualValues;

    internal bool HasIndividualValues
    {
      get
      {
        if (_IndividualValues == null)
          return false;
        else
          return _IndividualValues.Count > 0;
      }
    }

    #endregion

    #region Общие свойства и методы

    /// <summary>
    /// Возвращает true, если запрос пустой
    /// </summary>
    public bool IsEmpty { get { return _TablePages.Count == 0 && _IndividualValues.Count == 0; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      StringBuilder sb = new StringBuilder();
      if (HasTablePages)
      {
        sb.Append("Pages: ");
        for (int i = 0; i < TablePages.Count; i++)
        {
          if (i > 0)
            sb.Append(", ");
          TablePages[i].ToString(sb);
        }
      }
      if (HasIndividualValues)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("IndividualValues: ");
        for (int i = 0; i < IndividualValues.Count; i++)
        {
          if (i > 0)
            sb.Append(", ");
          IndividualValues[i].ToString(sb);
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Добавляет данные из другого запроса в текущий
    /// </summary>
    /// <param name="source"></param>
    internal void Add(DBxCacheLoadRequest source)
    {
      if (source.HasTablePages)
      {
        if (this.HasTablePages)
        {
          Dictionary<string, PageInfo> dict = new Dictionary<string, PageInfo>();
          for (int i = 0; i < this.TablePages.Count; i++)
            dict[this.TablePages[i].InternalKey] = this.TablePages[i];
          for (int i = 0; i < source.TablePages.Count; i++)
            dict[source.TablePages[i].InternalKey] = source.TablePages[i];

          _TablePages.Clear();
          _TablePages.AddRange(dict.Values);
        }
        else
        {
          _TablePages = new List<PageInfo>();
          _TablePages.AddRange(source.TablePages);
        }
      }

      if (source.HasIndividualValues)
      {
        if (this.HasIndividualValues)
        {
          Dictionary<string, IndividualInfo> dict = new Dictionary<string, IndividualInfo>();
          for (int i = 0; i < this.IndividualValues.Count; i++)
            dict[this.IndividualValues[i].InternalKey] = this.IndividualValues[i];
          for (int i = 0; i < source.IndividualValues.Count; i++)
            dict[source.IndividualValues[i].InternalKey] = source.IndividualValues[i];

          _IndividualValues.Clear();
          _IndividualValues.AddRange(dict.Values);
        }
        else
        {
          _IndividualValues = new List<IndividualInfo>();
          _IndividualValues.AddRange(source.IndividualValues);
        }
      }
    }

    internal DBxCacheLoadRequest Clone()
    {
      DBxCacheLoadRequest r2 = new DBxCacheLoadRequest();
      if (_TablePages != null)
      {
        r2._TablePages = new List<PageInfo>();
        r2._TablePages.AddRange(_TablePages);
      }
      if (_IndividualValues != null)
      {
        r2._IndividualValues = new List<IndividualInfo>();
        r2._IndividualValues.AddRange(_IndividualValues);
      }

      return r2;
    }

    /// <summary>
    /// Создает словарь запросов, разбивая их по отдельным таблицам.
    /// В словаре ключом является имя таблицы, значением - части запроса, относящиеся к таблицам
    /// </summary>
    /// <returns>Словарь запросов</returns>
    public Dictionary<string, DBxCacheLoadRequest> SplitByTables()
    {
      Dictionary<string, DBxCacheLoadRequest> dict = new Dictionary<string, DBxCacheLoadRequest>();
      if (HasTablePages)
      {
        for (int i = 0; i < TablePages.Count; i++)
        {
          DBxCacheLoadRequest r2;
          if (!dict.TryGetValue(TablePages[i].TableName, out r2))
          {
            r2 = new DBxCacheLoadRequest();
            dict.Add(TablePages[i].TableName, r2);
          }
          r2.TablePages.Add(TablePages[i]);
        }
      }
      if (HasIndividualValues)
      {
        for (int i = 0; i < IndividualValues.Count; i++)
        {
          DBxCacheLoadRequest r2;
          if (!dict.TryGetValue(IndividualValues[i].TableName, out r2))
          {
            r2 = new DBxCacheLoadRequest();
            dict.Add(IndividualValues[i].TableName, r2);
          }
          r2.IndividualValues.Add(IndividualValues[i]);
        }
      }

      return dict;
    }

    #endregion
  }

  /// <summary>
  /// Загруженные по запросу страницы кэша.
  /// Доступ к индивидуальным страницам невозможен из пользовательского кода.
  /// Метод ToCache() помещает страницы из этого объекта в локуальный кэш
  /// </summary>
  [Serializable]
  public sealed class DBxCacheLoadResponse
  {
    #region Защищенный конструктор

    internal DBxCacheLoadResponse()
    {
      //_TablePages = new List<DBxCacheTablePage>();
      //_IndividualValues = new List<DBxCacheIndividualValue>();
    }

    #endregion

    #region Страницы

    internal IDictionary<string, DBxCacheTablePage> TablePages
    {
      get
      {
        if (_TablePages == null)
          _TablePages = new Dictionary<string, DBxCacheTablePage>();
        return _TablePages;
      }
    }
    private Dictionary<string, DBxCacheTablePage> _TablePages;

    internal bool HasTablePages
    {
      get
      {
        if (_TablePages == null)
          return false;
        else
          return _TablePages.Count > 0;
      }
    }

    internal IDictionary<string, DBxCacheIndividualValue> IndividualValues
    {
      get
      {
        if (_IndividualValues == null)
          _IndividualValues = new Dictionary<string, DBxCacheIndividualValue>();
        return _IndividualValues;
      }
    }
    private Dictionary<string, DBxCacheIndividualValue> _IndividualValues;

    internal bool HasIndividualValues
    {
      get
      {
        if (_IndividualValues == null)
          return false;
        else
          return _IndividualValues.Count > 0;
      }
    }

    #endregion

    #region Общие свойства и методы

    /// <summary>
    /// Возвращает true, если нет ни одной передаваемой страницы
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return !(HasTablePages || HasIndividualValues);
      }
    }

    /// <summary>
    /// Вызывает Cache.SetItem() для всех страниц
    /// </summary>
    public void ToCache()
    {
      if (_TablePages != null)
      {
        foreach (DBxCacheTablePage p in _TablePages.Values)
        {
          Cache.SetItem<DBxCacheTablePage>(new string[4] { p.DBIdentityMD5, p.TableName, p.FirstId.ToString(), p.ColumnsMD5 },
            DBxTableCache.Persistance, p);
        }
      }
      if (_IndividualValues != null)
      {
        foreach (DBxCacheIndividualValue p in _IndividualValues.Values)
        {
          Cache.SetItem<DBxCacheIndividualValue>(new string[4] { p.DBIdentityMD5, p.TableName, p.Id.ToString(), p.ColumnName },
            DBxTableCache.Persistance, p);
        }
      }
    }

    /// <summary>
    /// Объединяет несколько ответов в один
    /// </summary>
    /// <param name="source">Массив ответов для объединения</param>
    /// <returns>Объединенный ответ</returns>
    public static DBxCacheLoadResponse Merge(DBxCacheLoadResponse[] source)
    {
      DBxCacheLoadResponse r = new DBxCacheLoadResponse();
      for (int i = 0; i < source.Length; i++)
        r.Add(source[i]);
      return r;
    }

    private void Add(DBxCacheLoadResponse source)
    {
      if (source.HasTablePages)
      {
        foreach (KeyValuePair<string, DBxCacheTablePage> pair in source.TablePages)
          TablePages[pair.Key] = pair.Value;
      }
      if (source.HasIndividualValues)
      {
        foreach (KeyValuePair<string, DBxCacheIndividualValue> pair in source.IndividualValues)
          IndividualValues[pair.Key] = pair.Value;
      }
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      StringBuilder sb = new StringBuilder();
      if (HasTablePages)
      {
        sb.Append("TablePages=");
        sb.Append(TablePages.Count.ToString());
      }
      if (HasIndividualValues)
      {
        if (sb.Length > 0)
          sb.Append(", ");

        sb.Append("IndividualValues=");
        sb.Append(IndividualValues.Count.ToString());
      }
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Исключение вызываемое при невозможности загрузить буферизованные данные
  /// </summary>
  [Serializable]
  public class DBxCacheLoadException : ApplicationException
  {
    #region Конструкторы

    internal DBxCacheLoadException(string message, DBxCacheLoadRequest request, Exception innerException)
      : base(message + ". Запрос DBxCache: " + request.ToString(), innerException)
    {
      if (request == null)
        throw new ArgumentNullException("request");
      _Request = request;
    }

    /// <summary>
    /// Этот конструктор требуется для десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxCacheLoadException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _Request = (DBxCacheLoadRequest)(info.GetValue("Request", typeof(DBxCacheLoadRequest)));
    }

    /// <summary>
    /// Используется для сериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Request", _Request);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Параметры запроса
    /// </summary>
    public DBxCacheLoadRequest Request { get { return _Request; } }
    private DBxCacheLoadRequest _Request;

    #endregion
  }

  /// <summary>
  /// Расширяет класс DataRowValueArray.
  /// Если таблица не содержит запрашиваемого ссылочного поля с точкой, то извлекается значение из кэша
  /// </summary>
  public class DBxDataRowValueArrayWithCache : DataRowValueArray, IDataRowNamedValuesAccess
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для длоступа к данным
    /// </summary>
    /// <param name="tableCache">Кэш таблицы</param>
    public DBxDataRowValueArrayWithCache(DBxTableCache tableCache)
    {
      if (tableCache == null)
        throw new ArgumentNullException("tableCache");
      _TableCache = tableCache;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Кэш таблицы
    /// </summary>
    public DBxTableCache tableCache { get { return _TableCache; } }
    private DBxTableCache _TableCache;

    #endregion

    #region IDataRowNamedValuesAccess

    /// <summary>
    /// Получить значение поля из текущей строки.
    /// Свойство CurrentRow должно быть предварительно установлено.
    /// </summary>
    /// <param name="name">Имя поля. Чувствительно к регистру</param>
    /// <returns>Значение поля</returns>
    public new object GetValue(string name)
    {
      if (CurrentRow == null)
        throw new NullReferenceException("Свойство CurrentRow не установлено");

      int p = ColumnNameIndexer.IndexOf(name);
      if (p >= 0)
        return CurrentRow[p];

      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      int p1 = name.IndexOf('.');
      if (p1 < 0)
      {
        // обычное поле
        Int32 Id = DataTools.GetInt(CurrentRow, "Id");
        if (Id < 0)
          return null;
        return tableCache.GetValue(Id, name);
      }
      else
      {
        // ссылочное поле

        string RefColumnName = name.Substring(0, p1);
        Int32 RefId;
        p = ColumnNameIndexer.IndexOf(RefColumnName);
        if (p < 0)
        {
          // ссылочного поля тоже нет
          Int32 Id = DataTools.GetInt(CurrentRow, "Id");
          if (Id < 0)
            return null;
          return tableCache.GetValue(Id, name);
        }
        else
        {
          RefId = DataTools.GetInt(CurrentRow[p]);
          if (RefId < 0)
            return null;
          return tableCache.GetRefValue(name, RefId);
        }
      }
    }


    ///// <summary>
    ///// Возвращает true, если таблица содержит столбец с заданным именем
    ///// </summary>
    ///// <param name="name">Имя поля</param>
    ///// <returns>Наличие поля</returns>
    //public bool Contains(string name)
    //{
    //  if (CurrentRow == null)
    //    return false;
    //  else
    //    return _ColumnNameIndexer.Contains(name);
    //}

    #endregion
  }

  /// <summary>
  /// Повторитель таблицы, который получает значения вычисляемых полей из кэша таблицы DBxTableCache.
  /// Прикладной код должен заполнить структуру таблицы SlaveTable, используя имена обычных и ссылочных полей.
  /// Предполагается, что MasterTable будет необходимые основные поля.
  /// </summary>
  public class DBxDataTableRepeaterWithCache : DataTableRepeater
  {
    #region Конструктор

    /// <summary>
    /// Создает повторитель
    /// </summary>
    /// <param name="tableCache">Доступ к кэшированным данным таблицы</param>
    public DBxDataTableRepeaterWithCache(DBxTableCache tableCache)
    {
      if (tableCache == null)
        throw new ArgumentNullException("tableCache");
      _TableCache = tableCache;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к кэшированным данным. Задается в конструкторе
    /// </summary>
    public DBxTableCache TableCache { get { return _TableCache; } }
    private DBxTableCache _TableCache;

    #endregion

    #region OnValueNeeded()

    /// <summary>
    /// Для полей с точкой вызывает DBxTableCache.GetRefValue().
    /// Затем вызывается обработчик события ValueNeeded.
    /// </summary>
    /// <param name="args">Аргументы события ValueNeeded</param>
    protected override void OnValueNeeded(DataTableRepeaterValueNeededEventArgs args)
    {
      int p = args.ColumnName.IndexOf('.');
      if (p >= 0)
      {
        string baseName = args.ColumnName.Substring(0, p);
        Int32 refId = DataTools.GetInt(args.SourceRow, baseName);
        if (refId >= 0) // 30.06.2021
          args.Value = _TableCache.GetRefValue(args.ColumnName, refId);
      }

      // Пользовательский обработчик вызывается последним
      base.OnValueNeeded(args);
    }

    #endregion
  }
}
