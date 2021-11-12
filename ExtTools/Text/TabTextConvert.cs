using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Text
{
  /// <summary>
  /// Преобразование текста с разделителем-табуляцией в/из одномерного и двумерного массива строк.
  /// Класс не является потокобезопасным, т.к. может использовать внутренние поля в процессе преобразования.
  /// Формат имеет условное имя TSV (tab separated values), но в текущей реализации не поддерживаются Escape-последовательности.
  /// То есть, внутри полей не должно быть символов табуляции и новой строки.
  /// http://www.iana.org/assignments/media-types/text/tab-separated-values
  /// </summary>
  public sealed class TabTextConvert
  {
    #region Конструктор

    /// <summary>
    /// Создает преобразователь с настройками по умолчанию
    /// </summary>
    public TabTextConvert()
    {
      _NewLine = Environment.NewLine;
      _AutoDetectNewLine = false;
    }

    #endregion

    #region Управляющие свойства

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

    #endregion

    #region Одномерный массив

    /// <summary>
    /// Преобразование одномерного массива строк в строку CSV.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
    /// </summary>
    /// <param name="a">Массив полей для преобразования в строку</param>
    /// <returns>CSV-строка</returns>
    public string ToString(string[] a)
    {
      if (a == null)
        return String.Empty;

      return String.Join("\t", a);
    }

    /// <summary>
    /// Преобразование одномерного массива строк в строку.
    /// Если <paramref name="a"/>=null или пустой, возвращается пустая строка.
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
          sb.Append('\t');
        sb.Append(a[i]);
      }
    }

    /// <summary>
    /// Преобразование строки, содержащей значения, разделенные табуляцией.
    /// Строка не должна содержать символов переноса строки. Если строка может
    /// содержать несколько строк, используйте ToArray2() для преобразования в двумерный массив.
    /// Если строка пустая, то возвращается null.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив строк-элементов</returns>
    public string[] ToArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      return s.Split('\t');
    }

    #endregion

    #region Двумерный массив

    /// <summary>
    /// Внутренний буфер для ToString()
    /// </summary>
    private StringBuilder _SB;

    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. 
    /// </summary>
    /// <param name="a">Исходный двумерный массив</param>
    /// <returns>Форматированная строка</returns>
    public string ToString(string[,] a)
    {
      if (_SB == null)
        _SB = new StringBuilder();
      _SB.Length = 0;
      ToString(_SB, a);
      return _SB.ToString();
    }


    /// <summary>
    /// Преобразование двумерного массива в строку с разделителем-табуляцией. Версия для StringfBuilder
    /// </summary>
    /// <param name="sb">Сюда записывается форматированная строка</param>
    /// <param name="a">Исходный двумерный массив</param>
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
        {
          if (j > 0)
            sb.Append('\t');
          sb.Append(a[i, j]);
        }
        sb.Append(NewLine);
      }
    }

    /// <summary>
    /// Получить двумерный массив из строки с заданными разделителями строк NewLine и
    /// разделителем столбов - табуляцией.
    /// Если строка пустая, то возращается null.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Двумерный массив</returns>
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
        s = s.Substring(0, s.Length - NewLine.Length); // 24.06.2019 было потеряно 04.06.2019

      string[] a1 = s.Split(new string[] { NewLine }, StringSplitOptions.None);
      string[][] a2 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].Split('\t');
      return DataTools.ToArray2<string>(a2);
    }

    #endregion
  }
}
