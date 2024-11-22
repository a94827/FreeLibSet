using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.Tests;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Calendar
{
  [TestFixture]
  public class DateRangeTests
  {
    #region Конструкторы

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

    #endregion

    #region Empty, Whole

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
      Assert.IsNull(DateRange.Whole.Tag, "Tag");

      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddDays(1); }, "AddDays(+1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddDays(-1); }, "AddDays(-1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddMonths(1); }, "AddMonths(+1)");
      Assert.Catch<ArgumentException>(delegate() { DateRange.Whole.AddMonths(-1); }, "AddMonths(-1)");
    }

    #endregion

    #region Days, Months, Years

    [Test]
    public void Days()
    {
      DateRange sut = new DateRange(2021);
      Assert.AreEqual(365, sut.Days);
    }

    [TestCase("20210101-20211231", Result = 12)]
    [TestCase("20210102-20211231", Result = 11)]
    [TestCase("20210101-20211230", Result = 11)]
    [TestCase("20210101-20220101", Result = 12)]
    [TestCase("20200201-20200228", Result = 0)]
    [TestCase("20200201-20200229", Result = 1)]
    [TestCase("20210201-20210228", Result = 1)]
    [TestCase("20220101-20220228", Result = 2)]
    public int Months(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.Months;
    }

    [TestCase("20220101-20221231",Result =1)]
    [TestCase("20220102-20221231", Result = 0)]
    public int Years(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.Years;
    }

    [TestCase("20220101-20220228", Result = 2)]
    [TestCase("20220101-20220101", Result = 0, Description="Minimum possible 1-day range")]
    [TestCase("20220115-20220213", Result = 0)]
    [TestCase("20220115-20220214", Result = 1)]
    public int SimpleMonths(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.SimpleMonths;
    }

    [TestCase("20220101-20221231", Result = 1)]
    [TestCase("20220101-20221230", Result = 0)]
    [TestCase("20220115-20230114", Result = 1)]
    [TestCase("20220115-20230113", Result = 0)]
    public int SimpleYears(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.SimpleYears;
    }

    #endregion

    #region AreWholeMonths/Years()


    [TestCase("20190101-20210228", Result = true)]
    [TestCase("20190101-20200228", Result = false)]
    public bool AreWholeMonths(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.AreWholeMonths;
    }


    [TestCase("20190101-20211231", Result = true)]
    [TestCase("20190101-20211230", Result = false)]
    [TestCase("20190201-20200131", Result = false)]
    public bool AreWholeYears(string sSUT)
    {
      DateRange sut = Creators.DateRange(sSUT);
      return sut.AreWholeYears;
    }

    #endregion

    #region YMRange

    [Test]
    public void YMRange()
    {
      DateRange sut = new DateRange(2021);
      Assert.AreEqual(new YearMonthRange(2021, 1, 12), sut.YMRange);
    }

    #endregion

    #region MinMaxDate/Year

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

    #endregion

    #region Contains()

    [TestCase("20210101-20211231", "20210101", true)]
    [TestCase("20210101-20211231", "20201231", false)]
    [TestCase("20210101-20211231", "20211231", true)]
    [TestCase("20210101-20211231", "20220101", false)]
    public void Contains(string sSUT, string sDt, bool wantedRes)
    {
      DateRange sut = Creators.DateRange(sSUT);
      DateTime test = Creators.DateTime(sDt);
      Assert.AreEqual(wantedRes, sut.Contains(test));
    }

    #endregion

    #region DateToRange()

    [TestCase("20210101-20211231", "20210101", "20210101")]
    [TestCase("20210101-20211231", "20201231", "20210101")]
    [TestCase("20210101-20211231", "20211231", "20211231")]
    [TestCase("20210101-20211231", "20220101", "20211231")]
    public void DateToRange(string sDTR, string sDt, string sWantedRes)
    {
      DateRange sut = Creators.DateRange(sDTR);
      DateTime dt = Creators.DateTime(sDt);
      DateTime wantedRes = Creators.DateTime(sWantedRes);
      sut.DateToRange(ref dt);

      Assert.AreEqual(wantedRes, dt);
    }

    [Test]
    public void DateToRange_empty()
    {
      DateTime dt1 = new DateTime(2021, 8, 16);
      DateTime dt2 = dt1;
      DateRange.Empty.DateToRange(ref dt2);
      Assert.AreEqual(dt1, dt2);
    }

    #endregion

    #region Операторы

    [TestCase("20210101-20211231", "20200101-20200131", "20200101-20211231")]
    [TestCase("20200101-20200131", "20210101-20211231", "20200101-20211231")]
    [TestCase("20210101-20211231", "20210201-20210228", "20210101-20211231")]
    public void Operator_Add_Range(string sSUT, string sArg, string sWantedRes)
    {
      DateRange sut = new DateRange(Creators.DateRange(sSUT), "123");
      DateRange arg = new DateRange(Creators.DateRange(sArg), "456");
      DateRange wantedRes = Creators.DateRange(sWantedRes);

      DateRange res1 = sut + arg;
      Assert.AreEqual(wantedRes.FirstDate, res1.FirstDate, "FirstDate");
      Assert.AreEqual(wantedRes.LastDate, res1.LastDate, "LastDate");
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

    #endregion

    #region AddMonths()

    [TestCase("20190101-20191231", 2, "20190301-20200229")]
    [TestCase("20190101-20191225", 2, "20190301-20200225")]
    [TestCase("20190101-20191231", 0, "20190101-20191231")]
    [TestCase("20190301-20200229", -2, "20190101-20191231")]
    [TestCase("20190301-20200225", -2, "20190101-20191225")]
    public void AddMonths(string sSUT, int months, string sWantedRes)
    {
      DateRange sut = new DateRange(Creators.DateRange(sSUT), "123");
      DateRange wantedRes = Creators.DateRange(sWantedRes);

      DateRange res = sut.AddMonths(months);

      Assert.AreEqual(wantedRes.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(wantedRes.LastDate, res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    #endregion

    #region Операторы сдвига

    [TestCase("20210101-20210228", 2, "20210501-20210630")]
    [TestCase("20210101-20210228", 0, "20210101-20210228")]
    [TestCase("20210101-20210228", -2, "20200901-20201031")]
    [TestCase("20210101-20210105", 2, "20210111-20210115")]
    [TestCase("20210101-20210105", -2, "20201222-20201226")]
    public void Operator_Shr(string sSUT, int rangeCount, string sWantedRes)
    {
      DateRange sut = new DateRange(Creators.DateRange(sSUT), "123");
      DateRange wantedRes = Creators.DateRange(sWantedRes);

      DateRange res = sut >> rangeCount;

      Assert.AreEqual(wantedRes.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(wantedRes.LastDate, res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    [TestCase("20210101-20210228", 2, "20200901-20201031")]
    [TestCase("20210101-20210228", 0, "20210101-20210228")]
    [TestCase("20210101-20210228", -2, "20210501-20210630")]
    [TestCase("20210101-20210105", 2, "20201222-20201226")]
    [TestCase("20210101-20210105", -2, "20210111-20210115")]
    public void Operator_Shl(string sSUT, int rangeCount, string sWantedRes)
    {
      DateRange sut = new DateRange(Creators.DateRange(sSUT), "123");
      DateRange wantedRes = Creators.DateRange(sWantedRes);

      DateRange res = sut << rangeCount;
      Assert.AreEqual(wantedRes.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(wantedRes.LastDate, res.LastDate, "LastDate");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    #endregion

    #region GetCross()

    [TestCase("20210101-20210331", "20210201-20210430", "20210201-20210331")]
    [TestCase("20210101-20210331", "20210201-20210228", "20210201-20210228")]
    public void GetCross_true(string sArg1, string sArg2, string sWantedRes)
    {
      DateRange arg1 = new DateRange(Creators.DateRange(sArg1), "123");
      DateRange arg2 = new DateRange(Creators.DateRange(sArg2), "456");
      DateRange wantedRes = Creators.DateRange(sWantedRes);

      Assert.IsTrue(DateRange.IsCrossed(arg1, arg2), "IsCrossed");

      DateRange res1 = DateRange.GetCross(arg1, arg2);
      Assert.AreEqual(wantedRes.FirstDate, res1.FirstDate, "FirstDate");
      Assert.AreEqual(wantedRes.LastDate, res1.LastDate, "LastDate");
      Assert.AreEqual("123", res1.Tag, "Tag");

      DateRange res2 = arg1 & arg2;
      Assert.AreEqual(res1, res2, "operator &");
    }

    [TestCase("20210101-20211231", "20200101-20201231")]
    [TestCase("20200101-20201231", "20210101-20211231")]
    public void GetCross_false(string sArg1, string sArg2)
    {
      DateRange arg1 = new DateRange(Creators.DateRange(sArg1), "123");
      DateRange arg2 = new DateRange(Creators.DateRange(sArg2), "456");

      Assert.IsFalse(DateRange.IsCrossed(arg1, arg2), "IsCrossed");

      DateRange res1 = DateRange.GetCross(arg1, arg2);
      Assert.IsTrue(res1.IsEmpty, "IsEmpty");

      DateRange dummy;
      Assert.Catch<Exception>(delegate() { dummy = arg1 & arg2; }, "operator &");
    }

    #endregion

    #region SplitIntoYears/Months()

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

    #endregion

    #region Age

    [TestCase("", "0:0:0")]
    [TestCase("20241105-20241204", "0:1:0")]
    [TestCase("20240201-20240229", "0:1:0")]
    [TestCase("20240201-20240228", "0:0:28")]
    [TestCase("20240229-20240229", "0:0:1")]
    [TestCase("00010101-99991231", "9999:0:0")]
    public void Age(string sSUT, string sWanted)
    {
      DateRange sut = Creators.DateRange(sSUT);
      YearMonthDayAge wanted = Creators.YearMonthDayAge(sWanted);

      YearMonthDayAge res = sut.Age;
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region GetEnumerator()

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

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DateRange sut = new DateRange(new DateTime(2023,1,1), new DateTime(2023,1,29), "123");

      byte[] b = SerializationTools.SerializeBinary(sut);
      DateRange res = (DateRange)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.FirstDate, res.FirstDate, "FirstDate");
      Assert.AreEqual(sut.LastDate, res.LastDate, "LastDate");
      Assert.AreEqual(sut.Tag, res.Tag, "Tag");
    }

    #endregion
  }
}
