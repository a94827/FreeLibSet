// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

/*
 * Работа с закрытыми интервалами дат
 */

namespace FreeLibSet.Calendar
{
  /// <summary>
  /// Интервал дат (закрытый)
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct DateRange : IEnumerable<DateTime>, IEquatable<DateRange>
  {
    #region Конструкторы

    /// <summary>
    /// Создание интервала дат.
    /// Конечная дата не может быть меньше начальной.
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    public DateRange(DateTime firstDate, DateTime lastDate)
      : this(firstDate, lastDate, null)
    {
    }

    /// <summary>
    /// Создание интервала дат.
    /// Конечная дата не может быть меньше начальной.
    /// Эта версия конструктора позволяет установить свойство Tag.
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    /// <param name="tag">Произвольные пользовательские данные</param>
    public DateRange(DateTime firstDate, DateTime lastDate, object tag)
    {
      _FirstDate = firstDate.Date;
      _LastDate = lastDate.Date;
      if (_FirstDate > _LastDate)
        throw new ArgumentException("Начальная дата (" + firstDate.ToString() + ") больше конечной (" + lastDate.ToString() + ")", "lastDate");
      _Tag = tag;
    }

    /// <summary>
    /// Создает копию интервала дат с заданием нового значения свойства Tag
    /// </summary>
    /// <param name="orgRange">Исходный диапазон дат</param>
    /// <param name="tag">Произвольные пользовательские данные</param>
    public DateRange(DateRange orgRange, object tag)
    {
      _FirstDate = orgRange.FirstDate;
      _LastDate = orgRange.LastDate;
      _Tag = tag;
    }

    /// <summary>
    /// Создание интервала за один год
    /// </summary>
    /// <param name="year">Год</param>
    public DateRange(int year)
    {
      _FirstDate = DataTools.BottomOfYear(year);
      _LastDate = DataTools.EndOfYear(year);
      _Tag = null;
    }

    /// <summary>
    /// Создание интервала в один месяц
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    public DateRange(int year, int month)
    {
      _FirstDate = DataTools.BottomOfMonth(year, month);
      _LastDate = DataTools.EndOfMonth(year, month);
      _Tag = null;
    }

    /// <summary>
    /// Создание интервала в один день
    /// </summary>
    /// <param name="date">Начальная и конечная дата диапазона</param>
    public DateRange(DateTime date)
      : this(date, date)
    {
    }

    /// <summary>
    /// Создание интервала из диапазона месяцев
    /// </summary>
    /// <param name="range">Диапазон месяцев</param>
    public DateRange(YearMonthRange range)
      : this(range.FirstYM.BottomOfMonth, range.LastYM.EndOfMonth)
    {
    }

    /// <summary>
    /// Создание интервала в один месяц
    /// </summary>
    /// <param name="ym">Год и месяц</param>
    public DateRange(YearMonth ym)
      : this(ym.BottomOfMonth, ym.EndOfMonth)
    {
    }

    /// <summary>
    /// Создает интервал из (полу)открытого интервала.
    /// Если обе даты заданы, то конечная дата не может быть меньше начальной.
    /// Если начальная дата не задана, используется DateTime.MinValue (01.01.0001).
    /// Если конечная дата не задана, используется DateTime.MaxValue.Date (31.12.9999).
    /// Эта версия конструктора позволяет установить свойство Tag.
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    /// <param name="tag">Произвольные пользовательские данные</param>
    public DateRange(DateTime? firstDate, DateTime? lastDate, object tag)
      : this(firstDate ?? DateTime.MinValue, lastDate ?? DateTime.MaxValue.Date, tag)
    {
    }

    /// <summary>
    /// Создает интервал из (полу)открытого интервала.
    /// Если обе даты заданы, то конечная дата не может быть меньше начальной.
    /// Если начальная дата не задана, используется DateTime.MinValue (01.01.0001).
    /// Если конечная дата не задана, используется DateTime.MaxValue.Date (31.12.9999).
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    public DateRange(DateTime? firstDate, DateTime? lastDate)
      : this(firstDate, lastDate, null)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Начальная дата
    /// </summary>
    public DateTime FirstDate { get { return _FirstDate; } }
    private DateTime _FirstDate;

    /// <summary>
    /// Конечная дата
    /// </summary>
    public DateTime LastDate { get { return _LastDate; } }
    private DateTime _LastDate;

    /// <summary>
    /// Произвольные пользовательские данные
    /// При манипуляциях со списком интервалов поле копируется и может использоваться
    /// для определения интервалов различных типов
    /// </summary>
    public object Tag { get { return _Tag; } }
    private object _Tag;

    /// <summary>
    /// Число дней в интервале
    /// </summary>
    public int Days
    {
      get
      {
        if (IsEmpty)
          return 0; // 16.04.2020
        else
        {
          TimeSpan ts = _LastDate - _FirstDate;
          return ts.Days + 1;
        }
      }
    }

    /// <summary>
    /// Число целых месяцев в интервале
    /// </summary>
    public int Months
    {
      get
      {
        if (IsEmpty)
          return 0; // 16.04.2020
        else
        {
          // int m = (LastDate.Year - FirstDate.Year) * 12 + (LastDate.Month - FirstDate.Month);
          // if (FirstDate.Day > LastDate.Day)
          //   m--;

          // 16.10.2017

          // Число месяцев, в предположении полных месяцев
          int m = (LastDate.Year - FirstDate.Year) * 12 + (LastDate.Month - FirstDate.Month) + 1;
          bool WholeFirstMonth = DataTools.IsBottomOfMonth(FirstDate);
          bool WholeLastMonth = DataTools.IsEndOfMonth(LastDate);
          if (!WholeFirstMonth)
          {
            m--;
            if (!WholeLastMonth)
            {
              if (FirstDate.Day > LastDate.Day)
                m -= 2;
            }
          }
          else
          {
            if (!WholeLastMonth)
              m--;
          }
          return m;
        }
      }
    }

    /// <summary>
    /// Число целых лет в интервале
    /// </summary>
    public int Years
    {
      get
      {
        return Months / 12;
      }
    }

    /// <summary>
    /// Возвращает true, если в интервал входит целое число месяцев,
    /// то есть начальная дата - это первое число, а конечная - последний день месяца
    /// </summary>
    public bool AreWholeMonths
    {
      get
      {
        return DataTools.IsBottomOfMonth(FirstDate) && DataTools.IsEndOfMonth(LastDate);
      }
    }

    /// <summary>
    /// Возвращает true, если в интервал входит целое число лет,
    /// то есть начальная дата - это первое января, а конечная - 31 декабря
    /// </summary>
    public bool AreWholeYears
    {
      get
      {
        return DataTools.IsBottomOfYear(FirstDate) && DataTools.IsEndOfYear(LastDate);
      }
    }


    /// <summary>
    /// Диапазон месяцев
    /// </summary>
    public YearMonthRange YMRange
    {
      get
      {
        if (IsEmpty)
          return new YearMonthRange(); // 16.08.2021
        else
          return new YearMonthRange(_FirstDate, _LastDate);
      }
    }

    /// <summary>
    /// Создает объект MinMaxDateTime, равный текущему интервалу
    /// </summary>
    public MinMaxDateTime MinMaxDate
    {
      get
      {
        if (IsEmpty)
          return new MinMaxDateTime(); // 16.08.2021
        else
          return new MinMaxDateTime(FirstDate, LastDate);
      }
    }

    /// <summary>
    /// Создает объект MinMaxInt, соответствующий годам в текущем интервале
    /// </summary>
    public MinMaxInt MinMaxYear
    {
      get
      {
        if (IsEmpty)
          return new MinMaxInt(); // 16.08.2021
        else
          return new MinMaxInt(FirstDate.Year, LastDate.Year);
      }
    }

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return _FirstDate.Ticks == 0L && _LastDate.Ticks == 0L;
      }
    }

    private void CheckIsNotEmpty()
    {
      if (IsEmpty)
        throw new InvalidOperationException("DateRange is empty");
    }

    /// <summary>
    /// Пустой интервал дат.
    /// В этот интервал не входит ни одна дата, то есть Contains() всегда возвращает false
    /// </summary>
    public static readonly DateRange Empty = new DateRange();

    /// <summary>
    /// Интервал дат, содержащий все даты с 01.01.0001 до 31.12.9999.
    /// </summary>
    public static readonly DateRange Whole = new DateRange(DateTime.MinValue, DateTime.MaxValue.Date);

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает текстовое представление интервала дат и поля Tag, если оно задано
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = DateRangeFormatter.Default.ToString(_FirstDate, _LastDate, false);
      if (_Tag != null)
        s += ", Tag=" + _Tag.ToString();
      return s;
    }

    /// <summary>
    /// Попадает ли заданная дата в интервал
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true, если дата попадает в диапазон</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Используйте метод Contains()", false)]
    public bool DateInRange(DateTime date)
    {
      return Contains(date);
    }

    /// <summary>
    /// Попадает ли заданная дата в интервал
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true, если дата попадает в диапазон</returns>
    public bool Contains(DateTime date)
    {
      date = date.Date;
      if (IsEmpty)
        return false;
      else
        return date >= FirstDate && date <= LastDate;
    }

    /// <summary>
    /// Корректирует дату <paramref name="date"/>, если она не попадает в диапазон.
    /// Если IsEmpty=true, текущая дата не меняется
    /// </summary>
    /// <param name="date">Проверяемая/корректируемая дата</param>
    public void DateToRange(ref DateTime date)
    {
      date = date.Date;
      if (!IsEmpty)
      {
        if (date < FirstDate)
          date = FirstDate;
        if (date > LastDate)
          date = LastDate;
      }
    }

    #endregion

    #region Сложение

    /// <summary>
    /// Возвращает объединенный интервал дат, в который полностью входят оба исходных интервала.
    /// Если первый или второй интервал пустой, возвращается другой интервал.
    /// Свойство Tag копируется из первого интервала.
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Объединенный интервал</returns>
    public static DateRange operator +(DateRange r1, DateRange r2)
    {
      // 16.08.2021
      if (r1.IsEmpty)
      {
        if (r2.IsEmpty)
          return r1;
        else
          return new DateRange(r2.FirstDate, r2.LastDate);
      }
      if (r2.IsEmpty)
        return r1;

      DateTime dt1 = r1.FirstDate < r2.FirstDate ? r1.FirstDate : r2.FirstDate;
      DateTime dt2 = r1.LastDate > r2.LastDate ? r1.LastDate : r2.LastDate;
      return new DateRange(dt1, dt2, r1.Tag);
    }

    /// <summary>
    /// Сдвигает интервал на заданное число дней.
    /// Если <paramref name="r1"/>IsEmpty=true, выбрасывается исключение.
    /// </summary>
    /// <param name="r1">Исходный интервал</param>
    /// <param name="days">Сдвиг в днях (положительное значение - сдвиг вперед, отрицательное - назад, 0 - нет сдвига)</param>
    /// <returns>Новый интервал</returns>
    public static DateRange operator +(DateRange r1, int days)
    {
      r1.CheckIsNotEmpty();
      return new DateRange(r1.FirstDate.AddDays(days), r1.LastDate.AddDays(days), r1.Tag);
    }

    /// <summary>
    /// Сдвигает интервал на заданное число дней.
    /// </summary>
    /// <param name="r1">Исходный интервал</param>
    /// <param name="delta">Сдвиг в днях. Берется свойство TimeSpan.Days (положительное значение - сдвиг вперед, отрицательное - назад, 0 - нет сдвига)</param>
    /// <returns>Новый интервал</returns>
    public static DateRange operator +(DateRange r1, TimeSpan delta)
    {
      return r1 + delta.Days;
    }

    /// <summary>
    /// Сдвигает интервал на заданное число дней в обратную сторону.
    /// Если <paramref name="r1"/>IsEmpty=true, выбрасывается исключение.
    /// </summary>
    /// <param name="r1">Исходный интервал</param>
    /// <param name="days">Сдвиг в днях (положительное значение - сдвиг назад, отрицательное - вперед, 0 - нет сдвига)</param>
    /// <returns>Новый интервал</returns>
    public static DateRange operator -(DateRange r1, int days)
    {
      r1.CheckIsNotEmpty();
      return new DateRange(r1.FirstDate.AddDays(-days), r1.LastDate.AddDays(-days), r1.Tag);
    }

    /// <summary>
    /// Сдвигает интервал на заданное число дней в обратную сторону.
    /// </summary>
    /// <param name="r1">Исходный интервал</param>
    /// <param name="delta">Сдвиг в днях Берется свойство TimeSpan.Days (положительное значение - сдвиг назад, отрицательное - вперед, 0 - нет сдвига)</param>
    /// <returns>Новый интервал</returns>
    public static DateRange operator -(DateRange r1, TimeSpan delta)
    {
      return r1 - delta.Days;
    }

    #endregion

    #region Сдвиг интервалов

    /// <summary>
    /// Сдвиг интервала на целое число дней.
    /// Тоже самое, что оператор "+" с вторым аргументом - целым числом
    /// </summary>
    /// <param name="days">Сдвиг в днях (положительное значение - сдвиг вперед, отрицательное - назад, 0 - нет сдвига)</param>
    /// <returns>Сдвинутый интервал</returns>
    public DateRange AddDays(int days)
    {
      return this + days;
    }

    /// <summary>
    /// Сдвиг интервала на указанное число месяцев.
    /// Если текущий интервал задает полные месяцы (AreWholeMonths=true),
    /// то и конечный интервал будет задавать полные месяцы.
    /// Иначе делается попытка получить даты с тем же днем месяца.
    /// 
    /// Сдвиг может быть неддитивной операцией.
    /// Например, сдвиг периода {01.01.2019-30.01.2019} на 2 месяца: {01.03.2019-30.03.2019}.
    /// А два сдвига по одному месяцу: {01.02.2019-28.02.2019}, {01.03.2019-31.03.2019}.
    /// Неаддитивность происходит при переходе от неполномесячного интервала к интервалу с полными месяцами.
    /// Операции над интервалами с целыми месяцами всегда аддитивны.
    /// 
    /// Если IsEmpty=true, выбрасывается исключение.
    /// </summary>                                               
    /// <param name="months">Сдвиг в месяцах (положительное значение - сдвиг вперед, отрицательное - назад, 0 - нет сдвига)</param>
    /// <returns>Сдвинутый интервал</returns>
    public DateRange AddMonths(int months)
    {
      CheckIsNotEmpty();

      if (months == 0)
        return this;

      if (AreWholeMonths)
      {
        DateTime dt1 = FirstDate.AddMonths(months);
        DateTime dt2 = DataTools.EndOfMonth(dt1.AddMonths(this.Months /* число месяцев в интервале */ - 1));
        return new DateRange(dt1, dt2, Tag);
      }
      else
      {
        DateTime dt1 = DataTools.BottomOfMonth(FirstDate).AddMonths(months);
        DateTime dt2 = DataTools.BottomOfMonth(LastDate).AddMonths(months);
        dt1 = DataTools.CreateDateTime(dt1, FirstDate.Day);
        dt2 = DataTools.CreateDateTime(dt2, LastDate.Day);
        return new DateRange(dt1, dt2, Tag);
      }
    }

    /// <summary>
    /// Сдвиг интервала на указанное число лет.
    /// Если текущий интервал задает полные месяцы (AreWholeMonths=true),
    /// то и конечный интервал будет задавать полные месяцы.
    /// Из-за этого операция сдвига в некоторых случаях может быть неаддитивной.
    /// Сдвиг интервала {01.02.2016-28.02.2016} на 4 года: {01.02.2020-28.02.2020}
    /// не равен двум сдвигам на два года: {01.02.2018-28.02.2018}, {01.02.2020-29.02.2020}.
    /// Неаддитивность происходит при переходе от неполномесячного интервала к интервалу с полными месяцами.
    /// Операции над интервалами с целыми месяцами всегда аддитивны.
    /// 
    /// Если IsEmpty=true, выбрасывается исключение.
    /// </summary>                                               
    /// <param name="years">Сдвиг в годах (положительное значение - сдвиг вперед, отрицательное - назад, 0 - нет сдвига)</param>
    /// <returns>Сдвинутый интервал</returns>
    public DateRange AddYears(int years)
    {
      return AddMonths(years * 12);
    }

    /// <summary>
    /// Сдвиг интервала на указанное число периодов вперед
    /// Если интервал <paramref name="r1"/> задает полные месяцы (AreWholeMonths=true),
    /// то и конечный интервал будет задавать полные месяцы.
    /// Иначе сдвиг выполняется на количество дней в интервале <paramref name="r1"/>.Days.
    /// 
    /// Из-за этого операция сдвига не аддивтивна, то есть:
    /// Пусть есть интервал r1={04.01.2019-31.01.2019}
    /// Тогда сдвиг на два интервала вперед будет {01.03.2019-28.03.2019}
    /// А два отдельных сдвига на один интервал: {01.02.2019-28.02.2019}, {01.03.2019-31.03.2019}.
    /// Неаддитивность происходит при переходе от неполномесячного интервала к интервалу с полными месяцами.
    /// Операции над интервалами с целыми месяцами всегда аддитивны.
    /// 
    /// Если <paramref name="r1"/>.IsEmpty=true, выбрасывается исключение.
    /// </summary>
    /// <param name="r1">исходный диапазон</param>
    /// <param name="rangeCount">Количество интервалов, на которое нужно сдвинуть исходный период</param>
    /// <returns>Сдвинутый интервал</returns>
    public static DateRange operator >>(DateRange r1, int rangeCount)
    {
      if (r1.AreWholeMonths)
        return r1.AddMonths(r1.Months * rangeCount);
      else
        return r1.AddDays(r1.Days * rangeCount);
    }

    /// <summary>
    /// Сдвиг интервала на указанное число периодов назад
    /// Если интервал <paramref name="r1"/> задает полные месяцы (AreWholeMonths=true),
    /// то и конечный интервал будет задавать полные месяцы.
    /// Иначе сдвиг выполняется на количество дней в интервале <paramref name="r1"/>.Days.
    /// Из-за этого операция сдвига не аддивтивна.
    /// См. описание операции сдвига вправо.
    /// Неаддитивность происходит при переходе от неполномесячного интервала к интервалу с полными месяцами.
    /// Операции над интервалами с целыми месяцами всегда аддитивны.
    /// </summary>
    /// <param name="r1">исходный диапазон</param>
    /// <param name="rangeCount">Количество интервалов, на которое нужно сдвинуть исходный период</param>
    /// <returns>Сдвинутый интервал</returns>
    public static DateRange operator <<(DateRange r1, int rangeCount)
    {
      if (r1.AreWholeMonths)
        return r1.AddMonths(-r1.Months * rangeCount);
      else
        return r1.AddDays(-r1.Days * rangeCount);
    }

    #endregion

    #region Объединение и пересечение периодов

    /// <summary>
    /// Возвращает интервал, включающие в себя и r1 и r2 (расширение интервалов).
    /// Операция применима к любым двум интервалам.
    /// Интервалы r1 и r2 равноправны. Поле Tag берется из первого интервала
    /// Если первый или второй интервал пустые, то возвращается пустой интервал.
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static DateRange operator |(DateRange r1, DateRange r2)
    {
      if (r1.IsEmpty || r2.IsEmpty)
        return Empty;
      DateTime dt1 = r1.FirstDate < r2.FirstDate ? r1.FirstDate : r2.FirstDate;
      DateTime dt2 = r1.LastDate > r2.LastDate ? r1.LastDate : r2.LastDate;
      return new DateRange(dt1, dt2, r1.Tag);
    }

    /// <summary>
    /// Возвращает интервал, в который входит пересечение интервалов  r1 и r2
    /// Если интервалы не пересекаются, вызывается исключение InvalidOperationException
    /// Интервалы r1 и r2 равноправны. Поле Tag берется из первого интервала.
    /// Если первый или второй интервал пустые, то возвращается пустой интервал.
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static DateRange operator &(DateRange r1, DateRange r2)
    {
      DateRange r = GetCross(r1, r2);
      if (r.IsEmpty)
        throw new InvalidOperationException("Интервалы " + r1.ToString() + " и " + r2.ToString() +
          " не пересекаются");
      return r;
    }


    /// <summary>
    /// Возвращает интервал, в который входит пересечение интервалов  r1 и r2
    /// Если интервалы не пересекаются, возвращается пустой диапазон (в отличие от оператора "И")
    /// Интервалы r1 и r2 равноправны. Поле Tag берется из первого интервала
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static DateRange GetCross(DateRange r1, DateRange r2)
    {
      if (r1.IsEmpty || r2.IsEmpty)
        return Empty;

      DateTime dt1 = r1.FirstDate > r2.FirstDate ? r1.FirstDate : r2.FirstDate;
      DateTime dt2 = r1.LastDate < r2.LastDate ? r1.LastDate : r2.LastDate;
      if (dt1 > dt2)
        return Empty;
      else
        return new DateRange(dt1, dt2, r1.Tag);
    }

    /// <summary>
    /// Возвращает true, если два интервала пересекаются (имеют хотя бы один общий день)
    /// Такая проверка может понадобиться, например, перед вычислением пересечения
    /// интервалов оператором "И"
    /// Интервалы r1 и r2 равноправны. 
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>True, если интервалы пересекаются</returns>
    public static bool IsCrossed(DateRange r1, DateRange r2)
    {
      DateTime dt1 = r1.FirstDate > r2.FirstDate ? r1.FirstDate : r2.FirstDate;
      DateTime dt2 = r1.LastDate < r2.LastDate ? r1.LastDate : r2.LastDate;
      return dt1 <= dt2;
    }

    #endregion

    #region Разделение на меньшие периоды

    /// <summary>
    /// Разделяет интервал на один или более периодов с границами по годам
    /// </summary>
    /// <returns></returns>
    public DateRangeList SplitIntoYears()
    {
      DateRangeList lst = new DateRangeList();
      for (int y = FirstDate.Year; y <= LastDate.Year; y++) // исправлено 16.08.2021
      {
        DateTime dt1 = DataTools.BottomOfYear(y);
        DateTime dt2 = DataTools.EndOfYear(y);
        if (dt1 < FirstDate)
          dt1 = FirstDate;
        if (dt2 > LastDate)
          dt2 = LastDate;
        lst.Append(new DateRange(dt1, dt2, Tag));
      }

      return lst;
    }

    /// <summary>
    /// Разделяет интервал на один или более периодов с границами по месяцам
    /// </summary>
    /// <returns></returns>
    public DateRangeList SplitIntoMonths()
    {
      DateRangeList lst = new DateRangeList();
      //int x1 = FirstDate.Year * 12 + FirstDate.Month - 1;
      //int x2 = LastDate.Year * 12 + LastDate.Month - 1;
      for (DateTime dt = DataTools.BottomOfMonth(FirstDate); dt <= LastDate; dt = dt.AddMonths(1))
      {
        DateTime dt1 = dt;
        DateTime dt2 = DataTools.EndOfMonth(dt);
        if (dt1 < FirstDate)
          dt1 = FirstDate;
        if (dt2 > LastDate)
          dt2 = LastDate;
        lst.Append(new DateRange(dt1, dt2, Tag));
      }

      return lst;
    }

    #endregion

    #region Сравнение интервалов

    /// <summary>
    /// Возвращает true, если интервалы совпадают. Поле Tag не участвует в сравнении
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(DateRange r1, DateRange r2)
    {
      return r1.FirstDate == r2.FirstDate && r1.LastDate == r2.LastDate;
    }

    /// <summary>
    /// Возвращает true, если интервалы различаются. Поле Tag не участвует в сравнении
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(DateRange r1, DateRange r2)
    {
      return r1.FirstDate != r2.FirstDate || r1.LastDate != r2.LastDate;
    }

    /// <summary>
    /// Возвращает true, если текущий интервал совпадает с <paramref name="other"/>
    /// </summary>
    /// <param name="other">Второй интервал</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(DateRange other)
    {
      return this == other;
    }

    /// <summary>
    /// Возвращает true, если текущий интервал совпадает с <paramref name="other"/>
    /// </summary>
    /// <param name="other">Второй интервал</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (!(other is DateRange))
        return false;
      DateRange r2 = (DateRange)other;
      return this == r2;
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns>FirstDate.GetHashCode()</returns>
    public override int GetHashCode()
    {
      return FirstDate.GetHashCode();
    }

    #endregion

    #region IEnumerable<DateTime> Members

    /// <summary>
    /// Перечислитель
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<DateTime>
    {
      #region Конструктор

      internal Enumerator(DateRange range)
      {
        _FirstDate = range.FirstDate;
        _Days = range.Days;
        _Index = -1;
      }

      #endregion

      #region Поля

      private DateTime _FirstDate;
      private int _Days;
      private int _Index;

      #endregion

      #region IEnumerator<DateTime> Members

      /// <summary>
      /// Текущая дата
      /// </summary>
      public DateTime Current
      {
        get { return _FirstDate.AddDays(_Index); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose() { }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к следующей дате
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < _Days;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по всем датам, входящим в период.
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<DateTime> IEnumerable<DateTime>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }

  /// <summary>
  /// Список интервалов дат, с которым можно выполнять различные действия
  /// Интервалы в списке отсортированы по возрастанию и не могут пересекаться
  /// </summary>
  [Serializable]
  public class DateRangeList : ICloneable, IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public DateRangeList()
    {
      _List = new List<DateRange>();
      _IsReadOnly = false;
    }

    private DateRangeList(bool isWhole)
      : this()
    {
      if (isWhole)
        Add(DateRange.Whole);

      SetReadOnly();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Число отдельных периодов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Получить диапазон из списка по индексу
    /// </summary>
    /// <param name="index">Индекс от 0 до (Count-1)</param>
    /// <returns></returns>
    public DateRange this[int index] { get { return _List[index]; } }

    private List<DateRange> _List;

    /// <summary>
    /// Начальная дата для первого интервала или null, если список пуст
    /// </summary>
    public DateTime? FirstDate
    {
      get
      {
        if (_List.Count > 0)
          return _List[0].FirstDate;
        else
          return null;
      }
    }

    /// <summary>
    /// Конечная дата для последнего интервала или null, если список пуст
    /// </summary>
    public DateTime? LastDate
    {
      get
      {
        if (_List.Count > 0)
          return _List[_List.Count - 1].LastDate;
        else
          return null;
      }
    }

    /// <summary>
    /// Число дней во всех диапазонах
    /// </summary>
    public int Days
    {
      get
      {
        int n = 0;
        for (int i = 0; i < _List.Count; i++)
          n += _List[i].Days;
        return n;
      }
    }

    /// <summary>
    /// Возвращает MinMaxDateTime для списка, куда входит минимальная и максимальная дата
    /// Возвращает пустой объект, если список интервалов пустой.
    /// </summary>
    public MinMaxDateTime MinMaxDate
    {
      get
      {
        if (Count > 0)
          return new MinMaxDateTime(FirstDate.Value, LastDate.Value);
        else
          return new MinMaxDateTime();
      }
    }

    /// <summary>
    /// Возвращает MinMaxInt для минимального и максимального года в списке.
    /// Возвращает пустой объект, если список интервалов пустой.
    /// </summary>
    public MinMaxInt MinMaxYear
    {
      get
      {
        if (Count > 0)
          return new MinMaxInt(FirstDate.Value.Year, LastDate.Value.Year);
        else
          return new MinMaxInt();
      }
    }

    #endregion

    #region Свойство IsReadOnly

    /// <summary>
    /// Возвращает true, если список находится в режиме "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит список в режим "Только чтение"
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException("Список интервалов дат предназначен только для чтения");
    }

    #endregion

    #region Основные методы

    /// <summary>
    /// Список интервалов через запятую
    /// </summary>
    /// <returns>Текстовое представление списка</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append('{');
      for (int i = 0; i < _List.Count; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append(DateRangeFormatter.Default.ToString(_List[i].FirstDate, _List[i].LastDate, false));
      }
      sb.Append('}');
      return sb.ToString();
    }

    /// <summary>
    /// Добавление очередного интервала
    /// Допускается добавление только интервала с начальной датой, больше последней
    /// существующей. То есть, метод может использоваться только для последовательного
    /// заполнения интервала дат
    /// 
    /// Этот метод является устаревшим. Используйте Add()
    /// </summary>
    /// <param name="range">Добавляемый интервал</param>
    public void Append(DateRange range)
    {
      CheckNotReadOnly();

      if (_List.Count > 0)
      {
        if (_List[_List.Count - 1].LastDate >= range.FirstDate)
          throw new ArgumentException("Добавляемый интервал " + range.ToString() +
            " пересекается с последним добавленным " + _List[_List.Count - 1].ToString(), "range");
      }
      _List.Add(range);
    }


    /// <summary>
    /// Добавление очередного интервала
    /// Допускается добавление только интервала с начальной датой, больше последней
    /// существующей. То есть, метод может использоваться только для последовательного
    /// заполнения интервала дат
    /// 
    /// Этот метод является устаревшим. Используйте Add()
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона</param>
    /// <param name="lastDate">Конечная дата диапазона</param>
    public void Append(DateTime firstDate, DateTime lastDate)
    {
      Append(new DateRange(firstDate, lastDate));
    }

    /// <summary>
    /// Очистка списка
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
    }

    /// <summary>
    /// Найти индекс интервала, в который входит дата.
    /// Возвращает -1, если дата не входит ни в один интервал
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>Индекс найденного диапазона в списке</returns>
    public int IndexOf(DateTime date)
    {
      int idx = NearestIndexOf(date);
      if (idx >= 0)
      {
        if (!_List[idx].Contains(date))
          idx = -1;
      }
      return idx;
    }

    /// <summary>
    /// Реализация быстрого поиска даты с использованием метода половинного деления
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>Индекс найденного диапазона в списке</returns>
    private int NearestIndexOf(DateTime date)
    {
      //if (FList.Count == 0)
      //  return -1;
      //if (FList.Count == 1)
      //  return 0;
      if (_List.Count <= 1)
        return _List.Count - 1;

      date = date.Date;

      int FirstIndex = 0;
      int LastIndex = _List.Count - 1;
      while (LastIndex > FirstIndex)
      {
        int MiddleIndex = (FirstIndex + LastIndex) / 2;
        if (date < _List[MiddleIndex].FirstDate)
        {
          LastIndex = MiddleIndex - 1;
          continue;
        }
        if (date > _List[MiddleIndex].LastDate)
        {
          FirstIndex = MiddleIndex + 1;
          continue;
        }
        // Попали в цель
        return MiddleIndex;
      }
      return FirstIndex;
    }

    #endregion

    #region Добавление и удаление интервалов

    /// <summary>
    /// Добавить интервал к списку.
    /// Существующие интервалы модифицируются: частично пересекающиеся с добавляемым
    /// укорачиваются сверху и / или снизу, а полностью попадающие внутрь добавляемого
    /// - удаляются.
    /// </summary>
    /// <param name="range"></param>
    public void Add(DateRange range)
    {
      CheckNotReadOnly();

      // Чтобы не дублировать код, сначала освобождаем место для интервала,
      // а потом добавляем новый
      Remove(range);

      // Так все-таки данные чаще добавляются по порядку возрастания дат,
      // выгоднее проверять от конца списка к началу
      for (int i = _List.Count - 1; i >= 0; i--)
      {
        if (_List[i].LastDate <= range.FirstDate)
        {
          // Вставляем интервал после i-го
          if (i == _List.Count - 1)
            _List.Add(range);
          else
            _List.Insert(i + 1, range);
          return;
        }
      }

      // Все интервалы в списке находятся после добавляемого
      // Вставляем интервал в начало списка
      _List.Insert(0, range);
    }

    /// <summary>
    /// Удалить (вырезать) интервал из списка
    /// Полностью пересекающиеся интервалы удаляются, а частично пересекающиеся -
    /// обрезаются сверху и снизу.
    /// Поле Tag вычитаемого интевала <paramref name="range"/> нигде не используется.
    /// </summary>
    /// <param name="range">Удаляемый диапазон</param>
    public void Remove(DateRange range)
    {
      CheckNotReadOnly();

      for (int i = _List.Count - 1; i >= 0; i--)
      {
        if (_List[i].FirstDate > range.LastDate)
          continue; // интервалы не пересекаются
        if (_List[i].LastDate < range.FirstDate)
          break; // интервалы не пересекаются

        // Возможны теоретически 9 вариантов пересечения интервалов, когда
        // начальная дата существующиего интервала 1)меньше, 2)равна и 3) больше
        // начальной даты удаляемого интервала, и, аналогично, для конечной даты.
        int Sign1 = Math.Sign(DateTime.Compare(_List[i].FirstDate, range.FirstDate));
        int Sign2 = Math.Sign(DateTime.Compare(_List[i].LastDate, range.LastDate));
        // Sign1 и Sign2 содержат:
        // (-1), если существующая дата меньше удаляемой
        // (0), если даты равны
        // (+1), если существующая дата больше удаляемой

        switch (Sign1) // по начальной дате
        {
          case -1:
            switch (Sign2)
            {
              case -1:
              case 0:
                // Требуется уменьшить конечную дату интервала
                _List[i] = new DateRange(_List[i].FirstDate, range.FirstDate.AddDays(-1), _List[i].Tag);
                break;
              case +1:
                // Самый интересный случай - разбиение существующего интервала на 2
                _List.Insert(i + 1, new DateRange(range.LastDate.AddDays(+1), _List[i].LastDate, _List[i].Tag));
                _List[i] = new DateRange(_List[i].FirstDate, range.FirstDate.AddDays(-1), _List[i].Tag);
                break;
            }
            break;

          case 0:
          case +1:
            switch (Sign2)
            {
              case -1:
              case 0:
                // Полное удаление существующего интервала
                _List.RemoveAt(i);
                break;
              case 1:
                // Требуется увелить начальную дату интервала
                _List[i] = new DateRange(range.LastDate.AddDays(+1), _List[i].LastDate, _List[i].Tag);
                break;
            }
            break;
        }
      }
    }

    /// <summary>
    /// Добавляет к текущему списку все диапазоны из другого списка
    /// </summary>
    /// <param name="rangeList">Добавляемый список</param>
    public void Add(DateRangeList rangeList)
    {
      CheckNotReadOnly();

      for (int i = 0; i < rangeList.Count; i++)
        Add(rangeList[i]);
    }

    /// <summary>
    /// Вычитает из текущего списка все диапазоны из другого списка
    /// </summary>
    /// <param name="rangeList">Вычитаемый список</param>
    public void Remove(DateRangeList rangeList)
    {
      CheckNotReadOnly();

      for (int i = 0; i < rangeList.Count; i++)
        Remove(rangeList[i]);
    }

    #endregion

    #region Разбиение на фрагменты

    /// <summary>
    /// Разбиение интервалов в списке на части по границам указанных дат.
    /// Вызывает перегрузку метода Split() для каждой даты в списке <paramref name="aDates"/>.
    /// О границах интервалов см. в перегрузке для аргумента DateTime.
    /// </summary>
    /// <param name="aDates">Список дат для разбиения</param>
    public void Split(DateTime[] aDates)
    {
      CheckNotReadOnly();

      for (int i = 0; i < aDates.Length; i++)
        Split(aDates[i]);
    }

    /// <summary>
    /// Разбиение интервала на 2, если указанная дата попадает в какой-нибудь
    /// интервал. При разбиении интервала поле Tag копируется из исходного DateRange в два
    /// новых.
    /// Определено, что при разбиении <paramref name="date"/> задает конец первого интервала. Начало второго интервала - на один день больше.
    /// Например, если есть интервал {01.01.2021-31.01.2021} и дата равна 15.01.2021, то после разбиения будет {01.01.2021-15.01.2021},{16.01.2021-31.01.2021}.
    /// При необходимости, используйте DateTime.AddDays(-1).
    /// </summary>
    /// <param name="date">Дата разбиения</param>
    public void Split(DateTime date)
    {
      CheckNotReadOnly();

      int idx = IndexOf(date);
      if (idx < 0)
        return; // Разбиение не требуется
      // Требуется разбить интервал на 2
      DateTime dt1 = _List[idx].FirstDate;
      DateTime dt2 = _List[idx].LastDate;
      if (date == dt2)
        return;
      object Tag = _List[idx].Tag;
      DateRange Range1 = new DateRange(dt1, date, Tag);
      DateRange Range2 = new DateRange(date.AddDays(1), dt2, Tag);
      _List[idx] = Range2;
      _List.Insert(idx, Range1);
    }

    /// <summary>
    /// Разбиение интервалов на годы
    /// </summary>
    public void SplitIntoYears()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      int FirstYear = FirstDate.Value.Year;
      int LastYear = LastDate.Value.Year;
      for (int y = FirstYear; y < LastYear; y++)
        Split(DataTools.EndOfYear(y)); // 23.07.2021
    }

    /// <summary>
    /// Разбиение интервалов на месяцы
    /// </summary>
    public void SplitIntoMonths()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      DateTime dt1 = DataTools.BottomOfMonth(FirstDate.Value);
      DateTime dt2 = DataTools.BottomOfMonth(LastDate.Value);

      for (DateTime dt = dt1.AddMonths(1); dt <= dt2; dt = dt.AddMonths(1))
        Split(dt.AddDays(-1)); // 23.07.2021
    }

    #endregion

    #region Объединение

    /// <summary>
    /// Объединение соседних интервалов
    /// Если есть два интервала и начальная дата второго на один день больше
    /// конечной даты первого, такие интервалы объединяются.
    /// Поле Tag берется из более раннего интервала
    /// </summary>
    public void Merge()
    {
      Merge(false);
    }

    /// <summary>
    /// Объединение соседних интервалов
    /// Если есть два интервала и начальная дата второго на один день больше
    /// конечной даты первого, такие интервалы объединяются.
    /// Если задано EqualTags=true, то объединение выполняется, только если поля
    /// Tag совпадают. Для сравнения Tag используется метод object.Equals()
    /// </summary>
    /// <param name="equalTags">true - объединяются только диапазоны с одинаковыми Tag. false - поле Tag не учитывается и берется из более раннего диапазона</param>
    public void Merge(bool equalTags)
    {
      CheckNotReadOnly();

      int CurrIndex = 0;
      while (CurrIndex < (_List.Count - 1))
      {
        DateRange r1 = _List[CurrIndex];
        DateRange r2 = _List[CurrIndex + 1];
        if (r1.LastDate.AddDays(1) == r2.FirstDate)
        {
          if (equalTags)
          {
            if (!object.Equals(r1.Tag, r2.Tag))
            {
              CurrIndex++;
              continue;
            }
          }

          // объедияем
          DateRange r3 = new DateRange(r1.FirstDate, r2.LastDate, r1.Tag);
          _List[CurrIndex] = r3;
          _List.RemoveAt(CurrIndex + 1);
        }
        else
          CurrIndex++;
      }
    }

    #endregion

    #region Определение пересечений

    /// <summary>
    /// Возвращает true, если проверяемый период хотя бы частично пересекается с любым
    /// из периодов в списке
    /// </summary>
    /// <param name="range">Проверяемый период</param>
    /// <returns>true, если пересечение найдено</returns>
    public bool IsCrossed(DateRange range)
    {
      for (int i = 0; i < _List.Count; i++)
      {
        if (DateRange.IsCrossed(_List[i], range))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если есть пересечение хотя бы одного интервала проверяемого списка <paramref name="rangeList"/>
    /// с любым из периодов в текущем списке.
    /// </summary>
    /// <param name="rangeList">Проверяемый список периодов</param>
    /// <returns>true, если пересечение найдено</returns>
    public bool IsCrossed(DateRangeList rangeList)
    {
      for (int i = 0; i < rangeList.Count; i++)
      {
        if (IsCrossed(rangeList[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает новый список, содержащий только те диапазоны, которые
    /// попадают в заданный. При этом некоторые диапазоны могут быть "укорочены".
    /// Существующий список не меняется
    /// </summary>
    /// <param name="range">Ограничивающй интервал дат</param>
    /// <returns>Новый список</returns>
    public DateRangeList GetCross(DateRange range)
    {
      DateRangeList NewList = new DateRangeList();
      for (int i = 0; i < Count; i++)
      {
        if (DateRange.IsCrossed(this[i], range))
          NewList.Append(this[i] & range);
      }
      return NewList;
    }

    /// <summary>
    /// Возвращает true, если проверяемый период полностью входит в список периодов.
    /// В списке может быть несколько периодов, идущих без разрыва, в которые
    /// попадают начальная и конечная дата проверяемого диапазона
    /// </summary>
    /// <param name="range">Проверяемый период</param>
    /// <returns>true, если есть вхождение</returns>
    public bool ContainsWhole(DateRange range)
    {
      int idx1 = IndexOf(range.FirstDate);
      int idx2 = IndexOf(range.LastDate);
      if (idx1 < 0 || idx2 < 0)
        return false;
      if (idx2 == idx1)
        return true; // начало и конец диапазона попадают в один интервал

      // Начальная и конечная даты проверяемого диапазона попадают в разные диапазоны в списке

      for (int i = idx1; i < idx2; i++)
      {
        DateRange r1 = _List[i];
        DateRange r2 = _List[i + 1];
        if (r2.FirstDate != r1.LastDate.AddDays(1))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Возвращает true, если все периоды в проверяемом списке <paramref name="rangeList"/> 
    /// полностью входят в текущий список периодов.
    /// В списке может быть несколько периодов, идущих без разрыва, в которые
    /// попадают начальная и конечная дата проверяемого диапазона
    /// </summary>
    /// <param name="rangeList">Проверяемый список периодов</param>
    /// <returns>true, если есть вхождение</returns>
    public bool ContainsWhole(DateRangeList rangeList)
    {
      for (int i = 0; i < rangeList.Count; i++)
      {
        if (!ContainsWhole(rangeList[i]))
          return false;
      }
      return true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Возвращает копию списка со сброшенным свойством IsReadOnly
    /// </summary>
    /// <returns></returns>
    public DateRangeList Clone()
    {
      DateRangeList r2 = new DateRangeList();
      r2._List.AddRange(_List);
      return r2;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Статические поля

    /// <summary>
    /// Пустой список, предназначенный только для чтения
    /// </summary>
    public static readonly DateRangeList Empty = new DateRangeList(false);

    /// <summary>
    /// Список, содержащий интервал дат с 01.01.0001 до 31.12.9999
    /// </summary>
    public static readonly DateRangeList Whole = new DateRangeList(true);

    #endregion
  }

}
