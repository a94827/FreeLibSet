using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DateRangeFilterTests
  {
    #region Конструкторы

    // Закрытые интервалы
    [TestCase("2023-05-29", "2023-06-15", "2023-05-29", "2023-06-15", DBxFilterDegeneration.None)]
    [TestCase("2023-05-29", "2023-05-29", "2023-05-29", "2023-05-29", DBxFilterDegeneration.None)]
    [TestCase("2023-05-29", "2023-05-28", "2023-05-29", "2023-05-28", DBxFilterDegeneration.AlwaysFalse)]
    // Полуоткрытые интервалы
    [TestCase("2023-05-29", "", "2023-05-29", "", DBxFilterDegeneration.None)]
    [TestCase("", "2023-05-29", "", "2023-05-29", DBxFilterDegeneration.None)]
    [TestCase("", "", "", "", DBxFilterDegeneration.AlwaysTrue)]
    // Замена DateTimeMinValue/MaxValue
    [TestCase("2023-05-29", "9999-12-31", "2023-05-29", "", DBxFilterDegeneration.None)]
    [TestCase("0001-01-01", "2023-05-29", "", "2023-05-29", DBxFilterDegeneration.None)]
    [TestCase("0001-01-01", "9999-12-31", "", "", DBxFilterDegeneration.AlwaysTrue)]
    // Обрезание компонентов времени
    [TestCase("2023-05-29", "2023-06-15T12:34:56", "2023-05-29", "2023-06-15", DBxFilterDegeneration.None)]
    [TestCase("2023-05-29T12:34:56", "2023-05-29", "2023-05-29", "2023-05-29", DBxFilterDegeneration.None)]
    [TestCase("2023-05-29T12:34:56", "", "2023-05-29", "", DBxFilterDegeneration.None)]
    [TestCase("0001-01-01T12:34:56", "2023-05-29", "", "2023-05-29", DBxFilterDegeneration.None)]
    [TestCase("2023-05-29", "9999-12-31T12:34:56", "2023-05-29", "", DBxFilterDegeneration.None)]
    public void Constructor_2NDateTime(string sMinValue, string sMaxValue, string sWantedMinValue, string sWantedMaxValue, DBxFilterDegeneration wantedDegeneration)
    {
      DateTime? minValue = null;
      if (sMinValue.Length > 0) minValue = StdConvert.ToDateTime(sMinValue, true);
      DateTime? maxValue = null;
      if (sMaxValue.Length > 0) maxValue = StdConvert.ToDateTime(sMaxValue, true);
      DateTime? wantedMinValue = null;
      if (sWantedMinValue.Length > 0) wantedMinValue = StdConvert.ToDateTime(sWantedMinValue, false);
      DateTime? wantedMaxValue = null;
      if (sWantedMaxValue.Length > 0) wantedMaxValue = StdConvert.ToDateTime(sWantedMaxValue, false);

      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("F1"), new DBxConst(new DateTime(2023, 5, 29)));
      DateRangeFilter sut1 = new DateRangeFilter(expr1, minValue, maxValue);
      DoTestConstructor_2NDateTime(sut1, "Expression", wantedMinValue, wantedMaxValue, wantedDegeneration);

      DateRangeFilter sut2 = new DateRangeFilter("F1", minValue, maxValue);
      Assert.IsInstanceOf<DBxColumn>(sut2.Expression);
      DoTestConstructor_2NDateTime(sut1, "ColumnName", wantedMinValue, wantedMaxValue, wantedDegeneration);
    }


    private static void DoTestConstructor_2NDateTime(DateRangeFilter sut, string MessagePrefix, DateTime? wantedMinValue, DateTime? wantedMaxValue, DBxFilterDegeneration wantedDegeneration)
    {
      Assert.AreEqual(wantedMinValue, sut.MinValue, MessagePrefix + "-MinValue");
      Assert.AreEqual(wantedMaxValue, sut.MaxValue, MessagePrefix + "-MaxValue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, MessagePrefix + "-Degeneration");
    }

    [Test]
    public void Constructor_Year()
    {
      DateRangeFilter sut = new DateRangeFilter("F1", 2023);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.AreEqual(new DateTime(2023, 1, 1), sut.MinValue, "MinValue");
      Assert.AreEqual(new DateTime(2023, 12, 31), sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_Year_Month()
    {
      DateRangeFilter sut = new DateRangeFilter("F1", 2023, 5);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.AreEqual(new DateTime(2023, 5, 1), sut.MinValue, "MinValue");
      Assert.AreEqual(new DateTime(2023, 5, 31), sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_Year_Month_Day()
    {
      DateRangeFilter sut = new DateRangeFilter("F1", 2023, 5, 29);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.AreEqual(new DateTime(2023, 5, 29), sut.MinValue, "MinValue");
      Assert.AreEqual(new DateTime(2023, 5, 29), sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_Date()
    {
      DateRangeFilter sut = new DateRangeFilter("F1", new DateTime(2023, 5, 29));
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.AreEqual(new DateTime(2023, 5, 29), sut.MinValue, "MinValue");
      Assert.AreEqual(new DateTime(2023, 5, 29), sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_DateRange()
    {
      DateRange dtr = new DateRange(2023, 5);
      DateRangeFilter sut = new DateRangeFilter("F1", dtr);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.AreEqual(new DateTime(2023, 5, 1), sut.MinValue, "MinValue");
      Assert.AreEqual(new DateTime(2023, 5, 31), sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_DateRange_Empty()
    {
      DateRange dtr = DateRange.Empty;
      DateRangeFilter sut = new DateRangeFilter("F1", dtr);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression);
      Assert.IsNull(sut.MinValue, "MinValue");
      Assert.IsNull(sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxFilterDegeneration.AlwaysTrue, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_Exception()
    {
      DBxExpression exprNull = null;
      Assert.Catch<ArgumentNullException>(delegate () { new DateRangeFilter(exprNull, new DateTime(2023, 5, 29), new DateTime(2023, 5, 29)); }, "Expression=null");
      Assert.Catch<ArgumentNullException>(delegate () { new DateRangeFilter("", new DateTime(2023, 5, 29), new DateTime(2023, 5, 29)); }, "ColumnName is empty");

      Assert.Catch<ArgumentException>(delegate () { new DateRangeFilter("F1", DateTime.MaxValue.Year + 1); }, "Year");
      Assert.Catch<ArgumentException>(delegate () { new DateRangeFilter("F1", 2023, 13); }, "Month");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr = new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("F1"), new DBxColumn("F2"));
      DateRangeFilter sut = new DateRangeFilter(expr, new DateTime(2023, 5, 29), new DateTime(2023, 5, 30));

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, lst);
    }

    #endregion

    #region TestFilter()

    // Закрытые интервалы
    [TestCase("2023-05-29", "2023-05-30", "2023-05-28", false)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-29", true)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-30", true)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-31", false)]
    // Полуоткрытые интервалы
    [TestCase("2023-05-29", "", "2023-05-28", false)]
    [TestCase("2023-05-29", "", "2023-05-29", true)]
    [TestCase("", "2023-05-29", "2023-05-29", true)]
    [TestCase("", "2023-05-29", "2023-05-30", false)]
    [TestCase("", "", "2023-05-29", true)]
    // Значение поля NULL
    [TestCase("2023-05-29", "2023-05-30", "", false)]
    [TestCase("2023-05-29", "", "", false)]
    [TestCase("", "2023-05-29", "", false)]
    // Значение поля содержит время
    [TestCase("2023-05-29", "2023-05-29", "2023-05-28T23:59:59", false)]
    [TestCase("2023-05-29", "2023-05-29", "2023-05-29T00:00:01", true)]
    [TestCase("2023-05-29", "2023-05-29", "2023-05-29T23:59:59", true)]
    [TestCase("2023-05-29", "2023-05-29", "2023-05-30T00:00:01", false)]
    public void TestFilter(string sMinValue, string sMaxValue, string sColValue, bool wantedRes)
    {
      DateTime? minValue = null;
      if (sMinValue.Length > 0) minValue = StdConvert.ToDateTime(sMinValue, false);
      DateTime? maxValue = null;
      if (sMaxValue.Length > 0) maxValue = StdConvert.ToDateTime(sMaxValue, false);
      DateTime? colValue = null;
      if (sColValue.Length > 0) colValue = StdConvert.ToDateTime(sColValue, true);

      DateRangeFilter sut = new DateRangeFilter("F1", minValue, maxValue);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }


    [TestCase("2023-05-29", "2023-05-30", "2023-05-28", false)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-29", true)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-30", true)]
    [TestCase("2023-05-29", "2023-05-30", "2023-05-31", false)]
    [TestCase("2023-05-29", "", "2023-05-28", false)]
    [TestCase("2023-05-29", "", "2023-05-29", true)]
    [TestCase("", "2023-05-29", "2023-05-29", true)]
    [TestCase("", "2023-05-29", "2023-05-30", false)]
    [TestCase("", "", "2023-05-29", true)]
    [TestCase("2023-05-29", "2023-05-30", "", false)]
    [TestCase("2023-05-29", "", "", false)]
    [TestCase("", "2023-05-29", "", false)]
    public void TestFilter_WithTime(string sMinValue, string sMaxValue, string sColValue, bool wantedRes)
    {
      DateTime? minValue = null;
      if (sMinValue.Length > 0) minValue = StdConvert.ToDateTime(sMinValue, false);
      DateTime? maxValue = null;
      if (sMaxValue.Length > 0) maxValue = StdConvert.ToDateTime(sMaxValue, false);
      DateTime? colValue = null;
      if (sColValue.Length > 0) colValue = StdConvert.ToDateTime(sColValue, true);

      DateRangeFilter sut = new DateRangeFilter("F1", minValue, maxValue);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DateRangeFilter sut = new DateRangeFilter("F1", 2023, 5);
      byte[] b = SerializationTools.SerializeBinary(sut);
      DateRangeFilter res = (DateRangeFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
