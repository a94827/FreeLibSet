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
  public class NumRangeFilterTests
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
      NumRangeFilter sut1 = new NumRangeFilter(expr1, minValue, maxValue);
      DoTestConstructor_Decimal(sut1, "With expression", minValue, maxValue, wantedDegeneration);

      NumRangeFilter sut2 = new NumRangeFilter("F1", minValue, maxValue);
      Assert.IsInstanceOf<DBxColumn>(sut2.Expression, "With columnName-Expression Type");
      DoTestConstructor_Decimal(sut2, "With columnName", minValue, maxValue, wantedDegeneration);
    }

    private static void DoTestConstructor_Decimal(NumRangeFilter sut, string messagePrefix, decimal? minValue, decimal? maxValue, DBxFilterDegeneration wantedDegeneration)
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

      NumRangeFilter sut = new NumRangeFilter("F1", minValue, maxValue);
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

      NumRangeFilter sut = new NumRangeFilter("F1", minValue, maxValue);
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

      NumRangeFilter sut = new NumRangeFilter("F1", minValue, maxValue);
      Assert.AreEqual(wantedMinValue, sut.MinValue, "MinValue");
      Assert.AreEqual(wantedMaxValue, sut.MaxValue, "MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      NumRangeFilter sut = new NumRangeFilter(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2")), 2m, 100m);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, lst);
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

    // Значение поля NULL интерапетируется как 0
    [TestCase("-2.5", "4.5", "", true)]
    [TestCase("2.5", "4.5", "", false)]
    [TestCase("-2.5", "", "", true)]
    [TestCase("2.5", "", "", false)]
    [TestCase("", "4.5", "", true)]
    [TestCase("", "-4.5", "", false)]
    [TestCase("", "", "", true)]

    public void TestFilter(string sMinValue, string sMaxValue, string sColValue, bool wantedRes)
    {
      decimal? minValue = null;
      if (sMinValue.Length > 0)
        minValue = StdConvert.ToDecimal(sMinValue);
      decimal? maxValue = null;
      if (sMaxValue.Length > 0)
        maxValue = StdConvert.ToDecimal(sMaxValue);
      decimal? colValue = null;
      if (sColValue.Length > 0)
        colValue = StdConvert.ToDecimal(sColValue);

      NumRangeFilter sut = new NumRangeFilter("F1", minValue, maxValue);
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue);

      Assert.AreEqual(wantedRes, sut.TestFilter(vals));
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      NumRangeFilter sut = new NumRangeFilter(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("F1"), new DBxColumn("F2")), 2m, 100m);
      byte[] b = SerializationTools.SerializeBinary(sut);
      NumRangeFilter res = (NumRangeFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.MinValue, res.MinValue, "MinValue");
      Assert.AreEqual(sut.MaxValue, res.MaxValue, "MaxValue");
    }

    #endregion
  }
}
