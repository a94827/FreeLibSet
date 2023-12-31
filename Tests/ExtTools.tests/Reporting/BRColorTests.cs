using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRColorTests
  {
    #region Конструктор

    [Test]
    public void Constructor_RGB()
    {
      BRColor sut = new BRColor(50, 100, 150);
      Assert.AreEqual(50, sut.R, "R");
      Assert.AreEqual(100, sut.G, "G");
      Assert.AreEqual(150, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [TestCase(-1, 0, 0)]
    [TestCase(256, 0, 0)]
    [TestCase(0, -1, 0)]
    [TestCase(0, 256, 0)]
    [TestCase(0, 0, -1)]
    [TestCase(0, 0, 256)]
    public void Constructor_RGB_ArgumentOutOfRangeException(int r, int g, int b)
    {
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { new BRColor(r, g, b); });
    }

    #endregion

    #region Предопределенные цвета

    [Test]
    public void Auto()
    {
      BRColor sut = BRColor.Auto;
      Assert.IsTrue(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Black()
    {
      BRColor sut = BRColor.Black;
      Assert.AreEqual(0, sut.R, "R");
      Assert.AreEqual(0, sut.G, "G");
      Assert.AreEqual(0, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Red()
    {
      BRColor sut = BRColor.Red;
      Assert.AreEqual(255, sut.R, "R");
      Assert.AreEqual(0, sut.G, "G");
      Assert.AreEqual(0, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Green()
    {
      BRColor sut = BRColor.Green;
      Assert.AreEqual(0, sut.R, "R");
      Assert.AreEqual(255, sut.G, "G");
      Assert.AreEqual(0, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Blue()
    {
      BRColor sut = BRColor.Blue;
      Assert.AreEqual(0, sut.R, "R");
      Assert.AreEqual(0, sut.G, "G");
      Assert.AreEqual(255, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Yellow()
    {
      BRColor sut = BRColor.Yellow;
      Assert.AreEqual(255, sut.R, "R");
      Assert.AreEqual(255, sut.G, "G");
      Assert.AreEqual(0, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Purple()
    {
      BRColor sut = BRColor.Purple;
      Assert.AreEqual(255, sut.R, "R");
      Assert.AreEqual(0, sut.G, "G");
      Assert.AreEqual(255, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void Cyan()
    {
      BRColor sut = BRColor.Cyan;
      Assert.AreEqual(0, sut.R, "R");
      Assert.AreEqual(255, sut.G, "G");
      Assert.AreEqual(255, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    [Test]
    public void White()
    {
      BRColor sut = BRColor.White;
      Assert.AreEqual(255, sut.R, "R");
      Assert.AreEqual(255, sut.G, "G");
      Assert.AreEqual(255, sut.B, "B");
      Assert.IsFalse(sut.IsAuto, "IsAuto");
    }

    #endregion

    #region Сравнение

    [Test]
    public void Equals()
    {
      BRColor[] values = new BRColor[] { BRColor.Auto, BRColor.Red, BRColor.Green, BRColor.Blue, new BRColor(128, 128, 128) };
      for (int i = 0; i < values.Length; i++)
      {
        for (int j = 0; j < values.Length; j++)
        {
          BRColor v1 = values[i];
          BRColor v2 = values[j];
          bool wantedRes = (i == j);

          bool res1 = v1 == v2;
          Assert.AreEqual(wantedRes, res1, "==");

          bool res2 = v1 != v2;
          Assert.AreEqual(wantedRes, !res2, "!=");

          bool res3 = v1.Equals(v2);
          Assert.AreEqual(wantedRes, res3, "Equals(BRColor)");

          bool res4 = v1.Equals((object)v2);
          Assert.AreEqual(wantedRes, res4, "Equals(object)");
        }
      }
    }

    #endregion
  }
}
