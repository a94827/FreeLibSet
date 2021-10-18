using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepByIndexTests
  {
    [TestCase(-1, "ZZZ")]
    [TestCase(0, "AAA")]
    [TestCase(2, "CCC")]
    [TestCase(3, "ZZZ")]
    public void ConstTest(int v, string wanted)
    {
      DepConst<int> indexArg = new DepConst<int>(v);
      DepByIndex<string> sut = new DepByIndex<string>(indexArg, new string[] { "AAA", "BBB", "CCC" }, "ZZZ");
      Assert.AreEqual(wanted, sut.Value);
    }
    [Test]
    public void DynamicTest()
    {
      DepInput<int> indexArg = new DepInput<int>();
      indexArg.Value = 0;
      DepValue<string> v1 = new DepInput<string>();
      v1.Value = "AAA";
      DepValue<string> v2 = new DepInput<string>();
      v2.Value = "BBB";
      DepValue<string> v3 = new DepInput<string>();
      v3.Value = "CCC";
      DepValue<string> vDef = new DepInput<string>();
      v3.Value = "ZZZ";
      DepByIndex<string> sut = new DepByIndex<string>(indexArg, new DepValue<string>[3] { v1, v2, v3 }, vDef);
      DepResultProducer<string> resprod = new DepResultProducer<string>(sut);
      Assert.AreEqual("AAA", resprod.ToString(), "Original");

      indexArg.Value = 1;
      Assert.AreEqual("AAA|BBB", resprod.ToString(), "Index changed #1");

      v2.Value = "BBB2";
      Assert.AreEqual("AAA|BBB|BBB2", resprod.ToString(), "Selected source value changed");

      v3.Value = "CCC3";
      Assert.AreEqual("AAA|BBB|BBB2", resprod.ToString(), "Other source value changed");

      indexArg.Value = 2;
      Assert.AreEqual("AAA|BBB|BBB2|CCC3", resprod.ToString(), "Index changed #2");

      indexArg.Value = 3;
      Assert.AreEqual("AAA|BBB|BBB2|CCC3|ZZZ", resprod.ToString(), "Index changed #3");

      vDef.Value = "ZZZ4";
      Assert.AreEqual("AAA|BBB|BBB2|CCC3|ZZZ|ZZZ4", resprod.ToString(), "Default value changed");
    }
  }
}
