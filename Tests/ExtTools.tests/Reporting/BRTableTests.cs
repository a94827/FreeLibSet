using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRTableTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      Assert.AreEqual(5, sut.RowCount, "RowCount");
      Assert.AreEqual(3, sut.ColumnCount, "ColumnCount");
      Assert.IsTrue(sut.AreAllBordersSame, "AreAllBordersSame");
      Assert.AreSame(report, sut.Report, "Report");
      Assert.AreSame(sect, sut.Section, "Section");
      Assert.AreEqual(0, sut.BandIndex, "BandIndex");
      Assert.IsTrue(sut.KeepWhole, "KeepWhole");
      Assert.IsFalse(sut.KeepWithNext, "KeepWithNext");
      Assert.IsFalse(sut.KeepWithPrev, "KeepWithPrev");
      Assert.AreEqual(0, sut.TopMargin, "TopMargin");
      Assert.AreEqual(0, sut.BottomMargin, "BottomMargin");
      Assert.IsNotNull(sut.Cells, "Cells");
      Assert.AreSame(sut, sut.Cells.Table, "Cells.Table");
      Assert.AreSame(sut, sut.Cells.Band, "Cells.Band");
      Assert.IsNotNull(sut.DefaultCellStyle, "DefaultCellStyle");
      BRCellStyleStorageTests.CompareProps(report.DefaultCellStyle, sut.DefaultCellStyle);
    }

    #endregion

    #region CellStyle

    [Test]
    public void DefaultCellStyle()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);

      sut.DefaultCellStyle.HAlign = BRHAlign.Center;
      sut.DefaultCellStyle.Italic = true;
      sut.Cells.Select(1, 2);
      sut.Cells.CellStyle.HAlign = BRHAlign.Right;
      sut.Cells.CellStyle.Italic = false;
      for (int i = 0; i < sut.RowCount; i++)
      {
        for (int j = 0; j < sut.ColumnCount; j++)
        {
          string prefix = "RowIndex=" + i.ToString() + ", ColumnIndex=" + j.ToString() + ". ";
          sut.Cells.Select(i, j);
          if (i == 1 && j == 2)
          {
            Assert.AreEqual(BRHAlign.Right, sut.Cells.CellStyle.HAlign, prefix + "HAlign");
            Assert.IsFalse(sut.Cells.CellStyle.Italic, prefix + "Italic");
          }
          else
          {
            Assert.AreEqual(BRHAlign.Center, sut.Cells.CellStyle.HAlign, prefix + "HAlign");
            Assert.IsTrue(sut.Cells.CellStyle.Italic, prefix + "Italic");
          }
        }
      }
    }

    #endregion

    #region Методы

    [Test]
    public void CreateSelector()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      BRSelector sel = sut.CreateSelector();
      Assert.IsInstanceOf<BRTableCellSelector>(sel, "GetType()");
      Assert.AreNotSame(sut.Cells, sel, "Cells");
      Assert.AreSame(sut, sel.Band, "Band");
    }

    [Test]
    public void SetValue()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      sut.SetValue(1, 2, 123);

      for (int i = 0; i < sut.RowCount; i++)
      {
        for (int j = 0; j < sut.ColumnCount; j++)
        {
          string prefix = "RowIndex=" + i.ToString() + ", ColumnIndex=" + j.ToString() + ". ";
          sut.Cells.Select(i, j);
          if (i == 1 && j == 2)
            Assert.AreEqual(123, sut.Cells.Value, prefix);
          else
            Assert.IsNull(sut.Cells.Value, prefix);
        }
      }
    }

    [Test]
    public void SetFormat()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      sut.SetFormat(1, 2, "0.0");

      for (int i = 0; i < sut.RowCount; i++)
      {
        for (int j = 0; j < sut.ColumnCount; j++)
        {
          string prefix = "RowIndex=" + i.ToString() + ", ColumnIndex=" + j.ToString() + ". ";
          sut.Cells.Select(i, j);
          if (i == 1 && j == 2)
            Assert.AreEqual("0.0", sut.Cells.CellStyle.Format, prefix);
          else
            Assert.AreEqual("", sut.Cells.CellStyle.Format, prefix);
        }
      }
    }

    [Test]
    public void AreAllBordersSame_1()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      Assert.IsTrue(sut.AreAllBordersSame, "#1");

      sut.Cells.Select(1, 2);
      sut.Cells.CellStyle.LeftBorder = BRLine.Thin;
      Assert.IsFalse(sut.AreAllBordersSame, "#2");

      sut.Cells.CellStyle.LeftBorder = BRLine.None;
      Assert.IsTrue(sut.AreAllBordersSame, "#3");

      sut.Cells.CellStyle.DiagonalUp = BRLine.Thick;
      sut.Cells.CellStyle.DiagonalDown = BRLine.Thick;
      Assert.IsTrue(sut.AreAllBordersSame, "#4");
    }

    [Test]
    public void AreAllBordersSame_2()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable sut = sect.Bands.Add(5, 3);
      sut.DefaultCellStyle.AllBorders = BRLine.Thin;
      Assert.IsTrue(sut.AreAllBordersSame, "#1");

      sut.Cells.Select(0, 0);
      sut.Cells.CellStyle.LeftBorder = new BRLine(BRLineStyle.Thin, BRColor.Red);
      Assert.IsFalse(sut.AreAllBordersSame, "#2");

      sut.Cells.CellStyle.LeftBorder = BRLine.Thin;
      Assert.IsTrue(sut.AreAllBordersSame, "#3");
    }

    #endregion
  }
}
