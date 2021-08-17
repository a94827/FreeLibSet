using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;

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

namespace AgeyevAV
{
  /// <summary>
  /// Структура для хранения дня и месяца (применимых к любому году).
  /// Хранит номер дня в году как число от 1 до 365 (29 февраля отсутствует).
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  [Serializable]
  public struct MonthDay : IEquatable<MonthDay>, IComparable<MonthDay>
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
        throw new ArgumentOutOfRangeException("dayOfYear", dayOfYear, "Значение должно быть равно 0 или лежать в диапазоне от 1 до 365");
      _DayOfYear = (short)dayOfYear;
    }

    /// <summary>
    /// Заполняет структуру.
    /// Эта версия конструктора не позволяет инициализировать пустую структуру
    /// </summary>
    /// <param name="month">Месяц в диапазоне от 1 до 12</param>
    /// <param name="day">День в диапазоне от 1 до 28,30 или 31, в зависимости от месяца. 29 февраля не допускается.</param>
    public MonthDay(int month, int day)
    {
      DateTime dt = new DateTime(2001, month, day);
      _DayOfYear = (short)(dt.DayOfYear);
    }

    /// <summary>
    /// Заполняет структуру из DateTime.
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
    /// Для Empty возвращает 0.
    /// </summary>
    public int DayOfYear { get { return _DayOfYear; } }
    private readonly short _DayOfYear;

    /// <summary>
    /// Дата, от которой отсчитывается DayOfYear.
    /// Используется для получения свойств Month и Day
    /// </summary>
    private static readonly DateTime BaseDate = new DateTime(2000, 12, 31);

    /// <summary>
    /// Месяц в диапазоне от 1 до 12.
    /// Для пустого значения Empty возвращает 0.
    /// </summary>
    public int Month
    {
      get
      {
        if (DayOfYear == 0)
          return 0;
        else
          return BaseDate.AddDays(DayOfYear).Month;
      }
    }

    /// <summary>
    /// День в диапазоне от 1 до 28,30 или 31 (в зависимости от месяца)
    /// Для пустого значения Empty возвращает 0.
    /// </summary>
    public int Day
    {
      get
      {
        if (DayOfYear == 0)
          return 0;
        else
          return BaseDate.AddDays(DayOfYear).Day;
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
    /// Для пустой структуры Empty возвращает ее же.
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
    /// Предыдущий день (29 февраля не бывает)/
    /// Для пустой структуры Empty возвращает ее же.
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
    /// Добавляет к значению MonthDay заданное количество дней.
    /// Предполагается, что в году 365 дней
    /// </summary>
    /// <param name="monthDay">Текущее значение. Не может быть пустой структурой</param>
    /// <param name="days">Количество добавляемых дней. Значение может быть положительным и отрицательным</param>
    /// <returns>Новое значение</returns>
    public static MonthDay operator +(MonthDay monthDay, int days)
    {
      if (monthDay.IsEmpty)
        throw new ArgumentException("Пустая дата", "monthDay");

      days = days % 365;

      int v = monthDay.DayOfYear + days;
      if (v < 1)
        v += 365;
      if (v > 365)
        v -= 365;
      return new MonthDay(v);
    }

    /// <summary>
    /// Вычитает из MonthDay заданное количество дней.
    /// Предполагается, что в году 365 дней.
    /// </summary>
    /// <param name="monthDay">Текущее значение. Не может быть пустой структурой</param>
    /// <param name="days">Количество вычитаемых дней. Значение может быть положительным и отрицательным</param>
    /// <returns>Новое значение</returns>
    public static MonthDay operator -(MonthDay monthDay, int days)
    {
      if (monthDay.IsEmpty)
        throw new ArgumentException("Пустая дата", "monthDay");

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
        throw new ArgumentException("Пустая первая дата", "a");
      if (b.IsEmpty)
        throw new ArgumentException("Пустая вторая дата", "b");

      int days= a.DayOfYear - b.DayOfYear;
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
        throw new InvalidOperationException("Структура не инициализирована");
      if (february29 && DayOfYear == DayFeb28)
        return DataTools.EndOfMonth(year, 2);
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
        format = "dd MMMM";

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
    /// Также поддерживает сравнение с DateTime.
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
    /// <param name="month">Номер месяцв (1-12)</param>
    /// <returns></returns>
    public static int DaysInMonth(int month)
    {
      return DateTime.DaysInMonth(2001, month);
    }

    /// <summary>
    /// Получить nullable-значение из заданного для и месяца.
    /// </summary>
    /// <param name="month">Месяц. Может быть в диапазоне от 0 до 12</param>
    /// <param name="day">День, может быть от 0 до 31</param>
    /// <returns>Заполненная структура или null</returns>
    [Obsolete("Структура MonthDay теперь поддерживает пустое значение Empty. Нет причин использовать nullable-значение", false)]
    public static Nullable<MonthDay> NullableFromMonthDay(int month, int day)
    {
      if (month == 0 || day == 0)
        return null;
      else
        return new MonthDay(month, day);
    }

    /// <summary>
    /// Получить значение из заданного для и месяца.
    /// В отличие от однотипного конструктора, позволяет передать значения 0.
    /// </summary>
    /// <param name="month">Месяц. Может быть в диапазоне от 0 до 12</param>
    /// <param name="day">День, может быть от 0 до 31</param>
    /// <returns>Заполненная или пустая структура</returns>
    public static MonthDay FromMonthDay(int month, int day)
    {
      if (month == 0 || day == 0)
        return Empty;
      else
        return new MonthDay(month, day);
    }

    /// <summary>
    /// Получить null или заполненную структуру из строки DataRow
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <param name="monthFieldName">Имя числового поля с номером месяца</param>
    /// <param name="dayFieldName">Имя числового поля с номером дня</param>
    /// <returns>Структура MonthDay или null</returns>
    [Obsolete("Структура MonthDay теперь поддерживает пустое значение Empty. Нет причин использовать nullable-значение", false)]
    public static Nullable<MonthDay> NullableFromDataRow(DataRow row, string monthFieldName, string dayFieldName)
    {
      int Month = DataTools.GetInt(row, monthFieldName);
      int Day = DataTools.GetInt(row, dayFieldName);

#pragma warning disable 0618 // Подавление сообщения Obsolete
      return NullableFromMonthDay(Month, Day);
#pragma warning restore 0618
    }

    /// <summary>
    /// Получить структуру из строки DataRow.
    /// Может быть возвращена пустая структура Empty
    /// </summary>
    /// <param name="row">Строка таблицы</param>
    /// <param name="monthFieldName">Имя числового поля с номером месяца</param>
    /// <param name="dayFieldName">Имя числового поля с номером дня</param>
    /// <returns>Структура MonthDay</returns>
    public static MonthDay FromDataRow(DataRow row, string monthFieldName, string dayFieldName)
    {
      int Month = DataTools.GetInt(row, monthFieldName);
      int Day = DataTools.GetInt(row, dayFieldName);
      return FromMonthDay(Month, Day);
    }

    // Метод не нужен, так как есть однотипный конструктор

    ///// <summary>
    ///// Получить структуру или null из порядкового номера дня в году.
    ///// Для нулевого значения возвращается пустая структура Empty
    ///// </summary>
    ///// <param name="IntValue">Номер дня в невисокосном году (1-365) или 0</param>
    ///// <returns>Структура MonthDay или null</returns>
    //public static MonthDay? NullableFromIntValue(int IntValue)
    //{
    //  if (IntValue == 0)
    //    return null;
    //  else
    //    return FromIntValue(IntValue);
    //}

    ///// <summary>
    ///// Получить структуру из порядкового номера дня в году.
    ///// Для нулевого значения возвращается пустая структура Empty
    ///// </summary>
    ///// <param name="IntValue">Номер дня в невисокосном году (1-365) или 0</param>
    ///// <returns>Структура MonthDay</returns>
    //public static MonthDay FromIntValue(int IntValue)
    //{
    //  if (IntValue < 0 || IntValue > 365)
    //    throw new ArgumentOutOfRangeException("IntValue", IntValue, "Номер дня в году должен быть в диапазоне от 1 до 365 или значение 0 - пустое значение");
    //  if (IntValue == 0)
    //    return Empty;

    //  DateTime dt = new DateTime(2001, 1, 1);
    //  dt = dt.AddDays(IntValue - 1);
    //  return new MonthDay(dt);
    //}

    #endregion
  }

  /// <summary>
  /// Диапазон дней.
  /// В отличие от DateRange, эта структура предполагает годовую цикличность.
  /// Поэтому свойство First может быть и больше Last.
  /// Если First.IsEmpty и Last.IsEmpty, то структура считается неинициализированной (IsEmpty=true). Не путать с полным периодом (IsWholeYear=true)
  /// Структура также содержит поле Tag, которое может хранить произвольные пользовательские данные
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  [Serializable]
  public struct MonthDayRange : IEnumerable<MonthDay>
  {
    #region Конструкторы

    /// <summary>
    /// Создает структуру.
    /// Первый и последний день диапазона должны быть оба заполнены или оба равны MonthDay.Empty для создания пустой структуры.
    /// Не может задаваться половинчатый диапазон, когда First.IsEmpty не равно Last.IsEmpty.
    /// Свойство Tag будет иметь значение null.
    /// </summary>
    /// <param name="first">Первый день диапазона</param>
    /// <param name="last">Последний день диапазона</param>
    public MonthDayRange(MonthDay first, MonthDay last)
      : this(first, last, null)
    {
    }

    /// <summary>
    /// Создает структуру.
    /// Первый и последний день диапазона должны быть оба заполнены или оба равны MonthDay.Empty для создания пустой структуры.
    /// Не может задаваться половинчатый диапазон, когда First.IsEmpty не равно Last.IsEmpty
    /// </summary>
    /// <param name="first">Первый день диапазона</param>
    /// <param name="last">Последний день диапазона</param>
    /// <param name="tag">Произвольные пользовательские данные</param>
    public MonthDayRange(MonthDay first, MonthDay last, object tag)
    {
      if (!(first.IsEmpty && last.IsEmpty))
      {
        if (first.IsEmpty)
          throw new ArgumentException("Пустое значение First", "first");
        if (last.IsEmpty)
          throw new ArgumentException("Пустое значение Last", "last");
      }

      _First = first;
      _Last = last;
      _Tag = tag;
    }

    /// <summary>
    /// Создает период из одного дня.
    /// Свойство Tag будет иметь значение null.
    /// </summary>
    /// <param name="day">Начало и конец диапазона</param>
    public MonthDayRange(MonthDay day)
      : this(day, day)
    {
    }

    /// <summary>
    /// Создает структуру для заданного диапазона дат.
    /// Если диапазон <paramref name="range"/> охватывает больше одного года (например, 05.07.2019-05.07.2020),
    /// то создается полный диапазон IsWholeYear=true, и даты из аргумента не используются.
    /// Если <paramref name="range"/>.IsEmpty=true, то создается пустой диапазон.
    /// Свойство Tag берется из DateRange.Tag
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

        int DeltaY = range.LastDate.Year - range.FirstDate.Year;
        bool SetWholeYear = false;
        switch (DeltaY)
        {
          case 0: // в пределах одного года
            break;
          case 1: // Два соседних года
            if (_Last >= _First)
              SetWholeYear = true;
            break;
          default:
            SetWholeYear = true;
            break;
        }
        if (SetWholeYear)
        {
          _First = MonthDay.BottomOfYear;
          _Last = MonthDay.EndOfYear;
        }
      }

      _Tag = range.Tag;
    }

    /// <summary>
    /// Создает копию диапазона, устанавливая новое значение свойства Tag
    /// </summary>
    /// <param name="source">Исходный объект</param>
    /// <param name="tag">Значение свойства Tag</param>
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
    /// Для пустого периода (IsEmpty=true) возвращает 0.
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
          return 364 - _First.DayOfYear + _Last.DayOfYear;
      }
    }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } }
    private object _Tag;

    /// <summary>
    /// Получить комплементарный интервал дат. Например, для периода "10 октября-15 мая"
    /// возвращается интервал "16 мая-09 октября".
    /// Свойство Tag дублируется в создаваемый интервал.
    /// Если IsEmpty=true, возвращается пустой интервал
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

    #endregion

    #region Попадание в интервал

    /// <summary>
    /// Возвращает попадание дня в текущий интервал.
    /// Если <paramref name="day"/>.IsEmpty=true или текущая структура не заполнена (IsEmpty=true), возвращает false
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
    /// Возвращает true, если два диапазона полностью или частично перекрываются.
    /// Если один или оба диапазона пустые, возвращается false
    /// </summary>
    /// <param name="r1">Первый диапазон</param>
    /// <param name="r2">Второй диапазон</param>
    /// <returns>Наличие пересечения</returns>
    public static bool IsCroosed(MonthDayRange r1, MonthDayRange r2)
    {
      return !GetCross(r1, r2).IsEmpty;
    }


    /// <summary>
    /// Возвращает пересечение двух интервалов. Поле Tag берется из первого интервала
    /// Если интервалы не пересекаются, или один из интервалов пустой, возвращается пустой интервал с Tag=null.
    /// </summary>
    /// <param name="r1">Первый диапазон</param>
    /// <param name="r2">Второй диапазон</param>
    /// <returns>Общий интервал</returns>
    public static MonthDayRange GetCross(MonthDayRange r1, MonthDayRange r2)
    {
      if (r1.IsEmpty || r2.IsEmpty)
        return Empty;

      // Хорошо бы сделать без DateRange, но это очень сложно
      DateRange dr1 = r1.GetDateRange(2001); // точно без високосного года
      DateRange dr2 = r2.GetDateRange(2001);
      DateRange dr = DateRange.GetCross(dr1, dr2);
      return new MonthDayRange(dr); // поле Tag копируется правильно.
    }

    /// <summary>
    /// Попадает ли заданная дата в интервал
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true, если дата попадает в диапазон</returns>
    public bool Contains(DateTime date)
    {
      MonthDay md = new MonthDay(date);
      return Contains(md);
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает диапазон дат для заданного года, соответствующий текущему диапазону.
    /// При этом даты полученного диапазона будут относится к одному году, если Last больше или равно First и к разным годам,
    /// когда First больше Last.
    /// Если последний день текущего диапазона Last задает 28 февраля и год конечной даты является високосным, конечная дата заменяется на 29 февраля.
    /// Для начальной даты такая замена не выполняется.
    /// DateRange.Tag берется из текущего объекта.
    /// Если IsEmpty=true, то возвращается DateRange.Empty с пустым Tag.
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
    /// При этом даты полученного диапазона будут относится к одному году, если Last больше или равно First и к разным годам,
    /// когда First больше Last.
    /// Если последний день текущего диапазона Last задает 28 февраля и год конечной даты является високосным, конечная дата заменяется на 29 февраля.
    /// Для начальной даты такая замена не выполняется.
    /// Если IsEmpty=true, то возвращается DateRange.Empty.
    /// </summary>
    /// <param name="year">Год для начальной даты диапазона</param>
    /// <returns>Диапазон дат</returns>
    public DateRange GetDateRange(int year)
    {
      return GetDateRange(year, false);
    }

    /// <summary>
    /// Сдвигает диапазон на указанное число дней. Високосные года не учитываются.
    /// Если IsEmpty=true, возвращается также пустая структура
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
    /// Возвращает текстовое представление интервала дат и поля Tag, если оно задано
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
    /// Если оба периода задают целый год, периоды также считаются одинаковыми
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
    /// Если оба периода задают целый год, периоды также считаются одинаковыми
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
      /// Текущий месяц
      /// </summary>
      public MonthDay Current { get { return _First.AddDays(_Count); } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose() { }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к следующему месяцу
      /// </summary>
      /// <returns></returns>
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
    /// Возвращает перечислитель, который перебирает все дни в диапазоне, как структуры MonthDay.
    /// </summary>
    /// <returns></returns>
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
