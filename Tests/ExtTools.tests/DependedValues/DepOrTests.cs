using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
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
      DepInput<bool> v1 = new DepInput<bool>(false, null);
      DepInput<bool> v2 = new DepInput<bool>(false, null);

      DepOr sut = new DepOr(v1, v2);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      v1.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Change #1");

      v2.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Change #2");
    }

    [Test]
    public void AttachInput_with_bare()
    {
      DepInput<bool> sut = new DepInput<bool>(false,null);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      DepInput<bool> att = new DepInput<bool>(false, null);

      DepOr.AttachInput(sut, att);
      Assert.AreEqual("False", resprod.ToString(), "Attached");

      att.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Attached value changed");
    }

    [Test]
    public void AttachInput_with_input()
    {
      DepInput<bool> sut = new DepInput<bool>(false, null);
      DepInput<bool> main = new DepInput<bool>(false, null);
      sut.Source = main;
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      DepInput<bool> att = new DepInput<bool>(true, null);

      DepOr.AttachInput(sut, att);
      Assert.AreEqual("False|True", resprod.ToString(), "Attached");

      att.Value = false;
      Assert.AreEqual("False|True|False", resprod.ToString(), "Attached value changed");

      main.Value = true;
      Assert.AreEqual("False|True|False|True", resprod.ToString(), "Main value changed");
    }
  }
}
