using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using FreeLibSet.Core;

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

namespace FreeLibSet.Parsing
{
  /// <summary>
  /// ������ ��� ����������� ���������� �������� 
  /// ������� ������� "Space". ���� ��������� �������� ���� ������, ��������� ������������ �������
  /// </summary>
  public class SpaceParser : IParser
  {
    #region ������������

    /// <summary>
    /// ������� ������, ������������ ������� "������", "����������� ������", "������� ������",
    /// "������� �������" � "���������"
    /// </summary>
    public SpaceParser()
      : this(' ', DataTools.NonBreakSpaceChar, '\r', '\n', 't')
    {
    }

    /// <summary>
    /// ������� ������, ������������ ��������� �������
    /// </summary>
    /// <param name="spaceChars">���������� �������, ������� ������ �������������� ��������</param>
    public SpaceParser(params char[] spaceChars)
    {
      _SpaceChars = spaceChars;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������������� ���������� �������.
    /// �������� � ������������
    /// </summary>
    public char[] SpaceChars { get { return _SpaceChars; } }
    private char[] _SpaceChars;

    #endregion

    #region IParser Members

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="data">������ ��������</param>
    public void Parse(ParsingData data)
    {
      int cnt = 0;
      for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
      {
        char ch = data.Text.Text[i];
        if (Array.IndexOf<char>(_SpaceChars, ch) < 0)
          break;
        cnt++;
      }

      if (cnt > 0)
        data.Tokens.Add(new Token(data, this, "Space", data.CurrPos, cnt));
    }

    /// <summary>
    /// ���������� null
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">������������</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����������� ����� ������
  /// ������������ ��� �������������� �������������� ������, ����� ����� ������ �������� �������� ��������,
  /// �������� �� �������.
  /// ������� ������� "NewLine" ������ 1 ��� 2 �������
  /// ���������� ������� CR � LF � ����� ����������
  /// ���� ����� ������ �������� ������� ���������� �������� (��� � ���������� C#), ������� ������������
  /// SpaceParser, ������ ������� ����� ������ � ������ ���������� ��������
  /// </summary>
  public class NewLineParser : IParser
  {
    #region IParser Members

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="data">������ ��������</param>
    public void Parse(ParsingData data)
    {
      switch (data.GetChar(data.CurrPos))
      {
        case '\n':
        case '\r':
          int Len = 1;
          switch (data.GetChar(data.CurrPos + 1))
          {
            case '\n':
            case '\r':
              if (data.GetChar(data.CurrPos + 1) != data.GetChar(data.CurrPos)) // ������ ���� �� ��� ���������� ������� ������
                Len++;
              break;
          }
          data.Tokens.Add(new Token(data, this, "NewLine", data.CurrPos, Len));
          break;
      }
    }

    /// <summary>
    /// ���������� null
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">������������</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����������� ������������, ������������ � ��������� ������� � ������� �� ����� ������,
  /// ��������, � C# - �����������, ������������ � ���� �������� "/", � ������� - � ���������
  /// ������� ����� ������ �� ������ � �������
  /// </summary>
  public class ToEOLCommentParser : IParser
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="startString">��������� �������</param>
    public ToEOLCommentParser(string startString)
    {
      if (String.IsNullOrEmpty(startString))
        throw new ArgumentNullException("startString");
      _StartString = startString;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������.
    /// �������� � ������������
    /// </summary>
    public string StartString { get { return _StartString; } }
    private string _StartString;

    #endregion

    #region IParser Members

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="data">������ ��������</param>
    public void Parse(ParsingData data)
    {
      if (data.StartsWith(_StartString, false))
      {
        TextPosition tp = data.Text.GetPosition(data.CurrPos);
        int cnt = data.Text.GetRowLength(tp.Row) - tp.Column;
        data.Tokens.Add(new Token(data, this, "Comment", data.CurrPos, cnt));
      }
    }

    /// <summary>
    /// ���������� null
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">������������</param>
    /// <returns>null</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      return null;
    }

    #endregion
  }
}