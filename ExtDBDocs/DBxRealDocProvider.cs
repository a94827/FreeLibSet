// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using FreeLibSet.IO;
using FreeLibSet.Remoting;
using FreeLibSet.Config;
using System.Diagnostics;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Основная реализация провайдера на сервере или в монолитном приложении.
  /// Класс является потокобезопасным, но можно ограничить искусственно использование только в потоке, в котором вызван конструктор.
  /// </summary>
  public class DBxRealDocProvider : DBxDocProvider
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// Для <paramref name="source"/> вызывается SetReadOnly()
    /// </summary>
    /// <param name="source">Источник с назначенными правами пользователя</param>
    /// <param name="userId">Идентификатор пользователя. Может быть 0</param>
    /// <param name="currentThreadOnly">Если true, то провайдер будет использоваться только из текущего потока</param>
    public DBxRealDocProvider(DBxRealDocProviderSource source, Int32 userId, bool currentThreadOnly)
      : this(source, userId, currentThreadOnly, 0)
    {
    }

    /// <summary>
    /// Создает провайдер.
    /// Для <paramref name="source"/> вызывается SetReadOnly()
    /// </summary>
    /// <param name="source">Источник с назначенными правами пользователя</param>
    /// <param name="userId">Идентификатор пользователя. Может быть 0</param>
    /// <param name="currentThreadOnly">Если true, то провайдер будет использоваться только из текущего потока</param>
    /// <param name="sessionId">Идентификатор сессии клиента. Если отслеживание сессий не
    /// используется или неприменимо (провайдер для автоматически создаваемых документов),
    /// должно быть задано нулевое значение</param>
    public DBxRealDocProvider(DBxRealDocProviderSource source, Int32 userId, bool currentThreadOnly, Int32 sessionId)
      : base(InitFixedInfo(source, userId, sessionId), currentThreadOnly)
    {
      _Source = source;

#if DEBUG
      if (userId != 0)
      {
        CheckIsRealDocId(userId);
        if (!source.GlobalData.DocTypes.UseUsers)
          throw new ArgumentException("UserId не может быть задан, т.к. DBxDocTypes.UseUsers=false", "userId");
      }
#endif

#if DEBUG
      _DebugGuid = Guid.NewGuid();

      if (DBxDocProvider.DebugDocProviderList != null)
        Trace.WriteLine(DateTime.Now.ToString("G") + ", " + GetType().ToString() + ", GUID=" + _DebugGuid.ToString() + " created.");
#endif
    }

    private static NamedValues InitFixedInfo(DBxRealDocProviderSource source, Int32 userId, Int32 sessionId)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      source.SetReadOnly();

      NamedValues FixedInfo = new NamedValues();

      FixedInfo["DocTypes"] = source.GlobalData.DocTypes;

      FixedInfo["UserId"] = userId;
      FixedInfo["SessionId"] = sessionId;

      FixedInfo["DBIdentity"] = source.GlobalData.DBIdentity;

      FixedInfo["BinDataInfo"] = new DBxBinDataHandlerInfo(source.GlobalData.BinDataHandler);

      FixedInfo["UseDocHist"] = source.GlobalData.UndoDBEntry != null;

      FixedInfo["MainDocTableServiceColumns"] = source.GlobalData.MainDocTableServiceColumns;
      FixedInfo["SubDocTableServiceColumns"] = source.GlobalData.SubDocTableServiceColumns;
      FixedInfo["AllDocServiceColumns"] = source.GlobalData.AllDocServiceColumns;
      FixedInfo["AllSubDocServiceColumns"] = source.GlobalData.AllSubDocServiceColumns;
      FixedInfo["DBPermissions"] = source.MainDBEntry.Permissions;
      FixedInfo["UserPermissions"] = source.UserPermissions.AsXmlText;

      FixedInfo.SetReadOnly();
      return FixedInfo;
    }

#if DEBUG

    /// <summary>
    /// Деструктор для отладки
    /// </summary>
    ~DBxRealDocProvider()
    {
      if (DBxDocProvider.DebugDocProviderList != null)
        Trace.WriteLine(DateTime.Now.ToString("G") + ", " + GetType().ToString() + ", GUID=" + _DebugGuid.ToString() + " finalized.");
    }

#endif

    #endregion

    #region Свойства

    /// <summary>
    /// Источник данных
    /// Источник данных недоступен как public, т.к. DocProvider может передаваться клиенту по ссылке
    /// </summary>
    internal DBxRealDocProviderSource Source { get { return _Source; } }
    private DBxRealDocProviderSource _Source;

    /// <summary>
    /// Данные о сессии (подключенном пользователе) произвольного типа
    /// Может использоваться, в частности, для вывода информации об установленных блокировках
    /// </summary>
    public object SessionInfo
    {
      get
      {
        CheckThread();

        return _SessionInfo;
      }
      set
      {
        CheckThread();

        _SessionInfo = value;
      }
    }
    private object _SessionInfo;

#if DEBUG

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в пользовательском коде
    /// </summary>
    public Guid DebugGuid { get { return _DebugGuid; } }
    private Guid _DebugGuid;

#endif

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      string s = base.ToString() + ", Source DB=" + _Source.MainDBEntry.DB.ToString();
#if DEBUG
      s += " Guid=" + _DebugGuid.ToString("B");
#endif
      return s;
    }

    //public override DateTime ServerTime { get { return DateTime.Now; } }

    #endregion

    #region Загрузка данных

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Таблица документов</returns>
    public override DataTable LoadDocData(string docTypeName, Int32[] docIds)
    {
      return LoadDocData(docTypeName, new IdsFilter(docIds));
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    public override DataTable LoadDocData(string docTypeName, DBxFilter filter)
    {
      CheckThread();

      DBxTableStruct Struct = DocTypes[docTypeName].Struct;
      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        DataTable Table = Con.FillSelect(docTypeName, GetColumns(docTypeName, null), filter);
        Struct.InitDataRowLimits(Table);
        Table.AcceptChanges(); // 06.08.2018
        return Table;
      }
    }

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    public override DataTable LoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      CheckThread();

      DBxTableStruct Struct = DocTypes[docTypeName].SubDocs[subDocTypeName].Struct;
      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        DBxFilter Filter = new IdsFilter("DocId", docIds);
        if (DocTypes.UseDeleted) // 06.08.2018
          Filter = new AndFilter(Filter, DBSSubDocType.DeletedFalseFilter);

        DataTable Table = Con.FillSelect(subDocTypeName, GetColumns(docTypeName, subDocTypeName),
          Filter);
        Struct.InitDataRowLimits(Table);
        Table.AcceptChanges(); // 06.08.2018
        return Table;
      }
    }

    /// <summary>
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    public override DataTable FillSelect(DBxSelectInfo info)
    {
      CheckThread();
      // System.Threading.Thread.Sleep(100000);

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.FillSelect(info);
      }

    }

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    public override DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.FillUniqueColumnValues(tableName, columnName, where);
      }
    }

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public override string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueStringValues(tableName, columnName, where);
      }
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
    public override int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueIntValues(tableName, columnName, where);
      }
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
    public override long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueInt64Values(tableName, columnName, where);
      }
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
    public override float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueSingleValues(tableName, columnName, where);
      }
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
    public override double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueDoubleValues(tableName, columnName, where);
      }
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
    public override decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueDecimalValues(tableName, columnName, where);
      }
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
    public override DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueDateTimeValues(tableName, columnName, where);
      }
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
    public override Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetUniqueGuidValues(tableName, columnName, where);
      }
    }

    #region GetRecordCount() и IsTableEmpty()

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    public override int GetRecordCount(string tableName)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetRecordCount(tableName);
      }
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    public override int GetRecordCount(string tableName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetRecordCount(tableName, where);
      }
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    public override bool IsTableEmpty(string tableName)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.IsTableEmpty(tableName);
      }
    }

    #endregion


    /// <summary>
    /// Загрузить страницы таблицы кэша
    /// </summary>
    /// <param name="request">Параметры запроса</param>
    /// <returns>Кэш страницы</returns>
    public override DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      CheckThread();
      return ((IDBxCacheSource)Source.DBCache).LoadCachePages(request);
    }

    /// <summary>
    /// Очистить страницы таблицы кэша
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Начальные идентификаторы страниц</param>
    public override void ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      CheckThread();
      ((IDBxCacheSource)Source.DBCache).ClearCachePages(tableName, columnNames, firstIds);
    }

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="orderBy">Порядок сортировки. Имеет значение, только если найдено
    /// больше одной записи, удовлетворяющей условию <paramref name="where"/>.
    /// Будет возвращена первая из записей, в соответствии с порядком.
    /// Если порядок не задан, какая запись будет возвращена, не определено</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    public override Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.FindRecord(tableName, where, orderBy);
      }
    }

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="singleOnly">Если true и найдено больше одной записи, удовлетворяющей условию
    /// <paramref name="where"/>, то возвращается 0</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    public override Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.FindRecord(tableName, where, singleOnly);
      }
    }

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0, тогда возвращается Value=null</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <param name="value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    public override bool GetValue(string tableName, Int32 id, string columnName, out object value)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetValue(tableName, id, columnName, out value);
      }
    }

    /// <summary>
    /// Получить значения для заданного списка полей для одной записи.
    /// Если не найдена строка с заданным идентификатором <paramref name="id"/>, 
    /// то возвращается массив, содержащий одни значения null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки</param>
    /// <param name="columnNames">Имена столбцов, значения которых нужно получить</param>
    /// <returns>Массив значений</returns>
    public override object[] GetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetValues(tableName, id, columnNames);
      }
    }

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    public override IdList GetIds(string tableName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetIds(tableName, where);
      }
    }

    /// <summary>
    /// Получить минимальное значение числового поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение</returns>
    public override object GetMinValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetMinValue(tableName, columnName, where);
      }
    }

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Максимальное значение</returns>
    public override object GetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetMaxValue(tableName, columnName, where);
      }
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей максимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="maxColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="maxColumnName">Имя поля, максимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в SearchFieldNames</returns>
    public override object[] GetValuesForMax(string tableName, DBxColumns columnNames, string maxColumnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetValuesForMax(tableName, columnNames, maxColumnName, where);
      }
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей минимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="minColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="minColumnName">Имя поля, минимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в SearchFieldNames</returns>
    public override object[] GetValuesForMin(string tableName, DBxColumns columnNames, string minColumnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetValuesForMin(tableName, columnNames, minColumnName, where);
      }
    }

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns>Суммарное значение или null</returns>
    public override object GetSumValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetSumValue(tableName, columnName, where);
      }
    }

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// Эта перегрузка возвращает идентификатор "зацикленного" узла. Возвращается только один узел, 
    /// а не вся цепочка зацикливания. Также таблица может содержать несколько цепочек зацикливания.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <param name="where">Дополнительный фильтр. Может быть null, если фильтра нет</param>
    /// <param name="loopedId">Сюда записывается идентификатор "зацикленного" узла</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    public override IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      CheckThread();

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        return Con.GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
      }
    }

    #endregion

    #region Права пользователя

    /// <summary>
    /// Возвращает DBxRealDocProviderSource.UserPermissions
    /// </summary>
    public override UserPermissions UserPermissions
    {
      get { return _Source.UserPermissions; }
    }

    #endregion

    #region Кэш

    /// <summary>
    /// Кэш базы данных.
    /// Возвращает DBxRealDocProviderSource.DBCache
    /// Возвращаемый объект является потокобезопасным
    /// </summary>
    public override DBxCache DBCache
    {
      get
      {
        return _Source.DBCache;
      }
    }

    /// <summary>
    /// Сброс кэшированных данных
    /// </summary>
    public override void ClearCache()
    {
      CheckThread();

      base.ClearCache();
      _Source.ClearCache();
    }

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    public override string GetTextValue(string tableName, Int32 id)
    {
      CheckThread();

      return _Source.GetTextValue(tableName, id);
    }

    /// <summary>
    /// Внутренний метод получения текста.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="primaryDS">Первичный набор данных</param>
    /// <returns>Текст для документа или поддокумента</returns>
    public override string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      CheckThread();

      return _Source.InternalGetTextValue(tableName, id, primaryDS);
    }

    #endregion

    #region Файлы в базе данных и двоичные данные

    /// <summary>
    /// Максимальный размер данных, который можно передать клиенту за один раз
    /// </summary>
    const int MaxBinDataSize = 10 * (int)(FileTools.MByte);

    #region Двоичные данные

    /// <summary>
    /// Внутренний метод получения идентификатора двоичных данных
    /// </summary>
    /// <param name="md5">Контрольная сумма</param>
    /// <returns>Идентификатор записи или 0, если таких данных нет</returns>
    public override Int32 InternalFindBinData(string md5)
    {
      CheckBinDataHandler();
      return _Source.GlobalData.BinDataHandler.FindBinData(md5);
    }

    private void CheckBinDataHandler()
    {
      if (_Source.GlobalData.BinDataHandler == null)
        throw new NullReferenceException("Обработчик двоичных данных не установлен");
    }

    /// <summary>
    /// Внутренний метод получения двоичных данных
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор двоичных данных</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и двочных данных, которые нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и двочных данных,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор двоичных данных. Значение - загруженные данные</returns>
    public override Dictionary<Int32, byte[]> InternalGetBinData2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      CheckDocColumnAccess(tableName, columnName, wantedId.DocId, wantedId.SubDocId, docVersion);

      CheckBinDataHandler();

      Dictionary<Int32, byte[]> dict = new Dictionary<int, byte[]>();

      byte[] b = _Source.GlobalData.BinDataHandler.GetBinData(wantedId.DataId);
      dict.Add(wantedId.DataId, b);

      int size = 0;
      if (b != null)
        size = b.Length;
      if (size < MaxBinDataSize)
      {
        for (int i = 0; i < preloadIds.Count; i++)
        {
          if (dict.ContainsKey(preloadIds[i].DataId))
            continue; // один повтор может быть - с wantedId

          if (GetDocColumnAccess(tableName, columnName, preloadIds[i].DocId, preloadIds[i].SubDocId, docVersion))
          {
            b = _Source.GlobalData.BinDataHandler.GetBinData(preloadIds[i].DataId);
#if DEBUG
            if (b == null)
              throw new NullReferenceException("b=null");
#endif
            if (size > (MaxBinDataSize - b.Length))
              break; // получается, что SQL-запрос выполнен зря.
            size += b.Length;
            dict.Add(preloadIds[i].DataId, b);
          }
        }
      }
      return dict;
    }

    #endregion

    #region Файлы

    /// <summary>
    /// Внутренний метод получения идентификатора хранимого файла
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="md5">Контрольная сумма содержимого файла</param>
    /// <param name="dataId">Сюда помещается идентификатор двоичных данных,
    /// возвращаемый методом InternalFindBinData()</param>
    /// <returns>Идентификатор записи файла в базе данных</returns>
    public override Int32 InternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 dataId)
    {
      CheckBinDataHandler();
      return _Source.GlobalData.BinDataHandler.FindDBFile(fileInfo, md5, out dataId);
    }

    /// <summary>
    /// Внутренний метод получения хранимого файла
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор файла</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и файла, который нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и файлов,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор файла. Значение - контейнер с файлом</returns>
    public override Dictionary<Int32, FileContainer> InternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      CheckDocColumnAccess(tableName, columnName, wantedId.DocId, wantedId.SubDocId, docVersion);

      CheckBinDataHandler();

      Dictionary<Int32, FileContainer> dict = new Dictionary<Int32, FileContainer>();

      FileContainer fc = _Source.GlobalData.BinDataHandler.GetDBFile(wantedId.DataId);
#if DEBUG
      if (fc == null)
        throw new BugException("fc==null");
#endif
      dict.Add(wantedId.DataId, fc);

      int size = fc.Contents.Length;
      if (size < MaxBinDataSize)
      {
        for (int i = 0; i < preloadIds.Count; i++)
        {
          if (dict.ContainsKey(preloadIds[i].DataId))
            continue; // один повтор может быть - с wantedId

          if (GetDocColumnAccess(tableName, columnName, preloadIds[i].DocId, preloadIds[i].SubDocId, docVersion))
          {
            fc = _Source.GlobalData.BinDataHandler.GetDBFile(preloadIds[i].DataId);
            if (size > (MaxBinDataSize - fc.Contents.Length))
              break; // получается, что SQL-запрос выполнен зря.
            size += fc.Contents.Length;
            dict.Add(preloadIds[i].DataId, fc);
          }
        }
      }
      return dict;
    }

    #endregion

    #region Проверка прав доступа

    /// <summary>
    /// Проверяет правомочность доступа к конкретному значению поля в документе или поддокументе.
    /// Открывает документ на просмотр. При этом проверяются права пользователя на документ (и поддокумент)
    /// и на поле.
    /// Если каких-либо прав не хватает, генерируется исключение.
    /// Поддерживает фиктивные идентификаторы. В этом случае, выполняется не просмотр, а попытка создания
    /// нового документа / поддокумента.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя столбца документа или подлокумента</param>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="subDocId">Идентификатор поддокумента, если проверка для поддокумента.</param>
    /// <param name="docVersion">Если задано ненулевое значение, то будет использован метод DBxMultiDocs.ViewVersion()</param>
    private void CheckDocColumnAccess(string tableName, string columnName, Int32 docId, Int32 subDocId, int docVersion)
    {
      DBxDocType dt;
      DBxSubDocType sdt;
      if (!DocTypes.FindByTableName(tableName, out dt, out sdt))
      {
        if (String.IsNullOrEmpty(tableName))
          throw new ArgumentNullException("tableName");
        else
          throw new ArgumentException("Неизвестная таблица \"" + tableName + "\"", "tableName");
      }

      // 25.04.2020
      // Не нужно!
      //CheckIsRealDocId(docId);
      if (sdt == null)
      {
        if (subDocId != 0)
          throw new ArgumentException("Задан ненулевой subDocId, хотя таблица \"" + tableName + "\" является документом", "subDocId");
      }
      //else
      //{
      //  CheckIsRealDocId(subDocId);
      //}

      // Открываем документ на просмотр, чтобы убедиться в наличии прав доступа
      DBxDocSet DocSet = new DBxDocSet(this);
      DBxSingleDoc Doc;
      if (docId < 0)
        Doc = DocSet[dt.Name].Insert();
      else
      {
        if (docVersion == 0)
          Doc = DocSet[dt.Name].View(docId); // тут может быть DBxAccessException
        else
          Doc = DocSet[dt.Name].ViewVersion(docId, docVersion); // 26.04.2017
      }

      if (sdt == null)
      {
        // Само значение идентификатора нам не нужно. Все равно оно могло уже устареть.
        // Главное, что у нас есть право на получение этого значения
        object DummyValue = Doc.Values[columnName].Value;
      }
      else
      {
        DBxSubDoc SubDoc;
        if (subDocId < 0)
        {
          if (docId > 0 && docVersion == 0) // 02.09.2018
            Doc.Edit(); // переключаемся на редактирование
          SubDoc = Doc.SubDocs[sdt.Name].Insert();
        }
        else
          SubDoc = Doc.SubDocs[sdt.Name].GetSubDocById(subDocId);
        object DummyValue = SubDoc.Values[columnName].Value;
      }
    }

    [DebuggerStepThrough]
    private bool GetDocColumnAccess(string tableName, string columnName, Int32 docId, Int32 subDocId, int docVersion)
    {
      try
      {
        CheckDocColumnAccess(tableName, columnName, docId, subDocId, docVersion);
        return true;
      }
      catch
      {
        return false;
      }
    }

    #endregion

    #endregion

    #region Запись изменений

    /// <summary>
    /// Не используется в пользовательском коде
    /// </summary>
    /// <returns></returns>
    public override DistributedCallData StartServerExecProc(NamedValues args)
    {
      return new RealDocProviderExecProc(this).StartDistributedCall(args);
    }

    /// <summary>
    /// Выполняемая процедура-заглушка, используемая, когда метод ApplyChanges() вызван не из ExecProc
    /// </summary>
    private class RealDocProviderExecProc : ExecProc
    {
      #region Конструктор

      public RealDocProviderExecProc(DBxRealDocProvider docProvider)
      {
        _DocProvider = docProvider;
        this.DisplayName = "RealDocProviderExecProc для " + docProvider.ToString();
        base.SetContext(NamedValues.Empty);
      }

      private DBxRealDocProvider _DocProvider;

      #endregion

      #region Переопределенные методы

      protected override NamedValues OnExecute(NamedValues args)
      {
        NamedValues DispRes = new NamedValues();
        string Action = args.GetString("Action");
        switch (Action)
        {
          case "FillSelect": // 19.08.2020
            DispRes["Table"] = _DocProvider.FillSelect((DBxSelectInfo)(args["SelectInfo"]));
            break;

          case "ApplyChanges":
            DispRes["DataSet"] = _DocProvider.ApplyChangesInternal(/*this,*/ (DataSet)(args["DataSet"]), args.GetBool("ReloadData"));
            break;

          case "RecalcColumns":
            _DocProvider.RecalcColumns(args.GetString("DocTypeName"), (Int32[])(args["DocIds"]));
            break;

          case "InternalGetBinData2": // 14.10.2020
            DispRes["Data"] = _DocProvider.InternalGetBinData2(args.GetString("TableName"),
              args.GetString("ColumnName"),
              (DocSubDocDataId)(args["WantedId"]),
              args.GetInt("DocVersion"),
              (List<DocSubDocDataId>)(args["PreloadIds"]));
            break;
          case "InternalGetDBFile2": // 14.10.2020
            DispRes["Data"] = _DocProvider.InternalGetDBFile2(args.GetString("TableName"),
              args.GetString("ColumnName"),
              (DocSubDocDataId)(args["WantedId"]),
              args.GetInt("DocVersion"),
              (List<DocSubDocDataId>)(args["PreloadIds"]));
            break;

          default:
            throw new ArgumentException("Неизвестный Action=" + Action.ToString(), "args");
        }
        return DispRes;
      }

      #endregion
    }

    #region Основные методы

    /// <summary>
    /// Применение изменений.
    /// Выполняется создание, изменение и удаление документов и поддокументов
    /// </summary>
    /// <param name="dataSet">Набор данных</param>
    /// <param name="reloadData">Если true, то будет возвращен тот же набор данных.
    /// В нем фиктивные идентификаторы новых документов и поддокументов будут заменены на реальные,
    /// а перекрестные ссылки на них исправлены. Это требуется в интерфейсе пользователя, когда
    /// пользователь нажимает кнопку "Применить", чтобы можно было продолжить сеанс редактирования.
    /// Если false, то исправленный набор данных не возвращается</param>
    /// <returns>Набор с исправленными ссылками или null</returns>
    protected override DataSet OnApplyChanges(DataSet dataSet, bool reloadData)
    {
      CheckThread();

      // Нам нужно использовать блокировки
      // Если метод вызван не из процедуры ExecProc, создаем временную процедуру
      ExecProc CurrProc = ExecProc.CurrentProc;
      if (CurrProc == null)
      {
        using (RealDocProviderExecProc Proc2 = new RealDocProviderExecProc(this))
        {
          NamedValues DispArgs = new NamedValues();
          DispArgs["Action"] = "ApplyChanges";
          DispArgs["DataSet"] = dataSet;
          DispArgs["ReloadData"] = reloadData;
          NamedValues DispRes = Proc2.Execute(DispArgs);
          return (DataSet)(DispRes["DataSet"]);
        }
      }
      else
        return ApplyChangesInternal(/*CurrProc, */dataSet, reloadData);
    }

    #region Сохранение ExtendedProperties

    /// <summary>
    /// Сохраняет и восстанавливает свойства DataTable.ExtendedProperties
    /// </summary>
    private class ExtendedPropertiesSaver
    {
      #region Конструктор

      public ExtendedPropertiesSaver(DataSet ds)
      {
#if DEBUG
        if (ds == null)
          throw new ArgumentNullException("ds");
#endif

        _TableProps = new Hashtable[ds.Tables.Count];
        for (int i = 0; i < ds.Tables.Count; i++)
        {
          if (ds.Tables[i].ExtendedProperties.Count > 0)
            _TableProps[i] = ds.Tables[i].ExtendedProperties.Clone() as Hashtable;
        }
      }

      #endregion

      #region Поля

      /// <summary>
      /// Копии наборов свойств DataTable.ExtendedProperties.
      /// Количество элементов в массиве соответствует количеству таблиц в DataSet. Доступ к таблицам по индексу, а не по имени.
      /// Значения null в массиве испрользуюися, если набор свойств таблицы пустой
      /// </summary>
      private Hashtable[] _TableProps;

      #endregion

      #region Восстановление свойств

      public void Restore(DataSet ds)
      {
#if DEBUG
        if (ds == null)
          throw new ArgumentNullException("ds");
#endif
        if (ds.Tables.Count != _TableProps.Length)
          throw new ArgumentException("Набор данных имеет другое количество таблиц: " + ds.Tables.Count.ToString() + ". Ожидалось: " + _TableProps.Length.ToString(), "ds"); // 20.06.2021

        for (int i = 0; i < ds.Tables.Count; i++)
        {
          PropertyCollection pc = ds.Tables[i].ExtendedProperties;
          pc.Clear();
          if (_TableProps[i] != null)
          {
            foreach (DictionaryEntry de in _TableProps[i])
              pc.Add(de.Key, de.Value);
          }
        }
      }

      #endregion
    }

    #endregion


    private DataSet ApplyChangesInternal(/*ExecProc caller, */DataSet ds, bool reloadData)
    {
      ExtendedPropertiesSaver OldProps = new ExtendedPropertiesSaver(ds);
      try
      {
        return ApplyChangesInternal2(/*caller, */ds, reloadData);
      }
      catch
      {
        try
        {
          OldProps.Restore(ds); // 24.11.2019
        }
        catch (Exception e2) // 20.06.2021
        {
          LogoutTools.LogoutException(e2, "Ошибка вызова ExtendedPropertiesSaver.Restore()");
        }
        throw;
      }
    }

    private DataSet ApplyChangesInternal2(/*ExecProc caller, */DataSet ds, bool reloadData)
    {
      #region Эти подготовительные действия можно выполнить до открытия соединений

      // Время редактирование лучше извлечь в первую очередь, чтобы оно было как можно точнее.
      // Если сначала выполнить другие действия, то набежит задержка.
      // Делать это в OnApplyChanges() нет особого смысла, т.к. ApplyChangesInternal() вызывается
      // синхронно, то есть без задержки.
      TimeSpan EditTime = TimeSpan.Parse(ds.ExtendedProperties["EditTime"].ToString());
      DateTime StartTime = DateTime.Now - EditTime;


      // Извлекаем параметры
      DBxDocSet docSet = new DBxDocSet(this, ds);
      if (this.UserId == 0 && DocTypes.UseUsers)
        throw new InvalidOperationException("Не задан идентификатор пользователя, выполняющего запись данных");

      RestoreAddedState(ds); // 19.03.2016

      // TODO: Когда будет доступ к тексту документа на сервере, перенести этот код после вызова события BeforeWrite
      if (String.IsNullOrEmpty(docSet.ActionInfo) && docSet.UserActionId == 0)
        docSet.ActionInfo = MakeActionInfo(docSet);

      //if (Source.GlobalData.UndoDBEntry == null)
      //  throw new NullReferenceException("Не установлено свойство DBxRealDocProviderGlobal.UndoDBEntry");

      //int DocCount = DocSet.GetDocCount(DBxDocState.Insert) +
      //  DocSet.GetDocCount(DBxDocState.Edit) + DocSet.GetDocCount(DBxDocState.Delete);

      #endregion

      // Установка соединений
      using (DBxConArray Cons = new DBxConArray(
        Source.MainDBEntry, // с правами текущего пользователя
        Source.GlobalData.MainDBEntry, // с полными правами
        Source.GlobalData.UndoDBEntry))
      {
        DBxCon MainConUser = Cons[0];
        DocUndoHelper UndoHelper = new DocUndoHelper(Cons[1], Cons[2],
        this.UserId, this.SessionId, docSet.UserActionId, docSet.ActionInfo, DocTypes, StartTime);

        // Установка блокировки на запись документов
        DBxShortDocsLock DBLock = new DBxShortDocsLock(this, docSet.IgnoreAllLocks, docSet.IgnoredLocks);
        DBLock.Data.Init(this, ds);
        using (new ExecProcLockKey(DBLock))
        {
          Source.PerformBeforeApplyChanges(docSet); // 15.11.2016

          // 31.01.2022. Загрузка исходных документов
          DBxDocSelection docSel = docSet.GetDocSelection(new DBxDocState[] { DBxDocState.Insert, DBxDocState.Edit, DBxDocState.Delete });
          DBxDocSet orgDocSet = new DBxDocSet(this);
          orgDocSet.UseTestDocument = false; // иначе будут лишние вызовы тестирования для режима View.
          orgDocSet.View(docSel);

          // Вызов обработчиков DBxDocType
          foreach (DBxMultiDocs Docs in docSet)
          {
#if DEBUG
            if (Docs.DocType.TableId == 0)
              throw new BugException("Для документов " + Docs.DocType.ToString() + " не установлено свойство TableId");
#endif

            for (int i = 0; i < Docs.DocCount; i++)
            {
              DBxSingleDoc doc = Docs[i];
              DBxSingleDoc orgDoc = new DBxSingleDoc();
              if (this.IsRealDocId(doc.DocId) && doc.DocState!=DBxDocState.View)
                orgDoc = orgDocSet[Docs.DocType.Name].GetDocById(doc.DocId);
              CallDocTypeEventsForDoc(doc, orgDoc);
            }
          }

          // Удаляем лишние записи из таблиц BinData и FileNames
          docSet.InternalDeleteUnusedBinDataAndFiles();

          // Вызываем AppendBinData() и заменяем фиктивные ссылки
          Dictionary<Int32, Int32> BinDataReplaces = CallAppendBinData(ds);
          ReplaceAppendBinData(ds, BinDataReplaces);
          // Вызываем AppendDBFile() и заменяем фиктивные ссылки
          Dictionary<Int32, Int32> FileNameReplaces = CallAppendDBFiles(ds);
          ReplaceAppendDBFiles(ds, FileNameReplaces);

          #region Проверка возможности удаления документов и поддокументов

          foreach (DBxMultiDocs MultiDocs in docSet)
            ApplyDocDelete1(MainConUser, MultiDocs);

          #endregion

          // Актуализация undo
          ActualizeUndo(docSet, UndoHelper, MainConUser);

          #region Выполняем реальную запись

          using (DBxTransactionArray ta = new DBxTransactionArray(Cons[2], Cons[0]))
          {
            #region Замена фиктивных идентификаторов Id для новых документов / поддокументов на реальные, которые будут записаны

            DBxDocProviderIdReplacer IdReplacer = new DBxDocProviderIdReplacer(this);
            IdReplacer.PerformReplace(docSet, ds);

            #endregion

            WriteChanges2(docSet, MainConUser, UndoHelper, IdReplacer);

            WriteDelayedColumns(IdReplacer.DelayedList, ds, MainConUser);

            if (docSet.CheckDocs)
              DoCheckDocs(docSet);

            ta.Commit();
          }

          docSet.UserActionId = UndoHelper.UserActionId;

          #region Убираем невозвратные таблицы

          int p = ds.Tables.IndexOf("FileNames");
          if (p >= 0)
            ds.Tables.RemoveAt(p);
          p = ds.Tables.IndexOf("BinData");
          if (p >= 0)
            ds.Tables.RemoveAt(p);

          #endregion

#if DEBUG
          base.CheckAreRealIds(ds);
#endif

          #endregion

          #region Сброс буферизованных таблиц

          _Source.ClearCache(ds); // очищаем кэш сервера текущего пользователя
          _Source.GlobalData.ClearCache(ds); // добавляем сведения об очистке для других пользователей

          #endregion
        }
      }
      // Извещаем о записи документа
      PerformAfterWrite(docSet);

      if (reloadData)
      {
        return ds;
      }
      else
        return null;
    }

    #endregion

    #region Подготовительные действия

    private string MakeActionInfo(DBxDocSet docSet)
    {
      StringBuilder sb = new StringBuilder();
      foreach (DBxMultiDocs Docs in docSet)
      {
        int pos = sb.Length;
        switch (docSet.DocStateNoView)
        {
          case DBxDocState.Insert:
            sb.Append("Создание");
            break;
          case DBxDocState.Edit:
            sb.Append("Изменение");
            break;
          case DBxDocState.Delete:
            sb.Append("Удаление");
            break;
          case DBxDocState.Mixed:
            sb.Append("Обработка");
            break;
          default:
            continue;
        }

        if (pos > 0)
          sb.Insert(pos, ", ");

        if (Docs.DocCount == 1)
        {
          sb.Append(" д-та \"");
          sb.Append(Docs.DocType.SingularTitle);
          sb.Append("\"");
        }
        else
        {
          sb.Append(" д-тов \"");
          sb.Append(Docs.DocType.PluralTitle);
          sb.Append("\" (");
          sb.Append(Docs.DocCount.ToString());
          sb.Append(")");
        }
      }

      if (sb.Length == 0)
        sb.Append("Нет действий");
      return sb.ToString();
    }

    /// <summary>
    /// Переводит все строки в состоянии Modified и с Id меньше 0 в состояние Added.
    /// Такие строки поддокументов могут возникнуть, если поддокумент был создан, а затем изменен до записи в базу данных
    /// </summary>
    /// <param name="ds"></param>
    private static void RestoreAddedState(DataSet ds)
    {
      foreach (DataTable Table in ds.Tables)
      {
        foreach (DataRow Row in Table.Rows)
        {
          if (Row.RowState == DataRowState.Modified)
          {
            if (DataTools.GetInt(Row, "Id") < 0)
              DataTools.SetRowState(Row, DataRowState.Added);
          }
        }
      }
    }

    #endregion

    #region Двоичные данные и файлы

    private Dictionary<Int32, Int32> CallAppendBinData(DataSet ds)
    {
      DataTable tblBinData = ds.Tables["BinData"];
      if (tblBinData == null)
        return null;

#if DEBUG
      if (_Source.GlobalData.BinDataHandler == null)
        throw new NullReferenceException("Обработчик BinDataHandler не присоединен");
#endif

      Dictionary<Int32, Int32> Dict = new Dictionary<Int32, Int32>();
      foreach (DataRow Row in tblBinData.Rows)
      {
        Int32 OldId = (Int32)(Row["Id"]);
#if DEBUG
        if (OldId >= 0)
          throw new BugException("В таблице BinData присутствует строка с Id=" + OldId.ToString() + ". Ожидался отрицательный идентификатор");
#endif
        byte[] Data = (byte[])(Row["Contents"]);
        Int32 NewId = _Source.GlobalData.BinDataHandler.AppendBinData(Data);
        Dict.Add(OldId, NewId);
      }

      return Dict;
    }

    private void ReplaceAppendBinData(DataSet ds, Dictionary<Int32, Int32> binDataReplaces)
    {
      if (binDataReplaces == null)
        return;

      foreach (DataTable Table in ds.Tables)
      {
        if (Table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocTypes.FindByTableName(Table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.BinDataRefs.Count; i++)
        {
          int p = Table.Columns.IndexOf(dt.BinDataRefs[i].Column.ColumnName);

          for (int j = 0; j < Table.Rows.Count; j++)
          {
            if (Table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 Id = DataTools.GetInt(Table.Rows[j][p]);
            Int32 NewId;
            if (binDataReplaces.TryGetValue(Id, out NewId))
              Table.Rows[j][p] = NewId;
          }
        }
      }

      #region Замена в FileNames

      DataTable tblFileNames = ds.Tables["FileNames"];
      if (tblFileNames != null)
      {
        foreach (DataRow Row in tblFileNames.Rows)
        {
          Int32 OldDataId = DataTools.GetInt(Row, "Data");
          Int32 NewDataId;
          if (binDataReplaces.TryGetValue(OldDataId, out NewDataId))
            Row["Data"] = NewDataId;
        }
      }

      #endregion
    }

    private Dictionary<Int32, Int32> CallAppendDBFiles(DataSet ds)
    {
      DataTable tblFileNames = ds.Tables["FileNames"];
      if (tblFileNames == null)
        return null;

#if DEBUG
      if (_Source.GlobalData.BinDataHandler == null)
        throw new NullReferenceException("Обработчик BinDataHandler не присоединен");
#endif

      Dictionary<Int32, Int32> Dict = new Dictionary<Int32, Int32>();
      foreach (DataRow Row in tblFileNames.Rows)
      {
        Int32 OldId = (Int32)(Row["Id"]);
#if DEBUG
        if (OldId >= 0)
          throw new BugException("В таблице FileNames присутствует строка с Id=" + OldId.ToString() + ". Ожидался отрицательный идентификатор");
#endif
        Int32 DataId = DataTools.GetInt(Row, "Data");
        if (DataId <= 0)
          throw new BugException("В таблице FileNames присутствует строка, в которой Data=" + DataId.ToString());

        StoredFileInfo FileInfo = new StoredFileInfo(DataTools.GetString(Row, "Name"),
          DataTools.GetInt(Row, "Length"),
          DataTools.GetNullableDateTime(Row, "CreationTime"),
          DataTools.GetNullableDateTime(Row, "LastWriteTime"));

        Int32 NewId = _Source.GlobalData.BinDataHandler.AppendDBFile(FileInfo, DataId);
        Dict.Add(OldId, NewId);
      }

      return Dict;
    }

    private void ReplaceAppendDBFiles(DataSet ds, Dictionary<Int32, Int32> fileNameReplaces)
    {
      if (fileNameReplaces == null)
        return;

      foreach (DataTable Table in ds.Tables)
      {
        if (Table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocTypes.FindByTableName(Table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.FileRefs.Count; i++)
        {
          int p = Table.Columns.IndexOf(dt.FileRefs[i].Column.ColumnName);

          for (int j = 0; j < Table.Rows.Count; j++)
          {
            if (Table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 Id = DataTools.GetInt(Table.Rows[j][p]);
            Int32 NewId;
            if (fileNameReplaces.TryGetValue(Id, out NewId))
              Table.Rows[j][p] = NewId;
          }
        }
      }

    }

    #endregion

    #region Вызов пользовательский обработчиков в DBxDocType

    /// <summary>
    /// Вызов пользовательских обработчиков DBxDocType.BeforeXXX и метода TestDocument()
    /// </summary>
    /// <param name="doc">Записываемый (удаляемый) документ</param>
    /// <param name="orgDoc">Перечитанный документ из базы данных (в режиме View) или пустышка</param>
    private void CallDocTypeEventsForDoc(DBxSingleDoc doc, DBxSingleDoc orgDoc)
    {
      DBxDocType docType = doc.DocType;
      DBxDocState docState = doc.DocState;

      if (docState == DBxDocState.Insert)
      {
        bool firstCall = (doc.MultiDocs.GetDocIdActionId(doc.DocId) == 0);
        if (firstCall)
        {
          this.TestDocument(doc, DBxDocPermissionReason.ApplyNew);
          docType.PerformBeforeInsert(doc, false);
          docType.PerformBeforeWrite(doc, true, false);
          return;
        }
        docState = DBxDocState.Edit;
      }

      if (docState == DBxDocState.Edit) // включая повторный вызов для Insert
      {
#if DEBUG
        this.CheckIsRealDocId(doc.DocId);
        if (orgDoc.MultiDocs == null)
          throw new NullReferenceException("Неинициализированный orgDoc для DocId=" + doc.DocId.ToString());
#endif

        if (DocTypes.UseDeleted)
        {
          if (orgDoc.Deleted)
            this.TestDocument(doc, DBxDocPermissionReason.ApplyRestore);
        }
        this.TestDocument(orgDoc, DBxDocPermissionReason.ApplyEditOrg);
        this.TestDocument(doc, DBxDocPermissionReason.ApplyEditNew);

        if (DocTypes.UseDeleted)
        {
          if (orgDoc.Deleted)
            docType.PerformBeforeInsert(doc, true); // 24.11.2017
        }
        docType.PerformBeforeWrite(doc, false, false);
        return;
      }

      if (docState == DBxDocState.Delete)
      {
#if DEBUG
        this.CheckIsRealDocId(doc.DocId);
        if (orgDoc.MultiDocs == null)
          throw new NullReferenceException("Неинициализированный orgDoc для DocId=" + doc.DocId.ToString());
#endif

        this.TestDocument(orgDoc, DBxDocPermissionReason.ApplyDelete);
        docType.PerformBeforeDelete(doc);
        return;
      }
    }

    #endregion

    #region Актуализация таблиц в Undo

    /// <summary>
    /// Актуализация прикладных таблиц документов и поддокументов
    /// В процессе актуализации используются "глобальные" соединения с БД,
    /// для которых не установлены ограничения пользователя
    /// - Для каждого документа проверяется корректность текущей записи в
    ///   таблице DocActions и, при необходимости, вносятся исправления
    /// - Для документов и поддокументов, которые предполагается изменить,
    ///   выполняется актуализация до текущей версии
    /// На момент вызова установка блокировка записи документов, поэтому 
    /// параллельный вызов метода для одного документа невозможен
    /// В процессе работы транзакции используются кратковременно, только на время 
    /// актуализации одной строки
    /// </summary>
    /// <param name="docSet">Набор данных, содержаший новые значения</param>
    /// <param name="undoHelper">Вспомогательный объект</param>
    /// <param name="mainConUser">Соединение с основной базой данных</param>
    private void ActualizeUndo(DBxDocSet docSet, DocUndoHelper undoHelper, DBxCon mainConUser)
    {
      if (undoHelper.UndoCon == null)
        return;

      undoHelper.InitUserAction();

      foreach (DBxMultiDocs MultiDocs in docSet)
      {
        for (int i = 0; i < MultiDocs.DocCount; i++)
        {
          DBxSingleDoc Doc = MultiDocs[i];
          switch (Doc.DocState)
          {
            case DBxDocState.Edit:
            case DBxDocState.Delete:
              if (MultiDocs.GetDocIdActionId(Doc.DocId) == 0)
              {
                // Первое обращение к документу - требуется проверка таблицы DocActions
                undoHelper.ValidateDocVersion(MultiDocs.DocType, Doc.DocId);
                if (Doc.DocState == DBxDocState.Edit && Doc.IsDataModified)
                {
                  int DocVersion = DataTools.GetInt(mainConUser.GetValue(MultiDocs.DocType.Name, Doc.DocId, "Version"));
                  if (DocVersion == 0) // Исправляем ошибку
                    DocVersion = 1;
                  if (Doc.IsMainDocModified)
                    undoHelper.ActualizeMainDoc(MultiDocs.DocType, Doc.DocId, DocVersion);
                  for (int j = 0; j < Doc.SubDocs.Count; j++)
                  {
                    foreach (DBxSubDoc SubDoc in Doc.SubDocs[j])
                    {
                      // 19.03.2016
                      if (SubDoc.SubDocId < 0)
                        continue; // сначала поддокумент добавили, затем сразу удалили или изменили

                      switch (SubDoc.SubDocState)
                      {
                        case DBxDocState.Edit:
                          if (SubDoc.IsDataModified)
                            undoHelper.ActualizeSubDoc(SubDoc.SubDocType, SubDoc.SubDocId, DocVersion);
                          break;
                        case DBxDocState.Delete:
                          // при удалении тоже актуализация
                          undoHelper.ActualizeSubDoc(SubDoc.SubDocType, SubDoc.SubDocId, DocVersion);
                          break;
                      }
                    }
                  }
                }
              }
              break;
          }
        }
      }
    }

    #endregion

    #region Основная запись изменений

    /// <summary>
    /// Выполнение действий внутри заблокированной БД и транзакций
    /// </summary>
    /// <param name="docSet"></param>
    /// <param name="mainConUser"></param>
    /// <param name="undoHelper"></param>
    /// <param name="idReplacer"></param>
    /// <returns></returns>
    private void WriteChanges2(DBxDocSet docSet, DBxCon mainConUser, DocUndoHelper undoHelper,
      DBxDocProviderIdReplacer idReplacer)
    {
      // Общее число документов
      int TypeIndex;
      int DocIndex;

      // Прерывать процесс записи, начиная с этой стадии, нельзя
      for (TypeIndex = 0; TypeIndex < docSet.Count; TypeIndex++)
      {
        DBxMultiDocs MultiDocs = docSet[TypeIndex];
        DBxColumns UsedColumnNames = null;
        for (DocIndex = 0; DocIndex < MultiDocs.DocCount; DocIndex++)
        {
          DBxSingleDoc Doc = MultiDocs[DocIndex];

          switch (Doc.DocState)
          {
            case DBxDocState.Insert:
            case DBxDocState.Edit:
              ApplyDocChanges2(MultiDocs.DocType, mainConUser, undoHelper, Doc, idReplacer, ref UsedColumnNames);
              break;
            case DBxDocState.Delete:
              ApplyDocDelete2(MultiDocs, mainConUser, undoHelper, Doc.DocId);
              break;
          }
        }
      }
    }

    /// <summary>
    /// Второй проход - Обработка записи для одного документа в режимах Insert и Edit
    /// </summary>
    private void ApplyDocChanges2(DBxDocType docType, DBxCon mainConUser, DocUndoHelper undoHelper,
      DBxSingleDoc Doc,
      DBxDocProviderIdReplacer IdReplacer, ref DBxColumns UsedColumnNames1)
    {
      //  SingleDocChangesInfo SingleChanges=MultiChanges.Docs[DocIndex];
      Int32 DocId = Doc.DocId; // идентификатор документа

      // Это первый вызов ApplyChanges для этого документа ?
      bool FirstCall = (Doc.MultiDocs.GetDocIdActionId(Doc.DocId) == 0);
      // Будет ли выполняться добавление строки в основную таблицу данных ?
      bool DocRealAdded = Doc.DocState == DBxDocState.Insert && FirstCall;

      bool IsModified = Doc.IsDataModified;

      // Определяем, есть ли какие-нибудь изменения
      // В режиме вставки изменения не имеют значения
      // Должно идти после обработчиков DocType, т.к. они могут вносить 
      // дополнительные изменения
      if ((!DocRealAdded) && (!IsModified) && (!Doc.Deleted))
        return;

      // Текущая версия документа
      int CurrVersion = 0;
      if (DocTypes.UseVersions)
      {
        if (!DocRealAdded)
          CurrVersion = DataTools.GetInt(mainConUser.GetValue(docType.Name, DocId, "Version"));
      }

      if (FirstCall)
      {
        UndoAction Action;
        if (Doc.DocState == DBxDocState.Edit)
          Action = UndoAction.Edit;
        else
          Action = UndoAction.Insert;
        Int32 DocActionId = undoHelper.AddDocAction(docType, DocId, Action, ref CurrVersion);
        // Сообщаем о действии
        Doc.MultiDocs.SetDocIdActionId(DocId, DocActionId);
      }

#if DEBUG
      if (DocTypes.UseVersions)
      {
        if (Doc.DocState == DBxDocState.Insert)
        {
          if (FirstCall)
          {
            // 05.02.2018
            // При повторных вызовах сохранения созданного документа, версия не обязана сохраняться.
            // Она сохраняется, только если пользователь держит длительную блокировку.
            // Однако, это не является обязательным, и другой пользователь может переписать документ

            if (CurrVersion != 1)
            {
              Exception e = new BugException("При создании документа должна быть версия 1, а не " + CurrVersion.ToString());
              e.Data["FirstCall"] = FirstCall;
              e.Data["DocRealAdded"] = DocRealAdded;
              AddExceptionInfo(e, Doc);
              throw e;
            }
          }
        }
        else
        {
          if (CurrVersion < 2)
          {
            Exception e = new BugException("При изменении/удалении документа должна быть версия, больше 1, а не " + CurrVersion.ToString());
            AddExceptionInfo(e, Doc);
            e.Data["FirstCall"] = FirstCall;
            e.Data["DocRealAdded"] = DocRealAdded;
            throw e;
          }
        }
      }
#endif


      // Формируем поля для основной записи
      Hashtable FieldPairs = new Hashtable();

      bool HasPairs = AddUserFieldPairs(FieldPairs, Doc.Row, DocRealAdded, DBPermissions, mainConUser, ref UsedColumnNames1, Doc.DocType.Struct);

      // Добавляем значения служебных полей
      if (Doc.DocState == DBxDocState.Insert && FirstCall)
      {
        // Первое действие для данного документа
        if (DocTypes.UseUsers)
          FieldPairs.Add("CreateUserId", undoHelper.UserId);
        if (DocTypes.UseTime)
          FieldPairs.Add("CreateTime", undoHelper.ActionTime);
        // TODO: int ImportId = DataTools.GetInt(row, "ImportId");
        // TODO: if (ImportId != 0)
        // TODO: FieldPairs.Add("ImportId", ImportId);
      }
      else // Edit
      {
        if (DocTypes.UseUsers)
          FieldPairs.Add("ChangeUserId", undoHelper.UserId);
        if (DocTypes.UseTime)
          FieldPairs.Add("ChangeTime", undoHelper.ActionTime); // пусть запишет также при повторном вызове записи для Insert
      }
      if (DocTypes.UseVersions)
      {
        FieldPairs.Add("Version", CurrVersion);
        Doc.Row["Version"] = CurrVersion;
      }
      if (DocTypes.UseDeleted)
      {
        //if (DataTools.GetBool(Doc.Row, "Deleted"))
        //  FieldPairs.Add("Deleted", false); // Отменяем удаление
        // 08.05.2019
        // Очищаем поле "Deleted" при каждой записи, а не только при первом вызове.
        // Сценарий:
        // - Пользователь 1 открывает документ в редакторе
        // - Пользователь 1 нажимает кнопку "Запись", но не закрывает редактор
        // - Пользователь 2 снимает блокировку документа
        // - Пользователь 2 удаляет документ
        // - Пользователь 1 сохраняет документ еще раз. При этом поле "Deleted" явно нуждается в установке в FALSE
        FieldPairs.Add("Deleted", false); // Отменяем удаление
        Doc.Row["Deleted"] = false;
      }

      if (DocTypes.UseVersions)
      {
#if DEBUG
        if (CurrVersion < 1)
        {
          Exception e = new BugException("CurrVersion=0");
          AddExceptionInfo(e, Doc);
          throw e;
        }
#endif

        if (Doc.DocState == DBxDocState.Insert || HasPairs)
          FieldPairs.Add("Version2", CurrVersion);
      }

      // TODO: FieldPairs.Add("CheckState", DocumentCheckState.Unchecked); // Отменяем результаты проверки
      //if (DocsChanges.MainTable.Columns.Contains("CheckState"))
      // TODO: row["CheckState"] = DocumentCheckState.Unchecked;
      // При первом добавлении требуется писать все данные, а потом
      // - только измененные данные


      // Выполняем добавление записи или обновление
      if (DocRealAdded)
      {
        FieldPairs.Add("Id", DocId);
        mainConUser.AddRecord(docType.Name, FieldPairs);
      }
      else
      {
        // TODO: if (DocType.Buffering != null)
        // TODO:   DocType.Buffering.ClearSinceId(Caller, DocId);
        mainConUser.SetValues(docType.Name, DocId, FieldPairs);
      }

      // Записываем поддокументы
      for (int i = 0; i < Doc.DocType.SubDocs.Count; i++)
      {
        string SubDocTypeName = Doc.DocType.SubDocs[i].Name;
        if (!Doc.MultiDocs.SubDocs.ContainsModified(SubDocTypeName))
          continue;

        DBxSingleSubDocs sds = Doc.SubDocs[SubDocTypeName];
        if (sds.SubDocCount == 0)
          continue;

        DBxTableStruct ts = sds.SubDocs.SubDocType.Struct;

        DBxColumns UsedColumnNames2 = null;

        foreach (DBxSubDoc SubDoc in sds)
        {
          switch (SubDoc.SubDocState)
          {
            case DBxDocState.Insert:
              bool SubDocRealAdded = IdReplacer.IsAdded(SubDocTypeName, SubDoc.SubDocId);
              SubApplyChange1(SubDoc, mainConUser, ts, SubDocRealAdded, CurrVersion, ref UsedColumnNames2);
              break;
            case DBxDocState.Edit:
              if (!SubDoc.IsDataModified)
                continue; // ничего не поменялось
              SubApplyChange1(SubDoc, mainConUser, ts, false, CurrVersion, ref UsedColumnNames2);
              break;
            case DBxDocState.Delete:
              SubApplyDelete1(SubDoc, mainConUser, CurrVersion);
              break;
          }
        }
      }

      // Сброс буферизации расчетных данных для журналов операций
      // TODO: if (DocType.Buffering != null)
      // TODO:   DocType.Buffering.ClearSinceId(Caller, DocId);
    }

    private static void AddExceptionInfo(Exception e, DBxSingleDoc doc)
    {
      e.Data["Doc.DocType.Name"] = doc.DocType.Name;
      e.Data["Doc.DocId"] = doc.DocId;
      e.Data["Doc.DocState"] = doc.DocState;
      e.Data["Doc.Version"] = doc.Version;
      e.Data["Doc.Deleted"] = doc.Deleted;
      e.Data["Doc.IsDataModified"] = doc.IsDataModified;
      e.Data["Doc.IsMainDocModified"] = doc.IsMainDocModified;
    }


    // Эти поля не обрабатываются
    private static readonly DBxColumns AddUserFieldPairsSysColNames = new DBxColumns(new string[] { "Id", "DocId", "Version", "Version2", "StartVersion", "Deleted", "CreateUserId", "CreateTime", "ChangeUserId", "ChangeTime", "ImportId", "CheckState" });

    private static bool AddUserFieldPairs(Hashtable fieldPairs, DataRow row, bool realAdd, DBxPermissions dbPermissions, DBxCon mainConUser, ref DBxColumns usedColumnNames, DBxTableStruct tableStruct)
    {
      #region Список обрабатываемых полей

      if (usedColumnNames == null)
      {
        DBxColumnList lst = new DBxColumnList();
        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
          string colName = row.Table.Columns[i].ColumnName;
          if (AddUserFieldPairsSysColNames.Contains(colName))
            continue;

          if (dbPermissions.ColumnModes[row.Table.TableName, colName] != DBxAccessMode.Full)
            continue; // 17.12.2015, 25.09.2020
          // 16.08.2021. Поля, недоступные для записи пропускаем всегда, а не только для пустых значений

          lst.Add(colName);
        }
        usedColumnNames = new DBxColumns(lst);
      }

      #endregion


      bool Res = false;

      if (row.RowState == DataRowState.Added)
      {
        #region Режим добавления

        for (int i = 0; i < usedColumnNames.Count; i++)
        {
          object NewValue = row[usedColumnNames[i]];
          DBxColumnStruct ColDef = tableStruct.Columns[usedColumnNames[i]];

          // 09.12.2015: Добавление всех полей при повторном вызове для добавляемой записи
          //             У нас нет оригинальной версии строки для DataRowState.Added
          // 17.12.2015: Код переработан
          // 16.08.2021: Учитывается DBxColumnStruct.DefaultValue

          if (realAdd)
          {
            if (ColDef.Nullable)
            {
              if (DataTools.IsEmptyValue(NewValue))
                continue; // 07.08.2018
            }
            else if (!Object.ReferenceEquals(ColDef.DefaultValue, null)) // дополнительное условие добавлено 19.09.2021
            {
              if (NewValue is DBNull)
                continue; // 16.08.2021
            }
          }
          if (NewValue is DBNull && (!ColDef.Nullable))
          {
            if (ColDef.DefaultValue == null)
              NewValue = DataTools.GetEmptyValue(ColDef.DataType); // 07.08.2018
            else
              NewValue = ColDef.DefaultValue; // 16.08.2021
          }

          fieldPairs.Add(usedColumnNames[i], NewValue);
          Res = true;
        }

        #endregion
      }
      else // Row.RowState == DataRowState.Modified
      {
        #region Режим обновления

        // 14.07.2016
        // Исходные значения берем не из версии строки Original, переданной снаружи,
        // а запрашиваем данные из базы данных
        Int32 Id = (Int32)(row["Id"]);
#if DEBUG
        if (Id <= 0)
          throw new BugException("Неправильное значение идентификатора Id=" + Id.ToString());
#endif
        object[] OrgValues = mainConUser.GetValues(row.Table.TableName, Id, usedColumnNames);

        for (int i = 0; i < usedColumnNames.Count; i++)
        {
          DBxColumnStruct ColDef = tableStruct.Columns[usedColumnNames[i]];

          object NewValue = row[usedColumnNames[i], DataRowVersion.Current];
          if (NewValue is DBNull && (!ColDef.Nullable))
            NewValue = DataTools.GetEmptyValue(ColDef.DataType); // 01.09.2018
          // object OrgValue = Row[i, DataRowVersion.Original];
          object OrgValue = OrgValues[i];

          if (!DataTools.AreValuesEqual(OrgValue, NewValue))
          {
            if (dbPermissions.ColumnModes[row.Table.TableName, usedColumnNames[i]] != DBxAccessMode.Full)
            {
              DBxAccessException e = new DBxAccessException("Невозможно изменить значения поля \"" + usedColumnNames[i] + "\" таблицы \"" + row.Table.TableName +
                "\", т.к. нет прав на изменение значения");
              e.Data["TableName"] = row.Table.TableName;
              e.Data["Id"] = row["Id"];
              e.Data["ColumnnNane"] = usedColumnNames[i];
              if (OrgValue is DBNull)
                e.Data["OrgValue"] = "NULL";
              else
                e.Data["OrgValue"] = OrgValue;
              if (NewValue is DBNull)
                e.Data["NewValue"] = "NULL";
              else
                e.Data["NewValue"] = NewValue;
              throw e;
            }

            fieldPairs.Add(usedColumnNames[i], NewValue);
            Res = true;
          }
        }

        #endregion
      }

      return Res;
    }

    private void SubApplyChange1(DBxSubDoc subDoc, DBxCon mainConUser, DBxTableStruct tableDef, bool added, int docVersion, ref DBxColumns usedColumnNames)
    {
      Hashtable FieldPairs = new Hashtable();
      // Служебные поля
      if (added)
      {
        FieldPairs.Add("DocId", subDoc.Doc.DocId);
        if (DocTypes.UseVersions)
          FieldPairs.Add("StartVersion", docVersion);
      }

      if (DocTypes.UseDeleted)
        FieldPairs.Add("Deleted", false);
      // Основные поля
      bool HasPairs = AddUserFieldPairs(FieldPairs, subDoc.Row, added, DBPermissions, mainConUser, ref usedColumnNames, subDoc.SubDocType.Struct);

      if (added || HasPairs)
      {
        if (DocTypes.UseVersions)
          FieldPairs.Add("Version2", docVersion);
      }

      if (added)
      {
        FieldPairs.Add("Id", subDoc.SubDocId);
        mainConUser.AddRecord(tableDef.TableName, FieldPairs);
      }
      else
        mainConUser.SetValues(tableDef.TableName, subDoc.SubDocId, FieldPairs);
    }

    private static readonly DBxColumns SubApplyDelete1Columns = new DBxColumns("Deleted,Version2");

    private void SubApplyDelete1(DBxSubDoc subDoc, DBxCon mainConUser, int docVersion)
    {
      Int32 SubDocId = (Int32)(subDoc.Row["Id", DataRowVersion.Original]);
      if (DocTypes.UseDeleted)
        mainConUser.SetValues(subDoc.SubDocType.Name, SubDocId, SubApplyDelete1Columns,
          new object[] { true, docVersion });
      else
        mainConUser.Delete(subDoc.SubDocType.Name, SubDocId);
    }

    #endregion

    #region Отложенная запись

    /// <summary>
    /// Отложенная запись значений полей (ссылочные поля, которые нельзя записать
    /// в основном проходе). Не забываем модифицировать исходный DataSet
    /// </summary>
    /// <param name="delayedList"></param>
    /// <param name="ds"></param>
    /// <param name="mainConUser"></param>
    private void WriteDelayedColumns(List<DBxDocProviderIdReplacer.DelayedFieldInfo> delayedList,
      DataSet ds, DBxCon mainConUser)
    {
      if (delayedList == null)
        return;

      for (int i = 0; i < delayedList.Count; i++)
      {
        mainConUser.SetValue(delayedList[i].TableName, delayedList[i].Id,
          delayedList[i].FieldName, delayedList[i].FieldValue);

        DataTable Table = ds.Tables[delayedList[i].TableName];
        DataRow Row = Table.Rows.Find(delayedList[i].Id);
        Row[delayedList[i].FieldName] = delayedList[i].FieldValue;
      }
    }

    #endregion

    #region Проверка записанных документов

    private void DoCheckDocs(DBxDocSet docSet)
    {
#if XXX
      for (TypeIndex = 0; TypeIndex < DocSet.Count; TypeIndex++)
      {
        MultiDocsChangesInfo DocsChanges = DocSet[TypeIndex];
        if (DocsChanges.Mode != DocMode.Delete)
        {
          DocCheckers Checkers = null;
          for (DocIndex = 0; DocIndex < DocsChanges.Docs.Count; DocIndex++)
          {
            if (CheckDocs)
            {
              if (Checkers == null)
                Checkers = Caller.GetBufferedDocCheckers(DocSet[TypeIndex].DocType);

              Nullable<DateTime> CheckTime;
              int CheckState = Checkers.PerformCheck(DocsChanges.GetDocId(DocIndex), out CheckTime, false);

              if (DocsChanges.MainTable.Columns.Contains("CheckState"))
                DocsChanges.MainTable.Rows[DocIndex]["CheckState"] = CheckState;
              if (DocsChanges.MainTable.Columns.Contains("CheckTime"))
                DataTools.SetNullableDateTime(DocsChanges.MainTable.Rows[DocIndex], "CheckTime", CheckTime);
            }
            else
            {
              if (DocsChanges.MainTable.Columns.Contains("CheckState"))
                DocsChanges.MainTable.Rows[DocIndex]["CheckState"] = DocumentCheckState.Unchecked;
              if (DocsChanges.MainTable.Columns.Contains("CheckTime"))
                DocsChanges.MainTable.Rows[DocIndex]["CheckTime"] = DBNull.Value;
            }
          }
        }
      }
#endif
    }

    #endregion

    #region Удаление документов

    /// <summary>
    /// Список перекрестных ссылок
    /// Загружается при первом выполнении удаления
    /// </summary>
    private DBxExtRefs _ExtRefs;

    #region Проверка возможности удаления

    private DBxColumns _FieldsCheckDelForDoc; // new DBxColumns("Id,Deleted");
    private DBxColumns _FieldsCheckDelForSubDoc; // new DBxColumns("Id,Deleted,DocId,DocId.Deleted");


    /// <summary>
    /// Проверка возможности удаления документов
    /// Отыскиваем все ссылки на документы и если есть среди действительных 
    /// документов / поддокументов, которые не удаляются вместе с данным документом,
    /// выбрасываем исключение
    /// </summary>
    private void ApplyDocDelete1(DBxCon mainCon, DBxMultiDocs multiDocs)
    {
      if (_FieldsCheckDelForDoc == null)
      {
        if (DocTypes.UseDeleted)
        {
          _FieldsCheckDelForDoc = new DBxColumns("Id,Deleted");
          _FieldsCheckDelForSubDoc = new DBxColumns("Id,Deleted,DocId,DocId.Deleted");
        }
        else
        {
          _FieldsCheckDelForDoc = DBSDocType.IdColumns;
          _FieldsCheckDelForSubDoc = new DBxColumns("Id,DocId");
        }
      }

      Int32[] DeletedDocIds = multiDocs.GetDocIds(DBxDocState.Delete);
      if (DeletedDocIds.Length > 0)
      {
        if (_ExtRefs == null)
          _ExtRefs = new DBxExtRefs(DocTypes, Source.GlobalData.BinDataHandler);


        // Проверка удаления документа
        IdsFilterGenerator DocFltGen = new IdsFilterGenerator(DeletedDocIds);

        ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, multiDocs.DocType, DocFltGen);
        // Проверка удаления всех поддокументов
        // Проверяем наличие непустых ExtRefs
        bool HasSubDocsExtRefs = false;
        for (int i = 0; i < multiDocs.SubDocs.Count; i++)
        {
          if (!_ExtRefs[multiDocs.SubDocs[i].SubDocType.Name].IsEmpty)
          {
            HasSubDocsExtRefs = true;
            break;
          }
        }
        if (HasSubDocsExtRefs)
        {
          DocFltGen.CreateFilters("DocId");
          for (int j = 0; j < DocFltGen.Count; j++)
          {
            for (int i = 0; i < multiDocs.SubDocs.Count; i++)
            {
              DBxSubDocType SubDocType = multiDocs.SubDocs[i].SubDocType;
              IdList SubDocIds = mainCon.GetIds(SubDocType.Name,
                new AndFilter(DocFltGen[j], DBSSubDocType.DeletedFalseFilter));

              IdsFilterGenerator SubDocFltGen = new IdsFilterGenerator(SubDocIds);
              ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, SubDocType, SubDocFltGen);
            }
          }
        }
      }

      Int32[] EditDocIds = multiDocs.GetDocIds(DBxDocState.Edit);
      if (EditDocIds.Length > 0)
      {
        if (_ExtRefs == null)
          _ExtRefs = new DBxExtRefs(DocTypes, Source.GlobalData.BinDataHandler);
        // Проверяем удаляемые поддокументы в редактируемом документе
        // Проверяем наличие непустых ExtRefs
        bool HasSubDocsExtRefs = false;
        for (int i = 0; i < multiDocs.SubDocs.Count; i++)
        {
          if (!_ExtRefs[multiDocs.SubDocs[i].SubDocType.Name].IsEmpty)
          {
            HasSubDocsExtRefs = true;
            break;
          }
        }
        if (HasSubDocsExtRefs)
        {
          IdsFilterGenerator DocFltGen = new IdsFilterGenerator(EditDocIds);
          DocFltGen.CreateFilters("DocId");
          for (int j = 0; j < DocFltGen.Count; j++)
          {
            for (int i = 0; i < multiDocs.SubDocs.Count; i++)
            {
              DBxMultiSubDocs SubDocs = multiDocs.SubDocs[i];
              DataRow[] Rows = SubDocs.Table.Select(DocFltGen[j].ToString(), string.Empty, DataViewRowState.Deleted);
              // Так нельзя, т.к. строки помечены Deleted
              //Int32[] SubDocIds = DataTools.GetIds(Rows);
              Int32[] SubDocIds = new Int32[Rows.Length];
              for (int k = 0; k < Rows.Length; k++)
                SubDocIds[k] = DataTools.GetInt(Rows[k]["Id", DataRowVersion.Original]);

              IdsFilterGenerator SubDocFltGen = new IdsFilterGenerator(SubDocIds);

              ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, SubDocs.SubDocType, SubDocFltGen);
            }
          }
        }
      }
    }

    /// <summary>
    /// Проверка возможности удаления документов/поддокументов одного вида
    /// </summary>
    /// <param name="mainCon">Соединение с основной базой данных</param>
    /// <param name="docSet">Набор данных</param>
    /// <param name="delType">Вид удаляемых документов/поддокументов</param>
    /// <param name="delFltGen">Идентификаторы удаляемых документов/поддокументов</param>
    private void ApplyDocsDelete1Table(DBxCon mainCon, DBxDocSet docSet, DBxDocTypeBase delType, IdsFilterGenerator delFltGen)
    {
      if (_ExtRefs == null)
        _ExtRefs = new DBxExtRefs(DocTypes, Source.GlobalData.BinDataHandler);

      DBxExtRefs.TableRefList ExtRefList = _ExtRefs[delType.Name];

      // 1. Проверяем ссылочные поля
      for (int i = 0; i < ExtRefList.RefColumns.Count; i++)
      {
        DBxExtRefs.RefColumnInfo Ref = ExtRefList.RefColumns[i];
        ApplyDocsDelete1CheckRef(mainCon, docSet, delType, delFltGen, Ref);
      }

      // 2. Проверяем VTReferences
      for (int i = 0; i < ExtRefList.VTRefs.Count; i++)
      {
        DBxExtRefs.VTRefInfo Ref = ExtRefList.VTRefs[i];
        if (delType.IsSubDoc)
          throw new BugException("Не может быть VTReference на поддокумент");
        ApplyDocsDelete1CheckVTRef(mainCon, docSet, (DBxDocType)delType, delFltGen, Ref);
      }
    }

    private void ApplyDocsDelete1CheckRef(DBxCon mainCon, DBxDocSet docSet, DBxDocTypeBase delType, IdsFilterGenerator delFltGen, DBxExtRefs.RefColumnInfo refInfo)
    {
      Int32[] AllDelIds = delFltGen.GetAllIds(); // все идентификаторы удаляемых документов и поддокументов

      delFltGen.CreateFilters(refInfo.ColumnDef.ColumnName);
      DBxColumns Columns = refInfo.IsSubDocType ? _FieldsCheckDelForSubDoc : _FieldsCheckDelForDoc;

      DataTable DetailsTable2 = docSet.DataSet.Tables[refInfo.DetailsTableName];
      //IdList DetailIds2 = new IdList();

      #region Первый проход - по записям в базе данных

      for (int j = 0; j < delFltGen.Count; j++)
      {
        DataTable DetailsTable;
        try
        {
          DBxFilter Filter = delFltGen[j];
          AddDeletedFilters(ref Filter, refInfo.DetailsType.IsSubDoc);
          DetailsTable = mainCon.FillSelect(refInfo.DetailsTableName, Columns, Filter);
        }
        catch (Exception e)
        {
          string s = "Ошибка при запросе таблицы \"" + refInfo.DetailsTableName + "\". ";
          s += "Удаляемые документы/поддокументы: \"" + delType.PluralTitle + "\"";
          s += ". Проверяемое ссылочное поле: \"" + refInfo.ColumnDef.ColumnName + "\"";
          s += ". " + e.Message;
          e.Data["RefColumnInfo"] = s;

          throw;
        }

        for (int k = 0; k < DetailsTable.Rows.Count; k++)
        {
          if (DetailsTable2 != null)
          {
            Int32 DetailId = (Int32)(DetailsTable.Rows[k]["Id"]);
            if (DetailsTable2.Rows.Find(DetailId) != null)
            {
              // DetailIds2.Add(DetailId); // проверим  на втором заходе
              continue;
            }
          }
          ApplyDocDelete1CheckOne(mainCon, docSet, delType, AllDelIds, refInfo.DetailsType, DetailsTable.Rows[k]);
        }
      }

      #endregion

      #region Второй проход - по записям в DBxDocSet

      if (DetailsTable2 != null)
      {
        // Проверять надо все записи
        IdList AllDelIdList = new IdList(AllDelIds);

        foreach (DataRow CheckRow in DetailsTable2.Rows)
        {
          if (CheckRow.RowState == DataRowState.Deleted)
            continue;

          //Int32 CheckedId = (Int32)(CheckRow["Id"]);
          Int32 RefId = DataTools.GetInt(CheckRow, refInfo.ColumnDef.ColumnName);
          if (AllDelIdList.Contains(RefId))
            ApplyDocDelete1CheckOne(mainCon, docSet, delType, AllDelIds, refInfo.DetailsType, CheckRow);
        }
      }

      #endregion
    }

    /// <summary>
    /// Добавляет фильтры по полям "Deleted" и "DocId.Deleted"
    /// </summary>
    /// <param name="filter">Исходный фильтр</param>
    /// <param name="isSubDoc">true, если нужен фильтр по "DocId.Deleted"</param>
    private void AddDeletedFilters(ref DBxFilter filter, bool isSubDoc)
    {
      if (!DocTypes.UseDeleted)
        return;

      if (isSubDoc)
        filter = AndFilter.FromArray(new DBxFilter[]{filter,
          DBSSubDocType.DeletedFalseFilter,
          DBSSubDocType.DocIdDeletedFalseFilter});
      else
        filter = new AndFilter(filter, DBSDocType.DeletedFalseFilter);
    }

    private void ApplyDocsDelete1CheckVTRef(DBxCon mainCon, DBxDocSet docSet, DBxDocType delDocType, IdsFilterGenerator delFltGen, DBxExtRefs.VTRefInfo refInfo)
    {
      delFltGen.CreateFilters(refInfo.VTRef.IdColumn.ColumnName);
      DBxColumns Columns = refInfo.IsSubDocType ? _FieldsCheckDelForSubDoc : _FieldsCheckDelForDoc;

      DataTable CheckTable2 = docSet.DataSet.Tables[refInfo.DetailsTableName];
      IdList CheckTable2UsedIds = new IdList(); // допускает и фиктивные идентификаторы

      for (int j = 0; j < delFltGen.Count; j++)
      {
        DataTable CheckTable = mainCon.FillSelect(refInfo.DetailsTableName,
            Columns,
            new AndFilter(new ValueFilter(refInfo.VTRef.TableColumn.ColumnName, delDocType.TableId),
            delFltGen[j]));

        for (int k = 0; k < CheckTable.Rows.Count; k++)
        {
          if (CheckTable2 != null)
          {
            Int32 CheckedId = (Int32)(CheckTable.Rows[k]["Id"]);
            if (CheckTable2.Rows.Find(CheckedId) != null)
            {
              CheckTable2UsedIds.Add(CheckedId);
              continue; // проверим  на втором заходе
            }
          }
          ApplyDocDelete1CheckOne(mainCon, docSet, delDocType, delFltGen.GetIds(j), refInfo.DetailsType, CheckTable2.Rows[k]);
        }
      }
    }

    /// <summary>
    /// Проверка одной ссылки
    /// </summary>
    /// <param name="mainCon">Соединение для основной базы данных</param>
    /// <param name="docSet">Записываемый набор данных</param>
    /// <param name="delType">Описание удаляемого документа или поддокумента</param>
    /// <param name="delIds">Массив идентификаторов удаляемых документов или поддокументов</param>
    /// <param name="chkType">Описание документа или поддокумента, на который выполняется ссылка</param>
    /// <param name="chkRow">Строка в детальной таблице, на которую проверяется ссылка</param>
    private void ApplyDocDelete1CheckOne(DBxCon mainCon, DBxDocSet docSet, DBxDocTypeBase delType, Int32[] delIds, DBxDocTypeBase chkType, DataRow chkRow)
    {
      //// Удаленные документы / поддокументы пропускаем
      //if (DocTypes.UseDeleted)
      //{
      //  if (DataTools.GetBool(ChkRow, "Deleted"))
      //    return;
      //}
      //if (ChkType.IsSubDoc)
      //{
      //  if (DataTools.GetBool(ChkRow, "DocId.Deleted"))
      //    return;
      //}

      Int32 CheckDocId = DataTools.GetInt(chkRow, chkType.IsSubDoc ? "DocId" : "Id");

      // Проверяем, нет ли ссылающегося документа в списке удаляемых сейчас
      DBxDocType ChkDocType = chkType.IsSubDoc ? ((DBxSubDocType)chkType).DocType : (DBxDocType)chkType;

      if (docSet.ContainsDocs(ChkDocType.Name)) // без этой проверки в DocSet будет добавлен новый вид документов
      {
        DBxMultiDocs MultiDocs = docSet[ChkDocType.Name];

        if (MultiDocs.IndexOfDocId(CheckDocId) >= 0)
        {
          DBxSingleDoc CheckDoc = MultiDocs.GetDocById(CheckDocId);

          if (CheckDoc.DocState == DBxDocState.Delete)
            return; // документ удаляется

          if (chkType.IsSubDoc && CheckDoc.DocState == DBxDocState.Edit)
          {
            Int32 CheckSubDocId = DataTools.GetInt(chkRow, "Id");
            if (MultiDocs.SubDocs.ContainsSubDocs(chkType.Name))
            {
              DBxMultiSubDocs SubDocs = MultiDocs.SubDocs[chkType.Name];

              DataRow[] Rows = SubDocs.Table.Select("Id=" + CheckSubDocId.ToString(),
                String.Empty, DataViewRowState.Deleted);
              if (Rows.Length > 0)
                return;
            }
          }
        }
      }

      // Нельзя удалять документ или поддокумент, т.к. на него имеются ссылки
      StringBuilder sb = new StringBuilder();
      if (delIds.Length == 1)
      {
        if (delType.IsSubDoc)
        {
          DBxDocType DelDocType = ((DBxSubDocType)delType).DocType;
          Int32 DelDocId = DataTools.GetInt(mainCon.GetValue(delType.Name, delIds[0], "DocId"));
          sb.Append("Нельзя удалить поддокумент ");
          AddDocOrSubDocText(sb, delType, delIds[0]);
          sb.Append(" документа ");
          AddDocOrSubDocText(sb, DelDocType, DelDocId);
        }
        else
        {

          sb.Append("Нельзя удалить документ \"");
          AddDocOrSubDocText(sb, delType, delIds[0]);
        }
      }
      else
      {
        if (delType.IsSubDoc)
        {
          //DBxDocType DelDocType = ((DBxSubDocType)delType).DocType;
          sb.Append("Нельзя удалить поддокументы \"");
          sb.Append(delType.PluralTitle);
          sb.Append("\" документа \"");
          sb.Append(delType.SingularTitle);
          sb.Append("\"");

        }
        else
        {

          sb.Append("Нельзя удалить документы \"");
          sb.Append(delType.PluralTitle);
          sb.Append("\"");
        }
      }

      sb.Append(", потому что ");
      if (delIds.Length == 1)
        sb.Append("на него");
      else
        sb.Append("на один из них");
      sb.Append(" есть ссылка в документе ");
      AddDocOrSubDocText(sb, ChkDocType, CheckDocId);
      if (chkType.IsSubDoc)
      {
        Int32 CheckSubDocId = DataTools.GetInt(chkRow, "Id");
        sb.Append(", поддокумент ");
        AddDocOrSubDocText(sb, chkType, CheckSubDocId);
      }
      throw new DBxDocCannotDeleteException(sb.ToString());
    }

    private void AddDocOrSubDocText(StringBuilder sb, DBxDocTypeBase dtb, Int32 id)
    {
      sb.Append("\"");
      sb.Append(dtb.SingularTitle);
      sb.Append("\" (");
      if (IsRealDocId(id))
      {
        sb.Append('\"');
        sb.Append(Source.GlobalData.TextHandlers.GetTextValue(dtb.Name, id));
        sb.Append('\"');
      }
      else
        sb.Append("[ Новый ]");
      if (dtb.IsSubDoc)
        sb.Append(", SubDocId=");
      else
        sb.Append(", DocId=");
      sb.Append(id.ToString());
      sb.Append(")");
    }

    #endregion

    #region Выполнение удаления

    private void ApplyDocDelete2(DBxMultiDocs multiDocs, DBxCon mainConUser, DocUndoHelper undoHelper, Int32 docId)
    {
      int CurrVersion = 0;
      if (DocTypes.UseVersions)
        CurrVersion = DataTools.GetInt(mainConUser.GetValue(multiDocs.DocType.Name, docId, "Version"));

      if (DocTypes.UseDeleted)
      {
        /*Int32 DocActionId = */
        undoHelper.AddDocAction(multiDocs.DocType, docId, UndoAction.Delete, ref CurrVersion);
        Hashtable FieldPairs = new Hashtable();
        // Добавляем значения изменяемых полей
        if (DocTypes.UseVersions)
          FieldPairs.Add("Version", CurrVersion);
        if (DocTypes.UseUsers) // 16.08.2018
          FieldPairs.Add("ChangeUserId", undoHelper.UserId);
        FieldPairs.Add("ChangeTime", undoHelper.ActionTime);
        FieldPairs.Add("Deleted", true);
        mainConUser.SetValues(multiDocs.DocType.Name, docId, FieldPairs);
      }
      else
        mainConUser.Delete(multiDocs.DocType.Name, docId);

      DataRow Row = multiDocs.GetDocById(docId).Row;

      // Нельзя устанавливать значения для удаленной строки напрямую
      Row.RejectChanges();
      if (DocTypes.UseVersions)
        Row["Version"] = CurrVersion;
      Row.Delete();

      // Удаляем буферизованные остатки
      // TODO: if (DocType.Buffering != null)
      // TODO:   DocType.Buffering.ClearSinceId(Caller, DocId);
    }

    #endregion

    #endregion

    #region Вызов события DBxDocSet.AfterWrite

    /// <summary>
    /// Вызов события ServerDocType.AfterWrite для каждого документа
    /// </summary>
    /// <param name="docSet"></param>
    private void PerformAfterWrite(DBxDocSet docSet)
    {
      for (int TypeIndex = 0; TypeIndex < docSet.Count; TypeIndex++)
      {
        DBxMultiDocs MultiDocs = docSet[TypeIndex];
        for (int DocIndex = 0; DocIndex < MultiDocs.DocCount; DocIndex++)
          MultiDocs.DocType.PerformAfterWrite(MultiDocs[DocIndex]);
      }
    }

    #endregion

    #endregion

    #region История изменений

    /// <summary>
    /// Получить таблицу истории для документа (выборка из DocActions)
    /// </summary>
    /// <param name="docTypeName"></param>
    /// <param name="docId"></param>
    /// <returns></returns>
    public override DataTable GetDocHistTable(string docTypeName, Int32 docId)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      DBxDocType DocType = DocTypes[docTypeName];
      if (DocType == null)
        throw new ArgumentException("Неизвестный тип документов \"" + docTypeName + "\"", "docTypeName");
      CheckIsRealDocId(docId);

      if (Source.GlobalData.UndoDBEntry == null)
        throw new NullReferenceException("Не установлено свойство DBxRealDocProviderGlobal.UndoDBEntry");

      #region Проверка прав доступа к истории

      // 1. Проверяем разрешение на просмотр таблицы документов
      if (DBPermissions.TableModes[docTypeName] == DBxAccessMode.None)
        throw new DBxAccessException("Нет доступа на просмотр документов \"" + DocTypes[docTypeName].PluralTitle + "\"");

      // 2. Проверяем разрешение DocTypeViewHistoryPermission
      if (!DocTypeViewHistoryPermission.GetAllowed(UserPermissions, docTypeName))
        throw new DBxAccessException("Нет доступа на просмотр истории документов \"" + DocTypes[docTypeName].PluralTitle + "\"");

      // 3. Проверяем право на просмотр конкретного документа
      DBxDocSet DocSet = new DBxDocSet(this);
      DBxSingleDoc Doc = DocSet[docTypeName].View(docId);

      // 4. Проверяем право на просмотр истории конкретно этого документа
      this.TestDocument(Doc, DBxDocPermissionReason.ViewHistory);

      #endregion

      using (DBxCon MainCon = new DBxCon(Source.MainDBEntry))
      {
        using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
        {
          // Актуальная строка данных
          if (MainCon.FindRecord(docTypeName, "Id", docId) == 0)
            throw new BugException("Не нашли строку документа DocId=" + docId.ToString());

          DBxColumns Columns = new DBxColumns(
            "Id,UserActionId,Version,Action,UserActionId.StartTime,UserActionId.ActionTime,UserActionId.ActionInfo,UserActionId.ApplyChangesTime,UserActionId.ApplyChangesCount");
          if (DocTypes.UseUsers)
            Columns += "UserActionId.UserId";
          if (DocTypes.UseSessionId)
            Columns += "UserActionId.SessionId";

          DataTable tblDocActions = UndoCon.FillSelect("DocActions", Columns,
            new AndFilter(new ValueFilter("DocTableId", DocType.TableId), new ValueFilter("DocId", docId)));

          // Добавляем текст
          DataRow[] BaseRows = tblDocActions.Select("Action=" + ((int)(UndoAction.Base)));
          for (int i = 0; i < BaseRows.Length; i++)
            BaseRows[i]["UserActionId.ActionInfo"] = "Исходное состояние документа";

          return tblDocActions;

#if XXX

          // Таблица истории
          DataTable TableHist = null;
          Int32[] DocActionIds = DataTools.GetIdsFromField(tblDocActions2, "Id");
          if (DocActionIds.Length > 0)
          {
            // TODO: ???
            /*
            TableHist = UndoCon.FillSelect(DocTypeName, null, new IdsFilter("DocActionId", DocActionIds));
            DataTable tbl2 = TableHist.Copy();
            tbl2.TableName = "UndoRows";
            dsRes.Tables.Add(tbl2);
             * */
          }
          // Формируем таблицу изменений
          // Поле Id используется как идентификатор документа (все значения одинаковые)
          DataTable ChTable;
          if (TableHist == null)
          {
            ChTable = MainTable.Clone();
            ChTable.Columns.Add("DocActionId", typeof(Int32));
            ChTable.Columns.Remove("CreateUserId");
            ChTable.Columns.Remove("CreateTime");
            ChTable.Columns.Remove("ChangeUserId");
            ChTable.Columns.Remove("ChangeTime");
            //ChTable.Columns.Remove("CheckState");
            //ChTable.Columns.Remove("CheckTime");
          }
          else
            ChTable = TableHist.Clone();
          // Добавляем служебные поля
          ChTable.Columns.Add("DocActionId.Version", typeof(int));
          ChTable.Columns.Add("DocActionId.UserActionId", typeof(Int32));
          ChTable.Columns.Add("DocActionId.UserActionId.Action", typeof(int));
          ChTable.Columns.Add("DocActionId.UserActionId.UserActionInfo", typeof(string));
          ChTable.Columns.Add("DocActionId.UserActionId.UserActionTime", typeof(DateTime));
          ChTable.Columns.Add("DocActionId.UserActionId.UserId", typeof(Int32));
          ChTable.Columns.Add("DocActionId.UserActionId.UserId.UserName", typeof(string));
          ChTable.Columns.Add("SavedHistoryId", typeof(Int32));

          // Перебираем таблицу версий
          DataView dv = new DataView(tblDocActions2);
          dv.Sort = "Version";
          foreach (DataRowView drv in dv)
          {
            Int32 DocActionId = DataTools.GetInt(drv.Row, "Id");
            DataRow ResRow = ChTable.NewRow();

            if (TableHist != null)
            {
              DataRow[] SrcRows = TableHist.Select("DocActionId=" + DocActionId.ToString());
              if (SrcRows.Length == 1)
              {
                DataTools.CopyRowValues(SrcRows[0], ResRow, true);
                ResRow["SavedHistoryId"] = SrcRows[0]["Id"];
              }
            }

            ResRow["Id"] = DocId;
            ResRow["DocActionId"] = DocActionId;
            ResRow["DocActionId.Version"] = drv.Row["Version"];
            ResRow["DocActionId.UserActionId"] = drv.Row["UserActionId"];
            ResRow["DocActionId.UserActionId.Action"] = drv.Row["Action"];
            ResRow["DocActionId.UserActionId.UserActionInfo"] = drv.Row["UserActionId.ActionInfo"];
            ResRow["DocActionId.UserActionId.UserActionTime"] = drv.Row["UserActionId.ActionTime"];
            ResRow["DocActionId.UserActionId.UserId"] = drv.Row["UserActionId.UserId"];
            ResRow["DocActionId.UserActionId.UserId.UserName"] = drv.Row["UserActionId.UserId.UserName"];
            ChTable.Rows.Add(ResRow);
          }

          return ChTable;

#endif
        }
      }
    }

    /// <summary>
    /// Получение таблицы действий, выполненных пользователем
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="lastDate"></param>
    /// <param name="userId"></param>
    /// <param name="singleDocTypeName"></param>
    /// <returns></returns>
    public override DataTable GetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      CheckThread();

      if (Source.GlobalData.UndoDBEntry == null)
        throw new NullReferenceException("Не установлено свойство DBxRealDocProviderGlobal.UndoDBEntry");

      if (DocTypes.UseUsers) // 21.05.2019
      {
        if (this.UserId == 0)
          throw new InvalidOperationException("Не установлено свойство DBxDocProvider.UserId");
        if (userId != this.UserId)
        {
          if (!ViewOtherUsersActionPermission)
            throw new DBxAccessException("Запрещен доступ на просмотр действий, выполненных другими пользователями");
        }
      }

      DataTable ResTable;

      using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        DBxFilter Filter = null;

        if (userId != 0)
          Filter = new ValueFilter("UserId", userId);

        if (firstDate.HasValue || lastDate.HasValue)
          Filter = Filter & new DateRangeFilter("ActionTime", firstDate, lastDate);

        DBxColumns Columns = new DBxColumns("Id,StartTime,ActionTime,ActionInfo,ApplyChangesTime,ApplyChangesCount");
        if (DocTypes.UseUsers) // 21.05.2019
          Columns += "UserId";
        if (DocTypes.UseSessionId)
          Columns += "SessionId";

        ResTable = UndoCon.FillSelect("UserActions", Columns,
          Filter, DBxOrder.ById);
        DataTools.SetPrimaryKey(ResTable, "Id");

        if (!String.IsNullOrEmpty(singleDocTypeName))
        {
          Int32 SingleDocTableId = DocTypes[singleDocTypeName].TableId;

          Int32[][] ActionIds = DataTools.GetBlockedIds(ResTable, 100);
          for (int i = 0; i < ActionIds.Length; i++)
          {
            DataTable TableDocs = UndoCon.FillUniqueColumnValues("DocActions", "UserActionId",
              new AndFilter(new ValueFilter("DocTableId", SingleDocTableId),
              new IdsFilter("UserActionId", ActionIds[i])));
            DataTools.SetPrimaryKey(TableDocs, "UserActionId");
            for (int j = 0; j < ActionIds[i].Length; j++)
            {
              Int32 Id = ActionIds[i][j];
              if (TableDocs.Rows.Find(Id) == null)
              {
                DataRow Row1 = ResTable.Rows.Find(Id);
                Row1.Delete();
              }
            }

          }

        }
      }

      // При передаче клиенту предотвращаем преобразование времени из-за разных часовых поясов
      SerializationTools.SetUnspecifiedDateTimeMode(ResTable);
      ResTable.AcceptChanges();

      return ResTable;
    }

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    public override DataTable GetUserActionDocTable(Int32 actionId)
    {
      CheckThread();

      if (actionId <= 0)
        throw new ArgumentException("Недопустимый идентификатор действия пользователя " + actionId.ToString());

      DataTable ResTable;
      using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {

        DBxFilter Filter = new ValueFilter("UserActionId", actionId);
        ResTable = UndoCon.FillSelect("DocActions", new DBxColumns("Id,DocTableId,DocId,Version,Action"),
          Filter, DBxOrder.ById);
      }

      // При передаче клиенту предотвращаем преобразование времени из-за разных часовых поясов
      SerializationTools.SetUnspecifiedDateTimeMode(ResTable);
      ResTable.AcceptChanges();

      return ResTable;
    }

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    public override DateTime? GetUserActionsLastTime(Int32 userId)
    {
      if (userId == 0)
        return null;

      CheckThread();

      if (Source.GlobalData.UndoDBEntry == null)
        throw new NullReferenceException("Не установлено свойство DBxRealDocProviderGlobal.UndoDBEntry");

      if (this.UserId == 0)
        throw new InvalidOperationException("Не установлено свойство DBxDocProvider.UserId");
      if (userId != this.UserId)
      {
        if (!ViewOtherUsersActionPermission)
          throw new DBxAccessException("Запрещен доступ на просмотр действий, выполненных другими пользователями");
      }

      object v;

      DBxFilter Filter = new ValueFilter("UserId", userId);

      using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        v = UndoCon.GetMaxValue("UserActions", "ActionTime", Filter);
      }

      DateTime? dt = DataTools.GetNullableDateTime(v);
      if (dt.HasValue)
        return DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
      else
        return null;
    }

    #endregion

    #region Доступ к версиям документов

    /// <summary>
    /// Получить таблицу версий строк документа.
    /// Возвращает таблицу с одной строкой
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="wantedDocVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    public override DataTable LoadDocDataVersion(string docTypeName, Int32 docId, int wantedDocVersion)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");

      if (Source.GlobalData.UndoDBEntry == null)
        throw new InvalidOperationException("История не ведется");

      CheckIsRealDocId(docId);
      if (wantedDocVersion < 1 || wantedDocVersion >= short.MaxValue)
        throw new ArgumentOutOfRangeException("wantedDocVersion", wantedDocVersion, "Недопустимый номер версии документа");

      using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        #region Поиск в таблице DocActions

        UndoAction CurrAction = InternalGetDocAction(docTypeName, docId, wantedDocVersion, UndoCon);

        #endregion

        DataTable Table;

        using (DBxCon MainCon = new DBxCon(Source.MainDBEntry))
        {
          int Version2 = DataTools.GetInt(MainCon.GetValue(docTypeName, docId, "Version2"));
          if (Version2 <= wantedDocVersion)
          {
            // Берем данные из основной таблицы
            Table = MainCon.FillSelect(docTypeName, GetColumns(docTypeName, null), new IdsFilter(docId));
          }
          else
          {
            // Берем данные из БД undo

            // В таблице данных undo нет столбцов Deleted и Version
            DBxColumns Columns1 = GetColumns(docTypeName, null);
            DBxColumns Columns2 = Columns1 - "DocId,Version,Deleted";

            // Находим в копии таблицы документов строку с последним значением Vesion2, меньшим или равным запрошенному
            Int32 LastCopyId = DataTools.GetInt(UndoCon.GetValuesForMax(docTypeName,
              DBxColumns.Id, "Version2", new AndFilter(
              new ValueFilter("DocId", docId), new ValueFilter("Version2", wantedDocVersion, CompareKind.LessOrEqualThan)))[0]);
            if (LastCopyId == 0)
              throw new BugException("В базе данных Undo в таблице \"" + docTypeName + "\" не найдена запись для DocId=" + docId.ToString() + " и Version2=" + Version2.ToString());

            DataTable Table1 = UndoCon.FillSelect(docTypeName, Columns2, new IdsFilter(LastCopyId));
            if (Table1.Rows.Count != 1)
              throw new BugException("В базе данных Undo в таблице \"" + docTypeName + "\" для ключевого поля Id=" + LastCopyId.ToString() + " возвращено неправильное число строк данных (" + Table1.Rows.Count.ToString() + ")");

            Table = GetTemplate(docTypeName, null);
            DataTools.CopyRowsToRows(Table1, Table, true, true);
            Table.Rows[0]["Id"] = docId;
            Table.Rows[0]["Deleted"] = (CurrAction == UndoAction.Delete);
          }

          Table.Rows[0]["Version"] = wantedDocVersion;
          Table.AcceptChanges();
          return Table;
        }
      }
    }

    /// <summary>
    /// Получить таблицу версий строк поддокументов.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="wantedDocVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    public override DataTable LoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int wantedDocVersion)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      if (String.IsNullOrEmpty(subDocTypeName))
        throw new ArgumentNullException("subDocTypeName");

      if (Source.GlobalData.UndoDBEntry == null)
        throw new InvalidOperationException("История не ведется");

      CheckIsRealDocId(docId);
      if (wantedDocVersion < 1 || wantedDocVersion >= short.MaxValue)
        throw new ArgumentOutOfRangeException("wantedDocVersion", wantedDocVersion, "Недопустимый номер версии документа");

      DataTable Table;
      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        DBxColumns Columns1 = GetColumns(docTypeName, subDocTypeName);
        Columns1 += "Deleted,Version2";
        Table = Con.FillSelect(subDocTypeName, Columns1,
          new AndFilter(new IdsFilter("DocId", docId), // включая помеченные на удаление
          new ValueFilter("StartVersion", wantedDocVersion, CompareKind.LessOrEqualThan))); // пропуская более новые документы
      }

      if (Table.Rows.Count == 0)
        return Table; // нет поддокументов

      using (DBxCon UndoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        DBxColumns Columns2 = GetColumns(docTypeName, subDocTypeName);
        Columns2 -= "DocId";
        Columns2 += "Deleted";


        foreach (DataRow Row in Table.Rows)
        {
          int Version2 = DataTools.GetInt(Row, "Version2");
          if (Version2 <= wantedDocVersion)
            continue; // актуальная версия

          Int32 SubDocId = DataTools.GetInt(Row, "Id");


          // Ищем последнюю версию в таблице Undo
          Int32 LastCopyId = DataTools.GetInt(UndoCon.GetValuesForMax(subDocTypeName,
            DBxColumns.Id,
            "Version2",
            new AndFilter(
              new ValueFilter("SubDocId", SubDocId),
              new ValueFilter("Version2", wantedDocVersion, CompareKind.LessOrEqualThan)))[0]);
          if (LastCopyId == 0)
          {
            if (DataTools.GetBool(Row, "Deleted"))
            {
              // 15.12.2017
              // Поддокумент был удален после запрошенной версии, но в истории нет записи.
              // Значит удаленная версия является действующей для запрошенной версии документа
              Row["Deleted"] = false;
              continue;
            }

            throw new DBxConsistencyException("В базе данных Undo в таблице \"" + subDocTypeName + "\" не найдена запись для SubDocId = " + SubDocId.ToString() + " и Version2 <= " + Version2.ToString());
          }

          DataTable Table1 = UndoCon.FillSelect(subDocTypeName, Columns2, new IdsFilter(LastCopyId));
          if (Table1.Rows.Count != 1)
            throw new BugException("В базе данных Undo в таблице \"" + subDocTypeName + "\" для ключевого поля Id=" + LastCopyId.ToString() + " возвращено неправильное число строк данных (" + Table1.Rows.Count.ToString() + ")");

          DataTools.CopyRowValues(Table1.Rows[0], Row, true);
          Table.Rows[0]["DocId"] = docId;
        }
      }

      // Убираем поддокументы, помеченные на удаление
      for (int i = Table.Rows.Count - 1; i >= 0; i--)
      {
        if (DataTools.GetBool(Table.Rows[i], "Deleted"))
          Table.Rows.RemoveAt(i);
      }

      Table.AcceptChanges();
      return Table;
    }

    private UndoAction InternalGetDocAction(string docTypeName, Int32 docId, int docVersion, DBxCon undoCon)
    {
      DBxDocType DocType = DocTypes[docTypeName];
      if (DocType == null)
        throw new ArgumentException("Неизвестный тип документов \"" + docTypeName + "\"");

      CheckIsRealDocId(docId);

      DBxFilter[] Filters = new DBxFilter[3];
      Filters[0] = new ValueFilter("DocTableId", DocType.TableId);
      Filters[1] = new ValueFilter("DocId", docId);
      Filters[2] = new ValueFilter("Version", docVersion);


      Int32 DocActionId = undoCon.FindRecord("DocActions", new AndFilter(Filters));
      if (DocActionId == 0)
        throw new BugException("В таблице DocActions не найдена запись для документа \"" + DocType.SingularTitle +
          "\" с DocId=" + docId.ToString() + " для версии " + docVersion.ToString());

      UndoAction CurrAction = (UndoAction)DataTools.GetInt(undoCon.GetValue("DocActions", DocActionId, "Action"));
      return CurrAction;
    }

    /// <summary>
    /// Возвращает все таблицы данных по документу. Возвращает таблицы документов (с одной строкой) и поддокументов,
    /// включая удаленные записи. В набор добавляются строки из таблиц UserActions и DocActions.
    /// В набор добавляются таблицы документов из базы данных истории. Чтобы отличить их от основных
    /// документов, перед именами таблиц добавляется префикс "Undo_".
    /// Метод предназначен для отладочных целей
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Набор таблиц</returns>
    public override DataSet LoadUnformattedDocData(string docTypeName, Int32 docId)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      DBxDocType DocType = DocTypes[docTypeName];
      if (DocType == null)
        throw new ArgumentException("Неизвестный вид документа \"" + docTypeName + "\"", "docTypeName");

      CheckIsRealDocId(docId);


      DataSet ds = new DataSet();

      #region Основная база данных

      using (DBxCon Con = new DBxCon(_Source.MainDBEntry))
      {
        ds.Tables.Add(Con.FillSelect(docTypeName, null, new ValueFilter("Id", docId)));
        foreach (DBxSubDocType SubDocType in DocType.SubDocs)
          ds.Tables.Add(Con.FillSelect(SubDocType.Name, null, new ValueFilter("DocId", docId)));
      }

      #endregion

      if (Source.GlobalData.UndoDBEntry != null)
      {
        using (DBxCon Con = new DBxCon(_Source.GlobalData.UndoDBEntry))
        {
          #region UserActions и DocActions

          DataTable TableDocActions = Con.FillSelect("DocActions", null,
            new AndFilter(new ValueFilter("DocTableId", DocType.TableId),
            new ValueFilter("DocId", docId)));
          Int32[] UserActionIds = DataTools.GetIdsFromColumn(TableDocActions, "UserActionId");
          if (UserActionIds.Length > 0)
            ds.Tables.Add(Con.FillSelect("UserActions", null, new IdsFilter("Id", UserActionIds)));
          else
            ds.Tables.Add(Con.CreateEmptyTable("UserActions", null));
          ds.Tables.Add(TableDocActions); // После UserActions

          #endregion

          #region Таблицы истории

          DataTable Table = Con.FillSelect(docTypeName, null, new ValueFilter("DocId", docId));
          Table.TableName = "Undo_" + Table.TableName;
          ds.Tables.Add(Table);
          foreach (DBxSubDocType SubDocType in DocType.SubDocs)
          {
            // В undo нет поля DocId.
            // Надо использовать идентификаторы поддокументов из основной таблицы
            Int32[] SubDocIds = DataTools.GetIds(ds.Tables[SubDocType.Name]);
            if (SubDocIds.Length > 0)
              Table = Con.FillSelect(SubDocType.Name, null, new IdsFilter("SubDocId", SubDocIds));
            else
              Table = Con.CreateEmptyTable(SubDocType.Name, null);
            Table.TableName = "Undo_" + Table.TableName;
            ds.Tables.Add(Table);
          }
        }

          #endregion
      }

      return ds;
    }

    #endregion

    #region Длительные блокировки

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docSel">Выборка документов, которую требуется заблокировать</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public override Guid AddLongLock(DBxDocSelection docSel)
    {
      CheckThread();

      DBxLongDocsLock Lock = new DBxLongDocsLock(this);
      Lock.Data.Init(docSel);
      Source.GlobalData.LongLocks.Add(Lock);
      return Lock.Guid;
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    public override bool RemoveLongLock(Guid lockGuid)
    {
      CheckThread();

      return Source.GlobalData.LongLocks.Remove(lockGuid);
    }

    #endregion

    #region Клонирование

    /// <summary>
    /// Создает копию DBxRealDocProvider, если это разрешено свойством CloningAllowed
    /// Этот метод является потокобезопасным
    /// </summary>
    /// <returns>Новый провайдер</returns>
    public override DBxDocProvider Clone()
    {
      if (!_CloningAllowed)
        throw new InvalidOperationException("Клонирование DBxRealDocProvider запрещено, т.к. свойство CloningAllowed не установлено");

      return new DBxRealDocProvider(Source, UserId, CurrentThreadOnly, SessionId);
    }

    /// <summary>
    /// Разрешение на клонирование провайдера
    /// По умолчанию клонирование запрещено
    /// </summary>
    public bool CloningAllowed { get { return _CloningAllowed; } set { _CloningAllowed = value; } }
    private bool _CloningAllowed;

    #endregion

    #region Пересчет вычисляемых полей

    private delegate void RecalcColumnsDelegate(string docTypeName, Int32[] docIds);

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документв</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    public override void RecalcColumns(string docTypeName, Int32[] docIds)
    {
      MethodInvokeExecProc.RunWithExecProc(new RecalcColumnsDelegate(RecalcColumns2), docTypeName, docIds);
    }

    private void RecalcColumns2(string docTypeName, Int32[] docIds)
    {
      DBxDocType DocType = DocTypes[docTypeName];
      if (DocType == null)
        throw new ArgumentException("Неизвестный вид документов \"" + docTypeName + "\"", "docTypeName");

      RecalcColumnsPermission Perm = Source.UserPermissions.GetLast<RecalcColumnsPermission>();
      if (Perm != null)
      {
        switch (Perm.Mode)
        {
          case RecalcColumnsPermissionMode.Disabled:
            throw new DBxAccessException("Запрещен пересчет вычисляемых полей");
          case RecalcColumnsPermissionMode.Selected:
            if (docIds == null)
              throw new DBxAccessException("Запрещен пересчет вычисляемых полей, если не указаны идентификаторы документов");
            break;
        }
      }


      if (!DocType.HasCalculatedColumns)
        return; // нет вычисляемых полей

      // Выполняем пересчет блоками по 100 документов

      if (docIds == null)
      {
        Int32 LastId;
        using (DBxCon Con = new DBxCon(Source.MainDBEntry))
        {
          LastId = DataTools.GetInt(Con.GetMaxValue(DocType.Name, "Id", null));
        }

        ISplash spl = ExecProc.CurrentProc.BeginSplash("Пересчет документов \"" + DocType.PluralTitle + "\" (" + LastId.ToString() + " шт.)");
        spl.PercentMax = LastId;
        spl.AllowCancel = true;
        for (Int32 FirstId = 1; FirstId <= LastId; FirstId += 100)
        {
          spl.Percent = FirstId - 1;
          Int32 LastId2 = Math.Min(LastId, FirstId + 99);
          Int32[] Ids = new Int32[LastId2 - FirstId + 1];
          for (int i = 0; i < Ids.Length; i++)
            Ids[i] = FirstId + i;
          RecalcColumns3(DocType, Ids);
        }
        ExecProc.CurrentProc.EndSplash();
      }
      else
      {
        ISplash spl = ExecProc.CurrentProc.BeginSplash("Пересчет документов \"" + DocType.PluralTitle + "\" (" + docIds.Length.ToString() + " шт.)");
        spl.PercentMax = docIds.Length;
        spl.AllowCancel = true;
        Int32[][] Ids2 = DataTools.GetBlockedArray<Int32>(docIds, 100);
        for (int i = 0; i < Ids2.Length; i++)
        {
          RecalcColumns3(DocType, Ids2[i]);
          spl.Percent += Ids2.Length;
        }
        ExecProc.CurrentProc.EndSplash();
      }
    }

    private void RecalcColumns3(DBxDocType docType, Int32[] ids)
    {
      if (ids.Length == 0)
        throw new ArgumentException("Ids.Length=0", "ids");

      CheckThread();

      // Проверяем права на запись документов этого вида
      if (DBPermissions.TableModes[docType.Name] != DBxAccessMode.Full)
        throw new DBxAccessException("Нет прав на запись документов \"" + docType.PluralTitle + "\"");

      // Загружаем существующие данные
      DBxDocSet DocSet = new DBxDocSet(this);
      DocSet[docType.Name].View(ids);
      DocSet.IgnoreAllLocks = true;

      // Права на запись для отдельных документов не проверяем
      // Вызываем обработчик BeforeWrite
      foreach (DBxSingleDoc Doc in DocSet[0])
        docType.PerformBeforeWrite(Doc, false, true);

      using (DBxCon Con = new DBxCon(Source.MainDBEntry))
      {
        // Установка блокировки на запись документов
        DBxShortDocsLock DBLock = new DBxShortDocsLock(this, DocSet.IgnoreAllLocks, DocSet.IgnoredLocks);
        DBLock.Data.Init(this, DocSet.DataSet);
        using (new ExecProcLockKey(DBLock))
        {
          foreach (DBxSingleDoc Doc in DocSet[0])
          {
            // Записываем основной документ
            Hashtable FieldPairs = new Hashtable();
            DBxColumns UsedColumnNames1 = null;
            bool HasPairs = AddUserFieldPairs(FieldPairs, Doc.Row, false, DBPermissions, Con, ref UsedColumnNames1, Doc.DocType.Struct);
            if (HasPairs)
              Con.SetValues(docType.Name, Doc.DocId, FieldPairs);

            // Записываем поддокументы
            for (int i = 0; i < Doc.DocType.SubDocs.Count; i++)
            {
              string SubDocTypeName = Doc.DocType.SubDocs[i].Name;
              if (!Doc.MultiDocs.SubDocs.ContainsModified(SubDocTypeName))
                continue;
              DBxSingleSubDocs sds = Doc.SubDocs[SubDocTypeName];
              if (sds.SubDocCount == 0)
                continue;

              DBxTableStruct ts = sds.SubDocs.SubDocType.Struct;

              DBxColumns UsedColumnNames2 = null;

              foreach (DBxSubDoc SubDoc in sds)
              {
                if (!SubDoc.IsDataModified)
                  continue; // ничего не поменялось
                FieldPairs = new Hashtable();
                HasPairs = AddUserFieldPairs(FieldPairs, SubDoc.Row, false, DBPermissions, Con, ref UsedColumnNames2, SubDoc.SubDocType.Struct);
                if (HasPairs)
                  Con.SetValues(sds.SubDocs.SubDocType.Name, SubDoc.SubDocId, FieldPairs);
              }
            }
          }
        }
      }
    }

    #endregion

    #region Ссылки на документы

    /// <summary>
    /// Получить набор таблиц для просмотра ссылок на один документ.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, на который ищутся ссылки</param>
    /// <param name="docId">Идентификатор документа, на который ищутся ссылки</param>
    /// <param name="showDeleted">Надо ли включать в таблицу ссылки из удаленных документов и поддокументов,
    /// а также ссылки на удаленные поддокументы выбранного документа.
    /// Не имеет значение, помечен ли сам документ <paramref name="docId"/> на удаление (за исключением ссылок документа на самого себя</param>
    /// <param name="unique">Если true, то будет возвращено только по одной ссылке из каждого документа.
    /// Это рекомендуемый вариант для показа таблицы ссылок. Если false, то в таблице может быть несколько ссылок из одного исходного документа</param>
    /// <param name="fromSingleDocTypeName">Единственный вид документа, из которого берутся ссылки. Если не задано, то берутся ссылки из всех документов</param>
    /// <param name="fromSingleDocId">Идентификатор единственного документа, из которого берутся ссылки. Если 0, то берутся ссылки из всех документов</param>
    /// <returns>Таблица ссылок</returns>
    public override DataTable GetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      DBxDocType ToDT = DocTypes[docTypeName];
      if (ToDT == null)
        throw new ArgumentException("Неизвестный вид документа \"" + docTypeName + "\", на ссылки на который должны быть найдены", "docTypeName");

      DBxDocType FromSingleDT = null;
      if (!String.IsNullOrEmpty(fromSingleDocTypeName))
      {
        FromSingleDT = DocTypes[fromSingleDocTypeName];
        if (FromSingleDT == null)
          throw new ArgumentException("Неизвестный вид документа \"" + fromSingleDocTypeName + "\", на ссылки c которых должны быть найдены", "fromSingleDocTypeName");
      }

      CheckIsRealDocId(docId);
      if (fromSingleDocId != 0)
      {
        if (FromSingleDT == null)
          throw new ArgumentException("Идентификатор единственного документа, ссылки в котором ищутся, не может задаваться без вида документов", "fromSingleDocId");
        CheckIsRealDocId(fromSingleDocId);
      }

      DataTable ResTable;

      using (DBxCon Con = new DBxCon(Source.MainDBEntry))
      {
        DocRefTableHelper Helper = new DocRefTableHelper(this, ToDT, docId, showDeleted, unique, FromSingleDT, fromSingleDocId, Con);
        Helper.Run();
        ResTable = Helper.ResTable;
      }

      return ResTable;

      // Возвращается только одна таблица ссылок.
      // В окне информации о документе, на вкладке просмотра ссылок используется двухтабличный просмотр.
      // Данные для верхней таблицы (виды документов и статистика) формируются на стороне клиента
      // из таблицы ссылок
    }

    /// <summary>
    /// Чтобы не таскать список параметров при построении таблицы в GetDocRefTable(), используем вспомогательный класс
    /// </summary>
    private class DocRefTableHelper
    {
      #region Конструктор

      public DocRefTableHelper(DBxRealDocProvider owner, DBxDocType toDT, Int32 docId, bool showDeleted, bool unique, DBxDocType fromSingleDT, Int32 fromSingleDocId, DBxCon con)
      {
        _Owner = owner;
        _ToDT = toDT;
        _DocId = docId;
        _ShowDeleted = showDeleted;
        _Unique = unique;
        _FromSingleDT = fromSingleDT;
        _FromSingleDocId = fromSingleDocId;
        _Con = con;

        #region Заготовка таблицы

        // Имена полей, начинающиеся с "From", задают документы и поддокументы, в которых есть ссылка
        // Имена полей, начинающиеся с "To", задают поддокументы в документе DocId, на которые выполняется ссылка
        _ResTable = new DataTable();
        _ResTable.Columns.Add("FromDocTableId", typeof(Int32));
        _ResTable.Columns.Add("FromDocId", typeof(Int32));
        _ResTable.Columns.Add("FromDocDeleted", typeof(bool));

        _ResTable.Columns.Add("FromSubDocName", typeof(string)); // Поля пустые, если ссылка идет из документа 
        _ResTable.Columns.Add("FromSubDocId", typeof(Int32));
        _ResTable.Columns.Add("FromSubDocDeleted", typeof(bool));
        _ResTable.Columns.Add("FromDeleted", typeof(bool)); //  FromDocDeleted || FromSubDocDeleted

        _ResTable.Columns.Add("FromColumnName", typeof(string));

        _ResTable.Columns.Add("ToSubDocName", typeof(string)); // Поля заполняются, если ссылка идет на поддокумент
        _ResTable.Columns.Add("ToSubDocId", typeof(Int32));
        _ResTable.Columns.Add("ToSubDocDeleted", typeof(bool));

        _ResTable.Columns.Add("IsSameDoc", typeof(bool)); // true, если документ ссылается сам на себя
        _ResTable.Columns.Add("_InternalCount", typeof(int)); // для внутреннего использования

        #endregion
      }

      #endregion

      #region Внешние параметры

      public DBxRealDocProvider Owner { get { return _Owner; } }
      private DBxRealDocProvider _Owner;

      public DBxDocType ToDT { get { return _ToDT; } }
      private DBxDocType _ToDT;

      public Int32 DocId { get { return _DocId; } }
      private Int32 _DocId;

      public bool ShowDeleted { get { return _ShowDeleted; } }
      private bool _ShowDeleted;

      public bool Unique { get { return _Unique; } }
      private bool _Unique;

      public DBxDocType FromSingleDT { get { return _FromSingleDT; } }
      private DBxDocType _FromSingleDT;

      public Int32 FromSingleDocId { get { return _FromSingleDocId; } }
      private Int32 _FromSingleDocId;

      public DBxCon Con { get { return _Con; } }
      private DBxCon _Con;

      #endregion

      #region Результат

      public DataTable ResTable { get { return _ResTable; } }
      private DataTable _ResTable;

      #endregion

      #region Заполнение таблицы

      public void Run()
      {
        DBxDocTypeRefInfo[] Infos = _Owner.DocTypes.GetToDocTypeRefs(_ToDT.Name);
        for (int i = 0; i < Infos.Length; i++)
        {
          if (FromSingleDT != null && Infos[i].FromDocType != FromSingleDT)
            continue;

          Int32[] ToIds;
          if (Infos[i].ToSubDocType == null)
            ToIds = new Int32[1] { DocId };
          else
          {
            List<DBxFilter> Filters = new List<DBxFilter>();
            Filters.Add(new ValueFilter("DocId", DocId));
            if (!ShowDeleted)
              Filters.Add(DBSSubDocType.DeletedFalseFilter);
            DBxFilter SubDocFilter = AndFilter.FromList(Filters);
            ToIds = Con.GetIds(Infos[i].ToSubDocType.Name, SubDocFilter).ToArray();

            if (ToIds.Length == 0)
              continue; // 16.08.2017
          }

          switch (Infos[i].RefType)
          {
            case DBxDocTypeRefType.Column:
              AddForColumn(Infos[i], ToIds);
              break;
            case DBxDocTypeRefType.VTRefernce:
              AddForVTRef(Infos[i], ToIds);
              break;
            default:
              throw new NotImplementedException("Неизвестный тип ссылки: " + Infos[i].RefType);
          }
        }

        int cnt = 0;
        foreach (DataRow Row in ResTable.Rows)
        {
          cnt++;
          Row["_InternalCount"] = cnt;
          Row["FromDeleted"] = DataTools.GetBool(Row, "FromDocDeleted") || DataTools.GetBool(Row, "FromSubDocDeleted");
          Int32 ThisFromDocTableId = DataTools.GetInt(Row, "FromDocTableId");
          Int32 ThisFromDocId = DataTools.GetInt(Row, "FromDocId");
          Row["IsSameDoc"] = (ThisFromDocTableId == ToDT.TableId && ThisFromDocId == DocId);
        }

        if (Unique)
        {
          // Предпочтительны живые ссылки а не удаленные. 
          ResTable.DefaultView.Sort = "FromDeleted,_InternalCount";
          _ResTable = DataTools.CreateUniqueTable(ResTable.DefaultView, "FromDocTableId,FromDocId");
        }
      }

      private void AddForColumn(DBxDocTypeRefInfo refInfo, Int32[] toIds)
      {
        DBxColumns FromColumns;
        if (refInfo.FromSubDocType == null)
        {
          FromColumns = new DBxColumns("Id");
          if (refInfo.ToSubDocType != null)
            FromColumns += refInfo.FromColumn.ColumnName; // 16.09.2018
          if (Owner.DocTypes.UseDeleted)
          {
            FromColumns += "Deleted";
            if (refInfo.ToSubDocType != null)
              FromColumns += (refInfo.FromColumn.ColumnName + ".Deleted"); // 16.09.2018
          }
        }
        else
        {
          FromColumns = new DBxColumns("Id,DocId");
          FromColumns += refInfo.FromColumn.ColumnName;
          if (Owner.DocTypes.UseDeleted)
          {
            FromColumns += "Deleted";
            FromColumns += "DocId.Deleted";
            FromColumns += (refInfo.FromColumn.ColumnName + ".Deleted");
          }
        }

        List<DBxFilter> Filters = new List<DBxFilter>();
        Filters.Add(new IdsFilter(refInfo.FromColumn.ColumnName, toIds));
        if ((!ShowDeleted) && Owner.DocTypes.UseDeleted)
        {
          if (refInfo.FromSubDocType == null)
          {
            Filters.Add(DBSDocType.DeletedFalseFilter);
          }
          else
          {
            Filters.Add(DBSSubDocType.DeletedFalseFilter);
            Filters.Add(DBSSubDocType.DocIdDeletedFalseFilter);
          }
        }
        if (FromSingleDocId != 0)
        {
          if (refInfo.FromSubDocType == null)
            Filters.Add(new ValueFilter(DBSDocType.Id, FromSingleDocId));
          else
            Filters.Add(new ValueFilter(DBSSubDocType.DocId, FromSingleDocId));
        }

        DataTable SrcTable = Con.FillSelect(refInfo.FromDocTypeBase.Name, FromColumns, AndFilter.FromList(Filters));

        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          DataRow ResRow = ResTable.NewRow();
          ResRow["FromDocTableId"] = refInfo.FromDocType.TableId;
          if (refInfo.FromSubDocType == null)
          {
            ResRow["FromDocId"] = SrcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
              ResRow["FromDocDeleted"] = SrcRow["Deleted"];
          }
          else
          {
            ResRow["FromDocId"] = SrcRow["DocId"];
            ResRow["FromSubDocId"] = SrcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
            {
              ResRow["FromDocDeleted"] = SrcRow["DocId.Deleted"];
              ResRow["FromSubDocDeleted"] = SrcRow["Deleted"];
            }
            ResRow["FromSubDocName"] = refInfo.FromSubDocType.Name;
          }

          ResRow["FromColumnName"] = refInfo.FromColumn.ColumnName;

          if (refInfo.ToSubDocType != null)
          {
            ResRow["ToSubDocName"] = refInfo.ToSubDocType.Name;
            ResRow["ToSubDocId"] = SrcRow[refInfo.FromColumn.ColumnName];
            if (Owner.DocTypes.UseDeleted)
              ResRow["ToSubDocDeleted"] = SrcRow[refInfo.FromColumn.ColumnName + ".Deleted"];
          }
          ResTable.Rows.Add(ResRow);
        }
      }

      private void AddForVTRef(DBxDocTypeRefInfo refInfo, Int32[] toIds)
      {
        throw new NotImplementedException();
      }

      #endregion
    }


    #endregion
  }
}
