// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Remoting;
using System.Data;
using System.Collections;
using FreeLibSet.Logging;
using System.Runtime.Serialization;
using FreeLibSet.Core;
using FreeLibSet.Collections;

// Кратковременные и долговременные блокировки документов

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Хранилище блокировок по разнотипным документам
  /// Реализует свойство Data в классах блокировок DBxShortDocsLock и DBxLongDocsLock 
  /// Является коллекцией блокировок по таблицам документов
  /// </summary>
  [Serializable]
  public sealed class DBxDocSetLockData : IReadOnlyObject, IEnumerable<DBxDocTableLockData>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустое хранилище
    /// </summary>
    public DBxDocSetLockData()
    {
      _Tables = new Dictionary<string, DBxDocTableLockData>();
      _IsReadOnly = false;
    }

    #endregion

    #region Список таблиц

    /// <summary>
    /// Доступ к блокировкам для одного вида документов.
    /// Если объект переведен в режим IsReadOnly, то свойство возвращает null, если нет блокировок для этой таблицы.
    /// Пока SetReadOnly() не вызван, создается новый DBxDocTableLockData, если его нет.
    /// Для перебора объектов DBxDocTableLockData в целях просмотра, рекомендуется использовать перечислитель
    /// </summary>
    /// <param name="tableName">Имя таблицы документа</param>
    /// <returns>Объект DBxDocTableLockData</returns>
    public DBxDocTableLockData this[string tableName]
    {
      get
      {
        DBxDocTableLockData res;
        if (!_Tables.TryGetValue(tableName, out res))
        {
          if (IsReadOnly)
            return null;

          res = new DBxDocTableLockData(this, tableName);
          _Tables.Add(tableName, res);
        }
        return res;
      }
    }

    private readonly Dictionary<string, DBxDocTableLockData> _Tables;

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Возвращает true, если хранилище блокировок переведено в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит хранилище в режим "только чтение".
    /// Вызывается перед установкой блокировки.
    /// Повторные вызовы игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      if (_IsReadOnly) // исправлено 15.09.2020
        return;

      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Дополнительные методы инициализации

    /// <summary>
    /// Инициализация списка блокировок для документов.
    /// Блокировки устанавливаются для всех документов в наборе <paramref name="ds"/>, 
    /// в состоянии изменения/создания/удаления. Документы в состоянии просмотра пропускаются 
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="ds">Набор таблиц документов, для которых надо установить блокировки</param>
    public void Init(DBxRealDocProvider docProvider, DataSet ds)
    {
      CheckNotReadOnly();
      for (int i = 0; i < ds.Tables.Count; i++)
      {
        string tableName = ds.Tables[i].TableName;
        if (docProvider.Source.GlobalData.DocTypes.Contains(tableName)) // блокируем только документы, а не поддокументы
          this[tableName].Init(ds.Tables[i]);
      }
    }

    /// <summary>
    /// Инициализация списка блокировок для выборки документов
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void Init(DBxDocSelection docSel)
    {
      CheckNotReadOnly();
      for (int i = 0; i < docSel.TableNames.Length; i++)
      {
        string tableName = docSel.TableNames[i];
        IIdSet<Int32> ids = docSel[tableName];
        foreach (Int32 id in ids)
          this[tableName].LockIds.Add(id);
      }
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Возвращает true, если текущая блокировка конфликтует с другой
    /// </summary>
    /// <param name="other">Другая блокировка</param>
    /// <returns>Наличие конфликта</returns>
    public bool TestConflict(DBxDocSetLockData other)
    {
      foreach (KeyValuePair<string, DBxDocTableLockData> pair in _Tables)
      {
        DBxDocTableLockData otherTable;
        if (other._Tables.TryGetValue(pair.Key, out otherTable))
        {
          if (pair.Value.TestConflict(otherTable))
            return true;
        }
      }
      return false;
    }

    #endregion

    #region IEnumerable

    /// <summary>
    /// Возвращает перечислитель по видам документов, для которых могли быть установлены блокировки.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, DBxDocTableLockData>.ValueCollection.Enumerator GetEnumerator()
    {
      return _Tables.Values.GetEnumerator();
    }

    IEnumerator<DBxDocTableLockData> IEnumerable<DBxDocTableLockData>.GetEnumerator()
    {
      return _Tables.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Tables.Values.GetEnumerator();
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает массив блокировок по таблицам в виде массива
    /// </summary>
    /// <returns></returns>
    public DBxDocTableLockData[] ToArray()
    {
      DBxDocTableLockData[] a = new DBxDocTableLockData[_Tables.Count];
      _Tables.Values.CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// Возвращает количество заблокированных документов всех видов.
    /// Блокировки <see cref="DBxDocTableLockData.LockAdd"/> не учитываются.
    /// </summary>
    public int DocCount
    {
      get
      {
        int cnt = 0;
        foreach (DBxDocTableLockData item in _Tables.Values)
          cnt += item.LockIds.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает информацию о единственном забловированном документе.
    /// Если есть блокировка <see cref="DBxDocTableLockData.LockAdd"/>, то возвращается false
    /// </summary>
    /// <param name="tableName">Сюда записывается имя таблицы заблокированного документа</param>
    /// <param name="docId">Сюда записывается идентификатор заблокированного документа</param>
    /// <returns>true, если заблокирован ровно один документ</returns>
    public bool GetSingleDoc(out string tableName, out Int32 docId)
    {
      tableName = null;
      docId = 0;
      if (_Tables.Count != 1)
        return false;

      DBxDocTableLockData tableData = ToArray()[0];
      if (tableData.LockAdd)
        return false;

      if (tableData.LockIds.Count != 1)
        return false;

      tableName = tableData.TableName;
      docId = tableData.LockIds.SingleId;
      return true;
    }

    /// <summary>
    /// Текстовое представление по установленным блокировкам.
    /// Рекомендуется использовать перегрузку, получающую <see cref="StringBuilder"/> и <see cref="DBxDocTextHandlers"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, null);
      return sb.ToString();
    }

    /// <summary>
    /// Текстовое представление по установленным блокировкам.
    /// Рекомендуется использовать перегрузку, получающую <see cref="StringBuilder"/> и <see cref="DBxDocTextHandlers"/>.
    /// </summary>
    /// <param name="sb">Объект StringBuilder для заполнения</param>
    public void ToString(StringBuilder sb)
    {
      ToString(sb, null);
    }

    /// <summary>
    /// Текстовое представление по установленным блокировкам.
    /// </summary>
    /// <param name="sb">Объект StringBuilder для заполнения</param>
    /// <param name="textHandlers">Используется для извлечения текстовых представлений. Может быть null.</param>
    public void ToString(StringBuilder sb, DBxDocTextHandlers textHandlers)
    {
      bool isFirstPair = true;
      foreach (KeyValuePair<string, DBxDocTableLockData> pair in _Tables)
      {
        if (!pair.Value.IsEmpty)
        {
          if (isFirstPair) // 06.07.2017. Используем флаг, вместо проверки sb.Length==0
            isFirstPair = false;
          else
            sb.Append(", ");
          pair.Value.ToString(sb, textHandlers);
        }
      }

      if (isFirstPair)
        sb.Append("{Пустая блокировка}");
    }

    internal static string ToString(DBxDocSetLockData data, DBxRealDocProvider docProvider)
    {
      if (docProvider.DocTypes.UseUsers)
      {
        string s2;
        if (docProvider.SessionInfo == null)
        {
          if (String.IsNullOrEmpty(docProvider.UserName))
            s2 = "UserId=" + docProvider.UserId.ToString();
          else
            s2 = "Пользователь \"" + docProvider.UserName + "\""; // 03.05.2018
        }
        else
          s2 = docProvider.SessionInfo.ToString();
        return data.ToString() + ", установлена " + s2;
      }
      else
        return data.ToString(); // 03.05.2018
    }

    #endregion
  }

  /// <summary>
  /// Хранилище блокировок по документам одного вида
  /// </summary>
  [Serializable]
  public sealed class DBxDocTableLockData
  {
    #region Защищенный конструктор

    internal DBxDocTableLockData(DBxDocSetLockData owner, string tableName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
#endif
      _Owner = owner;
      _TableName = tableName;

      _LockIds = new IdCollection<Int32>();
      if (owner.IsReadOnly)
        _LockIds.SetReadOnly();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public DBxDocSetLockData Owner { get { return _Owner; } }
    private readonly DBxDocSetLockData _Owner;

    /// <summary>
    /// Имя таблицы документа, для которого создан текущйи объект
    /// </summary>
    public string TableName { get { return _TableName; } }
    private readonly string _TableName;

    /// <summary>
    /// Предполагается добавление строк в таблицу
    /// </summary>
    public bool LockAdd
    {
      get { return _LockAdd; }
      set
      {
        Owner.CheckNotReadOnly();
        _LockAdd = value;
      }
    }
    private bool _LockAdd;

    /// <summary>
    /// Идентификаторы блокируемых документов
    /// </summary>
    public IdCollection<Int32> LockIds { get { return _LockIds; } }
    private readonly IdCollection<Int32> _LockIds;

    /// <summary>
    /// Текстовое представление.
    /// Рекомендуется использовать перегрузку, получающую <see cref="StringBuilder"/> и <see cref="DBxDocTextHandlers"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, null);
      return sb.ToString();
    }

    /// <summary>
    /// Получить текстовое представление для блокировок по таблицы
    /// Рекомендуется использовать перегрузку, получающую <see cref="StringBuilder"/> и <see cref="DBxDocTextHandlers"/>.
    /// </summary>
    /// <param name="sb">Объект для заполнения</param>
    public void ToString(StringBuilder sb)
    {
      ToString(sb, null);
    }

    /// <summary>
    /// Получить текстовое представление для блокировок по таблицам
    /// </summary>
    /// <param name="sb">Объект для заполнения</param>
    /// <param name="textHandlers">Используется для получения названия документа. Может быть null.</param>
    public void ToString(StringBuilder sb, DBxDocTextHandlers textHandlers)
    {
      if (textHandlers == null)
        sb.Append(TableName);
      else
      {
        DBxDocType dt = textHandlers.DocTypes[TableName];
        if (dt == null)
          sb.Append(TableName);
        else if ((!LockAdd) && LockIds.Count == 1)
          sb.Append(dt.SingularTitle);
        else
          sb.Append(dt.PluralTitle);
      }
      if (IsEmpty)
      {
        sb.Append(" ");
        sb.Append(Res.DBxDocsLock_Msg_IsEmpty);
      }
      else
      {
        if (LockAdd)
        {
          sb.Append(" ");
          sb.Append(Res.DBxDocsLock_Msg_LockAdd);
        }

        sb.Append(" ");
        if (LockIds.Count > 0)
        {
          if (LockIds.Count == 1)
          {
            // 06.07.2017
            // Если блокировка для единственного документа, то выводим название документа

            if (textHandlers != null)
            {
              try
              {
                string s = textHandlers.GetTextValue(TableName, LockIds.SingleId);
                sb.Append("(");
                sb.Append(s);
                sb.Append(")");
              }
              catch { }
            }
            else
            {
              sb.Append("(Id=");
              sb.Append(LockIds.SingleId);
              sb.Append(")");
            }
          }
          else
          {
            sb.Append("(");
            sb.Append(LockIds.Count.ToString());
            sb.Append(" шт.)");
          }
        }
      }
    }

    /// <summary>
    /// Возвращает true, если для текущей таблицы нет блокировок
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        if (LockAdd)
          return false;

        if (LockIds.Count > 0)
          return false;
        return true;
      }
    }

    #endregion

    #region Инициализация

    internal void Init(DataTable table)
    {
      foreach (DataRow row in table.Rows)
      {
        switch (row.RowState)
        {
          case DataRowState.Added:
            LockAdd = true;
            break;
          case DataRowState.Modified:
            LockIds.Add((Int32)(row["Id"]));
            break;
          case DataRowState.Deleted:
            LockIds.Add((Int32)(row["Id", DataRowVersion.Original]));
            break;
        }
      }
    }

    #endregion

    #region Проверка конфликта

    internal bool TestConflict(DBxDocTableLockData other)
    {
#if USE_LOCK_ADD
        if (LockAdd && Other.LockAdd)
          return true;
#endif
      if (LockIds.ContainsAny(other.LockIds))
        return true;
      return false;
    }

    #endregion
  }

  /// <summary>
  /// Общая часть для DBxShortDocsLock и DBxLongDocsLock
  /// </summary>
  public interface IDBxDocsLock : IReadOnlyObject
  {
    #region Свойства

    /// <summary>
    /// Данные по блокируемым документам
    /// </summary>
    DBxDocSetLockData Data { get;}

    /// <summary>
    /// Объект - владелец блокировки
    /// </summary>
    DBxRealDocProvider DocProvider { get;}

    #endregion
  }

  /// <summary>
  /// Кратковременная блокировка документов.
  /// Используется при внесении изменений в базу данных в <see cref="DBxRealDocProvider"/>
  /// </summary>
  public class DBxShortDocsLock : ExecProcMultiLock, IDBxDocsLock, IReadOnlyObject
  {
    /*
     * Порядок работы метода ApplyChanges()
     * 
     * 1. Создать новый объект DBxDocsLock
     * 2. Заполнить требуемые блокировки таблиц, передав DataSet
     * 3. Выполить блокировку в DBxDocsLock.Lock()
     * 4. Выполнить запись в БД
     * 5. Вызвать DBxDocsLock.Unlock()
     * 
     * Для действий 3 и 5 используется вспомогательный объект ExecProcLocker
     */


    #region Конструктор

    /// <summary>
    /// Создает кратковременную блокировку
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="ignoreAllLocks">Значение свойства <see cref="DBxDocSet.IgnoreAllLocks"/></param>
    /// <param name="ignoredLocks">Значение свойства <see cref="DBxDocSet.IgnoredLocks"/></param>
    public DBxShortDocsLock(DBxRealDocProvider docProvider, bool ignoreAllLocks, IList<Guid> ignoredLocks)
      : base(Res.DBxShortDocsLock_Name_Default)
    {
#if DEBUG
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
#endif
      _DocProvider = docProvider;
      _IgnoreAllLocks = ignoreAllLocks;
      _IgnoredLocks = ignoredLocks;
      _Data = new DBxDocSetLockData();
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Владелец блокировки
    /// </summary>
    public DBxRealDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxRealDocProvider _DocProvider;

    /// <summary>
    /// Значение свойства DBxDocSet.IgnoreAllLocks.
    /// Если true, то вызов метода Lock() всегда выполняется без ошибок, даже если есть конфликтующие 
    /// блокировки. Установка свойства не влияет на другие объекты, которые могут конфликтовать с текущей
    /// кратковременной блокировкой
    /// </summary>
    public bool IgnoreAllLocks { get { return _IgnoreAllLocks; } }
    private readonly bool _IgnoreAllLocks;

    /// <summary>
    /// Значение свойства DBxDocSet.IgnoredLocks.
    /// Блокировки из списка не учитываются при проверке конфликта блокировок
    /// </summary>
    public IList<Guid> IgnoredLocks { get { return _IgnoredLocks; } }
    private readonly IList<Guid> _IgnoredLocks;

    /// <summary>
    /// Данные блокировки по документам
    /// </summary>
    public DBxDocSetLockData Data { get { return _Data; } }
    private readonly DBxDocSetLockData _Data;

    /// <summary>
    /// Возвращает читаемую информацию о блокировке, включая имя пользователя, установившего эту блокировку
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return DBxDocSetLockData.ToString(Data, DocProvider);
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Возвращает Data.IsReadOnly
    /// </summary>
    public bool IsReadOnly { get { return Data.IsReadOnly; } }

    /// <summary>
    /// Вызывает Data.SetReadOnly()
    /// </summary>
    public void SetReadOnly()
    {
      Data.SetReadOnly();
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      Data.CheckNotReadOnly();
    }

    #endregion

    #region Блокировка

    /// <summary>
    /// Вызывается методом GetChildren()
    /// </summary>
    /// <returns></returns>
    protected override ExecProcLock[] CreateChildren()
    {
      SetReadOnly();

#if USE_LOCK_ADD
      // Собираем список блокировок добавлений записи
      List<ServerExecLock> lst = new List<ServerExecLock>();
#endif

      foreach (DBxDocTableLockData table in Data)
      {
        table.LockIds.SetReadOnly();

#if USE_LOCK_ADD
        if (Table.LockAdd)
          lst.Add(Owner.GetAddRecordLocker(Table.TableName));
#endif
      }

#if USE_LOCK_ADD
      return lst.ToArray();
#else
      return ExecProcLock.EmptyArray;
#endif
    }

    /// <summary>
    /// Выполняет попытку установить блокировку.
    /// Возвращает true, если блокировка была успешно установлена. 
    /// В этом случае должен быть вызван метод Unlock().
    /// В случае неудачи возвращается false, в этом случае вызывать Unlock() не нужно.
    /// </summary>
    /// <param name="caller">Вызывающая процедура</param>
    /// <param name="lockedObj">Объект, вызвавший конфликт, если не удалочь установить блокировку</param>
    /// <returns>Признак установки блокировки</returns>
    protected override bool DoTryLock(ExecProc caller, out ExecProcLock lockedObj)
    {
      if (DocProvider.Source.MainDBEntry == null)
        throw ExceptionFactory.ObjectPropertyNotSet(DocProvider.Source, "MainDBEntry");

      lockedObj = null;

      if (!IgnoreAllLocks)
      {
        // Проверяем длительные блокировки
        // Если есть конфликт, выбрасываем исключение, а не переходим в режим ожидания
        foreach (DBxLongDocsLock longLock in DocProvider.Source.GlobalData.LongLocks)
        {
          // 04.07.2016
          // Используем явно заданный список блокировок
          // if (LongLock.DocProvider == DocProvider) // иначе сами себя заблокируем
          //   continue;
          if (IgnoredLocks.Contains(longLock.Guid))
            continue;

          if (Data.TestConflict(longLock.Data))
            throw new DBxDocsLockException(this, longLock, _DocProvider.Source.GlobalData.TextHandlers);
        }
      }

      lock (DocProvider.Source.GlobalData.ActiveLocks)
      {
        // 11.12.2014
        // В OleDB разрешается только одна запись
        if (DocProvider.Source.GlobalData.MainDBEntry.DB.LockMode == DBxLockMode.SingleWrite && DocProvider.Source.GlobalData.ActiveLocks.Count > 0)
        {
          lockedObj = DocProvider.Source.GlobalData.ActiveLocks[0];
          return false;
        }

        // Проверяем кратковременные блокировки
        if (!IgnoreAllLocks)
        {
          for (int i = 0; i < DocProvider.Source.GlobalData.ActiveLocks.Count; i++)
          {
            if (Data.TestConflict(DocProvider.Source.GlobalData.ActiveLocks[i].Data))
            {
              lockedObj = DocProvider.Source.GlobalData.ActiveLocks[i];
              break;
            }
          }
        }

        if (lockedObj != null)
          return false;

        if (!base.DoTryLock(caller, out lockedObj))
          return false;

        DocProvider.Source.GlobalData.ActiveLocks.Add(this);
        AllLocks.Add(this);
      }

      return true;
    }

    /// <summary>
    /// Снятие блокироки
    /// </summary>                    
    protected override void DoUnlock()
    {
      lock (DocProvider.Source.GlobalData.ActiveLocks)
      {
        DocProvider.Source.GlobalData.ActiveLocks.Remove(this);
        AllLocks.Remove(this);

        base.DoUnlock();
      }
    }

    #endregion

    #region Отладочная информация

    /// <summary>
    /// Добавляет отладочную информацию
    /// </summary>
    /// <param name="args">Объект для записи log-данных</param>
    public override void GetDebugInfo(LogoutInfoNeededEventArgs args)
    {
      //Args.WritePair("Процедура", Owner.Caller.ToString());
      args.WritePair("Database", DocProvider.Source.GlobalData.ToString());
      args.WriteLine("Locked tables:");
      args.IndentLevel++;
      int cnt = 0;
      foreach (DBxDocTableLockData table in Data)
      {
        if (table.IsEmpty)
          continue;
        cnt++;
        args.WriteLine(table.ToString());
      }
      if (cnt == 0)
        args.WriteLine("No locked tables (empty object)");
      args.IndentLevel--;

      base.GetDebugInfo(args);
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Список всех выполняемых блокировок
    /// </summary>
    public static readonly SyncCollection<DBxShortDocsLock> AllLocks = new SyncCollection<DBxShortDocsLock>(new List<DBxShortDocsLock>());

    #endregion
  }

  /// <summary>
  /// Длительная блокировка документов
  /// </summary>
  public class DBxLongDocsLock : IDBxDocsLock, IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает блокировку
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    public DBxLongDocsLock(DBxRealDocProvider docProvider)
    {
#if DEBUG
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
#endif
      _DocProvider = docProvider;

      _Data = new DBxDocSetLockData();
      _Guid = Guid.NewGuid();
      _StartTime = DateTime.Now;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Владелец блокировки
    /// </summary>
    public DBxRealDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxRealDocProvider _DocProvider;

    /// <summary>
    /// Данные блокировки по документам
    /// </summary>
    public DBxDocSetLockData Data { get { return _Data; } }
    private readonly DBxDocSetLockData _Data;

    /// <summary>
    /// Идентификатор длительной блокировки
    /// Для кратковременной блокировки возвращает Guid.Empty
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private readonly Guid _Guid;

    /// <summary>
    /// Время установки блокировки
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private readonly DateTime _StartTime;

    /// <summary>
    /// Читаемое описание блокировки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return DBxDocSetLockData.ToString(Data, DocProvider);
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Возвращает Data.IsReadOnly
    /// </summary>
    public bool IsReadOnly { get { return Data.IsReadOnly; } }

    /// <summary>
    /// Вызывает Data.SetReadOnly()
    /// </summary>
    public void SetReadOnly()
    {
      Data.SetReadOnly();
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      Data.CheckNotReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое при конфликте кратковременной или длительной блокировки с другой
  /// длительной блокировкой
  /// </summary>
  [Serializable]
  public class DBxDocsLockException : ApplicationException
  {
    #region Конструкторы

    internal DBxDocsLockException(IDBxDocsLock pretender, IDBxDocsLock oldLock, DBxDocTextHandlers textHandlers)
      : base(GetMessage(pretender, oldLock))
    {
      _Pretender = new LockInfo(pretender.DocProvider.UserId, pretender.Data, pretender.DocProvider.UserName, textHandlers);
      _OldLock = new LockInfo(oldLock.DocProvider.UserId, oldLock.Data, oldLock.DocProvider.UserName, textHandlers);
      _IsSameDocProvider = oldLock.DocProvider == pretender.DocProvider;
    }

    private static string GetMessage(IDBxDocsLock pretender, IDBxDocsLock oldLock)
    {
      if (pretender.DocProvider == oldLock.DocProvider)
        return String.Format(Res.DBxDocsLockException_Err_SameUser, pretender, oldLock.Data);
      else
        return String.Format(Res.DBxDocsLockException_Err_Default, pretender, oldLock.Data);
    }

    /// <summary>
    /// Используется для десериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxDocsLockException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _Pretender = (LockInfo)(info.GetValue("Pretender", typeof(LockInfo)));
      _OldLock = (LockInfo)(info.GetValue("OldLock", typeof(LockInfo)));
      _IsSameDocProvider = info.GetBoolean("IsSameDocProvider");
    }

    /// <summary>
    /// Используется для сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Pretender", _Pretender);
      info.AddValue("OldLock", _OldLock);
      info.AddValue("IsSameDocProvider", _IsSameDocProvider);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Однотипная информация о блокировке
    /// </summary>
    [Serializable]
    public class LockInfo
    {
      #region Конструктор

      internal LockInfo(Int32 userId, DBxDocSetLockData data, string userName, DBxDocTextHandlers textHandlers)
      {
        _UserId = userId;
        _Data = data;
        _UserName = userName;
        try
        {
          StringBuilder sb = new StringBuilder();
          data.ToString(sb, textHandlers);
          _DocText = sb.ToString();
        }
        catch (Exception e)
        {
          _DocText = String.Format(Res.DBxDocsLockException_Err_DocText, e.Message);
        }
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Идентификатор пользователя
      /// </summary>
      public Int32 UserId { get { return _UserId; } }
      private readonly Int32 _UserId;

      /// <summary>
      /// Имя пользователя
      /// </summary>
      public string UserName { get { return _UserName; } }
      private readonly string _UserName;

      /// <summary>
      /// Данные по блокируемым документам
      /// </summary>
      public DBxDocSetLockData Data { get { return _Data; } }
      private readonly DBxDocSetLockData _Data;

      /// <summary>
      /// Текстовое представление блокируемых документов
      /// </summary>
      public string DocText { get { return _DocText; } }
      private readonly string _DocText;

      #endregion
    }

    /// <summary>
    /// Блокировка, которую не удалось установить
    /// </summary>
    public LockInfo Pretender { get { return _Pretender; } }
    private readonly LockInfo _Pretender;

    /// <summary>
    /// Существующая блокировка
    /// </summary>
    public LockInfo OldLock { get { return _OldLock; } }
    private readonly LockInfo _OldLock;

    /// <summary>
    /// Возвращает true, если существующая блокировка установлена из того же DBxRealDocProvider,
    /// что и потерпевший неудачу
    /// </summary>
    public bool IsSameDocProvider { get { return _IsSameDocProvider; } }
    private readonly bool _IsSameDocProvider;

    #endregion
  }

  /// <summary>
  /// Вспомогательный класс для установки длительных блокировок в одном фрагменте кода,
  /// что позволяет использовать в C# оператор using.
  /// Выполняет вызов DBxDocProvider.AddLongLock и RemoveLongLock
  /// </summary>
  public /*sealed class*/ struct/* 21.09.2020*/ DBxLongDocsLockHandler : IDisposable
  {
    #region Конструкторы и Dispose

    /// <summary>
    /// Установка блокировки для выборки документов.
    /// Если блокировку установить невозможно, выбрасывается исключение <see cref="DBxDocsLockException"/>.
    /// </summary>
    /// <param name="docProvider">Объект для доступа к документам</param>
    /// <param name="docSel">Выборка документов</param>
    public DBxLongDocsLockHandler(DBxDocProvider docProvider, DBxDocSelection docSel)
    {
      _LockGuid = docProvider.AddLongLock(docSel);
      _DocProvider = docProvider;
    }

    /// <summary>
    /// Установка блокировки для набора документов.
    /// Блокируются только документы в режимах Edit и Delete, а также Insert, если документы уже получили идентификаторы.
    /// Если блокировку установить невозможно, выбрасывается исключение <see cref="DBxDocsLockException"/>.
    /// </summary>
    /// <param name="docSet">Набор документов</param>
    public DBxLongDocsLockHandler(DBxDocSet docSet)
    {
      _LockGuid = docSet.AddLongLock();
      _DocProvider = docSet.DocProvider;
    }

    /// <summary>
    /// Снятие блокировок, установленных в конструкторе
    /// </summary>
    public void Dispose()
    {
      if (_DocProvider != null)
      {
        _DocProvider.RemoveLongLock(_LockGuid);
        _DocProvider = null;
      }
    }

    #endregion

    #region Свойства

    private /*readonly*/ DBxDocProvider _DocProvider;

    /// <summary>
    /// Идентификатор установленной блокировки
    /// </summary>
    public Guid LockGuid { get { return _LockGuid; } }
    private readonly Guid _LockGuid;

    #endregion
  }
}
