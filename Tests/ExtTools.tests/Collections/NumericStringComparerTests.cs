using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using NUnit.Framework;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class NumericStringComparerTests
  {
    #region Статические свойства

    [TestCase("10", "9", +1)]
    [TestCase("A10", "A9", +1)]
    [TestCase("192.168.0.10", "192.168.0.9", +1)]
    [TestCase("192.168.0.10", "192.168.0.10", 0)]
    [TestCase("A10", "a10", -1)]
    [TestCase("123A", "123B", -1)]
    [TestCase("0", "00", -1)]
    [TestCase("A0", "A00", -1)]
    public void Ordinal(string a, string b, int wantedRes)
    {
      int res1 = NumericStringComparer.Ordinal.Compare(a, b);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "#1");

      int res2 = NumericStringComparer.Ordinal.Compare(b, a);
      Assert.AreEqual(-wantedRes, Math.Sign(res2), "#2");
    }

    [TestCase("10", "9", +1)]
    [TestCase("A10", "A9", +1)]
    [TestCase("A10", "a10", 0)]
    public void OrdinalIgnoreCase(string a, string b, int wantedRes)
    {
      int res1 = NumericStringComparer.OrdinalIgnoreCase.Compare(a, b);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "#1");

      int res2 = NumericStringComparer.OrdinalIgnoreCase.Compare(b, a);
      Assert.AreEqual(-wantedRes, Math.Sign(res2), "#2");
    }

    #endregion

    #region Пользовательский компаратор

    private class TestStringComparer : IComparer<string>
    {
      public int Compare(string x, string y)
      {
        return -String.Compare(x, y, StringComparison.Ordinal);
      }
    }

    [TestCase("10", "9", +1)]
    [TestCase("A10", "A9", +1)]
    [TestCase("A0", "A00", +1)]
    [TestCase("A10", "B10", +1)]
    public void UserComparator(string a, string b, int wantedRes)
    {
      NumericStringComparer sut = new NumericStringComparer(new TestStringComparer());
      int res1 = sut.Compare(a, b);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "#1");

      int res2 = sut.Compare(b, a);
      Assert.AreEqual(-wantedRes, Math.Sign(res2), "#2");
    }

    #endregion
  }
}
