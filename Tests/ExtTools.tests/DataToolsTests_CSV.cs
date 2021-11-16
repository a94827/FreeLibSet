using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools.tests
{
#if XXX // TODO: StdConvertTests

  [TestFixture]
  class DataToolsTests_CSV
  {
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

#endif
}
