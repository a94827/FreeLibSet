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
  /// Список однотипных поддокументов, относящихся к одному документу
  /// При переборе поддокументов могут встречаться удаленные поддокументы
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
    private DBxSingleDoc _Doc;

    /// <summary>
    /// Доступ ко всем однотипным поддокументам всех документов набора DBxDocSe
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

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
      sb.Append(" для ");
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
        DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);

        if (SubDocRows.Length == 0)
          return DBxDocState.None;

        DBxDocState Res1 = DBxDocSet.GetDocState(SubDocRows[0]);
        for (int i = 1; i < SubDocRows.Length; i++)
        {
          DBxDocState Res2 = DBxDocSet.GetDocState(SubDocRows[i]);
          if (Res2 != Res1)
            return DBxDocState.Mixed;
        }
        return Res1;
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
        DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        return SubDocRows.Length;
      }
    }

    /// <summary>
    /// Возвращает число поддокументов, исключая помеченные на удаление в текущем сеансе работы
    /// </summary>
    public int NonDeletedSubDocCount
    {
      get
      {
        DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        int cnt=0;
        for (int i = 0; i < SubDocRows.Length; i++)
        {
          if (SubDocRows[i].RowState != DataRowState.Deleted)
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Доступ к поддокументу по индексу, а не по идентификатору
    /// </summary>
    /// <param name="index">Индекс поддокумента в диапазоне от 0 до (SubDocCount-1)</param>
    /// <returns>Поддокумент</returns>
    public DBxSubDoc this[int index]
    {
      get
      {
        DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
        DataRow Row = SubDocRows[index];
        if (Row.RowState == DataRowState.Detached)
          throw new InvalidOperationException("Нельзя получить доступ к поддокументу с индексом " + index.ToString() +
            ", т.к. строка для него была удалена из таблицы поддокументов (RowState=Detached)");
        int RowIndex = _SubDocs.Table.Rows.IndexOf(Row);
        return new DBxSubDoc(SubDocs, RowIndex);
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
      bool Res = SubDocs.TryGetSubDocById(subDocId, out subDoc);
      if (Res)
      {
        if (subDoc.DocId != Doc.DocId)
        {
          subDoc = new DBxSubDoc();
          return false;
        }
      }
      return Res;
    }

    /// <summary>
    /// Доступ к одному поддокументу по идентификатору.
    /// Если SubDocId=0 или в наборе нет поддокумента с таким идентификатором, выбрасывается исключение.
    /// Если найденный поддокуммент относится к другому документу в наборе, то также генерируется исключение.
    /// </summary>
    /// <param name="subDocId">Идентификатор поддокумента</param>
    /// <returns>Найденный поддокумент.
    /// Структура не может быть неинициализированной</returns>
    public DBxSubDoc GetSubDocById(Int32 subDocId)
    {
      DBxSubDoc Res = SubDocs.GetSubDocById(subDocId);
      if (Res.DocId != Doc.DocId)
        throw new ArgumentException("Поддокумент \"" + SubDocs.SubDocType.SingularTitle + "\" c SubDocId=" + subDocId.ToString() + " относится к документу \"" +
          Doc.DocType.SingularTitle + "\" с DocId=" + Res.DocId.ToString() + ", а не " + Doc.DocId.ToString(), "subDocId");
      return Res;
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

      DBxMultiSubDocs _SubDocs;

      private DataRow[] _SubDocRows;

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
          int RowIndex = _SubDocs.Table.Rows.IndexOf(_SubDocRows[_SubDocIndex]);
          return new DBxSubDoc(_SubDocs, RowIndex);
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
      DataRow[] SubDocRows = _SubDocs.GetRowsForDocRow(Doc.Row);
      return new Enumerator(_SubDocs, SubDocRows);
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
    /// - если у пользователя нет прав на изменение поддокумента
    /// - если документ находится в режиме View
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
    /// Генерирует исключение, если IsReadOnly=true
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
    /// Строки в таблице имеют порядок, соответствующий заданному порядку сортировки поддокументов в DBxSubDocType.DefaultOrder
    /// </summary>
    /// <returns></returns>
    public DataTable CreateSubDocsData()
    {
      DataView dv = new DataView(SubDocs.Table); // DefaultView нельзя использовать, т.к. оно используется в просмотре таблицы поддокументов
      try
      {
        dv.RowFilter = "DocId=" + Doc.DocId.ToString();
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
