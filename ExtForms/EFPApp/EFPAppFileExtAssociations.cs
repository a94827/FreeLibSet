using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Shell;
using FreeLibSet.IO;
using System.Drawing;
using FreeLibSet.Logging;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������� �������� EFPApp.FileExtAssociations.
  /// ������������ ����������� ������� ���������� �� ���������� ������.
  /// ����� ��������� ����������� ������� ����������.
  /// ������������ EFPFileAssociationsCommandItemsHandler.
  /// </summary>
  public sealed class EFPAppFileExtAssociations : IDisposable
  {
    #region ����������� � Dispose()

    internal EFPAppFileExtAssociations()
    {
      _FADict = new Dictionary<string, FileAssociations>();
      _IconImageDict = new Dictionary<string, Image>();
    }

    /// <summary>
    /// ���������� ���������� ��� �����������
    /// </summary>
    void IDisposable.Dispose()
    {
      Reset();
    }

    #endregion

    #region ������ � ����������� ������

    /// <summary>
    /// ���������� true, ���� ���������� ���������� ����������� ��� ������������ �������
    /// </summary>
    public bool IsSupported { get { return FileAssociations.IsSupported; } }

    // ������� ������������ ������� Cache, �.�. ������ ����� ������� ���������� ������:
    // ������ ".txt", ".html" � ".xml"

    private Dictionary<string, FileAssociations> _FADict;

    /// <summary>
    /// ���������� �������������� ������ �������� ����������.
    /// �� ����� ���������� null
    /// </summary>
    /// <param name="fileExt">���������� �����, ��������, ".txt"</param>
    /// <returns>�������� ����������</returns>
    public FileAssociations this[string fileExt]
    {
      get
      {
        FileAssociations FAItems;
        if (!_FADict.TryGetValue(fileExt, out FAItems))
        {
          FAItems = FileAssociations.FromFileExtension(fileExt);
          _FADict.Add(fileExt, FAItems);
        }
        return FAItems;
      }
    }

    /// <summary>
    /// ���������� �������������� ������ �������� ���������� ��� ��������� ��������.
    /// ��� Windows ���������� ������������ ���������� ��� ������� Windows Explorer.
    /// </summary>
    public FileAssociations ShowDirectory
    {
      get
      {
        FileAssociations FA = _ShowDirectory;
        if (FA == null)
        {
          FA = FileAssociations.FromDirectory();
          _ShowDirectory = FA;
        }
        return FA;
      }
    }
    private FileAssociations _ShowDirectory;

    #endregion

    #region ������ � �������

    //private struct FileIconInfo
    //{
    //  #region �����������

    //  public FileIconInfo(AbsPath IconPath, int IconIndex)
    //  {
    //    FIconPath = IconPath;
    //    FIconIndex = IconIndex;
    //  }

    //  #endregion
    //}

    /// <summary>
    /// ���� - ���� � ����� ���� ������ ������ ���� true/false
    /// �������� - ������
    /// </summary>
    private Dictionary<string, Image> _IconImageDict;

    /// <summary>
    /// �������� ������ �� �������� ����� ���������� �������.
    /// ����������� ������� WinFormsTools.ExtractIcon()
    /// ���� ��� ������ ��� ���������� �������, ������������ ������ ������� �������.
    /// ���� ���� �� ������ ��� � ����� ��� ������ � �������� ��������, ������������ null.
    /// ��� ��������, �������� �� Windows, ������ ���������� null.
    /// </summary>
    /// <param name="filePath">���� � �����</param>
    /// <param name="iconIndex">������ ������ � �����. 
    /// ��. �������� ������� Windows ExtractIcon ��� ExtractIconEx()</param>
    /// <param name="smallIcon">true - ������� ��������� ������ (16x16), false - ������� (32x32)</param>
    /// <returns>������ ��� null</returns>
    public Image GetIconImage(AbsPath filePath, int iconIndex, bool smallIcon)
    {
      string Key = filePath.Path + ";" + iconIndex.ToString() + ";" + smallIcon.ToString();
      Image Image;
      if (!_IconImageDict.TryGetValue(Key, out Image))
      {
        try
        {
          Image = WinFormsTools.ExtractIconImage(filePath, iconIndex, smallIcon);
        }
        catch (Exception e)
        {
          e.Data["Path"] = filePath;
          e.Data["IconIndex"] = iconIndex;
          e.Data["SmallIcon"] = smallIcon;
          LogoutTools.LogoutException(e, "������ ���������� ������ �� �����");
          Image = null;
        }
        _IconImageDict.Add(Key, Image);
      }
      return Image;
    }

    #endregion

    #region ����� �����������

    /// <summary>
    /// ����� �����������
    /// </summary>
    public void Reset()
    {
      _FADict.Clear();
      _ShowDirectory = null;

      foreach (Image Image in _IconImageDict.Values)
      {
        try
        {
          Image.Dispose();
        }
        catch { }
      }
      _IconImageDict.Clear();
    }

    /// <summary>
    /// ���� �� ����, ��� ����������� ��������� (� ������� SHChangeNotifyRegister ?)
    /// ����� SystemEvents ��� �� ��������.
    /// ���� ���� ���������� ���������� �� �������. ����������� ����� ��������, ������ ���� � �����
    /// ���� ���� ��������� ����������� ���������, ������������ EFPFileAssociationsCommandItemsHandler.
    /// </summary>
    internal void ResetFA()
    {
      _FADict.Clear();
    }

    #endregion
  }
}