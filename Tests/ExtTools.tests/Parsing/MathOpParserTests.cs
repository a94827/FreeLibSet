using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Parsing;
using FreeLibSet.Core;

namespace ExtTools_tests.Parsing
{
  [TestFixture]
  class MathOpParserTests : ParserListTestsBase
  {
    #region Математические операции

    // math operations
    [TestCase("8*2", 16)]
    [TestCase("8/2", 4)]
    [TestCase("8+2", 10)]
    [TestCase("8-2", 6)]
    // number signs
    [TestCase("+2", 2)]
    [TestCase("-2", -2)]
    [TestCase("-2+3", 1)]
    [TestCase("-2-3", -5)]
    [TestCase("2--3", 5, Ignore = true, IgnoreReason = "Unary op at the right side io binary op is not implemented yet")]
    [TestCase("2-+3", -1, Ignore = true, IgnoreReason = "Unary op at the right side io binary op is not implemented yet")]
    [TestCase("+(5-3)", 2)]
    [TestCase("-(5-3)", -2)]
    [TestCase("3*+2", 6, Ignore = true, IgnoreReason = "Unary op at the right side io binary op is not implemented yet")]
    [TestCase("3*-2", -6, Ignore = true, IgnoreReason = "Unary op at the right side io binary op is not implemented yet")]
    [TestCase("3*(+2)", 6)]
    [TestCase("3*(-2)", -6)]
    [TestCase("(+2)*3", 6)]
    [TestCase("(-2)*3", -6)]
    // natural operation order
    [TestCase("1+2*3", 7)]
    [TestCase("1+2*3-4", 3)]
    [TestCase("1+12/4", 4)]
    [TestCase("1*2+12/4", 5)]
    [TestCase("1+3*20/5", 13)]
    // parentheses
    [TestCase("2*(1+3)", 8)]
    [TestCase("(1+2)*(3+4)", 21)]
    [TestCase("(1+2*3)*4", 28)]
    [TestCase("1+(2*3+4)", 11)]
    [TestCase("((1+2)*3)*4", 36)]
    [TestCase("(((1+2)*3+4)*5)/13", 5)]

    // functions and parentheses
    [TestCase("MAX(MIN(1,3),MIN(2,4))", 2)]
    [TestCase("MAX((2+3)*2,((1+2)*3)*4)", 36)]
    [TestCase("MAX((MIN(1,3)+1)*2,2)", 4)]
    [TestCase("(MAX(1,3)+3)*5", 30)]
    [TestCase("(MAX(1,3)+3)*MIN(2,3)", 12)]
    [TestCase("MAX((2+MIN(1,2))*2,1)", 6)]
    public void MathExpressions(string expr, object wantedRes)
    {
      DoEvaluateAllTypes(expr, wantedRes);
    }

    [TestCase("2*2.5")]
    public void MathExpressions_error_nofloatconst(string expr)
    {
      DoEvaluateError(expr, FloatType.None);
    }

    [TestCase("2$2", Description = "Unknown op")]
    [TestCase("2+", Description = "Operand missing")]
    [TestCase("*2", Description = "Operand missing")]
    [TestCase("(2+2*3", Description = "Unpaired parenthesis")]
    [TestCase("2+2)*3", Description = "Unpaired parenthesis")]
    [TestCase("(2+2))*3", Description = "Extra parenthesis")]
    [TestCase("2*(", Description = "Broken after parenthesis")]
    [TestCase(")*2", Description = "Starts with closing parenthesis")]
    [TestCase("2*()", Description = "Void in parentheses")]
    [TestCase("(2+2)3", Description = "No operartion after parenthesis")]
    [TestCase("2(2+3)", Description = "No operartion before parenthesis")]
    [TestCase("2(2+)", Description = "Operand missing before parenthesis")]
    [TestCase("(2+2)(3+3)", Description = "Operand missing between parenthesis")]
    // Так можно. [TestCase("3*(- 2)", Description = "Space after unary op")]
    // Так можно. [TestCase("3*(+ 2)", Description = "Space after unary op")]
    [TestCase("3 * - 2", Description = "Space after unary op")]
    [TestCase("3 * + 2", Description = "Space after unary op")]
    [TestCase("3 * * 2", Description = "Operation twice")]
    [TestCase("3 / / 2", Description = "Operation twice")]
    [TestCase("MIN(1,2", Description = "Closing parenthesis missed")]
    [TestCase("MIN 1,2", Description = "Opening parenthesis missed")]
    [TestCase("MIN(1 2)", Description = "Arg separator missed")]
    [TestCase("MIN)(", Description = "Wrong parentheses")]
    [TestCase("MIN(1,)", Description = "Arg missed")]
    [TestCase("MIN(,2)", Description = "Extra comma")]
    [TestCase("MIN(1,,2)", Description = "Extra comma")]
    public void MathExpressions_error(string expr)
    {
      DoEvaluateError(expr, FloatType.Decimal);
    }

    #endregion

    #region Логические операции

    // number comparisions
    [TestCase("1>2", false)]
    [TestCase("2>2", false)]
    [TestCase("3>2", true)]
    [TestCase("1>=2", false)]
    [TestCase("2>=2", true)]
    [TestCase("3>=2", true)]
    [TestCase("1<2", true)]
    [TestCase("2<2", false)]
    [TestCase("3<2", false)]
    [TestCase("1<=2", true)]
    [TestCase("2<=2", true)]
    [TestCase("3<=2", false)]
    [TestCase("1=2", false)]
    [TestCase("2=2", true)]
    [TestCase("3=2", false)]
    [TestCase("1<>2", true)]
    [TestCase("2<>2", false)]
    [TestCase("3<>2", true)]
    public void LogicalExpressions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion

    #region Операции со строками

    [TestCase("\"AAA\"+\"BBB\"", "AAABBB")]
    public void StringExpressions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion

    #region Операции с датой и временем

    [Test]
    public void Date_add_Time()
    {
      DoEvaluate("DATE(2022,5,31)+TIME(12,34,56)", FloatType.None, new DateTime(2022, 5, 31, 12, 34, 56));
    }

    [Test]
    public void Date_sub_Time()
    {
      DoEvaluate("DATE(2022,5,31)-TIME(1,0,0)", FloatType.None, new DateTime(2022, 5, 30, 23, 0, 0));
    }

    [Test]
    public void Time_add_Time()
    {
      DoEvaluate("TIME(1,2,3)+TIME(4,5,6)", FloatType.None, new TimeSpan(5,7,9));
    }

    [Test]
    public void Time_sub_Time()
    {
      DoEvaluate("TIME(1,2,3)-TIME(4,5,6)", FloatType.None, new TimeSpan(3, 3, 3).Negate());
    }

    [Test]
    public void Date_sub_Date()
    {
      DoEvaluate("DATE(2022,5,31)-DATE(2022,5,30)", FloatType.None, new TimeSpan(1,0,0,0));
    }

    [TestCase("DATE(2022,2,27)>DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,28)>DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,3,1)>DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,2,27)>=DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,28)>=DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,3,1)>=DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,2,27)<DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,2,28)<DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,3,1)<DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,27)<=DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,2,28)<=DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,3,1)<=DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,27)=DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,28)=DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,3,1)=DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,2,27)<>DATE(2022,2,28)", true)]
    [TestCase("DATE(2022,2,28)<>DATE(2022,2,28)", false)]
    [TestCase("DATE(2022,3,1)<>DATE(2022,2,28)", true)]
    public void DateLogicalExpressions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    [TestCase("TIME(0,0,2)>TIME(0,0,1)", true)]
    [TestCase("TIME(0,0,2)>TIME(0,0,2)", false)]
    [TestCase("TIME(0,0,2)>TIME(0,0,3)", false)]
    [TestCase("TIME(0,0,2)>=TIME(0,0,1)", true)]
    [TestCase("TIME(0,0,2)>=TIME(0,0,2)", true)]
    [TestCase("TIME(0,0,2)>=TIME(0,0,3)", false)]
    [TestCase("TIME(0,0,2)<TIME(0,0,1)", false)]
    [TestCase("TIME(0,0,2)<TIME(0,0,2)", false)]
    [TestCase("TIME(0,0,2)<TIME(0,0,3)", true)]
    [TestCase("TIME(0,0,2)<=TIME(0,0,1)", false)]
    [TestCase("TIME(0,0,2)<=TIME(0,0,2)", true)]
    [TestCase("TIME(0,0,2)<=TIME(0,0,3)", true)]
    [TestCase("TIME(0,0,2)=TIME(0,0,1)", false)]
    [TestCase("TIME(0,0,2)=TIME(0,0,2)", true)]
    [TestCase("TIME(0,0,2)=TIME(0,0,3)", false)]
    [TestCase("TIME(0,0,2)<>TIME(0,0,1)", true)]
    [TestCase("TIME(0,0,2)<>TIME(0,0,2)", false)]
    [TestCase("TIME(0,0,2)<>TIME(0,0,3)", true)]
    public void TimeLogicalExpressions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    [TestCase("DATE(2022,11,8)+TIME(23,11,59)>DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)>DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)>DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,8)+TIME(23,11,59)>=DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)>=DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)>=DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,8)+TIME(23,11,59)<DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)<DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)<DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,8)+TIME(23,11,59)<=DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)<=DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)<=DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,8)+TIME(23,11,59)=DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)=DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)=DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,8)+TIME(23,11,59)<>DATE(2022,11,9)", true)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,0)<>DATE(2022,11,9)", false)]
    [TestCase("DATE(2022,11,9)+TIME(0,0,1)<>DATE(2022,11,9)", true)]
    public void DateTimeLogicalExpressions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion
  }
}
