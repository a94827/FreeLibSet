// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using FreeLibSet.Calendar;

namespace FreeLibSet.Core
{
	partial class DataTools
	{
    #region ������� ������ � �����

#if DEBUG

    private static void CheckYear(int year)
    {
      if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
        throw new ArgumentOutOfRangeException("year", year, "��� ������ ���� � ��������� �� " +
          DateTime.MinValue.Year.ToString() + " �� " + DateTime.MaxValue.Year.ToString());
    }

    private static void CheckMonth(int month)
    {
      if (month < 1 || month > 12)
        throw new ArgumentOutOfRangeException("month", month, "����� ������ ���� � ��������� �� 1 �� 12");
    }

#endif

    /// <summary>
    /// ���������� ��������� ���� ������, ������������� ����� <paramref name="date"/>.
    /// </summary>
    /// <param name="date">����, ������������ �����</param>
    /// <returns>����� ������</returns>
    public static DateTime EndOfMonth(DateTime date)
    {
      return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    /// <summary>
    /// ���������� ��������� ���� ������ ��������� ����.
    /// ��� ������ ���� � ���������, ������� �������������� DateTime.
    /// ����� ������ ���� � ��������� �� 1 �� 12.
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <returns>����� ������</returns>
    public static DateTime EndOfMonth(int year, int month)
    {
#if DEBUG
      CheckYear(year);
      CheckMonth(month);
#endif
      return new DateTime(year, month, DateTime.DaysInMonth(year, month));
    }

    /// <summary>
    /// ���������� ������ ���� ������, ������������� ����� <paramref name="date"/>.
    /// </summary>
    /// <param name="date">����, ������������ �����</param>
    /// <returns>������ ������</returns>
    public static DateTime BottomOfMonth(DateTime date)
    {
      return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// ���������� ������ ���� ������ ��������� ����.
    /// ��� ������ ���� � ���������, ������� �������������� DateTime.
    /// ����� ������ ���� � ��������� �� 1 �� 12.
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <returns>������ ������</returns>
    public static DateTime BottomOfMonth(int year, int month)
    {
#if DEBUG
      CheckYear(year);
      CheckMonth(month);
#endif
      return new DateTime(year, month, 1);
    }

    /// <summary>
    /// ���������� 31 ������� ����, � �������� ��������� �������� ����.
    /// </summary>
    /// <param name="date">����, ������������ ���</param>
    /// <returns>����� ����</returns>
    public static DateTime EndOfYear(DateTime date)
    {
      return new DateTime(date.Year, 12, 31);
    }

    /// <summary>
    /// ���������� 31 ������� ��������� ����.
    /// ��� ������ ���� � ���������, ������� �������������� DateTime
    /// </summary>
    /// <param name="year">���</param>
    /// <returns>����� ����</returns>
    public static DateTime EndOfYear(int year)
    {
#if DEBUG
      CheckYear(year);
#endif
      return new DateTime(year, 12, 31);
    }

    /// <summary>
    /// ���������� ������ ������ ����, � �������� ��������� �������� ����.
    /// </summary>
    /// <param name="date">����, ������������ ���</param>
    /// <returns>������ ����</returns>
    public static DateTime BottomOfYear(DateTime date)
    {
      return new DateTime(date.Year, 1, 1);
    }

    /// <summary>
    /// ���������� ������ ������ ��������� ����.
    /// ��� ������ ���� � ���������, ������� �������������� DateTime
    /// </summary>
    /// <param name="year">���</param>
    /// <returns>������ ����</returns>
    public static DateTime BottomOfYear(int year)
    {
#if DEBUG
      CheckYear(year);
#endif
      return new DateTime(year, 1, 1);
    }

    /// <summary>
    /// ���������� ��������� ���� ��������, � �������� ��������� �������� ����.
    /// </summary>
    /// <param name="date">����, ������������ �������</param>
    /// <returns>����� ��������</returns>
    public static DateTime EndOfQuarter(DateTime date)
    {
      int q = (date.Month + 2) / 3; // 1-4
      return EndOfMonth(date.Year, q * 3);
    }

    /// <summary>
    /// ���������� ������ ���� ��������, � �������� ��������� �������� ����.
    /// </summary>
    /// <param name="date">����, ������������ �������</param>
    /// <returns>������ ��������</returns>
    public static DateTime BottomOfQuarter(DateTime date)
    {
      int q = (date.Month + 2) / 3; // 1-4
      return BottomOfMonth(date.Year, q * 3 - 2);
    }

    /// <summary>
    /// ���������� ��������� ���� ������ - �����������
    /// </summary>
    /// <param name="date">����, ��������� � ������, ��� ������� ����� ���������� ���������</param>
    /// <returns>����, ������� ��� ������ ��������</returns>
    public static DateTime EndOfWeekSunday(DateTime date)
    {
      /*
       * ���� ������ DayOfWeek ���������� ��� 0-��, 1-��, ... 6-��
       */

      DayOfWeek dow = date.DayOfWeek;
      if (dow == DayOfWeek.Sunday)
        return date;
      else
        return date.AddDays(7 - (int)dow);
    }

    /// <summary>
    /// ���������� ������ ���� ������ - �����������
    /// </summary>
    /// <param name="date">����, ��������� � ������, ��� ������� ����� ���������� ������</param>
    /// <returns>����, ������� ��� ������ ��������</returns>
    public static DateTime BottomOfWeekMonday(DateTime date)
    {
      DayOfWeek dow = date.DayOfWeek;
      if (dow == DayOfWeek.Sunday)
        return date.AddDays(-6);
      else
        return date.AddDays(1 - (int)dow);
    }

    /// <summary>
    /// ���������� ��������� ���� "���������" ������.
    /// ���� <paramref name="lastDayOfWeek"/> ������ �����������, ������� ��������� � EndOfWeekSunday().
    /// </summary>
    /// <param name="date">����, ��������� � ������, ��� ������� ����� ���������� ���������</param>
    /// <param name="lastDayOfWeek">��������� ���� ������</param>
    /// <returns>����, ������� ��� ������ ��������</returns>
    public static DateTime EndOfWeek(DateTime date, DayOfWeek lastDayOfWeek)
    {
      int delta = (int)lastDayOfWeek - (int)(date.DayOfWeek);
      if (delta < 0)
        delta += 7;
      return date.AddDays(delta);
    }

    /// <summary>
    /// ���������� ������ ���� "���������" ������
    /// </summary>
    /// <param name="date">����, ��������� � ������, ��� ������� ����� ���������� ������</param>
    /// <param name="firstDayOfWeek">��������� ���� ������</param>
    /// <returns>����, ������� ��� ������ ��������</returns>
    public static DateTime BottomOfWeek(DateTime date, DayOfWeek firstDayOfWeek)
    {
      int delta = (int)(date.DayOfWeek) - (int)firstDayOfWeek;
      if (delta < 0)
        delta += 7;
      return date.AddDays(-delta);
    }

    /// <summary>
    /// ���������� true, ���� ���� � ���� �������� ������ ���� ������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ������� ��� ������</returns>
    public static bool IsBottomOfMonth(DateTime date)
    {
      return date.Day == 1;
    }

    /// <summary>
    /// ���������� true, ���� ���� � ���� �������� ��������� ���� ������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ���������� ��� ������</returns>
    public static bool IsEndOfMonth(DateTime date)
    {
      return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }

    /// <summary>
    /// ���������� true, ���� ���� - 1 ������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ������� ��� ����</returns>
    public static bool IsBottomOfYear(DateTime date)
    {
      return date.Day == 1 && date.Month == 1;
    }

    /// <summary>
    /// ���������� true, ���� ���� - 31 �������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ���������� ��� ����</returns>
    public static bool IsEndOfYear(DateTime date)
    {
      return date.Month == 12 && date.Day == 31;
    }

    /// <summary>
    /// ���������� true, ���� ���� - 1 ������, 1 ������, 1 ���� ��� 1 �������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ������� ��� ��������</returns>
    public static bool IsBottomOfQuarter(DateTime date)
    {
      return ((date.Month - 1) % 3) == 0 && date.Day == 1;
    }

    /// <summary>
    /// ���������� true, ���� ���� - 31 �����, 30 ����, 30 �������� ��� 31 �������
    /// </summary>
    /// <param name="date">����������� ����</param>
    /// <returns>true ��� ���������� ��� ��������</returns>
    public static bool IsEndOfQuarter(DateTime date)
    {
      return (date.Month % 3) == 0 && date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }

    /// <summary>
    /// ���������� ������������ �� ���� ���
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>dt1 ��� dt2</returns>
    public static DateTime Max(DateTime dt1, DateTime dt2)
    {
      return (dt1 > dt2) ? dt1 : dt2;
    }

    /// <summary>
    /// ������������ �������� �� ���� ���. ���� ���� �� ��� �� ������, �� ������������
    /// ������ ����
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>dt1 ��� dt2</returns>
    public static Nullable<DateTime> Max(Nullable<DateTime> dt1, Nullable<DateTime> dt2)
    {
      if (dt1.HasValue)
      {
        if (dt2.HasValue)
          return (dt1 > dt2) ? dt1 : dt2;
        else
          return dt1;
      }
      else
        return dt2;
    }

    /// <summary>
    /// ���������� ����������� �� ���� ���
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>dt1 ��� dt2</returns>
    public static DateTime Min(DateTime dt1, DateTime dt2)
    {
      return (dt1 < dt2) ? dt1 : dt2;
    }

    /// <summary>
    /// ����������� �������� �� ���� ���. ���� ���� �� ��� �� ������, �� ������������
    /// ������ ����
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>dt1 ��� dt2</returns>
    public static Nullable<DateTime> Min(Nullable<DateTime> dt1, Nullable<DateTime> dt2)
    {
      if (dt1.HasValue)
      {
        if (dt2.HasValue)
          return (dt1 < dt2) ? dt1 : dt2;
        else
          return dt1;
      }
      else
        return dt2;
    }

    /// <summary>
    /// ���������� true, ���� � ���� ��� ���������� ����� � ���.
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>��������� ���������</returns>
    public static bool IsEqualYearAndMonth(DateTime dt1, DateTime dt2)
    {
      return dt1.Year == dt2.Year && dt1.Month == dt2.Month;
    }

    /// <summary>
    /// ���������� true, ���� � ���� ��� ���������� ����� � ���.
    /// ���� ��� ���� ������, ������������ true.
    /// ���� ������ ���� ���� ������, ������������ false.
    /// </summary>
    /// <param name="dt1">������ ������������ ����</param>
    /// <param name="dt2">������ ������������ ����</param>
    /// <returns>��������� ���������</returns>
    public static bool IsEqualYearAndMonth(Nullable<DateTime> dt1, Nullable<DateTime> dt2)
    {
      //if (dt2.HasValue != dt2.HasValue) 
      if (dt1.HasValue != dt2.HasValue) // 27.12.2020
        return false;
      if (dt1.HasValue)
        return IsEqualYearAndMonth(dt1.Value, dt2.Value);
      else
        return true; // ��� ���� ������
    }

    /// <summary>
    /// ���������� ����, � ������� ��� � ����� ������� �� ��������� ���� <paramref name="dt"/>,
    /// � ���� �������� ��������.
    /// ���� � ������ ��� ������� ����, �� ������������ ��������� ���� ������
    /// </summary>
    /// <param name="dt">����, �� ������� ������� ��� � �����</param>
    /// <param name="day">����</param>
    /// <returns>��������� ����</returns>
    public static DateTime CreateDateTime(DateTime dt, int day)
    {
      //return CreateDateTime(dt.Year, dt.Month, dt.Day);
      return CreateDateTime(dt.Year, dt.Month, day); // 01.01.2021
    }

    /// <summary>
    /// ������� ���� �� �����������.
    /// ���� � ������ ��� ������� ����, �� ������������ ��������� ���� ������.
    /// ���� ��� ��� ���������, ������������ ��� DateTime, ������������� ArgumentOutOfRangeException
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <param name="day">����</param>
    /// <returns>������ ����</returns>
    [DebuggerStepThrough]
    public static DateTime CreateDateTime(int year, int month, int day)
    {
      if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
        throw new ArgumentOutOfRangeException("Year");

      if (month < 1)
        month = 1;
      else if (month > 12)
        month = 12;

      if (day < 1)
        day = 1;
      else
        day = Math.Min(day, DateTime.DaysInMonth(year, month));

      return new DateTime(year, month, day);
    }


    #region GetNullableDateTime()

    /// <summary>
    /// ������� ����� ������ DateTime, ���� ����, ����� � ��� �������� ���������� ��������.
    /// ����� ������������ null
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <param name="day">����</param>
    /// <returns>������ DateTime ��� null</returns>
    public static DateTime? CreateDateTimeIfValid(int year, int month, int day)
    {
      if (IsValidYMD(year, month, day))
        return new DateTime(year, month, day);
      else
        return null;
    }

    /// <summary>
    /// ������� ����� ������ DateTime, ���� ���������� ���� � ������� �������� ���������� ��������.
    /// ����� ������������ null
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <param name="day">����</param>
    /// <param name="hour">����</param>
    /// <param name="minute">������</param>
    /// <param name="second">�������</param>
    /// <returns>������ DateTime ��� null</returns>
    public static DateTime? CreateDateTimeIfValid(int year, int month, int day, int hour, int minute, int second)
    {
      if (IsValidYMD(year, month, day) && IsValidHMS(hour, minute, second))
        return new DateTime(year, month, day, hour, minute, second);
      else
        return null;
    }

    /// <summary>
    /// ������� ����� ������ DateTime, ���� ���������� ���� � ������� �������� ���������� ��������.
    /// ����� ������������ null
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <param name="day">����</param>
    /// <param name="hour">����</param>
    /// <param name="minute">������</param>
    /// <param name="second">�������</param>
    /// <param name="millisecond">������������</param>
    /// <returns>������ DateTime ��� null</returns>
    public static DateTime? CreateDateTimeIfValid(int year, int month, int day, int hour, int minute, int second, int millisecond)
    {
      if (IsValidYMD(year, month, day) && IsValidHMS(hour, minute, second) && IsValidMS(millisecond))
        return new DateTime(year, month, day, hour, minute, second, millisecond);
      else
        return null;
    }

    private static bool IsValidYMD(int year, int month, int day)
    {
      if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
      {
        return day >= 1 && day <= DateTime.DaysInMonth(year, month);
      }
      else
        return false;
    }

    private static bool IsValidHMS(int hour, int minute, int second)
    {
      return hour >= 0 && hour <= 23 &&
        minute >= 0 && minute <= 59 &&
        second >= 0 && second <= 59;
    }

    private static bool IsValidMS(int millisecond)
    {
      return millisecond >= 0 && millisecond <= 999;
    }

    #endregion

    #endregion

    #region ��������� ���

    /// <summary>
    /// �������� ��������� ���� � ��������.
    /// ������� ������ ����, ����� �������������.
    /// </summary>
    /// <param name="testDate">����������� ����</param>
    /// <param name="hasFirstDate">���� ��������� ���� ���������</param>
    /// <param name="firstDate">��������� ���� ��������� ��� HasFirstDate=True</param>
    /// <param name="hasLastDate">���� �������� ���� ���������</param>
    /// <param name="lastDate">�������� ���� ��������� ��� HasLastDate=True</param>
    /// <returns>True, ���� ���� ����� ������ ���������, false, ���� ��� �� ������ � ��������</returns>
    public static bool DateInRange(DateTime testDate, bool hasFirstDate, DateTime firstDate, bool hasLastDate, DateTime lastDate)
    {
      Nullable<DateTime> dt1 = null;
      if (hasFirstDate)
        dt1 = firstDate;
      Nullable<DateTime> dt2 = null;
      if (hasLastDate)
        dt2 = lastDate;
      return DateInRange(testDate, dt1, dt2);
    }

    /// <summary>
    /// �������� ��������� ���� � ��������
    /// ������� ������ ����, ����� �������������
    /// </summary>
    /// <param name="testDate">����������� ����</param>
    /// <param name="firstDate">��������� ���� ���������</param>
    /// <param name="lastDate">�������� ���� ���������</param>
    /// <returns>True, ���� ���� ����� ������ ���������, false, ���� ��� �� ������ � ��������</returns>
    public static bool DateInRange(DateTime testDate, DateTime? firstDate, DateTime? lastDate)
    {
      testDate = testDate.Date;
      if (firstDate.HasValue)
      {
        if (testDate < firstDate.Value.Date)
          return false;
      }
      if (lastDate.HasValue)
      {
        if (testDate > lastDate.Value.Date)
          return false;
      }
      return true;
    }

    /// <summary>
    /// ��������� true, ���� ��������� ��� �������� � �������� ���.
    /// ��� ��������� �������� � ��������, ���� ����� ���z �� ���� ���� �� ����.
    /// </summary>
    /// <param name="testYear">����������� ���</param>
    /// <param name="firstDate">��������� ���� ���������</param>
    /// <param name="lastDate">�������� ���� ���������</param>
    /// <returns>true, ���� ��� �������� � ������</returns>
    public static bool YearInRange(int testYear, DateTime? firstDate, DateTime? lastDate)
    {
      if (firstDate.HasValue)
      {
        if (testYear < firstDate.Value.Year)
          return false;
      }
      if (lastDate.HasValue)
      {
        if (testYear > lastDate.Value.Year)
          return false;
      }
      return true;
    }

    /*
     * ������ MonthInRange ���, �.�. ��� ������ ���� ����������� � YearMonth
     */

    /// <summary>
    /// �������� ���� � ��������.
    /// ���� ������ ��������� ���� ��������� <paramref name="firstDate"/> � �������� ���� <paramref name="dt"/> ������� ��, ���� <paramref name="dt"/>
    /// �������� ������ ��������� ����. ���������� ����������� �������� ���� ��������� <paramref name="lastDate"/>
    /// </summary>
    /// <param name="dt">����������� � �������������� ����</param>
    /// <param name="firstDate">��������� ���� ���������</param>
    /// <param name="lastDate">�������� ���� ���������</param>
    public static void DateToRange(ref DateTime dt, DateTime? firstDate, DateTime? lastDate)
    {
      dt = dt.Date;
      if (firstDate.HasValue)
      {
        if (dt < firstDate.Value.Date)
          dt = firstDate.Value;
      }
      if (lastDate.HasValue)
      {
        if (dt > lastDate.Value.Date)
          dt = lastDate.Value;
      }
    }



    /// <summary>
    /// ������������ �� ��� ��������� ���
    /// ������������ true, ���� ���� ���� �� ���� ����� ����
    /// ��� ��������� �����������
    /// ����� �� �����������
    /// </summary>
    /// <param name="firstDate1">������ ������� ���������</param>
    /// <param name="lastDate1">��������� ������� ���������</param>
    /// <param name="firstDate2">������ ������� ���������</param>
    /// <param name="lastDate2">��������� ������� ���������</param>
    /// <returns>true, ���� ��������� ������������</returns>
    public static bool DateRangeCrossed(Nullable<DateTime> firstDate1, Nullable<DateTime> lastDate1,
      Nullable<DateTime> firstDate2, Nullable<DateTime> lastDate2)
    {
      MakeDateOnly(ref firstDate1);
      MakeDateOnly(ref lastDate1);
      MakeDateOnly(ref firstDate2);
      MakeDateOnly(ref lastDate2);

      if (firstDate1.HasValue && lastDate2.HasValue)
      {
        if (firstDate1.Value > lastDate2.Value)
          return false;
      }

      if (firstDate2.HasValue && lastDate1.HasValue)
      {
        if (firstDate2.Value > lastDate1.Value)
          return false;
      }
      return true;
    }

    /// <summary>
    /// �������� ������� �� ���� ���� ����/�����
    /// </summary>
    /// <param name="dt"></param>
    private static void MakeDateOnly(ref Nullable<DateTime> dt)
    {
      if (dt.HasValue)
        dt = dt.Value.Date;
    }


    /// <summary>
    /// ���������� true, ���� �������� ��� {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} �������
    /// ���������� � ��������� ��� {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}
    /// ��������� �� �������� �������������
    /// ����� �� �����������.
    /// </summary>
    /// <param name="firstDate1">������ ������� ���������</param>
    /// <param name="lastDate1">��������� ������� ���������</param>
    /// <param name="firstDate2">������ ������� ���������</param>
    /// <param name="lastDate2">��������� ������� ���������</param>
    /// <returns>true, ���� ������ �������� ������ �� ������</returns>
    public static bool DateRangeInRange(Nullable<DateTime> firstDate1, Nullable<DateTime> lastDate1,
      Nullable<DateTime> firstDate2, Nullable<DateTime> lastDate2)
    {
      MakeDateOnly(ref firstDate1);
      MakeDateOnly(ref lastDate1);
      MakeDateOnly(ref firstDate2);
      MakeDateOnly(ref lastDate2);

      if (firstDate2.HasValue)
      {
        if (firstDate1.HasValue)
        {
          if (firstDate1.Value < firstDate2.Value)
            return false;
        }
        else
          return false;
      }

      if (lastDate2.HasValue)
      {
        if (lastDate1.HasValue)
        {
          if (lastDate1.Value > lastDate2.Value)
            return false;
        }
        else
          return false;
      }

      return true;
    }

    /// <summary>
    /// ���������� ��������� ��� {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} ���, ����� � ���� �������
    /// ��������� �������� {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}.
    /// </summary>
    /// <param name="firstDate1">������ ������� ���������</param>
    /// <param name="lastDate1">��������� ������� ���������</param>
    /// <param name="firstDate2">������ ������� ���������</param>
    /// <param name="lastDate2">��������� ������� ���������</param>
    public static void GetDateRangeUnion(ref Nullable<DateTime> firstDate1, ref Nullable<DateTime> lastDate1,
      Nullable<DateTime> firstDate2, Nullable<DateTime> lastDate2)
    {
      if (firstDate1.HasValue)
      {
        if (firstDate2.HasValue)
        {
          if (firstDate2.Value < firstDate1.Value)
            firstDate1 = firstDate2.Value;
        }
        else
          firstDate1 = null;
      }

      if (lastDate1.HasValue)
      {
        if (lastDate2.HasValue)
        {
          if (lastDate2.Value > lastDate1.Value)
            lastDate1 = lastDate2.Value;
        }
        else
          lastDate1 = null;
      }
    }

    /// <summary>
    /// ������� ��������� ��� {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} ���, ����� �� �� �������
    /// �� ������� ��������� {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}
    /// ���� ��������� ��� ��
    /// ������������ (DateRangeCrossed(<paramref name="firstDate1"/>, <paramref name="lastDate1"/>, <paramref name="firstDate2"/>, <paramref name="lastDate2"/>)=0),
    /// �� ������������ �������� false, � <paramref name="firstDate1"/> � <paramref name="lastDate1"/> ����������� �������� null.
    /// </summary>
    /// <param name="firstDate1">������ ������� ���������</param>
    /// <param name="lastDate1">��������� ������� ���������</param>
    /// <param name="firstDate2">������ ������� ���������</param>
    /// <param name="lastDate2">��������� ������� ���������</param>
    /// <returns>true, ���� ����������� ���������� ��������� � false, ���� ���������
    /// �� ������������</returns>
    public static bool GetDateRangeCross(ref Nullable<DateTime> firstDate1, ref Nullable<DateTime> lastDate1,
      Nullable<DateTime> firstDate2, Nullable<DateTime> lastDate2)
    {
      if (firstDate1.HasValue)
      {
        if (firstDate2.HasValue)
        {
          if (firstDate2.Value > firstDate1.Value)
            firstDate1 = firstDate2.Value;
        }
      }
      else
        firstDate1 = firstDate2;

      if (lastDate1.HasValue)
      {
        if (lastDate2.HasValue)
        {
          if (lastDate2.Value < lastDate1.Value)
            lastDate1 = lastDate2.Value;
        }
      }
      else
        lastDate1 = lastDate2;

      if (firstDate1.HasValue && lastDate1.HasValue)
      {
        if (firstDate1.Value > lastDate1.Value)
        {
          firstDate1 = null;
          lastDate1 = null;
          return false;
        }
      }
      return true;
    }

    #endregion

    #region ��������� TimeSpan

    /// <summary>
    /// ���������� ������������ �������� �� ���� ���������� �������.
    /// ������������� �������� �����������.
    /// </summary>
    /// <param name="ts1">������ ��������</param>
    /// <param name="ts2">������ ��������</param>
    /// <returns>������������ ��������</returns>
    public static TimeSpan Max(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1 > ts2) ? ts1 : ts2;
    }

    /// <summary>
    /// ������������ �������� �� ���� ���������� �������. ���� ���� �� ���������� �� �����, �� ������������
    /// ������ ��������
    /// </summary>
    /// <param name="ts1">������ ������������ ��������</param>
    /// <param name="ts2">������ ������������ ��������</param>
    /// <returns>ts1 ��� ts2</returns>
    public static Nullable<TimeSpan> Max(Nullable<TimeSpan> ts1, Nullable<TimeSpan> ts2)
    {
      if (ts1.HasValue)
      {
        if (ts2.HasValue)
          return (ts1 > ts2) ? ts1 : ts2;
        else
          return ts1;
      }
      else
        return ts2;
    }

    /// <summary>
    /// ���������� ����������� �������� �� ���� ���������� �������.
    /// ������������� �������� �����������.
    /// </summary>
    /// <param name="ts1">������ ��������</param>
    /// <param name="ts2">������ ��������</param>
    /// <returns>����������� ��������</returns>
    public static TimeSpan Min(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1 < ts2) ? ts1 : ts2;
    }

    /// <summary>
    /// ����������� �������� �� ���� ���������� �������. ���� ���� �� ���������� �� �����, �� ������������
    /// ������ ��������
    /// </summary>
    /// <param name="ts1">������ ������������ ��������</param>
    /// <param name="ts2">������ ������������ ��������</param>
    /// <returns>ts1 ��� ts22</returns>
    public static Nullable<TimeSpan> Min(Nullable<TimeSpan> ts1, Nullable<TimeSpan> ts2)
    {
      if (ts1.HasValue)
      {
        if (ts2.HasValue)
          return (ts1 < ts2) ? ts1 : ts2;
        else
          return ts1;
      }
      else
        return ts2;
    }

    #endregion
  }
}
