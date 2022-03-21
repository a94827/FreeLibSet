// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ���������� ��������� ����������. ������� �����.
  /// ����������� ������ ����������� � EFPDocGridView � ������ �������� ����������������� ���������.
  /// ���������:
  /// 1. ���������� ��������� ����� ���������� ���������� � ���� ������. ��������� ����������� ��������, ��������� ����������, �������� � ������� DocTypeUI.Browsers
  /// 2. ������������� ��������� �������� � ��������� ��� �������� ������ ���������
  /// </summary>
  public abstract class DocumentViewHandler : SimpleDisposableObject
  {
    // 03.01.2021
    // ����� ������������ ������� ����� ��� �����������

    #region ���������� � ���������

    /// <summary>
    /// �������� ���������� ��������� ���� ���������, � �������� ��������� ������ �������
    /// </summary>
    public abstract DocTypeUI DocTypeUI { get; }

    /// <summary>
    /// ��������������� ���������� ���������.
    /// �� ��������� �������������� ������������� �������� IsCaller � ������ ApplyChanges()
    /// </summary>
    public virtual Guid BrowserGuid { get { return Guid.Empty; } }

    /// <summary>
    /// ���������� ��� �������� ������� ���������� ��������� ��� ������ ������, ���� ������� ��������� ��������,
    /// ��� ���������� ����������
    /// </summary>
    public virtual string CurrentColumnName { get { return String.Empty; } }

    #endregion

    #region ���������� ����� ������ ����������

    /// <summary>
    /// �������� �������� ����� ������ ����������.
    /// ����� ���������� DocProviderUI.ApplyChanges(), ����� ����, ��� ������ �������� ������ ����������
    /// </summary>
    /// <param name="dataSet">����� ������ ����������, ���������� �� �������, � ������ ���������, ��������� ��������</param>
    /// <param name="isCaller">true, ���� ������������� BrowserGuid ��������� � ��������, ���������� � DocumentEditor,
    /// �� ���� ���� �������� ��� ������ �� ����� ���������. � ���� ������ � �������� ������ ���� ��������� ����� ������, 
    /// ���� ���� ��� �� �������� ������� �������</param>
    public virtual void ApplyChanges(DataSet dataSet, bool isCaller)
    {
    }

    /// <summary>
    /// �������� ��������� � ��������� ����������������.
    /// ����� ������� ����� ������ �� ����������������� ����, ������ ���� ������ ��� ������� ���������
    /// � DocTypeUI.TableCache.Clear(DocIds)
    /// </summary>
    /// <param name="docIds">������ ��������������� ����������</param>
    public virtual void UpdateRowsForIds(Int32[] docIds)
    {
    }

    /// <summary>
    /// �������� �������� ��������������� � DocumentEditor �� ����� ������ ApplyChanges
    /// </summary>
    internal static DocumentViewHandler CurrentHandler = null;

    #endregion

    #region ������������� ����� ���������

    /// <summary>
    /// ���������������� �������� ������ ���������).
    /// �� ������ ������ �������� ��������� � ������ Insert
    /// ���������� ���������� ����� ������������� �������� NewDoc.Values ��� 
    /// ��������� ������������
    /// </summary>
    /// <param name="newDoc">����������� ��������</param>
    public virtual void InitNewDocValues(DBxSingleDoc newDoc)
    {
    }

    /// <summary>
    /// ��������� �������� ����� ������� �� ������������ ������� �����������
    /// (��������, �������� �������).
    /// ���������� ���������� �� ������ �������� �������� ����� � SavingDoc
    /// </summary>
    /// <param name="savingDoc">������������ ��������</param>
    /// <param name="errorMessages">���� ����� �������� �������������� �
    /// �������������� ������� � ������� ErrorMessages.AddWarning(). ����� ����
    /// ��������� ��������� ��������������</param>
    public virtual void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
    }

    /// <summary>
    /// ��������������� �����, ������������ �������� ���� ��� ������ ���������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public object GetNewDocValue(string columnName)
    {
      // ������� ��������������
      DBxDocSet docSet = new DBxDocSet(DocTypeUI.UI.DocProvider);
      DBxSingleDoc doc = docSet[DocTypeUI.DocType.Name].Insert();
      InitNewDocValues(doc);
      return doc.Values[columnName].Value;
    }


    #endregion

    #region ������ ������

    /// <summary>
    /// ��������� ��������� ��������, ������� Control.Invalidate()
    /// </summary>
    public abstract void InvalidateControl();

    #endregion
  }

  /// <summary>
  /// ������ �������� DocumentViewHandler, ����������� ��������� DocTypeUI.Browsers
  /// </summary>
  public sealed class DocumentViewHandlerList : List<DocumentViewHandler>
  {
    #region ���������� �����������

    internal DocumentViewHandlerList(DocTypeUI docTypeUI)
    {
      _DocTypeUI = docTypeUI;
    }

    private DocTypeUI _DocTypeUI;

    #endregion

    #region ���������� ����� ������ ����������

    /// <summary>
    /// �������� ��������� � ��������� ����������������.
    /// ����� ������� ����� ������ �� ����������������� ����, ������ ���� ������ ��� ������� ���������
    /// � DocTypeUI.TableCache.Clear(<paramref name="docIds"/>)
    /// </summary>
    /// <param name="docIds">������ ��������������� ����������</param>
    public void UpdateRowsForIds(Int32[] docIds)
    {
      if (Count == 0)
        return;
      foreach (DocumentViewHandler item in this)
        item.UpdateRowsForIds(docIds);
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// �������� Control.Invalidate() ��� ���� ��������� ����������
    /// </summary>
    public void InvalidateControls()
    {
      if (Count == 0)
        return;
      foreach (DocumentViewHandler item in this)
        item.InvalidateControl();
    }

    /// <summary>
    /// ���������� ������� ���� ������ � ����� ��������� ���������� ��� ������ ����������.
    /// ��������������, ��� ����� ���������� ��� �������� �� ���� ������.
    /// ��������� ������ � ������ � ���������� ���������������� (�����) ������������.
    /// ���� ����� �������� ������ ��� ��������� ������� ����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="documents">����� ������</param>
    public void UpdateDBCacheAndRows(DBxDocSet documents)
    {
      // ���������� ���������� ���� � DBUI.UpdateDBCacheAndRows()

#if DEBUG
      if (documents == null)
        throw new ArgumentNullException("documents");
#endif

      if (documents.VersionView)
        return;

      documents.DocProvider.UpdateDBCache(documents.DataSet);
      Int32[] docIds = documents[_DocTypeUI.DocType.Name].DocIds;
      if (docIds.Length > 0)
        UpdateRowsForIds(docIds);
      _DocTypeUI.RefreshBufferedData(); // 03.02.2022
    }

    #endregion
  }

  /// <summary>
  /// ���������� DocumentViewHandler ��� EFPDocGridView.
  /// ������ ����� ����� ���� �������� � ���������������� ����������, ����������� IEFPDBxGridView
  /// </summary>
  public class StdDocumentViewHandler : DocumentViewHandler
  {
    #region ����������� � Dispose

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="owner">��������� ���������� ��������� - ��������</param>
    public StdDocumentViewHandler(IEFPDocView owner)
      : this(owner, owner.DocTypeUI)
    {
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="owner">��������� ���������� ��������� - ��������</param>
    /// <param name="docTypeName">��� ������� ���� ���������</param>
    public StdDocumentViewHandler(IEFPDBxView owner, string docTypeName)
      : this(owner, owner.UI.DocTypes[docTypeName])
    {
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="owner">��������� ���������� ��������� - ��������</param>
    /// <param name="docTypeUI">��������� ���� ���������</param>
    public StdDocumentViewHandler(IEFPDBxView owner, DocTypeUI docTypeUI)
    {
#if DEBUG
      if (owner == null)
        throw new ArgumentNullException("owner");
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
#endif
      if (!Object.ReferenceEquals(owner.UI, docTypeUI.UI))
        throw new ArgumentException("������ ������ �� DBUI");

      _Owner = owner;
      _DocTypeUI = docTypeUI;

      IEFPDocView owner2 = owner as IEFPDocView;
      if (owner2 != null)
      {
        _ExternalEditorCaller = owner2.ExternalEditorCaller;
        _BrowserGuid = owner2.BrowserGuid;
      }
      else
        _BrowserGuid = Guid.NewGuid();
    }

    /// <summary>
    /// ���������� ������ ��� �������� �����
    /// </summary>
    /// <param name="disposing">True, ���� ������ ����� Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      _Owner = null; // ������ ������, ����� ������� �������� ��� ���� ������
      _ExternalEditorCaller = null;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ���������� ��������� - ��������
    /// </summary>
    public IEFPDBxView Owner { get { return _Owner; } }
    private IEFPDBxView _Owner;

    private IEFPDocView Owner2 { get { return _Owner as IEFPDocView; } }

    /// <summary>
    /// ��������� ���� ����������
    /// </summary>
    public override DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;


    /// <summary>
    /// ������������ ��� ������������� ����� ������ ���������
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller { get { return _ExternalEditorCaller; } }
    private DocumentViewHandler _ExternalEditorCaller;

    /// <summary>
    /// ���������� EFPDocGridView.BrowserGuid
    /// </summary>
    public override Guid BrowserGuid { get { return _BrowserGuid; } }
    private Guid _BrowserGuid;


    #endregion

    #region ���������������� ��������

    /// <summary>
    /// ���������� EFPDocGridView.CurrentColumnName
    /// </summary>
    public override string CurrentColumnName
    {
      get
      {
        if (Owner == null)
          return String.Empty;
        else
          return Owner.CurrentColumnName;
      }
    }

    #endregion

    #region ApplyChanges()

    /// <summary>
    /// ���������� ���������� ���������
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="isCaller"></param>
    public override void ApplyChanges(DataSet dataSet, bool isCaller)
    {
      if (Owner == null)
        return;
      if (Owner.SourceAsDataTable == null)
        return; // 04.06.2018

      string docTypeName = DocTypeUI.DocType.Name;

      if (!dataSet.Tables.Contains(docTypeName))
        return; // ��� �������

      DataTable srcTable = dataSet.Tables[docTypeName];
      List<DataRow> newSelRows = new List<DataRow>();

      DBxColumns filterColumns = null;
      if (Owner2 != null)
        filterColumns = Owner2.Filters.GetColumnNames();

      // ���� � ������� ��������� ���� �� ���� "Id"?
      bool hasPrimaryKey = String.Compare(DataTools.GetPrimaryKey(_Owner.SourceAsDataTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;

      // 08.07.2016
      // ������� ����� �� ��������� ���������� �����
      DataView dvFind = null;
      if (!hasPrimaryKey)
      {
        dvFind = new DataView(Owner.SourceAsDataTable);
        dvFind.Sort = "Id";
      }
      try
      {
        foreach (DataRow srcRow in srcTable.Rows)
        {
          // 15.07.2020. �������� �������

          #region ���������� ������������� ������� ������ � ���������

          bool isVisible;
          switch (srcRow.RowState)
          {
            case DataRowState.Added: // ������ ������������ ������ �� RowState
              if (isCaller)
                isVisible = true; // �� ������, ��� ���� ������ ����������
              else if (Owner2 == null)
                isVisible = false;
              else
                isVisible = TestFilters(srcRow, filterColumns);
              break;
            case DataRowState.Modified:
              if (Owner2 == null)
                isVisible = true;
              else
                isVisible = TestFilters(srcRow, filterColumns);
              break;
            case DataRowState.Deleted:
              if (Owner2 == null)
                isVisible = true;
              else
              {
                if (Owner2.ShowDeleted)
                  isVisible = TestFilters(srcRow, filterColumns);
                else
                  isVisible = false;
              }
              break;
            default: // Unchanged
              continue;

          }

          #endregion

          #region ���������� ������������� ������ � ���������

          Int32 docId;
          if (srcRow.RowState == DataRowState.Deleted)
            docId = (Int32)(srcRow["Id", DataRowVersion.Original]);
          else
            docId = (Int32)(srcRow["Id"]);
          DataRow resRow = FindDocRow(docId, dvFind);

          #endregion

          #region ���������� / ���������� / �������� ������ � ���������

          if (isVisible)
          {
            if (resRow == null)
            {
              resRow = Owner.SourceAsDataTable.NewRow();
              DataTools.CopyRowValues(srcRow, resRow, true);
              UpdateRefValues(srcRow, resRow);
              Owner.SourceAsDataTable.Rows.Add(resRow);
            }
            else
            {
              DataTools.CopyRowValues(srcRow, resRow, true);
              UpdateRefValues(srcRow, resRow);
              Owner.InvalidateDataRow(resRow); // �� Update
            }

            // ���������� ���� 26.05.2021
            // ����� ���� ��������� ����������, ����������� � ����� ������������ ������� ������.
            // � ���� ������ ���������� ������� "ResRow==null" ����������� ��� ������� ���������,
            // � ������ �������� �� ����� ��������� ����������� ������ � �������.
            // �� ��������������� �� ����� ������ ���� � ����� ������, ���� ��� ����� � ���������.
            if (srcRow.RowState == DataRowState.Added)
              newSelRows.Add(resRow);
          }
          else
          {
            if (resRow != null)
              resRow.Delete();
          }

          #endregion
        }
      }
      finally
      {
        if (dvFind != null)
          dvFind.Dispose();
      }

      if (newSelRows.Count > 0)
      {
        try
        {
          Owner.SelectedDataRows = newSelRows.ToArray();
        }
        catch
        {
          EFPApp.ErrorMessageBox("�� ������� ������������ ����������� ������ � ��������� ���������");
        }
      }
    }

    private DataRow FindDocRow(Int32 docId, DataView dvFind)
    {
      if (docId == 0)
        return null;

      if (dvFind == null)
        return Owner.SourceAsDataTable.Rows.Find(docId);
      else
      {
        int p = dvFind.Find(docId);
        if (p >= 0)
          return dvFind[p].Row;
        else
          return null;
      }
    }

    private void UpdateRefValues(DataRow srcRow, DataRow resRow)
    {
      // 13.11.2020
      // ������ srcRow ����� ���� �������� �� ��������
      if (srcRow.RowState == DataRowState.Deleted)
        return;

      for (int i = 0; i < resRow.Table.Columns.Count; i++)
      {
        string colName = resRow.Table.Columns[i].ColumnName;
        int p = colName.IndexOf('.');
        if (p >= 0)
        {
          string mainColName = colName.Substring(0, p);
          int pCol = srcRow.Table.Columns.IndexOf(mainColName);
          if (pCol >= 0)
          {
            Int32 refId = DataTools.GetInt(srcRow, mainColName); // � ResRow ����� �� ���� �������� ����
            object refValue = Owner.UI.TextHandlers.DBCache[DocTypeUI.DocType.Name].GetRefValue(colName, refId);
            if (refValue == null)
              resRow[i] = DBNull.Value; // 26.10.2016
            else
              resRow[i] = refValue;
          }
        }
      }
    }

    /// <summary>
    /// ���������, �������� �� ������ ��������� �� ������ DBxDataSet ������� �������, �������� ��� ���������
    /// </summary>
    /// <param name="srcRow">������ ��������� ��� DBxSingleDoc</param>
    /// <param name="filterColumns">������ �������, ������� ��������� ��� �������� ������������� �������� ���������� ���������.
    /// � ������ ����� ���� ��������� ����</param>
    /// <returns>True, ���� �������� �������� ����������</returns>
    private bool TestFilters(DataRow srcRow, DBxColumns filterColumns)
    {
      if (filterColumns == null)
        return true; // ������ �����������

      // 13.11.2020
      // ����������� ������ ����� ���� �������� �� ��������
      DataRowVersion rowVer = srcRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;

      NamedValues pairs = new NamedValues();
      for (int i = 0; i < filterColumns.Count; i++)
      {
        string colName = filterColumns[i];
        object value;
        int pDot = colName.IndexOf('.');
        if (pDot >= 0)
        {
          string mainColName = colName.Substring(0, pDot);
          int pCol = srcRow.Table.Columns.IndexOf(mainColName);
          if (pCol >= 0)
          {
            Int32 refId = DataTools.GetInt(srcRow[mainColName, rowVer]);
            value = Owner.UI.TextHandlers.DBCache[DocTypeUI.DocType.Name].GetRefValue(colName, refId);
          }
          else
            throw new BugException("�� ������� ���� \"" + mainColName + "\"");
        }
        else
          value = srcRow[colName, rowVer];

        if (value is DBNull)
          value = null;
        pairs.Add(colName, value);
      }

      return Owner2.Filters.TestValues(pairs);
    }

    #endregion

    #region ������ ���������������� ������

    /// <summary>
    /// ���������� Owner.ToString(), ���� ������ ����������� � ���������
    /// </summary>
    /// <returns>��������� ������������� ��� ���������� ���������</returns>
    public override string ToString()
    {
      if (Owner == null)
        return "[ �������� �� ��������� ]";
      else
        return Owner.ToString();
    }


    /// <summary>
    /// �������� EFPDocGridView.UpdateRowsForIds()
    /// </summary>
    /// <param name="docIds">�������������� ����������� ����������</param>
    public override void UpdateRowsForIds(Int32[] docIds)
    {
      if (Owner != null)
        Owner.UpdateRowsForIds(docIds);
    }

    /// <summary>
    /// ������������� ����� ������ ���������
    /// </summary>
    /// <param name="newDoc"></param>
    public override void InitNewDocValues(DBxSingleDoc newDoc)
    {
      if (ExternalEditorCaller == null)
      {
        if (Owner2 != null)
        {
          if (Owner2.Filters != null)
            Owner2.Filters.InitNewDocValues(newDoc);


          // 10.06.2019
          DocTypeUI.InitNewDocGroupIdValue(newDoc, Owner2.AuxFilterGroupIds);
        }
      }
      else
        ExternalEditorCaller.InitNewDocValues(newDoc);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="savingDoc"></param>
    /// <param name="errorMessages"></param>
    public override void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      if (ExternalEditorCaller == null)
      {
        if (Owner2 != null)
        {
          if (Owner2.Filters != null)
            Owner2.Filters.ValidateDocValues(savingDoc, errorMessages);
        }
      }
      else
        ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
    }

    /// <summary>
    /// �������� DataGridView.Invalidate()
    /// </summary>
    public override void InvalidateControl()
    {
      if (Owner != null)
        Owner.Control.Invalidate();
    }

    #endregion
  }
}
