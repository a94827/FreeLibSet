using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.IO;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using AgeyevAV.Win32;

namespace AgeyevAV
{
  #region ������������ OpenOfficeKind

  /// <summary>
  /// ��� �����: OpenOffice ��� LibreOffice
  /// </summary>
  public enum OpenOfficeKind
  {
    /// <summary>
    /// ��� �������������� �����
    /// </summary>
    Unknown,

    /// <summary>
    /// ���������� OpenOffice
    /// </summary>
    OpenOffice,

    /// <summary>
    /// ���������� Libre Office
    /// </summary>
    LibreOffice
  }

  #endregion

  #region ������������ OpenOfficeArchitecture

  /// <summary>
  /// ����������� ���������� LibreOffice
  /// </summary>
  public enum OpenOfficePlatform
  {
    /// <summary>
    /// ����������� ����������
    /// </summary>
    Unknown,

    /// <summary>
    /// 32-������ ����������
    /// </summary>
    x86,

    /// <summary>
    /// 64-������ ����������
    /// </summary>
    x64
  }

  #endregion

  /// <summary>
  /// ��������� ��� OpenOffice / Libre Office.
  /// </summary>
  public static class OpenOfficeTools
  {
    #region ������ ������������� �����

    #region ������������ InfoSource

    /// <summary>
    /// ������ �������� ���������� �� ������������� �����
    /// </summary>
    public enum InfoSource
    {
      /// <summary>
      /// ����������
      /// </summary>
      Unknown,

      /// <summary>
      /// �� ������ � ������� Windows.
      /// � ���� ������ �������� OfficeInfo.InfoSourceString �������� ������ �������
      /// </summary>
      Registry,

      /// <summary>
      /// �� ���������� ���������.
      /// � ���� ������ �������� OfficeInfo.InfoSourceString �������� ��� ���������� ("PATH")
      /// </summary>
      EnvironmentVariable,

      /// <summary>
      /// ����� ��� �������� �� ����������������� ����
      /// </summary>
      PredefinedPath
    }

    #endregion

    /// <summary>
    /// ���������� �� ����� ������������� ����� �����
    /// </summary>
    public sealed class OfficeInfo
    {
      #region ������������

      /// <summary>
      /// ������ ������������ ��� �������� ��������� ����������.
      /// </summary>
      /// <param name="programDir"></param>
      /// <param name="kind"></param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind)
        : this(programDir, kind, InfoSource.Unknown, String.Empty, OpenOfficePlatform.Unknown)
      {
      }

      /// <summary>
      /// ������ ������������ � ��������� ��������� ����������.
      /// </summary>
      /// <param name="programDir"></param>
      /// <param name="kind"></param>
      /// <param name="infoSource"></param>
      /// <param name="infoSourceString"></param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString)
        : this(programDir, kind, infoSource, infoSourceString, OpenOfficePlatform.Unknown)
      {
      }

      /// <summary>
      /// ������ ������������ � ��������� ���������
      /// </summary>
      /// <param name="programDir">������� � ������������ ������� (� ������� ��������� soffice.exe ��� soffice</param>
      /// <param name="kind">OpenOffice ��� LibeOffice</param>
      /// <param name="infoSource">������ �������� ���������� �� ������������� �����</param>
      /// <param name="infoSourceString">�������������� ����������, ��� ���� ������� ��� ����� (���� ������� ��� ��� ���������� ���������)</param>
      /// <param name="platform">32-bit ��� 64-bit</param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString, OpenOfficePlatform platform)
      {
        #region ����������� ����������

        if (programDir.IsEmpty)
          throw new ArgumentException("�� ����� ProgramDir", "programDir");

        _ProgramDir = programDir;
        _Kind = kind;
        _InfoSource = infoSource;
        _InfoSourceString = infoSourceString;
        _Platform = platform;

        #endregion

        #region ����������� ������

        try
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Unix:
              InitVersionUnix(); // 17.05.2016
              break;
            default:
              _Version = FileTools.GetFileVersion(OfficePath);
              break;
          }
        }
        catch
        {
          _Version = new Version(); // ������ ������
        }
        if (_Version == null)
          _Version = new Version();

        #endregion

        #region ����������� ������� �����������

        _HasWriter = IsCompExists("swriter");
        _HasCalc = IsCompExists("scalc");
        _HasImpress = IsCompExists("simpress");
        _HasDraw = IsCompExists("sdraw");
        _HasBase = IsCompExists("sbase");
        _HasMath = IsCompExists("smath");

        #endregion
      }

      /// <summary>
      /// ����������� ������ OpenOffice/LibreOffice � Linux.
      /// ������� �� ����, ��� ��� ������� ���������.
      /// �� ���� ���� �� ����� ������, �������� � ��������, ��� � Windows.
      /// ����������� ��������� ����.
      /// </summary>
      /// <returns></returns>
      private void InitVersionUnix()
      {
        AbsPath TextFile = new AbsPath(ProgramDir, "versionrc");
        if (!File.Exists(TextFile.Path))
          return;

        string[] aLines = System.IO.File.ReadAllLines(TextFile.Path); // ?? ���������
        for (int i = 0; i < aLines.Length; i++)
        {
          // ������� ������ �������� ���:
          // BuildVersion=1:5.0.3-rc2-0ubuntu1-trusty2

          if (aLines[i].StartsWith("BuildVersion="))
          {
            string s = aLines[i].Substring(13); // ����� ����� ���������
            int p = s.IndexOf(':');
            if (p < 0)
              return;
            s = s.Substring(p + 1);
            p = s.IndexOf('-');
            if (p >= 0)
              s = s.Substring(0, p);
            _Version = FileTools.GetVersionFromStr(s);
          }
        }
        // �� ����� ������
      }

      private bool IsCompExists(string appName)
      {
        AbsPath FilePath = new AbsPath(ProgramDir, appName + GetExeExtension());
        return File.Exists(FilePath.Path);
      }

      #endregion

      #region �������� ����� �-�����

      /// <summary>
      /// ���������� ��� �������������� ����� ��� Unknown, ���� �� ����������
      /// </summary>
      public OpenOfficeKind Kind { get { return _Kind; } }
      private readonly OpenOfficeKind _Kind;

      /// <summary>
      /// �������� ������� � ������������ ������� (� ������� ��������� soffice.exe ��� soffice). 
      /// ���������� AbsPath.Empty, ���� ���� �� ����������.
      /// </summary>
      public AbsPath ProgramDir { get { return _ProgramDir; } }
      private readonly AbsPath _ProgramDir;

      /// <summary>
      /// ���������� ������ �����
      /// </summary>
      public Version Version { get { return _Version; } }
      private /*readonly */ Version _Version;

      /// <summary>
      /// ����������� ����������
      /// </summary>
      public OpenOfficePlatform Platform { get { return _Platform; } }
      private readonly OpenOfficePlatform _Platform;


      /// <summary>
      /// ���������� ���������� ���� � ����� soffice.exe (��� soffice ��� Linux).
      /// </summary>
      public AbsPath OfficePath
      {
        get
        {
          return new AbsPath(ProgramDir, "soffice" + GetExeExtension());
        }
      }

      /// <summary>
      /// ���������� "LibreOffice" ��� "OpenOffice"
      /// </summary>
      public string KindName
      {
        get
        {
          switch (Kind)
          {
            case OpenOfficeKind.LibreOffice: return "LibreOffice";
            case OpenOfficeKind.OpenOffice: return "OpenOffice";
            default: return "Unknown Office";
          }
        }
      }


      #endregion

      #region ������� ��������� �����������

      #region Writer

      /// <summary>
      /// ���������� true, ���� ��������� "Writer" ����������
      /// </summary>
      public bool HasWriter { get { return _HasWriter; } }
      private readonly bool _HasWriter;

      /// <summary>
      /// ���������� ������ ���� � ����� swriter.exe
      /// </summary>
      public AbsPath WriterPath
      {
        get
        {
          if (HasWriter)
            return new AbsPath(ProgramDir, "swriter" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Writer" ��� "LibreOffice Writer"
      /// </summary>
      public string WriterDisplayName
      {
        get { return KindName + " Writer"; }
      }

      #endregion

      #region Calc

      /// <summary>
      /// ���������� true, ���� ��������� "Calc" ����������
      /// </summary>
      public bool HasCalc { get { return _HasCalc; } }
      private readonly bool _HasCalc;

      /// <summary>
      /// ���������� ������ ���� � ����� scalc.exe
      /// </summary>
      public AbsPath CalcPath
      {
        get
        {
          if (HasCalc)
            return new AbsPath(ProgramDir, "scalc" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Calc" ��� "LibreOffice Calc"
      /// </summary>
      public string CalcDisplayName
      {
        get { return KindName + " Calc"; }
      }

      #endregion

      #region Impress

      /// <summary>
      /// ���������� true, ���� ��������� "Impress" ����������
      /// </summary>
      public bool HasImpress { get { return _HasImpress; } }
      private readonly bool _HasImpress;

      /// <summary>
      /// ���������� ������ ���� � ����� simpress.exe
      /// </summary>
      public AbsPath ImpressPath
      {
        get
        {
          if (HasImpress)
            return new AbsPath(ProgramDir, "simpress" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Impress" ��� "LibreOffice Impress"
      /// </summary>
      public string ImpressDisplayName
      {
        get { return KindName + " Impress"; }
      }

      #endregion

      #region Draw

      /// <summary>
      /// ���������� true, ���� ��������� "Draw" ����������
      /// </summary>
      public bool HasDraw { get { return _HasDraw; } }
      private readonly bool _HasDraw;

      /// <summary>
      /// ���������� ������ ���� � ����� sdraw.exe
      /// </summary>
      public AbsPath DrawPath
      {
        get
        {
          if (HasDraw)
            return new AbsPath(ProgramDir, "sdraw" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Draw" ��� "LibreOffice Draw"
      /// </summary>
      public string DrawDisplayName
      {
        get { return KindName + " Draw"; }
      }

      #endregion

      #region Base

      /// <summary>
      /// ���������� true, ���� ��������� "Base" ����������
      /// </summary>
      public bool HasBase { get { return _HasBase; } }
      private readonly bool _HasBase;

      /// <summary>
      /// ���������� ������ ���� � ����� sbase.exe
      /// </summary>
      public AbsPath BasePath
      {
        get
        {
          if (HasBase)
            return new AbsPath(ProgramDir, "sbase" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Base" ��� "LibreOffice Base"
      /// </summary>
      public string BaseDisplayName
      {
        get { return KindName + " Base"; }
      }

      #endregion

      #region Math

      /// <summary>
      /// ���������� true, ���� ��������� "Math" ����������
      /// </summary>
      public bool HasMath { get { return _HasMath; } }
      private readonly bool _HasMath;

      /// <summary>
      /// ���������� ������ ���� � ����� smath.exe
      /// </summary>
      public AbsPath MathPath
      {
        get
        {
          if (HasCalc)
            return new AbsPath(ProgramDir, "smath" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// ���������� "OpenOffice Math" ��� "LibreOffice Math"
      /// </summary>
      public string MathDisplayName
      {
        get { return KindName + " Math"; }
      }

      #endregion

      /// <summary>
      /// ���������� ������ � �������������� ������������, ��������, "Writer,Calc,Impress". �����������-�������.
      /// ������������� ��� ���������� �����
      /// </summary>
      public string ComponentsCSVString
      {
        get
        {
          StringBuilder sb = new StringBuilder();
          AddToCSV(sb, HasWriter, "Writer");
          AddToCSV(sb, HasWriter, "Calc");
          AddToCSV(sb, HasWriter, "Impress");
          AddToCSV(sb, HasWriter, "Draw");
          AddToCSV(sb, HasWriter, "Base");
          AddToCSV(sb, HasWriter, "Math");
          return sb.ToString();
        }
      }

      private static void AddToCSV(StringBuilder sb, bool flag, string name)
      {
        if (!flag)
          return;
        if (sb.Length > 0)
          sb.Append(',');
        sb.Append(name);
      }

      #endregion

      #region ������ ��������

      /// <summary>
      /// ��� ���� ������� ��� ����� ����� (����� ������ Windows, ���������� ��������� ...)
      /// </summary>
      public InfoSource InfoSource { get { return _InfoSource; } }
      private readonly InfoSource _InfoSource;

      /// <summary>
      /// �������������� ����������, ��� ���� ������� ��� ����� (���� ������� ��� ��� ���������� ���������)
      /// </summary>
      public string InfoSourceString { get { return _InfoSourceString; } }
      private readonly string _InfoSourceString;

      /// <summary>
      /// ���������� �������� � ������ �����
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return KindName + " " + Version.ToString() + PlatformSuffix;
      }

      private string PlatformSuffix
      {
        get
        {
          switch (Platform)
          {
            case OpenOfficePlatform.x86:
              return " (32-bit)";
            case OpenOfficePlatform.x64:
              return " (64-bit)";
            default:
              return String.Empty;
          }
        }
      }

      #endregion

      #region �������� ����� � Open Office

      /// <summary>
      /// ������� ���� ���������� ��������� � ��������� OpenOffice / LibreOffice Writer
      /// </summary>
      /// <param name="fileName">������ ���� � ODT-�����</param>
      /// <param name="asTemplate">���� true, �� ���� ������������ ��� ������.
      /// � ��������� �� ����� �������� ��� �����, � ������� "���������" ��������� ������� ��� �����.
      /// ������������ ��� ���������� ������ "���������"</param>
      public void OpenWithWriter(AbsPath fileName, bool asTemplate)
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = WriterPath.Path;
        if (String.IsNullOrEmpty(psi.FileName))
          throw new BugException("��������� Writer �� �����������");
        psi.Arguments = "\"" + fileName.Path + "\"";
        if (asTemplate)
          psi.Arguments = "-n " + psi.Arguments;
        using (new FileRedirectionSupressor())
        {
          Process.Start(psi);
        }
      }

      /// <summary>
      /// ������� ���� ���������� ��������� � ��������� OpenOffice / LibreOffice Calc
      /// </summary>
      /// <param name="fileName">������ ���� � ODS-�����</param>
      /// <param name="asTemplate">���� true, �� ���� ������������ ��� ������.
      /// � ��������� �� ����� �������� ��� �����, � ������� "���������" ��������� ������� ��� �����.
      /// ������������ ��� ���������� ������ "���������"</param>
      public void OpenWithCalc(AbsPath fileName, bool asTemplate)
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = CalcPath.Path;
        if (String.IsNullOrEmpty(psi.FileName))
          throw new BugException("��������� Calc �� �����������");
        psi.Arguments = "\"" + fileName.Path + "\"";
        if (asTemplate)
          psi.Arguments = "-n " + psi.Arguments;

        using (new FileRedirectionSupressor())
        {
          Process.Start(psi);
        }
      }

      #endregion
    }

    /// <summary>
    /// ������ ������������ ����� OpenOffice � LibreOffice.
    /// ���� ���� ����������, �� ������ ������ �������� ���� �������.
    /// ������, ����� ���� ����������� ��������� ��������� ����� �����.
    /// � ���� ������ ������ ������� ������������ "����������������" ������, �� ������� ��������� ������� � �������� 0.
    /// ���� ��� �������������� �����, ������������ ������ ������.
    /// ���� ���������� ���������� ExtForms.dll, ��� ����������� "�����������" ����� ������� ������������ �������� EFPApp.UsedOpenOffice
    /// </summary>
    public static OfficeInfo[] Installations { get { return _Installations; } }
    private static OfficeInfo[] _Installations = InitInstallations();

    #region ����� ������������� �����

    private static OfficeInfo[] InitInstallations()
    {
      // ���� ����� �� ����� ����� ����������� ����������.
      // ������ ���� ������� ���������� � log-����
      try
      {
        List<OfficeInfo> lst = new List<OfficeInfo>();

        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
          case PlatformID.Win32S: // ???
            FindFromRegistry(lst);
            break;
          case PlatformID.Unix:
            FindFromPredefined(lst); // 11.05.2016
            FindFromPath(lst); // 22.05.2016
            break;
        }

        FindFromUnoPath(lst); // ��� Windows � Unix

        return lst.ToArray();
      }
      catch(Exception e)
      {
        Trace.WriteLine("Exception caught when detecting installed OpenOffices/LibreOffices: " + e.Message);
        return new OfficeInfo[0];
      }
    }

    private static void FindFromUnoPath(List<OfficeInfo> srcList)
    {
      string s = Environment.GetEnvironmentVariable("UNO_PATH");
      FindOrAddItem(srcList, new AbsPath(s), OpenOfficeKind.Unknown, InfoSource.EnvironmentVariable, "UNO_PATH", OpenOfficePlatform.Unknown);
    }

    #region ����� ��� Windows

    private static void FindFromRegistry(List<OfficeInfo> lst)
    {
      // ����� ����� ������
      // 11.01.2012
      // � 64-��������� ������ Windows ����� ������� ����������� � ������� Wow6432Node

      // 22.05.2016
      // ����� ����� ���� ����� HKEY_CURRENT_USER

      /*
       * �� ����� ����� �����������:
       * 1-���������� (Net Framework'�), �.�. � ������� ����������� ����������� ������
       * 2-Windows
       * 3-LibreOffice
       * 
       * � ������� ������� ����� �������, ������� ����� ������
       * 
       * ����������  Windows  LibreOffice  ���� �������                 ����������
       *   32-bit     32-bit     32-bit    HKxx\SOFTWARE\
       *              64-bit     32-bit    HKxx\SOFTWARE\               ����������� ���� ������� Wow6432Node
       *                         64-bit    �� ����, ��� �����
       *   64-bit     64-bit     32-bit    HKxx\SOFTWARE\Wow6432Node\
       *                         64-bit    HKxx\SOFTWARE
       */

      // 32-��������� ������ ����������
      if (EnvironmentTools.Is64BitOperatingSystem)
      {
        using (RegistryTree2 tree = new RegistryTree2(true, RegistryView2.Registry64))
        {
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\", lst, OpenOfficePlatform.x64);
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\", lst, OpenOfficePlatform.x86);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\", lst, OpenOfficePlatform.x64);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\", lst, OpenOfficePlatform.x86);
        }
      }
      else
      {
        using (RegistryTree2 tree = new RegistryTree2(true, RegistryView2.Default))
        {
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\", lst, OpenOfficePlatform.x86);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\", lst, OpenOfficePlatform.x86);
        }
      }
    }


    private static void FindFromRegistry2(RegistryTree2 tree, string keyNameBase, List<OfficeInfo> lst, OpenOfficePlatform platform)
    {
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform); // 18.05.2016 - ��� OpenOffice 4.1.2
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice.org\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform);
      FindFromRegistry3(tree, keyNameBase + @"LibreOffice\UNO\InstallPath", lst, OpenOfficeKind.LibreOffice, platform);
    }

    private static void FindFromRegistry3(RegistryTree2 tree, string keyName, List<OfficeInfo> lst, OpenOfficeKind kind, OpenOfficePlatform platform)
    {
      // 30.09.2013
      // ����� �� ���� ������� � ����� �������
      try
      {
        AbsPath ProgramDir = new AbsPath(tree.GetString(keyName, String.Empty));
        if (ProgramDir.IsEmpty)
          return;

        FindOrAddItem(lst, ProgramDir, kind, InfoSource.Registry, keyName, platform);
      }
      catch
      {
      }
    }

    #endregion

    #region ����� ��� Linux

    private static void FindFromPath(List<OfficeInfo> lst)
    {
      string PathVar=Environment.GetEnvironmentVariable("PATH");
      if (String.IsNullOrEmpty(PathVar))
        return;

      string[] a = PathVar.Split(System.IO.Path.PathSeparator);
      for (int i = 0; i < a.Length; i++)
        FindOrAddItem(lst, new AbsPath(a[i]), OpenOfficeKind.Unknown, InfoSource.EnvironmentVariable, "Path", OpenOfficePlatform.Unknown);
    }

    private static void FindFromPredefined(List<OfficeInfo> lst)
    {
      AbsPath Dir = new AbsPath("/usr/lib/libreoffice/program");
      if (File.Exists(new AbsPath(Dir, "soffice").Path))
        FindOrAddItem(lst, Dir, OpenOfficeKind.LibreOffice, InfoSource.PredefinedPath, String.Empty, OpenOfficePlatform.Unknown);
      Dir = new AbsPath("/usr/lib/openoffice/program"); // !! ��������� ��� �����
      if (File.Exists(new AbsPath(Dir, "soffice").Path))
        FindOrAddItem(lst, Dir, OpenOfficeKind.OpenOffice, InfoSource.PredefinedPath, String.Empty, OpenOfficePlatform.Unknown);
    }

    #endregion

    #region ��������������� ������ ������

    private static void FindOrAddItem(List<OfficeInfo> lst, AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString, OpenOfficePlatform platform)
    {
      if (programDir.IsEmpty)
        return;

      if (!Directory.Exists(programDir.Path))
        return; // ��������

      AbsPath SOfficePath = new AbsPath(programDir, "soffice" + GetExeExtension());
      if (!File.Exists(SOfficePath.Path))
        return;

      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        AbsPath SOfficeBinPath = new AbsPath(programDir, "soffice.bin");
        if (!File.Exists(SOfficeBinPath.Path))
          return; // soffice ����� ���� ���������� �������. �������� �� �����������
      }

      // �� ��������� ��������, ��������� ������� � ������ ������ �� ����
      for (int i = 0; i < lst.Count; i++)
      {
        if (lst[i].ProgramDir == programDir)
          return;
      }

      lst.Add(new OfficeInfo(programDir, kind, infoSource, infoSourceString, platform));
    }

    private static string GetExeExtension()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE: // ???
          return ".exe";
        default:
          return String.Empty;
      }
    }

    #endregion

    #endregion

    /// <summary>
    /// ��������� ������ Installations.
    /// </summary>
    public static void RefreshInstalls()
    {
      _Installations = InitInstallations();
    }


    #endregion

    #region ����� Open Document Format

    #region ���������

    const string nmspcStyle = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    const string nmspcNumber = "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0";
    const string nmspcLoext = "urn:org:documentfoundation:names:experimental:office:xmlns:loext:1.0"; // 18.11.2016

    #endregion

    #region ������� ����� � ���� � ������ Open Document Format

    //  TODO: ��� ���� ������ ���-�� ���������, ��������� ������� �������

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elStyles"></param>
    /// <param name="formatText"></param>
    /// <param name="styleName"></param>
    /// <returns></returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName)
    {
      return ODFAddFormat(elStyles, formatText, styleName, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elStyles"></param>
    /// <param name="formatText"></param>
    /// <param name="styleName"></param>
    /// <param name="ci"></param>
    /// <returns></returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName,
      CultureInfo ci)
    {
      string Language = ci.Name.Substring(0, 2);
      string Country = String.Empty;
      if (ci.Name.Length == 5)
        Country = ci.Name.Substring(3, 2);
      return ODFAddFormat(elStyles, formatText, styleName,
        ci.NumberFormat, ci.DateTimeFormat, Language, Country);
    }

    /// <summary>
    /// � Open Document Format, � ������� �� ������ Microsoft Office, ������� �����
    /// � ��� �������� �� � ���� ����� ������, ��������, "0.00", � � ���� ���������
    /// ��������� ������
    /// ���������� �� ������!
    /// </summary>
    /// <param name="elStyles">���� "office:automatic-styles" ��� ���������� ��������</param>
    /// <param name="formatText">�������� ������ �����</param>
    /// <param name="styleName">��� ������������ �����</param>
    /// <param name="NumberFormat"></param>
    /// <param name="DateTimeFormat"></param>
    /// <param name="Language"></param>
    /// <param name="Country"></param>
    /// <returns>true - ����� ��������. false - � ������� ���������� ������ ������
    /// �� �������������</returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName,
      NumberFormatInfo NumberFormat, DateTimeFormatInfo DateTimeFormat, string Language, string Country)
    {
      if (String.IsNullOrEmpty(formatText))
        return false;

      if (DataTools.IndexOfAny(formatText, "yMdhmsDtTfFgGRruUY") >= 0)
        return ODFAddDateTimeFormat(elStyles, formatText, styleName,
          CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat, "ru", "RU");

      if (formatText.IndexOf('0') >= 0)
        return ODFAddNumberFormat(elStyles, formatText, styleName/*, NumberFormat*/);

      return false;
    }

    private static bool ODFAddNumberFormat(XmlElement elStyles, string formatText, string styleName/*,
      NumberFormatInfo NumberFormat*/)
    {
      XmlElement elStyle = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);

      // 18.11.2016
      // ������ ��������, ��������� �� ������, ����������� ��������
      // ��������� ����� ������� ������������ ��������������� ������ ����� number:number-style
      // ��� ������ ������ ��������� ��������� ����� number:number-style � ���������� P0, P1, ... 
      // ������ �������� elStyles, � � �������� ����� number:number-style �������� ������ � ���������

      string[] a = formatText.Split(';');
      XmlElement elStyleP0, elStyleP1;
      switch (a.Length)
      { 
        case 1: // ������� ������, ��� ����
          DoWriteNumberFormat(elStyle, formatText);
          break;
        case 2: // ����� >=0 � < 0
          elStyleP0 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyle);
          SetAttr(elStyleP0, "style:name", styleName+"P0", nmspcStyle);
          DoWriteNumberFormat(elStyleP0, a[0]);
          DoWriteFormatRef(elStyle, "value()>=0", styleName + "P0");

          DoWriteNumberFormat(elStyle, a[1]);
          break;
        case 3: // ����� >0, <0 � =0
          elStyleP0 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyleP0);
          SetAttr(elStyleP0, "style:name", styleName + "P0", nmspcStyle);
          DoWriteNumberFormat(elStyleP0, a[0]);
          DoWriteFormatRef(elStyle, "value()>0", styleName + "P0");

          elStyleP1 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyleP1);
          SetAttr(elStyleP1, "style:name", styleName + "P1", nmspcStyle);
          DoWriteNumberFormat(elStyleP1, a[1]);
          DoWriteFormatRef(elStyle, "value()<0", styleName + "P1");

          DoWriteNumberFormat(elStyle, a[2]);
          break;

        default:
          throw new ArgumentException("�������� ������ \"" + formatText + "\" ������� ������, ��� �� ���� ������", "formatText");
      }


      return true;
    }

    private static void DoWriteFormatRef(XmlElement elStyle, string condition, string styleName)
    {
      XmlElement elMap = elStyle.OwnerDocument.CreateElement("style:map", nmspcStyle);
      elStyle.AppendChild(elMap);
      SetAttr(elMap, "style:condition", condition,nmspcStyle);
      SetAttr(elMap, "style:apply-style-name", styleName, nmspcStyle);
    }

    private static void DoWriteNumberFormat(XmlElement elStyle, string formatText)
    {
      if (String.IsNullOrEmpty(formatText))
      {
        DoWriteTextFormat(elStyle, String.Empty);
        return;
      }

      if (formatText[0]=='\"')
      {
        DoWriteTextFormat(elStyle, UnquoteText(formatText));
        return;
      }

      if (formatText[0] == '-')
      { 
        // ���� ����� �� �������� ������ �������, � �������� �������
        DoWriteTextFormat(elStyle, "-");
        formatText = formatText.Substring(1);
      }


      /*
       * � Open Document Format �� ������������� �������� �������������� ���� ����� �������.
       * ���� ������ ������ "0.0#", ��������� �������� ODS,  ������� � ������� ������, �� ������ ���������� �� "0.00"
       */

      // ���������� ������� ����������� ����� � ������� ������� #
      bool ThousandSep = false;
      for (int i = 0; i < formatText.Length; i++)
      {
        bool brk=false;
        switch (formatText[i])
        { 
          case '#':
            break;
          case ',':
            ThousandSep=true;
            break;
          default:
            formatText = formatText.Substring(i);
            brk = true;
            break;
        }
        if (brk)
          break;
      }

      int MinIntDigs;
      int Decimals;
      int p = formatText.IndexOf('.');
      if (p < 0)
      {
        Decimals = 0;
        MinIntDigs = formatText.Length;
      }
      else
      {
        Decimals = formatText.Length - p - 1;
        MinIntDigs = formatText.Length - Decimals - 1;
      }

      int MinDecimals = Decimals;
      for (int i = 0; i < Decimals; i++)
      {
        if (formatText[formatText.Length - i - 1] == '#')
          MinDecimals--;
        else
          break;
      }

      XmlElement elNumber = elStyle.OwnerDocument.CreateElement("number:number", nmspcNumber);
      elStyle.AppendChild(elNumber);
      SetAttr(elNumber, "number:decimal-places", Decimals.ToString(), nmspcNumber);
      SetAttr(elNumber, "number:min-integer-digits", MinIntDigs.ToString(), nmspcNumber);
      if (MinDecimals < Decimals)
      {
        SetAttr(elNumber, "loext:min-decimal-places", MinDecimals.ToString(), nmspcLoext);
        SetAttr(elNumber, "number:decimal-replacement", "", nmspcNumber);
      }

      if (ThousandSep) // �� ����, ���� ���������, ��� ������� ���� ����� �� "." � ��������� ����� "0#"
        SetAttr(elNumber, "number:grouping", "true", nmspcNumber);
    }

    private static string UnquoteText(string s)
    {
      if (s.Length < 2) // ����
        return string.Empty; 
      s = s.Substring(1, s.Length - 2);
      s = s.Replace("\"\"", "\"");

      // TODO: ������ ESC-��������
      return s;
    }

    private static void DoWriteTextFormat(XmlElement elStyle, string s)
    {
      XmlElement elText = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elText);
      if (!String.IsNullOrEmpty(s))
      {
        XmlText txtNode = elText.OwnerDocument.CreateTextNode(s);
        elText.AppendChild(txtNode);
      }
    }

    private static bool ODFAddDateTimeFormat(XmlElement elStyles, string formatText, string styleName,
      DateTimeFormatInfo FormatInfo, string Language, string Country)
    {
      // �������� ����������� �����
      switch (formatText)
      {
        case "d": formatText = FormatInfo.ShortDatePattern; break;
        case "D": formatText = FormatInfo.LongDatePattern; break;
        case "t": formatText = FormatInfo.ShortTimePattern; break;
        case "T": formatText = FormatInfo.LongTimePattern; break;
        case "f": formatText = FormatInfo.LongDatePattern + " " + FormatInfo.ShortTimePattern; break;
        case "F": formatText = FormatInfo.FullDateTimePattern; break;
        case "g": formatText = FormatInfo.ShortDatePattern + " " + FormatInfo.ShortTimePattern; break;
        case "G": formatText = FormatInfo.ShortDatePattern + " " + FormatInfo.LongTimePattern; break;
        case "M":
        case "m": formatText = FormatInfo.MonthDayPattern; break;
        case "R":
        case "r": formatText = FormatInfo.RFC1123Pattern; break;
        case "s": formatText = FormatInfo.SortableDateTimePattern; break;
        case "u": formatText = FormatInfo.UniversalSortableDateTimePattern; break;
        case "U": formatText = FormatInfo.FullDateTimePattern; break;
        case "Y":
        case "y": formatText = FormatInfo.YearMonthPattern; break;
      }


      XmlElement elStyle = elStyles.OwnerDocument.CreateElement(IsTimeOnlyFormat(formatText) ? "number:time-style" : "number:date-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);
      SetAttr(elStyle, "number:language", Language, nmspcNumber);
      SetAttr(elStyle, "number:country", Country, nmspcNumber);



      XmlElement elPart;
      string AllMaskChars = "yMdhms";

      // ���������� ������� � FormatText
      // ������������ for ��������, �.�. ���� ������� ����� �������
      int pos = 0;
      while (pos < formatText.Length)
      {
        if (AllMaskChars.IndexOf(formatText[pos]) >= 0)
        {
          // ���� �� �������� �����
          // ������� ��� ����� �� �������
          int cnt = 1;
          for (int j = pos + 1; j < formatText.Length; j++)
          {
            if (formatText[j] == formatText[pos])
              cnt++;
            else
              break;
          }

          switch (formatText[pos])
          {
            case 'y':
              elPart = elStyle.OwnerDocument.CreateElement("number:year", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 2)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;
            case 'M':
              switch (cnt)
              {
                case 1: // "1"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                case 2: // "01"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
                case 3: // "���"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:textual", "true", nmspcNumber);
                  break;
                case 4: // "������"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:textual", "true", nmspcNumber);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);  // � ��� ������� ?????
                  break;
              }
              break;
            case 'd':
              switch (cnt)
              {
                case 1: // "1"
                  elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                case 2: // "01"
                  elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
                case 3: // "��"
                  elPart = elStyle.OwnerDocument.CreateElement("day-of-week", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                //case 4: // "�������"
                default:
                  elPart = elStyle.OwnerDocument.CreateElement("day-of-week", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
              }
              break;
            case 'h':
              elPart = elStyle.OwnerDocument.CreateElement("number:hours", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);

              break;
            case 'm':
              elPart = elStyle.OwnerDocument.CreateElement("number:minutes", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);

              break;
            case 's':
              elPart = elStyle.OwnerDocument.CreateElement("number:seconds", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;
          }
          pos += cnt;
          continue;
        }

        string s;

        if (formatText[pos] == '\'')
        {
          // ���� ������ ��������
          int p = formatText.IndexOf('\'', pos + 1);
          if (p < 0) // ������ - ��� ������� ���������
            p = formatText.Length; // �������, ��� ������ ���� �� ����� �������

          s = formatText.Substring(pos + 1, p - pos - 1);

          elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
          elStyle.AppendChild(elPart);
          elPart.InnerText = s; // !!! �������������� ������������
          pos = p;
          continue;
        }

        // ������� ������� � ����������� �������
        // ���� �� ����� ���� ������ ������ ������� ������
        switch (formatText[pos])
        {
          case '/':
            s = FormatInfo.DateSeparator;
            break;
          case ':':
            s = FormatInfo.TimeSeparator;
            break;
          default:
            s = new string(formatText[pos], 1);
            break;
        }

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = s; // !!! �������������� ������������

        pos++;
      }

      return true;

#if XXX
      // ���� ������
      XmlElement elStyle = elStyles.OwnerDocument.CreateElement("number:date-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", StyleName, nmspcStyle);

      XmlElement elPart;
      elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elPart);
      elPart.InnerText = ".";

      elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elPart);
      elPart.InnerText = ".";

      elPart = elStyle.OwnerDocument.CreateElement("number:year", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      if (FormatText.IndexOf(':') >= 0)
      {
        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = " ";

        elPart = elStyle.OwnerDocument.CreateElement("number:hours", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = ":";

        elPart = elStyle.OwnerDocument.CreateElement("number:minutes", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = ":";

        elPart = elStyle.OwnerDocument.CreateElement("number:seconds", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);
      }

      return true;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FormatText"></param>
    /// <returns></returns>
    public static bool IsTimeOnlyFormat(string FormatText)
    {
      if (String.IsNullOrEmpty(FormatText))
        return false;
      switch (FormatText)
      {
        case "d":
        case "D":
        case "f":
        case "F":
        case "g":
        case "G":
        case "M":
        case "m":
        case "R":
        case "r":
        case "s":
        case "u":
        case "U":
        case "Y":
        case "y":
          return false;
        case "t":
        case "T":
          return true;
      }

      return DataTools.IndexOfAny(FormatText, "dMy") < 0;
    }

    #endregion

    #region ��������������� ������

    private static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute Attr;
      if (String.IsNullOrEmpty(nmspc))
        Attr = el.OwnerDocument.CreateAttribute(name);
      else
        Attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      Attr.Value = value;
      el.Attributes.Append(Attr);
    }

    #endregion

    #endregion
  }
}
