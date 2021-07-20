﻿#if DEBUG
//#define DEBUG_LIST // Если определено, то будет отслеживаться список существующих провайдеров
//#define DEBUG_STACK // Если определено, в отладочной информации будет стек вызовов для конструктора DBxDocProvider
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;
using AgeyevAV.IO;
using AgeyevAV.Config;
using System.Xml;
using System.Threading;
using AgeyevAV.Remoting;
using System.Diagnostics;
using AgeyevAV.Caching;
using System.Runtime.InteropServices;

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
  /// <summary>
  /// Провайдер для работы с документами
  /// Сам по себе класс DBxDocProvider является потокобезопасным, но производные классы могут ограничивать работу
  /// только тем потоком, который использовался для создания объекта. Привязка к потоку задается при создании DBxDocProvider
  /// </summary>
  public abstract class DBxDocProvider:
    MarshalByRefSponsoredObject,
    IDBxCacheSource,
    IDBxConReadOnlyPKInt32,
    ICloneable
  {
    #region Конструктор

    internal DBxDocProvider(NamedValues fixedInfo, bool currentThreadOnly)
    {
      DBxDocDebugTools.InitLogout();

      this._FixedInfo = fixedInfo;

      if (currentThreadOnly)
        _SingleThread = Thread.CurrentThread;

      _FirstCacheKey = DataTools.MD5SumFromString(DBIdentity + "|" + UserPermissionsAsXmlString);

#if DEBUG
      _DebugCreateTime = DateTime.Now;
#if DEBUG_STACK
      _DebugConstructorStackTrace = Environment.StackTrace;
#endif
#if DEBUG_LIST
      DebugDocProviderList.Add(this);
#endif
#endif

      _SyncRoot = new object(); // нет других подходящих private-объектов для блокировок
    }

    #endregion

    #region Фиксированные свойства

    /// <summary>
    /// Внутренние фиксированные данные, задаваемые в конструкторе.
    /// Это свойство не предназначено для использования в прикладном коде.
    /// Используйте обычные свойства DBxDocProvider, например, UserId.
    /// </summary>
    private NamedValues _FixedInfo;

    /// <summary>
    /// Создает объект, который используется для создания цепочечного провайдера.
    /// Метод можно вызвать многократно, так как допускается "ветвление" цепочки провайдеров.
    /// </summary>
    /// <returns></returns>
    public DBxDocProviderProxy CreateProxy()
    {
      return new DBxDocProviderProxy(this, _FixedInfo);
    }

    /// <summary>
    /// Описания документов и поддокументов, включая те, к которым у пользователя нет доступа
    /// </summary>
    public DBxDocTypes DocTypes { get { return (DBxDocTypes)(_FixedInfo["DocTypes"]); } }

    /// <summary>
    /// Идентификатор пользователя.
    /// Может быть 0
    /// </summary>
    public Int32 UserId { get { return _FixedInfo.GetInt("UserId"); } }

    /// <summary>
    /// Идентификатор сессии клиента.
    /// Может быть 0
    /// </summary>
    public Int32 SessionId { get { return _FixedInfo.GetInt("SessionId"); } }

    /// <summary>
    /// Идентификатор базы данных для выборки документов DBxDocSelection
    /// </summary>
    public string DBIdentity { get { return _FixedInfo.GetString("DBIdentity"); } }

    /// <summary>
    /// Структура таблиц "BinData" и "FileNames"
    /// </summary>
    private DBxBinDataHandlerInfo BinDataInfo { get { return (DBxBinDataHandlerInfo)(_FixedInfo["BinDataInfo"]); } }

    /// <summary>
    /// Возвращает true, если в документах используются ссылки на двоичные данные не в виде файлов
    /// </summary>
    public bool UseBinDataRefs { get { return BinDataInfo.UseBinData; } }

    /// <summary>
    /// Возвращает true, если в документах используются ссылки на файлы
    /// </summary>
    public bool UseFileRefs { get { return BinDataInfo.UseFiles; } }

    /// <summary>
    /// Свойство возвращает true, если существует база данных истории документов
    /// </summary>
    public bool UseDocHist { get { return _FixedInfo.GetBool("UseDocHist"); } }

    /// <summary>
    /// Служебные поля в начале таблицы документов, загружаемых в таблицу DBxMultiDocs.
    /// Это не все служебные поля, а только те, которые добавляются в шаблон DataTable,
    /// возвращаемый методом DBxRealDocProvider.DoGetTemplate()
    /// Таблицы документов и поддокументов в базе данных имеют дополнительные поля, причем состав служебных
    /// полей в основной базе данных и undo не совпадают
    /// </summary>
    internal DBxColumns MainDocTableServiceColumns { get { return (DBxColumns)(_FixedInfo["MainDocTableServiceColumns"]); } }

    /// <summary>
    /// Служебные поля в начале таблицы поддокументов, загружаемых в таблицу DBxMultiSubDocs
    /// Это не все служебные поля, а только те, которые добавляются в шаблон DataTable,
    /// возвращаемый методом DBxRealDocProvider.DoGetTemplate()
    /// Таблицы документов и поддокументов в базе данных имеют дополнительные поля, причем состав служебных
    /// полей в основной базе данных и undo не совпадают
    /// </summary>
    internal DBxColumns SubDocTableServiceColumns { get { return (DBxColumns)(_FixedInfo["SubDocTableServiceColumns"]); } }

    /// <summary>
    /// Возвращает список всех служебных полей документов, в зависимости от флажков в DBxDocTypes.
    /// Обычно список содержит поля "Id", "Deleted", "Version", "Version2", "CreateTime", "CreateUserId", "ChangeTime", "ChangeUserId"
    /// </summary>
    public DBxColumns AllDocServiceColumns { get { return (DBxColumns)(_FixedInfo["AllDocServiceColumns"]); } }

    /// <summary>
    /// Возвращает список всех служебных полей поддокументов, в зависимости от флажков в DBxDocTypes
    /// Обычно список содержит поля "Id", "DocId", "Deleted", "StartVersion" и "Version2"
    /// </summary>
    public DBxColumns AllSubDocServiceColumns { get { return (DBxColumns)(_FixedInfo["AllSubDocServiceColumns"]); } }


    /// <summary>
    /// Вычисленные права доступа к объектам основной базы данных (доступ к таблицам и полям)
    /// Фиксированный набор.
    /// </summary>
    public DBxPermissions DBPermissions { get { return (DBxPermissions)(_FixedInfo["DBPermissions"]); } }


    /// <summary>
    /// Описание прав пользователя в виде строки XML
    /// Оригинальные объекты UserPermission хранятся на сервере в объекте DBxRealDocProviderSource.
    /// </summary>
    /// <returns>Строка, содержащая XML-документ для объекта UserPermissions</returns>
    /// <remarks>Метод не может быть объявлен protected, так как требуется из объекта DBxChainDocProvider</remarks>
    internal string UserPermissionsAsXmlString { get { return _FixedInfo.GetString("UserPermissions"); } }

    /// <summary>
    /// Ключ, используемый для внутренних кэшей в DBxChainProvider.
    /// Состоит из DBIdenitity и прав доступа путем вычисления MD5
    /// </summary>
    internal string FirstCacheKey { get { return _FirstCacheKey; } }
    private string _FirstCacheKey;

    /// <summary>
    /// Объект, используемый для блокировки потоконебезопасных операций
    /// </summary>
    protected object SyncRoot { get { return _SyncRoot; } }
    private object _SyncRoot;

    #endregion

    #region Абстрактные методы и свойства

#if XXX
    /// <summary>
    /// Текущее время сервера.
    /// Для DBxRealDocProvider возвращает DateTime.Now
    /// Для DBxChainDocProvider, чтобы не обращаться к серверу каждый раз, возвращает текущее время клиента
    /// с добавлением разности времени, которая вычисляется однократно при создании объекта
    /// </summary>
    public abstract DateTime ServerTime { get;}
#endif

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docSel">Выборка документов, которую требуется заблокировать</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public abstract Guid AddLongLock(DBxDocSelection docSel);

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public Guid AddLongLock(string docTypeName, Int32[] docIds)
    {
      DBxDocSelection DocSel = new DBxDocSelection(DBIdentity);
      DocSel.Add(docTypeName, docIds);
      return AddLongLock(DocSel);
    }

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public Guid AddLongLock(string docTypeName, IdList docIds)
    {
      DBxDocSelection DocSel = new DBxDocSelection(DBIdentity);
      DocSel.Add(docTypeName, docIds);
      return AddLongLock(DocSel);
    }

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public Guid AddLongLock(string docTypeName, Int32 docId)
    {
      DBxDocSelection DocSel = new DBxDocSelection(DBIdentity);
      DocSel.Add(docTypeName, docId);
      return AddLongLock(DocSel);
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    public abstract bool RemoveLongLock(Guid lockGuid);

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документв</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    public abstract void RecalcColumns(string docTypeName, Int32[] docIds);

    /// <summary>
    /// Создание копии провайдера.
    /// Полученный провайдер можно использовать в отдельном потоке
    /// Для цепочечных провайдеров создается также копия исходного провайдера
    /// Этот метод может вызываться из любого потока, даже если текущий DBxDocProvider привязан к другому потоку
    /// </summary>
    /// <returns></returns>
    public abstract DBxDocProvider Clone();

    #endregion

    #region Права доступа

    /// <summary>
    /// Представление разрешений пользователя в виде XML (для отладки).
    /// Этот свойство не предназначеншо для использования в пользовательском коде.
    /// Используйте свойство UserPermissions.
    /// </summary>
    public XmlDocument UserPermissionsAsXml
    {
      get
      {
        return DataTools.XmlDocumentFromString(UserPermissionsAsXmlString);
      }
    }

    /// <summary>
    /// Права, назначенные пользователю (включая классы, определенные в программе).
    /// Так как UserPermissions не является сериализуемым то может возникнуть ошибка при передаче через
    /// границы домена / приложения, если не предусмотрено создание объектов в целевом домене
    /// </summary>
    public abstract UserPermissions UserPermissions { get;}

    /// <summary>
    /// Сброс прав пользователя
    /// </summary>
    public virtual void ResetPermissions()
    {
      CheckThread();
      _DocPermissions = null;
    }

    /// <summary>
    /// Права доступа, использующие обработчики
    /// Так как UserPermissions, используемый объектом, не является сериализуемым то может возникнуть ошибка при передаче через
    /// границы домена / приложения, если не предусмотрено создание объектов в целевом домене
    /// </summary>
    public DBxDocPermissions DocPermissions
    {
      get
      {
        CheckThread();

        if (_DocPermissions == null)
          _DocPermissions = new DBxDocPermissions(this);
        return _DocPermissions;
      }
    }
    private DBxDocPermissions _DocPermissions;

    /// <summary>
    /// Вызывает DocPermissions.TestDocument().
    /// Метод может быть переопределен производным классом
    /// </summary>
    /// <param name="doc">Проверяемый документ</param>
    /// <param name="reason">Режим доступа</param>
    [DebuggerStepThrough]
    public virtual void TestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason)
    {
      DocPermissions.TestDocument(doc, reason);
    }


    /// <summary>
    /// Возвращает true, если текущий пользователь имеет право на просмотр действий, выполненных другими пользователями
    /// </summary>
    public bool ViewOtherUsersActionPermission
    {
      get
      {
        CheckThread();

        ViewOtherUsersActionPermission p = this.UserPermissions.GetLast<ViewOtherUsersActionPermission>();
        if (p == null)
          return false;
        else
          return p.Allowed;
      }
    }

    /// <summary>
    /// Добавляет в список генераторы стандартных видов разрешений
    /// </summary>
    /// <param name="creators">Заполняемый список</param>
    /// <param name="docTypes">Заполненный список видов документов</param>
    public static void InitStdUserPermissionCreators(UserPermissionCreators creators, DBxDocTypes docTypes)
    {
      if (creators == null)
        throw new ArgumentNullException("creators");
      if (docTypes == null)
        throw new ArgumentNullException("docTypes");

      creators.Add(new WholeDBPermission.Creator());
      creators.Add(new TablePermission.Creator());
      creators.Add(new DocTypePermission.Creator(docTypes));
      creators.Add(new DocTypeViewHistoryPermission.Creator(docTypes));
      creators.Add(new ViewOtherUsersActionPermission.Creator());
      creators.Add(new RecalcColumnsPermission.Creator());
    }

    #endregion

    #region Структура и шаблоны таблиц

    #region Структуры таблиц

    /// <summary>
    /// Интерфейс для получения структуры таблиц.
    /// Структуры содержат только поля, доступны пользователю.
    /// Также содержат служебные поля.
    /// </summary>
    public IDBxStructSource StructSource
    {
      get
      {
        if (_StructSource == null)
          _StructSource = new DBxDocStructSource(DocTypes, DBPermissions, BinDataInfo);
        return _StructSource;
      }
    }
    private DBxDocStructSource _StructSource;

    #endregion

    #region Заготовки таблиц

    /// <summary>
    /// Получение пустой таблицы документа. Используется буферизация структур.
    /// Полученную таблицу можно менять, т.к. возвращается всегда копия таблицы
    /// Если у пользователя есть ограничения на доступ к отдельным полям таблицы, может быть возвращена
    /// неполная структура
    /// В таблице документа есть поля Id, Deleted и Version, но нет полей CreateTime, CreateUserId, ChangeTime и ChangeUserId
    /// В таблице поддокумента есть поля Id, DocId и Deleted.
    /// Если для документа требуются поля CreateTime, CreateUserId, ChangeTime и ChangeUserId, следует использовать
    /// версию метода с тремя аргументами
    /// Возвращаемая таблица не имеет первичного ключа
    /// </summary>
    /// <param name="docTypeName">Имя типа документа</param>
    /// <param name="subDocTypeName">Имя типа поддокумента или null для документа</param>
    /// <returns>Пустая таблица, содержащая структуру</returns>
    public DataTable GetTemplate(string docTypeName, string subDocTypeName)
    {
      return GetTemplate(docTypeName, subDocTypeName, false);
    }

    /// <summary>
    /// Получение пустой таблицы документа с неполным набором полей. Используется буферизация структур.
    /// Полученную таблицу можно менять, т.к. возвращается всегда копия таблицы
    /// В таблице будут только те поля, имена которых заданы в ColumnNames. Порядок полей также сохраняется.
    /// Если в списке <paramref name="columnNames"/> заданы несуществующие поля или поля, к которым у
    /// пользователя нет доступа, генерируется исключение
    /// Возвращаемая таблица не имеет первичного ключа
    /// </summary>
    /// <param name="docTypeName">Имя типа документа</param>
    /// <param name="subDocTypeName">Имя типа поддокумента или null для документа</param>
    /// <param name="columnNames">Имена полей, которые должны быть в таблице, с учетом порядка столбцов</param>
    public DataTable GetTemplate(string docTypeName, string subDocTypeName, DBxColumns columnNames)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      bool AllSystemColumns = false;
      if (String.IsNullOrEmpty(subDocTypeName) && columnNames.ContainsAny(AuxDocSysColumns))
        AllSystemColumns = true;
      DataTable Table1 = GetTemplate(docTypeName, subDocTypeName, AllSystemColumns);

      // Оставляем только нужные поля
      DataTable Table2 = new DataTable(Table1.TableName);
      columnNames.AddContainedColumns(Table2.Columns, Table1.Columns);
      return Table2;
    }

    /// <summary>
    /// Внутреннее хранилище структур таблиц, чтобы не запрашивать
    /// одно и тоже
    /// </summary>
    private SyncDictionary<String, DataTable> _BufStructs;

    /// <summary>
    /// Получение пустой таблицы документа. Используется буферизация структур.
    /// Полученную таблицу можно менять, т.к. возвращается всегда копия таблицы
    /// Если у пользователя есть ограничения на доступ к отдельным полям таблицы, может быть возвращена
    /// неполная структура
    /// В таблице документа есть поля Id, Deleted и Version, можно задать наличие полей CreateTime, CreateUserId, ChangeTime и ChangeUserId
    /// В таблице поддокумента есть поля Id, DocId и Deleted.
    /// Возвращаемая таблица не имеет первичного ключа
    /// </summary>
    /// <param name="docTypeName">Имя типа документа</param>
    /// <param name="subDocTypeName">Имя типа поддокумента или null для документа</param>
    /// <param name="allSystemColumns">Необходимость включения дополнительных столбцов</param>
    public DataTable GetTemplate(string docTypeName, string subDocTypeName, bool allSystemColumns)
    {
      CheckThread();

      if (_BufStructs == null)
        _BufStructs = new SyncDictionary<string, DataTable>();

      string TableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;

      DataTable Table1;
      if (!_BufStructs.TryGetValue(TableName, out Table1))
      {
        Table1 = DoGetTemplate(TableName, !String.IsNullOrEmpty(subDocTypeName));
        _BufStructs[TableName] = Table1; // при асинхронном вызове ничего плохого не случится
      }

      // Возвращаем копию набора. Оригинал потом пригодится
      DataTable Table2 = Table1.Clone();
      if (allSystemColumns)
      {
        // Добавляем недостающие системные поля
        if (String.IsNullOrEmpty(subDocTypeName))
        {
          if (DocTypes.UseTime)
            Table2.Columns.Add("CreateTime", typeof(DateTime));
          if (DocTypes.UseUsers)
            Table2.Columns.Add("CreateUserId", typeof(Int32));
          if (DocTypes.UseTime)
            Table2.Columns.Add("ChangeTime", typeof(DateTime));
          if (DocTypes.UseUsers)
            Table2.Columns.Add("ChangeUserId", typeof(Int32));
        }
      }

      DataTools.SetUnspecifiedDateTimeMode(Table2);
      return Table2;
    }

    private static DBxColumns AuxDocSysColumns = new DBxColumns("CreateTime,CreateUserId,ChangeTime,ChangeUserId");

    /// <summary>
    /// Получение структуры таблицы документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя типа документа или поддокумента</param>
    /// <param name="isSubDoc">Поддокумент?</param>
    /// <returns>Пустая таблица, содержащая структуру. В ней присутствует поле Id </returns>
    private DataTable DoGetTemplate(string tableName, bool isSubDoc)
    {
      CheckThread();

      DBxTableStruct Struct = StructSource.GetTableStruct(tableName);

      DataTable Table = new DataTable(tableName);

      for (int i = 0; i < Struct.Columns.Count; i++)
      {
        DBxColumnStruct ColDef = Struct.Columns[i];

        #region Пропускаем некоторые служебные поля

        if (isSubDoc)
        {
          switch (ColDef.ColumnName)
          {
            case "StartVersion":
            case "Version2":
              continue;
          }
        }
        else // документ
        {
          switch (ColDef.ColumnName)
          {
            case "Version2":
            case "CreateTime":
            case "CreateUserId":
            case "ChangeTime":
            case "ChangeUserId":
              continue;
          }
        }

        #endregion

        if (DBPermissions.ColumnModes[Struct.TableName, ColDef.ColumnName] == DBxAccessMode.None)
          continue;

        DataColumn Col = ColDef.CreateDataColumn();
        if ((!Col.AllowDBNull) && Col.DefaultValue is DBNull)
        {
          // Так нельзя. Значение по умолчанию должно задаваться.
          // В процессе редактирования в таблицу добавляется пустая строка-заготовка.
          // Если нет значения по умолчанию, то возникнет ошибка в DataTable.Rows.Add().
          //if (Col.DataType!=typeof(DateTime)) // нулевая дата - неподходящее значение

          if (String.IsNullOrEmpty(ColDef.MasterTableName))
          {
            // Но для Id и DocId такое делать не надо        
            switch (ColDef.ColumnName)
            {
              case "Id":
              //case "DocId":
              case "Version":
              case "Deleted":
                Col.AllowDBNull = true;
                break;
              default:
                Col.DefaultValue = DataTools.GetEmptyValue(Col.DataType);
                break;
            }
          }
          else
            Col.AllowDBNull = true; // В ссылочные поля точно не нужно записывать 0, включая поле "DocId" 
        }

        Table.Columns.Add(Col);
      } // цикл по описаниям столбцов

      return Table;
    }

    /// <summary>
    /// Получить индексатор для имен полей документа/поддокумента.
    /// Список полей соответствует получаемому методом GetTemplate() без системных столбцов.
    /// Стандартный метод DataColumnCollection.IndexOf(columnName), использовавшийся ранее, реализован неэффективно
    /// Индексатор является нечувствительным к регистру.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Индексатор имен полей</returns>
    internal StringArrayIndexer GetColumnNameIndexer(string tableName)
    {
      if (_ColumnNameIndexers == null)
        _ColumnNameIndexers = new SyncDictionary<string, StringArrayIndexer>();

      StringArrayIndexer indexer;
      if (!_ColumnNameIndexers.TryGetValue(tableName, out indexer))
      {
        DBxDocType dt;
        DBxSubDocType sdt;
        if (!DocTypes.FindByTableName(tableName, out dt, out sdt))
          throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\"", "tableName");
        DataTable table;
        if (sdt == null)
          table = GetTemplate(dt.Name, null, false);
        else
          table = GetTemplate(dt.Name, sdt.Name, false);

        indexer = new StringArrayIndexer(DataTools.GetColumnNames(table), true);
        _ColumnNameIndexers[tableName] = indexer; // 28.08.2020 - обеспечение потокобезопасности
      }
      return indexer;
    }
    private SyncDictionary<string, StringArrayIndexer> _ColumnNameIndexers;

    #endregion

    #endregion

    #region Список полей, к которым есть доступ

    /// <summary>
    /// Получение списка столбцов таблицы документа или поддокумента, к которым у пользователя есть доступ
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="subDocTypeName">Вид поддокумента. Null или пустая строка - для документа</param>
    /// <returns>Список столбцов</returns>
    public DBxColumns GetColumns(string docTypeName, string subDocTypeName)
    {
      CheckThread();

      string TableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;

      if (_BufColumns == null)
        _BufColumns = new SyncDictionary<string, DBxColumns>();

      DBxColumns Columns;
      if (!_BufColumns.TryGetValue(TableName, out Columns))
      {
        Columns = DBxColumns.FromColumns(GetTemplate(docTypeName, subDocTypeName).Columns);
        _BufColumns[TableName] = Columns; // 28.08.2020 Обеспечение потокобезопасности
      }
      return Columns;
    }

    private SyncDictionary<string, DBxColumns> _BufColumns;

    #endregion

    #region Обработка данных

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Таблица документов</returns>
    public abstract DataTable LoadDocData(string docTypeName, Int32[] docIds);

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    public abstract DataTable LoadDocData(string docTypeName, DBxFilter filter);

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    public abstract DataTable LoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds);

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
    public DataSet ApplyChanges(DataSet dataSet, bool reloadData)
    {
      if (CacheClearing != null)
      {
        DBxClearCacheData Data = CreateClearCacheData(dataSet);
        OnCacheClearing(Data);
      }

      return OnApplyChanges(dataSet, reloadData);
    }

    private DBxClearCacheData CreateClearCacheData(DataSet dataSet)
    {
      DBxClearCacheData Data = new DBxClearCacheData();
      foreach (DataTable Table in dataSet.Tables)
      {
        foreach (DataRow Row in Table.Rows)
        {
          try
          {
            Int32 Id;
            switch (Row.RowState)
            {
              case DataRowState.Added:
              case DataRowState.Modified:
                Id = (Int32)Row["Id"];
                break;
              case DataRowState.Deleted:
                Id = (Int32)(Row["Id", DataRowVersion.Original]);
                break;
              default:
                continue;
            }

            // 25.06.2021
            // Фиктивные идентификаторы тоже добавляем. Они маркируют добавление записи
            //if (IsRealDocId(Id))
            //{
            Data.Add(Table.TableName, Id);
            //}
          }
          catch
          {
          }
        }
      }

      return Data;
    }

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
    protected abstract DataSet OnApplyChanges(DataSet dataSet, bool reloadData);

    /// <summary>
    /// Создает и запускает процедуру для асинхронной работы на сервере.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    /// <returns></returns>
    public abstract DistributedCallData StartServerExecProc(NamedValues args);

    #endregion

    #region Получение информации для документа

    /// <summary>
    /// Получить таблицу истории для документа
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Таблица истории</returns>
    public abstract DataTable GetDocHistTable(string docTypeName, Int32 docId);

    /// <summary>
    /// Получить таблицу действий пользователя или всех пользователей за определенный период
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    /// <param name="userId">Идентификатор пользователя. 0-все пользователи</param>
    /// <param name="singleDocTypeName">Имя таблицы документа. Пустая строка - документы всех видов</param>
    /// <returns>Таблица действий</returns>
    public abstract DataTable GetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName);

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    public abstract DataTable GetUserActionDocTable(Int32 actionId);

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    public abstract DateTime? GetUserActionsLastTime(Int32 userId);

    /// <summary>
    /// Получить таблицу для просмотра ссылок на один документ.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, на который ищутся ссылки</param>
    /// <param name="docId">Идентификатор документа, на который ищутся ссылки</param>
    /// <param name="showDeleted">Надо ли включать в таблицу ссылки из удаленных документов и поддокументов,
    /// а также ссылки на удаленные поддокументы выбранного документа.
    /// Не имеет значение, помечен ли сам документ <paramref name="docId"/> на удаление (за исключением ссылок документа на самого себя</param>
    /// <param name="unique">Если true, то будет возвращено только по одной ссылке из каждого документа.
    /// Это рекомендуемый вариант для показа таблицы ссылок. Если false, то в таблице может быть несколько ссылок из одного исходного документа</param>
    /// <returns>Таблица ссылок</returns>
    public DataTable GetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique)
    {
      return GetDocRefTable(docTypeName, docId, showDeleted, unique, null, 0);
    }

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
    public abstract DataTable GetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId);

    #endregion

    #region Доступ к версиям документа

    /// <summary>
    /// Получить таблицу версий строк документа.
    /// Возвращает таблицу с одной строкой
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    public abstract DataTable LoadDocDataVersion(string docTypeName, Int32 docId, int docVersion);

    /// <summary>
    /// Получить таблицу версий строк поддокументов.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    public abstract DataTable LoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int docVersion);

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
    public abstract DataSet LoadUnformattedDocData(string docTypeName, Int32 docId);

    #endregion

    #region Прямые запросы в базу данных

    /*
     * В целях безопасности соединение с базой данной недоступно через провайдер документов.
     * Доступны только некоторые вызовы
     */

    #region Получение значений полей

    /// <summary>
    /// Доступ к буферизованным данным
    /// Если метод не переопределен, возвращается null
    /// </summary>
    public virtual DBxCache DBCache { get { return null; } }

    /// <summary>
    /// Сброс кэшированных данных
    /// </summary>
    public virtual void ClearCache()
    {
      CheckThread();
      OnCacheClearing(DBxClearCacheData.AllTables);
    }


    /// <summary>
    /// Событие вызывается при очистке кэша вызовом ClearCache() или ApplyChanges()
    /// </summary>
    public event DBxClearCacheEventHandler CacheClearing;

    /// <summary>
    /// Вызывает событие CacheClearing, если для него установлен обработчик
    /// </summary>
    /// <param name="clearCacheData"></param>
    protected void OnCacheClearing(DBxClearCacheData clearCacheData)
    {
      if (CacheClearing != null)
        CacheClearing(this, clearCacheData);
    }

    ///// <summary>
    ///// Получение значений служебных полей произвольного документа или поддокумента в виде коллекции "Имя-Значение".
    ///// Возвращаются значения полей "Deleted", "Version" и прочих, определяемых свойствами
    ///// AllDocServiceColumns или AllSubDocServiceColumns.
    ///// Список полей зависит от установленных флажков в DBxDocTypes
    ///// </summary>
    ///// <param name="TableName">Имя таблицы документа или поддокумента</param>
    ///// <param name="DocId">Идентификатор документа или поддокумента</param>
    ///// <returns>Коллекция имен и значений полей</returns>
    //public NamedValues GetServiceValues(string TableName, Int32 DocId)
    //{
    //  if (DocId == 0)
    //    return NamedValues.Empty;

    //  DBxDocTypeBase dtb;
    //  if (DocTypes.FindByTableName(TableName, out dtb))
    //  {
    //    DBxColumns Cols = dtb.IsSubDoc ? AllSubDocServiceColumns : AllDocServiceColumns;
    //    object[] a = GetValues(TableName, DocId, AllDocServiceColumns);
    //    NamedValues Res = new NamedValues();
    //    for (int i = 0; i < Cols.Count; i++)
    //      Res.Add(Cols[i], a[i]);
    //    Res.SetReadOnly();
    //    return Res;
    //  }
    //  else
    //    return NamedValues.Empty;
    //}

    /// <summary>
    /// Получение значений служебных полей произвольного документа.
    /// Некоторые поля могут быть не заполнены, если у пользователя нет прав или информация недоступна из-за конфигурации DBxDocTypes.
    /// Выполняется SQL-запрос к базе данных.
    /// </summary>
    /// <param name="docTypeName">Имя вида документа</param>
    /// <param name="docId">Идентификатор вида документа. Не может быть фиктивным идентификатором или 0</param>
    /// <returns>Объект со значениями полей</returns>
    public DBxDocServiceInfo GetDocServiceInfo(string docTypeName, Int32 docId)
    {
      return GetDocServiceInfo(docTypeName, docId, false);
    }
    /// <summary>
    /// Получение значений служебных полей произвольного документа.
    /// Некоторые поля могут быть не заполнены, если у пользователя нет прав или информация недоступна из-за конфигурации DBxDocTypes.
    /// </summary>
    /// <param name="docTypeName">Имя вида документа</param>
    /// <param name="docId">Идентификатор вида документа. Не может быть фиктивным идентификатором или 0</param>
    /// <param name="fromCache">Если true, информация возвращается из кэша. Если false, то выполняется SQL-запрос к базе данных</param>
    /// <returns>Объект со значениями полей</returns>
    public DBxDocServiceInfo GetDocServiceInfo(string docTypeName, Int32 docId, bool fromCache)
    {
      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      if (!DocTypes.Contains(docTypeName))
        throw new ArgumentException("Неизвестный вид документа \"" + docTypeName + "\"");
      CheckIsRealDocId(docId);

      DBxColumns Cols = AllDocServiceColumns;
      object[] Values;
      if (fromCache)
        Values = DBCache[docTypeName].GetValues(docId, Cols);
      else
        Values = GetValues(docTypeName, docId, Cols);

      DBxDocServiceInfo Info = new DBxDocServiceInfo();

      if (DocTypes.UseVersions)
        Info.Version = DataTools.GetInt(Values[Cols.IndexOf("Version")]);
      if (DocTypes.UseDeleted)
        Info.Deleted = DataTools.GetBool(Values[Cols.IndexOf("Deleted")]);

      if (DocTypeViewHistoryPermission.GetAllowed(this.UserPermissions, docTypeName))
      {
        if (DocTypes.UseUsers)
        {
          Info.CreateUserId = DataTools.GetInt(Values[Cols.IndexOf("CreateUserId")]);
          Info.ChangeUserId = DataTools.GetInt(Values[Cols.IndexOf("ChangeUserId")]);
          if (Info.ChangeUserId == 0)
            Info.ChangeUserId = Info.CreateUserId;
        }
        if (DocTypes.UseTime)
        {
          DateTime? dt = DataTools.GetNullableDateTime(Values[Cols.IndexOf("CreateTime")]);
          if (dt.HasValue)
            Info.CreateTime = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
          dt = DataTools.GetNullableDateTime(Values[Cols.IndexOf("ChangeTime")]);
          if (dt.HasValue)
            Info.ChangeTime = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
          else
            Info.ChangeTime = Info.CreateTime;
        }
      }
      return Info;
    }

    /// <summary>
    /// Обновить строки кэша в DBCache на основании набора данных.
    /// Предполагается, что набор данных <paramref name="ds"/> был только что загружен из базы данных и содержит актуальные данные.
    /// Поля с точками, если они есть в таблицах, игнорируются.
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public void UpdateDBCache(DataSet ds)
    {
      if (!DBCache.IsValidThread)
        return; // вызов из чужого потока

      for (int i = 0; i < ds.Tables.Count; i++)
      {
        DBxTableCache tc = DBCache.GetIfExists(ds.Tables[i].TableName);
        if (tc == null)
          continue;

        List<DataRow> Rows = null;

        foreach (DataRow Row in ds.Tables[i].Rows)
        {
          if (Row.RowState == DataRowState.Deleted)
            continue;
          Int32 Id = (Int32)(Row["Id"]);
          if (IsRealDocId(Id))
          {
            if (Rows == null)
              Rows = new List<DataRow>();
            Rows.Add(Row);
          }
        }

        if (Rows != null)
          tc.UpdateRows(Rows.ToArray());

      } // цикл по таблицам
    }


    #endregion

    #region IDBxConReadOnlyBase Members

    #region FillSelect

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
    /// Загрузка выбранных полей всей таблицы.
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
    /// <param name="columnNames">Список имен полей. Если null, то возвращаются все столбцы, к которым у пользователя разрешен доступ</param>
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
    /// <param name="columnNames">Список имен полей. Если null, то возвращаются все столбцы, к которым у пользователя разрешен доступ</param>
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
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    public abstract DataTable FillSelect(DBxSelectInfo info);


    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <param name="ColumnName">Имя поля</param>
    /// <param name="Where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    public abstract DataTable FillUniqueColumnValues(string TableName, string ColumnName, DBxFilter Where);

    #endregion

    #region GetRecordCount() и IsTableEmpty()

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    public abstract int GetRecordCount(string tableName);

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    public abstract int GetRecordCount(string tableName, DBxFilter where);

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    public abstract bool IsTableEmpty(string tableName);

    #endregion

    #region Min, Max, Sum

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Максимальное значение</returns>
    public abstract object GetMaxValue(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить минимальное значение числового поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение</returns>
    public abstract object GetMinValue(string tableName, string columnName, DBxFilter where);

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
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    public abstract object[] GetValuesForMax(string tableName, DBxColumns columnNames,
      string maxColumnName, DBxFilter where);

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
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    public abstract object[] GetValuesForMin(string tableName, DBxColumns columnNames,
      string minColumnName, DBxFilter where);

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns>Суммарное значение или null</returns>
    public abstract object GetSumValue(string tableName, string columnName, DBxFilter where);

    #endregion

    #endregion

    #region IDBxConReadOnlyPKInt32 Members

    #region GetValue, GetValues

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
      object Value;
      GetValue(tableName, id, columnName, out Value);
      return Value;
    }

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена
    /// </summary>
    /// <param name="TableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="Id">Идентификатор строки. Может быть 0, тогда возвращается Value=null</param>
    /// <param name="ColumnName">Имя поля (может быть с точками)</param>
    /// <param name="Value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    public abstract bool GetValue(string TableName, Int32 Id, string ColumnName, out object Value);

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
    public abstract object[] GetValues(string tableName, Int32 id, DBxColumns columnNames);

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращается массив значений null подходящей длины.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список имен столбцов, разделенных запятыми</param>
    /// <returns>Массив значений полей строки</returns>
    public object[] GetValues(string tableName, Int32 id, string columnNames)
    {
      DBxColumns ColumnNames2 = new DBxColumns(columnNames);
      return GetValues(tableName, id, ColumnNames2);
    }

    #endregion

    #region GetIds

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    public abstract IdList GetIds(string tableName, DBxFilter where);

    /// <summary>
    /// Получить массив идентификаторов строк с заданным значением поля
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, string columnName, object value)
    {
      return GetIds(tableName, new ValueFilter(columnName, value));
    }

    /// <summary>
    /// Получить массив идентификаторов строк с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Массив идентификаторов строк</returns>
    public IdList GetIds(string tableName, System.Collections.Hashtable columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return GetIds(tableName, new DBxColumns(ColumnNames), Values);
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

      DBxFilter Filter = ValueFilter.CreateFilter(columnNames, values);
      return GetIds(tableName, Filter);
    }

    #endregion

    #region FindRecord

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
    public abstract Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy);

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="singleOnly">Если true и найдено больше одной записи, удовлетворяющей условию
    /// <paramref name="where"/>, то возвращается 0</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    public abstract Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly);

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
      return FindRecord(tableName, new ValueFilter(columnName, value));
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, System.Collections.Hashtable columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return FindRecord(tableName, new DBxColumns(ColumnNames), Values);
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

      DBxFilter Filter = ValueFilter.CreateFilter(columnNames, values);

      return FindRecord(tableName, Filter, orderBy);
    }

    ///// <summary>
    ///// Поиск любой строки таблицы без всяких условий.
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <returns>Идентификатор первой попавшейся записи или 0, если таблица не содержит записей</returns>
    //public Int32 FindRecord(string tableName)
    //{
    //  return FindRecord(tableName, (DBxFilter)null, (DBxOrder)null);
    //}

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

    #endregion

    #region GetUnuqueValues

    // Эти методы реализуются в DBxConBase, каждый отдельно

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить значения поля даты и/или времени без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить значения поля GUID без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public abstract Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where);

    #endregion

    #endregion

    /// <summary>
    /// Получить список идентификаторов поддокументов для заданного документа.
    /// Поддокументы извлекаются из базы данных.
    /// Если нет ни одного поддокумента заданного вида, возвращается пустой массив
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокумента</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] GetSubDocIds(string docTypeName, string subDocTypeName, Int32 docId)
    {
      CheckIsRealDocId(docId);
      AndFilter Filter = new AndFilter(new ValueFilter("DocId", docId), DBSSubDocType.DeletedFalseFilter);
      DataTable Table = FillSelect(subDocTypeName, DBSSubDocType.IdColumns, Filter);
      return DataTools.GetIds(Table);
    }

    #region GetInheritorIds

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested)
    {
      Int32 LoopedId;
      return GetInheritorIds(tableName, parentIdColumnName, parentId, nested, null, out LoopedId);
    }

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <param name="where">Дополнительный фильтр. Может быть null, если фильтра нет</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where)
    {
      Int32 LoopedId;
      return GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out LoopedId);
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
    public abstract IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId);

    #endregion

    #endregion

    #region Файлы и двоичные данные

    /// <summary>
    /// Идентификатор двоичных данных или файла, дополненный идентификатором документа и поддокумента.
    /// Эта структура не используется в прикладном коде
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct DocSubDocDataId
    {
      #region Конструктор

      /// <summary>
      /// Инициализирует структуру
      /// </summary>
      /// <param name="docId">Идентификатор документа</param>
      /// <param name="subDocId">Идентификатор поддокумента</param>
      /// <param name="dataId">Идентификатор двоочиных данных или файла</param>
      public DocSubDocDataId(Int32 docId, Int32 subDocId, Int32 dataId)
      {
        if (docId == 0) // может быть реальный или фиктиывный идкентификатор
          throw new ArgumentException("docId=" + docId.ToString(), "docId");

        //if (subDocId<0) // может быть реальный или фиктиывный идкентификатор или 0
        //  throw new ArgumentException("subDocId=" + subDocId.ToString(), "subDocId");
        if (dataId <= 0) // обязательно реальный
          throw new ArgumentException("dataId=" + dataId.ToString(), "dataId");

        _DocId = docId;
        _SubDocId = subDocId;
        _DataId = dataId;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Идентификатор документа. 
      /// Должен быть реальным идентификатором.
      /// </summary>
      public Int32 DocId { get { return _DocId; } }
      private Int32 _DocId;

      /// <summary>
      /// Идентификатор поддокумента.
      /// Должен быть реальным идентификатором или 0 для основного документа
      /// </summary>
      public Int32 SubDocId { get { return _SubDocId; } }
      private Int32 _SubDocId;

      /// <summary>
      /// Идентификатор двоичных данных или файла.
      /// Должен быть реальным идентификатором.
      /// </summary>
      public Int32 DataId { get { return _DataId; } }
      private Int32 _DataId;

      #endregion

      #region Методы

      /// <summary>
      /// Текстовое представление для отладки
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        sb.Append("DocId=");
        sb.Append(_DocId);
        if (_SubDocId != 0)
        {
          sb.Append(", SubDocId=");
          sb.Append(_SubDocId);
        }
        sb.Append(", DataId=");
        sb.Append(_DataId);
        return sb.ToString();
      }

      #endregion
    }

    #region Двоичные данные

    /// <summary>
    /// Получение двоичных данных из таблицы документа или поддокумента с использование буферизации.
    /// Выполняется проверка прав доступа пользователя к документу.
    /// Если поле не содержит ссылки на данные, возвращается null
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента.
    /// Это не идентификатор двоичных данных.</param>
    /// <param name="columnName">Имя столбца, хранящего ссылку</param>
    /// <returns>Двоичные данные</returns>
    public byte[] GetBinData(string tableName, Int32 id, string columnName)
    {
      if (id == 0)
        return null;
      CheckIsRealDocId(id);

      DBxDocType DocType;
      DBxSubDocType SubDocType;
      if (!DocTypes.FindByTableName(tableName, out DocType, out SubDocType))
        throw new ArgumentException("Таблица \"" + tableName + "\" не относится к документу или поддокументу");

      List<DocSubDocDataId> dummyIds = new List<DocSubDocDataId>();

      if (SubDocType == null)
      {
        Int32 BinDataId = DataTools.GetInt(GetValue(tableName, id, columnName));
        return InternalGetBinData1(tableName, columnName, new DocSubDocDataId(id, 0, BinDataId), 0, dummyIds);
      }
      else
      {
        // Поддокумент
        object[] Values = GetValues(tableName, id, new DBxColumns(new string[] { columnName, "DocId" }));
        Int32 BinDataId = DataTools.GetInt(Values[0]);
        Int32 DocId = DataTools.GetInt(Values[1]);
        return InternalGetBinData1(tableName, columnName, new DocSubDocDataId(DocId, id, BinDataId), 0, dummyIds);
      }
    }

    /// <summary>
    /// Получение двоичных данных в виде документа XML из таблицы документа или поддокумента с использование буферизации.
    /// Выполняется проверка прав доступа пользователя к документу.
    /// Если поле не содержит ссылки на данные, возвращается null
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента.
    /// Это не идентификатор двоичных данных</param>
    /// <param name="columnName">Имя столбца, хранящего ссылку</param>
    /// <returns>XML-документ</returns>
    public XmlDocument GetBinDataXml(string tableName, Int32 id, string columnName)
    {
      byte[] Data = GetBinData(tableName, id, columnName);
      if (Data == null)
        return null;
      else
        return DataTools.XmlDocumentFromByteArray(Data);
    }

    /// <summary>
    /// Внутренний метод получения идентификатора двоичных данных
    /// </summary>
    /// <param name="md5">Контрольная сумма</param>
    /// <returns>Идентификатор записи или 0, если таких данных нет</returns>
    public abstract Int32 InternalFindBinData(string md5);

    /// <summary>
    /// Нельзя использовать в системе буферизации просто байтовый массив.
    /// Обязательно нужен класс
    /// </summary>
    [Serializable]
    private class BinDataCacheItem
    {
      #region Поля

      public byte[] Data;

      #endregion
    }

    /// <summary>
    /// Внутренний метод получения двоичных данных с кэшированием
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя столбца двоичных данных документа или поддокумента</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и двоичных данных,
    /// которые требуется получить</param>
    /// <param name="docVersion">Версия документа. 0-действующая версия</param>
    /// <param name="preloadIds">Идентификаторы данных, которые могут потребоваться в будущем
    /// и которые желательно загрузить. Если данные уже есть в кэше или они загружены
    /// с сервера при вызове этого метода, из списка удаляются 
    /// соответствующие идентификаторы. Если нельзя загрузить все данные с сервера
    /// за один раз из-за размера, список станет короче, но не нулевой длины</param>
    /// <returns>Двоичные данные для <paramref name="wantedId"/>.</returns>
    internal byte[] InternalGetBinData1(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      try
      {
        return DoInternalGetBinData1(tableName, columnName, wantedId, docVersion, preloadIds);
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        e.Data["InternalGetBinData1.TableName"] = tableName;
        e.Data["InternalGetBinData1.ColumnName"] = columnName;
        e.Data["InternalGetBinData1.WantedId"] = wantedId;
        if (preloadIds != null)
          e.Data["InternalGetBinData1.PreloadIds"] = preloadIds.ToArray();
        throw;
      }
    }

    private byte[] DoInternalGetBinData1(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      #region 1. Извлечение запрошенных данных из кэша

      string[] keys = new string[] { FirstCacheKey, wantedId.DataId.ToString() };
      BinDataCacheItem item = Cache.GetItemIfExists<BinDataCacheItem>(keys, CachePersistance.MemoryAndTempDir);
      if (item != null)
        return item.Data;

      #endregion

      #region 2. Поиск в preloadIds №1

      for (int i = preloadIds.Count - 1; i >= 0; i--)
      {
        keys[1] = preloadIds[i].DataId.ToString();
        if (Cache.GetItemIfExists<BinDataCacheItem>(keys, CachePersistance.MemoryAndTempDir) != null)
          preloadIds.RemoveAt(i);
      }

      #endregion

      #region 3. Запрос к DBxRealDocProvider

      Dictionary<Int32, byte[]> dict = InternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds);
#if DEBUG
      if (dict.Count < 1)
        throw new BugException("InternalGetBinData2 вернул пустую коллекцию");
#endif

      #endregion

      #region 4. Добавление в кэш

      foreach (KeyValuePair<Int32, byte[]> pair in dict)
      {
#if DEBUG
        CheckIsRealDocId(pair.Key);
        if (pair.Value == null)
          throw new BugException("InternalGetBinData2 вернул null для BinDataId=" + pair.Key.ToString());
#endif
        item = new BinDataCacheItem();
        item.Data = pair.Value;

        keys[1] = pair.Key.ToString();
        Cache.SetItem<BinDataCacheItem>(keys, CachePersistance.MemoryAndTempDir, item);
      }

      #endregion

      #region 5. Поиск в preloadIds №2

      for (int i = preloadIds.Count - 1; i >= 0; i--)
      {
        if (dict.ContainsKey(preloadIds[i].DataId))
          preloadIds.RemoveAt(i);
      }

      #endregion

      return dict[wantedId.DataId];
    }

    /// <summary>
    /// Метод получения двоичных данных, реализуемый в DBxRealDocProvider.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор двоичных данных</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и двочных данных, которые нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и двочных данных,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор двоичных данных. Значение - загруженные данные</returns>
    public abstract Dictionary<Int32, byte[]> InternalGetBinData2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds);

    /// <summary>
    /// Получить длину блока двоичных данных по идентификатору двочиных данных.
    /// Используется кэширование и не проверяются права доступа к данным.
    /// Считается, что длина блока не является секретной информацией.
    /// Метод GetBinData(), возвращающий сами данные, напротив, выполняет проверку прав доступа.
    /// </summary>
    /// <param name="binDataId">Идентификатор двоичных данных</param>
    /// <returns>Длина блока данных</returns>
    public int GetBinDataLength(Int32 binDataId)
    {
      return DBCache["BinData"].GetInt(binDataId, "Length");
    }

    #endregion

    #region Информация о файле

    /// <summary>
    /// Получить имя, размер и время файла, сохраненного в базе данных, по идентификатору.
    /// Если задан нулевой идентификатор, возвращается пустой объект
    /// Метод использует буферизацию данных.
    /// Этот метод, в отличие от других, не проверяет права доступа пользователя на файл и может использоваться,
    /// например, в табличных просмотрах списков хранимых файлов, где требуется быстрый доступ к именам файлов.
    /// Предполагается, что имя и атрибуты файла не являются секретной информацией.
    /// Не существует такого же метода GetDBFile(), так как для доступа к содержимому файла требуется проверка прав доступа.
    /// </summary>
    /// <param name="fileId">Идентификатор, присвоенный файлу при сохранении в базе данных</param>
    /// <returns>Информация о файле</returns>
    public StoredFileInfo GetDBFileInfo(Int32 fileId)
    {
      if (DBCache == null)
        throw new NullReferenceException("Свойство DBCache не установлено");

      if (fileId == 0)
        return StoredFileInfo.Empty;

      object[] a = DBCache["FileNames"].GetValues(fileId, new DBxColumns("Name,Data.Length,CreationTime,LastWriteTime"));
      StoredFileInfo fi = new StoredFileInfo(DataTools.GetString(a[0]),
        DataTools.GetInt(a[1]),
        DataTools.GetNullableDateTime(a[2]),
        DataTools.GetNullableDateTime(a[3]));
      return fi;
    }

    /// <summary>
    /// Получить информацию о файле, хрнаящегося по ссылке в поле документа или поддокумента
    /// Метод использует буферизацию данных.
    /// Если поле не хранит ссылку на файл, возвращается пустой объект
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="columnName">Имя столбца, хранящего ссылку</param>
    /// <returns>Информация о файле</returns>
    public StoredFileInfo GetDBFileInfo(string tableName, Int32 id, string columnName)
    {
      if (id == 0)
        return StoredFileInfo.Empty;
      CheckIsRealDocId(id);

      Int32 FileId = DataTools.GetInt(GetValue(tableName, id, columnName));
      return GetDBFileInfo(FileId);
    }

    #endregion

    #region FileContainer

    /// <summary>
    /// Получение файла из таблицы документа или поддокумента с использование буферизации.
    /// Выполняется проверка прав доступа пользователя к документу.
    /// Если поле не содержит ссылки на данные, возвращается null
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="columnName">Имя столбца, хранящего ссылку</param>
    /// <returns>Двоичные данные</returns>
    public FileContainer GetDBFile(string tableName, Int32 id, string columnName)
    {
      if (id == 0)
        return null;
      CheckIsRealDocId(id);

      List<DocSubDocDataId> dummyIds = new List<DocSubDocDataId>();

      DBxDocType DocType;
      DBxSubDocType SubDocType;
      if (!DocTypes.FindByTableName(tableName, out DocType, out SubDocType))
        throw new ArgumentException("Таблица \"" + tableName + "\" не относится к документу или поддокументу");

      if (SubDocType == null)
      {
        Int32 FileId = DataTools.GetInt(GetValue(tableName, id, columnName));
        return InternalGetDBFile1(tableName, columnName, new DocSubDocDataId(id, 0, FileId), 0, dummyIds);
      }
      else
      {
        // Поддокумент
        object[] Values = GetValues(tableName, id, new DBxColumns(new string[] { columnName, "DocId" }));
        Int32 FileId = DataTools.GetInt(Values[0]);
        Int32 DocId = DataTools.GetInt(Values[1]);
        return InternalGetDBFile1(tableName, columnName, new DocSubDocDataId(DocId, id, FileId), 0, dummyIds);
      }
    }

    /// <summary>
    /// Внутренний метод получения идентификатора хранимого файла
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="md5">Контрольная сумма содержимого файла</param>
    /// <param name="binDataId">Сюда помещается идентификатор двоичных данных,
    /// возвращаемый методом InternalFindBinData()</param>
    /// <returns>Идентификатор записи файла в базе данных</returns>
    public abstract Int32 InternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId);


    /// <summary>
    /// Нельзя использовать в системе буферизации просто байтовый массив.
    /// Обязательно нужен класс
    /// </summary>
    [Serializable]
    private class DBFileCacheItem
    {
      #region Поля

      public FileContainer File;

      #endregion
    }

    /// <summary>
    /// Внутренний метод получения файла с кэшированием
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя столбца двоичных данных документа или поддокумента</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и файла,
    /// которые требуется получить</param>
    /// <param name="docVersion">Версия документа. 0-действующая версия</param>
    /// <param name="preloadIds">Идентификаторы данных, которые могут потребоваться в будущем
    /// и которые желательно загрузить. Если данные уже есть в кэше или они загружены
    /// с сервера при вызове этого метода, из списка удаляются 
    /// соответствующие идентификаторы. Если нельзя загрузить все данные с сервера
    /// за один раз из-за размера, список станет короче, но не нулевой длины</param>
    /// <returns>Файл для <paramref name="wantedId"/>.</returns>
    internal FileContainer InternalGetDBFile1(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      try
      {
        return DoInternalGetDBFile1(tableName, columnName, wantedId, docVersion, preloadIds);
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        e.Data["InternalGetDBFile1.TableName"] = tableName;
        e.Data["InternalGetDBFile1.ColumnName"] = columnName;
        e.Data["InternalGetDBFile1.WantedId"] = wantedId;
        if (preloadIds != null)
          e.Data["InternalGetDBFile1.PreloadIds"] = preloadIds.ToArray();
        throw;
      }
    }

    private FileContainer DoInternalGetDBFile1(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      #region 1. Извлечение запрошенных данных из кэша

      string[] keys = new string[] { FirstCacheKey, wantedId.ToString() };
      DBFileCacheItem item = Cache.GetItemIfExists<DBFileCacheItem>(keys, CachePersistance.MemoryAndTempDir);
      if (item != null)
        return item.File;

      #endregion

      #region 2. Поиск в preloadIds №1

      for (int i = preloadIds.Count - 1; i >= 0; i--)
      {
        keys[1] = preloadIds[i].DataId.ToString();
        if (Cache.GetItemIfExists<DBFileCacheItem>(keys, CachePersistance.MemoryAndTempDir) != null)
          preloadIds.RemoveAt(i);
      }

      #endregion

      #region 3. Запрос к DBxRealDocProvider

      Dictionary<Int32, FileContainer> dict = InternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
#if DEBUG
      if (dict.Count < 1)
        throw new BugException("InternalGetDBFile2 вернул пустую коллекцию");
#endif

      #endregion

      #region 4. Добавление в кэш

      foreach (KeyValuePair<Int32, FileContainer> pair in dict)
      {
#if DEBUG
        CheckIsRealDocId(pair.Key);
        if (pair.Value == null)
          throw new BugException("InternalGetDBFile2 вернул null для FileId=" + pair.Key.ToString());
#endif
        item = new DBFileCacheItem();
        item.File = pair.Value;

        keys[1] = pair.Key.ToString();
        Cache.SetItem<DBFileCacheItem>(keys, CachePersistance.MemoryAndTempDir, item);
      }

      #endregion

      #region 5. Поиск в preloadIds №2

      for (int i = preloadIds.Count - 1; i >= 0; i--)
      {
        if (dict.ContainsKey(preloadIds[i].DataId))
          preloadIds.RemoveAt(i);
      }

      #endregion

      return dict[wantedId.DataId];
    }

    /// <summary>
    /// Метод получения файла, реализуемый в DBxRealDocProvider.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор файла</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и файла, который нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и файлов,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор файла. Значение - контейнер с файлом</returns>
    public abstract Dictionary<Int32, FileContainer> InternalGetDBFile2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds);

    #endregion

    #endregion

    #region Доступ к кэшированным данным DBxCache

    /// <summary>
    /// Получить информацию о способе буферизации полей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Описание буферизации или null, если явная настройка полей не выполнялась на стороне сервера</returns>
    public DBxTableCacheInfo GetTableCacheInfo(string tableName)
    {
      DBxDocStructSource src = ((DBxDocStructSource)StructSource); // инициализация при первом обращении
      return src.GetTableCacheInfo(tableName);
    }

    /// <summary>
    /// Загрузить страницы кэша
    /// </summary>
    /// <param name="request">Данные запроса</param>
    /// <returns>Загруженные страницы страницы</returns>
    public abstract DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request);

    /// <summary>
    /// Очистить страницы таблицы кэша
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Начальные идентификаторы страниц</param>
    public abstract void ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds);

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    public abstract string GetTextValue(string tableName, Int32 id);

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName
    {
      get
      {
        // Не буферизуем строку, на случай, если имя пользователя поменяется дианмически

        if (UserId == 0)
          return "Нет пользователя";
        else
          return GetTextValue(DocTypes.UsersTableName, UserId);
      }
    }

    /// <summary>
    /// Получение текста документа или поддокумента в процессе редактирование.
    /// Создает маленький набор данных, содержащий только одну строку.
    /// Его можно передавать, не опасаясь перегрузки канала связи при сериализации
    /// </summary>
    /// <param name="row">Строка документа или поддокумента</param>
    /// <returns>Текстовое представление</returns>
    internal string InternalGetTextValue(DataRow row)
    {
      DataSet PrimaryDS = new DataSet();
      DataTable Table = row.Table.Clone(); // пустая таблица
      //DataTools.SetPrimaryKey(Table, "Id");
      Int32 Id;
      if (row.RowState == DataRowState.Deleted)
      {
        Table.Rows.Add(DataTools.GetRowValues(row, DataRowVersion.Original));
        Id = (Int32)(row["Id", DataRowVersion.Original]);
      }
      else
      {
        Table.Rows.Add(row.ItemArray);
        Id = (Int32)(row["Id"]);
      }
      PrimaryDS.Tables.Add(Table);
      PrimaryDS.AcceptChanges();



      return InternalGetTextValue(Table.TableName, Id, PrimaryDS);
    }


    /// <summary>
    /// Внутренний метод получения текста.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="primaryDS">Первичный набор данных</param>
    /// <returns>Текст для документа или поддокумента</returns>
    public /*internal protected */ abstract string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS);
    // Нельзя сделать internal protected, т.к. будет ошибка вызова удаленного метода

    #endregion

#if XXX

    // Нельзя, т.к. DBxDocProvider - MarshalByRefObject

    #region Кэширование данных

    /// <summary>
    /// Кэш значений полей.
    /// Доступ к значениям возможен только из потока, в котором создан DocProvider
    /// </summary>
    public DBxCache Cache
    {
      get
      {
        if (FCache == null)
          FCache = new DBxCache(this, true);
        return FCache;
      }
    }
    private DBxCache FCache;

    #endregion
#endif

    #region Проверка доступа из потока

    /// <summary>
    /// Если свойство возвращает true, то для всех вызовов будет проверяться, что вызов выполняется из того
    /// потока, в котором создан данный DocProvider
    /// </summary>
    public bool CurrentThreadOnly { get { return _SingleThread != null; } }

    /// <summary>
    /// Если свойство не равно null, то провайдер предназначен только для использования в потоке, 
    /// в котором был вызван конструкор
    /// Попытки вызовов методов из других потоков вызывают исключения.
    /// Класс DBxDocProvider в-целом не является потокобезопасным, но в некоторых сценариях может быть
    /// разрешена передача между потоками
    /// </summary>
    public Thread SingleThread { get { return _SingleThread; } }
    private Thread _SingleThread;

    /// <summary>
    /// Если в конструкторе был задан параметр CurrentThreadOnly, то проверяется совпадение текущего
    /// потока с тем, который вызвал конструктор провайдера.
    /// Если поток не совпадает, генерируется исключение.
    /// </summary>
    public void CheckThread()
    {
      if (_SingleThread != null)
      {
        if (_SingleThread != Thread.CurrentThread)
          throw new DifferentThreadException();
      }
    }
    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Возвращает true, если идентификатор больше 0
    /// </summary>
    /// <param name="id">Проверяемый идентификатор</param>
    /// <returns>Результат проверки</returns>
    public bool IsRealDocId(Int32 id)
    {
      return id > 0;
    }

    /// <summary>
    /// Проверяет, что идентификатор больше 0.
    /// Если передан неправильный (нулевой или фиктивный) идентификатор, генерируется исключение
    /// </summary>
    /// <param name="id">Проверяемый идентификатор</param>
    /// <returns>Результат проверки</returns>
    public void CheckIsRealDocId(Int32 id)
    {
      if (!IsRealDocId(id))
        throw new BugException("Недопустимый идентификатор " + id.ToString());
    }

    /// <summary>
    /// Выполняет для всех строк всех таблиц в наборе идентификаторы (поле "Id").
    /// Если какая-либо строка содержит фиктивный идентификатор, генерируется исключение.
    /// Строки данные, помеченные на удаление, пропускаются.
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public void CheckAreRealIds(DataSet ds)
    {
      foreach (DataTable Table in ds.Tables)
      {
        int pId = Table.Columns.IndexOf("Id"); // обычно, 0
        foreach (DataRow Row in Table.Rows)
        {
          if (Row.RowState == DataRowState.Deleted)
            continue;

          Int32 Id = DataTools.GetInt(Row[pId]);
          if (Id <= 0)
            throw new BugException("Недопустимый идентификатор " + Id.ToString() + " в таблице \"" + Table.TableName + "\"");
        }
      }
    }

#if XXX
    /// <summary>
    /// Список из одного столбца "Id"
    /// </summary>
    [Obsolete("Используйте поле класса DBSDocType или DBSSubDocType", false)]
    public static DBxColumns IdColumns { get { return DBxColumns.Id; } }

    /// <summary>
    /// Список из одного столбца "DocId"
    /// </summary>
    [Obsolete("Используйте поле класса DBSSubDocType", false)]
    public static readonly DBxColumns DocIdColumns = new DBxColumns("DocId");

    /// <summary>
    /// Фильтр по полю "Deleted" для отмены загрузки удаленных документов или поддокументов
    /// </summary>
    [Obsolete("Используйте поле класса DBSDocType или DBSSubDocType", false)]
    public static readonly ValueFilter DeletedFalseFilter = new ValueFilter("Deleted", false);

    /// <summary>
    /// Фильтр по полю "DocId.Deleted" для отмены загрузки поддокументов для удаленных документов
    /// </summary>
    [Obsolete("Используйте поле класса DBSSubDocType", false)]
    public static readonly ValueFilter DocIdDeletedFalseFilter = new ValueFilter("DocId.Deleted", false);

    /// <summary>
    /// Порядок сортировки по полю "Id"
    /// </summary>
    [Obsolete("Используйте поле класса DBSDocType или DBSSubDocType", false)]
    public static DBxOrder OrderById { get { return DBxOrder.ById; } }

#endif

    /// <summary>
    /// Очистка кэша.
    /// Вызывает <paramref name="dbCache"/>.ClearCache() для всех таблиц.
    /// Таблицы "BinData" и "FileNames" пропускаются
    /// </summary>
    /// <param name="dataSet">Набор данных</param>
    /// <param name="dbCache">Кэш</param>
    public static void ClearCache(DataSet dataSet, DBxCache dbCache)
    {
      foreach (DataTable Table in dataSet.Tables)
      {
        if (Table.Rows.Count == 0)
          continue;

        switch (Table.TableName)
        {
          case "BinData":
          case "FileNames":
            continue; // эти таблицы не сбрасываются
        }

        // Не выгодно выполнять сброс по одной строке, т.к. будет выполняться обращение к системе кэширования,
        // в том числе, обращение к диску
        IdList Ids = new IdList();
        foreach (DataRow Row in Table.Rows)
        {
          switch (Row.RowState)
          {
            case DataRowState.Added:
            case DataRowState.Modified:
              Int32 NewId = (Int32)(Row["Id"]);
              if (NewId > 0) // 19.03.2016 
                Ids.Add(NewId);
              break;
            case DataRowState.Deleted:
              Int32 OldId = (Int32)(Row["Id", DataRowVersion.Original]);
              if (OldId > 0)
                Ids.Add(OldId);
              break;
          }
        }

        DBxTableCache tc = dbCache[Table.TableName];
        tc.Clear(Ids);
      }

    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(this.GetType().ToString());
      sb.Append(". ");
      sb.Append("DBIdentity=");
      sb.Append(DBIdentity.ToString());
      sb.Append(", UserId=");
      sb.Append(UserId.ToString());
      return sb.ToString();
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Обработка ошибок

    /// <summary>
    /// Добавляет отладочную информацию в объект исключения.
    /// Используется в блоках catch.
    /// </summary>
    /// <param name="e">Исключение</param>
    public virtual void AddExceptionInfo(Exception e)
    {
      try
      {
        if (DocTypes.UseUsers)
          e.Data["DBxDocProvider.UserId"] = UserId;
        if (DocTypes.UseSessionId)
          e.Data["DBxDocProvider.SessionId"] = SessionId;
        e.Data["DBxDocProvider.DBIdentity"] = DBIdentity;
        e.Data["DBxDocProvider.UseBinDataRefs"] = UseBinDataRefs;
        e.Data["DBxDocProvider.UseFileRefs"] = UseFileRefs;
        e.Data["DBxDocProvider.UseDocHist"] = UseDocHist;
        try
        {
          e.Data["DBxDocProvider.UserPermissionsAsXml"] = DataTools.XmlDocumentToString(UserPermissionsAsXml);
        }
        catch { }
        e.Data["DBxDocProvider.ViewOtherUsersActionPermission"] = ViewOtherUsersActionPermission;
      }
      catch { }
    }

    #endregion

    #region Отладка

#if DEBUG

    /// <summary>
    /// Время создания объекта DBxDocProvider для отладки
    /// Не использовать в прикладном коде.
    /// </summary>
    public DateTime DebugCreateTime { get { return _DebugCreateTime; } }
    private DateTime _DebugCreateTime;

#if DEBUG_STACK

    /// <summary>
    /// Стек вызова при создании объекта для отладки.
    /// Не использовать в прикладном коде.
    /// </summary>
    public string DebugConstructorStackTrace { get { return _DebugConstructorStackTrace; } }
    private string _DebugConstructorStackTrace;
#endif

    /// <summary>
    /// Список существующих объектов DBxDocprovider для отладки.
    /// Не использовать в прикладном коде.
    /// </summary>
#if DEBUG_LIST
    public static readonly WeakReferenceCollection<DBxDocProvider> DebugDocProviderList = new WeakReferenceCollection<DBxDocProvider>();
#else
    public static readonly WeakReferenceCollection<DBxDocProvider> DebugDocProviderList = null;
#endif

#endif

    #endregion
  }


  /// <summary>
  /// Специальный тип исключения, генерируемого при невозможности удаления 
  /// документа или поддокумента из-за наличие ссылок на него в других документах.
  /// </summary>
  [Serializable]
  public class DBxDocCannotDeleteException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает исключение
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DBxDocCannotDeleteException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DBxDocCannotDeleteException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Информация о версии, времени и пользователе, который выполнил создание или изменение документа
  /// </summary>
  [Serializable]
  public sealed class DBxDocServiceInfo
  {
    #region Свойства

    /// <summary>
    /// Версия документа.
    /// Документ получает версию 1 при создании.
    /// Если DBxDocTypes.UserVersions=false, свойство возвращает 0.
    /// </summary>
    public int Version { get { return _Version; } internal set { _Version = value; } }
    private int _Version;

    /// <summary>
    /// Возвращает true, если документ помечен на удаление.
    /// Если документы удаляются сразу (DBxDocTypes.UseDeleted=false), свойство всегда возвращает false.
    /// </summary>
    public bool Deleted { get { return _Deleted; } internal set { _Deleted = value; } }
    private bool _Deleted;

    /// <summary>
    /// Идентификатор пользователя, который создал документ.
    /// Если DBxDocTypes.UseUsers=false, свойство возвращает 0.
    /// Если у текущего пользователя нет прав на просмотр истории изменения документов, свойство возвращает 0.
    /// </summary>
    public Int32 CreateUserId { get { return _CreateUserId; } internal set { _CreateUserId = value; } }
    private Int32 _CreateUserId;

    /// <summary>
    /// Время создания документа.
    /// Если DBxDocTypes.UseTime=false, свойство возвращает null.
    /// Если у текущего пользователя нет прав на просмотр истории изменения документов, свойство возвращает null.
    /// </summary>
    public DateTime? CreateTime { get { return _CreateTime; } internal set { _CreateTime = value; } }
    private DateTime? _CreateTime;

    /// <summary>
    /// Идентификатор пользователя, который последним изменил документ.
    /// Если документ не менялся после создания (Version=1), возвращает копию свойства CreateUserId.
    /// Если DBxDocTypes.UseUsers=false, свойство возвращает 0.
    /// Если у текущего пользователя нет прав на просмотр истории изменения документов, свойство возвращает 0.
    /// </summary>
    public Int32 ChangeUserId { get { return _ChangeUserId; } internal set { _ChangeUserId = value; } }
    private Int32 _ChangeUserId;

    /// <summary>
    /// Время последнего изменения документа.
    /// Если документ не менялся после создания (Version=1), возвращает копию свойства CreateTime.
    /// Если DBxDocTypes.UseTime=false, свойство возвращает null.
    /// Если у текущего пользователя нет прав на просмотр истории изменения документов, свойство возвращает null.
    /// </summary>
    public DateTime? ChangeTime { get { return _ChangeTime; } internal set { _ChangeTime = value; } }
    private DateTime? _ChangeTime;

    #endregion
  }
}
