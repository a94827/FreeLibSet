using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Logging;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// ������������� �������� ������ �����.
  /// ��������� ������� �������� ���������� ����� "��� ���������"
  /// </summary>
  public class EFPGroupDocTreeView : EFPDocTreeView, IEFPDBxView
  {
    #region ������������


    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� - ������</param>
    /// <param name="docTypeUI">��������� ������� � ���� ���������� ������</param>
    public EFPGroupDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, GroupDocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI)
    {
      Init();
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="controlWithToolBar">����������� ������� � ������ ������������</param>
    /// <param name="docTypeUI">��������� ������� � ���� ���������� ������</param>
    public EFPGroupDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, GroupDocTypeUI docTypeUI)
      : base(controlWithToolBar, docTypeUI)
    {
      Init();
    }

    private void Init()
    {
      Control.SelectionMode = TreeViewAdvSelectionMode.Single;
      _IncludeNested = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������� � ���������� ������
    /// </summary>
    public new GroupDocTypeUI DocTypeUI { get { return (GroupDocTypeUI)(base.DocTypeUI); } }

    /// <summary>
    /// ������������� ��������� ������.
    /// ��� ���� "��� ���������" ������������ 0.
    /// ������ ����� �� ������������ ������������� ����� ����������, ������� �������� ����������
    /// ������, ��������� �� ������ �������������� ��� ������ ������.
    /// ������� ��������� �������� ��� ������� � ����������� ����������, �������� � ������� ����������.
    /// </summary>
    public override Int32[] SelectedIds
    {
      get
      {
        if (CurrentId == 0)
          return DataTools.EmptyIds;
        else
          return new Int32[1] { CurrentId };
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyIds;
        if (value.Length == 0)
          CurrentId = 0;
        else if (value.Length > 1)
          throw new ArgumentException("������������� ����� ����� � ������ ����� ���������� �� �����������");
        else
          CurrentId = value[0];
      }
    }

    /// <summary>
    /// ������������� ��������� ������.
    /// ��� ���� "��� ���������" ������������ 0.
    /// </summary>
    public override Int32 CurrentId
    {
      get
      {
        Int32 v = base.CurrentId;
        if (v == RootNodeDocId)
          return 0;
        else
          return v;
      }
      set
      {
        if (value == 0)
          base.CurrentId = RootNodeDocId;
        else
          base.CurrentId = value;
      }
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ������ ��� �������������� ��������
    /// </summary>
    /// <param name="args">�� ������������</param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (args == null)
        EFPApp.BeginWait("�������� ������", DocTypeUI.TableImageKey);
      else
        EFPApp.BeginWait("���������� ������", "Refresh");
      try
      {
        DBxDocTreeModel Model = new DBxDocTreeModel(DocTypeUI.UI.DocProvider, DocTypeUI.DocType, UsedColumnNames);
        AddRootNode(Model);
        Control.Model = Model;

        base.CallRefreshDataEventHandler(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// ���������� ��� ����������� ��� ���������, � �������� ��������� ����.
    /// ����������� ��������� � ������ EFPApp.MainImages
    /// </summary>
    /// <param name="node">����</param>
    /// <returns>��� �����������</returns>
    public override string GetNodeImageKey(TreeNodeAdv node)
    {
      DataRow Row = node.Tag as DataRow;
      if (Row == null)
        return base.GetNodeImageKey(node);

      Int32 DocId = DataTools.GetInt(Row, "Id");
      if (DocId == RootNodeDocId)
        return RootNodeImageKey;
      else if (node.IsLeaf)
        return node.IsSelected ? "TreeViewOpenFolder" : "TreeViewClosedFolder";
      else
        //return base.GetNodeImageKey(Node);
        return String.Empty; // �������������� ���������
    }

    /// <summary>
    /// �������� DocTypeUI.PerformEditing() ��� ��������� �����
    /// </summary>
    /// <param name="args">�� ������������</param>
    /// <returns>������������</returns>
    protected override bool OnEditData(EventArgs args)
    {
      return DocTypeUI.PerformEditing(this.SelectedIds, State, Control.FindForm().Modal, ViewHandler);
    }

    #endregion

    #region �������� IncludeNested

    /// <summary>
    /// ������� "������� ��������� �����".
    /// ��� �������� ������ �� ����������� ��������� ����,
    /// ���� �������� RootNodeTextValue � RootNodeImageKey �� ����������� � ����� ����.
    /// ���� true (�� ���������), �� ������������ ������ "��� ���������",
    /// ����� ������������ "��������� ��� ��������".
    /// ����� �������� ������ �� ��������, ������������ AuxFilterGroupIds.
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

        if (_RootNodeDataRow != null)
        {
          if (FirstTextControl != null)
            _RootNodeDataRow[FirstTextControl.DataPropertyName] = this.RootNodeTextValue;
          DBxDocTreeModel Model = (DBxDocTreeModel)(Control.Model);
          Model.RefreshNode(Model.TreePathFromDataRow(_RootNodeDataRow));
        }

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
        _IncludeNestedEx = new DepInput<bool>();
        _IncludeNestedEx.OwnerInfo = new DepOwnerInfo(this, "IncludeNestedEx");
        _IncludeNestedEx.Value = IncludeNested;
        _IncludeNestedEx.ValueChanged += new EventHandler(IncludeNestedEx_ValueChanged);
      }
    }

    private void IncludeNestedEx_ValueChanged(object sender, EventArgs args)
    {
      IncludeNested = _IncludeNestedEx.Value;
    }

    #endregion

    #region �������� ����

    /// <summary>
    /// ��������� ������������� ��������� ��� ��������� ���� � ��������
    /// </summary>
    private const Int32 RootNodeDocId = unchecked((int)0x80000000);

    /// <summary>
    /// ���������� ������ "������", ��������������� ��������� ����
    /// </summary>
    //public DataRow RootNodeDataRow { get { return FRootNodeDataRow; } }
    private DataRow _RootNodeDataRow;

    private void AddRootNode(DBxDocTreeModel model)
    {
      foreach (DataRow Row in model.Table.Rows)
      {
        if (DataTools.GetInt(Row, model.ParentColumnName) == 0)
          Row[model.ParentColumnName] = RootNodeDocId;
      }

      _RootNodeDataRow = model.Table.NewRow();
      _RootNodeDataRow["Id"] = RootNodeDocId;

      BaseTextControl tc = base.FirstTextControl;
      if (tc != null)
      {
        _RootNodeDataRow[tc.DataPropertyName] = RootNodeTextValue;
      }
      model.Table.Rows.Add(_RootNodeDataRow);
    }

    /// <summary>
    /// ����� ��� ���� � ������� ���������������.
    /// �� ��������� ���������� "��� ���������"
    /// </summary>
    public string RootNodeTextValue
    {
      get
      {
        if (String.IsNullOrEmpty(_RootNodeTextValue))
          return "[ " + (_IncludeNested ? DocTypeUI.AllGroupsDisplayName : DocTypeUI.NoGroupDisplayName) + " ]";
        else
          return _RootNodeTextValue;
      }
      set
      {
        _RootNodeTextValue = value;
        if (_RootNodeDataRow != null && FirstTextControl != null)
          _RootNodeDataRow[FirstTextControl.DataPropertyName] = this.RootNodeTextValue;
      }
    }
    private string _RootNodeTextValue;

    /// <summary>
    /// ������ ��� ��������� ����
    /// </summary>
    public string RootNodeImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(_RootNodeImageKey))
          return _IncludeNested ? "Table" : "No";
        else
          return _RootNodeImageKey;
      }
      set
      {
        _RootNodeImageKey = value;
        if (_RootNodeDataRow != null)
        {
          DBxDocTreeModel Model = (DBxDocTreeModel)(Control.Model);
          Model.RefreshNode(Model.TreePathFromDataRow(_RootNodeDataRow));
        }
      }
    }
    private string _RootNodeImageKey;

    #endregion

    #region ���������� DocumentViewHandler

    /// <summary>
    /// ��������� ������ ������� ��������� ���� � ��������� ���������������
    /// </summary>
    protected new class IntDocumentViewHandler : EFPDocTreeView.IntDocumentViewHandler
    {
      #region �����������

      /// <summary>
      /// ����������� �������
      /// </summary>
      /// <param name="owner">��������� �������������� ��������� - ��������</param>
      public IntDocumentViewHandler(EFPGroupDocTreeView owner)
        : base(owner)
      {
      }

      #endregion

      /// <summary>
      /// ���������� ������ DataRow <paramref name="resRow"/>, ������������ ������� ������������� ���������.
      /// ��� ����� �������� ������ �������� �������� ���� "ParentId" ��������� ���������.
      /// </summary>
      /// <param name="srcRow">������ � ����������� ������ ������</param>
      /// <param name="resRow">����������� ������ � ������ �������������� ���������</param>
      protected override void CopyRowValues(DataRow srcRow, DataRow resRow)
      {
        base.CopyRowValues(srcRow, resRow);

        if ((Int32)(resRow["Id"]) == RootNodeDocId)
        {
#if DEBUG
          try
          {
            throw new BugException("������� ���������� ��� ��������� ������");
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "EFPGroupTreeView.InitDocumentViewHandler.CopyRowValues()");
          }
#endif
          return;
        }

        int p = resRow.Table.Columns.IndexOf("ParentId");
        if (p < 0)
          return;

        if (resRow.IsNull(p))
        {
          // �������� �� ��������� �������������
          resRow[p] = RootNodeDocId;
        }
      }
    }

    /// <summary>
    /// ������� ������ DocumentViewHandler ��� ���������.
    /// </summary>
    /// <returns>������ ������, ������������ �� DocumentViewHandler</returns>
    protected override EFPDocTreeView.IntDocumentViewHandler CreateDocumentViewHandler()
    {
      return new IntDocumentViewHandler(this); // // 10.06.2019 ���������������� �����
    }

    #endregion
  }
}
