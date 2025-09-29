using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Tests;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class TimeToolsTests
  {
    #region Преобразования из константных строк

    /// <summary>
    /// Соответствует DateTime.MinValue
    /// </summary>
    const string MinDate = "00010101";

    /// <summary>
    /// Соответствует DateTime.MaxValue.Date
    /// </summary>
    const string MaxDate = "99991231";


    #endregion

    #region Constants()

    [Test]
    public void Constants()
    {
      Assert.AreEqual(DateTime.MinValue, Creators.DateTime(MinDate));
      Assert.AreEqual(DateTime.MaxValue.Date, Creators.DateTime(MaxDate));
    }

    #endregion

    #region DateInRange()

    [TestCase(MinDate, "", "", true)]
    [TestCase(MaxDate, "", "", true)]
    [TestCase("20210714", "20210714", "20210714", true)]
    [TestCase("20210714", "20210713", "20210713", false)]
    [TestCase("20210714", "20210715", "20210715", false)]
    [TestCase("20210714", "20210714", "", true)]
    [TestCase("20210714", "20210715", "", false)]
    [TestCase("20210714", "", "20210714", true)]
    [TestCase("20210714", "", "20210713", false)]
    public void DateInRange(string sTestDate, string sFirstDate, string sLastDate, bool wanted)
    {
      DateTime testDate = Creators.DateTime(sTestDate);
      DateTime? firstDate = Creators.NDateTime(sFirstDate);
      DateTime? lastDate = Creators.NDateTime(sLastDate);

      bool res1 = TimeTools.DateInRange(testDate, firstDate, lastDate);
      Assert.AreEqual(wanted, res1, "Nullable<DateTime>");
    }

    #endregion

    #region DateRangeInRange()

    [TestCase("20210101", "20210131", "20201201", "20210228", true)]
    [TestCase("20210101", "20210131", "20210101", "20210131", true)]
    [TestCase("20210101", "20210131", "20210102", "20210131", false)]
    [TestCase("20210101", "20210131", "20210101", "20210130", false)]
    [TestCase("20210101", "20210131", "20210101", "", true)]
    [TestCase("20210101", "20210131", "", "20210131", true)]
    [TestCase("20210101", "20210131", "20210102", "", false)]
    [TestCase("20210101", "20210131", "", "20210130", false)]
    [TestCase("20210101", "20210131", "", "", true)]
    [TestCase("", "20210131", "20201201", "20210228", false)]
    [TestCase("20210101", "", "20201201", "20210228", false)]
    [TestCase("", "", "20201201", "20210228", false)]
    [TestCase("", "20210131", "", "20210131", true)]
    [TestCase("", "20210131", "", "20210130", false)]
    [TestCase("20210101", "", "20210101", "", true)]
    [TestCase("20210101", "", "20210102", "", false)]
    [TestCase("", "", "20210101", "", false)]
    [TestCase("", "", "", "20210131", false)]
    [TestCase("", "", "", "", true)]
    public void DateRangeInRange(string sFirstDate1, string sLastDate1, string sFirstDate2, string sLastDate2, bool wanted)
    {
      DateTime? firstDate1 = Creators.NDateTime(sFirstDate1);
      DateTime? lastDate1 = Creators.NDateTime(sLastDate1);
      DateTime? firstDate2 = Creators.NDateTime(sFirstDate2);
      DateTime? lastDate2 = Creators.NDateTime(sLastDate2);

      bool res = TimeTools.DateRangeInRange(firstDate1, lastDate1, firstDate2, lastDate2);
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region DateRangeCrossed()

    [TestCase("20210701", "20210710", "20210710", "20210711", Result = true)]
    [TestCase("20210701", "20210710", "20210630", "20210701", Result = true)]
    [TestCase("20210701", "20210710", "20210711", "20210711", Result = false)]
    [TestCase("20210701", "20210710", "20210630", "20210630", Result = false)]
    [TestCase("", "", "20210714", "20210714", Result = true)]
    [TestCase("20210714", "20210714", "", "", Result = true)]
    [TestCase("", "20210714", "20210714", "", Result = true)]
    [TestCase("", "20210714", "20210715", "", Result = false)]
    [TestCase("20210714", "", "", "20210714", Result = true)]
    [TestCase("20210715", "", "", "20210714", Result = false)]
    [TestCase("", "", "", "", Result = true)]
    public bool DateRangesCrossed(string firstDate1, string lastDate1, string firstDate2, string lastDate2)
    {
      return TimeTools.DateRangesCrossed(Creators.NDateTime(firstDate1), Creators.NDateTime(lastDate1),
        Creators.NDateTime(firstDate2), Creators.NDateTime(lastDate2));
    }

    #endregion

    #region DateToRange()

    [TestCase("20210714", "20210101", "20211231", "20210714")]
    [TestCase("20210714", "20210714", "20211231", "20210714")]
    [TestCase("20210714", "20210101", "20210714", "20210714")]
    [TestCase("20210714", "20210715", "20211231", "20210715")]
    [TestCase("20210714", "20210101", "20210713", "20210713")]
    [TestCase("20210714", "20210714", "20211231", "20210714")]
    [TestCase("20210714", "", "20210714", "20210714")]
    [TestCase("20210714", "", "20210713", "20210713")]
    [TestCase("20210714", "20210714", "", "20210714")]
    [TestCase("20210714", "20210715", "", "20210715")]
    public void DateToRange(string testDate, string firstDate, string lastDate, string res)
    {
      DateTime dt = Creators.DateTime(testDate);
      TimeTools.DateToRange(ref dt,
        Creators.NDateTime(firstDate), Creators.NDateTime(lastDate));

      Assert.AreEqual(Creators.DateTime(res), dt);
    }

    #endregion

    #region GetDateRangeCross()

    [TestCase("20210714", "20210715", "20210716", "20210717", false, "", "")]
    [TestCase("20210714", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "", "20210715", "20210717", true, "20210715", "20210717")]
    [TestCase("", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "20210715", "", "", true, "20210714", "20210715")]
    public void GetDateRangeCross(string sdt11, string sdt12, string sdt21, string sdt22, bool wanted, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.NDateTime(sdt11);
      DateTime? dt12 = Creators.NDateTime(sdt12);
      DateTime? dt21 = Creators.NDateTime(sdt21);
      DateTime? dt22 = Creators.NDateTime(sdt22);

      bool res = TimeTools.GetDateRangeCross(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(wanted, res, "Result");
      Assert.AreEqual(Creators.NDateTime(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.NDateTime(sdt12res), dt12, "LastDate1");
    }

    #endregion

    #region GetDateRangeUnion()

    [TestCase("20210714", "20210715", "20210716", "20210717", "20210714", "20210717")]
    [TestCase("20210801", "20210831", "20210714", "20210714", "20210714", "20210831")]
    [TestCase("20210714", "20210715", "20210716", "", "20210714", "")]
    [TestCase("20210714", "20210715", "", "20210716", "", "20210716")]
    [TestCase("20210714", "", "", "20210716", "", "")]
    public void GetDateRangeUnion(string sdt11, string sdt12, string sdt21, string sdt22, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.NDateTime(sdt11);
      DateTime? dt12 = Creators.NDateTime(sdt12);
      DateTime? dt21 = Creators.NDateTime(sdt21);
      DateTime? dt22 = Creators.NDateTime(sdt22);

      TimeTools.GetDateRangeUnion(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(Creators.NDateTime(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.NDateTime(sdt12res), dt12, "LastDate1");
    }

    #endregion

    #region YearInRange()

    [TestCase(2021, "20211231", "20211231", Result = true)]
    [TestCase(2022, "20211231", "20211231", Result = false)]
    [TestCase(2021, "", "20210101", Result = true)]
    [TestCase(2021, "20211231", "", Result = true)]
    [TestCase(2021, "", "20201231", Result = false)]
    [TestCase(2021, "20220101", "", Result = false)]
    [TestCase(9999, "", "", Result = true)]
    public bool YearInRange(int year, string firstDate, string lastDate)
    {
      return TimeTools.YearInRange(year, Creators.NDateTime(firstDate), Creators.NDateTime(lastDate));
    }

    #endregion

    #region Bottom/EndOfYear/Quarter/Month/Week()

    [TestCase("20210101", "20210101")]
    [TestCase("20210924", "20210101")]
    [TestCase("20211231", "20210101")]
    public void BottomOfYear(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.BottomOfYear(dt);
      Assert.AreEqual(wanted, res1, "BottomOfYear(DateTime)");

      DateTime res2 = TimeTools.BottomOfYear(dt.Year);
      Assert.AreEqual(wanted, res2, "BottomOfYear(int)");

      bool res3 = TimeTools.IsBottomOfYear(dt);
      Assert.AreEqual(res1 == dt, res3, "IsBottomOfYear()");
    }

    [TestCase("20210101", "20211231")]
    [TestCase("20210924", "20211231")]
    [TestCase("20211231", "20211231")]
    public void EndOfYear(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.EndOfYear(dt);
      Assert.AreEqual(wanted, res1, "EndOfYear(DateTime)");

      DateTime res2 = TimeTools.EndOfYear(dt.Year);
      Assert.AreEqual(wanted, res2, "EndOfYear(int)");

      bool res3 = TimeTools.IsEndOfYear(dt);
      Assert.AreEqual(res1 == dt, res3, "IsEndOfYear()");
    }

    [TestCase("20210101", "20210101")]
    [TestCase("20200301", "20200301")]
    [TestCase("20200229", "20200201")]
    [TestCase("20211231", "20211201")]
    public void BottomOfMonth(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.BottomOfMonth(dt);
      Assert.AreEqual(wanted, res1, "BottomOfMonth(DateTime)");

      DateTime res2 = TimeTools.BottomOfMonth(dt.Year, dt.Month);
      Assert.AreEqual(wanted, res2, "BottomOfMonth(int, int)");

      bool res3 = TimeTools.IsBottomOfMonth(dt);
      Assert.AreEqual(res1 == dt, res3, "IsBottomOfMonth()");
    }

    [TestCase("20210101", "20210131")]
    [TestCase("20200301", "20200331")]
    [TestCase("20200201", "20200229")]
    [TestCase("20200229", "20200229")]
    [TestCase("20210201", "20210228")]
    [TestCase("20210228", "20210228")]
    [TestCase("20211231", "20211231")]
    public void EndOfMonth(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.EndOfMonth(dt);
      Assert.AreEqual(wanted, res1, "EndOfMonth(DateTime)");

      DateTime res2 = TimeTools.EndOfMonth(dt.Year, dt.Month);
      Assert.AreEqual(wanted, res2, "EndOfMonth(int, int)");

      bool res3 = TimeTools.IsEndOfMonth(dt);
      Assert.AreEqual(res1 == dt, res3, "EndOfMonth()");
    }

    [TestCase("20210101", "20210101")]
    [TestCase("20210331", "20210101")]
    [TestCase("20210401", "20210401")]
    [TestCase("20210630", "20210401")]
    [TestCase("20210701", "20210701")]
    [TestCase("20210930", "20210701")]
    [TestCase("20211001", "20211001")]
    [TestCase("20211231", "20211001")]
    public void BottomOfQuarter(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.BottomOfQuarter(dt);
      Assert.AreEqual(wanted, res1, "BottomOfQuarter(DateTime)");

      bool res2 = TimeTools.IsBottomOfQuarter(dt);
      Assert.AreEqual(res1 == dt, res2, "IsBottomOfQuarter()");
    }

    [TestCase("20210101", "20210331")]
    [TestCase("20210331", "20210331")]
    [TestCase("20210401", "20210630")]
    [TestCase("20210630", "20210630")]
    [TestCase("20210701", "20210930")]
    [TestCase("20210930", "20210930")]
    [TestCase("20211001", "20211231")]
    [TestCase("20211231", "20211231")]
    public void EndOfQuarter(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = TimeTools.EndOfQuarter(dt);
      Assert.AreEqual(wanted, res1, "EndOfQuarter(DateTime)");

      bool res2 = TimeTools.IsEndOfQuarter(dt);
      Assert.AreEqual(res1 == dt, res2, "IsEndOfQuarter()");
    }

    [TestCase("20210924", DayOfWeek.Monday, "20210920")]
    [TestCase("20210924", DayOfWeek.Thursday, "20210923")]
    [TestCase("20210924", DayOfWeek.Friday, "20210924")]
    [TestCase("20210924", DayOfWeek.Saturday, "20210918")]
    public void BottomOfWeek(string sValue, DayOfWeek dow, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.BottomOfWeek(dt, dow);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("20210919", "20210913")]
    [TestCase("20210920", "20210920")]
    [TestCase("20210921", "20210920")]
    public void BottomOfWeekMonday(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.BottomOfWeekMonday(dt);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("20210924", DayOfWeek.Monday, "20210927")]
    [TestCase("20210924", DayOfWeek.Thursday, "20210930")]
    [TestCase("20210924", DayOfWeek.Friday, "20210924")]
    [TestCase("20210924", DayOfWeek.Saturday, "20210925")]
    public void EndOfWeek(string sValue, DayOfWeek dow, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.EndOfWeek(dt, dow);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("20210918", "20210919")]
    [TestCase("20210919", "20210919")]
    [TestCase("20210920", "20210926")]
    public void EndOfWeekSunday(string sValue, string sWanted)
    {
      DateTime dt = Creators.DateTime(sValue);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.EndOfWeekSunday(dt);
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region Min/Max()

    [TestCase("20211220", "20211221", "20211220")]
    public void Min_DateTime(string s1, string s2, string sWanted)
    {
      DateTime dt1 = Creators.DateTime(s1);
      DateTime dt2 = Creators.DateTime(s2);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, TimeTools.Min(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.Min(dt2, dt1), "#2");
    }

    [TestCase("20211220", "20211221", "20211221")]
    public void Max_DateTime(string s1, string s2, string sWanted)
    {
      DateTime dt1 = Creators.DateTime(s1);
      DateTime dt2 = Creators.DateTime(s2);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, TimeTools.Max(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.Max(dt2, dt1), "#2");
    }

    [TestCase("20211220", "20211221", "20211220")]
    [TestCase("20211220", "", "20211220")]
    [TestCase("", "", "")]
    public void Min_NDateTime(string s1, string s2, string sWanted)
    {
      DateTime? dt1 = Creators.NDateTime(s1);
      DateTime? dt2 = Creators.NDateTime(s2);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, TimeTools.Min(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.Min(dt2, dt1), "#2");
    }

    [TestCase("20211220", "20211221", "20211221")]
    [TestCase("20211220", "", "20211220")]
    [TestCase("", "", "")]
    public void Max_NDateTime(string s1, string s2, string sWanted)
    {
      DateTime? dt1 = Creators.NDateTime(s1);
      DateTime? dt2 = Creators.NDateTime(s2);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, TimeTools.Max(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.Max(dt2, dt1), "#2");
    }

    [TestCase("1:2:3", "1:2:4", "1:2:3")]
    public void Min_TimeSpan(string s1, string s2, string sWanted)
    {
      TimeSpan ts1 = TimeSpan.Parse(s1);
      TimeSpan ts2 = TimeSpan.Parse(s2);
      TimeSpan wanted = TimeSpan.Parse(sWanted);

      Assert.AreEqual(wanted, TimeTools.Min(ts1, ts2), "#1");
      Assert.AreEqual(wanted, TimeTools.Min(ts2, ts1), "#2");
    }

    [TestCase("1:2:3", "1:2:4", "1:2:4")]
    public void Max_TimeSpan(string s1, string s2, string sWanted)
    {
      TimeSpan ts1 = TimeSpan.Parse(s1);
      TimeSpan ts2 = TimeSpan.Parse(s2);
      TimeSpan wanted = TimeSpan.Parse(sWanted);

      Assert.AreEqual(wanted, TimeTools.Max(ts1, ts2), "#1");
      Assert.AreEqual(wanted, TimeTools.Max(ts2, ts1), "#2");
    }


    [TestCase("1:2:3", "1:2:4", "1:2:3")]
    [TestCase("1:2:3", "", "1:2:3")]
    [TestCase("", "", "")]
    public void Min_NTimeSpan(string s1, string s2, string sWanted)
    {
      TimeSpan? ts1 = Creators.NTimeSpan(s1);
      TimeSpan? ts2 = Creators.NTimeSpan(s2);
      TimeSpan? wanted = Creators.NTimeSpan(sWanted);

      Assert.AreEqual(wanted, TimeTools.Min(ts1, ts2), "#1");
      Assert.AreEqual(wanted, TimeTools.Min(ts2, ts1), "#2");
    }

    [TestCase("1:2:3", "1:2:4", "1:2:4")]
    [TestCase("1:2:3", "", "1:2:3")]
    [TestCase("", "", "")]
    public void Max_NTimeSpan(string s1, string s2, string sWanted)
    {
      TimeSpan? ts1 = Creators.NTimeSpan(s1);
      TimeSpan? ts2 = Creators.NTimeSpan(s2);
      TimeSpan? wanted = Creators.NTimeSpan(sWanted);

      Assert.AreEqual(wanted, TimeTools.Max(ts1, ts2), "#1");
      Assert.AreEqual(wanted, TimeTools.Max(ts2, ts1), "#2");
    }

    #endregion

    #region IsEqualYearAndMonth()

    [TestCase("20211220", "20211221", true)]
    [TestCase("20211220", "20211120", false)]
    [TestCase("20211220", "20201220", false)]
    public void IsEqualYearAndMonth_DateTime(string s1, string s2, bool wanted)
    {
      DateTime dt1 = Creators.DateTime(s1);
      DateTime dt2 = Creators.DateTime(s2);

      Assert.AreEqual(wanted, TimeTools.IsEqualYearAndMonth(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.IsEqualYearAndMonth(dt2, dt1), "#2");
    }

    [TestCase("20211220", "20211221", true)]
    [TestCase("20211220", "20211120", false)]
    [TestCase("20211220", "20201220", false)]
    [TestCase("20211220", "", false)]
    [TestCase("", "", true)]
    public void IsEqualYearAndMonth_NDateTime(string s1, string s2, bool wanted)
    {
      DateTime? dt1 = Creators.NDateTime(s1);
      DateTime? dt2 = Creators.NDateTime(s2);

      Assert.AreEqual(wanted, TimeTools.IsEqualYearAndMonth(dt1, dt2), "#1");
      Assert.AreEqual(wanted, TimeTools.IsEqualYearAndMonth(dt2, dt1), "#2");
    }

    #endregion

    #region CreateDateTime()

    [TestCase(2021, 12, 21, "20211221")]
    [TestCase(2021, 12, 0, "20211201")]
    [TestCase(2021, 12, 32, "20211231")]
    [TestCase(2020, 2, 30, "20200229")]
    [TestCase(2021, 0, 15, "20210115")]
    [TestCase(2021, 13, 15, "20211215")]
    public void CreateDateTime_YMD(int y, int m, int d, string sWanted)
    {
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.CreateDateTime(y, m, d);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("20211215", 21, "20211221")]
    [TestCase("20211215", 0, "20211201")]
    [TestCase("20211215", 32, "20211231")]
    public void SetDateDay(string sDate, int d, string sWanted)
    {
      DateTime date = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res = TimeTools.SetDateDay(date, d);
      Assert.AreEqual(wanted, res);
    }


    #endregion

    #region CreateDateTimeIfValid()

    // Год не проверяем

    [TestCase(2020, 2, 29, true)]
    [TestCase(2020, 0, 1, false)]
    [TestCase(2020, 13, 1, false)]
    [TestCase(2020, 2, 0, false)]
    [TestCase(2020, 2, 30, false)]
    public void CreateDateTimeIfValid_date(int year, int month, int day, bool isValid)
    {
      DateTime? res = TimeTools.CreateDateTimeIfValid(year, month, day);
      Assert.AreEqual(isValid, res.HasValue, "HasValue");
      if (res.HasValue)
      {
        Assert.AreEqual(year, res.Value.Year, "year");
        Assert.AreEqual(month, res.Value.Month, "month");
        Assert.AreEqual(day, res.Value.Day, "day");
      }
    }

    [TestCase(2020, 2, 29, 23, 59, 59, true)]
    [TestCase(2020, 0, 1, 23, 59, 59, false)]
    [TestCase(2020, 13, 1, 23, 59, 59, false)]
    [TestCase(2020, 2, 0, 23, 59, 59, false)]
    [TestCase(2020, 2, 30, 23, 59, 59, false)]
    [TestCase(2020, 2, 29, -1, 59, 59, false)]
    [TestCase(2020, 2, 29, 24, 59, 59, false)]
    [TestCase(2020, 2, 30, 23, -1, 59, false)]
    [TestCase(2020, 2, 30, 23, 60, 59, false)]
    [TestCase(2020, 2, 29, 24, 59, -1, false)]
    [TestCase(2020, 2, 29, 24, 59, 60, false)]
    public void CreateDateTimeIfValid_datetime(int year, int month, int day, int hour, int minute, int second, bool isValid)
    {
      DateTime? res = TimeTools.CreateDateTimeIfValid(year, month, day, hour, minute, second);
      Assert.AreEqual(isValid, res.HasValue, "HasValue");
      if (res.HasValue)
      {
        Assert.AreEqual(year, res.Value.Year, "year");
        Assert.AreEqual(month, res.Value.Month, "month");
        Assert.AreEqual(day, res.Value.Day, "day");
        Assert.AreEqual(hour, res.Value.Hour, "hour");
        Assert.AreEqual(minute, res.Value.Minute, "minute");
        Assert.AreEqual(second, res.Value.Second, "second");
      }
    }

    [TestCase(2020, 2, 29, 23, 59, 59, 0, true)]
    [TestCase(2020, 0, 1, 23, 59, 59, 0, false)]
    [TestCase(2020, 13, 1, 23, 59, 59, 0, false)]
    [TestCase(2020, 2, 0, 23, 59, 59, 0, false)]
    [TestCase(2020, 2, 30, 23, 59, 59, 0, false)]
    [TestCase(2020, 2, 29, -1, 59, 59, 0, false)]
    [TestCase(2020, 2, 29, 24, 59, 59, 0, false)]
    [TestCase(2020, 2, 30, 23, -1, 59, 0, false)]
    [TestCase(2020, 2, 30, 23, 60, 59, 0, false)]
    [TestCase(2020, 2, 29, 24, 59, -1, 0, false)]
    [TestCase(2020, 2, 29, 24, 59, 60, 0, false)]
    [TestCase(2020, 2, 29, 23, 59, 59, -1, false)]
    [TestCase(2020, 2, 29, 23, 59, 59, 1000, false)]
    public void CreateDateTimeIfValid_datetime_ms(int year, int month, int day, int hour, int minute, int second, int millisecond, bool isValid)
    {
      DateTime? res = TimeTools.CreateDateTimeIfValid(year, month, day, hour, minute, second, millisecond);
      Assert.AreEqual(isValid, res.HasValue, "HasValue");
      if (res.HasValue)
      {
        Assert.AreEqual(year, res.Value.Year, "year");
        Assert.AreEqual(month, res.Value.Month, "month");
        Assert.AreEqual(day, res.Value.Day, "day");
        Assert.AreEqual(hour, res.Value.Hour, "hour");
        Assert.AreEqual(minute, res.Value.Minute, "minute");
        Assert.AreEqual(second, res.Value.Second, "second");
        Assert.AreEqual(millisecond, res.Value.Millisecond, "millisecond");
      }
    }

    #endregion
  }
}
