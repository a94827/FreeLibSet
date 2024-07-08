// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using FreeLibSet.Logging;
using FreeLibSet.IO;

namespace FreeLibSet.Diagnostics
{
  /// <summary>
  /// Наличие свободной оперативной памяти
  /// </summary>
  [Serializable]
  public enum AvailableMemoryState
  {
    /// <summary>
    /// Имеется достаточное количество свободной памяти. Файл подкачки не задействован
    /// </summary>
    Normal,

    /// <summary>
    /// Свободная физическая память закончилась. Используется файл подкачки.
    /// Желательно освободить память
    /// </summary>
    Swapping,

    /// <summary>
    /// Виртуальная память заканчивается.
    /// Велика вероятность получения OutOfMemoryException
    /// </summary>
    Low
  }

  /// <summary>
  /// Методы определения наличия свободной оперативной памяти
  /// </summary>
  public static class MemoryTools
  {
    #region CheckSufficientMemory

    private static bool _CheckSufficientMemoryExceptionLogged = false;

    /// <summary>
    /// Проверить возможность выделения блока памяти с помощью MemoryFailPoint
    /// </summary>
    /// <param name="sizeMB">Размер блока в мегабайтах</param>
    /// <returns>true - выделение блок возможно</returns>
    [DebuggerStepThrough]
    public static bool CheckSufficientMemory(int sizeMB)
    {
      try
      {
        using (/*MemoryFailPoint mfp = */new MemoryFailPoint(sizeMB))
        {
        }
        return true;
      }
      catch (InsufficientMemoryException)
      {
        return false;
      }
      catch /*(Exception e)*/
      {
        // 20.06.2017
        // В Mono MemoryFailPoint не работает и выбрасывает NotImplementedException
        // TODO: Надо проверять свободную память как-то по-другому
        if (!_CheckSufficientMemoryExceptionLogged)
        {
          _CheckSufficientMemoryExceptionLogged = true;
          // Надоело исключение!
          // LogoutTools.LogoutException(e, "MemoryTools.CheckSufficientMemory");
        }

        return true;
      }
    }

    #endregion

    #region P/Invoke

#pragma warning disable 649 // Предупреждение "Значение не присваивается полю"

    /// <summary>
    /// contains information about the current state of both physical and virtual memory, including extended memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
      #region Поля

      /// <summary>
      /// Size of the structure, in bytes. You must set this member before calling GlobalMemoryStatusEx. 
      /// </summary>
      public uint dwLength;

      /// <summary>
      /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use). 
      /// </summary>
      public uint dwMemoryLoad;

      /// <summary>
      /// Total size of physical memory, in bytes.
      /// </summary>
      public ulong ullTotalPhys;

      /// <summary>
      /// Size of physical memory available, in bytes. 
      /// </summary>
      public ulong ullAvailPhys;

      /// <summary>
      /// Size of the committed memory limit, in bytes. This is physical memory plus the size of the page file, minus a small overhead. 
      /// </summary>
      public ulong ullTotalPageFile;



      /// <summary>
      /// Size of available memory to commit, in bytes. The limit is ullTotalPageFile. 
      /// </summary>
      public ulong ullAvailPageFile;

      /// <summary>
      /// Total size of the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullTotalVirtual;

      /// <summary>
      /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullAvailVirtual;

      /// <summary>
      /// Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullAvailExtendedVirtual;

      #endregion

      #region Конструктор

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MEMORYSTATUSEX"/> class.
      /// </summary>
      public MEMORYSTATUSEX()
      {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
      }

      #endregion
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    /// <summary>
    /// The MEMORYSTATUS structure contains information about the current state of both physical and virtual memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUS
    {
      #region Поля

      /// <summary>
      /// Size of the MEMORYSTATUS data structure, in bytes. You do not need to set this member before calling the GlobalMemoryStatus function; the function sets it. 
      /// </summary>
      public uint dwLength;

      /// <summary>
      /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use). 
      /// Windows NT:  Percentage of approximately the last 1000 pages of physical memory that is in use.
      /// </summary>
      public uint dwMemoryLoad;

      /// <summary>
      /// Total size of physical memory, in bytes. 
      /// </summary>
      public UIntPtr dwTotalPhys;

      /// <summary>
      /// Size of physical memory available, in bytes
      /// </summary>
      public UIntPtr dwAvailPhys;

      /// <summary>
      /// Size of the committed memory limit, in bytes. 
      /// </summary>
      public UIntPtr dwTotalPageFile;

      /// <summary>
      /// Size of available memory to commit, in bytes. 
      /// </summary>
      public UIntPtr dwAvailPageFile;

      /// <summary>
      /// Total size of the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public UIntPtr dwTotalVirtual;

      /// <summary>
      /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public UIntPtr dwAvailVirtual;

      #endregion

      #region Конструктор

      public MEMORYSTATUS()
      {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUS));
      }

      #endregion
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalMemoryStatus([In, Out] MEMORYSTATUS lpBuffer);

#pragma warning restore 649

    #endregion

    #region MemoryLoad

    /// <summary>
    /// Процент использования оперативной памяти (0-100).
    /// Возвращает константу UnknownMemoryLoad, если для ОС нельзя получить это значение
    /// </summary>
    public static int MemoryLoad
    {
      get
      {
        try
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Win32NT:
              return GetMemoryLoadNT();
            case PlatformID.Win32Windows:
              return GetMemoryLoadWin();
            case PlatformID.Unix:
              return GetMemoryLoadUnix();
            default:
              return UnknownMemoryLoad;
          }
        }
        catch
        {
          return UnknownMemoryLoad;
        }
      }
    }

    private static int GetMemoryLoadNT()
    {
      MEMORYSTATUSEX ms = new MEMORYSTATUSEX();
      if (GlobalMemoryStatusEx(ms))
        return (int)(ms.dwMemoryLoad);
      else
        return UnknownMemoryLoad;
    }

    private static int GetMemoryLoadWin()
    {
      MEMORYSTATUS ms = new MEMORYSTATUS();
      if (GlobalMemoryStatus(ms))
        return (int)(ms.dwMemoryLoad);
      else
        return UnknownMemoryLoad;
    }

    private static int GetMemoryLoadUnix()
    {
      try
      {
        if (System.IO.File.Exists("/proc/meminfo"))
          return GetMemoryLoadFromFile_meminfo();
      }
      catch { }
      return UnknownMemoryLoad;
    }

    private static int GetMemoryLoadFromFile_meminfo()
    {
      // Используем отношение MemAvailable к MemTotal как процент свободной памяти.
      // Вообще не уверен, что это правильно.

      string[] a = System.IO.File.ReadAllLines("/proc/meminfo");
      long vTotal = -1L;
      long vAvail = -1L;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i].StartsWith("MemTotal:", StringComparison.Ordinal))
          vTotal = GetMemoryBytesFromUnixFileString(a[i]);
        else if (a[i].StartsWith("MemAvailable:", StringComparison.Ordinal))
          vAvail = GetMemoryBytesFromUnixFileString(a[i]);
        if (vTotal>=0 && vAvail>=0)
          break; // строки идут в начале файла
      }

      if (vTotal <= 0L || vAvail < 0L) // может быть 0
        return UnknownMemoryLoad;

      if (vAvail>vTotal) // ерунда какая-то
        return UnknownMemoryLoad;

      return 100 - (int)(vAvail * 100L / vTotal);
    }

    /// <summary>
    /// Это значение возвращается свойством MemoryLoad, когда нельзя получить процент использования памяти
    /// </summary>
    public const int UnknownMemoryLoad = -1;

    #endregion

    #region TotalPhysicalMemory

    /// <summary>
    /// Общий объем установленной физической памяти.
    /// Возвращает UnknownMemorySize, если определить объем невозможно
    /// </summary>
    public static long TotalPhysicalMemory
    {
      get
      {
        try
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Win32NT:
              return GetTotalPhysicalMemoryNT();
            case PlatformID.Win32Windows:
              return GetTotalPhysicalMemoryWin();
            case PlatformID.Unix:
              return GetTotalPhysicalMemoryUnix();
            default:
              return UnknownMemorySize;
          }
        }
        catch
        {
          return UnknownMemorySize;
        }
      }
    }

    private static long GetTotalPhysicalMemoryNT()
    {
      MEMORYSTATUSEX ms = new MEMORYSTATUSEX();
      if (GlobalMemoryStatusEx(ms))
        return (long)(ms.ullTotalPhys);
      else
        return UnknownMemorySize;
    }

    private static long GetTotalPhysicalMemoryWin()
    {
      MEMORYSTATUS ms = new MEMORYSTATUS();
      if (GlobalMemoryStatus(ms))
        return (long)(ms.dwTotalPhys);
      else
        return UnknownMemorySize;
    }

    private static long GetTotalPhysicalMemoryUnix()
    {
      try
      {
        if (System.IO.File.Exists("/proc/meminfo"))
          return GetTotalPhysicalMemoryFromFile_meminfo();
      }
      catch { }

      return UnknownMemorySize;
    }

    /// <summary>
    /// Извлечение из файла proc/meminfo строки "MemTotal:    1234567 kB"
    /// </summary>
    /// <returns></returns>
    private static long GetTotalPhysicalMemoryFromFile_meminfo()
    {
      string[] a = System.IO.File.ReadAllLines("/proc/meminfo");
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i].StartsWith("MemTotal:", StringComparison.Ordinal))
          return GetMemoryBytesFromUnixFileString(a[i]);
      }

      throw new InvalidOperationException("Не найдена строка MemTotal");
    }

    private static long GetMemoryBytesFromUnixFileString(string s)
    {
      int p = s.IndexOf(':');
      if (p < 0)
        throw new ArgumentException("Нет символа \":\"");
      s = s.Substring(p + 1).Trim();

      p = s.IndexOf(' ');
      if (p < 0)
        throw new InvalidOperationException("Нет единицы измерения");
      string s1 = s.Substring(0, p); // число
      string s2 = s.Substring(p + 1); // единица измерения

      long v = long.Parse(s1);
      switch (s2)
      {
        case "kB": v = v * FileTools.KByte; break;
        default: throw new InvalidOperationException("Неизвестная единица измерения: \"" + s2 + "\"");
      }

      return v;
    }

    /// <summary>
    /// Значение, возвращаемое свойством TotalPhysicalMemory, если объем памяти неизвестен
    /// </summary>
    public const long UnknownMemorySize = -1L;

    #endregion

    #region AvailableMemoryState

    /// <summary>
    /// Возвращает текущее состояние оперативной памяти
    /// </summary>
    public static AvailableMemoryState AvailableMemoryState
    {
      get
      {
        if (!CheckSufficientMemory(LowMemorySizeMB))
          return AvailableMemoryState.Low;
        if (MemoryLoad > 80)
          return AvailableMemoryState.Swapping;
        else
          return AvailableMemoryState.Normal;
      }
    }

    /// <summary>
    /// Объем свободной виртуальной памяти в МБ, который считается критическим.
    /// Если не удается выделить блок памяти такого размера, считается, что памяти мало.
    /// По умолчанию - 100МБ
    /// </summary>
    public static int LowMemorySizeMB
    {
      get { return _LowMemorySizeMB; }
      set { _LowMemorySizeMB = value; }
    }
    private static int _LowMemorySizeMB = 100;

    #endregion
  }
}
