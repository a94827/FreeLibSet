using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV;

namespace ExtTools.tests
{
  [TestFixture]
  public class MonthDayRangeTests
  {
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

    [TestCase("0101-0331", 31 + 28 + 31)]
    [TestCase("1101-0131", 30 + 31 + 31)]
    [TestCase("0101-1231", 365)]
    [TestCase("1231-0101", 2)]
    [TestCase("1231-1231", 1)]
    [TestCase("0101-0101", 1)]
    [TestCase("0701-0630", 365)]
    public void Days(string sSUT, int wanted)
    {
      MonthDayRange sut = Creators.CreateMonthDayRange(sSUT);

      Assert.AreEqual(wanted, sut.Days);
    }

    [TestCase("0503-0701", "0702-0502")]
    [TestCase("0702-0502", "0503-0701")]
    public void Complement(string sSUT, string sWanted)
    {
      MonthDayRange sut = Creators.CreateMonthDayRange(sSUT);

      MonthDayRange res = sut.Complement;

      Assert.AreEqual(sWanted, Creators.ToString(res));
    }

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
      MonthDayRange sut = Creators.CreateMonthDayRange(sSUT);
      MonthDay md = Creators.CreateMonthDay(sMD);
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

    [TestCase("0201-0331", "0325-0408", "0325-0331")]
    [TestCase("0331-0201", "0325-0408", "0331-0408")]
    [TestCase("0201-0331", "0408-0325", "0201-0325")]
    public void GetCross_True(string sR1, string sR2, string sWanted)
    {
      MonthDayRange r1 = Creators.CreateMonthDayRange(sR1);
      MonthDayRange r2 = Creators.CreateMonthDayRange(sR2);

      MonthDayRange res = MonthDayRange.GetCross(r1, r2);

      Assert.AreEqual(sWanted, Creators.ToString(res), "GetCross()");

      Assert.IsTrue(MonthDayRange.IsCrossed(r1, r2), "IsCrossed()");
    }

    [TestCase("0201-0331", "0401-0131")]
    [TestCase("0331-0201", "0202-0330")]
    public void GetCross_False(string sR1, string sR2)
    {
      MonthDayRange r1 = Creators.CreateMonthDayRange(sR1);
      MonthDayRange r2 = Creators.CreateMonthDayRange(sR2);

      MonthDayRange res = MonthDayRange.GetCross(r1, r2);
      Assert.IsTrue(res.IsEmpty, "GetCross()");

      Assert.IsFalse(MonthDayRange.IsCrossed(r1, r2), "IsCrossed()");
    }
  }
}
