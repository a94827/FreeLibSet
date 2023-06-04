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
  public class DateRangeInclusionFilterTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Expression()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      DateRangeInclusionFilter sut = new DateRangeInclusionFilter(expr1, expr2, new DateTime(2023, 5, 30));
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(new DateTime(2023, 5, 30), sut.Value, "Value");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName()
    {
      DateRangeInclusionFilter sut = new DateRangeInclusionFilter("F1", "F2", new DateTime(2023, 5, 30));
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName 1");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreEqual("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName 2");
      Assert.AreEqual(new DateTime(2023, 5, 30), sut.Value, "Value");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      DateRangeInclusionFilter sut = new DateRangeInclusionFilter(expr1, expr2, new DateTime(2023, 5, 30));

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("2023-05-30", "2023-05-31", "2023-05-29", false)]
    [TestCase("2023-05-30", "2023-05-31", "2023-05-30", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-05-31", true)]
    [TestCase("2023-05-30", "2023-05-31", "2023-06-01", false)]

    [TestCase("2023-05-30", "", "2023-05-29", false)]
    [TestCase("2023-05-30", "", "2023-05-30", true)]
    [TestCase("", "2023-05-30", "2023-05-30", true)]
    [TestCase("", "2023-05-30", "2023-05-31", false)]
    [TestCase("", "", "2023-05-30", true)]
    public void TestFilter(string sColValue1, string sColValue2, string sFilterValue, bool wantedRes)
    {
      DateTime? colValue1 = null;
      if (sColValue1.Length > 0) colValue1 = StdConvert.ToDateTime(sColValue1, false);
      DateTime? colValue2 = null;
      if (sColValue2.Length > 0) colValue2 = StdConvert.ToDateTime(sColValue2, false);
      DateTime filterValue = StdConvert.ToDateTime(sFilterValue, true);

      DateRangeInclusionFilter sut = new DateRangeInclusionFilter("F1", "F2", filterValue);

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
      DateRangeInclusionFilter sut = new DateRangeInclusionFilter("F1", "F2", new DateTime(2023, 5, 30));
      byte[] b = SerializationTools.SerializeBinary(sut);
      DateRangeInclusionFilter res = (DateRangeInclusionFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
