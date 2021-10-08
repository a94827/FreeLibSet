using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepEqualTests
  {
    [TestCase(1, 2, false)]
    [TestCase(2, 2, true)]
    public void ConstTest(int v1, int v2, bool wanted)
    {
      DepEqual<int> sut = new DepEqual<int>(new DepConst<int>(v1), v2);
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<int> v1 = new DepInput<int>();
      v1.Value = 1;
      DepInput<int> v2 = new DepInput<int>();
      v2.Value = 2;

      DepEqual<int> sut = new DepEqual<int>(v1, v2);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      v1.Value = 2;
      Assert.AreEqual("False|True", resprod.ToString(), "First value changed");

      v2.Value = 3;
      Assert.AreEqual("False|True|False", resprod.ToString(), "Second value changed");
    }
  }
}
