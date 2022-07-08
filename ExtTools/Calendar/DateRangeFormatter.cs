// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Formatting;

namespace FreeLibSet.Calendar
{

  /// <summary>
  /// Форматизатор интервала дат.
  /// Поддерживает полуоткрытые и полностью открытые интервалы.
  /// Класс является потокобезопасным.
  /// Производные классы также должны быть потокобезопасными.
  /// </summary>
  public class DateRangeFormatter
  {
    #region Виртуальные методы

    /// <summary>
    /// Получение текста даты.
    /// Непереопределенная версия метода в DateRangeFormatter использует DateTime.ToShortDateString() и
    /// DateTime.ToLongDateString()
    /// </summary>
    /// <param name="date">Преобразуемая дата</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public virtual string ToString(DateTime date, bool longFormat)
    {
      if (longFormat)
        return date.ToLongDateString();
      else
        return date.ToShortDateString();
    }

    /// <summary>
    /// Получение текста даты.
    /// Если дата не задана, возвращает пустую строку.
    /// Для получения даты, когда она задана, используется перегрузка метода для даты не-nullable.
    /// </summary>
    /// <param name="date">Преобразуемая дата или null</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(DateTime? date, bool longFormat)
    {
      if (date.HasValue)
        return ToString(date.Value, longFormat);
      else
        return String.Empty;
    }

    /// <summary>
    /// Получение текста для диапазона дат.
    /// Диапазон может быть закрытым, полуоткрытым или полностью открытым
    /// Версия метода в DateRangeFormatter использует для вывода дат вызов ToString() для даты.
    /// Переопределенный метод может реализовать более интеллектуальное представление, например,
    /// объединяя даты в пределах месяца: "01-31 июля 2017 года"
    /// </summary>
    /// <param name="firstDate">Начальная дата диапазона или null для (полу)открытого интервала</param>
    /// <param name="lastDate">Конечная дата диапазона или null для (полу)открытого интервала</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public virtual string ToString(DateTime? firstDate, DateTime? lastDate, bool longFormat)
    {
      if (firstDate.HasValue)
      {
        if (lastDate.HasValue)
        {
          if (firstDate.Value == lastDate.Value)
            return ToString(firstDate.Value, longFormat);
          else
            return ToString(firstDate.Value, longFormat) + "-" + ToString(lastDate.Value, longFormat);
        }
        else
          return ">=" + ToString(firstDate.Value, longFormat);
      }
      else
      {
        if (lastDate.HasValue)
          return "<=" + ToString(lastDate.Value, longFormat);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Получение текста для диапазона месяцев.
    /// Диапазон может быть закрытым, полуоткрытым или полностью открытым (используется YearMonth.Empty).
    /// Версия метода в DateRangeFormatter использует для вывода дат вызов DateTime.ToString("y") для даты.
    /// </summary>
    /// <param name="firstYM">Начальный месяц диапазона</param>
    /// <param name="lastYM">Конечный месяц диапазона</param>
    /// <returns>Текстовое представление</returns>
    public virtual string ToString(YearMonth firstYM, YearMonth lastYM)
    {
      if (!firstYM.IsEmpty)
      {
        if (!lastYM.IsEmpty)
        {
          if (firstYM == lastYM)
            return firstYM.BottomOfMonth.ToString("y");
          else
            return firstYM.BottomOfMonth.ToString("y") + "-" + lastYM.EndOfMonth.ToString("y");
        }
        else
          return ">=" + firstYM.BottomOfMonth.ToString("y");
      }
      else
      {
        if (!lastYM.IsEmpty)
          return "<=" + lastYM.EndOfMonth.ToString("y");
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Возвращает true, если идет сначала месяц, потом день.
    /// ВОзвращает false, если сначала должен быть день, потом месяц
    /// </summary>
    private static bool IsMDOrder
    {
      get
      {
        //return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Contains("M/d");
        switch (FormatStringTools.DateFormatOrder)
        {
          case DateFormatYMDOrder.YMD:
          case DateFormatYMDOrder.MDY:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Возвращает текстовое представление для значения "Месяц и день"
    /// </summary>
    /// <param name="day">День для вывода</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public virtual string ToString(MonthDay day, bool longFormat)
    {
      if (day.IsEmpty)
        return String.Empty;

      if (longFormat)
        return day.ToString(IsMDOrder ? "MMMM dd" : "dd MMMM");
      else
        return day.ToString(IsMDOrder ? "MM/dd" : "dd/MM");
    }

    /// <summary>
    /// Возвращает текстовое представление для интервала дней
    /// </summary>
    /// <param name="range">Интервал</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public virtual string ToString(MonthDayRange range, bool longFormat)
    {
      if (range.IsEmpty)
        return String.Empty;

      if (range.First <= range.Last && range.First.Month == range.Last.Month)
      {
        // Можно объединить
        if (IsMDOrder)
          return ToString(range.First, longFormat) + "-" + range.Last.ToString("dd");
        else
          return range.First.ToString("dd") + "-" + ToString(range.Last, longFormat);
      }
      else
        return ToString(range.First, longFormat) + "-" + ToString(range.Last, longFormat);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получение текста для диапазона дат.
    /// </summary>
    /// <param name="range">Диапазон дат</param>
    /// <param name="longFormat">Нужно ли использовать длинный формат данных (true) или краткий (false)</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(DateRange range, bool longFormat)
    {
      if (range.IsEmpty)
        return ToString((DateTime?)null, (DateTime?)null, longFormat);
      else
        return ToString(range.FirstDate, range.LastDate, longFormat);
    }

    #endregion

    #region Размеры полей

    // Упрощенная поддержка многопготочности.
    // Обращение к свойству XxxTextLength может произойти асинхронно.
    // Максимум, что произойдет, вызов метода CalcXxxTextLeng() произойдет дважды.
    // Присвовение значения полю Int32 можно считать атомарной операцией

    #region DateRange

    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта DateRange при Long=true
    /// </summary>
    public virtual int DateRangeLongTextLength
    {
      get
      {
        if (_DateRangeLongTextLength == 0)
          _DateRangeLongTextLength = CalcDateRangeLongTextLength();
        return _DateRangeLongTextLength;
      }
    }

    private int _DateRangeLongTextLength;

    private int CalcDateRangeLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        DateTime dt1 = new DateTime(2001, 1, 31);
        DateTime dt2 = new DateTime(2002, month, 28); // в другом году
        DateRange r = new DateRange(dt1, dt2);
        w = Math.Max(w, this.ToString(r, true).Length);
      }
      return w;
    }


    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта DateRange при Long=false
    /// Обычно это 11 символов
    /// </summary>
    public virtual int DateRangeShortTextLength
    {
      get
      {
        if (_DateRangeShortTextLength == 0)
          _DateRangeShortTextLength = CalcDateRangeShortTextLength();
        return _DateRangeShortTextLength;
      }
    }

    private int _DateRangeShortTextLength;

    private int CalcDateRangeShortTextLength()
    {
      DateTime dt1 = new DateTime(2001, 1, 31);
      DateTime dt2 = new DateTime(2002, 12, 31); // в другом году 
      DateRange r = new DateRange(dt1, dt2);
      return this.ToString(r, false).Length;
    }

    #endregion

    #region MonthDay

    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта MonthDay при Long=true
    /// </summary>
    public virtual int MonthDayLongTextLength
    {
      get
      {
        if (_MonthDayLongTextLength == 0)
          _MonthDayLongTextLength = CalcMonthDayLongTextLength();
        return _MonthDayLongTextLength;
      }
    }

    private int _MonthDayLongTextLength;

    private int CalcMonthDayLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        MonthDay md = new MonthDay(month, 28);
        w = Math.Max(w, this.ToString(md, true).Length);
      }
      return w;
    }


    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта MonthDay при Long=false
    /// Обычно это 5 символов ("ДД.ММ")
    /// </summary>
    public virtual int MonthDayShortTextLength
    {
      get
      {
        if (_MonthDayShortTextLength == 0)
          _MonthDayShortTextLength = CalcMonthDayShortTextLength();
        return _MonthDayShortTextLength;
      }
    }

    private int _MonthDayShortTextLength;

    private int CalcMonthDayShortTextLength()
    {
      return this.ToString(MonthDay.EndOfYear, false).Length;
    }

    #endregion

    #region MonthDayRange

    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта MonthDayRange при Long=true
    /// </summary>
    public virtual int MonthDayRangeLongTextLength
    {
      get
      {
        if (_MonthDayRangeLongTextLength == 0)
          _MonthDayRangeLongTextLength = CalcMonthDayRangeLongTextLength();
        return _MonthDayRangeLongTextLength;
      }
    }

    private int _MonthDayRangeLongTextLength;

    private int CalcMonthDayRangeLongTextLength()
    {
      int w = 1;
      for (int month = 1; month <= 12; month++)
      {
        MonthDay md1 = new MonthDay(month, 28);
        MonthDayRange r = new MonthDayRange(md1, MonthDay.EndOfYear);
        w = Math.Max(w, this.ToString(r, true).Length);
      }
      return w;
    }


    /// <summary>
    /// Возвращает максимальное количество символов, которое возвращает методом ToString() для объекта MonthDayRange при Long=false
    /// Обычно это 11 символов ("ДД.ММ-ДД.ММ")
    /// </summary>
    public virtual int MonthDayRangeShortTextLength
    {
      get
      {
        if (_MonthDayRangeShortTextLength == 0)
          _MonthDayRangeShortTextLength = CalcMonthDayRangeShortTextLength();
        return _MonthDayRangeShortTextLength;
      }
    }

    private int _MonthDayRangeShortTextLength;

    private int CalcMonthDayRangeShortTextLength()
    {
      return this.ToString(MonthDayRange.WholeYear, false).Length;
    }

    #endregion

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Форматировщик, используемый по умолчанию
    /// </summary>
    public static DateRangeFormatter Default
    {
      get { return _Default; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Default = value;
      }
    }
    private static DateRangeFormatter _Default = new DateRangeFormatter();

    #endregion
  }
}
