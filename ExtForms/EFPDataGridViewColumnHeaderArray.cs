using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Описание массива строк для заголовков табличного просмотра при печати и экспорте в Excel, HTML, ...
  /// Содержит текст и признаки объединения строк и столбцов
  /// </summary>
  public class EFPDataGridViewColumnHeaderArray
  {
    #region Конструктор

    /// <summary>
    /// Создание массива
    /// </summary>
    /// <param name="headers">Описатели заголовков.
    /// Первая размерность массива соответствует столбцам.
    /// Вторая размерность - строкам в заголовках</param>
    public EFPDataGridViewColumnHeaderArray(string[][] headers)
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
    public EFPDataGridViewColumnHeaderArray(string[][] headers, bool mixedSpanAllowed)
    {
#if DEBUG
      if (headers == null)
        throw new ArgumentNullException("headers");
#endif

      #region Определение размеров матрицы

      _ColumnCount = headers.Length;
      _RowCount = 1;
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
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Число строк в заголовках.
    /// То есть, сколько строк требуется выделить на листе Excel, чтобы разместить заголовки
    /// </summary>
    public int RowCount { get { return _RowCount; } }
    private int _RowCount;

    /// <summary>
    /// Количество столбцов таблицы
    /// </summary>
    public int ColumnCount { get { return _ColumnCount; } }
    private int _ColumnCount;

    /// <summary>
    /// Текст ячеек. Первый индекс - строка, второй - столбец
    /// Может содержать символы переноса строки \r\n, символы мягкого переноса
    /// и неразрывного пробела
    /// </summary>
    public string[,] Text { get { return _Text; } }
    private string[,] _Text;

    /// <summary>
    /// Число объединяемых строк. Установлено для первой строки объединения,
    /// для остальных - 0. Для обычной (необъединяемой) строки равно 1
    /// </summary>
    public int[,] RowSpan { get { return _RowSpan; } }
    private int[,] _RowSpan;

    /// <summary>
    /// Число объединяемых столбцов. Установлено для первого столбца объединения,
    /// для остальных - 0. Для обычного (необъединяемого) столбца равно 1
    /// </summary>
    public int[,] ColumnSpan { get { return _ColumnSpan; } }
    private int[,] _ColumnSpan;

    /// <summary>
    /// Возвращает текст "RowCount x ColumnCount"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return RowCount.ToString() + "x" + ColumnCount.ToString();
    }

    #endregion
  }
}
