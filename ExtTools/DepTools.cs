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
  /// �������, ������� ����� ������������ � �������� DepExprXXX ��� ���������� � ��������� ���������������� ���������� (RI).
  /// � RI ������ ������ ������������ �������� �� ����������� ���������������� ������, ���� �������� ������ � ���� ������� ���������.
  /// � ������� �� ����������� ������� Net Framework, ������ DepTools �� ������������� ����������.
  /// </summary>
  public static class DepTools
  {
    #region ��������� �������

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

    #endregion
  }
}
