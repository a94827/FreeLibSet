using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UISimpleValidableObjectTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      UISimpleValidableObject sut = new UISimpleValidableObject();
      Assert.AreEqual(UIValidateState.Ok, sut.ValidateState, "ValidateState");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Message), "Message");
    }

    [Test]
    public void Clear()
    {
      UISimpleValidableObject sut = new UISimpleValidableObject();
      sut.SetError("Test");

      sut.Clear();

      Assert.AreEqual(UIValidateState.Ok, sut.ValidateState, "ValidateState");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Message), "Message");
    }

    [Test]
    public void SetError()
    {
      UISimpleValidableObject sut = new UISimpleValidableObject();

      sut.SetError("Test1");
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #1");
      Assert.AreEqual("Test1", sut.Message, "Message #1");

      sut.SetError("Test2"); // nothing has been changed
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #2");
      Assert.AreEqual("Test1", sut.Message, "Message #2");

      sut.SetWarning("Test3");
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #3");
      Assert.AreEqual("Test1", sut.Message, "Message #3");

      sut.Clear();
      sut.SetWarning("Test4");
      Assert.AreEqual(UIValidateState.Warning, sut.ValidateState, "ValidateState #4");
      Assert.AreEqual("Test4", sut.Message, "Message #4");

      sut.SetError("Test5");
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #5");
      Assert.AreEqual("Test5", sut.Message, "Message #5");
    }

    [Test]
    public void SetWarning()
    {
      UISimpleValidableObject sut = new UISimpleValidableObject();

      sut.SetWarning("Test1");
      Assert.AreEqual(UIValidateState.Warning, sut.ValidateState, "ValidateState #1");
      Assert.AreEqual("Test1", sut.Message, "Message #1");

      sut.SetError("Test2");
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #2");
      Assert.AreEqual("Test2", sut.Message, "Message #2");

      sut.SetWarning("Test3"); // not changed
      Assert.AreEqual(UIValidateState.Error, sut.ValidateState, "ValidateState #3");
      Assert.AreEqual("Test2", sut.Message, "Message #3");
    }

    #endregion
  }
  }
