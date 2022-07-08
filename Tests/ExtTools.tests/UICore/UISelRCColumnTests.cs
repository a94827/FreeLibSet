using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System.Globalization;
using FreeLibSet.Collections;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UISelRCColumnTests
  {
    #region Конструктор

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

    #region Тестовые данные

    public static UISelRCGridData CreateTestData(string textValue, UISelRCColumn sut)
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
      ValidatingCounter counter = new ValidatingCounter();

      UISelRCColumn sut = new UISelRCColumn("Col1", "Col1", counter.Validating);
      sut.CanBeEmpty = canBeEmpty;
      UISelRCGridData data = CreateTestData("", sut); // пустые данные
      object value1 = data[0, 0];
      Assert.AreEqual(wantedCount, counter.Count);
    }

    #endregion
  }

  public class UISelRCColumn_InheritorTests
  {
    #region Столбцы ввода простых значений

    [Test]
    public void UISelRCIntColumn_main()
    {
      DoTest(new UISelRCIntColumn("Col1"), 123, 123.45.ToString(), 0);
    }

    [Test]
    public void UISelRCSingleColumn_main()
    {
      DoTest(new UISelRCSingleColumn("Col1"), 123.45f, "ABC", 0f);
    }

    [Test]
    public void UISelRCDoubleColumn_main()
    {
      DoTest(new UISelRCDoubleColumn("Col1"), 123.45, "ABC", 0.0);
    }

    [Test]
    public void UISelRCDecimalColumn_main()
    {
      DoTest(new UISelRCDecimalColumn("Col1"), 123.45m, "ABC", 0m);
    }

    [Test]
    public void UISelRCDateColumn_main()
    {
      DoTest(new UISelRCDateColumn("Col1"), DateTime.Today, "ABC", null);
    }

    private void DoTest(UISelRCColumn col, object testValue, string badText, object defValue)
    {
      string errorText;
      UISelRCGridData data1 = UISelRCColumnTests.CreateTestData(testValue.ToString(), col);
      Assert.AreEqual(testValue, data1[0, 0], "Good value #1");
      Assert.AreEqual(UIValidateState.Ok, data1.Validate(0, 0, out errorText), "Validate() #1");

      UISelRCGridData data2 = UISelRCColumnTests.CreateTestData(badText, col);
      // Ошибочное значение не определено
      //Assert.AreEqual(defValue, data2[0, 0], "Default value #2");
      Assert.AreEqual(UIValidateState.Error, data2.Validate(0, 0, out errorText), "Validate() #2");

      col.CanBeEmpty = true;
      UISelRCGridData data3 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(defValue, data3[0, 0], "Default value #3");
      Assert.AreEqual(UIValidateState.Ok, data3.Validate(0, 0, out errorText), "Validate() #3");

      col.CanBeEmpty = false;
      UISelRCGridData data4 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(defValue, data4[0, 0], "Default value #4");
      Assert.AreEqual(UIValidateState.Error, data4.Validate(0, 0, out errorText), "Validate() #4");
    }

    #endregion

    #region Столбцы перечислений

    private enum SimpleEnum
    { 
      Zero, One, Two, Three
    }

    [Test]
    public void UISelRCEnumColumn_main()
    {
      UISelRCEnumColumn<SimpleEnum> col = new UISelRCEnumColumn<SimpleEnum>("Col1", "Test", new string[]{
        "Value 0",
        "Value 1",
        "Value 2",
        "Value 3"});

      string errorText;
      UISelRCGridData data1 = UISelRCColumnTests.CreateTestData("Value 2", col);
      Assert.AreEqual(SimpleEnum.Two, data1[0, 0], "Good value #1");
      Assert.AreEqual(UIValidateState.Ok, data1.Validate(0, 0, out errorText), "Validate() #1");


      UISelRCGridData data2 = UISelRCColumnTests.CreateTestData("Two", col);
      Assert.AreEqual(UIValidateState.Error, data2.Validate(0, 0, out errorText), "Validate() #2");

      col.CanBeEmpty = true;
      UISelRCGridData data3 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(null, data3[0, 0], "Default value #3");
      Assert.AreEqual(UIValidateState.Ok, data3.Validate(0, 0, out errorText), "Validate() #3");

      col.CanBeEmpty = false;
      UISelRCGridData data4 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(null, data4[0, 0], "Default value #4");
      Assert.AreEqual(UIValidateState.Error, data4.Validate(0, 0, out errorText), "Validate() #4");
    }

    [Test]
    public void UISelRCEnumColumnWithDict_main()
    {
      TypedStringDictionary<SimpleEnum> dict = new TypedStringDictionary<SimpleEnum>(true);
      dict.Add("Value 1", SimpleEnum.One);
      dict.Add("Value 3", SimpleEnum.Three);
      dict.Add("XXX 3", SimpleEnum.Three); // повтор

      UISelRCEnumColumnWithDict<SimpleEnum> col = new UISelRCEnumColumnWithDict<SimpleEnum>("Col1", "Test", dict);

      string errorText;
      UISelRCGridData data1 = UISelRCColumnTests.CreateTestData("XXX 3", col);
      Assert.AreEqual(SimpleEnum.Three, data1[0, 0], "Good value #1");
      Assert.AreEqual(UIValidateState.Ok, data1.Validate(0, 0, out errorText), "Validate() #1");


      UISelRCGridData data2 = UISelRCColumnTests.CreateTestData("Value 2", col);
      Assert.AreEqual(UIValidateState.Error, data2.Validate(0, 0, out errorText), "Validate() #2");

      col.CanBeEmpty = true;
      UISelRCGridData data3 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(null, data3[0, 0], "Default value #3");
      Assert.AreEqual(UIValidateState.Ok, data3.Validate(0, 0, out errorText), "Validate() #3");

      col.CanBeEmpty = false;
      UISelRCGridData data4 = UISelRCColumnTests.CreateTestData(String.Empty, col);
      Assert.AreEqual(null, data4[0, 0], "Default value #4");
      Assert.AreEqual(UIValidateState.Error, data4.Validate(0, 0, out errorText), "Validate() #4");
    }

    #endregion
  }
}
