using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;
using FreeLibSet.Tests;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRReportTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      BRReport sut = new BRReport();

      Assert.AreEqual(0, sut.Sections.Count, "Sections.Count");
      Assert.AreEqual(0, sut.BandCount, "BandCount");
      Assert.AreEqual(BRReport.AppDefaultEmptyRowHeight, sut.DefaultEmptyRowHeight, "DefaultEmptyRowHeight");
      Assert.AreEqual(BRReport.AppDefaultColumnWidth, sut.DefaultColumnWidth, "DefaultColumnWidth");
      BRCellStyleStorageTests.ValidateProps(sut.DefaultCellStyle);
      BRCellStyleStorageTests.CompareProps(BRReport.AppDefaultCellStyle, sut.DefaultCellStyle);
      Assert.AreEqual(0, sut.NamedCellStyles.Count, "NamedCellStyles.Count");
      CollectionAssert.AreEqual(new string[1] { sut.DefaultCellStyle.FontName }, sut.GetFontNames(), "GetFontNames()");

      Assert.AreEqual(BRReport.AppDefaultDocumentProperties.Title, sut.DocumentProperties.Title, "Title");
      Assert.AreEqual(BRReport.AppDefaultDocumentProperties.Subject, sut.DocumentProperties.Subject, "Subject");
      Assert.AreEqual(BRReport.AppDefaultDocumentProperties.Author, sut.DocumentProperties.Author, "Author");
      Assert.AreEqual(BRReport.AppDefaultDocumentProperties.Company, sut.DocumentProperties.Company, "Company");
    }

    #endregion

    #region Sections

    [Test]
    public void Sections()
    {
      BRReport sut = new BRReport();
      BRSection sect1 = sut.Sections.Add();
      sect1.PageSetup.PaperHeight = 25000;
      sect1.PageSetup.PaperWidth = 20000;
      BRSection sect2 = sut.Sections.Add();

      Assert.AreEqual(2, sut.Sections.Count, "Sections.Count");
      CollectionAssert.AreEqual(new BRSection[2] { sect1, sect2 }, sut.Sections, "Sections");
      Assert.AreSame(sect1, sut.Sections[0], "Sections[0]");
      Assert.AreSame(sect2, sut.Sections[1], "Sections[1]");
      Assert.AreEqual(0, sut.Sections.IndexOf(sect1), "Sections.IndexOf(sect1)");
      Assert.AreEqual(1, sut.Sections.IndexOf(sect2), "Sections.IndexOf(sect2)");

      Assert.AreEqual(sect1.PageSetup.PaperHeight, sect2.PageSetup.PaperHeight, "PaperHeight");
      Assert.AreEqual(sect1.PageSetup.PaperWidth, sect2.PageSetup.PaperWidth, "PaperWidth");
    }

    [Test]
    public void BandCount()
    {
      BRReport sut = new BRReport();
      BRSection sect1 = sut.Sections.Add();
      sect1.Bands.Add(1, 1);
      sect1.Bands.Add(1, 1);
      BRSection sect2 = sut.Sections.Add();
      sect2.Bands.Add(1, 1);
      sect2.Bands.Add(1, 1);
      sect2.Bands.Add(1, 1);
      Assert.AreEqual(5, sut.BandCount);
    }


    #endregion

    #region AppDefaultPageSetup

    [Test]
    public void AppDefaultPageSetup()
    {
      Assert.GreaterOrEqual(BRReport.AppDefaultPageSetup.PaperHeight, 1000, "PaperHeight");
      Assert.GreaterOrEqual(BRReport.AppDefaultPageSetup.PaperWidth, 1000, "PaperWidth");
      Assert.GreaterOrEqual(BRReport.AppDefaultPageSetup.PrintAreaHeight, 1000, "PrintAreaHeight");
      Assert.GreaterOrEqual(BRReport.AppDefaultPageSetup.PrintAreaWidth, 1000, "PrintAreaWidth");
      Assert.AreEqual(BROrientation.Portrait, BRReport.AppDefaultPageSetup.Orientation, "Orientation");
      Assert.IsFalse(BRReport.AppDefaultPageSetup.CenterVertical, "CenterVertical");
      Assert.IsFalse(BRReport.AppDefaultPageSetup.CenterHorizontal, "CenterHorizontal");
      Assert.IsFalse(BRReport.AppDefaultPageSetup.DuplexNewPage, "DuplexNewPage");
    }

    #endregion

    #region Размеры ячеек

    private class DummyMeasurer : IBRMeasurer
    {
      public int GetWantedHeight(BRSelector sel, int columnWidth)
      {
        return 100;
      }

      public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
      {
        width = 100;
        height = 100;
      }
    }


    [Test]
    public void AppDefaultEmptyRowHeight()
    {
      Assert.GreaterOrEqual(BRReport.AppDefaultEmptyRowHeight, BRReport.MinRowHeight, "min");
      Assert.LessOrEqual(BRReport.AppDefaultEmptyRowHeight, BRReport.MaxRowHeight, "max");
    }

    [Test]
    public void DefaultEmptyRowHeight()
    {
      BRReport sut = new BRReport();
      sut.DefaultEmptyRowHeight = 123;
      BRSection sect = sut.Sections.Add();
      BRBand band = sect.Bands.Add(1, 1);
      BRPaginator paginator = new BRPaginator(new DummyMeasurer());
      BRPaginatorPageInfo[] pages = paginator.CreatePages(sut);
      Assert.AreEqual(1, pages.Length, "Pages.Length");
      Assert.AreEqual(1, pages[0].Blocks.Length, "Blocks.Length");
      BRPaginatiorBlockInfo bi = pages[0].Blocks[0];
      CollectionAssert.AreEqual(new int[1] { 123 }, bi.RowHeights, "RowHeights");
    }

    [Test]
    public void AppDefaultColumnWidth()
    {
      Assert.GreaterOrEqual(BRReport.AppDefaultColumnWidth, BRReport.MinColumnWidth, "min");
      Assert.LessOrEqual(BRReport.AppDefaultColumnWidth, BRReport.MaxColumnWidth, "max");
    }

    [Test]
    public void DefaultColumnWidth()
    {
      BRReport sut = new BRReport();
      sut.DefaultColumnWidth = 234;
      BRSection sect = sut.Sections.Add();
      BRBand band = sect.Bands.Add(1, 3);
      BRSelector sel = band.CreateSelector();
      sel.ColumnIndex = 1;
      sel.ColumnInfo.SetWidth(500, false);
      List<int> lst = new List<int>();
      for (int i = 0; i < band.ColumnCount; i++)
      {
        sel.ColumnIndex = i;
        lst.Add(sel.ColumnInfo.Width);
      }
      CollectionAssert.AreEqual(new int[] { 234, 500, 234 }, lst);
    }

    #endregion

    #region DefaultCellStyle

    [Test]
    public void AppDefaultCellStyle()
    {
      BRCellStyleStorageTests.ValidateProps(BRReport.AppDefaultCellStyle);

      Assert.AreEqual(0, BRReport.AppDefaultCellStyle.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(0, BRReport.AppDefaultCellStyle.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(100, BRReport.AppDefaultCellStyle.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(100, BRReport.AppDefaultCellStyle.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.IsFalse(BRReport.AppDefaultCellStyle.AlwaysEnlarge, "AlwaysEnlarge");
      Assert.IsFalse(BRReport.AppDefaultCellStyle.Bold, "Bold");
      Assert.IsFalse(BRReport.AppDefaultCellStyle.Italic, "Italic");
      Assert.IsFalse(BRReport.AppDefaultCellStyle.Underline, "Underline");
      Assert.IsFalse(BRReport.AppDefaultCellStyle.Strikeout, "Strikeout");
    }

    [Test]
    public void DefaultCellStyle()
    {
      BRReport sut = new BRReport();
      sut.DefaultCellStyle.FontName = "Jabberwockey";
      sut.DefaultCellStyle.FontHeightPt = 12;
      sut.DefaultCellStyle.LineHeightPt = 16;
      sut.DefaultCellStyle.FontWidthPt = 8;
      sut.DefaultCellStyle.Bold = true;

      BRSection sect = sut.Sections.Add();
      BRBand band = sect.Bands.Add(1, 1);
      BRSelector sel = band.CreateSelector();
      BRCellStyleStorageTests.CompareProps(sut.DefaultCellStyle, sel.CellStyle);
      Assert.AreEqual("Jabberwockey", sel.CellStyle.FontName, "FontName");
      Assert.AreEqual(12 * 20, sel.CellStyle.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(16 * 20, sel.CellStyle.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(8 * 20, sel.CellStyle.FontWidthTwip, "FontWidthTwip");
      Assert.IsTrue(sel.CellStyle.Bold, "Bold");
    }

    #endregion

    #region NamedCellStyles

    [Test]
    public void NamedCellStyles()
    {
      BRReport sut = new BRReport();
      sut.DefaultCellStyle.FontName = "Font0";
      sut.DefaultCellStyle.FontHeightPt = 14;

      BRNamedCellStyle style1 = sut.NamedCellStyles.Add("Style1");
      BRNamedCellStyle style2 = sut.NamedCellStyles.Add("Style2", "Style1");
      BRNamedCellStyle style3 = sut.NamedCellStyles.Add("Style3", style1);

      Assert.AreEqual(3, sut.NamedCellStyles.Count, "Count");
      CollectionAssert.AreEqual(new BRNamedCellStyle[3] { style1, style2, style3 }, sut.NamedCellStyles, "NamedCellStyles");
      Assert.AreSame(style1, sut.NamedCellStyles[0], "NamedCellStyles[0]");
      Assert.AreSame(style2, sut.NamedCellStyles[1], "NamedCellStyles[1]");
      Assert.AreSame(style3, sut.NamedCellStyles[2], "NamedCellStyles[2]");
      Assert.AreSame(style1, sut.NamedCellStyles["Style1"], "NamedCellStyles[Style1]");
      Assert.AreSame(style2, sut.NamedCellStyles["Style2"], "NamedCellStyles[Style2]");
      Assert.AreSame(style3, sut.NamedCellStyles["Style3"], "NamedCellStyles[Style3]");

      Assert.AreEqual("Font0", style1.FontName, "FontName1 #1");
      Assert.AreEqual("Font0", style2.FontName, "FontName2 #1");
      Assert.AreEqual("Font0", style3.FontName, "FontName3 #1");

      style1.FontName = "Font1";
      Assert.AreEqual("Font1", style1.FontName, "FontName1 #2");
      Assert.AreEqual("Font1", style2.FontName, "FontName2 #2");
      Assert.AreEqual("Font1", style3.FontName, "FontName3 #2");

      style2.FontName = "Font2";
      style3.FontName = "Font3";
      Assert.AreEqual("Font1", style1.FontName, "FontName1 #3");
      Assert.AreEqual("Font2", style2.FontName, "FontName2 #3");
      Assert.AreEqual("Font3", style3.FontName, "FontName3 #3");

      BRSection sect = sut.Sections.Add();
      BRBand band = sect.Bands.Add(1, 4);
      BRSelector sel = band.CreateSelector();
      sel.ColumnIndex = 1;
      sel.CellStyle.ParentStyleName = "Style1";
      sel.ColumnIndex = 2;
      sel.CellStyle.ParentStyle = style2;
      sel.ColumnIndex = 3;
      sel.CellStyle.ParentStyleName = "Style3";

      List<string> lst = new List<string>();
      for (int i = 0; i < band.ColumnCount; i++)
      {
        sel.ColumnIndex = i;
        lst.Add(sel.CellStyle.FontName);
      }

      CollectionAssert.AreEqual(new string[] { "Font0", "Font1", "Font2", "Font3" }, lst, "CellStyle.FontName");
    }

    #endregion

    #region GetFontNames()

    [Test]
    public void GetFontNames()
    {
      BRReport sut = new BRReport();
      sut.DefaultCellStyle.FontName = "Font1";
      BRSection sect = sut.Sections.Add();
      BRBand band = sect.Bands.Add(1, 2);
      BRSelector sel = band.CreateSelector();
      sel.CellStyle.FontName = "Font2";

      string[] res = sut.GetFontNames();
      CollectionAssert.AreEquivalent(new string[] { "Font1", "Font2" }, res);
    }

    #endregion

    #region Сводка

    [Test]
    public void AppDefaultDocumentProperties()
    {
      ExtStringAssert.IsNullOrEmpty(BRReport.AppDefaultDocumentProperties.Title, "Title");
      ExtStringAssert.IsNullOrEmpty(BRReport.AppDefaultDocumentProperties.Subject, "Subject");
      ExtStringAssert.IsNullOrEmpty(BRReport.AppDefaultDocumentProperties.Author, "Author");
      ExtStringAssert.IsNullOrEmpty(BRReport.AppDefaultDocumentProperties.Company, "Company");
    }

    [Test]
    public void DocumentProperties()
    {
      BRReport sut = new BRReport();
      Assert.AreNotSame(BRReport.AppDefaultDocumentProperties, sut.DocumentProperties);
    }

    #endregion
  }
}
