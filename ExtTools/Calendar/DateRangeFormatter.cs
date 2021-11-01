using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Formatting;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Calendar
{

  /// <summary>
  /// ������������ ��������� ���.
  /// ������������ ������������ � ��������� �������� ���������.
  /// ����� �������� ����������������.
  /// ����������� ������ ����� ������ ���� �����������������.
  /// </summary>
  public class DateRangeFormatter
  {
    #region ����������� ������

    /// <summary>
    /// ��������� ������ ����.
    /// ������������������ ������ ������ � DateRangeFormatter ���������� DateTime.ToShortDateString() �
    /// DateTime.ToLongDateString()
    /// </summary>
    /// <param name="date">������������� ����</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public virtual string ToString(DateTime date, bool longFormat)
    {
      if (longFormat)
        return date.ToLongDateString();
      else
        return date.ToShortDateString();
    }

    /// <summary>
    /// ��������� ������ ����.
    /// ���� ���� �� ������, ���������� ������ ������.
    /// ��� ��������� ����, ����� ��� ������, ������������ ���������� ������ ��� ���� ��-nullable.
    /// </summary>
    /// <param name="date">������������� ���� ��� null</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public string ToString(DateTime? date, bool longFormat)
    {
      if (date.HasValue)
        return ToString(date.Value, longFormat);
      else
        return String.Empty;
    }

    /// <summary>
    /// ��������� ������ ��� ��������� ���.
    /// �������� ����� ���� ��������, ������������ ��� ��������� ��������
    /// ������ ������ � DateRangeFormatter ���������� ��� ������ ��� ����� ToString() ��� ����.
    /// ���������������� ����� ����� ����������� ����� ���������������� �������������, ��������,
    /// ��������� ���� � �������� ������: "01-31 ���� 2017 ����"
    /// </summary>
    /// <param name="firstDate">��������� ���� ��������� ��� null ��� (����)��������� ���������</param>
    /// <param name="lastDate">�������� ���� ��������� ��� null ��� (����)��������� ���������</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public virtual string ToString(DateTime? firstDate, DateTime? lastDate, bool longFormat)
    {
      if (firstDate.HasValue)
      {
        if (lastDate.HasValue)
        {
          if (firstDate.Value == lastDate.Value)
            return ToString(firstDate.Value, longFormat);
          else
            return ToString(firstDate.Value, longFormat) + "-" + ToString(lastDate.Value, longFormat);
        }
        else
          return ">=" + ToString(firstDate.Value, longFormat);
      }
      else
      {
        if (lastDate.HasValue)
          return "<=" + ToString(lastDate.Value, longFormat);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// ��������� ������ ��� ��������� �������.
    /// �������� ����� ���� ��������, ������������ ��� ��������� �������� (������������ YearMonth.Empty).
    /// ������ ������ � DateRangeFormatter ���������� ��� ������ ��� ����� DateTime.ToString("y") ��� ����.
    /// </summary>
    /// <param name="firstYM">��������� ����� ���������</param>
    /// <param name="lastYM">�������� ����� ���������</param>
    /// <returns>��������� �������������</returns>
    public virtual string ToString(YearMonth firstYM, YearMonth lastYM)
    {
      if (!firstYM.IsEmpty)
      {
        if (!lastYM.IsEmpty)
        {
          if (firstYM == lastYM)
            return firstYM.BottomOfMonth.ToString("y");
          else
            return firstYM.BottomOfMonth.ToString("y") + "-" + lastYM.EndOfMonth.ToString("y");
        }
        else
          return ">=" + firstYM.BottomOfMonth.ToString("y");
      }
      else
      {
        if (!lastYM.IsEmpty)
          return "<=" + lastYM.EndOfMonth.ToString("y");
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// ���������� true, ���� ���� ������� �����, ����� ����.
    /// ���������� false, ���� ������� ������ ���� ����, ����� �����
    /// </summary>
    private static bool IsMDOrder
    {
      get
      {
        //return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Contains("M/d");
        switch (FormatStringTools.DateFormatOrder)
        {
          case DateFormatYMDOrder.YMD:
          case DateFormatYMDOrder.MDY:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// ���������� ��������� ������������� ��� �������� "����� � ����"
    /// </summary>
    /// <param name="day">���� ��� ������</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public virtual string ToString(MonthDay day, bool longFormat)
    {
      if (day.IsEmpty)
        return String.Empty;

      if (longFormat)
        return day.ToString(IsMDOrder ? "MMMM dd" : "dd MMMM");
      else
        return day.ToString(IsMDOrder ? "MM/dd" : "dd/MM");
    }

    /// <summary>
    /// ���������� ��������� ������������� ��� ��������� ����
    /// </summary>
    /// <param name="range">��������</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public virtual string ToString(MonthDayRange range, bool longFormat)
    {
      if (range.IsEmpty)
        return String.Empty;

      if (range.First <= range.Last && range.First.Month == range.Last.Month)
      {
        // ����� ����������
        if (IsMDOrder)
          return ToString(range.First, longFormat) + "-" + range.Last.ToString("dd");
        else
          return range.First.ToString("dd") + "-" + ToString(range.Last, longFormat);
      }
      else
        return ToString(range.First, longFormat) + "-" + ToString(range.Last, longFormat);
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ��������� ������ ��� ��������� ���.
    /// </summary>
    /// <param name="range">�������� ���</param>
    /// <param name="longFormat">����� �� ������������ ������� ������ ������ (true) ��� ������� (false)</param>
    /// <returns>��������� �������������</returns>
    public string ToString(DateRange range, bool longFormat)
    {
      if (range.IsEmpty)
        return ToString((DateTime?)null, (DateTime?)null, longFormat);
      else
        return ToString(range.FirstDate, range.LastDate, longFormat);
    }

    #endregion

    #region ������� �����

    // ���������� ��������� ����������������.
    // ��������� � �������� XxxTextLength ����� ��������� ����������.
    // ��������, ��� ����������, ����� ������ CalcXxxTextLeng() ���������� ������.
    // ����������� �������� ���� Int32 ����� ������� ��������� ���������

    #region DateRange

    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� DateRange ��� Long=true
    /// </summary>
    public virtual int DateRangeLongTextLength
    {
      get
      {
        if (_DateRangeLongTextLength == 0)
          _DateRangeLongTextLength = CalcDateRangeLongTextLength();
        return _DateRangeLongTextLength;
      }
    }

    private int _DateRangeLongTextLength;

    private int CalcDateRangeLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        DateTime dt1 = new DateTime(2001, 1, 31);
        DateTime dt2 = new DateTime(2002, month, 28); // � ������ ����
        DateRange r = new DateRange(dt1, dt2);
        w = Math.Max(w, this.ToString(r, true).Length);
      }
      return w;
    }


    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� DateRange ��� Long=false
    /// ������ ��� 11 ��������
    /// </summary>
    public virtual int DateRangeShortTextLength
    {
      get
      {
        if (_DateRangeShortTextLength == 0)
          _DateRangeShortTextLength = CalcDateRangeShortTextLength();
        return _DateRangeShortTextLength;
      }
    }

    private int _DateRangeShortTextLength;

    private int CalcDateRangeShortTextLength()
    {
      DateTime dt1 = new DateTime(2001, 1, 31);
      DateTime dt2 = new DateTime(2002, 12, 31); // � ������ ���� 
      DateRange r = new DateRange(dt1, dt2);
      return this.ToString(r, false).Length;
    }

    #endregion

    #region MonthDay

    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� MonthDay ��� Long=true
    /// </summary>
    public virtual int MonthDayLongTextLength
    {
      get
      {
        if (_MonthDayLongTextLength == 0)
          _MonthDayLongTextLength = CalcMonthDayLongTextLength();
        return _MonthDayLongTextLength;
      }
    }

    private int _MonthDayLongTextLength;

    private int CalcMonthDayLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        MonthDay md = new MonthDay(month, 28);
        w = Math.Max(w, this.ToString(md, true).Length);
      }
      return w;
    }


    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� MonthDay ��� Long=false
    /// ������ ��� 5 �������� ("��.��")
    /// </summary>
    public virtual int MonthDayShortTextLength
    {
      get
      {
        if (_MonthDayShortTextLength == 0)
          _MonthDayShortTextLength = CalcMonthDayShortTextLength();
        return _MonthDayShortTextLength;
      }
    }

    private int _MonthDayShortTextLength;

    private int CalcMonthDayShortTextLength()
    {
      return this.ToString(MonthDay.EndOfYear, false).Length;
    }

    #endregion

    #region MonthDayRange

    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� MonthDayRange ��� Long=true
    /// </summary>
    public virtual int MonthDayRangeLongTextLength
    {
      get
      {
        if (_MonthDayRangeLongTextLength == 0)
          _MonthDayRangeLongTextLength = CalcMonthDayRangeLongTextLength();
        return _MonthDayRangeLongTextLength;
      }
    }

    private int _MonthDayRangeLongTextLength;

    private int CalcMonthDayRangeLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        MonthDay md1 = new MonthDay(month, 28);
        MonthDayRange r = new MonthDayRange(md1, MonthDay.EndOfYear);
        w = Math.Max(w, this.ToString(r, true).Length);
      }
      return w;
    }


    /// <summary>
    /// ���������� ������������ ���������� ��������, ������� ���������� ������� ToString() ��� ������� MonthDayRange ��� Long=false
    /// ������ ��� 11 �������� ("��.��-��.��")
    /// </summary>
    public virtual int MonthDayRangeShortTextLength
    {
      get
      {
        if (_MonthDayRangeShortTextLength == 0)
          _MonthDayRangeShortTextLength = CalcMonthDayRangeShortTextLength();
        return _MonthDayRangeShortTextLength;
      }
    }

    private int _MonthDayRangeShortTextLength;

    private int CalcMonthDayRangeShortTextLength()
    {
      return this.ToString(MonthDayRange.WholeYear, false).Length;
    }

    #endregion

    #endregion

    #region ����������� ���������

    /// <summary>
    /// �������������, ������������ �� ���������
    /// </summary>
    public static DateRangeFormatter Default
    {
      get { return _Default; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Default = value;
      }
    }
    private static DateRangeFormatter _Default = new DateRangeFormatter();

    #endregion
  }
}