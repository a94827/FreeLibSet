using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepOrTests
  {
    [TestCase(false, false, false, false)]
    [TestCase(false, true, false, true)]
    [TestCase(true, false, true, true)]
    [TestCase(true, true, true, true)]
    public void ConstTest(bool v1, bool v2, bool v3, bool wanted)
    {
      DepOr sut = new DepOr(new DepConst<bool>(v1), new DepConst<bool>(v2), new DepConst<bool>(v3));
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<bool> v1 = new DepInput<bool>();
      v1.Value = false;
      DepInput<bool> v2 = new DepInput<bool>();
      v2.Value = false;

      DepOr sut = new DepOr(v1, v2);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      v1.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Change #1");

      v2.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Change #2");
    }
  }
}
