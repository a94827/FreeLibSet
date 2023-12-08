using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UIValidatorListTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      UIValidatorList sut = new UIValidatorList();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region AddError()

    [Test]
    public void AddError_resultEx_preconditionEx()
    {
      DepValue<UIValidateResult> resultEx = new DepConst<UIValidateResult>(UIValidateResult.Ok);
      DepValue<bool> preconditionEx = new DepConst<bool>(true);

      UIValidatorList sut = new UIValidatorList();
      sut.AddError(resultEx, preconditionEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsTrue(sut[0].IsError, "IsError");
      Assert.AreSame(resultEx, sut[0].ResultEx, "ResultEx");
      Assert.AreSame(preconditionEx, sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddError_resultEx()
    {
      DepValue<UIValidateResult> resultEx = new DepConst<UIValidateResult>(UIValidateResult.Ok);

      UIValidatorList sut = new UIValidatorList();
      sut.AddError(resultEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsTrue(sut[0].IsError, "IsError");
      Assert.AreSame(resultEx, sut[0].ResultEx, "ResultEx");
      Assert.IsNull(sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddError_expressionEx_message_preconditionEx()
    {
      DepValue<bool> expressionEx = new DepConst<bool>(false);
      string message = "Hello";
      DepValue<bool> preconditionEx = new DepConst<bool>(true);

      UIValidatorList sut = new UIValidatorList();
      sut.AddError(expressionEx, message, preconditionEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsTrue(sut[0].IsError, "IsError");
      UIValidateResult res = sut[0].ResultEx.Value;
      Assert.AreEqual(message, res.Message, "ResultEx.Message");
      Assert.AreSame(preconditionEx, sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddError_expressionEx_message()
    {
      DepValue<bool> expressionEx = new DepConst<bool>(false);
      string message = "Hello";

      UIValidatorList sut = new UIValidatorList();
      sut.AddError(expressionEx, message);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsTrue(sut[0].IsError, "IsError");
      UIValidateResult res = sut[0].ResultEx.Value;
      Assert.AreEqual(message, res.Message, "ResultEx.Message");
      Assert.IsNull(sut[0].PreconditionEx, "PreconditionEx");
    }

    #endregion

    #region AddWarning()

    [Test]
    public void AddWarning_resultEx_preconditionEx()
    {
      DepValue<UIValidateResult> resultEx = new DepConst<UIValidateResult>(UIValidateResult.Ok);
      DepValue<bool> preconditionEx = new DepConst<bool>(true);

      UIValidatorList sut = new UIValidatorList();
      sut.AddWarning(resultEx, preconditionEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsFalse(sut[0].IsError, "IsError");
      Assert.AreSame(resultEx, sut[0].ResultEx, "ResultEx");
      Assert.AreSame(preconditionEx, sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddWarning_resultEx()
    {
      DepValue<UIValidateResult> resultEx = new DepConst<UIValidateResult>(UIValidateResult.Ok);

      UIValidatorList sut = new UIValidatorList();
      sut.AddWarning(resultEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsFalse(sut[0].IsError, "IsError");
      Assert.AreSame(resultEx, sut[0].ResultEx, "ResultEx");
      Assert.IsNull(sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddWarning_expressionEx_message_preconditionEx()
    {
      DepValue<bool> expressionEx = new DepConst<bool>(false);
      string message = "Hello";
      DepValue<bool> preconditionEx = new DepConst<bool>(true);

      UIValidatorList sut = new UIValidatorList();
      sut.AddWarning(expressionEx, message, preconditionEx);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsFalse(sut[0].IsError, "IsError");
      UIValidateResult res = sut[0].ResultEx.Value;
      Assert.AreEqual(message, res.Message, "ResultEx.Message");
      Assert.AreSame(preconditionEx, sut[0].PreconditionEx, "PreconditionEx");
    }

    [Test]
    public void AddWarning_expressionEx_message()
    {
      DepValue<bool> expressionEx = new DepConst<bool>(false);
      string message = "Hello";

      UIValidatorList sut = new UIValidatorList();
      sut.AddWarning(expressionEx, message);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsFalse(sut[0].IsError, "IsError");
      UIValidateResult res = sut[0].ResultEx.Value;
      Assert.AreEqual(message, res.Message, "ResultEx.Message");
      Assert.IsNull(sut[0].PreconditionEx, "PreconditionEx");
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Тестовый объект.
    /// Содежит 3 условия: Ошибку, предупреждение и снова ошибку.
    /// По умолчанию все выражения равны true (нет ошибок) и предусловия равны true (проверки выполняются).
    /// </summary>
    private class TestObject
    {
      #region Конструктор

      public TestObject()
      {
        SUT = new UIValidatorList();

        Expr1 = new DepInput<bool>();
        Expr1.Value = true;
        Prec1 = new DepInput<bool>();
        Prec1.Value = true;
        SUT.AddError(Expr1, "Message1", Prec1);

        Expr2 = new DepInput<bool>();
        Expr2.Value = true;
        Prec2 = new DepInput<bool>();
        Prec2.Value = true;
        SUT.AddWarning(Expr2, "Message2", Prec2);

        Expr3 = new DepInput<bool>();
        Expr3.Value = true;
        Prec3 = new DepInput<bool>();
        Prec3.Value = true;
        SUT.AddError(Expr3, "Message3", Prec3);

        VO = new UISimpleValidableObject();
      }

      #endregion

      #region Управляющие поля

      public DepInput<bool> Expr1, Expr2, Expr3;
      public DepInput<bool> Prec1, Prec2, Prec3;

      #endregion

      #region Тестовый объект

      public UIValidatorList SUT;

      public UISimpleValidableObject VO;

      #endregion
    }

    [Test]
    public void Validate_ok()
    {
      TestObject test = new TestObject();

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Ok, test.VO.ValidateState, "ValidateState");
    }

    [Test]
    public void Validate_error()
    {
      TestObject test = new TestObject();
      test.Expr1.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Error, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message1", test.VO.Message, "Message");
    }

    [Test]
    public void Validate_warning()
    {
      TestObject test = new TestObject();
      test.Expr2.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Warning, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message2", test.VO.Message, "Message");
    }

    [Test]
    public void Validate_error_warning()
    {
      TestObject test = new TestObject();
      test.Expr1.Value = false;
      test.Expr2.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Error, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message1", test.VO.Message, "Message");
    }

    [Test]
    public void Validate_warning_error()
    {
      TestObject test = new TestObject();
      test.Expr2.Value = false;
      test.Expr3.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Error, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message3", test.VO.Message, "Message");
    }

    [Test]
    public void Validate_error_error()
    {
      TestObject test = new TestObject();
      test.Expr1.Value = false;
      test.Expr3.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Error, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message1", test.VO.Message, "Message");
    }

    [Test]
    public void Validate_error_precondition()
    {
      TestObject test = new TestObject();
      test.Expr1.Value = false;
      test.Prec1.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Ok, test.VO.ValidateState, "ValidateState");
    }

    [Test]
    public void Validate_error_warning_precondition()
    {
      TestObject test = new TestObject();
      test.Expr1.Value = false;
      test.Expr2.Value = false;
      test.Prec1.Value = false;

      test.SUT.Validate(test.VO);
      Assert.AreEqual(UIValidateState.Warning, test.VO.ValidateState, "ValidateState");
      Assert.AreEqual("Message2", test.VO.Message, "Message");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      UIValidatorList sut = new UIValidatorList();
      sut.AddError(new DepConst<UIValidateResult>(UIValidateResult.Ok));

      sut.SetReadOnly();
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.AddError(new DepConst<UIValidateResult>(UIValidateResult.Ok)); }, "AddError()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.AddWarning(new DepConst<UIValidateResult>(UIValidateResult.Ok)); }, "AddWarning()");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)]bool setReadOnly)
    {
      DepValue<UIValidateResult> resultEx = new DepConst<UIValidateResult>(UIValidateResult.Ok);
      DepValue<bool> preconditionEx = new DepConst<bool>(true);

      UIValidatorList sut = new UIValidatorList();
      sut.AddError(resultEx, preconditionEx);
      if (setReadOnly)
        sut.SetReadOnly();

      byte[] b = SerializationTools.SerializeBinary(sut);

      UIValidatorList res = (UIValidatorList)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(1, res.Count, "Count");
      Assert.AreEqual(setReadOnly, res.IsReadOnly, "IsReadOnly");

      Assert.IsTrue(sut[0].IsError, "IsError");
      Assert.IsInstanceOf<DepConst<UIValidateResult>>(res[0].ResultEx, "ResultEx type");
      Assert.IsInstanceOf<DepConst<bool>>(res[0].PreconditionEx, "PreconditionEx type");
    }

    #endregion
  }
}
