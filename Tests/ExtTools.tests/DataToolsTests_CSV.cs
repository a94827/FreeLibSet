using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_CSV
  {
    [TestCase("ABC|DEF", Result = "ABC,DEF")]
    [TestCase("|ABC|DEF", Result = ",ABC,DEF")]
    [TestCase("ABC|DEF|", Result = "ABC,DEF,")]
    [TestCase("A,BC|DEF", Result = "\"A,BC\",DEF")]
    public string CommaStringFromArray(string s1)
    {
      string[] a = s1.Split('|');
      return DataTools.CommaStringFromArray(a);
    }

    [Test]
    public void CommaStringFromArray_empty()
    {
      string s1 = DataTools.CommaStringFromArray(new string[] { });
      Assert.AreEqual("", s1);

      string s2 = DataTools.CommaStringFromArray(null);
      Assert.AreEqual("", s2);
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
    public string[] CommaStringToArray(string s)
    {
      return DataTools.CommaStringToArray(s);
    }

    [TestCase("1,2,3,0", Result = "1|2|3|0")]
    public string CommaStringToIds(string s)
    {
      Int32[] a1 = DataTools.CommaStringToIds(s);
      string[] a2 = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = StdConvert.ToString(a1[i]);
      return String.Join("|", a2);
    }

    [TestCase("1|2|3", false, Result = "1,2,3")]
    [TestCase("1|2|3", true, Result = "1, 2, 3")]
    public string CommaStringFromIds(string s, bool addSpace)
    {
      string[] a1 = s.Split('|');
      Int32[] a2 = new Int32[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = StdConvert.ToInt32(a1[i]);

      return DataTools.CommaStringFromIds(a2, addSpace);
    }

    [Test]
    public void CommaStringFromIds_empty()
    {
      string s1 = DataTools.CommaStringFromIds(new Int32[] { }, false);
      Assert.AreEqual("", s1);

      string s2 = DataTools.CommaStringFromIds(null, false);
      Assert.AreEqual("", s2);
    }
  }
}
