using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Calendar;
using FreeLibSet.Tests;

namespace ExtTools_tests.Calendar
{
  [TestFixture]
  public class YearMonthDayAgeTests
  {
    #region Конструктор

    // Нужно подробное тестирование конструктора, так как используется сложная система хранения значения

    [TestCase(1, 2, 3)]
    [TestCase(-1, -2, -3)]
    [TestCase(0, 0, 0)]
    [TestCase(9998, 11, 30)]
    [TestCase(-9998, -11, -30)]
    [TestCase(0, 0, 30)]
    [TestCase(0, 0, -30)]
    [TestCase(0, 11, 0)]
    [TestCase(0, -11, 0)]
    [TestCase(1, 0, 0)]
    [TestCase(-1, 0, 0)]
    public void Constructor(int years, int months, int days)
    {
      YearMonthDayAge sut = new YearMonthDayAge(years, months, days);
      Assert.AreEqual(years, sut.Years, "Years");
      Assert.AreEqual(months, sut.Months, "Months");
      Assert.AreEqual(days, sut.Days, "Days");
    }

    // OutOfRange
    [TestCase(1, 2, 31)]
    [TestCase(-1, -2, -31)]
    [TestCase(1, 12, 3)]
    [TestCase(-1, -12, -3)]
    [TestCase(9999, 2, 3)]
    [TestCase(-9999, -2, -3)]
    // Signs
    [TestCase(1, 2, -3)]
    [TestCase(-1, -2, 3)]
    [TestCase(1, 0, -3)]
    [TestCase(-1, 0, 3)]
    [TestCase(1, -2, 3)]
    [TestCase(-1, 2, -3)]
    [TestCase(1, -2, 0)]
    [TestCase(-1, 2, 0)]
    [TestCase(0, -2, 3)]
    [TestCase(0, 2, -3)]
    public void Constructor_exception(int years, int months, int days)
    {
      Assert.Catch<ArgumentException>(delegate () { new YearMonthDayAge(years, months, days); });
    }

    #endregion

    #region FromDates()

    [TestCase("20241102", "20241102", 0, 0, 0)]
    [TestCase("20241102", "20241103", 0, 0, 1)]
    [TestCase("20241102", "20241202", 0, 1, 0)]
    [TestCase("20241102", "20250102", 0, 2, 0)]
    [TestCase("20241102", "20250103", 0, 2, 1)]
    [TestCase("20241102", "20251101", 0, 11, 29)]
    [TestCase("20241102", "20251102", 1, 0, 0)]
    [TestCase("20240228", "20240229", 0, 0, 1)]
    [TestCase("20240228", "20240301", 0, 0, 2)]
    [TestCase("20240227", "20240302", 0, 0, 4)]
    [TestCase("00010101", "99991231", 9998, 11, 30)]
    public void FromDates(string sFirstDate, string sLastDate, int wantedYears, int wantedMonths, int wantedDays)
    {
      DateTime firstDate = Creators.DateTime(sFirstDate);
      DateTime lastDate = Creators.DateTime(sLastDate);
      YearMonthDayAge wanted1 = new YearMonthDayAge(wantedYears, wantedMonths, wantedDays);
      YearMonthDayAge wanted2 = new YearMonthDayAge(-wantedYears, -wantedMonths, -wantedDays);

      YearMonthDayAge sut1 = YearMonthDayAge.FromDates(firstDate, lastDate);
      Assert.AreEqual(wanted1, sut1, "#1");

      YearMonthDayAge sut2 = YearMonthDayAge.FromDates(lastDate, firstDate);
      Assert.AreEqual(wanted2, sut2, "#2");
    }

    #endregion

    #region Константы

    [Test]
    public void Zero()
    {
      YearMonthDayAge sut = YearMonthDayAge.Zero;

      Assert.AreEqual(0, sut.Years, "Years");
      Assert.AreEqual(0, sut.Months, "Months");
      Assert.AreEqual(0, sut.Days, "Days");
    }

    [Test]
    public void MinValue()
    {
      Assert.Less(YearMonthDayAge.MinValue.Years, 0, "Is negative");
      YearMonthDayAge dummy;
      Assert.Catch<ArgumentException>(delegate () { dummy = YearMonthDayAge.MinValue - new YearMonthDayAge(0, 0, 1); }, "Cannot be less");
    }

    [Test]
    public void MaxValue()
    {
      Assert.Greater(YearMonthDayAge.MaxValue.Years, 0, "Is positive");
      YearMonthDayAge dummy;
      Assert.Catch<ArgumentException>(delegate () { dummy = YearMonthDayAge.MaxValue + new YearMonthDayAge(0, 0, 1); }, "Cannot be greater");
    }

    #endregion

    #region Операции над YearMonthDayAge

    [TestCase("0:0:0")]
    [TestCase("1:2:3")]
    [TestCase("-1:-2:-3")]
    public void Operator_Neg(string sArg)
    {
      YearMonthDayAge arg = Creators.YearMonthDayAge(sArg);
      YearMonthDayAge res1 = -arg;
      Assert.AreEqual(-arg.Years, res1.Years, "Years");
      Assert.AreEqual(-arg.Months, res1.Months, "Months");
      Assert.AreEqual(-arg.Days, res1.Days, "Days");

      YearMonthDayAge res2 = -res1;
      Assert.AreEqual(arg, res2, "Twice inverted");
    }

    [TestCase("0:0:0", "0:0:0", "0:0:0")]
    [TestCase("1:2:3", "0:0:0", "1:2:3")]
    [TestCase("1:2:3", "-2:-3:-4", "-1:-1:-1")]
    [TestCase("1:2:3", "-1:-2:-3", "0:0:0")]
    [TestCase("1:2:3", "0:0:27", "1:2:30")]
    [TestCase("1:2:3", "0:0:28", "1:3:0")]
    [TestCase("1:7:3", "1:4:5", "2:11:8")]
    [TestCase("1:7:3", "1:5:5", "3:0:8")]
    [TestCase("10:5:6", "-1:-6:-7", "8:10:30")]
    public void Operator_Add_Sub_YearMonthDayAge(string sArg1, string sArg2, string sWanted)
    {
      YearMonthDayAge arg1 = Creators.YearMonthDayAge(sArg1);
      YearMonthDayAge arg2 = Creators.YearMonthDayAge(sArg2);
      YearMonthDayAge wanted = Creators.YearMonthDayAge(sWanted);

      YearMonthDayAge res1 = arg1 + arg2;
      Assert.AreEqual(wanted, res1, "operator + #1");

      YearMonthDayAge res2 = arg2 + arg1;
      Assert.AreEqual(wanted, res2, "operator + #2");

      YearMonthDayAge res3 = arg1 - (-arg2);
      Assert.AreEqual(wanted, res3, "operator - #3");

      YearMonthDayAge res4 = arg2 - (-arg1);
      Assert.AreEqual(wanted, res4, "operator - #4");
    }


    // Операторы сложения с датой / временем не являются обратимыми

    [TestCase("20241102", "0:0:0", "20241102")]
    [TestCase("20241102", "1:0:28", "20251130")]
    [TestCase("20241102", "1:0:29", "20251201")]
    [TestCase("20241102", "-1:-10:-1", "20230101")]
    [TestCase("20241102", "-1:-10:-2", "20221231")]
    [TestCase("20240301", "0:0:-1", "20240229")]
    [TestCase("20240301", "0:0:-28", "20240202")]
    [TestCase("20240301", "0:0:-29", "20240201")]
    [TestCase("20240301", "0:0:-30", "20240131")]
    [TestCase("20240301", "-1:0:-1", "20230228")]
    [TestCase("20240301", "-1:0:-28", "20230201")]
    [TestCase("20240301", "-1:0:-29", "20230131")]
    [TestCase("20240301", "-1:0:-30", "20230130")]
    public void Operator_Add_Sub_DateTime(string sArg1, string sArg2, string sWanted)
    {
      DateTime arg1 = Creators.DateTime(sArg1);
      YearMonthDayAge arg2 = Creators.YearMonthDayAge(sArg2);
      DateTime wanted = Creators.DateTime(sWanted);

      DateTime res1 = arg1 + arg2;
      Assert.AreEqual(wanted, res1, "operator +");

      DateTime res2 = arg1 - (-arg2);
      Assert.AreEqual(wanted, res2, "operator -");
    }

    #endregion

    #region Прочие свойства

    [TestCase("0:0:0", 0)]
    [TestCase("0:0:1", 1)]
    [TestCase("0:0:-1", -1)]
    [TestCase("0:1:0", 1)]
    [TestCase("0:-1:0", -1)]
    [TestCase("1:0:0", 1)]
    [TestCase("-1:0:0", -1)]
    [TestCase("1:2:3", 1)]
    [TestCase("-1:-2:-3", -1)]
    public void Sign(string sSUT, int wantedRes)
    {
      YearMonthDayAge sut = Creators.YearMonthDayAge(sSUT);
      Assert.AreEqual(wantedRes, sut.Sign);
    }

    #endregion

    #region Сравнение

    [TestCase("1:2:2", "1:2:1", 1)]
    [TestCase("1:2:2", "1:2:2", 0)]
    [TestCase("1:2:2", "1:2:3", -1)]
    [TestCase("0:0:0", "0:0:-1", 1)]
    [TestCase("0:0:0", "0:0:0", 0)]
    [TestCase("0:0:0", "0:0:1", -1)]
    public void Compare(string sArg1, string sArg2, int wantedRes)
    {
      YearMonthDayAge arg1 = Creators.YearMonthDayAge(sArg1);
      YearMonthDayAge arg2 = Creators.YearMonthDayAge(sArg2);

      int res1 = arg1.CompareTo(arg2);
      Assert.AreEqual(wantedRes, Math.Sign(res1), "CompareTo(YearMonthDayAge)");

      bool res2 = arg1.Equals(arg2);
      Assert.AreEqual(wantedRes==0, res2, "Equals(YearMonthDayAge)");

      bool res3 = arg1.Equals((object)arg2);
      Assert.AreEqual(wantedRes == 0, res3, "Equals(Object)");
    }

    #endregion
  }
}
