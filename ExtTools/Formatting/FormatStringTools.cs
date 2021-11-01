using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.Formatting
{
  #region ������������ DateFormatYMDOrder

  /// <summary>
  /// ��������� ������������� ������� ������������ ����������� ���, ������ � ���� � ������� ����
  /// </summary>
  public enum DateFormatYMDOrder
  {
    // ������� ��������� �����!

    /// <summary>
    /// ���, ����� ���� ("��������")
    /// </summary>
    YMD = 0,

    /// <summary>
    /// ���, ����, ����� (�� ����, ��� ����� ����)
    /// </summary>
    YDM = 1,

    /// <summary>
    /// �����, ����, ��� (���)
    /// </summary>
    MDY = 2,

    /// <summary>
    /// ����, �����, ��� (������)
    /// </summary>
    DMY = 3,
  }

  #endregion

  /// <summary>
  /// ����������� ������� ��� ������ �� �������� ��������������
  /// </summary>
  public sealed class FormatStringTools
  {
    #region �������� �������

    /// <summary>
    /// �������� �������� ������ ��� ��������� ����� ���������� ��������.
    /// ��� ������������� �������� <paramref name="decimalPlaces"/> ������������ ������ ������.
    /// ��� ������� - ������������� ������ "0", ��� ������������� - ������ "0.0000" � ��������������� ������ �����
    /// </summary>
    /// <param name="decimalPlaces">���������� �������� ����� ���������� �����</param>
    /// <returns>�������� ������</returns>
    public static string DecimalPlacesToNumberFormat(int decimalPlaces)
    {
      if (decimalPlaces < 0)
        return String.Empty;
      else if (decimalPlaces == 0)
        return "0";
      else
        return "0." + new string('0', decimalPlaces);
    }

    /// <summary>
    /// �������� ���������� ���������� �������� �� ��������� �������.
    /// ���� �������� <paramref name="format"/> �� ������ ��� ������ �� ������� ���������� ��� ��������, ������������ (-1).
    /// ������� ���������� �������� ����� ������� � ���������� ������ ������� � ������ � "#": "0", "0.0", "0.0#", � �.�.
    /// </summary>
    /// <param name="format">�������� ������</param>
    /// <returns>���������� ���������� ��������</returns>
    public static int DecimalPlacesFromNumberFormat(string format)
    {
      if (String.IsNullOrEmpty(format))
        return -1;
      if (format.Length == 1) // ���� �� ����������� ��������, ��������, "G"
        return -1;

      int p = format.IndexOf('.');
      if (p < 0)
        return 0;

      int cnt = 0;
      for (int i = p + 1; i < format.Length; i++)
      {
        if (format[i] == '0' || format[i] == '#')
          cnt++;
        else
          break;
      }
      return cnt;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// ����������, �������� �� ������ �������������� ������� ��� ���� �/��� �������.
    /// ������������ �������������� � ��������������� ������ ��������������, ������� ����� ���������� ������ DateTime.ToString().
    /// ���� <paramref name="formatString"/>-������ ������, �� <paramref name="containsDate"/>=true � <paramref name="containsTime"/>=true.
    /// </summary>
    /// <param name="formatString">������ �������������� ��� DateTime</param>
    /// <param name="containsDate">���� ������������ true, ���� ������ �������������� �������� ����</param>
    /// <param name="containsTime">���� ������������ true, ���� ������ �������������� �������� �����</param>
    public static void ContainsDateTime(string formatString, out bool containsDate, out bool containsTime)
    {
      if (String.IsNullOrEmpty(formatString))
      {
        containsDate = true;
        containsTime = true;
        return;
      }

      #region ����������� ������ ��������������

      if (formatString.Length == 1)
      {
        switch (formatString[0])
        {
          case 'd':
          case 'D':
          case 'm':
          case 'M':
          case 'y':
          case 'Y':
            containsDate = true;
            containsTime = false;
            break;
          case 't':
          case 'T':
            containsDate = false;
            containsTime = true;
            break;
          case 'f':
          case 'F':
          case 'g':
          case 'G':
          case 'r':
          case 'R':
          case 's':
          case 'u':
          case 'U':
            containsDate = true;
            containsTime = true;
            break;
          default:
            containsDate = false;
            containsTime = false;
            break;
        }
        return;
      }

      #endregion

      #region ����������� ������ ��������������

      containsDate = false;
      containsTime = false;

      int p = 0;
      while (p < formatString.Length)
      {
        switch (formatString[p])
        {
          case 'd':
          case 'g':
          case 'M':
          case 'y':
            //case '/':
            containsDate = true;
            break;

          case 'h':
          case 'H':
          case 'm':
          case 's':
          case 'f':
          case 'F':
          case 't':
          case 'z':
            //case ':':
            containsTime = true;
            break;

          case '%': // ��������� �������
          case '\\': // escape-������
            p++;
            break;

          case '\"':
          case '\'':
            // ������ � ��������
            char ch = formatString[p];
            p++;
            while (p < formatString.Length)
            {
              if (formatString[p] == ch)
                break;
              else
                p++;
            }
            break;
        }
        p++;
      }

      #endregion
    }

    /// <summary>
    /// ����������, �������� �� ������ �������������� ������� ��� ����.
    /// ������������ �������������� � ��������������� ������ ��������������, ������� ����� ���������� ������ DateTime.ToString().
    /// ���� <paramref name="formatString"/>-������ ������, �� ������������ true.
    /// </summary>
    /// <param name="formatString">������ �������������� ��� DateTime</param>
    /// <returns>true, ���� ������ �������������� �������� ����</returns>
    public static bool ContainsDate(string formatString)
    {
      bool containsDate;
      bool containsTime;
      ContainsDateTime(formatString, out containsDate, out containsTime);
      return containsDate;
    }

    /// <summary>
    /// ����������, �������� �� ������ �������������� ������� ��� �������.
    /// ������������ �������������� � ��������������� ������ ��������������, ������� ����� ���������� ������ DateTime.ToString().
    /// ���� <paramref name="formatString"/>-������ ������, �� ������������ true.
    /// </summary>
    /// <param name="formatString">������ �������������� ��� DateTime</param>
    /// <returns>true, ���� ������ �������������� �������� �������</returns>
    public static bool ContainsTime(string formatString)
    {
      bool containsDate;
      bool containsTime;
      ContainsDateTime(formatString, out containsDate, out containsTime);
      return containsTime;
    }

    /// <summary>
    /// ���������� ������� ���������� ���, ������ � ���� � ������� ����.
    /// ���� ������ �� ������ ��� ����� ����� � ���� ������ (��������, "d"), ������������
    /// ShortDatePattern �� ������� ��������.
    /// ������������� ������� �������� "d", "M" � "y" ��� ����������� �������.
    /// ���� �����-�� ��������� ���, ����������� �������� ���������� �������.
    /// ���� ������ �� �������� �� ������ �� ��������, ������������ DateFormatYMDOrder.YMD
    /// </summary>
    /// <param name="formatString">�������� ������ ����</param>
    /// <returns>������� ���������� �����������</returns>
    public static DateFormatYMDOrder GetDateFormatOrder(string formatString)
    {
      if (String.IsNullOrEmpty(formatString))
        formatString = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
      else if (formatString.Length == 1) // ����������� ������, ��������, "d"
        formatString = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;

      int pD = formatString.IndexOf("d");
      int pM = formatString.IndexOf("M");
      int pY = formatString.IndexOf("y");

      if (pY < 0)
        pY = -3;
      if (pM < 0)
        pM = -2;
      if (pD < 0)
        pD = -1;

      int index1 = pY < pM ? 0 : 2;
      int index2 = pM < pD ? 0 : 1;
      return (DateFormatYMDOrder)(index1 + index2);
    }

    /// <summary>
    /// ���������� ������� ���������� ���, ������ � ����, ������������ ������� ���������
    /// </summary>
    public static DateFormatYMDOrder DateFormatOrder
    {
      get { return GetDateFormatOrder(null); }
    }

#if XXX
    private static readonly string[] _Date10Formats = new string[] { "yyyy/MM/dd", "yyyy/dd/MM", "MM/dd/yyyy", "dd/MM/yyyy" };

    /// <summary>
    /// ���������� "�������������������" ������ ���� �� 10 ��������, ����� ��� ������������ 4 �������,
    /// ����� � ���� - �����. ����� ������� �������� ������-�����������.
    /// </summary>
    /// <param name="formatString">�������� ������ ����</param>
    /// <returns>"�������������������" 10-���������� ������</returns>
    public static string GetDate10Format(string formatString)
    {
      return _Date10Formats[(int)GetDateFormatOrder(formatString)];
    }

    /// <summary>
    /// ���������� "�������������������" ������ ���� �� 10 ��������, ������������ ������� ���������.
    /// ��� ������������ 4 �������, ����� � ���� - �����. ����� ������� �������� ������-�����������.
    /// </summary>
    public static string Date10Format
    {
      get { return GetDate10Format(null); }
    }

    /// <summary>
    /// ����� ��� �������������� ����: "00/00/0000" ��� "0000/00/00"
    /// </summary>
    public static string Date10EditMask
    {
      get
      {
        switch (DateFormatOrder)
        {
          case DateFormatYMDOrder.YDM:
          case DateFormatYMDOrder.YMD:
            return "0000/00/00";
          default:
            return "00/00/0000";
        }
      }
    }
#endif
    #endregion
  }

  #region ������������ EditableDateTimeFormatterKind

  /// <summary>
  /// ��� ������������� ���� � �������
  /// </summary>
  public enum EditableDateTimeFormatterKind
  {
    /// <summary>
    /// �������� ������ ���� ("��.��.����")
    /// ��� ������ ������� Parse()/TryParse() ��������� ������� ����� �������� 00:00:00.
    /// </summary>
    Date,

    /// <summary>
    /// �������� ������ ������� ��� ������ ("��:��").
    /// ��� ������ ������� Parse()/TryParse() ��������� ���� ����� �������� ������� ���� DateTime.Today.
    /// </summary>
    ShortTime,

    /// <summary>
    /// ������ ������� � ��������� ("��:��:��")
    /// ��� ������ ������� Parse()/TryParse() ��������� ���� ����� �������� ������� ���� DateTime.Today.
    /// </summary>
    Time,

    /// <summary>
    /// ������ ���� � ������� ��� ������ "��.��.���� ��:��"
    /// </summary>
    ShortDateTime,

    /// <summary>
    /// ������ ���� � ������� � ��������� "��.��.���� ��:��:��"
    /// </summary>
    DateTime
  }

  #endregion

  /// <summary>
  /// ������������ ����/�������, ������� ������ ������������ � ����� �����, �.�.
  /// �������������� ���������� ���������� �������� ��� ���� ��������
  /// </summary>
  public sealed class EditableDateTimeFormatter
  {
    #region �����������

    private static readonly string[] _Date10Formats = new string[] { "yyyy/MM/dd", "yyyy/dd/MM", "MM/dd/yyyy", "dd/MM/yyyy" };

    /// <summary>
    /// ����������� ����������� �������� ������ EditableDateTimeFormatters ��� ������� � ��������������
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <param name="kind"></param>
    public EditableDateTimeFormatter(CultureInfo cultureInfo, EditableDateTimeFormatterKind kind)
    {
      _FormatInfo = cultureInfo.DateTimeFormat;
      _kind = kind;

      #region ����

      string dateFormat;
      string dateMask;
      switch (kind)
      {
        case EditableDateTimeFormatterKind.Date:
        case EditableDateTimeFormatterKind.ShortDateTime:
        case EditableDateTimeFormatterKind.DateTime:
          DateFormatYMDOrder dateOrder = FormatStringTools.GetDateFormatOrder(_FormatInfo.ShortDatePattern);
          dateFormat = _Date10Formats[(int)dateOrder];
          if (dateOrder == DateFormatYMDOrder.YDM || dateOrder == DateFormatYMDOrder.YMD)
            dateMask = "0000/00/00";
          else
            dateMask = "00/00/0000";
          break;
        default:
          dateFormat = String.Empty;
          dateMask = String.Empty;
          break;
      }

      #endregion

      #region �����

      string timeFormat;
      string timeMask;
      int timeWidth;
      int ampmTextWidth = 0;
      bool useAMPM;
      switch (kind)
      {
        case EditableDateTimeFormatterKind.ShortTime:
        case EditableDateTimeFormatterKind.ShortDateTime:
          useAMPM = _FormatInfo.ShortTimePattern.IndexOf("tt") >= 0;
          timeFormat = useAMPM ? "hh:mm tt" : "HH:mm";
          timeMask = useAMPM ? ("00:00 " + GetAMPMMask(_FormatInfo, out ampmTextWidth)) : "00:00";
          timeWidth = 6;
          break;
        case EditableDateTimeFormatterKind.Time:
        case EditableDateTimeFormatterKind.DateTime:
          useAMPM = _FormatInfo.LongTimePattern.IndexOf("tt") >= 0;
          timeFormat = useAMPM ? "hh:mm:ss tt" : "HH:mm:ss";
          timeMask = useAMPM ? ("00:00:00 " + GetAMPMMask(_FormatInfo, out ampmTextWidth)) : "00:00:00";
          timeWidth = 8;
          break;
        default:
          useAMPM = false;
          timeFormat = String.Empty;
          timeMask = String.Empty;
          timeWidth = 0;
          break;
      }

      #endregion

      #region ������������� �������

      _Format = dateFormat;
      _EditMask = dateMask;
      _TextWidth = dateFormat.Length;

      switch (kind)
      { 
        case EditableDateTimeFormatterKind.ShortDateTime:
        case EditableDateTimeFormatterKind.DateTime:
          // ��������� ����������� ����� ����� � ��������
          _Format += " ";
          _EditMask += " ";
          _TextWidth += 1;
          break;
      }

      if (timeFormat.Length > 0)
      {
        _Format += timeFormat;
        _EditMask += timeMask;
        _TextWidth += timeWidth+ (useAMPM ? (1 + ampmTextWidth) : 0);
      }

      _MaskProvider = new StdMaskProvider(_EditMask, cultureInfo);

      #endregion
    }

    /// <summary>
    /// ���������� ����� ����� ��� �������������� AM/PM
    /// </summary>
    private string GetAMPMMask(DateTimeFormatInfo formatInfo, out int ampmTextWidth)
    {
      string s1 = formatInfo.AMDesignator;
      string s2 = formatInfo.PMDesignator;
      int nChars = Math.Max(s1.Length, s2.Length);
      // ���� �� ����������� ����� ���� ������ �����, �� �� ������ ������ ��������
      s1 = s1.PadRight(nChars);
      s2 = s2.PadRight(nChars);

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < nChars; i++)
      {
        if (s1[i] == s2[i])
        {
          // ��� ���������� �������
          sb.Append('\\');
          sb.Append(s1[i]);
        }
        else
        {
          // TODO: �������� �������
          sb.Append("C"); // ����� ������
        }
      }

      ampmTextWidth = nChars;
      return sb.ToString();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� �������������.
    /// </summary>
    public EditableDateTimeFormatterKind Kind { get { return _kind; } }
    private EditableDateTimeFormatterKind _kind;

    /// <summary>
    /// ������
    /// </summary>
    public string Format { get { return _Format; } }
    private string _Format;

    /// <summary>
    /// ��������� ��� ��������������
    /// </summary>
    public IFormatProvider FormatProvider { get { return _FormatInfo; } }
    private DateTimeFormatInfo _FormatInfo;

    /// <summary>
    /// �����, ������� ����� ������������ � ���� �����, ��������, MaskedTextBox
    /// </summary>
    public string EditMask { get { return _EditMask; } }
    private string _EditMask;

    /// <summary>
    /// ������ ���������� ���� (���������� ������ � ���� �����)
    /// </summary>
    public int TextWidth { get { return _TextWidth; } }
    private int _TextWidth;

    /// <summary>
    /// ��������� ��� ������������ ����� �����
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } }
    private StdMaskProvider _MaskProvider;

    /// <summary>
    /// ���������� true, ���� ������ Parse() � TryParse() ����� ������������ �������� defaultYear, �����
    /// ��� �� ����� � ������������� ������. ���� false, �� ���� �������� ������������
    /// </summary>
    public bool DefaultYearSupported
    {
      get
      {
        return Kind == EditableDateTimeFormatterKind.Date &&
          Format[Format.Length - 1] == 'y'; // ������������� �� ���
      }
    }

    /// <summary>
    /// ���������� true, ���� � ������� ������������ ��������� ����.
    /// ��� �������, ���� �� ������� ContainsDate � ContainsTime ������ ���������� true.
    /// </summary>
    public bool ContainsDate 
    {
      get
      {
        switch (Kind)
        { 
          case EditableDateTimeFormatterKind.Date:
          case EditableDateTimeFormatterKind.DateTime:
          case EditableDateTimeFormatterKind.ShortDateTime:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// ���������� true, ���� � ������� ������������ ��������� �������.
    /// ��� �������, ���� �� ������� ContainsDate � ContainsTime ������ ���������� true.
    /// </summary>
    public bool ContainsTime
    {
      get
      {
        switch (Kind)
        {
          case EditableDateTimeFormatterKind.Time:
          case EditableDateTimeFormatterKind.ShortTime:
          case EditableDateTimeFormatterKind.DateTime:
          case EditableDateTimeFormatterKind.ShortDateTime:
            return true;
          default:
            return false;
        }
      }
    }

    #endregion

    #region ������

    /// <summary>
    /// ���������� ����/�����, ����������������� � ������������ � ����� Format
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public string ToString(DateTime value)
    {
      return value.ToString(Format, FormatProvider);
    }

    /// <summary>
    /// ��������� ������� �������������� ������ � �������� ����/�������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="value">���� ������������ ��������</param>
    /// <param name="defaultYear">���� �������� ��������� ��������, �������� DefaultYearSupported=true,
    /// � � ������ ��� ����, �� ������������ ���� ���</param>
    /// <returns>true, ���� �������������� ������� ���������</returns>
    public bool TryParse(string s, out DateTime value, int defaultYear)
    {
      if (defaultYear != 0 && DefaultYearSupported)
      {
        if (s.Length == 6)
          s += defaultYear.ToString("0000");
      }

      //return DateTime.TryParseExact(s, Format, FormatProvider, DateTimeStyles.AllowWhiteSpaces, out value);
      return DateTime.TryParse(s, FormatProvider, DateTimeStyles.AllowWhiteSpaces, out value);
    }

    /// <summary>
    /// ��������� ������� �������������� ������ � �������� ����/�������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="value">���� ������������ ��������</param>
    /// <returns>true, ���� �������������� ������� ���������</returns>
    public bool TryParse(string s, out DateTime value)
    {
      return TryParse(s, out value, 0);
    }

    /// <summary>
    /// ��������� �������������� ������ � �������� ����/�������.
    /// � ������ ������ ������������ FormatException
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="defaultYear">���� �������� ��������� ��������, �������� DefaultYearSupported=true,
    /// � � ������ ��� ����, �� ������������ ���� ���</param>
    /// <returns>��������������� ��������</returns>
    public DateTime Parse(string s, int defaultYear)
    {
      DateTime value;
      if (TryParse(s, out value, defaultYear))
        return value;
      else
        throw new FormatException();
    }

    /// <summary>
    /// ��������� �������������� ������ � �������� ����/�������.
    /// � ������ ������ ������������ FormatException
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>��������������� ��������</returns>
    public DateTime Parse(string s)
    {
      return Parse(s, 0);
    }

    /// <summary>
    /// �������������� ������ � Nullable-��������.
    /// � ������ ������ ������������ null, ��� ��� ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="defaultYear">���� �������� ��������� ��������, �������� DefaultYearSupported=true,
    /// � � ������ ��� ����, �� ������������ ���� ���</param>
    /// <returns>��������������� �������� ��� null</returns>
    public DateTime? ToNValue(string s, int defaultYear)
    {
      DateTime value;
      if (TryParse(s, out value, defaultYear))
        return value;
      else
        return null;
    }
    /// <summary>
    /// �������������� ������ � Nullable-��������.
    /// � ������ ������ ������������ null, ��� ��� ������ ������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>��������������� �������� ��� null</returns>
    public DateTime? ToNValue(string s)
    {
      return ToNValue(s, 0);
    }

    #endregion
  }

  /// <summary>
  /// ��������� �������������� ����/�������, ����������� � �������� ������
  /// </summary>
  public static class EditableDateTimeFormatters
  {
    #region ������������ ��������� � ������

    [ThreadStatic]
    private static DateTimeFormatInfo _CurrentInfo;

    private static void CheckCurrentInfo()
    {
      if (!Object.ReferenceEquals(_CurrentInfo, Thread.CurrentThread.CurrentCulture.DateTimeFormat))
      {
        _Items = null;
        _MonthNames12 = null;
        _MonthGenitiveNames12 = null;
        _CurrentInfo = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
      }

    }

    #endregion

    #region ������� EditableDateTimeFormatter

    /// <summary>
    /// ������������ ��� ���� 
    /// </summary>
    public static EditableDateTimeFormatter Date { get { return Get(EditableDateTimeFormatterKind.Date); } }

    /// <summary>
    /// ������������ ��� ������� ��� ������
    /// </summary>
    public static EditableDateTimeFormatter ShortTime { get { return Get(EditableDateTimeFormatterKind.ShortTime); } }

    /// <summary>
    /// ������������ ��� ������� � ���������
    /// </summary>
    public static EditableDateTimeFormatter Time { get { return Get(EditableDateTimeFormatterKind.Time); } }

    /// <summary>
    /// ������������ ��� ���� � ������� ��� ������
    /// </summary>
    public static EditableDateTimeFormatter ShortDateTime { get { return Get(EditableDateTimeFormatterKind.ShortDateTime); } }

    /// <summary>
    /// ������������ ��� ���� � ������� � ���������
    /// </summary>
    public static EditableDateTimeFormatter DateTime { get { return Get(EditableDateTimeFormatterKind.DateTime); } }

    [ThreadStatic]
    private static EditableDateTimeFormatter[] _Items;

    /// <summary>
    /// ������ � ������������� �� ��� ����
    /// </summary>
    /// <param name="kind">��� �������������</param>
    /// <returns>������������</returns>
    public static EditableDateTimeFormatter Get(EditableDateTimeFormatterKind kind)
    {
      CheckCurrentInfo();

      if (_Items == null)
        _Items = new EditableDateTimeFormatter[5];
      if (_Items[(int)kind] == null)
        _Items[(int)kind] = new EditableDateTimeFormatter(Thread.CurrentThread.CurrentCulture, kind);
      return _Items[(int)kind];
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ���������� DateTimeFormatInfo.MonthNames ��� ������� 13-�� ������ ("������", "�������", ..., "�������")
    /// </summary>
    public static string[] MonthNames12
    {
      get
      {
        CheckCurrentInfo();

        if (_MonthNames12 == null)
        {
          string[] a = new string[12];
          for (int i = 0; i < 12; i++)
            a[i] = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i];
          _MonthNames12 = a;
        }
        return _MonthNames12;
      }
    }
    [ThreadStatic]
    private static string[] _MonthNames12;


    /// <summary>
    /// ���������� DateTimeFormatInfo.MonthGenitiveNames ��� ������� 13-�� ������ ("������", "�������", ..., "�������")
    /// </summary>
    public static string[] MonthGenitiveNames12
    {
      get
      {
        CheckCurrentInfo();

        if (_MonthGenitiveNames12 == null)
        {
          string[] a = new string[12];
          for (int i = 0; i < 12; i++)
            a[i] = CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames[i];
          _MonthGenitiveNames12 = a;
        }
        return _MonthGenitiveNames12;
      }
    }
    [ThreadStatic]
    private static string[] _MonthGenitiveNames12;

    #endregion
  }
}