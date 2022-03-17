// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Shell;
using FreeLibSet.IO;
using System.Drawing;
using FreeLibSet.Logging;

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
        FileAssociations faItems;
        if (!_FADict.TryGetValue(fileExt, out faItems))
        {
          faItems = FileAssociations.FromFileExtension(fileExt);
          _FADict.Add(fileExt, faItems);
        }
        return faItems;
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
        FileAssociations faItems = _ShowDirectory;
        if (faItems == null)
        {
          faItems = FileAssociations.FromDirectory();
          _ShowDirectory = faItems;
        }
        return faItems;
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
      string key = filePath.Path + ";" + iconIndex.ToString() + ";" + smallIcon.ToString();
      Image image;
      if (!_IconImageDict.TryGetValue(key, out image))
      {
        try
        {
          image = WinFormsTools.ExtractIconImage(filePath, iconIndex, smallIcon);
        }
        catch (Exception e)
        {
          e.Data["Path"] = filePath;
          e.Data["IconIndex"] = iconIndex;
          e.Data["SmallIcon"] = smallIcon;
          LogoutTools.LogoutException(e, "������ ���������� ������ �� �����");
          image = null;
        }
        _IconImageDict.Add(key, image);
      }
      return image;
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

      foreach (Image image in _IconImageDict.Values)
      {
        try
        {
          image.Dispose();
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
