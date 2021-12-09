using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  public class DepEvalExprTests
  {
    [Test]
    public void DynamicTest()
    {
      DepInput<int> arg = new DepInput<int>(1, null);

      DepEvalExpr<int> sut = new DepEvalExpr<int>("@1+10", new IDepValue[1] { arg });

      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("11", resprod.ToString(), "Original");

      arg.Value = 2;
      Assert.AreEqual("11|12", resprod.ToString(), "Changed");
    }

    [TestCase(1.0, 2, "@1+@2", 3.0)]
    [TestCase(1.0, 2, "@1-@2", -1.0)]
    [TestCase(1.0, 2, "@1*@2", 2.0)]
    [TestCase(1.0, 2, "@1/@2", 0.5)]
    [TestCase(1.0, 2, "-@2", -2.0)]
    [TestCase(-10.0, 0, "ABS(@1)", 10.0)]
    [TestCase(0, -5, "ABS(@2)", 5.0)]
    public void MathOps(double arg1, int arg2, string expr, double wanted)
    {
      IDepValue[] args = new IDepValue[2] { new DepConst<double>(arg1), new DepConst<int>(arg2) };
      DepEvalExpr<double> sut = new DepEvalExpr<double>(expr, args);
      Assert.AreEqual(wanted, sut.Value, 0.001);
    }

    [TestCase("ABC", "DEF", "@1+@2", "ABCDEF")]
    [TestCase("ABC", null, "@1+@2", "ABC")]
    [TestCase("AbCd", null, "UPPER(@1)", "ABCD")]
    [TestCase("AbCd", null, "LOWER(@1)", "abcd")]
    public void StringOps(string arg1, string arg2, string expr, string wanted)
    {
      IDepValue[] args = new IDepValue[2] { new DepConst<string>(arg1), new DepConst<string>(arg2) };
      DepEvalExpr<string> sut = new DepEvalExpr<string>(expr, args);
      Assert.AreEqual(wanted, sut.Value);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("@0+1")] // неправильный номер аргумента
    [TestCase("@2+1")] // неправильный номер аргумента
    [TestCase("@+1")] // номер аргумента не указан
    [TestCase("SOMETHINGUNKNOWN(@1)")] // неправильное имя функции
    [TestCase("/@1")] // неправильная операция
    public void Expression_exception(string expr)
    {
      IDepValue[] args = new IDepValue[1] { new DepConst<int>(0) };
      Assert.Catch(delegate() { new DepEvalExpr<double>(expr, args); });
    }
  }
}
