using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_Date
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

    [Test]
    public void Constants()
    {
      Assert.AreEqual(DateTime.MinValue, Creators.CreateDate(MinDate));
      Assert.AreEqual(DateTime.MaxValue.Date, Creators.CreateDate(MaxDate));
    }

    [TestCase(MinDate, "", "", Result = true)]
    [TestCase(MaxDate, "", "", Result = true)]
    [TestCase("20210714", "20210714", "20210714", Result = true)]
    [TestCase("20210714", "20210713", "20210713", Result = false)]
    [TestCase("20210714", "20210715", "20210715", Result = false)]
    [TestCase("20210714", "20210714", "", Result = true)]
    [TestCase("20210714", "20210715", "", Result = false)]
    [TestCase("20210714", "", "20210714", Result = true)]
    [TestCase("20210714", "", "20210713", Result = false)]
    public bool DateInRange(string testDate, string firstDate, string lastDate)
    {
      return DataTools.DateInRange(Creators.CreateDate(testDate),
        Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
    }

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
    public bool DateRangeCrossed(string firstDate1, string lastDate1, string firstDate2, string lastDate2)
    {
      return DataTools.DateRangeCrossed(Creators.CreateNDate(firstDate1), Creators.CreateNDate(lastDate1),
        Creators.CreateNDate(firstDate2), Creators.CreateNDate(lastDate2));
    }

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
      DateTime dt = Creators.CreateDate(testDate);
      DataTools.DateToRange(ref dt,
        Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
      
      Assert.AreEqual(Creators.CreateDate(res), dt);
    }

    [TestCase("20210714", "20210715", "20210716", "20210717", false, "", "")]
    [TestCase("20210714", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "", "20210715", "20210717", true, "20210715", "20210717")]
    [TestCase("", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "20210715", "", "", true, "20210714", "20210715")]
    public void GetDateRangeCross(string sdt11, string sdt12, string sdt21, string sdt22, bool wanted, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.CreateNDate(sdt11);
      DateTime? dt12 = Creators.CreateNDate(sdt12);
      DateTime? dt21 = Creators.CreateNDate(sdt21);
      DateTime? dt22 = Creators.CreateNDate(sdt22);

      bool res = DataTools.GetDateRangeCross(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(wanted, res, "Result");
      Assert.AreEqual(Creators.CreateNDate(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.CreateNDate(sdt12res), dt12, "LastDate1");
    }


    [TestCase("20210714", "20210715", "20210716", "20210717", "20210714", "20210717")]
    [TestCase("20210801", "20210831", "20210714", "20210714", "20210714", "20210831")]
    [TestCase("20210714", "20210715", "20210716", "", "20210714", "")]
    [TestCase("20210714", "20210715", "", "20210716", "", "20210716")]
    [TestCase("20210714", "", "", "20210716", "", "")]
    public void GetDateRangeUnion(string sdt11, string sdt12, string sdt21, string sdt22, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.CreateNDate(sdt11);
      DateTime? dt12 = Creators.CreateNDate(sdt12);
      DateTime? dt21 = Creators.CreateNDate(sdt21);
      DateTime? dt22 = Creators.CreateNDate(sdt22);

      DataTools.GetDateRangeUnion(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(Creators.CreateNDate(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.CreateNDate(sdt12res), dt12, "LastDate1");
    }


    [TestCase(2021, "20211231", "20211231", Result = true)]
    [TestCase(2022, "20211231", "20211231", Result = false)]
    [TestCase(2021, "", "20210101", Result = true)]
    [TestCase(2021, "20211231", "", Result = true)]
    [TestCase(2021, "", "20201231", Result = false)]
    [TestCase(2021, "20220101", "", Result = false)]
    [TestCase(9999, "", "", Result = true)]
    public bool YearInRange(int year, string firstDate, string lastDate)
    {
      return DataTools.YearInRange(year, Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
    }

    [TestCase("20210101", "20210101")]
    [TestCase("20210924", "20210101")]
    [TestCase("20211231", "20210101")]
    public void BottomOfYear(string sValue, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.BottomOfYear(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "BottomOfYear(DateTime)");

      DateTime res2 = DataTools.BottomOfYear(dt.Year);
      Assert.AreEqual(sWanted, Creators.ToString(res2), "BottomOfYear(int)");

      bool res3 = DataTools.IsBottomOfYear(dt);
      Assert.AreEqual(res1 == dt, res3, "IsBottomOfYear()");
    }

    [TestCase("20210101", "20211231")]
    [TestCase("20210924", "20211231")]
    [TestCase("20211231", "20211231")]
    public void EndOfYear(string sValue, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.EndOfYear(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "EndOfYear(DateTime)");

      DateTime res2 = DataTools.EndOfYear(dt.Year);
      Assert.AreEqual(sWanted, Creators.ToString(res2), "EndOfYear(int)");

      bool res3 = DataTools.IsEndOfYear(dt);
      Assert.AreEqual(res1 == dt, res3, "IsEndOfYear()");
    }

    [TestCase("20210101", "20210101")]
    [TestCase("20200301", "20200301")]
    [TestCase("20200229", "20200201")]
    [TestCase("20211231", "20211201")]
    public void BottomOfMonth(string sValue, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.BottomOfMonth(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "BottomOfMonth(DateTime)");

      DateTime res2 = DataTools.BottomOfMonth(dt.Year, dt.Month);
      Assert.AreEqual(sWanted, Creators.ToString(res2), "BottomOfMonth(int, int)");

      bool res3 = DataTools.IsBottomOfMonth(dt);
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
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.EndOfMonth(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "EndOfMonth(DateTime)");

      DateTime res2 = DataTools.EndOfMonth(dt.Year, dt.Month);
      Assert.AreEqual(sWanted, Creators.ToString(res2), "EndOfMonth(int, int)");

      bool res3 = DataTools.IsEndOfMonth(dt);
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
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.BottomOfQuarter(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "BottomOfQuarter(DateTime)");

      bool res2 = DataTools.IsBottomOfQuarter(dt);
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
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res1 = DataTools.EndOfQuarter(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "EndOfQuarter(DateTime)");

      bool res2 = DataTools.IsEndOfQuarter(dt);
      Assert.AreEqual(res1 == dt, res2, "IsEndOfQuarter()");
    }

    [TestCase("20210924", DayOfWeek.Monday, "20210920")]
    [TestCase("20210924", DayOfWeek.Thursday, "20210923")]
    [TestCase("20210924", DayOfWeek.Friday, "20210924")]
    [TestCase("20210924", DayOfWeek.Saturday, "20210918")]
    public void BottomOfWeek(string sValue, DayOfWeek dow, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res = DataTools.BottomOfWeek(dt, dow);
      Assert.AreEqual(sWanted, Creators.ToString(res));
    }

    [TestCase("20210919", "20210913")]
    [TestCase("20210920", "20210920")]
    [TestCase("20210921", "20210920")]
    public void BottomOfWeekMonday(string sValue, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res = DataTools.BottomOfWeekMonday(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res));
    }

    [TestCase("20210924", DayOfWeek.Monday, "20210927")]
    [TestCase("20210924", DayOfWeek.Thursday, "20210930")]
    [TestCase("20210924", DayOfWeek.Friday, "20210924")]
    [TestCase("20210924", DayOfWeek.Saturday, "20210925")]
    public void EndOfWeek(string sValue, DayOfWeek dow, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res = DataTools.EndOfWeek(dt, dow);
      Assert.AreEqual(sWanted, Creators.ToString(res));
    }

    [TestCase("20210918", "20210919")]
    [TestCase("20210919", "20210919")]
    [TestCase("20210920", "20210926")]
    public void EndOfWeekSunday(string sValue, string sWanted)
    {
      DateTime dt = Creators.CreateDate(sValue);

      DateTime res = DataTools.EndOfWeekSunday(dt);
      Assert.AreEqual(sWanted, Creators.ToString(res));
    }
  }
}
