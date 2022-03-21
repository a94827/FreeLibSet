// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.UICore;

// ����� ������� ��� ������ ���������� � �������������

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ������ ������ ���������� � ������������� � �������� DocSelectDialog � SubDocSelectDialog 
  /// </summary>
  public enum DocSelectionMode
  {
    /// <summary>
    /// ����������� ����� ������ ������ ��������� ��� ������������.
    /// ���� ����� ����� �� ���������
    /// </summary>
    Single,

    /// <summary>
    /// ����� ���������� ���������� ��� ������������� ���������� ���������� ����� ����� ��� ������� Shift/Control+���������.
    /// ���� ����� ������, ������ ���� ���� ������� ����������� �������� ������ ������ ��������� ��� ������.
    /// </summary>
    MultiSelect,

    /// <summary>
    /// ����� � ������� �������
    /// </summary>
    MultiCheckBoxes,

    /// <summary>
    /// ������������ ������ � ���������� �����������. � ���� ����� ��������� ��������� �� ������� ������.
    /// </summary>
    MultiList
  }

  /// <summary>
  /// ������ ������ ������ ��� ���������� ���������� ��������� ����
  /// </summary>
  public sealed class DocSelectDialog
  {
    #region �����������

    /// <summary>
    /// ������� ������ �� ���������� �� ���������
    /// </summary>
    /// <param name="docTypeUI">��������� ��� ���� ���������. ������ ������ ���� ������</param>
    public DocSelectDialog(DocTypeUI docTypeUI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;
      _SelectionMode = DocSelectionMode.Single;
      _DocIds = DataTools.EmptyIds;
      _CanBeEmpty = false;
      _DialogPosition = new EFPDialogPosition();

      //_DefaultGridConfigName = String.Empty;
      //_DefaultTreeConfigName = String.Empty;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ������� � ����������.
    /// �������� � ������������.
    /// �� ����� ���� null
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// ����� ������ ��������� Single, MultiSelect, MultiCheckBoxes, MultiList.
    /// �� ��������� - Single
    /// </summary>
    public DocSelectionMode SelectionMode
    {
      get { return _SelectionMode; }
      set
      {
        switch (value)
        {
          case DocSelectionMode.Single:
          case DocSelectionMode.MultiSelect:
          case DocSelectionMode.MultiCheckBoxes:
          case DocSelectionMode.MultiList:
            _SelectionMode = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private DocSelectionMode _SelectionMode;

    /// <summary>
    /// �������������� �������� ��� �������� SelectionMode.
    /// �������� true, ���� �������� ����� ���������� ����������
    /// </summary>
    public bool MultiSelect
    {
      get { return _SelectionMode != DocSelectionMode.Single; }
      set { _SelectionMode = value ? DocSelectionMode.MultiSelect : DocSelectionMode.Single; }
    }

    /// <summary>
    /// ����-�����. ������������� ���������� ��������� ��� MultiSelect=false
    /// </summary>
    public Int32 DocId
    {
      get
      {
        if (_DocIds.Length == 0)
          return 0;
        else
          return _DocIds[0];
      }
      set
      {
        if (value == 0)
          _DocIds = DataTools.EmptyIds;
        else
          _DocIds = new Int32[1] { value };
      }
    }

    /// <summary>
    /// ����-�����. �������������� ��������� ���������� ��� MultiSelect=true.
    /// </summary>
    public Int32[] DocIds
    {
      get { return _DocIds; }
      set
      {
        if (value == null)
          _DocIds = DataTools.EmptyIds;
        else
          _DocIds = value;
      }
    }
    private Int32[] _DocIds;

    /// <summary>
    /// ��������� ����� �������.
    /// �� ���������: "����� ��������� XXX" ��� "����� ���������� XXX" ��� MultiSelect=true
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
        {
          if (MultiSelect)
            return "����� ���������� \"" + DocTypeUI.DocType.PluralTitle + "\"";
          else
            return "����� ��������� \"" + DocTypeUI.DocType.SingularTitle + "\"";
        }
        else
          return _Title;
      }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// ���������� ������� � ������� ������ "���", ����� ����� ���� ���������� DocId=0.
    /// �� ��������� - false;
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// ����� ������������� �������� ���������� ���������.
    /// �������� null (�� ���������) �������� � ������������� �������� ������
    /// ������������� ������������� ��������.
    /// ��������� ������ ��� FixedDocIds=null.
    /// </summary>
    public GridFilters Filters { get { return _Filters; } set { _Filters = value; } }
    private GridFilters _Filters;

    /// <summary>
    /// ������ ������������� ��������������� ��� ������.
    /// ���� null (�� ���������), �� ����� �������������� � ������� ����� DocTableViewForm, ������� �������� ��������.
    /// ���� �������� �����������, �� ����� �������������� � ������� ���������� �����
    /// </summary>
    public IdList FixedDocIds
    {
      get { return _FixedDocIds; }
      set { _FixedDocIds = value; }
    }
    private IdList _FixedDocIds;

    /// <summary>
    /// ������� ������, ������������ ��������� �������� ��� �������� ��������� ������ ����� �������.
    /// ���� �����, �� ������ ���������� ��� ��������, ����� �������� ������ ������� ��������, ���� ������ ������� � �������� Filters,
    /// �.�. ���� ������� �� ������������ ��� ��������� ��������.
    /// �� ��������� - null.
    /// ��������� ������ ��� FixedDocIds=null.
    /// </summary>
    public DocumentViewHandler EditorCaller { get { return _EditorCaller; } set { _EditorCaller = value; } }
    private DocumentViewHandler _EditorCaller;

    /// <summary>
    /// ������� ������ ����� �������.
    /// ����� ���� �������� ������������ ������, ���� ���� �������� ������ �� ����� ����� EFPDialogPosition.
    /// ������������ � �����������
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    // ���� �� ������, ��� ����
    /*
    /// <summary>
    /// ��� ������������� ��������� ���������� ���
    /// </summary>
    public string DefaultGridConfigName { get { return _DefaultGridConfigName; } set { _DefaultGridConfigName = value; } }
    private string _DefaultGridConfigName;

    public string DefaultTreeConfigName { get { return _DefaultTreeConfigName; } set { _DefaultTreeConfigName = value; } }
    private string _DefaultTreeConfigName;

    public object UserInitData { get { return _UserInitData; } set { _UserInitData = value; } }
    private object _UserInitData;
     * */

    #endregion

    #region ����� ����� �������

    #region �������� ����� ShowDialog()

    /// <summary>
    /// ������� ���� �������.
    /// ���������� Ok, ���� ������������ ������ �����, � ��� ����� ����� ������ "���"
    /// </summary>
    /// <returns></returns>
    public DialogResult ShowDialog()
    {
      if (FixedDocIds == null)
      {
        if (SelectionMode == DocSelectionMode.MultiList)
          return ShowDialogMultiList();
        else if (CanBeUsedGroupDialog())
          return ShowDialogForGroups();
        else
          return ShowDialogNormal();
      }
      else
        return ShowDialogFixedIds();
    }

    #endregion

    #region ������� ������ � DocTableViewForm

    private DialogResult ShowDialogNormal()
    {
      DialogResult res = DialogResult.Cancel;

      using (DocTableViewForm form = new DocTableViewForm(DocTypeUI, GetDocTableViewMode(SelectionMode)))
      {
        form.Text = Title;
        form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            form.SelectedDocIds = DocIds;
          else
            form.CurrentDocId = DocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "�� ������� ���������� ��������� ��������� \"" + DocTypeUI.DocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "�� ������� ���������� ��������� �������� \"" + DocTypeUI.DocType.SingularTitle + "\" � DocId=" + DocId.ToString());
        }
        form.ExternalFilters = Filters;
        form.ExternalEditorCaller = EditorCaller;
        /*
        if (Form.ViewProvider.DocGridView != null)
        {
          Form.ViewProvider.DocGridView.DefaultConfigName = DefaultGridConfigName;
          Form.ViewProvider.DocGridView.UserInitData = UserInitData;
        }
        if (Form.ViewProvider.DocTreeView != null)
        {
          Form.ViewProvider.DocTreeView.DefaultConfigName = DefaultTreeConfigName;
          Form.ViewProvider.DocTreeView.UserInitData = UserInitData;
        }
          */
        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = form.SelectedDocIds;
            else
              DocId = form.CurrentDocId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
    }

    internal static DocTableViewMode GetDocTableViewMode(DocSelectionMode selectionMode)
    {
      switch (selectionMode)
      {
        case DocSelectionMode.Single: return DocTableViewMode.SelectSingle;
        case DocSelectionMode.MultiSelect:
        case DocSelectionMode.MultiList: return DocTableViewMode.SelectMulti;
        case DocSelectionMode.MultiCheckBoxes: return DocTableViewMode.SelectMultiWithFlags;
        default:
          throw new ArgumentException();
      }
    }

    #endregion

    #region ������ � ��������� �������������� ���������� EFPDocSelTextGridView

    private DialogResult ShowDialogMultiList()
    {
      DialogResult res = DialogResult.Cancel;
      using (OKCancelGridForm form = new OKCancelGridForm())
      {
        form.Text = Title;
        form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_MultiList";

        EFPDocSelTextGridView gh = new EFPDocSelTextGridView(form.ControlWithToolBar, DocTypeUI);
        if (Filters != null)
          gh.Filters = Filters;
        gh.CommandItems.CanEditFilters = false; // 09.07.2019
        gh.ExternalEditorCaller = EditorCaller;
        gh.Ids = DocIds;

        gh.CanBeEmpty = CanBeEmpty;
        gh.OrderMode = EFPDocSelGridViewOrderMode.Manual;

        if (EFPApp.ShowDialog(form, false, DialogPosition) == DialogResult.OK)
        {
          DocIds = gh.Ids;
          res = DialogResult.OK;
        }
      }

      return res;
    }

    #endregion

    #region ������ ������ �� �������������� ������

    private DialogResult ShowDialogFixedIds()
    {
      DialogResult res = DialogResult.Cancel;
      using (OKCancelGridForm form = new OKCancelGridForm())
      {
        form.Text = Title;
        form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_FixedIds";
        form.NoButtonProvider.Visible = CanBeEmpty;

        EFPDocGridView gh = new EFPDocGridView(form.ControlWithToolBar, DocTypeUI);
        gh.FixedDocIds = FixedDocIds;
        gh.Validating += new UIValidatingEventHandler(SelectSingleDoc_ValidateNotEmpty);
        gh.Control.MultiSelect = MultiSelect;
        gh.CommandItems.EnterAsOk = true;

        try
        {
          if (MultiSelect)
            gh.SelectedIds = DocIds;
          else
            gh.CurrentId = DocId;
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "�� ������� ���������� ��������� �������� \"" + DocTypeUI.DocType.SingularTitle + "\" � DocId=" + DocId.ToString());
        }

        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = gh.SelectedIds;
            else
              DocId = gh.CurrentId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
    }

    private void SelectSingleDoc_ValidateNotEmpty(object sender, UIValidatingEventArgs args)
    {
      EFPDocGridView gh = (EFPDocGridView)sender;
      if (MultiSelect)
      {
        if (gh.SelectedRowCount == 0)
          args.SetError("��������� ������ ���� �������");
      }
      else
      {
        if (gh.CurrentId == 0)
          args.SetError("�������� ������ ���� ������");
      }
    }

    #endregion

    #region ������ ������ ������

    /// <summary>
    /// ���������� true, ���� ����� ������������ ������ ������ ������
    /// </summary>
    /// <returns></returns>
    private bool CanBeUsedGroupDialog()
    {
      if (!(_DocTypeUI is GroupDocTypeUI))
        return false;
      if (SelectionMode != DocSelectionMode.MultiSelect)
        return false;
      if (DocIds.Length > 0)
        return false; // ?? ����� ����, ����� ������� ������
      if (_Filters != null)
      {
        if (_Filters.Count != 0)
          return false;
      }

      return true;
    }

    private DialogResult ShowDialogForGroups()
    {
      GroupDocTypeUI docTypeUI2 = (GroupDocTypeUI)_DocTypeUI;

      if (!GroupGridFilterForm.PerformEdit(docTypeUI2, Title, docTypeUI2.ImageKey, ref docTypeUI2.LastGroupId, ref docTypeUI2.LastIncludeNestedGroups, CanBeEmpty, DialogPosition))
        return DialogResult.Cancel;
      IdList groupIds = docTypeUI2.GetAuxFilterGroupIdList(docTypeUI2.LastGroupId, docTypeUI2.LastIncludeNestedGroups);
      if (groupIds != null)
        DocIds = groupIds.ToArray();
      else
        DocIds = DataTools.EmptyIds;

      return DialogResult.OK;
    }

    #endregion

    #endregion
  }
  /// <summary>
  /// ������ ������ ������ ��� ���������� ������������� ��������� ����
  /// </summary>
  public sealed class SubDocSelectDialog
  {
    #region �����������

    /// <summary>
    /// ������� ������ �� ���������� �� ���������
    /// </summary>
    /// <param name="subDocTypeUI">��������� ��� ���� ������������. ������ ������ ���� ������</param>
    /// <param name="subDocs">������ �������������, �� ������� ����� ��������. ������ ������ ���� ������</param>
    public SubDocSelectDialog(SubDocTypeUI subDocTypeUI, DBxMultiSubDocs subDocs)
    {
      if (subDocTypeUI == null)
        throw new ArgumentNullException("subDocTypeUI");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");

      _SubDocTypeUI = subDocTypeUI;
      _SubDocs = subDocs;
      _SelectionMode = DocSelectionMode.Single;
      _SubDocIds = DataTools.EmptyIds;
      _CanBeEmpty = false;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ������� � ����������.
    /// �������� � ������������.
    /// �� ����� ���� null
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// ������ �������������, �� ������� ����� ��������.
    /// �������� � ������������.
    /// �� ����� ���� null
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// ����� ������ ��������� Single, MultiSelect ��� MultiCheckBoxes.
    /// ����� MultiList �� ��������������
    /// �� ��������� - Single.
    /// </summary>
    public DocSelectionMode SelectionMode
    {
      get { return _SelectionMode; }
      set
      {
        switch (value)
        {
          case DocSelectionMode.Single:
          case DocSelectionMode.MultiSelect:
          case DocSelectionMode.MultiCheckBoxes:
            _SelectionMode = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private DocSelectionMode _SelectionMode;

    /// <summary>
    /// �������������� �������� ��� �������� SelectionMode.
    /// �������� true, ���� �������� ����� ���������� ����������
    /// </summary>
    public bool MultiSelect
    {
      get { return _SelectionMode != DocSelectionMode.Single; }
      set { _SelectionMode = value ? DocSelectionMode.MultiSelect : DocSelectionMode.Single; }
    }

    /// <summary>
    /// ����-�����. ������������� ���������� ������������ ��� MultiSelect=false
    /// </summary>
    public Int32 SubDocId
    {
      get
      {
        if (_SubDocIds.Length == 0)
          return 0;
        else
          return _SubDocIds[0];
      }
      set
      {
        if (value == 0)
          _SubDocIds = DataTools.EmptyIds;
        else
          _SubDocIds = new Int32[1] { value };
      }
    }

    /// <summary>
    /// ����-�����. �������������� ��������� ���������� ��� MultiSelect=true.
    /// </summary>
    public Int32[] SubDocIds
    {
      get { return _SubDocIds; }
      set
      {
        if (value == null)
          _SubDocIds = DataTools.EmptyIds;
        else
          _SubDocIds = value;
      }
    }
    private Int32[] _SubDocIds;

    /// <summary>
    /// ��������� ����� �������.
    /// �� ���������: "����� ������������ XXX" ��� "����� ������������� XXX" ��� MultiSelect=true
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
        {
          if (MultiSelect)
            return "����� ������������� \"" + SubDocTypeUI.SubDocType.PluralTitle + "\"";
          else
            return "����� ������������ \"" + SubDocTypeUI.SubDocType.SingularTitle + "\"";
        }
        else
          return _Title;
      }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// ���������� ������� � ������� ������ "���", ����� ����� ���� ���������� SubDocId=0.
    /// �� ��������� - false;
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    ///// <summary>
    ///// ����� ������������� �������� ���������� ���������.
    ///// �������� null (�� ���������) �������� � ������������� �������� ������
    ///// ������������� ������������� ��������.
    ///// ��������� ������ ��� FixedDocIds=null.
    ///// </summary>
    //public GridFilters Filters { get { return _Filters; } set { _Filters = value; } }
    //private GridFilters _Filters;

    ///// <summary>
    ///// ������ ������������� ��������������� ��� ������.
    ///// ���� null (�� ���������), �� ����� �������������� � ������� ����� DocTableViewForm, ������� �������� ��������.
    ///// ���� �������� �����������, �� ����� �������������� � ������� ���������� �����
    ///// </summary>
    //public IdList FixedDocIds
    //{
    //  get { return _FixedDocIds; }
    //  set { _FixedDocIds = value; }
    //}
    //private IdList _FixedDocIds;

    /// <summary>
    /// ������� ������ ����� �������.
    /// ����� ���� �������� ������������ ������, ���� ���� �������� ������ �� ����� ����� EFPDialogPosition.
    /// ������������ � �����������
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region ����� ����� �������

    /// <summary>
    /// ������� ���� �������.
    /// ���������� Ok, ���� ������������ ������ �����, � ��� ����� ����� ������ "���"
    /// </summary>
    /// <returns></returns>
    public DialogResult ShowDialog()
    {
      DialogResult res = DialogResult.Cancel;
      using (SubDocTableViewForm form = new SubDocTableViewForm(SubDocTypeUI, DocSelectDialog.GetDocTableViewMode(SelectionMode), SubDocs))
      {
        form.Text = Title;
        form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            form.SelectedSubDocIds = SubDocIds;
          else
            form.CurrentSubDocId = SubDocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "�� ������� ���������� ��������� ������������ \"" + SubDocTypeUI.SubDocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "�� ������� ���������� ��������� ����������� \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" � SubDocId=" + SubDocId.ToString());
        }
        //Form.ExternalFilters = Filters;

        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              SubDocIds = form.SelectedSubDocIds;
            else
              SubDocId = form.CurrentSubDocId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            SubDocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
    }

    #endregion
  }
}
