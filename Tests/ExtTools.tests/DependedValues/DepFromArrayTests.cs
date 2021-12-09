using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  public class DepFromArrayTests
  {
    public void DynamicTest()
    {
      DepInput<int> indexArg = new DepInput<int>(0, null);
      string[] values = new string[] { "AAA", "BBB", "CCC" };
      DepFromArray<string> sut = new DepFromArray<string>(indexArg, values, "ZZZ");

      DepResultProducer<string> resprod = new DepResultProducer<string>(sut);
      Assert.AreEqual("AAA", resprod.ToString(), "Original");

      indexArg.Value = 1;
      Assert.AreEqual("AAA|BBB", resprod.ToString(), "Index changed #1");

      indexArg.Value = 2;
      Assert.AreEqual("AAA|BBB|CCC", resprod.ToString(), "Index changed #2");

      indexArg.Value = 3;
      Assert.AreEqual("AAA|BBB|CCC|ZZZ", resprod.ToString(), "Index changed #3");

      indexArg.Value = -1;
      Assert.AreEqual("AAA|BBB|CCC|ZZZ", resprod.ToString(), "Index changed #4");
    }
  }
}
