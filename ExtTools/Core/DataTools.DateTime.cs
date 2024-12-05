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
    #region Функции работы с датой

#if DEBUG

    private static void CheckYear(int year)
    {
      if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
        throw new ArgumentOutOfRangeException("year", year, "Год должен быть в диапазоне от " +
          DateTime.MinValue.Year.ToString() + " до " + DateTime.MaxValue.Year.ToString());
    }

    private static void CheckMonth(int month)
    {
      if (month < 1 || month > 12)
        throw new ArgumentOutOfRangeException("month", month, "Месяц должен быть в диапазоне от 1 до 12");
    }

#endif

    /// <summary>
    /// Возвращает последний день месяца, определяемого датой <paramref name="date"/>.
    /// </summary>
    /// <param name="date">Дата, определяющая месяц</param>
    /// <returns>Конец месяца</returns>
    public static DateTime EndOfMonth(DateTime date)
    {
      return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    /// <summary>
    /// Возвращает последний день месяца заданного года.
    /// Год должен быть в диапазоне, который поддерживается DateTime.
    /// Месяц должен быть в диапазоне от 1 до 12.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>Конец месяца</returns>
    public static DateTime EndOfMonth(int year, int month)
    {
#if DEBUG
      CheckYear(year);
      CheckMonth(month);
#endif
      return new DateTime(year, month, DateTime.DaysInMonth(year, month));
    }

    /// <summary>
    /// Возвращает первый день месяца, определяемого датой <paramref name="date"/>.
    /// </summary>
    /// <param name="date">Дата, определяющая месяц</param>
    /// <returns>Начало месяца</returns>
    public static DateTime BottomOfMonth(DateTime date)
    {
      return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Возвращает первый день месяца заданного года.
    /// Год должен быть в диапазоне, который поддерживается DateTime.
    /// Месяц должен быть в диапазоне от 1 до 12.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>Начало месяца</returns>
    public static DateTime BottomOfMonth(int year, int month)
    {
#if DEBUG
      CheckYear(year);
      CheckMonth(month);
#endif
      return new DateTime(year, month, 1);
    }

    /// <summary>
    /// Возвращает 31 декабря года, к которому относится заданная дата.
    /// </summary>
    /// <param name="date">Дата, определяющая год</param>
    /// <returns>Конец года</returns>
    public static DateTime EndOfYear(DateTime date)
    {
      return new DateTime(date.Year, 12, 31);
    }

    /// <summary>
    /// Возвращает 31 декабря заданного года.
    /// Год должен быть в диапазоне, который поддерживается DateTime
    /// </summary>
    /// <param name="year">Год</param>
    /// <returns>Конец года</returns>
    public static DateTime EndOfYear(int year)
    {
#if DEBUG
      CheckYear(year);
#endif
      return new DateTime(year, 12, 31);
    }

    /// <summary>
    /// Возвращает первое января года, к которому относится заданная дата.
    /// </summary>
    /// <param name="date">Дата, определяющая год</param>
    /// <returns>Начало года</returns>
    public static DateTime BottomOfYear(DateTime date)
    {
      return new DateTime(date.Year, 1, 1);
    }

    /// <summary>
    /// Возвращает первое января заданного года.
    /// Год должен быть в диапазоне, который поддерживается <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">Год</param>
    /// <returns>Начало года</returns>
    public static DateTime BottomOfYear(int year)
    {
#if DEBUG
      CheckYear(year);
#endif
      return new DateTime(year, 1, 1);
    }

    /// <summary>
    /// Возвращает последний день квартала, к которому относится заданная дата.
    /// </summary>
    /// <param name="date">Дата, определяющая квартал</param>
    /// <returns>Конец квартала</returns>
    public static DateTime EndOfQuarter(DateTime date)
    {
      int q = (date.Month + 2) / 3; // 1-4
      return EndOfMonth(date.Year, q * 3);
    }

    /// <summary>
    /// Возвращает первый день квартала, к которому относится заданная дата.
    /// </summary>
    /// <param name="date">Дата, определяющая квартал</param>
    /// <returns>Начало квартала</returns>
    public static DateTime BottomOfQuarter(DateTime date)
    {
      int q = (date.Month + 2) / 3; // 1-4
      return BottomOfMonth(date.Year, q * 3 - 2);
    }

    /// <summary>
    /// Возвращает последний день недели - воскресенье
    /// </summary>
    /// <param name="date">Дата, относящая к неделе, для которой нужно определить окончание</param>
    /// <returns>Дата, большая или равная заданной</returns>
    public static DateTime EndOfWeekSunday(DateTime date)
    {
      /*
       * День недели DayOfWeek кодируется как 0-ВС, 1-ПН, ... 6-СБ
       */

      DayOfWeek dow = date.DayOfWeek;
      if (dow == DayOfWeek.Sunday)
        return date;
      else
        return date.AddDays(7 - (int)dow);
    }

    /// <summary>
    /// Возвращает первый день недели - понедельник
    /// </summary>
    /// <param name="date">Дата, относящая к неделе, для которой нужно определить начало</param>
    /// <returns>Дата, меньшая или равная заданной</returns>
    public static DateTime BottomOfWeekMonday(DateTime date)
    {
      DayOfWeek dow = date.DayOfWeek;
      if (dow == DayOfWeek.Sunday)
        return date.AddDays(-6);
      else
        return date.AddDays(1 - (int)dow);
    }

    /// <summary>
    /// Возвращает последний день "плавающей" недели.
    /// Если <paramref name="lastDayOfWeek"/> задает воскресенье, функция совпадает с EndOfWeekSunday().
    /// </summary>
    /// <param name="date">Дата, относящая к неделе, для которой нужно определить окончание</param>
    /// <param name="lastDayOfWeek">Последний день недели</param>
    /// <returns>Дата, большая или равная заданной</returns>
    public static DateTime EndOfWeek(DateTime date, DayOfWeek lastDayOfWeek)
    {
      int delta = (int)lastDayOfWeek - (int)(date.DayOfWeek);
      if (delta < 0)
        delta += 7;
      return date.AddDays(delta);
    }

    /// <summary>
    /// Возвращает первый день "плавающей" недели
    /// </summary>
    /// <param name="date">Дата, относящая к неделе, для которой нужно определить начало</param>
    /// <param name="firstDayOfWeek">Последний день недели</param>
    /// <returns>Дата, меньшая или равная заданной</returns>
    public static DateTime BottomOfWeek(DateTime date, DayOfWeek firstDayOfWeek)
    {
      int delta = (int)(date.DayOfWeek) - (int)firstDayOfWeek;
      if (delta < 0)
        delta += 7;
      return date.AddDays(-delta);
    }

    /// <summary>
    /// Возвращает true, если день в дате является первым днем месяца
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для первого дня месяца</returns>
    public static bool IsBottomOfMonth(DateTime date)
    {
      return date.Day == 1;
    }

    /// <summary>
    /// Возвращает true, если день в дате является последним днем месяца
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для последнего дня месяца</returns>
    public static bool IsEndOfMonth(DateTime date)
    {
      return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }

    /// <summary>
    /// Возвращает true, если дата - 1 января
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для первого дня года</returns>
    public static bool IsBottomOfYear(DateTime date)
    {
      return date.Day == 1 && date.Month == 1;
    }

    /// <summary>
    /// Возвращает true, если дата - 31 декабря
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для последнего дня года</returns>
    public static bool IsEndOfYear(DateTime date)
    {
      return date.Month == 12 && date.Day == 31;
    }

    /// <summary>
    /// Возвращает true, если дата - 1 января, 1 апреля, 1 июля или 1 октября
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для первого дня квартала</returns>
    public static bool IsBottomOfQuarter(DateTime date)
    {
      return ((date.Month - 1) % 3) == 0 && date.Day == 1;
    }

    /// <summary>
    /// Возвращает true, если дата - 31 марта, 30 июня, 30 сентября или 31 декабря
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true для последнего дня квартала</returns>
    public static bool IsEndOfQuarter(DateTime date)
    {
      return (date.Month % 3) == 0 && date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }

    /// <summary>
    /// Возвращает максимальную из двух дат
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>dt1 или dt2</returns>
    public static DateTime Max(DateTime dt1, DateTime dt2)
    {
      return (dt1 > dt2) ? dt1 : dt2;
    }

    /// <summary>
    /// Максимальное значение из двух дат. Если одна из дат не задана, то возвращается
    /// другая дата
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>dt1 или dt2</returns>
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
    /// Возвращает минимальную из двух дат
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>dt1 или dt2</returns>
    public static DateTime Min(DateTime dt1, DateTime dt2)
    {
      return (dt1 < dt2) ? dt1 : dt2;
    }

    /// <summary>
    /// Минимальное значение из двух дат. Если одна из дат не задана, то возвращается
    /// другая дата.
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>dt1 или dt2</returns>
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
    /// Возвращает true, если у двух дат одинаковые месяц и год.
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>Результат сравнения</returns>
    public static bool IsEqualYearAndMonth(DateTime dt1, DateTime dt2)
    {
      return dt1.Year == dt2.Year && dt1.Month == dt2.Month;
    }

    /// <summary>
    /// Возвращает true, если у двух дат одинаковые месяц и год.
    /// Если обе даты пустые, возвращается true.
    /// Если только одна дата пустая, возвращается false.
    /// </summary>
    /// <param name="dt1">Первая сравниваемая дата</param>
    /// <param name="dt2">Вторая сравниваемая дата</param>
    /// <returns>Результат сравнения</returns>
    public static bool IsEqualYearAndMonth(Nullable<DateTime> dt1, Nullable<DateTime> dt2)
    {
      //if (dt2.HasValue != dt2.HasValue) 
      if (dt1.HasValue != dt2.HasValue) // 27.12.2020
        return false;
      if (dt1.HasValue)
        return IsEqualYearAndMonth(dt1.Value, dt2.Value);
      else
        return true; // обе даты пустые
    }

    /// <summary>
    /// Возвращает дату, у которой год и месяц берутся из указанной даты <paramref name="dt"/>,
    /// а день задается отдельно.
    /// Если в месяце нет столько дней, то возвращается последний день месяца.
    /// </summary>
    /// <param name="dt">Дата, из которой берутся год и месяц</param>
    /// <param name="day">День</param>
    /// <returns>Созданная дата</returns>
    public static DateTime CreateDateTime(DateTime dt, int day)
    {
      //return CreateDateTime(dt.Year, dt.Month, dt.Day);
      return CreateDateTime(dt.Year, dt.Month, day); // 01.01.2021
    }

    /// <summary>
    /// Создает дату из компонентов.
    /// Если в месяце нет столько дней, то возвращается последний день месяца.
    /// Если год вне диапазона, разрешенного для <see cref="DateTime"/>, выбрасывается <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    /// <returns>Объект даты</returns>
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
    /// Создает новый объект <see cref="DateTime"/>, если день, месяц и год содержат допустимое значение.
    /// Иначе возвращается null.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    /// <returns>Объект <see cref="DateTime"/> или null</returns>
    public static DateTime? CreateDateTimeIfValid(int year, int month, int day)
    {
      if (IsValidYMD(year, month, day))
        return new DateTime(year, month, day);
      else
        return null;
    }

    /// <summary>
    /// Создает новый объект <see cref="DateTime"/>, если компоненты даты и времени содержат допустимые значение.
    /// Иначе возвращается null.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    /// <param name="hour">Часы</param>
    /// <param name="minute">Минуты</param>
    /// <param name="second">Секунды</param>
    /// <returns>Объект DateTime или null</returns>
    public static DateTime? CreateDateTimeIfValid(int year, int month, int day, int hour, int minute, int second)
    {
      if (IsValidYMD(year, month, day) && IsValidHMS(hour, minute, second))
        return new DateTime(year, month, day, hour, minute, second);
      else
        return null;
    }

    /// <summary>
    /// Создает новый объект <see cref="DateTime"/>, если компоненты даты и времени содержат допустимые значение.
    /// Иначе возвращается null.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    /// <param name="hour">Часы</param>
    /// <param name="minute">Минуты</param>
    /// <param name="second">Секунды</param>
    /// <param name="millisecond">Миллисекунды</param>
    /// <returns>Объект <see cref="DateTime"/> или null</returns>
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

    #region Интервалы дат

    /// <summary>
    /// Проверка попадания даты в интервал.
    /// Берутся только даты, время отбрасывается.
    /// </summary>
    /// <param name="testDate">Проверяемая дата</param>
    /// <param name="hasFirstDate">Есть начальная дата диапазона</param>
    /// <param name="firstDate">Начальная дата диапазона при <paramref name="hasFirstDate"/>=True</param>
    /// <param name="hasLastDate">Есть конечная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона при <paramref name="hasLastDate"/>=True</param>
    /// <returns>True, если дата лежит внутри диапазона, false, если она не входит в интервал</returns>
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
    /// Проверка попадания даты в интервал.
    /// Берутся только даты, время отбрасывается.
    /// </summary>
    /// <param name="testDate">Проверяемая дата</param>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    /// <returns>True, если дата лежит внутри диапазона, false, если она не входит в интервал</returns>
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
    /// Возвращет true, если указанный год попадает в интервал дат.
    /// Год считается попавшим в диапазон, если попал хотя бы один день из года.
    /// </summary>
    /// <param name="testYear">Проверяемый год</param>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    /// <returns>true, если год попадает в диазон</returns>
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
     * Метода MonthInRange нет, т.к. это должно быть реализовано в YearMonth
     */

    /// <summary>
    /// Помещает дату в диапазон.
    /// Если задана начальная дата диапазона <paramref name="firstDate"/> и заданная дата <paramref name="dt"/> меньшее ее, дата <paramref name="dt"/>
    /// делается равной начальной дате. Аналогично учитывается конечная дата диапазона <paramref name="lastDate"/>.
    /// </summary>
    /// <param name="dt">Проверяемая и корректируемая дата</param>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
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
    /// Пересекаются ли два интервала дат.
    /// Возвращается true, если есть хотя бы один общий день.
    /// Оба интервала равноправны.
    /// Время не учитывается.
    /// Интервалы могут быть открытыми или полуоткрытыми.
    /// </summary>
    /// <param name="firstDate1">Начало первого интервала</param>
    /// <param name="lastDate1">Окончание первого интервала</param>
    /// <param name="firstDate2">Начало второго интервала</param>
    /// <param name="lastDate2">Окончание второго интервала</param>
    /// <returns>true, если интервалы пересекаются</returns>
    public static bool DateRangesCrossed(Nullable<DateTime> firstDate1, Nullable<DateTime> lastDate1,
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
    /// Удаление времени из поля типа дата/время
    /// </summary>
    /// <param name="dt"></param>
    private static void MakeDateOnly(ref Nullable<DateTime> dt)
    {
      if (dt.HasValue)
        dt = dt.Value.Date;
    }


    /// <summary>
    /// Use DateRangesCrossed()
    /// </summary>
    /// <param name="firstDate1"></param>
    /// <param name="lastDate1"></param>
    /// <param name="firstDate2"></param>
    /// <param name="lastDate2"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Method renamed to DateRangesCrossed()")]
    public static bool DateRangeCrossed(Nullable<DateTime> firstDate1, Nullable<DateTime> lastDate1,
      Nullable<DateTime> firstDate2, Nullable<DateTime> lastDate2)
    {
      return DateRangesCrossed(firstDate1, lastDate1, firstDate2, lastDate2);
    }


    /// <summary>
    /// Возвращает true, если интервал дат {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} целиком
    /// помещается в интервале дат {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}
    /// Интервалы не являются равноправными.
    /// Время не учитывается.
    /// </summary>
    /// <param name="firstDate1">Начало первого интервала</param>
    /// <param name="lastDate1">Окончание первого интервала</param>
    /// <param name="firstDate2">Начало второго интервала</param>
    /// <param name="lastDate2">Окончание второго интервала</param>
    /// <returns>true, если первый интервал входит во второй</returns>
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
    /// Расширение интервала дат {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} так, чтобы в него целиком
    /// помещался интервал {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}.
    /// </summary>
    /// <param name="firstDate1">Начало первого интервала</param>
    /// <param name="lastDate1">Окончание первого интервала</param>
    /// <param name="firstDate2">Начало второго интервала</param>
    /// <param name="lastDate2">Окончание второго интервала</param>
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
    /// Сужение интервала дат {<paramref name="firstDate1"/>, <paramref name="lastDate1"/>} так, чтобы он не выходил
    /// за пределы интервала {<paramref name="firstDate2"/>, <paramref name="lastDate2"/>}
    /// Если интервалы дат не
    /// пересекаются (DateRangeCrossed(<paramref name="firstDate1"/>, <paramref name="lastDate1"/>, <paramref name="firstDate2"/>, <paramref name="lastDate2"/>)=0),
    /// то возвращается значение false, а <paramref name="firstDate1"/> и <paramref name="lastDate1"/> приобретают значение null.
    /// </summary>
    /// <param name="firstDate1">Начало первого интервала</param>
    /// <param name="lastDate1">Окончание первого интервала</param>
    /// <param name="firstDate2">Начало второго интервала</param>
    /// <param name="lastDate2">Окончание второго интервала</param>
    /// <returns>true, если пересечение интервалов выполнено и false, если интервалы
    /// не пересекаются</returns>
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

    #region Сравнение TimeSpan

    /// <summary>
    /// Возвращает максимальное значение из двух интервалов времени.
    /// Отрицательные значения допускаются.
    /// </summary>
    /// <param name="ts1">Первый интервал</param>
    /// <param name="ts2">Второй интервал</param>
    /// <returns>Максимальное значение</returns>
    public static TimeSpan Max(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1 > ts2) ? ts1 : ts2;
    }

    /// <summary>
    /// Максимальное значение из двух интервалов времени. Если один из интервалов не задан, то возвращается
    /// другой интервал
    /// </summary>
    /// <param name="ts1">Первый сравниваемый интервал</param>
    /// <param name="ts2">Второй сравниваемый интервал</param>
    /// <returns><paramref name="ts1"/> или <paramref name="ts2"/></returns>
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
    /// Возвращает минимальное значение из двух интервалов времени.
    /// Отрицательные значения допускаются.
    /// </summary>
    /// <param name="ts1">Первый интервал</param>
    /// <param name="ts2">Второй интервал</param>
    /// <returns>Минимальное значение</returns>
    public static TimeSpan Min(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1 < ts2) ? ts1 : ts2;
    }

    /// <summary>
    /// Минимальное значение из двух интервалов времени. Если один из интервалов не задан, то возвращается
    /// другой интервал
    /// </summary>
    /// <param name="ts1">Первый сравниваемый интервал</param>
    /// <param name="ts2">Второй сравниваемый интервал</param>
    /// <returns><paramref name="ts1"/> или <paramref name="ts2"/></returns>
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
