using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV;

namespace ExtTools.tests
{
  [TestFixture]
  public class MonthDayTests
  {
    /// <summary>
    /// Номер дня в году для 8 марта
    /// </summary>
    const int March8Day = 31 + 28 + 8;
    const int February28Day = 31 + 28;
    const int March1Day = 31 + 28 + 1;

    [Test]
    public void Constructor_Day()
    {
      DoTestConstructor(new MonthDay(March8Day));
    }


    [Test]
    public void Constructor_MD()
    {
      DoTestConstructor(new MonthDay(3, 8));
    }

    [Test]
    public void Constructor_Date()
    {
      DoTestConstructor(new MonthDay(new DateTime(2021, 3, 8)));
    }

    private void DoTestConstructor(MonthDay sut)
    {
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(March8Day, sut.DayOfYear, "DayOfYear");
      Assert.AreEqual(3, sut.Month, "Month");
      Assert.AreEqual(8, sut.Day, "Day");
    }

    [Test]
    public void Empty()
    {
      Assert.IsTrue(MonthDay.Empty.IsEmpty, "IsEmpty");
      Assert.IsTrue(MonthDay.Empty.BottomOfMonth.IsEmpty, "BottomOfMonth");
      Assert.IsTrue(MonthDay.Empty.BottomOfQuarter.IsEmpty, "BottomOfMonth");
      Assert.IsTrue(MonthDay.Empty.EndOfMonth.IsEmpty, "EndOfMonth");
      Assert.IsTrue(MonthDay.Empty.EndOfQuarter.IsEmpty, "EndOfMonth");
      Assert.IsTrue(MonthDay.Empty.NextDay.IsEmpty, "NextDay");
      Assert.IsTrue(MonthDay.Empty.PrevDay.IsEmpty, "PrevDay");
      Assert.IsTrue(MonthDay.Empty.AddDays(1).IsEmpty, "AddDays()");
    }

    [Test]
    public void BottomOfMonth()
    {
      MonthDay sut = new MonthDay(March8Day);
      Assert.AreEqual(new DateTime(2021, 3, 1), sut.BottomOfMonth.GetDate(2021, false));
    }

    [Test]
    public void EndOfMonth()
    {
      MonthDay sut = new MonthDay(March8Day);
      Assert.AreEqual(new DateTime(2021, 3, 31), sut.EndOfMonth.GetDate(2021, false));
    }

    [Test]
    public void BottomOfQuarter()
    {
      MonthDay sut = new MonthDay(March8Day);
      Assert.AreEqual(new DateTime(2021, 1, 1), sut.BottomOfQuarter.GetDate(2021, false));
    }

    [Test]
    public void EndOfQuarter()
    {
      MonthDay sut = new MonthDay(March8Day);
      Assert.AreEqual(new DateTime(2021, 3, 31), sut.EndOfQuarter.GetDate(2021, false));
    }

    [Test]
    public void BottomOfYear()
    {
      Assert.AreEqual(1, MonthDay.BottomOfYear.DayOfYear);
    }

    [Test]
    public void EndOfYear()
    {
      Assert.AreEqual(365, MonthDay.EndOfYear.DayOfYear);
    }

    [TestCase(March8Day, March8Day + 1)]
    [TestCase(1, 2)]
    [TestCase(365, 1)]
    [TestCase(February28Day, March1Day)]
    public void NextDay(int curr, int wanted)
    {
      MonthDay sut = new MonthDay(curr);
      MonthDay res = sut.NextDay;
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase(March8Day, March8Day - 1)]
    [TestCase(2, 1)]
    [TestCase(1, 365)]
    [TestCase(March1Day, February28Day)]
    public void PrevDay(int curr, int wanted)
    {
      MonthDay sut = new MonthDay(curr);
      MonthDay res = sut.PrevDay;
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase(February28Day, 1, March1Day)]
    [TestCase(February28Day, 3 * 365 + 1, March1Day)]
    [TestCase(February28Day, -2 * 365 + 1, March1Day)]
    [TestCase(365, 1, 1)]
    public void Operator_Add_Days(int curr, int days, int wanted)
    {
      MonthDay arg = new MonthDay(curr);
      MonthDay res = arg + days;
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase(March1Day, 1, February28Day)]
    [TestCase(March1Day, 3 * 365 + 1, February28Day)]
    [TestCase(March1Day, -2 * 365 + 1, February28Day)]
    [TestCase(1, 1, 365)]
    public void Operator_Substract_Days(int curr, int days, int wanted)
    {
      MonthDay arg = new MonthDay(curr);
      MonthDay res = arg - days;
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase(1, 2, 364)]
    [TestCase(2, 1, 1)]
    [TestCase(March1Day, February28Day, 1)]
    [TestCase(March1Day, March1Day, 0)]
    [TestCase(February28Day, March1Day, 364)]
    public void Operator_Substract_MonthDay(int arg1, int arg2, int wanted)
    {
      MonthDay arg1a = new MonthDay(arg1);
      MonthDay arg2a = new MonthDay(arg2);
      int res = arg1a - arg2a;
      Assert.AreEqual(wanted, res);
    }

    [TestCase(1, 1, 2)]
    [TestCase(365, 1, 1)]
    [TestCase(1, -1, 365)]
    [TestCase(1, 3 * 365, 1)]
    [TestCase(1, -2 * 365, 1)]
    [TestCase(1, 0, 1)]
    public void AddDays(int curr, int days, int wanted)
    {
      MonthDay sut = new MonthDay(curr);
      MonthDay res = sut.AddDays(days);
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase(1, 2021, false, "20210101")]
    [TestCase(February28Day, 2020, false, "20200228")]
    [TestCase(February28Day, 2020, true, "20200229")]
    [TestCase(February28Day, 2021, false, "20210228")]
    [TestCase(February28Day, 2021, true, "20210228")]
    public void GetDate(int curr, int year, bool february29, string sRes)
    {
      MonthDay sut = new MonthDay(curr);
      DateTime dt = sut.GetDate(year, february29);
      Assert.AreEqual(CreateDate(sRes), dt);
    }

    #region Вспомогательные методы

    public static DateTime CreateDate(string s)
    {
      return DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    #endregion
  }
}
