using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

// TODO: Не все методы

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  class DepToolsTests
  {
    [TestCase("ABC", 3)]
    [TestCase(null, 0)]
    public void Length(string s, int wanted)
    {
      Assert.AreEqual(wanted, DepTools.Length(s));
    }

    [TestCase("AbCdE", 0, 2, "Ab")]
    [TestCase("AbCdE", 1, 2, "bC")]
    [TestCase("AbCdE", 3, 2, "dE")]
    [TestCase("AbCdE", 5, 2, "")]
    [TestCase(null, 0, 2, "")]
    [TestCase("AbCdE", -1, 2, "")]
    [TestCase("AbCdE", 1, 0, "")]
    [TestCase("AbCdE", 1, -1, "")]
    public void Substing(string s, int startIndex, int length, string wanted)
    {
      Assert.AreEqual(wanted, DepTools.Substring(s, startIndex, length));
    }

    [TestCase("AbCdE", "Ab", true)]
    [TestCase("AbCdE", "ab", false)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "Ab", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void StartsWithOrdinal(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.StartsWithOrdinal(s, substring));
    }

    [TestCase("AbCdE", "Ab", true)]
    [TestCase("AbCdE", "ab", true)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "Ab", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void StartsWithOrdinalIgnoreCase(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.StartsWithOrdinalIgnoreCase(s, substring));
    }

    [TestCase("AbCdE", "dE", true)]
    [TestCase("AbCdE", "de", false)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "dE", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void EndsWithOrdinal(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.EndsWithOrdinal(s, substring));
    }

    [TestCase("AbCdE", "dE", true)]
    [TestCase("AbCdE", "de", true)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "dE", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void EndsWithOrdinalIgnoreCase(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.EndsWithOrdinalIgnoreCase(s, substring));
    }

    [Test]
    public void ToString_int()
    {
      Assert.AreEqual("123", DepTools.ToString<int>(123));
    }

    private class TestClass
    {
      public override string ToString()
      {
        return "Hello";
      }
    }

    [Test]
    public void ToString_object()
    {
      Assert.AreEqual("Hello", DepTools.ToString<TestClass>(new TestClass()));
    }

    [Test]
    public void ToString_null()
    {
      Assert.AreEqual("", DepTools.ToString<TestClass>(null));
    }

    [TestCase("ABC", "^[0-9]", false)]
    [TestCase("1ABC", "^[0-9]", true)]
    [TestCase(null, "^[0-9]", false)]
    [TestCase("ABC", null, true)]
    public void RegexIsMatch(string s, string pattern, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.RegexIsMatch(s, pattern));
    }
  }
}
