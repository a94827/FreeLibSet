using System;
using System.Collections.Generic;
using System.Text;

namespace AgeyevAV
{
	partial class DataTools
	{
    #region CommaString

    #region ���� ������

    /// <summary>
    /// �������������� ������, ���������� ��������, ����������� ��������.
    /// ������ �� ������ ��������� �������� �������� ������. ���� ������ �����
    /// ��������� ��������� �����, ����������� CommaStringToArray2().
    /// �������� ������� �������������. ����� ������������� ������� ����� � ������
    /// �� ������ ������� (���� ��� �� ������ �������). ���� ������ ������ ��� �� �������� ������, ����� ��������,
    /// �� ������������ null.
    /// ������� ������ ������� ��������� ������ ������. ������� ������� ����������
    /// �� ���������. � ������ ��������� ��������� CSV ���������� ����������
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>������ �����-���������</returns>
    /// <exception cref="AgeyevAV.ParsingException">��������� ��������� CSV</exception>
    public static string[] CommaStringToArray(string s)
    {
      return CommaStringToArray(s, ',');
    }

    /// <summary>
    /// �������������� ������, ���������� ��������, ����������� �������� ��� ������ ������������, ��������, ����������.
    /// ������ �� ������ ��������� �������� �������� ������. ���� ������ �����
    /// ��������� ��������� �����, ����������� CommaStringToArray2().
    /// �������� ������� �������������. ����� ������������� ������� ����� � ������
    /// �� ������� ����������� (���� ��� �� ������ �������). ���� ������ ������ ��� �� �������� ������, ����� ��������,
    /// �� ������������ null.
    /// ����������� ������ ������� ��������� ������ ������. ������� ������� ����������
    /// �� ���������. 
    /// </summary>
    /// <param name="s">������</param>
    /// <param name="fieldDelimiter"></param>
    /// <returns>������ �����-���������</returns>
    /// <exception cref="AgeyevAV.ParsingException">��������� ��������� CSV</exception>
    public static string[] CommaStringToArray(string s, char fieldDelimiter)
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

      if (s.IndexOf(fieldDelimiter) < 0)
      {
        // 10.11.2012
        // ������ �� �������� �����������

        if (s.Length < 3 || s[0] != '\"' || s[s.Length - 1] != '\"')
          return new string[1] { s };
      }

      string FieldDelimiterText;
      switch (fieldDelimiter)
      {
        case '\t': FieldDelimiterText = "<Tab>"; break;
        case ' ': FieldDelimiterText = "<������>"; break;
        default:
          FieldDelimiterText = fieldDelimiter.ToString();
          break;
      }

      List<string> lst = new List<string>();
      StringBuilder sb = new StringBuilder();
      int Phase = PhaseWaitStr;
      for (int i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case ' ':
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
            break;
          case '"':
            switch (Phase)
            {
              case PhaseWaitStr:
                Phase = PhaseInStrQ;
                break;
              case PhaseWaitSep:
                throw new ParsingException("� ������� " + (i + 1).ToString() + " ����������� ������ \". �������� ����������� \"" + FieldDelimiterText + "\"");
              case PhaseInStrNQ:
                // ���� ����� ������� �� � �������, �� ������� � ������ �� ������������
                sb.Append('"');
                break;
              case PhaseInStrQ:
                if (i < (s.Length - 1) && s[i + 1] == '"')
                {
                  sb.Append('"');
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
            break;
          default:
            if (s[i] == fieldDelimiter)
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
                  if (s[i] == '\n')
                  {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
                      sb.Append('\r');
                  }
                  sb.Append(s[i]);
                  Phase = PhaseInStrNQ;
                  break;
                case PhaseWaitSep:
                  throw new ParsingException("� ������� " + (i + 1).ToString() + " ����������� ������ \"" + s[i] + "\". �������� ����������� \"" + FieldDelimiterText + "\"");
                case PhaseInStrNQ:
                case PhaseInStrQ:
                  if (s[i] == '\n')
                  {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '\r')
                      sb.Append('\r');
                  }
                  sb.Append(s[i]);
                  break;
              }
            }
            break;
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

    private static string[] SimpleCommaStringToArray(string s, char fieldDelimiter)
    {
      string[] a = s.Split(fieldDelimiter);
      for (int i = 0; i < a.Length; i++)
        a[i] = a[i].Trim();
      return a;
    }

    /// <summary>
    /// �������������� ����������� ������� ����� � ������ � ������������-�������.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// ���� � �������� ������� ���� ������� ��� �������, ������� ����������� � �������,
    /// � ���������� ������� �����������.
    /// </summary>
    /// <param name="a">������ ��� �������������� � ������</param>
    /// <returns>CSV-������</returns>
    public static string CommaStringFromArray(string[] a)
    {
      return CommaStringFromArray(a, ',');
    }

    /// <summary>
    /// �������������� ����������� ������� ����� � ������ � ��������� ������������.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// ���� � �������� ������� ���� ������-����������� ��� �������, ������� ����������� � �������,
    /// � ���������� ������� �����������.
    /// </summary>
    /// <param name="a">������ ��� �������������� � ������</param>
    /// <param name="fieldDelimiter">������-�����������</param>
    /// <returns>CSV-������</returns>
    public static string CommaStringFromArray(string[] a, char fieldDelimiter)
    {
      if (a == null)
        return String.Empty;
      if (a.Length == 0)
        return String.Empty; // 16.11.2016

      StringBuilder sb = new StringBuilder();
      CommaStringFromArray(sb, a, fieldDelimiter);
      return sb.ToString();
    }

    /// <summary>
    /// �������������� ����������� ������� ����� � ������ � ������������-�������.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// ���� � �������� ������� ���� ������� ��� �������, ������� ����������� � �������,
    /// � ���������� ������� �����������.
    /// ��� ������ ���������� StringBuilder ��� ���������� ������.
    /// </summary>
    /// <param name="sb">���� ������������ CSV-������</param>
    /// <param name="a">������ ��� �������������� � ������</param>
    public static void CommaStringFromArray(StringBuilder sb, string[] a)
    {
      CommaStringFromArray(sb, a, ',');
    }


    /// <summary>
    /// �������������� ����������� ������� ����� � ������ � ��������� ������������.
    /// ���� <paramref name="a"/>=null ��� ������, ������������ ������ ������.
    /// ���� � �������� ������� ���� ������-����������� ��� �������, ������� ����������� � �������,
    /// � ���������� ������� �����������.
    /// ��� ������ ���������� StringBuilder ��� ���������� ������.
    /// </summary>
    /// <param name="sb">���� ������������ CSV-������</param>
    /// <param name="a">������ ��� �������������� � ������</param>
    /// <param name="fieldDelimiter">������-�����������</param>
    public static void CommaStringFromArray(StringBuilder sb, string[] a, char fieldDelimiter)
    {
      if (a == null)
        return; // 16.11.2016

      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(fieldDelimiter);

        if (!String.IsNullOrEmpty(a[i]))
        {
          if (a[i].IndexOf(fieldDelimiter) > 0 || a[i].IndexOf('\"') >= 0)
          {
            // � ���� ���� ������-�����������
            // �������� ����� � �������, ������� � ������ ���������
            sb.Append('\"');
            if (a[i].IndexOf('\"') >= 0)
            {
              // ���� ������� ��� ����������
              for (int j = 0; j < a[i].Length; j++)
              {
                if (a[i][j] == '\"')
                  sb.Append("\"\"");
                else
                  sb.Append(a[i][j]);
              }
            }
            else
              // ��� �������
              sb.Append(a[i]);
            sb.Append('\"');
          }
          else
            // ������ �� �������� ������� �����������
            sb.Append(a[i]);
        }
      }
    }

    private static void SimpleCommaStringFromArray(StringBuilder sb, string[] a, char fieldDelimiter)
    {
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(fieldDelimiter);

        if (!String.IsNullOrEmpty(a[i]))
          sb.Append(a[i]);
      }
    }



    /// <summary>
    /// �������������� ������ � ������ ���������������. ������ ������ ���������
    /// ������ �����, ����������� ��������. ������� �������������
    /// ������������ null, ���� ������ ������ ��� �������� ������ �������
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>������ ����� ��� null</returns>
    public static Int32[] CommaStringToIds(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Int32[] Res = new Int32[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = Int32.Parse(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// ��������� ������ �����, ����������� �������� �� ������� ���������������.
    /// ���� ������ <paramref name="ids"/> ������ ��� ������ ����� null, ������������ ������ ������.
    /// </summary>
    /// <param name="ids">������ ��������������� (����� ���� null)</param>
    /// <param name="addSpace">�������� ������� ����� �������</param>
    /// <returns>������</returns>
    public static string CommaStringFromIds(Int32[] ids, bool addSpace)
    {
      if (ids == null)
        return String.Empty;
      if (ids.Length == 0)
        return String.Empty;
      if (ids.Length == 1)
        return ids[0].ToString(); // ����� �� ��������� ������
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < ids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(ids[i]);
      }
      return sb.ToString();
    }

    /// <summary>
    /// �������������� ������ � ������ ��������������� GUID. ������ ������ ���������
    /// ������ ��������������� GUID, ����������� ��������. ������� �������������
    /// ������������ null, ���� ������ ������ ��� �������� ������ �������
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>������ ��������������� ��� null</returns>
    public static Guid[] CommaStringToGuids(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Guid[] Res = new Guid[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = new Guid(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// ��������� ������ � GUID'���, ����������� �������� �� ������� ���������������
    /// </summary>
    /// <param name="guids">������ ��������������� (����� ���� null)</param>
    /// <param name="addSpace">�������� ������� ����� �������</param>
    /// <returns>������</returns>
    public static string CommaStringFromGuids(Guid[] guids, bool addSpace)
    {
      if (guids == null)
        return String.Empty;
      if (guids.Length == 0)
        return String.Empty;
      if (guids.Length == 1)
        return guids[0].ToString(); // ����� �� ��������� ������
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < guids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(guids[i].ToString());
      }
      return sb.ToString();
    }

    #endregion

    #region ����� -> ������

    /// <summary>
    /// �������������� ������ CSV � ��������� ������ �����. ������ �����������
    /// ��������� CR+LF, � ������� - ��������.
    /// ���� ������ �� �������������, �� ������������� ����������
    /// </summary>
    /// <param name="s">������ � ������� CSV</param>
    /// <returns>��������� ������ �����</returns>
    public static string[,] CommaStringToArray2(string s)
    {
      return CommaStringToArray2(s, ',', String.Empty);
    }


    /// <summary>
    /// �������������� ������ CSV � ��������� ������ �����. ������ �����������
    /// ��������� CR � LF � ����� �� ���������� AllPossibleLineSeparators (������������ �������������), � ������� - ��������� ������������.
    /// ���� ������ �� �������������, �� ������������� ����������
    /// </summary>
    /// <param name="s">������ � ������� CSV</param>
    /// <param name="fieldDelimiter">����������� ����� � ������</param>
    /// <returns>��������� ������ �����</returns>
    public static string[,] CommaStringToArray2(string s, char fieldDelimiter)
    {
      return CommaStringToArray2(s, fieldDelimiter, String.Empty);
    }

    /// <summary>
    /// �������������� ������ CSV � ��������� ������ �����. ������ �����������
    /// ��������� CR+LF, � ������� - ��������� ������������.
    /// ���� ������ �� �������������, �� ������������� ����������
    /// </summary>
    /// <param name="s">������ � ������� CSV</param>
    /// <param name="fieldDelimiter">����������� ����� � ������</param>
    /// <param name="newLine">������� �������� ������. ���� �� ������, ������������ �������������</param>
    /// <returns>��������� ������ �����</returns>
    public static string[,] CommaStringToArray2(string s, char fieldDelimiter, string newLine)
    {
      if (String.IsNullOrEmpty(s))
        return null;

      if (String.IsNullOrEmpty(newLine))
      {
        newLine = GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(newLine))
          newLine = Environment.NewLine;
      }

      if (s.EndsWith(newLine))
        s = s.Substring(0, s.Length - newLine.Length);

      string[] a1 = s.Split(new string[] { newLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = CommaStringToArray2Internal(a1, fieldDelimiter, false);
      }
      catch
      {
        a2 = CommaStringToArray2Internal(a1, fieldDelimiter, true);
      }

      return a2;
    }

    private static string[,] CommaStringToArray2Internal(string[] a1, char fieldDelimiter, bool isSimple)
    {
      int n = 0;
      string[][] a3 = new string[a1.Length][];
      for (int i = 0; i < a1.Length; i++)
      {
        try
        {
          if (isSimple)
            a3[i] = SimpleCommaStringToArray(a1[i], fieldDelimiter);
          else
            a3[i] = CommaStringToArray(a1[i], fieldDelimiter);
        }
        catch (Exception e)
        {
          throw new ParsingException("������ � ������ " + (i + 1).ToString() + ". " + e.Message);
        }

        if (a3[i] != null && a3[i].Length > 0)
        {
          if (n == 0)
            n = a3[i].Length;
          else
          {
            if (a3[i].Length != n)
              throw new InvalidOperationException("����� �������� � ������ " + (i + 1).ToString() + " (" + a3.Length.ToString() +
                ") �� ��������� � ������ �������� � ������ ������ (" + n.ToString() + ")");
          }
        }
      }



      string[,] a2 = new string[a1.Length, n];
      for (int i = 0; i < a1.Length; i++)
      {
        if (a3[i] == null)
          continue;
        int n2 = Math.Min(a3[i].Length, n);
        for (int j = 0; j < n2; j++)
          a2[i, j] = a3[i][j];
      }

      return a2;
    }

    /// <summary>
    /// �������� ��������� ������ �� ������ � ������������ ������� - ����������.
    /// ������� �������� ������ ������������ �������������
    /// </summary>
    /// <param name="s"></param>
    /// <returns>��������� ������ �����</returns>
    public static string[,] TabbedStringToArray2(string s)
    {
      return TabbedStringToArray2(s, String.Empty);
    }

    /// <summary>
    /// �������� ��������� ������ �� ������ � ��������� ������������� ����� CR/LF �
    /// ������������ ������� - ����������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="newLine">����������� �����. ���� �� �����, ������������ ������������� �� ������������� ������</param>
    /// <returns>��������� ������</returns>
    public static string[,] TabbedStringToArray2(string s, string newLine)
    {
      if (String.IsNullOrEmpty(s))
        return null;


      if (String.IsNullOrEmpty(newLine))
      {
        newLine = GetNewLineSeparators(s);
        if (String.IsNullOrEmpty(newLine))
          newLine = Environment.NewLine;
      }

      if (s.EndsWith(newLine))
        s = s.Substring(0, s.Length - newLine.Length); // 24.06.2019 ���� �������� 04.06.2019


      string[] a1 = s.Split(new string[] { newLine }, StringSplitOptions.None);
      string[,] a2;

      try
      {
        a2 = TabbedStringToArray2Internal(a1, false);
      }
      catch
      {
        a2 = TabbedStringToArray2Internal(a1, true);
      }

      return a2;
    }

    private static string[,] TabbedStringToArray2Internal(string[] a1, bool isSimple)
    {
      string[,] a2 = null;
      int n = 0;
      for (int i = 0; i < a1.Length; i++)
      {
        string[] a3;
        try
        {
          if (isSimple)
            a3 = SimpleCommaStringToArray(a1[i], '\t');
          else
            a3 = CommaStringToArray(a1[i], '\t');
          if (a3 == null)
            a3 = new string[1] { "" };
        }
        catch (Exception e)
        {
          throw new ParsingException("������ � ������ " + (i + 1).ToString() + ". " + e.Message, e);
        }
        if (i == 0)
        {
          a2 = new string[a1.Length, a3.Length];
          n = a3.Length;
        }
        else
        {
          if (a3.Length != n)
            throw new InvalidOperationException("����� �������� � ������ " + (i + 1).ToString() + " (" + a3.Length.ToString() +
              ") �� ��������� � ������ �������� � ������ ������ (" + n.ToString() + ")");
        }

        for (int j = 0; j < n; j++)
          a2[i, j] = a3[j];
      }
      return a2;
    }

    #endregion

    #region ������ -> �����

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� Environment.NewLine, ���� ����������� �������
    /// </summary>
    /// <param name="a">������ �������� ��������</param>
    /// <returns>������ CSV</returns>
    public static string CommaStringFromArray2(string[,] a)
    {
      return CommaStringFromArray2(a, ',', Environment.NewLine);
    }

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� Environment.NewLine, ���� ����������� ��������� ��������
    /// </summary>
    /// <param name="a">������ �������� ��������</param>
    /// <param name="fieldDelimiter">����������� �������</param>
    /// <returns>������ CSV</returns>
    public static string CommaStringFromArray2(string[,] a, char fieldDelimiter)
    {
      return CommaStringFromArray2(a, fieldDelimiter, Environment.NewLine);
    }

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� CR+LF, ���� ����������� ��������� ��������
    /// </summary>
    /// <param name="a">������ �������� ��������</param>
    /// <param name="fieldDelimiter">����������� �������</param>
    /// <param name="newLine">����������� �����. ������ ������������ Environment.NewLine</param>
    /// <returns>������ CSV</returns>
    public static string CommaStringFromArray2(string[,] a, char fieldDelimiter, string newLine)
    {
      StringBuilder sb = new StringBuilder();
      CommaStringFromArray2(sb, a, fieldDelimiter, newLine);
      return sb.ToString();
    }

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� Environment.NewLine, ���� ����������� �������
    /// </summary>
    /// <param name="sb">����������� StringBuilder</param>
    /// <param name="a">������ �������� ��������</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a)
    {
      CommaStringFromArray2(sb, a, ',', Environment.NewLine);
    }


    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� Environment.NewLine, ���� ����������� ��������� ��������
    /// </summary>
    /// <param name="sb">����������� StringBuilder</param>
    /// <param name="a">������ �������� ��������</param>
    /// <param name="fieldDelimiter">����������� �������</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a, char fieldDelimiter)
    {
      CommaStringFromArray2(sb, a, fieldDelimiter, Environment.NewLine);
    }

    /// <summary>
    /// ���������� ������ � ������� CSV ��� ���������� �������.
    /// ������ ����������� ��������� <paramref name="newLine"/>, ���� ����������� ��������� �������� <paramref name="fieldDelimiter"/>
    /// </summary>
    /// <param name="sb">����������� StringBuilder</param>
    /// <param name="a">������ �������� ��������</param>
    /// <param name="fieldDelimiter">����������� �������</param>
    /// <param name="newLine">����������� �����. ���� �� �����, ������������ Environment.NewLine</param>
    public static void CommaStringFromArray2(StringBuilder sb, string[,] a, char fieldDelimiter, string newLine)
    {
      if (a == null)
        return;
      if (String.IsNullOrEmpty(newLine))
        newLine = Environment.NewLine;

      int n = a.GetLength(0);
      int m = a.GetLength(1);

      string[] b = new string[m];

      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
          b[j] = a[i, j];
        CommaStringFromArray(sb, b, fieldDelimiter);
        sb.Append(newLine);
      }
    }

    /// <summary>
    /// �������������� ���������� ������� � ������ � ������������-����������. 
    /// ������, ���������� �������, ����������� � �������, ���� ������� �����������.
    /// ������ ����������� ��������� Environment.NewLine.
    /// </summary>
    /// <param name="a">�������� ��������� ������</param>
    /// <returns>��������������� ������</returns>
    public static string TabbedStringFromArray2(string[,] a)
    {
      return TabbedStringFromArray2(a, false, Environment.NewLine);
    }

    /// <summary>
    /// �������������� ���������� ������� � ������ � ������������-����������. 
    /// ������ ����������� ��������� Environment.NewLine.
    /// </summary>
    /// <param name="a">�������� ��������� ������</param>
    /// <param name="simpleValues">���� true, �� �������������� �������� �� �����������. ���� false, �� ������, ���������� �������, ����������� � �������, ���� ������� �����������</param>
    /// <returns>��������������� ������</returns>
    public static string TabbedStringFromArray2(string[,] a, bool simpleValues)
    {
      return TabbedStringFromArray2(a, simpleValues, Environment.NewLine);
    }

    /// <summary>
    /// �������������� ���������� ������� � ������ � ������������-����������. 
    /// </summary>
    /// <param name="a">�������� ��������� ������</param>
    /// <param name="simpleValues">���� true, �� �������������� �������� �� �����������. ���� false, �� ������, ���������� �������, ����������� � �������, ���� ������� �����������</param>
    /// <param name="newLine">������� �������� ������. ������ ������� ������������ Environment.NewLine</param>
    /// <returns>��������������� ������</returns>
    public static string TabbedStringFromArray2(string[,] a, bool simpleValues, string newLine)
    {
      StringBuilder sb = new StringBuilder();
      TabbedStringFromArray2(sb, a, simpleValues, newLine);
      return sb.ToString();
    }


    /// <summary>
    /// �������������� ���������� ������� � ������ � ������������-����������. ������ ��� StringfBuilder
    /// </summary>
    /// <param name="sb">���� ������������ ��������������� ������</param>
    /// <param name="a">�������� ��������� ������</param>
    /// <param name="simpleValues">���� true, �� �������������� �������� �� �����������. ���� false, �� ������, ���������� �������, ����������� � �������, ���� ������� �����������</param>
    public static void TabbedStringFromArray2(StringBuilder sb, string[,] a, bool simpleValues)
    {
      TabbedStringFromArray2(sb, a, simpleValues, Environment.NewLine);
    }

    /// <summary>
    /// �������������� ���������� ������� � ������ � ������������-����������. ������ ��� StringfBuilder
    /// </summary>
    /// <param name="sb">���� ������������ ��������������� ������</param>
    /// <param name="a">�������� ��������� ������</param>
    /// <param name="simpleValues">���� true, �� �������������� �������� �� �����������. ���� false, �� ������, ���������� �������, ����������� � �������, ���� ������� �����������</param>
    /// <param name="newLine">������� �������� ������. ���� �� ������, ������������ Environment.NewLine</param>
    public static void TabbedStringFromArray2(StringBuilder sb, string[,] a, bool simpleValues, string newLine)
    {
      if (a == null)
        return;

      if (String.IsNullOrEmpty(newLine))
        newLine = Environment.NewLine;

      int n = a.GetLength(0);
      int m = a.GetLength(1);

      string[] b = new string[m];

      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
          b[j] = a[i, j];
        if (simpleValues)
          SimpleCommaStringFromArray(sb, b, '\t');
        else
          CommaStringFromArray(sb, b, '\t');
        sb.Append(newLine);
      }
    }

    #endregion

    #endregion
  }
}
