// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Calendar
{
  /// <summary>
  /// Структура для хранения дня и месяца (применимых к любому году).
  /// Хранит номер дня в году как число от 1 до 365 (29 февраля отсутствует).
  /// </summary>
  [Serializable]
  public struct MonthDay : IEquatable<MonthDay>, IComparable<MonthDay>, IFormattable
  {
    #region Конструктор

    /// <summary>
    /// Основная версия конструктора, принимающая номер дня в году.
    /// Позволяет инициализировать пустое значение.
    /// </summary>
    /// <param name="dayOfYear">Номер дня в году, в диапазоне от 1 до 365. 0 создает пустую структуру</param>
    public MonthDay(int dayOfYear)
    {
      if (dayOfYear < 0 || dayOfYear > 365)
        throw new ArgumentOutOfRangeException("dayOfYear", dayOfYear, Res.MonthDay_Arg_DayOfYearOutOfRange);
      _DayOfYear = (short)dayOfYear;
    }

    /// <summary>
    /// Заполняет структуру.
    /// Эта версия конструктора не позволяет инициализировать пустую структуру
    /// </summary>
    /// <param name="month">Месяц в диапазоне от 1 до 12</param>
    /// <param name="day">День в диапазоне от 1 до 28, 30 или 31, в зависимости от месяца. 29 февраля не допускается.</param>
    public MonthDay(int month, int day)
    {
      DateTime dt = new DateTime(2001, month, day);
      _DayOfYear = (short)(dt.DayOfYear);
    }

    /// <summary>
    /// Заполняет структуру из <see cref="DateTime"/>.
    /// 29 февраля, которое может быть в високосном году, заменяется на 28 февраля.
    /// </summary>
    /// <param name="date">Отсюда берется месяц и день</param>
    public MonthDay(DateTime date)
      : this(date.Month, CorrectDay(date.Day, date.Month))
    {
      // нельзя использовать Date.DayOfYear напрямую, т.к. может быть високосный год
    }

    /// <summary>
    /// 04.07.2019
    /// Корректируем 29 февраля, которое может быть в високосном году
    /// </summary>
    /// <param name="day"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    private static int CorrectDay(int day, int month)
    {
      if (day == 29 && month == 2)
        return 28;
      else
        return day;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает номер дня в году о 1 до 365.
    /// Для <see cref="IsEmpty"/>=true возвращает 0.
    /// </summary>
    public int DayOfYear { get { return _DayOfYear; } }
    private readonly short _DayOfYear;

    /// <summary>
    /// Дата, от которой отсчитывается <see cref="DayOfYear"/>.
    /// Используется для получения свойств Month и Day
    /// </summary>
    private static readonly DateTime _BaseDate = new DateTime(2000, 12, 31);

    /// <summary>
    /// Месяц в диапазоне от 1 до 12.
    /// Для пустого значения <see cref="IsEmpty"/>=true возвращает 0.
    /// </summary>
    public int Month
    {
      get
      {
        if (DayOfYear == 0)
          return 0;
        else
          return _BaseDate.AddDays(DayOfYear).Month;
      }
    }

    /// <summary>
    /// День в диапазоне от 1 до 28,30 или 31 (в зависимости от месяца).
    /// Для пустого значения <see cref="IsEmpty"/>=true возвращает 0.
    /// </summary>
    public int Day
    {
      get
      {
        if (DayOfYear == 0)
          return 0;
        else
          return _BaseDate.AddDays(DayOfYear).Day;
      }
    }

    /// <summary>
    /// Возвращает true для неинициализированной структуры
    /// </summary>
    public bool IsEmpty { get { return _DayOfYear == 0; } }

    /// <summary>
    /// Неинициализированная структура.
    /// </summary>
    public static readonly MonthDay Empty = new MonthDay();

    /// <summary>
    /// Возвращает первый день месяца, заданного в текущем объекте.
    /// Для пустой структуры возвращает ее же.
    /// </summary>
    public MonthDay BottomOfMonth
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return new MonthDay(Month, 1);
      }
    }

    /// <summary>
    /// Возвращает последний день месяца, заданного в текущем объекте.
    /// Для пустой структуры возвращает ее же.
    /// </summary>
    public MonthDay EndOfMonth
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return new MonthDay(Month, DaysInMonth(Month));
      }
    }

    /// <summary>
    /// Возвращает первый день квартала, заданного в текущем объекте.
    /// Для пустой структуры возвращает ее же.
    /// </summary>
    public MonthDay BottomOfQuarter
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return new MonthDay(((Month - 1) / 3) * 3 + 1, 1);
      }
    }

    /// <summary>
    /// Возвращает последний день квартала, заданного в текущем объекте.
    /// Для пустой структуры возвращает ее же.
    /// </summary>
    public MonthDay EndOfQuarter
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
        {
          int m = ((Month - 1) / 3) * 3 + 3;
          return new MonthDay(m, DaysInMonth(m));
        }
      }
    }

    /// <summary>
    /// Структура, содержащая "1 января"
    /// </summary>
    public static readonly MonthDay BottomOfYear = new MonthDay(1);

    /// <summary>
    /// Структура, содержащая "31 декабря"
    /// </summary>
    public static readonly MonthDay EndOfYear = new MonthDay(365);

    #endregion

    #region Сложение дат

    /// <summary>
    /// Следующий день (29 февраля не бывает).
    /// Для пустой структуры <see cref="IsEmpty"/>=true возвращает ее же.
    /// </summary>
    public MonthDay NextDay
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return this + 1;
      }
    }

    /// <summary>
    /// Предыдущий день (29 февраля не бывает).
    /// Для пустой структуры <see cref="IsEmpty"/>=true возвращает ее же.
    /// </summary>
    public MonthDay PrevDay
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return this - 1;
      }
    }

    /// <summary>
    /// Добавляет к значению <see cref="MonthDay"/> заданное количество дней.
    /// Предполагается, что в году 365 дней.
    /// </summary>
    /// <param name="monthDay">Текущее значение. Не может быть пустой структурой</param>
    /// <param name="days">Количество добавляемых дней. Значение может быть положительным и отрицательным</param>
    /// <returns>Новое значение</returns>
    public static MonthDay operator +(MonthDay monthDay, int days)
    {
      if (monthDay.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("monthDay");

      days = days % 365;

      int v = monthDay.DayOfYear + days;
      if (v < 1)
        v += 365;
      if (v > 365)
        v -= 365;
      return new MonthDay(v);
    }

    /// <summary>
    /// Вычитает из <see cref="MonthDay"/> заданное количество дней.
    /// Предполагается, что в году 365 дней.
    /// </summary>
    /// <param name="monthDay">Текущее значение. Не может быть пустой структурой</param>
    /// <param name="days">Количество вычитаемых дней. Значение может быть положительным и отрицательным</param>
    /// <returns>Новое значение</returns>
    public static MonthDay operator -(MonthDay monthDay, int days)
    {
      if (monthDay.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("monthDay");

      days = days % 365;

      int v = monthDay.DayOfYear - days;
      if (v < 1)
        v += 365;
      if (v > 365)
        v -= 365;
      return new MonthDay(v);
    }

    /// <summary>
    /// Возвращает количество дней между <paramref name="a"/> и <paramref name="b"/>.
    /// Возвращаемое значение может быть в диапазоне от 0 до (+364).
    /// </summary>
    /// <param name="a">Первое значение. Не может быть пустой структурой</param>
    /// <param name="b">Второе значение. Не может быть пустой структурой</param>
    /// <returns>Разность</returns>
    public static int operator -(MonthDay a, MonthDay b)
    {
      if (a.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("a");
      if (b.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("b");

      int days = a.DayOfYear - b.DayOfYear;
      if (days < 0)
        days += 365;
      return days;
    }

    /// <summary>
    /// Сдвигает диапазон на указанное число дней.
    /// В отличие от оператора сложения, для пустой структуры возвращается также пустая структура, а не генерируется исключение
    /// </summary>
    /// <param name="days">Количество дней</param>
    /// <returns>Новый день</returns>
    public MonthDay AddDays(int days)
    {
      if (IsEmpty)
        return Empty;
      return this + days;
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Получить дату для выбранного года
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="february29">Если true и год високосный, то 28 февраля заменяется на 29</param>
    /// <returns></returns>
    public DateTime GetDate(int year, bool february29)
    {
      const int DayFeb28 = 31 + 28; // 28 февраля

      if (IsEmpty)
        throw ExceptionFactory.StructureNotInit(typeof(MonthDay));
      if (february29 && DayOfYear == DayFeb28)
        return TimeTools.EndOfMonth(year, 2);
      else
        return new DateTime(year, Month, Day);
    }

    /// <summary>
    /// Возвращает текстовое представление в формате "ДД месяц"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      //return "Month=" + Month.ToString() + ", Day=" + Day.ToString();
      // 14.06.2019
      return ToString(String.Empty, null);
    }

    /// <summary>
    /// Возвращает текстовое представление в заданном формате
    /// </summary>
    /// <param name="format">Формат</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(string format)
    {
      return ToString(format, null);
    }

    /// <summary>
    /// Возвращает текстовое представление в формате "ДД месяц"
    /// </summary>
    /// <param name="formatProvider">Форматировщик</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(IFormatProvider formatProvider)
    {
      return ToString(String.Empty, formatProvider);
    }

    /// <summary>
    /// Возвращает текстовое представление в заданном формате
    /// </summary>
    /// <param name="format">Формат</param>
    /// <param name="formatProvider">Форматировщик</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      if (IsEmpty)
        return String.Empty;

      if (String.IsNullOrEmpty(format))
      {
        switch (Formatting.FormatStringTools.GetDateFormatOrder(String.Empty, formatProvider))
        {
          case Formatting.DateFormatYMDOrder.DMY:
          case Formatting.DateFormatYMDOrder.YDM:
            format = "dd MMMM";
            break;
          default:
            format = "MMMM dd";
            break;
        }
      }

      return GetDate(2001, false).ToString(format, formatProvider);
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если текущий объект равен <paramref name="other"/>.
    /// Также поддерживает сравнение с DateTime.
    /// </summary>
    /// <param name="other">Второй объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(MonthDay other)
    {
      return this == other;
    }

    /// <summary>
    /// Возвращает true, если текущий объект равен <paramref name="other"/>.
    /// Также поддерживает сравнение с <see cref="DateTime"/>.
    /// </summary>
    /// <param name="other">Второй объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is MonthDay)
      {
        return (MonthDay)other == this;
      }
      if (other is DateTime)
      {
        DateTime dt = (DateTime)other;
        return dt.Month == Month && dt.Day == Day;
      }
      return false;
    }

    #endregion

    #region Операторы сравнения

    /// <summary>
    /// Возвращает true, если структуры идентичны
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear == value2.DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если структуры различаются
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear != value2.DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если первая структура меньше второй.
    /// Сначала сравниваются месяцы, а, если они совпадают, то дни.
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator <(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear < value2.DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если первая структура меньше или равна второй.
    /// Сначала сравниваются месяцы, а, если они совпадают, то дни.
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator <=(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear <= value2.DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если первая структура больше второй.
    /// Сначала сравниваются месяцы, а, если они совпадают, то дни.
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator >(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear > value2.DayOfYear;
    }

    /// <summary>
    /// Возвращает true, если первая структура больше или равна второй.
    /// Сначала сравниваются месяцы, а, если они совпадают, то дни.
    /// </summary>
    /// <param name="value1">Первый сравниваемый объект</param>
    /// <param name="value2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator >=(MonthDay value1, MonthDay value2)
    {
      return value1.DayOfYear >= value2.DayOfYear;
    }

    /// <summary>
    /// Сравнение с другим периодом.
    /// Если текущий интервал больше <paramref name="other"/>, возвращается положительное число.
    /// Если текущий интервал меньше <paramref name="other"/>, возвращается отрицательное число.
    /// Если интервалы равны, возвращается 0.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public int CompareTo(MonthDay other)
    {
      return this.DayOfYear - other.DayOfYear;
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Получить число дней в месяце (для невисокосного года)
    /// </summary>
    /// <param name="month">Номер месяца (1-12)</param>
    /// <returns>Дни</returns>
    public static int DaysInMonth(int month)
    {
      return DateTime.DaysInMonth(2001, month);
    }

    /// <summary>
    /// Получить значение из заданного для и месяца.
    /// В отличие от однотипного конструктора, позволяет передать значения 0.
    /// </summary>
    /// <param name="month">Месяц. Может быть в диапазоне от 0 до 12</param>
    /// <param name="day">День, может быть от 0 до 31</param>
    /// <returns>Заполненная или пустая структура <see cref="IsEmpty"/>=true</returns>
    public static MonthDay FromMonthDay(int month, int day)
    {
      if (month == 0 || day == 0)
        return Empty;
      else
        return new MonthDay(month, day);
    }

    #endregion
  }

  /// <summary>
  /// Диапазон дней.
  /// В отличие от <see cref="DateRange"/>, эта структура предполагает годовую цикличность.
  /// Поэтому свойство First может быть и больше Last.
  /// Если First.IsEmpty и Last.IsEmpty, то структура считается неинициализированной (IsEmpty=true). Не путать с полным периодом (IsWholeYear=true)
  /// Структура также содержит поле Tag, которое может хранить произвольные пользовательские данные.
  /// </summary>
  [Serializable]
  public struct MonthDayRange : IEnumerable<MonthDay>
  {
    #region Конструкторы

    /// <summary>
    /// Создает структуру.
    /// Первый и последний день диапазона должны быть оба заполнены или оба равны <see cref="MonthDay.Empty"/> для создания пустой структуры.
    /// Не может задаваться половинчатый диапазон, когда <paramref name="first"/>.IsEmpty не равно <paramref name="last"/>.IsEmpty.
    /// Свойство <see cref="Tag"/> будет иметь значение null.
    /// </summary>
    /// <param name="first">Первый день диапазона</param>
    /// <param name="last">Последний день диапазона</param>
    public MonthDayRange(MonthDay first, MonthDay last)
      : this(first, last, null)
    {
    }

    /// <summary>
    /// Создает структуру.
    /// Первый и последний день диапазона должны быть оба заполнены или оба равны <see cref="MonthDay.Empty"/> для создания пустой структуры.
    /// Не может задаваться половинчатый диапазон, когда <paramref name="first"/>.IsEmpty не равно <paramref name="last"/>.IsEmpty.
    /// </summary>
    /// <param name="first">Первый день диапазона</param>
    /// <param name="last">Последний день диапазона</param>
    /// <param name="tag">Произвольные пользовательские данные</param>
    public MonthDayRange(MonthDay first, MonthDay last, object tag)
    {
      if (!(first.IsEmpty && last.IsEmpty))
      {
        if (first.IsEmpty)
          throw ExceptionFactory.ArgIsEmpty("first");
        if (last.IsEmpty)
          throw ExceptionFactory.ArgIsEmpty("last");
      }

      _First = first;
      _Last = last;
      _Tag = tag;
    }

    /// <summary>
    /// Создает период из одного дня.
    /// Свойство <see cref="Tag"/> будет иметь значение null.
    /// </summary>
    /// <param name="day">Начало и конец диапазона</param>
    public MonthDayRange(MonthDay day)
      : this(day, day)
    {
    }

    /// <summary>
    /// Создает структуру для заданного диапазона дат.
    /// Если диапазон <paramref name="range"/> охватывает больше одного года (например, 05.07.2019-05.07.2020),
    /// то создается полный диапазон <see cref="IsWholeYear"/>=true, и даты из аргумента не используются.
    /// Если <paramref name="range"/>.IsEmpty=true, то создается пустой диапазон.
    /// Свойство <see cref="Tag"/> берется из <see cref="DateRange.Tag"/>.
    /// </summary>
    /// <param name="range">Диапазон дат</param>
    public MonthDayRange(DateRange range)
    {
      if (range.IsEmpty)
      {
        _First = MonthDay.Empty;
        _Last = MonthDay.Empty;
      }
      else
      {
        _First = new MonthDay(range.FirstDate);
        _Last = new MonthDay(range.LastDate);

        int deltaY = range.LastDate.Year - range.FirstDate.Year;
        bool setWholeYear = false;
        switch (deltaY)
        {
          case 0: // в пределах одного года
            break;
          case 1: // Два соседних года
            if (_Last >= _First)
              setWholeYear = true;
            break;
          default:
            setWholeYear = true;
            break;
        }
        if (setWholeYear)
        {
          _First = MonthDay.BottomOfYear;
          _Last = MonthDay.EndOfYear;
        }
      }

      _Tag = range.Tag;
    }

    /// <summary>
    /// Создает копию диапазона, устанавливая новое значение свойства <see cref="Tag"/>
    /// </summary>
    /// <param name="source">Исходный объект</param>
    /// <param name="tag">Значение свойства <see cref="Tag"/></param>
    public MonthDayRange(MonthDayRange source, object tag)
    {
      _First = source._First;
      _Last = source._Last;
      _Tag = tag;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Первый день диапазона
    /// </summary>
    public MonthDay First { get { return _First; } }
    private readonly MonthDay _First;

    /// <summary>
    /// Последний день диапазона
    /// </summary>
    public MonthDay Last { get { return _Last; } }
    private readonly MonthDay _Last;

    /// <summary>
    /// Возвращает true, если диапазон пустой
    /// </summary>
    public bool IsEmpty
    {
      get { return _First.IsEmpty; } // проверять Last необязательно
    }

    /// <summary>
    /// Возвращает true, если период охватывает весь год
    /// </summary>
    public bool IsWholeYear
    {
      get
      {
        return Days == 365;
      }
    }

    /// <summary>
    /// Возвращает количество дней в диапазоне без учета високосного года.
    /// Для непустого периода возвращает число от 1 до 365. 365 означает полный год
    /// Для пустого периода (<see cref="IsEmpty"/>=true) возвращает 0.
    /// </summary>
    public int Days
    {
      get
      {
        if (_First.IsEmpty)
          return 0;

        if (_Last >= _First)
          return _Last.DayOfYear - _First.DayOfYear + 1;
        else
          return 366 - _First.DayOfYear + _Last.DayOfYear;
      }
    }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } }
    private readonly object _Tag;

    /// <summary>
    /// Получить комплементарный интервал дат. Например, для периода "10 октября-15 мая"
    /// возвращается интервал "16 мая-09 октября".
    /// Свойство <see cref="Tag"/> дублируется в создаваемый интервал.
    /// Если <see cref="IsEmpty"/>=true, возвращается пустой интервал.
    /// Для полного интервала <see cref="IsWholeYear"/> возвращается полный интервал.
    /// </summary>
    public MonthDayRange Complement
    {
      get
      {
        if (IsEmpty)
          return Empty;
        else
          return new MonthDayRange(this.Last.NextDay, this.First.PrevDay, this.Tag);
      }
    }

    /// <summary>
    /// Возвращает количество месяцев и дней в интервале. Предполагается, что год не високосный.
    /// Дни считаются включительно, то есть для периода {01.02-28.02} возвращается ровно 1 месяц
    /// Если <see cref="IsWholeYear"/>=true, то возвращается 1 год. 
    /// Если <see cref="IsEmpty"/>=true, возвращается <see cref="YearMonthDayAge.Zero"/>
    /// </summary>
    public YearMonthDayAge Age
    {
      get
      {
        //if (IsEmpty)
        //  return YearMonthDayAge.Zero;
        //if (IsWholeYear)
        //  return new YearMonthDayAge(1, 0, 0);
        return GetDateRange(2022).Age;
      }
    }

    #endregion

    #region Попадание в интервал

    /// <summary>
    /// Возвращает попадание дня в текущий интервал.
    /// Если <paramref name="day"/>.IsEmpty=true или текущая структура не заполнена (<see cref="IsEmpty"/>=true), возвращает false.
    /// </summary>
    /// <param name="day">Проверяемый день</param>
    /// <returns>Признак попадания</returns>
    public bool Contains(MonthDay day)
    {
      if (IsEmpty || day.IsEmpty)
        return false;
      if (_Last >= _First)
        return day >= _First && day <= _Last;
      else
        return day <= _Last || day >= _First;
    }

    /// <summary>
    /// Попадает ли заданная дата в интервал.
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true, если дата попадает в диапазон</returns>
    public bool Contains(DateTime date)
    {
      MonthDay md = new MonthDay(date);
      return Contains(md);
    }

    #endregion

    #region Пересечение

    /// <summary>
    /// Возвращает пересечение двух интервалов. Поле <see cref="Tag"/> берется из первого интервала.
    /// Метод возвращает пустой массив, если интервалы не пересекаются, например [01.02-31.03]-[01.04-30.04]
    /// Возвращается массив из одного элемента, когда есть одно пересечение. Например, для [01.02-31.05]-[01.03-30.06] возвращается [01.03-31.05].
    /// Возвращается пересечение из двух интервалов для двух пересечений. Например, для [01.02-30.09]-[01.08-31.03] возвращается [01.02-31.03] и [01.08-30.09].
    /// При наличии двух пересечений интервалы сортируются по возрастанию начальной даты.
    /// Если интервалы не пересекаются, или один из интервалов пустой, возвращается пустой интервал с Tag=null.
    /// Если обменять местами аргументы, то результат должен быть тот же самый, за исключением свойства Tag.
    /// </summary>
    /// <param name="r1">Первый диапазон</param>
    /// <param name="r2">Второй диапазон</param>
    /// <returns>Общий интервал</returns>
    public static MonthDayRange[] GetCrosses(MonthDayRange r1, MonthDayRange r2)
    {
      if (r1.IsEmpty || r2.IsEmpty)
        return _EmptyArray;
      if (r2.IsWholeYear)
        return new MonthDayRange[1] { r1 };
      if (r1.IsWholeYear)
        return new MonthDayRange[1] { new MonthDayRange(r2, r1.Tag) };

      CrossResults res = GetCrossResult(r1.First, r1.Last, r2.First, r2.Last);
      switch (res)
      {
        case CrossResults.AB: return new MonthDayRange[1] { r1 };
        case CrossResults.CD: return new MonthDayRange[1] { new MonthDayRange(r2, r1.Tag) };
        case CrossResults.AD: return new MonthDayRange[1] { new MonthDayRange(r1.First, r2.Last, r1.Tag) };
        case CrossResults.CB: return new MonthDayRange[1] { new MonthDayRange(r2.First, r1.Last, r1.Tag) };
        case CrossResults.ADCB:
          if (r1.First <= r2.First)
            return new MonthDayRange[2] { new MonthDayRange(r1.First, r2.Last, r1.Tag), new MonthDayRange(r2.First, r1.Last, r1.Tag) };
          else
            return new MonthDayRange[2] { new MonthDayRange(r2.First, r1.Last, r1.Tag), new MonthDayRange(r1.First, r2.Last, r1.Tag) };
        case CrossResults.None: return _EmptyArray;
        default: throw new BugException();
      }
    }

    private static readonly MonthDayRange[] _EmptyArray = new MonthDayRange[0];

    /*
     * Теория.
     * Пусть первый интервал [AB] и второй [CD]. Имеются 4 даты, которые можно сравнивать на больше-равно. 
     * Это дает 6 пар (AB, CD, AC, BD, AD, BC), для каждой из которых возможны два результата, итого, 64 комбинации.
     * В результирующие периоды всегда входят начало одного периода и окончание другого.
     * Возможные комбинации: Пусто, [AB], [CD], [AD], [BC], [AD]+[CB].
     * 
     * Можно сначала выполнить сравнения дат, получить номер комбинации от 0-12, а потом выбрать один из 6 результатов
     */

    [Flags]
    private enum CrossResults
    {
      None = 0,
      AB = 1,
      CD = 2,
      AD = 4,
      CB = 8,
      ADCB = AD | CB
    };

    private static CrossResults GetCrossResult(MonthDay A, MonthDay B, MonthDay C, MonthDay D)
    {
      // Так как интервалы [AB] и [CD] можно поменять местами, можно сократить количество комбинаций в два раза

      if (A > C)
      {
        CrossResults res = GetCrossResult2(C, D, A, B);
        switch (res)
        {
          case CrossResults.AB: return CrossResults.CD;
          case CrossResults.CD: return CrossResults.AB;
          case CrossResults.AD: return CrossResults.CB;
          case CrossResults.CB: return CrossResults.AD;
          default: return res;
        }
      }
      else
        return GetCrossResult2(A, B, C, D);
    }

    private static CrossResults GetCrossResult2(MonthDay A, MonthDay B, MonthDay C, MonthDay D)
    {
      if (A <= B) // первый интервал прямой
      {
        if (C <= D) // второй интервал прямой
        {
          if (C <= B)
            return CrossResults.CB;
          else
            return CrossResults.None;
        }
        else // второй интервал вывернутый
        {
          if (D >= B)
            return CrossResults.AB;
          else
          {
            CrossResults res = CrossResults.None;
            if (C <= B)
              res |= CrossResults.CB;
            if (A <= D)
              res |= CrossResults.AD;
            return res;
          }
        }
      }
      else  // первый интервал вывернутый
      {
        if (C <= D) // второй интервал прямой
        {
          return CrossResults.CD;
        }
        else // второй интервал вывернутый
        {
          CrossResults res = CrossResults.CB;
          if (A <= D)
            res |= CrossResults.AD;
          return res;
        }
      }
    }


    /// <summary>
    /// Возвращает true, если два диапазона полностью или частично перекрываются.
    /// Если один или оба диапазона пустые, возвращается false.
    /// Если обменять местами аргументы, то результат должен быть тот же самый, за исключением свойства Tag.
    /// </summary>
    /// <param name="r1">Первый диапазон</param>
    /// <param name="r2">Второй диапазон</param>
    /// <returns>Наличие пересечения</returns>
    public static bool IsCrossed(MonthDayRange r1, MonthDayRange r2)
    {
#if XXX

#pragma warning disable 0618 // Obsolete
      return !GetCross(r1, r2).IsEmpty;
#pragma warning restore 0618

#else

      if (r1.IsEmpty || r2.IsEmpty)
        return false;
      if (r1.IsWholeYear || r2.IsWholeYear)
        return true;

      return GetCrossResult(r1.First, r1.Last, r2.First, r2.Last) != CrossResults.None;
#endif
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает диапазон дат для заданного года, соответствующий текущему диапазону.
    /// При этом даты полученного диапазона будут относится к одному году, если <see cref="Last"/> больше или равно <see cref="First"/> и к разным годам,
    /// когда <see cref="First"/> больше <see cref="Last"/>.
    /// Если последний день текущего диапазона <see cref="Last"/> задает 28 февраля и год конечной даты является високосным, конечная дата заменяется на 29 февраля.
    /// Для начальной даты такая замена не выполняется.
    /// <see cref="DateRange.Tag"/> берется из текущего объекта (свойство <see cref="Tag"/>).
    /// Если <see cref="IsEmpty"/>=true, то возвращается <see cref="DateRange.Empty"/> с пустым <see cref="DateRange.Tag"/>.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="yearIsForLastDay">Если false, то год <paramref name="year"/> будет применен к первому дню. При этом конечная дата диапазона может оказаться
    /// в следующем году. 
    /// Если true, то год будет применен к последнему дню, а начальная дата диапазона может оказаться в предыдущем году</param>
    /// <returns>Диапазон дат</returns>
    public DateRange GetDateRange(int year, bool yearIsForLastDay)
    {
      if (IsEmpty)
        return DateRange.Empty;

      DateTime dt1, dt2;
      if (yearIsForLastDay)
      {
        dt2 = _Last.GetDate(year, true);
        if (_Last >= _First)
          dt1 = _First.GetDate(year, false);
        else
          dt1 = _First.GetDate(year - 1, false);
      }
      else
      {
        dt1 = _First.GetDate(year, false);
        if (_Last >= _First)
          dt2 = _Last.GetDate(year, true);
        else
          dt2 = _Last.GetDate(year + 1, true);
      }
      return new DateRange(dt1, dt2, Tag);
    }

    /// <summary>
    /// Возвращает диапазон дат для заданного года, соответствующий текущему диапазону.
    /// При этом даты полученного диапазона будут относится к одному году, если <see cref="Last"/> больше или равно <see cref="First"/> и к разным годам,
    /// когда <see cref="First"/> больше <see cref="Last"/>.
    /// Если последний день текущего диапазона <see cref="Last"/> задает 28 февраля и год конечной даты является високосным, конечная дата заменяется на 29 февраля.
    /// Для начальной даты такая замена не выполняется.
    /// Если <see cref="IsEmpty"/>=true, то возвращается <see cref="DateRange.Empty"/>.
    /// </summary>
    /// <param name="year">Год для начальной даты диапазона</param>
    /// <returns>Диапазон дат</returns>
    public DateRange GetDateRange(int year)
    {
      return GetDateRange(year, false);
    }

    /// <summary>
    /// Сдвигает диапазон на указанное число дней. Високосные года не учитываются.
    /// Если <see cref="IsEmpty"/>=true, возвращается также пустая структура.
    /// </summary>
    /// <param name="days">Сдвиг в днях</param>
    /// <returns>Новый диапазон</returns>
    public MonthDayRange AddDays(int days)
    {
      if (IsEmpty)
        return Empty;

      return new MonthDayRange(_First + days, _Last + days, Tag);
    }

    /// <summary>
    /// Возвращает текстовое представление интервала дат и поля <see cref="Tag"/>, если оно задано.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = DateRangeFormatter.Default.ToString(this, false);
      if (_Tag != null)
        s += ", Tag=" + _Tag.ToString();
      return s;
    }

    #endregion

    #region Статические свойства

    /// <summary>
    /// Пустая структура, означающая отсутствие периода.
    /// В такой "период" не попадает ни один день.
    /// </summary>
    public static readonly MonthDayRange Empty = new MonthDayRange();

    /// <summary>
    /// Структура, задающая диапазон "весь год", в которую попадает любой день
    /// </summary>
    public static readonly MonthDayRange WholeYear = new MonthDayRange(MonthDay.BottomOfYear, MonthDay.EndOfYear);

    #endregion

    #region Операторы сравнения

    /// <summary>
    /// Возвращает true, если периоды одинаковы.
    /// Если оба периода задают целый год, периоды также считаются одинаковыми.
    /// </summary>
    /// <param name="a">Первый сравниваемый период</param>
    /// <param name="b">Второй сравниваемый период</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(MonthDayRange a, MonthDayRange b)
    {
      if (a._First == b._First && a._Last == b._Last)
        return true;
      if (a.Days == 365 && b.Days == 365)
        return true;
      return false;
    }

    /// <summary>
    /// Возвращает false, если периоды одинаковы.
    /// Если оба периода задают целый год, периоды также считаются одинаковыми.
    /// </summary>
    /// <param name="a">Первый сравниваемый период</param>
    /// <param name="b">Второй сравниваемый период</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(MonthDayRange a, MonthDayRange b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Возвращает true, если текущий период совпадает с заданным.
    /// Если оба периода задают целый год, периоды также считаются одинаковыми.
    /// </summary>
    /// <param name="obj">Второй сравниваемый период</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is MonthDayRange)
        return this == (MonthDayRange)obj;
      else
        return false;
    }

    /// <summary>
    /// Возвращает хэш-код для коллекций
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return _First.GetHashCode(); // Можно обойтись одной половинкой
    }

    #endregion

    #region IEnumerable<MonthDay> Members

    /// <summary>
    /// Перечислитель по дням
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<MonthDay>
    {
      #region Конструктор

      internal Enumerator(MonthDayRange range)
      {
        _First = range._First;
        _Days = range.Days;
        _Count = -1;
      }

      #endregion

      #region Поля

      /// <summary>
      /// Первый день диапазона
      /// </summary>
      private MonthDay _First;

      /// <summary>
      /// Количество дней в диапазоне
      /// </summary>
      int _Days;

      /// <summary>
      /// Счетчик
      /// </summary>
      int _Count;

      #endregion

      #region IEnumerator<MonthDay> Members

      /// <summary>
      /// Текущий день
      /// </summary>
      public MonthDay Current { get { return _First.AddDays(_Count); } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose() { }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к следующему дню
      /// </summary>
      /// <returns>false, если перечисление закончено</returns>
      public bool MoveNext()
      {
        _Count++;
        return _Count < _Days;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Count = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель, который перебирает все дни в диапазоне, как структуры <see cref="MonthDay"/>.
    /// Если требуется перечислять объекты <see cref="DateTime"/>, используйте метод <see cref="GetDateRange(int, bool)"/> и выполняйте перечисление для <see cref="DateRange"/>.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<MonthDay> IEnumerable<MonthDay>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}
