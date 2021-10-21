using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepInArrayTests
  {
    [Test]
    public void DynamicTest()
    {
      DepInput<int> v = new DepInput<int>();
      v.Value = 1;
      DepInArray<int> sut = new DepInArray<int>(v, new int[] { 1,2,3});
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("True", resprod.ToString(), "Original");

      v.Value = 2;
      Assert.AreEqual("True", resprod.ToString(), "First change");

      v.Value = 4;
      Assert.AreEqual("True|False", resprod.ToString(), "Second change");
    }
  }
}
