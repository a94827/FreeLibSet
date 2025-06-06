﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

// #define DEBUG_DICT // Для отладки списка DocRowDict

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  /*
   * Хранение поддокументов
   * Поддокументы одного вида для всех загруженных документов хранятся в одной таблице, присоединенной к
   * набору DBxDocSet.DataSet
   * Загрузка строк поддокументов с сервера выполняется только при обращении к ним. Если в процессе редактирования
   * поддокументы не нужны, они не загружаются.
   * Документы могут добавляться в набор не за один раз, поэтому однократная загрузка поддокументов невозможна.
   * Когда выполняется обращение к поддокументам одного вида, проверяется, для каких документов загрузка уже
   * выполнялась. Составляется список идентификаторов документов, поддокументы которых еще не загружены.
   * Выполняется загрузка поддокументов сразу для всех документов, а не для одного документа, чтобы избежать
   * многократного обращения к серверу. 
   * 
   * В отличие от документов, удаление поддокументов выполняется без использования отдельной del-таблицы,
   * таким образом, для удаления требуется предварительная загрузка поддокументов
   * 
   * При создании копии документа должны быть загружены и скопированы поддокументы всех видов
   * 
   * При сохранении данных на сервере нет необходимости учитывать незагруженные поддокументы, таким образом
   * серверу нет необходимости знать, какие поддокументы были загружены.
   * 
   * Для отслеживания загрузки поддокументов, объект DBxMultiSubDocs содержит внутренний словарь DocRowDict с 
   * ключом по строкам документа. 
   * Значение словаря не имеет значения для определения факта загрузки и первоначально содержит значение null.
   * Для объекта одиночного документа DBxSingleDoc может потребоваться доступ к его поддокументам, а не ко
   * всем. В тоже время нецелесообразно хранить какие-либо данные в DBxSingleDoc. Когда DBxSingleDoc требуется
   * список поддокументов, из общего списка поддокументов фильтруется персональный список и сохраняется в виде
   * массива строк. Использовать DataView с фильтром было бы удобнее, но тогда требуется отдельный DataView
   * для каждого документа, что нельзя сделать из-за катастрофического падения производительности при наличии
   * множества DataView у одной DataTable
   * 
   * Редактирование части поддокументов
   * ----------------------------------
   * Исходный объект DBxMultiSubDocs хранит все поддокументы редактируемых документов.
   * Пользователь может выбрать из них один или несколько поддокументов и выполнить редактирование только этих
   * поддокументов.
   * Метод DBxMultiSubDocs.GetSubSet() создает дополнительный объект DBxMultiSubDocs, содержащий копии выбранных
   * поддокументов. Когда редактирование выполнено, должен быть выполнен метод MergeSubSet(), вносящий в основной
   * набор метод из основного набора
   */

  /// <summary>
  /// Коллекция поддокументов одного вида, относящихся к нескольким однотипным документам
  /// Этот объект содержит ссылку на таблицу поддокументов
  /// При переборе поддокументов могут встречаться удаленные поддокументы
  /// </summary>
  public class DBxMultiSubDocs : /*IObjectWithCode, */IEnumerable<DBxSubDoc>
  {
    #region Защищенный конструктор

    internal DBxMultiSubDocs(DBxMultiDocs owner, DBxSubDocType subDocType)
    {
      _Owner = owner;
      _SubDocType = subDocType;

      InitTables();

      _LastFictiveId = 0;
      _DocRowDict = new Dictionary<DataRow, DataRow[]>();
    }

    internal void InitTables()
    {
      _Table = null;
      _TableIsReady = false;
      _DocRowDict = new Dictionary<DataRow,DataRow[]>(); // 11.07.2016

      /*
  if (Owner.DocSet.DataSet.Tables.Contains(SubDocType.Name))
    FTable = Owner.DocSet.DataSet.Tables[SubDocType.Name];
  else
  {
    FTable = Owner.DocProvider.GetTemplate(SubDocType.Name);
    DataTools.SetPrimaryKey(FTable, "Id");
    //CorrectTableProps(FTable);
    Owner.DocSet.DataSet.Tables.Add(FTable);
  }
   * */
    }

    #endregion

    #region Конструктор для подмножества

    /// <summary>
    /// Создание подмножества строк.
    /// В полученном наборе для всех поддокументов устанавливается состояние View, независимо от состояния поддокументов
    /// в исходном наборе <paramref name="mainObj"/>. В исходном наборе состояние поддокументов не меняется.
    /// </summary>
    /// <param name="mainObj">Основной набор поддокументов, присоединенный к DBxDocSet</param>
    /// <param name="subDocIds">Идентификаторы поддокументов, которые должны попасть в подмножество. 
    /// Могут быть фиктивные идентификаторы для несохраненных поддокументов</param>
    public DBxMultiSubDocs(DBxMultiSubDocs mainObj, Int32[] subDocIds)
      : this(mainObj, GetSrcSubDocRows(mainObj, subDocIds))
    {
    }

    private static DataRow[] GetSrcSubDocRows(DBxMultiSubDocs mainObj, Int32[] subDocIds)
    {
      if (mainObj == null)
        throw new ArgumentNullException("mainObj");
      if (mainObj._MainObj != null)
        mainObj = mainObj._MainObj;
      if (subDocIds == null)
        throw new ArgumentNullException("subDocIds");

      mainObj.GetTableReady();
      DataRow[] srcSubDocRows = new DataRow[subDocIds.Length];
      for (int i = 0; i < subDocIds.Length; i++)
      {
        if (subDocIds[i] == 0)
          throw ExceptionFactory.ArgInvalidEnumerableItem("subDocIds", subDocIds, 0);
        // Фиктивные идентификаторы могут быть

        srcSubDocRows[i] = mainObj._Table.Rows.Find(subDocIds[i]);
        if (srcSubDocRows[i] == null)
          throw new ArgumentException(String.Format(Res.DBxMultiSubDoc_Err_RowNotFound, subDocIds[i]), "subDocIds");
      }

      return srcSubDocRows;
    }

    /// <summary>
    /// Эта версия конструктора используется в табличном просмотре поддокументов, относящихся к некоторому документу.
    /// Не предназначено для использования непосредственно в пользовательском коде.
    /// </summary>         
    /// <param name="mainObj">Основной набор поддокументов, присоединенный к DBxDocSet</param>
    /// <param name="srcSubDocRows">Строки поддокументов. Строки должны относиться к таблице в <paramref name="mainObj"/>.</param>
    public DBxMultiSubDocs(DBxMultiSubDocs mainObj, DataRow[] srcSubDocRows)
      : this(mainObj._Owner, mainObj.SubDocType)
    {
      if (mainObj == null)
        throw new ArgumentNullException("mainObj");
      if (mainObj._MainObj != null)
        mainObj = mainObj._MainObj;

      _MainObj = mainObj;

      // Для подмножества сразу создаем таблицу
      mainObj.GetTableReady(); // 15.04.2022
      _Table = mainObj._Table.Clone();
      DataTools.SetPrimaryKey(_Table, "Id");

      // Псевдонабор нужен для ссылки на DocSet
      DataSet dummyDataSet = new DataSet();
      dummyDataSet.Tables.Add(_Table);
      foreach (object key in mainObj.DocSet.DataSet.ExtendedProperties.Keys)
        dummyDataSet.ExtendedProperties[key] = mainObj.DocSet.DataSet.ExtendedProperties[key];

      if (srcSubDocRows != null)
      {
        for (int i = 0; i < srcSubDocRows.Length; i++)
        {
          DataRow resRow = _Table.NewRow();
          resRow.ItemArray = srcSubDocRows[i].ItemArray;
          _Table.Rows.Add(resRow);

          resRow.AcceptChanges(); // независимо от исходного состояния
          //DataTools.SetRowState(ResRow, SrcSubDocRows[i].RowState);
        }
      }

      _SubDocValues = new MultiSubDocValues(this);
      _TableIsReady = true;

      foreach (DataRow mainRow in mainObj.Owner.Table.Rows)
        _DocRowDict.Add(mainRow, null); // 11.07.2016

#if DEBUG_DICT
      DebugDocRowDict();
#endif
    }

#if XXX
    private static void CopyRowState(DataRow SrcRow, DataRow ResRow)
    {
      switch (SrcRow.RowState)
      {
        case DataRowState.Added:
          if (ResRow.RowState != DataRowState.Added)
            ResRow.SetAdded();
          break;
        case DataRowState.Modified:
          if (ResRow.RowState != DataRowState.Modified)
            ResRow.SetModified();
          break;
        case DataRowState.Unchanged:
          if (ResRow.RowState != DataRowState.Unchanged)
            ResRow.AcceptChanges();
          break;
        case DataRowState.Deleted:
          if (ResRow.RowState != DataRowState.Deleted)
            ResRow.Delete();
          break;
        default:
          throw new BugException("Неизвестное состояние исходной строки: " + SrcRow.RowState.ToString());
      }
    }
#endif

    #endregion

    #region Общие свойства

    /// <summary>
    /// Набор данных
    /// </summary>
    public DBxDocSet DocSet { get { return _Owner.DocSet; } }

    /// <summary>
    /// Доступ к списку документов
    /// </summary>
    public DBxMultiDocs Owner { get { return _Owner; } }
    private readonly DBxMultiDocs _Owner;

    /// <summary>
    /// Описание вида поддокументов
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocType; } }
    private readonly DBxSubDocType _SubDocType;


    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _Owner.DocProvider; } }

    /// <summary>
    /// Для набора, созданного методом GetSubSet(), содержит ссылку на основной набор.
    /// Для основного набора поддокументов содержит null
    /// </summary>
    private readonly DBxMultiSubDocs _MainObj;

    /// <summary>
    /// Возвращает права доступа к таблице документов
    /// </summary>
    public DBxAccessMode Permissions
    {
      get
      {
        return DocSet.DocProvider.DBPermissions.TableModes[SubDocType.Name];
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(SubDocType.PluralTitle);
      sb.Append(", SubDocCount=");
      sb.Append(SubDocCount);
      sb.Append(", SubDocState=");
      sb.Append(SubDocState.ToString());
      if (_MainObj != null)
        sb.Append(" (Partial subset)");
      return sb.ToString();
    }

    #endregion

    #region Таблица данных

    /// <summary>
    /// Таблица поддокументов в наборе данных, относящихся к DocSet
    /// </summary>
    internal DataTable Table
    {
      get
      {
        GetTableReady();
        return _Table;
      }
    }
    private DataTable _Table;

    /// <summary>
    /// Просмотр поддокументов (для редактора)
    /// </summary>
    public DataView SubDocsView
    {
      get
      {
        GetTableReady();
        return _Table.DefaultView;
      }
    }

    /// <summary>
    /// Создает новый объект <see cref="DataView"/> для таблицы поддокументов.
    /// Если метод вызывается несколько раз, то следует удалять созданные объекты вызовом <see cref="IDisposable.Dispose()"/>,
    /// т.к. объекты <see cref="DataView"/> являются ресурсоемкими.
    /// Также можно использовать директиву using.
    /// </summary>
    /// <returns>Новый объект <see cref="DataView"/></returns>
    public DataView CreateSubDocsView()
    {
      GetTableReady();
      return new DataView(_Table);
    }

    private void GetTableReady()
    {
      if (_TableIsReady)
        return;

      #region Создание списка документов, для которых нужны поддокументы

      List<Int32> docIds = new List<Int32>();
      for (int docIndex = 0; docIndex < Owner.DocCount; docIndex++)
      {
        DataRow row = Owner.GetDocRow(docIndex);
        if (_DocRowDict.ContainsKey(row))
          continue;
        Int32 docId = DataTools.GetInt(DBxDocSet.GetValue(row, "Id"));
        if (DocSet.DocProvider.IsRealDocId(docId))
          docIds.Add(docId);
      }

      #endregion

      #region Загрузка поддокументов

      DoInitTable(docIds);

#if DEBUG
      if (_SubDocValues == null)
        throw new BugException();
#endif

      #endregion

      #region Помечаем документы, что загрузка выполнена

      // Добавляем в коллекцию даже документы с фиктивными идентификаторами, т.к. для них может выполняться
      // создание поддокументов

      for (int docIndex = 0; docIndex < Owner.DocCount; docIndex++)
      {
        DataRow row = Owner.GetDocRow(docIndex);

        if (!_DocRowDict.ContainsKey(row))
          _DocRowDict.Add(row, null);
      }

#if DEBUG_DICT
      DebugDocRowDict();
#endif

      #endregion

      #region Порядок строк в таблице

      _Table.DefaultView.Sort = SubDocType.DefaultOrder.ToString();

      #endregion

      _TableIsReady = true;
    }

    private bool _TableIsReady;

    private void DoInitTable(List<Int32> docIds)
    {
      if (_Table == null)
      {
        if (Owner.DocSet.DataSet.Tables.Contains(SubDocType.Name))
        {
          // В наборе есть таблица поддокументов.
          // Ее надо присоединить, загружать из БД не нужно

          _Table = Owner.DocSet.DataSet.Tables[SubDocType.Name];
          if (_SubDocValues == null)
            _SubDocValues = new MultiSubDocValues(this);
          else
            _SubDocValues.Table = _Table;
          return;
        }
      }

      if (docIds.Count > 0)
      {
        DataTable table2;
        if (DocSet.VersionView)
          table2 = DoInitVersionTable(docIds);
        else
          table2 = DocSet.DocProvider.LoadSubDocData(Owner.DocType.Name, SubDocType.Name, docIds.ToArray());

        bool tableIsEmpty;
        if (_Table == null)
          tableIsEmpty = true;
        else
          tableIsEmpty = _Table.Rows.Count == 0;
        if (tableIsEmpty)
        {
          // Первое обращение
          // Используем полученную таблицу
          DataTools.AddTableToDataSet(DocSet.DataSet, table2);
          DataTools.SetPrimaryKey(table2, "Id");
          _Table = table2;
          if (_SubDocValues == null)
            _SubDocValues = new MultiSubDocValues(this);
          else
            _SubDocValues.Table = _Table;
        }
        else
        {
          // Добавляем строки к существующей таблице

          _Table.BeginLoadData();
          try
          {
            for (int i = 0; i < table2.Rows.Count; i++)
            {
              DataRow dstRow = _Table.NewRow();
              DataTools.CopyRowValues(table2.Rows[i], dstRow, false);
              _Table.Rows.Add(dstRow);
            }
          }
          finally
          {
            _Table.EndLoadData();
            _SubDocValues.ResetBuffer();
          }
        }
      }
      else if (_Table == null)
      {
        // Инициализируем пустую таблицу поддокументов
        _Table = DocSet.DocProvider.GetTemplate(Owner.DocType.Name, SubDocType.Name);
        DataTools.SetPrimaryKey(_Table, "Id");
        DataTools.AddTableToDataSet(DocSet.DataSet, _Table);
        if (_SubDocValues == null)
          _SubDocValues = new MultiSubDocValues(this);
        else
          _SubDocValues.Table = _Table;
      }

    }

    /// <summary>
    /// Специальная версия загрузки таблицы поддокументов, когда набор находится в режиме просмотра версий
    /// документов (истории), а не реальных данных
    /// </summary>
    /// <param name="docIds"></param>
    /// <returns></returns>
    private DataTable DoInitVersionTable(List<Int32> docIds)
    {
      DataTable resTable = null;
      for (int i = 0; i < docIds.Count; i++)
      {
        DBxSingleDoc doc = Owner.GetDocById(docIds[i]);
        DataTable table2 = Owner.DocProvider.LoadSubDocDataVersion(Owner.DocType.Name, SubDocType.Name, docIds[i], doc.Version);
        if (i == 0)
          resTable = table2;
        else
          DataTools.CopyRowsToRows(table2, resTable, false, true);
      }
      return resTable;
    }

    /// <summary>
    /// Этот метод сбрасывает флаг TableIsReady в false, но не удаляет данные поддокументов
    /// </summary>
    internal void ResetTable()
    {
      if (_MainObj != null)
        throw new InvalidOperationException(Res.DBxMultiSubDoc_Err_ResetTableForSubSet);
      _TableIsReady = false;
    }

    /// <summary>
    /// Этот метод полностью очищает таблицу поддокументов
    /// </summary>
    internal void ClearList()
    {
      _Table = null; // 11.07.2016
      ResetTable();
      if (DocSet.DataSet.Tables.Contains(_SubDocType.Name))
        DocSet.DataSet.Tables.Remove(_SubDocType.Name);
      _DocRowDict.Clear(); // 11.07.2016

#if DEBUG_DICT
      DebugDocRowDict();
#endif
    }

    /// <summary>
    /// Возвращает КОПИЮ таблицы данных поддокументов.
    /// Удаленные поддокументы не включаются в таблицу
    /// </summary>
    /// <returns></returns>
    public DataTable CreateSubDocsData()
    {
      DataTable table;
      using(DataView dv = new DataView(Table)) // DefaultView использовать нельзя использовать, т.к. оно используется в просмотре таблицы поддокументов
      {
        string s = SubDocType.DefaultOrder.ToString();
        if (String.IsNullOrEmpty(s))
          dv.Sort = "DocId";
        else
          dv.Sort = "DocId," + s;
        table = dv.ToTable();
      }
      return table;
    }

    #endregion

    #region Групповой доступ к поддокументам

    private class MultiSubDocValues : DBxDataTableExtValues, IDBxExtValues, IDBxBinDataExtValues
    {
      #region Конструктор

      public MultiSubDocValues(DBxMultiSubDocs multiSubDocs)
        : base(multiSubDocs._Table, multiSubDocs.ColumnNameIndexer) // Важно, что _Table, а не Table. Иначе будет рекурсия
      {
        _MultiSubDocs = multiSubDocs;
      }

      #endregion

      #region Свойства

      private DBxMultiSubDocs _MultiSubDocs;

      #endregion

      #region IDBxExtValues Members

      public new DBxExtValue this[string name]
      {
        get
        {
          // return FDocValues[Name];
          // Так неправильно. 
          // Нужно, чтобы возвращался объект, ссылающийся на нас
          int index = _MultiSubDocs.ColumnNameIndexer.IndexOf(name);
          if (index < 0)
            throw ExceptionFactory.ArgUnknownColumnName("name",_MultiSubDocs._Table, name);
          return new DBxExtValue(this, index);
        }
      }


      /// <summary>
      /// Находятся ли поддокументы в-целом в режиме "только просмотр"
      /// Возвращает true, если: 
      /// - нет прав на изменение поддокументов
      /// - нет открытых документов
      /// - есть хотя бы один документ в состоянии View
      /// </summary>
      public new bool IsReadOnly
      {
        get
        {
          if (_MultiSubDocs.DocSet.DocProvider.DBPermissions.TableModes[_MultiSubDocs.SubDocType.Name] != DBxAccessMode.Full)
            return true; // ???

          // Для команды Insert проверка бессмысленна

          DataTable table = _MultiSubDocs.Table;

          foreach (DataRow row in table.Rows)
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

      private DataColumn GetColumnDef(int index)
      {
        //return _MultiSubDocs.DocProvider.GetColumnDef(_MultiSubDocs.SubDocType.Name, this[index].Name);
        return _MultiSubDocs.DocProvider.GetColumnDef(_MultiSubDocs.SubDocType.Name, index);
      }

      public new bool GetValueReadOnly(int index)
      {
        // лишнее
        //if (IsReadOnly)
        //  return true;

        if (index < _MultiSubDocs.DocProvider.SubDocTableServiceColumns.Count)
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
        return _MultiSubDocs.DocSet.InternalGetBinData(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetFirstDocId(), 
          _MultiSubDocs.SubDocType.Name, GetFirstId(),
          GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiSubDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int index)
      {
        return _MultiSubDocs.DocSet.InternalGetDBFile(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetFirstDocId(), 
          _MultiSubDocs.SubDocType.Name, GetFirstId(),
          GetName(index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiSubDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetFirstDocId(), 
          _MultiSubDocs.SubDocType.Name, GetFirstId(),
          GetName(index));
      }

      public void SetDBFile(int Index, FreeLibSet.IO.FileContainer file)
      {
        SetValue(Index, _MultiSubDocs.DocSet.InternalSetFile(file));
      }

      private Int32 GetFirstDocId()
      {
        if (_MultiSubDocs.Table.Rows.Count == 0)
          return -1;
        else
          return (Int32)(DBxDocSet.GetValue(_MultiSubDocs.Table.Rows[0], "DocId"));
      }

      private Int32 GetFirstId()
      {
        if (_MultiSubDocs.Table.Rows.Count == 0)
          return -1;
        else
          return (Int32)(DBxDocSet.GetValue(_MultiSubDocs.Table.Rows[0], "Id"));
      }

      #endregion
    }

    /// <summary>
    /// Реализация доступа к "серым" значениям полей в таблице
    /// Инициализируется при изменении FTable
    /// </summary>
    public IDBxExtValues Values
    {
      get
      {
        GetTableReady();
        return _SubDocValues;
      }
    }
    private MultiSubDocValues _SubDocValues;

    internal void ResetValueBuffer()
    {
      if (_SubDocValues != null)
      {
        _SubDocValues.ResetBuffer();
        //FSubDocValues.IsReadOnly = IsReadOnly;
      }
    }

    internal void ResetValueBuffer(int index)
    {
      if (_SubDocValues != null)
        _SubDocValues.ResetBuffer(index);
    }

    #endregion

    #region Таблица загрузки по отдельным документам

    /// <summary>
    /// Загруженные документы
    /// Ключом является строка документа. Если коллекция содержит ключ, строки для документа были загружены
    /// в таблицу _Table
    /// Значением является массив строк поддокументов, если они были определены из _Table, или null,
    /// если строки были загружены но не определены. 
    /// Методы ResetRowsForDocXXX() переводят значения в null, а GetRowsForXXX() создают массивы строк
    /// Сам список существует всегда (11.07.2016)
    /// </summary>
    private Dictionary<DataRow, DataRow[]> _DocRowDict;

    internal DataRow[] GetRowsForDocId(Int32 docId)
    {
      DBxSingleDoc doc = _Owner.GetDocById(docId);
      return GetRowsForDocRow(doc.Row);
    }

    internal DataRow[] GetRowsForDocRow(DataRow docRow)
    {
      GetTableReady();
      DataRow[] res;
      if (!_DocRowDict.TryGetValue(docRow, out res))
      {
        Int32 DocId = DataTools.GetInt(docRow, "Id");
        throw new BugException("Subdocument table for document with DocId=" + DocId.ToString() + " has not been loaded");
      }

      if (res == null)
      {
        // Загружаем список поддокументов для документа
        Int32 docId = DataTools.GetInt(docRow, "Id");
        DataView dv = new DataView(_Table); // DefaultView использовать нельзя использовать, т.к. оно используется в просмотре таблицы поддокументов
        try
        {
          dv.RowFilter = "DocId=" + docId.ToString(); // включая поддокументы с записанным полем Deleted=true
          dv.RowStateFilter |= DataViewRowState.Deleted;
          dv.Sort = SubDocType.DefaultOrder.ToString();
          res = new DataRow[dv.Count];
          for (int i = 0; i < res.Length; i++)
            res[i] = dv[i].Row;
          _DocRowDict[docRow] = res;
        }
        finally
        {
          dv.Dispose();
        }

#if DEBUG_DICT
        DebugDocRowDict();
#endif
      }

      return res;
    }

    /// <summary>
    /// Сброс внутреннего списка поддокументов DocRowDict для данного документа.
    /// Этот вызов является "безвредным", т.к. не приводит к повторной загрузке поддокументов из базы данных
    /// </summary>
    /// <param name="docId"></param>
    internal void ResetRowsForDocId(Int32 docId)
    {
      DBxSingleDoc doc = _Owner.GetDocById(docId);
      ResetRowsForDocRow(doc.Row);
    }

    internal void ResetRowsForDocRow(DataRow docRow)
    {
      if (_DocRowDict.ContainsKey(docRow))
        _DocRowDict[docRow] = null;
    }


    #region Отладочный метод

#if DEBUG_DICT

    private void DebugDocRowDict()
    {
      if (FTable == null)
        return;

      Int32[] DocIds = DataTools.GetIdsFromField(FTable, "DocId");
      for (int i = 0; i < DocIds.Length; i++)
      {
        DataRow DocRow = FOwner.Table.Rows.Find(DocIds[i]);
        if (DocRow == null)
          throw new BugException("В таблице \"" + FTable.TableName + "\" есть поддокумент(ы) для документа с DocId=" + DocIds[i].ToString() +
         ", которого нет основном объекте документов");

        if (!DocRowDict.ContainsKey(DocRow))
          throw new BugException("В DocRowDict нет ключа для документа с DocId=" + DocIds[i].ToString());
      }
    }
#endif

    #endregion

    #endregion

    #region Доступ к отдельным поддокументам

    /// <summary>
    /// Возвращает число поддокументов, включая помеченные на удаление в текущем сеансе работы
    /// </summary>
    public int SubDocCount
    {
      get
      {
        GetTableReady();
        return _Table.Rows.Count;
      }
    }


    /// <summary>
    /// Возвращает число поддокументов, исключая помеченные на удаление в текущем сеансе работы
    /// </summary>
    public int NonDeletedSubDocCount
    {
      get
      {
        GetTableReady();
        int cnt = 0;
        for (int i = 0; i < _Table.Rows.Count; i++)
        {
          if (_Table.Rows[i].RowState != DataRowState.Deleted)
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Доступ к документу по индексу
    /// </summary>
    /// <param name="index">Индекс от 0 до (SubDocCount-1)</param>
    /// <returns></returns>
    public DBxSubDoc this[int index]
    {
      get { return new DBxSubDoc(this, index); }
    }

    /// <summary>
    /// Доступ к документу по строке данных
    /// </summary>
    /// <param name="row">Строка таблицы поддокументов</param>
    /// <returns>Объект поддокумента</returns>
    public DBxSubDoc this[DataRow row]
    {
      get
      {                              
        GetTableReady();
        int p = _Table.Rows.IndexOf(row);
        if (p < 0)
        {
          if (row == null)
            throw new ArgumentNullException("row");
          if (row.Table != _Table)
            throw ExceptionFactory.ArgDataRowNotInSameTable("row", row, _Table);
          else
            throw ExceptionFactory.ArgNotInCollection("row", row, _Table.Rows);
        }
        return new DBxSubDoc(this, p);
      }
    }

    /// <summary>
    /// Поиск поддокумента по идентификатору.
    /// Если нет поддокумента с таким идентификатором, или SubDocId=0, то возвращается false.
    /// </summary>
    /// <param name="subDocId">Идентификатор искомого поддокумента</param>
    /// <param name="subDoc">Сюда записывается найденный поддокумент</param>
    /// <returns>True, если поддокумент найден</returns>
    public bool TryGetSubDocById(Int32 subDocId, out DBxSubDoc subDoc)
    {
      int p = IndexOfSubDocId(subDocId);
      if (p < 0)
      {
        subDoc = new DBxSubDoc();
        return false;
      }
      else
      {
        subDoc = this[p];
        return true;
      }
    }

    /// <summary>
    /// Поиск поддокумента по идентификатору.
    /// Если нет поддокумента с таким идентификатором, или SubDocId=0, то выбрасывается исключение
    /// </summary>
    /// <param name="subDocId">Идентификатор искомого поддокумента</param>
    /// <returns>Найденный поддокумент.
    /// Структура не может быть неинициализированной</returns>
    public DBxSubDoc GetSubDocById(Int32 subDocId)
    {
      int p = IndexOfSubDocId(subDocId);
      if (p < 0)
        throw new ArgumentException(String.Format(Res.DBxMultiSubDoc_Err_RowNotFound, subDocId), "subDocId");
      else
        return this[p];
    }

    /// <summary>
    /// Поиск поддокумента с заданным идентификатором SubDocId
    /// Возвращает индекс поддокумента в таблице или (-1), если такого поддокумента нет
    /// или SubDocId=0
    /// </summary>
    /// <param name="subDocId">Идентифккатор поддокумента</param>
    /// <returns>Индекс поддокумента</returns>
    public int IndexOfSubDocId(Int32 subDocId)
    {
      if (subDocId == 0)
        return -1;

      GetTableReady();
      for (int i = 0; i < Table.Rows.Count; i++)
      {
        Int32 ThisId = DataTools.GetInt(DBxDocSet.GetValue(Table.Rows[i], "Id"));
        if (ThisId == subDocId)
          return i;
      }

      return -1;
    }

    #endregion

    #region Значения для отдельных поддокументов


    /// <summary>
    /// Реализация свойства DBxSubDoc.Values
    /// </summary>
    private class SingleSubDocValues : IDBxExtValues, IDBxBinDataExtValues
    {
      #region Защищенный конструктор

      internal SingleSubDocValues(DBxMultiSubDocs multiSubDocs, int rowIndex, DataRowVersion rowVersion)
      {
        if (rowIndex < 0)
          throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, null);

        _MultiSubDocs = multiSubDocs;
        _RowIndex = rowIndex;
        _RowVersion = rowVersion;
      }

      #endregion

      #region Свойства

      private int _RowIndex;

      private DBxMultiSubDocs _MultiSubDocs;

      private DataRowVersion _RowVersion;

      internal DataRow Row { get { return _MultiSubDocs.Table.Rows[_RowIndex]; } }

      #endregion

      #region IDBxExtValues Members

      public DBxExtValue this[string name]
      {
        get
        {
          int index = _MultiSubDocs.ColumnNameIndexer.IndexOf(name);
          if (index < 0)
            throw ExceptionFactory.ArgUnknownColumnName("name", _MultiSubDocs.Table, name);
          return new DBxExtValue(this, index);
        }
      }

      public string GetName(int index)
      {
        return _MultiSubDocs.Table.Columns[index].ColumnName;
      }

      public string GetDisplayName(int index)
      {
        string displayName = _MultiSubDocs.Table.Columns[index].Caption;
        if (String.IsNullOrEmpty(displayName))
          return GetName(index);
        else
          return displayName;
      }

      public int IndexOf(string name)
      {
        return _MultiSubDocs.ColumnNameIndexer.IndexOf(name);
      }

      public DBxExtValue this[int index]
      {
        get { return new DBxExtValue(this, index); }
      }

      public int Count
      {
        get { return _MultiSubDocs.Table.Columns.Count; }
      }

      int IDBxExtValues.RowCount { get { return 1; } }

      public bool IsReadOnly
      {
        get
        {
          if (_MultiSubDocs.DocSet.DocProvider.DBPermissions.TableModes[_MultiSubDocs.SubDocType.Name] != DBxAccessMode.Full)
            return true; // ???

          // Для команды Insert проверка бессмысленна

          switch (Row.RowState)
          {
            case DataRowState.Unchanged:
            case DataRowState.Deleted:
              return true;
          }
          return false;
        }
      }

      public object GetValue(int index, DBxExtValuePreferredType preferredType)
      {
        if (_RowVersion == DataRowVersion.Original)
          return Row[index, DataRowVersion.Original];
        return Row[index];
      }

      public void SetValue(int index, object value)
      {
        _MultiSubDocs.ResetValueBuffer(index);
        if (value == null)
          Row[index] = DBNull.Value;
        else
          Row[index] = value;
      }

      public bool IsNull(int index)
      {
        if (_RowVersion == DataRowVersion.Original)
          return Row.IsNull(_MultiSubDocs._Table.Columns[index], DataRowVersion.Original);
        else
          return Row.IsNull(index);
      }

      private DataColumn GetColumnDef(int index)
      {
        //return _MultiSubDocs.DocProvider.GetColumnDef(_MultiSubDocs.SubDocType.Name, this[index].Name);
        return _MultiSubDocs.DocProvider.GetColumnDef(_MultiSubDocs.SubDocType.Name, index);
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
        // Лишнее
        // if (IsReadOnly)
        //   return true;

        if (index < _MultiSubDocs.DocProvider.MainDocTableServiceColumns.Count)
          return true; // Id и Deleted

        return GetColumnDef(index).ReadOnly;
      }

      public bool GetGrayed(int index)
      {
        return false;
      }

      public object[] GetValueArray(int index)
      {
        return new object[1] { GetValue(index, DBxExtValuePreferredType.Unknown) };
      }

      public void SetValueArray(int index, object[] values)
      {
        if (values.Length != 1)
          throw new ArgumentException("values.Length must be 1", "values");
        SetValue(index, values[0]);
      }

      public object GetRowValue(int valueIndex, int rowIndex)
      {
        if (rowIndex != 0)
          throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
        return GetValue(valueIndex, DBxExtValuePreferredType.Unknown);
      }

      public void SetRowValue(int valueIndex, int rowIndex, object value)
      {
        if (rowIndex != 0)
          throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
        SetValue(valueIndex, value);
      }

      #endregion

      #region IEnumerable<DBxExtValue> Members

      public IEnumerator<DBxExtValue> GetEnumerator()
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
        return _MultiSubDocs.DocSet.InternalGetBinData(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetDocId(), 
          _MultiSubDocs.SubDocType.Name, GetId(), 
          GetName(index));
      }

      public void SetBinData(int index, byte[] data)
      {
        SetValue(index, _MultiSubDocs.DocSet.InternalSetBinData(data));
      }

      public FreeLibSet.IO.FileContainer GetDBFile(int index)
      {
        return _MultiSubDocs.DocSet.InternalGetDBFile(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetDocId(),
          _MultiSubDocs.SubDocType.Name, GetId(),
          GetName(index));
      }

      public FreeLibSet.IO.StoredFileInfo GetDBFileInfo(int index)
      {
        return _MultiSubDocs.DocSet.InternalGetDBFileInfo(GetValue(index, DBxExtValuePreferredType.Int32),
          _MultiSubDocs.Owner.DocType.Name, GetDocId(),
          _MultiSubDocs.SubDocType.Name, GetId(),
          GetName(index));
      }

      public void SetDBFile(int index, FreeLibSet.IO.FileContainer file)
      {
        SetValue(index, _MultiSubDocs.DocSet.InternalSetFile(file));
      }

      private Int32 GetId()
      {
        return (Int32)(DBxDocSet.GetValue(Row, "Id"));
      }

      private Int32 GetDocId()
      {
        return (Int32)(DBxDocSet.GetValue(Row, "DocId"));
      }

      #endregion
    }

    /// <summary>
    /// Коллекция доступа к объектам значений поддокументов.
    /// Ключом является номер строки в таблице.
    /// Так как SingleSubDocValues не содержит ссылок на таблицу
    /// </summary>
    private Dictionary<int, SingleSubDocValues> _SingleSubDocValues;

    internal IDBxExtValues GetSingleSubDocValues(int rowIndex)
    {
      if (_SingleSubDocValues == null)
        _SingleSubDocValues = new Dictionary<int, SingleSubDocValues>();
      SingleSubDocValues values;
      if (!_SingleSubDocValues.TryGetValue(rowIndex, out values))
      {
        values = new SingleSubDocValues(this, rowIndex, DataRowVersion.Current);
        _SingleSubDocValues.Add(rowIndex, values);
      }
      return values;
    }

    /// <summary>
    /// Реализация свойства DBxSubDoc.OriginalValues
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    internal IDBxExtValues GetSingleSubDocOriginalValues(int rowIndex)
    {
      if (_SingleSubDocOriginalValues == null)
        _SingleSubDocOriginalValues = new Dictionary<int, SingleSubDocValues>();

      SingleSubDocValues values;
      if (!_SingleSubDocOriginalValues.TryGetValue(rowIndex, out values))
      {
        values = new SingleSubDocValues(this, rowIndex, DataRowVersion.Original);
        _SingleSubDocOriginalValues.Add(rowIndex, values);
      }
      return values;
    }

    private Dictionary<int, SingleSubDocValues> _SingleSubDocOriginalValues;

    #endregion

    //#region IObjectWithCode Members

    //string IObjectWithCode.Code
    //{
    //  get { throw new NotImplemntedException(); }
    //}

    //#endregion

    #region IEnumerable<DBxSubDoc> Members

    /// <summary>
    /// Перечислитель поддокументов
    /// </summary>
    public struct Enumerator : IEnumerator<DBxSubDoc>
    {
      #region Конструктор

      internal Enumerator(DBxMultiSubDocs owner)
      {
        _Owner = owner;
        _SubDocIndex = -1;
      }

      #endregion

      #region Поля

      private readonly DBxMultiSubDocs _Owner;

      private int _SubDocIndex;

      #endregion

      #region IEnumerator<DBxSubDoc> Members

      /// <summary>
      /// Текущий поддокумент
      /// </summary>
      public DBxSubDoc Current
      {
        get { return _Owner[_SubDocIndex]; }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Owner[_SubDocIndex]; }
      }

      /// <summary>
      /// Переход к следующему поддокументу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _SubDocIndex++;
        return _SubDocIndex < _Owner.SubDocCount;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _SubDocIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по поддокументам
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<DBxSubDoc> IEnumerable<DBxSubDoc>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Состояние поддокументов

    /// <summary>
    /// Получить количество документов с заданным состоянием
    /// </summary>
    /// <param name="state">Состояние документа (Insert, Edit, View или Delete)</param>
    /// <returns>Количество документов в заданном состоянии</returns>
    public int GetSubDocCount(DBxDocState state)
    {
      GetTableReady();

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
    /// Состояния поддокументов
    /// </summary>
    public DBxDocState SubDocState
    {
      get
      {
        GetTableReady();

        if (_Table.Rows.Count == 0)
          return DBxDocState.None;

        DBxDocState res1 = DBxDocSet.GetDocState(_Table.Rows[0]);
        for (int i = 1; i < _Table.Rows.Count; i++)
        {
          DBxDocState res2 = DBxDocSet.GetDocState(_Table.Rows[i]);
          if (res2 != res1)
            return DBxDocState.Mixed;
        }
        return res1;
      }
    }

    /// <summary>
    /// Возвращает true, если для этого типа документов нет ни одного документа
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        GetTableReady();
        return _Table.Rows.Count == 0;
      }
    }

    /// <summary>
    /// Возвращает true, если есть документы в состоянии Insert, Edit или Delete
    /// </summary>
    public bool ContainsModified
    {
      get
      {
        switch (SubDocState)
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

    #endregion

    #region Изменение состояния поддокументов

    internal void CheckCanModify()
    {
      DocSet.CheckNotReadOnly();

      if (Permissions != DBxAccessMode.Full)
      {
        if (Permissions == DBxAccessMode.None)
          throw new DBxAccessException(String.Format(Res.DBxMultiSubDocs_Err_AccessDenied, SubDocType.PluralTitle));
        else
          throw new DBxAccessException(String.Format(Res.DBxMultiSubDocs_Err_AccessReadOnly, SubDocType.PluralTitle));
      }
    }

    /// <summary>
    /// Возвращает следующий фиктивный идентификатор для создания поддокумента
    /// Если MainObj!=null, вызывает метод из основного набора
    /// </summary>
    /// <returns></returns>
    internal Int32 NextFictiveId()
    {
      if (_MainObj == null)
      {
        _LastFictiveId--;
        return _LastFictiveId;
      }
      else
        return _MainObj.NextFictiveId();
    }
    private Int32 _LastFictiveId;

    /// <summary>
    /// Создает новый поддокумент для заданного документа
    /// </summary>
    /// <param name="doc">Объект для доступа к документу</param>
    /// <returns></returns>
    public DBxSubDoc Insert(DBxSingleDoc doc)
    {
      // При внесении изменений не забыть про InsertForDocIds()

      GetTableReady(); // 09.02.2017
      CheckCanModify();

      switch (doc.DocState)
      {
        case DBxDocState.Insert:
        case DBxDocState.Edit:
          // 
          DataRow row = _Table.NewRow();
          row["Id"] = NextFictiveId();
          row["DocId"] = doc.DocId;
          _Table.Rows.Add(row);
          ResetRowsForDocRow(doc.Row);

#if DEBUG_DICT
          DebugDocRowDict();
#endif

          return new DBxSubDoc(this, _Table.Rows.Count - 1);
        default:
          throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_InsertDocState,
            doc.DocType.SingularTitle, doc.DocId, doc.DocState));
      }
    }

    /// <summary>
    /// Создает новый поддокумент для единственного документа
    /// </summary>
    /// <returns></returns>
    public DBxSubDoc Insert()
    {
      if (Owner.DocCount == 0)
        throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_InsertNoDocuments,
          SubDocType.SingularTitle, Owner.DocType.PluralTitle));
      if (Owner.DocCount != 1)
        throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_InsertMultiDocs,
          Owner.DocType.SingularTitle, Owner.DocCount.ToString()));

      CheckCanModify();
      return Insert(Owner[0]);
    }

    /// <summary>
    /// Групповая вставка поддокументов для указанных идентификаторов документов.
    /// Документы должны быть в состоянии <see cref="DBxDocState.Insert"/> или <see cref="DBxDocState.Edit"/>.
    /// Также выбрасывается исключение, если задан идентификатор документа, которого нет в списке.
    /// </summary>
    /// <param name="docIds">Идентификаторы документов. Могут быть фиктивные идентификаторы для новых документов, но не может быть нулевого значения</param>
    /// <returns>Новые поддокументы в состоянии <see cref="DBxDocState.Insert"/></returns>
    public DBxSubDoc[] InsertForDocIds(Int32[] docIds)
    {
#if DEBUG
      if (docIds == null)
        throw new ArgumentNullException("docIds");
#endif
      GetTableReady();
      CheckCanModify();

      // На первом проходе проверяем права доступа, а на втором - добавляем строки в таблицу
      DBxSingleDoc[] docs = new DBxSingleDoc[docIds.Length];
      for (int i = 0; i < docIds.Length; i++)
      {
        docs[i] = Owner.GetDocById(docIds[i]);

        switch (docs[i].DocState)
        {
          case DBxDocState.Insert:
          case DBxDocState.Edit:
            break;
          default:
            throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_InsertDocState,
              docs[i].DocType.SingularTitle, docs[i].DocId, docs[i].DocState));
        }
      }

      DBxSubDoc[] subDocs = new DBxSubDoc[docIds.Length];
      for (int i = 0; i < docIds.Length; i++)
      {
        DataRow row = _Table.NewRow();
        row["Id"] = NextFictiveId();
        row["DocId"] = docs[i].DocId;
        _Table.Rows.Add(row);
        ResetRowsForDocRow(docs[i].Row);

        subDocs[i] = new DBxSubDoc(this, _Table.Rows.Count - 1);
      }

#if DEBUG_DICT
          DebugDocRowDict();
#endif

      return subDocs;
    }

    /// <summary>
    /// Создает копии всех открытых поддокументов.
    /// Замена ссылок выполняется только в этих поддокументах, если они образуют древовидную (или другую)
    /// структуру, то есть когда есть ссылки "между собой"
    /// </summary>
    public void InsertCopy()
    {
      GetTableReady();

      if (SubDocState != DBxDocState.View)
      {
        if (SubDocCount == 0)
          throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_IsEmpty, SubDocType.PluralTitle));
        else
          throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_StateRequired, SubDocType.PluralTitle,"View"));
      }

      CheckCanModify();

      DataSet tempDS = new DataSet();

      foreach (DataRow row in _Table.Rows)
      {
        DBxDocSet.DoInsertCopy1(tempDS, SubDocType.Name, (Int32)(row["Id"]), NextFictiveId());
      }

      DBxDocSet.DoInsertCopy2(tempDS, _Table.DataSet, DocSet.DocProvider);
    }

    /// <summary>
    /// Переводит все поддокументы в режим редактирования.
    /// Все поддокументы должны находиться в состоянии View.
    /// </summary>
    public void Edit()
    {
      CheckCanModify();

      switch (SubDocState) // таблица подготовлена
      {
        case DBxDocState.None:
        case DBxDocState.Edit:
          return;
        case DBxDocState.View:
          foreach (DataRow Row in _Table.Rows)
            Row.SetModified();
          break;
        default:
          throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Err_SetAllState,
            SubDocType.PluralTitle, "Edit", SubDocState));
      }
    }

    /// <summary>
    /// Удаляет все поддокументы
    /// </summary>
    public void Delete()
    {
      GetTableReady();
      CheckCanModify();


      // При удалении нужно сбрасывать списки поддокументов, т.к. строки в состоянии Added удаляются из списка
      Int32[] docIds = DataTools.GetIdsFromColumn(_Table, "DocId");
      for (int i = 0; i < docIds.Length; i++)
      {
        ResetRowsForDocId(docIds[i]);
        DBxSingleDoc doc = Owner.GetDocById(docIds[i]);
        doc.CheckCanDeleteSubDocs(); // 03.02.2022
      }

      for (int i = 0; i < _Table.Rows.Count; i++)
        _Table.Rows[i].Delete();

#if DEBUG_DICT
      DebugDocRowDict();
#endif
    }

    /// <summary>
    /// Удаляет все поддокументы с задаными идентификаторами
    /// </summary>
    /// <param name="subDocIds">Идентификаторы поддокументов</param>
    public void Delete(Int32[] subDocIds)
    {
      IdList docIds = new IdList();

      for (int i = 0; i < subDocIds.Length; i++)
      {
        DBxSubDoc subDoc = GetSubDocById(subDocIds[i]);
        docIds.Add(subDoc.DocId);
        subDoc.Doc.CheckCanDeleteSubDocs(); // 03.02.2022
        subDoc.Delete();
      }

      foreach (Int32 docId in docIds)
        ResetRowsForDocId(docId);

#if DEBUG_DICT
      DebugDocRowDict();
#endif
    }

    #endregion

    #region Подмножество

    /// <summary>
    /// Создание подмножества поддокументов с единственным поддокументом.
    /// В полученном наборе для всех поддокументов устанавливается состояние <see cref="DBxDocState.View"/>, независимо от состояния поддокументов
    /// в текущем наборе. В текущем наборе состояние поддокументов не меняется.
    /// Используйте <see cref="ReplaceWithSubSet(DBxMultiSubDocs)"/> по окончании работы с подмножеством.
    /// </summary>
    /// <param name="subDocId">Идентификатор поддокумента, который должен попасть в подмножество. 
    /// Могут быть фиктивные идентификаторы для несохраненных поддокументов</param>
    /// <returns>Подмножество поддокументов</returns>
    public DBxMultiSubDocs CreateSubSet(Int32 subDocId)
    {
      return CreateSubSet(new Int32[] { subDocId });
    }

    /// <summary>
    /// Создание подмножества поддокументов с заданными идентификаторами.
    /// В полученном наборе для всех поддокументов устанавливается состояние <see cref="DBxDocState.View"/>, независимо от состояния поддокументов
    /// в текущем наборе. В текущем наборе состояние поддокументов не меняется.
    /// Используйте <see cref="ReplaceWithSubSet(DBxMultiSubDocs)"/> по окончании работы с подмножеством.
    /// </summary>
    /// <param name="subDocIds">Идентификаторы поддокументов, которые должны попасть в подмножество. 
    /// Могут быть фиктивные идентификаторы для несохраненных поддокументов</param>
    /// <returns>Подмножество поддокументов</returns>
    public DBxMultiSubDocs CreateSubSet(Int32[] subDocIds)
    {
      return new DBxMultiSubDocs(this, subDocIds);
    }

    /// <summary>
    /// Присоединяет к текущему (основному) набору поддокументов поддокументы из подмножества после редактирования
    /// Строки в присоединяемом наборе <paramref name="subSet"/> в состоянии View пропускаются.
    /// В текущем наборе состояния поддокументов изменяются.
    /// Поддокументы объединяются на основании идентификатора поддокумента (поле "Id").
    /// </summary>
    /// <param name="subSet">Присоединяемое подмножество документов</param>
    public void MergeSubSet(DBxMultiSubDocs subSet)
    {
      /*
       * Правила изменения состояния поддокументов
       * -------------------------------------------------------
       * | в SubSet -> |         |         |         |         |
       * |             |         |         |         |         |
       * |в текущем    |  View   |  Edit   |  Add    | Deleted |
       * | наборе      |         |         |         |         |
       * |-----------------------------------------------------|
       * |[нет строки] | Ошибка! | Ошибка! |  Add    | Ошибка! |
       * |-----------------------------------------------------|
       * |  View       |  View   |  Edit   | Ошибка  | Deleted |
       * |-----------------------------------------------------|
       * |  Edit       |  Edit   |  Edit   | Ошибка  | Deleted |
       * |-----------------------------------------------------|
       * |  Add        |  Add    |  Add    | Ошибка  | Detached|
       * |-----------------------------------------------------|
       * | Deleted     | Deleted | Ошибка  | Ошибка  | Ошибка  |
       * -------------------------------------------------------
       */



      if (_MainObj != null)
      {
        _MainObj.MergeSubSet(subSet);
        return;
      }

      if (subSet._MainObj != this)
        throw new ArgumentException(Res.DBxMultiSubDocs_Arg_MergeAlienSubSet, "subSet");

      GetTableReady();

      foreach (DataRow srcRow in subSet._Table.Rows)
      {
        if (srcRow.RowState == DataRowState.Unchanged)
          continue;

        Int32 docId = DataTools.GetInt(DBxDocSet.GetValue(srcRow, "DocId"));
        ResetRowsForDocId(docId);

        Int32 id = DataTools.GetInt(DBxDocSet.GetValue(srcRow, "Id"));
        DataRow resRow = _Table.Rows.Find(id);
        if (resRow == null)
        {
          if (srcRow.RowState == DataRowState.Deleted)
            continue; // в подмножество добавили поддокумент, а затем - удалили.

          if (DocSet.DocProvider.IsRealDocId(id))
            throw new InvalidOperationException(String.Format(Res.DBxMultiSubDocs_Arg_MergeSubSetRowState,
              SubDocType.SingularTitle, id, srcRow.RowState));

          resRow = _Table.NewRow();
          resRow.ItemArray = srcRow.ItemArray;
          _Table.Rows.Add(resRow);
        }
        else
        {
          if (srcRow.RowState == DataRowState.Deleted)
          {
            resRow.Delete();
            continue; // 26.08.2015. Вызов SetRowState() вызовет ошибку
          }
          else
            resRow.ItemArray = srcRow.ItemArray;
        }

        DataTools.SetRowState(resRow, srcRow.RowState);
      }
    }

    /// <summary>
    /// Замещает поддокументы на созданное подмножество
    /// 1. Сначала вызывается CreateSubSet() с пустым массивом идентификаторов.
    /// 2. Выполняется добавление поддокументов в подмножество.
    /// 3. Вызывается ReplaceWithSubSet().
    /// При этом все существующие поддокументы заменяются на новые.
    /// Состояние поддокументов в подмножестве (обычно, Insert) игнорируется.
    /// Существующие поддокументы переводятся в режим Edit, лишние - удаляются, если недостает - создаются новые.
    /// Предупреждение: После вызова метода, идентификаторы поддокументов не сохраняются!
    /// </summary>
    /// <param name="subSet">Заполненное подмножество</param>
    public void ReplaceWithSubSet(DBxMultiSubDocs subSet)
    {
      if (_MainObj != null)
      {
        _MainObj.ReplaceWithSubSet(subSet); // ??
        return;
      }

      // if (SubSet.FMainObj != this)
      //   throw new ArgumentException("Заменяющий дочерний набор не был создан из текущего набора поддокументов");

      GetTableReady();

      for (int i = 0; i < subSet.SubDocCount; i++)
      {
        DBxSubDoc resSubDoc;
        if (i < SubDocCount)
        {
          // Заменяем существующий документ
          resSubDoc = this[i];
          if (resSubDoc.SubDocState == DBxDocState.View)
            resSubDoc.Edit();
        }
        else
        {
          // Добавляеми поддокумент
          resSubDoc = Insert();
        }

        DBxExtValue.CopyValues(subSet[i].Values, resSubDoc.Values);
      }

      // Удаляем лишние поддокументы
      for (int i = SubDocCount - 1; i >= subSet.SubDocCount; i--)
        this[i].Delete();
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
          _ColumnNameIndexer = DocProvider.GetColumnNameIndexer(SubDocType.Name);
        return _ColumnNameIndexer;
      }
    }
    private StringArrayIndexer _ColumnNameIndexer;

    #endregion
  }
}
