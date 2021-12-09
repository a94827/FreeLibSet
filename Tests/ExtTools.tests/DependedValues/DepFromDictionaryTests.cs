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
  public class DepFromDictionaryTests
  {
    public void DynamicTest()
    {
      Dictionary<float, string> dict=new Dictionary<float, string>();
      dict.Add(0.0f, "AAA");
      dict.Add(3.0f, "BBB");
      dict.Add(5.0f, "CCC");


      DepInput<float> keyArg = new DepInput<float>(0f, null);
      DepFromDictionary<float, string> sut = new DepFromDictionary<float,string>(keyArg, dict, "ZZZ");

      DepResultProducer<string> resprod = new DepResultProducer<string>(sut);
      Assert.AreEqual("AAA", resprod.ToString(), "Original");

      keyArg.Value = 3.0f;
      Assert.AreEqual("AAA|BBB", resprod.ToString(), "Key changed #1");

      keyArg.Value = 5.0f;
      Assert.AreEqual("AAA|BBB|CCC", resprod.ToString(), "Key changed #2");

      keyArg.Value = 2.0f;
      Assert.AreEqual("AAA|BBB|CCC|ZZZ", resprod.ToString(), "Key changed #3");

      keyArg.Value = 4.0f;
      Assert.AreEqual("AAA|BBB|CCC|ZZZ", resprod.ToString(), "Key changed #4");
    }
  }
}
