using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2018, Ageyev A.V.
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

namespace AgeyevAV.IO
{
  /// <summary>
  /// ������������� ��� ���������� ���� � ����� ��� ��������
  /// ��������� ������ ��� ����������� � ������, ���������� � System.IO.Path
  /// �� ��������� ������� �������� � ��������� ������� � ����������
  /// </summary>
  [Serializable]
  public class RelPath
  {
    #region ������������

    /// <summary>
    /// ������� ������, ���������� ���������� ���� � ����� ��� ��������
    /// ���� �������� ������������� �� ����, �� �� ���������
    /// ������� ���� � Uri-������� �� �����������
    /// </summary>
    /// <param name="s"></param>
    public RelPath(string s)
    {
      try
      {
        // ���� ����� ���� �������� � �������
        string s2 = AbsPath.RemoveQuotes(s);
        s2 = AbsPath.RemoveDirNameSlash(s2);
        if (String.IsNullOrEmpty(s2))
          _Path = String.Empty;
        else
        {
          if (AbsPath.StartsWithUriScheme(s2))
            throw new NotImplementedException("������� �������������� ���� � ������� URI �� ��������������");
          else
          {
            _Path = s2;
          }
        }
      }
      catch (Exception e)
      {
        throw new ArgumentException("�� ������� ������������� \"" + s + "\" � ������������� ����. " + e.Message, e);
      }
    }
 

    /// <summary>
    /// ������� ���� �� ������ ��������, � ������������ ������ ������������
    /// </summary>
    /// <param name="basePath">������� �������</param>
    /// <param name="subNames">�������� �����������</param>
    public RelPath(RelPath basePath, params string[] subNames)
    {
      RelPath p1 = basePath;
      for (int i = 0; i < subNames.Length; i++)
      {
        // ������� �������
        string s2 = AbsPath.RemoveQuotes(subNames[i]);

        // ��������� ������� ������ ������������ � �������� ������������
        if (String.IsNullOrEmpty(s2))
          continue;

        if (s2.IndexOf(System.IO.Path.DirectorySeparatorChar) >= 0)
        {
          string[] a = s2.Split(System.IO.Path.DirectorySeparatorChar);
          for (int j = 0; j < a.Length; j++)
          {
            if (String.IsNullOrEmpty(a[j]))
              continue;
            p1 = p1 + a[j];
          }
        }
        else
          p1 = p1 + s2;
      }
      _Path = p1._Path;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� � �������� ��� �����
    /// ���� �������� � �����, ���������
    /// ��� ������������� �������� ������� � System.IO
    /// �������� � ������������.
    /// </summary>
    public string Path { get { return _Path; } }
    private readonly string _Path;

    /// <summary>
    /// ���������� true, ���� ���� �� �����
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Path); } }

    /// <summary>
    /// ���������� true, ���� ���� �������� ����������.
    /// �������� System.IO.Path.IsPathRooted().
    /// </summary>
    public bool IsAbsPath
    {
      get
      {
        return System.IO.Path.IsPathRooted(Path);
      }
    }

    /// <summary>
    /// ���������� Path
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (_Path == null)
        return String.Empty;
      else
        return _Path;
    }

    /// <summary>
    /// ���������� ����, ��������������� �������� ������
    /// ������������, ����� ������ ������ ������� � ����� �������� ������ � ������
    /// ������������ � �������� �����
    /// </summary>
    public string SlashedPath
    {
      get
      {
        if (String.IsNullOrEmpty(_Path))
          return String.Empty;
        if (_Path[_Path.Length - 1] == System.IO.Path.DirectorySeparatorChar)
          return _Path;
        else
          return _Path + System.IO.Path.DirectorySeparatorChar; // ���������� ���� 17.03.2017
      }
    }

    /// <summary>
    /// ���������� ����, ����������� � ������� (��� �������� � �������� ��������� ������� ����������)
    /// </summary>
    public string QuotedPath
    {
      get
      {
        if (IsEmpty)
          return String.Empty;

        // ��� ������. ������ ����� �� ����� ���� �������
        return "\"" + Path + "\"";
      }
    }

    #endregion

    #region ��������������

    /// <summary>
    /// ����������� � ���������� ����, ���������, ��� �������������, ������� ������� � �������� ��������.
    /// </summary>
    /// <param name="relPath"></param>
    /// <returns></returns>
    public static implicit operator AbsPath(RelPath relPath)
    {
      return new AbsPath(relPath.Path);
    }

    #endregion

    #region ���������� �����������

    /// <summary>
    /// ���������� �������������� ����.
    /// ���������� ������� System.IO.Path.Combine()
    /// </summary>
    /// <param name="basePath">�������� ����</param>
    /// <param name="subDir">����������</param>
    /// <returns>����� ������������� ����</returns>
    public static RelPath operator +(RelPath basePath, string subDir)
    {
      if (basePath.IsEmpty)
        //return new AbsPath(SubDir);
        throw new ArgumentException("������� ������� ������", "basePath");

      if (String.IsNullOrEmpty(subDir))
        return basePath;

      return new RelPath(System.IO.Path.Combine(basePath.Path, subDir));
    }

    /// <summary>
    /// ������������� �������������� ���� � �����������.
    /// ���� <paramref name="subPath"/> ������ ���������� ����, � �� �������������,
    /// �� ������������, � <paramref name="basePath"/> ������������.
    /// </summary>
    /// <param name="basePath">���������� ������� ����</param>
    /// <param name="subPath">������������� ����</param>
    /// <returns>���������� ����</returns>
    public static AbsPath operator +(AbsPath basePath, RelPath subPath)
    {
      if (subPath.IsEmpty)
        return basePath;
      if (subPath.IsAbsPath)
        return new AbsPath(subPath.Path);
      else
        return new AbsPath(basePath, subPath.Path);
    }

    #endregion
  }
}
