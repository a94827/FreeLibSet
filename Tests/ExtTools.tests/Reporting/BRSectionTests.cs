using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;
using FreeLibSet.Tests;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRSectionTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      BRReport report = new BRReport();
      BRSection sut1 = report.Sections.Add();
      BRSection sut2 = report.Sections.Add();
      Assert.AreSame(report, sut2.Report);
      Assert.AreEqual(0, sut1.SectionIndex, "SectionIndex #1");
      Assert.AreEqual(1, sut2.SectionIndex, "SectionIndex #2");
      Assert.AreEqual(0, sut2.Bands.Count, "Bands.Count");
      ExtStringAssert.IsNotNullOrEmpty(sut2.Name, "Name");
      Assert.AreNotEqual(sut1.Name, sut2.Name, "Names different");
      Assert.AreNotSame(sut1.PageSetup, sut2.PageSetup, "PageSetup");
    }

    #endregion

    #region Полосы

    [Test]
    public void Bands_Add_Table()
    {
      BRReport report = new BRReport();
      BRSection sut = report.Sections.Add();
      BRTable res = sut.Bands.Add(2, 3);
      Assert.AreEqual(1, sut.Bands.Count, "Bands.Count");
      CollectionAssert.AreEqual(new BRBand[] { res }, sut.Bands, "Bands");
      Assert.AreEqual(2, res.RowCount, "RowCount");
      Assert.AreEqual(3, res.ColumnCount, "ColumnCount");
      Assert.AreEqual(0, res.BandIndex, "BandIndex");
      Assert.AreSame(sut, res.Section, "Section");
      BRCellStyleStorageTests.CompareProps(report.DefaultCellStyle, res.DefaultCellStyle);
    }

    private class TestBand : BRBand
    {
      public TestBand(BRSection sect)
        : base(sect, 3, 4)
      {
      }

      public override BRSelector CreateSelector()
      {
        return new TestSelector(this);
      }
    }

    private class TestSelector : BRSelector
    {
      public TestSelector(TestBand band)
        : base(band)
      {
      }

      public override BRCellStyle CellStyle
      {
        get
        {
          throw new NotImplementedException();
        }
      }

      public override BRColumnInfo ColumnInfo
      {
        get
        {
          throw new NotImplementedException();
        }
      }

      public override BRRowInfo RowInfo
      {
        get
        {
          throw new NotImplementedException();
        }
      }

      public override object Value
      {
        get
        {
          throw new NotImplementedException();
        }

        set
        {
          throw new NotImplementedException();
        }
      }
    }

    [Test]
    public void Bands_Add_Band()
    {
      BRReport report = new BRReport();
      BRSection sut = report.Sections.Add();
      TestBand band = new TestBand(sut);
      sut.Bands.Add(band);

      Assert.AreEqual(1, sut.Bands.Count, "Bands.Count");
      CollectionAssert.AreEqual(new BRBand[] { band }, sut.Bands, "Bands");
      Assert.AreEqual(0, band.BandIndex, "BandIndex");
      Assert.AreSame(sut, band.Section, "Section");
    }

    [Test]
    public void Bands_IndexOf()
    {
      BRReport report = new BRReport();
      BRSection sut = report.Sections.Add();
      BRTable band1 = sut.Bands.Add(3, 4);
      BRTable band2 = sut.Bands.Add(1, 2);
      Assert.AreEqual(0, sut.Bands.IndexOf(band1), "#1");
      Assert.AreEqual(1, sut.Bands.IndexOf(band2), "#2");
    }


    [Test]
    public void Bands_GetEnumerator()
    {
      BRReport report = new BRReport();
      BRSection sut = report.Sections.Add();
      BRTable band1 = sut.Bands.Add(3, 4);
      BRTable band2 = sut.Bands.Add(1, 2);

      CollectionAssert.AreEqual(new BRBand[] { band1, band2 }, sut.Bands);
    }

    #endregion

    #region Name

    [Test]
    public void Name()
    {
      BRReport report = new BRReport();
      BRSection sut = report.Sections.Add();

      sut.Name = "ABC";
      Assert.AreEqual("ABC", sut.Name, "#1");

      sut.Name = String.Empty;
      ExtStringAssert.IsNotNullOrEmpty(sut.Name, "#2");
    }

    #endregion
  }
}
