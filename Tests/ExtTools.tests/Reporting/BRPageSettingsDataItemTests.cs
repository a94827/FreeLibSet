using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;
using FreeLibSet.Config;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRPageSettingsDataItemTests
  {
    #region Конструктор

    [Test]
    public void Constructor_0args()
    {
      BRPageSettingsDataItem sut = new BRPageSettingsDataItem();
      Assert.AreNotSame(BRReport.AppDefaultPageSetup, sut.PageSetup, "PageSetup");
      BRPageSetupTests.CompareProps(BRReport.AppDefaultPageSetup, sut.PageSetup);
    }


    [Test]
    public void Constructor_1args()
    {
      BRPageSetup ps = BRPageSetupTests.CreateSUT();
      BRPageSettingsDataItem sut = new BRPageSettingsDataItem(ps);
      Assert.AreSame(ps, sut.PageSetup, "PageSetup");
    }

    #endregion

    #region Read/Write

    [Test]
    public void WriteValues_ReadValues()
    {
      BRPageSettingsDataItem sut = new BRPageSettingsDataItem(BRPageSetupTests.CreateSUT());
      sut.PageSetup.InvertOrientation();
      TempCfg cfg = new TempCfg();
      sut.WriteConfig(cfg);

      BRPageSettingsDataItem res = new BRPageSettingsDataItem();
      res.ReadConfig(cfg);
      BRPageSetupTests.TestSizesRotated(res.PageSetup, "");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      BRPageSettingsDataItem sut = new BRPageSettingsDataItem(BRPageSetupTests.CreateSUT());
      sut.PageSetup.InvertOrientation();

      BRPageSettingsDataItem res = sut.Clone();
      BRPageSetupTests.TestSizesRotated(res.PageSetup, "");
    }


    #endregion
  }
}
