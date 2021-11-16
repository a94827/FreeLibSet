using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Text
{
  /// <summary>
  /// Преобразование текста CSV в/из одномерного и двумерного массива строк.
  /// RFC 4180.
  /// https://datatracker.ietf.org/doc/html/rfc4180
  /// Исключение: свойство NewLine
  /// Класс не является потокобезопасным, т.к. может использовать внутренние поля в процессе преобразования
  /// </summary>
  public class CsvTextConvert
  {
    #region Конструктор

    /// <summary>
    /// Создает преобразователь с настройками по умолчанию
    /// </summary>
    public CsvTextConvert()
    {
      _FieldDelimiter = ',';
      _NewLine = Environment.NewLine;
      _AutoDetectNewLine = false;
      _Quote = '\"';
      _AlwaysQuote = false;
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Символ-разделитель полей в пределах строки.
    /// По умолчанию - запятая
    /// </summary>
    public char FieldDelimiter
    {
      get { return _FieldDelimiter; }
      set
      {
        _FieldDelimiter = value;
        _QuotingRequiredChars = null; // индексатор недействителен
      }
    }
    private char _FieldDelimiter;

    /// <summary>
    /// Разделитель строк. По умолчанию - Environment.NewLine.
    /// Внимание! В RFC 4180 используется разделитель CR+LF. Для соответствия стандарту на не-Windows платформах
    /// следует установить свойство вручную
    /// </summary>
    public string NewLine
    {
      get { return _NewLine; }
      set
      {
        _NewLine = value;
        _QuotingRequiredChars = null; // индексатор недействителен
      }
    }
    private string _NewLine;

    /// <summary>
    /// Нужно ли автоматически определять символы новой строки при преобразовании строки CSV в двумерный массив.
    /// По умолчанию - false - используется текущее значение свойства NewLine.
    /// Если установить в true, то свойство NewLine получит новое значение при вызове ToArray2().
    /// </summary>
    public bool AutoDetectNewLine
    {
      get { return _AutoDetectNewLine; }
      set { _AutoDetectNewLine = value; }
    }
    private bool _AutoDetectNewLine;

    /// <summary>
    /// Символ кавычки
    /// </summary>
    public char Quote
    {
      get { return _Quote; }
      set
      {
        _Quote = value;
        _QuotingRequiredChars = null; // индексатор недействителен
      }
    }
    private char _Quote;

    /// <summary>
    /// Если true, то поля будут всегда заключаться в кавычки, включая пустые поля.
    /// По умолчанию - false, кавычки ставятся только при необходимости: при наличии в поле запятых, кавычек или переноса строк.
    /// Влияет только на методы ToString(). При разборе CSV-текста свойство не учитывается.
    /// </summary>
    public bool AlwaysQuote
    {
      get { return _AlwaysQuote; }
      set { _AlwaysQuote = value; }
    }
    private bool _AlwaysQuote;

    #endregion

    #region Массив -> CSV

    #region Одномерный массив

    /// <summary>
    /// Внутренний буфер.
    /// Используется для методов ToString(), возвращающих string
    /// </summary>
    private StringBuilder _SB;

    /// <summary>
    /// Преобразование одномерного массива строк в строку CSV.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// </summary>
    /// <param name="a">Массив полей для преобразования в строку</param>
    /// <returns>CSV-строка</returns>
    public string ToString(string[] a)
    {
      if (_SB == null)
        _SB = new StringBuilder();
      _SB.Length = 0;

      ToString(_SB, a);
      return _SB.ToString();
    }

    /// <summary>
    /// Преобразование одномерного массива строк в строку с указанным разделителем.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть символ-разделитель или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// Эта версия использует StringBuilder для построения строки.
    /// </summary>
    /// <param name="sb">Сюда записывается CSV-строка</param>
    /// <param name="a">Массив полей для преобразования в строку</param>
    public void ToString(StringBuilder sb, string[] a)
    {
      if (a == null)
        return; // 16.11.2016

      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(FieldDelimiter);

        string fld = a[i];
        if (fld == null)
          fld = String.Empty;

        if (QuotingRequired(fld))
        {
          // в поле есть символ-разделитель
          // Помещаем текст в кавычки, кавычки в тексте удваиваем
          sb.Append(Quote);
          if (fld.IndexOf(Quote) >= 0)
          {
            // Есть кавычки для удваивания
            for (int j = 0; j < fld.Length; j++)
            {
              if (fld[j] == Quote)
                sb.Append(Quote);
              sb.Append(fld[j]);
            }
          }
          else
            // Нет кавычек
            sb.Append(fld);
          sb.Append(Quote);
        }
        else
          // Строка не содержит символа разделителя
          sb.Append(fld);
      }
    }

    private ArrayIndexer<char> _QuotingRequiredChars; // выгоднее использовать ArrayIndexer, а не CharArrayIndexer

    /// <summary>
    /// Нужно ли заключать поле в кавычки?
    /// </summary>
    /// <param name="fld">Поле</param>
    /// <returns>Необходимость кавычек</returns>
    private bool QuotingRequired(string fld)
    {
      if (AlwaysQuote)
        return true;
      if (fld.Length == 0)
        return false;

      if (_QuotingRequiredChars == null)
      {
        SingleScopeList<char> lst = new SingleScopeList<char>();
        lst.Add(FieldDelimiter);
        lst.Add(Quote);
        for (int i = 0; i < NewLine.Length; i++)
          lst.Add(NewLine[i]);
        lst.Add('\r');
        lst.Add('\n');
        _QuotingRequiredChars = new ArrayIndexer<char>(lst);
      }

      for (int i = 0; i < fld.Length; i++)
      {
        if (_QuotingRequiredChars.Contains(fld[i]))
          return true;
      }

      return false;
    }

    #endregion

    #region Двумерный массив

    /// <summary>
    /// Возвращает строку в формате CSV для двумерного массива.
    /// </summary>
    /// <param name="a">Массив исходных значений</param>
    /// <returns>Строка CSV</returns>
    public string ToString(string[,] a)
    {
      if (_SB == null)
        _SB = new StringBuilder();
      _SB.Length = 0;
      ToString(_SB, a);
      return _SB.ToString();
    }

    /// <summary>
    /// Записывает строку в формате CSV для двумерного массива.
    /// </summary>
    /// <param name="sb">Заполняемый StringBuilder</param>
    /// <param name="a">Массив исходных значений</param>
    public void ToString(StringBuilder sb, string[,] a)
    {
      if (a == null)
        return;

      int n = a.GetLength(0);
      int m = a.GetLength(1);

      string[] b = new string[m];

      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
          b[j] = a[i, j];
        ToString(sb, b);
        sb.Append(NewLine);
      }
    }

    #endregion

    #endregion

    #region CSV->Массив

    #region Одномерный массив

    /// <summary>
    /// Для выдачи исключения
    /// </summary>
    private string FieldDelimiterText
    {
      get
      {
        switch (FieldDelimiter)
        {
          case '\t': return "<Tab>";
          case ' ': return "<Space>";
          default: return FieldDelimiter.ToString();
        }
      }
    }

    /// <summary>
    /// Преобразование строки, содержащей значения, разделенные запятыми или другим разделителем, например, табуляцией.
    /// Строка не должна содержать символов переноса строки (вне поля в кавычках). Если строка может
    /// содержать несколько строк, используйте ToArray2() для преобразования в двумерный массив.
    /// Концевые пробелы отбрасываются. Также отбрасываются пробелы слева и справа
    /// от каждого разделителя (если они не внутри кавычек). 
    /// Если строка пустая или не содержит ничего, кроме пробелов, то возвращается null.
    /// Разделители внутри кавычек считаются частью строки.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив строк-элементов</returns>
    /// <exception cref="FreeLibSet.Core.ParsingException">Нарушение структуры CSV</exception>
    public string[] ToArray(string s)
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

      // 12.11.2021 Пока не уверен
      //if (s.IndexOf(fieldDelimiter) < 0)
      //{
      //  // 10.11.2012
      //  // Строка не содержит разделителя

      //  if (s.Length < 3 || s[0] != '\"' || s[s.Length - 1] != '\"')
      //    return new string[1] { s };
      //}

      List<string> lst = new List<string>();
      StringBuilder sb = new StringBuilder();
      int Phase = PhaseWaitStr;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == ' ')
        {
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
        }
        else if (s[i] == Quote)
        {
          switch (Phase)
          {
            case PhaseWaitStr:
              Phase = PhaseInStrQ;
              break;
            case PhaseWaitSep:
              throw new ParsingException("В позиции " + (i + 1).ToString() + " неожиданный символ \". Ожидался разделитель \"" + FieldDelimiterText + "\"");
            case PhaseInStrNQ:
              // если текст начался не с кавычки, то кавычки в строке не задваиваются
              sb.Append(Quote);
              break;
            case PhaseInStrQ:
              if (i < (s.Length - 1) && s[i + 1] == Quote)
              {
                sb.Append(Quote);
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
        }
        else if (s[i] == FieldDelimiter)
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
              // 12.11.2021 Не уверен
              //if (s[i] == '\n')
              //{
              //  if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
              //    sb.Append('\r');
              //}
              sb.Append(s[i]);
              Phase = PhaseInStrNQ;
              break;
            case PhaseWaitSep:
              throw new ParsingException("В позиции " + (i + 1).ToString() + " неожиданный символ \"" + s[i] + "\". Ожидался разделитель \"" + FieldDelimiterText + "\"");
            case PhaseInStrNQ:
            case PhaseInStrQ:
              // 12.11.2021 Не уверен
              //if (s[i] == '\n')
              //{
              //  if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
              //    sb.Append('\r');
              //}
              sb.Append(s[i]);
              break;
          }
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

    #endregion

    #region Двумерный массив

    /// <summary>
    /// Преобразование строки CSV в двумерный массив строк. Строки разделяются
    /// символами CR+LF, а столбцы - указанным разделителем.
    /// Если массив не прямоугольный, то выбрасывается исключение
    /// Если строка пустая, возвращается null.
    /// </summary>
    /// <param name="s">Строка в формате CSV</param>
    /// <returns>Двумерный массив строк</returns>
    public string[,] ToArray2(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;

      if (AutoDetectNewLine)
      {
        NewLine = DataTools.GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(NewLine))
          NewLine = Environment.NewLine;
      }

      if (s.EndsWith(NewLine))
        s = s.Substring(0, s.Length - NewLine.Length);

      string[] a1 = s.Split(new string[] { NewLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = ToArray2Internal(a1, false);
      }
      catch
      {
        a2 = ToArray2Internal(a1, true);
      }

      return a2;
    }

    private string[,] ToArray2Internal(string[] a1, bool isSimple)
    {
      string[][] a3 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
      {
        try
        {
          if (isSimple)
            a3[i] = SimpleToArray(a1[i]);
          else
            a3[i] = ToArray(a1[i]);
        }
        catch (Exception e)
        {
          throw new ParsingException("Ошибка в строке " + (i + 1).ToString() + ". " + e.Message);
        }
      }
      return DataTools.ToArray2<string>(a3);
    }

    private string[] SimpleToArray(string s)
    {
      string[] a = s.Split(FieldDelimiter);
      for (int i = 0; i < a.Length; i++)
        a[i] = a[i].Trim();
      return a;
    }


    #endregion

    #endregion
  }
}
