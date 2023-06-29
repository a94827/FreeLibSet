// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using FreeLibSet.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;

namespace FreeLibSet.Core
{
  #region Перечисление WinNTProductType

  /// <summary>
  /// Значения для поля структуры OSVERSIONINFOEX.wProductType
  /// </summary>
  [Serializable]
  public enum WinNTProductType
  {
    /// <summary>
    /// Неизвестное значение
    /// </summary>
    Unknown = 0x0,

    /// <summary>
    /// Рабочая станция
    /// </summary>
    Workstation = 0x1,

    /// <summary>
    /// Контроллер домена
    /// </summary>
    DomainController = 0x2,

    /// <summary>
    /// Серверная ОС
    /// </summary>
    Server = 0x3
  }

  #endregion

  /// <summary>
  /// Расширенные статические свойства среды выполнения
  /// </summary>
  public static class EnvironmentTools
  {
    #region Статический конструктор

    static EnvironmentTools()
    {
      // 26.03.2018
      // Используем статический конструктор, чтобы поля инициализировались в правильном порядке,
      // а не как придется
      _Is64BitOperatingSystem = GetIs64BitOperatingSystem();
      _IsWine = GetIsWine();
      _MonoVersion = GetMonoVersion();

      _WinNTProductType = GetWinNTProductType();
      _OSVersionText = GetOSVersionText();
    }

    #endregion

    #region Приложение

    /// <summary>
    /// Возвращает имя запускаемой сборки без пути и расширения.
    /// Удаляется суффикс ".vshost", присваиваемый Visual Studio
    /// </summary>
    public static string ApplicationName
    {
      get
      {
        return FileTools.ApplicationPath.FileNameWithoutExtension;
      }
    }

    /// <summary>
    /// Возвращает <see cref="Assembly.GetEntryAssembly()"/>
    /// Для доменов, отличных от домена приложения, пытается вернуть самую первую сборку.
    /// </summary>
    public static Assembly EntryAssembly
    {
      get
      {
        Assembly asm = Assembly.GetEntryAssembly();
        if (asm != null)
          return asm;

        string AppName = ApplicationName;
        //if (String.IsNullOrEmpty(ApplicationName))
        //  return null;

        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        Assembly firstAsm = null;
        for (int i = 0; i < asms.Length; i++)
        {
          if (asms[i].GlobalAssemblyCache)
            continue;

          if (firstAsm == null)
            firstAsm = asms[i];

          string name = asms[i].FullName;
          int p = name.IndexOf(',');
          if (p >= 0)
            name = name.Substring(0, p);
          name = name.Trim();
          if (String.Equals(name, AppName, StringComparison.OrdinalIgnoreCase))
            return asms[i];
        }

        return firstAsm;
      }
    }

    #endregion

    #region .Net Framework / Mono

    /// <summary>
    /// Возвращает true, если выполнение осуществляется под MONO, а не .NET Framework
    /// </summary>
    public static bool IsMono { get { return _MonoVersion != null; } }

    /// <summary>
    /// Возвращает версию Mono.
    /// Эта версия не совпадает с <see cref="Environment.Version"/>.
    /// Если <see cref="IsMono"/>=false, возвращает версию "0.0.0.0"
    /// </summary>
    public static Version MonoVersion
    {
      get
      {
        if (_MonoVersion == null)
          return new Version();
        else
          return _MonoVersion;
      }
    }
    private static Version _MonoVersion;


    private static Version GetMonoVersion()
    {
      Type type = Type.GetType("Mono.Runtime");
      if (type == null)
        return null; // Net Framework

      try
      {
        MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
        if (displayName == null)
          return new Version();
        string s = (string)(displayName.Invoke(null, null).ToString());
        if (String.IsNullOrEmpty(s))
          return new Version();
        int p = s.IndexOf(' ');
        if (p >= 0)
          s = s.Substring(0, p);
        return new Version(s);
      }
      catch
      {
        return new Version();
      }
    }

    /// <summary>
    /// Возвращает читаемое название версии .NET Framework/Mono с указанием разрядности.
    /// Для моно возвращается версия mono и версия Common Language Runtime.
    /// </summary>
    public static string NetVersionText
    {
      get
      {
        if (IsMono)
          return "Mono " + MonoVersion.ToString() + ", CLR " + Environment.Version.ToString() + (IntPtr.Size == 8 ? " (64 bit)" : " (32 bit)");
        else
          return ".NET Framework " + Environment.Version.ToString() + (IntPtr.Size == 8 ? " (64 bit)" : " (32 bit)");
      }
    }

    #endregion

    #region OS

    /// <summary>
    /// Текстовое представление для версии ОС <see cref="Environment.OSVersion"/>, дополненное разрядностью (32 bit, 64 bit)
    /// </summary>
    public static string OSVersionText { get { return _OSVersionText; } }
    private static string _OSVersionText;

    private static string GetOSVersionText()
    {
      string s = GetSpecialOSVersion();
      if (String.IsNullOrEmpty(s))
        s = Environment.OSVersion.ToString();

      if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        s += EnvironmentTools.Is64BitOperatingSystem ? " (64 bit)" : " (32 bit)";

      if (IsWine) // 11.03.2017
        s = "Wine: " + s;

      return s;
    }

    #region GetVersionEx

    // Взято из исходников NetFramework

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OSVERSIONINFOEX
    {

      public OSVERSIONINFOEX()
      {
        OSVersionInfoSize = (int)Marshal.SizeOf(this);
      }

      // The OSVersionInfoSize field must be set to Marshal.SizeOf(this)
      internal int OSVersionInfoSize = 0;
      internal int MajorVersion = 0;
      internal int MinorVersion = 0;
      internal int BuildNumber = 0;
      internal int PlatformId = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string CSDVersion = null;
      internal ushort ServicePackMajor = 0;
      internal ushort ServicePackMinor = 0;
      internal short SuiteMask = 0;
      internal byte ProductType = 0;
      internal byte Reserved = 0;
    }

    [DllImport("KERNEL32", CharSet = CharSet.Auto)]
    internal static extern bool GetVersionEx([In, Out] OSVERSIONINFOEX lposvi);

    #endregion

    /// <summary>
    /// Возвращает уточненный тип операционной системы Windows NT.
    /// Значение действительно только, если <see cref="System.Environment.OSVersion"/>.Platform = Win32NT
    /// </summary>
    public static WinNTProductType WinNTProductType { get { return _WinNTProductType; } }
    private static WinNTProductType _WinNTProductType;

    private static WinNTProductType GetWinNTProductType()
    {
      try
      {
        if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
          OSVERSIONINFOEX osVer = new OSVERSIONINFOEX();
          if (GetVersionEx(osVer))
            return (WinNTProductType)(osVer.ProductType);
          else
            return WinNTProductType.Unknown;
        }
        else
          return WinNTProductType.Unknown;
      }
      catch
      {
        return WinNTProductType.Unknown;
      }
    }

    /// <summary>
    /// Получить читаемое название для некоторых версий Windows
    /// </summary>
    /// <returns></returns>
    private static string GetSpecialOSVersion()
    {
      bool addSP, addVer;
      string s = DoGetSpecialOSVersion(out addSP, out addVer);
      if (String.IsNullOrEmpty(s))
        return String.Empty; // 26.03.2018
      if (addSP)
      {
        if (!String.IsNullOrEmpty(Environment.OSVersion.ServicePack))
          s += " " + Environment.OSVersion.ServicePack;
      }
      if (addVer)
        s += " (" + Environment.OSVersion.Version.ToString() + ")";
      return s;
    }

    private static string DoGetSpecialOSVersion(out bool addSP, out bool addVer)
    {
      // Взято из:
      // https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions

      addSP = true; // надо ли будет добавить информацию о Service Pack?
      addVer = true; // надо ли будет добавить информацию о полной версии в скобках

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          bool isServer = ((int)(WinNTProductType)) > 1;

          if (isServer)
          {
            #region Серверные версии

            switch (Environment.OSVersion.Version.Major)
            {
              case 5:
                // Не уверен, что это будет работать
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows Server 2000";
                  case 2:
                    //if (SystemInformation(SM_SERVER2) != 0)
                    //  return "Windows Server 2003 R2";
                    //else 
                    return "Windows Server 2003";
                }
                break;
              case 6:
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows Server 2008";
                  case 1: return "Windows Server 2008 R2";
                  case 2: return "Windows Server 2012";
                  case 3: return "Windows Server 2012 R2";
                }
                break;
              case 10:
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows Server 2016";
                }
                break;
            }

            #endregion
          }
          else
          {
            #region Десктопные версии

            switch (Environment.OSVersion.Version.Major)
            {
              case 5:
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows 2000";
                  case 1: return "Windows XP";
                  case 2: return "Windows XP Professional X64"; // такой не видел
                }
                break;
              case 6:
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows Vista";
                  case 1: return "Windows 7";
                  case 2:
                    if (!IsWine) // 05.07.2022
                    {
                      try
                      {
                        addSP = false;
                        return GetWindows10Version(out addVer); // 12.02.2018
                      }
                      catch { }
                      return "Windows 8 (?)";
                    }
                    break;
                  case 3: return "Windows 8.1";
                }
                break;
              case 10: // Это никогда не будет вызвано, т.к. Windows 10 обманывает приложение
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0:
                    if (Environment.OSVersion.Version.Build >= 22000)
                      return "Windows 11"; // 05.07.2022
                    else
                      return "Windows 10";
                }
                break;
            }

            #endregion
          }
          break;

        case PlatformID.Win32Windows:
          switch (Environment.OSVersion.Version.Major)
          {
            case 4:
              switch (Environment.OSVersion.Version.Minor)
              {
                case 0: return "Windows 95";
                case 10: return "Windows 98"; // проверить
                case 90: return "Windows ME"; // проверить
              }
              break;
          }
          break;

      }

      return String.Empty;
    }

    /// <summary>
    /// Возвращает версию Windows-10/11, которая подделывается под Windows 8.1.
    /// Смотрим версию по файлу "user32.dll" 
    /// </summary>
    /// <returns></returns>
    private static string GetWindows10Version(out bool addVer)
    {
      AbsPath path = new AbsPath(new AbsPath(Environment.GetFolderPath(Environment.SpecialFolder.System)), "user32.dll");
      //if (!System.IO.File.Exists(Path))
      //{
      //  addVer = true;
      //  return "Windows 8 (?)";
      //}

      Version ver = FileTools.GetFileVersion(path); // 27.12.2020 Здесь есть проверка существования файла
      if (ver == null)
      {
        addVer = true;
        return "Windows 8 (?)"; // 27.12.2020
      }

      string s = GetWindows10Version2(ver);
      s += " (" + ver.ToString() + ")";
      addVer = false;
      return s;
    }

    private static string GetWindows10Version2(Version ver)
    {
      if (ver.Major == 6 && ver.Minor == 2)
        return "Windows 8";
      if (ver.Major == 6 && ver.Minor == 3)
        return "Windows 8.1";

      if (ver.Major < 6)
        return "Windows 8 (?)";

      // Дальше, наверное, версии будут правильными
      // 05.07.2022: Не будут. Windows 11 имеет версию 10
      string s;
      if (ver.Major == 10)
      {
        if (ver.Build >= 22000)
          s = "Windows 11"; // 05.07.2022
        else
          s = "Windows 10";
      }
      else
        s = "Windows " + ver.Major.ToString();
      if (ver.Minor > 0)
        s += "." + ver.Minor.ToString();
      return s;
    }


    /// <summary>
    /// Возвращает true, если работает 64-битная версия Widnows
    /// В Net Framework 4 есть аналогичное свойство в классе <see cref="Environment"/>.
    /// </summary>
    public static bool Is64BitOperatingSystem { get { return _Is64BitOperatingSystem; } }
    private static bool _Is64BitOperatingSystem;

    private static bool GetIs64BitOperatingSystem()
    {
      if (Environment.OSVersion.Platform == PlatformID.Win32NT)
      {
        if (Environment.OSVersion.Version.Major < 5)
          return false;

        string KeyName = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

        string s = Registry.GetValue(KeyName, "PROCESSOR_ARCHITECTURE", String.Empty).ToString();
        return s != "x86";
      }

      //return false;
      return IntPtr.Size == 8; // 28.06.2023
    }

    #endregion

    #region Идентификатор сессии

    /// <summary>
    /// Идентификатор сессии, от которой запущен текущий процесс.
    /// Возвращает <see cref="Process.SessionId"/>.
    /// </summary>
    public static int CurrentProcessSessionId
    {
      get
      {
        try
        {
          return Process.GetCurrentProcess().SessionId;
        }
        catch
        {
          return 0;
        }
      }
    }

    /// <summary>
    /// Отсутствие сессии (-1).
    /// Возвращается <see cref="ActiveConsoleSessionId"/> для неподходящей операционной системы
    /// </summary>
    public const int NoSessionId = -1;

    /// <summary>
    /// Идентификатор сессии, связанной с текущей консолью.
    /// Работает только для Windows XP (или Windows 2000?) и старше. Использует WTSGetActiveConsoleSessionId().
    /// Для остальных операционных систем возвращает <see cref="NoSessionId"/>.
    /// </summary>
    public static int ActiveConsoleSessionId
    {
      get
      {
        try
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Win32NT:
              if (Environment.OSVersion.Version.Major >= 5) // Windows 2000
                return WTSGetActiveConsoleSessionId();
              else
                return NoSessionId;
            default:
              return NoSessionId;
          }
        }
        catch
        {
          return NoSessionId;
        }
      }
    }

    /// <summary>
    /// The WTSGetActiveConsoleSessionId function retrieves the Remote Desktop Services session that
    /// is currently attached to the physical console. The physical console is the monitor, keyboard, and mouse.
    /// Note that it is not necessary that Remote Desktop Services be running for this function to succeed.
    /// </summary>
    /// <returns>The session identifier of the session that is attached to the physical console. If there is no
    /// session attached to the physical console, (for example, if the physical console session is in the process
    /// of being attached or detached), this function returns 0xFFFFFFFF.</returns>
    [DllImport("kernel32.dll")]
    private static extern Int32 WTSGetActiveConsoleSessionId();

    #endregion

    #region Wine

    /// <summary>
    /// Возвращает true, если приложение запущено под Wine (https://www.winehq.org)
    /// </summary>
    public static bool IsWine { get { return _IsWine; } }
    private static bool _IsWine;

    private static bool GetIsWine()
    {
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        return false;
      try
      {
        return DoGetWine();
      }
      catch
      {
        return false;
      }
    }

    #region P/Invoke

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    [ResourceExposure(ResourceScope.Process)]
    private static extern IntPtr GetModuleHandle(string modName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [ResourceExposure(ResourceScope.Machine)]
    private static extern IntPtr LoadLibrary(string libname);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [ResourceExposure(ResourceScope.None)]
    private static extern bool FreeLibrary(HandleRef hModule);

    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Ansi)]
    [ResourceExposure(ResourceScope.Process)]
    private static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);

    #endregion

    /// <summary>
    /// В этом методе используется P/Invoke.
    /// Способ определения наличия Wine взят из: https://www.winehq.org/pipermail/wine-devel/2008-September/069387.html
    /// (с изменениями)
    /// </summary>
    /// <returns></returns>
    private static bool DoGetWine()
    {
      IntPtr hModule = GetModuleHandle("ntdll.dll");
      if (hModule != IntPtr.Zero)
      {
        IntPtr pFunc = GetProcAddress(new HandleRef(null, hModule), "wine_get_version");
        return (pFunc != IntPtr.Zero);
      }
      else
      {
        // Загружаем библиотеку, если ее вдруг нет
        hModule = LoadLibrary("ntdll.dll");
        if (hModule != IntPtr.Zero)
        {
          try
          {
            IntPtr pFunc = GetProcAddress(new HandleRef(null, hModule), "wine_get_version");
            return (pFunc != IntPtr.Zero);
          }
          finally
          {
            FreeLibrary(new HandleRef(null, hModule));
          }
        }
      }
      return false;
    }

    #endregion
  }
}
