// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ��������� �������� �������� OrderMode � EFPDocSelGridView � EFPDocSelTextGridView
  /// </summary>
  public enum EFPDocSelGridViewOrderMode
  {
    /// <summary>
    /// �������� �� ���������.
    /// ������� ����� �� ����� �������� �������������, � ������������ �������� ���������������
    /// </summary>
    Fixed,

    /// <summary>
    /// ������� ����� ����� ��������.
    /// ������������ ����� ������ ������� ����� (������ ����������)
    /// </summary>
    Manual,

    /// <summary>
    /// ������� ����� ������������ ������� ��������� �����������. 
    /// ������������ ����� �������� ������� ���������� (������ ��� EFPDocSelGridView).
    /// ��� EFPDocSelTextGridView ������������ ���������� �� ���������� �������������
    /// </summary>
    Natural,
  }

  /// <summary>
  /// ��������� ��������� ������� ����������.
  /// ��������� �������� ��������� ���������� ���������.
  /// ���� ��� ��������� ����������� ������ EFPDocSelTextGridView.
  /// </summary>
  public interface IEFPDocSelView : IEFPDBxView
  {
    /// <summary>
    /// ��������� ���� ����������, ������������ � �������
    /// </summary>
    DocTypeUI DocTypeUI { get; }

    /// <summary>
    /// �������������� ����������, ������������ � �������
    /// </summary>
    Int32[] Ids { get; set;}

    /// <summary>
    /// ��������� ���� ������ � �������.
    /// ���� ��� ���� ������ � ����� ���������������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="id">������������� ������������ ���������</param>
    /// <returns>true, ���� ������ ���� ���������</returns>
    bool AddId(Int32 id);

    /// <summary>
    /// ��������� ��������� ����� � �������.
    /// ��� ���������������, ������� ��� ���� � �������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="ids">�������������� ����������� ����������</param>
    /// <returns>���������� ����������, ������� ���������</returns>
    int AddIds(Int32[] ids);

    /// <summary>
    /// ����� �� � � ������ �� ���� �� ������ ���������?
    /// </summary>
    bool CanBeEmpty { get;set;}
  }

  /// <summary>
  /// ��������� ���������� ��������� ������� ���������.
  /// ������������ ��������� ������ ������ ����.
  /// ��� ��������� ������������ ������ ������ � ��������� �������������.
  /// �������� Ids ������������ ��� ��������� � ��������� ������ �����.
  /// ������� ����� ����� ����� ��� �� ����� ��������, � ����������� �� �������� OrderMode.
  /// </summary>
  public class EFPDocSelTextGridView : EFPDBxGridView, IEFPDocSelView
  {
    #region ������������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� ���������� ���������</param>
    /// <param name="docTypeUI">��������� ���� ����������</param>
    public EFPDocSelTextGridView(EFPBaseProvider baseProvider, DataGridView control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="controlWithToolBar">����������� ������� � ������ ������������</param>
    /// <param name="docTypeUI">��������� ���� ����������</param>
    public EFPDocSelTextGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI)
      : base(controlWithToolBar, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� ���������� ���������</param>
    /// <param name="ui">��������� ��� ������� � ����������</param>
    /// <param name="docTypeName">��� ���� ���������� � ���������</param>
    public EFPDocSelTextGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui, string docTypeName)
      : base(baseProvider, control, ui)
    {
      Init(ui.DocTypes[docTypeName]);
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="controlWithToolBar">����������� ������� � ������ ������������</param>
    /// <param name="ui">��������� ��� ������� � ����������</param>
    /// <param name="DocTypeName">��� ���� ���������� � ���������</param>
    public EFPDocSelTextGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DBUI ui, string DocTypeName)
      : base(controlWithToolBar, ui)
    {
      Init(ui.DocTypes[DocTypeName]);
    }

    private void Init(DocTypeUI docTypeUI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");

      _DocTypeUI = docTypeUI;
      _OrderMode = EFPDocSelGridViewOrderMode.Fixed;

      _Table = new DataTable();
      _Table.Columns.Add("Id", typeof(Int32));
      _Table.Columns.Add("Text", typeof(string));
      _Table.Columns.Add("Order", typeof(int)); // ������������ � ������ ���������� Manual � Fixed
      DataTools.SetPrimaryKey(_Table, "Id");

      Control.AutoGenerateColumns = false;
      if (EFPApp.ShowListImages) // 14.09.2016
      {
        Columns.AddImage();
        GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(GridHandler_GetCellAttributes);
      }
      Columns.AddTextFill("Text", true, DocType.SingularTitle, 100, 15);
      Columns.LastAdded.CanIncSearch = true;
      if (UI.DebugShowIds)
      {
        Columns.AddInt("Id", true, "Id", 5);
        Columns.LastAdded.CanIncSearch = true;
      }
      CurrentColumnIndex = 1;
      DisableOrdering();
      ReadOnly = false;
      CanInsertCopy = false;
      CanView = true;
      CanMultiEdit = false; // DocTypeUI.CanMultiEdit;
      Control.MultiSelect = true;
      Control.ReadOnly = true;
      CommandItems.Cut += new EventHandler(GridHandler_Cut);

      DBxDocSelectionPasteFormat fmtDocSel = new DBxDocSelectionPasteFormat(UI, DocTypeName);
      fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_Paste);
      CommandItems.PasteHandler.Add(fmtDocSel);
      CommandItems.ClipboardInToolBar = true;

      _CanBeEmpty = true;

      _DocTypeUI.CallInitDocSelView(this);
    }

    #endregion

    #region �������� Ids

    /// <summary>
    /// �������, ���������� ���� "Id", "Text" � "Order"
    /// </summary>
    private DataTable _Table;

    /// <summary>
    /// �������������� ����������, ������������ � �������
    /// </summary>
    public Int32[] Ids
    {
      get
      {
        return DataTools.GetIds(_Table.DefaultView);
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyIds;

        _Table.BeginLoadData();
        try
        {
          _Table.Rows.Clear();
          for (int i = 0; i < value.Length; i++)
          {
            UI.DocProvider.CheckIsRealDocId(value[i]);
            _Table.Rows.Add(value[i], DocTypeUI.GetTextValue(value[i]), i + 1);
          }
        }
        finally
        {
          _Table.EndLoadData();
        }
        Validate(); // 08.07.2019
      }
    }

    /// <summary>
    /// ��������� ���� ������ � �������.
    /// ���� ��� ���� ������ � ����� ���������������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="id">������������� ������������ ���������</param>
    /// <returns>true, ���� ������ ���� ���������</returns>
    public bool AddId(Int32 id)
    {
      if (id == 0)
        return false;
      if (_Table.Rows.Find(id) != null)
        return false; // ��� ����
      DataRow NewRow = _Table.NewRow();
      NewRow["Id"] = id;
      NewRow["Text"] = DocTypeUI.GetTextValue(id);
      NewRow["Order"] = (DataTools.MaxInt(_Table, "Order", false) ?? 0) + 1;
      _Table.Rows.Add(NewRow);
      Validate(); // 08.07.2019
      return true;
    }

    /// <summary>
    /// ��������� ��������� ����� � �������.
    /// ��� ���������������, ������� ��� ���� � �������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="ids">�������������� ����������� ����������</param>
    /// <returns>���������� ����������, ������� ���������</returns>
    public int AddIds(Int32[] ids)
    {
      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        if (AddId(ids[i]))
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region �������� CanBeEmpty

    /// <summary>
    /// ����� �� � � ������ �� ���� �� ������ ���������?
    /// �� ��������� - true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        Validate(); // 08.07.2019
      }
    }
    private bool _CanBeEmpty;

    #endregion

    #region ������ ��������

    /// <summary>
    /// ��������� ��� ������ � ����������� ��������� ����
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// ��� ���� ���������
    /// </summary>
    public string DocTypeName { get { return _DocTypeUI.DocType.Name; } }

    /// <summary>
    /// �������� ��������� � ���� ������
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// ������� ���������� ������.
    /// �� ��������� ������������ ����� Fixed.
    /// �������� ����� ��������������� ������ �� ������ ��������� �� �����
    /// </summary>
    public EFPDocSelGridViewOrderMode OrderMode
    {
      get { return _OrderMode; }
      set
      {
        switch (value)
        {
          case EFPDocSelGridViewOrderMode.Fixed:
          case EFPDocSelGridViewOrderMode.Manual:
          case EFPDocSelGridViewOrderMode.Natural:
            break;
          default:
            throw new ArgumentException();
        }
        CheckHasNotBeenCreated();
        _OrderMode = value;
      }
    }
    private EFPDocSelGridViewOrderMode _OrderMode;

    /// <summary>
    /// ������� �� ������ ��������������� �� ������, ������������ � ���������.
    /// ������� ������������ ��� ���������� ����� �����.
    /// ����� ����������� �������� ����������� ����� ��������� � ������ OnValidate() ��� �������� ���������.
    /// ��� ��������� �������� ������ ������� ��������� �������������� �������� �������������, ��������� �������� CommandItems.CanEditFilters=false
    /// </summary>
    public new GridFilters Filters
    {
      get { return base.Filters; }
      set
      {
        base.Filters = value;
        Validate(); // 08.07.2019
      }
    }

    #endregion

    #region OnShown � OnHidden

    /// <summary>
    /// ���������� ��� ������ ������ ����� �� ������
    /// </summary>
    protected override void OnCreated()
    {
      switch (OrderMode)
      {
        case EFPDocSelGridViewOrderMode.Fixed:
          _Table.DefaultView.Sort = "Order";
          break;

        case EFPDocSelGridViewOrderMode.Natural:
          _Table.DefaultView.Sort = "Text";
          break;

        case EFPDocSelGridViewOrderMode.Manual:
          _Table.DefaultView.Sort = "Order";
          CommandItems.ManualOrderColumn = "Order";
          break;
      }

      Control.DataSource = _Table.DefaultView;

      base.OnCreated();

    }

    /// <summary>
    /// ���������� ��� ������ �������.
    /// ������� ������ DocumentViewhandler
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      _ViewHandler = new IntDocumentViewHandler();
      _ViewHandler.Owner = this;
      _ViewHandler.ExternalEditorCaller = ExternalEditorCaller;
      DocTypeUI.Browsers.Add(_ViewHandler);
    }

    /// <summary>
    /// ����������, ����� ����� � ����������� ��������� �����������.
    /// </summary>
    protected override void OnDetached()
    {
      if (_ViewHandler != null)
      {
        _ViewHandler.Owner = null; // ������ ������, ����� ������� �������� ��� ���� ������
        DocTypeUI.Browsers.Remove(_ViewHandler);
        _ViewHandler = null;
      }
      base.OnDetached();
    }

    #endregion

    #region ���������� ���������

    void GridHandler_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnIndex != 0)
        return;
      if (args.DataRow == null)
        return;
      Int32 Id = DataTools.GetInt(args.DataRow, "Id");
      args.Value = DocTypeUI.GetImageValue(Id);
    }

    #endregion

    #region ��������������

    /// <summary>
    /// ��������� ��������������.
    /// </summary>
    /// <param name="args">������������</param>
    /// <returns>������ true, �.�. �������������� �� ����� ����������� �� ������</returns>
    protected override bool OnEditData(EventArgs args)
    {
      //base.OnEditData(Args);

      Int32 id;
      switch (State)
      {
        case EFPDataGridViewState.Insert:
          Int32[] selIds = DocTypeUI.SelectDocs("���������� ���������� \"" + DocType.PluralTitle + "\" � ��������", this.Filters);
          if (selIds.Length == 0)
            return true;
          for (int i = 0; i < selIds.Length; i++)
            AddId(selIds[i]);
          SelectedIds = selIds;
          break;
        case EFPDataGridViewState.Delete:
          DataRow[] selRows = SelectedDataRows;
          for (int i = 0; i < selRows.Length; i++)
            selRows[i].Delete();
          Validate(); // 08.07.2019
          break;
        case EFPDataGridViewState.View:
          DocTypeUI.PerformEditing(CurrentId, true);
          break;
        case EFPDataGridViewState.Edit:
          id = CurrentId;
          if (DocTypeUI.SelectDoc(ref id, "��������� ������ �� ��������", false, this.Filters))
          {
            CurrentDataRow.Delete();
            AddId(id);
            CurrentId = id;
            Validate(); // 08.07.2019
          }
          break;
        default:
          EFPApp.ShowTempMessage("����������� ����� ��������������");
          break;
      }
      return true;
    }

    /// <summary>
    /// ������� ������������� ��� ����� ����������
    /// ���� �������� �����������, �� ��� �������� ������ ��������� � ��������
    /// �������������� �������� ����� (�������� Caller ��� ������ ClientDocType.PerformEdit()) 
    /// ����� ����������� ���� ������������� ������ ������� �������� (DocGridHandler.DocFilters)
    /// �������� ����� ��������������� ������ �� ������ ��������� �� �����
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller
    {
      get { return _ExternalEditorCaller; }
      set
      {
        CheckHasNotBeenCreated();
        _ExternalEditorCaller = value;
      }
    }
    private DocumentViewHandler _ExternalEditorCaller;

    #endregion

    #region ������� ����������

    /// <summary>
    /// ���������� true
    /// </summary>
    public override bool HasGetDocSelHandler { get { return true; } }

    /// <summary>
    /// �������� DocTypeUI.PerformgetDocSel() ��� ��������������� ���������� � ���������.
    /// ����� �������� ���������� ������� GetDocSel, ���� �� ����������.
    /// </summary>
    /// <param name="args">��������� ������� GetDocSel</param>
    protected override void OnGetDocSel(EFPDBxGridViewDocSelEventArgs args)
    {
      Int32[] ids = DataTools.GetIds(args.DataRows);
      DocTypeUI.PerformGetDocSel(args.DocSel, ids, args.Reason);

      base.OnGetDocSel(args); // ���� ���� ���������������� ����������
    }

    #endregion

    #region ����� ������

    void GridHandler_Cut(object sender, EventArgs args)
    {
      if (CommandItems.PerformCopy())
      {
        DataRow[] selRows = SelectedDataRows;
        for (int i = 0; i < selRows.Length; i++)
          selRows[i].Delete();
        Validate(); // 08.07.2019
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      Int32[] ids = fmtDocSel.DocSel[DocTypeName];

      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        if (AddId(ids[i]))
          cnt++;
      }
      if (cnt == 0)
        EFPApp.ShowTempMessage("��� ��������� \"" + DocType.PluralTitle + "\" � ������ ������ (" + ids.Length.ToString() + " ��.) ��� ���� � ������");
    }

    #endregion

    #region ���������� DocumentViewHandler

    private class IntDocumentViewHandler : DocumentViewHandler
    {
      #region ��������

      public EFPDocSelTextGridView Owner;

      public DocumentViewHandler ExternalEditorCaller;

      #endregion

      #region ���������������� ������ � ��������

      public override DocTypeUI DocTypeUI
      {
        get
        {
          if (Owner == null)
            return null;
          else
            return Owner.DocTypeUI;
        }
      }

      public override Guid BrowserGuid
      {
        get
        {
          if (Owner == null)
            return Guid.Empty;
          else
            return Owner.BrowserGuid;
        }
      }

#pragma warning disable 0219 // TODO: ������ ������ ����������

      /// <summary>
      /// ���������� ���������� ���������
      /// </summary>
      /// <param name="dataSet"></param>
      /// <param name="isCaller"></param>
      public override void ApplyChanges(DataSet dataSet, bool isCaller)
      {
        if (Owner == null)
          return;

        if (!dataSet.Tables.Contains(Owner.DocType.Name))
          return; // ��� �������

        DataTable srcTable = dataSet.Tables[Owner.DocType.Name];
        DataRow resRow;
        Int32 docId;
        bool changed = false;

        foreach (DataRow srcRow in srcTable.Rows)
        {
          switch (srcRow.RowState)
          {
            case DataRowState.Modified:
            case DataRowState.Deleted:
              docId = (Int32)(srcRow["Id"]);
              resRow = Owner._Table.Rows.Find(docId);
              if (resRow == null)
                continue;
              Owner.InvalidateDataRow(resRow); // �� Update
              changed = true;
              break;
          }
        }
      }

#pragma warning restore 0219

      public override void UpdateRowsForIds(Int32[] docIds)
      {
        if (Owner != null)
          Owner.UpdateRowsForIds(docIds);
      }

      public override void InitNewDocValues(DBxSingleDoc newDoc)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.InitNewDocValues(newDoc);
          }
        }
        else
          ExternalEditorCaller.InitNewDocValues(newDoc);
      }

      public override void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.ValidateDocValues(savingDoc, errorMessages);
          }
        }
        else
          ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
      }

      public override void InvalidateControl()
      {
        if (Owner != null)
          Owner.Control.Invalidate();
      }

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

      #endregion
    }

    /// <summary>
    /// ���������� ��������� ����������, ��������� � ������� ����������.
    /// �������� ����� �������� �� null, ����� �������� ������� �� �����
    /// </summary>
    public DocumentViewHandler ViewHandler { get { return _ViewHandler; } }
    private IntDocumentViewHandler _ViewHandler;

    /// <summary>
    /// ������������� ���������.
    /// ������������ DocumentViewHandler
    /// </summary>
    public Guid BrowserGuid
    {
      get { return _BrowserGuid; }
      set { _BrowserGuid = value; }
    }
    private Guid _BrowserGuid;

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������������ ������.
    /// ���� ����������� �������� �����, �������� CanBeEmpty=false � ��� �� ������ ��������� � ���������, ��������������� ������� ������
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      //if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      //{
      // 09.07.2019 - ��������� ������
      if ((!CanBeEmpty) && _Table.DefaultView.Count == 0)
        base.SetError("�� ������� �� ������ ���������");
      //}
      if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
        ValidateFilters();
    }

    private void ValidateFilters()
    {
      if (Filters == null)
        return;
      if (Filters.IsEmpty)
        return;

      IdList badIds;
      EFPApp.BeginWait("�������� ��������� ����������", "Filter");
      try
      {
        // ������ ����� ��� �������
        DBxColumnList colList = new DBxColumnList();
        colList.Add("Id"); // ����� ��� ��� ������
        Filters.GetColumnNames(colList);
        // ��������� ��������
        DataTable table2 = DocTypeUI.TableCache.CreateTable(this.Ids, new DBxColumns(colList));
        DataTableValueArray va = new DataTableValueArray(table2);

        // ��������� ���������
        badIds = new IdList();
        foreach (DataRow row in table2.Rows)
        {
          va.CurrentRow = row;
          DBxCommonFilter BadFilter;
          if (!Filters.TestValues(va, out BadFilter))
            badIds.Add(DataTools.GetInt(row, "Id"));
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (badIds.Count > 0)
      {
        this.SelectedIds = badIds.ToArray();
        SetError("��������� ��������� (" + badIds.Count.ToString() + " �� " + Control.RowCount.ToString() + ") �� �������� ������� �������");
      }
    }

    #endregion
  }
}
