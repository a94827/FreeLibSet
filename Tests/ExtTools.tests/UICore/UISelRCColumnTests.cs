using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System.Globalization;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UISelRCColumnTests
  {
    #region  онструктор

    [Test]
    public void Constructor_1()
    {
      UISelRCColumn sut = new UISelRCColumn("Col1");
      Assert.AreEqual("Col1", sut.Code, "Code");
      Assert.IsTrue(sut.CanBeEmpty, "CanBeEmpty");
      Assert.AreEqual("Col1", sut.DisplayName, "DisplayName");
    }

    [Test]
    public void Constructor_2()
    {
      UISelRCColumn sut = new UISelRCColumn("Col2", "Text2");
      Assert.AreEqual("Col2", sut.Code, "Code");
      Assert.IsTrue(sut.CanBeEmpty, "CanBeEmpty");
      Assert.AreEqual("Text2", sut.DisplayName, "DisplayName");
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch<ArgumentException>(delegate() { new UISelRCColumn(""); });
    }

    #endregion

    #region “естовые данные

    private static UISelRCGridData CreateTestData(string textValue, UISelRCColumn sut)
    {
      string[,] sourceData = new string[1, 1] { { textValue } };
      UISelRCColumn[] availableColumns = new UISelRCColumn[1] { sut };

      UISelRCGridData data = new UISelRCGridData(sourceData, availableColumns);
      // помечаем строку и столбец как выбранные
      data.SelRows[0] = true;
      data.SelColumns[0] = sut;
      return data;
    }

    #endregion

    #region CanBeEmpty

    [TestCase("AAA", true, UIValidateState.Ok)]
    [TestCase("", true, UIValidateState.Ok)]
    [TestCase("AAA", false, UIValidateState.Ok)]
    [TestCase("", false, UIValidateState.Error)]
    public void CanBeEmpty(string textValue, bool canBeEmpty, UIValidateState wantedRes)
    {
      UISelRCColumn sut = new UISelRCColumn("Col1");
      sut.CanBeEmpty = canBeEmpty;
      Assert.AreEqual(canBeEmpty, sut.CanBeEmpty, "CanBeEmpty");

      UISelRCGridData data = CreateTestData(textValue, sut);

      string errorText;
      UIValidateState res = data.Validate(0, 0, out errorText);
      Assert.AreEqual(wantedRes, res, "Result");
    }

    #endregion

    #region Validating

    private class ValidatingTester
    {
      public UIValidateState ValidateState;
      public string ErrorText;
      public object ResultValue;

      public void Validating(object sender, UISelRCValidatingEventArgs args)
      {
        switch (ValidateState)
        {
          case UIValidateState.Error:
            args.SetError(ErrorText);
            Assert.AreEqual(ErrorText, args.ErrorText, "ErrorText on Validating event handler");
            break;

          case UIValidateState.Warning:
            args.SetWarning(ErrorText);
            Assert.AreEqual(ErrorText, args.ErrorText, "ErrorText on Validating event handler");
            break;
        }

        Assert.AreEqual(ValidateState, args.ValidateState, "ValidateState on Validating event handler");

        args.ResultValue = ResultValue;
      }

      public void DoTest(UISelRCGridData data, string info)
      {
        string errorText;
        UIValidateState res = data.Validate(0, 0, out errorText);
        Assert.AreEqual(this.ValidateState, res, info + " - ValidateState");
        if (res == UIValidateState.Ok)
          Assert.IsTrue(String.IsNullOrEmpty(errorText), info + "- empty errorText");
        else
          Assert.AreEqual(this.ErrorText, errorText, info + " - errorText");

        object v = data[0, 0];
        Assert.AreEqual(this.ResultValue, v, info + " - result value");
      }
    }

    [TestCase(UIValidateState.Ok, null, 123)]
    [TestCase(UIValidateState.Warning, "test message 1", 456)]
    [TestCase(UIValidateState.Error, "test message 2", 789)]
    public void Validating(UIValidateState validateState, string errorText, object resultValue)
    {
      ValidatingTester tester = new ValidatingTester();
      tester.ValidateState = validateState;
      tester.ErrorText = errorText;
      tester.ResultValue = resultValue;

      // присоединение обработчика после конструктора
      UISelRCColumn sut1 = new UISelRCColumn("Col1");
      sut1.Validating += tester.Validating;
      UISelRCGridData data1 = CreateTestData("Dummy", sut1);
      tester.DoTest(data1, "#1");

      // задание обработчика в конструкторе
      UISelRCColumn sut2 = new UISelRCColumn("Col2", "Col2", tester.Validating);
      UISelRCGridData data2 = CreateTestData("Dummy", sut2);
      tester.DoTest(data2, "#2");
    }

    private class ValidatingCounter
    {
      public int Count;

      public void Validating(object sender, UISelRCValidatingEventArgs args)
      {
        Count++;
      }
    }

    [TestCase(true, 1)]
    [TestCase(false, 0)]
    public void Validating_and_CanBeEmpty(bool canBeEmpty, int wantedCount)
    {
      ValidatingCounter counter=new ValidatingCounter();

      UISelRCColumn sut = new UISelRCColumn("Col1", "Col1", counter.Validating);
      sut.CanBeEmpty = canBeEmpty;
      UISelRCGridData data = CreateTestData("", sut); // пустые данные
      object value1 = data[0, 0];
      Assert.AreEqual(wantedCount, counter.Count);
    }

    #endregion
  }
}
