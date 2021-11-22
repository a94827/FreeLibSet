// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using FreeLibSet.Core;

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
