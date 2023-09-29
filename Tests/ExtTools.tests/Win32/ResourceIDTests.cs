using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Win32;

namespace ExtTools_tests.Win32
{
  [TestFixture]
  public class ResourceIDTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_ID()
    {
      ResourceID sut = new ResourceID(123);
      Assert.IsTrue(sut.IsID, "IsId");
      Assert.IsFalse(sut.IsName, "IsName");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(123, sut.ID, "ID");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Name), "Name");
    }

    [Test]
    public void Constructor_Name()
    {
      ResourceID sut = new ResourceID("AbC");
      Assert.IsFalse(sut.IsID, "IsId");
      Assert.IsTrue(sut.IsName, "IsName");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(0, sut.ID, "ID");
      Assert.AreEqual("AbC", sut.Name, "Name");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      ResourceID sut = ResourceID.Empty;
      Assert.IsFalse(sut.IsID, "IsId");
      Assert.IsFalse(sut.IsName, "IsName");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(0, sut.ID, "ID");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Name), "Name");
    }

    #endregion

    #region Сравнение

    [TestCase("", "", 0)]
    [TestCase("", "#1", -1)]
    [TestCase("", "ABC", -1)]
    [TestCase("ABC", "#1", -1)]
    [TestCase("1", "#1", -1)]
    [TestCase("#1", "#2", -1)]
    [TestCase("#1", "#1", 0)]
    [TestCase("ABC", "ABD", -1)]
    [TestCase("ABC", "abc", 0)]
    public void CompareTo(string s1, string s2, int wantedRes)
    {
      ResourceID a = CreateObject(s1);
      ResourceID b = CreateObject(s2);

      DoCompareTo(a, b, wantedRes, " - #1");
      DoCompareTo(b, a, -wantedRes, " - #2");
    }

    private static void DoCompareTo(ResourceID a, ResourceID b, int wantedRes, string suffix)
    {
      int res1 = a.CompareTo(b);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "CompareTo()" + suffix);

      bool res2 = a.Equals(b);
      Assert.AreEqual(wantedRes == 0, res2, "Equals(ResourceID)" + suffix);

      bool res3 = a.Equals((object)b);
      Assert.AreEqual(wantedRes == 0, res3, "Equals(Object)" + suffix);

      bool res4 = (a == b);
      Assert.AreEqual(wantedRes == 0, res4, "==" + suffix);

      bool res5 = (a != b);
      Assert.AreEqual(wantedRes != 0, res5, "!=" + suffix);

      if (wantedRes == 0)
      {
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "GetHashCode()" + suffix);
      }
    }

    private static ResourceID CreateObject(string s)
    {
      if (String.IsNullOrEmpty(s))
        return ResourceID.Empty;
      if (s[0] == '#')
        return new ResourceID(int.Parse(s.Substring(1)));
      else
        return new ResourceID(s);
    }

    #endregion
  }
}
