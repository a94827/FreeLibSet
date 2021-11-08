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
    #region Конструктор формы

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

    #region Поля

    public EFPGroupDocTreeView efpGroup;

    public EFPCheckBox efpIncludeNestedGroups;

    #endregion

    #region Статический метод установки фильтра

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
          Form.FormProvider.AddFormCheck(new UIValidatingEventHandler(Form.efpForm_ValidatingNoRoot));
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

    void efpForm_ValidatingNoRoot(object sender, UIValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;
      if (efpGroup.CurrentId == 0)
        args.SetError("Должна быть выбрана какая-либо группа, а не корневой узел");
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра или фильтр отчета по группе документов.
  /// Обычно используется только в отчетах, так как табличные просмотры с группами реализуются в DocTableViewForm без необходимости ручного создания фильтра по группе
  /// </summary>
  public class RefGroupDocGridFilter : RefGroupDocCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструкторы

    /// <summary>
    /// Создает фильтр 
    /// </summary>
    /// <param name="groupDocTypeUI">Интерфейс вида документов групп</param>
    /// <param name="groupRefColumnName">Ссылочное поле на группу</param>
    public RefGroupDocGridFilter(GroupDocTypeUI groupDocTypeUI, string groupRefColumnName)
      : base(groupDocTypeUI.UI.DocProvider, groupDocTypeUI.DocType, groupRefColumnName)
    {
      _GroupDocTypeUI = groupDocTypeUI;
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="ui">Интефрейс пользователя для базы данных</param>
    /// <param name="groupDocTypeName">Имя вида документов групп</param>
    /// <param name="groupRefColumnName">Ссылочное поле на группу</param>
    public RefGroupDocGridFilter(DBUI ui, string groupDocTypeName, string groupRefColumnName)
      : base(ui.DocProvider, groupDocTypeName, groupRefColumnName)
    {
      _GroupDocTypeUI = ui.DocTypes[groupDocTypeName] as GroupDocTypeUI;
      if (_GroupDocTypeUI == null)
        throw new ArgumentException("Вид документов \"" + groupDocTypeName + "\" не является деревом групп", "groupDocTypeName");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс вида документов групп
    /// </summary>
    public GroupDocTypeUI GroupDocTypeUI { get { return _GroupDocTypeUI; } }
    private GroupDocTypeUI _GroupDocTypeUI;

    #endregion

    #region IEFPGridFilter Members

    /// <summary>
    /// Текстовое представление для значения фильтра
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
            return "Документы без групп";
        }
        else
        {
          string s = _GroupDocTypeUI.GetTextValue(GroupId);
          if (IncludeNestedGroups)
          {
            if (AuxFilterGroupIdList.Count > 1)
            {
              s += " и вложенные группы";
            }
          }
          return s;
        }
      }
    }

    /// <summary>
    /// Значок для значения фильтра
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
    /// Показывает диалог установки фильтра
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