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
  public class YearMonthTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_2args()
    {
      // Проверяем все возможные значения
      for (int y = YearMonth.MinYear; y <= YearMonth.MaxYear; y++)
      {
        for (int m = 1; m <= 12; m++)
        {
          YearMonth sut = new YearMonth(y, m);
          Assert.AreEqual(y, sut.Year, "Year");
          Assert.AreEqual(m, sut.Month, "Month");
          Assert.IsFalse(sut.IsEmpty, "IsEmpty");
        }
      }
    }

    [Test]
    public void Constructor_2args_error()
    {
      YearMonth sut;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut = new YearMonth(2023, 0); }, "Month<1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut = new YearMonth(2023, 13); }, "Month>12");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut = new YearMonth(YearMonth.MinYear - 1, 1); }, "Year<1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut = new YearMonth(YearMonth.MaxYear + 1, 1); }, "Year is too big");
    }

    [Test]
    public void Constructor_DateTime()
    {
      DateTime date = new DateTime(2023, 3, 9);
      YearMonth sut = new YearMonth(date);
      Assert.AreEqual(2023, sut.Year, "Year");
      Assert.AreEqual(3, sut.Month, "Month");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_default()
    {
      YearMonth sut = new YearMonth();
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #endregion

    #region BottomOfMonth, EndOfMonth, DateRange

    [TestCase(2023, 3, "20230301")]
    [TestCase(2020, 2, "20200201")]
    public void BottomOfMonth(int year, int month, string sWantedRes)
    {
      DateTime wantedRes = Creators.DateTime(sWantedRes);
      YearMonth sut = new YearMonth(year, month);
      Assert.AreEqual(wantedRes, sut.BottomOfMonth);
    }

    [TestCase(2023, 3, "20230331")]
    [TestCase(2020, 2, "20200229")]
    public void EndOfMonth(int year, int month, string sWantedRes)
    {
      DateTime wantedRes = Creators.DateTime(sWantedRes);
      YearMonth sut = new YearMonth(year, month);
      Assert.AreEqual(wantedRes, sut.EndOfMonth);
    }

    [TestCase(2023, 3, "20230301-20230331")]
    [TestCase(2020, 2, "20200201-20200229")]
    public void DateRange(int year, int month, string sWantedRes)
    {
      DateRange wantedRes = Creators.DateRange(sWantedRes);
      YearMonth sut = new YearMonth(year, month);
      Assert.AreEqual(wantedRes, sut.DateRange);
    }

    #endregion

    #region Попадание даты

    [TestCase(2023, 3, "20230301", true)]
    [TestCase(2023, 3, "20230331", true)]
    [TestCase(2023, 3, "20230228", false)]
    [TestCase(2023, 3, "20230401", false)]
    public void Contains(int year, int month, string sDate, bool wantedRes)
    {
      DateTime date = Creators.DateTime(sDate);
      YearMonth sut = new YearMonth(year, month);
      Assert.AreEqual(wantedRes, sut.Contains(date));
    }

    [TestCase(2023, 3, "20230301", "20230301")]
    [TestCase(2023, 3, "20230331", "20230331")]
    [TestCase(2023, 3, "20230228", "20230301")]
    [TestCase(2023, 3, "20230401", "20230331")]
    public void DateToRange(int year, int month, string sDate, string sWantedRes)
    {
      DateTime date = Creators.DateTime(sDate);
      DateTime wantedRes = Creators.DateTime(sWantedRes);
      YearMonth sut = new YearMonth(year, month);
      sut.DateToRange(ref date);
      Assert.AreEqual(wantedRes, date);
    }

    #endregion

    #region Попадание в интервал

    [TestCase("202303", "202303", "202401", true)]
    [TestCase("202303", "202304", "202401", false)]
    [TestCase("202303", "202101", "202303", true)]
    [TestCase("202303", "202101", "202302", false)]
    [TestCase("202303", "", "202303", true)]
    [TestCase("202303", "", "202302", false)]
    [TestCase("202303", "202303", "", true)]
    [TestCase("202303", "202304", "", false)]
    [TestCase("202303", "", "", true)]
    [TestCase("", "202301", "202303", false)]
    [TestCase("", "202301", "", false)]
    [TestCase("", "", "202303", false)]
    [TestCase("", "", "", false)]
    public void IsInRange(string sSUT, string sMin, string sMax, bool wantedRes)
    {
      YearMonth sut = Creators.YearMonth(sSUT);
      YearMonth min = Creators.YearMonth(sMin);
      YearMonth max = Creators.YearMonth(sMax);
      Assert.AreEqual(wantedRes, sut.IsInRange(min, max));
    }

    #endregion

    #region Операторы "+" и "-"

    [TestCase("202303", 15, "202406")]
    [TestCase("202303", 0, "202303")]
    [TestCase("202303", -10, "202205")]
    public void OpAdd_months(string sSUT, int months, string sWantedRes)
    {
      YearMonth sut = Creators.YearMonth(sSUT);
      YearMonth wantedRes = Creators.YearMonth(sWantedRes);
      YearMonth res = sut + months;
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void OpAdd_months_empty()
    {
      YearMonth sut = new YearMonth();
      YearMonth res;
      Assert.Catch(delegate() { res = sut + 1; });
    }

    [TestCase("202303", 15, "202112")]
    [TestCase("202303", 0, "202303")]
    [TestCase("202303", -10, "202401")]
    public void OpSub_months(string sSUT, int months, string sWantedRes)
    {
      YearMonth sut = Creators.YearMonth(sSUT);
      YearMonth wantedRes = Creators.YearMonth(sWantedRes);
      YearMonth res = sut - months;
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void OpSub_months_empty()
    {
      YearMonth sut = new YearMonth();
      YearMonth res;
      Assert.Catch(delegate() { res = sut - 1; });
    }

    [TestCase("202201", "202303", -14)]
    [TestCase("202303", "202303", 0)]
    [TestCase("202303", "202201", 14)]
    public void OpSub_YearMonth(string sArg1, string sArg2, int wantedRes)
    {
      YearMonth arg1 = Creators.YearMonth(sArg1);
      YearMonth arg2 = Creators.YearMonth(sArg2);
      int res = arg1 - arg2;
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("202301", "")]
    [TestCase("", "202301")]
    [TestCase("", "")]
    public void OpSub_YearMonth_empty(string sArg1, string sArg2)
    {
      YearMonth arg1 = Creators.YearMonth(sArg1);
      YearMonth arg2 = Creators.YearMonth(sArg2);
      int res;

      Assert.Catch(delegate() { res = arg1 - arg2; });
    }

    #endregion

    #region Операции сравнения

    [TestCase("202212", "202301", -1)]
    [TestCase("202301", "202301", 0)]
    [TestCase("202302", "202301", 1)]
    [TestCase("202301", "", 1)]
    [TestCase("", "202301", -1)]
    [TestCase("", "", 0)]
    public void Compare(string sArg1, string sArg2, int wantedRes)
    {
      YearMonth arg1 = Creators.YearMonth(sArg1);
      YearMonth arg2 = Creators.YearMonth(sArg2);
      int res1 = arg1.CompareTo(arg2);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "CompareTo(YearMonth)");

      Assert.AreEqual(wantedRes > 0, arg1 > arg2, ">");
      Assert.AreEqual(wantedRes >= 0, arg1 >= arg2, ">=");
      Assert.AreEqual(wantedRes < 0, arg1 < arg2, "<");
      Assert.AreEqual(wantedRes <= 0, arg1 <= arg2, "<=");
      Assert.AreEqual(wantedRes == 0, arg1 == arg2, "==");
      Assert.AreEqual(wantedRes != 0, arg1 != arg2, "!=");
      Assert.AreEqual(wantedRes == 0, arg1.Equals(arg2), "Equals(YearMonth)");
      Assert.AreEqual(wantedRes == 0, arg1.Equals((object)arg2), "Equals(Object)");
      if (wantedRes == 0)
        Assert.IsTrue(arg1.GetHashCode() == arg2.GetHashCode(), "GetHashCode()");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      YearMonth sut = Creators.YearMonth("202307");

      byte[] b = SerializationTools.SerializeBinary(sut);
      YearMonth res = (YearMonth)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(sut.Year, res.Year, "Year");
      Assert.AreEqual(sut.Month, res.Month, "Month");
    }

    #endregion
  }
}
