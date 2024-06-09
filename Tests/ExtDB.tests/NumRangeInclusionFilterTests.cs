using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class NumRangeInclusionFilterTests
  {
    #region Конструкторы

    [Test]
    public void Constructors_Decimal()
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));

      NumRangeInclusionFilter sut1 = new NumRangeInclusionFilter(expr1, expr2, 1m);
      DoTestConstructor(sut1, "Decimal #1");

      NumRangeInclusionFilter sut2 = new NumRangeInclusionFilter("F1", "F2", 1m);
      DoTestConstructor(sut2, "Decimal #2");

      NumRangeInclusionFilter sut3 = new NumRangeInclusionFilter(expr1, expr2, 1.0);
      DoTestConstructor(sut3, "Double #3");

      NumRangeInclusionFilter sut4 = new NumRangeInclusionFilter("F1", "F2", 1.0);
      DoTestConstructor(sut4, "Double #4");

      NumRangeInclusionFilter sut5 = new NumRangeInclusionFilter(expr1, expr2, 1f);
      DoTestConstructor(sut5, "Single #5");

      NumRangeInclusionFilter sut6 = new NumRangeInclusionFilter("F1", "F2", 1f);
      DoTestConstructor(sut6, "Single #6");

      NumRangeInclusionFilter sut7 = new NumRangeInclusionFilter(expr1, expr2, 1);
      DoTestConstructor(sut7, "Int32 #7");

      NumRangeInclusionFilter sut8 = new NumRangeInclusionFilter("F1", "F2", 1);
      DoTestConstructor(sut8, "Int32 #8");
    }

    private static void DoTestConstructor(NumRangeInclusionFilter sut, string messagePrefix)
    {
      Assert.AreEqual(1m, sut.Value, messagePrefix + " - Value");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, messagePrefix + " - Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));
      NumRangeInclusionFilter sut = new NumRangeInclusionFilter(expr1, expr2, 100m);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      CollectionAssert.AreEquivalent(new string[] { "F1", "F2", "F3", "F4" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("-2.5", "4.5", "3", true)]
    [TestCase("-2.5", "4.5", "-2.5", true)]
    [TestCase("-2.5", "4.5", "4.5", true)]
    [TestCase("-2.5", "4.5", "-2.6", false)]
    [TestCase("-2.5", "4.5", "4.6", false)]

    [TestCase("-2.5", "", "-2.4", true)]
    [TestCase("-2.5", "", "-2.5", true)]
    [TestCase("-2.5", "", "-2.6", false)]

    [TestCase("", "4.5", "4.4", true)]
    [TestCase("", "4.5", "4.5", true)]
    [TestCase("", "4.5", "4.6", false)]

    [TestCase("", "", "2", true)]

    public void TestFilter(string sColValue1, string sColValue2, string sValue, bool wantedRes)
    {
      decimal? colValue1 = null;
      if (sColValue1.Length > 0)
        colValue1 = StdConvert.ToDecimal(sColValue1);
      decimal? colValue2 = null;
      if (sColValue2.Length > 0)
        colValue2 = StdConvert.ToDecimal(sColValue2);
      decimal value = StdConvert.ToDecimal(sValue);

      NumRangeInclusionFilter sut = new NumRangeInclusionFilter("F1", "F2", value);
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue1);
      vals.Add("F2", colValue2);

      Assert.AreEqual(wantedRes, sut.TestFilter(vals));
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));
      NumRangeInclusionFilter sut = new NumRangeInclusionFilter(expr1, expr2, 123m);
      byte[] b = SerializationTools.SerializeBinary(sut);
      NumRangeInclusionFilter res = (NumRangeInclusionFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Value, res.Value, "Value");
    }

    #endregion
  }
}
