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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// �������� ������� ����� ��� ���������� ���������� ��������� ��� ������ � �������� � Excel, HTML, ...
  /// �������� ����� � �������� ����������� ����� � ��������
  /// </summary>
  public class EFPDataGridViewColumnHeaderArray
  {
    #region �����������

    /// <summary>
    /// �������� �������
    /// </summary>
    /// <param name="headers">��������� ����������.
    /// ������ ����������� ������� ������������� ��������.
    /// ������ ����������� - ������� � ����������</param>
    public EFPDataGridViewColumnHeaderArray(string[][] headers)
      : this(headers, false)
    {
    }

    /// <summary>
    /// �������� �������
    /// </summary>
    /// <param name="headers">��������� ����������.
    /// ������ ����������� ������� ������������� ��������.
    /// ������ ����������� - ������� � ����������</param>
    /// <param name="mixedSpanAllowed">���� true, �� ��������� ����������� �� ����������� ��� �����,
    /// ����� ���� ������� �������������� ������.
    /// ���� false, �� ��������� �������� ������ ��������������</param>
    public EFPDataGridViewColumnHeaderArray(string[][] headers, bool mixedSpanAllowed)
    {
#if DEBUG
      if (headers == null)
        throw new ArgumentNullException("headers");
#endif

      #region ����������� �������� �������

      _ColumnCount = headers.Length;
      _RowCount = 1;
      for (int iCol = 0; iCol < headers.Length; iCol++)
        _RowCount = Math.Max(RowCount, headers[iCol].Length);

      #endregion

      #region �������� ��������

      _Text = new string[RowCount, ColumnCount];
      _RowSpan = new int[RowCount, ColumnCount];
      _ColumnSpan = new int[RowCount, ColumnCount];

      #endregion

      #region ���������� � ������������ �����������

      for (int iCol = 0; iCol < ColumnCount; iCol++)
      {
        string[] a = headers[iCol];
        // ��������� �������� ��� �����������
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
            Text[0, iCol] = String.Empty; // ������ ������ �� ������ ��������� null'�
          }
          else
            RowSpan[a.Length - 1, iCol] = RowCount - a.Length + 1;
        }

        // 17.04.2019
        // ��������� ����������� �� ��������� ��� ������� ��������� �����
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

      #region ����������� �� �����������

      for (int iRow = 0; iRow < RowCount; iRow++)
      {
        for (int iCol = ColumnCount - 1; iCol >= 1; iCol--)
        {
          if (!String.Equals(Text[iRow, iCol], Text[iRow, iCol - 1], StringComparison.Ordinal))
            continue;
          if (RowSpan[iRow, iCol] == 0)
            continue; // ��� ������������ ������ �����

          if (RowSpan[iRow, iCol] != RowSpan[iRow, iCol - 1])
            continue;

          if (!mixedSpanAllowed)
          {
            // 24.08.2015
            // ������������� ����������� ������ ������, ����� � ��� ���� ���������� ��������, � � ���������� ������ - ������
            if (iRow > 0)
            {
              //if (Text[iRow - 1, iCol] != null)
              if (ColumnSpan[iRow - 1, iCol] > 0) // 01.06.2021
                continue;
            }
          }

          ColumnSpan[iRow, iCol - 1] += ColumnSpan[iRow, iCol];
          // ���������� ����� ������� �����
          for (int k = 0; k < RowSpan[iRow, iCol - 1]; k++)
          {
            // ���������� 01.06.2021
            Text[iRow + k, iCol] = null;
            RowSpan[iRow + k, iCol] = 0;
            ColumnSpan[iRow + k, iCol] = 0;
          }
        } // iCol
      } // iRow

      #endregion
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ����� � ����������.
    /// �� ����, ������� ����� ��������� �������� �� ����� Excel, ����� ���������� ���������
    /// </summary>
    public int RowCount { get { return _RowCount; } }
    private int _RowCount;

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    public int ColumnCount { get { return _ColumnCount; } }
    private int _ColumnCount;

    /// <summary>
    /// ����� �����. ������ ������ - ������, ������ - �������
    /// ����� ��������� ������� �������� ������ \r\n, ������� ������� ��������
    /// � ������������ �������
    /// </summary>
    public string[,] Text { get { return _Text; } }
    private string[,] _Text;

    /// <summary>
    /// ����� ������������ �����. ����������� ��� ������ ������ �����������,
    /// ��� ��������� - 0. ��� ������� (��������������) ������ ����� 1
    /// </summary>
    public int[,] RowSpan { get { return _RowSpan; } }
    private int[,] _RowSpan;

    /// <summary>
    /// ����� ������������ ��������. ����������� ��� ������� ������� �����������,
    /// ��� ��������� - 0. ��� �������� (���������������) ������� ����� 1
    /// </summary>
    public int[,] ColumnSpan { get { return _ColumnSpan; } }
    private int[,] _ColumnSpan;

    /// <summary>
    /// ���������� ����� "RowCount x ColumnCount"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return RowCount.ToString() + "x" + ColumnCount.ToString();
    }

    #endregion
  }
}