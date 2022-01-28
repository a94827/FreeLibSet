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

          DBxMultiSubDocs Dummy = SubDocs[sdt.Name];
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
    private DBxDocSet _DocSet;

    /// <summary>
    /// Возвращает DocSet.DocProvider
    /// </summary>
    public DBxDocProvider DocProvider { get { return DocSet.DocProvider; } }

    /// <summary>
    /// Возвращает описание вида документов
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private DBxDocType _DocType;

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
          foreach (DataRow Row in _Table.Rows)
          {
            if (Row.RowState == DataRowState.Added)
              cnt++;
          }
          break;
        case DBxDocState.Edit:
          foreach (DataRow Row in _Table.Rows)
          {
            if (Row.RowState == DataRowState.Modified)
              cnt++;
          }
          break;
        case DBxDocState.View:
          foreach (DataRow Row in _Table.Rows)
          {
            if (Row.RowState == DataRowState.Unchanged)
              cnt++;
          }
          break;
        case DBxDocState.Delete:
          foreach (DataRow Row in _Table.Rows)
          {
            if (Row.RowState == DataRowState.Deleted)
              cnt++;
          }
          break;
        default:
          throw new ArgumentException("Недопустимое значение state=" + state.ToString(), "state");
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

        DBxDocState Res1 = DBxDocSet.GetDocState(_Table.Rows[0]);
        for (int i = 0; i < _Table.Rows.Count; i++)
        {
          DBxDocState Res2 = DBxDocSet.GetDocState(_Table.Rows[i]);
          if (Res2 != Res1)
            return DBxDocState.Mixed;
        }
        return Res1;
      }
    }


    /// <summary>
    /// Состояния документов. Документы в состоянии View пропускаются
    /// </summary>
    public DBxDocState DocStateNoView
    {
      get
      {
        DBxDocState Res1 = DBxDocState.None;
        for (int i = 0; i < _Table.Rows.Count; i++)
        {
          DBxDocState Res2 = DBxDocSet.GetDocState(_Table.Rows[i]);
          if (Res2 == DBxDocState.View)
            continue;
          if (Res1 == DBxDocState.None)
            Res1 = Res2;
          else
          {
            if (Res2 != Res1)
              return DBxDocState.Mixed;
          }
        }
        return Res1;
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
      foreach (DBxSingleDoc Doc in this)
      {
        if (Doc.DocState == oldState)
        {
          switch (newState)
          {
            case DBxDocState.View:
              Doc.View();
              break;
            case DBxDocState.Edit:
              Doc.Edit();
              break;
            case DBxDocState.Delete:
              Doc.Delete();
              break;
            default:
              throw new ArgumentException("Нельзя переводить документы в состояние " + newState.ToString());
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
        DBxDocSelection DocSel = new DBxDocSelection(DocSet.DocProvider.DBIdentity);
        AddToDocSel(DocSel);
        return DocSel;
      }
    }

    internal void AddToDocSel(DBxDocSelection docSel)
    {
      foreach (DataRow Row in _Table.Rows)
      {
        switch (Row.RowState)
        {
          case DataRowState.Modified:
          case DataRowState.Unchanged:
            Int32 DocId = DataTools.GetInt(Row, "Id");
            docSel.Add(DocType.Name, DocId);
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
        throw new ArgumentException("Не найден документ \"" + DocType.SingularTitle + "\" с идентификатором " + docId.ToString());
      else
        return this[p];
    }

    /// <summary>
    /// Поиск документа с заданным идентификатором <paramref name="docId"/>.
    /// Возвращает индекс документа в DocIds или (-1), если такого документа нет
    /// или DocId=0
    /// </summary>
    /// <param name="docId">Идентифккатор документа</param>
    /// <returns>Индекс документа</returns>
    public int IndexOfDocId(Int32 docId)
    {
      if (docId == 0)
        return -1;
#if DEBUG
      if (DocIds.Length != DocCount)
        throw new BugException("Неправильная длина массива DocIds");
#endif

      if (DocIds.Length <= 3)
        return Array.IndexOf<Int32>(DocIds, docId);
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
    /// Также доступен в режиме добавления после вызова ApplyChanges().
    /// Если есть новые документы, которые не были сохранены, в массиве будут присутствовать фиктивные идентификаторы
    /// </summary>
    public Int32[] DocIds
    {
      get
      {
        if (_DocIds == null)
          _DocIds = DataTools.GetIds(_Table);
        return _DocIds;
      }
    }
    private Int32[] _DocIds;

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
    /// Метод может вернуть фиктивные идентификаторы при DocState=Insert.
    /// </summary>
    /// <param name="docState">Состояние документов</param>
    /// <returns>Массив идентификаторов документов</returns>
    public Int32[] GetDocIds(DBxDocState docState)
    {
      List<Int32> lst = null;
      foreach (DataRow Row in Table.Rows)
      {
        if (DBxDocSet.GetDocState(Row) == docState)
        {
          if (lst == null)
            lst = new List<Int32>();

          lst.Add((Int32)DBxDocSet.GetValue(Row, "Id"));
        }
      }

      if (lst == null)
        return DataTools.EmptyIds;
      else
        return lst.ToArray();
    }

    #endregion

    #region Групповой доступ к значениям документам

    private class MultiDocValues : DataTableDocValues,
      IDBxDocValues, // переопределяем некоторые методы интерфейса
      IDBxBinDataDocValues
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
      /// Нельзя использовать прямую ссылку на FTable, т.к. она может меняться
      /// </summary>
      private DBxMultiDocs _MultiDocs;

      #endregion

      #region IDBxDocValues Members

      /// <summary>
      /// Возвращает true, если: 
      /// - нет открытых документов
      /// - есть хотя бы один документ в состоянии View
      /// </summary>
      public new bool IsReadOnly
      {
        get
        {
          if (_MultiDocs._Table.Rows.Count == 0)
            return false;

          foreach (DataRow Row in _MultiDocs._Table.Rows)
          {
            switch (Row.RowState)
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
          throw new ObjectReadOnlyException("Редактирование значений не допускается");
      }

      private DataColumn GetColumnDef(int index)
      {
        //return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, this[index].Name);
        return _MultiDocs.DocProvider.GetColumnDef(_MultiDocs.DocType.Name, index);
      }

      public new bool GetValueReadOnly(int index)
      {
        if (IsReadOnly)
          return true;

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
        return _MultiDocs.DocSet.InternalGetBinData(GetValue(index, DBxDocValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetFirstId(), null, 0, GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFile(GetValue(index, DBxDocValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetFirstId(), null, 0, GetName(index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxDocValuePreferredType.Int32),
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
    /// Доступ к "серым" значениям полей документов
    /// Инициализируется при изменении FTable
    /// </summary>
    public IDBxDocValues Values { get { return _DocValues; } }
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
    /// Значения флажков AllowDBNull для оптимизации IDBxDocValues
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
        a[i] = DataTools.GetBool(_Table.Columns[i].ExtendedProperties["AllowDBNull"]);
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
    private class SingleDocValues : IDBxDocValues, IDBxBinDataDocValues
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

      #region IDBxDocValues Members

      public DBxDocValue this[string name]
      {
        get
        {
          int Index = _MultiDocs.ColumnNameIndexer.IndexOf(name);
          if (Index < 0)
            throw new ArgumentException("Поле \"" + name + "\" не принадлежит документу \"" + _MultiDocs.DocType.SingularTitle + "\"", "name");
          return new DBxDocValue(this, Index);
        }
      }

      public string GetName(int index)
      {
        return _MultiDocs.Table.Columns[index].ColumnName;
      }

      public string GetDisplayName(int index)
      {
        string DisplayName = _MultiDocs.Table.Columns[index].Caption;
        if (String.IsNullOrEmpty(DisplayName))
          return GetName(index);
        else
          return DisplayName;
      }

      public int IndexOf(string name)
      {
        return _MultiDocs.ColumnNameIndexer.IndexOf(name);
      }

      public DBxDocValue this[int index]
      {
        get { return new DBxDocValue(this, index); }
      }

      public int Count
      {
        get { return _MultiDocs.Table.Columns.Count; }
      }

      public int DocCount { get { return 1; } }

      public bool IsReadOnly
      {
        get
        {
          if (_RowVersion == DataRowVersion.Original)
            return true;


          // TODO:

          return false;
        }
      }

      public object GetValue(int index, DBxDocValuePreferredType preferredType)
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
        if (IsReadOnly)
          return true;

        if (index < _MultiDocs.DocProvider.MainDocTableServiceColumns.Count)
          return true; // Id и Deleted

        return GetColumnDef(index).ReadOnly;
      }

      bool IDBxDocValues.GetGrayed(int index)
      {
        return false;
      }

      object IDBxDocValues.GetComplexValue(int index)
      {
        return GetValue(index, DBxDocValuePreferredType.Unknown);
      }

      void IDBxDocValues.SetComplexValue(int index, object value)
      {
        SetValue(index, value);
      }

      #endregion

      #region IEnumerable<DBxDocValue> Members

      /// <summary>
      /// Возвращает перечислитель по значениям
      /// </summary>
      /// <returns></returns>
      public DBxDocValueEnumerator GetEnumerator()
      {
        return new DBxDocValueEnumerator(this);
      }

      IEnumerator<DBxDocValue> IEnumerable<DBxDocValue>.GetEnumerator()
      {
        return new DBxDocValueEnumerator(this);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new DBxDocValueEnumerator(this);
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
        return _MultiDocs.DocSet.InternalGetBinData(GetValue(index, DBxDocValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetId(), null, 0, GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int Index)
      {
        return _MultiDocs.DocSet.InternalGetDBFile(GetValue(Index, DBxDocValuePreferredType.Int32),
          _MultiDocs.DocType.Name, GetId(), null, 0, GetName(Index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxDocValuePreferredType.Int32),
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
    internal IDBxDocValues GetSingleDocValues(int rowIndex)
    {
      if (Table.Rows[rowIndex].RowState == DataRowState.Deleted)
        return GetSingleDocOriginalValues(rowIndex); // 14.09.2015

      if (_SingleDocValues == null)
        _SingleDocValues = new Dictionary<int, SingleDocValues>();

      SingleDocValues Values;
      if (!_SingleDocValues.TryGetValue(rowIndex, out Values))
      {
        Values = new SingleDocValues(this, rowIndex, DataRowVersion.Current);
        _SingleDocValues.Add(rowIndex, Values);
      }
      return Values;
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
    internal IDBxDocValues GetSingleDocOriginalValues(int rowIndex)
    {
      if (_SingleDocOriginalValues == null)
        _SingleDocOriginalValues = new Dictionary<int, SingleDocValues>();

      SingleDocValues Values;
      if (!_SingleDocOriginalValues.TryGetValue(rowIndex, out Values))
      {
        Values = new SingleDocValues(this, rowIndex, DataRowVersion.Original);
        _SingleDocOriginalValues.Add(rowIndex, Values);
      }
      return Values;
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

      return DataTools.GetInt(_Table.ExtendedProperties["DocActionId" + docId.ToString()]);
    }

    internal void SetDocIdActionId(Int32 docId, Int32 actionId)
    {
#if DEBUG
      if (!DocSet.DocProvider.IsRealDocId(docId))
        throw new ArgumentException("Недопустимый идентификатор документа: " + docId.ToString(), "docId");
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
      View(new Int32[] { docId });
      //return this[DocCount - 1];
      return GetDocById(docId); // 15.10.2015
    }

    /// <summary>
    /// Открывает документ с заданными идентификаторами на просмотр и добавляет его в список открытых
    /// документов В списке ранее открытых документов не должно быть документов с этим идентификатором
    /// </summary>
    /// <param name="docIds">Массив идентификаторов</param>
    public void View(Int32[] docIds)
    {
      if (docIds == null)
        throw new ArgumentNullException("docIds");
      if (docIds.Length < 1)
        throw new ArgumentException("Массив идентификаторов нулевой длины", "docIds");
      for (int i = 0; i < docIds.Length; i++)
      {
        if (!DocProvider.IsRealDocId(docIds[i]))
          throw new ArgumentException("В позиции " + i.ToString() + " задан недопустимый идентификатор документа " + docIds[i].ToString(), "docIds");
      }

      // 15.10.2015 CheckNotInTable(DocIds);

      ResetDocIds();

      DataTable Table2 = DocProvider.LoadDocData(DocType.Name, docIds);
      DoView(Table2);
    }

    /// <summary>
    /// Открывает документ с заданными идентификаторами на просмотр и добавляет его в список открытых
    /// документов В списке ранее открытых документов не должно быть документов с этим идентификатором
    /// </summary>
    /// <param name="docIds">Список идентификаторов</param>
    public void View(IdList docIds)
    {
      if (docIds == null)
        throw new ArgumentNullException("docIds");
      View(docIds.ToArray());
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    public void View(DBxFilter filter)
    {
      DataTable Table2 = DocProvider.LoadDocData(DocType.Name, filter);
      // 15.10.2015 CheckNotInTable(DataTools.GetIds(Table2));

      DoView(Table2);
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// Эта версия дополнительно возвращает массив идентификаторов документов, которые проходят условие фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    /// <param name="docIds">Массив добавленных документов</param>
    public void View(DBxFilter filter, out Int32[] docIds)
    {
      DataTable Table2 = DocProvider.LoadDocData(DocType.Name, filter);
      docIds = DataTools.GetIds(Table2);

      DoView(Table2);
    }

    private void DoView(DataTable Table2)
    {
      if (Permissions == DBxAccessMode.None)
        throw new DBxAccessException("У пользователя нет прав на просмотр документов \"" + DocType.PluralTitle + "\"");

      int OldDocCount = DocCount;

      if (_Table.Rows.Count == 0)
      {
        // Заменяем старую таблицу на новую
        DataSet ds = _Table.DataSet;
        ds.Tables.Remove(_Table);
        ds.Tables.Add(Table2);
        DataTools.SetPrimaryKey(Table2, "Id");
        _Table = Table2; // 
        _Table.AcceptChanges(); // переход в режим View
        _DocValues.Table = _Table;

        // 14.01.2022. Замечание:
        // В этот момент меняется структура таблицы. Свойства DataColumn.AllowDBNull и MaxLength могут быть не инициализированы правильно,
        // т.к. таблица является результатом выполнения SELECT(). Поэтому на них нельзя ориентироваться в DBxDocValue
      }
      else
      {
        // Добавляем строки в существующую таблицу
        // 15.10.2015
        // Пропускаем уже существующие документы
        _Table.BeginLoadData();
        try
        {
          for (int i = 0; i < Table2.Rows.Count; i++)
          {
            Int32 DocId = (Int32)(Table2.Rows[i]["Id"]);
            if (_Table.Rows.Find(DocId) == null)
            {
              DataRow DstRow = _Table.NewRow();
              DataTools.CopyRowValues(Table2.Rows[i], DstRow, false);
              _Table.Rows.Add(DstRow);
              DstRow.AcceptChanges();
            }
          }
        }
        finally
        {
          _Table.EndLoadData();
          _DocValues.ResetBuffer();
        }
      }

      try
      {
        for (int i = OldDocCount; i < _Table.Rows.Count; i++)
          DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.View);
      }
      catch
      {
        // Убираем загруженные данные, иначе от выброса исключения мало толку
        if (OldDocCount == 0)
          _Table.Rows.Clear();
        else
        {
          for (int i = _Table.Rows.Count - 1; i >= OldDocCount; i--)
            _Table.Rows.Remove(_Table.Rows[i]);
        }
        throw;
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
          throw new DBxAccessException("У пользователя нет прав на доступ к документам \"" + DocType.PluralTitle + "\"");
        else
          throw new DBxAccessException("У пользователя нет прав на создание, редактирование и удаление документов \"" + DocType.PluralTitle + "\"");
      }
    }

    /// <summary>
    /// Добавляет в таблицу открытых документов документы с заданными идентификаторами и переводит их состояние
    /// на редактирование. В списке ранее открытых документов не должно быть документов с этими идентификаторами
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public void Edit(Int32[] docIds)
    {
      CheckCanModify();

      //int OldDocCount = DocCount;
      View(docIds);
      /*
      for (int i = OldDocCount; i < FTable.Rows.Count; i++)
      {
        DocProvider.DocPermissions.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeEdit);
        FTable.Rows[i].SetModified();
      }
       * */

      // 15.10.2015
      // Может быть, часть строк были уже открыты на просмотр или на редактирование
      SetEditState(docIds);
    }

    private void SetEditState(Int32[] docIds)
    {
      for (int i = 0; i < docIds.Length; i++)
      {
        DataRow Row = _Table.Rows.Find(docIds[i]);
        if (Row == null)
          throw new BugException("Потеряна строка документа с DocId=" + docIds[i].ToString());
        switch (Row.RowState)
        {
          case DataRowState.Modified:
            break; // ничего не надо проверять
          case DataRowState.Unchanged:
            int RowIndex = _Table.Rows.IndexOf(Row);
            DBxSingleDoc Doc = new DBxSingleDoc(this, RowIndex);
            if (Doc.Deleted)
              DocProvider.TestDocument(Doc, DBxDocPermissionReason.BeforeRestore);
            DocProvider.TestDocument(Doc, DBxDocPermissionReason.BeforeEdit);
            Row.SetModified();
            break;
          default:
            throw new InvalidOperationException("Нельзя перевести документ \"" + DocType.SingularTitle + "\" с DocId=" +
              docIds[i].ToString() + " в режим редактирования, так как строка находится в состоянии " + Row.RowState.ToString());
        }
      }
    }


    /// <summary>
    /// Добавляет в таблицу открытых документов документы с заданными идентификаторами и переводит их состояние
    /// на редактирование. В списке ранее открытых документов не должно быть документов с этими идентификаторами
    /// </summary>
    /// <param name="docIds">Список идентификаторов документов</param>
    public void Edit(IdList docIds)
    {
      if (docIds == null)
        throw new ArgumentNullException("docIds");
      Edit(docIds.ToArray());
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
      Edit(new Int32[] { docId });
      //return this[DocCount - 1];
      return GetDocById(docId); // 15.10.2015
    }


    /// <summary>
    /// Открытие документов на редактирование с использованием фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    public void Edit(DBxFilter filter)
    {
      Int32[] DocIds;
      Edit(filter, out DocIds);
    }

    /// <summary>
    /// Просмотр документов с использованием фильтра
    /// Эта версия дополнительно возвращает массив идентификаторов документов, которые проходят условие фильтра
    /// </summary>
    /// <param name="filter">Фильтр по таблице документов</param>
    /// <param name="docIds">Массив идентификаторов добавленных документов</param>
    public void Edit(DBxFilter filter, out Int32[] docIds)
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
          throw new InvalidOperationException("Нет открытых документов \"" + DocType.PluralTitle + "\"");
        else
          throw new InvalidOperationException("Все документы \"" + DocType.PluralTitle + "\" должны находиться в состоянии View");
      }

      CheckCanModify();

      for (int i = 0; i < _Table.Rows.Count; i++)
      {
        DBxSingleDoc Doc = new DBxSingleDoc(this, i);
        if (Doc.Deleted)
          DocProvider.TestDocument(Doc, DBxDocPermissionReason.BeforeRestore);
        DocProvider.TestDocument(Doc, DBxDocPermissionReason.BeforeEdit);
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

      DataRow Row = _Table.NewRow();
      Row["Id"] = NextFictiveId();
      _Table.Rows.Add(Row);
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
    public void InsertCopy(Int32[] docIds)
    {
      CheckCanModify();

      //int OldDocCount = DocCount;
      View(docIds);

      DataSet TempDS = new DataSet();

      for (int i = 0; i < docIds.Length; i++)
        DoInsertCopy1(TempDS, docIds[i]);

      DBxDocSet.DoInsertCopy2(TempDS, Table.DataSet, DocSet.DocProvider);

      ResetDocIds();

#if DEBUG
      DocSet.CheckDataSet();
#endif
    }

    /// <summary>
    /// Открывает для просмотра документы с заданными идентификаторами, заменяет идентификаторы на временные и
    /// добавляет документы в список открытых. Документы получают состояние Insert
    /// В списке могут быть другие документы в состоянии View, Edit и Insert
    /// </summary>
    /// <param name="docIds">Список идентификаторов документов</param>
    public void InsertCopy(IdList docIds)
    {
      if (docIds == null)
        throw new ArgumentNullException("docIds");
      InsertCopy(docIds.ToArray());
    }



    /// <summary>
    /// Открывает для просмотра документ с заданным идентификатором, заменяет идентификатор на временный и
    /// добавляет документ в список открытых. Документ получает состояние Insert
    /// В списке могут быть другие документы в состоянии View, Edit и Insert
    /// </summary>
    /// <param name="docId">Идентификатор оригинального документа</param>
    public DBxSingleDoc InsertCopy(Int32 docId)
    {
      InsertCopy(new Int32[] { docId });
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
          throw new InvalidOperationException("Нет открытых документов \"" + DocType.PluralTitle + "\"");
        else
          throw new InvalidOperationException("Все документы \"" + DocType.PluralTitle + "\" должны находиться в состоянии View");
      }

      CheckCanModify();

      DataSet TempDS = new DataSet();
      Int32[] DocIds = GetDocIds(DBxDocState.View);
      for (int i = 0; i < DocIds.Length; i++)
        DoInsertCopy1(TempDS, DocIds[i]);

      DBxDocSet.DoInsertCopy2(TempDS, Table.DataSet, DocSet.DocProvider);

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
        throw new ArgumentException("Недопустимый DocId=" + docId.ToString(), "docId");

      DBxSingleDoc Doc = GetDocById(docId);

      if (Doc.DocState != DBxDocState.View)
        throw new InvalidOperationException("Документ \"" + DocType.SingularTitle + "\" с DocId=" + docId.ToString() +
          " не может быть переведен в режим создания копии, т.к. он находится в состоянии " +
          Doc.DocState.ToString() + ", а не View");

      DBxDocSet.DoInsertCopy1(tempDS, DocType.Name, docId, NextFictiveId());

      // Перебираем для замены все поддокументы.
      // При необходимости, они подгружаются
      for (int i = 0; i < Doc.SubDocs.Count; i++)
      {
        DBxSingleSubDocs SubDocs1 = Doc.SubDocs[i];
        foreach (DBxSubDoc SubDoc in SubDocs1)
        {
          DBxDocSet.DoInsertCopy1(tempDS, SubDocs1.SubDocs.SubDocType.Name, SubDoc.SubDocId, Doc.SubDocs[i].SubDocs.NextFictiveId());
        }
      }

    }

    #endregion

    #region Delete()

    /// <summary>
    /// Помещает документы с заданными идентификаторами в список на удаление
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public void Delete(Int32[] docIds)
    {
      CheckCanModify();

      if (docIds == null)
        throw new ArgumentNullException("docIds");
      //if (docIds.Length < 1)
      //  throw new ArgumentException("Массив идентификаторов нулевой длины", "docIds");
      //for (int i = 0; i < docIds.Length; i++)
      //{
      //  if (!DocProvider.IsRealDocId(docIds[i]))
      //    throw new ArgumentException("В позиции " + i.ToString() + " задан недопустимый идентификатор документа " + docIds[i].ToString());
      //}

      CheckNotInTable(docIds);

      ResetDocIds();

      int OldDocCount = DocCount;

      DataTable Table2 = DocProvider.LoadDocData(DocType.Name, docIds);
      if (Table2.Rows.Count != docIds.Length)
        throw new InvalidOperationException("Не удалось загрузить все требуемые документы");
      DoView(Table2);

      if (DocCount != OldDocCount + docIds.Length)
        throw new BugException("Invalid DocCount");

      for (int i = OldDocCount; i < _Table.Rows.Count; i++)
        DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeDelete);
      for (int i = OldDocCount; i < _Table.Rows.Count; i++)
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
    /// Помещает документы с заданными идентификаторами в список на удаление
    /// </summary>
    /// <param name="docIds">Список идентификаторов документов</param>
    public void Delete(IdList docIds)
    {
      if (docIds == null)
        throw new ArgumentNullException("docIds");
      Delete(docIds.ToArray());
    }

    /// <summary>
    /// Помещает документ с заданным идентификатором в список на удаление
    /// </summary>
    /// <param name="docId">Идентификатор удаляемого документа</param>
    public void Delete(Int32 docId)
    {
      Delete(new Int32[] { docId });
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
        switch (DBxDocSet.GetDocState(_Table.Rows[i]))
        {
          case DBxDocState.Delete:
            break;
          case DBxDocState.View:
            DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.BeforeDelete); // 28.01.2022
            if (rowsToDelete == null)
              rowsToDelete = new List<DataRow>();
            rowsToDelete.Add(_Table.Rows[i]);
            break;
          default:
            throw new InvalidOperationException("Нельзя удалить документ, находящийся в состоянии " + DBxDocSet.GetDocState(_Table.Rows[i]).ToString());
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

    private void CheckNotInTable(Int32[] docIds)
    {
      for (int i = 0; i < docIds.Length; i++)
      {
        if (_Table.Rows.Contains(docIds[i]))
          throw new InvalidOperationException("Документ " + DocType.SingularTitle + "с идентификатором " + docIds[i].ToString() +
            " уже есть в наборе");
      }
    }

    private void CheckNotInTable(Int32 docId)
    {
      if (_Table.Rows.Contains(docId))
        throw new InvalidOperationException("Документ " + DocType.SingularTitle + "с идентификатором " + docId.ToString() +
          " уже есть в наборе");
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
      if (!DocProvider.IsRealDocId(docId))
        throw new ArgumentException("Недопустимый идентификатор документа " + docId.ToString(), "docId");

      CheckNotInTable(docId);

      int OldDocCount = DocCount;

      ResetDocIds();

      DocSet.VersionView = true;

      DataTable Table2 = DocProvider.LoadDocDataVersion(DocType.Name, docId, version);
      if (_Table.Rows.Count == 0)
      {
        // Заменяем старую таблицу на новую
        DataSet ds = _Table.DataSet;
        ds.Tables.Remove(_Table);
        ds.Tables.Add(Table2);
        DataTools.SetPrimaryKey(Table2, "Id");
        _Table = Table2;
        _Table.AcceptChanges();
        _DocValues.Table = _Table;
      }
      else
      {
        // Добавляем строки в существующую таблицу
        _Table.BeginLoadData();
        try
        {
          for (int i = 0; i < Table2.Rows.Count; i++)
          {
            DataRow DstRow = _Table.NewRow();
            DataTools.CopyRowValues(Table2.Rows[i], DstRow, false);
            _Table.Rows.Add(DstRow);
            DstRow.AcceptChanges();
          }
        }
        finally
        {
          _Table.EndLoadData();
          _DocValues.ResetBuffer();
        }
      }

      // 28.01.2022 - Как в DoView()
      try
      {
        for (int i = OldDocCount; i < _Table.Rows.Count; i++)
          DocProvider.TestDocument(new DBxSingleDoc(this, i), DBxDocPermissionReason.View);
      }
      catch
      {
        // Убираем загруженные данные, иначе от выброса исключения мало толку
        if (OldDocCount == 0)
          _Table.Rows.Clear();
        else
        {
          for (int i = _Table.Rows.Count - 1; i >= OldDocCount; i--)
            _Table.Rows.Remove(_Table.Rows[i]);
        }
        throw;
      }


      return this[DocCount - 1];
    }

    #endregion

    #region IEnumerable<DBxSingleDoc> Members

    /// <summary>
    /// Перечислитель однотипных документов
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
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

      private DBxMultiDocs _Owner;

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
