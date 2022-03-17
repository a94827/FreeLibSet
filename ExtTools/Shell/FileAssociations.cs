// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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
        _DisplayName = GetDisplayName(_ProgramPath);
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
      string displayName = String.Empty;
      if (System.IO.File.Exists(programPath.Path))
      {
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(programPath.Path);
        if (!String.IsNullOrEmpty(fvi.FileDescription))
          displayName = fvi.FileDescription;
      }
      if (String.IsNullOrEmpty(displayName))
        displayName = programPath.FileNameWithoutExtension;

      return displayName;
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
        FileAssociations faItems = DoFromFileExtension(fileExt);
        faItems.SetReadOnly();
        return faItems;
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
      FileAssociations faItems = new FileAssociations(false);
      faItems.Exception = e;
      faItems.SetReadOnly();
      return faItems;
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
          string mimeType = Linux.GetMimeTypeFromFileExtension(fileExt);
          if (mimeType.Length == 0)
            return Empty;
          else
            return Linux.FromMimeType(mimeType);
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
        FileAssociations faItems = DoFromMimeType(mimeType);
        faItems.SetReadOnly();
        return faItems;
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
        FileAssociations faItems = DoFromDirectory();
        faItems.SetReadOnly();
        return faItems;
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
        FileAssociations faItems = new FileAssociations(false);

        using (RegistryTree2 tree = new RegistryTree2(true))
        {
          FromFileExtensionExplorer(fileExt, faItems, tree);
          FromFileExtensionsHKCR(fileExt, faItems, tree);

          if (faItems.OpenItem == null && faItems.OpenWithItems.Count > 0)
            faItems.OpenItem = faItems.OpenWithItems[0];
          else if (faItems.OpenItem != null)
          {
            if (!faItems.OpenWithContains(faItems.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              faItems.OpenWithItems.Add(faItems.OpenItem);
          }
        }

        return faItems;
      }

      private static void FromFileExtensionExplorer(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key2 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\UserChoice"];
        if (key2 != null)
        {
          FileAssociationItem item2 = GetProgIdItem(tree, DataTools.GetString(key2.GetValue("progid")), key2.Name);
          if (item2 != null)
          {
            faItems.OpenItem = item2;
            if (!faItems.OpenWithContains(item2))
              faItems.OpenWithItems.Insert(0, item2);
          }
        }

        // ������ - OpenWithList
        RegistryKey2 key3 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\OpenWithList"];
        if (key3 != null)
        {
          string mruList = DataTools.GetString(key3.GetValue("MRUList")); // ���������� ���������� �������
          for (int i = 0; i < mruList.Length; i++)
          {
            string valName = new string(mruList[i], 1); // ������ �� ����� �����
            string progId3 = DataTools.GetString(key3.GetValue(valName));
            FileAssociationItem item = GetProgIdItem(tree, progId3, key3.Name);
            if (item != null && (!faItems.OpenWithContains(item)))
              faItems._OpenWithItems.Add(item);
          }
        }
      }

      private static void FromFileExtensionsHKCR(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key1 = tree[@"HKEY_CLASSES_ROOT\" + fileExt];
        if (key1 != null)
        {
          RegistryKey2 keyOWPI = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithProgIds"];
          if (keyOWPI != null)
          {
            string[] aProgIds = keyOWPI.GetValueNames();
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem item = GetProgIdItem(tree, aProgIds[i], keyOWPI.Name);
              if (item != null && (!faItems.OpenWithContains(item)))
                faItems._OpenWithItems.Add(item);
            }
          }
          RegistryKey2 keyOWL = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithList"];
          if (keyOWL != null)
          {
            string[] aProgIds = keyOWL.GetSubKeyNames(); // � �� value names
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem item = GetProgIdItem(tree, aProgIds[i], keyOWL.Name);
              if (item != null && (!faItems.OpenWithContains(item)))
                faItems._OpenWithItems.Add(item);
            }
          }

          FileAssociationItem item0 = GetProgIdItem(tree, DataTools.GetString(key1.GetValue(String.Empty)), key1.Name);
          if (item0 != null)
          {
            int p = faItems.OpenWithIndexOf(item0);
            if (p < 0)
            {
              faItems.OpenWithItems.Insert(0, item0);
              p = 0;
            }
            faItems.OpenItem = faItems.OpenWithItems[p]; // � �� Item0
          }
        }
      }

      private static FileAssociationItem GetProgIdItem(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (String.IsNullOrEmpty(progId))
          return null;

        RegistryKey2 keyProgId = tree[@"HKEY_CLASSES_ROOT\" + progId];
        if (keyProgId == null)
          return GetProgIdItemForExeFile(tree, progId, infoSourceString);

        return DoGetProgIdItem(tree, progId, keyProgId, infoSourceString);
      }

      private static FileAssociationItem DoGetProgIdItem(RegistryTree2 tree, string progId, RegistryKey2 keyProgId, string infoSourceString)
      {
        string cmd = tree.GetString(keyProgId.Name + @"\shell\open\command", String.Empty);
        if (String.IsNullOrEmpty(cmd))
          return null;

        if (cmd.IndexOf(@"%1", StringComparison.Ordinal) < 0)
          // ����� � ������� DDE �� ����������
          return null;

        string fileName, arguments;
        if (!SplitFileNameAndArgs(cmd, out fileName, out arguments))
          return null;

        fileName = Environment.ExpandEnvironmentVariables(fileName);
        AbsPath path = AbsPath.Create(fileName);
        if (path.IsEmpty)
          return null; // 25.01.2019
        if (!System.IO.File.Exists(path.Path))
          return null;

        string displayName = String.Empty;

        if (path.FileName.ToLowerInvariant() == "rundll32.exe")
        {
          // 22.09.2019
          // ��������� ������ �� ���������� 

          string fileName2 = GetFileNameFromArgs(arguments);
          if (!String.IsNullOrEmpty(fileName2))
          {
            fileName2 = Environment.ExpandEnvironmentVariables(fileName2);
            AbsPath path2 = AbsPath.Create(fileName2);
            if (!path2.IsEmpty)
              displayName = FileAssociationItem.GetDisplayName(path2);
          }
        }

        AbsPath iconPath = AbsPath.Empty;
        int iconIndex = 0;
        RegistryKey2 keyDefIcon = tree[keyProgId.Name + @"\DefaultIcon"];
        if (keyDefIcon != null)
        {
          string s = DataTools.GetString(keyDefIcon.GetValue(String.Empty));
          if (!(s == "%1" || s == "\"%1\""))
          {
            ParseIconInfo(s, out iconPath, out iconIndex);
          }
        }
        if (iconPath.IsEmpty)
        {
          iconPath = path;
          iconIndex = 0;
        }

        return new FileAssociationItem(progId, path, arguments, displayName, iconPath, iconIndex, false,
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
          int p = s.IndexOf('\"');
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

        FileAssociationItem faItem = GetProgIdItemForExeFileHKCRApplications(tree, progId, infoSourceString);
        if (faItem != null)
          return faItem;
        else
          return GetProgIdItemForExeFileAppPathes(tree, progId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileHKCRApplications(RegistryTree2 tree, string progId, string infoSourceString)
      {
        RegistryKey2 keyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        if (keyProgId == null && progId.IndexOf('\\') < 0)
        {
          // ����� ���� ������ ������ ��� EXE-�����, ��������, "notepad.exe", ����� ��� ���� ������
          // � ���������� "Applications"
          // ������������� ���������� ���� ���� ��������, ����� � ������ ����� ��� ��������
          progId = @"Applications\" + progId;
          keyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        }
        if (keyProgId == null)
          return null;

        return DoGetProgIdItem(tree, progId, keyProgId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileAppPathes(RegistryTree2 tree, string progId, string infoSourceString)
      {
        try
        {
          // ��������� ������� - ����� ���� � ����������
          string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId;
          string filePath = tree.GetString(keyName, String.Empty);
          AbsPath path = AbsPath.Create(filePath); // 25.01.2019
          if (path.IsEmpty)
            return null;
          if (!System.IO.File.Exists(path.Path))
            return null;

          return new FileAssociationItem(progId, path, "\"%1\"", String.Empty,
            path, 0,
            tree.GetBool(keyName, "useURL"),
            infoSourceString + Environment.NewLine + keyName);
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

        string fileName;
        int p = iconInfo.LastIndexOf(',');
        if (p >= 0)
        {
          fileName = iconInfo.Substring(0, p);
          string sIconIndex = iconInfo.Substring(p + 1);
          int.TryParse(sIconIndex, out iconIndex);
        }
        else
        {
          fileName = iconInfo;
          iconIndex = 0;
        }
        fileName = Environment.ExpandEnvironmentVariables(fileName);
        if (fileName.IndexOf(System.IO.Path.DirectorySeparatorChar) < 0) // ��� ����� ��� ����
        {
          // 13.12.2018 �������� ����� � ��������� ��������
          iconPath = AbsPath.Create(AbsPath.Create(Environment.SystemDirectory), fileName);
          //if (!System.IO.File.Exists(IconPath.Path))
          //  IconPath = FileTools.FindExecutableFilePath(FileName); 
        }
        if (iconPath.IsEmpty)
          iconPath = AbsPath.Create(fileName);
      }

      #endregion

      #region FromMimeType

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations faItems = new FileAssociations(false);

        using (RegistryTree2 tree = new RegistryTree2(true))
        {
          FromMimeTypeHKCR_MIME(mimeType, faItems, tree);

          if (faItems.OpenItem == null && faItems.OpenWithItems.Count > 0)
            faItems.OpenItem = faItems.OpenWithItems[0];
          else if (faItems.OpenItem != null)
          {
            if (!faItems.OpenWithContains(faItems.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              faItems.OpenWithItems.Add(faItems.OpenItem);
          }
        }

        return faItems;
      }

      private static void FromMimeTypeHKCR_MIME(string mimeType, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key1 = tree[@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType];
        if (key1 != null)
        {
          string clsId = DataTools.GetString(key1.GetValue("CLSID"));
          if (!String.IsNullOrEmpty(clsId))
          {
            RegistryKey2 key2 = tree[@"HKEY_CLASSES_ROOT\CLSID\" + clsId + @"\ProgId"];
            if (key2 != null)
            {
              string progId = DataTools.GetString(key2.GetValue(String.Empty));
              FileAssociationItem faItem = GetProgIdItem(tree, progId, key1.Name);
              if (faItem != null)
                faItems.OpenWithItems.Add(faItem);
            }
          }
        }
      }

      #endregion

      #region ���������� ��� ��������

      internal static FileAssociations FromDirectory()
      {
        // TODO: ����� ������ explorer.exe

        FileAssociations faItems = new FileAssociations(false);
        AbsPath path = FileTools.FindExecutableFilePath("explorer.exe");
        if (path.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("explorer.exe not found");
#endif
          return faItems;
        }

        FileAssociationItem faItem = new FileAssociationItem("Explorer", path, "%1", "Windows Explorer",
          path, 0, false, "Fixed");
        faItems.OpenWithItems.Add(faItem);
        faItems.OpenItem = faItem;
        return faItems;
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
              System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
              xmlDoc.Load(aFiles[i]);
              System.Xml.XmlNamespaceManager nmSpcMan = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
              nmSpcMan.AddNamespace("Def", @"http://www.freedesktop.org/standards/shared-mime-info");
              System.Xml.XmlNodeList mtnodes = xmlDoc.SelectNodes("Def:mime-info/Def:mime-type", nmSpcMan);
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("  mime-type count=" + mtnodes.Count.ToString());
#endif
              foreach (System.Xml.XmlNode mtnode in mtnodes)
              {
                string mimetype = GetAttrStr(mtnode, "type");
                if (mimetype.Length == 0)
                  continue;

                foreach (System.Xml.XmlNode globnode in mtnode.SelectNodes("Def:glob", nmSpcMan))
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
        FileAssociations faItems;
        lock (_SyncRoot)
        {
          if (NeedsRecreateMimeDesktopFiles())
            CreateMimeDesktopFiles();

          faItems = DoFromMimeType(mimeType);
        }
        return faItems;
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
        FileAssociations faItems = new FileAssociations(false);
        for (int i = 0; i < aDesktopFiles.Length; i++)
        {
          FileAssociationItem faItem = CreateFromDesktopFile(aDesktopFiles[i]);
          if (faItem != null)
            faItems.OpenWithItems.Add(faItem);
        }
        if (faItems.OpenWithItems.Count > 0)
          faItems.OpenItem = faItems.OpenWithItems[0];
        faItems.SetReadOnly();
        return faItems;
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

        IniFile file = new IniFile(true);
        file.Load(DesktopFilePath);
        string displayName = file["Desktop Entry", "Name[" + LanguageStr + "]"]; // Name[ru]
        if (String.IsNullOrEmpty(displayName))
          displayName = file["Desktop Entry", "Name"];
        if (String.IsNullOrEmpty(displayName))
          displayName = desktopFileName;
        string sExec = file["Desktop Entry", "Exec"];
        if (String.IsNullOrEmpty(sExec))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("There is no \"Exec\" key in " + DesktopFilePath.Path);
#endif
          return null;
        }
        string fileName;
        string arguments;
        if (!SplitFileNameAndArgs(sExec, out fileName, out arguments))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot split \"Exec\" key=\"" + sExec + "\" found in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath programPath = FileTools.FindExecutableFilePath(fileName);
        if (programPath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot find executable file \"" + FileName + "\", defined in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath iconPath = AbsPath.Empty;
        string sIcon = file["Desktop Entry", "Icon"];

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
              iconPath = new AbsPath(a[0]);
          }
          catch { }
        }

        return new FileAssociationItem(desktopFileName, programPath, arguments, displayName, iconPath, 0, false, DesktopFilePath.Path);
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
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(new AbsPath(_MimeinfoCacheFilePath));
          foreach (IniKeyValue pair in ini.GetKeyValues("MIME Cache"))
            _MimeDesktopFiles[pair.Key] = pair.Value;
          _MimeinfoCacheFileTime = System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath);
        }

        #endregion

        #region ���������������� ���������

        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + DefaultsListFilePath);
#endif
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(new AbsPath(_DefaultsListFilePath));
          foreach (IniKeyValue pair in ini.GetKeyValues("Default Applications"))
            _MimeDesktopFiles[pair.Key] = pair.Value;
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
              char nextChar = commandLine[i + 1];
              if (nextChar == '\"') // ��������� �������
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
