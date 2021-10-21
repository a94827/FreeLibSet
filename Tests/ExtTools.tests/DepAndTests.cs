using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

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

    [Test]
    public void AttachInput_with_bare()
    {
      DepInput<bool> sut = new DepInput<bool>();
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("False", resprod.ToString(), "Original");

      DepInput<bool> att = new DepInput<bool>();
      att.Value = false;

      DepAnd.AttachInput(sut, att);
      Assert.AreEqual("False", resprod.ToString(), "Attached");

      att.Value = true;
      Assert.AreEqual("False|True", resprod.ToString(), "Attached value changed");
    }

    [Test]
    public void AttachInput_with_input()
    {
      DepInput<bool> sut = new DepInput<bool>();
      DepInput<bool> main = new DepInput<bool>();
      main.Value = true;
      sut.Source = main;
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("True", resprod.ToString(), "Original");

      DepInput<bool> att = new DepInput<bool>();
      att.Value = false;

      DepAnd.AttachInput(sut, att);
      Assert.AreEqual("True|False", resprod.ToString(), "Attached");

      att.Value = true;
      Assert.AreEqual("True|False|True", resprod.ToString(), "Attached value changed");

      main.Value = false;
      Assert.AreEqual("True|False|True|False", resprod.ToString(), "Main value changed");
    }
  }
}
