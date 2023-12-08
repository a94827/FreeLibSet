using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Forms.Reporting;
using FreeLibSet.Models.Tree;
using FreeLibSet.Reporting;

namespace BRReportDemo
{
  public static class Tester
  {
    #region Тестирование BRReport

    public static void TestBRReport(string configSectionName, IFormatProvider formatProvider)
    {
      BRPrintPreviewDialog dlg = new BRPrintPreviewDialog();
      dlg.Report = DemoReport.Create(formatProvider);
      dlg.ConfigSectionName = configSectionName;
      dlg.ShowDialog();
    }

    #endregion

    #region Тестирование EFPDataTreeView

    public enum DefConfigMode { NotSet, Default, Named }

    public static void TestTreeView(bool useColumns, string configSectionName, bool useGridProducer, bool removeOutItem, bool addOutItem, bool multiSelect, DefConfigMode configMode)
    {
      DataTable table = CreateTestTableTree();
      DataTableTreeModel model = new DataTableTreeModel(table, "Id", "ParentId");

      SimpleForm<TreeViewAdv> form = new SimpleForm<TreeViewAdv>();
      EFPConfigurableDataTreeView efpTree = new EFPConfigurableDataTreeView(form.ControlWithToolBar);
      efpTree.Control.UseColumns = useColumns;

      // Вызывать методы EFPDataTreeViewColumns.AddXXX() можно, даже если useColumns=false.
      // При этом создается NodeControl, но не создается TreeColumn

      if (useColumns && useGridProducer)
        efpTree.GridProducer = CreateGridProducer(true, configMode);
      else
      {
        efpTree.Columns.AddText("Name", true, "Название", 50, 10);
        efpTree.Columns.AddDate("Date1", true, "Начало");
        efpTree.Columns.AddDate("Date2", true, "Окончание");
        efpTree.Columns.AddInt("Id", true, "Id", 3);
        efpTree.Columns.AddInt("ParentId", true, "ParentId", 3);
        efpTree.Columns.AddBool("Flag", true, "Flag");
        if (useColumns)
        {
          efpTree.Columns["Id"].SizeGroup = "Id";
          efpTree.Columns["ParentId"].SizeGroup = "Id";
          efpTree.Columns["Id"].ColorType = EFPDataGridViewColorType.Total1;

          efpTree.Columns["Date1"].PrintHeadersSpec = "Период|Начало";
          efpTree.Columns["Date2"].PrintHeadersSpec = "Период|Окончание";

          efpTree.Columns["Id"].PrintHeadersSpec = "Идентификатор|Id";
          efpTree.Columns["ParentId"].PrintHeadersSpec = "Идентификатор|ParentId";
          efpTree.Columns["Id"].DisplayName = "Идентификатор узла";
          efpTree.Columns["ParentId"].DisplayName = "Идентификатор родительского узла";
        }

        if (!removeOutItem)
        {
          if (configMode != DefConfigMode.NotSet)
          {
            //efpGrid.DefaultOutItem.Default.View.BorderStyle = BRDataViewBorderStyle.All;

            if (useColumns)
            {
              //efpGrid.Columns["Name"].PrintWidth = 600;
              //efpGrid.Columns["Name"].PrintAutoGrow = true;
              efpTree.Columns["Date1"].PrintWidth = 250;
              efpTree.Columns["Date2"].PrintWidth = 250;
              efpTree.Columns["Id"].PrintWidth = 200;
              efpTree.Columns["ParentId"].PrintWidth = 200;
              efpTree.Columns["Flag"].PrintWidth = 150;
            }
            if (configMode == DefConfigMode.Named)
            {
              efpTree.DefaultOutItem.Add("A4", "A4 portrait");
              efpTree.DefaultOutItem.Add("A4L", "A4 landscape");
              efpTree.DefaultOutItem["A4L"].PageSetup.InvertOrientation();
            }
          }
        }
      }

      efpTree.ReadOnly = true;
      efpTree.CanView = false;
      efpTree.Control.Model = model;
      if (multiSelect)
        efpTree.Control.SelectionMode = TreeViewAdvSelectionMode.Multi;
      else
        efpTree.Control.SelectionMode = TreeViewAdvSelectionMode.Single;

      efpTree.DocumentProperties.Author = "Ageyev A.V.";
      efpTree.DocumentProperties.Title = "Test EFPDataTreeView";
      efpTree.DocumentProperties.Subject = "Demonstrate BRReport features";
      efpTree.DocumentProperties.Company = "Free software";

      efpTree.ConfigSectionName = configSectionName;

      if (removeOutItem)
        efpTree.CommandItems.OutHandler.Items.Clear();
      if (addOutItem)
        InitAuxReports(efpTree);

      //efpTree.Attached += EfpTree_Attached;
      EFPApp.ShowFormOrDialog(form);
    }

    private static void EfpTree_Attached(object sender, EventArgs args)
    {
      EFPDataTreeView efpTree = (EFPDataTreeView)sender;
      DebugTools.DebugCommandItems(efpTree.CommandItems, "efpTree.CommandItems");
    }

    private static DataTable CreateTestTableTree()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("ParentId", typeof(int));
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("Date1", typeof(DateTime));
      table.Columns.Add("Date2", typeof(DateTime));
      table.Columns.Add("Flag", typeof(bool));

      for (int i = 1; i <= 10; i++)
        table.Rows.Add(i, DBNull.Value, "Корневой узел №" + i.ToString(), DateTime.Today.AddDays(i - 1), DateTime.Today.AddDays(i + 1), DBNull.Value);

      for (int i = 101; i <= 200; i++)
        table.Rows.Add(i, (i % 10) + 1, "Дочерний узел №" + i.ToString(), DateTime.Today.AddDays(i - 1), DateTime.Today.AddDays(i + 1), ((i / 10) % 2) == 0);

      DataTools.SetPrimaryKey(table, "Id");
      return table;
    }

    #endregion

    #region Тестирование EFPDataGridView

    public static void TestGridView(string configSectionName, bool useGridProducer, bool removeOutItem, bool addOutItem, bool multiSelect, DefConfigMode configMode)
    {
      DataTable table = CreateTestTableGrid();
      SimpleGridForm form = new SimpleGridForm();
      EFPConfigurableDataGridView efpGrid = new EFPConfigurableDataGridView(form.ControlWithToolBar);
      efpGrid.Control.AutoGenerateColumns = false;
      efpGrid.ConfigSectionName = configSectionName;

      if (useGridProducer)
        efpGrid.GridProducer = CreateGridProducer(false, configMode);
      else
      {
        efpGrid.Columns.AddTextFill("Name", true, "Название", 50, 10);
        efpGrid.Columns.AddDate("Date1", true, "Начало");
        efpGrid.Columns.AddDate("Date2", true, "Окончание");
        efpGrid.Columns.AddInt("Id", true, "Id", 3);
        efpGrid.Columns.LastAdded.SizeGroup = "Id";
        efpGrid.Columns["Id"].PrintHeadersSpec = "Идентификатор|Id";
        efpGrid.Columns["Id"].DisplayName = "Идентификатор узла";
        efpGrid.Columns.LastAdded.ColorType = EFPDataGridViewColorType.Total1;
        efpGrid.Columns.AddBool("Flag", true, "Flag");

        if (!removeOutItem)
        {
          if (configMode != DefConfigMode.NotSet)
          {
            //efpGrid.DefaultOutItem.Default.View.BorderStyle = BRDataViewBorderStyle.All;

            //efpGrid.Columns["Name"].PrintWidth = 600;
            //efpGrid.Columns["Name"].PrintAutoGrow = true;
            efpGrid.Columns["Date1"].PrintWidth = 250;
            efpGrid.Columns["Date2"].PrintWidth = 250;
            efpGrid.Columns["Id"].PrintWidth = 200;
            efpGrid.Columns["Flag"].PrintWidth = 150;

            if (configMode == DefConfigMode.Named)
            {
              efpGrid.DefaultOutItem.Add("A4", "A4 portrait");
              efpGrid.DefaultOutItem.Add("A4L", "A4 landscape");
              efpGrid.DefaultOutItem["A4L"].PageSetup.InvertOrientation();
            }
          }
        }
        efpGrid.DisableOrdering();
      }

      efpGrid.GetRowAttributes += EfpGrid_GetRowAttributes;
      efpGrid.GetCellAttributes += EfpGrid_GetCellAttributes;
      efpGrid.Control.MultiSelect = multiSelect;
      efpGrid.ReadOnly = true;
      efpGrid.CanView = false;
      efpGrid.Control.DataSource = table.DefaultView;

      efpGrid.DocumentProperties.Author = "Ageyev A.V.";
      efpGrid.DocumentProperties.Title = "Test EFPDataGridView";
      efpGrid.DocumentProperties.Subject = "Demonstrate BRReport features";
      efpGrid.DocumentProperties.Company = "Free software";

      if (removeOutItem)
        efpGrid.CommandItems.OutHandler.Items.Clear();
      if (addOutItem)
        InitAuxReports(efpGrid);

      //efpTree.Attached += EfpTree_Attached;
      EFPApp.ShowFormOrDialog(form);
    }

    private static void EfpGrid_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      int id = DataTools.GetInt(args.DataRow, "Id");
      if (id == 201)
      {
        args.ColorType = EFPDataGridViewColorType.TotalRow;
        args.ControlContentVisible = false;
        args.PrintWithPrevious = true;
        return;
      }

      if ((id % 2) == 0)
        args.ColorType = EFPDataGridViewColorType.Alter;

      switch (id % 10)
      {
        case 1:
          args.ColorType = EFPDataGridViewColorType.Header;
          args.PrintWithNext = true;
          break;
        case 5:
          args.ColorType = EFPDataGridViewColorType.Total2;
          args.PrintWithPrevious = true;
          break;
        case 0:
          args.ColorType = EFPDataGridViewColorType.Total1;
          args.PrintWithPrevious = true;
          break;
      }
    }

    private static void EfpGrid_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      int id = DataTools.GetInt(args.DataRow, "Id");
      switch (args.ColumnName)
      {
        case "Date1":
        case "Date2":
          switch (id % 10)
          {
            case 1:
            case 5:
            case 0:
              args.ContentVisible = false;
              break;
            default:
              DateTime dt = DataTools.GetDateTime(args.DataRow, args.ColumnName);
              switch (dt.DayOfWeek)
              {
                case DayOfWeek.Sunday:
                  args.ColorType = EFPDataGridViewColorType.Error;
                  break;
                case DayOfWeek.Saturday:
                  args.ColorType = EFPDataGridViewColorType.Warning;
                  break;
              }
              break;
          }
          break;


        case "Name":
          switch (id % 10)
          {
            case 1:
            case 5:
            case 0:
              break;
            default:
              args.IndentLevel = 1;
              break;
          }
          break;
      }
    }

    private static DataTable CreateTestTableGrid()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("Date1", typeof(DateTime));
      table.Columns.Add("Date2", typeof(DateTime));
      table.Columns.Add("Flag", typeof(bool));

      for (int i = 1; i <= 201; i++)
      {
        object flag;
        switch (i % 3)
        {
          case 0: flag = true; break;
          case 1: flag = false; break;
          default: flag = DBNull.Value; break;
        }
        table.Rows.Add(i, "Строка №" + i.ToString(), DateTime.Today.AddDays(i - 1), DateTime.Today.AddDays(i + 1), flag);
      }

      DataTools.SetPrimaryKey(table, "Id");
      return table;
    }
    private static EFPGridProducer CreateGridProducer(bool isTree, DefConfigMode configMode)
    {
      EFPGridProducer producer = new EFPGridProducer();
      producer.Columns.AddText("Name", "Название", 50, 10);
      producer.Columns.AddDate("Date1", "Начало");
      producer.Columns.AddDate("Date2", "Окончание");
      producer.Columns.AddInt("Id", "Id", 3);
      producer.Columns.LastAdded.SizeGroup = "Id";
      producer.Columns.LastAdded.ColorType = EFPDataGridViewColorType.Total1;
      producer.Columns["Id"].PrintHeadersSpec = "Идентификатор|Id";
      producer.Columns["Id"].DisplayName = "Идентификатор узла";
      if (isTree)
      {
        producer.Columns.AddInt("ParentId", "ParentId", 3);
        producer.Columns.LastAdded.SizeGroup = "Id";
        producer.Columns["ParentId"].DisplayName = "Идентификатор родительского узла";
      }
      producer.Columns.AddBool("Flag", "Flag");

      if (configMode != DefConfigMode.NotSet)
      {
        //efpGrid.DefaultOutItem.Default.View.BorderStyle = BRDataViewBorderStyle.All;

        //efpGrid.Columns["Name"].PrintWidth = 600;
        //efpGrid.Columns["Name"].PrintAutoGrow = true;
        producer.Columns["Date1"].PrintWidth = 250;
        producer.Columns["Date2"].PrintWidth = 250;
        producer.Columns["Id"].PrintWidth = 200;
        producer.Columns["Flag"].PrintWidth = 150;

        if (configMode == DefConfigMode.Named)
        {
          producer.OutItem.Add("A4", "A4 portrait");
          producer.OutItem.Add("A4L", "A4 landscape");
          producer.OutItem["A4L"].PageSetup.InvertOrientation();
        }
      }

      return producer;
    }

    #endregion

    #region Дополнительные отчеты

    private static void InitAuxReports(IEFPDataView controlProvider)
    {
      BRMenuOutItem myOutItem1 = new BRMenuOutItem("SelRows");
      myOutItem1.DisplayName = "Тестовый отчет для выбранных строк";
      myOutItem1.SettingsData.Add(new BRFontSettingsDataItem());
      myOutItem1.Tag = controlProvider;
      myOutItem1.InitDialog += MyOutItem1_InitDialog;
      myOutItem1.CreateReport += MyOutItem1_CreateReport;
      controlProvider.MenuOutItems.Add(myOutItem1);

      BRMenuOutItem myOutItem2 = new BRMenuOutItem("Cards");
      myOutItem2.SettingsData.Remove<BRPageSettingsDataItem>();
      myOutItem2.DisplayName = "Карточки для выбранных строк";
      myOutItem2.Tag = controlProvider;
      myOutItem2.CreateReport += MyOutItem2_CreateReport;
      controlProvider.MenuOutItems.Add(myOutItem2);
    }


    private static void MyOutItem1_InitDialog(object sender, BRMenuOutItemInitDialogEventArgs args)
    {
      if (args.DialogKind == BRDialogKind.PageSetup)
        args.AddFontPage();
    }

    private static void MyOutItem1_CreateReport(object sender, BRMenuOutItemCreateReportEventArgs args)
    {
      BRMenuOutItem outItem = (BRMenuOutItem)sender;
      IEFPDataView efpTree = (IEFPDataView)(outItem.Tag);
      DataRow[] rows = efpTree.SelectedDataRows;

      outItem.SettingsData.GetRequired<BRFontSettingsDataItem>().InitCellStyle(args.Report.DefaultCellStyle);
      BRSection sect = args.Report.Sections.Add();
      BRTable band = sect.Bands.Add(1, 1);
      band.Cells.Value = "Тестовый отчет для выбранных строк (" + rows.Length.ToString() + ")";
      band.Cells.CellStyle.Bold = true;
      foreach (DataRow row in rows)
      {
        band = sect.Bands.Add(3, 2);
        band.BottomMargin = 30;
        band.DefaultCellStyle.AllBorders = BRLine.Thin;
        band.SetValue(0, 0, "Id");
        band.SetValue(1, 0, "Name");
        band.SetValue(2, 0, "Date");
        band.SetValue(0, 1, row["Id"]);
        band.SetValue(1, 1, row["Name"]);
        band.SetValue(2, 1, row["Date"]);
      }
    }

    private static void MyOutItem2_CreateReport(object sender, BRMenuOutItemCreateReportEventArgs args)
    {
      BRMenuOutItem outItem = (BRMenuOutItem)sender;
      IEFPDataView efpTree = (IEFPDataView)(outItem.Tag);
      DataRow[] rows = efpTree.SelectedDataRows;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        BRSection sect = args.Report.Sections.Add();
        sect.PageSetup.Orientation = BROrientation.Landscape;
        sect.PageSetup.PaperHeight = 1400;
        sect.PageSetup.PaperWidth = 2100;
        sect.PageSetup.TopMargin = 200;
        sect.PageSetup.LeftMargin = 100;
        sect.PageSetup.RightMargin = 100;
        sect.PageSetup.BottomMargin = 100;

        BRTable table = sect.Bands.Add(1, 1);
        table.Cells.RowInfo.Height = sect.PageSetup.PrintAreaHeight;
        table.Cells.CellStyle.AllBorders = BRLine.Thick;

        table.Cells.Value = row["Name"];
        table.Cells.CellStyle.FontName = "Times New Roman";
        table.Cells.CellStyle.FontHeightPt = 32;
        table.Cells.CellStyle.Bold = true;
        table.Cells.CellStyle.Italic = true;
        table.Cells.CellStyle.HAlign = BRHAlign.Center;
        table.Cells.CellStyle.VAlign = BRVAlign.Center;
        int v = cnt * 255 / rows.Length;
        table.Cells.CellStyle.BackColor = new BRColor(255, 255, 255 - v);
        cnt++;
      }
    }

    #endregion
  }
}
