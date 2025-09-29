using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Models.Tree;
using FreeLibSet.UICore;

namespace WinFormsDemo.ManualOrderColumnDemo
{
  public partial class ManualOrderColumnParamForm : Form
  {
    #region Конструктор

    public ManualOrderColumnParamForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpView = new EFPRadioButtons(efpForm, rbEFPDataGridView);
      efpView.SelectedIndex = 0;

      efpManualOrderColumn = new EFPCheckBox(efpForm, cbManualOrderColumn);
      efpManualOrderColumn.Checked = true;

      efpDefaultManualOrderColumn = new EFPCheckBox(efpForm, cbDefaultManualOrderColumn);

      efpReadOnly = new EFPCheckBox(efpForm, cbReadOnly);

      efpMultiSelect = new EFPCheckBox(efpForm, cbMultiSelect);
      efpMultiSelect.Checked = true;

      efpOrderDataType = new EFPListComboBox(efpForm, cbOrderDataType);
      efpOrderDataType.SelectedIndex = 0;

      efpOrderStartMode = new EFPListComboBox(efpForm, cbOrderStartMode);
      efpOrderStartMode.SelectedIndex = 0;
    }

    #endregion

    #region Свойства

    public EFPRadioButtons efpView;

    public EFPCheckBox efpManualOrderColumn, efpDefaultManualOrderColumn, efpReadOnly, efpMultiSelect;

    public EFPListComboBox efpOrderDataType;

    public EFPListComboBox efpOrderStartMode;

    #endregion

    #region Статический метод запуска теста

    public static void PerformTest()
    {
      ManualOrderColumnParamForm parForm = new ManualOrderColumnParamForm();
      if (EFPApp.ShowDialog(parForm, true) == DialogResult.OK)
      {
        bool useTree = false;
        switch (parForm.efpView.SelectedIndex)
        {
          case 1:
          case 2:
            useTree = true; break;
        }

        Type orderDataType;
        switch (parForm.efpOrderDataType.SelectedIndex)
        {
          case 0: orderDataType = typeof(Int16); break;
          case 1: orderDataType = typeof(Int32); break;
          // пока не поддерживается // case 2: orderDataType = typeof(Int64); break;
          default: throw new BugException();
        }
        DataTable table = CreateTestTable(useTree, orderDataType);
        switch (parForm.efpOrderStartMode.SelectedIndex)
        {
          case 0: InitOrder123(table); break;
          case 1: break; // нули
          case 2: InitOrderRandom(table); break;
        }

        table.DefaultView.Sort = "Order";

        Form testForm;
        EFPDataGridView gr1 = null;
        EFPDataTreeView tv2 = null;
        switch (parForm.efpView.SelectedIndex)
        {
          case 0: // простая таблица
            testForm = new SimpleGridForm();
            gr1 = new EFPDataGridView(((SimpleGridForm)testForm).ControlWithToolBar);
            break;

          case 1: // дерево
            testForm = new SimpleForm<TreeViewAdv>();
            tv2 = new EFPDataTreeView(((SimpleForm<TreeViewAdv>)testForm).ControlWithToolBar);
            break;

          case 2: // таблица плюс дерево
            testForm = CreateTestForm2(out gr1, out tv2);
            break;

          case 3: // таблица для ввода по месту
            testForm = new SimpleGridForm();
            gr1 = new EFPInputDataGridView(((SimpleGridForm)testForm).ControlWithToolBar);
            UIInputGridData idata = new UIInputGridData(table);
            ((EFPInputDataGridView)gr1).FixedRows = parForm.efpReadOnly.Checked;
            ((EFPInputDataGridView)gr1).Data = idata;
            break;

          default:
            throw new BugException();
        }

        if (gr1 != null)
        {
          gr1.Control.MultiSelect = parForm.efpMultiSelect.Checked;
          if (parForm.efpManualOrderColumn.Checked)
            gr1.ManualOrderColumn = "Order";
          if (parForm.efpDefaultManualOrderColumn.Checked)
            gr1.DefaultManualOrderColumn = "Text";

          if (parForm.efpView.SelectedIndex != 3) // не EFPInputDataGridView
          {
            gr1.ReadOnly = parForm.efpReadOnly.Checked;
            gr1.CanView = false;
            gr1.Control.DataSource = table.DefaultView;
          }
        }
        if (tv2 != null)
        {
          tv2.Columns.AddText("Text", true, "Text", 10, 1);
          tv2.Columns.AddInt32("Order", true, "Order", 5);
          tv2.Columns.AddInt32("Id", true, "Id", 5);
          tv2.Columns.AddInt32("ParentId", true, "ParentId", 5);

          tv2.Control.SelectionMode = parForm.efpMultiSelect.Checked ? TreeViewAdvSelectionMode.Multi : TreeViewAdvSelectionMode.Single;
          tv2.ReadOnly = parForm.efpReadOnly.Checked;
          tv2.CanView = false;
          if (parForm.efpManualOrderColumn.Checked)
            tv2.ManualOrderColumn = "Order";
          if (parForm.efpDefaultManualOrderColumn.Checked)
            tv2.DefaultManualOrderColumn = "Text";

          DataTableTreeModel treeModel = new DataTableTreeModel(table, "Id", "ParentId");
          // не требуется. // treeModel.Sort = "Order";
          tv2.Control.Model = treeModel;

          if (gr1 != null)
            // В режиме "Дерево плюс таблица" нужно, чтобы в таблице тоже использовался DataTableTreeReorderHelper,
            // а не стандартный объект. Иначе иерархия не будет учитываться при перестановке строк в таблице
            gr1.DataReorderHelperNeeded += tv2.CreateDataReorderHelper;
        }

        Application.Run(testForm);
      }
    }

    #region Создание тестовой таблицы

    private static DataTable CreateTestTable(bool useTree, Type orderDataType)
    {
      DataTable table = new DataTable("TestTable");
      table.Columns.Add("Text", typeof(string));
      table.Columns.Add("Order", orderDataType);
      if (useTree)
      {
        table.Columns.Add("Id", typeof(Int32));
        table.Columns.Add("ParentId", typeof(Int32));
      }

      table.Rows.Add("AAA");
      table.Rows.Add("BBB");
      table.Rows.Add("CCC");
      table.Rows.Add("DDD");
      table.Rows.Add("EEE");
      table.Rows.Add("FFF");

      if (useTree)
      {
        for (int i = 0; i < table.Rows.Count; i++)
          table.Rows[i]["Id"] = i + 1;
        table.Rows[2]["ParentId"] = 1;
        table.Rows[3]["ParentId"] = 1;
        table.Rows[4]["ParentId"] = 2;
        table.Rows[5]["ParentId"] = 2;
      }

      //if (useTree)
      //{
      //  StringBuilder sb = new StringBuilder();
      //  DataTableTreeModel model = new DataTableTreeModel(table, "Id", "ParentId");
      //  foreach (TreePath p in new TreePathEnumerable(model))
      //  {
      //    sb.Append(Environment.NewLine);
      //    sb.Append(p.ToString());
      //    sb.Append(": ");
      //    DataRow row = model.TreePathToDataRow(p);
      //    sb.Append(row["Text"].ToString());
      //  }
      //}

      return table;
    }

    #endregion

    #region Инициализация поля Order

    private static void InitOrder123(DataTable table)
    {
      for (int i = 0; i < table.Rows.Count; i++)
        DataTools.SetInt32(table.Rows[i], "Order", i + 1);
    }

    private static void InitOrderRandom(DataTable table)
    {
      Random rnd = new Random();
      for (int i = 0; i < table.Rows.Count; i++)
        DataTools.SetInt32(table.Rows[i], "Order", rnd.Next(1, 100));
    }

    #endregion

    #region Тестовая форма с двумя вкладками

    private static Form CreateTestForm2(out EFPDataGridView gr1, out EFPDataTreeView tv2)
    {
      Form form = new Form();
      EFPFormProvider efpForm = new EFPFormProvider(form);

      TabControl tabControl = new TabControl();
      tabControl.Dock = DockStyle.Fill;
      form.Controls.Add(tabControl);
      tabControl.TabPages.Add("EFPDataGridView");
      tabControl.TabPages.Add("EFPDataTreeView");

      EFPControlWithToolBar<DataGridView> cwt1 = new EFPControlWithToolBar<DataGridView>(efpForm, tabControl.TabPages[0]);
      gr1 = new EFPDataGridView(cwt1);

      EFPControlWithToolBar<TreeViewAdv> cwt2 = new EFPControlWithToolBar<TreeViewAdv>(efpForm, tabControl.TabPages[1]);
      tv2 = new EFPDataTreeView(cwt2);

      return form;
    }

    #endregion

    #endregion
  }
}
