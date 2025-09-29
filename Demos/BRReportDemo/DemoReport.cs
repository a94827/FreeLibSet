using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Reporting;
using FreeLibSet.IO;

namespace BRReportDemo
{
  /// <summary>
  /// Тестовый отчет
  /// </summary>
  public static class DemoReport
  {
    public static BRReport Create(IFormatProvider formatProvider)
    {
      //BRReport.AppDefaultCellStyle.BackColor = new BRColor(255, 128, 128);

      BRReport report = new BRReport();
      report.DefaultCellStyle.FormatProvider = formatProvider;

      report.DocumentProperties.Title = "BRReport features demo";
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

      #region Выравнивание

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Forced aligmnent";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;
      table.Bookmarks.Add("Top");

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

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Automatic alignment";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(5, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;
      table.SetValue(0, 0, "One-line text");
      table.SetValue(0, 1, "AAA");
      table.SetValue(1, 0, "Multi-line text");
      table.SetValue(1, 1, "BBB" + Environment.NewLine + "CCC" + Environment.NewLine + "DDD");
      table.SetValue(2, 0, "A number");
      table.SetValue(2, 1, 123.456m);
      table.SetValue(3, 0, "A date");
      table.SetValue(3, 1, DateTime.Today);
      table.SetFormat(3, 1, "d");
      table.SetValue(4, 0, "A date with time");
      table.SetValue(4, 1, DateTime.Now);
      table.SetFormat(4, 1, "g");

      table = sect.Bands.Add(1, 1);
      table.SetValue(0, 0, "A plain text could contain" + Environment.NewLine + "a new line character too");

      #endregion

      #region Перенос по словам

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Word wrapping";
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
      table.Cells.Value = "Fonts";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      #region FontName

      //string[] a = EFPApp.Fonts.FontNames;
      //string[] a = new string[] {"Gabriola" };
      //string[] aFontNames;
      //if (Environment.OSVersion.Platform==PlatformID.Unix)
      //  aFontNames = new string[] { "Liberation Sans", "Droid serif", "Noto Serif", "Liberation Mono" };
      //else
      //  aFontNames = new string[] { "Arial", "Times New Roman", "Microsoft Sans Serif", "Courier New" };

      List<string> lstFontDescrs = new List<string>();
      List<string> lstFontNames = new List<string>();
      lstFontDescrs.Add("BRReport.DefaultCellStyle");
      lstFontNames.Add(report.DefaultCellStyle.FontName);

      lstFontDescrs.Add("FontFamily.GenericSerif");
      lstFontNames.Add(System.Drawing.FontFamily.GenericSerif.Name);
      lstFontDescrs.Add("FontFamily.GenericSansSerif");
      lstFontNames.Add(System.Drawing.FontFamily.GenericSansSerif.Name);
      lstFontDescrs.Add("FontFamily.GenericMonospace");
      lstFontNames.Add(System.Drawing.FontFamily.GenericMonospace.Name);

      lstFontDescrs.Add("SystemFonts.DefaultFont");
      lstFontNames.Add(System.Drawing.SystemFonts.DefaultFont.Name);

      lstFontDescrs.Add("Font substitute");
      lstFontNames.Add("Tms Rmn");

      table = sect.Bands.Add(lstFontNames.Count, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;

      for (int i = 0; i < lstFontNames.Count; i++)
      {
        table.Cells.RowIndex = i;
        table.Cells.ColumnIndex = 0;
        table.Cells.Value = lstFontDescrs[i];
        table.Cells.ColumnIndex = 1;
        using (System.Drawing.Font font = new System.Drawing.Font(lstFontNames[i], 10))
        {
          string s = lstFontNames[i];
          if (!String.Equals(font.Name, lstFontNames[i], StringComparison.Ordinal))
            s += " -> " + font.Name;
          table.Cells.Value = s;
          table.Cells.CellStyle.FontName = font.Name;
        }
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
        table.Cells.Value = "Font of size " + fontSizes[i].ToString() + " pt.";
      }
      table.Bookmarks.Add("F12", 2, 0); // закладка на ячейку

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

      report.Bookmarks.Add(new BRBookmark("FontStyles", table)); // прямое добавление закладки

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
      table.Cells.Value = "Colors";
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

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "A plain text could contain the font color ...";
      table.Cells.CellStyle.ForeColor = BRColor.Green;

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "... or the background color";
      table.Cells.CellStyle.BackColor = BRColor.Yellow;

      #endregion

      #region Границы

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Borders";
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
      table.Cells.Value = "Cell merging";
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
      table.Cells.Value = "Cell filler";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      Array aTextFiller = Enum.GetValues(typeof(BRTextFiller));
      table = sect.Bands.Add(aTextFiller.Length, 3);
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
      foreach (BRTextFiller filler in aTextFiller)
      {
        table.Cells.RowIndex = cnt;
        table.Cells.RowCellStyle.TextFiller = filler;
        table.SetValue(cnt, 0, filler.ToString());
        table.SetValue(cnt, 1, filler.ToString());
        table.SetValue(cnt, 2, filler.ToString());
        cnt++;
      }

      #endregion

      #region Специальные символы

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Special characters";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "Text could contain the couple  of space chars together or even tree   space chars";
      table.Cells.CellStyle.FontName = "Courier";

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = MakeLongString("Non_breakable_space").Replace('_', StringTools.NonBreakSpaceChar);
      table.Cells.CellStyle.WrapMode = BRWrapMode.WordWrap;

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = MakeLongString("Hyp^he^na^ti^on cha^rac^ters pre^sen^ted").Replace('^', StringTools.SoftHyphenChar);
      table.Cells.CellStyle.WrapMode = BRWrapMode.WordWrap;

      #endregion

      #region Ссылки

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "BRValueWithLink references (work when report exported, not in the view)";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;

      table = sect.Bands.Add(8, 2);
      table.DefaultCellStyle.AllBorders = BRLine.Thin;

      table.SetValue(0, 0, "Internet reference");
      table.SetValue(0, 1, new BRValueWithLink(@"https://stackoverflow.com"));

      AbsPath tempPath = new AbsPath(System.IO.Path.GetTempPath());
      table.SetValue(1, 0, "Directory reference");
      table.SetValue(1, 1, new BRValueWithLink(tempPath.FileName, tempPath.UriString));

      table.SetValue(2, 0, "Reference as a number");
      table.SetValue(2, 1, new BRValueWithLink(451, @"https://en.wikipedia.org/wiki/Fahrenheit_451"));

      string testEmail = "test@test.com";
      table.SetValue(3, 0, "mailto:// reference");
      table.SetValue(3, 1, new BRValueWithLink(testEmail, "mailto://" + testEmail));

      table.SetValue(4, 0, "Links to bookmarks");
      table.SetValue(4, 1, new BRValueWithLink("Top", "#Top"));
      table.SetValue(5, 1, new BRValueWithLink("Font size 12pt", "#F12"));
      table.SetValue(6, 1, new BRValueWithLink("Font styles", "#FontStyles"));
      table.SetValue(7, 0, "(forward link)");
      table.SetValue(7, 1, new BRValueWithLink("Big table cell", "#B52"));

      #endregion

      //#region Скрытые строки и столбцы

      //table = sect.Bands.Add(1, 1);
      //table.Cells.Value = "Hidden rows and columns";
      //table.Cells.CellStyle.ParentStyle = head1;
      //table.KeepWithNext = true;

      //table = CreateTestTableForHiding(sect);
      //table.SetValue(4, 4, "Nothing is hidden");

      //table = CreateTestTableForHiding(sect);
      //table.SetValue(4, 4, "Column with index 2 is hidden");
      //table.Cells.ColumnIndex = 2;
      //table.Cells.ColumnCellStyle.BackColor = BRColor.Red;
      //table.Cells.ColumnInfo.Visible = false;

      //table = CreateTestTableForHiding(sect);
      //table.SetValue(4, 4, "Row with index 2 is hidden");
      //table.Cells.RowIndex = 2;
      //table.Cells.RowCellStyle.BackColor = BRColor.Red;
      //table.Cells.RowInfo.Visible = false;

      //#endregion

      #region Большая таблица

      sect = report.Sections.Add();
      sect.PageSetup.SetOrientation(BROrientation.Landscape, true);
      sect.Name = "Big table";

      table = sect.Bands.Add(1, 1);
      table.Cells.Value = "The big multipage table";
      table.Cells.CellStyle.ParentStyle = head1;
      table.KeepWithNext = true;
      // Если задать высоту строки, то заголовок будет идти как таблица из одной ячейки
      //table.Cells.RowInfo.Height = 50;

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

      table.Bookmarks.Add("B52", 49, 1);

      #endregion

      return report;
    }

    //private static BRTable CreateTestTableForHiding(BRSection sect)
    //{
    //  BRTable table = sect.Bands.Add(5, 5);
    //  table.DefaultCellStyle.HAlign = BRHAlign.Center;
    //  table.DefaultCellStyle.AllBorders = BRLine.Thin;
    //  table.DefaultCellStyle.WrapMode = BRWrapMode.WordWrap;
    //  table.SetValue(0, 0, 1);
    //  table.SetValue(0, 1, 2);
    //  table.SetValue(0, 2, 3);
    //  table.SetValue(0, 3, 3);
    //  table.SetValue(0, 4, 5);
    //  table.SetValue(1, 0, 2);
    //  table.SetValue(2, 0, 3);
    //  table.SetValue(3, 0, 4);
    //  table.SetValue(4, 0, 5);

    //  table.Cells.Select(1, 1);
    //  table.Cells.CellStyle.AllBorders = BRLine.Medium;
    //  table.Cells.CellStyle.BackColor = new BRColor(255, 255, 128);
    //  table.Cells.Value = "Merged cells #1. Two cells merged vertically";
    //  table.Cells.Merge(2, 1);

    //  table.Cells.Select(1, 2);
    //  table.Cells.CellStyle.AllBorders = BRLine.Medium;
    //  table.Cells.CellStyle.BackColor = new BRColor(128, 255, 128);
    //  table.Cells.Value = "Merged cells #2. Two cells merged horizontally";
    //  table.Cells.Merge(1, 2);

    //  table.Cells.Select(2, 3);
    //  table.Cells.CellStyle.AllBorders = BRLine.Medium;
    //  table.Cells.CellStyle.BackColor = new BRColor(128, 128, 255);
    //  table.Cells.Value = "Merged cells #3. Two cells merged vertically";
    //  table.Cells.Merge(2, 1);

    //  table.Cells.Select(3, 1);
    //  table.Cells.CellStyle.AllBorders = BRLine.Medium;
    //  table.Cells.CellStyle.BackColor = new BRColor(255, 128, 255);
    //  table.Cells.Value = "Merged cells #4. Two cells merged horizontally";
    //  table.Cells.Merge(1, 2);

    //  table.BottomMargin = 50;

    //  return table;
    //}

    public static string MakeLongString(string s)
    {
      string[] a = new string[20];
      ArrayTools.FillArray<string>(a, s);
      return String.Join(", ", a);
    }
  }
}
