// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Доступ к отдельному поддокументу
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DBxSubDoc
  {
    #region Защищенный конструктор

    internal DBxSubDoc(DBxMultiSubDocs multiSubDocs, int rowIndex)
    {
      if (rowIndex < 0)
        throw new ArgumentOutOfRangeException();

      _MultiSubDocs = multiSubDocs;
      _RowIndex = rowIndex;
    }

    #endregion

    #region Свойства

    // Как и для документа DBxSingleDoc, не стоит хранить прямую ссылку на строку

    internal DataRow Row { get { return _MultiSubDocs.Table.Rows[_RowIndex]; } }

    private int _RowIndex;

    //private DataRow FRow;

    /// <summary>
    /// Набор данных с документами
    /// </summary>
    public DBxDocSet DocSet
    {
      get
      {
        return _MultiSubDocs.DocSet;
      }
    }

    /// <summary>
    /// Документ, к которому относится поддокумент
    /// </summary>
    public DBxSingleDoc Doc
    {
      get
      {
        return MultiDocs.GetDocById(DocId);
      }
    }

    /// <summary>
    /// Провайдер доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _MultiSubDocs.DocProvider; } }

    /// <summary>
    /// Идентификатор документа, к которому относится поддокумент
    /// </summary>
    public Int32 DocId { get { return (Int32)(DBxDocSet.GetValue(Row, "DocId")); } }

    /// <summary>
    /// Идентификатор поддокумента.
    /// Для новых поддокументов или документов, если не было вызова ApplyChanges(), 
    /// возвращается фиктивный (отрицательный) идентификатор
    /// </summary>
    public Int32 SubDocId { get { return (Int32)(DBxDocSet.GetValue(Row, "Id")); } }

    /// <summary>
    /// Возвращает идентификатор поддокумента, если он "реальный".
    /// Если документ или поддокумент еще не был ни разу сохранен в базе данных, возвращается 0,
    /// а не фиктивный идентфикатор
    /// </summary>
    public Int32 RealSubDocId
    {
      get
      {
        Int32 v = SubDocId;
        if (DocSet.DocProvider.IsRealDocId(v))
          return v;
        else
          return 0;
      }
    }

    /// <summary>
    /// Возвращает состояние поддокумента
    /// </summary>
    public DBxDocState SubDocState
    {
      get
      {
        return DBxDocSet.GetDocState(Row);
      }
    }

#if XXX
    /// <summary>
    /// Возвращает true, если открытый документ был помечен на удаление.
    /// Если DocState=Edit, то после вызова ApplyChanges() документ будет восстановлен
    /// </summary>
    public bool Deleted 
    {
      get { return DataTools.GetBool(FRow, "Deleted"); }
    }
#endif

    /// <summary>
    /// Доступ ко всем документам одного вида, загруженных в DBxDocSet
    /// </summary>
    public DBxMultiDocs MultiDocs { get { return _MultiSubDocs.Owner; } }

    /// <summary>
    /// Доступ ко всем поддокументам одного вида, загруженных в DBxDocSet
    /// </summary>
    public DBxMultiSubDocs MultiSubDocs { get { return _MultiSubDocs; } }
    private DBxMultiSubDocs _MultiSubDocs;

    /// <summary>
    /// Описание вида документа
    /// </summary>
    public DBxDocType DocType { get { return MultiDocs.DocType; } }

    /// <summary>
    /// Описание вида поддокумента
    /// </summary>
    public DBxSubDocType SubDocType { get { return _MultiSubDocs.SubDocType; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(SubDocType.SingularTitle);
      sb.Append(", SubDocId=");
      sb.Append(SubDocId.ToString());
      sb.Append(" (DocId=");
      sb.Append(DocId.ToString());
      sb.Append(")");
      sb.Append(", SubDocState=");
      sb.Append(SubDocState.ToString());
      return sb.ToString();
    }

    #endregion

    #region Значения полей

    /// <summary>
    /// Интерфейс доступа к значениям поддокумента
    /// </summary>
    public IDBxDocValues Values
    {
      get { return _MultiSubDocs.GetSingleSubDocValues(_RowIndex); }
    }

    /// <summary>
    /// Доступ к оригинальным значениям в режиме Edit
    /// </summary>
    public IDBxDocValues OriginalValues
    {
      get
      {
        if (SubDocState == DBxDocState.Edit || SubDocState == DBxDocState.Delete)
          return _MultiSubDocs.GetSingleSubDocOriginalValues(_RowIndex);
        else
          return null;
      }
    }

    #endregion

    #region Изменение состояния поддокумента

    /// <summary>
    /// Переводит один поддокумент в режим копии.
    /// Если исходный поддокукумент содержит ссылку сам на себя, то такая ссылка будет изменена.
    /// остальные ссылки не изменяются
    /// </summary>
    public void InsertCopy()
    {
      if (SubDocState != DBxDocState.View)
        throw new InvalidOperationException("Поддокумент должен находиться в состоянии View, а не " + SubDocState.ToString());
      DataSet TempDS = new DataSet();

      DBxDocSet.DoInsertCopy1(TempDS, MultiSubDocs.SubDocType.Name, SubDocId, MultiSubDocs.NextFictiveId());
      DBxDocSet.DoInsertCopy2(TempDS, MultiSubDocs.Table.DataSet, DocSet.DocProvider);
    }

    /// <summary>
    /// Переводит документ в режим редактирования.
    /// Документ должен находиться в состоянии View
    /// </summary>
    public void Edit()
    {
      MultiSubDocs.CheckCanModify();

      DataTools.SetRowState(Row, DataRowState.Modified);
    }

    /// <summary>
    /// Помечает поддокумент на удаление
    /// </summary>
    public void Delete()
    {
      MultiSubDocs.CheckCanModify();

      MultiSubDocs.ResetRowsForDocRow(Doc.Row);

      Row.Delete();
    }

#if XXX
    /// <summary>
    /// Удаляет документ из списка открытых документов без сохранения изменений.
    /// Документ может находиться в любом состоянии (View, Edit или Insert)
    /// После вызова метода, дальнейшее использованиет объекта DBxSingleDoc невозможно
    /// </summary>
    public void RemoveFromList()
    {
      throw new NotImplementedException();
    }
#endif

    #endregion

    #region Проверка наличия изменений в поддокументе

    /// <summary>
    /// Свойство возвращает true, если для поддокумента в состоянии Edit есть измененные значения
    /// </summary>
    public bool IsDataModified
    {
      get
      {
        switch (SubDocState)
        {
          case DBxDocState.Insert:
          case DBxDocState.Delete:
            return true;
          case DBxDocState.Edit:
            return DBxDocSet.GetIsModified(Row);

          default:
            return false;
        }
      }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает true, если текущий и сравниваемый поддокумент относятся к одному и тому же 
    /// поддокументу базы данных.
    /// Поддокументы могут относится к разным DBxDocProvider
    /// Возвращает false, если текущий поддокумент не был сохранен в базе данных
    /// </summary>
    /// <param name="otherSubDoc">Сравниваемый поддокумент</param>
    /// <returns>true, если один поддокумент</returns>
    public bool IsSameSubDoc(DBxSubDoc otherSubDoc)
    {
      if (SubDocId <= 0)
        return false;
      if (SubDocId != otherSubDoc.SubDocId)
        return false;

      if (SubDocType.Name != otherSubDoc.SubDocType.Name)
        return false;
      if (DocSet.DocProvider.DBIdentity != otherSubDoc.DocSet.DocProvider.DBIdentity)
        return false;
      return true;
    }

    #endregion

    #region Текстовое представление поддокумента

    /// <summary>
    /// Возвращает текстовое представление поддокумента
    /// </summary>
    public string TextValue
    {
      get
      {
        // Если тупо передать весь набор данных в DocProvider, то может быть передача большого объема данных через сериализацию
        return DocSet.DocProvider.InternalGetTextValue(this.Row);
      }
    }

    #endregion
  }
}
