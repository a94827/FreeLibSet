using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UIValidateResultTests
  {
    #region Конструктор

    [Test]
    public void Constructor_valid()
    {
      UIValidateResult sut = new UIValidateResult(true, "");
      Assert.IsTrue(sut.IsValid, "IsValid");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Message), "Message");
    }

    [Test]
    public void Constructor_notValid()
    {
      UIValidateResult sut = new UIValidateResult(false, "Test");
      Assert.IsFalse(sut.IsValid, "IsValid");
      Assert.AreEqual("Test", sut.Message, "Message");
    }

    #endregion

    #region Сравнение

    [TestCase("", "", true)]
    [TestCase("Test1", "", false)]
    [TestCase("Test1", "Test2", false)]
    [TestCase("Test1", "Test1", true)]
    public void Equals(string s1, string s2, bool wantedRes)
    {
      DoEquals(s1, s2, wantedRes, "#1");
      DoEquals(s2, s1, wantedRes, "#2");
    }

    private static void DoEquals(string s1, string s2, bool wantedRes, string suffix)
    {
      UIValidateResult v1 = s1.Length == 0 ? UIValidateResult.Ok : new UIValidateResult(false, s1);
      UIValidateResult v2 = s2.Length == 0 ? UIValidateResult.Ok : new UIValidateResult(false, s2);

      Assert.AreEqual(wantedRes, v1 == v2, suffix + " ==");
      Assert.AreEqual(!wantedRes, v1 != v2, suffix + " !=");
      Assert.AreEqual(wantedRes, v1.Equals(v2), suffix + " Equals(UIValidateResult)");
      Assert.AreEqual(wantedRes, v1.Equals((object)v2), suffix + " Equals(Object)");
    }

    #endregion

    #region Прочее

    [Test]
    public static void Ok()
    {
      Assert.IsTrue(UIValidateResult.Ok.IsValid, "IsValid");
    }

    #endregion
  }
}
