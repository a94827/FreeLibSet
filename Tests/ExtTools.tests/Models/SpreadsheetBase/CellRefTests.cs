using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.SpreadsheetBase;

namespace ExtTools_tests.Models.SpreadsheetBase
{
  [TestFixture]
  public class CellRefTests
  {
    #region Конструктор

    [TestCase(1, 1, "A1")]
    [TestCase(100, 1, "A100")]
    [TestCase(1, 100, "CV1")]
    [TestCase(1048576, 16384, "XFD1048576")] // предел для Excel-2007+
    public void Constructor(int row, int column, string wantedText)
    {
      CellRef sut = new CellRef(row, column);

      Assert.AreEqual(row, sut.Row, "RowNumber");
      Assert.AreEqual(column, sut.Column, "ColumnNumber");
      Assert.AreEqual(wantedText, sut.ToString(), "ToString()");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    public void Constructor_exception(int row, int column)
    {
      CellRef dummy;
      Assert.Catch<ArgumentException>(delegate() { dummy = new CellRef(row, column); });
    }

    #endregion

    #region Преобразование в строку

    [TestCase(2, 1, CellRefFormat.A1, "A2")]
    [TestCase(2, 1, CellRefFormat.Abs, "$A$2")]
    [TestCase(2, 1, CellRefFormat.R1C1, "R2C1")]
    public void ToString(int row, int column, CellRefFormat format, string wantedText)
    {
      CellRef sut = new CellRef(row, column);
      string res = sut.ToString(format);
      Assert.AreEqual(wantedText, res);
    }

    #endregion

    #region Преобразование из строки

    [TestCase("A1", true, "A1")]
    [TestCase("A0", false, "")]
    [TestCase("123", false, "")]
    [TestCase("ABC", false, "")]
    [TestCase("1A", false, "")]
    [TestCase("A1B", false, "")]
    [TestCase("XFD1048576", true, "XFD1048576")]

    [TestCase("$A$1", true, "A1")]
    [TestCase("$A$0", false, "")]
    [TestCase("$A", false, "")]
    [TestCase("$0", false, "")]
    [TestCase("$$A$1", false, "")]
    [TestCase("$A$$1", false, "")]

    [TestCase("$A1", true, "A1")]
    [TestCase("A$1", true, "A1")]
    [TestCase("A1$", false, "")]

    [TestCase("R5C4", true, "D5")]
    [TestCase("R123C28", true, "AB123")]
    [TestCase("R0C4", false, "")]
    [TestCase("R5C0", false, "")]
    [TestCase("R0C", false, "")]
    [TestCase("RR5C4", false, "")]
    [TestCase("R5CC4", false, "")]
    [TestCase("$R5C4", false, "")]
    [TestCase("R5$C4", false, "")]

    [TestCase("", true, "")]
    public void Parse_TryParse(string s, bool wantedRes, string wantedValue)
    {
      CellRef value1;
      bool res = CellRef.TryParse(s, out value1);
      Assert.AreEqual(wantedRes, res, "TryParse() result");
      if (res)
      {
        Assert.AreEqual(wantedValue, value1.ToString(), "TryParse() value");

        CellRef value2 = CellRef.Parse(s);
        Assert.AreEqual(wantedValue, value2.ToString(), "Parse() result");
      }
      else
      {
        CellRef value2;
        Assert.Catch<FormatException>(delegate() { value2 = CellRef.Parse(s); }, "Parse throws");
      }
    }

    #endregion

    #region Сравнение

    [TestCase("A1", "A1", true)]
    [TestCase("A1", "A2", false)]
    [TestCase("A1", "B1", false)]
    [TestCase("A1", "", false)]
    [TestCase("", "A1", false)]
    [TestCase("", "", true)]
    public void Equals(string s1, string s2, bool wantedEq)
    {
      CellRef sut1 = CellRef.Parse(s1);
      CellRef sut2 = CellRef.Parse(s2);

      bool res1 = (sut1 == sut2);
      Assert.AreEqual(wantedEq, res1, "operator ==");

      bool res2 = (sut1 != sut2);
      Assert.AreEqual(!wantedEq, res2, "operator !=");

      bool res3 = (sut1.Equals(sut2));
      Assert.AreEqual(wantedEq, res3, "Equals(CellRef)");

      bool res4 = Object.Equals(sut1, sut2);
      Assert.AreEqual(wantedEq, res4, "Equals(Object)");
    }

    #endregion

    #region Прочие

    [Test]
    public void IsEmpty()
    {
      CellRef sut = new CellRef();
      Assert.IsTrue(sut.IsEmpty);
    }

    [Test]
    public void Empty()
    {
      Assert.IsTrue(CellRef.Empty.IsEmpty);
      Assert.AreEqual("", CellRef.Empty.ToString(), "ToString()");
      Assert.AreEqual("", CellRef.Empty.ToString(CellRefFormat.A1), "ToString(A1)");
      Assert.AreEqual("", CellRef.Empty.ToString(CellRefFormat.Abs), "ToString(Abs)");
      Assert.AreEqual("", CellRef.Empty.ToString(CellRefFormat.R1C1), "ToString(R1C1)");
    }

    #endregion
  }
}
