using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Text
{
  /// <summary>
  /// �������������� ������ CSV �/�� ����������� � ���������� ������� �����.
  /// RFC 4180.
  /// https://datatracker.ietf.org/doc/html/rfc4180
  /// ����������: �������� NewLine
  /// ����� �� �������� ����������������, �.�. ����� ������������ ���������� ���� � �������� ��������������
  /// </summary>
  public class CsvTextConvert
  {
    #region �����������

    /// <summary>
    /// ������� ��������������� � ����������� �� ���������
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

    #region ����������� ��������

    /// <summary>
    /// ������-����������� ����� � �������� ������.
    /// �� ��������� - �������
    /// </summary>
    public char FieldDelimiter
    {
      get { return _FieldDelimiter; }
      set
      {
        _FieldDelimiter = value;
        _QuotingRequiredChars = null; // ���������� ��������������
      }
    }
    private char _FieldDelimiter;

    /// <summary>
    /// ����������� �����. �� ��������� - Environment.NewLine.
    /// ��������! � RFC 4180 ������������ ����������� CR+LF. ��� ������������ ��������� �� ��-Windows ����������
    /// ������� ���������� �������� �������
    /// </summary>
    public string NewLine
    {
      get { return _NewLine; }
      set
      {
        _NewLine = value;
        _QuotingRequiredChars = null; // ���������� ��������������
      }
    }
    private string _NewLine;

    /// <summary>
    /// ����� �� ������������� ���������� ������� ����� ������ ��� �������������� ������ CSV � ��������� ������.
    /// �� ��������� - false - ������������ ������� �������� �������� NewLine.
    /// ���� ���������� � true, �� �������� NewLine ������� ����� �������� ��� ������ ToArray2().
    /// </summary>
    public bool AutoDetectNewLine
    {
      get { return _AutoDetectNewLine; }
      set { _AutoDetectNewLine = value; }
    }
    private bool _AutoDetectNewLine;

    /// <summary>
    /// ������ �������
    /// </summary>
    public char Quote
    {
      get { return _Quote; }
      set
      {
        _Quote = value;
        _QuotingRequiredChars = null; // ���������� ��������������
      }
    }
    private char _Quote;

    /// <summary>
    /// ���� true, �� ���� ����� ������ ����������� � �������, ������� ������ ����.
    /// �� ��������� - false, ������� �������� ������ ��� �������������: ��� ������� � ���� �������, ������� ��� �������� �����.
    /// ������ ������ �� ������ ToString(). ��� ������� CSV-������ �������� �� �����������.
    /// </summary>
    public bool AlwaysQuote
    {
      get { return _AlwaysQuote; }
      set { _AlwaysQuote = value; }
    }
    private bool _AlwaysQuote;

    #endregion

    #region ������ -> CSV

    #region ���������� ������

    /// <summary>
    /// ���������� �����.
    /// ������������ ��� ������� ToString(), ������������ string
    /// </summary>
    private StringBuilder _SB;

    /// <summary>
    /// �������������� ����������� ������� ����� � ������ CSV.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// </summary>
    /// <param name="a">������ ����� ��� �������������� � ������</param>
    /// <returns>CSV-������</returns>
    public string ToString(string[] a)
    {
      if (_SB == null)
        _SB = new StringBuilder();
      _SB.Length = 0;

      ToString(_SB, a);
      return _SB.ToString();
    }

    /// <summary>
    /// �������������� ����������� ������� ����� � ������ � ��������� ������������.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// ���� � �������� ������� ���� ������-����������� ��� �������, ������� ����������� � �������,
    /// � ���������� ������� �����������.
    /// ��� ������ ���������� StringBuilder ��� ���������� ������.
    /// </summary>
    /// <param name="sb">���� ������������ CSV-������</param>
    /// <param name="a">������ ����� ��� �������������� � ������</param>
    /// <param name="fieldDelimiter">������-�����������</param>
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
          // � ���� ���� ������-�����������
          // �������� ����� � �������, ������� � ������ ���������
          sb.Append(Quote);
          if (fld.IndexOf(Quote) >= 0)
          {
            // ���� ������� ��� ����������
            for (int j = 0; j < fld.Length; j++)
            {
              if (fld[j] == Quote)
                sb.Append(Quote);
              sb.Append(fld[j]);
            }
          }
          else
            // ��� �������
            sb.Append(fld);
          sb.Append(Quote);
        }
        else
          // ������ �� �������� ������� �����������
          sb.Append(fld);
      }
    }

    private ArrayIndexer<char> _QuotingRequiredChars; // �������� ������������ ArrayIndexer, � �� CharArrayIndexer

    /// <summary>
    /// ����� �� ��������� ���� � �������?
    /// </summary>
    /// <param name="fld">����</param>
    /// <returns>������������� �������</returns>
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

    #region ��������� ������

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// </summary>
    /// <param name="a">������ �������� ��������</param>
    /// <returns>������ CSV</returns>
    public string ToString(string[,] a)
    {
      if (_SB == null)
        _SB = new StringBuilder();
      _SB.Length = 0;
      ToString(_SB, a);
      return _SB.ToString();
    }

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// </summary>
    /// <param name="sb">����������� StringBuilder</param>
    /// <param name="a">������ �������� ��������</param>
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

    #region CSV->������

    #region ���������� ������

    /// <summary>
    /// ��� ������ ����������
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
    /// �������������� ������, ���������� ��������, ����������� �������� ��� ������ ������������, ��������, ����������.
    /// ������ �� ������ ��������� �������� �������� ������ (��� ���� � ��������). ���� ������ �����
    /// ��������� ��������� �����, ����������� ToArray2() ��� �������������� � ��������� ������.
    /// �������� ������� �������������. ����� ������������� ������� ����� � ������
    /// �� ������� ����������� (���� ��� �� ������ �������). 
    /// ���� ������ ������ ��� �� �������� ������, ����� ��������, �� ������������ null.
    /// ����������� ������ ������� ��������� ������ ������.
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>������ �����-���������</returns>
    /// <exception cref="FreeLibSet.Core.ParsingException">��������� ��������� CSV</exception>
    public string[] ToArray(string s)
    {
      const int PhaseWaitStr = 1;   // ���� ������ ������
      const int PhaseInStrNQ = 2;     // ������ ���������� ������ ��� �������
      const int PhaseInStrQ = 3;     // ������ ���������� ������ � ��������
      const int PhaseWaitSep = 4; // ��������� ��������� ������ - ���� �������

      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim(); // ������� ������� �����
      if (s.Length == 0)
        return null;

      // 12.11.2021 ���� �� ������
      //if (s.IndexOf(fieldDelimiter) < 0)
      //{
      //  // 10.11.2012
      //  // ������ �� �������� �����������

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
              throw new ParsingException("� ������� " + (i + 1).ToString() + " ����������� ������ \". �������� ����������� \"" + FieldDelimiterText + "\"");
            case PhaseInStrNQ:
              // ���� ����� ������� �� � �������, �� ������� � ������ �� ������������
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
                lst.Add(sb.ToString()); // ��� �������
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
              // ������ ������ - ��� ������� ������
              lst.Add("");
              break;
            case PhaseWaitSep:
              Phase = PhaseWaitStr;
              break;
            case PhaseInStrNQ:
              // ������ ���������
              lst.Add(sb.ToString().Trim());
              sb.Length = 0;
              Phase = PhaseWaitStr;
              break;
            case PhaseInStrQ:
              sb.Append(s[i]);
              break;
          }
        }
        else // ������� ������
        {
          switch (Phase)
          {
            case PhaseWaitStr:
              // 12.11.2021 �� ������
              //if (s[i] == '\n')
              //{
              //  if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
              //    sb.Append('\r');
              //}
              sb.Append(s[i]);
              Phase = PhaseInStrNQ;
              break;
            case PhaseWaitSep:
              throw new ParsingException("� ������� " + (i + 1).ToString() + " ����������� ������ \"" + s[i] + "\". �������� ����������� \"" + FieldDelimiterText + "\"");
            case PhaseInStrNQ:
            case PhaseInStrQ:
              // 12.11.2021 �� ������
              //if (s[i] == '\n')
              //{
              //  if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
              //    sb.Append('\r');
              //}
              sb.Append(s[i]);
              break;
          }
        }
      } // ����� ����� �� ������
      switch (Phase)
      {
        case PhaseInStrNQ:
          lst.Add(sb.ToString().Trim());
          break;
        case PhaseWaitStr:
          lst.Add(String.Empty);
          break;
        case PhaseInStrQ:
          throw new ParsingException("����������� ����� ������. �� ������� ����������� �������");
      }

      return lst.ToArray();
    }

    #endregion

    #region ��������� ������

    /// <summary>
    /// �������������� ������ CSV � ��������� ������ �����. ������ �����������
    /// ��������� CR+LF, � ������� - ��������� ������������.
    /// ���� ������ �� �������������, �� ������������� ����������
    /// ���� ������ ������, ������������ null.
    /// </summary>
    /// <param name="s">������ � ������� CSV</param>
    /// <returns>��������� ������ �����</returns>
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
          throw new ParsingException("������ � ������ " + (i + 1).ToString() + ". " + e.Message);
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
