using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Models.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormsDemo.EFPDataViewDemo
{
  public partial class EFPDataViewParamsForm : Form
  {
    #region Конструктор формы

    public EFPDataViewParamsForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpReadOnly = new EFPCheckBox(efpForm, cbReadOnly);
      efpCanInsert = new EFPCheckBox(efpForm, cbCanInsert);
      efpCanInsertCopy = new EFPCheckBox(efpForm, cbCanInsertCopy);
      efpCanDelete = new EFPCheckBox(efpForm, cbCanDelete);
      efpCanEdit = new EFPCheckBox(efpForm, cbCanEdit);
      efpCanView = new EFPCheckBox(efpForm, cbCanView);
      efpMultiSelect = new EFPCheckBox(efpForm, cbMultiSelect);
      efpCanMultiEdit = new EFPCheckBox(efpForm, cbCanMultiEdit);
      efpEnterKeyMode = new EFPListComboBox(efpForm, cbEnterKeyMode);
      efpEnterKeyMode.Control.Items.AddRange(Enum.GetNames(typeof(EFPDataViewEnterKeyMode)));

      ReadValues();
    }

    private void ReadValues()
    {
      efpReadOnly.Checked = ReadOnly;
      efpCanInsert.Checked = CanInsert;
      efpCanInsertCopy.Checked = CanInsertCopy;
      efpCanDelete.Checked = CanDelete;
      efpCanEdit.Checked = CanEdit;
      efpCanView.Checked = CanView;
      efpMultiSelect.Checked = MultiSelect;
      efpCanMultiEdit.Checked = CanMultiEdit;
      efpEnterKeyMode.SelectedIndex = (int)EnterKeyMode;
    }

    private void WriteValues()
    {
      ReadOnly = efpReadOnly.Checked;
      CanInsert = efpCanInsert.Checked;
      CanInsertCopy = efpCanInsertCopy.Checked;
      CanDelete = efpCanDelete.Checked;
      CanEdit = efpCanEdit.Checked;
      CanView = efpCanView.Checked;
      MultiSelect = efpMultiSelect.Checked;
      CanMultiEdit = efpCanMultiEdit.Checked;
      EnterKeyMode = (EFPDataViewEnterKeyMode)(efpEnterKeyMode.SelectedIndex);
    }

    #endregion

    #region Поля

    private EFPCheckBox efpReadOnly, efpCanInsert, efpCanInsertCopy, efpCanDelete, efpCanEdit, efpCanView, efpMultiSelect, efpCanMultiEdit;
    private EFPListComboBox efpEnterKeyMode;

    #endregion

    #region Статический метод

    private static bool ReadOnly = false;
    private static bool CanInsert = true;
    private static bool CanInsertCopy = false;
    private static bool CanDelete = true;
    private static bool CanEdit = true;
    private static bool CanView = true;
    private static bool MultiSelect = false;
    private static bool CanMultiEdit = false;
    private static EFPDataViewEnterKeyMode EnterKeyMode = EFPDataViewEnterKeyMode.EditOrView;

    public static void PerformTest()
    {
      using (EFPDataViewParamsForm paramForm = new EFPDataViewParamsForm())
      {
        if (EFPApp.ShowDialog(paramForm, false) == DialogResult.OK)
        {
          paramForm.WriteValues();

          DataTable table = new DataTable();
          table.Columns.Add("Id", typeof(Int32));
          table.Columns.Add("ParentId", typeof(Int32));
          table.Columns.Add("Name", typeof(string));
          DataTools.SetPrimaryKey(table, "id");
          table.Rows.Add("1", DBNull.Value, "One");
          table.Rows.Add("2", DBNull.Value, "Two");
          table.Rows.Add("3", DBNull.Value, "Three");

          OKCancelForm testForm = new OKCancelForm();
          TabControl theTabControl = new TabControl();
          theTabControl.Dock = DockStyle.Fill;
          testForm.MainPanel.Controls.Add(theTabControl);

          TabPage tpGrid = new TabPage("EFPDataGridView");
          theTabControl.TabPages.Add(tpGrid);
          EFPControlWithToolBar<DataGridView> cwtGrid = new EFPControlWithToolBar<DataGridView>(testForm.FormProvider, tpGrid);
          EFPDataGridView efpGrid = new EFPDataGridView(cwtGrid);
          efpGrid.Control.ReadOnly = true;
          InitProps(efpGrid);
          efpGrid.Control.DataSource = table.DefaultView;

          TabPage tpTree = new TabPage("EFPDataTreeView");
          theTabControl.TabPages.Add(tpTree);
          EFPControlWithToolBar<TreeViewAdv> cwtTree = new EFPControlWithToolBar<TreeViewAdv>(testForm.FormProvider, tpTree);
          EFPDataTreeView efpTree = new EFPDataTreeView(cwtTree);
          efpTree.Columns.AddTextFill("Name", true, "Name", 100, 10);
          InitProps(efpTree);
          efpTree.Control.Model = new DataTableTreeModel(table, "Id", "ParentId");

          EFPApp.ShowDialog(testForm, true);
        }
      }
    }

    private static void InitProps(IEFPDataView controlProvider)
    {
      controlProvider.ReadOnly = ReadOnly;
      controlProvider.CanInsert = CanInsert;
      controlProvider.CanInsertCopy = CanInsertCopy;
      controlProvider.CanDelete = CanDelete;
      controlProvider.CanEdit = CanEdit;
      controlProvider.CanView = CanView;
      controlProvider.MultiSelect = MultiSelect;
      controlProvider.CanMultiEdit = CanMultiEdit;
      controlProvider.EnterKeyMode = EnterKeyMode;
      controlProvider.EditData += ControlProvider_EditData;
    }

    private static void ControlProvider_EditData(object sender, EventArgs args)
    {
      IEFPDataView controlProvider = (IEFPDataView)sender;
      EFPApp.MessageBox("State=" + controlProvider.State.ToString(), "EditData event");
    }

    #endregion
  }
}
