// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Calendar;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.ComponentModel;

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
      bool res;
      try
      {
        res = DoShiftDateRange(ref dt1, ref dt2, forward);
      }
      catch
      {
        res = false;
      }
      if (res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      if (firstDate.HasValue && lastDate.HasValue)
      {
        // Обычный сдвиг
        if (lastDate.Value >= firstDate.Value)
        {
          DateRange dtr = new DateRange(firstDate.Value, lastDate.Value);

          dtr = dtr >> (forward ? +1 : -1);

          firstDate = dtr.FirstDate;
          lastDate = dtr.LastDate;
          return true;
        }
        else
          return false;
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
      bool res;
      try
      {
        res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        res = false;
      }
      if (res)
      {
        firstDate = dt1.Value;
        lastDate = dt2.Value;
      }
      return res;
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
      bool res;
      try
      {
        res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        res = false;
      }
      if (res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      bool res = false;
      bool isWholeMonth = false;
      if (firstDate.HasValue && lastDate.HasValue)
      {
        if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
          isWholeMonth = true;
      }

      if (firstDate.HasValue)
      {
        firstDate = DataTools.CreateDateTime(firstDate.Value.Year + (forward ? +1 : -1), firstDate.Value.Month, firstDate.Value.Day);
        res = true;
      }

      if (lastDate.HasValue)
      {
        lastDate = DataTools.CreateDateTime(lastDate.Value.Year + (forward ? +1 : -1), lastDate.Value.Month,
          isWholeMonth ? 31 : lastDate.Value.Day);
        res = true;
      }
      return res;
    }

    #endregion

    #endregion

    #region UIValidateResult

    /// <summary>
    /// Создает вычисляемое выражение, которое возвращает <see cref="UIValidateResult"/> на основании другого
    /// вычисляемого выражения логического типа и фиксированного текста сообщения об ошибке.
    /// Полученное выражение может быть передано конструктору <see cref="UIValidator"/>.
    /// </summary>
    /// <param name="expressionEx">Вычисляемое выражение, которое должно возвращать true, если проверка успешно пройдена, и false в случае ошибки</param>
    /// <param name="message">Текст выводимого сообщения об ошибке</param>
    /// <returns>Вычисляемое выражение</returns>
    public static DependedValues.DepValue<UIValidateResult> CreateValidateResultEx(DependedValues.DepValue<bool> expressionEx, string message)
    {
      if (expressionEx == null)
        throw new ArgumentNullException("expressionEx");
      return new FreeLibSet.DependedValues.DepExpr2<UIValidateResult, bool, string>(expressionEx, message, CreateValidateResult);
    }

    private static UIValidateResult CreateValidateResult(bool isValid, string message)
    {
      return new UIValidateResult(isValid, message);
    }

    #endregion

    #region Преобразование чисел

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator.
    /// Также убираются пробелы.
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    /// <param name="nfi">Объект, содержщий параметры форматирования</param>
    public static void CorrectNumberString(ref string s, NumberFormatInfo nfi)
    {
      if (String.IsNullOrEmpty(s))
        return;

      if (nfi == null)
        nfi = NumberFormatInfo.CurrentInfo;
      switch (nfi.NumberDecimalSeparator)
      {
        case ",":
          s = s.Replace('.', ',');
          break;
        case ".":
          s = s.Replace(',', '.');
          break;
      }

      s = s.Replace(" ", "");
      s = s.Replace(DataTools.NonBreakSpaceStr, "");
    }

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator.
    /// Также убираются пробелы.
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    public static void CorrectNumberString(ref string s)
    {
      CorrectNumberString(ref s, NumberFormatInfo.CurrentInfo);
    }

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator.
    /// Также убираются пробелы.
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    /// <param name="formatProvider">Форматизатор</param>
    public static void CorrectNumberString(ref string s, IFormatProvider formatProvider)
    {
      if (formatProvider == null)
        CorrectNumberString(ref s);
      else
      {
        NumberFormatInfo nfi = formatProvider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
        CorrectNumberString(ref s, nfi);
      }
    }

    #endregion

    #region Для MaskedTextProdider

    /// <summary>
    /// Возвращает текущее значение, введенное пользователем, из объекта <see cref="MaskedTextProvider "/>.
    /// Если пользователь не заполнил ни одной позиции, то возвращается пустая строка.
    /// Если пользователь заполнил все доступные позиции, возвращается весь текст, включая литеральные символы.
    /// Если заполнена часть символов, то возврашается текст по последний введенный символ включительно, но без литеральных символов справа.
    /// Используется провайдерами управляющих элементов, такими как EFPMaskedTextBox, для извлечения осмысленного текста, который можно
    /// проверяет и передавать в прикладной код
    /// </summary>
    /// <param name="provider">Провайдер, из которого нужно извлечь текст. Не может быть null</param>
    /// <returns>Извлеченный текст</returns>
    public static string GetMaskedText(MaskedTextProvider provider)
    {
#if DEBUG
      if (provider == null)
        throw new ArgumentNullException("provider");
#endif

      // Первоначальная реализация была в методе EFPMaskedTextBox.get_ControlText

      if (provider.AssignedEditPositionCount == 0)
        return String.Empty;

      // Убрано 27.03.2013 (ExtForms, v.36 п.1)
      // Например, если задана маска "00.##" и введено значение "12.  ", то MaskCompleted=true, но Text="12.". Должно возвращаться значение "12".
      // if (Control.MaskCompleted)
      //   return Control.Text;

      // 19.07.2023
      // Однако, если в конце маски есть литеральные символы, например при вводе Guid в формате "B" ("{CCCCCCCC-...-C}"), то последний литерал должен
      // попасть в возвращаемый текст
      if (provider.AssignedEditPositionCount == provider.EditPositionCount) // не то же самое, что и MaskCompleted
        return provider.ToString();

      // Маска заполнена частично
      int p = provider.LastAssignedPosition;
      //if (p >= Control.Text.Length)
      //  return Control.Text;
      return provider.ToString().Substring(0, p + 1);
    }

    /// <summary>
    /// Возвращает true, если в маске есть позиции для ввода буквенных символов, но нет маркеров преобразования регистра (знаков "больше" и "меньше").
    /// Если <paramref name="provider"/>=null или <see cref="MaskedTextProvider.Mask"/> не задана, возвращается true.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static bool IsNormalCharacterCasing(MaskedTextProvider provider)
    {
      if (provider == null)
        return true;
      string mask = provider.Mask;
      if (String.IsNullOrEmpty(mask))
        return true;

      bool hasLetter=false;
      int i = 0;
      while (i < mask.Length)
      {
        switch (mask[i])
        {
          case '\\':
            i++;
            break; // пропускаем экранированный символ
          case '>':
          case '<':
            return false; // можно дальше не искать
          case 'L':
          case '?':
          case '&':
          case 'C':
          case 'A':
          case 'a':
            hasLetter = true;
            break;
        }
        i++;
      }

      return hasLetter; 
    }

    #endregion

    #region Маски для ввода GUID'ов

    /// <summary>
    /// Возвращает маску ввода GUID для MaskedTextBox или <see cref="MaskedTextProvider.Mask"/>
    /// </summary>
    /// <param name="format">Один из форматов "N", "D", "B", "P". Пустая строка означает "D". 
    /// См. описание форматов метода<see cref="Guid.ToString(string)"/>.</param>
    /// <param name="upperCase">Если true, то предполагается ввод символов верхнего регистра, если false - нижнего</param>
    /// <returns>Маска ввода</returns>
    public static string GetGuidEditMask(string format, bool upperCase)
    {
      if (String.IsNullOrEmpty(format))
        format = "D";

      switch (format)
      {
        case "N":
        case "D":
        case "B":
        case "P":
          break;
        default:
          throw new ArgumentException("Неизвестный формат", "format");
      }

      string mask = Guid.Empty.ToString(format); // получили строку из нулей и знаков
      mask = mask.Replace('0', 'A');
      if (upperCase)
        mask = ">" + mask;
      else
        mask = "<" + mask;
      return mask;
    }

    /// <summary>
    /// Возвращает регулярное выражение, которое можно использовать для проверки корректности регулярного выражения для ввода GUID
    /// </summary>
    /// <param name="format">Один из форматов "N", "D", "B", "P". Пустая строка означает "D". 
    /// См. описание форматов метода<see cref="Guid.ToString(string)"/>.</param>
    /// <returns>Шаблон регулярного выражения</returns>
    public static string GetGuidRegEx(string format)
    {
      if (String.IsNullOrEmpty(format))
        format = "D";

      if (format == "N")
        return "^[0-9A-Fa-f]{32}$";

      string s = @"[0-9A-Fa-f]{8}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{12}";
      switch (format)
      {
        case "D":
          break;
        case "B":
          s = @"\{" + s + @"\}";
          break;
        case "P":
          s = @"\(" + s + @"\)";
          break;
      }
      return "^" + s + "$";
    }

    #endregion
  }
}
