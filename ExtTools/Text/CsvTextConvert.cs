﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace FreeLibSet.Text
{
  /// <summary>
  /// Преобразование текста CSV в/из одномерного и двумерного массива строк.
  /// RFC 4180.
  /// https://datatracker.ietf.org/doc/html/rfc4180
  /// Исключение: свойство NewLine
  /// Класс не является потокобезопасным, т.к. может использовать внутренние поля в процессе преобразования
  /// </summary>
  public class CsvTextConvert: ITextConvert, ICloneable
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
    /// Разделитель строк. По умолчанию - <see cref="Environment.NewLine"/>.
    /// Внимание! В RFC 4180 используется разделитель CR+LF. Для соответствия стандарту на не-Windows платформах
    /// следует установить свойство вручную.
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
    /// По умолчанию - false - используется текущее значение свойства <see cref="NewLine"/>.
    /// Если установить в true, то свойство <see cref="NewLine"/> получит новое значение при вызове <see cref="ToMatrixInternal(string[], bool)"/>.
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
    /// Влияет только на методы <see cref="ToString(string[])"/>. При разборе CSV-текста свойство не учитывается.
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
    /// Преобразование одномерного массива строк в строку с указанным разделителем <see cref="FieldDelimiter"/>.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// Если в элементе массива есть символ-разделитель или кавычка, элемент заключается в кавычки,
    /// а внутренние кавычки удваиваются.
    /// Эта версия использует <see cref="StringBuilder"/> для построения строки.
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
    /// <param name="sb">Заполняемый <see cref="StringBuilder"/></param>
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
    /// содержать несколько строк, используйте <see cref="ToMatrix(string)"/> для преобразования в двумерный массив.
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
      int phase = PhaseWaitStr;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == ' ')
        {
          switch (phase)
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
          switch (phase)
          {
            case PhaseWaitStr:
              phase = PhaseInStrQ;
              break;
            case PhaseWaitSep:
              throw new ParsingException(String.Format(Res.CsvTextConvert_Err_BadChar, s[i], i + 1, FieldDelimiterText));
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
                phase = PhaseWaitSep;
              }
              break;
          }
        }
        else if (s[i] == FieldDelimiter)
        {
          switch (phase)
          {
            case PhaseWaitStr:
              // пустая строка - две запятые подряд
              lst.Add("");
              break;
            case PhaseWaitSep:
              phase = PhaseWaitStr;
              break;
            case PhaseInStrNQ:
              // строка закончена
              lst.Add(sb.ToString().Trim());
              sb.Length = 0;
              phase = PhaseWaitStr;
              break;
            case PhaseInStrQ:
              sb.Append(s[i]);
              break;
          }
        }
        else // обычный символ
        {
          switch (phase)
          {
            case PhaseWaitStr:
              // 12.11.2021 Не уверен
              //if (s[i] == '\n')
              //{
              //  if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
              //    sb.Append('\r');
              //}
              sb.Append(s[i]);
              phase = PhaseInStrNQ;
              break;
            case PhaseWaitSep:
              throw new ParsingException(String.Format(Res.CsvTextConvert_Err_BadChar, s[i], i + 1, FieldDelimiterText));
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
      switch (phase)
      {
        case PhaseInStrNQ:
          lst.Add(sb.ToString().Trim());
          break;
        case PhaseWaitStr:
          lst.Add(String.Empty);
          break;
        case PhaseInStrQ:
          throw new ParsingException(Res.CsvTextConvert_Err_StringWithoutEnd);
      }

      return lst.ToArray();
    }

    #endregion

    #region Двумерный массив

    /// <summary>
    /// Преобразование строки CSV в двумерный массив строк. Строки разделяются
    /// символами <see cref="NewLine"/> (если <see cref="AutoDetectNewLine"/>=true, то разделитель строк определяется автоматически и записывается в свойство <see cref="NewLine"/>), а столбцы - указанным разделителем <see cref="FieldDelimiter"/>.
    /// Если массив не прямоугольный, то выбрасывается исключение.
    /// Если строка пустая, возвращается null.
    /// Наличие символов перевода строки внутри текстовых полей не поддерживается, будет возвращен некорректный результат или сгенерировано исключение.
    /// </summary>
    /// <param name="s">Строка в формате CSV</param>
    /// <returns>Двумерный массив строк</returns>
    public string[,] ToMatrix(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;

      if (AutoDetectNewLine)
      {
        NewLine = DataTools.GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(NewLine))
          NewLine = Environment.NewLine;
      }

      if (s.EndsWith(NewLine, StringComparison.Ordinal))
        s = s.Substring(0, s.Length - NewLine.Length);

      string[] a1 = s.Split(new string[] { NewLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = ToMatrixInternal(a1, false);
      }
      catch
      {
        a2 = ToMatrixInternal(a1, true);
      }

      return a2;
    }

    private string[,] ToMatrixInternal(string[] a1, bool isSimple)
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
          throw new ParsingException(String.Format(Res.CsvTextConvert_Err_WithLineNumber, i + 1, e.Message));
        }
      }
      return DataTools.MatrixFromRows<string>(a3);
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

    #region ICloneable

    /// <summary>
    /// Создает копию конвертера с такими же управляющими свойствами
    /// </summary>
    /// <returns>Копия конвертера</returns>
    public CsvTextConvert Clone()
    {
      CsvTextConvert res = new CsvTextConvert();
      res.NewLine = NewLine;
      res.AutoDetectNewLine = AutoDetectNewLine;
      res.AlwaysQuote = AlwaysQuote;
      res.Quote = Quote;
      res.FieldDelimiter = FieldDelimiter;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

#if XXX // Не реализовано

  public struct CsvTextFieldData
  {
  #region Конструктор

    public CsvTextFieldData(string value, string separator)
    {
      _Value = value;
      _Separator = separator;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Значение очередного поля
    /// </summary>
    public string Value { get { return _Value; } }
    private readonly string _Value;


    /// <summary>
    /// Разделитель, идущий после поля.
    /// Это может быть запятая или символ конца строки (CR, LF или CR+LF).
    /// </summary>
    public string Separator { get { return _Separator; } }
    private readonly string _Separator;

  #endregion
  }

  /// <summary>
  /// Потоковый разбор файла CSV с перечислением по отдельным полям.
  /// За один шаг перечисления возвращается очередное поле и разделитель после этого поля.
  /// 
  /// </summary>
  public sealed class CsvTextFieldEnumerable : IEnumerable<CsvTextFieldData>
  {
  #region Конструктор

    public CsvTextFieldEnumerable(TextReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      _Reader = reader;
      _FieldDelimiter = ',';
      _Quote = '\"';
    }

    private TextReader _Reader;

    private bool _EnumeratorCalled;

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
      }
    }
    private char _FieldDelimiter;

    /// <summary>
    /// Символ кавычки
    /// </summary>
    public char Quote
    {
      get { return _Quote; }
      set
      {
        _Quote = value;
      }
    }
    private char _Quote;

  #endregion

  #region Перечислитель

    public struct Enumerator : IEnumerator<CsvTextFieldData>
    {
  #region Защишенный конструктор

      internal Enumerator(CsvTextFieldEnumerable owner)
      {
        _Owner = owner;
        _Buffer = new char[512];
        _CurrPos = 0;
        _BufferLength = _Owner._Reader.ReadBlock(_Buffer, 0, _Buffer.Length);
        _SB = new StringBuilder();
      }

      private CsvTextFieldEnumerable _Owner;

  #endregion

  #region Внутренний буфер

      private char[] _Buffer;
      private int _CurrPos;
      private int _BufferLength;

      /// <summary>
      /// Посмотреть следующий символ
      /// </summary>
      /// <param name="ch">Следующий символ</param>
      /// <returns>true, если символ существует. false, если поток закончился</returns>
      private bool PeekChar(out char ch)
      {
        if (_CurrPos >= _BufferLength)
        {
          _CurrPos = 0;
          _BufferLength = _Owner._Reader.ReadBlock(_Buffer, 0, _Buffer.Length);
          if (_BufferLength == 0)
          {
            ch = '\0';
            return false;
          }
        }
        ch = _Buffer[_CurrPos];
        return true;
      }

      /// <summary>
      /// Перейти к следующему символу
      /// </summary>
      private void AcceptChar()
      {
        _CurrPos++;
      }

  #endregion

      /// <summary>
      /// Текущее поле и сепаратор
      /// </summary>
      public CsvTextFieldData Current { get { return _Current; } }
      private CsvTextFieldData _Current;

      object IEnumerator.Current { get { return _Current; } }

      /// <summary>
      /// Не выполняет никаких действий
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Сборка значений полей и разделителей
      /// </summary>
      private StringBuilder _SB;
      
      /// <summary>
      /// Выполняет чтение очередного поля и сепаратора
      /// </summary>
      /// <returns>Наличие поля и/или сепаратора</returns>
      public bool MoveNext()
      {
        char ch;
        if (!PeekChar(out ch))
        {
          // Нормальное окончание потока
          _Current = new CsvTextFieldData();
          return false;
        }


  #region Чтение поля

        _SB.Length = 0;

        while (true)
        {
          if PeekChar(out ch);
            
        }

  #endregion
      }

      void IEnumerator.Reset()
      {
        throw new InvalidOperationException("Допускается только однократное перечисление");
      }
    }

    /// <summary>
    /// Возвращает перечислитель
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      if (_EnumeratorCalled)
        throw new InvalidOperationException("Повторный вызов метода GetEnumerator() не допускается");

      _EnumeratorCalled = true;
      return new Enumerator(this);
    }

    IEnumerator<CsvTextFieldData> IEnumerable<CsvTextFieldData>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

  #endregion
  }

  /// <summary>
  /// Потоковый разбор файла CSV с перечислением по строкам.
  /// Для каждой строки возвращается массив значений полей в виде массива строк.
  /// Не гарантируется, что для каждой строки будет возвращено одинаковое количество полей.
  /// Класс разрешает только один проход по файлу. Повторные вызовы метода GetEnumerator() не допускаются, как и вызов <see cref="System.Collections.IEnumerator.Reset()"/>.
  /// В отличие от методов <see cref="CsvTextConvert.ToMatrix(string)"/>, поддерживается наличие символов новой строки внутри полей.
  /// Эти символы не преобразуются 
  /// </summary>
  public sealed class CsvTextLineEnumerable : IEnumerable<string[]>
  {
  #region Конструктор

    public CsvTextLineEnumerable(TextReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      _Reader = reader;
    }

    private TextReader _Reader;

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
      }
    }
    private char _FieldDelimiter;

    /// <summary>
    /// Разделитель строк. По умолчанию - <see cref="Environment.NewLine"/>.
    /// Внимание! В RFC 4180 используется разделитель CR+LF. Для соответствия стандарту на не-Windows платформах
    /// следует установить свойство вручную.
    /// </summary>
    public string NewLine
    {
      get { return _NewLine; }
      set
      {
        _NewLine = value;
      }
    }
    private string _NewLine;

    /// <summary>
    /// Нужно ли автоматически определять символы новой строки.
    /// По умолчанию - false - используется текущее значение свойства <see cref="NewLine"/>.
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
      }
    }
    private char _Quote;

  #endregion
  }

#endif
}
