using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV;

namespace ExtTools.tests
{
  /// <summary>
  /// ���������� �������� ��� ������������
  /// </summary>
  public static class Creators
  {
    #region ������� ���� � �������

    /// <summary>
    /// ������� DateTime �� ������ � ������� "��������"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime CreateDate(string s)
    {
      if (s.Length != 8)
        throw new ArgumentException();
      return DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string ToString(DateTime value)
    {
      return value.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// ��� ������ ������ ���������� null, ����� ���������� DateTime �� ������ � ������� "��������"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime? CreateNDate(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return CreateDate(s);
    }

    public static string ToString(DateTime? value)
    {
      if (value.HasValue)
        return ToString(value.Value);
      else
        return String.Empty;
    }

    /// <summary>
    /// ������� ������ DateRange �� ������ � ������� "��������-��������".
    /// ���������� DateRange.Empty ��� ������ ������
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateRange CreateDateRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DateRange.Empty;
      else
      {
        if (s.Length != 17 || s[8] != '-')
          throw new ArgumentException();

        return new DateRange(CreateDate(s.Substring(0, 8)), CreateDate(s.Substring(9, 8)));
      }
    }

    public static string ToString(DateRange value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return ToString(value.FirstDate) + "-" + ToString(value.LastDate);
    }


    /// <summary>
    /// ������� ������ MonthDay �� ����������������� ������ "����".
    /// ��� ������ ������ ��������� ������ ������
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDay CreateMonthDay(string s)
    {
      if (s.Length == 0)
        return MonthDay.Empty;

      if (s.Length != 4)
        throw new ArgumentException();

      int m = int.Parse(s.Substring(0, 2));
      int d = int.Parse(s.Substring(2, 2));
      return new MonthDay(m, d);
    }

    public static string ToString(MonthDay value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return value.Month.ToString("00") + value.Day.ToString("00");
    }

    /// <summary>
    /// ������� ������ MonthDayRange �� ������ � ������� "����-����".
    /// ��� ������ ������ ��������� MonthDayRange.Empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDayRange CreateMonthDayRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return MonthDayRange.Empty;
      else
      {
        if (s.Length != 9 || s[4] != '-')
          throw new ArgumentException();
        return new MonthDayRange(CreateMonthDay(s.Substring(0, 4)), CreateMonthDay(s.Substring(5, 4)));
      }
    }

    public static string ToString(MonthDayRange value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return ToString(value.First) + "-" + ToString(value.Last);
    }


    /// <summary>
    /// ������� ������ ��������� ������������� MonthDayRange � ���� "����-����,����-����".
    /// ��� ������� ������� ���������� ������ ������
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static string ToString(MonthDayRange[] a)
    {
      if (a.Length == 0)
        return string.Empty;
      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = ToString(a[i]);
      return String.Join(",", a2);
    }

    #endregion
  }
}
