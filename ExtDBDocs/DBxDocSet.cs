// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

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
      if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(docProvider))
        throw new ArgumentException("Провайдер DocProvider является удаленным объектом (TransparentProxy). " +
          "Для правильной работы требуется, чтобы он был в текущем контексте. Используйте переходник DBxChainDocProvider", "docProvider");

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
      foreach (DataTable Table in dataSet.Tables)
      {
        DBxDocType DocType = docProvider.DocTypes[Table.TableName];
        if (DocType != null)
        {
          DBxMultiDocs MultiDocs = new DBxMultiDocs(this, DocType);
          _MultiDocs.Add(MultiDocs);
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
        DBxMultiDocs Res;
        if (!_MultiDocs.TryGetValue(docTypeName, out Res))
        {
          DBxDocType dt = DocProvider.DocTypes[docTypeName];
          if (dt == null)
          {
            if (String.IsNullOrEmpty(docTypeName))
              throw new ArgumentNullException("docTypeName");
            else
              throw new ArgumentException("Неизвестный тип документов \"" + docTypeName + "\"", "docTypeName");
          }
          DBxMultiDocs Res2 = new DBxMultiDocs(this, dt);
          if (!_MultiDocs.TryGetValue(docTypeName, out Res)) // 22.01.2019 поддержка реетрабельности
          {
            Res = Res2;
            _MultiDocs.Add(Res);
          }
        }
        return Res;
      }
    }

    /// <summary>
    /// Доступ к коллекции документов одного вида по индекск
    /// </summary>
    /// <param name="docTypeIndex">Индекс вида документов от 0 до Count-1</param>
    /// <returns></returns>
    public DBxMultiDocs this[int docTypeIndex]
    {
      get { return _MultiDocs[docTypeIndex]; }
    }

    /// <summary>
    /// Возвращает количество видов документов в наборе.
    /// Для получения количества документов используйте свойство DocCount.
    /// </summary>
    public int Count { get { return _MultiDocs.Count; } }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Провайдер для работы с документами.
    /// Задается в конструкторе.
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// Хранилище для всех документов.
    /// Прикладной код не должен использовать это свойство.
    /// </summary>
    public DataSet DataSet { get { return _DataSet; } }
    private DataSet _DataSet;


    /// <summary>
    /// Идентификатор действия пользователя для undo. Присваивается
    /// после первого вызова ApplyChanges()
    /// </summary>
    public Int32 UserActionId
    {
      get { return DataTools.GetInt(_DataSet.ExtendedProperties["UserActionId"]); }
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
          throw new InvalidOperationException("Нельзя устанавливать свойство ActionInfo после применения изменений");
        _DataSet.ExtendedProperties["ActionInfo"] = value;
      }
    }

    /// <summary>
    /// Надо ли проверять документ после записи (по умолчанию - false)
    /// </summary>
    public bool CheckDocs
    {
      get { return DataTools.GetBool(_DataSet.ExtendedProperties["CheckDocs"]); }
      set
      {
        if (UserActionId != 0)
          throw new InvalidOperationException("Нельзя устанавливать свойство CheckDocs после применения изменений");
        _DataSet.ExtendedProperties["CheckDocs"] = value ? "1" : "0";
      }
    }

#if XXX // все равно не работает
    /// <summary>
    /// Надо ли записывать документы в режиме Edit, если в них не было никаких изменений.
    /// По умолчанию - false. При этом документу не присваивается новый номер версии.
    /// На документы в других состояниях не влияет
    /// </summary>
    public bool EditIfNotChanged
    {
      get { return DataTools.GetBool(_DataSet.ExtendedProperties["EditIfNotChanged"]); }
      set
      {
        if (UserActionId != 0)
          throw new InvalidOperationException("Нельзя устанавливать свойство EditIfNotChanged после применения изменений");
        _DataSet.ExtendedProperties["EditIfNotChanged"] = value ? "1" : "0";
      }
    }
#endif

    /// <summary>
    /// Коллекция наборов документов по типам
    /// </summary>
    private NamedList<DBxMultiDocs> _MultiDocs;

    /// <summary>
    /// Состояния документов.
    /// Если документы находятся в разных состояниях, возвращается DBxDocState.Mixed.
    /// </summary>
    public DBxDocState DocState
    {
      get
      {
        bool FirstFlag = true;
        DBxDocState Res = DBxDocState.None;
        foreach (DBxMultiDocs MultiDocs in _MultiDocs)
        {
          DBxDocState Res2 = MultiDocs.DocState;
          if (FirstFlag)
          {
            Res = Res2;
            FirstFlag = false; // 28.12.2020
          }
          else
          {
            if (Res2 == DBxDocState.None)
              continue;
            if (Res2 != Res)
              return DBxDocState.Mixed;
          }
        }

        return Res;
      }
    }


    /// <summary>
    /// Состояния документов. Документы в состоянии View пропускаются
    /// </summary>
    public DBxDocState DocStateNoView
    {
      get
      {
        bool FirstFlag = true;
        DBxDocState Res = DBxDocState.None;
        foreach (DBxMultiDocs MultiDocs in _MultiDocs)
        {
          DBxDocState Res2 = MultiDocs.DocStateNoView;
          if (FirstFlag)
          {
            Res = Res2;
            FirstFlag = false;
          }
          else
          {
            if (Res2 == DBxDocState.None)
              continue;
            if (Res2 != Res)
              return DBxDocState.Mixed;
          }
        }

        return Res;
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
          throw new ArgumentException("Недопустимое значение state=" + state.ToString(), "state");
      }
    }

#endif

    /// <summary>
    /// Возвращает true, если в наборе есть документы заданного типа (в любом состоянии).
    /// В отличие от вызова MultiDocs.Contains() проверяет наличие ненулевого количества документов.
    /// </summary>
    /// <param name="docTypeName">Имя вида документа (совпадает с именем таблицы в базе даннных)</param>
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
      foreach (DBxMultiDocs MultiDocs in _MultiDocs)
      {
        if (MultiDocs.DocCount == 0)
          continue;
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(MultiDocs.DocType.PluralTitle);
        sb.Append(" DocCount=");
        sb.Append(MultiDocs.DocCount.ToString());
        sb.Append(", DocState=");
        sb.Append(MultiDocs.DocState.ToString());
      }

      if (!String.IsNullOrEmpty(ActionInfo))
      {
        sb.Append(", ActionInfo=");
        sb.Append(ActionInfo);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Возвращает true, если набор предназначен для просмотра версий документов
    /// </summary>
    public bool VersionView
    {
      get { return DataTools.GetBool(_DataSet.ExtendedProperties["VersionView"]); }
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
      DataSet TempDS = new DataSet();
      foreach (DBxMultiDocs mDocs in this)
      {
        Int32[] DocIds = mDocs.GetDocIds(DBxDocState.View);
        for (int i = 0; i < DocIds.Length; i++)
          mDocs.DoInsertCopy1(TempDS, DocIds[i]);
      }

      DBxDocSet.DoInsertCopy2(TempDS, DataSet, DocProvider);
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

      DataTable Table;
      if (tempDS.Tables.Contains(tableName))
        Table = tempDS.Tables[tableName];
      else
      {
        Table = tempDS.Tables.Add(tableName);
        Table.Columns.Add("Id", typeof(Int32));
        Table.Columns.Add("NewId", typeof(Int32));
        DataTools.SetPrimaryKey(Table, "Id");
      }

      Table.Rows.Add(currId, newId);
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
      foreach (DataTable TempTable in tempDS.Tables)
      {
        DBxDocTypeBase dtb;
        docProvider.DocTypes.FindByTableName(TempTable.TableName, out dtb);

        DataTable MainTable = mainDS.Tables[TempTable.TableName]; // таблица документа или поддокумента

        foreach (DataRow TempRow in TempTable.Rows)
        {
          Int32 OldId = (Int32)(TempRow["Id"]);
          Int32 NewId = (Int32)(TempRow["NewId"]);
#if DEBUG
          if (OldId == 0) // может быть <0
            throw new BugException("OldId=" + OldId.ToString());
          if (NewId >= 0)
            throw new BugException("NewId=" + NewId.ToString());
#endif

          DataRow MainRow = MainTable.Rows.Find(OldId);
          if (MainRow == null)
            throw new BugException("Не нашли основную строку таблицы \"" + TempTable.TableName + "\", Id=" + OldId.ToString());

          MainRow["Id"] = NewId;
          if (dtb.IsSubDoc)
          {
            string DocTypeName = ((DBxSubDocType)dtb).DocType.Name;
            if (tempDS.Tables.Contains(DocTypeName))
            {
              DataTable RefTable = tempDS.Tables[DocTypeName];
              Int32 OldDocId = (Int32)(MainRow["DocId"]);
              DataRow RefRow = RefTable.Rows.Find(OldDocId);
              if (RefRow != null) // может быть замена поддокумента без основного документа (DBxMultiSubDocs.InsertCopy())
                MainRow["DocId"] = RefRow["NewId"];
            }
          }

          // Замена ссылочных полей
          // Перебираем реальные поля таблицы, а не TableStruct.Columns
          for (int i = 0; i < MainTable.Columns.Count; i++)
          {
            string ColumnName = MainTable.Columns[i].ColumnName;
            switch (ColumnName)
            {
              case "Id":
              case "DocId":
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }
            int pColStr = dtb.Struct.Columns.IndexOf(ColumnName);
            if (pColStr < 0)
              continue; // какое-то служебное поле

            DBxColumnStruct ColStr = dtb.Struct.Columns[pColStr];

            if (String.IsNullOrEmpty(ColStr.MasterTableName))
              continue;

            Int32 RefId = DataTools.GetInt(MainRow[i]);
            if (RefId == 0)
              continue;

            if (!tempDS.Tables.Contains(ColStr.MasterTableName))
              continue; // ссылочное поле на таблицу, которой нет в списке замен

            DataTable RefTable = tempDS.Tables[ColStr.MasterTableName];
            DataRow RefRow = RefTable.Rows.Find(RefId);
            if (RefRow == null)
              continue; // эта ссылка остается без изменений

            // Замена ссылки
            MainRow[i] = RefRow["NewId"];
          }

          // Изменяем состояние документа / поддокумента
          DataTools.SetRowState(MainRow, DataRowState.Added);
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
        foreach (DBxMultiDocs MultiDocs in this)
          MultiDocs.InitTables();
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
      foreach (DBxMultiDocs MultiDocs in this)
        cnt += MultiDocs.ChangeDocState(oldState, newState);
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
        DBxDocSelection DocSel = new DBxDocSelection(DocProvider.DBIdentity);
        for (int i = 0; i < _MultiDocs.Count; i++)
          _MultiDocs[i].AddToDocSel(DocSel);
        return DocSel;
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

      DBxDocSelection DocSel = new DBxDocSelection(DocProvider.DBIdentity);
      foreach (DBxMultiDocs MultiDocs in this)
      {
        foreach (DBxSingleDoc Doc in MultiDocs)
        {
          if (Doc.DocState == docState)
          {
            if (DocProvider.IsRealDocId(Doc.DocId))
              DocSel.Add(MultiDocs.DocType.Name, Doc.DocId);
          }
        }
      }

      return DocSel;
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

      DBxDocSelection DocSel = new DBxDocSelection(DocProvider.DBIdentity);
      foreach (DBxMultiDocs MultiDocs in this)
      {
        foreach (DBxSingleDoc Doc in MultiDocs)
        {
          if (Array.IndexOf<DBxDocState>(docStates, Doc.DocState) >= 0) // исправлено 15.05.2020
          {
            if (DocProvider.IsRealDocId(Doc.DocId))
              DocSel.Add(MultiDocs.DocType.Name, Doc.DocId);
          }
        }
      }

      return DocSel;
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
      ErrorMessageList Errors = new ErrorMessageList();
      if (!Validate(Errors))
        throw new ErrorMessageListException(Errors);
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

      int ErrorCount = errors.Count;

      #region Первый проход

      Dictionary<string, IdList> QueryIds = new Dictionary<string, IdList>();

      foreach (DBxMultiDocs mDocs in this)
      {
        string Prefix = "Документ \"" + mDocs.DocType.SingularTitle + "\"";
        ValidateTable1(errors, mDocs.Table, mDocs.DocType, Prefix, QueryIds);
        foreach (DBxSubDocType sdt in mDocs.DocType.SubDocs)
        {
          if (!mDocs.SubDocs.ContainsSubDocs(sdt.Name))
            continue;
          DBxMultiSubDocs sds = mDocs.SubDocs[sdt.Name];

          Prefix = "Поддокумент \"" + sdt.SingularTitle + "\"";
          ValidateTable1(errors, sds.Table, sdt, Prefix, QueryIds);

          // Проверяем, что все поддокументы относятся к нашим документам
          foreach (DataRow SubDocRow in sds.Table.Rows)
          {
            if (SubDocRow.RowState == DataRowState.Deleted)
              continue; // 19.03.2016

            Int32 ThisDocId = DataTools.GetInt(SubDocRow, "DocId");
            DataRow DocRow = mDocs.Table.Rows.Find(ThisDocId);
            if (DocRow == null)
              errors.AddError("Поддокумент \"" + sds.Table.TableName + "\" с идентификатором SubDocId=" +
                DataTools.GetInt(SubDocRow, "Id") + " относится к документу с идентификатором DocId=" + ThisDocId.ToString() +
                ", которого нет в наборе");
            else
            {
              // Проверяем состояние документа
              if (SubDocRow.RowState != DataRowState.Unchanged)
              {
                switch (DocRow.RowState)
                {
                  case DataRowState.Added:
                  case DataRowState.Modified:
                  case DataRowState.Deleted:
                    continue;
                  default:
                    errors.AddError("Поддокумент \"" + sds.Table.TableName + "\" с идентификатором SubDocId=" +
                      DataTools.GetInt(SubDocRow, "Id") + " находится в состоянии " + SubDocRow.RowState.ToString() + ", а строка документа с идентификатором DocId=" + ThisDocId.ToString() +
                      ", к которому относится поддокумент - в состоянии " + DocRow.RowState.ToString());
                    break;
                }
              }
            }
          }
        }
      }

      #endregion

      #region Запрос удаленных документов

      Dictionary<string, DataTable> QueryTables = new Dictionary<string, DataTable>(QueryIds.Count);
      foreach (KeyValuePair<string, IdList> Pair in QueryIds)
      {
        DBxDocTypeBase dtb;
        if (!DocProvider.DocTypes.FindByTableName(Pair.Key, out dtb))
          throw new BugException("Не найден документ или поддокумент с именем \"" + Pair.Key + "\"");
        DBxColumns Cols;
        if (DocProvider.DocTypes.UseDeleted)
        {
          if (dtb.IsSubDoc)
            Cols = new DBxColumns("Id,Deleted,DocId,DocId.Deleted");
          else
            Cols = new DBxColumns("Id,Deleted");
        }
        else // 19.12.2017
        {
          if (dtb.IsSubDoc)
            Cols = new DBxColumns("Id,DocId");
          else
            Cols = DBSDocType.IdColumns;
        }
        DataTable Table = DocProvider.FillSelect(Pair.Key, Cols, new IdsFilter(Pair.Value));
        DataTools.SetPrimaryKey(Table, "Id");
        QueryTables.Add(Pair.Key, Table);
      }

      #endregion

      #region Второй проход

      foreach (DBxMultiDocs mDocs in this)
      {
        string Prefix = "Документ \"" + mDocs.DocType.SingularTitle + "\"";
        ValidateTable2(errors, mDocs.Table, mDocs.DocType, Prefix, QueryTables);
        foreach (DBxSubDocType sdt in mDocs.DocType.SubDocs)
        {
          if (!mDocs.SubDocs.ContainsSubDocs(sdt.Name))
            continue;
          DBxMultiSubDocs sds = mDocs.SubDocs[sdt.Name];

          Prefix = "Поддокумент \"" + sdt.SingularTitle + "\"";
          ValidateTable2(errors, sds.Table, sdt, Prefix, QueryTables);
        }
      }

      #endregion

      return errors.Count == ErrorCount;
    }

    private void ValidateTable1(ErrorMessageList errors, DataTable table, DBxDocTypeBase docType, string prefix,
      Dictionary<string, IdList> queryIds)
    {
      DBxTableStruct ts = this.DocProvider.StructSource.GetTableStruct(docType.Name);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;

        if (DataTools.GetInt(Row, "Id") < 0 && Row.RowState == DataRowState.Unchanged)
          errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ". Строка находится в состоянии Unchanged, а Id=" + DataTools.GetInt(Row, "Id"));

        for (int i = 0; i < ts.Columns.Count; i++)
        {
          string RefTableName = ts.Columns[i].MasterTableName;
          if (!String.IsNullOrEmpty(RefTableName))
          {
            switch (ts.Columns[i].ColumnName)
            {
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }


            Int32 RefId = DataTools.GetInt(Row, ts.Columns[i].ColumnName);
            if (RefId == 0)
              continue;
            if (RefId < 0)
            {
              if (Row.RowState == DataRowState.Unchanged)
                errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                  "\". Задана ссылка на новый документ/поддокумент RefId=" + RefId.ToString() +
                  ", но строка находится в режиме просмотра");

              if (!_DataSet.Tables.Contains(RefTableName))
              {
                errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                  "\". Задана ссылка на новый документ/поддокумент RefId=" + RefId.ToString() +
                  ", но в наборе нет таблицы \"" + RefTableName + "\"");
                continue;
              }

              DataRow RefRow = _DataSet.Tables[RefTableName].Rows.Find(RefId);
              if (RefRow == null)
                errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                  "\". Задана ссылка на новый документ/поддокумент RefId=" + RefId.ToString() +
                  ", но в таблице \"" + RefTableName + "\" нет строки с таким идентификатором");
              else if (RefRow.RowState == DataRowState.Deleted)
                errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                  "\". Задана ссылка на новый документ/поддокумент RefId=" + RefId.ToString() +
                  ", но в таблице \"" + RefTableName + "\" строка помечена на удаление");
            }
            else // RefId>0
            {
              if (_DataSet.Tables.Contains(RefTableName))
              {
                DataRow RefRow = _DataSet.Tables[RefTableName].Rows.Find(RefId);
                if (RefRow != null)
                {
                  if (RefRow.RowState == DataRowState.Deleted)
                    errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                     "\". Задана ссылка на существующий документ/поддокумент RefId=" + RefId.ToString() +
                     ", но в таблице \"" + RefTableName + "\" строка помечена на удаление");
                  continue;
                }
              }

              IdList RefIds;
              if (!queryIds.TryGetValue(RefTableName, out RefIds))
              {
                RefIds = new IdList();
                queryIds.Add(RefTableName, RefIds);
              }
              RefIds.Add(RefId);
            }
          }
        }
      }
    }

    private void ValidateTable2(ErrorMessageList errors, DataTable table, DBxDocTypeBase docType, string prefix,
      Dictionary<string, DataTable> queryTables)
    {
      DBxTableStruct ts = this.DocProvider.StructSource.GetTableStruct(docType.Name);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;

        for (int i = 0; i < ts.Columns.Count; i++)
        {
          string RefTableName = ts.Columns[i].MasterTableName;
          if (!String.IsNullOrEmpty(RefTableName))
          {
            switch (ts.Columns[i].ColumnName)
            {
              case "CreateUserId":
              case "ChangeUserId":
                continue;
            }

            Int32 RefId = DataTools.GetInt(Row, ts.Columns[i].ColumnName);
            if (RefId > 0)
            {
              if (_DataSet.Tables.Contains(RefTableName))
              {
                DataRow RefRow1 = _DataSet.Tables[RefTableName].Rows.Find(RefId);
                if (RefRow1 != null)
                  continue;
              }

              DataTable RefTable = queryTables[RefTableName];
              DataRow RefRow = RefTable.Rows.Find(RefId);
              if (RefRow == null)
                errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                  "\". Задана ссылка на документ/поддокумент \"" + RefTableName + "\" с Id=" + RefId.ToString() +
                  ", которого нет в базе данных");
              else if (DocProvider.DocTypes.UseDeleted)
              {
                if (DataTools.GetBool(RefRow, "Deleted"))
                  errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                    "\". Задана ссылка на документ/поддокумент \"" + RefTableName + "\" с Id=" + RefId.ToString() +
                    ", который в базе данных помечен как удаленный");
                else if (RefTable.Columns.Contains("DocId.Deleted"))
                {
                  if (DataTools.GetBool(RefRow, "DocId.Deleted"))
                    errors.AddError(prefix + ", Id=" + DataTools.GetInt(Row, "Id") + ", поле \"" + ts.Columns[i].ColumnName +
                      "\". Задана ссылка на поддокумент \"" + RefTableName + "\" с Id=" + RefId.ToString() +
                      ", документ которого с DocId=" + DataTools.GetInt(RefRow, "Docid") + " помечен как удаленный");
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
      get { return DataTools.GetBool(_DataSet.ExtendedProperties["IgnoreAllLocks"]); }
      set
      {
        if (UserActionId != 0)
          throw new InvalidOperationException("Нельзя устанавливать свойство IgnoreAllLocks после применения изменений");
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
    /// Добавляет полученнную блокировку в список IgnoredLocks
    /// </summary>
    /// <returns>Идентификатор блокировки</returns>
    public Guid AddLongLock()
    {
      DBxDocSelection DocSel = GetDocSelection(new DBxDocState[] { DBxDocState.Edit, DBxDocState.Delete, DBxDocState.Insert });
      return AddLongLock(DocSel);
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
      Guid LockGuid = DocProvider.AddLongLock(docSel);
      try
      {
        IgnoredLocks.Add(LockGuid);
        return LockGuid;
      }
      catch
      {
        DocProvider.RemoveLongLock(LockGuid);
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

      string MD5 = DataTools.MD5Sum(data);
      DataTable tblBinData = DataSet.Tables["BinData"];
      if (tblBinData != null)
      {
        // Выполняем поиск на случай, если метод вызван в процессе редактирования несколько раз для
        // одних и тех же данных
        int p = tblBinData.DefaultView.Find(MD5);
        if (p >= 0)
          return tblBinData.DefaultView[p].Row["Id"];
      }

      Int32 Id = DocProvider.InternalFindBinData(MD5);
      if (Id > 0)
        return Id; // на сервере уже есть такие данные

      tblBinData = GetReadyTableBinData();

      Id = (-tblBinData.Rows.Count) - 1; // -1, -2, ...
      tblBinData.Rows.Add(Id, MD5, data);
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

      IdList BinDataIds = new IdList();
      IdList FileNameIds = new IdList();

      #region Список используемых фиктивных идентификаторов

      foreach (DataTable Table in _DataSet.Tables)
      {
        if (Table.Rows.Count == 0)
          continue;

        DBxDocTypeBase dt = DocProvider.DocTypes.FindByTableName(Table.TableName);
        if (dt == null)
          continue;

        for (int i = 0; i < dt.BinDataRefs.Count; i++)
        {
          int p = Table.Columns.IndexOf(dt.BinDataRefs[i].Column.ColumnName);
#if DEBUG
          if (p < 0)
            throw new BugException("Не найден столбец \"" + dt.BinDataRefs[i].Column.ColumnName +
              "\" в таблице \"" + Table.TableName + "\", заданный в BinDataRefs");
#endif
          for (int j = 0; j < Table.Rows.Count; j++)
          {
            if (Table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 Id = DataTools.GetInt(Table.Rows[j][p]);
            if (Id < 0)
              BinDataIds.Add(Id);
          }
        }

        for (int i = 0; i < dt.FileRefs.Count; i++)
        {
          int p = Table.Columns.IndexOf(dt.FileRefs[i].Column.ColumnName);
#if DEBUG
          if (p < 0)
            throw new BugException("Не найден столбец \"" + dt.FileRefs[i].Column.ColumnName +
              "\" в таблице \"" + Table.TableName + "\", заданный в FileRefs");
#endif
          for (int j = 0; j < Table.Rows.Count; j++)
          {
            if (Table.Rows[j].RowState == DataRowState.Deleted)
              continue;
            Int32 Id = DataTools.GetInt(Table.Rows[j][p]);
            if (Id < 0)
              FileNameIds.Add(Id);
          }
        }
      }

      #endregion

      #region Список ссылок на данные в таблице файлов

      if (tblFileNames != null)
      {
        foreach (DataRow Row in tblFileNames.Rows)
        {
          Int32 DataId = DataTools.GetInt(Row, "Data");
          if (DataId < 0)
            BinDataIds.Add(DataId);
        }
      }

      #endregion

      #region Проверяем "потерянные" идентификаторы

      if (BinDataIds.Count > 0)
      {
        if (tblBinData == null)
          throw new BugException("В наборе есть ссылки на новые двоичные данные, но нет таблицы BinData");

        foreach (Int32 Id in BinDataIds)
        {
          if (tblBinData.Rows.Find(Id) == null)
            throw new BugException("В наборе есть ссылки на новые двоичные данные с Id=" + Id.ToString() + ", но в таблице BinData нет такой строки");
        }
      }

      if (FileNameIds.Count > 0)
      {
        if (tblFileNames == null)
          throw new BugException("В наборе есть ссылки на новые файлы, но нет таблицы FileNames");
        foreach (Int32 Id in FileNameIds)
        {
          if (tblFileNames.Rows.Find(Id) == null)
            throw new BugException("В наборе есть ссылки на новые файлы с Id=" + Id.ToString() + ", но в таблице FileNames нет такой строки");
        }
      }

      #endregion

      #region Удаление лишних строк

      if (tblBinData != null)
      {
        for (int i = tblBinData.Rows.Count - 1; i >= 0; i--)
        {
          Int32 Id = (Int32)(tblBinData.Rows[i][0]);
          if (!BinDataIds.Contains(Id))
            tblBinData.Rows.RemoveAt(i);
        }

        tblBinData.AcceptChanges();
      }

      if (tblFileNames != null)
      {
        for (int i = tblFileNames.Rows.Count - 1; i >= 0; i--)
        {
          Int32 Id = (Int32)(tblFileNames.Rows[i][0]);
          if (!FileNameIds.Contains(Id))
            tblFileNames.Rows.RemoveAt(i);
        }

        tblFileNames.AcceptChanges();
      }

      #endregion
    }

    internal byte[] InternalGetBinData(object binDataId, string docTypeName, Int32 docId, string subDocTypeName, Int32 subDocId, string columnName)
    {
      Int32 binDataId2 = DataTools.GetInt(binDataId);
      if (binDataId2 == 0)
        return null;

      #region Поиск во временной таблице

      if (binDataId2 < 0)
      {
        DataTable tblBinData = DataSet.Tables["BinData"];
        if (tblBinData == null)
          throw new InvalidOperationException("Запрошены двоичные данные с временным идентификатором, но таблицы BinData нет в наборе данных");

        DataRow Row = tblBinData.Rows.Find(binDataId2);
        if (Row == null)
          throw new InvalidOperationException("Запрошены двоичные данные для несуществующего временного идентификатора Id=" + binDataId2.ToString());
        return (byte[])(Row["Contents"]);
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

      Int32 DataId;
      string MD5 = DataTools.MD5Sum(file.Contents);

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
          int p = tblBinData.DefaultView.Find(MD5);
          if (p >= 0)
          {
            DataId = (Int32)(tblBinData.DefaultView[p].Row["Id"]);
            p = tblFileNames.DefaultView.Find(new object[] {
            file.FileInfo.Name, DataId, file.FileInfo.CreationTime, file.FileInfo.LastWriteTime});
            if (p >= 0)
              return tblFileNames.DefaultView[p].Row["Id"];
          }
        }
      }

      #endregion

      #region Обращение к серверу

      Int32 FileId = DocProvider.InternalFindDBFile(file.FileInfo, MD5, out DataId);
      if (FileId > 0)
        return FileId; // на сервере уже есть такие данные

      #endregion

      #region Добавление в BinData и FileNames

      if (DataId == 0)
      {
        DataTable tblBinData = GetReadyTableBinData();

        // Пытаемся найти данные еще раз.
        // Может устанавливаться новый файл но с содержимым, совпадающим с предыдущим вызовом 
        int p = tblBinData.DefaultView.Find(MD5);
        if (p >= 0)
          DataId = (Int32)(tblBinData.DefaultView[p].Row["Id"]);
        else
        {
          DataId = (-tblBinData.Rows.Count) - 1; // -1, -2, ...
          tblBinData.Rows.Add(DataId, MD5, file.Contents);
        }
      }

      tblFileNames = GetReadyTableFileNames();

      FileId = (-tblFileNames.Rows.Count) - 1; // -1, -2, ...
      tblFileNames.Rows.Add(FileId, file.FileInfo.Name, DataId,
        file.FileInfo.CreationTime, file.FileInfo.LastWriteTime,
        file.Contents.Length);

      #endregion

      return FileId;
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
      Int32 fileId2 = DataTools.GetInt(fileId);
      if (fileId2 == 0)
        return FreeLibSet.IO.StoredFileInfo.Empty;

      #region Поиск во временной таблице

      if (fileId2 < 0)
      {
        DataTable tblFileNames = DataSet.Tables["FileNames"];
        if (tblFileNames == null)
          throw new InvalidOperationException("Запрошен файл с временным идентификатором, но таблицы FileNames нет в наборе данных");

        DataRow Row = tblFileNames.Rows.Find(fileId2);
        if (Row == null)
          throw new InvalidOperationException("Запрошен файл для несуществующего временного идентификатора Id=" + fileId2.ToString());
        return new FreeLibSet.IO.StoredFileInfo((string)(Row["Name"]),
          (int)(Row["Length"]), DataTools.GetNullableDateTime(Row, "CreationTime"), DataTools.GetNullableDateTime(Row, "LastWriteTime"));
      }

      #endregion

      return DocProvider.GetDBFileInfo(fileId2);
    }

    internal FreeLibSet.IO.FileContainer InternalGetDBFile(object fileId, string docTypeName, Int32 docId, string subDocTypeName, Int32 subDocId, string columnName)
    {
      Int32 fileId2 = DataTools.GetInt(fileId);
      if (fileId2 == 0)
        return null;

      #region Поиск во временной таблице

      if (fileId2 < 0)
      {
        DataTable tblFileNames = DataSet.Tables["FileNames"];
        if (tblFileNames == null)
          throw new InvalidOperationException("Запрошен файл с временным идентификатором, но таблицы FileNames нет в наборе данных");

        DataRow Row = tblFileNames.Rows.Find(fileId2);
        if (Row == null)
          throw new InvalidOperationException("Запрошен файл для несуществующего временного идентификатора Id=" + fileId2.ToString());
        FreeLibSet.IO.StoredFileInfo FileInfo = new FreeLibSet.IO.StoredFileInfo((string)(Row["Name"]),
          (int)(Row["Length"]), DataTools.GetNullableDateTime(Row, "CreationTime"), DataTools.GetNullableDateTime(Row, "LastWriteTime"));

        Int32 DataId = (Int32)(Row["Data"]);
        // DataId может быть и фиктивным идентификатором, и реальным
        // Если DataId>0, то все равно придется обращаться к серверу
        byte[] Contents = InternalGetBinData(DataId, docTypeName, docId, subDocTypeName, subDocId, columnName);
        return new FreeLibSet.IO.FileContainer(FileInfo, Contents);
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
      IdList tempIds = new IdList();
      foreach (DataRow row in _DataSet.Tables[tableName].Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;

        Int32 binDataId = DataTools.GetInt(row, columnName);
        if (binDataId <= 0)
          continue;
        if (tempIds.Contains(binDataId))
          continue; // уже есть

        Int32 docId, subDocId;
        if (isSubDoc)
        {
          docId = DataTools.GetInt(row, "DocId");
          subDocId = DataTools.GetInt(row, "Id");
        }
        else
        {
          docId = DataTools.GetInt(row, "Id");
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
        default: throw new BugException("Строка таблицы документов находится в недопустимом состоянии RowState=" + dataRow.RowState.ToString());
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
