using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepConstTests
  {
    [Test]
    public void Constructor()
    {
      DepConst<int> sut = new DepConst<int>(1);
      Assert.AreEqual(1, sut.Value, "Value");
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
      Assert.IsTrue(sut.IsConst, "IsConst");
    }

    [Test]
    public void CreateArray()
    {
      DepConst<string>[] a = DepConst<string>.CreateArray(new string[] { "1", "2", "3" });
      Assert.AreEqual(3, a.Length, "Length");
      Assert.AreEqual("1", a[0].Value, "[0]");
      Assert.AreEqual("2", a[1].Value, "[1]");
      Assert.AreEqual("3", a[2].Value, "[2]");
    }
  }
}
