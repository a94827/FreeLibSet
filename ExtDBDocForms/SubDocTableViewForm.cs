using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.Logging;
using FreeLibSet.Controls;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ����� ��� ��������� ��� ������ �������������.
  /// �������� ��������� ��������, ������������� �������� (���� �������������� ����� �������������)
  /// � ������ (� ������ ������)
  /// </summary>
  public partial class SubDocTableViewForm : Form
  {
    #region ����������� � Disposing

    /// <summary>
    /// ����������� ������������ � EFPSubDocTableView, ����� ��������� ���������� �������� �� ������������ ������
    /// </summary>
    internal SubDocTableViewForm()
    {
      InitializeComponent();
    }

    /// <summary>
    /// ������� �����
    /// </summary>
    /// <param name="subDocTypeUI">��������� ������������ ��� ������� � �������������</param>
    /// <param name="mode">����� ��������� ��� ������</param>
    /// <param name="subDocs">������ �������������</param>
    public SubDocTableViewForm(SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
    {
      InitializeComponent();

      base.Text = subDocTypeUI.SubDocType.PluralTitle;
      base.Icon = EFPApp.MainImageIcon(subDocTypeUI.TableImageKey);


      _FormProvider = new EFPFormProvider(this);

      _ViewProvider = new EFPSubDocTableView(this, subDocTypeUI, mode, subDocs);


      TheButtonPanel.Visible = (mode != DocTableViewMode.Browse);

      _FormProvider.ConfigSectionName = SubDocTypeName;

      _ViewProvider.SaveFormConfig = false; // ���� ������, �.�. ��������� ������ �� ��������� ���� �����
      _FormProvider.ConfigHandler.Sources.Add(new FormConfigurable(this));
    }

    /// <summary>
    /// ���������� �������� �����
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected override void OnFormClosing(FormClosingEventArgs args)
    {
      _FormProvider.ConfigHandler.Changed[EFPConfigCategories.Form] = true; // ����� ����, �� ������

      base.OnFormClosing(args);
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ����������� �������
    /// </summary>
    public EFPSubDocTableView ViewProvider { get { return _ViewProvider; } }
    private EFPSubDocTableView _ViewProvider;

    /// <summary>
    /// ���������� �����
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// ������������� ��������. ���� ��� ���� ������������ �� ���������� ���� ParentId, 
    /// �������� �������� null
    /// </summary>
    public EFPSubDocTreeView SubDocTreeView { get { return ViewProvider.SubDocTreeView; } }

    /// <summary>
    /// �������� ��������� �������� ����������
    /// </summary>
    public EFPSubDocGridView SubDocGridView { get { return ViewProvider.SubDocGridView; } }

    /// <summary>
    /// �������� �������� (����������� �������� ���������� DocView)
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return ViewProvider.FilterView; } }

    /// <summary>
    /// ��������� ������������ ��� ������� � �������������
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return ViewProvider.SubDocTypeUI; } }

    /// <summary>
    /// ��������� ������������ ��� ������� � �������������
    /// </summary>
    public string SubDocTypeName { get { return ViewProvider.SubDocTypeName; } }

    /// <summary>
    /// ����� ������ �����
    /// �� ��� ������ �����������
    /// </summary>
    public DocTableViewMode Mode { get { return ViewProvider.Mode; } }

    /// <summary>
    /// ������������� � ������ ������ 
    /// ���� ����������� � true, �� �������� ������ "���"
    /// </summary>
    public bool CanBeEmpty
    {
      get { return ViewProvider.CanBeEmpty; }
      set
      {
        ViewProvider.CanBeEmpty = value;
        TheNoButton.Visible = value;
      }
    }

    /// <summary>
    /// ���� ��� �������� �����������, �� ������ ��������, ���������� �������������,
    /// ����� ������������ ��� �������. ������������ �� ����� �� �������������
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return ViewProvider.ExternalFilters; }
      set { ViewProvider.ExternalFilters = value; }
    }

    /// <summary>
    /// ����� �������� ������� �����, ����� ��� ������������ ������������ ������������� �������� � ������
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get { return ViewProvider.ActiveTab; }
      set { ViewProvider.ActiveTab = value; }
    }


    /// <summary>
    /// ������������� �������� ���������
    /// </summary>
    public Int32 CurrentSubDocId
    {
      get { return ViewProvider.CurrentSubDocId; }
      set { ViewProvider.CurrentSubDocId = value; }
    }


    /// <summary>
    /// �������������� ��������� ����������
    /// </summary>
    public Int32[] SelectedSubDocIds
    {
      get { return ViewProvider.SelectedSubDocIds; }
      set { ViewProvider.SelectedSubDocIds = value; }
    }
    #endregion

    #region ����������� ������

#if XXX
    private void FormOKButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void FormCancelButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.Cancel;
        Close();
      }
    }

    private void FormNoButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.No;
        Close();
      }
    }
#endif

    #endregion

    #region ����������� �����

    /// <summary>
    /// ���������� ������� ��������� �����
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected override void OnActivated(EventArgs args)
    {
      base.OnActivated(args);

      try
      {
        // ��� � ���� ���������. ���� �������� ������ ActiveControl � Select() �
        // ������ �����, �� ���������� �����. ��������, ���� ������� � VisibleChanged
        // (��� VisibleEx=true), �� ������ ��� ���������, � ����� ��������� ���������
        // ����� ����� ������������ �� ������ ��������.
        // ���� ������� � ������������ (��� ���� �� �������), �� ��� ����: ��� ���������
        // ����� ����� �������� ������, ��� "��������" � ������ �������� ������ �����
        // ���� ������ ������� �� ������ ����������.
        //
        // ���, �������, ������, �� ��������. ��� ����� �� ������ � ������ ������� 
        // ������ MDI-���� �� ����������, ������� ������ ��������� �� ���������� �
        // ����� �� �������

        ViewProvider.ActiveTab = ActiveTab;
      }
      catch (Exception e)
      {
        // �� ����� �������� EFPApp.ShowException(), ����� �� ������������� 
        // ��������� ���������
        LogoutTools.LogoutException(e, "SubDocTableViewForm.OnActivated()");
      }
    }

    #endregion

    #region ���������� �������� ���� �����

    /// <summary>
    /// �� ������� ������������� IEFPConfigurable ��������������� � DocTableViewForm
    /// </summary>
    private class FormConfigurable : IEFPConfigurable
    {
      #region �����������

      public FormConfigurable(SubDocTableViewForm form)
      {
        _Form = form;
      }

      private SubDocTableViewForm _Form;

      #endregion

      #region IEFPConfigurable Members

      public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
      {
        categories.Add(EFPConfigCategories.Form);
      }

      public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          _Form.ViewProvider.WriteFormConfigPart(cfg);
        }
      }

      public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          _Form.ViewProvider.ReadFormConfigPart(cfg);
        }
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// ��������� ���������� ������������ �������� ��� ���������� ��������� ��� ������ �������������.
  /// ����� �������� �������, ����� ��������� �������� �������� � �������� � ���� ������.
  /// </summary>
  public class EFPSubDocTableView : EFPControl<Control>
  {
    #region ������������

    internal EFPSubDocTableView(SubDocTableViewForm form, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
      : base(form.FormProvider, form.ControlPanel, false)
    {
      Init(form, form.FormProvider, subDocTypeUI, mode, subDocs);
    }

    /// <summary>
    /// ������� ��������� ��� ���������� ��������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="parentControl">������ ������ ��� ���������� ���������� ��������</param>
    /// <param name="subDocTypeUI">��������� ������� � �������������</param>
    /// <param name="mode">����� ��������� ��� ������ �������������</param>
    /// <param name="subDocs">������ �������������</param>
    public EFPSubDocTableView(EFPBaseProvider baseProvider, Control parentControl, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
      : base(baseProvider, parentControl, false)
    {
      if (parentControl.HasChildren)
        throw new ArgumentException("� ������ �� ������ ���� ����������� ���������", "parentControl");

      SubDocTableViewForm DummyForm = new SubDocTableViewForm();

      WinFormsTools.MoveControls(DummyForm.ControlPanel, parentControl);

      Init(DummyForm, baseProvider, subDocTypeUI, mode, subDocs);

      base.InitConfigHandler();
      base.ConfigSectionName = SubDocTypeName; // ����� InitConfigHandler
    }


    private void Init(SubDocTableViewForm form, EFPBaseProvider baseProvider, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
    {
      if (subDocTypeUI == null)
        throw new ArgumentNullException("subDocTypeUI");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
      if (!Object.ReferenceEquals(subDocTypeUI.SubDocType, subDocs.SubDocType))
        throw new ArgumentException("SubDocTypeUI � SubDocs ��������� � ������ �������� SubDocType", "subDocs");

      _Form = form;


      if (!String.IsNullOrEmpty(subDocTypeUI.SubDocType.TreeParentColumnName))
      {
        _TheTabControl = new TabControl();
        _TheTabControl.Dock = DockStyle.Fill;
        _TheTabControl.ImageList = EFPApp.MainImages;
        form.MainPanel.Controls.Add(_TheTabControl);

        TabPage tpTree = new TabPage("������");
        _TheTabControl.Controls.Add(tpTree);
        tpTree.ImageKey = "TreeView";

        _SubDocTree = new TreeViewAdv();
        _SubDocTree.Dock = DockStyle.Fill;
        _SubDocTree.Name = "SubDocTree";
        tpTree.Controls.Add(_SubDocTree);

        _SubDocTreeSpeedPanel = new Panel();
        _SubDocTreeSpeedPanel.Dock = DockStyle.Top;
        tpTree.Controls.Add(_SubDocTreeSpeedPanel);

        TabPage tpTable = new TabPage("�������");
        _TheTabControl.Controls.Add(tpTable);
        tpTable.ImageKey = "Table";

        _SubDocGrid = new DataGridView();
        _SubDocGrid.Dock = DockStyle.Fill;
        _SubDocGrid.Name = "SubDocGrid";
        tpTable.Controls.Add(_SubDocGrid);

        _SubDocGridSpeedPanel = new Panel();
        _SubDocGridSpeedPanel.Dock = DockStyle.Top;
        tpTable.Controls.Add(_SubDocGridSpeedPanel);
      }
      else
      {
        _SubDocGrid = new DataGridView();
        _SubDocGrid.Dock = DockStyle.Fill;
        _SubDocGrid.Name = "SubDocGrid";
        form.MainPanel.Controls.Add(_SubDocGrid);

        _SubDocGridSpeedPanel = new Panel();
        _SubDocGridSpeedPanel.Dock = DockStyle.Top;
        form.MainPanel.Controls.Add(_SubDocGridSpeedPanel);
      }


      if (!String.IsNullOrEmpty(subDocTypeUI.SubDocType.TreeParentColumnName))
      {
        _SubDocTreeView = new EFPSubDocTreeView(baseProvider, _SubDocTree, subDocs, subDocTypeUI.UI);
        _SubDocTreeView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
        _SubDocTreeView.ToolBarPanel = _SubDocTreeSpeedPanel;
      }

      _SubDocGridView = new EFPSubDocGridView(baseProvider, _SubDocGrid, subDocs, subDocTypeUI.UI);
      _SubDocGridView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
      _SubDocGridView.ToolBarPanel = _SubDocGridSpeedPanel;

      if (mode == DocTableViewMode.SelectSingle)
      {
        _SubDocGridView.Control.MultiSelect = false;
        _SubDocGridView.CanMultiEdit = false;
        if (_SubDocTreeView != null)
        {
          _SubDocTreeView.Control.SelectionMode = TreeViewAdvSelectionMode.Single;
          _SubDocTreeView.CanMultiEdit = false;
        }
      }

      if (_SubDocTreeView != null)
        // �������������
        new EFPDBxViewSync(_SubDocGridView, _SubDocTreeView);


      _FilterView = new EFPGridFilterGridView(_SubDocGridView, form.FilterGrid);
      _Mode = mode;

      _SaveFormConfig = true;
    }

    #endregion

    #region ��������

    private SubDocTableViewForm _Form;

    /// <summary>
    /// ����������, ���� ���� ����������� ��������
    /// </summary>
    private TabControl _TheTabControl;

    private FreeLibSet.Controls.TreeViewAdv _SubDocTree;
    private Panel _SubDocTreeSpeedPanel;

    private DataGridView _SubDocGrid;
    private Panel _SubDocGridSpeedPanel;

    /// <summary>
    /// ������������� ��������. ���� ��� ���� ������������ �� ���������� ���� ParentId, 
    /// �������� �������� null
    /// </summary>
    public EFPSubDocTreeView SubDocTreeView { get { return _SubDocTreeView; } }
    private EFPSubDocTreeView _SubDocTreeView;

    /// <summary>
    /// �������� ��������� �������� ����������
    /// </summary>
    public EFPSubDocGridView SubDocGridView { get { return _SubDocGridView; } }
    private EFPSubDocGridView _SubDocGridView;

    /// <summary>
    /// �������� �������� (����������� �������� ���������� DocView)
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return _FilterView; } }
    private EFPGridFilterGridView _FilterView;

    /// <summary>
    /// ��������� ������� � �������������
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocGridView.SubDocTypeUI; } }

    /// <summary>
    /// ��� ���� ��������������� �������������
    /// </summary>
    public string SubDocTypeName { get { return _SubDocGridView.SubDocType.Name; } }

    /// <summary>
    /// ����� ������ �����
    /// �� ��� ������ �����������
    /// </summary>
    public DocTableViewMode Mode { get { return _Mode; } }
    private DocTableViewMode _Mode;

    /// <summary>
    /// ������������� � ������ ������ 
    /// ���� ����������� � true, �� �������� ������ "���"
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// ���� ��� �������� �����������, �� ������ ��������, ���������� �������������,
    /// ����� ������������ ��� �������. ������������ �� ����� �� �������������
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return _ExternalFilters; }
      set
      {
        _ExternalFilters = value;
        if (value != null)
        {
          _SubDocGridView.Filters = value;
          _SubDocGridView.CommandItems.CanEditFilters = false;
        }
      }
    }
    private GridFilters _ExternalFilters;

    ///// <summary>
    ///// ���� ��� ������� �����������, �� ��� ����� ������� ����� ����, ��� �����
    ///// ��������� �������� ��������, ����������� � ������������
    ///// </summary>
    //public event InitEFPDBxViewEventHandler InitFilters;


    /// <summary>
    /// ����� �������� ������� �����, ����� ��� ������������ ������������ ������������� �������� � ������
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get
      {
        if (_SubDocTreeView == null)
          return DocViewFormActiveTab.Grid;
        else
        {
          if (_TheTabControl.SelectedIndex <= 0)
            return DocViewFormActiveTab.Tree;
          else
            return DocViewFormActiveTab.Grid;
        }
      }
      set
      {
        switch (value)
        {
          case DocViewFormActiveTab.Tree:
            if (_SubDocTreeView == null)
              throw new InvalidOperationException("����� �� �������� ������� �������������� ���������");
            else
              _TheTabControl.SelectedIndex = 0;
            break;
          case DocViewFormActiveTab.Grid:
            if (_SubDocTreeView != null)
              _TheTabControl.SelectedIndex = 1;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }


    /// <summary>
    /// ������������� �������� ���������
    /// </summary>
    public Int32 CurrentSubDocId
    {
      get
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: return SubDocGridView.CurrentId;
          case DocViewFormActiveTab.Tree: return SubDocTreeView.CurrentId;
          default:
            throw new BugException();
        }
      }
      set
      {
        // 16.11.2017
        SubDocGridView.CurrentId = value;
        if (SubDocTreeView != null)
          SubDocTreeView.CurrentId = value;
      }
    }


    /// <summary>
    /// �������������� ��������� ����������
    /// </summary>
    public Int32[] SelectedSubDocIds
    {
      get
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: return SubDocGridView.SelectedIds;
          case DocViewFormActiveTab.Tree: return SubDocTreeView.SelectedIds;
          default:
            throw new BugException();
        }
      }
      set
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: SubDocGridView.SelectedIds = value; break;
          case DocViewFormActiveTab.Tree: SubDocTreeView.SelectedIds = value; break;
        }
      }
    }
    #endregion

    #region ����������� �����

    /// <summary>
    /// ����� ���������� ��� ������ ��������� �������� �� ������
    /// </summary>
    protected override void OnCreated()
    {
      if (!String.IsNullOrEmpty(Control.Name))
      {
        if (_SubDocTree != null)
          _SubDocTree.Name = Control.Name + _SubDocTree.Name;
        _SubDocGrid.Name = Control.Name + _SubDocGrid.Name;
      }

      base.OnCreated();
    }

    //protected override void OnActivated(EventArgs Args)
    //{
    //  base.OnActivated(Args);

    //  try
    //  {
    //    // ��� � ���� ���������. ���� �������� ������ ActiveControl � Select() �
    //    // ������ �����, �� ���������� �����. ��������, ���� ������� � VisibleChanged
    //    // (��� VisibleEx=true), �� ������ ��� ���������, � ����� ��������� ���������
    //    // ����� ����� ������������ �� ������ ��������.
    //    // ���� ������� � ������������ (��� ���� �� �������), �� ��� ����: ��� ���������
    //    // ����� ����� �������� ������, ��� "��������" � ������ �������� ������ �����
    //    // ���� ������ ������� �� ������ ����������.
    //    //
    //    // ���, �������, ������, �� ��������. ��� ����� �� ������ � ������ ������� 
    //    // ������ MDI-���� �� ����������, ������� ������ ��������� �� ���������� �
    //    // ����� �� �������

    //    switch (ActiveTab)
    //    {
    //      case DocViewFormActiveTab.Grid: ActiveControl = SubDocGrid; break;
    //      case DocViewFormActiveTab.Tree: ActiveControl = SubDocTree; break;
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    // �� ����� �������� EFPApp.ShowException(), ����� �� ������������� 
    //    // ��������� ���������
    //    LogoutTools.LogoutException(e, "DocTableViewForm.OnActivated()");
    //  }
    //}

    /// <summary>
    /// ����������, ����� ����� � ����������� ��������� �����������.
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (SaveFormConfig)
        ConfigHandler.Changed[EFPConfigCategories.Form] = true;

      base.OnSaveConfig();
    }

    /// <summary>
    /// ��������� ������� ���������� ������������, ���� �������� CanBeEmpty=false.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == EFPValidateState.Error)
        return;

      switch (Mode)
      {
        case DocTableViewMode.SelectSingle:
          if (!CanBeEmpty)
          {
            if (CurrentSubDocId == 0)
              base.SetError("����������� �� ������");
          }
          break;
      }
    }

    /// <summary>
    /// ������������� ��������� ������ ��� ���������� ���������� ������������
    /// </summary>
    protected override void InitControlColors()
    {
      // 07.12.2018 �� ���� ������������ ������, ����� ������������ ������� ������ � �������� ��������
    }

    #endregion

    #region ���������� ������������

    /// <summary>
    /// ���� �� ��������� ������������ � ������ "Form".
    /// ������������ �������� "ActiveTab".
    /// ���� �������� ����������� � true (�� ���������), �� ������� ����� ��� ��������� ���� ������.
    /// ���� �������� ������� � false, �� ��������������, ��� ��������� ������ ������������ �� ������ �����.
    /// � ���� ������ ����� ������������ ������ WriteFormConfigPart() � ReadFormConfigPart().
    /// �������� ����� ��������������� ������ �� ������ �������� �� �����
    /// </summary>
    public bool SaveFormConfig
    {
      get { return _SaveFormConfig; }
      set
      {
        CheckHasNotBeenCreated();
        _SaveFormConfig = value;
      }
    }
    private bool _SaveFormConfig;

    /// <summary>
    /// ��������� � ������ ��������� "Form"
    /// </summary>
    /// <param name="categories">������ ��� ���������� ���������</param>
    /// <param name="rwMode">������ ��� ������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);
      if (SaveFormConfig)
        categories.Add(EFPConfigCategories.Form);
    }

    /// <summary>
    /// ��������� ������ ������ ������������ "Form", ���� �������� SaveFormConfig=true.
    /// ��� ����� ���������� ����� WriteFormConfigPart().
    /// </summary>
    /// <param name="category">���������</param>
    /// <param name="cfg">������ ������������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.WriteConfigPart(category, cfg, actionInfo);
      switch (category)
      {
        case EFPConfigCategories.Form:
          if (SaveFormConfig)
            WriteFormConfigPart(cfg);
          break;
      }
    }

    /// <summary>
    /// ��������� ������ ������ ������������ "Form", ���� �������� SaveFormConfig=true.
    /// ��� ����� ���������� ����� ReadFormConfigPart().
    /// </summary>
    /// <param name="category">��������� ������������ ������</param>
    /// <param name="cfg">������ ������������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.ReadConfigPart(category, cfg, actionInfo);
      switch (category)
      {
        case EFPConfigCategories.Form:
          if (SaveFormConfig)
            ReadFormConfigPart(cfg);
          break;
      }
    }

    /// <summary>
    /// ��������� ������ ������ ������������ "Form".
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public void WriteFormConfigPart(CfgPart cfg)
    {
      if (SubDocTreeView != null)
      {
        if (ActiveTab == DocViewFormActiveTab.Grid)
          cfg.SetString("ActiveTab", "Grid");
        else
          cfg.SetString("ActiveTab", "Tree");
      }
    }

    /// <summary>
    /// ��������� ������ ������ ������������ "Form".
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public void ReadFormConfigPart(CfgPart cfg)
    {
      if (SubDocTreeView != null)
      {
        if (cfg.GetString("ActiveTab") == "Tree")
          ActiveTab = DocViewFormActiveTab.Tree;
        else
          ActiveTab = DocViewFormActiveTab.Grid;
      }
    }

    #endregion
  }
}