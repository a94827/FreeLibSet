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
  public class YearMonthRangeTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_YearMonth()
    {
      YearMonth arg1 = new YearMonth(2022, 1);
      YearMonth arg2 = new YearMonth(2023, 3);
      YearMonthRange sut = new YearMonthRange(arg1, arg2);
      Assert.AreEqual(arg1, sut.FirstYM, "FirstYM");
      Assert.AreEqual(arg2, sut.LastYM, "LastYM");
      Assert.AreEqual(15, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20220101-20230331"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [TestCase("202301", "")]
    [TestCase("", "202301")]
    [TestCase("202303", "202301")]
    // Случай [TestCase("", "")] пока не определен
    public void Constructor_YearMonth_exception(string sArg1, string sArg2)
    {
      YearMonth arg1 = Creators.YearMonth(sArg1);
      YearMonth arg2 = Creators.YearMonth(sArg2);
      YearMonthRange sut;
      Assert.Catch(delegate() { sut = new YearMonthRange(arg1, arg2); });
    }

    [Test]
    public void Constructor_DateTime()
    {
      DateTime arg1 = new DateTime(2022, 12, 15);
      DateTime arg2 = new DateTime(2023, 3, 9);
      YearMonthRange sut = new YearMonthRange(arg1, arg2);
      Assert.AreEqual(Creators.YearMonth("202212"), sut.FirstYM, "FirstYM");
      Assert.AreEqual(Creators.YearMonth("202303"), sut.LastYM, "LastYM");
      Assert.AreEqual(4, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20221201-20230331"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_Year()
    {
      YearMonthRange sut = new YearMonthRange(2023);
      Assert.AreEqual(Creators.YearMonth("202301"), sut.FirstYM, "FirstYM");
      Assert.AreEqual(Creators.YearMonth("202312"), sut.LastYM, "LastYM");
      Assert.AreEqual(12, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20230101-20231231"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_Year_Month()
    {
      YearMonthRange sut = new YearMonthRange(2023, 3);
      Assert.AreEqual(Creators.YearMonth("202303"), sut.FirstYM, "FirstYM");
      Assert.AreEqual(Creators.YearMonth("202303"), sut.LastYM, "LastYM");
      Assert.AreEqual(1, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20230301-20230331"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_SingleYearMonth()
    {
      YearMonth arg = new YearMonth(2023, 3);
      YearMonthRange sut = new YearMonthRange(arg);
      Assert.AreEqual(Creators.YearMonth("202303"), sut.FirstYM, "FirstYM");
      Assert.AreEqual(Creators.YearMonth("202303"), sut.LastYM, "LastYM");
      Assert.AreEqual(1, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20230301-20230331"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_Year_TwoMonths()
    {
      YearMonthRange sut = new YearMonthRange(2023, 2, 3);
      Assert.AreEqual(Creators.YearMonth("202302"), sut.FirstYM, "FirstYM");
      Assert.AreEqual(Creators.YearMonth("202303"), sut.LastYM, "LastYM");
      Assert.AreEqual(2, sut.MonthCount, "MonthCount");
      Assert.AreEqual(Creators.DateRange("20230201-20230331"), sut.DateRange, "DateRange");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_default()
    {
      YearMonthRange sut = new YearMonthRange();
      Assert.IsTrue(sut.FirstYM.IsEmpty, "FirstYM");
      Assert.IsTrue(sut.LastYM.IsEmpty, "LastYM");
      Assert.AreEqual(0, sut.MonthCount, "MonthCount");
      Assert.IsTrue(sut.DateRange.IsEmpty, "DateRange");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(0, sut.ToArray().Length, "ToArray()");
    }

    #endregion

    #region Свойство Item

    [Test]
    public void Item()
    {
      YearMonthRange sut = Creators.YearMonthRange("202211-202303");
      Assert.AreEqual(Creators.YearMonth("202211"), sut[0], "[0]");
      Assert.AreEqual(Creators.YearMonth("202212"), sut[1], "[1]");
      Assert.AreEqual(Creators.YearMonth("202301"), sut[2], "[2]");
      Assert.AreEqual(Creators.YearMonth("202302"), sut[3], "[3]");
      Assert.AreEqual(Creators.YearMonth("202303"), sut[4], "[4]");

      YearMonth dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "[-1]");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[5]; }, "[5]");
    }

    #endregion

    #region ToArray()

    [Test]
    public void ToArray()
    {
      YearMonthRange sut = Creators.YearMonthRange("202211-202303");
      YearMonth[] res = sut.ToArray();
      Assert.AreEqual(new YearMonth[5]{
        Creators.YearMonth("202211"),
        Creators.YearMonth("202212"),
        Creators.YearMonth("202301"),
        Creators.YearMonth("202302"),
        Creators.YearMonth("202303")}, res);
    }

    #endregion

    #region Объединение и пересечение периодов

    [TestCase("202112-202202", "202211-202302", "202112-202302")]
    [TestCase("202112-202202", "202201-202302", "202112-202302")]
    [TestCase("202112-202302", "202205-202207", "202112-202302")]
    [TestCase("202211-202302", "", "202211-202302")]
    [TestCase("", "", "")]
    public void OpOr(string sArg1, string sArg2, string sWantedRes)
    {
      YearMonthRange arg1 = Creators.YearMonthRange(sArg1);
      YearMonthRange arg2 = Creators.YearMonthRange(sArg2);
      YearMonthRange wantedRes = Creators.YearMonthRange(sWantedRes);

      YearMonthRange res1 = arg1 | arg2;
      Assert.AreEqual(wantedRes, res1, "#1");

      YearMonthRange res2 = arg2 | arg1;
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    [TestCase("202112-202202", "202201-202302", "202201-202202")]
    [TestCase("202112-202302", "202205-202207", "202205-202207")]
    public void OpAnd(string sArg1, string sArg2, string sWantedRes)
    {
      YearMonthRange arg1 = Creators.YearMonthRange(sArg1);
      YearMonthRange arg2 = Creators.YearMonthRange(sArg2);
      YearMonthRange wantedRes = Creators.YearMonthRange(sWantedRes);

      YearMonthRange res1 = arg1 & arg2;
      Assert.AreEqual(wantedRes, res1, "#1");

      YearMonthRange res2 = arg2 & arg1;
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    [TestCase("202112-202202", "202203-202204")]
    [TestCase("202112-202202", "")]
    [TestCase("", "")]
    public void OpAnd_exception(string sArg1, string sArg2)
    {
      YearMonthRange arg1 = Creators.YearMonthRange(sArg1);
      YearMonthRange arg2 = Creators.YearMonthRange(sArg2);

      YearMonthRange dummy;
      Assert.Catch(delegate() { dummy = arg1 & arg2; }, "#1");
      Assert.Catch(delegate() { dummy = arg2 & arg1; }, "#2");
    }


    [TestCase("202112-202202", "202201-202302", true)]
    [TestCase("202112-202302", "202205-202207", true)]
    [TestCase("202112-202202", "202203-202204", false)]
    [TestCase("202112-202202", "", false)]
    [TestCase("", "", false)]
    public void IsCrossed(string sArg1, string sArg2, bool wantedRes)
    {
      YearMonthRange arg1 = Creators.YearMonthRange(sArg1);
      YearMonthRange arg2 = Creators.YearMonthRange(sArg2);

      Assert.AreEqual(wantedRes, YearMonthRange.IsCrossed(arg1, arg2), "#1");
      Assert.AreEqual(wantedRes, YearMonthRange.IsCrossed(arg2, arg1), "#2");
    }

    #endregion

    #region Операторы "+" и "-"

    [TestCase("202105-202203", 8, "202201-202211")]
    [TestCase("202105-202203", 0, "202105-202203")]
    [TestCase("202105-202203", -8, "202009-202107")]
    public void OpAdd_months(string sSut, int months, string sWantedRes)
    {
      YearMonthRange sut = Creators.YearMonthRange(sSut);
      YearMonthRange wantedRes = Creators.YearMonthRange(sWantedRes);

      Assert.AreEqual(wantedRes, sut + months);
    }

    [Test]
    public void OpAdd_months_empty()
    {
      YearMonthRange sut = new YearMonthRange();
      YearMonthRange dummy;
      Assert.Catch(delegate() { dummy = sut + 1; });
    }

    [TestCase("202105-202203", 8, "202009-202107")]
    [TestCase("202105-202203", 0, "202105-202203")]
    [TestCase("202105-202203", -8, "202201-202211")]
    public void OpSub_months(string sSut, int months, string sWantedRes)
    {
      YearMonthRange sut = Creators.YearMonthRange(sSut);
      YearMonthRange wantedRes = Creators.YearMonthRange(sWantedRes);

      Assert.AreEqual(wantedRes, sut - months);
    }

    [Test]
    public void OpSub_months_empty()
    {
      YearMonthRange sut = new YearMonthRange();
      YearMonthRange dummy;
      Assert.Catch(delegate() { dummy = sut - 1; });
    }

    #endregion

    #region Перечислитель

    [Test]
    public void Enumerator()
    {
      YearMonthRange sut = Creators.YearMonthRange("202211-202302");
      List<YearMonth> lst = new List<YearMonth>();
      foreach (YearMonth ym in sut)
        lst.Add(ym);

      Assert.AreEqual(new YearMonth[4]{
        Creators.YearMonth("202211"),
        Creators.YearMonth("202212"),
        Creators.YearMonth("202301"),
        Creators.YearMonth("202302")}, lst.ToArray());
    }

    [Test]
    public void Enumerator_empty()
    {
      YearMonthRange sut = Creators.YearMonthRange("");
      List<YearMonth> lst = new List<YearMonth>();
      foreach (YearMonth ym in sut)
        lst.Add(ym);

      Assert.AreEqual(0, lst.Count);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      YearMonthRange sut = Creators.YearMonthRange("202301-202307");

      byte[] b = SerializationTools.SerializeBinary(sut);
      YearMonthRange res = (YearMonthRange)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(sut.FirstYM, res.FirstYM, "FirstYM");
      Assert.AreEqual(sut.LastYM, res.LastYM, "LastYM");
      // пока нет поля Tag Assert.AreEqual(sut.Tag, res.Tag, "Tag");
    }

    #endregion
  }
}
