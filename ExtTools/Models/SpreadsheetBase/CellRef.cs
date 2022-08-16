// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace FreeLibSet.Models.SpreadsheetBase
{
  /// <summary>
  /// ����������� ������ ��� ����������� ������
  /// </summary>
  public static class SpreadsheetTools
  {

    #region ������ ��������

    /// <summary>
    /// �������������� ������ ������� (1 - 65535) � ������� "A1" ("A", "B" .., "Z", "AA", "AB..", "ZZ", "AAA", ...).
    /// </summary>
    /// <param name="columnNumber">����� �������, ������� � 1</param>
    /// <returns>��������� ����������� �������</returns>
    /// <remarks>    
    /// ��� ����������� �������������� ������ ������ � ������� ����������� ��������� CellRef
    /// </remarks>
    public static string GetColumnName(int columnNumber)
    {
      if (columnNumber < 1)
        throw new ArgumentOutOfRangeException("columnNumber", columnNumber, "����� ������� ������ ���� ������ 0");

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
    /// �������������� ����������� ������� � ������� "A1" � ����� �������: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// ���� ������ ������ ��� null, ���������� ArgumentNullException. ���� ������ �������� �������, �������� �� "A"-"Z", ������������
    /// FormatException. ���� ����� ������ int.MaxValue, ������������ OverflowException
    /// </summary>
    /// <param name="columnName">��������� ����������� �������</param>
    /// <returns>����� �������, ������� � 1</returns>
    /// <remarks>    
    /// ��� ����������� �������������� ������ ������ � ������� ����������� ��������� CellRef
    /// </remarks>
    public static int GetColumnNumber(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

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
    /// �������������� ����������� ������� � ������� "A1" � ����� �������: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// ���� ������ ������ ��� �� ����� ���� �������������, ������������ false
    /// </summary>
    /// <param name="columnName">��������� ����������� �������</param>
    /// <param name="columnNumber">��������� �������������� (����� �������) ��� 0 � ������ ������</param>
    /// <returns>true, ���� �������������� ������� ���������</returns>
    /// <remarks>    
    /// ��� ����������� �������������� ������ ������ � ������� ����������� ��������� CellRef
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
  }

  /// <summary>
  /// �������� ������ ������ (������ � �������).
  /// ��������� ���������� � 1.
  /// �������� ������� ������� (65535 �����, 255 ��������) �� �����������
  /// </summary>
  [Serializable]
  public struct CellRef : IEquatable<CellRef>
  {
    #region �����������

    /// <summary>
    /// ������� ����������� �����.
    /// </summary>
    /// <param name="row">����� ������. ��������� ���������� � 1</param>
    /// <param name="column">����� �������. ��������� ���������� � 1</param>
    public CellRef(int row, int column)
    {
      if (row < 1)
        throw new ArgumentOutOfRangeException("row", row, "����� ������ ������ ���� ������ 0");
      if (column < 1)
        throw new ArgumentOutOfRangeException("column", column, "����� ������� ������ ���� ������ 0");

      _Row = row;
      _Column = column;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������. ��������� ���������� � 1
    /// </summary>
    public int Row { get { return _Row; } }
    private readonly int _Row;

    /// <summary>
    /// ����� �������. ��������� ���������� � 1
    /// </summary>
    public int Column { get { return _Column; } }
    private readonly int _Column;

    /// <summary>
    /// ���������� true, ���� ��������� �� ���� ����������������
    /// </summary>
    public bool IsEmpty { get { return _Row == 0; } }

    /// <summary>
    /// �������������������� ���������
    /// </summary>
    public static readonly CellRef Empty = new CellRef();

    #endregion

    #region ���������

    /// <summary>
    /// ���������� true, ���� ������ ����������
    /// </summary>
    /// <param name="a">������ ������������ �����</param>
    /// <param name="b">������ ������������ �����</param>
    /// <returns>��������� ���������</returns>
    public static bool operator ==(CellRef a, CellRef b)
    {
      return a.Row == b.Row && a.Column == b.Column;
    }

    /// <summary>
    /// ���������� true, ���� ������ ������
    /// </summary>
    /// <param name="a">������ ������������ �����</param>
    /// <param name="b">������ ������������ �����</param>
    /// <returns>��������� ���������</returns>
    public static bool operator !=(CellRef a, CellRef b)
    {
      return a.Row != b.Row || a.Column != b.Column;
    }

    /// <summary>
    /// ���������� true, ���� ������ ����������
    /// </summary>
    /// <param name="other">������ ������������ �����</param>
    /// <returns>��������� ���������</returns>
    public bool Equals(CellRef other)
    {
      return this == other;
    }

    /// <summary>
    /// ���������� true, ���� ������ ����������
    /// </summary>
    /// <param name="other">������ ������������ �����</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object other)
    {
      if (other is CellRef)
        return this == (CellRef)other;
      else
        return false;
    }

    /// <summary>
    /// ���-��� ��� ���������
    /// </summary>
    /// <returns>�������� ���</returns>
    public override int GetHashCode()
    {
      return ((Row & 0xFFFF) << 8) | (Column & 0xFFFF);
    }

    #endregion

    #region �������������� � ������ / �� ������

    /// <summary>
    /// ���������� ��������� ������������� � ������� "A1".
    /// ��� IsEmpty=true ���������� ������ ������.
    /// </summary>
    /// <returns>�������� �������������</returns>
    public override string ToString()
    {
      if (_Row == 0)
        return String.Empty;
      return SpreadsheetTools.GetColumnName(Column) + Row.ToString();
    }

    /// <summary>
    /// �������������� ������ � ������� "A1".
    /// ������ ������ ������������� � �������������������� ��������� ��� ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>���������</returns>
    public static CellRef Parse(string s)
    {
      CellRef res;
      if (TryParse(s, out res))
        return res;
      else
        throw new FormatException();
    }

    /// <summary>
    /// ������� �������������� ������ � ������� "A1" � ����� ������.
    /// ������ ������ ������������� � �������������������� ��������� ��� ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="res">��������� �������������� ��� �������������������� ���������, � ������ ������</param>
    /// <returns>true, ���� �������������� ������� ���������</returns>
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

      // ���� ���������, ��� ���� ������ �����.
      // ������ ������ �� ����� ���� 0.

      if (s[nChars] < '1' || s[nChars] > '9')
      {
        res = Empty;
        return false;
      }

      // ������ ����� ������������ ����������� ��������������
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

    #endregion
  }

  /// <summary>
  /// ������������� �������� �����
  /// </summary>
  [Serializable]
  public struct RangeRef : IEquatable<RangeRef>
  {
    #region ������������

    /// <summary>
    /// ������� �������� �� ������ � ��������� ������.
    /// ���� ������ <paramref name="cell2"/> ��������� ���� ��� ����� <paramref name="cell1"/>, ��
    /// ��� ����� ��������� ���������� ��������.
    /// </summary>
    /// <param name="cell1">������ ������</param>
    /// <param name="cell2">������ ������</param>
    public RangeRef(CellRef cell1, CellRef cell2)
    {
      if (cell1.IsEmpty || cell2.IsEmpty)
        throw new ArgumentException("cells are empty");

      int firstRow = Math.Min(cell1.Row, cell2.Row);
      int lastRow = Math.Max(cell1.Row, cell2.Row);
      int firstColumn = Math.Min(cell1.Column, cell2.Column);
      int lastColumn = Math.Max(cell1.Column, cell2.Column);

      _FirstCell = new CellRef(firstRow, firstColumn);
      _LastCell = new CellRef(lastRow, lastColumn);
    }

    /// <summary>
    /// ������� ��������� ��������.
    /// ���� ������ ��� ������� ������ � ������������ �������, �� ��� �������������� �������
    /// </summary>
    /// <param name="firstRow">������ ������</param>
    /// <param name="firstColumn">������ �������</param>
    /// <param name="lastRow">��������� ������</param>
    /// <param name="lastColumn">��������� �������</param>
    public RangeRef(int firstRow, int firstColumn, int lastRow, int lastColumn)
      : this(new CellRef(firstRow, firstColumn), new CellRef(lastRow, lastColumn))
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ����� ������ ���������
    /// </summary>
    public CellRef FirstCell { get { return _FirstCell; } }
    private readonly CellRef _FirstCell;

    /// <summary>
    /// ������ ������ ������ ���������
    /// </summary>
    public CellRef LastCell { get { return _LastCell; } }
    private readonly CellRef _LastCell;

    /// <summary>
    /// ���������� ���������� ����� � ��������
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
    /// ���������� ���������� �������� � ���������
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
    /// ���������� ���������� ����� � ���������
    /// </summary>
    public int CellCount
    {
      get { return RowCount * ColumnCount; }
    }

    /// <summary>
    /// ���������� true, ���� ��������� �� ���� ����������������
    /// </summary>
    public bool IsEmpty { get { return _FirstCell.IsEmpty; } }

    /// <summary>
    /// �������������������� ���������
    /// </summary>
    public static readonly RangeRef Empty = new RangeRef();

    #endregion

    #region ���������

    /// <summary>
    /// ��������� ���� ����������
    /// </summary>
    /// <param name="a">������ ������������ ��������</param>
    /// <param name="b">������ ������������ ��������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator ==(RangeRef a, RangeRef b)
    {
      return a.FirstCell == b.FirstCell && a.LastCell == b.LastCell;
    }

    /// <summary>
    /// ��������� ���� ����������
    /// </summary>
    /// <param name="a">������ ������������ ��������</param>
    /// <param name="b">������ ������������ ��������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator !=(RangeRef a, RangeRef b)
    {
      return a.FirstCell != b.FirstCell || a.LastCell != b.LastCell;
    }

    /// <summary>
    /// ��������� � ������ ����������
    /// </summary>
    /// <param name="other">������ ������������ ��������</param>
    /// <returns>��������� ���������</returns>
    public bool Equals(RangeRef other)
    {
      return this == other;
    }

    /// <summary>
    /// ��������� � ������ ����������
    /// </summary>
    /// <param name="obj">������ ������������ ��������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      if (obj is RangeRef)
        return this == (RangeRef)obj;
      else
        return false;
    }

    /// <summary>
    /// ��������� ���-���� ��� ���������
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return FirstCell.GetHashCode();
    }

    #endregion

    #region �������������� �� ������ / � �����

    /// <summary>
    /// ���������� ������� � ���� "A1:B2".
    /// ���� �������� �������� ���� ������, �� ����������� �� ������������, ������������ "A1".
    /// ���� IsEmpty=true, ������������ ������ ������.
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return String.Empty;

      if (FirstCell == LastCell)
        return FirstCell.ToString();
      else
        return FirstCell.ToString() + ":" + LastCell.ToString();
    }

    /// <summary>
    /// ��������� �������������� ������ ���� "A1:B10" ��� "A1" � ��������.
    /// ��� ������ ������ ������������ �������������������� ��������� ��� ������� ����������.
    /// ���� ������ ����� ������������ ������, ������������� ���������� FormatException.
    /// � ���������, �� ����������� �������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>��������� ��������������</returns>
    public static RangeRef Parse(string s)
    {
      RangeRef value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException();
    }

    /// <summary>
    /// ��������� ������� �������������� ������ ���� "A1:B10" ��� "A1" � ��������.
    /// ���� ������ ����� ������������ ������, �� ������������ false.
    /// � ���������, �� ����������� ������ ������ � �������.
    /// ��� ������ ������ ������������ �������������������� ��������� ��� ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="value">���� ������������ ��������� ��������������</param>
    /// <returns>True, ���� �������������� ���������</returns>
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

    #region ���������

    /// <summary>
    /// ���������� true, ���� ������� �������� �������� ��������� ������
    /// </summary>
    /// <param name="cell">����������� ������</param>
    /// <returns>��������� ������ � ��������</returns>
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
    /// ���������� true, ���� ������� �������� ��������� �������� � ���� ������ ��������
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
    /// ���������� true, ���� ��� ��������� ������������
    /// </summary>
    /// <param name="a">������ ����������� ��������</param>
    /// <param name="b">������ ����������� ��������</param>
    /// <returns>������� ����� �����</returns>
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
    /// ���������� ����������� ���� ����������.
    /// ���� ��������� �� ������������, ������������ ������ ��������
    /// </summary>
    /// <param name="a">������ ����������� ��������</param>
    /// <param name="b">������ ����������� ��������</param>
    /// <returns>�����������</returns>
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