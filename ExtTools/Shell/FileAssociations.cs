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

//#define USE_TRACE // ����������� � �������� ����� �����

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using System.Diagnostics;
using Microsoft.Win32;
using FreeLibSet.Win32;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Shell
{
  /// <summary>
  /// �������� ������� "�������" ��� "������� � �������"
  /// </summary>
  [Serializable]
  public class FileAssociationItem
  {
    #region �����������

    internal FileAssociationItem(string progId, AbsPath programPath, string arguments, string displayName, AbsPath iconPath, int iconIndex, bool useURL, string infoSourceString)
    {
      if (String.IsNullOrEmpty(progId))
        throw new ArgumentNullException("progId");
      _ProgId = progId;

      if (programPath.IsEmpty)
        throw new ArgumentNullException("programPath");
      _ProgramPath = programPath;

      if (String.IsNullOrEmpty(arguments))
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            _Arguments = "\"%1\"";
            break;
          case PlatformID.Unix:
            _Arguments = "%U";
            break;
        }
      }
      else
        _Arguments = arguments;

      if (String.IsNullOrEmpty(displayName))
        _DisplayName=GetDisplayName(_ProgramPath);
      else
        _DisplayName = displayName;

      _IconPath = iconPath;
      _IconIndex = iconIndex;

      _UseURL = useURL;

#if DEBUG
      _InfoSourceString = infoSourceString;
#endif
    }

    /// <summary>
    /// ���������� ������������ ��� ��������� ��� ��������� ����.
    /// ���� ����������� ���� �� �������� �������, ������������ ��� ����� ��� ����������
    /// </summary>
    /// <param name="programPath">���� � ������������ �����</param>
    /// <returns>������������ ���</returns>
    internal static string GetDisplayName(AbsPath programPath)
    {
      if (programPath.IsEmpty)
        return String.Empty;
      string DisplayName = String.Empty;
      if (System.IO.File.Exists(programPath.Path))
      {
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(programPath.Path);
        if (!String.IsNullOrEmpty(fvi.FileDescription))
          DisplayName = fvi.FileDescription;
      }
      if (String.IsNullOrEmpty(DisplayName))
        DisplayName = programPath.FileNameWithoutExtension;

      return DisplayName;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������������� ProgId
    /// </summary>
    public string ProgId { get { return _ProgId; } }
    private string _ProgId;

    /// <summary>
    /// ���� � ������������ ����� ����������
    /// </summary>
    public AbsPath ProgramPath { get { return _ProgramPath; } }
    private AbsPath _ProgramPath;

    /// <summary>
    /// ��������� ��������� ������ ��� ������� ����������.
    /// �������� "%1" ���������� �� ���� � �����
    /// </summary>
    public string Arguments { get { return _Arguments; } }
    private string _Arguments;

    //public string CommandLine { get { return FCommandLine; } set { FCommandLine = value; } }

    /// <summary>
    /// ������������ ��� ��������� ��� ������ "������� � �������"
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// ���� � ����� � ������� ���������� ������.
    /// ����� ���� �� �����
    /// </summary>
    public AbsPath IconPath { get { return _IconPath; } }
    private AbsPath _IconPath;

    /// <summary>
    /// ������ ������ � �����.
    /// ��. �������� ������� Windows ExtractIcon()
    /// </summary>
    public int IconIndex { get { return _IconIndex; } }
    private int _IconIndex;
    /// <summary>
    /// ���� true, �� ��� ����������� ����� ����� � ��������� ������ ����� �������������� ����� "file:///"
    /// </summary>
    public bool UserURL { get { return true; } }
    private bool _UseURL;

#if DEBUG
    /// <summary>
    /// �������������� ����������, ��� ���� ������� ��� ����� (���� ������� ��� ��� ���������� ���������).
    /// ��� �������� ���������� ������ � ���������� ������
    /// </summary>
    public string InfoSourceString { get { return _InfoSourceString; } }
    private string _InfoSourceString;
#endif

    /// <summary>
    /// ���������� DisplayName
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region ���������� �������

    private ProcessStartInfo CreateProcessStartInfo(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException();

      ProcessStartInfo psi = new ProcessStartInfo();
      psi.UseShellExecute = false;
      psi.FileName = this.ProgramPath.Path;
      psi.Arguments = this.Arguments;
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          //case PlatformID.Win32S:
          //case PlatformID.WinCE:
          psi.Arguments = psi.Arguments.Replace("%1", filePath.Path);
          break;
        case PlatformID.Unix:
          psi.Arguments = psi.Arguments.Replace("%U", filePath.QuotedPath); // � LibreOffice � ������� �����
          psi.Arguments = psi.Arguments.Replace("%u", filePath.QuotedPath); // � FireFox - � ���������. � ��� �������?

          psi.Arguments = psi.Arguments.Replace("%F", filePath.QuotedPath); // � ��������� ��������� �������� Thunar
          psi.Arguments = psi.Arguments.Replace("%f", filePath.QuotedPath); // ��� ���������
          break;
        default:
          throw new PlatformNotSupportedException();
      }

      return psi;
    }

    /// <summary>
    /// ��������� ���������� � ��������� �������� � ���.
    /// </summary>
    /// <param name="filePath">���� � ���������</param>
    public void Execute(AbsPath filePath)
    {
      ProcessStartInfo psi = CreateProcessStartInfo(filePath);
#if USE_TRACE
      System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + ". Starting file association \"" + DisplayName + "\" for \"" + FilePath.Path + "\".");
      string s;
      try
      {
        s = LogoutTools.LogoutObjectToString(psi);
      }
      catch (Exception e)
      {
        s = "*** " + e.Message + " ***";
      }
      //System.Diagnostics.Trace.Indent();
      try
      {
        System.Diagnostics.Trace.WriteLine("Process information:");
        System.Diagnostics.Trace.WriteLine(s);
      }
      finally
      {
        //System.Diagnostics.Trace.Unindent();
      }
#endif

      try
      {
        Process.Start(psi);
      }
      catch (Exception e)
      {
        try
        {
          e.Data["ProcessStartInfo.FileName"] = psi.FileName;
          e.Data["ProcessStartInfo.Arguments"] = psi.Arguments;
          e.Data["ProcessStartInfo.UseShellExecute"] = psi.UseShellExecute;
        }
        catch { }

        throw;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� ��� ��������� ���� ������.
  /// ��� ��������� ���������� ����������� ����������� ����� FromFileExtension()
  /// </summary>
  [Serializable]
  public class FileAssociations : IReadOnlyObject
  {
    #region �����������

    private FileAssociations(bool isReadOnly)
    {
      _OpenWithItems = new OpenWithItemList();
      if (isReadOnly)
        SetReadOnly();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� "�������"
    /// </summary>
    public FileAssociationItem OpenItem
    {
      get { return _OpenItem; }
      set
      {
        ((IReadOnlyObject)this).CheckNotReadOnly();
        _OpenItem = value;
      }
    }
    private FileAssociationItem _OpenItem;

    [Serializable]
    private class OpenWithItemList : ListWithReadOnly<FileAssociationItem>
    {
      // ������������� ������������� ������� ����� NamedList.
      // �� �� ����� ������ ������������, �.�. ����� ������������� �������� ������� � �� ProgId � �� ProgramPath

      #region ������

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    /// <summary>
    /// ������� "������� � �������"
    /// </summary>
    public IList<FileAssociationItem> OpenWithItems { get { return _OpenWithItems; } }
    private OpenWithItemList _OpenWithItems;

    private bool OpenWithContains(FileAssociationItem item)
    {
      return OpenWithIndexOf(item) >= 0;
    }

    private int OpenWithIndexOf(FileAssociationItem item)
    {
      for (int i = 0; i < _OpenWithItems.Count; i++)
      {
        if (String.Equals(_OpenWithItems[i].ProgId, item.ProgId, StringComparison.OrdinalIgnoreCase))
          return i;
        if (_OpenWithItems[i].ProgramPath == item.ProgramPath)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// ������ ������, ��������� ������ ��� ������
    /// </summary>
    public static readonly FileAssociations Empty = new FileAssociations(true);

    /// <summary>
    /// ���������� true, ���� ���������� ���������� ����������� ��� ������������ �������
    /// </summary>
    public static bool IsSupported
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
          //case PlatformID.Win32S:
          //case PlatformID.WinCE:
          case PlatformID.Unix:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// ���� �� ����� ��������� ������ ���������� �������� ����������, ��� �����������
    /// � ���� ����.
    /// ���� ��������� ������ �������, �� �������� �������� null.
    /// </summary>
    public Exception Exception
    {
      get { return _Exception; }
      set
      {
        ((IReadOnlyObject)this).CheckNotReadOnly();
        _Exception = value;
      }
    }
    [NonSerialized]
    private Exception _Exception;

    #endregion

    #region ��������� ��� ���������� �����

    /// <summary>
    /// ������� ������ ���������� ��� ��������� ���������� �����.
    /// ����������� ��� Windows � Linux. 
    /// ��� ������ ������������ ������ ���������� ������ ������.
    /// ������������ ������ ���������� ��������� � ����� "������ ������".
    /// 
    /// ���� ����� ��������� ����� ������� ��� ������ ������.
    /// ����������� �������� EFPApp.FileExtAssociations, ������� ������������ �����������
    /// </summary>
    /// <param name="fileExt">���������� �����, ������� �����</param>
    /// <returns>������ ����������</returns>
    public static FileAssociations FromFileExtension(string fileExt)
    {
      try
      {
        FileAssociations Items = DoFromFileExtension(fileExt);
        Items.SetReadOnly();
        return Items;
      }
      catch (Exception e)
      {
        e.Data["FileExt"] = fileExt;
        LogoutTools.LogoutException(e, "������ �������� �������� ����������");
        return FromError(e);
      }
    }

    /// <summary>
    /// ���������� ������ ������ ���������� � �������� ������� ����������.
    /// </summary>
    /// <param name="e">������������� ����������</param>
    /// <returns>������ ������</returns>
    private static FileAssociations FromError(Exception e)
    {
      FileAssociations FA = new FileAssociations(false);
      FA.Exception = e;
      FA.SetReadOnly();
      return FA;
    }

    private static FileAssociations DoFromFileExtension(string fileExt)
    {
      // � ������ ��������� �� ������ ��� �������� IsSupported

      if (String.IsNullOrEmpty(fileExt))
        throw new ArgumentNullException("fileExt");
      if (fileExt[0] != '.')
        throw new ArgumentException("���������� ������ ���������� � �����", "fileExt");

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromFileExtension(fileExt);
        case PlatformID.Unix:
          string MimeType = Linux.GetMimeTypeFromFileExtension(fileExt);
          if (MimeType.Length == 0)
            return Empty;
          else
            return Linux.FromMimeType(MimeType);
        default:
          return Empty;
      }
    }

    #endregion

    #region ��������� ��� MIME-����

    /// <summary>
    /// ������� ������ ���������� ��� ��������� ���� MIME.
    /// ����������� ��� Windows � Linux. 
    /// ��� ������ ������������ ������ ���������� ������ ������.
    /// ������������ ������ ���������� ��������� � ����� "������ ������".
    /// 
    /// ��� Windows ���� ����� ����������� ����������.
    /// </summary>
    /// <param name="mimeType">MIME-���, ��������, "text/plain"</param>
    /// <returns>������ ����������</returns>
    public static FileAssociations FromMimeType(string mimeType)
    {
      try
      {
        FileAssociations Items = DoFromMimeType(mimeType);
        Items.SetReadOnly();
        return Items;
      }
      catch (Exception e)
      {
        e.Data["MimeType"] = mimeType;
        LogoutTools.LogoutException(e, "������ �������� �������� ���������� ��� MIME-����");
        return FromError(e);
      }
    }

    private static FileAssociations DoFromMimeType(string mimeType)
    {
      // � ������ ��������� �� ������ ��� �������� IsSupported


      if (String.IsNullOrEmpty(mimeType))
        throw new ArgumentNullException("mimeType");

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromMimeType(mimeType);
        case PlatformID.Unix:
          return Linux.FromMimeType(mimeType);
        default:
          return Empty;
      }
    }

    #endregion

    #region ��� ��������

    /// <summary>
    /// ������� ������ �������� ���������� ��� ��������� ���������.
    /// ��� Windows ������ ���������� ������������ ������� - explorer.exe.
    /// 
    /// ����������� �������� EFPApp.FileAssociations.ShowDirectory
    /// </summary>
    /// <returns></returns>
    public static FileAssociations FromDirectory()
    {
      try
      {
        FileAssociations Items = DoFromDirectory();
        Items.SetReadOnly();
        return Items;
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "������ �������� �������� ���������� ��� ���������");
        return FromError(e);
      }
    }

    private static FileAssociations DoFromDirectory()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromDirectory();
        case PlatformID.Unix:
          return Linux.FromDirectory();
        default:
          return Empty;
      }
    }

    #endregion

    #region �� ������� Windows

    private static class Windows
    {
      #region FromFileExtension

      internal static FileAssociations FromFileExtension(string fileExt)
      {
        FileAssociations FA = new FileAssociations(false);

        using (RegistryTree2 Tree = new RegistryTree2(true))
        {
          FromFileExtensionExplorer(fileExt, FA, Tree);
          FromFileExtensionsHKCR(fileExt, FA, Tree); 

          if (FA.OpenItem == null && FA.OpenWithItems.Count > 0)
            FA.OpenItem = FA.OpenWithItems[0];
          else if (FA.OpenItem != null)
          {
            if (!FA.OpenWithContains(FA.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              FA.OpenWithItems.Add(FA.OpenItem);
          }
        }

        return FA;
      }

      private static void FromFileExtensionExplorer(string fileExt, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key2 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\UserChoice"];
        if (Key2 != null)
        {
          FileAssociationItem Item2 = GetProgIdItem(tree, DataTools.GetString(Key2.GetValue("progid")), Key2.Name);
          if (Item2 != null)
          {
            fa.OpenItem = Item2;
            if (!fa.OpenWithContains(Item2))
              fa.OpenWithItems.Insert(0, Item2);
          }
        }

        // ������ - OpenWithList
        RegistryKey2 Key3 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\OpenWithList"];
        if (Key3 != null)
        {
          string MRUList = DataTools.GetString(Key3.GetValue("MRUList")); // ���������� ���������� �������
          for (int i = 0; i < MRUList.Length; i++)
          {
            string ValName = new string(MRUList[i], 1); // ������ �� ����� �����
            string ProgId3 = DataTools.GetString(Key3.GetValue(ValName));
            FileAssociationItem Item = GetProgIdItem(tree, ProgId3, Key3.Name);
            if (Item != null && (!fa.OpenWithContains(Item)))
              fa._OpenWithItems.Add(Item);
          }
        }
      }

      private static void FromFileExtensionsHKCR(string fileExt, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key1 = tree[@"HKEY_CLASSES_ROOT\" + fileExt];
        if (Key1 != null)
        {
          RegistryKey2 KeyOWPI = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithProgIds"];
          if (KeyOWPI != null)
          {
            string[] aProgIds = KeyOWPI.GetValueNames();
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem Item = GetProgIdItem(tree, aProgIds[i], KeyOWPI.Name);
              if (Item != null && (!fa.OpenWithContains(Item)))
                fa._OpenWithItems.Add(Item);
            }
          }
          RegistryKey2 KeyOWL = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithList"];
          if (KeyOWL != null)
          {
            string[] aProgIds = KeyOWL.GetSubKeyNames(); // � �� value names
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem Item = GetProgIdItem(tree, aProgIds[i], KeyOWL.Name);
              if (Item != null && (!fa.OpenWithContains(Item)))
                fa._OpenWithItems.Add(Item);
            }
          }

          FileAssociationItem Item0 = GetProgIdItem(tree, DataTools.GetString(Key1.GetValue(String.Empty)), Key1.Name);
          if (Item0 != null)
          {
            int p = fa.OpenWithIndexOf(Item0);
            if (p < 0)
            {
              fa.OpenWithItems.Insert(0, Item0);
              p = 0;
            }
            fa.OpenItem = fa.OpenWithItems[p]; // � �� Item0
          }
        }
      }

      private static FileAssociationItem GetProgIdItem(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (String.IsNullOrEmpty(progId))
          return null;

        RegistryKey2 KeyProgId = tree[@"HKEY_CLASSES_ROOT\" + progId];
        if (KeyProgId == null)
          return GetProgIdItemForExeFile(tree, progId, infoSourceString);

        return DoGetProgIdItem(tree, progId, KeyProgId, infoSourceString);
      }

      private static FileAssociationItem DoGetProgIdItem(RegistryTree2 tree, string progId, RegistryKey2 keyProgId, string infoSourceString)
      {
        string Cmd = tree.GetString(keyProgId.Name + @"\shell\open\command", String.Empty);
        if (String.IsNullOrEmpty(Cmd))
          return null;

        if (Cmd.IndexOf(@"%1", StringComparison.Ordinal) < 0)
          // ����� � ������� DDE �� ����������
          return null;

        string FileName, Arguments;
        if (!SplitFileNameAndArgs(Cmd, out FileName, out Arguments))
          return null;

        FileName = Environment.ExpandEnvironmentVariables(FileName);
        AbsPath Path = AbsPath.Create(FileName);
        if (Path.IsEmpty)
          return null; // 25.01.2019
        if (!System.IO.File.Exists(Path.Path))
          return null;

        string DisplayName = String.Empty;

        if (Path.FileName.ToLowerInvariant() == "rundll32.exe")
        {
          // 22.09.2019
          // ��������� ������ �� ���������� 

          string FileName2 = GetFileNameFromArgs(Arguments);
          if (!String.IsNullOrEmpty(FileName2))
          { 
            FileName2 = Environment.ExpandEnvironmentVariables(FileName2);
            AbsPath Path2 = AbsPath.Create(FileName2);
            if (!Path2.IsEmpty)
              DisplayName = FileAssociationItem.GetDisplayName(Path2);
          }
        }

        AbsPath IconPath = AbsPath.Empty;
        int IconIndex = 0;
        RegistryKey2 KeyDefIcon = tree[keyProgId.Name + @"\DefaultIcon"];
        if (KeyDefIcon != null)
        {
          string s = DataTools.GetString(KeyDefIcon.GetValue(String.Empty));
          if (!(s == "%1" || s == "\"%1\""))
          {
            ParseIconInfo(s, out IconPath, out IconIndex);
          }
        }
        if (IconPath.IsEmpty)
        {
          IconPath = Path;
          IconIndex = 0;
        }

        return new FileAssociationItem(progId, Path, Arguments, DisplayName, IconPath, IconIndex, false,
          infoSourceString + Environment.NewLine + keyProgId.Name + @"\shell\open\command");
      }

      /// <summary>
      /// �������� ��� ����� �� ������ ����������.
      /// ��� ����� ����� ���� � ��������
      /// </summary>
      /// <param name="arguments"></param>
      /// <returns></returns>
      private static string GetFileNameFromArgs(string arguments)
      {
        if (String.IsNullOrEmpty(arguments))
          return String.Empty;
        if (arguments[0] == '\"')
        {
          // ���� ����������� �������
          // TODO: ������� � ����� �����
          string s = arguments.Substring(1);
          int p=s.IndexOf('\"');
          if (p < 0)
            return string.Empty; // ������
          else
            return s.Substring(0, p);
        }
        else
        {
          // ��� ����� ��� �������.
          // ���������� ��� �� ������� �������
          int p = arguments.IndexOf(' ');
          if (p >= 0)
            return arguments.Substring(0, p);
          else
            return arguments;
        }
      }

      private static FileAssociationItem GetProgIdItemForExeFile(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (!progId.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
          return null;

        FileAssociationItem FAItem = GetProgIdItemForExeFileHKCRApplications(tree, progId, infoSourceString);
        if (FAItem != null)
          return FAItem;
        else
          return GetProgIdItemForExeFileAppPathes(tree, progId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileHKCRApplications(RegistryTree2 tree, string progId, string infoSourceString)
      {
        RegistryKey2 KeyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        if (KeyProgId == null && progId.IndexOf('\\') < 0)
        {
          // ����� ���� ������ ������ ��� EXE-�����, ��������, "notepad.exe", ����� ��� ���� ������
          // � ���������� "Applications"
          // ������������� ���������� ���� ���� ��������, ����� � ������ ����� ��� ��������
          progId = @"Applications\" + progId;
          KeyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        }
        if (KeyProgId == null)
          return null;

        return DoGetProgIdItem(tree, progId, KeyProgId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileAppPathes(RegistryTree2 tree, string progId, string infoSourceString)
      {
        try
        {
          // ��������� ������� - ����� ���� � ����������
          string KeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId;
          string FilePath = tree.GetString(KeyName, String.Empty);
          AbsPath Path = AbsPath.Create(FilePath); // 25.01.2019
          if (Path.IsEmpty)
            return null;
          if (!System.IO.File.Exists(Path.Path))
            return null;


          return new FileAssociationItem(progId, Path, "\"%1\"", String.Empty,
            Path, 0,
            tree.GetBool(KeyName, "useURL"),
            infoSourceString + Environment.NewLine + KeyName);
        }
        catch // System.Security.SecurityException
        {
          return null; // 27.05.2017
        }
      }

      private static void ParseIconInfo(string iconInfo, out AbsPath iconPath, out int iconIndex)
      {
        iconPath = AbsPath.Empty;
        iconIndex = 0;
        if (String.IsNullOrEmpty(iconInfo))
          return;

        string FileName;
        int p = iconInfo.LastIndexOf(',');
        if (p >= 0)
        {
          FileName = iconInfo.Substring(0, p);
          string sIconIndex = iconInfo.Substring(p + 1);
          int.TryParse(sIconIndex, out iconIndex);
        }
        else
        {
          FileName = iconInfo;
          iconIndex = 0;
        }
        FileName = Environment.ExpandEnvironmentVariables(FileName);
        if (FileName.IndexOf(System.IO.Path.DirectorySeparatorChar) < 0) // ��� ����� ��� ����
        {
          // 13.12.2018 �������� ����� � ��������� ��������
          iconPath = AbsPath.Create(AbsPath.Create(Environment.SystemDirectory), FileName);
          //if (!System.IO.File.Exists(IconPath.Path))
          //  IconPath = FileTools.FindExecutableFilePath(FileName); 
        }
        if (iconPath.IsEmpty)
          iconPath = AbsPath.Create(FileName);
      }

      #endregion

      #region FromMimeType

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations FA = new FileAssociations(false);

        using (RegistryTree2 Tree = new RegistryTree2(true))
        {
          FromMimeTypeHKCR_MIME(mimeType, FA, Tree);

          if (FA.OpenItem == null && FA.OpenWithItems.Count > 0)
            FA.OpenItem = FA.OpenWithItems[0];
          else if (FA.OpenItem != null)
          {
            if (!FA.OpenWithContains(FA.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              FA.OpenWithItems.Add(FA.OpenItem);
          }
        }

        return FA;
      }

      private static void FromMimeTypeHKCR_MIME(string mimeType, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key1 = tree[@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType];
        if (Key1 != null)
        {
          string ClsId = DataTools.GetString(Key1.GetValue("CLSID"));
          if (!String.IsNullOrEmpty(ClsId))
          {
            RegistryKey2 Key2 = tree[@"HKEY_CLASSES_ROOT\CLSID\" + ClsId + @"\ProgId"];
            if (Key2 != null)
            {
              string ProgId = DataTools.GetString(Key2.GetValue(String.Empty));
              FileAssociationItem FAItem = GetProgIdItem(tree, ProgId, Key1.Name);
              if (FAItem != null)
                fa.OpenWithItems.Add(FAItem);
            }
          }
        }
      }

      #endregion

      #region ���������� ��� ��������

      internal static FileAssociations FromDirectory()
      {
        // TODO: ����� ������ explorer.exe

        FileAssociations FA = new FileAssociations(false);
        AbsPath Path = FileTools.FindExecutableFilePath("explorer.exe");
        if (Path.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("explorer.exe not found");
#endif
          return FA;
        }

        FileAssociationItem FAItem = new FileAssociationItem("Explorer", Path, "%1", "Windows Explorer",
          Path, 0, false, "Fixed");
        FA.OpenWithItems.Add(FAItem);
        FA.OpenItem = FAItem;
        return FA;
      }

      #endregion
    }

    #endregion

    #region Linux

    private static class Linux
    {
      #region ���������� ����� -> MIME

      /// <summary>
      /// ������� ������������ ���������� MIME-�����.
      /// ����������� ��� ������ ���������
      /// ���� - ���������� ����� (� ������) � ������� ��������
      /// �������� - ��� MIME.
      /// </summary>
      private static Dictionary<string, string> _FileExtMimeDict = new Dictionary<string, string>();

      /// <summary>
      /// ���������� mime-��� ��� ���������� �����.
      /// </summary>
      /// <param name="fileExt">���������� �����</param>
      /// <returns>MIME-���</returns>
      internal static string GetMimeTypeFromFileExtension(string fileExt)
      {
        string mime;
        lock (_FileExtMimeDict)
        {
          if (_FileExtMimeDict.Count == 0)
            InitFileExtMimeDict();

          if (!_FileExtMimeDict.TryGetValue(fileExt.ToUpperInvariant(), out mime))
            mime = String.Empty;
        }
        return mime;
      }

      /// <summary>
      /// ������������� ������� FileExtMimeDict
      /// </summary>
      private static void InitFileExtMimeDict()
      {
#if USE_TRACE
        Trace.WriteLine("Loading file mime types ...");
#endif

        #region ��������� �����������

        _FileExtMimeDict[".TXT"] = "text/plain";
        _FileExtMimeDict[".HTM"] = "text/html";
        _FileExtMimeDict[".HTML"] = "text/html";
        _FileExtMimeDict[".XML"] = "text/xml";

        #endregion

        #region ��������� �� ������ XML

        // �� ����, ����� ������������� ����� "/usr/share/mime/packages/*.xml", � ����������� freedesktop.org.xml.

        try
        {
          string[] aFiles = System.IO.Directory.GetFiles("/usr/share/mime/packages", "*.xml", System.IO.SearchOption.TopDirectoryOnly);
          for (int i = 0; i < aFiles.Length; i++)
          {
#if USE_TRACE
            System.Diagnostics.Trace.WriteLine("Loading "+aFiles[i]+" ...");
#endif
            try
            {
              System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
              XmlDoc.Load(aFiles[i]);
              System.Xml.XmlNamespaceManager NmSpcMan = new System.Xml.XmlNamespaceManager(XmlDoc.NameTable);
              NmSpcMan.AddNamespace("Def", @"http://www.freedesktop.org/standards/shared-mime-info");
              System.Xml.XmlNodeList mtnodes = XmlDoc.SelectNodes("Def:mime-info/Def:mime-type", NmSpcMan);
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("  mime-type count=" + mtnodes.Count.ToString());
#endif
              foreach (System.Xml.XmlNode mtnode in mtnodes)
              {
                string mimetype = GetAttrStr(mtnode, "type");
                if (mimetype.Length == 0)
                  continue;

                foreach (System.Xml.XmlNode globnode in mtnode.SelectNodes("Def:glob", NmSpcMan))
                {
                  string pattern = GetAttrStr(globnode, "pattern");
                  if (!pattern.StartsWith("*."))
                    continue;

                  _FileExtMimeDict[pattern.Substring(1).ToUpperInvariant()] = mimetype;
                }
              }
            }
            catch (Exception e)
            {
              LogoutTools.LogoutException(e, "������ �������� " + aFiles[i].Length);
            }
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "������ ���������� ������� mime-�����");
        }

        #endregion

#if USE_TRACE
        Trace.WriteLine("File mime types loaded. Count=" + FileExtMimeDict.Count.ToString()+".");
#endif
      }

      private static string GetAttrStr(System.Xml.XmlNode node, string name)
      {
        System.Xml.XmlAttribute attr = node.Attributes[name];
        if (attr == null)
          return String.Empty;
        else
          return attr.Value;
      }

      #endregion

      #region ���������� ��� mime-�����

      /// <summary>
      /// ��� ��� ����� FileAssociations �������������� ����������������,
      /// ������ ���������� ��������� ����������
      /// </summary>
      private static object _SyncRoot = new object();

      private static string _DefaultsListFilePath = "~/.local/share/applications/defaults.list";

      private static string _MimeinfoCacheFilePath = "/usr/share/applications/mimeinfo.cache";

      /// <summary>
      /// ����� ����������� ����� "~/.local/share/applications/defaults.list"
      /// </summary>
      private static DateTime _DefaultsListFileTime;

      /// <summary>
      /// ����� ����������� ����� "/usr/share/applications/mimeinfo.cache"
      /// </summary>
      private static DateTime _MimeinfoCacheFileTime;

      /// <summary>
      /// ������� ������������ mime-����� � desktop-������.
      /// ���� - MIME-���, �������� - ������ ������� .desktop (����� ����� � �������)
      /// </summary>
      private static Dictionary<string, string> _MimeDesktopFiles;

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations FAs;
        lock (_SyncRoot)
        {
          if (NeedsRecreateMimeDesktopFiles())
            CreateMimeDesktopFiles();

          FAs = DoFromMimeType(mimeType);
        }
        return FAs;
      }

      private static FileAssociations DoFromMimeType(string mimeType)
      {
        string sDesktopFiles;
        if (!_MimeDesktopFiles.TryGetValue(mimeType, out sDesktopFiles))
          return FileAssociations.Empty; // ����������� mime-���
        if (String.IsNullOrEmpty(sDesktopFiles))
          return FileAssociations.Empty; // ��� ����������

#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Desktop files for MIMETYPE=\"" + MimeType + "\": \"" + sDesktopFiles + "\"");
#endif

        string[] aDesktopFiles = sDesktopFiles.Split(';');
        FileAssociations FAs = new FileAssociations(false);
        for (int i = 0; i < aDesktopFiles.Length; i++)
        {
          FileAssociationItem FA = CreateFromDesktopFile(aDesktopFiles[i]);
          if (FA != null)
            FAs.OpenWithItems.Add(FA);
        }
        if (FAs.OpenWithItems.Count > 0)
          FAs.OpenItem = FAs.OpenWithItems[0];
        FAs.SetReadOnly();
        return FAs;
      }

      private static FileAssociationItem CreateFromDesktopFile(string desktopFileName)
      {
        if (String.IsNullOrEmpty(desktopFileName))
          return null;
        if (!desktopFileName.EndsWith(".desktop"))
          desktopFileName += ".desktop";
        AbsPath DesktopFilePath = new AbsPath("/usr/share/applications/" + desktopFileName);
        if (!System.IO.File.Exists(DesktopFilePath.Path))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Desktop entry file not found: " + DesktopFilePath.Path);
#endif
          return null;
        }

        IniFile File = new IniFile(true);
        File.Load(DesktopFilePath);
        string DisplayName = File["Desktop Entry", "Name[" + LanguageStr + "]"]; // Name[ru]
        if (String.IsNullOrEmpty(DisplayName))
          DisplayName = File["Desktop Entry", "Name"];
        if (String.IsNullOrEmpty(DisplayName))
          DisplayName = desktopFileName;
        string sExec = File["Desktop Entry", "Exec"];
        if (String.IsNullOrEmpty(sExec))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("There is no \"Exec\" key in " + DesktopFilePath.Path);
#endif
          return null;
        }
        string FileName;
        string Arguments;
        if (!SplitFileNameAndArgs(sExec, out FileName, out Arguments))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot split \"Exec\" key=\"" + sExec + "\" found in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath ProgramPath = FileTools.FindExecutableFilePath(FileName);
        if (ProgramPath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot find executable file \"" + FileName + "\", defined in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath IconPath = AbsPath.Empty;
        string sIcon = File["Desktop Entry", "Icon"];

#if USE_TRACE
        //System.Diagnostics.Trace.WriteLine("Icon=" + sIcon);
#endif
        if (!String.IsNullOrEmpty(sIcon))
        {
          try
          {
            /*

            AbsPath IndexThemePath = new AbsPath("/usr/share/icons/default/index.theme");
            if (System.IO.File.Exists(IndexThemePath.Path))
            {
              IniFile IndexThemeFile = new IniFile();
              IndexThemeFile.Load(IndexThemePath.Path);
              string Theme = IndexThemeFile["Icon Theme", "Inherits"];
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("Theme=" + Theme);
#endif
              if (!String.IsNullOrEmpty(Theme))
              {
                AbsPath IconPath2 = new AbsPath("/usr/share/icons/" + Theme + "/" + sIcon + ".png");
#if USE_TRACE
                System.Diagnostics.Trace.WriteLine("Icon Path=" + IconPath2);
#endif
                if (System.IO.File.Exists(IconPath2.Path))
                  IconPath = IconPath2;
              }
            }
             * 
             * */

            // �.�., ��� ��������� ������� ����
            string[] a = System.IO.Directory.GetFiles("/usr/share/icons", sIcon + ".png", System.IO.SearchOption.AllDirectories);
            if (a.Length > 0)
              IconPath = new AbsPath(a[0]);
          }
          catch { }
        }

        return new FileAssociationItem(desktopFileName, ProgramPath, Arguments, DisplayName, IconPath, 0, false, DesktopFilePath.Path);
      }

      /// <summary>
      /// ���������� ������������� ����� �������, ��������, "ru"
      /// </summary>
      private static string LanguageStr
      {
        get
        {
          string s = System.Globalization.CultureInfo.CurrentUICulture.Name;
          int p = s.IndexOf('-');
          if (p >= 0)
            s = s.Substring(0, p);
          return s;
        }
      }

      /// <summary>
      /// ���� �� �������� ������� ���������� MimeDesktopFiles.
      /// </summary>
      /// <returns></returns>
      private static bool NeedsRecreateMimeDesktopFiles()
      {
        if (_MimeDesktopFiles == null)
          return true;
        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
          if (System.IO.File.GetLastWriteTime(_DefaultsListFilePath) != _DefaultsListFileTime)
            return true;
        }

        if (System.IO.File.Exists(_MimeinfoCacheFilePath))
        {
          if (System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath) != _MimeinfoCacheFileTime)
            return true;
        }
        return false;
      }

      private static void CreateMimeDesktopFiles()
      {
#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Recreating desktop file associations ...");
#endif
        _MimeDesktopFiles = new Dictionary<string, string>();

        #region ����� ������

        if (System.IO.File.Exists(_MimeinfoCacheFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + MimeinfoCacheFilePath);
#endif
          FreeLibSet.IO.IniFile Ini = new IniFile(true);
          Ini.Load(new AbsPath(_MimeinfoCacheFilePath));
          foreach (IniKeyValue Pair in Ini.GetKeyValues("MIME Cache"))
            _MimeDesktopFiles[Pair.Key] = Pair.Value;
          _MimeinfoCacheFileTime = System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath);
        }

        #endregion

        #region ���������������� ���������

        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + DefaultsListFilePath);
#endif
          FreeLibSet.IO.IniFile Ini = new IniFile(true);
          Ini.Load(new AbsPath(_DefaultsListFilePath));
          foreach (IniKeyValue Pair in Ini.GetKeyValues("Default Applications"))
            _MimeDesktopFiles[Pair.Key] = Pair.Value;
          _DefaultsListFileTime = System.IO.File.GetLastWriteTime(_DefaultsListFilePath);
        }

        #endregion

#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Desktop file associations finished. MIME type count=" + MimeDesktopFiles.Count.ToString());
#endif
      }

      #endregion

      #region ���������� ��� ���������

      internal static FileAssociations FromDirectory()
      {
        return FromMimeType("inode/directory");
      }

      #endregion
    }

    #endregion

    #region ��������������� ������

    private static bool SplitFileNameAndArgs(string commandLine, out string fileName, out string arguments)
    {
      fileName = String.Empty;
      arguments = String.Empty;
      if (String.IsNullOrEmpty(commandLine))
        return false;

      if (commandLine[0] == '\"')
      {
        // ��� ��������� � ��������
        StringBuilder sb = new StringBuilder();
        int pEndQuota = -1;
        for (int i = 1; i < commandLine.Length; i++)
        {
          if (commandLine[i] == '\"')
          {
            if (i < (commandLine.Length - 1))
            {
              char NextChar = commandLine[i + 1];
              if (NextChar == '\"') // ��������� �������
              {
                sb.Append('\"');
                i++; // ���������� ���� ������
                continue;
              }
            }
            pEndQuota = i;
            break;
          }
          else
            sb.Append(commandLine[i]);
        }
        fileName = sb.ToString();
        if (pEndQuota < 0)
          return false;

        if (pEndQuota < (commandLine.Length - 1))
          arguments = commandLine.Substring(pEndQuota + 1).TrimStart(' ');
        return true;
      }
      else
      {
        // ��� ��������� �� ���������� ���������� ��������
        int p = commandLine.IndexOf(' ');
        if (p >= 0)
        {
          fileName = commandLine.Substring(0, p);
          arguments = commandLine.Substring(p + 1);
        }
        else
          fileName = commandLine;
        return true;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    bool IReadOnlyObject.IsReadOnly
    {
      get { return _OpenWithItems.IsReadOnly; }
    }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      if (_OpenWithItems.IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    private void SetReadOnly()
    {
      _OpenWithItems.SetReadOnly();
    }

    #endregion
  }
}