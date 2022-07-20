﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Shell;

namespace ExtTools_tests.Shell
{
  [TestFixture]
  public class ExcelRectangleTests
  {
    #region Конструктор

    [TestCase("A1", "A1", "A1")]
    [TestCase("A1", "A2", "A1:A2")]
    [TestCase("A2", "A1", "A1:A2")]
    [TestCase("A1", "B3", "A1:B3")]
    [TestCase("B3", "A1", "A1:B3")]
    [TestCase("A3", "B1", "A1:B3")]
    [TestCase("B1", "A3", "A1:B3")]
    public void Constructor_ExcelCell(string s1, string s2, string wantedText)
    {
      ExcelCell cell1 = ExcelCell.Parse(s1);
      ExcelCell cell2 = ExcelCell.Parse(s2);

      ExcelRectangle sut = new ExcelRectangle(cell1, cell2);
      Assert.AreEqual(wantedText, sut.ToString(), "ToString()");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.IsTrue(sut.FirstCell.Row <= sut.LastCell.Row, "Row");
      Assert.IsTrue(sut.FirstCell.Column <= sut.LastCell.Column, "Column");
    }

    [Test]
    public void Constructor_ExcelCell_exception()
    {
      ExcelRectangle dummy;
      Assert.Catch<ArgumentException>(delegate()
      {
        dummy = new ExcelRectangle(new ExcelCell(), new ExcelCell(1,1));
      }, "#1");

      Assert.Catch<ArgumentException>(delegate()
      {
        dummy = new ExcelRectangle(new ExcelCell(1, 1), new ExcelCell());
      }, "#2");
    }


    [TestCase(1, 1, 1, 1, "A1")]
    [TestCase(1, 1, 3, 2, "A1:B3")]
    [TestCase(3, 1, 1, 2, "A1:B3")]
    [TestCase(1, 2, 3, 1, "A1:B3")]
    [TestCase(3, 1, 1, 2, "A1:B3")]
    public void Constructor_int(int r1, int c1, int r2, int c2, string wantedText)
    {
      ExcelRectangle sut = new ExcelRectangle(r1, c1, r2, c2);
      Assert.AreEqual(wantedText, sut.ToString(), "ToString()");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.IsTrue(sut.FirstCell.Row <= sut.LastCell.Row, "RowNumber");
      Assert.IsTrue(sut.FirstCell.Column <= sut.LastCell.Column, "ColumnNumber");
    }

    [TestCase(0, 1, 1, 1)]
    [TestCase(1, 0, 1, 1)]
    [TestCase(1, 1, 0, 1)]
    [TestCase(1, 1, 1, 0)]
    public void Constructor_int_exception(int r1, int c1, int r2, int c2)
    {
      ExcelRectangle dummy;
      Assert.Catch<ArgumentException>(delegate() { dummy = new ExcelRectangle(r1,c1,r2,c2); });
    }

    #endregion

    #region Преобразование из строки

    [TestCase("A1", true, "A1")]
    [TestCase("B3:A1", true, "A1:B3")]
    [TestCase("A0", false, "")]
    [TestCase("B3:A0", false, "")]
    [TestCase("", true, "")]
    public void Parse_TryParse(string s, bool wantedRes, string wantedValue)
    {
      ExcelRectangle value1;
      bool res = ExcelRectangle.TryParse(s, out value1);
      Assert.AreEqual(wantedRes, res, "TryParse() result");
      Assert.AreEqual(wantedValue, value1.ToString(), "TryParse() value");

      if (res)
      {
        ExcelRectangle value2 = ExcelRectangle.Parse(s);
        Assert.AreEqual(wantedValue, value2.ToString(), "Parse() result");
      }
      else
      {
        ExcelRectangle value2;
        Assert.Catch<FormatException>(delegate() { value2 = ExcelRectangle.Parse(s); }, "Parse throws");
      }
    }

    #endregion

    #region Сравнение

    [TestCase("A1", "A1", true)]
    [TestCase("A1:B3", "A3:B1", true)]
    [TestCase("A1", "B1", false)]
    [TestCase("A1:B3", "A1:C3", false)]
    [TestCase("A1:B3", "A1:B2", false)]
    [TestCase("A1:B3", "A2:B3", false)]
    [TestCase("A1:B3", "B1:B3", false)]
    [TestCase("A1:B3", "", false)]
    [TestCase("", "A1:B3", false)]
    [TestCase("", "", true)]
    public void Equals(string s1, string s2, bool wantedEq)
    {
      ExcelRectangle sut1 = ExcelRectangle.Parse(s1);
      ExcelRectangle sut2 = ExcelRectangle.Parse(s2);

      bool res1 = (sut1 == sut2);
      Assert.AreEqual(wantedEq, res1, "operator ==");

      bool res2 = (sut1 != sut2);
      Assert.AreEqual(!wantedEq, res2, "operator !=");

      bool res3 = (sut1.Equals(sut2));
      Assert.AreEqual(wantedEq, res3, "Equals(ExcelRCRectangeRange)");

      bool res4 = Object.Equals(sut1, sut2);
      Assert.AreEqual(wantedEq, res4, "Equals(Object)");
    }

    #endregion

    #region Contains()

    [TestCase("B2:D4", "C3", true)]
    [TestCase("A1", "A1", true)]
    [TestCase("B2:D4", "B2", true)]
    [TestCase("B2:D4", "D4", true)]
    [TestCase("B2:D4", "B4", true)]
    [TestCase("B2:D4", "D4", true)]
    [TestCase("B2:D4", "A2", false)]
    [TestCase("B2:D4", "E2", false)]
    [TestCase("B2:D4", "B1", false)]
    [TestCase("B2:D4", "B5", false)]
    [TestCase("B2:D4", "", false)]
    [TestCase("", "B2", false)]
    [TestCase("", "", false)]
    public void Contains_ExcelCell(string sRect, string sCell, bool wantedRes)
    {
      ExcelRectangle sut = ExcelRectangle.Parse(sRect);
      ExcelCell cell = ExcelCell.Parse(sCell);

      bool res = sut.Contains(cell);

      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("B2:E5", "C3:D4", true)]
    [TestCase("B2:E5", "B2:E5", true)]
    [TestCase("B2:E5", "A2:E5", false)]
    [TestCase("B2:E5", "B2:F5", false)]
    [TestCase("B2:E5", "B1:E5", false)]
    [TestCase("B2:E5", "B2:E6", false)]
    [TestCase("A1", "A1", true)]
    [TestCase("B2:E5", "", false)]
    [TestCase("", "A1", false)]
    [TestCase("", "", false)]
    public void Contains_ExcelRectange(string sRect, string sArg, bool wantedRes)
    {
      ExcelRectangle sut = ExcelRectangle.Parse(sRect);
      ExcelRectangle arg = ExcelRectangle.Parse(sArg);

      bool res = sut.Contains(arg);

      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region GetCross()/IsCrossed()

    // Попадание 4 ячеек по углам
    [TestCase("B2:D4", "A1:C3", "B2:C3")]
    [TestCase("B2:D4", "C1:E3", "C2:D3")]
    [TestCase("B2:D4", "C3:E5", "C3:D4")]
    [TestCase("B2:D4", "A3:C5", "B3:C4")]

    // Попадание 2 ячеек по углам
    [TestCase("B2:D4", "A2:B2", "B2")]
    [TestCase("B2:D4", "B1:B2", "B2")]
    [TestCase("B2:D4", "D1:D2", "D2")]
    [TestCase("B2:D4", "D2:E2", "D2")]
    [TestCase("B2:D4", "D4:E4", "D4")]
    [TestCase("B2:D4", "D4:D5", "D4")]
    [TestCase("B2:D4", "B4:B5", "B4")]
    [TestCase("B2:D4", "A4:B4", "B4")]

    // Попадание одной угловой ячейки
    [TestCase("B2:D4", "B2", "B2")]
    [TestCase("B2:D4", "D2", "D2")]
    [TestCase("B2:D4", "D4", "D4")]
    [TestCase("B2:D4", "B4", "B4")]

    // Попадание всей области
    [TestCase("B2:D4", "B2:C3", "B2:C3")]

    // Промахи
    [TestCase("B2:D4", "B1:D1", "")]
    [TestCase("B2:D4", "E2:E4", "")]
    [TestCase("B2:D4", "B5:D5", "")]
    [TestCase("B2:D4", "A2:A4", "")]

    // Пустые области
    [TestCase("B2:D4", "", "")]
    [TestCase("", "", "")]
    public void GetCross_IsCrossed(string s1, string s2, string sWanted)
    {
      ExcelRectangle r1 = ExcelRectangle.Parse(s1);
      ExcelRectangle r2 = ExcelRectangle.Parse(s2);
      ExcelRectangle wantedRes = ExcelRectangle.Parse(sWanted);

      ExcelRectangle res1 = ExcelRectangle.GetCross(r1, r2);
      Assert.AreEqual(wantedRes, res1, "GetCross()");

      bool res2 = ExcelRectangle.IsCrossed(r1, r2);
      Assert.AreEqual(sWanted.Length > 0, res2, "IsCrossed()");

      ExcelRectangle res3 = ExcelRectangle.GetCross(r2, r1);
      Assert.AreEqual(wantedRes, res3, "GetCross() inverted");

      bool res4 = ExcelRectangle.IsCrossed(r2, r1);
      Assert.AreEqual(sWanted.Length > 0, res4, "IsCrossed() inverted");
    }

    #endregion

    #region Прочие

    [Test]
    public void IsEmpty()
    {
      ExcelRectangle sut = new ExcelRectangle();
      Assert.IsTrue(sut.IsEmpty);
    }


    [Test]
    public void Empty()
    {
      Assert.IsTrue(ExcelRectangle.Empty.IsEmpty);
      Assert.AreEqual("", ExcelRectangle.Empty.ToString(), "ToString()");
      Assert.AreEqual(0, ExcelRectangle.Empty.RowCount, "RowCount");
      Assert.AreEqual(0, ExcelRectangle.Empty.ColumnCount, "ColumnCount");
    }

    [TestCase("B2:F4", 3)]
    [TestCase("B2:F2", 1)]
    [TestCase("", 0)]
    public void RowCount(string sSUT, int wantedRes)
    {
      ExcelRectangle sut = ExcelRectangle.Parse(sSUT);
      Assert.AreEqual(wantedRes, sut.RowCount);
    }

    [TestCase("B2:F4", 5)]
    [TestCase("B2:B4", 1)]
    [TestCase("", 0)]
    public void ColumnCount(string sSUT, int wantedRes)
    {
      ExcelRectangle sut = ExcelRectangle.Parse(sSUT);
      Assert.AreEqual(wantedRes, sut.ColumnCount);
    }

    [TestCase("B2:F4", 15)]
    [TestCase("B2:B4", 3)]
    [TestCase("B2", 1)]
    [TestCase("", 0)]
    public void CellCount(string sSUT, int wantedRes)
    {
      ExcelRectangle sut = ExcelRectangle.Parse(sSUT);
      Assert.AreEqual(wantedRes, sut.CellCount);
    }

    #endregion
  }
}
