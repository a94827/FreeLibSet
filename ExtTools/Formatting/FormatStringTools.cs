// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

namespace FreeLibSet.Formatting
{
  #region Перечисление DateFormatYMDOrder

  /// <summary>
  /// Возможный относительный порядок расположения компонентов дня, месяца и года в формате дате
  /// </summary>
  public enum DateFormatYMDOrder
  {
    // Порядок элементов в перечислении важен!

    /// <summary>
    /// Год, месяц день ("японский")
    /// </summary>
    YMD = 0,

    /// <summary>
    /// Год, день, месяц (такой вариант, насколько я знаю, не встречается)
    /// </summary>
    YDM = 1,

    /// <summary>
    /// Месяц, день, год (США)
    /// </summary>
    MDY = 2,

    /// <summary>
    /// День, месяц, год (Россия)
    /// </summary>
    DMY = 3,
  }

  #endregion

  /// <summary>
  /// Статические функции для работы со строками форматирования
  /// </summary>
  public static class FormatStringTools
  {
    #region Числовые форматы

    /// <summary>
    /// Получить числовой формат для заданного числа десятичных разрядов.
    /// При отрицательном значении <paramref name="decimalPlaces"/> возвращается пустой формат.
    /// При нулевом - целочисленный формат "0", при положительном - строка "0.0000" с соответствующим числом нулей
    /// </summary>
    /// <param name="decimalPlaces">Количество разрядов после десятичной точки</param>
    /// <returns>Числовой формат</returns>
    public static string DecimalPlacesToNumberFormat(int decimalPlaces)
    {
      if (decimalPlaces < 0)
        return String.Empty;
      else if (decimalPlaces == 0)
        return "0";
      else
        return "0." + new string('0', decimalPlaces);
    }

    /// <summary>
    /// Получить количество десятичных разрядов из числового формата.
    /// Если значение <paramref name="format"/> не задано или формат не удается распознать как числовой, возвращается (-1).
    /// Текущая реализация является очень простой и распознает только форматы с нулями и "#": "0", "0.0", "0.0#", и т.п.
    /// </summary>
    /// <param name="format">Числовой формат</param>
    /// <returns>Количество десятичных разрядов</returns>
    public static int DecimalPlacesFromNumberFormat(string format)
    {
      if (String.IsNullOrEmpty(format))
        return -1;
      if (format.Length == 1) // один из стандартных форматов, например, "G"
      {
        char ch = format[0];
        if (ch>='A' && ch<='Z')
          return -1;
        if (ch >= 'a' && ch <= 'z')
          return -1;

        // 14.07.2023
        // Односимвольный формат может быть "0"
      }

      if (format.IndexOfAny(new char[] { '0', '#' }) < 0)
        return -1;

      int p = format.IndexOf('.');
      if (p < 0)
        return 0;

      int cnt = 0;
      for (int i = p + 1; i < format.Length; i++)
      {
        if (format[i] == '0' || format[i] == '#')
          cnt++;
        else
          break;
      }
      return cnt;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Определяет, содержит ли строка форматирования символы для даты и/или времени.
    /// Распознаются односимвольные и многосимвольные строки форматирования, которые можно передавать методу <see cref="DateTime.ToString(string)"/>.
    /// Если <paramref name="formatString"/>-пустая строка, то <paramref name="containsDate"/>=true и <paramref name="containsTime"/>=true.
    /// </summary>
    /// <param name="formatString">Строка форматирования для <see cref="DateTime"/></param>
    /// <param name="containsDate">Сюда записывается true, если строка форматирования содержит дату</param>
    /// <param name="containsTime">Сюда записывается true, если строка форматирования содержит время</param>
    public static void ContainsDateTime(string formatString, out bool containsDate, out bool containsTime)
    {
      if (String.IsNullOrEmpty(formatString))
      {
        containsDate = true;
        containsTime = true;
        return;
      }

      #region Стандартная строка форматирования

      if (formatString.Length == 1)
      {
        switch (formatString[0])
        {
          case 'd':
          case 'D':
          case 'm':
          case 'M':
          case 'y':
          case 'Y':
            containsDate = true;
            containsTime = false;
            break;
          case 't':
          case 'T':
            containsDate = false;
            containsTime = true;
            break;
          case 'f':
          case 'F':
          case 'g':
          case 'G':
          case 'r':
          case 'R':
          case 's':
          case 'u':
          case 'U':
          case 'o':
          case 'O':
            containsDate = true;
            containsTime = true;
            break;
          default:
            containsDate = false;
            containsTime = false;
            break;
        }
        return;
      }

      #endregion

      #region Разбираемые строки форматирования

      containsDate = false;
      containsTime = false;

      int p = 0;
      while (p < formatString.Length)
      {
        switch (formatString[p])
        {
          case 'd':
          case 'g':
          case 'M':
          case 'y':
            //case '/':
            containsDate = true;
            break;

          case 'h':
          case 'H':
          case 'm':
          case 's':
          case 'f':
          case 'F':
          case 't':
          case 'z':
            //case ':':
            containsTime = true;
            break;

          case '%': // получение шаблона
          case '\\': // escape-символ
            p++;
            break;

          case '\"':
          case '\'':
            // строки в кавычках
            char ch = formatString[p];
            p++;
            while (p < formatString.Length)
            {
              if (formatString[p] == ch)
                break;
              else
                p++;
            }
            break;
        }
        p++;
      }

      #endregion
    }

    /// <summary>
    /// Определяет, содержит ли строка форматирования символы для даты.
    /// Распознаются односимвольные и многосимвольные строки форматирования, которые можно передавать методу <see cref="DateTime.ToString(string)"/>.
    /// Если <paramref name="formatString"/>-пустая строка, то возвращается true.
    /// </summary>
    /// <param name="formatString">Строка форматирования для <see cref="DateTime"/></param>
    /// <returns>true, если строка форматирования содержит дату</returns>
    public static bool ContainsDate(string formatString)
    {
      bool containsDate;
      bool containsTime;
      ContainsDateTime(formatString, out containsDate, out containsTime);
      return containsDate;
    }

    /// <summary>
    /// Определяет, содержит ли строка форматирования символы для времени.
    /// Распознаются односимвольные и многосимвольные строки форматирования, которые можно передавать методу <see cref="DateTime.ToString(string)"/>.
    /// Если <paramref name="formatString"/>-пустая строка, то возвращается true.
    /// </summary>
    /// <param name="formatString">Строка форматирования для <see cref="DateTime"/></param>
    /// <returns>true, если строка форматирования содержит времени</returns>
    public static bool ContainsTime(string formatString)
    {
      bool containsDate;
      bool containsTime;
      ContainsDateTime(formatString, out containsDate, out containsTime);
      return containsTime;
    }

    /// <summary>
    /// Расширяет односимвольный формат даты/времени в полный формат.
    /// Если длина переданного формата отличается от 1 (формат не задан или задан пользовательский формат),
    /// он возвращается без изменений.
    /// Если передан неизвестный односимвольный формат, он также возвращается, без выброса исключений
    /// </summary>
    /// <param name="formatString">Исходный формат</param>
    /// <param name="dateTimeFormat">Форматизатор. Если null, то используется формат из <see cref="CultureInfo.CurrentCulture"/></param>
    /// <returns>Развернутый формат или <paramref name="formatString"/> без изменений</returns>
    public static string ExpandDateTimeFormat(string formatString, DateTimeFormatInfo dateTimeFormat)
    {
      if (dateTimeFormat == null)
        dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

      switch (formatString)
      {
        case "d":
          return dateTimeFormat.ShortDatePattern;
        case "D":
          return dateTimeFormat.LongDatePattern;
        case "f":
          return dateTimeFormat.LongDatePattern + " " + dateTimeFormat.ShortTimePattern;
        case "F":
        case "U":
          return dateTimeFormat.FullDateTimePattern;
        case "g":
          return dateTimeFormat.ShortDatePattern + " " + dateTimeFormat.ShortTimePattern;
        case "G":
          return dateTimeFormat.ShortDatePattern + " " + dateTimeFormat.LongTimePattern;
        case "o":
        case "O":
          return @"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK";
        case "r":
        case "R":
          return dateTimeFormat.RFC1123Pattern;
        case "s":
          return dateTimeFormat.SortableDateTimePattern;
        case "u":
          return dateTimeFormat.UniversalSortableDateTimePattern;
        case "t":
          return dateTimeFormat.ShortTimePattern;
        case "T":
          return dateTimeFormat.LongTimePattern;
        case "M":
        case "m":
          return dateTimeFormat.MonthDayPattern;
        case "Y":
        case "y":
          return dateTimeFormat.YearMonthPattern;
      }
      return formatString;
    }

    /// <summary>
    /// Расширяет односимвольный формат даты/времени в полный формат.
    /// Если длина переданного формата отличается от 1 (формат не задан или задан пользовательский формат),
    /// он возвращается без изменений.
    /// Если передан неизвестный односимвольный формат, он также возвращается, без выброса исключений.
    /// Эта перегрузка использует объект <see cref="DateTimeFormatInfo"/> из <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    /// <param name="formatString">Исходный формат</param>
    /// <returns>Развернутый формат или <paramref name="formatString"/> без изменений</returns>
    public static string ExpandDateTimeFormat(string formatString)
    {
      return ExpandDateTimeFormat(formatString, null);
    }


    /// <summary>
    /// Возвращает порядок следования дня, месяца и года в строке форматирования даты.
    /// Анализируется наличие символов "d", "M" и "y" для определения порядка.
    /// Если задан стандартный односимвольный формат (например, "D"), то он заменяется на полный формат 
    /// с помощью метода <see cref="ExpandDateTimeFormat(string, DateTimeFormatInfo)"/>.
    /// Если строка не задана или не содержит ни одного из символов "d", "M" и "y", то используется шаблон 
    /// <see cref="DateTimeFormatInfo.ShortDatePattern"/>.
    /// Если в строке части элементов нет, подбирается наиболее подходящий вариант.
    /// </summary>
    /// <param name="formatString">Короткий формат даты</param>
    /// <param name="dateTimeFormat">Формат даты/времени, используемый для преобразования стандартного формата.
    /// Если null, то используется <see cref="DateTimeFormatInfo"/> из <see cref="CultureInfo.CurrentCulture"/></param>
    /// <returns>Порядок следования компонентов</returns>
    public static DateFormatYMDOrder GetDateFormatOrder(string formatString, DateTimeFormatInfo dateTimeFormat)
    {
      if (dateTimeFormat == null)
        dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

      if (String.IsNullOrEmpty(formatString))
        formatString = "d";
      if (formatString.Length == 1) // стандартный формат, например, "d"
        formatString = ExpandDateTimeFormat(formatString, dateTimeFormat); // 13.07.2023

      DateFormatYMDOrder result;
      if (!DoGetDateFormatOrder(formatString, out result))
        DoGetDateFormatOrder(dateTimeFormat.ShortDatePattern, out result); // 13.07.2023
      return result;
    }

    private static bool DoGetDateFormatOrder(string formatString, out DateFormatYMDOrder result)
    {

      //int pD = formatString.IndexOf('d');
      //int pM = formatString.IndexOf('M');
      //int pY = formatString.IndexOf('y');
      //if (pY < 0)
      //  pY = -3;
      //if (pM < 0)
      //  pM = -2;
      //if (pD < 0)
      //  pD = -1;

      // 13.07.2023
      // Учитываем возможность наличия текстовых фрагментов в строке и Escape-символов

      int pY = -3;
      int pM = -2;
      int pD = -1;

      int p = 0;
      while (p < formatString.Length)
      {
        switch (formatString[p])
        {
          // Поскольку интересны только относительные положения символов d, M и y, неважно, будет ли использовано первое или последнее вхождение в последовательность.
          case 'd': pD = p; break;
          case 'M': pM = p; break;
          case 'y': pY = p; break;

          case '%': // получение шаблона
          case '\\': // escape-символ
            p++;
            break;

          case '\"':
          case '\'':
            // строки в кавычках
            char ch = formatString[p];
            p++;
            while (p < formatString.Length)
            {
              if (formatString[p] == ch)
                break;
              else
                p++;
            }
            break;
        }
        p++;
      }

      int index1 = pY < pM ? 0 : 2;
      int index2 = pM < pD ? 0 : 1;
      result = (DateFormatYMDOrder)(index1 + index2);
      return (pY >= 0 || pM >= 0 || pD >= 0);
    }

    /// <summary>
    /// Возвращает порядок следования дня, месяца и года в строке форматирования даты.
    /// Анализируется наличие символов "d", "M" и "y" для определения порядка.
    /// Если задан стандартный односимвольный формат (например, "D"), то он заменяется на полный формат 
    /// с помощью метода <see cref="ExpandDateTimeFormat(string)"/>.
    /// Если строка не задана или не содержит ни одного из символов "d", "M" и "y", то используется шаблон 
    /// <see cref="DateTimeFormatInfo.ShortDatePattern"/> для текущей культуры.
    /// Если в строке части элементов нет, подбирается наиболее подходящий вариант.
    /// Эта перегрузка использует текущую культуру <see cref="CultureInfo.CurrentCulture"/> для стандартных форматов.
    /// </summary>
    /// <param name="formatString">Короткий формат даты</param>
    /// <returns>Порядок следования компонентов</returns>
    public static DateFormatYMDOrder GetDateFormatOrder(string formatString)
    {
      return GetDateFormatOrder(formatString, null);
    }

    /// <summary>
    /// Возвращает порядок следования дня, месяца и года, используемый текущей культурой
    /// </summary>
    public static DateFormatYMDOrder DateFormatOrder
    {
      get { return GetDateFormatOrder(null); }
    }

#if XXX
    private static readonly string[] _Date10Formats = new string[] { "yyyy/MM/dd", "yyyy/dd/MM", "MM/dd/yyyy", "dd/MM/yyyy" };

    /// <summary>
    /// Возвращает "стандартизированный" формат даты из 10 символов, когда год отображается 4 цифрами,
    /// месяц и день - двумя. Между частями задается символ-разделитель.
    /// </summary>
    /// <param name="formatString">Короткий формат даты</param>
    /// <returns>"Стандартизированный" 10-символьный формат</returns>
    public static string GetDate10Format(string formatString)
    {
      return _Date10Formats[(int)GetDateFormatOrder(formatString)];
    }

    /// <summary>
    /// Возвращает "стандартизированный" формат даты из 10 символов, используемый текущей культурой.
    /// Год отображается 4 цифрами, месяц и день - двумя. Между частями задается символ-разделитель.
    /// </summary>
    public static string Date10Format
    {
      get { return GetDate10Format(null); }
    }

    /// <summary>
    /// Маска для редактирования даты: "00/00/0000" или "0000/00/00"
    /// </summary>
    public static string Date10EditMask
    {
      get
      {
        switch (DateFormatOrder)
        {
          case DateFormatYMDOrder.YDM:
          case DateFormatYMDOrder.YMD:
            return "0000/00/00";
          default:
            return "00/00/0000";
        }
      }
    }
#endif
    #endregion
  }

  #region Перечисление EditableDateTimeFormatterKind

  /// <summary>
  /// Тип форматизатора даты и времени
  /// </summary>
  public enum EditableDateTimeFormatterKind
  {
    /// <summary>
    /// Короткий формат даты ("ДД.ММ.ГГГГ")
    /// При вызове методов Parse()/TryParse() компонент времени имеет значение 00:00:00.
    /// </summary>
    Date,

    /// <summary>
    /// Короткий формат времени без секунд ("ЧЧ:ММ").
    /// При вызове методов Parse()/TryParse() компонент даты имеет значение текущей даты DateTime.Today.
    /// </summary>
    ShortTime,

    /// <summary>
    /// Формат времени с секундами ("ЧЧ:ММ:СС")
    /// При вызове методов Parse()/TryParse() компонент даты имеет значение текущей даты DateTime.Today.
    /// </summary>
    Time,

    /// <summary>
    /// Формат даты и времени без секунд "ДД.ММ.ГГГГ ЧЧ:ММ"
    /// </summary>
    ShortDateTime,

    /// <summary>
    /// Формат даты и времени с секундами "ДД.ММ.ГГГГ ЧЧ:ММ:СС"
    /// </summary>
    DateTime
  }

  #endregion

  /// <summary>
  /// Форматизатор даты/времени, который удобно использовать в полях ввода, т.к.
  /// обеспечивается одинаковое количество символов для всех значений
  /// </summary>
  public sealed class EditableDateTimeFormatter
  {
    #region Конструктор

    private static readonly string[] _Date10Formats = new string[] { "yyyy/MM/dd", "yyyy/dd/MM", "MM/dd/yyyy", "dd/MM/yyyy" };

    /// <summary>
    /// Используйте статические свойства класса <see cref="EditableDateTimeFormatters"/> для доступа к форматизаторам
    /// </summary>
    /// <param name="cultureInfo">Культура. Не может быть null</param>
    /// <param name="kind">Способ форматирования даты/времени</param>
    public EditableDateTimeFormatter(CultureInfo cultureInfo, EditableDateTimeFormatterKind kind)
    {
#if DEBUG
      if (cultureInfo == null)
        throw new ArgumentNullException("cultureInfo");
#endif
      _FormatInfo = cultureInfo.DateTimeFormat;
      _kind = kind;

      #region Дата

      string dateFormat;
      string dateMask;
      switch (kind)
      {
        case EditableDateTimeFormatterKind.Date:
        case EditableDateTimeFormatterKind.ShortDateTime:
        case EditableDateTimeFormatterKind.DateTime:
          DateFormatYMDOrder dateOrder = FormatStringTools.GetDateFormatOrder(_FormatInfo.ShortDatePattern);
          dateFormat = _Date10Formats[(int)dateOrder];
          if (dateOrder == DateFormatYMDOrder.YDM || dateOrder == DateFormatYMDOrder.YMD)
            dateMask = "0000/00/00";
          else
            dateMask = "00/00/0000";
          break;
        default:
          dateFormat = String.Empty;
          dateMask = String.Empty;
          break;
      }

      #endregion

      #region Время

      string timeFormat;
      string timeMask;
      int timeWidth;
      int ampmTextWidth = 0;
      bool useAMPM;
      switch (kind)
      {
        case EditableDateTimeFormatterKind.ShortTime:
        case EditableDateTimeFormatterKind.ShortDateTime:
          useAMPM = _FormatInfo.ShortTimePattern.IndexOf("tt", StringComparison.Ordinal) >= 0;
          timeFormat = useAMPM ? "hh:mm tt" : "HH:mm";
          timeMask = useAMPM ? ("00:00 " + GetAMPMMask(_FormatInfo, out ampmTextWidth)) : "00:00";
          timeWidth = 6;
          break;
        case EditableDateTimeFormatterKind.Time:
        case EditableDateTimeFormatterKind.DateTime:
          useAMPM = _FormatInfo.LongTimePattern.IndexOf("tt", StringComparison.Ordinal) >= 0;
          timeFormat = useAMPM ? "hh:mm:ss tt" : "HH:mm:ss";
          timeMask = useAMPM ? ("00:00:00 " + GetAMPMMask(_FormatInfo, out ampmTextWidth)) : "00:00:00";
          timeWidth = 8;
          break;
        default:
          useAMPM = false;
          timeFormat = String.Empty;
          timeMask = String.Empty;
          timeWidth = 0;
          break;
      }

      #endregion

      #region Инициализация свойств

      _Format = dateFormat;
      _EditMask = dateMask;
      _TextWidth = dateFormat.Length;

      switch (kind)
      {
        case EditableDateTimeFormatterKind.ShortDateTime:
        case EditableDateTimeFormatterKind.DateTime:
          // Добавляем разделитель между датой и временем
          _Format += " ";
          _EditMask += " ";
          _TextWidth += 1;
          break;
      }

      if (timeFormat.Length > 0)
      {
        _Format += timeFormat;
        _EditMask += timeMask;
        _TextWidth += timeWidth + (useAMPM ? (1 + ampmTextWidth) : 0);
      }

      _MaskProvider = new StdMaskProvider(_EditMask, cultureInfo);

      #endregion
    }

    /// <summary>
    /// Возвращает часть маски для редактирования AM/PM
    /// </summary>
    private string GetAMPMMask(DateTimeFormatInfo formatInfo, out int ampmTextWidth)
    {
      string s1 = formatInfo.AMDesignator;
      string s2 = formatInfo.PMDesignator;
      ampmTextWidth = Math.Max(s1.Length, s2.Length);
      // Вряд ли десигнаторы могут быть разной длины, но на всякий случай проверим

      bool hasStrangeChars = false;
      for (int i = 0; i < s1.Length; i++)
      {
        UnicodeCategory uc = Char.GetUnicodeCategory(s1, i);
        if (!Char.IsLetterOrDigit(s1, i))
        {
          hasStrangeChars = true;
          break;
        }
      }
      for (int i = 0; i < s2.Length; i++)
      {
        if (!Char.IsLetterOrDigit(s2, i))
        {
          hasStrangeChars = true;
          break;
        }
      }

      if (hasStrangeChars)
        return new String('C', ampmTextWidth); // 20.07.2023

      s1 = s1.PadRight(ampmTextWidth);
      s2 = s2.PadRight(ampmTextWidth);

      
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < ampmTextWidth; i++)
      {
        if (s1[i] == s2[i])
        {
          // Два одинаковых символа
          sb.Append('\\');
          sb.Append(s1[i]);
        }
        else
        {
          // TODO: уточнить форматы
          sb.Append("C"); // любой символ
        }
      }

      return sb.ToString();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Тип форматизатора.
    /// </summary>
    public EditableDateTimeFormatterKind Kind { get { return _kind; } }
    private EditableDateTimeFormatterKind _kind;

    /// <summary>
    /// Формат
    /// </summary>
    public string Format { get { return _Format; } }
    private string _Format;

    /// <summary>
    /// Провайдер для форматирования
    /// </summary>
    public IFormatProvider FormatProvider { get { return _FormatInfo; } }
    private DateTimeFormatInfo _FormatInfo;

    /// <summary>
    /// Маска, которую нужно использовать в поле ввода, например, MaskedTextBox
    /// </summary>
    public string EditMask { get { return _EditMask; } }
    private string _EditMask;

    /// <summary>
    /// Ширина текстового поля (количество знаков в поле ввода)
    /// </summary>
    public int TextWidth { get { return _TextWidth; } }
    private int _TextWidth;

    /// <summary>
    /// Провайдер для обслуживания маски ввода
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } }
    private StdMaskProvider _MaskProvider;

    /// <summary>
    /// Возвращает true, если методы <see cref="Parse(string, int)"/> и <see cref="TryParse(string, out DateTime, int)"/> могут обрабатывать аргумент defaultYear, когда
    /// год не задан в преобразуемой строке. Если false, то этот аргумент игнорируется.
    /// </summary>
    public bool DefaultYearSupported
    {
      get
      {
        return Kind == EditableDateTimeFormatterKind.Date &&
          Format[Format.Length - 1] == 'y'; // Заканчивается на год
      }
    }

    /// <summary>
    /// Возвращает true, если в формате присутствует компонент даты.
    /// Как минимум, одно из свойств <see cref="ContainsDate"/> и <see cref="ContainsTime"/> должно возвращать true.
    /// </summary>
    public bool ContainsDate
    {
      get
      {
        switch (Kind)
        {
          case EditableDateTimeFormatterKind.Date:
          case EditableDateTimeFormatterKind.DateTime:
          case EditableDateTimeFormatterKind.ShortDateTime:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Возвращает true, если в формате присутствует компонент времени.
    /// Как минимум, одно из свойств <see cref="ContainsDate"/> и <see cref="ContainsTime"/> должно возвращать true.
    /// </summary>
    public bool ContainsTime
    {
      get
      {
        switch (Kind)
        {
          case EditableDateTimeFormatterKind.Time:
          case EditableDateTimeFormatterKind.ShortTime:
          case EditableDateTimeFormatterKind.DateTime:
          case EditableDateTimeFormatterKind.ShortDateTime:
            return true;
          default:
            return false;
        }
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает дату/время, отформатированную в соответствии со свойством <see cref="Format"/>
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(DateTime value)
    {
      return value.ToString(Format, FormatProvider);
    }

    /// <summary>
    /// Выполняет попытку преобразования строки в значение даты/времени
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <param name="defaultYear">Если передано ненулевое значение, свойство <see cref="DefaultYearSupported"/>=true,
    /// а в строке нет года, то используется этот год</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public bool TryParse(string s, out DateTime value, int defaultYear)
    {
      if (defaultYear != 0 && DefaultYearSupported)
      {
        if (s.Length == 6)
          s += defaultYear.ToString("0000");
      }

      //return DateTime.TryParseExact(s, Format, FormatProvider, DateTimeStyles.AllowWhiteSpaces, out value);
      return DateTime.TryParse(s, FormatProvider, DateTimeStyles.AllowWhiteSpaces, out value);
    }

    /// <summary>
    /// Выполняет попытку преобразования строки в значение даты/времени
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public bool TryParse(string s, out DateTime value)
    {
      return TryParse(s, out value, 0);
    }

    /// <summary>
    /// Выполняет преобразование строки в значение даты/времени.
    /// В случае ошибки генерируется <see cref="FormatException"/>.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="defaultYear">Если передано ненулевое значение, свойство <see cref="DefaultYearSupported"/>=true,
    /// а в строке нет года, то используется этот год</param>
    /// <returns>Преобразованное значение</returns>
    public DateTime Parse(string s, int defaultYear)
    {
      DateTime value;
      if (TryParse(s, out value, defaultYear))
        return value;
      else
        throw new FormatException();
    }

    /// <summary>
    /// Выполняет преобразование строки в значение даты/времени.
    /// В случае ошибки генерируется <see cref="FormatException"/>
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение</returns>
    public DateTime Parse(string s)
    {
      return Parse(s, 0);
    }

    /// <summary>
    /// Преобразование строки в Nullable-значение.
    /// В случае ошибки возвращается null, как для пустой строки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="defaultYear">Если передано ненулевое значение, свойство <see cref="DefaultYearSupported"/>=true,
    /// а в строке нет года, то используется этот год</param>
    /// <returns>Преобразованное значение или null</returns>
    public DateTime? ToNValue(string s, int defaultYear)
    {
      DateTime value;
      if (TryParse(s, out value, defaultYear))
        return value;
      else
        return null;
    }
    /// <summary>
    /// Преобразование строки в Nullable-значение.
    /// В случае ошибки возвращается null, как для пустой строки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Преобразованное значение или null</returns>
    public DateTime? ToNValue(string s)
    {
      return ToNValue(s, 0);
    }

    #endregion
  }

  /// <summary>
  /// Коллекция форматизаторов даты/времени, относящихся к текущему потоку
  /// </summary>
  public static class EditableDateTimeFormatters
  {
    #region Отслеживание изменений в потоке

    [ThreadStatic]
    private static DateTimeFormatInfo _CurrentInfo;

    private static void CheckCurrentInfo()
    {
      if (!Object.ReferenceEquals(_CurrentInfo, Thread.CurrentThread.CurrentCulture.DateTimeFormat))
      {
        _Items = null;
        _MonthNames12 = null;
        _MonthGenitiveNames12 = null;
        _CurrentInfo = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
      }

    }

    #endregion

    #region Объекты EditableDateTimeFormatter

    /// <summary>
    /// Форматизатор для даты 
    /// </summary>
    public static EditableDateTimeFormatter Date { get { return Get(EditableDateTimeFormatterKind.Date); } }

    /// <summary>
    /// Форматизатор для времени без секунд
    /// </summary>
    public static EditableDateTimeFormatter ShortTime { get { return Get(EditableDateTimeFormatterKind.ShortTime); } }

    /// <summary>
    /// Форматизатор для времени с секундами
    /// </summary>
    public static EditableDateTimeFormatter Time { get { return Get(EditableDateTimeFormatterKind.Time); } }

    /// <summary>
    /// Форматизатор для даты и времени без секунд
    /// </summary>
    public static EditableDateTimeFormatter ShortDateTime { get { return Get(EditableDateTimeFormatterKind.ShortDateTime); } }

    /// <summary>
    /// Форматизатор для даты и времени с секундами
    /// </summary>
    public static EditableDateTimeFormatter DateTime { get { return Get(EditableDateTimeFormatterKind.DateTime); } }

    [ThreadStatic]
    private static EditableDateTimeFormatter[] _Items;

    /// <summary>
    /// Доступ к форматизатору по его виду
    /// </summary>
    /// <param name="kind">Вид форматизатора</param>
    /// <returns>Форматизатор</returns>
    public static EditableDateTimeFormatter Get(EditableDateTimeFormatterKind kind)
    {
      CheckCurrentInfo();

      if (_Items == null)
        _Items = new EditableDateTimeFormatter[5];
      if (_Items[(int)kind] == null)
        _Items[(int)kind] = new EditableDateTimeFormatter(Thread.CurrentThread.CurrentCulture, kind);
      return _Items[(int)kind];
    }

    #endregion

    #region Статические списки

    /// <summary>
    /// Возвращает <see cref="DateTimeFormatInfo.MonthNames"/> без пустого 13-го месяца ("Январь", "Февраль", ..., "Декабрь")
    /// </summary>
    public static string[] MonthNames12
    {
      get
      {
        CheckCurrentInfo();

        if (_MonthNames12 == null)
        {
          string[] a = new string[12];
          Array.Copy(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, 0, a, 0, 12);
          _MonthNames12 = a;
        }
        return _MonthNames12;
      }
    }
    [ThreadStatic]
    private static string[] _MonthNames12;


    /// <summary>
    /// Возвращает <see cref="DateTimeFormatInfo.MonthGenitiveNames"/> без пустого 13-го месяца ("января", "февраля", ..., "декабря")
    /// </summary>
    public static string[] MonthGenitiveNames12
    {
      get
      {
        CheckCurrentInfo();

        if (_MonthGenitiveNames12 == null)
        {
          string[] a = new string[12];
          Array.Copy(CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames, 0, a, 0, 12);
          _MonthGenitiveNames12 = a;
        }
        return _MonthGenitiveNames12;
      }
    }
    [ThreadStatic]
    private static string[] _MonthGenitiveNames12;

    #endregion
  }
}
