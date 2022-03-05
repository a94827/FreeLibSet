// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Text
{
  /// <summary>
  /// Специальные значения сепараторов для конструктора TextWithRows 
  /// </summary>
  public enum TextWithRowsLineSeparators
  { 
    /// <summary>
    /// Значение по умолчанию - единственный сепаратор, задаваемый Environment.NewLine
    /// </summary>
    Default,

    /// <summary>
    /// Все возможные сочетания CR и LF
    /// </summary>
    AllPosible,
  }

  /// <summary>
  /// Хранение текста, содержащего строки
  /// Позволяет выполнить преобразование из абсолютной позиции в номер строки / столбца и обратно
  /// Потокобезопасный класс "однократной записи" (все значение устанавливаются в конструкторе)
  /// Класс реализует перечислитель по строкам
  /// </summary>
  public sealed class TextWithRows : IEnumerable<string>
  {
    #region Конструкторы

    /// <summary>
    /// Создание объекта с использованием разделителя по умолчанию Environment.NewLine 
    /// </summary>
    /// <param name="text">Текст</param>
    public TextWithRows(string text)
      : this(text, DataTools.NewLineSeparators)
    {
    }

    /// <summary>
    /// Создание объекта с использованием заданного разделителя. 
    /// </summary>
    /// <param name="Text">Текст</param>
    /// <param name="lineSeparators">Разделитель строк</param>
    public TextWithRows(string Text, TextWithRowsLineSeparators lineSeparators)
      : this(Text, GetLineSeparators(lineSeparators))
    {
    }

    private static string[] GetLineSeparators(TextWithRowsLineSeparators lineSeparators)
    {
      switch (lineSeparators)
      { 
        case TextWithRowsLineSeparators.Default:
          return DataTools.NewLineSeparators;
        case TextWithRowsLineSeparators.AllPosible:
          return DataTools.AllPossibleLineSeparators;
        default:
          throw new ArgumentException("Неизвестное значение для списка сепараторов " + lineSeparators.ToString(), "lineSeparators");
      }
    }

    /// <summary>
    /// Создание объекта с использованием указанных разделителей
    /// При этом "сложные" разделители должны идти перед простыми, то есть правильным будет порядок
    /// { "\r\n", "\r\n", "\r", "\n" }
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="lineSeparators">Список разделителей строк. Если не содержит разделителей, считается, что текст содержит единственную строку</param>
    public TextWithRows(string text, string[] lineSeparators)
    {
      if (text == null)
        throw new ArgumentNullException("Text");
      _Text = text;

      _RowStarts = new List<int>();
      _RowLengths = new List<int>();

      if (lineSeparators == null)
        lineSeparators = DataTools.EmptyStrings;
      if (lineSeparators.Length == 0)
      {
        _RowStarts = new List<int>(1);
        _RowLengths = new List<int>(1);
        _RowStarts.Add(0);
        _RowLengths.Add(text.Length);
      }

      _RowStarts = new List<int>();
      _RowLengths = new List<int>();

      int pos = 0;

      if (lineSeparators.Length == 1)
      {
        // Оптимзированная реализация для одного разделителя
        while (pos < text.Length)
        {
          int p = text.IndexOf(lineSeparators[0], pos, text.Length - pos, StringComparison.Ordinal);
          if (p < 0)
            break;

          // Нашли сепаратор. Можно дальше не искать
          _RowStarts.Add(pos);
          _RowLengths.Add(p - pos);
          pos = p + lineSeparators[0].Length;
        }
      }
      else
      {
        while (pos < text.Length)
        {
          bool found = false;
          for (int i = 0; i < lineSeparators.Length; i++)
          {
            int p = text.IndexOf(lineSeparators[i], pos, text.Length - pos, StringComparison.Ordinal);
            if (p < 0)
              continue;

            // Нашли сепаратор. Можно дальше не искать
            _RowStarts.Add(pos);
            _RowLengths.Add(p - pos);
            pos = p + lineSeparators[i].Length;
            found = true;
            break;
          }
          if (!found)
            break;
        }
      }

      if (pos < (text.Length - 1))
      {
        // Последняя строка, не завершенная символом конца строки
        _RowStarts.Add(pos);
        _RowLengths.Add(text.Length - pos);
      }
    }

    #endregion

    #region Сплошной текст

    /// <summary>
    /// Текст в исходном виде.
    /// Преобразование сепараторов не учитывается, даже если оно выполнялось в конструкторе
    /// </summary>
    public string Text { get { return _Text; } }
    private string _Text;

    /// <summary>
    /// Возвращает свойство Text
    /// Преобразование сепараторов не учитывается, даже если оно выполнялось в конструкторе
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Text;
    }

    /// <summary>
    /// Возвращает массив отдельных строк.
    /// Если в конструкторе была задана замена сепараторов, то она учитывается при разбиении на строки
    /// </summary>
    /// <returns>Массив строк, содержащий RowCount элементов</returns>
    public string[] ToArray()
    {
      string[] a = new string[RowCount];
      for (int i = 0; i < a.Length; i++)
        a[i] = this[i];
      return a;
    }

    #endregion

    #region Доступ к отдельным строкам

    /// <summary>
    /// Массив позиций начала каждой строки.
    /// Первый элемент массива содержит 0
    /// </summary>
    private List<int> _RowStarts;

    /// <summary>
    /// Массив, содержащий длину каждой строки
    /// В длину строки не входят символы перхода на новую строку
    /// </summary>
    private List<int> _RowLengths;

    /// <summary>
    /// Возвращает число строк
    /// </summary>
    public int RowCount { get { return _RowStarts.Count; } }

    /// <summary>
    /// Получить строку с заданным индексом от 0 до RowCount-1
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <returns>Строка</returns>
    public string this[int rowIndex]
    {
      get
      {
        if (rowIndex < 0 || rowIndex >= _RowStarts.Count)
          throw new ArgumentOutOfRangeException("rowIndex", rowIndex, "Индекс строки должен быть в диапазоне от 0 до " + (_RowStarts.Count - 1).ToString());
        return _Text.Substring(_RowStarts[rowIndex], _RowLengths[rowIndex]);
      }
    }

    #endregion

    #region Преобразование абсолютной позиции в номер строки/столбца и обратно

    // "Position" - номер строки и столбца
    // "Index" - индекс в строке Text

    /// <summary>
    /// Получить номер строки и столбца, соответствующего заданному смещению в строке Text.
    /// <paramref name="index"/> должен быть в диапазоне от 0 до Text.Length-1.
    /// Если смещение указывает на символы конца строки, то возвращается номер столбца, больший, чем длина строки
    /// </summary>
    /// <param name="index">Смещение в строке Text</param>
    /// <returns>Позиция символа (индекс строки и столбца)</returns>
    public TextPosition GetPosition(int index)
    {
      int row = GetRow(index);
      int rowStart = _RowStarts[row];
      return new TextPosition(row, index - rowStart);
    }

    /// <summary>
    /// Получить номер строки, соответствующий заданному смещению в строке Text.
    /// <paramref name="index"/> должен быть в диапазоне от 0 до Text.Length-1.
    /// Если Index указывает на символ-разделитель строк, то возвращается индекс предыдущей строки.
    /// Для получения номера столбца следует использовать метод GetPosition() или вычислить смещение, используя
    /// GetRowStartIndex()
    /// </summary>
    /// <param name="index">Смещение в строке Text</param>
    /// <returns>Индекс строки</returns>
    public int GetRow(int index)
    {
      if (index < 0 || index >= _Text.Length)
        throw new ArgumentOutOfRangeException("index", index, "Индекс должен быть в диапазоне от 0 до " + (_Text.Length - 1).ToString());

      int p = _RowStarts.BinarySearch(index);
      if (p >= 0)
        return p;
      else
        return (~p) - 1;
    }

    /// <summary>
    /// Возвращает индекс первой позиции в строке Text для строки с заданным номером.
    /// Номер строки <paramref name="row"/> должен быть в диапазоне от 0 до RowCount-1
    /// </summary>
    /// <param name="row">Номер строки (нумерация начинается с 0)</param>
    /// <returns>Индекс в строке Text</returns>
    public int GetRowStartIndex(int row)
    {
      if (row < 0 || row >= _RowStarts.Count)
        throw new ArgumentOutOfRangeException("row", row, "Номер строки должен быть в диапазоне от 0 до " + (_RowStarts.Count - 1).ToString());
      return _RowStarts[row];
    }

    /// <summary>
    /// Возвращает длину строки с заданным номером.
    /// Номер строки <paramref name="row"/> должен быть в диапазоне от 0 до RowCount-1.
    /// В длину не входят символы новой строки
    /// </summary>
    /// <param name="row">Номер строки (нумерация начинается с 0)</param>
    /// <returns>Количество символов в строке</returns>
    public int GetRowLength(int row)
    {
      if (row < 0 || row >= _RowStarts.Count)
        throw new ArgumentOutOfRangeException("row", row, "Номер строки должен быть в диапазоне от 0 до " + (_RowStarts.Count - 1).ToString());
      return _RowLengths[row];
    }

    /// <summary>
    /// Получить смещение в массиве Text, соответствующее заданному номеру строки и столбца
    /// Если номер строки в <paramref name="position"/> выходит за пределы допустимого значения,
    /// выбрасывается ArgumentOutOfRangeException. 
    /// Если номер столбца больше, чем длина строки, возвращается индекс, на один больший, чем длина строки
    /// Если строка не является последней, возвращается позиция разделителя строки. Для последней строки
    /// возвращается либо позиция разделителя, либо значение Text.Length, если последняя строка не содержит
    /// разделителя
    /// </summary>
    /// <param name="position">Позиция символа (индекс строки и символоа в строке)</param>
    /// <returns>Индекс в строке Text</returns>
    public int GetCharIndex(TextPosition position)
    {
      int startIndex = GetRowStartIndex(position.Row); // там проверяется номер строки
      int length = _RowLengths[position.Row];
      int p = position.Column;
      if (p > length)
        p = length;
      return startIndex + p;
    }

    #endregion

    #region IEnumerable<string> Members

    /// <summary>
    /// Перечислитель по строкам
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<string>
    {
      #region Конструктор

      internal Enumerator(TextWithRows source)
      {
        _Source = source;
        _RowIndex = -1;
      }

      #endregion

      #region Поля

      TextWithRows _Source;

      int _RowIndex;

      #endregion

      #region IEnumerator<string> Members

      /// <summary>
      /// Текущая строка
      /// </summary>
      public string Current
      {
        get { return _Source[_RowIndex]; }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Source[_RowIndex]; }
      }

      /// <summary>
      /// Переход к следующей строке
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _RowIndex++;
        return _RowIndex < _Source.RowCount;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _RowIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по строкам
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }

  /// <summary>
  /// Структура, хранящая индекс строки и столбца текстовой позиции
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  [Serializable]
  public struct TextPosition
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру
    /// </summary>
    /// <param name="row">Индекс строки</param>
    /// <param name="column">Индекс столбца</param>
    public TextPosition(int row, int column)
    {
      if (row < 0)
        throw new ArgumentOutOfRangeException("row", row, "Номер строки не может быть отрицательным");
      if (column < 0)
        throw new ArgumentOutOfRangeException("column", column, "Номер столбца не может быть отрицательным");

      _Row = row;
      _Column = column;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Номер строки. Нумерация начинается с 0
    /// </summary>
    public int Row { get { return _Row; } }
    private int _Row;

    /// <summary>
    /// Номер столбца. Нумерация начинаяется с 0
    /// </summary>
    public int Column { get { return _Column; } }
    private int _Column;

    #endregion

    #region Методы

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "{" + Row.ToString() + "," + Column.ToString() + "}";
    }

    #endregion
  }
}
