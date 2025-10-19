using FreeLibSet.Forms;
using FreeLibSet.Forms.Data;
using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormsDemo.DataTableEditDialogDemo
{
  public partial class TestDataTableEditDialogForm : Form
  {
    public TestDataTableEditDialogForm()
    {
      InitializeComponent();
    }

    #region Основной метод

    static DataTable _TheTable;

    internal static void PerformTest()
    {
      if (_TheTable == null)
      {
        _TheTable = new DataTable();
        _TheTable.Columns.Add("F1", typeof(string));
        _TheTable.Columns.Add("F2", typeof(Int32));
        _TheTable.Rows.Add("One", 1);
        _TheTable.Rows.Add("Two", 2);
        _TheTable.Rows.Add("Three", 3);
      }

      SimpleGridForm form1 = new SimpleGridForm();
      EFPDataGridView efpGr = new EFPDataGridView(form1);
      efpGr.Control.AutoGenerateColumns = false;
      efpGr.Columns.AddText("F1", true, "F1", 20, 5);
      efpGr.Columns.LastAdded.CanIncSearch = true;
      efpGr.Columns.AddInteger("F2", true, "F2", 8);
      efpGr.Columns.LastAdded.CanIncSearch = true;
      efpGr.Columns.LastAdded.Summable = true;
      efpGr.DisableOrdering();
      efpGr.Control.ReadOnly = true;
      efpGr.Control.MultiSelect = true;
      efpGr.CanMultiEdit = true;
      efpGr.CanInsertCopy = true;
      efpGr.EditData += EfpGr_EditData;
      efpGr.Control.DataSource = _TheTable.DefaultView;

      SimpleForm<ListBox> form2 = new SimpleForm<ListBox>();
      form2.TopMost = true;
      form2.Size = new Size(200, 250);
      _TheListBox = form2.ControlWithToolBar.Control;
      form2.Show();

      EFPApp.ShowDialog(form1, true);
      _TheListBox = null;
      form2.Visible = false;
      form2.Dispose();
    }

    private static void EfpGr_EditData(object sender, EventArgs args)
    {
      EFPDataGridView efpGr = (EFPDataGridView)sender;
      if (efpGr.State != UIDataState.Insert && efpGr.SelectedDataRows.Length == 0)
      {
        EFPApp.ShowTempMessage("There is no selected row");
        return;
      }
      DataTableEditDialog editor = new DataTableEditDialog(efpGr);

      editor.FormShown += Editor_FormShown;
      editor.InitEditForm += Editor_InitEditForm;
      editor.BeforeReadData += Editor_BeforeReadData;
      editor.AfterReadData += Editor_AfterReadData;
      editor.BeforeWriteData += Editor_BeforeWriteData;
      editor.AfterWriteData += Editor_AfterWriteData;
      editor.FormClosed += Editor_FormClosed;
      editor.PageShow += Editor_PageShow;

      DialogResult res = editor.ShowDialog();
      AddMessage("DBxDataTableExtEditor finished. DialogResult=" + res.ToString());
    }

    private static void Editor_PageShow(object sender, ExtEditPageEventArgs args)
    {
      AddMessage("PageShow");
    }

    private static void Editor_FormClosed(object sender, EventArgs args)
    {
      AddMessage("FormClosed");
    }

    private static void Editor_AfterWriteData(object sender, CancelEventArgs args)
    {
      AddMessage("AfterWriteData");
    }

    private static void Editor_BeforeWriteData(object sender, CancelEventArgs args)
    {
      AddMessage("BeforeWriteData");
    }

    private static void Editor_AfterReadData(object sender, CancelEventArgs args)
    {
      AddMessage("AfterReadData");
    }

    private static void Editor_BeforeReadData(object sender, CancelEventArgs args)
    {
      AddMessage("BeforeReadData");
    }

    private static void Editor_FormShown(object sender, EventArgs args)
    {
      AddMessage("FormShown");
    }

    private static void Editor_InitEditForm(object sender, DBxExtValuesDialogInitEventArgs args)
    {
      AddMessage("InitEditForm");

      TestDataTableEditDialogForm templateForm = new TestDataTableEditDialogForm();

      ExtEditPage page = args.Dialog.Pages.Add(templateForm.Panel1);
      page.Title = "Page 1";
      page.ImageKey = "Properties";

      EFPTextBox efpF1 = new EFPTextBox(page.BaseProvider, templateForm.ed1);
      args.AddText(efpF1, "F1", false);

      EFPInt32EditBox efpF2 = new EFPInt32EditBox(page.BaseProvider, templateForm.ed2);
      args.AddInt32(efpF2, "F2", true);
    }

    #endregion

    #region Вывод диагностики

    private static ListBox _TheListBox;

    internal static void AddMessage(string s)
    {
      if (_TheListBox == null)
        return;

      int p = _TheListBox.Items.Add(s);
      _TheListBox.SelectedIndex = p;
    }

    #endregion
  }
}
