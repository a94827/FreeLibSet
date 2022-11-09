using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace ExtTools_tests.Calendar
{
  [TestFixture]
  public class DateRangeTests
  {
    [Test]
    public void Constructor_simple()
    {
      DateRange sut = new DateRange(new DateTime(2021, 1, 1), new DateTime(2021, 12, 31), "123");

      Assert.AreEqual(new DateTime(2021, 1, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 31), sut.LastDate, "LastDate");
      Assert.AreEqual("123", sut.Tag);
      Assert.IsFalse(sut.IsEmpty);
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch<ArgumentException>(delegate() { new DateRange(new DateTime(2021, 1, 2), new DateTime(2021, 1, 1)); });
    }


    [Test]
    public void Constructor_year()
    {
      DateRange sut = new DateRange(2021);
      Assert.AreEqual(new DateTime(2021, 1, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 31), sut.LastDate, "LastDate");
    }

    [Test]
    public void Constructor_month()
    {
      DateRange sut = new DateRange(2021, 7);
      Assert.AreEqual(new DateTime(2021, 7, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 7, 31), sut.LastDate, "LastDate");
    }

    [Test]
    public void Constructor_copy_with_tag()
    {
      DateRange r1 = new DateRange(2021);
      DateRange sut = new DateRange(r1, "123");

      Assert.AreEqual(new DateTime(2021, 1, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 31), sut.LastDate, "LastDate");
      Assert.AreEqual("123", sut.Tag);
    }

    [Test]
    public void Constructor_half_open()
    {
      DateRange sut1 = new DateRange(null, new DateTime(2021, 8, 16));
      Assert.AreEqual(DateRange.Whole.FirstDate, sut1.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 8, 16), sut1.LastDate, "LastDate");

      DateRange sut2 = new DateRange(new DateTime(2021, 8, 16), null);
      Assert.AreEqual(new DateTime(2021, 8, 16), sut2.FirstDate, "FirstDate");
      Assert.AreEqual(DateRange.Whole.LastDate, sut2.LastDate, "LastDate");

      DateRange sut3 = new DateRange(null, null);
      Assert.AreEqual(DateRange.Whole.FirstDate, sut3.FirstDate, "FirstDate");
      Assert.AreEqual(DateRange.Whole.LastDate, sut3.LastDate, "LastDate");
    }

    [Test]
    public void Empty()
    {
      Assert.IsTrue(DateRange.Empty.IsEmpty, "IsEmpty");
      Assert.AreEqual(0, DateRange.Empty.Days, "Days");
      Assert.AreEqual(0, DateRange.Empty.Months, "Months");
      Assert.AreEqual(0, DateRange.Empty.Years, "Years");
      Assert.AreEqual(0, DateRange.Empty.SimpleMonths, "SimpleMonths");
      Assert.AreEqual(0, DateRange.Empty.SimpleYears, "SimpleYears");
      Assert.IsNull(DateRange.Empty.Tag, "Tag");

      Assert.IsTrue(DateRange.Empty.YMRange.IsEmpty, "YMRange.IsEmpty");
      Assert.IsFalse(DateRange.Empty.MinMaxDate.HasValue, "MinMaxDate.HasValue");
      Assert.IsFalse(DateRange.Empty.MinMaxYear.HasValue, "MinMaxYear.HasValue");

      int cnt = 0;
      foreach (DateTime dt in DateRange.Empty)
        cnt++;
      Assert.AreEqual(0, cnt, "GetEnumerator() count");
    }

    [Test]
    public void Whole()
    {
      Assert.IsFalse(DateRange.Whole.IsEmpty, "IsEmpty");
      Assert.IsTrue(DateRange.Whole.AreWholeMonths, "AreWholeMonths");
      Assert.IsTrue(DateRange.Whole.AreWholeYears, "AreWholeYears");

      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddDays(1); }, "AddDays(+1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddDays(-1); }, "AddDays(-1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddMonths(1); }, "AddMonths(+1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddMonths(-1); }, "AddMonths(-1)");
    }

    [Test]
    public void Days()
    {
      DateRange sut = new DateRange(2021);
      Assert.AreEqual(365, sut.Days);
    }

    [TestCase("20210101", "20211231", Result = 12)]
    [TestCase("20210102", "20211231", Result = 11)]
    [TestCase("20210101", "20211230", Result = 11)]
    [TestCase("20210101", "20220101", Result = 12)]
    [TestCase("20200201", "20200228", Result = 0)]
    [TestCase("20200201", "20200229", Result = 1)]
    [TestCase("20210201", "20210228", Result = 1)]
    [TestCase("20220101", "20220228", Result = 2)]
    public int Months(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.Months;
    }

    [TestCase("20220101", "20221231",Result =1)]
    [TestCase("20220102", "20221231", Result = 0)]
    public int Years(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.Years;
    }

    [TestCase("20220101", "20220228", Result = 2)]
    [TestCase("20220101", "20220101", Result = 0, Description="Minimum possible 1-day range")]
    [TestCase("20220115", "20220213", Result = 0)]
    [TestCase("20220115", "20220214", Result = 1)]
    public int SimpleMonths(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.SimpleMonths;
    }

    [TestCase("20220101", "20221231", Result = 1)]
    [TestCase("20220101", "20221230", Result = 0)]
    [TestCase("20220115", "20230114", Result = 1)]
    [TestCase("20220115", "20230113", Result = 0)]
    public int SimpleYears(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.SimpleYears;
    }


    [TestCase("20190101", "20210228", Result = true)]
    [TestCase("20190101", "20200228", Result = false)]
    public bool AreWholeMonths(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.AreWholeMonths;
    }


    [TestCase("20190101", "20211231", Result = true)]
    [TestCase("20190101", "20211230", Result = false)]
    [TestCase("20190201", "20200131", Result = false)]
    public bool AreWholeYears(string sDate1, string sDate2)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.AreWholeYears;
    }

    [Test]
    public void YMRange()
    {
      DateRange sut = new DateRange(2021);
      Assert.AreEqual(new YearMonthRange(2021, 1, 12), sut.YMRange);
    }

    [Test]
    public void MinMaxDate()
    {
      DateRange sut = new DateRange(2021);
      MinMax<DateTime> wanted = new MinMax<DateTime>(new DateTime(2021, 1, 1), new DateTime(2021, 12, 31));
      Assert.AreEqual(wanted, sut.MinMaxDate);
    }

    [Test]
    public void MinMaxYear()
    {
      DateRange sut = new DateRange(2021);
      MinMax<Int32> wanted = new MinMax<Int32>(2021, 2021);
      Assert.AreEqual(wanted, sut.MinMaxYear);
    }

    [TestCase("20210101", "20211231", "20210101", Result = true)]
    [TestCase("20210101", "20211231", "20201231", Result = false)]
    [TestCase("20210101", "20211231", "20211231", Result = true)]
    [TestCase("20210101", "20211231", "20220101", Result = false)]
    public bool Contains(string sDate1, string sDate2, string sTest)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      return sut.Contains(CreateDate(sTest));
    }

    [TestCase("20210101", "20211231", "20210101", "20210101")]
    [TestCase("20210101", "20211231", "20201231", "20210101")]
    [TestCase("20210101", "20211231", "20211231", "20211231")]
    [TestCase("20210101", "20211231", "20220101", "20211231")]
    public void DateToRange(string sDate1, string sDate2, string sTest, string sRes)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      DateTime dt = CreateDate(sTest);
      sut.DateToRange(ref dt);

      Assert.AreEqual(CreateDate(sRes), dt);
    }

    [Test]
    public void DateToRange_empty()
    {
      DateTime dt1 = new DateTime(2021, 8, 16);
      DateTime dt2 = dt1;
      DateRange.Empty.DateToRange(ref dt2);
      Assert.AreEqual(dt1, dt2);
    }

    [TestCase("20210101", "20211231", "20200101", "20200131", "20200101", "20211231")]
    [TestCase("20200101", "20200131", "20210101", "20211231", "20200101", "20211231")]
    [TestCase("20210101", "20211231", "20210201", "20210228", "20210101", "20211231")]
    public void Operator_Add_Range(string sDate1, string sDate2, string sDate3, string sDate4, string sDate5, string sDate6)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange arg = new DateRange(CreateDate(sDate3), CreateDate(sDate4), "456");
      DateRange res1 = sut + arg;
      Assert.AreEqual(CreateDate(sDate5), res1.FirstDate, "FirstDate");
      Assert.AreEqual(CreateDate(sDate6), res1.LastDate, "LastDate");
      Assert.AreEqual("123", res1.Tag, "Tag");

      // Такой же результат дает оператор "|"
      DateRange res2 = sut | arg;
      Assert.AreEqual(res1, res2, "Operator |");
    }

    [Test]
    public void Operator_Add_Range_Empty_1()
    {
      DateRange sut = new DateRange(2021);
      DateRange res = sut + DateRange.Empty;

      Assert.AreEqual(sut.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(sut.LastDate, res.LastDate, "LastDate");
    }

    [Test]
    public void Operator_Add_Range_Empty_2()
    {
      DateRange arg = new DateRange(2021);
      DateRange res = DateRange.Empty + arg;

      Assert.AreEqual(arg.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(arg.LastDate, res.LastDate, "LastDate");
    }

    [Test]
    public void Operator_Add_Days()
    {
      DateRange sut = new DateRange(2021);
      DateRange res = sut + 3;
      Assert.AreEqual(new DateTime(2021, 1, 4), res.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2022, 1, 3), res.LastDate, "LastDate");
    }

    [Test]
    public void Operator_Substract_Days()
    {
      DateRange sut = new DateRange(2021);
      DateRange res = sut - 3;
      Assert.AreEqual(new DateTime(2020, 12, 29), res.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 28), res.LastDate, "LastDate");
    }

    [TestCase("20190101", "20191231", 2, "20190301", "20200229")]
    [TestCase("20190101", "20191225", 2, "20190301", "20200225")]
    [TestCase("20190101", "20191231", 0, "20190101", "20191231")]
    [TestCase("20190301", "20200229", -2, "20190101", "20191231")]
    [TestCase("20190301", "20200225", -2, "20190101", "20191225")]
    public void AddMonths(string sDate1, string sDate2, int months, string sDate3, string sDate4)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange res = sut.AddMonths(months);
      Assert.AreEqual(CreateDate(sDate3), res.FirstDate, "FirstDate");
      Assert.AreEqual(CreateDate(sDate4), res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    [TestCase("20210101", "20210228", 2, "20210501", "20210630")]
    [TestCase("20210101", "20210228", 0, "20210101", "20210228")]
    [TestCase("20210101", "20210228", -2, "20200901", "20201031")]
    [TestCase("20210101", "20210105", 2, "20210111", "20210115")]
    [TestCase("20210101", "20210105", -2, "20201222", "20201226")]
    public void Operator_Shr(string sDate1, string sDate2, int rangeCount, string sDate3, string sDate4)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange res = sut >> rangeCount;
      Assert.AreEqual(CreateDate(sDate3), res.FirstDate, "FirstDate");
      Assert.AreEqual(CreateDate(sDate4), res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    [TestCase("20210101", "20210228", 2, "20200901", "20201031")]
    [TestCase("20210101", "20210228", 0, "20210101", "20210228")]
    [TestCase("20210101", "20210228", -2, "20210501", "20210630")]
    [TestCase("20210101", "20210105", 2, "20201222", "20201226")]
    [TestCase("20210101", "20210105", -2, "20210111", "20210115")]
    public void Operator_Shl(string sDate1, string sDate2, int rangeCount, string sDate3, string sDate4)
    {
      DateRange sut = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange res = sut << rangeCount;
      Assert.AreEqual(CreateDate(sDate3), res.FirstDate, "FirstDate");
      Assert.AreEqual(CreateDate(sDate4), res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    [TestCase("20210101", "20210331", "20210201", "20210430", "20210201", "20210331")]
    [TestCase("20210101", "20210331", "20210201", "20210228", "20210201", "20210228")]
    public void GetCross_true(string sDate1, string sDate2, string sDate3, string sDate4, string sDate5, string sDate6)
    {
      DateRange sut1 = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange sut2 = new DateRange(CreateDate(sDate3), CreateDate(sDate4), "456");

      Assert.IsTrue(DateRange.IsCrossed(sut1, sut2), "IsCrossed");

      DateRange res1 = DateRange.GetCross(sut1, sut2);
      Assert.AreEqual(CreateDate(sDate5), res1.FirstDate, "FirstDate");
      Assert.AreEqual(CreateDate(sDate6), res1.LastDate, "LastDate");
      Assert.AreEqual("123", res1.Tag, "Tag");

      DateRange res2 = sut1 & sut2;
      Assert.AreEqual(res1, res2, "operator &");
    }

    [TestCase("20210101", "20211231", "20200101", "20201231")]
    [TestCase("20200101", "20201231", "20210101", "20211231")]
    public void GetCross_false(string sDate1, string sDate2, string sDate3, string sDate4)
    {
      DateRange sut1 = new DateRange(CreateDate(sDate1), CreateDate(sDate2), "123");
      DateRange sut2 = new DateRange(CreateDate(sDate3), CreateDate(sDate4), "456");

      Assert.IsFalse(DateRange.IsCrossed(sut1, sut2), "IsCrossed");

      DateRange res1 = DateRange.GetCross(sut1, sut2);
      Assert.IsTrue(res1.IsEmpty, "IsEmpty");

      DateRange dummy;
      Assert.Catch<Exception>(delegate() { dummy = sut1 & sut2; }, "operator &");
    }

    [Test]
    public void SplitIntoYears()
    {
      DateRange sut = new DateRange(new DateTime(2019,5,1), new DateTime(2021,3,31));
      DateRangeList res = sut.SplitIntoYears();
      Assert.AreEqual(3, res.Count, "Count");
      Assert.AreEqual(new DateTime(2019, 5, 1), res[0].FirstDate, "FirstDate[0]");
      Assert.AreEqual(new DateTime(2019, 12, 31), res[0].LastDate, "LastDate[0]");
      Assert.AreEqual(new DateTime(2020, 1, 1), res[1].FirstDate, "FirstDate[1]");
      Assert.AreEqual(new DateTime(2020, 12, 31), res[1].LastDate, "LastDate[1]");
      Assert.AreEqual(new DateTime(2021, 1, 1), res[2].FirstDate, "FirstDate[2]");
      Assert.AreEqual(new DateTime(2021, 3, 31), res[2].LastDate, "LastDate[2]");
    }

    [Test]
    public void SplitIntoMonths()
    {
      DateRange sut = new DateRange(new DateTime(2019, 5, 1), new DateTime(2021, 3, 31));
      DateRangeList res = sut.SplitIntoMonths();
      Assert.AreEqual(23, res.Count, "Count");
    }

    [Test]
    public void GetEnumerator()
    {
      DateRange sut = new DateRange(2021, 3);
      int cnt = 0;
      foreach (DateTime dt in sut)
      {
        Assert.AreEqual(sut.FirstDate.AddDays(cnt), dt);
        cnt++;
      }
      Assert.AreEqual(31, cnt, "Count");
    }

    #region Вспомогательные методы

    public static DateTime CreateDate(string s)
    {
      return DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    #endregion
  }
}
