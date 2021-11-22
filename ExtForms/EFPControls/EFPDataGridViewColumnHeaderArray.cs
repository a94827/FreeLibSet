// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

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
