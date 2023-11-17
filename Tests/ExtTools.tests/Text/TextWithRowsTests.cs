using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Text;

namespace ExtTools_tests.Text
{
  [TestFixture]
  public class TextWithRowsTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_autoSep()
    {
      TextWithRows sut1 = new TextWithRows("ABC" + Environment.NewLine + "DEF");
      Assert.AreEqual(2, sut1.RowCount, "RowCount #1");

      TextWithRows sut2 = new TextWithRows("ABC" + Environment.NewLine + "DEF" + Environment.NewLine);
      Assert.AreEqual(2, sut2.RowCount, "RowCount #2");

      TextWithRows sut3 = new TextWithRows(Environment.NewLine + "ABC" + Environment.NewLine + "DEF");
      Assert.AreEqual(3, sut3.RowCount, "RowCount #3");
    }

    [Test]
    public void Constructor_defaultSep()
    {
      TextWithRows sut = new TextWithRows("ABC" + Environment.NewLine + "DEF", TextWithRowsLineSeparators.Default);
      Assert.AreEqual(2, sut.RowCount, "RowCount");
    }

    [TestCase("\r\n")]
    [TestCase("\r")]
    [TestCase("\n")]
    public void Constructor_allSep(string sep)
    {
      TextWithRows sut = new TextWithRows("ABC" + sep + "DEF", TextWithRowsLineSeparators.AllPosible);
      Assert.AreEqual(2, sut.RowCount, "RowCount");
    }

    #endregion

    [TestCase("\r\n")]
    [TestCase("\r")]
    [TestCase("\n")]
    public void Text(string sep)
    {
      string s = "ABC" + sep + "DEF";
      TextWithRows sut = new TextWithRows(s);
      Assert.AreEqual(s, sut.Text);
    }

    [TestCase("\r\n")]
    [TestCase("\r")]
    [TestCase("\n")]
    public void ToArray(string sep)
    {
      string s = "ABC" + sep + "DEF";
      TextWithRows sut = new TextWithRows(s, new string[1] { sep });
      Assert.AreEqual(new string[2] { "ABC", "DEF" }, sut.ToArray());
    }

    [TestCase("\r\n")]
    [TestCase("\r")]
    [TestCase("\n")]
    public void Item(string sep)
    {
      string s = "ABC" + sep + "DEF";
      TextWithRows sut = new TextWithRows(s, new string[1] { sep });
      Assert.AreEqual("ABC", sut[0], "#0");
      Assert.AreEqual("DEF", sut[1], "#1");

      string dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "-1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[2]; }, "-1");
    }

    [TestCase(0, 0, 0)]
    [TestCase(2, 0, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(4, 0, 4)]
    [TestCase(5, 1, 0)]
    [TestCase(7, 1, 2)]
    public void GetPosition(int index, int resRow, int resColumn)
    {
      //                                   0123 4 567
      TextWithRows sut = new TextWithRows("ABC\r\nDEF", new string[] { "\r\n" });
      TextPosition res = sut.GetPosition(index);
      Assert.AreEqual(resRow, res.Row);
      Assert.AreEqual(resColumn, res.Column);
    }

    [TestCase(0, 0)]
    [TestCase(4, 0)]
    [TestCase(5, 1)]
    [TestCase(7, 1)]
    public void GetRow(int index, int resRow)
    {
      //                                   0123 4 567
      TextWithRows sut = new TextWithRows("ABC\r\nDEF", new string[] { "\r\n" });
      int res = sut.GetRow(index);
      Assert.AreEqual(resRow, res);
    }

    [TestCase(0, 0)]
    [TestCase(1, 5)]
    public void GetRowStartIndex(int row, int resIndex)
    {
      //                                   0123 4 567
      TextWithRows sut = new TextWithRows("ABC\r\nDEF", new string[] { "\r\n" });
      int res = sut.GetRowStartIndex(row);
      Assert.AreEqual(resIndex, res);
    }

    [TestCase(0, 3)]
    [TestCase(1, 3)]
    public void GetRowLength(int row, int resLen)
    {
      //                                   0123 4 567
      TextWithRows sut = new TextWithRows("ABC\r\nDEF", new string[] { "\r\n" });
      int res = sut.GetRowLength(row);
      Assert.AreEqual(resLen, res);
    }
    [TestCase(0, 0, 0)]
    [TestCase(0, 2, 2)]
    [TestCase(1, 0, 5)]
    [TestCase(1, 2, 7)]
    public void GetCharIndex(int row, int column, int resIndex)
    {
      //                                   0123 4 567
      TextWithRows sut = new TextWithRows("ABC\r\nDEF", new string[] { "\r\n" });
      int res = sut.GetCharIndex(new TextPosition(row, column));
      Assert.AreEqual(resIndex, res);
    }

    [Test]
    public void GetEnumerator()
    {
      TextWithRows sut = new TextWithRows("ABC" + Environment.NewLine + "DEF");
      List<string> lst = new List<string>();
      foreach (string s in sut)
        lst.Add(s);

      Assert.AreEqual(new string[] { "ABC", "DEF" }, lst.ToArray());
    }
  }
}
