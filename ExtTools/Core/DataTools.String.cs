// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections;
using FreeLibSet.Collections;

namespace FreeLibSet.Core
{
  partial class DataTools
  {
    #region Манипуляции со строками

    #region ReplaceDigits

    /// <summary>
    /// Заменить все числовые символы в строке на заданный символ
    /// </summary>
    /// <param name="str">Строка</param>
    /// <param name="c">Символ, на который нужно заменить числовые символы</param>
    /// <returns>Строка после замены</returns>
    public static string ReplaceDigits(string str, char c)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      StringBuilder s1 = null; // создадим, если нужно
      for (int i = 0; i < str.Length; i++)
      {
        if (str[i] >= '0' && str[i] <= '9')
        {
          if (s1 == null)
          {
            s1 = new StringBuilder(str.Length);
            if (i > 0)
              s1.Append(str.Substring(0, i)); // все предвдущие символы
          }
          s1.Append(c);
        }
        else
          if (s1 != null)
          s1.Append(str[i]);
      }
      if (s1 == null)
        return str;
      else
        return s1.ToString();
    }

    #endregion

    #region PadXXX

    /// <summary>
    /// Дополнение строки <paramref name="s"/> справа до нужной длины <paramref name="length"/> символом <paramref name="paddingChar"/>.
    /// В отличие от <see cref="String.PadRight(int, char)"/>, выполняет обрезание строки, если она длиннее,
    /// чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <param name="paddingChar">Символ-заполнитель</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadRight(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        return s.Substring(0, length);
      return s.PadRight(length, paddingChar);
    }

    /// <summary>
    /// Дополнение строки <paramref name="s"/> справа до нужной длины <paramref name="length"/> пробелами.
    /// В отличие от <see cref="String.PadRight(int)"/>, выполняет обрезание строки, если она длиннее,
    /// чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadRight(string s, int length)
    {
      return PadRight(s, length, ' ');
    }

    /// <summary>
    /// Дополнение строки <paramref name="s"/> слева до нужной длины <paramref name="length"/> символом <paramref name="paddingChar"/>.
    /// В отличие от <see cref="String.PadLeft(int, char)"/>, выполняет обрезание строки, если она длиннее,
    /// чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <param name="paddingChar">Символ-заполнитель</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadLeft(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        return s.Substring(s.Length - length);
      return s.PadLeft(length, paddingChar);
    }

    /// <summary>
    /// Дополнение строки <paramref name="s"/> слева до нужной длины <paramref name="length"/> пробелами.
    /// В отличие от <see cref="String.PadLeft(int)"/>, выполняет обрезание строки, если она длиннее,
    /// чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadLeft(string s, int length)
    {
      return PadLeft(s, length, ' ');
    }

    /// <summary>
    /// Дополнение строки <paramref name="s"/> слева и справа до нужной длины <paramref name="length"/> символом <paramref name="paddingChar"/>.
    /// Выполняет обрезание начальных и конечных символов строки,
    /// если она длиннее, чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <param name="paddingChar">Символ-заполнитель</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadCenter(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        //return s.Substring(s.Length - length);
        return s.Substring(0, length); // 14.07.2021

      // Дополнение пробелами с обеих сторон
      int n1 = (length - s.Length) / 2;
      int n2 = length - s.Length - n1;
      return new string(paddingChar, n1) + s + new string(paddingChar, n2);
    }

    /// <summary>
    /// Дополнение строки <paramref name="s"/> слева и справа до нужной длины <paramref name="length"/> пробелами.
    /// Выполняет обрезание начальных и конечных символов строки,
    /// если она длиннее, чем <paramref name="length"/>. Также правильно обрабатывается <paramref name="s"/>=null.
    /// Возвращаемая строка всегда имеет длину ровно <paramref name="length"/> символов. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <returns>Строка длиной <paramref name="length"/> символов</returns>
    public static string PadCenter(string s, int length)
    {
      return PadCenter(s, length, ' ');
    }

    #endregion

    #region StrXXX

    /// <summary>
    /// Возвращает первые <paramref name="length"/> символов из строки <paramref name="s"/>.
    /// Аналог Clipper-функции LEFT()
    /// Строка может быть пустой, null или короче <paramref name="length"/>. В этом случае возвращается
    /// строка меньшей длины или <see cref="String.Empty"/>. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <returns>Строка длиной не более <paramref name="length"/> символов</returns>
    public static string StrLeft(string s, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.Length <= length)
        return s;
      return s.Substring(0, length);
    }

    /// <summary>
    /// Возвращает последние <paramref name="length"/> символов из строки <paramref name="s"/>.
    /// Аналог Clipper-функции RIGHT()
    /// Строка может быть пустой, null или короче <paramref name="length"/>. В этом случае возвращается
    /// строка меньшей длины или <see cref="String.Empty"/>. Значение null заменяется на <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Может быть null</param>
    /// <param name="length">Требуемая длина строки. Если меньше или равно 0,
    /// то будет возвращена пустая строка</param>
    /// <returns>Строка длиной не более <paramref name="length"/> символов</returns>
    public static string StrRight(string s, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.Length <= length)
        return s;
      return s.Substring(s.Length - length);
    }

    #endregion

    #region Верхний/нижний регистр регистр

    // Здесь используются пары методов Xxx() и XxxInvariant()

    #region ToUpperFirst()

    /// <summary>
    /// Преобразование первого символа строки к верхнему регистру, а остальных - к нижнему
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="culture">Культура, используемая при вызове методов <see cref="String.ToUpper(CultureInfo)"/> и <see cref="string.ToLower(CultureInfo)"/></param>
    /// <returns>Строка с измененным регистром символов</returns>
    public static string ToUpperFirst(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      else
        return s.Substring(0, 1).ToUpper(culture) + s.Substring(1).ToLower(culture);
    }

    /// <summary>
    /// Преобразование первого символа строки к верхнему регистру, а остальных - к нижнему.
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Строка с измененным регистром символов</returns>
    public static string ToUpperFirstInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      else
        return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1).ToLowerInvariant();
    }

    #endregion

    #region ChangeUpperLower()

    /// <summary>
    /// Заменяет буквы верхнего регистра на буквы нижнего регистра и наоборот.
    /// Например, "Hello, world!" будет заменено на "hELLO, wORLD!".
    /// <param name="culture">Культура, используемая при вызове методов <see cref="String.ToUpper(CultureInfo)"/> и <see cref="string.ToLower(CultureInfo)"/></param>
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Строка с измененным регистром</returns>
    public static string ChangeUpperLower(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        if (char.IsUpper(s, i))
          a[i] = char.ToLower(s[i], culture);
        else if (char.IsLower(s, i))
          a[i] = char.ToUpper(s[i], culture);
        else
          a[i] = s[i];
      }

      return new string(a);
    }

    /// <summary>
    /// Заменяет буквы верхнего регистра на буквы нижнего регистра и наоборот.
    /// Например, "Hello, world!" будет заменено на "hELLO, wORLD!".
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Строка с измененным регистром</returns>
    public static string ChangeUpperLowerInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        if (char.IsUpper(s, i))
          a[i] = char.ToLowerInvariant(s[i]);
        else if (char.IsLower(s, i))
          a[i] = char.ToUpperInvariant(s[i]);
        else
          a[i] = s[i];
      }

      return new string(a);
    }

    #endregion

    #region ToUpperWords()

    /// <summary>
    /// Преобразует первые символы каждого слова к верхнему регистру, а остальные - к нижнему.
    /// Границей слова является любой небуквенный символ, для которого <see cref="Char.IsLetter(char)"/> возвращает false.
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="culture">Культура, используемая при вызове методов <see cref="Char.ToUpper(char, CultureInfo)"/> и <see cref="Char.ToLower(char, CultureInfo)"/></param>
    /// <returns>Преобразованный текст</returns>
    public static string ToUpperWords(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];

      bool nextUpper = true;
      for (int i = 0; i < s.Length; i++)
      {
        if (Char.IsLetter(s[i]))
        {
          if (nextUpper)
          {
            a[i] = Char.ToUpper(s[i], culture);
            nextUpper = false;
          }
          else
          {
            a[i] = Char.ToLower(s[i], culture);
          }
        }
        else
        {
          nextUpper = true;
          a[i] = s[i]; // 14.07.2021
        }
      }

      return new string(a);
    }


    /// <summary>
    /// Преобразует первые символы каждого слова к верхнему регистру, а остальные - к нижнему.
    /// Границей слова является любой небуквенный символ, для которого <see cref="Char.IsLetter(char)"/> возвращает false.
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Преобразованный текст</returns>
    public static string ToUpperWordsInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];

      bool nextUpper = true;
      for (int i = 0; i < s.Length; i++)
      {
        if (Char.IsLetter(s[i]))
        {
          if (nextUpper)
          {
            a[i] = Char.ToUpperInvariant(s[i]);
            nextUpper = false;
          }
          else
          {
            a[i] = Char.ToLowerInvariant(s[i]);
          }
        }
        else
        {
          nextUpper = true;
          a[i] = s[i]; // 14.07.2021
        }
      }

      return new string(a);
    }

    #endregion

    #region Преобразование регистра для массива строк

    /// <summary>
    /// Создает копию массива строк <paramref name="a"/>, в котором преобразует все строки к верхнему регистру с учетом культуры.
    /// Исходный массив <paramref name="a"/> не модифицируется.
    /// Если <paramref name="a"/>==null, возвращается null.
    /// Если в массиве встречаются значения null, они будут и в результирующем массиве
    /// </summary>
    /// <param name="a">Исходный массив строк</param>
    /// <param name="culture">Культура, используемая для преобразования</param>
    /// <returns>Копия массива с преобразованными строками</returns>
    public static string[] ToUpper(string[] a, CultureInfo culture)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToUpper(culture);
      }
      return b;
    }

    /// <summary>
    /// Создает копию массива строк <paramref name="a"/>, в котором преобразует все строки к верхнему регистру.
    /// Исходный массив <paramref name="a"/> не модифицируется.
    /// Если <paramref name="a"/>==null, возвращается null.
    /// Если в массиве встречаются значения null, они будут и в результирующем массиве
    /// </summary>
    /// <param name="a">Исходный массив строк</param>
    /// <returns>Копия массива с преобразованными строками</returns>
    public static string[] ToUpperInvariant(string[] a)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToUpperInvariant();
      }
      return b;
    }

    /// <summary>
    /// Создает копию массива строк <paramref name="a"/>, в котором преобразует все строки к нижнему регистру с учетом культуры.
    /// Исходный массив <paramref name="a"/> не модифицируется.
    /// Если <paramref name="a"/>==null, возвращается null.
    /// Если в массиве встречаются значения null, они будут и в результирующем массиве
    /// </summary>
    /// <param name="a">Исходный массив строк</param>
    /// <param name="culture">Культура, используемая для преобразования</param>
    /// <returns>Копия массива с преобразованными строками</returns>
    public static string[] ToLower(string[] a, CultureInfo culture)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToLower(culture);
      }
      return b;
    }

    /// <summary>
    /// Создает копию массива строк <paramref name="a"/>, в котором преобразует все строки к верхнему регистру.
    /// Исходный массив <paramref name="a"/> не модифицируется.
    /// Если <paramref name="a"/>==null, возвращается null.
    /// Если в массиве встречаются значения null, они будут и в результирующем массиве
    /// </summary>
    /// <param name="a">Исходный массив строк</param>
    /// <returns>Копия массива с преобразованными строками</returns>
    public static string[] ToLowerInvariant(string[] a)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToLowerInvariant();
      }
      return b;
    }

    #endregion

    #endregion

    #region AddStrIfNoEmpty

    /// <summary>
    /// Добавляет строку <paramref name="addedStr"/> к строке <paramref name="resStr"/>, если <paramref name="addedStr"/> непустая. 
    /// Перед <paramref name="addedStr"/> добавляется сепаратор <paramref name="separator"/>. 
    /// Если <paramref name="addedStr"/> - пустая строка, то <paramref name="resStr"/> остается без изменений. 
    /// Сепаратор не добавляется, если <paramref name="resStr"/> - пустая строка.
    /// Рекомендуется использовать версию для <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="resStr">Собираемая строка (по ссылке)</param>
    /// <param name="addedStr">Добавляемая строка</param>
    /// <param name="separator">Разделитель</param>
    public static void AddStrIfNotEmpty(ref string resStr, string addedStr, string separator)
    {
      if (String.IsNullOrEmpty(addedStr))
        return;

      if (String.IsNullOrEmpty(resStr))
        resStr = addedStr;
      else
      {
        if (!String.IsNullOrEmpty(separator))
          resStr += separator;
        resStr += addedStr;
      }
    }

    /// <summary>
    /// Добавляет строку <paramref name="addedStr"/> к строке <paramref name="resStr"/>, если <paramref name="addedStr"/> непустая. 
    /// Перед <paramref name="addedStr"/> добавляется сепаратор <paramref name="separator"/>. 
    /// Если <paramref name="addedStr"/> - пустая строка, то <paramref name="resStr"/> остается без изменений. 
    /// Сепаратор не добавляется, если <paramref name="resStr"/> - пустая строка.
    /// Версия для <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="resStr">Собираемая строка (по ссылке)</param>
    /// <param name="addedStr">Добавляемая строка</param>
    /// <param name="separator">Разделитель</param>
    public static void AddStrIfNotEmpty(StringBuilder resStr, string addedStr, string separator)
    {
      if (String.IsNullOrEmpty(addedStr))
        return;

      if (resStr.Length > 0 && (!String.IsNullOrEmpty(separator)))
        resStr.Append(separator);
      resStr.Append(addedStr);
    }

    #endregion

    #region Шестнадцатиричное представление

    /// <summary>
    /// Преобразование массива байтов в сплошную строку (без пробелов) в
    /// шестнадцатиричном формате. Выходная строка в 2 раза длиннее, чем
    /// длина массива <paramref name="bytes"/>.
    /// Если <paramref name="bytes"/>=null, то возвращается пустая строка
    /// Метод может использоваться, например, для вывода хэш-суммы, вычисленной
    /// по алгоритму MD5
    /// </summary>
    /// <param name="bytes">Массив байт произвольной длины</param>
    /// <param name="upperCase">Если true, то используются буквы "A"-"F",
    /// если false, то "a"-"f"</param>
    /// <returns>Строка 16-ричных символов</returns>
    public static string BytesToHex(byte[] bytes, bool upperCase)
    {
      if (bytes == null)
        return String.Empty;
      if (bytes.Length == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder(bytes.Length * 2);

      //for (int i = 0; i < bytes.Length; i++)
      //  sb.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
      BytesToHex(sb, bytes, upperCase); // 28.01.2020

      return sb.ToString();
    }

    /// <summary>
    /// Преобразование массива байтов в сплошную строку (без пробелов) в
    /// шестнадцатиричном формате. Выходная строка в 2 раза длиннее, чем
    /// длина массива <paramref name="bytes"/>.
    /// Если <paramref name="bytes"/>=null, то возвращается пустая строка
    /// Метод может использоваться, например, для вывода хэш-суммы, вычисленной
    /// по алгоритму MD5
    /// </summary>
    /// <param name="sb">Буфер для записи</param>
    /// <param name="bytes">Массив байт произвольной длины</param>
    /// <param name="upperCase">Если true, то используются буквы "A"-"F",
    /// если false, то "a"-"f"</param>
    public static void BytesToHex(StringBuilder sb, byte[] bytes, bool upperCase)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
#endif

      if (bytes == null)
        return;

      string s16 = upperCase ? "0123456789ABCDEF" : "0123456789abcdef";

      for (int i = 0; i < bytes.Length; i++)
      {
        //sb.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
        // 28.01.2020
        sb.Append(s16[bytes[i] >> 4]);
        sb.Append(s16[bytes[i] & 0x0F]);
      }
    }

    /// <summary>
    /// Преобразование последовательности символов 0-9,A-F,a-f в массив байт.
    /// Строка должна содержать четное число символов.
    /// Если длина строки - нечетная или строка содержит недопустимые символы - выбрасывается ArgumentException
    /// Пустая строка преобразуется в пустой массив.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Массив байт</returns>
    public static byte[] HexToBytes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return EmptyBytes;
      if ((s.Length % 2) != 0)
        throw new ArgumentException("Строка содержит нечетное количество символов (" + s.Length.ToString() + ")", "s");
      byte[] a = new byte[s.Length / 2];
      for (int i = 0; i < a.Length; i++)
        a[i] = (byte)(GetBytePart(s[2 * i]) << 4 | GetBytePart(s[2 * i + 1]));
      return a;
    }

    private static int GetBytePart(char c)
    {
      if (c >= '0' && c <= '9')
        return (c - '0');
      if (c >= 'A' && c <= 'F')
        return (c - 'A') + 10;
      if (c >= 'a' && c <= 'f')
        return (c - 'a') + 10;

      throw new ArgumentException("Недопустимый символ \"" + c + "\"");
    }

    #endregion

    #region Last/IndexOfAny/Other

    /// <summary>
    /// Возвращает true, если метод поиска символов выгодно реализовать с использованием <see cref="CharArrayIndexer"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="searchChars"></param>
    /// <returns></returns>
    private static bool PreferCharArrayIndexer(string str, string searchChars)
    {
      return (str.Length > 2 && str.Length * searchChars.Length > 50);
    }


    /// <summary>
    /// Версия стандартого метода <see cref="String.IndexOfAny(char[])"/>, которая, в отличие от оригинала,
    /// проверяемые символы берет из строки, а не из массива.
    /// Функция возвращает индекс первого символа из строки <paramref name="str"/>, который присутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых нет в строке <paramref name="searchChars"/>, то возвращается (-1)
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int IndexOfAny(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(searchChars))
        return -1;

      if (PreferCharArrayIndexer(str, searchChars))
        return IndexOfAny(str, new CharArrayIndexer(searchChars));

      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// Версия стандартого метода <see cref="String.IndexOfAny(char[])"/>, которая, в отличие от оригинала,
    /// проверяемые символы берет из строки, а не из массива.
    /// Функция возвращает индекс первого символа из строки <paramref name="str"/>, который присутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых нет в строке <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int IndexOfAny(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str) || searchChars.Count == 0)
        return -1;
      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// Функция, обратная к <see cref="IndexOfAny(string, string)"/>.
    /// Функция возвращает индекс первого символа из строки <paramref name="str"/>, который отсутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// входящих в строку <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если строка <paramref name="str"/> непустая, а <paramref name="searchChars"/> - пустая, то возвращается значение 0, т.к.
    /// первый же символ не входит в список
    /// Функция полезна для проверки правильности имен и других подобных случаев. 
    /// В качестве строки <paramref name="searchChars"/> следует передавать список всех допустимых символов и
    /// сообщать об ошибке, если функция вернула значение, большее или равное 0.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int IndexOfAnyOther(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (String.IsNullOrEmpty(searchChars))
        return 0; // любой символ подходит

      if (PreferCharArrayIndexer(str, searchChars))
        return IndexOfAnyOther(str, new CharArrayIndexer(searchChars));

      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Функция, обратная к <see cref="IndexOfAny(string, CharArrayIndexer)"/>.
    /// Функция возвращает индекс первого символа из строки <paramref name="str"/>, который отсутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых входящих в строку <paramref name="searchChars"/>, то возвращается (-1)
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если строка <paramref name="str"/> непустая, а <paramref name="searchChars"/> - пустая, то возвращается значение 0, т.к.
    /// первый же символ не входит в список
    /// Функция полезна для проверки правильности имен и других подобных случаев. 
    /// В качестве строки <paramref name="searchChars"/> следует передавать список всех допустимых символов и
    /// сообщать об ошибке, если функция вернула значение, большее или равное 0.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int IndexOfAnyOther(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (searchChars.Count == 0)
        return 0; // любой символ подходит
      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// Версия стандартого метода <see cref="String.LastIndexOfAny(char[])"/>, которая, в отличие от оригинала,
    /// проверяемые символы берет из строки, а не из массива.
    /// Функция возвращает индекс последнего символа из строки <paramref name="str"/>, который присутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых нет в строке <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int LastIndexOfAny(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(searchChars))
        return -1;

      if (PreferCharArrayIndexer(str, searchChars))
        return LastIndexOfAny(str, new CharArrayIndexer(searchChars));

      for (int i = str.Length - 1; i >= 0; i--)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// Версия стандартого метода <see cref="String.LastIndexOfAny(char[])"/>, которая, в отличие от оригинала,
    /// проверяемые символы берет из строки, а не из массива.
    /// Функция возвращает индекс последнего символа из строки <paramref name="str"/>, который присутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых нет в строке <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int LastIndexOfAny(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str) || searchChars.Count == 0)
        return -1;
      for (int i = str.Length - 1; i >= 0; i--)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// Функция, обратная к <see cref="LastIndexOfAny(string, string)"/>.
    /// Функция возвращает индекс последнего символа из строки <paramref name="str"/>, который отсутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// входящих в строку <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если строка <paramref name="str"/> непустая, а <paramref name="searchChars"/> - пустая, то возвращается значение (<paramref name="str"/>.Length-1), т.к.
    /// первый же символ не входит в список.
    /// В качестве строки <paramref name="searchChars"/> следует передавать список всех допустимых символов и
    /// сообщать об ошибке, если функция вернула значение, большее или равное 0.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int LastIndexOfAnyOther(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (String.IsNullOrEmpty(searchChars))
        return str.Length - 1; // любой символ подходит

      if (PreferCharArrayIndexer(str, searchChars))
        return LastIndexOfAnyOther(str, new CharArrayIndexer(searchChars));

      for (int i = str.Length - 1; i >= 0; i--)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Функция, обратная к <see cref="LastIndexOfAny(string, CharArrayIndexer)"/>.
    /// Функция возвращает индекс первого символа из строки <paramref name="str"/>, который отсутствует
    /// в строке <paramref name="searchChars"/>. Если строка <paramref name="str"/> пустая или состоит исключительно из символов,
    /// которых входящих в строку <paramref name="searchChars"/>, то возвращается (-1).
    /// Поиск является культуро-независимым, все символы сравниваются по их кодам
    /// отдельно друг от друга.
    /// Если строка <paramref name="str"/> непустая, а <paramref name="searchChars"/> - пустая, то возвращается значение (<paramref name="str"/>.Length-1), т.к.
    /// первый же символ не входит в список.
    /// В качестве строки <paramref name="searchChars"/> следует передавать список всех допустимых символов и
    /// сообщать об ошибке, если функция вернула значение, большее или равное 0.
    /// </summary>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="searchChars">Искомые символы</param>
    /// <returns>Индекс в строке <paramref name="str"/></returns>
    public static int LastIndexOfAnyOther(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (searchChars.Count == 0)
        return str.Length - 1; // любой символ подходит
      for (int i = str.Length - 1; i >= 0; i--)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }

    #endregion

    #region IndexOfOccurence, LastIndexOfOccurence

    /// <summary>
    /// Найти позицию заданного вхождения подстроки <paramref name="value"/> в строку <paramref name="str"/>.
    /// Если <paramref name="occurence"/>=0, то вызов аналогичен <see cref="String.IndexOf(string)"/>.
    /// Если <paramref name="value"/> - пустая строка, то возвращается 0.
    /// Если строка не содержит нужного количества вхождений, возвращается (-1).
    ///    
    /// Обрабатывается перекрытие вхождений. Например, если <paramref name="str"/>="ABABABA", <paramref name="value"/>="ABA",
    /// <paramref name="occurence"/>=1, то возвращается 4, а не 2.
    /// </summary>
    /// <param name="str">Строка, в которой выполняется поиск</param>
    /// <param name="value">Искомая подстрока</param>
    /// <param name="occurence">Номер вхождения. Нумерация вхождений начинается с 0</param>
    /// <param name="comparisonType">Режим сравнения строк</param>
    /// <returns>Найденная позиция</returns>
    public static int IndexOfOccurence(string str, string value, int occurence, StringComparison comparisonType)
    {
      if (occurence < 0)
        throw new ArgumentOutOfRangeException("occurence");

      if (String.IsNullOrEmpty(value))
        return 0;
      if (String.IsNullOrEmpty(str))
        return -1;

      int currPos = 0;
      int count = 0;
      while (currPos < str.Length)
      {
        int p = str.IndexOf(value, currPos, str.Length - currPos, comparisonType);
        if (p < 0)
          return -1;
        if (count == occurence)
          return p;

        count++;
        currPos = p + value.Length;
      }

      return -1;
    }


    /// <summary>
    /// Найти позицию заданного вхождения подстроки <paramref name="value"/> в строку <paramref name="str"/>, начиная просмотр с конца строки.
    /// Если длина подстроки больше 1, то возвращается позиция первого символа найденного вхождения.
    /// Если <paramref name="occurence"/>=0, то вызов аналогичен <see cref="String.LastIndexOf(string)"/>.
    /// Если <paramref name="value"/> - пустая строка, то возвращается 0.
    /// Если строка не содержит нужного количества вхождений, возвращается (-1).
    ///    
    /// Обрабатывается перекрытие вхождений. Например, если <paramref name="str"/>="ABABABA", <paramref name="value"/>="ABA",
    /// <paramref name="occurence"/>=1, то возвращается 4, а не 2.
    /// </summary>
    /// <param name="str">Строка, в которой выполняется поиск</param>
    /// <param name="value">Искомая подстрока</param>
    /// <param name="occurence">Номер вхождения. Нумерация вхождений начинается с 0</param>
    /// <param name="comparisonType">Режим сравнения строк</param>
    /// <returns>Найденная позиция</returns>
    public static int LastIndexOfOccurence(string str, string value, int occurence, StringComparison comparisonType)
    {
      if (occurence < 0)
        throw new ArgumentOutOfRangeException("occurence");

      if (String.IsNullOrEmpty(value))
        return 0;
      if (String.IsNullOrEmpty(str))
        return -1;

      int currPos = str.Length - 1;
      int count = 0;
      while (currPos >= 0)
      {
        int p = str.LastIndexOf(value, currPos, currPos + 1, comparisonType);
        if (p < 0)
          return -1;
        if (count == occurence)
          return p;

        count++;
        currPos = p - 1;// value.Length;
      }

      return -1;
    }

    #endregion

    #region ReplaceXXX

    /// <summary>
    /// Замена символов по словарю
    /// </summary>
    /// <param name="str">Строка, в которой производится замена</param>
    /// <param name="replaceDict">Словарь для замены. Ключами являются заменяемые символы, значениями - замещающие</param>
    /// <returns>Строка с замененными символами</returns>
    public static string ReplaceChars(string str, Dictionary<char, char> replaceDict)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

#if DEBUG
      if (replaceDict == null)
        throw new ArgumentNullException("Dict");
#endif
      if (replaceDict.Count == 0)
        return str;

      StringBuilder sb = null; // потом создадим
      for (int i = 0; i < str.Length; i++)
      {
        char ResChar;
        if (replaceDict.TryGetValue(str[i], out ResChar))
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = ResChar; // заменяем
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Замена символов.
    /// При многократных вызовах рекомендуется использовать перегрузку метода, принимающую аргумент
    /// типа <see cref="Dictionary{Char, Char}"/>.
    /// </summary>
    /// <param name="str">Строка, в которой производится замена</param>
    /// <param name="searchChars">Заменяемые символы</param>
    /// <param name="replaceChars">Замещающие символы</param>
    /// <returns>Строка с замененными символами</returns>
    public static string ReplaceChars(string str, string searchChars, string replaceChars)
    {
      if (replaceChars.Length != searchChars.Length)
        throw new ArgumentException("Длина строк SearchChars и replaceChars должна быть одинаковой", "replaceChars");

      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Length == 1)
        return str.Replace(searchChars[0], replaceChars[0]);

      // Замена по словарю
      Dictionary<char, char> dict = new Dictionary<char, char>(searchChars.Length);
      for (int i = 0; i < searchChars.Length; i++)
        dict.Add(searchChars[i], replaceChars[i]);
      return ReplaceChars(str, dict);
    }

    /// <summary>
    /// Замена диапазона символов в строке на заданный символ.
    /// Диапазон символов определяется их кодами в таблице Unicode.
    /// </summary>
    /// <param name="str">Строка, в которой производится замена</param>
    /// <param name="firstChar">Первый символ заменяемого диапазона</param>
    /// <param name="lastChar">Последний символ заменяемого диапазона</param>
    /// <param name="replaceChar">Заменяющий символ</param>
    /// <returns>Исходная строка с выполненной заменой</returns>
    public static string ReplaceCharRange(string str, char firstChar, char lastChar, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      StringBuilder sb = null; // потом создадим
      for (int i = 0; i < str.Length; i++)
      {
        if (str[i] >= firstChar && str[i] <= lastChar)
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = replaceChar; // заменяем
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Выполняет замену любого из заданных символов <paramref name="searchChars"/> в строке <paramref name="str"/> на символ <paramref name="replaceChar"/>
    /// Все символы заменяются на один и тот же символ.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Строка, в которой осуществляется поиск</param>
    /// <param name="searchChars">Символы, подлежащие замене</param>
    /// <param name="replaceChar">Заменяющий символ</param>
    /// <returns>Строка с выполненными заменами</returns>
    public static string ReplaceAny(string str, string searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (String.IsNullOrEmpty(searchChars))
        return str;

      if (PreferCharArrayIndexer(str, searchChars))
        return ReplaceAny(str, new CharArrayIndexer(searchChars), replaceChar);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Выполняет замену любого из заданных символов <paramref name="searchChars"/> в строке <paramref name="str"/> на символ <paramref name="replaceChar"/>
    /// Все символы заменяются на один и тот же символ.
    /// </summary>
    /// <param name="str">Строка, в которой осуществляется поиск</param>
    /// <param name="searchChars">Символы, подлежащие замене</param>
    /// <param name="replaceChar">Заменяющий символ</param>
    /// <returns>Строка с выполненными заменами</returns>
    public static string ReplaceAny(string str, CharArrayIndexer searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Count == 0)
        return str;

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Выполняет замену всех символов, не входящих в заданный список <paramref name="searchChars"/> в строке <paramref name="str"/> 
    /// на символ <paramref name="replaceChar"/>.
    /// Все символы заменяются на один и тот же символ.
    /// Если требуется многократный вызов с одним и тем же набором символов <paramref name="searchChars"/>,
    /// создайте <see cref="CharArrayIndexer"/> и используйте другую перегрузку метода.
    /// </summary>
    /// <param name="str">Строка, в которой осуществляется поиск</param>
    /// <param name="searchChars">Символы, которые должны остаться в строке</param>
    /// <param name="replaceChar">Заменяющий символ</param>
    /// <returns>Строка с выполненными заменами</returns>
    public static string ReplaceAnyOther(string str, string searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (String.IsNullOrEmpty(searchChars))
        return new string(replaceChar, str.Length);

      if (PreferCharArrayIndexer(str, searchChars))
        return ReplaceAnyOther(str, new CharArrayIndexer(searchChars), replaceChar);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Выполняет замену всех символов, не входящих в заданный список <paramref name="searchChars"/> в строке <paramref name="str"/> 
    /// на символ <paramref name="replaceChar"/>.
    /// Все символы заменяются на один и тот же символ.
    /// </summary>
    /// <param name="str">Строка, в которой осуществляется поиск</param>
    /// <param name="searchChars">Символы, которые должны остаться в строке</param>
    /// <param name="replaceChar">Заменяющий символ</param>
    /// <returns>Строка с выполненными заменами</returns>
    public static string ReplaceAnyOther(string str, CharArrayIndexer searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Count == 0)
        return new string(replaceChar, str.Length);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // полная копия строки
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    #endregion

    #region RemoveChars, RemoveOtherChars

    /// <summary>
    /// Удаляет из строки <paramref name="str"/> все символы, входящие в список <paramref name="removedChars"/>.
    /// Если метод должен многократно вызываться для одного и того же набора символов <paramref name="removedChars"/>,
    /// рекомендуется однократно создать <see cref="CharArrayIndexer"/> и использовать другую перегрузку метода.
    /// </summary>
    /// <param name="str">Исходная строка</param>
    /// <param name="removedChars">Cимволы, которые могут оставаться в строке</param>
    /// <returns>Строка <paramref name="str"/> или строка меньшей длины</returns>
    public static string RemoveChars(string str, string removedChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (String.IsNullOrEmpty(removedChars))
        //return String.Empty;
        return str; // 14.07.2021

      if (PreferCharArrayIndexer(str, removedChars))
        return RemoveChars(str, new CharArrayIndexer(removedChars));

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (removedChars.IndexOf(str[i]) >= 0)
        {
          // Символ удаляется

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // все предыдущие символы
          }
        }
        else
        {
          // Символ добавляется в строку

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Удаляет из строки <paramref name="str"/> все символы, входящие в список <paramref name="removedChars"/>.
    /// </summary>
    /// <param name="str">Исходная строка</param>
    /// <param name="removedChars">Cимволы, которые могут оставаться в строке</param>
    /// <returns>Строка <paramref name="str"/> или строка меньшей длины</returns>
    public static string RemoveChars(string str, CharArrayIndexer removedChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (removedChars.Count == 0)
        //return String.Empty;
        return str; // 14.07.2021

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (removedChars.Contains(str[i]))
        {
          // Символ удаляется

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // все предыдущие символы
          }
        }
        else
        {
          // Символ добавляется в строку

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Удаляет из строки <paramref name="str"/> все символы, кроме входящих в список <paramref name="validChars"/>.
    /// Если метод должен многократно вызываться для одного и того же набора символов <paramref name="validChars"/>,
    /// рекомендуется однократно создать <see cref="CharArrayIndexer"/> и использовать другую перегрузку метода.
    /// </summary>
    /// <param name="str">Исходная строка</param>
    /// <param name="validChars">Cимволы, которые могут оставаться в строке</param>
    /// <returns>Строка <paramref name="str"/> или строка меньшей длины</returns>
    public static string RemoveOtherChars(string str, string validChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (String.IsNullOrEmpty(validChars))
        //return str; 
        // 14.07.2021
        return String.Empty; // нет символов которые можн было бы оставить

      if (PreferCharArrayIndexer(str, validChars))
        return RemoveOtherChars(str, new CharArrayIndexer(validChars));

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (validChars.IndexOf(str[i]) < 0)
        {
          // Символ удаляется

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // все предыдущие символы
          }
        }
        else
        {
          // Символ добавляется в строку

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Удаляет из строки <paramref name="str"/> все символы, кроме входящих в список <paramref name="validChars"/>.
    /// </summary>
    /// <param name="str">Исходная строка</param>
    /// <param name="validChars">Cимволы, которые могут оставаться в строке</param>
    /// <returns>Строка <paramref name="str"/> или строка меньшей длины</returns>
    public static string RemoveOtherChars(string str, CharArrayIndexer validChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (validChars.Count == 0)
        //return str; 
        // 14.07.2021
        return String.Empty; // нет символов которые можн было бы оставить

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (!validChars.Contains(str[i]))
        {
          // Символ удаляется

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // все предыдущие символы
          }
        }
        else
        {
          // Символ добавляется в строку

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    #endregion

    #region Удаление повторов символов

    /// <summary>
    /// Выполняет поиск в строке двойных и более вхождений символов <paramref name="searchChar"/> в строку <paramref name="str"/>.
    /// Такие последовательности заменяются на один символ.
    /// Если двойных символов не найдено, возвращается исходная строка.
    /// В отличие от стандартного метода <see cref="String.Replace(string, string)"/>, заменяются последовательности из трех и символов и длиннее.
    /// </summary>
    /// <param name="str">Строка для поиска</param>
    /// <param name="searchChar">Символ для поиска. Чаще всего - пробел</param>
    /// <returns>Строка с удаленными повторами</returns>
    public static string RemoveDoubleChars(string str, char searchChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      int p = str.IndexOf(new string(searchChar, 2), StringComparison.Ordinal);
      if (p < 0)
        return str;

      StringBuilder sb = new StringBuilder(str.Length - 1);
      sb.Append(str, 0, p + 1); // включая первый из двух символов в паре
      for (int i = p + 1; i < str.Length; i++)
      {
        char LastChar = sb[sb.Length - 1];
        if (LastChar == searchChar)
        {
          if (str[i] != LastChar)
            sb.Append(str[i]);
        }
        else
          sb.Append(str[i]);
      }
      return sb.ToString();
    }

    #endregion

    #region Поиск без учета регистра

    /// <summary>
    /// Поиск строки в массиве с возможностью игнорировать верхний / нижний регистр.
    /// Для сравнения используется функция <see cref="String.Equals(string, string, StringComparison)"/>.
    /// </summary>
    /// <param name="a">Массив, в котором выполняется поиск</param>
    /// <param name="searchStr">Искомая строка</param>
    /// <param name="comparisonType">Способ сравнения строк</param>
    /// <returns>Индекс найденной строки в массиве или (-1)</returns>
    public static int IndexOf(string[] a, string searchStr, StringComparison comparisonType)
    {
      for (int i = 0; i < a.Length; i++)
      {
        if (String.Equals(a[i], searchStr, comparisonType))
          return i;
      }
      return -1;
    }

    #endregion

    #region StrToCSharpString

    /// <summary>
    /// Преобразует строку в стиль C#.
    /// 1. Окружает строку кавычками.
    /// 2. Заменяет управляющие символы, апострофы и кавычки на Escape-последовательности.
    /// Может быть, есть какая-нибудь готовая функция?
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Строка в кавычках</returns>
    public static string StrToCSharpString(string s)
    {
      if (s == null)
        return "null";
      StringBuilder sb = new StringBuilder(s.Length + 2);
      sb.Append('\"');
      for (int i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case '\t': sb.Append(@"\t"); break;
          case '\r': sb.Append(@"\r"); break;
          case '\n': sb.Append(@"\n"); break;
          case '\\': sb.Append(@"\\"); break;
          case '\"': sb.Append(@"\"""); break;
          case '\'': sb.Append(@"\'"); break;
          default:
            if (s[i] < ' ')
            {
              sb.Append('\\');
              sb.Append((int)(s[i]));
            }
            else
              sb.Append(s[i]); // как есть
            break;
        }
      }
      sb.Append('\"');
      return sb.ToString();
    }

    #endregion

    /// <summary>
    /// Получение подстроки.
    /// В отличие от стандартного метода <see cref="String.Substring(int, int)"/>, не выбрасывается исключение, если заданы неправильные аргументы.
    /// При этом возвращаемая подстрока может оказаться короче, чем <paramref name="length"/>.
    /// </summary>
    /// <param name="s">Исходная строка. Если null, то возвращается пустая строка</param>
    /// <param name="start">Начальная позиция в строке. Нумерация начинается с 0. Если больше, чем длина строки <paramref name="s"/>,
    /// то возвращается пустая строка. Если задано отрицательное значение, то заменяется на 0 с соответствующим уменьшением аргумента <paramref name="length"/>.</param>
    /// <param name="length">Длина подстроки. Если длина строки <paramref name="s"/> недостаточна для заданной позиции и длины подстроки, возвращается более короткое значение или пустая строка.
    /// Если задано нулевое или отрицательное значение, возвращается пустая строка</param>
    /// <returns>Подстрока</returns>
    public static string Substring(string s, int start, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (start < 0)
      {
        length += start;
        start = 0;
      }
      if (start + length > s.Length)
        length = s.Length - start;
      if (length <= 0)
        return String.Empty;
      else
        return s.Substring(start, length);
    }

    #region IsSubstring

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> содержит подстроку <paramref name="substring"/>,
    /// начиная с позиции <paramref name="startPos"/>.
    /// В отличие от <see cref="String.Compare(string, int, string, int, int, StringComparison)"/>, не выбрасывает исключение, если запрошенная позиция подстроки 
    /// выходит за пределы строки <paramref name="s"/>. 
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="startPos">Позиция начала подстроки для проверки. Может быть отрицательной или выходить
    /// за пределы строки</param>
    /// <param name="substring">Проверяемая подстрока</param>
    /// <param name="comparisonType">Способ сравнения строк</param>
    /// <returns>Совпадение подстроки</returns>
    public static bool IsSubstring(string s, int startPos, string substring, StringComparison comparisonType)
    {
      if (s == null)
        s = String.Empty;
      if (substring == null)
        substring = String.Empty;

      if (startPos < 0 || (startPos + substring.Length) > s.Length)
        return false;

      return String.Compare(s, startPos, substring, 0, substring.Length, comparisonType) == 0; // TODO: Equals()
    }

    /// <summary>
    /// Возвращает true, если строка <paramref name="s"/> содержит подстроку <paramref name="substring"/>,
    /// начиная с позиции <paramref name="startPos"/>.
    /// В отличие от String.Compare(), не выбрасывает исключение, если запрошенная позиция подстроки 
    /// выходит за пределы строки <paramref name="s"/>. 
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="startPos">Позиция начала подстроки для проверки. Может быть отрицательной или выходить
    /// за пределы строки</param>
    /// <param name="substring">Проверяемая подстрока</param>
    /// <param name="ignoreCase">Если true, то регистр символов игнорируется</param>
    /// <param name="culture">Информация о культуре, используемой при сравнении строк</param>
    /// <returns>Совпадение подстроки</returns>
    public static bool IsSubstring(string s, int startPos, string substring, bool ignoreCase, CultureInfo culture)
    {
      if (s == null)
        s = String.Empty;
      if (substring == null)
        substring = String.Empty;

      if (startPos < 0 || (startPos + substring.Length) > s.Length)
        return false;

      return String.Compare(s, startPos, substring, 0, substring.Length, ignoreCase, culture) == 0; // TODO: Equals()
    }

    #endregion

    #region Константы для String.Split()

    /// <summary>
    /// Массив из одной строки, содержащей пару символов CR+LF, независимо от операционной системы.
    /// Для использования в методе <see cref="String.Split(string[], StringSplitOptions)"/>.
    /// </summary>
    public static readonly string[] CRLFSeparators = new string[] { "\r\n" };

    /// <summary>
    /// Массив из одной строки, содержащей пару символов CR+LF, или другие символы, в зависимости от операционной системы.
    /// Строка содержит символы из <see cref="Environment.NewLine"/>.
    /// Для использования в методе <see cref="String.Split(string[], StringSplitOptions)"/>.
    /// </summary>
    public static readonly string[] NewLineSeparators = new string[] { Environment.NewLine };

    /// <summary>
    /// Массив из четырех строк, содержащий возможные комбинации символов CR и LF.
    /// Двусимвольные комбинации идут перед односимвольными.
    /// Для использования в методе <see cref="String.Split(string[], StringSplitOptions)"/>.
    /// </summary>
    public static readonly string[] AllPossibleLineSeparators = new string[] { "\r\n", "\n\r", "\r", "\n" };

    #endregion

    #region GetNewLineSeparators

    /// <summary>
    /// Определение символов, которые используются для перевода строки.
    /// Возвращает одну из строк в <see cref="AllPossibleLineSeparators"/>.
    /// Если строка пустая или не содержит символа перевода строк, то возвращается пустая строка.
    /// В этом случае обычно следует предполагать <see cref="Environment.NewLine"/>.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <returns>Символы перевода строки</returns>
    public static string GetNewLineSeparators(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        if (s.Contains(AllPossibleLineSeparators[i]))
          return AllPossibleLineSeparators[i];
      }

      return String.Empty;
    }

    #endregion

    #region TrimStart/EndNewLineSeparators

    /// <summary>
    /// Если строка начинается с символов конца строки, то они удаляются.
    /// Символами конца строки считаются любые комбинации из <see cref="AllPossibleLineSeparators"/> (пары или одиночные символы).
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="trimAll">Если true, то удаляются все пары символов в начале строки, иначе удаляется не более одной пары</param>
    /// <returns>Укороченная или исходная строка</returns>
    public static string TrimStartNewLineSeparators(string s, bool trimAll)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        bool flag = false;
        int st = 0;
        while (IsSubstring(s, st, AllPossibleLineSeparators[i], StringComparison.Ordinal))
        {
          flag = true;
          st += AllPossibleLineSeparators[i].Length;
          if (!trimAll)
            break;
        }
        if (flag)
          return s.Substring(st);
      }
      // нет разделителя
      return s;
    }
    /// <summary>
    /// Если строка начинается с символов конца строки, то они удаляются.
    /// Символами конца строки считается только заданная комбинация символов <paramref name="newLine"/>.
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="trimAll">Если true, то удаляются все пары символов в начале строки, иначе удаляется не более одной пары</param>
    /// <param name="newLine">Удаляемая комбинация символов. Используйте <see cref="Environment.NewLine"/>. Не может быть пустой строкой</param>
    /// <returns>Укороченная или исходная строка</returns>
    public static string TrimStartNewLineSeparators(string s, bool trimAll, string newLine)
    {
      if (String.IsNullOrEmpty(newLine))
        throw new ArgumentNullException("newLine");

      if (String.IsNullOrEmpty(s))
        return String.Empty;
      bool flag = false;
      int st = 0;
      while (IsSubstring(s, st, newLine, StringComparison.Ordinal))
      {
        flag = true;
        st += newLine.Length;
        if (!trimAll)
          break;
      }
      if (flag)
        return s.Substring(st);
      // нет разделителя
      return s;
    }

    /// <summary>
    /// Если строка заканчивается символами конца строки, то они удаляются.
    /// Символами конца строки считаются любые комбинации из <see cref="AllPossibleLineSeparators"/> (пары или одиночные символы).
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="trimAll">Если true, то удаляются все пары символов в конце строки, иначе удаляется не более одной пары</param>
    /// <returns>Укороченная или исходная строка</returns>
    public static string TrimEndNewLineSeparators(string s, bool trimAll)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        bool flag = false;
        int l = s.Length;
        while (IsSubstring(s, l - AllPossibleLineSeparators[i].Length, AllPossibleLineSeparators[i], StringComparison.Ordinal))
        {
          flag = true;
          l -= AllPossibleLineSeparators[i].Length;
          if (!trimAll)
            break;
        }
        if (flag)
          return s.Substring(0, l);
      }
      // нет разделителя
      return s;
    }

    /// <summary>
    /// Если строка заканчивается символами конца строки, то они удаляются.
    /// Символами конца строки считается только заданная комбинация символов <paramref name="newLine"/>.
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <param name="trimAll">Если true, то удаляются все пары символов в конце строки, иначе удаляется не более одной пары</param>
    /// <param name="newLine">Удаляемая комбинация символов. Используйте <see cref="Environment.NewLine"/>. Не может быть пустой строкой</param>
    /// <returns>Укороченная или исходная строка</returns>
    public static string TrimEndNewLineSeparators(string s, bool trimAll, string newLine)
    {
      if (String.IsNullOrEmpty(newLine))
        throw new ArgumentNullException("newLine");

      if (String.IsNullOrEmpty(s))
        return String.Empty;
      bool flag = false;
      int l = s.Length;
      while (IsSubstring(s, l - newLine.Length, newLine, StringComparison.Ordinal))
      {
        flag = true;
        l -= newLine.Length;
        if (!trimAll)
          break;
      }
      if (flag)
        return s.Substring(0, l);
      // нет разделителя
      return s;
    }

    #endregion

    #region GetCharCount

    /// <summary>
    /// Подсчитывает количество вхождений символа <paramref name="searchChar"/> в строку <paramref name="s"/>.
    /// Если строка пустая или null, возвращается 0
    /// </summary>
    /// <param name="s">Строка, в которой выполняется поиск</param>
    /// <param name="searchChar">Искомый символ</param>
    /// <returns>Количество вхождений</returns>
    public static int GetCharCount(string s, char searchChar)
    {
      if (String.IsNullOrEmpty(s))
        return 0;
      int cnt = 0;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == searchChar)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region JoinNotEmptyStrings()

    /// <summary>
    /// Вызывает <see cref="String.Join(string, string[])"/> для непустых строк в массиве <paramref name="values"/>.
    /// Если массив пустой или не содержит непустых строк, возвращается <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="separator">Разделитель</param>
    /// <param name="values">Объединяемые значения</param>
    /// <returns>Объединенная строка</returns>
    public static string JoinNotEmptyStrings(string separator, string[] values)
    {
      // На первом проходе определяем наличие данных
      int nItems = 0;
      for (int i = 0; i < values.Length; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
          nItems++;
      }
      if (nItems == 0)
        return String.Empty;
      if (nItems == values.Length) // все элементы непустые
        return String.Join(separator, values);

      // Часть строк пустые, часть - нет
      string[] a = new string[nItems];
      nItems = 0;
      for (int i = 0; i < values.Length; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
        {
          a[nItems] = values[i];
          nItems++;
        }
      }
      return String.Join(separator, a);
    }

    /// <summary>
    /// Вызывает <see cref="String.Join(string, string[])"/> для непустых строк в списке <paramref name="values"/>.
    /// Если список пустой или не содержит непустых строк, возвращается <see cref="String.Empty"/>
    /// </summary>
    /// <param name="separator">Разделитель</param>
    /// <param name="values">Объединяемые значения</param>
    /// <returns>Объединенная строка</returns>
    public static string JoinNotEmptyStrings(string separator, IList<string> values)
    {
      // На первом проходе определяем наличие данных
      int nItems = 0;
      for (int i = 0; i < values.Count; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
          nItems++;
      }
      if (nItems == 0)
        return String.Empty;

      // Часть строк пустые, часть - нет
      string[] a = new string[nItems];
      if (nItems == values.Count) // все элементы непустые
        values.CopyTo(a, 0);
      else
      {
        nItems = 0;
        for (int i = 0; i < values.Count; i++)
        {
          if (!String.IsNullOrEmpty(values[i]))
          {
            a[nItems] = values[i];
            nItems++;
          }
        }
      }
      return String.Join(separator, a);
    }


    /// <summary>
    /// Вызывает <see cref="String.Join(string, string[])"/> для непустых строк в списке <paramref name="values"/>.
    /// Если список пустой или не содержит непустых строк, возвращается <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="separator">Разделитель</param>
    /// <param name="values">Объединяемые значения</param>
    /// <returns>Объединенная строка</returns>
    public static string JoinNotEmptyStrings(string separator, IEnumerable<string> values)
    {
      // Эта версия является однопроходной.
      List<string> lst = new List<string>();
      foreach (string s in values)
      {
        if (!String.IsNullOrEmpty(s))
          lst.Add(s);
      }

      switch (lst.Count)
      {
        case 0:
          return String.Empty;
        case 1:
          return lst[0];
        default:
          return String.Join(separator, lst.ToArray());
      }
    }

    #endregion

    #region Вызовы ToString() для массивов и перечислений

    #region ToStringArray()

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Полученные строки возвращаются в виде массива.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то массив не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="a">Массив для перечисления</param>
    /// <returns>Массив строк</returns>
    public static string[] ToStringArray<T>(T[] a)
    {
      string[] res = new string[a.Length];
      for (int i = 0; i < res.Length; i++)
        res[i] = a[i].ToString();
      return res;
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Полученные строки возвращаются в виде массива.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то список не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="lst">Список для перечисления</param>
    /// <returns>Массив строк</returns>
    public static string[] ToStringArray<T>(IList<T> lst)
    {
      string[] res = new string[lst.Count];
      for (int i = 0; i < res.Length; i++)
        res[i] = lst[i].ToString();
      return res;
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Полученные строки возвращаются в виде массива.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то список не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="en">Перечислимый объект</param>
    /// <returns>Массив строк</returns>
    public static string[] ToStringArray<T>(IEnumerable<T> en)
    {
      List<string> lst = null;
      foreach (T x in en)
      {
        if (lst == null)
          lst = new List<string>();
        lst.Add(x.ToString());
      }

      if (lst == null)
        return EmptyStrings;
      else
        return lst.ToArray();
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Полученные строки возвращаются в виде массива.
    /// Список не может содержать значения null.
    /// </summary>
    /// <param name="lst">Список для перечисления</param>
    /// <returns>Массив строк</returns>
    public static string[] ToStringArray(IList lst)
    {
      if (lst.Count == 0)
        return EmptyStrings;

      string[] res = new string[lst.Count];
      for (int i = 0; i < res.Length; i++)
        res[i] = lst[i].ToString();
      return res;
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Полученные строки возвращаются в виде массива.
    /// Список не может содержать значения null.
    /// </summary>
    /// <param name="en">Перечислимый объект</param>
    /// <returns>Массив строк</returns>
    public static string[] ToStringArray(IEnumerable en)
    {
      List<string> lst = null;
      foreach (object x in en)
      {
        if (lst == null)
          lst = new List<string>();
        lst.Add(x.ToString());
      }

      if (lst == null)
        return EmptyStrings;
      else
        return lst.ToArray();
    }

    #endregion

    #region ToStringJoin()

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Возвращается объединенная строка с заданными разделителями <paramref name="separator"/>.
    /// Эквивалентно вызову String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="a"/>)), но является более экономичным.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то массив не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="separator">Разделитель между элементами (аналогично применяемому в методе <see cref="String.Join(string, string[])"/>)</param>
    /// <param name="a">Массив для перечисления</param>
    /// <returns>Объединенная строка</returns>
    public static string ToStringJoin<T>(string separator, T[] a)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(a[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Возвращается объединенная строка с заданными разделителями <paramref name="separator"/>.
    /// Эквивалентно вызову String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="lst"/>)), но является более экономичным.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то список не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="separator">Разделитель между элементами (аналогично применяемому в методе <see cref="String.Join(string, string[])"/>)</param>
    /// <param name="lst">Список для перечисления</param>
    /// <returns>Объединенная строка</returns>
    public static string ToStringJoin<T>(string separator, IList<T> lst)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < lst.Count; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(lst[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Возвращается объединенная строка с заданными разделителями <paramref name="separator"/>.
    /// Эквивалентно вызову String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="en"/>)), но является более экономичным.
    /// Если <paramtyperef name="T"/> является классом, а не значимым типом, то список не может содержать значения null.
    /// </summary>
    /// <typeparam name="T">Тип перебираемых элементов</typeparam>
    /// <param name="separator">Разделитель между элементами (аналогично применяемому в методе <see cref="String.Join(string, string[])"/>)</param>
    /// <param name="en">Перечислимый объект</param>
    /// <returns>Объединенная строка</returns>
    public static string ToStringJoin<T>(string separator, IEnumerable<T> en)
    {
      //StringBuilder sb = new StringBuilder();
      StringBuilder sb = null; // исправлено 14.07.2021
      foreach (T x in en)
      {
        if (sb == null)
          sb = new StringBuilder();
        else
          sb.Append(separator);
        sb.Append(x.ToString());
      }

      if (sb == null)
        return String.Empty;
      else
        return sb.ToString();
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Возвращается объединенная строка с заданными разделителями <paramref name="separator"/>.
    /// Эквивалентно вызову String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="lst"/>)), но является более экономичным.
    /// Список не может содержать значения null.
    /// </summary>
    /// <param name="separator">Разделитель между элементами (аналогично применяемому в методе <see cref="String.Join(string, string[])"/>)</param>
    /// <param name="lst">Список для перечисления</param>
    /// <returns>Объединенная строка</returns>
    public static string ToStringJoin(string separator, IList lst)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < lst.Count; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(lst[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// Выполняет перечисление элементов и для каждого из них вызывается метод <see cref="Object.ToString()"/>.
    /// Возвращается объединенная строка с заданными разделителями <paramref name="separator"/>.
    /// Эквивалентно вызову String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="en"/>)), но является более экономичным.
    /// Список не может содержать значения null.
    /// </summary>
    /// <param name="separator">Разделитель между элементами (аналогично применяемому в методе <see cref="String.Join(string, string[])"/>)</param>
    /// <param name="en">Перечислимый объект</param>
    /// <returns>Объединенная строка</returns>
    public static string ToStringJoin(string separator, IEnumerable en)
    {
      //StringBuilder sb = new StringBuilder();
      StringBuilder sb = null; // исправлено 14.07.2021
      foreach (object x in en)
      {
        if (sb == null)
          sb = new StringBuilder();
        else
          sb.Append(separator);
        sb.Append(x.ToString());
      }

      if (sb == null)
        return String.Empty;
      else
        return sb.ToString();
    }

    #endregion

    #endregion

    internal static bool GetIgnoreCase(StringComparer comparer)
    {
      if (comparer == null)
        return false;
      else
        return comparer.Equals("A", "a");
    }

    #endregion

    #region Удаление строк и столбцов из массива

    /// <summary>
    /// Удаление пустых строк из двухмерного массива строк.
    /// Возвращается массив, число столбцов в котором (второй индекс) совпадает с
    /// числом строк в исходном массиве, а число строк может быть меньше.
    /// Если исходный массив не содержит ни одного значения, возвращается null.
    /// Если пустых строк нет, то возвращается исходный массив
    /// </summary>
    /// <param name="a">Исходный двумерный массив строк</param>
    /// <returns>Преобразованный массив</returns>
    public static string[,] RemoveEmptyRows(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] RowFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
            RowFlags[i] = true;
        }
      }

      int nRows2 = 0;
      int[] rowRefs = new int[nRows];
      for (int i = 0; i < nRows; i++)
      {
        if (RowFlags[i])
        {
          rowRefs[i] = nRows2;
          nRows2++;
        }
        else
          rowRefs[i] = -1;
      }

      if (nRows2 == 0)
        return null;
      if (nRows2 == nRows)
        return a;

      string[,] a2 = new string[nRows2, nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (rowRefs[i] >= 0)
            a2[rowRefs[i], j] = a[i, j];
        }
      }

      return a2;
    }

    /// <summary>
    /// Удаление пустых столбцов из двухмерного массива строк.
    /// Возвращается массив, число строк в котором (первый индекс) совпадает с
    /// числом строк в исходном массиве, а число столбцов может быть меньше.
    /// Если исходный массив не содержит ни одного значения, возвращается null.
    /// Если пустых столбцов нет, то возвращается исходный массив
    /// </summary>
    /// <param name="a">Исходный двумерный массив строк</param>
    /// <returns>Преобразованный массив</returns>
    public static string[,] RemoveEmptyColumns(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] colFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
            colFlags[j] = true;
        }
      }

      int nCols2 = 0;
      int[] colRefs = new int[nCols];
      for (int j = 0; j < nCols; j++)
      {
        if (colFlags[j])
        {
          colRefs[j] = nCols2;
          nCols2++;
        }
        else
          colRefs[j] = -1;
      }

      if (nCols2 == 0)
        return null;
      if (nCols2 == nCols)
        return a;

      string[,] a2 = new string[nRows, nCols2];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (colRefs[j] >= 0)
            a2[i, colRefs[j]] = a[i, j];
        }
      }

      return a2;
    }

    /// <summary>
    /// Удаление пустых строк и столбцов из двухмерного массива строк.
    /// Возвращается массив, число строк и столбцов в котором может быть меньше.
    /// Если исходный массив не содержит ни одного значения, возвращается null.
    /// Если пустых строк и столбцов нет, то возвращается исходный массив.
    /// Эквивалентно последовательному вызову RemoveEmptyColumns(RemoveEmptyRows())
    /// </summary>
    /// <param name="a">Исходный двумерный массив строк</param>
    /// <returns>Преобразованный массив</returns>
    public static string[,] RemoveEmptyRowsAndColumns(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] rowFlags = new bool[nRows];
      bool[] colFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
          {
            rowFlags[i] = true;
            colFlags[j] = true;
          }
        }
      }

      int nRows2 = 0;
      int[] rowRefs = new int[nRows];
      for (int i = 0; i < nRows; i++)
      {
        if (rowFlags[i])
        {
          rowRefs[i] = nRows2;
          nRows2++;
        }
        else
          rowRefs[i] = -1;
      }

      int nCols2 = 0;
      int[] colRefs = new int[nCols];
      for (int j = 0; j < nCols; j++)
      {
        if (colFlags[j])
        {
          colRefs[j] = nCols2;
          nCols2++;
        }
        else
          colRefs[j] = -1;
      }

#if DEBUG
      if ((nRows2 == 0) != (nCols2 == 0))
        throw new BugException("Неправильное определение пустых строк и столбцов. nRows2=" + nRows2.ToString() + ", nCols2=" + nCols2);
#endif

      if (nRows2 == 0 || nCols2 == 0) // вообще-то должны быть обе вместе
        return null;
      if (nRows2 == nRows && nCols2 == nCols) // нет ни пустых строк, ни пустых столбцов
        return a;

      string[,] a2 = new string[nRows2, nCols2];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (rowRefs[i] >= 0 && colRefs[j] >= 0)
            a2[rowRefs[i], colRefs[j]] = a[i, j];
        }
      }

      return a2;
    }

    #endregion

    #region Специальные символы в строке

    /// <summary>
    /// Символ "мягкого" переноса
    /// </summary>
    public const char SoftHyphenChar = '\u00AD';

    /// <summary>
    /// Строка, содержащая единственный символ "мягкого" переноса
    /// </summary>
    public const string SoftHyphenStr = "\u00AD";

    /// <summary>
    /// Символ неразрывного пробела
    /// </summary>
    public const char NonBreakSpaceChar = '\u00A0';

    /// <summary>
    /// Строка, содержащая единственный символ неразрывного пробела
    /// </summary>
    public const string NonBreakSpaceStr = "\u00A0";

    /// <summary>
    /// Удаление символов "мягкого" переноса из строки
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Строка без символов</returns>
    public static string RemoveSoftHyphens(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.IndexOf(SoftHyphenChar) >= 0)
        return s.Replace(SoftHyphenStr, String.Empty);
      else
        return s;
    }

    /// <summary>
    /// Символ открывающей угловой кавычки
    /// </summary>
    public const char LeftDoubleAngleQuotationChar = '\u00AB';

    /// <summary>
    /// Символ открывающей угловой кавычки
    /// </summary>
    public const string LeftDoubleAngleQuotationStr = "\u00AB";

    /// <summary>
    /// Символ закрывающей угловой кавычки
    /// </summary>
    public const char RightDoubleAngleQuotationChar = '\u00BB';

    /// <summary>
    /// Символ закрывающей угловой кавычки
    /// </summary>
    public const string RightDoubleAngleQuotationStr = "\u00BB";

    /// <summary>
    /// Преобразование массива строк, содержащих специальные символы, в одну строку
    /// с заменой символов:
    /// - мягкий перенос заменяется на "^"
    /// - неразрывный пробел заменяется на "_"
    /// Строки, образующие массив, разделяются символом "|"
    /// </summary>
    /// <param name="a">Массив строк, содержащих спецсимволы</param>
    /// <returns>Строка с замененными символами</returns>
    public static string StrFromSpecCharsArray(string[] a)
    {
      if (a == null)
        return String.Empty;
      string s = String.Join("|", a);
      return StrFromSpecCharsStr(s);
    }

    /// <summary>
    /// Преобразование строки, содержащей спецсимволы.
    /// Выполняется замена символов
    /// - мягкий перенос заменяется на "^"
    /// - неразрывный пробел заменяется на "_"
    /// </summary>
    /// <param name="s">Строка, содержащая спецсимволы</param>
    /// <returns>Строка с замененными символами</returns>
    public static string StrFromSpecCharsStr(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      s = s.Replace(NonBreakSpaceChar, '_');
      s = s.Replace(SoftHyphenChar, '^');
      return s;
    }

    /// <summary>
    /// Преобазование строки, содержащей символы "|", "^" и "_" в массив строк.
    /// - строка разбивается на элементы массива по символу "|";
    /// - символ "_" заменяется на неразрывный пробел
    /// - символ "^" заменяется на мягкий перенос
    /// Для пустой строки возвращается пустой массив строк
    /// </summary>
    /// <param name="s">Строка, содержащая символы, которые требуется заменить</param>
    /// <returns>Массив строк со спецсимволами</returns>
    public static string[] StrToSpecCharsArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DataTools.EmptyStrings;
      string[] a = s.Split('|');
      for (int i = 0; i < a.Length; i++)
        a[i] = StrToSpecCharsStr(a[i]);
      return a;
    }

    /// <summary>
    /// Преобазование строки, содержащей символы "^" и "_" в строку со спецсимволами.
    /// - символ "_" заменяется на неразрывный пробел
    /// - символ "^" заменяется на мягкий перенос
    /// </summary>
    /// <param name="s">Строка, содержащая символы, которые требуется заменить</param>
    /// <returns>Строка со спецсимволами</returns>
    public static string StrToSpecCharsStr(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      s = s.Replace('_', NonBreakSpaceChar);
      s = s.Replace('^', SoftHyphenChar);
      return s;
    }

    #endregion
  }
}
