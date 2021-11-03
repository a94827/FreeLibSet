using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepNotTests
  {
    [TestCase(false, true)]
    [TestCase(true, false)]
    public void ConstTest(bool v, bool wanted)
    {
      DepNot sut = new DepNot(new DepConst<bool>(v));
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<bool> v = new DepInput<bool>(false, null);

      DepNot sut = new DepNot(v);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("True", resprod.ToString(), "Original");

      v.Value = true;
      Assert.AreEqual("True|False", resprod.ToString(), "Changed");
    }
  }
}
