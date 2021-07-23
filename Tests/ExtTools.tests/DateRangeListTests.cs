using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV;
using System.Globalization;

namespace ExtTools.tests
{
  [TestFixture]
  class DateRangeListTests
  {
    [Test]
    public void Constructor()
    {
      DateRangeList sut = new DateRangeList();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsNull(sut.FirstDate, "FirstDate");
      Assert.IsNull(sut.LastDate, "LastDate");
      Assert.AreEqual(0, sut.Days, "Days");
      Assert.IsFalse(sut.MinMaxDate.HasValue, "MinMaxDate.HasValue");
      Assert.IsFalse(sut.MinMaxYear.HasValue, "MinMaxYear.HasValue");

      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, DateRangeList.Empty.Count, "Count");
      Assert.IsTrue(DateRangeList.Empty.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Whole()
    {
      Assert.AreEqual(1, DateRangeList.Whole.Count, "Count");
      Assert.AreEqual(DateTime.MinValue, DateRangeList.Whole.FirstDate);
      Assert.AreEqual(DateTime.MaxValue.Date, DateRangeList.Whole.LastDate);
      Assert.IsTrue(DateRangeList.Whole.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Append_first()
    {
      DateRangeList sut = new DateRangeList();

      sut.Append(new DateRange(2021));

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(new DateTime(2021, 1, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 31), sut.LastDate, "LastDate");
      Assert.AreEqual(365, sut.Days, "Days");
      Assert.IsTrue(sut.MinMaxDate.HasValue, "MinMaxDate.HasValue");
      Assert.IsTrue(sut.MinMaxYear.HasValue, "MinMaxYear.HasValue");
    }

    [Test]
    public void Append_second()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2019));

      sut.Append(new DateRange(2021));

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(new DateTime(2019, 1, 1), sut.FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2021, 12, 31), sut.LastDate, "LastDate");
      Assert.AreEqual(365 + 365, sut.Days, "Days");
      Assert.IsTrue(sut.MinMaxDate.HasValue, "MinMaxDate.HasValue");
      Assert.IsTrue(sut.MinMaxYear.HasValue, "MinMaxYear.HasValue");
    }

    [Test]
    public void Append_exception_crossed()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2019));

      DateRange r = new DateRange(new DateTime(2019, 12, 31), new DateTime(2020, 1, 1));
      Assert.Catch<Exception>(delegate() { sut.Append(r); });
    }


    [Test]
    public void SetReadOnly()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2021));

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(1, sut.Count, "Count");
    }

    [Test]
    public void IsReadOnly_exceptions()
    {
      DateRangeList sut = new DateRangeList();
      sut.SetReadOnly();

      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Append(new DateRange(2021)); }, "Append()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Clear(); }, "Clear()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Add(new DateRange(2021)); }, "Add(DateRange)");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Remove(new DateRange(2021)); }, "Remove(DateRange)");

      DateRangeList list2 = new DateRangeList();
      list2.Append(new DateRange(2021));
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Add(list2); }, "Add(DateRangeList)");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Remove(list2); }, "Remove(DateRangeList)");

      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Split(new DateTime(2021, 7, 1)); }, "Split(DateTime)");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Split(new DateTime[] { new DateTime(2021, 7, 1) }); }, "Split(DateTime[])");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SplitIntoYears(); }, "SplitIntoYears()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SplitIntoMonths(); }, "SplitIntoMonths()");

      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Merge(); }, "Merge()");
    }

    [Test]
    public void Clear()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2021));

      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    [TestCase("20181231", Result = -1)]
    [TestCase("20190101", Result = 0)]
    [TestCase("20191231", Result = 0)]
    [TestCase("20200101", Result = -1)]
    [TestCase("20201231", Result = -1)]
    [TestCase("20210101", Result = 1)]
    [TestCase("20211231", Result = 1)]
    [TestCase("20220101", Result = -1)]
    public int IndexOf(string sDate)
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2019));
      sut.Append(new DateRange(2021));

      return sut.IndexOf(CreateDate(sDate));
    }

    [Test]
    public void Merge_ok()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2019));
      sut.Append(new DateRange(2020));
      Assert.AreEqual(2, sut.Count, "Count before");

      sut.Merge();

      Assert.AreEqual(1, sut.Count, "Count after");
      Assert.AreEqual(new DateTime(2019, 1, 1), sut[0].FirstDate, "FirstDate");
      Assert.AreEqual(new DateTime(2020, 12, 31), sut[0].LastDate, "LastDate");
    }

    [Test]
    public void Merge_none()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2019));
      sut.Append(new DateRange(new DateTime(2020, 1, 2), new DateTime(2020, 12, 31)));
      Assert.AreEqual(2, sut.Count, "Count before");

      sut.Merge();

      Assert.AreEqual(2, sut.Count, "Count after");
    }


    [TestCase("20200201", "20200229", "20190101-20191231,20200201-20200229,20210101-20211231")]
    [TestCase("20210201", "20210228", "20190101-20191231,20210101-20211231")]
    [TestCase("20180201", "20180228", "20180201-20180228,20190101-20191231,20210101-20211231")]
    [TestCase("20191101", "20200228", "20190101-20200228,20210101-20211231")]
    [TestCase("20200101", "20201231", "20190101-20211231")]
    public void Add(string sDate1, string sDate2, string result)
    {
      DateRange r = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      DateRangeList list2 = new DateRangeList();
      list2.Append(r);

      DateRangeList sut1 = new DateRangeList();
      sut1.Append(new DateRange(2019));
      sut1.Append(new DateRange(2021));
      DateRangeList sut2 = sut1.Clone();

      sut1.Add(r);
      sut1.Merge(); // иначе пересечения интервалов могут зависеть от реализации
      Assert.AreEqual(result, ToString(sut1));

      sut2.Add(list2);
      sut2.Merge(); // иначе пересечения интервалов могут зависеть от реализации
      Assert.AreEqual(result, ToString(sut2));
    }

    [TestCase("20200201", "20200229", "20190101-20191231,20210101-20211231")]
    [TestCase("20210201", "20210228", "20190101-20191231,20210101-20210131,20210301-20211231")]
    [TestCase("20201230", "20210228", "20190101-20191231,20210301-20211231")]
    [TestCase("20201230", "20211231", "20190101-20191231")]
    [TestCase("20190101", "20211231", "")]
    public void Remove(string sDate1, string sDate2, string result)
    {
      DateRange r = new DateRange(CreateDate(sDate1), CreateDate(sDate2));
      DateRangeList list2 = new DateRangeList();
      list2.Append(r);

      DateRangeList sut1 = new DateRangeList();
      sut1.Append(new DateRange(2019));
      sut1.Append(new DateRange(2021));
      DateRangeList sut2 = sut1.Clone();

      sut1.Remove(r);
      sut1.Merge(); // иначе пересечения интервалов могут зависеть от реализации
      Assert.AreEqual(result, ToString(sut1));

      sut2.Remove(list2);
      sut2.Merge(); // иначе пересечения интервалов могут зависеть от реализации
      Assert.AreEqual(result, ToString(sut2));
    }

    [TestCase("20210501", "20210101-20210501,20210502-20211231")]
    [TestCase("20210101", "20210101-20210101,20210102-20211231")]
    [TestCase("20211231", "20210101-20211231")]
    [TestCase("20201231", "20210101-20211231")]
    [TestCase("20220101", "20210101-20211231")]
    public void Split_Date(string sDate, string result)
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2021));

      sut.Split(CreateDate(sDate));
      Assert.AreEqual(result, ToString(sut));
    }

    [Test]
    public void Split_Dates()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2021));

      sut.Split(new DateTime[] { new DateTime(2021, 5, 1), new DateTime(2020, 5, 1) });


      Assert.AreEqual("20210101-20210501,20210502-20211231", ToString(sut));
    }

    [Test]
    public void SplitIntoYears()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(new DateTime(2018, 5, 1), new DateTime(2020, 3, 15)));
      sut.Append(new DateRange(new DateTime(2020, 5, 1), new DateTime(2021, 3, 15)));

      sut.SplitIntoYears();

      Assert.AreEqual("20180501-20181231,20190101-20191231,20200101-20200315,20200501-20201231,20210101-20210315",
        ToString(sut));
    }

    [Test]
    public void SplitIntoMonthes()
    {
      DateRangeList sut = new DateRangeList();
      sut.Append(new DateRange(2021));

      sut.SplitIntoMonths();

      Assert.AreEqual(12, sut.Count, "Count");
      for (int i = 0; i < 12; i++)
      {
        Assert.AreEqual(DataTools.BottomOfMonth(2021, i + 1), sut[i].FirstDate, "[0].FirstDate");
        Assert.AreEqual(DataTools.EndOfMonth(2021, i + 1), sut[i].LastDate, "[0].LastDate");
      }
    }

    #region Вспомогательные методы

    public static DateTime CreateDate(string s)
    {
      return DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    public static string ToString(DateRangeList obj)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < obj.Count; i++)
      {
        if (i > 0)
          sb.Append(',');
        sb.Append(obj[i].FirstDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        sb.Append('-');
        sb.Append(obj[i].LastDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
      }
      return sb.ToString();
    }

    #endregion
  }
}
