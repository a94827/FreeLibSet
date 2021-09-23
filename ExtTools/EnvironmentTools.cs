using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using AgeyevAV.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;

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

namespace AgeyevAV
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
      FIsWine = GetIsWine();
      FIsMono = Type.GetType("Mono.Runtime") != null;

      FWinNTProductType = GetWinNTProductType();
      FOSVersionText = GetOSVersionText();
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
    /// Возвращает Assembly.GetEntryAssembly()
    /// Для доменов, отличных от домена приложения, пытается вернуть самую первую сборку
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
        Assembly FirstAsm = null;
        for (int i = 0; i < asms.Length; i++)
        {
          if (asms[i].GlobalAssemblyCache)
            continue;

          if (FirstAsm == null)
            FirstAsm = asms[i];

          string Name = asms[i].FullName;
          int p = Name.IndexOf(',');
          if (p >= 0)
            Name = Name.Substring(0, p);
          Name = Name.Trim();
          if (String.Equals(Name, AppName, StringComparison.OrdinalIgnoreCase))
            return asms[i];
        }

        return FirstAsm;
      }
    }

    #endregion

    #region .Net Framework / Mono

    /// <summary>
    /// Возвращает true, если выполнение осуществляется под MONO, а не .NET Framework
    /// </summary>
    public static bool IsMono { get { return FIsMono; } }
    private static bool FIsMono;

    /// <summary>
    /// Возвращает читаемое название версии .NET Framework с указанием разрядности
    /// </summary>
    public static string NetVersionText
    {
      get
      {
        return (IsMono ? "Mono" : ".NET Framework") + " " + Environment.Version.ToString() + (IntPtr.Size == 8 ? " (64 bit)" : " (32 bit)");
      }
    }


    #endregion

    #region OS

    /// <summary>
    /// Текстовое представление для версии ОС Environment.OSVersion, дополненное разрядностью (32 bit, 64 bit)
    /// </summary>
    public static string OSVersionText { get { return FOSVersionText; } }
    private static string FOSVersionText;

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
    /// Значение действительно только, если Systrem.Environment.OSVersion.Platform = Win32NT
    /// </summary>
    public static WinNTProductType WinNTProductType { get { return FWinNTProductType; } }
    private static WinNTProductType FWinNTProductType;

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
      bool AddSP, AddVer;
      string s = DoGetSpecialOSVersion(out AddSP, out AddVer);
      if (String.IsNullOrEmpty(s))
        return String.Empty; // 26.03.2018
      if (AddSP)
      {
        if (!String.IsNullOrEmpty(Environment.OSVersion.ServicePack))
          s += " " + Environment.OSVersion.ServicePack;
      }
      if (AddVer)
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
          bool IsServer = ((int)(WinNTProductType)) > 1;

          if (IsServer)
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
                    try
                    {
                      addSP = false;
                      return GetWindows10Version(out addVer); // 12.02.2018
                    }
                    catch { }
                    return "Windows 8 (?)";
                  case 3: return "Windows 8.1";
                }
                break;
              case 10: // Это никогда не будет вызвано, т.к. Windows 10 обманывает приложение
                switch (Environment.OSVersion.Version.Minor)
                {
                  case 0: return "Windows 10";
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
    /// Возвращает версию Windows-10, которая подделывается под Windows 8.1.
    /// Смотрим версию по файлу "user32.dll" 
    /// </summary>
    /// <returns></returns>
    private static string GetWindows10Version(out bool addVer)
    {
      string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "user32.dll");
      //if (!System.IO.File.Exists(Path))
      //{
      //  addVer = true;
      //  return "Windows 8 (?)";
      //}

      Version Ver = FileTools.GetFileVersion(Path); // 27.12.2020 Здесь есть проверка существования файла
      if (Ver == null)
      {
        addVer = true;
        return "Windows 8 (?)"; // 27.12.2020
      }

      string s = GetWindows10Version2(Ver);
      s += " (" + Ver.ToString() + ")";
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
      string s = "Windows " + ver.Major.ToString();
      if (ver.Minor > 0)
        s += "." + ver.Minor.ToString();
      return s;
    }


    /// <summary>
    /// Возвращает true, если работает 64-битная версия Widnows
    /// В Net Framework 4 есть аналогичное свойство в классе Environment
    /// </summary>
    public static bool Is64BitOperatingSystem
    {
      get
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
          if (Environment.OSVersion.Version.Major < 5)
            return false;

          string KeyName = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

          string s = Registry.GetValue(KeyName, "PROCESSOR_ARCHITECTURE", String.Empty).ToString();
          return s != "x86";
        }

        return false; // !!!
      }
    }


    #endregion

    #region Идентификатор сессии

    /// <summary>
    /// Идентификатор сессии, от которой запущен текущий процесс.
    /// Возвращает Process.SessionId
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
    /// Возвращается ActiveConsoleSessionId для неподходящей операционной системы
    /// </summary>
    public const int NoSessionId = -1;

    /// <summary>
    /// Идентификатор сессии, связанной с текущей консолью.
    /// Работает только для Windows XP (или Windows 2000?) и старше. Использует WTSGetActiveConsoleSessionId().
    /// Для остальных операционных систем возвращает NoSessionId
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
    public static bool IsWine { get { return FIsWine; } }
    private static bool FIsWine;

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
