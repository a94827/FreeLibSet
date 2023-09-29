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

    public static void TestBRReport(IFormatProvider formatProvider)
    {
      BRPrintPreviewDialog dlg = new BRPrintPreviewDialog();
      dlg.Report = CreateDemoReport(formatProvider);
      dlg.ShowDialog();
    }


    private static BRReport CreateDemoReport(IFormatProvider formatProvider)
    {
      //BRReport.AppDefaultCellStyle.BackColor = new BRColor(255, 128, 128);

      BRReport report = new BRReport();
      report.DefaultCellStyle.FormatProvider = formatProvider;

      report.DocumentProperties.Title = "Тестирование возможностей BRReport";
      report.DocumentProperties.Author = Environment.UserName;
      report.DocumentProperties.Subject = "BRReport";
      report.DocumentProperties.Company = "FreeLibSet";

      BRNamedCellStyle head1 = report.NamedCellStyles.Add("Head1");
      head1.LeftMargin = 0;
      head1.Bold = true;
      //head1.ForeColor = BRColor.Blue;
      BRSection sect = report.Sections.Add();
      BRTable table;

      //table = sect.Bands.Add(1, 1);
      //table.Cells.Value = "Hello, World!";
      //table.Cells.CellStyle.AllBorders = BRLine.Thin;

#if !XXX

      #region Выравнивание

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Выравнивание";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(4, 4);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.Cells.RowIndex = 0;
      table.Cells.ColumnIndex = 1;
      table.Cells.Value = "Left";
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Left;
      table.Cells.ColumnIndex = 2;
      table.Cells.Value = "Center";
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Center;
      table.Cells.ColumnIndex = 3;
      table.Cells.Value = "Right";
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Right;

      table.Cells.ColumnIndex = 0;
      table.Cells.RowIndex = 1;
      table.Cells.Value = "Top";
      table.Cells.RowCellStyle.VAlign = BRVAlign.Top;
      table.Cells.RowInfo.Height = 100;

      table.Cells.RowIndex = 2;
      table.Cells.Value = "Center";
      table.Cells.RowCellStyle.VAlign = BRVAlign.Center;
      table.Cells.RowInfo.Height = 100;

      table.Cells.RowIndex = 3;
      table.Cells.Value = "Bottom";
      table.Cells.RowCellStyle.VAlign = BRVAlign.Bottom;
      table.Cells.RowInfo.Height = 100;

      for (int i = 1; i <= 3; i++)
      {
        table.Cells.RowIndex = i;
        for (int j = 1; j <= 3; j++)
        {
          table.Cells.ColumnIndex = j;
          table.Cells.Value = "Text";
        }
      }

      table = sect.Bands.Add(4, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.TopMargin = 30;
      table.SetValue(0, 0, "Строка");
      table.SetValue(0, 1, "AAA" + Environment.NewLine + "BBB" + Environment.NewLine + "CCC");
      table.SetValue(1, 0, "Число");
      table.SetValue(1, 1, 123.456m);
      table.SetValue(2, 0, "Дата");
      table.SetValue(2, 1, DateTime.Today);
      table.SetFormat(2, 1, "d");
      table.SetValue(3, 0, "Дата/время");
      table.SetValue(3, 1, DateTime.Now);
      table.SetFormat(3, 1, "g");

      #endregion

      #region Перенос по словам

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Перенос по словам";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(2, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.Cells.RowIndex = 0;
      table.Cells.RowCellStyle.HAlign = BRHAlign.Center;
      table.SetValue(0, 0, "NoWrap");
      table.SetValue(0, 1, "WordWrap");
      table.Cells.RowIndex = 1;
      table.Cells.RowCellStyle.HAlign = BRHAlign.Left;

      string longText = "Wikipedia is an online encyclopedia written and maintained by a community of volunteers, known as Wikipedians, through open collaboration and using a wiki-based editing system called MediaWiki.";
      table.Cells.ColumnIndex = 0;
      table.Cells.Value = longText;
      table.Cells.CellStyle.WrapMode = BRWrapMode.NoWrap;

      table.Cells.ColumnIndex = 1;
      table.Cells.Value = longText;
      table.Cells.CellStyle.WrapMode = BRWrapMode.WordWrap;
      #endregion

      #region Шрифты

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Шрифты";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      #region FontName

      //string[] a = EFPApp.Fonts.FontNames;
      //string[] a = new string[] {"Gabriola" };
      GenericFontFamilies[] gff = (GenericFontFamilies[])Enum.GetValues(typeof(GenericFontFamilies));
      string[] a = new string[gff.Length];
      for (int i = 0; i < gff.Length; i++)
        a[i] = new FontFamily(gff[i]).Name;
      table = sect.Bands.Add(a.Length, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;

      for (int i = 0; i < a.Length; i++)
      {
        table.Cells.RowIndex = i;
        table.Cells.ColumnIndex = 0;
        table.Cells.Value = a[i];
        table.Cells.ColumnIndex = 1;
        table.Cells.Value = a[i];
        table.Cells.CellStyle.FontName = a[i];
      }

      #endregion

      #region FontHeight

      float[] fontSizes = new float[] { 36, 24, 12, 10, 8, 6, 4 };
      table = sect.Bands.Add(fontSizes.Length, 1);
      //table.DefaultCellStyle.AllBorders = BRLine.Thin;
      for (int i = 0; i < fontSizes.Length; i++)
      {
        table.Cells.RowIndex = i;
        table.Cells.CellStyle.FontHeightPt = fontSizes[i];
        table.Cells.Value = "Шрифт " + fontSizes[i].ToString() + " пт.";
      }

      #endregion

      #region FontWidth

      table = sect.Bands.Add(5, 4);
      table.TopMargin = 30;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.Cells.RowIndex = 0;
      table.Cells.RowCellStyle.HAlign = BRHAlign.Center;
      table.Cells.ColumnIndex = 0;
      table.Cells.ColumnInfo.Width = 150;
      table.Cells.Value = "FontWidthPercent";
      table.Cells.Merge(1, 2);
      table.Cells.ColumnIndex = 2;
      table.Cells.ColumnInfo.Width = 150;
      table.Cells.Value = "FontWidthPt";
      table.Cells.Merge(1, 2);
      int[] fontWidthPercents = new int[] { 25, 50, 100, 200 };
      float[] fontWidthPts = new float[] { 3f, 6f, 12f, 24f };
      for (int i = 0; i < 4; i++)
      {
        table.Cells.RowIndex = i + 1;
        table.Cells.ColumnIndex = 0;
        table.Cells.Value = fontWidthPercents[i].ToString() + "%";
        table.Cells.ColumnIndex = 1;
        table.Cells.Value = "Text 12345678";
        table.Cells.CellStyle.FontWidthPercent = fontWidthPercents[i];
        table.Cells.ColumnIndex = 2;
        table.Cells.Value = fontWidthPts[i].ToString() + "пт.";
        table.Cells.ColumnIndex = 3;
        table.Cells.Value = "Text 12345678";
        table.Cells.CellStyle.FontWidthPt = fontWidthPts[i];
      }

      #endregion

      #region LineHeight

      table = sect.Bands.Add(2, 4);
      table.TopMargin = 30;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      float[] lineHeights = new float[] { 10f, 12f, 14f, 16f };
      for (int i = 0; i < table.ColumnCount; i++)
      {
        table.Cells.ColumnIndex = i;
        table.Cells.RowIndex = 0;
        table.Cells.CellStyle.HAlign = BRHAlign.Center;
        table.Cells.Value = "LineHeight=" + lineHeights[i].ToString() + "пт.";

        table.Cells.RowIndex = 1;
        table.Cells.RowInfo.Height = 200; // TODO: 11.08.2023 Автоопределение высоты не учитывает интервал
        table.Cells.RowCellStyle.VAlign = BRVAlign.Top;
        table.Cells.RowCellStyle.WrapMode = BRWrapMode.NoWrap;
        table.Cells.Value = "Hello." + Environment.NewLine + "Hello?" + Environment.NewLine + "Hello!";
        table.Cells.CellStyle.LineHeightPt = lineHeights[i];
      }

      #endregion

      #region Enlarge

      table = sect.Bands.Add(5, 3);
      table.TopMargin = 30;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      int[] maxEnlargePercents = new int[] { 100, 200, 300, 400 };
      table.Cells.RowIndex = 0;
      table.Cells.RowCellStyle.HAlign = BRHAlign.Center;
      table.SetValue(0, 0, "MaxEnlargePercent");
      table.SetValue(0, 1, "AlwaysEnlarge=false");
      table.SetValue(0, 2, "AlwaysEnlarge=true");
      table.Cells.ColumnIndex = 1;
      table.Cells.ColumnCellStyle.AlwaysEnlarge = false;
      table.Cells.ColumnIndex = 2;
      table.Cells.ColumnCellStyle.AlwaysEnlarge = true;
      for (int i = 0; i < 4; i++)
      {
        table.Cells.RowIndex = i + 1;
        table.Cells.ColumnIndex = 0;
        table.Cells.ColumnInfo.Width = 200;
        table.Cells.Value = maxEnlargePercents[i].ToString() + "%";
        for (int j = 1; j <= 2; j++)
        {
          table.Cells.ColumnIndex = j;
          table.Cells.Value = "Text Text Text Text";
          table.Cells.CellStyle.MaxEnlargePercent = maxEnlargePercents[i];
        }
      }

      #endregion

      #region Styles

      table = sect.Bands.Add(6, 6);
      table.TopMargin = 30;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.SetValue(0, 1, "Regular");
      table.SetValue(1, 0, "Regular");
      table.SetValue(0, 2, "Bold");
      table.SetValue(2, 0, "Bold");
      table.Cells.RowIndex = 2;
      table.Cells.RowCellStyle.Bold = true;
      table.Cells.ColumnIndex = 2;
      table.Cells.ColumnCellStyle.Bold = true;
      table.SetValue(0, 3, "Italic");
      table.SetValue(3, 0, "Italic");
      table.Cells.RowIndex = 3;
      table.Cells.RowCellStyle.Italic = true;
      table.Cells.ColumnIndex = 3;
      table.Cells.ColumnCellStyle.Italic = true;
      table.SetValue(0, 4, "Underline");
      table.SetValue(4, 0, "Underline");
      table.Cells.RowIndex = 4;
      table.Cells.RowCellStyle.Underline = true;
      table.Cells.ColumnIndex = 4;
      table.Cells.ColumnCellStyle.Underline = true;
      table.SetValue(0, 5, "Strikeout");
      table.SetValue(5, 0, "Strikeout");
      table.Cells.RowIndex = 5;
      table.Cells.RowCellStyle.Strikeout = true;
      table.Cells.ColumnIndex = 5;
      table.Cells.ColumnCellStyle.Strikeout = true;
      for (int i = 1; i <= 5; i++)
        for (int j = 1; j <= 5; j++)
          table.SetValue(i, j, "Hello!");

      #endregion

      #endregion

      #region Отступы

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "IndentLevel";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(10, 2);
      table.DefaultCellStyle.WrapMode = BRWrapMode.WordWrap;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.Cells.ColumnIndex = 0;
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Left;
      table.Cells.ColumnIndex = 1;
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Right;
      for (int i = 0; i < table.RowCount; i++)
      {
        table.Cells.RowIndex = i;
        table.Cells.RowCellStyle.IndentLevel = i;
        table.SetValue(i, 0, "Text Text Text Text Text");
        table.SetValue(i, 1, "Text Text Text Text Text");
      }

      #endregion

      #region Цвета

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Цвета";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      BRColor[] colors = new BRColor[9] { BRColor.Auto, BRColor.Black, BRColor.Blue, BRColor.Green, BRColor.Red, BRColor.Purple, BRColor.Cyan, BRColor.Yellow, BRColor.White };
      table = sect.Bands.Add(colors.Length, colors.Length);
      table.DefaultCellStyle.WrapMode = BRWrapMode.WordWrap;
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      for (int i = 0; i < colors.Length; i++)
      {
        table.Cells.RowIndex = i;
        table.Cells.RowCellStyle.BackColor = colors[i];
        table.Cells.ColumnIndex = i;
        table.Cells.ColumnCellStyle.ForeColor = colors[i];

        for (int j = 0; j < colors.Length; j++)
          table.SetValue(i, j, colors[i].ToString() + " / " + colors[j].ToString());
      }

      #endregion

      #region Границы

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Границы";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(11, 1);
      table.DefaultCellStyle.HAlign = BRHAlign.Center;

      table.Cells.RowIndex = 0;
      table.Cells.Value = "LeftBorder";
      table.Cells.CellStyle.LeftBorder = BRLine.Thin;

      table.Cells.RowIndex = 2;
      table.Cells.Value = "TopBorder";
      table.Cells.CellStyle.TopBorder = BRLine.Thin;

      table.Cells.RowIndex = 4;
      table.Cells.Value = "RightBorder";
      table.Cells.CellStyle.RightBorder = BRLine.Thin;

      table.Cells.RowIndex = 6;
      table.Cells.Value = "BottomBorder";
      table.Cells.CellStyle.BottomBorder = BRLine.Thin;

      table.Cells.RowIndex = 8;
      table.Cells.Value = "DiagonalDown";
      table.Cells.CellStyle.DiagonalDown = BRLine.Thin;

      table.Cells.RowIndex = 10;
      table.Cells.Value = "DiagonalUp";
      table.Cells.CellStyle.DiagonalUp = BRLine.Thin;

      Array aLineStyles = Enum.GetValues(typeof(BRLineStyle));
      table = sect.Bands.Add(aLineStyles.Length * 2, 2);
      table.TopMargin = 30;
      int cnt = 0;
      foreach (BRLineStyle ls in aLineStyles)
      {
        table.Cells.RowIndex = cnt * 2;
        table.Cells.ColumnIndex = 0;
        table.Cells.Value = ls.ToString();
        table.Cells.Merge(2, 1);
        table.Cells.ColumnIndex = 1;
        table.Cells.CellStyle.BottomBorder = new BRLine(ls);
        cnt++;
      }

      table = sect.Bands.Add(2, 2);
      table.TopMargin = 30;
      table.Cells.Select(0, 0);
      table.Cells.CellStyle.AllBorders = new BRLine(BRLineStyle.Thick, BRColor.Black);
      table.Cells.Select(0, 1);
      table.Cells.CellStyle.AllBorders = new BRLine(BRLineStyle.Thick, BRColor.Red);
      table.Cells.Select(1, 0);
      table.Cells.CellStyle.AllBorders = new BRLine(BRLineStyle.Thick, BRColor.Green);
      table.Cells.Select(1, 1);
      table.Cells.CellStyle.AllBorders = new BRLine(BRLineStyle.Thick, BRColor.Blue);

      #endregion

      #region Merge

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Объединение ячеек";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(4, 4);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.DefaultCellStyle.HAlign = BRHAlign.Center;
      table.DefaultCellStyle.BackColor = new BRColor(255, 255, 128);
      table.DefaultCellStyle.DiagonalUp = new BRLine(BRLineStyle.Dot, BRColor.Red);
      table.DefaultCellStyle.DiagonalDown = table.DefaultCellStyle.DiagonalUp;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          table.SetValue(i, j, i * 4 + j + 1);

      table.Cells.Select(0, 0);
      table.Cells.CellStyle.BackColor = BRColor.Green;
      table.Cells.Merge(2, 2);

      table.Cells.Select(0, 2);
      table.Cells.CellStyle.BackColor = new BRColor(128, 255, 128);
      table.Cells.Merge(1, 2);

      table.Cells.Select(2, 0);
      table.Cells.CellStyle.BackColor = new BRColor(128, 128, 255);
      table.Cells.Merge(2, 1);

      #endregion

      #region Заполнитель

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Заполнитель";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      Array aTextLeaders = Enum.GetValues(typeof(BRTextLeader));
      table = sect.Bands.Add(aTextLeaders.Length, 3);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.Cells.ColumnIndex = 0;
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Left;
      table.Cells.ColumnIndex = 1;
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Center;
      table.Cells.ColumnCellStyle.ForeColor = BRColor.Blue;
      table.Cells.ColumnIndex = 2;
      table.Cells.ColumnCellStyle.HAlign = BRHAlign.Right;
      table.Cells.ColumnCellStyle.ForeColor = BRColor.Green;
      cnt = 0;
      foreach (BRTextLeader leader in aTextLeaders)
      {
        table.Cells.RowIndex = cnt;
        table.Cells.RowCellStyle.TextLeader = leader;
        table.SetValue(cnt, 0, leader.ToString());
        table.SetValue(cnt, 1, leader.ToString());
        table.SetValue(cnt, 2, leader.ToString());
        cnt++;
      }

      #endregion

      #region Большая таблица

      sect = report.Sections.Add();
      sect.PageSetup.Landscape = true;

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Большая таблица";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;
      table.Cells.RowInfo.Height = 50;

      table = sect.Bands.Add(100, 20);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      for (int i = 0; i < table.ColumnCount; i++)
      {
        table.Cells.ColumnIndex = i;
        table.Cells.ColumnInfo.Width = 200;
      }
      for (int i = 0; i < table.RowCount; i++)
      {
        for (int j = 0; j < table.ColumnCount; j++)
          table.SetValue(i, j, (i + 1) * (j + 1));
        table.Cells.RowIndex = i;
        table.Cells.RowInfo.Height = 50;
      }
      table.Cells.RowIndex = 0;
      table.Cells.RowInfo.Repeatable = true;
      table.Cells.RowCellStyle.BackColor = new BRColor(128, 128, 255);
      table.Cells.ColumnIndex = 0;
      table.Cells.ColumnInfo.Repeatable = true;
      table.Cells.ColumnInfo.AutoGrow = true;
      table.Cells.ColumnCellStyle.BackColor = new BRColor(128, 128, 255);

      #endregion
#endif
      return report;
    }


    #endregion

    #region Тестирование TreeView

    public static void TestTreeView(bool useColumns, bool addOutItem)
    {
      DataTable table = CreateTestTable();
      DataTableTreeModel model = new DataTableTreeModel(table, "Id", "ParentId");

      SimpleForm<TreeViewAdv> form = new SimpleForm<TreeViewAdv>();
      EFPDataTreeView efpTree = new EFPDataTreeView(form.ControlWithToolBar);
      efpTree.Control.UseColumns = useColumns;

      // Вызывать методы EFPDataTreeViewColumns.AddXXX() можно, даже если useColumns=false.
      // При этом создается NodeControl, но не создается TreeColumn

      efpTree.Columns.AddText("Name", true, "Название", 50, 10);
      //if (useColumns)
      {
        efpTree.Columns.AddDate("Date", true, "Дата");
        efpTree.Columns.AddInt("Id", true, "Id", 3);
        if (useColumns)
          efpTree.Columns.LastAdded.SizeGroup = "Id";
        efpTree.Columns.AddInt("ParentId", true, "ParentId", 3);
        if (useColumns)
          efpTree.Columns.LastAdded.SizeGroup = "Id";
        efpTree.Columns.AddBool("Flag", true, "Flag");
        if (useColumns)
        {
          efpTree.Columns["Id"].PrintHeadersSpec = "Идентификатор|Id";
          efpTree.Columns["ParentId"].PrintHeadersSpec = "Идентификатор|ParentId";
          efpTree.Columns["Id"].DisplayName = "Идентификатор узла";
          efpTree.Columns["ParentId"].DisplayName = "Идентификатор родительского узла";
        }
      }
      efpTree.ReadOnly = true;
      efpTree.CanView = false;
      efpTree.Control.Model = model;
      efpTree.Control.SelectionMode = TreeViewAdvSelectionMode.Multi;

      efpTree.DocumentProperties.Author = "Ageyev A.V.";
      efpTree.DocumentProperties.Title = "Test Tree View";
      efpTree.DocumentProperties.Subject = "Demonstrate BRReport features";
      efpTree.DocumentProperties.Company = "Free software";

      if (addOutItem)
      {
        BRMenuOutItem myOutItem1 = new BRMenuOutItem("SelRows");
        myOutItem1.DisplayName = "Тестовый отчет для выбранных строк";
        myOutItem1.SettingsData.Add(new BRFontSettings());
        myOutItem1.Tag = efpTree;
        myOutItem1.InitDialog += MyOutItem1_InitDialog;
        myOutItem1.CreateReport += MyOutItem1_CreateReport;
        efpTree.CommandItems.OutHandler.Items.Add(myOutItem1);

        BRMenuOutItem myOutItem2 = new BRMenuOutItem("Cards");
        myOutItem2.SettingsData.Remove<BRPageSetup>();
        myOutItem2.DisplayName = "Карточки для выбранных строк";
        myOutItem2.Tag = efpTree;
        myOutItem2.CreateReport += MyOutItem2_CreateReport;
        efpTree.CommandItems.OutHandler.Items.Add(myOutItem2);
      }

      //efpTree.Attached += EfpTree_Attached;
      EFPApp.ShowFormOrDialog(form);
    }

    private static void EfpTree_Attached(object sender, EventArgs args)
    {
      EFPDataTreeView efpTree = (EFPDataTreeView)sender;
      DebugTools.DebugCommandItems(efpTree.CommandItems, "efpTree.CommandItems");
    }

    private static void MyOutItem1_InitDialog(object sender, BRMenuOutItemInitDialogEventArgs args)
    {
      if (args.Action == BROutAction.Print)
        args.AddFontPage();
    }

    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("ParentId", typeof(int));
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("Date", typeof(DateTime));
      table.Columns.Add("Flag", typeof(bool));

      for (int i = 1; i <= 10; i++)
        table.Rows.Add(i, DBNull.Value, "Корневой узел №" + i.ToString(), DateTime.Today.AddDays(i - 1), DBNull.Value);

      for (int i = 101; i <= 200; i++)
        table.Rows.Add(i, (i % 10) + 1, "Дочерний узел №" + i.ToString(), DateTime.Today.AddDays(i - 1), ((i/10)%2)==0);

      DataTools.SetPrimaryKey(table, "Id");
      return table;
    }

    private static void MyOutItem1_CreateReport(object sender, BRMenuOutItemCreateReportEventArgs args)
    {
      BRMenuOutItem outItem = (BRMenuOutItem)sender;
      EFPDataTreeView efpTree = (EFPDataTreeView)(outItem.Tag);
      DataRow[] rows = efpTree.SelectedDataRows;

      outItem.SettingsData.GetRequired<BRFontSettings>().InitCellStyle(args.Report.DefaultCellStyle);
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
      EFPDataTreeView efpTree = (EFPDataTreeView)(outItem.Tag);
      DataRow[] rows = efpTree.SelectedDataRows;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        BRSection sect = args.Report.Sections.Add();
        sect.PageSetup.Landscape = true;
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
