using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepAndTests
  {
    [TestCase(false, false, false, false)]
    [TestCase(false, true, false, false)]
    [TestCase(true, false, true, false)]
    [TestCase(true, true, true, true)]
    public void ConstTest(bool v1, bool v2, bool v3, bool wanted)
    {
      DepAnd sut = new DepAnd(new DepConst<bool>(v1), new DepConst<bool>(v2), new DepConst<bool>(v3));
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<bool> v1 = new DepInput<bool>();
      v1.Value = false;
      DepInput<bool> v2 = new DepInput<bool>();
      v2.Value = false;

      DepAnd sut = new DepAnd(v1, v2);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      v1.Value = true;
      Assert.AreEqual("False", resprod.ToString(), "Change #1");

      v2.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Change #2");
    }
  }
}
