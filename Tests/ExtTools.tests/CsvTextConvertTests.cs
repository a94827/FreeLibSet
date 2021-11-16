using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools.tests
{
  [TestFixture]
  public class CsvTextConvertTests
  {
    [Test]
    public void Constructor()
    {
      CsvTextConvert sut = new CsvTextConvert();
      Assert.AreEqual(',', sut.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual(Environment.NewLine, sut.NewLine, "NewLine");
      Assert.IsFalse(sut.AutoDetectNewLine, "AutoDetectNewLine");
      Assert.AreEqual('\"', sut.Quote, "Quote");
      Assert.IsFalse(sut.AlwaysQuote, "AlwaysQuote");
    }

    [TestCase("ABC/DEF", "ABC,DEF")]
    [TestCase("/ABC/DEF", ",ABC,DEF")]
    [TestCase("ABC/DEF/", "ABC,DEF,")]
    [TestCase("A,BC/DEF", "\"A,BC\",DEF")]
    public void ToString_1(string s1, string wanted)
    {
      string[] a = s1.Split('/');

      CsvTextConvert sut = new CsvTextConvert();
      Assert.AreEqual(wanted, sut.ToString(a));
    }

    [Test]
    public void ToString_1_empty()
    {
      CsvTextConvert sut = new CsvTextConvert();
      string s1 = sut.ToString(new string[] { });
      Assert.AreEqual("", s1);

      string s2 = sut.ToString((string[])null);
      Assert.AreEqual("", s2);
    }

    [TestCase("ABC/DEF|GHI/JKL", "ABC,DEF|GHI,JKL|")]
    [TestCase("A\"BC/DEF|GHI/JKL", "\"A\"\"BC\",DEF|GHI,JKL|")]
    [TestCase("ABC/DEF", "ABC,DEF|")]
    public void ToString_2(string s1, string wanted)
    {
      string[] a1 = s1.Split('|');
      string[][] a2 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].Split('/');
      string[,] a3 = DataTools.ToArray2<string>(a2);

      CsvTextConvert sut = new CsvTextConvert();
      Assert.AreEqual(wanted.Replace("|", Environment.NewLine), sut.ToString(a3));
    }

    [TestCase("ABC,DEF", Result = new string[] { "ABC", "DEF" })]
    [TestCase("ABC,DEF,", Result = new string[] { "ABC", "DEF", "" })]
    [TestCase("ABC,DEF,,", Result = new string[] { "ABC", "DEF", "", "" })]
    [TestCase(",,ABC,DEF", Result = new string[] { "", "", "ABC", "DEF" })]
    [TestCase("ABC,,DEF", Result = new string[] { "ABC", "", "DEF" })]
    [TestCase("ABC,,,DEF", Result = new string[] { "ABC", "", "", "DEF" })]
    [TestCase("\"ABC\",\"DE\"\"FG\"", Result = new string[] { "ABC", "DE\"FG" })]
    [TestCase(" ABC, \"DEF\" ", Result = new string[] { "ABC", "DEF" }, Description = "with spaces")]
    [TestCase("", Result = null)]
    //[TestCase("\"ABC", ExpectedException = typeof(ParsingException), Description="last quote missing")]
    //[TestCase("ABC\"", ExpectedException = typeof(ParsingException), Description = "first quote missing")]
    [TestCase("\"AB\"CD\"", ExpectedException = typeof(ParsingException), Description = "middle quote missing")]
    public string[] ToArray(string s)
    {
      CsvTextConvert sut = new CsvTextConvert();

      return sut.ToArray(s);
    }

    [Test]
    public void FieldDelimiter()
    {
      CsvTextConvert sut = new CsvTextConvert();
      sut.FieldDelimiter = ';';

      string res1 = sut.ToString(new string[] { "ABC", "DEF" });
      Assert.AreEqual("ABC;DEF", res1, "ToString()");

      string[] res2 = sut.ToArray("ABC;DEF");
      Assert.AreEqual(2, res2.Length, "ToArray()");
    }

    [Test]
    public void Quote()
    {
      CsvTextConvert sut = new CsvTextConvert();
      sut.Quote = '\'';

      string res1 = sut.ToString(new string[] { "A,BC", "D\'EF", "GHI" });
      Assert.AreEqual("\'A,BC\',\'D\'\'EF',GHI", res1, "ToString()");

      string[] res2 = sut.ToArray("\'A,BC\',\'D\'\'EF',GHI");
      Assert.AreEqual(3, res2.Length, "ToArray()");
    }

    [Test]
    public void AlwaysQuote()
    {
      CsvTextConvert sut = new CsvTextConvert();
      sut.AlwaysQuote = true;

      string res = sut.ToString(new string[] { "ABC", "DEF" });
      Assert.AreEqual("\"ABC\",\"DEF\"", res);
    }

    [Test]
    public void NewLine()
    {
      CsvTextConvert sut = new CsvTextConvert();
      sut.NewLine = "@";

      string[,] a = new string[2, 2];
      a[0, 0] = "AAA";
      a[0, 1] = "BBB";
      a[1, 0] = "CCC";
      a[1, 1] = "DDD";

      string res1 = sut.ToString(a);
      Assert.AreEqual("AAA,BBB@CCC,DDD@", res1, "ToString()");

      string[,] res2 = sut.ToArray2("AAA,BBB@CCC,DDD@");
      Assert.AreEqual(a, res2, "ToArray()");
    }

    [Test]
    public void AutoDetectNewLine()
    {
      CsvTextConvert sut = new CsvTextConvert();
      sut.AutoDetectNewLine = true;

      string[,] res = sut.ToArray2("AAA,BBB\rCCC,DDD\r");
      Assert.AreEqual(2, res.GetLength(0));
    }
  }
}
