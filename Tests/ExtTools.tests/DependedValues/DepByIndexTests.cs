using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  public class DepByIndexTests
  {
    [Test]
    public void DynamicTest()
    {
      DepInput<int> indexArg = new DepInput<int>(0, null);
      DepInput<string> v1 = new DepInput<string>("AAA", null);
      DepInput<string> v2 = new DepInput<string>("BBB", null);   
      DepInput<string> v3 = new DepInput<string>("CCC", null);
      DepInput<string> vDef = new DepInput<string>("ZZZ", null);
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
