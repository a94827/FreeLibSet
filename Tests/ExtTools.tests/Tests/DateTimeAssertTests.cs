using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Tests;

namespace ExtTools_tests.Tests
{
  [TestFixture]
  public class DateTimeAssertTests
  {
    #region Assert

    [TestCase("20230807123456", "20230807123457", "0:0:1")]
    [TestCase("20230807123457", "20230807123456", "0:0:1")]
    public void Assert_DateTime_ok(string sExpected, string sActual, string sDelta)
    {
      DateTime expected = Creators.DateTime(sExpected);
      DateTime actual = Creators.DateTime(sActual);
      TimeSpan delta = TimeSpan.Parse(sDelta);

      Assert.DoesNotThrow(delegate () { DateTimeAssert.AreEqual(expected, actual, delta); });
    }

    [TestCase("20230807123456", "20230807123458", "0:0:1")]
    [TestCase("20230807123458", "20230807123456", "0:0:1")]
    public void Assert_DateTime_diff(string sExpected, string sActual, string sDelta)
    {
      DateTime expected = Creators.DateTime(sExpected);
      DateTime actual = Creators.DateTime(sActual);
      TimeSpan delta = TimeSpan.Parse(sDelta);

      Assert.Catch<AssertionException>(delegate () { DateTimeAssert.AreEqual(expected, actual, delta); });
    }

    [TestCase("20230807123456", "20230807123457", "0:0:1")]
    [TestCase("20230807123457", "20230807123456", "0:0:1")]
    [TestCase("", "", "0:0:1")]
    public void Assert_NullableDateTime_ok(string sExpected, string sActual, string sDelta)
    {
      DateTime? expected = Creators.NDateTime(sExpected);
      DateTime? actual = Creators.NDateTime(sActual);
      TimeSpan delta = TimeSpan.Parse(sDelta);

      Assert.DoesNotThrow(delegate () { DateTimeAssert.AreEqual(expected, actual, delta); });
    }

    [TestCase("20230807123456", "20230807123458", "0:0:1")]
    [TestCase("20230807123458", "20230807123456", "0:0:1")]
    [TestCase("20230807123456", "", "0:0:1")]
    [TestCase("", "20230807123457", "0:0:1")]
    public void Assert_NullableDateTime_diff(string sExpected, string sActual, string sDelta)
    {
      DateTime? expected = Creators.NDateTime(sExpected);
      DateTime? actual = Creators.NDateTime(sActual);
      TimeSpan delta = TimeSpan.Parse(sDelta);

      Assert.Catch<AssertionException>(delegate () { DateTimeAssert.AreEqual(expected, actual, delta); });
    }

    #endregion
  }
}
