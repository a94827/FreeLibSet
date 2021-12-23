using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DummyNamedValuesTests
  {
    [Test]
    public void Constructor()
    {
      DummyNamedValues sut = new DummyNamedValues();

      Assert.IsFalse(sut.Contains("ABC"), "Contains");
      Assert.AreEqual(0, sut.GetNames().Length, "GetNames()");
    }
  }
}
