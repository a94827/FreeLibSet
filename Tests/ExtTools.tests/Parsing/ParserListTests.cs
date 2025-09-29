using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Parsing;
using FreeLibSet.Core;

namespace ExtTools_tests.Parsing
{
  public abstract class ParserListTestsBase
  {
    #region Тестовый объект

    public enum FloatType { None, Single, Double, Decimal }

    public static ParserList CreateParserList(FloatType floatType)
    {
      ParserList pl = new ParserList();

      FunctionParser fp = new FunctionParser(); // перед MathOpParser
      fp.ArgSeparators.Clear();
      fp.ArgSeparators.Add(",");
      ExcelFunctions.AddFunctions(fp);
      pl.Add(fp);

      pl.Add(new MathOpParser());

      NumConstParser ncp = new NumConstParser();
      ncp.NumberFormat = StdConvert.NumberFormat;
      ncp.AllowInt32 = true;
      ncp.AllowSingle = false;
      ncp.AllowDouble = false;
      ncp.AllowDecimal = false;
      switch (floatType)
      {
        case FloatType.Single: ncp.AllowSingle = true; break;
        case FloatType.Double: ncp.AllowDecimal = true; break;
        case FloatType.Decimal: ncp.AllowDecimal = true; break;
      }
      pl.Add(ncp);

      pl.Add(new StrConstParser());
      pl.Add(new SpaceParser());

      return pl;
    }

    #endregion

    #region Реализация методов вычислений

    protected void DoEvaluate(string expr, FloatType floatType, object wantedRes)
    {
      ParserList pl = CreateParserList(floatType);
      ParsingData pd = new ParsingData(expr);
      pl.Parse(pd);
      DoLogError(pd);
      Assert.IsNull(pd.FirstErrorToken, floatType.ToString() + ". Parsing error");
      IExpression expr2 = pl.CreateExpression(pd);
      DoLogError(pd);
      Assert.IsNotNull(expr2, floatType.ToString() + ". CreateExpression()");
      Assert.IsNull(pd.FirstErrorToken, floatType.ToString() + ". CreateExpression() error");
      object res = expr2.Calc();
      Assert.AreEqual(wantedRes, res, floatType.ToString() + ". Result");
    }

    protected void DoEvaluateAllTypes(string expr, object wantedRes)
    {
      DoEvaluate(expr, FloatType.None, wantedRes);
      DoEvaluate(expr, FloatType.Single, wantedRes);
      DoEvaluate(expr, FloatType.Double, wantedRes);
      DoEvaluate(expr, FloatType.Decimal, wantedRes);
    }

    private void DoLogError(ParsingData pd)
    {
      if (pd.FirstErrorToken != null)
        Console.WriteLine(pd.Text.ToString() + ", Error token=" + pd.FirstErrorToken.ToString() + ". Error: " + pd.FirstErrorToken.ErrorMessage.Value.Text);
    }

    protected void DoEvaluateDouble(string expr, double wantedRes, double delta)
    {
      ParserList pl = CreateParserList(FloatType.Double);
      ParsingData pd = new ParsingData(expr);
      pl.Parse(pd);
      DoLogError(pd);
      Assert.IsNull(pd.FirstErrorToken, "parsing error");
      IExpression expr2 = pl.CreateExpression(pd);
      DoLogError(pd);
      Assert.IsNull(pd.FirstErrorToken, "CreateExpression() error");
      object res = expr2.Calc();
      Assert.AreEqual(wantedRes, (double)res, delta, "result");
    }

    protected void DoEvaluateError(string expr, FloatType floatType)
    {
      ParserList pl = CreateParserList(floatType);
      ParsingData pd = new ParsingData(expr);
      pl.Parse(pd);
      IExpression expr2 = pl.CreateExpression(pd);
      DoLogError(pd);
      //Assert.IsNull(expr2, "CreateExpression() result");
      Assert.IsNotNull(pd.FirstErrorToken, "FirstErrorToken");
    }

    #endregion
  }
}
