// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#if DEBUG
//#define DEBUG_LIST // Если определено, то будет отслеживаться список существующих провайдеров
//#define DEBUG_STACK // Если определено, в отладочной информации будет стек вызовов для конструктора DBxDocProvider
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;
using FreeLibSet.IO;
using FreeLibSet.Config;
using System.Xml;
using System.Threading;
using FreeLibSet.Remoting;
using System.Diagnostics;
using FreeLibSet.Caching;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data.Docs
{
  /*
   * 07.07.2022.
   * Mono выполняет аварийное завершение программы при сетевом (или междоменном) вызове абстрактного виртуального метода в MarshalByRefObject.
   * Поэтому в MBR-классе не должно быть методов с сигнатурой public abstract и public virtual.
   * 
   * Есть 2 варианта исправления.
   * 
   * 1. Создать интерфейс IDBxDocProvider. Реализуется в DBxRealDocProvider и DBxChainDocProvider.
   * Эти классы наследуют класс DBxDocProviderBase, куда переносятся неабстрактные методы (не реализует интерфейсы).
   * 
   * 2. Не менять состав классов, но сделать все public-методы невиртуальными. Они вызывают виртуальные методы DoXXX()
   * 
   * Второй способ проще. Есть много групп методов, в которых только один является абстрактным, а остальные его вызывают.
   * Для первого способа пришлось бы делать класс DBxDocProviderBase абстрактным, чтобы можно было перенести туда методы
   */


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

#if XXX
    /// <summary>
    /// Текущее время сервера.
    /// Для DBxRealDocProvider возвращает DateTime.Now
    /// Для DBxChainDocProvider, чтобы не обращаться к серверу каждый раз, возвращает текущее время клиента
    /// с добавлением разности времени, которая вычисляется однократно при создании объекта
    /// </summary>
    public abstract DateTime ServerTime { get;}
#endif

    #region Длительная блокировка

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docSel">Выборка документов, которую требуется заблокировать</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public Guid AddLongLock(DBxDocSelection docSel)
    {
      return DoAddLongLock(docSel);
    }

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docSel">Выборка документов, которую требуется заблокировать</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    protected abstract Guid DoAddLongLock(DBxDocSelection docSel);

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    public Guid AddLongLock(string docTypeName, Int32[] docIds)
    {
      DBxDocSelection docSel = new DBxDocSelection(DBIdentity);
      docSel.Add(docTypeName, docIds);
      return DoAddLongLock(docSel);
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
      DBxDocSelection docSel = new DBxDocSelection(DBIdentity);
      docSel.Add(docTypeName, docIds);
      return DoAddLongLock(docSel);
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
      DBxDocSelection docSel = new DBxDocSelection(DBIdentity);
      docSel.Add(docTypeName, docId);
      return DoAddLongLock(docSel);
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    public bool RemoveLongLock(Guid lockGuid)
    {
      return DoRemoveLongLock(lockGuid);
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    protected abstract bool DoRemoveLongLock(Guid lockGuid);

    #endregion

    #region Вычисляемые поля

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    public void RecalcColumns(string docTypeName, Int32[] docIds)
    {
      DoRecalcColumns(docTypeName, docIds);
    }

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    protected abstract void DoRecalcColumns(string docTypeName, Int32[] docIds);

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
    public UserPermissions UserPermissions 
    {
      get { return DoGetUserPermissions(); }
    }

    /// <summary>
    /// Права, назначенные пользователю (включая классы, определенные в программе).
    /// Так как UserPermissions не является сериализуемым то может возникнуть ошибка при передаче через
    /// границы домена / приложения, если не предусмотрено создание объектов в целевом домене
    /// </summary>
    protected abstract UserPermissions DoGetUserPermissions();
    

    /// <summary>
    /// Сброс прав пользователя
    /// </summary>
    public void ResetPermissions()
    {
      DoResetPermissions();
    }

    /// <summary>
    /// Сброс прав пользователя
    /// </summary>
    protected virtual void DoResetPermissions()
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
    public void TestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason)
    {
      DoTestDocument(doc, reason);
    }

    /// <summary>
    /// Вызывает DocPermissions.TestDocument().
    /// Метод может быть переопределен производным классом
    /// </summary>
    /// <param name="doc">Проверяемый документ</param>
    /// <param name="reason">Режим доступа</param>
    [DebuggerStepThrough]
    protected virtual void DoTestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason)
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

    #region GetTemplate()

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
      bool allSystemColumns = false;
      if (String.IsNullOrEmpty(subDocTypeName) && columnNames.ContainsAny(_AuxDocSysColumns))
        allSystemColumns = true;
      DataTable table1 = GetTemplate(docTypeName, subDocTypeName, allSystemColumns);

      // Оставляем только нужные поля
      DataTable table2 = new DataTable(table1.TableName);
      columnNames.AddContainedColumns(table2.Columns, table1.Columns);
      return table2;
    }

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

      string tableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;

      DataTable table1 = GetBufferedTemplate(tableName);

      // Возвращаем копию набора. Оригинал потом пригодится
      DataTable table2 = table1.Clone();
      if (allSystemColumns)
      {
        // Добавляем недостающие системные поля
        if (String.IsNullOrEmpty(subDocTypeName))
        {
          if (DocTypes.UseTime)
            table2.Columns.Add("CreateTime", typeof(DateTime));
          if (DocTypes.UseUsers)
            table2.Columns.Add("CreateUserId", typeof(Int32));
          if (DocTypes.UseTime)
            table2.Columns.Add("ChangeTime", typeof(DateTime));
          if (DocTypes.UseUsers)
            table2.Columns.Add("ChangeUserId", typeof(Int32));
        }
      }

      SerializationTools.SetUnspecifiedDateTimeMode(table2);
      return table2;
    }

    #endregion

    #region Буферизация структур таблиц

    /// <summary>
    /// Внутреннее хранилище структур таблиц, чтобы не запрашивать
    /// одно и тоже
    /// </summary>
    private SyncDictionary<String, DataTable> _BufStructs;

    /// <summary>
    /// Возвращает буферизованное описание таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Таблица, которую нельзя менять</returns>
    private DataTable GetBufferedTemplate(string tableName)
    {
      if (_BufStructs == null)
        _BufStructs = new SyncDictionary<string, DataTable>();

      DataTable table1;
      if (!_BufStructs.TryGetValue(tableName, out table1))
      {
        DBxDocTypeBase dtb = DocTypes.FindByTableName(tableName);
        if (dtb == null)
          throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\"", "tableName");


        table1 = DoGetTemplate(tableName, dtb.IsSubDoc);
        _BufStructs[tableName] = table1; // при асинхронном вызове ничего плохого не случится
      }
      return table1;
    }

    private static DBxColumns _AuxDocSysColumns = new DBxColumns("CreateTime,CreateUserId,ChangeTime,ChangeUserId");

    /// <summary>
    /// Получение структуры таблицы документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя типа документа или поддокумента</param>
    /// <param name="isSubDoc">Поддокумент?</param>
    /// <returns>Пустая таблица, содержащая структуру. В ней присутствует поле Id </returns>
    private DataTable DoGetTemplate(string tableName, bool isSubDoc)
    {
      CheckThread();

      DBxTableStruct ts = StructSource.GetTableStruct(tableName);

      DataTable table = new DataTable(tableName);

      for (int i = 0; i < ts.Columns.Count; i++)
      {
        DBxColumnStruct colDef = ts.Columns[i];

        #region Пропускаем некоторые служебные поля

        if (isSubDoc)
        {
          switch (colDef.ColumnName)
          {
            case "StartVersion":
            case "Version2":
              continue;
          }
        }
        else // документ
        {
          switch (colDef.ColumnName)
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

        if (DBPermissions.ColumnModes[ts.TableName, colDef.ColumnName] == DBxAccessMode.None)
          continue;

        DataColumn col = colDef.CreateDataColumn();
        if ((!col.AllowDBNull) && col.DefaultValue is DBNull)
        {
          // Так нельзя. Значение по умолчанию должно задаваться.
          // В процессе редактирования в таблицу добавляется пустая строка-заготовка.
          // Если нет значения по умолчанию, то возникнет ошибка в DataTable.Rows.Add().
          //if (Col.DataType!=typeof(DateTime)) // нулевая дата - неподходящее значение

          if (String.IsNullOrEmpty(colDef.MasterTableName))
          {
            // Но для Id и DocId такое делать не надо        
            switch (colDef.ColumnName)
            {
              case "Id":
              //case "DocId":
              case "Version":
              case "Deleted":
                col.AllowDBNull = true;
                break;
              default:
                col.DefaultValue = DataTools.GetEmptyValue(col.DataType);
                break;
            }
          }
          else
            col.AllowDBNull = true; // В ссылочные поля точно не нужно записывать 0, включая поле "DocId" 
        }

        table.Columns.Add(col);
      } // цикл по описаниям столбцов

      return table;
    }

    #endregion

    #region GetColumnNameIndexer()

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
        DataTable table = GetBufferedTemplate(tableName);

        indexer = new StringArrayIndexer(DataTools.GetColumnNames(table), true);
        _ColumnNameIndexers[tableName] = indexer; // 28.08.2020 - обеспечение потокобезопасности
      }
      return indexer;
    }
    private SyncDictionary<string, StringArrayIndexer> _ColumnNameIndexers;

    #endregion

    #region GetColumnDef()

#if !XXX
    // Эта перегрузка потенциально опасна, вдруг таблица, возвращаемая SELECT(), имеет другой порядок столбцов. По идее, не должно, но кто его знает. Лучше обращаться по имени столбца
    // Зато быстрее

    /// <summary>
    /// Используется в реализациях интерфейса IDBxDocValue
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>Описание столбца</returns>
    internal DataColumn GetColumnDef(string tableName, int columnIndex)
    {
      return GetBufferedTemplate(tableName).Columns[columnIndex];
    }
#else
    /// <summary>
    /// Используется в реализациях интерфейса IDBxDocValue
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    internal DataColumn GetColumnDef(string tableName, string columnName)
    {
      return GetBufferedTemplate(tableName).Columns[columnName];
    }
#endif

    #endregion

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

      string tableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;

      if (_BufColumns == null)
        _BufColumns = new SyncDictionary<string, DBxColumns>();

      DBxColumns columns;
      if (!_BufColumns.TryGetValue(tableName, out columns))
      {
        columns = DBxColumns.FromColumns(GetTemplate(docTypeName, subDocTypeName).Columns);
        _BufColumns[tableName] = columns; // 28.08.2020 Обеспечение потокобезопасности
      }
      return columns;
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
    public DataTable LoadDocData(string docTypeName, Int32[] docIds)
    {
      return DoLoadDocData(docTypeName, docIds);
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Таблица документов</returns>
    protected abstract DataTable DoLoadDocData(string docTypeName, Int32[] docIds);

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    public DataTable LoadDocData(string docTypeName, DBxFilter filter)
    {
      return DoLoadDocData(docTypeName, filter);
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    protected abstract DataTable DoLoadDocData(string docTypeName, DBxFilter filter);

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    public DataTable LoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      return DoLoadSubDocData(docTypeName, subDocTypeName, docIds);
    }

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    protected abstract DataTable DoLoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds);

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
        DBxClearCacheData data = CreateClearCacheData(dataSet);
        OnCacheClearing(data);
      }

      return DoApplyChanges(dataSet, reloadData);
    }

    private DBxClearCacheData CreateClearCacheData(DataSet dataSet)
    {
      DBxClearCacheData data = new DBxClearCacheData();
      foreach (DataTable table in dataSet.Tables)
      {
        foreach (DataRow row in table.Rows)
        {
          try
          {
            Int32 Id;
            switch (row.RowState)
            {
              case DataRowState.Added:
              case DataRowState.Modified:
                Id = (Int32)row["Id"];
                break;
              case DataRowState.Deleted:
                Id = (Int32)(row["Id", DataRowVersion.Original]);
                break;
              default:
                continue;
            }

            // 25.06.2021
            // Фиктивные идентификаторы тоже добавляем. Они маркируют добавление записи
            //if (IsRealDocId(Id))
            //{
            data.Add(table.TableName, Id);
            //}
          }
          catch
          {
          }
        }
      }

      return data;
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
    protected abstract DataSet DoApplyChanges(DataSet dataSet, bool reloadData);

    /// <summary>
    /// Создает и запускает процедуру для асинхронной работы на сервере.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    /// <returns></returns>
    public DistributedCallData StartServerExecProc(NamedValues args)
    {
      return DoStartServerExecProc(args);
    }

    /// <summary>
    /// Создает и запускает процедуру для асинхронной работы на сервере.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    /// <returns></returns>
    protected abstract DistributedCallData DoStartServerExecProc(NamedValues args);

    #endregion

    #region Получение информации для документа

    /// <summary>
    /// Получить таблицу истории для документа
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Таблица истории</returns>
    public DataTable GetDocHistTable(string docTypeName, Int32 docId)
    {
      return DoGetDocHistTable(docTypeName, docId);
    }

    /// <summary>
    /// Получить таблицу истории для документа
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Таблица истории</returns>
    protected abstract DataTable DoGetDocHistTable(string docTypeName, Int32 docId);

    /// <summary>
    /// Получить таблицу действий пользователя или всех пользователей за определенный период
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    /// <param name="userId">Идентификатор пользователя. 0-все пользователи</param>
    /// <param name="singleDocTypeName">Имя таблицы документа. Пустая строка - документы всех видов</param>
    /// <returns>Таблица действий</returns>
    public DataTable GetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      return DoGetUserActionsTable(firstDate, lastDate, userId, singleDocTypeName);
    }

    /// <summary>
    /// Получить таблицу действий пользователя или всех пользователей за определенный период
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    /// <param name="userId">Идентификатор пользователя. 0-все пользователи</param>
    /// <param name="singleDocTypeName">Имя таблицы документа. Пустая строка - документы всех видов</param>
    /// <returns>Таблица действий</returns>
    protected abstract DataTable DoGetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName);

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    public DataTable GetUserActionDocTable(Int32 actionId)
    {
      return DoGetUserActionDocTable(actionId);
    }

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    protected abstract DataTable DoGetUserActionDocTable(Int32 actionId);

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    public DateTime? GetUserActionsLastTime(Int32 userId)
    {
      return DoGetUserActionsLastTime(userId);
    }

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    protected abstract DateTime? DoGetUserActionsLastTime(Int32 userId);

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
      return DoGetDocRefTable(docTypeName, docId, showDeleted, unique, null, 0);
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
    public DataTable GetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      return DoGetDocRefTable(docTypeName, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId);
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
    protected abstract DataTable DoGetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId);

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
    public DataTable LoadDocDataVersion(string docTypeName, Int32 docId, int docVersion)
    {
      return DoLoadDocDataVersion(docTypeName, docId, docVersion);
    }

    /// <summary>
    /// Получить таблицу версий строк документа.
    /// Возвращает таблицу с одной строкой
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    protected abstract DataTable DoLoadDocDataVersion(string docTypeName, Int32 docId, int docVersion);

    /// <summary>
    /// Получить таблицу версий строк поддокументов.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    public DataTable LoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int docVersion)
    {
      return DoLoadSubDocDataVersion(docTypeName, subDocTypeName, docId, docVersion);
    }

    /// <summary>
    /// Получить таблицу версий строк поддокументов.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    protected abstract DataTable DoLoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int docVersion);

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
    public DataSet LoadUnformattedDocData(string docTypeName, Int32 docId)
    {
      return DoLoadUnformattedDocData(docTypeName, docId);
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
    protected abstract DataSet DoLoadUnformattedDocData(string docTypeName, Int32 docId);

    #endregion

    #region Прямые запросы в базу данных

    /*
     * В целях безопасности соединение с базой данной недоступно через провайдер документов.
     * Доступны только некоторые вызовы
     */

    #region DBCache

    /// <summary>
    /// Доступ к буферизованным данным.
    /// </summary>
    public DBxCache DBCache { get { return DoGetDBCache(); } }

    /// <summary>
    /// Доступ к буферизованным данным.
    /// Если метод не переопределен, возвращается null.
    /// </summary>
    protected virtual DBxCache DoGetDBCache()
    {
      return null;  
    }

    /// <summary>
    /// Сброс кэшированных данных
    /// </summary>
    public void ClearCache()
    {
      DoClearCache();
    }

    /// <summary>
    /// Сброс кэшированных данных
    /// </summary>
    protected virtual void DoClearCache()
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

        List<DataRow> rows = null;

        foreach (DataRow row in ds.Tables[i].Rows)
        {
          if (row.RowState == DataRowState.Deleted)
            continue;
          Int32 id = (Int32)(row["Id"]);
          if (IsRealDocId(id))
          {
            if (rows == null)
              rows = new List<DataRow>();
            rows.Add(row);
          }
        }

        if (rows != null)
          tc.UpdateRows(rows.ToArray());

      } // цикл по таблицам
    }

    #endregion

    #region GetDocServiceInfo()

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

      DBxColumns cols = AllDocServiceColumns;
      object[] values;
      if (fromCache)
        values = DBCache[docTypeName].GetValues(docId, cols);
      else
        values = GetValues(docTypeName, docId, cols);

      DBxDocServiceInfo info = new DBxDocServiceInfo();

      if (DocTypes.UseVersions)
        info.Version = DataTools.GetInt(values[cols.IndexOf("Version")]);
      if (DocTypes.UseDeleted)
        info.Deleted = DataTools.GetBool(values[cols.IndexOf("Deleted")]);

      if (DocTypeViewHistoryPermission.GetAllowed(this.UserPermissions, docTypeName))
      {
        if (DocTypes.UseUsers)
        {
          info.CreateUserId = DataTools.GetInt(values[cols.IndexOf("CreateUserId")]);
          info.ChangeUserId = DataTools.GetInt(values[cols.IndexOf("ChangeUserId")]);
          if (info.ChangeUserId == 0)
            info.ChangeUserId = info.CreateUserId;
        }
        if (DocTypes.UseTime)
        {
          DateTime? dt = DataTools.GetNullableDateTime(values[cols.IndexOf("CreateTime")]);
          if (dt.HasValue)
            info.CreateTime = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
          dt = DataTools.GetNullableDateTime(values[cols.IndexOf("ChangeTime")]);
          if (dt.HasValue)
            info.ChangeTime = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
          else
            info.ChangeTime = info.CreateTime;
        }
      }
      return info;
    }

    #endregion

    #region IDBxConReadOnlyBase Members

    #region FillSelect()

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
    public DataTable FillSelect(DBxSelectInfo info)
    {
      return DoFillSelect(info);
    }

    /// <summary>
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    protected abstract DataTable DoFillSelect(DBxSelectInfo info);

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    public DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      return DoFillUniqueColumnValues(tableName, columnName, where);
    }

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    protected abstract DataTable DoFillUniqueColumnValues(string tableName, string columnName, DBxFilter where);

    #endregion

    #region GetRecordCount() и IsTableEmpty()

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    public int GetRecordCount(string tableName)
    {
      return DoGetRecordCount(tableName);
    }

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    protected abstract int DoGetRecordCount(string tableName);

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    public int GetRecordCount(string tableName, DBxFilter where)
    {
      return DoGetRecordCount(tableName, where);
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    protected abstract int DoGetRecordCount(string tableName, DBxFilter where);

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    public bool IsTableEmpty(string tableName)
    {
      return DoIsTableEmpty(tableName);
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    protected abstract bool DoIsTableEmpty(string tableName);

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
    public object GetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      return DoGetMaxValue(tableName, columnName, where);
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
    protected abstract object DoGetMaxValue(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить минимальное значение числового поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение</returns>
    public object GetMinValue(string tableName, string columnName, DBxFilter where)
    {
      return DoGetMinValue(tableName, columnName, where);
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
    protected abstract object DoGetMinValue(string tableName, string columnName, DBxFilter where);

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
    public object[] GetValuesForMax(string tableName, DBxColumns columnNames,
      string maxColumnName, DBxFilter where)
    {
      return DoGetValuesForMax(tableName, columnNames, maxColumnName, where);
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
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    protected abstract object[] DoGetValuesForMax(string tableName, DBxColumns columnNames,
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
    public object[] GetValuesForMin(string tableName, DBxColumns columnNames,
      string minColumnName, DBxFilter where)
    {
      return DoGetValuesForMin(tableName, columnNames, minColumnName, where);
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
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    protected abstract object[] DoGetValuesForMin(string tableName, DBxColumns columnNames,
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
    public object GetSumValue(string tableName, string columnName, DBxFilter where)
    {
      return DoGetSumValue(tableName, columnName, where);
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
    protected abstract object DoGetSumValue(string tableName, string columnName, DBxFilter where);

    #endregion

    #endregion

    #region IDBxConReadOnlyPKInt32 Members

    #region GetValue(), GetValues()

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
      DoGetValue(tableName, id, columnName, out value);
      return value;
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
    public bool GetValue(string tableName, Int32 id, string columnName, out object value)
    {
      return DoGetValue(tableName, id, columnName, out value);
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
    protected abstract bool DoGetValue(string tableName, Int32 id, string columnName, out object value);

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
    public object[] GetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      return DoGetValues(tableName, id, columnNames);
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
    protected abstract object[] DoGetValues(string tableName, Int32 id, DBxColumns columnNames);

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
      DBxColumns columnNames2 = new DBxColumns(columnNames);
      return DoGetValues(tableName, id, columnNames2);
    }

    #endregion

    #region GetIds()

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    public IdList GetIds(string tableName, DBxFilter where)
    {
      return DoGetIds(tableName, where);
    }

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    protected abstract IdList DoGetIds(string tableName, DBxFilter where);

    /// <summary>
    /// Получить массив идентификаторов строк с заданным значением поля
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, string columnName, object value)
    {
      return DoGetIds(tableName, new ValueFilter(columnName, value));
    }


    /// <summary>
    /// Получить массив идентификаторов строк с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Массив идентификаторов строк</returns>
    public IdList GetIds(string tableName, System.Collections.IDictionary columnNamesAndValues)
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
      return DoGetIds(tableName, filter);
    }

    #endregion

    #region FindRecord()

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
    public Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      return DoFindRecord(tableName, where, orderBy);
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
    protected abstract Int32 DoFindRecord(string tableName, DBxFilter where, DBxOrder orderBy);

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="singleOnly">Если true и найдено больше одной записи, удовлетворяющей условию
    /// <paramref name="where"/>, то возвращается 0</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    public Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      return DoFindRecord(tableName, where, singleOnly);
    }

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="singleOnly">Если true и найдено больше одной записи, удовлетворяющей условию
    /// <paramref name="where"/>, то возвращается 0</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    protected abstract Int32 DoFindRecord(string tableName, DBxFilter where, bool singleOnly);

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
      return DoFindRecord(tableName, new ValueFilter(columnName, value), false);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, System.Collections.IDictionary columnNamesAndValues)
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
      return DoFindRecord(tableName, where, false);
    }

    #endregion

    #region GetUnuqueValues()

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
    public string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      return DoGetUniqueStringValues(tableName, columnName, where);
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
    protected abstract string[] DoGetUniqueStringValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueIntValues(tableName, columnName, where);
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
    protected abstract int[] DoGetUniqueIntValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueInt64Values(tableName, columnName, where);
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
    protected abstract long[] DoGetUniqueInt64Values(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueSingleValues(tableName, columnName, where);
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
    protected abstract float[] DoGetUniqueSingleValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueDoubleValues(tableName, columnName, where);
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
    protected abstract double[] DoGetUniqueDoubleValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueDecimalValues(tableName, columnName, where);
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
    protected abstract decimal[] DoGetUniqueDecimalValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueDateTimeValues(tableName, columnName, where);
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
    protected abstract DateTime[] DoGetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where);

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
      return DoGetUniqueGuidValues(tableName, columnName, where);
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
    protected abstract Guid[] DoGetUniqueGuidValues(string tableName, string columnName, DBxFilter where);

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
      AndFilter filter = new AndFilter(new ValueFilter("DocId", docId), DBSSubDocType.DeletedFalseFilter);
      DataTable table = FillSelect(subDocTypeName, DBSSubDocType.IdColumns, filter);
      return DataTools.GetIds(table);
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
      Int32 loopedId;
      return DoGetInheritorIds(tableName, parentIdColumnName, parentId, nested, null, out loopedId);
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
      Int32 loopedId;
      return DoGetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
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
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      return DoGetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
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
    protected abstract IdList DoGetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId);

    #endregion

    #endregion

    #region Файлы и двоичные данные

    /// <summary>
    /// Идентификатор двоичных данных или файла, дополненный идентификатором документа и поддокумента.
    /// Эта структура не используется в прикладном коде
    /// </summary>
    [Serializable]
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

      DBxDocType docType;
      DBxSubDocType subDocType;
      if (!DocTypes.FindByTableName(tableName, out docType, out subDocType))
        throw new ArgumentException("Таблица \"" + tableName + "\" не относится к документу или поддокументу");

      List<DocSubDocDataId> dummyIds = new List<DocSubDocDataId>();

      if (subDocType == null)
      {
        Int32 binDataId = DataTools.GetInt(GetValue(tableName, id, columnName));
        return InternalGetBinData1(tableName, columnName, new DocSubDocDataId(id, 0, binDataId), 0, dummyIds);
      }
      else
      {
        // Поддокумент
        object[] values = GetValues(tableName, id, new DBxColumns(new string[] { columnName, "DocId" }));
        Int32 binDataId = DataTools.GetInt(values[0]);
        Int32 docId = DataTools.GetInt(values[1]);
        return InternalGetBinData1(tableName, columnName, new DocSubDocDataId(docId, id, binDataId), 0, dummyIds);
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
      byte[] data = GetBinData(tableName, id, columnName);
      if (data == null)
        return null;
      else
        return DataTools.XmlDocumentFromByteArray(data);
    }

    /// <summary>
    /// Внутренний метод получения идентификатора двоичных данных
    /// </summary>
    /// <param name="md5">Контрольная сумма</param>
    /// <returns>Идентификатор записи или 0, если таких данных нет</returns>
    public Int32 InternalFindBinData(string md5)
    {
      return DoInternalFindBinData(md5);
    }

    /// <summary>
    /// Внутренний метод получения идентификатора двоичных данных
    /// </summary>
    /// <param name="md5">Контрольная сумма</param>
    /// <returns>Идентификатор записи или 0, если таких данных нет</returns>
    protected abstract Int32 DoInternalFindBinData(string md5);

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
    public Dictionary<Int32, byte[]> InternalGetBinData2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      return DoInternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds);
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
    protected abstract Dictionary<Int32, byte[]> DoInternalGetBinData2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds);

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

      Int32 fileId = DataTools.GetInt(GetValue(tableName, id, columnName));
      return GetDBFileInfo(fileId);
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

      DBxDocType docType;
      DBxSubDocType subDocType;
      if (!DocTypes.FindByTableName(tableName, out docType, out subDocType))
        throw new ArgumentException("Таблица \"" + tableName + "\" не относится к документу или поддокументу");

      if (subDocType == null)
      {
        Int32 FileId = DataTools.GetInt(GetValue(tableName, id, columnName));
        return InternalGetDBFile1(tableName, columnName, new DocSubDocDataId(id, 0, FileId), 0, dummyIds);
      }
      else
      {
        // Поддокумент
        object[] values = GetValues(tableName, id, new DBxColumns(new string[] { columnName, "DocId" }));
        Int32 fileId = DataTools.GetInt(values[0]);
        Int32 docId = DataTools.GetInt(values[1]);
        return InternalGetDBFile1(tableName, columnName, new DocSubDocDataId(docId, id, fileId), 0, dummyIds);
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
    public Int32 InternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId)
    {
      return DoInternalFindDBFile(fileInfo, md5, out binDataId);
    }

    /// <summary>
    /// Внутренний метод получения идентификатора хранимого файла
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="md5">Контрольная сумма содержимого файла</param>
    /// <param name="binDataId">Сюда помещается идентификатор двоичных данных,
    /// возвращаемый методом InternalFindBinData()</param>
    /// <returns>Идентификатор записи файла в базе данных</returns>
    protected abstract Int32 DoInternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId);


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
    public Dictionary<Int32, FileContainer> InternalGetDBFile2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds)
    {
      return DoInternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
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
    protected abstract Dictionary<Int32, FileContainer> DoInternalGetDBFile2(string tableName, string columnName, DocSubDocDataId wantedId, int docVersion, List<DocSubDocDataId> preloadIds);

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
    public DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      return DoLoadCachePages(request);
    }

    /// <summary>
    /// Загрузить страницы кэша
    /// </summary>
    /// <param name="request">Данные запроса</param>
    /// <returns>Загруженные страницы страницы</returns>
    protected abstract DBxCacheLoadResponse DoLoadCachePages(DBxCacheLoadRequest request);

    /// <summary>
    /// Очистить страницы таблицы кэша
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Начальные идентификаторы страниц</param>
    public void ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      DoClearCachePages(tableName, columnNames, firstIds);
    }

    /// <summary>
    /// Очистить страницы таблицы кэша
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Начальные идентификаторы страниц</param>
    protected abstract void DoClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds);

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано имя существующего документа или поддокумента</param>
    /// <param name="id">Идентификатор. Если задано значение 0, возвращается пустая строка</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(string tableName, Int32 id)
    {
      return DoGetTextValue(tableName, id);
    }

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано имя существующего документа или поддокумента</param>
    /// <param name="id">Идентификатор. Если задано значение 0, возвращается пустая строка</param>
    /// <returns>Текстовое представление</returns>
    protected abstract string DoGetTextValue(string tableName, Int32 id);

    /// <summary>
    /// Получение текстового представления для нескольких документов или поддокументов одного вида.
    /// Этот метод можно использовать, например, для вывода текста в табличках фильтров.
    /// Количество строк в возвращаемом массиве равно <paramref name="ids"/>.Length.
    /// Если <paramref name="ids"/>=null или задан пустой массив, возвращается пустой массив строк.
    /// Массив может содержать повторяющиеся идентификаторы. Для идентификаторов 0 возвращаются пустые строки.
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано имя существующего документа или поддокумента</param>
    /// <param name="ids">Массив идентификаторов. Может быть null или пустым массивом</param>
    /// <returns>Текстовое представление</returns>
    public string[] GetTextValues(string tableName, Int32[] ids)
    {
      // Пока нет смысла делать методы GetTextValues() виртуальными.

      if (ids == null)
        return DataTools.EmptyStrings;
      if (ids.Length == 0)
        return DataTools.EmptyStrings;
      string[] a = new string[ids.Length];
      for (int i = 0; i < ids.Length; i++)
        a[i] = GetTextValue(tableName, ids[i]);
      return a;
    }

    /// <summary>
    /// Получение текстового представления для нескольких документов или поддокументов одного вида.
    /// Этот метод можно использовать, например, для вывода текста в табличках фильтров.
    /// Количество строк в возвращаемом массиве равно <paramref name="ids"/>.Count.
    /// Если <paramref name="ids"/>=null или задан пустой список, возвращается пустой массив строк.
    /// </summary>
    /// <param name="tableName">Имя таблицы. Должно быть задано имя существующего документа или поддокумента</param>
    /// <param name="ids">Список идентификаторов. Может быть null или пустым списком</param>
    /// <returns>Текстовое представление</returns>
    public string[] GetTextValues(string tableName, IdList ids)
    {
      // Пока нет смысла делать методы GetTextValues() виртуальными.

      if (ids == null)
        return DataTools.EmptyStrings;
      if (ids.Count == 0)
        return DataTools.EmptyStrings;
      string[] a = new string[ids.Count];
      int cnt = 0;
      foreach (Int32 id in ids)
      {
        a[cnt] = GetTextValue(tableName, id);
        cnt++;
      }
      return a;
    }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName
    {
      get
      {
        // Не буферизуем строку, на случай, если имя пользователя поменяется динамически

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
      DataSet pimaryDS = new DataSet();
      DataTable table = row.Table.Clone(); // пустая таблица
      //DataTools.SetPrimaryKey(Table, "Id");
      Int32 id;
      if (row.RowState == DataRowState.Deleted)
      {
        table.Rows.Add(DataTools.GetRowValues(row, DataRowVersion.Original));
        id = (Int32)(row["Id", DataRowVersion.Original]);
      }
      else
      {
        table.Rows.Add(row.ItemArray);
        id = (Int32)(row["Id"]);
      }
      pimaryDS.Tables.Add(table);
      pimaryDS.AcceptChanges();

      return InternalGetTextValue(table.TableName, id, pimaryDS);
    }

    /// <summary>
    /// Внутренний метод получения текста.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="primaryDS">Первичный набор данных</param>
    /// <returns>Текст для документа или поддокумента</returns>
    public /*internal protected */ string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      return DoInternalGetTextValue(tableName, id, primaryDS);
    }
    // Нельзя сделать internal protected, т.к. будет ошибка вызова удаленного метода


    /// <summary>
    /// Внутренний метод получения текста.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="primaryDS">Первичный набор данных</param>
    /// <returns>Текст для документа или поддокумента</returns>
    protected abstract string DoInternalGetTextValue(string tableName, Int32 id, DataSet primaryDS);

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
      foreach (DataTable table in ds.Tables)
      {
        int pId = table.Columns.IndexOf("Id"); // обычно, 0
        foreach (DataRow row in table.Rows)
        {
          if (row.RowState == DataRowState.Deleted)
            continue;

          Int32 Id = DataTools.GetInt(row[pId]);
          if (Id <= 0)
            throw new BugException("Недопустимый идентификатор " + Id.ToString() + " в таблице \"" + table.TableName + "\"");
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
      foreach (DataTable table in dataSet.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        switch (table.TableName)
        {
          case "BinData":
          case "FileNames":
            continue; // эти таблицы не сбрасываются
        }

        // Не выгодно выполнять сброс по одной строке, т.к. будет выполняться обращение к системе кэширования,
        // в том числе, обращение к диску
        IdList ids = new IdList();
        foreach (DataRow row in table.Rows)
        {
          switch (row.RowState)
          {
            case DataRowState.Added:
            case DataRowState.Modified:
              Int32 newId = (Int32)(row["Id"]);
              if (newId > 0) // 19.03.2016 
                ids.Add(newId);
              break;
            case DataRowState.Deleted:
              Int32 oldId = (Int32)(row["Id", DataRowVersion.Original]);
              if (oldId > 0)
                ids.Add(oldId);
              break;
          }
        }

        DBxTableCache tc = dbCache[table.TableName];
        tc.Clear(ids);
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

    /// <summary>
    /// Создание копии провайдера.
    /// Полученный провайдер можно использовать в отдельном потоке
    /// Для цепочечных провайдеров создается также копия исходного провайдера
    /// Этот метод может вызываться из любого потока, даже если текущий DBxDocProvider привязан к другому потоку
    /// </summary>
    /// <returns></returns>
    public DBxDocProvider Clone()
    {
      return DoClone();
    }

    /// <summary>
    /// Создание копии провайдера.
    /// Полученный провайдер можно использовать в отдельном потоке
    /// Для цепочечных провайдеров создается также копия исходного провайдера
    /// Этот метод может вызываться из любого потока, даже если текущий DBxDocProvider привязан к другому потоку
    /// </summary>
    /// <returns></returns>
    protected abstract DBxDocProvider DoClone();

    object ICloneable.Clone()
    {
      return DoClone();
    }

    #endregion

    #region Обработка ошибок

    /// <summary>
    /// Добавляет отладочную информацию в объект исключения.
    /// Используется в блоках catch.
    /// </summary>
    /// <param name="e">Исключение</param>
    public void AddExceptionInfo(Exception e)
    {
      DoAddExceptionInfo(e);
    }

    /// <summary>
    /// Добавляет отладочную информацию в объект исключения.
    /// Используется в блоках catch.
    /// </summary>
    /// <param name="e">Исключение</param>
    protected virtual void DoAddExceptionInfo(Exception e)
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
