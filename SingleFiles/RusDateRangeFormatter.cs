// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

// Класс RusDateRangeFormatter не может быть размещен в библиотеке ExtRussian.dll, т.к. она 
// не ссылается на ExtTools.dll
// Сама библиотека ExtRussian.dll не нужна для использования этого модуля

namespace FreeLibSet.Russian
{
  /// <summary>
  /// Форматировщик диапазонов дат по правилам русского языка.
  /// В частности, реализует объединение диапазонов дат, например, "01-30 июня 2017 г.".
  /// Для ициализации обработчика, выполните в начале работы приложения вызов:
  /// DateRangeFormatter.Default=new RusDateRangeFormatter()
  /// </summary>
  public class RusDateRangeFormatter : DateRangeFormatter
  {
    /// <summary>
    /// Форматировщик для дат
    /// </summary>
    public static readonly IFormatProvider FormatProvider = CultureInfo.GetCultureInfo("ru-RU");

    #region Переопределенные методы

    public override string ToString(DateTime date, bool isLong)
    {
      if (isLong)
        return date.ToString(@"dd MMMM yyyy г.", FormatProvider);
      else
        return date.ToString(@"dd\.MM\.yyyy", FormatProvider);
    }

    public override string ToString(DateTime? firstDate, DateTime? lastDate, bool isLong)
    {
      #region (Полу)открытые интервалы

      if (!(firstDate.HasValue || lastDate.HasValue))
        return "все даты";
      if (!firstDate.HasValue)
        return "по " + ToString(lastDate.Value, isLong);
      if (!lastDate.HasValue)
        return "с " + ToString(firstDate.Value, isLong);

      #endregion

      #region Закрытые интервалы

      if (firstDate.Value.Year != lastDate.Value.Year)
        // Нет объединения
        return ToString(firstDate.Value, isLong) + " - " + ToString(lastDate.Value, isLong);

      if (DataTools.IsBottomOfYear(firstDate.Value) && DataTools.IsEndOfYear(lastDate.Value))
      { 
        // 31.08.2018 - целый год
        if (isLong)
          return lastDate.Value.ToString(@"yyyy г.", FormatProvider);
        else
          return lastDate.Value.ToString(@"yyyy", FormatProvider);
      }

      if (firstDate.Value.Month != lastDate.Value.Month)
      {
        // Объединяется только год
        if (isLong)
          return firstDate.Value.ToString(@"dd MMMM", FormatProvider) + " - " + lastDate.Value.ToString(@"dd MMMM yyyy г.", FormatProvider);
        else
          return firstDate.Value.ToString(@"dd\.MM", FormatProvider) + "-" + lastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
      }

      if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
      { 
        // 31.08.2018 - целый месяц
        if (isLong)
          return lastDate.Value.ToString(@"MMMM yyyy г.", FormatProvider);
        else
          return lastDate.Value.ToString(@"MM\.yyyy", FormatProvider);
      }

      if (firstDate.Value.Day != lastDate.Value.Day)
      { 
        // Объединяется год и месяц
        if (isLong)
          return firstDate.Value.ToString(@"dd", FormatProvider) + " - " + lastDate.Value.ToString(@"dd MMMM yyyy г.", FormatProvider);
        else
        {
          //return FirstDate.Value.ToString(@"dd\.MM", FormatProvider) + "-" + LastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
          // 06.08.2018
          return firstDate.Value.ToString(@"dd", FormatProvider) + "-" + lastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
        }
      }

      // Полное совпадение
      return ToString(firstDate.Value, isLong);

      #endregion
    }

    public override string ToString(YearMonth firstYM, YearMonth lastYM)
    {
      if (!firstYM.IsEmpty)
      {
        if (!lastYM.IsEmpty)
        {
          if (firstYM == lastYM)
            return firstYM.BottomOfMonth.ToString("MMMM yyyy г.");
          else if (firstYM.Month==1 && lastYM.Month==12)
            return lastYM.EndOfMonth.ToString("yyyy г."); // 31.08.2018
          else
            return firstYM.BottomOfMonth.ToString("MMMM") + "-" + lastYM.EndOfMonth.ToString("MMMM yyyy г.");
        }
        else
          return "с " + firstYM.BottomOfMonth.ToString("MMMM yyyy г.");
      }
      else
      {
        if (!lastYM.IsEmpty)
          return "по " + lastYM.EndOfMonth.ToString("MMM yyyy г.");
        else
          return "все даты";
      }
    }

    #endregion
  }
}
