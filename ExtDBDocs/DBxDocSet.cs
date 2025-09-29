// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using FreeLibSet.UICore;

/*
 * Редактирование документов
 * -------------------------
 * 
 * 1. Создается объект DBxDocSet
 * 2. Устанавливается DBxDocSet.ActionInfo
 * 3. Вызываются методы DBxMultiDocs.Insert(), Edit(), Delete(), View()
 *    Для одного вида документов может быть выполнены разные функции. Например, один документ создается,
 *    а другой - удаляется (лишь бы действия относились к разным документам или были последовательными)
 * 4. Вызывается метод DBxDocSet.Apply(), который выполняет все действия. Если вызвать Dispose() без Apply(),
 *    то изменения не будут сохранены
 * 
 * Методы View(), Edit() и Insert() добавляют строки во внутренний набор данных. Для документов в этоми списке 
 * можно получать/устанавливать групповые значения или получать доступ к отдельным документам (объекты DBxSingleDoc) 
 * 
 * Для всего списка в-целом можно вызывать методы Edit() и Delete() без аргументов
 * 
 * Метод DBxSingleDoc.Delete() помечает загруженную строку документа на удаление. Метод DBxMultiDocs.Delete() с
 * заданными идентификаторами документов создают в таблице документов псевдостроки, в которых задано только поле
 * Id, а все поля данных имеют значения по умолчанию. Это сделано, чтобы для групповой операции удаления не нужно
 * было загружать данные с сервера, которые все равно не нужны. При удалении свежесозданного документа, который
 * еще не записан (с фиктивным идентификатором), никаких действий не выполняется
 * 
 * В некоторых сценариях может быть удобно загрузить на просмотр множество документов, а затем поштучно их
 * редактировать или удалять. Объект DBxSingleDoc() содержит методы Edit(), Delete() и InsertCopy() 
 * для изменения статуса отдельного документа
 * 
 * Для добавленных документов (со статусом Insert) используется временный идентификатор документа, чтобы его
 * можно было использовать в ссылочных полях
 * 
 * После вызова Apply() список загруженных документов со статусом View, Edit и Insert сохраняется, все документы
 * остаются в режиме просмотра. Добавленные документы получают реальный идентификатор Id.
 * При необходимости повторного использования может быть вызван метод DBxDocSet.Clear(), который, однако 
 * сохраняет UserActionId. Таким образом, повторный Apply() будет считаться тем же действием пользователя,
 * а не новым.
 * 
 * Блокировка документов
 * ---------------------
 * В некоторых сценариях может быть целесообразно блокировать документы для повторной записи, например,
 * чтобы два пользователя не могли одновременно реактировать документ (второй пользователь может смотреть
 * документ). 
 * Список текущих блокировок хранится на сервере в объекте DBxRealDocProviderList.
 * Свойство DBxDocSet.LockMode позволяет устанавливать блокировки по мере вызова методов View() (в том числе 
 * и View!), Edit() и Delete(). Если хотя бы на один из документов уже есть блокировка, вызывается исключение
 * и состояние набора не меняется. При вызове Dispose() все блокировки отменяются
 * 
 */

namespace FreeLibSet.Data.Docs
{
  #region Перечисления

  /// <summary>
  /// Операция, выполняемая над документом
  /// </summary>
  [Serializable]
  public enum DBxDocState
  {
    /// <summary>
    /// Документ не был загружен
    /// </summary>
    None,

    /// <summary>
    /// Документ загружен для просмотра
    /// </summary>
    View,

    /// <summary>
    /// Документ загружен для редактирования
    /// </summary>
    Edit,

    /// <summary>
    /// Создается новый документ
    /// </summary>
    Insert,

    /// <summary>
    /// В это режиме никогда не находятся. Он используется временно
    /// при вызове метода Delete
    /// </summary>
    Delete,

    /// <summary>
    /// Документы находятся в разных состояниях
    /// </summary>
    Mixed,
  }

  #endregion

  /// <summary>
  /// Хранилище разнотипных документов, предназначенных для просмотра и редактирования.
  /// Класс не является потокобезопасным.
  /// Может хранить произвольное количество документов разных видов, при этом возможно одновременное
  /// добавление/редактирование/удаление документов одного вида
  /// Обеспечивает транзактивную целостность.
  /// </summary>
  public class DBxDocSet : /*DisposableObject, */IEnumerable<DBxMultiDocs>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой набор данных
    /// </summary>
    /// <param name="docProvider">Провайдер для работы с документами</param>
    public DBxDocSet(DBxDocProvider docProvider)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
#if !NET
      if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(docProvider))
        throw new ArgumentException(Res.DBxDocSet_Arg_ProviderIsProxy, "docProvider");
#endif

      _DocProvider = docProvider;

      _DataSet = new DataSet();
      SerializationTools.SetUnspecifiedDateTimeMode(_DataSet);
      _DataSet.ExtendedProperties["DocSet"] = this;
      _MultiDocs = new NamedList<DBxMultiDocs>();
      _IgnoredLocks = new GuidList();
      _StartTime = DateTime.Now;
    }

    /// <summary>
    /// Конструктор используется серверной реализацией DocProvider для воссоздания набора из переданного DataSet
    /// </summary>
    /// <param name="docProvider">Объект DBxRealDocProvider</param>
    /// <param name="dataSet">Набор DataSet с данными для записи</param>
    internal DBxDocSet(DBxDocProvider docProvider, DataSet dataSet)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (dataSet == null)
        throw new ArgumentNullException("dataSet");
      _DocProvider = docProvider;
      _DataSet = dataSet;
      _DataSet.ExtendedProperties["DocSet"] = this;
      _MultiDocs = new NamedList<DBxMultiDocs>();
      foreach (DataTable table in dataSet.Tables)
      {
        DBxDocType docType = docProvider.DocTypes[table.TableName];
        if (docType != null)
        {
          DBxMultiDocs mDocs = new DBxMultiDocs(this, docType);
          _MultiDocs.Add(mDocs);
        }
      }

      _IgnoredLocks = new GuidList();
      string strIgnoredLocks = dataSet.ExtendedProperties["IgnoredLocks"] as string;
      Guid[] aIgnoredLocks = StdConvert.ToGuidArray(strIgnoredLocks);
      if (aIgnoredLocks.Length > 0)
        _IgnoredLocks.AddRange(aIgnoredLocks);
      _IgnoredLocks.SetReadOnly();
      _StartTime = DateTime.Now;
    }

    /*
        protected override void Dispose(bool Disposing)
        {
          if (FDataSet != null)
          {
            FDataSet = null;
            FDocProvider = null;
          }
          base.Dispose(Disposing);
        }
      */
#endregion

    #region Доступ к типам документов

    /// <summary>
    /// Возвращает объект для доступа к документов одного типа.
    /// Объект автоматически создается при первом обращении к данному типу
    /// </summary>
    /// <param name="docTypeName">Имя вида документа (совпадает с именем таблицы в базе данных)</param>
    /// <returns>Коллекция докуменетов одного вида</returns>
    public DBxMultiDocs this[string docTypeName]
    {
      get
      {
        DBxMultiDocs res;
        if (!_MultiDocs.TryGetValue(docTypeName, out res))
        {
          DBxDocType dt = DocProvider.DocTypes[docTypeName];
          if (dt == null)
          {
            if (String.IsNullOrEmpty(docTypeName))
              throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
            else
              throw ExceptionFactory.ArgUnknownValue("docTypeName", docTypeName);
          }
          DBxMultiDocs res2 = new DBxMultiDocs(this, dt);
          if (!_MultiDocs.TryGetValue(docTypeName, out res)) // 22.01.2019 поддержка реетрабельности
          {
            res = res2;
            _MultiDocs.Add(res);
          }
        }
        return res;
      }
    }

    /// <summary>
    /// Доступ к коллекции документов одного вида по индекск
    /// </summary>
    /// <param name="docTypeIndex">Индекс вида документов от 0 до (<see cref="Count"/>-1)</param>
    /// <returns></returns>
    public DBxMultiDocs this[int docTypeIndex]
    {
      get { return _MultiDocs[docTypeIndex]; }
    }

    /// <summary>
    /// Возвращает количество видов документов в наборе.
    /// Для получения количества документов используйте свойство <see cref="DocCount"/>.
    /// </summary>
    public int Count { get { return _MultiDocs.Count; } }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Провайдер для работы с документами.
    /// Задается в конструкторе.
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxDocProvider _DocProvider;

    /// <summary>
    /// Хранилище для всех документов.
    /// Прикладной код не должен использовать это свойство.
    /// </summary>
    public DataSet DataSet { get { return _DataSet; } }
    private /*readonly*/ DataSet _DataSet;


    /// <summary>
    /// Идентификатор действия пользователя для undo. Присваивается
    /// после первого вызова ApplyChanges()
    /// </summary>
    public Int32 UserActionId
    {
      get { return DataTools.GetInt32(_DataSet.ExtendedProperties["UserActionId"]); }
      internal set { _DataSet.ExtendedProperties["UserActionId"] = value.ToString(); }
    }


    /// <summary>
    /// Описание действия. 
    /// Используется при записи изменений.
    /// Если не задано, то будет придумано автоматически.
    /// Свойство может быть установлено только до первого вызова ApplyChanges()
    /// </summary>
    public string ActionInfo
    {
      get { return DataTools.GetString(_DataSet.ExtendedProperties["ActionInfo"]); }
      set
      {
        if (UserActionId != 0)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "UserActionId");
        _DataSet.ExtendedProperties["ActionInfo"] = value;
      }
    }

    /// <summary>
    /// Надо ли проверять документ после записи (по умолчанию - false)
    /// </summary>
    public bool CheckDocs
    {
      get { return DataTools.GetBoolean(_DataSet.ExtendedProperties["CheckDocs"]); }
      set
      {
        if (UserActionId != 0)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "UserActionId");
        _DataSet.ExtendedProperties["CheckDocs"] = value ? "1" : "0";
      }
    }

    /// <summary>
    /// Надо ли записывать документы в режиме Edit, если в них не было никаких изменений.
    /// По умолчанию - false. При этом документу не присваивается новый номер версии.
    /// На документы в других состояниях не влияет
    /// </summary>
    public bool WriteIfNotChanged
    {
      get { return DataTools.GetBoolean(_DataSet.ExtendedProperties["WriteIfNotChanged"]); }
      set
      {
        if (UserActionId != 0)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "UserActionId");
        _DataSet.ExtendedProperties["WriteIfNotChanged"] = value ? "1" : "0";
      }
    }

    /// <summary>
    /// Коллекция наборов документов по типам
    /// </summary>
    private NamedList<DBxMultiDocs> _MultiDocs;

    /// <summary>
    /// Состояния документов.
    /// Если документы находятся в разных состояниях, возвращается <see cref="DBxDocState.Mixed"/>.
    /// </summary>
    public DBxDocState DocState
    {
      get
      {
        bool firstFlag = true;
        DBxDocState res = DBxDocState.None;
        foreach (DBxMultiDocs mDocs in _MultiDocs)
        {
          DBxDocState res2 = mDocs.DocState;
          if (firstFlag)
          {
            res = res2;
            firstFlag = false; // 28.12.2020
          }
          else
          {
            if (res2 == DBxDocState.None)
              continue;
            if (res2 != res)
              return DBxDocState.Mixed;
          }
        }

        return res;
      }
    }

    /// <summary>
    /// Состояния документов. Документы в состоянии <see cref="DBxDocState.View"/> пропускаются.
    /// </summary>
    public DBxDocState DocStateNoView
    {
      get
      {
        bool firstFlag = true;
        DBxDocState res = DBxDocState.None;
        foreach (DBxMultiDocs mDocs in _MultiDocs)
        {
          DBxDocState res2 = mDocs.DocStateNoView;
          if (firstFlag)
          {
            res = res2;
            firstFlag = false;
          }
          else
          {
            if (res2 == DBxDocState.None)
              continue;
            if (res2 != res)
              return DBxDocState.Mixed;
          }
        }

        return res;
      }
    }

    /// <summary>
    /// Возвращает общее количество документов в наборе
    /// </summary>
    public int DocCount
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _MultiDocs.Count; i++)
          cnt += _MultiDocs[i].DocCount;
        return cnt;
      }
    }

    /// <summary>
    /// Получить количество документов с заданным состоянием
    /// </summary>
    /// <param name="state">Состояние документа (Insert, Edit, View или Delete)</param>
    /// <returns>Количество документов в заданном состоянии</returns>
    public int GetDocCount(DBxDocState state)
    {
#if DEBUG
      CheckDocState(state);
#endif

      int cnt = 0;
      for (int i = 0; i < _MultiDocs.Count; i++)
        cnt += _MultiDocs[i].GetDocCount(state);
      return cnt;
    }

#if DEBUG

    private static void CheckDocState(DBxDocState state)
    {
      switch (state)
      {
        case DBxDocState.View:
        case DBxDocState.Edit:
        case DBxDocState.Insert:
        case DBxDocState.Delete:
          break;
        default:
          throw ExceptionFactory.ArgUnknownValue("state", state);
      }
    }

#endif

    /// <summary>
    /// Возвращает true, если в наборе есть документы заданного типа (в любом состоянии).
    /// В отличие от вызова MultiDocs.Contains() проверяет наличие ненулевого количества документов.
    /// </summary>
    /// <param name="docTypeName">Имя вида документа (совпадает с именем таблицы в базе данных)</param>
    /// <returns>Признак наличия документов</returns>
    public bool ContainsDocs(string docTypeName)
    {
      if (!_MultiDocs.Contains(docTypeName))
        return false;
      return _MultiDocs[docTypeName].DocCount > 0;
    }

    /// <summary>
    /// текстовое представление набора (для отладочных целей)
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        if (mDocs.DocCount == 0)
          continue;
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(mDocs.DocType.PluralTitle);
        sb.Append(" DocCount=");
        sb.Append(mDocs.DocCount.ToString());
        sb.Append(", DocState=");
        sb.Append(mDocs.DocState.ToString());
      }

      if (!String.IsNullOrEmpty(ActionInfo))
      {
        sb.Append(", ActionInfo=");
        sb.Append(ActionInfo);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Возвращает строку с описанием типов и количества документов вида
    /// "1 документ 'XXX', 2 документа 'YYY' и 5 документов 'ZZZ'".
    /// Используется в интерфейсе пользователя
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public string DocTypeAndCountToString()
    {
      string[] a = new string[Count];
      for (int i = 0; i < Count; i++)
      {
        if (this[i].DocCount == 1)
          a[i] = String.Format(Res.DBxDocSet_Msg_SingleDocOfType, this[i].DocType.SingularTitle);
        else
          a[i] = String.Format(Res.DBxDocSet_Msg_MultiDocsOfType, this[i].DocCount, this[i].DocType.PluralTitle);
      }
      return String.Join(", ", a);
    }

    /// <summary>
    /// Возвращает true, если набор предназначен для просмотра версий документов
    /// </summary>
    public bool VersionView
    {
      get { return DataTools.GetBoolean(_DataSet.ExtendedProperties["VersionView"]); }
      internal set
      {
        _DataSet.ExtendedProperties["VersionView"] = value ? "1" : "0";
      }
    }

    /// <summary>
    /// Время создания этого экземпляра DBxDocSet по часам текущего компьютера.
    /// Используется для записи поля StartTime в истрии действий пользователя
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private DateTime _StartTime;

    /// <summary>
    /// Использование асинхронной записи при вызове ApplyChanges().
    /// Если свойство не установлено в явном виде, то определяется автоматически, исходя из количества документов в наборе.
    /// Свойство используется только на стороне клиента провайдером DocproviderUI, в остальных случаях игнорируется
    /// </summary>
    public bool UseAsyncWriting
    {
      get
      {
        if (_UseAsyncWriting.HasValue)
          return _UseAsyncWriting.Value;
        else
          return DocCount >= 10; // ни на чем не основанное значение
      }
      set
      {
        _UseAsyncWriting = value;
      }
    }
    private bool? _UseAsyncWriting;

    /// <summary>
    /// Свойство возвращает true, если в списке документов есть добавленные/удаленные, или для документа в состоянии Edit есть измененные значения
    /// Также возвращается true, если есть какие-либо изменения в поддокументах.
    /// </summary>
    public bool IsDataModified
    {
      get
      {
        foreach (DBxMultiDocs mDocs in _MultiDocs)
        {
          if (mDocs.IsDataModified)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Должно ли выполняться тестирование документов.
    /// По умолчанию - true.
    /// Внутреннее свойство, используемое только на стороне сервера.
    /// </summary>
    internal bool UseTestDocument { get { return !_DontUseTestDocument; } set { _DontUseTestDocument = !value; } }
    private bool _DontUseTestDocument;

    #endregion

    #region InsertCopy

    /*
     * При создании копии сначала существующий(е) документ(ы) открываются на просмотр, а затем - переводятся
     * в режим создания. При этом реальные идентификаторы документов заменяются на временные (отрицатльные).
     * Также требуется заменять все ссылки внутри набора данных, включая ссылки на документы и поддокументы.
     * Замена ссылок выполняется однократно. Если после вызова InsertCopy(), будет вызван метод Edit() или InsertCopy()
     * для другого документа, в котором есть ссылка на оригинальный документ (для котрого вызван InsertCopy()), 
     * то замена ссылок "задним числом" не выполняется. 
     * Замена ссылок выполняется только для тех документов и их поддокументов, для которых вызван
     * DBxDocSet.InsertCopy() или DBxMultiDocs.InsertCopy(). Если на момент вызова в наборе есть другие документы,
     * то они не меняются, даже если содержат ссылки на исходные документы
     * При вызове DBxMultiSubDocs.InsertCopy() выполняется замена ссылок только в пределах этих поддокументов
     * (если они образуют древовидную структуру)
     * 
     * Порядок работы методов.
     * Перебираются все документы, подлежащие переводу из View в Insert. Также просматриваются все их поддокументы
     * (их, возможно, требуется дозагрузить). При этом составляется временный набор таблиц соответствия, в
     * которых есть исходный Id (из базы данных) и временный Id (отрицательный). Во время этого перебора пока
     * никаких замен не выполняется.
     * На втором проходе перебираются таблицы только из временного набора. Для каждой строки документа или 
     * поддокумента:
     * - заменяется Id
     * - заменяется DocId
     * - заменяются ссылочные поля, которые есть в списке таблиц и для которых есть идентификаторы
     */

    /// <summary>
    /// Переводит все документы, находящиеся в состоянии View, в состояние Insert. В состояние Insert
    /// также переводятся все их поддокументы.
    /// Если в наборе есть документы в состояниях Insert, Edit или Delete, то они не меняются, даже если содержат
    /// ссылки на "оригинальные" документы
    /// </summary>
    public void InsertCopy()
    {
      DataSet tempDS = new DataSet();
      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        IIdSet<Int32> docIds = mDocs.GetDocIds(DBxDocState.View);
        foreach (Int32 docId in docIds)
          mDocs.DoInsertCopy1(tempDS, docId);
      }

      DBxDocSet.DoInsertCopy2(tempDS, DataSet, DocProvider);
    }

    /// <summary>
    /// Первый шаг перехода в InsertCopy - добавление ссылки в TempDS
    /// </summary>
    /// <param name="tempDS">Коллекция пар "Id-NewId" для каждой таблицы.
    /// Первоначально этот набор пустой и заполняется в процессе вызова этого метода</param>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="currId">Существующий идентификатор. Может быть реальным или фиктивным, но не 0</param>
    /// <param name="newId">Новый фиктивный идентификатор документа, на который будет выполняться замена</param>
    internal static void DoInsertCopy1(DataSet tempDS, string tableName, Int32 currId, Int32 newId)
    {
#if DEBUG
      if (currId == 0) // может быть и фиктивный идентификатор поддокумента, если копирование
        // выполняется в промежуточном наборе
        throw new BugException("CurrId=" + currId.ToString());
      if (newId >= 0)
        throw new BugException("NewId=" + newId.ToString());
#endif

      DataTable table;
      if (tempDS.Tables.Contains(tableName))
        table = tempDS.Tables[tableName];
      else
      {
        table = tempDS.Tables.Add(tableName);
        table.Columns.Add("Id", typeof(Int32));
        table.Columns.Add("NewId", typeof(Int32));
        DataTools.SetPrimaryKey(table, "Id");
      }

      table.Rows.Add(currId, newId);
    }

    /// <summary>
    /// Второй шаг, выполняющий переход в режим InsertCopy.
    /// Выполняет замену идентификаторов и переводит строку в состояние Insert
    /// Для поддокументов этот метод может вызываться из присоединенного набора. В этом случае должен
    /// быть передан собственный DataSet, а не DBxDocSet.DataSet, иначе 
    /// </summary>
    /// <param name="tempDS">Коллекция пар "Id-NewId" для каждой таблицы. Коллекция была заполнена вызовами DoInsertCopy2</param>
    /// <param name="mainDS">Набор, в котором выполняется замена</param>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    internal static void DoInsertCopy2(DataSet tempDS, DataSet mainDS, DBxDocProvider docProvider)
    {
      foreach (DataTable tempTable in tempDS.Tables)
      {
        DBxDocTypeBase dtb;
        docProvider.DocTypes.FindByTableName(tempTable.TableName, out dtb);

        DataTable mainTable = mainDS.Tables[tempTable.TableName]; // таблица документа или поддокумента

        foreach (DataRow tempRow in tempTable.Rows)
        {
          Int32 oldId = (Int32)(tempRow["Id"]);
          Int32 newId = (Int32)(tempRow["NewId"]);
#if DEBUG
          if (oldId == 0) // может быть <0
            throw new BugException("OldId=" + oldId.ToString());
          if (newId >= 0)
            throw new BugException("NewId=" + newId.ToString());
#endif

          DataRow mainRow = mainTable.Rows.Find(oldId);
          if (mainRow == null)
            throw new BugException("Row in main table \"" + tempTable.TableName + "\"not found for Id=" + oldId.ToString());

          mainRow["Id"] = newId;
          if (dtb.IsSubDoc)
          {
            string docTypeName = ((DBxSubDocType)dtb).DocType.Name;
            if (tempDS.Tables.Contains(docTypeName))
            {
              DataTable refTable = tempDS.Tables[docTypeName];
              Int32 oldDocId = (Int32)(mainRow["DocId"]);
              DataRow refRow = refTable.Rows.Find(oldDocId);
              if (refRow != null) // может быть замена поддокумента без основного документа (DBxMultiSubDocs.InsertCopy())
                mainRow["DocId"] = refRow["NewId"];
            }
          }

          // Замена ссылочных полей
          // Перебираем реальные поля таблицы, а не TableStruct.Columns
          for (int i = 0; i < mainTable.Columns.Count; i++)
          {
            string columnName = mainTable.Columns[i].ColumnName;
            switch (columnName)
            {
              case "Id":
              case "DocId":
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }
            int pColStr = dtb.Struct.Columns.IndexOf(columnName);
            if (pColStr < 0)
              continue; // какое-то служебное поле

            DBxColumnStruct colStr = dtb.Struct.Columns[pColStr];

            if (String.IsNullOrEmpty(colStr.MasterTableName))
              continue;

            Int32 refId = DataTools.GetInt32(mainRow[i]);
            if (refId == 0)
              continue;

            if (!tempDS.Tables.Contains(colStr.MasterTableName))
              continue; // ссылочное поле на таблицу, которой нет в списке замен

            DataTable refTable = tempDS.Tables[colStr.MasterTableName];
            DataRow refRow = refTable.Rows.Find(refId);
            if (refRow == null)
              continue; // эта ссылка остается без изменений

            // Замена ссылки
            mainRow[i] = refRow["NewId"];
          }

          // Изменяем состояние документа / поддокумента
          DataTools.SetRowState(mainRow, DataRowState.Added);
        }
      }

      // Проверяем корректность
      /*
#if DEBUG
      CheckDataSet();
#endif  */
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Выполнение записи изменений/удаления всех документов в состоянии Insert, Edit и Delete на сервере
    /// Документы в состоянии View остаются без изменений.
    /// После успешного вызова, если <paramref name="reloadData"/> равен true, все документы сохраняют свой состояние.
    /// Для документов в состоянии Insert, при первом вызове, присваивается новое значение свойства DocId.
    /// Аналогично, для поддокументов, сохраняется текущее состояние и устанавливается свойство SubDocId
    /// Если <paramref name="reloadData"/> равен false, данные не возвращаются от сервера. Набор DBxDataSet очищается.
    /// Когда не требуется дальнейшая работа с записанными документами, рекомендуется задавать <paramref name="reloadData"/>=false
    /// </summary>
    /// <param name="reloadData">Нужно ли возвращать данные от сервера</param>
    public void ApplyChanges(bool reloadData)
    {
      if (!reloadData)
        ClearView(); // Уменьшаем объем передаваемых данных

      InternalDeleteUnusedBinDataAndFiles();

      _DataSet.ExtendedProperties["DocSet"] = null;

      // Убрано 05.07.2016 
      // Иначе редактор документа DocumentEditor не сможет установить блокировку после создания нового документа
      // FIgnoredLocks.SetReadOnly();
      _DataSet.ExtendedProperties["IgnoredLocks"] = StdConvert.ToString(_IgnoredLocks.ToArray());
      TimeSpan EditTime = DateTime.Now - StartTime;
      _DataSet.ExtendedProperties["EditTime"] = EditTime.ToString();
      _DataSet.ExtendedProperties["UseAsyngWriting"] = UseAsyncWriting ? "1" : "0";

      try
      {
        _DataSet = DocProvider.ApplyChanges(_DataSet, reloadData);
      }
      finally
      {
        if (_DataSet != null)
          _DataSet.ExtendedProperties["DocSet"] = this;
      }
      if (!reloadData)
      {
        _DataSet = new DataSet();
        _DataSet.ExtendedProperties["DocSet"] = this;
        _MultiDocs.Clear();
      }
      else
      {
        foreach (DBxMultiDocs mDocs in _MultiDocs)
          mDocs.InitTables();
      }
    }

    /// <summary>
    /// Отменяет все внесенные изменения, сделанные после последнего ApplyChanges(), если он был.
    /// Очищает список всех видов документов
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i < _MultiDocs.Count; i++)
        _MultiDocs[i].ClearList();

      _MultiDocs.Clear();
      _DataSet.Tables.Clear();
      _PreloadIdsDict = null;
    }

    /// <summary>
    /// Удаляет все документы, открытые для просмотра (в состоянии View)
    /// Позволяет уменьшить объем данных, передаваемых на сервер для сохранения изменений
    /// </summary>
    public void ClearView()
    {
      for (int i = _MultiDocs.Count - 1; i >= 0; i--)
      {
        _MultiDocs[i].ClearView();
        if (_MultiDocs[i].IsEmpty)
          RemoveDocType(_MultiDocs[i].DocType.Name);
      }
    }

    private void RemoveDocType(string docTypeName)
    {
      if (_DataSet.Tables.Contains(docTypeName))
        _DataSet.Tables.Remove(docTypeName);

      _MultiDocs.Remove(docTypeName);

      _PreloadIdsDict = null; // неохота чистить
    }

    /// <summary>
    /// Изменяет документы, находящиеся в состоянии <paramref name="oldState"/> на состояние <paramref name="newState"/>
    /// </summary>
    /// <param name="oldState">Текущее состояние документов</param>
    /// <param name="newState">Новое состояние документов</param>
    /// <returns>Количество документов, состояние которых изменено</returns>
    public int ChangeDocState(DBxDocState oldState, DBxDocState newState)
    {
      int cnt = 0;
      foreach (DBxMultiDocs mDocs in _MultiDocs)
        cnt += mDocs.ChangeDocState(oldState, newState);
      return cnt;
    }


    /// <summary>
    /// Открывает на просмотр указанные документы
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void View(DBxDocSelection docSel)
    {
      foreach (string tableName in docSel.TableNames)
        this[tableName].View(docSel[tableName]);
    }

    /// <summary>
    /// Открывает на просмотр указанные документы
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void Edit(DBxDocSelection docSel)
    {
      foreach (string tableName in docSel.TableNames)
        this[tableName].Edit(docSel[tableName]);
    }

    /// <summary>
    /// Открывает на просмотр указанные документы
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void Delete(DBxDocSelection docSel)
    {
      foreach (string tableName in docSel.TableNames)
        this[tableName].Delete(docSel[tableName]);
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Получить открытые документы в виде выборки
    /// В выборку входят только документы в состоянии View и Edit
    /// Документы в режиме Insert (не записанные) и Delete не включаются
    /// </summary>
    public DBxDocSelection DocSelection
    {
      get
      {
        DBxDocSelection docSel = new DBxDocSelection(DocProvider.DBIdentity);
        for (int i = 0; i < _MultiDocs.Count; i++)
          _MultiDocs[i].AddToDocSel(docSel);
        return docSel;
      }
    }

    /// <summary>
    /// Получить выборку документов, находящихся в заданном состоянии.
    /// Предупреждение. Если <paramref name="docState"/>=Insert, в выборку не попадут 
    /// несохраненные объекты, т.к. для них нет действительных идентификаторов документов
    /// </summary>
    /// <param name="docState">Состояние документов</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection GetDocSelection(DBxDocState docState)
    {
#if DEBUG
      CheckDocState(docState);
#endif

      DBxDocSelection docSel = new DBxDocSelection(DocProvider.DBIdentity);
      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        foreach (DBxSingleDoc doc in mDocs)
        {
          if (doc.DocState == docState)
          {
            if (DocProvider.IsRealDocId(doc.DocId))
              docSel.Add(mDocs.DocType.Name, doc.DocId);
          }
        }
      }

      return docSel;
    }


    /// <summary>
    /// Получить выборку документов, находящихся в заданных состояниях.
    /// Предупреждение. Если <paramref name="docStates"/> содержит состояние Insert, в выборку не попадут 
    /// несохраненные объекты, т.к. для них нет действительных идентификаторов документов
    /// </summary>
    /// <param name="docStates">Состояния документов, которые попадут в выборку</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection GetDocSelection(DBxDocState[] docStates)
    {
#if DEBUG
      if (docStates == null)
        throw new ArgumentNullException("docStates");
      for (int i = 0; i < docStates.Length; i++)
        CheckDocState(docStates[i]);
#endif

      DBxDocSelection docSel = new DBxDocSelection(DocProvider.DBIdentity);
      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        foreach (DBxSingleDoc doc in mDocs)
        {
          if (Array.IndexOf<DBxDocState>(docStates, doc.DocState) >= 0) // исправлено 15.05.2020
          {
            if (DocProvider.IsRealDocId(doc.DocId))
              docSel.Add(mDocs.DocType.Name, doc.DocId);
          }
        }
      }

      return docSel;
    }

    #endregion

    #region IEnumerable<DBxMultiDocs> Members

    /// <summary>
    /// Возвращает перечислитель по видам документов.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<DBxMultiDocs>.Enumerator GetEnumerator()
    {
      return _MultiDocs.GetEnumerator();
    }

    IEnumerator<DBxMultiDocs> IEnumerable<DBxMultiDocs>.GetEnumerator()
    {
      return _MultiDocs.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _MultiDocs.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если для набора допускается только просмотр документов, но не изменение
    /// </summary>
    public bool IsReadOnly
    {
      get { return /*DocProvider.IsReadOnly || */VersionView; }
    }

    /// <summary>
    /// Выбрасывает ObjectReadOnlyException, если допускается только просмотр документов, но не изменение
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Проверка корректности данных

    /// <summary>
    /// Выполняет проверку корректности данных в наборе.
    /// при обнаружении ошибок генерирует исключение.
    /// Предназначен для отладочных целей.
    /// </summary>
    public void CheckDataSet()
    {
      ErrorMessageList errors = new ErrorMessageList();
      if (!Validate(errors))
        throw new ErrorMessageListException(errors);
    }

    /// <summary>
    /// Выполняет проверку корректности данных в наборе, заполняя список ошибок.
    /// Предназначен для отладочных целей.
    /// При записи изменений в базу данных (DBxRealDocProvider.Validate()) выполняется независимая проверка
    /// корректности ссылок
    /// </summary>
    /// <param name="errors">Список для добавления сообщений</param>
    /// <returns>true, если ошибок не обнаружено</returns>
    public bool Validate(ErrorMessageList errors)
    {
      if (errors == null)
        throw new ArgumentNullException("errors");

      int errorCount = errors.Count;

      #region Первый проход

      Dictionary<string, IdCollection<Int32>> queryIds = new Dictionary<string, IdCollection<Int32>>();

      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        string prefix = String.Format(Res.DBxDocSet_Msg_DocWithType, mDocs.DocType.SingularTitle);
        ValidateTable1(errors, mDocs.Table, mDocs.DocType, prefix, queryIds);
        foreach (DBxSubDocType sdt in mDocs.DocType.SubDocs)
        {
          if (!mDocs.SubDocs.ContainsSubDocs(sdt.Name))
            continue;
          DBxMultiSubDocs sds = mDocs.SubDocs[sdt.Name];

          prefix = String.Format(Res.DBxDocSet_Msg_SubDocWithType, sdt.SingularTitle);
          ValidateTable1(errors, sds.Table, sdt, prefix, queryIds);

          // Проверяем, что все поддокументы относятся к нашим документам
          foreach (DataRow subDocRow in sds.Table.Rows)
          {
            if (subDocRow.RowState == DataRowState.Deleted)
              continue; // 19.03.2016

            Int32 thisDocId = DataTools.GetInt32(subDocRow, "DocId");
            DataRow docRow = mDocs.Table.Rows.Find(thisDocId);
            if (docRow == null)
              errors.AddError(String.Format(Res.DBxDocSet_Err_NoDocForSubDoc,
                sdt.SingularTitle, DataTools.GetInt32(subDocRow, "Id"),
                mDocs.DocType.SingularTitle, thisDocId));
            else
            {
              // Проверяем состояние документа
              if (subDocRow.RowState != DataRowState.Unchanged)
              {
                switch (docRow.RowState)
                {
                  case DataRowState.Added:
                  case DataRowState.Modified:
                  case DataRowState.Deleted:
                    continue;
                  default:
                    errors.AddError(String.Format(Res.DBxDocSet_Err_DocSubDocStateMismatch,
                      sdt.SingularTitle, DataTools.GetInt32(subDocRow, "Id"), subDocRow.RowState,
                      mDocs.DocType.SingularTitle, thisDocId, docRow.RowState));
                    break;
                }
              }
            }
          }
        }
      }

      #endregion

      #region Запрос удаленных документов

      Dictionary<string, DataTable> queryTables = new Dictionary<string, DataTable>(queryIds.Count);
      foreach (KeyValuePair<string, IdCollection<Int32>> pair in queryIds)
      {
        if (!IsValidableTable(pair.Key))
          continue;
        DBxDocTypeBase dtb;
        dtb = DocProvider.DocTypes.GetByTableName(pair.Key);
        DBxColumns cols;
        if (DocProvider.DocTypes.UseDeleted)
        {
          if (dtb.IsSubDoc)
            cols = new DBxColumns("Id,Deleted,DocId,DocId.Deleted");
          else
            cols = new DBxColumns("Id,Deleted");
        }
        else // 19.12.2017
        {
          if (dtb.IsSubDoc)
            cols = new DBxColumns("Id,DocId");
          else
            cols = DBSDocType.IdColumns;
        }
        DataTable table = DocProvider.FillSelect(pair.Key, cols, new ValueInListFilter("Id", pair.Value));
        DataTools.SetPrimaryKey(table, "Id");
        queryTables.Add(pair.Key, table);
      }

      #endregion

      #region Второй проход

      foreach (DBxMultiDocs mDocs in _MultiDocs)
      {
        string prefix = String.Format(Res.DBxDocSet_Msg_DocWithType, mDocs.DocType.SingularTitle);
        ValidateTable2(errors, mDocs.Table, mDocs.DocType, prefix, queryTables);
        foreach (DBxSubDocType sdt in mDocs.DocType.SubDocs)
        {
          if (!mDocs.SubDocs.ContainsSubDocs(sdt.Name))
            continue;
          DBxMultiSubDocs sds = mDocs.SubDocs[sdt.Name];

          prefix = String.Format(Res.DBxDocSet_Msg_SubDocWithType, sdt.SingularTitle);
          ValidateTable2(errors, sds.Table, sdt, prefix, queryTables);
        }
      }

      #endregion

      return errors.Count == errorCount;
    }

    private bool IsValidableTable(string tableName) // 30.10.2024
    {
      if (DocProvider.UseBinDataRefs || DocProvider.UseFileRefs)
      {
        switch (tableName)
        {
          case "BinData":
          case "FileNames":
            return false;
        }
      }
      return true;
    }

    private void ValidateTable1(ErrorMessageList errors, DataTable table, DBxDocTypeBase docType, string prefix,
      Dictionary<string, IdCollection<Int32>> queryIds)
    {
      DBxTableStruct ts = this.DocProvider.StructSource.GetTableStruct(docType.Name);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;

        if (DataTools.GetInt32(row, "Id") < 0 && row.RowState == DataRowState.Unchanged)
          errors.AddError(String.Format(Res.DBxDocSet_Err_FicvtiveIdInUnchangedRow,
            prefix, DataTools.GetInt32(row, "Id")));

        for (int i = 0; i < ts.Columns.Count; i++)
        {
          string refTableName = ts.Columns[i].MasterTableName;
          if (!String.IsNullOrEmpty(refTableName))
          {
            switch (ts.Columns[i].ColumnName)
            {
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }


            Int32 refId = DataTools.GetInt32(row, ts.Columns[i].ColumnName);
            if (refId == 0)
              continue;
            if (refId < 0)
            {
              if (row.RowState == DataRowState.Unchanged)
                errors.AddError(String.Format(Res.DBxDocSet_Err_RefToNewInUnchangedRow,
                  prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName, refId));

              if (!_DataSet.Tables.Contains(refTableName))
              {
                errors.AddError(String.Format(Res.DBxDocSet_Err_RefToNewButNoTable,
                  prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName, refId, refTableName));
                continue;
              }

              DataRow refRow = _DataSet.Tables[refTableName].Rows.Find(refId);
              if (refRow == null)
                errors.AddError(String.Format(Res.DBxDocSet_Err_RefToNewUnknown,
                  prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName, refId, refTableName));
              else if (refRow.RowState == DataRowState.Deleted)
                errors.AddError(String.Format(Res.DBxDocSet_Err_RefToOldDeleted,
                  prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName, refId, refTableName));
            }
            else // RefId>0
            {
              if (_DataSet.Tables.Contains(refTableName))
              {
                DataRow refRow = _DataSet.Tables[refTableName].Rows.Find(refId);
                if (refRow != null)
                {
                  if (refRow.RowState == DataRowState.Deleted)
                    errors.AddError(String.Format(Res.DBxDocSet_Err_RefToNewDeleted,
                      prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName, refId, refTableName));
                  continue;
                }
              }

              IdCollection<Int32> refIds;
              if (!queryIds.TryGetValue(refTableName, out refIds))
              {
                refIds = new IdCollection<Int32>();
                queryIds.Add(refTableName, refIds);
              }
              refIds.Add(refId);
            }
          }
        }
      }
    }

    private void ValidateTable2(ErrorMessageList errors, DataTable table, DBxDocTypeBase docType, string prefix,
      Dictionary<string, DataTable> queryTables)
    {
      DBxTableStruct ts = this.DocProvider.StructSource.GetTableStruct(docType.Name);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;

        for (int i = 0; i < ts.Columns.Count; i++)
        {
          string refTableName = ts.Columns[i].MasterTableName;
          if (!String.IsNullOrEmpty(refTableName))
          {
            if (!IsValidableTable(refTableName))
              continue;

            switch (ts.Columns[i].ColumnName)
            {
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }

            Int32 refId = DataTools.GetInt32(row, ts.Columns[i].ColumnName);
            if (refId > 0)
            {
              if (_DataSet.Tables.Contains(refTableName))
              {
                DataRow RefRow1 = _DataSet.Tables[refTableName].Rows.Find(refId);
                if (RefRow1 != null)
                  continue;
              }

              DataTable refTable = queryTables[refTableName];
              DataRow refRow = refTable.Rows.Find(refId);
              if (refRow == null)
                errors.AddError(String.Format(Res.DBxDocSet_Err_RefToNotInDB,
                  prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName,
                  refTableName, refId));
              else if (DocProvider.DocTypes.UseDeleted)
              {
                if (DataTools.GetBoolean(refRow, "Deleted"))
                  errors.AddError(String.Format(Res.DBxDocSet_Err_RefToDeletedInDB,
                    prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName,
                    refTableName, refId));
                else if (refTable.Columns.Contains("DocId.Deleted"))
                {
                  DBxDocType refDoc = DocProvider.DocTypes[refTableName];
                  if (DataTools.GetBoolean(refRow, "DocId.Deleted"))
                    errors.AddError(String.Format(Res.DBxDocSet_Err_RefToDocDeletedInDB,
                      prefix, DataTools.GetInt32(row, "Id"), ts.Columns[i].ColumnName +
                      refTableName, refId,
                       refDoc.SingularTitle, DataTools.GetInt32(refRow, "Docid")));
                }
              }
            }
          }
        }
      }
    }

    #endregion

    #region Длительные блокировки

    /// <summary>
    /// Если свойство установлено в true, то при вызова ApplyChanges() не будут проверяться блокировки
    /// По умолчанию - false - выполняется проверка как долгосрочных, так и кратковременных блокировок.
    /// Отдельные блокировки можно запретить с помощью списка IgnoredLocks
    /// </summary>
    public bool IgnoreAllLocks
    {
      get { return DataTools.GetBoolean(_DataSet.ExtendedProperties["IgnoreAllLocks"]); }
      set
      {
        if (UserActionId != 0)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "UserActionId");
        _DataSet.ExtendedProperties["IgnoreAllLocks"] = value ? "1" : "0";
      }
    }

    /// <summary>
    /// Список длительных блокировок, которые требуется игнорировать.
    /// Используется в редакторе документов. Когда редактор документов устанавливает
    /// блокировку, она не должна учитываться при сохранении изменений с помощью ApplyChanges(),
    /// иначе мы заблокировали сами себя.
    /// Методы Add/RemoveLongLock() этого объекта DBxDocSet() обрабатывают список автоматически
    /// </summary>
    public IList<Guid> IgnoredLocks { get { return _IgnoredLocks; } }
    private GuidList _IgnoredLocks;

    private class GuidList : ListWithReadOnly<Guid>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }


    /// <summary>
    /// Установка длительной блокировки для загруженных документов в наборе.
    /// Блокируются только документы в режимах Edit и Delete, а также Insert, если документы уже получили идентификаторы.
    /// В случае невозможности установки блокировки генерируется DBxDocsLockException.
    /// Основная работа выполняется одноименным методом в DBxDocProvider.
    /// Добавляет полученную блокировку в список IgnoredLocks
    /// </summary>
    /// <returns>Идентификатор блокировки</returns>
    public Guid AddLongLock()
    {
      DBxDocSelection docSel = GetDocSelection(new DBxDocState[] { DBxDocState.Edit, DBxDocState.Delete, DBxDocState.Insert });
      return AddLongLock(docSel);
    }

    /// <summary>
    /// Установка длительной блокировки для выборки документов.
    /// Вызывает DBxDocProvider.AddLongLock().
    /// Текущий набор не участвует в этой операции. Метод нужен только для возможности вызова RemoveLongLock() для этого набора
    /// В случае невозможности установки блокировки генерируется DBxDocsLockException.
    /// </summary>
    /// <returns>Идентификатор блокировки</returns>
    public Guid AddLongLock(DBxDocSelection docSel)
    {
      if (docSel.IsEmpty)
        return new Guid();
      _IgnoredLocks.CheckNotReadOnly();
      Guid lockGuid = DocProvider.AddLongLock(docSel);
      try
      {
        IgnoredLocks.Add(lockGuid);
        return lockGuid;
      }
      catch
      {
        DocProvider.RemoveLongLock(lockGuid);
        throw;
      }
    }

    /// <summary>
    /// Вызывает DBxDocProvider.RemoveLongLock().
    /// Удаляет блокировку из списка IgnoredLocks
    /// </summary>
    /// <param name="lockGuid">Идентификатор блокировки, полученный от AddLongLock()</param>
    public void RemoveLongLock(Guid lockGuid)
    {
      IgnoredLocks.Remove(lockGuid);
      DocProvider.RemoveLongLock(lockGuid);
    }

    #endregion

    #region Двоичные данные и файлы

    internal object InternalSetBinData(byte[] data)
    {
      if (data == null)
        return DBNull.Value;

      string md5 = MD5Tools.MD5Sum(data);
      DataTable tblBinData = DataSet.Tables["BinData"];
      if (tblBinData != null)
      {
        // Выполняем поиск на случай, если метод вызван в процессе редактирования несколько раз для
        // одних и тех же данных
        int p = tblBinData.DefaultView.Find(md5);
        if (p >= 0)
          return tblBinData.DefaultView[p].Row["Id"];
      }

      Int32 Id = DocProvider.InternalFindBinData(md5);
      if (Id > 0)
        return Id; // на сервере уже есть такие данные

      tblBinData = GetReadyTableBinData();

      Id = (-tblBinData.Rows.Count) - 1; // -1, -2, ...
      tblBinData.Rows.Add(Id, md5, data);
      return Id;
    }

    private DataTable GetReadyTableBinData()
    {
      DataTable tblBinData = DataSet.Tables["BinData"];
      if (tblBinData == null)
      {
        tblBinData = DataSet.Tables.Add("BinData");
        tblBinData.Columns.Add("Id", typeof(Int32));
        tblBinData.Columns.Add("MD5", typeof(string));
        tblBinData.Columns.Add("Contents", typeof(byte[]));
        DataTools.SetPrimaryKey(tblBinData, "Id");
        tblBinData.DefaultView.Sort = "MD5";
      }
      return tblBinData;
    }

    internal void InternalDeleteUnusedBinDataAndFiles()
    {
      DataTable tblBinData = DataSet.Tables["BinData"];
      DataTable tblFileNames = DataSet.Tables["FileNames"];
      if (tblBinData == null && tblFileNames == null)
        return; // нет новых данных

      IdCollection<Int32> binDataIds = new IdCollection<Int32>();
      IdCollection<Int32> fileNameIds = new IdCollection<Int32>();

      #region Список используемых фиктивных идентификаторов

      foreach (DataTable table in _DataSet.Tables)
      {
        if (table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocProvider.DocTypes.FindByTableName(table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.BinDataRefs.Count; i++)
        {
          int p = table.Columns.IndexOf(dt.BinDataRefs[i].Column.ColumnName);
#if DEBUG
          if (p < 0)
            throw new BugException("Column \"" + dt.BinDataRefs[i].Column.ColumnName +
              "\" not found in table \"" + table.TableName + "\", which presented in BinDataRefs");
#endif
          for (int j = 0; j < table.Rows.Count; j++)
          {
            if (table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 id = DataTools.GetInt32(table.Rows[j][p]);
            if (id < 0)
              binDataIds.Add(id);
          }
        }

        for (int i = 0; i < dt.FileRefs.Count; i++)
        {
          int p = table.Columns.IndexOf(dt.FileRefs[i].Column.ColumnName);
#if DEBUG
          if (p < 0)
            throw new BugException("Column \"" + dt.FileRefs[i].Column.ColumnName +
              "\" not found in table \"" + table.TableName + "\", which presented in FileRefs");
#endif
          for (int j = 0; j < table.Rows.Count; j++)
          {
            if (table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 id = DataTools.GetInt32(table.Rows[j][p]);
            if (id < 0)
              fileNameIds.Add(id);
          }
        }
      }

      #endregion

      #region Список ссылок на данные в таблице файлов

      if (tblFileNames != null)
      {
        foreach (DataRow row in tblFileNames.Rows)
        {
          Int32 dataId = DataTools.GetInt32(row, "Data");
          if (dataId < 0)
            binDataIds.Add(dataId);
        }
      }

      #endregion

      #region Проверяем "потерянные" идентификаторы

      if (binDataIds.Count > 0)
      {
        if (tblBinData == null)
          throw new BugException("DBxDocSet contains references to new binary data, but the there is no 'BinData' table exists");

        foreach (Int32 id in binDataIds)
        {
          if (tblBinData.Rows.Find(id) == null)
            throw new BugException("DBxDocSet contains references to new binary data with Id=" + id.ToString() + ", but table 'BinData' does not contain a row with this id");
        }
      }

      if (fileNameIds.Count > 0)
      {
        if (tblFileNames == null)
          throw new BugException("DBxDocSet contains references to new files, but the there is no 'FileNames' table exists");
        foreach (Int32 id in fileNameIds)
        {
          if (tblFileNames.Rows.Find(id) == null)
            throw new BugException("DBxDocSet contains references to new file with Id=" + id.ToString() + ", but table 'FileNames' does not contain a row with this id");
        }
      }

      #endregion

      #region Удаление лишних строк

      if (tblBinData != null)
      {
        for (int i = tblBinData.Rows.Count - 1; i >= 0; i--)
        {
          Int32 id = (Int32)(tblBinData.Rows[i][0]);
          if (!binDataIds.Contains(id))
            tblBinData.Rows.RemoveAt(i);
        }

        tblBinData.AcceptChanges();
      }

      if (tblFileNames != null)
      {
        for (int i = tblFileNames.Rows.Count - 1; i >= 0; i--)
        {
          Int32 id = (Int32)(tblFileNames.Rows[i][0]);
          if (!fileNameIds.Contains(id))
            tblFileNames.Rows.RemoveAt(i);
        }

        tblFileNames.AcceptChanges();
      }

      #endregion
    }

    internal byte[] InternalGetBinData(object binDataId, string docTypeName, Int32 docId, string subDocTypeName, Int32 subDocId, string columnName)
    {
      Int32 binDataId2 = DataTools.GetInt32(binDataId);
      if (binDataId2 == 0)
        return null;

      #region Поиск во временной таблице

      if (binDataId2 < 0)
      {
        DataTable tblBinData = DataSet.Tables["BinData"];
        if (tblBinData == null)
          throw new InvalidOperationException(Res.DBxDocSet_Err_NoBinDataTable);

        DataRow row = tblBinData.Rows.Find(binDataId2);
        if (row == null)
          throw new InvalidOperationException(String.Format(Res.DBxDocSet_Err_UnknownBinDataId, binDataId2));
        return (byte[])(row["Contents"]);
      }

      #endregion

      int docVersion = 0;
      if (VersionView)
        docVersion = this[docTypeName].GetDocById(docId).Version;

      string tableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;
      List<DBxDocProvider.DocSubDocDataId> PreloadIds = GetPreloadIds(tableName, columnName, !String.IsNullOrEmpty(subDocTypeName));

      DBxDocProvider.DocSubDocDataId wantedId = new DBxDocProvider.DocSubDocDataId(docId, subDocId, binDataId2);
      return DocProvider.InternalGetBinData1(tableName, columnName, wantedId, docVersion, PreloadIds);
    }

    internal object InternalSetFile(FreeLibSet.IO.FileContainer file)
    {
      if (file == null)
        return DBNull.Value;

      Int32 dataId;
      string md5 = MD5Tools.MD5Sum(file.Content);

      #region Проверка повторного вызова

      DataTable tblFileNames = DataSet.Tables["FileNames"];
      if (tblFileNames != null)
      {
        // Выполняем поиск на случай, если метод вызван в процессе редактирования несколько раз для
        // одного и того же файла

        // Таблица BinData может существовать, а может и не существовать.
        // Ранее файл мог быть добавлен в FileNames, но данные не менялись
        DataTable tblBinData = DataSet.Tables["BinData"];

        if (tblBinData != null)
        {
          int p = tblBinData.DefaultView.Find(md5);
          if (p >= 0)
          {
            dataId = (Int32)(tblBinData.DefaultView[p].Row["Id"]);
            p = tblFileNames.DefaultView.Find(new object[] {
            file.FileInfo.Name, dataId, file.FileInfo.CreationTime, file.FileInfo.LastWriteTime});
            if (p >= 0)
              return tblFileNames.DefaultView[p].Row["Id"];
          }
        }
      }

      #endregion

      #region Обращение к серверу

      Int32 fileId = DocProvider.InternalFindDBFile(file.FileInfo, md5, out dataId);
      if (fileId > 0)
        return fileId; // на сервере уже есть такие данные

      #endregion

      #region Добавление в BinData и FileNames

      if (dataId == 0)
      {
        DataTable tblBinData = GetReadyTableBinData();

        // Пытаемся найти данные еще раз.
        // Может устанавливаться новый файл но с содержимым, совпадающим с предыдущим вызовом 
        int p = tblBinData.DefaultView.Find(md5);
        if (p >= 0)
          dataId = (Int32)(tblBinData.DefaultView[p].Row["Id"]);
        else
        {
          dataId = (-tblBinData.Rows.Count) - 1; // -1, -2, ...
          tblBinData.Rows.Add(dataId, md5, file.Content);
        }
      }

      tblFileNames = GetReadyTableFileNames();

      fileId = (-tblFileNames.Rows.Count) - 1; // -1, -2, ...
      tblFileNames.Rows.Add(fileId, file.FileInfo.Name, dataId,
        file.FileInfo.CreationTime, file.FileInfo.LastWriteTime,
        file.Content.Length);

      #endregion

      return fileId;
    }

    private DataTable GetReadyTableFileNames()
    {
      DataTable tblFileNames = DataSet.Tables["FileNames"];
      if (tblFileNames == null)
      {
        tblFileNames = DataSet.Tables.Add("FileNames");
        tblFileNames.Columns.Add("Id", typeof(Int32));
        tblFileNames.Columns.Add("Name", typeof(string));
        tblFileNames.Columns.Add("Data", typeof(Int32)); // ссылка на таблицу BinData
        tblFileNames.Columns.Add("CreationTime", typeof(DateTime));
        tblFileNames.Columns.Add("LastWriteTime", typeof(DateTime));
        tblFileNames.Columns.Add("Length", typeof(int)); // Нужно для ускоренного вызова InternalGetFileInfo()
        DataTools.SetPrimaryKey(tblFileNames, "Id");
        //                                 0   1         2            3
        tblFileNames.DefaultView.Sort = "Name,Data,CreationTime,LastWriteTime";
      }
      return tblFileNames;
    }


    internal FreeLibSet.IO.StoredFileInfo InternalGetDBFileInfo(object fileId, string docTypeName, Int32 docId, string subDocTypeName, Int32 subDocId, string columnName)
    {
      Int32 fileId2 = DataTools.GetInt32(fileId);
      if (fileId2 == 0)
        return FreeLibSet.IO.StoredFileInfo.Empty;

      #region Поиск во временной таблице

      if (fileId2 < 0)
      {
        DataTable tblFileNames = DataSet.Tables["FileNames"];
        if (tblFileNames == null)
          throw new InvalidOperationException(Res.DBxDocSet_Err_NoFileNameTable);

        DataRow row = tblFileNames.Rows.Find(fileId2);
        if (row == null)
          throw new InvalidOperationException(String.Format(Res.DBxDocSet_Err_UnknownFileNameId, fileId2));
        return new FreeLibSet.IO.StoredFileInfo((string)(row["Name"]),
          (int)(row["Length"]), DataTools.GetNullableDateTime(row, "CreationTime"), DataTools.GetNullableDateTime(row, "LastWriteTime"));
      }

      #endregion

      return DocProvider.GetDBFileInfo(fileId2);
    }

    internal FreeLibSet.IO.FileContainer InternalGetDBFile(object fileId, string docTypeName, Int32 docId, string subDocTypeName, Int32 subDocId, string columnName)
    {
      Int32 fileId2 = DataTools.GetInt32(fileId);
      if (fileId2 == 0)
        return null;

      #region Поиск во временной таблице

      if (fileId2 < 0)
      {
        DataTable tblFileNames = DataSet.Tables["FileNames"];
        if (tblFileNames == null)
          throw new InvalidOperationException(Res.DBxDocSet_Err_NoFileNameTable);

        DataRow row = tblFileNames.Rows.Find(fileId2);
        if (row == null)
          throw new InvalidOperationException(String.Format(Res.DBxDocSet_Err_UnknownFileNameId, fileId2));
        FreeLibSet.IO.StoredFileInfo FileInfo = new FreeLibSet.IO.StoredFileInfo((string)(row["Name"]),
          (int)(row["Length"]), DataTools.GetNullableDateTime(row, "CreationTime"), DataTools.GetNullableDateTime(row, "LastWriteTime"));

        Int32 binDataId = (Int32)(row["Data"]);
        // DataId может быть и фиктивным идентификатором, и реальным
        // Если DataId>0, то все равно придется обращаться к серверу
        byte[] contents = InternalGetBinData(binDataId, docTypeName, docId, subDocTypeName, subDocId, columnName);
        return new FreeLibSet.IO.FileContainer(FileInfo, contents);
      }

      #endregion

      int docVersion = 0;
      if (VersionView)
        docVersion = this[docTypeName].GetDocById(docId).Version;

      string tableName = String.IsNullOrEmpty(subDocTypeName) ? docTypeName : subDocTypeName;
      List<DBxDocProvider.DocSubDocDataId> PreloadIds = GetPreloadIds(tableName, columnName, !String.IsNullOrEmpty(subDocTypeName));

      DBxDocProvider.DocSubDocDataId wantedId = new DBxDocProvider.DocSubDocDataId(docId, subDocId, fileId2);

      return DocProvider.InternalGetDBFile1(tableName, columnName, wantedId, docVersion, PreloadIds);
    }

    #region Коллекции предзагруженных идентификаторов

    /// <summary>
    /// Дополняет обычный список, который требуется DBxDocProvider,
    /// полем для хранения числа строк в таблице.
    /// Если таблица пополнилась, по сравнению с прошлым разом,
    /// например при вызове DBxMultiDocs.View(), список требуется перестроить
    /// </summary>
    private class DataIdInfo
    {
      #region Конструктор

      public DataIdInfo()
      {
        _List = new List<DBxDocProvider.DocSubDocDataId>();
      }

      #endregion

      #region Поля

      public List<DBxDocProvider.DocSubDocDataId> List { get { return _List; } }
      private List<DBxDocProvider.DocSubDocDataId> _List;

      public int TableRowCount;

      #endregion
    }

    /// <summary>
    /// Списки идентификаторов предзагрузки для двоичных данных или файлов
    /// Ключ - "ИмяТаблицы|ИмяПоля"
    /// Значение - список идентификаторов документа + поддокумента + BinDataId/FileId, которые нужно загрузить
    /// </summary>
    private Dictionary<string, DataIdInfo> _PreloadIdsDict;

    /// <summary>
    /// Получение списка идентификаторов для предзагрузки (двоичные данные и файлы)
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца в таблице данных</param>
    /// <param name="isSubDoc">Поддокументы?</param>
    /// <returns>Список идентификаторов для предзагрузки</returns>
    private List<DBxDocProvider.DocSubDocDataId> GetPreloadIds(string tableName, string columnName, bool isSubDoc)
    {
      if (_PreloadIdsDict == null)
        _PreloadIdsDict = new Dictionary<string, DataIdInfo>();

      DataIdInfo info;
      if (_PreloadIdsDict.TryGetValue(tableName + "|" + columnName, out info))
      {
        if (info.TableRowCount == _DataSet.Tables[tableName].Rows.Count)
          return info.List;
      }

      // Требуется перестроение списка
      info = new DataIdInfo();
      IdCollection<Int32> tempIds = new IdCollection<Int32>();
      foreach (DataRow row in _DataSet.Tables[tableName].Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;

        Int32 binDataId = DataTools.GetInt32(row, columnName);
        if (binDataId <= 0)
          continue;
        if (tempIds.Contains(binDataId))
          continue; // уже есть

        Int32 docId, subDocId;
        if (isSubDoc)
        {
          docId = DataTools.GetInt32(row, "DocId");
          subDocId = DataTools.GetInt32(row, "Id");
        }
        else
        {
          docId = DataTools.GetInt32(row, "Id");
          subDocId = 0;
        }
        //if (docId < 0 || subDocId < 0)
        //  continue; // фиктивные идентификаторы
        info.List.Add(new DBxDocProvider.DocSubDocDataId(docId, subDocId, binDataId));
        tempIds.Add(binDataId);
      }

      info.TableRowCount = _DataSet.Tables[tableName].Rows.Count;
      _PreloadIdsDict[tableName + "|" + columnName] = info;
      return info.List;
    }

    #endregion

    #endregion

    #region Статические методы

    internal static DBxDocState GetDocState(DataRow dataRow)
    {
      switch (dataRow.RowState)
      {
        case DataRowState.Added: return DBxDocState.Insert;
        case DataRowState.Modified: return DBxDocState.Edit;
        case DataRowState.Deleted: return DBxDocState.Delete;
        case DataRowState.Unchanged: return DBxDocState.View;
        default: throw new BugException("Document table row is invalid RowState=" + dataRow.RowState.ToString());
      }
    }

    internal static object GetValue(DataRow dataRow, string columnName)
    {
      if (dataRow.RowState == DataRowState.Deleted)
        return dataRow[columnName, DataRowVersion.Original];
      else
        return dataRow[columnName];
    }

    internal static object GetValue(DataRow dataRow, int columnIndex)
    {
      if (dataRow.RowState == DataRowState.Deleted)
        return dataRow[columnIndex, DataRowVersion.Original];
      else
        return dataRow[columnIndex];
    }

    /// <summary>
    /// Возвращает true, если есть изменения в каком-либо поле строки
    /// </summary>
    /// <param name="dataRow"></param>
    /// <returns></returns>
    internal static bool GetIsModified(DataRow dataRow)
    {
      for (int i = 0; i < dataRow.Table.Columns.Count; i++)
      {
        object v1 = dataRow[i, DataRowVersion.Original];
        object v2 = dataRow[i, DataRowVersion.Current];
        if (!DataTools.AreValuesEqual(v1, v2))
          return true;
      }
      return false;
    }

    #endregion
  }
}
