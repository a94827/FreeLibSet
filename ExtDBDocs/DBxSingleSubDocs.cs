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
  /// Список однотипных поддокументов, относящихся к одному документу.
  /// При переборе поддокументов могут встречаться удаленные поддокументы.
  /// </summary>
  public struct DBxSingleSubDocs : IObjectWithCode, IEnumerable<DBxSubDoc>, IReadOnlyObject
  {
    #region Защищенный конструктор

    internal DBxSingleSubDocs(DBxSingleDoc doc, DBxMultiSubDocs subDocs)
    {
      _Doc = doc;
      _SubDocs = subDocs;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Документ, к которому относятся поддокументы
    /// </summary>
    public DBxSingleDoc Doc { get { return _Doc; } }
    private readonly DBxSingleDoc _Doc;

    /// <summary>
    /// Доступ ко всем однотипным поддокументам всех документов набора <see cref="DBxDocSet"/>
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private readonly DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Набор данных, к которому относится документ
    /// </summary>
    public DBxDocSet DocSet { get { return SubDocs.DocSet; } }

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return SubDocs.DocProvider; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_SubDocs.SubDocType.PluralTitle);
      sb.Append(" for ");
      sb.Append(_Doc.ToString());
      sb.Append(", SubDocCount=");
      sb.Append(SubDocCount.ToString());
      sb.Append(", SubDocState=");
      sb.Append(SubDocState.ToString());
      return sb.ToString();
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return _SubDocs.SubDocType.Name; } }

    #endregion

    #region Состояние поддокументов


    /// <summary>
    /// Состояния поддокументов
    /// </summary>
    public DBxDocState SubDocState
    {
      get
      {
        DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);

        if (subDocRows.Length == 0)
          return DBxDocState.None;

        DBxDocState res1 = DBxDocSet.GetDocState(subDocRows[0]);
        for (int i = 1; i < subDocRows.Length; i++)
        {
          DBxDocState res2 = DBxDocSet.GetDocState(subDocRows[i]);
          if (res2 != res1)
            return DBxDocState.Mixed;
        }
        return res1;
      }
    }


    #endregion

    #region Методы изменения состояния

    /// <summary>
    /// Создает новый поддокумент для документа
    /// </summary>
    /// <returns></returns>
    public DBxSubDoc Insert()
    {
      CheckNotReadOnly();
      return _SubDocs.Insert(_Doc);
    }

    /// <summary>
    /// Удаляет все поддокументы данного вида для документа
    /// </summary>
    public void Delete()
    {
      CheckNotReadOnly();
      Doc.CheckCanDeleteSubDocs(); // 03.02.2022

      // Требуется получение списка
      DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
      for (int i = 0; i < SubDocRows.Length; i++)
        SubDocRows[i].Delete();

      _SubDocs.ResetRowsForDocRow(Doc.Row);
    }

    #endregion

    #region Доступ к поддокументам

    /// <summary>
    /// Возвращает число поддокументов, включая помеченные на удаление в текущем сеансе работы
    /// В обработчике BeforeWrite для записи вычисляемых полей документа следует использовать свойство
    /// NonDeletedSubDocCount
    /// </summary>
    public int SubDocCount
    {
      get
      {
        DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        return subDocRows.Length;
      }
    }

    /// <summary>
    /// Возвращает число поддокументов, исключая помеченные на удаление в текущем сеансе работы
    /// </summary>
    public int NonDeletedSubDocCount
    {
      get
      {
        DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        int cnt = 0;
        for (int i = 0; i < subDocRows.Length; i++)
        {
          if (subDocRows[i].RowState != DataRowState.Deleted)
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Доступ к поддокументу по индексу, а не по идентификатору
    /// </summary>
    /// <param name="index">Индекс поддокумента в диапазоне от 0 до (<see cref="SubDocCount"/>-1)</param>
    /// <returns>Поддокумент</returns>
    public DBxSubDoc this[int index]
    {
      get
      {
        DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        DataRow row = subDocRows[index];
        if (row.RowState == DataRowState.Detached)
          throw new InvalidOperationException(String.Format(Res.DBxSingleSubDocs_Err_RowDetached, index));
        int rowIndex = _SubDocs.Table.Rows.IndexOf(row);
        return new DBxSubDoc(SubDocs, rowIndex);
      }
    }

    /// <summary>
    /// Поиск поддокумента по идентификатору. Если поддокумент не найден, возвращается false.
    /// Если найденный поддокуммент относится к другому документу в наборе, то возвращается false.
    /// </summary>
    /// <param name="subDocId">Идентификатор поддокумента</param>
    /// <param name="subDoc">Сюда записывается найденный поддокумент</param>
    /// <returns>True, если поддокумент найден</returns>
    public bool TryGetSubDocById(Int32 subDocId, out DBxSubDoc subDoc)
    {
      bool res = SubDocs.TryGetSubDocById(subDocId, out subDoc);
      if (res)
      {
        if (subDoc.DocId != Doc.DocId)
        {
          subDoc = new DBxSubDoc();
          return false;
        }
      }
      return res;
    }

    /// <summary>
    /// Доступ к одному поддокументу по идентификатору.
    /// Если <paramref name="subDocId"/>=0 или в наборе нет поддокумента с таким идентификатором, выбрасывается исключение.
    /// Если найденный поддокуммент относится к другому документу в наборе, то также генерируется исключение.
    /// </summary>
    /// <param name="subDocId">Идентификатор поддокумента</param>
    /// <returns>Найденный поддокумент.
    /// Структура не может быть неинициализированной</returns>
    public DBxSubDoc GetSubDocById(Int32 subDocId)
    {
      DBxSubDoc res = SubDocs.GetSubDocById(subDocId);
      if (res.DocId != Doc.DocId)
        throw new ArgumentException(String.Format(Res.DBxSingleSubDocs_Arg_SubDocDiffDoc,
          SubDocs.SubDocType.SingularTitle, subDocId, Doc.DocType.SingularTitle, res.DocId, Doc.DocId), "subDocId");
      return res;
    }

    /// <summary>
    /// Возвращает массив идентификаторов поддокументов.
    /// В массиве могут быть фиктивные идентификаторы, если есть были созданы поддокументы
    /// </summary>
    public IIdSet<Int32> SubDocIds
    {
      get
      {
        DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        if (subDocRows.Length == 0)
          return IdArray<Int32>.Empty;
        int pId = subDocRows[0].Table.Columns.IndexOf("Id");
#if DEBUG
        if (pId < 0)
          throw new BugException();
#endif
        Int32[] ids = new Int32[subDocRows.Length];
        for (int i = 0; i < ids.Length; i++)
          ids[i] = (Int32)(subDocRows[i][pId]);
        return new IdArray<Int32>(ids);
      }
    }

    #endregion

    #region IEnumerable<DBxSubDoc> Members

    /// <summary>
    /// Перечислитель поддокументов одного вида в одном документе
    /// </summary>
    public struct Enumerator : IEnumerator<DBxSubDoc>
    {
      #region Конструктор

      internal Enumerator(DBxMultiSubDocs subDocs, DataRow[] subDocRows)
      {
        _SubDocs = subDocs;
        _SubDocRows = subDocRows;
        _SubDocIndex = -1;
      }

      #endregion

      #region Поля

      private readonly DBxMultiSubDocs _SubDocs;

      private readonly DataRow[] _SubDocRows;

      private int _SubDocIndex;

      #endregion

      #region IEnumerator<DBxSubDoc> Members

      /// <summary>
      /// Текущий поддокумент
      /// </summary>
      public DBxSubDoc Current
      {
        get
        {
          int rowIndex = _SubDocs.Table.Rows.IndexOf(_SubDocRows[_SubDocIndex]);
          return new DBxSubDoc(_SubDocs, rowIndex);
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }


      object System.Collections.IEnumerator.Current
      {
        get { return this.Current; }
      }

      /// <summary>
      /// Переход к следующему поддокументу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _SubDocIndex++;
        return _SubDocIndex < _SubDocRows.Length;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _SubDocIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель поддокументов для одного документа.
    /// Будут перечислены все поддокументы, включая удаленные в текущем сеансе редактирования.
    /// Поддокументы, которые были удалены не в текущем сеансе, не загружаются вместе с документом 
    /// (за исключением просмотра истории) и, соответственно, не могут быть перечислены.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      DataRow[] subDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
      return new Enumerator(_SubDocs, subDocRows);
    }


    IEnumerator<DBxSubDoc> IEnumerable<DBxSubDoc>.GetEnumerator()
    {
      return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true:
    /// - если у пользователя нет прав на изменение поддокумента.
    /// - если документ находится в режиме <see cref="DBxDocState.View"/>.
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        if (_SubDocs.DocSet.DocProvider.DBPermissions.TableModes[_SubDocs.SubDocType.Name] != DBxAccessMode.Full)
          return true;
        if (Doc.DocState == DBxDocState.View)
          return true;

        return false;
      }
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Доступ к таблице

    /// <summary>
    /// Возвращает КОПИЮ таблицы данных поддокументов.
    /// Удаленные поддокументы не включаются в таблицу.
    /// Строки в таблице имеют порядок, соответствующий заданному порядку сортировки поддокументов в DBxSubDocType.DefaultOrder.
    /// </summary>
    /// <returns></returns>
    public DataTable CreateSubDocsData()
    {
      DataView dv = new DataView(SubDocs.Table); // DefaultView нельзя использовать, т.к. оно используется в просмотре таблицы поддокументов
      try
      {
        //dv.RowFilter = "DocId=" + Doc.DocId.ToString();
        DBxFilter filter = new ValueFilter("DocId", Doc.DocId);
        if (DocSet.DocProvider.DocTypes.UseDeleted)
          filter = new AndFilter(DBSSubDocType.DeletedFalseFilter, filter); // 04.10.2025
        dv.RowFilter = filter.ToString();
        dv.Sort = SubDocs.SubDocType.DefaultOrder.ToString();
        return dv.ToTable();
      }
      finally
      {
        dv.Dispose();
      }
    }

    /// <summary>
    /// Возвращает массив строк поддокументов
    /// </summary>
    /// <returns></returns>
    internal DataRow[] GetRows()
    {
      DataView dv = new DataView(SubDocs.Table); // DefaultView использовать нельзя использовать, т.к. оно используется в просмотре таблицы поддокументов
      try
      {
        dv.RowFilter = "DocId=" + Doc.DocId.ToString();
        dv.Sort = SubDocs.SubDocType.DefaultOrder.ToString();
        DataRow[] a = new DataRow[dv.Count];
        for (int i = 0; i < dv.Count; i++)
          a[i] = dv[i].Row;
        return a;
      }
      finally
      {
        dv.Dispose();
      }
    }

    #endregion
  }
}
