// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using System.Reflection;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// �������, ������� ����� ������������ � �������� DepExprX ��� ���������� � ��������� ���������������� ���������� (RI).
  /// � RI ������ ������ ������������ �������� �� ����������� ���������������� ������, ���� �������� ������ � ���� ������� ���������.
  /// � ������� �� ����������� ������� Net Framework, ������ DepTools �� ������������� ����������.
  /// ������� � ��������� Ex ���������� ������� ��������� DepExprX �� ��������� ���������� DepValue.
  /// 
  /// ������� CreateXXX() ������������� ��� �������� �������������� ������� �� ��������� ��������� Type
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
    /// <returns>����� ������</returns>
    private static int Length(string s)
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
    public static DepValue<int> LengthEx(DepValue<string> s)
    {
      return new DepExpr1<int, string>(s, Length);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// ���������� true, ���� ������ �������� (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">����������� ������</param>
    /// <returns>������� �������� ������</returns>
    private static bool IsNotEmpty(string value)
    {
      return !String.IsNullOrEmpty(value);
    }

    /// <summary>
    /// ���������� true, ���� ������ �������� (!String.IsNullOrEmpty())
    /// </summary>
    /// <param name="value">����������� ������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<string> value)
    {
      return new DepExpr1<bool, string>(value, IsNotEmpty);
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
    private static string Substring(string s, int startIndex, int length)
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
    public static DepValue<string> SubstringEx(DepValue<string> s, DepValue<int> startIndex, DepValue<int> length)
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
    public static DepValue<string> SubstringEx(DepValue<string> s, int startIndex, int length)
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
    private static bool StartsWithOrdinal(string s, string substring)
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
    public static DepValue<bool> StartsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepValue<bool> StartsWithOrdinalEx(DepValue<string> s, string substring)
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
    private static bool StartsWithOrdinalIgnoreCase(string s, string substring)
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
    public static DepValue<bool> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepValue<bool> StartsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
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
    private static bool EndsWithOrdinal(string s, string substring)
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
    public static DepValue<bool> EndsWithOrdinalEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepValue<bool> EndsWithOrdinalEx(DepValue<string> s, string substring)
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
    private static bool EndsWithOrdinalIgnoreCase(string s, string substring)
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
    public static DepValue<bool> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, DepValue<string> substring)
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
    public static DepValue<bool> EndsWithOrdinalIgnoreCaseEx(DepValue<string> s, string substring)
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
    private static string ToString<T>(T value)
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
    public static DepValue<string> ToStringEx<T>(DepValue<T> value)
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
    private static bool RegexIsMatch(string s, string pattern)
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
    public static DepValue<bool> RegexIsMatchEx(DepValue<string> s, string pattern)
    {
      return new DepExpr2<bool, string, string>(s, pattern, RegexIsMatch);
    }

    #endregion

    #endregion

    #region DateTime

    #region ����������

    #region Year

    /// <summary>
    /// ���������� ���. ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Year(DateTime? dt)
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
    public static DepValue<int> YearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Year);
    }

    /// <summary>
    /// ���������� ���. ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Year(DateTime dt)
    {
      return dt.Year;
    }

    /// <summary>
    /// ���������� ���. 
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> YearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Year);
    }

    #endregion

    #region Month

    /// <summary>
    /// ���������� ����� (1-12). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Month(DateTime? dt)
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
    public static DepValue<int> MonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Month);
    }

    /// <summary>
    /// ���������� ����� (1-12)
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Month(DateTime dt)
    {
      return dt.Month;
    }

    /// <summary>
    /// ���������� ����� (1-12). 
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> MonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Month);
    }

    #endregion

    #region Day

    /// <summary>
    /// ���������� ���� ������ (1-31). ���� ���� �� ������, ���������� 0
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Day(DateTime? dt)
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
    public static DepValue<int> DayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<int, DateTime?>(dt, Day);
    }

    /// <summary>
    /// ���������� ���� ������ (1-31). 
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static int Day(DateTime dt)
    {
      return dt.Day;
    }

    /// <summary>
    /// ���������� ���� ������ (1-31).
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> DayEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<int, DateTime>(dt, Day);
    }

    #endregion

    #region DayOfWeek

    /// <summary>
    /// ���������� ���� ������. ���� ���� �� ������, ���������� �����������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static DayOfWeek DayOfWeek(DateTime? dt)
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
    public static DepValue<DayOfWeek> DayOfWeekEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime?>(dt, DayOfWeek);
    }

    /// <summary>
    /// ���������� ���� ������. 
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>�������� ����������</returns>
    private static DayOfWeek DayOfWeek(DateTime dt)
    {
      return dt.DayOfWeek;
    }

    /// <summary>
    /// ���������� ���� ������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DayOfWeek> DayOfWeekEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DayOfWeek, DateTime>(dt, DayOfWeek);
    }

    #endregion

    #endregion

    #region IsBottom/EndOfXXX()

    #region Year

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 1 ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsBottomOfYear(DateTime? dt)
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
    public static DepValue<bool> IsBottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfYear);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 1 ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsBottomOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsBottomOfYear);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 31 �������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsEndOfYear(DateTime? dt)
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
    public static DepValue<bool> IsEndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfYear);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� 31 �������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsEndOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsEndOfYear);
    }

    /// <summary>
    /// ���������� ���� 01 ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? BottomOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.BottomOfYear(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ���� 01 ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> BottomOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfYear);
    }

    /// <summary>
    /// ���������� ���� 01 ������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> BottomOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.BottomOfYear);
    }

    /// <summary>
    /// ���������� ���� 31 �������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? EndOfYear(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.EndOfYear(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ���� 31 �������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> EndOfYearEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfYear);
    }


    /// <summary>
    /// ���������� ���� 31 �������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> EndOfYearEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.EndOfYear);
    }

    #endregion

    #region Quarter

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsBottomOfQuarter(DateTime? dt)
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
    public static DepValue<bool> IsBottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfQuarter);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsBottomOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsBottomOfQuarter);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsEndOfQuarter(DateTime? dt)
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
    public static DepValue<bool> IsEndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfQuarter);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ��������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsEndOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsEndOfQuarter);
    }

    /// <summary>
    /// ���������� ���� ������ ��������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? BottomOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.BottomOfQuarter(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ���� ������ ��������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> BottomOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfQuarter);
    }

    /// <summary>
    /// ���������� ���� ������ ��������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepExpr1<DateTime, DateTime> BottomOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.BottomOfQuarter);
    }

    /// <summary>
    /// ���������� ���� ����� ��������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? EndOfQuarter(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.EndOfQuarter(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ���� ����� ��������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> EndOfQuarterEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfQuarter);
    }

    /// <summary>
    /// ���������� ���� ����� ��������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> EndOfQuarterEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.EndOfQuarter);
    }

    #endregion

    #region Month

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsBottomOfMonth(DateTime? dt)
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
    public static DepValue<bool> IsBottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsBottomOfMonth);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ������ ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsBottomOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsBottomOfMonth);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static bool IsEndOfMonth(DateTime? dt)
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
    public static DepValue<bool> IsEndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<bool, DateTime?>(dt, IsEndOfMonth);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���������� �� ��������� ���� ������
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsEndOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<bool, DateTime>(dt, DataTools.IsEndOfMonth);
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? BottomOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.BottomOfMonth(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> BottomOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, BottomOfMonth);
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> BottomOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.BottomOfMonth);
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��������� ��������</returns>
    private static DateTime? EndOfMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return DataTools.EndOfMonth(dt.Value);
      else
        return null;
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// ���������� null, ���� <paramref name="dt"/>=null.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime?> EndOfMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<DateTime?, DateTime?>(dt, EndOfMonth);
    }

    /// <summary>
    /// ���������� ������ ���� ������.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> EndOfMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<DateTime, DateTime>(dt, DataTools.EndOfMonth);
    }

    #endregion

    #endregion

    #endregion

    #region YearMonth

    #region ����������

    private static int Year(YearMonth value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Year;
    }

    /// <summary>
    /// ���������� ��� �� ��������� YearMonth.
    /// ���� YearMonth.IsEmpty=true, ���������� 0.
    /// </summary>
    /// <param name="value">����������� �������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> YearEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<int, YearMonth>(value, Year);
    }


    private static int Month(YearMonth value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Month;
    }

    /// <summary>
    /// ���������� ����� (1-12) �� ��������� YearMonth.
    /// ���� YearMonth.IsEmpty=true, ���������� 0.
    /// </summary>
    /// <param name="value">����������� �������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> MonthEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<int, YearMonth>(value, Month);
    }

    #endregion

    #region NBottom/EndOfMonth()

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    private static DateTime? NBottomOfMonth(YearMonth ym)
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
    public static DepValue<DateTime?> NBottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime?, YearMonth>(ym, NBottomOfMonth);
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ null
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    private static DateTime? NEndOfMonth(YearMonth ym)
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
    public static DepValue<DateTime?> NEndOfMonthEx(DepValue<YearMonth> ym)
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
    private static DateTime BottomOfMonth(YearMonth ym)
    {
      return ym.BottomOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ������� ��� ������.
    /// ��� YearMonth.Empty ������������ 01.01.0001.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> BottomOfMonthEx(DepValue<YearMonth> ym)
    {
      return new DepExpr1<DateTime, YearMonth>(ym, BottomOfMonth);
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ 31.12.9999.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����</returns>
    private static DateTime EndOfMonth(YearMonth ym)
    {
      return ym.EndOfMonth;
    }

    /// <summary>
    /// ���������� ����, ��������������� ���������� ��� ������.
    /// ��� YearMonth.Empty ������������ 31.12.9999.
    /// </summary>
    /// <param name="ym">����� � ���</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<DateTime> EndOfMonthEx(DepValue<YearMonth> ym)
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
    private static YearMonth YearMonth(DateTime? dt)
    {
      if (dt.HasValue)
        return YearMonth(dt.Value);
      else
        return new YearMonth();
    }

    /// <summary>
    /// �������������� ���� � ��������� YearMonth.
    /// ��� �������� null ���������� YearMonth.Empty
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<YearMonth, DateTime?>(dt, YearMonth);
    }


    /// <summary>
    /// �������������� ���� � ��������� YearMonth.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>��� � �����</returns>
    private static YearMonth YearMonth(DateTime dt)
    {
      if (dt.Year >= FreeLibSet.Calendar.YearMonth.MinYear && dt.Year <= FreeLibSet.Calendar.YearMonth.MaxYear)
        return new YearMonth(dt);
      else
        return new YearMonth();
    }

    /// <summary>
    /// �������������� ���� � ��������� YearMonth.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<YearMonth, DateTime>(dt, YearMonth);
    }


    private static YearMonth YearMonth(int year, int month)
    {
      if (year >= FreeLibSet.Calendar.YearMonth.MinYear && year <= FreeLibSet.Calendar.YearMonth.MaxYear &&
        month >= 1 && month <= 12)
        return new YearMonth(year, month);
      else
        return new YearMonth();
    }

    /// <summary>
    /// �������� ��������� YearMonth �� ���� � ������ (1-12).
    /// ���� ��� ��� ����� ����� ������������ ��������, ������������ YearMonth.Empty
    /// </summary>
    /// <param name="year">���</param>
    /// <param name="month">�����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<YearMonth> YearMonthEx(DepValue<int> year, DepValue<int> month)
    {
      return new DepExpr2<YearMonth, int, int>(year, month, YearMonth);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// ���������� true, ���� �������� �������� (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>�������� ��������</returns>
    private static bool IsNotEmpty(YearMonth value)
    {
      return !value.IsEmpty;
    }

    /// <summary>
    /// ���������� true, ���� �������� �������� (YearMonth.IsEmpty=false)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<YearMonth> value)
    {
      return new DepExpr1<bool, YearMonth>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region MonthDay

    #region ����������

    private static int Month(MonthDay value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Month;
    }

    /// <summary>
    /// ���������� ����� (1-12) �� ��������� MonthDay.
    /// ���� MonthDay.IsEmpty=true, ���������� 0.
    /// </summary>
    /// <param name="value">����������� �������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> MonthEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<int, MonthDay>(value, Month);
    }


    private static int Day(MonthDay value)
    {
      if (value.IsEmpty)
        return 0;
      else
        return value.Day;
    }

    /// <summary>
    /// ���������� ���� �� ��������� MonthDay.
    /// ���� MonthDay.IsEmpty=true, ���������� 0.
    /// </summary>
    /// <param name="value">����������� �������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> DayEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<int, MonthDay>(value, Day);
    }

    #endregion

    #region MonthDay

    /// <summary>
    /// �������������� ���� � ��������� MonthDay.
    /// ��� �������� null ���������� MonthDay.Empty
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>���������</returns>
    private static MonthDay MonthDay(DateTime? dt)
    {
      if (dt.HasValue)
        return MonthDay(dt.Value);
      else
        return new MonthDay();
    }

    /// <summary>
    /// �������������� ���� � ��������� MonthDay.
    /// ��� �������� null ���������� MonthDay.Empty
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<DateTime?> dt)
    {
      return new DepExpr1<MonthDay, DateTime?>(dt, MonthDay);
    }


    /// <summary>
    /// �������������� ���� � ��������� MonthDay.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>���������</returns>
    private static MonthDay MonthDay(DateTime dt)
    {
      return new MonthDay(dt);
    }

    /// <summary>
    /// �������������� ���� � ��������� MonthDay.
    /// </summary>
    /// <param name="dt">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<DateTime> dt)
    {
      return new DepExpr1<MonthDay, DateTime>(dt, MonthDay);
    }


    private static MonthDay MonthDay(int month, int day)
    {
      if (month < 1 || month > 12)
        return new MonthDay();

      if (day < 1 || day > DateTime.DaysInMonth(2021, month)) // ����������� ������������ ���
        return new MonthDay();

      return new MonthDay(month, day);
    }

    /// <summary>
    /// �������� ��������� MonthDay �� ������ (1-12) � ��� (1-28/30/31).
    /// ���� ��� ��� ����� ����� ������������ ��������, ������������ MonthDay.Empty
    /// </summary>
    /// <param name="month">�����</param>
    /// <param name="day">����</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<MonthDay> MonthDayEx(DepValue<int> month, DepValue<int> day)
    {
      return new DepExpr2<MonthDay, int, int>(month, day, MonthDay);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// ���������� true, ���� �������� �������� (MonthDay.IsEmpty=false)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>�������� ��������</returns>
    private static bool IsNotEmpty(MonthDay value)
    {
      return !value.IsEmpty;
    }

    /// <summary>
    /// ���������� true, ���� �������� �������� (MonthDay.IsEmpty=false)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsNotEmptyEx(DepValue<MonthDay> value)
    {
      return new DepExpr1<bool, MonthDay>(value, IsNotEmpty);
    }

    #endregion

    #region GetDate()

    /// <summary>
    /// ���������� ����, ������������� ��������� ����.
    /// ���� ��������� <paramref name="md"/> �� ����������������, ��� <paramref name="year"/> ������ ����������� ���, ������������ null.
    /// </summary>
    /// <param name="md">����� � ����</param>
    /// <param name="year">���</param>
    /// <param name="february29">���� true � ��� ����������, �� 28 ������� ���������� �� 29</param>
    /// <returns>���� ��� null</returns>
    private static DateTime? GetNDate(MonthDay md, int year, bool february29)
    {
      if (md.IsEmpty || year < DateRange.Whole.FirstDate.Year || year > DateRange.Whole.LastDate.Year)
        return null;
      else
        return md.GetDate(year, february29);
    }

    /// <summary>
    /// ���������� ����, ������������� ��������� ����.
    /// ���� ��������� <paramref name="md"/> �� ����������������, ��� <paramref name="year"/> ������ ����������� ���, ������������ null.
    /// </summary>
    /// <param name="md">����� � ����</param>
    /// <param name="year">���</param>
    /// <param name="february29">���� true � ��� ����������, �� 28 ������� ���������� �� 29</param>
    /// <returns>���� ��� null</returns>
    public static DepValue<DateTime?> GetNDateEx(DepValue<MonthDay> md, DepValue<int> year, bool february29)
    {
      return new DepExpr3<DateTime?, MonthDay, int, bool>(md, year, february29, GetNDate);
    }

    /// <summary>
    /// ���������� ����, ������������� ��������� ����.
    /// ���� ��������� <paramref name="md"/> �� ����������������, ��� <paramref name="year"/> ������ ����������� ���, ������������ DataTime.MinValue.
    /// </summary>
    /// <param name="md">����� � ����</param>
    /// <param name="year">���</param>
    /// <param name="february29">���� true � ��� ����������, �� 28 ������� ���������� �� 29</param>
    /// <returns>���� ��� null</returns>
    private static DateTime GetDate(MonthDay md, int year, bool february29)
    {
      if (md.IsEmpty || year < DateRange.Whole.FirstDate.Year || year > DateRange.Whole.LastDate.Year)
        return DateTime.MinValue;
      else
        return md.GetDate(year, february29);
    }

    /// <summary>
    /// ���������� ����, ������������� ��������� ����.
    /// ���� ��������� <paramref name="md"/> �� ����������������, ��� <paramref name="year"/> ������ ����������� ���, ������������ DataTime.MinValue.
    /// </summary>
    /// <param name="md">����� � ����</param>
    /// <param name="year">���</param>
    /// <param name="february29">���� true � ��� ����������, �� 28 ������� ���������� �� 29</param>
    /// <returns>���� ��� null</returns>
    public static DepValue<DateTime> GetDateEx(DepValue<MonthDay> md, DepValue<int> year, bool february29)
    {
      return new DepExpr3<DateTime, MonthDay, int, bool>(md, year, february29, GetDate);
    }

    #endregion

    #endregion

    #region Nullable

    #region ReplaceNull

    /// <summary>
    /// ������ ��� Nullable-�������� (�������� ?? � C#).
    /// �� ����� ������ ������������ � ���������� ����
    /// </summary>
    /// <param name="value">��������, ������� ����� ���� null</param>
    /// <param name="nullValue">���������� �������� ��� null</param>
    /// <returns>�������� <paramref name="value"/>.Value ��� <paramref name="nullValue"/>.</returns>
    private static T ReplaceNull<T>(T? value, T nullValue)
      where T : struct
    {
      return value ?? nullValue;
    }

    /// <summary>
    /// ������ ��� Nullable-�������� (�������� ?? � C#).
    /// </summary>
    /// <param name="value">��������, ������� ����� ���� null</param>
    /// <param name="nullValue">���������� �������� ��� null</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> ReplaceNullEx<T>(DepValue<T?> value, T nullValue)
      where T : struct
    {
      return new DepExpr2<T, T?, T>(value, nullValue, ReplaceNull);
    }

    /// <summary>
    /// ������ ��� Nullable-�������� (�������� ?? � C#) �� �������� �� ��������� default(T).
    /// </summary>
    /// <param name="value">��������, ������� ����� ���� null</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> ReplaceNullEx<T>(DepValue<T?> value)
      where T : struct
    {
      return new DepExpr2<T, T?, T>(value, default(T), ReplaceNull);
    }

    #endregion

    #region IsNotEmpty()

    /// <summary>
    /// ���������� true, ���� ���� Nullable-�������� (�������� HasValue)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>������� �������� ������</returns>
    private static bool IsNotEmpty<T>(T? value)
      where T : struct
    {
      return value.HasValue;
    }

    /// <summary>
    /// ���������� true, ���� ���� Nullable-�������� (�������� HasValue)
    /// </summary>
    /// <param name="value">����������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> IsNotEmptyEx<T>(DepValue<T?> value)
      where T : struct
    {
      return new DepExpr1<bool, T?>(value, IsNotEmpty);
    }

    #endregion

    #endregion

    #region ������������

    #region Enum <--> Int32

    /// <summary>
    /// �������������� ������������ � ������������� ��������
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="value">������������ ��������</param>
    /// <returns>��������������� ��������</returns>
    private static int EnumToInt<T>(T value)
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
    public static DepValue<int> EnumToIntEx<T>(DepValue<T> value)
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
    private static T EnumFromInt<T>(int value)
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
    public static DepValue<T> EnumFromIntEx<T>(DepValue<int> value)
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
    private static string EnumToString<T>(T value)
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
    public static DepValue<string> EnumToStringEx<T>(DepValue<T> value)
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
    private static T EnumFromString<T>(string value)
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
    public static DepValue<T> EnumFromStringEx<T>(DepValue<string> value)
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
    /// <param name="array">������</param>
    /// <returns>����� �������</returns>
    private static int Length<T>(T[] array)
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
    /// <param name="array">������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<int> LengthEx<T>(DepValue<T[]> array)
    {
      return new DepExpr1<int, T[]>(array, Length<T>);
    }

    #endregion

    #endregion

    #region �������������� �������

    #region Min()/Max()

    #region Min()

    /// <summary>
    /// ���������� ����������� ��������.
    /// ������ ���������� �� ����� ���� ������.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ��������</returns>
    private static T Min<T>(params T[] values)
      where T : IComparable<T>
    {
      if (values.Length == 0)
        throw new ArgumentException("������ ���������� ������");

      T res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i].CompareTo(res) < 0)
          res = values[i];
      }
      return res;
    }

    /// <summary>
    /// ���������� ����������� ��������.
    /// ������ ���������� �� ����� ���� ������.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> MinEx<T>(params DepValue<T>[] values)
      where T : IComparable<T>
    {
      return new DepExprTA<T, T>(values, Min<T>);
    }

    /// <summary>
    /// ���������� ����������� ��������.
    /// �������� null ������������.
    /// ���� ������ ���������� ������ ��� �� �������� ��������, �������� �� null - ������������ null.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ��������</returns>
    private static T? Min<T>(params T?[] values)
      where T : struct, IComparable<T>
    {
      T? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value.CompareTo(res.Value) < 0)
              res = values[i];
          }
          else
            res = values[i];
        }
      }
      return res;
    }

    /// <summary>
    /// ���������� ����������� ��������.
    /// �������� null ������������.
    /// ���� ������ ���������� ������ ��� �� �������� ��������, �������� �� null - ������������ null.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T?> MinEx<T>(params DepValue<T?>[] values)
      where T : struct, IComparable<T>
    {
      return new DepExprTA<T?, T?>(values, Min);
    }

    #endregion

    #region Max()

    /// <summary>
    /// ���������� ������������ ��������.
    /// ������ ���������� �� ����� ���� ������.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ��������</returns>
    private static T Max<T>(params T[] values)
      where T : IComparable<T>
    {
      if (values.Length == 0)
        throw new ArgumentException("������ ���������� ������");

      T res = values[0];
      for (int i = 1; i < values.Length; i++)
      {
        if (values[i].CompareTo(res) > 0)
          res = values[i];
      }
      return res;
    }

    /// <summary>
    /// ���������� ������������ ��������.
    /// ������ ���������� �� ����� ���� ������.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> MaxEx<T>(params DepValue<T>[] values)
      where T : IComparable<T>
    {
      return new DepExprTA<T, T>(values, Max<T>);
    }

    /// <summary>
    /// ���������� ������������ ��������.
    /// �������� null ������������.
    /// ���� ������ ���������� ������ ��� �� �������� ��������, �������� �� null - ������������ null.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ��������</returns>
    private static T? Max<T>(params T?[] values)
      where T : struct, IComparable<T>
    {
      T? res = null;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i].HasValue)
        {
          if (res.HasValue)
          {
            if (values[i].Value.CompareTo(res.Value) > 0)
              res = values[i];
          }
          else
            res = values[i];
        }
      }
      return res;
    }

    /// <summary>
    /// ���������� ������������ ��������.
    /// �������� null ������������.
    /// ���� ������ ���������� ������ ��� �� �������� ��������, �������� �� null - ������������ null.
    /// </summary>
    /// <param name="values">������ ����������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T?> MaxEx<T>(params DepValue<T?>[] values)
      where T : struct, IComparable<T>
    {
      return new DepExprTA<T?, T?>(values, Max);
    }

    #endregion

    #endregion

    #region InRange()

    /// <summary>
    /// ���������� true, ���� �������� ��������� � ��������� ���������.
    /// �������������� �������� � ������������ ���������.
    /// </summary>
    /// <typeparam name="T">�������� ��� (������, ��������) ������. ������ ������������ ��������� ��� ��������� ��������</typeparam>
    /// <param name="value">����������� ��������</param>
    /// <param name="minimum">����������� �������� ��� null, ���� ����������� �� ������</param>
    /// <param name="maximum">������������ �������� ��� null, ���� ����������� �� ������</param>
    /// <returns>true, ���� �������� ��������� ������ ���������</returns>
    private static bool InRange<T>(T value, T? minimum, T? maximum)
      where T : struct, IComparable<T>
    {
      if (minimum.HasValue)
      {
        if (value.CompareTo(minimum.Value) < 0)
          return false;
      }
      if (maximum.HasValue)
      {
        if (value.CompareTo(maximum.Value) > 0)
          return false;
      }
      return true;
    }

    /// <summary>
    /// ���������� true, ���� �������� ��������� � ��������� ���������.
    /// �������������� �������� � ������������ ���������.
    /// </summary>
    /// <typeparam name="T">�������� ��� (������, ��������) ������. ������ ������������ ��������� ��� ��������� ��������</typeparam>
    /// <param name="value">����������� ��������</param>
    /// <param name="minimum">����������� �������� ��� null, ���� ����������� �� ������</param>
    /// <param name="maximum">������������ �������� ��� null, ���� ����������� �� ������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> InRangeEx<T>(DepValue<T> value, T? minimum, T? maximum)
      where T : struct, IComparable<T>
    {
      return new DepExpr3<bool, T, T?, T?>(value, minimum, maximum, InRange);
    }

    /// <summary>
    /// ���������� true, ���� �������� ��������� � ��������� ���������.
    /// �������������� �������� � ������������ ���������.
    /// </summary>
    /// <typeparam name="T">�������� ��� (������, ��������) ������. ������ ������������ ��������� ��� ��������� ��������</typeparam>
    /// <param name="value">����������� ��������</param>
    /// <param name="minimum">����������� �������� ��� null, ���� ����������� �� ������</param>
    /// <param name="maximum">������������ �������� ��� null, ���� ����������� �� ������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<bool> InRangeEx<T>(DepValue<T> value, DepValue<T?> minimum, DepValue<T?> maximum)
      where T : struct, IComparable<T>
    {
      return new DepExpr3<bool, T, T?, T?>(value, minimum, maximum, InRange);
    }

    #endregion

    #endregion

    #region �������������� �����

#if! XXX // ���� �� ��������
    /// <summary>
    /// ���� ��� T �������� Nullable-����������, ���������� �������� ���, ��� �������� ��������� ���������.
    /// ����� ���������� null
    /// </summary>
    /// <param name="t">����������� ��� ������</param>
    /// <returns>������� �������� ��� ��� null</returns>
    private static Type GetNullableBaseType(Type t)
    {
      if (!t.IsGenericType)
        return null;

      if (t.GetGenericTypeDefinition() != typeof(Nullable<>))
        return null;

      Type[] a = t.GetGenericArguments();
#if DEBUG
      if (a.Length != 1)
        throw new BugException("GetGenericArguments() ��� ���� " + t.ToString());
#endif
      return a[0];
    }


    private class DepToType<T> : DepExprOA<T>
    {
      #region �����������

      public DepToType(IDepValue arg)
        : base(new IDepValue[1] { arg }, null)
      {
        BaseSetValue(Calculate(), false);
      }

      #endregion

      #region ������

      protected override T Calculate()
      {
        if (object.ReferenceEquals(Args[0].Value, null))
          return default(T);

        Type t2 = DepTools.GetNullableBaseType(typeof(T));

        if (t2 == null)
          return (T)(Convert.ChangeType(Args[0].Value, typeof(T)));
        else
          return (T)(Convert.ChangeType(Args[0].Value, t2));
      }

      #endregion
    }

    /// <summary>
    /// �������������� Object � ��������� ����.
    /// ��� �������������� ������������ ����� Convert.ChangeType().
    /// ���� �������� �������� ����� null, �� ������������ default(T).
    /// �������� � ������ ������, ������� nullable.
    /// ���� �������� ����������� �������� ��� ����� ���������� ���, ��� ������������ ��� ���������.
    /// ���� ��� ���� ��� ��� ������ ���������������, �� ������������ ������������ ���������.
    /// � ��������� ������ ��������� ����� ������ ���������������
    /// </summary>
    /// <typeparam name="T">��� ������, � �������� ��������� ��������������</typeparam>
    /// <param name="value">�������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> ToTypeEx2<T>(IDepValue value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      DepValue<T> v2 = value as DepValue<T>;
      if (v2 != null)
        return v2;

      if (!value.IsConst) // ����� ����� ����������
      {
        IDepExpr[] a = value.GetChildExpressions(false);
        for (int i = 0; i < a.Length; i++)
        {
          DepToType<T> v3 = a[i] as DepToType<T>;
          if (v3 != null)
            return v3;
        }
      }

      // ������� ����� ���������������
      return new DepToType<T>(value);
    }

#endif

    private static T ToType<T>(object[] a)
    {
      if (object.ReferenceEquals(a[0], null))
        return default(T);
      else
        return (T)(Convert.ChangeType(a[0], typeof(T)));
    }

    /// <summary>
    /// �������������� Object � ��������� ����.
    /// ��� �������������� ������������ ����� Convert.ChangeType().
    /// ���� �������� �������� ����� null, �� ������������ default(T).
    /// </summary>
    /// <typeparam name="T">��� ������, � �������� ��������� ��������������</typeparam>
    /// <param name="value">�������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T> ToTypeEx<T>(IDepValue value)
    {
      return new DepExprOA<T>(new IDepValue[1] { value }, ToType<T>);
    }

    private static T? ToNType<T>(object[] a)
      where T : struct
    {
      if (Object.ReferenceEquals(a[0], null))
        return null;
      else
      {
        T res = (T)(Convert.ChangeType(a[0], typeof(T)));
        return res;
      }
    }

    /// <summary>
    /// �������������� Object � ��������� Nullable-����.
    /// ���� �������� �������� ����� null, �� ������������ null.
    /// ����� ��� �������������� ������������ ����� Convert.ChangeType().
    /// </summary>
    /// <typeparam name="T">��� ������, � �������� ��������� ��������������</typeparam>
    /// <param name="value">�������� ��������</param>
    /// <returns>����������� ���������</returns>
    public static DepValue<T?> ToNTypeEx<T>(IDepValue value)
      where T : struct
    {
      return new DepExprOA<T?>(new IDepValue[1] { value }, ToNType<T>);
    }

    #endregion

    #region ������ Create()

    /// <summary>
    /// ������� ��������� ���������� ������ DepOutput, ��������� �������� ���������.
    /// ��������� ��������� IDepOutput.Value ����� �������� �� ��������� ��� ���� <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">��� ������, ������� ����� ��������� � ����� �������. ������ ���� �����.</param>
    /// <returns>����� ������ DepOutput</returns>
    public static IDepOutput CreateOutput(Type valueType)
    {
      if (valueType == null)
        throw new ArgumentNullException("valueType");

      Type t2 = typeof(DepOutput<>).MakeGenericType(valueType);
      ConstructorInfo ci = t2.GetConstructor(new Type[0]);
      return (IDepOutput)(ci.Invoke(DataTools.EmptyObjects));
    }

    /// <summary>
    /// ������� ��������� ���������� ������ DepInput, ��������� �������� ���������.
    /// ��������� ��������� IDepInput.Value ����� �������� �� ��������� ��� ���� <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">��� ������, ������� ����� ��������� � ����� �������. ������ ���� �����.</param>
    /// <returns>����� ������ DepInput</returns>
    public static IDepInput CreateInput(Type valueType)
    {
      if (valueType == null)
        throw new ArgumentNullException("valueType");

      Type t2 = typeof(DepInput<>).MakeGenericType(valueType);
      ConstructorInfo ci = t2.GetConstructor(new Type[0]);
      return (IDepInput)(ci.Invoke(DataTools.EmptyObjects));
    }

    /// <summary>
    /// ������ ������ �������� IDepInput.
    /// </summary>
    internal static readonly IDepInput[] EmptyInputs = new IDepInput[0];

    internal static IDepExpr[] EmptyDepExpr = new IDepExpr[0];

    #endregion
  }
}
