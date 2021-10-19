using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections;

namespace AgeyevAV
{
  partial class DataTools
  {
    #region ����������� �� ��������

    #region ReplaceDigits

    /// <summary>
    /// �������� ��� �������� ������� � ������ �� �������� ������
    /// </summary>
    /// <param name="str">������</param>
    /// <param name="c">������, �� ������� ����� �������� �������� �������</param>
    /// <returns>������ ����� ������</returns>
    public static string ReplaceDigits(string str, char c)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      StringBuilder s1 = null; // ��������, ���� �����
      for (int i = 0; i < str.Length; i++)
      {
        if (str[i] >= '0' && str[i] <= '9')
        {
          if (s1 == null)
          {
            s1 = new StringBuilder(str.Length);
            if (i > 0)
              s1.Append(str.Substring(0, i)); // ��� ���������� �������
          }
          s1.Append(c);
        }
        else
          if (s1 != null)
            s1.Append(str[i]);
      }
      if (s1 == null)
        return str;
      else
        return s1.ToString();
    }

    #endregion

    #region PadXXX

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ������ �� ������ ����� <paramref name="length"/> �������� <paramref name="paddingChar"/>.
    /// �-������� �� String.PadRight(), ��������� ��������� ������, ���� ��� �������,
    /// ��� Length. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <param name="paddingChar">������-�����������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadRight(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        return s.Substring(0, length);
      return s.PadRight(length, paddingChar);
    }

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ������ �� ������ ����� <paramref name="length"/> ���������.
    /// �-������� �� String.PadRight(), ��������� ��������� ������, ���� ��� �������,
    /// ��� Length. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadRight(string s, int length)
    {
      return PadRight(s, length, ' ');
    }

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ����� �� ������ ����� <paramref name="length"/> �������� <paramref name="paddingChar"/>.
    /// �-������� �� String.PadRight(), ��������� ��������� ������, ���� ��� �������,
    /// ��� Length. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <param name="paddingChar">������-�����������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadLeft(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        return s.Substring(s.Length - length);
      return s.PadLeft(length, paddingChar);
    }

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ����� �� ������ ����� <paramref name="length"/> ���������.
    /// �-������� �� String.PadRight(), ��������� ��������� ������, ���� ��� �������,
    /// ��� Length. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadLeft(string s, int length)
    {
      return PadLeft(s, length, ' ');
    }

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ����� � ������ �� ������ ����� <paramref name="length"/> �������� <paramref name="paddingChar"/>.
    /// ��������� ��������� ��������� � �������� �������� ������,
    /// ���� ��� �������, ��� <paramref name="length"/>. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <param name="paddingChar">������-�����������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadCenter(string s, int length, char paddingChar)
    {
      if (length <= 0)
        return String.Empty;
      if (String.IsNullOrEmpty(s))
        return new string(paddingChar, length);
      if (s.Length == length)
        return s;
      if (s.Length > length)
        //return s.Substring(s.Length - length);
        return s.Substring(0, length); // 14.07.2021

      // ���������� ��������� � ����� ������
      int n1 = (length - s.Length) / 2;
      int n2 = length - s.Length - n1;
      return new string(paddingChar, n1) + s + new string(paddingChar, n2);
    }

    /// <summary>
    /// ���������� ������ <paramref name="s"/> ����� � ������ �� ������ ����� <paramref name="length"/> ���������.
    /// ��������� ��������� ��������� � �������� �������� ������,
    /// ���� ��� �������, ��� <paramref name="length"/>. ����� ��������� �������������� <paramref name="s"/>=null.
    /// ������������ ������ ������ ����� ����� ����� <paramref name="length"/> ��������. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <returns>������ ������ <paramref name="length"/> ��������</returns>
    public static string PadCenter(string s, int length)
    {
      return PadCenter(s, length, ' ');
    }

    #endregion

    #region StrXXX

    /// <summary>
    /// ���������� ������ <paramref name="length"/> �������� �� ������ <paramref name="s"/>.
    /// ������ Clipper-������� LEFT()
    /// ������ ����� ���� ������, null ��� ������ <paramref name="length"/>. � ���� ������ ������������
    /// ������ ������� ����� ��� String.Empty. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <returns>������ ������ �� ����� Length ��������</returns>
    public static string StrLeft(string s, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.Length <= length)
        return s;
      return s.Substring(0, length);
    }

    /// <summary>
    /// ���������� ��������� <paramref name="length"/> �������� �� ������ <paramref name="s"/>.
    /// ������ Clipper-������� RIGHT()
    /// ������ ����� ���� ������, null ��� ������ <paramref name="length"/>. � ���� ������ ������������
    /// ������ ������� ����� ��� String.Empty. �������� null ���������� �� String.Empty.
    /// </summary>
    /// <param name="s">�������� ������. ����� ���� null</param>
    /// <param name="length">��������� ����� ������. ���� ������ ��� ����� 0,
    /// �� ����� ���������� ������ ������</param>
    /// <returns>������ ������ �� ����� <paramref name="length"/> ��������</returns>
    public static string StrRight(string s, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.Length <= length)
        return s;
      return s.Substring(s.Length - length);
    }

    #endregion

    #region �������/������ ������� �������

    // ����� ������������ ���� ������� Xxx() � XxxInvariant()

    #region ToUpperFirst()

    /// <summary>
    /// �������������� ������� ������� ������ � �������� ��������, � ��������� - � �������
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="culture">��������, ������������ ��� ������ ������� String.ToUpper() � ToLower()</param>
    /// <returns>������ � ���������� ��������� ��������</returns>
    public static string ToUpperFirst(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      else
        return s.Substring(0, 1).ToUpper(culture) + s.Substring(1).ToLower(culture);
    }

    /// <summary>
    /// �������������� ������� ������� ������ � �������� ��������, � ��������� - � �������.
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>������ � ���������� ��������� ��������</returns>
    public static string ToUpperFirstInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      else
        return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1).ToLowerInvariant();
    }

    #endregion

    #region ChangeUpperLower()

    /// <summary>
    /// �������� ����� �������� �������� �� ����� ������� �������� � ��������.
    /// ��������, "Hello, world!" ����� �������� �� "hELLO, wORLD!".
    /// <param name="culture">��������, ������������ ��� ������ ������� String.ToUpper() � ToLower()</param>
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>������ � ��������� ���������</returns>
    public static string ChangeUpperLower(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        if (char.IsUpper(s, i))
          a[i] = char.ToLower(s[i], culture);
        else if (char.IsLower(s, i))
          a[i] = char.ToUpper(s[i], culture);
        else
          a[i] = s[i];
      }

      return new string(a);
    }

    /// <summary>
    /// �������� ����� �������� �������� �� ����� ������� �������� � ��������.
    /// ��������, "Hello, world!" ����� �������� �� "hELLO, wORLD!".
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>������ � ��������� ���������</returns>
    public static string ChangeUpperLowerInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        if (char.IsUpper(s, i))
          a[i] = char.ToLowerInvariant(s[i]);
        else if (char.IsLower(s, i))
          a[i] = char.ToUpperInvariant(s[i]);
        else
          a[i] = s[i];
      }

      return new string(a);
    }

    #endregion

    #region ToUpperWords()

    /// <summary>
    /// ����������� ������ ������� ������� ����� � �������� ��������, � ��������� - � �������.
    /// �������� ����� �������� ����� ����������� ������, ��� �������� Char.IsLetter() ���������� false.
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="culture">��������, ������������ ��� ������ ������� String.ToUpper() � ToLower()</param>
    /// <returns>��������������� �����</returns>
    public static string ToUpperWords(string s, CultureInfo culture)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];

      bool NextUpper = true;
      for (int i = 0; i < s.Length; i++)
      {
        if (Char.IsLetter(s[i]))
        {
          if (NextUpper)
          {
            a[i] = Char.ToUpper(s[i], culture);
            NextUpper = false;
          }
          else
          {
            a[i] = Char.ToLower(s[i], culture);
          }
        }
        else
        {
          NextUpper = true;
          a[i] = s[i]; // 14.07.2021
        }
      }

      return new string(a);
    }


    /// <summary>
    /// ����������� ������ ������� ������� ����� � �������� ��������, � ��������� - � �������.
    /// �������� ����� �������� ����� ����������� ������, ��� �������� Char.IsLetter() ���������� false
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>��������������� �����</returns>
    public static string ToUpperWordsInvariant(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      char[] a = new char[s.Length];

      bool NextUpper = true;
      for (int i = 0; i < s.Length; i++)
      {
        if (Char.IsLetter(s[i]))
        {
          if (NextUpper)
          {
            a[i] = Char.ToUpperInvariant(s[i]);
            NextUpper = false;
          }
          else
          {
            a[i] = Char.ToLowerInvariant(s[i]);
          }
        }
        else
        {
          NextUpper = true;
          a[i] = s[i]; // 14.07.2021
        }
      }

      return new string(a);
    }

    #endregion

    #region �������������� �������� ��� ������� �����

    /// <summary>
    /// ������� ����� ������� ����� <paramref name="a"/>, � ������� ����������� ��� ������ � �������� �������� � ������ ��������.
    /// �������� ������ <paramref name="a"/> �� ��������������.
    /// ���� <paramref name="a"/>==null, ������������ null.
    /// ���� � ������� ����������� �������� null, ��� ����� � � �������������� �������
    /// </summary>
    /// <param name="a">�������� ������ �����</param>
    /// <param name="culture">��������, ������������ ��� ��������������</param>
    /// <returns>����� ������� � ���������������� ��������</returns>
    public static string[] ToUpper(string[] a, CultureInfo culture)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToUpper(culture);
      }
      return b;
    }

    /// <summary>
    /// ������� ����� ������� ����� <paramref name="a"/>, � ������� ����������� ��� ������ � �������� ��������.
    /// �������� ������ <paramref name="a"/> �� ��������������.
    /// ���� <paramref name="a"/>==null, ������������ null.
    /// ���� � ������� ����������� �������� null, ��� ����� � � �������������� �������
    /// </summary>
    /// <param name="a">�������� ������ �����</param>
    /// <returns>����� ������� � ���������������� ��������</returns>
    public static string[] ToUpperInvariant(string[] a)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToUpperInvariant();
      }
      return b;
    }

    /// <summary>
    /// ������� ����� ������� ����� <paramref name="a"/>, � ������� ����������� ��� ������ � ������� �������� � ������ ��������.
    /// �������� ������ <paramref name="a"/> �� ��������������.
    /// ���� <paramref name="a"/>==null, ������������ null.
    /// ���� � ������� ����������� �������� null, ��� ����� � � �������������� �������
    /// </summary>
    /// <param name="a">�������� ������ �����</param>
    /// <param name="culture">��������, ������������ ��� ��������������</param>
    /// <returns>����� ������� � ���������������� ��������</returns>
    public static string[] ToLower(string[] a, CultureInfo culture)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToLower(culture);
      }
      return b;
    }

    /// <summary>
    /// ������� ����� ������� ����� <paramref name="a"/>, � ������� ����������� ��� ������ � �������� ��������.
    /// �������� ������ <paramref name="a"/> �� ��������������.
    /// ���� <paramref name="a"/>==null, ������������ null.
    /// ���� � ������� ����������� �������� null, ��� ����� � � �������������� �������
    /// </summary>
    /// <param name="a">�������� ������ �����</param>
    /// <returns>����� ������� � ���������������� ��������</returns>
    public static string[] ToLowerInvariant(string[] a)
    {
      if (a == null)
        return null;
      string[] b = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          b[i] = a[i].ToLowerInvariant();
      }
      return b;
    }

    #endregion

    #endregion

    #region AddStrIfNoEmpty

    /// <summary>
    /// ��������� ������ <paramref name="addedStr"/> � ������ <paramref name="resStr"/>, ���� <paramref name="addedStr"/> ��������. 
    /// ����� <paramref name="addedStr"/> ����������� ��������� <paramref name="separator"/>. 
    /// ���� <paramref name="addedStr"/> - ������ ������, �� <paramref name="resStr"/> �������� ��� ���������. 
    /// ��������� �� �����������, ���� <paramref name="resStr"/> - ������ ������.
    /// ������������� ������������ ������ ��� StringBuilder.
    /// </summary>
    /// <param name="resStr">���������� ������ (�� ������)</param>
    /// <param name="addedStr">����������� ������</param>
    /// <param name="separator">�����������</param>
    public static void AddStrIfNotEmpty(ref string resStr, string addedStr, string separator)
    {
      if (String.IsNullOrEmpty(addedStr))
        return;

      if (String.IsNullOrEmpty(resStr))
        resStr = addedStr;
      else
      {
        if (!String.IsNullOrEmpty(separator))
          resStr += separator;
        resStr += addedStr;
      }
    }

    /// <summary>
    /// ��������� ������ <paramref name="addedStr"/> � ������ <paramref name="resStr"/>, ���� <paramref name="addedStr"/> ��������. 
    /// ����� <paramref name="addedStr"/> ����������� ��������� <paramref name="separator"/>. 
    /// ���� <paramref name="addedStr"/> - ������ ������, �� <paramref name="resStr"/> �������� ��� ���������. 
    /// ��������� �� �����������, ���� <paramref name="resStr"/> - ������ ������.
    /// ������ ��� StringBuilder.
    /// </summary>
    /// <param name="resStr">���������� ������ (�� ������)</param>
    /// <param name="addedStr">����������� ������</param>
    /// <param name="separator">�����������</param>
    public static void AddStrIfNotEmpty(StringBuilder resStr, string addedStr, string separator)
    {
      if (String.IsNullOrEmpty(addedStr))
        return;

      if (resStr.Length > 0 && (!String.IsNullOrEmpty(separator)))
        resStr.Append(separator);
      resStr.Append(addedStr);
    }

    #endregion

    #region ����������������� �������������

    /// <summary>
    /// �������������� ������� ������ � �������� ������ (��� ��������) �
    /// ����������������� �������. �������� ������ � 2 ���� �������, ���
    /// ����� ������� <paramref name="bytes"/>.
    /// ���� <paramref name="bytes"/>=null, �� ������������ ������ ������
    /// ����� ����� ��������������, ��������, ��� ������ ���-�����, �����������
    /// �� ��������� MD5
    /// </summary>
    /// <param name="bytes">������ ���� ������������ �����</param>
    /// <param name="upperCase">���� true, �� ������������ ����� "A"-"F",
    /// ���� false, �� "a"-"f"</param>
    /// <returns>������ 16-������ ��������</returns>
    public static string BytesToHex(byte[] bytes, bool upperCase)
    {
      if (bytes == null)
        return String.Empty;
      if (bytes.Length == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder(bytes.Length * 2);

      //for (int i = 0; i < bytes.Length; i++)
      //  sb.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
      BytesToHex(sb, bytes, upperCase); // 28.01.2020

      return sb.ToString();
    }

    /// <summary>
    /// �������������� ������� ������ � �������� ������ (��� ��������) �
    /// ����������������� �������. �������� ������ � 2 ���� �������, ���
    /// ����� ������� <paramref name="bytes"/>.
    /// ���� <paramref name="bytes"/>=null, �� ������������ ������ ������
    /// ����� ����� ��������������, ��������, ��� ������ ���-�����, �����������
    /// �� ��������� MD5
    /// </summary>
    /// <param name="sb">����� ��� ������</param>
    /// <param name="bytes">������ ���� ������������ �����</param>
    /// <param name="upperCase">���� true, �� ������������ ����� "A"-"F",
    /// ���� false, �� "a"-"f"</param>
    public static void BytesToHex(StringBuilder sb, byte[] bytes, bool upperCase)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
#endif

      if (bytes == null)
        return;

      string s16 = upperCase ? "0123456789ABCDEF" : "0123456789abcdef";

      for (int i = 0; i < bytes.Length; i++)
      {
        //sb.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
        // 28.01.2020
        sb.Append(s16[bytes[i] >> 4]);
        sb.Append(s16[bytes[i] & 0x0F]);
      }
    }

    /// <summary>
    /// �������������� ������������������ �������� 0-9,A-F,a-f � ������ ����.
    /// ������ ������ ��������� ������ ����� ��������.
    /// ���� ����� ������ - �������� ��� ������ �������� ������������ ������� - ������������� ArgumentException
    /// ������ ������ ������������� � ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>������ ����</returns>
    public static byte[] HexToBytes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return EmptyBytes;
      if ((s.Length % 2) != 0)
        throw new ArgumentException("������ �������� �������� ���������� �������� (" + s.Length.ToString() + ")", "s");
      byte[] a = new byte[s.Length / 2];
      for (int i = 0; i < a.Length; i++)
        a[i] = (byte)(GetBytePart(s[2 * i]) << 4 | GetBytePart(s[2 * i + 1]));
      return a;
    }

    private static int GetBytePart(char c)
    {
      if (c >= '0' && c <= '9')
        return (c - '0');
      if (c >= 'A' && c <= 'F')
        return (c - 'A') + 10;
      if (c >= 'a' && c <= 'f')
        return (c - 'a') + 10;

      throw new ArgumentException("������������ ������ \"" + c + "\"");
    }

    #endregion

    #region IndexOfAny

    /// <summary>
    /// ���������� true, ���� ����� ������ �������� ������� ����������� � ������������� CharArrayIndexer
    /// </summary>
    /// <param name="str"></param>
    /// <param name="searchChars"></param>
    /// <returns></returns>
    private static bool PreferCharArrayIndexer(string str, string searchChars)
    {
      return (str.Length > 2 && str.Length * searchChars.Length > 50);
    }


    /// <summary>
    /// ������ ����������� ������ String.IndexOfAny(), �������, � ������� �� ���������,
    /// ����������� ������� ����� �� ������, � �� �� �������
    /// ������� ���������� ������ ������� ������� �� ������ <paramref name="str"/>, ������� ������������
    /// � ������ <paramref name="searchChars"/>. ���� ������ <paramref name="str"/> ������ ��� ������� ������������� �� ��������,
    /// ������� ��� � ������ <paramref name="searchChars"/>, �� ������������ (-1)
    /// ����� �������� ��������-�����������, ��� ������� ������������ �� �� �����
    /// �������� ���� �� �����.
    /// ���� ��������� ������������ ����� � ����� � ��� �� ������� �������� <paramref name="searchChars"/>,
    /// �������� CharArrayIndexer � ����������� ������ ���������� ������.
    /// </summary>
    /// <param name="str">����������� ������</param>
    /// <param name="searchChars">������� �������</param>
    /// <returns>������ � ������ <paramref name="str"/></returns>
    public static int IndexOfAny(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(searchChars))
        return -1;

      if (PreferCharArrayIndexer(str, searchChars))
        return IndexOfAny(str, new CharArrayIndexer(searchChars));

      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// ������ ����������� ������ String.IndexOfAny(), �������, � ������� �� ���������,
    /// ����������� ������� ����� �� ������, � �� �� �������
    /// ������� ���������� ������ ������� ������� �� ������ <paramref name="str"/>, ������� ������������
    /// � ������ <paramref name="searchChars"/>. ���� ������ <paramref name="str"/> ������ ��� ������� ������������� �� ��������,
    /// ������� ��� � ������ <paramref name="searchChars"/>, �� ������������ (-1).
    /// ����� �������� ��������-�����������, ��� ������� ������������ �� �� �����
    /// �������� ���� �� �����.
    /// </summary>
    /// <param name="str">����������� ������</param>
    /// <param name="searchChars">������� �������</param>
    /// <returns>������ � ������ <paramref name="str"/></returns>
    public static int IndexOfAny(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str) || searchChars.Count == 0)
        return -1;
      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
          return i;
      }
      return -1;
    }


    /// <summary>
    /// �������, �������� � IndexOfAny().
    /// ������� ���������� ������ ������� ������� �� ������ <paramref name="str"/>, ������� �����������
    /// � ������ <paramref name="searchChars"/>. ���� ������ <paramref name="str"/> ������ ��� ������� ������������� �� ��������,
    /// ������� �������� � ������ <paramref name="searchChars"/>, �� ������������ (-1).
    /// ����� �������� ��������-�����������, ��� ������� ������������ �� �� �����
    /// �������� ���� �� �����
    /// ���� ������ <paramref name="str"/> ��������, � <paramref name="searchChars"/> - ������, �� ������������ �������� 0, �.�.
    /// ������ �� ������ �� ������ � ������
    /// ������� ������� ��� �������� ������������ ���� � ������ �������� �������. 
    /// � �������� ������ <paramref name="searchChars"/> ������� ���������� ������ ���� ���������� �������� �
    /// �������� �� ������, ���� ������� ������� ��������, ������� ��� ������ 0.
    /// ���� ��������� ������������ ����� � ����� � ��� �� ������� �������� <paramref name="searchChars"/>,
    /// �������� CharArrayIndexer � ����������� ������ ���������� ������.
    /// </summary>
    /// <param name="str">����������� ������</param>
    /// <param name="searchChars">������� �������</param>
    /// <returns>������ � ������ <paramref name="str"/></returns>
    public static int IndexOfAnyOther(string str, string searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (String.IsNullOrEmpty(searchChars))
        return 0; // ����� ������ ��������

      if (PreferCharArrayIndexer(str, searchChars))
        return IndexOfAnyOther(str, new CharArrayIndexer(searchChars));

      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// �������, �������� � IndexOfAny().
    /// ������� ���������� ������ ������� ������� �� ������ <paramref name="str"/>, ������� �����������
    /// � ������ <paramref name="searchChars"/>. ���� ������ <paramref name="str"/> ������ ��� ������� ������������� �� ��������,
    /// ������� �������� � ������ <paramref name="searchChars"/>, �� ������������ (-1)
    /// ����� �������� ��������-�����������, ��� ������� ������������ �� �� �����
    /// �������� ���� �� �����
    /// ���� ������ <paramref name="str"/> ��������, � <paramref name="searchChars"/> - ������, �� ������������ �������� 0, �.�.
    /// ������ �� ������ �� ������ � ������
    /// ������� ������� ��� �������� ������������ ���� � ������ �������� �������. 
    /// � �������� ������ <paramref name="searchChars"/> ������� ���������� ������ ���� ���������� �������� �
    /// �������� �� ������, ���� ������� ������� ��������, ������� ��� ������ 0.
    /// </summary>
    /// <param name="str">����������� ������</param>
    /// <param name="searchChars">������� �������</param>
    /// <returns>������ � ������ <paramref name="str"/></returns>
    public static int IndexOfAnyOther(string str, CharArrayIndexer searchChars)
    {
      if (String.IsNullOrEmpty(str))
        return -1;
      if (searchChars.Count == 0)
        return 0; // ����� ������ ��������
      int n = str.Length;
      for (int i = 0; i < n; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
          return i;
      }
      return -1;
    }

    #endregion

    #region ReplaceXXX

    /// <summary>
    /// ������ �������� �� �������
    /// </summary>
    /// <param name="str">������, � ������� ������������ ������</param>
    /// <param name="replaceDict">������� ��� ������. ������� �������� ���������� �������, ���������� - ����������</param>
    /// <returns>������ � ����������� ���������</returns>
    public static string ReplaceChars(string str, Dictionary<char, char> replaceDict)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

#if DEBUG
      if (replaceDict == null)
        throw new ArgumentNullException("Dict");
#endif
      if (replaceDict.Count == 0)
        return str;

      StringBuilder sb = null; // ����� ��������
      for (int i = 0; i < str.Length; i++)
      {
        char ResChar;
        if (replaceDict.TryGetValue(str[i], out ResChar))
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = ResChar; // ��������
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ������ ��������.
    /// ��� ������������ ������� ������������� ������������ ���������� ������, ����������� ��������
    /// ���� Dictionary.
    /// </summary>
    /// <param name="str">������, � ������� ������������ ������</param>
    /// <param name="searchChars">���������� �������</param>
    /// <param name="replaceChars">���������� �������</param>
    /// <returns>������ � ����������� ���������</returns>
    public static string ReplaceChars(string str, string searchChars, string replaceChars)
    {
      if (replaceChars.Length != searchChars.Length)
        throw new ArgumentException("����� ����� SearchChars � replaceChars ������ ���� ����������", "replaceChars");

      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Length == 1)
        return str.Replace(searchChars[0], replaceChars[0]);

      // ������ �� �������
      Dictionary<char, char> Dict = new Dictionary<char, char>(searchChars.Length);
      for (int i = 0; i < searchChars.Length; i++)
        Dict.Add(searchChars[i], replaceChars[i]);
      return ReplaceChars(str, Dict);
    }

    /// <summary>
    /// ������ ��������� �������� � ������ �� �������� ������
    /// �������� �������� ������������ �� ������ � ������� Unicode
    /// </summary>
    /// <param name="str">������, � ������� ������������ ������</param>
    /// <param name="firstChar">������ ������ ����������� ���������</param>
    /// <param name="lastChar">��������� ������ ����������� ���������</param>
    /// <param name="replaceChar">���������� ������</param>
    /// <returns>�������� ������ � ����������� �������</returns>
    public static string ReplaceCharRange(string str, char firstChar, char lastChar, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      StringBuilder sb = null; // ����� ��������
      for (int i = 0; i < str.Length; i++)
      {
        if (str[i] >= firstChar && str[i] <= lastChar)
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = replaceChar; // ��������
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ��������� ������ ������ �� �������� �������� <paramref name="searchChars"/> � ������ <paramref name="str"/> �� ������ <paramref name="replaceChar"/>
    /// ��� ������� ���������� �� ���� � ��� �� ������.
    /// ���� ��������� ������������ ����� � ����� � ��� �� ������� �������� <paramref name="searchChars"/>,
    /// �������� CharArrayIndexer � ����������� ������ ���������� ������.
    /// </summary>
    /// <param name="str">������, � ������� �������������� �����</param>
    /// <param name="searchChars">�������, ���������� ������</param>
    /// <param name="replaceChar">���������� ������</param>
    /// <returns>������ � ������������ ��������</returns>
    public static string ReplaceAny(string str, string searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (String.IsNullOrEmpty(searchChars))
        return str;

      if (PreferCharArrayIndexer(str, searchChars))
        return ReplaceAny(str, new CharArrayIndexer(searchChars), replaceChar);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ��������� ������ ������ �� �������� �������� <paramref name="searchChars"/> � ������ <paramref name="str"/> �� ������ <paramref name="replaceChar"/>
    /// ��� ������� ���������� �� ���� � ��� �� ������
    /// </summary>
    /// <param name="str">������, � ������� �������������� �����</param>
    /// <param name="searchChars">�������, ���������� ������</param>
    /// <param name="replaceChar">���������� ������</param>
    /// <returns>������ � ������������ ��������</returns>
    public static string ReplaceAny(string str, CharArrayIndexer searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Count == 0)
        return str;

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) >= 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ��������� ������ ���� ��������, �� �������� � �������� ������ <paramref name="searchChars"/> � ������ <paramref name="str"/> 
    /// �� ������ <paramref name="replaceChar"/>.
    /// ��� ������� ���������� �� ���� � ��� �� ������.
    /// ���� ��������� ������������ ����� � ����� � ��� �� ������� �������� <paramref name="searchChars"/>,
    /// �������� CharArrayIndexer � ����������� ������ ���������� ������.
    /// </summary>
    /// <param name="str">������, � ������� �������������� �����</param>
    /// <param name="searchChars">�������, ������� ������ �������� � ������</param>
    /// <param name="replaceChar">���������� ������</param>
    /// <returns>������ � ������������ ��������</returns>
    public static string ReplaceAnyOther(string str, string searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (String.IsNullOrEmpty(searchChars))
        return new string(replaceChar, str.Length);

      if (PreferCharArrayIndexer(str, searchChars))
        return ReplaceAnyOther(str, new CharArrayIndexer(searchChars), replaceChar);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ��������� ������ ���� ��������, �� �������� � �������� ������ <paramref name="searchChars"/> � ������ <paramref name="str"/> 
    /// �� ������ <paramref name="replaceChar"/>.
    /// ��� ������� ���������� �� ���� � ��� �� ������.
    /// </summary>
    /// <param name="str">������, � ������� �������������� �����</param>
    /// <param name="searchChars">�������, ������� ������ �������� � ������</param>
    /// <param name="replaceChar">���������� ������</param>
    /// <returns>������ � ������������ ��������</returns>
    public static string ReplaceAnyOther(string str, CharArrayIndexer searchChars, char replaceChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      if (searchChars.Count == 0)
        return new string(replaceChar, str.Length);

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (searchChars.IndexOf(str[i]) < 0)
        {
          if (sb == null)
            sb = new StringBuilder(str); // ������ ����� ������
          sb[i] = replaceChar;
        }
      }
      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    #endregion

    #region RemoveChars, RemoveOtherChars

    /// <summary>
    /// ������� �� ������ <paramref name="str"/> ��� �������, �������� � ������ <paramref name="removedChars"/>.
    /// ���� ����� ������ ����������� ���������� ��� ������ � ���� �� ������ �������� <paramref name="removedChars"/>,
    /// ������������� ���������� ������� CharArrayIndexer � ������������ ������ ���������� ������
    /// </summary>
    /// <param name="str">�������� ������</param>
    /// <param name="removedChars">C������, ������� ����� ���������� � ������</param>
    /// <returns>������ <paramref name="str"/> ��� ������ ������� �����</returns>
    public static string RemoveChars(string str, string removedChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (String.IsNullOrEmpty(removedChars))
        //return String.Empty;
        return str; // 14.07.2021

      if (PreferCharArrayIndexer(str, removedChars))
        return RemoveChars(str, new CharArrayIndexer(removedChars));

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (removedChars.IndexOf(str[i]) >= 0)
        {
          // ������ ���������

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // ��� ���������� �������
          }
        }
        else
        {
          // ������ ����������� � ������

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ������� �� ������ <paramref name="str"/> ��� �������, �������� � ������ <paramref name="removedChars"/>.
    /// </summary>
    /// <param name="str">�������� ������</param>
    /// <param name="removedChars">C������, ������� ����� ���������� � ������</param>
    /// <returns>������ <paramref name="str"/> ��� ������ ������� �����</returns>
    public static string RemoveChars(string str, CharArrayIndexer removedChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (removedChars.Count == 0)
        //return String.Empty;
        return str; // 14.07.2021

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (removedChars.Contains(str[i]))
        {
          // ������ ���������

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // ��� ���������� �������
          }
        }
        else
        {
          // ������ ����������� � ������

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ������� �� ������ <paramref name="str"/> ��� �������, ����� �������� � ������ <paramref name="validChars"/>.
    /// ���� ����� ������ ����������� ���������� ��� ������ � ���� �� ������ �������� <paramref name="validChars"/>,
    /// ������������� ���������� ������� CharArrayIndexer � ������������ ������ ���������� ������
    /// </summary>
    /// <param name="str">�������� ������</param>
    /// <param name="validChars">C������, ������� ����� ���������� � ������</param>
    /// <returns>������ <paramref name="str"/> ��� ������ ������� �����</returns>
    public static string RemoveOtherChars(string str, string validChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (String.IsNullOrEmpty(validChars))
        //return str; 
        // 14.07.2021
        return String.Empty; // ��� �������� ������� ���� ���� �� ��������

      if (PreferCharArrayIndexer(str, validChars))
        return RemoveOtherChars(str, new CharArrayIndexer(validChars));

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (validChars.IndexOf(str[i]) < 0)
        {
          // ������ ���������

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // ��� ���������� �������
          }
        }
        else
        {
          // ������ ����������� � ������

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ������� �� ������ <paramref name="str"/> ��� �������, ����� �������� � ������ <paramref name="validChars"/>.
    /// </summary>
    /// <param name="str">�������� ������</param>
    /// <param name="validChars">C������, ������� ����� ���������� � ������</param>
    /// <returns>������ <paramref name="str"/> ��� ������ ������� �����</returns>
    public static string RemoveOtherChars(string str, CharArrayIndexer validChars)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;
      if (validChars.Count == 0)
        //return str; 
        // 14.07.2021
        return String.Empty; // ��� �������� ������� ���� ���� �� ��������

      StringBuilder sb = null;
      for (int i = 0; i < str.Length; i++)
      {
        if (!validChars.Contains(str[i]))
        {
          // ������ ���������

          if (sb == null)
          {
            sb = new StringBuilder(str.Length - 1);
            if (i > 0)
              sb.Append(str, 0, i); // ��� ���������� �������
          }
        }
        else
        {
          // ������ ����������� � ������

          if (sb != null)
            sb.Append(str[i]);
        }
      }

      if (sb == null)
        return str;
      else
        return sb.ToString();
    }

    #endregion

    #region �������� �������� ��������

    /// <summary>
    /// ��������� ����� � ������ ������� � ����� ��������� �������� <paramref name="searchChar"/> � ������ <paramref name="str"/>.
    /// ����� ������������������ ���������� �� ���� ������.
    /// ���� ������� �������� �� �������, ������������ �������� ������.
    /// � ������� �� ������������ ������ String.Replace(), ���������� ������������������ �� ���� � �������� � �������.
    /// </summary>
    /// <param name="str">������ ��� ������</param>
    /// <param name="searchChar">������ ��� ������. ���� ����� - ������</param>
    /// <returns>������ � ���������� ���������</returns>
    public static string RemoveDoubleChars(string str, char searchChar)
    {
      if (String.IsNullOrEmpty(str))
        return String.Empty;

      int p = str.IndexOf(new string(searchChar, 2));
      if (p < 0)
        return str;

      StringBuilder sb = new StringBuilder(str.Length - 1);
      sb.Append(str, 0, p + 1); // ������� ������ �� ���� �������� � ����
      for (int i = p + 1; i < str.Length; i++)
      {
        char LastChar = sb[sb.Length - 1];
        if (LastChar == searchChar)
        {
          if (str[i] != LastChar)
            sb.Append(str[i]);
        }
        else
          sb.Append(str[i]);
      }
      return sb.ToString();
    }

    #endregion

    #region ����� ��� ����� ��������

    /// <summary>
    /// ����� ������ � ������� � ������������ ������������ ������� / ������ �������.
    /// ��� ��������� ������������ ������� String.Equals()
    /// </summary>
    /// <param name="a">������, � ������� ����������� �����</param>
    /// <param name="searchStr">������� ������</param>
    /// <param name="comparisonType">������ ��������� �����</param>
    /// <returns>������ ��������� ������ � ������� ��� (-1)</returns>
    public static int IndexOf(string[] a, string searchStr, StringComparison comparisonType)
    {
      for (int i = 0; i < a.Length; i++)
      {
        if (String.Equals(a[i], searchStr, comparisonType)) 
          return i;
      }
      return -1;
    }

    #endregion

    #region StrToCSharpString

    /// <summary>
    /// ����������� ������ � ����� C#.
    /// 1. �������� ������ ���������.
    /// 2. �������� ����������� �������, ��������� � ������� �� Escape-������������������.
    /// ����� ����, ���� �����-������ ������� �������?
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>������ � ��������</returns>
    public static string StrToCSharpString(string s)
    {
      if (s == null)
        return "null";
      StringBuilder sb = new StringBuilder(s.Length + 2);
      sb.Append('\"');
      for (int i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case '\t': sb.Append(@"\t"); break;
          case '\r': sb.Append(@"\r"); break;
          case '\n': sb.Append(@"\n"); break;
          case '\\': sb.Append(@"\\"); break;
          case '\"': sb.Append(@"\"""); break;
          case '\'': sb.Append(@"\'"); break;
          default:
            if (s[i] < ' ')
            {
              sb.Append('\\');
              sb.Append((int)(s[i]));
            }
            else
              sb.Append(s[i]); // ��� ����
            break;
        }
      }
      sb.Append('\"');
      return sb.ToString();
    }

    #endregion

    #region IsSubstring

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> �������� ��������� <paramref name="substring"/>,
    /// ������� � ������� <paramref name="startPos"/>.
    /// � ������� �� String.Compare(), �� ����������� ����������, ���� ����������� ������� ��������� 
    /// ������� �� ������� ������ <paramref name="s"/>. 
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="startPos">������� ������ ��������� ��� ��������. ����� ���� ������������� ��� ��������
    /// �� ������� ������</param>
    /// <param name="substring">����������� ���������</param>
    /// <param name="comparisonType">������ ��������� �����</param>
    /// <returns>���������� ���������</returns>
    public static bool IsSubstring(string s, int startPos, string substring, StringComparison comparisonType)
    {
      if (s == null)
        s = String.Empty;
      if (substring == null)
        substring = String.Empty;

      if (startPos < 0 || (startPos + substring.Length) > s.Length)
        return false;

      return String.Compare(s, startPos, substring, 0, substring.Length, comparisonType) == 0; // TODO: Equals()
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> �������� ��������� <paramref name="substring"/>,
    /// ������� � ������� <paramref name="startPos"/>.
    /// � ������� �� String.Compare(), �� ����������� ����������, ���� ����������� ������� ��������� 
    /// ������� �� ������� ������ <paramref name="s"/>. 
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="startPos">������� ������ ��������� ��� ��������. ����� ���� ������������� ��� ��������
    /// �� ������� ������</param>
    /// <param name="substring">����������� ���������</param>
    /// <param name="ignoreCase">���� true, �� ������� �������� ������������</param>
    /// <param name="culture">���������� � ��������, ������������ ��� ��������� �����</param>
    /// <returns>���������� ���������</returns>
    public static bool IsSubstring(string s, int startPos, string substring, bool ignoreCase, CultureInfo culture)
    {
      if (s == null)
        s = String.Empty;
      if (substring == null)
        substring = String.Empty;

      if (startPos < 0 || (startPos + substring.Length) > s.Length)
        return false;

      return String.Compare(s, startPos, substring, 0, substring.Length, ignoreCase, culture) == 0; // TODO: Equals()
    }

    #endregion

    #region ��������� ��� String.Split()

    /// <summary>
    /// ������ �� ����� ������, ���������� ���� �������� CR+LF, ���������� �� ������������ �������.
    /// ��� ������������� � ������ String.Split()
    /// </summary>
    public static readonly string[] CRLFSeparators = new string[] { "\r\n" };

    /// <summary>
    /// ������ �� ����� ������, ���������� ���� �������� CR+LF, ��� ������ �������, � ����������� �� ������������ �������.
    /// ������ �������� ������� �� Environment.NewLine
    /// ��� ������������� � ������ String.Split().
    /// </summary>
    public static readonly string[] NewLineSeparators = new string[] { Environment.NewLine };

    /// <summary>
    /// ������ �� ������� �����, ���������� ��������� ���������� �������� CR � LF.
    /// ������������� ���������� ���� ����� ���������������
    /// ��� ������������� � ������ String.Split().
    /// </summary>
    public static readonly string[] AllPossibleLineSeparators = new string[] { "\r\n", "\n\r", "\r", "\n" };

    #endregion

    #region GetNewLineSeparators

    /// <summary>
    /// ����������� ��������, ������� ������������ ��� �������� ������.
    /// ���������� ���� �� ����� � AllPossibleLineSeparators.
    /// ���� ������ ������ ��� �� �������� ������� �������� �����, �� ������������ ������ ������.
    /// � ���� ������ ������ ������� ������������ Environment.NewLine
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <returns>������� �������� ������</returns>
    public static string GetNewLineSeparators(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        if (s.Contains(AllPossibleLineSeparators[i]))
          return AllPossibleLineSeparators[i];
      }

      return String.Empty;
    }

    #endregion

    #region TrimStart/EndNewLineSeparators

    /// <summary>
    /// ���� ������ ���������� � �������� ����� ������, �� ��� ���������.
    /// ��������� ����� ������ ��������� ����� ���������� �� AllPossibleLineSeparators (���� ��� ��������� �������).
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="trimAll">���� true, �� ��������� ��� ���� �������� � ������ ������, ����� ��������� �� ����� ����� ����</param>
    /// <returns>����������� ��� �������� ������</returns>
    public static string TrimStartNewLineSeparators(string s, bool trimAll)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        bool flag = false;
        int st = 0;
        while (IsSubstring(s, st, AllPossibleLineSeparators[i], StringComparison.Ordinal))
        {
          flag = true;
          st += AllPossibleLineSeparators[i].Length;
          if (!trimAll)
            break;
        }
        if (flag)
          return s.Substring(st);
      }
      // ��� �����������
      return s;
    }
    /// <summary>
    /// ���� ������ ���������� � �������� ����� ������, �� ��� ���������.
    /// ��������� ����� ������ ��������� ������ �������� ���������� �������� <paramref name="newLine"/>.
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="trimAll">���� true, �� ��������� ��� ���� �������� � ������ ������, ����� ��������� �� ����� ����� ����</param>
    /// <param name="newLine">��������� ���������� ��������. ����������� Environment.NewLine. �� ����� ���� ������ �������</param>
    /// <returns>����������� ��� �������� ������</returns>
    public static string TrimStartNewLineSeparators(string s, bool trimAll, string newLine)
    {
      if (String.IsNullOrEmpty(newLine))
        throw new ArgumentNullException("newLine");

      if (String.IsNullOrEmpty(s))
        return String.Empty;
      bool flag = false;
      int st = 0;
      while (IsSubstring(s, st, newLine, StringComparison.Ordinal))
      {
        flag = true;
        st += newLine.Length;
        if (!trimAll)
          break;
      }
      if (flag)
        return s.Substring(st);
      // ��� �����������
      return s;
    }

    /// <summary>
    /// ���� ������ ������������� ��������� ����� ������, �� ��� ���������.
    /// ��������� ����� ������ ��������� ����� ���������� �� AllPossibleLineSeparators (���� ��� ��������� �������).
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="trimAll">���� true, �� ��������� ��� ���� �������� � ����� ������, ����� ��������� �� ����� ����� ����</param>
    /// <returns>����������� ��� �������� ������</returns>
    public static string TrimEndNewLineSeparators(string s, bool trimAll)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      for (int i = 0; i < AllPossibleLineSeparators.Length; i++)
      {
        bool flag = false;
        int l = s.Length;
        while (IsSubstring(s, l - AllPossibleLineSeparators[i].Length, AllPossibleLineSeparators[i], StringComparison.Ordinal))
        {
          flag = true;
          l -= AllPossibleLineSeparators[i].Length;
          if (!trimAll)
            break;
        }
        if (flag)
          return s.Substring(0, l);
      }
      // ��� �����������
      return s;
    }

    /// <summary>
    /// ���� ������ ������������� ��������� ����� ������, �� ��� ���������.
    /// ��������� ����� ������ ��������� ������ �������� ���������� �������� <paramref name="newLine"/>.
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <param name="trimAll">���� true, �� ��������� ��� ���� �������� � ����� ������, ����� ��������� �� ����� ����� ����</param>
    /// <param name="newLine">��������� ���������� ��������. ����������� Environment.NewLine. �� ����� ���� ������ �������</param>
    /// <returns>����������� ��� �������� ������</returns>
    public static string TrimEndNewLineSeparators(string s, bool trimAll, string newLine)
    {
      if (String.IsNullOrEmpty(newLine))
        throw new ArgumentNullException("newLine");

      if (String.IsNullOrEmpty(s))
        return String.Empty;
      bool flag = false;
      int l = s.Length;
      while (IsSubstring(s, l - newLine.Length, newLine, StringComparison.Ordinal))
      {
        flag = true;
        l -= newLine.Length;
        if (!trimAll)
          break;
      }
      if (flag)
        return s.Substring(0, l);
      // ��� �����������
      return s;
    }

    #endregion

    #region GetCharCount

    /// <summary>
    /// ������������ ���������� ��������� ������� <paramref name="searchChar"/> � ������ <paramref name="s"/>.
    /// ���� ������ ������ ��� null, ������������ 0
    /// </summary>
    /// <param name="s">������, � ������� ����������� �����</param>
    /// <param name="searchChar">������� ������</param>
    /// <returns>���������� ���������</returns>
    public static int GetCharCount(string s, char searchChar)
    {
      if (String.IsNullOrEmpty(s))
        return 0;
      int cnt = 0;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == searchChar)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region JoinNotEmptyStrings()

    /// <summary>
    /// �������� String.Join() ��� �������� ����� � ������� <paramref name="values"/>.
    /// ���� ������ ������ ��� �� �������� �������� �����, ������������ String.Empty
    /// </summary>
    /// <param name="separator">�����������</param>
    /// <param name="values">������������ ��������</param>
    /// <returns>������������ ������</returns>
    public static string JoinNotEmptyStrings(string separator, string[] values)
    {
      // �� ������ ������� ���������� ������� ������
      int nItems = 0;
      for (int i = 0; i < values.Length; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
          nItems++;
      }
      if (nItems == 0)
        return String.Empty;
      if (nItems == values.Length) // ��� �������� ��������
        return String.Join(separator, values);

      // ����� ����� ������, ����� - ���
      string[] a = new string[nItems];
      nItems = 0;
      for (int i = 0; i < values.Length; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
        {
          a[nItems] = values[i];
          nItems++;
        }
      }
      return String.Join(separator, a);
    }

    /// <summary>
    /// �������� String.Join() ��� �������� ����� � ������ <paramref name="values"/>.
    /// ���� ������ ������ ��� �� �������� �������� �����, ������������ String.Empty
    /// </summary>
    /// <param name="separator">�����������</param>
    /// <param name="values">������������ ��������</param>
    /// <returns>������������ ������</returns>
    public static string JoinNotEmptyStrings(string separator, IList<string> values)
    {
      // �� ������ ������� ���������� ������� ������
      int nItems = 0;
      for (int i = 0; i < values.Count; i++)
      {
        if (!String.IsNullOrEmpty(values[i]))
          nItems++;
      }
      if (nItems == 0)
        return String.Empty;

      // ����� ����� ������, ����� - ���
      string[] a = new string[nItems];
      if (nItems == values.Count) // ��� �������� ��������
        values.CopyTo(a, 0);
      else
      {
        nItems = 0;
        for (int i = 0; i < values.Count; i++)
        {
          if (!String.IsNullOrEmpty(values[i]))
          {
            a[nItems] = values[i];
            nItems++;
          }
        }
      }
      return String.Join(separator, a);
    }


    /// <summary>
    /// �������� String.Join() ��� �������� ����� � ������ <paramref name="values"/>.
    /// ���� ������ ������ ��� �� �������� �������� �����, ������������ String.Empty.
    /// </summary>
    /// <param name="separator">�����������</param>
    /// <param name="values">������������ ��������</param>
    /// <returns>������������ ������</returns>
    public static string JoinNotEmptyStrings(string separator, IEnumerable<string> values)
    {
      // ��� ������ �������� �������������.
      List<string> lst = new List<string>();
      foreach (string s in values)
      {
        if (!String.IsNullOrEmpty(s))
          lst.Add(s);
      }

      switch (lst.Count)
      {
        case 0:
          return String.Empty;
        case 1:
          return lst[0];
        default:
          return String.Join(separator, lst.ToArray());
      }
    }

    #endregion

    #region ������ ToString() ��� �������� � ������������

    #region ToStringArray()

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ���������� ������ ������������ � ���� �������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="a">������ ��� ������������</param>
    /// <returns>������ �����</returns>
    public static string[] ToStringArray<T>(T[] a)
    {
      string[] res = new string[a.Length];
      for (int i = 0; i < res.Length; i++)
        res[i] = a[i].ToString();
      return res;
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ���������� ������ ������������ � ���� �������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="lst">������ ��� ������������</param>
    /// <returns>������ �����</returns>
    public static string[] ToStringArray<T>(IList<T> lst)
    {
      string[] res = new string[lst.Count];
      for (int i = 0; i < res.Length; i++)
        res[i] = lst[i].ToString();
      return res;
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ���������� ������ ������������ � ���� �������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="en">������������ ������</param>
    /// <returns>������ �����</returns>
    public static string[] ToStringArray<T>(IEnumerable<T> en)
    {
      List<string> lst = null;
      foreach (T x in en)
      {
        if (lst == null)
          lst = new List<string>();
        lst.Add(x.ToString());
      }

      if (lst == null)
        return EmptyStrings;
      else
        return lst.ToArray();
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ���������� ������ ������������ � ���� �������.
    /// ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <param name="lst">������ ��� ������������</param>
    /// <returns>������ �����</returns>
    public static string[] ToStringArray(IList lst)
    {
      if (lst.Count == 0)
        return EmptyStrings;

      string[] res = new string[lst.Count];
      for (int i = 0; i < res.Length; i++)
        res[i] = lst[i].ToString();
      return res;
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ���������� ������ ������������ � ���� �������.
    /// ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <param name="en">������������ ������</param>
    /// <returns>������ �����</returns>
    public static string[] ToStringArray(IEnumerable en)
    {
      List<string> lst = null;
      foreach (object x in en)
      {
        if (lst == null)
          lst = new List<string>();
        lst.Add(x.ToString());
      }

      if (lst == null)
        return EmptyStrings;
      else
        return lst.ToArray();
    }

    #endregion

    #region ToStringJoin()

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ������������ ������������ ������ � ��������� ������������� <paramref name="separator"/>.
    /// ������������ ������ String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="a"/>)), �� �������� ����� �����������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="separator">����������� ����� ���������� (���������� ������������ � ������ String.Join())</param>
    /// <param name="a">������ ��� ������������</param>
    /// <returns>������������ ������</returns>
    public static string ToStringJoin<T>(string separator, T[] a)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(a[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ������������ ������������ ������ � ��������� ������������� <paramref name="separator"/>.
    /// ������������ ������ String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="lst"/>)), �� �������� ����� �����������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="separator">����������� ����� ���������� (���������� ������������ � ������ String.Join())</param>
    /// <param name="lst">������ ��� ������������</param>
    /// <returns>������������ ������</returns>
    public static string ToStringJoin<T>(string separator, IList<T> lst)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < lst.Count; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(lst[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ������������ ������������ ������ � ��������� ������������� <paramref name="separator"/>.
    /// ������������ ������ String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="en"/>)), �� �������� ����� �����������.
    /// ���� <paramtyperef name="T"/> �������� �������, � �� �������� �����, �� ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <typeparam name="T">��� ������������ ���������</typeparam>
    /// <param name="separator">����������� ����� ���������� (���������� ������������ � ������ String.Join())</param>
    /// <param name="en">������������ ������</param>
    /// <returns>������������ ������</returns>
    public static string ToStringJoin<T>(string separator, IEnumerable<T> en)
    {
      //StringBuilder sb = new StringBuilder();
      StringBuilder sb = null; // ���������� 14.07.2021
      foreach (T x in en)
      {
        if (sb == null)
          sb = new StringBuilder();
        else
          sb.Append(separator);
        sb.Append(x.ToString());
      }

      if (sb == null)
        return String.Empty;
      else
        return sb.ToString();
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ������������ ������������ ������ � ��������� ������������� <paramref name="separator"/>.
    /// ������������ ������ String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="lst"/>)), �� �������� ����� �����������.
    /// ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <param name="separator">����������� ����� ���������� (���������� ������������ � ������ String.Join())</param>
    /// <param name="lst">������ ��� ������������</param>
    /// <returns>������������ ������</returns>
    public static string ToStringJoin(string separator, IList lst)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < lst.Count; i++)
      {
        if (i > 0)
          sb.Append(separator);
        sb.Append(lst[i].ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// ��������� ������������ ��������� � ��� ������� �� ��� ���������� ����� ToString().
    /// ������������ ������������ ������ � ��������� ������������� <paramref name="separator"/>.
    /// ������������ ������ String.Join(<paramref name="separator"/>, ToStringArray(<paramref name="en"/>)), �� �������� ����� �����������.
    /// ������ �� ����� ��������� �������� null.
    /// </summary>
    /// <param name="separator">����������� ����� ���������� (���������� ������������ � ������ String.Join())</param>
    /// <param name="en">������������ ������</param>
    /// <returns>������������ ������</returns>
    public static string ToStringJoin(string separator, IEnumerable en)
    {
      //StringBuilder sb = new StringBuilder();
      StringBuilder sb = null; // ���������� 14.07.2021
      foreach (object x in en)
      {
        if (sb == null)
          sb = new StringBuilder();
        else
          sb.Append(separator);
        sb.Append(x.ToString());
      }

      if (sb == null)
        return String.Empty;
      else
        return sb.ToString();
    }

    #endregion

    #endregion

    #endregion

    #region �������� ����� � �������� �� �������

    /// <summary>
    /// �������� ������ ����� �� ����������� ������� �����.
    /// ������������ ������, ����� �������� � ������� (������ ������) ��������� �
    /// ������ ����� � �������� �������, � ����� ����� ����� ���� ������.
    /// ���� �������� ������ �� �������� �� ������ ��������, ������������ null.
    /// ���� ������ ����� ���, �� ������������ �������� ������
    /// </summary>
    /// <param name="a">�������� ��������� ������ �����</param>
    /// <returns>��������������� ������</returns>
    public static string[,] RemoveEmptyRows(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] RowFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
            RowFlags[i] = true;
        }
      }

      int nRows2 = 0;
      int[] RowRefs = new int[nRows];
      for (int i = 0; i < nRows; i++)
      {
        if (RowFlags[i])
        {
          RowRefs[i] = nRows2;
          nRows2++;
        }
        else
          RowRefs[i] = -1;
      }

      if (nRows2 == 0)
        return null;
      if (nRows2 == nRows)
        return a;

      string[,] a2 = new string[nRows2, nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (RowRefs[i] >= 0)
            a2[RowRefs[i], j] = a[i, j];
        }
      }

      return a2;
    }

    /// <summary>
    /// �������� ������ �������� �� ����������� ������� �����.
    /// ������������ ������, ����� ����� � ������� (������ ������) ��������� �
    /// ������ ����� � �������� �������, � ����� �������� ����� ���� ������.
    /// ���� �������� ������ �� �������� �� ������ ��������, ������������ null.
    /// ���� ������ �������� ���, �� ������������ �������� ������
    /// </summary>
    /// <param name="a">�������� ��������� ������ �����</param>
    /// <returns>��������������� ������</returns>
    public static string[,] RemoveEmptyColumns(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] ColFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
            ColFlags[j] = true;
        }
      }

      int nCols2 = 0;
      int[] ColRefs = new int[nCols];
      for (int j = 0; j < nCols; j++)
      {
        if (ColFlags[j])
        {
          ColRefs[j] = nCols2;
          nCols2++;
        }
        else
          ColRefs[j] = -1;
      }

      if (nCols2 == 0)
        return null;
      if (nCols2 == nCols)
        return a;

      string[,] a2 = new string[nRows, nCols2];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (ColRefs[j] >= 0)
            a2[i, ColRefs[j]] = a[i, j];
        }
      }

      return a2;
    }

    /// <summary>
    /// �������� ������ ����� � �������� �� ����������� ������� �����.
    /// ������������ ������, ����� ����� � �������� � ������� ����� ���� ������.
    /// ���� �������� ������ �� �������� �� ������ ��������, ������������ null.
    /// ���� ������ ����� � �������� ���, �� ������������ �������� ������.
    /// ������������ ����������������� ������ RemoveEmptuyColumns(RemoveEmptyRows())
    /// </summary>
    /// <param name="a">�������� ��������� ������ �����</param>
    /// <returns>��������������� ������</returns>
    public static string[,] RemoveEmptyRowsAndColumns(string[,] a)
    {
      if (a == null)
        return null;

      int nRows = a.GetLength(0);
      int nCols = a.GetLength(1);
      bool[] RowFlags = new bool[nRows];
      bool[] ColFlags = new bool[nCols];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (!String.IsNullOrEmpty(a[i, j]))
          {
            RowFlags[i] = true;
            ColFlags[j] = true;
          }
        }
      }

      int nRows2 = 0;
      int[] RowRefs = new int[nRows];
      for (int i = 0; i < nRows; i++)
      {
        if (RowFlags[i])
        {
          RowRefs[i] = nRows2;
          nRows2++;
        }
        else
          RowRefs[i] = -1;
      }

      int nCols2 = 0;
      int[] ColRefs = new int[nCols];
      for (int j = 0; j < nCols; j++)
      {
        if (ColFlags[j])
        {
          ColRefs[j] = nCols2;
          nCols2++;
        }
        else
          ColRefs[j] = -1;
      }

#if DEBUG
      if ((nRows2 == 0) != (nCols2 == 0))
        throw new BugException("������������ ����������� ������ ����� � ��������. nRows2=" + nRows2.ToString() + ", nCols2=" + nCols2);
#endif

      if (nRows2 == 0 || nCols2 == 0) // ������-�� ������ ���� ��� ������
        return null;
      if (nRows2 == nRows && nCols2 == nCols) // ��� �� ������ �����, �� ������ ��������
        return a;

      string[,] a2 = new string[nRows2, nCols2];
      for (int i = 0; i < nRows; i++)
      {
        for (int j = 0; j < nCols; j++)
        {
          if (RowRefs[i] >= 0 && ColRefs[j] >= 0)
            a2[RowRefs[i], ColRefs[j]] = a[i, j];
        }
      }

      return a2;
    }

    #endregion

    #region ����������� ������� � ������

    /// <summary>
    /// ������ "�������" ��������
    /// </summary>
    public const char SoftHyphenChar = '\u00AD';

    /// <summary>
    /// ������, ���������� ������������ ������ "�������" ��������
    /// </summary>
    public const string SoftHyphenStr = "\u00AD";

    /// <summary>
    /// ������ ������������ �������
    /// </summary>
    public const char NonBreakSpaceChar = '\u00A0';

    /// <summary>
    /// ������, ���������� ������������ ������ ������������ �������
    /// </summary>
    public const string NonBreakSpaceStr = "\u00A0";

    /// <summary>
    /// �������� �������� "�������" �������� �� ������
    /// </summary>
    /// <param name="s">�������� ������</param>
    /// <returns>������ ��� ��������</returns>
    public static string RemoveSoftHyphens(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.IndexOf(SoftHyphenChar) >= 0)
        return s.Replace(SoftHyphenStr, String.Empty);
      else
        return s;
    }

    /// <summary>
    /// ������ ����������� ������� �������
    /// </summary>
    public const char LeftDoubleAngleQuotationChar = '\u00AB';

    /// <summary>
    /// ������ ����������� ������� �������
    /// </summary>
    public const string LeftDoubleAngleQuotationStr = "\u00AB";

    /// <summary>
    /// ������ ����������� ������� �������
    /// </summary>
    public const char RightDoubleAngleQuotationChar = '\u00BB';

    /// <summary>
    /// ������ ����������� ������� �������
    /// </summary>
    public const string RightDoubleAngleQuotationStr = "\u00BB";

    /// <summary>
    /// �������������� ������� �����, ���������� ����������� �������, � ���� ������
    /// � ������� ��������:
    /// - ������ ������� ���������� �� "^"
    /// - ����������� ������ ���������� �� "_"
    /// ������, ���������� ������, ����������� �������� "|"
    /// </summary>
    /// <param name="a">������ �����, ���������� �����������</param>
    /// <returns>������ � ����������� ���������</returns>
    public static string StrFromSpecCharsArray(string[] a)
    {
      if (a == null)
        return String.Empty;
      string s = String.Join("|", a);
      return StrFromSpecCharsStr(s);
    }

    /// <summary>
    /// �������������� ������, ���������� �����������.
    /// ����������� ������ ��������
    /// - ������ ������� ���������� �� "^"
    /// - ����������� ������ ���������� �� "_"
    /// </summary>
    /// <param name="s">������, ���������� �����������</param>
    /// <returns>������ � ����������� ���������</returns>
    public static string StrFromSpecCharsStr(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      s = s.Replace(NonBreakSpaceChar, '_');
      s = s.Replace(SoftHyphenChar, '^');
      return s;
    }

    /// <summary>
    /// ������������� ������, ���������� ������� "|", "^" � "_" � ������ �����.
    /// - ������ ����������� �� �������� ������� �� ������� "|";
    /// - ������ "_" ���������� �� ����������� ������
    /// - ������ "^" ���������� �� ������ �������
    /// ��� ������ ������ ������������ ������ ������ �����
    /// </summary>
    /// <param name="s">������, ���������� �������, ������� ��������� ��������</param>
    /// <returns>������ ����� �� �������������</returns>
    public static string[] StrToSpecCharsArray(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DataTools.EmptyStrings;
      string[] a = s.Split('|');
      for (int i = 0; i < a.Length; i++)
        a[i] = StrToSpecCharsStr(a[i]);
      return a;
    }

    /// <summary>
    /// ������������� ������, ���������� ������� "^" � "_" � ������ �� �������������.
    /// - ������ "_" ���������� �� ����������� ������
    /// - ������ "^" ���������� �� ������ �������
    /// </summary>
    /// <param name="s">������, ���������� �������, ������� ��������� ��������</param>
    /// <returns>������ �� �������������</returns>
    public static string StrToSpecCharsStr(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      s = s.Replace('_', NonBreakSpaceChar);
      s = s.Replace('^', SoftHyphenChar);
      return s;
    }

    #endregion
  }
}