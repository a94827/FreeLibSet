﻿// Part of FreeLibSet.
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
using FreeLibSet.Collections;

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
    /// Для <paramref name="source"/> вызывается <see cref="DBxRealDocProviderSource.SetReadOnly()"/>.
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
    /// Для <paramref name="source"/> вызывается <see cref="DBxRealDocProviderSource.SetReadOnly()"/>.
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
          throw new ArgumentException(Res.DBxDocProvider_Arg_UserIdNoUsers, "userId");
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

      NamedValues fixedInfo = new NamedValues();

      fixedInfo["DocTypes"] = source.GlobalData.DocTypes;

      fixedInfo["UserId"] = userId;
      fixedInfo["SessionId"] = sessionId;

      fixedInfo["DBIdentity"] = source.GlobalData.DBIdentity;

      fixedInfo["BinDataInfo"] = new DBxBinDataHandlerInfo(source.GlobalData.BinDataHandler);

      fixedInfo["UseDocHist"] = source.GlobalData.UndoDBEntry != null;

      fixedInfo["MainDocTableServiceColumns"] = source.GlobalData.MainDocTableServiceColumns;
      fixedInfo["SubDocTableServiceColumns"] = source.GlobalData.SubDocTableServiceColumns;
      fixedInfo["AllDocServiceColumns"] = source.GlobalData.AllDocServiceColumns;
      fixedInfo["AllSubDocServiceColumns"] = source.GlobalData.AllSubDocServiceColumns;
      fixedInfo["DBPermissions"] = source.MainDBEntry.Permissions;
      fixedInfo["UserPermissions"] = source.UserPermissions.AsXmlText;

      fixedInfo.SetReadOnly();
      return fixedInfo;
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
    /// Источник данных.
    /// Источник данных недоступен как public, т.к. DocProvider может передаваться клиенту по ссылке
    /// </summary>
    internal DBxRealDocProviderSource Source { get { return _Source; } }
    private readonly DBxRealDocProviderSource _Source;

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
    /// Не использовать в пользовательском коде.
    /// </summary>
    public Guid DebugGuid { get { return _DebugGuid; } }
    private readonly Guid _DebugGuid;

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
    protected override DataTable DoLoadDocData(string docTypeName, Int32[] docIds)
    {
      return DoLoadDocData(docTypeName, new IdsFilter(docIds));
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoLoadDocData(string docTypeName, DBxFilter filter)
    {
      CheckThread();

      DBxTableStruct tblStruct = DocTypes[docTypeName].Struct;
      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        DataTable tbl = con.FillSelect(docTypeName, GetColumns(docTypeName, null), filter);
        InitMaxLength(tblStruct, tbl);
        tbl.AcceptChanges(); // 06.08.2018
        return tbl;
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
    protected override DataTable DoLoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      CheckThread();

      DBxTableStruct tblStruct = DocTypes[docTypeName].SubDocs[subDocTypeName].Struct;
      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        DBxFilter filter = new IdsFilter("DocId", docIds);
        if (DocTypes.UseDeleted) // 06.08.2018
          filter = new AndFilter(filter, DBSSubDocType.DeletedFalseFilter);

        DataTable tbl = con.FillSelect(subDocTypeName, GetColumns(docTypeName, subDocTypeName),
          filter);
        InitMaxLength(tblStruct, tbl);
        tbl.AcceptChanges(); // 06.08.2018
        return tbl;
      }
    }


    /// <summary>
    /// Установка свойcтв <see cref="DataColumn.MaxLength"/>.
    /// Таблица <paramref name="table"/> содержит столбцы, для которых устанавливаются ограничения.
    /// Если в таблице есть посторонние столбцы, они пропускаются.
    /// </summary>
    /// <param name="ts">Описание структуры таблицы</param>
    /// <param name="table">Таблица со столбцами, для которых требуется установить свойства</param>
    public static void InitMaxLength(DBxTableStruct ts, DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      foreach (DataColumn column in table.Columns)
      {
        int p = ts.Columns.IndexOf(column.ColumnName);
        if (p < 0)
          continue; // служебное поле
        DBxColumnStruct colDef = ts.Columns[p];
        if (colDef.ColumnType == DBxColumnType.String && column.DataType == typeof(string))
          column.MaxLength = colDef.MaxLength;
        /* Нельзя !
        if (FieldDef.Type==DatabaseStruct.FieldType.String ||
            FieldDef.Type==DatabaseStruct.FieldType.Date ||
            FieldDef.Type==DatabaseStruct.FieldType.Time)
          Column.AllowDBNull=FieldDef.CanBeEmpty;
        */
      }
    }


    /// <summary>
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    protected override DataTable DoFillSelect(DBxSelectInfo info)
    {
      CheckThread();
      // System.Threading.Thread.Sleep(100000);

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.FillSelect(info);
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
    protected override DataTable DoFillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.FillUniqueColumnValues(tableName, columnName, where);
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
    protected override string[] DoGetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueStringValues(tableName, columnName, where);
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
    protected override int[] DoGetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueIntValues(tableName, columnName, where);
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
    protected override long[] DoGetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueInt64Values(tableName, columnName, where);
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
    protected override float[] DoGetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueSingleValues(tableName, columnName, where);
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
    protected override double[] DoGetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueDoubleValues(tableName, columnName, where);
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
    protected override decimal[] DoGetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueDecimalValues(tableName, columnName, where);
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
    protected override DateTime[] DoGetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueDateTimeValues(tableName, columnName, where);
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
    protected override Guid[] DoGetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetUniqueGuidValues(tableName, columnName, where);
      }
    }

    #region GetRecordCount() и IsTableEmpty()

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    protected override int DoGetRecordCount(string tableName)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetRecordCount(tableName);
      }
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    protected override int DoGetRecordCount(string tableName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetRecordCount(tableName, where);
      }
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    protected override bool DoIsTableEmpty(string tableName)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.IsTableEmpty(tableName);
      }
    }

    #endregion

    /// <summary>
    /// Загрузить страницы таблицы кэша
    /// </summary>
    /// <param name="request">Параметры запроса</param>
    /// <returns>Кэш страницы</returns>
    protected override DBxCacheLoadResponse DoLoadCachePages(DBxCacheLoadRequest request)
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
    protected override void DoClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
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
    protected override Int32 DoFindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.FindRecord(tableName, where, orderBy);
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
    protected override Int32 DoFindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.FindRecord(tableName, where, singleOnly);
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
    protected override bool DoGetValue(string tableName, Int32 id, string columnName, out object value)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetValue(tableName, id, columnName, out value);
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
    protected override object[] DoGetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetValues(tableName, id, columnNames);
      }
    }

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    protected override IdList DoGetIds(string tableName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetIds(tableName, where);
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
    protected override object DoGetMinValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetMinValue(tableName, columnName, where);
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
    protected override object DoGetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetMaxValue(tableName, columnName, where);
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
    protected override object[] DoGetValuesForMax(string tableName, DBxColumns columnNames, string maxColumnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetValuesForMax(tableName, columnNames, maxColumnName, where);
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
    protected override object[] DoGetValuesForMin(string tableName, DBxColumns columnNames, string minColumnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetValuesForMin(tableName, columnNames, minColumnName, where);
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
    protected override object DoGetSumValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetSumValue(tableName, columnName, where);
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
    protected override IdList DoGetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      CheckThread();

      using (DBxCon con = new DBxCon(_Source.MainDBEntry))
      {
        return con.GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
      }
    }

    #endregion

    #region Права пользователя

    /// <summary>
    /// Возвращает DBxRealDocProviderSource.UserPermissions
    /// </summary>
    protected override UserPermissions DoGetUserPermissions()
    {
      return _Source.UserPermissions;
    }

    #endregion

    #region Кэш

    /// <summary>
    /// Кэш базы данных.
    /// Возвращает DBxRealDocProviderSource.DBCache
    /// Возвращаемый объект является потокобезопасным
    /// </summary>
    protected override DBxCache DoGetDBCache()
    {
      return _Source.DBCache;
    }

    /// <summary>
    /// Сброс кэшированных данных
    /// </summary>
    protected override void DoClearCache()
    {
      CheckThread();

      base.DoClearCache(); // испр. 12.07.2022
      _Source.ClearCache();
    }

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    protected override string DoGetTextValue(string tableName, Int32 id)
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
    protected override string DoInternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
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
    protected override Int32 DoInternalFindBinData(string md5)
    {
      CheckBinDataHandler();
      return _Source.GlobalData.BinDataHandler.FindBinData(md5);
    }

    private void CheckBinDataHandler()
    {
      if (_Source.GlobalData.BinDataHandler == null)
        throw new NullReferenceException(Res.DBxDocProvider_Err_NoBinDataHandler);
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
    protected override Dictionary<Int32, byte[]> DoInternalGetBinData2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
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
    protected override Int32 DoInternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 dataId)
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
    protected override Dictionary<Int32, FileContainer> DoInternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
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

      int size = fc.Content.Length;
      if (size < MaxBinDataSize)
      {
        for (int i = 0; i < preloadIds.Count; i++)
        {
          if (dict.ContainsKey(preloadIds[i].DataId))
            continue; // один повтор может быть - с wantedId

          if (GetDocColumnAccess(tableName, columnName, preloadIds[i].DocId, preloadIds[i].SubDocId, docVersion))
          {
            fc = _Source.GlobalData.BinDataHandler.GetDBFile(preloadIds[i].DataId);
            if (size > (MaxBinDataSize - fc.Content.Length))
              break; // получается, что SQL-запрос выполнен зря.
            size += fc.Content.Length;
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
          throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownTable, tableName), "tableName");
      }

      // 25.04.2020
      // Не нужно!
      //CheckIsRealDocId(docId);
      if (sdt == null)
      {
        if (subDocId != 0)
          throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_SubDocIdForDoc, tableName), "subDocId");
      }
      //else
      //{
      //  CheckIsRealDocId(subDocId);
      //}

      // Открываем документ на просмотр, чтобы убедиться в наличии прав доступа
      DBxDocSet docSet = new DBxDocSet(this);
      DBxSingleDoc doc;
      if (docId < 0)
        doc = docSet[dt.Name].Insert();
      else
      {
        if (docVersion == 0)
          doc = docSet[dt.Name].View(docId); // тут может быть DBxAccessException
        else
          doc = docSet[dt.Name].ViewVersion(docId, docVersion); // 26.04.2017
      }

      if (sdt == null)
      {
        // Само значение идентификатора нам не нужно. Все равно оно могло уже устареть.
        // Главное, что у нас есть право на получение этого значения
        object dummyValue = doc.Values[columnName].Value;
      }
      else
      {
        DBxSubDoc subDoc;
        if (subDocId < 0)
        {
          if (docId > 0 && docVersion == 0) // 02.09.2018
            doc.Edit(); // переключаемся на редактирование
          subDoc = doc.SubDocs[sdt.Name].Insert();
        }
        else
          subDoc = doc.SubDocs[sdt.Name].GetSubDocById(subDocId);
        object dummyValue = subDoc.Values[columnName].Value;
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

    #region Процедура ExecProc

    /// <summary>
    /// Не используется в пользовательском коде
    /// </summary>
    /// <returns></returns>
    protected override DistributedCallData DoStartServerExecProc(NamedValues args)
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
        this.DisplayName = "RealDocProviderExecProc for " + docProvider.ToString();
        base.SetContext(NamedValues.Empty);
      }

      private DBxRealDocProvider _DocProvider;

      #endregion

      #region Переопределенные методы

      protected override NamedValues OnExecute(NamedValues args)
      {
        NamedValues dispRes = new NamedValues();
        string action = args.GetString("Action");
        base.ActionName = action; // 11.01.2023

        string tableName, columnName;
        DocSubDocDataId wantedId;
        int docVersion;
        List<DocSubDocDataId> preloadIds;


        switch (action)
        {
          case "FillSelect": // 19.08.2020
            DBxSelectInfo selInfo = (DBxSelectInfo)(args["SelectInfo"]);
            base.ActionName += " (" + selInfo.TableName + ")";
            dispRes["Table"] = _DocProvider.FillSelect(selInfo);
            break;

          case "ApplyChanges":
            dispRes["DataSet"] = _DocProvider.ApplyChangesInternal(/*this,*/ (DataSet)(args["DataSet"]), args.GetBool("ReloadData"));
            break;

          case "RecalcColumns":
            _DocProvider.RecalcColumns(args.GetString("DocTypeName"), (Int32[])(args["DocIds"]));
            break;

          case "InternalGetBinData2": // 14.10.2020
            tableName = args.GetString("TableName");
            columnName = args.GetString("ColumnName");
            wantedId = (DocSubDocDataId)(args["WantedId"]);
            docVersion = args.GetInt("DocVersion");
            preloadIds = (List<DocSubDocDataId>)(args["PreloadIds"]);
            base.ActionName += " (" + tableName + "." + columnName + " " + wantedId.ToString() + ", preloadIds:" + preloadIds.Count + ")";
            dispRes["Data"] = _DocProvider.InternalGetBinData2(tableName,
              columnName, wantedId, docVersion, preloadIds);
            break;
          case "InternalGetDBFile2": // 14.10.2020
            tableName = args.GetString("TableName");
            columnName = args.GetString("ColumnName");
            wantedId = (DocSubDocDataId)(args["WantedId"]);
            docVersion = args.GetInt("DocVersion");
            preloadIds = (List<DocSubDocDataId>)(args["PreloadIds"]);
            base.ActionName += " (" + tableName + "." + columnName + " " + wantedId.ToString() + ", preloadIds:" + preloadIds.Count + ")";
            dispRes["Data"] = _DocProvider.InternalGetDBFile2(tableName,
              columnName, wantedId, docVersion, preloadIds);
            break;

          default:
            throw new ArgumentException("Unknown Action=" + action.ToString(), "args");
        }
        return dispRes;
      }

      #endregion
    }

    #endregion

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
    protected override DataSet DoApplyChanges(DataSet dataSet, bool reloadData)
    {
      CheckThread();

      // Нам нужно использовать блокировки
      // Если метод вызван не из процедуры ExecProc, создаем временную процедуру
      ExecProc currProc = ExecProc.CurrentProc;
      if (currProc == null)
      {
        using (RealDocProviderExecProc proc2 = new RealDocProviderExecProc(this))
        {
          NamedValues dispArgs = new NamedValues();
          dispArgs["Action"] = "ApplyChanges";
          dispArgs["DataSet"] = dataSet;
          dispArgs["ReloadData"] = reloadData;
          NamedValues DispRes = proc2.Execute(dispArgs);
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
      /// Значения null в массиве используются, если набор свойств таблицы пустой
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
          throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_WrongTableCount,
            ds.Tables.Count, _TableProps.Length), "ds"); // 20.06.2021

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
      ExtendedPropertiesSaver oldProps = new ExtendedPropertiesSaver(ds);
      try
      {
        return ApplyChangesInternal2(/*caller, */ds, reloadData);
      }
      catch
      {
        try
        {
          oldProps.Restore(ds); // 24.11.2019
        }
        catch (Exception e2) // 20.06.2021
        {
          LogoutTools.LogoutException(e2, LogoutTools.GetTitleForCall("ExtendedPropertiesSaver.Restore()"));
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
      TimeSpan editTime = TimeSpan.Parse(ds.ExtendedProperties["EditTime"].ToString());
      DateTime startTime = DateTime.Now - editTime;


      // Извлекаем параметры
      DBxDocSet docSet = new DBxDocSet(this, ds);
      if (this.UserId == 0 && DocTypes.UseUsers)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "UserId");

      RestoreAddedState(ds); // 19.03.2016

      // TODO: Когда будет доступ к тексту документа на сервере, перенести этот код после вызова события BeforeWrite
      if (String.IsNullOrEmpty(docSet.ActionInfo) && docSet.UserActionId == 0)
        docSet.ActionInfo = MakeActionInfo(docSet);

      #endregion

      // Установка соединений
      using (DBxConArray cons = new DBxConArray(
        Source.MainDBEntry, // с правами текущего пользователя
        Source.GlobalData.MainDBEntry, // с полными правами
        Source.GlobalData.UndoDBEntry))
      {
        DBxCon mainConUser = cons[0];
        DocUndoHelper undoHelper = new DocUndoHelper(cons[1], cons[2],
        this.UserId, this.SessionId, docSet.UserActionId, docSet.ActionInfo, DocTypes, startTime);

        // Установка блокировки на запись документов
        DBxShortDocsLock dbLock = new DBxShortDocsLock(this, docSet.IgnoreAllLocks, docSet.IgnoredLocks);
        dbLock.Data.Init(this, ds);
        using (new ExecProcLockKey(dbLock))
        {
          Source.PerformBeforeApplyChanges(docSet); // 15.11.2016

          // 31.01.2022. Загрузка исходных документов
          DBxDocSelection docSel = docSet.GetDocSelection(new DBxDocState[] { DBxDocState.Insert, DBxDocState.Edit, DBxDocState.Delete });
          DBxDocSet orgDocSet = new DBxDocSet(this);
          orgDocSet.UseTestDocument = false; // иначе будут лишние вызовы тестирования для режима View.
          orgDocSet.View(docSel);

          // Вызов обработчиков DBxDocType
          foreach (DBxMultiDocs docs in docSet)
          {
#if DEBUG
            if (docs.DocType.TableId == 0)
              throw new BugException("For documents of type " + docs.DocType.ToString() + ", property TableId is not set");
#endif

            for (int i = 0; i < docs.DocCount; i++)
            {
              DBxSingleDoc doc = docs[i];
              DBxSingleDoc orgDoc = new DBxSingleDoc();
              if (this.IsRealDocId(doc.DocId) && doc.DocState != DBxDocState.View)
                orgDoc = orgDocSet[docs.DocType.Name].GetDocById(doc.DocId);
              CallDocTypeEventsForDoc(doc, orgDoc);
            }
          }

          // Удаляем лишние записи из таблиц BinData и FileNames
          docSet.InternalDeleteUnusedBinDataAndFiles();

          // Вызываем AppendBinData() и заменяем фиктивные ссылки
          Dictionary<Int32, Int32> binDataReplaces = CallAppendBinData(ds);
          ReplaceAppendBinData(ds, binDataReplaces);
          // Вызываем AppendDBFile() и заменяем фиктивные ссылки
          Dictionary<Int32, Int32> fileNameReplaces = CallAppendDBFiles(ds);
          ReplaceAppendDBFiles(ds, fileNameReplaces);

          #region Проверка возможности удаления документов и поддокументов

          foreach (DBxMultiDocs multiDocs in docSet)
            ApplyDocDelete1(mainConUser, multiDocs);

          #endregion

          // Актуализация undo
          ActualizeUndo(docSet, undoHelper, mainConUser);

          #region Выполняем реальную запись

          using (DBxTransactionArray ta = new DBxTransactionArray(cons[2], cons[0]))
          {
            #region Замена фиктивных идентификаторов Id для новых документов / поддокументов на реальные, которые будут записаны

            DBxDocProviderIdReplacer idReplacer = new DBxDocProviderIdReplacer(this);
            idReplacer.PerformReplace(docSet, ds);

            #endregion

            WriteChanges2(docSet, mainConUser, undoHelper, idReplacer);

            WriteDelayedColumns(idReplacer.DelayedList, ds, mainConUser);

            ValidateRefs(docSet, mainConUser); // должно быть после WriteDelayedColumns()

            if (docSet.CheckDocs)
              DoCheckDocs(docSet);

            ta.Commit();
          }

          docSet.UserActionId = undoHelper.UserActionId;

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
      PerformAfterChange(docSet);

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
      foreach (DBxMultiDocs docs in docSet)
      {
        string actName;
        switch (docSet.DocStateNoView)
        {
          case DBxDocState.Insert:
            actName = Res.DBxDocProvider_Msg_ActionInfoInsert;
            break;
          case DBxDocState.Edit:
            actName = Res.DBxDocProvider_Msg_ActionInfoEdit;
            break;
          case DBxDocState.Delete:
            actName = Res.DBxDocProvider_Msg_ActionInfoDelete;
            break;
          case DBxDocState.Mixed:
            actName = Res.DBxDocProvider_Msg_ActionInfoMixed;
            break;
          default:
            continue;
        }

        if (sb.Length > 0)
          sb.Append(", ");

        if (docs.DocCount == 1)
          sb.Append(String.Format(Res.DBxDocProvider_Msg_ActionInfoSingleDoc,actName, docs.DocType.SingularTitle));
        else
          sb.Append(String.Format(Res.DBxDocProvider_Msg_ActionInfoMultiDocs, actName, docs.DocType.PluralTitle, docs.DocCount));
      }

      if (sb.Length == 0)
        sb.Append(Res.DBxDocProvider_Msg_ActionInfoEmpty);
      return sb.ToString();
    }

    /// <summary>
    /// Переводит все строки в состоянии Modified и с Id меньше 0 в состояние Added.
    /// Такие строки поддокументов могут возникнуть, если поддокумент был создан, а затем изменен до записи в базу данных
    /// </summary>
    /// <param name="ds"></param>
    private static void RestoreAddedState(DataSet ds)
    {
      foreach (DataTable table in ds.Tables)
      {
        foreach (DataRow row in table.Rows)
        {
          if (row.RowState == DataRowState.Modified)
          {
            if (DataTools.GetInt(row, "Id") < 0)
              DataTools.SetRowState(row, DataRowState.Added);
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
        throw new NullReferenceException(Res.DBxDocProvider_Err_NoBinDataHandler);
#endif

      Dictionary<Int32, Int32> dict = new Dictionary<Int32, Int32>();
      foreach (DataRow row in tblBinData.Rows)
      {
        Int32 oldId = (Int32)(row["Id"]);
#if DEBUG
        if (oldId >= 0)
          throw new BugException("In the 'BinData' there is a row with Id=" + oldId.ToString() + ". Negative identifier expected");
#endif
        byte[] binData = (byte[])(row["Contents"]);
        Int32 newId = _Source.GlobalData.BinDataHandler.AppendBinData(binData);
        dict.Add(oldId, newId);
      }

      return dict;
    }

    private void ReplaceAppendBinData(DataSet ds, Dictionary<Int32, Int32> binDataReplaces)
    {
      if (binDataReplaces == null)
        return;

      foreach (DataTable table in ds.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocTypes.FindByTableName(table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.BinDataRefs.Count; i++)
        {
          int p = table.Columns.IndexOf(dt.BinDataRefs[i].Column.ColumnName);

          for (int j = 0; j < table.Rows.Count; j++)
          {
            if (table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 id = DataTools.GetInt(table.Rows[j][p]);
            Int32 newId;
            if (binDataReplaces.TryGetValue(id, out newId))
              table.Rows[j][p] = newId;
          }
        }
      }

      #region Замена в FileNames

      DataTable tblFileNames = ds.Tables["FileNames"];
      if (tblFileNames != null)
      {
        foreach (DataRow row in tblFileNames.Rows)
        {
          Int32 oldDataId = DataTools.GetInt(row, "Data");
          Int32 newDataId;
          if (binDataReplaces.TryGetValue(oldDataId, out newDataId))
            row["Data"] = newDataId;
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
        throw new NullReferenceException(Res.DBxDocProvider_Err_NoBinDataHandler);
#endif

      Dictionary<Int32, Int32> dict = new Dictionary<Int32, Int32>();
      foreach (DataRow row in tblFileNames.Rows)
      {
        Int32 oldId = (Int32)(row["Id"]);
#if DEBUG
        if (oldId >= 0)
          throw new BugException("In the table 'FileNames' there is a row with Id=" + oldId.ToString() + ". Negative identifier expected");
#endif
        Int32 dataId = DataTools.GetInt(row, "Data");
        if (dataId <= 0)
          throw new BugException("В таблице FileNames присутствует строка, в которой Data=" + dataId.ToString());

        StoredFileInfo fileInfo = new StoredFileInfo(DataTools.GetString(row, "Name"),
          DataTools.GetInt(row, "Length"),
          DataTools.GetNullableDateTime(row, "CreationTime"),
          DataTools.GetNullableDateTime(row, "LastWriteTime"));

        Int32 newId = _Source.GlobalData.BinDataHandler.AppendDBFile(fileInfo, dataId);
        dict.Add(oldId, newId);
      }

      return dict;
    }

    private void ReplaceAppendDBFiles(DataSet ds, Dictionary<Int32, Int32> fileNameReplaces)
    {
      if (fileNameReplaces == null)
        return;

      foreach (DataTable table in ds.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocTypes.FindByTableName(table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.FileRefs.Count; i++)
        {
          int p = table.Columns.IndexOf(dt.FileRefs[i].Column.ColumnName);

          for (int j = 0; j < table.Rows.Count; j++)
          {
            if (table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 id = DataTools.GetInt(table.Rows[j][p]);
            Int32 newId;
            if (fileNameReplaces.TryGetValue(id, out newId))
              table.Rows[j][p] = newId;
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
          throw new NullReferenceException("Uninitialized orgDoc for DocId=" + doc.DocId.ToString());
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
          throw new NullReferenceException("Uninitialized orgDoc for DocId=" + doc.DocId.ToString());
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

      foreach (DBxMultiDocs multiDocs in docSet)
      {
        for (int i = 0; i < multiDocs.DocCount; i++)
        {
          DBxSingleDoc doc = multiDocs[i];
          switch (doc.DocState)
          {
            case DBxDocState.Edit:
            case DBxDocState.Delete:
              if (multiDocs.GetDocIdActionId(doc.DocId) == 0)
              {
                // Первое обращение к документу - требуется проверка таблицы DocActions
                undoHelper.ValidateDocVersion(multiDocs.DocType, doc.DocId);
                if (doc.DocState == DBxDocState.Edit && (doc.IsDataModified || docSet.WriteIfNotChanged))
                {
                  int docVersion = DataTools.GetInt(mainConUser.GetValue(multiDocs.DocType.Name, doc.DocId, "Version"));
                  if (docVersion == 0) // Исправляем ошибку
                    docVersion = 1;
                  if (doc.IsMainDocModified)
                    undoHelper.ActualizeMainDoc(multiDocs.DocType, doc.DocId, docVersion);
                  for (int j = 0; j < doc.SubDocs.Count; j++)
                  {
                    foreach (DBxSubDoc subDoc in doc.SubDocs[j])
                    {
                      // 19.03.2016
                      if (subDoc.SubDocId < 0)
                        continue; // сначала поддокумент добавили, затем сразу удалили или изменили

                      switch (subDoc.SubDocState)
                      {
                        case DBxDocState.Edit:
                          if (subDoc.IsDataModified)
                            undoHelper.ActualizeSubDoc(subDoc.SubDocType, subDoc.SubDocId, docVersion);
                          break;
                        case DBxDocState.Delete:
                          // при удалении тоже актуализация
                          undoHelper.ActualizeSubDoc(subDoc.SubDocType, subDoc.SubDocId, docVersion);
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
      // 03.02.2022
      // Собираем идентификаторы, которые нужно реально удалять.
      // Ключ - имя таблицы документа или поддокумента
      // Значение - удаляемые идентификаторы
      RealDelList realDel = null;
      if (!DocTypes.UseDeleted)
        realDel = new RealDelList();

      // Прерывать процесс записи, начиная с этой стадии, нельзя
      for (int typeIndex = 0; typeIndex < docSet.Count; typeIndex++)
      {
        DBxMultiDocs multiDocs = docSet[typeIndex];
        DBxColumns usedColumnNames = null;
        for (int docIndex = 0; docIndex < multiDocs.DocCount; docIndex++)
        {
          DBxSingleDoc doc = multiDocs[docIndex];

          switch (doc.DocState)
          {
            case DBxDocState.Insert:
            case DBxDocState.Edit:
              ApplyDocChanges2(multiDocs.DocType, mainConUser, undoHelper, doc, idReplacer, ref usedColumnNames, realDel);
              break;
            case DBxDocState.Delete:
              ApplyDocDelete2(multiDocs, mainConUser, undoHelper, doc.DocId, realDel);
              break;
          }
        }
      }

      if (!DocTypes.UseDeleted)
        ApplyRealDelete(mainConUser, realDel);
    }

    /// <summary>
    /// Второй проход - Обработка записи для одного документа в режимах Insert и Edit
    /// </summary>
    private void ApplyDocChanges2(DBxDocType docType, DBxCon mainConUser, DocUndoHelper undoHelper,
      DBxSingleDoc doc,
      DBxDocProviderIdReplacer idReplacer, ref DBxColumns usedColumnNames1,
      RealDelList realDel)
    {
      //  SingleDocChangesInfo SingleChanges=MultiChanges.Docs[DocIndex];
      Int32 docId = doc.DocId; // идентификатор документа

      // Это первый вызов ApplyChanges для этого документа ?
      bool firstCall = (doc.MultiDocs.GetDocIdActionId(doc.DocId) == 0);
      // Будет ли выполняться добавление строки в основную таблицу данных ?
      bool docRealAdded = doc.DocState == DBxDocState.Insert && firstCall;

      bool isModified;
      if (doc.DocState == DBxDocState.Edit && doc.DocSet.WriteIfNotChanged)
        isModified = true; // 17.02.2022
      else
        isModified = doc.IsDataModified;

      // Определяем, есть ли какие-нибудь изменения
      // В режиме вставки изменения не имеют значения
      // Должно идти после обработчиков DocType, т.к. они могут вносить 
      // дополнительные изменения
      if ((!docRealAdded) && (!isModified) && (!doc.Deleted))
        return;

      // Текущая версия документа
      int currVersion = 0;
      if (DocTypes.UseVersions)
      {
        if (!docRealAdded)
          currVersion = DataTools.GetInt(mainConUser.GetValue(docType.Name, docId, "Version"));
      }

      if (firstCall)
      {
        UndoAction action;
        if (doc.DocState == DBxDocState.Edit)
          action = UndoAction.Edit;
        else
          action = UndoAction.Insert;
        Int32 docActionId = undoHelper.AddDocAction(docType, docId, action, ref currVersion);
        // Сообщаем о действии
        doc.MultiDocs.SetDocIdActionId(docId, docActionId);
      }

#if DEBUG
      if (DocTypes.UseVersions)
      {
        if (doc.DocState == DBxDocState.Insert)
        {
          if (firstCall)
          {
            // 05.02.2018
            // При повторных вызовах сохранения созданного документа, версия не обязана сохраняться.
            // Она сохраняется, только если пользователь держит длительную блокировку.
            // Однако, это не является обязательным, и другой пользователь может переписать документ

            if (currVersion != 1)
            {
              Exception e = new BugException("When document is created, it must have Version=1, but it is " + currVersion.ToString());
              e.Data["FirstCall"] = firstCall;
              e.Data["DocRealAdded"] = docRealAdded;
              AddExceptionInfo(e, doc);
              throw e;
            }
          }
        }
        else
        {
          if (currVersion < 2)
          {
            Exception e = new BugException("When document is changed or removed it must have Version greater than 1, but it is " + currVersion.ToString());
            AddExceptionInfo(e, doc);
            e.Data["FirstCall"] = firstCall;
            e.Data["DocRealAdded"] = docRealAdded;
            throw e;
          }
        }
      }
#endif

      // Формируем поля для основной записи
      Hashtable fieldPairs = new Hashtable();

      bool hasPairs = AddUserFieldPairs(fieldPairs, doc.Row, docRealAdded, DBPermissions, mainConUser, ref usedColumnNames1, doc.DocType.Struct);

      // Добавляем значения служебных полей
      if (doc.DocState == DBxDocState.Insert && firstCall)
      {
        // Первое действие для данного документа
        if (DocTypes.UseUsers)
          fieldPairs.Add("CreateUserId", undoHelper.UserId);
        if (DocTypes.UseTime)
          fieldPairs.Add("CreateTime", undoHelper.ActionTime);
        // TODO: int ImportId = DataTools.GetInt(row, "ImportId");
        // TODO: if (ImportId != 0)
        // TODO: FieldPairs.Add("ImportId", ImportId);
      }
      else // Edit
      {
        if (DocTypes.UseUsers)
          fieldPairs.Add("ChangeUserId", undoHelper.UserId);
        if (DocTypes.UseTime)
          fieldPairs.Add("ChangeTime", undoHelper.ActionTime); // пусть запишет также при повторном вызове записи для Insert
      }
      if (DocTypes.UseVersions)
      {
        fieldPairs.Add("Version", currVersion);
        doc.Row["Version"] = currVersion;
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
        fieldPairs.Add("Deleted", false); // Отменяем удаление
        doc.Row["Deleted"] = false;
      }

      if (DocTypes.UseVersions)
      {
#if DEBUG
        if (currVersion < 1)
        {
          Exception e = new BugException("CurrVersion=0");
          AddExceptionInfo(e, doc);
          throw e;
        }
#endif

        if (doc.DocState == DBxDocState.Insert || hasPairs)
          fieldPairs.Add("Version2", currVersion);
      }

      // TODO: FieldPairs.Add("CheckState", DocumentCheckState.Unchecked); // Отменяем результаты проверки
      //if (DocsChanges.MainTable.Columns.Contains("CheckState"))
      // TODO: row["CheckState"] = DocumentCheckState.Unchecked;
      // При первом добавлении требуется писать все данные, а потом
      // - только измененные данные


      // Выполняем добавление записи или обновление
      if (docRealAdded)
      {
        fieldPairs.Add("Id", docId);
        mainConUser.AddRecord(docType.Name, fieldPairs);
      }
      else
      {
        // TODO: if (DocType.Buffering != null)
        // TODO:   DocType.Buffering.ClearSinceId(Caller, DocId);

        if (fieldPairs.Count > 0) // 01.02.2022
          mainConUser.SetValues(docType.Name, docId, fieldPairs);
      }

      // Записываем поддокументы
      for (int i = 0; i < doc.DocType.SubDocs.Count; i++)
      {
        string subDocTypeName = doc.DocType.SubDocs[i].Name;
        if (!doc.MultiDocs.SubDocs.ContainsModified(subDocTypeName))
          continue;

        DBxSingleSubDocs sds = doc.SubDocs[subDocTypeName];
        if (sds.SubDocCount == 0)
          continue;

        DBxTableStruct ts = sds.SubDocs.SubDocType.Struct;

        DBxColumns usedColumnNames2 = null;

        foreach (DBxSubDoc subDoc in sds)
        {
          switch (subDoc.SubDocState)
          {
            case DBxDocState.Insert:
              bool subDocRealAdded = idReplacer.IsAdded(subDocTypeName, subDoc.SubDocId);
              SubApplyChange1(subDoc, mainConUser, ts, subDocRealAdded, currVersion, ref usedColumnNames2);
              break;
            case DBxDocState.Edit:
              if (!subDoc.IsDataModified)
                continue; // ничего не поменялось
              SubApplyChange1(subDoc, mainConUser, ts, false, currVersion, ref usedColumnNames2);
              break;
            case DBxDocState.Delete:
              SubApplyDelete1(subDoc, mainConUser, currVersion, realDel);
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

      bool res = false;

      if (row.RowState == DataRowState.Added)
      {
        #region Режим добавления

        for (int i = 0; i < usedColumnNames.Count; i++)
        {
          object newValue = row[usedColumnNames[i]];
          DBxColumnStruct colDef = tableStruct.Columns[usedColumnNames[i]];

          // 09.12.2015: Добавление всех полей при повторном вызове для добавляемой записи
          //             У нас нет оригинальной версии строки для DataRowState.Added
          // 17.12.2015: Код переработан
          // 16.08.2021: Учитывается DBxColumnStruct.DefaultValue

          if (realAdd)
          {
            if (colDef.Nullable)
            {
              if (DataTools.IsEmptyValue(newValue))
                continue; // 07.08.2018
            }
            else if (!Object.ReferenceEquals(colDef.DefaultValue, null)) // дополнительное условие добавлено 19.09.2021
            {
              if (newValue is DBNull)
                continue; // 16.08.2021
            }
          }
          if (newValue is DBNull && (!colDef.Nullable))
          {
            if (colDef.DefaultValue == null)
              newValue = DataTools.GetEmptyValue(colDef.DataType); // 07.08.2018
            else
              newValue = colDef.DefaultValue; // 16.08.2021
          }

          fieldPairs.Add(usedColumnNames[i], newValue);
          res = true;
        }

        #endregion
      }
      else // Row.RowState == DataRowState.Modified
      {
        #region Режим обновления

        // 14.07.2016
        // Исходные значения берем не из версии строки Original, переданной снаружи,
        // а запрашиваем данные из базы данных
        Int32 id = (Int32)(row["Id"]);
#if DEBUG
        if (id <= 0)
          throw new BugException("Wrong Id=" + id.ToString());
#endif
        object[] orgValues = mainConUser.GetValues(row.Table.TableName, id, usedColumnNames);

        for (int i = 0; i < usedColumnNames.Count; i++)
        {
          DBxColumnStruct colDef = tableStruct.Columns[usedColumnNames[i]];

          object newValue = row[usedColumnNames[i], DataRowVersion.Current];
          if (newValue is DBNull && (!colDef.Nullable))
            newValue = DataTools.GetEmptyValue(colDef.DataType); // 01.09.2018
          // object OrgValue = Row[i, DataRowVersion.Original];
          object orgValue = orgValues[i];

          if (!DataTools.AreValuesEqual(orgValue, newValue))
          {
            if (dbPermissions.ColumnModes[row.Table.TableName, usedColumnNames[i]] != DBxAccessMode.Full)
            {
              DBxAccessException e = new DBxAccessException(String.Format(Res.DBxDocPrivider_Err_SetValueDenied,
                usedColumnNames[i], row.Table.TableName));
              e.Data["TableName"] = row.Table.TableName;
              e.Data["Id"] = row["Id"];
              e.Data["ColumnnNane"] = usedColumnNames[i];
              if (orgValue is DBNull)
                e.Data["OrgValue"] = "NULL";
              else
                e.Data["OrgValue"] = orgValue;
              if (newValue is DBNull)
                e.Data["NewValue"] = "NULL";
              else
                e.Data["NewValue"] = newValue;
              throw e;
            }

            fieldPairs.Add(usedColumnNames[i], newValue);
            res = true;
          }
        }

        #endregion
      }

      return res;
    }

    private void SubApplyChange1(DBxSubDoc subDoc, DBxCon mainConUser, DBxTableStruct tableDef, bool added, int docVersion, ref DBxColumns usedColumnNames)
    {
      Hashtable fieldPairs = new Hashtable();
      // Служебные поля
      if (added)
      {
        fieldPairs.Add("DocId", subDoc.Doc.DocId);
        if (DocTypes.UseVersions)
          fieldPairs.Add("StartVersion", docVersion);
      }

      if (DocTypes.UseDeleted)
        fieldPairs.Add("Deleted", false);
      // Основные поля
      bool hasPairs = AddUserFieldPairs(fieldPairs, subDoc.Row, added, DBPermissions, mainConUser, ref usedColumnNames, subDoc.SubDocType.Struct);

      if (added || hasPairs)
      {
        if (DocTypes.UseVersions)
          fieldPairs.Add("Version2", docVersion);
      }

      if (added)
      {
        fieldPairs.Add("Id", subDoc.SubDocId);
        mainConUser.AddRecord(tableDef.TableName, fieldPairs);
      }
      else
      {
        if (fieldPairs.Count > 0) // 07.03.2022
          mainConUser.SetValues(tableDef.TableName, subDoc.SubDocId, fieldPairs);
      }
    }

    private void SubApplyDelete1(DBxSubDoc subDoc, DBxCon mainConUser, int docVersion,
      RealDelList realDel)
    {
      Int32 subDocId = (Int32)(subDoc.Row["Id", DataRowVersion.Original]);
#if DEBUG
      CheckIsRealDocId(subDocId);
#endif
      if (DocTypes.UseDeleted)
      {
        if (DocTypes.UseVersions)
          mainConUser.SetValues(subDoc.SubDocType.Name, subDocId, new DBxColumns("Deleted,Version2"),
            new object[] { true, docVersion });
        else
          mainConUser.SetValue(subDoc.SubDocType.Name, subDocId, "Deleted", true); // 01.02.2022
      }
      else
      {
        realDel[subDoc.SubDocType.Name, false].Add(subDocId);
      }
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
          delayedList[i].ColumnName, delayedList[i].Value);

        DataTable table = ds.Tables[delayedList[i].TableName];
        DataRow row = table.Rows.Find(delayedList[i].Id);
        row[delayedList[i].ColumnName] = delayedList[i].Value;
      }
    }

    #endregion

    #region Проверка корректности ссылок

    /// <summary>
    /// Проверка ссылочных полей, которые есть в наборе <paramref name="docSet"/>.
    /// В случае некорректных ссылок выбрасывается исключение.
    /// Проверяются, в том числе, ссылки на документы, внешние по отношению к набору. Для этого посылаются запросы в базу данных.
    /// Проверяются обычные ссылочные поля и "переменные ссылки".
    /// На момент вызова уже определены идентификаторы новых строк в наборе. Фиктивные идентификаторы заменены на настоящие (которых еще нет, однако, в базе данных).
    /// Выполняется внутри транзакции записи документов.
    /// </summary>
    /// <param name="docSet">Набор данных для записи</param>
    /// <param name="mainConUser">Соединение с основной базой данных</param>
    private void ValidateRefs(DBxDocSet docSet, DBxCon mainConUser)
    {
      // При UseDeleted=false можно не проверять идентификаторы для обычных ссылочных полей, т.к.
      // СУБД проверяет целостность ссылок.
      // При DBxDocType.UseDeleted=true проверка нужна, чтобы не ссылаться на документы, помеченные на удаление.
      // Для VTRef надо проверять в любом случае.


      #region 1. Поиск неправильных идентификаторов и сбор правильных

      // Ключ - имя мастер-таблицы
      // Значение - идентификаторы в мастер таблице
      Dictionary<string, IdList> refDict = new Dictionary<string, IdList>();

      foreach (DBxMultiDocs multiDocs in docSet)
      {
        ValidateTableRefsInternal(multiDocs.Table, multiDocs.DocType, refDict);
        foreach (DBxSubDocType sdt in multiDocs.DocType.SubDocs)
        {
          if (multiDocs.SubDocs.ContainsSubDocs(sdt.Name))
            ValidateTableRefsInternal(multiDocs.SubDocs[sdt.Name].Table, sdt, refDict);
        }
      }

      #endregion

      #region 2. Проверка всех идентификаторов

      // Эти ссылки проверять не надо
      if (Source.GlobalData.BinDataHandler != null)
      {
        refDict.Remove("BinData");
        refDict.Remove("FileNames");
      }

      foreach (KeyValuePair<string, IdList> pair in refDict)
      {
        IdList ids = pair.Value;

        #region Удаляем ссылки, которые есть в нашем наборе

        if (docSet.DataSet.Tables.Contains(pair.Key))
        {
          DataTable tbl = docSet.DataSet.Tables[pair.Key];
          foreach (DataRow row in tbl.Rows)
          {
            switch (row.RowState)
            {
              case DataRowState.Added:
              case DataRowState.Modified:
                Int32 id = (Int32)(row["Id"]);
#if DEBUG
                CheckIsRealDocId(id);
#endif
                ids.Remove(id);
                break;
            }
          }
        }

        #endregion

        #region Выполняем запрос

        if (ids.Count > 0)
        {
          DBxDocTypeBase dtb = DocTypes.FindByTableName(pair.Key);
          IdsFilterGenerator idsGen = new IdsFilterGenerator(ids);
          idsGen.CreateFilters();
          for (int i = 0; i < idsGen.Count; i++)
          {
            Int32[] wantedIds = idsGen.GetIds(i);

            DBxFilter where = idsGen[i];
            if (dtb != null) // может быть, ссылка на таблицу BinData
              AddDeletedFilters(ref where, dtb.IsSubDoc);
            IdList realIds = mainConUser.GetIds(pair.Key, where);
            if (realIds.Count < wantedIds.Length)
            {
              // Не хватает
              realIds.Remove(wantedIds);
              throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_RefIdsNotInDB,
                pair.Key, UICore.UITools.ValueListToString(realIds)));
            }
          }
        }

        #endregion
      }

      #endregion
    }

    private void ValidateTableRefsInternal(DataTable table, DBxDocTypeBase dtb, Dictionary<string, IdList> refDict)
    {
      #region Обычные ссылки

      if (DocTypes.UseDeleted)
      {
        for (int i = 0; i < dtb.Struct.Columns.Count; i++)
        {
          DBxColumnStruct col = dtb.Struct.Columns[i];
          if (!String.IsNullOrEmpty(col.MasterTableName))
          {
            int pCol = table.Columns.IndexOf(col.ColumnName);
            if (pCol < 0)
              continue;

            foreach (DataRow row in table.Rows)
            {
              switch (row.RowState)
              {
                case DataRowState.Added:
                case DataRowState.Modified:
                  Int32 refId = DataTools.GetInt(row[pCol]);
                  if (refId < 0)
                    throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_WrongFictiveRefId,
                      table.TableName, col.ColumnName, refId));
                  if (refId > 0)
                  {
                    IdList ids;
                    if (!refDict.TryGetValue(col.MasterTableName, out ids))
                    {
                      ids = new IdList();
                      refDict.Add(col.MasterTableName, ids);
                    }
                    ids.Add(refId);
                  }
                  break;
              }
            }
          }
        }
      }

      #endregion

      #region Переменные ссылки

      for (int i = 0; i < dtb.VTRefs.Count; i++)
      {
        DBxVTReference vtr = dtb.VTRefs[i];
        int pTableIdCol = table.Columns.IndexOf(vtr.TableIdColumn.ColumnName);
        int pDocIdCol = table.Columns.IndexOf(vtr.DocIdColumn.ColumnName);
        if (pTableIdCol < 0 && pDocIdCol < 0)
          continue;
        if (pTableIdCol < 0 || pDocIdCol < 0)
          throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_IncompleteVTRef, table.TableName, vtr.Name));

        foreach (DataRow row in table.Rows)
        {
          switch (row.RowState)
          {
            case DataRowState.Added:
            case DataRowState.Modified:
              Int32 refDocId = DataTools.GetInt(row[pDocIdCol]);
              Int32 refTableId = DataTools.GetInt(row[pTableIdCol]);
              if (refTableId == 0 && refDocId == 0)
                continue;
              if (refTableId == 0 || refDocId == 0)
                throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_VTRefIncompleteIds,
                  table.TableName, vtr.Name, vtr.TableIdColumn.ColumnName, refTableId, vtr.DocIdColumn.ColumnName, refDocId));

              if (refTableId < 0)
                throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_VTRefFictiveTable,
                  table.TableName, vtr.Name, vtr.TableIdColumn.ColumnName, refTableId));

              DBxDocType masterDT = DocTypes.FindByTableId(refTableId);
              if (masterDT == null)
                throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_VTRefUnknownDocType,
                  table.TableName, vtr.Name, vtr.TableIdColumn.ColumnName, refTableId));

              if (!vtr.MasterTableNames.Contains(masterDT.Name))
                throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_VTRefDisabledDocType,
                  table.TableName, vtr.Name, vtr.TableIdColumn.ColumnName, refTableId, masterDT.PluralTitle,
                  DataTools.JoinNotEmptyStrings(", ", vtr.MasterTableNames)));

              if (refDocId < 0)
                throw new InvalidOperationException(String.Format(Res.DBxDocPrivider_Err_VTRefUnknownDocId,
                  table.TableName, vtr.Name, vtr.TableIdColumn.ColumnName, refDocId));

              IdList ids;
              if (!refDict.TryGetValue(masterDT.Name, out ids))
              {
                ids = new IdList();
                refDict.Add(masterDT.Name, ids);
              }
              ids.Add(refDocId);

              break;
          }
        }
      }

      #endregion
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

      #region 1. Удаляемые документы

      Int32[] deletedDocIds = multiDocs.GetDocIds(DBxDocState.Delete);
      if (deletedDocIds.Length > 0)
      {
        // Проверка удаления документа
        IdsFilterGenerator docFltGen = new IdsFilterGenerator(deletedDocIds);

        ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, multiDocs.DocType, docFltGen);
        // Проверка удаления всех поддокументов
        // Проверяем наличие непустых ExtRefs
        bool hasSubDocsExtRefs = false;
        for (int i = 0; i < multiDocs.SubDocs.Count; i++)
        {
          if (!Source.GlobalData.MasterRefs[multiDocs.SubDocs[i].SubDocType.Name].IsEmpty)
          {
            hasSubDocsExtRefs = true;
            break;
          }
        }
        if (hasSubDocsExtRefs)
        {
          docFltGen.CreateFilters("DocId");
          for (int j = 0; j < docFltGen.Count; j++)
          {
            for (int i = 0; i < multiDocs.SubDocs.Count; i++)
            {
              DBxSubDocType subDocType = multiDocs.SubDocs[i].SubDocType;

              DBxFilter filter = docFltGen[j];
              if (DocTypes.UseDeleted) // 02.02.2022
                filter = new AndFilter(filter, DBSSubDocType.DeletedFalseFilter);
              IdList subDocIds = mainCon.GetIds(subDocType.Name, filter);

              IdsFilterGenerator subDocFltGen = new IdsFilterGenerator(subDocIds);
              ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, subDocType, subDocFltGen);
            }
          }
        }
      }

      #endregion

      #region 2. Редактируемые документы, в которых могут удаляться поддокументы

      Int32[] editDocIds = multiDocs.GetDocIds(DBxDocState.Edit);
      if (editDocIds.Length > 0)
      {
        // Проверяем удаляемые поддокументы в редактируемом документе
        // Проверяем наличие непустых ExtRefs
        bool hasSubDocsExtRefs = false;
        for (int i = 0; i < multiDocs.SubDocs.Count; i++)
        {
          if (!Source.GlobalData.MasterRefs[multiDocs.SubDocs[i].SubDocType.Name].IsEmpty)
          {
            hasSubDocsExtRefs = true;
            break;
          }
        }
        if (hasSubDocsExtRefs)
        {
          IdsFilterGenerator docFltGen = new IdsFilterGenerator(editDocIds);
          docFltGen.CreateFilters("DocId");
          for (int j = 0; j < docFltGen.Count; j++)
          {
            for (int i = 0; i < multiDocs.SubDocs.Count; i++)
            {
              DBxMultiSubDocs subDocs = multiDocs.SubDocs[i];
              DataRow[] rows = subDocs.Table.Select(docFltGen[j].ToString(), string.Empty, DataViewRowState.Deleted);
              // Так нельзя, т.к. строки помечены Deleted
              //Int32[] subDocIds = DataTools.GetIds(Rows);
              Int32[] subDocIds = new Int32[rows.Length];
              for (int k = 0; k < rows.Length; k++)
                subDocIds[k] = DataTools.GetInt(rows[k]["Id", DataRowVersion.Original]);

              IdsFilterGenerator subDocFltGen = new IdsFilterGenerator(subDocIds);

              ApplyDocsDelete1Table(mainCon, multiDocs.DocSet, subDocs.SubDocType, subDocFltGen);
            }
          }
        }
      }

      #endregion
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
      DBxExtRefs.TableRefList extRefList = Source.GlobalData.MasterRefs[delType.Name];

      #region 1. Проверяем ссылочные поля

      for (int i = 0; i < extRefList.RefColumns.Count; i++)
      {
        DBxExtRefs.RefColumnInfo refInfo = extRefList.RefColumns[i];
        ApplyDocsDelete1CheckRef(mainCon, docSet, delType, delFltGen, refInfo);
      }

      #endregion

      #region 2. Проверяем VTReferences

      for (int i = 0; i < extRefList.VTRefs.Count; i++)
      {
        DBxExtRefs.VTRefInfo refInfo = extRefList.VTRefs[i];
        if (delType.IsSubDoc)
          throw new BugException("VTReference to subdocument is not possible");
        ApplyDocsDelete1CheckVTRef(mainCon, docSet, (DBxDocType)delType, delFltGen, refInfo);
      }

      #endregion
    }

    /// <summary>
    /// Проверка для одного ссылочного поля
    /// </summary>
    /// <param name="mainCon"></param>
    /// <param name="docSet"></param>
    /// <param name="delType">Документы/поддокументы, которые предполагается удалять</param>
    /// <param name="delFltGen"></param>
    /// <param name="refInfo">Буферизованное описание ссылки</param>
    private void ApplyDocsDelete1CheckRef(DBxCon mainCon, DBxDocSet docSet, DBxDocTypeBase delType, IdsFilterGenerator delFltGen, DBxExtRefs.RefColumnInfo refInfo)
    {
      Int32[] allDelIds = delFltGen.GetAllIds(); // все идентификаторы удаляемых документов/поддокументов

      delFltGen.CreateFilters(refInfo.ColumnDef.ColumnName);
      DBxColumns checkDelColumns = refInfo.IsSubDocType ? _FieldsCheckDelForSubDoc : _FieldsCheckDelForDoc;

      DataTable detailsTable2 = docSet.DataSet.Tables[refInfo.DetailsTableName];
      //IdList DetailIds2 = new IdList();

      #region Первый проход - по записям в базе данных

      for (int j = 0; j < delFltGen.Count; j++)
      {
        DataTable detailsTable;
        try
        {
          DBxFilter Filter = delFltGen[j];
          AddDeletedFilters(ref Filter, refInfo.DetailsType.IsSubDoc);
          detailsTable = mainCon.FillSelect(refInfo.DetailsTableName, checkDelColumns, Filter);
        }
        catch (Exception e)
        {
          e.Data["RefColumnInfo"] = String.Format(Res.DBxDocPrivider_Err_DelRefColumnInfo,
            refInfo.DetailsTableName, delType.PluralTitle, refInfo.ColumnDef.ColumnName, e.Message);

          throw;
        }

        for (int k = 0; k < detailsTable.Rows.Count; k++)
        {
          if (detailsTable2 != null)
          {
            Int32 detailId = (Int32)(detailsTable.Rows[k]["Id"]);
            if (detailsTable2.Rows.Find(detailId) != null)
            {
              // detailIds2.Add(DetailId); // проверим  на втором заходе
              continue;
            }
          }
          ApplyDocDelete1CheckOne(mainCon, docSet, delType, allDelIds, refInfo.DetailsType, detailsTable.Rows[k]);
        }
      }

      #endregion

      #region Второй проход - по записям в DBxDocSet

      if (detailsTable2 != null)
      {
        // Проверять надо все записи
        IdList allDelIdList = new IdList(allDelIds);

        foreach (DataRow checkRow in detailsTable2.Rows)
        {
          if (checkRow.RowState == DataRowState.Deleted)
            continue;

          //Int32 CheckedId = (Int32)(CheckRow["Id"]);
          Int32 refId = DataTools.GetInt(checkRow, refInfo.ColumnDef.ColumnName);
          if (allDelIdList.Contains(refId))
            ApplyDocDelete1CheckOne(mainCon, docSet, delType, allDelIds, refInfo.DetailsType, checkRow);
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
      delFltGen.CreateFilters(refInfo.VTRef.DocIdColumn.ColumnName);
      DBxColumns checkDelColumns = refInfo.IsSubDocType ? _FieldsCheckDelForSubDoc : _FieldsCheckDelForDoc;

      DataTable checkTable2 = docSet.DataSet.Tables[refInfo.DetailsTableName];
      // IdList checkTable2UsedIds = new IdList(); // допускает и фиктивные идентификаторы

      #region Первый проход - по записям в базе данных

      for (int j = 0; j < delFltGen.Count; j++)
      {
        DBxFilter where = new AndFilter(new ValueFilter(refInfo.VTRef.TableIdColumn.ColumnName, delDocType.TableId),
            delFltGen[j]);
        AddDeletedFilters(ref where, refInfo.IsSubDocType); // 14.02.2022
        DataTable checkTable = mainCon.FillSelect(refInfo.DetailsTableName,
            checkDelColumns, where);

        for (int k = 0; k < checkTable.Rows.Count; k++)
        {
          if (checkTable2 != null)
          {
            Int32 checkedId = (Int32)(checkTable.Rows[k]["Id"]);
            if (checkTable2.Rows.Find(checkedId) != null)
            {
              // checkTable2UsedIds.Add(checkedId);
              continue; // проверим  на втором заходе
            }
          }
          ApplyDocDelete1CheckOne(mainCon, docSet, delDocType, delFltGen.GetIds(j), refInfo.DetailsType, checkTable.Rows[k]); // испр. 14.02.2022
        }
      }

      #endregion

      #region Второй проход - по записям в DBxDocSet

      // Проверка в DBxDocSet добавлена 14.02.2022

      DataTable detailsTable2 = docSet.DataSet.Tables[refInfo.DetailsTableName];
      if (detailsTable2 != null)
      {
        Int32[] allDelIds = delFltGen.GetAllIds(); // все идентификаторы удаляемых документов

        // Проверять надо все записи
        IdList allDelIdList = new IdList(allDelIds);

        foreach (DataRow checkRow in detailsTable2.Rows)
        {
          if (checkRow.RowState == DataRowState.Deleted)
            continue;

          //Int32 CheckedId = (Int32)(CheckRow["Id"]);
          Int32 tableId = DataTools.GetInt(checkRow, refInfo.VTRef.TableIdColumn.ColumnName);
          if (tableId == delDocType.TableId)
          {
            Int32 refId = DataTools.GetInt(checkRow, refInfo.VTRef.DocIdColumn.ColumnName);
            if (allDelIdList.Contains(refId))
              ApplyDocDelete1CheckOne(mainCon, docSet, delDocType, allDelIds, refInfo.DetailsType, checkRow);
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Проверка одной ссылки
    /// </summary>
    /// <param name="mainCon">Соединение для основной базы данных</param>
    /// <param name="docSet">Записываемый набор данных</param>
    /// <param name="delType">Описание удаляемого документа или поддокумента (master)</param>
    /// <param name="delIds">Массив идентификаторов удаляемых документов или поддокументов</param>
    /// <param name="chkType">Описание документа или поддокумента, на который выполняется ссылка</param>
    /// <param name="chkRow">Строка в детальной таблице, на которую проверяется ссылка (detail)</param>
    private void ApplyDocDelete1CheckOne(DBxCon mainCon, DBxDocSet docSet, DBxDocTypeBase delType, Int32[] delIds, DBxDocTypeBase chkType, DataRow chkRow)
    {
      // Удаленные документы / поддокументы пропускаем
      if (DocTypes.UseDeleted)
      {
        if (DataTools.GetBool(chkRow, "Deleted"))
          return;
        if (chkType.IsSubDoc)
        {
          if (DataTools.GetBool(chkRow, "DocId.Deleted"))
            return;
        }
      }

      Int32 chkDocId = DataTools.GetInt(chkRow, chkType.IsSubDoc ? "DocId" : "Id");

      // Проверяем, нет ли ссылающегося документа в списке удаляемых сейчас
      DBxDocType chkDocType = chkType.IsSubDoc ? ((DBxSubDocType)chkType).DocType : (DBxDocType)chkType;

      if (docSet.ContainsDocs(chkDocType.Name)) // без этой проверки в DocSet будет добавлен новый вид документов
      {
        DBxMultiDocs multiDocs = docSet[chkDocType.Name];

        if (multiDocs.IndexOfDocId(chkDocId) >= 0)
        {
          DBxSingleDoc chkDoc = multiDocs.GetDocById(chkDocId);

          if (chkDoc.DocState == DBxDocState.Delete)
            return; // документ удаляется

          if (chkType.IsSubDoc && chkDoc.DocState == DBxDocState.Edit)
          {
            Int32 chkSubDocId = DataTools.GetInt(chkRow, "Id");
            if (multiDocs.SubDocs.ContainsSubDocs(chkType.Name))
            {
              DBxMultiSubDocs subDocs = multiDocs.SubDocs[chkType.Name];

              DataRow[] rows = subDocs.Table.Select("Id=" + chkSubDocId.ToString(),
                String.Empty, DataViewRowState.Deleted);
              if (rows.Length > 0)
                return;
            }
          }
        }
      }

      // Нельзя удалять документ или поддокумент, т.к. на него имеются ссылки

      #region Выбрасывание DBxDocCannotDeleteException

      string txtChk;
      if (chkType.IsSubDoc)
      {
        Int32 chkSubDocId = DataTools.GetInt(chkRow, "Id");
        txtChk = String.Format(Res.DBxDocProvider_Msg_SingleSubDoc, GetDocOrSubDocText(chkType, chkSubDocId), GetDocOrSubDocText(chkDocType, chkDocId));
      }
      else
        txtChk = String.Format(Res.DBxDocProvider_Msg_SingleDoc, GetDocOrSubDocText(chkDocType, chkDocId));

      StringBuilder sb = new StringBuilder();
      if (delIds.Length == 1)
      {
        string txtMain;
        if (delType.IsSubDoc)
        {
          DBxDocType delDocType = ((DBxSubDocType)delType).DocType;
          Int32 delDocId = DataTools.GetInt(mainCon.GetValue(delType.Name, delIds[0], "DocId"));
          txtMain = String.Format(Res.DBxDocProvider_Msg_SingleSubDoc, GetDocOrSubDocText(delType, delIds[0]), GetDocOrSubDocText(delDocType, delDocId));
        }
        else
          txtMain = GetDocOrSubDocText(delType, delIds[0]);

        throw new DBxDocCannotDeleteException(String.Format(Res.DBxDocProvider_Err_DelMulti, txtMain, txtChk));

      }
      else
      {
        string txtMain;
        if (delType.IsSubDoc)
        {
          DBxDocType delDocType = ((DBxSubDocType)delType).DocType;
          txtMain = String.Format(Res.DBxDocProvider_Msg_MultiSubDocs, delType.PluralTitle, delDocType.SingularTitle);
        }
        else
          txtMain = String.Format(Res.DBxDocProvider_Msg_MultiDocs, delType.PluralTitle);
        throw new DBxDocCannotDeleteException(String.Format(Res.DBxDocProvider_Err_DelMulti, txtMain, txtChk));
      }

      #endregion
    }

    private string GetDocOrSubDocText(DBxDocTypeBase dtb, Int32 id)
    {
      string docText;
      if (IsRealDocId(id))
        docText = Source.GlobalData.TextHandlers.GetTextValue(dtb.Name, id);
      else
        docText = Res.DBxDocProvider_Msg_NewDocSubDoc;

      string idText;
      if (dtb.IsSubDoc)
        idText=", SubDocId=";
      else
        idText=", DocId=";
      idText += id.ToString();

      return String.Format(Res.DBxDocProvider_Msg_DocSubDocText, dtb.SingularTitle, docText, idText);
    }

    #endregion

    /// <summary>
    /// Хранение идентификаторов документов и поддокументов для реального удаления
    /// </summary>
    private class RealDelList
    {
      #region Конструктор

      public RealDelList()
      {
        _Dict = new Dictionary<string, TableInfo>();
      }

      #endregion

      #region Словарь

      /// <summary>
      /// Информация по одной таблице
      /// </summary>
      internal struct TableInfo
      {
        #region Поля

        /// <summary>
        /// Идентификаторы документов и поддокументов, которые надо удалять по полю "Id"
        /// </summary>
        public IdList DelById;

        /// <summary>
        /// Идентификаторы поддокументов, которые надо удалять по полю "DocId"
        /// </summary>
        public IdList DelByDocId;

        #endregion
      }

      public IdList this[string tableName, bool byDocId]
      {
        get
        {
          TableInfo ti;
          if (!_Dict.TryGetValue(tableName, out ti))
          {
            ti = new TableInfo();
            ti.DelById = new IdList();
            ti.DelByDocId = new IdList();
            _Dict.Add(tableName, ti);
          }
          if (byDocId)
            return ti.DelByDocId;
          else
            return ti.DelById;
        }
      }

      public Dictionary<string, TableInfo> Dict { get { return _Dict; } }
      private Dictionary<string, TableInfo> _Dict;

      #endregion
    }

    #region Выполнение удаления

    private void ApplyDocDelete2(DBxMultiDocs multiDocs, DBxCon mainConUser, DocUndoHelper undoHelper, Int32 docId, RealDelList realDel)
    {
      int currVersion = 0;
      if (DocTypes.UseVersions)
        currVersion = DataTools.GetInt(mainConUser.GetValue(multiDocs.DocType.Name, docId, "Version"));

      if (DocTypes.UseDeleted)
      {
        /*Int32 DocActionId = */
        undoHelper.AddDocAction(multiDocs.DocType, docId, UndoAction.Delete, ref currVersion);
        Hashtable fieldPairs = new Hashtable();
        // Добавляем значения изменяемых полей
        if (DocTypes.UseVersions)
          fieldPairs.Add("Version", currVersion);
        if (DocTypes.UseUsers) // 16.08.2018
          fieldPairs.Add("ChangeUserId", undoHelper.UserId);
        if (DocTypes.UseTime) // 01.02.2022
          fieldPairs.Add("ChangeTime", undoHelper.ActionTime);
        fieldPairs.Add("Deleted", true);
        mainConUser.SetValues(multiDocs.DocType.Name, docId, fieldPairs);
      }
      else
      {
        // 01.02.2022
        // Удаляем все поддокументы
        foreach (DBxSubDocType sdt in multiDocs.DocType.SubDocs)
          realDel[sdt.Name, true].Add(docId);

        realDel[multiDocs.DocType.Name, false].Add(docId);
      }

      DataRow row = multiDocs.GetDocById(docId).Row;

      // Нельзя устанавливать значения для удаленной строки напрямую
      row.RejectChanges();
      if (DocTypes.UseVersions)
        row["Version"] = currVersion;
      row.Delete();

      // Удаляем буферизованные остатки
      // TODO: if (DocType.Buffering != null)
      // TODO:   DocType.Buffering.ClearSinceId(Caller, DocId);
    }

    /// <summary>
    /// Выполнение реального удаления документов и поддокументов при UseDeleted=false
    /// </summary>
    /// <param name="mainConUser"></param>
    /// <param name="realDel"></param>
    private void ApplyRealDelete(DBxCon mainConUser, RealDelList realDel)
    {
      if (realDel.Dict.Count == 0)
        return;

      // Имена таблиц документов/поддокументов, которые надо удалять в первую очередь
      SingleScopeList<string> firstPriorityTableNames = new SingleScopeList<string>();

      foreach (KeyValuePair<string, RealDelList.TableInfo> pair in realDel.Dict)
      {
        #region Обнуление ссылок

        // Ссылки VTRef игнорируются, т.к. это не настоящие ссылки с точки зрения БД, их не надо обнулять
        // Нельзя использовать DBxExtRefs (поле _ExtRefs), т.к. там идет группировка по мастер-таблице, а
        // не по документу/поддокументу, содержащему ссылку
        DBxTableStruct str = DocTypes.FindByTableName(pair.Key).Struct;
        List<String> nullableCols = null;
        foreach (DBxColumnStruct colDef in str.Columns)
        {
          if (String.IsNullOrEmpty(colDef.MasterTableName))
            continue;

          // Обнуление имеет смысл только для ссылок на таблицы, которые есть среди удаляемых
          if (!realDel.Dict.ContainsKey(colDef.MasterTableName))
            continue; // в большинстве случае будут ссылки, которые никому не мешают

          if (colDef.Nullable)
          {
            if (nullableCols == null)
              nullableCols = new List<string>();
            nullableCols.Add(colDef.ColumnName);
          }
          else
            firstPriorityTableNames.Add(pair.Key); // нужно удалять в первую очередь
        }

        if (nullableCols != null)
        {
          Hashtable fieldPairs = new Hashtable();
          foreach (string colName in nullableCols)
            fieldPairs.Add(colName, null);

          if (pair.Value.DelById.Count > 0)
            mainConUser.SetValues(pair.Key, new IdsFilter("Id", pair.Value.DelById), fieldPairs);
          if (pair.Value.DelByDocId.Count > 0)
            mainConUser.SetValues(pair.Key, new IdsFilter("DocId", pair.Value.DelByDocId), fieldPairs);
        }

        #endregion
      }

      #region Вызов DELETE

      foreach (string tableName in firstPriorityTableNames)
        ApplyRealDeleteOneTable(mainConUser, tableName, realDel.Dict[tableName]);

      foreach (KeyValuePair<string, RealDelList.TableInfo> pair in realDel.Dict)
      {
        if (!firstPriorityTableNames.Contains(pair.Key))
          ApplyRealDeleteOneTable(mainConUser, pair.Key, pair.Value);
      }

      #endregion
    }

    private void ApplyRealDeleteOneTable(DBxCon mainConUser, string tableName, RealDelList.TableInfo tableInfo)
    {
      if (tableInfo.DelById.Count > 0)
        mainConUser.Delete(tableName, new IdsFilter("Id", tableInfo.DelById));
      if (tableInfo.DelByDocId.Count > 0)
        mainConUser.Delete(tableName, new IdsFilter("DocId", tableInfo.DelByDocId));
    }

    #endregion

    #endregion

    #region Вызов события DBxDocType.AfterChange

    /// <summary>
    /// Вызов события DBxDocType.AfterChange для каждого документа
    /// </summary>
    /// <param name="docSet"></param>
    private void PerformAfterChange(DBxDocSet docSet)
    {
      for (int typeIndex = 0; typeIndex < docSet.Count; typeIndex++)
      {
        DBxMultiDocs multiDocs = docSet[typeIndex];
        for (int docIndex = 0; docIndex < multiDocs.DocCount; docIndex++)
          multiDocs.DocType.PerformAfterChange(multiDocs[docIndex]);
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
    protected override DataTable DoGetDocHistTable(string docTypeName, Int32 docId)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
      DBxDocType docType = DocTypes[docTypeName];
      if (docType == null)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, docTypeName), "docTypeName");
      CheckIsRealDocId(docId);

      if (Source.GlobalData.UndoDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Source.GlobalData, "UndoDBEntry");

      #region Проверка прав доступа к истории

      // 1. Проверяем разрешение на просмотр таблицы документов
      if (DBPermissions.TableModes[docTypeName] == DBxAccessMode.None)
        throw DBxDocPermissions.GetException(DocTypes[docTypeName], DBxDocPermissionReason.View);

      // 2. Проверяем разрешение DocTypeViewHistoryPermission
      if (!DocTypeViewHistoryPermission.GetAllowed(UserPermissions, docTypeName))
        throw DBxDocPermissions.GetException(DocTypes[docTypeName], DBxDocPermissionReason.ViewHistory);

      // 3. Проверяем право на просмотр конкретного документа
      DBxDocSet docSet = new DBxDocSet(this);
      DBxSingleDoc doc = docSet[docTypeName].View(docId);

      // 4. Проверяем право на просмотр истории конкретно этого документа
      this.TestDocument(doc, DBxDocPermissionReason.ViewHistory);

      #endregion

      using (DBxCon mainCon = new DBxCon(Source.MainDBEntry))
      {
        using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
        {
          // Актуальная строка данных
          if (mainCon.FindRecord(docTypeName, "Id", docId) == 0)
            throw new BugException("Document row not found. DocId=" + docId.ToString());

          DBxColumns columns = new DBxColumns(
            "Id,UserActionId,Version,Action,UserActionId.StartTime,UserActionId.ActionTime,UserActionId.ActionInfo,UserActionId.ApplyChangesTime,UserActionId.ApplyChangesCount");
          if (DocTypes.UseUsers)
            columns += "UserActionId.UserId";
          if (DocTypes.UseSessionId)
            columns += "UserActionId.SessionId";

          DataTable tblDocActions = undoCon.FillSelect("DocActions", columns,
            new AndFilter(new ValueFilter("DocTableId", docType.TableId), new ValueFilter("DocId", docId)));

          // Добавляем текст
          DataRow[] baseRows = tblDocActions.Select("Action=" + ((int)(UndoAction.Base)));
          for (int i = 0; i < baseRows.Length; i++)
            baseRows[i]["UserActionId.ActionInfo"] = Res.UndoAction_Msg_Base;

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
    protected override DataTable DoGetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      CheckThread();

      if (Source.GlobalData.UndoDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Source.GlobalData, "UndoDBEntry");

      if (DocTypes.UseUsers) // 21.05.2019
      {
        if (this.UserId == 0)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "UserId");
        if (userId != this.UserId)
        {
          if (!ViewOtherUsersActionPermission)
            throw new DBxAccessException(Res.DBxDocProvider_Err_ViewOtherUsersAction);
        }
      }

      DataTable resTable;

      using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        DBxFilter filter = null;

        if (userId != 0)
          filter = new ValueFilter("UserId", userId);

        if (firstDate.HasValue || lastDate.HasValue)
          filter = filter & new DateRangeFilter("ActionTime", firstDate, lastDate);

        DBxColumns columns = new DBxColumns("Id,StartTime,ActionTime,ActionInfo,ApplyChangesTime,ApplyChangesCount");
        if (DocTypes.UseUsers) // 21.05.2019
          columns += "UserId";
        if (DocTypes.UseSessionId)
          columns += "SessionId";

        resTable = undoCon.FillSelect("UserActions", columns,
          filter, DBxOrder.ById);
        DataTools.SetPrimaryKey(resTable, "Id");

        if (!String.IsNullOrEmpty(singleDocTypeName))
        {
          Int32 singleDocTableId = DocTypes[singleDocTypeName].TableId;

          Int32[][] actionIds = DataTools.GetBlockedIds(resTable, 100);
          for (int i = 0; i < actionIds.Length; i++)
          {
            DataTable tableDocs = undoCon.FillUniqueColumnValues("DocActions", "UserActionId",
              new AndFilter(new ValueFilter("DocTableId", singleDocTableId),
              new IdsFilter("UserActionId", actionIds[i])));
            DataTools.SetPrimaryKey(tableDocs, "UserActionId");
            for (int j = 0; j < actionIds[i].Length; j++)
            {
              Int32 id = actionIds[i][j];
              if (tableDocs.Rows.Find(id) == null)
              {
                DataRow row1 = resTable.Rows.Find(id);
                row1.Delete();
              }
            }
          }
        }
      }

      // При передаче клиенту предотвращаем преобразование времени из-за разных часовых поясов
      SerializationTools.SetUnspecifiedDateTimeMode(resTable);
      resTable.AcceptChanges();

      return resTable;
    }

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoGetUserActionDocTable(Int32 actionId)
    {
      CheckThread();

      if (actionId <= 0)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_ActionId, actionId));

      DataTable resTable;
      using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        DBxFilter filter = new ValueFilter("UserActionId", actionId);
        resTable = undoCon.FillSelect("DocActions", new DBxColumns("Id,DocTableId,DocId,Version,Action"),
          filter, DBxOrder.ById);
      }

      // При передаче клиенту предотвращаем преобразование времени из-за разных часовых поясов
      SerializationTools.SetUnspecifiedDateTimeMode(resTable);
      resTable.AcceptChanges();

      return resTable;
    }

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    protected override DateTime? DoGetUserActionsLastTime(Int32 userId)
    {
      if (userId == 0)
        return null;

      CheckThread();

      if (Source.GlobalData.UndoDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Source.GlobalData, "UndoDBEntry");

      if (this.UserId == 0)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "UserId");
      if (userId != this.UserId)
      {
        if (!ViewOtherUsersActionPermission)
          throw new DBxAccessException(Res.DBxDocProvider_Err_ViewOtherUsersAction);
      }

      object v;

      DBxFilter filter = new ValueFilter("UserId", userId);

      using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        v = undoCon.GetMaxValue("UserActions", "ActionTime", filter);
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
    protected override DataTable DoLoadDocDataVersion(string docTypeName, Int32 docId, int wantedDocVersion)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");

      if (Source.GlobalData.UndoDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Source.GlobalData, "UndoDBEntry");

      CheckIsRealDocId(docId);
      if (wantedDocVersion < 1 || wantedDocVersion >= short.MaxValue)
        throw ExceptionFactory.ArgOutOfRange("wantedDocVersion", wantedDocVersion, 1, short.MaxValue);

      using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        #region Поиск в таблице DocActions

        UndoAction currAction = InternalGetDocAction(docTypeName, docId, wantedDocVersion, undoCon);

        #endregion

        DataTable table;

        using (DBxCon mainCon = new DBxCon(Source.MainDBEntry))
        {
          int version2 = DataTools.GetInt(mainCon.GetValue(docTypeName, docId, "Version2"));
          if (version2 <= wantedDocVersion)
          {
            // Берем данные из основной таблицы
            table = mainCon.FillSelect(docTypeName, GetColumns(docTypeName, null), new IdsFilter(docId));
          }
          else
          {
            // Берем данные из БД undo

            // В таблице данных undo нет столбцов Deleted и Version
            DBxColumns columns1 = GetColumns(docTypeName, null);
            DBxColumns columns2 = columns1 - "DocId,Version,Deleted";

            // Находим в копии таблицы документов строку с последним значением Vesion2, меньшим или равным запрошенному
            Int32 lastCopyId = DataTools.GetInt(undoCon.GetValuesForMax(docTypeName,
              DBxColumns.Id, "Version2", new AndFilter(
              new ValueFilter("DocId", docId), new ValueFilter("Version2", wantedDocVersion, CompareKind.LessOrEqualThan)))[0]);
            if (lastCopyId == 0)
              throw new BugException("Record not found in the Undo db in table \"" + docTypeName + "\", DocId=" + docId.ToString() + " и Version2=" + version2.ToString());

            DataTable table1 = undoCon.FillSelect(docTypeName, columns2, new IdsFilter(lastCopyId));
            if (table1.Rows.Count != 1)
              throw new BugException("In the Undo db in table \"" + docTypeName + "\" for key Id=" + lastCopyId.ToString() + " returned wrong number of rows (" + table1.Rows.Count.ToString() + ")");

            table = GetTemplate(docTypeName, null);
            DataTools.CopyRowsToRows(table1, table, true, true);
            table.Rows[0]["Id"] = docId;
            table.Rows[0]["Deleted"] = (currAction == UndoAction.Delete);
          }

          table.Rows[0]["Version"] = wantedDocVersion;
          table.AcceptChanges();
          return table;
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
    protected override DataTable DoLoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int wantedDocVersion)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
      if (String.IsNullOrEmpty(subDocTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("subDocTypeName");

      if (Source.GlobalData.UndoDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Source.GlobalData, "UndoDBEntry");

      CheckIsRealDocId(docId);
      if (wantedDocVersion < 1 || wantedDocVersion >= short.MaxValue)
        throw ExceptionFactory.ArgOutOfRange("wantedDocVersion", wantedDocVersion, 1, short.MaxValue);

      DataTable table;
      using (DBxCon mainCon = new DBxCon(_Source.MainDBEntry))
      {
        DBxColumns columns1 = GetColumns(docTypeName, subDocTypeName);
        columns1 += "Deleted,Version2";
        table = mainCon.FillSelect(subDocTypeName, columns1,
          new AndFilter(new IdsFilter("DocId", docId), // включая помеченные на удаление
          new ValueFilter("StartVersion", wantedDocVersion, CompareKind.LessOrEqualThan))); // пропуская более новые документы
      }

      if (table.Rows.Count == 0)
        return table; // нет поддокументов

      using (DBxCon undoCon = new DBxCon(Source.GlobalData.UndoDBEntry))
      {
        DBxColumns columns2 = GetColumns(docTypeName, subDocTypeName);
        columns2 -= "DocId";
        columns2 += "Deleted";


        foreach (DataRow row in table.Rows)
        {
          int version2 = DataTools.GetInt(row, "Version2");
          if (version2 <= wantedDocVersion)
            continue; // актуальная версия

          Int32 subDocId = DataTools.GetInt(row, "Id");


          // Ищем последнюю версию в таблице Undo
          Int32 lastCopyId = DataTools.GetInt(undoCon.GetValuesForMax(subDocTypeName,
            DBxColumns.Id,
            "Version2",
            new AndFilter(
              new ValueFilter("SubDocId", subDocId),
              new ValueFilter("Version2", wantedDocVersion, CompareKind.LessOrEqualThan)))[0]);
          if (lastCopyId == 0)
          {
            if (DataTools.GetBool(row, "Deleted"))
            {
              // 15.12.2017
              // Поддокумент был удален после запрошенной версии, но в истории нет записи.
              // Значит удаленная версия является действующей для запрошенной версии документа
              row["Deleted"] = false;
              continue;
            }
            else
            {
              // 24.03.2023
              // Если нет записи, то поддокумент считается существующим "с начала времен".
              // Не уверен, что это правильно. Поддокумент мог быть удаленным
              continue; 
            }
            //throw new DBxConsistencyException("В базе данных Undo в таблице \"" + subDocTypeName + "\" не найдена запись для SubDocId = " + subDocId.ToString() + " и Version2 <= " + version2.ToString());
          }

          DataTable table1 = undoCon.FillSelect(subDocTypeName, columns2, new IdsFilter(lastCopyId));
          if (table1.Rows.Count != 1)
            throw new BugException("In the Undo databse, in table \"" + subDocTypeName + "\" for key Id=" + lastCopyId.ToString() + " returned wrong number of rows (" + table1.Rows.Count.ToString() + ")");

          DataTools.CopyRowValues(table1.Rows[0], row, true);
          table.Rows[0]["DocId"] = docId;
        }
      }

      // Убираем поддокументы, помеченные на удаление
      for (int i = table.Rows.Count - 1; i >= 0; i--)
      {
        if (DataTools.GetBool(table.Rows[i], "Deleted"))
          table.Rows.RemoveAt(i);
      }

      table.AcceptChanges();
      return table;
    }

    private UndoAction InternalGetDocAction(string docTypeName, Int32 docId, int docVersion, DBxCon undoCon)
    {
      DBxDocType docType = DocTypes[docTypeName];
      if (docType == null)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, docTypeName), "docTypeName");

      CheckIsRealDocId(docId);

      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = new ValueFilter("DocTableId", docType.TableId);
      filters[1] = new ValueFilter("DocId", docId);
      filters[2] = new ValueFilter("Version", docVersion);


      Int32 docActionId = undoCon.FindRecord("DocActions", new AndFilter(filters));
      if (docActionId == 0)
        throw new BugException("In the table 'DocActions' record is found for document type \"" + docType.SingularTitle +
          "\", DocId=" + docId.ToString() + ", Version=" + docVersion.ToString());

      UndoAction currAction = (UndoAction)DataTools.GetInt(undoCon.GetValue("DocActions", docActionId, "Action"));
      return currAction;
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
    protected override DataSet DoLoadUnformattedDocData(string docTypeName, Int32 docId)
    {
      CheckThread();

      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
      DBxDocType docType = DocTypes[docTypeName];
      if (docType == null)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, docTypeName), "docTypeName");

      CheckIsRealDocId(docId);


      DataSet ds = new DataSet();

      #region Основная база данных

      using (DBxCon mainCon = new DBxCon(_Source.MainDBEntry))
      {
        ds.Tables.Add(mainCon.FillSelect(docTypeName, null, new ValueFilter("Id", docId)));
        foreach (DBxSubDocType sdt in docType.SubDocs)
          ds.Tables.Add(mainCon.FillSelect(sdt.Name, null, new ValueFilter("DocId", docId)));
      }

      #endregion

      if (Source.GlobalData.UndoDBEntry != null)
      {
        using (DBxCon undoCon = new DBxCon(_Source.GlobalData.UndoDBEntry))
        {
          #region UserActions и DocActions

          DataTable tableDocActions = undoCon.FillSelect("DocActions", null,
            new AndFilter(new ValueFilter("DocTableId", docType.TableId),
            new ValueFilter("DocId", docId)));
          Int32[] userActionIds = DataTools.GetIdsFromColumn(tableDocActions, "UserActionId");
          if (userActionIds.Length > 0)
            ds.Tables.Add(undoCon.FillSelect("UserActions", null, new IdsFilter("Id", userActionIds)));
          else
            ds.Tables.Add(undoCon.CreateEmptyTable("UserActions", null));
          ds.Tables.Add(tableDocActions); // После UserActions

          #endregion

          #region Таблицы истории

          DataTable table = undoCon.FillSelect(docTypeName, null, new ValueFilter("DocId", docId));
          table.TableName = "Undo_" + table.TableName;
          ds.Tables.Add(table);
          foreach (DBxSubDocType sdt in docType.SubDocs)
          {
            // В undo нет поля DocId.
            // Надо использовать идентификаторы поддокументов из основной таблицы
            Int32[] subDocIds = DataTools.GetIds(ds.Tables[sdt.Name]);
            if (subDocIds.Length > 0)
              table = undoCon.FillSelect(sdt.Name, null, new IdsFilter("SubDocId", subDocIds));
            else
              table = undoCon.CreateEmptyTable(sdt.Name, null);
            table.TableName = "Undo_" + table.TableName;
            ds.Tables.Add(table);
          }

          #endregion
        }
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
    protected override Guid DoAddLongLock(DBxDocSelection docSel)
    {
      CheckThread();

      DBxLongDocsLock longLock = new DBxLongDocsLock(this);
      longLock.Data.Init(docSel);
      Source.GlobalData.LongLocks.Add(longLock);
      return longLock.Guid;
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    protected override bool DoRemoveLongLock(Guid lockGuid)
    {
      CheckThread();

      return Source.GlobalData.LongLocks.Remove(lockGuid);
    }

    #endregion

    #region Клонирование

    /// <summary>
    /// Разрешение на клонирование провайдера.
    /// По умолчанию клонирование запрещено.
    /// </summary>
    public bool CloningAllowed { get { return _CloningAllowed; } set { _CloningAllowed = value; } }
    private bool _CloningAllowed;

    /// <summary>
    /// Создает копию <see cref="DBxRealDocProvider"/>, если это разрешено свойством <see cref="CloningAllowed"/>.
    /// Этот метод является потокобезопасным.
    /// </summary>
    /// <returns>Новый провайдер</returns>
    protected override DBxDocProvider DoClone()
    {
      if (!_CloningAllowed)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CloningAllowed");

      return new DBxRealDocProvider(Source, UserId, CurrentThreadOnly, SessionId);
    }

    #endregion

    #region Пересчет вычисляемых полей

    private delegate void RecalcColumnsDelegate(string docTypeName, Int32[] docIds);

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документв</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    protected override void DoRecalcColumns(string docTypeName, Int32[] docIds)
    {
      MethodInvokeExecProc.RunWithExecProc(new RecalcColumnsDelegate(RecalcColumns2), docTypeName, docIds);
    }

    private void RecalcColumns2(string docTypeName, Int32[] docIds)
    {
      DBxDocType docType = DocTypes[docTypeName];
      if (docType == null)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, docTypeName), "docTypeName");

      RecalcColumnsPermission perm = Source.UserPermissions.GetLast<RecalcColumnsPermission>();
      if (perm != null)
      {
        switch (perm.Mode)
        {
          case RecalcColumnsPermissionMode.Disabled:
            throw new DBxAccessException(Res.RecalcColumnsPermissionMode_Err_Disabled);
          case RecalcColumnsPermissionMode.Selected:
            if (docIds == null)
              throw new DBxAccessException(Res.RecalcColumnsPermissionMode_Err_Selected);
            break;
        }
      }

      if (!docType.HasCalculatedColumns)
        return; // нет вычисляемых полей

      // Выполняем пересчет блоками по 100 документов

      if (docIds == null)
      {
        Int32 lastId;
        using (DBxCon mainCon = new DBxCon(Source.MainDBEntry))
        {
          lastId = DataTools.GetInt(mainCon.GetMaxValue(docType.Name, "Id", null));
        }

        ISplash spl = ExecProc.CurrentProc.BeginSplash(String.Format(Res.DBxDocProvider_Phase_DocumentRecalc, docType.PluralTitle, lastId));
        spl.PercentMax = lastId;
        spl.AllowCancel = true;
        for (Int32 firstId = 1; firstId <= lastId; firstId += 100)
        {
          spl.Percent = firstId - 1;
          Int32 lastId2 = Math.Min(lastId, firstId + 99);
          Int32[] ids = new Int32[lastId2 - firstId + 1];
          for (int i = 0; i < ids.Length; i++)
            ids[i] = firstId + i;
          RecalcColumns3(docType, ids);
        }
        ExecProc.CurrentProc.EndSplash();
      }
      else
      {
        ISplash spl = ExecProc.CurrentProc.BeginSplash(String.Format(Res.DBxDocProvider_Phase_DocumentRecalc, docType.PluralTitle, docIds.Length));
        spl.PercentMax = docIds.Length;
        spl.AllowCancel = true;
        Int32[][] ids2 = DataTools.GetBlockedArray<Int32>(docIds, 100);
        for (int i = 0; i < ids2.Length; i++)
        {
          RecalcColumns3(docType, ids2[i]);
          spl.Percent += ids2.Length;
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
        throw DBxDocPermissions.GetException(docType, DBxDocPermissionReason.BeforeEdit);

      // Загружаем существующие данные
      DBxDocSet docSet = new DBxDocSet(this);
      docSet[docType.Name].View(ids);
      docSet.IgnoreAllLocks = true;

      // Права на запись для отдельных документов не проверяем
      // Вызываем обработчик BeforeWrite
      foreach (DBxSingleDoc doc in docSet[0])
      {
        if (doc.Deleted)
          continue; // 12.07.2022

        try
        {
          docType.PerformBeforeWrite(doc, false, true);
        }
        catch (Exception e)
        {
          AddExceptionInfo(e);

          e.Data["DocType"] = doc.DocType.Name;
          e.Data["DocId"] = doc.DocId;
          try { e.Data["DocText"] = GetTextValue(doc.DocType.Name, doc.DocId); }
          catch { }

          throw;
        }
      }

      using (DBxCon mainCon = new DBxCon(Source.MainDBEntry))
      {
        // Установка блокировки на запись документов
        DBxShortDocsLock dbLock = new DBxShortDocsLock(this, docSet.IgnoreAllLocks, docSet.IgnoredLocks);
        dbLock.Data.Init(this, docSet.DataSet);
        using (new ExecProcLockKey(dbLock))
        {
          foreach (DBxSingleDoc doc in docSet[0])
          {
            if (doc.Deleted)
              continue; // 12.07.2022

            try
            {
              // Записываем основной документ
              Hashtable fieldPairs = new Hashtable();
              DBxColumns usedColumnNames1 = null;
              bool hasPairs = AddUserFieldPairs(fieldPairs, doc.Row, false, DBPermissions, mainCon, ref usedColumnNames1, doc.DocType.Struct);
              if (hasPairs)
                mainCon.SetValues(docType.Name, doc.DocId, fieldPairs);

              // Записываем поддокументы
              for (int i = 0; i < doc.DocType.SubDocs.Count; i++)
              {
                string subDocTypeName = doc.DocType.SubDocs[i].Name;
                if (!doc.MultiDocs.SubDocs.ContainsModified(subDocTypeName))
                  continue;
                DBxSingleSubDocs sds = doc.SubDocs[subDocTypeName];
                if (sds.SubDocCount == 0)
                  continue;

                DBxTableStruct ts = sds.SubDocs.SubDocType.Struct;

                DBxColumns usedColumnNames2 = null;

                foreach (DBxSubDoc subDoc in sds)
                {
                  if (!subDoc.IsDataModified)
                    continue; // ничего не поменялось
                  fieldPairs = new Hashtable();
                  hasPairs = AddUserFieldPairs(fieldPairs, subDoc.Row, false, DBPermissions, mainCon, ref usedColumnNames2, subDoc.SubDocType.Struct);
                  if (hasPairs)
                    mainCon.SetValues(sds.SubDocs.SubDocType.Name, subDoc.SubDocId, fieldPairs);
                }
              }
            }
            catch (Exception e)
            {
              AddExceptionInfo(e);

              e.Data["DocType"] = doc.DocType.Name;
              e.Data["DocId"] = doc.DocId;
              try { e.Data["DocText"] = GetTextValue(doc.DocType.Name, doc.DocId); }
              catch { }

              throw;
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
    protected override DataTable DoGetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      DBxDocType toDT = DocTypes[docTypeName];
      if (toDT == null)
        throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, docTypeName), "docTypeName");

      DBxDocType fromSingleDT = null;
      if (!String.IsNullOrEmpty(fromSingleDocTypeName))
      {
        fromSingleDT = DocTypes[fromSingleDocTypeName];
        if (fromSingleDT == null)
          throw new ArgumentException(String.Format(Res.DBxDocProvider_Arg_UnknownDocType, fromSingleDocTypeName), "fromSingleDocTypeName");
      }

      CheckIsRealDocId(docId);
      if (fromSingleDocId != 0)
      {
        if (fromSingleDT == null)
          throw new ArgumentException(Res.DBxDocProvider_NoSingleDocType, "fromSingleDocId");
        CheckIsRealDocId(fromSingleDocId);
      }

      DataTable resTable;

      using (DBxCon mainCon = new DBxCon(Source.MainDBEntry))
      {
        DocRefTableHelper refHelper = new DocRefTableHelper(this, toDT, docId, showDeleted, unique, fromSingleDT, fromSingleDocId, mainCon);
        refHelper.Run();
        resTable = refHelper.ResTable;
      }

      return resTable;

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
      private readonly DBxRealDocProvider _Owner;

      public DBxDocType ToDT { get { return _ToDT; } }
      private readonly DBxDocType _ToDT;

      public Int32 DocId { get { return _DocId; } }
      private readonly Int32 _DocId;

      public bool ShowDeleted { get { return _ShowDeleted; } }
      private readonly bool _ShowDeleted;

      public bool Unique { get { return _Unique; } }
      private readonly bool _Unique;

      public DBxDocType FromSingleDT { get { return _FromSingleDT; } }
      private readonly DBxDocType _FromSingleDT;

      public Int32 FromSingleDocId { get { return _FromSingleDocId; } }
      private readonly Int32 _FromSingleDocId;

      public DBxCon Con { get { return _Con; } }
      private readonly DBxCon _Con;

      #endregion

      #region Результат

      public DataTable ResTable { get { return _ResTable; } }
      private DataTable _ResTable;

      #endregion

      #region Заполнение таблицы

      public void Run()
      {
        DBxDocTypeRefInfo[] refInfos = _Owner.DocTypes.GetToDocTypeRefs(_ToDT.Name);
        for (int i = 0; i < refInfos.Length; i++)
        {
          if (FromSingleDT != null && refInfos[i].FromDocType != FromSingleDT)
            continue;

          Int32[] toIds;
          if (refInfos[i].ToSubDocType == null)
            toIds = new Int32[1] { DocId };
          else
          {
            List<DBxFilter> filters = new List<DBxFilter>();
            filters.Add(new ValueFilter("DocId", DocId));
            if (!ShowDeleted)
              filters.Add(DBSSubDocType.DeletedFalseFilter);
            DBxFilter SubDocFilter = AndFilter.FromList(filters);
            toIds = Con.GetIds(refInfos[i].ToSubDocType.Name, SubDocFilter).ToArray();

            if (toIds.Length == 0)
              continue; // 16.08.2017
          }

          switch (refInfos[i].RefType)
          {
            case DBxDocTypeRefType.Column:
              AddForColumn(refInfos[i], toIds);
              break;
            case DBxDocTypeRefType.VTRefernce:
              AddForVTRef(refInfos[i], toIds);
              break;
            default:
              throw new NotImplementedException("Unknown reference type: " + refInfos[i].RefType);
          }
        }

        int cnt = 0;
        foreach (DataRow row in ResTable.Rows)
        {
          cnt++;
          row["_InternalCount"] = cnt;
          row["FromDeleted"] = DataTools.GetBool(row, "FromDocDeleted") || DataTools.GetBool(row, "FromSubDocDeleted");
          Int32 ThisFromDocTableId = DataTools.GetInt(row, "FromDocTableId");
          Int32 ThisFromDocId = DataTools.GetInt(row, "FromDocId");
          row["IsSameDoc"] = (ThisFromDocTableId == ToDT.TableId && ThisFromDocId == DocId);
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
        DBxColumns fromColumns;
        if (refInfo.FromSubDocType == null)
        {
          fromColumns = new DBxColumns("Id");
          if (refInfo.ToSubDocType != null)
            fromColumns += refInfo.FromColumn.ColumnName; // 16.09.2018
          if (Owner.DocTypes.UseDeleted)
          {
            fromColumns += "Deleted";
            if (refInfo.ToSubDocType != null)
              fromColumns += (refInfo.FromColumn.ColumnName + ".Deleted"); // 16.09.2018
          }
        }
        else
        {
          fromColumns = new DBxColumns("Id,DocId");
          fromColumns += refInfo.FromColumn.ColumnName;
          if (Owner.DocTypes.UseDeleted)
          {
            fromColumns += "Deleted";
            fromColumns += "DocId.Deleted";
            fromColumns += (refInfo.FromColumn.ColumnName + ".Deleted");
          }
        }

        List<DBxFilter> filters = new List<DBxFilter>();
        filters.Add(new IdsFilter(refInfo.FromColumn.ColumnName, toIds));
        if ((!ShowDeleted) && Owner.DocTypes.UseDeleted)
        {
          if (refInfo.FromSubDocType == null)
          {
            filters.Add(DBSDocType.DeletedFalseFilter);
          }
          else
          {
            filters.Add(DBSSubDocType.DeletedFalseFilter);
            filters.Add(DBSSubDocType.DocIdDeletedFalseFilter);
          }
        }
        if (FromSingleDocId != 0)
        {
          if (refInfo.FromSubDocType == null)
            filters.Add(new ValueFilter(DBSDocType.Id, FromSingleDocId));
          else
            filters.Add(new ValueFilter(DBSSubDocType.DocId, FromSingleDocId));
        }

        DataTable srcTable = Con.FillSelect(refInfo.FromDocTypeBase.Name, fromColumns, AndFilter.FromList(filters));

        foreach (DataRow srcRow in srcTable.Rows)
        {
          DataRow resRow = ResTable.NewRow();
          resRow["FromDocTableId"] = refInfo.FromDocType.TableId;
          if (refInfo.FromSubDocType == null)
          {
            resRow["FromDocId"] = srcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
              resRow["FromDocDeleted"] = srcRow["Deleted"];
          }
          else
          {
            resRow["FromDocId"] = srcRow["DocId"];
            resRow["FromSubDocId"] = srcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
            {
              resRow["FromDocDeleted"] = srcRow["DocId.Deleted"];
              resRow["FromSubDocDeleted"] = srcRow["Deleted"];
            }
            resRow["FromSubDocName"] = refInfo.FromSubDocType.Name;
          }

          resRow["FromColumnName"] = refInfo.FromColumn.ColumnName;

          if (refInfo.ToSubDocType != null)
          {
            resRow["ToSubDocName"] = refInfo.ToSubDocType.Name;
            resRow["ToSubDocId"] = srcRow[refInfo.FromColumn.ColumnName];
            if (Owner.DocTypes.UseDeleted)
              resRow["ToSubDocDeleted"] = srcRow[refInfo.FromColumn.ColumnName + ".Deleted"];
          }
          ResTable.Rows.Add(resRow);
        }
      }

      private void AddForVTRef(DBxDocTypeRefInfo refInfo, Int32[] toIds)
      {
#if DEBUG
        if (refInfo.ToSubDocType != null)
          throw new BugException("ToSubDocType!=null");
#endif

        DBxColumns fromColumns;
        if (refInfo.FromSubDocType == null)
        {
          fromColumns = new DBxColumns("Id");
          if (Owner.DocTypes.UseDeleted)
            fromColumns += "Deleted";
        }
        else
        {
          fromColumns = new DBxColumns("Id,DocId");
          fromColumns += refInfo.FromVTReference.DocIdColumn.ColumnName;
          if (Owner.DocTypes.UseDeleted)
          {
            fromColumns += "Deleted";
            fromColumns += "DocId.Deleted";
            //???fromColumns += (refInfo.FromColumn.ColumnName + ".Deleted");
          }
        }

        List<DBxFilter> filters = new List<DBxFilter>();
        filters.Add(new ValueFilter(refInfo.FromVTReference.TableIdColumn.ColumnName, refInfo.ToDocType.TableId));
        filters.Add(new IdsFilter(refInfo.FromVTReference.DocIdColumn.ColumnName, toIds));
        if ((!ShowDeleted) && Owner.DocTypes.UseDeleted)
        {
          if (refInfo.FromSubDocType == null)
          {
            filters.Add(DBSDocType.DeletedFalseFilter);
          }
          else
          {
            filters.Add(DBSSubDocType.DeletedFalseFilter);
            filters.Add(DBSSubDocType.DocIdDeletedFalseFilter);
          }
        }
        if (FromSingleDocId != 0)
        {
          if (refInfo.FromSubDocType == null)
            filters.Add(new ValueFilter(DBSDocType.Id, FromSingleDocId));
          else
            filters.Add(new ValueFilter(DBSSubDocType.DocId, FromSingleDocId));
        }

        DataTable srcTable = Con.FillSelect(refInfo.FromDocTypeBase.Name, fromColumns, AndFilter.FromList(filters));

        foreach (DataRow srcRow in srcTable.Rows)
        {
          DataRow resRow = ResTable.NewRow();
          resRow["FromDocTableId"] = refInfo.FromDocType.TableId;
          if (refInfo.FromSubDocType == null)
          {
            resRow["FromDocId"] = srcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
              resRow["FromDocDeleted"] = srcRow["Deleted"];
          }
          else
          {
            resRow["FromDocId"] = srcRow["DocId"];
            resRow["FromSubDocId"] = srcRow["Id"];
            if (Owner.DocTypes.UseDeleted)
            {
              resRow["FromDocDeleted"] = srcRow["DocId.Deleted"];
              resRow["FromSubDocDeleted"] = srcRow["Deleted"];
            }
            resRow["FromSubDocName"] = refInfo.FromSubDocType.Name;
          }

          resRow["FromColumnName"] = refInfo.FromVTReference.DocIdColumn.ColumnName;

          ResTable.Rows.Add(resRow);
        }
      }

      #endregion
    }

    #endregion
  }
}
