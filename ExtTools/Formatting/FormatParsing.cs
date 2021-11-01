using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Parsing;

/*
 * The BSD License
 * 
 * Copyright (c) 2016, Ageyev A.V.
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

/*
 * ������� �������� ��������, ������������ Net Framework, Excel � Calc
 * �� �������. ������� ������ �������
 */

#if XXX
namespace FreeLibSet.FormatParsing
{
  #region ������������ FormatKind

  [Serializable]
  public enum FormatKind
  {
    Unknown,
    NetFramework,
    Excel,
    Calc
  }

  #endregion

  /// <summary>
  /// �������� �������
  /// </summary>
  [Serializable]
  public sealed class Format
  {
    #region �����������

    public Format(FormatSection[] Sections, int LCID)
    {
      FSections = Sections;
      FLCId = LCID;
    }

    #endregion

    #region ��������

    public FormatSection this[int Index] { get { return FSections[Index]; } }
    private FormatSection[] FSections;

    private int Count { get { return FSections.Length; } }

    /// <summary>
    /// ��� ������.
    /// ������ - �� ����� (0)
    /// </summary>
    public int LCID { get { return FLCId; } }
    private int FLCId;

    #endregion

    #region ��������� ������������� �������

    public override string ToString()
    {
      return ToString(FormatKind.NetFramework);
    }

    public string ToString(FormatKind Kind)
    {
      if (Count == 0 && LCID == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      ToString(sb, Kind);
      return sb.ToString();
    }

    private void ToString(StringBuilder sb, FormatKind Kind)
    {
      switch (Kind)
      {
        case FormatKind.Excel:
        case FormatKind.Calc:
          if (LCID != 0)
          {
            sb.Append("[$-");
            sb.Append(LCID.ToString("X"));
            sb.Append(']');
          }
          break;
      }
      for (int i = 0; i < Count; i++)
      {
        if (i > 0)
          sb.Append(';');
        FSections[i].ToString(sb, Kind);
      }
    }

    #endregion

    #region �������

    public static Format Parse(string s, FormatKind Kind)
    {
      Format Res;
      String ErrorText;

      if (TryParse(s, Kind, out Res, out ErrorText))
        return Res;
      else
        throw new FormatException("������ �������������� ������ � �������� ������. " + ErrorText +
          ". ������ �������: \"" + s + "\"");
    }

    public static bool TryParse(string s, FormatKind Kind, out Format Res)
    {
      String ErrorText;
      return TryParse(s, Kind, out Res, out ErrorText);
    }

    public static bool TryParse(string s, FormatKind Kind, out Format Res, out String ErrorText)
    {
      int ErrorStart;
      int ErrorLength;
      if (TryParse(s, Kind, out Res, out ErrorText, out ErrorStart, out ErrorLength))
        return true;
      if (ErrorLength > 0)
        ErrorText += ". ��������� ��������: \"" + s.Substring(ErrorStart, ErrorLength) + "\" (������� " + (ErrorStart + 1).ToString() + ")";
      return false;
    }

    public static bool TryParse(string s, FormatKind Kind, out Format Res, out String ErrorText, out int ErrorStart, out int ErrorLength)
    {
      Res = null;
      ErrorText = null;
      ErrorStart = -1;
      ErrorLength = 0;

      if (String.IsNullOrEmpty(s))
      {
        Res = Format.Default;
        return true;
      }

      #region ��������� �� �������

      ParsingData pd = new ParsingData(s);
      ParserList Parser;
      switch (Kind)
      {
        case FormatKind.NetFramework:
          Parser = FormatParser.NetParser;
          break;
        case FormatKind.Excel:
        case FormatKind.Calc:
          Parser = FormatParser.ExcelParser;
          break;
        default:
          ErrorText = "�� ����� ��� �������";
          return false;
      }

      Parser.Parse(pd);

      if (pd.FirstErrorToken != null)
      {
        ErrorText = pd.FirstErrorToken.ErrorMessage.Value.Text;
        ErrorStart = pd.FirstErrorToken.Start;
        ErrorLength = pd.FirstErrorToken.Length;
        return false;
      }

      #endregion

      #region ��������� LCID

      int LCID = 0;
      if (pd.Tokens.Count > 0)
      {
        Token tk = pd.Tokens[0];
        if (tk.TokenType == "[]" && ((string)(tk.AuxData)).StartsWith("$-"))
        {
          string s2 = ((string)(tk.AuxData)).Substring(2);
          if (!int.TryParse(s2, System.Globalization.NumberStyles.AllowHexSpecifier, null, out LCID))
          {
            ErrorText = "������������ ��������� ������. ������ ������������� \"" + s2 + "\"";
            ErrorStart = tk.Start;
            ErrorLength = tk.Length;
            return false;
          }

          pd = pd.CreateSubTokens(1);
        }
      }

      #endregion

      #region ��������� �� ������

      ParsingData[] pds = pd.SplitTokens(";");
      FormatSection[] Sections = new FormatSection[pds.Length];
      for (int i = 0; i < pds.Length; i++)
      {
        FormatSection Sect;
        if (!TryParseSect(pds[i], Kind, out Sect, out ErrorText, out ErrorStart, out ErrorLength))
          return false;
        Sections[i] = Sect;
      }

      #endregion

      Res = new Format(Sections, LCID);
      return true;
    }

    private static bool TryParseSect(ParsingData pd, FormatKind Kind, out FormatSection Sect, out string ErrorText, out int ErrorStart, out int ErrorLength)
    {
      ErrorText = null;
      ErrorStart = -1;
      ErrorLength = 0;

      if (pd.Tokens.Count == 0)
      {
        // ������ ������
        Sect = FormatSection.Empty;
        return true;
      }

      FormatColor Color = FormatColor.Auto;
      List<FormatItem> Items = new List<FormatItem>();
      for (int i = 0; i < pd.Tokens.Count; i++)
      {
        Token tk = pd.Tokens[i];
        switch (tk.TokenType)
        {
          case "[]":
            switch (Kind)
            {
              case FormatKind.Excel:
              case FormatKind.Calc:
                string ColorName = DataTools.GetString(tk.AuxData);
                int pColor = Array.IndexOf<string>(FormatColors.ColorNames, ColorName);
                if (pColor < 0)
                {
                  ErrorText = "������� " + tk.Text + " ������ ������������� � ����";
                  ErrorStart = tk.Start;
                  ErrorLength = tk.Length;
                  return false;
                }
                if (!Color.IsEmpty)
                {
                  ErrorText = "��������� ���������� ����� ��� ������";
                  ErrorStart = tk.Start;
                  ErrorLength = tk.Length;
                  return false;
                }
                Color = FormatColors.Colors[pColor];
                break;
              default:
                ErrorText = "������� ����������� ����� \"" + tk.Text + "\" ����������� ��� ����� �������";
                ErrorStart = tk.Start;
                ErrorLength = tk.Length;
                return false;
            }
            break;
          case "Literal":
            LiteralFormatItem LitItem = new LiteralFormatItem(DataTools.GetString(tk.AuxData));
            Items.Add(LitItem);
            break;
          case "Char":
            string Chars = DataTools.GetString(tk.AuxData);
            char ch = Chars.ToUpperInvariant()[0];
            if (ch >= 'A' && ch <= 'Z')
            {
              FormatItem CharItem;
              if (Kind == FormatKind.NetFramework)
                CharItem = AddCharItemNet(Chars);
              else
                CharItem = AddCharItemExcelCalc(Chars);
              if (CharItem != null)
              {
                Items.Add(CharItem);
                continue;
              }

              // ��������� ��� ������� (???)
              Items.Add(new LiteralFormatItem(Chars));
            }
            else
            {
              ErrorText = "�������������� �������: \"" + tk.Text + "\"";
              ErrorStart = tk.Start;
              ErrorLength = tk.Length;
              return false;
            }
            break;
        }
      }
    }

    private static FormatItem GetCharItemNet(string Chars, ref string ErrorText, ref int ErrorStart, ref int ErrorLength)
    {
      switch (Chars)
      {
        case "d": return new DateTimeFormatItem(DateTimePart.Day, 1);
        case "dd": return new DateTimeFormatItem(DateTimePart.Day, 2);
        case "ddd": return new DateTimeFormatItem(DateTimePart.DayOfWeekAbbreviated);
        case "dddd": return new DateTimeFormatItem(DateTimePart.DayOfWeekName);

        case "H": return new DateTimeFormatItem(DateTimePart.Hour24, 1);
        case "HH": return new DateTimeFormatItem(DateTimePart.Hour24, 2);
        case "h": return new DateTimeFormatItem(DateTimePart.Hour12, 1);
        case "hh": return new DateTimeFormatItem(DateTimePart.Hour12, 2);

        case "M": return new DateTimeFormatItem(DateTimePart.Month, 1);
        case "MM": return new DateTimeFormatItem(DateTimePart.Month, 2);
        case "MMM": return new DateTimeFormatItem(DateTimePart.MonthAbbreviated);
        case "MMMM": return new DateTimeFormatItem(DateTimePart.MonthName);
        case "m": return new DateTimeFormatItem(DateTimePart.Minute, 1);
        case "mm": return new DateTimeFormatItem(DateTimePart.Minute, 2);

        case "s": return new DateTimeFormatItem(DateTimePart.Second, 2);
        case "ss": return new DateTimeFormatItem(DateTimePart.Second, 2);

        case "yy": return new DateTimeFormatItem(DateTimePart.Year, 2);
        case "yyyy": return new DateTimeFormatItem(DateTimePart.Year, 4);
      }
    }

    private static FormatItem AddCharItemExcelCalc(string Chars, FormatKind Kind, ref string ErrorText, ref int ErrorStart, ref int ErrorLength)
    {
      return null;
    }

    #endregion

    #region ����������� ��������

    /// <summary>
    /// ����� �������� ������
    /// </summary>
    public static readonly Format Default = new Format(new FormatSection[0], 0);

    #endregion
  }

  /// <summary>
  /// ���� ������ �������
  /// </summary>
  [Serializable]
  public sealed class FormatSection
  {
    #region �����������

    internal FormatSection(FormatItem[] Items, FormatColor Color)
    {
      FItems = Items;
      FColor = Color;
    }

    #endregion

    #region ��������

    public FormatItem this[int Index] { get { return FItems[Index]; } }

    public int Count { get { return FItems.Length; } }
    private FormatItem[] FItems;

    public FormatColor Color { get { return FColor; } }
    private FormatColor FColor;

    /// <summary>
    /// ������ ������ �������
    /// </summary>
    public static readonly FormatSection Empty = new FormatSection(FormatItem[0], FormatColor.Auto);

    #endregion

    #region �������������� ������

    internal void ToString(StringBuilder sb, FormatKind Kind)
    {
      switch (Kind)
      {
        case FormatKind.Excel:
        case FormatKind.Calc:
          if (!Color.IsEmpty)
          {
            int p = Array.IndexOf<FormatColor>(FormatColors.Colors, Color);
            if (p >= 0)
            {
              sb.Append('[');
              sb.Append(FormatColors.ColorNames[p]);
              sb.Append(']');
            }
          }
          break;
      }

      for (int i = 0; i < Count; i++)
        FItems[i].ToString(sb, Kind);
    }

    #endregion
  }

  /// <summary>
  /// ���� ������
  /// </summary>
  [Serializable]
  public struct FormatColor
  {
    #region �����������

    public FormatColor(int Red, int Green, int Blue)
    {
      FRed = (byte)Red;
      FGreen = (byte)Green;
      FBlue = (byte)Blue;
      Flag = 1;
    }

    #endregion

    #region ��������

    public int Red { get { return FRed; } }
    private byte FRed;

    public int Green { get { return FGreen; } }
    private byte FGreen;

    public int Blue { get { return FBlue; } }
    private byte FBlue;

    public bool IsEmpty { get { return Flag != 0; } }
    private byte Flag;

    #endregion

    #region ����������� ��������

    public static readonly FormatColor Auto = new FormatColor();

    #endregion

    #region ���������

    public override int GetHashCode()
    {
      if (IsEmpty)
        return -1;
      return (Red << 16) | (Green << 8) | Blue;
    }

    public static bool operator ==(FormatColor a, FormatColor b)
    {
      return a.GetHashCode() == b.GetHashCode();
    }

    public static bool operator !=(FormatColor a, FormatColor b)
    {
      return a.GetHashCode() != b.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is FormatColor)
        return this == (FormatColor)obj;
      else
        return false;
    }

    #endregion
  }

  public static class FormatColors
  {
    #region ������ ������

 public static readonly FormatColor Cyan = new FormatColor(0, 255, 255);
    public static readonly FormatColor Black = new FormatColor(0, 0, 0);
    public static readonly FormatColor Magenta = new FormatColor(255, 0, 255);
    public static readonly FormatColor White = new FormatColor(255, 255, 255);
    public static readonly FormatColor Green = new FormatColor(0, 255, 0);
    public static readonly FormatColor Blue = new FormatColor(0, 0, 255);
    public static readonly FormatColor Red = new FormatColor(255, 0, 0);
    public static readonly FormatColor Yellow = new FormatColor(255, 255, 0);

    public static readonly string [] ColorNames = new string [] { "CYAN" , "BLACK", "MAGENTA", "WHITE", "GREEN", "BLUE", "RED", "YELLOW" };
    public static readonly FormatColor [] Colors = new FormatColor [] { Cyan , Black, Magenta, White, Green, Blue, Red, Yellow };

    #endregion
  }

  /// <summary>
  /// ���� ����� ������ �������
  /// </summary>
  [Serializable]
  public abstract class FormatItem
  {
    #region ��������� �������������

    public abstract void ToString(StringBuilder sb, FormatKind Kind);

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, FormatKind.NetFramework);
      return sb.ToString();
    }

    #endregion
  }

  [Serializable]
  public class LiteralFormatItem : FormatItem
  {
    #region �����������

    internal LiteralFormatItem(string Text)
    {
      FText = Text;
    }

    #endregion

    #region ��������

    public string Text { get { return FText; } }
    private string FText;

    #endregion

    #region ��������� �������������

    public override void ToString(StringBuilder sb, FormatKind Kind)
    {
      if (String.IsNullOrEmpty(Text))
        return;

      if (Text.Length == 1)
        AddOneChar(sb, Kind, Text[0]);
      else
      {
        if (Text.IndexOf('\'') >= 0)
        {
          string[] a = Text.Split('\'');
          for (int i = 0; i < a.Length; i++)
          {
            if (i > 0)
              sb.Append("\\\'");
            AddGroup(sb, a[i]);
          }
        }
        else
          AddGroup(sb, Text);
      }
    }

    private static void AddGroup(StringBuilder sb, string Text)
    {
      if (String.IsNullOrEmpty(Text))
        return;
      sb.Append('\'');
      sb.Append(Text);
      sb.Append('\'');
    }

    /// <summary>
    /// ����� ������� ������������ ��� ��������������
    /// </summary>
    const string ImmCharNet = " ";
    const string ImmCharExcel = " $(){}:^<>=!&~";
    const string ImmCharCalc = ImmCharExcel;

    private static void AddOneChar(StringBuilder sb, FormatKind Kind, char ch)
    {
      string ImmChar;
      switch (Kind)
      {
        case FormatKind.NetFramework: ImmChar = ImmCharNet; break;
        case FormatKind.Excel: ImmChar = ImmCharExcel; break;
        case FormatKind.Calc: ImmChar = ImmCharCalc; break;
        default:
          throw new ArgumentException("����������� ����� " + Kind.ToString(), "Kind");
      }

      if (ImmChar.IndexOf(ch) >= 0)
        sb.Append(ch);
      else
      {
        sb.Append('\\');
        sb.Append(ch);
      }
    }

    #endregion
  }

  [Serializable]
  public enum FormatSignMode { Auto, Always, None };

  [Serializable]
  public class NumberFormatItem : FormatItem
  {
    #region �����������

    internal NumberFormatItem(FormatSignMode Sign, int LeftDigits, int RightDigits, int AuxRightDigits)
    {
      FSign = Sign;
      FLeftDigits = LeftDigits;
      FRightDigits = RightDigits;
      FAuxRightDigits = AuxRightDigits;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� �����
    /// </summary>
    public FormatSignMode Sign { get { return FSign; } }
    private FormatSignMode FSign;

    /// <summary>
    /// ����� ������������ ���� ����� �� ���������� �����
    /// </summary>
    public int LeftDigits { get { return FLeftDigits; } }
    private int FLeftDigits;

    /// <summary>
    /// ����� ������������ ���� ������ �� ���������� �����
    /// </summary>
    public int RightDigits { get { return FRightDigits; } }
    private int FRightDigits;

    /// <summary>
    /// ����� �������������� ���� ("#") ������ �� ���������� �����
    /// </summary>
    public int AuxRightDigits { get { return FAuxRightDigits; } }
    private int FAuxRightDigits;

    #endregion

    #region ��������� �������������

    public override void ToString(StringBuilder sb, FormatKind Kind)
    {
      // TODO: ����
      // TODO: ����������� �����
      for (int i = 0; i < LeftDigits; i++)
        sb.Append('0');
      if (RightDigits > 0 || AuxRightDigits > 0)
        sb.Append('.');
      for (int i = 0; i < RightDigits; i++)
        sb.Append('0');
      for (int i = 0; i < AuxRightDigits; i++)
        sb.Append('#');
    }

    #endregion
  }

  #region ������������ DateTimePart

  [Serializable]
  public enum DateTimePart
  {
    /// <summary>
    /// ��� (������� "yy" ��� "yyyy")
    /// </summary>
    Year,

    /// <summary>
    /// ����� ������ (������� "M" � "MM")
    /// </summary>
    Month,

    /// <summary>
    /// ����������� �������� ������ ("MMM")
    /// </summary>
    MonthAbbreviated,

    /// <summary>
    /// ������ ������������ ������ ("MMMM")
    /// </summary>
    MonthName,

    /// <summary>
    /// ����� ��� (������� "d" � "dd")
    /// </summary>
    Day,

    /// <summary>
    /// ����������� ������������ ��� ������ (������ "ddd")
    /// </summary>
    DayOfWeekAbbreviated,

    /// <summary>
    /// ������ ������������ ��� ������
    /// </summary>
    DayOfWeekName,

    /// <summary>
    /// ��� � ������� 1-12 (������� "h" � "hh")
    /// </summary>
    Hour12,

    /// <summary>
    /// ��� � ������� 0-23 (������� "H" � "HH")
    /// </summary>
    Hour24,

    /// <summary>
    /// ������ (0-59) (������� "m" � "mm")
    /// </summary>
    Minute,

    /// <summary>
    /// ������� (0-59) (������� "s" � "ss")
    /// </summary>
    Second,

    /// <summary>
    /// ������� ����� ������ (������� "f" � "F").
    /// �������� Digits ������ ������������ ����� "f".
    /// �������� AuxDigits ������ �������������� ����� "F"
    /// </summary>
    SecondFraction,

    /// <summary>
    /// ������ "tt" (������ "t" �� ��������������)
    /// </summary>
    AMPM,

    DateSeparator,

    TimeSeparator
  }

  #endregion

  [Serializable]
  public class DateTimeFormatItem : FormatItem
  {
    #region �����������

    internal DateTimeFormatItem(DateTimePart Part)
    {
      FPart = Part;
    }

    internal DateTimeFormatItem(DateTimePart Part, int Digits)
    {
      FPart = Part;
      FDigits = Digits;
    }

    internal DateTimeFormatItem(DateTimePart Part, int Digits, int AuxDigits)
    {
      FPart = Part;
      FDigits = Digits;
      FAuxDigits = AuxDigits;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ����/�������
    /// </summary>
    public DateTimePart Part { get { return FPart; } }
    private DateTimePart FPart;

    /// <summary>
    /// ���������� ������ (���� ���������)
    /// </summary>
    public int Digits { get { return FDigits; } }
    private int FDigits;

    /// <summary>
    /// �������������� ����� ��� ������� ����� ������
    /// </summary>
    public int AuxDigits { get { return FAuxDigits; } }
    private int FAuxDigits;

    #endregion

    #region ��������� �������������

    public override void ToString(StringBuilder sb, FormatKind Kind)
    {
      switch (Kind)
      {
        case FormatKind.NetFramework:
          ToStringNet(sb);
          break;
        case FormatKind.Excel:
        case FormatKind.Calc:
          ToStringExcel(sb);
          break;
        default:
          throw new ArgumentException("Kind=" + Kind.ToString(), "Kind");
      }
    }

    private void ToStringNet(StringBuilder sb)
    {
      switch (Part)
      {
        case DateTimePart.Year:
          if (Digits > 2)
            sb.Append("yyyy");
          else
            sb.Append("yy");
          break;
        case DateTimePart.Month:
          if (Digits > 1)
            sb.Append("MM");
          else
            sb.Append("M");
          break;
        case DateTimePart.MonthAbbreviated:
          sb.Append("MMM");
          break;
        case DateTimePart.MonthName:
          sb.Append("MMMM");
          break;
        case DateTimePart.Day:
          if (Digits > 1)
            sb.Append("dd");
          else
            sb.Append("d");
          break;
        case DateTimePart.DayOfWeekAbbreviated:
          sb.Append("ddd");
          break;
        case DateTimePart.DayOfWeekName:
          sb.Append("dddd");
          break;
        case DateTimePart.Hour12:
          if (Digits > 1)
            sb.Append("hh");
          else
            sb.Append("h");
          break;
        case DateTimePart.Hour24:
          if (Digits > 1)
            sb.Append("HH");
          else
            sb.Append("H");
          break;
        case DateTimePart.Minute:
          if (Digits > 1)
            sb.Append("mm");
          else
            sb.Append("m");
          break;
        case DateTimePart.Second:
          if (Digits > 1)
            sb.Append("ss");
          else
            sb.Append("s");
          break;
        case DateTimePart.SecondFraction:
          for (int i = 0; i < Digits; i++)
            sb.Append('f');
          for (int i = 0; i < AuxDigits; i++)
            sb.Append('F');
          break;
        case DateTimePart.AMPM:
          sb.Append("tt");
          break;
        case DateTimePart.DateSeparator:
          sb.Append('/');
          break;
        case DateTimePart.TimeSeparator:
          sb.Append(':');
          break;
        default:
          throw new BugException("����������� �������� Part=" + Part.ToString());
      }
    }

    private void ToStringExcel(StringBuilder sb)
    {
      switch (Part)
      {
        case DateTimePart.Year:
          if (Digits > 2)
            sb.Append("YYYY");
          else
            sb.Append("YY");
          break;
        case DateTimePart.Month:
          if (Digits > 1)
            sb.Append("MM");
          else
            sb.Append("M");
          break;
        case DateTimePart.MonthAbbreviated:
          sb.Append("MMM");
          break;
        case DateTimePart.MonthName:
          sb.Append("MMMM");
          break;
        case DateTimePart.Day:
          if (Digits > 1)
            sb.Append("DD");
          else
            sb.Append("D");
          break;
        case DateTimePart.DayOfWeekAbbreviated:
          sb.Append("DDD");
          break;
        case DateTimePart.DayOfWeekName:
          sb.Append("DDDD");
          break;
        case DateTimePart.Hour12:
        case DateTimePart.Hour24:
          if (Digits > 1)
            sb.Append("HH");
          else
            sb.Append("H");
          break;
        case DateTimePart.Minute:
          if (Digits > 1)
            sb.Append("MM");
          else
            sb.Append("M");
          break;
        case DateTimePart.Second:
          if (Digits > 1)
            sb.Append("SS");
          else
            sb.Append("S");
          break;
        case DateTimePart.SecondFraction:
          // ���� ������� �� �������������� � Excel � Calc
          break;
        case DateTimePart.AMPM:
          sb.Append("AM/PM");
          break;
        case DateTimePart.DateSeparator:
          sb.Append('/');
          break;
        case DateTimePart.TimeSeparator:
          sb.Append(':');
          break;
        default:
          throw new BugException("����������� �������� Part=" + Part.ToString());
      }
    }

    #endregion
  }

  #region �������

  internal class FormatParser : ParserList
  {
    #region �����������

    private FormatParser(bool IsExcel)
    {
      if (IsExcel)
        base.Add(new BracketParser());
      Add(new LiteralParser());
      Add(new CharParser()); // ����������� ���������
    }

    #endregion

    #region ������ ��������

    /// <summary>
    /// ������� ������� ";"
    /// </summary>
    internal class GroupSepParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData Data)
      {
        if (Data.GetChar(Data.CurrPos) == ';')
          Data.Tokens.Add(new Token(Data, this, ";", Data.CurrPos, 1));
      }

      public IExpression CreateExpression(ParsingData Data, IExpression LeftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// ��� ����� ��� LCID � �������.
    /// ���������� ������� "[]". ������� �������� ����� ��� ������
    /// </summary>
    internal class BracketParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData Data)
      {
        if (Data.GetChar(Data.CurrPos) != '[')
          return;
        for (int pos = Data.CurrPos + 1; pos < Data.Text.Text.Length; pos++)
        {
          if (Data.GetChar(pos) == ']')
          {
            Token tk = new Token(Data, this, "[]", Data.CurrPos, pos - Data.CurrPos + 1,
              Data.Text.Text.Substring(Data.CurrPos + 1, pos - Data.CurrPos - 1));
            Data.Tokens.Add(tk);
            return;
          }
        }
        // �� ������ ����� �������
        Token tk2 = new Token(Data, this, "[]", Data.CurrPos, Data.Text.Text.Length - Data.CurrPos,
          Data.Text.Text.Substring(Data.CurrPos + 1), new ErrorMessageItem(ErrorMessageKind.Error,
          "�� ������� ����������� ������ \"]\""));
        Data.Tokens.Add(tk2);
      }

      public IExpression CreateExpression(ParsingData Data, IExpression LeftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// �������������� ���������, ����������� � ��������� ��� ��������� � �������� backslash.
    /// ������� ������� "Literal"
    /// </summary>
    internal class LiteralParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData Data)
      {
        if (Data.GetChar(Data.CurrPos) == '\\')
        {
          char ch = Data.GetChar(Data.CurrPos + 1);
          int Len = 2;
          ErrorMessageItem? Error = null;
          if (Data.CurrPos == (Data.Text.Text.Length - 1))
          {
            Len = 1;
            Error = new ErrorMessageItem(ErrorMessageKind.Error, "Escape-������ \\ �� ����� ���� ��������� � ������ �������");
          }
          Data.Tokens.Add(new Token(Data, this, "Literal", Data.CurrPos, Len, new string(ch, 1), Error));
          return;
        }

        if (Data.GetChar(Data.CurrPos) == '\'')
        {
          for (int pos = Data.CurrPos + 1; pos < Data.Text.Text.Length; pos++)
          {
            if (Data.GetChar(pos) == '\'')
            {
              // ������ ����� ������
              string Text = Data.Text.Text.Substring(Data.CurrPos + 1, pos - Data.CurrPos - 1);
              Data.Tokens.Add(new Token(Data, this, "Literal", Data.CurrPos, pos - Data.CurrPos + 1, Text));
              return;
            }
          }

          // �� ������ ����� ������
          string Text2 = Data.Text.Text.Substring(Data.CurrPos + 1);
          Data.Tokens.Add(new Token(Data, this, "Literal", Data.CurrPos, Data.Text.Text.Length - Data.CurrPos, Text2,
            new ErrorMessageItem(ErrorMessageKind.Error, "�� ������ ���������� ������ \' ������� ���������")));
          return;

        }
      }

      public IExpression CreateExpression(ParsingData Data, IExpression LeftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// �������������� ������������������ ���������� ��������.
    /// ������� ������� "Char". ������ �������� ������ �� ������ ��� ���������� ���������� ��������.
    /// ���� ������ ������ ���� ��������� � ������, �.�. �������� "��������"
    /// </summary>
    internal class CharParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData Data)
      {
        char ch = Data.GetChar(Data.CurrPos);
        int n = 1;
        for (int pos = Data.CurrPos + 1; pos < Data.Text.Text.Length; pos++)
        {
          if (Data.GetChar(pos) == ch)
            n++;
          else
            break;
        }

        Data.Tokens.Add(new Token(Data, this, "Char", Data.CurrPos, n, new string(ch, n)));
      }

      public IExpression CreateExpression(ParsingData Data, IExpression LeftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    #endregion

    #region ����������� ��������� �������

    /// <summary>
    /// ������ ������� Net Framework
    /// </summary>
    public static FormatParser NetParser = new FormatParser(false);

    /// <summary>
    /// ������ ������� Excel / Calc
    /// </summary>
    public static FormatParser ExcelParser = new FormatParser(true);

    #endregion
  }

  #endregion
}

#endif