// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FreeLibSet.Calendar
{
  /// <summary>
  /// Структура для хранения года и месяца
  /// </summary>
  [Serializable]
  public struct YearMonth : IComparable<YearMonth>, IComparable, IEquatable<YearMonth>
  {
    #region Константы

    /// <summary>
    /// Минимальное значения для поля "Год"
    /// </summary>
    public const int MinYear = 1;

    /// <summary>
    /// Максимальное значение для поля "Год"
    /// </summary>
    public const int MaxYear = 5460;

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает объект для заданного года и месяца
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц (1 - 12)</param>
    public YearMonth(int year, int month)
    {
#if DEBUG
      if (year < MinYear || year > MaxYear)
        throw new ArgumentOutOfRangeException("year", year, "Год должен быть в диапазоне от " + MinYear.ToString() + " до " + MaxYear.ToString());
      if (month < 1 || month > 12)
        throw new ArgumentOutOfRangeException("month", month, "Месяц должен быть в диапазоне от 1 до 12");
#endif
      _Value = (UInt16)(year * 12 + month - 1);
    }

    /// <summary>
    /// Создает объект, извлекая год и месяц из объекта <see cref="DateTime"/>
    /// </summary>
    /// <param name="date">Дата</param>
    public YearMonth(DateTime date)
      : this(date.Year, date.Month)
    {
    }

    internal YearMonth(int value)
    {
      _Value = (UInt16)value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Год в диапазоне <see cref="MinYear"/>-<see cref="MaxYear"/>.
    /// Если <see cref="IsEmpty"/>=true, возвращает 0.
    /// </summary>
    public int Year { get { return _Value / 12; } }

    /// <summary>
    /// Месяц (1-12).
    /// Если <see cref="IsEmpty"/>=true, значение не определено
    /// </summary>
    public int Month { get { return _Value % 12 + 1; } }

    /// <summary>
    /// Значение, хранящее номер года *12 плюс месяц
    /// </summary>
    internal UInt16 Value { get { return _Value; } }
    private readonly UInt16 _Value;

    /// <summary>
    /// Возвращает true, если структура не заполнена
    /// </summary>
    public bool IsEmpty { get { return _Value == 0; } }

    /// <summary>
    /// Первый день месяца
    /// </summary>
    public DateTime BottomOfMonth
    {
      get
      {
        if (_Value == 0)
          return DateRange.Whole.FirstDate;
        else
          return new DateTime(Year, Month, 1);
      }
    }

    /// <summary>
    /// Последний день месяца
    /// </summary>
    public DateTime EndOfMonth
    {
      get
      {
        if (_Value == 0)
          return DateRange.Whole.LastDate; // 14.10.2021
        else
          return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
      }
    }

    /// <summary>
    /// Возвращает интервал дат, содержащий месяц.
    /// Если текущий объект не инициализирован, возвращается <see cref="DateRange.Empty"/>.
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        if (_Value == 0)
          return DateRange.Empty;
        else
          return new DateRange(BottomOfMonth, EndOfMonth, null);
      }
    }

    #endregion

    #region Попадание

    /// <summary>
    /// Попадает ли заданная дата в интервал
    /// </summary>
    /// <param name="date">Проверяемая дата</param>
    /// <returns>true, если дата попадает в диапазон</returns>
    public bool Contains(DateTime date)
    {
      return date.Year == Year && date.Month == Month;
    }

    /// <summary>
    /// Корректирует дату <paramref name="date"/>, если она не попадает в диапазон
    /// </summary>
    /// <param name="date">Проверяемая/корректируемая дата</param>
    public void DateToRange(ref DateTime date)
    {
      DateRange.DateToRange(ref date);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Текстовое представление в формате "Год/Месяц"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      else
        return BottomOfMonth.ToString("MM/yyyy");
    }

    /// <summary>
    /// Используется для реализации коллекций
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _Value.GetHashCode();
    }

    /// <summary>
    /// Сравнение с другим <see cref="YearMonth"/>.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(YearMonth other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим <see cref="YearMonth"/>.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is YearMonth)
        return _Value.Equals(((YearMonth)other)._Value);
      else
        return false;
    }

    internal void CheckNoEmpty()
    {
      if (IsEmpty)
        throw new NullReferenceException("Структура YearMonth не была инициализирована");
    }

    /// <summary>
    /// Возвращает true, если текущее значение находится в указанном диапазоне.
    /// Учитываются полуоткрытые диапазоны, когда граница задана как <see cref="YearMonth.IsEmpty"/>=true.
    /// </summary>
    /// <param name="minimum">Начальный месяц/год диапазлна</param>
    /// <param name="maximum">Последний месяц/год диапазлна</param>
    /// <returns>true, если текущий месяц входит в диапазон</returns>
    public bool IsInRange(YearMonth minimum, YearMonth maximum)
    {
      if (IsEmpty)
        return false;
      if (!minimum.IsEmpty)
      {
        if (this < minimum)
          return false;
      }
      if (!maximum.IsEmpty)
      {
        if (this > maximum)
          return false;
      }
      return true;
    }

    #endregion

    #region Операторы

    /// <summary>
    /// Прибавляет к объекту <see cref="YearMonth"/> заданное число месяцев.
    /// </summary>
    /// <param name="value1">Объект <see cref="YearMonth"/></param>
    /// <param name="months">Количество добавляемых месяцев. Допускаются отрицательные значения</param>
    /// <returns>Новый объект <see cref="YearMonth"/></returns>
    public static YearMonth operator +(YearMonth value1, int months)
    {
#if DEBUG
      value1.CheckNoEmpty();
#endif
      return new YearMonth(value1._Value + months);
    }

    /// <summary>
    /// Вычитает из объекта <see cref="YearMonth"/> заданное число месяцев.
    /// </summary>
    /// <param name="value2">Объект <see cref="YearMonth"/></param>
    /// <param name="months">Количество вычитаемых месяцев. Допускаются отрицательные значения</param>
    /// <returns>Новый объект <see cref="YearMonth"/></returns>
    public static YearMonth operator -(YearMonth value2, int months)
    {
#if DEBUG
      value2.CheckNoEmpty();
#endif
      return new YearMonth(value2._Value - months);
    }

    /// <summary>
    /// Вычитает из одного <see cref="YearMonth"/> другой и возвращает разность между ними как число месяцев
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>Количество месяцев</returns>
    public static int operator -(YearMonth value1, YearMonth value2)
    {
#if DEBUG
      value1.CheckNoEmpty();
      value2.CheckNoEmpty();
#endif
      return value1._Value - value2._Value;
    }

#if XXX // Убрано 09.03.2023. Не должно быть таких операторов, т.к. YearMonth является неизменяемой структурой

    /// <summary>
    /// Прибавляет к объекту YearMonth один месяц
    /// </summary>
    /// <param name="value">Объект YearMonth</param>
    /// <returns>Новый объект YearMonth</returns>
    public static YearMonth operator ++(YearMonth value)
    {
#if DEBUG
      value.CheckNoEmpty();
#endif
      return new YearMonth(value._Value + 1);
    }

    /// <summary>
    /// Вычитает из объекта YearMonth один месяц
    /// </summary>
    /// <param name="value">Объект YearMonth</param>
    /// <returns>Новый объект YearMonth</returns>
    public static YearMonth operator --(YearMonth value)
    {
#if DEBUG
      value.CheckNoEmpty();
#endif
      return new YearMonth(value._Value - 1);
    }

#endif

    /// <summary>
    /// Проверяет два объекта <see cref="YearMonth"/> на равенство
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool operator ==(YearMonth value1, YearMonth value2)
    {
      return value1._Value == value2._Value;
    }

    /// <summary>
    /// Проверяет два объекта <see cref="YearMonth"/> на неравенство
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>false, если значения одинаковы</returns>
    public static bool operator !=(YearMonth value1, YearMonth value2)
    {
      return value1._Value != value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта <see cref="YearMonth"/> 
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>true, первый объект меньше второго</returns>
    public static bool operator <(YearMonth value1, YearMonth value2)
    {
      return value1._Value < value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта <see cref="YearMonth"/> 
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>true, первый объект больше второго</returns>
    public static bool operator >(YearMonth value1, YearMonth value2)
    {
      return value1._Value > value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта <see cref="YearMonth"/>
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>true, первый объект меньше или равен второму</returns>
    public static bool operator <=(YearMonth value1, YearMonth value2)
    {
      return value1._Value <= value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта <see cref="YearMonth"/> 
    /// </summary>
    /// <param name="value1">Первый объект <see cref="YearMonth"/></param>
    /// <param name="value2">Второй объект <see cref="YearMonth"/></param>
    /// <returns>true, первый объект больше или равен второму</returns>
    public static bool operator >=(YearMonth value1, YearMonth value2)
    {
      return value1._Value >= value2._Value;
    }

    #endregion

    #region IComparable<YearMonth> Members

    /// <summary>
    /// Сравнение с другим периодом.
    /// Если текущий интервал больше <paramref name="other"/>, возвращается положительное число.
    /// Если текущий интервал меньше <paramref name="other"/>, возвращается отрицательное число.
    /// Если интервалы равны, возвращается 0.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public int CompareTo(YearMonth other)
    {
      return _Value.CompareTo(other._Value);
    }

    #endregion

    #region IComparable Members

    int IComparable.CompareTo(object other)
    {
      if (other is YearMonth)
        return _Value.CompareTo(((YearMonth)other)._Value);
      else
        return -1;
    }

    #endregion
  }

  /// <summary>
  /// Структура, задающая диапазон месяцев.
  /// Месяцы не обязаны относиться к одному году.
  /// Открытые интервалы не поддерживаются.
  /// </summary>
  [Serializable]
  public struct YearMonthRange : IEnumerable<YearMonth>
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый диапазон.
    /// Начало и конец диапазона должны быть заданы (<see cref="YearMonth.IsEmpty"/>==false).
    /// Конец диапазоне не может быть меньше, чем начало.
    /// </summary>
    /// <param name="firstYM">Начало диапазона</param>
    /// <param name="lastYM">Конец диапазона</param>
    public YearMonthRange(YearMonth firstYM, YearMonth lastYM)
    {
      if (firstYM.IsEmpty)
        throw new ArgumentException("Не задан начальный месяц/год", "firstYM");
      if (lastYM.IsEmpty)
        throw new ArgumentException("Не задан последний месяц/год", "lastYM");
      if (firstYM > lastYM)
        throw new ArgumentException("Первый месяц больше чем последний", "lastYM");

      _FirstYM = firstYM;
      _LastYM = lastYM;
    }

    /// <summary>
    /// Создает диапазон, извлекая год и месяц из <see cref="DateTime"/>.
    /// Значение поля "День" игнорируется.
    /// Год <see cref="DateTime.Year"/> у обеих дат должен быть в допустимом диапазоне <see cref="YearMonth.MinYear"/> - <see cref="YearMonth.MaxYear"/>.
    /// </summary>
    /// <param name="firstDate">Начало диапазона</param>
    /// <param name="lastDate">Конец диапазона</param>
    public YearMonthRange(DateTime firstDate, DateTime lastDate)
      : this(new YearMonth(firstDate), new YearMonth(lastDate))
    {
    }

    /// <summary>
    /// Создает диапазон, содержащий 12 месяцев
    /// </summary>
    /// <param name="year">Год</param>
    public YearMonthRange(int year)
      : this(new YearMonth(year, 1), new YearMonth(year, 12))
    {
    }

    /// <summary>
    /// Создает диапазон, содержащий 1 месяц.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц (1 - 12)</param>
    public YearMonthRange(int year, int month)
      : this(new YearMonth(year, month), new YearMonth(year, month))
    {
    }

    /// <summary>
    /// Создает диапазон месяцев в пределах одного года 
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="firstMonth">Первый месяц (1-12)</param>
    /// <param name="lastMonth">Последний месяц (1-12)</param>
    public YearMonthRange(int year, int firstMonth, int lastMonth)
      : this(new YearMonth(year, firstMonth), new YearMonth(year, lastMonth))
    {
    }

    /// <summary>
    /// Создает диапазон, содержащий 1 месяц.
    /// </summary>
    /// <param name="month">Год и месяц</param>
    public YearMonthRange(YearMonth month)
      : this(month, month)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Первый месяц диапазона
    /// </summary>
    public YearMonth FirstYM { get { return _FirstYM; } }
    private readonly YearMonth _FirstYM;

    /// <summary>
    /// Последний месяц диапазона
    /// </summary>
    public YearMonth LastYM { get { return _LastYM; } }
    private readonly YearMonth _LastYM;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _FirstYM.IsEmpty; } }

    /// <summary>
    /// Число месяцев в диапазоне (не может быть меньше 1, если <see cref="IsEmpty"/>=false)
    /// </summary>
    public int MonthCount
    {
      get
      {
        if (IsEmpty)
          return 0;
        else
          return _LastYM.Value - _FirstYM.Value + 1;
      }
    }

    /// <summary>
    /// Возвращает месяц в диапазоне по индексу
    /// </summary>
    /// <param name="index">Индекс в диапазоне от 0 до <see cref="MonthCount"/>-1</param>
    /// <returns>Месяц и год</returns>
    public YearMonth this[int index]
    {
      get
      {
        if (index < 0 || index >= MonthCount)
          throw new ArgumentOutOfRangeException();
        else
          return FirstYM + index;
      }
    }

    /// <summary>
    /// Возвращает интервал дат.
    /// Если <see cref="IsEmpty"/>=true, возвращается пустой интервал дат.
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        if (IsEmpty)
          return DateRange.Empty;
        else
          return new DateRange(FirstYM.BottomOfMonth, LastYM.EndOfMonth);
      }
    }

    /// <summary>
    /// Возвращает количество лет и месяцев в интервале.
    /// Так как интервал содержит полные месяцы, возвращаемое <see cref="YearMonthDayAge.Days"/> всегда равно 0.
    /// Возвращается положительное значение, если интервал задан и нулевое значение при <see cref="IsEmpty"/>=true.
    /// </summary>
    public YearMonthDayAge Age
    {
      get { return DateRange.Age; }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return FirstYM.ToString() + "-" + LastYM.ToString();
    }

    /// <summary>
    /// Создает массив структур <see cref="YearMonth"/>.
    /// Длина массива совпадает с числом месяцев в диапазоне.
    /// Если <see cref="IsEmpty"/>=true, возвращает пустой массив.
    /// Обычно не следует использовать этот метод. Используйте перечислитель.
    /// </summary>
    /// <returns>Новый массив</returns>
    public YearMonth[] ToArray()
    {
      if (IsEmpty)
        return new YearMonth[0]; // 09.03.2023

      int n = _LastYM - _FirstYM + 1;
      YearMonth[] a = new YearMonth[n];
      for (int i = 0; i < n; i++)
        a[i] = _FirstYM + i;
      return a;
    }

    #endregion

    #region Объединение и пересечение периодов

    /// <summary>
    /// Возвращает интервал, включающие в себя и <paramref name="r1"/> и <paramref name="r2"/> (расширение интервалов).
    /// Операция применима к любым двум интервалам.
    /// Интервалы <paramref name="r1"/> и <paramref name="r2"/> равноправны. 
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static YearMonthRange operator |(YearMonthRange r1, YearMonthRange r2)
    {
      if (r1.IsEmpty)
        return r2; 
      if (r2.IsEmpty)
        return r1; // 09.03.2023

      YearMonth ym1 = r1.FirstYM < r2.FirstYM ? r1.FirstYM : r2.FirstYM;
      YearMonth ym2 = r1.LastYM > r2.LastYM ? r1.LastYM : r2.LastYM;
      return new YearMonthRange(ym1, ym2);
    }

    /// <summary>
    /// Возвращает интервал, в который входит пересечение интервалов <paramref name="r1"/> и <paramref name="r2"/>.
    /// Если интервалы не пересекаются, или один из интервалов пустой, вызывается исключение <see cref="InvalidOperationException"/>.
    /// Интервалы <paramref name="r1"/> и <paramref name="r2"/> равноправны. 
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static YearMonthRange operator &(YearMonthRange r1, YearMonthRange r2)
    {
      if (r1.IsEmpty ||r2.IsEmpty)
        throw new InvalidOperationException("Один из интервалов пустой"); // 09.03.2023

      YearMonth ym1 = r1.FirstYM > r2.FirstYM ? r1.FirstYM : r2.FirstYM;
      YearMonth ym2 = r1.LastYM < r2.LastYM ? r1.LastYM : r2.LastYM;
      if (ym1 > ym2)
        throw new InvalidOperationException("Интервалы " + r1.ToString() + " и " + r2.ToString() +
          " не пересекаются");
      return new YearMonthRange(ym1, ym2);
    }

    /// <summary>
    /// Возвращает true, если два интервала пересекаются (имеют хотя бы один общий месяц)
    /// Такая проверка может понадобиться, например, перед вычислением пересечения
    /// интервалов оператором "and".
    /// Если один или оба интервала пустые, возвращается false.
    /// Интервалы <paramref name="r1"/> и <paramref name="r2"/> равноправны. 
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>True, если интервалы пересекаются</returns>
    public static bool IsCrossed(YearMonthRange r1, YearMonthRange r2)
    {
      if (r1.IsEmpty || r2.IsEmpty)
        return false;

      YearMonth ym1 = r1.FirstYM > r2.FirstYM ? r1.FirstYM : r2.FirstYM;
      YearMonth ym2 = r1.LastYM < r2.LastYM ? r1.LastYM : r2.LastYM;
      return ym1 <= ym2;
    }

    #endregion

    #region Операторы

    /// <summary>
    /// Добавляет к интервалу заданное число месяцев.
    /// Например, если <paramref name="months"/> равно 12, то будет возвращен интервал следующего года.
    /// Если исходный интервал пустой, выбрасывается исключение.
    /// </summary>
    /// <param name="range">Исходный диапазон</param>
    /// <param name="months">Количество добавляемых месяцев. Допускаются любые значения</param>
    /// <returns>Сдвинутый интервал</returns>
    public static YearMonthRange operator +(YearMonthRange range, int months)
    {
      if (range.IsEmpty)
        throw new ArgumentException("Пустой интервал", "range"); // 09.03.2023
      return new YearMonthRange(range.FirstYM + months, range.LastYM + months);
    }

    /// <summary>
    /// Вычитает из интервала заданное число месяцев.
    /// Например, если <paramref name="months"/> равно 12, то будет возвращен интервал предыдущего года.
    /// Если исходный интервал пустой, выбрасывается исключение.
    /// </summary>
    /// <param name="range">Исходный диапазон</param>
    /// <param name="months">Количество вычитаемых месяцев. Допускаются любые значения</param>
    /// <returns>Сдвинутый интервал</returns>
    public static YearMonthRange operator -(YearMonthRange range, int months)
    {
      if (range.IsEmpty)
        throw new ArgumentException("Пустой интервал", "range"); // 09.03.2023
      return new YearMonthRange(range.FirstYM - months, range.LastYM - months);
    }

    // Операторы ++ и -- не определяем

    #endregion

    #region IEnumerable<YearMonth> Members

    /// <summary>
    /// Перечислитель по списку месяцев
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<YearMonth>
    {
      #region Конструктор

      internal Enumerator(YearMonthRange range)
      {
        _Range = range;
        if (range.IsEmpty)
          _Current = new YearMonth(); // 16.04.2020
        else
          _Current = range._FirstYM - 1;
      }

      #endregion

      #region Поля

      private readonly YearMonthRange _Range;
      private YearMonth _Current;

      #endregion

      #region IEnumerator<YearMonth> Members

      /// <summary>
      /// Текущий месяц
      /// </summary>
      public YearMonth Current { get { return _Current; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return _Current; } }

      void System.Collections.IEnumerator.Reset()
      {
        _Current = _Range.FirstYM - 1;
      }

      /// <summary>
      /// Переход к следующему месяцу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        if (_Range.IsEmpty)
          return false; // 16.04.2020
        _Current = _Current + 1;
        return _Current <= _Range.LastYM;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по структурам <see cref="YearMonth"/>.
    /// Перечислитель последовательно возвращает каждый месяц в диапазоне.
    /// Если требуется перечислить дни в диапазоне, то используйте свойство <see cref="DateRange"/> и используйте перечислитель объекта <see cref="FreeLibSet.Calendar.DateRange"/> по объектам <see cref="System.DateTime"/>.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<YearMonth> IEnumerable<YearMonth>.GetEnumerator()
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
