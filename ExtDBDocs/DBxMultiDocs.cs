// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Коллекция документов одного вида
  /// Для доступа к этой коллекции используется индексированное свойство DBxDocSet[]
  /// При переборе документов могут встречаться удаленные документы
  /// </summary>
  public sealed class DBxMultiDocs : IObjectWithCode, IEnumerable<DBxSingleDoc>
  {
    #region Защищенный конструктор

    internal DBxMultiDocs(DBxDocSet docSet, DBxDocType docType)
    {
      if (docSet == null)
        throw new ArgumentNullException("docSet");
      if (docType == null)
        throw new ArgumentNullException("docType");

      _DocType = docType;
      _DocSet = docSet;

      InitTables();

      _LastFictiveId = 0;
    }

    /// <summary>
    /// Метод также вызывается после ApplyChanges()
    /// </summary>
    internal void InitTables()
    {
      if (DocSet.DataSet.Tables.Contains(DocType.Name))
        _Table = DocSet.DataSet.Tables[DocType.Name];
      else
      {
        _Table = DocProvider.GetTemplate(DocType.Name, null);
        DataTools.SetPrimaryKey(_Table, "Id");
        //CorrectTableProps(FTable);
        if (!DocSet.DataSet.Tables.Contains(DocType.Name)) // 22.01.2019 - проверка реентрабельности
          DocSet.DataSet.Tables.Add(_Table);
      }

      if (_DocValues == null)
        _DocValues = new MultiDocValues(this);
      else
        _DocValues.Table = _Table;

      ResetDocIds(); // 20.01.2016

      foreach (DBxSubDocType sdt in DocType.SubDocs)
      {
        if (DocSet.DataSet.Tables.Contains(sdt.Name))
        {
          //int x = DocSet.DataSet.Tables["ПакетыФормФормы"].Rows.Count;

          DBxMultiSubDocs dummy = SubDocs[sdt.Name];
          //int n = Dummy.SubDocsView.Table.Rows.Count;
          //       DataRow r = Dummy.SubDocsView.Table.Rows[0];
        }
      }

      SubDocs.InitTables();
    }

    /*
    /// <summary>
    /// В таблице нельзя использовать свойство DataColumn
    /// </summary>
    /// <param name="Table"></param>
    private static void CorrectTableProps(DataTable Table)
    {
      for (int i = 0; i < Table.Columns.Count; i++)
      { 
        if (Table.Columns[i].AllowDBNull)
      }
    }
     * */

    #endregion

    #region Свойства

    /// <summary>
    /// Набор, к которому относится коллекция объектов
    /// </summary>
    public DBxDocSet DocSet { get { return _DocSet; } }
    private readonly DBxDocSet _DocSet;

    /// <summary>
    /// Возвращает DocSet.DocProvider
    /// </summary>
    public DBxDocProvider DocProvider { get { return DocSet.DocProvider; } }

    /// <summary>
    /// Возвращает описание вида документов
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private readonly DBxDocType _DocType;

    /// <summary>
    /// Количество открытых документов. Документы в списке на удаление не учитываются
    /// </summary>
    public int DocCount { get { return _Table.Rows.Count; } }

    /// <summary>
    /// Основная таблица данных, принадлежащая набору DBxDocSet.DataSet.
    /// Содержит данные для документов в режимах Insert, Edit и View
    /// Таблица содержит первичный ключ по полю Id
    /// При изменении таблицы должен вызываться метод FDocValues.ResetBuffer() или создаваться новый
    /// FDocValues, если присоединяется новая таблица
    /// </summary>
    internal DataTable Table { get { return _Table; } }
    private DataTable _Table;

    /// <summary>
    /// Возвращает права доступа к таблице документов
    /// </summary>
    public DBxAccessMode Permissions
    {
      get
      {
        return DocSet.DocProvider.DBPermissions.TableModes[DocType.Name];
      }
    }

    /// <summary>
    /// Текстовое представление, содержащаее число документов и их состояние
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DocType.PluralTitle);
      sb.Append(", DocCount=");
      sb.Append(DocCount.ToString());
      sb.Append(", DocState=");
      sb.Append(DocState.ToString());
      return sb.ToString();
    }

    #endregion

    #region Состояние документов

#if XXX
    /// <summary>
    /// Количество документов, помеченных на удаление
    /// </summary>
    public int DelDocCount
    {
      get
      {
        if (FDelTable == null)
          return 0;
        else
          return FDelTable.Rows.Count;
      }
    }

    /// <summary>
    /// Возвращает true, если документ с заданным идентификатором помечен на удаление
    /// </summary>
    /// <param name="DocId"></param>
    /// <returns></returns>
    public bool ContainsDelDocId(Int32 DocId)
    {
      if (FDelTable == null)
        return false;
      return FDelTable.Rows.Find(DocId) != null;
    }

    /// <summary>
    /// Возвращает массив идентификаторов документов, подлежащих удалению
    /// </summary>
    public Int32[] DelDocIds
    {
      get
      {
        if (FDelTable == null)
          return DataTools.EmptyIds;
        else
          return DataTools.GetIds(FDelTable);
      }
    }
#endif

    /// <summary>
    /// Получить количество документов с заданным состоянием
    /// </summary>
    /// <param name="state">Состояние документа (Insert, Edit, View или Delete)</param>
    /// <returns>Количество документов в заданном состоянии</returns>
    public int GetDocCount(DBxDocState state)
    {
      int cnt = 0;
      switch (state)
      {
        case DBxDocState.Insert:
          foreach (DataRow row in _Table.Rows)
          {
            if (row.RowState == DataRowState.Added)
              cnt++;
          }
          break;
        case DBxDocState.Edit:
          foreach (DataRow row in _Table.Rows)
          {
            if (row.RowState == DataRowState.Modified)
              cnt++;
          }
          break;
        case DBxDocState.View:
          foreach (DataRow row in _Table.Rows)
          {
            if (row.RowState == DataRowState.Unchanged)
              cnt++;
          }
          break;
        case DBxDocState.Delete:
          foreach (DataRow row in _Table.Rows)
          {
            if (row.RowState == DataRowState.Deleted)
              cnt++;
          }
          break;
        default:
          throw ExceptionFactory.ArgUnknownValue("state", state);
      }
      return cnt;
    }

    /// <summary>
    /// Состояния документов
    /// </summary>
    public DBxDocState DocState
    {
      get
      {
        if (_Table.Rows.Count == 0)
          return DBxDocState.None;

        DBxDocState res1 = DBxDocSet.GetDocState(_Table.Rows[0]);
        for (int i = 0; i < _Table.Rows.Count; i++)
        {
          DBxDocState res2 = DBxDocSet.GetDocState(_Table.Rows[i]);
          if (res2 != res1)
            return DBxDocState.Mixed;
        }
        return res1;
      }
    }


    /// <summary>
    /// Состояния документов. Документы в состоянии View пропускаются
    /// </summary>
    public DBxDocState DocStateNoView
    {
      get
      {
        DBxDocState res1 = DBxDocState.None;
        for (int i = 0; i < _Table.Rows.Count; i++)
        {
          DBxDocState res2 = DBxDocSet.GetDocState(_Table.Rows[i]);
          if (res2 == DBxDocState.View)
            continue;
          if (res1 == DBxDocState.None)
            res1 = res2;
          else
          {
            if (res2 != res1)
              return DBxDocState.Mixed;
          }
        }
        return res1;
      }
    }

    /// <summary>
    /// Возвращает true, если для этого типа документов нет ни одного документа
    /// </summary>
    public bool IsEmpty
    {
      get { return DocCount == 0; }
    }

    /// <summary>
    /// Возвращает true, если есть документы в состоянии Insert, Edit или Delete
    /// </summary>
    public bool ContainsModified
    {
      get
      {
        switch (DocState)
        {
          case DBxDocState.Insert:
          case DBxDocState.Edit:
          case DBxDocState.Delete:
          case DBxDocState.Mixed:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Изменяет документы, находящиеся в состоянии <paramref name="oldState"/> на состояние <paramref name="newState"/>
    /// </summary>
    /// <param name="oldState">Текущее состояние документов</param>
    /// <param name="newState">Новое состояние документов</param>
    /// <returns>Количество документов, состояние которых изменено</returns>
    public int ChangeDocState(DBxDocState oldState, DBxDocState newState)
    {
      if (newState == oldState)
        return 0;

      int cnt = 0;
      foreach (DBxSingleDoc doc in this)
      {
        if (doc.DocState == oldState)
        {
          switch (newState)
          {
            case DBxDocState.View:
              doc.View();
              break;
            case DBxDocState.Edit:
              doc.Edit();
              break;
            case DBxDocState.Delete:
              doc.Delete();
              break;
            default:
              throw ExceptionFactory.ArgUnknownValue("newState", newState, new object[] {
              DBxDocState.View, DBxDocState.Edit, DBxDocState.Delete});
          }

          cnt++;
        }
      }

      return cnt;
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает выборку из документов одного вида.
    /// В выборку входят все документы, кроме удаленных и еще не сохраненных новых документов с 
    /// фиктивными идентификаторами.
    /// </summary>
    public DBxDocSelection DocSelection
    {
      get
      {
        DBxDocSelection docSel = new DBxDocSelection(DocSet.DocProvider.DBIdentity);
        AddToDocSel(docSel);
        return docSel;
      }
    }

    internal void AddToDocSel(DBxDocSelection docSel)
    {
      foreach (DataRow row in _Table.Rows)
      {
        switch (row.RowState)
        {
          case DataRowState.Modified:
          case DataRowState.Unchanged:
            Int32 docId = DataTools.GetInt32(row, "Id");
            docSel.Add(DocType.Name, docId);
            break;
        }
      }
    }

    #endregion

    #region Доступ к одиночным документам

    /// <summary>
    /// Доступ к одиночному документу по индексу.
    /// </summary>
    /// <param name="docIndex">Индекс документа в пределах от 0 до (DocCount-1)</param>
    /// <returns>Объект для доступа к одиночному документу</returns>
    public DBxSingleDoc this[int docIndex]
    {
      get
      {
        return new DBxSingleDoc(this, docIndex);
      }
    }

#if XXX // пока не нужен
    /// <summary>
    /// Доступ к одиночному документу по строке данных.
    /// </summary>
    /// <param name="Row">Строка из таблицы Table</param>
    /// <returns>Объект для доступа к одиночному документу</returns>
    internal DBxSingleDoc this[DataRow Row]
    {
      get
      {
        int p = FTable.Rows.IndexOf(Row);
        if (p < 0)
        {
          if (Row == null)
            throw new ArgumentNullException("Row");
          if (Row.Table != FTable)
            throw new ArgumentException("Строка относится к другой таблице", "Row");
          else
            throw new ArgumentException("Строка не найдена в таблице");
        }
        return new DBxSingleDoc(this, p);
      }
    }
#endif


    /*
    public DBxSingleDoc SingleDoc
    {
      get
      {
        if (Count == 1)
          return this[0];
        return null; // ?? выбрасывать исключение ??
      }
    }
    */


    /// <summary>
    /// Поиск документа по идентификатору.
    /// Если нет документа с таким идентификатором, или <paramref name="docId"/>=0, то возвращается false.
    /// </summary>
    /// <param name="docId">Идентификатор искомого документа</param>
    /// <param name="doc">Сюда записывается найденный документ</param>
    /// <returns>True, если документ найден</returns>
    public bool TryGetDocById(Int32 docId, out DBxSingleDoc doc)
    {
      int p = IndexOfDocId(docId);
      if (p < 0)
      {
        doc = new DBxSingleDoc();
        return false;
      }
      else
      {
        doc = this[p];
        return true;
      }
    }

    /// <summary>
    /// Поиск документа по идентификатору.
    /// Если нет документа с таким идентификатором, или <paramref name="docId"/>=0, то выбрасывается исключение.
    /// </summary>
    /// <param name="docId">Идентификатор искомого документа</param>
    /// <returns>Найденный документ.
    /// Структура не может быть неинициализированной</returns>
    public DBxSingleDoc GetDocById(Int32 docId)
    {
      int p = IndexOfDocId(docId);
      if (p < 0)
        throw new ArgumentException(String.Format(Res.DBxMultiDocs_Err_DocNotFound, DocType.SingularTitle, docId));
      else
        return this[p];
    }

    /// <summary>
    /// Поиск документа с заданным идентификатором <paramref name="docId"/>.
    /// Возвращает индекс документа в <see cref="DocIds"/> или (-1), если такого документа нет
    /// или <paramref name="docId"/>=0.
    /// </summary>
    /// <param name="docId">Идентифккатор документа</param>
    /// <returns>Индекс документа</returns>
    public int IndexOfDocId(Int32 docId)
    {
      if (docId == 0)
        return -1;
#if DEBUG
      if (DocIds.Count != DocCount)
        throw new BugException("DocIds length is wrong");
#endif

      if (DocIds.Count <= 3)
        return DocIds.IndexOf(docId);
      else
      {
        if (_DocIdIndexer == null)
          _DocIdIndexer = new ArrayIndexer<Int32>(DocIds);
        return _DocIdIndexer.IndexOf(docId);
      }
    }

    /// <summary>
    /// Индексатор к массиву DocIds.
    /// Используется в IndexOfDocId()
    /// </summary>
    private ArrayIndexer<Int32> _DocIdIndexer; // 21.07.2020

    internal DataRow GetDocRow(int docIndex)
    {
      return _Table.Rows[docIndex];
    }

    #endregion

    #region Массив идентификаторов документов

    /// <summary>
    /// Список идентификаторов документов в режиме просмотра и редактирование
    /// Также доступен в режиме добавления после вызова <see cref="DBxDocProvider.ApplyChanges(DataSet, bool)"/>.
    /// Если есть новые документы, которые не были сохранены, в массиве будут присутствовать фиктивные идентификаторы.
    /// </summary>
    public IdList<Int32> DocIds
    {
      get
      {
        if (_DocIds == null)
          _DocIds = (IdList<Int32>)(IdTools.GetIdsFromColumn<Int32>(_Table, DBSDocType.Id, true));
        return _DocIds;
      }
    }
    private IdList<Int32> _DocIds;

    internal void ResetDocIds()
    {
      _DocIds = null;
      _DocIdIndexer = null;
      if (_SubDocs != null)
        _SubDocs.ResetTables();
    }

    /// <summary>
    /// Получить массив идентификаторов документа, находящихся в заданном состоянии.
    /// Если нет ни одного документа в заданном состоянии, возвращается пустой массив.
    /// Метод может вернуть фиктивные идентификаторы при <paramref name="docState"/>=Insert.
    /// </summary>
    /// <param name="docState">Состояние документов</param>
    /// <returns>Массив идентификаторов документов</returns>
    public IIdSet<Int32> GetDocIds(DBxDocState docState)
    {
      IdList<Int32> lst = null;
      foreach (DataRow row in Table.Rows)
      {
        if (DBxDocSet.GetDocState(row) == docState)
        {
          if (lst == null)
            lst = new IdList<Int32>();

          lst.Add((Int32)DBxDocSet.GetValue(row, "Id"));
        }
      }

      if (lst == null)
        return IdList<Int32>.Empty;
      else
        return lst;
    }

    #endregion

    #region Групповой доступ к значениям документам

    private class MultiDocValues : DBxDataTableExtValues,
      IDBxExtValues, // переопределяем некоторые методы интерфейса
      IDBxBinDataExtValues
    {
      #region Конструктор

      public MultiDocValues(DBxMultiDocs multiDocs)
        : base(multiDocs._Table, multiDocs.ColumnNameIndexer)
      {
        _MultiDocs = multiDocs;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Объект-владелец
      /// Нельзя использовать прямую ссылку на Table, т.к. она может меняться
      /// </summary>
      private DBxMultiDocs _MultiDocs;

      #endregion

      #region IDBxExtValues Members

      /// <summary>
      /// Возвращает true, если: 
      /// - нет открытых документов
      /// - есть хотя бы один документ в состоянии <see cref="DBxDocState.View"/>.
      /// </summary>
      public new bool IsReadOnly
      {
        get
        {
          if (_MultiDocs._Table.Rows.Count == 0)
            return false;

          foreach (DataRow row in _MultiDocs._Table.Rows)
          {
            switch (row.RowState)
            {
              case DataRowState.Unchanged:
              case DataRowState.Deleted:
                return true;
            }
          }
          return false;
        }
      }

      public new void CheckNotReadOnly()
      {
        if (IsReadOnly)
          throw new ObjectReadOnlyException(Res.DBxMultiDocs_Err_IsReadOnly);
      }

      private DataColumn GetColumnDef(int index)
      {
        //return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, this[index].Name);
        return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, index);
      }

      public new bool GetValueReadOnly(int index)
      {
        //лишнее
        //if (IsReadOnly)
        //  return true;

        if (index < _MultiDocs.DocProvider.MainDocTableServiceColumns.Count)
          return true; // Id и Deleted

        return GetColumnDef(index).ReadOnly;
      }

      public new bool AllowDBNull(int index)
      {
        return GetColumnDef(index).AllowDBNull;
      }

      public new int MaxLength(int index)
      {
        return GetColumnDef(index).MaxLength;
      }

      #endregion

      #region IDBxBinDataDocValues Members

      public byte[] GetBinData(int index)
      {
        return _MultiDocs.DocSet.InternalGetBinData(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetFirstId(), null, 0, GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFile(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetFirstId(), null, 0, GetName(index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetFirstId(), null, 0, GetName(index));
      }

      public void SetDBFile(int index, FreeLibSet.IO.FileContainer file)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetFile(file));
      }

      private Int32 GetFirstId()
      {
        if (_MultiDocs.Table.Rows.Count == 0)
          return -1;
        else
          return (Int32)(DBxDocSet.GetValue(_MultiDocs.Table.Rows[0], "Id"));
      }

      #endregion
    }

    /// <summary>
    /// Доступ к "серым" значениям полей документов.
    /// Инициализируется при изменении <see cref="Table"/>.
    /// </summary>
    public IDBxExtValues Values { get { return _DocValues; } }
    private MultiDocValues _DocValues;

    internal void ResetValueBuffer()
    {
      _DocValues.ResetBuffer();
      //FDocValues.IsReadOnly = IsReadOnly;
    }

    internal void ResetValueBuffer(int index)
    {
      _DocValues.ResetBuffer(index);
    }

    /// <summary>
    /// Значения флажков AllowDBNull для оптимизации IDBxExtValues
    /// </summary>
    internal bool[] AllowDBNullFlags
    {
      get
      {
        if (_AllowDBNullFlags == null)
          InitAllowDBNullFlags();
        return _AllowDBNullFlags;
      }
    }
    private bool[] _AllowDBNullFlags;

    private void InitAllowDBNullFlags()
    {
      bool[] a = new bool[_Table.Columns.Count];
      for (int i = 0; i < _Table.Columns.Count; i++)
        a[i] = DataTools.GetBoolean(_Table.Columns[i].ExtendedProperties["AllowDBNull"]);
      _AllowDBNullFlags = a;
    }

    #endregion

    #region Доступ к значениям штучных документов

    /// <summary>
    /// Доступ к значениям полей одного документа
    /// Также, как и DBxSingleDoc, содержит ссылку на DBxMultiDocs и номер строки в таблице.
    /// Поэтому, можно не пересоздавать объекты при смене таблицы
    /// В отличие от DBxSingleDoc, нет смысла делать структурой, т.к. наружу выдается только интерфейс
    /// </summary>
    private class SingleDocValues : IDBxExtValues, IDBxBinDataExtValues
    {
      #region Защищенный конструктор

      /// <summary>
      /// Создает объект доступа
      /// </summary>
      /// <param name="multiDocs">Объект доступа к однотипным документам</param>
      /// <param name="rowIndex">Индекс строки документа в таблице DBxMultiDocs.Table</param>
      /// <param name="rowVersion">Версия данных строки таблицы (текущая или оригинальная</param>
      public SingleDocValues(DBxMultiDocs multiDocs, int rowIndex, DataRowVersion rowVersion)
      {
        _MultiDocs = multiDocs;
        _RowIndex = rowIndex;
        _RowVersion = rowVersion;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Строка документа
      /// </summary>
      public DataRow Row { get { return _MultiDocs.Table.Rows[_RowIndex]; } }
      private int _RowIndex;

      /// <summary>
      /// Объект-владелец
      /// </summary>
      private DBxMultiDocs _MultiDocs;

      /// <summary>
      /// Версия данных для извлечения значений
      /// </summary>
      private DataRowVersion _RowVersion;

      #endregion

      #region IDBxExtValues Members

      public DBxExtValue this[string name]
      {
        get
        {
          int colIndex = _MultiDocs.ColumnNameIndexer.IndexOf(name);
          if (colIndex < 0)
            throw new ArgumentException(String.Format(Res.DBxMultiDocs_Arg_UnknownColumn, name, _MultiDocs.DocType.SingularTitle), "name");
          return new DBxExtValue(this, colIndex);
        }
      }

      public string GetName(int index)
      {
        return _MultiDocs.Table.Columns[index].ColumnName;
      }

      public string GetDisplayName(int index)
      {
        string displayName = _MultiDocs.Table.Columns[index].Caption;
        if (String.IsNullOrEmpty(displayName))
          return GetName(index);
        else
          return displayName;
      }

      public int IndexOf(string name)
      {
        return _MultiDocs.ColumnNameIndexer.IndexOf(name);
      }

      public DBxExtValue this[int index]
      {
        get { return new DBxExtValue(this, index); }
      }

      public int Count
      {
        get { return _MultiDocs.Table.Columns.Count; }
      }

      public int RowCount { get { return 1; } }

      public bool IsReadOnly
      {
        get
        {
          if (_RowVersion == DataRowVersion.Original)
            return true;

          switch (this.Row.RowState)
          {
            case DataRowState.Unchanged:
            case DataRowState.Deleted:
              return true; // 15.02.2022
            default:
              return false;
          }
        }
      }

      public object GetValue(int index, DBxExtValuePreferredType preferredType)
      {
        if (_RowVersion == DataRowVersion.Original)
          return Row[index, DataRowVersion.Original];
        else
          return Row[index];
      }

      public void SetValue(int index, object value)
      {
        _MultiDocs.ResetValueBuffer(index);
        if (value == null)
          Row[index] = DBNull.Value; // 14.10.2015
        else
          Row[index] = value;
      }

      public bool IsNull(int index)
      {
        if (_RowVersion == DataRowVersion.Original)
        {
          return Row.IsNull(_MultiDocs._Table.Columns[index], DataRowVersion.Original);
        }
        else
          return Row.IsNull(index);
      }

      private DataColumn GetColumnDef(int index)
      {
        //return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, this[index].Name);
        return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, index);
      }

      public bool AllowDBNull(int index)
      {
        return GetColumnDef(index).AllowDBNull;
      }

      public int MaxLength(int index)
      {
        return GetColumnDef(index).MaxLength;
      }

      public bool GetValueReadOnly(int index)
      {
        // лишнее
        //if (IsReadOnly)
        //  return true;

        if (index < _MultiDocs.DocProvider.MainDocTableServiceColumns.Count)
          return true; // Id и Deleted

        return GetColumnDef(index).ReadOnly;
      }

      bool IDBxExtValues.GetGrayed(int index)
      {
        return false;
      }

      object[] IDBxExtValues.GetValueArray(int index)
      {
        return new object[1] { GetValue(index, DBxExtValuePreferredType.Unknown) };
      }

      void IDBxExtValues.SetValueArray(int index, object[] values)
      {
        if (values.Length != 1)
          throw new ArgumentException("values.Length must be 1", "values");

        SetValue(index, values[0]);
      }

      object IDBxExtValues.GetRowValue(int valueIndex, int rowIndex)
      {
        if (rowIndex != 0)
          throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
        return ((IDBxExtValues)this).GetValue(valueIndex, DBxExtValuePreferredType.Unknown);
      }

      void IDBxExtValues.SetRowValue(int valueIndex, int rowIndex, object value)
      {
        if (rowIndex != 0)
          throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
        ((IDBxExtValues)this).SetValue(valueIndex, value);
      }

      #endregion

      #region IEnumerable<DBxExtValue> Members

      /// <summary>
      /// Возвращает перечислитель по значениям
      /// </summary>
      /// <returns></returns>
      public DBxExtValueEnumerator GetEnumerator()
      {
        return new DBxExtValueEnumerator(this);
      }

      IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
      {
        return new DBxExtValueEnumerator(this);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new DBxExtValueEnumerator(this);
      }

      #endregion

      #region IReadOnlyObject Members

      public void CheckNotReadOnly()
      {
        if (IsReadOnly)
          throw new ObjectReadOnlyException();
      }

      #endregion

      #region IDBxBinDataDocValues Members

      public byte[] GetBinData(int index)
      {
        return _MultiDocs.DocSet.InternalGetBinData(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetId(), null, 0, GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFile(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetId(), null, 0, GetName(index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetId(), null, 0, GetName(index));
      }

      public void SetDBFile(int index, FreeLibSet.IO.FileContainer file)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetFile(file));
      }

      private Int32 GetId()
      {
        return (Int32)(DBxDocSet.GetValue(Row, "Id"));
      }

      #endregion
    }

    /// <summary>
    /// Реализация свойства DBxSingleDoc.Values
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    internal IDBxExtValues GetSingleDocValues(int rowIndex)
    {
      if (Table.Rows[rowIndex].RowState == DataRowState.Deleted)
        return GetSingleDocOriginalValues(rowIndex); // 14.09.2015

      if (_SingleDocValues == null)
        _SingleDocValues = new Dictionary<int, SingleDocValues>();

      SingleDocValues docValues;
      if (!_SingleDocValues.TryGetValue(rowIndex, out docValues))
      {
        docValues = new SingleDocValues(this, rowIndex, DataRowVersion.Current);
        _SingleDocValues.Add(rowIndex, docValues);
      }
      return docValues;
    }

    /// <summary>
    /// Хранилище объектов доступа к полям одиночных документов.
    /// Ключ - индекс строки документа в таблице Table
    /// Значение - объект доступа к значениям.
    /// </summary>
    private Dictionary<int, SingleDocValues> _SingleDocValues;

    /// <summary>
    /// Реализация свойства DBxSingleDoc.OriginalValues
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    internal IDBxExtValues GetSingleDocOriginalValues(int rowIndex)
    {
      if (_SingleDocOriginalValues == null)
        _SingleDocOriginalValues = new Dictionary<int, SingleDocValues>();

      SingleDocValues docValues;
      if (!_SingleDocOriginalValues.TryGetValue(rowIndex, out docValues))
      {
        docValues = new SingleDocValues(this, rowIndex, DataRowVersion.Original);
        _SingleDocOriginalValues.Add(rowIndex, docValues);
      }
      return docValues;
    }

    private Dictionary<int, SingleDocValues> _SingleDocOriginalValues;

    #endregion

    #region Поддокументы

    /// <summary>
    /// Коллекция видов поддокументов
    /// </summary>
    public DBxMultiDocsSubDocs SubDocs
    {
      get
      {
        if (_SubDocs == null)
          _SubDocs = new DBxMultiDocsSubDocs(this);
        return _SubDocs;
      }
    }
    private DBxMultiDocsSubDocs _SubDocs;

    #endregion

    #region DocActionId

    /// <summary>
    /// Получение DocActionId для документа
    /// </summary>
    /// <param name="docId">Идентификтор документа</param>
    /// <returns></returns>
    internal Int32 GetDocIdActionId(Int32 docId)
    {
      if (!DocSet.DocProvider.IsRealDocId(docId))
        return 0;

      return DataTools.GetInt32(_Table.ExtendedProperties["DocActionId" + docId.ToString()]);
    }

    internal void SetDocIdActionId(Int32 docId, Int32 actionId)
    {
#if DEBUG
      DocSet.DocProvider.CheckIsRealDocId(docId);
#endif

      _Table.ExtendedProperties["DocActionId" + docId.ToString()] = actionId.ToString();
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return _DocType.Name; } }

    #endregion

    #region Методы изменения состояния документов

    #region View()

    /// <summary>
    /// Добавляет в таблицу открытых документов документ с заданным идентификатором. 
    /// В списке ранее открытых документов не должно быть этого документа
    /// </summary>
    /// <param name="docId">Идентификатор добавляемого документа</param>
    public DBxSingleDoc View(Int32 docId)
    {
      View(IdArray<Int32>.FromId(docId));
      //return this[DocCount - 1];
      return GetDocById(docId); // 15.10.2015
    }

    /// <summary>
    /// Открывает документ с заданными идентификаторами на просмотр и добавляет его в список открытых
    /// документов В списке ранее открытых документов не должно быть документов с этим идентификатором
    /// </summary>
    /// <param name="docIds">Массив идентификаторов</param>
    public void View(IEnumerable<Int32> docIds)
    {
      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);
      if (docIds2.Count < 1)
        throw ExceptionFactory.ArgIsEmpty("docIds");
      foreach (Int32 docId in docIds2)
      {
        DocProvider.CheckIsRealDocId(docId); 
      }

      // 15.10.2015 CheckNotInTable(DocIds);

      ResetDocIds();

      DataTable table2 = DocProvider.LoadDocData(DocType.Name, docIds2);
      DoView(table2);
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    public void View(DBxFilter filter)
    {
      DataTable table2 = DocProvider.LoadDocData(DocType.Name, filter);
      // 15.10.2015 CheckNotInTable(DataTools.GetIds(Table2));

      DoView(table2);
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// Эта версия дополнительно возвращает массив идентификаторов документов, которые проходят условие фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    /// <param name="docIds">Массив добавленных документов</param>
    public void View(DBxFilter filter, out IIdSet<Int32> docIds)
    {
      DataTable table2 = DocProvider.LoadDocData(DocType.Name, filter);
      docIds = IdTools.GetIdsFromColumn<Int32>(table2, DBSDocType.Id, true);

      DoView(table2);
    }

    private void DoView(DataTable table2)
    {
      if (Permissions == DBxAccessMode.None)
        throw new DBxAccessException(String.Format(Res.DBxMultiDocs_Err_AccessDenied, DocType.PluralTitle));

      int oldDocCount = DocCount;

      if (_Table.Rows.Count == 0)
      {
        // Заменяем старую таблицу на новую
        DataSet ds = _Table.DataSet;
        ds.Tables.Remove(_Table);
        ds.Tables.Add(table2);
        DataTools.SetPrimaryKey(table2, "Id");
        _Table = table2; // 
        _Table.AcceptChanges(); // переход в режим View
        _DocValues.Table = _Table;

        // 14.01.2022. Замечание:
        // В этот момент меняется структура таблицы. Свойства DataColumn.AllowDBNull и MaxLength могут быть не инициализированы правильно,
        // т.к. таблица является результатом выполнения SELECT(). Поэтому на них нельзя ориентироваться в DBxExtValue
      }
      else
      {
        // Добавляем строки в существующую таблицу
        // 15.10.2015
        // Пропускаем уже существующие документы
        _Table.BeginLoadData();
        try
        {
          for (int i = 0; i < table2.Rows.Count; i++)
          {
            Int32 DocId = (Int32)(table2.Rows[i]["Id"]);
            if (_Table.Rows.Find(DocId) == null)
            {
              DataRow dstRow = _Table.NewRow();
              DataTools.CopyRowValues(table2.Rows[i], dstRow, false);
              _Table.Rows.Add(dstRow);
              dstRow.AcceptChanges();
            }
          }
        }
        finally
        {
          _Table.EndLoadData();
          _DocValues.ResetBuffer();
        }
      }

      if (DocSet.UseTestDocument)
      {
        try
        {
          for (int i = oldDocCount; i < _Table.Rows.Count; i++)
            DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.View);
        }
        catch
        {
          // Убираем загруженные данные, иначе от выброса исключения мало толку
          if (oldDocCount == 0)
            _Table.Rows.Clear();
          else
          {
            for (int i = _Table.Rows.Count - 1; i >= oldDocCount; i--)
              _Table.Rows.Remove(_Table.Rows[i]);
          }
          throw;
        }
      }

#if XXX
#if DEBUG
      // 28.12.2017
      // Проверка наличия удаленных документов
      using (DataView dv2 = new DataView(FTable))
      {
        dv2.Sort = "Id";
        dv2.RowStateFilter = DataViewRowState.Deleted;

        foreach (DataRow Row in FTable.Rows)
        {
          if (Row.RowState == DataRowState.Deleted)
            continue;
          Int32 DocId = (Int32)(Row["Id"]);
          int p=dv2.Find(DocId);
          if (p >= 0)
          {
            throw new InvalidOperationException("Документ " + DocType.SingularTitle + " с идентификатором DocId=" + DocId.ToString() + " уже есть в наборе, но он был помечен на удаление. Повторное добавление этого же документа в набор не допускается");
          }
        }
      }
#endif
#endif
    }

    #endregion

    #region Edit()

    internal void CheckCanModify()
    {
      DocSet.CheckNotReadOnly();

      if (Permissions != DBxAccessMode.Full)
      {
        if (Permissions == DBxAccessMode.None)
          throw new DBxAccessException(String.Format(Res.DBxMultiDocs_Err_AccessDenied, DocType.PluralTitle));
        else
          throw new DBxAccessException(String.Format(Res.DBxMultiDocs_Err_AccessReadOnly, DocType.PluralTitle));
      }
    }

    /// <summary>
    /// Добавляет в таблицу открытых документов документы с заданными идентификаторами и переводит их состояние
    /// на редактирование. В списке ранее открытых документов не должно быть документов с этими идентификаторами
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public void Edit(IEnumerable<Int32> docIds)
    {
      CheckCanModify();

      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);

      //int OldDocCount = DocCount;
      View(docIds2);
      /*
      for (int i = OldDocCount; i < FTable.Rows.Count; i++)
      {
        DocProvider.DocPermissions.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeEdit);
        FTable.Rows[i].SetModified();
      }
       * */

      // 15.10.2015
      // Может быть, часть строк были уже открыты на просмотр или на редактирование
      SetEditState(docIds2);
    }

    private void SetEditState(IIdSet<Int32> docIds)
    {
      foreach (Int32 docId in docIds)
      {
        DataRow row = _Table.Rows.Find(docId);
        if (row == null)
          throw new BugException("DataRow is lost for DocId=" + docId.ToString());
        int rowIndex = _Table.Rows.IndexOf(row);
        DBxSingleDoc doc = new DBxSingleDoc(this, rowIndex);
        switch (row.RowState)
        {
          case DataRowState.Modified:
            break; // ничего не надо проверять
          case DataRowState.Unchanged:
            if (DocSet.UseTestDocument)
            {
              if (doc.Deleted)
                DocProvider.TestDocument(doc, DBxDocPermissionReason.BeforeRestore);
              DocProvider.TestDocument(doc, DBxDocPermissionReason.BeforeEdit);
            }
            row.SetModified();
            break;
          default:
            throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_SetDocState, 
              DocType.SingularTitle, docId, "Edit", doc.DocState));
        }
      }
    }

    /// <summary>
    /// Открывает документ с заданным идентификатором на редактирование и добавляет его в список открытых
    /// документов.
    /// В списке ранее открытых документов не должно быть документов с этим идентификатором.
    /// </summary>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Объект доступа к документу</returns>
    public DBxSingleDoc Edit(Int32 docId)
    {
      Edit(IdArray<Int32>.FromId(docId));
      //return this[DocCount - 1];
      return GetDocById(docId); // 15.10.2015
    }


    /// <summary>
    /// Открытие документов на редактирование с использованием фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    public void Edit(DBxFilter filter)
    {
      IIdSet<Int32> DocIds;
      Edit(filter, out DocIds);
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// Эта версия дополнительно возвращает массив идентификаторов документов, которые проходят условие фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    /// <param name="docIds">Массив идентификаторов добавленных документов</param>
    public void Edit(DBxFilter filter, out IIdSet<Int32> docIds)
    {
      View(filter, out docIds);
      SetEditState(docIds);
    }


    /// <summary>
    /// Переводит открытые на просмотр документы в режим редактирования. Не должно быть документов этого 
    /// типа в состоянии, отличном от View
    /// </summary>
    public void Edit()
    {
      if (DocState != DBxDocState.View)
      {
        if (DocCount == 0)
          throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_IsEmpty, DocType.PluralTitle));
        else
          throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_StateRequired,
            DocType.PluralTitle, "View"));
      }

      CheckCanModify();

      for (int i = 0; i < _Table.Rows.Count; i++)
      {
        DBxSingleDoc doc = new DBxSingleDoc(this, i);
        if (DocSet.UseTestDocument)
        {
          if (doc.Deleted)
            DocProvider.TestDocument(doc, DBxDocPermissionReason.BeforeRestore);
          DocProvider.TestDocument(doc, DBxDocPermissionReason.BeforeEdit);
        }
        _Table.Rows[i].SetModified();
      }
    }

    #endregion

    #region Insert()

    /// <summary>
    /// Возвращает следующий фиктивный идентификатор документа, используемый методами Insert
    /// </summary>
    internal Int32 NextFictiveId()
    {
      _LastFictiveId--;
      return _LastFictiveId;
    }
    private Int32 _LastFictiveId;

    /// <summary>
    /// Добавляет в таблицу документов один новый документ. Документ получает временный идентификатор,
    /// который будет заменен постоянным после вызова DBxDocSet.ApplyChanges()
    /// В списке могут быть другие документы в состоянии View, Edit и Insert
    /// </summary>
    /// <returns></returns>
    public DBxSingleDoc Insert()
    {
      CheckCanModify();

      DataRow row = _Table.NewRow();
      row["Id"] = NextFictiveId();
      _Table.Rows.Add(row);
      ResetDocIds(); // 08.07.2016
      return new DBxSingleDoc(this, _Table.Rows.Count - 1);
    }

    #endregion

    #region InsertCopy()

    /// <summary>
    /// Открывает для просмотра документы с заданными идентификаторами, заменяет идентификаторы на временные и
    /// добавляет документы в список открытых. Документы получают состояние Insert
    /// В списке могут быть другие документы в состоянии View, Edit и Insert
    /// </summary>
    public void InsertCopy(IEnumerable<Int32> docIds)
    {
      CheckCanModify();

      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);

      //int OldDocCount = DocCount;
      View(docIds2);

      DataSet tempDS = new DataSet();

      foreach (Int32 docId in docIds2)
        DoInsertCopy1(tempDS, docId);

      DBxDocSet.DoInsertCopy2(tempDS, Table.DataSet, DocSet.DocProvider);

      ResetDocIds();

#if DEBUG
      DocSet.CheckDataSet();
#endif
    }

    /// <summary>
    /// Открывает для просмотра документ с заданным идентификатором, заменяет идентификатор на временный и
    /// добавляет документ в список открытых. Документ получает состояние Insert
    /// В списке могут быть другие документы в состоянии View, Edit и Insert
    /// </summary>
    /// <param name="docId">Идентификатор оригинального документа</param>
    public DBxSingleDoc InsertCopy(Int32 docId)
    {
      InsertCopy(IdArray<Int32>.FromId(docId));
      //return this[DocCount - 1];
      return GetDocById(docId); // 15.10.2015
    }

    /// <summary>
    /// Переводит все открытые на просмотр документы в режим создания копии. В списке открытых документов должен
    /// быть один или несколько документов в режиме View. Наличие документов в других состояниях, кроме Delete, 
    /// не допускается
    /// </summary>
    public void InsertCopy()
    {
      if (DocState != DBxDocState.View)
      {
        if (DocCount == 0)
          throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_IsEmpty, DocType.PluralTitle));
        else
          throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_StateRequired,
            DocType.PluralTitle, "View"));
      }

      CheckCanModify();

      DataSet tempDS = new DataSet();
      IIdSet<Int32> viewDocIds = GetDocIds(DBxDocState.View);
      foreach(Int32 viewDocId in viewDocIds)
        DoInsertCopy1(tempDS, viewDocId);

      DBxDocSet.DoInsertCopy2(tempDS, Table.DataSet, DocSet.DocProvider);

      ResetDocIds();

#if DEBUG
      DocSet.CheckDataSet();
#endif
    }

    /// <summary>
    /// Первый шаг перехода в режим InsertCopy()
    /// </summary>
    /// <param name="tempDS">Таблицы замены идентификаторов</param>
    /// <param name="docId"></param>
    internal void DoInsertCopy1(DataSet tempDS, Int32 docId)
    {
      if (docId <= 0)
        throw new ArgumentException("DocId=" + docId.ToString(), "docId");

      DBxSingleDoc doc = GetDocById(docId);

      if (doc.DocState != DBxDocState.View)
        throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_SetDocState,
          DocType.SingularTitle, docId, "InsertCopy", doc.DocState));

      DBxDocSet.DoInsertCopy1(tempDS, DocType.Name, docId, NextFictiveId());

      // Перебираем для замены все поддокументы.
      // При необходимости, они подгружаются
      for (int i = 0; i < doc.SubDocs.Count; i++)
      {
        DBxSingleSubDocs subDocs1 = doc.SubDocs[i];
        foreach (DBxSubDoc subDoc in subDocs1)
        {
          DBxDocSet.DoInsertCopy1(tempDS, subDocs1.SubDocs.SubDocType.Name, subDoc.SubDocId, doc.SubDocs[i].SubDocs.NextFictiveId());
        }
      }

    }

    #endregion

    #region Delete()

    /// <summary>
    /// Помещает документы с заданными идентификаторами в список на удаление
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public void Delete(IEnumerable<Int32> docIds)
    {
      CheckCanModify();

      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);

      //if (docIds.Length < 1)
      //  throw new ArgumentException("Массив идентификаторов нулевой длины", "docIds");
      //for (int i = 0; i < docIds.Length; i++)
      //{
      //  if (!DocProvider.IsRealDocId(docIds[i]))
      //    throw new ArgumentException("В позиции " + i.ToString() + " задан недопустимый идентификатор документа " + docIds[i].ToString());
      //}

      CheckNotInTable(docIds2);

      ResetDocIds();

      int oldDocCount = DocCount;

      DataTable table2 = DocProvider.LoadDocData(DocType.Name, docIds2);
      DoView(table2);

      if (DocCount != oldDocCount + docIds2.Count)
        throw new BugException("Invalid DocCount");

      if (DocSet.UseTestDocument)
      {
        for (int i = oldDocCount; i < _Table.Rows.Count; i++)
          DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeDelete);
      }
      for (int i = oldDocCount; i < _Table.Rows.Count; i++)
        _Table.Rows[i].Delete();


      /*
       * 28.12.2017
       * Зачем этот блок был нужен?
       * 
      DataTable Table2 = DocProvider.LoadDocData(DocType.Name, DocIds);
      if (FTable.Rows.Count == 0)
      {
        // Заменяем старую таблицу на новую
        DataSet ds = FTable.DataSet;
        ds.Tables.Remove(FTable);
        ds.Tables.Add(Table2);
        DataTools.SetPrimaryKey(Table2, "Id");
        FTable = Table2;
        FDocValues.Table = FTable;
      }
      else
      {
        // Добавляем строки в существующую таблицу
        FTable.BeginLoadData();
        try
        {
          for (int i = 0; i < Table2.Rows.Count; i++)
          {
            DataRow DstRow = FTable.NewRow();
            DataTools.CopyRowValues(Table2.Rows[i], DstRow, false);
            FTable.Rows.Add(DstRow);
          }
        }
        finally
        {
          FTable.EndLoadData();
          FDocValues.ResetBuffer();
        }
      }
       * */

      _DocValues.ResetBuffer();
    }

    /// <summary>
    /// Помещает документ с заданным идентификатором в список на удаление
    /// </summary>
    /// <param name="docId">Идентификатор удаляемого документа</param>
    public void Delete(Int32 docId)
    {
      Delete(IdArray<Int32>.FromId(docId));
    }

    /// <summary>
    /// Перемещает все документы, находящиеся в режиме View, в список на удаление. 
    /// Документы, уже находящиеся в состоянии Delete, пропускаются.
    /// Наличие документов в режимах, отличных от View и Delete, не допускается.
    /// </summary>
    public void Delete()
    {
      CheckCanModify();

      List<DataRow> rowsToDelete = null;

      for (int i = 0; i < DocCount; i++)
      {
        DBxSingleDoc doc = new DBxSingleDoc(this, i);
        switch (doc.DocState)
        {
          case DBxDocState.Delete:
            break;
          case DBxDocState.View:
            if (DocSet.UseTestDocument)
              DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeDelete); // 28.01.2022
            if (rowsToDelete == null)
              rowsToDelete = new List<DataRow>();
            rowsToDelete.Add(_Table.Rows[i]);
            break;
          default:
            throw new InvalidOperationException(String.Format(Res.DBxMultiDocs_Err_SetDocState,
              DocType.Name, doc.DocId, "Delete", doc.DocState));
        }
      }

      if (rowsToDelete != null)
      {
        foreach (DataRow row in rowsToDelete)
          row.Delete();
        ResetDocIds();
      }
    }

    #endregion

    #region ClearList(), ClearView() /*, RemoveFromList() */

    /// <summary>
    /// Очищает список открытых документов и список на удаление без сохранения изменений
    /// </summary>
    public void ClearList()
    {
      _Table.Rows.Clear();
      if (_SubDocs != null)
        _SubDocs.ClearList();

      ResetDocIds();
    }

    /// <summary>
    /// Удаляет все документы, для которых выполняется просмотр.
    /// Позволяет уменьшить объем данных, передаваемых на сервер для сохранения изменений
    /// </summary>
    public void ClearView()
    {
      for (int i = _Table.Rows.Count - 1; i >= 0; i--)
      {
        if (_Table.Rows[i].RowState == DataRowState.Unchanged)
          _Table.Rows.RemoveAt(i);
      }

      ResetDocIds();
    }

    #endregion

    #region Проверочные методы

    private void CheckNotInTable(IIdSet<Int32> docIds)
    {
      foreach (Int32 docId in docIds)
      {
        if (_Table.Rows.Contains(docId))
          throw new InvalidOperationException(String.Format(Res.DBxDocProvider_Err_DocInDocSet, 
            DocType.SingularTitle, docId));
      }
    }

    private void CheckNotInTable(Int32 docId)
    {
      if (_Table.Rows.Contains(docId))
        throw new InvalidOperationException(String.Format(Res.DBxDocProvider_Err_DocInDocSet,
          DocType.SingularTitle, docId));
    }

    #endregion

    #endregion

    #region Проверка наличия изменений в документе

    /// <summary>
    /// Свойство возвращает true, если в списке документов есть добавленные/удаленные, или для документа в состоянии Edit есть измененные значения
    /// Также возвращается true, если есть какие-либо изменения в поддокументах.
    /// </summary>
    public bool IsDataModified
    {
      get
      {
        for (int i = 0; i < Table.Rows.Count; i++)
        {
          if (this[i].IsDataModified)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region Просмотр старых версий

    /// <summary>
    /// Открывает на просмотр документ с заданным номером версии
    /// </summary>
    /// <param name="docId">Идентификатор существующего документа</param>
    /// <param name="version">Версия документа (от 1 до значения поля Version в таблице документа)</param>
    /// <returns>Объект одиночного документа для доступа к значениям полей</returns>
    public DBxSingleDoc ViewVersion(Int32 docId, int version)
    {
      DocProvider.CheckIsRealDocId(docId);

      CheckNotInTable(docId);

      int oldDocCount = DocCount;

      ResetDocIds();

      DocSet.VersionView = true;

      DataTable table2 = DocProvider.LoadDocDataVersion(DocType.Name, docId, version);
      if (_Table.Rows.Count == 0)
      {
        // Заменяем старую таблицу на новую
        DataSet ds = _Table.DataSet;
        ds.Tables.Remove(_Table);
        ds.Tables.Add(table2);
        DataTools.SetPrimaryKey(table2, "Id");
        _Table = table2;
        _Table.AcceptChanges();
        _DocValues.Table = _Table;
      }
      else
      {
        // Добавляем строки в существующую таблицу
        _Table.BeginLoadData();
        try
        {
          for (int i = 0; i < table2.Rows.Count; i++)
          {
            DataRow dstRow = _Table.NewRow();
            DataTools.CopyRowValues(table2.Rows[i], dstRow, false);
            _Table.Rows.Add(dstRow);
            dstRow.AcceptChanges();
          }
        }
        finally
        {
          _Table.EndLoadData();
          _DocValues.ResetBuffer();
        }
      }

      // 28.01.2022 - Как в DoView()
      if (DocSet.UseTestDocument)
      {
        try
        {
          for (int i = oldDocCount; i < _Table.Rows.Count; i++)
            DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.View);
        }
        catch
        {
          // Убираем загруженные данные, иначе от выброса исключения мало толку
          if (oldDocCount == 0)
            _Table.Rows.Clear();
          else
          {
            for (int i = _Table.Rows.Count - 1; i >= oldDocCount; i--)
              _Table.Rows.Remove(_Table.Rows[i]);
          }
          throw;
        }
      }

      return this[DocCount - 1];
    }

    #endregion

    #region IEnumerable<DBxSingleDoc> Members

    /// <summary>
    /// Перечислитель однотипных документов
    /// </summary>
    public struct Enumerator : IEnumerator<DBxSingleDoc>
    {
      #region Конструктор

      internal Enumerator(DBxMultiDocs owner)
      {
        _Owner = owner;
        _DocIndex = -1;
      }

      #endregion

      #region Поля

      private readonly DBxMultiDocs _Owner;

      private int _DocIndex;

      #endregion

      #region IEnumerator<DBxSingleDoc> Members

      /// <summary>
      /// Текущий документ
      /// </summary>
      public DBxSingleDoc Current
      {
        get { return _Owner[_DocIndex]; }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Owner[_DocIndex]; }
      }

      /// <summary>
      /// Переход к следующему документу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _DocIndex++;
        return _DocIndex < _Owner.DocCount;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _DocIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по документам одного вида
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<DBxSingleDoc> IEnumerable<DBxSingleDoc>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Позиции столбцов в таблице

    /// <summary>
    /// Индексатор позиций столбцов в таблице данных документа
    /// </summary>
    internal StringArrayIndexer ColumnNameIndexer
    {
      get
      {
        if (_ColumnNameIndexer == null)
          _ColumnNameIndexer = DocProvider.GetColumnNameIndexer(DocType.Name);
        return _ColumnNameIndexer;
      }
    }
    private StringArrayIndexer _ColumnNameIndexer;

    #endregion
  }
}
