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
  public class NumRangeCrossFilterTests
  {
    #region Конструкторы

    [TestCase("1.5", "3.5", DBxFilterDegeneration.None)]
    [TestCase("1.5", "", DBxFilterDegeneration.None)]
    [TestCase("", "3.5", DBxFilterDegeneration.None)]
    [TestCase("", "", DBxFilterDegeneration.AlwaysTrue)]
    [TestCase("2", "2", DBxFilterDegeneration.None)]
    [TestCase("3.5", "3.4", DBxFilterDegeneration.AlwaysFalse)]
    public void Constructors_Decimal(string sMinValue, string sMaxValue, DBxFilterDegeneration wantedDegeneration)
    {
      decimal? minValue = null;
      if (sMinValue.Length > 0)
        minValue = StdConvert.ToDecimal(sMinValue);

      decimal? maxValue = null;
      if (sMaxValue.Length > 0)
        maxValue = StdConvert.ToDecimal(sMaxValue);

      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));
      NumRangeCrossFilter sut1 = new NumRangeCrossFilter(expr1, expr2, minValue, maxValue);
      DoTestConstructor_Decimal(sut1, "With expression", minValue, maxValue, wantedDegeneration);

      NumRangeCrossFilter sut2 = new NumRangeCrossFilter("F1", "F2", minValue, maxValue);
      Assert.IsInstanceOf<DBxColumn>(sut2.Expression1, "With columnName-Expression1 Type");
      Assert.IsInstanceOf<DBxColumn>(sut2.Expression2, "With columnName-Expression2 Type");
      DoTestConstructor_Decimal(sut2, "With columnName", minValue, maxValue, wantedDegeneration);
    }

    private static void DoTestConstructor_Decimal(NumRangeCrossFilter sut, string messagePrefix, decimal? minValue, decimal? maxValue, DBxFilterDegeneration wantedDegeneration)
    {
      Assert.AreEqual(minValue, sut.MinValue, messagePrefix + "-MinValue");
      Assert.AreEqual(maxValue, sut.MaxValue, messagePrefix + "-MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, messagePrefix + "-Degeneration");
    }

    [TestCase(-2, 3, DBxFilterDegeneration.None)]
    [TestCase(-2, null, DBxFilterDegeneration.None)]
    [TestCase(null, 3, DBxFilterDegeneration.None)]
    [TestCase(null, null, DBxFilterDegeneration.AlwaysTrue)]
    [TestCase(2, 2, DBxFilterDegeneration.None)]
    [TestCase(3, 2, DBxFilterDegeneration.AlwaysFalse)]
    public void Constructor_Int(int? minValue, int?maxValue, DBxFilterDegeneration wantedDegeneration)
    {
      decimal? wantedMinValue = minValue;
      decimal? wantedMaxValue = maxValue;

      NumRangeCrossFilter sut = new NumRangeCrossFilter("F1", "F2", minValue, maxValue);
      Assert.AreEqual(wantedMinValue, sut.MinValue, "MinValue");
      Assert.AreEqual(wantedMaxValue, sut.MaxValue, "MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    [TestCase(-2.5f, 3.5f, DBxFilterDegeneration.None)]
    [TestCase(-2f, null, DBxFilterDegeneration.None)]
    [TestCase(null, 3f, DBxFilterDegeneration.None)]
    [TestCase(null, null, DBxFilterDegeneration.AlwaysTrue)]
    [TestCase(2f, 2f, DBxFilterDegeneration.None)]
    [TestCase(3f, 2f, DBxFilterDegeneration.AlwaysFalse)]
    public void Constructor_Single(float? minValue, float? maxValue, DBxFilterDegeneration wantedDegeneration)
    {
      decimal? wantedMinValue = null;
      if (minValue.HasValue) wantedMinValue = (decimal)(minValue.Value);
      decimal? wantedMaxValue = null;
      if (maxValue.HasValue) wantedMaxValue = (decimal)(maxValue.Value);

      NumRangeCrossFilter sut = new NumRangeCrossFilter("F1", "F2", minValue, maxValue);
      Assert.AreEqual(wantedMinValue, sut.MinValue, "MinValue");
      Assert.AreEqual(wantedMaxValue, sut.MaxValue, "MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    [TestCase(-2.5, 3.5, DBxFilterDegeneration.None)]
    [TestCase(-2.0, null, DBxFilterDegeneration.None)]
    [TestCase(null, 3.0, DBxFilterDegeneration.None)]
    [TestCase(null, null, DBxFilterDegeneration.AlwaysTrue)]
    [TestCase(2.0, 2.0, DBxFilterDegeneration.None)]
    [TestCase(3.0, 2.0, DBxFilterDegeneration.AlwaysFalse)]
    public void Constructor_Double(double? minValue, double? maxValue, DBxFilterDegeneration wantedDegeneration)
    {
      decimal? wantedMinValue = null;
      if (minValue.HasValue) wantedMinValue = (decimal)(minValue.Value);
      decimal? wantedMaxValue = null;
      if (maxValue.HasValue) wantedMaxValue = (decimal)(maxValue.Value);

      NumRangeCrossFilter sut = new NumRangeCrossFilter("F1", "F2", minValue, maxValue);
      Assert.AreEqual(wantedMinValue, sut.MinValue, "MinValue");
      Assert.AreEqual(wantedMaxValue, sut.MaxValue, "MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));
      NumRangeCrossFilter sut = new NumRangeCrossFilter(expr1, expr2, 2m, 100m);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      CollectionAssert.AreEquivalent(new string[] { "F1", "F2", "F3", "F4" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("1", "2", "2.0", "3", true)]
    [TestCase("1", "2", "2.1", "3", false)]
    [TestCase("1", "2", "", "1.0", true)]
    [TestCase("1", "2", "", "0.9", false)]
    [TestCase("1", "2", "2.0", "", true)]
    [TestCase("1", "2", "2.1", "", false)]
    [TestCase("", "2", "2", "", true)]
    [TestCase("", "2", "2.1", "", false)]
    [TestCase("", "", "", "", true)]

    public void TestFilter(string sMinValue, string sMaxValue, string sCol1Value, string sCol2Value, bool wantedRes)
    {
      decimal? minValue = null;
      if (sMinValue.Length > 0)
        minValue = StdConvert.ToDecimal(sMinValue);
      decimal? maxValue = null;
      if (sMaxValue.Length > 0)
        maxValue = StdConvert.ToDecimal(sMaxValue);
      decimal? col1Value = null;
      if (sCol1Value.Length > 0)
        col1Value = StdConvert.ToDecimal(sCol1Value);
      decimal? col2Value = null;
      if (sCol2Value.Length > 0)
        col2Value = StdConvert.ToDecimal(sCol2Value);

      NumRangeCrossFilter sut1 = new NumRangeCrossFilter("F1", "F2", minValue, maxValue);
      TypedStringDictionary<object> vals1 = new TypedStringDictionary<object>(false);
      vals1.Add("F1", col1Value);
      vals1.Add("F2", col2Value);
      Assert.AreEqual(wantedRes, sut1.TestFilter(vals1), "#1");

      NumRangeCrossFilter sut2 = new NumRangeCrossFilter("F1", "F2", col1Value, col2Value);
      TypedStringDictionary<object> vals2 = new TypedStringDictionary<object>(false);
      vals2.Add("F1", minValue);
      vals2.Add("F2", maxValue);
      Assert.AreEqual(wantedRes, sut2.TestFilter(vals2), "#2");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2"));
      DBxExpression expr2 = new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F3"), new DBxColumn("F4"));
      NumRangeCrossFilter sut = new NumRangeCrossFilter(expr1, expr2, 2m, 100m);
      byte[] b = SerializationTools.SerializeBinary(sut);
      NumRangeCrossFilter res = (NumRangeCrossFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.MinValue, res.MinValue, "MinValue");
      Assert.AreEqual(sut.MaxValue, res.MaxValue, "MaxValue");
    }

    #endregion
  }
}
