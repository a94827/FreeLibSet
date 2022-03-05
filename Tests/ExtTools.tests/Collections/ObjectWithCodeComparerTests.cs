using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class ObjectWithCodeComparerTests
  {
    [TestCase("AAA", "AAA", 0)]
    [TestCase("AAA", "aaa", -1)]
    [TestCase("aaa", "AAA", 1)]
    [TestCase("AAA", "BBB", -1)]
    public void Ordinal(string v1, string v2, int wanted)
    {
      ObjectWithCode obj1 = new ObjectWithCode(v1);
      ObjectWithCode obj2 = new ObjectWithCode(v2);
      int res = ObjectWithCodeComparer<ObjectWithCode>.Ordinal.Compare(obj1, obj2);
      Assert.AreEqual(wanted, Math.Sign(res));
    }

    [TestCase("AAA", "AAA", 0)]
    [TestCase("AAA", "aaa", 0)]
    [TestCase("aaa", "AAA", 0)]
    [TestCase("AAA", "BBB", -1)]
    public void OrdinalIgnoreCase(string v1, string v2, int wanted)
    {
      ObjectWithCode obj1 = new ObjectWithCode(v1);
      ObjectWithCode obj2 = new ObjectWithCode(v2);
      int res = ObjectWithCodeComparer<ObjectWithCode>.OrdinalIgnoreCase.Compare(obj1, obj2);
      Assert.AreEqual(wanted, Math.Sign(res));
    }
  }
}
