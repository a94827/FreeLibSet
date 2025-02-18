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
using FreeLibSet.Formatting;

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

    #region Реализация проверок для управляющих элементов


    /// <summary>
    /// Метод для выдачи ошибки или предупреждения, в зависимости от свойства 'CanBeEmptyMode'.
    /// На момент вызова должно быть проверено значение, что оно является пустым.
    /// </summary>
    /// <param name="canBeEmptyMode">Значение свойства 'CanBeEmptyMode'.
    /// Если равно <see cref="UIValidateState.Ok"/>, никаких действий не выполняется</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    public static void ValidateCanBeEmptyMode(UIValidateState canBeEmptyMode, IUIValidableObject obj, string displayName)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      switch (canBeEmptyMode)
      {
        case UIValidateState.Error:
          obj.SetError(String.Format(Res.UITools_Err_CanBeEmptyModeError, displayName));
          break;
        case UIValidateState.Warning:
          obj.SetWarning(String.Format(Res.UITools_Err_CanBeEmptyModeWarning, displayName));
          break;
      }
    }

    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения в диапазон.
    /// Диапазон может быть закрытым, открытым или полуоткрытым.
    /// </summary>
    /// <typeparam name="T">Тип значения. Должен быть структурой, допускающей сравнение значений (число, <see cref="DateTime"/>, <see cref="TimeSpan"/>, ...)</typeparam>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Начало диапазона или null</param>
    /// <param name="maximum">Конец диапазона или null</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    /// <param name="format">Формат. Используется, если <typeparamref name="T"/> реализует интерфейс <see cref="IFormattable"/>.</param>
    /// <param name="formatProvider">Провайдер форматирования. Используется, если <typeparamref name="T"/> реализует интерфейс <see cref="IFormattable"/>.</param>
    public static void ValidateInRange<T>(T value, T? minimum, T? maximum, IUIValidableObject obj, string displayName, bool isError, string format, IFormatProvider formatProvider)
      where T:struct, IComparable<T>
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      string message = null;
      if (minimum.HasValue)
      {
        if (maximum.HasValue)
        {
          if (value.CompareTo(minimum.Value) < 0 || value.CompareTo(maximum.Value) > 0)
            message = String.Format(Res.UITools_Err_OutOfRangeMinMax, displayName,
              ToString(minimum, format, formatProvider), ToString(maximum, format, formatProvider));
        }
        else
        {
          if (value.CompareTo(minimum.Value) < 0)
            message = String.Format(Res.UITools_Err_OutOfRangeMin, displayName, ToString(minimum, format, formatProvider));
        }
      }
      else
      {
        if (maximum.HasValue)
        {
          if (value.CompareTo(maximum.Value) > 0)
            message = String.Format(Res.UITools_Err_OutOfRangeMax, displayName, ToString(maximum, format, formatProvider));
        }
      }

      if (message != null)
      {
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
    }

    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения <see cref="DateTime"/> в диапазон.
    /// Диапазон может быть закрытым, открытым или полуоткрытым.
    /// Для показа диапазона в сообщении используется <see cref="DateRangeFormatter"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Начало диапазона или null</param>
    /// <param name="maximum">Конец диапазона или null</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    public static void ValidateInRange(DateTime value, DateTime? minimum, DateTime? maximum, IUIValidableObject obj, string displayName, bool isError)
    {
      if (!DataTools.DateInRange(value, minimum, maximum))
      {
        string message = String.Format(Res.UITools_Err_OutOfRange, displayName, DateRangeFormatter.Default.ToString(minimum, maximum, true));
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
    }

    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения <see cref="DateTime"/> в диапазон.
    /// Диапазон может быть закрытым, открытым или полуоткрытым.
    /// Для показа диапазона в сообщении используется <see cref="DateRangeFormatter"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="range">Диапазон дат</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    public static void ValidateInRange(DateTime value, DateRange range, IUIValidableObject obj, string displayName, bool isError)
    {
      if (!range.Contains(value))
      {
        string message = String.Format(Res.UITools_Err_OutOfRange, displayName, DateRangeFormatter.Default.ToString(range, true));
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
    }


    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения <see cref="YearMonth"/> в диапазон.
    /// Диапазон может быть закрытым, открытым или полуоткрытым.
    /// Для показа диапазона в сообщении используется <see cref="DateRangeFormatter"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="minimum">Начало диапазона или null</param>
    /// <param name="maximum">Конец диапазона или null</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    public static void ValidateInRange(YearMonth value, YearMonth minimum, YearMonth maximum, IUIValidableObject obj, string displayName, bool isError)
    {
      if (!value.IsInRange(minimum, maximum))
      {
        string message = String.Format(Res.UITools_Err_OutOfRange, displayName, DateRangeFormatter.Default.ToString(minimum, maximum));
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
    }


    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения <see cref="YearMonth"/> в диапазон.
    /// Диапазон может быть закрытым, открытым или полуоткрытым.
    /// Для показа диапазона в сообщении используется <see cref="DateRangeFormatter"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="range">Диапазон</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    public static void ValidateInRange(YearMonth value, YearMonthRange range, IUIValidableObject obj, string displayName, bool isError)
    {
      if (!range.Contains(value))
      {
        string message = String.Format(Res.UITools_Err_OutOfRange, displayName, DateRangeFormatter.Default.ToString(range.FirstYM, range.LastYM));
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
    }

    /// <summary>
    /// Метод для выдачи ошибки или предупреждения при проверке на попадание значения <see cref="MonthDay"/> в диапазон.
    /// Диапазон может быть закрытым или открытым.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="range">Диапазон значений</param>
    /// <param name="obj">Объект, для которого можно установить состояние ошибки или предупреждения</param>
    /// <param name="displayName">Описание значения, которое должно быть заполнено</param>
    /// <param name="isError">True - устанавливать ошибку, false - предупреждение</param>
    public static void ValidateInRange(MonthDay value, MonthDayRange range, IUIValidableObject obj, string displayName, bool isError)
    {
      if (!range.Contains(value))
      {
        string message = String.Format(Res.UITools_Err_OutOfRange, displayName, DateRangeFormatter.Default.ToString(range, true));
        if (isError)
          obj.SetError(message);
        else
          obj.SetWarning(message);
      }
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

      bool hasLetter = false;
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
          throw ExceptionFactory.ArgUnknownValue("format", format);
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

    #region Текстовое представление

    /// <summary>
    /// Вспомогательный метод для текстового представления объекта.
    /// Вызывает метод <see cref="Object.ToString()"/> или <see cref="IFormattable.ToString(string, IFormatProvider)"/>,
    /// в зависимости от того, поддерживает ли <paramref name="value"/> интерфейс <see cref="IFormattable"/>.
    /// Если <paramref name="value"/>=null, возвращается строка "null".
    /// </summary>
    /// <param name="value">Значение, которое нужно преобразовать</param>
    /// <param name="format">Формат. Используется, если <paramref name="value"/> реализует интерфейс <see cref="IFormattable"/>.</param>
    /// <param name="formatProvider">Провайдер форматирования. Используется, если <paramref name="value"/>  реализует интерфейс <see cref="IFormattable"/>.</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(object value, string format, IFormatProvider formatProvider)
    {
      if (value == null)
        return "null";
      IFormattable value2 = value as IFormattable;
      if (value2 == null)
        return value.ToString();
      else
        return value2.ToString(format, formatProvider);
    }

    /// <summary>
    /// Получение человеко-читаемого представления для заданного типа данных.
    /// Например, для <see cref="System.Int32"/> возвращается "целое число".
    /// Для <paramref name="typ"/>=null возвращает "пусто".
    /// Для неизвестных типов данных возвращает имя типа без преобразования.
    /// </summary>
    /// <param name="typ">Тип данных</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(Type typ)
    {
      if (typ == null || typ == typeof(DBNull))
        return Res.UITools_Msg_Null;
      if (DataTools.IsIntegerType(typ))
        return Res.UITools_Msg_Int;
      if (DataTools.IsFloatType(typ))
        return Res.UITools_Msg_Float;
      if (typ == typeof(DateTime))
        return Res.UITools_Msg_DateTime;
      if (typ == typeof(string))
        return Res.UITools_Msg_String;
      if (typ == typeof(Boolean))
        return Res.UITools_Msg_Boolean;

      return typ.ToString();
    }

    /// <summary>
    /// Получение человеко-читаемого представления для типа даты и/или времени.
    /// Возвращает строку "дата", "время" или "дата и время".
    /// </summary>
    /// <param name="kind">Тип даты/времени</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(EditableDateTimeFormatterKind kind)
    {
      switch (kind)
      {
        case EditableDateTimeFormatterKind.Date: return Res.UITools_Msg_Date;
        case EditableDateTimeFormatterKind.Time:
        case EditableDateTimeFormatterKind.ShortTime: return Res.UITools_Msg_Time;
        case EditableDateTimeFormatterKind.DateTime:
        case EditableDateTimeFormatterKind.ShortDateTime: return Res.UITools_Msg_DateTime;
        default:
          throw ExceptionFactory.ArgUnknownValue("kind", kind);
      }
    }

    /// <summary>
    /// Получение текста сообщения "Текст 'XXX' нельзя преобразовать в значение типа 'YYY'".
    /// </summary>
    /// <param name="text">Текст, введенный пользователем</param>
    /// <param name="typ">Тип, в который выполнялось преобразование</param>
    /// <returns>Текстовое представление</returns>
    public static string ConvertErrorMessage(string text, Type typ)
    {
      return String.Format(Res.UITools_Msg_ConvertErrorMessage, text, ToString(typ));
    }


    /// <summary>
    /// Получение текста сообщения "Текст 'XXX' нельзя преобразовать в значение типа 'YYY'" для даты/времени.
    /// </summary>
    /// <param name="text">Текст, введенный пользователем</param>
    /// <param name="kind">Тип даты/времени</param>
    /// <returns>Текстовое представление</returns>
    public static string ConvertErrorMessage(string text, EditableDateTimeFormatterKind kind)
    {
      return String.Format(Res.UITools_Msg_ConvertErrorMessage, text, ToString(kind));
    }

    /// <summary>
    /// Возвращает "да" или "нет"
    /// </summary>
    /// <param name="value">значение</param>
    /// <returns>Текстовое представление</returns>
    public static string ToYesNo(bool value)
    {
      return value ? Res.UITools_Msg_Yes : Res.UITools_Msg_No;
    }

    /// <summary>
    /// Читаемое текстовое представление для перечисления <see cref="MaskedTextResultHint"/>
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(MaskedTextResultHint value)
    {
      switch (value)
      {
        case MaskedTextResultHint.DigitExpected:
          return Res.MaskedTextResultHint_Msg_DigitExpected;
        case MaskedTextResultHint.AlphanumericCharacterExpected:
          return Res.MaskedTextResultHint_Msg_AlphanumericCharacterExpected;
        case MaskedTextResultHint.LetterExpected:
          return Res.MaskedTextResultHint_Msg_LetterExpected;
        case MaskedTextResultHint.UnavailableEditPosition:
          return Res.MaskedTextResultHint_Msg_UnavailableEditPosition;
        case MaskedTextResultHint.CharacterEscaped:
          return Res.MaskedTextResultHint_Msg_CharacterEscaped;
        default:
          return value.ToString();
      }
    }

    /// <summary>
    /// Читаемое текстовое представление для перечисления <see cref="ListSortDirection"/>
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(ListSortDirection value)
    {
      return ToString(value, true);
    }

    /// <summary>
    /// Читаемое текстовое представление для перечисления <see cref="ListSortDirection"/>
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <param name="isLong">True - длинная форма. False - короткая форма</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(ListSortDirection value, bool isLong)
    {
      switch (value)
      {
        case ListSortDirection.Ascending:
          return isLong ? Res.ListSortDirection_Name_Ascending : Res.ListSortDirection_Name_AscendingShort;
        case ListSortDirection.Descending:
          return isLong ? Res.ListSortDirection_Name_Descending : Res.ListSortDirection_Name_DescendingShort;
        default:
          throw ExceptionFactory.ArgUnknownValue("value", value);
      }
    }

    /// <summary>
    /// Читаемое текстовое представление для перечисления <see cref="ErrorMessageKind"/>
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(ErrorMessageKind value)
    {
      return ToString(value, true);
    }

    /// <summary>
    /// Читаемое текстовое представление для перечисления <see cref="ErrorMessageKind"/>
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <param name="isLong">True - длинная форма. False - короткая форма</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(ErrorMessageKind value, bool isLong)
    {
      switch (value)
      {
        case ErrorMessageKind.Error:
          return isLong ? Res.ErrorMessageKind_Msg_Error : Res.ErrorMessageKind_Msg_ErrorShort;
        case ErrorMessageKind.Warning:
          return isLong ? Res.ErrorMessageKind_Msg_Warning : Res.ErrorMessageKind_Msg_WarningShort;
        case ErrorMessageKind.Info:
          return isLong ? Res.ErrorMessageKind_Msg_Info : Res.ErrorMessageKind_Msg_InfoShort;
        default:
          throw ExceptionFactory.ArgUnknownValue("value", value);
      }
    }

    #endregion
  }

  #region Перечисления

  #region UIDataState

  /// <summary>
  /// Режим редактирования данных в табличном просмотре
  /// </summary>
  public enum UIDataState
  {
    /// <summary>
    /// Режим просмотра.
    /// В этом режиме табличный просмотр находится постоянно, пока не выбрана одна из команд редактирования.
    /// Также используется, если выбрана команда "Просмотреть запись".
    /// </summary>
    View,

    /// <summary>
    /// Выбрана команда "Редактировать запись".
    /// </summary>
    Edit,

    /// <summary>
    /// Выбрана команда "Новая запись"
    /// </summary>
    Insert,

    /// <summary>
    /// Выбрана команда "Копия записи"
    /// </summary>
    InsertCopy,

    /// <summary>
    /// Выбрана команда "Удалить запись"
    /// </summary>
    Delete,
  };

  #endregion

  #region UISelectedRowsState

  /// <summary>
  /// Сколько строк сейчас выбрано в просмотре: Одна, несколько или ни одной
  /// </summary>
  public enum UISelectedRowsState
  {
    /// <summary>
    /// Выбрана одна ячейка или одна строка целиком или несколько ячеек в одной строке
    /// </summary>
    SingleRow,

    /// <summary>
    /// Выбрано несколько строк или ячейки, расположенные на разных строках
    /// </summary>
    MultiRows,

    /// <summary>
    /// Нет ни одной выбранной ячейки (просмотр пуст)
    /// </summary>
    NoSelection
  }

  #endregion

  #region UIDataViewColorType

  /// <summary>
  /// Тип стандартизированного цветового оформления ячеек табличного просмотра
  /// </summary>
  public enum UIDataViewColorType
  {
    /// <summary>
    /// Обычная ячейка. Для невыделенной ячейки используется цвет фона и цвет текста по умолчанию.
    /// </summary>
    Normal,

    /// <summary>
    /// Альтернативный цвет для создания "полосатых" просмотров, когда строки
    /// (обычно, через одну) имеют немного отличающийся цвет
    /// </summary>
    Alter,

    /// <summary>
    /// Выделение бледно-желтым цветом
    /// </summary>
    Special,

    /// <summary>
    /// Итог 1 (зеленые строки)
    /// </summary>
    Total1,

    /// <summary>
    /// Подытог 2 (сиреневые строки)
    /// </summary>
    Total2,

    /// <summary>
    /// Итоговая строка по всей таблице
    /// </summary>
    TotalRow,

    /// <summary>
    /// Заголовок
    /// </summary>
    Header,

    /// <summary>
    /// Ячейка с ошибкой (красный фон).
    /// </summary>
    Error,

    /// <summary>
    /// Ячейка с предупреждением (ярко-желтый фон, красные буквы)
    /// </summary>
    Warning,
  }

  #endregion

  #region UIDataViewBorderStyle

  /// <summary>
  /// Типы границы ячейки при экспорте табличного просмотра в различные печатные форматы
  /// </summary>
  public enum UIDataViewBorderStyle
  {
    /// <summary>
    /// Граница по умолчанию (в зависимости от оформления границ таблицы)
    /// </summary>
    Default,

    /// <summary>
    /// Отключить границу
    /// (требуется, чтобы граница была отключена и у соседней ячейки)
    /// </summary>
    None,

    /// <summary>
    /// Тонкая линия
    /// </summary>
    Thin,

    /// <summary>
    /// Толстая линия
    /// </summary>
    Thick
  }

  #endregion

  #region UIDataViewImageKind

  /// <summary>
  /// Изображения, которые можно выводить в заголовке строк.
  /// </summary>
  public enum UIDataViewImageKind
  {
    /// <summary>
    /// Нет изображения или используется пользовательское изображение
    /// </summary>
    None,

    /// <summary>
    /// Значок "i"
    /// </summary>
    Information,

    /// <summary>
    /// Восклицательный знак
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка (красный крест)
    /// </summary>
    Error,
  }

  #endregion

  #endregion

}
