using FreeLibSet.Calendar;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FreeLibSet.UICore
{
  /// <summary>
  /// Статические методы для реализации пользовательского интерфейса
  /// </summary>
  public static class UITools
  {
    #region Text <-> Lines

    /// <summary>
    /// Преобразование свойства Text в Lines для многострочного текста
    /// </summary>
    /// <param name="text">Строка</param>
    /// <returns>Массив строк</returns>
    public static string[] TextToLines(string text)
    {
      if (String.IsNullOrEmpty(text))
        return DataTools.EmptyStrings;
      else
        return text.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
    }

    /// <summary>
    /// Преобразование свойства Lines в Text для многострочного текста
    /// </summary>
    /// <param name="lines">Массив строк</param>
    /// <returns>Строка</returns>
    public static string LinesToText(string[] lines)
    {
      if (lines == null)
        return String.Empty;
      if (lines.Length == 0)
        return String.Empty;
      else
        return String.Join(Environment.NewLine, lines);
    }

    #endregion

    #region Сдвиг интервала дат

    #region ShiftDateRange

    /// <summary>
    /// Сдвиг интервала дат вперед или назад.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на число месяцев, содержащихся в периоде. Иначе сдвиг выполняется на число дней в периоде.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// 
    /// Эта версия позволяет сдвигать полузакрытые интервалы. Если задана только начальная дата, то можно
    /// выполнить "сдвиг назад", с <paramref name="forward"/>=false. Если задана только конечная дата,
    /// то можно выполнить "сдвиг вперед", с <paramref name="forward"/>=true. 
    /// 
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// См. также операторы сдвига вправо/влева для типа DateRange.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true - для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRange(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      if (firstDate.HasValue && lastDate.HasValue)
      {
        // Обычный сдвиг
        DateRange dtr = new DateRange(firstDate.Value, lastDate.Value);

        dtr = dtr >> (forward ? +1 : -1);

        firstDate = dtr.FirstDate;
        lastDate = dtr.LastDate;
        return true;
      }

      if (firstDate.HasValue)
      {
        if (!forward)
        {
          if (firstDate.Value == DateTime.MinValue)
            return false; // 02.09.2020
          lastDate = firstDate.Value.AddDays(-1);
          firstDate = null;
          return true;
        }
      }
      if (lastDate.HasValue)
      {
        if (forward)
        {
          if (lastDate.Value == DateTime.MaxValue.Date)
            return false; // 02.09.2020
          firstDate = lastDate.Value.AddDays(1);
          lastDate = null;
          return true;
        }
      }
      return false;
    }

    #endregion

    #region ShiftDateRangeYear

    /// <summary>
    /// Сдвиг интервала дат вперед или назад на один год.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на 12 месяцев. Иначе сдвиг выполняется по дням. Эта тонкость имеет значение только для високосных
    /// годов, если период заканчивается в феврале.
    /// Например, период {01.10.2018-28.02.2019} сдвигается вперед до {01.10.2019-29.02.2020},
    /// а {02.10.2018-28.02.2019} - до {02.10.2019-28.02.2020}.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime firstDate, ref DateTime lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1.Value;
        lastDate = dt2.Value;
      }
      return Res;
    }

    /// <summary>
    /// Сдвиг интервала дат вперед или назад на один год.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на 12 месяцев. Иначе сдвиг выполняется по дням. Эта тонкость имеет значение только для високосных
    /// годов, если период заканчивается в феврале.
    /// Например, период {01.10.2018-28.02.2019} сдвигается вперед до {01.10.2019-29.02.2020},
    /// а {02.10.2018-28.02.2019} - до {02.10.2019-28.02.2020}.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// Эта версия позволяет сдвигать полузакрытые интервалы.
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      bool Res = false;
      bool IsWholeMonth = false;
      if (firstDate.HasValue && lastDate.HasValue)
      {
        if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
          IsWholeMonth = true;
      }

      if (firstDate.HasValue)
      {
        firstDate = DataTools.CreateDateTime(firstDate.Value.Year + (forward ? +1 : -1), firstDate.Value.Month, firstDate.Value.Day);
        Res = true;
      }

      if (lastDate.HasValue)
      {
        lastDate = DataTools.CreateDateTime(lastDate.Value.Year + (forward ? +1 : -1), lastDate.Value.Month,
          IsWholeMonth ? 31 : lastDate.Value.Day);
        Res = true;
      }
      return Res;
    }

    #endregion

    #endregion

    #region Получение инкрементных значений


    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=5,
    /// то будут возвращаться значения 0, 5, 10, 15, ... Но, для значения 1 возвращается 5, а не 2.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static int GetIncrementedValue(int currValue, int increment)
    {
      return currValue + increment;
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static float GetIncrementedValue(float currValue, float increment)
    {
      // TODO: Не реализовано

      return currValue + increment;
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static double GetIncrementedValue(double currValue, double increment)
    {
      // TODO: Не реализовано

      return currValue + increment;

      /*
      if (increment == 0.0)
        return currValue;

      double incMod = Math.Abs(increment);
      if (incMod >= 1.0)
      {
        if (Math.Truncate(incMod) != incMod)
          return currValue + increment;

        if (increment>0)
          currValue=
      }
      else
      { 
      }
       * */
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static decimal GetIncrementedValue(decimal currValue, decimal increment)
    {
      // TODO: Не реализовано

      return currValue + increment;
    }

    #endregion
  }
}
