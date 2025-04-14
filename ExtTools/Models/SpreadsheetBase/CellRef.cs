// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Core;
using System.Text.RegularExpressions;

namespace FreeLibSet.Models.SpreadsheetBase
{
  /// <summary>
  /// Статические методы для электронных таблиц
  /// </summary>
  public static class SpreadsheetTools
  {
    #region Номера столбцов

    /// <summary>
    /// Преобразование номера столбца (1 - 65535) в нотации "A1" ("A", "B" .., "Z", "AA", "AB..", "ZZ", "AAA", ...).
    /// </summary>
    /// <param name="columnNumber">Номер столбца, начиная с 1</param>
    /// <returns>Буквенное обозначение столбца</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру <see cref="CellRef"/>.
    /// </remarks>
    public static string GetColumnName(int columnNumber)
    {
      if (columnNumber < 1)
        throw ExceptionFactory.ArgOutOfRange("columnNumber", columnNumber, 1, null);

      string s = String.Empty;
      do
      {
        int v1 = (columnNumber - 1) % 26;
        columnNumber = (columnNumber - 1) / 26;
        char c = (char)('A' + v1);
        s = c + s;
      }
      while (columnNumber > 0);

      return s;
    }

    /// <summary>
    /// Преобразование обозначения столбца в нотации "A1" в номер столбца: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// Если строка пустая или null, вызывается ArgumentNullException. Если строка содержит символы, отличные от "A"-"Z", генерируется
    /// <see cref="FormatException"/>. Если число больше <see cref="System.Int32.MaxValue"/>, генерируется <see cref="OverflowException"/>.
    /// </summary>
    /// <param name="columnName">Буквенное обозначение столбца</param>
    /// <returns>Номер столбца, начиная с 1</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру <see cref="CellRef"/>.
    /// </remarks>
    public static int GetColumnNumber(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      int columnNumber = 0;

      for (int i = 0; i < columnName.Length; i++)
      {
        char ch = columnName[i];
        if (ch < 'A' || ch > 'Z')
          throw new FormatException();
        int v = (ch - 'A') + 1;
        checked
        {
          columnNumber = columnNumber * 26 + v;
        }
      }

      return columnNumber;
    }

    /// <summary>
    /// Преобразование обозначения столбца в нотации "A1" в номер столбца: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// Если строка пустая или не может быть преобразована, возвращается false.
    /// </summary>
    /// <param name="columnName">Буквенное обозначение столбца</param>
    /// <param name="columnNumber">Результат преобразования (номер столбца) или 0 в случае ошибки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру <see cref="CellRef"/>.
    /// </remarks>
    public static bool TryGetColumnNumber(string columnName, out int columnNumber)
    {
      columnNumber = 0;
      if (String.IsNullOrEmpty(columnName))
        return false;

      for (int i = 0; i < columnName.Length; i++)
      {
        char ch = columnName[i];
        if (ch < 'A' || ch > 'Z')
        {
          columnNumber = 0;
          return false;
        }
        int v = (ch - 'A') + 1;
        try
        {
          checked
          {
            columnNumber = columnNumber * 26 + v;
          }
        }
        catch (OverflowException)
        {
          columnNumber = 0;
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Имя листа


    /// <summary>
    /// Возвращает имя листа в апострофах, если это необходимо.
    /// Если имя не содержит плохих символов, возвращается исходное имя.
    /// Используется при задании ссылок.
    /// </summary>
    /// <param name="name">Имя листа без апострофов</param>
    /// <returns>Имя с апострофами</returns>
    public static string GetQuotedSheetName(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      if (Regex.IsMatch(name, @"^{Letter}({Letter}|\d)*$"))
        return name;
      else
        return "'" + name + "'";
    }

    #endregion
  }

  /// <summary>
  /// Стиль форматирования ссылки для <see cref="CellRef"/> и <see cref="RangeRef"/>
  /// </summary>
  public enum CellRefFormat
  {
    /// <summary>
    /// Основной стиль ("A1")
    /// </summary>
    A1,

    /// <summary>
    /// Абсолютная ссылка с долларами ("$A$1")
    /// </summary>
    Abs,

    /// <summary>
    /// RC-ссылка ("R1C1")
    /// </summary>
    R1C1
  }

  /// <summary>
  /// Хранение адреса ячейки (строки и столбца).
  /// Нумерация начинается с 1.
  /// Проверка верхней границы (65535 строк, 255 столбцов) не выполняется
  /// </summary>
  [Serializable]
  public struct CellRef : IEquatable<CellRef>
  {
    #region Конструктор

    /// <summary>
    /// Создает заполненный адрес.
    /// </summary>
    /// <param name="row">Номер строки. Нумерация начинается с 1</param>
    /// <param name="column">Номер столбца. Нумерация начинается с 1</param>
    public CellRef(int row, int column)
    {
      if (row < 1)
        throw ExceptionFactory.ArgOutOfRange("row", row, 1, null);
      if (column < 1)
        throw ExceptionFactory.ArgOutOfRange("column", column, 1, null);

      _Row = row;
      _Column = column;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Номер строки. Нумерация начинается с 1
    /// </summary>
    public int Row { get { return _Row; } }
    private readonly int _Row;

    /// <summary>
    /// Номер столбца. Нумерация начинается с 1
    /// </summary>
    public int Column { get { return _Column; } }
    private readonly int _Column;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _Row == 0; } }

    /// <summary>
    /// Неинициализированная структура
    /// </summary>
    public static readonly CellRef Empty = new CellRef();

    #endregion

    #region Сравнение

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="a">Первый сравниваемый адрес</param>
    /// <param name="b">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(CellRef a, CellRef b)
    {
      return a.Row == b.Row && a.Column == b.Column;
    }

    /// <summary>
    /// Возвращает true, если адреса разные
    /// </summary>
    /// <param name="a">Первый сравниваемый адрес</param>
    /// <param name="b">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(CellRef a, CellRef b)
    {
      return a.Row != b.Row || a.Column != b.Column;
    }

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(CellRef other)
    {
      return this == other;
    }

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is CellRef)
        return this == (CellRef)other;
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns>Числовой код</returns>
    public override int GetHashCode()
    {
      return ((Row & 0xFFFF) << 8) | (Column & 0xFFFF);
    }

    #endregion

    #region Преобразование в строку / из строки

    /// <summary>
    /// Возвращает текстовое представление в формате "A1"
    /// При IsEmpty=true возвращает пустую строку.
    /// </summary>
    /// <returns>Тестовое представление</returns>
    public override string ToString()
    {
      return ToString(CellRefFormat.A1);
    }

    /// <summary>
    /// Возвращает текстовое представление в формате "A1", "$A$1" или "R1C1".
    /// При <see cref="IsEmpty"/>=true возвращает пустую строку.
    /// </summary>
    /// <param name="format">Формат представления</param>
    /// <returns>Тестовое представление</returns>
    public string ToString(CellRefFormat format)
    {
      if (_Row == 0)
        return String.Empty;
      switch (format)
      {
        case CellRefFormat.A1: return SpreadsheetTools.GetColumnName(Column) + StdConvert.ToString(Row);
        case CellRefFormat.Abs: return "$" + SpreadsheetTools.GetColumnName(Column) + "$" + StdConvert.ToString(Row);
        case CellRefFormat.R1C1: return "R" + StdConvert.ToString(Row) + "C" + StdConvert.ToString(Column);
        default:
          throw ExceptionFactory.ArgUnknownValue("format", format);
      }
    }

    /// <summary>
    /// Преобразование строки в формате "A1".
    /// Пустая строка преобразуется в неинициализированную структуру без выдачи ошибки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Структура</returns>
    public static CellRef Parse(string s)
    {
      CellRef res;
      if (TryParse(s, out res))
        return res;
      else
        throw new FormatException();
    }

#if XXX

    /// <summary>
    /// Попытка преобразования строки в формате "A1" в адрес ячейки.
    /// Пустая строка преобразуется в неинициализированную структуру без выдачи ошибки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Результат преобразования или неинициализированная структура, в случае ошибки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out CellRef res)
    {
      if (String.IsNullOrEmpty(s))
      {
        res = Empty;
        return true; // 18.07.2022
      }

      s = s.ToUpperInvariant();

      int nChars = 0;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] >= 'A' && s[i] <= 'Z')
          nChars++;
        else
          break;
      }

      if (nChars < 1 || nChars == s.Length)
      {
        res = Empty;
        return false;
      }

      int columnNumber;
      if (!SpreadsheetTools.TryGetColumnNumber(s.Substring(0, nChars), out columnNumber))
      {
        res = Empty;
        return false;
      }

      // Надо проверить, что идут только цифры.
      // Первый символ не может быть 0.

      if (s[nChars] < '1' || s[nChars] > '9')
      {
        res = Empty;
        return false;
      }

      // Дальше можно использовать стандартное преобразование
      int rowNumber;
      if (!int.TryParse(s.Substring(nChars), System.Globalization.NumberStyles.None,
        CultureInfo.InvariantCulture, out rowNumber))
      {
        res = Empty;
        return false;
      }

      res = new CellRef(rowNumber, columnNumber);
      return true;
    }

#else

    /// <summary>
    /// Попытка преобразования строки в адрес ячейки.
    /// Строка может быть задана в формате "A1", "$A$1" или "R1C1"
    /// Пустая строка преобразуется в неинициализированную структуру без выдачи ошибки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Результат преобразования или неинициализированная структура, в случае ошибки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out CellRef res)
    {
      res = Empty;
      if (String.IsNullOrEmpty(s))
        return true; // 18.07.2022

      s = s.ToUpperInvariant();

      // "Группами" являются части регулярного выражения в круглых скобках.
      // Значения групп доступно через коллекцию Match.Groups.
      // Индекс группы начинается с 1, т.к. индекс 0 соответствует всему выражению
      const string regexA1_Abs = "^[$]?([A-Z]+)[$]?([0-9]+)$";
      const string regexR1C1 = "^[R]([0-9]+)[C]([0-9]+)$";

      int row, col;

      Match m = Regex.Match(s, regexA1_Abs);
      if (m.Success)
      {
        string sCol = m.Groups[1].Value;
        string sRow = m.Groups[2].Value;

        if (!SpreadsheetTools.TryGetColumnNumber(sCol, out col))
          return false;
        if (!StdConvert.TryParse(sRow, out row))
          return false;
      }
      else
      {
        m = Regex.Match(s, regexR1C1);
        if (m.Success)
        {
          string sRow = m.Groups[1].Value;
          string sCol = m.Groups[2].Value;

          if (!StdConvert.TryParse(sRow, out row))
            return false;
          if (!StdConvert.TryParse(sCol, out col))
            return false;
        }
        else
          return false;
      }

      if (row < 1 || col < 1)
        return false;

      res = new CellRef(row, col);
      return true;
    }

#endif

    #endregion
  }

  /// <summary>
  /// Прямоугольный диапазон ячеек
  /// </summary>
  [Serializable]
  public struct RangeRef : IEquatable<RangeRef>
  {
    #region Конструкторы

    /// <summary>
    /// Создает диапазон из первой и последней ячейки.
    /// Если ячейка <paramref name="cell2"/> находится выше или левее <paramref name="cell1"/>, то
    /// все равно создается правильный диапазон.
    /// </summary>
    /// <param name="cell1">Первая ячейка</param>
    /// <param name="cell2">Вторая ячейка</param>
    public RangeRef(CellRef cell1, CellRef cell2)
    {
      if (cell1.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("cell1");
      if (cell2.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("cell2");

      int firstRow = Math.Min(cell1.Row, cell2.Row);
      int lastRow = Math.Max(cell1.Row, cell2.Row);
      int firstColumn = Math.Min(cell1.Column, cell2.Column);
      int lastColumn = Math.Max(cell1.Column, cell2.Column);

      _FirstCell = new CellRef(firstRow, firstColumn);
      _LastCell = new CellRef(lastRow, lastColumn);
    }

    /// <summary>
    /// Создает указанный диапазон.
    /// Если строки или столбцы заданы в неправильном порядке, то они переставляются местами.
    /// </summary>
    /// <param name="firstRow">Первая строка</param>
    /// <param name="firstColumn">Первый столбец</param>
    /// <param name="lastRow">Последняя строка</param>
    /// <param name="lastColumn">Последний столбец</param>
    public RangeRef(int firstRow, int firstColumn, int lastRow, int lastColumn)
      : this(new CellRef(firstRow, firstColumn), new CellRef(lastRow, lastColumn))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Верхняя левая ячейка диапазона
    /// </summary>
    public CellRef FirstCell { get { return _FirstCell; } }
    private readonly CellRef _FirstCell;

    /// <summary>
    /// Правая нижняя ячейка диапазона
    /// </summary>
    public CellRef LastCell { get { return _LastCell; } }
    private readonly CellRef _LastCell;

    /// <summary>
    /// Возвращает количество строк в дипазоне
    /// </summary>
    public int RowCount
    {
      get
      {
        if (IsEmpty)
          return 0;
        else
          return _LastCell.Row - _FirstCell.Row + 1;
      }
    }

    /// <summary>
    /// Возвращает количество столбцов в диапазоне
    /// </summary>
    public int ColumnCount
    {
      get
      {
        if (IsEmpty)
          return 0;
        else
          return _LastCell.Column - _FirstCell.Column + 1;
      }
    }

    /// <summary>
    /// Возвращает количество ячеек в диапазоне
    /// </summary>
    public int CellCount
    {
      get { return RowCount * ColumnCount; }
    }

    /// <summary>
    /// Возвращает true, если диапазон содержит одну ячейку.
    /// При <see cref="IsEmpty"/>=true возвращается false.
    /// </summary>
    public bool IsSingleCell
    {
      get
      {
        if (IsEmpty)
          return false;
        else
          return _LastCell.Row == _FirstCell.Row && _LastCell.Column == _FirstCell.Column;
      }
    }

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _FirstCell.IsEmpty; } }

    /// <summary>
    /// Неинициализированная структура
    /// </summary>
    public static readonly RangeRef Empty = new RangeRef();

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение двух диапазонов
    /// </summary>
    /// <param name="a">Первый сравниваемый диапазон</param>
    /// <param name="b">Второй сравниваемый диапазон</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(RangeRef a, RangeRef b)
    {
      return a.FirstCell == b.FirstCell && a.LastCell == b.LastCell;
    }

    /// <summary>
    /// Сравнение двух диапазонов
    /// </summary>
    /// <param name="a">Первый сравниваемый диапазон</param>
    /// <param name="b">Второй сравниваемый диапазон</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(RangeRef a, RangeRef b)
    {
      return a.FirstCell != b.FirstCell || a.LastCell != b.LastCell;
    }

    /// <summary>
    /// Сравнение с другим диапазоном
    /// </summary>
    /// <param name="other">Второй сравниваемый диапазон</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(RangeRef other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим диапазоном
    /// </summary>
    /// <param name="obj">Второй сравниваемый диапазон</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is RangeRef)
        return this == (RangeRef)obj;
      else
        return false;
    }

    /// <summary>
    /// Получение хэш-кода для коллекций
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return FirstCell.GetHashCode();
    }

    #endregion

    #region Преобразование из текста / в текст

    /// <summary>
    /// Возвращает диаазон в виде "A1:B2".
    /// Если диапазон содержит одну ячейку, то разделитель не используется, возвращается "A1".
    /// Если <see cref="IsEmpty"/>=true, возвращается пустая строка.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return ToString(CellRefFormat.A1);
    }


    /// <summary>
    /// Возвращает диаазон в виде "A1:B2", "$A$1:$B$2" или "R1C1:R2C2".
    /// Если диапазон содержит одну ячейку, то разделитель не используется, возвращается "A1".
    /// Если <see cref="IsEmpty"/>=true, возвращается пустая строка.
    /// </summary>
    /// <param name="format">Формат представления</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(CellRefFormat format)
    {
      if (IsEmpty)
        return String.Empty;

      if (FirstCell == LastCell)
        return FirstCell.ToString(format);
      else
        return FirstCell.ToString(format) + ":" + LastCell.ToString(format);
    }

    /// <summary>
    /// Выполняет преобразование строки вида "A1:B10" или "A1" в диапазон.
    /// Для пустой строки возвращается неинициализированная структура без выброса исключения.
    /// Если строка имеет неподходящий формат, выбрасывается исключение <see cref="FormatException"/>.
    /// В частности, не допускаются пробелы.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Результат преобразования</returns>
    public static RangeRef Parse(string s)
    {
      RangeRef value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException();
    }

    /// <summary>
    /// Выполняет попытку преобразования строки вида "A1:B10" или "A1" в диапазон.
    /// Если строка имеет неподходящий формат, то возвращается false.
    /// В частности, не допускаются пустые строки и пробелы.
    /// Для пустой строки возвращается неинициализированная структура без выдачи ошибки.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="value">Сюда записывается результат преобразования</param>
    /// <returns>True, если преобразование выполнено</returns>
    public static bool TryParse(string s, out RangeRef value)
    {
      if (String.IsNullOrEmpty(s))
      {
        value = new RangeRef();
        return true;
      }
      int p = s.IndexOf(':');
      if (p >= 0)
      {
        string s1 = s.Substring(0, p);
        string s2 = s.Substring(p + 1);

        CellRef c1, c2;
        if (CellRef.TryParse(s1, out c1) && CellRef.TryParse(s2, out c2))
        {
          value = new RangeRef(c1, c2);
          return true;
        }
      }
      else
      {
        CellRef c;
        if (CellRef.TryParse(s, out c))
        {
          value = new RangeRef(c, c);
          return true;
        }
      }
      value = new RangeRef();
      return false;
    }

    #endregion

    #region Попадание

    /// <summary>
    /// Возвращает true, если текущий диапазон содержит указанную ячейку
    /// </summary>
    /// <param name="cell">Проверяемая ячейка</param>
    /// <returns>Попадание ячейки в диапазон</returns>
    public bool Contains(CellRef cell)
    {
      if (IsEmpty || cell.IsEmpty)
        return false;

      return
        cell.Row >= FirstCell.Row &&
        cell.Row <= LastCell.Row &&
        cell.Column >= FirstCell.Column &&
        cell.Column <= LastCell.Column;
    }

    /// <summary>
    /// Возвращает true, если текущий диапазон полностью включает в себя другой диапазон
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Contains(RangeRef range)
    {
      if (IsEmpty || range.IsEmpty)
        return false;

      return
        range.FirstCell.Row >= this.FirstCell.Row &&
        range.LastCell.Row <= this.LastCell.Row &&
        range.FirstCell.Column >= this.FirstCell.Column &&
        range.LastCell.Column <= this.LastCell.Column;
    }

    /// <summary>
    /// Возвращает true, если два диапазона пересекаются
    /// </summary>
    /// <param name="a">Первый проверяемый диапазон</param>
    /// <param name="b">Второй проверяемый диапазон</param>
    /// <returns>Наличие общих ячеек</returns>
    public static bool IsCrossed(RangeRef a, RangeRef b)
    {
      if (a.IsEmpty || b.IsEmpty)
        return false;

      return
        a.FirstCell.Row <= b.LastCell.Row &&
        a.LastCell.Row >= b.FirstCell.Row &&
        a.FirstCell.Column <= b.LastCell.Column &&
        a.LastCell.Column >= b.FirstCell.Column;
    }

    /// <summary>
    /// Возвращает пересечение двух диапазонов.
    /// Если диапазоны не пересекаются, возвращается пустой диапазон
    /// </summary>
    /// <param name="a">Первый проверяемый диапазон</param>
    /// <param name="b">Второй проверяемый диапазон</param>
    /// <returns>Пересечение</returns>
    public static RangeRef GetCross(RangeRef a, RangeRef b)
    {
      if (IsCrossed(a, b))
        return new RangeRef(
          Math.Max(a.FirstCell.Row, b.FirstCell.Row),
          Math.Max(a.FirstCell.Column, b.FirstCell.Column),
          Math.Min(a.LastCell.Row, b.LastCell.Row),
          Math.Min(a.LastCell.Column, b.LastCell.Column));
      else
        return RangeRef.Empty;
    }

    #endregion
  }
}
