// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.IO
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
