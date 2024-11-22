// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Calendar
{
  /// <summary>
  /// <para>
  /// Количество лет, месяцев и дней.
  /// </para>
  /// <para>
  /// Поддерживаются положительные, отрицательные и нулевые значения.
  /// При этом знак у всех компонентов должен быть либо неотрицательный, либо неположительный.
  /// </para>
  /// <para>
  /// Диапазон допустимых значений соответствует <see cref="DateRange.Whole"/> (01.01.0001-31.12.9999), 
  /// чтобы свойство <see cref="DateRange.Age"/> не выбрасывало исключение. Это составляет 9999 лет ровно
  /// </para>
  /// <para>
  /// Структура однократной записи.
  /// </para>
  /// </summary>
  public struct YearMonthDayAge : IComparable<YearMonthDayAge>, IEquatable<YearMonthDayAge>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру
    /// </summary>
    /// <param name="years">Количество лет</param>
    /// <param name="months">Количество месяцев</param>
    /// <param name="days">Количество дней</param>
    public YearMonthDayAge(int years, int months, int days)
    {
      if (Math.Abs(years) > 9998)
      {
        if (Math.Abs(years) == 9999)
        {
          if (months != 0 || days != 0)
            throw new ArgumentException("years", "Значение 9999 допускается, только если months=0 и days==0");
        }
        else
          throw new ArgumentOutOfRangeException("years", years, "Число лет должно быть не больше 9999");
      }
      if (Math.Abs(months) > 11)
        throw new ArgumentOutOfRangeException("months", months, "Число месяцев должно быть в диапазоне от 0 до 11");
      if (Math.Abs(days) > 30)
        throw new ArgumentOutOfRangeException("days", days, "Число дней должно быть в диапазоне от 0 до 30");


      int sign = 0;
      PrepareSign(ref years, ref sign);
      PrepareSign(ref months, ref sign);
      PrepareSign(ref days, ref sign);

      _Value = (sign < 0 ? SIGN_MASK : 0) |
        (years << YEARS_SHIFT) |
        (months << MONTHS_SHIFT) |
        (days << DAYS_SHIFT);
    }

    private static void PrepareSign(ref int value, ref int sign)
    {
      if (value == 0)
        return;

      int s = Math.Sign(value);
      if (sign == 0)
        sign = s;
      else if (s != sign)
        throw new ArgumentException("Не допускается смешение отрицательных и положительных компонентов");

      if (s < 0)
        value = -value;
    }

    private YearMonthDayAge(int value)
    {
      _Value = value;
    }

    #endregion

    #region Основные свойства

    // Внутреннее представление.
    // Данные хранятся в единственном поле типа Int32
    // Биты:
    // 31 - знак интервала
    // 16-30 год
    // 8-15 месяц
    // 0-7 день


    private readonly int _Value;

    private const int SIGN_MASK = unchecked((int)0x80000000);
    private const int YEARS_MASK = 0x7FFF0000;
    private const int MONTHS_MASK = 0x0000FF00;
    private const int DAYS_MASK = 0x000000FF;

    private const int YEARS_SHIFT = 16;
    private const int MONTHS_SHIFT = 8;
    private const int DAYS_SHIFT = 0;

    private bool IsNegative { get { return (_Value & SIGN_MASK) != 0; } }

    /// <summary>
    /// Количество лет (-9999:0:9999)
    /// </summary>
    public int Years
    {
      get
      {
        return IsNegative ?
          -((_Value & YEARS_MASK) >> YEARS_SHIFT) :
          ((_Value & YEARS_MASK) >> YEARS_SHIFT);
      }
    }

    /// <summary>
    /// Количество месяцев (-11:0:11)
    /// </summary>
    public int Months
    {
      get
      {
        return IsNegative ?
          -((_Value & MONTHS_MASK) >> MONTHS_SHIFT) :
          ((_Value & MONTHS_MASK) >> MONTHS_SHIFT);
      }
    }


    /// <summary>
    /// Количество дней (-30:0:30)
    /// </summary>
    public int Days
    {
      get
      {
        return IsNegative ?
          -((_Value & DAYS_MASK) >> DAYS_SHIFT) :
          ((_Value & DAYS_MASK) >> DAYS_SHIFT);
      }
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Years.ToString() + "y," + Months.ToString() + "m," + Days.ToString() + "d";
    }

    #endregion

    #region Дополнительные методы создания

    /// <summary>
    /// Создает объект, содержащий разницу между первой и второй датой.
    /// Значение будет положительным, если <paramref name="lastDate"/> больше, чем <paramref name="firstDate"/>,
    /// отрицательным, если если <paramref name="lastDate"/> меньше, чем <paramref name="firstDate"/>,
    /// и нулевым, если даты совпадают
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    public static YearMonthDayAge FromDates(DateTime firstDate, DateTime lastDate)
    {
      firstDate = firstDate.Date;
      lastDate = lastDate.Date;
      bool isNeg = false;
      if (firstDate > lastDate)
      {
        DataTools.Swap<DateTime>(ref firstDate, ref lastDate);
        isNeg = true;
      }

      int years = lastDate.Year - firstDate.Year;
      int months = lastDate.Month - firstDate.Month;
      int days = lastDate.Day - firstDate.Day;
      if (days < 0)
      {
        months--;
        days = DateTime.DaysInMonth(firstDate.Year, firstDate.Month) + days;
      }
      if (months < 0)
      {
        years--;
        months = 12 + months;
      }

      if (isNeg)
      {
        years = -years;
        months = -months;
        days = -days;
      }

      return new YearMonthDayAge(years, months, days);
    }

    /// <summary>
    /// Нулевое значение
    /// </summary>
    public static YearMonthDayAge Zero { get { return new YearMonthDayAge(); } }

    /// <summary>
    /// Максимально возможное значение
    /// </summary>
    public static YearMonthDayAge MaxValue { get { return new YearMonthDayAge(9999, 0, 0); } }

    /// <summary>
    /// Минимально возможное значение
    /// </summary>
    public static YearMonthDayAge MinValue { get { return new YearMonthDayAge(-9999, 0, 0); } }

    #endregion

    #region Операции над YearMonthDayAge

    /// <summary>
    /// Возвращает объект с инвертированными значениями свойств <see cref="Years"/>, <see cref="Months"/> и <see cref="Days"/>.
    /// Оператор является обратимым.
    /// </summary>
    /// <param name="arg">Исходный объект</param>
    /// <returns>Инвертированный объект</returns>
    public static YearMonthDayAge operator -(YearMonthDayAge arg)
    {
      return new YearMonthDayAge(-(arg.Years), -(arg.Months), -(arg.Days));
    }

    /// <summary>
    /// Складывает значения двух объектов.
    /// Оператор является обратимым.
    /// </summary>
    /// <param name="arg1">Первый объект</param>
    /// <param name="arg2">Второй объект</param>
    /// <returns>Результат</returns>
    public static YearMonthDayAge operator +(YearMonthDayAge arg1, YearMonthDayAge arg2)
    {
      if (arg1._Value == 0)
        return arg2;
      if (arg2._Value == 0)
        return arg1;

      int years = arg1.Years + arg2.Years;
      int months = arg1.Months + arg2.Months;
      int days = arg1.Days + arg2.Days;

      // определяем знак
      int sign = Math.Sign(years);
      if (sign == 0)
      {
        sign = Math.Sign(months);
        if (sign == 0)
        {
          sign = Math.Sign(days);
          if (sign == 0)
            return Zero;
        }
      }

      if (sign > 0)
        return DoGetPositive(years, months, days);
      else
      return -DoGetPositive(-years, -months, -days);

    }

    private static YearMonthDayAge DoGetPositive(int years, int months, int days)
    {
      if (days > 30)
      {
        days -= 31;
        months++;
      }
      else if (days < 0)
      {
        days += 31;
        months--;
      }

      if (months > 11)
      {
        months -= 12;
        years++;
      }
      else if (months < 0)
      {
        months += 12;
        years--;
      }
      return new YearMonthDayAge(years, months, days);
    }

    /// <summary>
    /// Вычитает значения двух объектов.
    /// Оператор является обратимым.
    /// </summary>
    /// <param name="arg1">Первый объект</param>
    /// <param name="arg2">Второй объект</param>
    /// <returns>Результат</returns>
    public static YearMonthDayAge operator -(YearMonthDayAge arg1, YearMonthDayAge arg2)
    {
      return arg1 + (-arg2);
    }

    /// <summary>
    /// Возвращает знак: +1 для положительного значения, -1 для отрицательного, 0 для нулевого
    /// </summary>
    public int Sign
    {
      get { return Math.Sign(_Value); }
    }

    #endregion

    #region Операции сложения и вычитания с датой

    /// <summary>
    /// Добавляет к <see cref="DateTime"/> указанное значение.
    /// Оператор не является обратимым.
    /// </summary>
    /// <param name="arg1">Исходная дата</param>
    /// <param name="arg2">Добавляемое значение</param>
    /// <returns>Новая дата</returns>
    public static DateTime operator +(DateTime arg1, YearMonthDayAge arg2)
    {
      if (arg2._Value == 0)
        return arg1;
      return arg1.AddYears(arg2.Years).AddMonths(arg2.Months).AddDays(arg2.Days);
    }

    /// <summary>
    /// Вычитает из <see cref="DateTime"/> указанное значение.
    /// Оператор не является обратимым.
    /// </summary>
    /// <param name="arg1">Исходная дата</param>
    /// <param name="arg2">Вычитаемое значение</param>
    /// <returns>Новая дата</returns>
    public static DateTime operator -(DateTime arg1, YearMonthDayAge arg2)
    {
      if (arg2._Value == 0)
        return arg1;
      return arg1.AddYears(-arg2.Years).AddMonths(-arg2.Months).AddDays(-arg2.Days);
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Возвращает отрицательное значение, если текущий объект меньше, чем <paramref name="other"/>.
    /// Возвращает положительное значение, если текущий объект больше, чем <paramref name="other"/>.
    /// Возвращает 0, если значения одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public int CompareTo(YearMonthDayAge other)
    {
      return _Value.CompareTo(other._Value);
    }

    /// <summary>
    /// Возвращает true, если объекты одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(YearMonthDayAge other)
    {
      return _Value == other._Value;
    }

    /// <summary>
    /// Возвращает true, если объекты одинаковые
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is YearMonthDayAge)
        return Equals((YearMonthDayAge)obj);
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _Value;
    }

    #endregion
  }
}
