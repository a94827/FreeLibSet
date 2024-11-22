using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Calendar;
using FreeLibSet.Tests;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Calendar
{
  [TestFixture]
  public class MonthDayRangeTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Simple()
    {
      MonthDayRange sut = new MonthDayRange(new MonthDay(5, 3), new MonthDay(7, 1), "AAA");
      DoTestConstructor(sut);
    }

    [Test]
    public void Constructor_DateRange()
    {
      DateRange dtr = new DateRange(new DateTime(2021, 5, 3), new DateTime(2021, 7, 1), "AAA");
      MonthDayRange sut = new MonthDayRange(dtr);
      DoTestConstructor(sut);
    }

    [Test]
    public void Constructor_Copy()
    {
      MonthDayRange src = new MonthDayRange(new MonthDay(5, 3), new MonthDay(7, 1), "BBB");
      MonthDayRange sut = new MonthDayRange(src, "AAA");
      DoTestConstructor(sut);
    }

    private void DoTestConstructor(MonthDayRange sut)
    {
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.IsFalse(sut.IsWholeYear, "IsWholeYear");
      Assert.AreEqual(5, sut.First.Month, "FirstMonth");
      Assert.AreEqual(3, sut.First.Day, "FirstDay");
      Assert.AreEqual(7, sut.Last.Month, "LastMonth");
      Assert.AreEqual(1, sut.Last.Day, "FirstDay");
      Assert.AreEqual("AAA", sut.Tag, "Tag");
    }

    #endregion

    #region Days

    [TestCase("0101-0331", 31 + 28 + 31)]
    [TestCase("1101-0131", 30 + 31 + 31)]
    [TestCase("0101-1231", 365)]
    [TestCase("1231-0101", 2)]
    [TestCase("1231-1231", 1)]
    [TestCase("0101-0101", 1)]
    [TestCase("0701-0630", 365)]
    public void Days(string sSUT, int wanted)
    {
      MonthDayRange sut = Creators.MonthDayRange(sSUT);

      Assert.AreEqual(wanted, sut.Days);
    }

    #endregion

    #region Complement

    [TestCase("0503-0701", "0702-0502")]
    [TestCase("0702-0502", "0503-0701")]
    [TestCase("0101-1231", "0101-1231")]
    [TestCase("", "")]
    public void Complement(string sSUT, string sWanted)
    {
      MonthDayRange sut = Creators.MonthDayRange(sSUT);
      MonthDayRange wanted = Creators.MonthDayRange(sWanted);

      MonthDayRange res = sut.Complement;

      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region Age

    [TestCase("", "0:0:0")]
    [TestCase("0101-1231", "1:0:0")]
    [TestCase("0401-1230", "0:8:30")]
    [TestCase("0402-1231", "0:8:29")]
    [TestCase("0301-0331", "0:1:0")]
    [TestCase("0220-0319", "0:1:0")]
    [TestCase("0220-0320", "0:1:1")]
    [TestCase("0220-0318", "0:0:27")]
    [TestCase("1231-0105", "0:0:6")]
    public void Age(string sSut, string sWanted)
    {
      MonthDayRange sut = Creators.MonthDayRange(sSut);
      YearMonthDayAge wanted = Creators.YearMonthDayAge(sWanted);
      Assert.AreEqual(wanted, sut.Age);
    }

    #endregion

    #region Contains()

    [TestCase("0305-0701", "0410", true)]
    [TestCase("0305-0701", "0305", true)]
    [TestCase("0305-0701", "0701", true)]
    [TestCase("0305-0701", "0302", false)]
    [TestCase("0305-0701", "0702", false)]

    [TestCase("0701-0305", "1013", true)]
    [TestCase("0701-0305", "0701", true)]
    [TestCase("0701-0305", "0305", true)]
    [TestCase("0701-0305", "0630", false)]
    [TestCase("0701-0305", "0306", false)]
    public void Contains(string sSUT, string sMD, bool wanted)
    {
      MonthDayRange sut = Creators.MonthDayRange(sSUT);
      MonthDay md = Creators.MonthDay(sMD);
      Assert.AreEqual(wanted, sut.Contains(md), "Contains MonthDay");

      DateTime dt = new DateTime(2021, md.Month, md.Day);
      Assert.AreEqual(wanted, sut.Contains(dt), "Contains DateTime");
    }

    [Test]
    public void Contains_February29()
    {
      DateTime dt = new DateTime(2020, 2, 29);

      MonthDayRange sut1 = new MonthDayRange(new MonthDay(2, 28), new MonthDay(3, 1));
      Assert.IsTrue(sut1.Contains(dt), sut1.ToString());

      MonthDayRange sut2 = new MonthDayRange(new MonthDay(3, 2), new MonthDay(2, 28));
      Assert.IsTrue(sut2.Contains(dt), sut2.ToString());

      MonthDayRange sut3 = new MonthDayRange(new MonthDay(3, 1), new MonthDay(2, 27));
      Assert.IsFalse(sut3.Contains(dt), sut3.ToString());
    }

    #endregion

    #region GetCrosses()

#if XXX // Метод GetCross() является устаревшим
    [TestCase("0201-0331", "0325-0408", "0325-0331")]
    [TestCase("0331-0201", "0325-0408", "0331-0408")]
    [TestCase("0201-0331", "0408-0325", "0201-0325")]
    public void GetCross_True(string sR1, string sR2, string sWanted)
    {
      MonthDayRange r1 = Creators.CreateMonthDayRange(sR1);
      MonthDayRange r2 = Creators.CreateMonthDayRange(sR2);

      // Метод GetCross() должен возвращать одинаковый результат при перестановке аргументов.

      MonthDayRange res1 = MonthDayRange.GetCross(r1, r2);
      Assert.AreEqual(sWanted, Creators.ToString(res1), "GetCross() - Direct");
      Assert.IsTrue(MonthDayRange.IsCrossed(r1, r2), "IsCrossed() - Direct");

      MonthDayRange res2 = MonthDayRange.GetCross(r2, r1);
      Assert.AreEqual(sWanted, Creators.ToString(res2), "GetCross() - Exchanged");
      Assert.IsTrue(MonthDayRange.IsCrossed(r2, r1), "IsCrossed() - Exchanged");
    }

    [TestCase("0201-0331", "0401-0131")]
    [TestCase("0331-0201", "0202-0330")]
    public void GetCross_False(string sR1, string sR2)
    {
      MonthDayRange r1 = Creators.CreateMonthDayRange(sR1);
      MonthDayRange r2 = Creators.CreateMonthDayRange(sR2);

      MonthDayRange res1 = MonthDayRange.GetCross(r1, r2);
      Assert.IsTrue(res1.IsEmpty, "GetCross() - Direct");
      Assert.IsFalse(MonthDayRange.IsCrossed(r1, r2), "IsCrossed() - Direct");

      MonthDayRange res2 = MonthDayRange.GetCross(r2, r1);
      Assert.IsTrue(res2.IsEmpty, "GetCross() - Direct");
      Assert.IsFalse(MonthDayRange.IsCrossed(r2, r1), "IsCrossed() - Direct");
    }
#endif

    [TestCase("0201-0331", "0325-0408", "0325-0331")]
    [TestCase("0331-0201", "0325-0408", "0331-0408")]
    [TestCase("0201-0331", "0408-0325", "0201-0325")]
    [TestCase("0201-0331", "0408-0401", "0201-0331")]
    [TestCase("0201-0331", "0408-0401", "0201-0331")]
    [TestCase("0301-0228", "0401-0430", "0401-0430")]
    [TestCase("0201-0331", "0401-0131", "")]
    [TestCase("0331-0201", "0202-0330", "")]
    [TestCase("0201-0930", "0801-0331", "0201-0331,0801-0930")]
    [TestCase("1201-0731", "0501-0131", "0501-0731,1201-0131")]
    [TestCase("0201-0331", "", "")]
    [TestCase("", "0201-0331", "")]
    [TestCase("0201-0131", "0601-0630", "0601-0630")] // полный год
    [TestCase("0601-0630", "0101-1231", "0601-0630")] // полный год
    public void GetCrosses(string sR1, string sR2, string sWanted)
    {
      MonthDayRange r1 = Creators.MonthDayRange(sR1);
      r1 = new MonthDayRange(r1, "123");
      MonthDayRange r2 = Creators.MonthDayRange(sR2);
      r2 = new MonthDayRange(r2, "456");
      MonthDayRange[] wanted = Creators.MonthDayRangeArray(sWanted);

      MonthDayRange[] res1 = MonthDayRange.GetCrosses(r1, r2);
      CollectionAssert.AreEqual(wanted, res1, "GetCross() - Direct");
      Assert.AreEqual(wanted.Length > 0, MonthDayRange.IsCrossed(r1, r2), "IsCrossed() - Direct");
      for (int i = 0; i < res1.Length; i++)
        Assert.AreEqual("123", res1[i].Tag, "Tag");

      MonthDayRange[] res2 = MonthDayRange.GetCrosses(r2, r1);
      CollectionAssert.AreEqual(wanted, res2, "GetCross() - Inverted");
      Assert.AreEqual(wanted.Length > 0, MonthDayRange.IsCrossed(r2, r1), "IsCrossed() - Inverted");
    }

    #endregion

    #region GetDateRange()

    [TestCase("0228-0331", 2020, false, "20200228-20200331")]
    [TestCase("0228-0331", 2020, true, "20200228-20200331")]
    [TestCase("0331-0228", 2020, false, "20200331-20210228")]
    [TestCase("0331-0228", 2020, true, "20190331-20200229")]
    [TestCase("0408-0325", 2021, false, "20210408-20220325")]
    [TestCase("0408-0325", 2021, true, "20200408-20210325")]
    public void GetDateRange(string sMDR, int year, bool yearIsForLastDay, string sWanted)
    {
      DateRange wantedRes = Creators.DateRange(sWanted);
      MonthDayRange sut = Creators.MonthDayRange(sMDR);
      DateRange res = sut.GetDateRange(year, yearIsForLastDay);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region AddDays()

    [TestCase("0401-0630", 10, "0411-0710")]
    [TestCase("0401-0630", 0, "0401-0630")]
    [TestCase("0401-0630", -10, "0322-0620")]
    [TestCase("0401-0630", 2 * 365, "0401-0630")]
    [TestCase("0401-0630", -2 * 365, "0401-0630")]
    public void AddDays(string sMDR, int days, string sWanted)
    {
      MonthDayRange wanted = Creators.MonthDayRange(sWanted);
      MonthDayRange sut = Creators.MonthDayRange(sMDR);
      sut = new MonthDayRange(sut, "123");

      MonthDayRange res = sut.AddDays(days);
      Assert.AreEqual(wanted, res, "Result");
      Assert.AreEqual("123", res.Tag, "Tag");
    }

    #endregion

    #region Статические поля

    [Test]
    public void Empty()
    {
      Assert.IsTrue(MonthDayRange.Empty.IsEmpty, "IsEmpty");
      Assert.IsTrue(MonthDayRange.Empty.First.IsEmpty, "First");
      Assert.IsTrue(MonthDayRange.Empty.Last.IsEmpty, "Last");
      Assert.AreEqual(0, MonthDayRange.Empty.Days, "Days");
      Assert.IsFalse(MonthDayRange.Empty.IsWholeYear, "IsWholeYear");
      Assert.IsNull(MonthDayRange.Empty.Tag, "Tag");
      Assert.IsTrue(MonthDayRange.Empty.Complement.IsEmpty, "Complement");
    }

    [Test]
    public void WholeYear()
    {
      Assert.IsTrue(MonthDayRange.WholeYear.IsWholeYear, "IsWholeYear");
      Assert.IsFalse(MonthDayRange.WholeYear.IsEmpty, "IsEmpty");
      Assert.AreEqual(365, MonthDayRange.WholeYear.Days, "Days");
      Assert.IsNull(MonthDayRange.WholeYear.Tag, "Tag");
      Assert.IsTrue(MonthDayRange.WholeYear.Complement.IsWholeYear, "Complement");
    }

    #endregion

    #region Сравнение

    [TestCase("1231-0102", "1231-0102", true)]
    [TestCase("1231-0102", "0102-1231", false)]
    [TestCase("", "0102-1231", false)]
    [TestCase("0102-1231", "", false)]
    [TestCase("", "", true)]
    [TestCase("0101-1231", "0201-0131", true)] // whole year
    [TestCase("0810-0809", "0201-0131", true)] // whole year
    public void Operator_Equal(string sArg1, string sArg2, bool wanted)
    {
      MonthDayRange arg1 = Creators.MonthDayRange(sArg1);
      MonthDayRange arg2 = Creators.MonthDayRange(sArg2);

      bool res1 = (arg1 == arg2);
      Assert.AreEqual(wanted, res1, "==");

      bool res2 = (arg1 != arg2);
      Assert.AreEqual(!wanted, res2, "!=");
    }

    #endregion

    #region GetEnumerator()

    [TestCase("0101-0104", "0101,0102,0103,0104")]
    [TestCase("1231-0102", "1231,0101,0102")]
    [TestCase("", "")]
    public void GetEnumerator(string sMDR, string sWanted)
    {
      MonthDayRange sut = Creators.MonthDayRange(sMDR);
      List<MonthDay> lst = new List<MonthDay>();
      foreach (MonthDay item in sut)
        lst.Add(item);
      MonthDay[] wanted = Creators.MonthDayArray(sWanted);
      CollectionAssert.AreEqual(wanted, lst);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      MonthDayRange sut = new MonthDayRange (Creators.MonthDayRange("0101-0104"), "123");

      byte[] b = SerializationTools.SerializeBinary(sut);
      MonthDayRange res = (MonthDayRange)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(sut.First, res.First, "First");
      Assert.AreEqual(sut.Last, res.Last, "Last");
      Assert.AreEqual(sut.Tag, res.Tag, "Tag");
    }

    #endregion
  }
}
