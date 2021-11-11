using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  class DepOutputTests
  {
    [Test]
    public void Constructor()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      Assert.AreEqual(1, sut.Value, "Value");
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
      Assert.IsFalse(sut.IsConst, "IsConst");
    }

    [Test]
    public void ValueChanged_added()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      sut.ValueChanged += new EventHandler(Dummy_ValueChanged);
      Assert.AreEqual(1, sut.Value, "Value");
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsTrue(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsTrue(sut.IsConnected, "IsConnected");
      Assert.IsFalse(sut.IsConst, "IsConst");
    }

    static void Dummy_ValueChanged(object sender, EventArgs args)
    {
    }

    [Test]
    public void Output_added()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      DepInput<int> dummy = new DepInput<int>(0, Dummy_ValueChanged);
      dummy.Source = sut;

      Assert.AreEqual(1, sut.Value, "Value"); // не должна испортиться
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsTrue(sut.HasOutputs, "HasOutputs");
      Assert.IsTrue(sut.IsConnected, "IsConnected");
      Assert.IsFalse(sut.IsConst, "IsConst");
    }

    [Test]
    public void Output_removed()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      DepInput<int> dummy = new DepInput<int>(0, Dummy_ValueChanged);
      dummy.Source = sut;
      // Теперь отключаем
      dummy.Source = null;

      Assert.AreEqual(1, sut.Value, "Value"); // не должна испортиться
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
    }

    [Test]
    public void OwnerSetValue()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      sut.OwnerSetValue(2);
      Assert.AreEqual("1|2", resprod.ToString(), "Value changed");
      sut.OwnerSetValue(2);
      Assert.AreEqual("1|2", resprod.ToString(), "Value does not changed");
      sut.OwnerSetValue(1);
      Assert.AreEqual("1|2|1", resprod.ToString(), "Value changed again");
    }

    #region Событие ValueChanged

    [Test]
    public void ValueChanged()
    {
      DepOutput<int> sut = new DepOutput<int>(1);
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual(1, sut.Value, "Original value");

      sut.OwnerSetValue(2);
      Assert.AreEqual(2, sut.Value, "Changed value");
      Assert.AreEqual("1|2", resprod.ToString(), "ValueChangedCalled");

      sut.OwnerSetValue(2);
      Assert.AreEqual("1|2", resprod.ToString(), "Not changed");
    }

    #endregion
  }
}
