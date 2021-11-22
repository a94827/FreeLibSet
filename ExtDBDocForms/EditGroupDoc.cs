// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Редактор группы.
  /// Возможно, будет убрано
  /// </summary>
  internal partial class EditGroupDoc : Form
  {
    #region Конструктор формы

    public EditGroupDoc()
    {
      InitializeComponent();
    }

    #endregion

    #region Редактор

    #region InitDocEditForm

    private DocumentEditor _Editor;

    public static void InitDocEditForm(object sender, InitDocEditFormEventArgs args)
    {
      EditGroupDoc Form = new EditGroupDoc();

      Form._Editor = args.Editor;

      Form.AddPage1(args);
    }

    #endregion

    #region Страница 1 (общие)

    private EFPTextBox efpName;

    private void AddPage1(InitDocEditFormEventArgs args)
    {
      DocEditPage Page = args.AddPage("Группа", MainPanel1);
      Page.ImageKey = args.Editor.DocTypeUI.ImageKey;

      //Page.HelpContext = "BuxBase.chm::CompanyEdit.htm#Общие";

      GroupDocTypeUI dtui=(GroupDocTypeUI)(args.Editor.DocTypeUI);

      efpName = new EFPTextBox(Page.BaseProvider, edName);
      efpName.CanBeEmpty = false;
      args.AddText(efpName, dtui.NameColumnName, false);

      EFPDocComboBox efpParent = new EFPDocComboBox(Page.BaseProvider, cbParent, dtui);
      efpParent.CanBeEmpty = true;
      args.AddRef(efpParent, "ParentId", true);
    }

    #endregion

    #endregion
  }
}