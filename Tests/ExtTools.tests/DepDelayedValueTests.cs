using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools.tests
{
  [TestFixture]
  public class DepDelayedValueTests
  {
    #region Класс для обработки события

    private class ValueNeededClass
    {
      public string Value;

      public int CallCount;

      public void ValueNeeded(object sender, DepValueNeededEventArgs<string> args)
      {
        args.Value = Value;
        CallCount++;
      }
    }

    #endregion

    [Test]
    public void Constructor()
    {
      ValueNeededClass vn = new ValueNeededClass();
      vn.Value = "1";
      DepDelayedValue<string> sut = new DepDelayedValue<string>(vn.ValueNeeded);
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
      Assert.IsFalse(sut.IsConst, "IsConst");
    }

    [Test]
    public void ValueNeeded()
    {
      ValueNeededClass vn = new ValueNeededClass();
      vn.Value = "1";
      DepDelayedValue<string> sut = new DepDelayedValue<string>(vn.ValueNeeded);

      string res1 = sut.Value;
      Assert.AreEqual("1", res1, "Result #1");
      Assert.AreEqual(1, vn.CallCount, "Call count #1");

      vn.Value = "2"; // не повлияет
      string res2 = sut.Value;
      Assert.AreEqual("1", res2, "Result #2");
      Assert.AreEqual(1, vn.CallCount, "Call count #2");
    }

    [Test]
    public void SetDelayed()
    {
      ValueNeededClass vn = new ValueNeededClass();
      vn.Value = "1";
      DepDelayedValue<string> sut = new DepDelayedValue<string>(vn.ValueNeeded);

      string res1 = sut.Value;
      Assert.AreEqual("1", res1, "Result #1");
      Assert.AreEqual(1, vn.CallCount, "Call count #1");

      vn.Value = "2";
      sut.SetDelayed();

      string res2 = sut.Value;
      Assert.AreEqual("2", res2, "Result #2");
      Assert.AreEqual(2, vn.CallCount, "Call count #2");
    }
  }
}
