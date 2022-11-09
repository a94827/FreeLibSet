using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Parsing;
using FreeLibSet.Core;

namespace ExtTools_tests.Parsing
{
  [TestFixture]
  class ExcelFunctionsTests : ParserListTestsBase
  {
    // TODO: Не все функции

    #region Математические функции

    [TestCase("SUM(1)", 1)]
    [TestCase("SUM(1,2,3)", 6)]

    [TestCase("SIGN(10)", 1)]
    [TestCase("SIGN(-10)", -1)]
    [TestCase("SIGN(0)", 0)]

    [TestCase("ABS(10)", 10)]
    [TestCase("ABS(-10)", 10)]
    [TestCase("ABS(0)", 0)]

    [TestCase("MIN(1,3,5)", 1)]
    [TestCase("MIN(1)", 1)]

    [TestCase("MAX(1,3,5)", 5)]
    [TestCase("MAX(1)", 1)]
    public void MathFunctions_AnyType(string expr, object wantedRes)
    {
      DoEvaluateAllTypes(expr, wantedRes);
    }

    [TestCase("ROUND(1.52)", 2)]
    [TestCase("ROUND(1.52, 1)", 1.5)]
    [TestCase("ROUND(-1.52)", -2)]
    [TestCase("ROUND(126, -1)", 130)]

    [TestCase("FLOOR(1.52)", 1)]
    [TestCase("FLOOR(1.52, 1)", 1.5)]
    [TestCase("FLOOR(-1.52)", -2)]
    [TestCase("FLOOR(126, -1)", 120)]

    [TestCase("CEILING(1.52)", 2)]
    [TestCase("CEILING(1.52, 1)", 1.6)]
    [TestCase("CEILING(-1.52)", -1)]
    [TestCase("CEILING(126, -1)", 130)]

    [TestCase("TRUNC(1.52)", 1)]
    [TestCase("TRUNC(1.52, 1)", 1.5)]
    [TestCase("TRUNC(-1.52)", -1)]
    [TestCase("TRUNC(126, -1)", 120)]

    [TestCase("POWER(2, 10)", 1024)]
    [TestCase("SQRT(16)", 4)]
    public void MathFunctions_Double_or_Decimal(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.Decimal, wantedRes);
      DoEvaluate(expr, FloatType.Double, wantedRes);
    }

    [TestCase("PI()", 3.14, 0.01)]
    [TestCase("DEGREES(3.14159)", 180.0, 0.1)]
    [TestCase("RADIANS(180)", 3.14, 0.01)]
    [TestCase("COS(0)", 1.00, 0.01)]
    [TestCase("COS(3.14/3)", 0.50, 0.01)]
    [TestCase("ACOS(1)", 0.00, 0.01)]
    [TestCase("ACOS(0.5)", 3.14 / 3, 0.01)]
    [TestCase("SIN(0)", 0.00, 0.01)]
    [TestCase("SIN(3.14/6)", 0.50, 0.01)]
    [TestCase("ASIN(0)", 0.00, 0.01)]
    [TestCase("ASIN(0.5)", 3.14 / 6, 0.01)]
    [TestCase("TAN(0)", 0.00, 0.01)]
    [TestCase("TAN(3.14/4)", 1.00, 0.01)]
    [TestCase("ATAN(0)", 0.00, 0.01)]
    [TestCase("ATAN(1)", 3.14 / 4, 0.01)]
    [TestCase("ATAN2(0, 1)", 3.14 / 2, 0.01)]
    [TestCase("ATAN2(1, 0)", 0.00, 0.01)]
    // TODO: Гиперболические функции

    [TestCase("EXP(1)", 2.72, 0.01)]
    [TestCase("LN(1)", 0.00, 0.01)]
    [TestCase("LN(2.72)", 1.00, 0.01)]
    [TestCase("LOG10(1)", 0.00, 0.01)]
    [TestCase("LOG10(100)", 2.00, 0.01)]
    [TestCase("LOG10(0.1)", -1.00, 0.01)]
    [TestCase("LOG(1,2)", 0.00, 0.01)]
    [TestCase("LOG(16,4)", 2.00, 0.01)]
    [TestCase("POWER(2,3)", 8.00, 0.01)]
    public void MathFunctions_Double(string expr, double wantedRes, double delta)
    {
      DoEvaluateDouble(expr, wantedRes, delta);
    }

    #endregion

    #region Строковые функции

    [TestCase("LEN(\"AAA\")", 3)]
    [TestCase("LEN(\"\")", 0)]

    [TestCase("LEFT(\"ABCD\", 2)", "AB")]
    [TestCase("LEFT(\"ABCD\", 5)", "ABCD")]
    [TestCase("LEFT(\"ABCD\", 0)", "")]
    [TestCase("LEFT(\"\", 2)", "")]

    [TestCase("RIGHT(\"ABCD\", 2)", "CD")]
    [TestCase("RIGHT(\"ABCD\", 5)", "ABCD")]
    [TestCase("RIGHT(\"ABCD\", 0)", "")]
    [TestCase("RIGHT(\"\", 2)", "")]

    [TestCase("LOWER(\"AbCd\")", "abcd")]
    [TestCase("LOWER(\"\")", "")]

    [TestCase("UPPER(\"AbCd\")", "ABCD")]
    [TestCase("UPPER(\"\")", "")]

    [TestCase("CONCATENATE()", "")]
    [TestCase("CONCATENATE(\"ABC\")", "ABC")]
    [TestCase("CONCATENATE(\"ABC\", \"DEF\")", "ABCDEF")]
    [TestCase("CONCATENATE(\"ABC\", \"DEF\", \"GHI\")", "ABCDEFGHI")]

    [TestCase("REPLACE(\"ABCD\",2,2,\"GHI\")", "AGHID")]
    [TestCase("REPLACE(\"ABCD\",1,0,\"GHI\")", "GHIABCD")]
    [TestCase("REPLACE(\"ABCD\",2,2,\"\")", "AD")]

    [TestCase("SUBSTITUTE(\"AABBBAACCA\",\"AA\",\"GGG\")", "GGGBBBGGGCCA")]
    [TestCase("SUBSTITUTE(\"AABBBAACCA\",\"AA\",\"\")", "BBBCCA")]
    public void StringFunctions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion

    #region Логические

    [TestCase("TRUE()", true)]
    [TestCase("FALSE()", false)]

    [TestCase("AND(2>1,4>3)", true)]
    [TestCase("AND(2>1,4<3)", false)]
    [TestCase("AND(2<1,4>3)", false)]
    [TestCase("AND(2<1,4<3)", false)]

    [TestCase("OR(2>1,4>3)", true)]
    [TestCase("OR(2>1,4<3)", true)]
    [TestCase("OR(2<1,4>3)", true)]
    [TestCase("OR(2<1,4<3)", false)]

    [TestCase("NOT(2>1)", false)]
    [TestCase("NOT(2<1)", true)]

    [TestCase("IF(2>1,10,20)", 10)]
    [TestCase("IF(2<1,10,20)", 20)]

    [TestCase("CHOOSE(1,10,20,30)", 10)]
    [TestCase("CHOOSE(2,10,20,30)", 20)]
    [TestCase("CHOOSE(3,10,20,30)", 30)]
    public void LogicalFunctions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion

    #region Дата и время

    [Test]
    public void Date()
    {
      DoEvaluate("DATE(2022,5,31)", FloatType.None, new DateTime(2022, 5, 31));
    }

    [Test]
    public void Time()
    {
      DoEvaluate("TIME(12,34,56)", FloatType.None, new TimeSpan(12, 34, 56));
    }

    [TestCase("YEAR(DATE(2022, 5, 31))", 2022)]
    [TestCase("MONTH(DATE(2022, 5, 31))", 5)]
    [TestCase("DAY(DATE(2022, 5, 31))", 31)]
    [TestCase("HOUR(TIME(12, 34, 56))", 12)]
    [TestCase("MINUTE(TIME(12, 34, 56))", 34)]
    [TestCase("SECOND(TIME(12, 34, 56))", 56)]
    [TestCase("HOUR(DATE(2022, 5, 31))", 0)]
    [TestCase("MINUTE(DATE(2022, 5, 31))", 0)]
    [TestCase("SECOND(DATE(2022, 5, 31))", 0)]

    [TestCase("WEEKDAY(DATE(2022, 5, 31))", 3)] // 31.05.2022 - вторник, возвращаемый номер: 1-вс,2-пн,3-вт,...

    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2022,12,31),\"d\")", 364)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2023,1,1),\"d\")", 365)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2023,1,1),\"D\")", 365, Description="Uppercased 3rd arg")]

    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2022,12,31),\"m\")", 11)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2023,1,1),\"m\")", 12)]
    [TestCase("DATEDIF(DATE(2022,2,15),DATE(2022,3,14),\"m\")", 0)]
    [TestCase("DATEDIF(DATE(2022,2,15),DATE(2022,3,15),\"m\")", 1)]
    [TestCase("DATEDIF(DATE(2022,2,28),DATE(2022,3,27),\"m\")", 0)]
    [TestCase("DATEDIF(DATE(2022,2,28),DATE(2022,3,28),\"m\")", 1)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2022,12,31),\"M\")", 11, Description = "Uppercased 3rd arg")]

    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2022,12,31),\"y\")", 0)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2023,1,1),\"y\")", 1)]
    [TestCase("DATEDIF(DATE(2022,1,1),DATE(2023,1,1),\"Y\")", 1, Description = "Uppercased 3rd arg")]

    [TestCase("DAYS(DATE(2023,1,1),DATE(2022,1,1))", 365)]
    public void DateTimeFunctions(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
    }

    #endregion

    //[Test]
    //public void Function_WrongArgCount(string expr)
    //{
    //  DoEvaluateError(expr, FloatType.Decimal);
    //}
  }
}
