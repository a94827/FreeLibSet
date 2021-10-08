using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepIfTests
  {
    [TestCase(true, 1, 2, 1)]
    [TestCase(false, 1, 2, 2)]
    public void ConstTest(bool cond, int trueVal, int falseVal, int wanted)
    {
      DepIf<int> sut = new DepIf<int>(new DepConst<bool>(cond), new DepConst<int>(trueVal), new DepConst<int>(falseVal));
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<bool> vCond = new DepInput<bool>();
      vCond.Value = true;
      DepInput<int> vTrue = new DepInput<int>();
      vTrue.Value = 1;
      DepInput<int> vFalse = new DepInput<int>();
      vFalse.Value = 2;

      DepIf<int> sut = new DepIf<int>(vCond, vTrue, vFalse);
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      vCond.Value = false;
      Assert.AreEqual("1|2", resprod.ToString(), "Condition changed first time");

      vTrue.Value = 3;
      Assert.AreEqual("1|2", resprod.ToString(), "True value changed");

      vFalse.Value = 4;
      Assert.AreEqual("1|2|4", resprod.ToString(), "False value changed");

      vCond.Value = true;
      Assert.AreEqual("1|2|4|3", resprod.ToString(), "Condition changed again");
    }
  }
}
