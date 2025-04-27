// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Описание массива строк для заголовков табличного просмотра при печати и экспорте в Excel, HTML, ...
  /// Содержит текст и признаки объединения строк и столбцов
  /// </summary>
  public sealed class BRColumnHeaderArray
  {
    #region Конструктор

    /// <summary>
    /// Создание массива
    /// </summary>
    /// <param name="headers">Описатели заголовков.
    /// Первая размерность массива соответствует столбцам.
    /// Вторая размерность - строкам в заголовках</param>
    public BRColumnHeaderArray(string[][] headers)
      : this(headers, false)
    {
    }

    /// <summary>
    /// Создание массива
    /// </summary>
    /// <param name="headers">Описатели заголовков.
    /// Первая размерность массива соответствует столбцам.
    /// Вторая размерность - строкам в заголовках</param>
    /// <param name="mixedSpanAllowed">Если true, то разрешено объединение по горизонтали для строк,
    /// когда выше имеются необъединенные ячейки.
    /// Если false, то заголовки являются строго иерархическими</param>
    public BRColumnHeaderArray(string[][] headers, bool mixedSpanAllowed)
    {
#if DEBUG
      if (headers == null)
        throw new ArgumentNullException("headers");
#endif

      #region Определение размеров матрицы

      _ColumnCount = headers.Length;
      //_RowCount = 1;
      _RowCount = 0; // 25.08.2023
      for (int iCol = 0; iCol < headers.Length; iCol++)
        _RowCount = Math.Max(RowCount, headers[iCol].Length);

      #endregion

      #region Создание массивов

      _Text = new string[RowCount, ColumnCount];
      _RowSpan = new int[RowCount, ColumnCount];
      _ColumnSpan = new int[RowCount, ColumnCount];

      #endregion

      #region Заполнение и вертикальное объединение

      for (int iCol = 0; iCol < ColumnCount; iCol++)
      {
        string[] a = headers[iCol];
        // Заполняем значения без объединения
        for (int iRow = 0; iRow < a.Length; iRow++)
        {
          Text[iRow, iCol] = a[iRow];
          RowSpan[iRow, iCol] = 1;
          ColumnSpan[iRow, iCol] = 1;
        }

        if (a.Length < RowCount)
        {
          if (a.Length == 0)
          {
            RowSpan[0, iCol] = RowCount;
            Text[0, iCol] = String.Empty; // первая строка не должна содержать null'ы
          }
          else
            RowSpan[a.Length - 1, iCol] = RowCount - a.Length + 1;
        }

        // 17.04.2019
        // Выполняем объединение по вертикали при наличии пропусков строк
        int cntAdd = 0;
        for (int iRow = a.Length - 1; iRow >= 0; iRow--)
        {
          if (Text[iRow, iCol] == null && iRow > 0)
          {
            cntAdd += RowSpan[iRow, iCol];
            RowSpan[iRow, iCol] = 0;
          }
          else
          {
            RowSpan[iRow, iCol] += cntAdd;
            cntAdd = 0;
          }
        } // iRow
      } // iCol

      #endregion

      #region Объединение по горизонтали

      for (int iRow = 0; iRow < RowCount; iRow++)
      {
        for (int iCol = ColumnCount - 1; iCol >= 1; iCol--)
        {
          if (!String.Equals(Text[iRow, iCol], Text[iRow, iCol - 1], StringComparison.Ordinal))
            continue;
          if (RowSpan[iRow, iCol] == 0)
            continue; // уже объединенная ячейка внизу

          if (RowSpan[iRow, iCol] != RowSpan[iRow, iCol - 1])
            continue;

          if (!mixedSpanAllowed)
          {
            // 24.08.2015
            // Предотвращаем объединение нижней строки, когда в ней идут одинаковые значения, а в предыдущей строке - разные
            if (iRow > 0)
            {
              //if (Text[iRow - 1, iCol] != null)
              if (ColumnSpan[iRow - 1, iCol] > 0) // 01.06.2021
                continue;
            }
          }

          ColumnSpan[iRow, iCol - 1] += ColumnSpan[iRow, iCol];
          // Объединяем сразу столбик ячеек
          for (int k = 0; k < RowSpan[iRow, iCol - 1]; k++)
          {
            // исправлено 01.06.2021
            Text[iRow + k, iCol] = null;
            RowSpan[iRow + k, iCol] = 0;
            ColumnSpan[iRow + k, iCol] = 0;
          }
        } // iCol
      } // iRow

      #endregion

      #region BaseCells

      _BaseCells = new CellAddress[RowCount, ColumnCount];
#if DEBUG
      DataTools.FillMatrix<CellAddress>(_BaseCells, new CellAddress(-1, -1));
#endif


      for (int i = 0; i < RowCount; i++)
      {
        for (int j = 0; j < ColumnCount; j++)
        {
          int rs = RowSpan[i, j];
          int cs = ColumnSpan[i, j];
          if (rs > 0 && cs >= 0)
          {
            for (int k = 0; k < rs; k++)
            {
              for (int l = 0; l < cs; l++)
              {
#if DEBUG
                if (_BaseCells[i + k, j + l].RowIndex >= 0)
                  throw new BugException("Merge overlap");
#endif
                _BaseCells[i + k, j + l] = new CellAddress(i, j);
              }
            }
          }
        }
      }

#if DEBUG
      for (int i = 0; i < RowCount; i++)
      {
        for (int j = 0; j < ColumnCount; j++)
        {
          if (_BaseCells[i, j].RowIndex < 0)
            throw new BugException("Merge incomplete");
        }
      }
#endif

      #endregion
    }

    /// <summary>
    /// Создает объект-пустышку с заданным количеством столбцов <see cref="ColumnCount"/> и количеством строк <see cref="RowCount"/>.
    /// Объединение ячеек не выполняется.
    /// </summary>
    /// <param name="rowCount">Количество строк</param>
    /// <param name="columnCount">Количество столбцов</param>
    public BRColumnHeaderArray(int rowCount, int columnCount)
    {
      _RowCount = rowCount;
      _ColumnCount = columnCount;
      _Text = new string[RowCount, ColumnCount];
      _RowSpan = new int[RowCount, ColumnCount];
      _ColumnSpan = new int[RowCount, ColumnCount];
      DataTools.FillMatrix<string>(_Text, String.Empty);
      DataTools.FillMatrix<int>(_RowSpan, 1);
      DataTools.FillMatrix<int>(_ColumnSpan, 1);

      _BaseCells = new CellAddress[RowCount, ColumnCount];
      for (int i = 0; i < rowCount; i++)
      {
        for (int j = 0; j < columnCount; j++)
          _BaseCells[i, j] = new CellAddress(i, j);
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Число строк в заголовках.
    /// То есть, сколько строк требуется выделить на листе Excel, чтобы разместить заголовки
    /// </summary>
    public int RowCount { get { return _RowCount; } }
    private readonly int _RowCount;

    /// <summary>
    /// Количество столбцов таблицы
    /// </summary>
    public int ColumnCount { get { return _ColumnCount; } }
    private readonly int _ColumnCount;

    /// <summary>
    /// Текст ячеек. Первый индекс - строка, второй - столбец
    /// Может содержать символы переноса строки \r\n, символы мягкого переноса
    /// и неразрывного пробела
    /// </summary>
    public string[,] Text { get { return _Text; } }
    private readonly string[,] _Text;

    /// <summary>
    /// Число объединяемых строк. Установлено для первой строки объединения,
    /// для остальных - 0. Для обычной (необъединяемой) строки равно 1
    /// </summary>
    public int[,] RowSpan { get { return _RowSpan; } }
    private readonly int[,] _RowSpan;

    /// <summary>
    /// Число объединяемых столбцов. Установлено для первого столбца объединения,
    /// для остальных - 0. Для обычного (необъединяемого) столбца равно 1
    /// </summary>
    public int[,] ColumnSpan { get { return _ColumnSpan; } }
    private readonly int[,] _ColumnSpan;

    /// <summary>
    /// Адрес ячейки.
    /// Структура однократной записи
    /// </summary>
    public struct CellAddress
    {
      #region Конструктор

      /// <summary>
      /// Инициализация структуры
      /// </summary>
      /// <param name="rowIndex">Индекс строки</param>
      /// <param name="columnIndex">Индекс столбца</param>
      public CellAddress(int rowIndex, int columnIndex)
      {
        _RowIndex = rowIndex;
        _ColumnIndex = columnIndex;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Индекс строки
      /// </summary>
      public int RowIndex { get { return _RowIndex; } }
      private readonly int _RowIndex;

      /// <summary>
      /// Индекс столбца
      /// </summary>
      public int ColumnIndex { get { return _ColumnIndex; } }
      private readonly int _ColumnIndex;

      /// <summary>
      /// Для отладки
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return "RowIndex=" + RowIndex.ToString() + ", ColumnIndex=" + ColumnIndex.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Индексы базовых ячеек.
    /// Для обычной, не объединенной, ячейки содержит ее же координаты.
    /// Для ячеек объединения содержит координаты верхней левой ячейки объединения
    /// </summary>
    public CellAddress[,] BaseCells { get { return _BaseCells; } }
    private readonly CellAddress[,] _BaseCells;

    /// <summary>
    /// Возвращает текст "RowCount x ColumnCount"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return RowCount.ToString() + "x" + ColumnCount.ToString();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Определение области объединения для произвольной ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки проверяемой ячейки</param>
    /// <param name="columnIndex">Индекс столбца проверяемой ячейки</param>
    /// <param name="firstRowIndex">Результат: Строка базовой ячейки</param>
    /// <param name="firstColumnIndex">Результат: Столбец базовой ячейки</param>
    /// <param name="rowCount">Результат: Число строк в объединении</param>
    /// <param name="columnCount">Результат: Число столбцов в объединении</param>
    public void GetMergeArea(int rowIndex, int columnIndex, out int firstRowIndex, out int firstColumnIndex, out int rowCount, out int columnCount)
    {
      if (rowIndex < 0 || rowIndex >= this.RowCount)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, this.RowCount - 1);
      if (columnIndex < 0 || columnIndex >= this.ColumnCount)
        throw ExceptionFactory.ArgOutOfRange("columnIndex", columnIndex, 0, this.ColumnCount - 1);

      firstRowIndex = BaseCells[rowIndex, columnIndex].RowIndex;
      firstColumnIndex = BaseCells[rowIndex, columnIndex].ColumnIndex;
      rowCount = RowSpan[firstRowIndex, firstColumnIndex];
      columnCount = ColumnSpan[firstRowIndex, firstColumnIndex];
#if DEBUG
      if (rowCount < 1 || columnCount < 1)
        throw new BugException();
#endif
    }

    #endregion
  }
}
