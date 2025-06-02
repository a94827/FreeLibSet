using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace FreeLibSet.Tests
{
  /// <summary>
  /// Генераторы объектов для тестирования
  /// </summary>
  public static class Creators
  {
    #region Объекты даты и времени

    #region DateTime

    /// <summary>
    /// Создает DateTime из строки в формате "ГГГГММДД" или "ГГГГММДДЧЧММСС"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime DateTime(string s)
    {
      switch (s.Length)
      {
        case 8:
          return System.DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        case 14:
          return System.DateTime.ParseExact(s, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
        default:
          throw new ArgumentException("String \"" + s + "\" cannot be converted to DateTime");
      }
    }

    /// <summary>
    /// Для пустой строки возвращает null, иначе возвращает DateTime из строки в формате "ГГГГММДД" или "ГГГГММДДЧЧММСС"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime? NDateTime(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return DateTime(s);
    }

    /// <summary>
    /// Создает объект DateRange из строки в формате "ГГГГММДД-ГГГГММДД".
    /// Возвращает DateRange.Empty для пустой строки
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateRange DateRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return Calendar.DateRange.Empty;
      else
      {
        if (s.Length != 17 || s[8] != '-')
          throw new ArgumentException();

        return new DateRange(DateTime(s.Substring(0, 8)), DateTime(s.Substring(9, 8)));
      }
    }

    /// <summary>
    /// Создает массив объектов DateRange из строки в формате "ГГГГММДД-ГГГГММДД,ГГГГММДД-ГГГГММДД,...".
    /// Возвращает пустой массив для пустой строки.
    /// </summary>
    public static DateRange[] DateRangeArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new DateRange[0];

      string[] a1 = s.Split(',');
      DateRange[] a2 = new DateRange[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = DateRange(a1[i]);
      return a2;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразует строку вида "Ч:М:С" или "Д.Ч:М:С" в структуру TimeSpan.
    /// Для пустой строки возвращает null.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Nullable-структура</returns>
    public static TimeSpan? NTimeSpan(string s)
    {
      if (s.Length == 0)
        return null;
      else
        return TimeSpan.Parse(s);
    }

    #endregion

    #region YearMonth

    /// <summary>
    /// Создает объект YearMonth из строки вида "YYYYMM".
    /// Если строка - пустая, возвращает YearMonth.Empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static YearMonth YearMonth(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new YearMonth();
      else
      {
        if (s.Length != 6)
          throw new ArgumentException();
        int y = int.Parse(s.Substring(0, 4));
        int m = int.Parse(s.Substring(4, 2));
        return new YearMonth(y, m);
      }
    }

    /// <summary>
    /// Создает объект YearMonthRange из строки вида "YYYYMM-YYYYMM".
    /// Если строка пустая, то возвращает пустой диапазон
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static YearMonthRange YearMonthRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new YearMonthRange();
      else
      {
        if (s.Length != 13)
          throw new ArgumentException();
        YearMonth ym1 = YearMonth(s.Substring(0, 6));
        YearMonth ym2 = YearMonth(s.Substring(7, 6));
        return new YearMonthRange(ym1, ym2);
      }
    }


    #endregion

    #region MonthDay

    /// <summary>
    /// Создает объект MonthDay из четырехсимвольной строки "ММДД".
    /// Для пустой строки возращает пустой объект
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDay MonthDay(string s)
    {
      if (s.Length == 0)
        return Calendar.MonthDay.Empty;

      if (s.Length != 4)
        throw new ArgumentException();

      int m = int.Parse(s.Substring(0, 2));
      int d = int.Parse(s.Substring(2, 2));
      return new MonthDay(m, d);
    }

    public static MonthDay[] MonthDayArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new MonthDay[0];
      string[] a1 = s.Split(',');
      MonthDay[] a2 = new MonthDay[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = MonthDay(a1[i]);
      return a2;
    }

    /// <summary>
    /// Создает объект MonthDayRange из строки в формате "ММДД-ММДД".
    /// Для пустой строки возращает MonthDayRange.Empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDayRange MonthDayRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return Calendar.MonthDayRange.Empty;
      else
      {
        if (s.Length != 9 || s[4] != '-')
          throw new ArgumentException();
        return new MonthDayRange(MonthDay(s.Substring(0, 4)), MonthDay(s.Substring(5, 4)));
      }
    }

    public static MonthDayRange[] MonthDayRangeArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new MonthDayRange[0];

      string[] a1 = s.Split(',');
      MonthDayRange[] a2 = new MonthDayRange[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = MonthDayRange(a1[i]);
      return a2;
    }

    #endregion

    #region YearMonthDayAge

    /// <summary>
    /// Получает объект YearMonthDayAge из строки в формате "Г:М:Д"
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns></returns>
    public static YearMonthDayAge YearMonthDayAge(string s)
    {
      string[] a = s.Split(':');
      if (a.Length != 3)
        throw new ArgumentException();
      int years = StdConvert.ToInt32(a[0]);
      int months = StdConvert.ToInt32(a[1]);
      int days = StdConvert.ToInt32(a[2]);
      return new Calendar.YearMonthDayAge(years, months, days);
    }

    public static YearMonthDayAge? NYearMonthDayAge(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return YearMonthDayAge(s);
    }

    #endregion

    #endregion

    #region Guid

    public static Guid? NGuid(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return new Guid(s);
    }

    /// <summary>
    /// Возвращает массив из 16 байт с помощью метода <see cref="Guid.ToByteArray()"/>.
    /// Для пустой строки возвращает null.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static byte[] GuidBytes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        Guid g = new Guid(s);
        return g.ToByteArray();
      }
    }

    #endregion
  }

}
