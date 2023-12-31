using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRPageSetupTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      BRPageSetup sut = new BRPageSetup();
      CompareProps(BRReport.AppDefaultPageSetup, sut);
    }

    internal static  void CompareProps(BRPageSetup wanted, BRPageSetup sut)
    {
      Assert.AreEqual(wanted.PaperHeight, sut.PaperHeight, "PaperHeight");
      Assert.AreEqual(wanted.PaperWidth, sut.PaperWidth, "PaperWidth");
      Assert.AreEqual(wanted.Orientation, sut.Orientation, "Orientation");
      Assert.AreEqual(wanted.LeftMargin, sut.LeftMargin, "LeftMargin");
      Assert.AreEqual(wanted.TopMargin, sut.TopMargin, "TopMargin");
      Assert.AreEqual(wanted.RightMargin, sut.RightMargin, "RightMargin");
      Assert.AreEqual(wanted.BottomMargin, sut.BottomMargin, "BottomMargin");
      Assert.AreEqual(wanted.CenterVertical, sut.CenterVertical, "CenterVertical");
      Assert.AreEqual(wanted.CenterHorizontal, sut.CenterHorizontal, "CenterHorizontal");
      Assert.AreEqual(wanted.DuplexNewPage, sut.DuplexNewPage, "DuplexNewPage");
    }

    #endregion

    #region PrintArea

    [Test]
    public void PrintAreaHeight()
    {
      BRPageSetup sut = new BRPageSetup();
      sut.PaperHeight = 1234;
      sut.TopMargin = 123;
      sut.BottomMargin = 456;
      Assert.AreEqual(1234 - 123 - 456, sut.PrintAreaHeight);
    }

    [Test]
    public void PrintAreaWidth()
    {
      BRPageSetup sut = new BRPageSetup();
      sut.PaperWidth = 1234;
      sut.LeftMargin = 123;
      sut.RightMargin = 456;
      Assert.AreEqual(1234 - 123 - 456, sut.PrintAreaWidth);
    }

    #endregion

    #region Ориентация

    [Test]
    public void Orientation()
    {
      BRPageSetup sut = CreateSUT();

      sut.Orientation = BROrientation.Landscape;

      TestSizesNormal(sut, "");
    }

    [Test]
    public void SetOrientation_false()
    {
      BRPageSetup sut = CreateSUT();
      sut.SetOrientation(BROrientation.Landscape, false);
      TestSizesNormal(sut, "#1. ");
      sut.SetOrientation(BROrientation.Portrait, false);
      TestSizesNormal(sut, "#2. ");
    }

    [Test]
    public void SetOrientation_true()
    {
      BRPageSetup sut = CreateSUT();
      sut.SetOrientation(BROrientation.Landscape, true);
      TestSizesRotated(sut, "#1. ");
      sut.SetOrientation(BROrientation.Landscape, true);
      TestSizesRotated(sut, "#2. ");
      sut.SetOrientation(BROrientation.Portrait, true);
      TestSizesNormal(sut, "#3. ");
    }

    [Test]
    public void InvertOrientation()
    {
      BRPageSetup sut = CreateSUT();

      sut.InvertOrientation();
      Assert.AreEqual(BROrientation.Landscape, sut.Orientation, "Orientation #1");
      TestSizesRotated(sut, "#1. ");

      sut.InvertOrientation();
      Assert.AreEqual(BROrientation.Portrait, sut.Orientation, "Orientation #1");
      TestSizesNormal(sut, "#2. ");
    }

    internal static BRPageSetup CreateSUT()
    {
      BRPageSetup sut = new BRPageSetup();
      sut.Orientation = BROrientation.Portrait;
      sut.PaperHeight = 2500;
      sut.PaperWidth = 2000;
      sut.LeftMargin = 100;
      sut.TopMargin = 200;
      sut.RightMargin = 300;
      sut.BottomMargin = 400;
      return sut;
    }

    internal static void TestSizesNormal(BRPageSetup sut, string msgPrefix)
    {
      Assert.AreEqual(2500, sut.PaperHeight, msgPrefix+"PaperHeight");
      Assert.AreEqual(2000, sut.PaperWidth, msgPrefix + "PaperWidth");
      Assert.AreEqual(100, sut.LeftMargin, msgPrefix + "LeftMargin");
      Assert.AreEqual(200, sut.TopMargin, msgPrefix + "TopMargin");
      Assert.AreEqual(300, sut.RightMargin, msgPrefix + "RightMargin");
      Assert.AreEqual(400, sut.BottomMargin, msgPrefix + "BottomMargin");
    }

    internal static void TestSizesRotated(BRPageSetup sut, string msgPrefix)
    {
      Assert.AreEqual(2000, sut.PaperHeight, msgPrefix + "PaperHeight");
      Assert.AreEqual(2500, sut.PaperWidth, msgPrefix + "PaperWidth");
      Assert.AreEqual(400, sut.LeftMargin, msgPrefix + "BottomMargin");
      Assert.AreEqual(100, sut.TopMargin, msgPrefix + "LeftMargin");
      Assert.AreEqual(200, sut.RightMargin, msgPrefix + "TopMargin");
      Assert.AreEqual(300, sut.BottomMargin, msgPrefix + "RightMargin");
    }

    #endregion

    #region Клонирование

    [Test]
    public void Clone()
    {
      BRPageSetup sut = CreateSUT();
      BRPageSetup res = sut.Clone();
      TestSizesNormal(res, "");
    }

    #endregion
  }
}
