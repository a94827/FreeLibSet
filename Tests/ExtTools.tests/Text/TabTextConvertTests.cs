using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools_tests.Text
{
  [TestFixture]
  public class TabTextConvertTests
  {
    #region Конструкторы

    [Test]
    public void Constructor()
    {
      TabTextConvert sut = new TabTextConvert();
      Assert.AreEqual(Environment.NewLine, sut.NewLine, "NewLine");
      Assert.IsFalse(sut.AutoDetectNewLine, "AutoDetectNewLine");
    }

    #endregion

    #region Методы

    [TestCase("ABC/DEF", "ABC\tDEF")]
    [TestCase("/ABC/DEF", "\tABC\tDEF")]
    [TestCase("ABC/DEF/", "ABC\tDEF\t")]
    [TestCase("A,BC/DEF", "A,BC\tDEF")]
    public void ToString_1(string s1, string wanted)
    {
      string[] a = s1.Split('/');

      TabTextConvert sut = new TabTextConvert();
      Assert.AreEqual(wanted, sut.ToString(a));
    }

    [Test]
    public void ToString_1_empty()
    {
      TabTextConvert sut = new TabTextConvert();
      string s1 = sut.ToString(new string[] { });
      Assert.AreEqual("", s1);

      string s2 = sut.ToString((string[])null);
      Assert.AreEqual("", s2);
    }

    [TestCase("ABC/DEF|GHI/JKL", "ABC\tDEF|GHI\tJKL|")]
    [TestCase("ABC/DEF", "ABC\tDEF|")]
    public void ToString_2(string s1, string wanted)
    {
      string[] a1 = s1.Split('|');
      string[][] a2 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].Split('/');
      string[,] a3 = DataTools.MatrixFromRows<string>(a2);

      TabTextConvert sut = new TabTextConvert();
      Assert.AreEqual(wanted.Replace("|", Environment.NewLine), sut.ToString(a3));
    }

    [TestCase("ABC\tDEF", Result = new string[] { "ABC", "DEF" })]
    [TestCase("ABC\tDEF\t", Result = new string[] { "ABC", "DEF", "" })]
    [TestCase("ABC\tDEF\t\t", Result = new string[] { "ABC", "DEF", "", "" })]
    [TestCase("\t\tABC\tDEF", Result = new string[] { "", "", "ABC", "DEF" })]
    [TestCase("ABC\t\tDEF", Result = new string[] { "ABC", "", "DEF" })]
    [TestCase("ABC\t\t\tDEF", Result = new string[] { "ABC", "", "", "DEF" })]
    public string[] ToArray(string s)
    {
      TabTextConvert sut = new TabTextConvert();

      return sut.ToArray(s);
    }

    #endregion

    #region Управляющие свойства

    [Test]
    public void NewLine()
    {
      TabTextConvert sut = new TabTextConvert();
      sut.NewLine = "@";

      string[,] a = new string[2, 2];
      a[0, 0] = "AAA";
      a[0, 1] = "BBB";
      a[1, 0] = "CCC";
      a[1, 1] = "DDD";

      string res1 = sut.ToString(a);
      Assert.AreEqual("AAA\tBBB@CCC\tDDD@", res1, "ToString()");

      string[,] res2 = sut.ToMatrix("AAA\tBBB@CCC\tDDD@");
      Assert.AreEqual(a, res2, "ToMatrix()");
    }

    [Test]
    public void AutoDetectNewLine()
    {
      TabTextConvert sut = new TabTextConvert();
      sut.AutoDetectNewLine = true;

      string[,] res = sut.ToMatrix("AAA,BBB\rCCC,DDD\r");
      Assert.AreEqual(2, res.GetLength(0));
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      TabTextConvert sut = new TabTextConvert();
      for (int i = 0; i <= 3; i++)
      {
        Clone_ModifySUT(sut, i);

        TabTextConvert res = sut.Clone();

        Assert.AreEqual(sut.AutoDetectNewLine, res.AutoDetectNewLine, "AutoDetectNewLine");
        Assert.AreEqual(sut.NewLine, res.NewLine, "NewLine");
      }
    }

    private static void Clone_ModifySUT(TabTextConvert sut, int i)
    {
      switch (i)
      {
        case 1: sut.AutoDetectNewLine = true; break;
        case 2: sut.NewLine = "\r"; break;
        case 3: sut.NewLine = "\n"; break;
      }
    }

    #endregion
  }
}
