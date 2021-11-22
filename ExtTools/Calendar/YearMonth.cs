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
    /// Создает объект, извлекая год и месяц из объекта DateTime
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
    /// Год
    /// </summary>
    public int Year { get { return _Value / 12; } }

    /// <summary>
    /// Месяц (1-12)
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
    /// Если текущий объект не инициализирован, возвращается DateRange.Empty
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
    /// Сравнение с другим YearMonth
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(YearMonth other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим YearMonth
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
    /// Учитываются полуоткрытые диапазоны, когда граница задана как YearMonth.Empty
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
    /// Прибавляет к объекту YearMonth заданное число месяцев.
    /// </summary>
    /// <param name="value1">Объект YearMonth</param>
    /// <param name="months">Количество добавляемых месяцев. Допускаются отрицательные значения</param>
    /// <returns>Новый объект YearMonth</returns>
    public static YearMonth operator +(YearMonth value1, int months)
    {
#if DEBUG
      value1.CheckNoEmpty();
#endif
      return new YearMonth(value1._Value + months);
    }

    /// <summary>
    /// Вычитает из объекта YearMonth заданное число месяцев.
    /// </summary>
    /// <param name="value2">Объект YearMonth</param>
    /// <param name="months">Количество вычитаемых месяцев. Допускаются отрицательные значения</param>
    /// <returns>Новый объект YearMonth</returns>
    public static YearMonth operator -(YearMonth value2, int months)
    {
#if DEBUG
      value2.CheckNoEmpty();
#endif
      return new YearMonth(value2._Value - months);
    }

    /// <summary>
    /// Вычитает из одного YearMonth другой и возвращает разность между ними как число месяцев
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>Количество месяцев</returns>
    public static int operator -(YearMonth value1, YearMonth value2)
    {
#if DEBUG
      value1.CheckNoEmpty();
      value2.CheckNoEmpty();
#endif
      return value1._Value - value2._Value;
    }

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

    /// <summary>
    /// Проверяет два объекта YearMonth на равенство
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>true, если значения одинаковы</returns>
    public static bool operator ==(YearMonth value1, YearMonth value2)
    {
      return value1._Value == value2._Value;
    }

    /// <summary>
    /// Проверяет два объекта YearMonth на неравенство
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>false, если значения одинаковы</returns>
    public static bool operator !=(YearMonth value1, YearMonth value2)
    {
      return value1._Value != value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта YearMonth 
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>true, первый объект меньше второго</returns>
    public static bool operator <(YearMonth value1, YearMonth value2)
    {
      return value1._Value < value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта YearMonth 
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>true, первый объект больше второго</returns>
    public static bool operator >(YearMonth value1, YearMonth value2)
    {
      return value1._Value > value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта YearMonth 
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
    /// <returns>true, первый объект меньше или равен второму</returns>
    public static bool operator <=(YearMonth value1, YearMonth value2)
    {
      return value1._Value <= value2._Value;
    }

    /// <summary>
    /// Сравнивает два объекта YearMonth 
    /// </summary>
    /// <param name="value1">Первый объект YearMonth</param>
    /// <param name="value2">Второй объект YearMonth</param>
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
  [StructLayout(LayoutKind.Auto)]
  [Serializable]
  public struct YearMonthRange : IEnumerable<YearMonth>
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый диапазон
    /// </summary>
    /// <param name="firstYM">Начало диапазона</param>
    /// <param name="lastYM">Конец диапазона</param>
    public YearMonthRange(YearMonth firstYM, YearMonth lastYM)
    {
#if DEBUG
      if (firstYM.IsEmpty)
        throw new ArgumentException("Не задан начальный месяц/год", "firstYM");
      if (lastYM.IsEmpty)
        throw new ArgumentException("Не задан последний месяц/год", "lastYM");
      if (firstYM > lastYM)
        throw new ArgumentException("Первый месяц больше чем последний", "lastYM");
#endif
      _FirstYM = firstYM;
      _LastYM = lastYM;
    }

    /// <summary>
    /// Создает диапазон, извлекая год и месяц из DateTime.
    /// Значение поля "День" игнорируется
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
    /// Создает диапазон мечяцев в пределах одного года 
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="firstMonth">Первый месяц (1-12)</param>
    /// <param name="lastMonth">Последний месяц (1-12)</param>
    public YearMonthRange(int year, int firstMonth, int lastMonth)
      :this(new YearMonth(year, firstMonth), new YearMonth(year, lastMonth))
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
    /// Первый месяць диапазона
    /// </summary>
    public YearMonth FirstYM { get { return _FirstYM; } }
    private readonly YearMonth _FirstYM;

    /// <summary>
    /// Последний месяц диапазона
    /// </summary>
    public YearMonth LastYM { get { return _LastYM; } }
    private readonly YearMonth _LastYM;

    /// <summary>
    /// Вохвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _FirstYM.IsEmpty; } }

    /// <summary>
    /// Число месяцев в диапазоне (не может быть меньше 1, если IsEmpty=false)
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
    /// <param name="index">Индекс в диапазоне от 0 до MonthCount-1</param>
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
    /// Если IsEmpty=true, возвращается пустой интервал дат
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

    #endregion

    #region Устаревшие свойства

    /// <summary>
    /// Устаревшее свойство
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [Obsolete("Используйте свойство FirstYM", false)]
    public YearMonth FirstMonth { get { return _FirstYM; } }


    /// <summary>
    /// Устаревшее свойство
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [Obsolete("Используйте свойство LastYM", false)]
    public YearMonth LastMonth { get { return _LastYM; } }

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
    /// Создает массив структур YearMonth.
    /// Длина массива совпадает с числом месяцев в диапазоне.
    /// Обычно не следует использовать этот метод. Используйте перечислитель.
    /// </summary>
    /// <returns>Новый массив</returns>
    public YearMonth[] ToArray()
    {
      int n = _LastYM - _FirstYM + 1;
      YearMonth[] a = new YearMonth[n];
      for (int i = 0; i < n; i++)
        a[i] = _FirstYM + i;
      return a;
    }

    #endregion

    #region Объединение и пересечение периодов

    /// <summary>
    /// Возвращает интервал, включающие в себя и r1 и r2 (расширение интервалов)
    /// Операция применима к любым двум интервалам
    /// Интервалы r1 и r2 равноправны
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static YearMonthRange operator |(YearMonthRange r1, YearMonthRange r2)
    {
      YearMonth ym1 = r1.FirstYM < r2.FirstYM ? r1.FirstYM : r2.FirstYM;
      YearMonth ym2 = r1.LastYM > r2.LastYM ? r1.LastYM : r2.LastYM;
      return new YearMonthRange(ym1, ym2);
    }

    /// <summary>
    /// Возвращает интервал, в который входит пересечение интервалов  r1 и r2
    /// Если интервалы не пересекаются, вызывается исключение InvalidOperationException
    /// Интервалы r1 и r2 равноправны
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>Результирующий интервал</returns>
    public static YearMonthRange operator &(YearMonthRange r1, YearMonthRange r2)
    {
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
    /// интервалов оператором "and"
    /// Интервалы r1 и r2 равноправны. 
    /// </summary>
    /// <param name="r1">Первый интервал</param>
    /// <param name="r2">Второй интервал</param>
    /// <returns>True, если интервалы пересекаются</returns>
    public static bool IsCrossed(YearMonthRange r1, YearMonthRange r2)
    {
      YearMonth ym1 = r1.FirstYM > r2.FirstYM ? r1.FirstYM : r2.FirstYM;
      YearMonth ym2 = r1.LastYM < r2.LastYM ? r1.LastYM : r2.LastYM;
      return ym1 <= ym2;
    }

    #endregion

    #region Операторы

    /// <summary>
    /// Добавляет к интервалу заданное число месяцев.
    /// Например, если <paramref name="months"/> равно 12, то будет возвращен интервал следующего года.
    /// Если исходный интервал пустой, возвращается пустой интервал.
    /// </summary>
    /// <param name="range">Исходный диапазон</param>
    /// <param name="months">Количество добавляемых месяцев. Допускаются любые значения</param>
    /// <returns>Сдвинутый интервал</returns>
    public static YearMonthRange operator +(YearMonthRange range, int months)
    { 
      if (range.IsEmpty)
        return range;
      return new YearMonthRange(range.FirstYM + months, range.LastYM + months);
    }

    /// <summary>
    /// Вычитает из интервала заданное число месяцев.
    /// Например, если <paramref name="months"/> равно 12, то будет возвращен интервал предыдущего года
    /// Если исходный интервал пустой, возвращается пустой интервал.
    /// </summary>
    /// <param name="range">Исходный диапазон</param>
    /// <param name="months">Количество вычитаемых месяцев. Допускаются любые значения</param>
    /// <returns>Сдвинутый интервал</returns>
    public static YearMonthRange operator -(YearMonthRange range, int months)
    {
      if (range.IsEmpty)
        return range;
      return new YearMonthRange(range.FirstYM - months, range.LastYM - months);
    }

    // Операторы ++ и -- не определям

    #endregion

    #region IEnumerable<YearMonth> Members

    /// <summary>
    /// Перечислитель по списку месяцев
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
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
      YearMonth _Current;

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
        _Current++;
        return _Current <= _Range.LastYM;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по структурам YearMonth.
    /// Перечислитель последовательно возвращает каждый месяц в диапазоне.
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
