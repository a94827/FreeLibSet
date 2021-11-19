using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;

namespace ExtTools_tests
{
  /// <summary>
  /// Генераторы объектов для тестирования
  /// </summary>
  public static class Creators
  {
    #region Объекты даты и времени

    /// <summary>
    /// Создает DateTime из строки в формате "ГГГГММДД"
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
    /// Для пустой строки возвращает null, иначе возвращает DateTime из строки в формате "ГГГГММДД"
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
    /// Создает объект DateRange из строки в формате "ГГГГММДД-ГГГГММДД".
    /// Возвращает DateRange.Empty для пустой строки
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
    /// Создает объект MonthDay из четырехсимвольной строки "ММДД".
    /// Для пустой строки возращает пустой объект
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
    /// Создает массив текстовых представлений MonthDay в виде "ММДД,ММДД".
    /// Для пустого массива возвращает пустую строку
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static string ToString(MonthDay[] a)
    {
      if (a.Length == 0)
        return string.Empty;
      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = ToString(a[i]);
      return String.Join(",", a2);
    }


    /// <summary>
    /// Создает объект MonthDayRange из строки в формате "ММДД-ММДД".
    /// Для пустой строки возращает MonthDayRange.Empty
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
    /// Создает массив текстовых представлений MonthDayRange в виде "ММДД-ММДД,ММДД-ММДД".
    /// Для пустого массива возвращает пустую строку
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
