//#define OLD_TOTYPE

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using NUnit.Framework;
using FreeLibSet.Tests;
using FreeLibSet.DependedValues;
using FreeLibSet.Calendar;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  public class DepToolsTests_String
  {
    #region Строковые функции

    [TestCase("ABC", 3)]
    [TestCase("", 0)]
    [TestCase(null, 0)]
    public void LengthEx(string s, int wanted)
    {
      Assert.AreEqual(wanted, DepTools.LengthEx(new DepConst<string>(s)).Value);
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("ABC", true)]
    public void IsNotEmptyEx(string s, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.IsNotEmptyEx(new DepConst<string>(s)).Value);
    }


    [TestCase("AbCdE", 0, 2, "Ab")]
    [TestCase("AbCdE", 1, 2, "bC")]
    [TestCase("AbCdE", 3, 2, "dE")]
    [TestCase("AbCdE", 5, 2, "")]
    [TestCase(null, 0, 2, "")]
    [TestCase("AbCdE", -1, 2, "")]
    [TestCase("AbCdE", 1, 0, "")]
    [TestCase("AbCdE", 1, -1, "")]
    public void SubstingEx(string s, int startIndex, int length, string wanted)
    {
      Assert.AreEqual(wanted, DepTools.SubstringEx(new DepConst<string>(s),
        new DepConst<int>(startIndex),
        new DepConst<int>(length)).Value);
    }

    [TestCase("AbCdE", "Ab", true)]
    [TestCase("AbCdE", "ab", false)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "Ab", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void StartsWithOrdinalEx(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.StartsWithOrdinalEx(new DepConst<string>(s),
        new DepConst<string>(substring)).Value);
    }

    [TestCase("AbCdE", "Ab", true)]
    [TestCase("AbCdE", "ab", true)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "Ab", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void StartsWithOrdinalIgnoreCaseEx(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.StartsWithOrdinalIgnoreCaseEx(new DepConst<string>(s),
        new DepConst<string>(substring)).Value);
    }

    [TestCase("AbCdE", "dE", true)]
    [TestCase("AbCdE", "de", false)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "dE", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void EndsWithOrdinalEx(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.EndsWithOrdinalEx(new DepConst<string>(s),
        new DepConst<string>(substring)).Value);
    }

    [TestCase("AbCdE", "dE", true)]
    [TestCase("AbCdE", "de", true)]
    [TestCase("AbCdE", "z", false)]
    [TestCase(null, "dE", false)]
    [TestCase("AbCdE", "", true)]
    [TestCase("AbCdE", null, true)]
    public void EndsWithOrdinalIgnoreCaseEx(string s, string substring, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.EndsWithOrdinalIgnoreCaseEx(new DepConst<string>(s),
        new DepConst<string>(substring)).Value);
    }

    [Test]
    public void ToStringEx_int()
    {
      Assert.AreEqual("123", DepTools.ToStringEx<int>(new DepConst<int>(123)).Value);
    }

    private class TestClass
    {
      public override string ToString()
      {
        return "Hello";
      }
    }

    [Test]
    public void ToStringEx_object()
    {
      Assert.AreEqual("Hello", DepTools.ToStringEx<TestClass>(new DepConst<TestClass>(new TestClass())).Value);
    }

    [Test]
    public void ToStringEx_null()
    {
      Assert.AreEqual("", DepTools.ToStringEx<TestClass>(new DepConst<TestClass>(null)).Value);
    }

    [TestCase("ABC", "^[0-9]", false)]
    [TestCase("1ABC", "^[0-9]", true)]
    [TestCase(null, "^[0-9]", false)]
    [TestCase("ABC", null, true)]
    public void RegexIsMatchEx(string s, string pattern, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.RegexIsMatchEx(new DepConst<string>(s), pattern).Value);
    }

    #endregion
  }

  [TestFixture]
  public class DepToolsTests_DateTime
  {
    #region Компоненты

    [TestCase(null, 0)]
    [TestCase("20211201", 2021)]
    public void YearEx_NDate(string sDate, int wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.YearEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20211201", 2021)]
    public void YearEx_Date(string sDate, int wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.YearEx(new DepConst<DateTime>(dt)).Value);
    }


    [TestCase(null, 0)]
    [TestCase("20211201", 12)]
    public void MonthEx_NDate(string sDate, int wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.MonthEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20211201", 12)]
    public void MonthEx_Date(string sDate, int wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.MonthEx(new DepConst<DateTime>(dt)).Value);
    }


    [TestCase(null, 0)]
    [TestCase("20211201", 1)]
    public void DayEx_NDate(string sDate, int wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.DayEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20211201", 1)]
    public void DayEx_Date(string sDate, int wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.DayEx(new DepConst<DateTime>(dt)).Value);
    }


    // [TestCase(null, "Sunday")] // Это значение не является регламентированным
    [TestCase("20211201", "Wednesday")]
    public void DayOfWeekEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(sWanted, DepTools.DayOfWeekEx(new DepConst<DateTime?>(dt)).Value.ToString());
    }

    [TestCase("20211201", "Wednesday")]
    public void DayOfWeekEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(sWanted, DepTools.DayOfWeekEx(new DepConst<DateTime>(dt)).Value.ToString());
    }

    #endregion

    #region IsBottom/EndOfXXX()

    #region Year

    [TestCase(null, false)]
    [TestCase("20211201", false)]
    [TestCase("20210401", false)]
    [TestCase("20210101", true)]
    public void IsBottomOfYearEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfYearEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase(null, false)]
    [TestCase("20211201", false)]
    [TestCase("20210930", false)]
    [TestCase("20211231", true)]
    public void IsEndOfYearEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfYearEx(new DepConst<DateTime?>(dt)).Value);
    }


    [TestCase("20211201", false)]
    [TestCase("20210401", false)]
    [TestCase("20210101", true)]
    public void IsBottomOfYearEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfYearEx(new DepConst<DateTime>(dt)).Value);
    }

    [TestCase("20211201", false)]
    [TestCase("20210930", false)]
    [TestCase("20211231", true)]
    public void IsEndOfYearEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfYearEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region Quarter

    [TestCase(null, false)]
    [TestCase("20211201", false)]
    [TestCase("20210401", true)]
    [TestCase("20210101", true)]
    public void IsBottomOfQuarterEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfQuarterEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase(null, false)]
    [TestCase("20211031", false)]
    [TestCase("20210930", true)]
    [TestCase("20211231", true)]
    public void IsEndOfQuarterEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfQuarterEx(new DepConst<DateTime?>(dt)).Value);
    }


    [TestCase("20211201", false)]
    [TestCase("20210401", true)]
    [TestCase("20210101", true)]
    public void IsBottomOfQuarterEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfQuarterEx(new DepConst<DateTime>(dt)).Value);
    }

    [TestCase("20211031", false)]
    [TestCase("20210930", true)]
    [TestCase("20211231", true)]
    public void IsEndOfQuarterEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfQuarterEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region Month

    [TestCase(null, false)]
    [TestCase("20210801", true)]
    [TestCase("20210802", false)]
    public void IsBottomOfMonthEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfMonthEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase(null, false)]
    [TestCase("20210831", true)]
    [TestCase("20210802", false)]
    public void IsEndOfMonthEx_NDate(string sDate, bool wanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfMonthEx(new DepConst<DateTime?>(dt)).Value);
    }


    [TestCase("20210801", true)]
    [TestCase("20210802", false)]
    public void IsBottomOfMonthEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsBottomOfMonthEx(new DepConst<DateTime>(dt)).Value);
    }

    [TestCase("20210831", true)]
    [TestCase("20210802", false)]
    public void IsEndOfMonthEx_Date(string sDate, bool wanted)
    {
      DateTime dt = Creators.DateTime(sDate);

      Assert.AreEqual(wanted, DepTools.IsEndOfMonthEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #endregion

    #region Bottom/EndOfXXX()

    #region Year

    [TestCase("20210515", "20210101")]
    [TestCase("", "")]
    public void BottomOfYearEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfYearEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20210101")]
    public void BottomOfYearEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfYearEx(new DepConst<DateTime>(dt)).Value);
    }


    [TestCase("20210515", "20211231")]
    [TestCase("", "")]
    public void EndOfYearEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfYearEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20211231")]
    public void EndOfYearEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfYearEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region Quarter

    [TestCase("20210515", "20210401")]
    [TestCase("", "")]
    public void BottomOfQuarterEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfQuarterEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20210401")]
    public void BottomOfQuarterEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfQuarterEx(new DepConst<DateTime>(dt)).Value);
    }


    [TestCase("20210515", "20210630")]
    [TestCase("", "")]
    public void EndOfQuarterEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfQuarterEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20210630")]
    public void EndOfQuarterEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfQuarterEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region Month

    [TestCase("20210515", "20210501")]
    [TestCase("", "")]
    public void BottomOfMonthEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfMonthEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20210501")]
    public void BottomOfMonthEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfMonthEx(new DepConst<DateTime>(dt)).Value);
    }


    [TestCase("20210515", "20210531")]
    [TestCase("", "")]
    public void EndOfMonthEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfMonthEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "20210531")]
    public void EndOfMonthEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfMonthEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #endregion
  }

  [TestFixture]
  public class DepToolsTests_YearMonth
  {
    #region YearEx, MonthEx

    [TestCase("202105", 2021)]
    [TestCase("", 0)]
    public void YearEx(string sYM, int wanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      Assert.AreEqual(wanted, DepTools.YearEx(new DepConst<YearMonth>(ym)).Value);
    }

    [TestCase("202105", 5)]
    [TestCase("", 0)]
    public void MonthEx(string sYM, int wanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      Assert.AreEqual(wanted, DepTools.MonthEx(new DepConst<YearMonth>(ym)).Value);
    }

    #endregion

    #region N/Bottom/EndOfMonthEx

    [TestCase("202105", "20210501")]
    [TestCase("", "")]
    public void NBottomOfMonthEx(string sYM, string sWanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.NBottomOfMonthEx(new DepConst<YearMonth>(ym)).Value);
    }

    [TestCase("202105", "20210501")]
    public void BottomOfMonthEx(string sYM, string sWanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.BottomOfMonthEx(new DepConst<YearMonth>(ym)).Value);
    }

    [TestCase("202105", "20210531")]
    [TestCase("", "")]
    public void NEndOfMonthEx(string sYM, string sWanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      DateTime? wanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.NEndOfMonthEx(new DepConst<YearMonth>(ym)).Value);
    }

    [TestCase("202105", "20210531")]
    public void EndOfMonthEx(string sYM, string sWanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);
      DateTime wanted = Creators.DateTime(sWanted);

      Assert.AreEqual(wanted, DepTools.EndOfMonthEx(new DepConst<YearMonth>(ym)).Value);
    }

    #endregion

    #region YearMonthEx

    [TestCase(2021, 11, "202111")]
    [TestCase(0, 0, "")]
    [TestCase(2021, 13, "")]
    [TestCase(10000, 1, "")]
    public void YearMonthEx_year_month(int year, int month, string sWanted)
    {
      YearMonth ym = DepTools.YearMonthEx(new DepConst<int>(year), new DepConst<int>(month)).Value;

      YearMonth wanted = Creators.YearMonth(sWanted);
      Assert.AreEqual(wanted, ym);
    }

    [TestCase("20210515", "202105")]
    [TestCase("", "")]
    public void YearMonthEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      YearMonth wanted = Creators.YearMonth(sWanted);
      Assert.AreEqual(wanted, DepTools.YearMonthEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "202105")]
    public void YearMonthEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      YearMonth wanted = Creators.YearMonth(sWanted);
      Assert.AreEqual(wanted, DepTools.YearMonthEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region IsNotEmptyEx

    [TestCase("202105", true)]
    [TestCase("", false)]
    public void IsNotEmptyEx(string sYM, bool wanted)
    {
      YearMonth ym = Creators.YearMonth(sYM);

      Assert.AreEqual(wanted, DepTools.IsNotEmptyEx(new DepConst<YearMonth>(ym)).Value);
    }

    #endregion
  }

  [TestFixture]
  public class DepToolsTests_MonthDay
  {
    #region MonthEx, DayEx

    [TestCase("1201", 12)]
    [TestCase("", 0)]
    public void MonthEx(string sMD, int wanted)
    {
      MonthDay md = Creators.MonthDay(sMD);
      Assert.AreEqual(wanted, DepTools.MonthEx(new DepConst<MonthDay>(md)).Value);
    }

    [TestCase("1201", 1)]
    [TestCase("", 0)]
    public void DayEx(string sMD, int wanted)
    {
      MonthDay md = Creators.MonthDay(sMD);
      Assert.AreEqual(wanted, DepTools.DayEx(new DepConst<MonthDay>(md)).Value);
    }

    #endregion

    #region MonthDayEx

    [TestCase(11, 15, "1115")]
    [TestCase(0, 0, "")]
    [TestCase(13, 1, "")]
    [TestCase(2, 29, "")]
    public void MonthDayEx_month_day(int month, int day, string sWanted)
    {
      MonthDay md = DepTools.MonthDayEx(new DepConst<int>(month), new DepConst<int>(day)).Value;

      MonthDay wanted = Creators.MonthDay(sWanted);
      Assert.AreEqual(wanted, md);
    }

    [TestCase("20210515", "0515")]
    [TestCase("", "")]
    public void MonthExEx_NDate(string sDate, string sWanted)
    {
      DateTime? dt = Creators.NDateTime(sDate);
      MonthDay wanted = Creators.MonthDay(sWanted);
      Assert.AreEqual(wanted, DepTools.MonthDayEx(new DepConst<DateTime?>(dt)).Value);
    }

    [TestCase("20210515", "0515")]
    public void YearMonthEx_Date(string sDate, string sWanted)
    {
      DateTime dt = Creators.DateTime(sDate);
      MonthDay wanted = Creators.MonthDay(sWanted);
      Assert.AreEqual(wanted, DepTools.MonthDayEx(new DepConst<DateTime>(dt)).Value);
    }

    #endregion

    #region IsNotEmptyEx

    [TestCase("0515", true)]
    [TestCase("", false)]
    public void IsNotEmptyEx(string sMD, bool wanted)
    {
      MonthDay md = Creators.MonthDay(sMD);

      Assert.AreEqual(wanted, DepTools.IsNotEmptyEx(new DepConst<MonthDay>(md)).Value);
    }

    #endregion

    #region GetDateEx

    [TestCase("0228", 2020, false, "20200228")]
    [TestCase("0228", 2020, true, "20200229")]
    [TestCase("", 2020, false, "")]
    [TestCase("0228", 10000, false, "")]
    public void GetNDateEx(string sMD, int year, bool february29, string sWanted)
    {
      MonthDay md = Creators.MonthDay(sMD);
      DateTime? wanted1 = Creators.NDateTime(sWanted);
      DateTime wanted2 = wanted1 ?? DateTime.MinValue;

      Assert.AreEqual(wanted1, DepTools.GetNDateEx(new DepConst<MonthDay>(md),
        new DepConst<int>(year),
        february29).Value, "GetNDate()");

      Assert.AreEqual(wanted2, DepTools.GetDateEx(new DepConst<MonthDay>(md),
        new DepConst<int>(year),
        february29).Value, "GetDate()");
    }

    #endregion
  }

  [TestFixture]
  public class DepToolsTests_Math
  {
    #region Min/Max

    [TestCase("20210408", "20211201", "20211215", "20210408")]
    [TestCase("20211201", "20210408", "20211215", "20210408")]
    [TestCase("20211201", "20211215", "20210408", "20210408")]
    public void MinEx_simple(string sDate1, string sDate2, string sDate3, string sWanted)
    {
      DateTime dt1 = Creators.DateTime(sDate1);
      DateTime dt2 = Creators.DateTime(sDate2);
      DateTime dt3 = Creators.DateTime(sDate3);
      DateTime dtWanted = Creators.DateTime(sWanted);

      Assert.AreEqual(dtWanted, DepTools.MinEx<DateTime>(new DepConst<DateTime>(dt1),
        new DepConst<DateTime>(dt2),
        new DepConst<DateTime>(dt3)).Value);
    }

    [TestCase("20211215", "20211201", "20210408", "20211215")]
    [TestCase("20211201", "20211215", "20210408", "20211215")]
    [TestCase("20211201", "20210408", "20211215", "20211215")]
    public void MaxEx_simple(string sDate1, string sDate2, string sDate3, string sWanted)
    {
      DateTime dt1 = Creators.DateTime(sDate1);
      DateTime dt2 = Creators.DateTime(sDate2);
      DateTime dt3 = Creators.DateTime(sDate3);
      DateTime dtWanted = Creators.DateTime(sWanted);

      Assert.AreEqual(dtWanted, DepTools.MaxEx<DateTime>(new DepConst<DateTime>(dt1),
        new DepConst<DateTime>(dt2),
        new DepConst<DateTime>(dt3)).Value);
    }

    [TestCase("20210408", "20211201", "20211215", "20210408")]
    [TestCase("20211201", "20210408", "20211215", "20210408")]
    [TestCase("20211201", "20211215", "20210408", "20210408")]
    [TestCase("20210408", "", "", "20210408")]
    [TestCase("", "20210408", "", "20210408")]
    [TestCase("", "", "", "")]
    public void MinEx_nullable(string sDate1, string sDate2, string sDate3, string sWanted)
    {
      DateTime? dt1 = Creators.NDateTime(sDate1);
      DateTime? dt2 = Creators.NDateTime(sDate2);
      DateTime? dt3 = Creators.NDateTime(sDate3);
      DateTime? dtWanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(dtWanted, DepTools.MinEx<DateTime>(new DepConst<DateTime?>(dt1),
        new DepConst<DateTime?>(dt2),
        new DepConst<DateTime?>(dt3)).Value);
    }

    [TestCase("20211215", "20211201", "20210408", "20211215")]
    [TestCase("20211201", "20211215", "20210408", "20211215")]
    [TestCase("20211201", "20210408", "20211215", "20211215")]
    [TestCase("20210408", "", "", "20210408")]
    [TestCase("", "20210408", "", "20210408")]
    [TestCase("", "", "", "")]
    public void MaxEx_nullable(string sDate1, string sDate2, string sDate3, string sWanted)
    {
      DateTime? dt1 = Creators.NDateTime(sDate1);
      DateTime? dt2 = Creators.NDateTime(sDate2);
      DateTime? dt3 = Creators.NDateTime(sDate3);
      DateTime? dtWanted = Creators.NDateTime(sWanted);

      Assert.AreEqual(dtWanted, DepTools.MaxEx<DateTime>(new DepConst<DateTime?>(dt1),
        new DepConst<DateTime?>(dt2),
        new DepConst<DateTime?>(dt3)).Value);
    }

    #endregion

    #region InRange()

    [TestCase("20210515", "20210501", "20210531", true)]
    [TestCase("20210501", "20210501", "20210531", true)]
    [TestCase("20210531", "20210501", "20210531", true)]
    [TestCase("20210430", "20210501", "20210531", false)]
    [TestCase("20210601", "20210501", "20210531", false)]
    [TestCase("20210515", "", "20210515", true)]
    [TestCase("20210515", "", "20210514", false)]
    [TestCase("20210515", "20210515", "", true)]
    [TestCase("20210515", "20210516", "", false)]
    [TestCase("20210515", "", "", true)]
    public void InRangeEx(string sDate1, string sDate2, string sDate3, bool wanted)
    {
      DateTime dt1 = Creators.DateTime(sDate1);
      DateTime? dt2 = Creators.NDateTime(sDate2);
      DateTime? dt3 = Creators.NDateTime(sDate3);

      Assert.AreEqual(wanted, DepTools.InRangeEx<DateTime>(new DepConst<DateTime>(dt1),
        new DepConst<DateTime?>(dt2),
        new DepConst<DateTime?>(dt3)).Value);
    }

    #endregion
  }

  [TestFixture]
  public class DepToolsTests_Other
  {
    #region Nullable

    [TestCase(null, 0)]
    [TestCase(1, 1)]
    public void ReplaceNullEx_1(int? value, int wanted)
    {
      Assert.AreEqual(wanted, DepTools.ReplaceNullEx<int>(new DepConst<int?>(value)).Value);
    }

    [TestCase(null, 2, 2)]
    [TestCase(1, 2, 1)]
    public void ReplaceNullEx_2(int? value, int nullValue, int wanted)
    {
      Assert.AreEqual(wanted, DepTools.ReplaceNullEx<int>(new DepConst<int?>(value), nullValue).Value);
    }


    [TestCase(1, true)]
    [TestCase(0, true)]
    [TestCase(null, false)]
    public void IsNotEmptyEx(int? value, bool wanted)
    {
      Assert.AreEqual(wanted, DepTools.IsNotEmptyEx(new DepConst<int?>(value)).Value);
    }

    #endregion

    #region Enum

    private enum TestEnum
    {
      One = 1,
      Three = 3,
      Five = 5
    }

    [Test]
    public void EnumToIntEx()
    {
      Assert.AreEqual(3, DepTools.EnumToIntEx<TestEnum>(new DepConst<TestEnum>(TestEnum.Three)).Value);
    }

    [Test]
    public void EnumFromIntEx()
    {
      Assert.AreEqual(TestEnum.Five, DepTools.EnumFromIntEx<TestEnum>(new DepConst<int>(5)).Value);
    }

    [Test]
    public void EnumToStringEx()
    {
      Assert.AreEqual("Three", DepTools.EnumToStringEx<TestEnum>(new DepConst<TestEnum>(TestEnum.Three)).Value);
    }

    [Test]
    public void EnumFromStringEx()
    {
      Assert.AreEqual(TestEnum.Five, DepTools.EnumFromStringEx<TestEnum>(new DepConst<string>("Five")).Value);
    }

    #endregion

    #region Array

    [Test]
    public void LengthEx()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { };
      int[] a3 = null;

      Assert.AreEqual(3, DepTools.LengthEx<int>(new DepConst<int[]>(a1)).Value, "int[3]");
      Assert.AreEqual(0, DepTools.LengthEx<int>(new DepConst<int[]>(a2)).Value, "int[0]");
      Assert.AreEqual(0, DepTools.LengthEx<int>(new DepConst<int[]>(a3)).Value, "null");
    }

    #endregion

    #region ToTypeEx()

#if OLD_TOTYPE

    [Test]
    public void ToType_int_to_int()
    {
      DepValue<int> src = new DepConst<int>(1);
      Assert.AreEqual(1, DepTools.ToTypeEx<int>(src).Value);
    }


    [Test]
    public void ToType_double_to_int()
    { 
      DepValue<double> src=new DepConst<double>(1.2);
      Assert.AreEqual(1, DepTools.ToTypeEx<int>(src).Value);
    }

    [Test]
    public void ToType_object_to_int()
    {
      DepValue<object> src = new DepConst<object>(1);
      Assert.AreEqual(1, DepTools.ToTypeEx<int>(src).Value);
    }


    [Test]
    public void ToType_null_to_zero()
    {
      DepValue<object> src = new DepConst<object>(null);
      Assert.AreEqual(0, DepTools.ToTypeEx<int>(src).Value);
    }

    [Test]
    public void ToType_null_to_null()
    {
      DepValue<object> src = new DepConst<object>(null);
      Assert.IsNull(DepTools.ToTypeEx<string>(src).Value);
    }

    [Test]
    public void ToType_invalid_cast()
    {
      DepValue<double> src = new DepConst<double>(1.2);

      DateTime res;
      Assert.Catch(delegate(){res = DepTools.ToTypeEx<DateTime>(src).Value;});
    }

    [Test]
    public void ToNType()
    {
      DepValue<object> src1 = new DepConst<object>(1.2);
      Assert.AreEqual(1, DepTools.ToNTypeEx<int>(src1).Value, "has value");

      DepValue<object> src2 = new DepConst<object>(null);
      Assert.IsNull(DepTools.ToNTypeEx<int>(src2).Value, "null");
    }

#else

    [Test]
    public void ToTypeEx_int()
    {
      IDepValue src = new DepOutput<int>(123);

      DepValue<int> v1 = DepTools.ToTypeEx<int>(src);
      Assert.AreSame(src, v1, "To Int32");

      DepValue<double> v2 = DepTools.ToTypeEx<double>(src);
      Assert.AreEqual(123.0, v2.Value, "To Double");

      DepValue<double> v3 = DepTools.ToTypeEx<double>(src);
      Assert.AreSame(v2, v3, "To Double again");

      DepValue<int?> v4 = DepTools.ToTypeEx<int?>(src);
      Assert.AreEqual(123, v4.Value, "To Nullable<Int32>");

      DepValue<double?> v5 = DepTools.ToTypeEx<double?>(src);
      Assert.AreEqual(123.0, v5.Value, "To Nullable<Double>");

      DepValue<string> v6 = DepTools.ToTypeEx<string>(src);
      Assert.AreEqual(123.ToString(), v6.Value, "To String");

      ((IDepOutput)src).OwnerSetValue(0);
      Assert.AreEqual(0.0, v2.Value, "After set value - Double");
      Assert.AreEqual(0, v4.Value, "After set value - Nullable<Int32>");
      Assert.AreEqual(0.0, v5.Value, "After set value - Nullable<Double>");
      Assert.AreEqual(0.ToString(), v6.Value, "After set value - String");
    }

    [Test]
    public void ToTypeEx_nullable_int()
    {
      IDepValue src = new DepOutput<int?>(123);

      DepValue<int> v1 = DepTools.ToTypeEx<int>(src);
      Assert.AreEqual(123, v1.Value, "To Int32");

      DepValue<double> v2 = DepTools.ToTypeEx<double>(src);
      Assert.AreEqual(123.0, v2.Value, "To Double");

      DepValue<int?> v3 = DepTools.ToTypeEx<int?>(src);
      Assert.AreSame(src, v3, "To Nullable<Int32>");

      DepValue<double?> v4 = DepTools.ToTypeEx<double?>(src);
      Assert.AreEqual(123.0, v4.Value, "To Nullable<Double>");

      DepValue<string> v5 = DepTools.ToTypeEx<string>(src);
      Assert.AreEqual(123.ToString(), v5.Value, "To String");

      ((IDepOutput)src).OwnerSetValue(0);
      Assert.AreEqual(0, v1.Value, "After set value 0 - Int32");
      Assert.AreEqual(0.0, v2.Value, "After set value 0 - Double");
      Assert.AreEqual(0.0, v4.Value, "After set value 0 - Nullable<Double>");
      Assert.AreEqual(0.ToString(), v5.Value, "After set value 0 - String");

      ((IDepOutput)src).OwnerSetValue(null);
      Assert.AreEqual(0, v1.Value, "After set value null - Int32");
      Assert.AreEqual(0.0, v2.Value, "After set value null - Double");
      Assert.IsNull(v4.Value, "After set value null - Nullable<Double>");
      Assert.IsNull(v5.Value, "After set value null - String");
    }

    [Test]
    public void ToTypeEx_string()
    {
      IDepValue src = new DepOutput<string>("123");

      DepValue<int> v1 = DepTools.ToTypeEx<int>(src, CultureInfo.InvariantCulture);
      Assert.AreEqual(123, v1.Value, "To Int32");

      DepValue<double> v2 = DepTools.ToTypeEx<double>(src, CultureInfo.InvariantCulture);
      Assert.AreEqual(123.0, v2.Value, "To Double");

      DepValue<int?> v3 = DepTools.ToTypeEx<int?>(src, CultureInfo.InvariantCulture);
      Assert.AreEqual(123, v3.Value, "To Nullable<Int32>");

      DepValue<double?> v4 = DepTools.ToTypeEx<double?>(src, CultureInfo.InvariantCulture);
      Assert.AreEqual(123.0, v4.Value, "To Nullable<Double>");

      DepValue<string> v5 = DepTools.ToTypeEx<string>(src);
      Assert.AreSame(src, v5, "To String");

      ((IDepOutput)src).OwnerSetValue("");
      Assert.AreEqual(0, v1.Value, "After set value String.Empty - Int32");
      Assert.AreEqual(0.0, v2.Value, "After set value String.Empty - Double");
      Assert.IsNull(v3.Value, "After set value String.Empty - Nullable<Int32>");
      Assert.IsNull(v4.Value, "After set value String.Empty - Nullable<Double>");
      Assert.AreEqual("", v5.Value, "After set value String.Empty - String");

      ((IDepOutput)src).OwnerSetValue("XXX"); // непреобразуемое значение
      Assert.AreEqual(0, v1.Value, "After set value to wrong text - Int32");
      Assert.AreEqual(0.0, v2.Value, "After set value to wrong text - Double");
      Assert.IsNull(v3.Value, "After set value value to wrong text  - Nullable<Int32>");
      Assert.IsNull(v4.Value, "After set value value to wrong text  - Nullable<DOuble>");
      Assert.AreEqual("XXX", v5.Value, "After set value String.Empty - String");

      ((IDepOutput)src).OwnerSetValue(null);
      Assert.AreEqual(0, v1.Value, "After set value null - Int32");
      Assert.AreEqual(0.0, v2.Value, "After set value null - Double");
      Assert.IsNull(v3.Value, "After set value null - Nullable<Ште32>");
      Assert.IsNull(v4.Value, "After set value null - Nullable<Double>");
      Assert.IsNull(v5.Value, "After set value null - String");
    }

    [Test]
    public void ToTypeEx_cultureInfo()
    {
      IDepValue src = new DepOutput<double>(-123.45);

      DepValue<string> v1 = DepTools.ToTypeEx<string>(src, CultureInfo.GetCultureInfo("en-US"));
      DepValue<string> v2 = DepTools.ToTypeEx<string>(src, CultureInfo.GetCultureInfo("ru-RU"));

      Assert.AreEqual("-123.45", v1.Value, "en-US");
      Assert.AreEqual("-123,45", v2.Value, "ru-RU");

      DepValue<string> v3 = DepTools.ToTypeEx<string>(src, CultureInfo.GetCultureInfo("en-US"));
      Assert.AreSame(v1, v3, "en-US again");
    }

#endif

    #endregion

    #region Create()

    [Test]
    public void CreateOutput_class()
    {
      IDepOutput sut = DepTools.CreateOutput(typeof(string));
      Assert.IsInstanceOf(typeof(DepOutput<string>), sut, "Type");
      Assert.IsNull(sut.Value, "Value");

      sut.OwnerSetValue("123");
      Assert.AreEqual("123", sut.Value, "After OwnerSetValue(string)");

      sut.OwnerSetValue(null);
      Assert.IsNull(sut.Value, "After OwnerSetValue(null)");
    }

    [Test]
    public void CreateOutput_valuetype()
    {
      IDepOutput sut = DepTools.CreateOutput(typeof(int));
      Assert.IsInstanceOf(typeof(DepOutput<int>), sut, "Type");
      Assert.AreEqual(0, sut.Value, "Value");

      sut.OwnerSetValue(123);
      Assert.AreEqual(123, sut.Value, "After OwnerSetValue(int)");

      sut.OwnerSetValue(null);
      Assert.AreEqual(0, sut.Value, "After OwnerSetValue(null)");
    }

    [Test]
    public void CreateInput_class()
    {
      IDepInput sut = DepTools.CreateInput(typeof(string));
      Assert.IsInstanceOf(typeof(DepInput<string>), sut, "Type");
      Assert.IsNull(sut.Value, "Value");

      sut.Value="123";
      Assert.AreEqual("123", sut.Value, "After set value (string)");

      sut.Value = null;
      Assert.IsNull(sut.Value, "After set value (null)");
    }

    [Test]
    public void CreateInput_valuetype()
    {
      IDepInput sut = DepTools.CreateInput(typeof(int));
      Assert.IsInstanceOf(typeof(DepInput<int>), sut, "Type");
      Assert.AreEqual(0, sut.Value, "Value");

      sut.Value = 123;
      Assert.AreEqual(123, sut.Value, "After set value (int)");

      sut.Value = null;
      Assert.AreEqual(0, sut.Value, "After set value (null)");
    }

    #endregion
  }
}
