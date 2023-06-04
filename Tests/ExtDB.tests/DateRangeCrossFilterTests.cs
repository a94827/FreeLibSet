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

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DateRangeCrossFilterTests
  {
    #region Конструкторы

    [TestCase("2023-05-30", "2023-05-31", "2023-05-30", "2023-05-31", DBxFilterDegeneration.None)]
    [TestCase("2023-05-30", "", "2023-05-30", "", DBxFilterDegeneration.None)]
    [TestCase("", "2023-05-31", "", "2023-05-31", DBxFilterDegeneration.None)]
    [TestCase("", "", "", "", DBxFilterDegeneration.AlwaysTrue)]
    [TestCase("2023-05-31", "2023-05-30", "2023-05-31", "2023-05-30", DBxFilterDegeneration.None, Description ="Invalid date range makes no degeneration")]
    [TestCase("0001-01-01", "2023-05-31", "", "2023-05-31", DBxFilterDegeneration.None)]
    [TestCase("2023-05-30", "9999-12-31", "2023-05-30", "", DBxFilterDegeneration.None)]
    [TestCase("0001-01-01", "9999-12-31", "", "", DBxFilterDegeneration.AlwaysTrue)]
    public void Constructor_Expression(string sFirstDate, string sLastDate, string sWantedFirstDate, string sWantedLastDate, DBxFilterDegeneration wantedDegeneration)
    {
      DateTime? firstDate = null;
      if (sFirstDate.Length > 0) firstDate = StdConvert.ToDateTime(sFirstDate, false);
      DateTime? lastDate = null;
      if (sLastDate.Length > 0) lastDate = StdConvert.ToDateTime(sLastDate, false);
      DateTime? wantedFirstDate = null;
      if (sWantedFirstDate.Length > 0) wantedFirstDate = StdConvert.ToDateTime(sWantedFirstDate, false);
      DateTime? wantedLastDate = null;
      if (sWantedLastDate.Length > 0) wantedLastDate = StdConvert.ToDateTime(sWantedLastDate, false);

      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      DateRangeCrossFilter sut = new DateRangeCrossFilter(expr1, expr2, firstDate, lastDate);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(wantedFirstDate, sut.FirstDate, "FirstDate");
      Assert.AreEqual(wantedLastDate, sut.LastDate, "LastDate");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName()
    {
      DateRangeCrossFilter sut = new DateRangeCrossFilter("F1", "F2", new DateTime(2023, 5, 30), new DateTime(2023, 5, 31));
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName 1");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreEqual("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName 2");
      Assert.AreEqual(new DateTime(2023, 5, 30), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2023, 5, 31), sut.LastDate, "LastDate");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_DateRange()
    {
      DateRangeCrossFilter sut = new DateRangeCrossFilter("F1", "F2", new DateRange(2023, 5));
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName 1");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreEqual("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName 2");
      Assert.AreEqual(new DateTime(2023, 5, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2023, 5, 31), sut.LastDate, "LastDate");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_DateRange_Empty()
    {
      DateRangeCrossFilter sut = new DateRangeCrossFilter("F1", "F2", DateRange.Empty);
      Assert.IsNull(sut.FirstDate, "FirstDate");
      Assert.IsNull(sut.LastDate, "LastDate");
      Assert.AreEqual(DBxFilterDegeneration.AlwaysTrue, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      DateRangeCrossFilter sut = new DateRangeCrossFilter(expr1, expr2, new DateTime(2023, 5, 30), new DateTime(2023, 5, 31));

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("2023-05-30", "2023-05-31", "2023-05-28", "2023-05-29", false)]
    [TestCase("2023-05-30", "2023-05-31", "2023-05-28", "2023-05-30", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-05-31", "2023-06-01", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-06-01", "2023-06-01", false)]
    [TestCase("2023-05-30", "", "2023-05-28", "2023-05-29", false)]
    [TestCase("2023-05-30", "", "2023-05-28", "2023-05-30", true)]
    [TestCase("", "2023-05-28", "2023-05-28", "2023-05-29", true)]
    [TestCase("", "2023-05-27", "2023-05-28", "2023-05-29", false)]
    [TestCase("2023-05-30", "2023-05-31", "", "2023-05-29", false)]
    [TestCase("2023-05-30", "2023-05-31", "", "2023-05-30", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-05-31", "", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-06-01", "", false)]
    [TestCase("", "", "2023-05-28", "2023-05-29", true)]
    [TestCase("2023-05-30", "2023-05-31", "", "", true)]
    [TestCase("", "", "", "", true)]
    // Неправильные интервалы - поведение зависит от реализации
    //[TestCase("2023-05-31", "2023-05-30", "2023-05-30", "2023-05-31", false)]
    //[TestCase("2023-05-30", "2023-05-31", "2023-05-31", "2023-05-30", false)]
    // Комбинации полуоткрытых и неправильных интервалов не определены.
    public void TestFilter(string sColValue1, string sColValue2, string sFirstDate, string sLastDate, bool wantedRes)
    {
      DateTime? colValue1 = null;
      if (sColValue1.Length > 0) colValue1 = StdConvert.ToDateTime(sColValue1, false);
      DateTime? colValue2 = null;
      if (sColValue2.Length > 0) colValue2 = StdConvert.ToDateTime(sColValue2, false);
      DateTime? firstDate = null;
      if (sFirstDate.Length > 0) firstDate = StdConvert.ToDateTime(sFirstDate, false);
      DateTime? lastDate = null;
      if (sLastDate.Length > 0) lastDate = StdConvert.ToDateTime(sLastDate, false);

      DateRangeCrossFilter sut = new DateRangeCrossFilter("F1", "F2", firstDate, lastDate);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue1);
      vals.Add("F2", colValue2);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DateRangeCrossFilter sut = new DateRangeCrossFilter("F1", "F2", new DateTime(2023, 5, 30), new DateTime(2023, 5, 31));
      byte[] b = SerializationTools.SerializeBinary(sut);
      DateRangeCrossFilter res = (DateRangeCrossFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
