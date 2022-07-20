using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Shell;

namespace ExtTools_tests.Shell
{
  [TestFixture]
  public class ExcelCellTests
  {
    #region Конструктор

    [TestCase(1, 1, "A1")]
    [TestCase(100, 1, "A100")]
    [TestCase(1, 100, "CV1")]
    [TestCase(1048576, 16384, "XFD1048576")] // предел для Excel-2007+
    public void Constructor(int row, int column, string wantedText)
    {
      ExcelCell sut = new ExcelCell(row, column);

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
      ExcelCell dummy;
      Assert.Catch<ArgumentException>(delegate() { dummy = new ExcelCell(row, column); });
    }

    #endregion

    #region Преобразование из строки

    [TestCase("A1", true)]
    [TestCase("A0", false)]
    [TestCase("123", false)]
    [TestCase("ABC", false)]
    [TestCase("1A", false)]
    [TestCase("A1B", false)]
    [TestCase("XFD1048576", true)]
    [TestCase("", true)]
    public void Parse_TryParse(string s, bool wantedRes)
    {
      ExcelCell value1;
      bool res = ExcelCell.TryParse(s, out value1);
      Assert.AreEqual(wantedRes, res, "TryParse() result");
      if (res)
      {
        Assert.AreEqual(s, value1.ToString(), "TryParse() value");

        ExcelCell value2 = ExcelCell.Parse(s);
        Assert.AreEqual(s, value2.ToString(), "Parse() result");
      }
      else
      {
        ExcelCell value2;
        Assert.Catch<FormatException>(delegate() { value2 = ExcelCell.Parse(s); }, "Parse throws");
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
      ExcelCell sut1 = ExcelCell.Parse(s1);
      ExcelCell sut2 = ExcelCell.Parse(s2);

      bool res1 = (sut1 == sut2);
      Assert.AreEqual(wantedEq, res1, "operator ==");

      bool res2 = (sut1 != sut2);
      Assert.AreEqual(!wantedEq, res2, "operator !=");

      bool res3 = (sut1.Equals(sut2));
      Assert.AreEqual(wantedEq, res3, "Equals(ExcelCell)");

      bool res4 = Object.Equals(sut1, sut2);
      Assert.AreEqual(wantedEq, res4, "Equals(Object)");
    }

    #endregion

    #region Прочие

    [Test]
    public void IsEmpty()
    {
      ExcelCell sut = new ExcelCell();
      Assert.IsTrue(sut.IsEmpty);
    }

    [Test]
    public void Empty()
    {
      Assert.IsTrue(ExcelCell.Empty.IsEmpty);
      Assert.AreEqual("", ExcelCell.Empty.ToString(), "ToString()");
    }

    #endregion
  }
}
