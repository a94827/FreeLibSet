using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Core
{
	partial class DataTools
	{
    #region CommaString

    #region Одна строка

    /// <summary>
    /// Преобразование строки, содержащей значения, разделенные запятыми.
    /// Строка не должна содержать символов переноса строки. Если строка может
    /// содержать несколько строк, используйте CommaStringToArray2().
    /// Концевые пробелы отбрасываются. Также отбрасываются пробелы слева и справа
    /// от каждой запятой (если они не внутри кавычек). Если строка пустая или не содержит ничего, кроме пробелов,
    /// то возвращается null.
    /// Запятые внутри кавычек считаются частью строки. Двойные кавычки заменяются
    /// на одинарные. В случае нарушения структуры CSV вызывается исключение
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив строк-элементов</returns>
    /// <exception cref="FreeLibSet.Core.ParsingException">Нарушение структуры CSV</exception>
    public static string[] CommaStringToArray(string s)
    {
      return CommaStringToArray(s, ',');
    }

    /// <summary>
    /// Преобразование строки, содержащей значения, разделенные запятыми или другим разделителем, например, табуляцией.
    /// Строка не должна содержать символов переноса строки. Если строка может
    /// содержать несколько строк, используйте CommaStringToArray2().
    /// Концевые пробелы отбрасываются. Также отбрасываются пробелы слева и справа
    /// от каждого разделителя (если они не внутри кавычек). Если строка пустая или не содержит ничего, кроме пробелов,
    /// то возвращается null.
    /// Разделители внутри кавычек считаются частью строки. Двойные кавычки заменяются
    /// на одинарные. 
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="fieldDelimiter"></param>
    /// <returns>Массив строк-элементов</returns>
    /// <exception cref="FreeLibSet.Core.ParsingException">Нарушение структуры CSV</exception>
    public static string[] CommaStringToArray(string s, char fieldDelimiter)
    {
      const int PhaseWaitStr = 1;   // ждем начала строки
      const int PhaseInStrNQ = 2;     // начато заполнение строки без кавычек
      const int PhaseInStrQ = 3;     // начато заполнение строки в кавычках
      const int PhaseWaitSep = 4; // закончено заполение строки - ждем запятую

      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim(); // пробелы убираем сразу
      if (s.Length == 0)
        return null;

      if (s.IndexOf(fieldDelimiter) < 0)
      {
        // 10.11.2012
        // Строка не содержит разделителя

        if (s.Length < 3 || s[0] != '\"' || s[s.Length - 1] != '\"')
          return new string[1] { s };
      }

      string FieldDelimiterText;
      switch (fieldDelimiter)
      {
        case '\t': FieldDelimiterText = "<Tab>"; break;
        case ' ': FieldDelimiterText = "<Пробел>"; break;
        default:
          FieldDelimiterText = fieldDelimiter.ToString();
          break;
      }

      List<string> lst = new List<string>();
      StringBuilder sb = new StringBuilder();
      int Phase = PhaseWaitStr;
      for (int i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case ' ':
            switch (Phase)
            {
              case PhaseWaitStr:
              case PhaseWaitSep:
                break;
              case PhaseInStrNQ:
              case PhaseInStrQ:
                sb.Append(' ');
                break;
            }
            break;
          case '"':
            switch (Phase)
            {
              case PhaseWaitStr:
                Phase = PhaseInStrQ;
                break;
              case PhaseWaitSep:
                throw new ParsingException("В позиции " + (i + 1).ToString() + " неожиданный символ \". Ожидался разделитель \"" + FieldDelimiterText + "\"");
              case PhaseInStrNQ:
                // если текст начался не с кавычки, то кавычки в строке не задваиваются
                sb.Append('"');
                break;
              case PhaseInStrQ:
                if (i < (s.Length - 1) && s[i + 1] == '"')
                {
                  sb.Append('"');
                  i++;
                }
                else
                {
                  lst.Add(sb.ToString()); // без обрезки
                  sb.Length = 0;
                  Phase = PhaseWaitSep;
                }
                break;
            }
            break;
          default:
            if (s[i] == fieldDelimiter)
            {
              switch (Phase)
              {
                case PhaseWaitStr:
                  // пустая строка - две запятые подряд
                  lst.Add("");
                  break;
                case PhaseWaitSep:
                  Phase = PhaseWaitStr;
                  break;
                case PhaseInStrNQ:
                  // строка закончена
                  lst.Add(sb.ToString().Trim());
                  sb.Length = 0;
                  Phase = PhaseWaitStr;
                  break;
                case PhaseInStrQ:
                  sb.Append(s[i]);
                  break;
              }
            }
            else // обычный символ
            {
              switch (Phase)
              {
                case PhaseWaitStr:
                  if (s[i] == '\n')
                  {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
                      sb.Append('\r');
                  }
                  sb.Append(s[i]);
                  Phase = PhaseInStrNQ;
                  break;
                case PhaseWaitSep:
                  throw new ParsingException("В позиции " + (i + 1).ToString() + " неожиданный символ \"" + s[i] + "\". Ожидался разделитель \"" + FieldDelimiterText + "\"");
                case PhaseInStrNQ:
                case PhaseInStrQ:
                  if (s[i] == '\n')
                  {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
                      sb.Append('\r');
                  }
                  sb.Append(s[i]);
                  break;
              }
            }
            break;
        }
      } // конец цикла по буквам
      switch (Phase)
      {
        case PhaseInStrNQ:
          lst.Add(sb.ToString().Trim());
          break;
        case PhaseWaitStr:
          lst.Add(String.Empty);
          break;
        case PhaseInStrQ:
          throw new ParsingException("Неожиданный конец строки. Не найдена завершающая кавычка");
      }

      return lst.ToArray();
    }

    private static string[] SimpleCommaStringToArray(string s, char fieldDelimiter)
    {
      string[] a = s.Split(fieldDelimiter);
      for (int i = 0; i < a.Length; i++)
        a[i] = a[i].Trim();
      return a;
    }

    /// <summary>
    /// Преобразование одномерного массива строк в строку с разделителем-запятой.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть запятая или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// </summary>
    /// <param name="a">Массив для преобразования в строку</param>
    /// <returns>CSV-строка</returns>
    public static string CommaStringFromArray(string[] a)
    {
      return CommaStringFromArray(a, ',');
    }

    /// <summary>
    /// Преобразование одномерного массива строк в строку с указанным разделителем.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть символ-разделитель или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// </summary>
    /// <param name="a">Массив для преобразования в строку</param>
    /// <param name="fieldDelimiter">Символ-разделитель</param>
    /// <returns>CSV-строка</returns>
    public static string CommaStringFromArray(string[] a, char fieldDelimiter)
    {
      if (a == null)
        return String.Empty;
      if (a.Length == 0)
        return String.Empty; // 16.11.2016

      StringBuilder sb = new StringBuilder();
      CommaStringFromArray(sb, a, fieldDelimiter);
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование одномерного массива строк в строку с разделителем-запятой.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть запятая или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// Эта версия использует StringBuilder для построения строки.
    /// </summary>
    /// <param name="sb">Сюда записывается CSV-строка</param>
    /// <param name="a">Массив для преобразования в строку</param>
    public static void CommaStringFromArray(StringBuilder sb, string[] a)
    {
      CommaStringFromArray(sb, a, ',');
    }


    /// <summary>
    /// Преобразование одномерного массива строк в строку с указанным разделителем.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть символ-разделитель или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// Эта версия использует StringBuilder для построения строки.
    /// </summary>
    /// <param name="sb">Сюда записывается CSV-строка</param>
    /// <param name="a">Массив для преобразования в строку</param>
    /// <param name="fieldDelimiter">Символ-разделитель</param>
    public static void CommaStringFromArray(StringBuilder sb, string[] a, char fieldDelimiter)
    {
      if (a == null)
        return; // 16.11.2016

      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(fieldDelimiter);

        if (!String.IsNullOrEmpty(a[i]))
        {
          if (a[i].IndexOf(fieldDelimiter) > 0 || a[i].IndexOf('\"') >= 0)
          {
            // в поле есть символ-разделитель
            // Помещаем текст в кавычки, кавычки в тексте удваиваем
            sb.Append('\"');
            if (a[i].IndexOf('\"') >= 0)
            {
              // Есть кавычки для удваивания
              for (int j = 0; j < a[i].Length; j++)
              {
                if (a[i][j] == '\"')
                  sb.Append("\"\"");
                else
                  sb.Append(a[i][j]);
              }
            }
            else
              // Нет кавычек
              sb.Append(a[i]);
            sb.Append('\"');
          }
          else
            // Строка не содержит символа разделителя
            sb.Append(a[i]);
        }
      }
    }

    private static void SimpleCommaStringFromArray(StringBuilder sb, string[] a, char fieldDelimiter)
    {
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(fieldDelimiter);

        if (!String.IsNullOrEmpty(a[i]))
          sb.Append(a[i]);
      }
    }



    /// <summary>
    /// Преобразование строки в массив идентификаторов. Строка должна содержать
    /// список чисел, разделенных запятыми. Пробелы отбрасываются
    /// Возвращается null, если строка пустая или содержит только пробелы
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив чисел или null</returns>
    public static Int32[] CommaStringToIds(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Int32[] Res = new Int32[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = Int32.Parse(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// Получение строки чисел, разделенных запятыми из массива идентификаторов.
    /// Если массив <paramref name="ids"/> пустой или ссылка равна null, возвращается пустая строка.
    /// </summary>
    /// <param name="ids">Массив идентификаторов (может быть null)</param>
    /// <param name="addSpace">Добавить пробелы после запятых</param>
    /// <returns>Строка</returns>
    public static string CommaStringFromIds(Int32[] ids, bool addSpace)
    {
      if (ids == null)
        return String.Empty;
      if (ids.Length == 0)
        return String.Empty;
      if (ids.Length == 1)
        return ids[0].ToString(); // чтобы не создавать объект
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < ids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(ids[i]);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование строки в массив идентификаторов GUID. Строка должна содержать
    /// список идентификаторов GUID, разделенных запятыми. Пробелы отбрасываются
    /// Возвращается null, если строка пустая или содержит только пробелы
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив идентификаторов или null</returns>
    public static Guid[] CommaStringToGuids(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Guid[] Res = new Guid[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = new Guid(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// Получение строки с GUID'ами, разделенных запятыми из массива идентификаторов
    /// </summary>
    /// <param name="guids">Массив идентификаторов (может быть null)</param>
    /// <param name="addSpace">Добавить пробелы после запятых</param>
    /// <returns>Строка</returns>
    public static string CommaStringFromGuids(Guid[] guids, bool addSpace)
    {
      if (guids == null)
        return String.Empty;
      if (guids.Length == 0)
        return String.Empty;
      if (guids.Length == 1)
        return guids[0].ToString(); // чтобы не создавать объект
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < guids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(guids[i].ToString());
      }
      return sb.ToString();
    }

    #endregion

    #region Текст -> Массив

    /// <summary>
    /// Преобразование строки CSV в двумерный массив строк. Строки разделяются
    /// символами CR+LF, а столбцы - запятыми.
    /// Если массив не прямоугольный, то выбрасывается исключение
    /// </summary>
    /// <param name="s">Строка в формате CSV</param>
    /// <returns>Двумерный массив строк</returns>
    public static string[,] CommaStringToArray2(string s)
    {
      return CommaStringToArray2(s, ',', String.Empty);
    }


    /// <summary>
    /// Преобразование строки CSV в двумерный массив строк. Строки разделяются
    /// символами CR и LF в одной из комбинаций AllPossibleLineSeparators (определяется автоматически), а столбцы - указанным разделителем.
    /// Если массив не прямоугольный, то выбрасывается исключение
    /// </summary>
    /// <param name="s">Строка в формате CSV</param>
    /// <param name="fieldDelimiter">Разделитель полей в строке</param>
    /// <returns>Двумерный массив строк</returns>
    public static string[,] CommaStringToArray2(string s, char fieldDelimiter)
    {
      return CommaStringToArray2(s, fieldDelimiter, String.Empty);
    }

    /// <summary>
    /// Преобразование строки CSV в двумерный массив строк. Строки разделяются
    /// символами CR+LF, а столбцы - указанным разделителем.
    /// Если массив не прямоугольный, то выбрасывается исключение
    /// </summary>
    /// <param name="s">Строка в формате CSV</param>
    /// <param name="fieldDelimiter">Разделитель полей в строке</param>
    /// <param name="newLine">Символы перевода строки. Если не задано, определяется автоматически</param>
    /// <returns>Двумерный массив строк</returns>
    public static string[,] CommaStringToArray2(string s, char fieldDelimiter, string newLine)
    {
      if (String.IsNullOrEmpty(s))
        return null;

      if (String.IsNullOrEmpty(newLine))
      {
        newLine = GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(newLine))
          newLine = Environment.NewLine;
      }

      if (s.EndsWith(newLine))
        s = s.Substring(0, s.Length - newLine.Length);

      string[] a1 = s.Split(new string[] { newLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = CommaStringToArray2Internal(a1, fieldDelimiter, false);
      }
      catch
      {
        a2 = CommaStringToArray2Internal(a1, fieldDelimiter, true);
      }

      return a2;
    }

    private static string[,] CommaStringToArray2Internal(string[] a1, char fieldDelimiter, bool isSimple)
    {
      int n = 0;
      string[][] a3 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
      {
        try
        {
          if (isSimple)
            a3[i] = SimpleCommaStringToArray(a1[i], fieldDelimiter);
          else
            a3[i] = CommaStringToArray(a1[i], fieldDelimiter);
        }
        catch (Exception e)
        {
          throw new ParsingException("Ошибка в строке " + (i + 1).ToString() + ". " + e.Message);
        }

        if (a3[i] != null && a3[i].Length > 0)
        {
          if (n == 0)
            n = a3[i].Length;
          else
          {
            if (a3[i].Length != n)
              throw new InvalidOperationException("Число столбцов в строке " + (i + 1).ToString() + " (" + a3.Length.ToString() +
                ") не совпадает с числом столбцов в первой строке (" + n.ToString() + ")");
          }
        }
      }



      string[,] a2 = new string[a1.Length, n];
      for (int i = 0; i < a1.Length; i++)
      {
        if (a3[i] == null)
          continue;
        int n2 = Math.Min(a3[i].Length, n);
        for (int j = 0; j < n2; j++)
          a2[i, j] = a3[i][j];
      }

      return a2;
    }

    /// <summary>
    /// Получить двумерный массив из строки с разделителем столбов - табуляцией.
    /// Символы перевода строки определяются автоматически
    /// </summary>
    /// <param name="s"></param>
    /// <returns>Двумерный массив строк</returns>
    public static string[,] TabbedStringToArray2(string s)
    {
      return TabbedStringToArray2(s, String.Empty);
    }

    /// <summary>
    /// Получить двумерный массив из строки с заданными разделителями строк CR/LF и
    /// разделителем столбов - табуляцией
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="newLine">Разделитель строк. Если не задан, определяется автоматически из преобразуемой строки</param>
    /// <returns>Двумерный массив</returns>
    public static string[,] TabbedStringToArray2(string s, string newLine)
    {
      if (String.IsNullOrEmpty(s))
        return null;


      if (String.IsNullOrEmpty(newLine))
      {
        newLine = GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(newLine))
          newLine = Environment.NewLine;
      }

      if (s.EndsWith(newLine))
        s = s.Substring(0, s.Length - newLine.Length); // 24.06.2019 было потеряно 04.06.2019


      string[] a1 = s.Split(new string[] { newLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = TabbedStringToArray2Internal(a1, false);
      }
      catch
      {
        a2 = TabbedStringToArray2Internal(a1, true);
      }

      return a2;
    }

    private static string[,] TabbedStringToArray2Internal(string[] a1, bool isSimple)
    {
      string[,] a2 = null;
      int n = 0;
      for (int i = 0; i < a1.Length; i++)
      {
        string[] a3;
        try
        {
          if (isSimple)
            a3 = SimpleCommaStringToArray(a1[i], '\t');
          else
            a3 = CommaStringToArray(a1[i], '\t');
          if (a3 == null)
            a3 = new string[1] { "" };
        }
        catch (Exception e)
        {
          throw new ParsingException("Ошибка в строке " + (i + 1).ToString() + ". " + e.Message, e);
        }
        if (i == 0)
        {
          a2 = new string[a1.Length, a3.Length];
          n = a3.Length;
        }
        else
        {
          if (a3.Length != n)
            throw new InvalidOperationException("Число столбцов в строке " + (i + 1).ToString() + " (" + a3.Length.ToString() +
              ") не совпадает с числом столбцов в первой строке (" + n.ToString() + ")");
        }

        for (int j = 0; j < n; j++)
          a2[i, j] = a3[j];
      }
      return a2;
    }

    #endregion

    #region Массив -> Текст

    /// <summary>
    /// Возвращает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами Environment.NewLine, поля разделяются запятой
    /// </summary>
    /// <param name="a">Массив исходных значений</param>
    /// <returns>Строка CSV</returns>
    public static string CommaStringFromArray2(string[,] a)
    {
      return CommaStringFromArray2(a, ',', Environment.NewLine);
    }

    /// <summary>
    /// Возвращает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами Environment.NewLine, поля разделяются указанным символом
    /// </summary>
    /// <param name="a">Массив исходных значений</param>
    /// <param name="fieldDelimiter">Разделитель записей</param>
    /// <returns>Строка CSV</returns>
    public static string CommaStringFromArray2(string[,] a, char fieldDelimiter)
    {
      return CommaStringFromArray2(a, fieldDelimiter, Environment.NewLine);
    }

    /// <summary>
    /// Возвращает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами CR+LF, поля разделяются указанным символом
    /// </summary>
    /// <param name="a">Массив исходных значений</param>
    /// <param name="fieldDelimiter">Разделитель записей</param>
    /// <param name="newLine">Разделитель строк. Обычно используется Environment.NewLine</param>
    /// <returns>Строка CSV</returns>
    public static string CommaStringFromArray2(string[,] a, char fieldDelimiter, string newLine)
    {
      StringBuilder sb = new StringBuilder();
      CommaStringFromArray2(sb, a, fieldDelimiter, newLine);
      return sb.ToString();
    }

    /// <summary>
    /// Записывает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами Environment.NewLine, поля разделяются запятой
    /// </summary>
    /// <param name="sb">Заполняемый StringBuilder</param>
    /// <param name="a">Массив исходных значений</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a)
    {
      CommaStringFromArray2(sb, a, ',', Environment.NewLine);
    }


    /// <summary>
    /// Записывает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами Environment.NewLine, поля разделяются указанным символом
    /// </summary>
    /// <param name="sb">Заполняемый StringBuilder</param>
    /// <param name="a">Массив исходных значений</param>
    /// <param name="fieldDelimiter">Разделитель записей</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a, char fieldDelimiter)
    {
      CommaStringFromArray2(sb, a, fieldDelimiter, Environment.NewLine);
    }

    /// <summary>
    /// Записывает строку в формате CSV для двумерного массива.
    /// Строки разделяются символами <paramref name="newLine"/>, поля разделяются указанным символом <paramref name="fieldDelimiter"/>
    /// </summary>
    /// <param name="sb">Заполняемый StringBuilder</param>
    /// <param name="a">Массив исходных значений</param>
    /// <param name="fieldDelimiter">Разделитель записей</param>
    /// <param name="newLine">Разделитель строк. Если не задан, используется Environment.NewLine</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a, char fieldDelimiter, string newLine)
    {
      if (a == null)
        return;
      if (String.IsNullOrEmpty(newLine))
        newLine = Environment.NewLine;

      int n = a.GetLength(0);
      int m = a.GetLength(1);

      string[] b = new string[m];

      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
          b[j] = a[i, j];
        CommaStringFromArray(sb, b, fieldDelimiter);
        sb.Append(newLine);
      }
    }

    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. 
    /// Строки, содержащие кавычки, заключаются в кавычки, сами кавычки удваиваются.
    /// Строки разделяются символами Environment.NewLine.
    /// </summary>
    /// <param name="a">Исходный двумерный массив</param>
    /// <returns>Форматированная строка</returns>
    public static string TabbedStringFromArray2(string[,] a)
    {
      return TabbedStringFromArray2(a, false, Environment.NewLine);
    }

    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. 
    /// Строки разделяются символами Environment.NewLine.
    /// </summary>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="simpleValues">Если true, то преобразование значений не выполняется. Если false, то строки, содержащие кавычки, заключаются в кавычки, сами кавычки удваиваются</param>
    /// <returns>Форматированная строка</returns>
    public static string TabbedStringFromArray2(string[,] a, bool simpleValues)
    {
      return TabbedStringFromArray2(a, simpleValues, Environment.NewLine);
    }

    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. 
    /// </summary>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="simpleValues">Если true, то преобразование значений не выполняется. Если false, то строки, содержащие кавычки, заключаются в кавычки, сами кавычки удваиваются</param>
    /// <param name="newLine">Символы перевода строки. Обычно следует использовать Environment.NewLine</param>
    /// <returns>Форматированная строка</returns>
    public static string TabbedStringFromArray2(string[,] a, bool simpleValues, string newLine)
    {
      StringBuilder sb = new StringBuilder();
      TabbedStringFromArray2(sb, a, simpleValues, newLine);
      return sb.ToString();
    }


    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. Версия для StringfBuilder
    /// </summary>
    /// <param name="sb">Сюда записывается форматированная строка</param>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="simpleValues">Если true, то преобразование значений не выполняется. Если false, то строки, содержащие кавычки, заключаются в кавычки, сами кавычки удваиваются</param>
    public static void TabbedStringFromArray2(StringBuilder sb, string[,] a, bool simpleValues)
    {
      TabbedStringFromArray2(sb, a, simpleValues, Environment.NewLine);
    }

    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. Версия для StringfBuilder
    /// </summary>
    /// <param name="sb">Сюда записывается форматированная строка</param>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="simpleValues">Если true, то преобразование значений не выполняется. Если false, то строки, содержащие кавычки, заключаются в кавычки, сами кавычки удваиваются</param>
    /// <param name="newLine">Символы перевода строки. Если не задано, используется Environment.NewLine</param>
    public static void TabbedStringFromArray2(StringBuilder sb, string[,] a, bool simpleValues, string newLine)
    {
      if (a == null)
        return;

      if (String.IsNullOrEmpty(newLine))
        newLine = Environment.NewLine;

      int n = a.GetLength(0);
      int m = a.GetLength(1);

      string[] b = new string[m];

      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
          b[j] = a[i, j];
        if (simpleValues)
          SimpleCommaStringFromArray(sb, b, '\t');
        else
          CommaStringFromArray(sb, b, '\t');
        sb.Append(newLine);
      }
    }

    #endregion

    #endregion
  }
}
