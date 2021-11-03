using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Models.Tree;
using FreeLibSet.Data;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ��������� ���������� ������ ������ ����������
  /// </summary>
  public class EFPGroupDocComboBox : EFPControl<ComboBox>
  {
    // ����� ������ �������� �� �� EFPDocComboBox, �� �� EFPListComboBox

    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="docTypeUI">��� ��������� �����</param>
    public EFPGroupDocComboBox(EFPBaseProvider baseProvider, ComboBox control, GroupDocTypeUI docTypeUI)
      : base(baseProvider, control, true)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;

      _DocId = 0;
      _IncludeNested = true;

      new ListControlImagePainter(control, new ListControlImageEventHandler(ControlPainter));
      control.DropDown += new EventHandler(Control_DropDown);
      //Control.DropDownClosed += new EventHandler(Control_DropDownClosed);
      control.SelectedValueChanged += new EventHandler(Control_SelectedValueChanged);
      base.UseIdle = true;

      _BrowserGuid = Guid.NewGuid();
    }

    #endregion

    #region ������ ��������

    /// <summary>
    /// ��� ��������� �����
    /// </summary>
    public GroupDocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private GroupDocTypeUI _DocTypeUI;

    #endregion

    #region �������� DocId

    /// <summary>
    /// ������������� ��������� ������.
    /// �������� DocTypeUi.RootNodeDocId �������� "��� ���������"
    /// </summary>
    public Int32 DocId
    {
      get { return _DocId; }
      set
      {
        if (value == _DocId)
          return;
        _DocId = value;

        if (!Control.DroppedDown)
        {
          Control.SelectedIndex = -1;
          Control.Invalidate();
        }
        if (_DocIdEx != null)
          _DocIdEx.Value = value;
        Validate();

        if (CommandItemsAssigned)
        {
          if (CommandItems is ControlItems)
            ((ControlItems)CommandItems).InitEnabled();
        }
      }
    }
    private Int32 _DocId;

    /// <summary>
    /// ����������� �������� ��� DocId
    /// </summary>
    public DepValue<Int32> DocIdEx
    {
      get
      {
        InitDocIdEx();
        return _DocIdEx;
      }
      set
      {
        InitDocIdEx();
        _DocIdEx.Source = value;
      }
    }
    private DepInput<Int32> _DocIdEx;

    private void InitDocIdEx()
    {
      if (_DocIdEx == null)
      {
        _DocIdEx = new DepInput<Int32>(DocId,DocIdEx_ValueChanged);
        _DocIdEx.OwnerInfo = new DepOwnerInfo(this, "DocIdEx");
      }
    }

    private void DocIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocId = _DocIdEx.Value;
    }

    #endregion

    #region �������� IncludeNested

    /// <summary>
    /// ������� "������� ��������� �����".
    /// ��� �������� ������ ������ �� ����������� �������� ��� DocId=DocTypeUI.RootNodeDocId.
    /// ���� true (�� ���������), �� ������������ ������ "��� ���������",
    /// ����� ������������ "��������� ��� ��������"
    /// </summary>
    public bool IncludeNested
    {
      get { return _IncludeNested; }
      set
      {
        if (value == _IncludeNested)
          return;
        _IncludeNested = value;
        if (_IncludeNestedEx != null)
          _IncludeNestedEx.Value = value;
        Control.Invalidate();
      }
    }
    private bool _IncludeNested;

    /// <summary>
    /// ����������� �������� IncludeNested.
    /// ������ ��� ������ ������ �������� EFPCheckBox.CheckedEx ��� ������ "������� ��������� ������"
    /// </summary>
    public DepValue<bool> IncludeNestedEx
    {
      get
      {
        InitIncludeNestedEx();
        return _IncludeNestedEx;
      }
      set
      {
        InitIncludeNestedEx();
        _IncludeNestedEx.Source = value;
      }
    }
    private DepInput<bool> _IncludeNestedEx;

    private void InitIncludeNestedEx()
    {
      if (_IncludeNestedEx == null)
      {
        _IncludeNestedEx = new DepInput<bool>(IncludeNested,IncludeNestedEx_ValueChanged);
        _IncludeNestedEx.OwnerInfo = new DepOwnerInfo(this, "IncludeNestedEx");
      }
    }

    private void IncludeNestedEx_ValueChanged(object sender, EventArgs args)
    {
      IncludeNested = _IncludeNestedEx.Value;
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ������ ������.
    /// ��� ������ ��������� � �������� ������� DBxDocTreeModel.
    /// </summary>
    public DBxDocTreeModel Model
    {
      get
      {
        if (_Model == null)
          _Model = new DBxDocTreeModel(DocTypeUI.UI.DocProvider,
            DocTypeUI.DocType,
            new DBxColumns(new string[] { "Id", DocTypeUI.DocType.TreeParentColumnName, DocTypeUI.NameColumnName }));
        return _Model;
      }
    }
    private DBxDocTreeModel _Model;

    /// <summary>
    /// ���������� ������ ������
    /// </summary>
    public void PerformRefresh()
    {
      _Model = null;
      Control.SelectedIndex = -1;
      Control.Invalidate();
      Validate();
    }

    #endregion

    #region �������� AuxFilterGroupIds

    /// <summary>
    /// ���������� ������ ��������������� ��������������� ����� ����������.
    /// ���� ������� "��� ���������", ���������� null.
    /// ���� ������� "��������� ��� �����", ���������� ������ ������� �����.
    /// ���� ���� ��������� ������, ���������� ������ �� ������ ��� ���������� ���������,
    /// � ����������� �� IncludeNested
    /// </summary>
    public Int32[] AuxFilterGroupIds
    {
      get
      {
        if (DocId == 0)
        {
          if (IncludeNested)
            return null;
          else
            return DataTools.EmptyIds;
        }
        else
        {
          if (IncludeNested)
            return Model.GetIdWithChildren(DocId);
          else
            return new Int32[1] { DocId };
        }
      }
    }

    #endregion

    #region �������� � ���������� ������

    /// <summary>
    /// ��������, ����������� � ���������.
    /// �� ����� ������ ������ ����������, �.�. ��� ���������� � ��������� �������� ������������� � Object
    /// </summary>
    private class ItemObject
    {
      #region �����������

      /// <summary>
      /// �������� ��������� ����
      /// </summary>
      public ItemObject()
        : this(0, 0 , true)
      {
      }

      public ItemObject(Int32 docId, int indentLevel, bool isOpen)
      {
        _DocId = docId;
        _IndentLevel = indentLevel;
        _IsOpen = isOpen;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ������������� ������
      /// </summary>
      public Int32 DocId { get { return _DocId; } }
      private Int32 _DocId;

      /// <summary>
      /// ������� ��������
      /// </summary>
      public int IndentLevel { get { return _IndentLevel; } }
      private int _IndentLevel;

      public bool IsOpen { get { return _IsOpen; } }
      private bool _IsOpen;

      #endregion

      #region ��������� �������������

      public override string ToString()
      {
        return _DocId.ToString();
      }

      #endregion
    }

    /// <summary>
    /// ������ ��������������� �� ����� ���������� ����������� ������
    /// </summary>
    private bool _InsideInitList;

    private List<ItemObject> InitItemList(out int selIndex)
    {
      List<ItemObject> Items = new List<ItemObject>();

      // 1. �������� ����
      Items.Add(new ItemObject());
      selIndex = 0;

      int IndentLevel = 1; // ������ ��� �������� ����


      // 2. �������� �� �������� ����, �� ������� ��� (����. 17.11.2017)
      if (DocId != 0)
      {
        object[] Rows = Model.TreePathFromId(DocId).FullPath;
        for (int i = 0; i < Rows.Length - 1; i++)
        {
          object[] PartRows = new object[i + 1];
          Array.Copy(Rows, PartRows, i + 1);
          TreePath PartPath = new TreePath(PartRows);

          Int32 ThisDocId = Model.TreePathToId(PartPath);
          Items.Add(new ItemObject(ThisDocId, i + 1, true));
        }

        IndentLevel = Rows.Length;
      }

      // 3. ������ �����, ����������� �� ����� ������ � �������, ������� �������
      TreePath ParentPath = Model.TreePathFromId(DocId).Parent;
      foreach (object Row in Model.GetChildren(ParentPath))
      {
        TreePath ThisPath = new TreePath(ParentPath, Row);
        Int32 ThisDocId = Model.TreePathToId(ThisPath);
        Items.Add(new ItemObject(ThisDocId, IndentLevel, ThisDocId == DocId));
        if (ThisDocId == DocId)
        {
          selIndex = Items.Count - 1;

          // 4. ������ �������� �����
          foreach (object Row2 in Model.GetChildren(ThisPath))
          {
            TreePath ThisPath2 = new TreePath(ThisPath, Row2);
            Int32 ThisDocId2 = Model.TreePathToId(ThisPath2);
            Items.Add(new ItemObject(ThisDocId2, IndentLevel + 1, false));
          }
        }
      }

      return Items;
    }

    #endregion

    #region ����������� ����������

    void Control_DropDown(object sender, EventArgs args)
    {
      try
      {
        _InsideInitList = true;
        try
        {
          int SelIndex;
          List<ItemObject> Items = InitItemList(out SelIndex);
          Control.Items.Clear();
          Control.Items.AddRange(Items.ToArray());
          Control.SelectedIndex = SelIndex;
        }
        finally
        {
          _InsideInitList = false;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "EFPGroupDocComboBox. ������ ��������� DropDownClosed");
      }
    }

    // ������� ��������� ������ ������� ������ ������� SelectedValueChanged � DropDownClosed
    // ��� ������ � ����� � � �����������

    //void Control_DropDownClosed(object Sender, EventArgs Args)
    //{
    //  try
    //  {
    //    InsideInitList = true;
    //    try
    //    {
    //      Control.SelectedItem = null;
    //      Control.Items.Clear(); // ����� ������ ���� ����������� ��������� ��� �������� ��������� 
    //                             // "�����/����" ��� �������� ������
    //    }
    //    finally
    //    {
    //      InsideInitList = false;
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    EFPApp.ShowException(e, "EFPGroupDocComboBox. ������ ��������� DropDownClosed");
    //  }
    //}

    void Control_SelectedValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideInitList)
        {
          ItemObject Item = (ItemObject)(Control.SelectedItem);
          if (Item != null)
            DocId = Item.DocId;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "EFPGroupDocComboBox. ������ ��������� SelectedValueChanged");
      }
    }

    /// <summary>
    /// ����� ��������� ������� Idle().
    /// ���������� ������������, ����� ���������� ����� �� ������.
    /// </summary>
    public override void HandleIdle()
    {
      // 18.11.2017
      // ������ ��������� ������� ������ � DropDownClosed, �.�. ��� ������ � ����� ������� ����������� ������,
      // � ����� ���������� SelectedValueChanged
      if ((!Control.DroppedDown) && Control.SelectedItem != null)
      {
        try
        {
          _InsideInitList = true;
          try
          {
            Control.SelectedItem = null;
            Control.Items.Clear(); // ����� ������ ���� ����������� ��������� ��� �������� ��������� 
            // "�����/����" ��� �������� ������
          }
          finally
          {
            _InsideInitList = false;
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "EFPGroupDocComboBox.HandleIdle()");
        }
      }
    }

    #endregion

    #region ��������� ������

    private void ControlPainter(object sender, ListControlImageEventArgs args)
    {
      ItemObject Item;
      if (args.ItemIndex < 0)
        Item = new ItemObject(DocId, 0, true);
      else
        Item = (ItemObject)(args.Item);

      PaintItem(args, Item);
    }

    private void PaintItem(ListControlImageEventArgs args, ItemObject item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      args.Text = item.ToString();
      if (item.DocId == 0)
      {
        if (IncludeNested)
        {
          args.Text = "[ " + DocTypeUI.AllGroupsDisplayName + " ]";
          args.ImageKey = "Table";
        }
        else
        {
          args.Text = "[ " + DocTypeUI.NoGroupDisplayName + " ]";
          args.ImageKey = "No";
        }
      }
      else
      {
        args.Text = DocTypeUI.GetTextValue(item.DocId);
        if (this._DocTypeUI.UI.DebugShowIds)
          args.Text += " (Id=" + item.DocId + ")";
        if (item.IsOpen)
          args.ImageKey = "TreeViewOpenFolder";
        else
          args.ImageKey = "TreeViewClosedFolder";
      }
      args.LeftMargin = 16 * item.IndentLevel;
    }

    #endregion

    #region ���������� DocumentViewHandler

    private class IntDocumentViewHandler : DocumentViewHandler
    {
      #region ��������

      public EFPGroupDocComboBox Owner;

#pragma warning disable 0649 // TODO: ������ ������ ����������

      public DocumentViewHandler ExternalEditorCaller;

#pragma warning restore 0649


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

      public override string CurrentColumnName
      {
        get
        {
          if (Owner == null)
            return String.Empty;
          else
            return Owner.DocTypeUI.NameColumnName;
        }
      }

      /// <summary>
      /// ���������� ���������� ���������
      /// </summary>
      /// <param name="dataSet"></param>
      /// <param name="isCaller"></param>
      public override void ApplyChanges(DataSet dataSet, bool isCaller)
      {
        if (Owner == null)
          return;


        if (!dataSet.Tables.Contains(Owner.DocTypeUI.DocType.Name))
          return; // ��� �������

        // ������ ��������� ������
        Owner._Model = null;
        Owner.Control.Invalidate();
      }

      //public override void UpdateRowsForIds32(int[] DocIds)
      //{
      //  if (Owner != null)
      //  {
      //    // ������ ��������� ������
      //    Owner.FModel = null;
      //    Owner.Control.Invalidate();
      //  }
      //}

      public override void InitNewDocValues(DBxSingleDoc newDoc)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.DocId != 0)
              newDoc.Values[Owner.DocTypeUI.DocType.GroupRefColumnName].SetInteger(Owner.DocId);
          }
        }
        else
          ExternalEditorCaller.InitNewDocValues(newDoc);
      }

      //public override void ValidateDocValues(DBxSingleDoc SavingDoc, ErrorMessageList ErrorMessages)
      //{
      //}

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
    /// ���������� ������������� ���������� ���������.
    /// ������������ DocumentViewHandler.
    /// </summary>
    public Guid BrowserGuid
    {
      get { return _BrowserGuid; }
      set { _BrowserGuid = value; }
    }
    private Guid _BrowserGuid;

    /// <summary>
    /// ����� ���������� ��� ������ ��������� �������� �� ������.
    /// �������������� �������� ViewHandler
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();

      _ViewHandler = new IntDocumentViewHandler();
      _ViewHandler.Owner = this;
      DocTypeUI.Browsers.Add(_ViewHandler);
    }


    /// <summary>
    /// ����������, ����� ����� � ����������� ��������� �����������.
    /// ������� �������� ViewHandler.
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

    #region ��������� ����

    /// <summary>
    /// ��������� ����� ������ ���������� ����
    /// </summary>
    private class ControlItems : EFPControlCommandItems
    {
      #region �����������

      public ControlItems(EFPGroupDocComboBox controlProvider)
      {
        _ControlProvider = controlProvider;

        ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
        ciCut.GroupBegin = true;
        ciCut.Click += new EventHandler(ciCut_Click);
        Add(ciCut);

        ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
        ciCopy.Click += new EventHandler(ciCopy_Click);
        Add(ciCopy);

        ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
        ciPaste.GroupEnd = true;
        ciPaste.Click += new EventHandler(ciPaste_Click);
        Add(ciPaste);

        ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
        ciShowDocInfo.MenuText = "���������� � ���������";
        ciShowDocInfo.ShortCut = Keys.F12;
        ciShowDocInfo.ImageKey = "Information";
        ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
        ciShowDocInfo.GroupBegin = true;
        ciShowDocInfo.GroupEnd = true;
        Add(ciShowDocInfo);
      }

      #endregion

      #region ��������

      private EFPGroupDocComboBox _ControlProvider;

      #endregion

      #region ������������� ����������� ������

      public void InitEnabled()
      {
        ciCut.Enabled = _ControlProvider.DocId != 0;
        ciCopy.Enabled = _ControlProvider.DocId != 0;
        ciPaste.Enabled = true;
        ciShowDocInfo.Enabled = _ControlProvider.DocId != 0;
      }

      #endregion

      #region ������� ������ ������

      private EFPCommandItem ciCut, ciCopy, ciPaste;

      void ciCut_Click(object sender, EventArgs args)
      {
        ciCopy_Click(null, null);
        _ControlProvider.DocId = 0;
      }

      void ciCopy_Click(object sender, EventArgs args)
      {
        if (_ControlProvider.DocId == 0)
        {
          EFPApp.ShowTempMessage("�������� ������ �� ������");
          return;
        }
        DBxDocSelection DocSel = new DBxDocSelection(_ControlProvider.DocTypeUI.UI.DocProvider.DBIdentity);
        DocSel.Add(_ControlProvider.DocTypeUI.DocType.Name, _ControlProvider.DocId);
        DataObject DObj = new DataObject();
        DObj.SetData(DocSel);
        _ControlProvider.DocTypeUI.UI.OnAddCopyFormats(DObj, DocSel);
        DObj.SetText(_ControlProvider.DocTypeUI.GetTextValue(_ControlProvider.DocId));
        EFPApp.Clipboard.SetDataObject(DObj, true);
      }

      void ciPaste_Click(object sender, EventArgs args)
      {
        DBxDocSelection DocSel = _ControlProvider.DocTypeUI.UI.PasteDocSel();
        if (DocSel == null)
        {
          EFPApp.ShowTempMessage("����� ������ �� �������� ������ �� ���������");
          return;
        }

        Int32[] Ids = DocSel[_ControlProvider.DocTypeUI.DocType.Name];
        if (Ids.Length == 0)
        {
          EFPApp.ShowTempMessage("����� ������ �� �������� ������ �� ��������� \"" + _ControlProvider.DocTypeUI.DocType.PluralTitle + "\"");
          return;
        }
        _ControlProvider.DocId = Ids[0];
      }

      #endregion

      #region ���������� � ���������

      EFPCommandItem ciShowDocInfo;

      void ciShowDocInfo_Click(object sender, EventArgs args)
      {
        if (_ControlProvider.DocId == 0)
        {
          EFPApp.ShowTempMessage("������ �� �������");
          return;
        }

        _ControlProvider.DocTypeUI.ShowDocInfo(_ControlProvider.DocId);
      }

      #endregion
    }

    /// <summary>
    /// ������� ������� ���������� ����
    /// </summary>
    /// <returns>��������� ������ EFPControlCommandItems</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      ControlItems Items = new ControlItems(this);
      Items.InitEnabled();
      return Items;
    }

    #endregion
  }
}
