using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  public class DepInRangeTests
  {
    [TestCase(0, 1, 3, false)]
    [TestCase(1, 1, 3, true)]
    [TestCase(2, 1, 3, true)]
    [TestCase(3, 1, 3, true)]
    [TestCase(4, 1, 3, false)]
    public void ConstTest(int value, int minimum, int maximum, bool wanted)
    {
      DepInRange<int> sut = new DepInRange<int>(new DepConst<int>(value), new DepConst<int>(minimum), new DepConst<int>(maximum));
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<int> vVal = new DepInput<int>(5, null);
      DepInput<int> vMin = new DepInput<int>(4, null);
      DepInput<int> vMax = new DepInput<int>(6, null);

      DepInRange<int> sut = new DepInRange<int>(vVal, vMin,vMax);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("True", resprod.ToString(), "Original");

      vVal.Value = 3;
      Assert.AreEqual("True|False", resprod.ToString(), "Tested value changed");

      vMin.Value = 1;
      Assert.AreEqual("True|False|True", resprod.ToString(), "Minimum changed");

      vMax.Value = 2;
      Assert.AreEqual("True|False|True|False", resprod.ToString(), "Maximum changed");
    }

    [Test]
    public void Comparer()
    {
      DepInRange<int> sut = new DepInRange<int>(new DepConst<int>(2), new DepConst<int>(3), new DepConst<int>(1), new InvertedTestComparer());
      Assert.AreEqual(true, sut.Value, "Inverted comparer");
    }
  }
}
