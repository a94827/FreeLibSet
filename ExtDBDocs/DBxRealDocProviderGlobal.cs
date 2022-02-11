// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Потокобезопасный класс, хранящий общие данные для базы данных в-целом
  /// Этот класс не сериализуется и существует только на стороне сервера, 
  /// т.к. содержит точки подключения к базе данных без ограничения доступа
  /// </summary>
  public class DBxRealDocProviderGlobal : IDBxCacheSource, IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Эта версия конструктура используется, когда в программе не используются таблицы BinData и FileNames
    /// </summary>
    /// <param name="docTypes">Описания видов документов. Список переводится в режим "Только чтение"</param>
    /// <param name="mainDBEntry">Главная точка входа</param>
    /// <param name="undoDBEntry">Точка входа в базу данных. Может быть null</param>
    public DBxRealDocProviderGlobal(DBxDocTypes docTypes, DBxEntry mainDBEntry, DBxEntry undoDBEntry)
      : this(docTypes, mainDBEntry, undoDBEntry, null)
    {
    }

    /// <summary>
    /// Эта версия конструктура дает возможность использовать таблицы BinData и FileNames
    /// </summary>
    /// <param name="docTypes">Описания видов документов. Список переводится в режим "Только чтение"</param>
    /// <param name="mainDBEntry">Главная точка входа</param>
    /// <param name="undoDBEntry">Точка входа в базу данных. Может быть null</param>
    /// <param name="binDataHandler">Обработчик двоичных данных. Переводится в режим "Только чтение". Может быть null</param>
    public DBxRealDocProviderGlobal(DBxDocTypes docTypes, DBxEntry mainDBEntry, DBxEntry undoDBEntry, DBxBinDataHandler binDataHandler)
    {
      if (docTypes == null)
        throw new ArgumentNullException("docTypes");

      docTypes.SetReadOnly();
      docTypes.CheckStruct(binDataHandler); // 07.12.2017
      _DocTypes = docTypes;

      if (mainDBEntry == null)
        throw new ArgumentNullException("mainDBEntry");
      _MainDBEntry = mainDBEntry;

      if (undoDBEntry != null)
      {
        if (!docTypes.UseDeleted)
          throw new ArgumentException("Ведение истории невозможно, если свойство DBxDocTypes.UseDeleted=false", "undoDBEntry");
        if (!docTypes.UseVersions)
          throw new ArgumentException("Ведение истории невозможно, если свойство DBxDocTypes.UseVersions=false", "undoDBEntry");
        if (!docTypes.UseTime)
          throw new ArgumentException("Ведение истории невозможно, если свойство DBxDocTypes.UseTime=false", "undoDBEntry");
      }
      else
      {
        if (docTypes.UseSessionId)
          throw new ArgumentException("Нельзя устанавливать свойство DBxDocTypes.UseSessionId, если история документов не ведется", "undoDBEntry");
      }
      _UndoDBEntry = undoDBEntry;

      if (binDataHandler == null)
      {
        if (docTypes.HasBinDataRefs || docTypes.HasFileRefs)
          throw new ArgumentNullException("binDataHandler", "Так как в DocTypes задано использование ссылок на двоичные данные, аргумент BinDataHandler должен быть задан");
      }
      else
        binDataHandler.SetReadOnly();
      _BinDataHandler = binDataHandler;

      _BinDataDBStruct = new DBxStruct();
      if (binDataHandler != null)
        binDataHandler.AddMainTableStructs(_BinDataDBStruct);
      _BinDataDBStruct.SetReadOnly();

      _LastUsedTableIds = new Dictionary<string, Int32>();

      _ActiveLocks = new List<DBxShortDocsLock>();
      _LongLocks = new LongTermLockList(this.TextHandlers);

      _ClearCacheBuffer = new DBxClearCacheBuffer(DefaultClearCacheBuffer);
      _TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();
      _StructSource = new InternalStructSource(this);
      _DBCache = new DBxCache(this, false);

      _TextHandlers = new DBxDocTextHandlers(docTypes, DBCache);


      #region MainDocTableServiceColumns и SubDocTableServiceColumns

      DBxColumnList cols;

      cols = new DBxColumnList();
      cols.Add("Id");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
        cols.Add("Version");
      _MainDocTableServiceColumns = new DBxColumns(cols);

      cols = new DBxColumnList();
      cols.Add("Id");
      cols.Add("DocId");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");

      _SubDocTableServiceColumns = new DBxColumns(cols);

      #endregion

      #region AllDocServiceColumns и AllSubDocServiceColumns

      cols = new DBxColumnList();
      cols.Add("Id");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        cols.Add("Version");
        cols.Add("Version2");
      }
      if (docTypes.UseUsers)
      {
        cols.Add("CreateUserId");
        cols.Add("ChangeUserId");
      }
      if (docTypes.UseTime)
      {
        cols.Add("CreateTime");
        cols.Add("ChangeTime");
      }
      _AllDocServiceColumns = new DBxColumns(cols);


      cols = new DBxColumnList();
      cols.Add("Id");
      cols.Add("DocId");
      if (docTypes.UseDeleted)
        cols.Add("Deleted");
      if (docTypes.UseVersions)
      {
        cols.Add("StartVersion");
        cols.Add("Version2");
      }

      _AllSubDocServiceColumns = new DBxColumns(cols);

      #endregion

      DBxDocDebugTools.InitLogout();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Размер кольцевого буфера для хранения обновлений.
    /// При необходимости, свойство может быть изменено до вызова конструктора DBxRealDocProviderGlobal.
    /// Оптимальное значение зависит от соотношения частоты вызова метода Swap() по таймеру на стороне 
    /// сервера к частоте опроса изменений клиентами.
    /// Если буфер маленький, то клиенты будут получать сигнал полной очистки, т.к. между опросами буфер
    /// будет убегать. Если буфер излишне большой, то будет лишний расход памяти
    /// Значение по умолчанию равно 10
    /// </summary>
    public static int DefaultClearCacheBuffer
    {
      get { return _DefaultClearCacheBuffer; }
      set { _DefaultClearCacheBuffer = value; }
    }
    private static int _DefaultClearCacheBuffer = 10;

    /// <summary>
    /// Объявления для документов
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private DBxDocTypes _DocTypes;


    /// <summary>
    /// Подключение к основной базе данных.
    /// Это подключение не содержит ограничений пользователя
    /// Не может быть null
    /// </summary>
    public DBxEntry MainDBEntry { get { return _MainDBEntry; } }
    private DBxEntry _MainDBEntry;

    /// <summary>
    /// Подключение к базе данных undo.
    /// Точка подключения не должна содержать ограничений пользователя, иначе при копировании данных документа 
    /// из базы Main в Undo будут возникать ошибки
    /// Может быть null, если база данных предназначена для чтения, или история не должна храниться. При этом не будет работать доступ к
    /// версиям документов. Однако в документах по-прежнему могут быть служебные поля Version и другие.
    /// Наличие служебных полей определяется управляющими свойствами в DBxDocTypes.
    /// </summary>
    public DBxEntry UndoDBEntry { get { return _UndoDBEntry; } }
    private DBxEntry _UndoDBEntry;

    /// <summary>
    /// Обработчик двоичных данных (таблиц BinData и Filenames)
    /// Может быть null, если хранение таких данных не предусмотрено
    /// </summary>
    public DBxBinDataHandler BinDataHandler { get { return _BinDataHandler; } }
    private DBxBinDataHandler _BinDataHandler;

    /// <summary>
    /// Структура, содержащая объявления таблиц "BinData" и "FileNames".
    /// Одна из таблиц может отсутствовать, если свойства BinDataHandler.UseBinData
    /// или UseFiles не установлено.
    /// Если BinDataHandler=null, содержит пустое объявление структуры, а не null.
    /// </summary>
    internal DBxStruct BinDataDBStruct { get { return _BinDataDBStruct; } }
    private DBxStruct _BinDataDBStruct;

    /// <summary>
    /// Идентификатор основной базы данных
    /// </summary>
    public string DBIdentity { get { return MainDBEntry.DB.DBIdentity; } }


    /// <summary>
    /// Служебные поля в начале таблицы документов, загружаемых в таблицу DBxMultiDocs
    /// </summary>
    internal DBxColumns MainDocTableServiceColumns { get { return _MainDocTableServiceColumns; } }
    private DBxColumns _MainDocTableServiceColumns;

    /// <summary>
    /// Служебные поля в начале таблицы поддокументов, загружаемых в таблицу DBxMultiSubDocs
    /// </summary>
    internal DBxColumns SubDocTableServiceColumns { get { return _SubDocTableServiceColumns; } }
    private DBxColumns _SubDocTableServiceColumns;

    /// <summary>
    /// Все служебные поля в таблицах документов
    /// </summary>
    internal DBxColumns AllDocServiceColumns { get { return _AllDocServiceColumns; } }
    private DBxColumns _AllDocServiceColumns;

    /// <summary>
    /// Все служебные поля в таблицах поддокументов
    /// </summary>
    internal DBxColumns AllSubDocServiceColumns { get { return _AllSubDocServiceColumns; } }
    private DBxColumns _AllSubDocServiceColumns;

    #endregion

    #region Получение идентификаторов для INSERT

    /// <summary>
    /// Последние идентификаторы, использованные для каждой таблицы документов
    /// При обращении этот объект блокируется
    /// </summary>
    private Dictionary<string, Int32> _LastUsedTableIds;

    /// <summary>
    /// Получить следующий свободный идентификатор для нового документа или поддокумента
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="mainDBEntry"></param>
    /// <returns></returns>
    internal Int32 GetNextId(string tableName, DBxEntry mainDBEntry)
    {
      Int32 nextId;

      lock (_LastUsedTableIds)
      {
        Int32 lastId;
        if (!_LastUsedTableIds.TryGetValue(tableName, out lastId))
        {
          using (DBxCon Con = new DBxCon(mainDBEntry))
          {
            lastId = DataTools.GetInt(Con.GetMaxValue(tableName, "Id", null));
          }
          _LastUsedTableIds.Add(tableName, lastId);
        }
        nextId = lastId + 1;
        _LastUsedTableIds[tableName] = nextId;
      }
      return nextId;
    }

    #endregion

    #region Блокировка

    #region Кратковременная

    /// <summary>
    /// Список активных кратковременных блокировок базы данных
    /// При работе с элементами списка должен блокироваться список
    /// </summary>
    internal List<DBxShortDocsLock> ActiveLocks { get { return _ActiveLocks; } }
    private List<DBxShortDocsLock> _ActiveLocks;

    #endregion

    #region Длительная

    /// <summary>
    /// Список активных длительных блокировок базы данных, используемых в процессе 
    /// редактирования документов пользователем.
    /// Класс является потокобезопасным.
    /// </summary>
    public class LongTermLockList : SyncCollection<DBxLongDocsLock>
    {
      #region Конструктор

      internal LongTermLockList(DBxDocTextHandlers textHandlers)
      {
        _Source = (List<DBxLongDocsLock>)(base.Source);
        _TextHandlers = textHandlers;
      }

      /// <summary>
      /// Чтобы быстрее обращаться без преобразования.
      /// На момент вызова обязательно должно быть lock(SyncRoot).
      /// </summary>
      private /*new*/ List<DBxLongDocsLock> _Source;

      private DBxDocTextHandlers _TextHandlers;

      #endregion

      #region Добавление и удаление блокировки

      /// <summary>
      /// Устанавливает блокировку.
      /// Если блокировка конфликтует с уже установленными, выбрасывается DBxDocsLockException
      /// </summary>
      /// <param name="item">Блокировка</param>
      public new void Add(DBxLongDocsLock item)
      {
        if (item == null)
          throw new ArgumentNullException("item");
        item.SetReadOnly();
#if DEBUG
        if (!item.IsReadOnly)
          throw new BugException("!DBxLongDocsLock.IsReadOnly");
#endif

        lock (base.SyncRoot)
        {
          foreach (DBxLongDocsLock oldItem in base.Source)
          {
            if (item.Data.TestConflict(oldItem.Data))
              throw new DBxDocsLockException(item, oldItem, _TextHandlers);
          }

          base.Add(item);
        }
      }

      /// <summary>
      /// Добавляет блокировки из списка
      /// </summary>
      /// <param name="collection">Блокировки</param>
      public new void AddRange(IEnumerable<DBxLongDocsLock> collection)
      {
#if DEBUG
        if (collection == null)
          throw new ArgumentException("collection");
#endif
        if (Object.ReferenceEquals(collection, this))
          throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

        foreach (DBxLongDocsLock item in collection)
          Add(item);
      }

      /// <summary>
      /// Удаляет блокировку с заданным идентификатором
      /// </summary>
      /// <param name="lockGuid">Идентификатор длительной блокировки</param>
      /// <returns>true, если блокировка найдена и снята</returns>
      public bool Remove(Guid lockGuid)
      {
        if (lockGuid == Guid.Empty)
          return false;

        lock (SyncRoot)
        {
          ResetCopyArray();
          for (int i = 0; i < _Source.Count; i++)
          {
            if (_Source[i].Guid == lockGuid)
            {
              _Source.RemoveAt(i);
              return true;
            }
          }
        }

        return false;
      }

      #endregion
    }

    /// <summary>
    /// Список установленных длительных блокировок
    /// </summary>
    public LongTermLockList LongLocks { get { return _LongLocks; } }
    private LongTermLockList _LongLocks;

    #endregion

    #endregion

    #region Кэширование данных

    /// <summary>
    /// Объект кэширования данных "без ограничений".
    /// </summary>
    public DBxCache DBCache { get { return _DBCache; } }
    private DBxCache _DBCache;

    #endregion

    #region Список данных для очистки буферизации клиентов

    /// <summary>
    /// Буфер для отслеживания изменений в таблицах.
    /// Приложение сервера должно вызывать по таймеру метод ClearCacheBuffer.Swap()
    /// </summary>
    public DBxClearCacheBuffer ClearCacheBuffer { get { return _ClearCacheBuffer; } }
    private DBxClearCacheBuffer _ClearCacheBuffer;

    /// <summary>
    /// Очищает кэш DBCache.
    /// В буфер ClearCacheBuffer добавляет сигнал на сброс буферизации всех таблиц
    /// </summary>
    public void ClearCache()
    {
      DBCache.Clear();
      ClearCacheBuffer.Holder.AddAllTables();
    }

    /// <summary>
    /// Очищает кэш для заданной таблицы и посылает сигнал в ClearCacheBuffer
    /// </summary>
    /// <param name="tableName">Имя таблицы базы данных</param>
    public void ClearCache(string tableName)
    {
      DBCache.Clear(tableName);
      ClearCacheBuffer.Holder.Add(tableName);
    }

    /// <summary>
    /// Очищает кэш для заданного списка идентификаторов и посылает сигнал в ClearCacheBuffer
    /// </summary>
    /// <param name="tableName">Имя таблицы базы данных</param>
    /// <param name="ids">Список идентификаторов</param>
    public void ClearCache(string tableName, IdList ids)
    {
      DBCache.Clear(tableName, ids);
      ClearCacheBuffer.Holder.Add(tableName);
    }

    /// <summary>
    /// Очищает кэш для заданных таблиц и идентификаторов и посылает сигнал в ClearCacheBuffer.
    /// </summary>
    /// <param name="ds">Набор данных таблиц, в которых учитываются строки в состоянии
    /// DataRowState.Added, Modified и Deleted</param>
    public void ClearCache(DataSet ds)
    {
      foreach (DataTable table in ds.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        IdList ids = new IdList();
        foreach (DataRow row in table.Rows)
        {
          switch (row.RowState)
          {
            case DataRowState.Added:
              Int32 newId = (Int32)(row["Id"]);
              if (newId > 0)
                ids.Add(newId);
              break;
            case DataRowState.Modified:
              ids.Add((Int32)(row["Id"]));
              break;
            case DataRowState.Deleted:
              Int32 oldId = (Int32)(row["Id", DataRowVersion.Original]);
              if (oldId > 0)
                ids.Add(oldId);
              break;
          }
        }

        DBCache.Clear(table.TableName, ids);
        ClearCacheBuffer.Holder.Add(table.TableName, ids.ToArray());
      }
    }

    #endregion

    #region IDBxCacheSource Members

    /// <summary>
    /// Реализация DBxStructSource, возвращающая только те таблицы и поля, к которым у пользователя
    /// есть доступ на чтение или на запись
    /// </summary>
    private class InternalStructSource : MarshalByRefObject, IDBxStructSource
    {
      #region Конструктор

      public InternalStructSource(DBxRealDocProviderGlobal owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Поля

      private DBxRealDocProviderGlobal _Owner;

      #endregion

      #region Переопределенные методы

      public string[] GetAllTableNames()
      {
        return _Owner.MainDBEntry.DB.Struct.AllTableNames;
      }

      public DBxTableStruct GetTableStruct(string tableName)
      {
        return _Owner.MainDBEntry.DB.Struct.Tables[tableName];
      }

      #endregion
    }

    private InternalStructSource _StructSource;

    IDBxStructSource IDBxCacheSource.StructSource { get { return _StructSource; } }


    /// <summary>
    /// Коллекция описаний полей таблиц для буферизации.
    /// Ключ - имя таблицы.
    /// На момент обращения коллекция блокируется для обеспечения потокобезопасности
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo info;
      lock (_TableCacheInfos)
      {
        if (!_TableCacheInfos.TryGetValue(tableName, out info))
        {
          DBxDocTypeBase docTypeBase;
          if (!DocTypes.FindByTableName(tableName, out docTypeBase))
            throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\"", "tableName");

          if (docTypeBase.IndividualCacheColumns.AreAllDefaults)
            info = null;
          else
          {
            info = new DBxTableCacheInfo(_StructSource.GetTableStruct(tableName));
            for (int i = 0; i < info.TableStruct.Columns.Count; i++)
            {
              string columnName = info.TableStruct.Columns[i].ColumnName;
              if (docTypeBase.Struct.Columns.Contains(columnName))
                info.IndividualColumnFlags[i] = docTypeBase.IndividualCacheColumns[columnName];
              // Для полей типа "Id" нет соответствующего описания в структуре полей DBxDocTypeBase.Struct
            }
            info.SetReadOnly();
          }
          _TableCacheInfos.Add(tableName, info);
        }
      }
      return info;
    }

    DBxCacheLoadResponse IDBxCacheSource.LoadCachePages(DBxCacheLoadRequest request)
    {
      using (DBxCon mainCon = new DBxCon(MainDBEntry))
      {
        return mainCon.LoadCachePages(request);
      }
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      // Ничего не делаем
    }

    #endregion

    #region Текстовое представление документов

    /// <summary>
    /// Генераторы текстового представления документа.
    /// Список должен быть заполнен (вызовами Add()) до создания первого объекта DBxRealDocProviderSource
    /// </summary>
    public DBxDocTextHandlers TextHandlers { get { return _TextHandlers; } }
    private DBxDocTextHandlers _TextHandlers;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект был переведен в режим "только чтения".
    /// Режим означает, что объект уже используется DBxRealDocProviderSource, а
    /// не режим просмотра данных.
    /// </summary>
    public bool IsReadOnly { get { return _TextHandlers.IsReadOnly; } }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит объект в режим "Только чтение".
    /// Метод не должен использоваться в пользовательском коде.
    /// </summary>
    public void SetReadOnly()
    {
      _TextHandlers.SetReadOnly();
    }

    #endregion

    #region Методы работы со списком баз данных

    /// <summary>
    /// Возвращает массив используемых баз данных (без повторов)
    /// </summary>
    /// <returns></returns>
    public DBx[] GetDBs()
    {
      SingleScopeList<DBx> dbs = new SingleScopeList<DBx>();

      dbs.Add(MainDBEntry.DB);
      if (UndoDBEntry != null)
        dbs.Add(UndoDBEntry.DB);
      if (BinDataHandler != null)
      {
        dbs.Add(BinDataHandler.MainEntry.DB);
        DBxEntry[] entrs = BinDataHandler.GetSectionEntries();
        for (int i = 0; i < entrs.Length; i++)
          dbs.Add(entrs[i].DB);
      }

      return dbs.ToArray();
    }


    /// <summary>
    /// Вызывает для всех объектов DBx метод Dispose().
    /// Сам DBxRealDocProviderGlobal не является Disposable-объектом
    /// </summary>
    public void DisposeDBs()
    {
      DBx[] dbs = GetDBs();
      for (int i = 0; i < dbs.Length; i++)
      {
        if (!dbs[i].IsDisposed)
          dbs[i].Dispose();
      }
    }

    #endregion
  }

  internal static class DBxDocDebugTools
  {
    #region Статический метод инициализации

    public static void InitLogout()
    {
      DBxTools.InitLogout();

      lock (typeof(DBxDocDebugTools))
      {
        if (_FirstCall)
        {
          _FirstCall = false;
          LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
          LogoutTools.LogoutProp += new LogoutPropEventHandler(LogoutTools_LogoutProp);
        }
      }
    }

    private static bool _FirstCall = true;

    #endregion

    #region Дополнительная отладочная информация

    static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
#if DEBUG
      if (DBxDocProvider.DebugDocProviderList != null)
      {
        GC.Collect(); // надо ли?

        DBxDocProvider[] a = DBxDocProvider.DebugDocProviderList.ToArray();
        args.WriteHeader("DBxDocProvider list (" + a.Length.ToString() + ")");
        LogoutTools.LogoutObject(args, a);
      }
#endif
    }

    #endregion

    #region Блокировка вывода свойств

    static void LogoutTools_LogoutProp(object sender, LogoutPropEventArgs args)
    {
      if (args.Object is DBxDocProvider)
      {
        switch (args.PropertyName)
        {
          case "DocTypes":
          case "InternalFixedInfo": // 28.04.2018
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is DBxDocTextHandlers)
      {
        switch (args.PropertyName)
        {
          case "DocTypes":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is UserPermissions)
      {
        switch (args.PropertyName)
        {
          case "AsXmlText":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is DBxRealDocProviderSource)
      {
        switch (args.PropertyName)
        {
          case "GlobalData":
            args.Mode = LogoutPropMode.None; // 13.01.2017
            break;
        }
      }

      if (args.Object is DBxDocSet) // 21.03.2017
      {
        switch (args.PropertyName)
        {
          case "DocProvider":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is DBxSingleDoc ||
        args.Object is DBxMultiDocs ||
        args.Object is DBxSubDoc ||
        args.Object is DBxSingleSubDocs ||
        args.Object is DBxMultiSubDocs)
      {
        switch (args.PropertyName)
        {
          case "DocProvider":
          case "DocSet":
          case "Values":
          case "OriginalValues":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
    }

    #endregion
  }
}
