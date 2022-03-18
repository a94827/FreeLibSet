// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.ComponentModel;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;

namespace FreeLibSet.Data
{

  /// <summary>
  /// Абстрактный класс соединения с базой данных
  /// Для каждого типа базы данных определяется производный класс, реализующий выполнение запросов
  /// </summary>
  public abstract class DBxConBase : DisposableObject, IDBxCon
  {
    // 02.01.2021
    // Нельзя использовать SimpleDisposableObject, т.к. есть объект Connection у производных классов

    #region Конструктор

    /// <summary>
    /// Создает соединение и присоединяет его к точке входа
    /// </summary>
    /// <param name="entry">Обязательная ссылка на точку входа</param>
    protected DBxConBase(DBxEntry entry)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");
      entry.DB.CheckNotDisposed();

      _Entry = entry;

      _Buffer = new DBxSqlBuffer(entry.DB.Formatter);
      _Validator = new DBxNameValidator(entry, _Buffer);

      //_Thread = Thread.CurrentThread;

      lock (_Entry.DB._Cons.SyncRoot)
      {
        _Entry.DB._HasConnected = true;
        _Entry.DB._Cons.Add(this);
      }

      _LogoutSqlExceptions = true;
      _CommandTimeout = entry.DB.CommandTimeout;
    }

    /// <summary>
    /// Удаляет соединение из точки входа, к которой оно было присоединено в конструкторе
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Close вызывается производным классом
        // Если обратиться к DBConnection, будет создано новое соединение.
        //if (DbConnection != null)
        //  DbConnection.Close(); 
        _Entry.DB._Cons.Remove(this);
      }

      base.Dispose(disposing);

      if (disposing && ClearPoolCalled)
        GC.Collect(); // 09.09.2020
    }

    #endregion

    #region Свойства

    ///// <summary>
    ///// Поток к которому относится соединение
    ///// </summary>
    //public Thread Thread { get { return _Thread; } }
    //private Thread _Thread;

    /// <summary>
    /// Точка входа, к которой относится соедиенение
    /// </summary>
    public DBxEntry Entry { get { return _Entry; } }
    private DBxEntry _Entry;

    /// <summary>
    /// База данных, к которой относится соединение
    /// </summary>
    public DBx DB { get { return _Entry.DB; } }

    /// <summary>
    /// Объект для сборки строк SQL-команд и форматизатор SQL-запросов
    /// </summary>
    protected DBxSqlBuffer Buffer { get { return _Buffer; } }
    private DBxSqlBuffer _Buffer;

    /// <summary>
    /// Нужно ли создавать log-файл, если при выполнении SQL-запроса возникло исключение.
    /// По умолчанию - true.
    /// </summary>
    public bool LogoutSqlExceptions
    {
      get { return _LogoutSqlExceptions; }
      set { _LogoutSqlExceptions = value; }
    }
    private bool _LogoutSqlExceptions;

    /// <summary>
    /// Нужно ли выполнять обрезку значений текстовых полей по размеру поля перед выполнением изменяющих запросов.
    /// По умолчанию - false
    /// </summary>
    public bool TrimValues
    {
      get { return _TrimValues; }
      set { _TrimValues = value; }
    }
    private bool _TrimValues;


    /// <summary>
    /// Время выполнения команд в секундах. 0-бесконечное время ожидания.
    /// При создании объекта соединения, свойство имеет значение равное заданному в DBx.CommandTimeout (обычно, 30 секунд).
    /// Установка этого свойства влияет только на текущее соединение.
    /// Это свойство не дублируется в основном соединении DBxCon. Следовательно, проверка может быть отключена
    /// только на стороне сервера (безопасность).
    /// </summary>
    public int CommandTimeout
    {
      get { return _CommandTimeout; }
      set
      {
        if (_CommandTimeout < 0)
          throw new ArgumentException("Значение не может быть отрицательным");
        _CommandTimeout = value;
      }
    }
    private int _CommandTimeout;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("DB=");
      sb.Append(DB.DisplayName);
      sb.Append(", Entry=");
      if (String.IsNullOrEmpty(Entry.DisplayName))
        sb.Append("[without display name]");
      else
        sb.Append(Entry.DisplayName);
      return sb.ToString();
    }

    #endregion

    #region Выполнение запросов

    #region SELECT

    /// <summary>
    /// Загрузка всей таблицы.
    /// Загружаются все поля SELECT * FROM [TableName] 
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName)
    {
      return FillSelect(tableName, null, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames)
    {
      return FillSelect(tableName, columnNames, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where)
    {
      return FillSelect(tableName, columnNames, where, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию с заданным порядком сортировки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <param name="orderBy">Порядок сортировки</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where, DBxOrder orderBy)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(columnNames);
      info.Where = where;
      info.OrderBy = orderBy;
      return FillSelect(info);
    }

    /// <summary>
    /// Вызов оператора SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Заполненная таблица с результатами запроса</returns>
    public DataTable FillSelect(DBxSelectInfo info)
    {
      DBxSelectFormatter fsf = new DBxSelectFormatter(info, Validator);
      Buffer.Clear();
      fsf.Format(Buffer);
      DataTable table = SQLExecuteDataTable(Buffer.SB.ToString(), info.TableName);

      // Мы не устанавливали альясы на имена полей, т.к. альясы не могут содержать
      // точки, а нам очень хочется вернуть имена столбцов точно так, как они были
      // запрошены
      try
      {
        fsf.CorrectColumnNames(table);
        fsf.CorrectColumnTypes(ref table, Buffer);
      }
      catch (Exception e)
      {
        e.Data["DBxConBase"] = this.ToString();
        e.Data["SQLQueryText"] = Buffer.SB.ToString();
        try
        {
          e.Data["DbConnection.State"] = DbConnection.State;
        }
        catch { }
        throw;
      }

      return table;
    }


    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Набор данных из одной таблицы с единственной колонкой</returns>
    public DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(columnName);
      info.Where = where;
      info.Unique = true;
      info.OrderBy = new DBxOrder(columnName);
      return FillSelect(info);
    }


    /// <summary>
    /// Загрузка всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName)
    {
      return ReaderSelect(tableName, null, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames)
    {
      return ReaderSelect(tableName, columnNames, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames, DBxFilter where)
    {
      return ReaderSelect(tableName, columnNames, where, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <param name="orderBy">Порядок сортировки</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames, DBxFilter where, DBxOrder orderBy)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(columnNames);
      info.Where = where;
      info.OrderBy = orderBy;
      return ReaderSelect(info);
    }

    /// <summary>
    /// Универсальный метод выполнения запроса SELECT.
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(DBxSelectInfo info)
    {
      DBxSelectFormatter fsf = new DBxSelectFormatter(info, Validator);
      Buffer.Clear();
      fsf.Format(Buffer);

      string cmdText = Buffer.SB.ToString();

      DbDataReader rdr = SQLExecuteReader(cmdText);

#if XXXXX
      // Мы не устанавливали альясы на имена полей, т.к. альясы не могут содержать
      // точки, а нам очень хочется вернуть имена столбцов точно так, как они были
      // запрошены
      if (FillSelectHelper.HasDotFields)
      {
        if (FieldNames != null)
        {
          for (i = 0; i < FieldNames.Count; i++)
            ds.Tables[0].Columns[i].ColumnName = FieldNames[i];
        }
        else
        {
          // Имена полей не были заданы. Однако, часть столбцов теперь имеет имена
          // ИмяТаблицы.ИмяПоля, а часть - нет
          for (i = 0; i < ds.Tables[0].Columns.Count; i++)
          {
            if (ds.Tables[0].Columns[i].ColumnName.StartsWith(TableName + "."))
              ds.Tables[0].Columns[i].ColumnName =
                ds.Tables[0].Columns[i].ColumnName.Substring(TableName.Length + 1);
          }
        }
      }
#endif
      return rdr;
    }

    #endregion

    #region FindRecord

    /// <summary>
    /// Найти строку с заданным значением поля
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, string columnName, object value)
    {
      string[] columnNames = new string[1];
      columnNames[0] = columnName;
      object[] values = new object[1];
      values[0] = value;
      return FindRecord(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      return FindRecord(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      return FindRecord(tableName, columnNames, values, (DBxOrder)null);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей. 
    /// Если задан порядок сортировки, то отыскиваются все строки с заданными значениями, 
    /// они упорядочиваются и возвращается идентификатор первой строки. Если OrderBy=null,
    /// то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <param name="orderBy">Порядок сортировки (может быть null)</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values, DBxOrder orderBy)
    {
#if DEBUG
      if (columnNames == null || columnNames.Count == 0)
        throw new ArgumentException("Не задано ни одного поля для поиска в FindRecord для таблицы " + tableName, "columnNames");
#endif

      DBxFilter filter = ValueFilter.CreateFilter(columnNames, values);

      return FindRecord(tableName, filter, orderBy);
    }

    /// <summary>
    /// Поиск первой строки, удовлетворяющей условию.
    /// Если есть несколько подходящих строк, то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр</param>
    public Int32 FindRecord(string tableName, DBxFilter where)
    {
      return FindRecord(tableName, where, false);
    }

    ///// <summary>
    ///// Поиск любой строки таблицы без всяких условий
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <returns>Идентификатор первой попавшейся записи или 0, если таблица не содержит записей</returns>
    //public Int32 FindRecord(string tableName)
    //{
    //  return FindRecord(tableName, (DBxFilter)null, (DBxOrder)null);
    //}

    /// <summary>
    /// Выполняет поиск записи, удовлетворяющей условиям фильтра <paramref name="where"/>, и возвращает идентификатор найденной записи.
    /// Если нет ни одной строки таблицы, удовлетворяющей условиям, возвращает 0.
    /// Если найдено несколько подходящих записей, то используется заданный порядок сортировки <paramref name="orderBy"/> 
    /// и возвращается первая запись. Если порядок сортировки не задан, то не определено, какая запись будет возвращена.
    /// Если не задано условие <paramref name="where"/> то будет возвращена первая запись в таблице
    /// согласно порядку сортировки (если <paramref name="orderBy"/> задан).
    /// Оба аргумента <paramref name="where"/> и <paramref name="orderBy"/> могут быть null,
    /// тогда будет возвращена произвольная запись из таблицы, что обычно не имеет смысла.
    /// Имена полей в фильтрах <paramref name="where"/> и порядке сортировки <paramref name="orderBy"/>
    /// могут содержать точки (ссылочные поля).
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано обязательно</param>
    /// <param name="where">Условия отбора или null</param>
    /// <param name="orderBy">Порядок сортировки или null</param>
    /// <returns>Идентификатор найденной записи или null</returns>
    public Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);


      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(DB.Struct.Tables[tableName].PrimaryKey);
      info.Where = where;
      info.OrderBy = orderBy;
      info.MaxRecordCount = 1;

      DBxSelectFormatter fsf = new DBxSelectFormatter(info, Validator);
      Buffer.Clear();
      fsf.Format(Buffer);
      object res = SQLExecuteScalar(Buffer.SB.ToString());
      if (res == null)
        return 0;
      return DataTools.GetInt(res);
    }

    /// <summary>
    /// Поиск записи в таблице.
    /// Таблица должна иметь первичный ключ по числовому полю.
    /// Возвращает идентификатор строки (значение первичного ключа), если запись найдена.
    /// Возвращает 0, если запись не найдена.
    /// Можно задать дополнительное ограничение на уникальнойсть найденной записи.
    /// Если огранчиение указано и найдено больше одной строки, возвращается 0.
    /// Имена полей в фильтрах <paramref name="where"/> могут содержать точки (ссылочные поля).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие поиска</param>
    /// <param name="singleOnly">Если true, то найденная запись должна быть уникальной</param>
    /// <returns>Имя столбца (обычно, "Id")</returns>
    public Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);

      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(DB.Struct.Tables[tableName].PrimaryKey);
      info.Where = where;

      if (singleOnly)
        info.MaxRecordCount = 2; // чтобы отличить, найдена ли ровно одна запись
      else
        info.MaxRecordCount = 1;

      DBxSelectFormatter fsf = new DBxSelectFormatter(info, Validator);
      Buffer.Clear();
      fsf.Format(Buffer);

      if (singleOnly)
      {
        DataTable table = SQLExecuteDataTable(Buffer.SB.ToString(), tableName);
        if (table.Rows.Count == 1)
          return DataTools.GetInt(table.Rows[0][0]);
        else
          return 0;
      }
      else
      {
        object res = SQLExecuteScalar(Buffer.SB.ToString());
        if (res == null)
          return 0;
        return DataTools.GetInt(res);
      }
    }

    #endregion

#if XXX
    #region FindRecordValue

    /// <summary>
    /// Найти строку в таблице с указанными значениями полей (ключами) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено.
    /// Этот метод не поддерживает использование ссылочных полей (с точками).
    /// Используйте метод FillSelect() при необходимости.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnNames">Имена ключевых полей для поиска. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValues">Значения ключевых полей</param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    public object FindRecordValue(string tableName, DBxColumns searchColumnNames, object[] searchValues, string resColumnName)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnNames(tableName, searchColumnNames, false, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, resColumnName, false, DBxAccessMode.ReadOnly);

      if (searchColumnNames.Count != searchValues.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений", "searchValues");

      if (searchColumnNames.Count == 0)
        throw new ArgumentException("Не задано ни одного поля для поиска в FindRecordValue для таблицы " + tableName, "searchColumnNames");

      Buffer.SB.Append("SELECT TOP 1 ");
      Buffer.FormatColumnName(resColumnName);
      Buffer.SB.Append(" FROM ");
      Buffer.FormatTableName(tableName);
      DBxFilter Filter = ValueFilter.CreateFilter(searchColumnNames, searchValues);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatFilter(Filter);

      object res = SQLExecuteScalar(Buffer.SB.ToString());
      return res;
    }

    /// <summary>
    /// Найти строку в таблице с указанным значением заданного поля (используется ValueFilter) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено.
    /// Этот метод не поддерживает использование ссылочных полей (с точками).
    /// Используйте метод FillSelect() при необходимости.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnName">Имя поля фильтра. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValue">Значение поля фильтра <paramref name="searchColumnName"/></param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    public object FindRecordValue(string tableName, string searchColumnName, object searchValue, string resColumnName)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, searchColumnName, false, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, resColumnName, false, DBxAccessMode.ReadOnly);

      Buffer.SB.Append("SELECT TOP 1 ");
      Buffer.FormatColumnName(resColumnName);
      Buffer.SB.Append(" FROM ");
      Buffer.FormatTableName(tableName);
      DBxFilter Filter = new ValueFilter(searchColumnName, searchValue);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatFilter(Filter);

      object res = SQLExecuteScalar(Buffer.SB.ToString());
      return res;
    }

    #endregion
#endif

    #region GetIds

    /// <summary>
    /// Получить массив идентификаторов строк с заданным значением поля
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, string columnName, object value)
    {
      string[] columnNames = new string[1];
      columnNames[0] = columnName;
      object[] values = new object[1];
      values[0] = value;
      return GetIds(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Получить массив идентификаторов строк с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Массив идентификаторов строк</returns>
    public IdList GetIds(string tableName, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      return GetIds(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Получить массив идентификаторов строк таблицы с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Таблица</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, DBxColumns columnNames, object[] values)
    {
      if (columnNames.Count == 0)
        throw new ArgumentException("Не задано ни одного поля для поиска в GetIds для таблицы " + tableName, "columnNames");

      DBxFilter filter = ValueFilter.CreateFilter(columnNames, values);
      return GetIds(tableName, filter);
    }

    /// <summary>
    /// Получение массива идентификаторов первичного ключа (обычно, поля "Id") для всех строк, удовлетворяющих
    /// условию.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие (фильтр)</param>
    /// <returns>Массив идентификаторов (значений поля "Id")</returns>
    public IdList GetIds(string tableName, DBxFilter where)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);
      IdList lst = new IdList();

      using (DbDataReader rdr = ReaderSelect(tableName, DB.Struct.Tables[tableName].PrimaryKey, where))
      {
        while (rdr.Read())
          lst.Add(rdr.GetInt32(0));
      }

      return lst;
    }

    #endregion

    #region Информационные методы

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    public virtual int GetRecordCount(string tableName)
    {

      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      //Buffer.SB.Append("SELECT COUNT(*) AS [cnt] FROM ");
      Buffer.SB.Append("SELECT COUNT(*) FROM "); // 09.08.2016
      Buffer.FormatTableName(tableName);
      //return (int)SQLExecuteScalar(Buffer.SB.ToString());
      return DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString())); // 09.08.2016
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию.
    /// Если условие не задано, возвращается общее число строк в таблице.
    /// Этот метод не поддерживает ссылочные поля (с точками)
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    public virtual int GetRecordCount(string tableName, DBxFilter where)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);

      // Buffer.SB.Append("SELECT COUNT(*) AS [cnt] FROM ");
      Buffer.SB.Append("SELECT COUNT(*) FROM "); // 09.08.2016
      Buffer.FormatTableName(tableName);

      if (where != null)
      {
        switch (where.Degeneration)
        {
          case DBxFilterDegeneration.None:
            Buffer.SB.Append(" WHERE ");
            Buffer.FormatFilter(where);
            break;
          case DBxFilterDegeneration.AlwaysFalse:
            return 0;
        }
      }

      //return (int)SQLExecuteScalar(Buffer.SB.ToString());
      return DataTools.GetInt(SQLExecuteScalar(Buffer.SB.ToString())); // 09.08.2016
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    public virtual bool IsTableEmpty(string tableName)
    {
      return GetRecordCount(tableName) == 0;
    }

    #endregion

    #region Получение значений полей

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. 
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Имя поля может содержать точки для получения значения ссылочного поля с помощью INNER JOIN.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <returns>Значение</returns>
    public object GetValue(string tableName, Int32 id, string columnName)
    {
      object value;
      if (!GetValue(tableName, id, columnName, out value))
      {
        if (id != 0)
          throw new DBxRecordNotFoundException(tableName, id.ToString()); // 20.12.2019
      }
      return value;
    }

    /// <summary>
    /// Получение значения для одного поля. Имя поля <paramref name="columnName"/> может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена.
    /// Таблица должна иметь целочисленный первичный ключ.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0, тогда возвращается Value=null 
    /// без выполнения SQL-запроса</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <param name="value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    public virtual bool GetValue(string tableName, Int32 id, string columnName, out object value)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, columnName, true, DBxAccessMode.ReadOnly);
      Validator.CheckTablePrimaryKeyInt32(tableName);

      value = null;

      if (id == 0)
        return false;

      int p = columnName.IndexOf('.');
      if (p >= 0)
      {
        // Ссылочное поле
        string colName1 = columnName.Substring(0, p);
        object value1;
        if (!GetValue(tableName, id, colName1, out value1))
          return false;
        Int32 id2 = DataTools.GetInt(value1);
        if (id2 == 0)
          return false;

        // Находим ссылочную таблицу
        DBxColumnStruct colDef = DB.Struct.Tables[tableName].Columns[colName1];
        if (String.IsNullOrEmpty(colDef.MasterTableName))
          throw new InvalidOperationException("Поле \"" + colName1 + "\" таблицы \"" + tableName + "\" не является ссылочным");
        // Рекурсивный вызов
        return GetValue(colDef.MasterTableName, id2, columnName.Substring(p + 1), out value);
      }

      Buffer.SB.Append("SELECT ");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append(" FROM ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatColumnName(DB.Struct.Tables[tableName].PrimaryKey[0]);
      Buffer.SB.Append("=");
      Buffer.FormatValue(id, DBxColumnType.Int);

      object res = SQLExecuteScalar(Buffer.SB.ToString());
      if (res == null)
        return false; // нет строки

      if (!(res is DBNull))
        value = res;
      return true;
    }

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращается массив значений null подходящей длины.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    public virtual object[] GetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);

      if (id == 0)
        return new object[columnNames.Count];

      DbDataReader rdr = ReaderSelect(tableName, columnNames, new IdsFilter(DB.Struct.Tables[tableName].PrimaryKey[0], id), null);
      try
      {
        object[] res = DoGetValuesFromReader(rdr);
        if (res == null)
          throw new DBxRecordNotFoundException(tableName, id.ToString()); // 20.12.2019
        else
          return res;
      }
      finally
      {
        rdr.Close();
      }
    }

    private object[] DoGetValuesFromReader(DbDataReader rdr)
    {
      if (!rdr.Read())
        return null;

      object[] res = new object[rdr.FieldCount];

      for (int i = 0; i < res.Length; i++)
      {
        res[i] = rdr.GetValue(i);
        // Заменяем DBNull на null
        if (res[i] is DBNull)
          res[i] = null;
      }
      return res;
    }

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// Если задан нулевой или несуществующий идентификатор записи, возвращается массив значений null
    /// требуемой длины.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    public object[] GetValues(string tableName, Int32 id, string columnNames)
    {
      DBxColumns columnNames2 = new DBxColumns(columnNames);
      return GetValues(tableName, id, columnNames2);
    }


#if XXX
    /// <summary>
    /// Внутренняя функция извлечения значений полей из набора данных, созданного
    /// FillSelect() в виде массива значений для GetValues()
    /// </summary>
    /// <param name="ds2">Набор данных с единственной таблицей, которая содержит
    /// одну строку или пустая</param>
    /// <param name="SearchFieldNames">Имена полей, которые нужно извлечь</param>
    /// <returns>Массив значений полей или массив значений null</returns>
    private object[] GetValuesFromDataSet(DataSet ds, DataFields FieldNames)
    {
      if (ds.Tables[0].Rows.Count == 0)
        return new object[FieldNames.Count];
      object[] res = ds.Tables[0].Rows[0].ItemArray;
      // Заменяем DBNull на нулл
      for (int i = 0; i < FieldNames.Count; i++)
      {
        if (res[i] is DBNull)
          res[i] = null;
      }
      return res;
    }
#endif

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается null.
    /// В текущей реализации не поддерживаются ссылочные поля (с точками).
    /// </summary>                                             
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Значение поля или null</returns>
    public virtual object GetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, columnName, false, DBxAccessMode.ReadOnly);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);

      // здесь не надо AddNotNullFilterIfRequired(ref where, tableName, columnName); 

      Buffer.SB.Append("SELECT MAX (");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append(") FROM ");
      Buffer.FormatTableName(tableName);
      if (where != null)
      {
        switch (where.Degeneration)
        {
          case DBxFilterDegeneration.None:
            Buffer.SB.Append(" WHERE ");
            Buffer.FormatFilter(where);
            break;
          case DBxFilterDegeneration.AlwaysFalse:
            return null;
        }
      }
      return SQLExecuteScalar(Buffer.SB.ToString());
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей максимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Длина возвращаемого массива всегда совпадает с количеством полей в <paramref name="columnNames"/>.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="maxColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="maxColumnName">Имя поля, максимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    public virtual object[] GetValuesForMax(string tableName, DBxColumns columnNames,
      string maxColumnName, DBxFilter where)
    {
      AddNotNullFilterIfRequired(ref where, tableName, maxColumnName); // 27.11.2019

      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(columnNames);
      info.Where = where;
      info.OrderBy = new DBxOrder(maxColumnName, ListSortDirection.Descending); // по убыванию
      info.MaxRecordCount = 1; // одна строка

      DbDataReader rdr = ReaderSelect(info);
      try
      {
        object[] res = DoGetValuesFromReader(rdr);
        if (res == null)
          return new object[columnNames.Count];
        else
          return res;
      }
      finally
      {
        rdr.Close();
      }

    }

    /// <summary>
    /// Получить миниимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается null.
    /// В текущей реализации не поддерживаются ссылочные поля (с точками).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns></returns>
    public virtual object GetMinValue(string tableName, string columnName, DBxFilter where)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, columnName, false, DBxAccessMode.ReadOnly);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);

      // здесь не надо AddNotNullFilterIfRequired(ref where, tableName, columnName); 

      Buffer.SB.Append("SELECT MIN (");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append(") FROM ");
      Buffer.FormatTableName(tableName);
      if (where != null)
      {
        switch (where.Degeneration)
        {
          case DBxFilterDegeneration.None:
            Buffer.SB.Append(" WHERE ");
            Buffer.FormatFilter(where);
            break;
          case DBxFilterDegeneration.AlwaysFalse:
            return null;
        }
      }
      return SQLExecuteScalar(Buffer.SB.ToString());
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей миниимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Длина возвращаемого массива всегда совпадает с количеством полей в <paramref name="columnNames"/>.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="minColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="minColumnName">Имя поля, минимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    public virtual object[] GetValuesForMin(string tableName, DBxColumns columnNames,
      string minColumnName, DBxFilter where)
    {
      AddNotNullFilterIfRequired(ref where, tableName, minColumnName); // 27.11.2019

      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(columnNames);
      info.Where = where;
      info.OrderBy = new DBxOrder(minColumnName, ListSortDirection.Ascending); // 27.11.2019 - по возрастанию
      info.MaxRecordCount = 1; // одна строка

      DbDataReader rdr = ReaderSelect(info); // одна строка
      try
      {
        object[] res = DoGetValuesFromReader(rdr);
        if (res == null)
          return new object[columnNames.Count];
        else
          return res;
      }
      finally
      {
        rdr.Close();
      }
    }


    /// <summary>
    /// Если столбец <paramref name="columnName"/> может принимать значения NULL, то требуется добавить фильтр к <paramref name="where"/>,
    /// чтобы значения NULL исключить из поиска минимального/максимального значения
    /// </summary>
    /// <param name="where">Фильтр, переданный из вызывающего кода. Может быть null. Сюда помещается модифицированный фильтр, если
    /// требуется</param>
    /// <param name="tableName">Таблица</param>
    /// <param name="columnName">Проверяемое поле</param>
    private void AddNotNullFilterIfRequired(ref DBxFilter where, string tableName, string columnName)
    {
      DBxTableStruct ts = DB.Struct.Tables[tableName];
      if (ts == null)
        return; // Мы не сможем добавить NullNotNullFilter, так как для него нужен тип данных, а мы его не знаем.
      // Проверрки на null нужны при NameCheckingEnabled=false
      DBxColumnStruct cs = ts.Columns[columnName];
      if (cs == null)
        return;

      if (!cs.Nullable)
        return; // столбец не поддерживает значения NULL, не о чем беспокоиться

      if (where == null)
      {
        // Если пользовательский код не передал фильтров, просто используем свой фильтр
        where = new NotNullFilter(columnName, cs.DataType);
        return;
      }

      // Ищем в фильтре явно заданный NotNullFilter для нашего поля.
      // В фильтрах могут быть, например выражения типа "columnName > 0", но лучше все равно добавить проверку на NULL,
      // т.к., например "columnName > -1" уже не сработает
      List<DBxFilter> list = new List<DBxFilter>();
      where.GetAllFilters(list);
      for (int i = 0; i < list.Count; i++)
      {
        NotNullFilter f = list[i] as NotNullFilter;
        if (f != null)
        {
          DBxColumn colexpr = f.Expression1 as DBxColumn;
          if (colexpr != null)
          {
            if (colexpr.ColumnName == columnName)
              return; // нашли уже существующий фильтр
          }
        }
      }

      // Добавляем фильтр
      where = new AndFilter(new NotNullFilter(columnName, cs.DataType), where);
    }

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// В текущей реализации не поддерживаются ссылочные поля (с точками).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns></returns>
    public object GetSumValue(string tableName, string columnName, DBxFilter where)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, columnName, false, DBxAccessMode.ReadOnly);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);

      Buffer.SB.Append("SELECT SUM (");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append(") FROM ");
      Buffer.FormatTableName(tableName);
      if (where != null)
      {
        switch (where.Degeneration)
        {
          case DBxFilterDegeneration.None:
            Buffer.SB.Append(" WHERE ");
            Buffer.FormatFilter(where);
            break;
          case DBxFilterDegeneration.AlwaysFalse:
            return null;
        }
      }
      return SQLExecuteScalar(Buffer.SB.ToString());
    }

#if XXX

    /// <summary>
    /// Получить значение для ссылочного поля
    /// Анализирует структуру таблицы, чтобы узнать имя таблицы, на которую ссылается
    /// поле.
    /// </summary>
    /// <param name="TableName">Имя таблицы, содержащей ссылочное поле</param>
    /// <param name="RefFieldName">Имя поля в виде ИмяСсылочногоПоля.ИмяТребуемогоПоля</param>
    /// <param name="RefValue">Значение ссылочного поля</param>
    /// <returns>Значение требуемого поля</returns>
    public object GetRefValue(string TableName, string RefFieldName, int RefValue)
    {
      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");
      if (String.IsNullOrEmpty(RefFieldName))
        throw new ArgumentNullException("RefFieldName");

      if (RefValue == 0)
        return null;

      // Ищем описание таблицы
      DBStruct.Table TableDef = DBStruct.Tables[TableName];
      if (TableDef == null)
        throw new ArgumentException("Не найдено описание таблицы \"" + TableName + "\"");

      // Ищем описание поля
      int p = RefFieldName.IndexOf('.');
      if (p < 0)
        throw new ArgumentException("Неправильный формат имени ссылочного поля \"" + RefFieldName + "\". Нет точки");
      string MainFieldName = RefFieldName.Substring(0, p);
      DBStruct.Field FieldDef = TableDef.Fields[MainFieldName];
      if (FieldDef == null)
        throw new ArgumentException("Неправильное имя поля \"" + RefFieldName + "\". Описание основного поля \"" + MainFieldName +
          "\" не найдено в таблице \"" + TableDef.TableName + "\"");
      if (FieldDef.FieldType != DBStruct.AccDepFieldType.Reference)
        throw new ArgumentException("Неправильное имя поля \"" + RefFieldName + "\". Основное поле \"" + MainFieldName +
          "\" не является ссылочным");

      string WantedFieldName = RefFieldName.Substring(p + 1);
      return GetValue(FieldDef.MasterTableName, RefValue, WantedFieldName);
    }
#endif

    #endregion

    #region GetUniqueXxxValues()

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<string> lst = new SingleScopeList<string>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(rdr.GetString(0).TrimEnd());
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<int> lst = new SingleScopeList<int>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(DataTools.GetInt(rdr.GetValue(0)));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<long> lst = new SingleScopeList<long>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(DataTools.GetInt64(rdr.GetValue(0)));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<float> lst = new SingleScopeList<float>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(DataTools.GetSingle(rdr.GetValue(0)));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<double> lst = new SingleScopeList<double>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(DataTools.GetDouble(rdr.GetValue(0)));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<decimal> lst = new SingleScopeList<decimal>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(DataTools.GetDecimal(rdr.GetValue(0)));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить значения поля даты и/или времени без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<DateTime> lst = new SingleScopeList<DateTime>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(rdr.GetDateTime(0));
        }
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Получить значения поля GUID без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = tableName;
      info.Expressions.Add(new DBxColumn(columnName));
      info.Where = where;
      info.OrderBy = new DBxOrder(columnName);
      info.Unique = true;

      SingleScopeList<Guid> lst = new SingleScopeList<Guid>();
      using (DbDataReader rdr = ReaderSelect(info))
      {
        while (rdr.Read())
        {
          if (rdr.IsDBNull(0))
            continue;
          lst.Add(rdr.GetGuid(0));
        }
      }
      return lst.ToArray();
    }

    #endregion

    #region Запись значений полей

    /// <summary>
    /// Установить значения одного поля для одной строки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки (значение первичного ключа)</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string tableName, Int32 id, string columnName, object value)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);

      if (id == 0)
        throw new DBxNoIdArgumentException("Не задан идентификатор записи", "id"); // 20.12.2019

      string[] columnNames = new string[1];
      columnNames[0] = columnName;
      object[] Values = new object[1];
      Values[0] = value;

      SetValues(tableName, new IdsFilter(DB.Struct.Tables[tableName].PrimaryKey[0], id), new DBxColumns(columnNames), Values);
    }

    /// <summary>
    /// Установить значение одного поля для нескольких строк таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string tableName, DBxFilter where, string columnName, object value)
    {
      string[] columnNames = new string[1];
      columnNames[0] = columnName;
      object[] values = new object[1];
      values[0] = value;

      SetValues(tableName, where, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Записать значения полей. Выполняет SQL-запрос "UPDATE".
    /// Ссылочные поля (с точками) не поддерживаются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по измененяемым строкам таблицы.</param>
    /// <param name="columnNames">Имена записываемых полей</param>
    /// <param name="values">Записываемые значения. Длина массива должна совпадать с количеством столбцов в <paramref name="columnNames"/>.</param>
    public virtual void SetValues(string tableName, DBxFilter where, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);

      // 12.07.2021
      // Фильтра может не быть
      //if (where == null)
      //  throw new ArgumentNullException("where");
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Buffer.SB.Append("UPDATE ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" SET ");

      for (int i = 0; i < columnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.FormatColumnName(columnNames[i]);
        Buffer.SB.Append("=");
        Buffer.FormatValue(values[i], columnTypes[i]);
      }

      if (where != null) // 12.07.2021
      {
        Buffer.SB.Append(" WHERE ");
        Buffer.FormatFilter(where);
      }
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Установить значения полей для записи с заданным идентификатором
    /// В отличие от других вариантов вызова функции, выполняется 5 попыток 
    /// установить значение, если происходит блокировка записи в другом потоке
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="columnNames">Имена устанавливаемых полей</param>
    /// <param name="values">Записываемые значения полей</param>
    public void SetValues(string tableName, Int32 id, DBxColumns columnNames, object[] values)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);
      if (id == 0)
        throw new DBxNoIdArgumentException("Не задан идентификатор записи", "id"); // 20.12.2019

      string PKColumnName = DB.Struct.Tables[tableName].CheckTablePrimaryKeyInt32();

      SetValues(tableName, new IdsFilter(PKColumnName, id), columnNames, values);
    }


    /// <summary>
    /// Установка значений нескольких полей для одной таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    public void SetValues(string tableName, Int32 id, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      SetValues(tableName, id, new DBxColumns(columnNames), values);
    }
    
    /// <summary>
    /// Записать значения полей. Выполняет SQL-запрос "UPDATE".
    /// Ссылочные поля (с точками) не поддерживаются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по измененяемым строкам таблицы.</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    public virtual void SetValues(string tableName, DBxFilter where, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      SetValues(tableName, where, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Выполняет укорачивание длины текстовых полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов</param>
    /// <param name="values">Значения</param>
    protected void PerformTrimValues(string tableName, DBxColumns columnNames, object[] values)
    {
      DBxTableStruct ts = DB.Struct.Tables[tableName];

      for (int i = 0; i < columnNames.Count; i++)
      {
        DBxColumnStruct cs = ts.Columns[columnNames[i]];
        if (cs.ColumnType == DBxColumnType.String)
        {
          string s = (string)(values[i]);
          if (!String.IsNullOrEmpty(s))
          {
            if (s.Length > cs.MaxLength)
            {
              s = s.Substring(0, cs.MaxLength);
              values[i] = s;
            }
          }
        }
      }
    }

    /// <summary>
    /// Выполняет укорачивание длины текстовых полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Таблица данных, откуда извлекаются значения и куда записываются исправленные значения</param>
    protected void PerformTrimValues(string tableName, DataTable table)
    {
      if (table.Rows.Count == 0)
        return;

      #region Определение необходимости обрезки

      DBxTableStruct ts = DB.Struct.Tables[tableName];

      DBxColumnStruct[] colDefs = new DBxColumnStruct[table.Columns.Count];

      bool needed = false;
      for (int i = 0; i < table.Columns.Count; i++)
      {
        DBxColumnStruct cs = ts.Columns[table.Columns[i].ColumnName];
        if (cs.ColumnType == DBxColumnType.String)
        {
          colDefs[i] = cs;
          needed = true;
        }
      }

      if (!needed)
        return;

      #endregion

      #region Обрезка

      foreach (DataRow row in table.Rows)
      {
        for (int i = 0; i < table.Columns.Count; i++)
        {
          DBxColumnStruct cs = colDefs[i];
          if (cs == null)
            continue;

          string s = DataTools.GetString(row[i]);
          if (!String.IsNullOrEmpty(s))
          {
            if (s.Length > cs.MaxLength)
            {
              s = s.Substring(0, cs.MaxLength);
              row[i] = s;
            }
          }
        }
      }

      #endregion
    }

    #endregion

    #region Добавление и обновление записей

    #region AddRecordWithIdResult()

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// В строку записывается значение только одного поля (не считая автоинкрементного), поэтому, обычно следует использовать 
    /// другие перегрузки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя устанавливаемого столбца</param>
    /// <param name="value">Значение поля</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    public Int32 AddRecordWithIdResult(string tableName, string columnName, object value)
    {
      object[] values = new object[1];
      values[0] = value;
      return AddRecordWithIdResult(tableName, new DBxColumns(columnName), values);
    }

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых полей и значения</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    public Int32 AddRecordWithIdResult(string tableName, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      return AddRecordWithIdResult(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Добавить строку с автоматическим присвоением идентификатора.
    /// Полученный идентификатор возвращается и может быть использован для ссылок на строку
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    /// <returns>Идентификатор добавленной записи</returns>
    public abstract Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values);

    #endregion

    #region AddRecord()

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// Эта перегрузка позволяет задать только одно значение поля для строки,
    /// поэтому имеет очень ограниченное применение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца, для которого задается значение</param>
    /// <param name="value">Значение для поля <paramref name="columnName"/></param>
    public void AddRecord(string tableName, string columnName, object value)
    {
      object[] values = new object[1];
      values[0] = value;
      AddRecord(tableName, new DBxColumns(columnName), values);
    }

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена столбцов и значения полей</param>
    public void AddRecord(string tableName, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      AddRecord(tableName, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Добавить строку в таблицу.
    /// Список столбцов <paramref name="columnNames"/> может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// Полученный идентификатор не возвращается, благодаря чему можно ограничиться запросом INSERT INTO 
    /// без дополнительных действий по получению идентификатора.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    public virtual void AddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      DBxColumnType[] ColumnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);

      if (columnNames.Count != values.Length)
        throw new ArgumentException("Число полей не совпадает с числом значений");

      if (TrimValues)
        PerformTrimValues(tableName, columnNames, values);

      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(tableName); // 20.09.2019
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(columnNames);
      Buffer.SB.Append(") VALUES (");
      Buffer.FormatCSValues(values, ColumnTypes);
      Buffer.SB.Append(')');

      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    #endregion

    #region AddRecords()

    /// <summary>
    /// Начать групповое добавление и/или изменение строк таблицы.
    /// После вызова метода и до вызова DBxDataWriter.Dispose() нельзя вызывать другие методы для текущего соединения.
    /// </summary>
    /// <param name="writerInfo">Параметры обработки</param>
    /// <returns>Объект для записи</returns>
    public DBxDataWriter CreateWriter(DBxDataWriterInfo writerInfo)
    {
      if (writerInfo == null)
        throw new ArgumentNullException("writerInfo");
      if (String.IsNullOrEmpty(writerInfo.TableName))
        throw new ArgumentException("DBxDataWriterInfo.TableName==null", "writerInfo");
      if (writerInfo.Columns == null)
        throw new ArgumentException("DBxDataWriterInfo.Columns==null", "writerInfo");
      if (writerInfo.Columns.Count == 0)
        throw new ArgumentException("DBxDataWriterInfo.Columns.Count=null", "writerInfo");

      writerInfo.SetReadOnly();
      return OnCreateWriter(writerInfo);
    }

    /// <summary>
    /// Создает объект класса, произодного от DBxDataWriter.
    /// Непереопределенный метод возвращает "медленный" объект DBxDefaultDataWriter.
    /// </summary>
    /// <param name="writerInfo">Параметры обработки</param>
    /// <returns>Объект для записи</returns>
    protected virtual DBxDataWriter OnCreateWriter(DBxDataWriterInfo writerInfo)
    {
      return new DBxDefaultDataWriter(this, writerInfo);
    }

    #region Для DataTable

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица может содержать, а может и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    public void AddRecords(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(table.TableName))
        throw new ArgumentException("У таблицы не задано свойство TableName. Следует использовать перегрузку метода AddRecords() с аргументом TableName", "table");

      AddRecords(table.TableName, table);
    }

    /// <summary>
    /// Групповое добавление записей (Bulk Copy) из таблицы данных.
    /// Если для базы данных не предусмотрена такая возможность, выполняется поштучное добавление записей
    /// Таблица может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    public void AddRecords(string tableName, DataTable table)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      for (int i = 0; i < table.Columns.Count; i++)
        Validator.CheckTableColumnName(tableName, table.Columns[i].ColumnName, false, DBxAccessMode.Full);

      if (table.Rows.Count == 0)
        return; // Нечего добавлять

      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        DoAddRecords(tableName, table);
        tr.Commit();
      }
    }

    /// <summary>
    /// Реализация группового добавления записей.
    /// Метод вызывается внутри транзакции.
    /// Непереопределенный метод вызывает AddRecord() для каждой строки таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    protected virtual void DoAddRecords(string tableName, DataTable table)
    {
      if (Buffer.Formatter.MaxInsertIntoValueRowCount > 1)
        DoAddRecordsMultiRows(tableName, table);
      else
      {
        DBxColumns columnNames = DBxColumns.FromColumns(table.Columns);
        foreach (DataRow srcRow in table.Rows)
          AddRecord(tableName, columnNames, srcRow.ItemArray);
      }
    }

    /// <summary>
    /// Реализация группового добавления записей с помощью INSERT INTO с заданием VALUES() для множества строк.
    /// Метод может вызываться из переопределенного DoAddRecords(), если база данных поддерживает такие запросы
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    private void DoAddRecordsMultiRows(string tableName, DataTable table)
    {
      DBxColumns columnNames = DBxColumns.FromColumns(table.Columns);
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);

      DBxSqlBuffer buffer2 = new DBxSqlBuffer(Buffer.Formatter); // для загрузки списка значений
      string delayedBuffer = null; // если на предыдущем такте стало слишком много команд
      object[] values = new object[columnNames.Count];

      int i1 = 0;
      while (i1 < table.Rows.Count)
      {
        Buffer.Clear();
        Buffer.SB.Append("INSERT INTO ");
        Buffer.FormatTableName(tableName); // 20.09.2019
        Buffer.SB.Append(" (");
        Buffer.FormatCSColumnNames(columnNames);
        Buffer.SB.Append(") VALUES ");

        int rowCount = Math.Min(Buffer.Formatter.MaxInsertIntoValueRowCount, table.Rows.Count - i1);
        int i2 = 0;
        while (i2 < rowCount)
        {
          if (delayedBuffer != null)
          {
            // На предыдущем цикле переполнился буфер
            Buffer.SB.Append(delayedBuffer);
            delayedBuffer = null;
            i1++;
            i2++;
            continue;
          }

          DataRow srcRow = table.Rows[i1]; // а не i1+i2

          for (int j = 0; j < values.Length; j++)
          {
            values[j] = srcRow[j];
            if (values[j] is DBNull)
              values[j] = null;
          }
          buffer2.Clear();
          buffer2.SB.Append("(");
          buffer2.FormatCSValues(values, columnTypes);
          buffer2.SB.Append(')');

          if (i2 > 0 && (Buffer.Formatter.MaxSqlLength - Buffer.SB.Length) < (buffer2.SB.Length + 1))
          {
            delayedBuffer = buffer2.SB.ToString(); // на следующем такте
            break;
          }

          if (i2 > 0)
            Buffer.SB.Append(",");
          Buffer.SB.Append(buffer2.SB.ToString());
          i1++;
          i2++;
        }

        SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    /// <summary>
    /// Групповое добавление записей (Bulk Copy) из нескольких таблиц данных.
    /// Если для базы данных не предусмотрена такая возможность, выполняется поштучное добавление записей
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public void AddRecords(DataSet ds)
    {
      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        foreach (DataTable table in ds.Tables)
        {
          Validator.CheckTableName(table.TableName, DBxAccessMode.Full);
          if (table.Rows.Count == 0)
            continue; // Нечего добавлять

          DoAddRecords(table.TableName, table);
        }

        tr.Commit();
      }
    }

    #endregion

    #region Для DbDataReader

    /// <summary>
    /// Групповое добавление записей (Bulk Copy) из открытого объекта DbDataReader.
    /// Если для базы данных не предусмотрена такая возможность, выполняется поштучное добавление записей
    /// Таблица может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="source">Открытая на чтение таблица данных, возможно, в другой базе данных</param>
    public void AddRecords(string tableName, DbDataReader source)
    {
      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        Validator.CheckTableName(tableName, DBxAccessMode.Full);
        DoAddRecords(tableName, source);

        tr.Commit();
      }
    }

    /// <summary>
    /// Реализация группового добавления строк.
    /// Метод вызывается из транзации
    /// Непереопределенный метод вызывает AddRecord() для каждой строки таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="source">Открытая на чтение таблица данных, возможно, в другой базе данных</param>
    protected virtual void DoAddRecords(string tableName, DbDataReader source)
    {
      if (Buffer.Formatter.MaxInsertIntoValueRowCount > 1)
        DoAddRecordsMultiRows(tableName, source);
      else
      {
        DBxColumns columnNames = DBxColumns.FromDataReader(source);
        object[] values = new object[columnNames.Count];
        while (source.Read())
        {
          for (int j = 0; j < values.Length; j++)
            values[j] = source[j];
          AddRecord(tableName, columnNames, values);
        }
      }
    }

    /// <summary>
    /// Реализация группового добавления записей с помощью INSERT INTO с заданием VALUES() для множества строк.
    /// Метод может вызываться из переопределенного DoAddRecords(), если база данных поддерживает такие запросы
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="source">Открытая на чтение таблица данных, возможно, в другой базе данных</param>
    private void DoAddRecordsMultiRows(string tableName, DbDataReader source)
    {
      DBxColumns columnNames = DBxColumns.FromDataReader(source);
      DBxColumnType[] columnTypes = Validator.CheckTableColumnNames(tableName, columnNames, false, DBxAccessMode.Full);

      DBxSqlBuffer buffer2 = new DBxSqlBuffer(Buffer.Formatter); // для загрузки списка значений
      string delayedBuffer = null; // если на предыдущем такте стало слишком много команд
      object[] values = new object[columnNames.Count];

      int rowCount = Buffer.Formatter.MaxInsertIntoValueRowCount;

      bool finished = false;

      while (!finished)
      {
        Buffer.Clear();
        Buffer.SB.Append("INSERT INTO ");
        Buffer.FormatTableName(tableName); // 20.09.2019
        Buffer.SB.Append(" (");
        Buffer.FormatCSColumnNames(columnNames);
        Buffer.SB.Append(") VALUES ");

        int i2 = 0;
        while (i2 < rowCount)
        {
          if (delayedBuffer != null)
          {
            // На предыдущем цикле переполнился буфер
            Buffer.SB.Append(delayedBuffer);
            delayedBuffer = null;
            i2++;
            continue;
          }

          finished = !source.Read();
          if (finished)
            break;

          for (int j = 0; j < values.Length; j++)
          {
            values[j] = source[j];
            if (values[j] is DBNull)
              values[j] = null;
          }

          buffer2.Clear();
          buffer2.SB.Append("(");
          buffer2.FormatCSValues(values, columnTypes);
          buffer2.SB.Append(')');

          if (i2 > 0 && (Buffer.Formatter.MaxSqlLength - Buffer.SB.Length) < (buffer2.SB.Length + 1))
          {
            delayedBuffer = buffer2.SB.ToString(); // на следующем такте
            break;
          }

          if (i2 > 0)
            Buffer.SB.Append(",");
          Buffer.SB.Append(buffer2.SB.ToString());
          i2++;
        }

        SQLExecuteNonQuery(Buffer.SB.ToString());
      } // while !Finished
    }

    #endregion

    #region Для IDictionary

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Пары можгут содержать, а могут и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValuesArray">Массив хэш-таблиц, по одной таблице в массмвк для каждой записи.
    /// Каждая хэш таблица содержит пары "ИмяПоля"-"Значение" для одной записи</param>
    public void AddRecords(string tableName, IDictionary[] columnNamesAndValuesArray)
    {
#if DEBUG
      if (columnNamesAndValuesArray == null)
        throw new ArgumentNullException("columnNamesAndValuesArray");
#endif

      if (columnNamesAndValuesArray.Length == 0)
        return;

      #region Структура таблицы

      DataTable table = new DataTable();
      for (int i = 0; i < columnNamesAndValuesArray.Length; i++)
      {
        if (columnNamesAndValuesArray[i] == null)
          throw new ArgumentNullException("ColumnNamesAndValuesArray[" + i.ToString() + "]");

        foreach (string columnName in columnNamesAndValuesArray[i].Keys)
        {
          if (!table.Columns.Contains(columnName))
          {
            object v = columnNamesAndValuesArray[i][columnName];
            if (v != null)
              table.Columns.Add(columnName, v.GetType());
          }
        }
      }

      #endregion

      #region Заполнение промежуточной таблицы

      for (int i = 0; i < columnNamesAndValuesArray.Length; i++)
      {
        DataRow row = table.Rows.Add();
        foreach (string columnName in columnNamesAndValuesArray[i].Keys)
        {
          object v = columnNamesAndValuesArray[i][columnName];
          if (v != null)
            row[columnName] = v;
        }
      }

      #endregion

      // Используем основной метод, принимающий DataTable
      AddRecords(tableName, table);
    }

    #endregion

    #endregion

    #region UpdateRecords()

    /// <summary>
    /// Обновление множества записей из таблицы данных.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    public void UpdateRecords(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (String.IsNullOrEmpty(table.TableName))
        throw new ArgumentException("У таблицы не задано свойство TableName. Следует использовать перегрузку метода UpdateRecords() с аргументом TableName", "table");

      UpdateRecords(table.TableName, table);
    }

    /// <summary>
    /// Групповое обновление записей из таблицы данных.
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы базы данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    public void UpdateRecords(string tableName, DataTable table)
    {
      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      if (table.Rows.Count == 0)
        return; // Нечего обновлять

      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        UpdateRecords2(tableName, table);

        tr.Commit();
      }
    }

    /// <summary>
    /// Позиции столбцов в таблицн DataTable, входящие в состав первичного ключа, и позиции прочих столбцов.
    /// Предполагается, что таблица содержит все столбцы, входящие в первичный ключ таблицы базы данных.
    /// Первичный ключ DataTable.PrimaryKey никак не учитывается.
    /// </summary>
    protected sealed class DataTableColumnPositions
    {
      #region Конструктор

      /// <summary>
      /// Заполняет позиции полей для таблицы
      /// </summary>
      /// <param name="ts">Описание структуры таблицы в базе данных</param>
      /// <param name="validator">Объект для проверки имен полей</param>
      /// <param name="table">Таблица исходных данных, для которой вычисляются позиции столбцов</param>
      public DataTableColumnPositions(DBxTableStruct ts, DBxNameValidator validator, DataTable table)
      {
        #region Проверка аргументов

#if DEBUG
        if (ts == null)
          throw new ArgumentNullException("ts");
        if (validator == null)
          throw new ArgumentNullException("validator");
        if (table == null)
          throw new ArgumentNullException("table");
#endif

        #endregion

        #region Первичный ключ

        this.PKColumnNames = ts.PrimaryKey;
        int[] pPKs = new int[this.PKColumnNames.Count];
        for (int i = 0; i < pPKs.Length; i++)
        {
          pPKs[i] = table.Columns.IndexOf(PKColumnNames[i]);
          if (pPKs[i] < 0)
            throw new ArgumentException("Таблица DataTable не содержит поля первичного ключа \"" + PKColumnNames[i] + "\", объявленного в таблице \"" + ts.TableName + "\" базы данных \"" + validator.Entry.DB.DatabaseName + "\"");
        }
        this.PKColumnPositions = pPKs;

        #endregion

        #region Другие столбцы

        string[] aOtherNames = new string[table.Columns.Count - pPKs.Length];
        int[] pOthers = new int[aOtherNames.Length];
        int cntCol = 0;
        for (int i = 0; i < table.Columns.Count; i++)
        {
          if (PKColumnNames.Contains(table.Columns[i].ColumnName))
            continue;
          aOtherNames[cntCol] = table.Columns[i].ColumnName;
          pOthers[cntCol] = i;
          cntCol++;
        }

        this.OtherColumnNames = new DBxColumns(aOtherNames);
        validator.CheckTableColumnNames(ts.TableName, OtherColumnNames, false, DBxAccessMode.Full);
        this.OrherColumnPositions = pOthers;

        #endregion

#if DEBUG
        if (PKColumnPositions.Length + OrherColumnPositions.Length != table.Columns.Count)
          throw new BugException("table.Columns.Count");
        if (cntCol != OtherColumnNames.Count)
          throw new BugException("cntCol");
#endif
      }

      #endregion

      #region Поля

      // Сделаны поля вместо свойств для ускорения доступа

      /// <summary>
      /// Имена столбцов, входящих в первичный ключ таблицы.
      /// Совпадает с DBxTableStruct.PrimaryKey.
      /// </summary>
      public readonly DBxColumns PKColumnNames;

      /// <summary>
      /// Позиции столбцов в DataTable, соответствующие PKColumnNames
      /// </summary>
      public readonly int[] PKColumnPositions;

      /// <summary>
      /// Имена столбцов, не входящих в первичный ключ таблицы.
      /// </summary>
      public readonly DBxColumns OtherColumnNames;

      /// <summary>
      /// Позиции столбцов в DataTable, соответствующие OtherColumnNames
      /// </summary>
      public readonly int[] OrherColumnPositions;

      #endregion

      #region Методы

      /// <summary>
      /// Создает фильтр ValueFilter или AndFilter для полей первичного ключа таблицы
      /// </summary>
      /// <param name="row">Строка исходных данных</param>
      /// <returns></returns>
      public DBxFilter CreatePKFilter(DataRow row)
      {
#if DEBUG
        if (row == null)
          throw new ArgumentNullException("row");
#endif
        object v;

        switch (PKColumnPositions.Length)
        {
          case 0:
            return null;

          case 1:
            v = row[PKColumnPositions[0]];
            if (v is DBNull)
              throw new ArgumentException("Строка данных имеет значение DBNull в поле первичного ключа \"" + PKColumnNames[0] + "\"", "row");
            return new ValueFilter(PKColumnNames[0], v);

          default:
            DBxFilter[] a = new DBxFilter[PKColumnNames.Count];
            for (int i = 0; i < a.Length; i++)
            {
              v = row[PKColumnPositions[i]];
              if (v is DBNull)
                throw new ArgumentException("Строка данных имеет значение DBNull в поле первичного ключа \"" + PKColumnNames[i] + "\"", "row");
              a[i] = new ValueFilter(PKColumnNames[i], v);
            }
            return new AndFilter(a);
        }
      }

      /// <summary>
      /// Заполняет массив значениями полей, не входязими в первичный кюлюч таблицы
      /// </summary>
      /// <param name="row">Строка исходных данных</param>
      /// <param name="values">Заполняемый массив</param>
      public void FillOtherValues(DataRow row, object[] values)
      {
#if DEBUG
        if (row == null)
          throw new ArgumentNullException("row");
        if (values.Length != OtherColumnNames.Count)
          throw new ArgumentException("Неправильная длина массива", "values");
#endif

        for (int i = 0; i < values.Length; i++)
        {
          values[i] = row[OrherColumnPositions[i]];
          if (values[i] is DBNull)
            values[i] = null;
        }
      }

      #endregion
    }


    private void UpdateRecords2(string tableName, DataTable table)
    {
      // 19.01.2020. Возможность использования составных и нечисловых первичных ключей

      DBxTableStruct ts = DB.Struct.Tables[tableName];
      if (ts.PrimaryKey.Count == 0)
        throw new InvalidOperationException("Таблица \"" + tableName + "\" в базе данных \"" + DB.DatabaseName + "\" не имеет первичного ключа. Для нее нельзя использовать операцию обновления строк");

      DataTableColumnPositions colPosInfo = new DataTableColumnPositions(ts, Validator, table);
      if (colPosInfo.OtherColumnNames.Count == 0)
        throw new ArgumentException("Таблица DataTable имеет только столбцы первичного ключа (" + colPosInfo.PKColumnNames.ToString() + ") и ни одного дополнительного столбца для обновления", "table");
      DoUpdateRecords(tableName, table, colPosInfo);
    }

    /// <summary>
    /// Реализация группового обновления записей из таблицы DataTable.
    /// Метод вызывается из транзакции.
    /// Непереопределенный метод вызывает SetValues() для каждой строки таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    /// <param name="colPosInfo">Информация о позициях столбцов в таблице <paramref name="table"/></param>
    protected virtual void DoUpdateRecords(string tableName, DataTable table, DataTableColumnPositions colPosInfo)
    {
      object[] values = new object[colPosInfo.OtherColumnNames.Count];

      foreach (DataRow srcRow in table.Rows)
      {
        DBxFilter filter = colPosInfo.CreatePKFilter(srcRow);
        colPosInfo.FillOtherValues(srcRow, values);

        SetValues(tableName, filter, colPosInfo.OtherColumnNames, values);
      }
    }

    /// <summary>
    /// Групповое обновление записей из нескольких таблиц данных.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>.
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public void UpdateRecords(DataSet ds)
    {
      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        foreach (DataTable table in ds.Tables)
        {
          Validator.CheckTableName(table.TableName, DBxAccessMode.Full);
          if (table.Rows.Count == 0)
            continue; // Нечего обновлять
          UpdateRecords2(table.TableName, table);
        }

        tr.Commit();
      }
    }

    #endregion

    #region AddOrUpdateRecords()

    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    public void AddOrUpdateRecords(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (String.IsNullOrEmpty(table.TableName))
        throw new ArgumentException("У таблицы не задано свойство TableName. Следует использовать перегрузку метода UpdateRecords() с аргументом TableName", "table");

      AddOrUpdateRecords(table.TableName, table);
    }

    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы в базе данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    public void AddOrUpdateRecords(string tableName, DataTable table)
    {
      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      if (table.Rows.Count == 0)
        return; // Нечего обновлять
      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        DoAddOrUpdateRecords(tableName, table);

        tr.Commit();
      }
    }

    private void DoAddOrUpdateRecords(string tableName, DataTable table)  // пока нет смысла делать виртуальным
    {
      // 19.01.2020
      // Возможность использования нечисловых и составных первичных ключей

#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      DBxTableStruct ts = DB.Struct.Tables[tableName];
      switch (ts.PrimaryKey.Count)
      {
        case 0:
          throw new InvalidOperationException("Таблица \"" + tableName + "\" в базе данных \"" + DB.DatabaseName + "\" не имеет первичного ключа. Обновление невозможно");
        case 1:
          DoAddOrUpdateRecordsSingleColumn(tableName, table, ts.PrimaryKey[0]);
          break;
        default:
          DoAddOrUpdateRecordsMultiColumns(tableName, table);
          break;
      }
    }

    private void DoAddOrUpdateRecordsSingleColumn(string tableName, DataTable table, string pkColumnName)  // пока нет смысла делать виртуальным
    {
      if (table.Rows.Count <= 500)
        DoAddOrUpdateRecordsSingleColumn2(tableName, table, pkColumnName);
      else
      {
        // Разбиваем таблицу на блоки
        for (int i = 0; i < table.Rows.Count; i += 500)
        {
          int n = Math.Min(table.Rows.Count - i, 500);
          DataTable table2 = table.Clone();
          for (int j = 0; j < n; j++)
            table2.Rows.Add(table.Rows[i + j].ItemArray);
          table2.AcceptChanges();
          DoAddOrUpdateRecordsSingleColumn2(tableName, table2, pkColumnName);
        }
      }
    }


    private void DoAddOrUpdateRecordsSingleColumn2(string tableName, DataTable table, string pkColumnName)
    {
      int pPK = table.Columns.IndexOf(pkColumnName);
      if (pPK < 0)
        throw new ArgumentException("Таблица " + table.TableName + " должна содержать поле первичного ключа \"" + pkColumnName + "\"", "table");
#if DEBUG
      if (table.Rows.Count < 1)
        throw new BugException("table.Rows.Count=0");
#endif

      object[] pkValues = new object[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
      {
        pkValues[i] = table.Rows[i][pPK]; // тут не может быть 
        if (pkValues[i] is DBNull)
          throw new ArgumentException("В поле \"" + pkColumnName + "\" не могут встречаться значения DBNull, так как это первичный ключ таблицы \"" + tableName + "\" базы данных \"" + DB.DatabaseName + "\"");
      }

      DBxColumnType pkColType = DBxColumnType.Unknown;
      DBxColumnStruct pkColumnDef = DB.Struct.FindColumn(tableName, pkColumnName);
      if (pkColumnDef != null) // на случай, если проверка структуры отключена
        pkColType = pkColumnDef.ColumnType;

      DataTable exTable = FillSelect(tableName,
        DB.Struct.Tables[tableName].PrimaryKey,
        new ValuesFilter(pkColumnName, pkValues, pkColType),
        null);
      if (exTable.Rows.Count == 0)
      {
        // Все строки новые
        DoAddRecords(tableName, table);
      }
      else if (exTable.Rows.Count == table.Rows.Count)
      {
        // Все строки существующие
        UpdateRecords2(tableName, table);
      }
      else
      {
        DataTools.SetPrimaryKey(exTable, pkColumnName);
        // Часть строк новые, а часть - старые
        DataTable newTable = table.Clone();
        DataTable updTable = table.Clone();
        foreach (DataRow SrcRow in table.Rows)
        {
          if (exTable.Rows.Find(SrcRow[pPK]) != null)
            updTable.Rows.Add(SrcRow.ItemArray);
          else
            newTable.Rows.Add(SrcRow.ItemArray);
        }

#if DEBUG
        if (updTable.Rows.Count == 0 || newTable.Rows.Count == 0)
          throw new BugException("Одна из таблиц пустая");
#endif

        UpdateRecords2(tableName, updTable);
        DoAddRecords(tableName, newTable);
      }
    }

    private void DoAddOrUpdateRecordsMultiColumns(string tableName, DataTable table)
    {
      // Возможен только поштучный перебор строк

      DBxTableStruct ts = DB.Struct.Tables[tableName];
      if (ts.PrimaryKey.Count == 0)
        throw new InvalidOperationException("Таблица \"" + tableName + "\" в базе данных \"" + DB.DatabaseName + "\" не имеет первичного ключа. Для нее нельзя использовать операцию обновления строк");
      DataTableColumnPositions colPosInfo = new DataTableColumnPositions(ts, Validator, table);
      if (colPosInfo.OtherColumnNames.Count == 0)
        throw new ArgumentException("Таблица DataTable имеет только столбцы первичного ключа (" + colPosInfo.PKColumnNames.ToString() + ") и ни одного дополнительного столбца для обновления", "table");

      DataTable newTable = null;
      object[] values = new object[colPosInfo.OrherColumnPositions.Length];

      foreach (DataRow srcRow in table.Rows)
      {
        DBxFilter Filter = colPosInfo.CreatePKFilter(srcRow);
        if (GetRecordCount(tableName, Filter) == 0)
        {
          if (newTable == null)
            newTable = table.Clone();
          newTable.Rows.Add(srcRow.ItemArray);
        }
        else
        {
          colPosInfo.FillOtherValues(srcRow, values);
          SetValues(tableName, Filter, colPosInfo.OtherColumnNames, values);
        }
      }

      if (newTable != null)
        DoAddRecords(tableName, newTable);
    }

    /// <summary>
    /// Обновление множества записей из нескольких таблиц данных и добавление недостающих.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор таблиц</param>
    public void AddOrUpdateRecords(DataSet ds)
    {
      using (DBxTransactionArray tr = new DBxTransactionArray(this))
      {
        foreach (DataTable table in ds.Tables)
        {
          Validator.CheckTableName(table.TableName, DBxAccessMode.Full);
          if (table.Rows.Count == 0)
            continue; // Нечего обновлять
          DoAddOrUpdateRecords(table.TableName, table);
        }

        tr.Commit();
      }
    }

    #endregion

    #endregion

    #region BeginUpdate()/EndUpdate()

    /// <summary>
    /// Коллекция объектов для блокирования таблиц с помощью Begin/EndUpdate().
    /// Ключ - имя таблицы
    /// Значение - объект, для которого вызывается Monitor.Enter() и Exit().
    /// Является частью основной коллекции, получаемой DBx.GetUpdateTableLockObject().
    /// </summary>
    private Dictionary<string, object> _UpdateTableLockObjects;

    /// <summary>
    /// Начать обновление таблицы.
    /// Этот метод используется только в FindOrAddRecord() и аналогичных классах обновления данных, которые
    /// не могут выполняться параллельно в нескольких соединениях.
    /// Этот метод не является потокобезопасным. Допускается вложенный вызов в том же потоке.
    /// В блоке finally должен быть парный вызов EndUpdate().
    /// Метод не предполагается для использования в пользовательском коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы, которая будет обновляться. Таблица должна быть доступна для записи</param>
    public void BeginUpdate(string tableName)
    {
      _Validator.CheckTableName(tableName, DBxAccessMode.Full);

      if (_UpdateTableLockObjects == null)
        _UpdateTableLockObjects = new Dictionary<string, object>();

      object lockObj;
      if (!_UpdateTableLockObjects.TryGetValue(tableName, out lockObj))
      {
        lockObj = _Entry.DB.GetUpdateTableLockObject(tableName);
        _UpdateTableLockObjects.Add(tableName, lockObj);
      }

      Monitor.Enter(lockObj);
    }

    /// <summary>
    /// Парный вызов для BeginUpdate()
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void EndUpdate(string tableName)
    {
      if (_UpdateTableLockObjects == null)
        throw new InvalidOperationException("Не было вызова BeginUpdate()");
      object lockObj;
      if (!_UpdateTableLockObjects.TryGetValue(tableName, out lockObj))
        throw new InvalidOperationException("Не было вызова BeginUpdate() для таблицы \"" + tableName + "\"");

      Monitor.Exit(lockObj);
    }

    #endregion

    #region Комбинированный поиск и добавление записи

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена и значения полей</param>
    /// <param name="id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    public bool FindOrAddRecord(string tableName, IDictionary columnNamesAndValues, out Int32 id)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      return FindOrAddRecord(tableName, new DBxColumns(columnNames), values, out id);
    }

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <param name="id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    public bool FindOrAddRecord(string tableName, DBxColumns columnNames, object[] values, out Int32 id)
    {
      bool added;

      BeginUpdate(tableName);
      try
      {
        added = DoFindOrAddRecord(tableName, columnNames, values, out id);
      }
      finally
      {
        EndUpdate(tableName);
      }
      return added;
    }

    /// <summary>
    /// Реализация поиска и добавления строки.
    /// Непереопределенный метод вызывает сначала FindRecord(), а затем - AddRecord().
    /// На момент вызова, выполнен вызов BeginUpdate()
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов для поиска</param>
    /// <param name="values">Значения полей</param>
    /// <param name="id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    protected virtual bool DoFindOrAddRecord(string tableName, DBxColumns columnNames, object[] values, out Int32 id)
    {
      id = FindRecord(tableName, columnNames, values);
      bool added = (id == 0);
      if (added)
        id = AddRecordWithIdResult(tableName, columnNames, values);

      return added;
    }

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена и значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    public Int32 FindOrAddRecord(string tableName, IDictionary columnNamesAndValues)
    {
      Int32 id;
      FindOrAddRecord(tableName, columnNamesAndValues, out id);
      return id;
    }

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    public Int32 FindOrAddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      Int32 id;
      FindOrAddRecord(tableName, columnNames, values, out id);
      return id;
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    public Int32[] FindOrAddRecords(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(table.TableName))
        throw new ArgumentException("У таблицы не задано свойство TableName. Следует использовать перегрузку метода FindOrAddRecords() с аргументом TableName", "table");

      return FindOrAddRecords(table.TableName, table);
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    public Int32[] FindOrAddRecords(string tableName, DataTable table)
    {
      _Validator.CheckTableName(tableName, DBxAccessMode.Full);
      string IdColumnName = _Validator.CheckTablePrimaryKeyInt32(tableName);

#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.Columns.Count == 0)
        throw new ArgumentException("Таблица не содержит столбцов", "table");

      if (table.Columns.Contains(IdColumnName))
        throw new ArgumentException("Таблица со строками для поиска не может содержать столбца \"" + IdColumnName + "\" первичного ключа таблицы \"" + tableName + "\"");

      for (int j = 0; j < table.Columns.Count; j++)
        _Validator.CheckTableColumnName(tableName, table.Columns[j].ColumnName, false, DBxAccessMode.Full);

      if (table.Rows.Count == 0)
        return DataTools.EmptyIds;

      Int32[] ids = new Int32[table.Rows.Count];

      BeginUpdate(tableName);
      try
      {
        DoFindOrAddRecords(tableName, table, ids);
      }
      finally
      {
        EndUpdate(tableName);
      }

      return ids;
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// На момент вызова вызван BeginUpdate().
    /// Непереопределенный метод вызывает DoFindOrAddRecordsSingleColumn() или DoFindOrAddRecordsMultiColumns(),
    /// в зависимости от числа столбцов в таблице <paramref name="table"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не должен меняться</param>
    /// <param name="Ids">Сюда должны записываться идентификаторы строк</param>
    protected virtual void DoFindOrAddRecords(string tableName, DataTable table, Int32[] Ids)
    {
      if (table.Columns.Count == 1)
        DoFindOrAddRecordsSingleColumn(tableName, table, Ids);
      else
        DoFindOrAddRecordsMultiColumns(tableName, table, Ids);
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Выполняет поиск строк в таблице с помощью фильтра "WHERE имя-поля IN (список-значений)".
    /// Затем для недостающих строк выполняется групповое добавление записей с помощью AddRecords
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не должен меняться</param>
    /// <param name="ids">Сюда должны записываться идентификаторы строк</param>
    protected void DoFindOrAddRecordsSingleColumn(string tableName, DataTable table, Int32[] ids)
    {
      string idColumnName = _Validator.CheckTablePrimaryKeyInt32(tableName);

      if (table.Columns.Count != 1)
        throw new ArgumentException("Таблица должна иметь только один столбец", "table");

      DBxColumnType colType = DBxColumnType.Unknown;
      DBxColumnStruct colDef = DB.Struct.FindColumn(tableName, table.Columns[0].ColumnName);
      if (colDef != null)
        colType = colDef.ColumnType;

      #region Загрузка существующих записей

      // Поиск выполняем блоками по 100 строк, чтобы не получился слишком длинный фильтр
      DataTable midTable = null;
      for (int i = 0; i < table.Rows.Count; i += 100)
      {
        int n = Math.Min(table.Rows.Count - i, 100);
        object[] values = new object[n];
        for (int i2 = 0; i2 < n; i2++)
          values[i2] = table.Rows[i + i2][0];

        DBxSelectInfo info = new DBxSelectInfo();
        info.TableName = tableName;
        info.Expressions.Add(new DBxColumn(idColumnName));
        info.Expressions.Add(new DBxColumn(table.Columns[0].ColumnName));
        info.Where = new ValuesFilter(table.Columns[0].ColumnName, values, colType);
        DataTable MidTable2 = FillSelect(info);
        if (midTable == null)
          midTable = MidTable2;
        else
          midTable.Merge(MidTable2);
      }

      midTable.AcceptChanges();

      #endregion

      DataTable newTable = null;
      Int32 lastId = 0;

      #region Поиск

      midTable.DefaultView.Sort = table.Columns[0].ColumnName;
      for (int i = 0; i < table.Rows.Count; i++)
      {
        int p = midTable.DefaultView.Find(table.Rows[i][0]);
        if (p >= 0)
          // Нашли
          ids[i] = DataTools.GetInt(midTable.DefaultView[p].Row[0]);
        else
        {
          if (newTable == null)
          {
            newTable = midTable.Clone(); // Два столбца
            lastId = DataTools.GetInt(GetMaxValue(tableName, idColumnName, null));
          }

          checked { lastId++; }
          newTable.Rows.Add(lastId, table.Rows[i][0]);

          ids[i] = lastId;
        }
      }

      #endregion

      #region Добавление записей

      if (newTable != null)
        DoAddRecords(tableName, newTable);

      #endregion
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Вызывает DoFindOrAddRecord() для каждой строки таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не должен меняться</param>
    /// <param name="ids">Сюда должны записываться идентификаторы строк</param>
    protected void DoFindOrAddRecordsMultiColumns(string tableName, DataTable table, Int32[] ids)
    {
      DBxColumns columns = DBxColumns.FromColumns(table.Columns);
      for (int i = 0; i < table.Rows.Count; i++)
      {
        Int32 id;
        DoFindOrAddRecord(tableName, columns, table.Rows[i].ItemArray, out id);
        ids[i] = id;
      }
    }

    #endregion

    #region Удаление записи

    /// <summary>
    /// Удаление одной строки таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    public void Delete(string tableName, Int32 id)
    {
      Validator.CheckTablePrimaryKeyInt32(tableName);
      Delete(tableName, new IdsFilter(DB.Struct.Tables[tableName].PrimaryKey[0], id));
    }

    /// <summary>
    /// Удаление строк. Выполняет SQL-запрос "DELETE".
    /// Усливие <paramref name="where"/> должно быть задано. Для удаления всех строк таблицы используйте метод DeleteAll().
    /// Ссылочные поля (с точками) не поддерживаются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условия отбора строк. Не может быть null</param>
    public virtual void Delete(string tableName, DBxFilter where)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);

      if (where == null)
        throw new ArgumentNullException("where", "Удаление всех строк таблицы не поддерживается. Используйте метод DeleteAll");
      Validator.CheckFilterColumnNames(tableName, where, false);

      Buffer.SB.Append("DELETE FROM ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatFilter(where);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }

    /// <summary>
    /// Удаление всех строк таблицы. Выполняет SQL-запрос "DELETE" без условия "WHERE".
    /// </summary>
    /// <param name="tableName">Имя очищаемой таблицы</param>
    public virtual void DeleteAll(string tableName)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.Full);

      Buffer.SB.Append("DELETE FROM ");
      Buffer.FormatTableName(tableName);
      SQLExecuteNonQuery(Buffer.SB.ToString());
    }


    #endregion

    #region BLOB-поля

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, значение поля устанавливается для всех из них.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    public virtual void WriteBlob(string tableName, DBxFilter where, string columnName, byte[] value)
    {
      Buffer.Clear();

      Validator.CheckTableName(tableName, DBxAccessMode.Full);
      Validator.CheckTableColumnName(tableName, columnName, false, DBxAccessMode.Full);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);
      else
        throw new ArgumentNullException("where", "Фильтр по строкам должен быть задан");

      Buffer.SB.Append("UPDATE ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" SET ");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append("=");
      if (value == null)
        Buffer.SB.Append("NULL");
      else
        Buffer.FormatParamPlaceholder(0);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatFilter(where);

      if (value == null)
        SQLExecuteNonQuery(Buffer.SB.ToString());
      else
        SQLExecuteNonQuery(Buffer.SB.ToString(), new object[] { value });
    }

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    public void WriteBlob(string tableName, Int32 id, string columnName, byte[] value)
    {
      string idColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);
      if (id == 0)
        throw new DBxNoIdArgumentException("Не задан идентификатор записи", "id");

      WriteBlob(tableName, new IdsFilter(idColumnName, id), columnName, value);
    }

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// 
    /// Если нет ни одной строки, удовлетворяющей условию фильтра, то возвращается null.
    /// Null также возвращается, если в найденной строке поле имеет значение NULL.
    /// Используйте перегрузку, возвразающую значение Value по ссылке, чтобы различить эти ситуации.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    public byte[] ReadBlob(string tableName, DBxFilter where, string columnName)
    {
      byte[] value;
      ReadBlob(tableName, where, columnName, out value);
      return value;
    }

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    public byte[] ReadBlob(string tableName, Int32 id, string columnName)
    {
      string idColumnName = Validator.CheckTablePrimaryKeyInt32(tableName);

      if (id == 0)
        return null;

      byte[] value;
      if (ReadBlob(tableName, new IdsFilter(idColumnName, id), columnName, out value))
        return value;
      else
        throw new DBxRecordNotFoundException(tableName, id.ToString());
    }


    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Значение поля возвращается по ссылке <paramref name="value"/>. Если строка не найдена или
    /// поле содержит значение NULL, по ссылке передается null.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// Эта перегрузка метода позволяет определить наличие или отсутствие строки, удовлетворяющей фильтру.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Сюда записывается значение поля или null</param>
    /// <returns>True, если найдена строка, удовлетворяющая условию <paramref name="where"/>.
    /// False, если строка не найдена</returns>
    public virtual bool ReadBlob(string tableName, DBxFilter where, string columnName, out byte[] value)
    {
      Buffer.Clear();
      Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      Validator.CheckTableColumnName(tableName, columnName, false, DBxAccessMode.ReadOnly);
      if (where != null)
        Validator.CheckFilterColumnNames(tableName, where, false);
      else
        throw new ArgumentNullException("where", "Фильтр по строкам должен быть задан");

      bool res;

      Buffer.SB.Append("SELECT ");
      Buffer.FormatColumnName(columnName);
      Buffer.SB.Append(" FROM ");
      Buffer.FormatTableName(tableName);
      Buffer.SB.Append(" WHERE ");
      Buffer.FormatFilter(where);

      DbDataReader rdr = SQLExecuteReader(Buffer.SB.ToString());

      try
      {
        if (!rdr.Read())
        {
          res = false;
          value = null;
        }
        else
        {
          res = true;
          //Type ft = Reader.GetFieldType(0);
          object obj = rdr.GetValue(0);
          if (obj is byte[])
            value = (byte[])obj;
          else
            value = null;
        }
      }
      finally
      {
        rdr.Close();
      }
      return res;
    }

    #endregion

    #endregion

    #region Внутренняя реализация выполнения запросов

    ///// <summary>
    ///// Генерирует исключение, если текущий поток не совпадает с тем, в котором был вызван конструктор объекта
    ///// </summary>
    //public void CheckThread()
    //{
    //  if (Thread.CurrentThread != _Thread)
    //    throw new DifferentThreadException();
    //}

    /// <summary>
    /// Выполнение SQL-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Возвращаемое значение</returns>
    public object SQLExecuteScalar(string cmdText)
    {
      return SQLExecuteScalar(cmdText, null);
    }

    /// <summary>
    /// Выполнение SQL-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров</param>
    /// <returns>Возвращаемое значение</returns>
    public object SQLExecuteScalar(string cmdText, object[] paramValues)
    {
      CheckNotDisposed();

      // 09.12.2019
      // Проверка убрана.
      // Объект DBxCon может создаваться на стороне клиента с помощью Remoting. 
      // При этом не гарантируется, что методы объекта будут вызываться в том же потоке
      // CheckThread();

      //???Caller.OnBeforeActivity();

      DBxConQueryInfo ti = TraceSqlBegin(cmdText);

      object res = null;
      try
      {
        res = DoSQLExecuteScalar(cmdText, paramValues);
      }
      catch (ThreadAbortException e)
      {
        TraceSqlEnd(ti, e);
        throw;
      }
      catch (Exception e)
      {
        TraceSqlEnd(ti, e);
        ThrowSqlException(cmdText, e);
      }

      TraceSqlEnd(ti, null);

      return res;
    }

    /// <summary>
    /// Выполнение запроса, не возвращающего значение
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    public void SQLExecuteNonQuery(string cmdText)
    {
      SQLExecuteNonQuery(cmdText, null);
    }
    /// <summary>
    /// Выполнение SQL-запроса, не возвращающего значения
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    public void SQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      CheckNotDisposed();
      //???Caller.OnBeforeActivity();

      DBxConQueryInfo ti = TraceSqlBegin(cmdText);

      try
      {
        DoSQLExecuteNonQuery(cmdText, paramValues);
      }
      catch (ThreadAbortException e)
      {
        TraceSqlEnd(ti, e);
        throw;
      }
      catch (Exception e)
      {
        TraceSqlEnd(ti, e);
        ThrowSqlException(cmdText, e);
      }

      TraceSqlEnd(ti, null);
    }


    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable без имени
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText)
    {
      return SQLExecuteDataTable(cmdText, String.Empty, null);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable с указанным именем
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="tableName">Имя создаваемой таблицы</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText, string tableName)
    {
      return SQLExecuteDataTable(cmdText, tableName, null);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable с указанным именем
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="tableName">Имя создаваемой таблицы</param>
    /// <param name="paramValues">Значения параметров</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText, string tableName, object[] paramValues)
    {
      CheckNotDisposed();
      //???Caller.OnBeforeActivity();

      DataTable table = null;

      DBxConQueryInfo ti = TraceSqlBegin(cmdText);

      try
      {
        table = DoSQLExecuteDataTable(cmdText, tableName, paramValues);
      }
      catch (ThreadAbortException e)
      {
        TraceSqlEnd(ti, e);
        throw;
      }
      catch (Exception e)
      {
        TraceSqlEnd(ti, e);
        ThrowSqlException(cmdText, e);
      }

      TraceSqlEnd(ti, null);

      // При передаче клиенту предотвращаем преобразование времени из-за разных часовых поясов
      SerializationTools.SetUnspecifiedDateTimeMode(table);
      return table;
    }

    /// <summary>
    /// Этот метод следует присоединить как обработчик события DataAdapter.FillError
    /// для добавлению к исключению дополнительной отладочной информации
    /// </summary>
    /// <param name="sender">Объект DataAdapter</param>
    /// <param name="args">Аргументы события FillError</param>
    protected void HandleDataAdapterFillError(object sender, FillErrorEventArgs args)
    {
      args.Errors.Data["DataAdapter.FillError.Values"] = args.Values;
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат не извлекается,
    /// а возвращается DataReader. По окончании работы должен быть вызван метод
    /// Close(). До этого не разрешается выполнять другие запросы
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader SQLExecuteReader(string cmdText)
    {
      return SQLExecuteReader(cmdText, null);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат не извлекается,
    /// а возвращается DataReader. По окончании работы должен быть вызван метод
    /// Close(). До этого не разрешается выполнять другие запросы
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader SQLExecuteReader(string cmdText, object[] paramValues)
    {
      CheckNotDisposed();

      DbDataReader rdr = null; // чтобы не было ошибки компиляции

      DBxConQueryInfo ti = TraceSqlBegin(cmdText);

      try
      {
        rdr = DoSQLExecuteReader(cmdText, paramValues);
      }
      catch (ThreadAbortException e)
      {
        TraceSqlEnd(ti, e);
        throw;
      }
      catch (Exception e)
      {
        TraceSqlEnd(ti, e);
        ThrowSqlException(cmdText, e);
      }

      TraceSqlEnd(ti, null);

      return rdr;
    }

    /// <summary>
    /// Перевыброс исключения при выполнении SQL-запроса.
    /// Метод вызывается из блоков catch в методах SQLExecuteXXX().
    /// Вызывает метод DBx.OnCreateSqlException для вызова пользовательского обработчика события CreateSqlException.
    /// Затем, если установлено свойство LogoutSqlExceptions, вызывается метод LogoutException().
    /// Наконец, выбрасывается исключение, которое, возможно было заменено в CreateSqlException.
    /// </summary>
    /// <param name="cmdText">Текст SQLзапроса, при выполнении которого возникло исключение</param>
    /// <param name="innerException">Возникшее исключение</param>
    protected void ThrowSqlException(string cmdText, Exception innerException)
    {
      DBxCreateSqlExceptionEventArgs args = new DBxCreateSqlExceptionEventArgs(this, cmdText, innerException);
      DB.OnCreateSqlException(args);

      try
      {
        if (this.DbConnection != null)
        {
          args.Exception.Data["DbConnection.State"] = DbConnection.State;
        }
      }
      catch { }

      if (LogoutSqlExceptions)
        LogoutException(args.Exception, "Ошибка выполнения запроса");
      throw args.Exception;
    }

    /// <summary>
    /// Вызывает DBx.OnLogoutException()
    /// </summary>
    /// <param name="exception">Исключение</param>
    /// <param name="title">Описание</param>
    protected void LogoutException(Exception exception, string title)
    {
      DBxLogoutExceptionEventArgs args = new DBxLogoutExceptionEventArgs(this, exception, title);
      DB.OnLogoutException(args);
    }

    #endregion

    #region Абстрактные методы и свойства

    /// <summary>
    /// Получить полный список таблиц из схемы базы данных
    /// </summary>
    /// <returns>Список таблиц</returns>
    internal protected abstract string[] GetAllTableNamesFromSchema();

    /// <summary>
    /// Получить реальную структуру таблицы из схемы базы данных
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Описание реальной структуры таблицы</returns>
    internal protected abstract DBxTableStruct GetRealTableStructFromSchema(string tableName);

    /// <summary>
    /// Возвращает (нетипизированное) соединение ADO.NET.
    /// Объект создается при первом обращении к свойству.
    /// </summary>
    protected abstract DbConnection DbConnection { get; }

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего таблицу данных
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="tableName">Имя таблицы для возвращаемого DataTable</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Заполненная таблица</returns>
    protected abstract DataTable DoSQLExecuteDataTable(string cmdText, string tableName, object[] paramValues);

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего DbDataReader
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Объект для чтения данных</returns>
    protected abstract DbDataReader DoSQLExecuteReader(string cmdText, object[] paramValues);

    /// <summary>
    /// Абстрактный метод выполнения SLQ-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">Текст SQL-запроса</param>
    /// <param name="paramValues">Параметры запроса</param>
    /// <returns>Значение, возвращаемое запросом</returns>
    protected abstract object DoSQLExecuteScalar(string cmdText, object[] paramValues);

    /// <summary>
    /// Выполнение SQL-запроса, не возвращающего значения
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    protected abstract void DoSQLExecuteNonQuery(string cmdText, object[] paramValues);

    /*
    /// <summary>
    /// Абстрактный метод запуска транзакции
    /// </summary>
    /// <returns>Интерфейс объекта транзакции</returns>
    protected abstract IDbTransaction DoBeginTransaction();
     */

    /// <summary>
    /// Вызывает статический метод ClearPool(), если он определен для провайдера (например, SqlConnection.ClearPool())
    /// </summary>
    public virtual void ClearPool()
    {
      _ClearPoolCalled = true;
    }

    /// <summary>
    /// Возвращает true, если был вызов ClearPool()
    /// </summary>
    public bool ClearPoolCalled { get { return _ClearPoolCalled; } }
    private bool _ClearPoolCalled;

    #endregion

    #region Методы поиска

    /// <summary>
    /// Возвращает true, если среди переданного списка столбцов есть ключевое поле
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <returns>Наличие поля</returns>
    protected bool ContainsKeyColumn(string tableName, DBxColumns columnNames)
    {
      DBxTableStruct ts = DB.Struct.Tables[tableName];
      if (ts == null)
        return false;
      return columnNames.ContainsAny(ts.PrimaryKey);
    }

    /// <summary>
    /// Возвращает true, если среди переданного списка столбцов есть ключевое поле
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columns">Список полей</param>
    /// <returns>Наличие поля</returns>
    protected bool ContainsKeyColumn(string tableName, DataColumnCollection columns)
    {
      DBxTableStruct ts = DB.Struct.Tables[tableName];
      if (ts == null)
        return false;
      for (int i = 0; i < ts.PrimaryKey.Count; i++)
      {
        if (columns.Contains(ts.PrimaryKey[i]))
          return true;
      }
      return false;
    }

    #endregion

    #region Проверочные методы

    /// <summary>
    /// Объект, выполняющий проверку корректности имен таблиц и полей и их наличичие в структуре базы данных.
    /// Выполняет заполнение списка DBxSqlBuffer.ColumnStructs.
    /// </summary>
    public DBxNameValidator Validator { get { return _Validator; } }
    private DBxNameValidator _Validator;

    /// <summary>
    /// Если свойство установлено (по умолчанию), то выполняется проверка существования
    /// описаний таблиц и полей в реальной структцры таблицы.
    /// Если свойство сброшено в false, проверяется только общая корректность имен (на наличие недопустимых символов)
    /// Это свойство не дублируется в основном соединении DBxCon. Следовательно, проверка может быть отключена
    /// только на стороне сервера (безопасность).
    /// Свойство дублирует DBxNameValidator.NameCheckingEnabled.
    /// </summary>
    public bool NameCheckingEnabled
    {
      get { return _Validator.NameCheckingEnabled; }
      set { _Validator.NameCheckingEnabled = value; }
    }


    #endregion

    #region Транзакции

    /*
     * Одно соединение поддерживает только одну транзакцию одновременно
     * Порядок вызова функций:
     * TransactionBegin();
     * try
     * {
     *   ... // Выполнение нескольких SQL-запросов
     * 
     *   TransactionCommit();
     * }
     * catch
     * {
     *   TransactionRollback();
     * }
     * 
     */

    /// <summary>
    /// Текущая транзакция, если был вызов метода TransactionBegin(), или null, если нет активной транзакции
    /// </summary>
    public IDbTransaction CurrentTransaction { get { return _CurrentTransaction; } }
    private IDbTransaction _CurrentTransaction;

    /// <summary>
    /// Обрабатывается ли сейчас транзакция ?
    /// </summary>
    public bool InsideTransaction { get { return _CurrentTransaction != null; } }

#if DEBUG
    //    /// <summary>
    //    /// Если используется отладка - то идентификатор транзакции
    //    /// </summary>
    // TODO:   private int CurrentTransactionId;
    // TODO:   private DateTime CurrentTransactionStartTime;

#endif

    /// <summary>
    /// Начать транзакцию
    /// </summary>
    public void TransactionBegin()
    {
      CheckNotDisposed();

      if (_CurrentTransaction != null)
        throw new InvalidOperationException("Предыдущая транзакция не была завершена");

#if DEBUG
      //lock (LogSyncRoot)
      //{
      //  if (LogStream != null)
      //  {
      //    LogId++;
      //    CurrentTransactionId = LogId;
      //    CurrentTransactionStartTime = DateTime.Now;
      //    LogStream.WriteLine();
      //    LogTimeAndDB();
      //    LogStream.WriteLine("Начата транзакция " + CurrentTransactionId.ToString());
      //    LogStream.Flush();
      //  }
      //  else
      //    CurrentTransactionId = 0;
      //}
#endif

      //FCurrentTransaction = DoBeginTransaction();
      _CurrentTransaction = DbConnection.BeginTransaction(); // 14.02.2019
    }

    /// <summary>
    /// Завершение транзакции с сохранением изменений в базе данных
    /// </summary>
    public void TransactionCommit()
    {
      if (_CurrentTransaction == null)
        throw new InvalidOperationException("Транзакция не была начата");
      try
      {
        _CurrentTransaction.Commit();
      }
      finally
      {
        _CurrentTransaction = null;
      }

#if DEBUG
      //lock (LogSyncRoot)
      //{
      //  if (LogStream != null)
      //  {
      //    LogStream.WriteLine();
      //    LogTimeAndDB();
      //    LogStream.WriteLine("Завершена транзакция " + CurrentTransactionId.ToString());
      //    if (CurrentTransactionId != 0)
      //      LogTimeSpan(CurrentTransactionStartTime);
      //    LogStream.Flush();
      //  }
      //  else
      //    CurrentTransactionId = 0;
      //}
#endif

    }

    /// <summary>
    /// Откат транзакции
    /// </summary>
    public void TransactionRollback()
    {
      if (_CurrentTransaction == null)
        return;
      try
      {
        _CurrentTransaction.Rollback();
      }
      catch (Exception e)
      {
        LogoutException(e, "Ошибка отката транзакции в БД: " + ToString());
      }

      _CurrentTransaction = null;

#if DEBUG
      //lock (LogSyncRoot)
      //{
      //  if (LogStream != null)
      //  {
      //    LogStream.WriteLine();
      //    LogTimeAndDB();
      //    LogStream.WriteLine("Откат транзакции " + CurrentTransactionId.ToString());
      //    if (CurrentTransactionId != 0)
      //      LogTimeSpan(CurrentTransactionStartTime);
      //    LogStream.Flush();
      //  }
      //  else
      //    CurrentTransactionId = 0;
      //}
#endif
    }

    #endregion

    #region Трассировка SQL-запросов

    /// <summary>
    /// Управление запроса на уровне соединения.
    /// Если свойство не установлено в явном виде для этого соединения, возвращается значение DBx.TraceEnabled
    /// </summary>
    public bool TraceEnabled { get { return _TraceEnabled ?? DB.TraceEnabled; } set { _TraceEnabled = value; } }
    private bool? _TraceEnabled;

    /// <summary>
    /// Восстановление значения по умолчанию для TraceEnabled
    /// </summary>
    public void ResetTraceEnabled()
    {
      _TraceEnabled = null;
    }

    /// <summary>
    /// Возможность временной приостановки трассировки.
    /// Например, в SQL Server надо выполнять оператор "USE [БазаДанных]" при создании соединения.
    /// Оно может надоедать при трассировке.
    /// После выполнения оператора, следуеть вызывать ResumeTrace()
    /// </summary>
    protected void SuspendTrace()
    {
      TraceSuspendCount++;
    }

    /// <summary>
    /// Парный метод для SuspendTrace()
    /// </summary>
    protected void ResumeTrace()
    {
      TraceSuspendCount--;
    }

    int TraceSuspendCount;


    /// <summary>
    /// Отладочная информация о выполняющемся сейчас запросе
    /// </summary>
    public DBxConQueryInfo CurrentQuery { get { return _CurrentQuery; } }
    private DBxConQueryInfo _CurrentQuery; // блокировка не нужна, т.к. операции с указателем всегда выполняются атомарно.

    private DBxConQueryInfo TraceSqlBegin(string cmdText)
    {
      DBxConQueryInfo ti = new DBxConQueryInfo(cmdText, TraceEnabled && TraceSuspendCount == 0, _CurrentQuery);
      _CurrentQuery = ti;

      DBxSqlQueryStartedEventArgs args = new DBxSqlQueryStartedEventArgs(cmdText);
      DB.OnSqlQueryStarted(args);

      return ti;
    }

    private void TraceSqlEnd(DBxConQueryInfo ti, Exception e)
    {
      _CurrentQuery = ti.PrevInfo;

      DBxSqlQueryFinishedEventArgs args = new DBxSqlQueryFinishedEventArgs(ti.CmdText, ti.ExecutingTime, e);
      DB.OnQueryFinished(args);

      if (ti.TraceEnabled)
      {
        System.Diagnostics.Trace.WriteLine("SQL: " + ti.CmdText);
        System.Diagnostics.Trace.IndentLevel++;
        StringBuilder sb = new StringBuilder();
        if (e == null)
          sb.Append("OK. ");
        else
          sb.Append("Failed. ");
        sb.Append("Database: ");
        sb.Append(DB.DisplayName);
        if (e == null)
        {
          sb.Append(" Time=");
          sb.Append(ti.ExecutingTime.ToString());
        }
        System.Diagnostics.Trace.WriteLine(sb.ToString());
        if (e != null)
        {
          System.Diagnostics.Trace.IndentLevel++;
          System.Diagnostics.Trace.WriteLine("Exception: " + e.GetType().ToString());
          System.Diagnostics.Trace.WriteLine("Message  : " + e.Message);
          System.Diagnostics.Trace.IndentLevel--;
        }
        System.Diagnostics.Trace.IndentLevel--;
      }
    }

    #endregion

    #region Копирование свойств

    /// <summary>
    /// Копирует свойства текущего соединения в другое соединение
    /// </summary>
    /// <param name="dest">Соединение, свойства которого устанавливаются</param>
    public virtual void CopyTo(DBxConBase dest)
    {
      if (dest == null)
        throw new ArgumentNullException("Dest");

      dest.NameCheckingEnabled = this.NameCheckingEnabled;
      dest.LogoutSqlExceptions = this.LogoutSqlExceptions;
      dest.TrimValues = this.TrimValues;
      dest.CommandTimeout = this.CommandTimeout;
    }

    #endregion

    #region Информация о базе данных

    /// <summary>
    /// Источник для создания структуры базы данных
    /// </summary>
    IDBxStructSource IDBxCon.StructSource { get { return new DBxRealStructSource(Entry); } }

    #endregion

    #region Создание объектов, привязанных к соединению DbConnection

    /// <summary>
    /// Создает команду, привязанную к соединению
    /// </summary>
    /// <returns></returns>
    public DbCommand CreateDbCommand()
    {
      DbCommand cmd = DB.ProviderFactory.CreateCommand();
      cmd.Connection = this.DbConnection;
      return cmd;
    }


    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает пустую таблицу с заданными столбцами.
    /// Всегда создается новый объект DataTable.
    /// Можно использовать ссылочные столбцы, содержащие ".".
    /// Вызывает DBxStruct.CreateDataTable().
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов. Если null, то возвращаются все столбцы, определенные для таблицы</param>
    /// <returns>Пустая таблица</returns>
    public DataTable CreateEmptyTable(string tableName, DBxColumns columnNames)
    {
      return DB.Struct.CreateDataTable(tableName, columnNames);
    }

    #endregion
  }
}
