using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV;

namespace ExtTools.tests
{
#if XXX
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

    [TestCase(1, 1, 3, 31, 31 + 28 + 21)]
    [TestCase(11, 1, 1, 31, 30 + 31 + 31)]
    [TestCase(1, 1, 12, 31, 365)]
    [TestCase(1, 7, 6, 30, 365)]
    public void Days(int m1, int d1, int m2, int d2, int wanted)
    {
      MonthDay md1 = new MonthDay(m1, d1);
      MonthDay md2 = new MonthDay(m2, d2);
      MonthDayRange sut = new MonthDayRange(md1, md2);

      Assert.AreEqual(wanted, sut.Days);
    }

    [Test]
    public void Complement()
    {
      MonthDayRange sut = new MonthDayRange(new MonthDay(5, 3), new MonthDay(7, 1));

      MonthDayRange res1 = sut.Complement;

      Assert.AreEqual(7, sut.First.Month, "FirstMonth");
      Assert.AreEqual(2, sut.First.Day, "FirstDay");
      Assert.AreEqual(3, sut.Last.Month, "LastMonth");
      Assert.AreEqual(6, sut.Last.Day, "FirstDay");
    }

    [Test]
    public void Complement_Pair()
    {
      MonthDayRange sut = new MonthDayRange(new MonthDay(5, 3), new MonthDay(7, 1));
      MonthDayRange res1 = sut.Complement;
      MonthDayRange res2 = res1.Complement;
      Assert.AreEqual(sut, res2);
    }

    [TestCase(3, 5, 7, 1, 4, 10, true)]
    [TestCase(3, 5, 7, 1, 3, 5, true)]
    [TestCase(3, 5, 7, 1, 7, 1, true)]
    [TestCase(3, 5, 7, 1, 3, 2, false)]
    [TestCase(3, 5, 7, 1, 7, 2, false)]

    [TestCase(7, 1, 3, 5, 10, 13, true)]
    [TestCase(7, 1, 3, 5, 7, 1, true)]
    [TestCase(7, 1, 3, 5, 3, 5, true)]
    [TestCase(7, 1, 3, 5, 6, 30, false)]
    [TestCase(7, 1, 3, 5, 3, 6, false)]
    public void Contains(int m1, int d1, int m2, int d2, int m3, int d3, bool wanted)
    {
      MonthDay md1 = new MonthDay(m1, d1);
      MonthDay md2 = new MonthDay(m2, d2);
      MonthDayRange sut = new MonthDayRange(md1, md2);
      MonthDay md3 = new MonthDay(m3, d3);
      Assert.AreEqual(wanted, sut.Contains(md3), "Contains MonthDay");

      DateTime dt = new DateTime(2021, m3, d3);
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

    [TestCase(2, 1, 3, 31,
      3, 25, 4, 8,
      3, 25, 3, 31)]
    [TestCase(3, 31, 2, 1, 
      3, 25, 4, 8,
      3, 31, 4, 8)]
    [TestCase(2, 1, 3, 31,
      4, 8, 3, 25, 
      2, 1, 4, 8)]
    public void GetCross_True(int m1, int d1, int m2, int d2,
      int m3, int d3, int m4, int d4,
      int m5, int d5, int m6, int d6)
    {
      MonthDay md1 = new MonthDay(m1, d1);
      MonthDay md2 = new MonthDay(m2, d2);
      MonthDayRange r1 = new MonthDayRange(md1, md2);

      MonthDay md3 = new MonthDay(m3, d3);
      MonthDay md4 = new MonthDay(m4, d4);
      MonthDayRange r2 = new MonthDayRange(md3, md4);

      MonthDay md5 = new MonthDay(m5, d6);
      MonthDay md6 = new MonthDay(m6, d6);
      MonthDayRange wanted = new MonthDayRange(md5, md6);


      MonthDayRange res = MonthDayRange.GetCross(r1, r2);
      Assert.AreEqual(wanted, res, "GetCross()");

      Assert.IsTrue(MonthDayRange.IsCrossed(r1, r2), "IsCrossed()");
    }

    [TestCase(2, 1, 3, 31,
      4, 1, 1, 31)]
    [TestCase(3, 31, 2, 1, 
      2, 2, 3, 30)]
    public void GetCross_False(int m1, int d1, int m2, int d2,
      int m3, int d3, int m4, int d4)
    {
      MonthDay md1 = new MonthDay(m1, d1);
      MonthDay md2 = new MonthDay(m2, d2);
      MonthDayRange r1 = new MonthDayRange(md1, md2);

      MonthDay md3 = new MonthDay(m3, d3);
      MonthDay md4 = new MonthDay(m4, d4);
      MonthDayRange r2 = new MonthDayRange(md3, md4);


      MonthDayRange res = MonthDayRange.GetCross(r1, r2);
      Assert.IsTrue(wanted, res.IsEmpty, "GetCross()");

      Assert.IsFalse(MonthDayRange.IsCrossed(r1, r2), "IsCrossed()");
    }
  }
#endif
}
