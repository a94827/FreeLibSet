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
      EditGroupDoc form = new EditGroupDoc();

      form._Editor = args.Editor;

      form.AddPage1(args);
    }

    #endregion

    #region Страница 1 (общие)

    private EFPTextBox efpName;

    private void AddPage1(InitDocEditFormEventArgs args)
    {
      DocEditPage page = args.AddPage("Группа", MainPanel1);
      page.ImageKey = args.Editor.DocTypeUI.ImageKey;

      //Page.HelpContext = "BuxBase.chm::CompanyEdit.htm#Общие";

      GroupDocTypeUI dtui = (GroupDocTypeUI)(args.Editor.DocTypeUI);

      efpName = new EFPTextBox(page.BaseProvider, edName);
      efpName.CanBeEmpty = false;
      args.AddText(efpName, dtui.NameColumnName, false);

      EFPDocComboBox efpParent = new EFPDocComboBox(page.BaseProvider, cbParent, dtui);
      efpParent.CanBeEmpty = true;
      args.AddRefToParent(efpParent, "ParentId", true);
    }

    #endregion

    #endregion
  }
}