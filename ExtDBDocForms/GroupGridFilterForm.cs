using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  internal partial class GroupGridFilterForm : OKCancelForm
  {
    #region ����������� �����

    public GroupGridFilterForm(GroupDocTypeUI groupDocTypeUI)
    {
      InitializeComponent();

      efpGroup = new EFPGroupDocTreeView(FormProvider, tvGroup, groupDocTypeUI);
      efpGroup.CommandItems.EnterAsOk = true;
      efpIncludeNestedGroups = new EFPCheckBox(FormProvider, cbIncludeNestedGroups);

      efpGroup.IncludeNested = efpIncludeNestedGroups.Checked;
      efpIncludeNestedGroups.CheckedEx.ValueChanged += new EventHandler(efpIncludeNestedGroups_ValueChanged);

      base.NoButtonProvider.Visible = true;
      FormProvider.ConfigSectionName = "GroupGridFilterForm";

    }

    void efpIncludeNestedGroups_ValueChanged(object sender, EventArgs args)
    {
      efpGroup.IncludeNested = efpIncludeNestedGroups.Checked;
    }

    #endregion

    #region ����

    public EFPGroupDocTreeView efpGroup;

    public EFPCheckBox efpIncludeNestedGroups;

    #endregion

    #region ����������� ����� ��������� �������

    public static bool PerformEdit(GroupDocTypeUI groupDocTypeUI, string title, string imageKey, ref Int32 groupId, ref bool includeNestedGroups, bool canBeRoot, EFPDialogPosition dialogPosition)
    {
      using (GroupGridFilterForm Form = new GroupGridFilterForm(groupDocTypeUI))
      {
        Form.Text = title;
        if (!String.IsNullOrEmpty(imageKey))
          Form.Icon = EFPApp.MainImageIcon(imageKey);
        Form.efpGroup.CurrentId = groupId;
        Form.efpIncludeNestedGroups.Checked = includeNestedGroups;

        if (!canBeRoot)
        {
          Form.NoButtonProvider.Visible = false; // 18.06.2021
          Form.FormProvider.AddFormCheck(new EFPValidatingEventHandler(Form.efpForm_ValidatingNoRoot));
        }

        switch (EFPApp.ShowDialog(Form, false, dialogPosition))
        {
          case DialogResult.OK:
            groupId = Form.efpGroup.CurrentId;
            includeNestedGroups = Form.efpIncludeNestedGroups.Checked;
            return true;
          case DialogResult.No:
            groupId = 0;
            includeNestedGroups = true;
            return true;
          default:
            return false;
        }
      }
    }

    void efpForm_ValidatingNoRoot(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;
      if (efpGroup.CurrentId == 0)
        args.SetError("������ ���� ������� �����-���� ������, � �� �������� ����");
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ������ �� ������ ����������.
  /// ������ ������������ ������ � �������, ��� ��� ��������� ��������� � �������� ����������� � DocTableViewForm ��� ������������� ������� �������� ������� �� ������
  /// </summary>
  public class RefGroupDocGridFilter : RefGroupDocCommonFilter, IEFPGridFilterWithImageKey
  {
    #region ������������

    /// <summary>
    /// ������� ������ 
    /// </summary>
    /// <param name="groupDocTypeUI">��������� ���� ���������� �����</param>
    /// <param name="groupRefColumnName">��������� ���� �� ������</param>
    public RefGroupDocGridFilter(GroupDocTypeUI groupDocTypeUI, string groupRefColumnName)
      : base(groupDocTypeUI.UI.DocProvider, groupDocTypeUI.DocType, groupRefColumnName)
    {
      _GroupDocTypeUI = groupDocTypeUI;
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="ui">��������� ������������ ��� ���� ������</param>
    /// <param name="groupDocTypeName">��� ���� ���������� �����</param>
    /// <param name="groupRefColumnName">��������� ���� �� ������</param>
    public RefGroupDocGridFilter(DBUI ui, string groupDocTypeName, string groupRefColumnName)
      : base(ui.DocProvider, groupDocTypeName, groupRefColumnName)
    {
      _GroupDocTypeUI = ui.DocTypes[groupDocTypeName] as GroupDocTypeUI;
      if (_GroupDocTypeUI == null)
        throw new ArgumentException("��� ���������� \"" + groupDocTypeName + "\" �� �������� ������� �����", "groupDocTypeName");
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ���� ���������� �����
    /// </summary>
    public GroupDocTypeUI GroupDocTypeUI { get { return _GroupDocTypeUI; } }
    private GroupDocTypeUI _GroupDocTypeUI;

    #endregion

    #region IEFPGridFilter Members

    /// <summary>
    /// ��������� ������������� ��� �������� �������
    /// </summary>
    public string FilterText
    {
      get
      {
        if (GroupId == 0)
        {
          if (IncludeNestedGroups)
            return String.Empty;
          else
            return "��������� ��� �����";
        }
        else
        {
          string s = _GroupDocTypeUI.GetTextValue(GroupId);
          if (IncludeNestedGroups)
          {
            if (AuxFilterGroupIdList.Count > 1)
            {
              s += " � ��������� ������";
            }
          }
          return s;
        }
      }
    }

    /// <summary>
    /// ������ ��� �������� �������
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        if (GroupId == 0)
        {
          if (IncludeNestedGroups)
            return String.Empty;
          else
            return "No";
        }
        else
        {
          if (IncludeNestedGroups)
            return "TreeViewOpenFolder";
          else
            return "TreeViewClosedFolder";
        }
      }
    }

    /// <summary>
    /// ���������� ������ ��������� �������
    /// </summary>
    /// <returns></returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      Int32 GroupId2 = GroupId;
      bool IncludeNestedGroups2 = IncludeNestedGroups;
      if (GroupGridFilterForm.PerformEdit(GroupDocTypeUI, DisplayName, "Filter", ref GroupId2, ref IncludeNestedGroups2, true, dialogPosition))
      {
        GroupId = GroupId2;
        IncludeNestedGroups = IncludeNestedGroups2;
        return true;
      }
      else
        return false;
    }

    #endregion
  }
}