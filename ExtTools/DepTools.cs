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

namespace AgeyevAV.DependedValues
{
  /// <summary>
  /// �������, ������� ����� ������������ � �������� DepExprX ��� ���������� � ��������� ���������������� ���������� (RI).
  /// � RI ������ ������ ������������ �������� �� ����������� ���������������� ������, ���� �������� ������ � ���� ������� ���������.
  /// � ������� �� ����������� ������� Net Framework, ������ DepTools �� ������������� ����������.
  /// ��� ������ ������� ���������� ������ ������� � ��������� Ex, ������� ���������� ������� ��������� DepExprX �� ��������� ���������� DepValue.
  /// </summary>
  public static class DepTools
  {
    #region ��������� �������

    #region Length()

    /// <summary>
    /// ���������� ����� ������ String.Length.
    /// ����� �������������� � ������������ ������ DepExpr1.
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>����� �������</returns>
    public static int Length(string s)
    {
      if (Object.ReferenceEquals(s, null))
        return 0;
      else
        return s.Length;
    }

    /// <summary>
    /// ��������� ����� ������ String.Length
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, string> LengthEx(DepValue<string> s)
    {
      return new DepExpr1<int, string>(s, Length);
    }

    #endregion

    #region Substring()

    /// <summary>
    /// ��������� ��������� (����� String.Substring()).
    /// ���� <paramref name="startIndex"/> �/��� <paramref name="length"/> ������� �� ������� ������, ������������ ������ ������.
    /// ����� �������������� � ������������ ������ DepExpr3.
    /// </summary>
    /// <param name="s">������</param>
    /// <param name="startIndex">��������� ������</param>
    /// <param name="length">����� ���������</param>
    /// <returns>���������</returns>
    public static string Substring(string s, int startIndex, int length)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (startIndex < 0 || length <= 0)
        return String.Empty;

      if (s.Length < startIndex)
        return String.Empty;

      if (s.Length < (startIndex + length))
        return s.Substring(startIndex);
      else
        return s.Substring(startIndex, length);
    }

    /// <summary>
    /// ��������� ��������� String.Substring().
    /// </summary>
    /// <param name="s">������</param>
    /// <param name="startIndex">��������� ������</param>
    /// <param name="length">����� ���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr3<string, string, int, int> SubstringEx(DepValue<string> s, DepValue<int> startIndex, DepValue<int> length)
    {
      return new DepExpr3<string, string, int, int>(s, startIndex, length, Substring);
    }

    /// <summary>
    /// ��������� ��������� String.Substring().
    /// </summary>
    /// <param name="s">������</param>
    /// <param name="startIndex">��������� ������</param>
    /// <param name="length">����� ���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr3<string, string, int, int> SubstringEx(DepValue<string> s, int startIndex, int length)
    {
      return new DepExpr3<string, string, int, int>(s, startIndex, length, Substring);
    }

    #endregion

    #region StartsWith()

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// ���� <paramref name="s"/> - ������ ������, ������������ false.
    /// ���� <paramref name="substring"/> - ������ ������, ������������ true.
    /// ���� ��� ������ ������ - ������������ false.
    /// ����� �������������� � ������������ ������ DepExpr2.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>������� ����������</returns>
    public static bool StartsWithOrdinal(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.StartsWith(substring, StringComparison.Ordinal);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> StartsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinal);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> StartsWithOrdinalEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinal);
    }    

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// ���� <paramref name="s"/> - ������ ������, ������������ false.
    /// ���� <paramref name="substring"/> - ������ ������, ������������ true.
    /// ���� ��� ������ ������ - ������������ false.
    /// ����� �������������� � ������������ ������ DepExpr2.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>������� ����������</returns>
    public static bool StartsWithOrdinalIgnoreCase(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.StartsWith(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinalIgnoreCase);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ���������� � �������� ���������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, StartsWithOrdinalIgnoreCase);
    }

    #endregion

    #region EndsWith()

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// ���� <paramref name="s"/> - ������ ������, ������������ false.
    /// ���� <paramref name="substring"/> - ������ ������, ������������ true.
    /// ���� ��� ������ ������ - ������������ false.
    /// ����� �������������� � ������������ ������ DepExpr2.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>������� ����������</returns>
    public static bool EndsWithOrdinal(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.EndsWith(substring, StringComparison.Ordinal);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> EndsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinal);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� � ������ �������� StringComparison.Ordinal.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> EndsWithOrdinalEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinal);
    }



    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// ���� <paramref name="s"/> - ������ ������, ������������ false.
    /// ���� <paramref name="substring"/> - ������ ������, ������������ true.
    /// ���� ��� ������ ������ - ������������ false.
    /// ����� �������������� � ������������ ������ DepExpr2.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>������� ����������</returns>
    public static bool EndsWithOrdinalIgnoreCase(string s, string substring)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (String.IsNullOrEmpty(substring))
        return true;
      return s.EndsWith(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinalIgnoreCase);
    }

    /// <summary>
    /// ���������� true, ���� ������ <paramref name="s"/> ������������� �������� ����������.
    /// ������������ ����� ��������� ��� ����� �������� StringComparison.OrdinalIgnoreCase.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="substring">���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
    {
      return new DepExpr2<bool, string, string>(s, substring, EndsWithOrdinalIgnoreCase);
    }

    #endregion

    #region ToString()

    /// <summary>
    /// �������������� � ������ (����� Object.ToString()). ������������, � ��������, ��� ���������� �����.
    /// ���� <paramref name="value"/>=null, �� ������������ ������ ������
    /// ����� �������������� � ������������ ������ DepExpr1.
    /// </summary>
    /// <typeparam name="T">��� ������</typeparam>
    /// <param name="value">������������� ��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString<T>(T value)
    {
      if (value == null)
        return String.Empty;
      else
        return value.ToString();
    }

    /// <summary>
    /// �������������� � ������ (����� Object.ToString()). ������������, � ��������, ��� ���������� �����.
    /// </summary>
    /// <typeparam name="T">��� ������</typeparam>
    /// <param name="value">������������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<string, T> ToStringEx<T>(DepValue<T> value)
    {
      return new DepExpr1<string, T>(value, ToString);
    }

    #endregion

    #region Regex.IsMatch()

    /// <summary>
    /// �������� ������������ ������ ����������� ���������.
    /// ���� ����� �������� ����������� ����� Regex.IsMatch(). ���� ��������� <paramref name="s"/>==null ��� <paramref name="pattern"/>==null, �� 
    /// ��� ���������� �� ������ ������. ���� ����� Regex.IsMatch() �������� ���������� (��-�� ������������� <paramref name="pattern"/>), �� ��� ��������������� � ������������ false.
    /// ����� �������������� � ������������ ������ DepExpr2.
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="pattern">���������� ���������</param>
    /// <returns>������� ������������</returns>
    public static bool RegexIsMatch(string s, string pattern)
    {
      if (s == null)
        s = String.Empty;
      if (pattern == null)
        pattern = String.Empty;

      try
      {
        return System.Text.RegularExpressions.Regex.IsMatch(s, pattern);
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// �������� ������������ ������ ����������� ���������.
    /// ���� ����� �������� ����������� ����� Regex.IsMatch(). 
    /// </summary>
    /// <param name="s">����������� ������</param>
    /// <param name="pattern">���������� ���������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<bool, string, string> RegexIsMatchEx(DepValue<string> s, string pattern)
    {
      return new DepExpr2<bool, string, string>(s, pattern, RegexIsMatch);
    }

    #endregion

    #endregion

    #region DateTime

    #region ����������

    /// <summary>
    /// ���������� ���. ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    public static int Year(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Year;
      else
        return 0;
    }

    /// <summary>
    /// ���������� ���. ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, DateTime?> YearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Year);
    }

    /// <summary>
    /// ���������� ����� (1-12). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    public static int Month(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Month;
      else
        return 0;
    }

    /// <summary>
    /// ���������� ����� (1-12). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, DateTime?> MonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Month);
    }

    /// <summary>
    /// ���������� ���� ������ (1-31). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    public static int Day(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.Day;
      else
        return 0;
    }

    /// <summary>
    /// ���������� ���� ������ (1-31). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, DateTime?> DayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Day);
    }

    /// <summary>
    /// ���������� ���� ������. ���� ���� �� ������, ���������� �����������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    public static DayOfWeek DayOfWeek(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.DayOfWeek;
      else
        return System.DayOfWeek.Sunday;
    }

    /// <summary>
    /// ���������� ���� ������. ���� ���� �� ������, ���������� �����������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DayOfWeek, DateTime?> DayOfWeekEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime?>(dt, DayOfWeek);
    }

    #endregion

    #region IsBottom/EndOfXXX()

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 1 ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsBottomOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 1 ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfYear);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 31 �������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsEndOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfYear(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 31 �������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfYear);
    }


    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsBottomOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfMonth);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsEndOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfMonth(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfMonth);
    }


    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsBottomOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsBottomOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsBottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfQuarter);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    public static bool IsEndOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.IsEndOfQuarter(dt.Value);
      else
        return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<bool, DateTime?> IsEndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfQuarter);
    }

    #endregion

    #endregion

    #region YearMonth

    #region NBottom/EndOfMonth()

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    public static DateTime? NBottomOfMonth(YearMonth ym)
    {
      if (ym.IsEmpty)
        return null;
      else
        return ym.BottomOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DateTime?, YearMonth> NBottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NBottomOfMonth);
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    public static DateTime? NEndOfMonth(YearMonth ym)
    {
      if (ym.IsEmpty)
        return null;
      else
        return ym.EndOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DateTime?, YearMonth> NEndOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NEndOfMonth);
    }

    #endregion

    #region Bottom/EndOfMonth()

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ 01.01.0001.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    public static DateTime BottomOfMonth(YearMonth ym)
    {
        return ym.BottomOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ 01.01.0001.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DateTime, YearMonth> BottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, BottomOfMonth);
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ 31.12.9999.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    public static DateTime EndOfMonth(YearMonth ym)
    {
      return ym.EndOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ 31.12.9999.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DateTime, YearMonth> EndOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, EndOfMonth);
    }

    #endregion

    #region YearMonth

    /// <summary>
    /// �������������� ���� � ��������� YearMonth.
    /// ��� �������� null ���������� YearMonth.Empty
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��� � �����</returns>
    public static YearMonth YearMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return new YearMonth(dt.Value);
      else
        return new YearMonth();
    }

    /// <summary>
    /// �������������� ���� � ��������� YearMonth.
    /// ��� �������� null ���������� YearMonth.Empty
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<YearMonth, DateTime?> YearMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<YearMonth, DateTime?>(dt, YearMonth);
    }

    #endregion

    #endregion

    #region Nullable

    /// <summary>
    /// ������ ��� Nullable-�������� (�������� ?? � C#).
    /// �� ����� ������ ������������ � ���������� ����
    /// </summary>
    /// <param name="value">��������, ������� ����� ���� null</param>
    /// <param name="nullValue">���������� �������� ��� null</param>
    /// <returns>�������� <paramref name="value"/>.Value ��� <paramref name="nullValue"/>.</returns>
    public static T ReplaceNull<T>(T? value, T nullValue)
      where T:struct
    {
      return value ?? nullValue;
    }

    /// <summary>
    /// ������ ��� Nullable-�������� (�������� ?? � C#).
    /// </summary>
    /// <param name="value">��������, ������� ����� ���� null</param>
    /// <param name="nullValue">���������� �������� ��� null</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr2<T, Nullable<T>, T> ReplaceNullEx<T>(DepValue<T?> value, T nullValue)
      where T:struct
    {
      return new DepExpr2<T, T?, T>(value, nullValue, ReplaceNull);
    }

    #endregion

    #region ������������

    #region Enum <--> Int32

    /// <summary>
    /// �������������� ������������ � ������������� ��������
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>��������������� ��������</returns>
    public static int EnumToInt<T>(T value)
      where T : struct
    {
      return Convert.ToInt32((object)value);
    }

    /// <summary>
    /// �������������� ������������ � ������������� ��������
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, T> EnumToIntEx<T>(DepValue<T> value)
      where T : struct
    {
      return new DepExpr1<int, T>(value, EnumToInt<T>);
    }

    /// <summary>
    /// �������������� �������������� �������� � ������������
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������� ��������</param>
    /// <returns>��������������� ��������</returns>
    public static T EnumFromInt<T>(int value)
      where T : struct
    {
      return (T)Enum.ToObject(typeof(T), value);
    }


    /// <summary>
    /// �������������� �������������� �������� � ������������
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<T,int> EnumFromIntEx<T>(DepValue<int> value)
      where T : struct
    {
      return new DepExpr1<T, int>(value, EnumFromInt<T>);
    }

    #endregion

    #region Enum <--> String

    /// <summary>
    /// �������������� ������������ � ��������� ��������.
    /// ��������� ������� ����� <typeparamref name="T"/>.ToString().
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>��������������� ��������</returns>
    public static string EnumToString<T>(T value)
      where T : struct
    {
      return value.ToString();
    }

    /// <summary>
    /// �������������� ������������ � ��������� ��������.
    /// ��������� ������� ����� <typeparamref name="T"/>.ToString().
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<string, T> EnumToStringEx<T>(DepValue<T> value)
      where T : struct
    {
      return new DepExpr1<string, T>(value, EnumToString);
    }

    /// <summary>
    /// �������������� ������������ � ��������� ��������.
    /// �������� StdConvert.TryParseEnum(). ��� ������ ������ ��� ������������ ������ ���������� �������� ������������, ��������������� 0.
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>��������������� ��������</returns>
    public static T EnumFromString<T>(string value)
      where T : struct
    {
      T res;
      if (StdConvert.TryParseEnum<T>(value, out res))
        return res;
      else
        return (T)Enum.ToObject(typeof(T), 0);
    }

    /// <summary>
    /// �������������� ������������ � ��������� ��������.
    /// �������� StdConvert.TryParseEnum(). ��� ������ ������ ��� ������������ ������ ���������� �������� ������������, ��������������� 0.
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<T, string> EnumFromStringEx<T>(DepValue<string> value)
      where T : struct
    {
      return new DepExpr1<T, string>(value, EnumFromString<T>);
    }

    #endregion

    #endregion

    #region �������

    #region Length()

    /// <summary>
    /// ���������� ����� ������� Array.Length.
    /// ���� ������ �� ������ ����� null, ���������� 0.
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>����� �������</returns>
    public static int Length<T>(T[] array)
    {
      if (Object.ReferenceEquals(array, null))
        return 0;
      else
        return array.Length;
    }

    /// <summary>
    /// ���������� ����� ������� Array.Length.
    /// ���� ������ �� ������ ����� null, ���������� 0.
    /// </summary>
    /// <param name="s">������</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<int, T[]> LengthEx<T>(DepValue<T[]> s)
    {
      return new DepExpr1<int, T[]>(s, Length<T>);
    }

    #endregion


    #endregion
  }
}
