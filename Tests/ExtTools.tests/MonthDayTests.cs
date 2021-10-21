using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Calendar;

namespace ExtTools.tests
{
  [TestFixture]
  public class MonthDayTests
  {
    /// <summary>
    /// Ќомер дн€ в году дл€ 8 марта
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
      // «десь провер€ем установленные свойства поштучно.
      // Ќе пытаемс€ использовать Assert.AreEquals(CreateMonthDay("0308"), sut),
      // вдруг операци€ сравнени€ неправильно работает

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

    [TestCase("0308", "0309")]
    [TestCase("0101", "0102")]
    [TestCase("1231", "0101")]
    [TestCase("0228", "0301")]
    public void NextDay(string sCurr, string sWanted)
    {
      MonthDay sut = Creators.CreateMonthDay(sCurr);
      MonthDay res = sut.NextDay;
      MonthDay wanted = Creators.CreateMonthDay(sWanted);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("0309", "0308")]
    [TestCase("0102", "0101")]
    [TestCase("0101", "1231")]
    [TestCase("0301", "0228")]
    public void PrevDay(string sCurr, string sWanted)
    {
      MonthDay sut = Creators.CreateMonthDay(sCurr);
      MonthDay res = sut.PrevDay;
      MonthDay wanted = Creators.CreateMonthDay(sWanted);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("0228", 1, "0301")]
    [TestCase("0228", 3 * 365 + 1, "0301")]
    [TestCase("0228", -2 * 365 + 1, "0301")]
    [TestCase("1230", 2, "0101")]
    [TestCase("0228", 0, "0228")]
    [TestCase("0228", 365, "0228")]
    public void Operator_Add_Days(string sCurr, int days, string sWanted)
    {
      MonthDay arg = Creators.CreateMonthDay(sCurr);
      MonthDay res = arg + days;
      MonthDay wanted = Creators.CreateMonthDay(sWanted);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("0301", 1, "0228")]
    [TestCase("0301", 3 * 365 + 1, "0228")]
    [TestCase("0301", -2 * 365 + 1, "0228")]
    [TestCase("0101", 2, "1230")]
    [TestCase("0228", 0, "0228")]
    [TestCase("0228", 365, "0228")]
    public void Operator_Substract_Days(string sCurr, int days, string sWanted)
    {
      MonthDay arg = Creators.CreateMonthDay(sCurr);
      MonthDay res = arg - days;
      MonthDay wanted = Creators.CreateMonthDay(sWanted);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("0101", "0102", 364)]
    [TestCase("0102", "0101", 1)]
    [TestCase("0301", "0228", 1)]
    [TestCase("0301", "0301", 0)]
    [TestCase("0228", "0301", 364)]
    public void Operator_Substract_MonthDay(string arg1, string arg2, int wanted)
    {
      MonthDay arg1a = Creators.CreateMonthDay(arg1);
      MonthDay arg2a = Creators.CreateMonthDay(arg2);
      int res = arg1a - arg2a;
      Assert.AreEqual(wanted, res);
    }

    [TestCase("0101", 1, 2)]
    [TestCase("1231", 1, 1)]
    [TestCase("0101", -1, 365)]
    [TestCase("0101", 3 * 365, 1)]
    [TestCase("0101", -2 * 365, 1)]
    [TestCase("0101", 0, 1)]
    public void AddDays(string sCurr, int days, int wanted)
    {
      MonthDay sut = Creators.CreateMonthDay(sCurr);
      MonthDay res = sut.AddDays(days);
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      Assert.AreEqual(wanted, res.DayOfYear, "DayOfYear");
    }

    [TestCase("0101", 2021, false, "20210101")]
    [TestCase("0228", 2020, false, "20200228")]
    [TestCase("0228", 2020, true, "20200229")]
    [TestCase("0228", 2021, false, "20210228")]
    [TestCase("0228", 2021, true, "20210228")]
    public void GetDate(string sCurr, int year, bool february29, string sRes)
    {
      MonthDay sut = Creators.CreateMonthDay(sCurr);
      DateTime dt = sut.GetDate(year, february29);
      Assert.AreEqual(Creators.CreateDate(sRes), dt);
    }
  }
}
